using Godot;

namespace LegoSpaceRTS.Presentation;

public enum M7VisualStyle : byte
{
    IndustrialMass = 0,
    HeroicRts = 1,
    ConstructiveLego = 2,
    GraphicVolume = 3
}

public enum M7SurfaceSemantic : byte
{
    Body = 0,
    Accent = 1,
    Earth = 2,
    Neutral = 3,
    Tool = 4,
    Rubber = 5,
    Glass = 6,
    Signal = 7,
    Lamp = 8,
    Terrain = 9,
    Dust = 10,
    Spark = 11
}

public static class M7StyleMaterialFactory
{
    private static readonly Color Body = new("087f78");
    private static readonly Color Accent = new("e49b18");
    private static readonly Color Earth = new("6b3f27");
    private static readonly Color Neutral = new("242a2e");
    private static readonly Color Tool = new("929ba1");
    private static readonly Color Rubber = new("111416");
    private static readonly Color Glass = new(0.12f, 0.19f, 0.17f, 0.44f);
    private static readonly Color Signal = new(0.93f, 0.25f, 0.035f, 0.72f);
    private static readonly Color Lamp = new(0.34f, 1.0f, 0.22f, 0.88f);

    public static readonly M7VisualStyle[] Styles = Enum.GetValues<M7VisualStyle>();

    public static string Slug(M7VisualStyle style) => style switch
    {
        M7VisualStyle.IndustrialMass => "industrial-mass",
        M7VisualStyle.HeroicRts => "heroic-rts",
        M7VisualStyle.ConstructiveLego => "constructive-lego",
        M7VisualStyle.GraphicVolume => "graphic-volume",
        _ => "industrial-mass"
    };

    public static string DisplayName(M7VisualStyle style) => style switch
    {
        M7VisualStyle.IndustrialMass => "INDUSTRIAL MASS",
        M7VisualStyle.HeroicRts => "HEROIC RTS",
        M7VisualStyle.ConstructiveLego => "CONSTRUCTIVE LEGO",
        M7VisualStyle.GraphicVolume => "GRAPHIC VOLUME",
        _ => "INDUSTRIAL MASS"
    };

    public static M7VisualStyle Parse(string value) => value.Trim().ToLowerInvariant() switch
    {
        "industrial" or "industrial-mass" or "real" or "realism" or "material-realism" => M7VisualStyle.IndustrialMass,
        "heroic" or "heroic-rts" => M7VisualStyle.HeroicRts,
        "constructive" or "constructive-lego" or "clean" or "clean-pbr" or "pbr" => M7VisualStyle.ConstructiveLego,
        "graphic" or "graphic-volume" or "toon" or "graphic-toon" or "retro" or "painted" or "hand-painted-retro" => M7VisualStyle.GraphicVolume,
        _ => M7VisualStyle.IndustrialMass
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
            return EmissiveStandard(style == M7VisualStyle.GraphicVolume ? new Color("ffd85a") : new Color("ff9d28"), style == M7VisualStyle.IndustrialMass ? 2.2f : 3.1f);

        (Color color, float metallic, float roughness) = Properties(style, semantic);
        return style switch
        {
            M7VisualStyle.IndustrialMass => ProceduralIndustrialMaterial(color, metallic, roughness, semantic == M7SurfaceSemantic.Tool ? 0.22f : 0.07f),
            M7VisualStyle.HeroicRts => HeroicMaterial(color, semantic == M7SurfaceSemantic.Tool ? 0.62f : 0.14f),
            M7VisualStyle.ConstructiveLego => ConstructiveMaterial(color, metallic, roughness),
            M7VisualStyle.GraphicVolume => GraphicVolumeMaterial(color, semantic == M7SurfaceSemantic.Tool ? 0.52f : 0.11f),
            _ => Standard(color, metallic, roughness, semantic == M7SurfaceSemantic.Terrain)
        };
    }

    public static bool IsIntentionallyEmissive(M7SurfaceSemantic semantic)
        => semantic is M7SurfaceSemantic.Signal or M7SurfaceSemantic.Lamp or M7SurfaceSemantic.Spark;

