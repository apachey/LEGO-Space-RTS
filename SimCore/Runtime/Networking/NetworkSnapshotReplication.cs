using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace LegoSpaceRTS.SimCore
{
public readonly struct NetworkOwnPlayerState : IEquatable<NetworkOwnPlayerState>
{
    public readonly int Ore;
    public readonly int Crystal;
    public readonly int OperationsActive;
    public readonly int OperationsReserved;
    public readonly int OperationsMaximum;
    public readonly int AlienChargeCurrent;
    public readonly int AlienChargeMaximum;
    public readonly int AlienChargeGenerationPerSecond;
    public readonly bool ResonanceInitiationUnlocked;

    public NetworkOwnPlayerState(int ore, int crystal, OperationsCapacityState operations, AlienChargeState charge)
    {
        Ore = ore;
        Crystal = crystal;
        OperationsActive = operations.Active;
        OperationsReserved = operations.Reserved;
        OperationsMaximum = operations.Maximum;
        AlienChargeCurrent = charge.CurrentMillicharge;
        AlienChargeMaximum = charge.MaximumMillicharge;
        AlienChargeGenerationPerSecond = charge.GenerationMillichargePerSecond;
        ResonanceInitiationUnlocked = charge.ResonanceInitiationUnlocked;
    }

    public bool Equals(NetworkOwnPlayerState other) => Ore == other.Ore && Crystal == other.Crystal &&
        OperationsActive == other.OperationsActive && OperationsReserved == other.OperationsReserved && OperationsMaximum == other.OperationsMaximum &&
        AlienChargeCurrent == other.AlienChargeCurrent && AlienChargeMaximum == other.AlienChargeMaximum &&
        AlienChargeGenerationPerSecond == other.AlienChargeGenerationPerSecond && ResonanceInitiationUnlocked == other.ResonanceInitiationUnlocked;
    public override bool Equals(object? obj) => obj is NetworkOwnPlayerState other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Ore, Crystal, OperationsActive, OperationsReserved, OperationsMaximum,
        AlienChargeCurrent, AlienChargeMaximum, AlienChargeGenerationPerSecond);
}

public readonly struct NetworkReplicatedEntity
{
    public readonly EntityId EntityId;
    public readonly byte[] State;

    public NetworkReplicatedEntity(EntityId entityId, byte[] state)
    {
        if (entityId == EntityId.None) throw new ArgumentOutOfRangeException(nameof(entityId));
        if (state == null) throw new ArgumentNullException(nameof(state));
        EntityId = entityId;
        State = state;
    }

    public PresentationEntity Decode() => NetworkSnapshotProtocol.DecodePresentationEntity(EntityId, State);
}

public readonly struct NetworkReplicatedProjectile
{
    public readonly ProjectileId ProjectileId;
    public readonly byte Owner;
    public readonly FixVec2 Position;

    public NetworkReplicatedProjectile(ProjectileId projectileId, byte owner, FixVec2 position)
    {
        ProjectileId = projectileId;
        Owner = owner;
        Position = position;
    }
}

/// <summary>A recipient-specific, presentation-only state. It can never recreate SimulationWorld.</summary>
public sealed class NetworkSnapshotState
{
    public const int KnowledgeBitsetBytes = (FogState.Width * FogState.Height + 7) / 8;
    public NetworkSnapshotState(SimTick tick, byte playerSlot, NetworkOwnPlayerState ownPlayer,
        NetworkReplicatedEntity[] entities, NetworkReplicatedProjectile[] projectiles, byte[] exploredBits, byte[] visibleBits)
    {
        Tick = tick;
        PlayerSlot = playerSlot;
        OwnPlayer = ownPlayer;
        Entities = entities ?? throw new ArgumentNullException(nameof(entities));
        Projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
        ExploredBits = exploredBits ?? throw new ArgumentNullException(nameof(exploredBits));
        VisibleBits = visibleBits ?? throw new ArgumentNullException(nameof(visibleBits));
        ValidateStableOrder();
    }

    public SimTick Tick { get; }
    public byte PlayerSlot { get; }
    public NetworkOwnPlayerState OwnPlayer { get; }
    public NetworkReplicatedEntity[] Entities { get; }
    public NetworkReplicatedProjectile[] Projectiles { get; }
    public byte[] ExploredBits { get; }
    public byte[] VisibleBits { get; }

