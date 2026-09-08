using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Stable roster references shared by validation and their authoritative runtime consumers.</summary>
public static class CanonicalRosterReferences
{
    public const string T3Trike = "unit.astronauts.t3_trike";
    public const string SolarExplorer = "unit.astronauts.solar_explorer";
    public const string MartianWorkerRobot = "unit.martians.worker_robot";
    public const string MartianDoubleHover = "unit.martians.double_hover";
    public const string MartianJetScooter = "unit.martians.jet_scooter";
    public const string MartianExcavationSearcher = "unit.martians.excavation_searcher";
    public const string AlienRazorSkimmer = "unit.aliens.razor_skimmer";

    private static readonly IReadOnlyList<string> TubeEligibleKeys = Array.AsReadOnly(new[]
    {
        MartianWorkerRobot,
        MartianDoubleHover,
        MartianJetScooter
    });

    public static IReadOnlyList<string> TubeEligibleUnitKeys => TubeEligibleKeys;

    public static bool IsTubeEligible(ContentId contentType)
    {
        for (int i = 0; i < TubeEligibleKeys.Count; i++)
            if (StableId.FromKey(TubeEligibleKeys[i]) == contentType) return true;
        return false;
    }
}

/// <summary>
/// Shipping-roster closure checks. Generic and legacy catalogs remain readable; the content build
/// calls this stricter validator for the complete M8 roster and its authored map.
/// </summary>
public static class CanonicalRosterValidator
{
    private static readonly HashSet<string> Factions = new(StringComparer.Ordinal)
    {
        "RockRaiders", "Astronauts", "Aliens", "Martians"
    };

    private static readonly Dictionary<string, int> ExpectedUnitCounts = new(StringComparer.Ordinal)
    {
        ["RockRaiders"] = 8,
        ["Astronauts"] = 13,
        ["Aliens"] = 6,
        ["Martians"] = 8
    };

    private static readonly Dictionary<string, int> ExpectedBuildingCounts = new(StringComparer.Ordinal)
    {
        ["RockRaiders"] = 8,
        ["Astronauts"] = 8,
        ["Aliens"] = 6,
        ["Martians"] = 9
    };

    public static void Validate(PrototypeContentCatalog catalog, MapDefinition map)
        => Validate(catalog, map, CanonicalRosterReferences.TubeEligibleUnitKeys);

    public static void Validate(PrototypeContentCatalog catalog, MapDefinition map, IReadOnlyList<string> tubeEligibleUnitKeys)
    {
        if (catalog is null) throw new ArgumentNullException(nameof(catalog));
        if (map is null) throw new ArgumentNullException(nameof(map));
        if (tubeEligibleUnitKeys is null) throw new ArgumentNullException(nameof(tubeEligibleUnitKeys));
        ValidateStableIds(catalog);
        ValidateRosterContent(catalog);
        ValidateTubeEligibility(catalog, tubeEligibleUnitKeys);
        ValidateProductionExits(catalog);
        ValidateMap(catalog, map);
    }

    private static void ValidateStableIds(PrototypeContentCatalog catalog)
    {
        Dictionary<uint, string> ids = new();
        for (int i = 0; i < catalog.MovementProfiles.Length; i++) AddId(ids, catalog.MovementProfiles[i].StableKey, catalog.MovementProfiles[i].Id);
        for (int i = 0; i < catalog.Entities.Length; i++) AddId(ids, catalog.Entities[i].StableKey, catalog.Entities[i].Id);
        for (int i = 0; i < catalog.ResourceNodes.Length; i++) AddId(ids, catalog.ResourceNodes[i].StableKey, catalog.ResourceNodes[i].Id);
        for (int i = 0; i < catalog.Weapons.Length; i++) AddId(ids, catalog.Weapons[i].StableKey, catalog.Weapons[i].Id);
        for (int i = 0; i < catalog.Transformations.Length; i++)
        {
            TransformationDefinition transformation = catalog.Transformations[i];
            AddId(ids, transformation.StableKey, transformation.Id);
            AddId(ids, transformation.ModeA.StateKey, transformation.ModeA.StateId);
            AddId(ids, transformation.ModeB.StateKey, transformation.ModeB.StateId);
        }
        for (int i = 0; i < catalog.Research.Length; i++) AddId(ids, catalog.Research[i].StableKey, catalog.Research[i].Id);
        for (int i = 0; i < catalog.Commands.Length; i++) AddId(ids, catalog.Commands[i].StableKey, catalog.Commands[i].Id);
    }

