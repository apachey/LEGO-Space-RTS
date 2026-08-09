using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class StressTests
{
    [Test] public void SixtyMoversRemainInsideMapAndSimulationAdvances()
    {
        SimulationRunner r = new(ScenarioFactory.CreateStress60()); r.StepTicks(1600); Assert.That(r.World.Tick.Value, Is.EqualTo(1600));
        foreach (EntityId id in r.World.Entities.Alive)
        {
            if (!r.World.Entities.Ownership.TryGet(id, out Ownership o) || o.PlayerSlot != 0) continue; SimTransform t = r.World.Entities.Transform.Get(id);
            Assert.That(t.Position.X.FloorToInt(), Is.InRange(0,159)); Assert.That(t.Position.Y.FloorToInt(), Is.InRange(0,159));
        }
    }
}
