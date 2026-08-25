using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace LegoSpaceRTS.Presentation;

public sealed class M7LookProfile
{
    public const int CurrentSchemaVersion = 5;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public CameraLook Camera { get; set; } = new();
    public SceneLook Scene { get; set; } = new();
    public ShadingLook Shading { get; set; } = new();
    public MaterialCollection Materials { get; set; } = new();
    public GlassLook Glass { get; set; } = new();
    public EmissionLook Emission { get; set; } = new();
    public LightingLook Lighting { get; set; } = new();
    public WorldCycleLook WorldCycle { get; set; } = new();
    public PostLook Post { get; set; } = new();
    public OutlineLook Outline { get; set; } = new();
    public AnimationLook Animation { get; set; } = new();
    public DestructionLook Destruction { get; set; } = new();
    public VfxLook Vfx { get; set; } = new();
    public VfxPoolLook VfxPool { get; set; } = new();
    public GroundLook Ground { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public static M7LookProfile CreateDefault() => new();

    public string ToJson()
    {
        Normalize();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static bool TryFromJson(string json, out M7LookProfile profile, out string error)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            M7LookProfile? parsed = JsonSerializer.Deserialize<M7LookProfile>(json, JsonOptions);
            if (parsed is null)
            {
                profile = CreateDefault();
                error = "Clipboard does not contain an M7 Look profile.";
                return false;
            }
            if (parsed.SchemaVersion is not (1 or 2 or 3 or 4 or CurrentSchemaVersion))
            {
                profile = CreateDefault();
                error = $"Profile schema {parsed.SchemaVersion} is not supported; expected 1–{CurrentSchemaVersion}.";
                return false;
            }
            // Schema 1 was the first review fixture. Schema 2 removes the
            // deferred HUD/scorch controls and adds free-camera, texture and
            // emissive-response controls. Schema 3 adds animation and VFX-pool
            // controls. Schema 4 adds bounded LEGO destruction controls. Schema
            // 5 adds a practical texture stack, per-function emission response,
            // direct fire lighting and presentation-only world-light profiles. Missing
            // values intentionally inherit the current review baseline, so old
            // copied profiles remain usable.
            int sourceSchemaVersion = parsed.SchemaVersion;
            MergeMissingMaterialDefaults(parsed, document.RootElement);
            parsed.SchemaVersion = CurrentSchemaVersion;
            parsed.Normalize();
            if (sourceSchemaVersion == 1) ApplySchemaOneMigration(parsed);
            if (sourceSchemaVersion < 5) ApplySchemaFiveMigration(parsed);
            profile = parsed;
            error = string.Empty;
            return true;
        }
        catch (JsonException exception)
        {
            profile = CreateDefault();
            error = $"Invalid JSON: {exception.Message}";
            return false;
        }
    }

    public M7LookProfile Clone()
    {
        if (!TryFromJson(ToJson(), out M7LookProfile clone, out string error))
            throw new InvalidOperationException(error);
        return clone;
    }

    private static void ApplySchemaOneMigration(M7LookProfile profile)
    {
        MaterialCollection defaults = CreateDefault().Materials;
        CopyTextureDefaults(profile.Materials.PaintedHull, defaults.PaintedHull);
        CopyTextureDefaults(profile.Materials.StructuralEarth, defaults.StructuralEarth);
        CopyTextureDefaults(profile.Materials.Accent, defaults.Accent);
        CopyTextureDefaults(profile.Materials.DarkMechanic, defaults.DarkMechanic);
        CopyTextureDefaults(profile.Materials.ToolSteel, defaults.ToolSteel);
        CopyTextureDefaults(profile.Materials.Rubber, defaults.Rubber);
        CopyTextureDefaults(profile.Materials.BuildingShell, defaults.BuildingShell);
        CopyTextureDefaults(profile.Materials.GroundRock, defaults.GroundRock);
    }

    private static void CopyTextureDefaults(MaterialLook destination, MaterialLook source)
    {
        destination.TextureStrength = source.TextureStrength;
        destination.TextureScale = source.TextureScale;
        destination.ReliefStrength = source.ReliefStrength;
        destination.RoughnessVariation = source.RoughnessVariation;
        destination.TextureBlendMode = source.TextureBlendMode;
    }

    private static void ApplySchemaFiveMigration(M7LookProfile profile)
    {
        profile.Emission.SignalPulseAmount = profile.Emission.PulseAmount;
        profile.Emission.SignalPulseSpeed = profile.Emission.PulseSpeed;
        profile.Emission.LampPulseAmount = 0f;
        profile.Emission.LampPulseSpeed = 0f;
        profile.Emission.CrystalPulseAmount = profile.Emission.PulseAmount;
        profile.Emission.CrystalPulseSpeed = profile.Emission.PulseSpeed;
        profile.Emission.PulseAmount = 0f;
        profile.Emission.PulseSpeed = 0f;
    }