    private static void AddId(Dictionary<uint, string> ids, string key, ContentId id)
    {
        if (string.IsNullOrWhiteSpace(key) || id.Value == 0 || StableId.FromKey(key) != id)
            Fail("T074_INVALID_ID", $"Content key '{key}' has an invalid stable ID.");
        if (ids.TryGetValue(id.Value, out string? prior))
            Fail("T074_DUPLICATE_ID", $"Stable ID {id.Value} is shared by '{prior}' and '{key}'.");
        ids.Add(id.Value, key);
    }

    private static void ValidateRosterContent(PrototypeContentCatalog catalog)
    {
        Dictionary<string, int> units = EmptyFactionCounts();
        Dictionary<string, int> buildings = EmptyFactionCounts();
        HashSet<string> entityKeys = new(StringComparer.Ordinal);
        for (int i = 0; i < catalog.Entities.Length; i++)
        {
            PrototypeEntityDefinition entity = catalog.Entities[i];
            if (!entityKeys.Add(entity.StableKey))
                Fail("T074_DUPLICATE_ID", $"Duplicate entity definition '{entity.StableKey}'.");
            if (!catalog.TryGetMovement(entity.MovementProfileKey, out _))
                Fail("T074_UNRESOLVED_MOVEMENT", $"Entity '{entity.StableKey}' references missing movement profile '{entity.MovementProfileKey}'.");
            RequireEntityVisual(entity.StableKey, entity.ViewProfileKey);
            if (string.IsNullOrWhiteSpace(entity.SourceClassification))
                Fail("T074_MISSING_SOURCE_CLASSIFICATION", $"Roster entity '{entity.StableKey}' has no source classification.");
            if (entity.Combat.WeaponProfile.Value != 0 && !catalog.TryGetWeapon(entity.Combat.WeaponProfile, out _))
                Fail("T074_INVALID_WEAPON", $"Entity '{entity.StableKey}' references a missing weapon profile.");

            bool unit = entity.StableKey.StartsWith("unit.", StringComparison.Ordinal);
            bool building = entity.SelectableKind == SelectableKind.Building;
            if (!unit && !building) continue;
            if (!Factions.Contains(entity.FactionKey))
                Fail("T074_INVALID_FACTION", $"Roster entity '{entity.StableKey}' references invalid faction '{entity.FactionKey}'.");
            Dictionary<string, int> counts = unit ? units : buildings;
            counts[entity.FactionKey]++;
            if (unit && (entity.OperationsCapacity == 0 || !entity.Combat.IsTargetable))
                Fail("T074_INCOMPLETE_UNIT", $"Unit '{entity.StableKey}' is missing Operations Capacity or durability.");
            if (building && (entity.OperationsCapacity != 0 || !entity.Combat.IsTargetable))
                Fail("T074_INCOMPLETE_BUILDING", $"Building entity '{entity.StableKey}' has invalid OC or durability metadata.");
        }

        ValidateCounts("unit", units, ExpectedUnitCounts, 35);
        ValidateCounts("building", buildings, ExpectedBuildingCounts, 31);
        if (catalog.Buildings.Length != 31)
            Fail("T074_ROSTER_COUNT", $"Expected 31 infrastructure definitions, found {catalog.Buildings.Length}.");
        if (catalog.Production.Length != 35)
            Fail("T074_ROSTER_COUNT", $"Expected 35 unit production definitions, found {catalog.Production.Length}.");
        if (catalog.Research.Length != 38)
            Fail("T074_ROSTER_COUNT", $"Expected 38 research definitions, found {catalog.Research.Length}.");
        if (catalog.Commands.Length != 35)
            Fail("T074_ROSTER_COUNT", $"Expected 35 command definitions, found {catalog.Commands.Length}.");

        HashSet<string> buildingDefinitionKeys = new(StringComparer.Ordinal);
        for (int i = 0; i < catalog.Buildings.Length; i++)
        {
            BuildingDefinition building = catalog.Buildings[i];
            if (!buildingDefinitionKeys.Add(building.StableKey))
                Fail("T074_DUPLICATE_ID", $"Duplicate building definition '{building.StableKey}'.");
            if (!catalog.TryGetEntity(building.Id, out PrototypeEntityDefinition entity) ||
                entity.SelectableKind != SelectableKind.Building || !string.Equals(entity.StableKey, building.StableKey, StringComparison.Ordinal))
                Fail("T074_UNRESOLVED_BUILDING", $"Infrastructure '{building.StableKey}' has no matching building entity.");
            if (building.OreCost == 0 || building.BuildTicks == 0)
                Fail("T074_INCOMPLETE_BUILDING", $"Infrastructure '{building.StableKey}' is missing cost or build time.");
        }
        foreach (PrototypeEntityDefinition entity in catalog.Entities)
            if (entity.SelectableKind == SelectableKind.Building && !buildingDefinitionKeys.Contains(entity.StableKey))
                Fail("T074_UNRESOLVED_BUILDING", $"Building entity '{entity.StableKey}' has no infrastructure definition.");

        HashSet<string> productionUnits = new(StringComparer.Ordinal);
        for (int i = 0; i < catalog.Production.Length; i++) productionUnits.Add(catalog.Production[i].UnitStableKey);
        foreach (PrototypeEntityDefinition entity in catalog.Entities)
            if (entity.StableKey.StartsWith("unit.", StringComparison.Ordinal) && !productionUnits.Contains(entity.StableKey))
                Fail("T074_MISSING_PRODUCTION", $"Unit '{entity.StableKey}' has no production definition.");

        for (int i = 0; i < catalog.ResourceNodes.Length; i++)
        {
            ResourceNodeDefinition resource = catalog.ResourceNodes[i];
            RequireVisual(resource.StableKey, resource.ViewProfileKey, $"view.placeholder.{resource.StableKey}");
        }
        for (int i = 0; i < catalog.Transformations.Length; i++)
        {
            TransformationDefinition transformation = catalog.Transformations[i];
            RequireVisual(transformation.ModeA.StateKey, transformation.ModeA.ViewProfileKey, "view.placeholder.astronauts.mx41_ground");
            RequireVisual(transformation.ModeB.StateKey, transformation.ModeB.ViewProfileKey, "view.placeholder.astronauts.mx41_flight");
        }
        for (int i = 0; i < catalog.Research.Length; i++)
        {
            ResearchDefinition research = catalog.Research[i];
            if (!string.Equals(research.PresentationProfileKey, $"presentation.{research.StableKey}", StringComparison.Ordinal))
                Fail("T074_MISSING_VISUAL_PROFILE", $"Research '{research.StableKey}' has no resolved presentation profile.");
            if (!string.Equals(research.DisplayNameLocKey, $"loc.{research.StableKey}.name", StringComparison.Ordinal))
                Fail("T074_MISSING_LOCALIZATION", $"Research '{research.StableKey}' has no resolved localization key.");
        }
        for (int i = 0; i < catalog.Commands.Length; i++)
        {
            CommandDefinition command = catalog.Commands[i];
            if (!command.ValidationHandlerId.StartsWith("validate.", StringComparison.Ordinal) ||
                !command.ExecutionHandlerId.StartsWith("execute.", StringComparison.Ordinal))
                Fail("T074_UNRESOLVED_COMMAND", $"Command '{command.StableKey}' has an unresolved runtime handler.");
            if (!command.UiSlotProfile.StartsWith("slot.", StringComparison.Ordinal) ||
                !command.TargetingPreviewProfile.StartsWith("preview.", StringComparison.Ordinal))
                Fail("T074_UNRESOLVED_COMMAND", $"Command '{command.StableKey}' has an unresolved UI or targeting profile.");
        }
    }

