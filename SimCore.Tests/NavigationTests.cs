using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class NavigationTests
{
    [Test] public void SimpleRouteIsLegalAndRepeatable()
    {
        MapGrid map = DevMapFactory.Create(); HierarchicalPathfinder p = new(map); NavCell s = new(20,20); NavCell g = new(120,50);
        NavPath a = p.FindPath(s,g,FootprintClass.Medium); NavPath b = p.FindPath(s,g,FootprintClass.Medium);
        Assert.That(a.IsValid, Is.True); Assert.That(b.Cells, Is.EqualTo(a.Cells));
        foreach (NavCell c in a.Cells) Assert.That(p.IsPassable(c,FootprintClass.Medium), Is.True);
    }

    [Test] public void EqualCostTieBreakIsStable()
    {
        HierarchicalPathfinder p = new(DevMapFactory.Create()); NavPath a = p.FindPath(new NavCell(30,180), new NavCell(130,180), FootprintClass.Tiny);
        NavPath b = p.FindPath(new NavCell(30,180), new NavCell(130,180), FootprintClass.Tiny); Assert.That(b.Cells, Is.EqualTo(a.Cells));
    }

    [Test] public void HugeHasStricterClearanceThanTiny()
    {
        HierarchicalPathfinder p = new(DevMapFactory.Create()); NavCell nearWall = new(148,100);
        Assert.That(p.IsPassable(nearWall,FootprintClass.Tiny), Is.True); Assert.That(p.IsPassable(nearWall,FootprintClass.Huge), Is.False);
    }

    [Test] public void DynamicExcavationChangesTopologyLocally()
    {
        MapGrid map = DevMapFactory.Create(); HierarchicalPathfinder p = new(map); NavCell blocked = new(155,268);
        Assert.That(p.IsPassable(blocked,FootprintClass.Tiny), Is.False); IntRect rect = map.OpenFeature(DevMapFactory.ExcavatableFeatureId); p.RebuildAffected(rect);
        Assert.That(p.IsPassable(blocked,FootprintClass.Tiny), Is.True); Assert.That(map.TopologyVersion, Is.EqualTo(1));
    }
    [Test] public void ExcavatableShortcutProducesShorterRouteAfterOpening()
    {
        MapGrid map = DevMapFactory.Create(); HierarchicalPathfinder p = new(map);
        NavCell start = new(120,270), goal = new(200,270);
        NavPath before = p.FindPath(start,goal,FootprintClass.Tiny); Assert.That(before.IsValid, Is.True);
        IntRect affected = map.OpenFeature(DevMapFactory.ExcavatableFeatureId); p.RebuildAffected(affected);
        NavPath after = p.FindPath(start,goal,FootprintClass.Tiny); Assert.That(after.IsValid, Is.True);
        Assert.That(PathLength(after), Is.LessThan(PathLength(before)));
    }

    [Test] public void OpenRouteIsSmoothedIntoLongDeterministicSegments()
    {
        HierarchicalPathfinder p = new(DevMapFactory.Create());
        NavPath path = p.FindPath(new NavCell(20,20), new NavCell(120,50), FootprintClass.Medium);
        Assert.That(path.IsValid, Is.True);
        Assert.That(path.Cells.Count, Is.LessThan(40), "M2 path output should be a smoothed waypoint route, not every 0.5-cell A* step.");
        NavPath repeat = p.FindPath(new NavCell(20,20), new NavCell(120,50), FootprintClass.Medium);
        Assert.That(repeat.Cells, Is.EqualTo(path.Cells));
    }

    [Test] public void PersistentCorridorOutputIsDeterministic()
    {
        HierarchicalPathfinder p = new(DevMapFactory.Create());
        RouteCorridor first = new(); RouteCorridor repeat = new();
        p.FindCorridor(new NavCell(20,20), new NavCell(120,50), FootprintClass.Medium, first);
        p.FindCorridor(new NavCell(20,20), new NavCell(120,50), FootprintClass.Medium, repeat);
        Assert.That(first.IsValid, Is.True);
        Assert.That(repeat.Cells, Is.EqualTo(first.Cells));
        Assert.That(repeat.TopologyVersion, Is.EqualTo(first.TopologyVersion));
    }

    [Test] public void CorridorWindowAllowsBoundedLocalDeviation()
    {
        RouteCorridor corridor = new();
        corridor.Cells.Add(new NavCell(20,20)); corridor.Cells.Add(new NavCell(40,20)); corridor.Cells.Add(new NavCell(60,20));
        Assert.That(corridor.Contains(new NavCell(30,24),1), Is.True);
        Assert.That(corridor.Contains(new NavCell(30,25),1), Is.False);
        RouteCorridor diagonal = new(); diagonal.Cells.Add(new NavCell(20,20)); diagonal.Cells.Add(new NavCell(40,40));
        Assert.That(diagonal.Contains(new NavCell(20,40),1), Is.False, "A segment corridor is not its full axis-aligned bounding box.");
    }

    [Test] public void ExcavationInvalidatesOnlySpatiallyAffectedCorridors()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(2); EntityId[] ids = ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{ids[0]},FixVec2.FromInts(100,30)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,2,SimCommandType.Move,new[]{ids[1]},FixVec2.FromInts(110,40)));
        new SimulationRunner(world).StepOneTick();
        ExcavatableFeature feature = world.Map.Features[0];
        RouteCorridor affected = world.GetCorridor(ids[0])!; RouteCorridor unaffected = world.GetCorridor(ids[1])!;
        affected.Cells.Clear(); affected.Cells.Add(new NavCell(feature.NavRect.X,feature.NavRect.Y));
        unaffected.Cells.Clear(); unaffected.Cells.Add(new NavCell(10,10));
        ref NavigationAgent affectedNav = ref world.Entities.Navigation.Get(ids[0]);
        ref NavigationAgent unaffectedNav = ref world.Entities.Navigation.Get(ids[1]);
        affectedNav.PathDirty = false; unaffectedNav.PathDirty = false;
        world.OpenExcavatable(feature.FeatureId);
        Assert.That(affectedNav.PathDirty, Is.True);
        Assert.That(unaffectedNav.PathDirty, Is.False);
        Assert.That(unaffected.TopologyVersion, Is.EqualTo(world.Map.TopologyVersion));
        Assert.That(unaffectedNav.PathTopologyVersion, Is.EqualTo(world.Map.TopologyVersion));
    }

    private static Fix32 PathLength(NavPath path)
    {
        Fix32 total=Fix32.Zero;
        for(int i=1;i<path.Cells.Count;i++) total+=FixVec2.Distance(MapGrid.NavCellCenterToBuild(path.Cells[i-1]),MapGrid.NavCellCenterToBuild(path.Cells[i]));
        return total;
    }

}
