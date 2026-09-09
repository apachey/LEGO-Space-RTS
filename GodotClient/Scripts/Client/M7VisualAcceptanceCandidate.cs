using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Client;

/// <summary>
/// Bounded four-faction proof for the accepted M7 visual direction. It
/// deliberately reuses the current Look/HUD presentation systems and contains
/// no gameplay authority or production asset-pipeline work.
/// </summary>
public partial class M7VisualAcceptanceCandidate : Node3D
{
    private const float CloseZoomCells = 24f;
    private const float CombatZoomCells = 44f;
    private const float StrategicZoomCells = 72f;
    private const float DefaultZoomCells = 84f;
    private const float MaximumZoomCells = 108f;
    private const float WorldUnitsPerCell = 1f;

    private enum ReviewLook : byte
    {
        CurrentCandidate,
        M7Final,
        Hybrid
    }

    private sealed class FactionPalette
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        public required string Source { get; init; }
        public required Color PrimaryColor { get; init; }
        public required Color MechanicalColor { get; init; }
        public required Color AccentColor { get; init; }
        public required Color LightColor { get; init; }
        public required Dictionary<M7LookMaterialRole, Material> Materials { get; init; }
    }

    private sealed class FactionPrototype
    {
        public required Node3D Root { get; init; }
        public required FactionPalette Palette { get; init; }
        public required PresentationAnimationRigBinding Rig { get; init; }
        public required Label3D Label { get; init; }
        public required MeshInstance3D SelectionRing { get; init; }
        public required Vector3 ReviewFocus { get; init; }
        public List<Node3D> TransformPivots { get; } = new();
        public List<Vector3> TransformBaseRotations { get; } = new();
        public List<Node3D> FunctionPivots { get; } = new();
        public int MeshCount { get; set; }
    }

    private M7LookProfile _look = M7LookProfile.CreateDefault();
    private readonly M7HudProfile _hudProfile = M7HudProfile.CreateDefault();
    private readonly PresentationAnimationDriver _animationDriver = new();
    private readonly List<FactionPrototype> _prototypes = new();
    private readonly List<PendingImpact> _pendingImpacts = new();
    private readonly List<(MeshInstance3D Mesh, M7LookMaterialRole Role)> _neutralMaterialBindings = new();
    private readonly List<(MeshInstance3D Mesh, FactionPalette Palette, M7LookMaterialRole Role, Node3D TextureRoot)> _factionMaterialBindings = new();
    private Dictionary<M7LookMaterialRole, Material> _baseMaterials = new();
    private PresentationAnimationTuning _animationTuning = new();
    private PresentationDestructionTuning _destructionTuning = new();
    private Camera3D? _camera;
    private DirectionalLight3D? _keyLight;
    private DirectionalLight3D? _fillLight;
    private DirectionalLight3D? _rimLight;
    private Godot.Environment? _environment;
    private ProceduralSkyMaterial? _skyMaterial;
    private Sky? _sky;
    private ShaderMaterial? _screenLookMaterial;
    private MeshInstance3D? _screenLookQuad;
    private HudView? _hud;
    private CanvasLayer? _hudLayer;
    private CanvasLayer? _reviewLayer;
    private PanelContainer? _reviewPanel;
    private Label? _statusLabel;
    private MeshInstance3D? _ground;
    private PresentationVfxPool<PooledTracerEffect>? _tracerPool;
    private PresentationVfxPool<PooledParticleBurst>? _muzzlePool;
    private PresentationVfxPool<PooledParticleBurst>? _impactPool;
    private PresentationVfxPool<PooledLegoDebrisBurst>? _heroDebrisPool;
    private PresentationVfxPool<PooledParticleBurst>? _destructionDustPool;
    private PresentationVfxPool<PooledExplosionBurst>? _explosionPool;
    private Material? _tracerMaterial;
    private Material? _muzzleMaterial;
    private Material? _impactMaterial;
    private Material? _dustMaterial;
    private Action? _returnToPrototype;
    private Vector3 _cameraFocus = Vector3.Zero;
    private float _zoomCells = DefaultZoomCells;
    private int _focusedFaction;
    private uint _shotSequence;
    private double _time;
    private float _shotAccumulator;
    private float _autoDestructionAccumulator;
    private int _destructionCursor;
    private int _destroyedFaction = -1;
    private float _destructionElapsed;
    private bool _paused;
    private bool _labelsVisible = true;
    private bool _hudVisible = true;
    private bool _reviewVisible = true;
    private ReviewLook _reviewLook = ReviewLook.M7Final;
    private bool _outlineEnabled = true;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private int _captureFrame = 90;
    private string? _capturePath;

    private readonly record struct PendingImpact(float Delay, Vector3 Position);

    public void Configure(Action returnToPrototype, string[] arguments)
    {
        Name = "M7VisualAcceptanceCandidate";
        _returnToPrototype = returnToPrototype;
        _smoke = arguments.Contains("--m7-acceptance-smoke");
        _capturePath = ParseString(arguments, "--capture-path");
        if (int.TryParse(ParseString(arguments, "--m7-acceptance-capture-frame"), out int captureFrame))
            _captureFrame = Math.Clamp(captureFrame, 24, 600);
        if (float.TryParse(ParseString(arguments, "--m7-acceptance-zoom"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float zoom))
            _zoomCells = ClampReviewZoom(zoom);
        _labelsVisible = ParseString(arguments, "--m7-acceptance-labels") != "hidden";
        _hudVisible = ParseString(arguments, "--m7-acceptance-hud") != "hidden";
        _reviewVisible = ParseString(arguments, "--m7-acceptance-review") != "hidden";
        _reviewLook = ParseReviewLook(ParseString(arguments, "--m7-acceptance-look"));
        _outlineEnabled = ParseString(arguments, "--m7-acceptance-outline") != "off";

        ConfigureCandidateProfile();
        BuildWorld();
        BuildFactionProof();
        BuildVfx();
        BuildHud();
        BuildReviewOverlay();
        ApplyVisualPresentation(rebuildMaterials: false);
        SetFactionFocus(0, centerCamera: false);
        ApplyCamera();
        ProcessPriority = 1000;

        if (arguments.Contains("--m7-acceptance-destruction")) TriggerDestruction(0);
        GD.Print($"M7 ACCEPTANCE CANDIDATE: active acceptance=m7-final-outline-on factions=4 zoom={FormatZoom(_zoomCells)} look={ReviewLookSlug(_reviewLook)} outline={OutlineSlug()} looks=3 defaultZoom={FormatZoom(DefaultZoomCells)} scrollZoom=24-108 hud=HybridConsole 5/6/7=looks O=outline Z/X/C=24/44/72 1..4=factions V=VFX D=destruction H=HUD L=labels Tab=review Escape=return");
    }

    public override void _Process(double delta)
    {
        float frameDelta = _paused ? 0f : Math.Min((float)delta, 0.25f);
        _time += frameDelta;
        AnimateFactionProof(frameDelta);
        UpdateVfx(frameDelta);
        UpdateDestruction(frameDelta);

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < _captureFrame) return;
        bool valid = ValidateCandidate(out int meshCount);
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
        {
            GD.Print($"M7 ACCEPTANCE CANDIDATE: PASS factions=4 prototypes=4 meshes={meshCount} sources=4970,7647,7646,7313 zoom={FormatZoom(_zoomCells)} cameraBands=24,44,72 defaultZoom={FormatZoom(DefaultZoomCells)} scrollZoom=24-108 looks=3 activeLook={ReviewLookSlug(_reviewLook)} outline={OutlineSlug()} materials=role-authored lighting=m7-shared animationDrivers=4 vfxPools=6 destruction=ready hud=HybridConsole minimap=legal acceptance=m7-final-outline-on");
        }
        else
        {
            GD.PrintErr($"M7 ACCEPTANCE CANDIDATE: FAIL factions={_prototypes.Count} meshes={meshCount} acceptance=m7-final-outline-on");
        }
        AutomatedSmokeExit.Finish(this, valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mouse)
        {
            if (mouse.ButtonIndex == MouseButton.WheelUp) AdjustZoom(0.92f);
            else if (mouse.ButtonIndex == MouseButton.WheelDown) AdjustZoom(1.08f);
            else return;
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        switch (key.Keycode)
        {
            case Key.Key1: SetFactionFocus(0, centerCamera: true); break;
            case Key.Key2: SetFactionFocus(1, centerCamera: true); break;
            case Key.Key3: SetFactionFocus(2, centerCamera: true); break;
            case Key.Key4: SetFactionFocus(3, centerCamera: true); break;
            case Key.Key5: SetVisualLook(ReviewLook.CurrentCandidate); break;
            case Key.Key6: SetVisualLook(ReviewLook.M7Final); break;
            case Key.Key7: SetVisualLook(ReviewLook.Hybrid); break;
            case Key.Key0: ShowOverview(); break;
            case Key.Z: SetZoom(CloseZoomCells); break;
            case Key.X: SetZoom(CombatZoomCells); break;
            case Key.C: SetZoom(StrategicZoomCells); break;
            case Key.V: SpawnShot(); SetStatus("Combat VFX replayed"); break;
            case Key.D: TriggerDestruction(_focusedFaction); break;
            case Key.H: ToggleHud(); break;
            case Key.L: ToggleLabels(); break;
            case Key.O: ToggleOutline(); break;
            case Key.Tab: ToggleReviewPanel(); break;
            case Key.Space: _paused = !_paused; SetStatus(_paused ? "Animation paused" : "Animation resumed"); break;
            case Key.Escape: _returnToPrototype?.Invoke(); break;
            default: return;
        }
        GetViewport().SetInputAsHandled();
    }

    private void ConfigureCandidateProfile()
    {
        _look = M7LookProfile.CreateDefault();
        // The last M7 production-direction fixture was schema-9 Earth Hybrid
        // Surface. Keep that exact ground treatment in all three comparisons;
        // the modes isolate the surrounding world/render treatment.
        _look.Ground.SurfaceTreatment = M7GroundSurfaceTreatment.HybridSurface;
        if (_reviewLook != ReviewLook.M7Final) _look.Ground.UnitSeparation = 9f;
        _look.WorldCycle.Enabled = true;
        _look.WorldCycle.AnimatePreview = false;
        _look.WorldCycle.Environment = (int)M7WorldEnvironment.Earth;
        _look.WorldCycle.LocalTimeHours = 14f;
        _look.Post.Enabled = _reviewLook != ReviewLook.CurrentCandidate;
        _look.Outline.Enabled = _outlineEnabled;
        _look.Post.FilmGrain = 0f;
        _look.Normalize();
        _animationTuning = _look.Animation.ToTuning();
        _destructionTuning = _look.Destruction.ToTuning();
        _baseMaterials = M7LookMaterialFactory.BuildSharedMaterials(_look);

        _hudProfile.ArtSkin.Finish = HudArtFinish.HybridConsole;
        _hudProfile.ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound;
        _hudProfile.Normalize();
    }

    private void BuildWorld()
    {
        _camera = new Camera3D
        {
            Name = "AcceptanceCamera",
            Current = true,
            Fov = 36f,
            Near = 0.2f,
            Far = 400f
        };
        AddChild(_camera);

        M7WorldLightingFrame light = M7WorldLightingEvaluator.Evaluate(_look.WorldCycle);
        _keyLight = new DirectionalLight3D
        {
            Name = "CandidateKeyLight",
            RotationDegrees = new Vector3(-light.KeyElevation, light.KeyAzimuth, 0f),
            LightColor = light.KeyColor,
            LightEnergy = light.KeyEnergy,
            ShadowEnabled = true,
            SkyMode = DirectionalLight3D.SkyModeEnum.LightAndSky,
            DirectionalShadowMode = DirectionalLight3D.ShadowMode.Parallel4Splits,
            DirectionalShadowBlendSplits = true,
            DirectionalShadowSplit1 = 0.12f,
            DirectionalShadowSplit2 = 0.28f,
            DirectionalShadowSplit3 = 0.55f,
            DirectionalShadowFadeStart = 0.82f,
            DirectionalShadowMaxDistance = 120f,
            ShadowBlur = light.ShadowBlur,
            ShadowOpacity = light.ShadowOpacity,
            ShadowBias = 0.035f,
            ShadowNormalBias = 0.75f
        };
        _fillLight = new DirectionalLight3D
        {
            Name = "CandidateFillLight",
            RotationDegrees = new Vector3(-light.FillElevation, light.FillAzimuth, 0f),
            LightColor = light.FillColor,
            LightEnergy = light.FillEnergy,
            ShadowEnabled = false,
            SkyMode = DirectionalLight3D.SkyModeEnum.LightOnly
        };
        _rimLight = new DirectionalLight3D
        {
            Name = "CandidateRimLight",
            RotationDegrees = new Vector3(-28f, 224f, 0f),
            LightColor = light.RimColor,
            LightEnergy = light.RimEnergy,
            ShadowEnabled = false,
            SkyMode = DirectionalLight3D.SkyModeEnum.LightOnly,
            Visible = light.RimEnabled
        };
        AddChild(_keyLight);
        AddChild(_fillLight);
        AddChild(_rimLight);

        _skyMaterial = new ProceduralSkyMaterial
        {
            SkyTopColor = new Color("10192c"),
            SkyHorizonColor = new Color("334966"),
            SkyCurve = 0.18f,
            GroundBottomColor = new Color("090d13"),
            GroundHorizonColor = new Color("202b38"),
            GroundCurve = 0.14f,
            SunAngleMax = 1.2f,
            SunCurve = 0.08f,
            UseDebanding = true
        };
        _sky = new Sky
        {
            SkyMaterial = _skyMaterial,
            ProcessMode = Sky.ProcessModeEnum.Realtime,
            RadianceSize = Sky.RadianceSizeEnum.Size256
        };
        _environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            Sky = _sky,
            BackgroundColor = light.BackgroundColor,
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = light.AmbientColor,
            AmbientLightEnergy = light.AmbientEnergy,
            ReflectedLightSource = Godot.Environment.ReflectionSource.Bg,
            TonemapMode = Godot.Environment.ToneMapper.Filmic,
            TonemapExposure = Mathf.Pow(2f, _look.Post.Exposure),
            GlowEnabled = _look.Post.BloomEnabled,
            GlowNormalized = false,
            GlowIntensity = _look.Post.BloomIntensity,
            GlowStrength = 1.35f,
            GlowBloom = _look.Post.BloomSpread,
            GlowHdrThreshold = _look.Post.BloomThreshold,
            GlowHdrScale = 2f
        };
        AddChild(new WorldEnvironment { Name = "CandidateEnvironment", Environment = _environment });
        BuildScreenLookPass();

        _ground = new MeshInstance3D
        {
            Name = "NeutralReviewTerrain",
            Mesh = CreateGroundMesh(),
            MaterialOverride = _baseMaterials[M7LookMaterialRole.GroundRock],
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            ExtraCullMargin = 8f
        };
        AddChild(_ground);

        BuildNeutralTerrainDetails();
    }

    private void BuildScreenLookPass()
    {
        _screenLookMaterial = new ShaderMaterial
        {
            Shader = new Shader { Code = M7LookLab.PostShader },
            RenderPriority = -128
        };
        _screenLookQuad = new MeshInstance3D
        {
            Name = "M7ScreenSpaceLookPass",
            Mesh = new QuadMesh { Size = new Vector2(2f, 2f), FlipFaces = true },
            MaterialOverride = _screenLookMaterial,
            ExtraCullMargin = 16384f,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_screenLookQuad);
    }

    private Mesh CreateGroundMesh()
    {
        if (_reviewLook == ReviewLook.CurrentCandidate)
        {
            return new PlaneMesh
            {
                Size = new Vector2(180f, 180f),
                SubdivideWidth = 96,
                SubdivideDepth = 96
            };
        }
        return M7LookLab.BuildGroundMesh(240, 300f);
    }

    private void BuildNeutralTerrainDetails()
    {
        Vector3[] rocks =
        {
            new(-18f, 0.45f, -11f), new(-16f, 0.30f, 13f), new(17f, 0.38f, 12f),
            new(18f, 0.26f, -12f), new(-3f, 0.22f, 17f), new(4f, 0.26f, -17f)
        };
        for (int i = 0; i < rocks.Length; i++)
        {
            Vector3 size = new(2.2f + (i % 3) * 0.7f, 0.65f + (i % 2) * 0.35f, 1.6f + ((i + 1) % 3) * 0.5f);
            AddNeutralMesh(this, $"NeutralRock_{i:00}", new BoxMesh { Size = size }, rocks[i],
                new Vector3(0f, i * 23f, i % 2 == 0 ? 8f : -7f), M7LookMaterialRole.StructuralEarth);
        }

        Node3D crystal = new() { Name = "NeutralCrystalResource", Position = Vector3.Zero };
        AddChild(crystal);
        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.Tau / 5f;
            AddNeutralMesh(crystal, $"Crystal_{i:00}", new PrismMesh { Size = new Vector3(0.52f, 1.7f + 0.25f * (i % 2), 0.52f) },
                new Vector3(Mathf.Cos(angle) * 0.72f, 0.85f, Mathf.Sin(angle) * 0.72f),
                new Vector3(i % 2 == 0 ? 7f : -7f, Mathf.RadToDeg(angle), 0f),
                M7LookMaterialRole.Crystal);
        }
        crystal.AddChild(new OmniLight3D
        {
            Name = "CrystalLight",
            Position = Vector3.Up,
            LightColor = new Color("b7ff55"),
            LightEnergy = 0.75f,
            OmniRange = 4.8f,
            ShadowEnabled = false
        });

        AddNeutralMesh(this, "ReviewPad", new CylinderMesh { TopRadius = 2.35f, BottomRadius = 2.35f, Height = 0.12f, RadialSegments = 48 },
            new Vector3(0f, 0.03f, 0f), Vector3.Zero, M7LookMaterialRole.DarkMechanic);
    }

    private void BuildFactionProof()
    {
        FactionPalette raiders = CreatePalette("rock-raiders", "ROCK RAIDERS", "4970 CHROME CRUSHER",
            new Color("07867e"), new Color("71432d"), new Color("e8a21b"), new Color("ff7a20"));
        FactionPalette astronauts = CreatePalette("astronauts", "ASTRONAUTS", "7647 MX-41 SWITCH FIGHTER",
            new Color("c7cfcd"), new Color("3156a8"), new Color("ef7622"), new Color("60bfff"));
        FactionPalette aliens = CreatePalette("aliens", "MARS MISSION ALIENS", "7646 ETX ALIEN INFILTRATOR",
            new Color("111315"), new Color("6f777b"), new Color("78b82a"), new Color("7cff2e"));
        FactionPalette martians = CreatePalette("martians", "LIFE ON MARS MARTIANS", "7313 RED PLANET PROTECTOR",
            new Color("3165b3"), new Color("7193a8"), new Color("c84536"), new Color("ff7a24"));

        // These positions form a screen-space grid for the canonical 45-degree
        // gameplay yaw: x-z controls left/right while x+z controls depth.
        BuildChromeCrusher(raiders, new Vector3(-8f, 0.08f, 2f));
        BuildMx41(astronauts, new Vector3(2f, 0.08f, -8f));
        BuildAlienInfiltrator(aliens, new Vector3(-2f, 0.08f, 8f));
        BuildRedPlanetProtector(martians, new Vector3(8f, 0.08f, -2f));
    }

    private FactionPalette CreatePalette(string key, string displayName, string source,
        Color primary, Color mechanical, Color accent, Color light)
    {
        return new FactionPalette
        {
            Key = key,
            DisplayName = displayName,
            Source = source,
            PrimaryColor = primary,
            MechanicalColor = mechanical,
            AccentColor = accent,
            LightColor = light,
            Materials = BuildFactionMaterials(key, primary, mechanical, accent, light)
        };
    }

    private Dictionary<M7LookMaterialRole, Material> BuildFactionMaterials(string key,
        Color primary, Color mechanical, Color accent, Color light)
    {
        Dictionary<M7LookMaterialRole, Material> materials = new();
        foreach ((M7LookMaterialRole role, Material sourceMaterial) in _baseMaterials)
            materials[role] = sourceMaterial;
        materials[M7LookMaterialRole.PaintedHull] = Recolored(_baseMaterials[M7LookMaterialRole.PaintedHull], primary);
        materials[M7LookMaterialRole.StructuralEarth] = Recolored(_baseMaterials[M7LookMaterialRole.StructuralEarth], mechanical);
        materials[M7LookMaterialRole.Accent] = Recolored(_baseMaterials[M7LookMaterialRole.Accent], accent);
        materials[M7LookMaterialRole.BuildingShell] = Recolored(_baseMaterials[M7LookMaterialRole.BuildingShell], primary.Darkened(0.18f));
        materials[M7LookMaterialRole.Signal] = M7LookMaterialFactory.Emissive(light, 5.5f, 1f, _look.Emission.EdgeDarkening);
        materials[M7LookMaterialRole.Lamp] = M7LookMaterialFactory.Emissive(light, 7f, 1f, _look.Emission.EdgeDarkening);
        materials[M7LookMaterialRole.CanopyGlass] = GlassFor(key);
        return materials;
    }

    private static Material Recolored(Material source, Color color)
    {
        Material duplicate = (Material)source.Duplicate(true);
        if (duplicate is ShaderMaterial shader) shader.SetShaderParameter("base_color", color);
        return duplicate;
    }

    private Material GlassFor(string key)
    {
        GlassLook glass = new()
        {
            Tint = key switch
            {
                "astronauts" => "#8db8cf",
                "aliens" => "#82c941",
                "martians" => "#cc7443",
                _ => "#31595b"
            },
            Opacity = _look.Glass.Opacity,
            Roughness = _look.Glass.Roughness,
            Ior = _look.Glass.Ior,
            RefractionStrength = _look.Glass.RefractionStrength,
            EdgeBrightness = _look.Glass.EdgeBrightness
        };
        return M7LookMaterialFactory.Glass(glass);
    }

    private void BuildChromeCrusher(FactionPalette palette, Vector3 position)
    {
        FactionPrototype prototype = CreatePrototype(palette, position, 5.1f);
        Node3D suspension = new() { Name = "Pivot_Suspension" };
        prototype.Root.AddChild(suspension);
        AddPart(prototype, suspension, "Body_MainHull", new BoxMesh { Size = new Vector3(4.8f, 1.25f, 5.4f) },
            new Vector3(0f, 1.45f, 0f), Vector3.Zero, M7LookMaterialRole.PaintedHull);
        AddPart(prototype, suspension, "Body_RearHousing", new BoxMesh { Size = new Vector3(3.9f, 1.6f, 2.2f) },
            new Vector3(0f, 2.15f, 1.35f), Vector3.Zero, M7LookMaterialRole.StructuralEarth);
        AddPart(prototype, suspension, "Glass_Cab", new PrismMesh { Size = new Vector3(2.4f, 1.3f, 1.8f) },
            new Vector3(0f, 2.35f, -0.75f), Vector3.Zero, M7LookMaterialRole.CanopyGlass);
        AddPart(prototype, suspension, "Accent_HazardBand", new BoxMesh { Size = new Vector3(4.9f, 0.25f, 0.28f) },
            new Vector3(0f, 1.62f, -2.72f), Vector3.Zero, M7LookMaterialRole.Accent);
        AddPart(prototype, suspension, "Tool_UpperBrace", new BoxMesh { Size = new Vector3(1.4f, 0.38f, 2.4f) },
            new Vector3(0f, 1.65f, -3.2f), new Vector3(-12f, 0f, 0f), M7LookMaterialRole.ToolSteel);
        Node3D drill = new() { Name = "Pivot_Drill", Position = new Vector3(0f, 1.42f, -4.15f), RotationDegrees = new Vector3(90f, 0f, 0f) };
        prototype.Root.AddChild(drill);
        AddPart(prototype, drill, "Tool_DrillCone", new CylinderMesh { TopRadius = 0f, BottomRadius = 1.05f, Height = 2.8f, RadialSegments = 24 },
            Vector3.Zero, Vector3.Zero, M7LookMaterialRole.ToolSteel);
        for (int i = 0; i < 3; i++)
            AddPart(prototype, drill, $"Tool_DrillFin_{i}", new BoxMesh { Size = new Vector3(0.18f, 2.1f, 0.38f) },
                Vector3.Zero, new Vector3(0f, i * 60f, 28f), M7LookMaterialRole.Accent);
        AddPart(prototype, suspension, "Lamp_Work", new SphereMesh { Radius = 0.22f, Height = 0.44f, RadialSegments = 16, Rings = 8 },
            new Vector3(1.65f, 2.5f, -1.8f), Vector3.Zero, M7LookMaterialRole.Lamp);
        AddWheels(prototype, 3, 2.52f, 1.82f, 0.72f);
        AddStudGrid(prototype, suspension, 3, 2, new Vector3(0f, 2.94f, 1.35f), 0.72f, M7LookMaterialRole.StructuralEarth);
        FinishPrototype(prototype);
    }

    private void BuildMx41(FactionPalette palette, Vector3 position)
    {
        FactionPrototype prototype = CreatePrototype(palette, position, 4.6f);
        Node3D suspension = new() { Name = "Pivot_Suspension" };
        prototype.Root.AddChild(suspension);
        AddPart(prototype, suspension, "Body_WhiteFuselage", new BoxMesh { Size = new Vector3(3.2f, 1.05f, 4.8f) },
            new Vector3(0f, 1.45f, 0f), Vector3.Zero, M7LookMaterialRole.PaintedHull);
        AddPart(prototype, suspension, "Structural_BlueSpine", new BoxMesh { Size = new Vector3(1.5f, 0.55f, 5.4f) },
            new Vector3(0f, 1.98f, 0.15f), Vector3.Zero, M7LookMaterialRole.StructuralEarth);
        AddPart(prototype, suspension, "Glass_BlueCanopy", new PrismMesh { Size = new Vector3(1.7f, 1.05f, 2.05f) },
            new Vector3(0f, 2.45f, -0.82f), Vector3.Zero, M7LookMaterialRole.CanopyGlass);
        AddPart(prototype, suspension, "Accent_Nose", new PrismMesh { Size = new Vector3(1.85f, 0.52f, 1.8f) },
            new Vector3(0f, 1.6f, -3.0f), new Vector3(90f, 0f, 0f), M7LookMaterialRole.Accent);
        for (int side = -1; side <= 1; side += 2)
        {
            Node3D wing = new() { Name = side < 0 ? "Pivot_LeftWing" : "Pivot_RightWing", Position = new Vector3(side * 1.55f, 1.7f, 0.4f) };
            prototype.Root.AddChild(wing);
            AddPart(prototype, wing, side < 0 ? "Body_LeftWing" : "Body_RightWing",
                new BoxMesh { Size = new Vector3(2.9f, 0.28f, 3.2f) }, new Vector3(side * 1.25f, 0f, 0f),
                new Vector3(0f, 0f, side * -5f), M7LookMaterialRole.PaintedHull);
            AddPart(prototype, wing, side < 0 ? "Accent_LeftWingTip" : "Accent_RightWingTip",
                new BoxMesh { Size = new Vector3(0.48f, 0.36f, 2.5f) }, new Vector3(side * 2.55f, 0f, 0f),
                Vector3.Zero, M7LookMaterialRole.Accent);
            prototype.TransformPivots.Add(wing);
            prototype.TransformBaseRotations.Add(wing.RotationDegrees);
        }
        AddWheels(prototype, 2, 1.72f, 1.62f, 0.55f);
        AddPart(prototype, suspension, "Signal_Blue", new SphereMesh { Radius = 0.18f, Height = 0.36f, RadialSegments = 16, Rings = 8 },
            new Vector3(0f, 2.35f, 1.9f), Vector3.Zero, M7LookMaterialRole.Signal);
        AddStudGrid(prototype, suspension, 2, 3, new Vector3(0f, 2.32f, 1.25f), 0.65f, M7LookMaterialRole.StructuralEarth);
        FinishPrototype(prototype);
    }

    private void BuildAlienInfiltrator(FactionPalette palette, Vector3 position)
    {
        FactionPrototype prototype = CreatePrototype(palette, position, 4.5f);
        Node3D suspension = new() { Name = "Pivot_Suspension", Position = new Vector3(0f, 0.18f, 0f) };
        prototype.Root.AddChild(suspension);
        AddPart(prototype, suspension, "Body_BlackCore", new PrismMesh { Size = new Vector3(4.5f, 1.15f, 4.8f) },
            new Vector3(0f, 1.55f, 0f), new Vector3(0f, 90f, 0f), M7LookMaterialRole.PaintedHull);
        AddPart(prototype, suspension, "Glass_LimeCockpit", new SphereMesh { Radius = 1.05f, Height = 1.25f, RadialSegments = 24, Rings = 12 },
            new Vector3(0f, 2.25f, -0.55f), Vector3.Zero, M7LookMaterialRole.CanopyGlass);
        AddPart(prototype, suspension, "DarkMechanic_CoreRing", new TorusMesh { InnerRadius = 1.25f, OuterRadius = 1.55f, Rings = 24, RingSegments = 12 },
            new Vector3(0f, 1.72f, 0.25f), Vector3.Zero, M7LookMaterialRole.DarkMechanic);
        for (int side = -1; side <= 1; side += 2)
        {
            Node3D claw = new() { Name = side < 0 ? "Pivot_LeftClaw" : "Pivot_RightClaw", Position = new Vector3(side * 1.7f, 1.25f, 0.2f) };
            prototype.Root.AddChild(claw);
            AddPart(prototype, claw, side < 0 ? "Accent_LeftBlade" : "Accent_RightBlade",
                new PrismMesh { Size = new Vector3(2.7f, 0.48f, 3.5f) }, new Vector3(side * 1.25f, 0f, 0f),
                new Vector3(0f, side * 14f, side * 9f), M7LookMaterialRole.Accent);
            AddPart(prototype, claw, side < 0 ? "Tool_LeftEmitter" : "Tool_RightEmitter",
                new CylinderMesh { TopRadius = 0.34f, BottomRadius = 0.44f, Height = 1.5f, RadialSegments = 16 },
                new Vector3(side * 2.15f, -0.05f, -1.25f), new Vector3(90f, 0f, 0f), M7LookMaterialRole.ToolSteel);
            prototype.TransformPivots.Add(claw);
            prototype.TransformBaseRotations.Add(claw.RotationDegrees);
        }
        for (int i = -1; i <= 1; i += 2)
        {
            AddPart(prototype, suspension, $"DarkMechanic_RearLeg_{i}", new BoxMesh { Size = new Vector3(0.55f, 1.1f, 2.6f) },
                new Vector3(i * 1.45f, 0.62f, 1.6f), new Vector3(18f, 0f, i * 18f), M7LookMaterialRole.DarkMechanic);
            AddPart(prototype, suspension, $"Accent_RearPad_{i}", new BoxMesh { Size = new Vector3(1.25f, 0.26f, 1.45f) },
                new Vector3(i * 2.0f, 0.18f, 2.55f), Vector3.Zero, M7LookMaterialRole.Accent);
        }
        AddPart(prototype, suspension, "Signal_Resonance", new SphereMesh { Radius = 0.26f, Height = 0.52f, RadialSegments = 18, Rings = 9 },
            new Vector3(0f, 2.35f, 1.65f), Vector3.Zero, M7LookMaterialRole.Signal);
        AddStudGrid(prototype, suspension, 2, 2, new Vector3(0f, 2.28f, 0.85f), 0.72f, M7LookMaterialRole.Accent);
        FinishPrototype(prototype);
    }

    private void BuildRedPlanetProtector(FactionPalette palette, Vector3 position)
    {
        FactionPrototype prototype = CreatePrototype(palette, position, 5.8f);
        Node3D suspension = new() { Name = "Pivot_Suspension" };
        prototype.Root.AddChild(suspension);
        AddPart(prototype, suspension, "Body_BlueTorso", new BoxMesh { Size = new Vector3(3.8f, 2.0f, 3.2f) },
            new Vector3(0f, 3.6f, 0f), Vector3.Zero, M7LookMaterialRole.PaintedHull);
        AddPart(prototype, suspension, "Structural_SandBlueChest", new PrismMesh { Size = new Vector3(2.5f, 1.35f, 1.4f) },
            new Vector3(0f, 4.25f, -1.55f), new Vector3(90f, 0f, 0f), M7LookMaterialRole.StructuralEarth);
        AddPart(prototype, suspension, "Glass_OrangeCab", new SphereMesh { Radius = 0.82f, Height = 1.2f, RadialSegments = 20, Rings = 10 },
            new Vector3(0f, 4.75f, -0.2f), Vector3.Zero, M7LookMaterialRole.CanopyGlass);
        for (int side = -1; side <= 1; side += 2)
        {
            AddPart(prototype, suspension, $"DarkMechanic_UpperLeg_{side}", new BoxMesh { Size = new Vector3(0.72f, 2.6f, 0.75f) },
                new Vector3(side * 1.2f, 1.95f, 0.35f), new Vector3(0f, 0f, side * 8f), M7LookMaterialRole.DarkMechanic);
            AddPart(prototype, suspension, $"Structural_LowerLeg_{side}", new BoxMesh { Size = new Vector3(0.9f, 1.7f, 0.95f) },
                new Vector3(side * 1.42f, 0.75f, -0.1f), new Vector3(0f, 0f, side * -8f), M7LookMaterialRole.StructuralEarth);
            AddPart(prototype, suspension, $"Accent_Foot_{side}", new BoxMesh { Size = new Vector3(1.55f, 0.35f, 2.0f) },
                new Vector3(side * 1.55f, 0.2f, -0.38f), Vector3.Zero, M7LookMaterialRole.Accent);
            Node3D arm = new() { Name = side < 0 ? "Pivot_LeftArm" : "Pivot_RightArm", Position = new Vector3(side * 2.15f, 4.1f, 0f) };
            prototype.Root.AddChild(arm);
            AddPart(prototype, arm, $"Body_Shoulder_{side}", new SphereMesh { Radius = 0.52f, Height = 1.04f, RadialSegments = 16, Rings = 8 },
                Vector3.Zero, Vector3.Zero, M7LookMaterialRole.PaintedHull);
            AddPart(prototype, arm, $"Tool_Arm_{side}", new BoxMesh { Size = new Vector3(0.62f, 2.5f, 0.72f) },
                new Vector3(side * 0.2f, -1.25f, -0.2f), new Vector3(side * 10f, 0f, side * -7f), M7LookMaterialRole.ToolSteel);
            AddPart(prototype, arm, $"Accent_ControlPad_{side}", new BoxMesh { Size = new Vector3(1.0f, 0.42f, 1.35f) },
                new Vector3(side * 0.35f, -2.35f, -0.42f), Vector3.Zero, M7LookMaterialRole.Accent);
            prototype.FunctionPivots.Add(arm);
        }
        AddPart(prototype, suspension, "Signal_Orange", new SphereMesh { Radius = 0.23f, Height = 0.46f, RadialSegments = 16, Rings = 8 },
            new Vector3(0f, 5.48f, 0.35f), Vector3.Zero, M7LookMaterialRole.Signal);
        AddStudGrid(prototype, suspension, 3, 2, new Vector3(0f, 4.68f, 1.15f), 0.62f, M7LookMaterialRole.PaintedHull);
        FinishPrototype(prototype);
    }

    private FactionPrototype CreatePrototype(FactionPalette palette, Vector3 position, float labelHeight)
    {
        Node3D root = new() { Name = $"Prototype_{palette.Key}", Position = position };
        AddChild(root);
        Label3D label = new()
        {
            Name = $"SourceLabel_{palette.Key}",
            Text = $"{palette.Source}\n{palette.DisplayName}",
            Position = new Vector3(0f, labelHeight, 0f),
            FontSize = 22,
            PixelSize = 0.0105f,
            Modulate = Colors.White,
            OutlineModulate = new Color(0f, 0f, 0f, 0.84f),
            OutlineSize = 6,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true,
            Visible = _labelsVisible
        };
        root.AddChild(label);
        MeshInstance3D ring = new()
        {
            Name = $"SelectionRing_{palette.Key}",
            Position = new Vector3(0f, 0.05f, 0f),
            Mesh = new TorusMesh { InnerRadius = 3.0f, OuterRadius = 3.18f, Rings = 32, RingSegments = 10 },
            MaterialOverride = M7LookMaterialFactory.Emissive(palette.LightColor, 1.6f, 0.86f, 0.25f),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Visible = false
        };
        root.AddChild(ring);
        return new FactionPrototype
        {
            Root = root,
            Palette = palette,
            Rig = null!,
            Label = label,
            SelectionRing = ring,
            ReviewFocus = position
        };
    }

    private void FinishPrototype(FactionPrototype prototype)
    {
        FactionPrototype finished = new()
        {
            Root = prototype.Root,
            Palette = prototype.Palette,
            Rig = new PresentationAnimationRigBinding(prototype.Root),
            Label = prototype.Label,
            SelectionRing = prototype.SelectionRing,
            ReviewFocus = prototype.ReviewFocus,
            MeshCount = prototype.MeshCount
        };
        finished.TransformPivots.AddRange(prototype.TransformPivots);
        finished.TransformBaseRotations.AddRange(prototype.TransformBaseRotations);
        finished.FunctionPivots.AddRange(prototype.FunctionPivots);
        _prototypes.Add(finished);
        _animationDriver.SynchronizeFireSequence((uint)_prototypes.Count, 0u);
        finished.Root.AddChild(new OmniLight3D
        {
            Name = $"FactionLight_{finished.Palette.Key}",
            Position = new Vector3(0f, 2.2f, 0f),
            LightColor = finished.Palette.LightColor,
            LightEnergy = 0.42f,
            OmniRange = 3.8f,
            ShadowEnabled = false
        });
    }

    private void AddWheels(FactionPrototype prototype, int axles,
        float halfWidth, float halfLength, float radius)
    {
        for (int axle = 0; axle < axles; axle++)
        {
            float z = axles == 1 ? 0f : Mathf.Lerp(-halfLength, halfLength, axle / (float)(axles - 1));
            for (int side = -1; side <= 1; side += 2)
            {
                Node3D pivot = new()
                {
                    Name = $"Pivot_Wheel_{prototype.Palette.Key}_{axle}_{side}",
                    Position = new Vector3(side * halfWidth, radius, z)
                };
                prototype.Root.AddChild(pivot);
                AddPart(prototype, pivot, $"Rubber_Wheel_{axle}_{side}",
                    new CylinderMesh { TopRadius = radius, BottomRadius = radius, Height = 0.46f, RadialSegments = 20 },
                    Vector3.Zero, new Vector3(0f, 0f, 90f), M7LookMaterialRole.Rubber);
                AddPart(prototype, pivot, $"Tool_Hub_{axle}_{side}",
                    new CylinderMesh { TopRadius = radius * 0.42f, BottomRadius = radius * 0.42f, Height = 0.49f, RadialSegments = 16 },
                    Vector3.Zero, new Vector3(0f, 0f, 90f), M7LookMaterialRole.ToolSteel);
            }
        }
    }

    private void AddStudGrid(FactionPrototype prototype, Node3D parent, int columns, int rows,
        Vector3 center, float spacing, M7LookMaterialRole role)
    {
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            float x = (column - (columns - 1) * 0.5f) * spacing;
            float z = (row - (rows - 1) * 0.5f) * spacing;
            AddPart(prototype, parent, $"Stud_{role}_{row}_{column}",
                new CylinderMesh { TopRadius = 0.24f, BottomRadius = 0.24f, Height = 0.18f, RadialSegments = 16 },
                center + new Vector3(x, 0f, z), Vector3.Zero, role);
        }
    }

    private void AddPart(FactionPrototype prototype, Node3D parent, string name, PrimitiveMesh mesh,
        Vector3 position, Vector3 rotationDegrees, M7LookMaterialRole role)
    {
        MeshInstance3D instance = AddMesh(parent, name, mesh, position, rotationDegrees,
            prototype.Palette.Materials[role], prototype.Root);
        instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
        _factionMaterialBindings.Add((instance, prototype.Palette, role, prototype.Root));
        prototype.MeshCount++;
    }

    private MeshInstance3D AddNeutralMesh(Node3D parent, string name, PrimitiveMesh mesh,
        Vector3 position, Vector3 rotationDegrees, M7LookMaterialRole role)
    {
        MeshInstance3D instance = AddMesh(parent, name, mesh, position, rotationDegrees,
            _baseMaterials[role], null);
        _neutralMaterialBindings.Add((instance, role));
        return instance;
    }

    private static MeshInstance3D AddMesh(Node3D parent, string name, PrimitiveMesh mesh,
        Vector3 position, Vector3 rotationDegrees, Material material, Node3D? textureRoot)
    {
        MeshInstance3D instance = new()
        {
            Name = name,
            Mesh = mesh,
            Position = position,
            RotationDegrees = rotationDegrees,
            MaterialOverride = material
        };
        parent.AddChild(instance);
        if (textureRoot is not null)
        {
            Transform3D modelSpace = textureRoot.GlobalTransform.AffineInverse() * instance.GlobalTransform;
            M7LookMaterialFactory.SetTextureAnchor(instance, modelSpace);
        }
        return instance;
    }

    private void SetVisualLook(ReviewLook look)
    {
        _reviewLook = look;
        ConfigureCandidateProfile();
        ApplyVisualPresentation(rebuildMaterials: true);
        SetStatus($"LOOK · {ReviewLookName(_reviewLook)} · outline {OutlineSlug()} · compare with 5 / 6 / 7");
    }

    private void ToggleOutline()
    {
        _outlineEnabled = !_outlineEnabled;
        _look.Outline.Enabled = _outlineEnabled;
        ApplyScreenLookPass();
        SetStatus($"OUTLINE · {OutlineSlug().ToUpperInvariant()} · {ReviewLookName(_reviewLook)}");
    }

    private void ApplyVisualPresentation(bool rebuildMaterials)
    {
        if (_ground is not null)
        {
            _ground.Mesh = CreateGroundMesh();
            _ground.MaterialOverride = _baseMaterials[M7LookMaterialRole.GroundRock];
        }

        if (rebuildMaterials)
        {
            foreach ((MeshInstance3D mesh, M7LookMaterialRole role) in _neutralMaterialBindings)
                mesh.MaterialOverride = _baseMaterials[role];

            foreach (FactionPalette palette in _prototypes.Select(prototype => prototype.Palette).Distinct())
            {
                Dictionary<M7LookMaterialRole, Material> refreshed = BuildFactionMaterials(palette.Key,
                    palette.PrimaryColor, palette.MechanicalColor, palette.AccentColor, palette.LightColor);
                palette.Materials.Clear();
                foreach ((M7LookMaterialRole role, Material material) in refreshed)
                    palette.Materials[role] = material;
            }
            foreach ((MeshInstance3D mesh, FactionPalette palette, M7LookMaterialRole role, Node3D textureRoot) in _factionMaterialBindings)
            {
                mesh.MaterialOverride = palette.Materials[role];
                Transform3D modelSpace = textureRoot.GlobalTransform.AffineInverse() * mesh.GlobalTransform;
                M7LookMaterialFactory.SetTextureAnchor(mesh, modelSpace);
            }
        }

        ApplyWorldLook();
        ApplyScreenLookPass();
    }

    private void ApplyWorldLook()
    {
        if (_keyLight is null || _fillLight is null || _rimLight is null ||
            _environment is null || _skyMaterial is null || _sky is null) return;

        M7WorldLightingFrame light = M7WorldLightingEvaluator.Evaluate(_look.WorldCycle);
        _keyLight.Visible = light.KeyEnergy > 0.001f;
        _keyLight.RotationDegrees = new Vector3(-light.KeyElevation, light.KeyAzimuth, 0f);
        _keyLight.LightColor = light.KeyColor;
        _keyLight.LightEnergy = light.KeyEnergy;
        _keyLight.LightAngularDistance = 0f;
        _keyLight.ShadowBlur = light.ShadowBlur;
        _keyLight.ShadowOpacity = light.ShadowOpacity;
        _fillLight.RotationDegrees = new Vector3(-light.FillElevation, light.FillAzimuth, 0f);
        _fillLight.LightColor = light.FillColor;
        _fillLight.LightEnergy = light.FillEnergy;
        _rimLight.Visible = light.RimEnabled;
        _rimLight.LightColor = light.RimColor;
        _rimLight.LightEnergy = light.RimEnergy;
        _environment.BackgroundColor = light.BackgroundColor;
        _environment.AmbientLightColor = light.AmbientColor;
        _environment.AmbientLightEnergy = light.AmbientEnergy;
        _environment.Sky = _sky;
        _environment.TonemapMode = _reviewLook is ReviewLook.CurrentCandidate or ReviewLook.Hybrid
            ? Godot.Environment.ToneMapper.Filmic
            : (Godot.Environment.ToneMapper)_look.Post.Tonemapper;
        _environment.TonemapExposure = Mathf.Pow(2f, _look.Post.Exposure);
        _environment.GlowEnabled = _look.Post.BloomEnabled;
        _environment.GlowNormalized = false;
        _environment.GlowIntensity = _look.Post.BloomIntensity;
        _environment.GlowStrength = 1.35f;
        _environment.GlowBloom = _look.Post.BloomSpread;
        _environment.GlowHdrThreshold = _look.Post.BloomThreshold;
        _environment.GlowHdrScale = 2f;

        if (_reviewLook == ReviewLook.CurrentCandidate)
        {
            _environment.BackgroundMode = Godot.Environment.BGMode.Color;
            _environment.ReflectedLightSource = Godot.Environment.ReflectionSource.Bg;
        }
        else
        {
            M7LookLab.ApplyProceduralSky(_skyMaterial, _environment, light);
        }

        if (_ground?.MaterialOverride is ShaderMaterial groundMaterial)
        {
            groundMaterial.SetShaderParameter("background_color", light.BackgroundColor);
            groundMaterial.SetShaderParameter("background_influence", _look.Lighting.BackgroundInfluence);
            groundMaterial.SetShaderParameter("world_environment", _look.WorldCycle.Environment);
        }
    }

    private void ApplyScreenLookPass()
    {
        if (_screenLookMaterial is null || _screenLookQuad is null) return;
        M7LookLab.ApplyScreenCompositeParameters(_screenLookMaterial, _look);
        bool haloEnabled = _look.Post.BloomEnabled && _look.Emission.HaloIntensity > 0.001f;
        _screenLookQuad.Visible = _look.Post.Enabled || _look.Outline.Enabled || haloEnabled;
    }

    private void BuildVfx()
    {
        _tracerPool = new PresentationVfxPool<PooledTracerEffect>(this, "AcceptanceTracer", 12, _ => new PooledTracerEffect());
        _muzzlePool = new PresentationVfxPool<PooledParticleBurst>(this, "AcceptanceMuzzle", 6,
            _ => new PooledParticleBurst(16, 0.16f, 28f, 2.5f, 7.5f, Vector3.Zero));
        _impactPool = new PresentationVfxPool<PooledParticleBurst>(this, "AcceptanceImpact", 8,
            _ => new PooledParticleBurst(20, 0.34f, 78f, 2f, 8f, new Vector3(0f, -4.2f, 0f)));
        _heroDebrisPool = new PresentationVfxPool<PooledLegoDebrisBurst>(this, "AcceptanceLegoDebris", 4,
            _ => new PooledLegoDebrisBurst(DestructionDebrisMaterial()));
        _destructionDustPool = new PresentationVfxPool<PooledParticleBurst>(this, "AcceptanceDestructionDust", 4,
            _ => new PooledParticleBurst(64, 1.2f, 82f, 1.2f, 5.8f, new Vector3(0f, -3.8f, 0f)));
        _explosionPool = new PresentationVfxPool<PooledExplosionBurst>(this, "AcceptanceExplosion", 4,
            _ => new PooledExplosionBurst());

        Color tracer = M7LookMaterialFactory.ParseColor(_look.Vfx.TracerColor);
        _tracerMaterial = M7LookMaterialFactory.Emissive(tracer, _look.Vfx.TracerEnergy, 1f, 0.28f);
        _muzzleMaterial = ParticleBillboardMaterial(tracer.Lerp(Colors.White, 0.30f), _look.Vfx.TracerEnergy, true);
        _impactMaterial = ParticleBillboardMaterial(tracer.Lerp(Colors.White, 0.58f), _look.Vfx.TracerEnergy, true);
        _dustMaterial = ParticleBillboardMaterial(new Color(_look.Destruction.DustColor) { A = _look.Destruction.DustOpacity }, 0f, false);
    }

    private void BuildHud()
    {
        _hudLayer = new CanvasLayer { Name = "AcceptanceHudLayer", Layer = 30, Visible = _hudVisible };
        AddChild(_hudLayer);
        Control frame = new() { Name = "AcceptanceHudFrame", MouseFilter = Control.MouseFilterEnum.Ignore };
        frame.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _hudLayer.AddChild(frame);
        _hud = new HudView();
        frame.AddChild(_hud);
        _hud.Configure(_hudProfile);
    }

    private void BuildReviewOverlay()
    {
        _reviewLayer = new CanvasLayer { Name = "AcceptanceReviewLayer", Layer = 80, Visible = _reviewVisible };
        AddChild(_reviewLayer);
        _reviewPanel = new PanelContainer
        {
            Name = "AcceptanceReviewPanel",
            Position = new Vector2(12f, 72f),
            CustomMinimumSize = new Vector2(470f, 0f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        _reviewPanel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("10171d"), new Color("77858c")));
        _reviewLayer.AddChild(_reviewPanel);
        VBoxContainer box = new();
        box.AddThemeConstantOverride("separation", 5);
        _reviewPanel.AddChild(box);
        Label title = UiLabel("M7 VISUAL DIRECTION · M7 FINAL + OUTLINE", 18, new Color("f0b43c"));
        box.AddChild(title);
        Label purpose = UiLabel("Approved production direction: M7 Final with outline. Current and Hybrid remain bounded laboratory comparisons; prototype geometry is not final production art.", 12, new Color("c3ccd0"));
        purpose.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        box.AddChild(purpose);

        HFlowContainer looks = new();
        box.AddChild(looks);
        AddButton(looks, "5 · CURRENT", () => SetVisualLook(ReviewLook.CurrentCandidate));
        AddButton(looks, "6 · M7 FINAL", () => SetVisualLook(ReviewLook.M7Final));
        AddButton(looks, "7 · HYBRID", () => SetVisualLook(ReviewLook.Hybrid));
        AddButton(looks, "O · OUTLINE", ToggleOutline);

        HFlowContainer zooms = new();
        box.AddChild(zooms);
        AddButton(zooms, "Z · CLOSE 24", () => SetZoom(CloseZoomCells));
        AddButton(zooms, "X · COMBAT 44", () => SetZoom(CombatZoomCells));
        AddButton(zooms, "C · STRATEGIC 72", () => SetZoom(StrategicZoomCells));
        AddButton(zooms, "0 · OVERVIEW", ShowOverview);

        HFlowContainer factions = new();
        box.AddChild(factions);
        AddButton(factions, "1 · RAIDERS", () => SetFactionFocus(0, true));
        AddButton(factions, "2 · ASTRONAUTS", () => SetFactionFocus(1, true));
        AddButton(factions, "3 · ALIENS", () => SetFactionFocus(2, true));
        AddButton(factions, "4 · MARTIANS", () => SetFactionFocus(3, true));

        HFlowContainer actions = new();
        box.AddChild(actions);
        AddButton(actions, "V · VFX", () => { SpawnShot(); SetStatus("Combat VFX replayed"); });
        AddButton(actions, "D · DESTRUCTION", () => TriggerDestruction(_focusedFaction));
        AddButton(actions, "SPACE · PAUSE", () => { _paused = !_paused; SetStatus(_paused ? "Animation paused" : "Animation resumed"); });
        AddButton(actions, "H · HUD", ToggleHud);
        AddButton(actions, "L · LABELS", ToggleLabels);

        Label checklist = UiLabel(
            "REVIEW CHECKLIST\n" +
            "1  Approved baseline: M7 FINAL with outline ON\n" +
            "2  Readability at wide start / 24 / 44 / 72 cells and scroll zoom\n" +
            "3  Four-faction differentiation and LEGO identity\n" +
            "4  Hybrid HUD and fog-correct minimap compatibility\n" +
            "5  Animation feel and mechanical function\n" +
            "6  Combat VFX clarity and restraint\n" +
            "7  Bounded LEGO destruction presentation",
            12, new Color("e4e8e9"));
        checklist.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        box.AddChild(checklist);
        _statusLabel = UiLabel($"{ReviewLookName(_reviewLook)} · outline {OutlineSlug()} · WIDE {DefaultZoomCells:0} · Rock Raiders HUD", 12, new Color("8fd0d2"));
        _statusLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        box.AddChild(_statusLabel);
        Label shortcuts = UiLabel("Tab hides this checklist · Escape returns to the playable opening", 11, new Color("9faeb5"));
        box.AddChild(shortcuts);
    }

    private void SetFactionFocus(int index, bool centerCamera)
    {
        if (_prototypes.Count != 4 || _hud is null) return;
        _focusedFaction = Math.Clamp(index, 0, 3);
        for (int i = 0; i < _prototypes.Count; i++) _prototypes[i].SelectionRing.Visible = i == _focusedFaction;
        M7HudScenario scenario = _focusedFaction switch
        {
            1 => M7HudScenario.AstronautTransform,
            2 => M7HudScenario.AlienResonance,
            3 => M7HudScenario.MartianNetwork,
            _ => M7HudScenario.RockRaiderUnit
        };
        HudFrame frame = M7HudFixtures.Create(scenario);
        frame.MatchState = "M7 DIRECTION REVIEW  ·  NOT ACCEPTED";
        _hud.ApplyFrame(frame, true);
        if (centerCamera)
        {
            _cameraFocus = _prototypes[_focusedFaction].ReviewFocus;
            ApplyCamera();
        }
        SetStatus($"{ReviewLookName(_reviewLook)} · outline {OutlineSlug()} · {ZoomName(_zoomCells)} {_zoomCells:0} · {_prototypes[_focusedFaction].Palette.DisplayName}");
    }

    private void ShowOverview()
    {
        _cameraFocus = Vector3.Zero;
        ApplyCamera();
        SetStatus($"{ReviewLookName(_reviewLook)} · outline {OutlineSlug()} · {ZoomName(_zoomCells)} {_zoomCells:0} · four-faction overview");
    }

    private void SetZoom(float zoom)
    {
        _zoomCells = ClampReviewZoom(zoom);
        ApplyCamera();
        SetStatus($"{ReviewLookName(_reviewLook)} · outline {OutlineSlug()} · {ZoomName(_zoomCells)} {_zoomCells:0.0} · mouse wheel zoom · 0 overview");
    }

    private void AdjustZoom(float multiplier)
    {
        SetZoom(_zoomCells * multiplier);
    }

    private void ApplyCamera()
    {
        if (_camera is null) return;
        Vector2 viewport = GetViewport().GetVisibleRect().Size;
        float aspect = viewport.Y <= 1f ? 16f / 9f : viewport.X / viewport.Y;
        float halfWidthWorld = _zoomCells * WorldUnitsPerCell * 0.5f;
        float distance = halfWidthWorld / Mathf.Max(0.1f, Mathf.Tan(Mathf.DegToRad(_camera.Fov * 0.5f)) * aspect);
        float yaw = Mathf.DegToRad(45f);
        float pitch = Mathf.DegToRad(58f);
        Vector3 offset = new(Mathf.Sin(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(yaw) * Mathf.Cos(pitch));
        Vector3 cameraTarget = _cameraFocus;
        if (_hudVisible)
            cameraTarget += new Vector3(1f, 0f, 1f).Normalized() * (_zoomCells * 0.12f);
        _camera.GlobalPosition = cameraTarget + offset * distance;
        _camera.LookAt(cameraTarget, Vector3.Up);
        if (_keyLight is not null) _keyLight.DirectionalShadowMaxDistance = Mathf.Clamp(distance * 1.5f, 55f, 140f);
        if (_rimLight is not null) _rimLight.RotationDegrees = new Vector3(-28f, 225f, 0f);
    }

    private void AnimateFactionProof(float delta)
    {
        if (_prototypes.Count != 4) return;
        float transformProgress = 0.5f - 0.5f * Mathf.Cos((float)_time * 1.25f);
        float functionProgress = 0.5f + 0.5f * Mathf.Sin((float)_time * 1.8f);
        for (int i = 0; i < _prototypes.Count; i++)
        {
            FactionPrototype prototype = _prototypes[i];
            bool moving = i is 0 or 1;
            bool operating = i is 0 or 3;
            bool transforming = i is 1 or 2;
            float distanceCells = _camera is null ? 0f : _camera.GlobalPosition.DistanceTo(prototype.Root.GlobalPosition);
            PresentationAnimationInput input = new((uint)(i + 1), moving ? _look.Animation.PreviewLocomotionSpeed : 0f,
                moving, operating, false, transforming, transforming ? transformProgress : 0f, 1f, 0f,
                i == 2 ? _shotSequence : 0u, i == _focusedFaction || operating || transforming, distanceCells);
            PresentationAnimationFrame frame = _animationDriver.Update(input, delta, _animationTuning);
            prototype.Rig.Apply(frame, _animationTuning);
            for (int pivotIndex = 0; pivotIndex < prototype.TransformPivots.Count; pivotIndex++)
            {
                Node3D pivot = prototype.TransformPivots[pivotIndex];
                Vector3 baseRotation = prototype.TransformBaseRotations[pivotIndex];
                float side = pivotIndex % 2 == 0 ? -1f : 1f;
                pivot.RotationDegrees = baseRotation + new Vector3(0f, side * transformProgress * 18f, side * transformProgress * 32f);
            }
            for (int pivotIndex = 0; pivotIndex < prototype.FunctionPivots.Count; pivotIndex++)
            {
                float side = pivotIndex % 2 == 0 ? -1f : 1f;
                prototype.FunctionPivots[pivotIndex].RotationDegrees = new Vector3(side * (8f + functionProgress * 20f), 0f, side * -7f);
            }
        }
    }

    private void UpdateVfx(float delta)
    {
        _tracerPool?.Update(delta);
        _muzzlePool?.Update(delta);
        _impactPool?.Update(delta);
        _heroDebrisPool?.Update(delta);
        _destructionDustPool?.Update(delta);
        _explosionPool?.Update(delta);
        if (_paused) return;

        _shotAccumulator += delta;
        if (_shotAccumulator >= _look.Vfx.PreviewShotInterval)
        {
            _shotAccumulator -= _look.Vfx.PreviewShotInterval;
            SpawnShot();
        }
        for (int i = _pendingImpacts.Count - 1; i >= 0; i--)
        {
            PendingImpact pending = _pendingImpacts[i];
            float delay = pending.Delay - delta;
            if (delay > 0f)
            {
                _pendingImpacts[i] = pending with { Delay = delay };
                continue;
            }
            SpawnImpact(pending.Position);
            _pendingImpacts.RemoveAt(i);
        }
    }

    private void SpawnShot()
    {
        if (_prototypes.Count < 3 || _tracerPool is null || _muzzlePool is null ||
            _tracerMaterial is null || _muzzleMaterial is null) return;
        Vector3 start = _prototypes[2].Root.GlobalPosition + new Vector3(0f, 1.65f, -2.4f);
        Vector3 target = new(0f, 1.0f, 0f);
        float distance = start.DistanceTo(target);
        float lifetime = distance / _look.Vfx.TracerSpeed;
        _shotSequence++;
        if (_tracerPool.TryAcquire(lifetime, out PooledTracerEffect tracer))
        {
            tracer.Configure(start, target, _look.Vfx.TracerWidth, _look.Vfx.TracerLength, _tracerMaterial);
            _tracerPool.Activate(tracer);
        }
        if (_muzzlePool.TryAcquire(0.22f, out PooledParticleBurst muzzle))
        {
            muzzle.Configure(start, target, _muzzleMaterial, Vector2.One * _look.Vfx.MuzzleSize,
                Math.Max(1, _look.Vfx.SparkCount / 2), 0.16f);
            _muzzlePool.Activate(muzzle);
        }
        _pendingImpacts.Add(new PendingImpact(lifetime, target));
    }

    private void SpawnImpact(Vector3 position)
    {
        if (_impactPool is null || _impactMaterial is null ||
            !_impactPool.TryAcquire(0.42f, out PooledParticleBurst impact)) return;
        impact.Configure(position, position + Vector3.Up, _impactMaterial,
            Vector2.One * _look.Vfx.ImpactSize, _look.Vfx.SparkCount, 0.34f);
        _impactPool.Activate(impact);
    }

    private void UpdateDestruction(float delta)
    {
        if (_destroyedFaction >= 0)
        {
            _destructionElapsed += delta;
            if (_destructionElapsed >= 4.8f) RestoreDestroyedPrototype();
            return;
        }
        if (_paused) return;
        _autoDestructionAccumulator += delta;
        if (_autoDestructionAccumulator >= 8f)
        {
            _autoDestructionAccumulator = 0f;
            TriggerDestruction(_destructionCursor);
            _destructionCursor = (_destructionCursor + 1) % 4;
        }
    }

    private void TriggerDestruction(int index)
    {
        if (_prototypes.Count != 4 || _heroDebrisPool is null || _destructionDustPool is null ||
            _explosionPool is null || _dustMaterial is null) return;
        RestoreDestroyedPrototype();
        _destroyedFaction = Math.Clamp(index, 0, 3);
        _destructionElapsed = 0f;
        FactionPrototype target = _prototypes[_destroyedFaction];
        target.Root.Visible = false;
        Vector3 size = _destroyedFaction == 3 ? new Vector3(4.8f, 5.5f, 4f) : new Vector3(5f, 2.8f, 5.5f);
        Vector3 origin = target.Root.GlobalPosition + Vector3.Up * (size.Y * 0.42f);
        uint seed = (uint)(0xA771u + _destroyedFaction * 977);
        if (_explosionPool.TryAcquire(PooledExplosionBurst.RecommendedLifetimeSeconds, out PooledExplosionBurst blast))
        {
            blast.Configure(new ExplosionBurstRequest(origin, size, 0f,
                M7LookMaterialFactory.ParseColor(_look.Destruction.BlastColor),
                _look.Destruction.BlastVisualScale, _look.Destruction.BlastEmissionEnergy,
                _look.Destruction.BlastLightEnergy, _look.Destruction.BlastLightRangeMultiplier,
                _look.Destruction.BlastFlashPersistence, _look.Destruction.BlastSmokeDelay,
                _look.Destruction.BlastSmokeProminence, _look.Destruction.BlastRingStrength));
            _explosionPool.Activate(blast);
        }
        if (_heroDebrisPool.TryAcquire(_destructionTuning.DebrisLifetime, out PooledLegoDebrisBurst debris))
        {
            debris.Configure(new LegoDebrisBurstRequest(origin, size, target.Root.GlobalRotation.Y, 0.04f,
                _destructionTuning.ResolveHeroFragmentCount(PresentationDestructionScaleBand.Heavy),
                _destructionTuning.FragmentScale, _destructionTuning.OutwardSpeed, _destructionTuning.UpwardSpeed,
                _destructionTuning.Gravity, _destructionTuning.Drag, _destructionTuning.Bounce,
                Mathf.DegToRad(_destructionTuning.AngularSpeedDegrees),
                _destructionTuning.FadeSeconds / _destructionTuning.DebrisLifetime, seed,
                target.Palette.PrimaryColor, target.Palette.MechanicalColor, target.Palette.AccentColor));
            _heroDebrisPool.Activate(debris);
        }
        if (_destructionDustPool.TryAcquire(_destructionTuning.DustLifetime, out PooledParticleBurst dust))
        {
            dust.Configure(target.Root.GlobalPosition + Vector3.Up * 0.35f,
                target.Root.GlobalPosition + Vector3.Up * 2f, _dustMaterial,
                Vector2.One * _destructionTuning.DustSize, _destructionTuning.DustCount,
                _destructionTuning.DustLifetime);
            _destructionDustPool.Activate(dust);
        }
        SetStatus($"LEGO destruction · {target.Palette.DisplayName} · restores automatically");
    }

    private void RestoreDestroyedPrototype()
    {
        if (_destroyedFaction < 0 || _destroyedFaction >= _prototypes.Count) return;
        _prototypes[_destroyedFaction].Root.Visible = true;
        _destroyedFaction = -1;
        _destructionElapsed = 0f;
    }

    private void ToggleHud()
    {
        _hudVisible = !_hudVisible;
        if (_hudLayer is not null) _hudLayer.Visible = _hudVisible;
        ApplyCamera();
        SetStatus(_hudVisible ? "Hybrid HUD and minimap visible" : "HUD hidden for unobstructed world review");
    }

    private void ToggleLabels()
    {
        _labelsVisible = !_labelsVisible;
        foreach (FactionPrototype prototype in _prototypes) prototype.Label.Visible = _labelsVisible;
        SetStatus(_labelsVisible ? "Source labels visible" : "Source labels hidden · judge silhouettes without assistance");
    }

    private void ToggleReviewPanel()
    {
        _reviewVisible = !_reviewVisible;
        if (_reviewLayer is not null) _reviewLayer.Visible = _reviewVisible;
    }

    private bool ValidateCandidate(out int meshCount)
    {
        meshCount = _prototypes.Sum(prototype => prototype.MeshCount);
        bool sources = _prototypes.Count == 4 &&
            _prototypes.Select(prototype => prototype.Palette.Source).SequenceEqual(new[]
            {
                "4970 CHROME CRUSHER", "7647 MX-41 SWITCH FIGHTER",
                "7646 ETX ALIEN INFILTRATOR", "7313 RED PLANET PROTECTOR"
            });
        bool distinctPalettes = _prototypes.Select(prototype => prototype.Palette.PrimaryColor.ToHtml()).Distinct().Count() == 4;
        bool meshes = _prototypes.All(prototype => prototype.MeshCount >= 16) && meshCount >= 80;
        bool camera = _camera is { Fov: 36f } &&
            _zoomCells is >= CloseZoomCells and <= MaximumZoomCells;
        bool ground = _ground?.MaterialOverride is ShaderMaterial groundMaterial &&
            groundMaterial.GetShaderParameter("surface_treatment").As<int>() == (int)M7GroundSurfaceTreatment.HybridSurface;
        bool lookRouting = _screenLookMaterial is not null && _screenLookQuad is not null &&
            _screenLookMaterial.GetShaderParameter("outline_enabled").As<bool>() == _outlineEnabled &&
            _screenLookMaterial.GetShaderParameter("post_enabled").As<bool>() ==
                (_reviewLook != ReviewLook.CurrentCandidate) &&
            (_reviewLook == ReviewLook.CurrentCandidate
                ? _ground?.Mesh is PlaneMesh && _environment?.BackgroundMode == Godot.Environment.BGMode.Color
                : _ground?.Mesh is ArrayMesh && _environment?.BackgroundMode == Godot.Environment.BGMode.Sky) &&
            (_reviewLook == ReviewLook.M7Final
                ? _environment?.TonemapMode == (Godot.Environment.ToneMapper)_look.Post.Tonemapper
                : _environment?.TonemapMode == Godot.Environment.ToneMapper.Filmic);
        bool infrastructure = M7LookMaterialFactory.ValidateAuthoredTextureBindings(_baseMaterials, out _) &&
            _hudProfile.ArtSkin.Finish == HudArtFinish.HybridConsole && _hud is not null &&
            _animationDriver.TrackedEntityCount == 4 && _tracerPool?.Capacity == 12 &&
            _muzzlePool?.Capacity == 6 && _impactPool?.Capacity == 8 &&
            _heroDebrisPool?.Capacity == 4 && _destructionDustPool?.Capacity == 4 && _explosionPool?.Capacity == 4;
        return sources && distinctPalettes && meshes && camera && ground && lookRouting && infrastructure;
    }

    private bool CaptureViewport(string path)
    {
        string absolute = path.StartsWith("res://", StringComparison.Ordinal)
            ? ProjectSettings.GlobalizePath(path)
            : path;
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null || image.SavePng(absolute) != Error.Ok) return false;
        GD.Print($"M7 ACCEPTANCE CANDIDATE CAPTURE: PASS path={absolute}");
        return true;
    }

    private void SetStatus(string status)
    {
        if (_statusLabel is not null) _statusLabel.Text = status;
    }

    private static float ClampReviewZoom(float value) => Mathf.Clamp(value, CloseZoomCells, MaximumZoomCells);

    private static string ZoomName(float zoom) => zoom > StrategicZoomCells
        ? "WIDE"
        : zoom >= StrategicZoomCells ? "STRATEGIC"
        : zoom <= CloseZoomCells ? "CLOSE"
        : "COMBAT";

    private static string FormatZoom(float zoom) => zoom.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

    private static ReviewLook ParseReviewLook(string? value) => value switch
    {
        "current" => ReviewLook.CurrentCandidate,
        "hybrid" => ReviewLook.Hybrid,
        _ => ReviewLook.M7Final
    };

    private static string ReviewLookSlug(ReviewLook look) => look switch
    {
        ReviewLook.M7Final => "m7-final",
        ReviewLook.Hybrid => "hybrid",
        _ => "current"
    };

    private static string ReviewLookName(ReviewLook look) => look switch
    {
        ReviewLook.M7Final => "M7 FINAL · exact schema-9 Earth Hybrid stack",
        ReviewLook.Hybrid => "HYBRID · M7 stack with current Filmic response",
        _ => "CURRENT · original acceptance candidate"
    };

    private string OutlineSlug() => _outlineEnabled ? "on" : "off";

    private static string? ParseString(string[] arguments, string key)
    {
        for (int i = 0; i + 1 < arguments.Length; i++) if (arguments[i] == key) return arguments[i + 1];
        return null;
    }

    private static StandardMaterial3D DestructionDebrisMaterial() => new()
    {
        VertexColorUseAsAlbedo = true,
        Roughness = 0.48f,
        Metallic = 0.08f
    };

    private static StandardMaterial3D ParticleBillboardMaterial(Color color, float energy, bool additive)
    {
        StandardMaterial3D material = new()
        {
            AlbedoColor = color,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            EmissionEnabled = energy > 0f,
            Emission = color,
            EmissionEnergyMultiplier = energy
        };
        if (additive) material.BlendMode = BaseMaterial3D.BlendModeEnum.Add;
        return material;
    }

    private static StyleBoxFlat PanelStyle(Color background, Color border) => new()
    {
        BgColor = new Color(background.R, background.G, background.B, 0.96f),
        BorderColor = border,
        BorderWidthLeft = 1,
        BorderWidthTop = 1,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 3,
        CornerRadiusTopRight = 3,
        CornerRadiusBottomLeft = 3,
        CornerRadiusBottomRight = 3,
        ContentMarginLeft = 10f,
        ContentMarginTop = 8f,
        ContentMarginRight = 10f,
        ContentMarginBottom = 8f
    };

    private static Label UiLabel(string text, int size, Color color)
    {
        Label label = new() { Text = text };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private static void AddButton(Container parent, string text, Action action)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(0f, 28f) };
        button.Pressed += action;
        parent.AddChild(button);
    }
}
