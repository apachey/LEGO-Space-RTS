using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class M7LookLab : Node3D
{
    private const string ModelPath = "res://Assets/M7/raider_drill_rig.glb";
    private static readonly string[] Categories =
    {
        "SCENE & CAMERA", "SHADING", "MATERIALS", "GLASS & EMISSION", "LIGHTING", "POST FX", "OUTLINE", "VFX", "GROUND", "HUD"
    };

    private readonly List<Node3D> _units = new();
    private readonly List<Node3D> _drills = new();
    private readonly List<MeshInstance3D> _unitMeshes = new();
    private readonly List<(MeshInstance3D Mesh, M7LookMaterialRole Role)> _roleMeshes = new();
    private readonly List<MeshInstance3D> _fireFlames = new();
    private readonly List<MeshInstance3D> _smokePuffs = new();
    private readonly List<MeshInstance3D> _sparks = new();
    private readonly List<(Control Root, ColorRect Fill, Vector3 World, float Health)> _healthBars = new();
    private readonly List<(MeshInstance3D Background, MeshInstance3D Fill, float Health)> _worldHealthBars = new();
    private readonly List<(Label3D Label, float Health, bool Selected)> _worldHealthLabels = new();
    private readonly List<MeshInstance3D> _tracks = new();
    private Action? _returnToPrototype;
    private M7LookProfile _profile = M7LookProfile.CreateDefault();
    private M7LookProfile _defaults = M7LookProfile.CreateDefault();
    private Camera3D? _camera;
    private DirectionalLight3D? _keyLight;
    private DirectionalLight3D? _fillLight;
    private DirectionalLight3D? _rimLight;
    private Godot.Environment? _environment;
    private MeshInstance3D? _ground;
    private MeshInstance3D? _fogPreview;
    private MeshInstance3D? _selectionRing;
    private MeshInstance3D? _targetMarker;
    private MeshInstance3D? _tracer;
    private MeshInstance3D? _muzzle;
    private MeshInstance3D? _impact;
    private MeshInstance3D? _scorch;
    private MeshInstance3D? _postQuad;
    private ShaderMaterial? _postMaterial;
    private CanvasLayer? _fakeHudLayer;
    private Control? _fakeHudRoot;
    private ColorRect? _minimapGround;
    private CanvasLayer? _controlsLayer;
    private PanelContainer? _controlPanel;
    private VBoxContainer? _settingsBox;
    private Label? _statusLabel;
    private Button? _pauseButton;
    private int _category;
    private int _materialFamily;
    private bool _controlsVisible = true;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private double _time;
    private string? _capturePath;
    private Vector3 _shooterMuzzle = new(2.4f, 1.45f, 4.8f);
    private Vector3 _targetPoint = new(11.4f, 1.6f, -6.6f);

    public void Configure(Action returnToPrototype, string[] commandLineArgs)
    {
        Name = "M7LookLab";
        _returnToPrototype = returnToPrototype;
        _smoke = commandLineArgs.Contains("--m7-look-smoke");
        _controlsVisible = ParseString(commandLineArgs, "--m7-look-controls") != "hidden";
        _capturePath = ParseString(commandLineArgs, "--capture-path");
        if (float.TryParse(ParseString(commandLineArgs, "--m7-look-zoom"), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float zoom))
            _profile.Camera.ZoomCells = zoom;
        if (ParseString(commandLineArgs, "--m7-look-post") == "off") _profile.Post.Enabled = false;
        if (ParseString(commandLineArgs, "--m7-look-outline") == "on") _profile.Outline.Enabled = true;
        _profile.Normalize();
        _defaults = _profile.Clone();

        BuildScene();
        BuildFakeHud();
        BuildControls();
        ApplyProfile(rebuildMaterials: true);
        ProcessPriority = 1000;
        GD.Print($"M7 LOOK LAB: active schema={_profile.SchemaVersion} zoom={_profile.Camera.ZoomCells:0.##} controls={(_controlsVisible ? "visible" : "hidden")} Tab=controls wheel=zoom Escape=return");
    }

    public override void _Process(double delta)
    {
        if (!_profile.Scene.Paused) _time += delta * _profile.Scene.AnimationSpeed;
        AnimateScene();
        UpdateWorldHud();

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < 24) return;
        bool valid = ValidateLab(out int triangles);
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 LOOK LAB: PASS schema={_profile.SchemaVersion} units={_units.Count} meshes={_unitMeshes.Count} triangles={triangles} buildings=2 firing=1 burning=1 controls={(_controlsVisible ? "visible" : "hidden")} zoom={_profile.Camera.ZoomCells:0.##}");
        else
            GD.PrintErr($"M7 LOOK LAB: FAIL units={_units.Count} meshes={_unitMeshes.Count} triangles={triangles}");
        GetTree().Quit(valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mouse)
        {
            if (mouse.ButtonIndex == MouseButton.WheelUp) AdjustZoom(0.92f);
            if (mouse.ButtonIndex == MouseButton.WheelDown) AdjustZoom(1.08f);
        }
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (key.Keycode == Key.Tab)
        {
            SetControlsVisible(!_controlsVisible);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Escape)
        {
            _returnToPrototype?.Invoke();
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildScene()
    {
        _camera = new Camera3D { Name = "LookLabCamera", Current = true, Fov = 36f, Near = 0.2f, Far = 400f };
        AddChild(_camera);

        _keyLight = new DirectionalLight3D { Name = "KeyLight", ShadowEnabled = true, DirectionalShadowMaxDistance = 180f };
        _fillLight = new DirectionalLight3D { Name = "FillLight", ShadowEnabled = false };
        _rimLight = new DirectionalLight3D { Name = "RimLight", ShadowEnabled = false };
        AddChild(_keyLight); AddChild(_fillLight); AddChild(_rimLight);

        _environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            GlowEnabled = true,
            AdjustmentEnabled = false
        };
        AddChild(new WorldEnvironment { Name = "LookEnvironment", Environment = _environment });

        _ground = new MeshInstance3D
        {
            Name = "Ground",
            Mesh = BuildGroundMesh(48, 52f),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_ground);
        _roleMeshes.Add((_ground, M7LookMaterialRole.GroundRock));

        float spacing = _profile.Ground.UnitSeparation;
        CreateUnit("Shooter", new Vector3(0.35f * spacing, 0.08f, 0.82f * spacing), _targetPoint, true);
        CreateUnit("FacingNorthWest", new Vector3(-0.92f * spacing, 0.08f, 0.34f * spacing), 135f);
        CreateUnit("FacingSouthWest", new Vector3(0.88f * spacing, 0.08f, -0.25f * spacing), 225f);
        CreateUnit("FacingSouthEast", new Vector3(-0.34f * spacing, 0.08f, -0.86f * spacing), 315f);

        Node3D intact = BuildBuilding("IntactBuilding", new Vector3(11.4f, 0f, -6.6f), new Vector3(6.4f, 3.2f, 5.0f), false);
        Node3D burning = BuildBuilding("BurningBuilding", new Vector3(-10.5f, 0f, -8.7f), new Vector3(4.1f, 2.2f, 3.8f), true);
        AddChild(intact); AddChild(burning);
        _targetPoint = intact.GlobalPosition + new Vector3(0f, 1.55f, 0f);

        BuildGroundDetails();
        BuildCrystalCluster(new Vector3(10.2f, 0f, 6.8f));
        BuildCombatEffects();
        BuildBurningEffects(burning.GlobalPosition + new Vector3(0f, 2.1f, 0f));
        BuildPostProcess();
    }

    private void CreateUnit(string name, Vector3 position, float yawDegrees)
    {
        PackedScene packed = GD.Load<PackedScene>(ModelPath) ?? throw new InvalidOperationException($"Unable to load M7 look model: {ModelPath}");
        Node3D unit = packed.Instantiate<Node3D>();
        unit.Name = name;
        unit.Position = position;
        unit.RotationDegrees = new Vector3(0f, yawDegrees, 0f);
        AddChild(unit);
        RegisterUnit(unit);
    }

    private void CreateUnit(string name, Vector3 position, Vector3 target, bool shooter)
    {
        CreateUnit(name, position, 0f);
        Node3D unit = _units[^1];
        unit.LookAt(new Vector3(target.X, unit.GlobalPosition.Y, target.Z), Vector3.Up);
        if (shooter)
            _shooterMuzzle = unit.GlobalPosition + unit.GlobalBasis * new Vector3(0.85f, 1.38f, -3.35f);
    }

    private void RegisterUnit(Node3D unit)
    {
        _units.Add(unit);
        Node? drill = unit.FindChild("Pivot_Drill", true, false);
        if (drill is Node3D drill3D) _drills.Add(drill3D);
        CollectRoleMeshes(unit, unitMesh: true);
    }

    private void CollectRoleMeshes(Node root, bool unitMesh)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                M7LookMaterialRole role = M7LookMaterialFactory.InferRole(mesh.Name);
                _roleMeshes.Add((mesh, role));
                if (unitMesh) _unitMeshes.Add(mesh);
            }
            CollectRoleMeshes(child, unitMesh);
        }
    }

    private Node3D BuildBuilding(string name, Vector3 position, Vector3 size, bool damaged)
    {
        Node3D root = new() { Name = name, Position = position };
        AddBox(root, "Shell", new Vector3(0f, size.Y * 0.5f, 0f), size, M7LookMaterialRole.BuildingShell);
        AddBox(root, "DarkFoundation", new Vector3(0f, 0.22f, 0f), new Vector3(size.X + 0.8f, 0.44f, size.Z + 0.8f), M7LookMaterialRole.DarkMechanic);
        AddBox(root, "AccentBand", new Vector3(0f, size.Y * 0.68f, size.Z * 0.505f), new Vector3(size.X * 0.72f, 0.34f, 0.12f), M7LookMaterialRole.Accent);
        AddBox(root, "Door", new Vector3(0f, size.Y * 0.38f, size.Z * 0.515f), new Vector3(size.X * 0.34f, size.Y * 0.58f, 0.16f), M7LookMaterialRole.DarkMechanic);
        AddBox(root, "RoofCap", new Vector3(0f, size.Y + 0.22f, 0f), new Vector3(size.X * 0.76f, 0.44f, size.Z * 0.74f), damaged ? M7LookMaterialRole.StructuralEarth : M7LookMaterialRole.PaintedHull);
        int studsX = damaged ? 3 : 5;
        for (int i = 0; i < studsX; i++)
        {
            float x = Mathf.Lerp(-size.X * 0.30f, size.X * 0.30f, studsX == 1 ? 0.5f : i / (float)(studsX - 1));
            CylinderMesh studMesh = new() { TopRadius = 0.30f, BottomRadius = 0.30f, Height = 0.22f, RadialSegments = 16 };
            AddPrimitive(root, $"RoofStud_{i}", studMesh, new Vector3(x, size.Y + 0.53f, 0f), M7LookMaterialRole.PaintedHull);
        }
        if (!damaged)
        {
            AddPrimitive(root, "SignalLamp", new SphereMesh { Radius = 0.24f, Height = 0.48f, RadialSegments = 16, Rings = 8 },
                new Vector3(size.X * 0.34f, size.Y + 0.72f, 0f), M7LookMaterialRole.Signal);
            AddBox(root, "Vent", new Vector3(-size.X * 0.28f, size.Y + 0.55f, 0f), new Vector3(0.9f, 0.7f, 1.4f), M7LookMaterialRole.ToolSteel);
        }
        return root;
    }

    private void AddBox(Node3D parent, string name, Vector3 position, Vector3 size, M7LookMaterialRole role)
        => AddPrimitive(parent, name, new BoxMesh { Size = size }, position, role);

    private void AddPrimitive(Node3D parent, string name, PrimitiveMesh mesh, Vector3 position, M7LookMaterialRole role)
    {
        MeshInstance3D instance = new() { Name = name, Mesh = mesh, Position = position };
        parent.AddChild(instance);
        _roleMeshes.Add((instance, role));
    }

    private void BuildGroundDetails()
    {
        for (int i = 0; i < 6; i++)
        {
            MeshInstance3D track = new()
            {
                Name = $"TrackMark_{i}",
                Mesh = new PlaneMesh { Size = new Vector2(0.52f, 5.2f) },
                Position = new Vector3(-6.8f + i * 0.44f, 0.045f, 5.4f),
                RotationDegrees = new Vector3(0f, 28f, 0f),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(track); _tracks.Add(track);
        }
        _fogPreview = new MeshInstance3D
        {
            Name = "FogOfWarPreview",
            Mesh = new PlaneMesh { Size = new Vector2(24f, 18f) },
            Position = new Vector3(-17f, 0.07f, -14f),
            MaterialOverride = FogPreviewMaterial(),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_fogPreview);
        _scorch = new MeshInstance3D
        {
            Name = "ScorchMark",
            Mesh = new PlaneMesh { Size = new Vector2(2.1f, 2.1f) },
            Position = new Vector3(-10.5f, 0.06f, -8.7f),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_scorch);
    }

    private void BuildCrystalCluster(Vector3 origin)
    {
        Node3D cluster = new() { Name = "ResourceCrystalCluster", Position = origin };
        AddChild(cluster);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.Tau / 6f;
            BoxMesh shard = new() { Size = new Vector3(0.34f, 1.3f + (i % 3) * 0.36f, 0.34f) };
            MeshInstance3D mesh = new()
            {
                Name = $"Crystal_{i}", Mesh = shard,
                Position = new Vector3(Mathf.Cos(angle) * 0.65f, shard.Size.Y * 0.5f, Mathf.Sin(angle) * 0.65f),
                RotationDegrees = new Vector3(i % 2 == 0 ? 12f : -9f, Mathf.RadToDeg(angle), i % 3 * 7f)
            };
            cluster.AddChild(mesh); _roleMeshes.Add((mesh, M7LookMaterialRole.Crystal));
        }
    }

    private void BuildCombatEffects()
    {
        _tracer = new MeshInstance3D { Name = "WeaponTracer", Mesh = new BoxMesh { Size = new Vector3(0.1f, 0.1f, 1.5f) }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
        _muzzle = new MeshInstance3D { Name = "MuzzleFlash", Mesh = new SphereMesh { Radius = 0.38f, Height = 0.76f, RadialSegments = 12, Rings = 6 }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
        _impact = new MeshInstance3D { Name = "ImpactFlash", Mesh = new SphereMesh { Radius = 0.56f, Height = 1.12f, RadialSegments = 12, Rings = 6 }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
        AddChild(_tracer); AddChild(_muzzle); AddChild(_impact);
        for (int i = 0; i < 12; i++)
        {
            MeshInstance3D spark = new()
            {
                Name = $"ImpactSpark_{i}",
                Mesh = new BoxMesh { Size = new Vector3(0.045f, 0.045f, 0.38f) },
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(spark); _sparks.Add(spark);
        }
    }

    private void BuildBurningEffects(Vector3 origin)
    {
        for (int i = 0; i < 7; i++)
        {
            MeshInstance3D flame = new()
            {
                Name = $"FireFlame_{i}",
                Mesh = new SphereMesh { Radius = 0.35f, Height = 0.9f, RadialSegments = 10, Rings = 5 },
                Position = origin + new Vector3((i - 3) * 0.26f, 0f, (i % 2) * 0.24f),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(flame); _fireFlames.Add(flame);
        }
        for (int i = 0; i < 14; i++)
        {
            MeshInstance3D puff = new()
            {
                Name = $"FireSmoke_{i}",
                Mesh = new SphereMesh { Radius = 0.42f, Height = 0.74f, RadialSegments = 8, Rings = 4 },
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(puff); _smokePuffs.Add(puff);
        }
    }

    private void BuildPostProcess()
    {
        _postMaterial = new ShaderMaterial { Shader = new Shader { Code = PostShader }, RenderPriority = -128 };
        _postQuad = new MeshInstance3D
        {
            Name = "ScreenSpaceLookPass",
            Mesh = new QuadMesh { Size = new Vector2(2f, 2f), FlipFaces = true },
            MaterialOverride = _postMaterial,
            ExtraCullMargin = 16384f,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_postQuad);
    }

    private void BuildFakeHud()
    {
        _fakeHudLayer = new CanvasLayer { Name = "FakeGameplayHUD", Layer = 10 };
        AddChild(_fakeHudLayer);
        Control root = new() { Name = "HudRoot", MouseFilter = Control.MouseFilterEnum.Ignore };
        _fakeHudLayer.AddChild(root);
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _fakeHudRoot = root;

        Panel minimap = new() { Name = "Minimap", AnchorTop = 1f, AnchorBottom = 1f, OffsetLeft = 18f, OffsetTop = -208f, OffsetRight = 208f, OffsetBottom = -18f };
        minimap.AddThemeStyleboxOverride("panel", FlatPanel(new Color("17212a"), 0.92f, new Color("7c8d98")));
        root.AddChild(minimap);
        _minimapGround = new ColorRect { Position = new Vector2(10f, 10f), Size = new Vector2(170f, 170f), Color = new Color("34423f") };
        minimap.AddChild(_minimapGround);
        for (int i = 0; i < 7; i++)
        {
            ColorRect dot = new() { Position = new Vector2(24 + i * 17, 54 + (i % 3) * 22), Size = new Vector2(5, 5), Color = i < 4 ? new Color("e6a81f") : new Color("dd594b") };
            minimap.AddChild(dot);
        }

        Panel bottom = new() { Name = "SelectionPanel", AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 1f, AnchorBottom = 1f, OffsetLeft = -310f, OffsetRight = 310f, OffsetTop = -126f, OffsetBottom = -18f };
        bottom.AddThemeStyleboxOverride("panel", FlatPanel(new Color("18232d"), 0.92f, new Color("8799a5")));
        root.AddChild(bottom);
        Label selection = new() { Text = "RAIDER DRILL RIG     742 / 800\nHEAVY EXCAVATION VEHICLE\n● SELECTED    ◇ ATTACKING STRUCTURE", Position = new Vector2(22f, 18f), Size = new Vector2(570f, 76f) };
        selection.AddThemeColorOverride("font_color", new Color("dce7ea")); selection.AddThemeFontSizeOverride("font_size", 16); bottom.AddChild(selection);

        Panel resources = new() { AnchorLeft = 1f, AnchorRight = 1f, OffsetLeft = -390f, OffsetRight = -18f, OffsetTop = 18f, OffsetBottom = 64f };
        resources.AddThemeStyleboxOverride("panel", FlatPanel(new Color("15202a"), 0.88f, new Color("657986")));
        root.AddChild(resources);
        Label resourceText = new() { Text = "CRYSTALS  1 240      ENERGY  680      CAP  38/64", Position = new Vector2(14f, 11f), Size = new Vector2(345f, 24f) };
        resourceText.AddThemeColorOverride("font_color", new Color("e3eef0")); resources.AddChild(resourceText);

        Vector3[] anchors = _units.Select(unit => unit.GlobalPosition + Vector3.Up * 3.3f).ToArray();
        float[] health = { 0.93f, 0.68f, 1f, 0.46f };
        for (int i = 0; i < anchors.Length; i++)
        {
            CreateHealthBar(root, anchors[i], health[i], i == 0);
            CreateWorldHealthBar(anchors[i], health[i], i == 0);
        }

        _selectionRing = new MeshInstance3D
        {
            Name = "SelectionRing",
            Mesh = new TorusMesh { InnerRadius = 2.65f, OuterRadius = 2.78f, Rings = 32, RingSegments = 8 },
            Position = _units[0].GlobalPosition + Vector3.Up * 0.07f,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_selectionRing);
        _targetMarker = new MeshInstance3D
        {
            Name = "TargetMarker",
            Mesh = new TorusMesh { InnerRadius = 1.05f, OuterRadius = 1.16f, Rings = 24, RingSegments = 6 },
            Position = new Vector3(_targetPoint.X, 0.09f, _targetPoint.Z),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_targetMarker);
    }

    private void CreateHealthBar(Control root, Vector3 world, float health, bool selected)
    {
        Control bar = new() { CustomMinimumSize = new Vector2(58f, 9f), Size = new Vector2(58f, 9f), MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 20 };
        ColorRect background = new() { Color = new Color(0.02f, 0.03f, 0.035f, 0.88f), Size = new Vector2(58f, 9f), MouseFilter = Control.MouseFilterEnum.Ignore };
        ColorRect fill = new() { Color = selected ? new Color("5ddd7b") : health < 0.55f ? new Color("e6783d") : new Color("8ed957"), Position = new Vector2(2f, 2f), Size = new Vector2(54f * health, 5f), MouseFilter = Control.MouseFilterEnum.Ignore };
        bar.AddChild(background); bar.AddChild(fill);
        if (_fakeHudLayer is not null) _fakeHudLayer.AddChild(bar); else root.AddChild(bar);
        _healthBars.Add((bar, fill, world, health));
    }

    private void CreateWorldHealthBar(Vector3 world, float health, bool selected)
    {
        MeshInstance3D background = new()
        {
            Name = "WorldHealthBackground",
            Mesh = new QuadMesh { Size = new Vector2(1.55f, 0.18f) },
            Position = world,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        MeshInstance3D fill = new()
        {
            Name = "WorldHealthFill",
            Mesh = new QuadMesh { Size = new Vector2(1.46f * health, 0.11f) },
            Position = world + Vector3.Up * 0.003f,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        background.MaterialOverride = BillboardMaterial(new Color(0.015f, 0.02f, 0.025f, 0.88f));
        fill.MaterialOverride = BillboardMaterial(selected ? new Color("5ddd7b") : health < 0.55f ? new Color("e6783d") : new Color("8ed957"));
        AddChild(background); AddChild(fill);
        _worldHealthBars.Add((background, fill, health));
        int filled = Mathf.Clamp(Mathf.RoundToInt(health * 8f), 1, 8);
        Label3D label = new()
        {
            Name = "HealthReadabilityLabel",
            Text = new string('█', filled) + new string('░', 8 - filled),
            Position = world + Vector3.Up * 0.18f,
            FontSize = 30,
            PixelSize = 0.010f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true,
            Shaded = false,
            DoubleSided = true,
            OutlineSize = 7,
            OutlineModulate = new Color(0.01f, 0.015f, 0.02f, 0.96f),
            Modulate = selected ? new Color("5ddd7b") : health < 0.55f ? new Color("e6783d") : new Color("8ed957"),
            RenderPriority = 126,
            OutlineRenderPriority = 125
        };
        AddChild(label);
        _worldHealthLabels.Add((label, health, selected));
    }

    private void BuildControls()
    {
        _controlsLayer = new CanvasLayer { Name = "LookLabControls", Layer = 50, Visible = _controlsVisible };
        AddChild(_controlsLayer);
        _controlPanel = new PanelContainer { Name = "LookPanel", AnchorBottom = 1f, OffsetRight = 438f, OffsetBottom = 0f };
        _controlPanel.AddThemeStyleboxOverride("panel", FlatPanel(new Color("10171d"), 0.96f, new Color("52636e")));
        _controlsLayer.AddChild(_controlPanel);
        VBoxContainer outer = new();
        outer.AddThemeConstantOverride("separation", 8);
        _controlPanel.AddChild(outer);
        Label title = new() { Text = "M7 LOOK LAB  ·  NON-CANON", CustomMinimumSize = new Vector2(0f, 34f), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 18); title.AddThemeColorOverride("font_color", new Color("e8b640")); outer.AddChild(title);

        HFlowContainer actions = new(); outer.AddChild(actions);
        AddAction(actions, "COPY ALL JSON", CopyProfile, "Copies every current look parameter, including zoom.");
        AddAction(actions, "PASTE & APPLY", PasteProfile, "Applies a complete versioned M7 Look profile from the clipboard.");
        AddAction(actions, "RESET SECTION", ResetCurrentSection);
        AddAction(actions, "RESET ALL", ResetAll);
        _pauseButton = AddAction(actions, "PAUSE", TogglePause);
        AddAction(actions, "HIDE · TAB", () => SetControlsVisible(false));

        OptionButton category = new() { Name = "Category", CustomMinimumSize = new Vector2(0f, 36f) };
        foreach (string item in Categories) category.AddItem(item);
        category.Selected = _category;
        category.ItemSelected += index => { _category = (int)index; BuildCurrentSettings(); };
        outer.AddChild(category);

        ScrollContainer scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        outer.AddChild(scroll);
        _settingsBox = new VBoxContainer { Name = "Settings", CustomMinimumSize = new Vector2(406f, 0f) };
        scroll.AddChild(_settingsBox);
        _statusLabel = new Label { Text = "Ready — change any value live", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0f, 38f) };
        _statusLabel.AddThemeColorOverride("font_color", new Color("9fb2bc")); outer.AddChild(_statusLabel);
        BuildCurrentSettings();
    }

    private void BuildCurrentSettings()
    {
        if (_settingsBox is null) return;
        foreach (Node child in _settingsBox.GetChildren()) { _settingsBox.RemoveChild(child); child.QueueFree(); }
        switch (_category)
        {
            case 0: BuildSceneSettings(); break;
            case 1: BuildShadingSettings(); break;
            case 2: BuildMaterialSettings(); break;
            case 3: BuildGlassEmissionSettings(); break;
            case 4: BuildLightingSettings(); break;
            case 5: BuildPostSettings(); break;
            case 6: BuildOutlineSettings(); break;
            case 7: BuildVfxSettings(); break;
            case 8: BuildGroundSettings(); break;
            case 9: BuildHudSettings(); break;
        }
    }

    private void BuildSceneSettings()
    {
        AddNote("Actual gameplay camera: 36° FOV, canonical 24–72 build-cell zoom. Mouse wheel works over the scene.");
        AddSlider("Zoom · build cells", 24, 72, 0.5, () => _profile.Camera.ZoomCells, v => _profile.Camera.ZoomCells = v, false);
        AddSlider("Pitch · degrees", 52, 64, 0.5, () => _profile.Camera.PitchDegrees, v => _profile.Camera.PitchDegrees = v, false);
        AddOption("Yaw", new[] { "45°", "135°", "225°", "315°" }, YawIndex(), index => _profile.Camera.YawDegrees = 45f + index * 90f, false);
        AddSlider("Animation speed", 0, 2, 0.05, () => _profile.Scene.AnimationSpeed, v => _profile.Scene.AnimationSpeed = v, false);
        AddCheck("Weapon firing", () => _profile.Scene.FiringEnabled, v => _profile.Scene.FiringEnabled = v, false);
        AddCheck("Burning building", () => _profile.Scene.BurningEnabled, v => _profile.Scene.BurningEnabled = v, false);
        AddCheck("Dust / smoke", () => _profile.Scene.DustEnabled, v => _profile.Scene.DustEnabled = v, false);
        AddCheck("Fog-of-war preview", () => _profile.Scene.FogPreviewEnabled, v => _profile.Scene.FogPreviewEnabled = v, false);
        AddCheck("Fake gameplay HUD", () => _profile.Scene.HudEnabled, v => _profile.Scene.HudEnabled = v, false);
        AddCheck("Selection language", () => _profile.Scene.SelectionEnabled, v => _profile.Scene.SelectionEnabled = v);
    }

    private void BuildShadingSettings()
    {
        AddSlider("Diffuse wrap", 0, 1, 0.01, () => _profile.Shading.DiffuseWrap, v => _profile.Shading.DiffuseWrap = v);
        AddSlider("Shadow floor", 0, 1, 0.01, () => _profile.Shading.ShadowFloor, v => _profile.Shading.ShadowFloor = v);
        AddSlider("Light bands · 0=smooth", 0, 8, 1, () => _profile.Shading.LightBands, v => _profile.Shading.LightBands = (int)v);
        AddSlider("Band softness", 0, 1, 0.01, () => _profile.Shading.BandSoftness, v => _profile.Shading.BandSoftness = v);
        AddSlider("Global specular", 0, 2, 0.01, () => _profile.Shading.GlobalSpecular, v => _profile.Shading.GlobalSpecular = v);
        AddSlider("Material rim", 0, 2, 0.01, () => _profile.Shading.RimStrength, v => _profile.Shading.RimStrength = v);
        AddSlider("Rim width", 0.05, 1, 0.01, () => _profile.Shading.RimWidth, v => _profile.Shading.RimWidth = v);
        AddColor("Shadow tint", () => _profile.Shading.ShadowTint, v => _profile.Shading.ShadowTint = v);
        AddColor("Highlight tint", () => _profile.Shading.HighlightTint, v => _profile.Shading.HighlightTint = v);
    }

    private void BuildMaterialSettings()
    {
        string[] families = { "Painted hull", "Structural earth", "Accent", "Dark mechanic", "Tool steel", "Rubber", "Building shell", "Ground rock" };
        AddOption("Material family", families, _materialFamily, index => { _materialFamily = index; BuildCurrentSettings(); }, rebuildMaterials: false, applyProfile: false);
        MaterialLook look = SelectedMaterial();
        AddColor("Base color", () => look.BaseColor, v => look.BaseColor = v);
        AddSlider("Metallic", 0, 1, 0.01, () => look.Metallic, v => look.Metallic = v);
        AddSlider("Roughness", 0, 1, 0.01, () => look.Roughness, v => look.Roughness = v);
        AddSlider("Specular", 0, 1, 0.01, () => look.Specular, v => look.Specular = v);
        AddSlider("Clearcoat", 0, 1, 0.01, () => look.Clearcoat, v => look.Clearcoat = v);
        AddSlider("Clearcoat roughness", 0, 1, 0.01, () => look.ClearcoatRoughness, v => look.ClearcoatRoughness = v);
        AddSlider("Fresnel character", 0, 1, 0.01, () => look.Fresnel, v => look.Fresnel = v);
        AddSlider("Micro variation", 0, 1, 0.01, () => look.MicroStrength, v => look.MicroStrength = v);
        AddSlider("Micro scale", 0.1, 20, 0.1, () => look.MicroScale, v => look.MicroScale = v);
        AddSlider("Macro variation", 0, 1, 0.01, () => look.MacroVariation, v => look.MacroVariation = v);
        AddSlider("Edge wear", 0, 1, 0.01, () => look.EdgeWear, v => look.EdgeWear = v);
        AddSlider("Dust amount", 0, 1, 0.01, () => look.DustAmount, v => look.DustAmount = v);
        AddSlider("Brushed directionality", 0, 1, 0.01, () => look.BrushedAmount, v => look.BrushedAmount = v);
    }

    private void BuildGlassEmissionSettings()
    {
        AddHeading("NON-EMISSIVE GLASS");
        AddColor("Glass tint", () => _profile.Glass.Tint, v => _profile.Glass.Tint = v);
        AddSlider("Opacity", 0.05, 1, 0.01, () => _profile.Glass.Opacity, v => _profile.Glass.Opacity = v);
        AddSlider("Roughness", 0, 1, 0.01, () => _profile.Glass.Roughness, v => _profile.Glass.Roughness = v);
        AddSlider("IOR", 1, 2.5, 0.01, () => _profile.Glass.Ior, v => _profile.Glass.Ior = v);
        AddSlider("Refraction", 0, 1, 0.01, () => _profile.Glass.RefractionStrength, v => _profile.Glass.RefractionStrength = v);
        AddSlider("Edge brightness", 0, 2, 0.01, () => _profile.Glass.EdgeBrightness, v => _profile.Glass.EdgeBrightness = v);
        AddHeading("ACTUALLY EMISSIVE FUNCTIONS");
        AddColor("Signal color", () => _profile.Emission.SignalColor, v => _profile.Emission.SignalColor = v);
        AddSlider("Signal energy", 0, 12, 0.1, () => _profile.Emission.SignalEnergy, v => _profile.Emission.SignalEnergy = v);
        AddColor("Lamp color", () => _profile.Emission.LampColor, v => _profile.Emission.LampColor = v);
        AddSlider("Lamp energy", 0, 12, 0.1, () => _profile.Emission.LampEnergy, v => _profile.Emission.LampEnergy = v);
        AddColor("Crystal color", () => _profile.Emission.CrystalColor, v => _profile.Emission.CrystalColor = v);
        AddSlider("Crystal energy", 0, 12, 0.1, () => _profile.Emission.CrystalEnergy, v => _profile.Emission.CrystalEnergy = v);
        AddSlider("Emission pulse", 0, 1, 0.01, () => _profile.Emission.PulseAmount, v => _profile.Emission.PulseAmount = v);
        AddSlider("Pulse speed", 0, 8, 0.1, () => _profile.Emission.PulseSpeed, v => _profile.Emission.PulseSpeed = v);
    }

    private void BuildLightingSettings()
    {
        AddHeading("KEY / SHADOW");
        AddSlider("Key azimuth", 0, 359, 1, () => _profile.Lighting.KeyAzimuth, v => _profile.Lighting.KeyAzimuth = v);
        AddSlider("Key elevation", 10, 85, 1, () => _profile.Lighting.KeyElevation, v => _profile.Lighting.KeyElevation = v);
        AddColor("Key color", () => _profile.Lighting.KeyColor, v => _profile.Lighting.KeyColor = v);
        AddSlider("Key energy", 0, 8, 0.05, () => _profile.Lighting.KeyEnergy, v => _profile.Lighting.KeyEnergy = v, false);
        AddSlider("Angular size", 0, 10, 0.1, () => _profile.Lighting.KeyAngularSize, v => _profile.Lighting.KeyAngularSize = v, false);
        AddSlider("Shadow blur", 0, 8, 0.1, () => _profile.Lighting.ShadowBlur, v => _profile.Lighting.ShadowBlur = v, false);
        AddSlider("Shadow opacity", 0, 1, 0.01, () => _profile.Lighting.ShadowOpacity, v => _profile.Lighting.ShadowOpacity = v, false);
        AddHeading("FILL / AMBIENT / RIM");
        AddSlider("Fill azimuth", 0, 359, 1, () => _profile.Lighting.FillAzimuth, v => _profile.Lighting.FillAzimuth = v);
        AddSlider("Fill elevation", 0, 85, 1, () => _profile.Lighting.FillElevation, v => _profile.Lighting.FillElevation = v);
        AddColor("Fill color", () => _profile.Lighting.FillColor, v => _profile.Lighting.FillColor = v);
        AddSlider("Fill energy", 0, 8, 0.05, () => _profile.Lighting.FillEnergy, v => _profile.Lighting.FillEnergy = v, false);
        AddColor("Ambient color", () => _profile.Lighting.AmbientColor, v => _profile.Lighting.AmbientColor = v);
        AddSlider("Ambient energy", 0, 4, 0.02, () => _profile.Lighting.AmbientEnergy, v => _profile.Lighting.AmbientEnergy = v, false);
        AddCheck("Rim light enabled", () => _profile.Lighting.RimLightEnabled, v => _profile.Lighting.RimLightEnabled = v, false);
        AddColor("Rim color", () => _profile.Lighting.RimColor, v => _profile.Lighting.RimColor = v);
        AddSlider("Rim energy", 0, 8, 0.05, () => _profile.Lighting.RimEnergy, v => _profile.Lighting.RimEnergy = v, false);
        AddColor("Background", () => _profile.Lighting.BackgroundColor, v => _profile.Lighting.BackgroundColor = v);
    }

    private void BuildPostSettings()
    {
        AddCheck("Post pass enabled", () => _profile.Post.Enabled, v => _profile.Post.Enabled = v, false);
        AddSlider("Exposure · stops", -2, 2, 0.05, () => _profile.Post.Exposure, v => _profile.Post.Exposure = v, false);
        AddSlider("Brightness", 0.4, 1.8, 0.01, () => _profile.Post.Brightness, v => _profile.Post.Brightness = v, false);
        AddSlider("Contrast", 0.4, 2, 0.01, () => _profile.Post.Contrast, v => _profile.Post.Contrast = v, false);
        AddSlider("Saturation", 0, 2, 0.01, () => _profile.Post.Saturation, v => _profile.Post.Saturation = v, false);
        AddSlider("Temperature", -1, 1, 0.01, () => _profile.Post.Temperature, v => _profile.Post.Temperature = v, false);
        AddSlider("Green / magenta tint", -1, 1, 0.01, () => _profile.Post.Tint, v => _profile.Post.Tint = v, false);
        AddOption("Tonemapper", new[] { "Linear", "Reinhard", "Filmic", "ACES", "AgX" }, _profile.Post.Tonemapper, i => _profile.Post.Tonemapper = i, false);
        AddCheck("Bloom", () => _profile.Post.BloomEnabled, v => _profile.Post.BloomEnabled = v, false);
        AddSlider("Bloom intensity", 0, 3, 0.01, () => _profile.Post.BloomIntensity, v => _profile.Post.BloomIntensity = v, false);
        AddSlider("Bloom threshold", 0, 8, 0.05, () => _profile.Post.BloomThreshold, v => _profile.Post.BloomThreshold = v, false);
        AddSlider("Bloom spread", 0, 1, 0.01, () => _profile.Post.BloomSpread, v => _profile.Post.BloomSpread = v, false);
        AddSlider("Vignette", 0, 1, 0.01, () => _profile.Post.Vignette, v => _profile.Post.Vignette = v, false);
        AddSlider("Film grain", 0, 0.3, 0.005, () => _profile.Post.FilmGrain, v => _profile.Post.FilmGrain = v, false);
        AddSlider("Sharpen", 0, 1.5, 0.01, () => _profile.Post.Sharpen, v => _profile.Post.Sharpen = v, false);
        AddSlider("Posterize levels · 0=off", 0, 32, 1, () => _profile.Post.PosterizeLevels, v => _profile.Post.PosterizeLevels = (int)v, false);
        AddSlider("Dither", 0, 1, 0.01, () => _profile.Post.Dither, v => _profile.Post.Dither = v, false);
    }

    private void BuildOutlineSettings()
    {
        AddNote("True screen-space outline from scene depth + normals. It affects silhouettes and internal hard creases independently.");
        AddCheck("Outline enabled", () => _profile.Outline.Enabled, v => _profile.Outline.Enabled = v, false);
        AddColor("Outline color", () => _profile.Outline.Color, v => _profile.Outline.Color = v, false);
        AddSlider("Width · pixels", 0.5, 8, 0.1, () => _profile.Outline.WidthPixels, v => _profile.Outline.WidthPixels = v, false);
        AddSlider("Opacity", 0, 1, 0.01, () => _profile.Outline.Opacity, v => _profile.Outline.Opacity = v, false);
        AddSlider("Depth threshold", 0.0001, 0.2, 0.0005, () => _profile.Outline.DepthThreshold, v => _profile.Outline.DepthThreshold = v, false);
        AddSlider("Normal threshold", 0.01, 2, 0.01, () => _profile.Outline.NormalThreshold, v => _profile.Outline.NormalThreshold = v, false);
        AddSlider("Silhouette strength", 0, 3, 0.01, () => _profile.Outline.SilhouetteStrength, v => _profile.Outline.SilhouetteStrength = v, false);
        AddSlider("Crease strength", 0, 3, 0.01, () => _profile.Outline.CreaseStrength, v => _profile.Outline.CreaseStrength = v, false);
        AddSlider("Distance fade", 0, 2, 0.01, () => _profile.Outline.DistanceFade, v => _profile.Outline.DistanceFade = v, false);
    }

    private void BuildVfxSettings()
    {
        AddHeading("WEAPON READABILITY");
        AddColor("Tracer color", () => _profile.Vfx.TracerColor, v => _profile.Vfx.TracerColor = v);
        AddSlider("Tracer width", 0.01, 0.8, 0.01, () => _profile.Vfx.TracerWidth, v => _profile.Vfx.TracerWidth = v);
        AddSlider("Tracer length", 0.1, 8, 0.05, () => _profile.Vfx.TracerLength, v => _profile.Vfx.TracerLength = v);
        AddSlider("Tracer speed", 1, 50, 0.5, () => _profile.Vfx.TracerSpeed, v => _profile.Vfx.TracerSpeed = v);
        AddSlider("Tracer emission", 0, 12, 0.1, () => _profile.Vfx.TracerEnergy, v => _profile.Vfx.TracerEnergy = v);
        AddSlider("Muzzle size", 0.05, 2, 0.01, () => _profile.Vfx.MuzzleSize, v => _profile.Vfx.MuzzleSize = v);
        AddSlider("Impact size", 0.05, 3, 0.01, () => _profile.Vfx.ImpactSize, v => _profile.Vfx.ImpactSize = v);
        AddSlider("Spark count", 0, 20, 1, () => _profile.Vfx.SparkCount, v => _profile.Vfx.SparkCount = (int)v);
        AddSlider("Spark size", 0.01, 0.4, 0.01, () => _profile.Vfx.SparkSize, v => _profile.Vfx.SparkSize = v);
        AddHeading("FIRE / SMOKE");
        AddColor("Fire color", () => _profile.Vfx.FireColor, v => _profile.Vfx.FireColor = v);
        AddSlider("Fire size", 0.1, 4, 0.05, () => _profile.Vfx.FireSize, v => _profile.Vfx.FireSize = v);
        AddSlider("Fire emission", 0, 12, 0.1, () => _profile.Vfx.FireEnergy, v => _profile.Vfx.FireEnergy = v);
        AddSlider("Fire flicker", 0, 1, 0.01, () => _profile.Vfx.FireFlicker, v => _profile.Vfx.FireFlicker = v);
        AddColor("Smoke color", () => _profile.Vfx.SmokeColor, v => _profile.Vfx.SmokeColor = v);
        AddSlider("Smoke amount", 0, 20, 1, () => _profile.Vfx.SmokeAmount, v => _profile.Vfx.SmokeAmount = (int)v);
        AddSlider("Smoke opacity", 0, 1, 0.01, () => _profile.Vfx.SmokeOpacity, v => _profile.Vfx.SmokeOpacity = v);
        AddSlider("Smoke size", 0.1, 4, 0.05, () => _profile.Vfx.SmokeSize, v => _profile.Vfx.SmokeSize = v);
        AddSlider("Smoke rise", 0, 6, 0.05, () => _profile.Vfx.SmokeRise, v => _profile.Vfx.SmokeRise = v);
        AddSlider("Scorch size", 0, 6, 0.05, () => _profile.Vfx.ScorchSize, v => _profile.Vfx.ScorchSize = v);
    }

    private void BuildGroundSettings()
    {
        AddColor("Secondary color", () => _profile.Ground.SecondaryColor, v => _profile.Ground.SecondaryColor = v);
        AddSlider("Macro variation", 0, 1, 0.01, () => _profile.Ground.MacroAmount, v => _profile.Ground.MacroAmount = v);
        AddSlider("Macro scale", 0.01, 2, 0.01, () => _profile.Ground.MacroScale, v => _profile.Ground.MacroScale = v);
        AddSlider("Micro variation", 0, 1, 0.01, () => _profile.Ground.MicroAmount, v => _profile.Ground.MicroAmount = v);
        AddSlider("Micro scale", 0.1, 20, 0.1, () => _profile.Ground.MicroScale, v => _profile.Ground.MicroScale = v);
        AddSlider("Normal strength", 0, 2, 0.01, () => _profile.Ground.NormalStrength, v => _profile.Ground.NormalStrength = v);
        AddColor("Dust / track tint", () => _profile.Ground.DustTint, v => _profile.Ground.DustTint = v);
        AddSlider("Track opacity", 0, 1, 0.01, () => _profile.Ground.TracksOpacity, v => _profile.Ground.TracksOpacity = v);
        AddSlider("Scorch opacity", 0, 1, 0.01, () => _profile.Ground.ScorchOpacity, v => _profile.Ground.ScorchOpacity = v);
        AddSlider("Unit spacing", 4, 14, 0.1, () => _profile.Ground.UnitSeparation, v => _profile.Ground.UnitSeparation = v, false);
    }

    private void BuildHudSettings()
    {
        AddSlider("HUD scale", 0.7, 1.5, 0.01, () => _profile.Hud.Scale, v => _profile.Hud.Scale = v, false);
        AddSlider("Panel opacity", 0, 1, 0.01, () => _profile.Hud.PanelOpacity, v => _profile.Hud.PanelOpacity = v, false);
        AddSlider("HUD brightness", 0.3, 2, 0.01, () => _profile.Hud.Brightness, v => _profile.Hud.Brightness = v, false);
        AddSlider("HUD saturation", 0, 2, 0.01, () => _profile.Hud.Saturation, v => _profile.Hud.Saturation = v, false);
        AddColor("HUD accent", () => _profile.Hud.AccentColor, v => _profile.Hud.AccentColor = v, false);
        AddSlider("Minimap contrast", 0.4, 2, 0.01, () => _profile.Hud.MinimapContrast, v => _profile.Hud.MinimapContrast = v, false);
        AddSlider("Health bar width", 24, 120, 1, () => _profile.Hud.HealthBarWidth, v => _profile.Hud.HealthBarWidth = v, false);
        AddSlider("Health bar height", 2, 14, 0.5, () => _profile.Hud.HealthBarHeight, v => _profile.Hud.HealthBarHeight = v, false);
        AddSlider("Health bar opacity", 0, 1, 0.01, () => _profile.Hud.HealthBarOpacity, v => _profile.Hud.HealthBarOpacity = v, false);
        AddSlider("Selection ring width", 0.02, 0.5, 0.01, () => _profile.Hud.SelectionRingWidth, v => _profile.Hud.SelectionRingWidth = v, false);
        AddSlider("Selection brightness", 0, 5, 0.05, () => _profile.Hud.SelectionBrightness, v => _profile.Hud.SelectionBrightness = v, false);
        AddSlider("Selection pulse", 0, 1, 0.01, () => _profile.Hud.SelectionPulse, v => _profile.Hud.SelectionPulse = v, false);
        AddSlider("Target marker", 0, 5, 0.05, () => _profile.Hud.TargetMarkerBrightness, v => _profile.Hud.TargetMarkerBrightness = v, false);
        AddSlider("Indicator scale", 0.5, 2, 0.01, () => _profile.Hud.IndicatorScale, v => _profile.Hud.IndicatorScale = v, false);
    }

    private void ApplyProfile(bool rebuildMaterials)
    {
        _profile.Normalize();
        ApplyUnitLayout();
        ApplyCamera();
        ApplyLighting();
        if (rebuildMaterials) ApplyMaterials();
        ApplyPost();
        ApplyVfxAppearance();
        ApplySceneVisibility();
        ApplyHudAppearance();
    }

    private void ApplyUnitLayout()
    {
        if (_units.Count != 4) return;
        float spacing = _profile.Ground.UnitSeparation;
        Vector3[] positions =
        {
            new(0.35f * spacing, 0.08f, 0.82f * spacing),
            new(-0.92f * spacing, 0.08f, 0.34f * spacing),
            new(0.88f * spacing, 0.08f, -0.25f * spacing),
            new(-0.34f * spacing, 0.08f, -0.86f * spacing)
        };
        for (int i = 0; i < _units.Count; i++) _units[i].Position = positions[i];
        _units[0].LookAt(new Vector3(_targetPoint.X, _units[0].GlobalPosition.Y, _targetPoint.Z), Vector3.Up);
        _units[1].RotationDegrees = new Vector3(0f, 135f, 0f);
        _units[2].RotationDegrees = new Vector3(0f, 225f, 0f);
        _units[3].RotationDegrees = new Vector3(0f, 315f, 0f);
        _shooterMuzzle = _units[0].GlobalPosition + _units[0].GlobalBasis * new Vector3(0.85f, 1.38f, -3.35f);
        if (_selectionRing is not null) _selectionRing.Position = _units[0].GlobalPosition + Vector3.Up * 0.07f;
        for (int i = 0; i < _healthBars.Count; i++)
        {
            Vector3 world = _units[i].GlobalPosition + Vector3.Up * 3.3f;
            (Control root, ColorRect fill, _, float health) = _healthBars[i];
            _healthBars[i] = (root, fill, world, health);
            if (i < _worldHealthBars.Count)
            {
                (MeshInstance3D background, MeshInstance3D fillMesh, float worldHealth) = _worldHealthBars[i];
                background.Position = world;
                fillMesh.Position = world + Vector3.Up * 0.003f;
                _worldHealthBars[i] = (background, fillMesh, worldHealth);
            }
            if (i < _worldHealthLabels.Count) _worldHealthLabels[i].Label.Position = world + Vector3.Up * 0.18f;
        }
    }

    private void ApplyCamera()
    {
        if (_camera is null) return;
        Vector2 size = GetViewport().GetVisibleRect().Size;
        float aspect = size.Y <= 1f ? 16f / 9f : size.X / size.Y;
        float halfWidthWorld = _profile.Camera.ZoomCells * GodotConversions.WorldUnitsPerBuildCell * 0.5f;
        float distance = halfWidthWorld / Mathf.Max(0.1f, Mathf.Tan(Mathf.DegToRad(_camera.Fov * 0.5f)) * aspect);
        float yaw = Mathf.DegToRad(_profile.Camera.YawDegrees);
        float pitch = Mathf.DegToRad(_profile.Camera.PitchDegrees);
        Vector3 focus = new(0f, 0f, 0f);
        Vector3 offset = new(Mathf.Sin(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(yaw) * Mathf.Cos(pitch));
        _camera.GlobalPosition = focus + offset * distance;
        _camera.LookAt(focus, Vector3.Up);

        Vector2 desired = new(size.X * (_controlsVisible ? 0.63f : 0.5f), size.Y * 0.50f);
        if (TryProjectToGround(desired, out Vector3 groundAtDesired))
        {
            Vector3 translation = focus - groundAtDesired;
            focus += new Vector3(translation.X, 0f, translation.Z);
            _camera.GlobalPosition = focus + offset * distance;
            _camera.LookAt(focus, Vector3.Up);
        }
    }

    private bool TryProjectToGround(Vector2 screen, out Vector3 point)
    {
        if (_camera is null) { point = default; return false; }
        Vector3 origin = _camera.ProjectRayOrigin(screen);
        Vector3 direction = _camera.ProjectRayNormal(screen);
        if (Mathf.Abs(direction.Y) < 0.0001f) { point = default; return false; }
        float t = -origin.Y / direction.Y;
        if (t < 0f) { point = default; return false; }
        point = origin + direction * t;
        return true;
    }

    private void ApplyLighting()
    {
        if (_keyLight is null || _fillLight is null || _rimLight is null || _environment is null) return;
        _keyLight.RotationDegrees = new Vector3(-_profile.Lighting.KeyElevation, _profile.Lighting.KeyAzimuth, 0f);
        _keyLight.LightColor = M7LookMaterialFactory.ParseColor(_profile.Lighting.KeyColor);
        _keyLight.LightEnergy = _profile.Lighting.KeyEnergy;
        _keyLight.LightAngularDistance = _profile.Lighting.KeyAngularSize;
        _keyLight.ShadowBlur = _profile.Lighting.ShadowBlur;
        _keyLight.ShadowOpacity = _profile.Lighting.ShadowOpacity;
        _fillLight.RotationDegrees = new Vector3(-_profile.Lighting.FillElevation, _profile.Lighting.FillAzimuth, 0f);
        _fillLight.LightColor = M7LookMaterialFactory.ParseColor(_profile.Lighting.FillColor);
        _fillLight.LightEnergy = _profile.Lighting.FillEnergy;
        _rimLight.Visible = _profile.Lighting.RimLightEnabled;
        _rimLight.RotationDegrees = new Vector3(-28f, _profile.Camera.YawDegrees + 180f, 0f);
        _rimLight.LightColor = M7LookMaterialFactory.ParseColor(_profile.Lighting.RimColor);
        _rimLight.LightEnergy = _profile.Lighting.RimEnergy;
        _environment.BackgroundColor = M7LookMaterialFactory.ParseColor(_profile.Lighting.BackgroundColor);
        _environment.AmbientLightColor = M7LookMaterialFactory.ParseColor(_profile.Lighting.AmbientColor);
        _environment.AmbientLightEnergy = _profile.Lighting.AmbientEnergy;
        _environment.TonemapMode = (Godot.Environment.ToneMapper)_profile.Post.Tonemapper;
        _environment.TonemapExposure = Mathf.Pow(2f, _profile.Post.Exposure);
        _environment.GlowEnabled = _profile.Post.BloomEnabled;
        _environment.GlowIntensity = _profile.Post.BloomIntensity;
        _environment.GlowBloom = _profile.Post.BloomSpread;
        _environment.GlowHdrThreshold = _profile.Post.BloomThreshold;
    }

    private void ApplyMaterials()
    {
        Dictionary<M7LookMaterialRole, Material> materials = M7LookMaterialFactory.BuildSharedMaterials(_profile);
        foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _roleMeshes)
            mesh.MaterialOverride = materials[role];
    }

    private void ApplyPost()
    {
        if (_postMaterial is null) return;
        if (_postQuad is not null) _postQuad.Visible = _profile.Post.Enabled;
        _postMaterial.SetShaderParameter("brightness", _profile.Post.Brightness);
        _postMaterial.SetShaderParameter("contrast", _profile.Post.Contrast);
        _postMaterial.SetShaderParameter("saturation", _profile.Post.Saturation);
        _postMaterial.SetShaderParameter("temperature", _profile.Post.Temperature);
        _postMaterial.SetShaderParameter("tint", _profile.Post.Tint);
        _postMaterial.SetShaderParameter("vignette", _profile.Post.Vignette);
        _postMaterial.SetShaderParameter("film_grain", _profile.Post.FilmGrain);
        _postMaterial.SetShaderParameter("sharpen", _profile.Post.Sharpen);
        _postMaterial.SetShaderParameter("posterize_levels", (float)_profile.Post.PosterizeLevels);
        _postMaterial.SetShaderParameter("dither_amount", _profile.Post.Dither);
        _postMaterial.SetShaderParameter("outline_enabled", _profile.Outline.Enabled);
        _postMaterial.SetShaderParameter("outline_color", M7LookMaterialFactory.ParseColor(_profile.Outline.Color));
        _postMaterial.SetShaderParameter("outline_width", _profile.Outline.WidthPixels);
        _postMaterial.SetShaderParameter("outline_opacity", _profile.Outline.Opacity);
        _postMaterial.SetShaderParameter("depth_threshold", _profile.Outline.DepthThreshold);
        _postMaterial.SetShaderParameter("normal_threshold", _profile.Outline.NormalThreshold);
        _postMaterial.SetShaderParameter("silhouette_strength", _profile.Outline.SilhouetteStrength);
        _postMaterial.SetShaderParameter("crease_strength", _profile.Outline.CreaseStrength);
        _postMaterial.SetShaderParameter("distance_fade", _profile.Outline.DistanceFade);
    }

    private void ApplyVfxAppearance()
    {
        Color tracerColor = M7LookMaterialFactory.ParseColor(_profile.Vfx.TracerColor);
        Material tracerMaterial = M7LookMaterialFactory.Emissive(tracerColor, _profile.Vfx.TracerEnergy, 0.92f);
        if (_tracer?.Mesh is BoxMesh box)
        {
            box.Size = new Vector3(_profile.Vfx.TracerWidth, _profile.Vfx.TracerWidth, _profile.Vfx.TracerLength);
            _tracer.MaterialOverride = tracerMaterial;
        }
        if (_muzzle is not null) _muzzle.MaterialOverride = tracerMaterial;
        if (_impact is not null) _impact.MaterialOverride = tracerMaterial;
        foreach (MeshInstance3D spark in _sparks) spark.MaterialOverride = tracerMaterial;
        Material fire = M7LookMaterialFactory.Emissive(M7LookMaterialFactory.ParseColor(_profile.Vfx.FireColor), _profile.Vfx.FireEnergy, 0.82f);
        foreach (MeshInstance3D flame in _fireFlames) flame.MaterialOverride = fire;
        Material smoke = M7LookMaterialFactory.Transparent(WithAlpha(M7LookMaterialFactory.ParseColor(_profile.Vfx.SmokeColor), _profile.Vfx.SmokeOpacity));
        foreach (MeshInstance3D puff in _smokePuffs) puff.MaterialOverride = smoke;
        Color trackColor = WithAlpha(M7LookMaterialFactory.ParseColor(_profile.Ground.DustTint), _profile.Ground.TracksOpacity);
        foreach (MeshInstance3D track in _tracks) track.MaterialOverride = M7LookMaterialFactory.Transparent(trackColor);
        if (_scorch is not null)
        {
            _scorch.MaterialOverride = M7LookMaterialFactory.Transparent(new Color(0.025f, 0.018f, 0.014f, _profile.Ground.ScorchOpacity));
            _scorch.Scale = Vector3.One * _profile.Vfx.ScorchSize / 2.1f;
        }
        ApplySelectionMaterials();
    }

    private void ApplySelectionMaterials()
    {
        float pulse = 1f + Mathf.Sin((float)_time * 2.4f) * _profile.Hud.SelectionPulse;
        if (_selectionRing is not null)
        {
            if (_selectionRing.Mesh is TorusMesh ringMesh) ringMesh.OuterRadius = ringMesh.InnerRadius + _profile.Hud.SelectionRingWidth;
            _selectionRing.MaterialOverride = M7LookMaterialFactory.Emissive(M7LookMaterialFactory.ParseColor(_profile.Hud.AccentColor), _profile.Hud.SelectionBrightness * pulse, 0.90f);
            _selectionRing.Scale = Vector3.One * _profile.Hud.IndicatorScale;
        }
        if (_targetMarker is not null)
        {
            _targetMarker.MaterialOverride = M7LookMaterialFactory.Emissive(new Color("e35e42"), _profile.Hud.TargetMarkerBrightness, 0.86f);
            _targetMarker.Scale = Vector3.One * _profile.Hud.IndicatorScale;
        }
    }

    private void ApplySceneVisibility()
    {
        if (_fakeHudLayer is not null) _fakeHudLayer.Visible = _profile.Scene.HudEnabled;
        if (_fogPreview is not null) _fogPreview.Visible = _profile.Scene.FogPreviewEnabled;
        if (_selectionRing is not null) _selectionRing.Visible = _profile.Scene.SelectionEnabled;
        if (_targetMarker is not null) _targetMarker.Visible = _profile.Scene.SelectionEnabled;
        if (_tracer is not null) _tracer.Visible = _profile.Scene.FiringEnabled;
        if (_muzzle is not null) _muzzle.Visible = _profile.Scene.FiringEnabled;
        if (_impact is not null) _impact.Visible = _profile.Scene.FiringEnabled;
        foreach (MeshInstance3D spark in _sparks) spark.Visible = _profile.Scene.FiringEnabled;
        foreach (MeshInstance3D flame in _fireFlames) flame.Visible = _profile.Scene.BurningEnabled;
        foreach (MeshInstance3D puff in _smokePuffs) puff.Visible = _profile.Scene.BurningEnabled && _profile.Scene.DustEnabled;
    }

    private void ApplyHudAppearance()
    {
        if (_fakeHudRoot is null) return;
        Color modulation = M7LookMaterialFactory.ParseColor(_profile.Hud.AccentColor);
        float luminance = _profile.Hud.Brightness;
        _fakeHudRoot.Modulate = new Color(
            Mathf.Lerp(luminance, modulation.R * luminance, _profile.Hud.Saturation * 0.12f),
            Mathf.Lerp(luminance, modulation.G * luminance, _profile.Hud.Saturation * 0.12f),
            Mathf.Lerp(luminance, modulation.B * luminance, _profile.Hud.Saturation * 0.12f),
            _profile.Hud.PanelOpacity);
        _fakeHudRoot.Scale = Vector2.One * _profile.Hud.Scale;
        if (_minimapGround is not null)
        {
            float contrast = _profile.Hud.MinimapContrast;
            Color baseColor = new("34423f");
            _minimapGround.Color = new Color(
                Mathf.Clamp((baseColor.R - 0.5f) * contrast + 0.5f, 0f, 1f),
                Mathf.Clamp((baseColor.G - 0.5f) * contrast + 0.5f, 0f, 1f),
                Mathf.Clamp((baseColor.B - 0.5f) * contrast + 0.5f, 0f, 1f), 1f);
        }
        foreach ((Control root, ColorRect fill, _, float health) in _healthBars)
        {
            root.Size = new Vector2(_profile.Hud.HealthBarWidth, _profile.Hud.HealthBarHeight);
            fill.Position = new Vector2(2f, 2f);
            fill.Size = new Vector2(Mathf.Max(1f, (_profile.Hud.HealthBarWidth - 4f) * health), Mathf.Max(1f, _profile.Hud.HealthBarHeight - 4f));
            root.Modulate = new Color(1f, 1f, 1f, _profile.Hud.HealthBarOpacity);
            root.Scale = Vector2.One * _profile.Hud.Scale;
        }
        foreach ((MeshInstance3D background, MeshInstance3D fill, float health) in _worldHealthBars)
        {
            float width = _profile.Hud.HealthBarWidth / 35f;
            float height = _profile.Hud.HealthBarHeight / 34f;
            if (background.Mesh is QuadMesh backgroundMesh) backgroundMesh.Size = new Vector2(width, height);
            if (fill.Mesh is QuadMesh fillMesh) fillMesh.Size = new Vector2(Mathf.Max(0.04f, (width - 0.10f) * health), Mathf.Max(0.04f, height - 0.07f));
            if (background.MaterialOverride is StandardMaterial3D backgroundMaterial)
                backgroundMaterial.AlbedoColor = WithAlpha(backgroundMaterial.AlbedoColor, _profile.Hud.HealthBarOpacity);
            if (fill.MaterialOverride is StandardMaterial3D fillMaterial)
                fillMaterial.AlbedoColor = WithAlpha(fillMaterial.AlbedoColor, _profile.Hud.HealthBarOpacity);
            background.Scale = Vector3.One * _profile.Hud.Scale;
            fill.Scale = Vector3.One * _profile.Hud.Scale;
            background.Visible = _profile.Scene.HudEnabled;
            fill.Visible = _profile.Scene.HudEnabled;
        }
        foreach ((Label3D label, float health, bool selected) in _worldHealthLabels)
        {
            Color baseColor = selected ? new Color("5ddd7b") : health < 0.55f ? new Color("e6783d") : new Color("8ed957");
            label.Modulate = WithAlpha(baseColor, _profile.Hud.HealthBarOpacity);
            label.PixelSize = 0.010f * (_profile.Hud.HealthBarWidth / 54f) * _profile.Hud.Scale;
            label.FontSize = Mathf.RoundToInt(30f * (_profile.Hud.HealthBarHeight / 6f));
            label.Visible = _profile.Scene.HudEnabled;
        }
    }

    private void AnimateScene()
    {
        if (!_profile.Scene.Paused)
            foreach (Node3D drill in _drills) drill.RotateObjectLocal(Vector3.Forward, 4.8f * (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed);

        float pulse = 1f + Mathf.Sin((float)_time * _profile.Emission.PulseSpeed) * _profile.Emission.PulseAmount;
        foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _roleMeshes)
        {
            if (role is not (M7LookMaterialRole.Signal or M7LookMaterialRole.Lamp or M7LookMaterialRole.Crystal)) continue;
            float energy = role switch
            {
                M7LookMaterialRole.Signal => _profile.Emission.SignalEnergy,
                M7LookMaterialRole.Lamp => _profile.Emission.LampEnergy,
                _ => _profile.Emission.CrystalEnergy
            };
            Color color = role switch
            {
                M7LookMaterialRole.Signal => M7LookMaterialFactory.ParseColor(_profile.Emission.SignalColor),
                M7LookMaterialRole.Lamp => M7LookMaterialFactory.ParseColor(_profile.Emission.LampColor),
                _ => M7LookMaterialFactory.ParseColor(_profile.Emission.CrystalColor)
            };
            if (mesh.MaterialOverride is StandardMaterial3D material)
            {
                material.Emission = color;
                material.EmissionEnergyMultiplier = energy * pulse;
            }
        }
        AnimateWeapon();
        AnimateFire();
        ApplySelectionMaterials();
    }

    private void AnimateWeapon()
    {
        if (_tracer is null || _muzzle is null || _impact is null) return;
        float distance = _shooterMuzzle.DistanceTo(_targetPoint);
        float cycleSeconds = Mathf.Max(0.65f, distance / _profile.Vfx.TracerSpeed + 0.44f);
        float phase = Mathf.PosMod((float)_time, cycleSeconds) / cycleSeconds;
        float travel = Mathf.Clamp(phase * cycleSeconds * _profile.Vfx.TracerSpeed / Mathf.Max(0.01f, distance), 0f, 1f);
        _tracer.GlobalPosition = _shooterMuzzle.Lerp(_targetPoint, travel);
        _tracer.LookAt(_targetPoint, Vector3.Up);
        float muzzlePulse = Mathf.Clamp(1f - phase * 14f, 0f, 1f);
        float impactPulse = Mathf.Clamp(1f - Mathf.Abs(travel - 1f) * 18f, 0f, 1f);
        _muzzle.GlobalPosition = _shooterMuzzle;
        _muzzle.Scale = Vector3.One * _profile.Vfx.MuzzleSize * muzzlePulse;
        _impact.GlobalPosition = _targetPoint;
        _impact.Scale = Vector3.One * _profile.Vfx.ImpactSize * impactPulse;
        for (int i = 0; i < _sparks.Count; i++)
        {
            float local = Mathf.PosMod((float)_time * 2.3f + i * 0.127f, 1f);
            float angle = i * 2.399f;
            _sparks[i].GlobalPosition = _targetPoint + new Vector3(Mathf.Cos(angle) * local, 0.15f + local * 1.7f, Mathf.Sin(angle) * local) * _profile.Vfx.ImpactSize;
            _sparks[i].Scale = Vector3.One * _profile.Vfx.SparkSize * (1f - local) * 4f;
            _sparks[i].Visible = _profile.Scene.FiringEnabled && i < _profile.Vfx.SparkCount && impactPulse > 0.01f;
        }
    }

    private void AnimateFire()
    {
        Vector3 origin = new(-10.5f, 2.1f, -8.7f);
        for (int i = 0; i < _fireFlames.Count; i++)
        {
            float wobble = Mathf.Sin((float)_time * (4.3f + i * 0.17f) + i) * _profile.Vfx.FireFlicker;
            _fireFlames[i].Position = origin + new Vector3((i - 3) * 0.26f + wobble * 0.18f, 0.25f + (i % 3) * 0.28f, (i % 2) * 0.24f);
            _fireFlames[i].Scale = new Vector3(0.72f, 1.15f + wobble * 0.35f, 0.72f) * _profile.Vfx.FireSize;
        }
        for (int i = 0; i < _smokePuffs.Count; i++)
        {
            float phase = Mathf.PosMod((float)_time * 0.24f + i / (float)_smokePuffs.Count, 1f);
            float angle = i * 1.89f;
            _smokePuffs[i].Position = origin + new Vector3(Mathf.Cos(angle) * phase * 0.72f, 0.8f + phase * _profile.Vfx.SmokeRise * 3f, Mathf.Sin(angle) * phase * 0.72f);
            _smokePuffs[i].Scale = Vector3.One * _profile.Vfx.SmokeSize * (0.25f + phase * 0.75f);
            _smokePuffs[i].Visible = _profile.Scene.BurningEnabled && _profile.Scene.DustEnabled && i < _profile.Vfx.SmokeAmount;
        }
    }

    private void UpdateWorldHud()
    {
        if (_camera is null) return;
        foreach ((Control root, _, Vector3 world, _) in _healthBars)
        {
            Vector2 point = _camera.UnprojectPosition(world);
            root.Position = point - new Vector2(root.Size.X * 0.5f, root.Size.Y);
            root.Visible = true;
        }
    }

    private void AdjustZoom(float multiplier)
    {
        _profile.Camera.ZoomCells = Mathf.Clamp(_profile.Camera.ZoomCells * multiplier, 24f, 72f);
        ApplyCamera();
        BuildCurrentSettings();
        SetStatus($"Zoom {_profile.Camera.ZoomCells:0.0} build cells");
    }

    private void SetControlsVisible(bool visible)
    {
        _controlsVisible = visible;
        if (_controlsLayer is not null) _controlsLayer.Visible = visible;
        ApplyCamera();
    }

    private void CopyProfile()
    {
        DisplayServer.ClipboardSet(_profile.ToJson());
        SetStatus($"COPIED · schema {_profile.SchemaVersion} · zoom {_profile.Camera.ZoomCells:0.0}");
    }

    private void PasteProfile()
    {
        if (!M7LookProfile.TryFromJson(DisplayServer.ClipboardGet(), out M7LookProfile pasted, out string error))
        {
            SetStatus(error, errorState: true);
            return;
        }
        _profile = pasted;
        ApplyProfile(rebuildMaterials: true);
        BuildCurrentSettings();
        SetStatus($"APPLIED · schema {_profile.SchemaVersion} · zoom {_profile.Camera.ZoomCells:0.0}");
    }

    private void TogglePause()
    {
        _profile.Scene.Paused = !_profile.Scene.Paused;
        if (_pauseButton is not null) _pauseButton.Text = _profile.Scene.Paused ? "RESUME" : "PAUSE";
        SetStatus(_profile.Scene.Paused ? "Scene frozen for comparison" : "Scene animation resumed");
    }

    private void ResetAll()
    {
        _profile = _defaults.Clone();
        ApplyProfile(rebuildMaterials: true);
        BuildCurrentSettings();
        SetStatus("All settings reset to neutral lab baseline");
    }

    private void ResetCurrentSection()
    {
        M7LookProfile defaults = _defaults.Clone();
        switch (_category)
        {
            case 0: _profile.Camera = defaults.Camera; _profile.Scene = defaults.Scene; break;
            case 1: _profile.Shading = defaults.Shading; break;
            case 2: _profile.Materials = defaults.Materials; break;
            case 3: _profile.Glass = defaults.Glass; _profile.Emission = defaults.Emission; break;
            case 4: _profile.Lighting = defaults.Lighting; break;
            case 5: _profile.Post = defaults.Post; break;
            case 6: _profile.Outline = defaults.Outline; break;
            case 7: _profile.Vfx = defaults.Vfx; break;
            case 8: _profile.Ground = defaults.Ground; break;
            case 9: _profile.Hud = defaults.Hud; break;
        }
        ApplyProfile(rebuildMaterials: true);
        BuildCurrentSettings();
        SetStatus($"Reset {Categories[_category]}");
    }

    private Button AddAction(Container parent, string text, Action action, string tooltip = "")
    {
        Button button = new() { Text = text, TooltipText = tooltip, CustomMinimumSize = new Vector2(0f, 30f) };
        button.Pressed += action; parent.AddChild(button); return button;
    }

    private void AddSlider(string label, double min, double max, double step, Func<float> getter, Action<float> setter, bool rebuildMaterials = true)
    {
        if (_settingsBox is null) return;
        VBoxContainer block = new(); _settingsBox.AddChild(block);
        HBoxContainer header = new(); block.AddChild(header);
        Label name = new() { Text = label, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; header.AddChild(name);
        Label value = new() { Text = FormatValue(getter(), step), HorizontalAlignment = HorizontalAlignment.Right, CustomMinimumSize = new Vector2(64f, 0f) }; header.AddChild(value);
        HSlider slider = new() { MinValue = min, MaxValue = max, Step = step, Value = getter(), CustomMinimumSize = new Vector2(0f, 20f) };
        slider.ValueChanged += changed =>
        {
            setter((float)changed); value.Text = FormatValue((float)changed, step); ApplyProfile(rebuildMaterials); SetStatus($"{label}: {value.Text}");
        };
        block.AddChild(slider);
    }

    private void AddCheck(string label, Func<bool> getter, Action<bool> setter, bool rebuildMaterials = true)
    {
        if (_settingsBox is null) return;
        CheckButton button = new() { Text = label, ButtonPressed = getter() };
        button.Toggled += value => { setter(value); ApplyProfile(rebuildMaterials); SetStatus($"{label}: {(value ? "on" : "off")}"); };
        _settingsBox.AddChild(button);
    }

    private void AddColor(string label, Func<string> getter, Action<string> setter, bool rebuildMaterials = true)
    {
        if (_settingsBox is null) return;
        HBoxContainer row = new(); _settingsBox.AddChild(row);
        Label name = new() { Text = label, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; row.AddChild(name);
        ColorPickerButton picker = new() { Color = M7LookMaterialFactory.ParseColor(getter()), CustomMinimumSize = new Vector2(74f, 30f), EditAlpha = false };
        picker.ColorChanged += color => { string html = $"#{color.ToHtml(false)}"; setter(html); ApplyProfile(rebuildMaterials); SetStatus($"{label}: {html}"); };
        row.AddChild(picker);
    }

    private void AddOption(string label, string[] options, int selected, Action<int> setter, bool rebuildMaterials = true, bool applyProfile = true)
    {
        if (_settingsBox is null) return;
        HBoxContainer row = new(); _settingsBox.AddChild(row);
        Label name = new() { Text = label, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; row.AddChild(name);
        OptionButton option = new() { CustomMinimumSize = new Vector2(180f, 32f) };
        foreach (string item in options) option.AddItem(item);
        option.Selected = Mathf.Clamp(selected, 0, options.Length - 1);
        option.ItemSelected += index => { setter((int)index); if (applyProfile) ApplyProfile(rebuildMaterials); SetStatus($"{label}: {options[(int)index]}"); };
        row.AddChild(option);
    }

    private void AddHeading(string text)
    {
        if (_settingsBox is null) return;
        Label label = new() { Text = text, CustomMinimumSize = new Vector2(0f, 30f), VerticalAlignment = VerticalAlignment.Bottom };
        label.AddThemeColorOverride("font_color", new Color("e8b640")); label.AddThemeFontSizeOverride("font_size", 14); _settingsBox.AddChild(label);
    }

    private void AddNote(string text)
    {
        if (_settingsBox is null) return;
        Label note = new() { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0f, 48f) };
        note.AddThemeColorOverride("font_color", new Color("8fa3ae")); _settingsBox.AddChild(note);
    }

    private MaterialLook SelectedMaterial() => _materialFamily switch
    {
        0 => _profile.Materials.PaintedHull,
        1 => _profile.Materials.StructuralEarth,
        2 => _profile.Materials.Accent,
        3 => _profile.Materials.DarkMechanic,
        4 => _profile.Materials.ToolSteel,
        5 => _profile.Materials.Rubber,
        6 => _profile.Materials.BuildingShell,
        _ => _profile.Materials.GroundRock
    };

    private int YawIndex() => Mathf.Clamp(Mathf.RoundToInt((_profile.Camera.YawDegrees - 45f) / 90f), 0, 3);

    private void SetStatus(string text, bool errorState = false)
    {
        if (_statusLabel is null) return;
        _statusLabel.Text = text;
        _statusLabel.AddThemeColorOverride("font_color", errorState ? new Color("ff725f") : new Color("9fb2bc"));
    }

    private bool ValidateLab(out int triangles)
    {
        triangles = 0;
        foreach (MeshInstance3D mesh in _unitMeshes)
            if (mesh.Mesh is Mesh source) triangles += source.GetFaces().Length / 3;
        string json = _profile.ToJson();
        bool roundTrip = M7LookProfile.TryFromJson(json, out M7LookProfile parsed, out _) && parsed.Camera.ZoomCells == _profile.Camera.ZoomCells;
        bool roles = Enum.GetValues<M7LookMaterialRole>().All(role => _roleMeshes.Any(entry => entry.Role == role));
        if (_healthBars.Count > 0)
            GD.Print($"M7 LOOK LAB HUD: projectedHealth={_healthBars[0].Root.Position} worldHealth={_healthBars[0].World} billboardHealth={_worldHealthBars.Count}");
        return _camera is { Fov: 36f } && _units.Count == 4 && _unitMeshes.Count >= 180 && triangles >= 30_000 &&
            _drills.Count == 4 && roles && _ground is not null && _tracer is not null && _fireFlames.Count >= 5 &&
            _fakeHudLayer is not null && _controlsLayer is not null && _postMaterial is not null && roundTrip &&
            _profile.Camera.ZoomCells is >= 24f and <= 72f;
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null || image.SavePng(path) != Error.Ok) return false;
        GD.Print($"M7 LOOK LAB CAPTURE: PASS path={path}");
        return true;
    }

    private static string? ParseString(string[] arguments, string key)
    {
        for (int i = 0; i + 1 < arguments.Length; i++) if (arguments[i] == key) return arguments[i + 1];
        return null;
    }

    private static string FormatValue(float value, double step) => step >= 1d ? value.ToString("0") : step >= 0.1d ? value.ToString("0.0") : value.ToString("0.###");
    private static Color WithAlpha(Color color, float alpha) => new(color.R, color.G, color.B, alpha);

    private static ShaderMaterial FogPreviewMaterial()
    {
        Shader shader = new()
        {
            Code = """
shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, cull_disabled;
void fragment() {
    float border = smoothstep(0.0, 0.16, min(min(UV.x, 1.0 - UV.x), min(UV.y, 1.0 - UV.y)));
    float noisy_front = UV.y + sin(UV.x * 15.0) * 0.055 + sin(UV.x * 31.0 + 1.7) * 0.025;
    float body = smoothstep(0.30, 0.62, noisy_front);
    ALBEDO = vec3(0.006, 0.012, 0.018);
    ALPHA = border * body * 0.76;
}
"""
        };
        return new ShaderMaterial { Shader = shader, RenderPriority = 32 };
    }

    private static StandardMaterial3D BillboardMaterial(Color color) => new()
    {
        AlbedoColor = color,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
        NoDepthTest = true,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled
    };

    private static StyleBoxFlat FlatPanel(Color color, float alpha, Color border) => new()
    {
        BgColor = WithAlpha(color, alpha), BorderColor = border,
        BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
        CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
        ContentMarginLeft = 10f, ContentMarginTop = 8f, ContentMarginRight = 10f, ContentMarginBottom = 8f
    };

    private static ArrayMesh BuildGroundMesh(int cells, float extent)
    {
        SurfaceTool surface = new(); surface.Begin(Mesh.PrimitiveType.Triangles);
        for (int z = 0; z < cells; z++)
        for (int x = 0; x < cells; x++)
        {
            float x0 = Mathf.Lerp(-extent, extent, x / (float)cells); float x1 = Mathf.Lerp(-extent, extent, (x + 1) / (float)cells);
            float z0 = Mathf.Lerp(-extent, extent, z / (float)cells); float z1 = Mathf.Lerp(-extent, extent, (z + 1) / (float)cells);
            Vector3 a = new(x0, GroundHeight(x0, z0), z0); Vector3 b = new(x1, GroundHeight(x1, z0), z0);
            Vector3 c = new(x1, GroundHeight(x1, z1), z1); Vector3 d = new(x0, GroundHeight(x0, z1), z1);
            AddTriangle(surface, a, c, b); AddTriangle(surface, a, d, c);
        }
        return surface.Commit();
    }

    private static float GroundHeight(float x, float z)
    {
        float inner = Mathf.Clamp((new Vector2(x, z).Length() - 18f) / 22f, 0f, 1f);
        return -0.05f + inner * (Mathf.Sin(x * 0.21f) * 0.38f + Mathf.Cos(z * 0.17f) * 0.31f + Mathf.Sin((x + z) * 0.09f) * 0.22f);
    }

    private static void AddTriangle(SurfaceTool surface, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 normal = (b - a).Cross(c - a).Normalized();
        surface.SetNormal(normal); surface.AddVertex(a); surface.SetNormal(normal); surface.AddVertex(b); surface.SetNormal(normal); surface.AddVertex(c);
    }

    private const string PostShader = """
shader_type spatial;
render_mode unshaded, fog_disabled, depth_test_disabled, depth_draw_never, cull_disabled;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;
uniform sampler2D normal_roughness_texture : hint_normal_roughness_texture, repeat_disable, filter_nearest;
uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float temperature = 0.0;
uniform float tint = 0.0;
uniform float vignette = 0.0;
uniform float film_grain = 0.0;
uniform float sharpen = 0.0;
uniform float posterize_levels = 0.0;
uniform float dither_amount = 0.0;
uniform bool outline_enabled = false;
uniform vec4 outline_color : source_color = vec4(0.04, 0.07, 0.1, 1.0);
uniform float outline_width = 1.5;
uniform float outline_opacity = 0.7;
uniform float depth_threshold = 0.012;
uniform float normal_threshold = 0.34;
uniform float silhouette_strength = 1.0;
uniform float crease_strength = 0.7;
uniform float distance_fade = 0.25;

void vertex() { POSITION = vec4(VERTEX.xy, 1.0, 1.0); }
float linear_depth(vec2 uv, mat4 inv_projection) {
    float depth = texture(depth_texture, uv).r;
    vec4 view = inv_projection * vec4(uv * 2.0 - 1.0, depth, 1.0);
    return -view.z / max(0.00001, view.w);
}
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
void fragment() {
    vec2 pixel = outline_width / vec2(textureSize(screen_texture, 0));
    vec3 center = texture(screen_texture, SCREEN_UV).rgb;
    vec3 north = texture(screen_texture, SCREEN_UV + vec2(0.0, pixel.y)).rgb;
    vec3 south = texture(screen_texture, SCREEN_UV - vec2(0.0, pixel.y)).rgb;
    vec3 east = texture(screen_texture, SCREEN_UV + vec2(pixel.x, 0.0)).rgb;
    vec3 west = texture(screen_texture, SCREEN_UV - vec2(pixel.x, 0.0)).rgb;
    vec3 color = center + (center * 4.0 - north - south - east - west) * sharpen;
    color *= brightness;
    color = (color - 0.5) * contrast + 0.5;
    float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = mix(vec3(luminance), color, saturation);
    color *= vec3(1.0 + temperature * 0.13, 1.0 + tint * 0.08, 1.0 - temperature * 0.13);
    color.g *= 1.0 + tint * 0.08;
    if (posterize_levels > 1.5) color = floor(color * posterize_levels + 0.5) / posterize_levels;
    float grain = hash21(FRAGCOORD.xy + vec2(TIME * 17.0, TIME * 31.0)) - 0.5;
    color += grain * film_grain;
    color += grain * dither_amount / max(2.0, posterize_levels);
    vec2 centered = SCREEN_UV * 2.0 - 1.0;
    color *= 1.0 - vignette * smoothstep(0.25, 1.35, dot(centered, centered));

    if (outline_enabled) {
        float dc = linear_depth(SCREEN_UV, INV_PROJECTION_MATRIX);
        vec4 packed_center = texture(normal_roughness_texture, SCREEN_UV);
        vec3 nc = packed_center.xyz * 2.0 - 1.0;
        vec2 offsets[4] = vec2[4](vec2(pixel.x, 0.0), vec2(-pixel.x, 0.0), vec2(0.0, pixel.y), vec2(0.0, -pixel.y));
        float depth_edge = 0.0;
        float normal_edge = 0.0;
        for (int i = 0; i < 4; i++) {
            float dn = linear_depth(SCREEN_UV + offsets[i], INV_PROJECTION_MATRIX);
            vec3 nn = texture(normal_roughness_texture, SCREEN_UV + offsets[i]).xyz * 2.0 - 1.0;
            depth_edge = max(depth_edge, abs(dc - dn) / max(1.0, min(dc, dn)));
            normal_edge = max(normal_edge, length(nc - nn));
        }
        float silhouette = smoothstep(depth_threshold, depth_threshold * 2.4, depth_edge) * silhouette_strength;
        float smooth_surface = 1.0 - smoothstep(0.64, 0.90, packed_center.w);
        float crease = smoothstep(normal_threshold, normal_threshold * 1.8, normal_edge) * crease_strength * smooth_surface;
        float fade = 1.0 / (1.0 + dc * distance_fade * 0.025);
        float edge = clamp(max(silhouette, crease) * fade * outline_opacity, 0.0, 1.0);
        color = mix(color, outline_color.rgb, edge);
    }
    ALBEDO = clamp(color, vec3(0.0), vec3(16.0));
}
""";
}
