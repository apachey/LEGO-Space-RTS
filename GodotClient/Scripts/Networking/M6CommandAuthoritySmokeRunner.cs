using System.Net;
using System.Net.Sockets;
using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Networking;

/// <summary>Loopback acceptance for the real T058 carrier plus T059 authority path.</summary>
public partial class M6CommandAuthoritySmokeRunner : Node
{
    private sealed class SmokeClient
    {
        public readonly M6EnetPacketCarrier Carrier = new();
        public readonly List<NetworkCommandAcknowledgment> Acknowledgments = new();
        public NetworkCommandSessionWelcome? Welcome;
    }

    private readonly SmokeClient _clientOne = new();
    private readonly SmokeClient _clientTwo = new();
    private M6DedicatedServerCommandHost? _server;
    private EntityId _playerZeroEntity;
    private EntityId _playerOneEntity;
    private bool _requestsSent;
    private bool _finished;
    private ulong _deadlineMs;
    private int _latestAcceptedTick;

    public override void _Ready()
    {
        ProcessPriority = 1000;
        int port = ReserveLoopbackUdpPort();
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        _playerZeroEntity = ScenarioFactory.OwnedIds(world, 0).Single();
        _playerOneEntity = ScenarioFactory.OwnedIds(world, 1).Single();

        _server = new M6DedicatedServerCommandHost { Name = "M6DedicatedServerCommandHost" };
        AddChild(_server);
        Error serverResult = _server.Listen(new M6TransportHostOptions(IPAddress.Loopback.ToString(), port), world, world.Content.ContentHash);
        if (serverResult != Error.Ok)
        {
            Fail($"server create failed: {serverResult}");
            return;
        }

        ConfigureClient(_clientOne);
        ConfigureClient(_clientTwo);
        Error firstResult = _clientOne.Carrier.StartClient(IPAddress.Loopback.ToString(), port);
        if (firstResult != Error.Ok) { Fail($"client one create failed: {firstResult}"); return; }
        Error secondResult = _clientTwo.Carrier.StartClient(IPAddress.Loopback.ToString(), port);
        if (secondResult != Error.Ok) { Fail($"client two create failed: {secondResult}"); return; }
        _deadlineMs = Time.GetTicksMsec() + 10_000;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_finished || _server is null) return;
        _clientOne.Carrier.Poll();
        _clientTwo.Carrier.Poll();

