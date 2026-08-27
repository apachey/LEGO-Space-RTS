using Godot;

namespace LegoSpaceRTS.UI;

public enum HudFactionChromeRole : byte
{
    TopStrip,
    BottomDeck
}

/// <summary>
/// Composes one outer faction chassis from a square source atlas without
/// stretching its illustrated machinery. The source is used as hardware, not
/// wallpaper: large protected corners frame the console and a small number of
/// square modules mark real section junctions on code-native rails.
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
    private float _leftJunction = 0.22f;
    private float _rightJunction = 0.78f;

    public HudFactionChromeRole Role { get; private set; }
    public bool IsConfigured => _texture is not null;
    public bool UsesFixedSquareCorners => true;
    public bool UsesProtectedSourceModules => true;
    public bool UsesSparseJunctionModules => true;
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
        if (_texture is null || _intensity <= 0.001f) return;

        Rect2 outer = new(
            new Vector2(-_outerExpansion, -_outerExpansion),
            Size + new Vector2(_outerExpansion * 2f, _outerExpansion * 2f));
        if (outer.Size.X < 8f || outer.Size.Y < 8f) return;

        float requestedCorner = Role switch
        {
            HudFactionChromeRole.TopStrip => 21f,
            _ => 38f
        } * _chromeScale;
        float corner = Mathf.Min(requestedCorner, Mathf.Min(outer.Size.X, outer.Size.Y) * 0.45f);
        if (corner < 2f) return;

        Color modulate = new(1f, 1f, 1f, _intensity);
        DrawCorners(outer, corner, modulate);
        DrawRails(outer, corner);
        DrawFunctionalModules(outer, corner, modulate);
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

    public static Color SecondaryAccentForFaction(int faction) => Math.Clamp(faction, 0, Recipes.Length - 1) switch
    {
        1 => new Color("77a9bc"), // Mars Mission astronauts · navigation blue
        2 => new Color("7754a6"), // Mars Mission aliens · deep resonance violet
        3 => new Color("7296ad"), // Life on Mars astronauts · equipment blue
        4 => new Color("b9674f"), // Life on Mars Martians · articulated red
        _ => new Color("147f79")  // Rock Raiders · dark turquoise machinery
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

    private void DrawRails(Rect2 outer, float corner)
    {
        Color primary = new(_accent.R, _accent.G, _accent.B, _intensity * 0.72f);
        Color secondaryBase = SecondaryAccentForFaction(Array.IndexOf(Recipes, _recipe));
        Color secondary = new(secondaryBase.R, secondaryBase.G, secondaryBase.B, _intensity * 0.42f);
        float outerLine = Role == HudFactionChromeRole.TopStrip ? 1.5f : 2f;
        float inset = Role == HudFactionChromeRole.TopStrip ? 2f : 3f;
        DrawLine(new Vector2(outer.Position.X + corner, outer.Position.Y + inset),
            new Vector2(outer.End.X - corner, outer.Position.Y + inset), primary, outerLine, true);
        DrawLine(new Vector2(outer.Position.X + corner, outer.End.Y - inset),
            new Vector2(outer.End.X - corner, outer.End.Y - inset), primary, outerLine, true);
        DrawLine(new Vector2(outer.Position.X + inset, outer.Position.Y + corner),
            new Vector2(outer.Position.X + inset, outer.End.Y - corner), secondary, outerLine, true);
        DrawLine(new Vector2(outer.End.X - inset, outer.Position.Y + corner),
            new Vector2(outer.End.X - inset, outer.End.Y - corner), secondary, outerLine, true);
    }

    private void DrawFunctionalModules(Rect2 outer, float corner, Color modulate)
    {
        float source = _recipe.ProtectedSourceSize;
        float sourceX = (AtlasSize - source) * 0.5f;
        Rect2 topSource = new(sourceX, 0f, source, source);
        Rect2 bottomSource = new(sourceX, AtlasSize - source, source, source);
        float module = Mathf.Min(corner, Role == HudFactionChromeRole.TopStrip ? 20f * _chromeScale : 34f * _chromeScale);
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

    private readonly record struct ChromeRecipe(string TexturePath, int ProtectedSourceSize);
}