    private static void MergeMissingMaterialDefaults(M7LookProfile profile, JsonElement root)
    {
        if (!root.TryGetProperty("materials", out JsonElement materials) || materials.ValueKind != JsonValueKind.Object)
            return;

        MaterialCollection defaults = CreateDefault().Materials;
        MergeMissingMaterialDefaults(materials, "paintedHull", profile.Materials.PaintedHull, defaults.PaintedHull);
        MergeMissingMaterialDefaults(materials, "structuralEarth", profile.Materials.StructuralEarth, defaults.StructuralEarth);
        MergeMissingMaterialDefaults(materials, "accent", profile.Materials.Accent, defaults.Accent);
        MergeMissingMaterialDefaults(materials, "darkMechanic", profile.Materials.DarkMechanic, defaults.DarkMechanic);
        MergeMissingMaterialDefaults(materials, "toolSteel", profile.Materials.ToolSteel, defaults.ToolSteel);
        MergeMissingMaterialDefaults(materials, "rubber", profile.Materials.Rubber, defaults.Rubber);
        MergeMissingMaterialDefaults(materials, "buildingShell", profile.Materials.BuildingShell, defaults.BuildingShell);
        MergeMissingMaterialDefaults(materials, "groundRock", profile.Materials.GroundRock, defaults.GroundRock);
    }

    private static void MergeMissingMaterialDefaults(
        JsonElement materials,
        string familyName,
        MaterialLook? destination,
        MaterialLook defaults)
    {
        if (destination is null ||
            !materials.TryGetProperty(familyName, out JsonElement source) ||
            source.ValueKind != JsonValueKind.Object)
            return;

        if (!HasValue(source, "baseColor")) destination.BaseColor = defaults.BaseColor;
        if (!HasValue(source, "metallic")) destination.Metallic = defaults.Metallic;
        if (!HasValue(source, "roughness")) destination.Roughness = defaults.Roughness;
        if (!HasValue(source, "specular")) destination.Specular = defaults.Specular;
        if (!HasValue(source, "clearcoat")) destination.Clearcoat = defaults.Clearcoat;
        if (!HasValue(source, "clearcoatRoughness")) destination.ClearcoatRoughness = defaults.ClearcoatRoughness;
        if (!HasValue(source, "fresnel")) destination.Fresnel = defaults.Fresnel;
        if (!HasValue(source, "microStrength")) destination.MicroStrength = defaults.MicroStrength;
        if (!HasValue(source, "microScale")) destination.MicroScale = defaults.MicroScale;
        if (!HasValue(source, "macroVariation")) destination.MacroVariation = defaults.MacroVariation;
        if (!HasValue(source, "edgeWear")) destination.EdgeWear = defaults.EdgeWear;
        if (!HasValue(source, "dustAmount")) destination.DustAmount = defaults.DustAmount;
        if (!HasValue(source, "brushedAmount")) destination.BrushedAmount = defaults.BrushedAmount;
        if (!HasValue(source, "textureStrength")) destination.TextureStrength = defaults.TextureStrength;
        if (!HasValue(source, "textureScale")) destination.TextureScale = defaults.TextureScale;
        if (!HasValue(source, "reliefStrength")) destination.ReliefStrength = defaults.ReliefStrength;
        if (!HasValue(source, "roughnessVariation")) destination.RoughnessVariation = defaults.RoughnessVariation;
        if (!HasValue(source, "textureBlendMode")) destination.TextureBlendMode = defaults.TextureBlendMode;
    }

