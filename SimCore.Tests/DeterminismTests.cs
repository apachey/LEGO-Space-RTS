using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class DeterminismTests
{
    [Test] public void GoldenScenarioRepeatedRunsMatchAtCheckpoints()
    {
        int[] checkpoints = { 250, 500, 1000, 1500, 2000, 3000 };
        ulong[]? baseline = null;
        for (int run = 0; run < 10; run++)
        {
            SimulationRunner r = ScenarioFactory.CreateGoldenReplay().CreateRunner(); ulong[] hashes = new ulong[checkpoints.Length]; int previous = 0;
            for (int i = 0; i < checkpoints.Length; i++) { r.StepTicks(checkpoints[i] - previous); previous = checkpoints[i]; hashes[i] = StateHasher.Hash(r.World); }
            if (baseline is null) baseline = hashes; else Assert.That(hashes, Is.EqualTo(baseline));
        }
    }

    [Test] public void CapabilityForHundredRunGate()
    {
        ulong? expected = null;
        for (int run = 0; run < 100; run++)
        {
            SimulationRunner r = ScenarioFactory.CreateGoldenReplay().CreateRunner(); r.StepTicks(600); ulong h = StateHasher.Hash(r.World);
            if (expected is null) expected = h; else Assert.That(h, Is.EqualTo(expected.Value));
        }
    }
}
