using Godot;

namespace LegoSpaceRTS.UI;

/// <summary>
/// Resolves semantic HUD text colours against the actual surface they sit on.
/// Faction colours remain decorative accents; readable text is never allowed to
/// inherit a low-contrast faction token blindly.
/// </summary>
public readonly record struct HudTextColorSet(
    Color TopPrimary,
    Color TopMuted,
    Color DeckPrimary,
    Color DeckMuted,
    Color SelectionPrimary,
    Color SelectionMuted,
    Color RaisedPrimary,
    Color RaisedMuted,
    Color TopAccent,
    Color DeckAccent,
    Color SelectionAccent);

public static class HudTextPalette
{
    private static readonly Color LightFallback = new("f4f1e8");
    private static readonly Color DarkFallback = new("080a0b");

    public static HudTextColorSet Resolve(M7HudProfile profile)
    {
        Color background = Parse(profile.Colors.Background, new Color("303534"));
        Color deck = DeckSurface(profile);
        Color selection = SelectionSurface(profile);
        Color raised = Parse(profile.Colors.Raised, new Color("545b58"));
        Color preferred = Parse(profile.Colors.TextPrimary, LightFallback);
        Color accent = Parse(profile.Colors.Accent, new Color("a76538"));

        Color topPrimary = EnsureContrast(preferred, background, 4.5f);
        Color deckPrimary = EnsureContrast(preferred, deck, 4.5f);
        Color selectionPrimary = EnsureContrast(preferred, selection, 4.5f);
        Color raisedPrimary = EnsureContrast(preferred, raised, 4.5f);
        Color topMuted = MutedFor(background, topPrimary);
        Color deckMuted = MutedFor(deck, deckPrimary);
        Color selectionMuted = MutedFor(selection, selectionPrimary);
        Color raisedMuted = MutedFor(raised, raisedPrimary);
        Color topAccent = AccentFor(background, accent, topPrimary);
        Color deckAccent = AccentFor(deck, accent, deckPrimary);
        Color selectionAccent = AccentFor(selection, accent, selectionPrimary);
        return new HudTextColorSet(topPrimary, topMuted, deckPrimary, deckMuted,
            selectionPrimary, selectionMuted, raisedPrimary, raisedMuted,
            topAccent, deckAccent, selectionAccent);
    }

    public static Color DeckSurface(M7HudProfile profile)
        => CompositeSectionSurface(profile, DeckPlateSurface(profile));

    public static Color SelectionSurface(M7HudProfile profile)
        => CompositeSectionSurface(profile, SelectionPlateSurface(profile));

    public static Color DeckPlateSurface(M7HudProfile profile)
    {
        Color recessed = Parse(profile.Colors.Recessed, new Color("171b1a"));
        Color factionSurface = HudFactionChrome.SurfaceForFaction(profile.ArtSkin.Faction);
        float factionFill = LegacyFactionFill(profile);
        return recessed.Lightened(0.025f).Lerp(factionSurface, factionFill * 0.85f);
    }

    public static Color SelectionPlateSurface(M7HudProfile profile)
    {
        Color background = Parse(profile.Colors.Background, new Color("303534"));
        Color factionSurface = HudFactionChrome.SurfaceForFaction(profile.ArtSkin.Faction);
        float factionFill = LegacyFactionFill(profile);
        return background.Lightened(0.025f).Lerp(factionSurface, factionFill * 1.15f);
    }

