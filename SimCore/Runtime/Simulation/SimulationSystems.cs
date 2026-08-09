using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public interface ISimSystem { void Step(SimulationWorld world); }

public sealed class CommandExecutionSystem : ISimSystem
{
    private readonly FormationEntityComparer _formationComparer = new FormationEntityComparer();
    public void Step(SimulationWorld world)
    {
        world.Commands.DrainForTick(world.Tick, world.ScratchCommands);
        for (int c = 0; c < world.ScratchCommands.Count; c++) Execute(world, world.ScratchCommands[c]);
    }

    private void Execute(SimulationWorld world, CommandEnvelope command)
    {
        if (command.Type == SimCommandType.DebugOpenExcavatable)
        {
            world.OpenExcavatable(command.DebugFeatureId);
            return;
        }

        world.ScratchEntities.Clear();
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (!world.Entities.Exists(id) || !world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != command.PlayerSlot || !world.Entities.Navigation.Has(id)) continue;
            world.ScratchEntities.Add(id);
        }
        world.ScratchEntities.Sort(EntityIdComparer.Instance);

        if (command.Type == SimCommandType.Move)
        {
            FixVec2 centroid = FormationPlanner.ComputeCentroid(world, world.ScratchEntities);
            FixVec2 heading = command.TargetPosition - centroid;
            FootprintClass spacingClass = FormationPlanner.LargestFootprint(world, world.ScratchEntities);
            _formationComparer.World = world;
            world.ScratchEntities.Sort(_formationComparer);
            int columns = FormationPlanner.EstimateColumns(world, command.TargetPosition, world.ScratchEntities.Count, spacingClass);
            for (int i = 0; i < world.ScratchEntities.Count; i++)
            {
                EntityId id = world.ScratchEntities[i];
                FixVec2 desiredSlot = FormationPlanner.GetSlot(command.TargetPosition, i, world.ScratchEntities.Count, spacingClass, heading, columns);
                NavigationAgent slotNav = world.Entities.Navigation.Get(id);
                FixVec2 slotTarget = FormationPlanner.ResolvePassableSlot(world, desiredSlot, slotNav.Footprint);
                bool queued = (command.Modifiers & CommandModifiers.Queue) != 0;
                if (queued && world.Entities.Navigation.Get(id).HasTarget)
                {
                    world.GetQueue(id).Enqueue(new UnitOrder(UnitOrderType.Move, slotTarget));
                }
                else
                {
                    if (!queued) world.GetQueue(id).Clear();
                    SetMove(world, id, slotTarget);
                }
            }
            return;
        }

        for (int i = 0; i < world.ScratchEntities.Count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            ref Movement move = ref world.Entities.Movement.Get(id);
            world.GetQueue(id).Clear();
            nav.HasTarget = false; nav.PathDirty = false; move.PathIndex = 0;
            world.Paths.Remove(id.Value);
            move.CurrentSpeed = Fix32.Zero; move.CurrentVelocity = FixVec2.Zero; move.DesiredMovement = FixVec2.Zero;
            move.State = command.Type == SimCommandType.HoldPosition ? MovementState.Holding : MovementState.Idle;
        }
    }

    internal static void SetMove(SimulationWorld world, EntityId id, FixVec2 target)
    {
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
        ref Movement move = ref world.Entities.Movement.Get(id);
        nav.Target = target; nav.HasTarget = true; nav.PathDirty = true; move.PathIndex = 0; nav.RequestAge = 0;
        move.DesiredMovement = FixVec2.Zero; move.State = MovementState.WaitingForPath;
    }
}

internal sealed class EntityIdComparer : IComparer<EntityId>
{
    public static readonly EntityIdComparer Instance = new EntityIdComparer();
    private EntityIdComparer() { }
    public int Compare(EntityId a, EntityId b) => a.Value.CompareTo(b.Value);
}

public sealed class FormationEntityComparer : IComparer<EntityId>
{
    public SimulationWorld? World;
    public int Compare(EntityId a, EntityId b)
    {
        SimulationWorld world = World ?? throw new InvalidOperationException("Formation comparer is not bound to a world.");
        int ra = FormationPlanner.RoleRank(world, a), rb = FormationPlanner.RoleRank(world, b);
        int c = ra.CompareTo(rb); return c != 0 ? c : a.Value.CompareTo(b.Value);
    }
}

