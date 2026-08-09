using System;

namespace LegoSpaceRTS.SimCore
{
public static class DevMapFactory
{
    public const string StableKey = "map.dev_first_controllable_rts";
    public const ushort ExcavatableFeatureId = 1;

    public static MapGrid Create() => CreateDefinition().Grid;

    public static MapDefinition CreateDefinition()
    {
        MapGrid map = new(StableKey);

        // Hard border.
        map.SetFlagsRect(new IntRect(0, 0, 320, 2), MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
        map.SetFlagsRect(new IntRect(0, 318, 320, 2), MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
        map.SetFlagsRect(new IntRect(0, 0, 2, 320), MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
        map.SetFlagsRect(new IntRect(318, 0, 2, 320), MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);

        // Central cliff spine. Openings are authored by leaving gaps:
        // broad 22-build-cell lane, medium 13-cell passage, heavy 18-cell route,
        // plus the debug Excavatable shortcut.
        AddCliff(map, 152, 8, 16, 28);
        AddCliff(map, 152, 80, 16, 44);
        AddCliff(map, 152, 150, 16, 46);
        AddCliff(map, 152, 232, 16, 32);
        AddCliff(map, 152, 278, 16, 34);

        AddCliff(map, 46, 44, 36, 18);
        AddCliff(map, 238, 54, 34, 22);
        AddCliff(map, 54, 222, 44, 20);
        AddCliff(map, 224, 214, 42, 24);

        map.SetFlagsRect(new IntRect(36, 136, 58, 40), MapCellFlags.Rough);
        map.SetFlagsRect(new IntRect(226, 126, 46, 44), MapCellFlags.Rough);

        map.SetElevationRect(new IntRect(26, 26, 62, 52), 1);
        map.SetElevationRect(new IntRect(230, 232, 58, 48), 2);
        map.SetFlagsRect(new IntRect(90, 44, 8, 46), MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
        map.SetFlagsRect(new IntRect(212, 238, 8, 42), MapCellFlags.GroundOccluder, MapCellFlags.Buildable);

        map.AddExcavatable(new ExcavatableFeature(ExcavatableFeatureId, new IntRect(152, 264, 16, 14), false));

        MapStart[] starts =
        {
            new MapStart(0, FixVec2.FromInts(24, 74)),
            new MapStart(1, FixVec2.FromInts(132, 74))
        };
        InitialEntitySpawn[] spawns = BuildPrototypeSpawns();
        VisionTestRegion[] vision =
        {
            new VisionTestRegion("left_elevation_shelf", new IntRect(26, 26, 62, 52)),
            new VisionTestRegion("left_occluder_wall", new IntRect(90, 44, 8, 46)),
            new VisionTestRegion("right_elevation_shelf", new IntRect(230, 232, 58, 48)),
            new VisionTestRegion("right_occluder_wall", new IntRect(212, 238, 8, 42))
        };
        return new MapDefinition(map, starts, spawns, vision);
    }

    private static InitialEntitySpawn[] BuildPrototypeSpawns()
    {
        InitialEntitySpawn[] result = new InitialEntitySpawn[26];
        int at = 0;
        at = FillSpawns(result, at, 0, 18, 24, 74, 6);
        FillSpawns(result, at, 1, 8, 132, 74, 4);
        return result;
    }

    private static int FillSpawns(InitialEntitySpawn[] output, int at, byte player, int count, int originX, int originY, int columns)
    {
        for (int i = 0; i < count; i++)
        {
            int profile = i & 3;
            string contentKey; FootprintClass fp; MovementLayer layer; SelectableKind kind; byte vision;
            switch (profile)
            {
                case 0: contentKey = "unit.rock_raiders.crew"; fp = FootprintClass.Tiny; layer = MovementLayer.Ground; kind = SelectableKind.Worker; vision = 7; break;
                case 1: contentKey = "unit.rock_raiders.hover_scout"; fp = FootprintClass.Small; layer = MovementLayer.GroundHover; kind = SelectableKind.CombatSupport; vision = 9; break;
                case 2: contentKey = "unit.rock_raiders.loader_dozer"; fp = FootprintClass.Medium; layer = MovementLayer.Ground; kind = SelectableKind.CombatSupport; vision = 7; break;
                default: contentKey = "unit.rock_raiders.chrome_crusher"; fp = FootprintClass.Large; layer = MovementLayer.Ground; kind = SelectableKind.CombatSupport; vision = 8; break;
            }
            FixVec2 position = FixVec2.FromInts(originX + (i % columns) * 2, originY + (i / columns) * 2);
            output[at++] = new InitialEntitySpawn(player, contentKey, position, fp, layer, kind, vision);
        }
        return at;
    }

    private static void AddCliff(MapGrid map, short x, short y, short width, short height)
        => map.SetFlagsRect(new IntRect(x, y, width, height), MapCellFlags.Impassable | MapCellFlags.GroundOccluder, MapCellFlags.Buildable);
}
}
