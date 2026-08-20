using Godot;

namespace LegoSpaceRTS.Networking;

public enum M6TransportChannel : byte
{
    ReliableOrdered = 0,
    UnreliableSequenced = 1,
    ReliableBulk = 2,
}

public readonly struct M6TransportPacket
{
    public M6TransportPacket(int peerId, M6TransportChannel channel, MultiplayerPeer.TransferModeEnum transferMode, byte[] payload)
    {
        PeerId = peerId;
        Channel = channel;
        TransferMode = transferMode;
        Payload = payload;
    }

    public int PeerId { get; }
    public M6TransportChannel Channel { get; }
    public MultiplayerPeer.TransferModeEnum TransferMode { get; }
    public byte[] Payload { get; }
}

/// <summary>
/// Godot/ENet is only the packet carrier. Gameplay packet formats, validation,
/// ordering policy and state replication remain project-owned M6 layers.
/// </summary>
public sealed class M6EnetPacketCarrier : IDisposable
{
    public const int MaximumClientConnections = 2;
    public const int LogicalChannelCount = 3;
    public const int MaximumPacketBytes = 64 * 1024;

    private readonly SortedSet<int> _connectedPeers = new();
    private ENetMultiplayerPeer? _peer;
    private bool _isServer;

    public event Action<int>? PeerConnected;
    public event Action<int>? PeerDisconnected;
    public event Action<M6TransportPacket>? PacketReceived;
    public event Action<string>? CarrierError;

    public bool IsOpen => _peer is not null;
    public bool IsServer => IsOpen && _isServer;
    public int LocalPeerId => _peer?.GetUniqueId() ?? 0;
    public int ConnectedPeerCount => _connectedPeers.Count;
    public IReadOnlyCollection<int> ConnectedPeerIds => _connectedPeers;
    public MultiplayerPeer.ConnectionStatus Status => _peer?.GetConnectionStatus() ?? MultiplayerPeer.ConnectionStatus.Disconnected;

    public Error StartServer(string bindAddress, int port)
    {
        ValidateEndpoint(bindAddress, port);
        EnsureClosed();

        ENetMultiplayerPeer peer = CreatePeer();
        peer.SetBindIP(bindAddress);
        Error result = peer.CreateServer(port, MaximumClientConnections, LogicalChannelCount);
        return CompleteStart(peer, result, isServer: true);
    }

    public Error StartClient(string address, int port)
    {
        ValidateEndpoint(address, port);
        EnsureClosed();

        ENetMultiplayerPeer peer = CreatePeer();
        Error result = peer.CreateClient(address, port, LogicalChannelCount);
        return CompleteStart(peer, result, isServer: false);
    }

    public void Poll()
    {
        ENetMultiplayerPeer peer = RequirePeer();
        peer.Poll();
        while (peer.GetAvailablePacketCount() > 0)
        {
            int sourcePeer = peer.GetPacketPeer();
            int rawChannel = peer.GetPacketChannel();
            MultiplayerPeer.TransferModeEnum transferMode = peer.GetPacketMode();
            byte[] payload = peer.GetPacket();
            Error packetError = peer.GetPacketError();
            if (packetError != Error.Ok)
            {
                CarrierError?.Invoke($"ENet packet receive failed: {packetError}.");
                continue;
            }
            if (payload.Length > MaximumPacketBytes)
            {
                CarrierError?.Invoke($"ENet packet from peer {sourcePeer} exceeded the {MaximumPacketBytes}-byte carrier limit.");
                continue;
            }
            if (!TryMapChannel(rawChannel, out M6TransportChannel channel))
            {
                CarrierError?.Invoke($"ENet packet from peer {sourcePeer} used unsupported channel {rawChannel}.");
                continue;
            }
            PacketReceived?.Invoke(new M6TransportPacket(sourcePeer, channel, transferMode, payload));
        }
    }

    public Error Send(int targetPeerId, M6TransportChannel channel, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.Length > MaximumPacketBytes)
            throw new ArgumentOutOfRangeException(nameof(payload), $"Carrier packets may not exceed {MaximumPacketBytes} bytes.");
        if (targetPeerId == 0)
            throw new ArgumentOutOfRangeException(nameof(targetPeerId), "A packet must target one peer; transport broadcast is not part of T058.");

        ENetMultiplayerPeer peer = RequirePeer();
        peer.SetTargetPeer(targetPeerId);
        peer.TransferChannel = (int)channel;
        peer.TransferMode = TransferModeFor(channel);
        return peer.PutPacket(payload);
    }

    public void Close()
    {
        ENetMultiplayerPeer? peer = _peer;
        _peer = null;
        _isServer = false;
        _connectedPeers.Clear();
        if (peer is null) return;

        peer.PeerConnected -= OnPeerConnected;
        peer.PeerDisconnected -= OnPeerDisconnected;
        peer.Close();
        peer.Dispose();
    }

    public void Dispose() => Close();

    public static MultiplayerPeer.TransferModeEnum TransferModeFor(M6TransportChannel channel) => channel switch
    {
        M6TransportChannel.ReliableOrdered => MultiplayerPeer.TransferModeEnum.Reliable,
        M6TransportChannel.UnreliableSequenced => MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
        M6TransportChannel.ReliableBulk => MultiplayerPeer.TransferModeEnum.Reliable,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown M6 transport channel."),
    };

    private ENetMultiplayerPeer CreatePeer()
    {
        ENetMultiplayerPeer peer = new();
        peer.PeerConnected += OnPeerConnected;
        peer.PeerDisconnected += OnPeerDisconnected;
        return peer;
    }

    private Error CompleteStart(ENetMultiplayerPeer peer, Error result, bool isServer)
    {
        if (result != Error.Ok)
        {
            peer.PeerConnected -= OnPeerConnected;
            peer.PeerDisconnected -= OnPeerDisconnected;
            peer.Close();
            peer.Dispose();
            return result;
        }

        _peer = peer;
        _isServer = isServer;
        return Error.Ok;
    }

    private void OnPeerConnected(long peerId)
    {
        int id = checked((int)peerId);
        _connectedPeers.Add(id);
        if (_isServer && _connectedPeers.Count >= MaximumClientConnections && _peer is not null)
            _peer.RefuseNewConnections = true;
        PeerConnected?.Invoke(id);
    }

    private void OnPeerDisconnected(long peerId)
    {
        int id = checked((int)peerId);
        _connectedPeers.Remove(id);
        if (_isServer && _connectedPeers.Count < MaximumClientConnections && _peer is not null)
            _peer.RefuseNewConnections = false;
        PeerDisconnected?.Invoke(id);
    }

    private static bool TryMapChannel(int rawChannel, out M6TransportChannel channel)
    {
        if (rawChannel >= 0 && rawChannel < LogicalChannelCount)
        {
            channel = (M6TransportChannel)rawChannel;
            return true;
        }
        channel = default;
        return false;
    }

    private static void ValidateEndpoint(string address, int port)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("A bind/connect address is required.", nameof(address));
        if (port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(port), "The ENet port must be between 1 and 65535.");
    }

    private void EnsureClosed()
    {
        if (_peer is not null) throw new InvalidOperationException("The ENet carrier is already open.");
    }

    private ENetMultiplayerPeer RequirePeer() => _peer ?? throw new InvalidOperationException("The ENet carrier is not open.");
}