public static class FormationPlanner
{
    // M2 loose role-aware formation: heavy/frontline first, standards next, workers/support rear.
    public static int RoleRank(SimulationWorld world, EntityId id)
    {
        if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.Kind == SelectableKind.Worker) return 2;
        if (world.Entities.Navigation.TryGet(id, out NavigationAgent nav) && (nav.Footprint == FootprintClass.Huge || nav.Footprint == FootprintClass.Large)) return 0;
        return 1;
    }

    public static FixVec2 ComputeCentroid(SimulationWorld world, IReadOnlyList<EntityId> ids)
    {
        if (ids.Count == 0) return FixVec2.Zero;
        long x = 0, y = 0; int count = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            if (!world.Entities.Transform.TryGet(ids[i], out SimTransform t)) continue;
            x += t.Position.X.Raw; y += t.Position.Y.Raw; count++;
        }
        if (count == 0) return FixVec2.Zero;
        return new FixVec2(Fix32.FromRaw(checked((int)(x / count))), Fix32.FromRaw(checked((int)(y / count))));
    }

    public static FootprintClass LargestFootprint(SimulationWorld world, IReadOnlyList<EntityId> ids)
    {
        FootprintClass result = FootprintClass.Tiny;
        for (int i = 0; i < ids.Count; i++)
            if (world.Entities.Navigation.TryGet(ids[i], out NavigationAgent nav) && nav.Footprint > result) result = nav.Footprint;
        return result;
    }

    public static int EstimateColumns(SimulationWorld world, FixVec2 destination, int count, FootprintClass footprint)
    {
        int preferred = count <= 9 ? 3 : count <= 25 ? 5 : 8;
        NavCell center = MapGrid.BuildToNav(destination);
        int radius = FootprintRules.ClearanceNavCells(footprint);
        int clearX = 0;
        for (int x = center.X; x >= 0 && world.Pathfinder.IsPassable(new NavCell(checked((short)x), center.Y), footprint); x--) clearX++;
for (int x = center.X + 1; x < MapGrid.NavWidth && world.Pathfinder.IsPassable(new NavCell(checked((short)x), center.Y), footprint); x++) clearX++;

int clearY = 0;
for (int y = center.Y; y >= 0 && world.Pathfinder.IsPassable(new NavCell(center.X, checked((short)y)), footprint); y--) clearY++;
for (int y = center.Y + 1; y < MapGrid.NavHeight && world.Pathfinder.IsPassable(new NavCell(center.X, checked((short)y)), footprint); y++) clearY++;
        int usableNav = Math.Max(1, Math.Max(clearX, clearY) - radius * 2);
        int navSpacing = Math.Max(2, radius * 2 + 1);
        int available = Math.Max(1, usableNav / navSpacing);
        return Math.Max(1, Math.Min(preferred, available));
    }

    public static FixVec2 GetSlot(FixVec2 center, int index, int count, FootprintClass footprint, FixVec2 heading, int columns, bool spread = false)
    {
        if (count <= 1) return center;
        columns = Math.Max(1, Math.Min(columns, count));
        int row = index / columns;
        int col = index % columns;
        int rows = (count + columns - 1) / columns;
        Fix32 spacing = Fix32.FromRatio(11 + (int)footprint * 4, 10); // 1.1 .. 2.7 build cells.
        if (spread) spacing *= Fix32.FromRatio(14,10); // Phase 06 +40% separation foundation.
        Fix32 lateral = Fix32.FromRatio((col * 2 - (columns - 1)), 2) * spacing;
        // First row is the front row; final row is the rear/support row.
        Fix32 longitudinal = Fix32.FromRatio(((rows - 1) - row * 2), 2) * spacing;
        FixVec2 forward = heading.NormalizeSafe();
        if (forward.Equals(FixVec2.Zero)) forward = new FixVec2(Fix32.One, Fix32.Zero);
        FixVec2 right = new FixVec2(-forward.Y, forward.X);
        return center + right * lateral + forward * longitudinal;
    }

    public static FixVec2 ResolvePassableSlot(SimulationWorld world, FixVec2 desired, FootprintClass footprint)
    {
        NavCell origin = MapGrid.BuildToNav(desired);
        if (world.Pathfinder.IsPassable(origin, footprint)) return desired;

        const int MaxSearchRadiusNav = 16;
        for (int radius = 1; radius <= MaxSearchRadiusNav; radius++)
        {
            // Deterministic perimeter scan: top/bottom rows first, then left/right columns.
            for (int dx = -radius; dx <= radius; dx++)
            {
                int topX = origin.X + dx, topY = origin.Y - radius;
                if (TryPassable(world, topX, topY, footprint, out FixVec2 top)) return top;
                int bottomY = origin.Y + radius;
                if (TryPassable(world, topX, bottomY, footprint, out FixVec2 bottom)) return bottom;
            }
            for (int dy = -radius + 1; dy <= radius - 1; dy++)
            {
                int leftX = origin.X - radius, y = origin.Y + dy;
                if (TryPassable(world, leftX, y, footprint, out FixVec2 left)) return left;
                int rightX = origin.X + radius;
                if (TryPassable(world, rightX, y, footprint, out FixVec2 right)) return right;
            }
        }
        return desired;
    }

    private static bool TryPassable(SimulationWorld world, int x, int y, FootprintClass footprint, out FixVec2 buildPosition)
    {
        if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight)
        {
            buildPosition = default;
            return false;
        }
        NavCell cell = new NavCell(checked((short)x), checked((short)y));
        if (!world.Pathfinder.IsPassable(cell, footprint))
        {
            buildPosition = default;
            return false;
        }
        buildPosition = MapGrid.NavCellCenterToBuild(cell);
        return true;
    }
}