    public static NetworkSnapshotState Capture(SimulationWorld world, byte playerSlot)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (playerSlot >= world.PlayerCount) throw new ArgumentOutOfRangeException(nameof(playerSlot));
        PresentationSnapshot presentation = PresentationSnapshot.Capture(world, playerSlot);
        NetworkReplicatedEntity[] entities = new NetworkReplicatedEntity[presentation.Entities.Count];
        for (int i = 0; i < entities.Length; i++)
        {
            PresentationEntity entity = presentation.Entities[i];
            entities[i] = new NetworkReplicatedEntity(entity.EntityId, NetworkSnapshotProtocol.EncodePresentationEntity(entity));
        }
        NetworkReplicatedProjectile[] projectiles = new NetworkReplicatedProjectile[presentation.Projectiles.Count];
        for (int i = 0; i < projectiles.Length; i++)
        {
            PresentationProjectile projectile = presentation.Projectiles[i];
            projectiles[i] = new NetworkReplicatedProjectile(projectile.ProjectileId, projectile.Owner, projectile.Position);
        }
        OperationsCapacityState operations = world.GetOperationsCapacity(playerSlot);
        AlienChargeState charge = world.GetAlienCharge(playerSlot);
        NetworkOwnPlayerState own = new(world.GetProcessedResourceTotal(playerSlot, ResourceType.Ore),
            world.GetProcessedResourceTotal(playerSlot, ResourceType.Crystal), operations, charge);

        byte[] explored = new byte[KnowledgeBitsetBytes];
        byte[] visible = new byte[KnowledgeBitsetBytes];
        for (int y = 0; y < FogState.Height; y++)
        for (int x = 0; x < FogState.Width; x++)
        {
            int cell = y * FogState.Width + x;
            int mask = 1 << (cell & 7);
            if (world.Fog.IsExplored(playerSlot, x, y)) explored[cell >> 3] |= checked((byte)mask);
            if (world.Fog.IsVisible(playerSlot, x, y)) visible[cell >> 3] |= checked((byte)mask);
        }
        return new NetworkSnapshotState(world.Tick, playerSlot, own, entities, projectiles, explored, visible);
    }

    public VisibilityState GetKnowledge(int x, int y)
    {
        if ((uint)x >= FogState.Width || (uint)y >= FogState.Height || ExploredBits.Length != KnowledgeBitsetBytes || VisibleBits.Length != KnowledgeBitsetBytes)
            throw new ArgumentOutOfRangeException(nameof(x));
        int cell = y * FogState.Width + x;
        int mask = 1 << (cell & 7);
        if ((VisibleBits[cell >> 3] & mask) != 0) return VisibilityState.Visible;
        return (ExploredBits[cell >> 3] & mask) != 0 ? VisibilityState.Explored : VisibilityState.Unseen;
    }

    private void ValidateStableOrder()
    {
        uint previous = 0;
        for (int i = 0; i < Entities.Length; i++)
        {
            if (Entities[i].EntityId.Value <= previous) throw new InvalidDataException("Network entities are not in stable ID order.");
            previous = Entities[i].EntityId.Value;
        }
        previous = 0;
        for (int i = 0; i < Projectiles.Length; i++)
        {
            if (Projectiles[i].ProjectileId.Value <= previous) throw new InvalidDataException("Network projectiles are not in stable ID order.");
            previous = Projectiles[i].ProjectileId.Value;
        }
    }
}

public sealed class NetworkSnapshotFrame
{
    public uint Sequence { get; set; }
    public uint BaselineSequence { get; set; }
    public SimTick Tick { get; set; }
    public byte PlayerSlot { get; set; }
    public NetworkOwnPlayerState OwnPlayer { get; set; }
    public NetworkReplicatedEntity[] EntityUpserts { get; set; } = Array.Empty<NetworkReplicatedEntity>();
    public EntityId[] EntityRemovals { get; set; } = Array.Empty<EntityId>();
    public NetworkReplicatedProjectile[] ProjectileUpserts { get; set; } = Array.Empty<NetworkReplicatedProjectile>();
    public ProjectileId[] ProjectileRemovals { get; set; } = Array.Empty<ProjectileId>();
    public byte[]? ExploredBits { get; set; }
    public byte[]? VisibleBits { get; set; }
    public bool IsFull => BaselineSequence == 0;
}

public readonly struct NetworkSnapshotAcknowledgment
{
    public readonly ulong SessionToken;
    public readonly uint SnapshotSequence;
    public NetworkSnapshotAcknowledgment(ulong sessionToken, uint snapshotSequence)
    {
        SessionToken = sessionToken;
        SnapshotSequence = snapshotSequence;
    }
}

