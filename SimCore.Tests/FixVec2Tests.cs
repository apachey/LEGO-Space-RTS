using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class FixVec2Tests
{
    [Test] public void ArithmeticAndDotAreExact()
    {
        FixVec2 a = FixVec2.FromInts(3,4); FixVec2 b = FixVec2.FromInts(1,-2);
        Assert.That(a + b, Is.EqualTo(FixVec2.FromInts(4,2)));
        Assert.That(a - b, Is.EqualTo(FixVec2.FromInts(2,6)));
        Assert.That(FixVec2.Dot(a,b), Is.EqualTo(Fix32.FromInt(-5)));
    }
    [Test] public void LengthAndDistanceUseDeterministicSqrt()
    {
        FixVec2 a = FixVec2.FromInts(3,4);
        Assert.That(a.Length(), Is.EqualTo(Fix32.FromInt(5)));
        Assert.That(FixVec2.Distance(FixVec2.Zero,a), Is.EqualTo(Fix32.FromInt(5)));
    }
    [Test] public void NormalizeZeroIsSafe() => Assert.That(FixVec2.Zero.NormalizeSafe(), Is.EqualTo(FixVec2.Zero));
}
