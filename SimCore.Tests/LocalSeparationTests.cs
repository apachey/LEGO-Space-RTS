using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class LocalSeparationTests
{
    private static readonly Fix32 CollisionTolerance = Fix32.FromRatio(1, 100);

    [Test]
    public void EqualPriorityYieldRepeatsExactlyAcrossRuns()
    {
        SimulationRunner first = CreateHeadOnPair(FootprintClass.Small, FootprintClass.Small, 0, 0);
        SimulationRunner repeat = CreateHeadOnPair(FootprintClass.Small, FootprintClass.Small, 0, 0);

        for (int tick = 0; tick < 240; tick++)
        {
            first.StepOneTick();
            repeat.StepOneTick();
            Assert.That(StateHasher.Hash(repeat.World), Is.EqualTo(StateHasher.Hash(first.World)), $"Local yield diverged at tick {tick + 1}.");
        }
    }

    [Test]
    public void HeavyUnitKeepsStraighterLineThanLowerIdSmallUnit()
    {
        SimulationRunner runner = CreateHeadOnPair(FootprintClass.Small, FootprintClass.Large, 0, 0);
        EntityId small = runner.World.Entities.Alive[0];
        EntityId heavy = runner.World.Entities.Alive[1];
        Fix32 laneY = Fix32.FromInt(40);
        Fix32 smallMaxDeviation = Fix32.Zero;
        Fix32 heavyMaxDeviation = Fix32.Zero;

        for (int tick = 0; tick < 300; tick++)
        {
            runner.StepOneTick();
            smallMaxDeviation = Fix32.Max(smallMaxDeviation, Fix32.Abs(runner.World.Entities.Transform.Get(small).Position.Y - laneY));
            heavyMaxDeviation = Fix32.Max(heavyMaxDeviation, Fix32.Abs(runner.World.Entities.Transform.Get(heavy).Position.Y - laneY));
        }

        Assert.That(smallMaxDeviation.Raw, Is.GreaterThan(heavyMaxDeviation.Raw + Fix32.FromRatio(1, 20).Raw),
            "The lower-priority Small mover should yield laterally while the higher-ID Heavy mover holds the straighter line.");
    }

    [Test]
    public void EnemyUnitsNeverUseFriendlyCompression()
    {
        SimulationRunner runner = CreateHeadOnPair(FootprintClass.Small, FootprintClass.Small, 0, 1);
        EntityId a = runner.World.Entities.Alive[0];
        EntityId b = runner.World.Entities.Alive[1];
        Fix32 nominal = CollisionRadiusSum(runner.World, a, b);

        for (int tick = 0; tick < 300; tick++)
        {
            runner.StepOneTick();
            Fix32 distance = FixVec2.Distance(runner.World.Entities.Transform.Get(a).Position, runner.World.Entities.Transform.Get(b).Position);
            Assert.That(distance.Raw, Is.GreaterThanOrEqualTo(nominal.Raw - CollisionTolerance.Raw), $"Enemy units phased at tick {tick + 1}.");
            Assert.That(runner.World.Entities.Movement.Get(a).CompressionTicks, Is.EqualTo(0));
            Assert.That(runner.World.Entities.Movement.Get(b).CompressionTicks, Is.EqualTo(0));
        }
    }

    [Test]
    public void FriendlyCompressionClockDoesNotResetDuringContinuousContact()
    {
        SimulationWorld world = new(new MapGrid("map.test.local-separation.compression"), 2);
        EntityId a = SpawnMover(world, 0, FootprintClass.Small, FixVec2.FromInts(40, 40));
        EntityId b = SpawnMover(world, 0, FootprintClass.Small, new FixVec2(Fix32.FromRatio(4099, 100), Fix32.FromInt(40)));
        ref Movement aMove = ref world.Entities.Movement.Get(a);
        ref Movement bMove = ref world.Entities.Movement.Get(b);
        aMove.CompressionTicks = 29;
        bMove.CompressionTicks = 29;
        world.Spatial.Rebuild(world.Entities);
        QueueMove(world, a, 1, FixVec2.FromInts(60, 40));
        QueueMove(world, b, 2, FixVec2.FromInts(20, 40));
        SimulationRunner runner = new(world);
        Fix32 nominal = CollisionRadiusSum(world, a, b);
        Fix32 minimum = nominal * Fix32.FromRatio(85, 100);
        bool previousContact = true;
        int previousATicks = 29;
        int previousBTicks = 29;

        for (int tick = 0; tick < 40; tick++)
        {
            runner.StepOneTick();
            Fix32 distance = FixVec2.Distance(world.Entities.Transform.Get(a).Position, world.Entities.Transform.Get(b).Position);
            Assert.That(distance.Raw, Is.GreaterThanOrEqualTo(minimum.Raw - CollisionTolerance.Raw), $"Friendly compression exceeded 15% at tick {tick + 1}.");
            int aTicks = world.Entities.Movement.Get(a).CompressionTicks;
            int bTicks = world.Entities.Movement.Get(b).CompressionTicks;
            bool contact = distance < nominal;
            if (contact && previousContact)
            {
                Assert.That(aTicks, Is.GreaterThanOrEqualTo(previousATicks), $"Continuous friendly contact reset mover A's compression clock at tick {tick + 1}.");
                Assert.That(bTicks, Is.GreaterThanOrEqualTo(previousBTicks), $"Continuous friendly contact reset mover B's compression clock at tick {tick + 1}.");
            }
            Assert.That(aTicks, Is.LessThanOrEqualTo(30));
            Assert.That(bTicks, Is.LessThanOrEqualTo(30));
            previousContact = contact;
            previousATicks = aTicks;
            previousBTicks = bTicks;
        }
    }

    [Test]
    public void ExpiredFriendlyCompressionNeverMovesThePairCloser()
    {
        SimulationWorld world=new(new MapGrid("map.test.local-separation.recovery"),2);
        EntityId a=SpawnMover(world,0,FootprintClass.Small,FixVec2.FromInts(40,40));
        EntityId b=SpawnMover(world,0,FootprintClass.Small,new FixVec2(Fix32.FromRatio(4099,100),Fix32.FromInt(40)));
        world.Entities.Movement.Get(a).CompressionTicks=30;world.Entities.Movement.Get(b).CompressionTicks=30;
        world.Spatial.Rebuild(world.Entities);QueueMove(world,a,1,FixVec2.FromInts(60,40));QueueMove(world,b,2,FixVec2.FromInts(20,40));
        SimulationRunner runner=new(world);Fix32 nominal=CollisionRadiusSum(world,a,b);
        Fix32 previous=FixVec2.Distance(world.Entities.Transform.Get(a).Position,world.Entities.Transform.Get(b).Position);

        for(int tick=0;tick<40&&previous<nominal;tick++)
        {
            runner.StepOneTick();Fix32 current=FixVec2.Distance(world.Entities.Transform.Get(a).Position,world.Entities.Transform.Get(b).Position);
            Assert.That(current.Raw,Is.GreaterThanOrEqualTo(previous.Raw),$"Compressed pair moved closer at tick {tick+1}.");
            previous=current;
        }
    }

    private static SimulationRunner CreateHeadOnPair(FootprintClass firstFootprint, FootprintClass secondFootprint, byte firstPlayer, byte secondPlayer)
    {
        SimulationWorld world = new(new MapGrid("map.test.local-separation.head-on"), 2);
        EntityId first = SpawnMover(world, firstPlayer, firstFootprint, FixVec2.FromInts(30, 40));
        EntityId second = SpawnMover(world, secondPlayer, secondFootprint, FixVec2.FromInts(50, 40));
        world.Spatial.Rebuild(world.Entities);
        QueueMove(world, first, 1, FixVec2.FromInts(70, 40));
        QueueMove(world, second, 2, FixVec2.FromInts(10, 40));
        return new SimulationRunner(world);
    }

    private static EntityId SpawnMover(SimulationWorld world, byte player, FootprintClass footprint, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Movement.Set(id, new Movement
        {
            MaxSpeed = Fix32.FromInt(4),
            Acceleration = Fix32.FromInt(8),
            Deceleration = Fix32.FromInt(8),
            TurnRatePerTick = 4096,
            ReversePolicy = ReversePolicy.None,
            State = MovementState.Idle,
            LastPosition = position
        });
        world.Entities.Navigation.Set(id, new NavigationAgent
        {
            Footprint = footprint,
            Layer = MovementLayer.Ground,
            Target = position,
            PathTopologyVersion = world.Map.TopologyVersion
        });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, Kind = SelectableKind.CombatSupport });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 8, LastFogX = -1, LastFogY = -1 });
        world.GetQueue(id);
        return id;
    }

    private static void QueueMove(SimulationWorld world, EntityId id, uint sequence, FixVec2 target)
        => world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), world.Entities.Ownership.Get(id).PlayerSlot, sequence, SimCommandType.Move, new[] { id }, target));

    private static Fix32 CollisionRadiusSum(SimulationWorld world, EntityId a, EntityId b)
        => FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(a).Footprint)
         + FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(b).Footprint);
}
