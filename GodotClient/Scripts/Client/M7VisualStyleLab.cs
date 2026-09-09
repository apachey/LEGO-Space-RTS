using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class M7VisualStyleLab : Node3D
{
    private const string ModelPath = "res://Assets/M7/raider_drill_rig.glb";
    private readonly List<MeshInstance3D> _modelMeshes = new();
    private readonly List<Node3D> _wheelPivots = new();
    private readonly List<MeshInstance3D> _dust = new();
    private readonly List<MeshInstance3D> _sparks = new();
    private readonly Dictionary<M7SurfaceSemantic, int> _semanticCounts = new();
    private Action? _returnToPrototype;
    private Node3D? _model;
    private Node3D? _suspension;
    private Node3D? _drill;
    private Vector3 _suspensionBase;
    private MeshInstance3D? _ground;
    private DirectionalLight3D? _sun;
    private OmniLight3D? _workFill;
    private Godot.Environment? _environment;
    private M7VisualStyle _style;
    private bool _outlineEnabled;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private double _time;
    private string? _capturePath;

    public void Configure(Action returnToPrototype, string[] commandLineArgs)
    {
        Name = "M7VisualStyleLab";
        _returnToPrototype = returnToPrototype;
        _smoke = commandLineArgs.Contains("--m7-style-smoke");
        _style = ParseStyle(commandLineArgs);
        _outlineEnabled = ParseOutline(commandLineArgs);
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];

        BuildScene();
        ApplyStyle(_style);
        ProcessPriority = 1000;
        GD.Print($"M7 STYLE LAB: active={M7StyleMaterialFactory.Slug(_style)} outline={OutlineSlug()} controls=1..4/O/Escape");
    }

    public override void _Process(double delta)
    {
        _time += delta;
        AnimateUnit();
        AnimateEffects();

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < 20) return;

        bool valid = ValidateLab(out int triangles);
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 STYLE LAB: PASS styles={M7StyleMaterialFactory.Styles.Length} meshes={_modelMeshes.Count} triangles={triangles} canvasLayers=0 active={M7StyleMaterialFactory.Slug(_style)} outline={OutlineSlug()}");
        else
            GD.PrintErr($"M7 STYLE LAB: FAIL styles={M7StyleMaterialFactory.Styles.Length} meshes={_modelMeshes.Count}");
        AutomatedSmokeExit.Finish(this, valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        M7VisualStyle? next = key.Keycode switch
        {
            Key.Key1 => M7VisualStyle.IndustrialMass,
            Key.Key2 => M7VisualStyle.HeroicRts,
            Key.Key3 => M7VisualStyle.ConstructiveLego,
            Key.Key4 => M7VisualStyle.GraphicVolume,
            _ => null
        };
        if (next.HasValue)
        {
            ApplyStyle(next.Value);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.O)
        {
            _outlineEnabled = !_outlineEnabled;
            ApplyStyle(_style);
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
        Camera3D camera = new()
        {
            Name = "StyleLabCamera",
            Current = true,
            Fov = 36f,
            Position = new Vector3(9.6f, 8.0f, 12.6f)
        };
        AddChild(camera);
        camera.LookAt(new Vector3(0f, 1.05f, -0.25f), Vector3.Up);

        _sun = new DirectionalLight3D
        {
            Name = "InvariantSun",
            RotationDegrees = new Vector3(-54f, -38f, 0f),
            ShadowEnabled = true,
            DirectionalShadowMaxDistance = 40f
        };
        AddChild(_sun);

        _workFill = new OmniLight3D
        {
            Name = "InvariantWorkFill",
            Position = new Vector3(-4.6f, 4.3f, 3.2f),
            OmniRange = 11f,
            ShadowEnabled = false
        };
        AddChild(_workFill);

        _environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            GlowEnabled = true,
            GlowBloom = 0.08f
        };
        AddChild(new WorldEnvironment { Name = "StyleEnvironment", Environment = _environment });

        _ground = new MeshInstance3D { Name = "InvariantBasaltGround", Mesh = BuildGroundMesh() };
        AddChild(_ground);

        PackedScene packed = GD.Load<PackedScene>(ModelPath) ?? throw new InvalidOperationException($"Unable to load M7 style model: {ModelPath}");
        _model = packed.Instantiate<Node3D>();
        _model.Name = "InvariantStyleUnit";
        _model.Position = new Vector3(0f, 0.08f, 0f);
        AddChild(_model);

        CollectModelNodes(_model);
        _suspension = _model.FindChild("Pivot_Suspension", true, false) as Node3D;
        _drill = _model.FindChild("Pivot_Drill", true, false) as Node3D;
        if (_suspension is not null) _suspensionBase = _suspension.Position;
        BuildEffects();
    }

    private void CollectModelNodes(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                _modelMeshes.Add(mesh);
                M7SurfaceSemantic semantic = InferSemantic(mesh.Name);
                _semanticCounts[semantic] = _semanticCounts.GetValueOrDefault(semantic) + 1;
            }
            if (child is Node3D node && node.Name.ToString().StartsWith("Pivot_Wheel_", StringComparison.Ordinal))
                _wheelPivots.Add(node);
            CollectModelNodes(child);
        }
    }

    private void BuildEffects()
    {
        for (int i = 0; i < 7; i++)
        {
            MeshInstance3D puff = new()
            {
                Name = $"DustPuff_{i}",
                Mesh = new SphereMesh { Radius = 0.22f, Height = 0.32f, RadialSegments = 8, Rings = 4 }
            };
            AddChild(puff);
            _dust.Add(puff);
        }
        for (int i = 0; i < 6; i++)
        {
            MeshInstance3D spark = new()
            {
                Name = $"DrillSpark_{i}",
                Mesh = new BoxMesh { Size = new Vector3(0.035f, 0.035f, 0.32f) }
            };
            AddChild(spark);
            _sparks.Add(spark);
        }
    }

    private void ApplyStyle(M7VisualStyle style)
    {
        _style = style;
        foreach (MeshInstance3D mesh in _modelMeshes)
            mesh.MaterialOverride = M7StyleMaterialFactory.Create(style, InferSemantic(mesh.Name), _outlineEnabled);
        if (_ground is not null) _ground.MaterialOverride = M7StyleMaterialFactory.Create(style, M7SurfaceSemantic.Terrain);
        foreach (MeshInstance3D puff in _dust) puff.MaterialOverride = M7StyleMaterialFactory.Create(style, M7SurfaceSemantic.Dust);
        foreach (MeshInstance3D spark in _sparks) spark.MaterialOverride = M7StyleMaterialFactory.Create(style, M7SurfaceSemantic.Spark);
        ApplyLightProfile(style);
        GD.Print($"M7 STYLE LAB SWITCH: {M7StyleMaterialFactory.Slug(style)} outline={OutlineSlug()}");
    }

    private void ApplyLightProfile(M7VisualStyle style)
    {
        if (_sun is null || _workFill is null || _environment is null) return;
        switch (style)
        {
            case M7VisualStyle.IndustrialMass:
                _sun.LightColor = new Color("fff3e6"); _sun.LightEnergy = 1.68f;
                _workFill.LightColor = new Color("89aed1"); _workFill.LightEnergy = 0.58f;
                _environment.BackgroundColor = new Color("151b1f");
                _environment.AmbientLightColor = new Color("9ba5aa"); _environment.AmbientLightEnergy = 0.67f;
                _environment.GlowIntensity = 0.20f;
                break;
            case M7VisualStyle.HeroicRts:
                _sun.LightColor = new Color("ffe8c9"); _sun.LightEnergy = 1.72f;
                _workFill.LightColor = new Color("72a8ff"); _workFill.LightEnergy = 1.06f;
                _environment.BackgroundColor = new Color("111b2a");
                _environment.AmbientLightColor = new Color("7897bd"); _environment.AmbientLightEnergy = 0.48f;
                _environment.GlowIntensity = 0.34f;
                break;
            case M7VisualStyle.ConstructiveLego:
                _sun.LightColor = new Color("fff4df"); _sun.LightEnergy = 1.58f;
                _workFill.LightColor = new Color("c4ddf2"); _workFill.LightEnergy = 0.64f;
                _environment.BackgroundColor = new Color("222a30");
                _environment.AmbientLightColor = new Color("bec5ca"); _environment.AmbientLightEnergy = 0.78f;
                _environment.GlowIntensity = 0.20f;
                break;
            case M7VisualStyle.GraphicVolume:
                _sun.LightColor = new Color("ffdc9a"); _sun.LightEnergy = 1.62f;
                _workFill.LightColor = new Color("70a9ff"); _workFill.LightEnergy = 0.88f;
                _environment.BackgroundColor = new Color("152132");
                _environment.AmbientLightColor = new Color("829bb7"); _environment.AmbientLightEnergy = 0.48f;
                _environment.GlowIntensity = 0.28f;
                break;
        }
    }

    private void AnimateUnit()
    {
        if (_drill is not null) _drill.RotateObjectLocal(Vector3.Forward, 5.6f * (float)GetProcessDeltaTime());
        for (int i = 0; i < _wheelPivots.Count; i++)
            _wheelPivots[i].Rotation = new Vector3((float)Math.Sin(_time * 0.42) * 0.055f, 0f, 0f);
        if (_suspension is not null)
            _suspension.Position = _suspensionBase + Vector3.Up * ((float)Math.Sin(_time * 1.85) * 0.025f);
    }

    private void AnimateEffects()
    {
        for (int i = 0; i < _dust.Count; i++)
        {
            float phase = Mathf.PosMod((float)_time * 0.46f + i / (float)_dust.Count, 1f);
            float angle = i * Mathf.Tau / _dust.Count + phase * 0.75f;
            float radius = 0.18f + phase * 0.72f;
            _dust[i].Position = new Vector3(Mathf.Cos(angle) * radius, 0.11f + phase * 0.16f, -5.06f + Mathf.Sin(angle) * radius * 0.40f);
            _dust[i].Scale = Vector3.One * (0.18f + phase * 0.44f) * (1f - phase * 0.55f);
        }

        float burst = Mathf.PosMod((float)_time, 1.35f) / 1.35f;
        for (int i = 0; i < _sparks.Count; i++)
        {
            float local = Mathf.PosMod(burst + i * 0.11f, 1f);
            float angle = -0.95f + i * 0.38f;
            _sparks[i].Position = new Vector3(Mathf.Sin(angle) * local * 1.15f, 0.20f + local * 0.82f, -5.12f - local * 0.42f);
            _sparks[i].Rotation = new Vector3(angle, 0f, -angle * 0.65f);
            _sparks[i].Scale = Vector3.One * (1f - local * 0.82f);
        }
    }

    private bool ValidateLab(out int triangles)
    {
        triangles = 0;
        foreach (MeshInstance3D mesh in _modelMeshes)
            if (mesh.Mesh is Mesh source) triangles += source.GetFaces().Length / 3;

        bool semantics = _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Body) >= 3 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Earth) >= 2 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Tool) >= 6 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Rubber) >= 4 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Glass) >= 1 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Signal) >= 2 &&
            _semanticCounts.GetValueOrDefault(M7SurfaceSemantic.Lamp) >= 1;

        foreach (M7VisualStyle style in M7StyleMaterialFactory.Styles)
            foreach (M7SurfaceSemantic semantic in Enum.GetValues<M7SurfaceSemantic>())
                if (M7StyleMaterialFactory.Create(style, semantic) is null) return false;

        Material outlinedBody = M7StyleMaterialFactory.Create(M7VisualStyle.HeroicRts, M7SurfaceSemantic.Body, outlineEnabled: true);
        Material outlinedGlass = M7StyleMaterialFactory.Create(M7VisualStyle.HeroicRts, M7SurfaceSemantic.Glass, outlineEnabled: true);
        if (outlinedBody.NextPass is null || outlinedGlass.NextPass is not null) return false;

        return _model is not null && _suspension is not null && _drill is not null &&
            _modelMeshes.Count >= 45 && triangles is >= 7_500 and <= 20_000 &&
            _wheelPivots.Count == 6 && semantics && _ground is not null &&
            GetNodeOrNull<Camera3D>("StyleLabCamera") is { Fov: 36f } &&
            FindChildren("*", nameof(CanvasLayer), true, false).Count == 0;
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null) return false;
        Error result = image.SavePng(path);
        if (result != Error.Ok) return false;
        GD.Print($"M7 STYLE LAB CAPTURE: PASS style={M7StyleMaterialFactory.Slug(_style)} path={path}");
        return true;
    }

    private static M7VisualStyle ParseStyle(string[] arguments)
    {
        for (int i = 0; i + 1 < arguments.Length; i++)
            if (arguments[i] == "--m7-style") return M7StyleMaterialFactory.Parse(arguments[i + 1]);
        return M7VisualStyle.IndustrialMass;
    }

    private static bool ParseOutline(string[] arguments)
    {
        for (int i = 0; i + 1 < arguments.Length; i++)
            if (arguments[i] == "--m7-outline")
                return arguments[i + 1] is "on" or "true" or "1";
        return false;
    }

    private string OutlineSlug() => _outlineEnabled ? "on" : "off";

    private static M7SurfaceSemantic InferSemantic(StringName nodeName)
    {
        string name = nodeName.ToString();
        if (name is "Neutral_Chassis" or "Body_RearHousing") return M7SurfaceSemantic.Earth;
        if (name.StartsWith("Body_", StringComparison.Ordinal)) return M7SurfaceSemantic.Body;
        if (name.StartsWith("Accent_", StringComparison.Ordinal)) return M7SurfaceSemantic.Accent;
        if (name.StartsWith("Tool_", StringComparison.Ordinal)) return M7SurfaceSemantic.Tool;
        if (name.StartsWith("Rubber_", StringComparison.Ordinal)) return M7SurfaceSemantic.Rubber;
        if (name.StartsWith("Glass_", StringComparison.Ordinal)) return M7SurfaceSemantic.Glass;
        if (name.StartsWith("Signal_", StringComparison.Ordinal)) return M7SurfaceSemantic.Signal;
        if (name.StartsWith("Lamp_", StringComparison.Ordinal)) return M7SurfaceSemantic.Lamp;
        return M7SurfaceSemantic.Neutral;
    }

    private static ArrayMesh BuildGroundMesh()
    {
        const int cells = 30;
        const float extent = 12f;
        SurfaceTool surface = new();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        for (int z = 0; z < cells; z++)
        {
            for (int x = 0; x < cells; x++)
            {
                float x0 = Mathf.Lerp(-extent, extent, x / (float)cells);
                float x1 = Mathf.Lerp(-extent, extent, (x + 1) / (float)cells);
                float z0 = Mathf.Lerp(-extent, extent, z / (float)cells);
                float z1 = Mathf.Lerp(-extent, extent, (z + 1) / (float)cells);
                Vector3 a = new(x0, Height(x0, z0), z0);
                Vector3 b = new(x1, Height(x1, z0), z0);
                Vector3 c = new(x1, Height(x1, z1), z1);
                Vector3 d = new(x0, Height(x0, z1), z1);
                AddTriangle(surface, a, c, b, new Vector2(x / 6f, z / 6f), new Vector2((x + 1) / 6f, (z + 1) / 6f), new Vector2((x + 1) / 6f, z / 6f));
                AddTriangle(surface, a, d, c, new Vector2(x / 6f, z / 6f), new Vector2(x / 6f, (z + 1) / 6f), new Vector2((x + 1) / 6f, (z + 1) / 6f));
            }
        }
        return surface.Commit();
    }

    private static float Height(float x, float z)
    {
        float distance = new Vector2(x, z).Length();
        float flatten = Mathf.SmoothStep(0f, 1f, Mathf.Clamp((distance - 4.2f) / 4f, 0f, 1f));
        float broad = Mathf.Sin(x * 0.39f) * 0.10f + Mathf.Cos(z * 0.31f) * 0.08f + Mathf.Sin((x + z) * 0.18f) * 0.07f;
        return -0.04f + broad * flatten;
    }

    private static void AddTriangle(SurfaceTool surface, Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC)
    {
        Vector3 normal = (b - a).Cross(c - a).Normalized();
        surface.SetNormal(normal); surface.SetUV(uvA); surface.AddVertex(a);
        surface.SetNormal(normal); surface.SetUV(uvB); surface.AddVertex(b);
        surface.SetNormal(normal); surface.SetUV(uvC); surface.AddVertex(c);
    }
}
