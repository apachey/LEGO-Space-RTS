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
    public HudFaction Faction => _recipe.Faction;
    public bool IsConfigured => _texture is not null;
    public bool UsesFixedSquareCorners => true;
    public bool UsesProtectedSourceModules => true;
    public bool UsesSparseJunctionModules => !_frameOnly;
    public bool UsesTiledEdgeWalls => !_frameOnly;
    public bool UsesContinuousHybridRails => _frameOnly;
    public bool UsesCompleteHybridPerimeter => _frameOnly;
    public bool UsesIsotropicRasterModules => _frameOnly;
    public bool AvoidsFullSpanRasterStretch => _frameOnly;
    public bool SupportsCompleteHybridPerimeter => true;
    public bool SupportsIsotropicRasterModules => true;
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
        float corner = DestinationCornerSize(_recipe, Role, outer.Size, _chromeScale);
        if (corner < 2f) return;

        Color modulate = new(1f, 1f, 1f, _intensity);
        if (_frameOnly)
        {
            DrawContinuousHybridRails(outer, corner);
            DrawAuthoredEdgeModules(outer, corner, modulate);
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
        Texture2D? texture = ResourceLoader.Exists(recipe.HybridFramePath)
            ? GD.Load<Texture2D>(recipe.HybridFramePath)
            : null;
        return texture is null
            ? 0
            : Mathf.RoundToInt(Math.Min(texture.GetWidth(), texture.GetHeight()) * recipe.ProtectedFraction);
    }

    public static float DestinationCornerSize(HudFactionSkinRecipe recipe, HudFactionChromeRole role,
        Vector2 destinationSize, float chromeScale)
    {
        float requested = (role == HudFactionChromeRole.TopStrip ? 22f : 44f) *
            Mathf.Clamp(chromeScale, 0.75f, 1.35f) * recipe.DestinationScale;
        float available = Mathf.Min(destinationSize.X, destinationSize.Y) * 0.48f;
        return Mathf.Max(1f, Mathf.Round(Mathf.Min(requested, available)));
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
        return SourceCornerSize(_texture, _recipe);
    }

    internal static int SourceCornerSize(Texture2D texture, HudFactionSkinRecipe recipe)
    {
        int shortest = Math.Min(texture.GetWidth(), texture.GetHeight());
        return Mathf.Clamp(Mathf.RoundToInt(shortest * recipe.ProtectedFraction), 1, shortest / 2);
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
        Vector4 aperture = _recipe.ApertureInsetRatios;
        float leftRail = Mathf.Clamp(corner * aperture.X, 3f, outer.Size.X * 0.48f);
        float topRail = Mathf.Clamp(corner * aperture.Y, 3f, outer.Size.Y * 0.48f);
        float rightRail = Mathf.Clamp(corner * aperture.Z, 3f, outer.Size.X * 0.48f);
        float bottomRail = Mathf.Clamp(corner * aperture.W, 3f, outer.Size.Y * 0.48f);
        Color edge = WithAlpha(recess.Darkened(0.34f), 0.96f);
        Color plate = WithAlpha(shell, top ? 0.92f : 0.96f);

        DrawRect(new Rect2(outer.Position, new Vector2(outer.Size.X, topRail)), edge);
        DrawRect(new Rect2(new Vector2(outer.Position.X, outer.End.Y - bottomRail),
            new Vector2(outer.Size.X, bottomRail)), edge);
        DrawRect(new Rect2(outer.Position, new Vector2(leftRail, outer.Size.Y)), edge);
        DrawRect(new Rect2(new Vector2(outer.End.X - rightRail, outer.Position.Y),
            new Vector2(rightRail, outer.Size.Y)), edge);

        float inner = Mathf.Max(1f, topRail * 0.35f);
        float bottomInner = Mathf.Max(1f, bottomRail * 0.35f);
        DrawRect(new Rect2(new Vector2(outer.Position.X + corner * 0.60f, outer.Position.Y + 1f),
            new Vector2(Mathf.Max(0f, outer.Size.X - corner * 1.20f), inner)), plate);
        DrawRect(new Rect2(new Vector2(outer.Position.X + corner * 0.60f, outer.End.Y - bottomInner - 1f),
            new Vector2(Mathf.Max(0f, outer.Size.X - corner * 1.20f), bottomInner)), plate.Darkened(0.22f));

        float signal = Mathf.Max(1f, (top ? 1.1f : 1.7f) * _chromeScale);
        DrawFactionSignals(outer, corner, topRail, bottomRail, signal, accent, secondary);
    }

    private void DrawAuthoredEdgeModules(Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null) return;
        float source = SourceCornerSize();
        float sourceSpanX = _texture.GetWidth() - source * 2f;
        float sourceSpanY = _texture.GetHeight() - source * 2f;
        float destinationSpanX = outer.Size.X - corner * 2f;
        float destinationSpanY = outer.Size.Y - corner * 2f;
        if (sourceSpanX <= 1f || sourceSpanY <= 1f || destinationSpanX < 0f || destinationSpanY < 0f) return;

        Color edgeModulate = new(modulate, modulate.A *
            (Role == HudFactionChromeRole.TopStrip ? 0.78f : 0.94f));
        float isotropicScale = corner / source;
        if (destinationSpanX > 0.5f)
        {
            float centerSource = Mathf.Min(source * 1.35f, sourceSpanX * 0.42f);
            float shoulderSource = Mathf.Min(source, (sourceSpanX - centerSource) * 0.5f);
            float shoulderWidth = Mathf.Min(shoulderSource * isotropicScale, destinationSpanX * 0.22f);
            float centerWidth = Mathf.Min(centerSource * isotropicScale, destinationSpanX * 0.24f);
            shoulderSource = shoulderWidth / isotropicScale;
            centerSource = centerWidth / isotropicScale;
            DrawHorizontalModule(outer.Position.X + corner, shoulderWidth,
                source, shoulderSource, outer, corner, edgeModulate);
            DrawHorizontalModule(outer.End.X - corner - shoulderWidth, shoulderWidth,
                _texture.GetWidth() - source - shoulderSource, shoulderSource, outer, corner, edgeModulate);
            DrawHorizontalModule(outer.Position.X + (outer.Size.X - centerWidth) * 0.5f, centerWidth,
                (_texture.GetWidth() - centerSource) * 0.5f, centerSource, outer, corner, edgeModulate);
        }
        if (destinationSpanY > 0.5f)
        {
            float centerSource = Mathf.Min(source * 1.35f, sourceSpanY * 0.42f);
            float shoulderSource = Mathf.Min(source, (sourceSpanY - centerSource) * 0.5f);
            float shoulderHeight = Mathf.Min(shoulderSource * isotropicScale, destinationSpanY * 0.22f);
            float centerHeight = Mathf.Min(centerSource * isotropicScale, destinationSpanY * 0.24f);
            shoulderSource = shoulderHeight / isotropicScale;
            centerSource = centerHeight / isotropicScale;
            DrawVerticalModule(outer.Position.Y + corner, shoulderHeight,
                source, shoulderSource, outer, corner, edgeModulate);
            DrawVerticalModule(outer.End.Y - corner - shoulderHeight, shoulderHeight,
                _texture.GetHeight() - source - shoulderSource, shoulderSource, outer, corner, edgeModulate);
            DrawVerticalModule(outer.Position.Y + (outer.Size.Y - centerHeight) * 0.5f, centerHeight,
                (_texture.GetHeight() - centerSource) * 0.5f, centerSource, outer, corner, edgeModulate);
        }
    }

    private void DrawHorizontalModule(float destinationX, float destinationWidth,
        float sourceX, float sourceWidth, Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null || destinationWidth <= 0.5f || sourceWidth <= 0.5f) return;
        DrawTextureRectRegion(_texture,
            new Rect2(new Vector2(destinationX, outer.Position.Y), new Vector2(destinationWidth, corner)),
            new Rect2(sourceX, 0f, sourceWidth, SourceCornerSize()), modulate);
        DrawTextureRectRegion(_texture,
            new Rect2(new Vector2(destinationX, outer.End.Y - corner), new Vector2(destinationWidth, corner)),
            new Rect2(sourceX, _texture.GetHeight() - SourceCornerSize(), sourceWidth, SourceCornerSize()), modulate);
    }

    private void DrawVerticalModule(float destinationY, float destinationHeight,
        float sourceY, float sourceHeight, Rect2 outer, float corner, Color modulate)
    {
        if (_texture is null || destinationHeight <= 0.5f || sourceHeight <= 0.5f) return;
        float sourceCorner = SourceCornerSize();
        DrawTextureRectRegion(_texture,
            new Rect2(new Vector2(outer.Position.X, destinationY), new Vector2(corner, destinationHeight)),
            new Rect2(0f, sourceY, sourceCorner, sourceHeight), modulate);
        DrawTextureRectRegion(_texture,
            new Rect2(new Vector2(outer.End.X - corner, destinationY), new Vector2(corner, destinationHeight)),
            new Rect2(_texture.GetWidth() - sourceCorner, sourceY, sourceCorner, sourceHeight), modulate);
    }

    private void DrawFactionSignals(Rect2 outer, float corner, float topRail, float bottomRail, float width,
        Color accent, Color secondary)
    {
        float left = outer.Position.X + corner * 0.82f;
        float right = outer.End.X - corner * 0.82f;
        if (right <= left) return;
        float topY = outer.Position.Y + Mathf.Min(topRail - 1f, topRail * 0.58f);
        float bottomY = outer.End.Y - Mathf.Min(bottomRail - 1f, bottomRail * 0.48f);

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
