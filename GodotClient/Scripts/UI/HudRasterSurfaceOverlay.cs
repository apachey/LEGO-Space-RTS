using Godot;

namespace LegoSpaceRTS.UI;

public enum HudRasterSurfaceRole : byte
{
    Selection,
    Command
}

/// <summary>
/// Places a small number of bounded, uniformly scaled raster plates inside
/// code-native HUD bays. It deliberately does not tile a wallpaper or stretch
/// a complete generated frame: the raster supplies vents, fasteners and plate
/// seams while layout, silhouette, dividers and hit targets remain code-owned.
/// </summary>
public partial class HudRasterSurfaceOverlay : Control
{
    private static readonly Rect2[] SourcePlates =
    {
        new(18f, 18f, 292f, 176f),
        new(332f, 22f, 292f, 176f),
        new(646f, 18f, 292f, 176f),
        new(942f, 314f, 292f, 176f),
        new(176f, 620f, 292f, 176f),
        new(508f, 928f, 292f, 176f)
    };

    private Texture2D? _surface;
    private HudRasterSurfaceRole _role;
    private float _intensity;
    private Color _bayColor = new("8b8578");
    private Color _accentColor = new("d95f24");

    public HudRasterSurfaceRole Role => _role;
    public bool IsConfigured => _surface is not null;
    public bool UsesBoundedRasterPlates => true;
    public bool UsesUnstretchedSourceRegions => true;

    public HudRasterSurfaceOverlay()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
    }

    public void Configure(HudRasterSurfaceRole role, HudArtFinish finish, float intensity,
        Color bayColor, Color accentColor)
    {
        _role = role;
        _intensity = Mathf.Clamp(intensity, 0f, 1f);
        _bayColor = bayColor;
        _accentColor = accentColor;
        _surface = ResourceLoader.Exists(HudStructuralChrome.ConsoleSurfaceTexturePath)
            ? GD.Load<Texture2D>(HudStructuralChrome.ConsoleSurfaceTexturePath)
            : null;
        Visible = finish == HudArtFinish.HybridConsole && _surface is not null && _intensity > 0.001f;
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Visible || _surface is null || Size.X < 48f || Size.Y < 32f) return;

        float inset = Mathf.Clamp(Size.Y * 0.08f, 5f, 14f);
        Rect2 area = new(new Vector2(inset, inset), Size - new Vector2(inset * 2f, inset * 2f));
        int count = _role == HudRasterSurfaceRole.Selection
            ? Math.Clamp(Mathf.FloorToInt(area.Size.X / 330f), 2, 4)
            : 1;
        float gap = Mathf.Clamp(area.Size.X * 0.018f, 8f, 18f);
        float slotWidth = (area.Size.X - gap * (count - 1)) / count;
        float sourceAspect = SourcePlates[0].Size.X / SourcePlates[0].Size.Y;
        float plateHeight = Math.Min(area.Size.Y, slotWidth / sourceAspect);
        float plateWidth = plateHeight * sourceAspect;
        float startX = area.Position.X + Math.Max(0f,
            (area.Size.X - (plateWidth * count + gap * (count - 1))) * 0.5f);
        float y = area.Position.Y + (area.Size.Y - plateHeight) * 0.5f;
        float luminance = _bayColor.R * 0.2126f + _bayColor.G * 0.7152f + _bayColor.B * 0.0722f;
        float rasterAlpha = Mathf.Lerp(0.24f, 0.14f, Mathf.Clamp(luminance, 0f, 1f)) * _intensity;

        for (int i = 0; i < count; i++)
        {
            Rect2 destination = new(new Vector2(startX + i * (plateWidth + gap), y),
                new Vector2(plateWidth, plateHeight));
            DrawRect(destination.Grow(2f), new Color(_bayColor.Darkened(0.36f), 0.32f * _intensity));
            DrawTextureRectRegion(_surface, destination, SourcePlates[(i * 2 + (int)_role) % SourcePlates.Length],
                new Color(1f, 1f, 1f, rasterAlpha));
            DrawRect(destination, new Color(_accentColor, 0.16f * _intensity), false, 1f, true);
        }
    }
}
