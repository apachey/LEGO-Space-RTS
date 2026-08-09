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

    internal readonly Dictionary<uint, NavPath> Paths = new();
    internal readonly Dictionary<uint, UnitCommandQueue> Queues = new();
    internal readonly Dictionary<uint, FixVec2> PendingVelocity = new();
    internal readonly Dictionary<uint, bool> ReservationPermit = new();
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

    public NavPath? GetPath(EntityId id) => Paths.TryGetValue(id.Value, out NavPath path) ? path : null;
    public bool HasReservationPermit(EntityId id) => ReservationPermit.TryGetValue(id.Value, out bool permit) && permit;

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
            if (nav.HasTarget) nav.PathDirty = true;
        }
    }

    internal void ReplaceMap(MapGrid map)
    {
        Map = map;
        Pathfinder = new HierarchicalPathfinder(map);
    }
}
}
