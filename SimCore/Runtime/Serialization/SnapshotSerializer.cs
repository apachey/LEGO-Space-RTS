using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public static class SnapshotSerializer
{
    public const uint Magic = 0x53525453; // STRS
    public const ushort FormatVersion = 2;
    public const ushort SimulationProtocolVersion = 1;

    public static byte[] Serialize(SimulationWorld world)
    {
        using MemoryStream ms = new(); using BinaryWriter w = new(ms);
        w.Write(Magic); w.Write(FormatVersion); w.Write(SimulationProtocolVersion); w.Write(world.Tick.Value);
        world.Map.Serialize(w);
        w.Write(world.Entities.NextEntityValue);
        IReadOnlyList<EntityId> alive = world.Entities.Alive; w.Write(alive.Count);
        for (int i = 0; i < alive.Count; i++) WriteEntity(w, world, alive[i]);
        world.Commands.Serialize(w);
        world.Fog.Serialize(w);
        w.Flush(); return ms.ToArray();
    }

    public static SimulationWorld Deserialize(byte[] bytes)
    {
        using MemoryStream ms = new(bytes, false); using BinaryReader r = new(ms);
        if (r.ReadUInt32() != Magic) throw new InvalidDataException("Snapshot magic mismatch.");
        ushort format = r.ReadUInt16(), protocol = r.ReadUInt16();
        if (format != FormatVersion || protocol != SimulationProtocolVersion) throw new InvalidDataException($"Unsupported snapshot {format}/{protocol}.");
        SimTick tick = new(r.ReadInt32()); MapGrid map = MapGrid.Deserialize(r); uint nextEntity = r.ReadUInt32();
        EntityStore entities = new(); int entityCount = r.ReadInt32(); if (entityCount < 0 || entityCount > 10000) throw new InvalidDataException("Invalid entity count.");
        SimulationWorld temp = new(map, entities, new FogState(2), tick);
        for (int i = 0; i < entityCount; i++) ReadEntity(r, temp);
        entities.RestoreNextEntityValue(nextEntity);
        temp.Commands.Deserialize(r);
        temp.Fog = FogState.Deserialize(r);
        if (ms.Position != ms.Length) throw new InvalidDataException("Trailing snapshot bytes.");
        temp.Spatial.Rebuild(temp.Entities);
        return temp;
    }

    private static void WriteEntity(BinaryWriter w, SimulationWorld world, EntityId id)
    {
        w.Write(id.Value);
        Ownership owner = world.Entities.Ownership.Get(id); w.Write(owner.PlayerSlot);
        SimTransform t = world.Entities.Transform.Get(id); w.Write(t.Position.X.Raw); w.Write(t.Position.Y.Raw); w.Write(t.Orientation.Raw);
        Movement m = world.Entities.Movement.Get(id);
        w.Write(m.MaxSpeed.Raw); w.Write(m.Acceleration.Raw); w.Write(m.Deceleration.Raw); w.Write(m.TurnRatePerTick); w.Write((byte)m.ReversePolicy);
        w.Write(m.CurrentSpeed.Raw); w.Write(m.CurrentVelocity.X.Raw); w.Write(m.CurrentVelocity.Y.Raw); w.Write(m.DesiredMovement.X.Raw); w.Write(m.DesiredMovement.Y.Raw);
        w.Write(m.PathIndex); w.Write((byte)m.State); w.Write(m.StuckTicks); w.Write(m.CompressionTicks); w.Write(m.LastPosition.X.Raw); w.Write(m.LastPosition.Y.Raw);
        NavigationAgent n = world.Entities.Navigation.Get(id); w.Write((byte)n.Footprint); w.Write((byte)n.Layer); w.Write(n.Target.X.Raw); w.Write(n.Target.Y.Raw); w.Write(n.HasTarget); w.Write(n.PathDirty); w.Write(n.PathTopologyVersion); w.Write(n.RequestAge); FormationIntentCodec.Write(w,n.Formation);
        Selectable s = world.Entities.Selectable.Get(id); w.Write(s.IsSelectable); w.Write(s.ContentType.Value); w.Write((byte)s.Kind);
        Vision v = world.Entities.Vision.Get(id); w.Write(v.RadiusBuildCells); w.Write(v.IsAirVision); w.Write(v.LastFogX); w.Write(v.LastFogY);
        world.GetQueue(id).Serialize(w);
        if (world.Corridors.TryGetValue(id.Value, out RouteCorridor path))
        {
            w.Write(true); w.Write(path.TopologyVersion); w.Write(path.Cells.Count);
            for (int p = 0; p < path.Cells.Count; p++) { w.Write(path.Cells[p].X); w.Write(path.Cells[p].Y); }
        }
        else w.Write(false);
    }

    private static void ReadEntity(BinaryReader r, SimulationWorld world)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        world.Entities.Transform.Set(id, new SimTransform { Position = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), Orientation = new Angle16(r.ReadUInt16()) });
        world.Entities.Movement.Set(id, new Movement
        {
            MaxSpeed=Fix32.FromRaw(r.ReadInt32()), Acceleration=Fix32.FromRaw(r.ReadInt32()), Deceleration=Fix32.FromRaw(r.ReadInt32()), TurnRatePerTick=r.ReadUInt16(), ReversePolicy=(ReversePolicy)r.ReadByte(),
            CurrentSpeed=Fix32.FromRaw(r.ReadInt32()), CurrentVelocity=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())), DesiredMovement=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),
            PathIndex=r.ReadInt32(), State=(MovementState)r.ReadByte(), StuckTicks=r.ReadInt32(), CompressionTicks=r.ReadInt32(), LastPosition=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32()))
        });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = (FootprintClass)r.ReadByte(), Layer=(MovementLayer)r.ReadByte(), Target = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), HasTarget = r.ReadBoolean(), PathDirty = r.ReadBoolean(), PathTopologyVersion = r.ReadInt32(), RequestAge = r.ReadInt32(), Formation=FormationIntentCodec.Read(r) });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        world.GetQueue(id).Deserialize(r);
        if (r.ReadBoolean())
        {
            RouteCorridor path = new() { TopologyVersion = r.ReadInt32() }; int count = r.ReadInt32(); if (count < 0 || count > 20000) throw new InvalidDataException("Invalid path count.");
            for (int i = 0; i < count; i++) path.Cells.Add(new NavCell(r.ReadInt16(), r.ReadInt16())); world.Corridors[id.Value] = path;
        }
    }
}

public static class StateHasher
{
    public static ulong Hash(SimulationWorld world) => DeterministicHash.Fnv1A64(SnapshotSerializer.Serialize(world));
    public static string HashHex(SimulationWorld world) => Hash(world).ToString("X16");
}
}
