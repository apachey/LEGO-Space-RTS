using System;
using System.IO;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M6NetworkReplayTests
{
    private const ulong Token = 0x0102030405060708UL;

    [Test]
    public void ServerLogRoundTripsSeeksAndReplaysToTheAuthoritativeHash()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(2);
        NetworkMatchManifest manifest = new(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        ServerReplayRecorder recorder = new(world, manifest, deterministicSeed: 1234);
        EntityId mover = ScenarioFactory.OwnedIds(world, 0).First();
        ServerCommandSession session = new(2, 0, Token);
        CommandEnvelope request = new(new SimTick(999), 0, 1, SimCommandType.Move, new[] { mover }, FixVec2.FromInts(100, 40));
        NetworkCommandAcknowledgment acknowledgment = new ServerCommandAuthority().Process(world, session,
            NetworkCommandProtocol.EncodeRequest(Token, request), out CommandEnvelope accepted);
        Assert.That(acknowledgment.Accepted, Is.True);
        recorder.RecordAcceptedCommand(accepted);

        SimulationRunner live = new(world);
        for (int i = 0; i < 420; i++) { live.StepOneTick(); recorder.AfterTick(world); }
        ulong liveHash = StateHasher.Hash(world);
        ReplayLog log = ReplayLog.Deserialize(recorder.FinalizeAndSerialize(world));

        Assert.Multiple(() =>
        {
            Assert.That(log.GameplayContentHash, Is.EqualTo(manifest.GameplayContentHash));
            Assert.That(log.MapHash, Is.EqualTo(manifest.MapHash));
            Assert.That(log.DeterministicSeed, Is.EqualTo(1234));
            Assert.That(log.Commands.Select(command => command.Sequence), Is.EqualTo(new uint[] { 1 }));
            Assert.That(log.HashCheckpoints.Count, Is.EqualTo(21));
            Assert.That(log.SeekCheckpoints.Select(checkpoint => checkpoint.Tick.Value), Is.EqualTo(new[] { 200, 400 }));
            Assert.That(log.FinalTick, Is.EqualTo(420));
            Assert.That(log.ExpectedFinalHash, Is.EqualTo(liveHash));
        });

        SimulationRunner replayed = log.CreateRunnerAtTick(420);
        Assert.That(StateHasher.Hash(replayed.World), Is.EqualTo(liveHash));

        SimulationRunner direct350 = log.CreateRunner(); direct350.StepTicks(350);
        SimulationRunner seek350 = log.CreateRunnerAtTick(350);
        Assert.That(StateHasher.Hash(seek350.World), Is.EqualTo(StateHasher.Hash(direct350.World)));
    }

    [Test]
    public void PlaybackStopsOnAHashCheckpointMismatch()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        NetworkMatchManifest manifest = new(world.Content.ContentHash, NetworkMatchManifest.ComputeMapHash(world.Map));
        ServerReplayRecorder recorder = new(world, manifest);
        SimulationRunner live = new(world);
        for (int i = 0; i < 20; i++) { live.StepOneTick(); recorder.AfterTick(world); }
        ReplayLog log = recorder.Log;
        ReplayHashCheckpoint valid = log.HashCheckpoints.Single();
        log.HashCheckpoints[0] = new ReplayHashCheckpoint(valid.Tick, valid.StateHash ^ 1UL);
        Assert.Throws<InvalidDataException>(() => log.CreateRunnerAtTick(20));
    }

    [Test]
    public void ReliableBulkChunksReassembleOutOfOrderAndVerifyTheWholeReplayHash()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        ReplayLog log = new(SnapshotSerializer.Serialize(world))
        {
            GameplayContentHash = world.Content.ContentHash,
            MapHash = NetworkMatchManifest.ComputeMapHash(world.Map),
            ExpectedFinalHash = StateHasher.Hash(world)
        };
        byte[] replay = log.Serialize();
        NetworkReplayChunk[] chunks = NetworkReplayProtocol.CreateChunks(77, replay);
        Assert.That(chunks.Length, Is.GreaterThan(1));
        NetworkReplayAssembler assembler = new();
        byte[] assembled = Array.Empty<byte>();
        for (int i = chunks.Length - 1; i >= 0; i--)
        {
            byte[] packet = NetworkReplayProtocol.EncodeChunk(chunks[i]);
            Assert.That(packet.Length, Is.LessThanOrEqualTo(NetworkSnapshotProtocol.MaximumPacketBytes));
            Assert.That(NetworkReplayProtocol.TryDecodeChunk(packet, out NetworkReplayChunk decoded), Is.True);
            if (assembler.TryAdd(decoded, out byte[] completed)) assembled = completed;
        }
        Assert.That(assembled, Is.EqualTo(replay));
        Assert.That(ReplayLog.Deserialize(assembled).ExpectedFinalHash, Is.EqualTo(log.ExpectedFinalHash));
    }

    [Test]
    public void ReplayRequestAndLegacyV15LogRemainReadable()
    {
        byte[] request = NetworkReplayProtocol.EncodeRequest(new NetworkReplayRequest(Token));
        Assert.That(NetworkReplayProtocol.TryDecodeRequest(request, out NetworkReplayRequest decoded), Is.True);
        Assert.That(decoded.SessionToken, Is.EqualTo(Token));

        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        byte[] snapshot = SnapshotSerializer.Serialize(world);
        using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
        writer.Write(ReplayLog.Magic); writer.Write((ushort)15); writer.Write(snapshot.Length); writer.Write(snapshot);
        writer.Write(0); writer.Write(StateHasher.Hash(world)); writer.Flush();
        ReplayLog legacy = ReplayLog.Deserialize(stream.ToArray());
        Assert.Multiple(() =>
        {
            Assert.That(legacy.GameplayContentHash, Is.Zero);
            Assert.That(legacy.SeekCheckpoints, Is.Empty);
            Assert.That(StateHasher.Hash(legacy.CreateRunner().World), Is.EqualTo(StateHasher.Hash(world)));
        });
    }
}
