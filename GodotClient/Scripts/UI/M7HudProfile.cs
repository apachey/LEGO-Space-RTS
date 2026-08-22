using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegoSpaceRTS.UI;

public sealed class M7HudProfile
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public HudLayoutProfile Layout { get; set; } = new();
    public HudTypographyProfile Typography { get; set; } = new();
    public HudSurfaceProfile Surface { get; set; } = new();
    public HudColorProfile Colors { get; set; } = new();
    public HudContentProfile Content { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public static M7HudProfile CreateDefault() => new();

    public string ToJson()
    {
        Normalize();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static bool TryFromJson(string json, out M7HudProfile profile, out string error)
    {
        try
        {
            M7HudProfile? parsed = JsonSerializer.Deserialize<M7HudProfile>(json, JsonOptions);
            if (parsed is null)
            {
                profile = CreateDefault();
                error = "Clipboard does not contain an M7 HUD profile.";
                return false;
            }
            if (parsed.SchemaVersion != CurrentSchemaVersion)
            {
                profile = CreateDefault();
                error = $"HUD schema {parsed.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.";
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
        Colors ??= new HudColorProfile();
        Content ??= new HudContentProfile();
        Layout.SafeAreaPercent = Clamp(Layout.SafeAreaPercent, 90f, 100f);
        Layout.UiScale = Clamp(Layout.UiScale, 0.8f, 1.35f);
        Layout.TopStripHeight = Clamp(Layout.TopStripHeight, 44f, 78f);
        Layout.BottomRegionHeight = Clamp(Layout.BottomRegionHeight, 172f, 280f);
        Layout.MinimapSize = Clamp(Layout.MinimapSize, 164f, 260f);
        Layout.CommandPanelWidth = Clamp(Layout.CommandPanelWidth, 300f, 460f);
        Layout.SelectionMaxWidth = Clamp(Layout.SelectionMaxWidth, 540f, 960f);
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
    }

    private static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
}

public sealed class HudLayoutProfile
{
    public float SafeAreaPercent { get; set; } = 96f;
    public float UiScale { get; set; } = 1f;
    public float TopStripHeight { get; set; } = 54f;
    public float BottomRegionHeight { get; set; } = 214f;
    public float MinimapSize { get; set; } = 206f;
    public float CommandPanelWidth { get; set; } = 372f;
    public float SelectionMaxWidth { get; set; } = 820f;
    public float PanelGap { get; set; } = 10f;
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
    public int CornerRadius { get; set; } = 6;
    public int InnerPadding { get; set; } = 10;
    public int Separation { get; set; } = 7;
    public bool SolidCommandButtons { get; set; } = true;
    public bool HealthStateColors { get; set; } = true;
}

public sealed class HudColorProfile
{
    public string Background { get; set; } = "#111820";
    public string Raised { get; set; } = "#1b2731";
    public string Recessed { get; set; } = "#0b1016";
    public string Accent { get; set; } = "#e6ad28";
    public string TextPrimary { get; set; } = "#f2eee3";
    public string TextMuted { get; set; } = "#aab4b8";
    public string Good { get; set; } = "#65c987";
    public string Warning { get; set; } = "#f2b84b";
    public string Danger { get; set; } = "#ff6b45";
    public string Selection { get; set; } = "#5fc4d8";
}

public sealed class HudContentProfile
{
    public bool ShowResourceLabels { get; set; } = true;
    public bool ShowHotkeys { get; set; } = true;
    public bool ShowPortrait { get; set; } = true;
    public bool ShowEventFeed { get; set; } = true;
    public bool ShowObjectiveTracker { get; set; } = true;
    public bool ShowMinimapLegend { get; set; } = true;
    public bool ShowCommandCosts { get; set; } = true;
}
