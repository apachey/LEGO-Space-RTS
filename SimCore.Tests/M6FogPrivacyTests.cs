using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M6FogPrivacyTests
{
    [Test]
    public void RecipientSnapshotContainsOnlyOwnOrCurrentlyVisibleEntities()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(2);
        EntityId own = ScenarioFactory.OwnedIds(world, 0).First();
        EntityId enemy = ScenarioFactory.OwnedIds(world, 1).First();
        SimTransform enemyTransform = world.Entities.Transform.Get(enemy);
        Assert.That(world.Fog.IsVisible(0, enemyTransform.Position.X.FloorToInt(), enemyTransform.Position.Y.FloorToInt()), Is.False);

        WeaponState weapon = world.Entities.Weapon.TryGet(own, out WeaponState existing) ? existing : default;
        weapon.FireSequence = 99;
        weapon.LastFiredTarget = enemy;
        world.Entities.Weapon.Set(own, weapon);

        NetworkSnapshotState state = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotFrame decoded = Decode(NetworkSnapshotProtocol.EncodeFrame(NetworkSnapshotProtocol.BuildFrame(1, state)));
        PresentationEntity ownPresentation = decoded.EntityUpserts.Single(value => value.EntityId == own).Decode();
        Assert.Multiple(() =>
        {
            Assert.That(decoded.EntityUpserts.Any(value => value.EntityId == enemy), Is.False);
            Assert.That(ownPresentation.WeaponFireSequence, Is.EqualTo(99));
            Assert.That(ownPresentation.WeaponFireTarget, Is.EqualTo(EntityId.None), "A stale reference must not disclose a hidden entity ID.");
        });
    }

    [Test]
    public void LossOfVisibilityIsARemovalAndLastKnownStateStaysClientSide()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId enemy = ScenarioFactory.OwnedIds(world, 1).Single();
        SimTransform enemyTransform = world.Entities.Transform.Get(enemy);
        int x = enemyTransform.Position.X.FloorToInt(), y = enemyTransform.Position.Y.FloorToInt();
        world.Fog.AddVisible(0, x, y);

        NetworkSnapshotState visible = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotFrame full = Decode(NetworkSnapshotProtocol.EncodeFrame(NetworkSnapshotProtocol.BuildFrame(1, visible)));
        NetworkSnapshotClientBuffer client = new();
        Assert.That(client.TryApply(full, out _), Is.True);
        Assert.That(visible.Entities.Any(value => value.EntityId == enemy), Is.True);

        world.Fog.ClearCurrent();
        NetworkSnapshotState hidden = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotFrame delta = Decode(NetworkSnapshotProtocol.EncodeFrame(NetworkSnapshotProtocol.BuildFrame(2, hidden, 1, visible)));
        Assert.That(client.TryApply(delta, out NetworkSnapshotState reconstructed), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(delta.EntityRemovals, Does.Contain(enemy));
            Assert.That(delta.EntityUpserts.Any(value => value.EntityId == enemy), Is.False);
            Assert.That(reconstructed.Entities.Any(value => value.EntityId == enemy), Is.False);
            Assert.That(client.LastVisibilityLosses.Select(value => value.EntityId), Does.Contain(enemy));
            Assert.That(client.LastVisibilityLosses.Single(value => value.EntityId == enemy).Position, Is.EqualTo(enemyTransform.Position));
            Assert.That(reconstructed.GetKnowledge(x, y), Is.EqualTo(VisibilityState.Explored));
        });
    }

    [Test]
    public void PrivateEconomyAndFogBitsetsBelongOnlyToTheirRecipient()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId secretBank = world.Entities.Create();
        world.Entities.Ownership.Set(secretBank, new Ownership { PlayerSlot = 1 });
        world.Entities.ResourceBank.Set(secretBank, new ResourceBank { Type = ResourceType.Crystal, ProcessedAmount = 987_654_321 });
        SimTransform playerOne = world.Entities.Transform.Get(ScenarioFactory.OwnedIds(world, 1).Single());
        int x = playerOne.Position.X.FloorToInt(), y = playerOne.Position.Y.FloorToInt();
        world.Fog.AddVisible(1, x, y);

        NetworkSnapshotState playerZero = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotState playerOneState = NetworkSnapshotState.Capture(world, 1);
        Assert.Multiple(() =>
        {
            Assert.That(playerZero.OwnPlayer.Crystal, Is.Not.EqualTo(987_654_321));
            Assert.That(playerOneState.OwnPlayer.Crystal, Is.EqualTo(987_654_321));
            Assert.That(playerZero.ExploredBits.Length, Is.EqualTo(NetworkSnapshotState.KnowledgeBitsetBytes));
            Assert.That(playerZero.VisibleBits.Length, Is.EqualTo(NetworkSnapshotState.KnowledgeBitsetBytes));
            Assert.That(playerOneState.GetKnowledge(x, y), Is.EqualTo(VisibilityState.Visible));
        });
    }

    private static NetworkSnapshotFrame Decode(byte[] packet)
    {
        Assert.That(NetworkSnapshotProtocol.TryDecodeFrame(packet, out NetworkSnapshotFrame frame), Is.True);
        return frame;
    }
}
