using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct NetworkMatchManifest
{
    public readonly ushort SimulationProtocolVersion;
    public readonly ulong GameplayContentHash;
    public readonly ulong MapHash;

    public NetworkMatchManifest(ulong gameplayContentHash, ulong mapHash)
        : this(SnapshotSerializer.SimulationProtocolVersion, gameplayContentHash, mapHash)
    {
    }

    internal NetworkMatchManifest(ushort simulationProtocolVersion, ulong gameplayContentHash, ulong mapHash)
    {
        SimulationProtocolVersion = simulationProtocolVersion;
        GameplayContentHash = gameplayContentHash;
        MapHash = mapHash;
    }

    public bool Matches(NetworkMatchManifest other) => SimulationProtocolVersion == other.SimulationProtocolVersion &&
        GameplayContentHash == other.GameplayContentHash && MapHash == other.MapHash;

    public static ulong ComputeMapHash(MapGrid map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, true)) map.Serialize(writer);
        return DeterministicHash.Fnv1A64(stream.ToArray());
    }
}

public enum NetworkReconnectRejection : byte
{
    None = 0,
    UnknownOrExpiredSession = 1,
    ManifestMismatch = 2,
    PlayerSlotUnavailable = 3,
    MalformedRequest = 4
}

public readonly struct NetworkReconnectRequest
{
    public readonly ulong SessionToken;
    public readonly byte PlayerSlot;
    public readonly NetworkMatchManifest Manifest;

    public NetworkReconnectRequest(ulong sessionToken, byte playerSlot, NetworkMatchManifest manifest)
    {
        SessionToken = sessionToken;
        PlayerSlot = playerSlot;
        Manifest = manifest;
    }
}

public readonly struct NetworkOwnedOrderQueue
{
    public readonly EntityId EntityId;
    public readonly byte[] SerializedOrders;

    public NetworkOwnedOrderQueue(EntityId entityId, byte[] serializedOrders)
    {
        EntityId = entityId;
        SerializedOrders = serializedOrders;
    }
}

public readonly struct NetworkProductionItem
{
    public readonly ContentId UnitType;
    public readonly ushort TotalTicks;
    public readonly ushort RemainingTicks;
    public readonly ushort ReservedOre;
    public readonly ushort RequiredEnergy;
    public readonly byte RequiredCrystals;
    public readonly byte ReservedOperationsCapacity;

    public NetworkProductionItem(ProductionQueueItem item)
    {
        UnitType = item.UnitType;
        TotalTicks = item.TotalTicks;
        RemainingTicks = item.RemainingTicks;
        ReservedOre = item.ReservedOre;
        RequiredEnergy = item.RequiredEnergy;
        RequiredCrystals = item.RequiredCrystals;
        ReservedOperationsCapacity = item.ReservedOperationsCapacity;
    }

    internal NetworkProductionItem(ContentId unitType, ushort totalTicks, ushort remainingTicks, ushort reservedOre,
        ushort requiredEnergy, byte requiredCrystals, byte reservedOperationsCapacity)
    {
        UnitType = unitType; TotalTicks = totalTicks; RemainingTicks = remainingTicks; ReservedOre = reservedOre;
        RequiredEnergy = requiredEnergy; RequiredCrystals = requiredCrystals; ReservedOperationsCapacity = reservedOperationsCapacity;
    }
}

public readonly struct NetworkOwnedProductionQueue
{
    public readonly EntityId Producer;
    public readonly bool SpawnBlocked;
    public readonly bool HasRallyPoint;
    public readonly FixVec2 RallyPoint;
    public readonly EntityId RallyTargetEntity;
    public readonly NetworkProductionItem[] Items;

    public NetworkOwnedProductionQueue(EntityId producer, Production production)
    {
        Producer = producer; SpawnBlocked = production.SpawnBlocked; HasRallyPoint = production.HasRallyPoint;
        RallyPoint = production.RallyPoint; RallyTargetEntity = production.RallyTargetEntity;
        Items = new NetworkProductionItem[production.Count];
        for (int i = 0; i < Items.Length; i++) Items[i] = new NetworkProductionItem(production.Get(i));
    }

    internal NetworkOwnedProductionQueue(EntityId producer, bool spawnBlocked, bool hasRallyPoint, FixVec2 rallyPoint,
        EntityId rallyTargetEntity, NetworkProductionItem[] items)
    {
        Producer = producer; SpawnBlocked = spawnBlocked; HasRallyPoint = hasRallyPoint; RallyPoint = rallyPoint;
        RallyTargetEntity = rallyTargetEntity; Items = items;
    }
}

