using Godot;

namespace LegoSpaceRTS.Presentation;

public enum M7VisualStyle : byte
{
    CleanPbr = 0,
    MaterialRealism = 1,
    GraphicToon = 2,
    HandPaintedRetro = 3
}

public enum M7SurfaceSemantic : byte
{
    Body = 0,
    Accent = 1,
    Neutral = 2,
    Tool = 3,
    Rubber = 4,
    Glass = 5,
    Signal = 6,
    Lamp = 7,
    Terrain = 8,
    Dust = 9,
    Spark = 10
}

public static class M7StyleMaterialFactory
{
    private static readonly Color Body = new("087f78");
    private static readonly Color Accent = new("e49b18");
    private static readonly Color Neutral = new("242a2e");
    private static readonly Color Tool = new("929ba1");
    private static readonly Color Rubber = new("111416");
    private static readonly Color Glass = new(0.12f, 0.19f, 0.17f, 0.44f);
    private static readonly Color Signal = new(0.93f, 0.25f, 0.035f, 0.72f);
    private static readonly Color Lamp = new(0.34f, 1.0f, 0.22f, 0.88f);

    public static readonly M7VisualStyle[] Styles = Enum.GetValues<M7VisualStyle>();

    public static string Slug(M7VisualStyle style) => style switch
    {
        M7VisualStyle.CleanPbr => "clean-pbr",
        M7VisualStyle.MaterialRealism => "material-realism",
        M7VisualStyle.GraphicToon => "graphic-toon",
        M7VisualStyle.HandPaintedRetro => "hand-painted-retro",
        _ => "clean-pbr"
    };

    public static M7VisualStyle Parse(string value) => value.Trim().ToLowerInvariant() switch
    {
        "clean" or "clean-pbr" or "pbr" => M7VisualStyle.CleanPbr,
        "real" or "realism" or "material-realism" => M7VisualStyle.MaterialRealism,
        "toon" or "graphic-toon" => M7VisualStyle.GraphicToon,
        "retro" or "painted" or "hand-painted-retro" => M7VisualStyle.HandPaintedRetro,
        _ => M7VisualStyle.CleanPbr
    };

    public static Material Create(M7VisualStyle style, M7SurfaceSemantic semantic)
    {
        if (semantic == M7SurfaceSemantic.Terrain)
            return TerrainMaterial(style);
        if (semantic is M7SurfaceSemantic.Glass or M7SurfaceSemantic.Signal or M7SurfaceSemantic.Lamp)
            return TransparentSemantic(style, semantic);
        if (semantic == M7SurfaceSemantic.Dust)
            return DustMaterial(style);
        if (semantic == M7SurfaceSemantic.Spark)
            return EmissiveStandard(style == M7VisualStyle.GraphicToon ? new Color("ffd85a") : new Color("ff9d28"), style == M7VisualStyle.HandPaintedRetro ? 1.4f : 2.8f);

        (Color color, float metallic, float roughness) = Properties(style, semantic);
        return style switch
        {
            M7VisualStyle.CleanPbr => Standard(color, metallic, roughness, semantic == M7SurfaceSemantic.Terrain),
            M7VisualStyle.MaterialRealism => ProceduralRealMaterial(color, metallic, roughness, semantic == M7SurfaceSemantic.Tool ? 0.34f : 0.12f),
            M7VisualStyle.GraphicToon => ToonMaterial(color, semantic == M7SurfaceSemantic.Tool ? 0.68f : 0.18f),
            M7VisualStyle.HandPaintedRetro => RetroMaterial(color, semantic == M7SurfaceSemantic.Tool ? 0.20f : 0.08f),
            _ => Standard(color, metallic, roughness, semantic == M7SurfaceSemantic.Terrain)
        };
    }

    public static bool IsIntentionallyEmissive(M7SurfaceSemantic semantic)
        => semantic is M7SurfaceSemantic.Lamp or M7SurfaceSemantic.Spark;

    private static (Color Color, float Metallic, float Roughness) Properties(M7VisualStyle style, M7SurfaceSemantic semantic)
    {
        Color color = semantic switch
        {
            M7SurfaceSemantic.Body => style switch
            {
                M7VisualStyle.MaterialRealism => new Color("146b66"),
                M7VisualStyle.GraphicToon => new Color("07978c"),
                M7VisualStyle.HandPaintedRetro => new Color("247a73"),
                _ => Body
            },
            M7SurfaceSemantic.Accent => style switch
            {
                M7VisualStyle.MaterialRealism => new Color("b97812"),
                M7VisualStyle.GraphicToon => new Color("ffb51e"),
                M7VisualStyle.HandPaintedRetro => new Color("cf8d24"),
                _ => Accent
            },
            M7SurfaceSemantic.Neutral => Neutral,
            M7SurfaceSemantic.Tool => Tool,
            M7SurfaceSemantic.Rubber => Rubber,
            M7SurfaceSemantic.Terrain => style switch
            {
                M7VisualStyle.GraphicToon => new Color("303846"),
                M7VisualStyle.HandPaintedRetro => new Color("3a332d"),
                _ => new Color("303337")
            },
            _ => Colors.White
        };
        float metallic = semantic == M7SurfaceSemantic.Tool ? 0.84f : 0f;
        float roughness = semantic switch
        {
            M7SurfaceSemantic.Body => style == M7VisualStyle.CleanPbr ? 0.32f : 0.48f,
            M7SurfaceSemantic.Accent => 0.40f,
            M7SurfaceSemantic.Neutral => style == M7VisualStyle.MaterialRealism ? 0.32f : 0.52f,
            M7SurfaceSemantic.Tool => style == M7VisualStyle.MaterialRealism ? 0.22f : 0.29f,
            M7SurfaceSemantic.Rubber => 0.82f,
            M7SurfaceSemantic.Terrain => style == M7VisualStyle.MaterialRealism ? 0.96f : 0.88f,
            _ => 0.50f
        };
        if (style == M7VisualStyle.MaterialRealism)
        {
            if (semantic == M7SurfaceSemantic.Body) metallic = 0.10f;
            if (semantic == M7SurfaceSemantic.Neutral) metallic = 0.62f;
        }
        return (color, metallic, roughness);
    }

