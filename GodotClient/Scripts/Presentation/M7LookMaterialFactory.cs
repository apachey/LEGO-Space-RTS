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
    private const string RegolithTexturePath = "res://Assets/M7/Textures/regolith_height.png";
    private const string MachinePanelTexturePath = "res://Assets/M7/Textures/machine_panel_height.png";
    private const string BuildingPanelTexturePath = "res://Assets/M7/Textures/building_panel_height.png";

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
            [M7LookMaterialRole.PaintedHull] = Opaque(profile.Materials.PaintedHull, profile.Shading, PaintedTexturePath),
            [M7LookMaterialRole.StructuralEarth] = Opaque(profile.Materials.StructuralEarth, profile.Shading, GroundTexturePath),
            [M7LookMaterialRole.Accent] = Opaque(profile.Materials.Accent, profile.Shading, PaintedTexturePath),
            [M7LookMaterialRole.DarkMechanic] = Opaque(profile.Materials.DarkMechanic, profile.Shading, MachinePanelTexturePath),
            [M7LookMaterialRole.ToolSteel] = Opaque(profile.Materials.ToolSteel, profile.Shading, MetalTexturePath),
            [M7LookMaterialRole.Rubber] = Opaque(profile.Materials.Rubber, profile.Shading, RubberTexturePath),
            [M7LookMaterialRole.BuildingShell] = Opaque(profile.Materials.BuildingShell, profile.Shading, BuildingPanelTexturePath),
            [M7LookMaterialRole.GroundRock] = Ground(profile),
            [M7LookMaterialRole.CanopyGlass] = Glass(profile.Glass),
            [M7LookMaterialRole.Signal] = Emissive(ParseColor(profile.Emission.SignalColor), profile.Emission.SignalEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Lamp] = Emissive(ParseColor(profile.Emission.LampColor), profile.Emission.LampEnergy, 1f, profile.Emission.EdgeDarkening),
            [M7LookMaterialRole.Crystal] = Emissive(ParseColor(profile.Emission.CrystalColor), profile.Emission.CrystalEnergy, 1f, profile.Emission.EdgeDarkening)
        };
    }

    public static ShaderMaterial Opaque(MaterialLook look, ShadingLook shading, string texturePath)
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
        material.SetShaderParameter("detail_texture", GD.Load<Texture2D>(texturePath));
        material.SetShaderParameter("texture_strength", look.TextureStrength);
        material.SetShaderParameter("texture_scale", look.TextureScale);
        material.SetShaderParameter("relief_strength", look.ReliefStrength);
        material.SetShaderParameter("roughness_variation", look.RoughnessVariation);
        material.SetShaderParameter("texture_blend_mode", (float)look.TextureBlendMode);
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
        material.SetShaderParameter("detail_texture", GD.Load<Texture2D>(RegolithTexturePath));
        material.SetShaderParameter("detail_texture_b", GD.Load<Texture2D>(GroundTexturePath));
        material.SetShaderParameter("texture_strength", look.TextureStrength);
        material.SetShaderParameter("texture_scale", look.TextureScale);
        material.SetShaderParameter("relief_strength", look.ReliefStrength);
        material.SetShaderParameter("roughness_variation", look.RoughnessVariation);
        material.SetShaderParameter("texture_blend_mode", (float)look.TextureBlendMode);
        material.SetShaderParameter("background_color", ParseColor(profile.Lighting.BackgroundColor));
        material.SetShaderParameter("background_influence", profile.Lighting.BackgroundInfluence);
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

    public static ShaderMaterial Emissive(Color color, float energy, float alpha = 1f, float edgeDarkening = 0.45f)
    {
        ShaderMaterial material = new() { Shader = new Shader { Code = EmissiveShader } };
        material.SetShaderParameter("emission_color", color);
        material.SetShaderParameter("emission_energy", energy);
        material.SetShaderParameter("alpha_value", alpha);
        material.SetShaderParameter("edge_darkening", edgeDarkening);
        return material;
    }

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
uniform float diffuse_wrap : hint_range(0.0, 1.0) = 0.15;
uniform float shadow_floor : hint_range(0.0, 1.0) = 0.12;
uniform float light_bands : hint_range(0.0, 8.0) = 0.0;
uniform float band_softness : hint_range(0.0, 1.0) = 0.3;
uniform float rim_strength : hint_range(0.0, 2.0) = 0.15;
uniform float rim_width : hint_range(0.05, 1.0) = 0.35;
uniform vec4 shadow_tint : source_color = vec4(0.15, 0.2, 0.28, 1.0);
uniform vec4 highlight_tint : source_color = vec4(1.0, 0.95, 0.86, 1.0);
varying vec3 local_position;

