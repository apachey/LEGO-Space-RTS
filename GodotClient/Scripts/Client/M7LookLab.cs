using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class M7LookLab : Node3D
{
    private const string ModelPath = "res://Assets/M7/raider_drill_rig.glb";
    private const string GlareTexturePath = "res://Assets/M7/Textures/emissive_glare.png";
    private const float GroundExtent = 300f;
    private static Texture2D? _softDiscTexture;
    private static readonly string[] Categories =
    {
        "SCENE & CAMERA", "SHADING", "MATERIALS", "GLASS & EMISSION", "LIGHTING", "WORLD LIGHT CYCLE", "POST FX", "OUTLINE", "ANIMATION", "DESTRUCTION", "VFX", "GROUND"
    };

    private readonly List<Node3D> _units = new();
    private readonly List<Node3D?> _unitMuzzleSockets = new();
    private readonly List<PresentationAnimationRigBinding> _animationRigs = new();
    private readonly List<MeshInstance3D> _unitMeshes = new();
    private readonly List<(MeshInstance3D Mesh, M7LookMaterialRole Role)> _roleMeshes = new();
    private Dictionary<M7LookMaterialRole, Material> _sharedMaterials = new();
    private readonly List<(OmniLight3D Light, M7LookMaterialRole Role)> _emissionLights = new();
    private readonly List<MeshInstance3D> _tracks = new();
    private readonly List<ColorRect> _worldGradientSwatches = new();
    private (int Environment, float Daylight, float Darkness, float Readability)? _worldGradientKey;
    private Action? _returnToPrototype;
    private M7LookProfile _profile = M7LookProfile.CreateDefault();
    private M7LookProfile _defaults = M7LookProfile.CreateDefault();
    private Camera3D? _camera;
    private DirectionalLight3D? _keyLight;
    private DirectionalLight3D? _fillLight;
    private DirectionalLight3D? _rimLight;
    private Godot.Environment? _environment;
    private MeshInstance3D? _ground;
    private readonly PresentationAnimationDriver _animationDriver = new();
    private readonly PresentationDestructionDriver _destructionDriver = new();
    private PresentationAnimationTuning _animationTuning = new();
    private PresentationDestructionTuning _destructionTuning = new();
    private PresentationVfxPool<PooledTracerEffect>? _tracerPool;
    private PresentationVfxPool<PooledParticleBurst>? _muzzlePool;
    private PresentationVfxPool<PooledParticleBurst>? _impactPool;
    private PresentationVfxPool<PooledLegoDebrisBurst>? _heroDebrisPool;
    private PresentationVfxPool<PooledParticleBurst>? _destructionDustPool;
    private PresentationVfxPool<PooledExplosionBurst>? _explosionPool;
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
    private Label? _worldPhaseLabel;
    private Button? _pauseButton;
    private int _category;
    private int _materialFamily;
    private bool _controlsVisible = true;
    private bool _smoke;
    private bool _materialAudit;
    private bool _finished;
    private int _frames;
    private int _captureFrame = 24;
    private double _time;
    private float _worldLocalLightMultiplier = 1f;
    private float _worldFunctionalLightFactor = 1f;
    private readonly List<PendingWeaponImpact> _pendingWeaponImpacts = new();
    private uint _previewShotSequence;
    private float _previewShotAccumulator;
    private bool _shotClockPrimed;
    private bool _shotTriggeredThisFrame;
    private int _previousPreviewEmitters;
    private float _impactLightRemaining;
    private float _telemetryUiElapsed;
    // Preserve the four-unit comparison on entry. Auto-destruction starts on
    // the first completed preview interval; the manual trigger remains instant.
    private int _lastDestructionCycle;
    private int _activeDestructionTarget = -1;
    private float _destructionPreviewElapsed = float.MaxValue;
    private uint _destructionSequence = 1u;
    private MouseButton _cameraDragButton = MouseButton.None;
    private Vector2 _lastDragPosition;
    private string? _capturePath;
    private Vector3 _shooterMuzzle = new(3.8f, 1.45f, 7.0f);
    private Vector3 _targetPoint = new(13f, 1.6f, -7f);
    private Node3D? _burningBuilding;

    public void Configure(Action returnToPrototype, string[] commandLineArgs)
    {
        Name = "M7LookLab";
        _returnToPrototype = returnToPrototype;
        _smoke = commandLineArgs.Contains("--m7-look-smoke");
        _controlsVisible = ParseString(commandLineArgs, "--m7-look-controls") != "hidden";
        _capturePath = ParseString(commandLineArgs, "--capture-path");
        string? profilePath = ParseString(commandLineArgs, "--m7-look-profile");
        if (!string.IsNullOrWhiteSpace(profilePath))
        {
            if (!File.Exists(profilePath))
                throw new FileNotFoundException("M7 Look Lab profile was not found.", profilePath);
            if (!M7LookProfile.TryFromJson(File.ReadAllText(profilePath), out M7LookProfile imported, out string error))
                throw new InvalidDataException($"M7 Look Lab profile is invalid: {error}");
            _profile = imported;
        }
        if (int.TryParse(ParseString(commandLineArgs, "--m7-look-capture-frame"), out int captureFrame))
            _captureFrame = Math.Clamp(captureFrame, 1, 600);
        string? materialView = ParseString(commandLineArgs, "--m7-look-material-view");
        _profile.Materials.InspectionPass = materialView switch
        {
            "combined" => 0,
            "base" => 1,
            "color" => 2,
            "relief" => 3,
            "reflection" => 4,
            _ => _profile.Materials.InspectionPass
        };
        _materialAudit = commandLineArgs.Contains("--m7-look-material-audit");
        if (_materialAudit)
        {
            _profile.Scene.FiringEnabled = false;
            _profile.Scene.BurningEnabled = false;
            _profile.Scene.DustEnabled = false;
            _profile.Animation.Enabled = false;
            _profile.Destruction.Enabled = false;
            _profile.WorldCycle.AnimatePreview = false;
            _profile.Emission.SignalPulseAmount = 0f;
            _profile.Emission.LampPulseAmount = 0f;
            _profile.Emission.CrystalPulseAmount = 0f;
        }
        if (float.TryParse(ParseString(commandLineArgs, "--m7-look-zoom"), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float zoom))
            _profile.Camera.ZoomCells = zoom;
        string? postOverride = ParseString(commandLineArgs, "--m7-look-post");
        if (postOverride == "off") _profile.Post.Enabled = false;
        else if (postOverride == "on") _profile.Post.Enabled = true;
        string? outlineOverride = ParseString(commandLineArgs, "--m7-look-outline");
        if (outlineOverride == "off") _profile.Outline.Enabled = false;
        else if (outlineOverride == "on") _profile.Outline.Enabled = true;
        string? worldOverride = ParseString(commandLineArgs, "--m7-look-world");
        if (worldOverride == "manual")
        {
            _profile.WorldCycle.Enabled = false;
            _profile.WorldCycle.AnimatePreview = false;
        }
        int worldEnvironment = worldOverride switch
        {
            "earth" => 0,
            "mars" => 1,
            "moon" => 2,
            "planet-u" => 3,
            "underground" => 4,
            _ => -1
        };
        if (worldEnvironment >= 0)
        {
            _profile.WorldCycle.Enabled = true;
            _profile.WorldCycle.AnimatePreview = false;
            ApplyWorldEnvironmentPreset(worldEnvironment);
        }
        if (float.TryParse(ParseString(commandLineArgs, "--m7-look-time"), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float localTime))
            _profile.WorldCycle.LocalTimeHours = localTime;
        _profile.Normalize();
        _defaults = _profile.Clone();

        BuildScene();
        _previousPreviewEmitters = Math.Min(_profile.VfxPool.PreviewEmitters, _units.Count);
        for (int i = 0; i < _units.Count; i++)
            _animationDriver.SynchronizeFireSequence((uint)(i + 1), 0u);
        BuildControls();
        ApplyProfile(rebuildMaterials: true);
        ProcessPriority = 1000;
        GD.Print($"M7 LOOK LAB: active schema={_profile.SchemaVersion} zoom={FormatCaptureFloat(_profile.Camera.ZoomCells)} controls={(_controlsVisible ? "visible" : "hidden")} post={(_profile.Post.Enabled ? "on" : "off")} outline={(_profile.Outline.Enabled ? "on" : "off")} world={CaptureWorldMarker()} time={CaptureTimeMarker()} phase={CapturePhaseMarker()} materialView={CaptureMaterialViewMarker()} audit={(_materialAudit ? "on" : "off")} Tab=controls wheel=zoom RMB=orbit MMB=pan WASD=pan F=reset Escape=return");
    }

    public override void _Process(double delta)
    {
        if (!_profile.Scene.Paused) _time += delta * _profile.Scene.AnimationSpeed;
        if (_profile.WorldCycle is { Enabled: true, AnimatePreview: true, Environment: not 4 } && !_profile.Scene.Paused)
        {
            float hoursPerSecond = 24f / _profile.WorldCycle.PreviewDaySeconds;
            _profile.WorldCycle.LocalTimeHours = Mathf.PosMod(
                _profile.WorldCycle.LocalTimeHours + (float)delta * _profile.Scene.AnimationSpeed * hoursPerSecond, 24f);
            ApplyLighting();
        }
        UpdateFreeCamera((float)delta);
        AnimateScene();
        _telemetryUiElapsed += (float)delta;
        if (_telemetryUiElapsed >= 0.12f)
        {
            _telemetryUiElapsed = 0f;
            UpdatePoolStatsLabel();
            UpdateDestructionStatsLabel();
        }

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < _captureFrame) return;
        bool valid = ValidateLab(out int triangles);
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 LOOK LAB: PASS schema={_profile.SchemaVersion} units={_units.Count} meshes={_unitMeshes.Count} triangles={triangles} buildings=2 firing={(_profile.Scene.FiringEnabled ? "on" : "off")} burning={(_profile.Scene.BurningEnabled ? "on" : "off")} animationDrivers={_animationRigs.Count} destructionDriver=1 vfxPools=6 prewarmed=180 controls={(_controlsVisible ? "visible" : "hidden")} zoom={FormatCaptureFloat(_profile.Camera.ZoomCells)} post={(_profile.Post.Enabled ? "on" : "off")} outline={(_profile.Outline.Enabled ? "on" : "off")} world={CaptureWorldMarker()} time={CaptureTimeMarker()} phase={CapturePhaseMarker()} materialView={CaptureMaterialViewMarker()} audit={(_materialAudit ? "on" : "off")}");
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

        _keyLight = new DirectionalLight3D
        {
            Name = "KeyLight",
            ShadowEnabled = true,
            DirectionalShadowMode = DirectionalLight3D.ShadowMode.Parallel4Splits,
            DirectionalShadowBlendSplits = true,
            DirectionalShadowSplit1 = 0.12f,
            DirectionalShadowSplit2 = 0.28f,
            DirectionalShadowSplit3 = 0.55f,
            DirectionalShadowFadeStart = 0.82f,
            DirectionalShadowMaxDistance = 72f,
            ShadowBias = 0.035f,
            ShadowNormalBias = 0.75f
        };
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
            // Keep authored terrain under every legal focus/zoom/pitch. The
            // expanded mesh preserves the prior cell density while covering
            // the far ray at 72-cell zoom and a fully panned focus.
            Mesh = BuildGroundMesh(240, GroundExtent),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_ground);
        _roleMeshes.Add((_ground, M7LookMaterialRole.GroundRock));

        float spacing = _profile.Ground.UnitSeparation;
        CreateUnit("Shooter", new Vector3(0.55f * spacing, 0.08f, 0.95f * spacing), 0f);
        CreateUnit("FacingNorthWest", new Vector3(-1.05f * spacing, 0.08f, 0.65f * spacing), 135f);
        CreateUnit("FacingSouthWest", new Vector3(-1.25f * spacing, 0.08f, -0.15f * spacing), 225f);
        CreateUnit("FacingSouthEast", new Vector3(-0.45f * spacing, 0.08f, -1.15f * spacing), 315f);

        Vector3 intactSize = new(6.4f, 3.2f, 5.0f);
        Node3D intact = BuildBuilding("IntactBuilding", new Vector3(13f, 0f, -7f), intactSize, false);
        Node3D burning = BuildBuilding("BurningBuilding", new Vector3(-13f, 0f, -10f), new Vector3(4.1f, 2.2f, 3.8f), true);
        _burningBuilding = burning;
        AddChild(intact); AddChild(burning);
        _targetPoint = SurfaceImpactPoint(intact.GlobalPosition + new Vector3(0f, 1.55f, 0f), intactSize, _shooterMuzzle);
        FaceModelForwardAt(_units[0], _targetPoint);
        _shooterMuzzle = ResolveUnitMuzzle(_units[0], _unitMuzzleSockets[0]);

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

    private void RegisterUnit(Node3D unit)
    {
        _units.Add(unit);
        _unitMuzzleSockets.Add(unit.FindChild("Tool_DrillTip", true, false) as Node3D);
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
                if (unitMesh) mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
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
                Position = new Vector3(x, GroundHeight(x, z) + 0.008f, z),
                RotationDegrees = new Vector3(0f, 28f, 0f),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(track); _tracks.Add(track);
        }
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
        _explosionPool = new PresentationVfxPool<PooledExplosionBurst>(this, "PooledExplosion", 12,
            _ => new PooledExplosionBurst());

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
            DrawPass1 = new QuadMesh { Size = new Vector2(0.62f, 1.18f) },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
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
            Name = "SmokeParticles", Position = origin + Vector3.Up * 1.55f, Amount = 20, Lifetime = 2.8,
            Randomness = 0.64f, ProcessMaterial = smokeProcess, LocalCoords = true, Emitting = true,
            DrawPass1 = new QuadMesh { Size = new Vector2(0.92f, 0.92f) },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
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
        _worldPhaseLabel = null;
        _worldGradientSwatches.Clear();
        _worldGradientKey = null;
        foreach (Node child in _settingsBox.GetChildren()) { _settingsBox.RemoveChild(child); child.QueueFree(); }
        switch (_category)
        {
            case 0: BuildSceneSettings(); break;
            case 1: BuildShadingSettings(); break;
            case 2: BuildMaterialSettings(); break;
            case 3: BuildGlassEmissionSettings(); break;
            case 4: BuildLightingSettings(); break;
            case 5: BuildWorldCycleSettings(); break;
            case 6: BuildPostSettings(); break;
            case 7: BuildOutlineSettings(); break;
            case 8: BuildAnimationSettings(); break;
            case 9: BuildDestructionSettings(); break;
            case 10: BuildVfxSettings(); break;
            case 11: BuildGroundSettings(); break;
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
        AddNote("Fog-of-war framing is deferred to its gameplay visibility fixture. The old finite black plane was removed because its edges intersected the terrain during camera orbit.");
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
        AddNote("Materials are now role-authored: paint, coated structure, blackened mechanism, bare tool steel, rubber and building panels each use their own restrained texture/normal/roughness response. This tab only changes identity color; Ground owns terrain materials. Internal map diagnostics remain available to automated captures.");
        string[] families = { "Painted hull", "Coated structure", "Accent coating", "Blackened mechanism", "Tool steel", "Rubber", "Building shell" };
        _materialFamily = Math.Clamp(_materialFamily, 0, families.Length - 1);
        AddOption("Material family", families, _materialFamily, index => { _materialFamily = index; BuildCurrentSettings(); }, rebuildMaterials: false, applyProfile: false);
        MaterialLook look = SelectedMaterial();
        AddNote(MaterialMapDescription(_materialFamily));
        AddColor("Base color", () => look.BaseColor, v => look.BaseColor = v);
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
        AddHeading("FUNCTION-SPECIFIC MOTION");
        AddNote("Work lamps remain steady and switch on automatically through dusk; they are off in daylight and always available Underground. Industrial status signals and resource crystals remain semantically independent.");
        AddSlider("Signal pulse amount", 0, 1, 0.01, () => _profile.Emission.SignalPulseAmount, v => _profile.Emission.SignalPulseAmount = v, false);
        AddSlider("Signal pulse speed", 0, 8, 0.1, () => _profile.Emission.SignalPulseSpeed, v => _profile.Emission.SignalPulseSpeed = v, false);
        AddSlider("Crystal pulse amount", 0, 1, 0.01, () => _profile.Emission.CrystalPulseAmount, v => _profile.Emission.CrystalPulseAmount = v, false);
        AddSlider("Crystal pulse speed", 0, 8, 0.1, () => _profile.Emission.CrystalPulseSpeed, v => _profile.Emission.CrystalPulseSpeed = v, false);
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
        AddNote("The backdrop also gives the terrain a restrained atmospheric tint, so changes remain visible from the overhead camera without darkening the map edges.");
        AddColor("Far-field background", () => _profile.Lighting.BackgroundColor, v => _profile.Lighting.BackgroundColor = v, false);
        AddSlider("Background influence", 0, 1, 0.01, () => _profile.Lighting.BackgroundInfluence, v => _profile.Lighting.BackgroundInfluence = v, false);
    }

    private void BuildWorldCycleSettings()
    {
        AddNote("Presentation-only lighting study: it never advances SimCore or gameplay time. Light/dark values define each world's relative cycle ratio inside the 24-hour review clock; Planet U is an authored non-canon test. Underground disables the sun and protects RTS readability with cool diffuse fill.");
        AddCheck("Use world light profile", () => _profile.WorldCycle.Enabled, v => _profile.WorldCycle.Enabled = v, false);
        AddOption("World", new[] { "Earth", "Mars", "Moon", "Planet U · authored", "Underground · fixed" },
            _profile.WorldCycle.Environment,
            index => { ApplyWorldEnvironmentPreset(index); ApplyProfile(false); BuildCurrentSettings(); },
            rebuildMaterials: false, applyProfile: false);
        _worldPhaseLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0f, 30f)
        };
        _worldPhaseLabel.AddThemeFontSizeOverride("font_size", 16);
        _worldPhaseLabel.AddThemeColorOverride("font_color", new Color("e8b640"));
        _settingsBox?.AddChild(_worldPhaseLabel);
        BuildWorldGradientStrip();
        if (_settingsBox is not null)
        {
            HFlowContainer phaseActions = new();
            _settingsBox.AddChild(phaseActions);
            AddAction(phaseActions, "NIGHT", () => SetWorldReviewTime(0f));
            AddAction(phaseActions, "BLUE HOUR", () => SetWorldReviewPhase(M7WorldLightPhase.BlueHour, true));
            AddAction(phaseActions, "DAWN", () => SetWorldReviewPhase(M7WorldLightPhase.SunriseSunset, true));
            AddAction(phaseActions, "GOLDEN", () => SetWorldReviewPhase(M7WorldLightPhase.GoldenHour, true));
            AddAction(phaseActions, "NOON", () => SetWorldReviewTime(12f));
            AddAction(phaseActions, "SUNSET", () => SetWorldReviewPhase(M7WorldLightPhase.SunriseSunset, false));
        }
        AddCheck("Animate laboratory clock", () => _profile.WorldCycle.AnimatePreview,
            v => _profile.WorldCycle.AnimatePreview = _profile.WorldCycle.Environment != 4 && v, false);
        AddSlider("Local time · hours", 0, 24, 0.05, () => _profile.WorldCycle.LocalTimeHours,
            v => _profile.WorldCycle.LocalTimeHours = v, false);
        AddSlider("Preview full day · seconds", 10, 600, 1, () => _profile.WorldCycle.PreviewDaySeconds,
            v => _profile.WorldCycle.PreviewDaySeconds = v, false);
        AddHeading("AUTHORED LIGHT / DARK RATIO");
        AddSlider("Relative light duration", 0.1, 720, 0.1, () => _profile.WorldCycle.DaylightHours,
            v => _profile.WorldCycle.DaylightHours = v, false);
        AddSlider("Relative dark duration", 0.1, 720, 0.1, () => _profile.WorldCycle.DarknessHours,
            v => _profile.WorldCycle.DarknessHours = v, false);
        AddHeading("PLAYABILITY SAFEGUARDS");
        AddSlider("Night readability floor", 0.1, 1, 0.01, () => _profile.WorldCycle.NightReadability,
            v => _profile.WorldCycle.NightReadability = v, false);
        AddSlider("Local lights at night", 0.5, 4, 0.01, () => _profile.WorldCycle.LocalLightBoost,
            v => _profile.WorldCycle.LocalLightBoost = v, false);
        ApplyLighting();
    }

    private void BuildWorldGradientStrip()
    {
        if (_settingsBox is null) return;
        HBoxContainer strip = new() { CustomMinimumSize = new Vector2(0f, 22f) };
        strip.AddThemeConstantOverride("separation", 0);
        for (int hour = 0; hour < 24; hour++)
        {
            ColorRect swatch = new()
            {
                Color = Colors.Black,
                CustomMinimumSize = new Vector2(8f, 20f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            strip.AddChild(swatch);
            _worldGradientSwatches.Add(swatch);
        }
        _settingsBox.AddChild(strip);
        RefreshWorldGradientStrip();
    }

    private void RefreshWorldGradientStrip()
    {
        if (_worldGradientSwatches.Count != 24) return;
        var key = (_profile.WorldCycle.Environment, _profile.WorldCycle.DaylightHours,
            _profile.WorldCycle.DarknessHours, _profile.WorldCycle.NightReadability);
        if (_worldGradientKey == key) return;
        _worldGradientKey = key;
        for (int hour = 0; hour < _worldGradientSwatches.Count; hour++)
        {
            M7WorldLightingFrame sample = M7WorldLightingEvaluator.Evaluate(_profile.WorldCycle, hour + 0.5f);
            _worldGradientSwatches[hour].Color = sample.BackgroundColor.Lerp(sample.AmbientColor, 0.42f);
            _worldGradientSwatches[hour].TooltipText = $"{hour:00}:30 · {sample.PhaseName}";
        }
    }

    private void SetWorldReviewTime(float hours)
    {
        _profile.WorldCycle.Enabled = true;
        _profile.WorldCycle.AnimatePreview = false;
        _profile.WorldCycle.LocalTimeHours = hours;
        ApplyProfile(false);
        BuildCurrentSettings();
    }

    private void SetWorldReviewPhase(M7WorldLightPhase requestedPhase, bool morning)
    {
        if (_profile.WorldCycle.Environment == (int)M7WorldEnvironment.Underground)
        {
            SetWorldReviewTime(12f);
            return;
        }

        float daylightSpan = 24f * _profile.WorldCycle.DaylightHours /
            (_profile.WorldCycle.DaylightHours + _profile.WorldCycle.DarknessHours);
        float darknessSpan = 24f - daylightSpan;
        float sunrise = 12f - daylightSpan * 0.5f;
        float sunset = 12f + daylightSpan * 0.5f;
        float targetAltitude = requestedPhase switch
        {
            M7WorldLightPhase.BlueHour => -8f,
            M7WorldLightPhase.SunriseSunset => -0.5f,
            M7WorldLightPhase.GoldenHour => 9f,
            _ => 12f
        };
        float bestTime = 12f;
        float bestScore = float.PositiveInfinity;
        // Sample normalized daylight and darkness arcs independently. Fixed
        // clock increments can entirely skip twilight when the authored
        // light/dark ratio makes one arc only a few seconds long.
        for (int arc = 0; arc < 2; arc++)
        for (int i = 0; i <= 256; i++)
        {
            float progress = i / 256f;
            float time = arc == 0
                ? sunrise + progress * daylightSpan
                : Mathf.PosMod(sunset + progress * darknessSpan, 24f);
            M7WorldLightingFrame frame = M7WorldLightingEvaluator.Evaluate(_profile.WorldCycle, time);
            if (frame.Phase != requestedPhase || frame.IsMorning != morning) continue;
            float score = Mathf.Abs(frame.SolarAltitude - targetAltitude);
            if (score >= bestScore) continue;
            bestScore = score;
            bestTime = time;
        }
        SetWorldReviewTime(float.IsFinite(bestScore) ? bestTime : 12f);
    }

    private void ApplyWorldEnvironmentPreset(int environment)
    {
        _profile.WorldCycle.Environment = environment;
        switch (environment)
        {
            case 0:
                _profile.WorldCycle.DaylightHours = 12f; _profile.WorldCycle.DarknessHours = 12f;
                _profile.WorldCycle.NightReadability = 0.58f; _profile.WorldCycle.LocalLightBoost = 1.35f;
                break;
            case 1:
                _profile.WorldCycle.DaylightHours = 12.33f; _profile.WorldCycle.DarknessHours = 12.33f;
                _profile.WorldCycle.NightReadability = 0.60f; _profile.WorldCycle.LocalLightBoost = 1.45f;
                break;
            case 2:
                _profile.WorldCycle.DaylightHours = 354.37f; _profile.WorldCycle.DarknessHours = 354.37f;
                _profile.WorldCycle.NightReadability = 0.64f; _profile.WorldCycle.LocalLightBoost = 1.75f;
                break;
            case 3:
                _profile.WorldCycle.DaylightHours = 18f; _profile.WorldCycle.DarknessHours = 12f;
                _profile.WorldCycle.NightReadability = 0.60f; _profile.WorldCycle.LocalLightBoost = 1.65f;
                break;
            default:
                _profile.WorldCycle.AnimatePreview = false;
                _profile.WorldCycle.NightReadability = 0.68f; _profile.WorldCycle.LocalLightBoost = 2.2f;
                break;
        }
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
        AddNote("The same sim-driven presentation driver used by unit views controls this rig. Preview choreography exposes locomotion, work, recoil and transformation without inventing a generic hit wobble.");
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
        AddSlider("Weapon recoil travel", 0, 0.35, 0.005, () => _profile.Animation.RecoilDistance, v => _profile.Animation.RecoilDistance = v, false);
        AddSlider("Recoil return speed", 1, 18, 0.1, () => _profile.Animation.RecoilRecovery, v => _profile.Animation.RecoilRecovery = v, false);
        AddSlider("Transformation lift", 0, 1.5, 0.01, () => _profile.Animation.TransformationLift, v => _profile.Animation.TransformationLift = v, false);
        AddSlider("Transformation tilt", -45, 45, 0.5, () => _profile.Animation.TransformationTiltDegrees, v => _profile.Animation.TransformationTiltDegrees = v, false);
        AddOption("Significance tier", new[] { "Auto", "A · every frame", "B · 30 Hz", "C · 15 Hz" },
            _profile.Animation.TierOverride, i => _profile.Animation.TierOverride = i, false);
    }

    private void BuildDestructionSettings()
    {
        AddNote("Presentation-only LEGO breakup. The intact source disappears at the burst instead of sinking or shrinking; pooled modules and dust carry the readable destruction. Fragments never block movement, deal damage or become selectable.");
        AddCheck("LEGO destruction enabled", () => _profile.Destruction.Enabled, v => _profile.Destruction.Enabled = v, false);
        AddCheck("Automatic loop preview", () => _profile.Destruction.AutoPreview, v => _profile.Destruction.AutoPreview = v, false);
        AddOption("Preview target", new[] { "Fourth unit", "Burning structure", "Alternate each cycle" },
            _profile.Destruction.PreviewTarget, i => _profile.Destruction.PreviewTarget = i, false);
        if (_settingsBox is not null)
            AddAction(_settingsBox, "TRIGGER DESTRUCTION NOW", TriggerDestructionPreview,
                "Runs one local presentation event even when automatic preview is disabled.");
        AddSlider("Preview loop · seconds", 2, 15, 0.1, () => _profile.Destruction.PreviewLoopSeconds, v => _profile.Destruction.PreviewLoopSeconds = v, false);
        AddSlider("Destroyed hold · seconds", 0.2, 14.6, 0.1, () => _profile.Destruction.PreviewHoldSeconds, v => _profile.Destruction.PreviewHoldSeconds = v, false);
        AddHeading("INITIAL BLAST");
        AddSlider("Active blast budget", 0, 12, 1, () => _profile.Destruction.BlastPoolBudget, v => _profile.Destruction.BlastPoolBudget = (int)v, false);
        AddColor("Blast color", () => _profile.Destruction.BlastColor, v => _profile.Destruction.BlastColor = v, false);
        AddSlider("Blast visual size", 0.1, 4, 0.05, () => _profile.Destruction.BlastVisualScale, v => _profile.Destruction.BlastVisualScale = v, false);
        AddSlider("Hot core emission", 0, 24, 0.1, () => _profile.Destruction.BlastEmissionEnergy, v => _profile.Destruction.BlastEmissionEnergy = v, false);
        AddSlider("Blast illumination", 0, 24, 0.1, () => _profile.Destruction.BlastLightEnergy, v => _profile.Destruction.BlastLightEnergy = v, false);
        AddSlider("Blast light reach", 0.5, 8, 0.1, () => _profile.Destruction.BlastLightRangeMultiplier, v => _profile.Destruction.BlastLightRangeMultiplier = v, false);
        AddSlider("Hot flash persistence", 0.5, 1.8, 0.01, () => _profile.Destruction.BlastFlashPersistence, v => _profile.Destruction.BlastFlashPersistence = v, false);
        AddSlider("Smoke arrival delay", 0, 0.55, 0.01, () => _profile.Destruction.BlastSmokeDelay, v => _profile.Destruction.BlastSmokeDelay = v, false);
        AddSlider("Blast smoke prominence", 0, 1, 0.01, () => _profile.Destruction.BlastSmokeProminence, v => _profile.Destruction.BlastSmokeProminence = v, false);
        AddSlider("Ground shock visibility", 0, 1, 0.01, () => _profile.Destruction.BlastRingStrength, v => _profile.Destruction.BlastRingStrength = v, false);
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
        AddSlider("Preview shot interval", 0.25, 6, 0.05, () => _profile.Vfx.PreviewShotInterval, v => _profile.Vfx.PreviewShotInterval = v, false);
        AddSlider("Muzzle size", 0.05, 2, 0.01, () => _profile.Vfx.MuzzleSize, v => _profile.Vfx.MuzzleSize = v, false);
        AddSlider("Impact size", 0.05, 3, 0.01, () => _profile.Vfx.ImpactSize, v => _profile.Vfx.ImpactSize = v, false);
        AddSlider("Impact light energy", 0, 20, 0.1, () => _profile.Vfx.ImpactLightEnergy, v => _profile.Vfx.ImpactLightEnergy = v, false);
        AddSlider("Impact flash duration", 0.02, 0.4, 0.01, () => _profile.Vfx.ImpactLightSeconds, v => _profile.Vfx.ImpactLightSeconds = v, false);
        AddSlider("Spark count", 0, 20, 1, () => _profile.Vfx.SparkCount, v => _profile.Vfx.SparkCount = (int)v, false);
        AddSlider("Spark size", 0.01, 0.4, 0.01, () => _profile.Vfx.SparkSize, v => _profile.Vfx.SparkSize = v, false);
        AddHeading("FIRE / SMOKE");
        AddColor("Fire color", () => _profile.Vfx.FireColor, v => _profile.Vfx.FireColor = v, false);
        AddSlider("Fire size", 0.1, 4, 0.05, () => _profile.Vfx.FireSize, v => _profile.Vfx.FireSize = v, false);
        AddSlider("Fire emission", 0, 12, 0.1, () => _profile.Vfx.FireEnergy, v => _profile.Vfx.FireEnergy = v, false);
        AddSlider("Fire flicker", 0, 1, 0.01, () => _profile.Vfx.FireFlicker, v => _profile.Vfx.FireFlicker = v, false);
        AddSlider("Fire illumination", 0, 12, 0.1, () => _profile.Vfx.FireLightEnergy, v => _profile.Vfx.FireLightEnergy = v, false);
        AddSlider("Fire light reach", 1, 18, 0.1, () => _profile.Vfx.FireLightRange, v => _profile.Vfx.FireLightRange = v, false);
        AddColor("Smoke color", () => _profile.Vfx.SmokeColor, v => _profile.Vfx.SmokeColor = v, false);
        AddSlider("Smoke amount", 0, 20, 1, () => _profile.Vfx.SmokeAmount, v => _profile.Vfx.SmokeAmount = (int)v, false);
        AddSlider("Smoke opacity", 0, 1, 0.01, () => _profile.Vfx.SmokeOpacity, v => _profile.Vfx.SmokeOpacity = v, false);
        AddSlider("Smoke size", 0.1, 4, 0.05, () => _profile.Vfx.SmokeSize, v => _profile.Vfx.SmokeSize = v, false);
        AddSlider("Smoke rise", 0, 6, 0.05, () => _profile.Vfx.SmokeRise, v => _profile.Vfx.SmokeRise = v, false);
    }

    private void BuildGroundSettings()
    {
        AddNote("Large-scale terrain breakup lives here. Authored texture blend, physical relief and roughness are controlled under Materials → Ground rock, so the two tabs do not expose duplicate sliders.");
        AddColor("Secondary color", () => _profile.Ground.SecondaryColor, v => _profile.Ground.SecondaryColor = v);
        AddSlider("Broad patch contrast", 0, 1, 0.01, () => _profile.Ground.MacroAmount, v => _profile.Ground.MacroAmount = v);
        AddSlider("Broad patch frequency", 0.01, 2, 0.01, () => _profile.Ground.MacroScale, v => _profile.Ground.MacroScale = v);
        AddSlider("Small mineral breakup", 0, 1, 0.01, () => _profile.Ground.MicroAmount, v => _profile.Ground.MicroAmount = v);
        AddSlider("Small breakup density", 0.1, 20, 0.1, () => _profile.Ground.MicroScale, v => _profile.Ground.MicroScale = v);
        AddColor("Dust / track tint", () => _profile.Ground.DustTint, v => _profile.Ground.DustTint = v, false);
        AddSlider("Track depression visibility", 0, 1, 0.01, () => _profile.Ground.TracksOpacity, v => _profile.Ground.TracksOpacity = v, false);
        AddSlider("Track width", 0.2, 2, 0.02, () => _profile.Ground.TracksWidth, v => _profile.Ground.TracksWidth = v, false);
        AddSlider("Track length", 1, 16, 0.1, () => _profile.Ground.TracksLength, v => _profile.Ground.TracksLength = v, false);
        AddSlider("Track tread scale", 1, 24, 0.5, () => _profile.Ground.TrackTreadScale, v => _profile.Ground.TrackTreadScale = v, false);
        AddSlider("Unit spacing", 4, 14, 0.1, () => _profile.Ground.UnitSeparation, v => _profile.Ground.UnitSeparation = v, false);
    }

    private void ApplyProfile(bool rebuildMaterials)
    {
        _profile.Normalize();
        _animationTuning = _profile.Animation.ToTuning();
        _animationTuning.Normalize();
        _destructionTuning = _profile.Destruction.ToTuning();
        _destructionTuning.Normalize();
        ApplyUnitLayout();
        ApplyCamera();
        if (rebuildMaterials) ApplyMaterials();
        ApplyLighting();
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
            new(0.55f * spacing, 0.08f, 0.95f * spacing),
            new(-1.05f * spacing, 0.08f, 0.65f * spacing),
            new(-1.25f * spacing, 0.08f, -0.15f * spacing),
            new(-0.45f * spacing, 0.08f, -1.15f * spacing)
        };
        for (int i = 0; i < _units.Count; i++) _units[i].Position = positions[i];
        FaceModelForwardAt(_units[0], _targetPoint);
        _units[1].RotationDegrees = new Vector3(0f, 135f, 0f);
        _units[2].RotationDegrees = new Vector3(0f, 225f, 0f);
        _units[3].RotationDegrees = new Vector3(0f, 315f, 0f);
        _shooterMuzzle = ResolveUnitMuzzle(_units[0], _unitMuzzleSockets[0]);
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
        if (_keyLight is not null)
        {
            // Fit cascades to what the RTS camera can actually see instead of
            // spreading a fixed 180 m map across mostly empty terrain.
            _keyLight.DirectionalShadowMaxDistance = Mathf.Clamp(distance * 1.4f, 55f, 140f);
        }
        // The rim is authored relative to the review camera. Keep it in sync
        // during RMB/Q/E orbit so materials do not appear to change merely
        // because the light was left behind in its old world direction.
        if (_rimLight is not null)
            _rimLight.RotationDegrees = new Vector3(-28f, _profile.Camera.YawDegrees + 180f, 0f);

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
        M7WorldLightingFrame frame = _profile.WorldCycle.Enabled
            ? M7WorldLightingEvaluator.Evaluate(_profile.WorldCycle)
            : ManualLightingFrame();
        _worldLocalLightMultiplier = frame.LocalLightMultiplier;
        _worldFunctionalLightFactor = frame.FunctionalLightFactor;
        _keyLight.Visible = frame.KeyEnergy > 0.001f;
        _keyLight.RotationDegrees = new Vector3(-frame.KeyElevation, frame.KeyAzimuth, 0f);
        _keyLight.LightColor = frame.KeyColor;
        _keyLight.LightEnergy = frame.KeyEnergy;
        // Godot's variable-penumbra sampler dithers at RTS scale. Stable PCF
        // blur gives a cleaner soft shadow without crawling pixel clusters.
        _keyLight.LightAngularDistance = 0f;
        _keyLight.ShadowBlur = frame.ShadowBlur;
        _keyLight.ShadowOpacity = frame.ShadowOpacity;
        _fillLight.RotationDegrees = new Vector3(-frame.FillElevation, frame.FillAzimuth, 0f);
        _fillLight.LightColor = frame.FillColor;
        _fillLight.LightEnergy = frame.FillEnergy;
        _rimLight.Visible = frame.RimEnabled;
        _rimLight.RotationDegrees = new Vector3(-28f, _profile.Camera.YawDegrees + 180f, 0f);
        _rimLight.LightColor = frame.RimColor;
        _rimLight.LightEnergy = frame.RimEnergy;
        _environment.BackgroundColor = frame.BackgroundColor;
        _environment.AmbientLightColor = frame.AmbientColor;
        _environment.AmbientLightEnergy = frame.AmbientEnergy;
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
            groundMaterial.SetShaderParameter("background_color", frame.BackgroundColor);
            groundMaterial.SetShaderParameter("background_influence", _profile.Lighting.BackgroundInfluence);
        }
        if (_worldPhaseLabel is not null)
            _worldPhaseLabel.Text = _profile.WorldCycle.Enabled
                ? $"{frame.PhaseName.ToUpperInvariant()}  ·  {frame.SolarAltitude:+0.0;-0.0;0.0}°  ·  {_profile.WorldCycle.LocalTimeHours:00.00}"
                : "MANUAL LIGHTING PROFILE";
        RefreshWorldGradientStrip();
    }

    private M7WorldLightingFrame ManualLightingFrame()
    {
        return new M7WorldLightingFrame(M7WorldEnvironment.Earth, M7WorldLightPhase.NeutralDay,
            "Manual", false, _profile.Lighting.KeyElevation,
            _profile.Lighting.KeyAzimuth, _profile.Lighting.KeyElevation,
            M7LookMaterialFactory.ParseColor(_profile.Lighting.KeyColor), _profile.Lighting.KeyEnergy,
            _profile.Lighting.FillAzimuth, _profile.Lighting.FillElevation,
            M7LookMaterialFactory.ParseColor(_profile.Lighting.FillColor), _profile.Lighting.FillEnergy,
            M7LookMaterialFactory.ParseColor(_profile.Lighting.AmbientColor), _profile.Lighting.AmbientEnergy,
            _profile.Lighting.RimLightEnabled, M7LookMaterialFactory.ParseColor(_profile.Lighting.RimColor),
            _profile.Lighting.RimEnergy, M7LookMaterialFactory.ParseColor(_profile.Lighting.BackgroundColor),
            _profile.Lighting.KeyAngularSize, _profile.Lighting.ShadowBlur, _profile.Lighting.ShadowOpacity,
            1f, 0f, 0f, 1f, 1f);
    }

    private float ResolveLocalLightBoost() => _profile.WorldCycle.Enabled ? _worldLocalLightMultiplier : 1f;

    private void ApplyMaterials()
    {
        _sharedMaterials = M7LookMaterialFactory.BuildSharedMaterials(_profile);
        foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _roleMeshes)
        {
            mesh.MaterialOverride = _sharedMaterials[role];
            if (role is M7LookMaterialRole.Signal or M7LookMaterialRole.Lamp or M7LookMaterialRole.Crystal)
                mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        }
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
        // World palettes already author their own warm/cool transition. A
        // global white-balance filter must not flatten that gradient.
        float postTemperature = _profile.WorldCycle.Enabled
            ? _profile.Post.Temperature * 0.25f
            : _profile.Post.Temperature;
        _postMaterial.SetShaderParameter("temperature", postTemperature);
        _postMaterial.SetShaderParameter("tint", _profile.Post.Tint);
        _postMaterial.SetShaderParameter("vignette", _profile.Post.Vignette);
        _postMaterial.SetShaderParameter("film_grain", _profile.Post.FilmGrain);
        _postMaterial.SetShaderParameter("grain_scale", _profile.Post.GrainScale);
        // The lab slider is perceptual: 1.0 is a useful clarity pass, not a
        // raw 100% Laplacian kernel that explodes texture noise.
        _postMaterial.SetShaderParameter("sharpen", _profile.Post.Sharpen * 0.14f);
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
            Material sharedBurstMaterial = ParticleBillboardMaterial(tracerColor, _profile.Vfx.TracerEnergy,
                additive: true, useAuthoredGlare: true);
            _muzzleVfxMaterial = sharedBurstMaterial;
            _impactVfxMaterial = ParticleBillboardMaterial(tracerColor.Lerp(Colors.White, 0.58f),
                _profile.Vfx.TracerEnergy, additive: true, useAuthoredGlare: true);
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
        if (_explosionPool is not null) _explosionPool.Budget = _profile.Destruction.BlastPoolBudget;

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
            _fireLight.LightEnergy = _profile.Vfx.FireLightEnergy;
            _fireLight.OmniRange = _profile.Vfx.FireLightRange;
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
        if (!_profile.Scene.FiringEnabled)
        {
            _tracerPool?.Clear();
            _muzzlePool?.Clear();
            _impactPool?.Clear();
            _pendingWeaponImpacts.Clear();
            _impactLightRemaining = 0f;
            if (_impactLight is not null) _impactLight.Visible = false;
        }
        if (!_profile.Destruction.Enabled)
        {
            _heroDebrisPool?.Clear();
            _destructionDustPool?.Clear();
            _explosionPool?.Clear();
            ResetDestructionPreview();
        }
        ApplyBurningEffectVisibility();
    }

    private void AnimateScene()
    {
        float vfxDelta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        AdvancePreviewShotClock(vfxDelta);
        AnimateUnits();
        AnimateDestruction();

        Color signalColor = M7LookMaterialFactory.ParseColor(_profile.Emission.SignalColor);
        Color lampColor = M7LookMaterialFactory.ParseColor(_profile.Emission.LampColor);
        Color crystalColor = M7LookMaterialFactory.ParseColor(_profile.Emission.CrystalColor);
        float signalPulse = ResolveEmissionPulse(M7LookMaterialRole.Signal);
        float lampPulse = ResolveEmissionPulse(M7LookMaterialRole.Lamp);
        float crystalPulse = ResolveEmissionPulse(M7LookMaterialRole.Crystal);
        UpdateSharedEmissionMaterial(M7LookMaterialRole.Signal, signalColor,
            _profile.Emission.SignalEnergy * signalPulse);
        UpdateSharedEmissionMaterial(M7LookMaterialRole.Lamp, lampColor,
            _profile.Emission.LampEnergy * lampPulse * _worldFunctionalLightFactor);
        UpdateSharedEmissionMaterial(M7LookMaterialRole.Crystal, crystalColor,
            _profile.Emission.CrystalEnergy * crystalPulse);
        foreach ((OmniLight3D light, M7LookMaterialRole role) in _emissionLights)
        {
            Color color = role switch
            {
                M7LookMaterialRole.Signal => signalColor,
                M7LookMaterialRole.Lamp => lampColor,
                _ => crystalColor
            };
            float energy = role switch
            {
                M7LookMaterialRole.Signal => _profile.Emission.SignalEnergy,
                M7LookMaterialRole.Lamp => _profile.Emission.LampEnergy,
                _ => _profile.Emission.CrystalEnergy
            };
            float pulse = role switch
            {
                M7LookMaterialRole.Signal => signalPulse,
                M7LookMaterialRole.Lamp => lampPulse,
                _ => crystalPulse
            };
            light.LightColor = color;
            float illuminationFactor = _profile.WorldCycle.Enabled ? _worldFunctionalLightFactor : 1f;
            light.LightEnergy = _profile.Emission.LocalLightEnergy * energy / 6f * pulse * ResolveLocalLightBoost() * illuminationFactor;
            light.Visible = light.LightEnergy > 0.001f;
            light.OmniRange = _profile.Emission.LocalLightRange;
        }
        AnimateWeapon();
        AnimateFire();
        _tracerPool?.Update(vfxDelta);
        _muzzlePool?.Update(vfxDelta);
        _impactPool?.Update(vfxDelta);
        _heroDebrisPool?.Update(vfxDelta);
        _destructionDustPool?.Update(vfxDelta);
        _explosionPool?.Update(vfxDelta);
    }

    private void UpdateSharedEmissionMaterial(M7LookMaterialRole role, Color color, float energy)
    {
        if (!_sharedMaterials.TryGetValue(role, out Material? shared) || shared is not ShaderMaterial material)
            return;
        material.SetShaderParameter("emission_color", color);
        material.SetShaderParameter("emission_energy", energy);
        material.SetShaderParameter("edge_darkening", _profile.Emission.EdgeDarkening);
    }

    private float ResolveEmissionPulse(M7LookMaterialRole role)
    {
        (float amount, float speed) = role switch
        {
            M7LookMaterialRole.Signal => (_profile.Emission.SignalPulseAmount, _profile.Emission.SignalPulseSpeed),
            M7LookMaterialRole.Crystal => (_profile.Emission.CrystalPulseAmount, _profile.Emission.CrystalPulseSpeed),
            _ => (_profile.Emission.LampPulseAmount, _profile.Emission.LampPulseSpeed)
        };
        return 1f + Mathf.Sin((float)_time * speed) * amount;
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
            delta, _destructionTuning);
        target.Scale = Vector3.One;
        // The source swap is atomic with the hot flash. Leaving the complete
        // model visible for ~58 ms made the destruction read as a pop.
        target.Visible = false;
        _ = frame;
        UpdateBurningEffectTransform();
        ApplyBurningEffectVisibility();
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
        target.Visible = false;
        PresentationDestructionTuning tuning = _destructionTuning;
        PresentationDestructionScaleBand band = targetIndex == 0
            ? PresentationDestructionScaleBand.Heavy
            : PresentationDestructionScaleBand.Structure;
        int fragments = tuning.ResolveHeroFragmentCount(band);
        Vector3 sourceSize = targetIndex == 0
            ? new Vector3(4.8f, 2.4f, 6.6f)
            : new Vector3(4.1f, 2.2f, 3.8f);
        Vector3 origin = target.GlobalPosition + Vector3.Up * (sourceSize.Y * 0.42f);
        float groundY = GroundHeight(target.GlobalPosition.X, target.GlobalPosition.Z) + 0.04f;
        if (_explosionPool is not null &&
            _explosionPool.TryAcquire(PooledExplosionBurst.RecommendedLifetimeSeconds, out PooledExplosionBurst blast))
        {
            blast.Configure(new ExplosionBurstRequest(origin, sourceSize, groundY,
                M7LookMaterialFactory.ParseColor(_profile.Destruction.BlastColor),
                _profile.Destruction.BlastVisualScale, _profile.Destruction.BlastEmissionEnergy,
                _profile.Destruction.BlastLightEnergy * ResolveLocalLightBoost(),
                _profile.Destruction.BlastLightRangeMultiplier,
                _profile.Destruction.BlastFlashPersistence, _profile.Destruction.BlastSmokeDelay,
                _profile.Destruction.BlastSmokeProminence, _profile.Destruction.BlastRingStrength));
            _explosionPool.Activate(blast);
        }
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
            _units[3].Visible = true;
            _units[3].Scale = Vector3.One;
            _units[3].RotationDegrees = new Vector3(0f, 315f, 0f);
        }
        if (_burningBuilding is not null)
        {
            _burningBuilding.Visible = true;
            _burningBuilding.Scale = Vector3.One;
            _burningBuilding.RotationDegrees = Vector3.Zero;
        }
        if (_activeDestructionTarget == 0) _destructionDriver.Remove(1004u);
        if (_activeDestructionTarget == 1) _destructionDriver.Remove(2001u);
        _activeDestructionTarget = -1;
        _destructionPreviewElapsed = float.MaxValue;
        UpdateBurningEffectTransform();
        ApplyBurningEffectVisibility();
    }

    private void UpdateBurningEffectTransform()
    {
        if (_burningBuilding is null) return;
        float collapsedHeight = 2.2f * _burningBuilding.Scale.Y;
        Vector3 fireOrigin = _burningBuilding.GlobalPosition + Vector3.Up * Math.Max(0.24f, collapsedHeight * 0.86f);
        if (_fireParticles is not null) _fireParticles.GlobalPosition = fireOrigin;
        if (_smokeParticles is not null) _smokeParticles.GlobalPosition = fireOrigin + Vector3.Up * 1.55f;
        if (_fireLight is not null) _fireLight.GlobalPosition = fireOrigin + Vector3.Up * 0.4f;
    }

    private void ApplyBurningEffectVisibility()
    {
        bool sourceAvailable = _activeDestructionTarget != 1;
        if (_fireParticles is not null)
            _fireParticles.Visible = sourceAvailable && _profile.Scene.BurningEnabled;
        if (_smokeParticles is not null)
            _smokeParticles.Visible = sourceAvailable && _profile.Scene.BurningEnabled && _profile.Scene.DustEnabled;
        if (_fireLight is not null)
            _fireLight.Visible = sourceAvailable && _profile.Scene.BurningEnabled;
    }

    private void AnimateUnits()
    {
        if (_animationRigs.Count != _units.Count) return;
        PresentationAnimationTuning tuning = _animationTuning;
        float delta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        int currentEmitters = _profile.Scene.FiringEnabled
            ? Math.Min(_profile.VfxPool.PreviewEmitters, _units.Count)
            : 0;
        if (!_shotTriggeredThisFrame && currentEmitters > _previousPreviewEmitters)
        {
            for (int i = _previousPreviewEmitters; i < currentEmitters; i++)
                _animationDriver.SynchronizeFireSequence((uint)(i + 1), _previewShotSequence);
        }
        for (int i = 0; i < _animationRigs.Count; i++)
        {
            bool choreography = _profile.Animation.PreviewChoreography;
            bool moving = choreography && i == 1;
            bool operating = choreography && i == 2;
            bool transforming = choreography && i == 3;
            float transformProgress = transforming ? 0.5f - 0.5f * Mathf.Cos((float)_time * 1.25f) : 0f;
            float health = 1f;
            uint fireSequence = i < _profile.VfxPool.PreviewEmitters && _profile.Scene.FiringEnabled
                ? _previewShotSequence
                : 0u;
            float distanceCells = _camera is null ? 0f : _camera.GlobalPosition.DistanceTo(_units[i].GlobalPosition) /
                GodotConversions.WorldUnitsPerBuildCell;
            PresentationAnimationInput input = new((uint)(i + 1), moving ? _profile.Animation.PreviewLocomotionSpeed : 0f,
                moving, operating, false, transforming, transformProgress, health, 0f, fireSequence,
                i == 0 || operating || transforming, distanceCells);
            PresentationAnimationFrame frame = _animationDriver.Update(input, delta, tuning);
            _animationRigs[i].Apply(frame, tuning);
        }
        _previousPreviewEmitters = currentEmitters;
    }

    private void AnimateWeapon()
    {
        if (_tracerPool is null || _muzzlePool is null || _impactPool is null ||
            _tracerVfxMaterial is null || _muzzleVfxMaterial is null || _impactVfxMaterial is null) return;
        if (_units.Count > 0) _shooterMuzzle = UnitMuzzle(0);
        float delta = _profile.Scene.Paused ? 0f : (float)GetProcessDeltaTime() * _profile.Scene.AnimationSpeed;
        UpdatePendingWeaponImpacts(delta);

        if (_shotTriggeredThisFrame && _profile.Scene.FiringEnabled)
        {
            int emitters = Math.Min(_profile.VfxPool.PreviewEmitters, _units.Count);
            for (int i = 0; i < emitters; i++)
            {
                Vector3 start = i == 0 ? _shooterMuzzle : UnitMuzzle(i);
                float travelSeconds = Math.Max(0.02f, start.DistanceTo(_targetPoint) / _profile.Vfx.TracerSpeed);
                if (_tracerPool.TryAcquire(travelSeconds, out PooledTracerEffect tracer))
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
                _pendingWeaponImpacts.Add(new PendingWeaponImpact(start, _targetPoint, travelSeconds));
            }
        }

        if (_impactLight is not null)
        {
            _impactLightRemaining = Math.Max(0f, _impactLightRemaining - delta);
            float pulse = _profile.Vfx.ImpactLightSeconds <= 0f
                ? 0f
                : Mathf.Clamp(_impactLightRemaining / _profile.Vfx.ImpactLightSeconds, 0f, 1f);
            _impactLight.Visible = _profile.Scene.FiringEnabled && pulse > 0.001f;
            _impactLight.LightEnergy = _profile.Vfx.ImpactLightEnergy * pulse * pulse * ResolveLocalLightBoost();
        }
    }

    private void AdvancePreviewShotClock(float delta)
    {
        _shotTriggeredThisFrame = false;
        if (!_profile.Scene.FiringEnabled)
        {
            _previewShotAccumulator = 0f;
            _shotClockPrimed = false;
            return;
        }
        if (!_shotClockPrimed)
        {
            _shotClockPrimed = true;
            _previewShotAccumulator = 0f;
            _previewShotSequence = checked(_previewShotSequence + 1u);
            _shotTriggeredThisFrame = true;
            return;
        }
        float interval = Math.Max(0.25f, _profile.Vfx.PreviewShotInterval);
        int elapsedShots = ConsumePreviewShotIntervals(ref _previewShotAccumulator, delta, interval);
        if (elapsedShots == 0) return;
        _previewShotSequence = checked(_previewShotSequence + (uint)elapsedShots);
        _shotTriggeredThisFrame = true;
    }

    private static int ConsumePreviewShotIntervals(ref float accumulator, float delta, float interval)
    {
        accumulator += Math.Max(0f, delta);
        float safeInterval = Math.Max(0.001f, interval);
        if (accumulator + 0.000001f < safeInterval) return 0;
        int elapsedShots = Math.Max(1, Mathf.FloorToInt((accumulator + 0.000001f) / safeInterval));
        accumulator = Math.Max(0f, accumulator - elapsedShots * safeInterval);
        return elapsedShots;
    }

    private void UpdatePendingWeaponImpacts(float delta)
    {
        for (int i = _pendingWeaponImpacts.Count - 1; i >= 0; i--)
        {
            PendingWeaponImpact pending = _pendingWeaponImpacts[i];
            pending.RemainingSeconds -= Math.Max(0f, delta);
            if (pending.RemainingSeconds > 0f)
            {
                _pendingWeaponImpacts[i] = pending;
                continue;
            }
            SpawnWeaponImpact(pending.Source, pending.Target);
            _pendingWeaponImpacts.RemoveAt(i);
        }
    }

    private void SpawnWeaponImpact(Vector3 source, Vector3 target)
    {
        if (_impactPool is not null && _impactVfxMaterial is not null &&
            _impactPool.TryAcquire(0.34f, out PooledParticleBurst impact))
        {
            float sparkDiameter = Math.Max(0.08f, _profile.Vfx.SparkSize * 4.2f);
            impact.Configure(target, source, _impactVfxMaterial,
                new Vector2(sparkDiameter, sparkDiameter * 3.8f) * _profile.Vfx.ImpactSize,
                _profile.Vfx.SparkCount);
            _impactPool.Activate(impact);
        }
        if (_impactLight is null) return;
        Vector3 incoming = source - target;
        if (incoming.LengthSquared() < 0.0001f) incoming = Vector3.Up;
        _impactLight.GlobalPosition = target + incoming.Normalized() * 0.12f;
        _impactLightRemaining = _profile.Vfx.ImpactLightSeconds;
    }

    private Vector3 UnitMuzzle(int unitIndex)
    {
        int safeIndex = Math.Clamp(unitIndex, 0, _units.Count - 1);
        Node3D unit = _units[safeIndex];
        FaceModelForwardAt(unit, _targetPoint);
        return ResolveUnitMuzzle(unit, _unitMuzzleSockets[safeIndex]);
    }

    private static void FaceModelForwardAt(Node3D unit, Vector3 target)
    {
        unit.LookAt(new Vector3(target.X, unit.GlobalPosition.Y, target.Z), Vector3.Up);
        // The authored Blender rig faces -Y, which becomes Godot +Z after glTF
        // conversion. Godot LookAt points -Z, so rotate once instead of firing
        // from the rear of the vehicle.
        unit.RotateY(Mathf.Pi);
    }

    private static Vector3 ResolveUnitMuzzle(Node3D unit, Node3D? tip = null)
    {
        if (tip is not null)
            return tip.GlobalPosition + unit.GlobalBasis.Z * 0.34f;
        return unit.GlobalPosition + unit.GlobalBasis * new Vector3(0f, 1.12f, 5.15f);
    }

    private struct PendingWeaponImpact
    {
        public readonly Vector3 Source;
        public readonly Vector3 Target;
        public float RemainingSeconds;

        public PendingWeaponImpact(Vector3 source, Vector3 target, float remainingSeconds)
        {
            Source = source;
            Target = target;
            RemainingSeconds = remainingSeconds;
        }
    }

    private void AnimateFire()
    {
        float flicker = 1f + Mathf.Sin((float)_time * 7.7f) * _profile.Vfx.FireFlicker * 0.22f
            + Mathf.Sin((float)_time * 13.1f + 1.7f) * _profile.Vfx.FireFlicker * 0.12f;
        if (_fireLight is not null)
            _fireLight.LightEnergy = _profile.Vfx.FireLightEnergy * Mathf.Max(0.25f, flicker) * ResolveLocalLightBoost();
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

    private static StandardMaterial3D ParticleBillboardMaterial(Color color, float energy, bool additive,
        bool useAuthoredGlare = false)
    {
        Color visible = new(color.R, color.G, color.B, Mathf.Clamp(color.A, 0f, 1f));
        Texture2D texture = useAuthoredGlare
            ? GD.Load<Texture2D>(GlareTexturePath)
            : SoftDiscTexture();
        return new StandardMaterial3D
        {
            AlbedoColor = visible,
            AlbedoTexture = texture,
            EmissionEnabled = energy > 0f,
            Emission = new Color(color.R, color.G, color.B),
            EmissionTexture = energy > 0f ? texture : null,
            EmissionEnergyMultiplier = energy,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            BlendMode = additive ? BaseMaterial3D.BlendModeEnum.Add : BaseMaterial3D.BlendModeEnum.Mix,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            NoDepthTest = false,
            RenderPriority = energy > 0f ? 4 : -4
        };
    }

    private static Texture2D SoftDiscTexture()
    {
        if (_softDiscTexture is not null) return _softDiscTexture;
        Gradient gradient = new();
        gradient.SetColor(0, Colors.White);
        gradient.SetColor(1, new Color(1f, 1f, 1f, 0f));
        _softDiscTexture = new GradientTexture2D
        {
            Gradient = gradient,
            Width = 64,
            Height = 64,
            Fill = GradientTexture2D.FillEnum.Radial,
            FillFrom = new Vector2(0.5f, 0.5f),
            FillTo = new Vector2(1f, 0.5f),
            UseHdr = true
        };
        return _softDiscTexture;
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
            case 5: _profile.WorldCycle = defaults.WorldCycle; break;
            case 6: _profile.Post = defaults.Post; break;
            case 7: _profile.Outline = defaults.Outline; break;
            case 8: _profile.Animation = defaults.Animation; break;
            case 9: _profile.Destruction = defaults.Destruction; break;
            case 10: _profile.Vfx = defaults.Vfx; _profile.VfxPool = defaults.VfxPool; break;
            case 11: _profile.Ground = defaults.Ground; break;
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
            setter((float)changed);
            ApplyProfile(rebuildMaterials);
            float normalized = getter();
            slider.SetValueNoSignal(normalized);
            value.Text = FormatValue(normalized, step);
            SetStatus($"{label}: {value.Text}");
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

    private static string MaterialMapDescription(int family) => family switch
    {
        0 => "Authored response: satin powder coating with nearly invisible color breakup and shallow roughness variation.",
        1 => "Authored response: matte coated structure. Brown describes the faction palette, not a stone material.",
        2 => "Authored response: clean accent coating with a tighter highlight than the main hull.",
        3 => "Authored response: blackened mechanism steel with restrained directional roughness.",
        4 => "Authored response: bare tool steel with a controlled brushed highlight.",
        5 => "Authored response: genuinely matte rubber with only a very shallow pore normal.",
        _ => "Authored response: coated building panels with shallow seams and no noisy full-surface tint."
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
        if (_destructionStatsLabel is null || _heroDebrisPool is null || _destructionDustPool is null ||
            _explosionPool is null) return;
        PresentationVfxPoolStats hero = _heroDebrisPool.GetStats();
        PresentationVfxPoolStats dust = _destructionDustPool.GetStats();
        PresentationVfxPoolStats blast = _explosionPool.GetStats();
        int fragments = 0;
        _heroDebrisPool.ForEachNode(effect => fragments += effect.ActiveFragmentCount);
        string target = _activeDestructionTarget switch { 0 => "UNIT", 1 => "STRUCTURE", _ => "READY" };
        _destructionStatsLabel.Text = $"{target}  ·  PREWARMED {hero.Created + dust.Created + blast.Created}  ·  ACTIVE BLAST/MODULE/DUST {blast.Active}/{hero.Active}/{dust.Active}  ·  " +
            $"VISIBLE MODULES {fragments}  ·  PEAK {blast.PeakActive}/{hero.PeakActive}/{dust.PeakActive}  ·  " +
            $"REUSED {blast.Reused + hero.Reused + dust.Reused}  ·  DROPPED {blast.Dropped + hero.Dropped + dust.Dropped}";
    }

    private bool ValidateLab(out int triangles)
    {
        triangles = 0;
        foreach (MeshInstance3D mesh in _unitMeshes)
            if (mesh.Mesh is Mesh source) triangles += source.GetFaces().Length / 3;
        string json = _profile.ToJson();
        bool roundTrip = M7LookProfile.TryFromJson(json, out M7LookProfile parsed, out _) &&
            parsed.Camera.ZoomCells == _profile.Camera.ZoomCells &&
            parsed.Materials.InspectionPass == _profile.Materials.InspectionPass &&
            Mathf.IsEqualApprox(parsed.Post.FilmGrain, _profile.Post.FilmGrain) &&
            Mathf.IsEqualApprox(parsed.Destruction.BlastFlashPersistence,
                _profile.Destruction.BlastFlashPersistence) &&
            Mathf.IsEqualApprox(parsed.Destruction.BlastSmokeDelay, _profile.Destruction.BlastSmokeDelay) &&
            parsed.WorldCycle.Environment == _profile.WorldCycle.Environment &&
            Mathf.IsEqualApprox(parsed.WorldCycle.NightReadability, _profile.WorldCycle.NightReadability);
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
        const string schemaFourFixture = "{\"schemaVersion\":4,\"emission\":{\"pulseAmount\":0.31,\"pulseSpeed\":4.2}}";
        bool schemaFourMigration = M7LookProfile.TryFromJson(schemaFourFixture, out M7LookProfile migratedFour, out _) &&
            migratedFour.SchemaVersion == M7LookProfile.CurrentSchemaVersion &&
            Mathf.IsEqualApprox(migratedFour.Emission.SignalPulseAmount, 0.31f) &&
            Mathf.IsEqualApprox(migratedFour.Emission.CrystalPulseAmount, 0.31f) &&
            Mathf.IsEqualApprox(migratedFour.Emission.SignalPulseSpeed, 4.2f) &&
            Mathf.IsZeroApprox(migratedFour.Emission.LampPulseAmount) && migratedFour.WorldCycle is not null &&
            !migratedFour.WorldCycle.Enabled && !migratedFour.WorldCycle.AnimatePreview;
        const string schemaFiveFixture = "{\"schemaVersion\":5,\"materials\":{\"paintedHull\":{\"reliefStrength\":0.9},\"groundRock\":{\"baseColor\":\"#9f732c\"}},\"ground\":{\"secondaryColor\":\"#3e3122\"},\"post\":{\"filmGrain\":0.2},\"destruction\":{\"blastVisualScale\":0.4}}";
        bool schemaFiveMigration = M7LookProfile.TryFromJson(schemaFiveFixture,
            out M7LookProfile migratedFive, out _) &&
            migratedFive.SchemaVersion == M7LookProfile.CurrentSchemaVersion &&
            Mathf.IsEqualApprox(migratedFive.Materials.PaintedHull.ReliefStrength, 0.05f) &&
            Mathf.IsZeroApprox(migratedFive.Post.FilmGrain) &&
            migratedFive.Materials.GroundRock.BaseColor == "#75684f" &&
            migratedFive.Ground.SecondaryColor == "#444844" &&
            Mathf.IsEqualApprox(migratedFive.Destruction.BlastVisualScale, 1.62f) &&
            !migratedFive.WorldCycle.Enabled && !migratedFive.WorldCycle.AnimatePreview;
        const string explicitSchemaFiveWorldFixture = "{\"schemaVersion\":5,\"worldCycle\":{\"enabled\":true,\"animatePreview\":false,\"environment\":3}}";
        bool explicitSchemaFiveWorld = M7LookProfile.TryFromJson(explicitSchemaFiveWorldFixture,
            out M7LookProfile migratedFiveWorld, out _) && migratedFiveWorld.WorldCycle.Enabled &&
            !migratedFiveWorld.WorldCycle.AnimatePreview &&
            migratedFiveWorld.WorldCycle.Environment == (int)M7WorldEnvironment.PlanetU;
        const string partialProfileFixture = "{\"schemaVersion\":4,\"materials\":{\"paintedHull\":{\"baseColor\":\"invalid\",\"metallic\":0.71}},\"lighting\":{\"keyColor\":\"not-a-color\"}}";
        bool profileSanitization = M7LookProfile.TryFromJson(partialProfileFixture,
            out M7LookProfile sanitized, out _) && sanitized.Materials.PaintedHull.BaseColor == "#07867e" &&
            Mathf.IsEqualApprox(sanitized.Materials.PaintedHull.Metallic, 0.71f) &&
            Mathf.IsEqualApprox(sanitized.Materials.PaintedHull.Roughness, 0.43f) &&
            sanitized.Lighting.KeyColor == "#fff4e5";
        bool roles = Enum.GetValues<M7LookMaterialRole>().All(role => _roleMeshes.Any(entry => entry.Role == role));
        bool emissiveBindings = _roleMeshes.Where(entry => entry.Role is M7LookMaterialRole.Signal or M7LookMaterialRole.Lamp or M7LookMaterialRole.Crystal)
            .All(entry => entry.Mesh.MaterialOverride is ShaderMaterial);
        Vector3 intactCenter = new(13f, 1.55f, -7f);
        bool exteriorImpact = Mathf.Abs(_targetPoint.X - intactCenter.X) >= 3.15f || Mathf.Abs(_targetPoint.Z - intactCenter.Z) >= 2.45f;
        bool animationBindings = _animationRigs.Count == 4 && _animationRigs.All(rig =>
            rig.WheelCount == 6 && rig.HasDrill && rig.HasSuspension) && ValidateAnimationDriver() &&
            ValidatePreviewShotClock();
        bool previewFiringActivity = _materialAudit || !_profile.Scene.FiringEnabled ||
            (_tracerPool?.GetStats().Spawned > 0 && _muzzlePool?.GetStats().Spawned > 0);
        bool vfxPools = _tracerPool is { Capacity: 64 } && _muzzlePool is { Capacity: 32 } &&
            _impactPool is { Capacity: 48 } && previewFiringActivity && ValidatePoolReuse();
        bool destructionActivity = _materialAudit || _smoke ||
            (_heroDebrisPool?.GetStats().Spawned > 0 && _destructionDustPool?.GetStats().Spawned > 0 &&
             _explosionPool?.GetStats().Spawned > 0);
        bool destruction = _heroDebrisPool is { Capacity: 12 } && _destructionDustPool is { Capacity: 12 } &&
            _explosionPool is { Capacity: 12 } && destructionActivity && ValidateDestructionDriver() &&
            PooledExplosionBurst.ValidateSmoke(this) &&
            M7WorldLightingEvaluator.ValidateDeterministic(out _);
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
            schemaTwoMigration && schemaThreeMigration && schemaFourMigration && profileSanitization &&
            schemaFiveMigration && explicitSchemaFiveWorld &&
            ResourceLoader.Exists("res://Assets/M7/Textures/painted_shell_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/brushed_metal_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/rubber_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/quarry_ground_detail.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/regolith_surface_v2.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/regolith_height.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/machine_panel_height.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/building_panel_height.png") &&
            ResourceLoader.Exists("res://Assets/M7/Textures/emissive_glare.png") &&
            _profile.Camera.ZoomCells is >= 24f and <= 72f;
        if (!valid)
        {
            GD.PrintErr($"M7 LOOK LAB AUDIT: roundTrip={roundTrip} migrations={schemaOneMigration}/{schemaTwoMigration}/{schemaThreeMigration}/{schemaFourMigration}/{schemaFiveMigration} " +
                $"schema5World={explicitSchemaFiveWorld} sanitization={profileSanitization} postComposition={postComposition} materialCache={materialCacheStable} " +
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
        PresentationAnimationDriver hitchDriver = new();
        _ = hitchDriver.Update(new PresentationAnimationInput(7, 0f, false, false,
            false, false, 0f, 1f, 0f, 0, false, 20f), 0f, tuning);
        PresentationAnimationFrame hitchEvent = hitchDriver.Update(new PresentationAnimationInput(7, 0f, false, false,
            false, false, 0f, 1f, 0f, 1, false, 20f), 0.25f, tuning);
        float recoil30 = SampleRecoilAfter(1f / 30f, 0.10f, tuning);
        float recoil60 = SampleRecoilAfter(1f / 60f, 0.10f, tuning);
        float recoil144 = SampleRecoilAfter(1f / 144f, 0.10f, tuning);
        PresentationAnimationDriver synchronizedDriver = new();
        synchronizedDriver.SynchronizeFireSequence(15u, 7u);
        PresentationAnimationFrame synchronized = synchronizedDriver.Update(new PresentationAnimationInput(15u,
            0f, false, false, false, false, 0f, 1f, 0f, 7u, false, 20f), 0f, tuning);
        PresentationAnimationFrame nextRealShot = synchronizedDriver.Update(new PresentationAnimationInput(15u,
            0f, false, false, false, false, 0f, 1f, 0f, 8u, false, 20f), 0f, tuning);
        return idle.Tier == PresentationAnimationTier.Normal && idle.WheelPhaseRadians == 0f &&
            active.Tier == PresentationAnimationTier.Important && active.WheelPhaseRadians > 0f &&
            active.LocomotionBlend > 0f && active.OperationBlend > 0f && active.TransformationProgress == 0.65f &&
            active.DamageAmount > 0.5f && active.Recoil > 0f && distant.Tier == PresentationAnimationTier.Distant &&
            !distant.ParametersUpdated && Mathf.IsEqualApprox(hitchEvent.Recoil, 1f) &&
            Mathf.Abs(recoil30 - recoil60) < 0.0001f && Mathf.Abs(recoil60 - recoil144) < 0.0001f &&
            Mathf.IsZeroApprox(synchronized.Recoil) && Mathf.IsEqualApprox(nextRealShot.Recoil, 1f);

        static float SampleRecoilAfter(float step, float elapsed, PresentationAnimationTuning tuning)
        {
            PresentationAnimationDriver sample = new();
            PresentationAnimationInput idleInput = new(9, 0f, false, false, false, false,
                0f, 1f, 0f, 0, false, 20f);
            PresentationAnimationInput firedInput = new(9, 0f, false, false, false, false,
                0f, 1f, 0f, 1, false, 20f);
            _ = sample.Update(idleInput, 0f, tuning);
            PresentationAnimationFrame frame = sample.Update(firedInput, 0f, tuning);
            float remaining = elapsed;
            while (remaining > 0.000001f)
            {
                float delta = Math.Min(step, remaining);
                frame = sample.Update(firedInput, delta, tuning);
                remaining -= delta;
            }
            return frame.Recoil;
        }
    }

    private static bool ValidatePreviewShotClock()
    {
        const float interval = 1.15f;
        return Simulate(1f / 30f) == 8 && Simulate(1f / 60f) == 8 && Simulate(1f / 144f) == 8;

        static int Simulate(float step)
        {
            float accumulator = 0f;
            float remaining = 10f;
            int shots = 0;
            while (remaining > 0.000001f)
            {
                float delta = Math.Min(step, remaining);
                shots += ConsumePreviewShotIntervals(ref accumulator, delta, interval);
                remaining -= delta;
            }
            return shots;
        }
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

    private static string FormatValue(float value, double step)
    {
        int decimals = step >= 1d ? 0 : Math.Clamp((int)Math.Ceiling(-Math.Log10(step)), 1, 6);
        string format = decimals == 0 ? "0" : $"0.{new string('#', decimals)}";
        return value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatCaptureFloat(float value) =>
        value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private string CaptureWorldMarker()
    {
        if (!_profile.WorldCycle.Enabled) return "manual";
        return (M7WorldEnvironment)_profile.WorldCycle.Environment switch
        {
            M7WorldEnvironment.Earth => "earth",
            M7WorldEnvironment.Mars => "mars",
            M7WorldEnvironment.Moon => "moon",
            M7WorldEnvironment.PlanetU => "planet-u",
            _ => "underground"
        };
    }

    private string CaptureTimeMarker() => _profile.WorldCycle.Enabled
        ? FormatCaptureFloat(_profile.WorldCycle.LocalTimeHours)
        : "manual";

    private string CapturePhaseMarker()
    {
        if (!_profile.WorldCycle.Enabled) return "manual";
        return M7WorldLightingEvaluator.Evaluate(_profile.WorldCycle).Phase switch
        {
            M7WorldLightPhase.DeepNight => "deep-night",
            M7WorldLightPhase.BlueHour => "blue-hour",
            M7WorldLightPhase.SunriseSunset => "sunrise-sunset",
            M7WorldLightPhase.GoldenHour => "golden-hour",
            M7WorldLightPhase.NeutralDay => "neutral-day",
            _ => "underground"
        };
    }

    private string CaptureMaterialViewMarker() => _profile.Materials.InspectionPass switch
    {
        1 => "base",
        2 => "color",
        3 => "relief",
        4 => "reflection",
        _ => "combined"
    };
    private static Color WithAlpha(Color color, float alpha) => new(color.R, color.G, color.B, alpha);

    private static StyleBoxFlat FlatPanel(Color color, float alpha, Color border) => new()
    {
        BgColor = WithAlpha(color, alpha), BorderColor = border,
        BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
        CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
        ContentMarginLeft = 10f, ContentMarginTop = 8f, ContentMarginRight = 10f, ContentMarginBottom = 8f
    };

    private static ArrayMesh BuildGroundMesh(int cells, float extent)
    {
        SurfaceTool surface = new();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        int row = cells + 1;
        for (int z = 0; z <= cells; z++)
        for (int x = 0; x <= cells; x++)
        {
            float px = Mathf.Lerp(-extent, extent, x / (float)cells);
            float pz = Mathf.Lerp(-extent, extent, z / (float)cells);
            surface.SetUV(new Vector2(x / (float)cells, z / (float)cells));
            surface.AddVertex(new Vector3(px, GroundHeight(px, pz), pz));
        }
        for (int z = 0; z < cells; z++)
        for (int x = 0; x < cells; x++)
        {
            int a = z * row + x;
            int b = a + 1;
            int d = a + row;
            int c = d + 1;
            surface.AddIndex(a); surface.AddIndex(c); surface.AddIndex(b);
            surface.AddIndex(a); surface.AddIndex(d); surface.AddIndex(c);
        }
        surface.GenerateNormals();
        return surface.Commit();
    }

    private static float GroundHeight(float x, float z)
    {
        float envelope = Mathf.Clamp((new Vector2(x, z).Length() - 22f) / 48f, 0f, 1f);
        envelope = envelope * envelope * (3f - 2f * envelope);
        // Broad, low-amplitude relief keeps the clean review ground alive
        // without exposing the 2.5 m terrain tessellation as black stair-step
        // bands under a low sun.
        float broadRelief = Mathf.Sin(x * 0.045f) * 0.10f +
            Mathf.Cos(z * 0.035f) * 0.07f +
            Mathf.Sin((x + z) * 0.022f) * 0.04f;
        return -0.05f + envelope * broadRelief;
    }

    private const string TrackShader = """
shader_type spatial;
render_mode blend_mix, depth_draw_never, cull_disabled, diffuse_burley, specular_schlick_ggx;
uniform vec4 track_color : source_color = vec4(0.35, 0.28, 0.20, 1.0);
uniform float track_opacity = 0.35;
uniform float tread_scale = 10.0;
void fragment() {
    float side_fade = smoothstep(0.0, 0.16, min(UV.x, 1.0 - UV.x));
    float end_fade = smoothstep(0.0, 0.12, min(UV.y, 1.0 - UV.y));
    float tread_phase = fract(UV.y * tread_scale + UV.x * 0.42);
    float tread = smoothstep(0.14, 0.28, tread_phase) * (1.0 - smoothstep(0.68, 0.86, tread_phase));
    float broken = smoothstep(0.18, 0.42, fract(UV.y * 3.7 + sin(UV.x * 11.0) * 0.08));
    float mask = side_fade * end_fade * tread * mix(0.55, 1.0, broken);
    float groove = mask * track_opacity;
    float slope_x = dFdx(groove) * 18.0;
    float slope_y = dFdy(groove) * 18.0;
    NORMAL_MAP = normalize(vec3(-slope_x, -slope_y, 1.0)) * 0.5 + 0.5;
    NORMAL_MAP_DEPTH = 1.35;
    float compressed_rim = clamp((abs(slope_x) + abs(slope_y)) * 0.12, 0.0, 1.0);
    ALBEDO = mix(track_color.rgb * 0.42, track_color.rgb * 0.86, compressed_rim);
    ROUGHNESS = 1.0;
    SPECULAR = 0.08;
    ALPHA = groove;
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
