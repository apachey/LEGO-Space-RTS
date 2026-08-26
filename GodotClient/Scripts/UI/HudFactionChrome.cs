using Godot;

namespace LegoSpaceRTS.UI;

public enum HudFactionChromeRole : byte
{
    TopStrip,
    SquarePanel,
    MainPanel
}

/// <summary>
/// Composes faction chrome from a square source atlas without stretching its
/// illustrated machinery. Corners remain square crops, while sparse square
/// motifs punctuate code-native rails. The transparent centre is deliberately
/// left to the functional HUD surface.
/// </summary>
public partial class HudFactionChrome : Control
{
    private const int AtlasSize = 256;

    private static readonly ChromeRecipe[] Recipes =
    {
        new("res://Assets/M7/Hud/rock_raiders_frame.png", 48),
        new("res://Assets/M7/Hud/astronauts_frame.png", 44),
        new("res://Assets/M7/Hud/aliens_frame.png", 52),
        new("res://Assets/M7/Hud/life_on_mars_astronauts_frame.png", 44),
        new("res://Assets/M7/Hud/martians_frame.png", 52)
    };

    private Texture2D? _texture;
    private ChromeRecipe _recipe = Recipes[0];
    private float _intensity = 0.78f;
    private float _chromeScale = 1f;
    private float _outerExpansion;
    private Color _accent = AccentForFaction(0);

    public HudFactionChromeRole Role { get; private set; }
    public bool IsConfigured => _texture is not null;
    public bool UsesFixedSquareCorners => true;
    public bool UsesProtectedSourceModules => true;
    public int ProtectedSourceSize => _recipe.ProtectedSourceSize;

