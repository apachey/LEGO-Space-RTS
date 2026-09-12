using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
[Flags]
public enum ResearchCategory : byte
{
    None = 0,
    Economic = 1 << 0,
    Operational = 1 << 1,
    Combat = 1 << 2,
    FactionSystem = 1 << 3,
    Progression = 1 << 4,
    Recovery = 1 << 5
}

public enum ResearchPrerequisiteKind : byte
{
    Research = 1,
    Building = 2,
    StateThreshold = 3
}

public enum ResearchPrerequisitePersistence : byte
{
    AtStart = 1,
    WhileResearching = 2
}

public enum ResearchModifierOperation : byte
{
    Add = 1,
    Set = 2,
    MultiplyBasisPoints = 3
}

/// <summary>One possible prerequisite inside an any-of group. All groups on a research definition are required.</summary>
public readonly struct ResearchPrerequisiteDefinition
{
    public readonly ResearchPrerequisiteKind Kind;
    public readonly string TargetStableKey;
    public readonly ContentId TargetId;
    public readonly ushort MinimumValue;
    public readonly ResearchPrerequisitePersistence Persistence;

    public ResearchPrerequisiteDefinition(ResearchPrerequisiteKind kind, string targetStableKey, ushort minimumValue = 1,
        ResearchPrerequisitePersistence persistence = ResearchPrerequisitePersistence.AtStart)
    {
        if (kind < ResearchPrerequisiteKind.Research || kind > ResearchPrerequisiteKind.StateThreshold)
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (string.IsNullOrWhiteSpace(targetStableKey)) throw new ArgumentNullException(nameof(targetStableKey));
        if (minimumValue == 0) throw new ArgumentOutOfRangeException(nameof(minimumValue));
        if (persistence < ResearchPrerequisitePersistence.AtStart || persistence > ResearchPrerequisitePersistence.WhileResearching)
            throw new ArgumentOutOfRangeException(nameof(persistence));
        if (kind != ResearchPrerequisiteKind.StateThreshold && minimumValue != 1)
            throw new ArgumentException("Research and building prerequisites are boolean references.");
        Kind = kind;
        TargetStableKey = targetStableKey;
        TargetId = StableId.FromKey(targetStableKey);
        MinimumValue = minimumValue;
        Persistence = persistence;
    }
}

public readonly struct ResearchPrerequisiteGroup
{
    public readonly ResearchPrerequisiteDefinition[] Alternatives;

    public ResearchPrerequisiteGroup(ResearchPrerequisiteDefinition[] alternatives)
    {
        if (alternatives is null || alternatives.Length == 0)
            throw new ArgumentException("A research prerequisite group requires at least one alternative.", nameof(alternatives));
        Alternatives = (ResearchPrerequisiteDefinition[])alternatives.Clone();
    }
}

