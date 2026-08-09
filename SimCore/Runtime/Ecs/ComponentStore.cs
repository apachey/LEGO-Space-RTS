using System;

namespace LegoSpaceRTS.SimCore
{
public sealed class ComponentStore<T> where T : struct
{
    private T[] _data = new T[128];
    private bool[] _present = new bool[128];

    public bool Has(EntityId id) => id.Value < _present.Length && _present[id.Value];

    public ref T Get(EntityId id)
    {
        if (!Has(id)) throw new InvalidOperationException($"Entity {id} has no {typeof(T).Name} component.");
        return ref _data[id.Value];
    }

    public void Set(EntityId id, T value)
    {
        Ensure(id.Value);
        _data[id.Value] = value;
        _present[id.Value] = true;
    }

    public bool TryGet(EntityId id, out T value)
    {
        if (Has(id)) { value = _data[id.Value]; return true; }
        value = default;
        return false;
    }

    public void Remove(EntityId id)
    {
        if (id.Value >= _present.Length) return;
        _present[id.Value] = false;
        _data[id.Value] = default;
    }

    private void Ensure(uint index)
    {
        if (index < _data.Length) return;
        int size = _data.Length;
        while (index >= size) size = checked(size * 2);
        Array.Resize(ref _data, size);
        Array.Resize(ref _present, size);
    }
}
}