    private static bool HasValue(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind != JsonValueKind.Null;

    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        Camera ??= new CameraLook();
        Scene ??= new SceneLook();
        Shading ??= new ShadingLook();
        Materials ??= new MaterialCollection();
        Glass ??= new GlassLook();
        Emission ??= new EmissionLook();
        Lighting ??= new LightingLook();
        WorldCycle ??= new WorldCycleLook();
        Post ??= new PostLook();
        Outline ??= new OutlineLook();
        Animation ??= new AnimationLook();
        Destruction ??= new DestructionLook();
        Vfx ??= new VfxLook();
        VfxPool ??= new VfxPoolLook();
        Ground ??= new GroundLook();

        Camera.ZoomCells = Clamp(Camera.ZoomCells, 24f, 72f);
        Camera.PitchDegrees = Clamp(Camera.PitchDegrees, 30f, 80f);
        Camera.YawDegrees = WrapDegrees(Camera.YawDegrees);
        Camera.FocusX = Clamp(Camera.FocusX, -42f, 42f);
        Camera.FocusZ = Clamp(Camera.FocusZ, -42f, 42f);
        Scene.AnimationSpeed = Clamp(Scene.AnimationSpeed, 0f, 2f);

        Shading.DiffuseWrap = Clamp01(Shading.DiffuseWrap);
        Shading.ShadowFloor = Clamp01(Shading.ShadowFloor);
        Shading.LightBands = Clamp(Shading.LightBands, 0, 8);
        Shading.BandSoftness = Clamp01(Shading.BandSoftness);
        Shading.GlobalSpecular = Clamp(Shading.GlobalSpecular, 0f, 2f);
        Shading.RimStrength = Clamp(Shading.RimStrength, 0f, 2f);
        Shading.RimWidth = Clamp(Shading.RimWidth, 0.05f, 1f);
        Shading.ShadowTint = M7ProfileColor.Normalize(Shading.ShadowTint, "#002d72");
        Shading.HighlightTint = M7ProfileColor.Normalize(Shading.HighlightTint, "#ffdcb6");

        Materials.Normalize();
        Glass.Tint = M7ProfileColor.Normalize(Glass.Tint, "#31595b");
        Glass.Opacity = Clamp(Glass.Opacity, 0.05f, 1f);
        Glass.Roughness = Clamp01(Glass.Roughness);
        Glass.Ior = Clamp(Glass.Ior, 1f, 2.5f);
        Glass.RefractionStrength = Clamp01(Glass.RefractionStrength);
        Glass.EdgeBrightness = Clamp(Glass.EdgeBrightness, 0f, 2f);
        Emission.SignalEnergy = Clamp(Emission.SignalEnergy, 0f, 12f);
        Emission.LampEnergy = Clamp(Emission.LampEnergy, 0f, 12f);
        Emission.CrystalEnergy = Clamp(Emission.CrystalEnergy, 0f, 12f);
        Emission.PulseAmount = Clamp01(Emission.PulseAmount);
        Emission.PulseSpeed = Clamp(Emission.PulseSpeed, 0f, 8f);
        Emission.SignalPulseAmount = Clamp01(Emission.SignalPulseAmount);
        Emission.SignalPulseSpeed = Clamp(Emission.SignalPulseSpeed, 0f, 8f);
        Emission.LampPulseAmount = Clamp01(Emission.LampPulseAmount);
        Emission.LampPulseSpeed = Clamp(Emission.LampPulseSpeed, 0f, 8f);
        Emission.CrystalPulseAmount = Clamp01(Emission.CrystalPulseAmount);
        Emission.CrystalPulseSpeed = Clamp(Emission.CrystalPulseSpeed, 0f, 8f);
        Emission.HaloIntensity = Clamp(Emission.HaloIntensity, 0f, 3f);
        Emission.HaloSize = Clamp(Emission.HaloSize, 0.5f, 4f);
        Emission.EdgeDarkening = Clamp01(Emission.EdgeDarkening);
        Emission.LocalLightEnergy = Clamp(Emission.LocalLightEnergy, 0f, 4f);
        Emission.LocalLightRange = Clamp(Emission.LocalLightRange, 0.5f, 8f);
        Emission.SignalColor = M7ProfileColor.Normalize(Emission.SignalColor, "#ef6915");
        Emission.LampColor = M7ProfileColor.Normalize(Emission.LampColor, "#7dff4c");
        Emission.CrystalColor = M7ProfileColor.Normalize(Emission.CrystalColor, "#b7ff55");

        Lighting.KeyAzimuth = WrapDegrees(Lighting.KeyAzimuth);
        Lighting.KeyElevation = Clamp(Lighting.KeyElevation, 10f, 85f);
        Lighting.KeyEnergy = Clamp(Lighting.KeyEnergy, 0f, 8f);
        Lighting.KeyAngularSize = Clamp(Lighting.KeyAngularSize, 0f, 10f);
        Lighting.ShadowBlur = Clamp(Lighting.ShadowBlur, 0f, 8f);
        Lighting.FillAzimuth = WrapDegrees(Lighting.FillAzimuth);
        Lighting.FillElevation = Clamp(Lighting.FillElevation, 0f, 85f);
        Lighting.FillEnergy = Clamp(Lighting.FillEnergy, 0f, 8f);
        Lighting.AmbientEnergy = Clamp(Lighting.AmbientEnergy, 0f, 4f);
        Lighting.RimEnergy = Clamp(Lighting.RimEnergy, 0f, 8f);
        Lighting.ShadowOpacity = Clamp01(Lighting.ShadowOpacity);
        Lighting.BackgroundInfluence = Clamp01(Lighting.BackgroundInfluence);
        Lighting.KeyColor = M7ProfileColor.Normalize(Lighting.KeyColor, "#fff4e5");
        Lighting.FillColor = M7ProfileColor.Normalize(Lighting.FillColor, "#7caee6");
        Lighting.AmbientColor = M7ProfileColor.Normalize(Lighting.AmbientColor, "#8ca1b5");
        Lighting.RimColor = M7ProfileColor.Normalize(Lighting.RimColor, "#74a9ff");
        Lighting.BackgroundColor = M7ProfileColor.Normalize(Lighting.BackgroundColor, "#151d25");
        WorldCycle.Normalize();

        Post.Exposure = Clamp(Post.Exposure, -2f, 2f);
        Post.Brightness = Clamp(Post.Brightness, 0.4f, 1.8f);
        Post.Contrast = Clamp(Post.Contrast, 0.4f, 2f);
        Post.Saturation = Clamp(Post.Saturation, 0f, 2f);
        Post.Temperature = Clamp(Post.Temperature, -1f, 1f);
        Post.Tint = Clamp(Post.Tint, -1f, 1f);
        Post.Tonemapper = Clamp(Post.Tonemapper, 0, 4);
        Post.BloomIntensity = Clamp(Post.BloomIntensity, 0f, 3f);
        Post.BloomThreshold = Clamp(Post.BloomThreshold, 0f, 8f);
        Post.BloomSpread = Clamp(Post.BloomSpread, 0f, 1f);
        Post.Vignette = Clamp01(Post.Vignette);
        Post.FilmGrain = Clamp(Post.FilmGrain, 0f, 0.3f);
        Post.GrainScale = Clamp(Post.GrainScale, 0.5f, 4f);
        Post.Sharpen = Clamp(Post.Sharpen, 0f, 1.5f);
        Post.PosterizeLevels = Clamp(Post.PosterizeLevels, 0, 32);
        Post.Dither = Clamp01(Post.Dither);

        Outline.WidthPixels = Clamp(Outline.WidthPixels, 0.5f, 8f);
        Outline.Opacity = Clamp01(Outline.Opacity);
        Outline.DepthThreshold = Clamp(Outline.DepthThreshold, 0.0001f, 0.2f);
        Outline.NormalThreshold = Clamp(Outline.NormalThreshold, 0.01f, 2f);
        Outline.SilhouetteStrength = Clamp(Outline.SilhouetteStrength, 0f, 3f);
        Outline.CreaseStrength = Clamp(Outline.CreaseStrength, 0f, 3f);
        Outline.DistanceFade = Clamp(Outline.DistanceFade, 0f, 2f);
        Outline.Color = M7ProfileColor.Normalize(Outline.Color, "#101820");

        Animation.Normalize();
        Destruction.Normalize();
        Destruction.DustColor = M7ProfileColor.Normalize(Destruction.DustColor, "#776657");

        Vfx.TracerWidth = Clamp(Vfx.TracerWidth, 0.01f, 0.8f);
        Vfx.TracerLength = Clamp(Vfx.TracerLength, 0.1f, 8f);
        Vfx.TracerSpeed = Clamp(Vfx.TracerSpeed, 1f, 50f);
        Vfx.TracerEnergy = Clamp(Vfx.TracerEnergy, 0f, 12f);
        Vfx.MuzzleSize = Clamp(Vfx.MuzzleSize, 0.05f, 2f);
        Vfx.ImpactSize = Clamp(Vfx.ImpactSize, 0.05f, 3f);
        Vfx.SparkCount = Clamp(Vfx.SparkCount, 0, 20);
        Vfx.SparkSize = Clamp(Vfx.SparkSize, 0.01f, 0.4f);
        Vfx.SmokeAmount = Clamp(Vfx.SmokeAmount, 0, 20);
        Vfx.SmokeOpacity = Clamp01(Vfx.SmokeOpacity);
        Vfx.SmokeSize = Clamp(Vfx.SmokeSize, 0.1f, 4f);
        Vfx.SmokeRise = Clamp(Vfx.SmokeRise, 0f, 6f);
        Vfx.FireSize = Clamp(Vfx.FireSize, 0.1f, 4f);
        Vfx.FireEnergy = Clamp(Vfx.FireEnergy, 0f, 12f);
        Vfx.FireFlicker = Clamp01(Vfx.FireFlicker);
        Vfx.FireLightEnergy = Clamp(Vfx.FireLightEnergy, 0f, 12f);
        Vfx.FireLightRange = Clamp(Vfx.FireLightRange, 1f, 18f);
        Vfx.TracerColor = M7ProfileColor.Normalize(Vfx.TracerColor, "#4bff2e");
        Vfx.SmokeColor = M7ProfileColor.Normalize(Vfx.SmokeColor, "#252a2d");
        Vfx.FireColor = M7ProfileColor.Normalize(Vfx.FireColor, "#ff6a16");
        VfxPool.Normalize();

        Ground.MacroAmount = Clamp01(Ground.MacroAmount);
        Ground.MacroScale = Clamp(Ground.MacroScale, 0.01f, 2f);
        Ground.MicroAmount = Clamp01(Ground.MicroAmount);
        Ground.MicroScale = Clamp(Ground.MicroScale, 0.1f, 20f);
        Ground.TracksOpacity = Clamp01(Ground.TracksOpacity);
        Ground.TracksWidth = Clamp(Ground.TracksWidth, 0.2f, 2f);
        Ground.TracksLength = Clamp(Ground.TracksLength, 1f, 16f);
        Ground.TrackTreadScale = Clamp(Ground.TrackTreadScale, 1f, 24f);
        Ground.UnitSeparation = Clamp(Ground.UnitSeparation, 4f, 14f);
        Ground.SecondaryColor = M7ProfileColor.Normalize(Ground.SecondaryColor, "#353d41");
        Ground.DustTint = M7ProfileColor.Normalize(Ground.DustTint, "#78644d");
    }