float triplanar_detail(vec3 p, vec3 n, float lod) {
    vec3 weights = pow(abs(n), vec3(4.0));
    weights /= max(0.0001, weights.x + weights.y + weights.z);
    return textureLod(detail_texture, p.yz, lod).r * weights.x
        + textureLod(detail_texture, p.xz, lod).r * weights.y
        + textureLod(detail_texture, p.xy, lod).r * weights.z;
}

vec3 blend_authored_texture(vec3 color, float height_value) {
    float signed_height = (height_value - 0.5) * 2.0;
    vec3 soft_light = color * (1.0 + signed_height * 0.72);
    vec3 groove_deepen = color * mix(0.58, 1.18, height_value);
    vec3 overlay = mix(2.0 * color * vec3(height_value),
        1.0 - 2.0 * (1.0 - color) * (1.0 - vec3(height_value)),
        step(vec3(0.5), color));
    vec3 selected = texture_blend_mode < 0.5 ? soft_light
        : (texture_blend_mode < 1.5 ? groove_deepen : overlay);
    return mix(color, selected, texture_strength);
}

vec3 relief_normal(vec3 base_normal, float height_value, float strength) {
    vec3 dpdx = dFdx(local_position);
    vec3 dpdy = dFdy(local_position);
    float dhdx = dFdx(height_value);
    float dhdy = dFdy(height_value);
    vec3 r1 = cross(dpdy, base_normal);
    vec3 r2 = cross(base_normal, dpdx);
    float determinant = dot(dpdx, r1);
    vec3 gradient = sign(determinant) * (dhdx * r1 + dhdy * r2);
    return normalize(abs(determinant) * base_normal - gradient * strength);
}

void vertex() {
    local_position = VERTEX;
}

