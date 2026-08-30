using Godot;

namespace LegoSpaceRTS.UI;

public enum HudFactionRailStyle : byte
{
    Industrial,
    Expedition,
    Resonance,
    Pneumatic
}

/// <summary>
/// One complete presentation recipe per playable faction. The frame, shell,
/// recessed field and accents are intentionally inseparable in normal use;
/// semantic health/warning/danger colours stay shared between factions.
/// </summary>
public readonly record struct HudFactionSkinRecipe(
    HudFaction Faction,
    string Name,
    string HybridFramePath,
    string LegacyFramePath,
    float ProtectedFraction,
    Vector4 ApertureInsetRatios,
    float DestinationScale,
    HudFactionRailStyle RailStyle,
    string Background,
    string Raised,
    string Recessed,
    string Accent,
    string Secondary,
    string TextPrimary,
    string TextMuted,
    string Selection);

public static class HudFactionSkinLibrary
{
    public const int Count = 4;

    private const string SharedGood = "#65c987";
    private const string SharedWarning = "#f2b84b";
    private const string SharedDanger = "#ff6b45";

    private static readonly HudFactionSkinRecipe[] Recipes =
    {
        new(
            HudFaction.RockRaiders,
            "Rock Raiders",
            "res://Assets/M7/Hud/rock_raiders_frame.png",
            "res://Assets/M7/Hud/rock_raiders_frame.png",
            48f / 256f,
            new Vector4(32f / 48f, 32f / 48f, 32f / 48f, 35f / 48f),
            1f,
            HudFactionRailStyle.Industrial,
            "#303534", "#545b58", "#171b1a", "#a76538", "#087f78",
            "#f1eadc", "#b7b1a4", "#20a69d"),
        new(
            HudFaction.Astronauts,
            "Astronauts",
            "res://Assets/M7/Hud/astronauts_unified_frame_v2.png",
            "res://Assets/M7/Hud/astronauts_frame.png",
            0.19f,
            new Vector4(154f / 238f, 154f / 238f, 154f / 238f, 170f / 238f),
            1.06f,
            HudFactionRailStyle.Expedition,
            "#d2d4d0", "#f0efe9", "#344149", "#e96f18", "#397fb5",
            "#101518", "#29373e", "#328fff"),
        new(
            HudFaction.Aliens,
            "Aliens",
            "res://Assets/M7/Hud/aliens_frame_v2.png",
            "res://Assets/M7/Hud/aliens_frame.png",
            0.19f,
            new Vector4(151f / 238f, 145f / 238f, 148f / 238f, 153f / 238f),
            1.06f,
            HudFactionRailStyle.Resonance,
            "#111315", "#293321", "#070908", "#78b82a", "#6f777b",
            "#e9f0df", "#9ca693", "#7cff2e"),
        new(
            HudFaction.Martians,
            "Martians",
            "res://Assets/M7/Hud/martians_frame.png",
            "res://Assets/M7/Hud/martians_frame.png",
            52f / 256f,
            new Vector4(28f / 52f, 29f / 52f, 28f / 52f, 34f / 52f),
            1.02f,
            HudFactionRailStyle.Pneumatic,
            "#8d796c", "#c2b397", "#343338", "#a64c49", "#3d7794",
            "#111517", "#303537", "#3272b8")
    };

    public static HudFactionSkinRecipe RecipeFor(int faction) =>
        Recipes[Math.Clamp(faction, 0, Recipes.Length - 1)];

    public static int IndexFor(HudFaction faction) => Math.Clamp((int)faction, 0, Recipes.Length - 1);

    public static void Apply(M7HudProfile profile, int faction)
    {
        profile.ArtSkin ??= new HudArtSkinProfile();
        profile.Colors ??= new HudColorProfile();
        HudFactionSkinRecipe recipe = RecipeFor(faction);
        profile.ArtSkin.Faction = (int)recipe.Faction;
        profile.ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound;
        profile.Colors.Background = recipe.Background;
        profile.Colors.Raised = recipe.Raised;
        profile.Colors.Recessed = recipe.Recessed;
        profile.Colors.Accent = recipe.Accent;
        profile.Colors.TextPrimary = recipe.TextPrimary;
        profile.Colors.TextMuted = recipe.TextMuted;
        profile.Colors.Good = SharedGood;
        profile.Colors.Warning = SharedWarning;
        profile.Colors.Danger = SharedDanger;
        profile.Colors.Selection = recipe.Selection;
    }

    public static bool Validate(out string error)
    {
        if (Recipes.Length != Count || Recipes.Select(recipe => recipe.Faction).Distinct().Count() != Count)
        {
            error = "Faction HUD recipes must map exactly once to all four playable factions.";
            return false;
        }

        foreach (HudFactionSkinRecipe recipe in Recipes)
        {
            if (recipe.ProtectedFraction is < 0.12f or > 0.28f)
            {
                error = $"Faction HUD recipe {recipe.Name} has unsafe frame guides.";
                return false;
            }
            if (recipe.ApertureInsetRatios.X is < 0.45f or > 0.90f ||
                recipe.ApertureInsetRatios.Y is < 0.45f or > 0.90f ||
                recipe.ApertureInsetRatios.Z is < 0.45f or > 0.90f ||
                recipe.ApertureInsetRatios.W is < 0.45f or > 0.90f ||
                recipe.DestinationScale is < 0.90f or > 1.15f)
            {
                error = $"Faction HUD recipe {recipe.Name} has unsafe aperture geometry.";
                return false;
            }
            if (!ResourceLoader.Exists(recipe.HybridFramePath) || !ResourceLoader.Exists(recipe.LegacyFramePath))
            {
                error = $"Faction HUD frame is missing for {recipe.Name}.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public static Color ColorOrFallback(string html, Color fallback) =>
        Color.HtmlIsValid(html) ? new Color(html) : fallback;
}
