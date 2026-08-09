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

    [TestCase(FootprintClass.Tiny)]
    [TestCase(FootprintClass.Small)]
    [TestCase(FootprintClass.Medium)]
    [TestCase(FootprintClass.Large)]
    [TestCase(FootprintClass.Huge)]
    public void CompletedFormationSlotsLeaveCanonicalSettlingMargin(FootprintClass footprint)
    {
        FixVec2 center=FixVec2.FromInts(40,40);
        FixVec2 first=FormationPlanner.GetSlot(center,0,2,footprint,FixVec2.FromInts(1,0),2,false);
        FixVec2 second=FormationPlanner.GetSlot(center,1,2,footprint,FixVec2.FromInts(1,0),2,false);
        Fix32 canonicalMinimum=FootprintRules.CollisionRadiusBuild(footprint)*Fix32.FromInt(2)+Fix32.FromRatio(35,100);

        Assert.That(FixVec2.Distance(first,second).Raw,Is.GreaterThanOrEqualTo(canonicalMinimum.Raw),
            $"{footprint} arrival slots would keep local separation active after the Move completed.");
    }
}
