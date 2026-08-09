using System;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct PrototypeMovementProfile
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly Fix32 MaxSpeed;
    public readonly Fix32 Acceleration;
    public readonly Fix32 Deceleration;
    public readonly ushort TurnRatePerTick;
    public readonly ReversePolicy ReversePolicy;
    public readonly MovementLayer Layer;
    public PrototypeMovementProfile(string stableKey, Fix32 maxSpeed, Fix32 acceleration, Fix32 deceleration, ushort turnRatePerTick, ReversePolicy reversePolicy, MovementLayer layer)
    {
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey); MaxSpeed = maxSpeed; Acceleration=acceleration; Deceleration=deceleration;
        TurnRatePerTick=turnRatePerTick; ReversePolicy=reversePolicy; Layer = layer;
    }
    public PrototypeMovementProfile(string stableKey, Fix32 maxSpeed, MovementLayer layer)
        : this(stableKey,maxSpeed,Fix32.FromInt(2),Fix32.FromRatio(5,2),865,ReversePolicy.Reduced,layer) { }
}

public readonly struct PrototypeEntityDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly string FactionKey;
    public readonly string SourceClassification;
    public readonly string MovementProfileKey;
    public readonly FootprintClass Footprint;
    public readonly SelectableKind SelectableKind;
    public readonly byte VisionRadius;
    public readonly string ViewProfileKey;
    public readonly ushort OreTicksPerUnit;
    public readonly byte OreCarryCapacity;

    public PrototypeEntityDefinition(string stableKey, string factionKey, string sourceClassification, string movementProfileKey,
        FootprintClass footprint, SelectableKind selectableKind, byte visionRadius, string viewProfileKey, ushort oreTicksPerUnit = 0, byte oreCarryCapacity = 0)
    {
        if ((oreTicksPerUnit == 0) != (oreCarryCapacity == 0)) throw new ArgumentException("Worker extraction cadence and carry capacity must both be present or absent.");
        if (oreCarryCapacity > 0 && selectableKind != SelectableKind.Worker) throw new ArgumentException("Only Worker definitions may carry worker harvesting metadata.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey)); Id = StableId.FromKey(stableKey);
        FactionKey = factionKey ?? throw new ArgumentNullException(nameof(factionKey));
        SourceClassification = sourceClassification ?? throw new ArgumentNullException(nameof(sourceClassification));
        MovementProfileKey = movementProfileKey ?? throw new ArgumentNullException(nameof(movementProfileKey));
        Footprint = footprint; SelectableKind = selectableKind; VisionRadius = visionRadius;
        ViewProfileKey = viewProfileKey ?? throw new ArgumentNullException(nameof(viewProfileKey));
        OreTicksPerUnit = oreTicksPerUnit; OreCarryCapacity = oreCarryCapacity;
    }
}

public readonly struct ResourceNodeDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly ResourceType Type;
    public readonly ResourceDepositSize DepositSize;
    public readonly int Capacity;
    public readonly HarvestInteraction HarvestInteraction;
    public readonly ResourceDepletionProfile DepletionProfile;
    public readonly ushort ReducedThresholdBasisPoints;
    public readonly ushort LowThresholdBasisPoints;
    public readonly ushort CriticalThresholdBasisPoints;
    public readonly string ViewProfileKey;

    public ResourceNodeDefinition(string stableKey, ResourceType type, ResourceDepositSize depositSize, int capacity,
        HarvestInteraction harvestInteraction, ResourceDepletionProfile depletionProfile,
        ushort reducedThresholdBasisPoints, ushort lowThresholdBasisPoints, ushort criticalThresholdBasisPoints,
        string viewProfileKey)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (reducedThresholdBasisPoints > 10_000 || lowThresholdBasisPoints > reducedThresholdBasisPoints || criticalThresholdBasisPoints > lowThresholdBasisPoints)
            throw new ArgumentOutOfRangeException(nameof(reducedThresholdBasisPoints), "Resource model-state thresholds must descend within 0..10000 basis points.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey);
        Type = type;
        DepositSize = depositSize;
        Capacity = capacity;
        HarvestInteraction = harvestInteraction;
        DepletionProfile = depletionProfile;
        ReducedThresholdBasisPoints = reducedThresholdBasisPoints;
        LowThresholdBasisPoints = lowThresholdBasisPoints;
        CriticalThresholdBasisPoints = criticalThresholdBasisPoints;
        ViewProfileKey = viewProfileKey ?? throw new ArgumentNullException(nameof(viewProfileKey));
    }
}