    private static float WrapDegrees(float value) => ((value % 360f) + 360f) % 360f;
    private static float Clamp01(float value) => Clamp(value, 0f, 1f);
    private static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
}

public sealed class CameraLook
{
    public float ZoomCells { get; set; } = 39.5f;
    public float PitchDegrees { get; set; } = 46.515995f;
    public float YawDegrees { get; set; } = 43.847992f;
    public float FocusX { get; set; }
    public float FocusZ { get; set; }
}

public sealed class SceneLook
{
    public float AnimationSpeed { get; set; } = 1f;
    public bool FiringEnabled { get; set; } = true;
    public bool BurningEnabled { get; set; } = true;
    public bool DustEnabled { get; set; } = true;
    public bool FogPreviewEnabled { get; set; } = true;
    public bool Paused { get; set; }
}

public sealed class ShadingLook
{
    public float DiffuseWrap { get; set; } = 0.02f;
    public float ShadowFloor { get; set; }
    public int LightBands { get; set; }
    public float BandSoftness { get; set; }
    public float GlobalSpecular { get; set; } = 0.43f;
    public float RimStrength { get; set; }
    public float RimWidth { get; set; } = 0.05f;
    public string ShadowTint { get; set; } = "#002d72";
    public string HighlightTint { get; set; } = "#ffdcb6";
}

