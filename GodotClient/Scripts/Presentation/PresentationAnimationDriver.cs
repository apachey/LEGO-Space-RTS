using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public enum PresentationAnimationTier : byte
{
    Important = 0,
    Normal = 1,
    Distant = 2
}

public readonly struct PresentationAnimationInput
{
    public readonly uint EntityKey;
    public readonly float SpeedWorldPerSecond;
    public readonly bool IsMoving;
    public readonly bool IsOperating;
    public readonly bool IsRepairing;
    public readonly bool IsTransforming;
    public readonly float TransformationProgress;
    public readonly float HealthRatio;
    public readonly float DestructionProgress;
    public readonly uint FireSequence;
    public readonly bool IsImportant;
    public readonly float DistanceBuildCells;

    public PresentationAnimationInput(uint entityKey, float speedWorldPerSecond, bool isMoving, bool isOperating,
        bool isRepairing, bool isTransforming, float transformationProgress, float healthRatio,
        float destructionProgress, uint fireSequence, bool isImportant, float distanceBuildCells)
    {
        EntityKey = entityKey;
        SpeedWorldPerSecond = Math.Max(0f, speedWorldPerSecond);
        IsMoving = isMoving;
        IsOperating = isOperating;
        IsRepairing = isRepairing;
        IsTransforming = isTransforming;
        TransformationProgress = Math.Clamp(transformationProgress, 0f, 1f);
        HealthRatio = Math.Clamp(healthRatio, 0f, 1f);
        DestructionProgress = Math.Clamp(destructionProgress, 0f, 1f);
        FireSequence = fireSequence;
        IsImportant = isImportant;
        DistanceBuildCells = Math.Max(0f, distanceBuildCells);
    }

    public static PresentationAnimationInput FromSnapshots(PresentationEntity previous, PresentationEntity current,
        float snapshotSeconds, bool isImportant, float distanceBuildCells)
    {
        float seconds = Math.Max(0.001f, snapshotSeconds);
        Vector3 previousWorld = previous.Position.ToWorld();
        Vector3 currentWorld = current.Position.ToWorld();
        float speed = previousWorld.DistanceTo(currentWorld) / seconds;
        float healthRatio = current.HasHealth && current.MaximumHitPointsRaw > 0
            ? Math.Clamp((float)current.CurrentHitPointsRaw / current.MaximumHitPointsRaw, 0f, 1f)
            : 1f;
        bool contactOperation = current.WeaponDelivery == WeaponDeliveryKind.Contact &&
            current.WeaponFireSequence != previous.WeaponFireSequence;
        return new PresentationAnimationInput(current.EntityId.Value, speed,
            current.Movement == MovementState.Moving, contactOperation, current.IsRepairing,
            current.TransformationPhase != TransformationPhase.Idle,
            current.TransformationProgressBasisPoints / 10_000f, healthRatio,
            current.DestructionProgressBasisPoints / 10_000f, current.WeaponFireSequence,
            isImportant, distanceBuildCells);
    }
}

public sealed class PresentationAnimationTuning
{
    public bool Enabled { get; set; } = true;
    public float LocomotionReferenceSpeed { get; set; } = 3f;
    public float BlendResponse { get; set; } = 8f;
    public float WheelTurnsPerWorldUnit { get; set; } = 0.34f;
    public float SuspensionAmplitude { get; set; } = 0.035f;
    public float SuspensionFrequency { get; set; } = 1.8f;
    public float BodyLeanDegrees { get; set; } = 1.2f;
    public float DrillTurnsPerSecond { get; set; } = 0.9f;
    public float RecoilDistance { get; set; } = 0.12f;
    public float RecoilRecovery { get; set; } = 7f;
    public float TransformationLift { get; set; } = 0.18f;
    public float TransformationTiltDegrees { get; set; } = 8f;
    public float DamageWobbleDegrees { get; set; } = 0.8f;
    public int TierOverride { get; set; }

    public void Normalize()
    {
        LocomotionReferenceSpeed = Math.Clamp(LocomotionReferenceSpeed, 0.1f, 20f);
        BlendResponse = Math.Clamp(BlendResponse, 0.1f, 30f);
        WheelTurnsPerWorldUnit = Math.Clamp(WheelTurnsPerWorldUnit, 0f, 2f);
        SuspensionAmplitude = Math.Clamp(SuspensionAmplitude, 0f, 0.3f);
        SuspensionFrequency = Math.Clamp(SuspensionFrequency, 0f, 8f);
        BodyLeanDegrees = Math.Clamp(BodyLeanDegrees, 0f, 12f);
        DrillTurnsPerSecond = Math.Clamp(DrillTurnsPerSecond, 0f, 5f);
        RecoilDistance = Math.Clamp(RecoilDistance, 0f, 0.8f);
        RecoilRecovery = Math.Clamp(RecoilRecovery, 0.1f, 30f);
        TransformationLift = Math.Clamp(TransformationLift, 0f, 1.5f);
        TransformationTiltDegrees = Math.Clamp(TransformationTiltDegrees, -45f, 45f);
        DamageWobbleDegrees = Math.Clamp(DamageWobbleDegrees, 0f, 12f);
        TierOverride = Math.Clamp(TierOverride, 0, 3);
    }
}

