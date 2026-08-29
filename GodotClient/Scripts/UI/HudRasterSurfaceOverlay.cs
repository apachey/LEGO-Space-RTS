using Godot;

namespace LegoSpaceRTS.UI;

public enum HudRasterSurfaceRole : byte
{
    Selection,
    Command
}

/// <summary>
/// Places one continuous, aspect-correct raster material field inside a
/// code-native HUD bay. The source is cropped, never stretched, to the current
/// adaptive panel. Raster vents, fasteners and seams therefore add material
/// detail without producing a repeated row of boxed plates.
/// </summary>
public partial class HudRasterSurfaceOverlay : Control
{
    private static readonly Rect2 SelectionSource = new(22f, 314f, 1210f, 382f);
    private static readonly Rect2 CommandSource = new(628f, 250f, 584f, 640f);

    private Texture2D? _surface;
    private HudRasterSurfaceRole _role;
    private float _intensity;
    private Color _bayColor = new("8b8578");
    private Color _accentColor = new("d95f24");

    public HudRasterSurfaceRole Role => _role;
    public bool IsConfigured => _surface is not null;
    public bool UsesBoundedRasterPlates => true;
    public bool UsesUnstretchedSourceRegions => true;
    public bool UsesSingleContinuousSurfaceField => true;

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

        float insetX = Mathf.Clamp(Size.X * 0.012f, 6f, 16f);
        float insetY = Mathf.Clamp(Size.Y * 0.065f, 6f, 13f);
        Rect2 area = new(new Vector2(insetX, insetY), Size - new Vector2(insetX * 2f, insetY * 2f));
        if (area.Size.X < 8f || area.Size.Y < 8f) return;

        Rect2 authoredSource = _role == HudRasterSurfaceRole.Selection ? SelectionSource : CommandSource;
        Rect2 source = CropToAspect(authoredSource, area.Size.X / area.Size.Y);
        float luminance = _bayColor.R * 0.2126f + _bayColor.G * 0.7152f + _bayColor.B * 0.0722f;
        float rasterAlpha = Mathf.Lerp(0.34f, 0.21f, Mathf.Clamp(luminance, 0f, 1f)) * _intensity;
        if (_role == HudRasterSurfaceRole.Command) rasterAlpha *= 0.90f;
        DrawTextureRectRegion(_surface, area, source, new Color(1f, 1f, 1f, rasterAlpha));

        // A very quiet palette tint integrates the neutral raster with the
        // selected surface family without drawing another visible rectangle.
        DrawRect(area, new Color(_accentColor, 0.018f * _intensity));
    }

    private static Rect2 CropToAspect(Rect2 source, float targetAspect)
    {
        targetAspect = Mathf.Max(0.05f, targetAspect);
        float sourceAspect = source.Size.X / source.Size.Y;
        if (sourceAspect > targetAspect)
        {
            float width = source.Size.Y * targetAspect;
            return new Rect2(source.Position + new Vector2((source.Size.X - width) * 0.5f, 0f),
                new Vector2(width, source.Size.Y));
        }

        float height = source.Size.X / targetAspect;
        return new Rect2(source.Position + new Vector2(0f, (source.Size.Y - height) * 0.5f),
            new Vector2(source.Size.X, height));
    }
}
