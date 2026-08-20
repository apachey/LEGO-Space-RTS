using Godot;
using LegoSpaceRTS.SimCore;
using CryptographicRandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace LegoSpaceRTS.Networking;

/// <summary>
/// M6 dedicated-server authority. It binds ENet peers to server-created
/// sessions, validates intent packets, advances the 20 Hz SimCore and publishes
/// recipient-specific T060 snapshots at 10 Hz.
/// </summary>
public partial class M6DedicatedServerCommandHost : Node
{
    private const double TickSeconds = 0.05;
    private const int MaximumCatchUpTicksPerFrame = 8;
    private const int ReconnectRetentionTicks = 60 * SimClock.TicksPerSecond;
    private sealed class RetainedSession
    {
        public RetainedSession(ServerCommandSession commandSession, int disconnectedTick)
        {
            CommandSession = commandSession;
            DisconnectedTick = disconnectedTick;
        }
        public ServerCommandSession CommandSession { get; }
        public int DisconnectedTick { get; }
    }
    private readonly SortedDictionary<int, ServerCommandSession> _sessions = new();
    private readonly SortedDictionary<int, ServerSnapshotSession> _snapshotSessions = new();
    private readonly SortedDictionary<ulong, RetainedSession> _retainedSessions = new();
    private readonly SortedSet<int> _pendingReconnectPeers = new();
    private readonly ServerCommandAuthority _authority = new();
    private M6DedicatedServerTransportHost? _transport;
    private SimulationRunner? _runner;
    private double _accumulator;

    public event Action<int, byte>? SessionOpened;
    public event Action<int, NetworkCommandAcknowledgment>? CommandAcknowledged;
    public event Action<int, uint, int>? SnapshotSent;
    public event Action<int, byte, int>? ReconnectCompleted;

    public bool IsListening => _transport?.IsListening == true;
    public int ConnectedClientCount => _transport?.ConnectedClientCount ?? 0;
    public SimulationWorld World => _runner?.World ?? throw new InvalidOperationException("The dedicated command host is not configured.");
    public ulong GameplayContentHash { get; private set; }
    public ulong MapHash { get; private set; }
    public NetworkMatchManifest MatchManifest => new(GameplayContentHash, MapHash);

    public Error Listen(M6TransportHostOptions options, SimulationWorld world, ulong gameplayContentHash)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (_transport is not null || _runner is not null) throw new InvalidOperationException("The dedicated command host is already configured.");
        if (world.PlayerCount < M6EnetPacketCarrier.MaximumClientConnections)
            throw new ArgumentException("The M6 1v1 authority requires two authoritative player slots.", nameof(world));

        _runner = new SimulationRunner(world);
        GameplayContentHash = gameplayContentHash;
        MapHash = NetworkMatchManifest.ComputeMapHash(world.Map);
        M6DedicatedServerTransportHost transport = new() { Name = "M6DedicatedServerTransportHost" };
        transport.ClientConnected += OnClientConnected;
        transport.ClientDisconnected += OnClientDisconnected;
        transport.PacketReceived += OnPacketReceived;
        AddChild(transport);
        Error result = transport.Listen(options);
        if (result != Error.Ok)
        {
            transport.ClientConnected -= OnClientConnected;
            transport.ClientDisconnected -= OnClientDisconnected;
            transport.PacketReceived -= OnPacketReceived;
            RemoveChild(transport);
            transport.Dispose();
            _runner = null;
            GameplayContentHash = 0;
            MapHash = 0;
            return result;
        }

