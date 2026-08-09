using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class FormationTests
{
    [Test] public void SlotsRotateWithHeadingAndSpreadIncreasesSeparation()
    {
        FixVec2 center=FixVec2.FromInts(40,40); FixVec2 heading=FixVec2.FromInts(1,0);
        FixVec2 normal=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,heading,3,false);
        FixVec2 spread=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,heading,3,true);
        Assert.That(FixVec2.Distance(center,spread),Is.GreaterThan(FixVec2.Distance(center,normal)));
        FixVec2 north=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,FixVec2.FromInts(0,1),3,false);
        Assert.That(north,Is.Not.EqualTo(normal));
    }
}
