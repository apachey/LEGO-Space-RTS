using System.Net;
using System.Net.Sockets;
using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Networking;

/// <summary>Real ENet disconnect/rebind/full-legal-state/resume acceptance for T062.</summary>
public partial class M6ReconnectSmokeRunner : Node
{
    private sealed class SmokeClient
    {
        public readonly M6EnetPacketCarrier Carrier = new();
        public NetworkCommandSessionWelcome? Welcome;
        public NetworkReconnectState? ReconnectState;
        public readonly List<NetworkCommandAcknowledgment> Acknowledgments = new();
    }

    private readonly SmokeClient _first = new();
    private readonly SmokeClient _second = new();
    private readonly SmokeClient _replacement = new();
    private M6DedicatedServerCommandHost? _server;
    private SmokeClient? _originalPlayerZero;
    private SmokeClient? _otherPlayer;
    private EntityId _playerZeroEntity;
    private int _port;
    private int _sequenceOneTick;
    private int _sequenceTwoTick;
    private bool _sequenceOneSent;
    private bool _originalClosed;
    private bool _replacementStarted;
    private bool _sequenceTwoSent;
    private bool _finished;
    private ulong _deadlineMs;
    private NetworkMatchManifest _manifest;

    public override void _Ready()
    {
        ProcessPriority = 1000;
        _port = ReserveLoopbackUdpPort();
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        _playerZeroEntity = ScenarioFactory.OwnedIds(world, 0).Single();
        _manifest = new NetworkMatchManifest(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        _server = new M6DedicatedServerCommandHost { Name = "M6DedicatedServerCommandHost" };
        AddChild(_server);
        Error serverResult = _server.Listen(new M6TransportHostOptions(IPAddress.Loopback.ToString(), _port), world, world.Content.ContentHash);
        if (serverResult != Error.Ok) { Fail($"server create failed: {serverResult}"); return; }

        Configure(_first); Configure(_second); Configure(_replacement);
        if (_first.Carrier.StartClient(IPAddress.Loopback.ToString(), _port) != Error.Ok ||
            _second.Carrier.StartClient(IPAddress.Loopback.ToString(), _port) != Error.Ok)
        {
            Fail("initial clients failed to connect"); return;
        }
        _deadlineMs = Time.GetTicksMsec() + 15_000;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_finished || _server is null) return;
        PollIfOpen(_first); PollIfOpen(_second); PollIfOpen(_replacement);

        if (!_sequenceOneSent && _first.Welcome.HasValue && _second.Welcome.HasValue)
        {
            _originalPlayerZero = _first.Welcome.Value.PlayerSlot == 0 ? _first : _second;
            _otherPlayer = ReferenceEquals(_originalPlayerZero, _first) ? _second : _first;
            _sequenceOneSent = true;
            SendMove(_originalPlayerZero, 1, 82, 40);
        }

        if (!_originalClosed && _originalPlayerZero is not null)
        {
            NetworkCommandAcknowledgment accepted = _originalPlayerZero.Acknowledgments.FirstOrDefault(value => value.ClientSequence == 1 && value.Accepted);
            if (accepted.ClientSequence == 1 && _server.World.Tick.Value >= accepted.ExecutionTick.Value)
            {
                _sequenceOneTick = accepted.ExecutionTick.Value;
                _originalClosed = true;
                _originalPlayerZero.Carrier.Close();
            }
        }

        if (_originalClosed && !_replacementStarted && _server.ConnectedClientCount == 1)
        {
            _replacementStarted = true;
            Error result = _replacement.Carrier.StartClient(IPAddress.Loopback.ToString(), _port);
            if (result != Error.Ok) { Fail($"replacement connect failed: {result}"); return; }
        }

        if (_replacement.ReconnectState?.Accepted == true && !_sequenceTwoSent)
        {
            NetworkReconnectState state = _replacement.ReconnectState;
            if (state.LastProcessedCommandSequence != 1 || state.PlayerSlot != 0 || !state.Manifest.Matches(_manifest) ||
                !NetworkSnapshotProtocol.TryDecodeFrame(state.FullLegalSnapshotPacket, out NetworkSnapshotFrame full) || !full.IsFull)
            {
                Fail("reconnect state did not restore the legal full view and command sequence"); return;
            }
            _sequenceTwoSent = true;
            SendMove(_replacement, 2, 88, 40, state.SessionToken);
        }

        if (_sequenceTwoSent)
        {
            NetworkCommandAcknowledgment accepted = _replacement.Acknowledgments.FirstOrDefault(value => value.ClientSequence == 2 && value.Accepted);
            if (accepted.ClientSequence == 2 && _server.World.Tick.Value >= accepted.ExecutionTick.Value)
            {
                _sequenceTwoTick = accepted.ExecutionTick.Value;
                NavigationAgent navigation = _server.World.Entities.Navigation.Get(_playerZeroEntity);
                if (!navigation.HasTarget || FixVec2.Distance(navigation.Target, FixVec2.FromInts(88, 40)) >= Fix32.FromInt(2))
                {
                    Fail("post-reconnect command did not execute"); return;
                }
                _finished = true;
                GD.Print($"M6 RECONNECT SMOKE: PASS player=0 lastSequence=1 restoredTick={_sequenceOneTick} resumedTick={_sequenceTwoTick}");
                CloseAll(); GetTree().Quit(0); return;
            }
        }

        if (Time.GetTicksMsec() >= _deadlineMs) Fail($"timeout connected={_server.ConnectedClientCount} replacement={_replacement.ReconnectState?.Rejection}");
    }

