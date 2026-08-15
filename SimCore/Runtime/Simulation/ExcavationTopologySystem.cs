using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>
/// Binds authored map Excavatable Features to authoritative ECS entities. The map owns the
/// dense navigation/LoS raster; the entity owns the stable gameplay identity and state.
/// </summary>
public static class ExcavationTopologySystem
{
    public static void InitializeFeatures(SimulationWorld world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            if (!world.Entities.Excavatable.TryGet(alive[i], out Excavatable authoritative)) continue;
            if (!world.Map.TryGetFeature(authoritative.MapFeatureId, out ExcavatableFeature authored))
                throw new InvalidOperationException($"Authoritative Excavatable Feature {authoritative.MapFeatureId} is absent from the map.");
            ValidateBinding(authoritative, authored);
        }

        List<ExcavatableFeature> ordered = new(world.Map.Features.Count);
        for (int i = 0; i < world.Map.Features.Count; i++) ordered.Add(world.Map.Features[i]);
        ordered.Sort((a, b) => a.FeatureId.CompareTo(b.FeatureId));

        for (int i = 0; i < ordered.Count; i++)
        {
            ExcavatableFeature feature = ordered[i];
            if (TryGetFeatureEntity(world, feature.FeatureId, out EntityId existing))
            {
                ValidateBinding(world.Entities.Excavatable.Get(existing), feature);
                continue;
            }

            EntityId id = world.Entities.Create();
            world.Entities.Excavatable.Set(id, FromFeature(feature));
        }
    }

    public static bool TryGetFeatureEntity(SimulationWorld world, ushort featureId, out EntityId entity)
    {
        entity = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Excavatable.TryGet(id, out Excavatable feature) || feature.MapFeatureId != featureId) continue;
            if (entity != EntityId.None) throw new InvalidOperationException($"Duplicate authoritative Excavatable Feature {featureId}.");
            entity = id;
        }
        return entity != EntityId.None;
    }

    internal static void MarkOpen(SimulationWorld world, ushort featureId)
    {
        if (!TryGetFeatureEntity(world, featureId, out EntityId entity))
            throw new InvalidOperationException($"Excavatable Feature {featureId} has no authoritative entity.");
        ref Excavatable feature = ref world.Entities.Excavatable.Get(entity);
        feature.State = ExcavatableFeatureState.Open;
    }

    private static Excavatable FromFeature(ExcavatableFeature feature) => new()
    {
        MapFeatureId = feature.FeatureId,
        StableId = feature.StableId,
        TerrainClass = feature.TerrainClass,
        State = feature.State,
        RequiredEnergy = feature.RequiredEnergy,
        VisualProfile = feature.VisualProfile
    };

    private static void ValidateBinding(Excavatable authoritative, ExcavatableFeature feature)
    {
        if (authoritative.StableId != feature.StableId || authoritative.TerrainClass != feature.TerrainClass ||
            authoritative.RequiredEnergy != feature.RequiredEnergy || authoritative.VisualProfile != feature.VisualProfile ||
            authoritative.State != feature.State)
            throw new InvalidOperationException($"Authoritative Excavatable Feature {feature.FeatureId} disagrees with map topology state.");
    }
}
}
