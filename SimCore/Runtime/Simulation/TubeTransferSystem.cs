using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class TubeTransitRoute
{
    public readonly List<EntityId> Links = new();
}

public sealed class TubeTransferSystem : ISimSystem
{
    public const int LoadingTicks = 3 * SimClock.TicksPerSecond;
    public const int UnloadingTicks = 2 * SimClock.TicksPerSecond;
    public const int ArrivalRecoveryTicks = 15;
    public const int ExitWaitLimitTicks = 4 * SimClock.TicksPerSecond;
    public const int BrokenRouteReturnTicks = 6 * SimClock.TicksPerSecond;
    public const byte BaselineStationCapacity = 2;
    public const byte HypersledStationCapacity = 3;

    private static readonly ContentId WorkerRobot = StableId.FromKey("unit.mar.worker_robot");
    private static readonly ContentId DoubleHover = StableId.FromKey("unit.mar.double_hover");
    private static readonly ContentId JetScooter = StableId.FromKey("unit.mar.jet_scooter");
    private static readonly Fix32 ApproachDistance = Fix32.FromRatio(3, 4);

    public static int GetTravelTicks(int lengthBuildCells)
        => lengthBuildCells <= 0 ? throw new ArgumentOutOfRangeException(nameof(lengthBuildCells)) : checked((lengthBuildCells * 12 + 4) / 5);

    public static bool IsEligible(SimulationWorld world, EntityId passenger)
    {
        if (!world.Entities.Selectable.TryGet(passenger, out Selectable selectable) ||
            (selectable.Kind != SelectableKind.Worker && selectable.Kind != SelectableKind.CombatSupport)) return false;
        return selectable.ContentType == WorkerRobot || selectable.ContentType == DoubleHover || selectable.ContentType == JetScooter;
    }

    public static bool TryQueueTransfer(SimulationWorld world, byte playerSlot, EntityId passenger, EntityId origin, EntityId destination)
    {
        if (origin == destination || world.Entities.TubeTransfer.Has(passenger) || !IsEligible(world, passenger) ||
            !OwnedBy(world, passenger, playerSlot) || !OwnedBy(world, origin, playerSlot) || !OwnedBy(world, destination, playerSlot) ||
            !world.Entities.Transform.Has(passenger) || !world.Entities.Navigation.Has(passenger) || !world.Entities.Movement.Has(passenger)) return false;
        List<EntityId> links = new();
        if (!TubeGraphSystem.TryFindOperationalRoute(world, origin, destination, links) ||
            !TubeGraphSystem.TryGetStationSocket(world, links[0], origin, out FixVec2 socket)) return false;
        TubeTransitRoute route = new(); route.Links.AddRange(links); world.TubeTransitRoutes.Add(passenger.Value, route);
        world.Entities.TubeTransfer.Set(passenger, new TubeTransfer
        {
            Origin = origin, Destination = destination, RequestedTick = world.Tick.Value, DepartureTick = -1,
            State = TubeTransferState.Approaching
        });
        CommandExecutionSystem.SetMove(world, passenger, socket);
        return true;
    }

    public static bool CaptureArrivalMoveOrder(SimulationWorld world, EntityId passenger, FixVec2 target)
    {
        if (!world.Entities.TubeTransfer.Has(passenger)) return false;
        ref TubeTransfer transfer = ref world.Entities.TubeTransfer.Get(passenger);
        transfer.HasArrivalMoveOrder = true; transfer.ArrivalMoveTarget = target;
        return true;
    }

    public static bool CanAttack(SimulationWorld world, EntityId passenger)
        => !world.Entities.TubeTransfer.TryGet(passenger, out TubeTransfer transfer) ||
           transfer.State == TubeTransferState.Approaching || transfer.State == TubeTransferState.Queued;

    public void Step(SimulationWorld world)
    {
        RemoveOrphanRoutes(world);
        List<EntityId> passengers = Collect(world);
        for (int i = 0; i < passengers.Count; i++) Advance(world, passengers[i]);
        StartQueuedTransfers(world, Collect(world));
    }

