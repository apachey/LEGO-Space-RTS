using System;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Signed Q16.16 fixed-point scalar. All arithmetic is checked and fails fast on overflow.</summary>
public readonly struct Fix32 : IComparable<Fix32>, IEquatable<Fix32>
{
    public const int FractionalBits = 16;
    public const int OneRaw = 1 << FractionalBits;
    public readonly int Raw;

    public static readonly Fix32 Zero = new(0, true);
    public static readonly Fix32 One = new(OneRaw, true);
    public static readonly Fix32 Half = new(OneRaw >> 1, true);
    public static readonly Fix32 Epsilon = new(1, true);
    public static readonly Fix32 MinValue = new(int.MinValue, true);
    public static readonly Fix32 MaxValue = new(int.MaxValue, true);

    private Fix32(int raw, bool _) => Raw = raw;
    public static Fix32 FromRaw(int raw) => new(raw, true);
    public static Fix32 FromInt(int value) => new(checked((int)((long)value << FractionalBits)), true);
    public static Fix32 FromRatio(int numerator, int denominator)
    {
        if (denominator == 0) throw new DivideByZeroException();
        long raw = ((long)numerator << FractionalBits) / denominator;
        return new(checked((int)raw), true);
    }

    public int FloorToInt() => Raw >> FractionalBits;
    public int CeilToInt()
    {
        int floor = Raw >> FractionalBits;
        return (Raw & (OneRaw - 1)) == 0 ? floor : checked(floor + 1);
    }
    public int RoundToInt()
    {
        long raw = Raw;
        if (raw >= 0) return checked((int)((raw + (OneRaw >> 1)) >> FractionalBits));
        return checked((int)(-(((-raw) + (OneRaw >> 1)) >> FractionalBits)));
    }

    public static Fix32 operator +(Fix32 a, Fix32 b) => new(checked(a.Raw + b.Raw), true);
    public static Fix32 operator -(Fix32 a, Fix32 b) => new(checked(a.Raw - b.Raw), true);
    public static Fix32 operator -(Fix32 value) => new(checked(-value.Raw), true);
    public static Fix32 operator *(Fix32 a, Fix32 b)
    {
        long product = (long)a.Raw * b.Raw;
        return new(checked((int)(product >> FractionalBits)), true);
    }
    public static Fix32 operator /(Fix32 a, Fix32 b)
    {
        if (b.Raw == 0) throw new DivideByZeroException();
        long quotient = ((long)a.Raw << FractionalBits) / b.Raw;
        return new(checked((int)quotient), true);
    }

    public static bool operator ==(Fix32 a, Fix32 b) => a.Raw == b.Raw;
    public static bool operator !=(Fix32 a, Fix32 b) => a.Raw != b.Raw;
    public static bool operator <(Fix32 a, Fix32 b) => a.Raw < b.Raw;
    public static bool operator >(Fix32 a, Fix32 b) => a.Raw > b.Raw;
    public static bool operator <=(Fix32 a, Fix32 b) => a.Raw <= b.Raw;
    public static bool operator >=(Fix32 a, Fix32 b) => a.Raw >= b.Raw;

    public static Fix32 Abs(Fix32 value) => value.Raw >= 0 ? value : -value;
    public static Fix32 Min(Fix32 a, Fix32 b) => a.Raw <= b.Raw ? a : b;
    public static Fix32 Max(Fix32 a, Fix32 b) => a.Raw >= b.Raw ? a : b;
    public static Fix32 Clamp(Fix32 value, Fix32 min, Fix32 max) => Max(min, Min(value, max));

    public static Fix32 Sqrt(Fix32 value)
    {
        if (value.Raw < 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (value.Raw == 0) return Zero;
        ulong n = (ulong)(uint)value.Raw << FractionalBits;
        ulong x = n;
        ulong y = (x + 1UL) >> 1;
        while (y < x)
        {
            x = y;
            y = (x + n / x) >> 1;
        }
        if (x > int.MaxValue) throw new OverflowException("Fix32 sqrt overflow.");
        return FromRaw((int)x);
    }

    public int CompareTo(Fix32 other) => Raw.CompareTo(other.Raw);
    public bool Equals(Fix32 other) => Raw == other.Raw;
    public override bool Equals(object? obj) => obj is Fix32 other && Equals(other);
    public override int GetHashCode() => Raw;
    public override string ToString() => $"{FloorToInt()}+{Math.Abs(Raw & 0xFFFF)}/65536";
}
}
