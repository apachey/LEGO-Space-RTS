using System.Text;
using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class M5PlaytestHud : CanvasLayer
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsCameraController? _camera;
    private Label? _status;
    private double _nextUpdate;
    private readonly StringBuilder _text = new(512);

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera, Action restart)
    {
        _bridge = bridge; _selection = selection; _camera = camera;
        Name = "M5PlaytestHUD"; Layer = 18; ProcessPriority = 205;

        PanelContainer panel = new()
        {
            Name = "M5AcceptancePanel",
            AnchorLeft = 0.68f, AnchorRight = 0.99f, AnchorTop = 0.08f, AnchorBottom = 0.65f,
            OffsetLeft = 0, OffsetRight = 0, OffsetTop = 0, OffsetBottom = 0
        };
        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.045f, 0.065f, 0.085f, 0.96f), BorderColor = new Color("e6ad28"),
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 7, CornerRadiusTopRight = 7, CornerRadiusBottomLeft = 7, CornerRadiusBottomRight = 7,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 10, ContentMarginBottom = 10
        };
        panel.AddThemeStyleboxOverride("panel", style);
        VBoxContainer box = new(); box.AddThemeConstantOverride("separation", 7); panel.AddChild(box);
        Label title = Label("M5 PLAYTEST — FOUR-FACTION PROOF", 18, new Color("e6ad28")); box.AddChild(title);
        Label instructions = Label("All proof states are prepared from a fresh deterministic launch. Use FOCUS, inspect the world and the selection panel, then RESTART M5 to replay.", 13, new Color("aab4b8"));
        instructions.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(instructions);
        _status = Label(string.Empty, 14, new Color("f2eee3")); _status.Name = "M5AcceptanceStatus"; _status.AutowrapMode = TextServer.AutowrapMode.WordSmart; _status.SizeFlagsVertical = Control.SizeFlags.ExpandFill; box.AddChild(_status);

        GridContainer focuses = new() { Columns = 2 }; focuses.AddThemeConstantOverride("h_separation", 6); focuses.AddThemeConstantOverride("v_separation", 6); box.AddChild(focuses);
        AddFocus(focuses, "WORKSITE", "building.rock_raiders.hq");
        AddFocus(focuses, "REFIT", M5AcceptanceScenarioFactory.T3TrikeKey);
        AddFocus(focuses, "CHARGE / SURGE", M5AcceptanceScenarioFactory.ResonanceCoreKey);
        AddFocus(focuses, "AERO TUBE", M5AcceptanceScenarioFactory.AeroTubeHangarKey);
        AddFocus(focuses, "STABILITY", M5AcceptanceScenarioFactory.DisplacementTargetKey);
        Button excavation = Button("EXCAVATION"); excavation.Pressed += FocusExcavation; focuses.AddChild(excavation);
        Button reset = Button("RESTART M5"); reset.Name = "RestartM5Acceptance"; reset.Pressed += restart; box.AddChild(reset);
        AddChild(panel);
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _status is null) return;
        double now = Time.GetTicksMsec() / 1000.0;
        if (now < _nextUpdate) return;
        _nextUpdate = now + 0.2;
        SimulationWorld world = _bridge.World;
        _text.Clear();
        EntityId[] worksites = WorksiteGraphSystem.GetPlayerComponents(world, 0);
        _text.Append("ROCK RAIDERS  •  Worksite ").Append(worksites.Length > 0 ? "CONNECTED" : "MISSING").Append('\n');

        if (M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.T3TrikeKey, out EntityId t3))
        {
            if (world.Entities.MissionRefitJob.TryGet(t3, out MissionRefitJob job))
                _text.Append("ASTRONAUTS  •  Survey Refit ").Append((job.TotalTicks - job.RemainingTicks) * 100 / job.TotalTicks).Append("%\n");
            else if (world.Entities.MissionRefitState.TryGet(t3, out MissionRefitState refit))
                _text.Append("ASTRONAUTS  •  ").Append(refit.CurrentConfiguration == MissionConfiguration.T3Survey ? "Survey installed" : "Refit window complete").Append('\n');
        }

        AlienChargeState charge = world.GetAlienCharge(0);
        _text.Append("ALIENS  •  Charge ").Append(Charge(charge.CurrentMillicharge)).Append(" / ").Append(Charge(charge.MaximumMillicharge));
        if (M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.ResonanceCoreKey, out EntityId core) && world.Entities.SurgeZone.TryGet(core, out SurgeZone zone))
            _text.Append(zone.BuildupRemainingTicks > 0 ? "  •  Surge buildup" : $"  •  Surge {zone.ActiveRemainingTicks / 20.0f:F1}s");
        else _text.Append("  •  Surge complete");
        _text.Append('\n');

        int approaching = 0, loading = 0, travelling = 0, arriving = 0;
        foreach (EntityId id in world.Entities.Alive)
        {
            if (!world.Entities.TubeTransfer.TryGet(id, out TubeTransfer transfer)) continue;
            if (transfer.State == TubeTransferState.Approaching || transfer.State == TubeTransferState.Queued) approaching++;
            else if (transfer.State == TubeTransferState.Loading) loading++;
            else if (transfer.State == TubeTransferState.Travelling) travelling++;
            else arriving++;
        }
        _text.Append("MARTIANS  •  Tube: ").Append(approaching).Append(" approach/queue, ").Append(loading).Append(" loading, ").Append(travelling).Append(" travelling, ").Append(arriving).Append(" arrival\n");

        int stability = 0;
        if (M5AcceptanceScenarioFactory.TryFindFirst(world, M5AcceptanceScenarioFactory.DisplacementTargetKey, out EntityId target))
            stability = DisplacementSystem.RemainingStabilityTicks(world, target);
        _text.Append("CONTROL  •  Stability ").Append((stability + 19) / 20).Append("s after repeated hostile Clamp\n");
        bool open = world.Map.TryGetFeature(M5AcceptanceScenarioFactory.ExcavatableFeatureId, out ExcavatableFeature feature) && feature.Open;
        _text.Append("EXCAVATION  •  Route ").Append(open ? "OPEN" : "CLOSED");
        _status.Text = _text.ToString();
    }

    private void AddFocus(Container parent, string text, string stableKey)
    {
        Button button = Button(text); button.Pressed += () => Focus(stableKey); parent.AddChild(button);
    }

    private void Focus(string stableKey)
    {
        if (_bridge is null || !M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, stableKey, out EntityId entity)) return;
        _selection?.SetSelection(new[] { entity });
        if (_bridge.World.Entities.Transform.TryGet(entity, out SimTransform transform)) _camera?.CenterOn(transform.Position.ToWorld());
    }

    private void FocusExcavation()
    {
        if (_bridge is null || !_bridge.World.Map.TryGetFeature(M5AcceptanceScenarioFactory.ExcavatableFeatureId, out ExcavatableFeature feature)) return;
        _selection?.SetSelection(Array.Empty<EntityId>());
        FixVec2 center = new(
            Fix32.FromRatio(feature.NavRect.X * 2 + feature.NavRect.Width, MapGrid.NavPerBuild * 2),
            Fix32.FromRatio(feature.NavRect.Y * 2 + feature.NavRect.Height, MapGrid.NavPerBuild * 2));
        _camera?.CenterOn(center.ToWorld());
    }

    private static Button Button(string text)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(0, 34) }; button.AddThemeFontSizeOverride("font_size", 13); return button;
    }

    private static Label Label(string text, int size, Color color)
    {
        Label label = new() { Text = text }; label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", color); return label;
    }

    private static string Charge(int millicharge) => $"{millicharge / 1000}.{(millicharge % 1000) / 100}";
}
