using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public enum PresentationDestructionScaleBand : byte
{
    Tiny = 0,
    Small = 1,
    Medium = 2,
    Heavy = 3,
    Massive = 4,
    Structure = 5
}

public sealed class PresentationDestructionTuning
{
    public bool Enabled { get; set; } = true;
    public float CollapseSeconds { get; set; } = 0.72f;
    public float SettleTiltDegrees { get; set; } = 9f;
    public float WreckWidthRatio { get; set; } = 0.78f;
    public float WreckHeightRatio { get; set; } = 0.15f;
    public int HeroFragmentCount { get; set; } = PooledLegoDebrisBurst.MaxFragments;
    public float FragmentScale { get; set; } = 0.85f;
    public float OutwardSpeed { get; set; } = 3.8f;
    public float UpwardSpeed { get; set; } = 4.6f;
    public float Gravity { get; set; } = 9.5f;
    public float Drag { get; set; } = 0.38f;
    public float Bounce { get; set; } = 0.24f;
    public float AngularSpeedDegrees { get; set; } = 260f;
    public float DebrisLifetime { get; set; } = 4.2f;
    public float FadeSeconds { get; set; } = 0.65f;
    public int DustCount { get; set; } = 22;
    public float DustSize { get; set; } = 0.55f;
    public float DustLifetime { get; set; } = 1.15f;

    public void Normalize()
    {
        CollapseSeconds = Math.Clamp(CollapseSeconds, 0.05f, 4f);
        SettleTiltDegrees = Math.Clamp(SettleTiltDegrees, 0f, 45f);
        WreckWidthRatio = Math.Clamp(WreckWidthRatio, 0.25f, 1.5f);
        WreckHeightRatio = Math.Clamp(WreckHeightRatio, 0.03f, 1f);
        HeroFragmentCount = Math.Clamp(HeroFragmentCount, 0, PooledLegoDebrisBurst.MaxFragments);
        FragmentScale = Math.Clamp(FragmentScale, 0.1f, 3f);
        OutwardSpeed = Math.Clamp(OutwardSpeed, 0f, 18f);
        UpwardSpeed = Math.Clamp(UpwardSpeed, 0f, 18f);
        Gravity = Math.Clamp(Gravity, 0f, 30f);
        Drag = Math.Clamp(Drag, 0f, 6f);
        Bounce = Math.Clamp(Bounce, 0f, 0.9f);
        AngularSpeedDegrees = Math.Clamp(AngularSpeedDegrees, 0f, 1080f);
        DebrisLifetime = Math.Clamp(DebrisLifetime, 0.1f, 15f);
        FadeSeconds = Math.Clamp(FadeSeconds, 0f, DebrisLifetime);
        DustCount = Math.Clamp(DustCount, 0, 64);
        DustSize = Math.Clamp(DustSize, 0.05f, 4f);
        DustLifetime = Math.Clamp(DustLifetime, 0.1f, 5f);
    }

    public int ResolveHeroFragmentCount(PresentationDestructionScaleBand band)
    {
        int scaleDefault = band switch
        {
            PresentationDestructionScaleBand.Tiny => 3,
            PresentationDestructionScaleBand.Small => 5,
            PresentationDestructionScaleBand.Medium => 7,
            PresentationDestructionScaleBand.Heavy => 14,
            PresentationDestructionScaleBand.Massive => 16,
            _ => 18
        };
        return Math.Clamp(Math.Min(HeroFragmentCount, scaleDefault), 0, PooledLegoDebrisBurst.MaxFragments);
    }

    public static PresentationDestructionScaleBand BandFor(PresentationEntity entity)
    {
        if (entity.DestructionKind == DestructionKind.Structure || entity.SelectableKind == SelectableKind.Building)
            return PresentationDestructionScaleBand.Structure;
        return entity.Footprint switch
        {
            FootprintClass.Tiny => PresentationDestructionScaleBand.Tiny,
            FootprintClass.Small => PresentationDestructionScaleBand.Small,
            FootprintClass.Medium => PresentationDestructionScaleBand.Medium,
            FootprintClass.Large => PresentationDestructionScaleBand.Heavy,
            _ => PresentationDestructionScaleBand.Massive
        };
    }
}

public readonly struct PresentationDestructionFrame
{
    public readonly Vector3 Scale;
    public readonly Vector3 TiltDegrees;
    public readonly float NormalizedProgress;
    public readonly bool Settled;

    public PresentationDestructionFrame(Vector3 scale, Vector3 tiltDegrees, float normalizedProgress, bool settled)
    {
        Scale = scale;
        TiltDegrees = tiltDegrees;
        NormalizedProgress = normalizedProgress;
        Settled = settled;
    }
}

public sealed class PresentationDestructionDriver
{
    private readonly Dictionary<uint, DestructionState> _states = new();

    public int TrackedEntityCount => _states.Count;

    public void Begin(uint entityKey)
    {
        _states[entityKey] = new DestructionState();
    }

