using Godot;
using GodotFileAccess = Godot.FileAccess;
using LegoSpaceRTS.SimCore;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Client;

public static class RuntimeScenarioLoader
{
    public const string ContentPath = "res://Compiled/PrototypeEntities.contentbin";
    public const string MapPath = "res://Compiled/DEV_FirstControllableRTS.mapbin";
    public static bool ForceM5Acceptance { get; set; }

    public static LoadedScenario LoadActive()
    {
        bool requested = ForceM5Acceptance || OS.GetCmdlineUserArgs().Contains("--m5-playtest");
        return requested ? LoadM5Acceptance() : LoadCanonicalOpening();
    }

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

    public static LoadedScenario LoadCanonicalOpening()
    {
        if (!GodotFileAccess.FileExists(ContentPath) || !GodotFileAccess.FileExists(MapPath))
        {
            GD.PushWarning("Compiled M3 content is absent. Falling back to the deterministic built-in canonical opening.");
            PrototypeContentCatalog fallbackCatalog = PrototypeContentFactory.CreateM2Catalog();
            MapDefinition fallbackMap = DevMapFactory.CreateDefinition();
            return new LoadedScenario(ScenarioFactory.CreateCanonicalOpening(fallbackMap, fallbackCatalog), fallbackCatalog.ContentHash, false, false);
        }

        byte[] contentBytes = ReadAll(ContentPath);
        byte[] mapBytes = ReadAll(MapPath);
        PrototypeContentCatalog catalog = PrototypeContentCodec.Read(contentBytes);
        MapDefinition definition = CompiledMapCodec.ReadDefinition(mapBytes);
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening(definition, catalog);
        return new LoadedScenario(world, catalog.ContentHash, true, false);
    }

    public static LoadedScenario LoadM5Acceptance()
    {
        PrototypeContentCatalog catalog;
        bool compiled = GodotFileAccess.FileExists(ContentPath);
        if (compiled) catalog = PrototypeContentCodec.Read(ReadAll(ContentPath));
        else
        {
            GD.PushWarning("Compiled content is absent. M5 acceptance is using the deterministic built-in prototype catalog.");
            catalog = PrototypeContentFactory.CreateM2Catalog();
        }
        int requestedStep = M5PlaytestHud.RequestedStep;
        return new LoadedScenario(M5AcceptanceScenarioFactory.Create(catalog,
            applyInitialDisplacement: requestedStep != 5,
            openExcavation: requestedStep != 6), catalog.ContentHash, compiled, true);
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
    public LoadedScenario(SimulationWorld world, ulong gameplayContentHash, bool loadedFromCompiledData, bool isM5Acceptance = false)
    {
        World = world;
        GameplayContentHash = gameplayContentHash;
        LoadedFromCompiledData = loadedFromCompiledData;
        IsM5Acceptance = isM5Acceptance;
    }

    public SimulationWorld World { get; }
    public ulong GameplayContentHash { get; }
    public bool LoadedFromCompiledData { get; }
    public bool IsM5Acceptance { get; }
}
