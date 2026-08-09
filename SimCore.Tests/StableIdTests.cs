using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class StableIdTests
{
    [Test] public void SameKeySameId() => Assert.That(StableId.FromKey("unit.rock_raiders.hover_scout"), Is.EqualTo(StableId.FromKey("UNIT.ROCK_RAIDERS.HOVER_SCOUT")));
    [Test] public void DuplicateRejected() { StableIdRegistry r = new(); r.Register("a"); Assert.Throws<InvalidOperationException>(() => r.Register("a")); }
}
