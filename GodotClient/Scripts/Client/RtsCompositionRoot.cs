using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Client;

public partial class RtsCompositionRoot : Node3D
{
    private static bool _forceM7MaterialLab;

    public override void _Ready()
    {
        Engine.MaxFps = 60;
        string[] commandLineArgs = OS.GetCmdlineUserArgs();
        if (_forceM7MaterialLab || commandLineArgs.Contains("--m7-material-lab"))
        {
            M7MaterialLab lab = new();
            AddChild(lab);
            lab.Configure(ReturnFromM7MaterialLab, commandLineArgs);
            return;
        }

        InputBindings.ConfigureDefaults();
        LoadedScenario scenario = RuntimeScenarioLoader.LoadActive();
        SimulationWorld world = scenario.World;

        GodotSimBridge bridge = new() { Name = "SimulationBridge" }; AddChild(bridge); bridge.Configure(world, scenario.GameplayContentHash);
        RtsCameraController camera = new() { Name = "RTSCamera" }; AddChild(camera);
        camera.CenterOn(ComputeInitialPlayerFocus(world, 0));
        SelectionController selection = new() { Name = "Selection" }; AddChild(selection); selection.Configure(bridge, camera);
        RtsInputController input = new() { Name = "InputAndCommands" }; AddChild(input); input.Configure(bridge, selection, camera);
        MapPresenter map = new() { Name = "MapPresentation" }; AddChild(map); map.Configure(world);
        UnitViewManager views = new() { Name = "UnitViews" }; AddChild(views); views.Configure(bridge, selection, input.Groups);
        FogPresenter fog = new() { Name = "FogPresentation" }; AddChild(fog); fog.Configure(bridge);
        DebugRenderer debug = new() { Name = "DebugVisualization" }; AddChild(debug); debug.Configure(bridge);
        BasicHud hud = new(); AddChild(hud); hud.Configure(bridge, selection, input);
        DebugHud developerHud = new(); AddChild(developerHud); developerHud.Configure(bridge, input, debug, fog, PrepareM5Acceptance, OpenM7MaterialLab);
        if (scenario.IsM5Acceptance)
        {
            bool pauseInitially = !commandLineArgs.Contains("--smoke") && !commandLineArgs.Contains("--capture-smoke");
            M5PlaytestHud playtestHud = new(); AddChild(playtestHud); playtestHud.Configure(bridge, selection, camera, RestartM5Acceptance, pauseInitially);
        }
        GD.Print($"Prototype content source: {(scenario.LoadedFromCompiledData ? "compiled runtime data" : "built-in deterministic fallback")}, content hash={scenario.GameplayContentHash:X16}");

        M7LightingRig.AddNeutralGameplayRig(this);

        if (commandLineArgs.Contains("--smoke") || commandLineArgs.Contains("--capture-smoke"))
        {
            GodotSmokeRunner smoke = new() { Name = "GodotSmokeRunner" };
            AddChild(smoke);
            smoke.Configure(bridge, selection, camera, input, views, commandLineArgs);
        }
    }

    private void PrepareM5Acceptance()
    {
        RuntimeScenarioLoader.ForceM5Acceptance = true;
        Callable.From(() => GetTree().ReloadCurrentScene()).CallDeferred();
    }

    private void RestartM5Acceptance()
    {
        RuntimeScenarioLoader.ForceM5Acceptance = true;
        Callable.From(() => GetTree().ReloadCurrentScene()).CallDeferred();
    }

    private void OpenM7MaterialLab()
    {
        _forceM7MaterialLab = true;
        Callable.From(() => GetTree().ReloadCurrentScene()).CallDeferred();
    }

    private void ReturnFromM7MaterialLab()
    {
        _forceM7MaterialLab = false;
        if (OS.GetCmdlineUserArgs().Contains("--m7-material-lab"))
        {
            GetTree().Quit();
            return;
        }
        Callable.From(() => GetTree().ReloadCurrentScene()).CallDeferred();
    }

    private static Vector3 ComputeInitialPlayerFocus(SimulationWorld world, byte playerSlot)
    {
        Vector3 sum = Vector3.Zero;
        int count = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot) continue;
            if (!world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            sum += transform.Position.ToWorld();
            count++;
        }
        if (count > 0) return sum / count;
        float center = MapGrid.BuildWidth * GodotConversions.WorldUnitsPerBuildCell * 0.5f;
        return new Vector3(center, 0f, center);
    }

}
