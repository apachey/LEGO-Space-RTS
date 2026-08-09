using System;

namespace LegoSpaceRTS.SimCore
{
public readonly struct SimTick : IComparable<SimTick>, IEquatable<SimTick>
{
    public readonly int Value;
    public SimTick(int value) => Value = value;
    public SimTick Next() => new(checked(Value + 1));
    public int CompareTo(SimTick other) => Value.CompareTo(other.Value);
    public bool Equals(SimTick other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is SimTick other && Equals(other);
    public override int GetHashCode() => Value;
    public override string ToString() => Value.ToString();
}

public readonly struct TickDuration
{
    public readonly int Ticks;
    public TickDuration(int ticks) => Ticks = ticks;
}

public static class SimClock
{
    public const int TicksPerSecond = 20;
    public const int MillisecondsPerTick = 50;
    public static readonly Fix32 TickSeconds = Fix32.FromRatio(1, TicksPerSecond);
    public static TickDuration FromSeconds(int seconds) => new(checked(seconds * TicksPerSecond));
    public static TickDuration FromMilliseconds(int milliseconds) => new((milliseconds + MillisecondsPerTick - 1) / MillisecondsPerTick);
}
}