    private static void ValidateTubeEligibility(PrototypeContentCatalog catalog, IReadOnlyList<string> keys)
    {
        HashSet<string> expected = new(CanonicalRosterReferences.TubeEligibleUnitKeys, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (!actual.Add(key) || !catalog.TryGetEntity(key, out PrototypeEntityDefinition entity) ||
                entity.FactionKey != "Martians" || entity.SelectableKind == SelectableKind.Building || entity.Footprint > FootprintClass.Small)
                Fail("T074_ILLEGAL_TUBE_UNIT", $"Aero Tube eligibility contains invalid unit '{key}'.");
        }
        if (!actual.SetEquals(expected))
            Fail("T074_ILLEGAL_TUBE_UNIT", "Aero Tube eligibility must resolve exactly Worker Robot, Double Hover and Jet Scooter.");
    }

    private static void ValidateProductionExits(PrototypeContentCatalog catalog)
    {
        Dictionary<ContentId, FootprintClass> largestByProducer = new();
        for (int i = 0; i < catalog.Production.Length; i++)
        {
            UnitProductionDefinition production = catalog.Production[i];
            if (!catalog.TryGetEntity(production.UnitType, out PrototypeEntityDefinition unit))
                Fail("T074_MISSING_PRODUCTION", $"Production references missing unit '{production.UnitStableKey}'.");
            for (int producerIndex = 0; producerIndex < production.ProducerTypes.Length; producerIndex++)
            {
                ContentId producer = production.ProducerTypes[producerIndex];
                if (!catalog.TryGetBuilding(producer, out _))
                    Fail("T074_MISSING_PRODUCTION_SOURCE", $"Production for '{production.UnitStableKey}' references missing producer '{production.ProducerStableKeys[producerIndex]}'.");
                if (!largestByProducer.TryGetValue(producer, out FootprintClass largest) || unit.Footprint > largest)
                    largestByProducer[producer] = unit.Footprint;
            }
        }

        foreach (KeyValuePair<ContentId, FootprintClass> entry in largestByProducer)
        {
            catalog.TryGetBuilding(entry.Key, out BuildingDefinition building);
            int requiredDiameter = RequiredExitDiameter(entry.Value);
            if (building.ProductionExitWidth < requiredDiameter || building.ProductionExitDepth < requiredDiameter ||
                building.ProductionExitFootprint < entry.Value)
                Fail("T074_IMPOSSIBLE_BUILDING_EXIT", $"Producer '{building.StableKey}' cannot spawn its largest authored unit footprint '{entry.Value}'.");
        }
    }