    private static StandardMaterial3D Standard(Color color, float metallic, float roughness, bool doubleSided = false) => new()
    {
        AlbedoColor = color,
        Metallic = metallic,
        Roughness = roughness,
        CullMode = doubleSided ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Back
    };

    private static Material TransparentSemantic(M7VisualStyle style, M7SurfaceSemantic semantic)
    {
        if (semantic == M7SurfaceSemantic.Lamp)
            return EmissiveStandard(Lamp, style == M7VisualStyle.HandPaintedRetro ? 1.6f : 2.4f, transparent: true);

        Color color = semantic == M7SurfaceSemantic.Signal ? Signal : Glass;
        StandardMaterial3D material = new()
        {
            AlbedoColor = color,
            Metallic = 0f,
            Roughness = style == M7VisualStyle.GraphicToon ? 0.42f : 0.17f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
        // Saturated transparent signal glass remains deliberately non-emissive.
        material.EmissionEnabled = false;
        return material;
    }

    private static StandardMaterial3D EmissiveStandard(Color color, float energy, bool transparent = false)
    {
        StandardMaterial3D material = new()
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = new Color(color.R, color.G, color.B),
            EmissionEnergyMultiplier = energy,
            Roughness = 0.24f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        if (transparent)
        {
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        }
        return material;
    }

    private static ShaderMaterial ProceduralRealMaterial(Color color, float metallic, float roughness, float edgeWear)
    {
        Shader shader = new() { Code = RealMaterialShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("metallic_value", metallic);
        material.SetShaderParameter("roughness_value", roughness);
        material.SetShaderParameter("edge_wear", edgeWear);
        return material;
    }

    private static ShaderMaterial ToonMaterial(Color color, float specularStrength)
    {
        Shader shader = new() { Code = ToonShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("specular_strength", specularStrength);
        return material;
    }

    private static ShaderMaterial RetroMaterial(Color color, float edgeAccent)
    {
        Shader shader = new() { Code = RetroShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("edge_accent", edgeAccent);
        return material;
    }

    private static StandardMaterial3D DustMaterial(M7VisualStyle style)
    {
        Color color = style switch
        {
            M7VisualStyle.GraphicToon => new Color(0.40f, 0.43f, 0.48f, 0.42f),
            M7VisualStyle.HandPaintedRetro => new Color(0.38f, 0.31f, 0.24f, 0.36f),
            _ => new Color(0.30f, 0.28f, 0.25f, 0.27f)
        };
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 1f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = style == M7VisualStyle.GraphicToon ? BaseMaterial3D.ShadingModeEnum.Unshaded : BaseMaterial3D.ShadingModeEnum.PerPixel
        };
    }

    private static Material TerrainMaterial(M7VisualStyle style)
    {
        if (style == M7VisualStyle.CleanPbr)
            return Standard(new Color("41484d"), 0f, 0.86f, doubleSided: true);

        Shader shader = new()
        {
            Code = style switch
            {
                M7VisualStyle.MaterialRealism => RealTerrainShader,
                M7VisualStyle.GraphicToon => ToonTerrainShader,
                _ => RetroTerrainShader
            }
        };
        return new ShaderMaterial { Shader = shader };
    }

    private const string RealMaterialShader = """
shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx, cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float metallic_value : hint_range(0.0, 1.0) = 0.0;
uniform float roughness_value : hint_range(0.0, 1.0) = 0.5;
uniform float edge_wear : hint_range(0.0, 0.5) = 0.1;
varying vec3 world_position;

float material_noise(vec3 p) {
    float a = sin(p.x * 31.7 + p.z * 17.1);
    float b = sin(p.y * 43.3 - p.x * 13.9);
    return (a + b) * 0.5;
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float grain = material_noise(world_position) * 0.018;
    float rim = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 3.5);
    vec3 worn = mix(base_color.rgb, vec3(0.62, 0.65, 0.66), rim * edge_wear);
    ALBEDO = worn * (1.0 + grain);
    METALLIC = metallic_value;
    ROUGHNESS = clamp(roughness_value + grain * 1.8, 0.05, 1.0);
}
""";

    private const string ToonShader = """
shader_type spatial;
render_mode specular_disabled, cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float specular_strength : hint_range(0.0, 1.0) = 0.2;

void fragment() {
    ALBEDO = base_color.rgb;
    ROUGHNESS = 0.88;
}

void light() {
    float ndl = clamp(dot(NORMAL, LIGHT), 0.0, 1.0);
    float band = ndl < 0.34 ? 0.28 : (ndl < 0.72 ? 0.66 : 1.0);
    vec3 diffuse = ALBEDO * LIGHT_COLOR * band * ATTENUATION;
    float half_lambert = clamp(dot(NORMAL, normalize(LIGHT + VIEW)), 0.0, 1.0);
    float graphic_specular = step(0.91, half_lambert) * specular_strength;
    DIFFUSE_LIGHT += diffuse;
    SPECULAR_LIGHT += LIGHT_COLOR * graphic_specular * ATTENUATION;
}
""";

    private const string RetroShader = """
shader_type spatial;
render_mode unshaded, cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float edge_accent : hint_range(0.0, 0.5) = 0.1;
varying vec3 world_normal;

void vertex() {
    world_normal = normalize(mat3(MODEL_MATRIX) * NORMAL);
}

void fragment() {
    vec3 key_direction = normalize(vec3(-0.46, 0.82, 0.34));
    float authored_light = clamp(dot(normalize(world_normal), key_direction) * 0.5 + 0.5, 0.0, 1.0);
    float bands = floor(authored_light * 4.0 + 0.5) / 4.0;
    float checker = mod(floor(FRAGCOORD.x * 0.5) + floor(FRAGCOORD.y * 0.5), 2.0) * 0.022;
    float rim = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 3.0) * edge_accent;
    vec3 warm_shadow = vec3(0.86, 0.78, 0.68);
    ALBEDO = base_color.rgb * mix(warm_shadow, vec3(1.08), bands) + rim - checker;
}
""";

    private const string RealTerrainShader = """
shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx, cull_disabled;
varying vec3 world_position;

float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float value_noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(cell);
    float b = hash21(cell + vec2(1.0, 0.0));
    float c = hash21(cell + vec2(0.0, 1.0));
    float d = hash21(cell + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 p = world_position.xz;
    float broad = value_noise(p * 0.34);
    float medium = value_noise(p * 1.15 + vec2(7.4, 2.1));
    float grit = value_noise(p * 5.8 + vec2(2.7, 9.2));
    float fissure_field = abs(value_noise(p * 0.78 + vec2(14.0, 5.0)) - 0.50) * 2.0;
    float fissure = 1.0 - smoothstep(0.025, 0.10, fissure_field);
    vec3 stone = mix(vec3(0.18, 0.17, 0.16), vec3(0.36, 0.32, 0.27), broad * 0.65 + medium * 0.35);
    ALBEDO = mix(stone * (0.90 + grit * 0.13), vec3(0.045, 0.042, 0.039), fissure * 0.62);
    ROUGHNESS = 0.94;
    METALLIC = 0.0;
}
""";

    private const string ToonTerrainShader = """
shader_type spatial;
render_mode specular_disabled, cull_disabled;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 tile = floor(world_position.xz * 0.42);
    float island = mod(tile.x + tile.y, 3.0);
    float line_x = smoothstep(0.46, 0.49, abs(fract(world_position.x * 0.42) - 0.5));
    float line_z = smoothstep(0.46, 0.49, abs(fract(world_position.z * 0.42) - 0.5));
    vec3 base = island < 1.0 ? vec3(0.25, 0.30, 0.38) : (island < 2.0 ? vec3(0.21, 0.26, 0.34) : vec3(0.28, 0.33, 0.40));
    ALBEDO = mix(base, vec3(0.12, 0.15, 0.20), max(line_x, line_z) * 0.50);
    ROUGHNESS = 1.0;
}

void light() {
    float ndl = clamp(dot(NORMAL, LIGHT), 0.0, 1.0);
    float band = ndl < 0.42 ? 0.52 : (ndl < 0.78 ? 0.78 : 1.0);
    DIFFUSE_LIGHT += ALBEDO * LIGHT_COLOR * band * ATTENUATION;
}
""";

    private const string RetroTerrainShader = """
shader_type spatial;
render_mode unshaded, cull_disabled;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float wash = sin(world_position.x * 0.72) * cos(world_position.z * 0.58) * 0.5 + 0.5;
    float contour = smoothstep(0.86, 0.96, abs(sin(world_position.x * 0.52 + world_position.z * 0.29)));
    float paper = mod(floor(FRAGCOORD.x * 0.5) + floor(FRAGCOORD.y * 0.5), 2.0) * 0.025;
    vec3 rust = mix(vec3(0.28, 0.21, 0.16), vec3(0.46, 0.34, 0.23), floor(wash * 4.0) / 3.0);
    ALBEDO = mix(rust, vec3(0.18, 0.15, 0.14), contour * 0.34) - paper;
}
""";
}
