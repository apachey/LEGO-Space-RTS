using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Client;

public partial class RtsCompositionRoot : Node3D
{
    public override void _Ready()
    {
        Engine.MaxFps = 60;
        InputBindings.ConfigureDefaults();
        LoadedScenario scenario = RuntimeScenarioLoader.LoadCanonicalOpening();
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
        DebugHud hud = new() { Name = "PrototypeHUD" }; AddChild(hud); hud.Configure(bridge, selection, input, debug, fog);
        GD.Print($"Prototype content source: {(scenario.LoadedFromCompiledData ? "compiled runtime data" : "built-in deterministic fallback")}, content hash={scenario.GameplayContentHash:X16}");

        DirectionalLight3D sun = new() { Name = "Sun", RotationDegrees = new Vector3(-58f, -35f, 0f), LightEnergy = 1.2f, ShadowEnabled = true }; AddChild(sun);
        WorldEnvironment environment = new() { Name = "WorldEnvironment", Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color(0.035f, 0.04f, 0.05f), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new Color(0.64f, 0.64f, 0.68f), AmbientLightEnergy = 0.72f } }; AddChild(environment);

        string[] commandLineArgs = OS.GetCmdlineUserArgs();
        if (commandLineArgs.Contains("--smoke") || commandLineArgs.Contains("--capture-smoke"))
        {
            GodotSmokeRunner smoke = new() { Name = "GodotSmokeRunner" };
            AddChild(smoke);
            smoke.Configure(bridge, commandLineArgs);
        }
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
