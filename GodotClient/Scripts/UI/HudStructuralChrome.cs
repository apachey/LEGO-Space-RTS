using Godot;

namespace LegoSpaceRTS.UI;

/// <summary>
/// Draws an original continuous RTS command-console chassis. This is a
/// structural alternative to <see cref="HudFactionChrome"/>, not a replacement
/// for its generated faction frames: the two controls can be compared in the
/// M7 laboratory without either one mutating the other.
/// </summary>
public partial class HudStructuralChrome : Control
{
    public const string ConsoleSurfaceTexturePath = "res://Assets/M7/Hud/console_surface_v1.png";

    private const int MaxJunctions = 8;
    private const int MaxSurfaceTiles = 96;

    private Texture2D? _consoleSurface;
    private int _faction;
    private float _intensity = 0.82f;
    private float _chromeScale = 1f;
    private float _outerExpansion;
    private bool _isConfigured;
    private float[] _junctions = { 0.22f, 0.78f };

    public HudFactionChromeRole Role { get; private set; }
    public bool IsConfigured => _isConfigured;
    public bool UsesTiledConsoleSurface => _consoleSurface is not null;
    public bool UsesSculptedShoulders => true;

    public HudStructuralChrome()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;
    }

    public void Configure(int faction, HudFactionChromeRole role, float intensity, float chromeScale, float outerExpansion)
    {
        _faction = Math.Clamp(faction, 0, 4);
        Role = role;
        _intensity = Mathf.Clamp(intensity, 0f, 1f);
        _chromeScale = Mathf.Clamp(chromeScale, 0.75f, 1.40f);
        _outerExpansion = Mathf.Clamp(outerExpansion, 0f, 32f);
        _consoleSurface = ResourceLoader.Exists(ConsoleSurfaceTexturePath)
            ? GD.Load<Texture2D>(ConsoleSurfaceTexturePath)
            : null;
        _isConfigured = true;
        Visible = _intensity > 0.001f;
        QueueRedraw();
    }

    /// <summary>
    /// Places structural dividers at normalized horizontal positions. Invalid
    /// values are ignored, duplicates are collapsed, and the list is bounded
    /// so a malformed laboratory payload cannot create unbounded draw work.
    /// </summary>
    public void SetJunctions(params float[] normalizedXs)
    {
        if (normalizedXs is null || normalizedXs.Length == 0)
        {
            _junctions = Array.Empty<float>();
            QueueRedraw();
            return;
        }

        List<float> accepted = new(Math.Min(MaxJunctions, normalizedXs.Length));
        for (int i = 0; i < normalizedXs.Length && accepted.Count < MaxJunctions; i++)
        {
            float value = normalizedXs[i];
            if (!float.IsFinite(value)) continue;
            accepted.Add(Mathf.Clamp(value, 0.08f, 0.92f));
        }
        accepted.Sort();

        List<float> unique = new(accepted.Count);
        for (int i = 0; i < accepted.Count; i++)
        {
            if (unique.Count == 0 || Mathf.Abs(unique[^1] - accepted[i]) >= 0.018f)
                unique.Add(accepted[i]);
        }
        _junctions = unique.ToArray();
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_isConfigured || _intensity <= 0.001f) return;

        Rect2 outer = new(
            new Vector2(-_outerExpansion, -_outerExpansion),
            Size + new Vector2(_outerExpansion * 2f, _outerExpansion * 2f));
        if (outer.Size.X < 32f || outer.Size.Y < 18f) return;

        bool top = Role == HudFactionChromeRole.TopStrip;
        float shoulder = Mathf.Clamp((top ? 34f : 76f) * _chromeScale, 12f, outer.Size.X * 0.18f);
        float topCut = Mathf.Min((top ? 8f : 20f) * _chromeScale, outer.Size.Y * 0.34f);
        float bottomCut = Mathf.Min((top ? 7f : 12f) * _chromeScale, outer.Size.Y * 0.28f);
        float rim = Mathf.Clamp((top ? 3.5f : 6f) * _chromeScale, 2f, outer.Size.Y * 0.14f);

        Color surface = HudFactionChrome.SurfaceForFaction(_faction);
        Color accent = HudFactionChrome.AccentForFaction(_faction);
        Color secondary = HudFactionChrome.SecondaryAccentForFaction(_faction);
        Color shadow = new Color(0.012f, 0.018f, 0.022f, 0.96f * _intensity);
        Color chassis = WithAlpha(surface.Lerp(new Color("536068"), 0.24f), 0.97f * _intensity);
        Color upperBevel = WithAlpha(chassis.Lerp(Colors.White, 0.22f), 0.82f * _intensity);
        Color lowerBevel = WithAlpha(Colors.Black, 0.82f * _intensity);

        Vector2[] outerShape = CreateChassisShape(outer, shoulder, topCut, bottomCut);
        DrawColoredPolygon(outerShape, shadow);
        DrawClosed(outerShape, WithAlpha(Colors.Black, 0.92f * _intensity), 3f * _chromeScale);

        Rect2 shellRect = outer.Grow(-Mathf.Max(1.5f, rim * 0.42f));
        Vector2[] shellShape = CreateChassisShape(
            shellRect,
            Mathf.Max(6f, shoulder - rim * 0.55f),
            Mathf.Max(2f, topCut - rim * 0.30f),
            Mathf.Max(2f, bottomCut - rim * 0.30f));
        DrawColoredPolygon(shellShape, chassis);
        DrawClosed(shellShape, upperBevel, Mathf.Max(1f, 1.5f * _chromeScale));

        Rect2 consoleArea = new(
            new Vector2(outer.Position.X + shoulder * 0.68f, outer.Position.Y + rim * 1.45f),
            new Vector2(
                Mathf.Max(1f, outer.Size.X - shoulder * 1.36f),
                Mathf.Max(1f, outer.Size.Y - rim * 2.9f)));
        DrawRecessedBays(consoleArea, accent, secondary, rim);
        DrawTiledConsoleSurface(consoleArea);
        DrawShoulders(outer, shoulder, topCut, bottomCut, accent, secondary, rim);
        DrawLayeredRails(outer, shoulder, rim, upperBevel, lowerBevel, accent, secondary);
        DrawJunctions(outer, consoleArea, accent, secondary, rim);
    }

    private void DrawRecessedBays(Rect2 area, Color accent, Color secondary, float rim)
    {
        if (area.Size.X < 8f || area.Size.Y < 5f) return;

        List<float> edges = new(_junctions.Length + 2) { area.Position.X };
        for (int i = 0; i < _junctions.Length; i++)
        {
            float x = Mathf.Lerp(area.Position.X, area.End.X, _junctions[i]);
            if (x > area.Position.X + rim * 2f && x < area.End.X - rim * 2f) edges.Add(x);
        }
        edges.Add(area.End.X);
        edges.Sort();

        float gap = Mathf.Max(2f, rim * 0.72f);
        float chamfer = Mathf.Min(8f * _chromeScale, area.Size.Y * 0.22f);
        for (int i = 0; i < edges.Count - 1; i++)
        {
            float left = edges[i] + gap;
            float right = edges[i + 1] - gap;
            if (right - left < 8f) continue;

            Rect2 bay = new(new Vector2(left, area.Position.Y), new Vector2(right - left, area.Size.Y));
            Vector2[] shape = CreatePanelShape(bay, chamfer);
            DrawColoredPolygon(shape, new Color(0.018f, 0.026f, 0.032f, 0.70f * _intensity));
            DrawClosed(shape, WithAlpha(Colors.Black, 0.72f * _intensity), Mathf.Max(1f, rim * 0.45f));

            Color bayGlint = i % 2 == 0 ? secondary : accent;
            DrawLine(
                new Vector2(left + chamfer, area.Position.Y + 1f),
                new Vector2(right - chamfer, area.Position.Y + 1f),
                WithAlpha(bayGlint, 0.18f * _intensity),
                Mathf.Max(1f, _chromeScale), true);
        }
    }

    private void DrawTiledConsoleSurface(Rect2 area)
    {
        if (_consoleSurface is null || area.Size.X < 2f || area.Size.Y < 2f) return;

        float destinationTile = (Role == HudFactionChromeRole.TopStrip ? 112f : 156f) * _chromeScale;
        float sourceTile = Math.Min(420f, Math.Min(_consoleSurface.GetWidth(), _consoleSurface.GetHeight()));
        if (destinationTile < 4f || sourceTile < 4f) return;

        int textureWidth = Math.Max(1, _consoleSurface.GetWidth());
        int textureHeight = Math.Max(1, _consoleSurface.GetHeight());
        int sourceRangeX = Math.Max(1, textureWidth - (int)sourceTile);
        int sourceRangeY = Math.Max(1, textureHeight - (int)sourceTile);
        Color modulate = new(1f, 1f, 1f,
            (Role == HudFactionChromeRole.TopStrip ? 0.34f : 0.42f) * _intensity);

        int tileCount = 0;
        int row = 0;
        for (float y = area.Position.Y; y < area.End.Y - 0.25f && tileCount < MaxSurfaceTiles; y += destinationTile, row++)
        {
            int column = 0;
            for (float x = area.Position.X; x < area.End.X - 0.25f && tileCount < MaxSurfaceTiles; x += destinationTile, column++)
            {
                float width = Mathf.Min(destinationTile, area.End.X - x);
                float height = Mathf.Min(destinationTile, area.End.Y - y);
                float sourceWidth = sourceTile * width / destinationTile;
                float sourceHeight = sourceTile * height / destinationTile;
                float sourceX = (column * 227 + row * 83) % sourceRangeX;
                float sourceY = (row * 193 + column * 61) % sourceRangeY;
                DrawTextureRectRegion(
                    _consoleSurface,
                    new Rect2(x, y, width, height),
                    new Rect2(sourceX, sourceY, sourceWidth, sourceHeight),
                    modulate);
                tileCount++;
            }
        }
    }

    private void DrawShoulders(Rect2 outer, float shoulder, float topCut, float bottomCut,
        Color accent, Color secondary, float rim)
    {
        float inset = Mathf.Max(2f, rim * 0.72f);
        float innerX = outer.Position.X + shoulder;
        float rightInnerX = outer.End.X - shoulder;
        Color plate = new Color(0.025f, 0.034f, 0.040f, 0.94f * _intensity);

        Vector2[] left =
        {
            new(outer.Position.X + inset, outer.Position.Y + topCut),
            new(innerX, outer.Position.Y + inset),
            new(innerX - shoulder * 0.18f, outer.End.Y - inset),
            new(outer.Position.X + inset, outer.End.Y - bottomCut)
        };
        Vector2[] right =
        {
            new(outer.End.X - inset, outer.Position.Y + topCut),
            new(rightInnerX, outer.Position.Y + inset),
            new(rightInnerX + shoulder * 0.18f, outer.End.Y - inset),
            new(outer.End.X - inset, outer.End.Y - bottomCut)
        };
        DrawColoredPolygon(left, plate);
        DrawColoredPolygon(right, plate);
        DrawClosed(left, WithAlpha(secondary, 0.54f * _intensity), Mathf.Max(1f, 1.5f * _chromeScale));
        DrawClosed(right, WithAlpha(accent, 0.54f * _intensity), Mathf.Max(1f, 1.5f * _chromeScale));

        float nodeRadius = Mathf.Clamp((Role == HudFactionChromeRole.TopStrip ? 2.5f : 4f) * _chromeScale, 1.5f, 7f);
        DrawCircle(new Vector2(innerX - shoulder * 0.22f, outer.GetCenter().Y), nodeRadius,
            WithAlpha(secondary, 0.78f * _intensity));
        DrawCircle(new Vector2(rightInnerX + shoulder * 0.22f, outer.GetCenter().Y), nodeRadius,
            WithAlpha(accent, 0.78f * _intensity));
    }

    private void DrawLayeredRails(Rect2 outer, float shoulder, float rim, Color upperBevel, Color lowerBevel,
        Color accent, Color secondary)
    {
        float left = outer.Position.X + shoulder * 0.84f;
        float right = outer.End.X - shoulder * 0.84f;
        if (right <= left) return;

        float topY = outer.Position.Y + rim * 0.62f;
        float bottomY = outer.End.Y - rim * 0.62f;
        float heavy = Mathf.Max(2f, rim * 0.72f);
        float light = Mathf.Max(1f, rim * 0.26f);

        DrawLine(new Vector2(left, topY + heavy * 0.46f), new Vector2(right, topY + heavy * 0.46f), lowerBevel, heavy, true);
        DrawLine(new Vector2(left, topY), new Vector2(right, topY), upperBevel, light, true);
        DrawLine(new Vector2(left, bottomY - heavy * 0.46f), new Vector2(right, bottomY - heavy * 0.46f), lowerBevel, heavy, true);
        DrawLine(new Vector2(left, bottomY), new Vector2(right, bottomY),
            WithAlpha(accent, 0.58f * _intensity), light, true);

        if (Role == HudFactionChromeRole.BottomDeck)
        {
            float underRail = outer.End.Y - rim * 1.65f;
            DrawLine(new Vector2(left + shoulder * 0.12f, underRail), new Vector2(right - shoulder * 0.12f, underRail),
                WithAlpha(secondary, 0.28f * _intensity), Mathf.Max(1f, light * 0.75f), true);
        }
    }

    private void DrawJunctions(Rect2 outer, Rect2 consoleArea, Color accent, Color secondary, float rim)
    {
        float halfWidth = Mathf.Clamp((Role == HudFactionChromeRole.TopStrip ? 6f : 11f) * _chromeScale, 4f, 16f);
        for (int i = 0; i < _junctions.Length; i++)
        {
            float x = Mathf.Lerp(consoleArea.Position.X, consoleArea.End.X, _junctions[i]);
            Color signal = i % 2 == 0 ? secondary : accent;
            Vector2[] divider =
            {
                new(x - halfWidth, outer.Position.Y + rim * 0.45f),
                new(x + halfWidth, outer.Position.Y + rim * 0.45f),
                new(x + halfWidth * 0.62f, outer.GetCenter().Y),
                new(x + halfWidth, outer.End.Y - rim * 0.45f),
                new(x - halfWidth, outer.End.Y - rim * 0.45f),
                new(x - halfWidth * 0.62f, outer.GetCenter().Y)
            };
            DrawColoredPolygon(divider, new Color(0.015f, 0.022f, 0.027f, 0.91f * _intensity));
            DrawClosed(divider, WithAlpha(Colors.Black, 0.92f * _intensity), Mathf.Max(1f, rim * 0.36f));
            DrawLine(new Vector2(x, outer.Position.Y + rim), new Vector2(x, outer.End.Y - rim),
                WithAlpha(signal, 0.74f * _intensity), Mathf.Max(1f, 1.35f * _chromeScale), true);
            DrawCircle(new Vector2(x, outer.GetCenter().Y), Mathf.Max(1.5f, 2.7f * _chromeScale),
                WithAlpha(signal, 0.88f * _intensity));
        }
    }

    private static Vector2[] CreateChassisShape(Rect2 rect, float shoulder, float topCut, float bottomCut)
    {
        shoulder = Mathf.Clamp(shoulder, 2f, rect.Size.X * 0.24f);
        topCut = Mathf.Clamp(topCut, 1f, rect.Size.Y * 0.44f);
        bottomCut = Mathf.Clamp(bottomCut, 1f, rect.Size.Y * 0.44f);
        return new Vector2[]
        {
            new(rect.Position.X + shoulder, rect.Position.Y),
            new(rect.End.X - shoulder, rect.Position.Y),
            new(rect.End.X, rect.Position.Y + topCut),
            new(rect.End.X, rect.End.Y - bottomCut),
            new(rect.End.X - shoulder * 0.56f, rect.End.Y),
            new(rect.Position.X + shoulder * 0.56f, rect.End.Y),
            new(rect.Position.X, rect.End.Y - bottomCut),
            new(rect.Position.X, rect.Position.Y + topCut)
        };
    }

    private static Vector2[] CreatePanelShape(Rect2 rect, float chamfer)
    {
        chamfer = Mathf.Clamp(chamfer, 1f, Math.Min(rect.Size.X, rect.Size.Y) * 0.28f);
        return new Vector2[]
        {
            new(rect.Position.X + chamfer, rect.Position.Y),
            new(rect.End.X - chamfer, rect.Position.Y),
            new(rect.End.X, rect.Position.Y + chamfer),
            new(rect.End.X, rect.End.Y - chamfer),
            new(rect.End.X - chamfer, rect.End.Y),
            new(rect.Position.X + chamfer, rect.End.Y),
            new(rect.Position.X, rect.End.Y - chamfer),
            new(rect.Position.X, rect.Position.Y + chamfer)
        };
    }

    private void DrawClosed(Vector2[] points, Color color, float width)
    {
        Vector2[] closed = new Vector2[points.Length + 1];
        Array.Copy(points, closed, points.Length);
        closed[^1] = points[0];
        DrawPolyline(closed, color, width, true);
    }

    private static Color WithAlpha(Color color, float alpha) =>
        new(color.R, color.G, color.B, Mathf.Clamp(alpha, 0f, 1f));
}