public readonly struct ResearchParameterModifier
{
    public readonly string TargetStableKey;
    public readonly ContentId TargetId;
    public readonly ResearchModifierOperation Operation;
    public readonly int Value;

    public ResearchParameterModifier(string targetStableKey, ResearchModifierOperation operation, int value)
    {
        if (string.IsNullOrWhiteSpace(targetStableKey)) throw new ArgumentNullException(nameof(targetStableKey));
        if (operation < ResearchModifierOperation.Add || operation > ResearchModifierOperation.MultiplyBasisPoints)
            throw new ArgumentOutOfRangeException(nameof(operation));
        if (operation == ResearchModifierOperation.MultiplyBasisPoints && value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        TargetStableKey = targetStableKey;
        TargetId = StableId.FromKey(targetStableKey);
        Operation = operation;
        Value = value;
    }
}

/// <summary>Canonical, data-only research metadata. T073 owns jobs, commands and effect application.</summary>
public readonly struct ResearchDefinition
{
    private const ResearchCategory AllCategories = ResearchCategory.Economic | ResearchCategory.Operational | ResearchCategory.Combat |
        ResearchCategory.FactionSystem | ResearchCategory.Progression | ResearchCategory.Recovery;

    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly string FactionKey;
    public readonly ResearchCategory Categories;
    public readonly string SourceBuildingStableKey;
    public readonly ContentId SourceBuildingType;
    public readonly ushort OreCost;
    public readonly ushort EnergyCost;
    public readonly byte CrystalCost;
    public readonly ushort ResearchTicks;
    public readonly ResearchPrerequisiteGroup[] PrerequisiteGroups;
    public readonly string[] UnlockTags;
    public readonly ResearchParameterModifier[] ParameterModifiers;
    public readonly string MutuallyExclusiveGroupKey;
    public readonly string PresentationProfileKey;
    public readonly string DisplayNameLocKey;

    public ResearchDefinition(string stableKey, string factionKey, ResearchCategory categories, string sourceBuildingStableKey,
        ushort oreCost, ushort energyCost, byte crystalCost, ushort researchTicks,
        ResearchPrerequisiteGroup[]? prerequisiteGroups, string[]? unlockTags, ResearchParameterModifier[]? parameterModifiers,
        string mutuallyExclusiveGroupKey, string presentationProfileKey, string displayNameLocKey)
    {
        if (string.IsNullOrWhiteSpace(stableKey) || string.IsNullOrWhiteSpace(factionKey) || string.IsNullOrWhiteSpace(sourceBuildingStableKey))
            throw new ArgumentException("Research requires stable, faction and source-building keys.");
        if (categories == ResearchCategory.None || (categories & ~AllCategories) != 0)
            throw new ArgumentOutOfRangeException(nameof(categories));
        if (oreCost == 0 || researchTicks == 0) throw new ArgumentOutOfRangeException(nameof(researchTicks));
        if (string.IsNullOrWhiteSpace(presentationProfileKey) || string.IsNullOrWhiteSpace(displayNameLocKey))
            throw new ArgumentException("Research requires presentation and localization references.");

        ResearchPrerequisiteGroup[] groups = prerequisiteGroups ?? Array.Empty<ResearchPrerequisiteGroup>();
        string[] tags = unlockTags ?? Array.Empty<string>();
        ResearchParameterModifier[] modifiers = parameterModifiers ?? Array.Empty<ResearchParameterModifier>();
        if (tags.Length == 0 && modifiers.Length == 0) throw new ArgumentException("Research requires at least one gameplay effect.");
        for (int i = 0; i < tags.Length; i++)
            if (string.IsNullOrWhiteSpace(tags[i])) throw new ArgumentException("Research unlock tags cannot be empty.", nameof(unlockTags));

        StableKey = stableKey;
        Id = StableId.FromKey(stableKey);
        FactionKey = factionKey;
        Categories = categories;
        SourceBuildingStableKey = sourceBuildingStableKey;
        SourceBuildingType = StableId.FromKey(sourceBuildingStableKey);
        OreCost = oreCost;
        EnergyCost = energyCost;
        CrystalCost = crystalCost;
        ResearchTicks = researchTicks;
        PrerequisiteGroups = (ResearchPrerequisiteGroup[])groups.Clone();
        UnlockTags = (string[])tags.Clone();
        ParameterModifiers = (ResearchParameterModifier[])modifiers.Clone();
        MutuallyExclusiveGroupKey = mutuallyExclusiveGroupKey ?? string.Empty;
        PresentationProfileKey = presentationProfileKey;
        DisplayNameLocKey = displayNameLocKey;
    }
}

internal static class ResearchDefinitionValidator
{
    private static readonly HashSet<string> CanonicalFactions = new HashSet<string>(StringComparer.Ordinal)
    {
        "RockRaiders", "Astronauts", "Aliens", "Martians"
    };

    public static void Validate(PrototypeContentCatalog catalog)
    {
        Dictionary<string, ResearchDefinition> byKey = new Dictionary<string, ResearchDefinition>(StringComparer.Ordinal);
        HashSet<uint> occupiedPrimaryIds = CollectPrimaryIds(catalog);
        for (int i = 0; i < catalog.Research.Length; i++)
        {
            ResearchDefinition definition = catalog.Research[i];
            if (!byKey.TryAdd(definition.StableKey, definition))
                throw new ArgumentException($"Duplicate research definition {definition.StableKey}.");
            if (!occupiedPrimaryIds.Add(definition.Id.Value))
                throw new ArgumentException($"Stable ID collision at research {definition.StableKey}.");
            if (!CanonicalFactions.Contains(definition.FactionKey))
                throw new ArgumentException($"Research {definition.StableKey} references invalid faction {definition.FactionKey}.");
            if (!catalog.TryGetBuilding(definition.SourceBuildingType, out _) ||
                !catalog.TryGetEntity(definition.SourceBuildingType, out PrototypeEntityDefinition sourceEntity) ||
                !string.Equals(sourceEntity.FactionKey, definition.FactionKey, StringComparison.Ordinal))
                throw new ArgumentException($"Research {definition.StableKey} references an invalid source building.");
            if (definition.MutuallyExclusiveGroupKey.Length != 0 &&
                !definition.MutuallyExclusiveGroupKey.StartsWith("research.exclusive.", StringComparison.Ordinal))
                throw new ArgumentException($"Research {definition.StableKey} has an invalid mutually-exclusive group key.");
        }

        for (int i = 0; i < catalog.Research.Length; i++)
        {
            ResearchDefinition definition = catalog.Research[i];
            ValidatePrerequisites(catalog, definition, byKey);
            ValidateEffects(catalog, definition);
        }
        ValidateDag(catalog.Research, byKey);
    }

    private static HashSet<uint> CollectPrimaryIds(PrototypeContentCatalog catalog)
    {
        HashSet<uint> result = new HashSet<uint>();
        for (int i = 0; i < catalog.MovementProfiles.Length; i++) result.Add(catalog.MovementProfiles[i].Id.Value);
        for (int i = 0; i < catalog.Entities.Length; i++) result.Add(catalog.Entities[i].Id.Value);
        for (int i = 0; i < catalog.ResourceNodes.Length; i++) result.Add(catalog.ResourceNodes[i].Id.Value);
        for (int i = 0; i < catalog.Weapons.Length; i++) result.Add(catalog.Weapons[i].Id.Value);
        for (int i = 0; i < catalog.Transformations.Length; i++)
        {
            result.Add(catalog.Transformations[i].Id.Value);
            result.Add(catalog.Transformations[i].ModeA.StateId.Value);
            result.Add(catalog.Transformations[i].ModeB.StateId.Value);
        }
        return result;
    }

    private static void ValidatePrerequisites(PrototypeContentCatalog catalog, ResearchDefinition definition,
        Dictionary<string, ResearchDefinition> researchByKey)
    {
        HashSet<string> requirementKeys = new HashSet<string>(StringComparer.Ordinal);
        for (int groupIndex = 0; groupIndex < definition.PrerequisiteGroups.Length; groupIndex++)
        {
            ResearchPrerequisiteDefinition[] alternatives = definition.PrerequisiteGroups[groupIndex].Alternatives;
            for (int alternativeIndex = 0; alternativeIndex < alternatives.Length; alternativeIndex++)
            {
                ResearchPrerequisiteDefinition prerequisite = alternatives[alternativeIndex];
                string uniqueKey = ((byte)prerequisite.Kind).ToString() + ":" + prerequisite.TargetStableKey;
                if (!requirementKeys.Add(uniqueKey))
                    throw new ArgumentException($"Research {definition.StableKey} repeats prerequisite {prerequisite.TargetStableKey}.");
                switch (prerequisite.Kind)
                {
                    case ResearchPrerequisiteKind.Research:
                        if (!researchByKey.TryGetValue(prerequisite.TargetStableKey, out ResearchDefinition requiredResearch) ||
                            !string.Equals(requiredResearch.FactionKey, definition.FactionKey, StringComparison.Ordinal) || requiredResearch.Id == definition.Id)
                            throw new ArgumentException($"Research {definition.StableKey} has an invalid technology prerequisite.");
                        break;
                    case ResearchPrerequisiteKind.Building:
                        if (!catalog.TryGetBuilding(prerequisite.TargetId, out _) ||
                            !catalog.TryGetEntity(prerequisite.TargetId, out PrototypeEntityDefinition buildingEntity) ||
                            !string.Equals(buildingEntity.FactionKey, definition.FactionKey, StringComparison.Ordinal))
                            throw new ArgumentException($"Research {definition.StableKey} has an invalid building prerequisite.");
                        break;
                    case ResearchPrerequisiteKind.StateThreshold:
                        if (!prerequisite.TargetStableKey.StartsWith("state.", StringComparison.Ordinal))
                            throw new ArgumentException($"Research {definition.StableKey} has an invalid state prerequisite.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private static void ValidateEffects(PrototypeContentCatalog catalog, ResearchDefinition definition)
    {
        HashSet<string> effectKeys = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < definition.UnlockTags.Length; i++)
        {
            string tag = definition.UnlockTags[i];
            if (!effectKeys.Add("unlock:" + tag)) throw new ArgumentException($"Research {definition.StableKey} repeats unlock {tag}.");
            if (tag.StartsWith("unit.", StringComparison.Ordinal) || tag.StartsWith("building.", StringComparison.Ordinal))
            {
                if (!catalog.TryGetEntity(tag, out PrototypeEntityDefinition entity) ||
                    !string.Equals(entity.FactionKey, definition.FactionKey, StringComparison.Ordinal))
                    throw new ArgumentException($"Research {definition.StableKey} references missing or cross-faction content {tag}.");
            }
            else if (!tag.StartsWith("capability.", StringComparison.Ordinal))
                throw new ArgumentException($"Research {definition.StableKey} has invalid unlock tag {tag}.");
        }
        for (int i = 0; i < definition.ParameterModifiers.Length; i++)
        {
            ResearchParameterModifier modifier = definition.ParameterModifiers[i];
            if (!modifier.TargetStableKey.StartsWith("parameter.", StringComparison.Ordinal))
                throw new ArgumentException($"Research {definition.StableKey} has invalid parameter target {modifier.TargetStableKey}.");
            if (!effectKeys.Add("parameter:" + modifier.TargetStableKey))
                throw new ArgumentException($"Research {definition.StableKey} repeats parameter target {modifier.TargetStableKey}.");
        }
    }

    private static void ValidateDag(ResearchDefinition[] definitions, Dictionary<string, ResearchDefinition> byKey)
    {
        Dictionary<string, byte> state = new Dictionary<string, byte>(StringComparer.Ordinal);
        for (int i = 0; i < definitions.Length; i++) Visit(definitions[i].StableKey, byKey, state);
    }

    private static void Visit(string key, Dictionary<string, ResearchDefinition> byKey, Dictionary<string, byte> state)
    {
        if (state.TryGetValue(key, out byte existing))
        {
            if (existing == 1) throw new ArgumentException($"Research prerequisite cycle includes {key}.");
            if (existing == 2) return;
        }
        state[key] = 1;
        ResearchDefinition definition = byKey[key];
        for (int groupIndex = 0; groupIndex < definition.PrerequisiteGroups.Length; groupIndex++)
        {
            ResearchPrerequisiteDefinition[] alternatives = definition.PrerequisiteGroups[groupIndex].Alternatives;
            for (int i = 0; i < alternatives.Length; i++)
                if (alternatives[i].Kind == ResearchPrerequisiteKind.Research)
                    Visit(alternatives[i].TargetStableKey, byKey, state);
        }
        state[key] = 2;
    }
}

internal static class CanonicalResearchDefinitions
{
    public static ResearchDefinition[] Create()
    {
        return new[]
        {
            Def("research.ali.advanced_resonance_architecture", "Aliens", ResearchCategory.Progression, "building.ali.reconfiguration_dock", 240, 110, 5, 80,
                Req(State("state.ali.committed_crystals", 4, ResearchPrerequisitePersistence.WhileResearching)),
                Unlock("unit.aliens.alien_mothership")),
            Def("research.ali.defense_resonance_shunt", "Aliens", ResearchCategory.FactionSystem, "building.ali.resonance_core", 120, 50, 1, 45,
                unlocks: Unlock("capability.ali.etx_defense_node.charge_shunt"),
                modifiers: Mods(Multiply("parameter.ali.etx_defense_node.cooldown", 7_500), Multiply("parameter.ali.etx_defense_node.tracking", 13_000))),
            Def("research.ali.etx_reconfiguration_matrix", "Aliens", ResearchCategory.Progression, "building.ali.resonance_core", 160, 65, 2, 55,
                Req(Building("building.ali.etx_fabricator")), Unlock("building.ali.reconfiguration_dock")),
            Def("research.ali.expanded_resonance_lattice", "Aliens", ResearchCategory.FactionSystem, "building.ali.resonance_core", 170, 70, 3, 60,
                Req(Research("research.ali.etx_reconfiguration_matrix")),
                modifiers: Mods(Set("parameter.ali.resonance_core.commitment_slots", 6))),
            Def("research.ali.infiltration_matrix", "Aliens", ResearchCategory.Combat | ResearchCategory.Operational, "building.ali.reconfiguration_dock", 150, 55, 2, 50,
                unlocks: Unlock("unit.aliens.etx_alien_infiltrator")),
            Def("research.ali.mothership_resonance_relay", "Aliens", ResearchCategory.Operational, "building.ali.reconfiguration_dock", 190, 80, 3, 60,
                Req(Research("research.ali.advanced_resonance_architecture")),
                Unlock("capability.ali.alien_mothership.mobile_surge_anchor"),
                Mods(Set("parameter.ali.alien_mothership.surge_anchor_range_cells", 10))),
            Def("research.ali.rapid_fabrication_conduits", "Aliens", ResearchCategory.Economic | ResearchCategory.Operational, "building.ali.resonance_core", 120, 50, 1, 45,
                unlocks: Unlock("capability.ali.charge_production_injection")),
            Def("research.ali.resonance_initiation", "Aliens", ResearchCategory.FactionSystem, "building.ali.resonance_core", 90, 40, 1, 40,
                unlocks: Unlock("capability.ali.surge_window")),
            Def("research.ali.resonant_recovery_latches", "Aliens", ResearchCategory.Economic | ResearchCategory.Recovery, "building.ali.resonance_core", 120, 45, 1, 45,
                modifiers: Mods(Add("parameter.ali.resonance_core.additional_intact_salvage_crystals", 1))),
            Def("research.ali.siege_phase_coupling", "Aliens", ResearchCategory.Combat | ResearchCategory.Operational, "building.ali.reconfiguration_dock", 150, 55, 2, 50,
                unlocks: Unlock("unit.aliens.etx_alien_strike")),

            Def("research.ast.aerospace_coordination", "Astronauts", ResearchCategory.Operational, "building.ast.service_refit_hub", 150, 60, 2, 55,
                Req(Research("research.ast.mission_operations_integration")),
                Unlock("unit.astronauts.mission_fighter", "unit.astronauts.mx71_recon_dropship")),
            Def("research.ast.deep_mission_drilling", "Astronauts", ResearchCategory.Combat, "building.ast.service_refit_hub", 220, 80, 3, 70,
                Req(Research("research.ast.heavy_mission_chassis")), Unlock("unit.astronauts.mt201_ultra_drill_walker")),
            Def("research.ast.field_survey_package", "Astronauts", ResearchCategory.Operational, "building.ast.service_refit_hub", 80, 15, 0, 35,
                unlocks: Unlock("capability.ast.field_survey_package", "capability.ast.t3_trike.survey_configuration"),
                modifiers: Mods(Add("parameter.ast.rover.sight_cells", 1), Add("parameter.ast.rover.detection_cells", 2))),
            Def("research.ast.field_sustainment_package", "Astronauts", ResearchCategory.Operational, "building.ast.service_refit_hub", 120, 30, 0, 45,
                unlocks: Unlock("capability.ast.solar_explorer.deployed_forward_service", "unit.astronauts.solar_explorer")),
            Def("research.ast.heavy_mission_chassis", "Astronauts", ResearchCategory.Combat | ResearchCategory.Operational, "building.ast.service_refit_hub", 180, 60, 2, 60,
                Req(Research("research.ast.mission_operations_integration")), Unlock("unit.astronauts.mt101_armored_drilling_unit")),
            Def("research.ast.integrated_expedition_command", "Astronauts", ResearchCategory.Progression | ResearchCategory.FactionSystem, "building.ast.service_refit_hub", 240, 90, 3, 75,
                Reqs(
                    Group(Research("research.ast.field_sustainment_package")),
                    Group(Research("research.ast.mission_operations_integration")),
                    Group(
                        Research("research.ast.aerospace_coordination"),
                        Research("research.ast.deep_mission_drilling"),
                        Research("research.ast.heavy_mission_chassis"),
                        Research("research.ast.specialized_extraction_modules"),
                        Research("research.ast.switchframe_actuation"))),
                Unlock("unit.astronauts.mx81_operations_aircraft"),
                Mods(Set("parameter.ast.service_refit_hub.refit_bays", 2))),
            Def("research.ast.mission_operations_integration", "Astronauts", ResearchCategory.Progression, "building.ast.service_refit_hub", 150, 50, 1, 55,
                unlocks: Unlock("building.ast.flight_operations_pad", "building.ast.mission_vehicle_bay")),
            Def("research.ast.mission_refit_protocols", "Astronauts", ResearchCategory.FactionSystem, "building.ast.service_refit_hub", 120, 35, 1, 45,
                unlocks: Unlock("capability.ast.mission_refit")),
            Def("research.ast.specialized_extraction_modules", "Astronauts", ResearchCategory.Economic, "building.ast.service_refit_hub", 130, 40, 1, 50,
                Req(Research("research.ast.mission_operations_integration")),
                Unlock("capability.ast.mobile_mining_platform.crystal_reaper_configuration")),
            Def("research.ast.switchframe_actuation", "Astronauts", ResearchCategory.Operational, "building.ast.service_refit_hub", 140, 55, 1, 50,
                Req(Research("research.ast.mission_operations_integration")), Unlock("unit.astronauts.mx41_switch_fighter")),

            Def("research.mar.advanced_excavation_systems", "Martians", ResearchCategory.Economic | ResearchCategory.Combat, "building.mar.routing_laboratory", 190, 65, 2, 60,
                unlocks: Unlock("capability.mar.excavation_searcher.advanced_excavation_systems", "capability.mar.excavation_searcher.excavation_clamp")),
            Def("research.mar.aero_handling_decks", "Martians", ResearchCategory.Operational, "building.mar.routing_laboratory", 100, 30, 0, 40,
                unlocks: Unlock("unit.martians.aero_skiff")),
            Def("research.mar.grand_network_integration", "Martians", ResearchCategory.Progression, "building.mar.routing_laboratory", 230, 80, 3, 75,
                Reqs(Group(Research("research.mar.redundant_routing")), Group(State("state.mar.connected_station_nodes", 2))),
                Unlock("capability.mar.excavation_searcher.grand_network_integration")),
            Def("research.mar.hypersled_throughput", "Martians", ResearchCategory.FactionSystem, "building.mar.routing_laboratory", 120, 30, 0, 45,
                modifiers: Mods(Set("parameter.mar.station.simultaneous_transfer_capacity", 3))),
            Def("research.mar.mechanical_worker_toolset", "Martians", ResearchCategory.Economic | ResearchCategory.Operational, "building.mar.routing_laboratory", 100, 20, 0, 40,
                unlocks: Unlock("capability.mar.worker_robot.station_area_automatic_maintenance"),
                modifiers: Mods(Set("parameter.mar.worker_robot.automatic_maintenance_hp_per_second", 3))),
            Def("research.mar.pressure_equalization_valves", "Martians", ResearchCategory.Economic, "building.mar.routing_laboratory", 130, 40, 1, 50,
                modifiers: Mods(Set("parameter.mar.tube_network.active_links_per_energy_demand", 2))),
            Def("research.mar.redundant_routing", "Martians", ResearchCategory.FactionSystem, "building.mar.routing_laboratory", 150, 45, 1, 50,
                unlocks: Unlock("capability.mar.tube_network.automatic_rerouting"),
                modifiers: Mods(Add("parameter.mar.station.connection_capacity", 1))),
            Def("research.mar.utility_mechanisms", "Martians", ResearchCategory.FactionSystem | ResearchCategory.Combat, "building.mar.routing_laboratory", 170, 50, 2, 55,
                unlocks: Unlock("building.mar.deflector_arm", "capability.mar.red_planet_protector.guard_sweep", "capability.mar.red_planet_protector.positional_resistance")),
            Def("research.mar.walker_articulation", "Martians", ResearchCategory.Operational | ResearchCategory.Combat, "building.mar.routing_laboratory", 160, 45, 1, 55,
                unlocks: Unlock("unit.martians.recon_mech_rp", "unit.martians.red_planet_protector")),

            Def("research.rr.advanced_power_distribution", "RockRaiders", ResearchCategory.Economic, "building.rock_raiders.engineering_workshop", 160, 50, 1, 55,
                modifiers: Mods(Add("parameter.rr.power_station.energy_generation_per_second", 3))),
            Def("research.rr.cutter_package", "RockRaiders", ResearchCategory.Combat, "building.rock_raiders.vehicle_service_bay", 90, 20, 0, 35,
                unlocks: Unlock("capability.rr.loader_dozer.cutter_package")),
            Def("research.rr.deep_core_engineering", "RockRaiders", ResearchCategory.Progression, "building.rock_raiders.crystal_vault", 220, 90, 4, 75,
                Reqs(Group(Building("building.rock_raiders.engineering_workshop")), Group(Building("building.rock_raiders.crystal_vault"))),
                Unlock("unit.rock_raiders.chrome_crusher", "unit.rock_raiders.tunnel_transport")),
            Def("research.rr.geological_survey_calibration", "RockRaiders", ResearchCategory.Economic, "building.rock_raiders.hq", 80, 20, 0, 35,
                unlocks: Unlock("capability.rr.hover_scout.extended_resource_and_excavation_survey")),
            Def("research.rr.high_capacity_processing", "RockRaiders", ResearchCategory.Economic, "building.rock_raiders.engineering_workshop", 140, 35, 0, 50,
                unlocks: Unlock("capability.rr.ore_processing_plant.full_rich_ore_exploitation"),
                modifiers: Mods(Set("parameter.rr.ore_processing_plant.active_raw_batches", 2))),
            Def("research.rr.industrial_expansion_program", "RockRaiders", ResearchCategory.Progression, "building.rock_raiders.hq", 150, 50, 0, 55,
                Reqs(
                    Group(Building("building.rock_raiders.ore_processing_plant")),
                    Group(Building("building.rock_raiders.power_station")),
                    Group(Building("building.rock_raiders.vehicle_service_bay"))),
                Unlock("building.rock_raiders.crystal_vault", "building.rock_raiders.cutter_mast", "building.rock_raiders.engineering_workshop")),
            Def("research.rr.reinforced_drilling_assemblies", "RockRaiders", ResearchCategory.Combat | ResearchCategory.Operational, "building.rock_raiders.engineering_workshop", 160, 45, 1, 55,
                unlocks: Unlock("capability.rr.drill_craft.reinforced_excavation", "unit.rock_raiders.granite_grinder")),
            Def("research.rr.service_gantries", "RockRaiders", ResearchCategory.Operational, "building.rock_raiders.vehicle_service_bay", 140, 40, 0, 50,
                modifiers: Mods(Set("parameter.rr.vehicle_service_bay.concurrent_repair_jobs", 2))),
            Def("research.rr.worksite_automation", "RockRaiders", ResearchCategory.FactionSystem, "building.rock_raiders.ore_processing_plant", 120, 30, 0, 45,
                modifiers: Mods(Set("parameter.rr.ore_processing_plant.automated_receiving_lanes", 2)))
        };
    }

    private static ResearchDefinition Def(string key, string faction, ResearchCategory categories, string sourceBuilding,
        ushort ore, ushort energy, byte crystals, ushort seconds, ResearchPrerequisiteGroup[]? requirements = null,
        string[]? unlocks = null, ResearchParameterModifier[]? modifiers = null)
    {
        return new ResearchDefinition(key, faction, categories, sourceBuilding, ore, energy, crystals,
            checked((ushort)(seconds * SimClock.TicksPerSecond)), requirements, unlocks, modifiers, string.Empty,
            "presentation." + key, "loc." + key + ".name");
    }

    private static ResearchPrerequisiteDefinition Research(string key) =>
        new ResearchPrerequisiteDefinition(ResearchPrerequisiteKind.Research, key);
    private static ResearchPrerequisiteDefinition Building(string key) =>
        new ResearchPrerequisiteDefinition(ResearchPrerequisiteKind.Building, key);
    private static ResearchPrerequisiteDefinition State(string key, ushort minimum,
        ResearchPrerequisitePersistence persistence = ResearchPrerequisitePersistence.AtStart) =>
        new ResearchPrerequisiteDefinition(ResearchPrerequisiteKind.StateThreshold, key, minimum, persistence);
    private static ResearchPrerequisiteGroup Group(params ResearchPrerequisiteDefinition[] alternatives) =>
        new ResearchPrerequisiteGroup(alternatives);
    private static ResearchPrerequisiteGroup[] Req(params ResearchPrerequisiteDefinition[] requirements)
    {
        ResearchPrerequisiteGroup[] groups = new ResearchPrerequisiteGroup[requirements.Length];
        for (int i = 0; i < requirements.Length; i++) groups[i] = Group(requirements[i]);
        return groups;
    }
    private static ResearchPrerequisiteGroup[] Reqs(params ResearchPrerequisiteGroup[] groups) => groups;
    private static string[] Unlock(params string[] tags) => tags;
    private static ResearchParameterModifier[] Mods(params ResearchParameterModifier[] modifiers) => modifiers;
    private static ResearchParameterModifier Add(string key, int value) => new ResearchParameterModifier(key, ResearchModifierOperation.Add, value);
    private static ResearchParameterModifier Set(string key, int value) => new ResearchParameterModifier(key, ResearchModifierOperation.Set, value);
    private static ResearchParameterModifier Multiply(string key, int basisPoints) => new ResearchParameterModifier(key, ResearchModifierOperation.MultiplyBasisPoints, basisPoints);
}
}
