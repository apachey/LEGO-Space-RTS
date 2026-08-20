using Godot;

namespace LegoSpaceRTS.Networking;

public readonly struct M6TransportHostOptions
{
    public const string DefaultBindAddress = "127.0.0.1";
    public const int DefaultPort = 24567;

    public M6TransportHostOptions(string bindAddress, int port)
    {
        BindAddress = bindAddress;
        Port = port;
    }

    public string BindAddress { get; }
    public int Port { get; }

    public static M6TransportHostOptions Parse(string[] arguments)
    {
        string bindAddress = DefaultBindAddress;
        int port = DefaultPort;
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] == "--network-bind")
            {
                if (++i >= arguments.Length) throw new ArgumentException("--network-bind requires an IPv4 or IPv6 address.");
                bindAddress = arguments[i];
            }
            else if (arguments[i] == "--network-port")
            {
                if (++i >= arguments.Length || !int.TryParse(arguments[i], out port) || port is < 1 or > 65535)
                    throw new ArgumentException("--network-port requires an integer from 1 through 65535.");
            }
        }
        return new M6TransportHostOptions(bindAddress, port);
    }
}

/// <summary>
/// Headless-ready server-side carrier host. T058 intentionally stops before
/// command validation (T059) and snapshot replication (T060).
/// </summary>
public partial class M6DedicatedServerTransportHost : Node
{
    private M6EnetPacketCarrier? _carrier;

    public event Action<int>? ClientConnected;
    public event Action<int>? ClientDisconnected;
    public event Action<M6TransportPacket>? PacketReceived;

    public bool IsListening => _carrier?.IsOpen == true;
    public int ConnectedClientCount => _carrier?.ConnectedPeerCount ?? 0;
    public IReadOnlyCollection<int> ConnectedClientIds => _carrier?.ConnectedPeerIds ?? Array.Empty<int>();

    public Error Listen(M6TransportHostOptions options)
    {
        if (_carrier is not null) throw new InvalidOperationException("The dedicated server transport host is already configured.");

        M6EnetPacketCarrier carrier = new();
        carrier.PeerConnected += OnClientConnected;
        carrier.PeerDisconnected += OnClientDisconnected;
        carrier.PacketReceived += OnPacketReceived;
        carrier.CarrierError += OnCarrierError;
        Error result = carrier.StartServer(options.BindAddress, options.Port);
        if (result != Error.Ok)
        {
            carrier.Dispose();
            return result;
        }

        _carrier = carrier;
        ProcessPriority = -1000;
        GD.Print($"M6 dedicated transport listening udp://{options.BindAddress}:{options.Port} maxClients={M6EnetPacketCarrier.MaximumClientConnections}");
        return Error.Ok;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        _carrier?.Poll();
    }

    public Error Send(int targetPeerId, M6TransportChannel channel, byte[] payload) =>
        (_carrier ?? throw new InvalidOperationException("The dedicated server transport host is not listening."))
        .Send(targetPeerId, channel, payload);

    public override void _ExitTree() => Stop();

    public void Stop()
    {
        M6EnetPacketCarrier? carrier = _carrier;
        _carrier = null;
        if (carrier is null) return;
        carrier.PeerConnected -= OnClientConnected;
        carrier.PeerDisconnected -= OnClientDisconnected;
        carrier.PacketReceived -= OnPacketReceived;
        carrier.CarrierError -= OnCarrierError;
        carrier.Dispose();
    }

    private void OnClientConnected(int peerId)
    {
        GD.Print($"M6 dedicated transport client connected peer={peerId} count={ConnectedClientCount}");
        ClientConnected?.Invoke(peerId);
    }

    private void OnClientDisconnected(int peerId)
    {
        GD.Print($"M6 dedicated transport client disconnected peer={peerId} count={ConnectedClientCount}");
        ClientDisconnected?.Invoke(peerId);
    }

    private void OnPacketReceived(M6TransportPacket packet) => PacketReceived?.Invoke(packet);
    private static void OnCarrierError(string message) => GD.PrintErr(message);
}
