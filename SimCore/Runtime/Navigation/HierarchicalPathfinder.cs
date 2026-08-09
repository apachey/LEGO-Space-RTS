using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>
/// Deterministic HPA-style planner. 10x10 navigation-cell clusters (5x5 build cells),
/// deterministic border portals, coarse cluster A*, then local deterministic A* constrained
/// to the strategic corridor plus a one-cluster halo. Scratch storage is reused between requests.
/// </summary>
public sealed class HierarchicalPathfinder
{
    public const int ClusterSize = 10;
    public const int ClusterWidth = MapGrid.NavWidth / ClusterSize;
    public const int ClusterHeight = MapGrid.NavHeight / ClusterSize;

    private readonly MapGrid _map;
    private readonly Portal?[,,,] _portals = new Portal?[5, ClusterWidth, ClusterHeight, 4];
    private static readonly int[] Dx = { 1, 0, -1, 0 };
    private static readonly int[] Dy = { 0, 1, 0, -1 };
    private static readonly int[] LocalDx = { 1, 0, -1, 0, 1, -1, -1, 1 };
    private static readonly int[] LocalDy = { 0, 1, 0, -1, 1, 1, -1, -1 };

    private readonly int[] _clusterG = new int[ClusterWidth * ClusterHeight];
    private readonly int[] _clusterParent = new int[ClusterWidth * ClusterHeight];
    private readonly int[] _clusterSeen = new int[ClusterWidth * ClusterHeight];
    private readonly int[] _clusterClosed = new int[ClusterWidth * ClusterHeight];
    private int _clusterStamp;
    private readonly List<int> _clusterOpen = new List<int>(128);
    private readonly List<ClusterCoord> _clusterRoute = new List<ClusterCoord>(64);

    private readonly int[] _navG = new int[MapGrid.NavWidth * MapGrid.NavHeight];
    private readonly int[] _navParent = new int[MapGrid.NavWidth * MapGrid.NavHeight];
    private readonly int[] _navSeen = new int[MapGrid.NavWidth * MapGrid.NavHeight];
    private readonly int[] _navClosed = new int[MapGrid.NavWidth * MapGrid.NavHeight];
    private readonly int[] _allowedStamp = new int[MapGrid.NavWidth * MapGrid.NavHeight];
    private int _navStamp;
    private int _allowedGeneration;
    private readonly List<int> _navOpen = new List<int>(4096);
    private readonly List<NavCell> _reverseCells = new List<NavCell>(1024);
    private readonly List<NavCell> _smoothedCells = new List<NavCell>(256);

    private const int StrategicRouteCacheCapacity = 128;
    private const int MaxCachedClusterRouteLength = 256;
    private readonly RouteCacheEntry[] _routeCache = new RouteCacheEntry[StrategicRouteCacheCapacity];
    private int _routeCacheCount;
    private int _routeCacheNext;

    private sealed class RouteCacheEntry
    {
        public ClusterCoord Start;
        public ClusterCoord Goal;
        public FootprintClass Footprint;
        public int Count;
        public readonly ClusterCoord[] Route = new ClusterCoord[MaxCachedClusterRouteLength];
    }

    public HierarchicalPathfinder(MapGrid map)
    {
        _map = map;
        for (int i = 0; i < _routeCache.Length; i++) _routeCache[i] = new RouteCacheEntry();
        RebuildAll();
    }

    public void RebuildAll()
    {
        ClearRouteCache();
        for (int cy = 0; cy < ClusterHeight; cy++)
            for (int cx = 0; cx < ClusterWidth; cx++)
                RebuildCluster((short)cx, (short)cy);
    }

    public void RebuildAffected(IntRect navRect)
    {
        ClearRouteCache();
        int minCx = Math.Max(0, navRect.X / ClusterSize - 1);
        int minCy = Math.Max(0, navRect.Y / ClusterSize - 1);
        int maxCx = Math.Min(ClusterWidth - 1, (navRect.X + navRect.Width - 1) / ClusterSize + 1);
        int maxCy = Math.Min(ClusterHeight - 1, (navRect.Y + navRect.Height - 1) / ClusterSize + 1);
        for (int cy = minCy; cy <= maxCy; cy++)
            for (int cx = minCx; cx <= maxCx; cx++)
                RebuildCluster((short)cx, (short)cy);
    }