public sealed class NavigationRequestSystem : ISimSystem
{
    public const int MaxRequestsPerTick = 24;
    private readonly RequestComparer _comparer = new RequestComparer();
    public void Step(SimulationWorld world)
    {
        world.ScratchEntities.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Navigation.Has(id)) continue;
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            if (!nav.HasTarget) continue;
            if (nav.PathTopologyVersion != world.Map.TopologyVersion) nav.PathDirty = true;
            if (nav.PathDirty) { nav.RequestAge++; world.ScratchEntities.Add(id); }
        }
        _comparer.World = world;
        world.ScratchEntities.Sort(_comparer);
        int count = Math.Min(MaxRequestsPerTick, world.ScratchEntities.Count);
        for (int i = 0; i < count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            SimTransform transform = world.Entities.Transform.Get(id);
            NavPath path;
            if (!world.Paths.TryGetValue(id.Value, out path)) { path = new NavPath(); world.Paths[id.Value] = path; }
            world.Pathfinder.FindPath(MapGrid.BuildToNav(transform.Position), MapGrid.BuildToNav(nav.Target), nav.Footprint, path, nav.Layer);
            world.PathRequestsProcessed++;
            nav.PathDirty = false; nav.PathTopologyVersion = world.Map.TopologyVersion; nav.RequestAge = 0;
            ref Movement move = ref world.Entities.Movement.Get(id);
            move.PathIndex = path.Cells.Count > 1 ? 1 : 0;
            move.State = path.IsValid ? MovementState.Moving : MovementState.StuckRecovery;
        }
    }

    private sealed class RequestComparer : IComparer<EntityId>
    {
        public SimulationWorld World = null!;
        public int Compare(EntityId a, EntityId b)
        {
            NavigationAgent na = World.Entities.Navigation.Get(a); NavigationAgent nb = World.Entities.Navigation.Get(b);
            Movement ma = World.Entities.Movement.Get(a); Movement mb = World.Entities.Movement.Get(b);
            bool aStuck = ma.StuckTicks >= 20, bStuck = mb.StuckTicks >= 20;
            if (aStuck != bStuck) return aStuck ? -1 : 1;
            int c = nb.RequestAge.CompareTo(na.RequestAge); if (c != 0) return c;
            c = FootprintRules.ReservationPriority(nb.Footprint).CompareTo(FootprintRules.ReservationPriority(na.Footprint)); if (c != 0) return c;
            return a.Value.CompareTo(b.Value);
        }
    }
}

public sealed class ReservationPlanningSystem : ISimSystem
{
    public const int HorizonTicks = 12;
    private readonly Dictionary<int, uint> _reserved = new Dictionary<int, uint>(2048);
    private readonly ReservationComparer _comparer = new ReservationComparer();
    private readonly List<NavCell> _horizonCells = new List<NavCell>(16);

    public void Step(SimulationWorld world)
    {
        _reserved.Clear(); world.ReservationPermit.Clear(); world.ScratchEntities.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.Navigation.Has(alive[i]) && world.Entities.Transform.Has(alive[i])) world.ScratchEntities.Add(alive[i]);
        _comparer.World = world;
        world.ScratchEntities.Sort(_comparer);

        // Reserve current occupied footprints first. Future reservations may never drive through
        // an entity that is already physically there. Heavy-first ordering resolves rare overlaps deterministically.
        for (int i = 0; i < world.ScratchEntities.Count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            NavigationAgent nav = world.Entities.Navigation.Get(id);
            if (nav.Layer == MovementLayer.TrueAir) continue;
            NavCell occupied = MapGrid.BuildToNav(world.Entities.Transform.Get(id).Position);
            ReserveCurrentFootprint(occupied, nav.Footprint, id.Value);
        }

