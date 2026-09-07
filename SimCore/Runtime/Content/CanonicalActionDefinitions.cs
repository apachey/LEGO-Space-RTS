using System;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Canon-only T073 bindings layered onto the T070-T072 roster data.</summary>
public static class CanonicalActionDefinitions
{
    public static BuildingDefinition[] BindBuildingPrerequisites(BuildingDefinition[] source)
    {
        BuildingDefinition[] result = new BuildingDefinition[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            BuildingDefinition b = source[i];
            result[i] = new BuildingDefinition(b.StableKey, b.FootprintWidth, b.FootprintHeight, b.FootprintMask, b.Rotatable,
                b.OreCost, b.EnergyCost, b.BuildTicks, b.ProductionExitWidth, b.ProductionExitDepth, b.ProductionExitFootprint,
                b.OperationsCapacityProvided, b.EnergyGenerationPerSecond, b.EnergyReserveCapacity, b.ContinuousEnergyDemandPerSecond,
                b.EnergyFunctionalClass, b.WorksiteServiceRadius, b.CrystalCost, b.FootprintMaskHigh, BuildingPrerequisites(b.StableKey));
        }
        return result;
    }

    public static ContentActionPrerequisiteGroup[] BuildingPrerequisites(string key) => key switch
    {
        "building.rock_raiders.ore_processing_plant" or
        "building.rock_raiders.power_station" or
        "building.rock_raiders.vehicle_service_bay" or
        "building.rock_raiders.crusher_barrier" => All(B("building.rock_raiders.hq")),
        "building.rock_raiders.engineering_workshop" or
        "building.rock_raiders.crystal_vault" => All(R("research.rr.industrial_expansion_program")),
        "building.rock_raiders.cutter_mast" => All(R("research.rr.industrial_expansion_program"), B("building.rock_raiders.power_station")),

        "building.ast.field_systems_garage" or
        "building.ast.solar_energy_array" or
        "building.ast.frontier_extraction_station" => All(B("building.ast.mb01_eagle_command_base")),
        "building.ast.mission_vehicle_bay" or
        "building.ast.flight_operations_pad" => All(R("research.ast.mission_operations_integration")),
        "building.ast.service_refit_hub" => new[]
        {
            One(B("building.ast.field_systems_garage")),
            Any(B("building.ast.solar_energy_array"), B("building.ast.frontier_extraction_station"))
        },
        "building.ast.modular_sentinel_defense" => All(B("building.ast.service_refit_hub")),

        "building.ali.etx_fabricator" or
        "building.ali.power_coupler" => All(B("building.ali.etx_command_core")),
        "building.ali.resonance_core" or
        "building.ali.etx_defense_node" => All(B("building.ali.etx_command_core"), B("building.ali.power_coupler")),
        "building.ali.reconfiguration_dock" => All(R("research.ali.etx_reconfiguration_matrix")),

        "building.mar.mechanical_workshop" or
        "building.mar.pressure_generator" or
        "building.mar.excavation_plant" => new[] { Any(B("building.mar.aero_tube_hangar"), B("building.mar.settlement_station")) },
        "building.mar.routing_laboratory" => All(B("building.mar.mechanical_workshop"), B("building.mar.pressure_generator"), B("building.mar.excavation_plant")),
        "building.mar.deflector_arm" => All(R("research.mar.utility_mechanisms")),
        "building.mar.aero_guard_tower" => All(B("building.mar.routing_laboratory")),
        // Aero Tube Links use the dedicated two-station TubeBuild command and
        // its length-based authoritative graph rules, not generic placement.
        _ => Array.Empty<ContentActionPrerequisiteGroup>()
    };

