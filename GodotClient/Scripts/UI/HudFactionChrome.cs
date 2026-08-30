using Godot;

namespace LegoSpaceRTS.UI;

public enum HudFactionChromeRole : byte
{
    TopStrip,
    BottomDeck
}

/// <summary>
/// Draws one faction-owned chassis. Hybrid protects the authored raster
/// corners and joins them with clean vector rails; it never repeats a complete
/// illustrated edge. Legacy deliberately retains the old tiled renderer so the
/// laboratory can compare it without making it the production default.
/// </summary>
public partial class HudFactionChrome : Control
{
    private Texture2D? _texture;
    private HudFactionSkinRecipe _recipe = HudFactionSkinLibrary.RecipeFor(0);
    private float _intensity = 0.88f;
    private float _chromeScale = 1f;
    private float _outerExpansion;
    private bool _frameOnly;
    private float _leftJunction = 0.22f;
    private float _rightJunction = 0.78f;

    public HudFactionChromeRole Role { get; private set; }
    public bool IsConfigured => _texture is not null;
    public bool UsesFixedSquareCorners => true;
    public bool UsesProtectedSourceModules => true;
    public bool UsesSparseJunctionModules => !_frameOnly;
    public bool UsesTiledEdgeWalls => !_frameOnly;
    public bool UsesContinuousHybridRails => _frameOnly;
    public bool UsesFactionSurfaceFill => !_frameOnly;
    public bool IsFrameOnly => _frameOnly;
    public bool UsesVectorAccentRails => true;
    public int ProtectedSourceSize => SourceCornerSize();

