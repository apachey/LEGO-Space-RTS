using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

/// <summary>Player-visible T081 Blender-to-Godot round-trip fixture.</summary>
public partial class M85AssetPipelineLab : Node3D
{
    private static readonly (string Name, Vector3 Position)[] LodColumns =
    {
        ("Close", new Vector3(-7f, 0f, 0f)),
        ("Combat", Vector3.Zero),
        ("Strategic", new Vector3(7f, 0f, 0f))
    };

    private readonly List<Node3D> _models = new();
    private readonly List<Node3D> _pivotMarkers = new();
    private readonly List<MeshInstance3D> _roleMeshes = new();
    private Action? _returnToPrototype;
    private Camera3D? _camera;
    private Label? _status;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private int _captureFrame = 45;
    private string? _capturePath;
    private float _zoomCells = 44f;
    private float _cameraYawDegrees = 36f;
    private double _time;
    private M85AssetPipelineValidationReport _report;
    private string _validationError = string.Empty;

    public void Configure(Action returnToPrototype, string[] arguments)
    {
        Name = "M85AssetPipelineLab";
        _returnToPrototype = returnToPrototype;
        _smoke = arguments.Contains("--m85-asset-pipeline-smoke");
        _capturePath = ParseString(arguments, "--capture-path");
        if (int.TryParse(ParseString(arguments, "--m85-asset-pipeline-capture-frame"), out int captureFrame))
            _captureFrame = Math.Clamp(captureFrame, 20, 300);
        if (float.TryParse(ParseString(arguments, "--m85-asset-pipeline-zoom"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float zoom))
            _zoomCells = Mathf.Clamp(zoom, 24f, 72f);
        if (float.TryParse(ParseString(arguments, "--m85-asset-pipeline-yaw"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float yaw))
            _cameraYawDegrees = NormalizeDegrees(yaw);

        BuildWorld();
        BuildModels();
        BuildOverlay();
        ApplyCamera();
        ProcessPriority = 1000;
        GD.Print($"M8.5 ASSET PIPELINE: active fixture=non-roster source=blend export=glb import=PackedScene lods=Close,Combat,Strategic zoom={_zoomCells:0} yaw={_cameraYawDegrees:0} Z/X/C=24/44/72 Q/E=orbit Escape=return");
    }

    public override void _Process(double delta)
    {
        _time += Math.Min(delta, 0.1);
        for (int i = 0; i < _pivotMarkers.Count; i++)
            _pivotMarkers[i].Rotation = new Vector3(0f, (float)_time * (0.55f + i * 0.08f), 0f);

        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < _captureFrame) return;
        bool valid = _models.Count == 3 && string.IsNullOrEmpty(_validationError) &&
            _report.CloseTriangles == 868 && _report.CombatTriangles == 332 &&
            _report.StrategicTriangles == 168 && _report.PivotCount == 6 &&
            _report.SocketCount == 6 && _roleMeshes.Count == 84;
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
        {
            GD.Print($"M8.5 ASSET PIPELINE: PASS source=blend export=glb import=PackedScene root=ground-centre scale=1 cellWorldUnits={M85AssetPipelineContract.WorldUnitsPerBuildCell:0} forward=-Z lods=3 close={_report.CloseTriangles} combat={_report.CombatTriangles} strategic={_report.StrategicTriangles} pivots={_report.PivotCount} sockets={_report.SocketCount} roleBindings={_roleMeshes.Count} zoom={_zoomCells:0} yaw={_cameraYawDegrees:0}");
        }
        else
        {
            GD.PrintErr($"M8.5 ASSET PIPELINE: FAIL models={_models.Count} roleBindings={_roleMeshes.Count} reason={_validationError}");
        }
        AutomatedSmokeExit.Finish(this, valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        switch (key.Keycode)
        {
            case Key.Z: SetZoom(24f); break;
            case Key.X: SetZoom(44f); break;
            case Key.C: SetZoom(72f); break;
            case Key.Q:
            case Key.Left: RotateCamera(-45f); break;
            case Key.E:
            case Key.Right: RotateCamera(45f); break;
            case Key.Escape: _returnToPrototype?.Invoke(); break;
            default: return;
        }
        GetViewport().SetInputAsHandled();
    }

    private void BuildWorld()
    {
        _camera = new Camera3D { Name = "M85PipelineCamera", Current = true, Fov = 36f, Near = 0.2f, Far = 300f };
        AddChild(_camera);
        WorldEnvironment world = new()
        {
            Name = "M85PipelineEnvironment",
            Environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color("#29302d"),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color("#aeb8b2"),
                AmbientLightEnergy = 0.72f,
                TonemapMode = Godot.Environment.ToneMapper.Aces,
                TonemapExposure = 1.0f
            }
        };
        AddChild(world);
        DirectionalLight3D key = new()
        {
            Name = "M85PipelineKey",
            RotationDegrees = new Vector3(-52f, -32f, 0f),
            LightColor = new Color("#fff0d2"),
            LightEnergy = 1.55f,
            ShadowEnabled = true,
            ShadowOpacity = 0.72f
        };
        AddChild(key);
        DirectionalLight3D fill = new()
        {
            Name = "M85PipelineFill",
            RotationDegrees = new Vector3(-36f, 142f, 0f),
            LightColor = new Color("#b9d7ef"),
            LightEnergy = 0.48f
        };
        AddChild(fill);

        BoxMesh groundMesh = new() { Size = new Vector3(28f, 0.18f, 12f) };
        MeshInstance3D ground = new()
        {
            Name = "PipelineReviewGround",
            Mesh = groundMesh,
            Position = new Vector3(0f, -0.1f, 0f),
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color("#4a544a"), Roughness = 0.92f
            }
        };
        AddChild(ground);
    }