void fragment() {
    vec3 detail_position = local_position * texture_scale;
    // Explicitly sample a stable mip for RTS distance. LOD 0 aliases badly
    // when a 1K authored map occupies only a few dozen screen pixels.
    float fine = triplanar_detail(detail_position * max(0.08, micro_scale * 0.20), NORMAL, 3.0);
    float coarse = triplanar_detail(detail_position * 0.28, NORMAL, 4.0);
    float broad = triplanar_detail(detail_position * 0.08, NORMAL, 6.0);
    float macro = clamp(coarse / max(0.08, broad) - 1.0, -1.0, 1.0);
    float micro = clamp(fine / max(0.08, coarse) - 1.0, -1.0, 1.0);
    float brush = sin((local_position.x + local_position.z * 0.17) * 46.0) * 0.5 + 0.5;
    float authored_detail = micro;
    NORMAL = relief_normal(normalize(NORMAL), fine, relief_strength * 0.68);
    float upward = clamp((NORMAL.y + 1.0) * 0.5, 0.0, 1.0);
    float variation = macro * macro_variation + micro * micro_strength;
    vec3 color = base_color.rgb * (1.0 + variation);
    color = blend_authored_texture(color, fine);
    color = mix(color, color * vec3(1.13, 1.10, 1.04), edge_wear * pow(1.0 - abs(dot(NORMAL, VIEW)), 3.0));
    color = mix(color, shadow_tint.rgb, dust_amount * upward * (0.32 + (macro * 0.5 + 0.5) * 0.38));
    ALBEDO = color;
    METALLIC = metallic_value;
    ROUGHNESS = clamp(roughness_value + micro * micro_strength * 0.22 - brush * brushed_amount * 0.22
        + authored_detail * roughness_variation * 0.42, 0.03, 1.0);
    SPECULAR = clamp(specular_value * mix(1.0 - fresnel_value * 0.25, 1.0 + fresnel_value * 0.25, pow(1.0 - dot(NORMAL, VIEW), 3.0)), 0.0, 1.0);
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
    DIFFUSE_LIGHT += LIGHT_COLOR * shadow_color * wrapped * shadowed * (1.0 - metallic_value * 0.72);

    vec3 half_vector = normalize(LIGHT + VIEW);
    float ndh = max(dot(NORMAL, half_vector), 0.0);
    float vdh = max(dot(VIEW, half_vector), 0.0);
    float spec_power = mix(180.0, 4.0, ROUGHNESS * ROUGHNESS);
    float lobe = pow(ndh, spec_power) * mix(0.35, 2.2, 1.0 - ROUGHNESS);
    vec3 dielectric_f0 = vec3(0.018 + clamp(specular_value, 0.0, 1.0) * 0.12);
    vec3 f0 = mix(dielectric_f0, ALBEDO, metallic_value);
    vec3 fresnel = f0 + (vec3(1.0) - f0) * pow(1.0 - vdh, mix(7.0, 3.0, fresnel_value));
    float coat_power = mix(220.0, 12.0, clearcoat_roughness);
    float coat = pow(ndh, coat_power) * clearcoat_value * 0.35;
    SPECULAR_LIGHT += LIGHT_COLOR * (fresnel * lobe + vec3(coat)) * ATTENUATION;

    float rim = pow(clamp(1.0 - dot(NORMAL, VIEW), 0.0, 1.0), mix(7.0, 1.2, rim_width));
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
uniform sampler2D detail_texture : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D detail_texture_b : filter_linear_mipmap_anisotropic, repeat_enable;
uniform float texture_strength = 0.35;
uniform float texture_scale = 0.16;
uniform float relief_strength = 0.45;
uniform float roughness_variation = 0.30;
uniform float texture_blend_mode = 1.0;
uniform vec4 background_color : source_color = vec4(0.08, 0.11, 0.14, 1.0);
uniform float background_influence = 0.15;
varying vec3 world_position;

void vertex() { world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz; }

float anti_tiled_height(vec2 uv) {
    mat2 rotate_a = mat2(vec2(0.866, 0.5), vec2(-0.5, 0.866));
    mat2 rotate_b = mat2(vec2(0.342, -0.940), vec2(0.940, 0.342));
    float a = textureLod(detail_texture, uv, 4.0).r;
    float b = textureLod(detail_texture, rotate_a * uv * 0.713 + vec2(0.37, 0.19), 4.0).r;
    float c = textureLod(detail_texture_b, rotate_b * uv * 1.371 + vec2(0.11, 0.63), 4.5).r;
    float broad = textureLod(detail_texture_b, rotate_a * uv * 0.181, 6.0).r;
    return clamp(a * 0.40 + b * 0.24 + c * 0.26 + broad * 0.10, 0.0, 1.0);
}

vec3 blend_ground_texture(vec3 color, float height_value) {
    float signed_height = (height_value - 0.5) * 2.0;
    vec3 soft_light = color * (1.0 + signed_height * 0.38);
    vec3 groove_deepen = color * mix(0.78, 1.08, height_value);
    vec3 overlay = mix(2.0 * color * vec3(height_value),
        1.0 - 2.0 * (1.0 - color) * (1.0 - vec3(height_value)),
        step(vec3(0.5), color));
    vec3 selected = texture_blend_mode < 0.5 ? soft_light
        : (texture_blend_mode < 1.5 ? groove_deepen : overlay);
    return mix(color, selected, texture_strength);
}

vec3 ground_relief_normal(vec3 base_normal, float height_value, float strength) {
    vec3 dpdx = dFdx(world_position);
    vec3 dpdy = dFdy(world_position);
    float dhdx = dFdx(height_value);
    float dhdy = dFdy(height_value);
    vec3 r1 = cross(dpdy, base_normal);
    vec3 r2 = cross(base_normal, dpdx);
    float determinant = dot(dpdx, r1);
    vec3 gradient = sign(determinant) * (dhdx * r1 + dhdy * r2);
    return normalize(abs(determinant) * base_normal - gradient * strength);
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
    vec2 detail_uv = world_position.xz * texture_scale * max(0.45, sqrt(micro_scale / 20.0));
    float detail_fine = mix(0.5, anti_tiled_height(detail_uv), 0.70);
    float authored_detail = (detail_fine - 0.5) * 2.0;
    float macro = broad_ground_breakup(world_position.xz * macro_scale);
    // "Small mineral breakup" is a supporting tint, not another full albedo.
    float blend = clamp(0.5 + macro * macro_amount + authored_detail * micro_amount * 0.16, 0.0, 1.0);
    vec3 ground_color = mix(base_color.rgb, secondary_color.rgb, blend);
    ground_color = blend_ground_texture(ground_color, detail_fine);
    float far_mix = smoothstep(24.0, 70.0, length(world_position.xz)) * background_influence;
    ALBEDO = mix(ground_color, background_color.rgb, far_mix);
    NORMAL = ground_relief_normal(normalize(NORMAL), detail_fine, relief_strength * 0.28);
    ROUGHNESS = clamp(roughness_value + authored_detail * roughness_variation * 0.18, 0.2, 1.0);
    SPECULAR = 0.08;
}
""";

    private const string EmissiveShader = """
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