    public override void _ExitTree() => CloseAll();

    private void Configure(SmokeClient client)
    {
        client.Carrier.PeerConnected += peerId =>
        {
            if (!ReferenceEquals(client, _replacement)) return;
            if (_originalPlayerZero?.Welcome is not NetworkCommandSessionWelcome welcome) { Fail("replacement has no retained welcome"); return; }
            Error result = client.Carrier.Send(peerId, M6TransportChannel.ReliableOrdered,
                NetworkReconnectProtocol.EncodeRequest(new NetworkReconnectRequest(welcome.SessionToken, welcome.PlayerSlot, _manifest)));
            if (result != Error.Ok) Fail($"reconnect request failed: {result}");
        };
        client.Carrier.PacketReceived += packet => OnPacket(client, packet);
        client.Carrier.CarrierError += Fail;
    }

    private void OnPacket(SmokeClient client, M6TransportPacket packet)
    {
        if (packet.Channel == M6TransportChannel.UnreliableSequenced && NetworkSnapshotProtocol.TryDecodeFrame(packet.Payload, out _)) return;
        if (packet.Channel == M6TransportChannel.ReliableBulk && NetworkReconnectProtocol.TryDecodeState(packet.Payload, out NetworkReconnectState reconnect))
        {
            client.ReconnectState = reconnect; return;
        }
        if (packet.Channel != M6TransportChannel.ReliableOrdered) { Fail("unexpected packet channel"); return; }
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

    private void SendMove(SmokeClient client, uint sequence, int x, int y, ulong? tokenOverride = null)
    {
        ulong token = tokenOverride ?? client.Welcome?.SessionToken ?? throw new InvalidOperationException("Client has no session token.");
        CommandEnvelope command = new(new SimTick(1), 0, sequence, SimCommandType.Move, new[] { _playerZeroEntity }, FixVec2.FromInts(x, y));
        Error result = client.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered,
            NetworkCommandProtocol.EncodeRequest(token, command));
        if (result != Error.Ok) Fail($"command {sequence} send failed: {result}");
    }

    private static void PollIfOpen(SmokeClient client)
    {
        if (client.Carrier.IsOpen) client.Carrier.Poll();
    }

    private void Fail(string reason)
    {
        if (_finished) return;
        _finished = true; GD.PrintErr($"M6 RECONNECT SMOKE: FAIL {reason}"); CloseAll(); GetTree().Quit(2);
    }

    private void CloseAll()
    {
        _server?.Stop(); _first.Carrier.Dispose(); _second.Carrier.Dispose(); _replacement.Carrier.Dispose(); _server = null;
    }

    private static int ReserveLoopbackUdpPort()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
