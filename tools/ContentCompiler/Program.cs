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
byte[] builtInBytes = PrototypeContentCodec.Write(PrototypeContentFactory.CreateM2Catalog());
if (!contentBytes.AsSpan().SequenceEqual(builtInBytes))
    throw new InvalidDataException("Checked-in prototype source and built-in fallback catalog disagree.");
string contentOutput = Path.Combine(outputDirectory, "PrototypeEntities.contentbin");
File.WriteAllBytes(contentOutput, contentBytes);
PrototypeContentCatalog contentRoundTrip = PrototypeContentCodec.Read(contentBytes);
if (contentRoundTrip.ContentHash != catalog.ContentHash || contentRoundTrip.Entities.Length != catalog.Entities.Length || contentRoundTrip.ResourceNodes.Length != catalog.ResourceNodes.Length || contentRoundTrip.Buildings.Length != catalog.Buildings.Length || contentRoundTrip.Production.Length != catalog.Production.Length || contentRoundTrip.Weapons.Length != catalog.Weapons.Length || contentRoundTrip.Transformations.Length != catalog.Transformations.Length)
    throw new InvalidDataException("Prototype content round-trip validation failed.");

MapDefinition definition = CompileMap(mapSource, catalog);
byte[] mapBytes = CompiledMapCodec.Write(definition);
string mapOutput = Path.Combine(outputDirectory, "DEV_FirstControllableRTS.mapbin");
File.WriteAllBytes(mapOutput, mapBytes);
MapDefinition mapRoundTrip = CompiledMapCodec.ReadDefinition(mapBytes);
if (mapRoundTrip.Grid.Id.Value != definition.Grid.Id.Value || mapRoundTrip.InitialEntities.Length != definition.InitialEntities.Length || mapRoundTrip.InitialResourceNodes.Length != definition.InitialResourceNodes.Length || mapRoundTrip.InitialResourceReceivers.Length != definition.InitialResourceReceivers.Length)
    throw new InvalidDataException("Compiled map round-trip validation failed.");

Console.WriteLine($"Prototype content: {contentOutput} ({contentBytes.Length} bytes), hash={catalog.ContentHash:X16}, entities={catalog.Entities.Length}, resourceDefinitions={catalog.ResourceNodes.Length}, buildingDefinitions={catalog.Buildings.Length}, productionDefinitions={catalog.Production.Length}, weaponDefinitions={catalog.Weapons.Length}, transformationDefinitions={catalog.Transformations.Length}");
Console.WriteLine($"Map: {mapOutput} ({mapBytes.Length} bytes), starts={definition.Starts.Length}, spawns={definition.InitialEntities.Length}, resourceNodes={definition.InitialResourceNodes.Length}, resourceReceivers={definition.InitialResourceReceivers.Length}, features={definition.Grid.Features.Count}");
return 0;

