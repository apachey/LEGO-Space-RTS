using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3ResourceBankingTests
{
    [Test]
    public void DeliveredOreRemainsHauledUntilTheNextAuthoritativeTick()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId receiverId = FirstOwnedReceiver(world, 0);
        ref Worker worker = ref world.Entities.Worker.Get(workerId);
        ref ResourceCarrier carrier = ref world.Entities.ResourceCarrier.Get(workerId);
        SimTransform receiverTransform = world.Entities.Transform.Get(receiverId);
        ref SimTransform workerTransform = ref world.Entities.Transform.Get(workerId);
        workerTransform.Position = receiverTransform.Position;
        world.Entities.Movement.Get(workerId).LastPosition = workerTransform.Position;
        worker.TaskState = WorkerTaskState.ReturningToReceiver;
        worker.ReceiverTarget = receiverId;
        worker.ResourceTarget = EntityId.None;
        carrier.Amount = 3;
        SimulationRunner runner = new(world);

        runner.StepOneTick();

        Assert.That(world.Entities.ResourceReceiver.Get(receiverId).PendingHauledAmount, Is.EqualTo(3));
        Assert.That(world.Entities.ResourceBank.Get(receiverId).ProcessedAmount, Is.EqualTo(500));

        runner.StepOneTick();

        Assert.That(world.Entities.ResourceReceiver.Get(receiverId).PendingHauledAmount, Is.Zero);
        Assert.That(world.Entities.ResourceBank.Get(receiverId).ProcessedAmount, Is.EqualTo(503));
    }

    [Test]
    public void BankingMovesHauledOreToTheOwningLocalReserveWithoutCreatingResource()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId player0 = FirstOwnedReceiver(world, 0);
        EntityId player1 = FirstOwnedReceiver(world, 1);
        world.Entities.ResourceReceiver.Get(player0).PendingHauledAmount = 7;
        ResourceConservationTotals before = ResourceConservation.Measure(world, ResourceType.Ore);

        new SimulationRunner(world).StepOneTick();

        ResourceConservationTotals after = ResourceConservation.Measure(world, ResourceType.Ore);
        Assert.That(after.Total, Is.EqualTo(before.Total));
        Assert.That(after.Hauled, Is.EqualTo(before.Hauled - 7));
        Assert.That(after.Processed, Is.EqualTo(before.Processed + 7));
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(507));
        Assert.That(world.GetProcessedResourceTotal(1, ResourceType.Ore), Is.EqualTo(500));
        Assert.That(world.Entities.ResourceBank.Get(player1).ProcessedAmount, Is.EqualTo(500));
    }

    [Test]
    public void FullHarvestRouteConservesOreAcrossEveryAuthoritativeStage()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId workerId = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId nodeId = FirstResourceNode(world);
        world.Entities.ResourceNode.Get(nodeId).Remaining = 3;
        PlaceWithinMiningRange(world, workerId, nodeId);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Harvest, new[] { workerId }, FixVec2.Zero, targetEntity: nodeId));
        ResourceConservationTotals initial = ResourceConservation.Measure(world, ResourceType.Ore);
        SimulationRunner runner = new(world);
        bool sawCarried = false, sawHauled = false, sawProcessed = false;

        for (int i = 0; i < 400; i++)
        {
            runner.StepOneTick();
            ResourceConservationTotals current = ResourceConservation.Measure(world, ResourceType.Ore);
            Assert.That(current.Total, Is.EqualTo(initial.Total), $"Ore conservation failed at tick {world.Tick.Value}.");
            sawCarried |= current.Carried > 0;
            sawHauled |= current.Hauled > 0;
            sawProcessed |= current.Processed > 0;
        }

        Assert.That(sawCarried, Is.True);
        Assert.That(sawHauled, Is.True);
        Assert.That(sawProcessed, Is.True);
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(503));
    }

    [Test]
    public void SnapshotPreservesHauledAndProcessedOreAndContinuesDeterministically()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId receiverId = FirstOwnedReceiver(world, 0);
        world.Entities.ResourceReceiver.Get(receiverId).PendingHauledAmount = 2;
        world.Entities.ResourceBank.Get(receiverId).ProcessedAmount = 5;

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(restored.Entities.ResourceReceiver.Get(receiverId).PendingHauledAmount, Is.EqualTo(2));
        Assert.That(restored.Entities.ResourceBank.Get(receiverId).ProcessedAmount, Is.EqualTo(5));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));

        SimulationRunner originalRunner = new(world);
        SimulationRunner restoredRunner = new(restored);
        originalRunner.StepTicks(20);
        restoredRunner.StepTicks(20);
        Assert.That(restored.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(7));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
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
        worker.Position = world.Entities.Transform.Get(nodeId).Position + new FixVec2(Fix32.One, Fix32.Zero);
        world.Entities.Movement.Get(workerId).LastPosition = worker.Position;
    }
}
}
