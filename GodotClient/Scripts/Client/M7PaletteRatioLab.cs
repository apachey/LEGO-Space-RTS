using Godot;

namespace LegoSpaceRTS.Client;

public enum M7PalettePage : byte
{
    Factions = 0,
    MartianSources = 1,
    FactionModels = 2,
    MartianModels = 3,
    Transparency = 4,
    LightLanguage = 5
}

public partial class M7PaletteRatioLab : Node3D
{
    private sealed record PaletteSegment(string Name, Color Color, int Ratio, bool Transparent = false, Color? EmissionColor = null);
    private sealed record PaletteGroup(string Name, params PaletteSegment[] Segments);
    private sealed record LightSample(string Name, Color LensColor, Color EmissionColor, bool Transparent = true);
    private sealed record LightGroup(string Name, params LightSample[] Samples);

    private static readonly PaletteGroup[] FactionGroups =
    {
        new("ROCK RAIDERS", new("Dark turquoise", new Color("087f78"), 28), new("Dark industrial gray", new Color("3e4549"), 22), new("Earth brown", new Color("6b3f27"), 18), new("Black", new Color("111416"), 12), new("Light gray / metal", new Color("aab0b2"), 9), new("Hazard yellow", new Color("e6ae22"), 5), new("Luminous neon orange", new Color(1f, 0.34f, 0.02f, 0.72f), 3, true, new Color("ff650f")), new("Luminous neon lime", new Color(0.44f, 1f, 0.18f, 0.72f), 3, true, new Color("79ff38"))),
        new("LIFE ON MARS · ASTRONAUTS", new("Blue + medium blue", new Color("4c78a8"), 29), new("White", new Color("e8e6dc"), 22), new("Gray", new Color("858a8e"), 20), new("Black", new Color("171a1c"), 13), new("Tan / earth orange", new Color("b59368"), 7), new("Luminous red signals", new Color("d93d32"), 4, false, new Color("ff493c")), new("Clear warm lamps", new Color(0.92f, 0.96f, 1f, 0.24f), 2, true, new Color("ffd28a")), new("Trans-smoke", new Color(0.31f, 0.18f, 0.12f, 0.62f), 3, true)),
        new("MARS MISSION · ASTRONAUTS", new("White", new Color("edece5"), 43), new("Orange", new Color("e96f18"), 20), new("Black", new Color("151719"), 13), new("Light blue-gray", new Color("aeb4b7"), 13), new("Dark blue-gray", new Color("4c555c"), 6), new("Luminous blue signals", new Color(0.12f, 0.52f, 0.93f, 0.66f), 5, true, new Color("328fff"))),
        new("MARS MISSION · ALIENS", new("Black", new Color("111315"), 43), new("Lime shells", new Color("78b82a"), 21), new("Luminous neon lime", new Color(0.48f, 1f, 0.12f, 0.72f), 14, true, new Color("7cff2e")), new("Dark red / blue", new Color("583649"), 9), new("Pearl / dark gray", new Color("6f777b"), 9), new("Signals", new Color("d65035"), 4)),
        new("LIFE ON MARS · MARTIAN SPECTRUM", new("Black", new Color("151718"), 18), new("Grays", new Color("777c7d"), 18), new("Tan", new Color("b49a70"), 14), new("Sand families", new Color("8f777f"), 22), new("Dark / bright primaries", new Color("456d65"), 17), new("Earth orange", new Color("a45f2b"), 5), new("Luminous red", new Color("d93d32"), 1, false, new Color("ff493c")), new("Luminous neon orange", new Color(1f, 0.39f, 0.02f, 0.72f), 2, true, new Color("ff650f")), new("Luminous neon lime", new Color(0.43f, 1f, 0.13f, 0.70f), 2, true, new Color("79ff38")), new("Luminous blue", new Color(0.12f, 0.52f, 0.93f, 0.66f), 1, true, new Color("328fff")))
    };

