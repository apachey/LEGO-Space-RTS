using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class PrototypeContentTests
{
    [Test]
    public void PrototypeContentBinaryRoundTripsAndHashesIdentically()
    {
        PrototypeContentCatalog source = new PrototypeContentCatalog(
            new[] { new PrototypeMovementProfile("movement.test", Fix32.FromRatio(3, 2), MovementLayer.GroundHover) },
            new[] { new PrototypeEntityDefinition("unit.test", "Technical", "ENGINEERING_ONLY", "movement.test", FootprintClass.Small, SelectableKind.CombatSupport, 8, "view.test") });
        byte[] bytes = PrototypeContentCodec.Write(source);
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(bytes);
        Assert.That(restored.ContentHash, Is.EqualTo(source.ContentHash));
        Assert.That(restored.Entities[0].StableKey, Is.EqualTo("unit.test"));
        Assert.That(restored.MovementProfiles[0].MaxSpeed, Is.EqualTo(Fix32.FromRatio(3, 2)));
    }
}
}