    private void RebuildCluster(short cx, short cy)
    {
        for (int fp = 0; fp < 5; fp++)
            for (int dir = 0; dir < 4; dir++)
            {
                int nx = cx + Dx[dir], ny = cy + Dy[dir];
                _portals[fp, cx, cy, dir] = nx < 0 || ny < 0 || nx >= ClusterWidth || ny >= ClusterHeight
                    ? (Portal?)null
                    : FindPortal(new ClusterCoord(cx, cy), (FootprintClass)fp, dir);
            }
    }

    private Portal? FindPortal(ClusterCoord from, FootprintClass footprint, int dir)
    {
        int bestStart = -1, bestLength = 0, runStart = -1, runLength = 0;
        for (int i = 0; i < ClusterSize; i++)
        {
            NavCell a, b;
            if (dir == 0 || dir == 2)
            {
                int borderX = dir == 0 ? (from.X + 1) * ClusterSize - 1 : from.X * ClusterSize;
                int otherX = borderX + (dir == 0 ? 1 : -1);
                int y = from.Y * ClusterSize + i;
                a = new NavCell((short)borderX, (short)y); b = new NavCell((short)otherX, (short)y);
            }
            else
            {
                int borderY = dir == 1 ? (from.Y + 1) * ClusterSize - 1 : from.Y * ClusterSize;
                int otherY = borderY + (dir == 1 ? 1 : -1);
                int x = from.X * ClusterSize + i;
                a = new NavCell((short)x, (short)borderY); b = new NavCell((short)x, (short)otherY);
            }
            bool open = IsPassable(a, footprint) && IsPassable(b, footprint);
            if (open) { if (runStart < 0) runStart = i; runLength++; }
            else { CommitRun(ref bestStart, ref bestLength, ref runStart, ref runLength); }
        }
        CommitRun(ref bestStart, ref bestLength, ref runStart, ref runLength);
        if (bestLength == 0) return null;
        int mid = bestStart + (bestLength - 1) / 2;
        NavCell cell;
        if (dir == 0 || dir == 2)
        {
            int x = dir == 0 ? (from.X + 1) * ClusterSize - 1 : from.X * ClusterSize;
            cell = new NavCell((short)x, (short)(from.Y * ClusterSize + mid));
        }
        else
        {
            int y = dir == 1 ? (from.Y + 1) * ClusterSize - 1 : from.Y * ClusterSize;
            cell = new NavCell((short)(from.X * ClusterSize + mid), (short)y);
        }
        ClusterCoord to = new ClusterCoord((short)(from.X + Dx[dir]), (short)(from.Y + Dy[dir]));
        return new Portal(cell, from, to);
    }

    private static void CommitRun(ref int bestStart, ref int bestLength, ref int runStart, ref int runLength)
    {
        if (runLength > bestLength) { bestStart = runStart; bestLength = runLength; }
        runStart = -1; runLength = 0;
    }

    public Portal? GetPortal(FootprintClass footprint,int clusterX,int clusterY,int direction)
    {
        if((uint)clusterX>=ClusterWidth||(uint)clusterY>=ClusterHeight||(uint)direction>=4)return null;
        return _portals[(int)footprint,clusterX,clusterY,direction];
    }

    public NavPath FindPath(NavCell start, NavCell goal, FootprintClass footprint)
    {
        NavPath result = new NavPath();
        FindCorridor(start, goal, footprint, result, MovementLayer.Ground);
        return result;
    }

    public void FindPath(NavCell start, NavCell goal, FootprintClass footprint, NavPath result, MovementLayer layer = MovementLayer.Ground)
        => FindCorridor(start, goal, footprint, result, layer);

    public void FindCorridor(NavCell start, NavCell goal, FootprintClass footprint, RouteCorridor result, MovementLayer layer = MovementLayer.Ground)
    {
        result.Cells.Clear(); result.TopologyVersion = _map.TopologyVersion;
        if (layer == MovementLayer.TrueAir)
        {
            if (!_map.InBounds(start.X,start.Y)||!_map.InBounds(goal.X,goal.Y)) return;
            result.Cells.Add(start); if(start!=goal)result.Cells.Add(goal); return;
        }
        if (!IsPassable(start, footprint) || !IsPassable(goal, footprint)) return;
        ClusterCoord startCluster = ToCluster(start), goalCluster = ToCluster(goal);
        if (!BuildClusterRoute(startCluster, goalCluster, footprint)) return;
        MarkAllowedCorridor();
        LocalAStar(start, goal, footprint, result, layer);
        if (result.Cells.Count > 2) SimplifyPath(result, footprint, layer);
    }