/// <summary>Immutable prototype gameplay metadata compiled from Content/PrototypeEntities.json.</summary>
public sealed class PrototypeContentCatalog
{
    public PrototypeMovementProfile[] MovementProfiles { get; }
    public PrototypeEntityDefinition[] Entities { get; }
    public ResourceNodeDefinition[] ResourceNodes { get; }
    public ulong ContentHash { get; internal set; }
    public PrototypeContentCatalog(PrototypeMovementProfile[] movementProfiles, PrototypeEntityDefinition[] entities, ResourceNodeDefinition[]? resourceNodes = null)
    {
        MovementProfiles = movementProfiles ?? Array.Empty<PrototypeMovementProfile>();
        Entities = entities ?? Array.Empty<PrototypeEntityDefinition>();
        ResourceNodes = resourceNodes ?? Array.Empty<ResourceNodeDefinition>();
    }

    public bool ContainsEntityKey(string stableKey) => TryGetEntity(stableKey, out _);
    public bool TryGetEntity(string stableKey, out PrototypeEntityDefinition definition)
    {
        for (int i = 0; i < Entities.Length; i++) if (string.Equals(Entities[i].StableKey, stableKey, StringComparison.Ordinal)) { definition = Entities[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetMovement(string stableKey, out PrototypeMovementProfile profile)
    {
        for (int i = 0; i < MovementProfiles.Length; i++) if (string.Equals(MovementProfiles[i].StableKey, stableKey, StringComparison.Ordinal)) { profile = MovementProfiles[i]; return true; }
        profile = default; return false;
    }
    public bool ContainsResourceNodeKey(string stableKey) => TryGetResourceNode(stableKey, out _);
    public bool TryGetResourceNode(string stableKey, out ResourceNodeDefinition definition)
    {
        for (int i = 0; i < ResourceNodes.Length; i++) if (string.Equals(ResourceNodes[i].StableKey, stableKey, StringComparison.Ordinal)) { definition = ResourceNodes[i]; return true; }
        definition = default; return false;
    }
}

public static class PrototypeContentFactory
{
    /// <summary>Built-in mirror of the checked-in prototype source data; used by headless/debug startup before generated files are present.</summary>
    public static PrototypeContentCatalog CreateM2Catalog()
    {
        PrototypeMovementProfile[] profiles =
        {
            new PrototypeMovementProfile("movement.prototype.chrome_crusher", Fix32.FromRatio(92,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.crew", Fix32.FromRatio(135,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.hover_scout", Fix32.FromRatio(225,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.prototype.loader_dozer", Fix32.FromRatio(130,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.nav_huge", Fix32.FromRatio(9,10), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 410, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.static", Fix32.Zero, Fix32.Zero, Fix32.Zero, 0, ReversePolicy.None, MovementLayer.Ground)
        };
        PrototypeEntityDefinition[] entities =
        {
            new PrototypeEntityDefinition("building.rock_raiders.hq", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.hq"),
            new PrototypeEntityDefinition("prototype.nav.huge", "Technical", "ENGINEERING_ONLY", "movement.prototype.nav_huge", FootprintClass.Huge, SelectableKind.CombatSupport, 8, "view.placeholder.navigation.huge"),
            new PrototypeEntityDefinition("unit.rock_raiders.chrome_crusher", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.chrome_crusher", FootprintClass.Large, SelectableKind.CombatSupport, 8, "view.placeholder.rock_raiders.chrome_crusher"),
            new PrototypeEntityDefinition("unit.rock_raiders.crew", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.crew", FootprintClass.Tiny, SelectableKind.Worker, 7, "view.placeholder.rock_raiders.crew", 30, 8),
            new PrototypeEntityDefinition("unit.rock_raiders.hover_scout", "RockRaiders", "OFFICIAL_DIRECT", "movement.prototype.hover_scout", FootprintClass.Small, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.hover_scout"),
            new PrototypeEntityDefinition("unit.rock_raiders.loader_dozer", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.loader_dozer", FootprintClass.Medium, SelectableKind.CombatSupport, 7, "view.placeholder.rock_raiders.loader_dozer")
        };
        ResourceNodeDefinition[] resources =
        {
            new ResourceNodeDefinition("resource.ore.deep_contested_seam", ResourceType.Ore, ResourceDepositSize.DeepContestedSeam, 2400, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.deep_contested_seam"),
            new ResourceNodeDefinition("resource.ore.rich", ResourceType.Ore, ResourceDepositSize.Rich, 1350, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.rich"),
            new ResourceNodeDefinition("resource.ore.small", ResourceType.Ore, ResourceDepositSize.Small, 600, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.small"),
            new ResourceNodeDefinition("resource.ore.standard", ResourceType.Ore, ResourceDepositSize.Standard, 900, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.standard")
        };
        PrototypeContentCatalog catalog = new PrototypeContentCatalog(profiles, entities, resources);
        PrototypeContentCodec.Write(catalog);
        return catalog;
    }
}


public static class PrototypeContentCodec
{
    private const int Magic = 0x4350534C; // LSPC little-endian bytes.
    public const int FormatVersion = 4;

    public static byte[] Write(PrototypeContentCatalog catalog)
    {
        using MemoryStream stream = new MemoryStream(4096);
        using BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(Magic); writer.Write(FormatVersion);
        writer.Write(catalog.MovementProfiles.Length);
        for (int i = 0; i < catalog.MovementProfiles.Length; i++)
        {
            PrototypeMovementProfile p = catalog.MovementProfiles[i];
            writer.Write(p.StableKey); writer.Write(p.Id.Value); writer.Write(p.MaxSpeed.Raw); writer.Write(p.Acceleration.Raw); writer.Write(p.Deceleration.Raw);
            writer.Write(p.TurnRatePerTick); writer.Write((byte)p.ReversePolicy); writer.Write((byte)p.Layer);
        }
        writer.Write(catalog.Entities.Length);
        for (int i = 0; i < catalog.Entities.Length; i++)
        {
            PrototypeEntityDefinition e = catalog.Entities[i];
            writer.Write(e.StableKey); writer.Write(e.Id.Value); writer.Write(e.FactionKey); writer.Write(e.SourceClassification);
            writer.Write(e.MovementProfileKey); writer.Write((byte)e.Footprint); writer.Write((byte)e.SelectableKind); writer.Write(e.VisionRadius); writer.Write(e.ViewProfileKey);
            writer.Write(e.OreTicksPerUnit); writer.Write(e.OreCarryCapacity);
        }
        writer.Write(catalog.ResourceNodes.Length);
        for (int i = 0; i < catalog.ResourceNodes.Length; i++)
        {
            ResourceNodeDefinition n = catalog.ResourceNodes[i];
            writer.Write(n.StableKey); writer.Write(n.Id.Value); writer.Write((byte)n.Type); writer.Write((byte)n.DepositSize); writer.Write(n.Capacity);
            writer.Write((byte)n.HarvestInteraction); writer.Write((byte)n.DepletionProfile);
            writer.Write(n.ReducedThresholdBasisPoints); writer.Write(n.LowThresholdBasisPoints); writer.Write(n.CriticalThresholdBasisPoints); writer.Write(n.ViewProfileKey);
        }
        writer.Flush();
        byte[] bytes = stream.ToArray(); catalog.ContentHash = DeterministicHash.Fnv1A64(bytes); return bytes;
    }

    public static PrototypeContentCatalog Read(byte[] bytes)
    {
        using MemoryStream stream = new MemoryStream(bytes, false);
        using BinaryReader reader = new BinaryReader(stream);
        if (reader.ReadInt32() != Magic) throw new InvalidDataException("Prototype content magic mismatch.");
        int formatVersion = reader.ReadInt32();
        if (formatVersion != 2 && formatVersion != 3 && formatVersion != FormatVersion) throw new InvalidDataException("Prototype content format version mismatch.");
        int profileCount = reader.ReadInt32(); if (profileCount < 0 || profileCount > 1024) throw new InvalidDataException("Invalid movement profile count.");
        PrototypeMovementProfile[] profiles = new PrototypeMovementProfile[profileCount];
        for (int i = 0; i < profileCount; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); Fix32 speed = Fix32.FromRaw(reader.ReadInt32());
            Fix32 acceleration=Fix32.FromRaw(reader.ReadInt32()); Fix32 deceleration=Fix32.FromRaw(reader.ReadInt32()); ushort turn=reader.ReadUInt16();
            ReversePolicy reverse=(ReversePolicy)reader.ReadByte(); MovementLayer layer = (MovementLayer)reader.ReadByte();
            profiles[i] = new PrototypeMovementProfile(key, speed, acceleration, deceleration, turn, reverse, layer); if (profiles[i].Id.Value != id) throw new InvalidDataException("Stable movement ID mismatch.");
        }
        int entityCount = reader.ReadInt32(); if (entityCount < 0 || entityCount > 4096) throw new InvalidDataException("Invalid prototype entity count.");
        PrototypeEntityDefinition[] entities = new PrototypeEntityDefinition[entityCount];
        for (int i = 0; i < entityCount; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); string faction = reader.ReadString(); string source = reader.ReadString(); string movement = reader.ReadString();
            FootprintClass fp = (FootprintClass)reader.ReadByte(); SelectableKind kind = (SelectableKind)reader.ReadByte(); byte vision = reader.ReadByte(); string view = reader.ReadString();
            ushort oreTicks = formatVersion >= 4 ? reader.ReadUInt16() : kind == SelectableKind.Worker ? (ushort)30 : (ushort)0;
            byte oreCapacity = formatVersion >= 4 ? reader.ReadByte() : kind == SelectableKind.Worker ? (byte)8 : (byte)0;
            entities[i] = new PrototypeEntityDefinition(key, faction, source, movement, fp, kind, vision, view, oreTicks, oreCapacity); if (entities[i].Id.Value != id) throw new InvalidDataException("Stable entity ID mismatch.");
        }
        ResourceNodeDefinition[] resourceNodes = Array.Empty<ResourceNodeDefinition>();
        if (formatVersion >= 3)
        {
            int resourceCount = reader.ReadInt32(); if (resourceCount < 0 || resourceCount > 1024) throw new InvalidDataException("Invalid resource node definition count.");
            resourceNodes = new ResourceNodeDefinition[resourceCount];
            for (int i = 0; i < resourceCount; i++)
            {
                string key = reader.ReadString(); uint id = reader.ReadUInt32(); ResourceType type = (ResourceType)reader.ReadByte(); ResourceDepositSize size = (ResourceDepositSize)reader.ReadByte(); int capacity = reader.ReadInt32();
                HarvestInteraction interaction = (HarvestInteraction)reader.ReadByte(); ResourceDepletionProfile depletion = (ResourceDepletionProfile)reader.ReadByte();
                ushort reduced = reader.ReadUInt16(); ushort low = reader.ReadUInt16(); ushort critical = reader.ReadUInt16(); string view = reader.ReadString();
                resourceNodes[i] = new ResourceNodeDefinition(key, type, size, capacity, interaction, depletion, reduced, low, critical, view);
                if (resourceNodes[i].Id.Value != id) throw new InvalidDataException("Stable resource node ID mismatch.");
            }
        }
        if (stream.Position != stream.Length) throw new InvalidDataException("Trailing prototype content bytes.");
        PrototypeContentCatalog result = new PrototypeContentCatalog(profiles, entities, resourceNodes) { ContentHash = DeterministicHash.Fnv1A64(bytes) };
        return result;
    }
}
}
