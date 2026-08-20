using Godot;

namespace LegoSpaceRTS.Client;

public enum M7PalettePage : byte
{
    Factions = 0,
    MartianSources = 1,
    Transparency = 2
}

public partial class M7PaletteRatioLab : Node3D
{
    private sealed record PaletteSegment(string Name, Color Color, int Ratio, bool Transparent = false);
    private sealed record PaletteGroup(string Name, params PaletteSegment[] Segments);

    private static readonly PaletteGroup[] FactionGroups =
    {
        new("ROCK RAIDERS", new("Dark turquoise", new Color("087f78"), 34), new("Dark industrial gray", new Color("3e4549"), 27), new("Black", new Color("111416"), 19), new("Light gray / metal", new Color("aab0b2"), 11), new("Hazard yellow", new Color("e6ae22"), 6), new("Trans-neon green", new Color(0.44f, 1f, 0.18f, 0.72f), 3, true)),
        new("ASTRONAUT · FIELD", new("Blue + medium blue", new Color("4c78a8"), 30), new("White", new Color("e8e6dc"), 22), new("Gray", new Color("858a8e"), 21), new("Black", new Color("171a1c"), 13), new("Tan / earth orange", new Color("b59368"), 7), new("Red + yellow", new Color("c94a36"), 4), new("Trans-smoke / red", new Color(0.31f, 0.18f, 0.12f, 0.62f), 3, true)),
        new("ASTRONAUT · MISSION", new("White", new Color("edece5"), 43), new("Orange", new Color("e96f18"), 20), new("Black", new Color("151719"), 13), new("Light blue-gray", new Color("aeb4b7"), 13), new("Dark blue-gray", new Color("4c555c"), 6), new("Transparent signals", new Color(0.95f, 0.33f, 0.04f, 0.68f), 5, true)),
        new("ALIENS", new("Black", new Color("111315"), 43), new("Lime shells", new Color("78b82a"), 21), new("Trans-neon green", new Color(0.48f, 1f, 0.12f, 0.72f), 14, true), new("Dark red / blue", new Color("583649"), 9), new("Pearl / dark gray", new Color("6f777b"), 9), new("Signals", new Color("d65035"), 4)),
        new("MARTIANS · SOURCE SPECTRUM", new("Black", new Color("151718"), 18), new("Grays", new Color("777c7d"), 18), new("Tan", new Color("b49a70"), 14), new("Sand families", new Color("8f777f"), 22), new("Dark / bright primaries", new Color("456d65"), 17), new("Earth orange", new Color("a45f2b"), 5), new("Transparent accents", new Color(0.77f, 0.42f, 0.16f, 0.70f), 6, true))
    };

    private static readonly PaletteGroup[] MartianGroups =
    {
        new("7311 · RED PLANET CRUISER", new("Sand green", new Color("76937a"), 34), new("Dark green", new Color("285347"), 23), new("Tan", new Color("b49a70"), 15), new("Black", new Color("151718"), 12), new("Gray", new Color("777c7d"), 9), new("Trans-neon orange", new Color(1f, 0.39f, 0.02f, 0.72f), 7, true)),
        new("7313 · RED PLANET PROTECTOR", new("Bright blue", new Color("3272b8"), 30), new("Sand blue", new Color("74899b"), 25), new("Grays", new Color("85898b"), 17), new("Black", new Color("151718"), 14), new("Trans-neon orange", new Color(1f, 0.39f, 0.02f, 0.72f), 8, true), new("White / tan", new Color("d2c9ac"), 6)),
        new("7314 · RECON MECH RP", new("Bright red", new Color("c83d32"), 33), new("Sand red", new Color("a66b68"), 25), new("Black", new Color("151718"), 15), new("Grays", new Color("85898b"), 14), new("Trans green", new Color(0.34f, 0.86f, 0.28f, 0.68f), 8, true), new("Tan", new Color("b49a70"), 5)),
        new("7316 · EXCAVATION SEARCHER", new("Black", new Color("151718"), 27), new("Tan", new Color("b49a70"), 20), new("Grays", new Color("777c7d"), 20), new("Earth orange", new Color("a65f2b"), 13), new("Sand red / purple", new Color("927181"), 12), new("Trans-neon green", new Color(0.43f, 1f, 0.13f, 0.70f), 8, true)),
        new("7317 · AERO TUBE HANGAR", new("Tan", new Color("b49a70"), 22), new("Light gray", new Color("aeb1b1"), 20), new("Black", new Color("151718"), 16), new("Sand purple", new Color("907393"), 13), new("Sand red", new Color("a66b68"), 10), new("Trans-smoke brown", new Color(0.25f, 0.15f, 0.10f, 0.58f), 10, true), new("Trans green / red", new Color(0.46f, 0.92f, 0.24f, 0.66f), 9, true))
    };

