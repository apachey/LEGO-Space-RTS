using Godot;

namespace LegoSpaceRTS.UI;

public enum HudRasterSurfaceRole : byte
{
    BottomDeck
}

/// <summary>
/// Places one continuous, aspect-correct raster material field across the
/// complete bottom command deck. The source is cropped, never stretched or
/// tiled. Raster vents, fasteners and seams therefore provide authored detail
/// without competing selection/portrait/command material systems.
/// </summary>
public partial class HudRasterSurfaceOverlay : Control
{
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
    public bool UsesSingleDeckWideSurface => _role == HudRasterSurfaceRole.BottomDeck;

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

        float insetX = Mathf.Clamp(Size.X * 0.006f, 7f, 14f);
        float insetY = Mathf.Clamp(Size.Y * 0.045f, 7f, 12f);
        Rect2 area = new(new Vector2(insetX, insetY), Size - new Vector2(insetX * 2f, insetY * 2f));
        if (area.Size.X < 8f || area.Size.Y < 8f) return;

        Rect2 authoredSource = new(0f, 0f, _surface.GetWidth(), _surface.GetHeight());
        Rect2 source = CropToAspect(authoredSource, area.Size.X / area.Size.Y);
        float luminance = _bayColor.R * 0.2126f + _bayColor.G * 0.7152f + _bayColor.B * 0.0722f;
        float rasterAlpha = Mathf.Lerp(0.28f, 0.16f, Mathf.Clamp(luminance, 0f, 1f)) * _intensity;
        DrawTextureRectRegion(_surface, area, source, new Color(1f, 1f, 1f, rasterAlpha));

        // A very quiet palette tint integrates the neutral raster with the
        // selected surface family without drawing another visible rectangle.
        DrawRect(area, new Color(_accentColor, 0.014f * _intensity));
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