    public HudFactionChrome()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;
    }

    public void Configure(int faction, HudFactionChromeRole role, float intensity, float chromeScale, float outerExpansion)
    {
        int index = Math.Clamp(faction, 0, Recipes.Length - 1);
        _recipe = Recipes[index];
        _texture = GD.Load<Texture2D>(_recipe.TexturePath);
        Role = role;
        _intensity = Mathf.Clamp(intensity, 0f, 1f);
        _chromeScale = Mathf.Clamp(chromeScale, 0.75f, 1.35f);
        _outerExpansion = Mathf.Max(0f, outerExpansion);
        _accent = AccentForFaction(index);
        Visible = _texture is not null && _intensity > 0.001f;
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized) QueueRedraw();
    }

    public override void _Draw()
    {
        if (_texture is null || _intensity <= 0.001f) return;

        Rect2 outer = new(
            new Vector2(-_outerExpansion, -_outerExpansion),
            Size + new Vector2(_outerExpansion * 2f, _outerExpansion * 2f));
        if (outer.Size.X < 8f || outer.Size.Y < 8f) return;

        float requestedCorner = Role switch
        {
            HudFactionChromeRole.TopStrip => 18f,
            HudFactionChromeRole.SquarePanel => 30f,
            _ => 28f
        } * _chromeScale;
        float corner = Mathf.Min(requestedCorner, Mathf.Min(outer.Size.X, outer.Size.Y) * 0.45f);
        if (corner < 2f) return;

        Color modulate = new(1f, 1f, 1f, _intensity);
        DrawCorners(outer, corner, modulate);
        DrawHorizontalRails(outer, corner, modulate);
        DrawVerticalRails(outer, corner, modulate);
    }

    public static string TexturePathForFaction(int faction) =>
        Recipes[Math.Clamp(faction, 0, Recipes.Length - 1)].TexturePath;

    public static Color AccentForFaction(int faction) => Math.Clamp(faction, 0, Recipes.Length - 1) switch
    {
        1 => new Color("e97832"), // Mars Mission astronauts · restrained orange
        2 => new Color("91d837"), // Mars Mission aliens · resonance lime
        3 => new Color("c84d43"), // Life on Mars astronauts · equipment red
        4 => new Color("6ea9b5"), // Life on Mars Martians · Aero Tube blue
        _ => new Color("b47a3a")  // Rock Raiders · industrial brown/copper
    };

    public static bool ValidateRecipes(out string error)
    {
        int[] expectedProtectedSizes = { 48, 44, 52, 44, 52 };
        for (int i = 0; i < Recipes.Length; i++)
        {
            ChromeRecipe recipe = Recipes[i];
            if (recipe.ProtectedSourceSize != expectedProtectedSizes[i] ||
                recipe.ProtectedSourceSize < 32 ||
                AtlasSize - recipe.ProtectedSourceSize * 2 < 128)
            {
                error = $"Faction chrome recipe {i} has unsafe source guides.";
                return false;
            }
            if (!ResourceLoader.Exists(recipe.TexturePath))
            {
                error = $"Faction chrome texture is missing: {recipe.TexturePath}";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private void DrawCorners(Rect2 outer, float corner, Color modulate)
    {
        float source = _recipe.ProtectedSourceSize;
        float right = AtlasSize - source;
        float bottom = AtlasSize - source;
        float destinationRight = outer.End.X - corner;
        float destinationBottom = outer.End.Y - corner;

        DrawTextureRectRegion(_texture!, new Rect2(outer.Position, new Vector2(corner, corner)),
            new Rect2(0f, 0f, source, source), modulate);
        DrawTextureRectRegion(_texture!, new Rect2(new Vector2(destinationRight, outer.Position.Y), new Vector2(corner, corner)),
            new Rect2(right, 0f, source, source), modulate);
        DrawTextureRectRegion(_texture!, new Rect2(new Vector2(outer.Position.X, destinationBottom), new Vector2(corner, corner)),
            new Rect2(0f, bottom, source, source), modulate);
        DrawTextureRectRegion(_texture!, new Rect2(new Vector2(destinationRight, destinationBottom), new Vector2(corner, corner)),
            new Rect2(right, bottom, source, source), modulate);
    }

    private void DrawHorizontalRails(Rect2 outer, float corner, Color modulate)
    {
        float available = outer.Size.X - corner * 2f;
        if (available <= 0f) return;

        float source = _recipe.ProtectedSourceSize;
        // The generated atlas contains a complete decorative rail. Repeating
        // that whole rail makes every panel look like a row of tiny frames.
        // A protected square motif at its centre retains faction character at
        // UI scale without anisotropic stretching or visual chatter.
        float sourceX = (AtlasSize - source) * 0.5f;
        Rect2 topSource = new(sourceX, 0f, source, source);
        Rect2 bottomSource = new(sourceX, AtlasSize - source, source, source);
        float start = outer.Position.X + corner;
        Color railColor = new(_accent.R, _accent.G, _accent.B, _intensity * 0.62f);
        float lineWidth = Role == HudFactionChromeRole.TopStrip ? 1.5f : 2f;
        DrawLine(new Vector2(start, outer.Position.Y + corner * 0.52f),
            new Vector2(start + available, outer.Position.Y + corner * 0.52f), railColor, lineWidth, true);
        DrawLine(new Vector2(start, outer.End.Y - corner * 0.52f),
            new Vector2(start + available, outer.End.Y - corner * 0.52f), railColor, lineWidth, true);
        DrawHorizontalModules(start, outer.Position.Y, available, corner, topSource, modulate);
        DrawHorizontalModules(start, outer.End.Y - corner, available, corner, bottomSource, modulate);
    }

    private void DrawVerticalRails(Rect2 outer, float corner, Color modulate)
    {
        float available = outer.Size.Y - corner * 2f;
        if (available <= 0f) return;

        float source = _recipe.ProtectedSourceSize;
        float sourceY = (AtlasSize - source) * 0.5f;
        Rect2 leftSource = new(0f, sourceY, source, source);
        Rect2 rightSource = new(AtlasSize - source, sourceY, source, source);
        float start = outer.Position.Y + corner;
        Color railColor = new(_accent.R, _accent.G, _accent.B, _intensity * 0.62f);
        DrawLine(new Vector2(outer.Position.X + corner * 0.52f, start),
            new Vector2(outer.Position.X + corner * 0.52f, start + available), railColor, 2f, true);
        DrawLine(new Vector2(outer.End.X - corner * 0.52f, start),
            new Vector2(outer.End.X - corner * 0.52f, start + available), railColor, 2f, true);
        DrawVerticalModules(outer.Position.X, start, corner, available, leftSource, modulate);
        DrawVerticalModules(outer.End.X - corner, start, corner, available, rightSource, modulate);
    }

    private void DrawHorizontalModules(float x, float y, float width, float height, Rect2 source, Color modulate)
    {
        float sourceScale = height / source.Size.Y;
        float moduleWidth = source.Size.X * sourceScale;
        if (moduleWidth > width) return;
        float targetSpacing = (Role switch
        {
            HudFactionChromeRole.TopStrip => 260f,
            HudFactionChromeRole.SquarePanel => 150f,
            _ => 180f
        }) * _chromeScale;
        int desiredCount = Role switch
        {
            HudFactionChromeRole.TopStrip => 5,
            HudFactionChromeRole.MainPanel => 1,
            _ => Math.Clamp(Mathf.RoundToInt(width / targetSpacing), 1, 4)
        };
        int nonOverlappingCount = Math.Max(1, Mathf.FloorToInt(width / moduleWidth));
        int count = Math.Min(desiredCount, nonOverlappingCount);
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? (width - moduleWidth) * 0.5f
                : Mathf.Lerp(0f, width - moduleWidth, i / (float)(count - 1));
            DrawTextureRectRegion(_texture!, new Rect2(x + offset, y, moduleWidth, height), source, modulate);
        }
    }

    private void DrawVerticalModules(float x, float y, float width, float height, Rect2 source, Color modulate)
    {
        float sourceScale = width / source.Size.X;
        float moduleHeight = source.Size.Y * sourceScale;
        if (moduleHeight > height) return;
        float targetSpacing = (Role == HudFactionChromeRole.SquarePanel ? 150f : 170f) * _chromeScale;
        int desiredCount = Role == HudFactionChromeRole.MainPanel
            ? 1
            : Math.Clamp(Mathf.RoundToInt(height / targetSpacing), 1, 4);
        int nonOverlappingCount = Math.Max(1, Mathf.FloorToInt(height / moduleHeight));
        int count = Math.Min(desiredCount, nonOverlappingCount);
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? (height - moduleHeight) * 0.5f
                : Mathf.Lerp(0f, height - moduleHeight, i / (float)(count - 1));
            DrawTextureRectRegion(_texture!, new Rect2(x, y + offset, width, moduleHeight), source, modulate);
        }
    }

    private readonly record struct ChromeRecipe(string TexturePath, int ProtectedSourceSize);
}
