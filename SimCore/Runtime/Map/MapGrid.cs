using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
[Flags]
public enum MapCellFlags : ushort
{
    None = 0,
    Ground = 1 << 0,
    Rough = 1 << 1,
    Impassable = 1 << 2,
    Buildable = 1 << 3,
    GroundOccluder = 1 << 4,
    AirBlocked = 1 << 5,
    Excavatable = 1 << 6
}

public enum ExcavatableTerrainClass : byte
{
    LooseRubbleBlockage = 0,
    FracturedRockWall = 1,
    MineralizedRidge = 2,
    SealedTunnelMouth = 3,
    ReinforcedBedrockBarrier = 4
}

public enum ExcavatableFeatureState : byte
{
    Blocked = 0,
    ActiveExcavation = 1,
    Open = 2
}

public readonly struct NavCell : IComparable<NavCell>, IEquatable<NavCell>
{
    public readonly short X; public readonly short Y;
    public NavCell(short x, short y) { X=x; Y=y; }
    public int CompareTo(NavCell other) { int c=Y.CompareTo(other.Y); return c!=0?c:X.CompareTo(other.X); }
    public bool Equals(NavCell other) => X==other.X && Y==other.Y;
    public override bool Equals(object? obj) => obj is NavCell other && Equals(other);
    public override int GetHashCode() => (X*397)^Y;
    public static bool operator ==(NavCell a,NavCell b)=>a.Equals(b);
    public static bool operator !=(NavCell a,NavCell b)=>!a.Equals(b);
}

public readonly struct IntRect
{
    public readonly short X; public readonly short Y; public readonly short Width; public readonly short Height;
    public IntRect(short x,short y,short width,short height){X=x;Y=y;Width=width;Height=height;}
    public bool Contains(int x,int y)=>x>=X&&y>=Y&&x<X+Width&&y<Y+Height;
}

public sealed class ExcavatableFeature
{
    public string StableKey { get; }
    public ContentId StableId { get; }
    public ushort FeatureId { get; }
    public IntRect NavRect { get; }
    public ExcavatableTerrainClass TerrainClass { get; }
    public ushort RequiredEnergy { get; }
    public ContentId VisualProfile { get; }
    public bool OpenBuildable { get; }
    public ExcavatableFeatureState State { get; internal set; }
    public bool Open => State == ExcavatableFeatureState.Open;

    public ExcavatableFeature(ushort featureId, IntRect navRect, bool open = false)
        : this($"map.feature.{featureId}", featureId, navRect, ExcavatableTerrainClass.FracturedRockWall,
            requiredEnergy: 25, LegoSpaceRTS.SimCore.StableId.FromKey("view.placeholder.excavatable.fractured_rock_wall"),
            openBuildable: true, open ? ExcavatableFeatureState.Open : ExcavatableFeatureState.Blocked)
    {
    }

    public ExcavatableFeature(string stableKey, ushort featureId, IntRect navRect,
        ExcavatableTerrainClass terrainClass, ushort requiredEnergy, ContentId visualProfile,
        bool openBuildable, ExcavatableFeatureState state = ExcavatableFeatureState.Blocked)
    {
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        StableId = LegoSpaceRTS.SimCore.StableId.FromKey(stableKey);
        FeatureId = featureId;
        NavRect = navRect;
        TerrainClass = terrainClass;
        RequiredEnergy = requiredEnergy;
        VisualProfile = visualProfile;
        OpenBuildable = openBuildable;
        State = state;
    }
}

public sealed class MapGrid
{
    public const int BuildWidth = 160;
    public const int BuildHeight = 160;
    public const int NavPerBuild = 2;
    public const int NavWidth = BuildWidth * NavPerBuild;
    public const int NavHeight = BuildHeight * NavPerBuild;

    private readonly MapCellFlags[] _flags = new MapCellFlags[NavWidth * NavHeight];
    private readonly sbyte[] _elevation = new sbyte[NavWidth * NavHeight];
    private readonly ushort[] _feature = new ushort[NavWidth * NavHeight];
    private readonly List<ExcavatableFeature> _features = new();

    public MapId Id { get; }
    public string StableKey { get; }
    public int TopologyVersion { get; private set; }
    public IReadOnlyList<ExcavatableFeature> Features => _features;

    public MapGrid(string stableKey)
    {
        StableKey = stableKey;
        Id = new MapId(StableId.FromKey(stableKey).Value);
        for (int i = 0; i < _flags.Length; i++) _flags[i] = MapCellFlags.Ground | MapCellFlags.Buildable;
    }

    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < NavWidth && y < NavHeight;
    public int Index(int x, int y) => y * NavWidth + x;
    public MapCellFlags GetFlags(int x, int y) => _flags[Index(x, y)];
    public sbyte GetElevation(int x, int y) => _elevation[Index(x, y)];
    public ushort GetFeatureId(int x, int y) => _feature[Index(x, y)];

    public void SetFlagsRect(IntRect rect, MapCellFlags add, MapCellFlags remove = MapCellFlags.None)
    {
        for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            for (int x = rect.X; x < rect.X + rect.Width; x++)
                if (InBounds(x, y)) _flags[Index(x, y)] = (_flags[Index(x, y)] | add) & ~remove;
    }

