using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegoSpaceRTS.Presentation;

public sealed class M7LookProfile
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public CameraLook Camera { get; set; } = new();
    public SceneLook Scene { get; set; } = new();
    public ShadingLook Shading { get; set; } = new();
    public MaterialCollection Materials { get; set; } = new();
    public GlassLook Glass { get; set; } = new();
    public EmissionLook Emission { get; set; } = new();
    public LightingLook Lighting { get; set; } = new();
    public PostLook Post { get; set; } = new();
    public OutlineLook Outline { get; set; } = new();
    public VfxLook Vfx { get; set; } = new();
    public GroundLook Ground { get; set; } = new();
    public HudLook Hud { get; set; } = new();

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
            M7LookProfile? parsed = JsonSerializer.Deserialize<M7LookProfile>(json, JsonOptions);
            if (parsed is null)
            {
                profile = CreateDefault();
                error = "Clipboard does not contain an M7 Look profile.";
                return false;
            }
            if (parsed.SchemaVersion != CurrentSchemaVersion)
            {
                profile = CreateDefault();
                error = $"Profile schema {parsed.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.";
                return false;
            }
            parsed.Normalize();
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
        Post ??= new PostLook();
        Outline ??= new OutlineLook();
        Vfx ??= new VfxLook();
        Ground ??= new GroundLook();
        Hud ??= new HudLook();

        Camera.ZoomCells = Clamp(Camera.ZoomCells, 24f, 72f);
        Camera.PitchDegrees = Clamp(Camera.PitchDegrees, 52f, 64f);
        Camera.YawDegrees = NormalizeYaw(Camera.YawDegrees);
        Scene.AnimationSpeed = Clamp(Scene.AnimationSpeed, 0f, 2f);

        Shading.DiffuseWrap = Clamp01(Shading.DiffuseWrap);
        Shading.ShadowFloor = Clamp01(Shading.ShadowFloor);
        Shading.LightBands = Clamp(Shading.LightBands, 0, 8);
        Shading.BandSoftness = Clamp01(Shading.BandSoftness);
        Shading.GlobalSpecular = Clamp(Shading.GlobalSpecular, 0f, 2f);
        Shading.RimStrength = Clamp(Shading.RimStrength, 0f, 2f);
        Shading.RimWidth = Clamp(Shading.RimWidth, 0.05f, 1f);

        Materials.Normalize();
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
        Vfx.ScorchSize = Clamp(Vfx.ScorchSize, 0f, 6f);

        Ground.MacroAmount = Clamp01(Ground.MacroAmount);
        Ground.MacroScale = Clamp(Ground.MacroScale, 0.01f, 2f);
        Ground.MicroAmount = Clamp01(Ground.MicroAmount);
        Ground.MicroScale = Clamp(Ground.MicroScale, 0.1f, 20f);
        Ground.NormalStrength = Clamp(Ground.NormalStrength, 0f, 2f);
        Ground.TracksOpacity = Clamp01(Ground.TracksOpacity);
        Ground.ScorchOpacity = Clamp01(Ground.ScorchOpacity);
        Ground.UnitSeparation = Clamp(Ground.UnitSeparation, 4f, 14f);

        Hud.Scale = Clamp(Hud.Scale, 0.7f, 1.5f);
        Hud.PanelOpacity = Clamp01(Hud.PanelOpacity);
        Hud.Brightness = Clamp(Hud.Brightness, 0.3f, 2f);
        Hud.Saturation = Clamp(Hud.Saturation, 0f, 2f);
        Hud.MinimapContrast = Clamp(Hud.MinimapContrast, 0.4f, 2f);
        Hud.HealthBarWidth = Clamp(Hud.HealthBarWidth, 24f, 120f);
        Hud.HealthBarHeight = Clamp(Hud.HealthBarHeight, 2f, 14f);
        Hud.HealthBarOpacity = Clamp01(Hud.HealthBarOpacity);
        Hud.SelectionRingWidth = Clamp(Hud.SelectionRingWidth, 0.02f, 0.5f);
        Hud.SelectionBrightness = Clamp(Hud.SelectionBrightness, 0f, 5f);
        Hud.SelectionPulse = Clamp01(Hud.SelectionPulse);
        Hud.TargetMarkerBrightness = Clamp(Hud.TargetMarkerBrightness, 0f, 5f);
        Hud.IndicatorScale = Clamp(Hud.IndicatorScale, 0.5f, 2f);
    }

    private static float NormalizeYaw(float yaw)
    {
        float wrapped = WrapDegrees(yaw);
        return MathF.Round((wrapped - 45f) / 90f) * 90f + 45f switch
        {
            >= 360f => 45f,
            < 0f => 315f,
            float value => value
        };
    }

    private static float WrapDegrees(float value) => ((value % 360f) + 360f) % 360f;
    private static float Clamp01(float value) => Clamp(value, 0f, 1f);
    private static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
}

public sealed class CameraLook
{
    public float ZoomCells { get; set; } = 44f;
    public float PitchDegrees { get; set; } = 58f;
    public float YawDegrees { get; set; } = 45f;
}