    public PresentationDestructionFrame Update(uint entityKey, bool destroyed, Vector3 baseScale,
        float renderDelta, PresentationDestructionTuning tuning)
    {
        tuning.Normalize();
        if (!destroyed)
        {
            _states.Remove(entityKey);
            return new PresentationDestructionFrame(baseScale, Vector3.Zero, 0f, false);
        }

        if (!_states.TryGetValue(entityKey, out DestructionState? state))
        {
            state = new DestructionState { Elapsed = tuning.CollapseSeconds };
            _states.Add(entityKey, state);
        }
        else
        {
            state.Elapsed = Math.Min(tuning.CollapseSeconds, state.Elapsed + Math.Clamp(renderDelta, 0f, 0.25f));
        }

        float progress = tuning.Enabled
            ? Math.Clamp(state.Elapsed / tuning.CollapseSeconds, 0f, 1f)
            : 1f;
        float eased = progress * progress * (3f - 2f * progress);
        Vector3 settledScale = new(baseScale.X * tuning.WreckWidthRatio,
            baseScale.Y * tuning.WreckHeightRatio, baseScale.Z * tuning.WreckWidthRatio);
        Vector3 scale = baseScale.Lerp(settledScale, eased);
        float signX = (entityKey & 1u) == 0 ? 1f : -1f;
        float signZ = (entityKey & 2u) == 0 ? 0.42f : -0.42f;
        Vector3 tilt = new(signX * tuning.SettleTiltDegrees * eased, 0f,
            signZ * tuning.SettleTiltDegrees * eased);
        return new PresentationDestructionFrame(scale, tilt, progress, progress >= 1f);
    }

    public void Remove(uint entityKey) => _states.Remove(entityKey);
    public void Clear() => _states.Clear();

    private sealed class DestructionState
    {
        public float Elapsed;
    }
}

public readonly struct LegoDebrisBurstRequest
{
    public readonly Vector3 Origin;
    public readonly Vector3 SourceSize;
    public readonly float SourceYawRadians;
    public readonly float GroundWorldY;
    public readonly int FragmentCount;
    public readonly float FragmentScale;
    public readonly float OutwardSpeed;
    public readonly float UpwardSpeed;
    public readonly float Gravity;
    public readonly float Drag;
    public readonly float Bounce;
    public readonly float AngularSpeedRadians;
    public readonly float FadeFraction;
    public readonly uint Seed;
    public readonly Color PrimaryColor;
    public readonly Color MechanicalColor;
    public readonly Color AccentColor;

    public LegoDebrisBurstRequest(Vector3 origin, Vector3 sourceSize, float sourceYawRadians, float groundWorldY,
        int fragmentCount, float fragmentScale, float outwardSpeed, float upwardSpeed, float gravity, float drag,
        float bounce, float angularSpeedRadians, float fadeFraction, uint seed, Color primaryColor,
        Color mechanicalColor, Color accentColor)
    {
        Origin = origin;
        SourceSize = new Vector3(Math.Max(0.1f, sourceSize.X), Math.Max(0.1f, sourceSize.Y),
            Math.Max(0.1f, sourceSize.Z));
        SourceYawRadians = sourceYawRadians;
        GroundWorldY = groundWorldY;
        FragmentCount = Math.Clamp(fragmentCount, 0, PooledLegoDebrisBurst.MaxFragments);
        FragmentScale = Math.Max(0.01f, fragmentScale);
        OutwardSpeed = Math.Max(0f, outwardSpeed);
        UpwardSpeed = Math.Max(0f, upwardSpeed);
        Gravity = Math.Max(0f, gravity);
        Drag = Math.Max(0f, drag);
        Bounce = Math.Clamp(bounce, 0f, 0.9f);
        AngularSpeedRadians = Math.Max(0f, angularSpeedRadians);
        FadeFraction = Math.Clamp(fadeFraction, 0f, 1f);
        Seed = seed == 0 ? 1u : seed;
        PrimaryColor = primaryColor;
        MechanicalColor = mechanicalColor;
        AccentColor = accentColor;
    }
}

public partial class PooledLegoDebrisBurst : Node3D, IPooledPresentationVfx
{
    public const int MaxFragments = 18;
    private const int MaxRoundFragments = 5;
    private const int MaxPlateFragments = MaxFragments - MaxRoundFragments;

    private readonly MultiMeshInstance3D _plates;
    private readonly MultiMeshInstance3D _roundParts;
    private readonly FragmentState[] _fragments = new FragmentState[MaxFragments];
    private int _fragmentCount;
    private float _groundLocalY;
    private float _gravity;
    private float _drag;
    private float _bounce;
    private float _fadeFraction;

    public int ActiveFragmentCount => Visible ? _fragmentCount : 0;

    public PooledLegoDebrisBurst(Material material)
    {
        BoxMesh plateMesh = new() { Size = Vector3.One };
        CylinderMesh roundMesh = new()
        {
            TopRadius = 0.5f,
            BottomRadius = 0.5f,
            Height = 0.36f,
            RadialSegments = 12,
            Rings = 1
        };
        _plates = CreateChannel("PlateFragments", plateMesh, material, MaxPlateFragments);
        _roundParts = CreateChannel("MechanicalFragments", roundMesh, material, MaxRoundFragments);
        AddChild(_plates);
        AddChild(_roundParts);
    }