    private static readonly PaletteGroup[] MartianGroups =
    {
        new("7311 · RED PLANET CRUISER", new("Sand green", new Color("76937a"), 34), new("Dark green", new Color("285347"), 23), new("Tan", new Color("b49a70"), 15), new("Black", new Color("151718"), 12), new("Gray", new Color("777c7d"), 9), new("Luminous neon orange", new Color(1f, 0.39f, 0.02f, 0.72f), 7, true, new Color("ff650f"))),
        new("7313 · RED PLANET PROTECTOR", new("Bright blue body", new Color("3272b8"), 30), new("Sand blue", new Color("74899b"), 25), new("Grays", new Color("85898b"), 17), new("Black", new Color("151718"), 14), new("Luminous neon orange", new Color(1f, 0.39f, 0.02f, 0.72f), 8, true, new Color("ff650f")), new("White / tan", new Color("d2c9ac"), 6)),
        new("7314 · RECON MECH RP", new("Bright red body", new Color("c83d32"), 33), new("Sand red", new Color("a66b68"), 25), new("Black", new Color("151718"), 15), new("Grays", new Color("85898b"), 14), new("Luminous neon lime", new Color(0.34f, 0.86f, 0.28f, 0.68f), 8, true, new Color("79ff38")), new("Tan", new Color("b49a70"), 5)),
        new("7316 · EXCAVATION SEARCHER", new("Tan / beige", new Color("b49a70"), 42), new("Black", new Color("151718"), 17), new("Grays", new Color("777c7d"), 15), new("Earth orange", new Color("a65f2b"), 10), new("Sand red / purple", new Color("927181"), 8), new("Luminous neon lime", new Color(0.43f, 1f, 0.13f, 0.70f), 8, true, new Color("79ff38"))),
        new("7317 · AERO TUBE HANGAR", new("Tan", new Color("b49a70"), 22), new("Light gray", new Color("aeb1b1"), 20), new("Black", new Color("151718"), 16), new("Sand purple", new Color("907393"), 13), new("Sand red", new Color("a66b68"), 10), new("Trans-smoke brown", new Color(0.25f, 0.15f, 0.10f, 0.58f), 10, true), new("Luminous neon lime", new Color(0.46f, 0.92f, 0.24f, 0.66f), 5, true, new Color("79ff38")), new("Luminous red", new Color(0.92f, 0.16f, 0.10f, 0.68f), 4, true, new Color("ff493c")))
    };

    private static readonly LightGroup[] FactionLightGroups =
    {
        new("ROCK RAIDERS", new("NEON ORANGE", new Color(1f, 0.34f, 0.02f, 0.70f), new Color("ff650f")), new("NEON LIME", new Color(0.44f, 1f, 0.18f, 0.70f), new Color("79ff38"))),
        new("MARS MISSION · ASTRONAUTS", new LightSample("BLUE", new Color(0.12f, 0.52f, 0.93f, 0.66f), new Color("328fff"))),
        new("MARS MISSION · ALIENS", new LightSample("NEON LIME", new Color(0.48f, 1f, 0.12f, 0.70f), new Color("7cff2e"))),
        new("LIFE ON MARS · ASTRONAUTS", new("RED", new Color("d93d32"), new Color("ff493c"), false), new("CLEAR / WARM", new Color(0.92f, 0.96f, 1f, 0.24f), new Color("ffd28a"))),
        new("LIFE ON MARS · MARTIANS", new("RED", new Color("d93d32"), new Color("ff493c"), false), new("NEON ORANGE", new Color(1f, 0.39f, 0.02f, 0.70f), new Color("ff650f")), new("NEON LIME", new Color(0.43f, 1f, 0.13f, 0.70f), new Color("79ff38")), new("BLUE", new Color(0.12f, 0.52f, 0.93f, 0.66f), new Color("328fff")))
    };