    private static void Advance(SimulationWorld world, EntityId passenger)
    {
        if (!world.Entities.TubeTransfer.TryGet(passenger, out TubeTransfer copy)) return;
        ref TubeTransfer transfer = ref world.Entities.TubeTransfer.Get(passenger);
        switch (transfer.State)
        {
            case TubeTransferState.Approaching:
                if (!world.TubeTransitRoutes.TryGetValue(passenger.Value, out TubeTransitRoute route) || route.Links.Count == 0 ||
                    !TubeGraphSystem.TryGetStationSocket(world, route.Links[0], transfer.Origin, out FixVec2 socket)) { Cancel(world, passenger); return; }
                if (world.Entities.Transform.TryGet(passenger, out SimTransform transform) && FixVec2.Distance(transform.Position, socket) <= ApproachDistance)
                {
                    Stop(world, passenger); transfer.State = TubeTransferState.Queued;
                }
                break;
            case TubeTransferState.Loading:
                if (--transfer.RemainingTicks <= 0) EnterNetwork(world, passenger, ref transfer);
                break;
            case TubeTransferState.Travelling:
                if (!ValidateOrRecoverRoute(world, passenger, ref transfer)) return;
                if (--transfer.RemainingTicks <= 0) { transfer.State = TubeTransferState.Unloading; transfer.RemainingTicks = UnloadingTicks; }
                else UpdateCurrentEdge(world, passenger, ref transfer);
                break;
            case TubeTransferState.Unloading:
                if (--transfer.RemainingTicks <= 0) TryExit(world, passenger, ref transfer, false);
                break;
            case TubeTransferState.ExitBlocked:
                transfer.ExitWaitTicks++;
                TryExit(world, passenger, ref transfer, transfer.ExitWaitTicks >= ExitWaitLimitTicks);
                break;
            case TubeTransferState.Returning:
                if (--transfer.RemainingTicks <= 0) TryExit(world, passenger, ref transfer, true, transfer.Origin);
                break;
            case TubeTransferState.ArrivalRecovery:
                if (--transfer.RemainingTicks <= 0) Complete(world, passenger, transfer);
                break;
        }
    }

    private static void StartQueuedTransfers(SimulationWorld world, List<EntityId> passengers)
    {
        passengers.Sort((a, b) =>
        {
            TubeTransfer ta = world.Entities.TubeTransfer.Get(a), tb = world.Entities.TubeTransfer.Get(b);
            int c = ta.RequestedTick.CompareTo(tb.RequestedTick); return c != 0 ? c : a.Value.CompareTo(b.Value);
        });
        for (int i = 0; i < passengers.Count; i++)
        {
            EntityId passenger = passengers[i]; ref TubeTransfer transfer = ref world.Entities.TubeTransfer.Get(passenger);
            if (transfer.State != TubeTransferState.Queued || !HasCapacity(world, transfer.Origin, passenger) || !HasCapacity(world, transfer.Destination, passenger)) continue;
            transfer.State = TubeTransferState.Loading; transfer.RemainingTicks = LoadingTicks;
        }
    }

    private static void EnterNetwork(SimulationWorld world, EntityId passenger, ref TubeTransfer transfer)
    {
        if (!world.TubeTransitRoutes.TryGetValue(passenger.Value, out TubeTransitRoute route) || !TryRouteLength(world, route, out int length)) { Cancel(world, passenger); return; }
        Stop(world, passenger); world.Entities.Transform.Remove(passenger);
        transfer.DepartureTick = world.Tick.Value;
        transfer.State = TubeTransferState.Travelling; transfer.TotalTravelTicks = GetTravelTicks(length);
        transfer.RemainingTicks = transfer.TotalTravelTicks; transfer.CurrentEdgeIndex = 0;
    }