public readonly struct PresentationAnimationFrame
{
    public readonly PresentationAnimationTier Tier;
    public readonly bool ParametersUpdated;
    public readonly float LocomotionBlend;
    public readonly float OperationBlend;
    public readonly float RepairBlend;
    public readonly float WheelPhaseRadians;
    public readonly float DrillPhaseRadians;
    public readonly float SuspensionOffset;
    public readonly float BodyLeanRadians;
    public readonly float Recoil;
    public readonly float TransformationProgress;
    public readonly float DamageAmount;
    public readonly float DestructionProgress;

    public PresentationAnimationFrame(PresentationAnimationTier tier, bool parametersUpdated, float locomotionBlend,
        float operationBlend, float repairBlend, float wheelPhaseRadians, float drillPhaseRadians,
        float suspensionOffset, float bodyLeanRadians, float recoil, float transformationProgress,
        float damageAmount, float destructionProgress)
    {
        Tier = tier;
        ParametersUpdated = parametersUpdated;
        LocomotionBlend = locomotionBlend;
        OperationBlend = operationBlend;
        RepairBlend = repairBlend;
        WheelPhaseRadians = wheelPhaseRadians;
        DrillPhaseRadians = drillPhaseRadians;
        SuspensionOffset = suspensionOffset;
        BodyLeanRadians = bodyLeanRadians;
        Recoil = recoil;
        TransformationProgress = transformationProgress;
        DamageAmount = damageAmount;
        DestructionProgress = destructionProgress;
    }
}

public sealed class PresentationAnimationDriver
{
    private const float NormalInterval = 1f / 30f;
    private const float DistantInterval = 1f / 15f;
    private readonly Dictionary<uint, DriverState> _states = new();

    public int TrackedEntityCount => _states.Count;

    public PresentationAnimationFrame Update(PresentationAnimationInput input, float renderDelta,
        PresentationAnimationTuning tuning)
    {
        tuning.Normalize();
        float delta = Math.Clamp(renderDelta, 0f, 0.25f);
        if (!_states.TryGetValue(input.EntityKey, out DriverState? state))
        {
            state = new DriverState { LastFireSequence = input.FireSequence };
            _states.Add(input.EntityKey, state);
        }

        PresentationAnimationTier tier = ResolveTier(input, tuning.TierOverride);
        float interval = tier switch
        {
            PresentationAnimationTier.Normal => NormalInterval,
            PresentationAnimationTier.Distant => DistantInterval,
            _ => 0f
        };
        state.ParameterAccumulator += delta;
        bool parameterUpdate = interval <= 0f || state.ParameterAccumulator >= interval;

        if (input.FireSequence > state.LastFireSequence)
            state.Recoil = tuning.Enabled ? 1f : 0f;
        state.LastFireSequence = Math.Max(state.LastFireSequence, input.FireSequence);
        state.Recoil = Math.Max(0f, state.Recoil - delta * tuning.RecoilRecovery);

        if (parameterUpdate)
        {
            float sampleDelta = Math.Max(delta, state.ParameterAccumulator);
            state.ParameterAccumulator = 0f;
            float targetLocomotion = tuning.Enabled && input.IsMoving
                ? Math.Clamp(input.SpeedWorldPerSecond / tuning.LocomotionReferenceSpeed, 0f, 1.5f)
                : 0f;
            float targetOperation = tuning.Enabled && input.IsOperating ? 1f : 0f;
            float targetRepair = tuning.Enabled && input.IsRepairing ? 1f : 0f;
            float blend = 1f - MathF.Exp(-tuning.BlendResponse * sampleDelta);
            state.Locomotion += (targetLocomotion - state.Locomotion) * blend;
            state.Operation += (targetOperation - state.Operation) * blend;
            state.Repair += (targetRepair - state.Repair) * blend;
            state.Transformation = tuning.Enabled && input.IsTransforming ? input.TransformationProgress : 0f;
            state.Damage = tuning.Enabled ? 1f - input.HealthRatio : 0f;
            state.Destruction = tuning.Enabled ? input.DestructionProgress : 0f;
            float rollingSpeed = tuning.Enabled && input.IsMoving ? input.SpeedWorldPerSecond : 0f;
            state.WheelPhase = WrapRadians(state.WheelPhase + rollingSpeed *
                tuning.WheelTurnsPerWorldUnit * MathF.Tau * sampleDelta);
            state.DrillPhase = WrapRadians(state.DrillPhase + tuning.DrillTurnsPerSecond * MathF.Tau *
                Math.Max(state.Operation, state.Repair) * sampleDelta);
            state.SuspensionPhase = WrapRadians(state.SuspensionPhase +
                tuning.SuspensionFrequency * MathF.Tau * sampleDelta);
        }

        float suspension = MathF.Sin(state.SuspensionPhase) * tuning.SuspensionAmplitude *
            Math.Clamp(state.Locomotion, 0f, 1f);
        float lean = MathF.Sin(state.SuspensionPhase * 0.5f) * Mathf.DegToRad(tuning.BodyLeanDegrees) *
            Math.Clamp(state.Locomotion, 0f, 1f);
        lean += MathF.Sin(state.SuspensionPhase * 0.73f + 0.8f) *
            Mathf.DegToRad(tuning.DamageWobbleDegrees) * state.Damage;

        return new PresentationAnimationFrame(tier, parameterUpdate, state.Locomotion, state.Operation,
            state.Repair, state.WheelPhase, state.DrillPhase, suspension, lean, state.Recoil,
            state.Transformation, state.Damage, state.Destruction);
    }

