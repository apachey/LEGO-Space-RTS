using System;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct MapStart
{
    public readonly byte PlayerSlot;
    public readonly FixVec2 Position;
    public MapStart(byte playerSlot, FixVec2 position) { PlayerSlot = playerSlot; Position = position; }
}

public readonly struct InitialEntitySpawn
{
    public readonly byte PlayerSlot;
    public readonly string ContentKey;
    public readonly FixVec2 Position;
    public readonly FootprintClass Footprint;
    public readonly MovementLayer Layer;
    public readonly SelectableKind SelectableKind;
    public readonly byte VisionRadius;

    public InitialEntitySpawn(byte playerSlot, string contentKey, FixVec2 position, FootprintClass footprint,
        MovementLayer layer, SelectableKind selectableKind, byte visionRadius)
    {
        PlayerSlot = playerSlot;
        ContentKey = contentKey ?? throw new ArgumentNullException(nameof(contentKey));
        Position = position;
        Footprint = footprint;
        Layer = layer;
        SelectableKind = selectableKind;
        VisionRadius = visionRadius;
    }
}

public readonly struct VisionTestRegion
{
    public readonly string Name;
    public readonly IntRect NavRect;
    public VisionTestRegion(string name, IntRect navRect) { Name = name ?? throw new ArgumentNullException(nameof(name)); NavRect = navRect; }
}

/// <summary>
/// Runtime map definition compiled from human-editable map source. MapGrid carries the dense
/// pathing/elevation/topology raster; the remaining fields carry authored starts, prototype spawns,
/// and engineering geometry metadata needed by M2 tools/tests.
/// </summary>
public sealed class MapDefinition
{
    public MapGrid Grid { get; }
    public MapStart[] Starts { get; }
    public InitialEntitySpawn[] InitialEntities { get; }
    public VisionTestRegion[] VisionTestGeometry { get; }

    public MapDefinition(MapGrid grid, MapStart[] starts, InitialEntitySpawn[] initialEntities, VisionTestRegion[] visionTestGeometry)
    {
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        Starts = starts ?? Array.Empty<MapStart>();
        InitialEntities = initialEntities ?? Array.Empty<InitialEntitySpawn>();
        VisionTestGeometry = visionTestGeometry ?? Array.Empty<VisionTestRegion>();
    }

    internal void Serialize(BinaryWriter writer)
    {
        Grid.Serialize(writer);
        writer.Write(Starts.Length);
        for (int i = 0; i < Starts.Length; i++)
        {
            writer.Write(Starts[i].PlayerSlot);
            writer.Write(Starts[i].Position.X.Raw);
            writer.Write(Starts[i].Position.Y.Raw);
        }
        writer.Write(InitialEntities.Length);
        for (int i = 0; i < InitialEntities.Length; i++)
        {
            InitialEntitySpawn s = InitialEntities[i];
            writer.Write(s.PlayerSlot); writer.Write(s.ContentKey);
            writer.Write(s.Position.X.Raw); writer.Write(s.Position.Y.Raw);
            writer.Write((byte)s.Footprint); writer.Write((byte)s.Layer); writer.Write((byte)s.SelectableKind); writer.Write(s.VisionRadius);
        }
        writer.Write(VisionTestGeometry.Length);
        for (int i = 0; i < VisionTestGeometry.Length; i++)
        {
            VisionTestRegion v = VisionTestGeometry[i];
            writer.Write(v.Name); writer.Write(v.NavRect.X); writer.Write(v.NavRect.Y); writer.Write(v.NavRect.Width); writer.Write(v.NavRect.Height);
        }
    }

    internal static MapDefinition Deserialize(BinaryReader reader)
    {
        MapGrid grid = MapGrid.Deserialize(reader);
        int startCount = reader.ReadInt32();
        MapStart[] starts = new MapStart[startCount];
        for (int i = 0; i < startCount; i++) starts[i] = new MapStart(reader.ReadByte(), new FixVec2(Fix32.FromRaw(reader.ReadInt32()), Fix32.FromRaw(reader.ReadInt32())));
        int spawnCount = reader.ReadInt32();
        InitialEntitySpawn[] spawns = new InitialEntitySpawn[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            byte player = reader.ReadByte(); string key = reader.ReadString();
            FixVec2 position = new FixVec2(Fix32.FromRaw(reader.ReadInt32()), Fix32.FromRaw(reader.ReadInt32()));
            FootprintClass fp = (FootprintClass)reader.ReadByte(); MovementLayer layer = (MovementLayer)reader.ReadByte();
            SelectableKind kind = (SelectableKind)reader.ReadByte(); byte vision = reader.ReadByte();
            spawns[i] = new InitialEntitySpawn(player, key, position, fp, layer, kind, vision);
        }
        int visionCount = reader.ReadInt32();
        VisionTestRegion[] visionRegions = new VisionTestRegion[visionCount];
        for (int i = 0; i < visionCount; i++)
            visionRegions[i] = new VisionTestRegion(reader.ReadString(), new IntRect(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()));
        return new MapDefinition(grid, starts, spawns, visionRegions);
    }
}
}
