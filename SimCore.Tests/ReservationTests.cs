using System.Collections.Generic;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class ReservationTests
{
    [Test] public void ReservationPermitsRepeatExactlyAcrossRuns()
    {
        SimulationRunner a=new(ScenarioFactory.CreateStress60()); SimulationRunner b=new(ScenarioFactory.CreateStress60());
        a.StepTicks(2); b.StepTicks(2);
        List<bool> pa=new(); List<bool> pb=new();
        foreach(EntityId id in a.World.Entities.Alive) if(a.World.Entities.Ownership.TryGet(id,out Ownership oa)&&oa.PlayerSlot==0) pa.Add(a.World.HasReservationPermit(id));
        foreach(EntityId id in b.World.Entities.Alive) if(b.World.Entities.Ownership.TryGet(id,out Ownership ob)&&ob.PlayerSlot==0) pb.Add(b.World.HasReservationPermit(id));
        Assert.That(pa,Is.EqualTo(pb)); Assert.That(pa.Exists(x=>x),Is.True);
    }
}