/// <summary>Project-owned T060 snapshot/delta packet format. ENet only carries these bytes.</summary>
public static class NetworkSnapshotProtocol
{
    public const ushort FormatVersion = 2;
    public const int MaximumPacketBytes = 64 * 1024;
    private const uint SnapshotMagic = 0x504E534C; // LSNP
    private const uint AcknowledgmentMagic = 0x414E534C; // LSNA
    private const int MaximumReplicatedRecords = 16_384;

    public static NetworkSnapshotFrame BuildFrame(uint sequence, NetworkSnapshotState current, uint baselineSequence = 0, NetworkSnapshotState? baseline = null)
    {
        if (sequence == 0) throw new ArgumentOutOfRangeException(nameof(sequence));
        if ((baselineSequence == 0) != (baseline is null)) throw new ArgumentException("A delta requires both a baseline sequence and baseline state.");
        if (baseline is not null && baseline.PlayerSlot != current.PlayerSlot) throw new ArgumentException("Snapshot baseline belongs to another player.");

        return new NetworkSnapshotFrame
        {
            Sequence = sequence,
            BaselineSequence = baselineSequence,
            Tick = current.Tick,
            PlayerSlot = current.PlayerSlot,
            OwnPlayer = current.OwnPlayer,
            EntityUpserts = baseline is null ? current.Entities : ChangedEntities(current.Entities, baseline.Entities),
            EntityRemovals = baseline is null ? Array.Empty<EntityId>() : RemovedEntities(current.Entities, baseline.Entities),
            ProjectileUpserts = baseline is null ? current.Projectiles : ChangedProjectiles(current.Projectiles, baseline.Projectiles),
            ProjectileRemovals = baseline is null ? Array.Empty<ProjectileId>() : RemovedProjectiles(current.Projectiles, baseline.Projectiles),
            ExploredBits = baseline is null || !BytesEqual(current.ExploredBits, baseline.ExploredBits) ? current.ExploredBits : null,
            VisibleBits = baseline is null || !BytesEqual(current.VisibleBits, baseline.VisibleBits) ? current.VisibleBits : null
        };
    }

