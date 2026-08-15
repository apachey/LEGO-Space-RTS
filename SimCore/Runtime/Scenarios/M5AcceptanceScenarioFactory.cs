using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>
/// Deterministic developer-only handoff fixture for the complete M5 four-faction
/// system proof. It uses authoritative systems and canonical timings, but it is
/// not normal skirmish content or a substitute for the later full-roster import.
/// </summary>
public static class M5AcceptanceScenarioFactory
{
    public const string StableMapKey = "map.dev.m5_acceptance";
    public const ushort ExcavatableFeatureId = 501;

    public const string T3TrikeKey = "unit.ast.t3_trike";
    public const string ResonanceCoreKey = "building.ali.resonance_core";
    public const string AeroTubeHangarKey = "building.mar.aero_tube_hangar";
    public const string SettlementStationKey = "building.mar.settlement_station";
    public const string WorkerRobotKey = "unit.mar.worker_robot";
    public const string DoubleHoverKey = "unit.mar.double_hover";
    public const string JetScooterKey = "unit.mar.jet_scooter";
    public const string DisplacementSourceKey = "unit.mar.excavation_searcher";
    public const string DisplacementTargetKey = "unit.ast.t3_trike.displacement_target";
    public const string ExcavationRunnerKey = "unit.rock_raiders.hover_scout.m5_excavation_runner";

    private const string ServiceHubKey = "building.ast.service_refit_hub";
    private const string CommandCoreKey = "building.ali.etx_command_core";
    public static SimulationWorld Create(PrototypeContentCatalog? content = null, bool applyInitialDisplacement = true, bool openExcavation = true)
    {
        PrototypeContentCatalog catalog = content ?? PrototypeContentFactory.CreateM2Catalog();
        MapGrid map = new(StableMapKey);
        map.AddExcavatable(new ExcavatableFeature(
            "map.feature.m5_acceptance.fractured_wall",
            ExcavatableFeatureId,
            new IntRect(116, 30, 4, 36),
            ExcavatableTerrainClass.FracturedRockWall,
            requiredEnergy: 25,
            StableId.FromKey("view.placeholder.excavatable.fractured_rock_wall"),
            openBuildable: false));

        SimulationWorld world = new(map, 2, catalog);
        ExcavationTopologySystem.InitializeFeatures(world);

        AddRockRaiderWorksite(world);
        WorksiteGraphSystem.InitializeOpening(world);

        EntityId t3 = AddAstronautRefitProof(world);
        EntityId resonanceCore = AddAlienChargeProof(world);
        (EntityId tubeOrigin, EntityId tubeDestination, EntityId[] passengers) = AddMartianTubeProof(world);
        (EntityId displacementSource, EntityId displacementTarget) = AddDisplacementProof(world);
        EntityId excavationRunner = AddUnit(world, ExcavationRunnerKey, 0, FixVec2.FromInts(52, 24), FootprintClass.Small, SelectableKind.CombatSupport, 9);

        SimulationRunner bootstrap = new(world);
        ref MissionRefitState refit = ref world.Entities.MissionRefitState.Get(t3);
        refit.SurveyUnlocked = true;
        if (!MissionRefitSystem.TryStartT3Refit(world, 0, t3, MissionConfiguration.T3Survey))
            throw new InvalidOperationException("M5 acceptance fixture could not start Mission Refit.");

        AlienChargeSystem.RecalculatePlayer(world, 0);
        ref AlienChargeState charge = ref world.GetAlienChargeRef(0);
        charge.CurrentMillicharge = charge.MaximumMillicharge;
        charge.ResonanceInitiationUnlocked = true;
        if (!AlienChargeSystem.TryStartSurge(world, 0, resonanceCore))
            throw new InvalidOperationException("M5 acceptance fixture could not start Surge.");

        for (int i = 0; i < passengers.Length; i++)
            if (!TubeTransferSystem.TryQueueTransfer(world, 0, passengers[i], tubeOrigin, tubeDestination))
                throw new InvalidOperationException("M5 acceptance fixture could not queue an Aero Tube transfer.");

        if (applyInitialDisplacement && !DisplacementSystem.TryApply(world, displacementSource, displacementTarget, DisplacementEffect.ExcavationClamp,
                DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out _))
            throw new InvalidOperationException("M5 acceptance fixture could not apply displacement and Stability.");

        if (openExcavation)
        {
            if (!world.OpenExcavatable(ExcavatableFeatureId))
                throw new InvalidOperationException("M5 acceptance fixture could not open its excavation route.");
            CommandExecutionSystem.SetMove(world, excavationRunner, FixVec2.FromInts(68, 24));
        }

        world.Spatial.Rebuild(world.Entities);
        return world;
    }