        Fix32 horizonFactor = Fix32.FromRatio(HorizonTicks * MapGrid.NavPerBuild, SimClock.TicksPerSecond); // speed(build/s) -> nav cells in 0.6 s.
        for (int i = 0; i < world.ScratchEntities.Count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            NavigationAgent nav = world.Entities.Navigation.Get(id);
            Movement movement = world.Entities.Movement.Get(id);
            if (!nav.HasTarget) { world.ReservationPermit[id.Value] = false; continue; }
            if (nav.Layer == MovementLayer.TrueAir) { world.ReservationPermit[id.Value] = true; continue; }
            if (!world.Paths.TryGetValue(id.Value, out NavPath path) || movement.PathIndex >= path.Cells.Count)
            { world.ReservationPermit[id.Value] = false; continue; }

            int horizonNavCells = Fix32.Max(Fix32.One, movement.MaxSpeed * horizonFactor).CeilToInt();
            CollectHorizonCells(world, id, path, movement.PathIndex, horizonNavCells);
            bool permit = true;
            for (int p = 0; p < _horizonCells.Count && permit; p++) permit = FootprintAvailable(_horizonCells[p], nav.Footprint, id.Value);
            if (permit)
                for (int p = 0; p < _horizonCells.Count; p++) ReserveFootprint(_horizonCells[p], nav.Footprint, id.Value);
            world.ReservationPermit[id.Value] = permit;
        }
    }

    private void CollectHorizonCells(SimulationWorld world, EntityId id, NavPath path, int pathIndex, int maxSteps)
    {
        _horizonCells.Clear();
        NavCell current = MapGrid.BuildToNav(world.Entities.Transform.Get(id).Position);
        int remaining = Math.Max(1, maxSteps);
        int index = pathIndex;
        while (remaining > 0 && index < path.Cells.Count)
        {
            NavCell goal = path.Cells[index];
            bool reached = AppendLineSteps(current, goal, ref remaining);
            if (_horizonCells.Count > 0) current = _horizonCells[_horizonCells.Count - 1];
            if (!reached) break;
            index++;
        }
        if (_horizonCells.Count == 0) _horizonCells.Add(current);
    }

    private bool AppendLineSteps(NavCell start, NavCell goal, ref int remaining)
    {
        int x = start.X, y = start.Y;
        int dx = Math.Abs(goal.X - x), sx = x < goal.X ? 1 : -1;
        int dy = -Math.Abs(goal.Y - y), sy = y < goal.Y ? 1 : -1;
        int err = dx + dy;
        if (x == goal.X && y == goal.Y) return true;
        while ((x != goal.X || y != goal.Y) && remaining > 0)
        {
            int e2 = err << 1;
            if (e2 >= dy) { err += dy; x += sx; }
            if (e2 <= dx) { err += dx; y += sy; }
            _horizonCells.Add(new NavCell(checked((short)x), checked((short)y)));
            remaining--;
        }
        return x == goal.X && y == goal.Y;
    }

    private bool FootprintAvailable(NavCell cell, FootprintClass footprint, uint owner)
    {
        int radius = FootprintRules.ClearanceNavCells(footprint), rr = radius * radius;
        for (int y = cell.Y - radius; y <= cell.Y + radius; y++)
            for (int x = cell.X - radius; x <= cell.X + radius; x++)
            {
                int dx=x-cell.X,dy=y-cell.Y;if(dx*dx+dy*dy>rr)continue;
                if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight) return false;
                int key=y*MapGrid.NavWidth+x;
                if(_reserved.TryGetValue(key,out uint existing)&&existing!=owner)return false;
            }
        return true;
    }

    private void ReserveCurrentFootprint(NavCell cell, FootprintClass footprint, uint owner)
    {
        int radius = FootprintRules.ClearanceNavCells(footprint), rr = radius * radius;
        for (int y = cell.Y - radius; y <= cell.Y + radius; y++)
            for (int x = cell.X - radius; x <= cell.X + radius; x++)
            {
                int dx=x-cell.X,dy=y-cell.Y;if(dx*dx+dy*dy>rr)continue;
                if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight) continue;
                int key=y*MapGrid.NavWidth+x;
                if(!_reserved.ContainsKey(key))_reserved[key]=owner;
            }
    }

    private void ReserveFootprint(NavCell cell, FootprintClass footprint, uint owner)
    {
        int radius = FootprintRules.ClearanceNavCells(footprint), rr = radius * radius;
        for (int y = cell.Y - radius; y <= cell.Y + radius; y++)
            for (int x = cell.X - radius; x <= cell.X + radius; x++)
            {
                int dx=x-cell.X,dy=y-cell.Y;if(dx*dx+dy*dy>rr)continue;
                if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight) continue;
                _reserved[y*MapGrid.NavWidth+x]=owner;
            }
    }

    private sealed class ReservationComparer : IComparer<EntityId>
    {
        public SimulationWorld World = null!;
        public int Compare(EntityId a, EntityId b)
        {
            int pa = FootprintRules.ReservationPriority(World.Entities.Navigation.Get(a).Footprint);
            int pb = FootprintRules.ReservationPriority(World.Entities.Navigation.Get(b).Footprint);
            int c = pb.CompareTo(pa);
            return c != 0 ? c : a.Value.CompareTo(b.Value);
        }
    }
}

