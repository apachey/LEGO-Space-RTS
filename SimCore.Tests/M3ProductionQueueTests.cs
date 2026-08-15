using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3ProductionQueueTests
{
    private static readonly ContentId Crew = StableId.FromKey("unit.rock_raiders.crew");
    private static readonly ContentId HoverScout = StableId.FromKey("unit.rock_raiders.hover_scout");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");

    [Test]
    public void FirstPlayableProductionDefinitionsMatchCanon()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        AssertDefinition(content, "unit.rock_raiders.crew", "building.rock_raiders.hq", 50, 0, 1, 320);
        AssertDefinition(content, "unit.rock_raiders.hover_scout", "building.rock_raiders.vehicle_service_bay", 75, 10, 1, 400);
        AssertDefinition(content, "unit.rock_raiders.rapid_rider", "building.rock_raiders.vehicle_service_bay", 90, 10, 2, 560);
        AssertDefinition(content, "unit.rock_raiders.loader_dozer", "building.rock_raiders.vehicle_service_bay", 125, 15, 3, 720);
        Assert.That(content.TryGetMovement("movement.prototype.rapid_rider", out PrototypeMovementProfile rider), Is.True);
        Assert.That(rider.MaxSpeed, Is.EqualTo(Fix32.FromRatio(210, 100)));
    }

    [Test]
    public void QueueReservesLocalOreAndEnforcesProducerAndCapacity()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = FirstProductionFacility(world, 0);
        ResourceConservationTotals initial = ResourceConservation.Measure(world, ResourceType.Ore);

        Assert.That(ProductionSystem.TryQueue(world, 0, hq, HoverScout), Is.False, "HQ cannot produce Service Bay units.");
        for (int i = 0; i < Production.Capacity; i++) Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True, $"queue item {i}");
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.False, "Queue is capped at eight items.");

        Production production = world.Entities.Production.Get(hq);
        Assert.That(production.Count, Is.EqualTo(8));
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(100));
        Assert.That(ResourceConservation.Measure(world, ResourceType.Ore).Total, Is.EqualTo(initial.Total));
    }

    [Test]
    public void TickDrivenCompletionSpawnsCanonicalCrewAndConsumesReservation()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = FirstProductionFacility(world, 0);
        int beforeEntities = world.Entities.Alive.Count;
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);

        new SimulationRunner(world).StepTicks(320);

        Assert.That(world.Entities.Production.Get(hq).Count, Is.Zero);
        Assert.That(world.Entities.Alive.Count, Is.EqualTo(beforeEntities + 1));
        EntityId spawned = world.Entities.Alive.Single(id => world.Entities.Worker.Has(id));
        Assert.That(world.Entities.Selectable.Get(spawned).ContentType, Is.EqualTo(Crew));
        Assert.That(world.Entities.Builder.Has(spawned), Is.True);
        Assert.That(world.Entities.Ownership.Get(spawned).PlayerSlot, Is.Zero);
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(450));
    }

    [Test]
    public void CompletedServiceBaySpawnsItsAuthoredHoverScout()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = world.Entities.Alive.Single(id => world.Entities.Builder.Has(id) &&
            world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0);
        FixVec2 siteEdge = FixVec2.FromInts(28, 92);
        world.Entities.Transform.Get(worker).Position = siteEdge;
        world.Entities.Movement.Get(worker).LastPosition = siteEdge;
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ServiceBay, 29, 90, 0, out EntityId serviceBay, out PlacementFailure failure), Is.True, failure.ToString());
        world.Entities.ConstructionSite.Get(serviceBay).RequiredTicks = 1;
        SimulationRunner runner = new(world);
        runner.StepOneTick();
        Assert.That(world.Entities.Production.Has(serviceBay), Is.True, "A completed production building must own its queue.");
        Assert.That(ProductionSystem.TryQueue(world, 0, serviceBay, HoverScout), Is.True);
        ref Production production = ref world.Entities.Production.Get(serviceBay);
        ProductionQueueItem item = production.Get(0); item.RemainingTicks = 1; production.Set(0, item);

        runner.StepOneTick();

        EntityId scout = world.Entities.Alive.Single(id => world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == HoverScout);
        Assert.That(world.Entities.Navigation.Get(scout).Footprint, Is.EqualTo(FootprintClass.Small));
        Assert.That(world.Entities.Ownership.Get(scout).PlayerSlot, Is.Zero);
        Assert.That(world.Entities.Production.Get(serviceBay).Count, Is.Zero);
    }

    [Test]
    public void CompletedUnitWaitsInFacilityUntilExitBecomesLegal()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = FirstProductionFacility(world, 0);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        ref Production production = ref world.Entities.Production.Get(hq);
        ProductionQueueItem item = production.Get(0); item.RemainingTicks = 1; production.Set(0, item);
        Building building = world.Entities.Building.Get(hq);
        BuildingDefinition definition = world.Content.Buildings.Single(x => x.Id == building.Type);
        IntRect exit = ConstructionPlacement.GetProductionExit(definition, building.AnchorX, building.AnchorY, building.Orientation);
        IntRect exitNav = new((short)(exit.X * MapGrid.NavPerBuild), (short)(exit.Y * MapGrid.NavPerBuild),
            (short)(exit.Width * MapGrid.NavPerBuild), (short)(exit.Height * MapGrid.NavPerBuild));
        world.Map.SetFlagsRect(exitNav, MapCellFlags.Impassable, MapCellFlags.Buildable);
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        Assert.That(world.Entities.Production.Get(hq).Count, Is.EqualTo(1));
        Assert.That(world.Entities.Production.Get(hq).SpawnBlocked, Is.True);
        Assert.That(world.Entities.Production.Get(hq).Get(0).RemainingTicks, Is.Zero);

        world.Map.SetFlagsRect(exitNav, MapCellFlags.Ground | MapCellFlags.Buildable, MapCellFlags.Impassable);
        runner.StepOneTick();
        Assert.That(world.Entities.Production.Get(hq).Count, Is.Zero);
        Assert.That(world.Entities.Alive.Count(id => world.Entities.Worker.Has(id)), Is.EqualTo(1));
    }

    [Test]
    public void WorkerRallyToVisibleResourceStartsHarvestAfterSpawn()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = FirstProductionFacility(world, 0);
        EntityId resource = world.Entities.Alive.First(id => world.Entities.ResourceNode.Has(id));
        FixVec2 resourcePosition = world.Entities.Transform.Get(resource).Position;
        Assert.That(ProductionSystem.TrySetRally(world, 0, hq, resourcePosition, resource), Is.True);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        ref Production production = ref world.Entities.Production.Get(hq);
        ProductionQueueItem item = production.Get(0); item.RemainingTicks = 1; production.Set(0, item);

        new SimulationRunner(world).StepOneTick();

        EntityId worker = world.Entities.Alive.Single(id => world.Entities.Worker.Has(id));
        Assert.That(world.Entities.Worker.Get(worker).ResourceTarget, Is.EqualTo(resource));
        Assert.That(world.Entities.Worker.Get(worker).TaskState, Is.EqualTo(WorkerTaskState.MovingToResource));
    }

    [Test]
    public void SnapshotPreservesQueueRallyAndDeterministicSpawn()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = FirstProductionFacility(world, 0);
        Assert.That(ProductionSystem.TrySetRally(world, 0, hq, FixVec2.FromInts(40, 80), EntityId.None), Is.True);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        new SimulationRunner(world).StepTicks(120);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(restored.Entities.Production.Get(hq).Get(0).RemainingTicks, Is.EqualTo(200));
        Assert.That(restored.Entities.Production.Get(hq).RallyPoint, Is.EqualTo(FixVec2.FromInts(40, 80)));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));

        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(200); restoredRunner.StepTicks(200);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        Assert.That(restored.Entities.Alive.Count(id => restored.Entities.Worker.Has(id)), Is.EqualTo(1));
    }

    private static void AssertDefinition(PrototypeContentCatalog content, string unit, string producer, int ore, int energy, int oc, int ticks)
    {
        Assert.That(content.TryGetProduction(StableId.FromKey(unit), out UnitProductionDefinition definition), Is.True, unit);
        Assert.Multiple(() =>
        {
            Assert.That(definition.ProducerType, Is.EqualTo(StableId.FromKey(producer)));
            Assert.That(definition.OreCost, Is.EqualTo(ore)); Assert.That(definition.EnergyCost, Is.EqualTo(energy));
            Assert.That(definition.OperationsCapacity, Is.EqualTo(oc)); Assert.That(definition.BuildTicks, Is.EqualTo(ticks));
        });
    }

    private static EntityId FirstProductionFacility(SimulationWorld world, byte player)
        => world.Entities.Alive.First(id => world.Entities.Production.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == player);

}
}