static PrototypeContentCatalog CompilePrototypeCatalog(string path)
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    int schemaVersion = root.GetProperty("schemaVersion").GetInt32();
    if (schemaVersion < 15 || schemaVersion > 16) throw new InvalidDataException("Unsupported prototype content schema.");
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

    List<ResourceNodeDefinition> resourceNodes = new();
    HashSet<string> resourceKeys = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("resourceNodeDefinitions").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        if (!resourceKeys.Add(key)) throw new InvalidDataException($"Duplicate resource node key {key}.");
        uint id = StableId.FromKey(key).Value; if (!stableIds.Add(id)) throw new InvalidDataException($"Stable ID collision at resource node {key}.");
        JsonElement thresholds = item.GetProperty("modelStateThresholdBasisPoints");
        if (thresholds.GetArrayLength() != 3) throw new InvalidDataException($"{key}: modelStateThresholdBasisPoints must contain reduced, low and critical thresholds.");
        resourceNodes.Add(new ResourceNodeDefinition(key,
            Enum.Parse<ResourceType>(RequiredString(item, "type"), false),
            Enum.Parse<ResourceDepositSize>(RequiredString(item, "depositSize"), false),
            item.GetProperty("capacity").GetInt32(),
            Enum.Parse<HarvestInteraction>(RequiredString(item, "harvestInteraction"), false),
            Enum.Parse<ResourceDepletionProfile>(RequiredString(item, "depletionProfile"), false),
            checked((ushort)thresholds[0].GetInt32()), checked((ushort)thresholds[1].GetInt32()), checked((ushort)thresholds[2].GetInt32()),
            RequiredString(item, "viewProfile")));
    }
    resourceNodes.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));

    List<WeaponDefinition> weapons = new();
    Dictionary<string, WeaponDefinition> weaponByKey = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("weaponDefinitions").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        uint id = StableId.FromKey(key).Value; if (!stableIds.Add(id)) throw new InvalidDataException($"Stable ID collision at weapon {key}.");
        WeaponDefinition weapon = new(key, ReadTargetLayerMask(item.GetProperty("legalLayers")), ReadTargetClassMask(item.GetProperty("legalClasses")),
            Enum.Parse<TargetPriorityProfile>(RequiredString(item, "priorityProfile"), false),
            checked((ushort)item.GetProperty("damage").GetInt32()), Enum.Parse<DamageType>(RequiredString(item, "damageType"), false),
            checked((ushort)item.GetProperty("cooldownTicks").GetInt32()), ReadRatio(item, key, "rangeRatio"), ReadRatio(item, key, "minimumRangeRatio"),
            Enum.Parse<WeaponDeliveryKind>(RequiredString(item, "delivery"), false), ReadRatio(item, key, "projectileSpeedRatio"), item.GetProperty("requiresLineOfSight").GetBoolean(),
            checked((ushort)item.GetProperty("facingToleranceDegrees").GetInt32()), checked((ushort)item.GetProperty("maximumMovingFireSpeedBasisPoints").GetInt32()));
        if (!weaponByKey.TryAdd(key, weapon)) throw new InvalidDataException($"Duplicate weapon key {key}.");
        weapons.Add(weapon);
    }
    weapons.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));

    List<PrototypeEntityDefinition> entities = new();
    HashSet<string> entityKeys = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("entities").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        if (!entityKeys.Add(key)) throw new InvalidDataException($"Duplicate entity key {key}.");
        uint id = StableId.FromKey(key).Value; if (!stableIds.Add(id)) throw new InvalidDataException($"Stable ID collision at entity {key}.");
        string movement = RequiredString(item, "movementProfile"); if (!profileKeys.Contains(movement)) throw new InvalidDataException($"{key}: unknown movementProfile {movement}.");
        SelectableKind selectableKind = Enum.Parse<SelectableKind>(RequiredString(item, "selectableKind"), false);
        ushort oreTicks = 0; byte oreCapacity = 0;
        if (item.TryGetProperty("workerHarvest", out JsonElement workerHarvest))
        {
            oreTicks = checked((ushort)workerHarvest.GetProperty("oreTicksPerUnit").GetInt32());
            oreCapacity = checked((byte)workerHarvest.GetProperty("oreCarryCapacity").GetInt32());
        }
        if (selectableKind == SelectableKind.Worker && (oreTicks == 0 || oreCapacity == 0)) throw new InvalidDataException($"{key}: Worker requires workerHarvest metadata.");
        byte operationsCapacity = checked((byte)item.GetProperty("operationsCapacity").GetInt32());
        if (selectableKind == SelectableKind.Building && operationsCapacity != 0) throw new InvalidDataException($"{key}: Buildings cannot consume Operations Capacity.");
        byte visionRadius = checked((byte)item.GetProperty("visionRadius").GetInt32());
        JsonElement combatTarget = item.GetProperty("combatTarget");
        CombatTargetClass targetClass = Enum.Parse<CombatTargetClass>(RequiredString(combatTarget, "class"), false);
        CombatTargetLayer targetLayer = Enum.Parse<CombatTargetLayer>(RequiredString(combatTarget, "layer"), false);
        CombatTargetFlags targetFlags = ReadCombatTargetFlags(combatTarget.GetProperty("flags"));
        ushort maximumHitPoints = checked((ushort)combatTarget.GetProperty("hitPoints").GetInt32());
        byte armorRating = checked((byte)combatTarget.GetProperty("armorRating").GetInt32());
        PrototypeCombatProfile combat;
        if (item.TryGetProperty("weaponProfile", out JsonElement weaponProfileElement))
        {
            string weaponKey = weaponProfileElement.GetString() ?? throw new InvalidDataException($"{key}: weaponProfile must be a string.");
            if (!weaponByKey.TryGetValue(weaponKey, out WeaponDefinition weapon)) throw new InvalidDataException($"{key}: unknown weaponProfile {weaponKey}.");
            Fix32 acquisitionRadius = weapon.Range + Fix32.FromInt(4);
            Fix32 sightRadius = Fix32.FromInt(visionRadius);
            if (acquisitionRadius > sightRadius) acquisitionRadius = sightRadius;
            combat = new PrototypeCombatProfile(targetClass, targetLayer, targetFlags, maximumHitPoints, armorRating,
                weapon.PriorityProfile, weapon.LegalTargetLayers, weapon.LegalTargetClasses, acquisitionRadius, weapon.Id);
        }
        else combat = new PrototypeCombatProfile(targetClass, targetLayer, targetFlags, maximumHitPoints, armorRating);
        entities.Add(new PrototypeEntityDefinition(key, RequiredString(item, "faction"), RequiredString(item, "sourceClassification"), movement,
            Enum.Parse<FootprintClass>(RequiredString(item, "footprint"), false), selectableKind,
            visionRadius, RequiredString(item, "viewProfile"), oreTicks, oreCapacity, operationsCapacity, combat));
    }
    entities.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));

    List<BuildingDefinition> buildings = new();
    HashSet<string> buildingKeys = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("buildingDefinitions").EnumerateArray())
    {
        string key = RequiredString(item, "stableId");
        if (!buildingKeys.Add(key)) throw new InvalidDataException($"Duplicate building definition key {key}.");
        if (!entityKeys.Contains(key)) throw new InvalidDataException($"{key}: building definition has no matching entity definition.");
        JsonElement rows = item.GetProperty("footprintMask");
        byte height = checked((byte)rows.GetArrayLength());
        if (height == 0 || height > 10) throw new InvalidDataException($"{key}: footprintMask height must be 1..10.");
        string firstRow = rows[0].GetString() ?? string.Empty;
        byte width = checked((byte)firstRow.Length);
        if (width == 0 || width > 10) throw new InvalidDataException($"{key}: footprintMask width must be 1..10.");
        ulong mask = 0, maskHigh = 0;
        for (int y = 0; y < height; y++)
        {
            string row = rows[y].GetString() ?? string.Empty;
            if (row.Length != width) throw new InvalidDataException($"{key}: footprintMask rows must share one width.");
            for (int x = 0; x < width; x++)
            {
                int bitIndex = y * width + x;
                if (row[x] == '1')
                {
                    if (bitIndex < 64) mask |= 1UL << bitIndex;
                    else maskHigh |= 1UL << (bitIndex - 64);
                }
                else if (row[x] != '0') throw new InvalidDataException($"{key}: footprintMask accepts only 0 and 1.");
            }
        }
        JsonElement cost = item.GetProperty("cost");
        byte exitWidth = 0, exitDepth = 0; FootprintClass exitFootprint = FootprintClass.Tiny;
        if (item.TryGetProperty("productionExit", out JsonElement exit))
        {
            exitWidth = checked((byte)exit.GetProperty("width").GetInt32());
            exitDepth = checked((byte)exit.GetProperty("depth").GetInt32());
            exitFootprint = Enum.Parse<FootprintClass>(RequiredString(exit, "largestFootprint"), false);
        }
        byte crystalCost = cost.TryGetProperty("crystals", out JsonElement crystals)
            ? checked((byte)crystals.GetInt32()) : (byte)0;
        if (schemaVersion >= 16 && !cost.TryGetProperty("crystals", out _)) throw new InvalidDataException($"{key}: schema 16 building cost requires crystals.");
        buildings.Add(new BuildingDefinition(key, width, height, mask, item.GetProperty("rotatable").GetBoolean(),
            checked((ushort)cost.GetProperty("ore").GetInt32()), checked((ushort)cost.GetProperty("energy").GetInt32()),
            checked((ushort)item.GetProperty("buildTicks").GetInt32()), exitWidth, exitDepth, exitFootprint,
            checked((byte)item.GetProperty("operationsCapacityProvided").GetInt32()),
            checked((ushort)item.GetProperty("energyGenerationPerSecond").GetInt32()),
            checked((ushort)item.GetProperty("energyReserveCapacity").GetInt32()),
            checked((ushort)item.GetProperty("continuousEnergyDemandPerSecond").GetInt32()),
            Enum.Parse<EnergyFunctionalClass>(RequiredString(item, "energyFunctionalClass"), false),
            item.TryGetProperty("worksiteServiceRadius", out JsonElement serviceRadius)
                ? checked((byte)serviceRadius.GetInt32()) : (byte)0,
            crystalCost, maskHigh));
    }
    buildings.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));
    List<UnitProductionDefinition> production = new();
    HashSet<string> producedUnits = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("productionDefinitions").EnumerateArray())
    {
        string unit = RequiredString(item, "unit"), producer = RequiredString(item, "producer");
        if (!entityKeys.Contains(unit)) throw new InvalidDataException($"Production references unknown unit {unit}.");
        if (!buildingKeys.Contains(producer)) throw new InvalidDataException($"Production references unknown producer {producer}.");
        if (!producedUnits.Add(unit)) throw new InvalidDataException($"Duplicate production definition for {unit}.");
        JsonElement cost = item.GetProperty("cost");
        byte operationsCapacity = checked((byte)item.GetProperty("operationsCapacity").GetInt32());
        PrototypeEntityDefinition unitDefinition = entities.Find(e => string.Equals(e.StableKey, unit, StringComparison.Ordinal));
        if (unitDefinition.OperationsCapacity != operationsCapacity) throw new InvalidDataException($"{unit}: production and unit Operations Capacity disagree.");
        production.Add(new UnitProductionDefinition(unit, producer,
            checked((ushort)cost.GetProperty("ore").GetInt32()), checked((ushort)cost.GetProperty("energy").GetInt32()),
            checked((byte)cost.GetProperty("crystals").GetInt32()), operationsCapacity,
            checked((ushort)item.GetProperty("buildTicks").GetInt32())));
    }
    production.Sort((a, b) => string.CompareOrdinal(a.UnitStableKey, b.UnitStableKey));
    List<TransformationDefinition> transformations = new();
    HashSet<string> transformedEntities = new(StringComparer.Ordinal);
    foreach (JsonElement item in root.GetProperty("transformationDefinitions").EnumerateArray())
    {
        string key = RequiredString(item, "stableId"), entityKey = RequiredString(item, "entity");
        if (!entityKeys.Contains(entityKey)) throw new InvalidDataException($"{key}: unknown transformation entity {entityKey}.");
        if (!transformedEntities.Add(entityKey)) throw new InvalidDataException($"{entityKey}: only one primary tactical transformation is allowed.");
        uint definitionId = StableId.FromKey(key).Value; if (!stableIds.Add(definitionId)) throw new InvalidDataException($"Stable ID collision at transformation {key}.");
        JsonElement modes = item.GetProperty("modes");
        if (modes.GetArrayLength() != 2) throw new InvalidDataException($"{key}: exactly two modes are required.");
        TransformationModeDefinition modeA = ReadTransformationMode(modes[0], key, profileKeys, weaponByKey, stableIds);
        TransformationModeDefinition modeB = ReadTransformationMode(modes[1], key, profileKeys, weaponByKey, stableIds);
        transformations.Add(new TransformationDefinition(key, entityKey, modeA, modeB,
            checked((ushort)item.GetProperty("aToBDurationTicks").GetInt32()), checked((ushort)item.GetProperty("bToADurationTicks").GetInt32()),
            checked((ushort)item.GetProperty("cancellationThresholdBasisPoints").GetInt32()), checked((ushort)item.GetProperty("rollbackTicks").GetInt32()),
            checked((ushort)item.GetProperty("reversalLockTicks").GetInt32()), item.GetProperty("moveDuringTransition").GetBoolean(),
            item.GetProperty("attackDuringTransition").GetBoolean(), ReadTargetLayerMask(item.GetProperty("transitionTargetLayers"))));
    }
    transformations.Sort((a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));
    return new PrototypeContentCatalog(profiles.ToArray(), entities.ToArray(), resourceNodes.ToArray(), buildings.ToArray(), production.ToArray(), weapons.ToArray(), transformations.ToArray());
}

