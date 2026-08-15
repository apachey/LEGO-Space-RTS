using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class DebugHud : CanvasLayer
{
    private GodotSimBridge? _bridge;
    private RtsInputController? _input;
    private PanelContainer? _panel;
    private Label? _label;
    private double _nextUpdate;

    public void Configure(GodotSimBridge bridge, RtsInputController input, DebugRenderer debug, FogPresenter fog, Action prepareM5Acceptance)
    {
        _bridge = bridge; _input = input; Name = "DeveloperHUD"; Layer = 20; ProcessPriority = 210;
        _panel = new PanelContainer { Name = "DeveloperPanel", Position = new Vector2(12, 90), CustomMinimumSize = new Vector2(760, 0), Visible = false };
        VBoxContainer box = new(); _panel.AddChild(box);
        HBoxContainer header = new(); box.AddChild(header);
        Label title = new() { Text = "DEVELOPER TOOLS — F8", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; title.AddThemeFontSizeOverride("font_size", 16); header.AddChild(title);
        Button drain = new() { Text = "Drain Energy" }; drain.Pressed += input.DebugDrainEnergy; header.AddChild(drain);
        Button m5 = new() { Name = "PrepareM5Acceptance", Text = "Prepare M5 Playtest" }; m5.Pressed += prepareM5Acceptance; header.AddChild(m5);
        HFlowContainer actions = new() { Name = "PreparedPlaytestActions" }; box.AddChild(actions);
        Button prepareConstruction = new() { Name = "PrepareConstructionTest", Text = "Prepare construction test" };
        prepareConstruction.TooltipText = "Supplies resources, creates a progressing construction site, selects it and centers the camera.";
        prepareConstruction.Pressed += input.DebugPrepareConstructionPlaytest; actions.AddChild(prepareConstruction);
        Button prepareDestruction = new() { Name = "PrepareDestructionTest", Text = "Prepare T045 arena" };
        prepareDestruction.TooltipText = "Creates and frames your Chrome Crusher, damaged enemy Crew/Chrome and a destructible building.";
        prepareDestruction.Pressed += input.DebugPrepareDestructionPlaytest; actions.AddChild(prepareDestruction);
        Button prepareRepair = new() { Name = "PrepareRepairTest", Text = "Prepare T046 repair" };
        prepareRepair.TooltipText = "Supplies resources, damages a Hover Scout, selects nearby Crew and centers the camera.";
        prepareRepair.Pressed += input.DebugPrepareRepairPlaytest; actions.AddChild(prepareRepair);
        Button prepareTransport = new() { Name = "PrepareTransportTest", Text = "Prepare T047 transport" };
        prepareTransport.TooltipText = "Creates a Rapid Rider and four nearby Crew, selects the Crew and centers the camera.";
        prepareTransport.Pressed += input.DebugPrepareTransportPlaytest; actions.AddChild(prepareTransport);
        Button destroyTransport = new() { Name = "DestroyTransportTest", Text = "Kill loaded Rider" };
        destroyTransport.TooltipText = "Destroys the prepared Rapid Rider so loaded Crew emergency-deploy at 40% HP.";
        destroyTransport.Pressed += input.DebugDestroyPreparedTransport; actions.AddChild(destroyTransport);
        Button prepareTransformation = new() { Name = "PrepareTransformationTest", Text = "Prepare T048 MX-41" };
        prepareTransformation.TooltipText = "Creates, selects and frames an MX-41 in Ground mode for Q/Stop transformation tests.";
        prepareTransformation.Pressed += input.DebugPrepareTransformationPlaytest; actions.AddChild(prepareTransformation);
        Button destroyCrew = new() { Name = "DestroyCrewTest", Text = "Kill test Crew" };
        destroyCrew.Pressed += input.DebugDestroyPreparedCrew; actions.AddChild(destroyCrew);
        Button destroyChrome = new() { Name = "DestroyChromeTest", Text = "Kill test Chrome" };
        destroyChrome.Pressed += input.DebugDestroyPreparedChrome; actions.AddChild(destroyChrome);
        Button destroyBuilding = new() { Name = "DestroyBuildingTest", Text = "Kill test building" };
        destroyBuilding.Pressed += input.DebugDestroyPreparedBuilding; actions.AddChild(destroyBuilding);
        Button moveEnemies = new() { Name = "MoveEnemyTest", Text = "Move visible enemies" };
        moveEnemies.TooltipText = "Issues a normal deterministic Move command to visible enemy units for moving-target combat tests.";
        moveEnemies.Pressed += input.DebugMoveVisibleEnemies; actions.AddChild(moveEnemies);
        _label = new Label(); _label.AddThemeFontSizeOverride("font_size", 14); box.AddChild(_label);
        HFlowContainer toggles = new(); box.AddChild(toggles);
        AddToggle(toggles, "Nav", () => debug.DrawNavigation, v => debug.DrawNavigation = v);
        AddToggle(toggles, "HPA", () => debug.DrawClusters, v => debug.DrawClusters = v);
        AddToggle(toggles, "Portals", () => debug.DrawPortals, v => debug.DrawPortals = v);
        AddToggle(toggles, "Paths", () => debug.DrawPaths, v => debug.DrawPaths = v);
        AddToggle(toggles, "Separation", () => debug.DrawLocalSeparation, v => debug.DrawLocalSeparation = v);
        AddToggle(toggles, "Buckets", () => debug.DrawSpatialBuckets, v => debug.DrawSpatialBuckets = v);
        AddToggle(toggles, "Vision", () => debug.DrawVision, v => debug.DrawVision = v);
        AddToggle(toggles, "Excavatable", () => debug.DrawExcavatable, v => debug.DrawExcavatable = v);
        AddToggle(toggles, "Forward Service", () => debug.DrawForwardService, v => debug.DrawForwardService = v);
        AddToggle(toggles, "Fog", () => fog.FogVisible, fog.SetFogVisible);
        AddChild(_panel);
    }

    public override void _Process(double delta)
    {
        if (_panel is null || _bridge is null || _input is null || _label is null) return;
        if (Input.IsActionJustPressed("debug_hud_toggle")) _panel.Visible = !_panel.Visible;
        if (!_panel.Visible) return;
        double now = Time.GetTicksMsec() / 1000.0;
        if (now < _nextUpdate) return;
        _nextUpdate = now + 0.25;
        _label.Text = $"{_input.DebugTestStatus}\nTick {_bridge.World.Tick.Value}   Hash {_bridge.StateHashHex()}   Content {_bridge.GameplayContentHash:X16}\nSim {_bridge.LastSimulationMs:F3} ms   Path {_bridge.LastPathfindingMs:F3} ms   Entities {_bridge.World.Entities.Alive.Count}";
    }

    private static void AddToggle(Container parent, string name, Func<bool> getter, Action<bool> setter)
    {
        Button button = new() { Text = name, ToggleMode = true, ButtonPressed = getter(), CustomMinimumSize = new Vector2(0, 30) };
        button.Pressed += () => setter(button.ButtonPressed); parent.AddChild(button);
    }
}