    private readonly List<StandardMaterial3D> _transparentNonEmissive = new();
    private Action? _returnToPrototype;
    private Camera3D? _camera;
    private Node3D? _pageRoot;
    private M7PalettePage _page;
    private bool _smoke;
    private bool _finished;
    private int _frames;
    private int _abstractPanelCount;
    private int _luminousSampleCount;
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
        GD.Print($"M7 PALETTE LAB: active={Slug(_page)} controls=1..6/Escape");
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
            GD.Print($"M7 PALETTE LAB: PASS factionGroups={FactionGroups.Length} martianGroups={MartianGroups.Length} nonEmissiveTransparent=5 luminousFunctions=10 abstractPanels={_abstractPanelCount} canvasLayers=0 active={Slug(_page)}");
        else
            GD.PrintErr("M7 PALETTE LAB: FAIL");
        AutomatedSmokeExit.Finish(this, valid ? 0 : 2);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        M7PalettePage? next = key.Keycode switch
        {
            Key.Key1 => M7PalettePage.Factions,
            Key.Key2 => M7PalettePage.MartianSources,
            Key.Key3 => M7PalettePage.FactionModels,
            Key.Key4 => M7PalettePage.MartianModels,
            Key.Key5 => M7PalettePage.Transparency,
            Key.Key6 => M7PalettePage.LightLanguage,
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
        _abstractPanelCount = 0;
        _luminousSampleCount = 0;
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
                BuildRatioPage("FACTION COLOR MASSING · CANDIDATE VISIBLE-AREA RATIOS", "WIDTH = VISIBLE SURFACE SHARE · NO PLAYER COLOR · 1–6 SWITCH PAGES", FactionGroups);
                break;
            case M7PalettePage.MartianSources:
                BuildRatioPage("MARTIAN COLOR FAMILIES · SOURCE SETS STAY DISTINCT", "7316 IS WEIGHTED BY VISIBLE PART AREA — NOT INVENTORY COUNT", MartianGroups);
                break;
            case M7PalettePage.FactionModels:
                BuildAbstractModelPage("FACTION ABSTRACT MODELS · 100-PANEL SURFACE MASSING", FactionGroups);
                break;
            case M7PalettePage.MartianModels:
                BuildAbstractModelPage("MARTIAN ABSTRACT MODELS · SOURCE FAMILIES", MartianGroups);
                break;
            case M7PalettePage.Transparency:
                BuildTransparencyPage();
                break;
            case M7PalettePage.LightLanguage:
                BuildLightLanguagePage();
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

    private void BuildAbstractModelPage(string title, IReadOnlyList<PaletteGroup> groups)
    {
        if (_pageRoot is null) return;
        AddCenteredLabel(_pageRoot, title, new Vector3(0f, 7.55f, 0.8f), 32, new Color("f2f3f4"));
        AddCenteredLabel(_pageRoot, "IDENTICAL SILHOUETTE · ONE FRONT PANEL = 1% VISIBLE AREA · COLOR MASSES GROW FROM CORE TO EDGE", new Vector3(0f, 6.82f, 0.8f), 18, new Color("aeb6bd"));

        float spacing = 5.45f;
        float firstX = -spacing * (groups.Count - 1) * 0.5f;
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            PaletteGroup group = groups[groupIndex];
            Node3D carrier = new() { Name = $"AbstractCarrier_{group.Name}", Position = new Vector3(firstX + groupIndex * spacing, 1.0f, 0f) };
            _pageRoot.AddChild(carrier);
            BuildHundredPanelCarrier(carrier, group);
            AddCenteredLabel(_pageRoot, group.Name, carrier.Position + new Vector3(0f, -2.25f, 0.7f), 18, new Color("e3e6e8"));
            AddCenteredLabel(_pageRoot, string.Join(" / ", group.Segments.Select(segment => segment.Ratio)), carrier.Position + new Vector3(0f, -2.78f, 0.7f), 15, new Color("aeb6bd"));
        }

        AddCenteredLabel(_pageRoot, "The sequence under each model matches the segment order on ratio pages 1 and 2. Emission is applied only to named luminous roles.", new Vector3(0f, -6.6f, 0.8f), 17, new Color("9fa8b2"));
    }

