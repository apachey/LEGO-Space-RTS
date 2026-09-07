using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M8CommandCatalogTests
{
    [Test]
    public void ProductionCatalogPreservesAllThirtyFivePhase04Recipes()
    {
        // Phase 04, Part IX: costs are O/E/C, followed by OC and 20-Hz ticks.
        (string Unit, string Producers, int Ore, int Energy, int Crystals, int Oc, int Ticks)[] expected =
        {
            ("unit.rock_raiders.crew", "building.rock_raiders.hq", 50, 0, 0, 1, 320),
            ("unit.rock_raiders.hover_scout", "building.rock_raiders.vehicle_service_bay", 75, 10, 0, 1, 400),
            ("unit.rock_raiders.rapid_rider", "building.rock_raiders.vehicle_service_bay", 90, 10, 0, 2, 560),
            ("unit.rock_raiders.loader_dozer", "building.rock_raiders.vehicle_service_bay", 125, 15, 0, 3, 720),
            ("unit.rock_raiders.drill_craft", "building.rock_raiders.engineering_workshop", 95, 15, 0, 2, 560),
            ("unit.rock_raiders.granite_grinder", "building.rock_raiders.engineering_workshop", 190, 35, 1, 4, 1000),
            ("unit.rock_raiders.chrome_crusher", "building.rock_raiders.engineering_workshop", 330, 90, 4, 6, 1560),
            ("unit.rock_raiders.tunnel_transport", "building.rock_raiders.engineering_workshop", 300, 110, 3, 5, 1500),
            ("unit.astronauts.expedition_crew", "building.ast.mb01_eagle_command_base", 50, 0, 0, 1, 320),
            ("unit.astronauts.rover", "building.ast.field_systems_garage", 70, 5, 0, 1, 360),
            ("unit.astronauts.t3_trike", "building.ast.field_systems_garage", 110, 15, 0, 2, 600),
            ("unit.astronauts.mono_jet", "building.ast.mb01_eagle_command_base", 100, 35, 0, 1, 640),
            ("unit.astronauts.solar_explorer", "building.ast.field_systems_garage", 180, 40, 1, 4, 1000),
            ("unit.astronauts.mission_fighter", "building.ast.flight_operations_pad", 140, 55, 1, 2, 800),
            ("unit.astronauts.mx41_switch_fighter", "building.ast.mission_vehicle_bay", 170, 60, 1, 3, 900),
            ("unit.astronauts.mobile_mining_platform", "building.ast.mission_vehicle_bay", 170, 25, 0, 3, 960),
            ("unit.astronauts.mx71_recon_dropship", "building.ast.flight_operations_pad", 200, 70, 1, 4, 1100),
            ("unit.astronauts.mt51_claw_tank", "building.ast.mission_vehicle_bay", 180, 35, 0, 3, 900),
            ("unit.astronauts.mt101_armored_drilling_unit", "building.ast.mission_vehicle_bay", 280, 70, 2, 5, 1300),
            ("unit.astronauts.mt201_ultra_drill_walker", "building.ast.mission_vehicle_bay", 320, 90, 3, 6, 1500),
            ("unit.astronauts.mx81_operations_aircraft", "building.ast.flight_operations_pad", 380, 130, 4, 6, 1800),
            ("unit.aliens.etx_servitor", "building.ali.etx_command_core,building.ali.etx_fabricator", 50, 5, 0, 1, 320),
            ("unit.aliens.alien_jet", "building.ali.etx_fabricator", 95, 35, 0, 2, 560),
            ("unit.aliens.razor_skimmer", "building.ali.etx_fabricator", 105, 25, 0, 2, 600),
            ("unit.aliens.etx_alien_strike", "building.ali.reconfiguration_dock", 190, 60, 1, 4, 1000),
            ("unit.aliens.etx_alien_infiltrator", "building.ali.reconfiguration_dock", 210, 65, 2, 4, 1100),
            ("unit.aliens.alien_mothership", "building.ali.reconfiguration_dock", 420, 160, 6, 8, 1900),
            ("unit.martians.worker_robot", "building.mar.aero_tube_hangar,building.mar.settlement_station", 50, 0, 0, 1, 320),
            ("unit.martians.double_hover", "building.mar.mechanical_workshop", 65, 10, 0, 1, 360),
            ("unit.martians.jet_scooter", "building.mar.mechanical_workshop", 90, 15, 0, 1, 500),
            ("unit.martians.aero_skiff", "building.mar.aero_tube_hangar", 120, 40, 0, 2, 700),
            ("unit.martians.red_planet_cruiser", "building.mar.mechanical_workshop", 150, 25, 0, 3, 800),
            ("unit.martians.recon_mech_rp", "building.mar.mechanical_workshop", 170, 35, 1, 3, 900),
            ("unit.martians.red_planet_protector", "building.mar.mechanical_workshop", 220, 50, 2, 4, 1100),
            ("unit.martians.excavation_searcher", "building.mar.mechanical_workshop", 330, 80, 3, 6, 1560)
        };
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        Assert.That(catalog.Production.Select(p => p.UnitStableKey), Is.EquivalentTo(expected.Select(row => row.Unit)));
        foreach (var row in expected)
        {
            UnitProductionDefinition recipe = catalog.Production.Single(p => p.UnitStableKey == row.Unit);
            Assert.Multiple(() =>
            {
                Assert.That(recipe.ProducerStableKeys, Is.EqualTo(row.Producers.Split(',')), row.Unit);
                Assert.That((recipe.OreCost, recipe.EnergyCost, recipe.CrystalCost, recipe.OperationsCapacity, recipe.BuildTicks),
                    Is.EqualTo(((ushort)row.Ore, (ushort)row.Energy, (byte)row.Crystals, (byte)row.Oc, (ushort)row.Ticks)), row.Unit);
                Assert.That(recipe.ProducerTypes.All(recipe.CanProduceAt), Is.True, row.Unit);
                Assert.That(recipe.CanProduceAt(StableId.FromKey("building.nonexistent")), Is.False, row.Unit);
            });
        }
    }

    [Test]
    public void ActionPrerequisitesPreserveAndOfOrSemantics()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        UnitProductionDefinition searcher = catalog.Production.Single(p => p.UnitStableKey == "unit.martians.excavation_searcher");
        UnitProductionDefinition mono = catalog.Production.Single(p => p.UnitStableKey == "unit.astronauts.mono_jet");
        BuildingDefinition hub = catalog.Buildings.Single(b => b.StableKey == "building.ast.service_refit_hub");
        BuildingDefinition workshop = catalog.Buildings.Single(b => b.StableKey == "building.mar.mechanical_workshop");
        Assert.Multiple(() =>
        {
            Assert.That(Requirements(searcher.PrerequisiteGroups), Is.EqualTo(new[]
            {
                "Research:research.mar.grand_network_integration",
                "Research:research.mar.advanced_excavation_systems"
            }));
            Assert.That(Requirements(mono.PrerequisiteGroups), Is.EqualTo(new[] { "Building:building.ast.solar_energy_array" }));
            Assert.That(Requirements(hub.PrerequisiteGroups), Is.EqualTo(new[]
            {
                "Building:building.ast.field_systems_garage",
                "Building:building.ast.solar_energy_array|Building:building.ast.frontier_extraction_station"
            }));
            Assert.That(Requirements(workshop.PrerequisiteGroups), Is.EqualTo(new[]
            {
                "Building:building.mar.aero_tube_hangar|Building:building.mar.settlement_station"
            }));
        });
    }

    [Test]
    public void PublicCommandCatalogIsCompleteOrderedAndHasCanonicalEligibility()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        Assert.Multiple(() =>
        {
            Assert.That(catalog.Commands.Select(c => (int)c.CommandType), Is.EquivalentTo(Enumerable.Range(1, 35)));
            Assert.That(catalog.Commands.Select(c => c.StableKey),
                Is.EqualTo(catalog.Commands.Select(c => c.StableKey).OrderBy(key => key, StringComparer.Ordinal)));
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.AttackMove).EligibleEntityTags,
                Does.Contain("component.navigation"), "Support units move with an Attack-Move group (Phase 07 command matrix).");
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.Ping).EligibleEntityTags,
                Is.EqualTo(new[] { "tag.player" }), "A ping does not require an entity selection.");
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.Repair).EligibleEntityTags,
                Is.EqualTo(new[] { "component.repairer" }));
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.ProtectorStance).RequiredResearch.Value,
                Is.Zero, "Utility Mechanisms upgrades Protector Stance effects; it does not unlock the base stance.");
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.StateChange).EligibleEntityTags,
                Does.Contain("building.ali.etx_defense_node"));
            Assert.That(catalog.Commands.Any(c => c.CommandType == SimCommandType.ReorderProduction), Is.True);
            Assert.That(catalog.Commands.Any(c => c.CommandType == SimCommandType.CancelMissionRefit), Is.True);
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.Unload).QueuePolicy, Is.EqualTo(CommandQueuePolicy.Conditional));
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.TubeTransfer).QueuePolicy, Is.EqualTo(CommandQueuePolicy.Conditional));
            Assert.That(catalog.Commands.Single(c => c.CommandType == SimCommandType.StateChange).QueuePolicy, Is.EqualTo(CommandQueuePolicy.Conditional));
        });
    }

    [Test]
    public void ReservedCatalogCommandsCannotEnterTheAcceptedCommandWireFormat()
    {
        foreach (SimCommandType type in Enum.GetValues<SimCommandType>().Where(type =>
                     type >= SimCommandType.AttackMove && type <= SimCommandType.CancelMissionRefit))
            Assert.Throws<ArgumentOutOfRangeException>(() => new CommandEnvelope(
                new SimTick(1), 0, 1, type, Array.Empty<EntityId>(), FixVec2.Zero), type.ToString());
    }

    [Test]
    public void CompleteProductionMetadataDoesNotPrematurelyEnableUnboundRecipes()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        Assert.Multiple(() =>
        {
            Assert.That(ProductionSystem.IsRuntimeEnabledUnit(StableId.FromKey("unit.rock_raiders.crew")), Is.True);
            Assert.That(ProductionSystem.IsRuntimeEnabledUnit(StableId.FromKey("unit.astronauts.rover")), Is.False);
            Assert.That(ProductionSystem.IsRuntimeEnabledProducer(catalog, StableId.FromKey("building.rock_raiders.hq")), Is.True);
            Assert.That(ProductionSystem.IsRuntimeEnabledProducer(catalog, StableId.FromKey("building.ast.field_systems_garage")), Is.False);
        });
    }

    [Test]
    public void CatalogRoundTripPreservesEveryCommandAndActionBinding()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        byte[] bytes = PrototypeContentCodec.Write(source);
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(bytes);
        Assert.That(PrototypeContentCodec.Write(restored), Is.EqualTo(bytes));
        Assert.That(restored.ContentHash, Is.EqualTo(source.ContentHash));
        for (int i = 0; i < source.Commands.Length; i++)
        {
            CommandDefinition expected = source.Commands[i], actual = restored.Commands[i];
            Assert.Multiple(() =>
            {
                Assert.That(actual.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.CommandType, Is.EqualTo(expected.CommandType));
                Assert.That(actual.EligibleEntityTags, Is.EqualTo(expected.EligibleEntityTags));
                Assert.That(actual.RequiredResearch, Is.EqualTo(expected.RequiredResearch));
                Assert.That(actual.TargetType, Is.EqualTo(expected.TargetType));
                Assert.That(actual.QueuePolicy, Is.EqualTo(expected.QueuePolicy));
                Assert.That(actual.ValidationHandlerId, Is.EqualTo(expected.ValidationHandlerId));
                Assert.That(actual.ExecutionHandlerId, Is.EqualTo(expected.ExecutionHandlerId));
                Assert.That(actual.UiSlotProfile, Is.EqualTo(expected.UiSlotProfile));
                Assert.That(actual.TargetingPreviewProfile, Is.EqualTo(expected.TargetingPreviewProfile));
            });
        }
        for (int i = 0; i < source.Buildings.Length; i++)
            Assert.That(Requirements(restored.Buildings[i].PrerequisiteGroups), Is.EqualTo(Requirements(source.Buildings[i].PrerequisiteGroups)));
        for (int i = 0; i < source.Production.Length; i++)
        {
            Assert.That(restored.Production[i].ProducerStableKeys, Is.EqualTo(source.Production[i].ProducerStableKeys));
            Assert.That(Requirements(restored.Production[i].PrerequisiteGroups), Is.EqualTo(Requirements(source.Production[i].PrerequisiteGroups)));
        }
    }

    [Test]
    public void UndefinedCommandCodeIsRejectedEvenInsideThePublicNumericRange()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        Assert.Throws<ArgumentException>(() =>
        {
            CommandDefinition[] commands = (CommandDefinition[])source.Commands.Clone();
            commands[0] = CopyCommand(commands[0], commandType: (SimCommandType)999);
            CopyCatalog(source, commands: commands);
        });
    }

    [Test]
    public void MissingOrDuplicatePublicCommandsAreRejected()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        Assert.Throws<ArgumentException>(() => CopyCatalog(source, commands: source.Commands.Skip(1).ToArray()));
        Assert.Throws<ArgumentException>(() => CopyCatalog(source, commands: source.Commands.Append(source.Commands[0]).ToArray()));
        Assert.Throws<ArgumentException>(() => CopyCatalog(source,
            commands: source.Commands.Append(CopyCommand(source.Commands[0], stableKey: "command.test.duplicate_network_code")).ToArray()));
        Assert.Throws<ArgumentException>(() => CopyCatalog(source,
            commands: source.Commands.Append(CopyCommand(source.Commands[0], commandType: source.Commands[1].CommandType)).ToArray()));
    }

    [Test]
    public void ProductionRejectsProducerAndRequirementFromAnotherFaction()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        UnitProductionDefinition original = source.Production.Single(p => p.UnitStableKey == "unit.aliens.etx_servitor");
        UnitProductionDefinition foreignProducer = new(original.UnitStableKey, new[] { "building.rock_raiders.hq" },
            original.OreCost, original.EnergyCost, original.CrystalCost, original.OperationsCapacity, original.BuildTicks, original.PrerequisiteGroups);
        UnitProductionDefinition foreignResearch = new(original.UnitStableKey, original.ProducerStableKeys,
            original.OreCost, original.EnergyCost, original.CrystalCost, original.OperationsCapacity, original.BuildTicks,
            new[] { new ContentActionPrerequisiteGroup(new[] { new ContentActionPrerequisite(ContentActionPrerequisiteKind.Research, "research.rr.industrial_expansion_program") }) });
        Assert.Throws<ArgumentException>(() => CopyCatalog(source,
            production: source.Production.Select(p => p.UnitType == original.UnitType ? foreignProducer : p).ToArray()));
        Assert.Throws<ArgumentException>(() => CopyCatalog(source,
            production: source.Production.Select(p => p.UnitType == original.UnitType ? foreignResearch : p).ToArray()));
    }

    [Test]
    public void ConstructionRejectsBuildingRequirementFromAnotherFaction()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        BuildingDefinition original = source.Buildings.Single(b => b.StableKey == "building.ali.etx_fabricator");
        BuildingDefinition modified = new(original.StableKey, original.FootprintWidth, original.FootprintHeight, original.FootprintMask,
            original.Rotatable, original.OreCost, original.EnergyCost, original.BuildTicks, original.ProductionExitWidth, original.ProductionExitDepth,
            original.ProductionExitFootprint, original.OperationsCapacityProvided, original.EnergyGenerationPerSecond, original.EnergyReserveCapacity,
            original.ContinuousEnergyDemandPerSecond, original.EnergyFunctionalClass, original.WorksiteServiceRadius, original.CrystalCost,
            original.FootprintMaskHigh, new[] { new ContentActionPrerequisiteGroup(new[]
            {
                new ContentActionPrerequisite(ContentActionPrerequisiteKind.Building, "building.rock_raiders.hq")
            }) });
        Assert.Throws<ArgumentException>(() => CopyCatalog(source,
            buildings: source.Buildings.Select(b => b.Id == original.Id ? modified : b).ToArray()));
    }

    private static string[] Requirements(ContentActionPrerequisiteGroup[] groups)
        => groups.Select(group => string.Join("|", group.Alternatives.Select(requirement => $"{requirement.Kind}:{requirement.TargetStableKey}"))).ToArray();

    private static PrototypeContentCatalog CopyCatalog(PrototypeContentCatalog source, CommandDefinition[]? commands = null,
        UnitProductionDefinition[]? production = null, BuildingDefinition[]? buildings = null)
        => new(source.MovementProfiles, source.Entities, source.ResourceNodes, buildings ?? source.Buildings,
            production ?? source.Production, source.Weapons, source.Transformations, source.Research, commands ?? source.Commands);

    private static CommandDefinition CopyCommand(CommandDefinition source, string? stableKey = null, SimCommandType? commandType = null)
        => new(stableKey ?? source.StableKey, commandType ?? source.CommandType, source.EligibleEntityTags, source.TargetType,
            source.QueuePolicy, source.RequiredResearchStableKey, source.ValidationHandlerId, source.ExecutionHandlerId,
            source.UiSlotProfile, source.TargetingPreviewProfile);
}
}