    private bool BuildClusterRoute(ClusterCoord start, ClusterCoord goal, FootprintClass footprint)
    {
        _clusterRoute.Clear();
        for (int cacheIndex = 0; cacheIndex < _routeCacheCount; cacheIndex++)
        {
            RouteCacheEntry cached = _routeCache[cacheIndex];
            if (cached.Count > 0 && cached.Start.Equals(start) && cached.Goal.Equals(goal) && cached.Footprint == footprint)
            {
                for (int i = 0; i < cached.Count; i++) _clusterRoute.Add(cached.Route[i]);
                return true;
            }
        }
        int stamp = NextStamp(ref _clusterStamp, _clusterSeen, _clusterClosed);
        _clusterOpen.Clear();
        int startIndex = start.Y * ClusterWidth + start.X, goalIndex = goal.Y * ClusterWidth + goal.X;
        SetClusterNode(startIndex, 0, -1, stamp); _clusterOpen.Add(startIndex);
        while (_clusterOpen.Count > 0)
        {
            int bestPos = 0;
            for (int i = 1; i < _clusterOpen.Count; i++) if (ClusterNodeLess(_clusterOpen[i], _clusterOpen[bestPos], goal, stamp)) bestPos = i;
            int current = _clusterOpen[bestPos]; _clusterOpen.RemoveAt(bestPos);
            if (_clusterClosed[current] == stamp) continue;
            _clusterClosed[current] = stamp;
            if (current == goalIndex)
            {
                while (current >= 0)
                {
                    _clusterRoute.Add(new ClusterCoord((short)(current % ClusterWidth), (short)(current / ClusterWidth)));
                    current = _clusterParent[current];
                }
                _clusterRoute.Reverse();
                StoreRouteCache(start, goal, footprint);
                return true;
            }
            int cx = current % ClusterWidth, cy = current / ClusterWidth;
            int currentG = _clusterG[current];
            for (int dir = 0; dir < 4; dir++)
            {
                if (_portals[(int)footprint, cx, cy, dir] == null) continue;
                int nx = cx + Dx[dir], ny = cy + Dy[dir], ni = ny * ClusterWidth + nx;
                int ng = currentG + 10;
                int oldG = _clusterSeen[ni] == stamp ? _clusterG[ni] : int.MaxValue;
                if (ng < oldG) { SetClusterNode(ni, ng, current, stamp); _clusterOpen.Add(ni); }
            }
        }
        return false;
    }

    private void ClearRouteCache()
    {
        for (int i = 0; i < _routeCacheCount; i++) _routeCache[i].Count = 0;
        _routeCacheCount = 0;
        _routeCacheNext = 0;
    }

    private void StoreRouteCache(ClusterCoord start, ClusterCoord goal, FootprintClass footprint)
    {
        if (_clusterRoute.Count > MaxCachedClusterRouteLength) return;
        int slot;
        if (_routeCacheCount < StrategicRouteCacheCapacity)
        {
            slot = _routeCacheCount;
            _routeCacheCount++;
        }
        else
        {
            slot = _routeCacheNext;
            _routeCacheNext = (_routeCacheNext + 1) % StrategicRouteCacheCapacity;
        }
        RouteCacheEntry entry = _routeCache[slot];
        entry.Start = start; entry.Goal = goal; entry.Footprint = footprint; entry.Count = _clusterRoute.Count;
        for (int i = 0; i < _clusterRoute.Count; i++) entry.Route[i] = _clusterRoute[i];
    }

    private void SetClusterNode(int index, int g, int parent, int stamp)
    { _clusterSeen[index] = stamp; _clusterG[index] = g; _clusterParent[index] = parent; }

    private bool ClusterNodeLess(int a, int b, ClusterCoord goal, int stamp)
    {
        int ax = a % ClusterWidth, ay = a / ClusterWidth, bx = b % ClusterWidth, by = b / ClusterWidth;
        int ah = Math.Abs(goal.X - ax) + Math.Abs(goal.Y - ay), bh = Math.Abs(goal.X - bx) + Math.Abs(goal.Y - by);
        int af = (_clusterSeen[a] == stamp ? _clusterG[a] : int.MaxValue) + ah * 10;
        int bf = (_clusterSeen[b] == stamp ? _clusterG[b] : int.MaxValue) + bh * 10;
        if (af != bf) return af < bf; if (ah != bh) return ah < bh; if (ay != by) return ay < by; return ax < bx;
    }