    public static byte[] EncodeFrame(NetworkSnapshotFrame frame)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(SnapshotMagic);
        writer.Write(FormatVersion);
        writer.Write(SnapshotSerializer.SimulationProtocolVersion);
        writer.Write(frame.Sequence);
        writer.Write(frame.BaselineSequence);
        writer.Write(frame.Tick.Value);
        writer.Write(frame.PlayerSlot);
        WriteOwnPlayer(writer, frame.OwnPlayer);
        WriteEntities(writer, frame.EntityUpserts);
        writer.Write(frame.EntityRemovals.Length);
        for (int i = 0; i < frame.EntityRemovals.Length; i++) writer.Write(frame.EntityRemovals[i].Value);
        WriteProjectiles(writer, frame.ProjectileUpserts);
        writer.Write(frame.ProjectileRemovals.Length);
        for (int i = 0; i < frame.ProjectileRemovals.Length; i++) writer.Write(frame.ProjectileRemovals[i].Value);
        WriteOptionalBytes(writer, frame.ExploredBits);
        WriteOptionalBytes(writer, frame.VisibleBits);
        writer.Flush();
        if (stream.Length > MaximumPacketBytes) throw new InvalidOperationException("Encoded snapshot exceeded the network packet bound.");
        return stream.ToArray();
    }

    public static bool TryDecodeFrame(byte[] packet, out NetworkSnapshotFrame frame)
    {
        frame = new NetworkSnapshotFrame();
        if (packet == null || packet.Length > MaximumPacketBytes) return false;
        try
        {
            using MemoryStream stream = new(packet, false);
            using BinaryReader reader = new(stream);
            if (reader.ReadUInt32() != SnapshotMagic || reader.ReadUInt16() != FormatVersion ||
                reader.ReadUInt16() != SnapshotSerializer.SimulationProtocolVersion) return false;
            uint sequence = reader.ReadUInt32();
            uint baseline = reader.ReadUInt32();
            int tick = reader.ReadInt32();
            byte player = reader.ReadByte();
            if (sequence == 0 || tick < 0) return false;
            frame = new NetworkSnapshotFrame
            {
                Sequence = sequence,
                BaselineSequence = baseline,
                Tick = new SimTick(tick),
                PlayerSlot = player,
                OwnPlayer = ReadOwnPlayer(reader),
                EntityUpserts = ReadEntities(reader),
                EntityRemovals = ReadEntityIds(reader),
                ProjectileUpserts = ReadProjectiles(reader),
                ProjectileRemovals = ReadProjectileIds(reader),
                ExploredBits = ReadOptionalBytes(reader),
                VisibleBits = ReadOptionalBytes(reader)
            };
            return stream.Position == stream.Length;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentException) { return false; }
        catch (OverflowException) { return false; }
    }

    public static byte[] EncodeAcknowledgment(NetworkSnapshotAcknowledgment acknowledgment)
    {
        if (acknowledgment.SessionToken == 0 || acknowledgment.SnapshotSequence == 0) throw new ArgumentOutOfRangeException(nameof(acknowledgment));
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(AcknowledgmentMagic);
        writer.Write(FormatVersion);
        writer.Write(SnapshotSerializer.SimulationProtocolVersion);
        writer.Write(acknowledgment.SessionToken);
        writer.Write(acknowledgment.SnapshotSequence);
        writer.Flush();
        return stream.ToArray();
    }

    public static bool TryDecodeAcknowledgment(byte[] packet, out NetworkSnapshotAcknowledgment acknowledgment)
    {
        acknowledgment = default;
        if (packet == null) return false;
        try
        {
            using MemoryStream stream = new(packet, false);
            using BinaryReader reader = new(stream);
            if (reader.ReadUInt32() != AcknowledgmentMagic || reader.ReadUInt16() != FormatVersion ||
                reader.ReadUInt16() != SnapshotSerializer.SimulationProtocolVersion) return false;
            ulong token = reader.ReadUInt64();
            uint sequence = reader.ReadUInt32();
            if (token == 0 || sequence == 0 || stream.Position != stream.Length) return false;
            acknowledgment = new NetworkSnapshotAcknowledgment(token, sequence);
            return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
    }

    internal static byte[] EncodePresentationEntity(PresentationEntity entity)
    {
        using MemoryStream stream = new();
        using BinaryWriter w = new(stream);
        w.Write(entity.ContentType.Value); w.Write(entity.Owner); w.Write(entity.Position.X.Raw); w.Write(entity.Position.Y.Raw); w.Write(entity.Orientation.Raw);
        w.Write((byte)entity.Movement); w.Write((byte)entity.Visibility); w.Write((byte)entity.Footprint); w.Write((byte)entity.SelectableKind); w.Write((byte)entity.ResourceState);
        w.Write(entity.BuildingWidth); w.Write(entity.BuildingHeight); w.Write(entity.IsConstructionSite); w.Write(entity.ConstructionProgressBasisPoints);
        w.Write(entity.IsEnergyConsumer); w.Write(entity.IsPowered); w.Write((byte)entity.EnergyPriority);
        w.Write(entity.WeaponFireSequence); w.Write(entity.WeaponFireTarget.Value); w.Write((byte)entity.WeaponDelivery);
        w.Write(entity.HasHealth); w.Write(entity.CurrentHitPointsRaw); w.Write(entity.MaximumHitPointsRaw); w.Write(entity.ArmorRating); w.Write(entity.LastDamageTick);
        w.Write(entity.IsDestroyed); w.Write((byte)entity.DestructionKind); w.Write(entity.DestructionProgressBasisPoints); w.Write(entity.NonBlockingDebrisTicks); w.Write(entity.PersistentDebris);
        w.Write(entity.IsRepairing); w.Write(entity.RepairTarget.Value);
        w.Write(entity.IsTransport); w.Write(entity.TransportOccupiedPoints); w.Write(entity.TransportCapacityPoints); w.Write(entity.TransportPassengerCount);
        w.Write((byte)entity.TransportJobState); w.Write(entity.TransportUnloadBlocked);
        w.Write(entity.IsTransformable); w.Write(entity.TransformationState.Value); w.Write(entity.TransformationDestination.Value); w.Write((byte)entity.TransformationPhase);
        w.Write(entity.TransformationProgressBasisPoints); w.Write(entity.ReversalLockRemainingTicks); w.Write(entity.TransformationQueued); w.Write(entity.IsTrueAir); w.Write(entity.Snap);
        w.Flush();
        return stream.ToArray();
    }

    internal static PresentationEntity DecodePresentationEntity(EntityId entityId, byte[] state)
    {
        using MemoryStream stream = new(state, false);
        using BinaryReader r = new(stream);
        PresentationEntity entity = new(entityId, new ContentId(r.ReadUInt32()), r.ReadByte(),
            new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), new Angle16(r.ReadUInt16()),
            (MovementState)r.ReadByte(), (VisibilityState)r.ReadByte(), (FootprintClass)r.ReadByte(), (SelectableKind)r.ReadByte(),
            resourceState: (ResourceVisualState)r.ReadByte(), buildingWidth: r.ReadByte(), buildingHeight: r.ReadByte(),
            isConstructionSite: r.ReadBoolean(), constructionProgressBasisPoints: r.ReadUInt16(), isEnergyConsumer: r.ReadBoolean(),
            isPowered: r.ReadBoolean(), energyPriority: (EnergyPriority)r.ReadByte(), weaponFireSequence: r.ReadUInt32(),
            weaponFireTarget: new EntityId(r.ReadUInt32()), weaponDelivery: (WeaponDeliveryKind)r.ReadByte(), hasHealth: r.ReadBoolean(),
            currentHitPointsRaw: r.ReadInt32(), maximumHitPointsRaw: r.ReadInt32(), armorRating: r.ReadByte(), lastDamageTick: r.ReadInt32(),
            isDestroyed: r.ReadBoolean(), destructionKind: (DestructionKind)r.ReadByte(), destructionProgressBasisPoints: r.ReadUInt16(),
            nonBlockingDebrisTicks: r.ReadUInt16(), persistentDebris: r.ReadBoolean(), isRepairing: r.ReadBoolean(), repairTarget: new EntityId(r.ReadUInt32()),
            isTransport: r.ReadBoolean(), transportOccupiedPoints: r.ReadByte(), transportCapacityPoints: r.ReadByte(), transportPassengerCount: r.ReadByte(),
            transportJobState: (TransportJobState)r.ReadByte(), transportUnloadBlocked: r.ReadBoolean(), isTransformable: r.ReadBoolean(),
            transformationState: new ContentId(r.ReadUInt32()), transformationDestination: new ContentId(r.ReadUInt32()),
            transformationPhase: (TransformationPhase)r.ReadByte(), transformationProgressBasisPoints: r.ReadUInt16(), reversalLockRemainingTicks: r.ReadUInt16(),
            transformationQueued: r.ReadBoolean(), isTrueAir: r.ReadBoolean(), snap: r.ReadBoolean());
        if (stream.Position != stream.Length) throw new InvalidDataException("Presentation entity payload has trailing data.");
        return entity;
    }

    private static NetworkReplicatedEntity[] ChangedEntities(NetworkReplicatedEntity[] current, NetworkReplicatedEntity[] baseline)
    {
        List<NetworkReplicatedEntity> changed = new();
        int baselineIndex = 0;
        for (int i = 0; i < current.Length; i++)
        {
            while (baselineIndex < baseline.Length && baseline[baselineIndex].EntityId.Value < current[i].EntityId.Value) baselineIndex++;
            if (baselineIndex >= baseline.Length || baseline[baselineIndex].EntityId != current[i].EntityId || !BytesEqual(baseline[baselineIndex].State, current[i].State))
                changed.Add(current[i]);
        }
        return changed.ToArray();
    }

    private static EntityId[] RemovedEntities(NetworkReplicatedEntity[] current, NetworkReplicatedEntity[] baseline)
    {
        HashSet<uint> present = new();
        for (int i = 0; i < current.Length; i++) present.Add(current[i].EntityId.Value);
        List<EntityId> removed = new();
        for (int i = 0; i < baseline.Length; i++) if (!present.Contains(baseline[i].EntityId.Value)) removed.Add(baseline[i].EntityId);
        return removed.ToArray();
    }

    private static NetworkReplicatedProjectile[] ChangedProjectiles(NetworkReplicatedProjectile[] current, NetworkReplicatedProjectile[] baseline)
    {
        List<NetworkReplicatedProjectile> changed = new();
        int baselineIndex = 0;
        for (int i = 0; i < current.Length; i++)
        {
            while (baselineIndex < baseline.Length && baseline[baselineIndex].ProjectileId.Value < current[i].ProjectileId.Value) baselineIndex++;
            if (baselineIndex >= baseline.Length || !ProjectileEqual(baseline[baselineIndex], current[i])) changed.Add(current[i]);
        }
        return changed.ToArray();
    }

    private static ProjectileId[] RemovedProjectiles(NetworkReplicatedProjectile[] current, NetworkReplicatedProjectile[] baseline)
    {
        HashSet<uint> present = new();
        for (int i = 0; i < current.Length; i++) present.Add(current[i].ProjectileId.Value);
        List<ProjectileId> removed = new();
        for (int i = 0; i < baseline.Length; i++) if (!present.Contains(baseline[i].ProjectileId.Value)) removed.Add(baseline[i].ProjectileId);
        return removed.ToArray();
    }

    private static bool ProjectileEqual(NetworkReplicatedProjectile a, NetworkReplicatedProjectile b) =>
        a.ProjectileId == b.ProjectileId && a.Owner == b.Owner && a.Position.Equals(b.Position);

    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    private static void WriteOwnPlayer(BinaryWriter w, NetworkOwnPlayerState state)
    {
        w.Write(state.Ore); w.Write(state.Crystal); w.Write(state.OperationsActive); w.Write(state.OperationsReserved); w.Write(state.OperationsMaximum);
        w.Write(state.AlienChargeCurrent); w.Write(state.AlienChargeMaximum); w.Write(state.AlienChargeGenerationPerSecond); w.Write(state.ResonanceInitiationUnlocked);
    }

    private static NetworkOwnPlayerState ReadOwnPlayer(BinaryReader r)
    {
        int ore = r.ReadInt32(), crystal = r.ReadInt32();
        OperationsCapacityState operations = new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
        AlienChargeState charge = new() { CurrentMillicharge = r.ReadInt32(), MaximumMillicharge = r.ReadInt32(), GenerationMillichargePerSecond = r.ReadInt32(), ResonanceInitiationUnlocked = r.ReadBoolean() };
        return new NetworkOwnPlayerState(ore, crystal, operations, charge);
    }

    private static void WriteEntities(BinaryWriter w, NetworkReplicatedEntity[] values)
    {
        w.Write(values.Length);
        for (int i = 0; i < values.Length; i++) { w.Write(values[i].EntityId.Value); w.Write(values[i].State.Length); w.Write(values[i].State); }
    }

    private static NetworkReplicatedEntity[] ReadEntities(BinaryReader r)
    {
        int count = ReadCount(r);
        NetworkReplicatedEntity[] values = new NetworkReplicatedEntity[count];
        uint previous = 0;
        for (int i = 0; i < count; i++)
        {
            uint id = r.ReadUInt32(); int length = r.ReadInt32();
            if (id <= previous || length < 0 || length > 4096) throw new InvalidDataException("Invalid replicated entity payload.");
            byte[] state = r.ReadBytes(length); if (state.Length != length) throw new EndOfStreamException();
            values[i] = new NetworkReplicatedEntity(new EntityId(id), state); previous = id;
        }
        return values;
    }

    private static EntityId[] ReadEntityIds(BinaryReader r)
    {
        int count = ReadCount(r); EntityId[] ids = new EntityId[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = r.ReadUInt32(); if (id <= previous) throw new InvalidDataException("Invalid entity removal order."); ids[i] = new EntityId(id); previous = id; }
        return ids;
    }

    private static void WriteProjectiles(BinaryWriter w, NetworkReplicatedProjectile[] values)
    {
        w.Write(values.Length);
        for (int i = 0; i < values.Length; i++) { w.Write(values[i].ProjectileId.Value); w.Write(values[i].Owner); w.Write(values[i].Position.X.Raw); w.Write(values[i].Position.Y.Raw); }
    }

    private static NetworkReplicatedProjectile[] ReadProjectiles(BinaryReader r)
    {
        int count = ReadCount(r); NetworkReplicatedProjectile[] values = new NetworkReplicatedProjectile[count]; uint previous = 0;
        for (int i = 0; i < count; i++)
        {
            uint id = r.ReadUInt32(); if (id <= previous) throw new InvalidDataException("Invalid projectile order.");
            values[i] = new NetworkReplicatedProjectile(new ProjectileId(id), r.ReadByte(), new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32()))); previous = id;
        }
        return values;
    }

    private static ProjectileId[] ReadProjectileIds(BinaryReader r)
    {
        int count = ReadCount(r); ProjectileId[] ids = new ProjectileId[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = r.ReadUInt32(); if (id <= previous) throw new InvalidDataException("Invalid projectile removal order."); ids[i] = new ProjectileId(id); previous = id; }
        return ids;
    }

    private static int ReadCount(BinaryReader r)
    {
        int count = r.ReadInt32();
        if (count < 0 || count > MaximumReplicatedRecords) throw new InvalidDataException("Invalid replicated record count.");
        return count;
    }

    private static void WriteOptionalBytes(BinaryWriter w, byte[]? bytes)
    {
        w.Write(bytes is not null);
        if (bytes is null) return;
        byte[] compressed = Compress(bytes);
        bool useCompressed = compressed.Length < bytes.Length;
        w.Write(bytes.Length); w.Write(useCompressed); w.Write(useCompressed ? compressed.Length : bytes.Length);
        w.Write(useCompressed ? compressed : bytes);
    }

    private static byte[]? ReadOptionalBytes(BinaryReader r)
    {
        if (!r.ReadBoolean()) return null;
        int length = r.ReadInt32(); bool compressed = r.ReadBoolean(); int storedLength = r.ReadInt32();
        if (length < 0 || length > FogState.Width * FogState.Height || storedLength < 0 || storedLength > FogState.Width * FogState.Height)
            throw new InvalidDataException("Invalid knowledge bitset size.");
        byte[] stored = r.ReadBytes(storedLength); if (stored.Length != storedLength) throw new EndOfStreamException();
        if (!compressed)
        {
            if (storedLength != length) throw new InvalidDataException("Raw knowledge bitset length mismatch.");
            return stored;
        }
        using MemoryStream input = new(stored, false); using DeflateStream inflater = new(input, CompressionMode.Decompress);
        byte[] bytes = new byte[length]; int offset = 0;
        while (offset < length)
        {
            int read = inflater.Read(bytes, offset, length - offset); if (read == 0) throw new InvalidDataException("Compressed knowledge bitset ended early."); offset += read;
        }
        if (inflater.ReadByte() != -1) throw new InvalidDataException("Compressed knowledge bitset exceeded its declared length.");
        return bytes;
    }

    private static byte[] Compress(byte[] bytes)
    {
        using MemoryStream output = new();
        using (DeflateStream deflater = new(output, CompressionLevel.Fastest, true)) deflater.Write(bytes, 0, bytes.Length);
        return output.ToArray();
    }
}

