using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Client;

public partial class M7HudLab : Node3D
{
    private Action? _returnToPrototype;
    private M7HudProfile _profile = M7HudProfile.CreateDefault();
    private M7HudScenario _scenario = M7HudScenario.MixedArmy;
    private HudFaction? _previewFactionOverride;
    private string _aspect = "16:9";
    private HudView? _hud;
    private Control? _previewFrame;
    private PanelContainer? _controlPanel;
    private Label? _status;
    private bool _controlsVisible = true;
    private bool _smoke;
    private bool _finished;
    private int _smokeExitCode;
    private int _frames;
    private string? _capturePath;
    private Vector2 _labCameraCenter = new(83f, 80f);

    public void Configure(Action returnToPrototype, string[] arguments)
    {
        Name = "M7HudLab";
        _returnToPrototype = returnToPrototype;
        _smoke = arguments.Contains("--m7-hud-smoke");
        if (arguments.Contains("--m7-hud-controls") && Array.IndexOf(arguments, "--m7-hud-controls") + 1 < arguments.Length)
        {
            int controlsIndex = Array.IndexOf(arguments, "--m7-hud-controls");
            _controlsVisible = arguments[controlsIndex + 1] != "hidden";
        }
        for (int i = 0; i + 1 < arguments.Length; i++)
        {
            if (arguments[i] == "--m7-hud-scenario") _scenario = M7HudFixtures.Parse(arguments[i + 1]);
            else if (arguments[i] == "--m7-hud-aspect") _aspect = ParseAspect(arguments[i + 1]);
            else if (arguments[i] == "--m7-hud-finish") _profile.ArtSkin.Finish = ParseFinish(arguments[i + 1]);
            else if (arguments[i] == "--m7-hud-palette") HudSurfacePaletteLibrary.Apply(_profile, ParsePalette(arguments[i + 1]));
            else if (arguments[i] == "--m7-hud-kit") _previewFactionOverride = ParseFactionKit(arguments[i + 1]);
            else if (arguments[i] == "--capture-path") _capturePath = arguments[i + 1];
        }
        if (_previewFactionOverride.HasValue)
        {
            int faction = HudFactionSkinLibrary.IndexFor(_previewFactionOverride.Value);
            if (_profile.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound)
                HudFactionSkinLibrary.Apply(_profile, faction);
            else _profile.ArtSkin.Faction = faction;
        }
        else BindFactionToScenario(_scenario);
        BuildWorldBackdrop();
        BuildHudPreview();
        BuildControls();
        ApplyAll();
        ProcessPriority = 1000;
        GD.Print($"M7 HUD LAB: active={M7HudFixtures.Slug(_scenario)} aspect={_aspect} finish={_profile.ArtSkin.Finish} palette={_profile.ArtSkin.SurfacePalette} controls=Tab 1..8 scenarios");
    }

    public override void _Process(double delta)
    {
        if (!_smoke) return;
        _frames++;
        if (_finished)
        {
            // Validation changes finishes, faction masks and preview sizes in
            // one frame. Let the rendering server consume those queued canvas
            // updates before destroying the headless viewport; otherwise the
            // alpha-mask backbuffer can race Godot's shutdown on macOS.
            if (_frames >= 32) GetTree().Quit(_smokeExitCode);
            return;
        }
        if (_frames < 24) return;
        bool valid = ValidateLab();
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        _smokeExitCode = valid ? 0 : 2;
        if (valid)
            GD.Print($"M7 HUD LAB: PASS scenarios=8 commands=12 minimap=legal markers=11 remembered=2 factionSkins=4 finishes=4 safeArea={_profile.Layout.SafeAreaPercent:0.#} uiScale={_profile.Layout.UiScale:0.00} aspect={_aspect} schema={M7HudProfile.CurrentSchemaVersion} active={M7HudFixtures.Slug(_scenario)} finish={_profile.ArtSkin.Finish} palette={_profile.ArtSkin.SurfacePalette} apertureMasks=4 kitSwitch=interactive kit={HudFactionSkinLibrary.RecipeFor(_profile.ArtSkin.Faction).Name}");
        else GD.PrintErr("M7 HUD LAB: FAIL");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        M7HudScenario? next = key.Keycode switch
        {
            Key.Key1 => M7HudScenario.RockRaiderUnit, Key.Key2 => M7HudScenario.MixedArmy,
            Key.Key3 => M7HudScenario.Production, Key.Key4 => M7HudScenario.Brownout,
            Key.Key5 => M7HudScenario.AstronautTransform, Key.Key6 => M7HudScenario.AlienResonance,
            Key.Key7 => M7HudScenario.MartianNetwork, Key.Key8 => M7HudScenario.CriticalTooltip, _ => null
        };
        if (next.HasValue) { SetScenario(next.Value); Handled(); }
        else if (key.Keycode == Key.Tab) { _controlsVisible = !_controlsVisible; if (_controlPanel is not null) _controlPanel.Visible = _controlsVisible; Handled(); }
        else if (key.Keycode == Key.Escape) { _returnToPrototype?.Invoke(); Handled(); }
    }

