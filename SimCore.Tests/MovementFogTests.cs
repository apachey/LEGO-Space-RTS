using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class MovementFogTests
{
    [Test] public void MoveAcceleratesAndAdvancesPathIndex()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4); EntityId id=ScenarioFactory.OwnedIds(world,0)[0]; FixVec2 before=world.Entities.Transform.Get(id).Position;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{id},before+FixVec2.FromInts(20,0)));
        SimulationRunner runner=new(world); runner.StepTicks(40); Movement movement=world.Entities.Movement.Get(id);
        Assert.That(movement.CurrentSpeed.Raw,Is.GreaterThan(0)); Assert.That(world.Entities.Transform.Get(id).Position,Is.Not.EqualTo(before)); Assert.That(movement.PathIndex,Is.GreaterThanOrEqualTo(1));
    }
    [Test] public void StopClearsMovementAndQueue()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4); EntityId id=ScenarioFactory.OwnedIds(world,0)[0]; SimulationRunner runner=new(world);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{id},FixVec2.FromInts(100,30)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2),0,2,SimCommandType.Move,new[]{id},FixVec2.FromInts(110,40),CommandModifiers.Queue));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(20),0,3,SimCommandType.Stop,new[]{id},FixVec2.Zero)); runner.StepTicks(20);
        Assert.That(world.Entities.Navigation.Get(id).HasTarget,Is.False); Assert.That(world.Entities.Movement.Get(id).State,Is.EqualTo(MovementState.Idle)); Assert.That(world.GetQueue(id).Count,Is.EqualTo(0));
    }
    [Test] public void CpuFogMarksVisionAndPreservesExploration()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(1); EntityId id=ScenarioFactory.OwnedIds(world,0)[0]; SimTransform t=world.Entities.Transform.Get(id); int x=t.Position.X.FloorToInt(),y=t.Position.Y.FloorToInt();
        Assert.That(world.Fog.Get(0,x,y),Is.EqualTo(VisibilityState.Visible));
        world.Entities.Vision.Remove(id); new VisionSystem().Step(world); Assert.That(world.Fog.Get(0,x,y),Is.EqualTo(VisibilityState.Explored));
    }
    [Test] public void CrossingUnitsDoNotInterpenetrateBelowCompressionLimit()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);
        EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        EntityId a=ids[0], b=ids[1];
        FixVec2 aStart=world.Entities.Transform.Get(a).Position, bStart=world.Entities.Transform.Get(b).Position;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{a},bStart));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,2,SimCommandType.Move,new[]{b},aStart));
        SimulationRunner runner=new(world);
        Fix32 min=FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(a).Footprint)+FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(b).Footprint);
        Fix32 compressed=min*Fix32.FromRatio(85,100);
        for(int i=0;i<240;i++)
        {
            runner.StepOneTick();
            Fix32 distance=FixVec2.Distance(world.Entities.Transform.Get(a).Position,world.Entities.Transform.Get(b).Position);
            Assert.That(distance.Raw,Is.GreaterThanOrEqualTo(compressed.Raw-Fix32.FromRatio(1,100).Raw),$"Units interpenetrated at tick {world.Tick.Value}.");
        }
    }

    [Test] public void QueuedMoveRemainsQueuedUntilCurrentMoveCompletes()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(1); EntityId id=ScenarioFactory.OwnedIds(world,0)[0];
        FixVec2 start=world.Entities.Transform.Get(id).Position; FixVec2 first=start+FixVec2.FromInts(4,0); FixVec2 second=start+FixVec2.FromInts(8,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{id},first));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,2,SimCommandType.Move,new[]{id},second,CommandModifiers.Queue));
        SimulationRunner runner=new(world); runner.StepOneTick(); runner.StepOneTick();
        Assert.That(world.GetQueue(id).Count,Is.EqualTo(1));
        Assert.That(world.Entities.Navigation.Get(id).Target,Is.Not.EqualTo(second));
        for(int i=0;i<400 && world.GetQueue(id).Count>0;i++) runner.StepOneTick();
        Assert.That(world.GetQueue(id).Count,Is.EqualTo(0));
        Assert.That(world.Entities.Navigation.Get(id).Target,Is.EqualTo(second));
    }

}
