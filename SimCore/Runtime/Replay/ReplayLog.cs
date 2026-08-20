using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct ReplayHashCheckpoint
{
    public readonly SimTick Tick;
    public readonly ulong StateHash;
    public ReplayHashCheckpoint(SimTick tick, ulong stateHash) { Tick = tick; StateHash = stateHash; }
}

public readonly struct ReplaySeekCheckpoint
{
    public readonly SimTick Tick;
    public readonly ulong StateHash;
    public readonly byte[] Snapshot;
    public ReplaySeekCheckpoint(SimTick tick, ulong stateHash, byte[] snapshot) { Tick = tick; StateHash = stateHash; Snapshot = snapshot; }
}

public sealed class ReplayLog
{
    public const uint Magic = 0x52545253; // RTRS
    public const ushort Version = 16;
    public byte[] InitialSnapshot { get; }
    public List<CommandEnvelope> Commands { get; } = new();
    public List<ReplaySeekCheckpoint> SeekCheckpoints { get; } = new();
    public List<ReplayHashCheckpoint> HashCheckpoints { get; } = new();
    public ulong GameplayContentHash { get; set; }
    public ulong MapHash { get; set; }
    public ulong DeterministicSeed { get; set; }
    public int FinalTick { get; set; }
    public ulong ExpectedFinalHash { get; set; }

    public ReplayLog(byte[] initialSnapshot)
    {
        InitialSnapshot = initialSnapshot ?? throw new ArgumentNullException(nameof(initialSnapshot));
    }

    public byte[] Serialize()
    {
        SortAndValidate();
        using MemoryStream ms = new(); using BinaryWriter w = new(ms);
        w.Write(Magic); w.Write(Version); w.Write(GameplayContentHash); w.Write(MapHash); w.Write(DeterministicSeed);
        WriteBytes(w, InitialSnapshot); w.Write(Commands.Count);
        for (int i = 0; i < Commands.Count; i++) Commands[i].Write(w);
        w.Write(SeekCheckpoints.Count);
        for (int i = 0; i < SeekCheckpoints.Count; i++)
        {
            ReplaySeekCheckpoint checkpoint = SeekCheckpoints[i]; w.Write(checkpoint.Tick.Value); w.Write(checkpoint.StateHash); WriteBytes(w, checkpoint.Snapshot);
        }
        w.Write(HashCheckpoints.Count);
        for (int i = 0; i < HashCheckpoints.Count; i++) { w.Write(HashCheckpoints[i].Tick.Value); w.Write(HashCheckpoints[i].StateHash); }
        w.Write(FinalTick); w.Write(ExpectedFinalHash); w.Flush(); return ms.ToArray();
    }

    public static ReplayLog Deserialize(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        using MemoryStream ms = new(bytes, false); using BinaryReader r = new(ms);
        if (r.ReadUInt32() != Magic) throw new InvalidDataException("Replay magic mismatch.");
        ushort version = r.ReadUInt16();
        if (version < 1 || (version > 6 && version != 15 && version != Version)) throw new InvalidDataException("Replay version mismatch.");
        if (version != Version) return ReadLegacy(r, version, ms);

        ulong contentHash = r.ReadUInt64(), mapHash = r.ReadUInt64(), seed = r.ReadUInt64();
        ReplayLog log = new(ReadBytes(r)) { GameplayContentHash = contentHash, MapHash = mapHash, DeterministicSeed = seed };
        ReadCommands(r, log, version);
        int seekCount = ReadCount(r, 100_000, "seek checkpoint"); int previousTick = -1;
        for (int i = 0; i < seekCount; i++)
        {
            int tick = r.ReadInt32(); ulong hash = r.ReadUInt64(); byte[] snapshot = ReadBytes(r);
            if (tick <= previousTick || tick % ServerReplayRecorder.SeekIntervalTicks != 0) throw new InvalidDataException("Replay seek checkpoints are not in canonical order.");
            log.SeekCheckpoints.Add(new ReplaySeekCheckpoint(new SimTick(tick), hash, snapshot)); previousTick = tick;
        }
        int hashCount = ReadCount(r, 1_000_000, "hash checkpoint"); previousTick = -1;
        for (int i = 0; i < hashCount; i++)
        {
            int tick = r.ReadInt32(); ulong hash = r.ReadUInt64();
            if (tick <= previousTick || tick % ServerReplayRecorder.HashIntervalTicks != 0) throw new InvalidDataException("Replay hash checkpoints are not in canonical order.");
            log.HashCheckpoints.Add(new ReplayHashCheckpoint(new SimTick(tick), hash)); previousTick = tick;
        }
        log.FinalTick = r.ReadInt32(); if (log.FinalTick < 0) throw new InvalidDataException("Replay final tick is invalid.");
        log.ExpectedFinalHash = r.ReadUInt64();
        if (ms.Position != ms.Length) throw new InvalidDataException("Replay has trailing data.");
        log.SortAndValidate(); return log;
    }