public sealed class MovementIntentSystem : ISimSystem
{
    private static readonly Fix32 Two = Fix32.FromInt(2);
    public void Step(SimulationWorld world)
    {
        world.PendingVelocity.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Navigation.TryGet(id, out NavigationAgent nav) || !world.Entities.Movement.Has(id) || !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            ref Movement move = ref world.Entities.Movement.Get(id);
            move.DesiredMovement = FixVec2.Zero;
            if (!nav.HasTarget || move.State == MovementState.Holding || !world.Paths.TryGetValue(id.Value, out NavPath path) || move.PathIndex >= path.Cells.Count)
            { world.PendingVelocity[id.Value] = FixVec2.Zero; continue; }

            bool permit = world.ReservationPermit.TryGetValue(id.Value, out bool reservationPermit) && reservationPermit;
            FixVec2 waypoint = MapGrid.NavCellCenterToBuild(path.Cells[move.PathIndex]);
            FixVec2 delta = waypoint - transform.Position;
            Fix32 distance = delta.Length();
            if (distance.Raw == 0) { world.PendingVelocity[id.Value] = FixVec2.Zero; continue; }
            // Intermediate waypoints are steering points, not stop points. v0.3 braked at every
            // 0.5-cell A* node, producing visibly jerky movement and queue delays.
            bool finalWaypoint = move.PathIndex >= path.Cells.Count - 1 && world.GetQueue(id).Count == 0;
            Fix32 brakingSpeed = finalWaypoint ? Fix32.Sqrt(Two * move.Deceleration * FixVec2.Distance(transform.Position, nav.Target)) : move.MaxSpeed;
            // A denied reservation means yield, not freeze. Local avoidance still owns the final collision-safe step.
            Fix32 reservationSpeedCap = permit ? move.MaxSpeed : move.MaxSpeed * Fix32.FromRatio(35, 100);
            Fix32 desiredSpeed = Fix32.Min(reservationSpeedCap, brakingSpeed);
            move.DesiredMovement = delta.NormalizeSafe() * desiredSpeed;
            world.PendingVelocity[id.Value] = move.DesiredMovement * SimClock.TickSeconds;
        }
    }
}

public sealed class LocalAvoidanceSystem : ISimSystem
{
    public const int NeighborCap = 24;
    private readonly List<EntityId> _neighbors = new List<EntityId>(64);
    private readonly NeighborDistanceComparer _neighborComparer = new NeighborDistanceComparer();
    // Q16.16 checked-in deterministic rotation constants.
    private static readonly int[] Cos = { 65536, 63303, 63303, 56756, 56756, 46341, 46341, 32768, 32768, 0, 0, 32768, 0 };
    private static readonly int[] Sin = { 0, 16962, -16962, 32768, -32768, 46341, -46341, 56756, -56756, 65536, -65536, 0, 0 };

    public void Step(SimulationWorld world)
    {
        world.CompressionUsed.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.PendingVelocity.TryGetValue(id.Value, out FixVec2 desired) || desired.Equals(FixVec2.Zero)) continue;
            SimTransform self = world.Entities.Transform.Get(id); NavigationAgent selfNav = world.Entities.Navigation.Get(id); Movement selfMove=world.Entities.Movement.Get(id);
            if(selfNav.Layer==MovementLayer.TrueAir)continue;
            world.Spatial.Query(self.Position, 4, _neighbors);
            _neighborComparer.World=world;_neighborComparer.Center=self.Position;_neighbors.Sort(_neighborComparer);