    public static bool IsFreshHandoffReady(SimulationWorld world, out string reason)
    {
        if (world.Map.StableKey != StableMapKey) { reason = "wrong scenario"; return false; }
        if (WorksiteGraphSystem.GetPlayerComponents(world, 0).Length == 0) { reason = "Worksite graph absent"; return false; }
        if (!TryFindFirst(world, T3TrikeKey, out EntityId t3) || !world.Entities.MissionRefitJob.Has(t3)) { reason = "Mission Refit inactive"; return false; }
        if (!TryFindFirst(world, ResonanceCoreKey, out EntityId core) || !world.Entities.SurgeZone.Has(core)) { reason = "Surge inactive"; return false; }
        int transfers = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.TubeTransfer.Has(alive[i])) transfers++;
        if (transfers != 3) { reason = $"expected 3 Tube transfers, found {transfers}"; return false; }
        if (!TryFindFirst(world, DisplacementTargetKey, out EntityId target) || !DisplacementSystem.IsStable(world, target)) { reason = "Stability inactive"; return false; }
        if (!world.Map.TryGetFeature(ExcavatableFeatureId, out ExcavatableFeature feature) || !feature.Open) { reason = "excavation route closed"; return false; }
        if (!TryFindFirst(world, ExcavationRunnerKey, out EntityId runner) || !world.Entities.Navigation.TryGet(runner, out NavigationAgent runnerNavigation) || !runnerNavigation.HasTarget)
        { reason = "excavation route demonstrator absent"; return false; }
        reason = string.Empty;
        return true;
    }

    public static bool TryRepeatDisplacement(SimulationWorld world, out FixVec2 resolvedPosition)
    {
        resolvedPosition = default;
        return TryFindFirst(world, DisplacementSourceKey, out EntityId source) &&
            TryFindFirst(world, DisplacementTargetKey, out EntityId target) &&
            DisplacementSystem.TryApply(world, source, target, DisplacementEffect.ExcavationClamp,
                DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out resolvedPosition);
    }

    public static bool TryOpenExcavationAndMove(SimulationWorld world)
    {
        if (!world.Map.TryGetFeature(ExcavatableFeatureId, out ExcavatableFeature feature) || feature.Open ||
            !TryFindFirst(world, ExcavationRunnerKey, out EntityId runner) || !world.OpenExcavatable(ExcavatableFeatureId)) return false;
        CommandExecutionSystem.SetMove(world, runner, FixVec2.FromInts(68, 24));
        world.Spatial.Rebuild(world.Entities);
        return true;
    }

    public static bool TryFindFirst(SimulationWorld world, string stableKey, out EntityId entity)
    {
        ContentId type = StableId.FromKey(stableKey);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Selectable.TryGet(alive[i], out Selectable selectable) && selectable.ContentType == type)
            { entity = alive[i]; return true; }
        entity = EntityId.None;
        return false;
    }

    private static void AddRockRaiderWorksite(SimulationWorld world)
    {
        EntityId hq = AddBuilding(world, "building.rock_raiders.hq", 0, 24, 24, 8, 8);
        EntityId bay = AddBuilding(world, "building.rock_raiders.vehicle_service_bay", 0, 47, 24, 8, 6);
        AddBuilding(world, "building.rock_raiders.power_station", 0, 36, 32, 5, 5);
        AddBuilding(world, "building.rock_raiders.ore_processing_plant", 0, 32, 18, 6, 6);
        world.Entities.ResourceBank.Set(hq, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = 275 });
        world.Entities.ResourceBank.Set(bay, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = 125 });
    }

    private static EntityId AddAstronautRefitProof(SimulationWorld world)
    {
        EntityId hub = AddBuilding(world, ServiceHubKey, 0, 112, 28, 7, 7);
        EntityId supportPower = AddBuilding(world, "building.rock_raiders.power_station", 0, 98, 28, 5, 5);
        world.Entities.EnergyDomainMember.Set(hub, new EnergyDomainMember { DomainRoot = hub });
        world.Entities.EnergyDomainMember.Set(supportPower, new EnergyDomainMember { DomainRoot = hub });
        world.Entities.EnergyDomain.Set(hub, new EnergyDomain { Reserve = Fix32.FromInt(120), ReserveCapacity = Fix32.FromInt(150), GenerationPerSecond = 12 });
        EntityId bank = world.Entities.Create();
        world.Entities.Ownership.Set(bank, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(bank, new SimTransform { Position = FixVec2.FromInts(105, 36), Orientation = Angle16.Zero });
        world.Entities.ResourceBank.Set(bank, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = 200 });
        EntityId t3 = AddUnit(world, T3TrikeKey, 0, FixVec2.FromInts(106, 28), FootprintClass.Medium, SelectableKind.CombatSupport, 9);
        MissionRefitSystem.EnsureState(world, t3);
        return t3;
    }

    private static EntityId AddAlienChargeProof(SimulationWorld world)
    {
        EntityId commandCore = AddBuilding(world, CommandCoreKey, 0, 104, 78, 7, 7);
        EntityId core = AddBuilding(world, ResonanceCoreKey, 0, 116, 78, 6, 6);
        EntityId supportPowerA = AddBuilding(world, "building.rock_raiders.power_station", 0, 94, 72, 5, 5);
        EntityId supportPowerB = AddBuilding(world, "building.rock_raiders.power_station", 0, 94, 84, 5, 5);
        world.Entities.EnergyDomainMember.Set(commandCore, new EnergyDomainMember { DomainRoot = commandCore });
        world.Entities.EnergyDomainMember.Set(core, new EnergyDomainMember { DomainRoot = commandCore });
        world.Entities.EnergyDomainMember.Set(supportPowerA, new EnergyDomainMember { DomainRoot = commandCore });
        world.Entities.EnergyDomainMember.Set(supportPowerB, new EnergyDomainMember { DomainRoot = commandCore });
        world.Entities.EnergyDomain.Set(commandCore, new EnergyDomain { Reserve = Fix32.FromInt(120), ReserveCapacity = Fix32.FromInt(150), GenerationPerSecond = 15 });
        EntityId crystalBank = world.Entities.Create();
        world.Entities.Ownership.Set(crystalBank, new Ownership { PlayerSlot = 0 });
        world.Entities.ResourceBank.Set(crystalBank, new ResourceBank { Type = ResourceType.Crystal, ProcessedAmount = 2 });
        if (!ResonanceCoreSystem.TryAttachCore(world, core, commandCore))
            throw new InvalidOperationException("M5 acceptance fixture could not attach its Resonance Core.");
        ref ResonanceCore resonance = ref world.Entities.ResonanceCore.Get(core);
        resonance.CommittedSlotMask = 0b0000_1111;
        resonance.DesiredCommittedCrystals = 4;
        EntityId receiver = AddUnit(world, "unit.ali.razor_skimmer", 0, FixVec2.FromInts(121, 84), FootprintClass.Small, SelectableKind.CombatSupport, 9);
        world.Entities.SurgeReceiver.Set(receiver, new SurgeReceiver());
        return core;
    }

    private static (EntityId Origin, EntityId Destination, EntityId[] Passengers) AddMartianTubeProof(SimulationWorld world)
    {
        EntityId origin = AddBuilding(world, AeroTubeHangarKey, 0, 26, 116, 9, 9);
        EntityId destination = AddBuilding(world, SettlementStationKey, 0, 75, 116, 7, 7);
        if (!TubeGraphSystem.TryRegisterStation(world, origin) || !TubeGraphSystem.TryRegisterStation(world, destination) ||
            !TubeGraphSystem.TryAddCompletedLink(world, 0, origin, destination, out EntityId link))
            throw new InvalidOperationException("M5 acceptance fixture could not create its Aero Tube graph.");
        if (!TubeGraphSystem.TryGetStationSocket(world, link, origin, out FixVec2 socket))
            throw new InvalidOperationException("M5 acceptance fixture could not resolve its Tube socket.");
        EntityId[] passengers =
        {
            AddUnit(world, WorkerRobotKey, 0, socket, FootprintClass.Tiny, SelectableKind.Worker, 7),
            AddUnit(world, DoubleHoverKey, 0, socket + new FixVec2(Fix32.Zero, Fix32.Half), FootprintClass.Small, SelectableKind.CombatSupport, 9),
            AddUnit(world, JetScooterKey, 0, socket + new FixVec2(Fix32.Zero, -Fix32.Half), FootprintClass.Small, SelectableKind.CombatSupport, 9)
        };
        return (origin, destination, passengers);
    }

    private static (EntityId Source, EntityId Target) AddDisplacementProof(SimulationWorld world)
    {
        EntityId source = AddUnit(world, DisplacementSourceKey, 0, FixVec2.FromInts(110, 125), FootprintClass.Huge, SelectableKind.CombatSupport, 14);
        EntityId target = AddUnit(world, DisplacementTargetKey, 1, FixVec2.FromInts(118, 125), FootprintClass.Medium, SelectableKind.CombatSupport, 9);
        return (source, target);
    }

    private static EntityId AddBuilding(SimulationWorld world, string stableKey, byte owner, short centerX, short centerY, byte width, byte height)
    {
        EntityId id = world.Entities.Create();
        ContentId type = StableId.FromKey(stableKey);
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = FixVec2.FromInts(centerX, centerY), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building
        {
            Type = type,
            AnchorX = checked((short)(centerX - width / 2)),
            AnchorY = checked((short)(centerY - height / 2)),
            FootprintWidth = width,
            FootprintHeight = height,
            State = BuildingState.Completed
        });
        return id;
    }

    private static EntityId AddUnit(SimulationWorld world, string stableKey, byte owner, FixVec2 position,
        FootprintClass footprint, SelectableKind kind, byte vision)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = StableId.FromKey(stableKey), Kind = kind });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = footprint, Layer = MovementLayer.Ground, Target = position, PathTopologyVersion = world.Map.TopologyVersion });
        world.Entities.Movement.Set(id, new Movement
        {
            MaxSpeed = Fix32.FromRatio(17, 10), Acceleration = Fix32.FromInt(4), Deceleration = Fix32.FromInt(4),
            TurnRatePerTick = 1638, ReversePolicy = ReversePolicy.Full, LastPosition = position
        });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = vision, LastFogX = -1, LastFogY = -1 });
        world.GetQueue(id);
        return id;
    }
}
}