public sealed class SceneLook
{
    public float AnimationSpeed { get; set; } = 1f;
    public bool FiringEnabled { get; set; } = true;
    public bool BurningEnabled { get; set; } = true;
    public bool DustEnabled { get; set; } = true;
    public bool FogPreviewEnabled { get; set; } = true;
    public bool HudEnabled { get; set; } = true;
    public bool SelectionEnabled { get; set; } = true;
    public bool Paused { get; set; }
}

public sealed class ShadingLook
{
    public float DiffuseWrap { get; set; } = 0.18f;
    public float ShadowFloor { get; set; } = 0.16f;
    public int LightBands { get; set; }
    public float BandSoftness { get; set; } = 0.35f;
    public float GlobalSpecular { get; set; } = 1f;
    public float RimStrength { get; set; } = 0.16f;
    public float RimWidth { get; set; } = 0.34f;
    public string ShadowTint { get; set; } = "#253246";
    public string HighlightTint { get; set; } = "#fff4df";
}

public sealed class MaterialCollection
{
    public MaterialLook PaintedHull { get; set; } = new("#07867e", 0.06f, 0.38f, 0.60f, 0.24f, 0.12f, 0.05f);
    public MaterialLook StructuralEarth { get; set; } = new("#71432d", 0.00f, 0.74f, 0.25f, 0.02f, 0.22f, 0.18f);
    public MaterialLook Accent { get; set; } = new("#e8a21b", 0.02f, 0.32f, 0.58f, 0.20f, 0.08f, 0.04f);
    public MaterialLook DarkMechanic { get; set; } = new("#253039", 0.64f, 0.30f, 0.72f, 0.05f, 0.10f, 0.08f);
    public MaterialLook ToolSteel { get; set; } = new("#aebbc2", 0.94f, 0.20f, 0.95f, 0.03f, 0.22f, 0.04f) { BrushedAmount = 0.42f };
    public MaterialLook Rubber { get; set; } = new("#101418", 0.00f, 0.92f, 0.10f, 0.00f, 0.04f, 0.16f) { MicroStrength = 0.26f, MicroScale = 11f };
    public MaterialLook BuildingShell { get; set; } = new("#596b73", 0.24f, 0.54f, 0.46f, 0.08f, 0.18f, 0.14f);
    public MaterialLook GroundRock { get; set; } = new("#565b5d", 0.00f, 0.92f, 0.14f, 0.00f, 0.24f, 0.16f) { MicroStrength = 0.20f, MicroScale = 7f };

