using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>
/// Authoritative persistent route corridor. The ordered cells are deterministic steering
/// segments produced by global routing; the bounded window permits local locomotion to
/// deviate from the exact waypoint line without turning the corridor into an occupancy
/// reservation schedule.
/// </summary>
public class RouteCorridor
{
    public const int LocalDeviationNavCells = 4;
    public readonly List<NavCell> Cells = new List<NavCell>();
    public int TopologyVersion;
    public bool IsValid => Cells.Count > 0;

    public bool Contains(NavCell cell, int pathIndex)
    {
        if (Cells.Count == 0) return false;
        if (Cells.Count == 1) return IsInsideSegmentWindow(cell, Cells[0], Cells[0]);
        int firstSegment = Math.Max(0, Math.Min(pathIndex - 1, Cells.Count - 2));
        int lastSegment = Math.Min(Cells.Count - 2, firstSegment + 2);
        for (int i = firstSegment; i <= lastSegment; i++)
            if (IsInsideSegmentWindow(cell, Cells[i], Cells[i + 1])) return true;
        return false;
    }

    public bool IsAffectedBy(IntRect navRect)
    {
        if (Cells.Count == 0) return true;
        if (Cells.Count == 1) return SegmentWindowIntersects(Cells[0], Cells[0], navRect);
        for (int i = 0; i < Cells.Count - 1; i++)
            if (SegmentWindowIntersects(Cells[i], Cells[i + 1], navRect)) return true;
        return false;
    }

    private static bool IsInsideSegmentWindow(NavCell cell, NavCell start, NavCell end)
    {
        long dx = end.X - start.X, dy = end.Y - start.Y;
        long px = cell.X - start.X, py = cell.Y - start.Y;
        long lengthSquared = dx * dx + dy * dy;
        long windowSquared = LocalDeviationNavCells * LocalDeviationNavCells;
        if (lengthSquared == 0) return px * px + py * py <= windowSquared;
        long projection = px * dx + py * dy;
        if (projection <= 0) return px * px + py * py <= windowSquared;
        if (projection >= lengthSquared)
        {
            long ex = cell.X - end.X, ey = cell.Y - end.Y;
            return ex * ex + ey * ey <= windowSquared;
        }
        long cross = px * dy - py * dx;
        return cross * cross <= windowSquared * lengthSquared;
    }

    private static bool SegmentWindowIntersects(NavCell start, NavCell end, IntRect navRect)
    {
        int minX = Math.Min(start.X, end.X) - LocalDeviationNavCells;
        int maxX = Math.Max(start.X, end.X) + LocalDeviationNavCells;
        int minY = Math.Min(start.Y, end.Y) - LocalDeviationNavCells;
        int maxY = Math.Max(start.Y, end.Y) + LocalDeviationNavCells;
        int rectMaxX = navRect.X + navRect.Width - 1;
        int rectMaxY = navRect.Y + navRect.Height - 1;
        return minX <= rectMaxX && maxX >= navRect.X && minY <= rectMaxY && maxY >= navRect.Y;
    }
}

/// <summary>Compatibility result type for direct pathfinder callers.</summary>
public sealed class NavPath : RouteCorridor
{
}

public readonly struct ClusterCoord : IComparable<ClusterCoord>, IEquatable<ClusterCoord>
{
    public readonly short X; public readonly short Y;
    public ClusterCoord(short x,short y){X=x;Y=y;}
    public int CompareTo(ClusterCoord other){int c=Y.CompareTo(other.Y);return c!=0?c:X.CompareTo(other.X);}
    public bool Equals(ClusterCoord other)=>X==other.X&&Y==other.Y;
    public override bool Equals(object? obj)=>obj is ClusterCoord other&&Equals(other);
    public override int GetHashCode()=>(X*397)^Y;
    public static bool operator ==(ClusterCoord a,ClusterCoord b)=>a.Equals(b);
    public static bool operator !=(ClusterCoord a,ClusterCoord b)=>!a.Equals(b);
}

public readonly struct Portal
{
    public readonly NavCell Cell; public readonly ClusterCoord From; public readonly ClusterCoord To;
    public Portal(NavCell cell,ClusterCoord from,ClusterCoord to){Cell=cell;From=from;To=to;}
}

public static class FootprintRules
{
    public static int ClearanceNavCells(FootprintClass footprint) => footprint switch
    {
        FootprintClass.Tiny => 1,
        FootprintClass.Small => 2,
        FootprintClass.Medium => 2,
        FootprintClass.Large => 3,
        FootprintClass.Huge => 4,
        _ => 1
    };
    public static int ReservationPriority(FootprintClass footprint) => (int)footprint;

    public static Fix32 CollisionRadiusBuild(FootprintClass footprint) => footprint switch
    {
        FootprintClass.Tiny => Fix32.FromRatio(35, 100),
        FootprintClass.Small => Fix32.FromRatio(55, 100),
        FootprintClass.Medium => Fix32.FromRatio(85, 100),
        FootprintClass.Large => Fix32.FromRatio(115, 100),
        FootprintClass.Huge => Fix32.FromRatio(160, 100),
        _ => Fix32.Half
    };
}
}
