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

    private static Fix32 PathLength(NavPath path)
    {
        Fix32 total=Fix32.Zero;
        for(int i=1;i<path.Cells.Count;i++) total+=FixVec2.Distance(MapGrid.NavCellCenterToBuild(path.Cells[i-1]),MapGrid.NavCellCenterToBuild(path.Cells[i]));
        return total;
    }

}