    private void BuildHundredPanelCarrier(Node3D parent, PaletteGroup group)
    {
        List<Vector3> slots = CreateCarrierSlots();
        if (slots.Count != 100) return;

        MeshInstance3D backing = new()
        {
            Name = "CarrierBacking",
            Mesh = new BoxMesh { Size = new Vector3(4.15f, 3.45f, 0.15f) },
            Position = new Vector3(0f, 0f, -0.11f),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color("252a2e"), Roughness = 0.84f, Metallic = 0.12f }
        };
        parent.AddChild(backing);

        BoxMesh panelMesh = new() { Size = new Vector3(0.292f, 0.292f, 0.17f) };
        int slotIndex = 0;
        foreach (PaletteSegment segment in group.Segments)
        {
            StandardMaterial3D material = RatioMaterial(segment);
            for (int i = 0; i < segment.Ratio; i++)
            {
                Vector3 slot = slots[slotIndex++];
                float radial = Mathf.Min(1f, new Vector2(slot.X / 2.0f, slot.Y / 1.65f).Length());
                float depthScale = 1.15f + (1f - radial) * 1.65f;
                MeshInstance3D panel = new()
                {
                    Name = $"Panel_{slotIndex:000}_{segment.Name}",
                    Mesh = panelMesh,
                    Position = slot + new Vector3(0f, 0f, 0.085f * depthScale),
                    Scale = new Vector3(1f, 1f, depthScale),
                    MaterialOverride = material
                };
                parent.AddChild(panel);
                _abstractPanelCount++;
            }
        }
    }

    private static List<Vector3> CreateCarrierSlots()
    {
        int[] rowCounts = { 6, 8, 10, 12, 12, 12, 12, 10, 10, 8 };
        List<Vector3> slots = new(100);
        const float spacing = 0.33f;
        for (int row = 0; row < rowCounts.Length; row++)
        {
            int columns = rowCounts[row];
            float y = (row - (rowCounts.Length - 1) * 0.5f) * spacing;
            for (int column = 0; column < columns; column++)
            {
                float x = (column - (columns - 1) * 0.5f) * spacing;
                slots.Add(new Vector3(x, y, 0f));
            }
        }

        return slots
            .OrderBy(slot => slot.X * slot.X + slot.Y * slot.Y)
            .ThenBy(slot => Mathf.Abs(slot.X))
            .ThenByDescending(slot => slot.Y)
            .ThenBy(slot => slot.X)
            .ToList();
    }

    private void BuildTransparencyPage()
    {
        if (_pageRoot is null || _camera is null) return;
        _camera.Projection = Camera3D.ProjectionType.Orthogonal;
        _camera.Size = 18f;
        _camera.Position = new Vector3(0f, 0f, 28f);
        _camera.LookAt(Vector3.Zero, Vector3.Up);

        AddCenteredLabel(_pageRoot, "TRANSPARENCY IS AN OPTICAL PROPERTY — NOT AN EMISSION RULE", new Vector3(0f, 7.35f, 1.2f), 30, new Color("f1f2f3"));
        AddCenteredLabel(_pageRoot, "Canopies and tubes are scene-lit. A matching hue glows only when the faction map assigns it a light role.", new Vector3(0f, 6.65f, 1.2f), 18, new Color("aeb6bd"));

        (string Name, Color Color)[] samples =
        {
            ("SMOKE BROWN", new Color(0.24f, 0.14f, 0.09f, 0.52f)),
            ("CLEAR", new Color(0.90f, 0.94f, 1f, 0.24f)),
            ("MM ORANGE GLASS", new Color(1f, 0.31f, 0.015f, 0.57f)),
            ("BLUE CANOPY", new Color(0.12f, 0.52f, 0.93f, 0.50f)),
            ("GREEN TUBE", new Color(0.26f, 0.92f, 0.16f, 0.52f))
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

        AddCenteredLabel(_pageRoot, "GENERIC FUNCTION EXAMPLES", new Vector3(0f, -2.2f, 0.8f), 22, new Color("f1f2f3"));
        AddEmissiveExample(_pageRoot, "WORK LAMP", new Vector3(-4.0f, -4.0f, 0f), new Color("ffd37a"), 2.2f);
        AddEmissiveExample(_pageRoot, "ENERGY CRYSTAL", new Vector3(3.3f, -4.0f, 0f), new Color("72ff48"), 2.6f);
        AddCenteredLabel(_pageRoot, "Local light + glow are enabled because the object is a lamp or energy carrier — not because the hue is saturated.", new Vector3(0f, -7.0f, 0.8f), 16, new Color("aeb6bd"));
    }

    private void BuildLightLanguagePage()
    {
        if (_pageRoot is null) return;
        AddCenteredLabel(_pageRoot, "FACTION LIGHT LANGUAGE · AUTHORED EMISSIVE ROLES", new Vector3(0f, 7.45f, 0.9f), 31, new Color("f2f3f4"));
        AddCenteredLabel(_pageRoot, "ONLY THESE ASSIGNED SIGNAL / LAMP COLORS EMIT · SAME-HUE BODY PLASTIC REMAINS NON-EMISSIVE", new Vector3(0f, 6.72f, 0.9f), 18, new Color("aeb6bd"));

        for (int row = 0; row < FactionLightGroups.Length; row++)
        {
            LightGroup group = FactionLightGroups[row];
            float y = 4.75f - row * 2.25f;
            AddLabel(_pageRoot, group.Name, new Vector3(-11.9f, y, 0.75f), 20, new Color("e1e4e6"));

            float sampleSpacing = 3.25f;
            float firstX = -0.3f - sampleSpacing * (group.Samples.Length - 1) * 0.5f;
            for (int sampleIndex = 0; sampleIndex < group.Samples.Length; sampleIndex++)
                AddFactionLightSample(_pageRoot, group.Samples[sampleIndex], new Vector3(firstX + sampleIndex * sampleSpacing, y, 0f));
        }

        AddCenteredLabel(_pageRoot, "Clear / warm = a colorless transparent lens carrying warm light. Mars Mission orange glass stays non-emissive on page 5.", new Vector3(0f, -6.75f, 0.85f), 17, new Color("9fa8b2"));
    }

    private void AddFactionLightSample(Node parent, LightSample sample, Vector3 position)
    {
        MeshInstance3D plaque = new()
        {
            Name = $"LightPlaque_{sample.Name}",
            Mesh = new BoxMesh { Size = new Vector3(2.75f, 1.32f, 0.18f) },
            Position = position + new Vector3(0f, 0f, -0.22f),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color("24282c"), Roughness = 0.88f }
        };
        parent.AddChild(plaque);

        StandardMaterial3D material = new()
        {
            AlbedoColor = sample.LensColor,
            Roughness = sample.Transparent ? 0.14f : 0.34f,
            Transparency = sample.Transparent ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled,
            CullMode = sample.Transparent ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Back,
            EmissionEnabled = true,
            Emission = sample.EmissionColor,
            EmissionEnergyMultiplier = 2.6f
        };
        MeshInstance3D lens = new()
        {
            Name = $"FactionLight_{sample.Name}",
            Mesh = new SphereMesh { Radius = 0.43f, Height = 0.86f, RadialSegments = 24, Rings = 12 },
            Position = position + new Vector3(-0.88f, 0f, 0.12f),
            MaterialOverride = material
        };
        parent.AddChild(lens);
        parent.AddChild(new OmniLight3D
        {
            Position = position + new Vector3(-0.88f, 0f, 0.65f),
            LightColor = sample.EmissionColor,
            LightEnergy = 1.05f,
            OmniRange = 2.25f,
            ShadowEnabled = false
        });
        AddCenteredLabel(parent, sample.Name, position + new Vector3(0.42f, 0f, 0.45f), 15, new Color("eef0f1"));
        _luminousSampleCount++;
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
        }
        material.EmissionEnabled = segment.EmissionColor.HasValue;
        if (segment.EmissionColor.HasValue)
        {
            material.Emission = segment.EmissionColor.Value;
            material.EmissionEnergyMultiplier = 2.15f;
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
        PaletteGroup rockRaiders = FactionGroups.Single(group => group.Name == "ROCK RAIDERS");
        bool rockRaidersBrown = rockRaiders.Segments.Any(segment => segment.Name.Contains("brown", StringComparison.OrdinalIgnoreCase) && segment.Ratio >= 15);
        PaletteGroup excavationSearcher = MartianGroups.Single(group => group.Name.StartsWith("7316", StringComparison.Ordinal));
        PaletteSegment excavationTan = excavationSearcher.Segments.Single(segment => segment.Name.Contains("Tan", StringComparison.OrdinalIgnoreCase));
        bool excavationTanDominates = excavationTan.Ratio == excavationSearcher.Segments.Max(segment => segment.Ratio) && excavationTan.Ratio >= 40;
        bool lightLanguage = FactionLightGroups.Length == 5 && FactionLightGroups.Sum(group => group.Samples.Length) == 10;
        bool transparentSemantics = true;
        foreach (Color color in new[] { new Color(0.24f, 0.14f, 0.09f, 0.52f), new Color(0.90f, 0.94f, 1f, 0.24f), new Color(1f, 0.31f, 0.015f, 0.57f), new Color(0.12f, 0.52f, 0.93f, 0.50f), new Color(0.26f, 0.92f, 0.16f, 0.52f) })
        {
            StandardMaterial3D material = NonEmissiveTransparent(color);
            transparentSemantics &= !material.EmissionEnabled && material.Transparency == BaseMaterial3D.TransparencyEnum.Alpha;
        }
        if (_page == M7PalettePage.Transparency)
            transparentSemantics &= _transparentNonEmissive.Count == 5 && _transparentNonEmissive.All(material => !material.EmissionEnabled);

        bool pageSpecific = _page switch
        {
            M7PalettePage.FactionModels => _abstractPanelCount == FactionGroups.Length * 100,
            M7PalettePage.MartianModels => _abstractPanelCount == MartianGroups.Length * 100,
            M7PalettePage.LightLanguage => _luminousSampleCount == 10,
            _ => _abstractPanelCount == 0 && _luminousSampleCount == 0
        };

        return ratios && noUniversalTeamColor && rockRaidersBrown && excavationTanDominates && lightLanguage && transparentSemantics && pageSpecific && _pageRoot is not null && _camera is not null &&
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
                "faction-models" or "faction-abstract" => M7PalettePage.FactionModels,
                "martian-models" or "martian-abstract" => M7PalettePage.MartianModels,
                "transparency" or "transparent" => M7PalettePage.Transparency,
                "lights" or "light-language" => M7PalettePage.LightLanguage,
                _ => M7PalettePage.Factions
            };
        }
        return M7PalettePage.Factions;
    }

    private static string Slug(M7PalettePage page) => page switch
    {
        M7PalettePage.Factions => "factions",
        M7PalettePage.MartianSources => "martian-sources",
        M7PalettePage.FactionModels => "faction-models",
        M7PalettePage.MartianModels => "martian-models",
        M7PalettePage.Transparency => "transparency",
        M7PalettePage.LightLanguage => "light-language",
        _ => "factions"
    };
}
