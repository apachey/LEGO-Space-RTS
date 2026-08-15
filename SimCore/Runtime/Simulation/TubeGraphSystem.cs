using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct TubeBuildCell : IComparable<TubeBuildCell>, IEquatable<TubeBuildCell>
{
    public readonly short X;
    public readonly short Y;
    public TubeBuildCell(short x, short y) { X = x; Y = y; }
    public int CompareTo(TubeBuildCell other) { int c = Y.CompareTo(other.Y); return c != 0 ? c : X.CompareTo(other.X); }
    public bool Equals(TubeBuildCell other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is TubeBuildCell other && Equals(other);
    public override int GetHashCode() => (X * 397) ^ Y;
    public static bool operator ==(TubeBuildCell a, TubeBuildCell b) => a.Equals(b);
    public static bool operator !=(TubeBuildCell a, TubeBuildCell b) => !a.Equals(b);
}

public sealed class TubeRoute
{
    public readonly List<TubeBuildCell> Cells = new();
}

public static class TubeGraphSystem
{
    public const byte SettlementStationConnectionLimit = 3;
    public const byte AeroTubeHangarConnectionLimit = 5;
    public const byte RedundantRoutingBonusConnections = 1;
    public const byte ActiveLinkEnergyDemandPerSecond = 1;
    public const int LinkBaseOreCost = 50;
    public const int LinkOreCostPerBuildCell = 2;
    public const int LinkActivationEnergyCost = 10;
    public const int LinkBaseConstructionTicks = 10 * EnergyDomainSystem.TicksPerSecond;
    public const int LinkConstructionTicksPerBuildCell = 8;

    private static readonly ContentId AeroTubeHangarType = StableId.FromKey("building.mar.aero_tube_hangar");
    private static readonly ContentId SettlementStationType = StableId.FromKey("building.mar.settlement_station");

    public static int GetOreCost(int lengthBuildCells)
        => lengthBuildCells <= 0 ? throw new ArgumentOutOfRangeException(nameof(lengthBuildCells)) : checked(LinkBaseOreCost + lengthBuildCells * LinkOreCostPerBuildCell);

    public static int GetConstructionTicks(int lengthBuildCells)
        => lengthBuildCells <= 0 ? throw new ArgumentOutOfRangeException(nameof(lengthBuildCells)) : checked(LinkBaseConstructionTicks + lengthBuildCells * LinkConstructionTicksPerBuildCell);

    public static bool TryRegisterStation(SimulationWorld world, EntityId entity, bool redundantRoutingUnlocked = false)
    {
        if (world.Entities.TubeStation.Has(entity) || !TryGetBaseLimit(world, entity, out byte baseLimit)) return false;
        world.Entities.TubeStation.Set(entity, new TubeStation
        {
            ConnectionLimit = checked((byte)(baseLimit + (redundantRoutingUnlocked ? RedundantRoutingBonusConnections : 0))),
            RedundantRoutingUnlocked = redundantRoutingUnlocked
        });
        Rebuild(world);
        return true;
    }

    public static bool TrySetRedundantRouting(SimulationWorld world, byte playerSlot, EntityId station, bool unlocked)
    {
        if (!IsOwnedStation(world, station, playerSlot) || !TryGetBaseLimit(world, station, out byte baseLimit)) return false;
        ref TubeStation state = ref world.Entities.TubeStation.Get(station);
        byte nextLimit = checked((byte)(baseLimit + (unlocked ? RedundantRoutingBonusConnections : 0)));
        if (!unlocked && state.ConnectionCount > nextLimit) return false;
        if (state.RedundantRoutingUnlocked == unlocked && state.ConnectionLimit == nextLimit) return false;
        state.RedundantRoutingUnlocked = unlocked;
        state.ConnectionLimit = nextLimit;
        Rebuild(world);
        return true;
    }

    public static bool TryAddCompletedLink(SimulationWorld world, byte playerSlot, EntityId first, EntityId second, out EntityId link)
    {
        link = EntityId.None;
        if (first == second || !IsOwnedStation(world, first, playerSlot) || !IsOwnedStation(world, second, playerSlot)) return false;
        Rebuild(world);
        if (HasDirectLink(world, first, second) || world.Entities.TubeStation.Get(first).ConnectionCount >= world.Entities.TubeStation.Get(first).ConnectionLimit ||
            world.Entities.TubeStation.Get(second).ConnectionCount >= world.Entities.TubeStation.Get(second).ConnectionLimit) return false;
        EntityId endpointA = first.Value < second.Value ? first : second;
        EntityId endpointB = first.Value < second.Value ? second : first;
        if (!TryBuildAutomaticRoute(world, endpointA, endpointB, out TubeRoute route) || route.Cells.Count > ushort.MaxValue) return false;
        link = world.Entities.Create();
        world.Entities.Ownership.Set(link, new Ownership { PlayerSlot = playerSlot });
        world.Entities.TubeLink.Set(link, new TubeLink
        {
            EndpointA = endpointA,
            EndpointB = endpointB,
            LengthBuildCells = checked((ushort)route.Cells.Count),
            EnergyDemandPerSecond = ActiveLinkEnergyDemandPerSecond,
            IsOperational = true
        });
        world.TubeRoutes.Add(link.Value, route);
        Rebuild(world);
        return true;
    }

    public static bool TrySetLinkOperational(SimulationWorld world, byte playerSlot, EntityId link, bool operational)
    {
        if (!world.Entities.TubeLink.TryGet(link, out TubeLink current) || current.IsOperational == operational ||
            !world.Entities.Ownership.TryGet(link, out Ownership owner) || owner.PlayerSlot != playerSlot) return false;
        world.Entities.TubeLink.Get(link).IsOperational = operational;
        Rebuild(world);
        return true;
    }

    public static bool TrySetHypersledThroughput(SimulationWorld world, byte playerSlot, EntityId station, bool unlocked)
    {
        if (!IsOwnedStation(world, station, playerSlot)) return false;
        ref TubeStation state = ref world.Entities.TubeStation.Get(station);
        if (state.HypersledThroughputUnlocked == unlocked) return false;
        state.HypersledThroughputUnlocked = unlocked;
        return true;
    }

    public static bool TryRemoveLink(SimulationWorld world, byte playerSlot, EntityId link)
    {
        if (!world.Entities.TubeLink.Has(link) || !world.Entities.Ownership.TryGet(link, out Ownership owner) || owner.PlayerSlot != playerSlot) return false;
        world.TubeRoutes.Remove(link.Value);
        if (!world.Entities.Destroy(link)) return false;
        Rebuild(world);
        return true;
    }

    public static bool SharesComponent(SimulationWorld world, EntityId first, EntityId second)
        => world.Entities.TubeStation.TryGet(first, out TubeStation a) && world.Entities.TubeStation.TryGet(second, out TubeStation b) &&
           a.ComponentRoot != EntityId.None && a.ComponentRoot == b.ComponentRoot;

    public static bool TryGetComponent(SimulationWorld world, EntityId station, out EntityId root)
    {
        if (world.Entities.TubeStation.TryGet(station, out TubeStation state) && world.Entities.TubeComponent.Has(state.ComponentRoot))
        { root = state.ComponentRoot; return true; }
        root = EntityId.None;
        return false;
    }

    public static bool TryGetStationSocket(SimulationWorld world, EntityId link, EntityId station, out FixVec2 socket)
    {
        socket = FixVec2.Zero;
        if (!world.Entities.TubeLink.TryGet(link, out TubeLink tube) || !world.TubeRoutes.TryGetValue(link.Value, out TubeRoute route) || route.Cells.Count == 0) return false;
        TubeBuildCell cell;
        if (tube.EndpointA == station) cell = route.Cells[0];
        else if (tube.EndpointB == station) cell = route.Cells[route.Cells.Count - 1];
        else return false;
        socket = new FixVec2(Fix32.FromRatio(cell.X * 2 + 1, 2), Fix32.FromRatio(cell.Y * 2 + 1, 2));
        return true;
    }

    public static bool TryFindOperationalRoute(SimulationWorld world, EntityId origin, EntityId destination, List<EntityId> output)
    {
        output.Clear();
        if (origin == destination || !SharesComponent(world, origin, destination)) return false;
        Queue<EntityId> queue = new();
        Dictionary<uint, uint> parent = new();
        Dictionary<uint, EntityId> parentLink = new();
        parent.Add(origin.Value, 0); queue.Enqueue(origin);
        while (queue.Count > 0 && !parent.ContainsKey(destination.Value))
        {
            EntityId current = queue.Dequeue();
            List<EntityId> candidates = new();
            IReadOnlyList<EntityId> alive = world.Entities.Alive;
            for (int i = 0; i < alive.Count; i++)
                if (world.Entities.TubeLink.TryGet(alive[i], out TubeLink link) && link.IsOperational && (link.EndpointA == current || link.EndpointB == current)) candidates.Add(alive[i]);
            candidates.Sort((a, b) => a.Value.CompareTo(b.Value));
            for (int i = 0; i < candidates.Count; i++)
            {
                TubeLink link = world.Entities.TubeLink.Get(candidates[i]);
                EntityId next = link.EndpointA == current ? link.EndpointB : link.EndpointA;
                if (parent.ContainsKey(next.Value)) continue;
                parent.Add(next.Value, current.Value); parentLink.Add(next.Value, candidates[i]); queue.Enqueue(next);
            }
        }
        if (!parent.ContainsKey(destination.Value)) return false;
        EntityId cursor = destination;
        while (cursor != origin)
        {
            output.Add(parentLink[cursor.Value]);
            cursor = new EntityId(parent[cursor.Value]);
        }
        output.Reverse();
        return output.Count > 0;
    }

    public static int GetConnectedProcessedResourceTotal(SimulationWorld world, EntityId station, ResourceType type)
    {
        if (!TryGetComponent(world, station, out EntityId root)) return 0;
        int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.TubeStation.TryGet(id, out TubeStation member) || member.ComponentRoot != root ||
                !world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type) continue;
            total = checked(total + bank.ProcessedAmount);
        }
        return total;
    }

    public static bool TrySpendConnectedProcessedResource(SimulationWorld world, byte playerSlot, EntityId station, ResourceType type, int amount)
    {
        if (amount < 0 || !IsOwnedStation(world, station, playerSlot) || !TryGetComponent(world, station, out EntityId root)) return false;
        List<EntityId> banks = new(); int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.TubeStation.TryGet(id, out TubeStation member) || member.ComponentRoot != root ||
                !world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot ||
                !world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type || bank.ProcessedAmount < 0) continue;
            banks.Add(id); total = checked(total + bank.ProcessedAmount);
        }
        if (total < amount) return false;
        banks.Sort((a, b) => a == b ? 0 : a == station ? -1 : b == station ? 1 : a.Value.CompareTo(b.Value));
        int remaining = amount;
        for (int i = 0; i < banks.Count && remaining > 0; i++)
        {
            ref ResourceBank bank = ref world.Entities.ResourceBank.Get(banks[i]);
            int spent = Math.Min(bank.ProcessedAmount, remaining);
            bank.ProcessedAmount -= spent; remaining -= spent;
        }
        return remaining == 0;
    }

    public static bool Rebuild(SimulationWorld world)
    {
        List<EntityId> stations = CollectStations(world);
        HashSet<uint> stationSet = new();
        for (int i = 0; i < stations.Count; i++) stationSet.Add(stations[i].Value);

        bool cleaned = false;
        List<EntityId> invalidLinks = new();
        List<EntityId> links = new();
        HashSet<ulong> endpointPairs = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.TubeLink.TryGet(id, out TubeLink tube)) continue;
            ulong endpointPair = ((ulong)tube.EndpointA.Value << 32) | tube.EndpointB.Value;
            if (!stationSet.Contains(tube.EndpointA.Value) || !stationSet.Contains(tube.EndpointB.Value) || tube.EndpointA.Value >= tube.EndpointB.Value || !endpointPairs.Add(endpointPair) ||
                !world.Entities.Ownership.TryGet(id, out Ownership linkOwner) || !world.Entities.Ownership.TryGet(tube.EndpointA, out Ownership aOwner) ||
                !world.Entities.Ownership.TryGet(tube.EndpointB, out Ownership bOwner) || linkOwner.PlayerSlot != aOwner.PlayerSlot || aOwner.PlayerSlot != bOwner.PlayerSlot ||
                tube.EnergyDemandPerSecond != ActiveLinkEnergyDemandPerSecond || !world.TubeRoutes.TryGetValue(id.Value, out TubeRoute route) ||
                !RouteIsStructurallyValid(route) || route.Cells.Count != tube.LengthBuildCells)
                invalidLinks.Add(id);
            else links.Add(id);
        }
        for (int i = 0; i < invalidLinks.Count; i++)
        {
            world.TubeRoutes.Remove(invalidLinks[i].Value);
            world.Entities.Destroy(invalidLinks[i]);
            cleaned = true;
        }
        List<uint> orphanRoutes = new();
        foreach (uint key in world.TubeRoutes.Keys)
            if (!world.Entities.Exists(new EntityId(key)) || !world.Entities.TubeLink.Has(new EntityId(key))) orphanRoutes.Add(key);
        for (int i = 0; i < orphanRoutes.Count; i++) { world.TubeRoutes.Remove(orphanRoutes[i]); cleaned = true; }

        Dictionary<uint, uint> oldRoots = new();
        for (int i = 0; i < stations.Count; i++)
            if (world.Entities.TubeStation.TryGet(stations[i], out TubeStation old)) oldRoots[stations[i].Value] = old.ComponentRoot.Value;

        Dictionary<uint, List<uint>> adjacency = new();
        Dictionary<uint, byte> connectionCounts = new();
        for (int i = 0; i < stations.Count; i++) adjacency.Add(stations[i].Value, new List<uint>());
        for (int i = 0; i < links.Count; i++)
        {
            TubeLink tube = world.Entities.TubeLink.Get(links[i]);
            Increment(connectionCounts, tube.EndpointA.Value); Increment(connectionCounts, tube.EndpointB.Value);
            if (!tube.IsOperational) continue;
            adjacency[tube.EndpointA.Value].Add(tube.EndpointB.Value);
            adjacency[tube.EndpointB.Value].Add(tube.EndpointA.Value);
        }
        foreach (List<uint> neighbors in adjacency.Values) neighbors.Sort();

        Dictionary<uint, uint> roots = new();
        Queue<uint> queue = new();
        for (int i = 0; i < stations.Count; i++)
        {
            uint start = stations[i].Value;
            if (roots.ContainsKey(start)) continue;
            List<uint> component = new(); uint root = start;
            roots.Add(start, 0); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                uint current = queue.Dequeue(); component.Add(current); if (current < root) root = current;
                List<uint> neighbors = adjacency[current];
                for (int n = 0; n < neighbors.Count; n++) if (!roots.ContainsKey(neighbors[n])) { roots.Add(neighbors[n], 0); queue.Enqueue(neighbors[n]); }
            }
            for (int n = 0; n < component.Count; n++) roots[component[n]] = root;
        }

        Dictionary<uint, ushort> stationCounts = new(), operationalLinkCounts = new();
        for (int i = 0; i < stations.Count; i++) Increment(stationCounts, roots[stations[i].Value]);
        for (int i = 0; i < links.Count; i++)
        {
            TubeLink tube = world.Entities.TubeLink.Get(links[i]);
            if (tube.IsOperational) Increment(operationalLinkCounts, roots[tube.EndpointA.Value]);
        }

        bool matches = !cleaned && Matches(world, stations, links, roots, connectionCounts, stationCounts, operationalLinkCounts);
        if (matches) return false;

        bool segmented = false;
        for (int i = 0; i < stations.Count && !segmented; i++)
        for (int j = i + 1; j < stations.Count; j++)
        {
            uint a = stations[i].Value, b = stations[j].Value;
            if (oldRoots.TryGetValue(a, out uint oldA) && oldRoots.TryGetValue(b, out uint oldB) && oldA != 0 && oldA == oldB && roots[a] != roots[b])
            { segmented = true; break; }
        }

        world.TubeTopologyRevision = checked(world.TubeTopologyRevision + 1);
        if (segmented) world.TubeSegmentationRevision = checked(world.TubeSegmentationRevision + 1);
        alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) world.Entities.TubeComponent.Remove(alive[i]);
        for (int i = 0; i < stations.Count; i++)
        {
            EntityId id = stations[i]; ref TubeStation station = ref world.Entities.TubeStation.Get(id);
            TryGetBaseLimit(world, id, out byte baseLimit);
            station.ComponentRoot = new EntityId(roots[id.Value]);
            station.ConnectionCount = connectionCounts.TryGetValue(id.Value, out byte count) ? count : (byte)0;
            station.ConnectionLimit = checked((byte)(baseLimit + (station.RedundantRoutingUnlocked ? RedundantRoutingBonusConnections : 0)));
        }
        for (int i = 0; i < links.Count; i++)
        {
            ref TubeLink tube = ref world.Entities.TubeLink.Get(links[i]);
            tube.ComponentRoot = tube.IsOperational ? new EntityId(roots[tube.EndpointA.Value]) : EntityId.None;
        }
        List<uint> orderedRoots = new(stationCounts.Keys); orderedRoots.Sort();
        for (int i = 0; i < orderedRoots.Count; i++)
        {
            uint root = orderedRoots[i];
            world.Entities.TubeComponent.Set(new EntityId(root), new TubeComponent
            {
                StationCount = stationCounts[root],
                OperationalLinkCount = operationalLinkCounts.TryGetValue(root, out ushort count) ? count : (ushort)0,
                TopologyRevision = world.TubeTopologyRevision
            });
        }
        return true;
    }

    public static bool RouteIsStructurallyValid(TubeRoute route)
    {
        if (route == null || route.Cells.Count == 0 || route.Cells.Count > ushort.MaxValue) return false;
        for (int i = 0; i < route.Cells.Count; i++)
        {
            TubeBuildCell cell = route.Cells[i];
            if (cell.X < 0 || cell.Y < 0 || cell.X >= MapGrid.BuildWidth || cell.Y >= MapGrid.BuildHeight) return false;
            if (i > 0)
            {
                TubeBuildCell previous = route.Cells[i - 1];
                if (Math.Abs(cell.X - previous.X) + Math.Abs(cell.Y - previous.Y) != 1) return false;
            }
        }
        return true;
    }

    private static bool Matches(SimulationWorld world, List<EntityId> stations, List<EntityId> links, Dictionary<uint, uint> roots,
        Dictionary<uint, byte> connectionCounts, Dictionary<uint, ushort> stationCounts, Dictionary<uint, ushort> operationalLinkCounts)
    {
        int componentCount = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.TubeComponent.Has(alive[i])) componentCount++;
        if (componentCount != stationCounts.Count) return false;
        for (int i = 0; i < stations.Count; i++)
        {
            EntityId id = stations[i]; TubeStation station = world.Entities.TubeStation.Get(id); TryGetBaseLimit(world, id, out byte baseLimit);
            byte count = connectionCounts.TryGetValue(id.Value, out byte value) ? value : (byte)0;
            if (station.ComponentRoot.Value != roots[id.Value] || station.ConnectionCount != count ||
                station.ConnectionLimit != baseLimit + (station.RedundantRoutingUnlocked ? RedundantRoutingBonusConnections : 0)) return false;
        }
        for (int i = 0; i < links.Count; i++)
        {
            TubeLink link = world.Entities.TubeLink.Get(links[i]);
            EntityId expected = link.IsOperational ? new EntityId(roots[link.EndpointA.Value]) : EntityId.None;
            if (link.ComponentRoot != expected) return false;
        }
        foreach (KeyValuePair<uint, ushort> entry in stationCounts)
        {
            if (!world.Entities.TubeComponent.TryGet(new EntityId(entry.Key), out TubeComponent component) || component.StationCount != entry.Value ||
                component.OperationalLinkCount != (operationalLinkCounts.TryGetValue(entry.Key, out ushort count) ? count : (ushort)0) ||
                component.TopologyRevision != world.TubeTopologyRevision) return false;
        }
        return true;
    }

    private static List<EntityId> CollectStations(SimulationWorld world)
    {
        List<EntityId> stations = new(); List<EntityId> invalid = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.TubeStation.Has(id)) continue;
            if (TryGetBaseLimit(world, id, out _)) stations.Add(id); else invalid.Add(id);
        }
        for (int i = 0; i < invalid.Count; i++) world.Entities.TubeStation.Remove(invalid[i]);
        stations.Sort((a, b) => a.Value.CompareTo(b.Value));
        return stations;
    }

    private static bool TryGetBaseLimit(SimulationWorld world, EntityId entity, out byte limit)
    {
        limit = 0;
        if (!world.Entities.Building.TryGet(entity, out Building building) || building.State != BuildingState.Completed ||
            !world.Entities.Selectable.TryGet(entity, out Selectable selectable) || selectable.ContentType != building.Type ||
            !world.Entities.Ownership.Has(entity) || !world.Entities.Transform.Has(entity)) return false;
        if (building.Type == SettlementStationType) { limit = SettlementStationConnectionLimit; return true; }
        if (building.Type == AeroTubeHangarType) { limit = AeroTubeHangarConnectionLimit; return true; }
        return false;
    }

    private static bool IsOwnedStation(SimulationWorld world, EntityId entity, byte playerSlot)
        => world.Entities.TubeStation.Has(entity) && TryGetBaseLimit(world, entity, out _) &&
           world.Entities.Ownership.TryGet(entity, out Ownership owner) && owner.PlayerSlot == playerSlot;

    private static bool HasDirectLink(SimulationWorld world, EntityId first, EntityId second)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.TubeLink.TryGet(alive[i], out TubeLink link) &&
                ((link.EndpointA == first && link.EndpointB == second) || (link.EndpointA == second && link.EndpointB == first))) return true;
        return false;
    }

    private static bool TryBuildAutomaticRoute(SimulationWorld world, EntityId first, EntityId second, out TubeRoute route)
    {
        route = new TubeRoute();
        if (!TryGetSocket(world, first, second, out TubeBuildCell start) || !TryGetSocket(world, second, first, out TubeBuildCell end)) return false;
        TubeRoute xFirst = BuildOrthogonalRoute(start, end, true);
        if (RouteCanBeBuilt(world, xFirst, first, second)) { route = xFirst; return true; }
        TubeRoute yFirst = BuildOrthogonalRoute(start, end, false);
        if (RouteCanBeBuilt(world, yFirst, first, second)) { route = yFirst; return true; }
        return false;
    }

    private static bool TryGetSocket(SimulationWorld world, EntityId source, EntityId destination, out TubeBuildCell socket)
    {
        Building building = world.Entities.Building.Get(source);
        SimTransform sourceTransform = world.Entities.Transform.Get(source), destinationTransform = world.Entities.Transform.Get(destination);
        int dx = destinationTransform.Position.X.Raw - sourceTransform.Position.X.Raw;
        int dy = destinationTransform.Position.Y.Raw - sourceTransform.Position.Y.Raw;
        int x, y;
        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            x = dx >= 0 ? building.AnchorX + building.FootprintWidth : building.AnchorX - 1;
            y = building.AnchorY + building.FootprintHeight / 2;
        }
        else
        {
            x = building.AnchorX + building.FootprintWidth / 2;
            y = dy >= 0 ? building.AnchorY + building.FootprintHeight : building.AnchorY - 1;
        }
        if (x < 0 || y < 0 || x >= MapGrid.BuildWidth || y >= MapGrid.BuildHeight) { socket = default; return false; }
        socket = new TubeBuildCell((short)x, (short)y); return true;
    }

    private static TubeRoute BuildOrthogonalRoute(TubeBuildCell start, TubeBuildCell end, bool xFirst)
    {
        TubeRoute route = new(); route.Cells.Add(start);
        int x = start.X, y = start.Y;
        if (xFirst) { AppendAxis(route, ref x, ref y, end.X, true); AppendAxis(route, ref x, ref y, end.Y, false); }
        else { AppendAxis(route, ref x, ref y, end.Y, false); AppendAxis(route, ref x, ref y, end.X, true); }
        return route;
    }

    private static void AppendAxis(TubeRoute route, ref int x, ref int y, int target, bool horizontal)
    {
        int current = horizontal ? x : y;
        int step = target >= current ? 1 : -1;
        while (current != target)
        {
            current += step; if (horizontal) x = current; else y = current;
            route.Cells.Add(new TubeBuildCell(checked((short)x), checked((short)y)));
        }
    }

    private static bool RouteCanBeBuilt(SimulationWorld world, TubeRoute route, EntityId first, EntityId second)
    {
        if (!RouteIsStructurallyValid(route)) return false;
        for (int i = 0; i < route.Cells.Count; i++)
        {
            TubeBuildCell cell = route.Cells[i];
            for (int ny = 0; ny < MapGrid.NavPerBuild; ny++)
            for (int nx = 0; nx < MapGrid.NavPerBuild; nx++)
            {
                MapCellFlags flags = world.Map.GetFlags(cell.X * MapGrid.NavPerBuild + nx, cell.Y * MapGrid.NavPerBuild + ny);
                if ((flags & MapCellFlags.Buildable) == 0 || (flags & MapCellFlags.Impassable) != 0) return false;
            }
            IReadOnlyList<EntityId> alive = world.Entities.Alive;
            for (int e = 0; e < alive.Count; e++)
            {
                EntityId id = alive[e];
                if (id == first || id == second || !world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed) continue;
                if (cell.X >= building.AnchorX && cell.Y >= building.AnchorY && cell.X < building.AnchorX + building.FootprintWidth && cell.Y < building.AnchorY + building.FootprintHeight) return false;
            }
        }
        return true;
    }

    private static void Increment(Dictionary<uint, byte> counts, uint key)
        => counts[key] = checked((byte)(counts.TryGetValue(key, out byte value) ? value + 1 : 1));

    private static void Increment(Dictionary<uint, ushort> counts, uint key)
        => counts[key] = checked((ushort)(counts.TryGetValue(key, out ushort value) ? value + 1 : 1));
}
}
