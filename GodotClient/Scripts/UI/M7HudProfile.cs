using System.Text.Json;
using System.Text.Json.Serialization;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.UI;

public sealed class M7HudProfile
{
    public const int CurrentSchemaVersion = 8;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public HudLayoutProfile Layout { get; set; } = new();
    public HudTypographyProfile Typography { get; set; } = new();
    public HudSurfaceProfile Surface { get; set; } = new();
    public HudArtSkinProfile ArtSkin { get; set; } = new();
    public HudColorProfile Colors { get; set; } = new();
    public HudContentProfile Content { get; set; } = new();
    public HudMinimapProfile Minimap { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public static M7HudProfile CreateDefault()
    {
        M7HudProfile profile = new();
        HudFactionSkinLibrary.Apply(profile, 0);
        profile.Normalize();
        return profile;
    }

    public string ToJson()
    {
        Normalize();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static bool TryFromJson(string json, out M7HudProfile profile, out string error)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            M7HudProfile? parsed = JsonSerializer.Deserialize<M7HudProfile>(json, JsonOptions);
            if (parsed is null)
            {
                profile = CreateDefault();
                error = "Clipboard does not contain an M7 HUD profile.";
                return false;
            }
            if (parsed.SchemaVersion < 1 || parsed.SchemaVersion > CurrentSchemaVersion)
            {
                profile = CreateDefault();
                error = $"HUD schema {parsed.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.";
                return false;
            }
            int sourceSchemaVersion = parsed.SchemaVersion;
            // Schema 1-4 profiles were authored against the generated frame
            // renderer. Schema 5 introduced Structural/Legacy/Clean but still
            // used the navy-biased baseline. Preserve both results exactly;
            // schema 6 introduced Hybrid + broad palettes and schema 7 changed
            // its composition. Schema 8 restores the four actual playable
            // factions and makes frame + surface one recipe. Explicit Custom
            // colours remain custom; named pre-schema-8 experiments become the
            // corresponding faction recipe in non-legacy finishes.
            parsed.ArtSkin ??= new HudArtSkinProfile();
            if (sourceSchemaVersion <= 4)
            {
                parsed.ArtSkin.Finish = HudArtFinish.LegacyFrames;
                parsed.ArtSkin.SurfacePalette = HudSurfacePalette.Custom;
            }
            else if (sourceSchemaVersion == 5)
            {
                if (!HasNestedProperty(document.RootElement, "artSkin", "finish"))
                    parsed.ArtSkin.Finish = HudArtFinish.StructuralConsole;
                parsed.ArtSkin.SurfacePalette = HudSurfacePalette.Custom;
            }
            if (sourceSchemaVersion <= 7)
            {
                parsed.ArtSkin.Faction = parsed.ArtSkin.Faction switch
                {
                    3 => 1, // retired Life on Mars astronaut sub-skin -> shared Astronaut HUD
                    4 => 3, // old fifth slot -> canonical Martian slot
                    _ => parsed.ArtSkin.Faction
                };
                if (sourceSchemaVersion >= 6 &&
                    parsed.ArtSkin.Finish != HudArtFinish.LegacyFrames &&
                    parsed.ArtSkin.SurfacePalette != HudSurfacePalette.Custom)
                    parsed.ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound;
            }
            if (sourceSchemaVersion <= 5)
                MergeMissingLegacyColorDefaults(parsed, document.RootElement);
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

    private static void MergeMissingLegacyColorDefaults(M7HudProfile profile, JsonElement root)
    {
        profile.Colors ??= new HudColorProfile();
        bool hasColors = root.TryGetProperty("colors", out JsonElement colors) &&
            colors.ValueKind == JsonValueKind.Object;
        if (!hasColors || !colors.TryGetProperty("background", out _)) profile.Colors.Background = "#111820";
        if (!hasColors || !colors.TryGetProperty("raised", out _)) profile.Colors.Raised = "#1b2731";
        if (!hasColors || !colors.TryGetProperty("recessed", out _)) profile.Colors.Recessed = "#0b1016";
        if (!hasColors || !colors.TryGetProperty("accent", out _)) profile.Colors.Accent = "#e6ad28";
        if (!hasColors || !colors.TryGetProperty("textPrimary", out _)) profile.Colors.TextPrimary = "#f2eee3";
        if (!hasColors || !colors.TryGetProperty("textMuted", out _)) profile.Colors.TextMuted = "#aab4b8";
        if (!hasColors || !colors.TryGetProperty("good", out _)) profile.Colors.Good = "#65c987";
        if (!hasColors || !colors.TryGetProperty("warning", out _)) profile.Colors.Warning = "#f2b84b";
        if (!hasColors || !colors.TryGetProperty("danger", out _)) profile.Colors.Danger = "#ff6b45";
        if (!hasColors || !colors.TryGetProperty("selection", out _)) profile.Colors.Selection = "#5fc4d8";
    }

    private static bool HasNestedProperty(JsonElement root, string parent, string child) =>
        root.TryGetProperty(parent, out JsonElement element) &&
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(child, out _);

    public M7HudProfile Clone()
    {
        if (!TryFromJson(ToJson(), out M7HudProfile clone, out string error)) throw new InvalidOperationException(error);
        return clone;
    }

    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        Layout ??= new HudLayoutProfile();
        Typography ??= new HudTypographyProfile();
        Surface ??= new HudSurfaceProfile();
        ArtSkin ??= new HudArtSkinProfile();
        Colors ??= new HudColorProfile();
        Content ??= new HudContentProfile();
        Minimap ??= new HudMinimapProfile();
        Layout.SafeAreaPercent = Clamp(Layout.SafeAreaPercent, 90f, 100f);
        Layout.UiScale = Clamp(Layout.UiScale, 0.8f, 1.35f);
        Layout.TopStripHeight = Clamp(Layout.TopStripHeight, 44f, 78f);
        Layout.BottomRegionHeight = Clamp(Layout.BottomRegionHeight, 172f, 280f);
        Layout.MinimapSize = Clamp(Layout.MinimapSize, 164f, 260f);
        Layout.CommandPanelWidth = Clamp(Layout.CommandPanelWidth, 220f, 380f);
        Layout.SelectionMaxWidth = Clamp(Layout.SelectionMaxWidth, 540f, 1200f);
        Layout.PanelGap = Clamp(Layout.PanelGap, 4f, 24f);
        Typography.TextScale = Clamp(Typography.TextScale, 0.8f, 1.4f);
        Typography.HeadingSize = Clamp(Typography.HeadingSize, 12, 26);
        Typography.BodySize = Clamp(Typography.BodySize, 11, 22);
        Typography.MicroSize = Clamp(Typography.MicroSize, 9, 18);
        Surface.PanelOpacity = Clamp(Surface.PanelOpacity, 0.35f, 1f);
        Surface.BorderWidth = Clamp(Surface.BorderWidth, 0, 5);
        Surface.CornerRadius = Clamp(Surface.CornerRadius, 0, 20);
        Surface.InnerPadding = Clamp(Surface.InnerPadding, 4, 24);
        Surface.Separation = Clamp(Surface.Separation, 2, 18);
        ArtSkin.Faction = Clamp(ArtSkin.Faction, 0, HudFactionSkinLibrary.Count - 1);
        // Schema-4 profiles created by the original full-frame experiment are
        // still accepted. Their implementation-specific controls migrate once
        // into the simpler modular chrome language and are omitted on export.
        if (ArtSkin.FrameOpacity.HasValue)
            ArtSkin.ChromeIntensity = ArtSkin.FrameOpacity.Value;
        if (ArtSkin.FrameThickness.HasValue)
            ArtSkin.ChromeScale = ArtSkin.FrameThickness.Value / 22f;
        ArtSkin.FrameOpacity = null;
        ArtSkin.FrameThickness = null;
        if (!Enum.IsDefined(ArtSkin.Finish)) ArtSkin.Finish = HudArtFinish.HybridConsole;
        if (!Enum.IsDefined(ArtSkin.SurfacePalette)) ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound;
        if (ArtSkin.SurfacePalette is not (HudSurfacePalette.Custom or HudSurfacePalette.FactionBound))
            ArtSkin.SurfacePalette = HudSurfacePalette.Custom;
        ArtSkin.ChromeIntensity = Clamp(ArtSkin.ChromeIntensity, 0f, 1f);
        ArtSkin.ChromeScale = Clamp(ArtSkin.ChromeScale, 0.75f, 1.35f);
        Colors.Background = M7ProfileColor.Normalize(Colors.Background, "#cfc6ae");
        Colors.Raised = M7ProfileColor.Normalize(Colors.Raised, "#eee6d0");
        Colors.Recessed = M7ProfileColor.Normalize(Colors.Recessed, "#8b8578");
        Colors.Accent = M7ProfileColor.Normalize(Colors.Accent, "#d95f24");
        Colors.TextPrimary = M7ProfileColor.Normalize(Colors.TextPrimary, "#171a1b");
        Colors.TextMuted = M7ProfileColor.Normalize(Colors.TextMuted, "#4c5355");
        Colors.Good = M7ProfileColor.Normalize(Colors.Good, "#2d6c42");
        Colors.Warning = M7ProfileColor.Normalize(Colors.Warning, "#8c5b00");
        Colors.Danger = M7ProfileColor.Normalize(Colors.Danger, "#ad2b20");
        Colors.Selection = M7ProfileColor.Normalize(Colors.Selection, "#166e7a");
        if (ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound)
            HudFactionSkinLibrary.Apply(this, ArtSkin.Faction);
        Minimap.MarkerScale = Clamp(Minimap.MarkerScale, 0.6f, 2.0f);
        Minimap.InterpolationSeconds = Clamp(Minimap.InterpolationSeconds, 0f, 0.30f);
        Minimap.ExploredFogOpacity = Clamp(Minimap.ExploredFogOpacity, 0.15f, 0.90f);
        Minimap.UnseenFogOpacity = Clamp(Minimap.UnseenFogOpacity, 0.55f, 1f);
        Minimap.RememberedOpacity = Clamp(Minimap.RememberedOpacity, 0.15f, 0.85f);
        Minimap.GridOpacity = Clamp(Minimap.GridOpacity, 0f, 0.30f);
        Minimap.ViewportLineWidth = Clamp(Minimap.ViewportLineWidth, 1f, 5f);
        Minimap.AlertPulseScale = Clamp(Minimap.AlertPulseScale, 0.5f, 2.5f);
        Minimap.GroundColor = M7ProfileColor.Normalize(Minimap.GroundColor, "#35403d");
        Minimap.RoughColor = M7ProfileColor.Normalize(Minimap.RoughColor, "#594735");
        Minimap.BlockedColor = M7ProfileColor.Normalize(Minimap.BlockedColor, "#1b2225");
        Minimap.ExcavatableColor = M7ProfileColor.Normalize(Minimap.ExcavatableColor, "#71512f");
        Minimap.OwnedColor = M7ProfileColor.Normalize(Minimap.OwnedColor, "#e6ad28");
        Minimap.AlliedColor = M7ProfileColor.Normalize(Minimap.AlliedColor, "#65c987");
        Minimap.EnemyColor = M7ProfileColor.Normalize(Minimap.EnemyColor, "#ff6b45");
        Minimap.NeutralColor = M7ProfileColor.Normalize(Minimap.NeutralColor, "#b4bec1");
        Minimap.ResourceColor = M7ProfileColor.Normalize(Minimap.ResourceColor, "#d9f24b");
        Minimap.ViewportColor = M7ProfileColor.Normalize(Minimap.ViewportColor, "#f2eee3");
        Minimap.AlertColor = M7ProfileColor.Normalize(Minimap.AlertColor, "#ff8a4d");
    }

    private static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
}

public sealed class HudLayoutProfile
{
    public float SafeAreaPercent { get; set; } = 98f;
    public float UiScale { get; set; } = 1f;
    public float TopStripHeight { get; set; } = 48f;
    public float BottomRegionHeight { get; set; } = 220f;
    public float MinimapSize { get; set; } = 210f;
    public float CommandPanelWidth { get; set; } = 280f;
    public float SelectionMaxWidth { get; set; } = 1200f;
    public float PanelGap { get; set; } = 6f;
}

public sealed class HudTypographyProfile
{
    public float TextScale { get; set; } = 1f;
    public int HeadingSize { get; set; } = 16;
    public int BodySize { get; set; } = 14;
    public int MicroSize { get; set; } = 11;
}

public sealed class HudSurfaceProfile
{
    public float PanelOpacity { get; set; } = 0.92f;
    public int BorderWidth { get; set; } = 1;
    public int CornerRadius { get; set; } = 2;
    public int InnerPadding { get; set; } = 8;
    public int Separation { get; set; } = 5;
    public bool SolidCommandButtons { get; set; } = true;
    public bool HealthStateColors { get; set; } = true;
}

public sealed class HudArtSkinProfile
{
    public bool Enabled { get; set; } = true;
    public int Faction { get; set; }
    public HudArtFinish Finish { get; set; } = HudArtFinish.HybridConsole;
    public HudSurfacePalette SurfacePalette { get; set; } = HudSurfacePalette.FactionBound;
    public float ChromeIntensity { get; set; } = 0.88f;
    public float ChromeScale { get; set; } = 1f;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public float? FrameOpacity { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? FrameThickness { get; set; }
}

public enum HudArtFinish : byte
{
    StructuralConsole = 0,
    LegacyFrames = 1,
    Clean = 2,
    HybridConsole = 3
}

public enum HudSurfacePalette : byte
{
    Custom = 0,
    LightCeramic = 1,
    WarmSandstone = 2,
    OxideWorkshop = 3,
    FieldOlive = 4,
    AlienPorcelain = 5,
    NeutralGraphite = 6,
    FactionBound = 7
}

public sealed class HudColorProfile
{
    public string Background { get; set; } = "#303534";
    public string Raised { get; set; } = "#545b58";
    public string Recessed { get; set; } = "#171b1a";
    public string Accent { get; set; } = "#a76538";
    public string TextPrimary { get; set; } = "#f1eadc";
    public string TextMuted { get; set; } = "#b7b1a4";
    public string Good { get; set; } = "#65c987";
    public string Warning { get; set; } = "#f2b84b";
    public string Danger { get; set; } = "#ff6b45";
    public string Selection { get; set; } = "#20a69d";
}

public static class HudSurfacePaletteLibrary
{
    public static void Apply(M7HudProfile profile, HudSurfacePalette palette)
    {
        profile.ArtSkin ??= new HudArtSkinProfile();
        profile.Colors ??= new HudColorProfile();
        if (palette == HudSurfacePalette.FactionBound)
        {
            HudFactionSkinLibrary.Apply(profile, profile.ArtSkin.Faction);
            return;
        }
        profile.ArtSkin.SurfacePalette = HudSurfacePalette.Custom;
        if (palette == HudSurfacePalette.Custom) return;

        HudPaletteTokens tokens = palette switch
        {
            HudSurfacePalette.WarmSandstone => new(
                "#5d4935", "#81694e", "#2e251e", "#e2a33f", "#fff0d8", "#c8ad87",
                "#78b975", "#efb84f", "#ff7154", "#43b6ad"),
            HudSurfacePalette.OxideWorkshop => new(
                "#44291f", "#75432e", "#19110e", "#ef8a37", "#f8e6d9", "#c59a83",
                "#86c879", "#ffc052", "#ff6548", "#72b8c2"),
            HudSurfacePalette.FieldOlive => new(
                "#3b422f", "#687351", "#171c14", "#e4bd4d", "#f1f0dc", "#b6b99c",
                "#82c56d", "#e6b54c", "#f06c4f", "#8eb6a8"),
            HudSurfacePalette.AlienPorcelain => new(
                "#3a2b43", "#6c5079", "#17111c", "#a8e63e", "#f4ecf6", "#bba9c3",
                "#91dd57", "#f3b84f", "#ff5f6c", "#72b8e8"),
            HudSurfacePalette.NeutralGraphite => new(
                "#353739", "#575b5d", "#181a1c", "#cf8241", "#f1eee8", "#aeb2b3",
                "#75bb82", "#e3b44e", "#ef6b50", "#6eb3ba"),
            _ => new(
                "#cfc6ae", "#eee6d0", "#8b8578", "#d95f24", "#171a1b", "#4c5355",
                "#2d6c42", "#8c5b00", "#ad2b20", "#166e7a")
        };
        profile.Colors.Background = tokens.Background;
        profile.Colors.Raised = tokens.Raised;
        profile.Colors.Recessed = tokens.Recessed;
        profile.Colors.Accent = tokens.Accent;
        profile.Colors.TextPrimary = tokens.TextPrimary;
        profile.Colors.TextMuted = tokens.TextMuted;
        profile.Colors.Good = tokens.Good;
        profile.Colors.Warning = tokens.Warning;
        profile.Colors.Danger = tokens.Danger;
        profile.Colors.Selection = tokens.Selection;
    }

