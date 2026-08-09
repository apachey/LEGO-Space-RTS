using Godot;
using GodotFileAccess = Godot.FileAccess;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Client;

public static class RuntimeScenarioLoader
{
    public const string ContentPath = "res://Compiled/PrototypeEntities.contentbin";
    public const string MapPath = "res://Compiled/DEV_FirstControllableRTS.mapbin";

    public static LoadedScenario LoadFirstControllable(int count = 18)
    {
        if (!GodotFileAccess.FileExists(ContentPath) || !GodotFileAccess.FileExists(MapPath))
        {
            GD.PushWarning("Compiled M2 content is absent. Falling back to deterministic built-in DEV factories. Run tools/ContentCompiler before release/export validation.");
            PrototypeContentCatalog fallbackCatalog = PrototypeContentFactory.CreateM2Catalog();
            MapDefinition fallbackMap = DevMapFactory.CreateDefinition();
            return new LoadedScenario(ScenarioFactory.CreateFirstControllable(fallbackMap, fallbackCatalog, count), fallbackCatalog.ContentHash, false);
        }

        byte[] contentBytes = ReadAll(ContentPath);
        byte[] mapBytes = ReadAll(MapPath);
        PrototypeContentCatalog catalog = PrototypeContentCodec.Read(contentBytes);
        MapDefinition definition = CompiledMapCodec.ReadDefinition(mapBytes);
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(definition, catalog, count);
        return new LoadedScenario(world, catalog.ContentHash, true);
    }

    private static byte[] ReadAll(string path)
    {
        using GodotFileAccess file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
        if (file == null) throw new InvalidOperationException($"Could not open {path}.");
        long length = checked((long)file.GetLength());
        return file.GetBuffer(length);
    }
}

public readonly struct LoadedScenario
{
    public LoadedScenario(SimulationWorld world, ulong gameplayContentHash, bool loadedFromCompiledData)
    {
        World = world;
        GameplayContentHash = gameplayContentHash;
        LoadedFromCompiledData = loadedFromCompiledData;
    }

    public SimulationWorld World { get; }
    public ulong GameplayContentHash { get; }
    public bool LoadedFromCompiledData { get; }
}
