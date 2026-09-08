using System;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3ConstructionPlacementTests
{
    private static readonly ContentId ProcessingPlant = StableId.FromKey("building.rock_raiders.ore_processing_plant");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");

    [Test]
    public void PrototypeUsesCanonicalBuildingFootprintsCostsAndTimes()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        AssertBuilding(content, "building.rock_raiders.hq", 8, 8, 320, 40, 1200);
        AssertBuilding(content, "building.rock_raiders.ore_processing_plant", 6, 6, 140, 15, 600);
        AssertBuilding(content, "building.rock_raiders.power_station", 5, 5, 150, 20, 700);
        AssertBuilding(content, "building.rock_raiders.vehicle_service_bay", 8, 6, 160, 20, 800);
    }

    [Test]
    public void T073CatalogExposesAllAuthoredBuildCommands()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        string[] available = world.Content.Buildings
            .Where(building => ConstructionPlacement.IsBuildCommandAvailable(building.Id))
            .Select(building => building.StableKey)
            .ToArray();
        ContentId crystalBuilding = StableId.FromKey("building.ast.mission_vehicle_bay");

        Assert.That(world.Content.TryGetBuilding(crystalBuilding, out BuildingDefinition definition), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(available, Is.EqualTo(world.Content.Buildings.Select(building => building.StableKey)));
            Assert.That(definition.CrystalCost, Is.EqualTo(1));
            Assert.That(ConstructionPlacement.Validate(world, 0, Array.Empty<EntityId>(), crystalBuilding, 0, 0, 0).Failure,
                Is.EqualTo(PlacementFailure.NoEligibleBuilder));
        });
    }

    [Test]
    public void ServerValidationRejectsMissingBuilderResourcesOccupancyAndTerrain()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId bank = FirstOwnedBank(world, 0);

        Assert.That(ConstructionPlacement.Validate(world, 0, Array.Empty<EntityId>(), ProcessingPlant, 6, 82, 0).Failure, Is.EqualTo(PlacementFailure.NoEligibleBuilder));
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 0;
        Assert.That(ConstructionPlacement.Validate(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0).Failure, Is.EqualTo(PlacementFailure.InsufficientOre));
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 500;
        Assert.That(ConstructionPlacement.Validate(world, 0, new[] { worker }, ProcessingPlant, 6, 70, 0).Failure, Is.EqualTo(PlacementFailure.FootprintOccupied));
        Assert.That(ConstructionPlacement.Validate(world, 0, new[] { worker }, ProcessingPlant, 45, 25, 0).Failure, Is.EqualTo(PlacementFailure.NonBuildableTerrain));
        Assert.That(ConstructionPlacement.Validate(world, 1, new[] { worker }, ProcessingPlant, 130, 82, 0).Failure, Is.EqualTo(PlacementFailure.NoEligibleBuilder));
    }

    [Test]
    public void BuildCommandReservesOreCreatesSameIdentitySiteAndBlocksFinalFootprint()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId bank = FirstOwnedBank(world, 0);
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 200;
        ResourceConservationTotals before = ResourceConservation.Measure(world, ResourceType.Ore);
        int topologyBefore = world.Map.TopologyVersion;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Build, new[] { worker }, FixVec2.FromInts(6, 82), contentType: ProcessingPlant));

        new SimulationRunner(world).StepOneTick();

        EntityId siteId = world.Entities.Alive.Single(id => world.Entities.ConstructionSite.Has(id));
        Building building = world.Entities.Building.Get(siteId);
        ConstructionSite site = world.Entities.ConstructionSite.Get(siteId);
        Assert.That(building.Type, Is.EqualTo(ProcessingPlant));
        Assert.That(building.State, Is.EqualTo(BuildingState.ConstructionSite));
        Assert.That(building.AnchorX, Is.EqualTo(6));
        Assert.That(building.AnchorY, Is.EqualTo(82));
        Assert.That(site.AssignedBuilder, Is.EqualTo(worker));
        Assert.That(site.FundingBank, Is.EqualTo(bank));
        Assert.That(site.ReservedOre, Is.EqualTo(140));
        Assert.That(site.RequiredEnergy, Is.EqualTo(15));
        Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.EqualTo(60));
        Assert.That(ResourceConservation.Measure(world, ResourceType.Ore).Total, Is.EqualTo(before.Total));
        Assert.That(world.Map.TopologyVersion, Is.EqualTo(topologyBefore + 1));
        Assert.That(world.Map.GetFlags(12, 164).HasFlag(MapCellFlags.Impassable), Is.True);
        Assert.That(world.Entities.Selectable.Get(siteId).ContentType, Is.EqualTo(building.Type), "The construction site retains the final building identity.");
    }

    [Test]
    public void UnstartedCancellationRefundsFullReservationAndReleasesFootprint()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId bank = FirstOwnedBank(world, 0);
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 200;
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId site, out PlacementFailure failure), Is.True, failure.ToString());
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 2, SimCommandType.CancelConstruction, Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: site));

        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Entities.Exists(site), Is.False);
        Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.EqualTo(200));
        Assert.That(world.Map.GetFlags(12, 164).HasFlag(MapCellFlags.Buildable), Is.True);
        Assert.That(world.Map.GetFlags(12, 164).HasFlag(MapCellFlags.Impassable), Is.False);
    }

    [Test]
    public void ProductionBuildingRequiresItsAuthoredExitToRemainLegal()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        world.Entities.ResourceBank.Get(FirstOwnedBank(world, 0)).ProcessedAmount = 500;
        BuildingDefinition definition = world.Content.Buildings.Single(x => x.Id == ServiceBay);
        IntRect exit = ConstructionPlacement.GetProductionExit(definition, 30, 90, 0);
        world.Map.SetFlagsRect(new IntRect((short)(exit.X * 2), (short)(exit.Y * 2), (short)(exit.Width * 2), (short)(exit.Height * 2)), MapCellFlags.Impassable, MapCellFlags.Buildable);

        PlacementValidation validation = ConstructionPlacement.Validate(world, 0, new[] { worker }, ServiceBay, 30, 90, 0);

        Assert.That(validation.Failure, Is.EqualTo(PlacementFailure.NoLegalProductionExit));
    }

    [Test]
    public void ConstructionSiteSnapshotContinuesWithReservationAndTopologyIntact()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        world.Entities.ResourceBank.Get(FirstOwnedBank(world, 0)).ProcessedAmount = 200;
        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, ProcessingPlant, 6, 82, 0, out EntityId site, out _), Is.True);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));

        Assert.That(restored.Entities.Building.Get(site).Type, Is.EqualTo(ProcessingPlant));
        Assert.That(restored.Entities.ConstructionSite.Get(site).ReservedOre, Is.EqualTo(140));
        Assert.That(restored.Map.GetFlags(12, 164).HasFlag(MapCellFlags.Impassable), Is.True);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(40); restoredRunner.StepTicks(40);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    [Test]
    public void PendingBuildCommandPreservesBuildingTypeAndRotationAcrossSnapshot()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        world.Entities.ResourceBank.Get(FirstOwnedBank(world, 0)).ProcessedAmount = 300;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(5), 0, 7, SimCommandType.Build, new[] { worker }, FixVec2.FromInts(30, 90), contentType: ServiceBay, orientation: 1));

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));

        Assert.That(restored.Commands.All[0].ContentType, Is.EqualTo(ServiceBay));
        Assert.That(restored.Commands.All[0].Orientation, Is.EqualTo(1));
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(5); restoredRunner.StepTicks(5);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        Assert.That(restored.Entities.Alive.Count(id => restored.Entities.ConstructionSite.Has(id)), Is.EqualTo(1));
    }

    private static void AssertBuilding(PrototypeContentCatalog content, string key, int width, int height, int ore, int energy, int ticks)
    {
        Assert.That(content.TryGetBuilding(key, out BuildingDefinition building), Is.True, key);
        Assert.Multiple(() =>
        {
            Assert.That(building.FootprintWidth, Is.EqualTo(width));
            Assert.That(building.FootprintHeight, Is.EqualTo(height));
            Assert.That(building.OreCost, Is.EqualTo(ore));
            Assert.That(building.EnergyCost, Is.EqualTo(energy));
            Assert.That(building.BuildTicks, Is.EqualTo(ticks));
        });
    }

    private static EntityId FirstOwnedBank(SimulationWorld world, byte player)
    {
        foreach (EntityId id in world.Entities.Alive)
            if (world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == player) return id;
        Assert.Fail("Scenario contains no owned resource bank.");
        return EntityId.None;
    }
}
}
