using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5TubeGraphTests
{
    private static readonly ContentId HangarType = StableId.FromKey("building.mar.aero_tube_hangar");
    private static readonly ContentId StationType = StableId.FromKey("building.mar.settlement_station");

    [Test]
    public void CanonicalStationsExposeConnectionLimitsAndRedundantRoutingBonus()
    {
        SimulationWorld world = CreateWorld();
        EntityId hangar = AddStation(world, 0, HangarType, 10, 10);
        EntityId station = AddStation(world, 0, StationType, 40, 10);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.TubeStation.Get(hangar).ConnectionLimit, Is.EqualTo(5));
            Assert.That(world.Entities.TubeStation.Get(station).ConnectionLimit, Is.EqualTo(3));
            Assert.That(world.Entities.TubeComponent.Get(hangar).StationCount, Is.EqualTo(1));
            Assert.That(world.Entities.TubeComponent.Get(station).StationCount, Is.EqualTo(1));
        });
        Assert.That(TubeGraphSystem.TrySetRedundantRouting(world, 0, station, true), Is.True);
        Assert.That(world.Entities.TubeStation.Get(station).ConnectionLimit, Is.EqualTo(4));
        Assert.That(TubeGraphSystem.TrySetRedundantRouting(world, 1, station, false), Is.False);
    }

    [Test]
    public void AutomaticLinksEnforceOwnershipDuplicatesLimitsAndCanonicalEconomics()
    {
        SimulationWorld world = CreateWorld();
        EntityId center = AddStation(world, 0, StationType, 70, 70);
        EntityId east = AddStation(world, 0, StationType, 100, 70);
        EntityId west = AddStation(world, 0, StationType, 35, 70);
        EntityId north = AddStation(world, 0, StationType, 70, 35);
        EntityId south = AddStation(world, 0, StationType, 70, 105);
        EntityId enemy = AddStation(world, 1, StationType, 120, 120);

        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, east, out EntityId first), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, east, out _), Is.False, "Duplicate Station pairs are invalid.");
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, enemy, out _), Is.False, "Enemy Stations cannot be linked.");
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, west, out _), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, north, out _), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, center, south, out _), Is.False, "Settlement Station baseline limit is three.");

        TubeLink link = world.Entities.TubeLink.Get(first);
        TubeRoute route = world.GetTubeRoute(first)!;
        Assert.Multiple(() =>
        {
            Assert.That(link.LengthBuildCells, Is.EqualTo(route.Cells.Count));
            Assert.That(link.EnergyDemandPerSecond, Is.EqualTo(1));
            Assert.That(TubeGraphSystem.GetOreCost(link.LengthBuildCells), Is.EqualTo(50 + 2 * link.LengthBuildCells));
            Assert.That(TubeGraphSystem.GetConstructionTicks(link.LengthBuildCells), Is.EqualTo(200 + 8 * link.LengthBuildCells));
            Assert.That(TubeGraphSystem.LinkActivationEnergyCost, Is.EqualTo(10));
            Assert.That(TubeGraphSystem.RouteIsStructurallyValid(route), Is.True);
        });

        SimulationWorld blocked = CreateWorld();
        EntityId blockedOrigin = AddStation(blocked, 0, StationType, 10, 10);
        EntityId blockedDestination = AddStation(blocked, 0, StationType, 45, 10);
        blocked.Map.SetFlagsRect(new IntRect(60, 0, 2, MapGrid.NavHeight), MapCellFlags.Impassable, MapCellFlags.Buildable);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(blocked, 0, blockedOrigin, blockedDestination, out _), Is.False, "A blocked automatic route does not create a Link.");
    }

    [Test]
    public void OperationalLinkChangesSplitAndReconnectStableComponentsWithoutMovingResources()
    {
        SimulationWorld world = CreateWorld();
        EntityId first = AddStation(world, 0, HangarType, 10, 20, ore: 100);
        EntityId middle = AddStation(world, 0, StationType, 50, 20);
        EntityId last = AddStation(world, 0, StationType, 90, 20, ore: 80);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, first, middle, out EntityId firstLink), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, middle, last, out _), Is.True);
        Assert.That(TubeGraphSystem.SharesComponent(world, first, last), Is.True);
        Assert.That(TubeGraphSystem.GetConnectedProcessedResourceTotal(world, middle, ResourceType.Ore), Is.EqualTo(180));
        Assert.That(TubeGraphSystem.TrySpendConnectedProcessedResource(world, 0, middle, ResourceType.Ore, 150), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.ResourceBank.Get(first).ProcessedAmount, Is.Zero);
            Assert.That(world.Entities.ResourceBank.Get(last).ProcessedAmount, Is.EqualTo(30));
        });

        uint beforeSegmentation = world.TubeSegmentationRevision;
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, firstLink, false), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(TubeGraphSystem.SharesComponent(world, first, last), Is.False);
            Assert.That(world.Entities.TubeStation.Get(first).ComponentRoot, Is.EqualTo(first));
            Assert.That(world.Entities.TubeStation.Get(middle).ComponentRoot, Is.EqualTo(middle));
            Assert.That(world.Entities.TubeLink.Get(firstLink).ComponentRoot, Is.EqualTo(EntityId.None));
            Assert.That(world.TubeSegmentationRevision, Is.EqualTo(beforeSegmentation + 1));
            Assert.That(TubeGraphSystem.GetConnectedProcessedResourceTotal(world, first, ResourceType.Ore), Is.Zero);
            Assert.That(TubeGraphSystem.GetConnectedProcessedResourceTotal(world, last, ResourceType.Ore), Is.EqualTo(30));
        });
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, firstLink, true), Is.True);
        Assert.That(TubeGraphSystem.SharesComponent(world, first, last), Is.True);
        Assert.That(world.Entities.TubeStation.Get(last).ComponentRoot, Is.EqualTo(first), "Lowest Station Entity ID is the stable component root.");
    }

    [Test]
    public void StationLossRemovesIncidentLinksAndEmitsOneComponentSegmentation()
    {
        SimulationWorld world = CreateWorld();
        EntityId first = AddStation(world, 0, HangarType, 10, 20, ore: 40);
        EntityId middle = AddStation(world, 0, StationType, 50, 20);
        EntityId last = AddStation(world, 0, StationType, 90, 20, ore: 60);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, first, middle, out EntityId firstLink), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, middle, last, out EntityId secondLink), Is.True);
        uint before = world.TubeSegmentationRevision;

        Assert.That(world.Entities.Destroy(middle), Is.True);
        Assert.That(TubeGraphSystem.Rebuild(world), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(firstLink), Is.False);
            Assert.That(world.Entities.Exists(secondLink), Is.False);
            Assert.That(world.GetTubeRoute(firstLink), Is.Null);
            Assert.That(world.GetTubeRoute(secondLink), Is.Null);
            Assert.That(TubeGraphSystem.SharesComponent(world, first, last), Is.False);
            Assert.That(world.TubeSegmentationRevision, Is.EqualTo(before + 1), "One topology event produces one component-level segmentation revision.");
            Assert.That(world.Entities.ResourceBank.Get(first).ProcessedAmount, Is.EqualTo(40));
            Assert.That(world.Entities.ResourceBank.Get(last).ProcessedAmount, Is.EqualTo(60));
        });
    }

    [Test]
    public void SnapshotPreservesTubeRoutesComponentsAndDeterministicContinuation()
    {
        SimulationWorld world = CreateWorld();
        EntityId first = AddStation(world, 0, HangarType, 15, 25);
        EntityId second = AddStation(world, 0, StationType, 55, 25);
        EntityId third = AddStation(world, 0, StationType, 95, 25);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, first, second, out EntityId firstLink), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, second, third, out EntityId secondLink), Is.True);
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, secondLink, false), Is.True);
        new SimulationRunner(world).StepTicks(9);
        ulong before = StateHasher.Hash(world);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(before));
            Assert.That(TubeGraphSystem.Rebuild(restored), Is.False, "Restored cached components are already exact.");
            Assert.That(restored.GetTubeRoute(firstLink)!.Cells.Count, Is.EqualTo(world.GetTubeRoute(firstLink)!.Cells.Count));
            Assert.That(restored.Entities.TubeLink.Get(secondLink).IsOperational, Is.False);
        });
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, secondLink, true), Is.True);
        Assert.That(TubeGraphSystem.TrySetLinkOperational(restored, 0, secondLink, true), Is.True);
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(60); restoredRunner.StepTicks(60);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static SimulationWorld CreateWorld() => new(new MapGrid("map.test.m5_tube_graph"), 2);

    private static EntityId AddStation(SimulationWorld world, byte owner, ContentId type, short anchorX, short anchorY, int? ore = null)
    {
        byte width = type == HangarType ? (byte)9 : (byte)7;
        byte height = width;
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform
        {
            Position = new FixVec2(Fix32.FromRatio(anchorX * 2 + width, 2), Fix32.FromRatio(anchorY * 2 + height, 2)),
            Orientation = Angle16.Zero
        });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building
        {
            Type = type, AnchorX = anchorX, AnchorY = anchorY, FootprintWidth = width, FootprintHeight = height, State = BuildingState.Completed
        });
        if (ore.HasValue) world.Entities.ResourceBank.Set(id, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = ore.Value });
        Assert.That(TubeGraphSystem.TryRegisterStation(world, id), Is.True);
        return id;
    }
}
}
