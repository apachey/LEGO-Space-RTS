using Godot;

namespace LegoSpaceRTS.Presentation;

public enum M7LookMaterialRole : byte
{
    PaintedHull,
    StructuralEarth,
    Accent,
    DarkMechanic,
    ToolSteel,
    Rubber,
    CanopyGlass,
    Signal,
    Lamp,
    BuildingShell,
    GroundRock,
    Crystal
}

public static class M7LookMaterialFactory
{
    private const string PaintedMacroTexturePath = "res://Assets/M7/Textures/painted_shell_macro_v2.png";
    private const string PaintedReliefTexturePath = "res://Assets/M7/Textures/painted_shell_detail.png";
    private const string MetalTexturePath = "res://Assets/M7/Textures/brushed_metal_detail.png";
    private const string RubberTexturePath = "res://Assets/M7/Textures/rubber_detail.png";
    private const string GroundTexturePath = "res://Assets/M7/Textures/quarry_ground_detail.png";
    private const string GroundAlbedoTexturePath = "res://Assets/M7/Textures/regolith_surface_v2.png";
    // The quiet generated regolith is reserved for colour. Relief and
    // reflection breakup use separately transformed quarry samples so no
    // single photograph is stamped into every output channel.
    private const string RegolithTexturePath = GroundAlbedoTexturePath;
    private const string MachinePanelTexturePath = "res://Assets/M7/Textures/machine_panel_height.png";
    private static Shader? _surfaceShaderResource;
    private static Shader? _groundShaderResource;
    private static Shader? _glassShaderResource;
    private static Shader? _opaqueEmissiveShaderResource;
    private static Shader? _transparentEmissiveShaderResource;

    private static Shader SurfaceShaderResource => _surfaceShaderResource ??= new Shader { Code = SurfaceShader };
    private static Shader GroundShaderResource => _groundShaderResource ??= new Shader { Code = GroundShader };
    private static Shader GlassShaderResource => _glassShaderResource ??= new Shader { Code = GlassShader };
    private static Shader OpaqueEmissiveShaderResource => _opaqueEmissiveShaderResource ??= new Shader { Code = OpaqueEmissiveShader };
    private static Shader TransparentEmissiveShaderResource => _transparentEmissiveShaderResource ??= new Shader { Code = TransparentEmissiveShader };

    public static M7LookMaterialRole InferRole(string nodeName)
    {
        if (nodeName is "Neutral_Chassis" or "Body_RearHousing") return M7LookMaterialRole.StructuralEarth;
        if (nodeName.StartsWith("Body_", StringComparison.Ordinal)) return M7LookMaterialRole.PaintedHull;
        if (nodeName.StartsWith("Accent_", StringComparison.Ordinal)) return M7LookMaterialRole.Accent;
        if (nodeName.StartsWith("Tool_", StringComparison.Ordinal)) return M7LookMaterialRole.ToolSteel;
        if (nodeName.StartsWith("Rubber_", StringComparison.Ordinal)) return M7LookMaterialRole.Rubber;
        if (nodeName.StartsWith("Glass_", StringComparison.Ordinal)) return M7LookMaterialRole.CanopyGlass;
        if (nodeName.StartsWith("Signal_", StringComparison.Ordinal)) return M7LookMaterialRole.Signal;
        if (nodeName.StartsWith("Lamp_", StringComparison.Ordinal)) return M7LookMaterialRole.Lamp;
        return M7LookMaterialRole.DarkMechanic;
    }

    public static Dictionary<M7LookMaterialRole, Material> BuildSharedMaterials(M7LookProfile profile)
    {
        profile.Normalize();
        return new Dictionary<M7LookMaterialRole, Material>
        {
            [M7LookMaterialRole.PaintedHull] = AuthoredOpaque(profile.Materials.PaintedHull, profile.Shading, PaintedMacroTexturePath, PaintedReliefTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.PaintedHull),
            [M7LookMaterialRole.StructuralEarth] = AuthoredOpaque(profile.Materials.StructuralEarth, profile.Shading, PaintedMacroTexturePath, PaintedReliefTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.StructuralEarth),
            [M7LookMaterialRole.Accent] = AuthoredOpaque(profile.Materials.Accent, profile.Shading, PaintedMacroTexturePath, PaintedReliefTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.Accent),
            [M7LookMaterialRole.DarkMechanic] = AuthoredOpaque(profile.Materials.DarkMechanic, profile.Shading, MetalTexturePath, MachinePanelTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.DarkMechanic),
            [M7LookMaterialRole.ToolSteel] = AuthoredOpaque(profile.Materials.ToolSteel, profile.Shading, MetalTexturePath, MetalTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.ToolSteel),
            [M7LookMaterialRole.Rubber] = AuthoredOpaque(profile.Materials.Rubber, profile.Shading, RubberTexturePath, RubberTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.Rubber),
            [M7LookMaterialRole.BuildingShell] = AuthoredOpaque(profile.Materials.BuildingShell, profile.Shading, PaintedMacroTexturePath, PaintedMacroTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.BuildingShell),
            [M7LookMaterialRole.GroundRock] = Ground(profile),
            [M7LookMaterialRole.CanopyGlass] = Glass(profile.Glass),
            [M7LookMaterialRole.Signal] = Emissive(ParseColor(profile.Emission.SignalColor), profile.Emission.SignalEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Lamp] = Emissive(ParseColor(profile.Emission.LampColor), profile.Emission.LampEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Crystal] = Emissive(ParseColor(profile.Emission.CrystalColor), profile.Emission.CrystalEnergy, 1f, profile.Emission.EdgeDarkening)
        };
    }

    public static void SetTextureAnchor(MeshInstance3D mesh, Transform3D modelSpace)
    {
        mesh.SetInstanceShaderParameter("texture_offset", modelSpace.Origin);
        mesh.SetInstanceShaderParameter("texture_axis_x", modelSpace.Basis.X);
        mesh.SetInstanceShaderParameter("texture_axis_y", modelSpace.Basis.Y);
        mesh.SetInstanceShaderParameter("texture_axis_z", modelSpace.Basis.Z);
    }