public sealed class NetworkSnapshotClientBuffer
{
    private const int HistoryLimit = 32;
    private readonly SortedDictionary<uint, NetworkSnapshotState> _history = new();

    public uint LatestSequence { get; private set; }
    public NetworkSnapshotState? Latest { get; private set; }
    public PresentationEntity[] LastVisibilityLosses { get; private set; } = Array.Empty<PresentationEntity>();

    public bool TryApply(NetworkSnapshotFrame frame, out NetworkSnapshotState state)
    {
        state = null!;
        if (frame.Sequence == 0 || frame.Sequence <= LatestSequence) return false;
        NetworkSnapshotState? baseline = null;
        if (!frame.IsFull && !_history.TryGetValue(frame.BaselineSequence, out baseline)) return false;
        if (baseline is not null && baseline.PlayerSlot != frame.PlayerSlot) return false;

        LastVisibilityLosses = CaptureVisibilityLosses(baseline, frame);

        NetworkReplicatedEntity[] entities = ApplyEntities(baseline?.Entities, frame.EntityUpserts, frame.EntityRemovals);
        NetworkReplicatedProjectile[] projectiles = ApplyProjectiles(baseline?.Projectiles, frame.ProjectileUpserts, frame.ProjectileRemovals);
        byte[] explored = frame.ExploredBits ?? baseline?.ExploredBits ?? Array.Empty<byte>();
        byte[] visible = frame.VisibleBits ?? baseline?.VisibleBits ?? Array.Empty<byte>();
        state = new NetworkSnapshotState(frame.Tick, frame.PlayerSlot, frame.OwnPlayer, entities, projectiles, explored, visible);
        LatestSequence = frame.Sequence;
        Latest = state;
        _history.Add(frame.Sequence, state);
        while (_history.Count > HistoryLimit) _history.Remove(FirstKey(_history));
        return true;
    }

