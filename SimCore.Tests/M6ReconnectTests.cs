using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M6ReconnectTests
{
    private const ulong Token = 0xAABBCCDDEEFF0011UL;

    [Test]
    public void RequestAuthenticatesTheRetainedSessionAndExactMatchManifest()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        NetworkMatchManifest manifest = new(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        NetworkReconnectRequest request = new(Token, 1, manifest);
        Assert.That(NetworkReconnectProtocol.TryDecodeRequest(NetworkReconnectProtocol.EncodeRequest(request), out NetworkReconnectRequest decoded), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.SessionToken, Is.EqualTo(Token));
            Assert.That(decoded.PlayerSlot, Is.EqualTo(1));
            Assert.That(decoded.Manifest.SimulationProtocolVersion, Is.EqualTo(SnapshotSerializer.SimulationProtocolVersion));
            Assert.That(decoded.Manifest.GameplayContentHash, Is.EqualTo(world.Content.ContentHash));
            Assert.That(decoded.Manifest.MapHash, Is.EqualTo(manifest.MapHash));
            Assert.That(decoded.Manifest.Matches(manifest), Is.True);
        });
    }

    [Test]
    public void ReconnectStateRestoresLegalFullViewCommandSequenceAndOwnPendingWork()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId owned = ScenarioFactory.OwnedIds(world, 0).Single();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 8, SimCommandType.Move,
            new[] { owned }, FixVec2.FromInts(90, 40)));
        world.GetQueue(owned).Enqueue(new UnitOrder(UnitOrderType.Move, FixVec2.FromInts(80, 40)));
        EntityId producer = world.Entities.Create();
        world.Entities.Ownership.Set(producer, new Ownership { PlayerSlot = 0 });
        Production production = new() { HasRallyPoint = true, RallyPoint = FixVec2.FromInts(42, 43) };
        production.TryEnqueue(new ProductionQueueItem
        {
            UnitType = StableId.FromKey("unit.test.reconnect"),
            TotalTicks = 100,
            RemainingTicks = 60,
            ReservedOre = 12,
            RequiredEnergy = 5,
            RequiredCrystals = 1,
            ReservedOperationsCapacity = 3
        });
        world.Entities.Production.Set(producer, production);

        ServerCommandSession retained = new(17, 0, Token, lastProcessedSequence: 8);
        ServerCommandSession rebound = retained.Rebind(23);
        ServerSnapshotSession snapshots = new();
        byte[] legalFull = snapshots.CreatePacket(world, 0);
        NetworkMatchManifest manifest = new(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        NetworkReconnectState captured = NetworkReconnectProtocol.CaptureAccepted(world, rebound, manifest, legalFull);

        byte[] packet = NetworkReconnectProtocol.EncodeState(captured);
        Assert.That(NetworkReconnectProtocol.TryDecodeState(packet, out NetworkReconnectState decoded), Is.True);
        Assert.That(NetworkSnapshotProtocol.TryDecodeFrame(decoded.FullLegalSnapshotPacket, out NetworkSnapshotFrame full), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.Accepted, Is.True);
            Assert.That(decoded.SessionToken, Is.EqualTo(Token));
            Assert.That(decoded.PlayerSlot, Is.Zero);
            Assert.That(decoded.LastProcessedCommandSequence, Is.EqualTo(8));
            Assert.That(decoded.PendingCommands.Select(command => command.Sequence), Is.EqualTo(new uint[] { 8 }));
            Assert.That(decoded.OwnedOrderQueues.Select(queue => queue.EntityId), Does.Contain(owned));
            Assert.That(decoded.OwnedProductionQueues.Select(queue => queue.Producer), Does.Contain(producer));
            Assert.That(decoded.OwnedProductionQueues.Single(queue => queue.Producer == producer).Items.Single().RemainingTicks, Is.EqualTo(60));
            Assert.That(full.IsFull, Is.True);
            Assert.That(full.PlayerSlot, Is.Zero);
            Assert.That(full.EntityUpserts.Any(entity => entity.Decode().Owner == 1), Is.False, "Reconnect must use the same legal recipient view as regular replication.");
        });
    }

    [Test]
    public void RejectionPacketContainsNoAuthoritativeOrRecipientState()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        NetworkMatchManifest manifest = new(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        NetworkReconnectState rejected = NetworkReconnectProtocol.Rejected(NetworkReconnectRejection.ManifestMismatch, manifest);
        Assert.That(NetworkReconnectProtocol.TryDecodeState(NetworkReconnectProtocol.EncodeState(rejected), out NetworkReconnectState decoded), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.Rejection, Is.EqualTo(NetworkReconnectRejection.ManifestMismatch));
            Assert.That(decoded.Accepted, Is.False);
            Assert.That(decoded.SessionToken, Is.Zero);
            Assert.That(decoded.FullLegalSnapshotPacket, Is.Empty);
            Assert.That(decoded.PendingCommands, Is.Empty);
            Assert.That(decoded.OwnedOrderQueues, Is.Empty);
            Assert.That(decoded.OwnedProductionQueues, Is.Empty);
        });
    }
}