            FixVec2 best = FixVec2.Zero; long bestScore = long.MaxValue;
            for (int candidateIndex = 0; candidateIndex < Cos.Length; candidateIndex++)
            {
                FixVec2 candidate;
                if(candidateIndex==11) candidate=desired*Fix32.Half; // slow
                else if(candidateIndex==12) candidate=FixVec2.Zero; // stop
                else candidate=Rotate(desired,Cos[candidateIndex],Sin[candidateIndex]);
                long score = ScoreCandidate(world,id,self,selfNav,selfMove,candidate,desired,candidateIndex);
                if(score<bestScore){bestScore=score;best=candidate;}
            }
            world.PendingVelocity[id.Value]=best;
            world.CompressionUsed[id.Value]=UsesFriendlyCompression(world,id,self,selfNav,selfMove,best);
        }
    }

    private long ScoreCandidate(SimulationWorld world,EntityId selfId,SimTransform self,NavigationAgent selfNav,Movement selfMove,FixVec2 candidate,FixVec2 desired,int candidateIndex)
    {
        // Prefer route progress and small heading changes; stuck units progressively care less about heading change.
        FixVec2 immediate=self.Position+candidate;
        if(selfNav.Layer!=MovementLayer.TrueAir&&!world.Pathfinder.IsPassable(MapGrid.BuildToNav(immediate),selfNav.Footprint))return long.MaxValue/4;
        long dotRaw = ((long)candidate.X.Raw*desired.X.Raw + (long)candidate.Y.Raw*desired.Y.Raw) >> Fix32.FractionalBits;
        long score = -dotRaw * 8L;
        int headingWeight=selfMove.StuckTicks>=20?2:12;
        score += (long)candidateIndex*headingWeight*Fix32.OneRaw;
        if(candidate.Equals(FixVec2.Zero))score += selfMove.StuckTicks>=20?Fix32.OneRaw:Fix32.OneRaw*20L;

        FixVec2 selfFuture=self.Position+candidate*Fix32.FromInt(ReservationPlanningSystem.HorizonTicks);
        int neighborCount=Math.Min(_neighbors.Count,NeighborCap);
        for(int n=0;n<neighborCount;n++)
        {
            EntityId otherId=_neighbors[n];if(otherId==selfId||!world.Entities.Transform.TryGet(otherId,out SimTransform other)||!world.Entities.Navigation.TryGet(otherId,out NavigationAgent otherNav))continue;
            if(otherNav.Layer==MovementLayer.TrueAir)continue;
            FixVec2 otherStep=world.PendingVelocity.TryGetValue(otherId.Value,out FixVec2 ov)?ov:FixVec2.Zero;
            Fix32 min=FootprintRules.CollisionRadiusBuild(selfNav.Footprint)+FootprintRules.CollisionRadiusBuild(otherNav.Footprint);
            bool friendly=world.Entities.Ownership.TryGet(selfId,out Ownership a)&&world.Entities.Ownership.TryGet(otherId,out Ownership b)&&a.PlayerSlot==b.PlayerSlot;
            Fix32 allowed=friendly && selfMove.CompressionTicks < 30 ? min*Fix32.FromRatio(85,100) : min;

            // Immediate one-tick collision safety. v0.3 only evaluated the 12-tick horizon,
            // which allowed two units to overlap before the future penalty became useful.
            Fix32 currentDistance=FixVec2.Distance(self.Position,other.Position);
            Fix32 nextDistance=FixVec2.Distance(immediate,other.Position+otherStep);
            if(currentDistance>=allowed && nextDistance<allowed) return long.MaxValue/8 + candidateIndex;
            if(currentDistance<allowed)
            {
                Fix32 overlap=allowed-nextDistance;
                if(overlap.Raw>0) score += (long)overlap.Raw*overlap.Raw*512L;
                // When already interpenetrating, strongly prefer a step that increases separation.
                if(nextDistance<=currentDistance && !candidate.Equals(FixVec2.Zero)) score += Fix32.OneRaw*200L;
            }

            FixVec2 otherFuture=other.Position+otherStep*Fix32.FromInt(ReservationPlanningSystem.HorizonTicks);
            Fix32 futureDistance=FixVec2.Distance(selfFuture,otherFuture);
            if(futureDistance<allowed)
            {
                Fix32 overlap=allowed-futureDistance;
                score += (long)overlap.Raw*overlap.Raw*24L;
            }
        }
        return score;
    }

    private bool UsesFriendlyCompression(SimulationWorld world,EntityId selfId,SimTransform self,NavigationAgent selfNav,Movement selfMove,FixVec2 candidate)
    {
        if (candidate.Equals(FixVec2.Zero) || selfMove.CompressionTicks >= 30) return false;
        FixVec2 selfNext=self.Position+candidate;
        int neighborCount=Math.Min(_neighbors.Count,NeighborCap);
        for(int n=0;n<neighborCount;n++)
        {
            EntityId otherId=_neighbors[n]; if(otherId==selfId||!world.Entities.Transform.TryGet(otherId,out SimTransform other)||!world.Entities.Navigation.TryGet(otherId,out NavigationAgent otherNav))continue;
            if(otherNav.Layer==MovementLayer.TrueAir)continue;
            if(!world.Entities.Ownership.TryGet(selfId,out Ownership a)||!world.Entities.Ownership.TryGet(otherId,out Ownership b)||a.PlayerSlot!=b.PlayerSlot)continue;
            FixVec2 otherStep=world.PendingVelocity.TryGetValue(otherId.Value,out FixVec2 ov)?ov:FixVec2.Zero;
            Fix32 distance=FixVec2.Distance(selfNext,other.Position+otherStep);
            Fix32 nominal=FootprintRules.CollisionRadiusBuild(selfNav.Footprint)+FootprintRules.CollisionRadiusBuild(otherNav.Footprint);
            if(distance<nominal && distance>=nominal*Fix32.FromRatio(85,100))return true;
        }
        return false;
    }

    private sealed class NeighborDistanceComparer : IComparer<EntityId>
    {
        public SimulationWorld? World;
        public FixVec2 Center;
        public int Compare(EntityId a,EntityId b)
        {
            SimulationWorld world=World ?? throw new InvalidOperationException("Neighbor comparer is not bound.");
            Fix32 da=world.Entities.Transform.TryGet(a,out SimTransform ta)?FixVec2.DistanceSquared(Center,ta.Position):Fix32.MaxValue;
            Fix32 db=world.Entities.Transform.TryGet(b,out SimTransform tb)?FixVec2.DistanceSquared(Center,tb.Position):Fix32.MaxValue;
            int c=da.Raw.CompareTo(db.Raw);return c!=0?c:a.Value.CompareTo(b.Value);
        }
    }

    private static FixVec2 Rotate(FixVec2 v,int cosRaw,int sinRaw)
    {
        long x=((long)v.X.Raw*cosRaw-(long)v.Y.Raw*sinRaw)>>Fix32.FractionalBits;
        long y=((long)v.X.Raw*sinRaw+(long)v.Y.Raw*cosRaw)>>Fix32.FractionalBits;
        return new FixVec2(Fix32.FromRaw(checked((int)x)),Fix32.FromRaw(checked((int)y)));
    }

}

