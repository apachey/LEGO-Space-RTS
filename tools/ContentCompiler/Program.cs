using System.Text.Json;
using LegoSpaceRTS.SimCore;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: ContentCompiler <PrototypeEntities.json> <source.map.json> <output-directory>");
    return 2;
}

string contentSource = args[0];
string mapSource = args[1];
string outputDirectory = args[2];
Directory.CreateDirectory(outputDirectory);

PrototypeContentCatalog catalog = CompilePrototypeCatalog(contentSource);
byte[] contentBytes = PrototypeContentCodec.Write(catalog);
string contentOutput = Path.Combine(outputDirectory, "PrototypeEntities.contentbin");
File.WriteAllBytes(contentOutput, contentBytes);
PrototypeContentCatalog contentRoundTrip = PrototypeContentCodec.Read(contentBytes);
if (contentRoundTrip.ContentHash != catalog.ContentHash || contentRoundTrip.Entities.Length != catalog.Entities.Length)
    throw new InvalidDataException("Prototype content round-trip validation failed.");

MapDefinition definition = CompileMap(mapSource, catalog);
byte[] mapBytes = CompiledMapCodec.Write(definition);
string mapOutput = Path.Combine(outputDirectory, "DEV_FirstControllableRTS.mapbin");
File.WriteAllBytes(mapOutput, mapBytes);
MapDefinition mapRoundTrip = CompiledMapCodec.ReadDefinition(mapBytes);
if (mapRoundTrip.Grid.Id.Value != definition.Grid.Id.Value || mapRoundTrip.InitialEntities.Length != definition.InitialEntities.Length)
    throw new InvalidDataException("Compiled map round-trip validation failed.");

Console.WriteLine($"Prototype content: {contentOutput} ({contentBytes.Length} bytes), hash={catalog.ContentHash:X16}, entities={catalog.Entities.Length}");
Console.WriteLine($"Map: {mapOutput} ({mapBytes.Length} bytes), starts={definition.Starts.Length}, spawns={definition.InitialEntities.Length}, features={definition.Grid.Features.Count}");
return 0;

static PrototypeContentCatalog CompilePrototypeCatalog(string path)
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    if (root.GetProperty("schemaVersion").GetInt32() != 2) throw new InvalidDataException("Unsupported prototype content schema.");
    if (!string.Equals(root.GetProperty("contentKind").GetString(), "prototype_entities", StringComparison.Ordinal)) throw new InvalidDataException("Unexpected contentKind.");

    List<PrototypeMovementProfile> profiles = new();
    HashSet<string> profileKeys = new(StringComparer.Ordinal);
    HashSet<uint> stableIds = new();
    foreach (JsonElement item in root.GetProperty("movementProfiles").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        if (!profileKeys.Add(key)) throw new InvalidDataException($"Duplicate movement profile key {key}.");
        uint id = StableId.FromKey(key).Value; if (!stableIds.Add(id)) throw new InvalidDataException($"Stable ID collision at movement profile {key}.");
        Fix32 speed = ReadRatio(item,key,"speedRatio");
        Fix32 acceleration = ReadRatio(item,key,"accelerationRatio");
        Fix32 deceleration = ReadRatio(item,key,"decelerationRatio");
        ushort turnRate=checked((ushort)item.GetProperty("turnRatePerTick").GetInt32());
        ReversePolicy reverse=Enum.Parse<ReversePolicy>(RequiredString(item,"reversePolicy"),false);
        MovementLayer layer = Enum.Parse<MovementLayer>(RequiredString(item, "layer"), false);
        profiles.Add(new PrototypeMovementProfile(key, speed, acceleration, deceleration, turnRate, reverse, layer));
    }
    profiles.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));

    List<PrototypeEntityDefinition> entities = new();
    HashSet<string> entityKeys = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("entities").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        if (!entityKeys.Add(key)) throw new InvalidDataException($"Duplicate entity key {key}.");
        uint id = StableId.FromKey(key).Value; if (!stableIds.Add(id)) throw new InvalidDataException($"Stable ID collision at entity {key}.");
        string movement = RequiredString(item, "movementProfile"); if (!profileKeys.Contains(movement)) throw new InvalidDataException($"{key}: unknown movementProfile {movement}.");
        entities.Add(new PrototypeEntityDefinition(key, RequiredString(item, "faction"), RequiredString(item, "sourceClassification"), movement,
            Enum.Parse<FootprintClass>(RequiredString(item, "footprint"), false), Enum.Parse<SelectableKind>(RequiredString(item, "selectableKind"), false),
            checked((byte)item.GetProperty("visionRadius").GetInt32()), RequiredString(item, "viewProfile")));
    }
    entities.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));
    return new PrototypeContentCatalog(profiles.ToArray(), entities.ToArray());
}

