using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class M7LookLab : Node3D
{
    private const string ModelPath = "res://Assets/M7/raider_drill_rig.glb";
    private static readonly string[] Categories =
    {
        "SCENE & CAMERA", "SHADING", "MATERIALS", "GLASS & EMISSION", "LIGHTING", "POST FX", "OUTLINE", "ANIMATION", "DESTRUCTION", "VFX", "GROUND"
    };

    private readonly List<Node3D> _units = new();
    private readonly List<PresentationAnimationRigBinding> _animationRigs = new();
    private readonly List<MeshInstance3D> _unitMeshes = new();
    private readonly List<(MeshInstance3D Mesh, M7LookMaterialRole Role)> _roleMeshes = new();
    private readonly List<(OmniLight3D Light, M7LookMaterialRole Role)> _emissionLights = new();
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
    private readonly PresentationAnimationDriver _animationDriver = new();
    private readonly PresentationDestructionDriver _destructionDriver = new();
    private PresentationVfxPool<PooledTracerEffect>? _tracerPool;
    private PresentationVfxPool<PooledParticleBurst>? _muzzlePool;
    private PresentationVfxPool<PooledParticleBurst>? _impactPool;
    private PresentationVfxPool<PooledLegoDebrisBurst>? _heroDebrisPool;
    private PresentationVfxPool<PooledParticleBurst>? _destructionDustPool;
    private Material? _tracerVfxMaterial;
    private Material? _muzzleVfxMaterial;
    private Material? _impactVfxMaterial;
    private Material? _destructionDustMaterial;
    private (string Color, float Energy, float EdgeDarkening)? _weaponMaterialKey;
    private (string Color, float Opacity)? _destructionDustMaterialKey;
    private (string FireColor, float FireEnergy, string SmokeColor, float SmokeOpacity)? _continuousVfxMaterialKey;
    private (string Color, float Opacity, float TreadScale)? _trackMaterialKey;
    private int _vfxMaterialBuildCount;
    private GpuParticles3D? _fireParticles;
    private GpuParticles3D? _smokeParticles;
    private OmniLight3D? _impactLight;
    private OmniLight3D? _fireLight;
    private MeshInstance3D? _postQuad;
    private ShaderMaterial? _postMaterial;
    private CanvasLayer? _controlsLayer;
    private PanelContainer? _controlPanel;
    private VBoxContainer? _settingsBox;
    private Label? _statusLabel;
    private Label? _poolStatsLabel;
    private Label? _destructionStatsLabel;
    private Button? _pauseButton;
    private int _category;
    private int _materialFamily;
    private bool _controlsVisible = true;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private double _time;
    private int _lastWeaponCycle = -1;
    private int _lastImpactCycle = -1;
    private int _lastDestructionCycle = -1;
    private int _activeDestructionTarget = -1;
    private float _destructionPreviewElapsed = float.MaxValue;
    private uint _destructionSequence = 1u;
    private MouseButton _cameraDragButton = MouseButton.None;
    private Vector2 _lastDragPosition;
    private string? _capturePath;
    private Vector3 _shooterMuzzle = new(2.4f, 1.45f, 4.8f);
    private Vector3 _targetPoint = new(11.4f, 1.6f, -6.6f);
    private Node3D? _burningBuilding;

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
        string? postOverride = ParseString(commandLineArgs, "--m7-look-post");
        if (postOverride == "off") _profile.Post.Enabled = false;
        else if (postOverride == "on") _profile.Post.Enabled = true;
        string? outlineOverride = ParseString(commandLineArgs, "--m7-look-outline");
        if (outlineOverride == "off") _profile.Outline.Enabled = false;
        else if (outlineOverride == "on") _profile.Outline.Enabled = true;
        _profile.Normalize();
        _defaults = _profile.Clone();

        BuildScene();
        BuildControls();
        ApplyProfile(rebuildMaterials: true);
        ProcessPriority = 1000;
        GD.Print($"M7 LOOK LAB: active schema={_profile.SchemaVersion} zoom={_profile.Camera.ZoomCells:0.##} controls={(_controlsVisible ? "visible" : "hidden")} post={(_profile.Post.Enabled ? "on" : "off")} outline={(_profile.Outline.Enabled ? "on" : "off")} Tab=controls wheel=zoom RMB=orbit MMB=pan WASD=pan F=reset Escape=return");
    }

    public override void _Process(double delta)
    {
        if (!_profile.Scene.Paused) _time += delta * _profile.Scene.AnimationSpeed;
        UpdateFreeCamera((float)delta);
        AnimateScene();
        UpdatePoolStatsLabel();
        UpdateDestructionStatsLabel();

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < 24) return;
        bool valid = ValidateLab(out int triangles);
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 LOOK LAB: PASS schema={_profile.SchemaVersion} units={_units.Count} meshes={_unitMeshes.Count} triangles={triangles} buildings=2 firing=1 burning=1 animationDrivers={_animationRigs.Count} destructionDriver=1 vfxPools=5 prewarmed=168 controls={(_controlsVisible ? "visible" : "hidden")} zoom={_profile.Camera.ZoomCells:0.##} post={(_profile.Post.Enabled ? "on" : "off")} outline={(_profile.Outline.Enabled ? "on" : "off")}");
        else
            GD.PrintErr($"M7 LOOK LAB: FAIL units={_units.Count} meshes={_unitMeshes.Count} triangles={triangles}");
        GetTree().Quit(valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.Pressed && mouse.ButtonIndex == MouseButton.WheelUp) AdjustZoom(0.92f);
            if (mouse.Pressed && mouse.ButtonIndex == MouseButton.WheelDown) AdjustZoom(1.08f);
            if (mouse.ButtonIndex is MouseButton.Right or MouseButton.Middle)
            {
                _cameraDragButton = mouse.Pressed ? mouse.ButtonIndex : MouseButton.None;
                _lastDragPosition = mouse.Position;
                GetViewport().SetInputAsHandled();
            }
        }
        if (@event is InputEventMouseMotion motion && _cameraDragButton != MouseButton.None)
        {
            Vector2 delta = motion.Position - _lastDragPosition;
            _lastDragPosition = motion.Position;
            if (_cameraDragButton == MouseButton.Right)
            {
                _profile.Camera.YawDegrees = Mathf.PosMod(_profile.Camera.YawDegrees - delta.X * 0.24f, 360f);
                _profile.Camera.PitchDegrees = Mathf.Clamp(_profile.Camera.PitchDegrees + delta.Y * 0.18f, 30f, 80f);
            }
            else
            {
                PanCamera(delta);
            }
            ApplyCamera();
            GetViewport().SetInputAsHandled();
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
        else if (key.Keycode == Key.F)
        {
            ResetView();
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

        Vector3 intactSize = new(6.4f, 3.2f, 5.0f);
        Node3D intact = BuildBuilding("IntactBuilding", new Vector3(11.4f, 0f, -6.6f), intactSize, false);
        Node3D burning = BuildBuilding("BurningBuilding", new Vector3(-10.5f, 0f, -8.7f), new Vector3(4.1f, 2.2f, 3.8f), true);
        _burningBuilding = burning;
        AddChild(intact); AddChild(burning);
        _targetPoint = SurfaceImpactPoint(intact.GlobalPosition + new Vector3(0f, 1.55f, 0f), intactSize, _shooterMuzzle);
        _units[0].LookAt(new Vector3(_targetPoint.X, _units[0].GlobalPosition.Y, _targetPoint.Z), Vector3.Up);
        _shooterMuzzle = _units[0].GlobalPosition + _units[0].GlobalBasis * new Vector3(0.85f, 1.38f, -3.35f);

        BuildGroundDetails();
        BuildCrystalCluster(new Vector3(10.2f, 0f, 6.8f));
        BuildCombatEffects();
        BuildBurningEffects(burning.GlobalPosition + new Vector3(0f, 1.9f, 0f));
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
        _animationRigs.Add(new PresentationAnimationRigBinding(unit));
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
        for (int i = 0; i < 2; i++)
        {
            float x = -6.55f + i * 1.15f;
            float z = 5.4f;
            MeshInstance3D track = new()
            {
                Name = $"TrackMark_{i}",
                Mesh = new PlaneMesh { Size = new Vector2(_profile.Ground.TracksWidth, _profile.Ground.TracksLength) },
                Position = new Vector3(x, GroundHeight(x, z) + 0.026f, z),
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
        _tracerPool = new PresentationVfxPool<PooledTracerEffect>(this, "PooledTracer", 64, _ => new PooledTracerEffect());
        _muzzlePool = new PresentationVfxPool<PooledParticleBurst>(this, "PooledMuzzle", 32,
            _ => new PooledParticleBurst(18, 0.16f, 28f, 2.5f, 7.5f, Vector3.Zero));
        _impactPool = new PresentationVfxPool<PooledParticleBurst>(this, "PooledImpact", 48,
            _ => new PooledParticleBurst(20, 0.34f, 78f, 2.0f, 8.0f, new Vector3(0f, -4.2f, 0f)));
        StandardMaterial3D debrisMaterial = DestructionDebrisMaterial();
        _heroDebrisPool = new PresentationVfxPool<PooledLegoDebrisBurst>(this, "PooledLegoDestruction", 12,
            _ => new PooledLegoDebrisBurst(debrisMaterial));
        _destructionDustPool = new PresentationVfxPool<PooledParticleBurst>(this, "PooledDestructionDust", 12,
            _ => new PooledParticleBurst(64, 1.2f, 82f, 1.2f, 5.8f, new Vector3(0f, -3.8f, 0f)));

        _impactLight = new OmniLight3D { Name = "ImpactLight", OmniRange = 3.2f, ShadowEnabled = false, Visible = false };
        AddChild(_impactLight);
    }

    private void BuildBurningEffects(Vector3 origin)
    {
        ParticleProcessMaterial fireProcess = new()
        {
            Direction = Vector3.Up,
            Spread = 24f,
            Gravity = new Vector3(0f, 1.8f, 0f),
            InitialVelocityMin = 0.7f,
            InitialVelocityMax = 2.2f,
            ScaleMin = 0.28f,
            ScaleMax = 0.82f,
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(1.25f, 0.10f, 0.80f)
        };
        _fireParticles = new GpuParticles3D
        {
            Name = "FireParticles", Position = origin, Amount = 28, Lifetime = 1.05, Randomness = 0.52f,
            ProcessMaterial = fireProcess, LocalCoords = true, Emitting = true,
            DrawPass1 = new QuadMesh { Size = new Vector2(0.62f, 1.18f) }
        };
        AddChild(_fireParticles);

        ParticleProcessMaterial smokeProcess = new()
        {
            Direction = Vector3.Up,
            Spread = 19f,
            Gravity = new Vector3(0f, 0.7f, 0f),
            InitialVelocityMin = 0.35f,
            InitialVelocityMax = 1.1f,
            ScaleMin = 0.30f,
            ScaleMax = 1.15f,
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(1.0f, 0.12f, 0.65f)
        };
        _smokeParticles = new GpuParticles3D
        {
            Name = "SmokeParticles", Position = origin + Vector3.Up * 0.7f, Amount = 20, Lifetime = 2.8,
            Randomness = 0.64f, ProcessMaterial = smokeProcess, LocalCoords = true, Emitting = true,
            DrawPass1 = new QuadMesh { Size = new Vector2(0.92f, 0.92f) }
        };
        AddChild(_smokeParticles);

        _fireLight = new OmniLight3D { Name = "FireLight", Position = origin + Vector3.Up * 0.4f, ShadowEnabled = false };
        AddChild(_fireLight);
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
        AddAction(actions, "RESET VIEW · F", ResetView);
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
        _poolStatsLabel = null;
        _destructionStatsLabel = null;
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
            case 7: BuildAnimationSettings(); break;
            case 8: BuildDestructionSettings(); break;
            case 9: BuildVfxSettings(); break;
            case 10: BuildGroundSettings(); break;
        }
    }

    private void BuildSceneSettings()
    {
        AddNote("Free inspection camera: RMB drag orbits, MMB drag pans, wheel zooms, WASD pans, Q/E rotates, F resets. The copied profile includes the exact view.");
        AddSlider("Zoom · build cells", 24, 72, 0.5, () => _profile.Camera.ZoomCells, v => _profile.Camera.ZoomCells = v, false);
        AddSlider("Pitch · degrees", 30, 80, 0.5, () => _profile.Camera.PitchDegrees, v => _profile.Camera.PitchDegrees = v, false);
        AddSlider("Yaw · degrees", 0, 359, 1, () => _profile.Camera.YawDegrees, v => _profile.Camera.YawDegrees = v, false);
        AddSlider("Focus X", -42, 42, 0.25, () => _profile.Camera.FocusX, v => _profile.Camera.FocusX = v, false);
        AddSlider("Focus Z", -42, 42, 0.25, () => _profile.Camera.FocusZ, v => _profile.Camera.FocusZ = v, false);
        AddSlider("Animation speed", 0, 2, 0.05, () => _profile.Scene.AnimationSpeed, v => _profile.Scene.AnimationSpeed = v, false);
        AddCheck("Weapon firing", () => _profile.Scene.FiringEnabled, v => _profile.Scene.FiringEnabled = v, false);
        AddCheck("Burning building", () => _profile.Scene.BurningEnabled, v => _profile.Scene.BurningEnabled = v, false);
        AddCheck("Dust / smoke", () => _profile.Scene.DustEnabled, v => _profile.Scene.DustEnabled = v, false);
        AddCheck("Fog-of-war preview", () => _profile.Scene.FogPreviewEnabled, v => _profile.Scene.FogPreviewEnabled = v, false);
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
        AddNote("Generated grayscale detail maps preserve the selected base color. Texture strength 0 disables the asset completely; scale changes its physical frequency.");
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
        AddSlider("Texture strength", 0, 1, 0.01, () => look.TextureStrength, v => look.TextureStrength = v);
        AddSlider("Texture scale", 0.05, 12, 0.05, () => look.TextureScale, v => look.TextureScale = v);
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
        AddColor("Signal color", () => _profile.Emission.SignalColor, v => _profile.Emission.SignalColor = v, false);
        AddSlider("Signal energy", 0, 12, 0.1, () => _profile.Emission.SignalEnergy, v => _profile.Emission.SignalEnergy = v, false);
        AddColor("Lamp color", () => _profile.Emission.LampColor, v => _profile.Emission.LampColor = v, false);
        AddSlider("Lamp energy", 0, 12, 0.1, () => _profile.Emission.LampEnergy, v => _profile.Emission.LampEnergy = v, false);
        AddColor("Crystal color", () => _profile.Emission.CrystalColor, v => _profile.Emission.CrystalColor = v, false);
        AddSlider("Crystal energy", 0, 12, 0.1, () => _profile.Emission.CrystalEnergy, v => _profile.Emission.CrystalEnergy = v, false);
        AddSlider("Emission pulse", 0, 1, 0.01, () => _profile.Emission.PulseAmount, v => _profile.Emission.PulseAmount = v, false);
        AddSlider("Pulse speed", 0, 8, 0.1, () => _profile.Emission.PulseSpeed, v => _profile.Emission.PulseSpeed = v, false);
        AddSlider("Halo intensity", 0, 3, 0.01, () => _profile.Emission.HaloIntensity, v => _profile.Emission.HaloIntensity = v, false);
        AddSlider("Halo size", 0.5, 4, 0.05, () => _profile.Emission.HaloSize, v => _profile.Emission.HaloSize = v, false);
        AddSlider("Darker luminous edge", 0, 1, 0.01, () => _profile.Emission.EdgeDarkening, v => _profile.Emission.EdgeDarkening = v, false);
        AddSlider("Local light energy", 0, 4, 0.02, () => _profile.Emission.LocalLightEnergy, v => _profile.Emission.LocalLightEnergy = v, false);
        AddSlider("Local light range", 0.5, 8, 0.1, () => _profile.Emission.LocalLightRange, v => _profile.Emission.LocalLightRange = v, false);
    }

    private void BuildLightingSettings()
    {
        AddHeading("KEY / SHADOW");
        AddSlider("Key azimuth", 0, 359, 1, () => _profile.Lighting.KeyAzimuth, v => _profile.Lighting.KeyAzimuth = v, false);
        AddSlider("Key elevation", 10, 85, 1, () => _profile.Lighting.KeyElevation, v => _profile.Lighting.KeyElevation = v, false);
        AddColor("Key color", () => _profile.Lighting.KeyColor, v => _profile.Lighting.KeyColor = v, false);
        AddSlider("Key energy", 0, 8, 0.05, () => _profile.Lighting.KeyEnergy, v => _profile.Lighting.KeyEnergy = v, false);
        AddSlider("Angular size", 0, 10, 0.1, () => _profile.Lighting.KeyAngularSize, v => _profile.Lighting.KeyAngularSize = v, false);
        AddSlider("Shadow blur", 0, 8, 0.1, () => _profile.Lighting.ShadowBlur, v => _profile.Lighting.ShadowBlur = v, false);
        AddSlider("Shadow opacity", 0, 1, 0.01, () => _profile.Lighting.ShadowOpacity, v => _profile.Lighting.ShadowOpacity = v, false);
        AddHeading("FILL / AMBIENT / RIM");
        AddSlider("Fill azimuth", 0, 359, 1, () => _profile.Lighting.FillAzimuth, v => _profile.Lighting.FillAzimuth = v, false);
        AddSlider("Fill elevation", 0, 85, 1, () => _profile.Lighting.FillElevation, v => _profile.Lighting.FillElevation = v, false);
        AddColor("Fill color", () => _profile.Lighting.FillColor, v => _profile.Lighting.FillColor = v, false);
        AddSlider("Fill energy", 0, 8, 0.05, () => _profile.Lighting.FillEnergy, v => _profile.Lighting.FillEnergy = v, false);
        AddColor("Ambient color", () => _profile.Lighting.AmbientColor, v => _profile.Lighting.AmbientColor = v, false);
        AddSlider("Ambient energy", 0, 4, 0.02, () => _profile.Lighting.AmbientEnergy, v => _profile.Lighting.AmbientEnergy = v, false);
        AddCheck("Rim light enabled", () => _profile.Lighting.RimLightEnabled, v => _profile.Lighting.RimLightEnabled = v, false);
        AddColor("Rim color", () => _profile.Lighting.RimColor, v => _profile.Lighting.RimColor = v, false);
        AddSlider("Rim energy", 0, 8, 0.05, () => _profile.Lighting.RimEnergy, v => _profile.Lighting.RimEnergy = v, false);
        AddNote("Far-field background affects both the clear color and distant terrain, so it remains visible in the overhead test.");
        AddColor("Far-field background", () => _profile.Lighting.BackgroundColor, v => _profile.Lighting.BackgroundColor = v, false);
        AddSlider("Background influence", 0, 1, 0.01, () => _profile.Lighting.BackgroundInfluence, v => _profile.Lighting.BackgroundInfluence = v, false);
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
        AddNote("Film grain and dither are static screen patterns. Dither is only used with posterization; neither swims over the image.");
        AddSlider("Film grain", 0, 0.3, 0.005, () => _profile.Post.FilmGrain, v => _profile.Post.FilmGrain = v, false);
        AddSlider("Film grain scale", 0.5, 4, 0.05, () => _profile.Post.GrainScale, v => _profile.Post.GrainScale = v, false);
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

    private void BuildAnimationSettings()
    {
        AddNote("The same sim-driven presentation driver used by unit views controls this rig. Preview choreography exposes locomotion, work, recoil, transformation and damage without changing gameplay state.");
        AddCheck("Animation drivers enabled", () => _profile.Animation.Enabled, v => _profile.Animation.Enabled = v, false);
        AddCheck("Four-state preview choreography", () => _profile.Animation.PreviewChoreography, v => _profile.Animation.PreviewChoreography = v, false);
        AddSlider("Preview locomotion speed", 0, 12, 0.1, () => _profile.Animation.PreviewLocomotionSpeed, v => _profile.Animation.PreviewLocomotionSpeed = v, false);
        AddSlider("Reference speed · full blend", 0.1, 20, 0.1, () => _profile.Animation.LocomotionReferenceSpeed, v => _profile.Animation.LocomotionReferenceSpeed = v, false);
        AddSlider("Blend response", 0.1, 30, 0.1, () => _profile.Animation.BlendResponse, v => _profile.Animation.BlendResponse = v, false);
        AddHeading("LOCOMOTION MECHANICS");
        AddSlider("Wheel turns / world unit", 0, 2, 0.01, () => _profile.Animation.WheelTurnsPerWorldUnit, v => _profile.Animation.WheelTurnsPerWorldUnit = v, false);
        AddSlider("Suspension travel", 0, 0.3, 0.005, () => _profile.Animation.SuspensionAmplitude, v => _profile.Animation.SuspensionAmplitude = v, false);
        AddSlider("Suspension frequency", 0, 8, 0.05, () => _profile.Animation.SuspensionFrequency, v => _profile.Animation.SuspensionFrequency = v, false);
        AddSlider("Body lean · degrees", 0, 12, 0.1, () => _profile.Animation.BodyLeanDegrees, v => _profile.Animation.BodyLeanDegrees = v, false);
        AddHeading("FUNCTION / STATE RESPONSE");
        AddSlider("Drill turns / second", 0, 5, 0.05, () => _profile.Animation.DrillTurnsPerSecond, v => _profile.Animation.DrillTurnsPerSecond = v, false);
        AddSlider("Weapon recoil distance", 0, 0.8, 0.01, () => _profile.Animation.RecoilDistance, v => _profile.Animation.RecoilDistance = v, false);
        AddSlider("Recoil recovery", 0.1, 30, 0.1, () => _profile.Animation.RecoilRecovery, v => _profile.Animation.RecoilRecovery = v, false);
        AddSlider("Transformation lift", 0, 1.5, 0.01, () => _profile.Animation.TransformationLift, v => _profile.Animation.TransformationLift = v, false);
        AddSlider("Transformation tilt", -45, 45, 0.5, () => _profile.Animation.TransformationTiltDegrees, v => _profile.Animation.TransformationTiltDegrees = v, false);
        AddSlider("Damage wobble", 0, 12, 0.1, () => _profile.Animation.DamageWobbleDegrees, v => _profile.Animation.DamageWobbleDegrees = v, false);
        AddOption("Significance tier", new[] { "Auto", "A · every frame", "B · 30 Hz", "C · 15 Hz" },
            _profile.Animation.TierOverride, i => _profile.Animation.TierOverride = i, false);
    }

    private void BuildDestructionSettings()
    {
        AddNote("Presentation-only LEGO breakup. The source body stops being a gameplay object before this local animation begins; fragments never block movement, deal damage or become selectable.");
        AddCheck("LEGO destruction enabled", () => _profile.Destruction.Enabled, v => _profile.Destruction.Enabled = v, false);
        AddCheck("Automatic loop preview", () => _profile.Destruction.AutoPreview, v => _profile.Destruction.AutoPreview = v, false);
        AddOption("Preview target", new[] { "Fourth unit", "Burning structure", "Alternate each cycle" },
            _profile.Destruction.PreviewTarget, i => _profile.Destruction.PreviewTarget = i, false);
        if (_settingsBox is not null)
            AddAction(_settingsBox, "TRIGGER DESTRUCTION NOW", TriggerDestructionPreview,
                "Runs one local presentation event even when automatic preview is disabled.");
        AddSlider("Preview loop · seconds", 2, 15, 0.1, () => _profile.Destruction.PreviewLoopSeconds, v => _profile.Destruction.PreviewLoopSeconds = v, false);
        AddSlider("Destroyed hold · seconds", 0.2, 10, 0.1, () => _profile.Destruction.PreviewHoldSeconds, v => _profile.Destruction.PreviewHoldSeconds = v, false);
        AddHeading("BODY FAILURE");
        AddSlider("Collapse duration", 0.05, 4, 0.05, () => _profile.Destruction.CollapseSeconds, v => _profile.Destruction.CollapseSeconds = v, false);
        AddSlider("Settle tilt · degrees", 0, 45, 0.5, () => _profile.Destruction.SettleTiltDegrees, v => _profile.Destruction.SettleTiltDegrees = v, false);
        AddSlider("Wreck horizontal ratio", 0.25, 1.5, 0.01, () => _profile.Destruction.WreckWidthRatio, v => _profile.Destruction.WreckWidthRatio = v, false);
        AddSlider("Wreck height ratio", 0.03, 1, 0.01, () => _profile.Destruction.WreckHeightRatio, v => _profile.Destruction.WreckHeightRatio = v, false);
        AddHeading("HERO LEGO MODULES");
        AddSlider("Active burst budget", 0, 12, 1, () => _profile.Destruction.HeroPoolBudget, v => _profile.Destruction.HeroPoolBudget = (int)v, false);
        AddSlider("Maximum hero fragments", 0, 18, 1, () => _profile.Destruction.HeroFragmentCount, v => _profile.Destruction.HeroFragmentCount = (int)v, false);
        AddSlider("Fragment scale", 0.1, 3, 0.05, () => _profile.Destruction.FragmentScale, v => _profile.Destruction.FragmentScale = v, false);
        AddSlider("Outward impulse", 0, 18, 0.1, () => _profile.Destruction.OutwardSpeed, v => _profile.Destruction.OutwardSpeed = v, false);
        AddSlider("Upward impulse", 0, 18, 0.1, () => _profile.Destruction.UpwardSpeed, v => _profile.Destruction.UpwardSpeed = v, false);
        AddSlider("Gravity", 0, 30, 0.1, () => _profile.Destruction.Gravity, v => _profile.Destruction.Gravity = v, false);
        AddSlider("Air drag", 0, 6, 0.05, () => _profile.Destruction.Drag, v => _profile.Destruction.Drag = v, false);
        AddSlider("Ground bounce", 0, 0.9, 0.01, () => _profile.Destruction.Bounce, v => _profile.Destruction.Bounce = v, false);
        AddSlider("Angular speed · degrees/s", 0, 1080, 5, () => _profile.Destruction.AngularSpeedDegrees, v => _profile.Destruction.AngularSpeedDegrees = v, false);
        AddSlider("Debris lifetime", 0.1, 15, 0.1, () => _profile.Destruction.DebrisLifetime, v => _profile.Destruction.DebrisLifetime = v, false);
        AddSlider("Final fade duration", 0, 15, 0.05, () => _profile.Destruction.FadeSeconds, v => _profile.Destruction.FadeSeconds = v, false);
        AddHeading("SECONDARY DUST");
        AddSlider("Dust burst budget", 0, 12, 1, () => _profile.Destruction.DustPoolBudget, v => _profile.Destruction.DustPoolBudget = (int)v, false);
        AddSlider("Dust particle count", 0, 64, 1, () => _profile.Destruction.DustCount, v => _profile.Destruction.DustCount = (int)v, false);
        AddSlider("Dust size", 0.05, 4, 0.05, () => _profile.Destruction.DustSize, v => _profile.Destruction.DustSize = v, false);
        AddSlider("Dust lifetime", 0.1, 5, 0.05, () => _profile.Destruction.DustLifetime, v => _profile.Destruction.DustLifetime = v, false);
        AddColor("Dust color", () => _profile.Destruction.DustColor, v => _profile.Destruction.DustColor = v, false);
        AddSlider("Dust opacity", 0, 1, 0.01, () => _profile.Destruction.DustOpacity, v => _profile.Destruction.DustOpacity = v, false);
        if (_settingsBox is not null)
        {
            _destructionStatsLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0f, 62f) };
            _destructionStatsLabel.AddThemeColorOverride("font_color", new Color("e8b640"));
            _settingsBox.AddChild(_destructionStatsLabel);
            UpdateDestructionStatsLabel();
        }
    }

    private void BuildVfxSettings()
    {
        AddNote("Weapon evidence is spawned through prewarmed production pools. Lower budgets visibly drop only cosmetic bursts; simulation and damage are unaffected.");
        AddHeading("POOL BUDGET / LOAD");
        AddSlider("Active tracer budget", 0, 64, 1, () => _profile.VfxPool.TracerBudget, v => _profile.VfxPool.TracerBudget = (int)v, false);
        AddSlider("Active muzzle budget", 0, 32, 1, () => _profile.VfxPool.MuzzleBudget, v => _profile.VfxPool.MuzzleBudget = (int)v, false);
        AddSlider("Active impact budget", 0, 48, 1, () => _profile.VfxPool.ImpactBudget, v => _profile.VfxPool.ImpactBudget = (int)v, false);
        AddSlider("Preview firing units", 1, 4, 1, () => _profile.VfxPool.PreviewEmitters, v => _profile.VfxPool.PreviewEmitters = (int)v, false);
        if (_settingsBox is not null)
        {
            _poolStatsLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0f, 48f) };
            _poolStatsLabel.AddThemeColorOverride("font_color", new Color("77d6c6"));
            _settingsBox.AddChild(_poolStatsLabel);
            UpdatePoolStatsLabel();
        }
        AddHeading("WEAPON READABILITY");
        AddColor("Tracer color", () => _profile.Vfx.TracerColor, v => _profile.Vfx.TracerColor = v, false);
        AddSlider("Tracer width", 0.01, 0.8, 0.01, () => _profile.Vfx.TracerWidth, v => _profile.Vfx.TracerWidth = v, false);
        AddSlider("Tracer length", 0.1, 8, 0.05, () => _profile.Vfx.TracerLength, v => _profile.Vfx.TracerLength = v, false);
        AddSlider("Tracer speed", 1, 50, 0.5, () => _profile.Vfx.TracerSpeed, v => _profile.Vfx.TracerSpeed = v, false);
        AddSlider("Tracer emission", 0, 12, 0.1, () => _profile.Vfx.TracerEnergy, v => _profile.Vfx.TracerEnergy = v, false);
        AddSlider("Muzzle size", 0.05, 2, 0.01, () => _profile.Vfx.MuzzleSize, v => _profile.Vfx.MuzzleSize = v, false);
        AddSlider("Impact size", 0.05, 3, 0.01, () => _profile.Vfx.ImpactSize, v => _profile.Vfx.ImpactSize = v, false);
        AddSlider("Spark count", 0, 20, 1, () => _profile.Vfx.SparkCount, v => _profile.Vfx.SparkCount = (int)v, false);
        AddSlider("Spark size", 0.01, 0.4, 0.01, () => _profile.Vfx.SparkSize, v => _profile.Vfx.SparkSize = v, false);
        AddHeading("FIRE / SMOKE");
        AddColor("Fire color", () => _profile.Vfx.FireColor, v => _profile.Vfx.FireColor = v, false);
        AddSlider("Fire size", 0.1, 4, 0.05, () => _profile.Vfx.FireSize, v => _profile.Vfx.FireSize = v, false);
        AddSlider("Fire emission", 0, 12, 0.1, () => _profile.Vfx.FireEnergy, v => _profile.Vfx.FireEnergy = v, false);
        AddSlider("Fire flicker", 0, 1, 0.01, () => _profile.Vfx.FireFlicker, v => _profile.Vfx.FireFlicker = v, false);
        AddColor("Smoke color", () => _profile.Vfx.SmokeColor, v => _profile.Vfx.SmokeColor = v, false);
        AddSlider("Smoke amount", 0, 20, 1, () => _profile.Vfx.SmokeAmount, v => _profile.Vfx.SmokeAmount = (int)v, false);
        AddSlider("Smoke opacity", 0, 1, 0.01, () => _profile.Vfx.SmokeOpacity, v => _profile.Vfx.SmokeOpacity = v, false);
        AddSlider("Smoke size", 0.1, 4, 0.05, () => _profile.Vfx.SmokeSize, v => _profile.Vfx.SmokeSize = v, false);
        AddSlider("Smoke rise", 0, 6, 0.05, () => _profile.Vfx.SmokeRise, v => _profile.Vfx.SmokeRise = v, false);
    }

    private void BuildGroundSettings()
    {
        AddColor("Secondary color", () => _profile.Ground.SecondaryColor, v => _profile.Ground.SecondaryColor = v);
        AddSlider("Macro variation", 0, 1, 0.01, () => _profile.Ground.MacroAmount, v => _profile.Ground.MacroAmount = v);
        AddSlider("Macro scale", 0.01, 2, 0.01, () => _profile.Ground.MacroScale, v => _profile.Ground.MacroScale = v);
        AddSlider("Micro variation", 0, 1, 0.01, () => _profile.Ground.MicroAmount, v => _profile.Ground.MicroAmount = v);
        AddSlider("Micro scale", 0.1, 20, 0.1, () => _profile.Ground.MicroScale, v => _profile.Ground.MicroScale = v);
        AddColor("Dust / track tint", () => _profile.Ground.DustTint, v => _profile.Ground.DustTint = v, false);
        AddSlider("Track opacity", 0, 1, 0.01, () => _profile.Ground.TracksOpacity, v => _profile.Ground.TracksOpacity = v, false);
        AddSlider("Track width", 0.2, 2, 0.02, () => _profile.Ground.TracksWidth, v => _profile.Ground.TracksWidth = v, false);
        AddSlider("Track length", 1, 16, 0.1, () => _profile.Ground.TracksLength, v => _profile.Ground.TracksLength = v, false);
        AddSlider("Track tread scale", 1, 24, 0.5, () => _profile.Ground.TrackTreadScale, v => _profile.Ground.TrackTreadScale = v, false);
        AddSlider("Unit spacing", 4, 14, 0.1, () => _profile.Ground.UnitSeparation, v => _profile.Ground.UnitSeparation = v, false);
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
        Vector3 focus = new(_profile.Camera.FocusX, 0f, _profile.Camera.FocusZ);
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
        _environment.GlowNormalized = false;
        _environment.GlowIntensity = _profile.Post.BloomIntensity;
        _environment.GlowStrength = 1.35f;
        _environment.GlowBloom = _profile.Post.BloomSpread;
        _environment.GlowHdrThreshold = _profile.Post.BloomThreshold;
        _environment.GlowHdrScale = 2.0f;
        if (_ground?.MaterialOverride is ShaderMaterial groundMaterial)
        {
            groundMaterial.SetShaderParameter("background_color", M7LookMaterialFactory.ParseColor(_profile.Lighting.BackgroundColor));
            groundMaterial.SetShaderParameter("background_influence", _profile.Lighting.BackgroundInfluence);
            groundMaterial.SetShaderParameter("ambient_floor", _profile.Lighting.AmbientEnergy * 0.55f);
        }
    }

    private void ApplyMaterials()
    {
        Dictionary<M7LookMaterialRole, Material> materials = M7LookMaterialFactory.BuildSharedMaterials(_profile);
        foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _roleMeshes)
            mesh.MaterialOverride = materials[role];
        EnsureEmissionLights();
    }

    private void ApplyPost()
    {
        if (_postMaterial is null) return;
        bool haloEnabled = _profile.Post.BloomEnabled && _profile.Emission.HaloIntensity > 0.001f;
        if (_postQuad is not null)
            _postQuad.Visible = _profile.Post.Enabled || _profile.Outline.Enabled || haloEnabled;
        _postMaterial.SetShaderParameter("post_enabled", _profile.Post.Enabled);
        _postMaterial.SetShaderParameter("brightness", _profile.Post.Brightness);
        _postMaterial.SetShaderParameter("contrast", _profile.Post.Contrast);
        _postMaterial.SetShaderParameter("saturation", _profile.Post.Saturation);
        _postMaterial.SetShaderParameter("temperature", _profile.Post.Temperature);
        _postMaterial.SetShaderParameter("tint", _profile.Post.Tint);
        _postMaterial.SetShaderParameter("vignette", _profile.Post.Vignette);
        _postMaterial.SetShaderParameter("film_grain", _profile.Post.FilmGrain);
        _postMaterial.SetShaderParameter("grain_scale", _profile.Post.GrainScale);
        _postMaterial.SetShaderParameter("sharpen", _profile.Post.Sharpen);
        _postMaterial.SetShaderParameter("posterize_levels", (float)_profile.Post.PosterizeLevels);
        _postMaterial.SetShaderParameter("dither_amount", _profile.Post.Dither);
        _postMaterial.SetShaderParameter("halo_intensity", _profile.Emission.HaloIntensity);
        _postMaterial.SetShaderParameter("halo_size", _profile.Emission.HaloSize);
        _postMaterial.SetShaderParameter("emissive_edge_darkening", _profile.Emission.EdgeDarkening);
        _postMaterial.SetShaderParameter("bloom_threshold", _profile.Post.BloomThreshold);
        _postMaterial.SetShaderParameter("bloom_enabled", _profile.Post.BloomEnabled);
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
        (string, float, float) weaponKey = (_profile.Vfx.TracerColor, _profile.Vfx.TracerEnergy,
            _profile.Emission.EdgeDarkening);
        if (_weaponMaterialKey != weaponKey)
        {
            _weaponMaterialKey = weaponKey;
            _tracerVfxMaterial = M7LookMaterialFactory.Emissive(tracerColor, _profile.Vfx.TracerEnergy, 0.92f,
                _profile.Emission.EdgeDarkening);
            Material sharedBurstMaterial = ParticleBillboardMaterial(tracerColor, _profile.Vfx.TracerEnergy, additive: true);
            _muzzleVfxMaterial = sharedBurstMaterial;
            _impactVfxMaterial = sharedBurstMaterial;
            _tracerPool?.ForEachNode(effect => effect.SetMaterial(_tracerVfxMaterial));
            _muzzlePool?.ForEachNode(effect => effect.SetMaterial(_muzzleVfxMaterial));
            _impactPool?.ForEachNode(effect => effect.SetMaterial(_impactVfxMaterial));
            _vfxMaterialBuildCount++;
        }

        (string, float) destructionDustKey = (_profile.Destruction.DustColor, _profile.Destruction.DustOpacity);
        if (_destructionDustMaterialKey != destructionDustKey)
        {
            _destructionDustMaterialKey = destructionDustKey;
            Color destructionDustColor = WithAlpha(M7LookMaterialFactory.ParseColor(_profile.Destruction.DustColor),
                _profile.Destruction.DustOpacity);
            _destructionDustMaterial = ParticleBillboardMaterial(destructionDustColor, 0f, additive: false);
            _destructionDustPool?.ForEachNode(effect => effect.SetMaterial(_destructionDustMaterial));
            _vfxMaterialBuildCount++;
        }

        if (_tracerPool is not null) _tracerPool.Budget = _profile.VfxPool.TracerBudget;
        if (_muzzlePool is not null) _muzzlePool.Budget = _profile.VfxPool.MuzzleBudget;
        if (_impactPool is not null) _impactPool.Budget = _profile.VfxPool.ImpactBudget;
        if (_heroDebrisPool is not null) _heroDebrisPool.Budget = _profile.Destruction.HeroPoolBudget;
        if (_destructionDustPool is not null) _destructionDustPool.Budget = _profile.Destruction.DustPoolBudget;

        Color fireColor = M7LookMaterialFactory.ParseColor(_profile.Vfx.FireColor);
        (string, float, string, float) continuousVfxKey = (_profile.Vfx.FireColor, _profile.Vfx.FireEnergy,
            _profile.Vfx.SmokeColor, _profile.Vfx.SmokeOpacity);
        if (_continuousVfxMaterialKey != continuousVfxKey)
        {
            _continuousVfxMaterialKey = continuousVfxKey;
            ApplyParticleDrawMaterial(_fireParticles,
                ParticleBillboardMaterial(fireColor, _profile.Vfx.FireEnergy, additive: true));
            ApplyParticleDrawMaterial(_smokeParticles,
                ParticleBillboardMaterial(WithAlpha(M7LookMaterialFactory.ParseColor(_profile.Vfx.SmokeColor),
                    _profile.Vfx.SmokeOpacity), 0f, additive: false));
            _vfxMaterialBuildCount++;
        }
        if (_fireParticles?.DrawPass1 is QuadMesh fireMesh) fireMesh.Size = new Vector2(0.24f, 0.46f) * _profile.Vfx.FireSize;
        if (_smokeParticles?.DrawPass1 is QuadMesh smokeMesh) smokeMesh.Size = Vector2.One * _profile.Vfx.SmokeSize;
        if (_smokeParticles is not null)
        {
            _smokeParticles.Amount = Mathf.Max(1, _profile.Vfx.SmokeAmount);
            _smokeParticles.AmountRatio = _profile.Vfx.SmokeAmount == 0 ? 0f : 1f;
        }
        if (_smokeParticles?.ProcessMaterial is ParticleProcessMaterial smokeProcess)
        {
            smokeProcess.Gravity = Vector3.Up * (0.25f + _profile.Vfx.SmokeRise * 0.32f);
            smokeProcess.InitialVelocityMin = 0.20f + _profile.Vfx.SmokeRise * 0.16f;
            smokeProcess.InitialVelocityMax = 0.55f + _profile.Vfx.SmokeRise * 0.34f;
        }
        if (_fireLight is not null)
        {
            _fireLight.LightColor = fireColor;
            _fireLight.LightEnergy = _profile.Vfx.FireEnergy * 0.22f;
            _fireLight.OmniRange = 3.0f + _profile.Vfx.FireSize * 1.1f;
        }
        if (_impactLight is not null)
        {
            _impactLight.LightColor = tracerColor;
            _impactLight.OmniRange = 2.2f + _profile.Vfx.ImpactSize;
        }

        (string, float, float) trackKey = (_profile.Ground.DustTint, _profile.Ground.TracksOpacity,
            _profile.Ground.TrackTreadScale);
        Material? sharedTrackMaterial = null;
        if (_trackMaterialKey != trackKey)
        {
            _trackMaterialKey = trackKey;
            sharedTrackMaterial = TrackMaterial(M7LookMaterialFactory.ParseColor(_profile.Ground.DustTint),
                _profile.Ground.TracksOpacity, _profile.Ground.TrackTreadScale);
            _vfxMaterialBuildCount++;
        }
        foreach (MeshInstance3D track in _tracks)
        {
            if (track.Mesh is PlaneMesh trackMesh) trackMesh.Size = new Vector2(_profile.Ground.TracksWidth, _profile.Ground.TracksLength);
            if (sharedTrackMaterial is not null) track.MaterialOverride = sharedTrackMaterial;
        }
        UpdatePoolStatsLabel();
    }

    private void ApplySceneVisibility()
    {
        float particleSpeed = _profile.Scene.Paused ? 0f : _profile.Scene.AnimationSpeed;
        _muzzlePool?.ForEachNode(effect => effect.Particles.SpeedScale = particleSpeed);
        _impactPool?.ForEachNode(effect => effect.Particles.SpeedScale = particleSpeed);
        _destructionDustPool?.ForEachNode(effect => effect.Particles.SpeedScale = particleSpeed);
        if (_fireParticles is not null) _fireParticles.SpeedScale = particleSpeed;
        if (_smokeParticles is not null) _smokeParticles.SpeedScale = particleSpeed;
        if (_pauseButton is not null) _pauseButton.Text = _profile.Scene.Paused ? "RESUME" : "PAUSE";
        if (_fogPreview is not null) _fogPreview.Visible = _profile.Scene.FogPreviewEnabled;
        if (!_profile.Scene.FiringEnabled)
        {
            _tracerPool?.Clear();
            _muzzlePool?.Clear();
            _impactPool?.Clear();
            if (_impactLight is not null) _impactLight.Visible = false;
        }
        if (!_profile.Destruction.Enabled)
        {
            _heroDebrisPool?.Clear();
            _destructionDustPool?.Clear();
            ResetDestructionPreview();
        }
        if (_fireParticles is not null) _fireParticles.Visible = _profile.Scene.BurningEnabled;
        if (_smokeParticles is not null) _smokeParticles.Visible = _profile.Scene.BurningEnabled && _profile.Scene.DustEnabled;
        if (_fireLight is not null) _fireLight.Visible = _profile.Scene.BurningEnabled;
    }

    private void AnimateScene()
    {
        AnimateUnits();
        AnimateDestruction();

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
            if (mesh.MaterialOverride is ShaderMaterial material)
            {
                material.SetShaderParameter("emission_color", color);
                material.SetShaderParameter("emission_energy", energy * pulse);
                material.SetShaderParameter("edge_darkening", _profile.Emission.EdgeDarkening);
            }
        }
        foreach ((OmniLight3D light, M7LookMaterialRole role) in _emissionLights)
        {
            Color color = role switch
            {
                M7LookMaterialRole.Signal => M7LookMaterialFactory.ParseColor(_profile.Emission.SignalColor),
                M7LookMaterialRole.Lamp => M7LookMaterialFactory.ParseColor(_profile.Emission.LampColor),
                _ => M7LookMaterialFactory.ParseColor(_profile.Emission.CrystalColor)
            };
            float energy = role switch
            {
                M7LookMaterialRole.Signal => _profile.Emission.SignalEnergy,
                M7LookMaterialRole.Lamp => _profile.Emission.LampEnergy,
                _ => _profile.Emission.CrystalEnergy
            };
            light.LightColor = color;
            light.LightEnergy = _profile.Emission.LocalLightEnergy * energy / 6f * pulse;
            light.OmniRange = _profile.Emission.LocalLightRange;
        }
        AnimateWeapon();
        AnimateFire();
        float vfxDelta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        _tracerPool?.Update(vfxDelta);
        _muzzlePool?.Update(vfxDelta);
        _impactPool?.Update(vfxDelta);
        _heroDebrisPool?.Update(vfxDelta);
        _destructionDustPool?.Update(vfxDelta);
    }

    private void AnimateDestruction()
    {
        if (!_profile.Destruction.Enabled)
        {
            _lastDestructionCycle = -1;
            ResetDestructionPreview();
            return;
        }

        int cycle = Mathf.FloorToInt((float)_time / _profile.Destruction.PreviewLoopSeconds);
        if (_profile.Destruction.AutoPreview && cycle != _lastDestructionCycle)
        {
            _lastDestructionCycle = cycle;
            TriggerDestructionPreview();
        }
        if (_activeDestructionTarget < 0) return;

        float delta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        _destructionPreviewElapsed += delta;
        if (_destructionPreviewElapsed >= _profile.Destruction.PreviewHoldSeconds)
        {
            ResetDestructionPreview();
            return;
        }

        uint entityKey = _activeDestructionTarget == 0 ? 1004u : 2001u;
        Node3D? target = _activeDestructionTarget == 0
            ? (_units.Count >= 4 ? _units[3] : null)
            : _burningBuilding;
        if (target is null) return;
        PresentationDestructionFrame frame = _destructionDriver.Update(entityKey, true, Vector3.One,
            delta, _profile.Destruction.ToTuning());
        float yaw = target.RotationDegrees.Y;
        target.Scale = frame.Scale;
        target.RotationDegrees = new Vector3(frame.TiltDegrees.X, yaw, frame.TiltDegrees.Z);
        UpdateBurningEffectTransform();
    }

    private void TriggerDestructionPreview()
    {
        if (!_profile.Destruction.Enabled) return;
        ResetDestructionPreview();
        int target = _profile.Destruction.PreviewTarget switch
        {
            0 => 0,
            1 => 1,
            _ => (int)(_destructionSequence & 1u)
        };
        _destructionSequence++;
        _activeDestructionTarget = target;
        _destructionPreviewElapsed = 0f;
        uint entityKey = target == 0 ? 1004u : 2001u;
        _destructionDriver.Begin(entityKey);
        SpawnLabDestruction(target, entityKey);
        SetStatus(target == 0 ? "Destruction preview · fourth unit" : "Destruction preview · burning structure");
    }

    private void SpawnLabDestruction(int targetIndex, uint entityKey)
    {
        Node3D? target = targetIndex == 0
            ? (_units.Count >= 4 ? _units[3] : null)
            : _burningBuilding;
        if (target is null) return;
        PresentationDestructionTuning tuning = _profile.Destruction.ToTuning();
        tuning.Normalize();
        PresentationDestructionScaleBand band = targetIndex == 0
            ? PresentationDestructionScaleBand.Heavy
            : PresentationDestructionScaleBand.Structure;
        int fragments = tuning.ResolveHeroFragmentCount(band);
        Vector3 sourceSize = targetIndex == 0
            ? new Vector3(4.8f, 2.4f, 6.6f)
            : new Vector3(4.1f, 2.2f, 3.8f);
        Vector3 origin = target.GlobalPosition + Vector3.Up * (sourceSize.Y * 0.42f);
        float groundY = GroundHeight(target.GlobalPosition.X, target.GlobalPosition.Z) + 0.04f;
        if (fragments > 0 && _heroDebrisPool is not null &&
            _heroDebrisPool.TryAcquire(tuning.DebrisLifetime, out PooledLegoDebrisBurst hero))
        {
            float fadeFraction = tuning.DebrisLifetime <= 0f ? 0f : tuning.FadeSeconds / tuning.DebrisLifetime;
            hero.Configure(new LegoDebrisBurstRequest(origin, sourceSize, target.GlobalRotation.Y, groundY,
                fragments, tuning.FragmentScale, tuning.OutwardSpeed, tuning.UpwardSpeed, tuning.Gravity,
                tuning.Drag, tuning.Bounce, Mathf.DegToRad(tuning.AngularSpeedDegrees), fadeFraction,
                unchecked(entityKey * 2_654_435_761u ^ _destructionSequence * 2_246_822_519u),
                M7LookMaterialFactory.ParseColor(_profile.Materials.PaintedHull.BaseColor),
                M7LookMaterialFactory.ParseColor(_profile.Materials.DarkMechanic.BaseColor),
                M7LookMaterialFactory.ParseColor(_profile.Materials.Accent.BaseColor)));
            _heroDebrisPool.Activate(hero);
        }
        if (tuning.DustCount <= 0 || _destructionDustPool is null || _destructionDustMaterial is null ||
            !_destructionDustPool.TryAcquire(tuning.DustLifetime, out PooledParticleBurst dust)) return;
        Vector3 dustOrigin = target.GlobalPosition + Vector3.Up * Math.Max(0.12f, sourceSize.Y * 0.15f);
        dust.Configure(dustOrigin, dustOrigin + Vector3.Up, _destructionDustMaterial,
            Vector2.One * tuning.DustSize, tuning.DustCount, tuning.DustLifetime);
        _destructionDustPool.Activate(dust);
    }

    private void ResetDestructionPreview()
    {
        if (_units.Count >= 4)
        {
            _units[3].Scale = Vector3.One;
            _units[3].RotationDegrees = new Vector3(0f, 315f, 0f);
        }
        if (_burningBuilding is not null)
        {
            _burningBuilding.Scale = Vector3.One;
            _burningBuilding.RotationDegrees = Vector3.Zero;
        }
        if (_activeDestructionTarget == 0) _destructionDriver.Remove(1004u);
        if (_activeDestructionTarget == 1) _destructionDriver.Remove(2001u);
        _activeDestructionTarget = -1;
        _destructionPreviewElapsed = float.MaxValue;
        UpdateBurningEffectTransform();
    }

    private void UpdateBurningEffectTransform()
    {
        if (_burningBuilding is null) return;
        float collapsedHeight = 2.2f * _burningBuilding.Scale.Y;
        Vector3 fireOrigin = _burningBuilding.GlobalPosition + Vector3.Up * Math.Max(0.24f, collapsedHeight * 0.86f);
        if (_fireParticles is not null) _fireParticles.GlobalPosition = fireOrigin;
        if (_smokeParticles is not null) _smokeParticles.GlobalPosition = fireOrigin + Vector3.Up * 0.7f;
        if (_fireLight is not null) _fireLight.GlobalPosition = fireOrigin + Vector3.Up * 0.4f;
    }

    private void AnimateUnits()
    {
        if (_animationRigs.Count != _units.Count) return;
        PresentationAnimationTuning tuning = _profile.Animation.ToTuning();
        float delta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        int weaponCycle = CurrentWeaponCycle();
        for (int i = 0; i < _animationRigs.Count; i++)
        {
            bool choreography = _profile.Animation.PreviewChoreography;
            bool moving = choreography && i == 1;
            bool operating = choreography && i == 2;
            bool transforming = choreography && i == 3;
            float transformProgress = transforming ? 0.5f - 0.5f * Mathf.Cos((float)_time * 1.25f) : 0f;
            float health = choreography && i == 3 ? 0.42f : 1f;
            uint fireSequence = i < _profile.VfxPool.PreviewEmitters && _profile.Scene.FiringEnabled
                ? checked((uint)Math.Max(0, weaponCycle + 1))
                : 0u;
            float distanceCells = _camera is null ? 0f : _camera.GlobalPosition.DistanceTo(_units[i].GlobalPosition) /
                GodotConversions.WorldUnitsPerBuildCell;
            PresentationAnimationInput input = new((uint)(i + 1), moving ? _profile.Animation.PreviewLocomotionSpeed : 0f,
                moving, operating, false, transforming, transformProgress, health, 0f, fireSequence,
                i == 0 || operating || transforming, distanceCells);
            PresentationAnimationFrame frame = _animationDriver.Update(input, delta, tuning);
            _animationRigs[i].Apply(frame, tuning);
        }
    }

    private void AnimateWeapon()
    {
        if (_tracerPool is null || _muzzlePool is null || _impactPool is null ||
            _tracerVfxMaterial is null || _muzzleVfxMaterial is null || _impactVfxMaterial is null) return;
        float distance = _shooterMuzzle.DistanceTo(_targetPoint);
        float cycleSeconds = Mathf.Max(0.65f, distance / _profile.Vfx.TracerSpeed + 0.44f);
        int cycle = CurrentWeaponCycle();
        float phase = Mathf.PosMod((float)_time, cycleSeconds) / cycleSeconds;
        float travel = Mathf.Clamp(phase * cycleSeconds * _profile.Vfx.TracerSpeed / Mathf.Max(0.01f, distance), 0f, 1f);
        float impactPulse = Mathf.Clamp(1f - Mathf.Abs(travel - 1f) * 18f, 0f, 1f);

        if (cycle != _lastWeaponCycle && _profile.Scene.FiringEnabled)
        {
            _lastWeaponCycle = cycle;
            int emitters = Math.Min(_profile.VfxPool.PreviewEmitters, _units.Count);
            for (int i = 0; i < emitters; i++)
            {
                Vector3 start = i == 0 ? _shooterMuzzle : UnitMuzzle(i);
                if (_tracerPool.TryAcquire(Math.Max(0.02f, start.DistanceTo(_targetPoint) / _profile.Vfx.TracerSpeed), out PooledTracerEffect tracer))
                {
                    tracer.Configure(start, _targetPoint, _profile.Vfx.TracerWidth, _profile.Vfx.TracerLength, _tracerVfxMaterial);
                    _tracerPool.Activate(tracer);
                }
                if (_muzzlePool.TryAcquire(0.18f, out PooledParticleBurst muzzle))
                {
                    muzzle.Configure(start, _targetPoint, _muzzleVfxMaterial,
                        new Vector2(0.28f, 0.52f) * _profile.Vfx.MuzzleSize, 18);
                    _muzzlePool.Activate(muzzle);
                }
            }
        }
        if (travel > 0.985f && cycle != _lastImpactCycle && _profile.Scene.FiringEnabled)
        {
            _lastImpactCycle = cycle;
            int emitters = Math.Min(_profile.VfxPool.PreviewEmitters, _units.Count);
            for (int i = 0; i < emitters; i++)
            {
                Vector3 source = i == 0 ? _shooterMuzzle : UnitMuzzle(i);
                if (!_impactPool.TryAcquire(0.38f, out PooledParticleBurst impact)) continue;
                float sparkDiameter = Math.Max(0.02f, _profile.Vfx.SparkSize * 3f);
                impact.Configure(_targetPoint, source, _impactVfxMaterial,
                    new Vector2(sparkDiameter, sparkDiameter * 3.4f) * _profile.Vfx.ImpactSize,
                    _profile.Vfx.SparkCount);
                _impactPool.Activate(impact);
            }
        }
        if (_impactLight is not null)
        {
            _impactLight.GlobalPosition = _targetPoint + (_shooterMuzzle - _targetPoint).Normalized() * 0.10f;
            _impactLight.Visible = _profile.Scene.FiringEnabled && impactPulse > 0.01f;
            _impactLight.LightEnergy = _profile.Vfx.TracerEnergy * impactPulse * 0.22f;
        }
    }

    private int CurrentWeaponCycle()
    {
        float distance = _shooterMuzzle.DistanceTo(_targetPoint);
        float cycleSeconds = Mathf.Max(0.65f, distance / _profile.Vfx.TracerSpeed + 0.44f);
        return Mathf.FloorToInt((float)_time / cycleSeconds);
    }

    private Vector3 UnitMuzzle(int unitIndex)
    {
        Node3D unit = _units[Math.Clamp(unitIndex, 0, _units.Count - 1)];
        unit.LookAt(new Vector3(_targetPoint.X, unit.GlobalPosition.Y, _targetPoint.Z), Vector3.Up);
        return unit.GlobalPosition + unit.GlobalBasis * new Vector3(0.85f, 1.38f, -3.35f);
    }

    private void AnimateFire()
    {
        float flicker = 1f + Mathf.Sin((float)_time * 7.7f) * _profile.Vfx.FireFlicker * 0.22f
            + Mathf.Sin((float)_time * 13.1f + 1.7f) * _profile.Vfx.FireFlicker * 0.12f;
        if (_fireLight is not null)
            _fireLight.LightEnergy = _profile.Vfx.FireEnergy * 0.22f * Mathf.Max(0.25f, flicker);
        if (_fireParticles is not null)
        {
            _fireParticles.SpeedScale = _profile.Scene.Paused ? 0f : _profile.Scene.AnimationSpeed;
            _fireParticles.AmountRatio = Mathf.Clamp(flicker, 0.35f, 1f);
        }
        if (_smokeParticles is not null)
            _smokeParticles.SpeedScale = _profile.Scene.Paused ? 0f : _profile.Scene.AnimationSpeed;
    }

    private void UpdateFreeCamera(float delta)
    {
        if (GetViewport().GuiGetFocusOwner() is LineEdit) return;
        float yaw = Mathf.DegToRad(_profile.Camera.YawDegrees);
        Vector3 screenForward = new(-Mathf.Sin(yaw), 0f, -Mathf.Cos(yaw));
        Vector3 right = new(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
        Vector3 pan = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) pan += screenForward;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) pan -= screenForward;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) pan -= right;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) pan += right;
        bool changed = false;
        if (pan.LengthSquared() > 0.0001f)
        {
            float speed = Mathf.Lerp(8f, 24f, Mathf.InverseLerp(24f, 72f, _profile.Camera.ZoomCells));
            Vector3 focus = new(_profile.Camera.FocusX, 0f, _profile.Camera.FocusZ);
            focus += pan.Normalized() * speed * delta;
            _profile.Camera.FocusX = Mathf.Clamp(focus.X, -42f, 42f);
            _profile.Camera.FocusZ = Mathf.Clamp(focus.Z, -42f, 42f);
            changed = true;
        }
        float rotate = 0f;
        if (Input.IsKeyPressed(Key.Q)) rotate -= 1f;
        if (Input.IsKeyPressed(Key.E)) rotate += 1f;
        if (rotate != 0f)
        {
            _profile.Camera.YawDegrees = Mathf.PosMod(_profile.Camera.YawDegrees + rotate * 58f * delta, 360f);
            changed = true;
        }
        if (changed) ApplyCamera();
    }

    private void PanCamera(Vector2 drag)
    {
        float yaw = Mathf.DegToRad(_profile.Camera.YawDegrees);
        Vector3 screenForward = new(-Mathf.Sin(yaw), 0f, -Mathf.Cos(yaw));
        Vector3 right = new(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
        float scale = 0.022f * _profile.Camera.ZoomCells / 35f;
        Vector3 shift = (-right * drag.X + screenForward * drag.Y) * scale;
        _profile.Camera.FocusX = Mathf.Clamp(_profile.Camera.FocusX + shift.X, -42f, 42f);
        _profile.Camera.FocusZ = Mathf.Clamp(_profile.Camera.FocusZ + shift.Z, -42f, 42f);
    }

    private void ResetView()
    {
        _profile.Camera = new CameraLook
        {
            ZoomCells = _defaults.Camera.ZoomCells,
            PitchDegrees = _defaults.Camera.PitchDegrees,
            YawDegrees = _defaults.Camera.YawDegrees,
            FocusX = _defaults.Camera.FocusX,
            FocusZ = _defaults.Camera.FocusZ
        };
        ApplyCamera();
        BuildCurrentSettings();
        SetStatus("Camera view reset");
    }

    private void EnsureEmissionLights()
    {
        if (_emissionLights.Count > 0) return;
        foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _roleMeshes)
        {
            if (role is not (M7LookMaterialRole.Signal or M7LookMaterialRole.Lamp or M7LookMaterialRole.Crystal)) continue;
            OmniLight3D light = new()
            {
                Name = $"{mesh.Name}_LocalLight",
                Position = mesh.Mesh?.GetAabb().GetCenter() ?? Vector3.Zero,
                ShadowEnabled = false,
                OmniAttenuation = 2.4f
            };
            mesh.AddChild(light);
            _emissionLights.Add((light, role));
        }
    }

    private static void ApplyParticleDrawMaterial(GpuParticles3D? particles, Material material)
    {
        if (particles?.DrawPass1 is QuadMesh quad) quad.Material = material;
    }

    private static StandardMaterial3D DestructionDebrisMaterial() => new()
    {
        AlbedoColor = Colors.White,
        VertexColorUseAsAlbedo = true,
        Metallic = 0.08f,
        Roughness = 0.48f
    };

    private static StandardMaterial3D ParticleBillboardMaterial(Color color, float energy, bool additive)
    {
        Color visible = new(color.R, color.G, color.B, Mathf.Clamp(color.A, 0f, 1f));
        Gradient gradient = new();
        gradient.SetColor(0, Colors.White);
        gradient.SetColor(1, new Color(1f, 1f, 1f, 0f));
        GradientTexture2D softDisc = new()
        {
            Gradient = gradient,
            Width = 64,
            Height = 64,
            Fill = GradientTexture2D.FillEnum.Radial,
            FillFrom = new Vector2(0.5f, 0.5f),
            FillTo = new Vector2(1f, 0.5f),
            UseHdr = true
        };
        return new StandardMaterial3D
        {
            AlbedoColor = visible,
            AlbedoTexture = softDisc,
            EmissionEnabled = energy > 0f,
            Emission = new Color(color.R, color.G, color.B),
            EmissionTexture = energy > 0f ? softDisc : null,
            EmissionEnergyMultiplier = energy,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            BlendMode = additive ? BaseMaterial3D.BlendModeEnum.Add : BaseMaterial3D.BlendModeEnum.Mix,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            NoDepthTest = false
        };
    }

    private static ShaderMaterial TrackMaterial(Color color, float opacity, float treadScale)
    {
        ShaderMaterial material = new() { Shader = new Shader { Code = TrackShader } };
        material.SetShaderParameter("track_color", color);
        material.SetShaderParameter("track_opacity", opacity);
        material.SetShaderParameter("tread_scale", treadScale);
        return material;
    }

    private static Vector3 SurfaceImpactPoint(Vector3 center, Vector3 size, Vector3 shooter)
    {
        Vector3 direction = shooter - center;
        direction.Y = 0f;
        direction = direction.LengthSquared() < 0.001f ? Vector3.Forward : direction.Normalized();
        float tx = Mathf.Abs(direction.X) < 0.001f ? float.PositiveInfinity : size.X * 0.5f / Mathf.Abs(direction.X);
        float tz = Mathf.Abs(direction.Z) < 0.001f ? float.PositiveInfinity : size.Z * 0.5f / Mathf.Abs(direction.Z);
        return center + direction * Mathf.Min(tx, tz) + direction * 0.10f;
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
        ApplySceneVisibility();
        SetStatus(_profile.Scene.Paused ? "Scene frozen for comparison" : "Scene animation resumed");
    }

    private void ResetAll()
    {
        _profile = _defaults.Clone();
        ApplyProfile(rebuildMaterials: true);
        BuildCurrentSettings();
        SetStatus("All settings reset to the reviewed profile baseline");
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
            case 7: _profile.Animation = defaults.Animation; break;
            case 8: _profile.Destruction = defaults.Destruction; break;
            case 9: _profile.Vfx = defaults.Vfx; _profile.VfxPool = defaults.VfxPool; break;
            case 10: _profile.Ground = defaults.Ground; break;
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

    private void SetStatus(string text, bool errorState = false)
    {
        if (_statusLabel is null) return;
        _statusLabel.Text = text;
        _statusLabel.AddThemeColorOverride("font_color", errorState ? new Color("ff725f") : new Color("9fb2bc"));
    }

    private void UpdatePoolStatsLabel()
    {
        if (_poolStatsLabel is null || _tracerPool is null || _muzzlePool is null || _impactPool is null) return;
        PresentationVfxPoolStats tracer = _tracerPool.GetStats();
        PresentationVfxPoolStats muzzle = _muzzlePool.GetStats();
        PresentationVfxPoolStats impact = _impactPool.GetStats();
        _poolStatsLabel.Text = $"PREWARMED {tracer.Created + muzzle.Created + impact.Created}  ·  ACTIVE {tracer.Active}/{muzzle.Active}/{impact.Active}  ·  " +
            $"PEAK {tracer.PeakActive}/{muzzle.PeakActive}/{impact.PeakActive}  ·  REUSED {tracer.Reused + muzzle.Reused + impact.Reused}  ·  " +
            $"DROPPED {tracer.Dropped + muzzle.Dropped + impact.Dropped}";
    }

    private void UpdateDestructionStatsLabel()
    {
        if (_destructionStatsLabel is null || _heroDebrisPool is null || _destructionDustPool is null) return;
        PresentationVfxPoolStats hero = _heroDebrisPool.GetStats();
        PresentationVfxPoolStats dust = _destructionDustPool.GetStats();
        int fragments = 0;
        _heroDebrisPool.ForEachNode(effect => fragments += effect.ActiveFragmentCount);
        string target = _activeDestructionTarget switch { 0 => "UNIT", 1 => "STRUCTURE", _ => "READY" };
        _destructionStatsLabel.Text = $"{target}  ·  PREWARMED {hero.Created + dust.Created}  ·  ACTIVE BURSTS {hero.Active}/{dust.Active}  ·  " +
            $"VISIBLE MODULES {fragments}  ·  PEAK {hero.PeakActive}/{dust.PeakActive}  ·  " +
            $"REUSED {hero.Reused + dust.Reused}  ·  DROPPED {hero.Dropped + dust.Dropped}";
    }

    private bool ValidateLab(out int triangles)
    {
        triangles = 0;
        foreach (MeshInstance3D mesh in _unitMeshes)
            if (mesh.Mesh is Mesh source) triangles += source.GetFaces().Length / 3;
        string json = _profile.ToJson();
        bool roundTrip = M7LookProfile.TryFromJson(json, out M7LookProfile parsed, out _) && parsed.Camera.ZoomCells == _profile.Camera.ZoomCells;
        const string schemaOneFixture = "{\"schemaVersion\":1,\"camera\":{\"zoomCells\":35},\"materials\":{\"paintedHull\":{\"baseColor\":\"#07867e\"}},\"hud\":{\"enabled\":true},\"vfx\":{\"scorchSize\":2}}";
        bool schemaOneMigration = M7LookProfile.TryFromJson(schemaOneFixture, out M7LookProfile migrated, out _) &&
            migrated.SchemaVersion == M7LookProfile.CurrentSchemaVersion && migrated.Camera.ZoomCells == 35f &&
            Mathf.IsEqualApprox(migrated.Materials.PaintedHull.TextureStrength, M7LookProfile.CreateDefault().Materials.PaintedHull.TextureStrength);
        const string schemaTwoFixture = "{\"schemaVersion\":2,\"camera\":{\"zoomCells\":72},\"vfx\":{\"tracerEnergy\":4.5}}";
        bool schemaTwoMigration = M7LookProfile.TryFromJson(schemaTwoFixture, out M7LookProfile migratedTwo, out _) &&
            migratedTwo.SchemaVersion == M7LookProfile.CurrentSchemaVersion && migratedTwo.Camera.ZoomCells == 72f &&
            Mathf.IsEqualApprox(migratedTwo.Vfx.TracerEnergy, 4.5f) && migratedTwo.Animation.Enabled;
        const string schemaThreeFixture = "{\"schemaVersion\":3,\"animation\":{\"recoilDistance\":0.22},\"vfxPool\":{\"tracerBudget\":7}}";
        bool schemaThreeMigration = M7LookProfile.TryFromJson(schemaThreeFixture, out M7LookProfile migratedThree, out _) &&
            migratedThree.SchemaVersion == M7LookProfile.CurrentSchemaVersion &&
            Mathf.IsEqualApprox(migratedThree.Animation.RecoilDistance, 0.22f) && migratedThree.VfxPool.TracerBudget == 7 &&
            migratedThree.Destruction.Enabled;
        const string partialProfileFixture = "{\"schemaVersion\":4,\"materials\":{\"paintedHull\":{\"baseColor\":\"invalid\",\"metallic\":0.71}},\"lighting\":{\"keyColor\":\"not-a-color\"}}";
        bool profileSanitization = M7LookProfile.TryFromJson(partialProfileFixture,
            out M7LookProfile sanitized, out _) && sanitized.Materials.PaintedHull.BaseColor == "#07867e" &&
            Mathf.IsEqualApprox(sanitized.Materials.PaintedHull.Metallic, 0.71f) &&
            Mathf.IsEqualApprox(sanitized.Materials.PaintedHull.Roughness, 0.38f) &&
            sanitized.Lighting.KeyColor == "#fff4e5";
        bool roles = Enum.GetValues<M7LookMaterialRole>().All(role => _roleMeshes.Any(entry => entry.Role == role));
        bool emissiveBindings = _roleMeshes.Where(entry => entry.Role is M7LookMaterialRole.Signal or M7LookMaterialRole.Lamp or M7LookMaterialRole.Crystal)
            .All(entry => entry.Mesh.MaterialOverride is ShaderMaterial);
        Vector3 intactCenter = new(11.4f, 1.55f, -6.6f);
        bool exteriorImpact = Mathf.Abs(_targetPoint.X - intactCenter.X) >= 3.15f || Mathf.Abs(_targetPoint.Z - intactCenter.Z) >= 2.45f;
        bool animationBindings = _animationRigs.Count == 4 && _animationRigs.All(rig =>
            rig.WheelCount == 6 && rig.HasDrill && rig.HasSuspension) && ValidateAnimationDriver();
        bool vfxPools = _tracerPool is { Capacity: 64 } && _muzzlePool is { Capacity: 32 } &&
            _impactPool is { Capacity: 48 } && ValidatePoolReuse();
        bool destruction = _heroDebrisPool is { Capacity: 12 } && _destructionDustPool is { Capacity: 12 } &&
            _heroDebrisPool.GetStats().Spawned > 0 && _destructionDustPool.GetStats().Spawned > 0 &&
            ValidateDestructionDriver();
        bool compositeExpected = _profile.Post.Enabled || _profile.Outline.Enabled ||
            (_profile.Post.BloomEnabled && _profile.Emission.HaloIntensity > 0.001f);
        bool postComposition = _postQuad is not null && _postQuad.Visible == compositeExpected;
        int materialBuilds = _vfxMaterialBuildCount;
        ApplyVfxAppearance();
        bool materialCacheStable = _vfxMaterialBuildCount == materialBuilds;
        float expectedParticleSpeed = _profile.Scene.Paused ? 0f : _profile.Scene.AnimationSpeed;
        bool particlePauseState = true;
        _muzzlePool?.ForEachNode(effect => particlePauseState &= Mathf.IsEqualApprox(effect.Particles.SpeedScale, expectedParticleSpeed));
        _impactPool?.ForEachNode(effect => particlePauseState &= Mathf.IsEqualApprox(effect.Particles.SpeedScale, expectedParticleSpeed));
        _destructionDustPool?.ForEachNode(effect => particlePauseState &= Mathf.IsEqualApprox(effect.Particles.SpeedScale, expectedParticleSpeed));
        bool pauseUiState = _pauseButton is not null && _pauseButton.Text == (_profile.Scene.Paused ? "RESUME" : "PAUSE");
        bool eventMemory = ValidateEventMemoryCleanup();
        bool valid = _camera is { Fov: 36f } && _units.Count == 4 && _unitMeshes.Count >= 180 && triangles >= 30_000 &&
            animationBindings && roles && _ground is not null && _fireParticles is not null && vfxPools && destruction &&
            _controlsLayer is not null && _postMaterial is not null && postComposition && materialCacheStable &&
            particlePauseState && pauseUiState && eventMemory &&
            _emissionLights.Count > 0 && emissiveBindings && exteriorImpact && roundTrip && schemaOneMigration &&
            schemaTwoMigration && schemaThreeMigration && profileSanitization &&
            ResourceLoader.Exists("res://Assets/M7/Textures/painted_shell_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/brushed_metal_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/rubber_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/quarry_ground_detail.png") &&
            _profile.Camera.ZoomCells is >= 24f and <= 72f;
        if (!valid)
        {
            GD.PrintErr($"M7 LOOK LAB AUDIT: roundTrip={roundTrip} migrations={schemaOneMigration}/{schemaTwoMigration}/{schemaThreeMigration} " +
                $"sanitization={profileSanitization} postComposition={postComposition} materialCache={materialCacheStable} " +
                $"particlePause={particlePauseState} pauseUi={pauseUiState} eventMemory={eventMemory} roles={roles} " +
                $"emission={emissiveBindings} impact={exteriorImpact} animation={animationBindings} pools={vfxPools} destruction={destruction}");
        }
        return valid;
    }

    private static bool ValidateAnimationDriver()
    {
        PresentationAnimationDriver driver = new();
        PresentationAnimationTuning tuning = new();
        PresentationAnimationFrame idle = driver.Update(new PresentationAnimationInput(1, 3f, false, false,
            false, false, 0f, 1f, 0f, 0, false, 20f), 0.05f, tuning);
        PresentationAnimationFrame active = driver.Update(new PresentationAnimationInput(1, 3f, true, true,
            false, true, 0.65f, 0.40f, 0f, 1, true, 20f), 0.05f, tuning);
        tuning.TierOverride = 3;
        PresentationAnimationFrame distant = driver.Update(new PresentationAnimationInput(1, 3f, true, false,
            false, false, 0f, 1f, 0f, 1, false, 72f), 0.01f, tuning);
        return idle.Tier == PresentationAnimationTier.Normal && idle.WheelPhaseRadians == 0f &&
            active.Tier == PresentationAnimationTier.Important && active.WheelPhaseRadians > 0f &&
            active.LocomotionBlend > 0f && active.OperationBlend > 0f && active.TransformationProgress == 0.65f &&
            active.DamageAmount > 0.5f && active.Recoil > 0f && distant.Tier == PresentationAnimationTier.Distant &&
            !distant.ParametersUpdated;
    }

    private static bool ValidateDestructionDriver()
    {
        PresentationDestructionDriver driver = new();
        PresentationDestructionTuning tuning = new() { CollapseSeconds = 0.5f, WreckWidthRatio = 0.7f,
            WreckHeightRatio = 0.12f, SettleTiltDegrees = 11f };
        driver.Begin(17u);
        PresentationDestructionFrame started = driver.Update(17u, true, Vector3.One, 0.05f, tuning);
        PresentationDestructionFrame settled = started;
        for (int i = 0; i < 12; i++) settled = driver.Update(17u, true, Vector3.One, 0.05f, tuning);
        PresentationDestructionFrame restored = driver.Update(17u, false, Vector3.One, 0.05f, tuning);
        PresentationDestructionFrame lateJoin = driver.Update(91u, true, Vector3.One, 0f, tuning);
        return started.NormalizedProgress > 0f && started.NormalizedProgress < 1f &&
            started.Scale.Y < 1f && started.TiltDegrees.LengthSquared() > 0f && settled.Settled &&
            Mathf.IsEqualApprox(settled.Scale.Y, 0.12f) && restored.Scale.IsEqualApprox(Vector3.One) &&
            !restored.Settled && lateJoin.Settled;
    }

    private bool ValidatePoolReuse()
    {
        PresentationVfxPool<PooledMeshEffect> probe = new(this, "VfxPoolProbe", 1,
            _ => new PooledMeshEffect(new BoxMesh(), M7LookMaterialFactory.Emissive(Colors.White, 1f, 1f, 0f)));
        bool first = probe.TryAcquire(0.01f, out PooledMeshEffect firstNode);
        if (first) probe.Activate(firstNode);
        probe.Update(0.02f);
        bool second = probe.TryAcquire(1f, out PooledMeshEffect secondNode);
        if (second) probe.Activate(secondNode);
        bool overflowDropped = !probe.TryAcquire(1f, out _);
        PresentationVfxPoolStats stats = probe.GetStats();
        probe.Clear();
        return first && second && overflowDropped && stats.Created == 1 && stats.PeakActive == 1 &&
            stats.Reused == 1 && stats.Dropped == 1;
    }

    private static bool ValidateEventMemoryCleanup()
    {
        PresentationEventDeduplicator deduplicator = new();
        bool seed = !deduplicator.TryAccept(10, 77u, 5u, PresentationEventFamily.WeaponFire, out _);
        bool nextAccepted = deduplicator.TryAccept(11, 77u, 6u, PresentationEventFamily.WeaponFire, out _);
        bool secondFamilySeed = !deduplicator.TryAccept(11, 77u, 1u, PresentationEventFamily.Impact, out _);
        bool tracked = deduplicator.TrackedStreamCount == 2;
        deduplicator.RemoveSource(77u);
        bool removed = deduplicator.TrackedStreamCount == 0;
        bool reseeded = !deduplicator.TryAccept(12, 77u, 1u, PresentationEventFamily.WeaponFire, out _) &&
            deduplicator.TrackedStreamCount == 1;
        return seed && nextAccepted && secondFamilySeed && tracked && removed && reseeded;
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
        surface.SetNormal(normal); surface.AddVertex(a);
        surface.SetNormal(normal); surface.AddVertex(b);
        surface.SetNormal(normal); surface.AddVertex(c);
    }

    private const string TrackShader = """
shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, cull_disabled;
uniform vec4 track_color : source_color = vec4(0.35, 0.28, 0.20, 1.0);
uniform float track_opacity = 0.35;
uniform float tread_scale = 10.0;
void fragment() {
    float side_fade = smoothstep(0.0, 0.16, min(UV.x, 1.0 - UV.x));
    float end_fade = smoothstep(0.0, 0.12, min(UV.y, 1.0 - UV.y));
    float tread_phase = fract(UV.y * tread_scale + UV.x * 0.42);
    float tread = smoothstep(0.14, 0.28, tread_phase) * (1.0 - smoothstep(0.68, 0.86, tread_phase));
    float broken = smoothstep(0.18, 0.42, fract(UV.y * 3.7 + sin(UV.x * 11.0) * 0.08));
    ALBEDO = track_color.rgb;
    ALPHA = track_opacity * side_fade * end_fade * tread * mix(0.55, 1.0, broken);
}
""";

    private const string PostShader = """
shader_type spatial;
render_mode unshaded, fog_disabled, depth_test_disabled, depth_draw_never, cull_disabled;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;
uniform sampler2D normal_roughness_texture : hint_normal_roughness_texture, repeat_disable, filter_nearest;
uniform bool post_enabled = true;
uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float temperature = 0.0;
uniform float tint = 0.0;
uniform float vignette = 0.0;
uniform float film_grain = 0.0;
uniform float grain_scale = 1.5;
uniform float sharpen = 0.0;
uniform float posterize_levels = 0.0;
uniform float dither_amount = 0.0;
uniform bool bloom_enabled = true;
uniform float bloom_threshold = 1.0;
uniform float halo_intensity = 1.0;
uniform float halo_size = 1.5;
uniform float emissive_edge_darkening = 0.45;
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
    vec3 color = center;
    if (post_enabled) color += (center * 4.0 - north - south - east - west) * sharpen;
    if (bloom_enabled && halo_intensity > 0.001) {
        vec2 halo_pixel = halo_size * 2.4 / vec2(textureSize(screen_texture, 0));
        vec2 directions[8] = vec2[8](
            vec2(1.0, 0.0), vec2(-1.0, 0.0), vec2(0.0, 1.0), vec2(0.0, -1.0),
            vec2(0.707, 0.707), vec2(-0.707, 0.707), vec2(0.707, -0.707), vec2(-0.707, -0.707));
        vec3 halo = vec3(0.0);
        float threshold = max(1.0, bloom_threshold);
        for (int i = 0; i < 8; i++) {
            vec3 sample_color = texture(screen_texture, SCREEN_UV + directions[i] * halo_pixel).rgb;
            float bright = max(max(sample_color.r, max(sample_color.g, sample_color.b)) - threshold, 0.0);
            halo += sample_color * bright;
        }
        color += halo * (halo_intensity * 0.022);
    }
    if (post_enabled) {
        color *= brightness;
        color = (color - 0.5) * contrast + 0.5;
        float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
        color = mix(vec3(luminance), color, saturation);
        color *= vec3(1.0 + temperature * 0.13, 1.0 + tint * 0.08, 1.0 - temperature * 0.13);
        color.g *= 1.0 + tint * 0.08;
        float grain = hash21(floor(FRAGCOORD.xy / max(0.5, grain_scale))) - 0.5;
        color += grain * film_grain;
        if (posterize_levels > 1.5) {
            float ordered = fract(dot(floor(FRAGCOORD.xy), vec2(0.754877666, 0.569840296))) - 0.5;
            color += ordered * dither_amount / posterize_levels;
            color = floor(color * posterize_levels + 0.5) / posterize_levels;
        }
        vec2 centered = SCREEN_UV * 2.0 - 1.0;
        color *= 1.0 - vignette * smoothstep(0.25, 1.35, dot(centered, centered));
    }

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
        vec3 luminous_reference = center;
        if (max(north.r, max(north.g, north.b)) > max(luminous_reference.r, max(luminous_reference.g, luminous_reference.b))) luminous_reference = north;
        if (max(south.r, max(south.g, south.b)) > max(luminous_reference.r, max(luminous_reference.g, luminous_reference.b))) luminous_reference = south;
        if (max(east.r, max(east.g, east.b)) > max(luminous_reference.r, max(luminous_reference.g, luminous_reference.b))) luminous_reference = east;
        if (max(west.r, max(west.g, west.b)) > max(luminous_reference.r, max(luminous_reference.g, luminous_reference.b))) luminous_reference = west;
        float luminous_peak = max(luminous_reference.r, max(luminous_reference.g, luminous_reference.b));
        float luminous_mask = smoothstep(1.0, 1.8, luminous_peak);
        vec3 luminous_outline = luminous_reference / max(1.0, luminous_peak) * mix(0.72, 0.28, emissive_edge_darkening);
        vec3 resolved_outline = mix(outline_color.rgb, luminous_outline, luminous_mask);
        color = mix(color, resolved_outline, edge);
    }
    ALBEDO = clamp(color, vec3(0.0), vec3(16.0));
}
""";
}