    private static (Color Color, float Metallic, float Roughness) Properties(M7VisualStyle style, M7SurfaceSemantic semantic)
    {
        Color color = semantic switch
        {
            M7SurfaceSemantic.Body => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("0f6f69"),
                M7VisualStyle.HeroicRts => new Color("07978c"),
                M7VisualStyle.GraphicVolume => new Color("118b82"),
                _ => Body
            },
            M7SurfaceSemantic.Accent => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("bd7b17"),
                M7VisualStyle.HeroicRts => new Color("ffb51e"),
                M7VisualStyle.GraphicVolume => new Color("f2a51c"),
                _ => Accent
            },
            M7SurfaceSemantic.Earth => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("65402c"),
                M7VisualStyle.HeroicRts => new Color("805035"),
                M7VisualStyle.GraphicVolume => new Color("75482f"),
                _ => Earth
            },
            M7SurfaceSemantic.Neutral => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("343b3d"),
                M7VisualStyle.HeroicRts => new Color("283443"),
                M7VisualStyle.ConstructiveLego => new Color("303538"),
                M7VisualStyle.GraphicVolume => new Color("26313d"),
                _ => Neutral
            },
            M7SurfaceSemantic.Tool => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("929a9a"),
                M7VisualStyle.HeroicRts => new Color("b3c0c9"),
                M7VisualStyle.ConstructiveLego => new Color("a4aaad"),
                M7VisualStyle.GraphicVolume => new Color("a5b1b4"),
                _ => Tool
            },
            M7SurfaceSemantic.Rubber => style switch
            {
                M7VisualStyle.HeroicRts => new Color("151d28"),
                M7VisualStyle.ConstructiveLego => new Color("191c1e"),
                M7VisualStyle.GraphicVolume => new Color("19232d"),
                _ => Rubber
            },
            M7SurfaceSemantic.Terrain => style switch
            {
                M7VisualStyle.IndustrialMass => new Color("302f2d"),
                M7VisualStyle.HeroicRts => new Color("383b3e"),
                M7VisualStyle.GraphicVolume => new Color("303946"),
                _ => new Color("303337")
            },
            _ => Colors.White
        };
        float metallic = semantic == M7SurfaceSemantic.Tool ? 0.84f : 0f;
        float roughness = semantic switch
        {
            M7SurfaceSemantic.Body => style == M7VisualStyle.ConstructiveLego ? 0.32f : 0.48f,
            M7SurfaceSemantic.Accent => 0.40f,
            M7SurfaceSemantic.Earth => 0.68f,
            M7SurfaceSemantic.Neutral => style == M7VisualStyle.IndustrialMass ? 0.34f : 0.52f,
            M7SurfaceSemantic.Tool => style == M7VisualStyle.IndustrialMass ? 0.24f : 0.30f,
            M7SurfaceSemantic.Rubber => 0.82f,
            M7SurfaceSemantic.Terrain => style == M7VisualStyle.IndustrialMass ? 0.96f : 0.88f,
            _ => 0.50f
        };
        if (style == M7VisualStyle.IndustrialMass)
        {
            if (semantic == M7SurfaceSemantic.Body) metallic = 0.08f;
            if (semantic == M7SurfaceSemantic.Neutral) metallic = 0.54f;
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
            return EmissiveStandard(Lamp, style == M7VisualStyle.IndustrialMass ? 2.1f : 2.8f, transparent: true);

        if (semantic == M7SurfaceSemantic.Signal)
            return EmissiveStandard(Signal, style == M7VisualStyle.IndustrialMass ? 1.9f : 2.6f, transparent: true);

        StandardMaterial3D material = new()
        {
            AlbedoColor = Glass,
            Metallic = 0f,
            Roughness = style == M7VisualStyle.GraphicVolume ? 0.34f : 0.17f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
        // Canopy glass is optical only. The Raider signal and work lamp are
        // separately authored emissive roles in the M7 style carrier.
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

    private static ShaderMaterial ProceduralIndustrialMaterial(Color color, float metallic, float roughness, float edgeWear)
    {
        Shader shader = new() { Code = IndustrialSurfaceShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("metallic_value", metallic);
        material.SetShaderParameter("roughness_value", roughness);
        material.SetShaderParameter("edge_wear", edgeWear);
        return material;
    }

    private static ShaderMaterial HeroicMaterial(Color color, float metallic)
    {
        Shader shader = new() { Code = HeroicSurfaceShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("metallic_value", metallic);
        return material;
    }

    private static ShaderMaterial ConstructiveMaterial(Color color, float metallic, float roughness)
    {
        Shader shader = new() { Code = ConstructiveSurfaceShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("metallic_value", metallic);
        material.SetShaderParameter("roughness_value", roughness);
        return material;
    }

    private static ShaderMaterial GraphicVolumeMaterial(Color color, float metallic)
    {
        Shader shader = new() { Code = GraphicVolumeSurfaceShader };
        ShaderMaterial material = new() { Shader = shader };
        material.SetShaderParameter("base_color", color);
        material.SetShaderParameter("metallic_value", metallic);
        material.NextPass = new ShaderMaterial
        {
            Shader = new Shader { Code = GraphicOutlineShader }
        };
        return material;
    }

    private static StandardMaterial3D DustMaterial(M7VisualStyle style)
    {
        Color color = style switch
        {
            M7VisualStyle.IndustrialMass => new Color(0.31f, 0.28f, 0.24f, 0.30f),
            M7VisualStyle.HeroicRts => new Color(0.43f, 0.40f, 0.34f, 0.36f),
            M7VisualStyle.GraphicVolume => new Color(0.38f, 0.42f, 0.48f, 0.35f),
            _ => new Color(0.30f, 0.28f, 0.25f, 0.27f)
        };
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 1f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = style == M7VisualStyle.GraphicVolume ? BaseMaterial3D.ShadingModeEnum.Unshaded : BaseMaterial3D.ShadingModeEnum.PerPixel
        };
    }

    private static Material TerrainMaterial(M7VisualStyle style)
    {
        Shader shader = new()
        {
            Code = style switch
            {
                M7VisualStyle.IndustrialMass => IndustrialTerrainShader,
                M7VisualStyle.HeroicRts => HeroicTerrainShader,
                M7VisualStyle.ConstructiveLego => ConstructiveTerrainShader,
                _ => GraphicVolumeTerrainShader
            }
        };
        return new ShaderMaterial { Shader = shader };
    }

    private const string IndustrialSurfaceShader = """
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
    float grain = material_noise(world_position) * 0.009;
    float rim = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 4.2);
    vec3 worn = mix(base_color.rgb, vec3(0.50, 0.53, 0.54), rim * edge_wear);
    ALBEDO = worn * (1.0 + grain);
    METALLIC = metallic_value;
    ROUGHNESS = clamp(roughness_value + grain * 1.2, 0.08, 1.0);
}
""";

    private const string HeroicSurfaceShader = """
shader_type spatial;
render_mode cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float metallic_value : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    ALBEDO = base_color.rgb;
    METALLIC = metallic_value;
    ROUGHNESS = mix(0.52, 0.30, metallic_value);
}

void light() {
    float ndl = clamp(dot(NORMAL, LIGHT), 0.0, 1.0);
    float wrapped = clamp(ndl * 0.86 + 0.14, 0.0, 1.0);
    float shaped = smoothstep(0.16, 0.88, wrapped);
    float rim = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 2.15);
    vec3 diffuse = ALBEDO * LIGHT_COLOR * (0.16 + shaped * 0.96 + rim * 0.14) * ATTENUATION;
    float half_lambert = clamp(dot(NORMAL, normalize(LIGHT + VIEW)), 0.0, 1.0);
    float graphic_specular = pow(half_lambert, 22.0) * mix(0.20, 0.88, metallic_value);
    DIFFUSE_LIGHT += diffuse;
    SPECULAR_LIGHT += LIGHT_COLOR * graphic_specular * ATTENUATION;
}
""";

    private const string ConstructiveSurfaceShader = """
shader_type spatial;
render_mode cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float metallic_value : hint_range(0.0, 1.0) = 0.0;
uniform float roughness_value : hint_range(0.0, 1.0) = 0.34;

void fragment() {
    ALBEDO = base_color.rgb;
    METALLIC = metallic_value;
    ROUGHNESS = roughness_value;
}

void light() {
    float ndl = max(dot(NORMAL, LIGHT), 0.0);
    DIFFUSE_LIGHT += ALBEDO * LIGHT_COLOR * (0.10 + ndl * 0.90) * ATTENUATION;
    vec3 half_vector = normalize(LIGHT + VIEW);
    float tight_highlight = pow(max(dot(NORMAL, half_vector), 0.0), mix(76.0, 38.0, metallic_value));
    float molded_edge = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 4.6);
    float highlight_strength = mix(0.62, 0.92, metallic_value);
    SPECULAR_LIGHT += LIGHT_COLOR * (tight_highlight * highlight_strength + molded_edge * 0.09) * ATTENUATION;
}
""";

    private const string GraphicVolumeSurfaceShader = """
shader_type spatial;
render_mode cull_disabled;

uniform vec4 base_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float metallic_value : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    ALBEDO = base_color.rgb;
    METALLIC = metallic_value;
    ROUGHNESS = mix(0.76, 0.36, metallic_value);
}

void light() {
    float ndl = clamp(dot(NORMAL, LIGHT), 0.0, 1.0);
    float band = ndl < 0.20 ? 0.28 : (ndl < 0.52 ? 0.55 : (ndl < 0.82 ? 0.82 : 1.08));
    float rim = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), 2.8) * 0.11;
    DIFFUSE_LIGHT += ALBEDO * LIGHT_COLOR * (band + rim) * ATTENUATION;
    float half_lambert = clamp(dot(NORMAL, normalize(LIGHT + VIEW)), 0.0, 1.0);
    SPECULAR_LIGHT += LIGHT_COLOR * step(0.94, half_lambert) * metallic_value * 0.55 * ATTENUATION;
}
""";

    private const string GraphicOutlineShader = """
shader_type spatial;
render_mode unshaded, cull_front;

void vertex() {
    VERTEX += NORMAL * 0.026;
}

void fragment() {
    ALBEDO = vec3(0.025, 0.034, 0.043);
}
""";

    private const string IndustrialTerrainShader = """
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
    float broad = value_noise(p * 0.22);
    float medium = value_noise(p * 0.72 + vec2(7.4, 2.1));
    float grit = value_noise(p * 4.8 + vec2(2.7, 9.2));
    vec3 stone = mix(vec3(0.18, 0.185, 0.175), vec3(0.34, 0.295, 0.235), broad * 0.72 + medium * 0.28);
    ALBEDO = stone * (0.93 + grit * 0.11);
    ROUGHNESS = 0.96;
    METALLIC = 0.0;
}
""";

    private const string HeroicTerrainShader = """
shader_type spatial;
render_mode diffuse_burley, specular_disabled, cull_disabled;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float broad = sin(world_position.x * 0.31 + sin(world_position.z * 0.19) * 0.8) * 0.5 + 0.5;
    float secondary = sin((world_position.x + world_position.z) * 0.83) * 0.5 + 0.5;
    vec3 cool_rock = vec3(0.17, 0.205, 0.245);
    vec3 warm_rock = vec3(0.39, 0.315, 0.235);
    ALBEDO = mix(cool_rock, warm_rock, broad * 0.78 + secondary * 0.22);
    ROUGHNESS = 0.94;
}
""";

    private const string ConstructiveTerrainShader = """
shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx, cull_disabled;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float broad = sin(world_position.x * 0.31 + sin(world_position.z * 0.17)) * 0.5 + 0.5;
    float grain = sin(world_position.x * 3.7) * sin(world_position.z * 4.1) * 0.5 + 0.5;
    vec3 basalt = mix(vec3(0.17, 0.185, 0.19), vec3(0.29, 0.30, 0.295), broad);
    ALBEDO = basalt * (0.96 + grain * 0.055);
    ROUGHNESS = 0.92;
    METALLIC = 0.0;
}
""";

    private const string GraphicVolumeTerrainShader = """
shader_type spatial;
render_mode specular_disabled, cull_disabled;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float strata = sin(world_position.x * 0.82 + world_position.z * 0.36 + sin(world_position.z * 0.34) * 0.65) * 0.5 + 0.5;
    float bands = floor(strata * 4.0) / 3.0;
    vec3 ground = mix(vec3(0.175, 0.205, 0.235), vec3(0.245, 0.265, 0.265), bands);
    ALBEDO = ground;
    ROUGHNESS = 1.0;
}

void light() {
    float ndl = clamp(dot(NORMAL, LIGHT), 0.0, 1.0);
    float band = ndl < 0.28 ? 0.48 : (ndl < 0.68 ? 0.76 : 1.0);
    DIFFUSE_LIGHT += ALBEDO * LIGHT_COLOR * band * ATTENUATION;
}
""";
}
