using System.Collections.Generic;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class ReservationTests
{
    [Test] public void ReservationPermitsRepeatExactlyAcrossRuns()
    {
        SimulationRunner a=CreateReservationConflict(); SimulationRunner b=CreateReservationConflict();
        AssertLegalStarts(a.World);
        a.StepTicks(2); b.StepTicks(2);
        List<bool> pa=new(); List<bool> pb=new();
        foreach(EntityId id in a.World.Entities.Alive) if(a.World.Entities.Ownership.TryGet(id,out Ownership oa)&&oa.PlayerSlot==0) pa.Add(a.World.HasReservationPermit(id));
        foreach(EntityId id in b.World.Entities.Alive) if(b.World.Entities.Ownership.TryGet(id,out Ownership ob)&&ob.PlayerSlot==0) pb.Add(b.World.HasReservationPermit(id));
        Assert.That(pa,Is.EqualTo(pb)); Assert.That(pa.Exists(x=>x),Is.True); Assert.That(pa.Exists(x=>!x),Is.True);
    }

    private static SimulationRunner CreateReservationConflict()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4); EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,ids,FixVec2.FromInts(84,80)));
        return new SimulationRunner(world);
    }

    private static void AssertLegalStarts(SimulationWorld world)
    {
        EntityId[] ids=ScenarioFactory.OwnedIds(world,0); Assert.That(ids.Length,Is.EqualTo(4));
        for(int i=0;i<ids.Length;i++)for(int j=i+1;j<ids.Length;j++)
        {
            Fix32 required=FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(ids[i]).Footprint)+FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(ids[j]).Footprint);
            Fix32 actual=FixVec2.Distance(world.Entities.Transform.Get(ids[i]).Position,world.Entities.Transform.Get(ids[j]).Position);
            Assert.That(actual,Is.GreaterThanOrEqualTo(required),$"Fixture starts overlap: {ids[i]}/{ids[j]}.");
        }
    }
}