    public void Normalize()
    {
        PaintedHull ??= new MaterialLook();
        StructuralEarth ??= new MaterialLook();
        Accent ??= new MaterialLook();
        DarkMechanic ??= new MaterialLook();
        ToolSteel ??= new MaterialLook();
        Rubber ??= new MaterialLook();
        BuildingShell ??= new MaterialLook();
        GroundRock ??= new MaterialLook();
        PaintedHull.Normalize(); StructuralEarth.Normalize(); Accent.Normalize(); DarkMechanic.Normalize();
        ToolSteel.Normalize(); Rubber.Normalize(); BuildingShell.Normalize(); GroundRock.Normalize();
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

    public void Normalize()
    {
        Metallic = Math.Clamp(Metallic, 0f, 1f); Roughness = Math.Clamp(Roughness, 0f, 1f);
        Specular = Math.Clamp(Specular, 0f, 1f); Clearcoat = Math.Clamp(Clearcoat, 0f, 1f);
        ClearcoatRoughness = Math.Clamp(ClearcoatRoughness, 0f, 1f); Fresnel = Math.Clamp(Fresnel, 0f, 1f);
        MicroStrength = Math.Clamp(MicroStrength, 0f, 1f); MicroScale = Math.Clamp(MicroScale, 0.1f, 20f);
        MacroVariation = Math.Clamp(MacroVariation, 0f, 1f); EdgeWear = Math.Clamp(EdgeWear, 0f, 1f);
        DustAmount = Math.Clamp(DustAmount, 0f, 1f); BrushedAmount = Math.Clamp(BrushedAmount, 0f, 1f);
    }
}

public sealed class GlassLook
{
    public string Tint { get; set; } = "#31595b";
    public float Opacity { get; set; } = 0.42f;
    public float Roughness { get; set; } = 0.16f;
    public float Ior { get; set; } = 1.46f;
    public float RefractionStrength { get; set; } = 0.12f;
    public float EdgeBrightness { get; set; } = 0.52f;
}

public sealed class EmissionLook
{
    public string SignalColor { get; set; } = "#ef4515";
    public float SignalEnergy { get; set; } = 3.2f;
    public string LampColor { get; set; } = "#7dff4c";
    public float LampEnergy { get; set; } = 4.0f;
    public string CrystalColor { get; set; } = "#55d9ff";
    public float CrystalEnergy { get; set; } = 4.4f;
    public float PulseAmount { get; set; } = 0.16f;
    public float PulseSpeed { get; set; } = 2.2f;
}

public sealed class LightingLook
{
    public float KeyAzimuth { get; set; } = 315f;
    public float KeyElevation { get; set; } = 54f;
    public string KeyColor { get; set; } = "#fff4e5";
    public float KeyEnergy { get; set; } = 0.92f;
    public float KeyAngularSize { get; set; } = 1.2f;
    public float ShadowBlur { get; set; } = 1.5f;
    public float FillAzimuth { get; set; } = 120f;
    public float FillElevation { get; set; } = 32f;
    public string FillColor { get; set; } = "#7caee6";
    public float FillEnergy { get; set; } = 0.20f;
    public string AmbientColor { get; set; } = "#8ca1b5";
    public float AmbientEnergy { get; set; } = 0.44f;
    public bool RimLightEnabled { get; set; } = true;
    public string RimColor { get; set; } = "#74a9ff";
    public float RimEnergy { get; set; } = 0.34f;
    public float ShadowOpacity { get; set; } = 0.86f;
    public string BackgroundColor { get; set; } = "#151d25";
}

public sealed class PostLook
{
    public bool Enabled { get; set; } = true;
    public float Exposure { get; set; }
    public float Brightness { get; set; } = 1f;
    public float Contrast { get; set; } = 1f;
    public float Saturation { get; set; } = 1.04f;
    public float Temperature { get; set; }
    public float Tint { get; set; }
    public int Tonemapper { get; set; } = 3;
    public bool BloomEnabled { get; set; } = true;
    public float BloomIntensity { get; set; } = 0.30f;
    public float BloomThreshold { get; set; } = 1.15f;
    public float BloomSpread { get; set; } = 0.28f;
    public float Vignette { get; set; } = 0.08f;
    public float FilmGrain { get; set; }
    public float Sharpen { get; set; } = 0.12f;
    public int PosterizeLevels { get; set; }
    public float Dither { get; set; }
}

public sealed class OutlineLook
{
    public bool Enabled { get; set; }
    public string Color { get; set; } = "#101820";
    public float WidthPixels { get; set; } = 2.2f;
    public float Opacity { get; set; } = 0.78f;
    public float DepthThreshold { get; set; } = 0.003f;
    public float NormalThreshold { get; set; } = 0.22f;
    public float SilhouetteStrength { get; set; } = 1.20f;
    public float CreaseStrength { get; set; } = 0.62f;
    public float DistanceFade { get; set; } = 0.06f;
}

public sealed class VfxLook
{
    public string TracerColor { get; set; } = "#ffb02e";
    public float TracerWidth { get; set; } = 0.10f;
    public float TracerLength { get; set; } = 1.5f;
    public float TracerSpeed { get; set; } = 15f;
    public float TracerEnergy { get; set; } = 5f;
    public float MuzzleSize { get; set; } = 0.38f;
    public float ImpactSize { get; set; } = 0.56f;
    public int SparkCount { get; set; } = 8;
    public float SparkSize { get; set; } = 0.06f;
    public int SmokeAmount { get; set; } = 9;
    public string SmokeColor { get; set; } = "#252a2d";
    public float SmokeOpacity { get; set; } = 0.54f;
    public float SmokeSize { get; set; } = 1.15f;
    public float SmokeRise { get; set; } = 1.5f;
    public string FireColor { get; set; } = "#ff6a16";
    public float FireSize { get; set; } = 1.1f;
    public float FireEnergy { get; set; } = 5f;
    public float FireFlicker { get; set; } = 0.38f;
    public float ScorchSize { get; set; } = 2.1f;
}

public sealed class GroundLook
{
    public string SecondaryColor { get; set; } = "#353d41";
    public float MacroAmount { get; set; } = 0.28f;
    public float MacroScale { get; set; } = 0.13f;
    public float MicroAmount { get; set; } = 0.18f;
    public float MicroScale { get; set; } = 6.5f;
    public float NormalStrength { get; set; } = 0.42f;
    public string DustTint { get; set; } = "#78644d";
    public float TracksOpacity { get; set; } = 0.34f;
    public float ScorchOpacity { get; set; } = 0.60f;
    public float UnitSeparation { get; set; } = 7.2f;
}

public sealed class HudLook
{
    public float Scale { get; set; } = 1f;
    public float PanelOpacity { get; set; } = 0.88f;
    public float Brightness { get; set; } = 1f;
    public float Saturation { get; set; } = 1f;
    public string AccentColor { get; set; } = "#e6a81f";
    public float MinimapContrast { get; set; } = 1f;
    public float HealthBarWidth { get; set; } = 54f;
    public float HealthBarHeight { get; set; } = 6f;
    public float HealthBarOpacity { get; set; } = 0.88f;
    public float SelectionRingWidth { get; set; } = 0.10f;
    public float SelectionBrightness { get; set; } = 1.9f;
    public float SelectionPulse { get; set; } = 0.12f;
    public float TargetMarkerBrightness { get; set; } = 1.6f;
    public float IndicatorScale { get; set; } = 1f;
}