    private static PresentationEntity[] CaptureVisibilityLosses(NetworkSnapshotState? baseline, NetworkSnapshotFrame frame)
    {
        if (baseline is null || frame.EntityRemovals.Length == 0) return Array.Empty<PresentationEntity>();
        HashSet<uint> removals = new();
        for (int i = 0; i < frame.EntityRemovals.Length; i++) removals.Add(frame.EntityRemovals[i].Value);
        List<PresentationEntity> losses = new();
        for (int i = 0; i < baseline.Entities.Length; i++)
        {
            if (!removals.Contains(baseline.Entities[i].EntityId.Value)) continue;
            PresentationEntity previous = baseline.Entities[i].Decode();
            if (previous.Owner != frame.PlayerSlot) losses.Add(previous);
        }
        return losses.ToArray();
    }

    private static NetworkReplicatedEntity[] ApplyEntities(NetworkReplicatedEntity[]? baseline, NetworkReplicatedEntity[] upserts, EntityId[] removals)
    {
        SortedDictionary<uint, NetworkReplicatedEntity> values = new();
        if (baseline is not null) for (int i = 0; i < baseline.Length; i++) values.Add(baseline[i].EntityId.Value, baseline[i]);
        for (int i = 0; i < removals.Length; i++) values.Remove(removals[i].Value);
        for (int i = 0; i < upserts.Length; i++) values[upserts[i].EntityId.Value] = upserts[i];
        NetworkReplicatedEntity[] result = new NetworkReplicatedEntity[values.Count]; int index = 0;
        foreach (NetworkReplicatedEntity value in values.Values) result[index++] = value;
        return result;
    }

