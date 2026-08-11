using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4WeaponTests
{
    [Test]
    public void CanonicalRockRaiderWeaponsUseExactTwentyHertzCooldowns()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        Assert.Multiple(() =>
        {
            AssertWeapon(content, "weapon.rr.crew.portable_mining_tool", 6, DamageType.General, 24, Fix32.FromRatio(4, 5), WeaponDeliveryKind.Contact);
            AssertWeapon(content, "weapon.rr.hover_scout.survey_pulse", 6, DamageType.General, 30, Fix32.FromInt(3), WeaponDeliveryKind.Projectile);
            AssertWeapon(content, "weapon.rr.loader_dozer.scoop_ram", 18, DamageType.General, 27, Fix32.FromRatio(9, 10), WeaponDeliveryKind.Contact);
            AssertWeapon(content, "weapon.rr.chrome_crusher.chrome_drill", 55, DamageType.Siege, 32, Fix32.FromRatio(21, 20), WeaponDeliveryKind.Contact);
            Assert.That(content.TryGetEntity("unit.rock_raiders.rapid_rider", out PrototypeEntityDefinition rider), Is.True);
            Assert.That(rider.Combat.WeaponProfile.Value, Is.Zero);
            Assert.That(content.TryGetWeapon("weapon.rr.hover_scout.survey_pulse", out WeaponDefinition pulse), Is.True);
            Assert.That(pulse.ProjectileSpeed, Is.EqualTo(Fix32.FromInt(12)));
            Assert.That(pulse.FacingToleranceAngle16, Is.EqualTo(Angle16.Half.Raw));
            Assert.That(pulse.MaximumMovingFireSpeedBasisPoints, Is.EqualTo(10_000));
            Assert.That(content.TryGetWeapon("weapon.rr.chrome_crusher.chrome_drill", out WeaponDefinition drill), Is.True);
            Assert.That(drill.FacingToleranceAngle16, Is.EqualTo(5461));
            Assert.That(drill.MaximumMovingFireSpeedBasisPoints, Is.EqualTo(3_500));
        });
    }

    [Test]
    public void ReadyWeaponFiresImmediatelyThenRepeatsOnExactCooldownTick()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmed(world, 0, FixVec2.FromInts(20, 20), "weapon.rr.hover_scout.survey_pulse");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(22, 20));
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        AssertFire(world.Entities.Weapon.Get(source), target, 1, 1, 30);

        runner.StepTicks(29);
        AssertFire(world.Entities.Weapon.Get(source), target, 1, 1, 1);

        runner.StepOneTick();
        AssertFire(world.Entities.Weapon.Get(source), target, 2, 31, 30);
    }

    [Test]
    public void CooldownAdvancesWithoutTargetAndReadyWeaponWaitsForRange()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmed(world, 0, FixVec2.FromInts(30, 30), "weapon.rr.hover_scout.survey_pulse");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(32, 30));
        SimulationRunner runner = new(world);
        runner.StepOneTick();

        world.Entities.Transform.Get(target).Position = FixVec2.FromInts(36, 30);
        runner.StepTicks(30);
        WeaponState ready = world.Entities.Weapon.Get(source);
        Assert.That(ready.FireSequence, Is.EqualTo(1));
        Assert.That(ready.CooldownRemainingTicks, Is.Zero);

        world.Entities.Transform.Get(target).Position = FixVec2.FromInts(32, 30);
        runner.StepOneTick();
        AssertFire(world.Entities.Weapon.Get(source), target, 2, 32, 30);
    }

    [Test]
    public void SharedVisionDoesNotPermitFireThroughAnOccludingWall()
    {
        MapGrid map = new("map.test.m4.weapon.los");
        map.SetFlagsRect(new IntRect(44, 40, 2, 2), MapCellFlags.GroundOccluder, MapCellFlags.None);
        SimulationWorld world = new(map, 2);
        EntityId source = SpawnArmed(world, 0, FixVec2.FromInts(20, 20), "weapon.rr.hover_scout.survey_pulse");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(23, 20));
        SpawnObserver(world, 0, FixVec2.FromInts(23, 21));
        ref Targeting targeting = ref world.Entities.Targeting.Get(source);
        targeting.CurrentTarget = target;
        targeting.SelectionKind = TargetSelectionKind.DirectOrder;

        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Fog.IsVisible(0, 23, 20), Is.True);
        Assert.That(world.Entities.Targeting.Get(source).CurrentTarget, Is.EqualTo(target));
        Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.Zero);
    }

    [Test]
    public void WeaponSnapshotAndPresentationEventContinueDeterministically()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmed(world, 0, FixVec2.FromInts(50, 50), "weapon.rr.hover_scout.survey_pulse", includePresentation: true);
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(52, 50));
        SimulationRunner a = new(world);
        a.StepOneTick();
        PresentationSnapshot presentation = PresentationSnapshot.Capture(world, 0);
        PresentationEntity sourcePresentation = presentation.Entities.Single(e => e.EntityId == source);
        Assert.That(sourcePresentation.WeaponFireSequence, Is.EqualTo(1));
        Assert.That(sourcePresentation.WeaponFireTarget, Is.EqualTo(target));
        Assert.That(sourcePresentation.WeaponDelivery, Is.EqualTo(WeaponDeliveryKind.Projectile));

        SimulationRunner b = new(SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world)));
        a.StepTicks(75); b.StepTicks(75);
        Assert.Multiple(() =>
        {
            Assert.That(b.World.Entities.Weapon.Get(source).FireSequence, Is.EqualTo(a.World.Entities.Weapon.Get(source).FireSequence));
            Assert.That(b.World.Entities.Weapon.Get(source).CooldownRemainingTicks, Is.EqualTo(a.World.Entities.Weapon.Get(source).CooldownRemainingTicks));
            Assert.That(StateHasher.Hash(b.World), Is.EqualTo(StateHasher.Hash(a.World)));
        });
    }

    private static SimulationWorld NewWorld() => new(new MapGrid("map.test.m4.weapon"), 2);

    private static EntityId SpawnArmed(SimulationWorld world, byte owner, FixVec2 position, string weaponKey, bool includePresentation = false)
    {
        Assert.That(world.Content.TryGetWeapon(weaponKey, out WeaponDefinition weapon), Is.True);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.LightMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        world.Entities.Targeting.Set(id, new Targeting
        {
            CurrentTarget = EntityId.None, AcquisitionRadius = weapon.Range + Fix32.FromInt(4), LegalLayers = weapon.LegalTargetLayers,
            LegalClasses = weapon.LegalTargetClasses, PriorityProfile = weapon.PriorityProfile, SelectionKind = TargetSelectionKind.None
        });
        world.Entities.Weapon.Set(id, new WeaponState { WeaponProfile = weapon.Id, LastFiredTarget = EntityId.None, LastFiredTick = -1 });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 10, LastFogX = -1, LastFogY = -1 });
        if (includePresentation)
        {
            world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = StableId.FromKey("unit.rock_raiders.hover_scout"), Kind = SelectableKind.CombatSupport });
            world.Entities.Movement.Set(id, new Movement { State = MovementState.Idle, LastPosition = position });
            world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = FootprintClass.Small, Layer = MovementLayer.GroundHover, Target = position });
        }
        return id;
    }

    private static EntityId SpawnTarget(SimulationWorld world, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.MediumMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        return id;
    }

    private static void SpawnObserver(SimulationWorld world, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 3, LastFogX = -1, LastFogY = -1 });
    }

    private static void AssertWeapon(PrototypeContentCatalog content, string key, ushort damage, DamageType damageType,
        ushort cooldown, Fix32 range, WeaponDeliveryKind delivery)
    {
        Assert.That(content.TryGetWeapon(key, out WeaponDefinition weapon), Is.True);
        Assert.That(weapon.BaseDamage, Is.EqualTo(damage));
        Assert.That(weapon.DamageType, Is.EqualTo(damageType));
        Assert.That(weapon.CooldownTicks, Is.EqualTo(cooldown));
        Assert.That(weapon.Range, Is.EqualTo(range));
        Assert.That(weapon.DeliveryKind, Is.EqualTo(delivery));
    }

    private static void AssertFire(WeaponState state, EntityId target, uint sequence, int tick, ushort cooldown)
    {
        Assert.Multiple(() =>
        {
            Assert.That(state.FireSequence, Is.EqualTo(sequence));
            Assert.That(state.LastFiredTarget, Is.EqualTo(target));
            Assert.That(state.LastFiredTick, Is.EqualTo(tick));
            Assert.That(state.CooldownRemainingTicks, Is.EqualTo(cooldown));
        });
    }
}
