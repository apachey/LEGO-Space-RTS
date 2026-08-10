using System;
using System.Collections.Generic;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class CommandTests
{
    [Test] public void BufferOrdersTickPlayerThenSequence()
    {
        CommandBuffer b=new(); EntityId[] none=Array.Empty<EntityId>();
        b.Enqueue(new CommandEnvelope(new SimTick(4),1,9,SimCommandType.Stop,none,FixVec2.Zero));
        b.Enqueue(new CommandEnvelope(new SimTick(4),0,7,SimCommandType.Stop,none,FixVec2.Zero));
        b.Enqueue(new CommandEnvelope(new SimTick(4),0,3,SimCommandType.Stop,none,FixVec2.Zero));
        List<CommandEnvelope> due=new(); b.DrainForTick(new SimTick(4),due);
        Assert.That(due[0].PlayerSlot,Is.EqualTo(0)); Assert.That(due[0].Sequence,Is.EqualTo(3)); Assert.That(due[1].Sequence,Is.EqualTo(7)); Assert.That(due[2].PlayerSlot,Is.EqualTo(1));
    }
    [Test] public void QueueRejectsNewestWhenFull()
    {
        UnitCommandQueue q=new(); for(int i=0;i<UnitCommandQueue.Capacity;i++) Assert.That(q.Enqueue(new UnitOrder(UnitOrderType.Move,FixVec2.FromInts(i,0))),Is.True);
        Assert.That(q.Enqueue(new UnitOrder(UnitOrderType.Move,FixVec2.Zero)),Is.False); Assert.That(q.Count,Is.EqualTo(UnitCommandQueue.Capacity));
    }
    [Test] public void UnknownCommandsFailLoudly() => Assert.Throws<ArgumentOutOfRangeException>(()=>new CommandEnvelope(new SimTick(1),0,1,(SimCommandType)999,Array.Empty<EntityId>(),FixVec2.Zero));
    [Test] public void AttackRequiresTargetAndMayBeQueued()
    {
        Assert.Throws<ArgumentException>(()=>new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Attack,new[]{new EntityId(1)},FixVec2.Zero));
        Assert.DoesNotThrow(()=>new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Attack,new[]{new EntityId(1)},FixVec2.Zero,CommandModifiers.Queue,targetEntity:new EntityId(2)));
    }
    [Test] public void RunnerExecutesCommandsOnTheirDeclaredTick()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(1); EntityId id=ScenarioFactory.OwnedIds(world,0)[0];
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{id},FixVec2.FromInts(100,30)));
        new SimulationRunner(world).StepTicks(1);
        Assert.That(world.Tick.Value,Is.EqualTo(1)); Assert.That(world.Entities.Navigation.Get(id).HasTarget,Is.True);
    }
    [Test] public void ProfiledRunnerExecutesCommandsOnTheirDeclaredTick()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(1); EntityId id=ScenarioFactory.OwnedIds(world,0)[0];
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,new[]{id},FixVec2.FromInts(100,30)));
        new SimulationRunner(world).StepOneTickProfiled();
        Assert.That(world.Tick.Value,Is.EqualTo(1)); Assert.That(world.Entities.Navigation.Get(id).HasTarget,Is.True);
    }
}
