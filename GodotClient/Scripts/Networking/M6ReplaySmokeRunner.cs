using System.Net;
using System.Net.Sockets;
using Godot;
using LegoSpaceRTS.Client;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Networking;

/// <summary>Post-match server-log delivery and deterministic playback acceptance for T063.</summary>
public partial class M6ReplaySmokeRunner : Node
{
    private sealed class SmokeClient
    {
        public readonly M6EnetPacketCarrier Carrier = new();
        public readonly NetworkReplayAssembler ReplayAssembler = new();
        public readonly List<NetworkCommandAcknowledgment> Acknowledgments = new();
        public NetworkCommandSessionWelcome? Welcome;
    }

    private readonly SmokeClient _first = new();
    private readonly SmokeClient _second = new();
    private M6DedicatedServerCommandHost? _server;
    private SmokeClient? _playerZero;
    private EntityId _playerZeroEntity;
    private bool _commandSent;
    private bool _replayRequested;
    private bool _finished;
    private ulong _deadlineMs;
    private ulong _serverFinalHash;

    public override void _Ready()
    {
        ProcessPriority = 1000;
        int port = ReserveLoopbackUdpPort();
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        _playerZeroEntity = ScenarioFactory.OwnedIds(world, 0).Single();
        _server = new M6DedicatedServerCommandHost { Name = "M6DedicatedServerCommandHost" };
        AddChild(_server);
        Error serverResult = _server.Listen(new M6TransportHostOptions(IPAddress.Loopback.ToString(), port), world, world.Content.ContentHash);
        if (serverResult != Error.Ok) { Fail($"server create failed: {serverResult}"); return; }
        Configure(_first); Configure(_second);
        if (_first.Carrier.StartClient(IPAddress.Loopback.ToString(), port) != Error.Ok ||
            _second.Carrier.StartClient(IPAddress.Loopback.ToString(), port) != Error.Ok)
        {
            Fail("clients failed to connect"); return;
        }
        _deadlineMs = Time.GetTicksMsec() + 15_000;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_finished || _server is null) return;
        _first.Carrier.Poll();
        if (_finished) return;
        _second.Carrier.Poll();
        if (_finished) return;
        if (!_commandSent && _first.Welcome.HasValue && _second.Welcome.HasValue)
        {
            _playerZero = _first.Welcome.Value.PlayerSlot == 0 ? _first : _second;
            _commandSent = true;
            CommandEnvelope command = new(new SimTick(999), 0, 1, SimCommandType.Move, new[] { _playerZeroEntity }, FixVec2.FromInts(90, 40));
            Error result = _playerZero.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered,
                NetworkCommandProtocol.EncodeRequest(_playerZero.Welcome!.Value.SessionToken, command));
            if (result != Error.Ok) { Fail($"command send failed: {result}"); return; }
        }

        if (!_replayRequested && _playerZero is not null && _server.World.Tick.Value >= 25 &&
            _playerZero.Acknowledgments.Any(value => value.ClientSequence == 1 && value.Accepted))
        {
            _serverFinalHash = StateHasher.Hash(_server.World);
            _server.CompleteMatch();
            _replayRequested = true;
            Error result = _playerZero.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered,
                NetworkReplayProtocol.EncodeRequest(new NetworkReplayRequest(_playerZero.Welcome!.Value.SessionToken)));
            if (result != Error.Ok) { Fail($"replay request failed: {result}"); return; }
        }
        if (Time.GetTicksMsec() >= _deadlineMs) Fail($"timeout tick={_server.World.Tick.Value} replayRequested={_replayRequested}");
    }

    public override void _ExitTree() => CloseAll();

    private void Configure(SmokeClient client)
    {
        client.Carrier.PacketReceived += packet => OnPacket(client, packet);
        client.Carrier.CarrierError += Fail;
    }

    private void OnPacket(SmokeClient client, M6TransportPacket packet)
    {
        if (packet.Channel == M6TransportChannel.UnreliableSequenced && NetworkSnapshotProtocol.TryDecodeFrame(packet.Payload, out _)) return;
        if (packet.Channel == M6TransportChannel.ReliableBulk && NetworkReplayProtocol.TryDecodeChunk(packet.Payload, out NetworkReplayChunk chunk))
        {
            if (client.ReplayAssembler.TryAdd(chunk, out byte[] replay)) VerifyReplay(replay);
            return;
        }
        if (packet.Channel != M6TransportChannel.ReliableOrdered) { Fail("unexpected server packet channel"); return; }
        if (!client.Welcome.HasValue && NetworkCommandProtocol.TryDecodeWelcome(packet.Payload, out NetworkCommandSessionWelcome welcome))
        {
            client.Welcome = welcome; return;
        }
        if (NetworkCommandProtocol.TryDecodeAcknowledgment(packet.Payload, out NetworkCommandAcknowledgment acknowledgment))
        {
            client.Acknowledgments.Add(acknowledgment); return;
        }
        Fail("client could not decode server packet");
    }

    private void VerifyReplay(byte[] replayBytes)
    {
        if (_finished || _server is null) return;
        try
        {
            ReplayLog log = ReplayLog.Deserialize(replayBytes);
            SimulationRunner playback = log.CreateRunnerAtTick(log.FinalTick);
            ulong playbackHash = StateHasher.Hash(playback.World);
            if (log.Commands.Count != 1 || log.Commands[0].Sequence != 1 || log.FinalTick < 25 ||
                !log.HashCheckpoints.Any(value => value.Tick.Value == 20) || log.ExpectedFinalHash != _serverFinalHash || playbackHash != _serverFinalHash)
            {
                Fail("delivered server log did not reproduce the final authoritative state"); return;
            }
            _finished = true;
            GD.Print($"M6 NETWORK REPLAY SMOKE: PASS commands=1 finalTick={log.FinalTick} chunksBytes={replayBytes.Length} finalHash={playbackHash:X16}");
            Callable.From(() => Finish(0)).CallDeferred();
        }
        catch (Exception exception) { Fail($"replay playback failed: {exception.Message}"); }
    }

    private void Fail(string reason)
    {
        if (_finished) return;
        _finished = true; GD.PrintErr($"M6 NETWORK REPLAY SMOKE: FAIL {reason}"); Callable.From(() => Finish(2)).CallDeferred();
    }

    private void Finish(int exitCode) { CloseAll(); AutomatedSmokeExit.Finish(this, exitCode); }

    private void CloseAll()
    {
        _server?.Stop(); _first.Carrier.Dispose(); _second.Carrier.Dispose(); _server = null;
    }

    private static int ReserveLoopbackUdpPort()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
