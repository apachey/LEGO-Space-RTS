using System.Collections.Generic;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3ResourceNodeTests
{
    [Test]
    public void CanonicalOreDefinitionsUseApprovedFiniteCapacities()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        Dictionary<string, int> expected = new()
        {
            ["resource.ore.small"] = 600,
            ["resource.ore.standard"] = 900,
            ["resource.ore.rich"] = 1350,
            ["resource.ore.deep_contested_seam"] = 2400
        };

        Assert.That(catalog.ResourceNodes.Length, Is.EqualTo(expected.Count));
        Assert.That(catalog.ResourceNodes.Select(x => x.StableKey), Is.Ordered.Using<string>(System.StringComparer.Ordinal));
        foreach (ResourceNodeDefinition definition in catalog.ResourceNodes)
        {
            Assert.That(definition.Type, Is.EqualTo(ResourceType.Ore));
            Assert.That(definition.DepletionProfile, Is.EqualTo(ResourceDepletionProfile.Finite));
            Assert.That(definition.Capacity, Is.EqualTo(expected[definition.StableKey]));
        }
    }

    [Test]
    public void PrototypeMapSpawnsTwoStandardDepositsPerStartingSide()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        int leftCapacity = 0, rightCapacity = 0, nodeCount = 0;
        foreach (EntityId id in world.Entities.Alive)
        {
            if (!world.Entities.ResourceNode.TryGet(id, out ResourceNode node)) continue;
            nodeCount++;
            Assert.That(node.DepositSize, Is.EqualTo(ResourceDepositSize.Standard));
            SimTransform transform = world.Entities.Transform.Get(id);
            if (transform.Position.X < Fix32.FromInt(80)) leftCapacity += node.Capacity;
            else rightCapacity += node.Capacity;
        }
        Assert.That(nodeCount, Is.EqualTo(4));
        Assert.That(leftCapacity, Is.EqualTo(1800));
        Assert.That(rightCapacity, Is.EqualTo(1800));
    }

    [Test]
    public void PrototypeMapSpawnsOneHqEmergencyReceiverPerPlayer()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        int[] receivers = new int[2];
        foreach (EntityId id in world.Entities.Alive)
        {
            if (!world.Entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver)) continue;
            Ownership owner = world.Entities.Ownership.Get(id);
            receivers[owner.PlayerSlot]++;
            Assert.That(receiver.AcceptedType, Is.EqualTo(ResourceType.Ore));
            Assert.That(receiver.IsHqEmergencyReceiver, Is.True);
            Assert.That(receiver.PendingHauledAmount, Is.Zero);
            ResourceBank bank = world.Entities.ResourceBank.Get(id);
            Assert.That(bank.Type, Is.EqualTo(ResourceType.Ore));
            Assert.That(bank.ProcessedAmount, Is.EqualTo(500));
        }
        Assert.That(receivers, Is.EqualTo(new[] { 1, 1 }));
    }

    [Test]
    public void ExtractionClampsAtZeroAndAdvancesVisibleDepletionStates()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId nodeId = FirstResourceNode(world);
        ref ResourceNode node = ref world.Entities.ResourceNode.Get(nodeId);
        Assert.That(node.VisualState, Is.EqualTo(ResourceVisualState.Full));

        Assert.That(world.TryExtractResource(nodeId, 225, out int first), Is.True);
        Assert.That(first, Is.EqualTo(225));
        Assert.That(node.Remaining, Is.EqualTo(675));
        Assert.That(node.VisualState, Is.EqualTo(ResourceVisualState.Reduced));

        Assert.That(world.TryExtractResource(nodeId, 1000, out int final), Is.True);
        Assert.That(final, Is.EqualTo(675));
        Assert.That(node.Remaining, Is.Zero);
        Assert.That(node.VisualState, Is.EqualTo(ResourceVisualState.Exhausted));
        Assert.That(world.TryExtractResource(nodeId, 1, out int afterDepletion), Is.False);
        Assert.That(afterDepletion, Is.Zero);
    }

    [Test]
    public void ResourceDepletionSurvivesSnapshotAndParticipatesInStateHash()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(4);
        EntityId nodeId = FirstResourceNode(world);
        ulong fullHash = StateHasher.Hash(world);
        world.TryExtractResource(nodeId, 137, out _);
        ulong depletedHash = StateHasher.Hash(world);
        Assert.That(depletedHash, Is.Not.EqualTo(fullHash));

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(restored.Entities.ResourceNode.Get(nodeId).Remaining, Is.EqualTo(763));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(depletedHash));
    }

    private static EntityId FirstResourceNode(SimulationWorld world)
    {
        foreach (EntityId id in world.Entities.Alive) if (world.Entities.ResourceNode.Has(id)) return id;
        Assert.Fail("Scenario contains no resource node.");
        return EntityId.None;
    }
}
}