    private void BuildModels()
    {
        PackedScene packed = GD.Load<PackedScene>(M85AssetPipelineContract.RuntimePath) ??
            throw new InvalidOperationException($"Unable to load T081 pipeline asset: {M85AssetPipelineContract.RuntimePath}");
        Dictionary<M7LookMaterialRole, Material> materials =
            M7LookMaterialFactory.BuildSharedMaterials(M7LookProfile.CreateDefault());
        for (int index = 0; index < LodColumns.Length; index++)
        {
            (string lodName, Vector3 position) = LodColumns[index];
            Node3D model = packed.Instantiate<Node3D>();
            if (!M85AssetPipelineContract.ValidateImportedScene(model, out M85AssetPipelineValidationReport report,
                    out string error))
            {
                _validationError = error;
                GD.PrintErr($"M8.5 ASSET PIPELINE IMPORT: FAIL {error}");
            }
            else _report = report;
            model.Name = $"PipelineReference_{lodName}";
            model.Position = position;
            AddChild(model);
            M85AssetPipelineContract.ShowOnlyLod(model, lodName);
            BindAcceptedMaterials(model, model, materials);
            AddAttachmentMarkers(model);
            AddLodLabel(lodName, position, lodName switch { "Close" => 868, "Combat" => 332, _ => 168 });
            _models.Add(model);
        }
    }

