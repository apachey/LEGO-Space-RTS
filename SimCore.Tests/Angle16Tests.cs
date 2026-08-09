using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class Angle16Tests
{
    [Test] public void ShortestDeltaWraps() { Assert.That(Angle16.ShortestDelta(new Angle16(65000), new Angle16(1000)), Is.EqualTo(1536)); Assert.That(Angle16.ShortestDelta(new Angle16(1000), new Angle16(65000)), Is.EqualTo(-1536)); }
    [Test] public void TurnTowardUsesFixedStep() => Assert.That(Angle16.TurnToward(Angle16.Zero, Angle16.Quarter, 1000).Raw, Is.EqualTo(1000));
    [Test] public void CardinalDirectionsAreStable() { Assert.That(Angle16.FromDirection(FixVec2.FromInts(1,0)).Raw, Is.EqualTo(0)); Assert.That(Angle16.FromDirection(FixVec2.FromInts(0,1)).Raw, Is.EqualTo(16384)); Assert.That(Angle16.FromDirection(FixVec2.FromInts(-1,0)).Raw, Is.EqualTo(32768)); Assert.That(Angle16.FromDirection(FixVec2.FromInts(0,-1)).Raw, Is.EqualTo(49152)); }
}
