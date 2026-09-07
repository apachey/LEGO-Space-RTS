using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public static class ScenarioFactory
{
    private const int CanonicalStartingCrewPerPlayer = 6;
    public const int Stress60FirstMoveTick = 1;
    public const int Stress60SecondMoveTick = 7000;
    public const int Stress60TopologyOpenTick = 16000;
    public const int Stress60ThirdMoveTick = 16001;
    public const int Stress60FinalEvaluationTick = 26000;
    public const int Stress60TravelAllowanceMultiplier = 2;

    public static SimulationWorld CreateFirstControllable(int count = 18)
        => CreateFirstControllable(DevMapFactory.CreateDefinition(), PrototypeContentFactory.CreateM2Catalog(), count);

    public static SimulationWorld CreateCanonicalOpening()
        => CreateCanonicalOpening(DevMapFactory.CreateDefinition(), PrototypeContentFactory.CreateM2Catalog());

    public static SimulationWorld CreateCanonicalOpening(MapDefinition definition, PrototypeContentCatalog content)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (content == null) throw new ArgumentNullException(nameof(content));
        SimulationWorld world = new(definition.Grid, 2, content);
        int[] crewCounts = new int[world.PlayerCount];
        for (int i = 0; i < definition.InitialEntities.Length; i++)
        {
            InitialEntitySpawn authored = definition.InitialEntities[i];
            if (authored.PlayerSlot >= crewCounts.Length || crewCounts[authored.PlayerSlot] >= CanonicalStartingCrewPerPlayer) continue;
            SpawnCanonicalCrew(world, authored.PlayerSlot, authored.Position, content);
            crewCounts[authored.PlayerSlot]++;
        }
        for (int player = 0; player < crewCounts.Length; player++)
            if (crewCounts[player] != CanonicalStartingCrewPerPlayer) throw new InvalidOperationException($"Canonical opening requires six Crew spawn positions for player {player}.");
        SpawnScenarioResourcesAndReceivers(world, definition, content);
        FinalizeScenario(world);
        return world;
    }

    public static SimulationWorld CreateFirstControllable(MapDefinition definition, PrototypeContentCatalog content, int count = 18)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (content == null) throw new ArgumentNullException(nameof(content));
        SimulationWorld world = new(definition.Grid, 2, content);
        int player0Remaining = count;
        int player1Remaining = Math.Min(8, count);
        for (int i = 0; i < definition.InitialEntities.Length; i++)
        {
            InitialEntitySpawn spawn = definition.InitialEntities[i];
            if (spawn.PlayerSlot == 0 && player0Remaining <= 0) continue;
            if (spawn.PlayerSlot == 1 && player1Remaining <= 0) continue;
            SpawnAuthored(world, spawn, content);
            if (spawn.PlayerSlot == 0) player0Remaining--; else if (spawn.PlayerSlot == 1) player1Remaining--;
        }
        SpawnScenarioResourcesAndReceivers(world, definition, content);
        FinalizeScenario(world);
        return world;
    }

    public static SimulationWorld CreateStress60()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        SpawnGrid(world, content, 0, 60, 20, 58, 10, 4);
        FinalizeScenario(world);
        EntityId[] ids = OwnedIds(world, 0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(Stress60FirstMoveTick), 0, 1, SimCommandType.Move, ids, FixVec2.FromInts(138, 80)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(Stress60SecondMoveTick), 0, 2, SimCommandType.Move, ids, FixVec2.FromInts(20, 112)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(Stress60TopologyOpenTick), 0, 3, SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: DevMapFactory.ExcavatableFeatureId));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(Stress60ThirdMoveTick), 0, 4, SimCommandType.Move, ids, FixVec2.FromInts(138, 135)));
        return world;
    }

    public static SimulationWorld CreateRepresentative24()
    {
        SimulationWorld world=new(DevMapFactory.Create(),2);PrototypeContentCatalog content=PrototypeContentFactory.CreateM2Catalog();
        SpawnRepresentativeCohort(world,content,0,10,18,64,4,4);
        SpawnRepresentativeCohort(world,content,0,10,138,99,4,4);
        SpawnPrototypeMover(world,content,0,FootprintClass.Small,FixVec2.FromInts(65,90));
        SpawnPrototypeMover(world,content,0,FootprintClass.Huge,FixVec2.FromInts(69,90));
        SpawnPrototypeMover(world,content,0,FootprintClass.Small,FixVec2.FromInts(91,90));
        SpawnPrototypeMover(world,content,0,FootprintClass.Huge,FixVec2.FromInts(95,90));
        FinalizeScenario(world);
        return world;
    }

    public static ReplayLog CreateGoldenReplay()
    {
        SimulationWorld initial = CreateFirstControllable(12); ReplayLog replay = new(SnapshotSerializer.Serialize(initial)); EntityId[] ids = OwnedIds(initial, 0);
        replay.Commands.Add(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Move, ids, FixVec2.FromInts(132, 30)));
        replay.Commands.Add(new CommandEnvelope(new SimTick(450), 0, 2, SimCommandType.Move, ids, FixVec2.FromInts(132, 68), CommandModifiers.Queue));
        replay.Commands.Add(new CommandEnvelope(new SimTick(900), 0, 3, SimCommandType.Move, ids, FixVec2.FromInts(24, 105)));
        replay.Commands.Add(new CommandEnvelope(new SimTick(1400), 0, 4, SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: DevMapFactory.ExcavatableFeatureId));
        replay.Commands.Add(new CommandEnvelope(new SimTick(1401), 0, 5, SimCommandType.Move, ids, FixVec2.FromInts(132, 135)));
        replay.Commands.Add(new CommandEnvelope(new SimTick(2300), 0, 6, SimCommandType.Stop, new[] { ids[0], ids[1] }, FixVec2.Zero));
        replay.Commands.Add(new CommandEnvelope(new SimTick(2400), 0, 7, SimCommandType.HoldPosition, new[] { ids[2], ids[3] }, FixVec2.Zero));
        return replay;
    }

    public static EntityId[] OwnedIds(SimulationWorld world, byte player)
    {
        List<EntityId> ids = new(); IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Navigation.Has(alive[i]) && world.Entities.Ownership.TryGet(alive[i], out Ownership o) && o.PlayerSlot == player) ids.Add(alive[i]);
        return ids.ToArray();
    }

    private static void SpawnAuthored(SimulationWorld world, InitialEntitySpawn spawn, PrototypeContentCatalog content)
    {
        if(!content.TryGetEntity(spawn.ContentKey,out PrototypeEntityDefinition definition)) throw new InvalidOperationException($"Missing prototype content {spawn.ContentKey}.");
        if(!content.TryGetMovement(definition.MovementProfileKey,out PrototypeMovementProfile profile)) throw new InvalidOperationException($"Missing movement profile {definition.MovementProfileKey}.");
        if(definition.Footprint!=spawn.Footprint||definition.SelectableKind!=spawn.SelectableKind||definition.VisionRadius!=spawn.VisionRadius||profile.Layer!=spawn.Layer) throw new InvalidOperationException($"Authored spawn {spawn.ContentKey} disagrees with compiled content.");
        EntityId id = world.Entities.Create();
        Movement movement = CreateMovement(profile, spawn.Position);
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = spawn.PlayerSlot });
        world.Entities.Transform.Set(id, new SimTransform { Position = spawn.Position, Orientation = Angle16.Zero });
        world.Entities.Movement.Set(id, movement);
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = spawn.Footprint, Layer = spawn.Layer, Target = spawn.Position, PathTopologyVersion = world.Map.TopologyVersion });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = StableId.FromKey(spawn.ContentKey), Kind = spawn.SelectableKind });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = spawn.VisionRadius, LastFogX = -1, LastFogY = -1 });
        AddCombatComponents(world, id, definition);
        AddTransformationComponents(world, id, definition);
        AddWorkerComponents(world, id, definition);
        AddTransportComponents(world, id, definition);
        world.GetQueue(id);
    }

    private static void SpawnCanonicalCrew(SimulationWorld world, byte playerSlot, FixVec2 position, PrototypeContentCatalog content)
    {
        const string crewKey = "unit.rock_raiders.crew";
        if (!content.TryGetEntity(crewKey, out PrototypeEntityDefinition crew) || !content.TryGetMovement(crew.MovementProfileKey, out PrototypeMovementProfile movement))
            throw new InvalidOperationException("Canonical Crew content is missing.");
        SpawnAuthored(world, new InitialEntitySpawn(playerSlot, crewKey, position, crew.Footprint, movement.Layer, crew.SelectableKind, crew.VisionRadius), content);
    }

    private static void SpawnScenarioResourcesAndReceivers(SimulationWorld world, MapDefinition definition, PrototypeContentCatalog content)
    {
        for (int i = 0; i < definition.InitialResourceNodes.Length; i++) SpawnResourceNode(world, definition.InitialResourceNodes[i], content);
        for (int i = 0; i < definition.InitialResourceReceivers.Length; i++) SpawnResourceReceiver(world, definition.InitialResourceReceivers[i], content);
    }

    private static void FinalizeScenario(SimulationWorld world)
    {
        ExcavationTopologySystem.InitializeFeatures(world);
        EnergyDomainSystem.InitializeOpeningDomains(world);
        OperationsCapacitySystem.Recalculate(world);
        world.Spatial.Rebuild(world.Entities);
        new VisionSystem().Step(world);
    }

    private static Movement CreateMovement(PrototypeMovementProfile profile, FixVec2 position)
    {
        return new Movement
        {
            MaxSpeed=profile.MaxSpeed, Acceleration=profile.Acceleration, Deceleration=profile.Deceleration, TurnRatePerTick=profile.TurnRatePerTick, ReversePolicy=profile.ReversePolicy,
            CurrentSpeed=Fix32.Zero, CurrentVelocity=FixVec2.Zero, DesiredMovement=FixVec2.Zero, PathIndex=0, State=MovementState.Idle, StuckTicks=0, CompressionTicks=0, LastPosition=position
        };
    }

    private static void SpawnResourceNode(SimulationWorld world, InitialResourceNodeSpawn spawn, PrototypeContentCatalog content)
    {
        if (!content.TryGetResourceNode(spawn.ContentKey, out ResourceNodeDefinition definition))
            throw new InvalidOperationException($"Missing resource node content {spawn.ContentKey}.");
        EntityId id = world.Entities.Create();
        world.Entities.Transform.Set(id, new SimTransform { Position = spawn.Position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = definition.Id, Kind = SelectableKind.ResourceNode });
        world.Entities.ResourceNode.Set(id, new ResourceNode
        {
            Type = definition.Type,
            DepositSize = definition.DepositSize,
            HarvestInteraction = definition.HarvestInteraction,
            DepletionProfile = definition.DepletionProfile,
            Capacity = definition.Capacity,
            Remaining = definition.Capacity,
            ReducedThresholdBasisPoints = definition.ReducedThresholdBasisPoints,
            LowThresholdBasisPoints = definition.LowThresholdBasisPoints,
            CriticalThresholdBasisPoints = definition.CriticalThresholdBasisPoints
        });
    }

    private static void SpawnGrid(SimulationWorld world, PrototypeContentCatalog content, byte player, int count, int originX, int originY, int columns, int spacing)
    {
        for (int i = 0; i < count; i++)
        {
            int x = originX + (i % columns) * spacing; int y = originY + (i / columns) * spacing;
            SpawnPrototypeMover(world,content,player,(FootprintClass)(i%5),FixVec2.FromInts(x,y));
        }
    }

    private static void SpawnRepresentativeCohort(SimulationWorld world,PrototypeContentCatalog content,byte player,int count,int originX,int originY,int columns,int spacing)
    {
        for(int i=0;i<count;i++)
        {
            FixVec2 position=FixVec2.FromInts(originX+(i%columns)*spacing,originY+(i/columns)*spacing);
            SpawnPrototypeMover(world,content,player,(FootprintClass)(i%5),position);
        }
    }

    private static void SpawnPrototypeMover(SimulationWorld world,PrototypeContentCatalog content,byte player,FootprintClass footprint,FixVec2 position)
    {
        string contentKey=footprint switch
        {
            FootprintClass.Tiny=>"unit.rock_raiders.crew",FootprintClass.Small=>"unit.rock_raiders.hover_scout",FootprintClass.Medium=>"unit.rock_raiders.loader_dozer",
            FootprintClass.Large=>"unit.rock_raiders.chrome_crusher",FootprintClass.Huge=>"prototype.nav.huge",_=>"prototype.nav.huge"
        };
        if(!content.TryGetEntity(contentKey,out PrototypeEntityDefinition definition)||!content.TryGetMovement(definition.MovementProfileKey,out PrototypeMovementProfile profile))throw new InvalidOperationException($"Prototype mover content missing {contentKey}.");
        EntityId id=world.Entities.Create();
        world.Entities.Ownership.Set(id,new Ownership{PlayerSlot=player});world.Entities.Transform.Set(id,new SimTransform{Position=position,Orientation=Angle16.Zero});
        world.Entities.Movement.Set(id,CreateMovement(profile,position));world.Entities.Navigation.Set(id,new NavigationAgent{Footprint=definition.Footprint,Layer=profile.Layer,Target=position,PathTopologyVersion=world.Map.TopologyVersion});
        world.Entities.Selectable.Set(id,new Selectable{IsSelectable=true,ContentType=definition.Id,Kind=definition.SelectableKind});
        world.Entities.Vision.Set(id,new Vision{RadiusBuildCells=definition.VisionRadius,LastFogX=-1,LastFogY=-1});AddCombatComponents(world,id,definition);AddTransformationComponents(world,id,definition);world.GetQueue(id);
        AddWorkerComponents(world,id,definition);
        AddTransportComponents(world,id,definition);
    }

    private static void AddWorkerComponents(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        if (definition.SelectableKind != SelectableKind.Worker) return;
        if (definition.OreTicksPerUnit == 0 || definition.OreCarryCapacity == 0) throw new InvalidOperationException($"Worker content {definition.StableKey} has no Ore harvesting metadata.");
        world.Entities.Worker.Set(id, new Worker { ResourceTarget = EntityId.None, ReceiverTarget = EntityId.None, TaskState = WorkerTaskState.Idle, TicksPerOre = definition.OreTicksPerUnit });
        world.Entities.Builder.Set(id, new Builder { ConstructionTarget = EntityId.None, JobState = BuilderJobState.Idle });
        world.Entities.ResourceCarrier.Set(id, new ResourceCarrier { Type = ResourceType.Ore, Amount = 0, Capacity = definition.OreCarryCapacity });
    }

    private static void SpawnResourceReceiver(SimulationWorld world, InitialResourceReceiverSpawn spawn, PrototypeContentCatalog content)
    {
        if (!content.TryGetEntity(spawn.ContentKey, out PrototypeEntityDefinition definition) || definition.SelectableKind != SelectableKind.Building)
            throw new InvalidOperationException($"Missing resource receiver content {spawn.ContentKey}.");
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = spawn.PlayerSlot });
        world.Entities.Transform.Set(id, new SimTransform { Position = spawn.Position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = definition.Id, Kind = SelectableKind.Building });
        AddCombatComponents(world, id, definition);
        world.Entities.ResourceReceiver.Set(id, new ResourceReceiver { AcceptedType = ResourceType.Ore, PendingHauledAmount = 0, IsHqEmergencyReceiver = true });
        world.Entities.ResourceBank.Set(id, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = 500 });
        if (!content.TryGetBuilding(definition.Id, out BuildingDefinition buildingDefinition)) throw new InvalidOperationException($"Missing building definition {spawn.ContentKey}.");
        byte width = buildingDefinition.RotatedWidth(0), height = buildingDefinition.RotatedHeight(0);
        Building building = new()
        {
            Type = definition.Id,
            AnchorX = checked((short)(spawn.Position.X.FloorToInt() - width / 2)),
            AnchorY = checked((short)(spawn.Position.Y.FloorToInt() - height / 2)),
            Orientation = 0,
            FootprintWidth = width,
            FootprintHeight = height,
            State = BuildingState.Completed
        };
        world.Entities.Building.Set(id, building);
        if (ProductionSystem.IsRuntimeEnabledProducer(content, definition.Id)) world.Entities.Production.Set(id, new Production());
        world.SetConstructionOccupied(building, true);
    }

    internal static EntityId SpawnProducedUnit(SimulationWorld world, byte playerSlot, ContentId unitType, FixVec2 position)
    {
        if (!world.Content.TryGetEntity(unitType, out PrototypeEntityDefinition definition) || definition.SelectableKind == SelectableKind.Building)
            throw new InvalidOperationException($"Missing produced unit content {unitType}.");
        if (!world.Content.TryGetMovement(definition.MovementProfileKey, out PrototypeMovementProfile profile))
            throw new InvalidOperationException($"Missing produced movement profile {definition.MovementProfileKey}.");
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = playerSlot });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Movement.Set(id, CreateMovement(profile, position));
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = definition.Footprint, Layer = profile.Layer, Target = position, PathTopologyVersion = world.Map.TopologyVersion });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = definition.Id, Kind = definition.SelectableKind });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = definition.VisionRadius, LastFogX = -1, LastFogY = -1 });
        AddCombatComponents(world, id, definition);
        AddTransformationComponents(world, id, definition);
        AddWorkerComponents(world, id, definition);
        AddTransportComponents(world, id, definition);
        world.GetQueue(id);
        return id;
    }

    internal static void AddTransportComponents(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        if (definition.Id == StableId.FromKey("unit.rock_raiders.crew"))
            world.Entities.Passenger.Set(id, new Passenger { Transport = EntityId.None, State = PassengerState.Grounded, SizePoints = 1 });
        if (definition.Id == StableId.FromKey("unit.rock_raiders.rapid_rider"))
            world.Entities.Transport.Set(id, new Transport { CapacityPoints = 4, JobState = TransportJobState.Idle });
    }

    internal static void AddTransformationComponents(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        if (!world.Content.TryGetTransformation(definition.Id, out TransformationDefinition transformation)) return;
        world.Entities.Transformation.Set(id, new Transformation
        {
            Definition = transformation.Id,
            CurrentState = transformation.ModeA.StateId,
            SourceState = transformation.ModeA.StateId,
            DestinationState = transformation.ModeA.StateId,
            Phase = TransformationPhase.Idle
        });
    }

    internal static void AddCombatComponents(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        PrototypeCombatProfile combat = definition.Combat;
        if (!combat.IsTargetable) return;
        world.Entities.Targetable.Set(id, new Targetable { Class = combat.TargetClass, Layer = combat.TargetLayer, Flags = combat.TargetFlags });
        Fix32 maximum = Fix32.FromInt(combat.MaximumHitPoints);
        world.Entities.Health.Set(id, new Health { Maximum = maximum, Current = maximum, ArmorRating = combat.ArmorRating, LastDamageTick = -1 });
        if (!combat.CanAcquireTargets) return;
        world.Entities.Targeting.Set(id, new Targeting
        {
            CurrentTarget = EntityId.None, AcquisitionRadius = combat.AcquisitionRadius, LegalLayers = combat.LegalTargetLayers,
            LegalClasses = combat.LegalTargetClasses, PriorityProfile = combat.PriorityProfile, SelectionKind = TargetSelectionKind.None
        });
        AddWeaponComponent(world, id, definition);
    }

    internal static void AddWeaponComponent(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        if (definition.Combat.WeaponProfile.Value == 0) return;
        if (!world.Content.TryGetWeapon(definition.Combat.WeaponProfile, out _)) throw new InvalidOperationException($"Missing weapon profile for {definition.StableKey}.");
        world.Entities.Weapon.Set(id, new WeaponState
        {
            WeaponProfile = definition.Combat.WeaponProfile,
            CooldownRemainingTicks = 0,
            FireSequence = 0,
            LastFiredTarget = EntityId.None,
            LastFiredTick = -1
        });
    }
}
}