public sealed class TransformMovementSystem : ISimSystem
{
    private static readonly Fix32 ArriveDistance = Fix32.FromRatio(3, 10);
    private static readonly Fix32 ProgressEpsilonSquared = Fix32.FromRatio(1, 1000) * Fix32.FromRatio(1, 1000);

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i]; if (!world.Entities.Transform.Has(id) || !world.Entities.Movement.Has(id) || !world.Entities.Navigation.Has(id)) continue;
            ref SimTransform transform = ref world.Entities.Transform.Get(id); ref Movement move = ref world.Entities.Movement.Get(id); ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            FixVec2 before = transform.Position;
            FixVec2 desiredStep = world.PendingVelocity.TryGetValue(id.Value, out FixVec2 pending) ? pending : FixVec2.Zero;
            bool mayAdvance = !desiredStep.Equals(FixVec2.Zero);
            Fix32 targetSpeed = mayAdvance ? desiredStep.Length() / SimClock.TickSeconds : Fix32.Zero;
            Fix32 speedDelta = targetSpeed - move.CurrentSpeed;
            if (speedDelta.Raw > 0) move.CurrentSpeed = Fix32.Min(targetSpeed, move.CurrentSpeed + move.Acceleration * SimClock.TickSeconds);
            else if (speedDelta.Raw < 0) move.CurrentSpeed = Fix32.Max(targetSpeed, move.CurrentSpeed - move.Deceleration * SimClock.TickSeconds);

            FixVec2 direction = mayAdvance ? desiredStep.NormalizeSafe() : move.CurrentVelocity.NormalizeSafe();
            if (!direction.Equals(FixVec2.Zero))
            {
                Angle16 targetOrientation = Angle16.FromDirection(direction);
                transform.Orientation = Angle16.TurnToward(transform.Orientation, targetOrientation, move.TurnRatePerTick);
            }

            if (mayAdvance && move.CurrentSpeed.Raw > 0)
            {
                move.CurrentVelocity = direction * move.CurrentSpeed;
                FixVec2 delta = move.CurrentVelocity * SimClock.TickSeconds;
                if (delta.LengthSquared() > desiredStep.LengthSquared()) delta = desiredStep;
                if (world.DiagnosticLastDelta.TryGetValue(id.Value, out FixVec2 previousDelta))
                {
                    long dot = (long)previousDelta.X.Raw * delta.X.Raw + (long)previousDelta.Y.Raw * delta.Y.Raw;
                    if (dot < 0)
                    {
                        int now = world.Tick.Value;
                        if (world.DiagnosticLastReversalTick.TryGetValue(id.Value, out int prior) && now - prior <= 10) world.OscillationDiagnostics++;
                        world.DiagnosticLastReversalTick[id.Value] = now;
                    }
                }
                world.DiagnosticLastDelta[id.Value] = delta;
                transform.Position += delta;
            }
            else
            {
                move.CurrentVelocity = FixVec2.Zero;
                if (!nav.HasTarget || move.State == MovementState.Holding) move.CurrentSpeed = Fix32.Zero;
            }

            bool compressed = world.CompressionUsed.TryGetValue(id.Value, out bool used) && used;
            move.CompressionTicks = compressed ? Math.Min(30, move.CompressionTicks + 1) : 0;

            if (FixVec2.DistanceSquared(before, transform.Position) <= ProgressEpsilonSquared && nav.HasTarget) move.StuckTicks++;
            else move.StuckTicks = 0;
            move.LastPosition = transform.Position;

            if (world.Paths.TryGetValue(id.Value, out NavPath path) && move.PathIndex < path.Cells.Count)
            {
                FixVec2 waypoint = MapGrid.NavCellCenterToBuild(path.Cells[move.PathIndex]);
                if (FixVec2.Distance(transform.Position, waypoint) <= ArriveDistance) move.PathIndex++;
            }
            if (nav.HasTarget && FixVec2.Distance(transform.Position, nav.Target) <= Fix32.FromRatio(2, 5)) CompleteOrder(world, id, ref nav, ref move);
            else if (move.StuckTicks == 20)
            {
                move.State = MovementState.StuckRecovery; world.StuckRecoveryDiagnostics++;
            }
            else if (move.StuckTicks == 40)
            {
                nav.PathDirty = true; move.State = MovementState.StuckRecovery;
            }
            else if (move.StuckTicks == 60)
            {
                nav.PathDirty = true; nav.RequestAge = Math.Max(nav.RequestAge, 20); move.State = MovementState.StuckRecovery;
            }
            else if (move.StuckTicks == 100)
            {
                world.DeadlockDiagnostics++; nav.PathDirty = true; nav.RequestAge = Math.Max(nav.RequestAge, 40); move.State = MovementState.StuckRecovery;
            }
        }
    }

    private static void CompleteOrder(SimulationWorld world, EntityId id, ref NavigationAgent nav, ref Movement move)
    {
        UnitCommandQueue queue = world.GetQueue(id);
        if (queue.TryPeek(out UnitOrder next))
        {
            queue.Dequeue(); CommandExecutionSystem.SetMove(world, id, next.Position);
        }
        else
        {
            nav.HasTarget = false; nav.PathDirty = false; move.PathIndex = 0; world.Paths.Remove(id.Value); move.State = MovementState.Idle; move.CurrentSpeed = Fix32.Zero; move.CurrentVelocity = FixVec2.Zero; move.DesiredMovement = FixVec2.Zero;
        }
    }
}

