using System;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M5ExcavationTopologyTests
{
    [Test]
    public void DevFeatureCarriesCanonicalAuthoredMetadataAndEntityIdentity()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        ExcavatableFeature feature = world.Map.Features.Single();
        EntityId entity = FeatureEntity(world, feature.FeatureId);
        Excavatable authoritative = world.Entities.Excavatable.Get(entity);

        Assert.Multiple(() =>
        {
            Assert.That(feature.StableKey, Is.EqualTo("feature.dev.fractured_shortcut"));
            Assert.That(feature.TerrainClass, Is.EqualTo(ExcavatableTerrainClass.FracturedRockWall));
            Assert.That(feature.RequiredEnergy, Is.EqualTo(25));
            Assert.That(feature.OpenBuildable, Is.False);
            Assert.That(feature.State, Is.EqualTo(ExcavatableFeatureState.Blocked));
            Assert.That(authoritative.MapFeatureId, Is.EqualTo(feature.FeatureId));
            Assert.That(authoritative.StableId, Is.EqualTo(feature.StableId));
            Assert.That(authoritative.State, Is.EqualTo(ExcavatableFeatureState.Blocked));
        });
    }

    [Test]
    public void OpeningIsOneWayIdempotentAndRebuildsOnlyLocalClusters()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        ExcavatableFeature feature = world.Map.Features.Single();
        int version = world.Map.TopologyVersion;

        Assert.That(world.OpenExcavatable(feature.FeatureId), Is.True);
        int locallyRebuilt = world.Pathfinder.LastRebuiltClusterCount;
        MapCellFlags openedFlags = world.Map.GetFlags(feature.NavRect.X, feature.NavRect.Y);
        Assert.Multiple(() =>
        {
            Assert.That(world.Map.TopologyVersion, Is.EqualTo(version + 1));
            Assert.That(locallyRebuilt, Is.GreaterThan(0));
            Assert.That(locallyRebuilt, Is.LessThan(HierarchicalPathfinder.ClusterWidth * HierarchicalPathfinder.ClusterHeight));
            Assert.That(openedFlags.HasFlag(MapCellFlags.Impassable), Is.False);
            Assert.That(openedFlags.HasFlag(MapCellFlags.GroundOccluder), Is.False);
            Assert.That(openedFlags.HasFlag(MapCellFlags.Excavatable), Is.False);
            Assert.That(openedFlags.HasFlag(MapCellFlags.Buildable), Is.False);
            Assert.That(world.Entities.Excavatable.Get(FeatureEntity(world, feature.FeatureId)).State, Is.EqualTo(ExcavatableFeatureState.Open));
        });

        Assert.That(world.OpenExcavatable(feature.FeatureId), Is.False);
        Assert.That(world.Map.TopologyVersion, Is.EqualTo(version + 1), "An already-open route must not rebuild or advance topology again.");
        Assert.That(world.Pathfinder.LastRebuiltClusterCount, Is.EqualTo(locallyRebuilt));
    }

    [Test]
    public void OpenedRouteBecomesOrdinaryUniversalGround()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        ExcavatableFeature feature = world.Map.Features.Single();
        NavCell start = new(120, 270), goal = new(200, 270);
        Fix32 before = PathLength(world.Pathfinder.FindPath(start, goal, FootprintClass.Tiny));

        world.OpenExcavatable(feature.FeatureId);
        NavPath playerZero = world.Pathfinder.FindPath(start, goal, FootprintClass.Tiny);
        NavPath anyOtherPlayer = world.Pathfinder.FindPath(start, goal, FootprintClass.Tiny);

        Assert.Multiple(() =>
        {
            Assert.That(playerZero.IsValid, Is.True);
            Assert.That(anyOtherPlayer.Cells, Is.EqualTo(playerZero.Cells), "Opened topology must not carry Raider ownership restrictions.");
            Assert.That(PathLength(playerZero), Is.LessThan(before));
        });
    }

    [Test]
    public void CompiledMapRoundTripPreservesExcavatableMetadata()
    {
        MapDefinition restored = CompiledMapCodec.ReadDefinition(CompiledMapCodec.Write(DevMapFactory.CreateDefinition()));
        ExcavatableFeature feature = restored.Grid.Features.Single();
        Assert.Multiple(() =>
        {
            Assert.That(CompiledMapCodec.Version, Is.EqualTo(4));
            Assert.That(feature.StableKey, Is.EqualTo("feature.dev.fractured_shortcut"));
            Assert.That(feature.TerrainClass, Is.EqualTo(ExcavatableTerrainClass.FracturedRockWall));
            Assert.That(feature.RequiredEnergy, Is.EqualTo(25));
            Assert.That(feature.VisualProfile.Value, Is.Not.EqualTo(0));
            Assert.That(feature.OpenBuildable, Is.False);
        });
    }

    [Test]
    public void SnapshotPreservesOpenedFeatureEntityAndDeterministicHash()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        ushort featureId = world.Map.Features.Single().FeatureId;
        world.OpenExcavatable(featureId);
        ulong expected = StateHasher.Hash(world);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(expected));
            Assert.That(restored.Map.Features.Single().State, Is.EqualTo(ExcavatableFeatureState.Open));
            Assert.That(restored.Entities.Excavatable.Get(FeatureEntity(restored, featureId)).State, Is.EqualTo(ExcavatableFeatureState.Open));
        });
    }

    [Test]
    public void AuthoredFeaturesRejectDuplicateIdsStableIdsAndOverlaps()
    {
        MapGrid map = new("map.excavation.validation");
        ExcavatableFeature first = Feature("feature.validation.first", 1, new IntRect(10, 10, 4, 4));
        map.AddExcavatable(first);

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => map.AddExcavatable(Feature("feature.validation.second", 1, new IntRect(20, 20, 4, 4))));
            Assert.Throws<InvalidOperationException>(() => map.AddExcavatable(Feature("feature.validation.first", 2, new IntRect(20, 20, 4, 4))));
            Assert.Throws<InvalidOperationException>(() => map.AddExcavatable(Feature("feature.validation.third", 3, new IntRect(12, 12, 4, 4))));
        });
    }

    private static ExcavatableFeature Feature(string key, ushort id, IntRect rect) => new(
        key, id, rect, ExcavatableTerrainClass.FracturedRockWall, 25,
        StableId.FromKey("view.placeholder.excavatable.fractured_rock_wall"), openBuildable: false);

    private static EntityId FeatureEntity(SimulationWorld world, ushort featureId)
    {
        Assert.That(ExcavationTopologySystem.TryGetFeatureEntity(world, featureId, out EntityId entity), Is.True);
        return entity;
    }

    private static Fix32 PathLength(NavPath path)
    {
        Fix32 total = Fix32.Zero;
        for (int i = 1; i < path.Cells.Count; i++)
            total += FixVec2.Distance(MapGrid.NavCellCenterToBuild(path.Cells[i - 1]), MapGrid.NavCellCenterToBuild(path.Cells[i]));
        return total;
    }
}
