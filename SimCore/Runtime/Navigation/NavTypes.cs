using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class NavPath
{
    public readonly List<NavCell> Cells = new List<NavCell>();
    public int TopologyVersion;
    public bool IsValid => Cells.Count > 0;
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