    public void Configure(LegoDebrisBurstRequest request)
    {
        GlobalPosition = request.Origin;
        Rotation = Vector3.Zero;
        _fragmentCount = request.FragmentCount;
        _groundLocalY = request.GroundWorldY - request.Origin.Y;
        _gravity = request.Gravity;
        _drag = request.Drag;
        _bounce = request.Bounce;
        _fadeFraction = request.FadeFraction;

        uint random = request.Seed;
        int plateIndex = 0;
        int roundIndex = 0;
        int desiredRound = Math.Min(MaxRoundFragments, Math.Max(request.FragmentCount - MaxPlateFragments,
            Math.Max(request.FragmentCount >= 3 ? 1 : 0, request.FragmentCount / 4)));
        float sourceRadius = Math.Max(0.45f, Math.Max(request.SourceSize.X, request.SourceSize.Z) * 0.36f);
        float sizeFactor = Math.Clamp((request.SourceSize.X + request.SourceSize.Z) * 0.12f, 0.55f, 2.1f) *
            request.FragmentScale;

        for (int i = 0; i < request.FragmentCount; i++)
        {
            bool round = i < desiredRound;
            float rx = Signed(ref random);
            float rz = Signed(ref random);
            Vector3 direction = new Vector3(rx, 0f, rz);
            if (direction.LengthSquared() < 0.04f) direction = Vector3.Right;
            direction = direction.Normalized().Rotated(Vector3.Up, request.SourceYawRadians);
            Vector3 position = new(
                Signed(ref random) * request.SourceSize.X * 0.26f,
                Random01(ref random) * request.SourceSize.Y * 0.58f,
                Signed(ref random) * request.SourceSize.Z * 0.26f);
            float outward = request.OutwardSpeed * (0.62f + Random01(ref random) * 0.72f);
            Vector3 velocity = direction * outward + Vector3.Up * request.UpwardSpeed *
                (0.65f + Random01(ref random) * 0.65f);
            velocity += new Vector3(Signed(ref random), Random01(ref random) * 0.28f,
                Signed(ref random)) * sourceRadius * 0.18f;
            Vector3 rotation = new(Random01(ref random) * MathF.Tau, Random01(ref random) * MathF.Tau,
                Random01(ref random) * MathF.Tau);
            Vector3 angular = new Vector3(Signed(ref random), Signed(ref random), Signed(ref random)).Normalized() *
                request.AngularSpeedRadians * (0.45f + Random01(ref random) * 0.75f);
            Vector3 scale = round
                ? new Vector3(0.34f + Random01(ref random) * 0.38f,
                    0.42f + Random01(ref random) * 0.72f, 0.34f + Random01(ref random) * 0.38f) * sizeFactor
                : new Vector3(0.26f + Random01(ref random) * 0.82f,
                    0.10f + Random01(ref random) * 0.22f, 0.30f + Random01(ref random) * 0.92f) * sizeFactor;
            int localIndex = round ? roundIndex++ : plateIndex++;
            Color color = i % 5 == 0 ? request.AccentColor : round || i % 3 == 0
                ? request.MechanicalColor
                : request.PrimaryColor;
            _fragments[i] = new FragmentState(round, localIndex, position, velocity, rotation, angular, scale);
            MultiMesh channel = round ? _roundParts.Multimesh : _plates.Multimesh;
            channel.SetInstanceColor(localIndex, color);
            ApplyFragment(_fragments[i], 1f);
        }

        _plates.Multimesh.VisibleInstanceCount = plateIndex;
        _roundParts.Multimesh.VisibleInstanceCount = roundIndex;
    }

    public void OnPoolActivated() => Visible = _fragmentCount > 0;

    public void OnPoolUpdated(float normalizedLifetime, float deltaSeconds)
    {
        float delta = Math.Clamp(deltaSeconds, 0f, 0.05f);
        float damping = MathF.Exp(-_drag * delta);
        float fade = _fadeFraction <= 0f
            ? 1f
            : 1f - Math.Clamp((normalizedLifetime - (1f - _fadeFraction)) / _fadeFraction, 0f, 1f);
        for (int i = 0; i < _fragmentCount; i++)
        {
            FragmentState fragment = _fragments[i];
            if (!fragment.Settled)
            {
                fragment.Velocity += Vector3.Down * _gravity * delta;
                fragment.Velocity *= damping;
                fragment.Position += fragment.Velocity * delta;
                fragment.Rotation += fragment.AngularVelocity * delta;
                if (fragment.Position.Y <= _groundLocalY)
                {
                    fragment.Position = new Vector3(fragment.Position.X, _groundLocalY, fragment.Position.Z);
                    if (MathF.Abs(fragment.Velocity.Y) > 0.72f)
                    {
                        fragment.Velocity = new Vector3(fragment.Velocity.X * 0.62f,
                            -fragment.Velocity.Y * _bounce, fragment.Velocity.Z * 0.62f);
                        fragment.AngularVelocity *= 0.72f;
                    }
                    else
                    {
                        fragment.Velocity = Vector3.Zero;
                        fragment.AngularVelocity = Vector3.Zero;
                        fragment.Settled = true;
                    }
                }
                _fragments[i] = fragment;
            }
            ApplyFragment(fragment, fade);
        }
    }

    public void OnPoolReleased()
    {
        _fragmentCount = 0;
        _plates.Multimesh.VisibleInstanceCount = 0;
        _roundParts.Multimesh.VisibleInstanceCount = 0;
        Visible = false;
    }

    private void ApplyFragment(FragmentState fragment, float fade)
    {
        Vector3 scaled = fragment.Scale * Math.Max(0f, fade);
        Basis basis = Basis.FromEuler(fragment.Rotation).Scaled(scaled);
        Transform3D transform = new(basis, fragment.Position);
        (fragment.Round ? _roundParts.Multimesh : _plates.Multimesh)
            .SetInstanceTransform(fragment.LocalIndex, transform);
    }

    private static MultiMeshInstance3D CreateChannel(string name, Mesh mesh, Material material, int capacity)
    {
        mesh.SurfaceSetMaterial(0, material);
        MultiMesh multiMesh = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = mesh,
            InstanceCount = capacity,
            VisibleInstanceCount = 0
        };
        return new MultiMeshInstance3D
        {
            Name = name,
            Multimesh = multiMesh,
            // Small airborne modules create unstable, pixel-sized shadow-map
            // holes at RTS distance. The intact source keeps its real shadow;
            // debris readability comes from silhouette, motion and the blast.
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            CustomAabb = new Aabb(new Vector3(-24f, -4f, -24f), new Vector3(48f, 32f, 48f))
        };
    }

    private static float Random01(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return (state & 0x00ffffffu) / 16777215f;
    }

    private static float Signed(ref uint state) => Random01(ref state) * 2f - 1f;

    private struct FragmentState
    {
        public readonly bool Round;
        public readonly int LocalIndex;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Rotation;
        public Vector3 AngularVelocity;
        public readonly Vector3 Scale;
        public bool Settled;

        public FragmentState(bool round, int localIndex, Vector3 position, Vector3 velocity, Vector3 rotation,
            Vector3 angularVelocity, Vector3 scale)
        {
            Round = round;
            LocalIndex = localIndex;
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            AngularVelocity = angularVelocity;
            Scale = scale;
            Settled = false;
        }
    }
}