    public SimulationRunner CreateRunner()
    {
        SimulationWorld world = SnapshotSerializer.Deserialize(InitialSnapshot);
        for (int i = 0; i < Commands.Count; i++) world.Commands.Enqueue(Commands[i]);
        return new SimulationRunner(world);
    }

    public SimulationRunner CreateRunnerAtTick(int targetTick, bool verifyHashCheckpoints = true)
    {
        if (FinalTick > 0 && targetTick > FinalTick) throw new ArgumentOutOfRangeException(nameof(targetTick), "Replay playback cannot advance past the recorded final tick.");
        SimulationWorld world;
        int startTick;
        ReplaySeekCheckpoint? nearest = null;
        for (int i = 0; i < SeekCheckpoints.Count; i++)
        {
            if (SeekCheckpoints[i].Tick.Value > targetTick) break;
            nearest = SeekCheckpoints[i];
        }
        if (nearest.HasValue)
        {
            ReplaySeekCheckpoint checkpoint = nearest.Value; world = SnapshotSerializer.Deserialize(checkpoint.Snapshot); startTick = checkpoint.Tick.Value;
            if (world.Tick.Value != startTick || StateHasher.Hash(world) != checkpoint.StateHash) throw new InvalidDataException("Replay seek checkpoint hash mismatch.");
        }
        else
        {
            world = SnapshotSerializer.Deserialize(InitialSnapshot); startTick = world.Tick.Value;
        }
        if (targetTick < startTick) throw new ArgumentOutOfRangeException(nameof(targetTick));
        for (int i = 0; i < Commands.Count; i++) if (Commands[i].ExecutionTick.Value > startTick) world.Commands.Enqueue(Commands[i]);
        SimulationRunner runner = new(world);
        if (verifyHashCheckpoints) VerifyCheckpointAtCurrentTick(runner.World);
        while (runner.World.Tick.Value < targetTick)
        {
            runner.StepOneTick();
            if (verifyHashCheckpoints) VerifyCheckpointAtCurrentTick(runner.World);
        }
        if (verifyHashCheckpoints && FinalTick > 0 && targetTick == FinalTick && ExpectedFinalHash != 0 && StateHasher.Hash(runner.World) != ExpectedFinalHash)
            throw new InvalidDataException("Replay final state hash mismatch.");
        return runner;
    }

    private void VerifyCheckpointAtCurrentTick(SimulationWorld world)
    {
        for (int i = 0; i < HashCheckpoints.Count; i++)
        {
            if (HashCheckpoints[i].Tick.Value < world.Tick.Value) continue;
            if (HashCheckpoints[i].Tick.Value > world.Tick.Value) return;
            if (StateHasher.Hash(world) != HashCheckpoints[i].StateHash) throw new InvalidDataException($"Replay state hash mismatch at tick {world.Tick.Value}.");
            return;
        }
    }

    private static ReplayLog ReadLegacy(BinaryReader r, ushort version, MemoryStream stream)
    {
        ReplayLog log = new(ReadBytes(r)); ReadCommands(r, log, version); log.ExpectedFinalHash = r.ReadUInt64();
        if (stream.Position != stream.Length) throw new InvalidDataException("Legacy replay has trailing data.");
        return log;
    }