public sealed class MaterialCollection
{
    public MaterialLook PaintedHull { get; set; } = new("#07867e", 0.59f, 0.43f, 0.05f, 0.58f, 0.20f, 0.54f)
        { ClearcoatRoughness = 0.22f, Fresnel = 0.20f, MicroStrength = 0.17f, MicroScale = 3.6f, EdgeWear = 1f,
          BrushedAmount = 0.11f, TextureStrength = 0.32f, TextureScale = 1.4f, ReliefStrength = 0.18f, RoughnessVariation = 0.24f };
    public MaterialLook StructuralEarth { get; set; } = new("#71432d", 0.00f, 0.74f, 0.25f, 0.02f, 0.22f, 0.18f) { TextureStrength = 0.18f, TextureScale = 1.8f };
    public MaterialLook Accent { get; set; } = new("#e8a21b", 0.02f, 0.32f, 0.58f, 0.20f, 0.08f, 0.04f) { TextureStrength = 0.10f, TextureScale = 1.6f };
    public MaterialLook DarkMechanic { get; set; } = new("#253039", 0.64f, 0.30f, 0.72f, 0.05f, 0.10f, 0.08f) { TextureStrength = 0.18f, TextureScale = 2.2f };
    public MaterialLook ToolSteel { get; set; } = new("#aebbc2", 0.94f, 0.20f, 0.95f, 0.03f, 0.22f, 0.04f) { BrushedAmount = 0.42f, TextureStrength = 0.30f, TextureScale = 2.4f };
    public MaterialLook Rubber { get; set; } = new("#101418", 0.00f, 0.92f, 0.10f, 0.00f, 0.04f, 0.16f) { MicroStrength = 0.26f, MicroScale = 11f, TextureStrength = 0.34f, TextureScale = 2.8f };
    public MaterialLook BuildingShell { get; set; } = new("#596b73", 0.24f, 0.54f, 0.46f, 0.08f, 0.18f, 0.14f) { TextureStrength = 0.26f, TextureScale = 1.2f };
    public MaterialLook GroundRock { get; set; } = new("#9f732c", 0.00f, 0.92f, 0.14f, 0.00f, 0.24f, 0.16f)
        { MicroStrength = 0.20f, MicroScale = 7f, TextureStrength = 0.42f, TextureScale = 0.055f,
          ReliefStrength = 0.48f, RoughnessVariation = 0.34f, TextureBlendMode = 1 };

    public void Normalize()
    {
        MaterialCollection defaults = new();
        PaintedHull ??= defaults.PaintedHull;
        StructuralEarth ??= defaults.StructuralEarth;
        Accent ??= defaults.Accent;
        DarkMechanic ??= defaults.DarkMechanic;
        ToolSteel ??= defaults.ToolSteel;
        Rubber ??= defaults.Rubber;
        BuildingShell ??= defaults.BuildingShell;
        GroundRock ??= defaults.GroundRock;
        PaintedHull.Normalize(defaults.PaintedHull); StructuralEarth.Normalize(defaults.StructuralEarth);
        Accent.Normalize(defaults.Accent); DarkMechanic.Normalize(defaults.DarkMechanic);
        ToolSteel.Normalize(defaults.ToolSteel); Rubber.Normalize(defaults.Rubber);
        BuildingShell.Normalize(defaults.BuildingShell); GroundRock.Normalize(defaults.GroundRock);
    }
}

public sealed class MaterialLook
{
    public MaterialLook() { }

    public MaterialLook(string color, float metallic, float roughness, float specular, float clearcoat, float macroVariation, float dustAmount)
    {
        BaseColor = color; Metallic = metallic; Roughness = roughness; Specular = specular;
        Clearcoat = clearcoat; MacroVariation = macroVariation; DustAmount = dustAmount;
    }

    public string BaseColor { get; set; } = "#808080";
    public float Metallic { get; set; }
    public float Roughness { get; set; } = 0.5f;
    public float Specular { get; set; } = 0.5f;
    public float Clearcoat { get; set; }
    public float ClearcoatRoughness { get; set; } = 0.22f;
    public float Fresnel { get; set; } = 0.35f;
    public float MicroStrength { get; set; } = 0.08f;
    public float MicroScale { get; set; } = 5f;
    public float MacroVariation { get; set; } = 0.08f;
    public float EdgeWear { get; set; } = 0.06f;
    public float DustAmount { get; set; } = 0.06f;
    public float BrushedAmount { get; set; }
    public float TextureStrength { get; set; }
    public float TextureScale { get; set; } = 1f;
    public float ReliefStrength { get; set; } = 0.12f;
    public float RoughnessVariation { get; set; } = 0.18f;
    public int TextureBlendMode { get; set; }