static MapDefinition CompileMap(string path, PrototypeContentCatalog catalog)
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    int schemaVersion = root.GetProperty("schemaVersion").GetInt32();
    if (schemaVersion != 1) throw new InvalidDataException($"Unsupported map source schema {schemaVersion}.");
    string stableId = RequiredString(root, "stableId");
    int width = root.GetProperty("buildSize")[0].GetInt32();
    int height = root.GetProperty("buildSize")[1].GetInt32();
    int navScale = root.GetProperty("navScale").GetInt32();
    if (stableId != DevMapFactory.StableKey || width != MapGrid.BuildWidth || height != MapGrid.BuildHeight || navScale != MapGrid.NavPerBuild)
        throw new InvalidDataException("DEV_FirstControllableRTS source does not match canonical M2 dimensions/stable ID.");

    MapGrid map = new(stableId);
    foreach (JsonElement item in root.GetProperty("flagRects").EnumerateArray()) { IntRect rect=ReadRect(item.GetProperty("rect")); ValidateRect(rect, "flag rect"); map.SetFlagsRect(rect, ReadFlags(item.GetProperty("add")), ReadFlags(item.GetProperty("remove"))); }
    foreach (JsonElement item in root.GetProperty("elevationRects").EnumerateArray()) { IntRect rect=ReadRect(item.GetProperty("rect")); ValidateRect(rect, "elevation rect"); map.SetElevationRect(rect, checked((sbyte)item.GetProperty("band").GetInt32())); }

    HashSet<ushort> featureIds = new();
    foreach (JsonElement item in root.GetProperty("excavatableFeatures").EnumerateArray())
    {
        ushort id = checked((ushort)item.GetProperty("featureId").GetInt32()); if (id == 0) throw new InvalidDataException("Excavatable Feature ID 0 is reserved."); if (!featureIds.Add(id)) throw new InvalidDataException($"Duplicate Excavatable Feature ID {id}.");
        bool open = string.Equals(item.GetProperty("initialState").GetString(), "Open", StringComparison.Ordinal);
        IntRect rect=ReadRect(item.GetProperty("navRect")); ValidateRect(rect, $"Excavatable Feature {id}");
        map.AddExcavatable(new ExcavatableFeature(id, rect, open));
    }

    List<MapStart> starts = new();
    foreach (JsonElement item in root.GetProperty("starts").EnumerateArray())
    {
        FixVec2 position=ReadBuildPosition(item.GetProperty("position")); ValidateBuildPosition(position,"start");
        starts.Add(new MapStart(checked((byte)item.GetProperty("playerSlot").GetInt32()), position));
    }

    HierarchicalPathfinder placementPathfinder = new HierarchicalPathfinder(map);
    List<InitialEntitySpawn> spawns = new();
    foreach (JsonElement item in root.GetProperty("initialEntities").EnumerateArray())
    {
        byte player = checked((byte)item.GetProperty("playerSlot").GetInt32()); string contentKey = RequiredString(item, "contentKey");
        if (!catalog.ContainsEntityKey(contentKey)) throw new InvalidDataException($"Map spawn references unknown prototype content key {contentKey}.");
        FootprintClass footprint = Enum.Parse<FootprintClass>(RequiredString(item, "footprint"), false);
        MovementLayer layer = Enum.Parse<MovementLayer>(RequiredString(item, "movementLayer"), false);
        SelectableKind kind = Enum.Parse<SelectableKind>(RequiredString(item, "selectableKind"), false);
        byte vision = checked((byte)item.GetProperty("visionRadius").GetInt32());
        FixVec2 position=ReadBuildPosition(item.GetProperty("position")); ValidateBuildPosition(position,$"spawn {contentKey}");
        if (!catalog.TryGetEntity(contentKey,out PrototypeEntityDefinition definition)) throw new InvalidDataException($"Map spawn references unknown prototype content key {contentKey}.");
        if (!catalog.TryGetMovement(definition.MovementProfileKey,out PrototypeMovementProfile movement)) throw new InvalidDataException($"{contentKey}: compiled movement profile missing.");
        if (definition.Footprint!=footprint||definition.SelectableKind!=kind||definition.VisionRadius!=vision||movement.Layer!=layer) throw new InvalidDataException($"{contentKey}: map spawn metadata disagrees with prototype content definition.");
        if (layer!=MovementLayer.TrueAir && !placementPathfinder.IsPassable(MapGrid.BuildToNav(position),footprint)) throw new InvalidDataException($"{contentKey}: prototype spawn is not pathable for its footprint.");
        spawns.Add(new InitialEntitySpawn(player, contentKey, position, footprint, layer, kind, vision));
    }

    List<VisionTestRegion> visionRegions = new();
    foreach (JsonElement item in root.GetProperty("visionTestGeometry").EnumerateArray()) { IntRect rect=ReadRect(item.GetProperty("navRect")); ValidateRect(rect,"vision geometry"); visionRegions.Add(new VisionTestRegion(RequiredString(item, "name"), rect)); }
    return new MapDefinition(map, starts.ToArray(), spawns.ToArray(), visionRegions.ToArray());
}

