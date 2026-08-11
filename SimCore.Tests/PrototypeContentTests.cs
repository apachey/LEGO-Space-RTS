using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class PrototypeContentTests
{
    [Test]
    public void BuiltInCatalogUsesCanonicalCompilerOrdering()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        AssertOrdered(catalog.MovementProfiles.Select(p => p.StableKey).ToArray());
        AssertOrdered(catalog.Entities.Select(e => e.StableKey).ToArray());
        AssertOrdered(catalog.ResourceNodes.Select(r => r.StableKey).ToArray());
        AssertOrdered(catalog.Buildings.Select(b => b.StableKey).ToArray());
        AssertOrdered(catalog.Production.Select(p => p.UnitStableKey).ToArray());
        AssertOrdered(catalog.Weapons.Select(w => w.StableKey).ToArray());
        AssertOrdered(catalog.Transformations.Select(t => t.StableKey).ToArray());
    }

    [Test]
    public void PrototypeContentBinaryRoundTripsAndHashesIdentically()
    {
        PrototypeContentCatalog source = new PrototypeContentCatalog(
            new[] { new PrototypeMovementProfile("movement.test", Fix32.FromRatio(3, 2), MovementLayer.GroundHover) },
            new[] { new PrototypeEntityDefinition("unit.test", "Technical", "ENGINEERING_ONLY", "movement.test", FootprintClass.Small, SelectableKind.CombatSupport, 8, "view.test", operationsCapacity: 3,
                combat: new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 120, 1, TargetPriorityProfile.Generalist, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromInt(7))) });
        byte[] bytes = PrototypeContentCodec.Write(source);
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(bytes);
        Assert.That(restored.ContentHash, Is.EqualTo(source.ContentHash));
        Assert.That(restored.Entities[0].StableKey, Is.EqualTo("unit.test"));
        Assert.That(restored.Entities[0].OperationsCapacity, Is.EqualTo(3));
        Assert.That(restored.Entities[0].Combat.TargetClass, Is.EqualTo(CombatTargetClass.LightMachine));
        Assert.That(restored.Entities[0].Combat.MaximumHitPoints, Is.EqualTo(120));
        Assert.That(restored.Entities[0].Combat.ArmorRating, Is.EqualTo(1));
        Assert.That(restored.Entities[0].Combat.PriorityProfile, Is.EqualTo(TargetPriorityProfile.Generalist));
        Assert.That(restored.Entities[0].Combat.AcquisitionRadius, Is.EqualTo(Fix32.FromInt(7)));
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
        BuildingDefinition sourceBuilding = new("building.test", 3, 2, 0b11_1111UL, true, 140, 15, 600, 2, 1, FootprintClass.Medium, 4, 10, 120, 3);
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
        Assert.That(building.OperationsCapacityProvided, Is.EqualTo(4));
        Assert.That(building.EnergyGenerationPerSecond, Is.EqualTo(10));
        Assert.That(building.EnergyReserveCapacity, Is.EqualTo(120));
        Assert.That(building.ContinuousEnergyDemandPerSecond, Is.EqualTo(3));
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

    [Test]
    public void WeaponDefinitionsRoundTripAuthoritativeFiringMetadata()
    {
        WeaponDefinition sourceWeapon = new("weapon.test", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Generalist,
            12, DamageType.General, 25, Fix32.FromRatio(7, 2), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(10), true, 45, 3_500);
        PrototypeContentCatalog source = new(System.Array.Empty<PrototypeMovementProfile>(), System.Array.Empty<PrototypeEntityDefinition>(), weapons: new[] { sourceWeapon });
        WeaponDefinition restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(source)).Weapons[0];
        Assert.Multiple(() =>
        {
            Assert.That(restored.Id, Is.EqualTo(StableId.FromKey("weapon.test")));
            Assert.That(restored.BaseDamage, Is.EqualTo(12));
            Assert.That(restored.CooldownTicks, Is.EqualTo(25));
            Assert.That(restored.Range, Is.EqualTo(Fix32.FromRatio(7, 2)));
            Assert.That(restored.DeliveryKind, Is.EqualTo(WeaponDeliveryKind.Projectile));
            Assert.That(restored.ProjectileSpeed, Is.EqualTo(Fix32.FromInt(10)));
            Assert.That(restored.RequiresLineOfSight, Is.True);
            Assert.That(restored.FacingToleranceAngle16, Is.EqualTo(Angle16.Quarter.Raw / 2));
            Assert.That(restored.MaximumMovingFireSpeedBasisPoints, Is.EqualTo(3_500));
        });
    }

    [Test]
    public void TransformationDefinitionsRoundTripCanonicalModeData()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        PrototypeContentCatalog restored = PrototypeContentCodec.Read(PrototypeContentCodec.Write(catalog));
        Assert.That(restored.Transformations, Has.Length.EqualTo(1));
        TransformationDefinition transformation = restored.Transformations[0];
        Assert.Multiple(() =>
        {
            Assert.That(transformation.EntityType, Is.EqualTo(StableId.FromKey("unit.astronauts.mx41_switch_fighter")));
            Assert.That(transformation.AToBDurationTicks, Is.EqualTo(45));
            Assert.That(transformation.CancellationThresholdBasisPoints, Is.EqualTo(4_000));
            Assert.That(transformation.RollbackTicks, Is.EqualTo(12));
            Assert.That(transformation.ReversalLockTicks, Is.EqualTo(160));
            Assert.That(transformation.ModeA.DisplayName, Is.EqualTo("Ground"));
            Assert.That(transformation.ModeB.DisplayName, Is.EqualTo("Flight"));
            Assert.That(transformation.ModeB.Combat.TargetLayer, Is.EqualTo(CombatTargetLayer.TrueAir));
            Assert.That(transformation.TransitionTargetLayers, Is.EqualTo(TargetLayerMask.All));
        });
    }

    private static void AssertOrdered(string[] keys)
    {
        string[] expected = keys.OrderBy(key => key, System.StringComparer.Ordinal).ToArray();
        Assert.That(keys, Is.EqualTo(expected));
    }
}
}
