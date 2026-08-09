using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public static class ScenarioFactory
{
    public static SimulationWorld CreateFirstControllable(int count = 18)
        => CreateFirstControllable(DevMapFactory.CreateDefinition(), PrototypeContentFactory.CreateM2Catalog(), count);

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
        for (int i = 0; i < definition.InitialResourceNodes.Length; i++) SpawnResourceNode(world, definition.InitialResourceNodes[i], content);
        for (int i = 0; i < definition.InitialResourceReceivers.Length; i++) SpawnResourceReceiver(world, definition.InitialResourceReceivers[i], content);
        world.Spatial.Rebuild(world.Entities);
        new VisionSystem().Step(world);
        return world;
    }

    public static SimulationWorld CreateStress60()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        SpawnGrid(world, content, 0, 60, 20, 58, 10);
        world.Spatial.Rebuild(world.Entities); new VisionSystem().Step(world);
        EntityId[] ids = OwnedIds(world, 0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Move, ids, FixVec2.FromInts(138, 80)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(700), 0, 2, SimCommandType.Move, ids, FixVec2.FromInts(20, 112)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1200), 0, 3, SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: DevMapFactory.ExcavatableFeatureId));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1201), 0, 4, SimCommandType.Move, ids, FixVec2.FromInts(138, 135)));
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
        world.Spatial.Rebuild(world.Entities);new VisionSystem().Step(world);
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
        AddWorkerComponents(world, id, definition);
        world.GetQueue(id);
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

    private static void SpawnGrid(SimulationWorld world, PrototypeContentCatalog content, byte player, int count, int originX, int originY, int columns)
    {
        for (int i = 0; i < count; i++)
        {
            int x = originX + (i % columns) * 2; int y = originY + (i / columns) * 2;
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
        world.Entities.Vision.Set(id,new Vision{RadiusBuildCells=definition.VisionRadius,LastFogX=-1,LastFogY=-1});world.GetQueue(id);
        AddWorkerComponents(world,id,definition);
    }

    private static void AddWorkerComponents(SimulationWorld world, EntityId id, PrototypeEntityDefinition definition)
    {
        if (definition.SelectableKind != SelectableKind.Worker) return;
        if (definition.OreTicksPerUnit == 0 || definition.OreCarryCapacity == 0) throw new InvalidOperationException($"Worker content {definition.StableKey} has no Ore harvesting metadata.");
        world.Entities.Worker.Set(id, new Worker { ResourceTarget = EntityId.None, ReceiverTarget = EntityId.None, TaskState = WorkerTaskState.Idle, TicksPerOre = definition.OreTicksPerUnit });
        world.Entities.ResourceCarrier.Set(id, new ResourceCarrier { Type = ResourceType.Ore, Amount = 0, Capacity = definition.OreCarryCapacity });
    }

    private static void SpawnResourceReceiver(SimulationWorld world, InitialResourceReceiverSpawn spawn, PrototypeContentCatalog content)
    {
        if (!content.TryGetEntity(spawn.ContentKey, out PrototypeEntityDefinition definition) || definition.SelectableKind != SelectableKind.Building)
            throw new InvalidOperationException($"Missing resource receiver content {spawn.ContentKey}.");
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = spawn.PlayerSlot });
        world.Entities.Transform.Set(id, new SimTransform { Position = spawn.Position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = false, ContentType = definition.Id, Kind = SelectableKind.Building });
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
        world.SetConstructionOccupied(building, true);
    }
}
}