        _transport = transport;
        ProcessPriority = -900;
        GD.Print($"M6 command authority ready simHz={SimClock.TicksPerSecond} protocol={SnapshotSerializer.SimulationProtocolVersion} contentHash={gameplayContentHash:X16}");
        return Error.Ok;
    }

    public override void _Process(double delta)
    {
        if (_runner is null) return;
        _accumulator += delta;
        int steps = 0;
        while (_accumulator >= TickSeconds && steps < MaximumCatchUpTicksPerFrame)
        {
            _runner.StepOneTick();
            ExpireRetainedSessions();
            if ((_runner.World.Tick.Value & 1) == 0) PublishSnapshots();
            _accumulator -= TickSeconds;
            steps++;
        }
    }

    public override void _ExitTree() => Stop();

    public void Stop()
    {
        M6DedicatedServerTransportHost? transport = _transport;
        _transport = null;
        _runner = null;
        _sessions.Clear();
        _snapshotSessions.Clear();
        _retainedSessions.Clear();
        _pendingReconnectPeers.Clear();
        _accumulator = 0;
        GameplayContentHash = 0;
        MapHash = 0;
        if (transport is null) return;
        transport.ClientConnected -= OnClientConnected;
        transport.ClientDisconnected -= OnClientDisconnected;
        transport.PacketReceived -= OnPacketReceived;
        transport.Stop();
    }

    private void OnClientConnected(int peerId)
    {
        if (_transport is null || _runner is null) return;
        if (!TryFindAvailablePlayerSlot(out byte playerSlot))
        {
            _pendingReconnectPeers.Add(peerId);
            GD.Print($"M6 peer awaiting reconnect authentication peer={peerId}");
            return;
        }
        OpenNewSession(peerId, playerSlot);
    }

    private void OpenNewSession(int peerId, byte playerSlot)
    {
        if (_transport is null) return;
        ulong token = CreateUniqueSessionToken();
        ServerCommandSession session = new(peerId, playerSlot, token);
        _sessions.Add(peerId, session);
        _snapshotSessions.Add(peerId, new ServerSnapshotSession());
        Error result = _transport.Send(peerId, M6TransportChannel.ReliableOrdered,
            NetworkCommandProtocol.EncodeWelcome(new NetworkCommandSessionWelcome(token, playerSlot)));
        if (result != Error.Ok)
        {
            _sessions.Remove(peerId);
            _snapshotSessions.Remove(peerId);
            GD.PrintErr($"M6 command session welcome failed peer={peerId} error={result}");
            return;
        }
        GD.Print($"M6 command session opened peer={peerId} player={playerSlot}");
        SessionOpened?.Invoke(peerId, playerSlot);
    }

    private void OnClientDisconnected(int peerId)
    {
        _pendingReconnectPeers.Remove(peerId);
        _snapshotSessions.Remove(peerId);
        if (!_sessions.TryGetValue(peerId, out ServerCommandSession? session)) return;
        _sessions.Remove(peerId);
        _retainedSessions[session.SessionToken] = new RetainedSession(session, _runner?.World.Tick.Value ?? 0);
        GD.Print($"M6 command session retained for reconnect peer={peerId} player={session.PlayerSlot}");
    }

    private void OnPacketReceived(M6TransportPacket packet)
    {
        if (_transport is null || _runner is null) return;
        if (packet.Channel == M6TransportChannel.ReliableOrdered && packet.TransferMode == MultiplayerPeer.TransferModeEnum.Reliable &&
            NetworkReconnectProtocol.TryDecodeRequest(packet.Payload, out NetworkReconnectRequest reconnectRequest))
        {
            HandleReconnect(packet.PeerId, reconnectRequest);
            return;
        }
        if (!_sessions.TryGetValue(packet.PeerId, out ServerCommandSession? session)) return;
        if (packet.Channel == M6TransportChannel.ReliableOrdered && packet.TransferMode == MultiplayerPeer.TransferModeEnum.Reliable &&
            NetworkSnapshotProtocol.TryDecodeAcknowledgment(packet.Payload, out NetworkSnapshotAcknowledgment snapshotAck))
        {
            if (snapshotAck.SessionToken == session.SessionToken && _snapshotSessions.TryGetValue(packet.PeerId, out ServerSnapshotSession? snapshots))
                snapshots.TryAcknowledge(snapshotAck.SnapshotSequence);
            return;
        }
        NetworkCommandAcknowledgment acknowledgment;
        if (packet.Channel != M6TransportChannel.ReliableOrdered || packet.TransferMode != MultiplayerPeer.TransferModeEnum.Reliable)
            acknowledgment = new NetworkCommandAcknowledgment(0, NetworkCommandRejection.MalformedPacket, new SimTick(-1));
        else
            acknowledgment = _authority.Process(_runner.World, session, packet.Payload);

        Error result = _transport.Send(packet.PeerId, M6TransportChannel.ReliableOrdered,
            NetworkCommandProtocol.EncodeAcknowledgment(acknowledgment));
        if (result != Error.Ok) GD.PrintErr($"M6 command acknowledgment failed peer={packet.PeerId} error={result}");
        CommandAcknowledged?.Invoke(packet.PeerId, acknowledgment);
    }

    private void HandleReconnect(int peerId, NetworkReconnectRequest request)
    {
        if (_transport is null || _runner is null) return;
        NetworkReconnectRejection rejection = NetworkReconnectRejection.None;
        if (!request.Manifest.Matches(MatchManifest)) rejection = NetworkReconnectRejection.ManifestMismatch;
        else if (!_retainedSessions.TryGetValue(request.SessionToken, out RetainedSession? retained) ||
                 retained.CommandSession.PlayerSlot != request.PlayerSlot)
            rejection = NetworkReconnectRejection.UnknownOrExpiredSession;
        else
        {
            foreach (KeyValuePair<int, ServerCommandSession> active in _sessions)
                if (active.Key != peerId && active.Value.PlayerSlot == request.PlayerSlot)
                {
                    rejection = NetworkReconnectRejection.PlayerSlotUnavailable;
                    break;
                }
        }

        if (rejection != NetworkReconnectRejection.None)
        {
            SendReconnectState(peerId, NetworkReconnectProtocol.Rejected(rejection, MatchManifest));
            return;
        }

        RetainedSession restored = _retainedSessions[request.SessionToken];
        _retainedSessions.Remove(request.SessionToken);
        _pendingReconnectPeers.Remove(peerId);
        _sessions.Remove(peerId);
        _snapshotSessions.Remove(peerId);
        ServerCommandSession rebound = restored.CommandSession.Rebind(peerId);
        ServerSnapshotSession snapshots = new();
        _sessions.Add(peerId, rebound);
        _snapshotSessions.Add(peerId, snapshots);
        byte[] fullSnapshot = snapshots.CreatePacket(_runner.World, rebound.PlayerSlot);
        NetworkReconnectState state = NetworkReconnectProtocol.CaptureAccepted(_runner.World, rebound, MatchManifest, fullSnapshot);
        SendReconnectState(peerId, state);
        GD.Print($"M6 reconnect restored peer={peerId} player={rebound.PlayerSlot} tick={_runner.World.Tick.Value} lastCommand={rebound.LastProcessedSequence}");
        ReconnectCompleted?.Invoke(peerId, rebound.PlayerSlot, _runner.World.Tick.Value);
    }

    private void SendReconnectState(int peerId, NetworkReconnectState state)
    {
        if (_transport is null) return;
        Error result = _transport.Send(peerId, M6TransportChannel.ReliableBulk, NetworkReconnectProtocol.EncodeState(state));
        if (result != Error.Ok) GD.PrintErr($"M6 reconnect state send failed peer={peerId} error={result}");
    }

    private void PublishSnapshots()
    {
        if (_transport is null || _runner is null) return;
        foreach (KeyValuePair<int, ServerCommandSession> pair in _sessions)
        {
            if (!_snapshotSessions.TryGetValue(pair.Key, out ServerSnapshotSession? snapshots)) continue;
            byte[] payload = snapshots.CreatePacket(_runner.World, pair.Value.PlayerSlot);
            Error result = _transport.Send(pair.Key, M6TransportChannel.UnreliableSequenced, payload);
            if (result != Error.Ok)
            {
                GD.PrintErr($"M6 snapshot send failed peer={pair.Key} error={result}");
                continue;
            }
            if (NetworkSnapshotProtocol.TryDecodeFrame(payload, out NetworkSnapshotFrame frame))
                SnapshotSent?.Invoke(pair.Key, frame.Sequence, payload.Length);
        }
    }

    private bool TryFindAvailablePlayerSlot(out byte playerSlot)
    {
        int playerCount = _runner?.World.PlayerCount ?? 0;
        for (byte candidate = 0; candidate < playerCount; candidate++)
        {
            bool occupied = false;
            foreach (ServerCommandSession session in _sessions.Values)
                if (session.PlayerSlot == candidate) { occupied = true; break; }
            if (!occupied)
                foreach (RetainedSession retained in _retainedSessions.Values)
                    if (retained.CommandSession.PlayerSlot == candidate) { occupied = true; break; }
            if (!occupied) { playerSlot = candidate; return true; }
        }
        playerSlot = 0;
        return false;
    }

    private ulong CreateUniqueSessionToken()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        while (true)
        {
            CryptographicRandomNumberGenerator.Fill(bytes);
            ulong token = BitConverter.ToUInt64(bytes);
            if (token == 0) continue;
            bool duplicate = false;
            foreach (ServerCommandSession session in _sessions.Values)
                if (session.SessionToken == token) { duplicate = true; break; }
            if (!duplicate && _retainedSessions.ContainsKey(token)) duplicate = true;
            if (!duplicate) return token;
        }
    }

    private void ExpireRetainedSessions()
    {
        if (_runner is null || _retainedSessions.Count == 0) return;
        List<ulong> expired = new();
        foreach (KeyValuePair<ulong, RetainedSession> pair in _retainedSessions)
            if (_runner.World.Tick.Value - pair.Value.DisconnectedTick > ReconnectRetentionTicks) expired.Add(pair.Key);
        for (int i = 0; i < expired.Count; i++) _retainedSessions.Remove(expired[i]);
    }
}