    private void MarkAllowedCorridor()
    {
        _allowedGeneration++;
        if (_allowedGeneration == int.MaxValue) { Array.Clear(_allowedStamp, 0, _allowedStamp.Length); _allowedGeneration = 1; }
        for (int i = 0; i < _clusterRoute.Count; i++)
        {
            ClusterCoord c = _clusterRoute[i]; MarkClusterAndHalo(c.X, c.Y);
        }
    }

    private void MarkClusterAndHalo(int cx, int cy)
    {
        for (int oy = -1; oy <= 1; oy++)
            for (int ox = -1; ox <= 1; ox++)
            {
                int ncx = cx + ox, ncy = cy + oy;
                if (ncx < 0 || ncy < 0 || ncx >= ClusterWidth || ncy >= ClusterHeight) continue;
                int minX = ncx * ClusterSize, minY = ncy * ClusterSize;
                for (int y = minY; y < minY + ClusterSize; y++)
                    for (int x = minX; x < minX + ClusterSize; x++)
                        _allowedStamp[y * MapGrid.NavWidth + x] = _allowedGeneration;
            }
    }

    private void LocalAStar(NavCell start, NavCell goal, FootprintClass footprint, RouteCorridor output, MovementLayer layer)
    {
        int stamp = NextStamp(ref _navStamp, _navSeen, _navClosed);
        _navOpen.Clear();
        int si = Index(start), gi = Index(goal); SetNavNode(si, 0, -1, stamp); _navOpen.Add(si);
        while (_navOpen.Count > 0)
        {
            int bestPos = 0;
            for (int i = 1; i < _navOpen.Count; i++) if (NavNodeLess(_navOpen[i], _navOpen[bestPos], goal, stamp)) bestPos = i;
            int current = _navOpen[bestPos]; _navOpen.RemoveAt(bestPos);
            if (_navClosed[current] == stamp) continue;
            _navClosed[current] = stamp;
            if (current == gi) { ReconstructCells(current, output); return; }
            int cx = current % MapGrid.NavWidth, cy = current / MapGrid.NavWidth, currentG = _navG[current];
            for (int d = 0; d < 8; d++)
            {
                int nx = cx + LocalDx[d], ny = cy + LocalDy[d];
                if (!_map.InBounds(nx, ny)) continue;
                int ni = ny * MapGrid.NavWidth + nx;
                if (_allowedStamp[ni] != _allowedGeneration) continue;
                NavCell n = new NavCell((short)nx, (short)ny); if (!IsPassable(n, footprint)) continue;
                if (d >= 4)
                {
                    NavCell sideA = new NavCell((short)(cx + LocalDx[d]), (short)cy);
                    NavCell sideB = new NavCell((short)cx, (short)(cy + LocalDy[d]));
                    if (!IsPassable(sideA, footprint) || !IsPassable(sideB, footprint)) continue;
                }
                int step = d < 4 ? 10 : 14; if (layer == MovementLayer.Ground && (_map.GetFlags(nx, ny) & MapCellFlags.Rough) != 0) step += 4;
                int ng = currentG + step, oldG = _navSeen[ni] == stamp ? _navG[ni] : int.MaxValue;
                if (ng < oldG) { SetNavNode(ni, ng, current, stamp); _navOpen.Add(ni); }
            }
        }
    }

    private void SimplifyPath(RouteCorridor path, FootprintClass footprint, MovementLayer layer)
    {
        if (path.Cells.Count <= 2) return;
        const int MaxLookAheadCells = 48; // 24 build cells at the canonical 0.5-cell nav resolution.
        _smoothedCells.Clear();
        _smoothedCells.Add(path.Cells[0]);
        int anchor = 0;
        while (anchor < path.Cells.Count - 1)
        {
            int chosen = anchor + 1;
            int maxCandidate = Math.Min(path.Cells.Count - 1, anchor + MaxLookAheadCells);
            int originalCost = 0;
            for (int candidate = anchor + 1; candidate <= maxCandidate; candidate++)
            {
                originalCost += StepCost(path.Cells[candidate - 1], path.Cells[candidate], layer);
                if (candidate <= anchor + 1) continue;
                if (TryStraightCost(path.Cells[anchor], path.Cells[candidate], footprint, layer, out int shortcutCost) && shortcutCost <= originalCost)
                    chosen = candidate;
            }
            _smoothedCells.Add(path.Cells[chosen]);
            anchor = chosen;
        }
        path.Cells.Clear();
        path.Cells.AddRange(_smoothedCells);
    }

