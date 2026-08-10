using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3ConstructionJobTests
{
    private static readonly ContentId ProcessingPlant = StableId.FromKey("building.rock_raiders.ore_processing_plant");

    [Test]
    public void CrewMustReachThePhysicalSiteBeforeConstructionBegins()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId worker = FirstBuilder(world, 0);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId siteId, out PlacementFailure failure), Is.True, failure.ToString());
        SimulationRunner runner = new(world);

        int travelTicks = 0;
        while (world.Entities.ConstructionSite.Get(siteId).ProgressTicks == 0 && travelTicks < 1_000)
        {
            runner.StepOneTick();
            travelTicks++;
        }

        ConstructionSite site = world.Entities.ConstructionSite.Get(siteId);
        Assert.That(travelTicks, Is.GreaterThan(1), "Construction must not begin on the placement tick while Crew is still away.");
        Assert.That(travelTicks, Is.LessThan(1_000), "Crew did not reach the reserved footprint edge.");
        Assert.That(site.ProgressTicks, Is.EqualTo(1));
        Assert.That(site.ConsumedOre, Is.EqualTo(28), "Physical construction commits the canonical first 20% immediately.");
        Assert.That(site.ReservedOre, Is.EqualTo(112));
        Assert.That(world.Entities.Builder.Get(worker).JobState, Is.EqualTo(BuilderJobState.Constructing));
    }

    [Test]
    public void CompletionKeepsTheSameEntityAndConsumesTheFullReservation()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId worker = FirstBuilder(world, 0);
        PlaceBuilderAtSiteEdge(world, worker, 5, 85);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId siteId, out _), Is.True);
        world.Entities.ConstructionSite.Get(siteId).RequiredTicks = 4;

        new SimulationRunner(world).StepTicks(4);

        Assert.That(world.Entities.Exists(siteId), Is.True);
        Assert.That(world.Entities.ConstructionSite.Has(siteId), Is.False);
        Assert.That(world.Entities.Building.Get(siteId).State, Is.EqualTo(BuildingState.Completed));
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(360));
        Assert.That(world.Entities.Builder.Get(worker).JobState, Is.EqualTo(BuilderJobState.Idle));
    }

    [Test]
    public void CancellationAfterWorkStartsReturnsUnspentAndHalfConsumedOre()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId worker = FirstBuilder(world, 0);
        PlaceBuilderAtSiteEdge(world, worker, 5, 85);
        ResourceConservationTotals initial = ResourceConservation.Measure(world, ResourceType.Ore);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId siteId, out _), Is.True);
        new SimulationRunner(world).StepOneTick();

        Assert.That(ConstructionPlacement.TryCancel(world, 0, siteId), Is.True);

        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(486));
        Assert.That(ResourceConservation.Measure(world, ResourceType.Ore).Total, Is.EqualTo(initial.Total - 14));
        Assert.That(world.Map.GetFlags(12, 164).HasFlag(MapCellFlags.Buildable), Is.True);
        Assert.That(world.Entities.Builder.Get(worker).JobState, Is.EqualTo(BuilderJobState.Idle));
    }

    [Test]
    public void ShiftPlacementQueuesSeveralSitesForOneCrewInOrder()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId worker = FirstBuilder(world, 0);
        PlaceBuilderAtSiteEdge(world, worker, 5, 85);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId first, out _), Is.True);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 96, 0, out EntityId second, out _, queueBuilder: true), Is.True);
        world.Entities.ConstructionSite.Get(first).RequiredTicks = 1;
        world.Entities.ConstructionSite.Get(second).RequiredTicks = 1;
        world = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(world.GetQueue(worker).Count, Is.EqualTo(1));
        Assert.That(world.GetQueue(worker)[0].Type, Is.EqualTo(UnitOrderType.Construct));
        Assert.That(world.GetQueue(worker)[0].TargetEntity, Is.EqualTo(second));
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        Assert.That(world.Entities.Building.Get(first).State, Is.EqualTo(BuildingState.Completed));
        Assert.That(world.Entities.Builder.Get(worker).ConstructionTarget, Is.EqualTo(second));

        PlaceBuilderAtSiteEdge(world, worker, 5, 99);
        runner.StepOneTick();
        Assert.That(world.Entities.Building.Get(second).State, Is.EqualTo(BuildingState.Completed));
        Assert.That(world.Entities.Builder.Get(worker).JobState, Is.EqualTo(BuilderJobState.Idle));
    }

    [Test]
    public void EligibleCrewCanAssistAndContributeDeterministically()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId[] builders = OwnedBuilders(world, 0).Take(2).ToArray();
        Assert.That(builders, Has.Length.EqualTo(2));
        PlaceBuilderAtSiteEdge(world, builders[0], 5, 85);
        PlaceBuilderAtSiteEdge(world, builders[1], 5, 84);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { builders[0] }, ProcessingPlant, 6, 82, 0, out EntityId siteId, out _), Is.True);
        world.Entities.ConstructionSite.Get(siteId).RequiredTicks = 4;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.AssistConstruction,
            new[] { builders[1] }, FixVec2.Zero, targetEntity: siteId));

        new SimulationRunner(world).StepTicks(2);

        Assert.That(world.Entities.Building.Get(siteId).State, Is.EqualTo(BuildingState.Completed));
        Assert.That(world.Entities.ConstructionSite.Has(siteId), Is.False);
    }

    [Test]
    public void SnapshotPreservesBuilderJobCommitmentAndDeterministicCompletion()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId worker = FirstBuilder(world, 0);
        PlaceBuilderAtSiteEdge(world, worker, 5, 85);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId siteId, out _), Is.True);
        world.Entities.ConstructionSite.Get(siteId).RequiredTicks = 10;
        SimulationRunner originalRunner = new(world);
        originalRunner.StepTicks(3);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(restored.Entities.Builder.Get(worker).ConstructionTarget, Is.EqualTo(siteId));
        Assert.That(restored.Entities.ConstructionSite.Get(siteId).ConsumedOre, Is.EqualTo(world.Entities.ConstructionSite.Get(siteId).ConsumedOre));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));

        SimulationRunner restoredRunner = new(restored);
        originalRunner.StepTicks(7);
        restoredRunner.StepTicks(7);
        Assert.That(restored.Entities.Building.Get(siteId).State, Is.EqualTo(BuildingState.Completed));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static EntityId FirstBuilder(SimulationWorld world, byte player) => OwnedBuilders(world, player).First();

    private static System.Collections.Generic.IEnumerable<EntityId> OwnedBuilders(SimulationWorld world, byte player)
        => world.Entities.Alive.Where(id => world.Entities.Builder.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == player);

    private static void PlaceBuilderAtSiteEdge(SimulationWorld world, EntityId builder, int x, int y)
    {
        FixVec2 position = FixVec2.FromInts(x, y);
        world.Entities.Transform.Get(builder).Position = position;
        world.Entities.Movement.Get(builder).LastPosition = position;
    }
}
}
