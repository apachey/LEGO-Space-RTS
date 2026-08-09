using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class Fix32Tests
{
    [Test] public void PositiveArithmetic() => Assert.That((Fix32.FromInt(2) + Fix32.FromInt(3)).Raw, Is.EqualTo(Fix32.FromInt(5).Raw));
    [Test] public void NegativeArithmetic() => Assert.That((Fix32.FromInt(-2) - Fix32.FromInt(3)).Raw, Is.EqualTo(Fix32.FromInt(-5).Raw));
    [Test] public void Multiply() => Assert.That((Fix32.FromRatio(3,2) * Fix32.FromInt(2)).Raw, Is.EqualTo(Fix32.FromInt(3).Raw));
    [Test] public void Divide() => Assert.That((Fix32.FromInt(3) / Fix32.FromInt(2)).Raw, Is.EqualTo(Fix32.FromRatio(3,2).Raw));
    [Test] public void Compare() => Assert.That(Fix32.FromRatio(11,10), Is.GreaterThan(Fix32.One));
    [Test] public void FloorCeilRound() { Assert.That(Fix32.FromRatio(17,10).FloorToInt(), Is.EqualTo(1)); Assert.That(Fix32.FromRatio(17,10).CeilToInt(), Is.EqualTo(2)); Assert.That(Fix32.FromRatio(17,10).RoundToInt(), Is.EqualTo(2)); }
    [Test] public void Sqrt() => Assert.That(Fix32.Sqrt(Fix32.FromInt(9)).Raw, Is.EqualTo(Fix32.FromInt(3).Raw));
    [Test] public void OverflowFailsFast() => Assert.Throws<OverflowException>(() => { _ = Fix32.FromInt(30000) * Fix32.FromInt(30000); });
    [Test] public void RepeatedOperationsAreExact() { Fix32 a = Fix32.FromRatio(7,3); Fix32 x = Fix32.One; for (int i=0;i<100;i++) x = (x * a) / a; Assert.That(x.Raw, Is.EqualTo(Fix32.One.Raw)); }
}