    public static bool ValidateFactionRecipes(out string error)
    {
        HudArtFinish[] finishes =
        {
            HudArtFinish.HybridConsole,
            HudArtFinish.StructuralConsole,
            HudArtFinish.LegacyFrames,
            HudArtFinish.Clean
        };
        for (int faction = 0; faction < HudFactionSkinLibrary.Count; faction++)
        foreach (HudArtFinish finish in finishes)
        {
            M7HudProfile profile = M7HudProfile.CreateDefault();
            HudFactionSkinLibrary.Apply(profile, faction);
            profile.ArtSkin.Finish = finish;
            HudTextColorSet set = Resolve(profile);
            Color top = Parse(profile.Colors.Background, Colors.Black);
            Color deck = DeckSurface(profile);
            Color selection = SelectionSurface(profile);
            Color raised = Parse(profile.Colors.Raised, Colors.Black);
            if (ContrastRatio(set.TopPrimary, top) < 4.5f ||
                ContrastRatio(set.DeckPrimary, deck) < 4.5f ||
                ContrastRatio(set.SelectionPrimary, selection) < 4.5f ||
                ContrastRatio(set.RaisedPrimary, raised) < 4.5f ||
                ContrastRatio(set.TopMuted, top) < 3f ||
                ContrastRatio(set.DeckMuted, deck) < 3f ||
                ContrastRatio(set.SelectionMuted, selection) < 3f ||
                ContrastRatio(set.RaisedMuted, raised) < 3f ||
                ContrastRatio(set.TopAccent, top) < 3f ||
                ContrastRatio(set.DeckAccent, deck) < 3f ||
                ContrastRatio(set.SelectionAccent, selection) < 3f)
            {
                error = $"Faction HUD text palette {faction}/{finish} does not meet its surface contrast floor.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public static float ContrastRatio(Color first, Color second)
    {
        float high = Math.Max(RelativeLuminance(first), RelativeLuminance(second));
        float low = Math.Min(RelativeLuminance(first), RelativeLuminance(second));
        return (high + 0.05f) / (low + 0.05f);
    }

    public static Color EnsureReadable(Color candidate, Color surface, Color fallback,
        float minimumContrast = 3f)
    {
        if (ContrastRatio(candidate, surface) >= minimumContrast) return candidate;
        for (int step = 1; step <= 10; step++)
        {
            Color adjusted = candidate.Lerp(fallback, step / 10f);
            if (ContrastRatio(adjusted, surface) >= minimumContrast) return adjusted;
        }
        return fallback;
    }

    private static Color MutedFor(Color surface, Color primary)
    {
        Color muted = surface.Lerp(primary, 0.72f);
        return EnsureContrast(muted, surface, 3f);
    }

    private static Color AccentFor(Color surface, Color accent, Color primary)
        => EnsureReadable(accent, surface, primary);

    private static float LegacyFactionFill(M7HudProfile profile) =>
        profile.ArtSkin.Enabled && profile.ArtSkin.Finish == HudArtFinish.LegacyFrames
            ? 0.16f * profile.ArtSkin.ChromeIntensity
            : 0f;

    private static Color CompositeSectionSurface(M7HudProfile profile, Color plate)
    {
        Color recessed = Parse(profile.Colors.Recessed, new Color("171b1a"));
        Color factionSurface = HudFactionChrome.SurfaceForFaction(profile.ArtSkin.Faction);
        Color deck = recessed.Lerp(factionSurface, LegacyFactionFill(profile) * 1.25f);
        float sectionOpacity = profile.ArtSkin.Enabled && profile.ArtSkin.Finish == HudArtFinish.HybridConsole
            ? 0.28f * profile.Surface.PanelOpacity
            : profile.Surface.PanelOpacity;
        return deck.Lerp(plate, Mathf.Clamp(sectionOpacity, 0f, 1f));
    }

    private static Color EnsureContrast(Color candidate, Color surface, float target)
    {
        if (ContrastRatio(candidate, surface) >= target) return candidate;
        return ContrastRatio(LightFallback, surface) >= ContrastRatio(DarkFallback, surface)
            ? LightFallback
            : DarkFallback;
    }

    private static float RelativeLuminance(Color color) =>
        0.2126f * LinearChannel(color.R) +
        0.7152f * LinearChannel(color.G) +
        0.0722f * LinearChannel(color.B);

    private static float LinearChannel(float channel) => channel <= 0.04045f
        ? channel / 12.92f
        : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);

    private static Color Parse(string html, Color fallback) =>
        Color.HtmlIsValid(html) ? new Color(html) : fallback;
}
