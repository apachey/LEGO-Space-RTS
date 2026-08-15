using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3EnergyDomainTests
{
    private static readonly ContentId ProcessingPlant = StableId.FromKey("building.rock_raiders.ore_processing_plant");
    private static readonly ContentId PowerStation = StableId.FromKey("building.rock_raiders.power_station");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");
    private static readonly ContentId HoverScout = StableId.FromKey("unit.rock_raiders.hover_scout");

    [Test]
    public void RockRaiderBuildingsExposeCanonicalEnergyMetadata()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        BuildingDefinition hq = Building(content, "building.rock_raiders.hq");
        BuildingDefinition processing = Building(content, "building.rock_raiders.ore_processing_plant");
        BuildingDefinition power = Building(content, "building.rock_raiders.power_station");
        BuildingDefinition bay = Building(content, "building.rock_raiders.vehicle_service_bay");

        Assert.Multiple(() =>
        {
            Assert.That(hq.EnergyGenerationPerSecond, Is.EqualTo(2));
            Assert.That(hq.EnergyReserveCapacity, Is.EqualTo(150));
            Assert.That(processing.ContinuousEnergyDemandPerSecond, Is.EqualTo(1));
            Assert.That(power.EnergyGenerationPerSecond, Is.EqualTo(10));
            Assert.That(power.EnergyReserveCapacity, Is.EqualTo(120));
            Assert.That(bay.ContinuousEnergyDemandPerSecond, Is.EqualTo(1));
        });
    }

    [Test]
    public void CanonicalOpeningStartsAtOneHundredTwentyOfOneHundredFiftyAndChargesAtTwoPerSecond()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EnergyDomain before = PlayerDomain(world, 0, out _);

        new SimulationRunner(world).StepTicks(20);

        EnergyDomain after = PlayerDomain(world, 0, out _);
        Assert.Multiple(() =>
        {
            Assert.That(before.Reserve, Is.EqualTo(Fix32.FromInt(120)));
            Assert.That(before.ReserveCapacity, Is.EqualTo(Fix32.FromInt(150)));
            Assert.That(before.GenerationPerSecond, Is.EqualTo(2));
            Assert.That(before.ContinuousDemandPerSecond, Is.Zero);
            Assert.That(after.Reserve, Is.EqualTo(Fix32.FromInt(122)));
        });
    }

    [Test]
    public void ConstructionReservesEnergyAndUnstartedCancellationRefundsItInFull()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        PlayerDomain(world, 0, out EntityId root);

        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId site, out PlacementFailure failure), Is.True, failure.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.EnergyDomain.Get(root).Reserve, Is.EqualTo(Fix32.FromInt(105)));
            Assert.That(world.Entities.ConstructionSite.Get(site).ReservedEnergy, Is.EqualTo(15));
            Assert.That(world.Entities.EnergyDomainMember.Get(site).DomainRoot, Is.EqualTo(root));
        });

        Assert.That(ConstructionPlacement.TryCancelUnstarted(world, 0, site), Is.True);
        Assert.That(world.Entities.EnergyDomain.Get(root).Reserve, Is.EqualTo(Fix32.FromInt(120)));
    }

    [Test]
    public void CompletedPowerStationExpandsTheSameDomainAndImmediatelyChangesFlow()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        FixVec2 builderPosition = FixVec2.FromInts(5, 84);
        world.Entities.Transform.Get(worker).Position = builderPosition;
        world.Entities.Movement.Get(worker).LastPosition = builderPosition;
        PlayerDomain(world, 0, out EntityId root);
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, PowerStation, 6, 82, 0, out EntityId site, out PlacementFailure failure), Is.True, failure.ToString());
        world.Entities.ConstructionSite.Get(site).RequiredTicks = 1;

        new SimulationRunner(world).StepOneTick();

        EnergyDomain domain = world.Entities.EnergyDomain.Get(root);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Building.Get(site).State, Is.EqualTo(BuildingState.Completed));
            Assert.That(domain.ReserveCapacity, Is.EqualTo(Fix32.FromInt(270)));
            Assert.That(domain.GenerationPerSecond, Is.EqualTo(12));
            Assert.That(domain.ContinuousDemandPerSecond, Is.Zero);
            Assert.That(domain.Reserve, Is.GreaterThan(Fix32.FromInt(100)), "The 20-Energy construction cost is paid before the new +12 E/s begins charging.");
        });
    }

    [Test]
    public void DeficitDrainsReserveDeterministicallyIntoBrownout()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        PlayerDomain(world, 0, out EntityId root);
        AddCompletedBuilding(world, 0, ProcessingPlant, root, 20, 20);
        AddCompletedBuilding(world, 0, ProcessingPlant, root, 30, 20);
        AddCompletedBuilding(world, 0, ProcessingPlant, root, 40, 20);
        EnergyDomainSystem.Recalculate(world, root);
        world.Entities.EnergyDomain.Get(root).Reserve = Fix32.FromInt(1);

        new SimulationRunner(world).StepTicks(20);

        EnergyDomain domain = world.Entities.EnergyDomain.Get(root);
        Assert.Multiple(() =>
        {
            Assert.That(domain.GenerationPerSecond, Is.EqualTo(2));
            Assert.That(domain.ContinuousDemandPerSecond, Is.EqualTo(3));
            Assert.That(domain.Reserve, Is.EqualTo(Fix32.Zero));
            Assert.That(domain.IsDeficit, Is.True);
            Assert.That(domain.IsBrownout, Is.True);
        });
    }

    [Test]
    public void ProductionRejectsInsufficientEnergyBeforeSpendingOreAndChargesAcceptedQueue()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        PlayerDomain(world, 0, out EntityId root);
        EntityId bank = world.Entities.Alive.Single(id => world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.Get(id).PlayerSlot == 0);
        EntityId bay = AddCompletedBuilding(world, 0, ServiceBay, root, 30, 90);
        world.Entities.Production.Set(bay, new Production());
        world.Entities.EnergyDomain.Get(root).Reserve = Fix32.FromInt(9);
        int oreBefore = world.Entities.ResourceBank.Get(bank).ProcessedAmount;

        Assert.That(ProductionSystem.TryQueue(world, 0, bay, HoverScout), Is.False);
        Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.EqualTo(oreBefore));
        world.Entities.EnergyDomain.Get(root).Reserve = Fix32.FromInt(10);
        Assert.That(ProductionSystem.TryQueue(world, 0, bay, HoverScout), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.EnergyDomain.Get(root).Reserve, Is.EqualTo(Fix32.Zero));
            Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.EqualTo(oreBefore - 75));
        });
    }

    [Test]
    public void SnapshotPreservesDomainReserveMembershipAndDeterministicContinuation()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        PlayerDomain(world, 0, out EntityId root);
        AddCompletedBuilding(world, 0, ProcessingPlant, root, 20, 20);
        EnergyDomainSystem.Recalculate(world, root);
        new SimulationRunner(world).StepTicks(7);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        Assert.That(restored.Entities.EnergyDomain.Get(root).Reserve, Is.EqualTo(world.Entities.EnergyDomain.Get(root).Reserve));
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(40); restoredRunner.StepTicks(40);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static EnergyDomain PlayerDomain(SimulationWorld world, byte player, out EntityId root)
    {
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, player, out root), Is.True);
        return world.Entities.EnergyDomain.Get(root);
    }

    private static BuildingDefinition Building(PrototypeContentCatalog content, string key)
    {
        Assert.That(content.TryGetBuilding(key, out BuildingDefinition definition), Is.True, key);
        return definition;
    }

    private static EntityId AddCompletedBuilding(SimulationWorld world, byte player, ContentId type, EntityId root, short anchorX, short anchorY)
    {
        BuildingDefinition definition = world.Content.Buildings.Single(x => x.Id == type);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = new FixVec2(Fix32.FromRatio(anchorX * 2 + definition.FootprintWidth, 2), Fix32.FromRatio(anchorY * 2 + definition.FootprintHeight, 2)), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, AnchorX = anchorX, AnchorY = anchorY, FootprintWidth = definition.FootprintWidth, FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed });
        world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = root });
        world.Entities.WorksiteMember.Set(id, new WorksiteMember { ComponentRoot = root });
        return id;
    }
}
}
