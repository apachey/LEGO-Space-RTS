using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class DebugHud : CanvasLayer
{
    private GodotSimBridge? _bridge;
    private PanelContainer? _panel;
    private Label? _label;
    private double _nextUpdate;

    public void Configure(GodotSimBridge bridge, RtsInputController input, DebugRenderer debug, FogPresenter fog)
    {
        _bridge = bridge; Name = "DeveloperHUD"; Layer = 20; ProcessPriority = 210;
        _panel = new PanelContainer { Name = "DeveloperPanel", Position = new Vector2(12, 90), CustomMinimumSize = new Vector2(680, 0), Visible = false };
        VBoxContainer box = new(); _panel.AddChild(box);
        HBoxContainer header = new(); box.AddChild(header);
        Label title = new() { Text = "DEVELOPER TOOLS — F8", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; title.AddThemeFontSizeOverride("font_size", 16); header.AddChild(title);
        Button drain = new() { Text = "Drain Energy" }; drain.Pressed += input.DebugDrainEnergy; header.AddChild(drain);
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
        AddToggle(toggles, "Fog", () => fog.FogVisible, fog.SetFogVisible);
        AddChild(_panel);
    }

    public override void _Process(double delta)
    {
        if (_panel is null || _bridge is null || _label is null) return;
        if (Input.IsActionJustPressed("debug_hud_toggle")) _panel.Visible = !_panel.Visible;
        if (!_panel.Visible) return;
        double now = Time.GetTicksMsec() / 1000.0;
        if (now < _nextUpdate) return;
        _nextUpdate = now + 0.25;
        _label.Text = $"Tick {_bridge.World.Tick.Value}   Hash {_bridge.StateHashHex()}   Content {_bridge.GameplayContentHash:X16}\nSim {_bridge.LastSimulationMs:F3} ms   Path {_bridge.LastPathfindingMs:F3} ms   Entities {_bridge.World.Entities.Alive.Count}";
    }

    private static void AddToggle(Container parent, string name, Func<bool> getter, Action<bool> setter)
    {
        Button button = new() { Text = name, ToggleMode = true, ButtonPressed = getter(), CustomMinimumSize = new Vector2(0, 30) };
        button.Pressed += () => setter(button.ButtonPressed); parent.AddChild(button);
    }
}
