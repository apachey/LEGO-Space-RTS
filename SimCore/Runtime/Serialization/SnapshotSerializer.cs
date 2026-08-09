using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public static class SnapshotSerializer
{
    public const uint Magic = 0x53525453; // STRS
    public const ushort FormatVersion = 5;
    public const ushort SimulationProtocolVersion = 3;

    [Flags]
    private enum EntityComponents : ushort
    {
        None = 0,
        Ownership = 1 << 0,
        Transform = 1 << 1,
        Movement = 1 << 2,
        Navigation = 1 << 3,
        Selectable = 1 << 4,
        Vision = 1 << 5,
        ResourceNode = 1 << 6,
        CommandQueue = 1 << 7,
        RouteCorridor = 1 << 8,
        Worker = 1 << 9,
        ResourceCarrier = 1 << 10,
        ResourceReceiver = 1 << 11,
        ResourceBank = 1 << 12,
        All = Ownership | Transform | Movement | Navigation | Selectable | Vision | ResourceNode | CommandQueue | RouteCorridor | Worker | ResourceCarrier | ResourceReceiver | ResourceBank
    }

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
        bool supportedLegacy = ((format == 2 || format == 3) && protocol == 1) || (format == 4 && protocol == 2);
        if (!supportedLegacy && (format != FormatVersion || protocol != SimulationProtocolVersion)) throw new InvalidDataException($"Unsupported snapshot {format}/{protocol}.");
        SimTick tick = new(r.ReadInt32()); MapGrid map = MapGrid.Deserialize(r); uint nextEntity = r.ReadUInt32();
        EntityStore entities = new(); int entityCount = r.ReadInt32(); if (entityCount < 0 || entityCount > 10000) throw new InvalidDataException("Invalid entity count.");
        SimulationWorld temp = new(map, entities, new FogState(2), tick);
        for (int i = 0; i < entityCount; i++)
        {
            if (format == 2) ReadEntityV2(r, temp);
            else ReadEntity(r, temp, format);
        }
        if (format < 5) AddLegacyResourceBanks(entities);
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
        EntityComponents components = EntityComponents.None;
        if (world.Entities.Ownership.Has(id)) components |= EntityComponents.Ownership;
        if (world.Entities.Transform.Has(id)) components |= EntityComponents.Transform;
        if (world.Entities.Movement.Has(id)) components |= EntityComponents.Movement;
        if (world.Entities.Navigation.Has(id)) components |= EntityComponents.Navigation;
        if (world.Entities.Selectable.Has(id)) components |= EntityComponents.Selectable;
        if (world.Entities.Vision.Has(id)) components |= EntityComponents.Vision;
        if (world.Entities.ResourceNode.Has(id)) components |= EntityComponents.ResourceNode;
        if (world.Entities.Worker.Has(id)) components |= EntityComponents.Worker;
        if (world.Entities.ResourceCarrier.Has(id)) components |= EntityComponents.ResourceCarrier;
        if (world.Entities.ResourceReceiver.Has(id)) components |= EntityComponents.ResourceReceiver;
        if (world.Entities.ResourceBank.Has(id)) components |= EntityComponents.ResourceBank;
        if (world.TryGetQueue(id, out _)) components |= EntityComponents.CommandQueue;
        if (world.Corridors.ContainsKey(id.Value)) components |= EntityComponents.RouteCorridor;
        w.Write((ushort)components);

        if ((components & EntityComponents.Ownership) != 0) w.Write(world.Entities.Ownership.Get(id).PlayerSlot);
        if ((components & EntityComponents.Transform) != 0) WriteTransform(w, world.Entities.Transform.Get(id));
        if ((components & EntityComponents.Movement) != 0) WriteMovement(w, world.Entities.Movement.Get(id));
        if ((components & EntityComponents.Navigation) != 0) WriteNavigation(w, world.Entities.Navigation.Get(id));
        if ((components & EntityComponents.Selectable) != 0)
        {
            Selectable s = world.Entities.Selectable.Get(id); w.Write(s.IsSelectable); w.Write(s.ContentType.Value); w.Write((byte)s.Kind);
        }
        if ((components & EntityComponents.Vision) != 0)
        {
            Vision v = world.Entities.Vision.Get(id); w.Write(v.RadiusBuildCells); w.Write(v.IsAirVision); w.Write(v.LastFogX); w.Write(v.LastFogY);
        }
        if ((components & EntityComponents.ResourceNode) != 0) WriteResourceNode(w, world.Entities.ResourceNode.Get(id));
        if ((components & EntityComponents.Worker) != 0) WriteWorker(w, world.Entities.Worker.Get(id));
        if ((components & EntityComponents.ResourceCarrier) != 0) WriteResourceCarrier(w, world.Entities.ResourceCarrier.Get(id));
        if ((components & EntityComponents.ResourceReceiver) != 0) WriteResourceReceiver(w, world.Entities.ResourceReceiver.Get(id));
        if ((components & EntityComponents.ResourceBank) != 0) WriteResourceBank(w, world.Entities.ResourceBank.Get(id));
        if ((components & EntityComponents.CommandQueue) != 0) world.GetQueue(id).Serialize(w, includeTargetEntity: true);
        if ((components & EntityComponents.RouteCorridor) != 0) WriteCorridor(w, world.Corridors[id.Value]);
    }

    private static void WriteTransform(BinaryWriter w, SimTransform t)
    {
        w.Write(t.Position.X.Raw); w.Write(t.Position.Y.Raw); w.Write(t.Orientation.Raw);
    }

    private static void WriteMovement(BinaryWriter w, Movement m)
    {
        w.Write(m.MaxSpeed.Raw); w.Write(m.Acceleration.Raw); w.Write(m.Deceleration.Raw); w.Write(m.TurnRatePerTick); w.Write((byte)m.ReversePolicy);
        w.Write(m.CurrentSpeed.Raw); w.Write(m.CurrentVelocity.X.Raw); w.Write(m.CurrentVelocity.Y.Raw); w.Write(m.DesiredMovement.X.Raw); w.Write(m.DesiredMovement.Y.Raw);
        w.Write(m.PathIndex); w.Write((byte)m.State); w.Write(m.StuckTicks); w.Write(m.CompressionTicks); w.Write(m.LastPosition.X.Raw); w.Write(m.LastPosition.Y.Raw);
    }

    private static void WriteNavigation(BinaryWriter w, NavigationAgent n)
    {
        w.Write((byte)n.Footprint); w.Write((byte)n.Layer); w.Write(n.Target.X.Raw); w.Write(n.Target.Y.Raw); w.Write(n.HasTarget); w.Write(n.PathDirty); w.Write(n.PathTopologyVersion); w.Write(n.RequestAge); FormationIntentCodec.Write(w,n.Formation);
    }

    private static void WriteResourceNode(BinaryWriter w, ResourceNode n)
    {
        w.Write((byte)n.Type); w.Write((byte)n.DepositSize); w.Write((byte)n.HarvestInteraction); w.Write((byte)n.DepletionProfile);
        w.Write(n.Capacity); w.Write(n.Remaining); w.Write(n.ReducedThresholdBasisPoints); w.Write(n.LowThresholdBasisPoints); w.Write(n.CriticalThresholdBasisPoints);
    }

    private static void WriteWorker(BinaryWriter w, Worker worker)
    {
        w.Write(worker.ResourceTarget.Value); w.Write(worker.ReceiverTarget.Value); w.Write((byte)worker.TaskState); w.Write(worker.ExtractionTicks); w.Write(worker.TicksPerOre);
    }

    private static void WriteResourceCarrier(BinaryWriter w, ResourceCarrier carrier)
    {
        w.Write((byte)carrier.Type); w.Write(carrier.Amount); w.Write(carrier.Capacity);
    }

    private static void WriteResourceReceiver(BinaryWriter w, ResourceReceiver receiver)
    {
        w.Write((byte)receiver.AcceptedType); w.Write(receiver.PendingHauledAmount); w.Write(receiver.IsHqEmergencyReceiver);
    }

    private static void WriteResourceBank(BinaryWriter w, ResourceBank bank)
    {
        w.Write((byte)bank.Type); w.Write(bank.ProcessedAmount);
    }

    private static void WriteCorridor(BinaryWriter w, RouteCorridor path)
    {
        w.Write(path.TopologyVersion); w.Write(path.Cells.Count);
        for (int p = 0; p < path.Cells.Count; p++) { w.Write(path.Cells[p].X); w.Write(path.Cells[p].Y); }
    }

    private static void ReadEntity(BinaryReader r, SimulationWorld world, ushort format)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        EntityComponents components = (EntityComponents)r.ReadUInt16();
        if ((components & ~EntityComponents.All) != 0) throw new InvalidDataException("Snapshot entity has unknown component bits.");
        if ((components & EntityComponents.Ownership) != 0) world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        if ((components & EntityComponents.Transform) != 0) world.Entities.Transform.Set(id, ReadTransform(r));
        if ((components & EntityComponents.Movement) != 0) world.Entities.Movement.Set(id, ReadMovement(r));
        if ((components & EntityComponents.Navigation) != 0) world.Entities.Navigation.Set(id, ReadNavigation(r));
        if ((components & EntityComponents.Selectable) != 0) world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        if ((components & EntityComponents.Vision) != 0) world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        if ((components & EntityComponents.ResourceNode) != 0) world.Entities.ResourceNode.Set(id, ReadResourceNode(r));
        if ((components & EntityComponents.Worker) != 0) world.Entities.Worker.Set(id, ReadWorker(r));
        if ((components & EntityComponents.ResourceCarrier) != 0) world.Entities.ResourceCarrier.Set(id, ReadResourceCarrier(r));
        if ((components & EntityComponents.ResourceReceiver) != 0) world.Entities.ResourceReceiver.Set(id, ReadResourceReceiver(r));
        if ((components & EntityComponents.ResourceBank) != 0) world.Entities.ResourceBank.Set(id, ReadResourceBank(r));
        if ((components & EntityComponents.CommandQueue) != 0) world.GetQueue(id).Deserialize(r, includeTargetEntity: format >= 4);
        if ((components & EntityComponents.RouteCorridor) != 0) world.Corridors[id.Value] = ReadCorridor(r);
    }

    private static SimTransform ReadTransform(BinaryReader r)
        => new() { Position = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), Orientation = new Angle16(r.ReadUInt16()) };

    private static Movement ReadMovement(BinaryReader r)
        => new()
        {
            MaxSpeed=Fix32.FromRaw(r.ReadInt32()), Acceleration=Fix32.FromRaw(r.ReadInt32()), Deceleration=Fix32.FromRaw(r.ReadInt32()), TurnRatePerTick=r.ReadUInt16(), ReversePolicy=(ReversePolicy)r.ReadByte(),
            CurrentSpeed=Fix32.FromRaw(r.ReadInt32()), CurrentVelocity=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())), DesiredMovement=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),
            PathIndex=r.ReadInt32(), State=(MovementState)r.ReadByte(), StuckTicks=r.ReadInt32(), CompressionTicks=r.ReadInt32(), LastPosition=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32()))
        };

    private static NavigationAgent ReadNavigation(BinaryReader r)
        => new() { Footprint = (FootprintClass)r.ReadByte(), Layer=(MovementLayer)r.ReadByte(), Target = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), HasTarget = r.ReadBoolean(), PathDirty = r.ReadBoolean(), PathTopologyVersion = r.ReadInt32(), RequestAge = r.ReadInt32(), Formation=FormationIntentCodec.Read(r) };

    private static ResourceNode ReadResourceNode(BinaryReader r)
        => new()
        {
            Type=(ResourceType)r.ReadByte(), DepositSize=(ResourceDepositSize)r.ReadByte(), HarvestInteraction=(HarvestInteraction)r.ReadByte(), DepletionProfile=(ResourceDepletionProfile)r.ReadByte(),
            Capacity=r.ReadInt32(), Remaining=r.ReadInt32(), ReducedThresholdBasisPoints=r.ReadUInt16(), LowThresholdBasisPoints=r.ReadUInt16(), CriticalThresholdBasisPoints=r.ReadUInt16()
        };

    private static Worker ReadWorker(BinaryReader r)
        => new() { ResourceTarget = new EntityId(r.ReadUInt32()), ReceiverTarget = new EntityId(r.ReadUInt32()), TaskState = (WorkerTaskState)r.ReadByte(), ExtractionTicks = r.ReadUInt16(), TicksPerOre = r.ReadUInt16() };

    private static ResourceCarrier ReadResourceCarrier(BinaryReader r)
        => new() { Type = (ResourceType)r.ReadByte(), Amount = r.ReadByte(), Capacity = r.ReadByte() };

    private static ResourceReceiver ReadResourceReceiver(BinaryReader r)
        => new() { AcceptedType = (ResourceType)r.ReadByte(), PendingHauledAmount = r.ReadInt32(), IsHqEmergencyReceiver = r.ReadBoolean() };

    private static ResourceBank ReadResourceBank(BinaryReader r)
        => new() { Type = (ResourceType)r.ReadByte(), ProcessedAmount = r.ReadInt32() };

    private static void AddLegacyResourceBanks(EntityStore entities)
    {
        IReadOnlyList<EntityId> alive = entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver)) continue;
            entities.ResourceBank.Set(id, new ResourceBank { Type = receiver.AcceptedType, ProcessedAmount = 0 });
        }
    }

    private static RouteCorridor ReadCorridor(BinaryReader r)
    {
        RouteCorridor path = new() { TopologyVersion = r.ReadInt32() }; int count = r.ReadInt32(); if (count < 0 || count > 20000) throw new InvalidDataException("Invalid path count.");
        for (int i = 0; i < count; i++) path.Cells.Add(new NavCell(r.ReadInt16(), r.ReadInt16()));
        return path;
    }

    private static void ReadEntityV2(BinaryReader r, SimulationWorld world)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        world.Entities.Transform.Set(id, ReadTransform(r));
        world.Entities.Movement.Set(id, ReadMovement(r));
        world.Entities.Navigation.Set(id, ReadNavigation(r));
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        world.GetQueue(id).Deserialize(r, includeTargetEntity: false);
        if (r.ReadBoolean()) world.Corridors[id.Value] = ReadCorridor(r);
    }
}

public static class StateHasher
{
    public static ulong Hash(SimulationWorld world) => DeterministicHash.Fnv1A64(SnapshotSerializer.Serialize(world));
    public static string HashHex(SimulationWorld world) => Hash(world).ToString("X16");
}
}
