using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5AcceptanceScenarioTests
{
    [Test]
    public void FreshHandoffContainsEveryM5ProofAndPreservesDeterministicContinuation()
    {
        SimulationWorld world = M5AcceptanceScenarioFactory.Create();
        Assert.That(M5AcceptanceScenarioFactory.IsFreshHandoffReady(world, out string initialReason), Is.True, initialReason);

        SimulationRunner runner = new(world);
        runner.StepTicks(40);
        Assert.That(M5AcceptanceScenarioFactory.IsFreshHandoffReady(world, out string runningReason), Is.True, runningReason);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        SimulationRunner restoredRunner = new(restored);
        runner.StepTicks(80);
        restoredRunner.StepTicks(80);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    [Test]
    public void AllAcceptancePassengersReappearOutsideTheDestinationBuilding()
    {
        SimulationWorld world = M5AcceptanceScenarioFactory.Create();
        Assert.That(M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.SettlementStationKey, out EntityId destination), Is.True);
        Building station = world.Entities.Building.Get(destination);
        ContentId[] passengerTypes =
        {
            StableId.FromKey(M5AcceptanceScenarioFactory.WorkerRobotKey),
            StableId.FromKey(M5AcceptanceScenarioFactory.DoubleHoverKey),
            StableId.FromKey(M5AcceptanceScenarioFactory.JetScooterKey)
        };

        SimulationRunner runner = new(world);
        for (int tick = 0; tick < 1200 && CountTransfers(world) > 0; tick++) runner.StepOneTick();

        Assert.That(CountTransfers(world), Is.Zero, "Every acceptance passenger must finish the transfer.");
        for (int i = 0; i < passengerTypes.Length; i++)
        {
            Assert.That(FindByType(world, passengerTypes[i], out EntityId passenger), Is.True);
            Assert.That(world.Entities.Transform.TryGet(passenger, out SimTransform transform), Is.True, $"Passenger {passenger.Value} never returned to the map.");
            Assert.That(IsInside(station, transform.Position), Is.False, $"Passenger {passenger.Value} exited inside the destination building.");
        }
    }

    private static int CountTransfers(SimulationWorld world)
    {
        int count = 0;
        foreach (EntityId id in world.Entities.Alive) if (world.Entities.TubeTransfer.Has(id)) count++;
        return count;
    }

    private static bool FindByType(SimulationWorld world, ContentId type, out EntityId entity)
    {
        foreach (EntityId id in world.Entities.Alive)
            if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type)
            { entity = id; return true; }
        entity = EntityId.None;
        return false;
    }

    private static bool IsInside(Building building, FixVec2 position)
        => position.X > Fix32.FromInt(building.AnchorX) && position.X < Fix32.FromInt(building.AnchorX + building.FootprintWidth) &&
           position.Y > Fix32.FromInt(building.AnchorY) && position.Y < Fix32.FromInt(building.AnchorY + building.FootprintHeight);
}
}
