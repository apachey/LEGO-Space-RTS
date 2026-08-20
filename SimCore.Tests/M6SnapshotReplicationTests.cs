using System;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M6SnapshotReplicationTests
{
    [Test]
    public void FullThenAcknowledgedDeltaReconstructsTheRecipientState()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(6);
        NetworkSnapshotState initial = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotFrame full = RoundTrip(NetworkSnapshotProtocol.BuildFrame(1, initial));
        NetworkSnapshotClientBuffer client = new();
        Assert.That(client.TryApply(full, out NetworkSnapshotState firstClientState), Is.True);

        EntityId mover = ScenarioFactory.OwnedIds(world, 0).First();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Move,
            new[] { mover }, FixVec2.FromInts(100, 40)));
        new SimulationRunner(world).StepTicks(2);
        NetworkSnapshotState current = NetworkSnapshotState.Capture(world, 0);
        NetworkSnapshotFrame delta = RoundTrip(NetworkSnapshotProtocol.BuildFrame(2, current, 1, initial));

        Assert.Multiple(() =>
        {
            Assert.That(full.IsFull, Is.True);
            Assert.That(delta.IsFull, Is.False);
            Assert.That(delta.BaselineSequence, Is.EqualTo(1));
            Assert.That(delta.EntityUpserts.Length, Is.LessThan(current.Entities.Length));
            Assert.That(firstClientState.Tick.Value, Is.Zero);
        });
        Assert.That(client.TryApply(delta, out NetworkSnapshotState reconstructed), Is.True);
        AssertStateEqual(current, reconstructed);
    }

    [Test]
    public void ServerUsesOnlyAnAcknowledgedRetainedBaseline()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(2);
        ServerSnapshotSession server = new();

        NetworkSnapshotFrame first = Decode(server.CreatePacket(world, 0));
        NetworkSnapshotFrame unacknowledged = Decode(server.CreatePacket(world, 0));
        Assert.That(first.IsFull, Is.True);
        Assert.That(unacknowledged.IsFull, Is.True);

        Assert.That(server.TryAcknowledge(first.Sequence), Is.True);
        new SimulationRunner(world).StepTicks(2);
        NetworkSnapshotFrame delta = Decode(server.CreatePacket(world, 0));
        Assert.Multiple(() =>
        {
            Assert.That(delta.IsFull, Is.False);
            Assert.That(delta.BaselineSequence, Is.EqualTo(first.Sequence));
            Assert.That(server.LastAcknowledgedSequence, Is.EqualTo(first.Sequence));
            Assert.That(server.TryAcknowledge(first.Sequence), Is.False);
        });
    }

    [Test]
    public void SnapshotAcknowledgmentAndPresentationEntityPayloadRoundTrip()
    {
        const ulong token = 0x8877665544332211UL;
        byte[] acknowledgment = NetworkSnapshotProtocol.EncodeAcknowledgment(new NetworkSnapshotAcknowledgment(token, 77));
        Assert.That(NetworkSnapshotProtocol.TryDecodeAcknowledgment(acknowledgment, out NetworkSnapshotAcknowledgment decoded), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.SessionToken, Is.EqualTo(token));
            Assert.That(decoded.SnapshotSequence, Is.EqualTo(77));
        });

        PresentationEntity original = PresentationSnapshot.Capture(ScenarioFactory.CreateFirstControllable(1), 0).Entities.First();
        NetworkReplicatedEntity replicated = NetworkSnapshotState.Capture(ScenarioFactory.CreateFirstControllable(1), 0).Entities.First();
        PresentationEntity restored = replicated.Decode();
        Assert.Multiple(() =>
        {
            Assert.That(restored.EntityId, Is.EqualTo(original.EntityId));
            Assert.That(restored.ContentType, Is.EqualTo(original.ContentType));
            Assert.That(restored.Position, Is.EqualTo(original.Position));
            Assert.That(restored.Orientation, Is.EqualTo(original.Orientation));
            Assert.That(restored.HasHealth, Is.EqualTo(original.HasHealth));
        });
    }

    [Test]
    public void ClientInterpolationUsesFixedPointAndTheShortestAngleArc()
    {
        PresentationEntity older = EntityAt(FixVec2.FromInts(10, 20), new Angle16(65_000));
        PresentationEntity newer = EntityAt(FixVec2.FromInts(14, 28), new Angle16(1_000));
        Assert.Multiple(() =>
        {
            Assert.That(NetworkSnapshotInterpolation.Position(older, newer, Fix32.Half), Is.EqualTo(FixVec2.FromInts(12, 24)));
            Assert.That(NetworkSnapshotInterpolation.Orientation(older, newer, Fix32.Half).Raw, Is.EqualTo(232));
        });
    }

    private static NetworkSnapshotFrame RoundTrip(NetworkSnapshotFrame frame) => Decode(NetworkSnapshotProtocol.EncodeFrame(frame));

    private static NetworkSnapshotFrame Decode(byte[] packet)
    {
        Assert.That(packet.Length, Is.LessThanOrEqualTo(NetworkSnapshotProtocol.MaximumPacketBytes));
        Assert.That(NetworkSnapshotProtocol.TryDecodeFrame(packet, out NetworkSnapshotFrame frame), Is.True);
        return frame;
    }

    private static void AssertStateEqual(NetworkSnapshotState expected, NetworkSnapshotState actual)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.Tick, Is.EqualTo(expected.Tick));
            Assert.That(actual.PlayerSlot, Is.EqualTo(expected.PlayerSlot));
            Assert.That(actual.OwnPlayer, Is.EqualTo(expected.OwnPlayer));
            Assert.That(actual.Entities.Select(value => value.EntityId.Value), Is.EqualTo(expected.Entities.Select(value => value.EntityId.Value)));
            Assert.That(actual.Projectiles.Select(value => value.ProjectileId.Value), Is.EqualTo(expected.Projectiles.Select(value => value.ProjectileId.Value)));
        });
        for (int i = 0; i < expected.Entities.Length; i++) Assert.That(actual.Entities[i].State, Is.EqualTo(expected.Entities[i].State));
    }

    private static PresentationEntity EntityAt(FixVec2 position, Angle16 orientation) => new(new EntityId(1), new ContentId(1), 0,
        position, orientation, MovementState.Moving, VisibilityState.Visible, FootprintClass.Small, SelectableKind.CombatSupport);
}
