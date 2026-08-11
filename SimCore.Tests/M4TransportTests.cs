using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4TransportTests
{
    [Test]
    public void RapidRiderLoadsCrewAfterCanonicalDockAndPassengerTimes()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        Place(world, crews[0], world.Entities.Transform.Get(rider).Position + new FixVec2(Fix32.One, Fix32.Zero));
        SimulationRunner runner = new(world);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 10, SimCommandType.Load,
            new[] { crews[0] }, FixVec2.Zero, targetEntity: rider));
        runner.StepOneTick();

        runner.StepTicks(TransportSystem.DockingTicks + TransportSystem.RapidRiderLoadTicks - 1);
        Assert.That(world.Entities.Passenger.Get(crews[0]).State, Is.Not.EqualTo(PassengerState.Loaded));
        runner.StepOneTick();

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Passenger.Get(crews[0]).State, Is.EqualTo(PassengerState.Loaded));
            Assert.That(world.Entities.Passenger.Get(crews[0]).Transport, Is.EqualTo(rider));
            Assert.That(world.Entities.Transform.Has(crews[0]), Is.False, "Loaded passengers leave ground/spatial queries.");
            Assert.That(world.Entities.Transport.Get(rider).PassengerCount, Is.EqualTo(1));
            Assert.That(world.Entities.Transport.Get(rider).OccupiedPoints, Is.EqualTo(1));
        });
    }

    [Test]
    public void RapidRiderCapacityIsFourPersonnelAndExtraCrewStayGrounded()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        SimulationRunner runner = new(world);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 11, SimCommandType.Load,
            crews, FixVec2.Zero, targetEntity: rider));
        runner.StepTicks(220);

        Transport transport = world.Entities.Transport.Get(rider);
        int loaded = crews.Count(id => world.Entities.Passenger.Get(id).State == PassengerState.Loaded);
        Assert.Multiple(() =>
        {
            Assert.That(transport.CapacityPoints, Is.EqualTo(4));
            Assert.That(transport.OccupiedPoints, Is.EqualTo(4));
            Assert.That(transport.PassengerCount, Is.EqualTo(4));
            Assert.That(loaded, Is.EqualTo(4));
            Assert.That(crews.Count(id => world.Entities.Passenger.Get(id).State == PassengerState.Grounded), Is.GreaterThanOrEqualTo(2));
        });
    }

    [Test]
    public void DirectDamagePausesLoadingForCanonicalPointSevenFiveSeconds()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        Place(world, crews[0], world.Entities.Transform.Get(rider).Position + new FixVec2(Fix32.One, Fix32.Zero));
        SimulationRunner runner = new(world);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 14, SimCommandType.Load,
            new[] { crews[0] }, FixVec2.Zero, targetEntity: rider));
        runner.StepOneTick();
        Assert.That(world.Entities.Transport.Get(rider).PhaseTicks, Is.EqualTo(TransportSystem.DockingTicks));
        ref Health passengerHealth = ref world.Entities.Health.Get(crews[0]);
        passengerHealth.LastDamageTick = world.Tick.Value;

        runner.StepTicks(TransportSystem.DamagePauseTicks);
        Assert.That(world.Entities.Transport.Get(rider).PhaseTicks, Is.EqualTo(TransportSystem.DockingTicks));
        runner.StepOneTick();
        Assert.That(world.Entities.Transport.Get(rider).PhaseTicks, Is.EqualTo(TransportSystem.DockingTicks - 1));
    }

    [Test]
    public void UnloadRestoresEveryPassengerAtReservedLegalNonOverlappingGround()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        SimulationRunner runner = new(world);
        LoadFour(world, runner, rider, crews);
        FixVec2 unload = world.Entities.Transform.Get(rider).Position;
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 12, SimCommandType.Unload,
            new[] { rider }, unload));
        runner.StepTicks(120);

        Assert.That(world.Entities.Transport.Get(rider).PassengerCount, Is.Zero);
        for (int i = 0; i < 4; i++)
        {
            Assert.That(world.Entities.Passenger.Get(crews[i]).State, Is.EqualTo(PassengerState.Grounded));
            Assert.That(world.Entities.Transform.Has(crews[i]), Is.True);
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(world.Entities.Transform.Get(crews[i]).Position), FootprintClass.Tiny), Is.True);
            Assert.That(FixVec2.Distance(world.Entities.Transform.Get(crews[i]).Position, world.Entities.Transform.Get(rider).Position),
                Is.GreaterThanOrEqualTo(FootprintRules.CollisionRadiusBuild(FootprintClass.Tiny) + FootprintRules.CollisionRadiusBuild(FootprintClass.Small)));
            for (int j = 0; j < i; j++)
                Assert.That(FixVec2.Distance(world.Entities.Transform.Get(crews[i]).Position, world.Entities.Transform.Get(crews[j]).Position),
                    Is.GreaterThanOrEqualTo(FootprintRules.CollisionRadiusBuild(FootprintClass.Tiny) * Fix32.FromInt(2)));
        }
    }

    [Test]
    public void DestroyedTransportEmergencyDeploysPassengersAtFortyPercentWithRecoveryPenalties()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        SimulationRunner runner = new(world);
        LoadFour(world, runner, rider, crews);
        ref Health riderHealth = ref world.Entities.Health.Get(rider);
        riderHealth.Current = Fix32.Zero;
        riderHealth.LastDamageTick = world.Tick.Value;
        runner.StepOneTick();
        int destructionTick = world.Tick.Value;

        Assert.That(world.Entities.Destruction.Has(rider), Is.True);
        for (int i = 0; i < 4; i++)
        {
            Passenger passenger = world.Entities.Passenger.Get(crews[i]);
            Assert.Multiple(() =>
            {
                Assert.That(passenger.State, Is.EqualTo(PassengerState.Grounded));
                Assert.That(passenger.Transport, Is.EqualTo(EntityId.None));
                Assert.That(world.Entities.Transform.Has(crews[i]), Is.True);
                Assert.That(world.Entities.Health.Get(crews[i]).Current, Is.EqualTo(Fix32.FromInt(44)));
                Assert.That(passenger.AttackLockedUntilTick, Is.EqualTo(destructionTick + TransportSystem.EmergencyAttackLockTicks));
                Assert.That(passenger.MovementPenaltyUntilTick, Is.EqualTo(destructionTick + TransportSystem.EmergencyMovementPenaltyTicks));
                Assert.That(TransportSystem.IsAttackLocked(world, crews[i]), Is.True);
                Assert.That(TransportSystem.MovementMultiplier(world, crews[i]), Is.EqualTo(Fix32.FromRatio(70, 100)));
            });
        }
    }

    [Test]
    public void LoadedPassengersAndTransportRoundTripWithDeterministicContinuation()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        SimulationRunner runner = new(world);
        LoadFour(world, runner, rider, crews);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world), world.Content);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));

        FixVec2 target = world.Entities.Transform.Get(rider).Position + FixVec2.FromInts(3, 0);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 13, SimCommandType.Unload, new[] { rider }, target));
        restored.Commands.Enqueue(new CommandEnvelope(restored.Tick.Next(), 0, 13, SimCommandType.Unload, new[] { rider }, target));
        runner.StepTicks(180);
        new SimulationRunner(restored).StepTicks(180);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    [Test]
    public void PreparedTransportArenaProvidesRiderAndAtLeastFourReadyCrew()
    {
        SimulationWorld world = Prepared(out EntityId rider, out EntityId[] crews);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Transport.Get(rider).CapacityPoints, Is.EqualTo(4));
            Assert.That(crews.Length, Is.GreaterThanOrEqualTo(4));
            Assert.That(crews.Take(4).All(id => world.Entities.Transform.Has(id) && world.Entities.Passenger.Get(id).State == PassengerState.Grounded), Is.True);
        });
    }

    private static SimulationWorld Prepared(out EntityId rider, out EntityId[] crews)
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.DebugPrepareTransportTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));
        new SimulationRunner(world).StepOneTick();
        rider = Find(world, DebugPlaytestScenario.RapidRiderKey);
        crews = world.Entities.Alive.Where(id => world.Entities.Passenger.Has(id) &&
            world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0).ToArray();
        return world;
    }

    private static void LoadFour(SimulationWorld world, SimulationRunner runner, EntityId rider, EntityId[] crews)
    {
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 20, SimCommandType.Load,
            crews.Take(4).ToArray(), FixVec2.Zero, targetEntity: rider));
        runner.StepTicks(220);
        Assert.That(world.Entities.Transport.Get(rider).PassengerCount, Is.EqualTo(4));
    }

    private static EntityId Find(SimulationWorld world, string key)
    {
        ContentId type = StableId.FromKey(key);
        return world.Entities.Alive.First(id => world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type &&
            world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0);
    }

    private static void Place(SimulationWorld world, EntityId id, FixVec2 position)
    {
        ref SimTransform transform = ref world.Entities.Transform.Get(id); transform.Position = position;
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(id); navigation.Target = position; navigation.HasTarget = false; navigation.PathDirty = false;
        ref Movement movement = ref world.Entities.Movement.Get(id); movement.CurrentSpeed = Fix32.Zero; movement.CurrentVelocity = FixVec2.Zero;
        movement.DesiredMovement = FixVec2.Zero; movement.LastPosition = position; movement.State = MovementState.Idle;
        world.Spatial.Rebuild(world.Entities);
    }
}