    public HudFactionChrome()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
    }

    public void Configure(int faction, HudFactionChromeRole role, float intensity, float chromeScale,
        float outerExpansion, bool frameOnly = false)
    {
        _recipe = HudFactionSkinLibrary.RecipeFor(faction);
        _frameOnly = frameOnly;
        string texturePath = frameOnly ? _recipe.HybridFramePath : _recipe.LegacyFramePath;
        _texture = ResourceLoader.Exists(texturePath) ? GD.Load<Texture2D>(texturePath) : null;
        Role = role;
        _intensity = Mathf.Clamp(intensity, 0f, 1f);
        _chromeScale = Mathf.Clamp(chromeScale, 0.75f, 1.35f);
        _outerExpansion = Mathf.Max(0f, outerExpansion);
        Visible = _texture is not null && _intensity > 0.001f;
        QueueRedraw();
    }

    public void SetJunctions(float leftNormalized, float rightNormalized)
    {
        _leftJunction = Mathf.Clamp(leftNormalized, 0.08f, 0.48f);
        _rightJunction = Mathf.Clamp(rightNormalized, 0.52f, 0.92f);
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized) QueueRedraw();
    }

    public override void _Draw()
    {
        if (_texture is null || _intensity <= 0.001f || Size.X < 8f || Size.Y < 8f) return;

        Rect2 outer = _frameOnly
            ? new Rect2(Vector2.Zero, Size)
            : new Rect2(new Vector2(-_outerExpansion, -_outerExpansion),
                Size + new Vector2(_outerExpansion * 2f, _outerExpansion * 2f));
        float requestedCorner = (Role == HudFactionChromeRole.TopStrip ? 15f : 30f) * _chromeScale;
        float corner = Mathf.Min(requestedCorner, Mathf.Min(outer.Size.X, outer.Size.Y) * 0.45f);
        if (corner < 2f) return;

        Color modulate = new(1f, 1f, 1f, _intensity);
        if (_frameOnly)
        {
            DrawContinuousHybridRails(outer, corner);
            DrawCorners(outer, corner, modulate);
            return;
        }

        DrawLegacySurfaceFill(outer, corner);
        DrawTiledEdgeWalls(outer, corner, modulate);
        DrawCorners(outer, corner, modulate);
        DrawLegacyRails(outer, corner);
        DrawFunctionalModules(outer, corner, modulate);
    }

    public static string TexturePathForFaction(int faction) =>
        HudFactionSkinLibrary.RecipeFor(faction).HybridFramePath;

    public static int ProtectedSourceSizeForFaction(int faction)
    {
        HudFactionSkinRecipe recipe = HudFactionSkinLibrary.RecipeFor(faction);
        return Mathf.RoundToInt(256f * recipe.ProtectedFraction);
    }

    public static Color AccentForFaction(int faction) =>
        HudFactionSkinLibrary.ColorOrFallback(HudFactionSkinLibrary.RecipeFor(faction).Accent, Colors.Orange);

    public static Color SecondaryAccentForFaction(int faction) =>
        HudFactionSkinLibrary.ColorOrFallback(HudFactionSkinLibrary.RecipeFor(faction).Secondary, Colors.Cyan);

    public static Color SurfaceForFaction(int faction) =>
        HudFactionSkinLibrary.ColorOrFallback(HudFactionSkinLibrary.RecipeFor(faction).Background, Colors.Black);

    public static bool ValidateRecipes(out string error)
    {
        if (!HudFactionSkinLibrary.Validate(out error)) return false;
        for (int i = 0; i < HudFactionSkinLibrary.Count; i++)
        {
            HudFactionSkinRecipe recipe = HudFactionSkinLibrary.RecipeFor(i);
            Texture2D? hybrid = GD.Load<Texture2D>(recipe.HybridFramePath);
            if (hybrid is null || hybrid.GetWidth() < 128 || hybrid.GetHeight() < 128)
            {
                error = $"Faction HUD frame has unsafe dimensions: {recipe.HybridFramePath}";
                return false;
            }
            int protectedSize = Mathf.RoundToInt(Math.Min(hybrid.GetWidth(), hybrid.GetHeight()) * recipe.ProtectedFraction);
            if (protectedSize < 24 || Math.Min(hybrid.GetWidth(), hybrid.GetHeight()) - protectedSize * 2 < 48)
            {
                error = $"Faction HUD frame has unsafe protected guides: {recipe.Name}";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private int SourceCornerSize()
    {
        if (_texture is null) return 0;
        int shortest = Math.Min(_texture.GetWidth(), _texture.GetHeight());
        return Mathf.Clamp(Mathf.RoundToInt(shortest * _recipe.ProtectedFraction), 1, shortest / 2);
    }

    private void DrawCorners(Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null) return;
        float source = SourceCornerSize();
        float sourceRight = _texture.GetWidth() - source;
        float sourceBottom = _texture.GetHeight() - source;
        float destinationRight = outer.End.X - corner;
        float destinationBottom = outer.End.Y - corner;

        DrawTextureRectRegion(_texture, new Rect2(outer.Position, new Vector2(corner, corner)),
            new Rect2(0f, 0f, source, source), modulate);
        DrawTextureRectRegion(_texture, new Rect2(new Vector2(destinationRight, outer.Position.Y), new Vector2(corner, corner)),
            new Rect2(sourceRight, 0f, source, source), modulate);
        DrawTextureRectRegion(_texture, new Rect2(new Vector2(outer.Position.X, destinationBottom), new Vector2(corner, corner)),
            new Rect2(0f, sourceBottom, source, source), modulate);
        DrawTextureRectRegion(_texture, new Rect2(new Vector2(destinationRight, destinationBottom), new Vector2(corner, corner)),
            new Rect2(sourceRight, sourceBottom, source, source), modulate);
    }

    private void DrawContinuousHybridRails(Rect2 outer, float corner)
    {
        Color shell = ColorFor(_recipe.Raised, Colors.Gray);
        Color recess = ColorFor(_recipe.Recessed, Colors.Black);
        Color accent = ColorFor(_recipe.Accent, Colors.Orange);
        Color secondary = ColorFor(_recipe.Secondary, Colors.Cyan);
        bool top = Role == HudFactionChromeRole.TopStrip;
        float rail = Mathf.Clamp((top ? 4.5f : 9f) * _chromeScale, 3f, outer.Size.Y * 0.16f);
        float side = Mathf.Clamp((top ? 5f : 10f) * _chromeScale, 3f, outer.Size.X * 0.05f);
        Color edge = WithAlpha(recess.Darkened(0.34f), 0.96f);
        Color plate = WithAlpha(shell, top ? 0.92f : 0.96f);

        DrawRect(new Rect2(outer.Position, new Vector2(outer.Size.X, rail)), edge);
        DrawRect(new Rect2(new Vector2(outer.Position.X, outer.End.Y - rail), new Vector2(outer.Size.X, rail)), edge);
        DrawRect(new Rect2(outer.Position, new Vector2(side, outer.Size.Y)), edge);
        DrawRect(new Rect2(new Vector2(outer.End.X - side, outer.Position.Y), new Vector2(side, outer.Size.Y)), edge);

        float inner = Mathf.Max(1f, rail * 0.35f);
        DrawRect(new Rect2(new Vector2(outer.Position.X + corner * 0.60f, outer.Position.Y + 1f),
            new Vector2(Mathf.Max(0f, outer.Size.X - corner * 1.20f), inner)), plate);
        DrawRect(new Rect2(new Vector2(outer.Position.X + corner * 0.60f, outer.End.Y - inner - 1f),
            new Vector2(Mathf.Max(0f, outer.Size.X - corner * 1.20f), inner)), plate.Darkened(0.22f));

        float signal = Mathf.Max(1f, (top ? 1.1f : 1.7f) * _chromeScale);
        DrawFactionSignals(outer, corner, rail, signal, accent, secondary);
    }

    private void DrawFactionSignals(Rect2 outer, float corner, float rail, float width,
        Color accent, Color secondary)
    {
        float left = outer.Position.X + corner * 0.82f;
        float right = outer.End.X - corner * 0.82f;
        if (right <= left) return;
        float topY = outer.Position.Y + Mathf.Min(rail - 1f, rail * 0.58f);
        float bottomY = outer.End.Y - Mathf.Min(rail - 1f, rail * 0.48f);

        switch (_recipe.RailStyle)
        {
            case HudFactionRailStyle.Industrial:
                DrawLine(new Vector2(left, topY), new Vector2(right, topY), WithAlpha(secondary, 0.58f), width, true);
                DrawSegmentMarkers(left, right, bottomY, accent, 0.12f, width * 1.25f);
                break;
            case HudFactionRailStyle.Expedition:
                DrawLine(new Vector2(left, topY), new Vector2(right, topY), WithAlpha(secondary, 0.46f), width, true);
                DrawSegmentMarkers(left, right, bottomY, accent, 0.18f, width * 1.35f);
                break;
            case HudFactionRailStyle.Resonance:
                DrawLine(new Vector2(left, topY), new Vector2(right, topY), WithAlpha(accent, 0.70f), width * 1.25f, true);
                DrawSegmentMarkers(left, right, bottomY, secondary, 0.09f, width);
                break;
            case HudFactionRailStyle.Pneumatic:
                DrawLine(new Vector2(left, topY), new Vector2(right, topY), WithAlpha(secondary, 0.55f), width, true);
                DrawLine(new Vector2(left, bottomY), new Vector2(right, bottomY), WithAlpha(accent, 0.44f), width, true);
                break;
        }
    }

    private void DrawSegmentMarkers(float left, float right, float y, Color color, float fraction, float width)
    {
        float length = right - left;
        float marker = Mathf.Clamp(length * fraction, 12f, 72f * _chromeScale);
        DrawLine(new Vector2(left, y), new Vector2(left + marker, y), WithAlpha(color, 0.68f), width, true);
        DrawLine(new Vector2(right - marker, y), new Vector2(right, y), WithAlpha(color, 0.68f), width, true);
    }

    private void DrawLegacySurfaceFill(Rect2 outer, float corner)
    {
        Color surface = SurfaceForFaction((int)_recipe.Faction);
        Color secondary = SecondaryAccentForFaction((int)_recipe.Faction);
        float fillAlpha = (Role == HudFactionChromeRole.TopStrip ? 0.16f : 0.10f) * _intensity;
        Rect2 inner = outer.Grow(-Mathf.Max(3f, corner * 0.42f));
        if (inner.Size.X <= 1f || inner.Size.Y <= 1f) return;
        DrawRect(inner, new Color(surface, fillAlpha));
        float bandHeight = Mathf.Clamp(inner.Size.Y * 0.18f, 3f, 13f * _chromeScale);
        DrawRect(new Rect2(inner.Position, new Vector2(inner.Size.X, bandHeight)),
            new Color(secondary, fillAlpha * 0.62f));
    }

    private void DrawTiledEdgeWalls(Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null) return;
        float sourceCorner = SourceCornerSize();
        float sourceSpanX = _texture.GetWidth() - sourceCorner * 2f;
        float sourceSpanY = _texture.GetHeight() - sourceCorner * 2f;
        if (sourceSpanX < 4f || sourceSpanY < 4f) return;
        float horizontalLength = Mathf.Max(0f, outer.Size.X - corner * 2f);
        float verticalLength = Mathf.Max(0f, outer.Size.Y - corner * 2f);

        if (horizontalLength > 0.5f)
        {
            DrawHorizontalTiles(new Vector2(outer.Position.X + corner, outer.Position.Y), horizontalLength, corner,
                new Rect2(sourceCorner, 0f, sourceSpanX, sourceCorner), modulate);
            DrawHorizontalTiles(new Vector2(outer.Position.X + corner, outer.End.Y - corner), horizontalLength, corner,
                new Rect2(sourceCorner, _texture.GetHeight() - sourceCorner, sourceSpanX, sourceCorner), modulate);
        }
        if (verticalLength > 0.5f)
        {
            DrawVerticalTiles(new Vector2(outer.Position.X, outer.Position.Y + corner), verticalLength, corner,
                new Rect2(0f, sourceCorner, sourceCorner, sourceSpanY), modulate);
            DrawVerticalTiles(new Vector2(outer.End.X - corner, outer.Position.Y + corner), verticalLength, corner,
                new Rect2(_texture.GetWidth() - sourceCorner, sourceCorner, sourceCorner, sourceSpanY), modulate);
        }
    }

    private void DrawHorizontalTiles(Vector2 start, float length, float thickness, Rect2 source, Color modulate)
    {
        float scale = thickness / source.Size.Y;
        float tileWidth = source.Size.X * scale;
        if (tileWidth <= 0.5f) return;
        for (float drawn = 0f; drawn < length - 0.25f;)
        {
            float destinationWidth = Mathf.Min(tileWidth, length - drawn);
            DrawTextureRectRegion(_texture!, new Rect2(start + new Vector2(drawn, 0f), new Vector2(destinationWidth, thickness)),
                new Rect2(source.Position, new Vector2(destinationWidth / scale, source.Size.Y)), modulate);
            drawn += destinationWidth;
        }
    }

    private void DrawVerticalTiles(Vector2 start, float length, float thickness, Rect2 source, Color modulate)
    {
        float scale = thickness / source.Size.X;
        float tileHeight = source.Size.Y * scale;
        if (tileHeight <= 0.5f) return;
        for (float drawn = 0f; drawn < length - 0.25f;)
        {
            float destinationHeight = Mathf.Min(tileHeight, length - drawn);
            DrawTextureRectRegion(_texture!, new Rect2(start + new Vector2(0f, drawn), new Vector2(thickness, destinationHeight)),
                new Rect2(source.Position, new Vector2(source.Size.X, destinationHeight / scale)), modulate);
            drawn += destinationHeight;
        }
    }

    private void DrawLegacyRails(Rect2 outer, float corner)
    {
        Color accent = WithAlpha(AccentForFaction((int)_recipe.Faction), 0.72f);
        Color secondary = WithAlpha(SecondaryAccentForFaction((int)_recipe.Faction), 0.42f);
        float line = Role == HudFactionChromeRole.TopStrip ? 1.5f : 2f;
        float inset = Role == HudFactionChromeRole.TopStrip ? 2f : 3f;
        DrawLine(new Vector2(outer.Position.X + corner, outer.Position.Y + inset), new Vector2(outer.End.X - corner, outer.Position.Y + inset), accent, line, true);
        DrawLine(new Vector2(outer.Position.X + corner, outer.End.Y - inset), new Vector2(outer.End.X - corner, outer.End.Y - inset), accent, line, true);
        DrawLine(new Vector2(outer.Position.X + inset, outer.Position.Y + corner), new Vector2(outer.Position.X + inset, outer.End.Y - corner), secondary, line, true);
        DrawLine(new Vector2(outer.End.X - inset, outer.Position.Y + corner), new Vector2(outer.End.X - inset, outer.End.Y - corner), secondary, line, true);
    }

    private void DrawFunctionalModules(Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null) return;
        float source = SourceCornerSize();
        float sourceX = (_texture.GetWidth() - source) * 0.5f;
        Rect2 topSource = new(sourceX, 0f, source, source);
        Rect2 bottomSource = new(sourceX, _texture.GetHeight() - source, source, source);
        float module = Mathf.Min(corner, Role == HudFactionChromeRole.TopStrip ? 14f * _chromeScale : 26f * _chromeScale);
        if (Role == HudFactionChromeRole.TopStrip)
        {
            DrawModule(outer, 0.5f, module, topSource, bottomSource, modulate);
            return;
        }
        DrawModule(outer, _leftJunction, module, topSource, bottomSource, modulate);
        DrawModule(outer, _rightJunction, module, topSource, bottomSource, modulate);
    }

    private void DrawModule(Rect2 outer, float normalizedX, float size, Rect2 topSource, Rect2 bottomSource, Color modulate)
    {
        float x = Mathf.Lerp(outer.Position.X, outer.End.X, normalizedX) - size * 0.5f;
        x = Mathf.Clamp(x, outer.Position.X + size, outer.End.X - size * 2f);
        DrawTextureRectRegion(_texture!, new Rect2(x, outer.Position.Y, size, size), topSource, modulate);
        DrawTextureRectRegion(_texture!, new Rect2(x, outer.End.Y - size, size, size), bottomSource, modulate);
    }

    private Color ColorFor(string html, Color fallback) =>
        WithAlpha(HudFactionSkinLibrary.ColorOrFallback(html, fallback), 1f);

    private Color WithAlpha(Color color, float alpha) =>
        new(color.R, color.G, color.B, Mathf.Clamp(alpha * _intensity, 0f, 1f));
}