    public void Normalize(MaterialLook? defaults = null)
    {
        BaseColor = M7ProfileColor.Normalize(BaseColor, defaults?.BaseColor ?? "#808080");
        Metallic = Math.Clamp(Metallic, 0f, 1f); Roughness = Math.Clamp(Roughness, 0f, 1f);
        Specular = Math.Clamp(Specular, 0f, 1f); Clearcoat = Math.Clamp(Clearcoat, 0f, 1f);
        ClearcoatRoughness = Math.Clamp(ClearcoatRoughness, 0f, 1f); Fresnel = Math.Clamp(Fresnel, 0f, 1f);
        MicroStrength = Math.Clamp(MicroStrength, 0f, 1f); MicroScale = Math.Clamp(MicroScale, 0.1f, 20f);
        MacroVariation = Math.Clamp(MacroVariation, 0f, 1f); EdgeWear = Math.Clamp(EdgeWear, 0f, 1f);
        DustAmount = Math.Clamp(DustAmount, 0f, 1f); BrushedAmount = Math.Clamp(BrushedAmount, 0f, 1f);
        TextureStrength = Math.Clamp(TextureStrength, 0f, 1f); TextureScale = Math.Clamp(TextureScale, 0.05f, 12f);
        ReliefStrength = Math.Clamp(ReliefStrength, 0f, 1.5f);
        RoughnessVariation = Math.Clamp(RoughnessVariation, 0f, 1f);
        TextureBlendMode = Math.Clamp(TextureBlendMode, 0, 2);
    }
}

public sealed class GlassLook
{
    public string Tint { get; set; } = "#31595b";
    public float Opacity { get; set; } = 0.74f;
    public float Roughness { get; set; } = 0.68f;
    public float Ior { get; set; } = 1.73f;
    public float RefractionStrength { get; set; } = 0.96f;
    public float EdgeBrightness { get; set; } = 2f;
}

public sealed class EmissionLook
{
    public string SignalColor { get; set; } = "#ef6915";
    public float SignalEnergy { get; set; } = 1f;
    public string LampColor { get; set; } = "#7dff4c";
    public float LampEnergy { get; set; } = 1f;
    public string CrystalColor { get; set; } = "#b7ff55";
    public float CrystalEnergy { get; set; } = 1.5f;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public float PulseAmount { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public float PulseSpeed { get; set; }
    public float SignalPulseAmount { get; set; } = 0.41f;
    public float SignalPulseSpeed { get; set; } = 2.5f;
    public float LampPulseAmount { get; set; }
    public float LampPulseSpeed { get; set; }
    public float CrystalPulseAmount { get; set; } = 0.41f;
    public float CrystalPulseSpeed { get; set; } = 2.5f;
    public float HaloIntensity { get; set; }
    public float HaloSize { get; set; } = 4f;
    public float EdgeDarkening { get; set; } = 0.27f;
    public float LocalLightEnergy { get; set; } = 4f;
    public float LocalLightRange { get; set; } = 7.8f;
}

public sealed class LightingLook
{
    public float KeyAzimuth { get; set; } = 144f;
    public float KeyElevation { get; set; } = 22f;
    public string KeyColor { get; set; } = "#fff4e5";
    public float KeyEnergy { get; set; } = 1.25f;
    public float KeyAngularSize { get; set; } = 1.2f;
    public float ShadowBlur { get; set; } = 1.6f;
    public float FillAzimuth { get; set; } = 132f;
    public float FillElevation { get; set; } = 36f;
    public string FillColor { get; set; } = "#7caee6";
    public float FillEnergy { get; set; } = 0.05f;
    public string AmbientColor { get; set; } = "#8ca1b5";
    public float AmbientEnergy { get; set; } = 0.20f;
    public bool RimLightEnabled { get; set; } = true;
    public string RimColor { get; set; } = "#74a9ff";
    public float RimEnergy { get; set; } = 0.10f;
    public float ShadowOpacity { get; set; } = 0.96f;
    public string BackgroundColor { get; set; } = "#151d25";
    public float BackgroundInfluence { get; set; } = 0.16f;
}

public sealed class WorldCycleLook
{
    public bool Enabled { get; set; }
    public bool AnimatePreview { get; set; }
    public int Environment { get; set; }
    public float LocalTimeHours { get; set; } = 14f;
    public float PreviewDaySeconds { get; set; } = 90f;
    public float DaylightHours { get; set; } = 12f;
    public float DarknessHours { get; set; } = 12f;
    public float NightReadability { get; set; } = 0.42f;
    public float LocalLightBoost { get; set; } = 1.35f;

