using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5AcceptanceScenarioTests
{
    [Test]
    public void FreshHandoffContainsEveryM5ProofAndPreservesDeterministicContinuation()
    {
        SimulationWorld world = M5AcceptanceScenarioFactory.Create();
        Assert.That(M5AcceptanceScenarioFactory.IsFreshHandoffReady(world, out string initialReason), Is.True, initialReason);

        SimulationRunner runner = new(world);
        runner.StepTicks(40);
        Assert.That(M5AcceptanceScenarioFactory.IsFreshHandoffReady(world, out string runningReason), Is.True, runningReason);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        SimulationRunner restoredRunner = new(restored);
        runner.StepTicks(80);
        restoredRunner.StepTicks(80);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }
}
}
