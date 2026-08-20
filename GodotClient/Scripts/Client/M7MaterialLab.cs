using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class M7MaterialLab : Node3D
{
    private readonly Dictionary<LegoMaterialFamily, StandardMaterial3D> _acceptanceMaterials = new();
    private Action? _returnToPrototype;
    private string? _capturePath;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private int _factionSwatchCount;
    private int _teamIdentificationTileCount;

    public void Configure(Action returnToPrototype, string[] commandLineArgs)
    {
        Name = "M7MaterialLab";
        _returnToPrototype = returnToPrototype;
        _smoke = commandLineArgs.Contains("--m7-material-smoke");
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];

        BuildMaterialSet();
        BuildStage();
        BuildInterface();
        ProcessPriority = 1000;
    }

    public override void _Process(double delta)
    {
        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < 12) return;

        bool valid = ValidateMaterialMasters();
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 T064 MATERIAL LAB: PASS families={_acceptanceMaterials.Count} factionSwatches={_factionSwatchCount} identificationTiles={_teamIdentificationTileCount}");
        else
            GD.PrintErr($"M7 T064 MATERIAL LAB: FAIL families={_acceptanceMaterials.Count} factionSwatches={_factionSwatchCount} identificationTiles={_teamIdentificationTileCount}");
        GetTree().Quit(valid ? 0 : 2);
    }

    private void BuildMaterialSet()
    {
        _acceptanceMaterials[LegoMaterialFamily.MoldedPolymer] = LegoMaterialLibrary.MoldedPolymer(M7DraftPalette.RockRaiderDarkTurquoise);
        _acceptanceMaterials[LegoMaterialFamily.ToolMetal] = LegoMaterialLibrary.ToolMetal(M7DraftPalette.ToolSteel);
        _acceptanceMaterials[LegoMaterialFamily.Rubber] = LegoMaterialLibrary.Rubber(M7DraftPalette.Rubber);
        _acceptanceMaterials[LegoMaterialFamily.TransparentPolymer] = LegoMaterialLibrary.TransparentPolymer(M7DraftPalette.TransparentBrown);
        _acceptanceMaterials[LegoMaterialFamily.Crystal] = LegoMaterialLibrary.Crystal(M7DraftPalette.CrystalGreen);
        _acceptanceMaterials[LegoMaterialFamily.Terrain] = LegoMaterialLibrary.Terrain(M7DraftPalette.Basalt);
    }

    private void BuildStage()
    {
        M7LightingRig.AddNeutralGameplayRig(this);

        Camera3D camera = new()
        {
            Name = "MaterialLabCamera",
            Current = true,
            Fov = 36f,
            Position = new Vector3(0f, 15.5f, 24.5f)
        };
        AddChild(camera);
        camera.LookAt(new Vector3(0f, 1.0f, 0f), Vector3.Up);

        AddMesh("BasaltStage", new BoxMesh { Size = new Vector3(34f, 0.45f, 23f) }, _acceptanceMaterials[LegoMaterialFamily.Terrain], new Vector3(0f, -0.3f, 0f));
        AddMesh("Backdrop", new BoxMesh { Size = new Vector3(34f, 5f, 0.5f) }, LegoMaterialLibrary.Terrain(new Color("24272a")), new Vector3(0f, 2.2f, -10.5f));

        AddFactionSwatch("ROCK RAIDERS", -12f, M7DraftPalette.RockRaiderDarkTurquoise, M7DraftPalette.ToolSteel);
        AddFactionSwatch("ASTRONAUT FIELD", -6f, M7DraftPalette.AstronautFieldBlue, M7DraftPalette.AstronautMissionWhite);
        AddFactionSwatch("ASTRONAUT MISSION", 0f, M7DraftPalette.AstronautMissionWhite, M7DraftPalette.AstronautMissionOrange);
        AddFactionSwatch("ALIENS", 6f, M7DraftPalette.AlienBlack, M7DraftPalette.AlienLime, emissionEnergy: 0.45f);
        AddFactionSwatch("MARTIANS", 12f, M7DraftPalette.MartianSandPurple, M7DraftPalette.MartianSandRed);

        AddFamilySample("POLYMER 0.40", LegoMaterialFamily.MoldedPolymer, -12.5f);
        AddFamilySample("TOOL METAL 0.30", LegoMaterialFamily.ToolMetal, -7.5f);
        AddFamilySample("RUBBER 0.78", LegoMaterialFamily.Rubber, -2.5f);
        AddFamilySample("TRANS POLYMER", LegoMaterialFamily.TransparentPolymer, 2.5f);
        AddFamilySample("CRYSTAL", LegoMaterialFamily.Crystal, 7.5f);
        AddFamilySample("BASALT 0.90", LegoMaterialFamily.Terrain, 12.5f);

        AddRoughnessSample("0.32", -4f, 0.32f);
        AddRoughnessSample("0.40", 0f, LegoMaterialLibrary.MoldedPolymerRoughness);
        AddRoughnessSample("0.50", 4f, 0.50f);
        AddWorldLabel("MOLDED POLYMER ROUGHNESS RANGE", new Vector3(0f, 0.2f, 9.0f), 30);
    }

    private void AddFactionSwatch(string label, float x, Color bodyColor, Color accentColor, float emissionEnergy = 0f)
    {
        Node3D root = new() { Name = $"FactionSwatch_{label.Replace(' ', '_')}", Position = new Vector3(x, 0f, -5.5f) };
        AddChild(root);

        StandardMaterial3D body = LegoMaterialLibrary.MoldedPolymer(bodyColor);
        StandardMaterial3D accent = emissionEnergy > 0f
            ? LegoMaterialLibrary.Create(LegoMaterialFamily.MoldedPolymer, accentColor, emissionEnergy)
            : LegoMaterialLibrary.MoldedPolymer(accentColor);
        AddMesh("Body", new BoxMesh { Size = new Vector3(4.2f, 1.25f, 2.8f) }, body, new Vector3(0f, 0.65f, 0f), root);
        AddMesh("Accent", new BoxMesh { Size = new Vector3(2.1f, 0.35f, 1.6f) }, accent, new Vector3(0f, 1.46f, 0f), root);
        AddMesh("IdentificationTile", new BoxMesh { Size = new Vector3(0.72f, 0.14f, 0.52f) },
            LegoMaterialLibrary.MoldedPolymer(M7DraftPalette.TeamIdentification), new Vector3(1.35f, 1.71f, 0.55f), root);
        AddWorldLabel(label, new Vector3(0f, 2.35f, 0f), 22, root);
        _factionSwatchCount++;
        _teamIdentificationTileCount++;
    }

    private void AddFamilySample(string label, LegoMaterialFamily family, float x)
    {
        PrimitiveMesh mesh = family switch
        {
            LegoMaterialFamily.ToolMetal => new CylinderMesh { TopRadius = 0.48f, BottomRadius = 0.72f, Height = 2.35f, RadialSegments = 12 },
            LegoMaterialFamily.Rubber => new CylinderMesh { TopRadius = 1.0f, BottomRadius = 1.0f, Height = 0.72f, RadialSegments = 18 },
            LegoMaterialFamily.TransparentPolymer => new SphereMesh { Radius = 1.02f, Height = 2.04f, RadialSegments = 24, Rings = 12 },
            LegoMaterialFamily.Crystal => new CylinderMesh { TopRadius = 0.12f, BottomRadius = 0.85f, Height = 2.5f, RadialSegments = 6 },
            LegoMaterialFamily.Terrain => new BoxMesh { Size = new Vector3(2.5f, 0.75f, 2.5f) },
            _ => new SphereMesh { Radius = 1.02f, Height = 2.04f, RadialSegments = 24, Rings = 12 }
        };
        Vector3 rotation = family == LegoMaterialFamily.Rubber ? new Vector3(90f, 0f, 0f) : Vector3.Zero;
        MeshInstance3D sample = AddMesh($"Family_{family}", mesh, _acceptanceMaterials[family], new Vector3(x, 1.05f, 1.9f));
        sample.RotationDegrees = rotation;
        AddWorldLabel(label, new Vector3(x, 2.95f, 1.9f), 19);
    }

    private void AddRoughnessSample(string label, float x, float roughness)
    {
        StandardMaterial3D material = LegoMaterialLibrary.MoldedPolymer(M7DraftPalette.RockRaiderDarkTurquoise, roughness);
        AddMesh($"PolymerRoughness_{label}", new SphereMesh { Radius = 0.82f, Height = 1.64f, RadialSegments = 24, Rings = 12 }, material, new Vector3(x, 1.0f, 7.2f));
        AddWorldLabel(label, new Vector3(x, 2.2f, 7.2f), 22);
    }

    private void BuildInterface()
    {
        CanvasLayer canvas = new() { Name = "MaterialLabHUD", Layer = 20 };
        AddChild(canvas);
        Control root = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        canvas.AddChild(root);

        PanelContainer panel = new()
        {
            AnchorLeft = 0.08f,
            AnchorRight = 0.92f,
            AnchorTop = 0f,
            AnchorBottom = 0f,
            OffsetTop = 12f,
            OffsetBottom = 88f,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle());
        HBoxContainer row = new() { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 20);
        panel.AddChild(row);
        VBoxContainer copy = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        Label title = new() { Text = "M7 · T064 MATERIAL LAB — ART-DIRECTION DRAFT" };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color("f3c55b"));
        copy.AddChild(title);
        Label subtitle = new() { Text = "Canonical material families · draft named-color matching · body palettes remain separate from team Identification Tiles" };
        subtitle.AddThemeFontSizeOverride("font_size", 14);
        subtitle.AddThemeColorOverride("font_color", new Color("c5ccd2"));
        copy.AddChild(subtitle);
        row.AddChild(copy);
        Button back = new() { Text = "RETURN TO PROTOTYPE", CustomMinimumSize = new Vector2(205f, 44f) };
        back.Pressed += () => _returnToPrototype?.Invoke();
        row.AddChild(back);
        root.AddChild(panel);

        Label footer = new()
        {
            Text = "Review: highlight width · black lift · white retention · transparent tint · crystal emission. Exact production palette is not accepted by this engineering smoke.",
            HorizontalAlignment = HorizontalAlignment.Center,
            AnchorLeft = 0.08f,
            AnchorRight = 0.92f,
            AnchorTop = 1f,
            AnchorBottom = 1f,
            OffsetTop = -52f,
            OffsetBottom = -16f
        };
        footer.AddThemeFontSizeOverride("font_size", 14);
        footer.AddThemeColorOverride("font_color", new Color("d4d7d9"));
        root.AddChild(footer);
    }

    private bool ValidateMaterialMasters()
    {
        if (_acceptanceMaterials.Count != Enum.GetValues<LegoMaterialFamily>().Length) return false;
        foreach ((LegoMaterialFamily family, StandardMaterial3D material) in _acceptanceMaterials)
            if (!LegoMaterialLibrary.IsCanonicalDraftProfile(family, material)) return false;
        return _factionSwatchCount == 5 && _teamIdentificationTileCount == _factionSwatchCount &&
            GetNodeOrNull<Camera3D>("MaterialLabCamera") is { Fov: 36f } &&
            GetNodeOrNull<WorldEnvironment>("WorldEnvironment")?.Environment?.GlowEnabled == true;
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null)
        {
            GD.PrintErr("M7 T064 MATERIAL LAB CAPTURE: FAIL viewport image unavailable");
            return false;
        }
        Error result = image.SavePng(path);
        if (result != Error.Ok)
        {
            GD.PrintErr($"M7 T064 MATERIAL LAB CAPTURE: FAIL error={result} path={path}");
            return false;
        }
        GD.Print($"M7 T064 MATERIAL LAB CAPTURE: PASS path={path}");
        return true;
    }

    private MeshInstance3D AddMesh(string name, PrimitiveMesh mesh, Material material, Vector3 position, Node3D? parent = null)
    {
        mesh.Material = material;
        MeshInstance3D instance = new() { Name = name, Mesh = mesh, Position = position };
        (parent ?? this).AddChild(instance);
        return instance;
    }

    private void AddWorldLabel(string text, Vector3 position, int fontSize, Node3D? parent = null)
    {
        Label3D label = new()
        {
            Text = text,
            Position = position,
            FontSize = fontSize,
            PixelSize = 0.012f,
            OutlineSize = 5,
            Modulate = new Color("edf0ed"),
            OutlineModulate = new Color(0.02f, 0.025f, 0.03f, 0.96f),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true
        };
        (parent ?? this).AddChild(label);
    }

    private static StyleBoxFlat PanelStyle() => new()
    {
        BgColor = new Color(0.045f, 0.06f, 0.075f, 0.97f),
        BorderColor = new Color("d4a82f"),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 7,
        CornerRadiusTopRight = 7,
        CornerRadiusBottomLeft = 7,
        CornerRadiusBottomRight = 7,
        ContentMarginLeft = 14,
        ContentMarginRight = 14,
        ContentMarginTop = 8,
        ContentMarginBottom = 8
    };
}