    private static void ReadCommands(BinaryReader r, ReplayLog log, ushort version)
    {
        int count = ReadCount(r, 1_000_000, "command");
        for (int i = 0; i < count; i++) log.Commands.Add(CommandEnvelope.Read(r, includeBuildFields: version >= 2,
            includeEnergyPriority: version >= 5, includeMissionRefit: version >= 15, includeResonanceCommitment: version >= 15));
    }

    private void SortAndValidate()
    {
        Commands.Sort(CompareCommands);
        SeekCheckpoints.Sort((a, b) => a.Tick.Value.CompareTo(b.Tick.Value));
        HashCheckpoints.Sort((a, b) => a.Tick.Value.CompareTo(b.Tick.Value));
        int previous = -1;
        for (int i = 0; i < SeekCheckpoints.Count; i++)
        {
            if (SeekCheckpoints[i].Tick.Value <= previous || SeekCheckpoints[i].Snapshot == null) throw new InvalidDataException("Invalid replay seek checkpoint ordering.");
            previous = SeekCheckpoints[i].Tick.Value;
        }
        previous = -1;
        for (int i = 0; i < HashCheckpoints.Count; i++)
        {
            if (HashCheckpoints[i].Tick.Value <= previous) throw new InvalidDataException("Invalid replay hash checkpoint ordering.");
            previous = HashCheckpoints[i].Tick.Value;
        }
    }

    private static int CompareCommands(CommandEnvelope a, CommandEnvelope b)
    {
        int c = a.ExecutionTick.Value.CompareTo(b.ExecutionTick.Value); if (c != 0) return c;
        c = a.PlayerSlot.CompareTo(b.PlayerSlot); return c != 0 ? c : a.Sequence.CompareTo(b.Sequence);
    }

    private static void WriteBytes(BinaryWriter writer, byte[] bytes)
    {
        writer.Write(bytes.Length); writer.Write(bytes);
    }

    private static byte[] ReadBytes(BinaryReader reader)
    {
        int length = reader.ReadInt32(); if (length < 0 || length > 64 * 1024 * 1024) throw new InvalidDataException("Invalid replay snapshot size.");
        byte[] bytes = reader.ReadBytes(length); if (bytes.Length != length) throw new EndOfStreamException(); return bytes;
    }

    private static int ReadCount(BinaryReader reader, int maximum, string label)
    {
        int count = reader.ReadInt32(); if (count < 0 || count > maximum) throw new InvalidDataException($"Invalid replay {label} count."); return count;
    }
}

public sealed class ServerReplayRecorder
{
    public const int HashIntervalTicks = SimClock.TicksPerSecond;
    public const int SeekIntervalTicks = 10 * SimClock.TicksPerSecond;
    private readonly ReplayLog _log;

    public ServerReplayRecorder(SimulationWorld initialWorld, NetworkMatchManifest manifest, ulong deterministicSeed = 0)
    {
        if (initialWorld == null) throw new ArgumentNullException(nameof(initialWorld));
        _log = new ReplayLog(SnapshotSerializer.Serialize(initialWorld))
        {
            GameplayContentHash = manifest.GameplayContentHash,
            MapHash = manifest.MapHash,
            DeterministicSeed = deterministicSeed
        };
    }

    public ReplayLog Log => _log;

    public void RecordAcceptedCommand(CommandEnvelope command) => _log.Commands.Add(command);

    public void AfterTick(SimulationWorld world)
    {
        int tick = world.Tick.Value;
        if (tick <= 0) return;
        ulong hash = 0;
        if (tick % HashIntervalTicks == 0)
        {
            hash = StateHasher.Hash(world);
            _log.HashCheckpoints.Add(new ReplayHashCheckpoint(world.Tick, hash));
        }
        if (tick % SeekIntervalTicks == 0)
        {
            if (hash == 0) hash = StateHasher.Hash(world);
            _log.SeekCheckpoints.Add(new ReplaySeekCheckpoint(world.Tick, hash, SnapshotSerializer.Serialize(world)));
        }
    }

    public byte[] FinalizeAndSerialize(SimulationWorld world)
    {
        _log.FinalTick = world.Tick.Value;
        _log.ExpectedFinalHash = StateHasher.Hash(world);
        return _log.Serialize();
    }
}
}
