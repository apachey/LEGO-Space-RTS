using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4ProjectileTests
{
    private const string PulseWeapon = "weapon.rr.hover_scout.survey_pulse";

    [Test]
    public void SurveyPulseTravelsAtCanonicalTwelveCellsPerSecondAndImpactsDeterministically()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnSource(world, FixVec2.FromInts(20, 20), PulseWeapon);
        EntityId target = SpawnTarget(world, FixVec2.FromInts(23, 20));
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        Assert.That(world.Projectiles.Count, Is.EqualTo(1));
        ProjectileRecord launched = world.Projectiles[0];
        Assert.Multiple(() =>
        {
            Assert.That(launched.Id.Value, Is.EqualTo(1));
            Assert.That(launched.Source, Is.EqualTo(source));
            Assert.That(launched.Target, Is.EqualTo(target));
            Assert.That(launched.Position, Is.EqualTo(FixVec2.FromInts(20, 20) + launched.Velocity * SimClock.TickSeconds));
            Assert.That(launched.Velocity, Is.EqualTo(new FixVec2(Fix32.FromInt(12), Fix32.Zero)));
            Assert.That(launched.CommittedImpactPosition, Is.EqualTo(FixVec2.FromInts(23, 20)));
            Assert.That(world.ProjectileImpacts, Is.Empty);
        });

        runner.StepTicks(4);
        Assert.That(world.Projectiles, Has.Count.EqualTo(1));
        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Projectiles, Is.Empty);
            Assert.That(world.ProjectileImpacts, Has.Count.EqualTo(1));
            Assert.That(world.ProjectileImpacts[0].ProjectileId.Value, Is.EqualTo(1));
            Assert.That(world.ProjectileImpacts[0].Position, Is.EqualTo(FixVec2.FromInts(23, 20)));
            Assert.That(world.ProjectileImpacts[0].ImpactTick, Is.EqualTo(6));
            Assert.That(world.ProjectileImpacts[0].BaseDamage, Is.EqualTo(6));
        });
        SimulationWorld restoredImpact = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(restoredImpact.ProjectileImpacts, Has.Count.EqualTo(1));
            Assert.That(restoredImpact.ProjectileImpacts[0].ProjectileId.Value, Is.EqualTo(1));
            Assert.That(StateHasher.Hash(restoredImpact), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    [Test]
    public void OrdinaryProjectileContinuesToCommittedPositionAfterTargetLossWithoutRetargeting()
    {
        SimulationWorld world = NewWorld();
        SpawnSource(world, FixVec2.FromInts(20, 20), PulseWeapon);
        EntityId originalTarget = SpawnTarget(world, FixVec2.FromInts(23, 20));
        EntityId replacement = SpawnTarget(world, FixVec2.FromInts(22, 21));
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        Assert.That(world.Projectiles[0].Target, Is.EqualTo(replacement), "Closer legal target should be selected deterministically for setup.");
        ProjectileRecord projectile = world.Projectiles[0];
        FixVec2 committed = projectile.CommittedImpactPosition;
        EntityId committedTarget = projectile.Target;
        Assert.That(world.Entities.Destroy(committedTarget), Is.True);

        int guard = 0;
        while (world.Projectiles.Count > 0 && guard++ < 10) runner.StepOneTick();
        ProjectileImpactRecord impact = world.ProjectileImpacts.Single();
        Assert.Multiple(() =>
        {
            Assert.That(impact.Target, Is.EqualTo(committedTarget));
            Assert.That(impact.Target, Is.Not.EqualTo(originalTarget));
            Assert.That(impact.Position, Is.EqualTo(committed));
        });
    }

    [Test]
    public void SimultaneousShotsRemainIndependentAndProduceOrderedOverkillImpacts()
    {
        SimulationWorld world = NewWorld();
        EntityId sourceA = SpawnSource(world, FixVec2.FromInts(20, 20), PulseWeapon);
        EntityId sourceB = SpawnSource(world, FixVec2.FromInts(20, 20), PulseWeapon);
        EntityId target = SpawnTarget(world, FixVec2.FromInts(23, 20));
        SimulationRunner runner = new(world);

        runner.StepTicks(6);

        Assert.Multiple(() =>
        {
            Assert.That(world.Projectiles, Is.Empty);
            Assert.That(world.ProjectileImpacts, Has.Count.EqualTo(2));
            Assert.That(world.ProjectileImpacts.Select(x => x.ProjectileId.Value), Is.EqualTo(new uint[] { 1, 2 }));
            Assert.That(world.ProjectileImpacts.Select(x => x.Source), Is.EqualTo(new[] { sourceA, sourceB }));
            Assert.That(world.ProjectileImpacts.All(x => x.Target == target), Is.True);
            Assert.That(world.ProjectileImpacts.Sum(x => x.BaseDamage), Is.EqualTo(12));
        });
    }

    [Test]
    public void InFlightProjectileSnapshotRestoresAndContinuesWithIdenticalHash()
    {
        SimulationWorld world = NewWorld();
        SpawnSource(world, FixVec2.FromInts(40, 40), PulseWeapon);
        SpawnTarget(world, FixVec2.FromInts(43, 40));
        SimulationRunner original = new(world);
        original.StepTicks(2);
        Assert.That(world.Projectiles, Has.Count.EqualTo(1));

        SimulationRunner restored = new(SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world)));
        original.StepTicks(64);
        restored.StepTicks(64);

        Assert.That(StateHasher.Hash(restored.World), Is.EqualTo(StateHasher.Hash(original.World)));
    }

    [Test]
    public void ContactWeaponDoesNotCreateProjectileRecord()
    {
        SimulationWorld world = NewWorld();
        SpawnSource(world, FixVec2.FromInts(10, 10), "weapon.rr.crew.portable_mining_tool");
        SpawnTarget(world, FixVec2.FromInts(10, 10));

        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Projectiles, Is.Empty);
        Assert.That(world.ProjectileImpacts, Is.Empty);
    }

    private static SimulationWorld NewWorld() => new(new MapGrid("map.test.m4.projectiles"), 2);

    private static EntityId SpawnSource(SimulationWorld world, FixVec2 position, string weaponKey)
    {
        Assert.That(world.Content.TryGetWeapon(weaponKey, out WeaponDefinition weapon), Is.True);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.LightMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        world.Entities.Targeting.Set(id, new Targeting
        {
            AcquisitionRadius = weapon.Range + Fix32.FromInt(2), LegalLayers = weapon.LegalTargetLayers, LegalClasses = weapon.LegalTargetClasses,
            PriorityProfile = weapon.PriorityProfile, CurrentTarget = EntityId.None, SelectionKind = TargetSelectionKind.None
        });
        world.Entities.Weapon.Set(id, new WeaponState { WeaponProfile = weapon.Id, LastFiredTarget = EntityId.None, LastFiredTick = -1 });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 10, LastFogX = -1, LastFogY = -1 });
        return id;
    }

    private static EntityId SpawnTarget(SimulationWorld world, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 1 });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.MediumMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        return id;
    }
}
