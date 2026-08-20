using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class StressTests
{
    [Test]
    public void SixtyMoverFixtureStartsPassableAndNonOverlapping()
    {
        SimulationWorld world=ScenarioFactory.CreateStress60();EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent navigation=world.Entities.Navigation.Get(ids[i]);SimTransform transform=world.Entities.Transform.Get(ids[i]);
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(transform.Position),navigation.Footprint),Is.True,$"Stress fixture starts {ids[i]} on impassable terrain.");
            for(int j=i+1;j<ids.Length;j++)
            {
                NavigationAgent otherNavigation=world.Entities.Navigation.Get(ids[j]);SimTransform otherTransform=world.Entities.Transform.Get(ids[j]);
                Fix32 minimum=FootprintRules.CollisionRadiusBuild(navigation.Footprint)+FootprintRules.CollisionRadiusBuild(otherNavigation.Footprint);
                Assert.That(FixVec2.Distance(transform.Position,otherTransform.Position).Raw,Is.GreaterThanOrEqualTo(minimum.Raw),$"Stress fixture starts overlap: {ids[i]}/{ids[j]}.");
            }
        }
    }

    [Test]
    public void SixtyMoverFormationTargetsAreUniqueAndNonOverlapping()
    {
        SimulationWorld world = ScenarioFactory.CreateStress60();
        EntityId[] ids = ScenarioFactory.OwnedIds(world, 0);
        new SimulationRunner(world).StepOneTick();
        for (int i = 0; i < ids.Length; i++)
        for (int j = i + 1; j < ids.Length; j++)
        {
            NavigationAgent first = world.Entities.Navigation.Get(ids[i]);
            NavigationAgent second = world.Entities.Navigation.Get(ids[j]);
            Fix32 minimum = FootprintRules.CollisionRadiusBuild(first.Footprint) + FootprintRules.CollisionRadiusBuild(second.Footprint);
            Assert.That(FixVec2.Distance(first.Target, second.Target).Raw, Is.GreaterThanOrEqualTo(minimum.Raw), $"Stress slots for {ids[i]}/{ids[j]} overlap.");
        }
    }

    [Test]
    public void SixtyMoverScheduleIsFeasibleForEveryAssignedRouteAndSlowestMember()
    {
        SimulationWorld world = ScenarioFactory.CreateStress60();
        EntityId[] ids = ScenarioFactory.OwnedIds(world, 0);
        CommandEnvelope[] authoredMoves = System.Linq.Enumerable.ToArray(
            System.Linq.Enumerable.Where(world.Commands.All, command => command.Type == SimCommandType.Move));
        Assert.That(authoredMoves.Length, Is.EqualTo(3));

        int[] travelTicks =
        {
            ScenarioFactory.Stress60SecondMoveTick - ScenarioFactory.Stress60FirstMoveTick,
            ScenarioFactory.Stress60TopologyOpenTick - ScenarioFactory.Stress60SecondMoveTick,
            ScenarioFactory.Stress60FinalEvaluationTick - ScenarioFactory.Stress60ThirdMoveTick
        };

        CommandExecutionSystem commands = new();
        for (int phase = 0; phase < authoredMoves.Length; phase++)
        {
            if (phase == 2) world.OpenExcavatable(DevMapFactory.ExcavatableFeatureId);
            world.Commands.Enqueue(new CommandEnvelope(world.Tick, 0, checked((uint)(100 + phase)), SimCommandType.Move, ids, authoredMoves[phase].TargetPosition));
            commands.Step(world);
            AssertPhaseTravelBudget(world, ids, travelTicks[phase], phase + 1);
            SettleAtAssignedTargets(world, ids);
        }
    }

    [Test] public void SixtyMoversRemainInsideMapAndSimulationAdvances()
    {
        SimulationRunner r = new(ScenarioFactory.CreateStress60()); r.StepTicks(1600); Assert.That(r.World.Tick.Value, Is.EqualTo(1600));
        foreach (EntityId id in r.World.Entities.Alive)
        {
            if (!r.World.Entities.Ownership.TryGet(id, out Ownership o) || o.PlayerSlot != 0) continue; SimTransform t = r.World.Entities.Transform.Get(id);
            Assert.That(t.Position.X.FloorToInt(), Is.InRange(0,159)); Assert.That(t.Position.Y.FloorToInt(), Is.InRange(0,159));
        }
    }

    private static void AssertPhaseTravelBudget(SimulationWorld world, EntityId[] ids, int availableTicks, int phase)
    {
        Fix32 availableSeconds = Fix32.FromRatio(availableTicks, SimClock.TicksPerSecond);
        for (int i = 0; i < ids.Length; i++)
        {
            EntityId id = ids[i];
            SimTransform transform = world.Entities.Transform.Get(id);
            Movement movement = world.Entities.Movement.Get(id);
            NavigationAgent navigation = world.Entities.Navigation.Get(id);
            RouteCorridor corridor = new();
            world.Pathfinder.FindCorridor(MapGrid.BuildToNav(transform.Position), MapGrid.BuildToNav(navigation.Target), navigation.Footprint, corridor, navigation.Layer);
            Assert.That(corridor.IsValid, Is.True, $"Stress phase {phase} route for {id} is unreachable.");

            Fix32 routeLength = CorridorLength(transform.Position, navigation.Target, corridor);
            Fix32 availableDistance = movement.MaxSpeed * availableSeconds;
            Fix32 requiredWithAllowance = routeLength * Fix32.FromInt(ScenarioFactory.Stress60TravelAllowanceMultiplier);
            Assert.That(availableDistance.Raw, Is.GreaterThanOrEqualTo(requiredWithAllowance.Raw),
                $"Stress phase {phase} gives {id} less than the required 2x route-time allowance.");
        }
    }

    private static Fix32 CorridorLength(FixVec2 start, FixVec2 target, RouteCorridor corridor)
    {
        FixVec2 previous = start;
        Fix32 length = Fix32.Zero;
        for (int i = 0; i < corridor.Cells.Count; i++)
        {
            FixVec2 next = MapGrid.NavCellCenterToBuild(corridor.Cells[i]);
            length += FixVec2.Distance(previous, next);
            previous = next;
        }
        return length + FixVec2.Distance(previous, target);
    }

    private static void SettleAtAssignedTargets(SimulationWorld world, EntityId[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            EntityId id = ids[i];
            ref SimTransform transform = ref world.Entities.Transform.Get(id);
            ref Movement movement = ref world.Entities.Movement.Get(id);
            transform.Position = world.Entities.Navigation.Get(id).Target;
            movement.CurrentSpeed = Fix32.Zero;
            movement.CurrentVelocity = FixVec2.Zero;
            movement.DesiredMovement = FixVec2.Zero;
            movement.LastPosition = transform.Position;
        }
        world.Spatial.Rebuild(world.Entities);
    }
}
