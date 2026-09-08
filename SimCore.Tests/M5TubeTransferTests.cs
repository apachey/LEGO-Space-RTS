using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public class M5TubeTransferTests
{
    private static readonly ContentId HangarType = StableId.FromKey("building.mar.aero_tube_hangar");
    private static readonly ContentId StationType = StableId.FromKey("building.mar.settlement_station");
    private static readonly ContentId WorkerRobot = StableId.FromKey(CanonicalRosterReferences.MartianWorkerRobot);
    private static readonly ContentId DoubleHover = StableId.FromKey(CanonicalRosterReferences.MartianDoubleHover);
    private static readonly ContentId JetScooter = StableId.FromKey(CanonicalRosterReferences.MartianJetScooter);

    [Test]
    public void EligibilityAndCanonicalPhaseTimingAreExact()
    {
        SimulationWorld world = CreateWorld(); EntityId a = AddStation(world, HangarType, 10, 20), b = AddStation(world, StationType, 40, 20);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, b, out EntityId link), Is.True);
        EntityId worker = AddPassenger(world, WorkerRobot, link, a), hover = AddPassenger(world, DoubleHover, link, a), scooter = AddPassenger(world, JetScooter, link, a);
        EntityId heavy = AddPassenger(world, StableId.FromKey(CanonicalRosterReferences.MartianExcavationSearcher), link, a);
        Assert.Multiple(() =>
        {
            Assert.That(TubeTransferSystem.IsEligible(world, worker), Is.True);
            Assert.That(TubeTransferSystem.IsEligible(world, hover), Is.True);
            Assert.That(TubeTransferSystem.IsEligible(world, scooter), Is.True);
            Assert.That(TubeTransferSystem.IsEligible(world, heavy), Is.False);
            Assert.That(TubeTransferSystem.LoadingTicks, Is.EqualTo(60));
            Assert.That(TubeTransferSystem.UnloadingTicks, Is.EqualTo(40));
            Assert.That(TubeTransferSystem.ArrivalRecoveryTicks, Is.EqualTo(15));
            Assert.That(TubeTransferSystem.GetTravelTicks(1), Is.EqualTo(3));
            Assert.That(TubeTransferSystem.GetTravelTicks(10), Is.EqualTo(24));
        });
    }

    [Test]
    public void StationQueuesAtTwoChannelsAndUpgradeRaisesCapacityToThree()
    {
        SimulationWorld world = CreateWorld(); EntityId a = AddStation(world, HangarType, 10, 20), b = AddStation(world, StationType, 45, 20);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, b, out EntityId link), Is.True);
        EntityId[] units = { AddPassenger(world, WorkerRobot, link, a), AddPassenger(world, DoubleHover, link, a), AddPassenger(world, JetScooter, link, a) };
        for (int i = 0; i < units.Length; i++) Assert.That(TubeTransferSystem.TryQueueTransfer(world, 0, units[i], a, b), Is.True);
        SimulationRunner runner = new(world); runner.StepOneTick();
        Assert.That(CountState(world, TubeTransferState.Loading), Is.EqualTo(2));
        Assert.That(CountState(world, TubeTransferState.Queued), Is.EqualTo(1));

        Assert.That(TubeGraphSystem.TrySetHypersledThroughput(world, 0, a, true), Is.True);
        Assert.That(TubeGraphSystem.TrySetHypersledThroughput(world, 0, b, true), Is.True);
        runner.StepOneTick();
        Assert.That(CountState(world, TubeTransferState.Loading), Is.EqualTo(3));
    }

    [Test]
    public void PassengerKeepsIdentityWhileOffMapAndExecutesDeferredMoveAfterRecovery()
    {
        SimulationWorld world = CreateWorld(); EntityId a = AddStation(world, HangarType, 10, 20), b = AddStation(world, StationType, 40, 20);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, b, out EntityId link), Is.True);
        EntityId unit = AddPassenger(world, WorkerRobot, link, a); Assert.That(TubeTransferSystem.TryQueueTransfer(world, 0, unit, a, b), Is.True);
        SimulationRunner runner = new(world); runner.StepOneTick(); runner.StepTicks(TubeTransferSystem.LoadingTicks);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(unit), Is.True);
            Assert.That(world.Entities.Transform.Has(unit), Is.False);
            Assert.That(world.Entities.TubeTransfer.Get(unit).State, Is.EqualTo(TubeTransferState.Travelling));
            Assert.That(TubeTransferSystem.CanAttack(world, unit), Is.False);
        });
        FixVec2 arrivalOrder = new(Fix32.FromInt(75), Fix32.FromInt(70));
        Assert.That(TubeTransferSystem.CaptureArrivalMoveOrder(world, unit, arrivalOrder), Is.True);
        StepUntil(runner, () => !world.Entities.TubeTransfer.Has(unit), 1000);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(unit), Is.True);
            Assert.That(world.Entities.Transform.Has(unit), Is.True);
            Assert.That(world.Entities.Navigation.Get(unit).HasTarget, Is.True);
            Assert.That(world.Entities.Navigation.Get(unit).Target, Is.EqualTo(arrivalOrder));
        });
    }

    [Test]
    public void BrokenRouteReturnsPassengerSafelyAfterSixSeconds()
    {
        SimulationWorld world = CreateWorld(); EntityId a = AddStation(world, HangarType, 10, 20), b = AddStation(world, StationType, 42, 20);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, b, out EntityId link), Is.True);
        EntityId unit = AddPassenger(world, JetScooter, link, a); Assert.That(TubeTransferSystem.TryQueueTransfer(world, 0, unit, a, b), Is.True);
        SimulationRunner runner = new(world); runner.StepOneTick(); runner.StepTicks(TubeTransferSystem.LoadingTicks);
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, link, false), Is.True); runner.StepOneTick();
        Assert.That(world.Entities.TubeTransfer.Get(unit).State, Is.EqualTo(TubeTransferState.Returning));
        Assert.That(world.Entities.TubeTransfer.Get(unit).RemainingTicks, Is.EqualTo(TubeTransferSystem.BrokenRouteReturnTicks));
        runner.StepTicks(TubeTransferSystem.BrokenRouteReturnTicks);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(unit), Is.True);
            Assert.That(world.Entities.Transform.Has(unit), Is.True);
            Assert.That(world.Entities.TubeTransfer.Get(unit).State, Is.EqualTo(TubeTransferState.ArrivalRecovery));
        });
    }

    [Test]
    public void RedundantRoutingSelectsTheStableAlternatePath()
    {
        SimulationWorld world = CreateWorld();
        EntityId a = AddStation(world, HangarType, 10, 25), upper = AddStation(world, StationType, 40, 8);
        EntityId lower = AddStation(world, StationType, 40, 42), destination = AddStation(world, HangarType, 72, 25);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, upper, out EntityId first), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, upper, destination, out _), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, lower, out _), Is.True);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, lower, destination, out _), Is.True);
        Assert.That(TubeGraphSystem.TrySetRedundantRouting(world, 0, a, true), Is.True);
        EntityId unit = AddPassenger(world, WorkerRobot, first, a); Assert.That(TubeTransferSystem.TryQueueTransfer(world, 0, unit, a, destination), Is.True);
        SimulationRunner runner = new(world); runner.StepOneTick(); runner.StepTicks(TubeTransferSystem.LoadingTicks);
        Assert.That(TubeGraphSystem.TrySetLinkOperational(world, 0, first, false), Is.True); runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.TubeTransfer.Get(unit).State, Is.EqualTo(TubeTransferState.Travelling));
            Assert.That(world.Entities.TubeTransfer.Get(unit).CurrentEdgeIndex, Is.Zero);
            Assert.That(world.Entities.TubeTransfer.Get(unit).RemainingTicks, Is.GreaterThan(0));
        });
        StepUntil(runner, () => !world.Entities.TubeTransfer.Has(unit), 1200);
        Assert.That(world.Entities.Transform.Has(unit), Is.True);
    }

    [Test]
    public void SnapshotPreservesMidTransitRouteAndDeterministicContinuation()
    {
        SimulationWorld world = CreateWorld(); EntityId a = AddStation(world, HangarType, 10, 20), b = AddStation(world, StationType, 50, 20);
        Assert.That(TubeGraphSystem.TryAddCompletedLink(world, 0, a, b, out EntityId link), Is.True);
        EntityId unit = AddPassenger(world, DoubleHover, link, a); Assert.That(TubeTransferSystem.TryQueueTransfer(world, 0, unit, a, b), Is.True);
        SimulationRunner runner = new(world); runner.StepOneTick(); runner.StepTicks(TubeTransferSystem.LoadingTicks + 7);
        FixVec2 target = new(Fix32.FromInt(80), Fix32.FromInt(80)); Assert.That(TubeTransferSystem.CaptureArrivalMoveOrder(world, unit, target), Is.True);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        SimulationRunner restoredRunner = new(restored); runner.StepTicks(200); restoredRunner.StepTicks(200);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static SimulationWorld CreateWorld() => new(new MapGrid("map.test.m5_tube_transit"), 2);

    private static EntityId AddStation(SimulationWorld world, ContentId type, short x, short y)
    {
        byte size = type == HangarType ? (byte)9 : (byte)7; EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(id, new SimTransform { Position = new FixVec2(Fix32.FromRatio(x * 2 + size, 2), Fix32.FromRatio(y * 2 + size, 2)), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, AnchorX = x, AnchorY = y, FootprintWidth = size, FootprintHeight = size, State = BuildingState.Completed });
        Assert.That(TubeGraphSystem.TryRegisterStation(world, id), Is.True); return id;
    }

    private static EntityId AddPassenger(SimulationWorld world, ContentId type, EntityId link, EntityId origin)
    {
        Assert.That(TubeGraphSystem.TryGetStationSocket(world, link, origin, out FixVec2 socket), Is.True); EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 0 }); world.Entities.Transform.Set(id, new SimTransform { Position = socket, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = type == WorkerRobot ? SelectableKind.Worker : SelectableKind.CombatSupport });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = FootprintClass.Tiny, Layer = MovementLayer.Ground });
        world.Entities.Movement.Set(id, new Movement { MaxSpeed = Fix32.FromInt(4), Acceleration = Fix32.FromInt(4), Deceleration = Fix32.FromInt(4), TurnRatePerTick = 2048 });
        return id;
    }

    private static int CountState(SimulationWorld world, TubeTransferState state)
    {
        int count = 0; foreach (EntityId id in world.Entities.Alive) if (world.Entities.TubeTransfer.TryGet(id, out TubeTransfer transfer) && transfer.State == state) count++; return count;
    }

    private static void StepUntil(SimulationRunner runner, System.Func<bool> condition, int limit)
    {
        for (int i = 0; i < limit && !condition(); i++) runner.StepOneTick(); Assert.That(condition(), Is.True, "Tube transfer did not reach the expected state.");
    }
}
}
