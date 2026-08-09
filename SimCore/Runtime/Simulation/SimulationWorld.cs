using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class SimulationWorld
{
    public SimTick Tick { get; internal set; }
    public EntityStore Entities { get; }
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

    internal readonly Dictionary<uint, RouteCorridor> Corridors = new();
    internal readonly Dictionary<uint, UnitCommandQueue> Queues = new();
    internal readonly Dictionary<uint, FixVec2> PendingVelocity = new();
    internal readonly Dictionary<uint, bool> CompressionUsed = new();
    internal readonly List<EntityId> ScratchEntities = new(128);
    internal readonly List<CommandEnvelope> ScratchCommands = new(32);
    internal readonly Dictionary<uint, FixVec2> DiagnosticLastDelta = new();
    internal readonly Dictionary<uint, int> DiagnosticLastReversalTick = new();

    public SimulationWorld(MapGrid map, int playerCount = 2)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
        Entities = new EntityStore();
        Fog = new FogState(playerCount);
        Tick = new SimTick(0);
    }

    internal SimulationWorld(MapGrid map, EntityStore entities, FogState fog, SimTick tick)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
        Entities = entities;
        Fog = fog;
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

    public void OpenExcavatable(ushort featureId)
    {
        IntRect rect = Map.OpenFeature(featureId);
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