    private bool TryStraightCost(NavCell start, NavCell goal, FootprintClass footprint, MovementLayer layer, out int cost)
    {
        cost = 0;
        int x = start.X, y = start.Y;
        int dx = Math.Abs(goal.X - x), sx = x < goal.X ? 1 : -1;
        int dy = -Math.Abs(goal.Y - y), sy = y < goal.Y ? 1 : -1;
        int err = dx + dy;
        while (x != goal.X || y != goal.Y)
        {
            int oldX = x, oldY = y;
            int e2 = err << 1;
            bool movedX = false, movedY = false;
            if (e2 >= dy) { err += dy; x += sx; movedX = true; }
            if (e2 <= dx) { err += dx; y += sy; movedY = true; }
            if (!_map.InBounds(x, y)) return false;
            NavCell next = new NavCell(checked((short)x), checked((short)y));
            if (!IsPassable(next, footprint)) return false;
            if (movedX && movedY)
            {
                NavCell sideA = new NavCell(checked((short)x), checked((short)oldY));
                NavCell sideB = new NavCell(checked((short)oldX), checked((short)y));
                if (!IsPassable(sideA, footprint) || !IsPassable(sideB, footprint)) return false;
            }
            cost += StepCost(new NavCell(checked((short)oldX), checked((short)oldY)), next, layer);
        }
        return true;
    }

    private int StepCost(NavCell from, NavCell to, MovementLayer layer)
    {
        bool diagonal = from.X != to.X && from.Y != to.Y;
        int step = diagonal ? 14 : 10;
        if (layer == MovementLayer.Ground && (_map.GetFlags(to.X, to.Y) & MapCellFlags.Rough) != 0) step += 4;
        return step;
    }

    private void SetNavNode(int index, int g, int parent, int stamp)
    { _navSeen[index] = stamp; _navG[index] = g; _navParent[index] = parent; }

    private bool NavNodeLess(int a, int b, NavCell goal, int stamp)
    {
        int ax = a % MapGrid.NavWidth, ay = a / MapGrid.NavWidth, bx = b % MapGrid.NavWidth, by = b / MapGrid.NavWidth;
        int adx = Math.Abs(goal.X - ax), ady = Math.Abs(goal.Y - ay), bdx = Math.Abs(goal.X - bx), bdy = Math.Abs(goal.Y - by);
        int ah = 10 * (adx + ady) - 6 * Math.Min(adx, ady), bh = 10 * (bdx + bdy) - 6 * Math.Min(bdx, bdy);
        int af = (_navSeen[a] == stamp ? _navG[a] : int.MaxValue) + ah, bf = (_navSeen[b] == stamp ? _navG[b] : int.MaxValue) + bh;
        if (af != bf) return af < bf; if (ah != bh) return ah < bh; if (ay != by) return ay < by; return ax < bx;
    }

    private void ReconstructCells(int current, RouteCorridor output)
    {
        _reverseCells.Clear();
        while (current >= 0) { _reverseCells.Add(new NavCell((short)(current % MapGrid.NavWidth), (short)(current / MapGrid.NavWidth))); current = _navParent[current]; }
        for (int i = _reverseCells.Count - 1; i >= 0; i--) output.Cells.Add(_reverseCells[i]);
    }

    private static int NextStamp(ref int stamp, int[] seen, int[] closed)
    {
        stamp++;
        if (stamp == int.MaxValue) { Array.Clear(seen, 0, seen.Length); Array.Clear(closed, 0, closed.Length); stamp = 1; }
        return stamp;
    }

    public bool IsPassable(NavCell cell, FootprintClass footprint)
    {
        int radius = FootprintRules.ClearanceNavCells(footprint), rr = radius * radius;
        for (int y = cell.Y - radius; y <= cell.Y + radius; y++)
            for (int x = cell.X - radius; x <= cell.X + radius; x++)
            {
                int dx = x - cell.X, dy = y - cell.Y; if (dx * dx + dy * dy > rr) continue;
                if (!_map.InBounds(x, y) || (_map.GetFlags(x, y) & MapCellFlags.Impassable) != 0) return false;
            }
        return true;
    }

    private static ClusterCoord ToCluster(NavCell cell) => new ClusterCoord((short)(cell.X / ClusterSize), (short)(cell.Y / ClusterSize));
    private static int Index(NavCell cell) => cell.Y * MapGrid.NavWidth + cell.X;
}
}
