using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4TransformationTests
{
    [Test]
    public void Mx41CompletesCanonicalGroundToFlightTransitionWithoutChangingEntityId()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        SimulationRunner runner = new(world);
        uint identity = fighter.Value;
        IssueStateChange(world, fighter, 10);

        runner.StepTicks(TransformationTicks - 1);
        Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.Transitioning));
        runner.StepOneTick();

        Transformation state = world.Entities.Transformation.Get(fighter);
        Assert.Multiple(() =>
        {
            Assert.That(fighter.Value, Is.EqualTo(identity));
            Assert.That(world.Entities.Exists(fighter), Is.True);
            Assert.That(state.Phase, Is.EqualTo(TransformationPhase.Idle));
            Assert.That(state.CurrentState, Is.EqualTo(StableId.FromKey("state.astronauts.mx41.flight")));
            Assert.That(world.Entities.Navigation.Get(fighter).Layer, Is.EqualTo(MovementLayer.TrueAir));
            Assert.That(world.Entities.Movement.Get(fighter).MaxSpeed, Is.EqualTo(Fix32.FromRatio(245, 100)));
            Assert.That(world.Entities.Targetable.Get(fighter).Layer, Is.EqualTo(CombatTargetLayer.TrueAir));
            Assert.That(world.Entities.Weapon.Get(fighter).WeaponProfile, Is.EqualTo(StableId.FromKey("weapon.ast.mx41.flight_pulse")));
        });
    }

    [Test]
    public void TransitionStopsMovementAndIsVulnerableToGroundAndAirWeapons()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        FixVec2 start = world.Entities.Transform.Get(fighter).Position;
        SimulationRunner runner = new(world);
        IssueStateChange(world, fighter, 11);
        runner.StepOneTick();

        EntityId groundAttacker = SpawnLayerAttacker(world, 1, start + FixVec2.FromInts(2, 0), TargetLayerMask.Ground);
        EntityId airAttacker = SpawnLayerAttacker(world, 1, start + FixVec2.FromInts(-2, 0), TargetLayerMask.TrueAir);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 12, SimCommandType.Move, new[] { fighter }, start + FixVec2.FromInts(8, 0)));
        runner.StepTicks(8);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Transform.Get(fighter).Position, Is.EqualTo(start));
            Assert.That(world.Entities.Movement.Get(fighter).CurrentSpeed, Is.EqualTo(Fix32.Zero));
            Assert.That(TransformationSystem.CanAttack(world, fighter), Is.False);
            Assert.That(TargetingSystem.IsLegalTarget(world, groundAttacker, fighter, requireVisible: false), Is.True);
            Assert.That(TargetingSystem.IsLegalTarget(world, airAttacker, fighter, requireVisible: false), Is.True);
        });
    }

    [Test]
    public void StopBeforeFortyPercentRollsBackForCanonicalPointSixSeconds()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        SimulationRunner runner = new(world);
        IssueStateChange(world, fighter, 13);
        runner.StepTicks(10);
        ushort progressBeforeCancel = TransformationSystem.ProgressBasisPoints(world.Entities.Transformation.Get(fighter));
        Assert.That(progressBeforeCancel, Is.LessThan(4_000));
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 14, SimCommandType.Stop, new[] { fighter }, FixVec2.Zero));
        runner.StepOneTick();
        Transformation rollingBack = world.Entities.Transformation.Get(fighter);
        Assert.Multiple(() =>
        {
            Assert.That(rollingBack.Phase, Is.EqualTo(TransformationPhase.RollingBack));
            Assert.That(TransformationSystem.ProgressBasisPoints(rollingBack), Is.GreaterThan(0));
            Assert.That(TransformationSystem.ProgressBasisPoints(rollingBack), Is.LessThan(progressBeforeCancel));
        });

        runner.StepTicks(10);
        Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.RollingBack));
        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.Idle));
            Assert.That(world.Entities.Transformation.Get(fighter).CurrentState, Is.EqualTo(StableId.FromKey("state.astronauts.mx41.ground")));
            Assert.That(world.Entities.Navigation.Get(fighter).Layer, Is.EqualTo(MovementLayer.Ground));
        });
    }

    [Test]
    public void StateChangeAtFortyPercentCommitsAndQueuesReverseUntilEightSecondLockExpires()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        SimulationRunner runner = new(world);
        IssueStateChange(world, fighter, 15);
        runner.StepTicks(18);
        Assert.That(TransformationSystem.ProgressBasisPoints(world.Entities.Transformation.Get(fighter)), Is.EqualTo(4_000));
        IssueStateChange(world, fighter, 16);
        runner.StepOneTick();
        Assert.That(world.Entities.Transformation.Get(fighter).QueuedToggle, Is.True);
        runner.StepTicks(TransformationTicks - 19);
        Transformation completed = world.Entities.Transformation.Get(fighter);
        Assert.That(completed.CurrentState, Is.EqualTo(StableId.FromKey("state.astronauts.mx41.flight")));
        int lockedUntil = completed.ReversalLockedUntilTick;

        runner.StepTicks(TransformationSystemLockTicks - 1);
        Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.Idle));
        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Tick.Value, Is.EqualTo(lockedUntil));
            Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.Transitioning));
            Assert.That(world.Entities.Transformation.Get(fighter).DestinationState, Is.EqualTo(StableId.FromKey("state.astronauts.mx41.ground")));
        });
    }

    [Test]
    public void FlightCannotBeginLandingOnOccupiedGround()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        SimulationRunner runner = new(world);
        IssueStateChange(world, fighter, 17);
        runner.StepTicks(TransformationTicks);
        runner.StepTicks(TransformationSystemLockTicks);
        FixVec2 position = world.Entities.Transform.Get(fighter).Position;
        SpawnGroundBlocker(world, 0, position);

        Assert.That(TransformationSystem.TryToggle(world, fighter), Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Transformation.Get(fighter).Phase, Is.EqualTo(TransformationPhase.Idle));
            Assert.That(world.Entities.Navigation.Get(fighter).Layer, Is.EqualTo(MovementLayer.TrueAir));
        });
    }

    [Test]
    public void MidTransformationSnapshotRoundTripsAndContinuesDeterministically()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        SimulationRunner runner = new(world);
        IssueStateChange(world, fighter, 18);
        runner.StepTicks(23);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world), world.Content);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));

        runner.StepTicks(220);
        new SimulationRunner(restored).StepTicks(220);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    [Test]
    public void PreparedTransformationFixtureProvidesSelectedReadyGroundModeUnit()
    {
        SimulationWorld world = Prepared(out EntityId fighter);
        TransformationDefinition definition = world.Content.Transformations.Single();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Selectable.Get(fighter).ContentType, Is.EqualTo(definition.EntityType));
            Assert.That(world.Entities.Transformation.Get(fighter).CurrentState, Is.EqualTo(definition.ModeA.StateId));
            Assert.That(world.Entities.Navigation.Get(fighter).Layer, Is.EqualTo(MovementLayer.Ground));
            Assert.That(world.Entities.Health.Get(fighter).Current, Is.EqualTo(Fix32.FromInt(320)));
        });
    }

    private const int TransformationTicks = 45;
    private const int TransformationSystemLockTicks = 160;

    private static SimulationWorld Prepared(out EntityId fighter)
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.DebugPrepareTransformationTest, System.Array.Empty<EntityId>(), FixVec2.Zero));
        new SimulationRunner(world).StepOneTick();
        ContentId type = StableId.FromKey(DebugPlaytestScenario.Mx41Key);
        fighter = world.Entities.Alive.Single(id => world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type &&
            world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0);
        return world;
    }

    private static void IssueStateChange(SimulationWorld world, EntityId fighter, uint sequence)
        => world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, sequence, SimCommandType.StateChange, new[] { fighter }, FixVec2.Zero));

    private static EntityId SpawnLayerAttacker(SimulationWorld world, byte owner, FixVec2 position, TargetLayerMask legalLayers)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targeting.Set(id, new Targeting
        {
            AcquisitionRadius = Fix32.FromInt(10), LegalLayers = legalLayers, LegalClasses = TargetClassMask.All,
            PriorityProfile = TargetPriorityProfile.Generalist, SelectionKind = TargetSelectionKind.None
        });
        return id;
    }

    private static EntityId SpawnGroundBlocker(SimulationWorld world, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = FootprintClass.Tiny, Layer = MovementLayer.Ground, Target = position });
        world.Entities.Movement.Set(id, new Movement { MaxSpeed = Fix32.One, LastPosition = position });
        return id;
    }
}
