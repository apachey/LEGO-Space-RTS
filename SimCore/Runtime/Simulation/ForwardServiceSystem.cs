using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic Astronaut Forward Service membership backed by cached provider buckets.</summary>
public sealed class ForwardServiceSystem : ISimSystem
{
    public const byte ServiceHubRadius = 18;
    public const byte DeployedSolarExplorerRadius = 10;
    public const int ProviderBucketBuildCells = 4;

    private static readonly ContentId ServiceHubType = StableId.FromKey("building.ast.service_refit_hub");
    private static readonly ContentId SolarExplorerType = StableId.FromKey("unit.ast.solar_explorer");
    private static readonly ContentId T3TrikeType = StableId.FromKey("unit.ast.t3_trike");
    private const int BucketWidth = MapGrid.BuildWidth / ProviderBucketBuildCells;
    private const int BucketHeight = MapGrid.BuildHeight / ProviderBucketBuildCells;

    private readonly Dictionary<int, List<EntityId>> _providerBuckets = new();
    private readonly List<ProviderInfo> _providers = new();
    private readonly List<ProviderInfo> _previousProviders = new();
    private bool _initialized;

    private readonly struct ProviderInfo : IEquatable<ProviderInfo>
    {
        public readonly EntityId Entity;
        public readonly byte Owner;
        public readonly FixVec2 Position;
        public readonly byte Radius;

        public ProviderInfo(EntityId entity, byte owner, FixVec2 position, byte radius)
        { Entity = entity; Owner = owner; Position = position; Radius = radius; }

        public bool Equals(ProviderInfo other)
            => Entity == other.Entity && Owner == other.Owner && Position.Equals(other.Position) && Radius == other.Radius;
    }

    public void Step(SimulationWorld world)
    {
        SynchronizeCanonicalComponents(world);
        CollectActiveProviders(world);
        bool providersChanged = !_initialized || !ProviderSetsEqual();
        if (providersChanged) RebuildProviderBuckets();
        UpdateMembers(world, providersChanged);
        CacheProviders();
        _initialized = true;
    }

    public static bool TryGetProviderForMember(SimulationWorld world, EntityId member, out EntityId provider)
    {
        if (world.Entities.ForwardServiceMember.TryGet(member, out ForwardServiceMember service) &&
            service.Provider != EntityId.None && world.Entities.ForwardServiceProvider.TryGet(service.Provider, out ForwardServiceProvider source) && source.IsActive &&
            world.Entities.Ownership.TryGet(member, out Ownership memberOwner) && world.Entities.Ownership.TryGet(service.Provider, out Ownership providerOwner) &&
            memberOwner.PlayerSlot == providerOwner.PlayerSlot && world.Entities.Transform.TryGet(member, out SimTransform memberTransform) &&
            world.Entities.Transform.TryGet(service.Provider, out SimTransform providerTransform) && Contains(providerTransform.Position, source.RadiusBuildCells, memberTransform.Position) &&
            IsCanonicalProviderActive(world, service.Provider))
        {
            provider = service.Provider;
            return true;
        }
        provider = EntityId.None;
        return false;
    }

