using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3WorkerHarvestingTests
{
    [Test]
    public void CrewSpawnsWithCanonicalOreExtractionAndCarryLimits()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0)[0];

        Worker worker = world.Entities.Worker.Get(workerId);
        ResourceCarrier carrier = world.Entities.ResourceCarrier.Get(workerId);
        Assert.That(worker.TicksPerOre, Is.EqualTo(30));
        Assert.That(carrier.Type, Is.EqualTo(ResourceType.Ore));
        Assert.That(carrier.Amount, Is.Zero);
        Assert.That(carrier.Capacity, Is.EqualTo(8));
    }

    [Test]
    public void HarvestAffectsEligibleWorkersOnlyAndExtractsOneOrePerThirtyActiveTicks()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(2);
        EntityId[] owned = ScenarioFactory.OwnedIds(world, 0);
        EntityId workerId = owned.Single(id => world.Entities.Worker.Has(id));
        EntityId nonWorkerId = owned.Single(id => !world.Entities.Worker.Has(id) && world.Entities.Navigation.Has(id));
        EntityId nodeId = FirstResourceNode(world);
        PlaceWithinMiningRange(world, workerId, nodeId);
        SimulationRunner runner = new(world);

        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Harvest, owned, FixVec2.Zero, targetEntity: nodeId));
        runner.StepTicks(29);
        Assert.That(world.Entities.ResourceNode.Get(nodeId).Remaining, Is.EqualTo(900));
        Assert.That(world.Entities.ResourceCarrier.Get(workerId).Amount, Is.Zero);
        Assert.That(world.Entities.Navigation.Get(nonWorkerId).HasTarget, Is.False);

        runner.StepOneTick();
        Assert.That(world.Entities.ResourceNode.Get(nodeId).Remaining, Is.EqualTo(899));
        Assert.That(world.Entities.ResourceCarrier.Get(workerId).Amount, Is.EqualTo(1));
        Assert.That(world.Entities.Worker.Get(workerId).TaskState, Is.EqualTo(WorkerTaskState.Mining));
    }

    [Test]
    public void WorkerStopsMiningAtEightOreAndStartsPhysicalReturn()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0)[0];
        EntityId nodeId = FirstResourceNode(world);
        PlaceWithinMiningRange(world, workerId, nodeId);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Harvest, new[] { workerId }, FixVec2.Zero, targetEntity: nodeId));

        new SimulationRunner(world).StepTicks(240);

        Assert.That(world.Entities.ResourceCarrier.Get(workerId).Amount, Is.EqualTo(8));
        Assert.That(world.Entities.ResourceNode.Get(nodeId).Remaining, Is.EqualTo(892));
        Assert.That(world.Entities.Worker.Get(workerId).TaskState, Is.EqualTo(WorkerTaskState.ReturningToReceiver));
        Assert.That(world.Entities.Worker.Get(workerId).ResourceTarget, Is.EqualTo(nodeId));
        Assert.That(world.Entities.Worker.Get(workerId).ReceiverTarget, Is.Not.EqualTo(EntityId.None));
    }

    [Test]
    public void DepletedNodePayloadIsPhysicallyDeliveredToOwnedHqReceiver()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0)[0];
        EntityId nodeId = FirstResourceNode(world);
        ref ResourceNode node = ref world.Entities.ResourceNode.Get(nodeId);
        node.Remaining = 3;
        PlaceWithinMiningRange(world, workerId, nodeId);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Harvest, new[] { workerId }, FixVec2.Zero, targetEntity: nodeId));

        new SimulationRunner(world).StepTicks(400);

        Assert.That(node.Remaining, Is.Zero);
        Assert.That(world.Entities.ResourceCarrier.Get(workerId).Amount, Is.Zero);
        Assert.That(world.Entities.Worker.Get(workerId).TaskState, Is.EqualTo(WorkerTaskState.Idle));
        EntityId receiverId = FirstOwnedReceiver(world, 0);
        Assert.That(world.Entities.ResourceReceiver.Get(receiverId).PendingHauledAmount, Is.Zero);
        Assert.That(world.Entities.ResourceBank.Get(receiverId).ProcessedAmount, Is.EqualTo(3));
    }

    [Test]
    public void SnapshotContinuationPreservesMiningProgressCargoAndQueuedResourceTarget()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0)[0];
        EntityId[] nodes = world.Entities.Alive.Where(id => world.Entities.ResourceNode.Has(id)).Take(2).ToArray();
        PlaceWithinMiningRange(world, workerId, nodes[0]);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Harvest, new[] { workerId }, FixVec2.Zero, targetEntity: nodes[0]));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2), 0, 2, SimCommandType.Harvest, new[] { workerId }, FixVec2.Zero, CommandModifiers.Queue, nodes[1]));
        SimulationRunner original = new(world);
        original.StepTicks(47);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(restoredWorld.Entities.Worker.Get(workerId).ExtractionTicks, Is.EqualTo(world.Entities.Worker.Get(workerId).ExtractionTicks));
        Assert.That(restoredWorld.Entities.ResourceCarrier.Get(workerId).Amount, Is.EqualTo(1));
        Assert.That(restoredWorld.GetQueue(workerId)[0].TargetEntity, Is.EqualTo(nodes[1]));
        SimulationRunner restored = new(restoredWorld);

        original.StepTicks(193);
        restored.StepTicks(193);
        Assert.That(StateHasher.Hash(restored.World), Is.EqualTo(StateHasher.Hash(original.World)));
    }

    private static EntityId FirstResourceNode(SimulationWorld world)
    {
        foreach (EntityId id in world.Entities.Alive) if (world.Entities.ResourceNode.Has(id)) return id;
        Assert.Fail("Scenario contains no resource node.");
        return EntityId.None;
    }

    private static EntityId FirstOwnedReceiver(SimulationWorld world, byte player)
    {
        foreach (EntityId id in world.Entities.Alive)
            if (world.Entities.ResourceReceiver.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == player) return id;
        Assert.Fail("Scenario contains no owned resource receiver.");
        return EntityId.None;
    }

    private static void PlaceWithinMiningRange(SimulationWorld world, EntityId workerId, EntityId nodeId)
    {
        ref SimTransform worker = ref world.Entities.Transform.Get(workerId);
        SimTransform node = world.Entities.Transform.Get(nodeId);
        worker.Position = node.Position + new FixVec2(Fix32.One, Fix32.Zero);
        ref Movement movement = ref world.Entities.Movement.Get(workerId);
        movement.LastPosition = worker.Position;
    }
}
}