    private static bool ValidateOrRecoverRoute(SimulationWorld world, EntityId passenger, ref TubeTransfer transfer)
    {
        if (world.TubeTransitRoutes.TryGetValue(passenger.Value, out TubeTransitRoute current) && RouteOperational(world, current)) return true;
        if (world.Entities.TubeStation.TryGet(transfer.Origin, out TubeStation origin) && origin.RedundantRoutingUnlocked)
        {
            List<EntityId> links = new();
            if (TubeGraphSystem.TryFindOperationalRoute(world, transfer.Origin, transfer.Destination, links))
            {
                TubeTransitRoute replacement = new(); replacement.Links.AddRange(links); world.TubeTransitRoutes[passenger.Value] = replacement;
                TryRouteLength(world, replacement, out int length); transfer.TotalTravelTicks = GetTravelTicks(length);
                transfer.RemainingTicks = transfer.TotalTravelTicks; transfer.CurrentEdgeIndex = 0; return true;
            }
        }
        transfer.State = TubeTransferState.Returning; transfer.RemainingTicks = BrokenRouteReturnTicks; transfer.CurrentEdgeIndex = 0; return false;
    }

    private static void TryExit(SimulationWorld world, EntityId passenger, ref TubeTransfer transfer, bool allowFallback, EntityId stationOverride = default)
    {
        EntityId station = stationOverride == EntityId.None ? transfer.Destination : stationOverride;
        if (!TryExitSocket(world, passenger, station, out FixVec2 socket) || !TryFindExit(world, passenger, socket, allowFallback, out FixVec2 exit))
        {
            if (stationOverride == EntityId.None) transfer.State = TubeTransferState.ExitBlocked;
            else { transfer.State = TubeTransferState.Returning; transfer.RemainingTicks = 0; }
            return;
        }
        world.Entities.Transform.Set(passenger, new SimTransform { Position = exit, Orientation = Angle16.Zero });
        transfer.State = TubeTransferState.ArrivalRecovery; transfer.RemainingTicks = ArrivalRecoveryTicks; transfer.ExitWaitTicks = 0;
    }

    private static bool TryExitSocket(SimulationWorld world, EntityId passenger, EntityId station, out FixVec2 socket)
    {
        socket = FixVec2.Zero;
        if (world.TubeTransitRoutes.TryGetValue(passenger.Value, out TubeTransitRoute route) && route.Links.Count > 0)
        {
            EntityId link = station == world.Entities.TubeTransfer.Get(passenger).Origin ? route.Links[0] : route.Links[route.Links.Count - 1];
            if (TubeGraphSystem.TryGetStationSocket(world, link, station, out socket)) return true;
        }
        if (!world.Entities.Transform.TryGet(station, out SimTransform transform)) return false;
        socket = transform.Position;
        if (world.Entities.Building.TryGet(station, out Building building)) socket += new FixVec2(Fix32.FromRatio(building.FootprintWidth + 1, 2), Fix32.Zero);
        return true;
    }