    public static UnitProductionDefinition[] CreateProduction()
    {
        UnitProductionDefinition[] result =
        {
            P("unit.rock_raiders.crew", Producers("building.rock_raiders.hq"), 50, 0, 0, 1, 16),
            P("unit.rock_raiders.hover_scout", Producers("building.rock_raiders.vehicle_service_bay"), 75, 10, 0, 1, 20),
            P("unit.rock_raiders.rapid_rider", Producers("building.rock_raiders.vehicle_service_bay"), 90, 10, 0, 2, 28),
            P("unit.rock_raiders.loader_dozer", Producers("building.rock_raiders.vehicle_service_bay"), 125, 15, 0, 3, 36),
            P("unit.rock_raiders.drill_craft", Producers("building.rock_raiders.engineering_workshop"), 95, 15, 0, 2, 28, R("research.rr.industrial_expansion_program")),
            P("unit.rock_raiders.granite_grinder", Producers("building.rock_raiders.engineering_workshop"), 190, 35, 1, 4, 50, R("research.rr.reinforced_drilling_assemblies")),
            P("unit.rock_raiders.chrome_crusher", Producers("building.rock_raiders.engineering_workshop"), 330, 90, 4, 6, 78, R("research.rr.deep_core_engineering")),
            P("unit.rock_raiders.tunnel_transport", Producers("building.rock_raiders.engineering_workshop"), 300, 110, 3, 5, 75, R("research.rr.deep_core_engineering")),

            P("unit.astronauts.expedition_crew", Producers("building.ast.mb01_eagle_command_base"), 50, 0, 0, 1, 16),
            P("unit.astronauts.rover", Producers("building.ast.field_systems_garage"), 70, 5, 0, 1, 18),
            P("unit.astronauts.t3_trike", Producers("building.ast.field_systems_garage"), 110, 15, 0, 2, 30),
            P("unit.astronauts.mono_jet", Producers("building.ast.mb01_eagle_command_base"), 100, 35, 0, 1, 32, B("building.ast.solar_energy_array")),
            P("unit.astronauts.solar_explorer", Producers("building.ast.field_systems_garage"), 180, 40, 1, 4, 50, R("research.ast.field_sustainment_package")),
            P("unit.astronauts.mission_fighter", Producers("building.ast.flight_operations_pad"), 140, 55, 1, 2, 40, R("research.ast.aerospace_coordination")),
            P("unit.astronauts.mx41_switch_fighter", Producers("building.ast.mission_vehicle_bay"), 170, 60, 1, 3, 45, R("research.ast.switchframe_actuation")),
            P("unit.astronauts.mobile_mining_platform", Producers("building.ast.mission_vehicle_bay"), 170, 25, 0, 3, 48, R("research.ast.mission_operations_integration")),
            P("unit.astronauts.mx71_recon_dropship", Producers("building.ast.flight_operations_pad"), 200, 70, 1, 4, 55, R("research.ast.aerospace_coordination")),
            P("unit.astronauts.mt51_claw_tank", Producers("building.ast.mission_vehicle_bay"), 180, 35, 0, 3, 45, R("research.ast.mission_operations_integration")),
            P("unit.astronauts.mt101_armored_drilling_unit", Producers("building.ast.mission_vehicle_bay"), 280, 70, 2, 5, 65, R("research.ast.heavy_mission_chassis")),
            P("unit.astronauts.mt201_ultra_drill_walker", Producers("building.ast.mission_vehicle_bay"), 320, 90, 3, 6, 75, R("research.ast.deep_mission_drilling")),
            P("unit.astronauts.mx81_operations_aircraft", Producers("building.ast.flight_operations_pad"), 380, 130, 4, 6, 90, R("research.ast.integrated_expedition_command")),

            P("unit.aliens.etx_servitor", Producers("building.ali.etx_command_core", "building.ali.etx_fabricator"), 50, 5, 0, 1, 16),
            P("unit.aliens.alien_jet", Producers("building.ali.etx_fabricator"), 95, 35, 0, 2, 28),
            P("unit.aliens.razor_skimmer", Producers("building.ali.etx_fabricator"), 105, 25, 0, 2, 30),
            P("unit.aliens.etx_alien_strike", Producers("building.ali.reconfiguration_dock"), 190, 60, 1, 4, 50, R("research.ali.siege_phase_coupling")),
            P("unit.aliens.etx_alien_infiltrator", Producers("building.ali.reconfiguration_dock"), 210, 65, 2, 4, 55, R("research.ali.infiltration_matrix")),
            P("unit.aliens.alien_mothership", Producers("building.ali.reconfiguration_dock"), 420, 160, 6, 8, 95, R("research.ali.advanced_resonance_architecture")),

            P("unit.martians.worker_robot", Producers("building.mar.aero_tube_hangar", "building.mar.settlement_station"), 50, 0, 0, 1, 16),
            P("unit.martians.double_hover", Producers("building.mar.mechanical_workshop"), 65, 10, 0, 1, 18),
            P("unit.martians.jet_scooter", Producers("building.mar.mechanical_workshop"), 90, 15, 0, 1, 25),
            P("unit.martians.aero_skiff", Producers("building.mar.aero_tube_hangar"), 120, 40, 0, 2, 35, R("research.mar.aero_handling_decks")),
            P("unit.martians.red_planet_cruiser", Producers("building.mar.mechanical_workshop"), 150, 25, 0, 3, 40, B("building.mar.routing_laboratory")),
            P("unit.martians.recon_mech_rp", Producers("building.mar.mechanical_workshop"), 170, 35, 1, 3, 45, R("research.mar.walker_articulation")),
            P("unit.martians.red_planet_protector", Producers("building.mar.mechanical_workshop"), 220, 50, 2, 4, 55, R("research.mar.walker_articulation")),
            P("unit.martians.excavation_searcher", Producers("building.mar.mechanical_workshop"), 330, 80, 3, 6, 78,
                R("research.mar.grand_network_integration"), R("research.mar.advanced_excavation_systems"))
        };
        Array.Sort(result, (a, b) => string.CompareOrdinal(a.UnitStableKey, b.UnitStableKey));
        return result;
    }

    private static UnitProductionDefinition P(string unit, string[] producers, ushort ore, ushort energy, byte crystals, byte oc, ushort seconds,
        params ContentActionPrerequisite[] requirements)
        => new(unit, producers, ore, energy, crystals, oc, checked((ushort)(seconds * 20)), All(requirements));

    private static string[] Producers(params string[] keys) => keys;
    private static ContentActionPrerequisite B(string key) => new(ContentActionPrerequisiteKind.Building, key);
    private static ContentActionPrerequisite R(string key) => new(ContentActionPrerequisiteKind.Research, key);
    private static ContentActionPrerequisiteGroup One(ContentActionPrerequisite requirement) => new(new[] { requirement });
    private static ContentActionPrerequisiteGroup Any(params ContentActionPrerequisite[] alternatives) => new(alternatives);
    private static ContentActionPrerequisiteGroup[] All(params ContentActionPrerequisite[] requirements)
    {
        ContentActionPrerequisiteGroup[] result = new ContentActionPrerequisiteGroup[requirements.Length];
        for (int i = 0; i < requirements.Length; i++) result[i] = One(requirements[i]);
        return result;
    }
}
}