    private void BindAcceptedMaterials(Node root, Node3D model,
        IReadOnlyDictionary<M7LookMaterialRole, Material> materials)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                M7LookMaterialRole role = M7LookMaterialFactory.InferRole(mesh.Name.ToString());
                mesh.MaterialOverride = materials[role];
                Transform3D modelSpace = model.GlobalTransform.AffineInverse() * mesh.GlobalTransform;
                M7LookMaterialFactory.SetTextureAnchor(mesh, modelSpace);
                _roleMeshes.Add(mesh);
            }
            BindAcceptedMaterials(child, model, materials);
        }
    }

    private void AddAttachmentMarkers(Node3D model)
    {
        foreach (string socketName in new[] { "Socket_Selection", "Socket_Health", "Socket_Weapon_Primary" })
        {
            if (model.FindChild(socketName, true, false) is not Node3D socket) continue;
            MeshInstance3D marker = new()
            {
                Name = $"Marker_{socketName}",
                Mesh = new SphereMesh { Radius = 0.10f, Height = 0.20f, RadialSegments = 10, Rings = 5 },
                MaterialOverride = MarkerMaterial(socketName == "Socket_Weapon_Primary" ? new Color("#ff6b35") : new Color("#62f5ff"))
            };
            socket.AddChild(marker);
        }
        foreach (string pivotName in new[]
        {
            "Pivot_ToolPrimary",
            "Pivot_Wheel_Left_Front", "Pivot_Wheel_Left_Rear",
            "Pivot_Wheel_Right_Front", "Pivot_Wheel_Right_Rear"
        })
        {
            if (model.FindChild(pivotName, true, false) is not Node3D pivot) continue;
            MeshInstance3D marker = new()
            {
                Name = $"Marker_{pivotName}",
                Mesh = new CylinderMesh { TopRadius = 0.13f, BottomRadius = 0.13f, Height = 0.34f, RadialSegments = 10 },
                MaterialOverride = MarkerMaterial(new Color("#ffd24a"))
            };
            pivot.AddChild(marker);
            _pivotMarkers.Add(marker);
        }
    }

    private void AddLodLabel(string lodName, Vector3 position, int triangles)
    {
        Label3D label = new()
        {
            Name = $"Label_{lodName}",
            Text = $"{lodName.ToUpperInvariant()}\n{triangles} TRIANGLES",
            Position = position + new Vector3(0f, 3.2f, 0f),
            FontSize = 40,
            OutlineSize = 8,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            Modulate = Colors.White
        };
        AddChild(label);
    }

    private void BuildOverlay()
    {
        CanvasLayer layer = new() { Name = "M85PipelineReviewUI", Layer = 30 };
        AddChild(layer);
        PanelContainer panel = new()
        {
            Position = new Vector2(24f, 24f),
            CustomMinimumSize = new Vector2(650f, 0f)
        };
        layer.AddChild(panel);
        VBoxContainer box = new(); panel.AddChild(box);
        Label title = new() { Text = "M8.5 · T081 ASSET PIPELINE ROUND TRIP" };
        title.AddThemeFontSizeOverride("font_size", 20); box.AddChild(title);
        Label body = new()
        {
            Text = "One Blender source → deterministic GLB → Godot PackedScene\n" +
                   "Cyan: selection/health · Orange: weapon · Yellow: mechanical pivots\n" +
                   "Q / E or ← / →: rotate camera · Z / X / C: 24 / 44 / 72 cells · Esc: return"
        };
        body.AddThemeFontSizeOverride("font_size", 14); box.AddChild(body);
        _status = new Label(); _status.AddThemeFontSizeOverride("font_size", 14); box.AddChild(_status);
        HBoxContainer cameraControls = new(); box.AddChild(cameraControls);
        Button rotateLeft = new() { Text = "↺ ROTATE LEFT" };
        rotateLeft.Pressed += () => RotateCamera(-45f); cameraControls.AddChild(rotateLeft);
        Button rotateRight = new() { Text = "ROTATE RIGHT ↻" };
        rotateRight.Pressed += () => RotateCamera(45f); cameraControls.AddChild(rotateRight);
        Button returnButton = new() { Text = "RETURN TO PROTOTYPE" };
        returnButton.Pressed += () => _returnToPrototype?.Invoke(); box.AddChild(returnButton);
        UpdateStatus();
    }

    private void SetZoom(float zoom)
    {
        _zoomCells = zoom;
        ApplyCamera();
        UpdateStatus();
    }

    private void RotateCamera(float deltaDegrees)
    {
        _cameraYawDegrees = NormalizeDegrees(_cameraYawDegrees + deltaDegrees);
        ApplyCamera();
        UpdateStatus();
    }

    private void ApplyCamera()
    {
        if (_camera is null) return;
        Vector2 size = GetViewport()?.GetVisibleRect().Size ?? new Vector2(1920f, 1080f);
        float aspect = size.Y <= 1f ? 16f / 9f : size.X / size.Y;
        float halfWidthWorld = _zoomCells * GodotConversions.WorldUnitsPerBuildCell * 0.5f;
        float distance = halfWidthWorld / Mathf.Max(0.1f, Mathf.Tan(Mathf.DegToRad(_camera.Fov * 0.5f)) * aspect);
        float yaw = Mathf.DegToRad(_cameraYawDegrees);
        float pitch = Mathf.DegToRad(58f);
        Vector3 direction = new(Mathf.Sin(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(yaw) * Mathf.Cos(pitch));
        Vector3 focus = new(0f, 0.9f, 0f);
        _camera.GlobalPosition = focus + direction * distance;
        _camera.LookAt(focus, Vector3.Up);
    }

    private void UpdateStatus()
    {
        if (_status is null) return;
        _status.Text = string.IsNullOrEmpty(_validationError)
            ? $"PASS · root/scale/axes preserved · LOD 868 → 332 → 168 · zoom {_zoomCells:0} cells · camera {_cameraYawDegrees:0}°"
            : $"FAIL · {_validationError}";
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null) return false;
        Error result = image.SavePng(path);
        if (result != Error.Ok) GD.PrintErr($"M8.5 ASSET PIPELINE: capture failed {result} path={path}");
        return result == Error.Ok;
    }

    private static StandardMaterial3D MarkerMaterial(Color color) => new()
    {
        AlbedoColor = color,
        EmissionEnabled = true,
        Emission = color,
        EmissionEnergyMultiplier = 1.8f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        NoDepthTest = true
    };

    private static float NormalizeDegrees(float degrees)
    {
        float normalized = degrees % 360f;
        return normalized < 0f ? normalized + 360f : normalized;
    }

    private static string? ParseString(string[] arguments, string name)
    {
        int index = Array.IndexOf(arguments, name);
        return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
    }
}
