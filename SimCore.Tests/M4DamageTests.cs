using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4DamageTests
{
    private const string PulseWeapon = "weapon.rr.hover_scout.survey_pulse";

    [Test]
    public void CanonicalDamageTypeMatrixIsCompleteAndExact()
    {
        int[,] expected =
        {
            { 125, 130, 90, 70, 60, 60, 50 },
            { 110, 105, 100, 90, 80, 80, 70 },
            { 75, 85, 110, 155, 145, 110, 100 },
            { 55, 65, 80, 100, 115, 160, 175 },
            { 90, 140, 125, 110, 95, 30, 25 },
            { 90, 100, 85, 70, 50, 35, 25 }
        };
        for (int damageType = 0; damageType < 6; damageType++)
            for (int targetClass = 0; targetClass < 7; targetClass++)
                Assert.That(DamageSystem.TypeMultiplier((DamageType)damageType, (CombatTargetClass)targetClass),
                    Is.EqualTo(Fix32.FromRatio(expected[damageType, targetClass], 100)), $"matrix[{damageType},{targetClass}]");
    }

    [Test]
    public void CanonicalArmorMultipliersAndMinimumDamageRuleAreApplied()
    {
        int[] expected = { 100, 96, 92, 88, 84, 80 };
        for (byte armor = 0; armor <= 5; armor++)
            Assert.That(DamageSystem.ArmorMultiplier(armor), Is.EqualTo(Fix32.FromRatio(expected[armor], 100)));

        Assert.That(DamageSystem.CalculateDamage(1, DamageType.General, CombatTargetClass.Personnel, 5), Is.EqualTo(Fix32.One),
            "Armor alone cannot reduce an otherwise whole point below one.");
        Assert.That(DamageSystem.CalculateDamage(1, DamageType.Control, CombatTargetClass.FortifiedStructure, 5), Is.LessThan(Fix32.One),
            "The damage-type counter matrix may still produce sub-one fractional damage.");
    }

    [Test]
    public void ProjectileImpactAppliesClassAndArmorDamageOnImpactTick()
    {
        SimulationWorld world = NewWorld();
        SpawnSource(world, FixVec2.FromInts(20, 20));
        EntityId target = SpawnTarget(world, FixVec2.FromInts(23, 20), CombatTargetClass.MediumMachine, 100, 2);
        SimulationRunner runner = new(world);

        runner.StepTicks(5);
        Assert.That(world.Entities.Health.Get(target).Current, Is.EqualTo(Fix32.FromInt(100)), "Travel must not deal early damage.");
        runner.StepOneTick();

        Fix32 expectedDamage = DamageSystem.CalculateDamage(6, DamageType.General, CombatTargetClass.MediumMachine, 2);
        Health health = world.Entities.Health.Get(target);
        Assert.Multiple(() =>
        {
            Assert.That(health.Current, Is.EqualTo(Fix32.FromInt(100) - expectedDamage));
            Assert.That(health.LastDamageTick, Is.EqualTo(6));
            Assert.That(world.ProjectileImpacts, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void OrderedSimultaneousHitsClampAtZeroAndDepletedTargetStopsCombat()
    {
        SimulationWorld world = NewWorld();
        EntityId sourceA = SpawnSource(world, FixVec2.FromInts(20, 20));
        SpawnSource(world, FixVec2.FromInts(20, 20));
        EntityId target = SpawnTarget(world, FixVec2.FromInts(23, 20), CombatTargetClass.MediumMachine, 10, 0);
        SimulationRunner runner = new(world);

        runner.StepTicks(6);
        Assert.That(world.Entities.Health.Get(target).Current, Is.EqualTo(Fix32.Zero));
        Assert.That(world.Entities.Targeting.Get(sourceA).CurrentTarget, Is.EqualTo(target), "Impact resolves after this tick's targeting pass.");

        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Targeting.Get(sourceA).CurrentTarget, Is.EqualTo(EntityId.None));
            Assert.That(world.Entities.Targeting.Get(sourceA).SelectionKind, Is.EqualTo(TargetSelectionKind.None));
        });
    }

    [Test]
    public void HealthStateRoundTripsAndHashesIdentically()
    {
        SimulationWorld world = NewWorld();
        EntityId target = SpawnTarget(world, FixVec2.FromInts(23, 20), CombatTargetClass.FortifiedStructure, 3000, 5);
        new SimulationRunner(world).StepTicks(12);
        ref Health health = ref world.Entities.Health.Get(target);
        health.Current -= Fix32.FromRatio(123, 2);
        health.LastDamageTick = 12;
        Fix32 expectedCurrent = health.Current;

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(restored.Entities.Health.Get(target).Current, Is.EqualTo(expectedCurrent));
            Assert.That(restored.Entities.Health.Get(target).Maximum, Is.EqualTo(Fix32.FromInt(3000)));
            Assert.That(restored.Entities.Health.Get(target).ArmorRating, Is.EqualTo(5));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    [TestCase("unit.rock_raiders.crew", 110, 0)]
    [TestCase("unit.rock_raiders.hover_scout", 120, 0)]
    [TestCase("unit.rock_raiders.rapid_rider", 170, 0)]
    [TestCase("unit.rock_raiders.loader_dozer", 360, 2)]
    [TestCase("unit.rock_raiders.chrome_crusher", 880, 5)]
    [TestCase("building.rock_raiders.hq", 3000, 5)]
    [TestCase("building.rock_raiders.ore_processing_plant", 1350, 2)]
    [TestCase("building.rock_raiders.power_station", 1000, 1)]
    [TestCase("building.rock_raiders.vehicle_service_bay", 1700, 3)]
    public void PrototypeContentCarriesCanonicalDurability(string key, int hitPoints, byte armor)
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        Assert.That(content.TryGetEntity(key, out PrototypeEntityDefinition definition), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(definition.Combat.MaximumHitPoints, Is.EqualTo(hitPoints));
            Assert.That(definition.Combat.ArmorRating, Is.EqualTo(armor));
        });
    }

    private static SimulationWorld NewWorld() => new(new MapGrid("map.test.m4.damage"), 2);

    private static EntityId SpawnSource(SimulationWorld world, FixVec2 position)
    {
        Assert.That(world.Content.TryGetWeapon(PulseWeapon, out WeaponDefinition weapon), Is.True);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.LightMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        world.Entities.Health.Set(id, NewHealth(120, 0));
        world.Entities.Targeting.Set(id, new Targeting
        {
            AcquisitionRadius = Fix32.FromInt(7), LegalLayers = weapon.LegalTargetLayers, LegalClasses = weapon.LegalTargetClasses,
            PriorityProfile = weapon.PriorityProfile, CurrentTarget = EntityId.None, SelectionKind = TargetSelectionKind.None
        });
        world.Entities.Weapon.Set(id, new WeaponState { WeaponProfile = weapon.Id, LastFiredTarget = EntityId.None, LastFiredTick = -1 });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 10, LastFogX = -1, LastFogY = -1 });
        return id;
    }

    private static EntityId SpawnTarget(SimulationWorld world, FixVec2 position, CombatTargetClass targetClass, int hitPoints, byte armor)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 1 });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = targetClass, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        world.Entities.Health.Set(id, NewHealth(hitPoints, armor));
        return id;
    }

    private static Health NewHealth(int hitPoints, byte armor)
    {
        Fix32 maximum = Fix32.FromInt(hitPoints);
        return new Health { Maximum = maximum, Current = maximum, ArmorRating = armor, LastDamageTick = -1 };
    }
}
