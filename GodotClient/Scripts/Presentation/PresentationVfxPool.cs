using Godot;

namespace LegoSpaceRTS.Presentation;

public enum PresentationEventFamily : byte
{
    WeaponFire = 0,
    Impact = 1,
    Destruction = 2,
    Repair = 3,
    Construction = 4,
    Environment = 5
}

public readonly struct PresentationEventId
{
    public readonly int ObservedTick;
    public readonly uint Ordinal;
    public readonly uint SourceEntity;
    public readonly PresentationEventFamily Family;

    public PresentationEventId(int observedTick, uint ordinal, uint sourceEntity, PresentationEventFamily family)
    {
        ObservedTick = observedTick;
        Ordinal = ordinal;
        SourceEntity = sourceEntity;
        Family = family;
    }
}

public sealed class PresentationEventDeduplicator
{
    private readonly Dictionary<(uint Source, PresentationEventFamily Family), uint> _highWatermarks = new();

    public int TrackedStreamCount => _highWatermarks.Count;

    public bool TryAccept(int observedTick, uint sourceEntity, uint ordinal, PresentationEventFamily family,
        out PresentationEventId eventId)
    {
        eventId = new PresentationEventId(observedTick, ordinal, sourceEntity, family);
        (uint Source, PresentationEventFamily Family) key = (sourceEntity, family);
        if (!_highWatermarks.TryGetValue(key, out uint seen))
        {
            _highWatermarks.Add(key, ordinal);
            return false;
        }
        if (ordinal <= seen) return false;
        _highWatermarks[key] = ordinal;
        return true;
    }

    public void ClearForNewSession() => _highWatermarks.Clear();
}

public interface IPooledPresentationVfx
{
    void OnPoolActivated();
    void OnPoolUpdated(float normalizedLifetime, float deltaSeconds);
    void OnPoolReleased();
}

public readonly struct PresentationVfxPoolStats
{
    public readonly int Capacity;
    public readonly int Budget;
    public readonly int Created;
    public readonly int Active;
    public readonly int PeakActive;
    public readonly long Spawned;
    public readonly long Reused;
    public readonly long Dropped;

    public PresentationVfxPoolStats(int capacity, int budget, int created, int active, int peakActive,
        long spawned, long reused, long dropped)
    {
        Capacity = capacity;
        Budget = budget;
        Created = created;
        Active = active;
        PeakActive = peakActive;
        Spawned = spawned;
        Reused = reused;
        Dropped = dropped;
    }
}

public sealed class PresentationVfxPool<T> where T : Node3D
{
    private readonly Entry[] _entries;
    private readonly Dictionary<T, int> _indices;
    private readonly Stack<int> _available;
    private int _budget;
    private int _activeCount;
    private int _peakActive;
    private long _spawned;
    private long _reused;
    private long _dropped;

    public int Capacity => _entries.Length;
    public int Budget
    {
        get => _budget;
        set => _budget = Math.Clamp(value, 0, Capacity);
    }

    public PresentationVfxPool(Node owner, string poolName, int capacity, Func<int, T> factory)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(factory);
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _entries = new Entry[capacity];
        _indices = new Dictionary<T, int>(capacity);
        _available = new Stack<int>(capacity);
        _budget = capacity;
        for (int i = 0; i < capacity; i++)
        {
            T node = factory(i) ?? throw new InvalidOperationException($"{poolName} factory returned null.");
            node.Name = $"{poolName}_{i:00}";
            node.Visible = false;
            owner.AddChild(node);
            _entries[i] = new Entry(node);
            _indices.Add(node, i);
        }
        for (int i = capacity - 1; i >= 0; i--) _available.Push(i);
    }

    public bool TryAcquire(float lifetimeSeconds, out T node)
    {
        if (_activeCount >= _budget || _available.Count == 0)
        {
            _dropped++;
            node = null!;
            return false;
        }
        int index = _available.Pop();
        Entry entry = _entries[index];
        if (entry.EverUsed) _reused++;
        entry.Active = true;
        entry.Activated = false;
        entry.EverUsed = true;
        entry.Lifetime = Math.Max(0f, lifetimeSeconds);
        entry.Remaining = entry.Lifetime;
        _activeCount++;
        _peakActive = Math.Max(_peakActive, _activeCount);
        _spawned++;
        node = entry.Node;
        return true;
    }

    public void Activate(T node)
    {
        if (!_indices.TryGetValue(node, out int index) || !_entries[index].Active)
            throw new InvalidOperationException("Only an acquired VFX node can be activated.");
        Entry entry = _entries[index];
        if (entry.Activated) return;
        entry.Activated = true;
        node.Visible = true;
        if (node is IPooledPresentationVfx lifecycle) lifecycle.OnPoolActivated();
    }

    public void Update(float deltaSeconds)
    {
        float delta = Math.Clamp(deltaSeconds, 0f, 0.25f);
        for (int i = 0; i < _entries.Length; i++)
        {
            Entry entry = _entries[i];
            if (!entry.Active || !entry.Activated) continue;
            if (entry.Lifetime > 0f) entry.Remaining -= delta;
            float normalized = entry.Lifetime <= 0f ? 0f : Math.Clamp(1f - entry.Remaining / entry.Lifetime, 0f, 1f);
            if (entry.Node is IPooledPresentationVfx lifecycle)
                lifecycle.OnPoolUpdated(normalized, delta);
            if (entry.Lifetime > 0f && entry.Remaining <= 0f) ReleaseAt(i);
        }
    }

    public bool Release(T node)
    {
        if (!_indices.TryGetValue(node, out int index) || !_entries[index].Active) return false;
        ReleaseAt(index);
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < _entries.Length; i++)
            if (_entries[i].Active) ReleaseAt(i);
    }

    public void ForEachNode(Action<T> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        for (int i = 0; i < _entries.Length; i++) visitor(_entries[i].Node);
    }

    public PresentationVfxPoolStats GetStats() => new(Capacity, Budget, Capacity, _activeCount,
        _peakActive, _spawned, _reused, _dropped);

    private void ReleaseAt(int index)
    {
        Entry entry = _entries[index];
        if (entry.Node is IPooledPresentationVfx lifecycle) lifecycle.OnPoolReleased();
        entry.Node.Visible = false;
        entry.Active = false;
        entry.Activated = false;
        entry.Lifetime = 0f;
        entry.Remaining = 0f;
        _activeCount--;
        _available.Push(index);
    }

    private sealed class Entry
    {
        public readonly T Node;
        public bool Active;
        public bool Activated;
        public bool EverUsed;
        public float Lifetime;
        public float Remaining;

        public Entry(T node) => Node = node;
    }
}