    public void Normalize()
    {
        Environment = Math.Clamp(Environment, 0, 4);
        LocalTimeHours = Math.Clamp(LocalTimeHours, 0f, 24f);
        PreviewDaySeconds = Math.Clamp(PreviewDaySeconds, 10f, 600f);
        DaylightHours = Math.Clamp(DaylightHours, 0.1f, 1000f);
        DarknessHours = Math.Clamp(DarknessHours, 0.1f, 1000f);
        NightReadability = Math.Clamp(NightReadability, 0.1f, 1f);
        LocalLightBoost = Math.Clamp(LocalLightBoost, 0.5f, 4f);
        if (Environment == 4) AnimatePreview = false;
    }
}

public sealed class PostLook
{
    public bool Enabled { get; set; } = true;
    public float Exposure { get; set; } = -0.35f;
    public float Brightness { get; set; } = 1.37f;
    public float Contrast { get; set; } = 1f;
    public float Saturation { get; set; } = 1.75f;
    public float Temperature { get; set; } = 1f;
    public float Tint { get; set; } = -0.1f;
    public int Tonemapper { get; set; }
    public bool BloomEnabled { get; set; } = true;
    public float BloomIntensity { get; set; } = 0.28f;
    public float BloomThreshold { get; set; } = 1.4f;
    public float BloomSpread { get; set; } = 0.21f;
    public float Vignette { get; set; } = 0.46f;
    public float FilmGrain { get; set; } = 0.01f;
    public float GrainScale { get; set; } = 4f;
    public float Sharpen { get; set; } = 1.06f;
    public int PosterizeLevels { get; set; }
    public float Dither { get; set; }
}

public sealed class OutlineLook
{
    public bool Enabled { get; set; } = true;
    public string Color { get; set; } = "#000000";
    public float WidthPixels { get; set; } = 3.1f;
    public float Opacity { get; set; } = 1f;
    public float DepthThreshold { get; set; } = 0.0026f;
    public float NormalThreshold { get; set; } = 1.78f;
    public float SilhouetteStrength { get; set; } = 1.5f;
    public float CreaseStrength { get; set; } = 0.98f;
    public float DistanceFade { get; set; }
}

public sealed class AnimationLook
{
    public bool Enabled { get; set; } = true;
    public bool PreviewChoreography { get; set; } = true;
    public float PreviewLocomotionSpeed { get; set; } = 2.2f;
    public float LocomotionReferenceSpeed { get; set; } = 3f;
    public float BlendResponse { get; set; } = 8f;
    public float WheelTurnsPerWorldUnit { get; set; } = 0.34f;
    public float SuspensionAmplitude { get; set; } = 0.035f;
    public float SuspensionFrequency { get; set; } = 1.8f;
    public float BodyLeanDegrees { get; set; } = 1.2f;
    public float DrillTurnsPerSecond { get; set; } = 0.9f;
    public float RecoilDistance { get; set; } = 0.12f;
    public float RecoilRecovery { get; set; } = 7f;
    public float TransformationLift { get; set; } = 0.18f;
    public float TransformationTiltDegrees { get; set; } = 8f;
    [JsonIgnore] public float DamageWobbleDegrees { get; set; }
    public int TierOverride { get; set; }

    public void Normalize()
    {
        PreviewLocomotionSpeed = Math.Clamp(PreviewLocomotionSpeed, 0f, 12f);
        PresentationAnimationTuning tuning = ToTuning();
        tuning.Normalize();
        LocomotionReferenceSpeed = tuning.LocomotionReferenceSpeed;
        BlendResponse = tuning.BlendResponse;
        WheelTurnsPerWorldUnit = tuning.WheelTurnsPerWorldUnit;
        SuspensionAmplitude = tuning.SuspensionAmplitude;
        SuspensionFrequency = tuning.SuspensionFrequency;
        BodyLeanDegrees = tuning.BodyLeanDegrees;
        DrillTurnsPerSecond = tuning.DrillTurnsPerSecond;
        RecoilDistance = tuning.RecoilDistance;
        RecoilRecovery = tuning.RecoilRecovery;
        TransformationLift = tuning.TransformationLift;
        TransformationTiltDegrees = tuning.TransformationTiltDegrees;
        DamageWobbleDegrees = tuning.DamageWobbleDegrees;
        TierOverride = tuning.TierOverride;
    }

    public PresentationAnimationTuning ToTuning() => new()
    {
        Enabled = Enabled,
        LocomotionReferenceSpeed = LocomotionReferenceSpeed,
        BlendResponse = BlendResponse,
        WheelTurnsPerWorldUnit = WheelTurnsPerWorldUnit,
        SuspensionAmplitude = SuspensionAmplitude,
        SuspensionFrequency = SuspensionFrequency,
        BodyLeanDegrees = BodyLeanDegrees,
        DrillTurnsPerSecond = DrillTurnsPerSecond,
        RecoilDistance = RecoilDistance,
        RecoilRecovery = RecoilRecovery,
        TransformationLift = TransformationLift,
        TransformationTiltDegrees = TransformationTiltDegrees,
        DamageWobbleDegrees = DamageWobbleDegrees,
        TierOverride = TierOverride
    };
}

public sealed class DestructionLook
{
    public bool Enabled { get; set; } = true;
    public bool AutoPreview { get; set; } = true;
    // Preserve the burning-structure lighting reference during the default
    // loop; the structure remains available as an explicit preview target.
    public int PreviewTarget { get; set; }
    public float PreviewLoopSeconds { get; set; } = 5.5f;
    public float PreviewHoldSeconds { get; set; } = 3.8f;
    public int HeroPoolBudget { get; set; } = 6;
    public int DustPoolBudget { get; set; } = 6;
    [JsonIgnore] public float CollapseSeconds { get; set; } = 0.72f;
    [JsonIgnore] public float SettleTiltDegrees { get; set; }
    [JsonIgnore] public float WreckWidthRatio { get; set; } = 1f;
    [JsonIgnore] public float WreckHeightRatio { get; set; } = 0.03f;
    public int HeroFragmentCount { get; set; } = PooledLegoDebrisBurst.MaxFragments;
    public float FragmentScale { get; set; } = 0.85f;
    public float OutwardSpeed { get; set; } = 3.8f;
    public float UpwardSpeed { get; set; } = 4.6f;
    public float Gravity { get; set; } = 9.5f;
    public float Drag { get; set; } = 0.38f;
    public float Bounce { get; set; } = 0.24f;
    public float AngularSpeedDegrees { get; set; } = 260f;
    public float DebrisLifetime { get; set; } = 4.2f;
    public float FadeSeconds { get; set; } = 0.65f;
    public int DustCount { get; set; } = 22;
    public float DustSize { get; set; } = 0.55f;
    public float DustLifetime { get; set; } = 1.15f;
    public string DustColor { get; set; } = "#776657";
    public float DustOpacity { get; set; } = 0.58f;