    public static bool ValidateAuthoredTextureBindings(
        IReadOnlyDictionary<M7LookMaterialRole, Material> materials, out string error)
    {
        (M7LookMaterialRole Role, string Detail, string Height)[] opaqueExpectations =
        {
            (M7LookMaterialRole.PaintedHull, PaintedMacroTexturePath, PaintedReliefTexturePath),
            (M7LookMaterialRole.StructuralEarth, PaintedMacroTexturePath, PaintedReliefTexturePath),
            (M7LookMaterialRole.Accent, PaintedMacroTexturePath, PaintedReliefTexturePath),
            (M7LookMaterialRole.DarkMechanic, MetalTexturePath, MachinePanelTexturePath),
            (M7LookMaterialRole.ToolSteel, MetalTexturePath, MetalTexturePath),
            (M7LookMaterialRole.Rubber, RubberTexturePath, RubberTexturePath),
            (M7LookMaterialRole.BuildingShell, PaintedMacroTexturePath, PaintedMacroTexturePath)
        };
        foreach ((M7LookMaterialRole role, string detailPath, string heightPath) in opaqueExpectations)
        {
            bool requiresAlbedoTexture = role is M7LookMaterialRole.PaintedHull or
                M7LookMaterialRole.StructuralEarth or M7LookMaterialRole.Accent or
                M7LookMaterialRole.BuildingShell;
            if (!materials.TryGetValue(role, out Material? raw) || raw is not ShaderMaterial material ||
                !TextureParameterMatches(material, "detail_texture", detailPath) ||
                !TextureParameterMatches(material, "height_texture", heightPath) ||
                (requiresAlbedoTexture && material.GetShaderParameter("texture_strength").As<double>() <= 0.0) ||
                material.GetShaderParameter("relief_strength").As<double>() <= 0.0 ||
                material.GetShaderParameter("roughness_variation").As<double>() <= 0.0)
            {
                error = $"Authored material texture routing is invalid for {role}.";
                return false;
            }

            // The raster can be technically bound yet disappear after RTS-scale
            // mip filtering. Keep a perceptual floor for the four macro-painted
            // families without relaxing the aggressive filter that prevents
            // high-frequency shimmer. This is deliberately not applied to metal
            // or rubber, whose texture is carried primarily by reflection/relief.
            if (requiresAlbedoTexture &&
                (material.GetShaderParameter("texture_strength").As<double>() < 0.20 ||
                 material.GetShaderParameter("detail_contrast").As<double>() < 7.0 ||
                 material.GetShaderParameter("detail_filter_width").As<double>() < 12.0))
            {
                error = $"Authored macro-albedo is too weak or insufficiently filtered for {role}.";
                return false;
            }
        }

        if (!materials.TryGetValue(M7LookMaterialRole.GroundRock, out Material? groundRaw) ||
            groundRaw is not ShaderMaterial ground ||
            !TextureParameterMatches(ground, "albedo_texture", RegolithTexturePath) ||
            !TextureParameterMatches(ground, "height_texture", GroundTexturePath) ||
            !TextureParameterMatches(ground, "detail_texture_b", GroundTexturePath) ||
            ground.GetShaderParameter("surface_treatment").As<int>() is < 0 or > 1 ||
            ground.GetShaderParameter("authored_zone_revision").As<double>() < 1.0 ||
            ground.GetShaderParameter("albedo_variation").As<double>() <= 0.0 ||
            ground.GetShaderParameter("relief_strength").As<double>() <= 0.0 ||
            ground.GetShaderParameter("roughness_variation").As<double>() <= 0.0)
        {
            error = "Authored ground texture routing is invalid.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool ValidateGroundTreatmentModes(out string error)
    {
        M7LookProfile authoredProfile = M7LookProfile.CreateDefault();
        authoredProfile.Ground.SurfaceTreatment = M7GroundSurfaceTreatment.AuthoredSurfaceStack;
        M7LookProfile legacyProfile = M7LookProfile.CreateDefault();
        legacyProfile.Ground.SurfaceTreatment = M7GroundSurfaceTreatment.LegacyRaster;
        ShaderMaterial authored = Ground(authoredProfile);
        ShaderMaterial legacy = Ground(legacyProfile);
        if (authored.GetShaderParameter("surface_treatment").As<int>() !=
                (int)M7GroundSurfaceTreatment.AuthoredSurfaceStack ||
            legacy.GetShaderParameter("surface_treatment").As<int>() !=
                (int)M7GroundSurfaceTreatment.LegacyRaster ||
            authored.GetShaderParameter("authored_zone_revision").As<double>() < 1.0 ||
            !TextureParameterMatches(authored, "albedo_texture", RegolithTexturePath) ||
            !TextureParameterMatches(legacy, "albedo_texture", RegolithTexturePath))
        {
            error = "Authored/legacy ground treatment routing is invalid.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TextureParameterMatches(ShaderMaterial material, string parameter, string path)
    {
        Variant value = material.GetShaderParameter(parameter);
        return value.VariantType == Variant.Type.Object &&
            value.As<GodotObject>() is Texture2D texture && texture.ResourcePath == path;
    }

    public static ShaderMaterial Opaque(MaterialLook look, ShadingLook shading, string texturePath, int inspectionPass = 0)
    {
        ShaderMaterial material = new() { Shader = SurfaceShaderResource };
        material.SetShaderParameter("base_color", ParseColor(look.BaseColor));
        material.SetShaderParameter("metallic_value", look.Metallic);
        material.SetShaderParameter("roughness_value", look.Roughness);
        material.SetShaderParameter("specular_value", look.Specular * shading.GlobalSpecular);
        material.SetShaderParameter("clearcoat_value", look.Clearcoat);
        material.SetShaderParameter("clearcoat_roughness", look.ClearcoatRoughness);
        material.SetShaderParameter("fresnel_value", look.Fresnel);
        material.SetShaderParameter("micro_strength", look.MicroStrength);
        material.SetShaderParameter("micro_scale", look.MicroScale);
        material.SetShaderParameter("macro_variation", look.MacroVariation);
        material.SetShaderParameter("edge_wear", look.EdgeWear);
        material.SetShaderParameter("dust_amount", look.DustAmount);
        material.SetShaderParameter("brushed_amount", look.BrushedAmount);
        material.SetShaderParameter("detail_texture", GD.Load<Texture2D>(texturePath));
        material.SetShaderParameter("height_texture", GD.Load<Texture2D>(texturePath));
        material.SetShaderParameter("texture_strength", look.TextureStrength);
        material.SetShaderParameter("texture_scale", look.TextureScale);
        material.SetShaderParameter("height_scale", look.TextureScale);
        material.SetShaderParameter("detail_center", 0.5f);
        material.SetShaderParameter("detail_contrast", 2f);
        material.SetShaderParameter("height_center", 0.5f);
        material.SetShaderParameter("height_contrast", 2f);
        material.SetShaderParameter("relief_strength", look.ReliefStrength);
        material.SetShaderParameter("roughness_variation", look.RoughnessVariation);
        material.SetShaderParameter("texture_blend_mode", (float)look.TextureBlendMode);
        material.SetShaderParameter("inspection_pass", inspectionPass);
        material.SetShaderParameter("diffuse_wrap", shading.DiffuseWrap);
        material.SetShaderParameter("shadow_floor", shading.ShadowFloor);
        material.SetShaderParameter("light_bands", (float)shading.LightBands);
        material.SetShaderParameter("band_softness", shading.BandSoftness);
        material.SetShaderParameter("rim_strength", shading.RimStrength);
        material.SetShaderParameter("rim_width", shading.RimWidth);
        material.SetShaderParameter("shadow_tint", ParseColor(shading.ShadowTint));
        material.SetShaderParameter("highlight_tint", ParseColor(shading.HighlightTint));
        return material;
    }

    private static ShaderMaterial AuthoredOpaque(
        MaterialLook look,
        ShadingLook shading,
        string detailTexturePath,
        string heightTexturePath,
        int inspectionPass,
        M7LookMaterialRole role)
    {
        ShaderMaterial material = Opaque(look, shading, detailTexturePath, inspectionPass);
        SurfaceRecipe recipe = role switch
        {
            M7LookMaterialRole.PaintedHull => new(0.03f, 0.42f, 0.48f, 0.18f, 0.28f, 0.18f, 0.00f,
                0.10f, 0.10f, 0.505f, 14f, 0.517f, 8f, 0.34f, 0.030f, 0.0015f, 16f, 6f),
            M7LookMaterialRole.StructuralEarth => new(0.02f, 0.72f, 0.30f, 0.04f, 0.55f, 0.12f, 0.00f,
                0.09f, 0.09f, 0.505f, 13f, 0.517f, 8f, 0.36f, 0.035f, 0.002f, 16f, 6f),
            M7LookMaterialRole.Accent => new(0.00f, 0.34f, 0.50f, 0.22f, 0.24f, 0.18f, 0.00f,
                0.11f, 0.11f, 0.505f, 12f, 0.517f, 6f, 0.26f, 0.020f, 0.001f, 16f, 6f),
            M7LookMaterialRole.DarkMechanic => new(0.72f, 0.42f, 0.68f, 0.02f, 0.50f, 0.25f, 0.08f,
                0.14f, 0.09f, 0.552f, 10f, 0.504f, 10f, 0.010f, 0.050f, 0.004f, 2.8f, 4f),
            M7LookMaterialRole.ToolSteel => new(0.92f, 0.30f, 0.72f, 0.02f, 0.40f, 0.26f, 0.12f,
                0.18f, 0.18f, 0.552f, 8f, 0.552f, 5f, 0.0f, 0.045f, 0.0003f, 2.8f, 8f),
            M7LookMaterialRole.Rubber => new(0.00f, 0.88f, 0.12f, 0.00f, 0.80f, 0.05f, 0.00f,
                0.15f, 0.15f, 0.281f, 6f, 0.281f, 5f, 0.010f, 0.030f, 0.002f, 4f, 6f),
            M7LookMaterialRole.BuildingShell => new(0.08f, 0.55f, 0.40f, 0.08f, 0.38f, 0.15f, 0.00f,
                0.075f, 0.07f, 0.505f, 11f, 0.505f, 3f, 0.30f, 0.012f, 0.0004f, 20f, 6f),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
        material.SetShaderParameter("metallic_value", recipe.Metallic);
        material.SetShaderParameter("roughness_value", recipe.Roughness);
        material.SetShaderParameter("specular_value", recipe.Specular * shading.GlobalSpecular);
        material.SetShaderParameter("clearcoat_value", recipe.Clearcoat);
        material.SetShaderParameter("clearcoat_roughness", recipe.ClearcoatRoughness);
        material.SetShaderParameter("fresnel_value", recipe.Fresnel);
        material.SetShaderParameter("micro_strength", 0f);
        material.SetShaderParameter("micro_scale", 1f);
        material.SetShaderParameter("macro_variation", 0f);
        material.SetShaderParameter("edge_wear", 0f);
        material.SetShaderParameter("dust_amount", 0f);
        material.SetShaderParameter("brushed_amount", recipe.BrushedAmount);
        material.SetShaderParameter("detail_texture", GD.Load<Texture2D>(detailTexturePath));
        material.SetShaderParameter("height_texture", GD.Load<Texture2D>(heightTexturePath));
        material.SetShaderParameter("texture_strength", recipe.AlbedoVariation);
        material.SetShaderParameter("texture_scale", recipe.DetailScale);
        material.SetShaderParameter("height_scale", recipe.HeightScale);
        material.SetShaderParameter("detail_center", recipe.DetailCenter);
        material.SetShaderParameter("detail_contrast", recipe.DetailContrast);
        material.SetShaderParameter("height_center", recipe.HeightCenter);
        material.SetShaderParameter("height_contrast", recipe.HeightContrast);
        material.SetShaderParameter("detail_filter_width", recipe.DetailFilterWidth);
        material.SetShaderParameter("height_filter_width", recipe.HeightFilterWidth);
        material.SetShaderParameter("groove_darkening", role == M7LookMaterialRole.DarkMechanic ? 0.015f : 0f);
        material.SetShaderParameter("groove_roughness", role == M7LookMaterialRole.DarkMechanic ? 0.025f : 0f);
        material.SetShaderParameter("relief_strength", recipe.BumpStrength);
        material.SetShaderParameter("roughness_variation", recipe.RoughnessVariation);
        material.SetShaderParameter("texture_blend_mode", 0f);
        return material;
    }

    public static ShaderMaterial Ground(M7LookProfile profile)
    {
        MaterialLook look = profile.Materials.GroundRock;
        ShaderMaterial material = new() { Shader = GroundShaderResource };
        material.SetShaderParameter("base_color", ParseColor(look.BaseColor));
        material.SetShaderParameter("secondary_color", ParseColor(profile.Ground.SecondaryColor));
        material.SetShaderParameter("roughness_value", 0.94f);
        material.SetShaderParameter("macro_amount", profile.Ground.MacroAmount);
        material.SetShaderParameter("macro_scale", profile.Ground.MacroScale);
        material.SetShaderParameter("micro_amount", profile.Ground.MicroAmount);
        material.SetShaderParameter("micro_scale", profile.Ground.MicroScale);
        // Keep the quiet generated regolith in the colour channel. Quarry is
        // sampled independently for height and roughness so the ground gains
        // material response without stamping the same photograph into colour,
        // bump and reflection at identical coordinates.
        material.SetShaderParameter("albedo_texture", GD.Load<Texture2D>(RegolithTexturePath));
        material.SetShaderParameter("height_texture", GD.Load<Texture2D>(GroundTexturePath));
        material.SetShaderParameter("detail_texture_b", GD.Load<Texture2D>(GroundTexturePath));
        material.SetShaderParameter("albedo_scale", 0.055f);
        material.SetShaderParameter("height_scale", 0.20f);
        material.SetShaderParameter("roughness_scale", 0.31f);
        material.SetShaderParameter("albedo_center", 0.548f);
        material.SetShaderParameter("height_center", 0.551f);
        material.SetShaderParameter("roughness_center", 0.551f);
        material.SetShaderParameter("albedo_contrast", 16f);
        material.SetShaderParameter("height_contrast", 6f);
        material.SetShaderParameter("roughness_contrast", 14f);
        material.SetShaderParameter("albedo_variation", 0.055f);
        material.SetShaderParameter("relief_strength", 0.018f);
        material.SetShaderParameter("roughness_variation", 0.080f);
        material.SetShaderParameter("surface_treatment", (int)profile.Ground.SurfaceTreatment);
        material.SetShaderParameter("authored_zone_revision", 1f);
        material.SetShaderParameter("inspection_pass", profile.Materials.InspectionPass);
        material.SetShaderParameter("background_color", ParseColor(profile.Lighting.BackgroundColor));
        material.SetShaderParameter("background_influence", profile.Lighting.BackgroundInfluence);
        return material;
    }

    public static ShaderMaterial Glass(GlassLook glass)
    {
        ShaderMaterial material = new() { Shader = GlassShaderResource };
        material.SetShaderParameter("glass_tint", ParseColor(glass.Tint));
        material.SetShaderParameter("opacity", glass.Opacity);
        material.SetShaderParameter("roughness_value", glass.Roughness);
        material.SetShaderParameter("ior", glass.Ior);
        material.SetShaderParameter("refraction_strength", glass.RefractionStrength);
        material.SetShaderParameter("edge_brightness", glass.EdgeBrightness);
        return material;
    }

    public static ShaderMaterial Emissive(Color color, float energy, float alpha = 1f, float edgeDarkening = 0.45f)
    {
        bool transparent = alpha < 0.999f;
        ShaderMaterial material = new()
        {
            Shader = transparent ? TransparentEmissiveShaderResource : OpaqueEmissiveShaderResource
        };
        material.SetShaderParameter("emission_color", color);
        material.SetShaderParameter("emission_energy", energy);
        if (transparent) material.SetShaderParameter("alpha_value", alpha);
        material.SetShaderParameter("edge_darkening", edgeDarkening);
        return material;
    }

    private readonly record struct SurfaceRecipe(
        float Metallic,
        float Roughness,
        float Specular,
        float Clearcoat,
        float ClearcoatRoughness,
        float Fresnel,
        float BrushedAmount,
        float DetailScale,
        float HeightScale,
        float DetailCenter,
        float DetailContrast,
        float HeightCenter,
        float HeightContrast,
        float AlbedoVariation,
        float RoughnessVariation,
        float BumpStrength,
        float DetailFilterWidth,
        float HeightFilterWidth);

    public static StandardMaterial3D Transparent(Color color) => new()
    {
        AlbedoColor = color,
        Roughness = 0.9f,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        NoDepthTest = true
    };

    public static Color ParseColor(string value)
    {
        if (Color.HtmlIsValid(value)) return Color.FromHtml(value);
        return Colors.Magenta;
    }

    private const string SurfaceShader = """
shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float metallic_value : hint_range(0.0, 1.0) = 0.0;
uniform float roughness_value : hint_range(0.0, 1.0) = 0.5;
uniform float specular_value : hint_range(0.0, 2.0) = 0.5;
uniform float clearcoat_value : hint_range(0.0, 1.0) = 0.0;
uniform float clearcoat_roughness : hint_range(0.0, 1.0) = 0.2;
uniform float fresnel_value : hint_range(0.0, 1.0) = 0.35;
uniform float micro_strength : hint_range(0.0, 1.0) = 0.05;
uniform float micro_scale = 5.0;
uniform float macro_variation : hint_range(0.0, 1.0) = 0.08;
uniform float edge_wear : hint_range(0.0, 1.0) = 0.05;
uniform float dust_amount : hint_range(0.0, 1.0) = 0.05;
uniform float brushed_amount : hint_range(0.0, 1.0) = 0.0;
uniform sampler2D detail_texture : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D height_texture : filter_linear_mipmap_anisotropic, repeat_enable;
uniform float texture_strength : hint_range(0.0, 1.0) = 0.0;
uniform float texture_scale = 1.0;
uniform float height_scale = 1.0;
uniform float detail_center = 0.5;
uniform float detail_contrast = 2.0;
uniform float height_center = 0.5;
uniform float height_contrast = 2.0;
uniform float detail_filter_width : hint_range(1.0, 16.0) = 1.0;
uniform float height_filter_width : hint_range(1.0, 16.0) = 1.0;
uniform float groove_darkening : hint_range(0.0, 0.2) = 0.0;
uniform float groove_roughness : hint_range(0.0, 0.2) = 0.0;
uniform float relief_strength : hint_range(0.0, 1.5) = 0.12;
uniform float roughness_variation : hint_range(0.0, 1.0) = 0.18;
uniform float texture_blend_mode : hint_range(0.0, 2.0) = 0.0;
uniform int inspection_pass : hint_range(0, 4) = 0;
uniform float diffuse_wrap : hint_range(0.0, 1.0) = 0.15;
uniform float shadow_floor : hint_range(0.0, 1.0) = 0.12;
uniform float light_bands : hint_range(0.0, 8.0) = 0.0;
uniform float band_softness : hint_range(0.0, 1.0) = 0.3;
uniform float rim_strength : hint_range(0.0, 2.0) = 0.15;
uniform float rim_width : hint_range(0.05, 1.0) = 0.35;
uniform vec4 shadow_tint : source_color = vec4(0.15, 0.2, 0.28, 1.0);
uniform vec4 highlight_tint : source_color = vec4(1.0, 0.95, 0.86, 1.0);
varying vec3 local_position;
varying vec3 local_normal;
varying vec3 view_position;
varying vec3 world_normal;
instance uniform vec3 texture_offset = vec3(0.0);
instance uniform vec3 texture_axis_x = vec3(1.0, 0.0, 0.0);
instance uniform vec3 texture_axis_y = vec3(0.0, 1.0, 0.0);
instance uniform vec3 texture_axis_z = vec3(0.0, 0.0, 1.0);

float filtered_detail(vec2 uv) {
    // Bias automatic mip selection toward the large authored shapes. This is
    // still derivative-aware as the RTS camera moves, but fine scratches can
    // no longer collapse into screen-space glitter on units and buildings.
    return textureGrad(detail_texture, uv,
        dFdx(uv) * detail_filter_width,
        dFdy(uv) * detail_filter_width).r;
}

float filtered_height(vec2 uv) {
    return textureGrad(height_texture, uv,
        dFdx(uv) * height_filter_width,
        dFdy(uv) * height_filter_width).r;
}

float triplanar_detail(vec3 p, vec3 n) {
    vec3 weights = pow(abs(n), vec3(4.0));
    weights /= max(0.0001, weights.x + weights.y + weights.z);
    // Automatic derivative-aware mip selection is essential here. A fixed
    // LOD made the same texture either sparkle or smear as the RTS camera
    // moved, while these coordinates remain locked to the object.
    return filtered_detail(p.yz) * weights.x
        + filtered_detail(p.xz) * weights.y
        + filtered_detail(p.xy) * weights.z;
}

float triplanar_height(vec3 p, vec3 n) {
    vec3 weights = pow(abs(n), vec3(4.0));
    weights /= max(0.0001, weights.x + weights.y + weights.z);
    return filtered_height(p.yz) * weights.x
        + filtered_height(p.xz) * weights.y
        + filtered_height(p.xy) * weights.z;
}

float authored_mask(float sample_value, float center, float contrast_value) {
    // The source maps are intentionally neutral. Their useful 64–128 px mips
    // occupy only a few code values around the mean, so applying another tiny
    // alpha made them mathematically invisible. Centre/contrast converts each
    // map into a bounded material mask before the per-channel artistic amount
    // is applied. This preserves automatic mip filtering and cannot amplify a
    // channel beyond its authored maximum.
    return clamp((sample_value - center) * contrast_value, -1.0, 1.0);
}

vec3 relief_normal(vec3 base_normal, float height_value, float strength) {
    // NORMAL and LIGHT are view-space in fragment(). Derivatives must use a
    // view-space position too; mixing them with object-space VERTEX produced
    // camera-dependent bumps and occasional zero/NaN normals.
    vec3 dpdx = dFdx(view_position);
    vec3 dpdy = dFdy(view_position);
    float dhdx = dFdx(height_value);
    float dhdy = dFdy(height_value);
    vec3 r1 = cross(dpdy, base_normal);
    vec3 r2 = cross(base_normal, dpdx);
    float determinant = dot(dpdx, r1);
    float safe_determinant = max(abs(determinant), 0.00001);
    vec3 gradient = sign(determinant) * (dhdx * r1 + dhdy * r2) / safe_determinant;
    vec3 candidate = base_normal - gradient * strength;
    return candidate / max(length(candidate), 0.0001);
}

void vertex() {
    // The reviewed GLB uses unit node scales. Keeping sampling coordinates in
    // object space makes texture features follow yaw and authored animation;
    // relief derivatives below remain in view space to match fragment NORMAL.
    // Transform every child mesh into one model-wide material space. Sampling
    // every LEGO module around its own local origin made even a strong raster
    // look uniform on units while large one-piece buildings showed it clearly.
    // Carrying all three axes also keeps rotated/offset parts from stretching
    // or changing the triplanar direction independently.
    local_position = texture_axis_x * VERTEX.x
        + texture_axis_y * VERTEX.y
        + texture_axis_z * VERTEX.z
        + texture_offset;
    local_normal = normalize(normalize(texture_axis_x) * NORMAL.x
        + normalize(texture_axis_y) * NORMAL.y
        + normalize(texture_axis_z) * NORMAL.z);
    view_position = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

void fragment() {
    vec3 mapping_normal = normalize(local_normal);
    float color_sample = triplanar_detail(local_position * texture_scale, mapping_normal);
    // A rotated, broader lookup decorrelates reflection breakup from colour.
    // The previous higher-frequency octave was the main remaining source of
    // shimmer at gameplay zoom, particularly across large building faces.
    vec3 reflection_position = local_position * (texture_scale * 0.58)
        + vec3(0.37, 0.61, 0.19);
    float reflection_sample = triplanar_detail(reflection_position.zyx, mapping_normal.zyx);
    float height_sample = triplanar_height(local_position * height_scale, mapping_normal);
    float color_mask = authored_mask(color_sample, detail_center, detail_contrast);
    float reflection_mask = authored_mask(reflection_sample, detail_center, detail_contrast * 0.72);
    float height_mask = authored_mask(height_sample, height_center, height_contrast);
    float brush = sin((local_position.z + local_position.x * 0.17) * 9.0) * 0.5 + 0.5;
    if (inspection_pass == 0) {
        NORMAL = relief_normal(normalize(NORMAL), height_mask, relief_strength);
    } else if (inspection_pass == 3) {
        NORMAL = relief_normal(normalize(NORMAL), height_mask, max(relief_strength, 0.028));
    } else {
        NORMAL = normalize(NORMAL);
    }
    float normal_view_dot = clamp(dot(normalize(NORMAL), normalize(VIEW)), -1.0, 1.0);
    float upward = clamp((world_normal.y + 1.0) * 0.5, 0.0, 1.0);
    vec3 color = base_color.rgb;
    if (inspection_pass == 0) {
        color *= 1.0 + color_mask * texture_strength;
        color *= 1.0 - max(-height_mask, 0.0) * groove_darkening;
        color = mix(color, color * vec3(1.13, 1.10, 1.04), edge_wear * pow(clamp(1.0 - abs(normal_view_dot), 0.0, 1.0), 3.0));
        color = mix(color, shadow_tint.rgb, dust_amount * upward * 0.18);
    } else if (inspection_pass == 2) {
        color = base_color.rgb * (1.0 + color_mask * 0.34);
    } else if (inspection_pass >= 3) {
        color = vec3(0.46);
    }
    ALBEDO = color;
    METALLIC = inspection_pass >= 3 ? 0.0 : metallic_value;
    float reflection_breakup = reflection_mask * roughness_variation
        - (brush - 0.5) * brushed_amount * 0.18
        + max(-height_mask, 0.0) * groove_roughness;
    ROUGHNESS = inspection_pass == 4
        ? mix(0.16, 0.92, reflection_mask * 0.5 + 0.5)
        : clamp(roughness_value + (inspection_pass == 0 ? reflection_breakup : 0.0), 0.03, 1.0);
    SPECULAR = clamp(specular_value * mix(1.0 - fresnel_value * 0.25, 1.0 + fresnel_value * 0.25, pow(clamp(1.0 - normal_view_dot, 0.0, 1.0), 3.0)), 0.0, 1.0);
    CLEARCOAT = clearcoat_value;
    CLEARCOAT_ROUGHNESS = clearcoat_roughness;
}

void light() {
    float ndl = dot(NORMAL, LIGHT);
    float wrapped = clamp((ndl + diffuse_wrap) / (1.0 + diffuse_wrap), 0.0, 1.0);
    if (light_bands > 1.5) {
        float steps = max(2.0, floor(light_bands + 0.5));
        float quantized = floor(wrapped * steps) / max(1.0, steps - 1.0);
        wrapped = mix(quantized, wrapped, band_softness);
    }
    // Directional shadows retain a controlled readability floor. Local lights
    // must obey their real cone/range attenuation or headlights leave a flat
    // residual wash across the whole clustered light volume.
    float diffuse_attenuation = LIGHT_IS_DIRECTIONAL
        ? mix(shadow_floor, 1.0, ATTENUATION)
        : ATTENUATION;
    vec3 shadow_color = LIGHT_IS_DIRECTIONAL
        ? mix(shadow_tint.rgb, vec3(1.0), ATTENUATION)
        : vec3(1.0);
    float inspection_metallic = inspection_pass >= 3 ? 0.0 : metallic_value;
    DIFFUSE_LIGHT += LIGHT_COLOR * shadow_color * wrapped * diffuse_attenuation * (1.0 - inspection_metallic * 0.72);

    vec3 half_sum = LIGHT + VIEW;
    vec3 half_vector = half_sum / max(length(half_sum), 0.0001);
    float front_lit = step(0.0, ndl);
    float ndh = clamp(dot(NORMAL, half_vector), 0.0, 1.0);
    float vdh = clamp(dot(VIEW, half_vector), 0.0, 1.0);
    float spec_power = mix(180.0, 4.0, ROUGHNESS * ROUGHNESS);
    float lobe = pow(ndh, spec_power) * mix(0.35, 2.2, 1.0 - ROUGHNESS);
    vec3 dielectric_f0 = vec3(0.018 + clamp(specular_value, 0.0, 1.0) * 0.12);
    vec3 f0 = mix(dielectric_f0, ALBEDO, inspection_metallic);
    vec3 fresnel = f0 + (vec3(1.0) - f0) * pow(1.0 - vdh, mix(7.0, 3.0, fresnel_value));
    float coat_power = mix(220.0, 12.0, clearcoat_roughness);
    float coat = pow(ndh, coat_power) * clearcoat_value * 0.35;
    SPECULAR_LIGHT += LIGHT_COLOR * (fresnel * lobe + vec3(coat)) * ATTENUATION * front_lit;

    float rim = pow(clamp(1.0 - clamp(dot(NORMAL, VIEW), -1.0, 1.0), 0.0, 1.0), mix(7.0, 1.2, rim_width));
    DIFFUSE_LIGHT += highlight_tint.rgb * rim * rim_strength * 0.18 * ATTENUATION;
}
""";

    private const string GroundShader = """
shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_schlick_ggx;
uniform vec4 base_color : source_color;
uniform vec4 secondary_color : source_color;
uniform float roughness_value = 0.95;
uniform float macro_amount = 0.25;
uniform float macro_scale = 0.13;
uniform float micro_amount = 0.18;
uniform float micro_scale = 6.0;
uniform sampler2D albedo_texture : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D height_texture : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D detail_texture_b : filter_linear_mipmap_anisotropic, repeat_enable;
uniform float albedo_scale = 0.055;
uniform float height_scale = 0.20;
uniform float roughness_scale = 0.31;
uniform float albedo_center = 0.548;
uniform float height_center = 0.551;
uniform float roughness_center = 0.551;
uniform float albedo_contrast = 16.0;
uniform float height_contrast = 6.0;
uniform float roughness_contrast = 14.0;
uniform float albedo_variation = 0.055;
uniform float relief_strength = 0.45;
uniform float roughness_variation = 0.30;
uniform int surface_treatment : hint_range(0, 1) = 0;
uniform float authored_zone_revision = 1.0;
uniform int inspection_pass : hint_range(0, 4) = 0;
uniform vec4 background_color : source_color = vec4(0.08, 0.11, 0.14, 1.0);
uniform float background_influence = 0.15;
varying vec3 world_position;
varying vec3 view_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    view_position = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

float anti_tiled_albedo(vec2 uv) {
    mat2 rotate_a = mat2(vec2(0.866, 0.5), vec2(-0.5, 0.866));
    mat2 rotate_b = mat2(vec2(0.342, -0.940), vec2(0.940, 0.342));
    float a = texture(albedo_texture, uv).r;
    float b = texture(albedo_texture, rotate_a * uv * 0.713 + vec2(0.37, 0.19)).r;
    float c = texture(albedo_texture, rotate_b * uv * 0.517 + vec2(0.11, 0.63)).r;
    return clamp(a * 0.54 + b * 0.29 + c * 0.17, 0.0, 1.0);
}

float anti_tiled_height(vec2 uv) {
    mat2 rotate_a = mat2(vec2(0.866, 0.5), vec2(-0.5, 0.866));
    mat2 rotate_b = mat2(vec2(0.342, -0.940), vec2(0.940, 0.342));
    float a = texture(height_texture, uv).r;
    float b = texture(height_texture, rotate_a * uv * 0.713 + vec2(0.37, 0.19)).r;
    float c = texture(height_texture, rotate_b * uv * 0.617 + vec2(0.11, 0.63)).r;
    return clamp(a * 0.52 + b * 0.30 + c * 0.18, 0.0, 1.0);
}

float anti_tiled_roughness(vec2 uv) {
    // This is intentionally a different transform family from height. Quarry
    // therefore contributes believable mineral response without embossing the
    // exact same dark patch into every output channel.
    mat2 rotate_a = mat2(vec2(0.643, -0.766), vec2(0.766, 0.643));
    mat2 rotate_b = mat2(vec2(-0.174, 0.985), vec2(-0.985, -0.174));
    float a = texture(detail_texture_b, uv + vec2(0.43, 0.17)).r;
    float b = texture(detail_texture_b, rotate_a * uv * 0.821 + vec2(0.07, 0.71)).r;
    float c = texture(detail_texture_b, rotate_b * uv * 0.563 + vec2(0.68, 0.29)).r;
    return clamp(a * 0.46 + b * 0.34 + c * 0.20, 0.0, 1.0);
}

float ground_material_mask(float sample_value, float center, float contrast_value) {
    return clamp((sample_value - center) * contrast_value, -1.0, 1.0);
}

vec3 ground_relief_normal(vec3 base_normal, float height_value, float strength) {
    vec3 dpdx = dFdx(view_position);
    vec3 dpdy = dFdy(view_position);
    float dhdx = dFdx(height_value);
    float dhdy = dFdy(height_value);
    vec3 r1 = cross(dpdy, base_normal);
    vec3 r2 = cross(base_normal, dpdx);
    float determinant = dot(dpdx, r1);
    float safe_determinant = max(abs(determinant), 0.00001);
    vec3 gradient = sign(determinant) * (dhdx * r1 + dhdy * r2) / safe_determinant;
    vec3 candidate = base_normal - gradient * strength;
    return candidate / max(length(candidate), 0.0001);
}

float ground_hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float ground_noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 fraction = fract(p);
    vec2 smooth_fraction = fraction * fraction * (3.0 - 2.0 * fraction);
    float a = ground_hash(cell);
    float b = ground_hash(cell + vec2(1.0, 0.0));
    float c = ground_hash(cell + vec2(0.0, 1.0));
    float d = ground_hash(cell + vec2(1.0, 1.0));
    return mix(mix(a, b, smooth_fraction.x), mix(c, d, smooth_fraction.x), smooth_fraction.y);
}

float broad_ground_breakup(vec2 p) {
    vec2 warp = vec2(
        ground_noise(p * 0.37 + vec2(7.2, 2.1)),
        ground_noise(p * 0.29 + vec2(1.4, 9.7))) * 2.0 - 1.0;
    vec2 warped = p + warp * 1.35;
    float broad = ground_noise(warped);
    float medium = ground_noise(warped * 2.07 + vec2(4.3, 6.8));
    return (broad * 0.74 + medium * 0.26 - 0.5) * 2.0;
}

float authored_ellipse(vec2 p, vec2 center, vec2 radii, float feather) {
    float distance_value = length((p - center) / radii);
    return 1.0 - smoothstep(1.0 - feather, 1.0 + feather, distance_value);
}

float authored_segment_distance(vec2 p, vec2 start, vec2 end) {
    vec2 segment = end - start;
    float along = clamp(dot(p - start, segment) / max(dot(segment, segment), 0.0001), 0.0, 1.0);
    return length(p - (start + segment * along));
}

vec4 authored_zone_masks(vec2 p) {
    // This is a unique map-scale composition rather than another tiled noise
    // layer: a central compacted work pad, three travelled service routes,
    // two exposed bedrock shelves, and a mineral seam leading to the review
    // crystal cluster. All positions are fixed in world space and therefore
    // remain stable while the RTS camera zooms and rotates.
    p += vec2(authored_zone_revision - 1.0) * 0.0001;
    float pad = authored_ellipse(p, vec2(0.0, -0.8), vec2(15.5, 11.8), 0.12);
    float route_a = 1.0 - smoothstep(3.0, 4.8,
        authored_segment_distance(p, vec2(-4.0, -2.0), vec2(22.0, -13.5)));
    float route_b = 1.0 - smoothstep(2.5, 4.2,
        authored_segment_distance(p, vec2(-7.0, -3.5), vec2(-23.0, -16.0)));
    float route_c = 1.0 - smoothstep(2.2, 4.0,
        authored_segment_distance(p, vec2(-5.0, 4.5), vec2(-27.0, 18.0)));
    float compacted = max(pad, max(route_a, max(route_b, route_c)) * 0.88);

    vec2 shelf_warp = vec2(
        ground_noise(p * 0.043 + vec2(1.7, 7.4)),
        ground_noise(p * 0.037 + vec2(8.2, 2.6))) * 2.0 - 1.0;
    float bedrock_a = authored_ellipse(p + shelf_warp * 2.6,
        vec2(-18.0, 10.5), vec2(10.5, 6.2), 0.16);
    float bedrock_b = authored_ellipse(p - shelf_warp * 2.1,
        vec2(23.0, 12.5), vec2(12.0, 7.4), 0.18);
    float bedrock = max(bedrock_a, bedrock_b) * (1.0 - compacted * 0.62);

    float seam_center = 7.0 + p.x * 0.10 + sin((p.x + 5.0) * 0.15) * 1.65;
    float seam_window = authored_ellipse(p, vec2(9.0, 7.8), vec2(28.0, 12.0), 0.28);
    float seam_breakup = mix(0.28, 1.0, smoothstep(0.30, 0.70,
        ground_noise(p * 0.17 + vec2(5.8, 9.1))));
    float seam = (1.0 - smoothstep(0.30, 1.04, abs(p.y - seam_center))) *
        seam_window * seam_breakup * (0.38 + bedrock * 0.62) * (1.0 - pad * 0.42);
    float loose = clamp(1.0 - compacted * 0.82 - bedrock * 0.72, 0.0, 1.0);
    return vec4(compacted, bedrock, seam, loose);
}

vec3 authored_surface_color(vec2 p, vec4 zones, float raster_mask,
        out float material_relief, out float material_roughness) {
    vec3 loose_color = mix(base_color.rgb, secondary_color.rgb, 0.22);
    vec3 compacted_color = mix(base_color.rgb, secondary_color.rgb, 0.42) * 0.98;
    // Exposed rock is cooler, not simply darker: keeping its luminance near the
    // surrounding soil prevents authored shelves from reading as the old black
    // terrain anomalies or as baked shadows.
    vec3 bedrock_color = mix(secondary_color.rgb, base_color.rgb, 0.62) * vec3(0.96, 1.00, 1.05);
    vec3 seam_color = mix(base_color.rgb, vec3(0.52, 0.45, 0.30), 0.28) * 0.99;

    vec3 result = loose_color;
    result = mix(result, compacted_color, zones.x);
    result = mix(result, bedrock_color, zones.y);
    result = mix(result, seam_color, zones.z * 0.54);

    // Sparse mid-scale breakup is tied to the relevant material family rather
    // than sprayed uniformly across the entire map.
    vec2 slab_uv = (p + vec2(2.1, 1.4)) / 6.8;
    vec2 slab_distance = min(fract(slab_uv), 1.0 - fract(slab_uv)) * 6.8;
    float slab_edge = 1.0 - smoothstep(0.11, 0.31, min(slab_distance.x, slab_distance.y));
    slab_edge *= zones.x;

    float strata_wave = sin(dot(p, vec2(0.32, 0.95)) * 0.78 +
        ground_noise(p * 0.075 + vec2(3.4, 8.1)) * 2.5);
    float strata = smoothstep(0.76, 0.95, abs(strata_wave)) * zones.y;

    float crack_field_a = ground_noise(p * 0.22 + vec2(4.7, 1.9));
    float crack_field_b = ground_noise(p * 0.22 + vec2(9.2, 6.3));
    float cracks = (1.0 - smoothstep(0.025, 0.095, abs(crack_field_a - crack_field_b))) *
        clamp(zones.y * 0.88 + zones.w * 0.24, 0.0, 1.0);

    vec2 gravel_cell = floor(p * 0.46);
    vec2 gravel_local = fract(p * 0.46) - 0.5;
    float gravel_seed = ground_hash(gravel_cell + vec2(14.0, 3.0));
    float gravel = step(0.79, gravel_seed) *
        (1.0 - smoothstep(0.11, 0.30, length(gravel_local))) * zones.w;

    result *= 1.0 - slab_edge * 0.080 - cracks * 0.085;
    result *= 1.0 + strata * 0.070 + gravel * 0.060;
    // Raster albedo now provides subordinate mineral response only; the large
    // authored colour masses survive mips and remain legible at 24–72 cells.
    result *= 1.0 + raster_mask * 0.042;
    material_relief = raster_mask * 0.16 - slab_edge * 0.62 +
        strata * 0.28 - cracks * 0.34 + gravel * 0.18;
    material_roughness = clamp(0.96 - zones.x * 0.08 - zones.y * 0.19 -
        zones.z * 0.16 + gravel * 0.035, 0.64, 0.99);
    return max(result, vec3(0.095));
}

void fragment() {
    // Each channel uses a world-space frequency selected for the 24–72 cell
    // camera range. Automatic mips remove sub-pixel grit; these scales retain
    // only the broad mineral shapes that remain stable during RTS movement.
    float mineral_density = clamp(micro_scale / 4.0, 0.025, 5.0);
    float albedo_mask = ground_material_mask(
        anti_tiled_albedo(world_position.xz * albedo_scale * mineral_density), albedo_center, albedo_contrast);
    float height_mask = ground_material_mask(
        anti_tiled_height(world_position.xz * height_scale), height_center, height_contrast);
    float roughness_mask = ground_material_mask(
        anti_tiled_roughness(world_position.xz * roughness_scale), roughness_center, roughness_contrast);
    float macro = broad_ground_breakup(world_position.xz * macro_scale);
    float blend = clamp(0.5 + macro * macro_amount * 0.62
        + albedo_mask * micro_amount * 0.035, 0.0, 1.0);
    vec3 ground_color = mix(base_color.rgb, secondary_color.rgb, blend);
    float composed_relief = height_mask;
    float composed_roughness = clamp(roughness_value + roughness_mask * roughness_variation, 0.2, 1.0);
    if (surface_treatment == 0) {
        vec4 zones = authored_zone_masks(world_position.xz);
        ground_color = authored_surface_color(world_position.xz, zones, albedo_mask,
            composed_relief, composed_roughness);
        // Broad procedural noise merely prevents perfectly sterile plateaus;
        // it no longer defines the terrain composition.
        ground_color *= 1.0 + macro * 0.018;
    }
    if (inspection_pass == 1) {
        ground_color = base_color.rgb;
    } else if (inspection_pass == 2) {
        ground_color = surface_treatment == 0
            ? ground_color
            : base_color.rgb * (1.0 + albedo_mask * 0.34);
    } else if (inspection_pass >= 3) {
        ground_color = vec3(0.46);
    } else {
        ground_color *= 1.0 + albedo_mask * albedo_variation;
    }
    // Far terrain should not turn into radial black pools. Background colour
    // remains the clear colour; ground keeps a stable, readable luminance.
    ALBEDO = mix(ground_color, background_color.rgb, background_influence * 0.22);
    if (inspection_pass == 0) {
        float response = surface_treatment == 0 ? 0.046 : relief_strength;
        NORMAL = ground_relief_normal(normalize(NORMAL), composed_relief, response);
    } else if (inspection_pass == 3) {
        NORMAL = ground_relief_normal(normalize(NORMAL), composed_relief, max(relief_strength, 0.040));
    } else {
        NORMAL = normalize(NORMAL);
    }
    ROUGHNESS = inspection_pass == 4
        ? (surface_treatment == 0
            ? mix(0.30, 0.98, composed_roughness)
            : mix(0.30, 0.98, roughness_mask * 0.5 + 0.5))
        : (inspection_pass == 0 ? composed_roughness : roughness_value);
    SPECULAR = 0.08;
}
""";

    private const string OpaqueEmissiveShader = """
shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_schlick_ggx;
uniform vec4 emission_color : source_color = vec4(1.0, 0.4, 0.1, 1.0);
uniform float emission_energy = 4.0;
uniform float edge_darkening = 0.45;
void fragment() {
    float edge = pow(clamp(1.0 - abs(dot(NORMAL, VIEW)), 0.0, 1.0), 1.5);
    float edge_factor = mix(1.0, 0.22, edge * edge_darkening);
    vec3 visible_color = emission_color.rgb * edge_factor;
    ALBEDO = visible_color * 0.18;
    EMISSION = visible_color * emission_energy * 2.2;
    ROUGHNESS = 0.28;
    SPECULAR = 0.35;
}
""";

    private const string TransparentEmissiveShader = """
shader_type spatial;
render_mode blend_mix, depth_prepass_alpha, cull_disabled, diffuse_burley, specular_schlick_ggx;
uniform vec4 emission_color : source_color = vec4(1.0, 0.4, 0.1, 1.0);
uniform float emission_energy = 4.0;
uniform float alpha_value = 1.0;
uniform float edge_darkening = 0.45;
void fragment() {
    float edge = pow(clamp(1.0 - abs(dot(NORMAL, VIEW)), 0.0, 1.0), 1.5);
    float edge_factor = mix(1.0, 0.22, edge * edge_darkening);
    vec3 visible_color = emission_color.rgb * edge_factor;
    ALBEDO = visible_color * 0.18;
    // Emissive controls represent the visible source, not only a color tint.
    // Keeping HDR output above the glow threshold lets small RTS-scale lamps
    // bloom without making ordinary terrain or painted surfaces emissive.
    EMISSION = visible_color * emission_energy * 2.2;
    ROUGHNESS = 0.28;
    SPECULAR = 0.35;
    ALPHA = alpha_value;
}
""";

    private const string GlassShader = """
shader_type spatial;
render_mode blend_mix, depth_prepass_alpha, cull_disabled, diffuse_burley, specular_schlick_ggx;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform vec4 glass_tint : source_color;
uniform float opacity = 0.42;
uniform float roughness_value = 0.16;
uniform float ior = 1.46;
uniform float refraction_strength = 0.12;
uniform float edge_brightness = 0.5;
void fragment() {
    float edge = pow(clamp(1.0 - abs(clamp(dot(NORMAL, VIEW), -1.0, 1.0)), 0.0, 1.0), 2.2);
    vec2 offset = NORMAL.xy * refraction_strength * (ior - 1.0) * 0.018;
    vec3 behind = textureLod(screen_texture, SCREEN_UV + offset, roughness_value * 4.0).rgb;
    ALBEDO = mix(behind, glass_tint.rgb, 0.36 + edge * 0.28);
    ROUGHNESS = roughness_value;
    METALLIC = 0.0;
    SPECULAR = 0.92;
    ALPHA = clamp(opacity + edge * 0.28, 0.05, 1.0);
    EMISSION = glass_tint.rgb * edge * edge_brightness * 0.08;
}
""";
}
