using Godot;

namespace LegoSpaceRTS.Presentation;

public enum LegoMaterialFamily : byte
{
    MoldedPolymer = 0,
    ToolMetal = 1,
    Rubber = 2,
    TransparentPolymer = 3,
    Crystal = 4,
    Terrain = 5
}

/// <summary>
/// Godot-native T064 material masters. The values are deliberately centralized:
/// the game director can art-direct a small number of presets without requiring
/// faction-specific shader forks or touching authoritative simulation code.
/// </summary>
public static class LegoMaterialLibrary
{
    public const float MoldedPolymerRoughness = 0.40f;
    public const float ToolMetalRoughness = 0.30f;
    public const float RubberRoughness = 0.78f;
    public const float TransparentPolymerRoughness = 0.24f;
    public const float CrystalRoughness = 0.18f;
    public const float TerrainRoughness = 0.90f;

    public static StandardMaterial3D Create(
        LegoMaterialFamily family,
        Color color,
        float emissionEnergy = 0f,
        float? roughnessOverride = null)
    {
        StandardMaterial3D material = new()
        {
            ResourceName = $"T064_{family}",
            AlbedoColor = NormalizeAlpha(family, color),
            Metallic = family == LegoMaterialFamily.ToolMetal ? 0.82f : 0f,
            Roughness = roughnessOverride ?? DefaultRoughness(family)
        };

        if (family is LegoMaterialFamily.TransparentPolymer or LegoMaterialFamily.Crystal)
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

        if (family == LegoMaterialFamily.Crystal || emissionEnergy > 0f)
        {
            material.EmissionEnabled = true;
            material.Emission = new Color(color.R, color.G, color.B).Lerp(Colors.White, family == LegoMaterialFamily.Crystal ? 0.18f : 0f);
            material.EmissionEnergyMultiplier = emissionEnergy > 0f ? emissionEnergy : 1.35f;
        }

        return material;
    }

    public static StandardMaterial3D MoldedPolymer(Color color, float? roughnessOverride = null)
        => Create(LegoMaterialFamily.MoldedPolymer, color, roughnessOverride: roughnessOverride);

    public static StandardMaterial3D ToolMetal(Color color, float? roughnessOverride = null)
        => Create(LegoMaterialFamily.ToolMetal, color, roughnessOverride: roughnessOverride);

    public static StandardMaterial3D Rubber(Color color)
        => Create(LegoMaterialFamily.Rubber, color);

    public static StandardMaterial3D TransparentPolymer(Color color)
        => Create(LegoMaterialFamily.TransparentPolymer, color);

    public static StandardMaterial3D Crystal(Color color, float emissionEnergy = 1.35f)
        => Create(LegoMaterialFamily.Crystal, color, emissionEnergy);

    public static StandardMaterial3D Terrain(Color color)
        => Create(LegoMaterialFamily.Terrain, color);

    public static bool IsCanonicalDraftProfile(LegoMaterialFamily family, StandardMaterial3D material)
    {
        if (!Mathf.IsEqualApprox(material.Roughness, DefaultRoughness(family))) return false;
        return family switch
        {
            LegoMaterialFamily.MoldedPolymer => material.Metallic < 0.01f && material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled,
            LegoMaterialFamily.ToolMetal => material.Metallic >= 0.75f && material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled,
            LegoMaterialFamily.Rubber => material.Metallic < 0.01f && material.Roughness > MoldedPolymerRoughness,
            LegoMaterialFamily.TransparentPolymer => material.Transparency == BaseMaterial3D.TransparencyEnum.Alpha && material.AlbedoColor.A < 1f,
            LegoMaterialFamily.Crystal => material.Transparency == BaseMaterial3D.TransparencyEnum.Alpha && material.EmissionEnabled && material.EmissionEnergyMultiplier > 1f,
            LegoMaterialFamily.Terrain => material.Metallic < 0.01f && material.Roughness >= 0.85f,
            _ => false
        };
    }

    private static float DefaultRoughness(LegoMaterialFamily family) => family switch
    {
        LegoMaterialFamily.MoldedPolymer => MoldedPolymerRoughness,
        LegoMaterialFamily.ToolMetal => ToolMetalRoughness,
        LegoMaterialFamily.Rubber => RubberRoughness,
        LegoMaterialFamily.TransparentPolymer => TransparentPolymerRoughness,
        LegoMaterialFamily.Crystal => CrystalRoughness,
        LegoMaterialFamily.Terrain => TerrainRoughness,
        _ => MoldedPolymerRoughness
    };

    private static Color NormalizeAlpha(LegoMaterialFamily family, Color color)
    {
        if (family is not (LegoMaterialFamily.TransparentPolymer or LegoMaterialFamily.Crystal))
            return new Color(color.R, color.G, color.B, 1f);
        float fallback = family == LegoMaterialFamily.Crystal ? 0.78f : 0.58f;
        return new Color(color.R, color.G, color.B, color.A < 0.999f ? color.A : fallback);
    }
}

/// <summary>
/// Initial look-development swatches derived from the canonical named color
/// families. Exact production color matching remains an art-direction decision.
/// </summary>
public static class M7DraftPalette
{
    public static readonly Color RockRaiderDarkTurquoise = new("087f78");
    public static readonly Color AstronautFieldBlue = new("4c83b7");
    public static readonly Color AstronautMissionWhite = new("e8e6dc");
    public static readonly Color AstronautMissionOrange = new("e96f18");
    public static readonly Color AlienBlack = new("15181a");
    public static readonly Color AlienLime = new("8bd11f");
    public static readonly Color MartianSandPurple = new("8d7290");
    public static readonly Color MartianSandRed = new("9d625b");
    public static readonly Color MartianTan = new("b89e72");
    public static readonly Color Basalt = new("343536");
    public static readonly Color ToolSteel = new("a7afb5");
    public static readonly Color Rubber = new("17191a");
    public static readonly Color TransparentBrown = new(0.34f, 0.19f, 0.10f, 0.56f);
    public static readonly Color CrystalGreen = new(0.40f, 1.00f, 0.38f, 0.78f);
    public static readonly Color TeamIdentification = new("46a9dc");
}