public sealed class NetworkReconnectState
{
    public NetworkReconnectRejection Rejection { get; set; }
    public ulong SessionToken { get; set; }
    public byte PlayerSlot { get; set; }
    public uint LastProcessedCommandSequence { get; set; }
    public NetworkMatchManifest Manifest { get; set; }
    public byte[] FullLegalSnapshotPacket { get; set; } = Array.Empty<byte>();
    public CommandEnvelope[] PendingCommands { get; set; } = Array.Empty<CommandEnvelope>();
    public NetworkOwnedOrderQueue[] OwnedOrderQueues { get; set; } = Array.Empty<NetworkOwnedOrderQueue>();
    public NetworkOwnedProductionQueue[] OwnedProductionQueues { get; set; } = Array.Empty<NetworkOwnedProductionQueue>();
    public bool Accepted => Rejection == NetworkReconnectRejection.None;
}

/// <summary>Reliable T062 authenticate/manifest/full-legal-state reconnect format.</summary>
public static class NetworkReconnectProtocol
{
    public const ushort FormatVersion = 1;
    public const int MaximumPacketBytes = 64 * 1024;
    private const uint RequestMagic = 0x5152434C; // LCRQ
    private const uint StateMagic = 0x5352434C; // LCRS
    private const int MaximumPendingCommands = 1024;
    private const int MaximumOwnedQueues = 16_384;

    public static byte[] EncodeRequest(NetworkReconnectRequest request)
    {
        if (request.SessionToken == 0) throw new ArgumentOutOfRangeException(nameof(request));
        using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
        WriteHeader(writer, RequestMagic);
        writer.Write(request.SessionToken); writer.Write(request.PlayerSlot); WriteManifest(writer, request.Manifest);
        writer.Flush(); return stream.ToArray();
    }