public readonly struct ExplosionBurstRequest
{
    public readonly Vector3 Origin;
    public readonly Vector3 SourceSize;
    public readonly float GroundWorldY;
    public readonly Color Color;
    public readonly float VisualScale;
    public readonly float EmissionEnergy;
    public readonly float LightEnergy;
    public readonly float LightRangeMultiplier;
    public readonly float FlashPersistence;
    public readonly float SmokeDelay;
    public readonly float SmokeProminence;
    public readonly float RingStrength;

    public ExplosionBurstRequest(Vector3 origin, Vector3 sourceSize, float groundWorldY, Color color,
        float visualScale = 1f, float emissionEnergy = 8f, float lightEnergy = 6f,
        float lightRangeMultiplier = 3.2f, float flashPersistence = 1.15f,
        float smokeDelay = 0.24f, float smokeProminence = 0.30f, float ringStrength = 0.16f)
    {
        Origin = origin;
        SourceSize = new Vector3(Math.Max(0.1f, MathF.Abs(sourceSize.X)),
            Math.Max(0.1f, MathF.Abs(sourceSize.Y)), Math.Max(0.1f, MathF.Abs(sourceSize.Z)));
        GroundWorldY = float.IsFinite(groundWorldY) ? groundWorldY : origin.Y;
        Color = new Color(Math.Clamp(color.R, 0f, 1f), Math.Clamp(color.G, 0f, 1f),
            Math.Clamp(color.B, 0f, 1f), 1f);
        VisualScale = Math.Clamp(visualScale, 0.1f, 4f);
        EmissionEnergy = Math.Clamp(emissionEnergy, 0f, 24f);
        LightEnergy = Math.Clamp(lightEnergy, 0f, 24f);
        LightRangeMultiplier = Math.Clamp(lightRangeMultiplier, 0.5f, 8f);
        FlashPersistence = Math.Clamp(flashPersistence, 0.5f, 1.8f);
        SmokeDelay = Math.Clamp(smokeDelay, 0f, 0.55f);
        SmokeProminence = Math.Clamp(smokeProminence, 0f, 1f);
        RingStrength = Math.Clamp(ringStrength, 0f, 1f);
    }
}

/// <summary>
/// Bounded, deterministic destruction flash. The owning PresentationVfxPool
/// supplies lifetime and reuse; this node owns a fixed core, glare, ground ring
/// and local light and never creates per-frame children or gameplay state.
/// </summary>
public partial class PooledExplosionBurst : Node3D, IPooledPresentationVfx
{
    public const float RecommendedLifetimeSeconds = 0.90f;
    private const int FireLobeCount = 5;
    private const int HotFragmentCount = 9;
    private const int SmokePuffCount = 7;
    private static readonly Vector3[] FireLobeOffsets =
    [
        new(-0.32f, 0.08f, 0.18f),
        new(0.25f, 0.16f, -0.22f),
        new(0.08f, 0.34f, 0.27f),
        new(-0.18f, 0.27f, -0.34f),
        new(0.34f, 0.31f, 0.10f)
    ];

    private readonly MeshInstance3D _core;
    private readonly MeshInstance3D _glare;
    private readonly MeshInstance3D _shockRing;
    private readonly OmniLight3D _light;
    private readonly MeshInstance3D[] _fireLobes = new MeshInstance3D[FireLobeCount];
    private readonly MeshInstance3D[] _hotFragments = new MeshInstance3D[HotFragmentCount];
    private readonly MeshInstance3D[] _smokePuffs = new MeshInstance3D[SmokePuffCount];
    private readonly Vector3[] _hotFragmentDirections = new Vector3[HotFragmentCount];
    private readonly float[] _hotFragmentWeights = new float[HotFragmentCount];
    private readonly Vector3[] _smokeDirections = new Vector3[SmokePuffCount];
    private readonly float[] _smokeWeights = new float[SmokePuffCount];
    private readonly StandardMaterial3D _coreMaterial;
    private readonly StandardMaterial3D _lobeMaterial;
    private readonly StandardMaterial3D _glareMaterial;
    private readonly StandardMaterial3D _hotFragmentMaterial;
    private readonly StandardMaterial3D _smokeMaterial;
    private readonly ShaderMaterial _ringMaterial;

    private Color _color = Colors.White;
    private float _coreDiameter;
    private float _glareDiameter;
    private float _shockDiameter;
    private float _emissionEnergy;
    private float _lightEnergy;
    private float _lightRange;
    private float _coreCenterY;
    private float _flashPersistence;
    private float _smokeDelay;
    private float _smokeProminence;
    private float _ringStrength;
    private bool _configured;

    public bool CoreVisible => _core.Visible;
    public bool GlareVisible => _glare.Visible;
    public bool ShockRingVisible => _shockRing.Visible;
    public bool LightVisible => _light.Visible;
    public float CurrentLightEnergy => _light.LightEnergy;
    public float ConfiguredCoreDiameter => _coreDiameter;
    public float ConfiguredShockDiameter => _shockDiameter;
    public Vector3 CurrentShockScale => _shockRing.Scale;
    public int VisibleHotFragmentCount => _hotFragments.Count(fragment => fragment.Visible);
    public int VisibleSmokePuffCount => _smokePuffs.Count(puff => puff.Visible);