public partial class PooledTracerEffect : Node3D, IPooledPresentationVfx
{
    private readonly MeshInstance3D _mesh;
    private Vector3 _start;
    private Vector3 _end;

    public PooledTracerEffect()
    {
        _mesh = new MeshInstance3D
        {
            Name = "TracerMesh",
            Mesh = new BoxMesh(),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_mesh);
    }

    public void Configure(Vector3 start, Vector3 end, float width, float length, Material material)
    {
        _start = start;
        _end = end;
        if (_mesh.Mesh is BoxMesh box) box.Size = new Vector3(width, width, length);
        _mesh.MaterialOverride = material;
        GlobalPosition = start;
        if (!start.IsEqualApprox(end)) LookAt(end, Vector3.Up);
    }

    public void SetMaterial(Material material) => _mesh.MaterialOverride = material;

    public void OnPoolActivated() => Visible = true;

    public void OnPoolUpdated(float normalizedLifetime, float deltaSeconds)
    {
        GlobalPosition = _start.Lerp(_end, normalizedLifetime);
    }

    public void OnPoolReleased() => Visible = false;
}

public partial class PooledParticleBurst : Node3D, IPooledPresentationVfx
{
    public GpuParticles3D Particles { get; }

    public PooledParticleBurst(int amount, float lifetime, float spread, float minVelocity,
        float maxVelocity, Vector3 gravity)
    {
        ParticleProcessMaterial process = new()
        {
            Direction = Vector3.Forward,
            Spread = spread,
            Gravity = gravity,
            InitialVelocityMin = minVelocity,
            InitialVelocityMax = maxVelocity,
            ScaleMin = 0.45f,
            ScaleMax = 1.0f
        };
        Particles = new GpuParticles3D
        {
            Name = "ParticleEmitter",
            Amount = Math.Max(1, amount),
            Lifetime = lifetime,
            OneShot = true,
            Explosiveness = 0.92f,
            Randomness = 0.42f,
            ProcessMaterial = process,
            LocalCoords = true,
            Emitting = false,
            DrawPass1 = new QuadMesh { Size = Vector2.One }
        };
        AddChild(Particles);
    }

    public void Configure(Vector3 position, Vector3 target, Material material, Vector2 size, int amount)
    {
        GlobalPosition = position;
        if (!position.IsEqualApprox(target)) LookAt(target, Vector3.Up);
        Particles.Amount = Math.Max(1, amount);
        Particles.AmountRatio = amount <= 0 ? 0f : 1f;
        if (Particles.DrawPass1 is QuadMesh quad)
        {
            quad.Size = size;
            quad.Material = material;
        }
    }

    public void SetMaterial(Material material)
    {
        if (Particles.DrawPass1 is QuadMesh quad) quad.Material = material;
    }

    public void OnPoolActivated()
    {
        Visible = true;
        Particles.Restart();
        Particles.Emitting = Particles.AmountRatio > 0f;
    }

    public void OnPoolUpdated(float normalizedLifetime, float deltaSeconds) { }

    public void OnPoolReleased()
    {
        Particles.Emitting = false;
        Visible = false;
    }
}

public partial class PooledMeshEffect : Node3D, IPooledPresentationVfx
{
    public MeshInstance3D MeshInstance { get; }

    public PooledMeshEffect(Mesh mesh, Material material)
    {
        MeshInstance = new MeshInstance3D
        {
            Name = "EffectMesh",
            Mesh = mesh,
            MaterialOverride = material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(MeshInstance);
    }

    public void OnPoolActivated() => Visible = true;
    public void OnPoolUpdated(float normalizedLifetime, float deltaSeconds) { }
    public void OnPoolReleased() => Visible = false;
}