    public void Remove(uint entityKey) => _states.Remove(entityKey);
    public void Clear() => _states.Clear();

    private static PresentationAnimationTier ResolveTier(PresentationAnimationInput input, int tierOverride)
    {
        if (tierOverride is >= 1 and <= 3) return (PresentationAnimationTier)(tierOverride - 1);
        if (input.IsImportant || input.IsOperating || input.IsRepairing || input.IsTransforming)
            return PresentationAnimationTier.Important;
        return input.DistanceBuildCells <= 48f ? PresentationAnimationTier.Normal : PresentationAnimationTier.Distant;
    }

    private static float WrapRadians(float value) => Mathf.PosMod(value, MathF.Tau);

    private sealed class DriverState
    {
        public float ParameterAccumulator;
        public float Locomotion;
        public float Operation;
        public float Repair;
        public float Transformation;
        public float Damage;
        public float Destruction;
        public float WheelPhase;
        public float DrillPhase;
        public float SuspensionPhase;
        public float Recoil;
        public uint LastFireSequence;
    }
}

public sealed class PresentationAnimationRigBinding
{
    private readonly List<(Node3D Node, Vector3 BaseRotation)> _wheels = new();
    private Node3D? _drill;
    private Vector3 _drillBaseRotation;
    private Node3D? _suspension;
    private Vector3 _suspensionBasePosition;
    private Vector3 _suspensionBaseRotation;

    public int WheelCount => _wheels.Count;
    public bool HasDrill => _drill is not null;
    public bool HasSuspension => _suspension is not null;

    public PresentationAnimationRigBinding(Node3D root)
    {
        Collect(root);
        _drill = root.FindChild("Pivot_Drill", true, false) as Node3D;
        _suspension = root.FindChild("Pivot_Suspension", true, false) as Node3D;
        if (_drill is not null) _drillBaseRotation = _drill.Rotation;
        if (_suspension is not null)
        {
            _suspensionBasePosition = _suspension.Position;
            _suspensionBaseRotation = _suspension.Rotation;
        }
    }

    public void Apply(PresentationAnimationFrame frame, PresentationAnimationTuning tuning)
    {
        for (int i = 0; i < _wheels.Count; i++)
        {
            (Node3D wheel, Vector3 baseRotation) = _wheels[i];
            wheel.Rotation = baseRotation + Vector3.Right * frame.WheelPhaseRadians;
        }
        if (_drill is not null)
            _drill.Rotation = _drillBaseRotation + Vector3.Forward * frame.DrillPhaseRadians;
        if (_suspension is null) return;
        float transformLift = frame.TransformationProgress * tuning.TransformationLift;
        float recoil = frame.Recoil * frame.Recoil * tuning.RecoilDistance;
        _suspension.Position = _suspensionBasePosition + Vector3.Up * (frame.SuspensionOffset + transformLift) +
            Vector3.Back * recoil;
        _suspension.Rotation = _suspensionBaseRotation + new Vector3(
            frame.BodyLeanRadians + Mathf.DegToRad(tuning.TransformationTiltDegrees) * frame.TransformationProgress,
            0f, 0f);
    }

    private void Collect(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Node3D node && node.Name.ToString().StartsWith("Pivot_Wheel_", StringComparison.Ordinal))
                _wheels.Add((node, node.Rotation));
            Collect(child);
        }
    }
}