    private void BuildWorldBackdrop()
    {
        Camera3D camera = new() { Name = "HudLabCamera", Current = true, Fov = 36f, Position = new Vector3(26f, 35f, 32f) };
        AddChild(camera); camera.LookAt(new Vector3(0f, 0f, 0f));
        DirectionalLight3D key = new() { RotationDegrees = new Vector3(-52f, -35f, 0f), LightEnergy = 1.15f, LightColor = new Color("fff3df"), ShadowEnabled = true };
        AddChild(key);
        DirectionalLight3D fill = new() { RotationDegrees = new Vector3(35f, 145f, 0f), LightEnergy = 0.35f, LightColor = new Color("9bc4e2") };
        AddChild(fill);
        Godot.Environment environment = new()
        {
            BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color("20272b"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new Color("7d8990"), AmbientLightEnergy = 0.55f,
            TonemapMode = Godot.Environment.ToneMapper.Filmic
        };
        AddChild(new WorldEnvironment { Environment = environment });
        MeshInstance3D ground = new()
        {
            Name = "TacticalGround", Mesh = new PlaneMesh { Size = new Vector2(90f, 90f), SubdivideWidth = 20, SubdivideDepth = 20 },
            MaterialOverride = Material(new Color("3c4645"), 0.94f)
        };
        AddChild(ground);
        AddBuilding(new Vector3(-10f, 1.6f, -5f), new Vector3(8f, 3.2f, 7f), new Color("48666a"), "CommandStructure");
        AddBuilding(new Vector3(9f, 1.1f, -9f), new Vector3(5f, 2.2f, 5f), new Color("705842"), "IndustrialStructure");
        Vector3[] positions =
        {
            new(-10f, 0.65f, 6f), new(-5f, 0.65f, 8f), new(0f, 0.65f, 7f), new(5f, 0.65f, 10f),
            new(9f, 0.65f, 4f), new(13f, 0.65f, 8f), new(-13f, 0.65f, 13f), new(1f, 0.65f, 15f)
        };
        for (int i = 0; i < positions.Length; i++) AddUnit(positions[i], i < 5 ? new Color("177f79") : new Color("bb4032"), i);
        AddSelectionRing(positions[0]); AddSelectionRing(positions[1]); AddSelectionRing(positions[2]);
    }

    private void BuildHudPreview()
    {
        CanvasLayer layer = new() { Name = "HudLabPreviewLayer", Layer = 30 };
        AddChild(layer);
        _previewFrame = new Control { Name = "HudViewportPreview", MouseFilter = Control.MouseFilterEnum.Ignore };
        layer.AddChild(_previewFrame);
        _hud = new HudView();
        _previewFrame.AddChild(_hud);
        _hud.Configure(_profile);
        _hud.MinimapCameraRequested += cell =>
        {
            _labCameraCenter = new Vector2(cell.X + 0.5f, cell.Y + 0.5f);
            ApplySyntheticCameraPolygon();
            SetStatus($"Camera centered at {cell.X}, {cell.Y}");
        };
        _hud.MinimapGroundCommandRequested += (cell, queued) =>
        {
            if (_hud is null) return;
            HudFrame frame = _hud.Frame;
            frame.Minimap.Pings.Clear();
            frame.Minimap.Pings.Add(new HudMinimapPingFrame
            {
                StableId = 9001, BuildX = cell.X + 0.5f, BuildY = cell.Y + 0.5f,
                Priority = HudAlertPriority.Normal, Phase = 0f
            });
            _hud.ApplyFrame(frame, true);
            ApplySyntheticCameraPolygon();
            SetStatus($"{(queued ? "Queued move" : "Move")} at {cell.X}, {cell.Y}");
        };
        _hud.AlertRequested += () =>
            SetStatus("Alert focus requested — production centers the associated target.");
    }

    private void BuildControls()
    {
        CanvasLayer layer = new() { Name = "HudLabControlsLayer", Layer = 80 };
        AddChild(layer);
        _controlPanel = new PanelContainer
        {
            Name = "HudLabControls", AnchorLeft = 1f, AnchorRight = 1f, AnchorTop = 0f, AnchorBottom = 1f,
            OffsetLeft = -430f, OffsetRight = -12f, OffsetTop = 12f, OffsetBottom = -12f, MouseFilter = Control.MouseFilterEnum.Stop
        };
        _controlPanel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("10161d"), new Color("61707a")));
        _controlPanel.Visible = _controlsVisible;
        layer.AddChild(_controlPanel);
        ScrollContainer scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _controlPanel.AddChild(scroll);
        VBoxContainer box = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 7); scroll.AddChild(box);
        Label title = LabLabel("M7 HUD LAB · HYBRID RTS CONSOLE", 20, new Color("e6ad28")); box.AddChild(title);
        Label note = LabLabel("Неканонічний hybrid: одна чиста адаптивна конструкція + захищені растрові деталі. Рамка, корпус і акценти зв'язані з однією з 4 фракцій; ручні кольори нижче — лише Custom audit. 1–8 — стани; Tab ховає контролі.", 13, new Color("b7c0c5"));
        note.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(note);

        AddSection(box, "SCENARIO FIXTURES");
        GridContainer scenarios = new() { Columns = 2 }; box.AddChild(scenarios);
        string[] scenarioNames = { "1 Unit", "2 Mixed 45", "3 Production", "4 Brownout", "5 Transform", "6 Alien", "7 Martian", "8 Critical" };
        for (int i = 0; i < scenarioNames.Length; i++)
        {
            M7HudScenario scenario = (M7HudScenario)i;
            Button button = LabButton(scenarioNames[i]); button.Pressed += () => SetScenario(scenario); scenarios.AddChild(button);
        }
        AddSection(box, "RESPONSIVE PREVIEW");
        HFlowContainer aspects = new(); box.AddChild(aspects);
        foreach (string aspect in new[] { "16:9", "16:10", "21:9", "4:3" })
        {
            string selected = aspect;
            Button button = LabButton(aspect); button.Pressed += () => { _aspect = selected; ApplyPreviewAspect(); SetStatus($"Preview {selected}"); }; aspects.AddChild(button);
        }

        AddSection(box, "HUD FINISH · SAME GEOMETRY");
        HFlowContainer finishes = new(); box.AddChild(finishes);
        foreach ((string label, HudArtFinish finish) in new[]
                 {
                     ("HYBRID · VECTOR + RASTER", HudArtFinish.HybridConsole),
                     ("STRUCTURAL CONSOLE", HudArtFinish.StructuralConsole),
                     ("LEGACY FRAMES", HudArtFinish.LegacyFrames),
                     ("CLEAN", HudArtFinish.Clean)
                 })
        {
            HudArtFinish selected = finish;
            Button button = LabButton(label);
            button.Pressed += () =>
            {
                _profile.ArtSkin.Finish = selected;
                ApplyAll();
                SetStatus($"HUD finish: {selected}");
            };
            finishes.AddChild(button);
        }

        AddSection(box, "FACTION HUD KIT · FRAME + SURFACE");
        AddNote(box, "Одна кнопка застосовує повний набір кольорів, стінок, растра й рамки. Astronauts об'єднує Life on Mars field equipment та Mars Mission aerospace design; Aliens не використовує фіолетовий.");
        HFlowContainer factions = new(); box.AddChild(factions);
        string[] factionNames = { "Rock Raiders", "Astronauts", "Aliens", "Martians" };
        for (int i = 0; i < factionNames.Length; i++)
        {
            int faction = i;
            Button button = LabButton(factionNames[i]);
            button.Name = $"FactionKit{faction}";
            button.Pressed += () => SelectFactionKit(faction, true);
            factions.AddChild(button);
        }
        AddToggle(box, "Faction chrome", () => _profile.ArtSkin.Enabled, value => _profile.ArtSkin.Enabled = value);
        AddSlider(box, "Chrome intensity", 0, 1, 0.01, () => _profile.ArtSkin.ChromeIntensity, value => _profile.ArtSkin.ChromeIntensity = (float)value);
        AddSlider(box, "Chrome scale", 0.75, 1.35, 0.01, () => _profile.ArtSkin.ChromeScale, value => _profile.ArtSkin.ChromeScale = (float)value);

        AddSection(box, "LAYOUT");
        AddSlider(box, "Safe area %", 90, 100, 0.5, () => _profile.Layout.SafeAreaPercent, value => _profile.Layout.SafeAreaPercent = (float)value);
        AddSlider(box, "UI scale", 0.8, 1.35, 0.01, () => _profile.Layout.UiScale, value => _profile.Layout.UiScale = (float)value);
        AddSlider(box, "Top strip height", 44, 78, 1, () => _profile.Layout.TopStripHeight, value => _profile.Layout.TopStripHeight = (float)value);
        AddSlider(box, "Bottom height", 172, 280, 1, () => _profile.Layout.BottomRegionHeight, value => _profile.Layout.BottomRegionHeight = (float)value);
        AddSlider(box, "Minimap width target", 164, 260, 1, () => _profile.Layout.MinimapSize, value => _profile.Layout.MinimapSize = (float)value);
        AddSlider(box, "Command width target", 220, 380, 1, () => _profile.Layout.CommandPanelWidth, value => _profile.Layout.CommandPanelWidth = (float)value);
        AddSlider(box, "Panel gap", 4, 24, 1, () => _profile.Layout.PanelGap, value => _profile.Layout.PanelGap = (float)value);

        AddSection(box, "TYPE & SURFACES");
        AddNote(box, "Типографічна ієрархія, базові розміри й контраст тексту тепер дизайнерськи визначені. Лабораторія лишає тільки загальний масштаб.");
        AddSlider(box, "Text scale", 0.8, 1.4, 0.01, () => _profile.Typography.TextScale, value => _profile.Typography.TextScale = (float)value);
        AddSlider(box, "Panel opacity", 0.35, 1, 0.01, () => _profile.Surface.PanelOpacity, value => _profile.Surface.PanelOpacity = (float)value);
        AddSlider(box, "Border", 0, 5, 1, () => _profile.Surface.BorderWidth, value => _profile.Surface.BorderWidth = (int)value);
        AddSlider(box, "Corner radius", 0, 20, 1, () => _profile.Surface.CornerRadius, value => _profile.Surface.CornerRadius = (int)value);
        AddSlider(box, "Inner padding", 4, 24, 1, () => _profile.Surface.InnerPadding, value => _profile.Surface.InnerPadding = (int)value);
        AddSlider(box, "Separation", 2, 18, 1, () => _profile.Surface.Separation, value => _profile.Surface.Separation = (int)value);
        AddToggle(box, "Solid command buttons", () => _profile.Surface.SolidCommandButtons, value => _profile.Surface.SolidCommandButtons = value);
        AddToggle(box, "Health state colors", () => _profile.Surface.HealthStateColors, value => _profile.Surface.HealthStateColors = value);

        AddSection(box, "CONTENT DENSITY");
        AddToggle(box, "Resource labels", () => _profile.Content.ShowResourceLabels, value => _profile.Content.ShowResourceLabels = value);
        AddToggle(box, "Hotkeys", () => _profile.Content.ShowHotkeys, value => _profile.Content.ShowHotkeys = value);
        AddToggle(box, "Portrait slot", () => _profile.Content.ShowPortrait, value => _profile.Content.ShowPortrait = value);
        AddToggle(box, "Event feed", () => _profile.Content.ShowEventFeed, value => _profile.Content.ShowEventFeed = value);
        AddToggle(box, "Objective tracker", () => _profile.Content.ShowObjectiveTracker, value => _profile.Content.ShowObjectiveTracker = value);
        AddToggle(box, "Minimap legend", () => _profile.Content.ShowMinimapLegend, value => _profile.Content.ShowMinimapLegend = value);
        AddToggle(box, "Command costs", () => _profile.Content.ShowCommandCosts, value => _profile.Content.ShowCommandCosts = value);

        AddSection(box, "MINIMAP · FUNCTION & READABILITY");
        AddSlider(box, "Marker scale", 0.6, 2.0, 0.01, () => _profile.Minimap.MarkerScale, value => _profile.Minimap.MarkerScale = (float)value);
        AddSlider(box, "Motion smoothing", 0, 0.30, 0.01, () => _profile.Minimap.InterpolationSeconds, value => _profile.Minimap.InterpolationSeconds = (float)value);
        AddSlider(box, "Explored fog", 0.15, 0.90, 0.01, () => _profile.Minimap.ExploredFogOpacity, value => _profile.Minimap.ExploredFogOpacity = (float)value);
        AddSlider(box, "Unseen fog", 0.55, 1.0, 0.01, () => _profile.Minimap.UnseenFogOpacity, value => _profile.Minimap.UnseenFogOpacity = (float)value);
        AddSlider(box, "Remembered opacity", 0.15, 0.85, 0.01, () => _profile.Minimap.RememberedOpacity, value => _profile.Minimap.RememberedOpacity = (float)value);
        AddSlider(box, "Grid opacity", 0, 0.30, 0.01, () => _profile.Minimap.GridOpacity, value => _profile.Minimap.GridOpacity = (float)value);
        AddSlider(box, "Viewport line", 1, 5, 0.1, () => _profile.Minimap.ViewportLineWidth, value => _profile.Minimap.ViewportLineWidth = (float)value);
        AddSlider(box, "Alert pulse", 0.5, 2.5, 0.05, () => _profile.Minimap.AlertPulseScale, value => _profile.Minimap.AlertPulseScale = (float)value);
        AddToggle(box, "Camera viewport", () => _profile.Minimap.ShowViewport, value => _profile.Minimap.ShowViewport = value);
        AddToggle(box, "Alert pings", () => _profile.Minimap.ShowAlerts, value => _profile.Minimap.ShowAlerts = value);
        AddToggle(box, "Network lines", () => _profile.Minimap.ShowNetworkLines, value => _profile.Minimap.ShowNetworkLines = value);
        AddColor(box, "Map ground", () => _profile.Minimap.GroundColor, value => _profile.Minimap.GroundColor = value);
        AddColor(box, "Map rough", () => _profile.Minimap.RoughColor, value => _profile.Minimap.RoughColor = value);
        AddColor(box, "Map blocked", () => _profile.Minimap.BlockedColor, value => _profile.Minimap.BlockedColor = value);
        AddColor(box, "Map excavatable", () => _profile.Minimap.ExcavatableColor, value => _profile.Minimap.ExcavatableColor = value);
        AddColor(box, "Owned markers", () => _profile.Minimap.OwnedColor, value => _profile.Minimap.OwnedColor = value);
        AddColor(box, "Allied markers", () => _profile.Minimap.AlliedColor, value => _profile.Minimap.AlliedColor = value);
        AddColor(box, "Enemy markers", () => _profile.Minimap.EnemyColor, value => _profile.Minimap.EnemyColor = value);
        AddColor(box, "Neutral markers", () => _profile.Minimap.NeutralColor, value => _profile.Minimap.NeutralColor = value);
        AddColor(box, "Resources", () => _profile.Minimap.ResourceColor, value => _profile.Minimap.ResourceColor = value);
        AddColor(box, "Camera viewport", () => _profile.Minimap.ViewportColor, value => _profile.Minimap.ViewportColor = value);
        AddColor(box, "Alerts", () => _profile.Minimap.AlertColor, value => _profile.Minimap.AlertColor = value);

        AddSection(box, "COLOR TOKENS");
        AddColor(box, "Background", () => _profile.Colors.Background, value => SetCustomColor(v => _profile.Colors.Background = v, value));
        AddColor(box, "Raised", () => _profile.Colors.Raised, value => SetCustomColor(v => _profile.Colors.Raised = v, value));
        AddColor(box, "Recessed", () => _profile.Colors.Recessed, value => SetCustomColor(v => _profile.Colors.Recessed = v, value));
        AddColor(box, "Accent", () => _profile.Colors.Accent, value => SetCustomColor(v => _profile.Colors.Accent = v, value));
        AddColor(box, "Good", () => _profile.Colors.Good, value => SetCustomColor(v => _profile.Colors.Good = v, value));
        AddColor(box, "Warning", () => _profile.Colors.Warning, value => SetCustomColor(v => _profile.Colors.Warning = v, value));
        AddColor(box, "Danger", () => _profile.Colors.Danger, value => SetCustomColor(v => _profile.Colors.Danger = v, value));
        AddColor(box, "Selection", () => _profile.Colors.Selection, value => SetCustomColor(v => _profile.Colors.Selection = v, value));

        AddSection(box, "PROFILE");
        HFlowContainer actions = new(); box.AddChild(actions);
        Button copy = LabButton("COPY JSON"); copy.Name = "CopyHudProfile"; copy.Pressed += CopyProfile; actions.AddChild(copy);
        Button paste = LabButton("PASTE JSON"); paste.Name = "PasteHudProfile"; paste.Pressed += PasteProfile; actions.AddChild(paste);
        Button reset = LabButton("RESET"); reset.Pressed += ResetProfile; actions.AddChild(reset);
        _status = LabLabel("Ready.", 12, new Color("65c987")); _status.AutowrapMode = TextServer.AutowrapMode.WordSmart; box.AddChild(_status);
        Button back = LabButton("RETURN TO PROTOTYPE"); back.Pressed += () => _returnToPrototype?.Invoke(); box.AddChild(back);
    }

    private void ApplyAll()
    {
        _profile.Normalize();
        ApplyPreviewAspect();
        HudFrame frame = EffectivePreviewFrame();
        _hud?.ApplyFrame(frame, true);
        _hud?.ApplyProfile(_profile);
        ApplySyntheticCameraPolygon();
    }

    private void SetScenario(M7HudScenario scenario)
    {
        _scenario = scenario;
        if (!_previewFactionOverride.HasValue) BindFactionToScenario(scenario);
        ApplyAll();
        SetStatus($"Scenario {M7HudFixtures.Slug(scenario)}");
    }

    private void SelectFactionKit(int faction, bool announce)
    {
        HudFactionSkinRecipe recipe = HudFactionSkinLibrary.RecipeFor(faction);
        _previewFactionOverride = recipe.Faction;
        HudFactionSkinLibrary.Apply(_profile, faction);
        ApplyAll();
        if (announce) SetStatus($"Faction HUD kit: {recipe.Name}");
    }

    private HudFrame EffectivePreviewFrame()
    {
        HudFrame frame = M7HudFixtures.Create(_scenario);
        if (_previewFactionOverride.HasValue) frame.Faction = _previewFactionOverride.Value;
        return frame;
    }

    private void BindFactionToScenario(M7HudScenario scenario)
    {
        int faction = HudFactionSkinLibrary.IndexFor(M7HudFixtures.Create(scenario).Faction);
        if (_profile.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound)
            HudFactionSkinLibrary.Apply(_profile, faction);
        else
            _profile.ArtSkin.Faction = faction;
    }

    private void ApplyPreviewAspect()
    {
        if (_previewFrame is null) return;
        Vector2 viewport = GetViewport().GetVisibleRect().Size;
        float targetAspect = _aspect switch { "16:10" => 1.6f, "21:9" => 21f / 9f, "4:3" => 4f / 3f, _ => 16f / 9f };
        float width = viewport.X, height = width / targetAspect;
        if (height > viewport.Y) { height = viewport.Y; width = height * targetAspect; }
        _previewFrame.Position = (viewport - new Vector2(width, height)) * 0.5f;
        _previewFrame.Size = new Vector2(width, height);
    }

    private bool ValidateLab()
    {
        bool fixtures = Enum.GetValues<M7HudScenario>().All(scenario =>
        {
            HudFrame frame = M7HudFixtures.Create(scenario);
            return frame.ContentSignature().Length > 80 && frame.Commands.Count <= 12 &&
                !frame.Selection.PortraitCaption.Contains("ART PENDING", StringComparison.OrdinalIgnoreCase) &&
                frame.Minimap.ValidateClientKnowledge(0, out _);
        });
        HudFrame mixed = M7HudFixtures.Create(M7HudScenario.MixedArmy);
        HudFrame production = M7HudFixtures.Create(M7HudScenario.Production);
        HudFrame brownout = M7HudFixtures.Create(M7HudScenario.Brownout);
        HudFrame critical = M7HudFixtures.Create(M7HudScenario.CriticalTooltip);
        string json = _profile.ToJson();
        bool roundTrip = M7HudProfile.TryFromJson(json, out M7HudProfile parsed, out _) && parsed.SchemaVersion == M7HudProfile.CurrentSchemaVersion &&
            Math.Abs(parsed.Layout.SafeAreaPercent - _profile.Layout.SafeAreaPercent) < 0.001f &&
            parsed.ArtSkin.Finish == _profile.ArtSkin.Finish &&
            parsed.ArtSkin.SurfacePalette == _profile.ArtSkin.SurfacePalette;
        bool migration = M7HudProfile.TryFromJson("{\"schemaVersion\":1}", out M7HudProfile migrated, out _) &&
            migrated.SchemaVersion == M7HudProfile.CurrentSchemaVersion && migrated.ArtSkin.Finish == HudArtFinish.LegacyFrames &&
            migrated.ArtSkin.SurfacePalette == HudSurfacePalette.Custom && migrated.Colors.Background == "#111820";
        const string legacyFactionArt = "{\"schemaVersion\":4,\"artSkin\":{\"enabled\":true,\"faction\":2,\"frameOpacity\":0.63,\"frameThickness\":22}}";
        bool chromeMigration = M7HudProfile.TryFromJson(legacyFactionArt, out M7HudProfile migratedChrome, out _) &&
            Math.Abs(migratedChrome.ArtSkin.ChromeIntensity - 0.63f) < 0.001f &&
            Math.Abs(migratedChrome.ArtSkin.ChromeScale - 1f) < 0.001f &&
            migratedChrome.ArtSkin.Finish == HudArtFinish.LegacyFrames &&
            migratedChrome.ArtSkin.FrameOpacity is null && migratedChrome.ArtSkin.FrameThickness is null;
        const string schemaFive = "{\"schemaVersion\":5,\"artSkin\":{\"finish\":2},\"colors\":{\"background\":\"#202122\"}}";
        bool schemaFiveMigration = M7HudProfile.TryFromJson(schemaFive, out M7HudProfile migratedFive, out _) &&
            migratedFive.ArtSkin.Finish == HudArtFinish.Clean &&
            migratedFive.ArtSkin.SurfacePalette == HudSurfacePalette.Custom &&
            migratedFive.Colors.Background == "#202122" && migratedFive.Colors.Raised == "#1b2731";
        const string schemaSix = "{\"schemaVersion\":6,\"artSkin\":{\"finish\":3,\"surfacePalette\":3,\"chromeIntensity\":0.73,\"chromeScale\":1.08},\"colors\":{\"background\":\"#522419\",\"raised\":\"#8b4430\",\"recessed\":\"#24130f\",\"accent\":\"#ff8a36\"}}";
        bool schemaSixMigration = M7HudProfile.TryFromJson(schemaSix, out M7HudProfile migratedSix, out _) &&
            migratedSix.SchemaVersion == M7HudProfile.CurrentSchemaVersion &&
            migratedSix.ArtSkin.Finish == HudArtFinish.HybridConsole &&
            migratedSix.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound &&
            Math.Abs(migratedSix.ArtSkin.ChromeIntensity - 0.73f) < 0.001f &&
            Math.Abs(migratedSix.ArtSkin.ChromeScale - 1.08f) < 0.001f &&
            migratedSix.Colors.Background == "#303534" && migratedSix.Colors.Accent == "#a76538" &&
            M7HudProfile.TryFromJson(migratedSix.ToJson(), out M7HudProfile schemaSixRoundTrip, out _) &&
            schemaSixRoundTrip.SchemaVersion == M7HudProfile.CurrentSchemaVersion &&
            schemaSixRoundTrip.ArtSkin.Finish == HudArtFinish.HybridConsole &&
            schemaSixRoundTrip.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound &&
            schemaSixRoundTrip.Colors.Background == "#303534";
        const string malformedColors = "{\"schemaVersion\":2,\"colors\":{\"background\":\"invalid\",\"accent\":null},\"minimap\":{\"enemyColor\":\"not-a-color\"}}";
        bool profileSanitization = M7HudProfile.TryFromJson(malformedColors, out M7HudProfile sanitized, out _) &&
            sanitized.Colors.Background == "#cfc6ae" && sanitized.Colors.Accent == "#d95f24" &&
            sanitized.Minimap.EnemyColor == "#ff6b45";
        bool mapping = HudMinimapView.PixelToBuildCell(Vector2.Zero, new Vector2(206, 206)) == Vector2I.Zero &&
            HudMinimapView.PixelToBuildCell(new Vector2(205.9f, 205.9f), new Vector2(206, 206)) == new Vector2I(159, 159);
        FixVec2 commandTarget = RtsInputController.MinimapBuildCellCenter(new Vector2I(12, 34));
        mapping = mapping && commandTarget.X == Fix32.FromRatio(25, 2) && commandTarget.Y == Fix32.FromRatio(69, 2);
        bool tree = _hud?.FindChild("ResourceStrip", true, false) is PanelContainer && _hud.FindChild("BottomDeck", true, false) is PanelContainer &&
            _hud.FindChild("MinimapSlot", true, false) is HudMinimapView &&
            _hud.FindChild("TacticalPortrait", true, false) is HudPortraitView &&
            _hud.FindChild("PortraitSlot", true, false) is PanelContainer &&
            _hud.FindChild("ResourceStripFactionSurfaceMask", true, false) is HudFactionSurfaceMask &&
            _hud.FindChild("BottomDeckFactionSurfaceMask", true, false) is HudFactionSurfaceMask &&
            _hud.FindChild("BottomDeckRasterSurface", true, false) is HudRasterSurfaceOverlay &&
            _hud.FindChild("SelectionPanelRasterSurface", true, false) is null &&
            _hud.FindChild("CommandPanelRasterSurface", true, false) is null &&
            _hud.FindChild("SelectionPanel", true, false) is PanelContainer && _hud.FindChild("CommandGrid", true, false) is GridContainer &&
            _hud.FindChild("EventFeed", true, false) is PanelContainer && _hud.FindChild("HudTooltip", true, false) is PanelContainer;
        HudFactionChrome? topChrome = _hud?.FindChild("ResourceStripFactionChrome", true, false) as HudFactionChrome;
        HudFactionChrome? deckChrome = _hud?.FindChild("BottomDeckFactionChrome", true, false) as HudFactionChrome;
        HudFactionSurfaceMask? topMask = _hud?.FindChild("ResourceStripFactionSurfaceMask", true, false) as HudFactionSurfaceMask;
        HudFactionSurfaceMask? deckMask = _hud?.FindChild("BottomDeckFactionSurfaceMask", true, false) as HudFactionSurfaceMask;
        HudStructuralChrome? topStructural = _hud?.FindChild("ResourceStripStructuralChrome", true, false) as HudStructuralChrome;
        HudStructuralChrome? deckStructural = _hud?.FindChild("BottomDeckStructuralChrome", true, false) as HudStructuralChrome;
        bool expectsApertureMask = _profile.ArtSkin.Enabled &&
            _profile.ArtSkin.Finish is HudArtFinish.HybridConsole or HudArtFinish.LegacyFrames;
        bool factionArt = HudFactionChrome.ValidateRecipes(out _) &&
            topChrome is { IsConfigured: true, Role: HudFactionChromeRole.TopStrip, UsesFixedSquareCorners: true,
                UsesProtectedSourceModules: true, SupportsCompleteHybridPerimeter: true,
                SupportsIsotropicRasterModules: true } &&
            deckChrome is { IsConfigured: true, Role: HudFactionChromeRole.BottomDeck,
                SupportsCompleteHybridPerimeter: true, SupportsIsotropicRasterModules: true } &&
            topMask is { IsConfigured: true, Role: HudFactionChromeRole.TopStrip,
                UsesSharedNineSliceGeometry: true, UsesShapedApertureCorners: true,
                HasTransparentOuterCorners: true } &&
            deckMask is { IsConfigured: true, Role: HudFactionChromeRole.BottomDeck,
                UsesSharedNineSliceGeometry: true, UsesShapedApertureCorners: true,
                HasTransparentOuterCorners: true, UsesFullBleedBottomDeck: true,
                RegisteredMaskedSurfaceCount: >= 4 } &&
            topMask.UsesFactionApertureMask == expectsApertureMask &&
            deckMask.UsesFactionApertureMask == expectsApertureMask &&
            topMask.UsesShaderApertureMask == expectsApertureMask &&
            deckMask.UsesShaderApertureMask == expectsApertureMask &&
            topMask.ClipsRasterAndContent == expectsApertureMask &&
            deckMask.ClipsRasterAndContent == expectsApertureMask &&
            topStructural is { IsConfigured: true, Role: HudFactionChromeRole.TopStrip } &&
            deckStructural is { IsConfigured: true, Role: HudFactionChromeRole.BottomDeck } &&
            _hud?.FindChild("BottomDeckRasterSurface", true, false) is HudRasterSurfaceOverlay
                { IsConfigured: true, UsesBoundedRasterPlates: true, UsesUnstretchedSourceRegions: true,
                    UsesSingleContinuousSurfaceField: true, UsesSingleDeckWideSurface: true } &&
            _hud?.FindChild("MinimapRegionFactionChrome", true, false) is null &&
            _hud?.FindChild("SelectionPanelFactionChrome", true, false) is null &&
            _hud?.FindChild("CommandPanelFactionChrome", true, false) is null &&
            _hud?.FindChild("PortraitSlotFactionChrome", true, false) is null &&
            _hud?.FindChild("EnergyDomainPopoverFactionChrome", true, false) is null &&
            _hud?.FindChild("ResourceStripFactionFrame", true, false) is null;
        bool states = mixed.Selection.Groups.Count == 5 && mixed.Selection.Count == 45 && production.Queue.Count == 3 &&
            brownout.Alert.Priority == HudAlertPriority.High && brownout.EnergyPopoverVisible && critical.ExpandedTooltip;
        string actionableSignature = brownout.ContentSignature();
        brownout.Alert.Actionable = !brownout.Alert.Actionable;
        bool actionableBinding = actionableSignature != brownout.ContentSignature();
        bool objectiveBounded = _hud?.FindChild("ObjectiveTracker", true, false) is Control objective &&
            objective.Size.Y <= 100f * PreviewRenderScale();
        bool typography = HudTypographyLibrary.Validate(out string typographyError);
        bool textContrast = HudTextPalette.ValidateFactionRecipes(out string textContrastError);
        bool finishModes = ValidateFinishModes();
        bool factionSkinModes = ValidateFactionSkinModes();
        bool layoutContained = ValidateSafeAreaContainment();
        bool interactionBindings = ValidateInteractiveBindings();
        bool productionKnowledge = ValidateProductionMinimapKnowledge();
        if (_hud?.FindChild("CommandPanel", true, false) is Control commandPanel &&
            _hud.FindChild("SelectionPanel", true, false) is Control selectionPanel &&
            _hud.FindChild("MinimapRegion", true, false) is Control minimapPanel &&
            _hud.FindChild("EventFeed", true, false) is Control eventPanel &&
            _hud.FindChild("ObjectiveTracker", true, false) is Control objectivePanel)
        {
            Label? objectiveLabel = objectivePanel.GetChildCount() > 0 ? objectivePanel.GetChild(0) as Label : null;
            GD.Print($"M7 HUD LAB LAYOUT: command={commandPanel.Position}/{commandPanel.Size} min={commandPanel.GetCombinedMinimumSize()} anchors={commandPanel.AnchorTop:0.#}-{commandPanel.AnchorBottom:0.#} selection={selectionPanel.Position}/{selectionPanel.Size} min={selectionPanel.GetCombinedMinimumSize()} anchors={selectionPanel.AnchorTop:0.#}-{selectionPanel.AnchorBottom:0.#} minimap={minimapPanel.Position}/{minimapPanel.Size} min={minimapPanel.GetCombinedMinimumSize()} anchors={minimapPanel.AnchorTop:0.#}-{minimapPanel.AnchorBottom:0.#} events={eventPanel.Position}/{eventPanel.Size} objective={objectivePanel.Position}/{objectivePanel.Size} label={objectiveLabel?.Position}/{objectiveLabel?.Size} text={objectiveLabel?.Text.Length ?? 0} anchors={objectivePanel.AnchorTop:0.#}-{objectivePanel.AnchorBottom:0.#} preview={_previewFrame?.Size} topFill={topMask?.SurfaceFillColor.ToHtml()} deckFill={deckMask?.SurfaceFillColor.ToHtml()}");
        }
        bool minimap = _hud?.FindChild("MinimapSlot", true, false) is HudMinimapView view && view.IsConfigured && view.IsNorthUp &&
            view.MarkerCount == 11 && view.RememberedMarkerCount == 2 && view.VisibleFogCells > 0 && view.ExploredFogCells > 0;
        HudMinimapFrame illegal = M7HudFixtures.Create(M7HudScenario.MixedArmy).Minimap;
        illegal.Markers.Add(new HudMinimapMarkerFrame
        {
            StableId = 9999, Owner = 1, BuildX = 20, BuildY = 20,
            Kind = HudMinimapMarkerKind.GroundMobile, Relation = HudMinimapRelation.Enemy
        });
        bool leakGuard = !illegal.ValidateClientKnowledge(0, out _);
        if (!(fixtures && roundTrip && migration && chromeMigration && schemaFiveMigration && schemaSixMigration && profileSanitization && mapping && tree && factionArt && finishModes && factionSkinModes && states && actionableBinding &&
              objectiveBounded && typography && textContrast && layoutContained && interactionBindings && minimap && leakGuard && productionKnowledge))
            GD.PrintErr($"M7 HUD LAB DETAIL: fixtures={fixtures} roundTrip={roundTrip} migration={migration} chromeMigration={chromeMigration} schema5={schemaFiveMigration} schema6={schemaSixMigration} sanitization={profileSanitization} mapping={mapping} tree={tree} factionArt={factionArt} finishes={finishModes} factionSkins={factionSkinModes} states={states} actionable={actionableBinding} objectiveBounded={objectiveBounded} typography={typography}:{typographyError} textContrast={textContrast}:{textContrastError} layoutContained={layoutContained} interactions={interactionBindings} minimap={minimap} leakGuard={leakGuard} productionKnowledge={productionKnowledge}");
        return fixtures && roundTrip && migration && chromeMigration && schemaFiveMigration && schemaSixMigration && profileSanitization && mapping && tree && factionArt && finishModes && factionSkinModes && states && actionableBinding &&
            objectiveBounded && typography && textContrast && layoutContained && interactionBindings && minimap && leakGuard && productionKnowledge;
    }

    private bool ValidateInteractiveBindings()
    {
        if (_hud?.FindChild("EnergyButton", true, false) is not Button energyButton ||
            _hud.FindChild("ActionableAlert", true, false) is not Button alertButton ||
            _hud.FindChild("Command0", true, false) is not Button commandButton) return false;
        foreach (Node child in GetDescendants(energyButton))
            if (child is Control control && control.MouseFilter != Control.MouseFilterEnum.Ignore) return false;

        int energyRequests = 0;
        void OnEnergyRequested() => energyRequests++;
        _hud.EnergyDetailsRequested += OnEnergyRequested;
        energyButton.EmitSignal(BaseButton.SignalName.Pressed);
        _hud.EnergyDetailsRequested -= OnEnergyRequested;

        _hud.ApplyFrame(M7HudFixtures.Create(M7HudScenario.CriticalTooltip), true);
        int alertRequests = 0;
        void OnAlertRequested() => alertRequests++;
        _hud.AlertRequested += OnAlertRequested;
        alertButton.EmitSignal(BaseButton.SignalName.Pressed);
        _hud.AlertRequested -= OnAlertRequested;

        M7HudProfile outlineButtons = _profile.Clone();
        outlineButtons.Surface.SolidCommandButtons = false;
        _hud.ApplyProfile(outlineButtons);
        float outlineAlpha = commandButton.GetThemeStylebox("normal") is StyleBoxFlat outlineStyle
            ? outlineStyle.BgColor.A : 1f;
        M7HudProfile solidButtons = _profile.Clone();
        solidButtons.Surface.SolidCommandButtons = true;
        _hud.ApplyProfile(solidButtons);
        float solidAlpha = commandButton.GetThemeStylebox("normal") is StyleBoxFlat solidStyle
            ? solidStyle.BgColor.A : 0f;

        _hud.ApplyProfile(_profile);
        _hud.ApplyFrame(EffectivePreviewFrame(), true);
        ApplySyntheticCameraPolygon();
        return energyRequests == 1 && alertRequests == 1 && outlineAlpha < 0.05f && solidAlpha > 0.95f;
    }

    private bool ValidateFinishModes()
    {
        if (_hud is null) return false;
        M7HudProfile original = _profile.Clone();
        HudFrame frame = EffectivePreviewFrame();
        Rect2[]? baseline = null;
        bool valid = true;
        foreach (HudArtFinish finish in Enum.GetValues<HudArtFinish>())
        {
            bool finishValid = true;
            M7HudProfile candidate = original.Clone();
            candidate.ArtSkin.Enabled = true;
            candidate.ArtSkin.Finish = finish;
            _hud.ApplyProfile(candidate);
            _hud.ApplyFrame(frame, true);
            HudFactionChrome? rasterFrame = _hud.FindChild("BottomDeckFactionChrome", true, false) as HudFactionChrome;
            HudStructuralChrome? structural = _hud.FindChild("BottomDeckStructuralChrome", true, false) as HudStructuralChrome;
            HudRasterSurfaceOverlay? raster = _hud.FindChild("BottomDeckRasterSurface", true, false) as HudRasterSurfaceOverlay;
            HudFactionSurfaceMask? mask = _hud.FindChild("BottomDeckFactionSurfaceMask", true, false) as HudFactionSurfaceMask;
            finishValid &= finish switch
            {
                HudArtFinish.HybridConsole =>
                    structural is { Visible: true, UsesInteriorOnlyHybrid: true, UsesSculptedShoulders: false,
                        UsesFactionRasterModules: false, UsesCleanHybridSeparators: true } &&
                    rasterFrame is { Visible: true, IsFrameOnly: true, UsesFactionSurfaceFill: false,
                        UsesVectorAccentRails: true, UsesContinuousHybridRails: true, UsesTiledEdgeWalls: false,
                        UsesSparseJunctionModules: false, UsesCompleteHybridPerimeter: true,
                        UsesIsotropicRasterModules: true, AvoidsFullSpanRasterStretch: true } &&
                    mask is { UsesFactionApertureMask: true, UsesShaderApertureMask: true,
                        UsesShapedApertureCorners: true, HasTransparentOuterCorners: true,
                        UsesFullBleedBottomDeck: true,
                        ClipsRasterAndContent: true, RegisteredMaskedSurfaceCount: >= 4 } &&
                    raster is { Visible: true, UsesSingleContinuousSurfaceField: true, UsesSingleDeckWideSurface: true },
                HudArtFinish.StructuralConsole =>
                    structural is { Visible: true, UsesInteriorOnlyHybrid: false, UsesSculptedShoulders: true,
                        UsesFactionRasterModules: false } && rasterFrame is { Visible: false } &&
                    mask is { UsesFactionApertureMask: false, UsesShaderApertureMask: false,
                        ClipsRasterAndContent: false } && raster is { Visible: false },
                HudArtFinish.LegacyFrames =>
                    rasterFrame is { Visible: true, IsFrameOnly: false, UsesFactionSurfaceFill: true,
                        UsesVectorAccentRails: true, UsesTiledEdgeWalls: true, UsesSparseJunctionModules: true } &&
                    mask is { UsesFactionApertureMask: true, UsesShaderApertureMask: true,
                        UsesShapedApertureCorners: true, HasTransparentOuterCorners: true,
                        UsesFullBleedBottomDeck: true,
                        ClipsRasterAndContent: true, RegisteredMaskedSurfaceCount: >= 4 } &&
                    structural is { Visible: false } && raster is { Visible: false },
                _ => rasterFrame is { Visible: false } && structural is { Visible: false } &&
                    mask is { UsesFactionApertureMask: false, UsesShaderApertureMask: false,
                        ClipsRasterAndContent: false } && raster is { Visible: false }
            };
            Rect2[]? rects = CapturePrimaryRects();
            if (rects is null) finishValid = false;
            else if (baseline is null) baseline = rects;
            else
                for (int i = 0; i < baseline.Length; i++)
                    finishValid &= RectNearlyEqual(baseline[i], rects[i]);
            if (!finishValid)
                GD.PrintErr($"M7 HUD LAB FINISH DETAIL: finish={finish} rects={FormatRects(rects)} baseline={FormatRects(baseline)}");
            valid &= finishValid;
        }
        _profile = original;
        ApplyAll();
        return valid;
    }

    private bool ValidateFactionSkinModes()
    {
        if (_hud is null) return false;
        M7HudProfile original = _profile.Clone();
        HudFaction? originalOverride = _previewFactionOverride;
        string fixtureTitle = M7HudFixtures.Create(_scenario).Selection.Title;
        HashSet<string> signatures = new(StringComparer.Ordinal);
        HashSet<string> apertureSignatures = new(StringComparer.Ordinal);
        bool expectsApertureMask = original.ArtSkin.Enabled &&
            original.ArtSkin.Finish is HudArtFinish.HybridConsole or HudArtFinish.LegacyFrames;
        Rect2? topBaseline = null;
        Rect2? deckBaseline = null;
        bool valid = true;
        for (int faction = 0; faction < HudFactionSkinLibrary.Count; faction++)
        {
            Button? kitButton = FindChild($"FactionKit{faction}", true, false) as Button;
            if (kitButton is null) valid = false;
            else kitButton.EmitSignal(Button.SignalName.Pressed);
            HudFactionSkinRecipe recipe = HudFactionSkinLibrary.RecipeFor(faction);
            HudFactionChrome? topChrome = _hud.FindChild("ResourceStripFactionChrome", true, false) as HudFactionChrome;
            HudFactionChrome? deckChrome = _hud.FindChild("BottomDeckFactionChrome", true, false) as HudFactionChrome;
            HudFactionSurfaceMask? topMask = _hud.FindChild("ResourceStripFactionSurfaceMask", true, false) as HudFactionSurfaceMask;
            HudFactionSurfaceMask? deckMask = _hud.FindChild("BottomDeckFactionSurfaceMask", true, false) as HudFactionSurfaceMask;
            valid &= _profile.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound &&
                _profile.ArtSkin.Faction == faction && _hud.Profile.ArtSkin.Faction == faction &&
                _hud.Frame.Faction == recipe.Faction && _hud.Frame.Selection.Title == fixtureTitle &&
                topChrome?.Faction == recipe.Faction && deckChrome?.Faction == recipe.Faction &&
                topMask is { IsConfigured: true } && deckMask is { IsConfigured: true } &&
                topMask.UsesFactionApertureMask == expectsApertureMask &&
                deckMask.UsesFactionApertureMask == expectsApertureMask &&
                topMask.UsesShaderApertureMask == expectsApertureMask &&
                deckMask.UsesShaderApertureMask == expectsApertureMask &&
                topMask.UsesShapedApertureCorners && deckMask.UsesShapedApertureCorners &&
                topMask.HasTransparentOuterCorners && deckMask.HasTransparentOuterCorners &&
                deckMask.UsesFullBleedBottomDeck && deckMask.RegisteredMaskedSurfaceCount >= 4 &&
                topMask.Faction == recipe.Faction && deckMask.Faction == recipe.Faction &&
                (faction != (int)HudFaction.RockRaiders ||
                    RelativeLuminance(topMask.SurfaceFillColor) < 0.28f &&
                    RelativeLuminance(deckMask.SurfaceFillColor) < 0.18f) &&
                kitButton is not null;
            signatures.Add($"{_profile.Colors.Background}/{_profile.Colors.Raised}/{_profile.Colors.Recessed}/{_profile.Colors.Accent}/{_profile.Colors.TextPrimary}");
            apertureSignatures.Add($"{recipe.ApertureInsetRatios}/{recipe.DestinationScale:0.00}/{recipe.ApertureCornerRadiusFraction:0.00}");

            if (_hud.FindChild("ResourceStrip", true, false) is not Control top ||
                _hud.FindChild("BottomDeck", true, false) is not Control deck) valid = false;
            else if (topBaseline is null)
            {
                topBaseline = top.GetGlobalRect();
                deckBaseline = deck.GetGlobalRect();
            }
            else valid &= RectNearlyEqual(topBaseline.Value, top.GetGlobalRect()) &&
                RectNearlyEqual(deckBaseline!.Value, deck.GetGlobalRect());
        }
        HudFactionSkinRecipe alien = HudFactionSkinLibrary.RecipeFor((int)HudFaction.Aliens);
        valid &= signatures.Count == HudFactionSkinLibrary.Count &&
            apertureSignatures.Count == HudFactionSkinLibrary.Count &&
            alien.Background == "#111315" && alien.Accent == "#78b82a" && alien.Secondary == "#6f777b";
        _profile = original;
        _previewFactionOverride = originalOverride;
        ApplyAll();
        return valid;
    }

    private Rect2[]? CapturePrimaryRects()
    {
        if (_hud is null) return null;
        string[] names = { "ResourceStrip", "BottomDeck", "MinimapRegion", "SelectionPanel", "PortraitSlot", "CommandPanel" };
        Rect2[] rects = new Rect2[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            if (_hud.FindChild(names[i], true, false) is not Control control) return null;
            rects[i] = control.GetGlobalRect();
        }
        return rects;
    }

    private static bool RectNearlyEqual(Rect2 left, Rect2 right) =>
        left.Position.DistanceTo(right.Position) <= 0.5f && left.Size.DistanceTo(right.Size) <= 0.5f;

    private static string FormatRects(Rect2[]? rects) => rects is null
        ? "missing"
        : string.Join(";", rects.Select(rect => $"{rect.Position}/{rect.Size}"));

    private float PreviewRenderScale()
    {
        float height = _previewFrame?.Size.Y ?? 1080f;
        return _profile.Layout.UiScale * Mathf.Clamp(height / 1080f, 2f / 3f, 2f);
    }

    private static IEnumerable<Node> GetDescendants(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            yield return child;
            foreach (Node descendant in GetDescendants(child)) yield return descendant;
        }
    }

    private bool ValidateSafeAreaContainment()
    {
        if (_hud?.FindChild("HudSafeArea", true, false) is not Control safeArea) return false;
        Rect2 safeBounds = safeArea.GetGlobalRect();
        string[] panelNames = { "ResourceStrip", "BottomDeck", "MinimapRegion", "SelectionPanel", "PortraitSlot", "CommandPanel" };
        for (int i = 0; i < panelNames.Length; i++)
        {
            if (_hud.FindChild(panelNames[i], true, false) is not Control panel) return false;
            Rect2 bounds = panel.GetGlobalRect();
            if (bounds.Position.X < safeBounds.Position.X - 0.5f || bounds.Position.Y < safeBounds.Position.Y - 0.5f ||
                bounds.End.X > safeBounds.End.X + 0.5f || bounds.End.Y > safeBounds.End.Y + 0.5f)
                return false;
        }
        if (_hud.FindChild("CommandPanel", true, false) is not Control commandPanel) return false;
        Rect2 commandBounds = commandPanel.GetGlobalRect();
        int expectedVisibleCommands = _hud.Frame.Commands.Count(command => command.Visible);
        int visibleWells = 0;
        int populatedCommands = 0;
        for (int i = 0; i < 12; i++)
        {
            if (_hud.FindChild($"Command{i}", true, false) is not Control command) return false;
            if (!command.Visible) continue;
            visibleWells++;
            if (_hud.Frame.Commands.Count > i && _hud.Frame.Commands[i].Visible) populatedCommands++;
            Rect2 commandButtonBounds = command.GetGlobalRect();
            if (commandButtonBounds.Position.X < commandBounds.Position.X - 0.5f ||
                commandButtonBounds.Position.Y < commandBounds.Position.Y - 0.5f ||
                commandButtonBounds.End.X > commandBounds.End.X + 0.5f ||
                commandButtonBounds.End.Y > commandBounds.End.Y + 0.5f)
                return false;
        }
        if (visibleWells != expectedVisibleCommands || populatedCommands != expectedVisibleCommands ||
            _hud.FindChild("BottomDeck", true, false) is not Control bottomDeck) return false;

        if (_hud.FindChild("MinimapRegion", true, false) is not Control minimapPanel ||
            _hud.FindChild("SelectionPanel", true, false) is not Control selectionPanel ||
            _hud.FindChild("PortraitSlot", true, false) is not Control portraitPanel ||
            _hud.FindChild("BottomDeckContent", true, false) is not Control deckContent ||
            _hud.FindChild("BottomDeckFactionSurfaceMask", true, false) is not HudFactionSurfaceMask deckMask) return false;
        Rect2 deckBounds = bottomDeck.GetGlobalRect();
        Rect2 contentBounds = deckContent.GetGlobalRect();
        Rect2 minimapBounds = minimapPanel.GetGlobalRect();
        Rect2 selectionBounds = selectionPanel.GetGlobalRect();
        Rect2 portraitBounds = portraitPanel.GetGlobalRect();
        Rect2 expectedContent = deckMask.InteractiveRectFor(deckMask.Size);
        if (!ContainsRect(deckBounds, contentBounds) || !ContainsRect(contentBounds, minimapBounds) ||
            !ContainsRect(contentBounds, selectionBounds) || !ContainsRect(contentBounds, portraitBounds) ||
            !ContainsRect(contentBounds, commandBounds) ||
            deckContent.Position.DistanceTo(expectedContent.Position) > 0.5f ||
            deckContent.Size.DistanceTo(expectedContent.Size) > 0.5f ||
            Math.Abs(minimapBounds.Position.X - contentBounds.Position.X) > 0.5f ||
            Math.Abs(commandBounds.End.X - contentBounds.End.X) > 0.5f ||
            Math.Abs(minimapBounds.Position.Y - contentBounds.Position.Y) > 0.5f ||
            Math.Abs(selectionBounds.Position.Y - contentBounds.Position.Y) > 0.5f ||
            Math.Abs(portraitBounds.Position.Y - contentBounds.Position.Y) > 0.5f ||
            Math.Abs(commandBounds.Position.Y - contentBounds.Position.Y) > 0.5f ||
            minimapBounds.End.X > selectionBounds.Position.X || selectionBounds.End.X > portraitBounds.Position.X ||
            portraitBounds.End.X > commandBounds.Position.X) return false;

        List<Rect2> visibleOverlays = new();
        string[] overlayNames = { "AlertAccess", "EventFeed", "ObjectiveTracker", "HudTooltip", "EnergyDomainPopover" };
        for (int i = 0; i < overlayNames.Length; i++)
        {
            if (_hud.FindChild(overlayNames[i], true, false) is not Control overlay) return false;
            if (!overlay.Visible) continue;
            Rect2 bounds = overlay.GetGlobalRect();
            if (!ContainsRect(safeBounds, bounds) || RectsOverlap(bounds, bottomDeck.GetGlobalRect())) return false;
            for (int previous = 0; previous < visibleOverlays.Count; previous++)
                if (RectsOverlap(bounds, visibleOverlays[previous])) return false;
            visibleOverlays.Add(bounds);
        }
        return true;
    }

    private static bool ContainsRect(Rect2 outer, Rect2 inner) =>
        inner.Position.X >= outer.Position.X - 0.5f && inner.Position.Y >= outer.Position.Y - 0.5f &&
        inner.End.X <= outer.End.X + 0.5f && inner.End.Y <= outer.End.Y + 0.5f;

    private static bool RectsOverlap(Rect2 a, Rect2 b) =>
        a.Position.X < b.End.X - 0.5f && a.End.X > b.Position.X + 0.5f &&
        a.Position.Y < b.End.Y - 0.5f && a.End.Y > b.Position.Y + 0.5f;

    private static bool ValidateProductionMinimapKnowledge()
    {
        SimulationWorld world = M5AcceptanceScenarioFactory.Create();
        EntityId linkEntity = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.TubeLink.Has(alive[i])) { linkEntity = alive[i]; break; }
        if (linkEntity == EntityId.None || !world.Entities.TubeLink.TryGet(linkEntity, out TubeLink link) ||
            world.GetTubeRoute(linkEntity) is not TubeRoute route) return false;

        world.Entities.Ownership.Get(linkEntity).PlayerSlot = 1;
        world.Entities.Ownership.Get(link.EndpointA).PlayerSlot = 1;
        world.Entities.Ownership.Get(link.EndpointB).PlayerSlot = 1;
        world.Fog.ClearCurrent();
        RevealTubeRoute(world, route, link);

        IReadOnlyList<EntityId> currentAlive = world.Entities.Alive;
        for (int i = 0; i < currentAlive.Count; i++)
            if (world.Entities.SurgeZone.TryGet(currentAlive[i], out SurgeZone zone))
            {
                zone.BuildupRemainingTicks = 0;
                zone.ActiveRemainingTicks = 60;
                world.Entities.SurgeZone.Set(currentAlive[i], zone);
                break;
            }

        MinimapPresentationSource source = new();
        HudMinimapFrame visible = source.Capture(world, PresentationSnapshot.Capture(world, 0), Array.Empty<EntityId>());
        bool discovered = visible.Lines.Any(lineFrame => lineFrame.Kind == HudMinimapLineKind.KnownEnemyNetwork) &&
            visible.Pings.Count > 0 && visible.ValidateClientKnowledge(0, out _);

        world.Fog.ClearCurrent();
        HudMinimapFrame remembered = source.Capture(world, PresentationSnapshot.Capture(world, 0), Array.Empty<EntityId>());
        bool retained = remembered.Lines.Any(lineFrame => lineFrame.Kind == HudMinimapLineKind.KnownEnemyNetwork) &&
            remembered.ValidateClientKnowledge(0, out _);

        world.Fog.ClearCurrent();
        RevealTubeRoute(world, route, link);
        if (!TubeGraphSystem.TryRemoveLink(world, 1, linkEntity)) return false;
        HudMinimapFrame disproved = source.Capture(world, PresentationSnapshot.Capture(world, 0), Array.Empty<EntityId>());
        bool removed = disproved.Lines.All(lineFrame => lineFrame.Kind != HudMinimapLineKind.KnownEnemyNetwork) &&
            disproved.ValidateClientKnowledge(0, out _);
        return discovered && retained && removed;
    }

    private static void RevealTubeRoute(SimulationWorld world, TubeRoute route, TubeLink link)
    {
        for (int i = 0; i < route.Cells.Count; i++) world.Fog.AddVisible(0, route.Cells[i].X, route.Cells[i].Y);
        if (world.Entities.Transform.TryGet(link.EndpointA, out SimTransform from))
            world.Fog.AddVisible(0, from.Position.X.FloorToInt(), from.Position.Y.FloorToInt());
        if (world.Entities.Transform.TryGet(link.EndpointB, out SimTransform to))
            world.Fog.AddVisible(0, to.Position.X.FloorToInt(), to.Position.Y.FloorToInt());
    }

    private void ApplySyntheticCameraPolygon()
    {
        Vector2 center = _labCameraCenter;
        _hud?.UpdateMinimapCamera(new[]
        {
            center + new Vector2(-28, -12), center + new Vector2(10, -29),
            center + new Vector2(29, 11), center + new Vector2(-11, 29)
        });
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null) return false;
        Error result = image.SavePng(path);
        if (result != Error.Ok) return false;
        GD.Print($"M7 HUD LAB CAPTURE: PASS scenario={M7HudFixtures.Slug(_scenario)} aspect={_aspect} path={path}");
        return true;
    }

    private void CopyProfile()
    {
        DisplayServer.ClipboardSet(_profile.ToJson());
        SetStatus($"Copied schema {M7HudProfile.CurrentSchemaVersion} HUD profile.");
    }

    private void PasteProfile()
    {
        if (!M7HudProfile.TryFromJson(DisplayServer.ClipboardGet(), out M7HudProfile pasted, out string error)) { SetStatus(error, true); return; }
        _profile = pasted;
        _previewFactionOverride = HudFactionSkinLibrary.RecipeFor(_profile.ArtSkin.Faction).Faction;
        ApplyAll();
        SetStatus("Pasted and applied HUD profile.");
    }

    private void ResetProfile()
    {
        _profile = M7HudProfile.CreateDefault();
        _previewFactionOverride = null;
        BindFactionToScenario(_scenario);
        ApplyAll();
        SetStatus("Reset to the scenario's faction-bound HUD kit.");
    }

    private void AddSlider(Container parent, string name, double min, double max, double step, Func<double> getter, Action<double> setter)
    {
        HBoxContainer row = new(); parent.AddChild(row);
        Label label = LabLabel(name, 12, new Color("d7dde0")); label.CustomMinimumSize = new Vector2(138, 0); row.AddChild(label);
        HSlider slider = new() { MinValue = min, MaxValue = max, Step = step, Value = getter(), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        Label valueLabel = LabLabel(slider.Value.ToString(step < 0.1 ? "0.00" : "0.#"), 11, new Color("9fb0b8")); valueLabel.CustomMinimumSize = new Vector2(48, 0);
        slider.ValueChanged += value => { setter(value); valueLabel.Text = value.ToString(step < 0.1 ? "0.00" : "0.#"); ApplyAll(); };
        row.AddChild(slider); row.AddChild(valueLabel);
    }

    private void AddToggle(Container parent, string name, Func<bool> getter, Action<bool> setter)
    {
        CheckButton toggle = new() { Text = name, ButtonPressed = getter() };
        toggle.Toggled += value => { setter(value); ApplyAll(); };
        parent.AddChild(toggle);
    }

    private void AddColor(Container parent, string name, Func<string> getter, Action<string> setter)
    {
        HBoxContainer row = new(); parent.AddChild(row);
        Label label = LabLabel(name, 12, new Color("d7dde0")); label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; row.AddChild(label);
        ColorPickerButton picker = new() { Color = Color.HtmlIsValid(getter()) ? new Color(getter()) : Colors.White, CustomMinimumSize = new Vector2(74, 28) };
        picker.ColorChanged += color => { setter($"#{color.ToHtml(false)}"); ApplyAll(); };
        row.AddChild(picker);
    }

    private void SetCustomColor(Action<string> setter, string value)
    {
        setter(value);
        _profile.ArtSkin.SurfacePalette = HudSurfacePalette.Custom;
    }

    private static void AddNote(Container parent, string text)
    {
        Label note = LabLabel(text, 11, new Color("9fb0b8"));
        note.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        parent.AddChild(note);
    }

    private static void AddSection(Container parent, string title)
    {
        HSeparator separator = new(); parent.AddChild(separator);
        parent.AddChild(LabLabel(title, 13, new Color("e6ad28")));
    }

    private static Button LabButton(string text) => new() { Text = text, CustomMinimumSize = new Vector2(0, 30) };
    private static Label LabLabel(string text, int size, Color color) { Label label = new() { Text = text }; label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", color); return label; }
    private static StyleBoxFlat PanelStyle(Color background, Color border) => new() { BgColor = new Color(background, 0.96f), BorderColor = border, BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1, CornerRadiusTopLeft = 7, CornerRadiusTopRight = 7, CornerRadiusBottomLeft = 7, CornerRadiusBottomRight = 7, ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 10, ContentMarginBottom = 10 };
    private void SetStatus(string text, bool error = false) { if (_status is null) return; _status.Text = text; _status.AddThemeColorOverride("font_color", error ? new Color("ff6b45") : new Color("65c987")); }
    private void Handled() => GetViewport().SetInputAsHandled();
    private static string ParseAspect(string value) => value.Trim().ToLowerInvariant() switch { "16:10" or "16-10" => "16:10", "21:9" or "21-9" => "21:9", "4:3" or "4-3" => "4:3", _ => "16:9" };
    private static float RelativeLuminance(Color color) =>
        color.R * 0.2126f + color.G * 0.7152f + color.B * 0.0722f;

    private static HudArtFinish ParseFinish(string value) => value.Trim().ToLowerInvariant() switch
    {
        "hybrid" or "hybrid-console" => HudArtFinish.HybridConsole,
        "structural" or "structural-console" or "vector" => HudArtFinish.StructuralConsole,
        "legacy" or "legacy-frames" or "frames" => HudArtFinish.LegacyFrames,
        "clean" or "none" => HudArtFinish.Clean,
        _ => HudArtFinish.HybridConsole
    };

    private static HudSurfacePalette ParsePalette(string value) => value.Trim().ToLowerInvariant() switch
    {
        "sand" or "sandstone" or "warm-sandstone" => HudSurfacePalette.WarmSandstone,
        "oxide" or "oxide-workshop" => HudSurfacePalette.OxideWorkshop,
        "olive" or "field-olive" => HudSurfacePalette.FieldOlive,
        "alien" or "alien-porcelain" => HudSurfacePalette.AlienPorcelain,
        "graphite" or "neutral-graphite" => HudSurfacePalette.NeutralGraphite,
        "custom" => HudSurfacePalette.Custom,
        "faction" or "faction-bound" or "auto" or "canonical" => HudSurfacePalette.FactionBound,
        _ => HudSurfacePalette.FactionBound
    };

    private static HudFaction? ParseFactionKit(string value) => value.Trim().ToLowerInvariant() switch
    {
        "rock" or "rock-raiders" or "rockraiders" => HudFaction.RockRaiders,
        "astronaut" or "astronauts" => HudFaction.Astronauts,
        "alien" or "aliens" => HudFaction.Aliens,
        "martian" or "martians" => HudFaction.Martians,
        _ => null
    };

    private void AddBuilding(Vector3 position, Vector3 size, Color color, string name)
    {
        MeshInstance3D building = new() { Name = name, Position = position, Mesh = new BoxMesh { Size = size }, MaterialOverride = Material(color, 0.55f) };
        AddChild(building);
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            MeshInstance3D stud = new() { Position = new Vector3(x * size.X * 0.24f, size.Y * 0.52f, z * size.Z * 0.24f), Mesh = new CylinderMesh { TopRadius = 0.34f, BottomRadius = 0.34f, Height = 0.18f }, MaterialOverride = Material(color.Lightened(0.07f), 0.42f) };
            building.AddChild(stud);
        }
    }

    private void AddUnit(Vector3 position, Color color, int index)
    {
        Node3D rig = new() { Name = $"BackdropUnit{index}", Position = position, RotationDegrees = new Vector3(0f, index * 37f, 0f) };
        AddChild(rig);
        MeshInstance3D body = new() { Mesh = new BoxMesh { Size = new Vector3(2.2f, 0.8f, 3.0f) }, MaterialOverride = Material(color, 0.48f) }; rig.AddChild(body);
        MeshInstance3D cabin = new() { Position = new Vector3(0f, 0.65f, -0.2f), Mesh = new BoxMesh { Size = new Vector3(1.35f, 0.7f, 1.45f) }, MaterialOverride = Material(color.Lightened(0.12f), 0.35f) }; rig.AddChild(cabin);
        for (int side = -1; side <= 1; side += 2)
        for (int axle = -1; axle <= 1; axle += 2)
        {
            MeshInstance3D wheel = new() { Position = new Vector3(side * 1.18f, -0.12f, axle * 0.92f), RotationDegrees = new Vector3(0f, 0f, 90f), Mesh = new CylinderMesh { TopRadius = 0.48f, BottomRadius = 0.48f, Height = 0.32f }, MaterialOverride = Material(new Color("111416"), 0.92f) };
            rig.AddChild(wheel);
        }
    }

    private void AddSelectionRing(Vector3 position)
    {
        MeshInstance3D ring = new() { Position = new Vector3(position.X, 0.035f, position.Z), Mesh = new TorusMesh { InnerRadius = 1.6f, OuterRadius = 1.72f, Rings = 24, RingSegments = 8 }, MaterialOverride = Material(new Color("5fc4d8"), 0.3f) };
        AddChild(ring);
    }

    private static StandardMaterial3D Material(Color color, float roughness) => new() { AlbedoColor = color, Roughness = roughness, Metallic = roughness < 0.5f ? 0.18f : 0f };
}