static Fix32 ReadRatio(JsonElement item,string key,string property)
{
    JsonElement ratio=item.GetProperty(property); if(ratio.GetArrayLength()!=2) throw new InvalidDataException($"{key}: {property} must be [numerator,denominator].");
    return Fix32.FromRatio(ratio[0].GetInt32(),ratio[1].GetInt32());
}

static string RequiredString(JsonElement element, string property)
    => element.GetProperty(property).GetString() ?? throw new InvalidDataException($"{property} missing");

static IntRect ReadRect(JsonElement element)
{
    if (element.GetArrayLength() != 4) throw new InvalidDataException("Rect requires [x,y,width,height].");
    return new IntRect(checked((short)element[0].GetInt32()), checked((short)element[1].GetInt32()), checked((short)element[2].GetInt32()), checked((short)element[3].GetInt32()));
}

static FixVec2 ReadBuildPosition(JsonElement element)
{
    if (element.GetArrayLength() != 2) throw new InvalidDataException("Position requires [x,y].");
    return FixVec2.FromInts(element[0].GetInt32(), element[1].GetInt32());
}

static void ValidateRect(IntRect rect,string label)
{
    if(rect.Width<=0||rect.Height<=0||rect.X<0||rect.Y<0||rect.X+rect.Width>MapGrid.NavWidth||rect.Y+rect.Height>MapGrid.NavHeight)
        throw new InvalidDataException($"{label} is outside the 320x320 nav raster: [{rect.X},{rect.Y},{rect.Width},{rect.Height}].");
}

static void ValidateBuildPosition(FixVec2 position,string label)
{
    if(position.X<Fix32.Zero||position.Y<Fix32.Zero||position.X>=Fix32.FromInt(MapGrid.BuildWidth)||position.Y>=Fix32.FromInt(MapGrid.BuildHeight))
        throw new InvalidDataException($"{label} is outside the 160x160 build grid.");
}

static MapCellFlags ReadFlags(JsonElement element)
{
    MapCellFlags flags = MapCellFlags.None;
    foreach (JsonElement flag in element.EnumerateArray()) flags |= Enum.Parse<MapCellFlags>(flag.GetString() ?? string.Empty, false);
    return flags;
}
