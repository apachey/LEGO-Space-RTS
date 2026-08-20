using Godot;
using LegoSpaceRTS.SimCore;
using CryptographicRandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace LegoSpaceRTS.Networking;

/// <summary>
/// T059 dedicated-server authority. It binds ENet peers to server-created
/// sessions, validates intent packets and advances only the 20 Hz SimCore.
/// Snapshot replication intentionally remains T060.
/// </summary>
public partial class M6DedicatedServerCommandHost : Node
{
    private const double TickSeconds = 0.05;
    private const int MaximumCatchUpTicksPerFrame = 8;
    private readonly SortedDictionary<int, ServerCommandSession> _sessions = new();
    private readonly ServerCommandAuthority _authority = new();
    private M6DedicatedServerTransportHost? _transport;
    private SimulationRunner? _runner;
    private double _accumulator;

    public event Action<int, byte>? SessionOpened;
    public event Action<int, NetworkCommandAcknowledgment>? CommandAcknowledged;

    public bool IsListening => _transport?.IsListening == true;
    public int ConnectedClientCount => _transport?.ConnectedClientCount ?? 0;
    public SimulationWorld World => _runner?.World ?? throw new InvalidOperationException("The dedicated command host is not configured.");
    public ulong GameplayContentHash { get; private set; }

    public Error Listen(M6TransportHostOptions options, SimulationWorld world, ulong gameplayContentHash)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (_transport is not null || _runner is not null) throw new InvalidOperationException("The dedicated command host is already configured.");
        if (world.PlayerCount < M6EnetPacketCarrier.MaximumClientConnections)
            throw new ArgumentException("The M6 1v1 authority requires two authoritative player slots.", nameof(world));

        _runner = new SimulationRunner(world);
        GameplayContentHash = gameplayContentHash;
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
        _accumulator = 0;
        if (transport is null) return;
        transport.ClientConnected -= OnClientConnected;
        transport.ClientDisconnected -= OnClientDisconnected;
        transport.PacketReceived -= OnPacketReceived;
        transport.Stop();
    }

    private void OnClientConnected(int peerId)
    {
        if (_transport is null || _runner is null) return;
        byte playerSlot = FindAvailablePlayerSlot();
        ulong token = CreateUniqueSessionToken();
        ServerCommandSession session = new(peerId, playerSlot, token);
        _sessions.Add(peerId, session);
        Error result = _transport.Send(peerId, M6TransportChannel.ReliableOrdered,
            NetworkCommandProtocol.EncodeWelcome(new NetworkCommandSessionWelcome(token, playerSlot)));
        if (result != Error.Ok)
        {
            _sessions.Remove(peerId);
            GD.PrintErr($"M6 command session welcome failed peer={peerId} error={result}");
            return;
        }
        GD.Print($"M6 command session opened peer={peerId} player={playerSlot}");
        SessionOpened?.Invoke(peerId, playerSlot);
    }

    private void OnClientDisconnected(int peerId)
    {
        if (_sessions.Remove(peerId)) GD.Print($"M6 command session closed peer={peerId}");
    }

    private void OnPacketReceived(M6TransportPacket packet)
    {
        if (_transport is null || _runner is null || !_sessions.TryGetValue(packet.PeerId, out ServerCommandSession? session)) return;
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

    private byte FindAvailablePlayerSlot()
    {
        int playerCount = _runner?.World.PlayerCount ?? 0;
        for (byte candidate = 0; candidate < playerCount; candidate++)
        {
            bool occupied = false;
            foreach (ServerCommandSession session in _sessions.Values)
                if (session.PlayerSlot == candidate) { occupied = true; break; }
            if (!occupied) return candidate;
        }
        throw new InvalidOperationException("No authoritative player slot is available for the connected peer.");
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
            if (!duplicate) return token;
        }
    }
}
