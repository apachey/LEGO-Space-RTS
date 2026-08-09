using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class SnapshotReplayTests
{
    [Test] public void SnapshotRoundTripPreservesHash()
    {
        SimulationRunner r = new(ScenarioFactory.CreateStress60()); r.StepTicks(200); ulong before = StateHasher.Hash(r.World);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(r.World)); Assert.That(StateHasher.Hash(restored), Is.EqualTo(before));
    }

    [Test] public void RestoreThenContinueMatches()
    {
        SimulationRunner a = new(ScenarioFactory.CreateStress60()); a.StepTicks(300); byte[] snap = SnapshotSerializer.Serialize(a.World); SimulationRunner b = new(SnapshotSerializer.Deserialize(snap));
        a.StepTicks(400); b.StepTicks(400); Assert.That(StateHasher.Hash(b.World), Is.EqualTo(StateHasher.Hash(a.World)));
    }

    [Test] public void ReplayProducesSameFinalHash()
    {
        ReplayLog log = ScenarioFactory.CreateGoldenReplay(); SimulationRunner a = log.CreateRunner(); SimulationRunner b = log.CreateRunner(); a.StepTicks(3000); b.StepTicks(3000);
        Assert.That(StateHasher.Hash(b.World), Is.EqualTo(StateHasher.Hash(a.World)));
    }
}