    public void SetElevationRect(IntRect rect, sbyte elevation)
    {
        for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            for (int x = rect.X; x < rect.X + rect.Width; x++)
                if (InBounds(x, y)) _elevation[Index(x, y)] = elevation;
    }

    public void AddExcavatable(ExcavatableFeature feature)
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));
        if (feature.FeatureId == 0) throw new ArgumentOutOfRangeException(nameof(feature), "Excavatable Feature ID 0 is reserved.");
        if (feature.StableId.Value == 0 || feature.VisualProfile.Value == 0 || feature.TerrainClass < ExcavatableTerrainClass.LooseRubbleBlockage ||
            feature.TerrainClass > ExcavatableTerrainClass.ReinforcedBedrockBarrier || feature.State < ExcavatableFeatureState.Blocked ||
            feature.State > ExcavatableFeatureState.Open)
            throw new ArgumentOutOfRangeException(nameof(feature), "Excavatable Feature metadata is invalid.");
        IntRect rect = feature.NavRect;
        if (rect.Width <= 0 || rect.Height <= 0 || rect.X < 0 || rect.Y < 0 || rect.X + rect.Width > NavWidth || rect.Y + rect.Height > NavHeight)
            throw new ArgumentOutOfRangeException(nameof(feature), "Excavatable Feature bounds must be positive and inside the navigation grid.");
        for (int i = 0; i < _features.Count; i++)
        {
            ExcavatableFeature existing = _features[i];
            if (existing.FeatureId == feature.FeatureId) throw new InvalidOperationException($"Duplicate Excavatable Feature ID {feature.FeatureId}.");
            if (existing.StableId == feature.StableId) throw new InvalidOperationException($"Duplicate Excavatable Feature stable ID {feature.StableKey}.");
        }
        for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            for (int x = rect.X; x < rect.X + rect.Width; x++)
                if (_feature[Index(x, y)] != 0) throw new InvalidOperationException($"Excavatable Feature {feature.FeatureId} overlaps another authored feature.");

        _features.Add(feature);
        for (int y = feature.NavRect.Y; y < feature.NavRect.Y + feature.NavRect.Height; y++)
            for (int x = feature.NavRect.X; x < feature.NavRect.X + feature.NavRect.Width; x++)
            {
                int i = Index(x, y);
                _feature[i] = feature.FeatureId;
                if (feature.Open)
                {
                    _flags[i] = (_flags[i] | MapCellFlags.Ground) & ~(MapCellFlags.Excavatable | MapCellFlags.Impassable | MapCellFlags.GroundOccluder);
                    if (feature.OpenBuildable) _flags[i] |= MapCellFlags.Buildable;
                    else _flags[i] &= ~MapCellFlags.Buildable;
                }
                else
                {
                    _flags[i] |= MapCellFlags.Excavatable | MapCellFlags.Impassable | MapCellFlags.GroundOccluder;
                    if (!feature.OpenBuildable) _flags[i] &= ~MapCellFlags.Buildable;
                }
            }
    }

    public IntRect OpenFeature(ushort featureId)
    {
        if (TryOpenFeature(featureId, out IntRect affected)) return affected;
        if (TryGetFeature(featureId, out ExcavatableFeature feature)) return feature.NavRect;
        throw new InvalidOperationException($"Unknown Excavatable Feature {featureId}.");
    }

    public bool TryOpenFeature(ushort featureId, out IntRect affected)
    {
        for (int i = 0; i < _features.Count; i++)
        {
            ExcavatableFeature feature = _features[i];
            if (feature.FeatureId != featureId) continue;
            affected = feature.NavRect;
            if (feature.Open) return false;
            feature.State = ExcavatableFeatureState.Open;
            SetFlagsRect(feature.NavRect, MapCellFlags.Ground,
                MapCellFlags.Excavatable | MapCellFlags.Impassable | MapCellFlags.GroundOccluder);
            if (feature.OpenBuildable) SetFlagsRect(feature.NavRect, MapCellFlags.Buildable);
            else SetFlagsRect(feature.NavRect, MapCellFlags.None, MapCellFlags.Buildable);
            TopologyVersion++;
            return true;
        }
        affected = default;
        return false;
    }

    internal bool SetFeatureState(ushort featureId, ExcavatableFeatureState state)
    {
        for (int i = 0; i < _features.Count; i++)
        {
            if (_features[i].FeatureId != featureId) continue;
            if (_features[i].Open || state == ExcavatableFeatureState.Open) return false;
            _features[i].State = state;
            return true;
        }
        return false;
    }

    public bool TryGetFeature(ushort featureId, out ExcavatableFeature feature)
    {
        for (int i = 0; i < _features.Count; i++)
            if (_features[i].FeatureId == featureId) { feature = _features[i]; return true; }
        feature = null!;
        return false;
    }

    public IntRect SetConstructionOccupied(short anchorX, short anchorY, BuildingDefinition definition, byte orientation, bool occupied)
    {
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        if (anchorX < 0 || anchorY < 0 || anchorX + width > BuildWidth || anchorY + height > BuildHeight)
            throw new ArgumentOutOfRangeException(nameof(anchorX), "Construction footprint is outside the build grid.");
        for (byte y = 0; y < height; y++)
        for (byte x = 0; x < width; x++)
        {
            if (!definition.Occupies(x, y, orientation)) continue;
            IntRect navCell = new((short)((anchorX + x) * NavPerBuild), (short)((anchorY + y) * NavPerBuild), NavPerBuild, NavPerBuild);
            if (occupied) SetFlagsRect(navCell, MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
            else SetFlagsRect(navCell, MapCellFlags.Ground | MapCellFlags.Buildable, MapCellFlags.Impassable | MapCellFlags.GroundOccluder);
        }
        TopologyVersion++;
        return new IntRect((short)(anchorX * NavPerBuild), (short)(anchorY * NavPerBuild), (short)(width * NavPerBuild), (short)(height * NavPerBuild));
    }

    public static FixVec2 NavCellCenterToBuild(NavCell cell) => new(
        Fix32.FromRatio(cell.X * 2 + 1, 4),
        Fix32.FromRatio(cell.Y * 2 + 1, 4));

    public static NavCell BuildToNav(FixVec2 position)
    {
        int x = (position.X.Raw * NavPerBuild) >> Fix32.FractionalBits;
        int y = (position.Y.Raw * NavPerBuild) >> Fix32.FractionalBits;
        if (x < 0) x = 0; else if (x >= NavWidth) x = NavWidth - 1;
        if (y < 0) y = 0; else if (y >= NavHeight) y = NavHeight - 1;
        return new NavCell((short)x, (short)y);
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(StableKey);
        writer.Write(TopologyVersion);
        writer.Write(_flags.Length);
        for (int i = 0; i < _flags.Length; i++) writer.Write((ushort)_flags[i]);
        writer.Write(_elevation.Length);
        for (int i = 0; i < _elevation.Length; i++) writer.Write(_elevation[i]);
        writer.Write(_feature.Length);
        for (int i = 0; i < _feature.Length; i++) writer.Write(_feature[i]);
        writer.Write(_features.Count);
        for (int i = 0; i < _features.Count; i++)
        {
            ExcavatableFeature f = _features[i];
            writer.Write(f.FeatureId);
            writer.Write(f.NavRect.X); writer.Write(f.NavRect.Y); writer.Write(f.NavRect.Width); writer.Write(f.NavRect.Height);
            writer.Write((byte)f.State);
            writer.Write(f.StableKey); writer.Write((byte)f.TerrainClass); writer.Write(f.RequiredEnergy);
            writer.Write(f.VisualProfile.Value); writer.Write(f.OpenBuildable);
        }
    }

    public static MapGrid Deserialize(BinaryReader reader, bool includeExcavatableMetadata = false)
    {
        string key = reader.ReadString();
        int topology = reader.ReadInt32();
        MapGrid map = new(key);
        int count = reader.ReadInt32();
        if (count != map._flags.Length) throw new InvalidDataException("Map flag size mismatch.");
        for (int i = 0; i < count; i++) map._flags[i] = (MapCellFlags)reader.ReadUInt16();
        count = reader.ReadInt32();
        if (count != map._elevation.Length) throw new InvalidDataException("Map elevation size mismatch.");
        for (int i = 0; i < count; i++) map._elevation[i] = reader.ReadSByte();
        count = reader.ReadInt32();
        if (count != map._feature.Length) throw new InvalidDataException("Map feature size mismatch.");
        for (int i = 0; i < count; i++) map._feature[i] = reader.ReadUInt16();
        int featureCount = reader.ReadInt32();
        for (int i = 0; i < featureCount; i++)
        {
            ushort id = reader.ReadUInt16();
            IntRect rect = new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            ExcavatableFeatureState state = includeExcavatableMetadata
                ? (ExcavatableFeatureState)reader.ReadByte()
                : (reader.ReadBoolean() ? ExcavatableFeatureState.Open : ExcavatableFeatureState.Blocked);
            if (state < ExcavatableFeatureState.Blocked || state > ExcavatableFeatureState.Open) throw new InvalidDataException("Invalid Excavatable Feature state.");
            string stableKey = includeExcavatableMetadata ? reader.ReadString() : $"{key}.excavatable.{id}";
            ExcavatableTerrainClass terrainClass = includeExcavatableMetadata ? (ExcavatableTerrainClass)reader.ReadByte() : ExcavatableTerrainClass.FracturedRockWall;
            ushort requiredEnergy = includeExcavatableMetadata ? reader.ReadUInt16() : (ushort)25;
            ContentId visualProfile = includeExcavatableMetadata ? new ContentId(reader.ReadUInt32()) : StableId.FromKey("view.placeholder.excavatable.fractured_rock_wall");
            bool openBuildable = !includeExcavatableMetadata || reader.ReadBoolean();
            map._features.Add(new ExcavatableFeature(stableKey, id, rect, terrainClass, requiredEnergy, visualProfile, openBuildable, state));
        }
        map.TopologyVersion = topology;
        return map;
    }
}
}
