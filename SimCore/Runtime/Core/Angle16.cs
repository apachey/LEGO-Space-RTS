using System;

namespace LegoSpaceRTS.SimCore
{
/// <summary>UInt16 turn representation: 0=0°, 16384=90°, 32768=180°.</summary>
public readonly struct Angle16 : IEquatable<Angle16>
{
    public readonly ushort Raw;
    public Angle16(ushort raw) => Raw = raw;
    public static readonly Angle16 Zero = new(0);
    public static readonly Angle16 Quarter = new(16384);
    public static readonly Angle16 Half = new(32768);

    public static int ShortestDelta(Angle16 from, Angle16 to)
    {
        int delta = to.Raw - from.Raw;
        if (delta > 32767) delta -= 65536;
        if (delta < -32768) delta += 65536;
        return delta;
    }

    public static bool IsClockwiseShortest(Angle16 from, Angle16 to) => ShortestDelta(from, to) > 0;

    public static Angle16 TurnToward(Angle16 current, Angle16 target, ushort maxStep)
    {
        int delta = ShortestDelta(current, target);
        if (delta == 0) return current;
        int step = delta > 0 ? Math.Min(delta, maxStep) : Math.Max(delta, -maxStep);
        return new Angle16(unchecked((ushort)(current.Raw + step)));
    }

    /// <summary>Deterministic octant-linear atan2 approximation; avoids transcendental arithmetic.</summary>
    public static Angle16 FromDirection(FixVec2 direction)
    {
        int x = direction.X.Raw;
        int y = direction.Y.Raw;
        if ((x | y) == 0) return Zero;
        int ax = x == int.MinValue ? int.MaxValue : Math.Abs(x);
        int ay = y == int.MinValue ? int.MaxValue : Math.Abs(y);
        int octantOffset;
        int local;
        if (ax >= ay)
        {
            local = ax == 0 ? 0 : (int)(((long)ay * 8192) / ax);
            if (x >= 0 && y >= 0) octantOffset = 0;
            else if (x < 0 && y >= 0) { octantOffset = 32768; local = -local; }
            else if (x < 0) { octantOffset = 32768; }
            else { octantOffset = 65536; local = -local; }
        }
        else
        {
            local = ay == 0 ? 0 : (int)(((long)ax * 8192) / ay);
            if (x >= 0 && y >= 0) { octantOffset = 16384; local = -local; }
            else if (x < 0 && y >= 0) { octantOffset = 16384; }
            else if (x < 0) { octantOffset = 49152; local = -local; }
            else { octantOffset = 49152; }
        }
        return new Angle16(unchecked((ushort)(octantOffset + local)));
    }

    public bool Equals(Angle16 other) => Raw == other.Raw;
    public override bool Equals(object? obj) => obj is Angle16 other && Equals(other);
    public override int GetHashCode() => Raw;
    public override string ToString() => Raw.ToString();
}
}