    public void Normalize()
    {
        PreviewTarget = Math.Clamp(PreviewTarget, 0, 2);
        PreviewLoopSeconds = Math.Clamp(PreviewLoopSeconds, 2f, 15f);
        PreviewHoldSeconds = Math.Clamp(PreviewHoldSeconds, 0.2f, PreviewLoopSeconds - 0.2f);
        HeroPoolBudget = Math.Clamp(HeroPoolBudget, 0, 12);
        DustPoolBudget = Math.Clamp(DustPoolBudget, 0, 12);
        DustOpacity = Math.Clamp(DustOpacity, 0f, 1f);
        PresentationDestructionTuning tuning = ToTuning();
        tuning.Normalize();
        CollapseSeconds = tuning.CollapseSeconds;
        SettleTiltDegrees = tuning.SettleTiltDegrees;
        WreckWidthRatio = tuning.WreckWidthRatio;
        WreckHeightRatio = tuning.WreckHeightRatio;
        HeroFragmentCount = tuning.HeroFragmentCount;
        FragmentScale = tuning.FragmentScale;
        OutwardSpeed = tuning.OutwardSpeed;
        UpwardSpeed = tuning.UpwardSpeed;
        Gravity = tuning.Gravity;
        Drag = tuning.Drag;
        Bounce = tuning.Bounce;
        AngularSpeedDegrees = tuning.AngularSpeedDegrees;
        DebrisLifetime = tuning.DebrisLifetime;
        FadeSeconds = tuning.FadeSeconds;
        DustCount = tuning.DustCount;
        DustSize = tuning.DustSize;
        DustLifetime = tuning.DustLifetime;
    }

    public PresentationDestructionTuning ToTuning() => new()
    {
        Enabled = Enabled,
        CollapseSeconds = CollapseSeconds,
        SettleTiltDegrees = SettleTiltDegrees,
        WreckWidthRatio = WreckWidthRatio,
        WreckHeightRatio = WreckHeightRatio,
        HeroFragmentCount = HeroFragmentCount,
        FragmentScale = FragmentScale,
        OutwardSpeed = OutwardSpeed,
        UpwardSpeed = UpwardSpeed,
        Gravity = Gravity,
        Drag = Drag,
        Bounce = Bounce,
        AngularSpeedDegrees = AngularSpeedDegrees,
        DebrisLifetime = DebrisLifetime,
        FadeSeconds = FadeSeconds,
        DustCount = DustCount,
        DustSize = DustSize,
        DustLifetime = DustLifetime
    };
}

public sealed class VfxLook
{
    public string TracerColor { get; set; } = "#4bff2e";
    public float TracerWidth { get; set; } = 0.08f;
    public float TracerLength { get; set; } = 3f;
    public float TracerSpeed { get; set; } = 31.5f;
    public float TracerEnergy { get; set; } = 12f;
    public float MuzzleSize { get; set; } = 1.26f;
    public float ImpactSize { get; set; } = 0.56f;
    public int SparkCount { get; set; } = 8;
    public float SparkSize { get; set; } = 0.06f;
    public int SmokeAmount { get; set; } = 20;
    public string SmokeColor { get; set; } = "#252a2d";
    public float SmokeOpacity { get; set; } = 1f;
    public float SmokeSize { get; set; } = 2.55f;
    public float SmokeRise { get; set; } = 6f;
    public string FireColor { get; set; } = "#ff6a16";
    public float FireSize { get; set; } = 4f;
    public float FireEnergy { get; set; } = 12f;
    public float FireFlicker { get; set; } = 1f;
    public float FireLightEnergy { get; set; } = 6f;
    public float FireLightRange { get; set; } = 10f;
}

public sealed class VfxPoolLook
{
    public int TracerBudget { get; set; } = 24;
    public int MuzzleBudget { get; set; } = 12;
    public int ImpactBudget { get; set; } = 16;
    public int PreviewEmitters { get; set; } = 1;

    public void Normalize()
    {
        TracerBudget = Math.Clamp(TracerBudget, 0, 64);
        MuzzleBudget = Math.Clamp(MuzzleBudget, 0, 32);
        ImpactBudget = Math.Clamp(ImpactBudget, 0, 48);
        PreviewEmitters = Math.Clamp(PreviewEmitters, 1, 4);
    }
}

public sealed class GroundLook
{
    public string SecondaryColor { get; set; } = "#3e3122";
    public float MacroAmount { get; set; } = 0.36f;
    public float MacroScale { get; set; } = 0.17f;
    public float MicroAmount { get; set; } = 1f;
    public float MicroScale { get; set; } = 20f;
    public string DustTint { get; set; } = "#78644d";
    public float TracksOpacity { get; set; } = 0.32f;
    public float TracksWidth { get; set; } = 0.7f;
    public float TracksLength { get; set; } = 5.3f;
    public float TrackTreadScale { get; set; } = 17f;
    public float UnitSeparation { get; set; } = 6.9f;
}

internal static class M7ProfileColor
{
    public static string Normalize(string? value, string fallback)
    {
        string source = !string.IsNullOrWhiteSpace(value) && Color.HtmlIsValid(value)
            ? value
            : fallback;
        return $"#{Color.FromHtml(source).ToHtml(false)}";
    }
}
