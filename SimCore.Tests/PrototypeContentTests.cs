using LegoSpaceRTS.SimCore;
using NUnit.Framework;
using System.IO;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class PrototypeContentTests
{
    [Test]
    public void BuiltInCatalogUsesCanonicalCompilerOrdering()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        AssertOrdered(catalog.MovementProfiles.Select(p => p.StableKey).ToArray());
        AssertOrdered(catalog.Entities.Select(e => e.StableKey).ToArray());
        AssertOrdered(catalog.ResourceNodes.Select(r => r.StableKey).ToArray());
        AssertOrdered(catalog.Buildings.Select(b => b.StableKey).ToArray());
        AssertOrdered(catalog.Production.Select(p => p.UnitStableKey).ToArray());
        AssertOrdered(catalog.Weapons.Select(w => w.StableKey).ToArray());
        AssertOrdered(catalog.Transformations.Select(t => t.StableKey).ToArray());
        AssertOrdered(catalog.Research.Select(r => r.StableKey).ToArray());
        AssertOrdered(catalog.Commands.Select(c => c.StableKey).ToArray());
    }

    [Test]
    public void M8T070CatalogContainsExactlyTheCanonicalThirtyFiveUnitRoster()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        PrototypeEntityDefinition[] units = catalog.Entities.Where(entity => entity.StableKey.StartsWith("unit.", System.StringComparison.Ordinal)).ToArray();
        string[] expectedKeys =
        {
            "unit.aliens.alien_jet",
            "unit.aliens.alien_mothership",
            "unit.aliens.etx_alien_infiltrator",
            "unit.aliens.etx_alien_strike",
            "unit.aliens.etx_servitor",
            "unit.aliens.razor_skimmer",
            "unit.astronauts.expedition_crew",
            "unit.astronauts.mission_fighter",
            "unit.astronauts.mobile_mining_platform",
            "unit.astronauts.mono_jet",
            "unit.astronauts.mt101_armored_drilling_unit",
            "unit.astronauts.mt201_ultra_drill_walker",
            "unit.astronauts.mt51_claw_tank",
            "unit.astronauts.mx41_switch_fighter",
            "unit.astronauts.mx71_recon_dropship",
            "unit.astronauts.mx81_operations_aircraft",
            "unit.astronauts.rover",
            "unit.astronauts.solar_explorer",
            "unit.astronauts.t3_trike",
            "unit.martians.aero_skiff",
            "unit.martians.double_hover",
            "unit.martians.excavation_searcher",
            "unit.martians.jet_scooter",
            "unit.martians.recon_mech_rp",
            "unit.martians.red_planet_cruiser",
            "unit.martians.red_planet_protector",
            "unit.martians.worker_robot",
            "unit.rock_raiders.chrome_crusher",
            "unit.rock_raiders.crew",
            "unit.rock_raiders.drill_craft",
            "unit.rock_raiders.granite_grinder",
            "unit.rock_raiders.hover_scout",
            "unit.rock_raiders.loader_dozer",
            "unit.rock_raiders.rapid_rider",
            "unit.rock_raiders.tunnel_transport"
        };

        Assert.Multiple(() =>
        {
            Assert.That(units.Select(unit => unit.StableKey), Is.EqualTo(expectedKeys));
            Assert.That(units.Count(unit => unit.FactionKey == "RockRaiders"), Is.EqualTo(8));
            Assert.That(units.Count(unit => unit.FactionKey == "Astronauts"), Is.EqualTo(13));
            Assert.That(units.Count(unit => unit.FactionKey == "Aliens"), Is.EqualTo(6));
            Assert.That(units.Count(unit => unit.FactionKey == "Martians"), Is.EqualTo(8));
            Assert.That(units, Has.All.Matches<PrototypeEntityDefinition>(unit => unit.OperationsCapacity > 0 && unit.Combat.IsTargetable));
        });
    }

    [Test]
    public void M8T070CanonicalChassisValuesSurviveBinaryRoundTrip()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(PrototypeContentFactory.CreateM2Catalog()));

        Assert.That(restored.TryGetEntity("unit.rock_raiders.tunnel_transport", out PrototypeEntityDefinition tunnel), Is.True);
        Assert.That(restored.TryGetEntity("unit.astronauts.mx81_operations_aircraft", out PrototypeEntityDefinition mx81), Is.True);
        Assert.That(restored.TryGetEntity("unit.aliens.alien_mothership", out PrototypeEntityDefinition mothership), Is.True);
        Assert.That(restored.TryGetEntity("unit.martians.excavation_searcher", out PrototypeEntityDefinition searcher), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(tunnel.Combat.MaximumHitPoints, Is.EqualTo(650));
            Assert.That(tunnel.OperationsCapacity, Is.EqualTo(5));
            Assert.That(mx81.VisionRadius, Is.EqualTo(15));
            Assert.That(mx81.Combat.TargetLayer, Is.EqualTo(CombatTargetLayer.TrueAir));
            Assert.That(mothership.Combat.MaximumHitPoints, Is.EqualTo(1200));
            Assert.That(mothership.OperationsCapacity, Is.EqualTo(8));
            Assert.That(searcher.Footprint, Is.EqualTo(FootprintClass.Huge));
            Assert.That(searcher.Combat.ArmorRating, Is.EqualTo(4));
        });
    }

    [Test]
    public void M8T071CatalogContainsExactlyTheCanonicalThirtyOneInfrastructureRoster()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        PrototypeEntityDefinition[] infrastructure = catalog.Entities.Where(entity => entity.SelectableKind == SelectableKind.Building).ToArray();
        string[] expectedKeys =
        {
            "building.ali.etx_command_core",
            "building.ali.etx_defense_node",
            "building.ali.etx_fabricator",
            "building.ali.power_coupler",
            "building.ali.reconfiguration_dock",
            "building.ali.resonance_core",
            "building.ast.field_systems_garage",
            "building.ast.flight_operations_pad",
            "building.ast.frontier_extraction_station",
            "building.ast.mb01_eagle_command_base",
            "building.ast.mission_vehicle_bay",
            "building.ast.modular_sentinel_defense",
            "building.ast.service_refit_hub",
            "building.ast.solar_energy_array",
            "building.mar.aero_guard_tower",
            "building.mar.aero_tube_hangar",
            "building.mar.aero_tube_link",
            "building.mar.deflector_arm",
            "building.mar.excavation_plant",
            "building.mar.mechanical_workshop",
            "building.mar.pressure_generator",
            "building.mar.routing_laboratory",
            "building.mar.settlement_station",
            "building.rock_raiders.crusher_barrier",
            "building.rock_raiders.crystal_vault",
            "building.rock_raiders.cutter_mast",
            "building.rock_raiders.engineering_workshop",
            "building.rock_raiders.hq",
            "building.rock_raiders.ore_processing_plant",
            "building.rock_raiders.power_station",
            "building.rock_raiders.vehicle_service_bay"
        };

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Buildings.Select(building => building.StableKey), Is.EqualTo(expectedKeys));
            Assert.That(infrastructure.Select(building => building.StableKey), Is.EqualTo(expectedKeys));
            Assert.That(infrastructure.Count(building => building.FactionKey == "RockRaiders"), Is.EqualTo(8));
            Assert.That(infrastructure.Count(building => building.FactionKey == "Astronauts"), Is.EqualTo(8));
            Assert.That(infrastructure.Count(building => building.FactionKey == "Aliens"), Is.EqualTo(6));
            Assert.That(infrastructure.Count(building => building.FactionKey == "Martians"), Is.EqualTo(9));
            Assert.That(infrastructure, Has.All.Matches<PrototypeEntityDefinition>(building => building.OperationsCapacity == 0 && building.Combat.IsTargetable));
            Assert.That(catalog.Buildings, Has.All.Matches<BuildingDefinition>(building => catalog.TryGetEntity(building.Id, out PrototypeEntityDefinition entity) && entity.SelectableKind == SelectableKind.Building));
        });
    }

    [Test]
    public void M8T071WideFootprintsAndCrystalCostsSurviveBinaryRoundTrip()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(PrototypeContentFactory.CreateM2Catalog()));

        Assert.That(restored.TryGetBuilding("building.ast.flight_operations_pad", out BuildingDefinition flightPad), Is.True);
        Assert.That(restored.TryGetBuilding("building.mar.aero_tube_hangar", out BuildingDefinition hangar), Is.True);
        Assert.That(restored.TryGetBuilding("building.ast.mission_vehicle_bay", out BuildingDefinition missionBay), Is.True);
        Assert.That(restored.TryGetBuilding("building.ali.reconfiguration_dock", out BuildingDefinition alienDock), Is.True);
        Assert.That(restored.TryGetBuilding("building.mar.routing_laboratory", out BuildingDefinition routingLab), Is.True);
        Assert.That(restored.TryGetBuilding("building.mar.aero_tube_link", out BuildingDefinition tubeLink), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That((flightPad.FootprintWidth, flightPad.FootprintHeight), Is.EqualTo((10, 8)));
            Assert.That(flightPad.FootprintMaskHigh, Is.EqualTo(0xFFFFUL));
            Assert.That(flightPad.Occupies(9, 7, 0), Is.True);
            Assert.That((flightPad.RotatedWidth(1), flightPad.RotatedHeight(1)), Is.EqualTo((8, 10)));
            Assert.That(flightPad.Occupies(7, 9, 1), Is.True);
            Assert.That((hangar.FootprintWidth, hangar.FootprintHeight), Is.EqualTo((9, 9)));
            Assert.That(hangar.FootprintMaskHigh, Is.EqualTo(0x1FFFFUL));
            Assert.That(hangar.Occupies(8, 8, 0), Is.True);
            Assert.That(flightPad.CrystalCost, Is.EqualTo(1));
            Assert.That(missionBay.CrystalCost, Is.EqualTo(1));
            Assert.That(alienDock.CrystalCost, Is.EqualTo(1));
            Assert.That(routingLab.CrystalCost, Is.EqualTo(1));
            Assert.That(tubeLink.OreCost, Is.EqualTo(TubeGraphSystem.LinkBaseOreCost));
            Assert.That(tubeLink.EnergyCost, Is.EqualTo(TubeGraphSystem.LinkActivationEnergyCost));
            Assert.That(tubeLink.BuildTicks, Is.EqualTo(10 * EnergyDomainSystem.TicksPerSecond));
        });
    }

    [Test]
    public void Format16BuildingDefinitionsRemainReadableWithLegacyDefaults()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(CreateFormat16BuildingCatalog());

        Assert.That(restored.TryGetBuilding("building.legacy", out BuildingDefinition building), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That((building.FootprintWidth, building.FootprintHeight), Is.EqualTo((3, 2)));
            Assert.That(building.FootprintMask, Is.EqualTo(0b11_1111UL));
            Assert.That(building.FootprintMaskHigh, Is.Zero);
            Assert.That(building.CrystalCost, Is.Zero);
            Assert.That(building.WorksiteServiceRadius, Is.EqualTo(12));
        });
    }

    [Test]
    public void M8T072CatalogContainsExactlyTheCanonicalThirtyEightResearchDefinitions()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        string[] expectedKeys =
        {
            "research.ali.advanced_resonance_architecture",
            "research.ali.defense_resonance_shunt",
            "research.ali.etx_reconfiguration_matrix",
            "research.ali.expanded_resonance_lattice",
            "research.ali.infiltration_matrix",
            "research.ali.mothership_resonance_relay",
            "research.ali.rapid_fabrication_conduits",
            "research.ali.resonance_initiation",
            "research.ali.resonant_recovery_latches",
            "research.ali.siege_phase_coupling",
            "research.ast.aerospace_coordination",
            "research.ast.deep_mission_drilling",
            "research.ast.field_survey_package",
            "research.ast.field_sustainment_package",
            "research.ast.heavy_mission_chassis",
            "research.ast.integrated_expedition_command",
            "research.ast.mission_operations_integration",
            "research.ast.mission_refit_protocols",
            "research.ast.specialized_extraction_modules",
            "research.ast.switchframe_actuation",
            "research.mar.advanced_excavation_systems",
            "research.mar.aero_handling_decks",
            "research.mar.grand_network_integration",
            "research.mar.hypersled_throughput",
            "research.mar.mechanical_worker_toolset",
            "research.mar.pressure_equalization_valves",
            "research.mar.redundant_routing",
            "research.mar.utility_mechanisms",
            "research.mar.walker_articulation",
            "research.rr.advanced_power_distribution",
            "research.rr.cutter_package",
            "research.rr.deep_core_engineering",
            "research.rr.geological_survey_calibration",
            "research.rr.high_capacity_processing",
            "research.rr.industrial_expansion_program",
            "research.rr.reinforced_drilling_assemblies",
            "research.rr.service_gantries",
            "research.rr.worksite_automation"
        };

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Research.Select(research => research.StableKey), Is.EqualTo(expectedKeys));
            Assert.That(catalog.Research.Count(research => research.FactionKey == "RockRaiders"), Is.EqualTo(9));
            Assert.That(catalog.Research.Count(research => research.FactionKey == "Astronauts"), Is.EqualTo(10));
            Assert.That(catalog.Research.Count(research => research.FactionKey == "Aliens"), Is.EqualTo(10));
            Assert.That(catalog.Research.Count(research => research.FactionKey == "Martians"), Is.EqualTo(9));
            Assert.That(catalog.Research, Has.All.Matches<ResearchDefinition>(research => research.OreCost > 0 && research.ResearchTicks > 0));
            Assert.That(catalog.Research, Has.All.Matches<ResearchDefinition>(research =>
                research.UnlockTags.Length > 0 || research.ParameterModifiers.Length > 0));
        });
    }

    [Test]
    public void M8T072CanonicalDagCostsAndEffectsSurviveBinaryRoundTrip()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(PrototypeContentFactory.CreateM2Catalog()));

        Assert.That(restored.TryGetResearch("research.ast.integrated_expedition_command", out ResearchDefinition integrated), Is.True);
        Assert.That(restored.TryGetResearch("research.ali.advanced_resonance_architecture", out ResearchDefinition alienAdvanced), Is.True);
        Assert.That(restored.TryGetResearch("research.mar.grand_network_integration", out ResearchDefinition grandNetwork), Is.True);
        Assert.That(restored.TryGetResearch("research.rr.industrial_expansion_program", out ResearchDefinition industrialExpansion), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That((integrated.OreCost, integrated.EnergyCost, integrated.CrystalCost, integrated.ResearchTicks), Is.EqualTo((240, 90, 3, 1_500)));
            Assert.That(integrated.PrerequisiteGroups, Has.Length.EqualTo(3));
            Assert.That(integrated.PrerequisiteGroups[2].Alternatives, Has.Length.EqualTo(5));
            Assert.That(integrated.UnlockTags, Does.Contain("unit.astronauts.mx81_operations_aircraft"));
            Assert.That(integrated.ParameterModifiers.Single().Value, Is.EqualTo(2));
            Assert.That(alienAdvanced.PrerequisiteGroups[0].Alternatives[0].TargetStableKey, Is.EqualTo("state.ali.committed_crystals"));
            Assert.That(alienAdvanced.PrerequisiteGroups[0].Alternatives[0].MinimumValue, Is.EqualTo(4));
            Assert.That(alienAdvanced.PrerequisiteGroups[0].Alternatives[0].Persistence, Is.EqualTo(ResearchPrerequisitePersistence.WhileResearching));
            Assert.That(grandNetwork.PrerequisiteGroups.SelectMany(group => group.Alternatives).Select(value => value.TargetStableKey),
                Is.EquivalentTo(new[] { "research.mar.redundant_routing", "state.mar.connected_station_nodes" }));
            Assert.That(industrialExpansion.PrerequisiteGroups, Has.Length.EqualTo(3));
            Assert.That(industrialExpansion.UnlockTags, Is.EquivalentTo(new[]
            {
                "building.rock_raiders.crystal_vault", "building.rock_raiders.cutter_mast", "building.rock_raiders.engineering_workshop"
            }));
        });
    }

    [Test]
    public void ResearchDagValidationRejectsCyclesAndUnresolvedContentEffects()
    {
        ResearchDefinition first = TestResearch("research.rr.first", "research.rr.second", "capability.rr.first");
        ResearchDefinition second = TestResearch("research.rr.second", "research.rr.first", "capability.rr.second");
        Assert.That(() => CreateResearchTestCatalog(first, second), Throws.ArgumentException.With.Message.Contains("cycle"));

        ResearchDefinition unresolved = TestResearch("research.rr.unresolved", null, "unit.rock_raiders.missing");
        Assert.That(() => CreateResearchTestCatalog(unresolved), Throws.ArgumentException.With.Message.Contains("missing or cross-faction content"));
    }

    [Test]
    public void Format17CatalogsRemainReadableWithoutResearchDefinitions()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(CreateFormat17EmptyCatalog());

        Assert.That(restored.Research, Is.Empty);
    }

    [Test]
    public void Format18CatalogsRemainReadableWithoutActionOrCommandDefinitions()
    {
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(CreateFormat18EmptyCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(restored.Research, Is.Empty);
            Assert.That(restored.Commands, Is.Empty);
            Assert.That(restored.Production, Is.Empty);
            Assert.That(restored.Buildings, Is.Empty);
        });
    }

    [Test]
    public void PrototypeContentBinaryRoundTripsAndHashesIdentically()
    {
        PrototypeContentCatalog source = new PrototypeContentCatalog(
            new[] { new PrototypeMovementProfile("movement.test", Fix32.FromRatio(3, 2), MovementLayer.GroundHover) },
            new[] { new PrototypeEntityDefinition("unit.test", "Technical", "ENGINEERING_ONLY", "movement.test", FootprintClass.Small, SelectableKind.CombatSupport, 8, "view.test", operationsCapacity: 3,
                combat: new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 120, 1, TargetPriorityProfile.Generalist, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromInt(7))) });
        byte[] bytes = PrototypeContentCodec.Write(source);
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(bytes);
        Assert.That(restored.ContentHash, Is.EqualTo(source.ContentHash));
        Assert.That(restored.Entities[0].StableKey, Is.EqualTo("unit.test"));
        Assert.That(restored.Entities[0].OperationsCapacity, Is.EqualTo(3));
        Assert.That(restored.Entities[0].Combat.TargetClass, Is.EqualTo(CombatTargetClass.LightMachine));
        Assert.That(restored.Entities[0].Combat.MaximumHitPoints, Is.EqualTo(120));
        Assert.That(restored.Entities[0].Combat.ArmorRating, Is.EqualTo(1));
        Assert.That(restored.Entities[0].Combat.PriorityProfile, Is.EqualTo(TargetPriorityProfile.Generalist));
        Assert.That(restored.Entities[0].Combat.AcquisitionRadius, Is.EqualTo(Fix32.FromInt(7)));
        Assert.That(restored.MovementProfiles[0].MaxSpeed, Is.EqualTo(Fix32.FromRatio(3, 2)));
    }

    [Test]
    public void ResourceNodeDefinitionsRoundTripWithStableIdsAndThresholds()
    {
        PrototypeContentCatalog source = new PrototypeContentCatalog(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(),
            new[] { new ResourceNodeDefinition("resource.test", ResourceType.Ore, ResourceDepositSize.Standard, 900, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.test.resource") });
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source));
        Assert.That(restored.ResourceNodes.Length, Is.EqualTo(1));
        Assert.That(restored.ResourceNodes[0].Id, Is.EqualTo(StableId.FromKey("resource.test")));
        Assert.That(restored.ResourceNodes[0].Capacity, Is.EqualTo(900));
        Assert.That(restored.ResourceNodes[0].CriticalThresholdBasisPoints, Is.EqualTo(2500));
    }

    [Test]
    public void BuildingDefinitionsRoundTripExplicitFootprintsCostsAndExits()
    {
        BuildingDefinition sourceBuilding = new("building.test", 3, 2, 0b11_1111UL, true, 140, 15, 600, 2, 1, FootprintClass.Medium, 4, 10, 120, 3, worksiteServiceRadius: 12);
        PrototypeContentCatalog source = new(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(), buildings: new[] { sourceBuilding });
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source));
        BuildingDefinition building = restored.Buildings[0];
        Assert.That(building.Id, Is.EqualTo(StableId.FromKey("building.test")));
        Assert.That(building.FootprintWidth, Is.EqualTo(3));
        Assert.That(building.FootprintHeight, Is.EqualTo(2));
        Assert.That(building.RotatedWidth(1), Is.EqualTo(2));
        Assert.That(building.OreCost, Is.EqualTo(140));
        Assert.That(building.EnergyCost, Is.EqualTo(15));
        Assert.That(building.CrystalCost, Is.Zero);
        Assert.That(building.ProductionExitFootprint, Is.EqualTo(FootprintClass.Medium));
        Assert.That(building.OperationsCapacityProvided, Is.EqualTo(4));
        Assert.That(building.EnergyGenerationPerSecond, Is.EqualTo(10));
        Assert.That(building.EnergyReserveCapacity, Is.EqualTo(120));
        Assert.That(building.ContinuousEnergyDemandPerSecond, Is.EqualTo(3));
        Assert.That(building.WorksiteServiceRadius, Is.EqualTo(12));
    }

    [Test]
    public void ProductionDefinitionsRoundTripCanonicalQueueMetadata()
    {
        UnitProductionDefinition sourceProduction = new("unit.test", "building.test", 90, 10, 0, 2, 560);
        PrototypeMovementProfile movement = new("movement.test", Fix32.FromInt(1), MovementLayer.Ground);
        PrototypeEntityDefinition unit = new("unit.test", "RockRaiders", "ENGINEERING_ONLY", movement.StableKey,
            FootprintClass.Small, SelectableKind.CombatSupport, 1, "view.test.unit", operationsCapacity: 2);
        PrototypeEntityDefinition producer = new("building.test", "RockRaiders", "ENGINEERING_ONLY", movement.StableKey,
            FootprintClass.Small, SelectableKind.Building, 1, "view.test.producer");
        BuildingDefinition building = new("building.test", 1, 1, 1UL, false, 1, 0, 1);
        PrototypeContentCatalog source = new(new[] { movement }, new[] { unit, producer }, buildings: new[] { building }, production: new[] { sourceProduction });
        UnitProductionDefinition restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source)).Production[0];
        Assert.Multiple(() =>
        {
            Assert.That(restored.UnitType, Is.EqualTo(StableId.FromKey("unit.test")));
            Assert.That(restored.ProducerType, Is.EqualTo(StableId.FromKey("building.test")));
            Assert.That(restored.OreCost, Is.EqualTo(90)); Assert.That(restored.OperationsCapacity, Is.EqualTo(2)); Assert.That(restored.BuildTicks, Is.EqualTo(560));
        });
    }

    [Test]
    public void WeaponDefinitionsRoundTripAuthoritativeFiringMetadata()
    {
        WeaponDefinition sourceWeapon = new("weapon.test", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Generalist,
            12, DamageType.General, 25, Fix32.FromRatio(7, 2), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(10), true, 45, 3_500);
        PrototypeContentCatalog source = new(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(), weapons: new[] { sourceWeapon });
        WeaponDefinition restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source)).Weapons[0];
        Assert.Multiple(() =>
        {
            Assert.That(restored.Id, Is.EqualTo(StableId.FromKey("weapon.test")));
            Assert.That(restored.BaseDamage, Is.EqualTo(12));
            Assert.That(restored.CooldownTicks, Is.EqualTo(25));
            Assert.That(restored.Range, Is.EqualTo(Fix32.FromRatio(7, 2)));
            Assert.That(restored.DeliveryKind, Is.EqualTo(WeaponDeliveryKind.Projectile));
            Assert.That(restored.ProjectileSpeed, Is.EqualTo(Fix32.FromInt(10)));
            Assert.That(restored.RequiresLineOfSight, Is.True);
            Assert.That(restored.FacingToleranceAngle16, Is.EqualTo(Angle16.Quarter.Raw / 2));
            Assert.That(restored.MaximumMovingFireSpeedBasisPoints, Is.EqualTo(3_500));
        });
    }

    [Test]
    public void TransformationDefinitionsRoundTripCanonicalModeData()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(catalog));
        Assert.That(restored.Transformations, Has.Length.EqualTo(1));
        TransformationDefinition transformation = restored.Transformations[0];
        Assert.Multiple(() =>
        {
            Assert.That(transformation.EntityType, Is.EqualTo(StableId.FromKey("unit.astronauts.mx41_switch_fighter")));
            Assert.That(transformation.AToBDurationTicks, Is.EqualTo(45));
            Assert.That(transformation.CancellationThresholdBasisPoints, Is.EqualTo(4_000));
            Assert.That(transformation.RollbackTicks, Is.EqualTo(12));
            Assert.That(transformation.ReversalLockTicks, Is.EqualTo(160));
            Assert.That(transformation.ModeA.DisplayName, Is.EqualTo("Ground"));
            Assert.That(transformation.ModeB.DisplayName, Is.EqualTo("Flight"));
            Assert.That(transformation.ModeB.Combat.TargetLayer, Is.EqualTo(CombatTargetLayer.TrueAir));
            Assert.That(transformation.TransitionTargetLayers, Is.EqualTo(TargetLayerMask.All));
        });
    }

    private static void AssertOrdered(string[] keys)
    {
        string[] expected = keys.OrderBy(key => key, System.StringComparer.Ordinal).ToArray();
        Assert.That(keys, Is.EqualTo(expected));
    }

    private static ResearchDefinition TestResearch(string key, string? prerequisite, string unlock)
    {
        ResearchPrerequisiteGroup[] requirements = prerequisite is null
            ? Array.Empty<ResearchPrerequisiteGroup>()
            : new[]
            {
                new ResearchPrerequisiteGroup(new[]
                {
                    new ResearchPrerequisiteDefinition(ResearchPrerequisiteKind.Research, prerequisite)
                })
            };
        return new ResearchDefinition(key, "RockRaiders", ResearchCategory.Economic, "building.rock_raiders.test_lab",
            10, 0, 0, 20, requirements, new[] { unlock }, Array.Empty<ResearchParameterModifier>(), string.Empty,
            "presentation." + key, "loc." + key + ".name");
    }

    private static PrototypeContentCatalog CreateResearchTestCatalog(params ResearchDefinition[] research)
    {
        PrototypeMovementProfile movement = new("movement.test.static", Fix32.Zero, Fix32.Zero, Fix32.Zero, 0, ReversePolicy.None, MovementLayer.Ground);
        PrototypeEntityDefinition entity = new("building.rock_raiders.test_lab", "RockRaiders", "ENGINEERING_ONLY", movement.StableKey,
            FootprintClass.Small, SelectableKind.Building, 1, "view.test.research_lab");
        BuildingDefinition building = new(entity.StableKey, 1, 1, 1UL, false, 1, 0, 1);
        return new PrototypeContentCatalog(new[] { movement }, new[] { entity }, buildings: new[] { building }, research: research);
    }

    private static byte[] CreateFormat17EmptyCatalog()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(0x4350534C);
        writer.Write(17);
        writer.Write(0); // Movement profiles.
        writer.Write(0); // Entities.
        writer.Write(0); // Resource nodes.
        writer.Write(0); // Buildings.
        writer.Write(0); // Production definitions.
        writer.Write(0); // Weapon definitions.
        writer.Write(0); // Transformation definitions.
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] CreateFormat18EmptyCatalog()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(0x4350534C);
        writer.Write(18);
        writer.Write(0); // Movement profiles.
        writer.Write(0); // Entities.
        writer.Write(0); // Resource nodes.
        writer.Write(0); // Buildings.
        writer.Write(0); // Production definitions.
        writer.Write(0); // Weapon definitions.
        writer.Write(0); // Transformation definitions.
        writer.Write(0); // Research definitions.
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] CreateFormat16BuildingCatalog()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(0x4350534C);
        writer.Write(16);
        writer.Write(0); // Movement profiles.
        writer.Write(0); // Entities.
        writer.Write(0); // Resource nodes.
        writer.Write(1); // Buildings.
        writer.Write("building.legacy");
        writer.Write(StableId.FromKey("building.legacy").Value);
        writer.Write((byte)3);
        writer.Write((byte)2);
        writer.Write(0b11_1111UL);
        writer.Write(true);
        writer.Write((ushort)140);
        writer.Write((ushort)15);
        writer.Write((ushort)600);
        writer.Write((byte)2);
        writer.Write((byte)1);
        writer.Write((byte)FootprintClass.Medium);
        writer.Write((byte)4);
        writer.Write((ushort)10);
        writer.Write((ushort)120);
        writer.Write((ushort)3);
        writer.Write((byte)EnergyFunctionalClass.ProductionAndResearch);
        writer.Write((byte)12);
        writer.Write(0); // Production definitions.
        writer.Write(0); // Weapon definitions.
        writer.Write(0); // Transformation definitions.
        writer.Flush();
        return stream.ToArray();
    }
}
}
