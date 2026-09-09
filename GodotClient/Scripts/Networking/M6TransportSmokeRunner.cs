using System.Net;
using System.Net.Sockets;
using System.Text;
using Godot;
using LegoSpaceRTS.Client;

namespace LegoSpaceRTS.Networking;

public partial class M6TransportSmokeRunner : Node
{
    private static readonly byte[] ClientOneMessage = Encoding.ASCII.GetBytes("m6-client-one");
    private static readonly byte[] ClientTwoMessage = Encoding.ASCII.GetBytes("m6-client-two");
    private static readonly byte[] ClientOneReply = Encoding.ASCII.GetBytes("m6-server-unreliable-sequenced");
    private static readonly byte[] ClientTwoReply = Encoding.ASCII.GetBytes("m6-server-reliable-bulk");

    private readonly HashSet<int> _serverPacketPeers = new();
    private M6DedicatedServerTransportHost? _server;
    private M6EnetPacketCarrier? _clientOne;
    private M6EnetPacketCarrier? _clientTwo;
    private ulong _deadlineMs;
    private bool _requestsSent;
    private bool _clientOneReplyReceived;
    private bool _clientTwoReplyReceived;
    private bool _finished;

    public override void _Ready()
    {
        ProcessPriority = 1000;
        int port = ReserveLoopbackUdpPort();

        _server = new M6DedicatedServerTransportHost { Name = "M6DedicatedServerTransportHost" };
        _server.PacketReceived += OnServerPacketReceived;
        AddChild(_server);
        Error serverResult = _server.Listen(new M6TransportHostOptions(IPAddress.Loopback.ToString(), port));
        if (serverResult != Error.Ok)
        {
            Fail($"server create failed: {serverResult}");
            return;
        }

        _clientOne = CreateClient(OnClientOnePacket);
        Error clientOneResult = _clientOne.StartClient(IPAddress.Loopback.ToString(), port);
        if (clientOneResult != Error.Ok)
        {
            Fail($"client one create failed: {clientOneResult}");
            return;
        }

        _clientTwo = CreateClient(OnClientTwoPacket);
        Error clientTwoResult = _clientTwo.StartClient(IPAddress.Loopback.ToString(), port);
        if (clientTwoResult != Error.Ok)
        {
            Fail($"client two create failed: {clientTwoResult}");
            return;
        }

        _deadlineMs = Time.GetTicksMsec() + 10_000;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_finished || _clientOne is null || _clientTwo is null || _server is null) return;

        _clientOne.Poll();
        _clientTwo.Poll();

        if (!_requestsSent && _server.ConnectedClientCount == M6EnetPacketCarrier.MaximumClientConnections &&
            _clientOne.Status == MultiplayerPeer.ConnectionStatus.Connected &&
            _clientTwo.Status == MultiplayerPeer.ConnectionStatus.Connected)
        {
            if (_clientOne.LocalPeerId <= MultiplayerPeer.TargetPeerServer || _clientTwo.LocalPeerId <= MultiplayerPeer.TargetPeerServer ||
                _clientOne.LocalPeerId == _clientTwo.LocalPeerId)
            {
                Fail("ENet did not assign two distinct client peer IDs.");
                return;
            }

            _requestsSent = true;
            RequireOk(_clientOne.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered, ClientOneMessage), "client one send");
            RequireOk(_clientTwo.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered, ClientTwoMessage), "client two send");
        }

        if (_requestsSent && _serverPacketPeers.Count == M6EnetPacketCarrier.MaximumClientConnections &&
            _clientOneReplyReceived && _clientTwoReplyReceived)
        {
            _finished = true;
            GD.Print($"M6 TRANSPORT SMOKE: PASS serverConnections={_server.ConnectedClientCount} clientOne={_clientOne.LocalPeerId} clientTwo={_clientTwo.LocalPeerId} channels=3");
            CloseAll();
            AutomatedSmokeExit.Finish(this, 0);
            return;
        }

        if (Time.GetTicksMsec() >= _deadlineMs)
            Fail($"timeout connections={_server.ConnectedClientCount} serverPackets={_serverPacketPeers.Count} replies={_clientOneReplyReceived}/{_clientTwoReplyReceived}");
    }

    public override void _ExitTree() => CloseAll();

    private void OnServerPacketReceived(M6TransportPacket packet)
    {
        if (_server is null || packet.Channel != M6TransportChannel.ReliableOrdered ||
            packet.TransferMode != MultiplayerPeer.TransferModeEnum.Reliable)
        {
            Fail($"server received unexpected channel metadata from peer {packet.PeerId}.");
            return;
        }

        if (packet.Payload.AsSpan().SequenceEqual(ClientOneMessage))
            RequireOk(_server.Send(packet.PeerId, M6TransportChannel.UnreliableSequenced, ClientOneReply), "server reply to client one");
        else if (packet.Payload.AsSpan().SequenceEqual(ClientTwoMessage))
            RequireOk(_server.Send(packet.PeerId, M6TransportChannel.ReliableBulk, ClientTwoReply), "server reply to client two");
        else
        {
            Fail($"server received an unknown T058 smoke payload from peer {packet.PeerId}.");
            return;
        }
        _serverPacketPeers.Add(packet.PeerId);
    }

    private void OnClientOnePacket(M6TransportPacket packet)
    {
        if (packet.PeerId != MultiplayerPeer.TargetPeerServer || packet.Channel != M6TransportChannel.UnreliableSequenced ||
            packet.TransferMode != MultiplayerPeer.TransferModeEnum.UnreliableOrdered ||
            !packet.Payload.AsSpan().SequenceEqual(ClientOneReply))
        {
            Fail("client one received incorrect peer, channel, mode or payload metadata.");
            return;
        }
        _clientOneReplyReceived = true;
    }

    private void OnClientTwoPacket(M6TransportPacket packet)
    {
        if (packet.PeerId != MultiplayerPeer.TargetPeerServer || packet.Channel != M6TransportChannel.ReliableBulk ||
            packet.TransferMode != MultiplayerPeer.TransferModeEnum.Reliable ||
            !packet.Payload.AsSpan().SequenceEqual(ClientTwoReply))
        {
            Fail("client two received incorrect peer, channel, mode or payload metadata.");
            return;
        }
        _clientTwoReplyReceived = true;
    }

    private M6EnetPacketCarrier CreateClient(Action<M6TransportPacket> packetHandler)
    {
        M6EnetPacketCarrier client = new();
        client.PacketReceived += packetHandler;
        client.CarrierError += Fail;
        return client;
    }

    private void RequireOk(Error result, string operation)
    {
        if (result != Error.Ok) Fail($"{operation} failed: {result}");
    }

    private void Fail(string reason)
    {
        if (_finished) return;
        _finished = true;
        GD.PrintErr($"M6 TRANSPORT SMOKE: FAIL {reason}");
        CloseAll();
        AutomatedSmokeExit.Finish(this, 2);
    }

    private void CloseAll()
    {
        if (_server is not null) _server.PacketReceived -= OnServerPacketReceived;
        _server?.Stop();
        _clientOne?.Dispose();
        _clientTwo?.Dispose();
        _server = null;
        _clientOne = null;
        _clientTwo = null;
    }

    private static int ReserveLoopbackUdpPort()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