    private static void SynchronizeCanonicalComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable)) continue;
            if (selectable.ContentType == ServiceHubType)
            {
                bool active = world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed && BrownoutSystem.IsOperational(world, id);
                SetProvider(world, id, ServiceHubRadius, active);
            }
            else if (selectable.ContentType == SolarExplorerType)
            {
                bool active = world.Entities.Deployment.TryGet(id, out Deployment deployment) && deployment.State == DeploymentState.Deployed;
                SetProvider(world, id, DeployedSolarExplorerRadius, active);
            }

            if (selectable.ContentType == T3TrikeType && world.Entities.Ownership.Has(id) && world.Entities.Transform.Has(id) && !world.Entities.ForwardServiceMember.Has(id))
                world.Entities.ForwardServiceMember.Set(id, new ForwardServiceMember { Provider = EntityId.None, QueryOwner = byte.MaxValue, QueryCellX = -1, QueryCellY = -1 });
        }
    }

    private static void SetProvider(SimulationWorld world, EntityId id, byte radius, bool active)
    {
        ForwardServiceProvider next = new() { RadiusBuildCells = radius, IsActive = active };
        if (!world.Entities.ForwardServiceProvider.TryGet(id, out ForwardServiceProvider current) ||
            current.RadiusBuildCells != next.RadiusBuildCells || current.IsActive != next.IsActive)
            world.Entities.ForwardServiceProvider.Set(id, next);
    }

    private static bool IsCanonicalProviderActive(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Selectable.TryGet(id, out Selectable selectable)) return false;
        if (selectable.ContentType == ServiceHubType)
            return world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed && BrownoutSystem.IsOperational(world, id);
        return selectable.ContentType == SolarExplorerType && world.Entities.Deployment.TryGet(id, out Deployment deployment) && deployment.State == DeploymentState.Deployed;
    }

    private void CollectActiveProviders(SimulationWorld world)
    {
        _providers.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ForwardServiceProvider.TryGet(id, out ForwardServiceProvider provider) || !provider.IsActive || provider.RadiusBuildCells == 0 ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            _providers.Add(new ProviderInfo(id, ownership.PlayerSlot, transform.Position, provider.RadiusBuildCells));
        }
    }

    private bool ProviderSetsEqual()
    {
        if (_providers.Count != _previousProviders.Count) return false;
        for (int i = 0; i < _providers.Count; i++) if (!_providers[i].Equals(_previousProviders[i])) return false;
        return true;
    }

    private void RebuildProviderBuckets()
    {
        _providerBuckets.Clear();
        for (int i = 0; i < _providers.Count; i++)
        {
            ProviderInfo provider = _providers[i];
            int minX = ClampBucket((provider.Position.X.FloorToInt() - provider.Radius) / ProviderBucketBuildCells, BucketWidth);
            int minY = ClampBucket((provider.Position.Y.FloorToInt() - provider.Radius) / ProviderBucketBuildCells, BucketHeight);
            int maxX = ClampBucket((provider.Position.X.FloorToInt() + provider.Radius) / ProviderBucketBuildCells, BucketWidth);
            int maxY = ClampBucket((provider.Position.Y.FloorToInt() + provider.Radius) / ProviderBucketBuildCells, BucketHeight);
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                int key = y * BucketWidth + x;
                if (!_providerBuckets.TryGetValue(key, out List<EntityId>? bucket)) { bucket = new List<EntityId>(4); _providerBuckets.Add(key, bucket); }
                bucket.Add(provider.Entity);
            }
        }
    }

    private void UpdateMembers(SimulationWorld world, bool providersChanged)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ForwardServiceMember.Has(id) || !world.Entities.Transform.TryGet(id, out SimTransform transform) ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership)) continue;
            int cellX = Math.Clamp(transform.Position.X.FloorToInt(), 0, MapGrid.BuildWidth - 1);
            int cellY = Math.Clamp(transform.Position.Y.FloorToInt(), 0, MapGrid.BuildHeight - 1);
            ref ForwardServiceMember member = ref world.Entities.ForwardServiceMember.Get(id);
            if (!providersChanged && member.QueryOwner == ownership.PlayerSlot && member.QueryCellX == cellX && member.QueryCellY == cellY) continue;
            member.QueryOwner = ownership.PlayerSlot;
            member.QueryCellX = checked((short)cellX);
            member.QueryCellY = checked((short)cellY);
            member.Provider = FindProvider(world, ownership.PlayerSlot, transform.Position, cellX / ProviderBucketBuildCells, cellY / ProviderBucketBuildCells);
        }
    }

    private EntityId FindProvider(SimulationWorld world, byte owner, FixVec2 position, int bucketX, int bucketY)
    {
        if (!_providerBuckets.TryGetValue(bucketY * BucketWidth + bucketX, out List<EntityId>? candidates)) return EntityId.None;
        EntityId best = EntityId.None;
        for (int i = 0; i < candidates.Count; i++)
        {
            EntityId candidate = candidates[i];
            if (!world.Entities.Ownership.TryGet(candidate, out Ownership candidateOwner) || candidateOwner.PlayerSlot != owner ||
                !world.Entities.Transform.TryGet(candidate, out SimTransform candidateTransform) ||
                !world.Entities.ForwardServiceProvider.TryGet(candidate, out ForwardServiceProvider provider) || !provider.IsActive ||
                !Contains(candidateTransform.Position, provider.RadiusBuildCells, position)) continue;
            if (best == EntityId.None || candidate.Value < best.Value) best = candidate;
        }
        return best;
    }

    private void CacheProviders()
    {
        _previousProviders.Clear();
        _previousProviders.AddRange(_providers);
    }

    private static bool Contains(FixVec2 center, byte radius, FixVec2 point)
    {
        FixVec2 delta = point - center;
        Fix32 range = Fix32.FromInt(radius);
        return delta.LengthSquared() <= range * range;
    }

    private static int ClampBucket(int value, int count) => value < 0 ? 0 : value >= count ? count - 1 : value;
}
}