static TransformationModeDefinition ReadTransformationMode(JsonElement item, string definitionKey, HashSet<string> profileKeys,
    Dictionary<string, WeaponDefinition> weaponByKey, HashSet<uint> stableIds)
{
    string stateKey = RequiredString(item, "stableId"), movement = RequiredString(item, "movementProfile"), weaponKey = RequiredString(item, "weaponProfile");
    uint stateId = StableId.FromKey(stateKey).Value; if (!stableIds.Add(stateId)) throw new InvalidDataException($"Stable ID collision at transformation state {stateKey}.");
    if (!profileKeys.Contains(movement)) throw new InvalidDataException($"{definitionKey}: unknown mode movement {movement}.");
    if (!weaponByKey.TryGetValue(weaponKey, out WeaponDefinition weapon)) throw new InvalidDataException($"{definitionKey}: unknown mode weapon {weaponKey}.");
    JsonElement combat = item.GetProperty("combatTarget");
    PrototypeCombatProfile combatProfile = new(
        Enum.Parse<CombatTargetClass>(RequiredString(combat, "class"), false),
        Enum.Parse<CombatTargetLayer>(RequiredString(combat, "layer"), false),
        ReadCombatTargetFlags(combat.GetProperty("flags")),
        checked((ushort)combat.GetProperty("hitPoints").GetInt32()), checked((byte)combat.GetProperty("armorRating").GetInt32()),
        weapon.PriorityProfile, weapon.LegalTargetLayers, weapon.LegalTargetClasses, ReadRatio(item, stateKey, "acquisitionRadiusRatio"), weapon.Id);
    return new TransformationModeDefinition(stateKey, RequiredString(item, "displayName"), movement,
        Enum.Parse<FootprintClass>(RequiredString(item, "footprint"), false), checked((byte)item.GetProperty("visionRadius").GetInt32()),
        RequiredString(item, "viewProfile"), combatProfile);
}

