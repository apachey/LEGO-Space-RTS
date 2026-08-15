using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class SimulationWorld
{
    public SimTick Tick { get; internal set; }
    public EntityStore Entities { get; }
    public PrototypeContentCatalog Content { get; }
    public MapGrid Map { get; private set; }
    public HierarchicalPathfinder Pathfinder { get; private set; }
    public SpatialGrid Spatial { get; } = new();
    public FogState Fog { get; internal set; }
    public CommandBuffer Commands { get; } = new();
    public int DeadlockDiagnostics { get; internal set; }
    public int StuckRecoveryDiagnostics { get; internal set; }
    public int OscillationDiagnostics { get; internal set; }
    public int PathRequestsProcessed { get; internal set; }
    public int FormationReflowDiagnostics { get; internal set; }
    public IReadOnlyList<ProjectileRecord> Projectiles => ProjectilesInternal;
    public IReadOnlyList<ProjectileImpactRecord> ProjectileImpacts => ProjectileImpactsInternal;
    public uint NextProjectileValue { get; internal set; } = 1;
    public uint TubeTopologyRevision { get; internal set; }
    public uint TubeSegmentationRevision { get; internal set; }
    private readonly OperationsCapacityState[] _operationsCapacity;
    private readonly AlienChargeState[] _alienCharge;

    public int PlayerCount => Fog.PlayerCount;

    internal readonly Dictionary<uint, RouteCorridor> Corridors = new();
    internal readonly Dictionary<uint, TubeRoute> TubeRoutes = new();
    internal readonly Dictionary<uint, TubeTransitRoute> TubeTransitRoutes = new();
    internal readonly Dictionary<uint, UnitCommandQueue> Queues = new();
    internal readonly Dictionary<uint, FixVec2> PendingVelocity = new();
    internal readonly Dictionary<uint, bool> CompressionUsed = new();
    internal readonly List<EntityId> ScratchEntities = new(128);
    internal readonly List<CommandEnvelope> ScratchCommands = new(32);
    internal readonly Dictionary<uint, FixVec2> DiagnosticLastDelta = new();
    internal readonly Dictionary<uint, int> DiagnosticLastReversalTick = new();
    internal readonly List<ProjectileRecord> ProjectilesInternal = new(64);
    internal readonly List<ProjectileImpactRecord> ProjectileImpactsInternal = new(16);

    public SimulationWorld(MapGrid map, int playerCount = 2, PrototypeContentCatalog? content = null)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
        Entities = new EntityStore();
        Content = content ?? PrototypeContentFactory.CreateM2Catalog();
        Fog = new FogState(playerCount);
        _operationsCapacity = new OperationsCapacityState[playerCount];
        _alienCharge = new AlienChargeState[playerCount];
        Tick = new SimTick(0);
    }

    internal SimulationWorld(MapGrid map, EntityStore entities, FogState fog, SimTick tick, PrototypeContentCatalog? content = null)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
        Entities = entities;
        Content = content ?? PrototypeContentFactory.CreateM2Catalog();
        Fog = fog;
        _operationsCapacity = new OperationsCapacityState[fog.PlayerCount];
        _alienCharge = new AlienChargeState[fog.PlayerCount];
        Tick = tick;
    }

    public UnitCommandQueue GetQueue(EntityId id)
    {
        if (!Queues.TryGetValue(id.Value, out UnitCommandQueue queue))
        {
            queue = new UnitCommandQueue();
            Queues.Add(id.Value, queue);
        }
        return queue;
    }

    internal bool TryGetQueue(EntityId id, out UnitCommandQueue queue) => Queues.TryGetValue(id.Value, out queue!);

    public RouteCorridor? GetCorridor(EntityId id) => Corridors.TryGetValue(id.Value, out RouteCorridor corridor) ? corridor : null;

    public TubeRoute? GetTubeRoute(EntityId link) => TubeRoutes.TryGetValue(link.Value, out TubeRoute route) ? route : null;

    public OperationsCapacityState GetOperationsCapacity(byte playerSlot)
    {
        if (playerSlot >= _operationsCapacity.Length) throw new System.ArgumentOutOfRangeException(nameof(playerSlot));
        return _operationsCapacity[playerSlot];
    }

    internal void SetOperationsCapacity(byte playerSlot, OperationsCapacityState state) => _operationsCapacity[playerSlot] = state;

    internal void ClearOperationsCapacity() => System.Array.Clear(_operationsCapacity, 0, _operationsCapacity.Length);

    internal void AddProjectile(ProjectileRecord projectile)
    {
        if (ProjectilesInternal.Count >= ProjectileSystem.MaximumProjectileRecords) throw new System.InvalidOperationException("Authoritative projectile capacity exceeded.");
        projectile.Id = new ProjectileId(NextProjectileValue);
        NextProjectileValue = checked(NextProjectileValue + 1);
        ProjectilesInternal.Add(projectile);
    }

    internal void AddRestoredProjectile(ProjectileRecord projectile)
    {
        if (projectile.Id.Value == 0 || ProjectilesInternal.Count >= ProjectileSystem.MaximumProjectileRecords) throw new System.IO.InvalidDataException("Invalid restored projectile record.");
        if (ProjectilesInternal.Count > 0 && ProjectilesInternal[^1].Id.Value >= projectile.Id.Value) throw new System.IO.InvalidDataException("Projectile records are not in stable ID order.");
        ProjectilesInternal.Add(projectile);
    }

    public AlienChargeState GetAlienCharge(byte playerSlot)
    {
        if (playerSlot >= _alienCharge.Length) throw new System.ArgumentOutOfRangeException(nameof(playerSlot));
        return _alienCharge[playerSlot];
    }

    internal ref AlienChargeState GetAlienChargeRef(byte playerSlot)
    {
        if (playerSlot >= _alienCharge.Length) throw new System.ArgumentOutOfRangeException(nameof(playerSlot));
        return ref _alienCharge[playerSlot];
    }

    internal void SetAlienCharge(byte playerSlot, AlienChargeState state) => _alienCharge[playerSlot] = state;
    public bool TryExtractResource(EntityId id, int requestedAmount, out int extractedAmount)
    {
        if (requestedAmount <= 0) throw new System.ArgumentOutOfRangeException(nameof(requestedAmount));
        if (!Entities.ResourceNode.Has(id)) { extractedAmount = 0; return false; }
        ref ResourceNode node = ref Entities.ResourceNode.Get(id);
        if (node.Remaining <= 0) { extractedAmount = 0; return false; }
        extractedAmount = System.Math.Min(requestedAmount, node.Remaining);
        node.Remaining -= extractedAmount;
        return true;
    }

    public int GetProcessedResourceTotal(byte playerSlot, ResourceType type)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type) continue;
            total = checked(total + bank.ProcessedAmount);
        }
        return total;
    }

    public int GetPendingHauledResourceTotal(byte playerSlot, ResourceType type)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !Entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver) || receiver.AcceptedType != type) continue;
            total = checked(total + receiver.PendingHauledAmount);
        }
        return total;
    }

    public bool OpenExcavatable(ushort featureId)
    {
        if (!Map.TryGetFeature(featureId, out _)) throw new System.InvalidOperationException($"Unknown Excavatable Feature {featureId}.");
        ExcavationTopologySystem.InitializeFeatures(this);
        if (!Map.TryOpenFeature(featureId, out IntRect rect)) return false;
        ExcavationTopologySystem.MarkOpen(this, featureId);
        ApplyTopologyChange(rect);
        return true;
    }

    internal void SetConstructionOccupied(Building building, bool occupied)
    {
        if (!Content.TryGetBuilding(building.Type, out BuildingDefinition definition)) throw new System.InvalidOperationException($"Unknown building definition {building.Type}.");
        IntRect rect = Map.SetConstructionOccupied(building.AnchorX, building.AnchorY, definition, building.Orientation, occupied);
        ApplyTopologyChange(rect);
    }

    internal void ClearDestroyedStructureFootprint(DestructionState destruction)
    {
        Building building = new()
        {
            Type = destruction.BuildingType,
            AnchorX = destruction.BuildingAnchorX,
            AnchorY = destruction.BuildingAnchorY,
            Orientation = destruction.BuildingOrientation,
            FootprintWidth = destruction.BuildingWidth,
            FootprintHeight = destruction.BuildingHeight,
            State = BuildingState.Completed
        };
        SetConstructionOccupied(building, false);
    }

    internal void RemoveRuntimeState(EntityId id)
    {
        Queues.Remove(id.Value);
        Corridors.Remove(id.Value);
        PendingVelocity.Remove(id.Value);
        CompressionUsed.Remove(id.Value);
        DiagnosticLastDelta.Remove(id.Value);
        DiagnosticLastReversalTick.Remove(id.Value);
    }

    private void ApplyTopologyChange(IntRect rect)
    {
        Pathfinder.RebuildAffected(rect);
        IReadOnlyList<EntityId> alive = Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!Entities.Navigation.Has(id)) continue;
            ref NavigationAgent nav = ref Entities.Navigation.Get(id);
            if (!nav.HasTarget) continue;
            if (!Corridors.TryGetValue(id.Value, out RouteCorridor corridor) || corridor.IsAffectedBy(rect))
            {
                nav.PathDirty = true;
                continue;
            }
            corridor.TopologyVersion = Map.TopologyVersion;
            nav.PathTopologyVersion = Map.TopologyVersion;
        }
    }

    internal void ReplaceMap(MapGrid map)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
    }
}
}
