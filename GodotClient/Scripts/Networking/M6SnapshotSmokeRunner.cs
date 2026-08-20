using System.Net;
using System.Net.Sockets;
using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Networking;

/// <summary>Two-recipient 10 Hz acknowledged-delta and no-hidden-data acceptance for T060/T061.</summary>
public partial class M6SnapshotSmokeRunner : Node
{
    private const int EnetDefaultMtuBytes = 1392;
    private sealed class SmokeClient
    {
        public readonly M6EnetPacketCarrier Carrier = new();
        public readonly NetworkSnapshotClientBuffer Snapshots = new();
        public readonly List<NetworkSnapshotFrame> Frames = new();
        public NetworkCommandSessionWelcome? Welcome;
        public int LargestPacketBytes;
    }

    private readonly SmokeClient _first = new();
    private readonly SmokeClient _second = new();
    private M6DedicatedServerCommandHost? _server;
    private bool _finished;
    private ulong _deadlineMs;

    public override void _Ready()
    {
        ProcessPriority = 1000;
        int port = ReserveLoopbackUdpPort();
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
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
        _deadlineMs = Time.GetTicksMsec() + 10_000;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_finished || _server is null) return;
        _first.Carrier.Poll(); if (_finished) return;
        _second.Carrier.Poll(); if (_finished) return;
        if (HasAcceptance(_first) && HasAcceptance(_second))
        {
            _finished = true;
            int largest = Math.Max(_first.LargestPacketBytes, _second.LargestPacketBytes);
            GD.Print($"M6 SNAPSHOT FOG SMOKE: PASS clients=2 cadenceHz=10 acknowledgedDeltas=2 hiddenEntities=0 maxPacketBytes={largest}");
            Callable.From(() => Finish(0)).CallDeferred(); return;
        }
        if (Time.GetTicksMsec() >= _deadlineMs) Fail($"timeout firstFrames={_first.Frames.Count} secondFrames={_second.Frames.Count}");
    }

    public override void _ExitTree() => CloseAll();

    private void Configure(SmokeClient client)
    {
        client.Carrier.PacketReceived += packet => OnPacket(client, packet);
        client.Carrier.CarrierError += Fail;
    }

    private void OnPacket(SmokeClient client, M6TransportPacket packet)
    {
        if (packet.Channel == M6TransportChannel.ReliableOrdered && NetworkCommandProtocol.TryDecodeWelcome(packet.Payload, out NetworkCommandSessionWelcome welcome))
        {
            client.Welcome = welcome; return;
        }
        if (packet.Channel != M6TransportChannel.UnreliableSequenced || packet.TransferMode != MultiplayerPeer.TransferModeEnum.UnreliableOrdered ||
            !NetworkSnapshotProtocol.TryDecodeFrame(packet.Payload, out NetworkSnapshotFrame frame) || !client.Welcome.HasValue)
        {
            Fail("client received invalid snapshot packet"); return;
        }
        if (!client.Snapshots.TryApply(frame, out NetworkSnapshotState state)) { Fail("client could not apply snapshot baseline"); return; }
        if (state.PlayerSlot != client.Welcome.Value.PlayerSlot || state.ExploredBits.Length != NetworkSnapshotState.KnowledgeBitsetBytes ||
            state.VisibleBits.Length != NetworkSnapshotState.KnowledgeBitsetBytes ||
            state.Entities.Any(value => value.Decode().Owner != state.PlayerSlot && value.Decode().Owner != byte.MaxValue))
        {
            Fail("recipient snapshot disclosed hidden opponent state or invalid fog knowledge"); return;
        }
        client.Frames.Add(frame); client.LargestPacketBytes = Math.Max(client.LargestPacketBytes, packet.Payload.Length);
        if (packet.Payload.Length > EnetDefaultMtuBytes) { Fail($"unreliable snapshot exceeded ENet MTU: {packet.Payload.Length}"); return; }
        Error result = client.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered,
            NetworkSnapshotProtocol.EncodeAcknowledgment(new NetworkSnapshotAcknowledgment(client.Welcome.Value.SessionToken, frame.Sequence)));
        if (result != Error.Ok) Fail($"snapshot acknowledgment failed: {result}");
    }

    private static bool HasAcceptance(SmokeClient client)
    {
        if (client.Frames.Count < 3 || !client.Frames[0].IsFull) return false;
        int deltas = 0;
        for (int i = 1; i < client.Frames.Count; i++)
        {
            if (client.Frames[i].Tick.Value - client.Frames[i - 1].Tick.Value != 2) return false;
            if (!client.Frames[i].IsFull && client.Frames[i].BaselineSequence > 0) deltas++;
        }
        return deltas >= 2;
    }

    private void Fail(string reason)
    {
        if (_finished) return;
        _finished = true; GD.PrintErr($"M6 SNAPSHOT FOG SMOKE: FAIL {reason}"); Callable.From(() => Finish(2)).CallDeferred();
    }

    private void Finish(int exitCode) { CloseAll(); GetTree().Quit(exitCode); }

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