    private readonly List<StandardMaterial3D> _transparentNonEmissive = new();
    private Action? _returnToPrototype;
    private Camera3D? _camera;
    private Node3D? _pageRoot;
    private M7PalettePage _page;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private string? _capturePath;

    public void Configure(Action returnToPrototype, string[] arguments)
    {
        Name = "M7PaletteRatioLab";
        _returnToPrototype = returnToPrototype;
        _smoke = arguments.Contains("--m7-palette-smoke");
        _page = ParsePage(arguments);
        for (int i = 0; i + 1 < arguments.Length; i++)
            if (arguments[i] == "--capture-path") _capturePath = arguments[i + 1];

        BuildInvariantScene();
        ShowPage(_page);
        ProcessPriority = 1000;
        GD.Print($"M7 PALETTE LAB: active={Slug(_page)} controls=1..3/Escape");
    }

    public override void _Process(double delta)
    {
        if (!_smoke || _finished) return;
        _frames++;
        if (_frames < 20) return;

        bool valid = ValidateLab();
        if (valid && _capturePath is not null) valid = CaptureViewport(_capturePath);
        _finished = true;
        if (valid)
            GD.Print($"M7 PALETTE LAB: PASS factionGroups={FactionGroups.Length} martianGroups={MartianGroups.Length} nonEmissiveTransparent=5 emissiveByFunction=2 canvasLayers=0 active={Slug(_page)}");
        else
            GD.PrintErr("M7 PALETTE LAB: FAIL");
        GetTree().Quit(valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        M7PalettePage? next = key.Keycode switch
        {
            Key.Key1 => M7PalettePage.Factions,
            Key.Key2 => M7PalettePage.MartianSources,
            Key.Key3 => M7PalettePage.Transparency,
            _ => null
        };
        if (next.HasValue)
        {
            ShowPage(next.Value);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Escape)
        {
            _returnToPrototype?.Invoke();
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildInvariantScene()
    {
        _camera = new Camera3D { Name = "PaletteCamera", Current = true, Projection = Camera3D.ProjectionType.Orthogonal, Size = 18f, Position = new Vector3(0f, 0f, 28f) };
        AddChild(_camera);
        _camera.LookAt(Vector3.Zero, Vector3.Up);

        DirectionalLight3D key = new() { Name = "PaletteKey", RotationDegrees = new Vector3(-24f, -28f, 0f), LightColor = new Color("fff2dc"), LightEnergy = 1.25f, ShadowEnabled = true };
        AddChild(key);
        DirectionalLight3D fill = new() { Name = "PaletteFill", RotationDegrees = new Vector3(22f, 150f, 0f), LightColor = new Color("a7c9e8"), LightEnergy = 0.65f, ShadowEnabled = false };
        AddChild(fill);
        Godot.Environment environment = new()
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color("15181c"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color("b7bcc2"),
            AmbientLightEnergy = 0.62f,
            GlowEnabled = true,
            GlowIntensity = 0.28f,
            GlowBloom = 0.06f
        };
        AddChild(new WorldEnvironment { Environment = environment });
    }

    private void ShowPage(M7PalettePage page)
    {
        _page = page;
        _transparentNonEmissive.Clear();
        if (_pageRoot is not null)
        {
            RemoveChild(_pageRoot);
            _pageRoot.QueueFree();
        }
        _pageRoot = new Node3D { Name = $"PalettePage_{Slug(page)}" };
        AddChild(_pageRoot);

        if (_camera is not null)
        {
            _camera.Projection = Camera3D.ProjectionType.Orthogonal;
            _camera.Size = 18f;
            _camera.Position = new Vector3(0f, 0f, 28f);
            _camera.LookAt(Vector3.Zero, Vector3.Up);
        }

        switch (page)
        {
            case M7PalettePage.Factions:
                BuildRatioPage("FACTION COLOR MASSING · CANDIDATE VISIBLE-AREA RATIOS", "WIDTH = SHARE · NO PLAYER COLOR · 1 FACTIONS  2 MARTIAN SOURCES  3 TRANSPARENCY", FactionGroups);
                break;
            case M7PalettePage.MartianSources:
                BuildRatioPage("MARTIAN COLOR FAMILIES · SOURCE SETS STAY DISTINCT", "NO SHARED BLUE OVERLAY · TRANSPARENT ACCENTS ARE NOT LIGHT SOURCES", MartianGroups);
                break;
            case M7PalettePage.Transparency:
                BuildTransparencyPage();
                break;
        }
        GD.Print($"M7 PALETTE LAB SWITCH: {Slug(page)}");
    }

    private void BuildRatioPage(string title, string subtitle, IReadOnlyList<PaletteGroup> groups)
    {
        if (_pageRoot is null) return;
        AddCenteredLabel(_pageRoot, title, new Vector3(0f, 7.65f, 0.45f), 34, new Color("f2f3f4"));
        AddCenteredLabel(_pageRoot, subtitle, new Vector3(0f, 6.92f, 0.45f), 19, new Color("9fa8b2"));
        const float barStart = -5.1f;
        const float barWidth = 19.2f;
        for (int row = 0; row < groups.Count; row++)
        {
            PaletteGroup group = groups[row];
            float y = 5.45f - row * 2.55f;
            AddCenteredLabel(_pageRoot, group.Name, new Vector3(-10.6f, y + 0.14f, 0.42f), 24, new Color("dfe3e6"));
            float x = barStart;
            foreach (PaletteSegment segment in group.Segments)
            {
                float width = barWidth * segment.Ratio / 100f;
                MeshInstance3D block = new()
                {
                    Name = $"Ratio_{group.Name}_{segment.Name}",
                    Mesh = new BoxMesh { Size = new Vector3(Mathf.Max(0.04f, width - 0.035f), 0.82f, 0.32f) },
                    Position = new Vector3(x + width * 0.5f, y + 0.18f, 0f),
                    MaterialOverride = RatioMaterial(segment)
                };
                _pageRoot.AddChild(block);
                if (segment.Ratio >= 9)
                    AddCenteredLabel(_pageRoot, $"{segment.Ratio}%", new Vector3(x + width * 0.5f, y + 0.18f, 0.35f), 19, Contrast(segment.Color));
                x += width;
            }
            string legend = string.Join("   ·   ", group.Segments.Select(segment => $"{segment.Name} {segment.Ratio}%"));
            AddCenteredLabel(_pageRoot, legend, new Vector3(barStart + barWidth * 0.5f, y - 0.66f, 0.42f), 16, new Color("b8bec4"));
        }
    }

    private void BuildTransparencyPage()
    {
        if (_pageRoot is null || _camera is null) return;
        _camera.Projection = Camera3D.ProjectionType.Orthogonal;
        _camera.Size = 18f;
        _camera.Position = new Vector3(0f, 0f, 28f);
        _camera.LookAt(Vector3.Zero, Vector3.Up);

        AddCenteredLabel(_pageRoot, "TRANSPARENCY IS AN OPTICAL PROPERTY — NOT AN EMISSION RULE", new Vector3(0f, 7.35f, 1.2f), 30, new Color("f1f2f3"));
        AddCenteredLabel(_pageRoot, "Smoke brown, clear, orange, blue and green are lit by the scene. None glows by color alone.", new Vector3(0f, 6.65f, 1.2f), 18, new Color("aeb6bd"));

        (string Name, Color Color)[] samples =
        {
            ("SMOKE BROWN", new Color(0.24f, 0.14f, 0.09f, 0.52f)),
            ("CLEAR", new Color(0.90f, 0.94f, 1f, 0.24f)),
            ("NEON ORANGE", new Color(1f, 0.31f, 0.015f, 0.57f)),
            ("TRANS BLUE", new Color(0.12f, 0.52f, 0.93f, 0.50f)),
            ("TRANS GREEN", new Color(0.26f, 0.92f, 0.16f, 0.52f))
        };

        for (int i = 0; i < samples.Length; i++)
        {
            float x = -7.6f + i * 3.75f;
            AddBacking(_pageRoot, new Vector3(x - 0.62f, 1.75f, -0.55f), Colors.White);
            AddBacking(_pageRoot, new Vector3(x + 0.62f, 1.75f, -0.55f), new Color("101214"));
            StandardMaterial3D material = NonEmissiveTransparent(samples[i].Color);
            _transparentNonEmissive.Add(material);
            MeshInstance3D sample = new()
            {
                Name = $"NonEmissive_{samples[i].Name}",
                Mesh = new CylinderMesh { TopRadius = 0.82f, BottomRadius = 0.82f, Height = 3.35f, RadialSegments = 32 },
                Position = new Vector3(x, 1.75f, 0f),
                MaterialOverride = material
            };
            _pageRoot.AddChild(sample);
            AddCenteredLabel(_pageRoot, $"{samples[i].Name}\nNON-EMISSIVE", new Vector3(x, -0.43f, 0.45f), 17, new Color("d7dbde"));
        }

        AddCenteredLabel(_pageRoot, "EMISSION REQUIRES FUNCTION", new Vector3(0f, -2.2f, 0.8f), 22, new Color("f1f2f3"));
        AddEmissiveExample(_pageRoot, "WORK LAMP", new Vector3(-4.0f, -4.0f, 0f), new Color("ffd37a"), 2.2f);
        AddEmissiveExample(_pageRoot, "ENERGY CRYSTAL", new Vector3(3.3f, -4.0f, 0f), new Color("72ff48"), 2.6f);
        AddCenteredLabel(_pageRoot, "Local light + glow are enabled because the object is a lamp or energy carrier — not because the hue is saturated.", new Vector3(0f, -6.15f, 0.8f), 17, new Color("aeb6bd"));
    }

    private static void AddBacking(Node parent, Vector3 position, Color color)
    {
        MeshInstance3D backing = new()
        {
            Mesh = new BoxMesh { Size = new Vector3(1.28f, 3.55f, 0.18f) },
            Position = position,
            MaterialOverride = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.92f }
        };
        parent.AddChild(backing);
    }

    private static void AddEmissiveExample(Node parent, string label, Vector3 position, Color color, float energy)
    {
        StandardMaterial3D material = new()
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = energy,
            Roughness = 0.18f
        };
        MeshInstance3D objectMesh = new()
        {
            Name = $"Emissive_{label}",
            Mesh = label == "WORK LAMP" ? new SphereMesh { Radius = 0.72f, Height = 1.44f, RadialSegments = 24, Rings = 12 } : new PrismMesh { Size = new Vector3(1.35f, 2.3f, 1.35f) },
            Position = position,
            MaterialOverride = material
        };
        parent.AddChild(objectMesh);
        parent.AddChild(new OmniLight3D { Position = position + Vector3.Back * 0.3f, LightColor = color, LightEnergy = energy * 0.72f, OmniRange = 4.2f, ShadowEnabled = false });
        AddCenteredLabel(parent, $"{label}\nEMISSIVE BY FUNCTION", position + new Vector3(0f, -1.7f, 0.6f), 17, new Color("f0f1f2"));
    }

    private StandardMaterial3D RatioMaterial(PaletteSegment segment)
    {
        StandardMaterial3D material = new()
        {
            AlbedoColor = segment.Color,
            Roughness = segment.Transparent ? 0.20f : 0.43f,
            Metallic = segment.Name.Contains("metal", StringComparison.OrdinalIgnoreCase) || segment.Name.Contains("Pearl", StringComparison.OrdinalIgnoreCase) ? 0.62f : 0f
        };
        if (segment.Transparent)
        {
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
            material.EmissionEnabled = false;
        }
        return material;
    }

    private static StandardMaterial3D NonEmissiveTransparent(Color color) => new()
    {
        AlbedoColor = color,
        Roughness = 0.16f,
        Metallic = 0f,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        EmissionEnabled = false
    };

    private static void AddLabel(Node parent, string text, Vector3 position, int fontSize, Color color)
    {
        Label3D label = new()
        {
            Text = text,
            Position = position,
            FontSize = fontSize,
            PixelSize = 0.0125f,
            Modulate = color,
            OutlineModulate = new Color(0f, 0f, 0f, 0.72f),
            OutlineSize = 5,
            NoDepthTest = true
        };
        parent.AddChild(label);
    }

    private static void AddCenteredLabel(Node parent, string text, Vector3 position, int fontSize, Color color)
    {
        Label3D label = new()
        {
            Text = text,
            Position = position,
            FontSize = fontSize,
            PixelSize = 0.0125f,
            Modulate = color,
            OutlineModulate = new Color(0f, 0f, 0f, 0.62f),
            OutlineSize = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            NoDepthTest = true
        };
        parent.AddChild(label);
    }

    private bool ValidateLab()
    {
        bool ratios = FactionGroups.Concat(MartianGroups).All(group => group.Segments.Sum(segment => segment.Ratio) == 100 && group.Segments.All(segment => segment.Ratio > 0));
        bool noUniversalTeamColor = FactionGroups.Concat(MartianGroups).SelectMany(group => group.Segments).All(segment => !segment.Name.Contains("team", StringComparison.OrdinalIgnoreCase));
        bool transparentSemantics = true;
        foreach (Color color in new[] { new Color(0.24f, 0.14f, 0.09f, 0.52f), new Color(0.90f, 0.94f, 1f, 0.24f), new Color(1f, 0.31f, 0.015f, 0.57f), new Color(0.12f, 0.52f, 0.93f, 0.50f), new Color(0.26f, 0.92f, 0.16f, 0.52f) })
        {
            StandardMaterial3D material = NonEmissiveTransparent(color);
            transparentSemantics &= !material.EmissionEnabled && material.Transparency == BaseMaterial3D.TransparencyEnum.Alpha;
        }
        if (_page == M7PalettePage.Transparency)
            transparentSemantics &= _transparentNonEmissive.Count == 5 && _transparentNonEmissive.All(material => !material.EmissionEnabled);

        return ratios && noUniversalTeamColor && transparentSemantics && _pageRoot is not null && _camera is not null &&
            FindChildren("*", nameof(CanvasLayer), true, false).Count == 0;
    }

    private bool CaptureViewport(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Image? image = GetViewport().GetTexture().GetImage();
        if (image is null || image.SavePng(path) != Error.Ok) return false;
        GD.Print($"M7 PALETTE LAB CAPTURE: PASS page={Slug(_page)} path={path}");
        return true;
    }

    private static Color Contrast(Color color)
    {
        float luminance = color.R * 0.2126f + color.G * 0.7152f + color.B * 0.0722f;
        return luminance > 0.48f ? new Color("151719") : new Color("f1f2f3");
    }

    private static M7PalettePage ParsePage(string[] arguments)
    {
        for (int i = 0; i + 1 < arguments.Length; i++)
        {
            if (arguments[i] != "--m7-palette-page") continue;
            return arguments[i + 1].Trim().ToLowerInvariant() switch
            {
                "martians" or "martian-sources" => M7PalettePage.MartianSources,
                "transparency" or "transparent" => M7PalettePage.Transparency,
                _ => M7PalettePage.Factions
            };
        }
        return M7PalettePage.Factions;
    }

    private static string Slug(M7PalettePage page) => page switch
    {
        M7PalettePage.Factions => "factions",
        M7PalettePage.MartianSources => "martian-sources",
        M7PalettePage.Transparency => "transparency",
        _ => "factions"
    };
}