    private static bool TryFindExit(SimulationWorld world, EntityId passenger, FixVec2 socket, bool allowFallback, out FixVec2 exit)
    {
        NavigationAgent nav = world.Entities.Navigation.Get(passenger); NavCell center = MapGrid.BuildToNav(socket);
        int maxRadius = allowFallback ? 6 : 2;
        for (int radius = 0; radius <= maxRadius; radius++)
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;
            int x = center.X + dx, y = center.Y + dy;
            if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight) continue;
            NavCell cell = new(checked((short)x), checked((short)y));
            if (!world.Pathfinder.IsPassable(cell, nav.Footprint)) continue;
            FixVec2 candidate = MapGrid.NavCellCenterToBuild(cell);
            if (IsOccupied(world, passenger, candidate)) continue;
            exit = candidate; return true;
        }
        exit = default; return false;
    }

    private static bool IsOccupied(SimulationWorld world, EntityId passenger, FixVec2 candidate)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (alive[i] != passenger && world.Entities.Transform.TryGet(alive[i], out SimTransform other) &&
                world.Entities.Navigation.Has(alive[i]) && FixVec2.Distance(candidate, other.Position) < Fix32.One) return true;
        return false;
    }

    private static void Complete(SimulationWorld world, EntityId passenger, TubeTransfer transfer)
    {
        world.Entities.TubeTransfer.Remove(passenger); world.TubeTransitRoutes.Remove(passenger.Value);
        if (transfer.HasArrivalMoveOrder) CommandExecutionSystem.SetMove(world, passenger, transfer.ArrivalMoveTarget);
    }

    private static void Cancel(SimulationWorld world, EntityId passenger)
    {
        world.Entities.TubeTransfer.Remove(passenger); world.TubeTransitRoutes.Remove(passenger.Value);
    }

    private static void Stop(SimulationWorld world, EntityId passenger)
    {
        if (!world.Entities.Navigation.Has(passenger) || !world.Entities.Movement.Has(passenger)) return;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(passenger); ref Movement move = ref world.Entities.Movement.Get(passenger);
        CommandExecutionSystem.StopMovement(world, passenger, ref nav, ref move);
    }

    private static bool HasCapacity(SimulationWorld world, EntityId station, EntityId candidate)
    {
        if (!world.Entities.TubeStation.TryGet(station, out TubeStation tube)) return false;
        int used = 0; IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i]; if (id == candidate || !world.Entities.TubeTransfer.TryGet(id, out TubeTransfer transfer)) continue;
            if (transfer.State != TubeTransferState.Approaching && transfer.State != TubeTransferState.Queued && transfer.State != TubeTransferState.ArrivalRecovery &&
                (transfer.Origin == station || transfer.Destination == station)) used++;
        }
        return used < (tube.HypersledThroughputUnlocked ? HypersledStationCapacity : BaselineStationCapacity);
    }

    private static void UpdateCurrentEdge(SimulationWorld world, EntityId passenger, ref TubeTransfer transfer)
    {
        if (!world.TubeTransitRoutes.TryGetValue(passenger.Value, out TubeTransitRoute route)) return;
        int elapsed = transfer.TotalTravelTicks - transfer.RemainingTicks, cumulative = 0;
        for (ushort i = 0; i < route.Links.Count; i++)
        {
            cumulative += GetTravelTicks(world.Entities.TubeLink.Get(route.Links[i]).LengthBuildCells);
            if (elapsed < cumulative) { transfer.CurrentEdgeIndex = i; return; }
        }
        transfer.CurrentEdgeIndex = checked((ushort)(route.Links.Count - 1));
    }

    private static bool RouteOperational(SimulationWorld world, TubeTransitRoute route)
    {
        if (route.Links.Count == 0) return false;
        for (int i = 0; i < route.Links.Count; i++) if (!world.Entities.TubeLink.TryGet(route.Links[i], out TubeLink link) || !link.IsOperational) return false;
        return true;
    }

    private static bool TryRouteLength(SimulationWorld world, TubeTransitRoute route, out int length)
    {
        length = 0;
        for (int i = 0; i < route.Links.Count; i++)
        {
            if (!world.Entities.TubeLink.TryGet(route.Links[i], out TubeLink link) || !link.IsOperational) return false;
            length = checked(length + link.LengthBuildCells);
        }
        return length > 0;
    }

    private static List<EntityId> Collect(SimulationWorld world)
    {
        List<EntityId> result = new(); IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.TubeTransfer.Has(alive[i])) result.Add(alive[i]);
        return result;
    }

    private static void RemoveOrphanRoutes(SimulationWorld world)
    {
        List<uint> orphaned = new();
        foreach (uint id in world.TubeTransitRoutes.Keys)
            if (!world.Entities.Exists(new EntityId(id)) || !world.Entities.TubeTransfer.Has(new EntityId(id))) orphaned.Add(id);
        orphaned.Sort(); for (int i = 0; i < orphaned.Count; i++) world.TubeTransitRoutes.Remove(orphaned[i]);
    }

    private static bool OwnedBy(SimulationWorld world, EntityId id, byte playerSlot)
        => world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot;
}
}
