using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3OperationsCapacityTests
{
    private static readonly ContentId Crew = StableId.FromKey("unit.rock_raiders.crew");
    private static readonly ContentId HoverScout = StableId.FromKey("unit.rock_raiders.hover_scout");
    private static readonly ContentId RapidRider = StableId.FromKey("unit.rock_raiders.rapid_rider");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");

    [Test]
    public void CanonicalRockRaiderUnitsAndBuildingsExposeOperationsCapacity()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        Assert.Multiple(() =>
        {
            Assert.That(Entity(content, "unit.rock_raiders.crew").OperationsCapacity, Is.EqualTo(1));
            Assert.That(Entity(content, "unit.rock_raiders.hover_scout").OperationsCapacity, Is.EqualTo(1));
            Assert.That(Entity(content, "unit.rock_raiders.rapid_rider").OperationsCapacity, Is.EqualTo(2));
            Assert.That(Entity(content, "unit.rock_raiders.loader_dozer").OperationsCapacity, Is.EqualTo(3));
            Assert.That(Entity(content, "unit.rock_raiders.chrome_crusher").OperationsCapacity, Is.EqualTo(6));
            Assert.That(Building(content, "building.rock_raiders.hq").OperationsCapacityProvided, Is.EqualTo(16));
            Assert.That(Building(content, "building.rock_raiders.vehicle_service_bay").OperationsCapacityProvided, Is.EqualTo(4));
        });
    }

    [Test]
    public void PlayableOpeningUsesSixCrewAndSixOfSixteenCapacity()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();

        EntityId[] playerUnits = ScenarioFactory.OwnedIds(world, 0);
        Assert.That(playerUnits, Has.Length.EqualTo(6));
        Assert.That(playerUnits.All(id => world.Entities.Worker.Has(id) && world.Entities.Selectable.Get(id).ContentType == Crew), Is.True);
        OperationsCapacityState capacity = world.GetOperationsCapacity(0);
        Assert.Multiple(() =>
        {
            Assert.That(capacity.Active, Is.EqualTo(6));
            Assert.That(capacity.Reserved, Is.Zero);
            Assert.That(capacity.Maximum, Is.EqualTo(16));
            Assert.That(capacity.IsAdvanceWarning, Is.False);
        });

        SimulationWorld engineering = ScenarioFactory.CreateFirstControllable(18);
        Assert.That(ScenarioFactory.OwnedIds(engineering, 0), Has.Length.EqualTo(18), "The M2 engineering scenario remains available for movement regression coverage.");
    }

    [Test]
    public void QueueReservesCapacityAndRejectsOrdersPastMaximumWithoutSpendingOre()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId hq = FirstProductionFacility(world, 0);
        EntityId bank = world.Entities.Alive.Single(id => world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.Get(id).PlayerSlot == 0);
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 5_000;
        EntityId serviceBay = AddCompletedServiceBay(world, 0);

        for (int i = 0; i < Production.Capacity; i++) Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        for (int i = 0; i < 3; i++) Assert.That(ProductionSystem.TryQueue(world, 0, serviceBay, RapidRider), Is.True);
        OperationsCapacitySystem.Recalculate(world);
        OperationsCapacityState full = world.GetOperationsCapacity(0);
        int oreBeforeRejectedOrder = world.Entities.ResourceBank.Get(bank).ProcessedAmount;

        Assert.Multiple(() =>
        {
            Assert.That(full.Active, Is.EqualTo(6));
            Assert.That(full.Reserved, Is.EqualTo(14));
            Assert.That(full.Maximum, Is.EqualTo(20));
            Assert.That(full.Used, Is.EqualTo(20));
            Assert.That(full.IsAdvanceWarning, Is.True);
            Assert.That(ProductionSystem.TryQueue(world, 0, serviceBay, HoverScout), Is.False);
            Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.EqualTo(oreBeforeRejectedOrder));
        });
    }

    [Test]
    public void CompletionMovesReservedCapacityToActive()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId hq = FirstProductionFacility(world, 0);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        OperationsCapacityState queued = world.GetOperationsCapacity(0);
        ref Production production = ref world.Entities.Production.Get(hq);
        ProductionQueueItem item = production.Get(0); item.RemainingTicks = 1; production.Set(0, item);

        new SimulationRunner(world).StepOneTick();

        OperationsCapacityState completed = world.GetOperationsCapacity(0);
        Assert.Multiple(() =>
        {
            Assert.That(queued.Active, Is.EqualTo(6));
            Assert.That(queued.Reserved, Is.EqualTo(1));
            Assert.That(completed.Active, Is.EqualTo(7));
            Assert.That(completed.Reserved, Is.Zero);
            Assert.That(completed.Maximum, Is.EqualTo(16));
        });
    }

    [Test]
    public void RemovingAQueuedItemReleasesItsReservedCapacity()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId hq = FirstProductionFacility(world, 0);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        Assert.That(world.GetOperationsCapacity(0).Reserved, Is.EqualTo(1));

        world.Entities.Production.Get(hq).RemoveFirst();
        OperationsCapacitySystem.Recalculate(world);

        Assert.That(world.GetOperationsCapacity(0).Reserved, Is.Zero);
    }

    [Test]
    public void CapacityProvidersClampAtTheCompetitiveMaximumOfOneHundred()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        for (int i = 0; i < 30; i++) AddCompletedServiceBay(world, 0);

        OperationsCapacitySystem.Recalculate(world);

        Assert.That(world.GetOperationsCapacity(0).Maximum, Is.EqualTo(OperationsCapacitySystem.CompetitiveMaximum));
    }

    [Test]
    public void ProviderLossCreatesOverCapacityWithoutDeletingExistingUnits()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId serviceBay = AddCompletedServiceBay(world, 0);
        for (int i = 0; i < 12; i++) AddCapacityOnlyUnit(world, 0, Crew);
        OperationsCapacitySystem.Recalculate(world);
        int aliveBefore = world.Entities.Alive.Count;
        Assert.That(world.GetOperationsCapacity(0).Active, Is.EqualTo(18));
        Assert.That(world.GetOperationsCapacity(0).Maximum, Is.EqualTo(20));

        Assert.That(world.Entities.Destroy(serviceBay), Is.True);
        OperationsCapacitySystem.Recalculate(world);

        OperationsCapacityState over = world.GetOperationsCapacity(0);
        Assert.Multiple(() =>
        {
            Assert.That(over.Active, Is.EqualTo(18));
            Assert.That(over.Maximum, Is.EqualTo(16));
            Assert.That(over.IsOverCapacity, Is.True);
            Assert.That(world.Entities.Alive.Count, Is.EqualTo(aliveBefore - 1), "Only the destroyed provider is removed.");
        });
    }

    [Test]
    public void SnapshotRederivesCapacityFromUnitsBuildingsAndQueue()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId hq = FirstProductionFacility(world, 0);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, Crew), Is.True);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));

        OperationsCapacityState capacity = restored.GetOperationsCapacity(0);
        Assert.Multiple(() =>
        {
            Assert.That(capacity.Active, Is.EqualTo(6));
            Assert.That(capacity.Reserved, Is.EqualTo(1));
            Assert.That(capacity.Maximum, Is.EqualTo(16));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    private static PrototypeEntityDefinition Entity(PrototypeContentCatalog content, string key)
    {
        Assert.That(content.TryGetEntity(key, out PrototypeEntityDefinition definition), Is.True, key);
        return definition;
    }

    private static BuildingDefinition Building(PrototypeContentCatalog content, string key)
    {
        Assert.That(content.TryGetBuilding(key, out BuildingDefinition definition), Is.True, key);
        return definition;
    }

    private static EntityId FirstProductionFacility(SimulationWorld world, byte player)
        => world.Entities.Alive.First(id => world.Entities.Production.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == player);

    private static EntityId AddCompletedServiceBay(SimulationWorld world, byte player)
    {
        BuildingDefinition definition = Building(world.Content, "building.rock_raiders.vehicle_service_bay");
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = FixVec2.FromInts(34, 94), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = ServiceBay, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building
        {
            Type = ServiceBay, AnchorX = 30, AnchorY = 91, Orientation = 0,
            FootprintWidth = definition.FootprintWidth, FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed
        });
        world.Entities.Production.Set(id, new Production());
        OperationsCapacitySystem.Recalculate(world);
        return id;
    }

    private static void AddCapacityOnlyUnit(SimulationWorld world, byte player, ContentId type)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Worker });
    }
}
}