    private static int RequiredExitDiameter(FootprintClass footprint)
    {
        int rawDiameter = checked(FootprintRules.CollisionRadiusBuild(footprint).Raw * 2);
        return Math.Max(1, (rawDiameter + Fix32.One.Raw - 1) / Fix32.One.Raw);
    }

    private static void ValidateMap(PrototypeContentCatalog catalog, MapDefinition map)
    {
        HashSet<uint> featureIds = new();
        for (int i = 0; i < map.Grid.Features.Count; i++)
        {
            ExcavatableFeature feature = map.Grid.Features[i];
            if (feature.StableId.Value == 0 || StableId.FromKey(feature.StableKey) != feature.StableId ||
                !featureIds.Add(feature.StableId.Value))
                Fail("T074_DUPLICATE_ID", $"Map feature '{feature.StableKey}' has an invalid or duplicate stable ID.");
            ContentId expectedVisual = StableId.FromKey("view.placeholder.excavatable.fractured_rock_wall");
            if (feature.VisualProfile.Value == 0 || feature.VisualProfile != expectedVisual)
                Fail("T074_MISSING_VISUAL_PROFILE", $"Map feature '{feature.StableKey}' has no resolved visual profile.");
        }

        if (map.Starts.Length != 2)
            Fail("T074_BAD_MAP_START", $"Expected two player starts, found {map.Starts.Length}.");
        HashSet<byte> slots = new();
        HierarchicalPathfinder pathfinder = new(map.Grid);
        for (int i = 0; i < map.Starts.Length; i++)
        {
            MapStart start = map.Starts[i];
            if (!slots.Add(start.PlayerSlot) || start.PlayerSlot >= map.Starts.Length || !InMap(start.Position) ||
                !pathfinder.IsPassable(MapGrid.BuildToNav(start.Position), FootprintClass.Huge))
                Fail("T074_BAD_MAP_START", $"Player start {start.PlayerSlot} is duplicate, outside the map or not passable for the largest footprint.");
        }

        for (int i = 0; i < map.InitialEntities.Length; i++)
        {
            InitialEntitySpawn spawn = map.InitialEntities[i];
            PrototypeEntityDefinition entity = default;
            PrototypeMovementProfile movement = default;
            if (!slots.Contains(spawn.PlayerSlot) || !catalog.TryGetEntity(spawn.ContentKey, out entity) ||
                !catalog.TryGetMovement(entity.MovementProfileKey, out movement))
                Fail("T074_BAD_MAP_REFERENCE", $"Map entity spawn '{spawn.ContentKey}' has an unresolved player, entity or movement reference.");
            if (spawn.Footprint != entity.Footprint || spawn.Layer != movement.Layer || spawn.SelectableKind != entity.SelectableKind ||
                spawn.VisionRadius != entity.VisionRadius || !InMap(spawn.Position) ||
                (spawn.Layer != MovementLayer.TrueAir && !pathfinder.IsPassable(MapGrid.BuildToNav(spawn.Position), spawn.Footprint)))
                Fail("T074_BAD_MAP_REFERENCE", $"Map entity spawn '{spawn.ContentKey}' disagrees with the roster or map topology.");
        }
        for (int i = 0; i < map.InitialResourceNodes.Length; i++)
            if (!catalog.TryGetResourceNode(map.InitialResourceNodes[i].ContentKey, out _) || !InMap(map.InitialResourceNodes[i].Position))
                Fail("T074_BAD_MAP_REFERENCE", $"Map resource spawn '{map.InitialResourceNodes[i].ContentKey}' is unresolved or outside the map.");
        for (int i = 0; i < map.InitialResourceReceivers.Length; i++)
        {
            InitialResourceReceiverSpawn receiver = map.InitialResourceReceivers[i];
            if (!slots.Contains(receiver.PlayerSlot) || !catalog.TryGetEntity(receiver.ContentKey, out PrototypeEntityDefinition entity) ||
                entity.SelectableKind != SelectableKind.Building || !catalog.TryGetBuilding(entity.Id, out _) || !InMap(receiver.Position))
                Fail("T074_BAD_MAP_REFERENCE", $"Map receiver '{receiver.ContentKey}' has an unresolved player, building or position.");
        }
    }