    private readonly record struct HudPaletteTokens(
        string Background, string Raised, string Recessed, string Accent,
        string TextPrimary, string TextMuted, string Good, string Warning,
        string Danger, string Selection);
}

public sealed class HudContentProfile
{
    public bool ShowResourceLabels { get; set; }
    public bool ShowHotkeys { get; set; } = true;
    public bool ShowPortrait { get; set; } = true;
    public bool ShowEventFeed { get; set; } = true;
    public bool ShowObjectiveTracker { get; set; } = true;
    public bool ShowMinimapLegend { get; set; } = true;
    // A classic RTS command card keeps the grid icon-first; full name,
    // description and cost remain available in the hover tooltip. The lab can
    // still enable inline costs for density comparison.
    public bool ShowCommandCosts { get; set; }
}

public sealed class HudMinimapProfile
{
    public float MarkerScale { get; set; } = 1f;
    public float InterpolationSeconds { get; set; } = 0.10f;
    public float ExploredFogOpacity { get; set; } = 0.56f;
    public float UnseenFogOpacity { get; set; } = 0.92f;
    public float RememberedOpacity { get; set; } = 0.48f;
    public float GridOpacity { get; set; } = 0.08f;
    public float ViewportLineWidth { get; set; } = 1.5f;
    public float AlertPulseScale { get; set; } = 1f;
    public bool ShowViewport { get; set; } = true;
    public bool ShowAlerts { get; set; } = true;
    public bool ShowNetworkLines { get; set; } = true;
    public string GroundColor { get; set; } = "#35403d";
    public string RoughColor { get; set; } = "#594735";
    public string BlockedColor { get; set; } = "#1b2225";
    public string ExcavatableColor { get; set; } = "#71512f";
    public string OwnedColor { get; set; } = "#e6ad28";
    public string AlliedColor { get; set; } = "#65c987";
    public string EnemyColor { get; set; } = "#ff6b45";
    public string NeutralColor { get; set; } = "#b4bec1";
    public string ResourceColor { get; set; } = "#d9f24b";
    public string ViewportColor { get; set; } = "#f2eee3";
    public string AlertColor { get; set; } = "#ff8a4d";
}
