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
            [M7LookMaterialRole.PaintedHull] = Opaque(profile.Materials.PaintedHull, profile.Shading),
            [M7LookMaterialRole.StructuralEarth] = Opaque(profile.Materials.StructuralEarth, profile.Shading),
            [M7LookMaterialRole.Accent] = Opaque(profile.Materials.Accent, profile.Shading),
            [M7LookMaterialRole.DarkMechanic] = Opaque(profile.Materials.DarkMechanic, profile.Shading),
            [M7LookMaterialRole.ToolSteel] = Opaque(profile.Materials.ToolSteel, profile.Shading),
            [M7LookMaterialRole.Rubber] = Opaque(profile.Materials.Rubber, profile.Shading),
            [M7LookMaterialRole.BuildingShell] = Opaque(profile.Materials.BuildingShell, profile.Shading),
            [M7LookMaterialRole.GroundRock] = Ground(profile),
            [M7LookMaterialRole.CanopyGlass] = Glass(profile.Glass),
            [M7LookMaterialRole.Signal] = Emissive(ParseColor(profile.Emission.SignalColor), profile.Emission.SignalEnergy),
            [M7LookMaterialRole.Lamp] = Emissive(ParseColor(profile.Emission.LampColor), profile.Emission.LampEnergy),
            [M7LookMaterialRole.Crystal] = Emissive(ParseColor(profile.Emission.CrystalColor), profile.Emission.CrystalEnergy)
        };
    }

    public static ShaderMaterial Opaque(MaterialLook look, ShadingLook shading)
    {
        ShaderMaterial material = new() { Shader = new Shader { Code = SurfaceShader } };
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

    public static ShaderMaterial Ground(M7LookProfile profile)
    {
        MaterialLook look = profile.Materials.GroundRock;
        ShaderMaterial material = new() { Shader = new Shader { Code = GroundShader } };
        material.SetShaderParameter("base_color", ParseColor(look.BaseColor));
        material.SetShaderParameter("secondary_color", ParseColor(profile.Ground.SecondaryColor));
        material.SetShaderParameter("roughness_value", look.Roughness);
        material.SetShaderParameter("macro_amount", profile.Ground.MacroAmount);
        material.SetShaderParameter("macro_scale", profile.Ground.MacroScale);
        material.SetShaderParameter("micro_amount", profile.Ground.MicroAmount);
        material.SetShaderParameter("micro_scale", profile.Ground.MicroScale);
        material.SetShaderParameter("normal_strength", profile.Ground.NormalStrength);
        return material;
    }

    public static ShaderMaterial Glass(GlassLook glass)
    {
        ShaderMaterial material = new() { Shader = new Shader { Code = GlassShader } };
        material.SetShaderParameter("glass_tint", ParseColor(glass.Tint));
        material.SetShaderParameter("opacity", glass.Opacity);
        material.SetShaderParameter("roughness_value", glass.Roughness);
        material.SetShaderParameter("ior", glass.Ior);
        material.SetShaderParameter("refraction_strength", glass.RefractionStrength);
        material.SetShaderParameter("edge_brightness", glass.EdgeBrightness);
        return material;
    }

    public static StandardMaterial3D Emissive(Color color, float energy, float alpha = 1f) => new()
    {
        AlbedoColor = new Color(color.R, color.G, color.B, alpha),
        EmissionEnabled = true,
        Emission = color,
        EmissionEnergyMultiplier = energy,
        Roughness = 0.24f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = alpha < 0.999f ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled
    };

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
uniform float diffuse_wrap : hint_range(0.0, 1.0) = 0.15;
uniform float shadow_floor : hint_range(0.0, 1.0) = 0.12;
uniform float light_bands : hint_range(0.0, 8.0) = 0.0;
uniform float band_softness : hint_range(0.0, 1.0) = 0.3;
uniform float rim_strength : hint_range(0.0, 2.0) = 0.15;
uniform float rim_width : hint_range(0.05, 1.0) = 0.35;
uniform vec4 shadow_tint : source_color = vec4(0.15, 0.2, 0.28, 1.0);
uniform vec4 highlight_tint : source_color = vec4(1.0, 0.95, 0.86, 1.0);
varying vec3 local_position;

float hash31(vec3 p) {
    p = fract(p * 0.1031);
    p += dot(p, p.yzx + 33.33);
    return fract((p.x + p.y) * p.z);
}

void vertex() {
    local_position = VERTEX;
}

void fragment() {
    float macro = hash31(floor(local_position * 1.7));
    float micro = hash31(floor(local_position * micro_scale * 9.0));
    float brush = sin((local_position.x + local_position.z * 0.17) * 46.0) * 0.5 + 0.5;
    float upward = clamp((NORMAL.y + 1.0) * 0.5, 0.0, 1.0);
    float variation = (macro - 0.5) * macro_variation + (micro - 0.5) * micro_strength;
    vec3 color = base_color.rgb * (1.0 + variation);
    color = mix(color, color * vec3(1.13, 1.10, 1.04), edge_wear * pow(1.0 - abs(dot(NORMAL, VIEW)), 3.0));
    color = mix(color, shadow_tint.rgb, dust_amount * upward * (0.32 + macro * 0.38));
    ALBEDO = color;
    METALLIC = metallic_value;
    ROUGHNESS = clamp(roughness_value + (micro - 0.5) * micro_strength * 0.35 - brush * brushed_amount * 0.16, 0.03, 1.0);
    SPECULAR = clamp(specular_value * mix(1.0 - fresnel_value * 0.25, 1.0 + fresnel_value * 0.25, pow(1.0 - dot(NORMAL, VIEW), 3.0)), 0.0, 1.0);
    CLEARCOAT = clearcoat_value;
    CLEARCOAT_ROUGHNESS = clearcoat_roughness;
    float rim = pow(clamp(1.0 - dot(NORMAL, VIEW), 0.0, 1.0), mix(7.0, 1.2, rim_width));
    EMISSION = highlight_tint.rgb * rim * rim_strength * 0.18;
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
    DIFFUSE_LIGHT += LIGHT_COLOR * wrapped * shadowed;
    float spec_power = mix(96.0, 5.0, ROUGHNESS);
    float spec = pow(max(dot(NORMAL, normalize(LIGHT + VIEW)), 0.0), spec_power);
    SPECULAR_LIGHT += LIGHT_COLOR * spec * clamp(specular_value, 0.0, 1.0) * ATTENUATION;
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
uniform float normal_strength = 0.4;
varying vec3 world_position;

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 345.45));
    p += dot(p, p + 34.345);
    return fract(p.x * p.y);
}

void vertex() { world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz; }
void fragment() {
    float macro = hash21(floor(world_position.xz * macro_scale));
    float micro = hash21(floor(world_position.xz * micro_scale));
    float blend = clamp(macro * macro_amount + (micro - 0.5) * micro_amount, 0.0, 1.0);
    ALBEDO = mix(base_color.rgb, secondary_color.rgb, blend);
    ROUGHNESS = roughness_value;
    SPECULAR = 0.18;
    vec2 slope = vec2(dFdx(micro), dFdy(micro)) * normal_strength;
    NORMAL_MAP = normalize(vec3(-slope.x, -slope.y, 1.0));
    NORMAL_MAP_DEPTH = normal_strength;
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
    float edge = pow(1.0 - abs(dot(NORMAL, VIEW)), 2.2);
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