    public static bool TryDecodeRequest(byte[] packet, out NetworkReconnectRequest request)
    {
        request = default;
        if (packet == null || packet.Length > MaximumPacketBytes) return false;
        try
        {
            using MemoryStream stream = new(packet, false); using BinaryReader reader = new(stream);
            if (!ReadHeader(reader, RequestMagic)) return false;
            ulong token = reader.ReadUInt64(); byte player = reader.ReadByte(); NetworkMatchManifest manifest = ReadManifest(reader);
            if (token == 0 || stream.Position != stream.Length) return false;
            request = new NetworkReconnectRequest(token, player, manifest); return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
    }

    public static NetworkReconnectState CaptureAccepted(SimulationWorld world, ServerCommandSession session,
        NetworkMatchManifest manifest, byte[] fullLegalSnapshotPacket)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (session == null) throw new ArgumentNullException(nameof(session));
        List<CommandEnvelope> pending = new();
        IReadOnlyList<CommandEnvelope> commands = world.Commands.All;
        for (int i = 0; i < commands.Count; i++) if (commands[i].PlayerSlot == session.PlayerSlot) pending.Add(commands[i]);

        List<NetworkOwnedOrderQueue> orderQueues = new();
        List<NetworkOwnedProductionQueue> productionQueues = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != session.PlayerSlot) continue;
            if (world.TryGetQueue(id, out UnitCommandQueue queue) && queue.Count > 0)
            {
                using MemoryStream queueStream = new(); using BinaryWriter queueWriter = new(queueStream);
                queue.Serialize(queueWriter); queueWriter.Flush();
                orderQueues.Add(new NetworkOwnedOrderQueue(id, queueStream.ToArray()));
            }
            if (world.Entities.Production.TryGet(id, out Production production)) productionQueues.Add(new NetworkOwnedProductionQueue(id, production));
        }

        return new NetworkReconnectState
        {
            Rejection = NetworkReconnectRejection.None,
            SessionToken = session.SessionToken,
            PlayerSlot = session.PlayerSlot,
            LastProcessedCommandSequence = session.LastProcessedSequence,
            Manifest = manifest,
            FullLegalSnapshotPacket = fullLegalSnapshotPacket,
            PendingCommands = pending.ToArray(),
            OwnedOrderQueues = orderQueues.ToArray(),
            OwnedProductionQueues = productionQueues.ToArray()
        };
    }

    public static byte[] EncodeState(NetworkReconnectState state)
    {
        using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
        WriteHeader(writer, StateMagic); writer.Write((byte)state.Rejection);
        writer.Write(state.SessionToken); writer.Write(state.PlayerSlot); writer.Write(state.LastProcessedCommandSequence); WriteManifest(writer, state.Manifest);
        WriteBytes(writer, state.FullLegalSnapshotPacket);
        writer.Write(state.PendingCommands.Length); for (int i = 0; i < state.PendingCommands.Length; i++) state.PendingCommands[i].Write(writer);
        writer.Write(state.OwnedOrderQueues.Length);
        for (int i = 0; i < state.OwnedOrderQueues.Length; i++)
        {
            writer.Write(state.OwnedOrderQueues[i].EntityId.Value); WriteBytes(writer, state.OwnedOrderQueues[i].SerializedOrders);
        }
        writer.Write(state.OwnedProductionQueues.Length);
        for (int i = 0; i < state.OwnedProductionQueues.Length; i++) WriteProduction(writer, state.OwnedProductionQueues[i]);
        writer.Flush();
        if (stream.Length > MaximumPacketBytes) throw new InvalidOperationException("Reconnect state exceeded the reliable-bulk packet bound.");
        return stream.ToArray();
    }

    public static bool TryDecodeState(byte[] packet, out NetworkReconnectState state)
    {
        state = new NetworkReconnectState();
        if (packet == null || packet.Length > MaximumPacketBytes) return false;
        try
        {
            using MemoryStream stream = new(packet, false); using BinaryReader reader = new(stream);
            if (!ReadHeader(reader, StateMagic)) return false;
            NetworkReconnectRejection rejection = (NetworkReconnectRejection)reader.ReadByte();
            if (!Enum.IsDefined(typeof(NetworkReconnectRejection), rejection)) return false;
            state = new NetworkReconnectState
            {
                Rejection = rejection,
                SessionToken = reader.ReadUInt64(),
                PlayerSlot = reader.ReadByte(),
                LastProcessedCommandSequence = reader.ReadUInt32(),
                Manifest = ReadManifest(reader),
                FullLegalSnapshotPacket = ReadBytes(reader, MaximumPacketBytes),
                PendingCommands = ReadCommands(reader),
                OwnedOrderQueues = ReadOrderQueues(reader),
                OwnedProductionQueues = ReadProductionQueues(reader)
            };
            if (state.Accepted && (state.SessionToken == 0 || state.FullLegalSnapshotPacket.Length == 0)) return false;
            return stream.Position == stream.Length;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentException) { return false; }
        catch (OverflowException) { return false; }
    }

    public static NetworkReconnectState Rejected(NetworkReconnectRejection rejection, NetworkMatchManifest manifest) => new()
    {
        Rejection = rejection,
        Manifest = manifest
    };

    private static void WriteHeader(BinaryWriter writer, uint magic)
    {
        writer.Write(magic); writer.Write(FormatVersion); writer.Write(SnapshotSerializer.SimulationProtocolVersion);
    }

    private static bool ReadHeader(BinaryReader reader, uint magic) => reader.ReadUInt32() == magic && reader.ReadUInt16() == FormatVersion &&
        reader.ReadUInt16() == SnapshotSerializer.SimulationProtocolVersion;

    private static void WriteManifest(BinaryWriter writer, NetworkMatchManifest manifest)
    {
        writer.Write(manifest.SimulationProtocolVersion); writer.Write(manifest.GameplayContentHash); writer.Write(manifest.MapHash);
    }

    private static NetworkMatchManifest ReadManifest(BinaryReader reader)
    {
        ushort protocol = reader.ReadUInt16(); ulong content = reader.ReadUInt64(); ulong map = reader.ReadUInt64();
        return new NetworkMatchManifest(protocol, content, map);
    }

    private static void WriteBytes(BinaryWriter writer, byte[] bytes)
    {
        writer.Write(bytes.Length); writer.Write(bytes);
    }

    private static byte[] ReadBytes(BinaryReader reader, int maximum)
    {
        int length = reader.ReadInt32(); if (length < 0 || length > maximum) throw new InvalidDataException("Invalid reconnect payload size.");
        byte[] bytes = reader.ReadBytes(length); if (bytes.Length != length) throw new EndOfStreamException(); return bytes;
    }

    private static CommandEnvelope[] ReadCommands(BinaryReader reader)
    {
        int count = ReadCount(reader, MaximumPendingCommands); CommandEnvelope[] commands = new CommandEnvelope[count];
        for (int i = 0; i < count; i++) commands[i] = CommandEnvelope.Read(reader); return commands;
    }

    private static NetworkOwnedOrderQueue[] ReadOrderQueues(BinaryReader reader)
    {
        int count = ReadCount(reader, MaximumOwnedQueues); NetworkOwnedOrderQueue[] queues = new NetworkOwnedOrderQueue[count]; uint previous = 0;
        for (int i = 0; i < count; i++)
        {
            uint id = reader.ReadUInt32(); if (id <= previous) throw new InvalidDataException("Invalid owned order queue order.");
            queues[i] = new NetworkOwnedOrderQueue(new EntityId(id), ReadBytes(reader, 16 * 1024)); previous = id;
        }
        return queues;
    }

    private static void WriteProduction(BinaryWriter writer, NetworkOwnedProductionQueue queue)
    {
        writer.Write(queue.Producer.Value); writer.Write(queue.SpawnBlocked); writer.Write(queue.HasRallyPoint);
        writer.Write(queue.RallyPoint.X.Raw); writer.Write(queue.RallyPoint.Y.Raw); writer.Write(queue.RallyTargetEntity.Value); writer.Write(queue.Items.Length);
        for (int i = 0; i < queue.Items.Length; i++)
        {
            NetworkProductionItem item = queue.Items[i]; writer.Write(item.UnitType.Value); writer.Write(item.TotalTicks); writer.Write(item.RemainingTicks);
            writer.Write(item.ReservedOre); writer.Write(item.RequiredEnergy); writer.Write(item.RequiredCrystals); writer.Write(item.ReservedOperationsCapacity);
        }
    }

    private static NetworkOwnedProductionQueue[] ReadProductionQueues(BinaryReader reader)
    {
        int count = ReadCount(reader, MaximumOwnedQueues); NetworkOwnedProductionQueue[] queues = new NetworkOwnedProductionQueue[count]; uint previous = 0;
        for (int i = 0; i < count; i++)
        {
            uint id = reader.ReadUInt32(); if (id <= previous) throw new InvalidDataException("Invalid production queue order.");
            bool blocked = reader.ReadBoolean(), hasRally = reader.ReadBoolean();
            FixVec2 rally = new(Fix32.FromRaw(reader.ReadInt32()), Fix32.FromRaw(reader.ReadInt32())); EntityId rallyTarget = new(reader.ReadUInt32());
            int itemCount = ReadCount(reader, Production.Capacity); NetworkProductionItem[] items = new NetworkProductionItem[itemCount];
            for (int item = 0; item < itemCount; item++) items[item] = new NetworkProductionItem(new ContentId(reader.ReadUInt32()), reader.ReadUInt16(),
                reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadByte(), reader.ReadByte());
            queues[i] = new NetworkOwnedProductionQueue(new EntityId(id), blocked, hasRally, rally, rallyTarget, items); previous = id;
        }
        return queues;
    }

    private static int ReadCount(BinaryReader reader, int maximum)
    {
        int count = reader.ReadInt32(); if (count < 0 || count > maximum) throw new InvalidDataException("Invalid reconnect record count."); return count;
    }
}
}
