using Godot;

namespace LegoSpaceRTS.UI;

/// <summary>
/// Loads the pinned M7 HUD typefaces once and exposes them by semantic role.
/// Typography styling remains owned by the HUD; this class only owns asset
/// identity, loading and validation.
/// </summary>
public static class HudTypographyLibrary
{
    public const string HeadingPath = "res://Assets/M7/Fonts/Oxanium-SemiBold.ttf";
    public const string BodyPath = "res://Assets/M7/Fonts/IBMPlexSans-Regular.ttf";
    public const string BodyMediumPath = "res://Assets/M7/Fonts/IBMPlexSans-Medium.ttf";

    private static Font? _heading;
    private static Font? _body;
    private static Font? _bodyMedium;

    public static Font? Heading => Load(ref _heading, HeadingPath);
    public static Font? Body => Load(ref _body, BodyPath);
    public static Font? BodyMedium => Load(ref _bodyMedium, BodyMediumPath);

    public static bool Validate(out string error)
    {
        if (!TryValidate(HeadingPath, ref _heading, "heading", out error)) return false;
        if (!TryValidate(BodyPath, ref _body, "body", out error)) return false;
        if (!TryValidate(BodyMediumPath, ref _bodyMedium, "medium body", out error)) return false;

        error = string.Empty;
        return true;
    }

    private static Font? Load(ref Font? cache, string path)
    {
        cache ??= ResourceLoader.Load<Font>(path);
        return cache;
    }

    private static bool TryValidate(string path, ref Font? cache, string role, out string error)
    {
        if (!ResourceLoader.Exists(path))
        {
            error = $"HUD {role} font asset is missing: {path}";
            return false;
        }

        if (Load(ref cache, path) is null)
        {
            error = $"HUD {role} font asset could not be loaded: {path}";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