        if (!_requestsSent && _clientOne.Welcome.HasValue && _clientTwo.Welcome.HasValue)
        {
            SmokeClient playerZero = ClientForPlayer(0);
            SmokeClient playerOne = ClientForPlayer(1);
            _requestsSent = true;

            Send(playerZero, Move(0, 1, _playerOneEntity, 80, 40));
            Send(playerZero, Move(0, 2, _playerZeroEntity, 80, 40));
            Send(playerZero, Move(0, 2, _playerZeroEntity, 80, 40));
            Send(playerZero, new CommandEnvelope(new SimTick(1), 0, 3, SimCommandType.DebugDrainEnergy, Array.Empty<EntityId>(), FixVec2.Zero));

            Send(playerOne, Move(0, 1, _playerOneEntity, 30, 105));
            Send(playerOne, Move(1, 1, _playerOneEntity, 30, 105));
            RequireOk(playerOne.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered, new byte[] { 1, 2, 3 }), "malformed request send");
            Send(playerOne, Move(1, 2, _playerOneEntity, 34, 105), tokenOverride: playerOne.Welcome!.Value.SessionToken ^ 1UL);
            Send(playerOne, Move(1, 2, _playerOneEntity, 34, 105));
        }

        if (_requestsSent && HasExpectedAcknowledgments() && _server.World.Tick.Value >= _latestAcceptedTick)
        {
            NavigationAgent playerZeroNavigation = _server.World.Entities.Navigation.Get(_playerZeroEntity);
            NavigationAgent playerOneNavigation = _server.World.Entities.Navigation.Get(_playerOneEntity);
            bool playerZeroExecuted = playerZeroNavigation.HasTarget && FixVec2.Distance(playerZeroNavigation.Target, FixVec2.FromInts(80, 40)) < Fix32.FromInt(2);
            bool playerOneExecuted = playerOneNavigation.HasTarget && FixVec2.Distance(playerOneNavigation.Target, FixVec2.FromInts(34, 105)) < Fix32.FromInt(2);
            if (!playerZeroExecuted || !playerOneExecuted)
            {
                Fail($"accepted commands did not execute p0={playerZeroExecuted} p1={playerOneExecuted} tick={_server.World.Tick.Value}");
                return;
            }
            _finished = true;
            GD.Print($"M6 COMMAND AUTHORITY SMOKE: PASS sessions=2 accepted=3 rejected=6 tick={_server.World.Tick.Value}");
            CloseAll();
            GetTree().Quit(0);
            return;
        }

        if (Time.GetTicksMsec() >= _deadlineMs)
            Fail($"timeout sessions={(_clientOne.Welcome.HasValue ? 1 : 0) + (_clientTwo.Welcome.HasValue ? 1 : 0)} acks={_clientOne.Acknowledgments.Count + _clientTwo.Acknowledgments.Count}");
    }

    public override void _ExitTree() => CloseAll();

    private void ConfigureClient(SmokeClient client)
    {
        client.Carrier.PacketReceived += packet => OnClientPacket(client, packet);
        client.Carrier.CarrierError += Fail;
    }

    private void OnClientPacket(SmokeClient client, M6TransportPacket packet)
    {
        if (packet.PeerId != MultiplayerPeer.TargetPeerServer || packet.Channel != M6TransportChannel.ReliableOrdered ||
            packet.TransferMode != MultiplayerPeer.TransferModeEnum.Reliable)
        {
            Fail("client received incorrect authority packet metadata");
            return;
        }
        if (!client.Welcome.HasValue && NetworkCommandProtocol.TryDecodeWelcome(packet.Payload, out NetworkCommandSessionWelcome welcome))
        {
            if (welcome.PlayerSlot > 1 ||
                (_clientOne.Welcome.HasValue && _clientOne.Welcome.Value.PlayerSlot == welcome.PlayerSlot) ||
                (_clientTwo.Welcome.HasValue && _clientTwo.Welcome.Value.PlayerSlot == welcome.PlayerSlot))
            {
                Fail($"server assigned duplicate or invalid player slot {welcome.PlayerSlot}");
                return;
            }
            client.Welcome = welcome;
            return;
        }
        if (!NetworkCommandProtocol.TryDecodeAcknowledgment(packet.Payload, out NetworkCommandAcknowledgment acknowledgment))
        {
            Fail("client could not decode command acknowledgment");
            return;
        }
        client.Acknowledgments.Add(acknowledgment);
        if (acknowledgment.Accepted) _latestAcceptedTick = Math.Max(_latestAcceptedTick, acknowledgment.ExecutionTick.Value);
    }

    private void Send(SmokeClient client, CommandEnvelope command, ulong? tokenOverride = null)
    {
        if (!client.Welcome.HasValue) throw new InvalidOperationException("The smoke client has no command session.");
        ulong token = tokenOverride ?? client.Welcome.Value.SessionToken;
        RequireOk(client.Carrier.Send(checked((int)MultiplayerPeer.TargetPeerServer), M6TransportChannel.ReliableOrdered,
            NetworkCommandProtocol.EncodeRequest(token, command)), $"command {command.Sequence} send");
    }

    private SmokeClient ClientForPlayer(byte playerSlot)
    {
        if (_clientOne.Welcome?.PlayerSlot == playerSlot) return _clientOne;
        if (_clientTwo.Welcome?.PlayerSlot == playerSlot) return _clientTwo;
        throw new InvalidOperationException($"No smoke client owns player slot {playerSlot}.");
    }

    private bool HasExpectedAcknowledgments()
    {
        if (!_clientOne.Welcome.HasValue || !_clientTwo.Welcome.HasValue) return false;
        SmokeClient playerZero = ClientForPlayer(0);
        SmokeClient playerOne = ClientForPlayer(1);
        return Has(playerZero, 1, NetworkCommandRejection.EntityNotOwned) &&
               Has(playerZero, 2, NetworkCommandRejection.None) &&
               Has(playerZero, 2, NetworkCommandRejection.SequenceMismatch) &&
               Has(playerZero, 3, NetworkCommandRejection.DebugCommandForbidden) &&
               Has(playerOne, 1, NetworkCommandRejection.PlayerSlotMismatch) &&
               Has(playerOne, 1, NetworkCommandRejection.None) &&
               Has(playerOne, 0, NetworkCommandRejection.MalformedPacket) &&
               Has(playerOne, 2, NetworkCommandRejection.SessionTokenMismatch) &&
               Has(playerOne, 2, NetworkCommandRejection.None);
    }

    private static bool Has(SmokeClient client, uint sequence, NetworkCommandRejection rejection)
        => client.Acknowledgments.Any(ack => ack.ClientSequence == sequence && ack.Rejection == rejection);

    private void RequireOk(Error result, string operation)
    {
        if (result != Error.Ok) Fail($"{operation} failed: {result}");
    }

    private void Fail(string reason)
    {
        if (_finished) return;
        _finished = true;
        GD.PrintErr($"M6 COMMAND AUTHORITY SMOKE: FAIL {reason}");
        CloseAll();
        GetTree().Quit(2);
    }

    private void CloseAll()
    {
        _server?.Stop();
        _clientOne.Carrier.Dispose();
        _clientTwo.Carrier.Dispose();
        _server = null;
    }

    private static CommandEnvelope Move(byte playerSlot, uint sequence, EntityId entity, int x, int y)
        => new(new SimTick(1), playerSlot, sequence, SimCommandType.Move, new[] { entity }, FixVec2.FromInts(x, y));

    private static int ReserveLoopbackUdpPort()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
