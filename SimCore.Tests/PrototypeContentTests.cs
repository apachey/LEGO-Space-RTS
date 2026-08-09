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

    [Test]
    public void ResourceNodeDefinitionsRoundTripWithStableIdsAndThresholds()
    {
        PrototypeContentCatalog source = new PrototypeContentCatalog(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(),
            new[] { new ResourceNodeDefinition("resource.test", ResourceType.Ore, ResourceDepositSize.Standard, 900, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.test.resource") });
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source));
        Assert.That(restored.ResourceNodes.Length, Is.EqualTo(1));
        Assert.That(restored.ResourceNodes[0].Id, Is.EqualTo(StableId.FromKey("resource.test")));
        Assert.That(restored.ResourceNodes[0].Capacity, Is.EqualTo(900));
        Assert.That(restored.ResourceNodes[0].CriticalThresholdBasisPoints, Is.EqualTo(2500));
    }
}
}