    private static NetworkReplicatedProjectile[] ApplyProjectiles(NetworkReplicatedProjectile[]? baseline, NetworkReplicatedProjectile[] upserts, ProjectileId[] removals)
    {
        SortedDictionary<uint, NetworkReplicatedProjectile> values = new();
        if (baseline is not null) for (int i = 0; i < baseline.Length; i++) values.Add(baseline[i].ProjectileId.Value, baseline[i]);
        for (int i = 0; i < removals.Length; i++) values.Remove(removals[i].Value);
        for (int i = 0; i < upserts.Length; i++) values[upserts[i].ProjectileId.Value] = upserts[i];
        NetworkReplicatedProjectile[] result = new NetworkReplicatedProjectile[values.Count]; int index = 0;
        foreach (NetworkReplicatedProjectile value in values.Values) result[index++] = value;
        return result;
    }

    private static uint FirstKey(SortedDictionary<uint, NetworkSnapshotState> values)
    {
        foreach (uint key in values.Keys) return key;
        throw new InvalidOperationException("Snapshot history is empty.");
    }
}

public sealed class ServerSnapshotSession
{
    private const int HistoryLimit = 32;
    private readonly SortedDictionary<uint, NetworkSnapshotState> _history = new();
    private uint _nextSequence = 1;