    public PooledExplosionBurst()
    {
        _coreMaterial = CreateCoreMaterial();
        _lobeMaterial = CreateCoreMaterial();
        _glareMaterial = CreateGlareMaterial();
        _hotFragmentMaterial = CreateHotFragmentMaterial();
        _smokeMaterial = CreateSmokeMaterial();
        _ringMaterial = CreateRingMaterial();

        _core = new MeshInstance3D
        {
            Name = "ExplosionCore",
            Mesh = new SphereMesh { Radius = 0.5f, Height = 1f, RadialSegments = 12, Rings = 6 },
            MaterialOverride = _coreMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Visible = false
        };
        _glare = new MeshInstance3D
        {
            Name = "ExplosionGlare",
            Mesh = new QuadMesh { Size = Vector2.One },
            MaterialOverride = _glareMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Visible = false
        };
        _shockRing = new MeshInstance3D
        {
            Name = "ExplosionShockRing",
            Mesh = new QuadMesh { Size = Vector2.One },
            MaterialOverride = _ringMaterial,
            RotationDegrees = new Vector3(-90f, 0f, 0f),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Visible = false
        };
        _light = new OmniLight3D
        {
            Name = "ExplosionLight",
            ShadowEnabled = false,
            OmniAttenuation = 2.15f,
            LightEnergy = 0f,
            Visible = false
        };
        AddChild(_core);
        AddChild(_glare);
        AddChild(_shockRing);
        AddChild(_light);
        for (int i = 0; i < FireLobeCount; i++)
        {
            MeshInstance3D lobe = new()
            {
                Name = $"ExplosionFireLobe_{i:00}",
                Mesh = new SphereMesh { Radius = 0.5f, Height = 1f, RadialSegments = 10, Rings = 5 },
                MaterialOverride = _lobeMaterial,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Visible = false
            };
            _fireLobes[i] = lobe;
            AddChild(lobe);
        }
        for (int i = 0; i < HotFragmentCount; i++)
        {
            // Golden-angle placement and deterministic unequal lengths avoid
            // the selection-ring / cartoon-sun regularity of evenly spaced rays.
            float angle = i * 2.3999632f;
            float lift = 0.08f + ((i * 37) % 7) / 6f * 0.72f;
            Vector3 direction = new(MathF.Cos(angle), lift, MathF.Sin(angle));
            direction = direction.Normalized();
            _hotFragmentDirections[i] = direction;
            _hotFragmentWeights[i] = 0.72f + ((i * 17) % 5) * 0.105f;
            MeshInstance3D fragment = new()
            {
                Name = $"ExplosionHotFragment_{i:00}",
                Mesh = new BoxMesh { Size = Vector3.One },
                MaterialOverride = _hotFragmentMaterial,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Visible = false,
                Quaternion = new Quaternion(Vector3.Up, direction)
            };
            _hotFragments[i] = fragment;
            AddChild(fragment);
        }
        for (int i = 0; i < SmokePuffCount; i++)
        {
            float angle = i * 2.3999632f + 0.61f;
            float lift = 0.28f + ((i * 23) % 5) * 0.13f;
            _smokeDirections[i] = new Vector3(MathF.Cos(angle), lift, MathF.Sin(angle)).Normalized();
            _smokeWeights[i] = 0.72f + ((i * 19) % 4) * 0.12f;
            MeshInstance3D puff = new()
            {
                Name = $"ExplosionSmokePuff_{i:00}",
                Mesh = new SphereMesh { Radius = 0.5f, Height = 1f, RadialSegments = 8, Rings = 4 },
                MaterialOverride = _smokeMaterial,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Visible = false
            };
            _smokePuffs[i] = puff;
            AddChild(puff);
        }
        Visible = false;
    }

    public void Configure(ExplosionBurstRequest request)
    {
        GlobalPosition = request.Origin;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;
        _color = request.Color;
        _emissionEnergy = request.EmissionEnergy;
        _lightEnergy = request.LightEnergy;
        _flashPersistence = request.FlashPersistence;
        _smokeDelay = request.SmokeDelay;
        _smokeProminence = request.SmokeProminence;
        _ringStrength = request.RingStrength;

        float horizontalRadius = Math.Max(0.25f, Math.Max(request.SourceSize.X, request.SourceSize.Z) * 0.5f);
        float height = Math.Max(0.1f, request.SourceSize.Y);
        _coreDiameter = Math.Max(0.62f, horizontalRadius * 0.82f + height * 0.18f) * request.VisualScale;
        _glareDiameter = _coreDiameter * 2.30f;
        _shockDiameter = Math.Max(1.1f, horizontalRadius * 2.40f) * request.VisualScale;
        _lightRange = Math.Max(1f, horizontalRadius * request.LightRangeMultiplier * request.VisualScale);

        float groundLocalY = request.GroundWorldY - request.Origin.Y + 0.035f;
        _coreCenterY = Math.Max(0.04f, height * 0.08f);
        _core.Position = Vector3.Up * _coreCenterY;
        _glare.Position = Vector3.Up * _coreCenterY;
        _shockRing.Position = Vector3.Up * groundLocalY;
        _light.Position = Vector3.Up * Math.Max(0.18f, height * 0.18f);
        _light.LightColor = _color;
        _light.OmniRange = _lightRange;
        _hotFragmentMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _hotFragmentMaterial.Emission = Colors.White.Lerp(_color, 0.35f);
        _hotFragmentMaterial.EmissionEnergyMultiplier = 0f;
        _lobeMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _lobeMaterial.Emission = _color;
        _lobeMaterial.EmissionEnergyMultiplier = 0f;
        Color smokeTint = new(0.075f + _color.R * 0.055f, 0.072f + _color.G * 0.035f,
            0.068f + _color.B * 0.025f, 0f);
        _smokeMaterial.AlbedoColor = smokeTint;
        _configured = true;
        ApplyFrame(0f);
    }

