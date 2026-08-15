using System.IO;

namespace LegoSpaceRTS.SimCore
{
public static class CompiledMapCodec
{
    public const uint Magic = 0x4D525453; // MRTS
    public const ushort Version = 4;

    public static byte[] Write(MapDefinition definition)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        writer.Write(Magic); writer.Write(Version); definition.Serialize(writer); writer.Flush();
        return ms.ToArray();
    }

    public static byte[] Write(MapGrid map) => Write(new MapDefinition(map, System.Array.Empty<MapStart>(), System.Array.Empty<InitialEntitySpawn>(), System.Array.Empty<VisionTestRegion>()));

    public static MapDefinition ReadDefinition(byte[] bytes)
    {
        using MemoryStream ms = new(bytes, false);
        using BinaryReader reader = new(ms);
        if (reader.ReadUInt32() != Magic) throw new InvalidDataException("Compiled map magic mismatch.");
        ushort version = reader.ReadUInt16();
        if (version < 1 || version > Version) throw new InvalidDataException("Compiled map version mismatch.");
        MapDefinition definition = MapDefinition.Deserialize(reader, version);
        if (ms.Position != ms.Length) throw new InvalidDataException("Trailing compiled map bytes.");
        return definition;
    }

    public static MapGrid Read(byte[] bytes) => ReadDefinition(bytes).Grid;
}
}