public sealed class SpatialIndexSystem : ISimSystem { public void Step(SimulationWorld world) => world.Spatial.Rebuild(world.Entities); }

public sealed class VisionSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        world.Fog.ClearCurrent();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Vision.TryGet(id, out Vision vision) || !world.Entities.Transform.TryGet(id, out SimTransform transform) || !world.Entities.Ownership.TryGet(id, out Ownership ownership)) continue;
            int sx = transform.Position.X.FloorToInt(), sy = transform.Position.Y.FloorToInt(); int r = vision.RadiusBuildCells;
            for (int y = sy - r; y <= sy + r; y++)
                for (int x = sx - r; x <= sx + r; x++)
                {
                    if ((uint)x >= MapGrid.BuildWidth || (uint)y >= MapGrid.BuildHeight) continue;
                    int dx = x - sx, dy = y - sy; if (dx * dx + dy * dy > r * r) continue;
                    if (HasLineOfSight(world.Map, sx, sy, x, y)) world.Fog.AddVisible(ownership.PlayerSlot, x, y);
                }
        }
    }

    private static bool HasLineOfSight(MapGrid map, int x0, int y0, int x1, int y1)
    {
        int e0=BuildElevation(map,x0,y0),e1=BuildElevation(map,x1,y1);
        int totalSteps=Math.Max(Math.Abs(x1-x0),Math.Abs(y1-y0));if(totalSteps==0)return true;
        int stepIndex=0;
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1; int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1; int err = dx + dy;
        while (true)
        {
            if (x0 == x1 && y0 == y1) return true;
            int e2 = err << 1; if (e2 >= dy) { err += dy; x0 += sx; } if (e2 <= dx) { err += dx; y0 += sy; }
            stepIndex++;
            if (x0 == x1 && y0 == y1) return true;
            int nx = x0 * MapGrid.NavPerBuild, ny = y0 * MapGrid.NavPerBuild;
            for (int oy = 0; oy < MapGrid.NavPerBuild; oy++) for (int ox = 0; ox < MapGrid.NavPerBuild; ox++)
                if ((map.GetFlags(nx + ox, ny + oy) & MapCellFlags.GroundOccluder) != 0) return false;
            int terrainElevation=BuildElevation(map,x0,y0);
            int sightLineNumerator=e0*(totalSteps-stepIndex)+e1*stepIndex;
            if(terrainElevation*totalSteps>sightLineNumerator+totalSteps)return false;
        }
    }

    private static int BuildElevation(MapGrid map,int buildX,int buildY)
    {
        int nx=buildX*MapGrid.NavPerBuild,ny=buildY*MapGrid.NavPerBuild,max=sbyte.MinValue;
        for(int oy=0;oy<MapGrid.NavPerBuild;oy++)for(int ox=0;ox<MapGrid.NavPerBuild;ox++)max=Math.Max(max,map.GetElevation(nx+ox,ny+oy));
        return max;
    }
}
}