    public void OnPoolActivated()
    {
        Visible = _configured;
        if (_configured) ApplyFrame(0f);
    }

    public void OnPoolUpdated(float normalizedLifetime, float deltaSeconds)
    {
        if (!_configured) return;
        ApplyFrame(Math.Clamp(normalizedLifetime, 0f, 1f));
    }

    public void OnPoolReleased()
    {
        _configured = false;
        Visible = false;
        _core.Visible = false;
        _glare.Visible = false;
        _shockRing.Visible = false;
        _light.Visible = false;
        _light.LightEnergy = 0f;
        foreach (MeshInstance3D fragment in _hotFragments)
        {
            fragment.Visible = false;
            fragment.Position = Vector3.Zero;
            fragment.Scale = Vector3.One;
        }
        foreach (MeshInstance3D lobe in _fireLobes)
        {
            lobe.Visible = false;
            lobe.Position = Vector3.Zero;
            lobe.Scale = Vector3.One;
        }
        foreach (MeshInstance3D puff in _smokePuffs)
        {
            puff.Visible = false;
            puff.Position = Vector3.Zero;
            puff.Scale = Vector3.One;
        }
        _core.Scale = Vector3.One;
        _glare.Scale = Vector3.One;
        _shockRing.Scale = Vector3.One;
        _core.Position = Vector3.Zero;
        _glare.Position = Vector3.Zero;
        _shockRing.Position = Vector3.Zero;
        _light.Position = Vector3.Zero;
        _coreMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _glareMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _coreMaterial.EmissionEnergyMultiplier = 0f;
        _lobeMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _lobeMaterial.EmissionEnergyMultiplier = 0f;
        _glareMaterial.EmissionEnergyMultiplier = 0f;
        _hotFragmentMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B, 0f);
        _hotFragmentMaterial.EmissionEnergyMultiplier = 0f;
        _smokeMaterial.AlbedoColor = new Color(_smokeMaterial.AlbedoColor.R,
            _smokeMaterial.AlbedoColor.G, _smokeMaterial.AlbedoColor.B, 0f);
        _ringMaterial.SetShaderParameter("ring_alpha", 0f);
        _coreDiameter = 0f;
        _glareDiameter = 0f;
        _shockDiameter = 0f;
        _emissionEnergy = 0f;
        _lightEnergy = 0f;
        _lightRange = 0f;
        _coreCenterY = 0f;
        _flashPersistence = 0f;
        _smokeDelay = 0f;
        _smokeProminence = 0f;
        _ringStrength = 0f;
    }

    /// <summary>
    /// Lightweight Godot smoke hook for the lab/headless runner. It verifies
    /// activation, request-scaled geometry, curve progression and clean reuse.
    /// </summary>
    public static bool ValidateSmoke(Node owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        PooledExplosionBurst burst = new();
        owner.AddChild(burst);
        burst.Configure(new ExplosionBurstRequest(new Vector3(2f, 1.2f, -3f), new Vector3(2f, 1f, 2f),
            0f, new Color(1f, 0.35f, 0.05f), 1f, 8f, 6f, 3f));
        float smallShock = burst.ConfiguredShockDiameter;
        burst.OnPoolActivated();
        float initialLight = burst.CurrentLightEnergy;
        Vector3 initialShockScale = burst.CurrentShockScale;
        bool activated = burst.Visible && burst.CoreVisible && burst.GlareVisible && burst.ShockRingVisible &&
            burst.LightVisible && initialLight > 0f;

        burst.OnPoolUpdated(0.28f, 0.1f);
        bool progressed = burst.CurrentShockScale.X > initialShockScale.X &&
            burst.VisibleHotFragmentCount == HotFragmentCount && burst.VisibleSmokePuffCount == SmokePuffCount &&
            burst.CurrentLightEnergy < initialLight && burst.CurrentLightEnergy >= 0f;
        burst.OnPoolUpdated(0.92f, 0.1f);
        bool lateClean = !burst.CoreVisible && !burst.GlareVisible && !burst.ShockRingVisible && !burst.LightVisible &&
            burst.VisibleHotFragmentCount == 0 && burst.VisibleSmokePuffCount == 0 &&
            Mathf.IsZeroApprox(burst.CurrentLightEnergy);
        burst.OnPoolReleased();
        bool released = !burst.Visible && !burst.CoreVisible && !burst.GlareVisible && !burst.ShockRingVisible &&
            burst.VisibleHotFragmentCount == 0 && burst.VisibleSmokePuffCount == 0 &&
            !burst.LightVisible && Mathf.IsZeroApprox(burst.CurrentLightEnergy);

        burst.Configure(new ExplosionBurstRequest(Vector3.Zero, new Vector3(8f, 4f, 6f), 0f,
            new Color(0.45f, 0.8f, 1f)));
        bool requestScaled = burst.ConfiguredShockDiameter > smallShock * 2f && burst.ConfiguredCoreDiameter > 0f;
        burst.Configure(new ExplosionBurstRequest(Vector3.Zero, new Vector3(2f, 1f, 2f), 0f,
            new Color(1f, 0.35f, 0.05f), flashPersistence: 1.8f));
        burst.OnPoolActivated();
        burst.OnPoolUpdated(0.92f, 0.1f);
        bool persistenceTailClean = !burst.CoreVisible && !burst.GlareVisible && !burst.LightVisible;
        burst.OnPoolReleased();
        burst.Free();
        return activated && progressed && lateClean && released && requestScaled && persistenceTailClean;
    }

    private void ApplyFrame(float normalizedLifetime)
    {
        float time = Math.Clamp(normalizedLifetime, 0f, 1f);
        float persistence = Math.Max(0.5f, _flashPersistence);
        float coreProgress = Math.Clamp(time / (0.72f * persistence), 0f, 1f);
        float releaseEnvelope = 1f - Smooth01(Math.Clamp((time - 0.72f) / 0.20f, 0f, 1f));
        float coreEnvelope = MathF.Pow(1f - coreProgress, 1.55f) * releaseEnvelope;
        float coreExpansion = EaseOut(Math.Clamp(time / (0.38f * persistence), 0f, 1f));
        float coreScale = _coreDiameter * Mathf.Lerp(0.24f, 0.64f, coreExpansion);
        float glareScale = _glareDiameter * Mathf.Lerp(0.55f, 1.18f, coreExpansion);
        _core.Scale = new Vector3(0.86f, 1.20f, 0.92f) * Math.Max(0.001f, coreScale);
        _glare.Scale = Vector3.One * Math.Max(0.001f, glareScale);
        _core.Visible = Visible && coreEnvelope > 0.012f;
        _glare.Visible = Visible && coreEnvelope > 0.018f;
        Color hotCore = Colors.White.Lerp(_color, Math.Clamp(coreProgress * 0.72f, 0f, 1f));
        _coreMaterial.AlbedoColor = new Color(hotCore.R, hotCore.G, hotCore.B,
            Math.Clamp(coreEnvelope * 0.82f, 0f, 1f));
        _glareMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B,
            Math.Clamp(coreEnvelope * 0.30f, 0f, 1f));
        _coreMaterial.Emission = hotCore;
        _glareMaterial.Emission = _color;
        _coreMaterial.EmissionEnergyMultiplier = _emissionEnergy * coreEnvelope;
        _glareMaterial.EmissionEnergyMultiplier = _emissionEnergy * 0.88f * coreEnvelope;

        Color lobeColor = Colors.White.Lerp(_color, 0.70f + coreProgress * 0.22f);
        _lobeMaterial.AlbedoColor = new Color(lobeColor.R, lobeColor.G, lobeColor.B,
            Math.Clamp(coreEnvelope * 0.84f, 0f, 1f));
        _lobeMaterial.Emission = lobeColor;
        _lobeMaterial.EmissionEnergyMultiplier = _emissionEnergy * 0.74f * coreEnvelope;

        for (int i = 0; i < _fireLobes.Length; i++)
        {
            float weight = 0.68f + ((i * 7) % 5) * 0.075f;
            MeshInstance3D lobe = _fireLobes[i];
            lobe.Position = Vector3.Up * _coreCenterY + FireLobeOffsets[i] * _coreDiameter * (0.90f + coreProgress * 0.32f);
            lobe.Scale = new Vector3(0.72f + (i % 3) * 0.13f, 0.92f + ((i * 2) % 4) * 0.12f,
                0.68f + ((i + 1) % 3) * 0.12f) *
                Math.Max(0.001f, coreScale * weight);
            lobe.Visible = Visible && coreEnvelope > 0.012f;
        }

        float fragmentProgress = Math.Clamp(time / 0.72f, 0f, 1f);
        float fragmentEnvelope = MathF.Sin(fragmentProgress * MathF.PI) * MathF.Pow(1f - fragmentProgress, 0.42f);
        float fragmentDistance = _coreDiameter * Mathf.Lerp(0.20f, 1.36f, EaseOut(fragmentProgress));
        float fragmentLength = _coreDiameter * Mathf.Lerp(0.34f, 0.10f, fragmentProgress);
        float fragmentWidth = _coreDiameter * Mathf.Lerp(0.060f, 0.025f, fragmentProgress);
        _hotFragmentMaterial.AlbedoColor = new Color(_color.R, _color.G, _color.B,
            Math.Clamp(fragmentEnvelope * 1.25f, 0f, 1f));
        _hotFragmentMaterial.EmissionEnergyMultiplier = _emissionEnergy * 1.18f * fragmentEnvelope;
        for (int i = 0; i < _hotFragments.Length; i++)
        {
            MeshInstance3D fragment = _hotFragments[i];
            Vector3 direction = _hotFragmentDirections[i];
            float weight = _hotFragmentWeights[i];
            fragment.Position = direction * fragmentDistance * weight;
            fragment.Scale = new Vector3(fragmentWidth, fragmentLength * weight, fragmentWidth);
            fragment.Visible = Visible && fragmentEnvelope > 0.018f;
        }

        float smokeProgress = Math.Clamp((time - _smokeDelay) / Math.Max(0.10f, 1f - _smokeDelay), 0f, 1f);
        float smokeIn = Smooth01(Math.Clamp(smokeProgress / 0.22f, 0f, 1f));
        float smokeOut = 1f - Smooth01(Math.Clamp((smokeProgress - 0.54f) / 0.36f, 0f, 1f));
        float smokeEnvelope = smokeIn * smokeOut;
        float smokeDistance = _coreDiameter * Mathf.Lerp(0.08f, 0.58f, EaseOut(smokeProgress));
        float smokeScale = _coreDiameter * Mathf.Lerp(0.20f, 0.42f, EaseOut(smokeProgress));
        _smokeMaterial.AlbedoColor = new Color(_smokeMaterial.AlbedoColor.R, _smokeMaterial.AlbedoColor.G,
            _smokeMaterial.AlbedoColor.B, Math.Clamp(smokeEnvelope * _smokeProminence, 0f, 1f));
        for (int i = 0; i < _smokePuffs.Length; i++)
        {
            MeshInstance3D puff = _smokePuffs[i];
            float weight = _smokeWeights[i];
            Vector3 direction = _smokeDirections[i];
            puff.Position = Vector3.Up * _coreCenterY + direction * smokeDistance * weight +
                Vector3.Up * (_coreDiameter * smokeProgress * (0.18f + i * 0.018f));
            puff.Scale = new Vector3(1.05f, 0.82f + i * 0.025f, 1f) * smokeScale * weight;
            puff.Visible = Visible && smokeEnvelope > 0.012f;
        }

        float ringProgress = Math.Clamp(time / 0.84f, 0f, 1f);
        float ringExpansion = EaseOut(ringProgress);
        float ringEnvelope = MathF.Sin(ringProgress * MathF.PI) * MathF.Pow(1f - ringProgress, 0.28f);
        float ringScale = _shockDiameter * Mathf.Lerp(0.22f, 1f, ringExpansion);
        _shockRing.Scale = new Vector3(Math.Max(0.001f, ringScale), Math.Max(0.001f, ringScale), 1f);
        _shockRing.Visible = Visible && ringProgress < 0.995f;
        _ringMaterial.SetShaderParameter("ring_color", _color);
        _ringMaterial.SetShaderParameter("ring_alpha", Math.Clamp(ringEnvelope * _ringStrength, 0f, 1f));
        _ringMaterial.SetShaderParameter("ring_energy", _emissionEnergy * 0.34f);

        float lightProgress = Math.Clamp(time / (0.68f * persistence), 0f, 1f);
        float lightEnvelope = MathF.Pow(1f - Smooth01(lightProgress), 1.20f) * releaseEnvelope;
        _light.OmniRange = _lightRange * Mathf.Lerp(0.62f, 1f, EaseOut(lightProgress));
        _light.LightEnergy = _lightEnergy * lightEnvelope;
        _light.Visible = Visible && _light.LightEnergy > 0.01f;
    }

    private static StandardMaterial3D CreateCoreMaterial() => new()
    {
        AlbedoColor = new Color(1f, 1f, 1f, 0f),
        EmissionEnabled = true,
        Emission = Colors.White,
        EmissionEnergyMultiplier = 0f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        BlendMode = BaseMaterial3D.BlendModeEnum.Add,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        RenderPriority = 4
    };

    private static StandardMaterial3D CreateGlareMaterial()
    {
        Gradient gradient = new();
        gradient.SetColor(0, Colors.White);
        gradient.SetColor(1, new Color(1f, 1f, 1f, 0f));
        GradientTexture2D texture = new()
        {
            Gradient = gradient,
            Width = 64,
            Height = 64,
            Fill = GradientTexture2D.FillEnum.Radial,
            FillFrom = new Vector2(0.5f, 0.5f),
            FillTo = new Vector2(1f, 0.5f),
            UseHdr = true
        };
        return new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 1f, 1f, 0f),
            AlbedoTexture = texture,
            EmissionEnabled = true,
            Emission = Colors.White,
            EmissionTexture = texture,
            EmissionEnergyMultiplier = 0f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            BlendMode = BaseMaterial3D.BlendModeEnum.Add,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            NoDepthTest = false,
            RenderPriority = 5
        };
    }

    private static StandardMaterial3D CreateHotFragmentMaterial() => new()
    {
        AlbedoColor = new Color(1f, 0.45f, 0.08f, 0f),
        EmissionEnabled = true,
        Emission = Colors.White,
        EmissionEnergyMultiplier = 0f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        BlendMode = BaseMaterial3D.BlendModeEnum.Add,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        RenderPriority = 4
    };

    private static StandardMaterial3D CreateSmokeMaterial() => new()
    {
        AlbedoColor = new Color(0.12f, 0.10f, 0.09f, 0f),
        Roughness = 0.96f,
        Metallic = 0f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        BlendMode = BaseMaterial3D.BlendModeEnum.Mix,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled,
        RenderPriority = -4
    };

    private static ShaderMaterial CreateRingMaterial()
    {
        Shader shader = new()
        {
            Code = """
shader_type spatial;
render_mode unshaded, blend_add, depth_draw_never, cull_disabled, fog_disabled;
uniform vec4 ring_color : source_color = vec4(1.0, 0.35, 0.05, 1.0);
uniform float ring_alpha = 0.0;
uniform float ring_energy = 3.0;
void fragment() {
    vec2 centered = UV * 2.0 - 1.0;
    float angle = dot(centered, centered) < 0.00000001 ? 0.0 : atan(centered.y, centered.x);
    float wobble = sin(angle * 7.0) * 0.018 + sin(angle * 11.0 + 1.7) * 0.011;
    float radius = length(centered) + wobble;
    float inner = smoothstep(0.52, 0.67, radius);
    float outer = 1.0 - smoothstep(0.70, 0.92, radius);
    float band = inner * outer;
    float ground_flash = (1.0 - smoothstep(0.0, 0.72, radius)) * 0.16;
    ALBEDO = ring_color.rgb;
    EMISSION = ring_color.rgb * ring_energy;
    ALPHA = (band + ground_flash) * ring_alpha;
}
"""
        };
        return new ShaderMaterial { Shader = shader, RenderPriority = 1 };
    }

    private static float EaseOut(float value)
    {
        float clamped = Math.Clamp(value, 0f, 1f);
        return 1f - (1f - clamped) * (1f - clamped);
    }

    private static float Smooth01(float value)
    {
        float clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - 2f * clamped);
    }
}
