using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct EntityId : IComparable<EntityId>, IEquatable<EntityId>
{
    public readonly uint Value;
    public EntityId(uint value) { Value = value; }
    public static readonly EntityId None = new EntityId(0);
    public int CompareTo(EntityId other) => Value.CompareTo(other.Value);
    public bool Equals(EntityId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);
    public override int GetHashCode() => (int)Value;
    public static bool operator ==(EntityId a, EntityId b) => a.Value == b.Value;
    public static bool operator !=(EntityId a, EntityId b) => a.Value != b.Value;
    public override string ToString() => Value.ToString();
}

public readonly struct ContentId : IComparable<ContentId>, IEquatable<ContentId>
{
    public readonly uint Value;
    public ContentId(uint value) { Value = value; }
    public int CompareTo(ContentId other) => Value.CompareTo(other.Value);
    public bool Equals(ContentId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ContentId other && Equals(other);
    public override int GetHashCode() => (int)Value;
    public static bool operator ==(ContentId a, ContentId b) => a.Value == b.Value;
    public static bool operator !=(ContentId a, ContentId b) => a.Value != b.Value;
    public override string ToString() => Value.ToString("X8");
}

public readonly struct UnitId { public readonly uint Value; public UnitId(uint value) { Value=value; } }
public readonly struct BuildingId { public readonly uint Value; public BuildingId(uint value) { Value=value; } }
public readonly struct MovementProfileId { public readonly uint Value; public MovementProfileId(uint value) { Value=value; } }
public readonly struct MapId { public readonly uint Value; public MapId(uint value) { Value=value; } }
public readonly struct CommandTypeId { public readonly ushort Value; public CommandTypeId(ushort value) { Value=value; } }

public static class StableId
{
    public static ContentId FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Stable key cannot be empty.", nameof(key));
        uint hash = 2166136261u;
        for (int i=0;i<key.Length;i++)
        {
            char c=key[i];
            if (c > 127) throw new ArgumentException("Stable keys are ASCII only.", nameof(key));
            if (c >= 'A' && c <= 'Z') c = (char)(c + ('a' - 'A'));
            hash = unchecked((hash ^ (byte)c) * 16777619u);
        }
        if (hash == 0) hash = 1;
        return new ContentId(hash);
    }
}

public sealed class StableIdRegistry
{
    private readonly Dictionary<uint, string> _keys = new Dictionary<uint, string>();
    public ContentId Register(string key)
    {
        ContentId id=StableId.FromKey(key);
        if (_keys.TryGetValue(id.Value,out string existing))
        {
            if (!string.Equals(existing,key,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException($"Stable ID collision: '{key}' and '{existing}' -> {id}.");
            throw new InvalidOperationException($"Duplicate stable key '{key}'.");
        }
        _keys.Add(id.Value,key); return id;
    }
}
}
