using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4ContactWeaponTests
{
    [Test]
    public void ContactAttackersReserveDifferentDeterministicApproachSlots()
    {
        SimulationWorld world = NewWorld();
        EntityId first = SpawnArmedUnit(world, 0, FixVec2.FromInts(16, 20), "unit.rock_raiders.crew");
        EntityId second = SpawnArmedUnit(world, 0, FixVec2.FromInts(16, 22), "unit.rock_raiders.crew");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(21, 21));
        new VisionSystem().Step(world);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, first, target), Is.True);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, second, target), Is.True);

        new ContactApproachSystem().Step(world);

        Targeting firstTargeting = world.Entities.Targeting.Get(first), secondTargeting = world.Entities.Targeting.Get(second);
        Assert.Multiple(() =>
        {
            Assert.That(firstTargeting.HasApproachSlot, Is.True);
            Assert.That(secondTargeting.HasApproachSlot, Is.True);
            Assert.That(secondTargeting.ApproachSlotIndex, Is.Not.EqualTo(firstTargeting.ApproachSlotIndex));
            Assert.That(world.Entities.Navigation.Get(first).Target, Is.Not.EqualTo(world.Entities.Navigation.Get(second).Target));
        });
    }

    [Test]
    public void ContactFireRequiresFacingAndLowSpeedThenUsesCommonDamageResolver()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmedUnit(world, 0, FixVec2.FromInts(20, 20), "unit.rock_raiders.crew");
        EntityId target = SpawnTarget(world, 1, new FixVec2(Fix32.FromRatio(43, 2), Fix32.FromInt(20)));
        new VisionSystem().Step(world);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, source, target), Is.True);
        ref SimTransform sourceTransform = ref world.Entities.Transform.Get(source);
        sourceTransform.Orientation = Angle16.Quarter;

        new WeaponSystem().Step(world);
        Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.Zero, "A side-facing contact tool must not hit.");

        sourceTransform.Orientation = Angle16.Zero;
        ref Movement movement = ref world.Entities.Movement.Get(source);
        movement.CurrentSpeed = movement.MaxSpeed;
        new WeaponSystem().Step(world);
        Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.Zero, "Full-speed drive-through damage is forbidden.");

        movement.CurrentSpeed = Fix32.Zero;
        Fix32 before = world.Entities.Health.Get(target).Current;
        new WeaponSystem().Step(world); new ContactDamageSystem().Step(world);
        Fix32 expected = DamageSystem.CalculateDamage(6, DamageType.General, CombatTargetClass.MediumMachine, 2);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.EqualTo(1));
            Assert.That(world.Entities.Health.Get(target).Current, Is.EqualTo(before - expected));
        });
    }

    [Test]
    public void OutOfRangeDirectProjectileAttackPursuesThenFires()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmedUnit(world, 0, FixVec2.FromInts(12, 20), "unit.rock_raiders.hover_scout");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(20, 20));
        new VisionSystem().Step(world);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, source, target), Is.True);
        SimulationRunner runner = new(world);

        runner.StepTicks(100);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Transform.Get(source).Position.X, Is.GreaterThan(Fix32.FromInt(12)));
            Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.GreaterThan(0));
            Assert.That(FixVec2.Distance(world.Entities.Transform.Get(source).Position, world.Entities.Transform.Get(target).Position), Is.LessThanOrEqualTo(Fix32.FromInt(3)));
        });
    }

    [Test]
    public void ContactPursuitStateRoundTripsDeterministically()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnArmedUnit(world, 0, FixVec2.FromInts(14, 20), "unit.rock_raiders.loader_dozer");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(20, 20));
        new VisionSystem().Step(world);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, source, target), Is.True);
        new ContactApproachSystem().Step(world);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(restored.Entities.Targeting.Get(source).ApproachSlotIndex, Is.EqualTo(world.Entities.Targeting.Get(source).ApproachSlotIndex));
            Assert.That(restored.Entities.Targeting.Get(source).HasApproachSlot, Is.True);
            Assert.That(restored.Entities.Targeting.Get(source).PursuitOrigin, Is.EqualTo(world.Entities.Targeting.Get(source).PursuitOrigin));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    [Test]
    public void DirectPursuitAbandonsTargetAtCanonicalTwelveCellLeash()
    {
        SimulationWorld world = NewWorld();
        FixVec2 origin = FixVec2.FromInts(12, 40);
        EntityId source = SpawnArmedUnit(world, 0, origin, "unit.rock_raiders.hover_scout");
        EntityId target = SpawnTarget(world, 1, FixVec2.FromInts(32, 40));
        new VisionSystem().Step(world);
        Assert.That(TargetingSystem.TryIssueDirectOrder(world, 0, source, target), Is.True);

        new SimulationRunner(world).StepTicks(240);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Targeting.Get(source).SelectionKind, Is.EqualTo(TargetSelectionKind.None));
            Assert.That(world.Entities.Weapon.Get(source).FireSequence, Is.Zero);
            Assert.That(FixVec2.Distance(origin, world.Entities.Transform.Get(source).Position), Is.LessThanOrEqualTo(Fix32.FromRatio(121, 10)));
        });
    }

    private static SimulationWorld NewWorld() => new(new MapGrid("map.test.m4.contact"), 2);

    private static EntityId SpawnArmedUnit(SimulationWorld world, byte owner, FixVec2 position, string entityKey)
    {
        Assert.That(world.Content.TryGetEntity(entityKey, out PrototypeEntityDefinition definition), Is.True);
        Assert.That(world.Content.TryGetMovement(definition.MovementProfileKey, out PrototypeMovementProfile profile), Is.True);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Movement.Set(id, new Movement
        {
            MaxSpeed = profile.MaxSpeed, Acceleration = profile.Acceleration, Deceleration = profile.Deceleration,
            TurnRatePerTick = profile.TurnRatePerTick, ReversePolicy = profile.ReversePolicy, State = MovementState.Idle, LastPosition = position
        });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = definition.Footprint, Layer = profile.Layer, Target = position, PathTopologyVersion = world.Map.TopologyVersion });
        world.Entities.Targetable.Set(id, new Targetable { Class = definition.Combat.TargetClass, Layer = definition.Combat.TargetLayer, Flags = definition.Combat.TargetFlags });
        world.Entities.Health.Set(id, new Health { Maximum = Fix32.FromInt(definition.Combat.MaximumHitPoints), Current = Fix32.FromInt(definition.Combat.MaximumHitPoints), ArmorRating = definition.Combat.ArmorRating, LastDamageTick = -1 });
        world.Entities.Targeting.Set(id, new Targeting
        {
            AcquisitionRadius = definition.Combat.AcquisitionRadius, LegalLayers = definition.Combat.LegalTargetLayers,
            LegalClasses = definition.Combat.LegalTargetClasses, PriorityProfile = definition.Combat.PriorityProfile
        });
        world.Entities.Weapon.Set(id, new WeaponState { WeaponProfile = definition.Combat.WeaponProfile, LastFiredTarget = EntityId.None, LastFiredTick = -1 });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 20, LastFogX = -1, LastFogY = -1 });
        world.GetQueue(id);
        return id;
    }

    private static EntityId SpawnTarget(SimulationWorld world, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = FootprintClass.Small, Layer = MovementLayer.Ground, Target = position });
        world.Entities.Targetable.Set(id, new Targetable { Class = CombatTargetClass.MediumMachine, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.CombatThreat });
        world.Entities.Health.Set(id, new Health { Maximum = Fix32.FromInt(200), Current = Fix32.FromInt(200), ArmorRating = 2, LastDamageTick = -1 });
        return id;
    }
}