    private static bool InMap(FixVec2 position)
        => position.X >= Fix32.Zero && position.Y >= Fix32.Zero &&
           position.X < Fix32.FromInt(MapGrid.BuildWidth) && position.Y < Fix32.FromInt(MapGrid.BuildHeight);

    private static Dictionary<string, int> EmptyFactionCounts()
    {
        Dictionary<string, int> result = new(StringComparer.Ordinal);
        foreach (string faction in Factions) result.Add(faction, 0);
        return result;
    }

    private static void ValidateCounts(string kind, Dictionary<string, int> actual, Dictionary<string, int> expected, int expectedTotal)
    {
        int total = 0;
        foreach (KeyValuePair<string, int> entry in expected)
        {
            total += actual[entry.Key];
            if (actual[entry.Key] != entry.Value)
                Fail("T074_ROSTER_COUNT", $"Expected {entry.Value} {entry.Key} {kind} definitions, found {actual[entry.Key]}.");
        }
        if (total != expectedTotal) Fail("T074_ROSTER_COUNT", $"Expected {expectedTotal} {kind} definitions, found {total}.");
    }

    private static void RequireEntityVisual(string owner, string profile)
    {
        string expected = owner switch
        {
            string key when key.StartsWith("unit.", StringComparison.Ordinal) => $"view.placeholder.{key[5..]}",
            string key when key.StartsWith("building.", StringComparison.Ordinal) => $"view.placeholder.{key[9..]}",
            "prototype.nav.huge" => "view.placeholder.navigation.huge",
            _ => string.Empty
        };
        RequireVisual(owner, profile, expected);
    }

    private static void RequireVisual(string owner, string profile, string expected)
    {
        if (string.IsNullOrWhiteSpace(expected) || !string.Equals(profile, expected, StringComparison.Ordinal))
            Fail("T074_MISSING_VISUAL_PROFILE", $"Content '{owner}' has no resolved visual profile.");
    }

    private static void Fail(string diagnostic, string message) => throw new InvalidDataException($"{diagnostic}: {message}");
}
}