static MapDefinition CompileMap(string path, PrototypeContentCatalog catalog)
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    int schemaVersion = root.GetProperty("schemaVersion").GetInt32();
    if (schemaVersion != 4) throw new InvalidDataException($"Unsupported map source schema {schemaVersion}.");
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
    StableIdRegistry featureStableIds = new();
    foreach (JsonElement item in root.GetProperty("excavatableFeatures").EnumerateArray())
    {
        ushort id = checked((ushort)item.GetProperty("featureId").GetInt32()); if (id == 0) throw new InvalidDataException("Excavatable Feature ID 0 is reserved."); if (!featureIds.Add(id)) throw new InvalidDataException($"Duplicate Excavatable Feature ID {id}.");
        string stableKey = RequiredString(item, "stableId"); featureStableIds.Register(stableKey);
        ExcavatableTerrainClass terrainClass = Enum.Parse<ExcavatableTerrainClass>(RequiredString(item, "class"), false);
        if (terrainClass < ExcavatableTerrainClass.LooseRubbleBlockage || terrainClass > ExcavatableTerrainClass.ReinforcedBedrockBarrier)
            throw new InvalidDataException($"Excavatable Feature {id} has invalid terrain class.");
        ushort requiredEnergy = checked((ushort)item.GetProperty("requiredEnergy").GetInt32());
        ushort canonicalEnergy = terrainClass == ExcavatableTerrainClass.LooseRubbleBlockage ? (ushort)0
            : terrainClass == ExcavatableTerrainClass.ReinforcedBedrockBarrier ? (ushort)50 : (ushort)25;
        if (requiredEnergy != canonicalEnergy) throw new InvalidDataException($"Excavatable Feature {id} Energy must be {canonicalEnergy} for {terrainClass}.");
        string initialStateText = RequiredString(item, "initialState");
        ExcavatableFeatureState state = initialStateText switch
        {
            "Blocked" => ExcavatableFeatureState.Blocked,
            "Open" => ExcavatableFeatureState.Open,
            _ => throw new InvalidDataException($"Excavatable Feature {id} has invalid authored initial state {initialStateText}.")
        };
        IntRect rect=ReadRect(item.GetProperty("navRect")); ValidateRect(rect, $"Excavatable Feature {id}");
        map.AddExcavatable(new ExcavatableFeature(stableKey, id, rect, terrainClass, requiredEnergy,
            StableId.FromKey(RequiredString(item, "visualProfile")), item.GetProperty("openBuildable").GetBoolean(), state));
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

    List<InitialResourceNodeSpawn> resourceNodes = new();
    foreach (JsonElement item in root.GetProperty("resourceNodes").EnumerateArray())
    {
        string contentKey = RequiredString(item, "contentKey");
        if (!catalog.ContainsResourceNodeKey(contentKey)) throw new InvalidDataException($"Map resource node references unknown prototype content key {contentKey}.");
        FixVec2 position = ReadBuildPosition(item.GetProperty("position")); ValidateBuildPosition(position, $"resource node {contentKey}");
        NavCell cell = MapGrid.BuildToNav(position);
        if ((map.GetFlags(cell.X, cell.Y) & MapCellFlags.Impassable) != 0) throw new InvalidDataException($"{contentKey}: resource node is authored on impassable terrain.");
        resourceNodes.Add(new InitialResourceNodeSpawn(contentKey, position));
    }

    List<InitialResourceReceiverSpawn> receivers = new();
    foreach (JsonElement item in root.GetProperty("resourceReceivers").EnumerateArray())
    {
        byte player = checked((byte)item.GetProperty("playerSlot").GetInt32());
        string contentKey = RequiredString(item, "contentKey");
        if (!catalog.TryGetEntity(contentKey, out PrototypeEntityDefinition receiverDefinition) || receiverDefinition.SelectableKind != SelectableKind.Building)
            throw new InvalidDataException($"Map receiver references invalid building content key {contentKey}.");
        FixVec2 position = ReadBuildPosition(item.GetProperty("position")); ValidateBuildPosition(position, $"resource receiver {contentKey}");
        receivers.Add(new InitialResourceReceiverSpawn(player, contentKey, position));
    }

    List<VisionTestRegion> visionRegions = new();
    foreach (JsonElement item in root.GetProperty("visionTestGeometry").EnumerateArray()) { IntRect rect=ReadRect(item.GetProperty("navRect")); ValidateRect(rect,"vision geometry"); visionRegions.Add(new VisionTestRegion(RequiredString(item, "name"), rect)); }
    return new MapDefinition(map, starts.ToArray(), spawns.ToArray(), visionRegions.ToArray(), resourceNodes.ToArray(), receivers.ToArray());
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

static CombatTargetFlags ReadCombatTargetFlags(JsonElement element)
{
    CombatTargetFlags flags = CombatTargetFlags.None;
    foreach (JsonElement flag in element.EnumerateArray()) flags |= Enum.Parse<CombatTargetFlags>(flag.GetString() ?? string.Empty, false);
    return flags;
}

static TargetLayerMask ReadTargetLayerMask(JsonElement element)
{
    TargetLayerMask mask = TargetLayerMask.None;
    foreach (JsonElement layer in element.EnumerateArray())
        mask |= Enum.Parse<CombatTargetLayer>(layer.GetString() ?? string.Empty, false) == CombatTargetLayer.Ground ? TargetLayerMask.Ground : TargetLayerMask.TrueAir;
    return mask;
}

static TargetClassMask ReadTargetClassMask(JsonElement element)
{
    TargetClassMask mask = TargetClassMask.None;
    foreach (JsonElement item in element.EnumerateArray())
    {
        CombatTargetClass targetClass = Enum.Parse<CombatTargetClass>(item.GetString() ?? string.Empty, false);
        mask |= (TargetClassMask)(1 << (int)targetClass);
    }
    return mask;
}
