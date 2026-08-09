using System;

namespace LegoSpaceRTS.SimCore
{
public readonly struct FixVec2 : IEquatable<FixVec2>
{
    public readonly Fix32 X;
    public readonly Fix32 Y;
    public static readonly FixVec2 Zero = new FixVec2(Fix32.Zero, Fix32.Zero);

    public FixVec2(Fix32 x, Fix32 y) { X = x; Y = y; }
    public static FixVec2 FromInts(int x, int y) => new FixVec2(Fix32.FromInt(x), Fix32.FromInt(y));

    public static FixVec2 operator +(FixVec2 a, FixVec2 b) => new FixVec2(a.X + b.X, a.Y + b.Y);
    public static FixVec2 operator -(FixVec2 a, FixVec2 b) => new FixVec2(a.X - b.X, a.Y - b.Y);
    public static FixVec2 operator *(FixVec2 v, Fix32 scalar) => new FixVec2(v.X * scalar, v.Y * scalar);
    public static FixVec2 operator /(FixVec2 v, Fix32 scalar) => new FixVec2(v.X / scalar, v.Y / scalar);

    /// <summary>Q16.16 squared length, saturated when the mathematically exact square exceeds Fix32 range.</summary>
    public Fix32 LengthSquared()
    {
        long sx=(long)X.Raw*X.Raw;
        long sy=(long)Y.Raw*Y.Raw;
        long raw=(sx+sy)>>Fix32.FractionalBits;
        return raw>int.MaxValue?Fix32.MaxValue:Fix32.FromRaw((int)raw);
    }

    /// <summary>Distance-safe vector length using 64-bit raw intermediates.</summary>
    public Fix32 Length()
    {
        ulong sx=(ulong)((long)X.Raw*X.Raw);
        ulong sy=(ulong)((long)Y.Raw*Y.Raw);
        ulong root=IntegerSqrt(sx+sy);
        if(root>int.MaxValue) throw new OverflowException("FixVec2 length overflow.");
        return Fix32.FromRaw((int)root);
    }

    public FixVec2 NormalizeSafe()
    {
        Fix32 len=Length();
        return len.Raw==0?Zero:this/len;
    }

    public static Fix32 Dot(FixVec2 a,FixVec2 b)
    {
        long raw=((long)a.X.Raw*b.X.Raw+(long)a.Y.Raw*b.Y.Raw)>>Fix32.FractionalBits;
        if(raw>int.MaxValue||raw<int.MinValue) throw new OverflowException("FixVec2 dot overflow.");
        return Fix32.FromRaw((int)raw);
    }
    public static Fix32 DistanceSquared(FixVec2 a,FixVec2 b)=>(a-b).LengthSquared();
    public static Fix32 Distance(FixVec2 a,FixVec2 b)=>(a-b).Length();

    private static ulong IntegerSqrt(ulong n)
    {
        if(n==0)return 0;
        ulong x=n; ulong y=(x+1UL)>>1;
        while(y<x){x=y;y=(x+n/x)>>1;}
        return x;
    }

    public bool Equals(FixVec2 other)=>X==other.X&&Y==other.Y;
    public override bool Equals(object? obj)=>obj is FixVec2 other&&Equals(other);
    public override int GetHashCode()=>unchecked((X.Raw*397)^Y.Raw);
    public override string ToString()=>$"({X},{Y})";
}
}
