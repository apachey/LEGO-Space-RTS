using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public sealed class ReplayLog
{
    public const uint Magic = 0x52545253; // RTRS
    public const ushort Version = 15;
    public byte[] InitialSnapshot { get; }
    public List<CommandEnvelope> Commands { get; } = new();
    public ulong ExpectedFinalHash { get; set; }

    public ReplayLog(byte[] initialSnapshot) => InitialSnapshot = initialSnapshot;

    public byte[] Serialize()
    {
        using MemoryStream ms = new(); using BinaryWriter w = new(ms);
        w.Write(Magic); w.Write(Version); w.Write(InitialSnapshot.Length); w.Write(InitialSnapshot); w.Write(Commands.Count);
        Commands.Sort((a, b) => { int c = a.ExecutionTick.Value.CompareTo(b.ExecutionTick.Value); if (c != 0) return c; c = a.PlayerSlot.CompareTo(b.PlayerSlot); return c != 0 ? c : a.Sequence.CompareTo(b.Sequence); });
        for (int i = 0; i < Commands.Count; i++) Commands[i].Write(w); w.Write(ExpectedFinalHash); w.Flush(); return ms.ToArray();
    }

    public static ReplayLog Deserialize(byte[] bytes)
    {
        using MemoryStream ms = new(bytes, false); using BinaryReader r = new(ms);
        if (r.ReadUInt32() != Magic) throw new InvalidDataException("Replay magic mismatch.");
        ushort version = r.ReadUInt16(); if (version < 1 || (version > 6 && version != Version)) throw new InvalidDataException("Replay version mismatch.");
        int len = r.ReadInt32(); if (len < 0 || len > 64 * 1024 * 1024) throw new InvalidDataException("Invalid replay snapshot size.");
        ReplayLog log = new(r.ReadBytes(len)); int count = r.ReadInt32(); if (count < 0 || count > 1_000_000) throw new InvalidDataException("Invalid replay command count.");
        for (int i = 0; i < count; i++) log.Commands.Add(CommandEnvelope.Read(r, includeBuildFields: version >= 2, includeEnergyPriority: version >= 5, includeMissionRefit: version >= 15, includeResonanceCommitment: version >= 15)); log.ExpectedFinalHash = r.ReadUInt64(); return log;
    }

    public SimulationRunner CreateRunner()
    {
        SimulationWorld world = SnapshotSerializer.Deserialize(InitialSnapshot);
        for (int i = 0; i < Commands.Count; i++) world.Commands.Enqueue(Commands[i]);
        return new SimulationRunner(world);
    }
}
}
