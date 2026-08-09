using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class FogLosTests
{
    [Test] public void AuthoredOccluderBlocksCellsBehindIt()
    {
        MapGrid map=new("map.test.los");
        map.SetFlagsRect(new IntRect(20,16,2,10),MapCellFlags.GroundOccluder,MapCellFlags.None);
        SimulationWorld world=new(map,1); EntityId id=world.Entities.Create(); FixVec2 pos=FixVec2.FromInts(8,10);
        world.Entities.Ownership.Set(id,new Ownership{PlayerSlot=0}); world.Entities.Transform.Set(id,new SimTransform{Position=pos,Orientation=Angle16.Zero});
        world.Entities.Vision.Set(id,new Vision{RadiusBuildCells=8,LastFogX=-1,LastFogY=-1}); new VisionSystem().Step(world);
        Assert.That(world.Fog.Get(0,9,10),Is.EqualTo(VisibilityState.Visible));
        Assert.That(world.Fog.Get(0,12,10),Is.EqualTo(VisibilityState.Unseen));
    }
}
