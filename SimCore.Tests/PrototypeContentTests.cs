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

    [Test]
    public void BuildingDefinitionsRoundTripExplicitFootprintsCostsAndExits()
    {
        BuildingDefinition sourceBuilding = new("building.test", 3, 2, 0b11_1111UL, true, 140, 15, 600, 2, 1, FootprintClass.Medium);
        PrototypeContentCatalog source = new(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(), buildings: new[] { sourceBuilding });
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source));
        BuildingDefinition building = restored.Buildings[0];
        Assert.That(building.Id, Is.EqualTo(StableId.FromKey("building.test")));
        Assert.That(building.FootprintWidth, Is.EqualTo(3));
        Assert.That(building.FootprintHeight, Is.EqualTo(2));
        Assert.That(building.RotatedWidth(1), Is.EqualTo(2));
        Assert.That(building.OreCost, Is.EqualTo(140));
        Assert.That(building.EnergyCost, Is.EqualTo(15));
        Assert.That(building.ProductionExitFootprint, Is.EqualTo(FootprintClass.Medium));
    }

    [Test]
    public void ProductionDefinitionsRoundTripCanonicalQueueMetadata()
    {
        UnitProductionDefinition sourceProduction = new("unit.test", "building.test", 90, 10, 0, 2, 560);
        PrototypeContentCatalog source = new(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(), production: new[] { sourceProduction });
        UnitProductionDefinition restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source)).Production[0];
        Assert.Multiple(() =>
        {
            Assert.That(restored.UnitType, Is.EqualTo(StableId.FromKey("unit.test")));
            Assert.That(restored.ProducerType, Is.EqualTo(StableId.FromKey("building.test")));
            Assert.That(restored.OreCost, Is.EqualTo(90)); Assert.That(restored.OperationsCapacity, Is.EqualTo(2)); Assert.That(restored.BuildTicks, Is.EqualTo(560));
        });
    }
}
}
