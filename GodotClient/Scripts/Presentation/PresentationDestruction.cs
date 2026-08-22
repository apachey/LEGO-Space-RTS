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
            PresentationDestructionScaleBand.Heavy => 10,
            PresentationDestructionScaleBand.Massive => 14,
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
            CastShadow = GeometryInstance3D.ShadowCastingSetting.On,
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
