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
    private const string PaintedTexturePath = "res://Assets/M7/Textures/painted_shell_detail.png";
    private const string MetalTexturePath = "res://Assets/M7/Textures/brushed_metal_detail.png";
    private const string RubberTexturePath = "res://Assets/M7/Textures/rubber_detail.png";
    private const string GroundTexturePath = "res://Assets/M7/Textures/quarry_ground_detail.png";
    private const string GroundAlbedoTexturePath = "res://Assets/M7/Textures/regolith_surface_v2.png";
    // The old photographic rock image produced recognizable repeated ridges
    // when reused as a height field. The generated low-frequency regolith map
    // is deliberately neutral enough to support both color and shallow relief.
    private const string RegolithTexturePath = GroundAlbedoTexturePath;
    private const string BuildingPanelTexturePath = "res://Assets/M7/Textures/building_panel_height.png";
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
            [M7LookMaterialRole.PaintedHull] = AuthoredOpaque(profile.Materials.PaintedHull, profile.Shading, PaintedTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.PaintedHull),
            [M7LookMaterialRole.StructuralEarth] = AuthoredOpaque(profile.Materials.StructuralEarth, profile.Shading, PaintedTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.StructuralEarth),
            [M7LookMaterialRole.Accent] = AuthoredOpaque(profile.Materials.Accent, profile.Shading, PaintedTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.Accent),
            [M7LookMaterialRole.DarkMechanic] = AuthoredOpaque(profile.Materials.DarkMechanic, profile.Shading, MetalTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.DarkMechanic),
            [M7LookMaterialRole.ToolSteel] = AuthoredOpaque(profile.Materials.ToolSteel, profile.Shading, MetalTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.ToolSteel),
            [M7LookMaterialRole.Rubber] = AuthoredOpaque(profile.Materials.Rubber, profile.Shading, RubberTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.Rubber),
            [M7LookMaterialRole.BuildingShell] = AuthoredOpaque(profile.Materials.BuildingShell, profile.Shading, BuildingPanelTexturePath, profile.Materials.InspectionPass, M7LookMaterialRole.BuildingShell),
            [M7LookMaterialRole.GroundRock] = Ground(profile),
            [M7LookMaterialRole.CanopyGlass] = Glass(profile.Glass),
            [M7LookMaterialRole.Signal] = Emissive(ParseColor(profile.Emission.SignalColor), profile.Emission.SignalEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Lamp] = Emissive(ParseColor(profile.Emission.LampColor), profile.Emission.LampEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Crystal] = Emissive(ParseColor(profile.Emission.CrystalColor), profile.Emission.CrystalEnergy, 1f, profile.Emission.EdgeDarkening)
        };
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
        material.SetShaderParameter("texture_strength", look.TextureStrength);
        material.SetShaderParameter("texture_scale", look.TextureScale);
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
        string texturePath,
        int inspectionPass,
        M7LookMaterialRole role)
    {
        ShaderMaterial material = Opaque(look, shading, texturePath, inspectionPass);
        SurfaceRecipe recipe = role switch
        {
            M7LookMaterialRole.PaintedHull => new(0.03f, 0.42f, 0.48f, 0.18f, 0.28f, 0.18f, 0.015f, 3.0f, 0.015f, 0.00f, 0.040f, 1.15f, 0.006f, 0.025f),
            M7LookMaterialRole.StructuralEarth => new(0.02f, 0.72f, 0.30f, 0.04f, 0.55f, 0.12f, 0.020f, 3.0f, 0.030f, 0.00f, 0.050f, 1.00f, 0.006f, 0.035f),
            M7LookMaterialRole.Accent => new(0.00f, 0.34f, 0.50f, 0.22f, 0.24f, 0.18f, 0.010f, 3.0f, 0.010f, 0.00f, 0.030f, 1.20f, 0.004f, 0.020f),
            M7LookMaterialRole.DarkMechanic => new(0.72f, 0.42f, 0.68f, 0.02f, 0.50f, 0.25f, 0.020f, 2.2f, 0.015f, 0.08f, 0.025f, 0.75f, 0.004f, 0.040f),
            M7LookMaterialRole.ToolSteel => new(0.92f, 0.30f, 0.72f, 0.02f, 0.40f, 0.26f, 0.010f, 2.0f, 0.010f, 0.18f, 0.020f, 1.10f, 0.003f, 0.035f),
            M7LookMaterialRole.Rubber => new(0.00f, 0.88f, 0.12f, 0.00f, 0.80f, 0.05f, 0.025f, 5.0f, 0.010f, 0.00f, 0.025f, 1.40f, 0.004f, 0.025f),
            M7LookMaterialRole.BuildingShell => new(0.08f, 0.55f, 0.40f, 0.08f, 0.38f, 0.15f, 0.015f, 2.0f, 0.025f, 0.00f, 0.060f, 0.42f, 0.008f, 0.035f),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
        material.SetShaderParameter("metallic_value", recipe.Metallic);
        material.SetShaderParameter("roughness_value", recipe.Roughness);
        material.SetShaderParameter("specular_value", recipe.Specular * shading.GlobalSpecular);
        material.SetShaderParameter("clearcoat_value", recipe.Clearcoat);
        material.SetShaderParameter("clearcoat_roughness", recipe.ClearcoatRoughness);
        material.SetShaderParameter("fresnel_value", recipe.Fresnel);
        material.SetShaderParameter("micro_strength", recipe.MicroStrength);
        material.SetShaderParameter("micro_scale", recipe.MicroScale);
        material.SetShaderParameter("macro_variation", recipe.MacroVariation);
        material.SetShaderParameter("edge_wear", 0f);
        material.SetShaderParameter("dust_amount", 0f);
        material.SetShaderParameter("brushed_amount", recipe.BrushedAmount);
        material.SetShaderParameter("texture_strength", recipe.TextureStrength);
        material.SetShaderParameter("texture_scale", recipe.TextureScale);
        material.SetShaderParameter("relief_strength", recipe.ReliefStrength);
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
        material.SetShaderParameter("roughness_value", look.Roughness);
        material.SetShaderParameter("macro_amount", profile.Ground.MacroAmount);
        material.SetShaderParameter("macro_scale", profile.Ground.MacroScale);
        material.SetShaderParameter("micro_amount", profile.Ground.MicroAmount);
        material.SetShaderParameter("micro_scale", profile.Ground.MicroScale);
        material.SetShaderParameter("albedo_texture", GD.Load<Texture2D>(GroundAlbedoTexturePath));
        material.SetShaderParameter("height_texture", GD.Load<Texture2D>(RegolithTexturePath));
        material.SetShaderParameter("detail_texture_b", GD.Load<Texture2D>(GroundTexturePath));
        material.SetShaderParameter("texture_strength", look.TextureStrength);
        material.SetShaderParameter("texture_scale", look.TextureScale);
        material.SetShaderParameter("relief_strength", look.ReliefStrength);
        material.SetShaderParameter("roughness_variation", look.RoughnessVariation);
        material.SetShaderParameter("texture_blend_mode", (float)look.TextureBlendMode);
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
        float MicroStrength,
        float MicroScale,
        float MacroVariation,
        float BrushedAmount,
        float TextureStrength,
        float TextureScale,
        float ReliefStrength,
        float RoughnessVariation);

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
uniform float texture_strength : hint_range(0.0, 1.0) = 0.0;
uniform float texture_scale = 1.0;
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

float triplanar_detail(vec3 p, vec3 n) {
    vec3 weights = pow(abs(n), vec3(4.0));
    weights /= max(0.0001, weights.x + weights.y + weights.z);
    // Automatic derivative-aware mip selection is essential here. A fixed
    // LOD made the same texture either sparkle or smear as the RTS camera
    // moved, while these coordinates remain locked to the object.
    return texture(detail_texture, p.yz).r * weights.x
        + texture(detail_texture, p.xz).r * weights.y
        + texture(detail_texture, p.xy).r * weights.z;
}

vec3 blend_authored_texture(vec3 color, float height_value) {
    float signed_height = (height_value - 0.5) * 2.0;
    vec3 soft_light = color * (1.0 + signed_height * 0.42);
    vec3 groove_deepen = color * mix(0.82, 1.08, height_value);
    vec3 overlay = mix(2.0 * color * vec3(height_value),
        1.0 - 2.0 * (1.0 - color) * (1.0 - vec3(height_value)),
        step(vec3(0.5), color));
    vec3 selected = texture_blend_mode < 0.5 ? soft_light
        : (texture_blend_mode < 1.5 ? groove_deepen : overlay);
    return mix(color, selected, texture_strength);
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
    local_position = VERTEX;
    local_normal = NORMAL;
    view_position = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

void fragment() {
    vec3 detail_position = local_position * texture_scale;
    vec3 mapping_normal = normalize(local_normal);
    float fine = triplanar_detail(detail_position * max(0.12, micro_scale * 0.12), mapping_normal);
    float coarse = triplanar_detail(detail_position * 0.22, mapping_normal);
    float broad = triplanar_detail(detail_position * 0.065, mapping_normal);
    // Differences, rather than ratios, keep low-contrast authored maps quiet.
    float macro = clamp((coarse - broad) * 2.0, -1.0, 1.0);
    float micro = clamp((fine - coarse) * 1.35, -1.0, 1.0);
    float brush = sin((local_position.z + local_position.x * 0.17) * 9.0) * 0.5 + 0.5;
    float authored_detail = clamp((fine - 0.5) * 2.0, -1.0, 1.0);
    if (inspection_pass == 0) {
        NORMAL = relief_normal(normalize(NORMAL), fine, relief_strength * 0.40);
    } else if (inspection_pass == 3) {
        NORMAL = relief_normal(normalize(NORMAL), fine, max(relief_strength, 0.18) * 0.52);
    } else {
        NORMAL = normalize(NORMAL);
    }
    float normal_view_dot = clamp(dot(normalize(NORMAL), normalize(VIEW)), -1.0, 1.0);
    float upward = clamp((world_normal.y + 1.0) * 0.5, 0.0, 1.0);
    float variation = macro * macro_variation + micro * micro_strength;
    vec3 color = base_color.rgb;
    if (inspection_pass == 0) {
        color *= 1.0 + variation;
        color = blend_authored_texture(color, fine);
        color = mix(color, color * vec3(1.13, 1.10, 1.04), edge_wear * pow(clamp(1.0 - abs(normal_view_dot), 0.0, 1.0), 3.0));
        color = mix(color, shadow_tint.rgb, dust_amount * upward * (0.12 + (macro * 0.5 + 0.5) * 0.20));
    } else if (inspection_pass == 2) {
        color = mix(base_color.rgb * 0.55, base_color.rgb * 1.45, fine);
    } else if (inspection_pass >= 3) {
        color = vec3(0.46);
    }
    ALBEDO = color;
    METALLIC = inspection_pass >= 3 ? 0.0 : metallic_value;
    float reflection_breakup = micro * micro_strength * 0.35 - (brush - 0.5) * brushed_amount * 0.35
        + authored_detail * roughness_variation * 0.55;
    ROUGHNESS = inspection_pass == 4
        ? mix(0.14, 0.90, fine)
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
    float shadowed = mix(shadow_floor, 1.0, ATTENUATION);
    vec3 shadow_color = mix(shadow_tint.rgb, vec3(1.0), ATTENUATION);
    float inspection_metallic = inspection_pass >= 3 ? 0.0 : metallic_value;
    DIFFUSE_LIGHT += LIGHT_COLOR * shadow_color * wrapped * shadowed * (1.0 - inspection_metallic * 0.72);

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
uniform float texture_strength = 0.35;
uniform float texture_scale = 0.16;
uniform float relief_strength = 0.45;
uniform float roughness_variation = 0.30;
uniform float texture_blend_mode = 1.0;
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
    float c = texture(detail_texture_b, rotate_b * uv * 0.617 + vec2(0.11, 0.63)).r;
    return clamp(a * 0.52 + b * 0.28 + c * 0.20, 0.0, 1.0);
}

vec3 blend_ground_texture(vec3 color, float height_value) {
    float signed_height = (height_value - 0.5) * 2.0;
    vec3 soft_light = color * (1.0 + signed_height * 0.34);
    vec3 groove_deepen = color * mix(0.90, 1.04, height_value);
    vec3 overlay = mix(2.0 * color * vec3(height_value),
        1.0 - 2.0 * (1.0 - color) * (1.0 - vec3(height_value)),
        step(vec3(0.5), color));
    vec3 selected = texture_blend_mode < 0.5 ? soft_light
        : (texture_blend_mode < 1.5 ? groove_deepen : overlay);
    return mix(color, selected, texture_strength);
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

void fragment() {
    // Density is expressed relative to the RTS camera baseline. It must not
    // turn the authored height map into per-pixel colour noise at high values.
    vec2 detail_uv = world_position.xz * texture_scale * max(0.65, sqrt(micro_scale / 8.0));
    float albedo_detail = mix(0.5, anti_tiled_albedo(detail_uv * 0.72), 0.74);
    float detail_height = mix(0.5, anti_tiled_height(detail_uv), 0.48);
    float authored_detail = (detail_height - 0.5) * 2.0;
    float macro = broad_ground_breakup(world_position.xz * macro_scale);
    // "Small mineral breakup" is a supporting tint, not another full albedo.
    float blend = clamp(0.5 + macro * macro_amount * 0.62 + authored_detail * micro_amount * 0.045, 0.0, 1.0);
    vec3 ground_color = mix(base_color.rgb, secondary_color.rgb, blend);
    if (inspection_pass == 1) {
        ground_color = base_color.rgb;
    } else if (inspection_pass == 2) {
        ground_color = mix(base_color.rgb * 0.48, base_color.rgb * 1.52, albedo_detail);
    } else if (inspection_pass >= 3) {
        ground_color = vec3(0.46);
    } else {
        ground_color = blend_ground_texture(ground_color, albedo_detail);
    }
    // Far terrain should not turn into radial black pools. Background colour
    // remains the clear colour; ground keeps a stable, readable luminance.
    ALBEDO = mix(ground_color, background_color.rgb, background_influence * 0.22);
    if (inspection_pass == 0) {
        NORMAL = ground_relief_normal(normalize(NORMAL), detail_height, relief_strength * 0.30);
    } else if (inspection_pass == 3) {
        NORMAL = ground_relief_normal(normalize(NORMAL), detail_height, max(relief_strength, 0.18) * 0.42);
    } else {
        NORMAL = normalize(NORMAL);
    }
    ROUGHNESS = inspection_pass == 4
        ? mix(0.32, 0.98, detail_height)
        : clamp(roughness_value + (inspection_pass == 0 ? authored_detail * roughness_variation * 0.32 : 0.0), 0.2, 1.0);
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