    public uint LastAcknowledgedSequence { get; private set; }

    public byte[] CreatePacket(SimulationWorld world, byte playerSlot)
    {
        NetworkSnapshotState current = NetworkSnapshotState.Capture(world, playerSlot);
        uint sequence = _nextSequence++;
        NetworkSnapshotState? baseline = null;
        if (LastAcknowledgedSequence != 0) _history.TryGetValue(LastAcknowledgedSequence, out baseline);
        NetworkSnapshotFrame frame = NetworkSnapshotProtocol.BuildFrame(sequence, current, baseline is null ? 0 : LastAcknowledgedSequence, baseline);
        _history.Add(sequence, current);
        while (_history.Count > HistoryLimit) _history.Remove(FirstKey());
        return NetworkSnapshotProtocol.EncodeFrame(frame);
    }

    public bool TryAcknowledge(uint sequence)
    {
        if (sequence <= LastAcknowledgedSequence || !_history.ContainsKey(sequence)) return false;
        LastAcknowledgedSequence = sequence;
        return true;
    }

    private uint FirstKey()
    {
        foreach (uint key in _history.Keys) return key;
        throw new InvalidOperationException("Snapshot history is empty.");
    }
}

public static class NetworkSnapshotInterpolation
{
    public static FixVec2 Position(PresentationEntity older, PresentationEntity newer, Fix32 alpha)
    {
        alpha = Fix32.Clamp(alpha, Fix32.Zero, Fix32.One);
        return older.Position + (newer.Position - older.Position) * alpha;
    }

    public static Angle16 Orientation(PresentationEntity older, PresentationEntity newer, Fix32 alpha)
    {
        alpha = Fix32.Clamp(alpha, Fix32.Zero, Fix32.One);
        int delta = Angle16.ShortestDelta(older.Orientation, newer.Orientation);
        int step = (int)((long)delta * alpha.Raw / Fix32.OneRaw);
        return new Angle16(unchecked((ushort)(older.Orientation.Raw + step)));
    }
}
}
