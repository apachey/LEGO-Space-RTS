using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic M4 T048 tactical mode changes with identity-preserving component swaps.</summary>
public sealed class TransformationSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Transformation.TryGet(id, out Transformation state) ||
                (world.Entities.Health.TryGet(id, out Health health) && health.IsDepleted) ||
                !TryGetDefinition(world, id, state, out TransformationDefinition definition)) continue;

            if (state.Phase == TransformationPhase.RollingBack) UpdateRollback(world, id);
            else if (state.Phase == TransformationPhase.Transitioning) UpdateTransition(world, id, definition);
            else if (state.QueuedToggle && world.Tick.Value >= state.ReversalLockedUntilTick)
            {
                ref Transformation stored = ref world.Entities.Transformation.Get(id);
                stored.QueuedToggle = false;
                TryStart(world, id, definition);
            }
        }
    }

    public static bool TryToggle(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Transformation.TryGet(id, out Transformation state) ||
            !TryGetDefinition(world, id, state, out TransformationDefinition definition) ||
            (world.Entities.Health.TryGet(id, out Health health) && health.IsDepleted)) return false;
        if (state.Phase == TransformationPhase.RollingBack) return false;
        if (state.Phase == TransformationPhase.Transitioning)
        {
            if (CanCancel(state, definition)) return BeginRollback(world, id, definition);
            ref Transformation committed = ref world.Entities.Transformation.Get(id);
            committed.QueuedToggle = true;
            return true;
        }
        if (world.Tick.Value < state.ReversalLockedUntilTick) return false;
        return TryStart(world, id, definition);
    }

    public static bool TryCancel(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Transformation.TryGet(id, out Transformation state) || state.Phase != TransformationPhase.Transitioning ||
            !TryGetDefinition(world, id, state, out TransformationDefinition definition) || !CanCancel(state, definition)) return false;
        return BeginRollback(world, id, definition);
    }

    public static bool IsBusy(SimulationWorld world, EntityId id)
        => world.Entities.Transformation.TryGet(id, out Transformation state) && state.Phase != TransformationPhase.Idle;

    public static bool CanAttack(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Transformation.TryGet(id, out Transformation state) || state.Phase == TransformationPhase.Idle) return true;
        return TryGetDefinition(world, id, state, out TransformationDefinition definition) && definition.AttackDuringTransition;
    }

    public static TargetLayerMask EffectiveTargetLayers(SimulationWorld world, EntityId id, Targetable targetable)
    {
        if (world.Entities.Transformation.TryGet(id, out Transformation state) && state.Phase != TransformationPhase.Idle &&
            TryGetDefinition(world, id, state, out TransformationDefinition definition)) return definition.TransitionTargetLayers;
        return targetable.Layer == CombatTargetLayer.Ground ? TargetLayerMask.Ground : TargetLayerMask.TrueAir;
    }

    public static ushort ProgressBasisPoints(Transformation state)
    {
        if (state.Phase == TransformationPhase.Idle || state.TotalTicks == 0) return 0;
        if (state.Phase == TransformationPhase.RollingBack)
            return checked((ushort)((long)state.ProgressTicks * 10_000 / state.TotalTicks));
        return checked((ushort)System.Math.Min(10_000, (long)state.ProgressTicks * 10_000 / state.TotalTicks));
    }

    internal static void ResetToInitial(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Transformation.TryGet(id, out Transformation state) || !TryGetDefinition(world, id, state, out TransformationDefinition definition)) return;
        ApplyMode(world, id, definition.ModeA);
        world.Entities.Transformation.Set(id, new Transformation
        {
            Definition = definition.Id, CurrentState = definition.ModeA.StateId, SourceState = definition.ModeA.StateId,
            DestinationState = definition.ModeA.StateId, Phase = TransformationPhase.Idle
        });
    }

    private static bool TryStart(SimulationWorld world, EntityId id, TransformationDefinition definition)
    {
        ref Transformation state = ref world.Entities.Transformation.Get(id);
        TransformationModeDefinition destination = definition.GetDestination(state.CurrentState);
        ushort duration = definition.GetDuration(state.CurrentState);
        if (destination.StateId.Value == 0 || duration == 0 || !CanEnterMode(world, id, destination)) return false;
        StopForTransition(world, id);
        state.SourceState = state.CurrentState;
        state.DestinationState = destination.StateId;
        state.Phase = TransformationPhase.Transitioning;
        state.ProgressTicks = 0;
        state.TotalTicks = duration;
        state.RollbackTicksRemaining = 0;
        state.QueuedToggle = false;
        return true;
    }

    private static void UpdateTransition(SimulationWorld world, EntityId id, TransformationDefinition definition)
    {
        StopForTransition(world, id);
        ref Transformation state = ref world.Entities.Transformation.Get(id);
        if (state.ProgressTicks < state.TotalTicks) state.ProgressTicks++;
        if (state.ProgressTicks < state.TotalTicks) return;
        TransformationModeDefinition destination = definition.GetMode(state.DestinationState);
        if (destination.StateId.Value == 0 || !CanEnterMode(world, id, destination)) return;
        ApplyMode(world, id, destination);
        bool queued = state.QueuedToggle;
        state.CurrentState = destination.StateId;
        state.SourceState = destination.StateId;
        state.DestinationState = destination.StateId;
        state.Phase = TransformationPhase.Idle;
        state.ProgressTicks = 0;
        state.TotalTicks = 0;
        state.RollbackTicksRemaining = 0;
        state.ReversalLockedUntilTick = checked(world.Tick.Value + definition.ReversalLockTicks);
        state.QueuedToggle = queued;
    }

    private static void UpdateRollback(SimulationWorld world, EntityId id)
    {
        StopForTransition(world, id);
        ref Transformation state = ref world.Entities.Transformation.Get(id);
        if (state.RollbackTicksRemaining > 0) state.RollbackTicksRemaining--;
        if (state.ProgressTicks > 0) state.ProgressTicks--;
        if (state.RollbackTicksRemaining > 0) return;
        state.DestinationState = state.CurrentState;
        state.SourceState = state.CurrentState;
        state.Phase = TransformationPhase.Idle;
        state.ProgressTicks = 0;
        state.TotalTicks = 0;
        state.QueuedToggle = false;
    }

    private static bool BeginRollback(SimulationWorld world, EntityId id, TransformationDefinition definition)
    {
        ref Transformation state = ref world.Entities.Transformation.Get(id);
        state.Phase = TransformationPhase.RollingBack;
        state.TotalTicks = definition.RollbackTicks;
        state.ProgressTicks = definition.RollbackTicks;
        state.RollbackTicksRemaining = definition.RollbackTicks;
        state.QueuedToggle = false;
        StopForTransition(world, id);
        return true;
    }

    private static bool CanCancel(Transformation state, TransformationDefinition definition)
        => state.TotalTicks > 0 && (long)state.ProgressTicks * 10_000 < (long)state.TotalTicks * definition.CancellationThresholdBasisPoints;

    private static void StopForTransition(SimulationWorld world, EntityId id)
    {
        TargetingSystem.ClearTarget(world, id);
        if (!world.Entities.Navigation.Has(id) || !world.Entities.Movement.Has(id)) return;
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(id);
        ref Movement movement = ref world.Entities.Movement.Get(id);
        CommandExecutionSystem.StopMovement(world, id, ref navigation, ref movement);
    }

    private static bool CanEnterMode(SimulationWorld world, EntityId id, TransformationModeDefinition mode)
    {
        if (!world.Content.TryGetMovement(mode.MovementProfileKey, out PrototypeMovementProfile profile) ||
            !world.Entities.Transform.TryGet(id, out SimTransform transform)) return false;
        if (profile.Layer == MovementLayer.TrueAir) return true;
        NavCell cell = MapGrid.BuildToNav(transform.Position);
        if (!world.Pathfinder.IsPassable(cell, mode.Footprint)) return false;
        Fix32 radius = FootprintRules.CollisionRadiusBuild(mode.Footprint);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId other = alive[i];
            if (other == id || !world.Entities.Transform.TryGet(other, out SimTransform otherTransform) ||
                !world.Entities.Navigation.TryGet(other, out NavigationAgent otherNavigation) || otherNavigation.Layer == MovementLayer.TrueAir) continue;
            if (FixVec2.Distance(transform.Position, otherTransform.Position) < radius + FootprintRules.CollisionRadiusBuild(otherNavigation.Footprint)) return false;
        }
        return true;
    }

    private static void ApplyMode(SimulationWorld world, EntityId id, TransformationModeDefinition mode)
    {
        PrototypeMovementProfile profile = world.Content.TryGetMovement(mode.MovementProfileKey, out PrototypeMovementProfile found)
            ? found : throw new System.InvalidOperationException($"Missing transformation movement profile {mode.MovementProfileKey}.");
        ref Movement movement = ref world.Entities.Movement.Get(id);
        movement.MaxSpeed = profile.MaxSpeed; movement.Acceleration = profile.Acceleration; movement.Deceleration = profile.Deceleration;
        movement.TurnRatePerTick = profile.TurnRatePerTick; movement.ReversePolicy = profile.ReversePolicy;
        movement.CurrentSpeed = Fix32.Zero; movement.CurrentVelocity = FixVec2.Zero; movement.DesiredMovement = FixVec2.Zero; movement.PathIndex = 0; movement.State = MovementState.Idle;
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(id);
        navigation.Footprint = mode.Footprint; navigation.Layer = profile.Layer; navigation.Target = world.Entities.Transform.Get(id).Position;
        navigation.HasTarget = false; navigation.PathDirty = false; navigation.Formation = default; navigation.RequestAge = 0; navigation.PathTopologyVersion = world.Map.TopologyVersion;
        world.Corridors.Remove(id.Value);

        PrototypeCombatProfile combat = mode.Combat;
        world.Entities.Targetable.Set(id, new Targetable { Class = combat.TargetClass, Layer = combat.TargetLayer, Flags = combat.TargetFlags });
        ref Health health = ref world.Entities.Health.Get(id);
        health.Maximum = Fix32.FromInt(combat.MaximumHitPoints); health.Current = Fix32.Min(health.Current, health.Maximum); health.ArmorRating = combat.ArmorRating;
        world.Entities.Targeting.Set(id, new Targeting
        {
            CurrentTarget = EntityId.None, AcquisitionRadius = combat.AcquisitionRadius, LegalLayers = combat.LegalTargetLayers,
            LegalClasses = combat.LegalTargetClasses, PriorityProfile = combat.PriorityProfile, SelectionKind = TargetSelectionKind.None
        });
        WeaponState oldWeapon = world.Entities.Weapon.Get(id);
        ushort remappedCooldown = 0;
        if (world.Content.TryGetWeapon(oldWeapon.WeaponProfile, out WeaponDefinition oldDefinition) &&
            world.Content.TryGetWeapon(combat.WeaponProfile, out WeaponDefinition newDefinition) && oldWeapon.CooldownRemainingTicks > 0)
            remappedCooldown = checked((ushort)System.Math.Min(newDefinition.CooldownTicks,
                ((long)oldWeapon.CooldownRemainingTicks * newDefinition.CooldownTicks + oldDefinition.CooldownTicks - 1) / oldDefinition.CooldownTicks));
        oldWeapon.WeaponProfile = combat.WeaponProfile; oldWeapon.CooldownRemainingTicks = remappedCooldown;
        world.Entities.Weapon.Set(id, oldWeapon);
        ref Vision vision = ref world.Entities.Vision.Get(id);
        vision.RadiusBuildCells = mode.VisionRadius; vision.IsAirVision = profile.Layer == MovementLayer.TrueAir; vision.LastFogX = -1; vision.LastFogY = -1;
    }

    private static bool TryGetDefinition(SimulationWorld world, EntityId id, Transformation state, out TransformationDefinition definition)
    {
        if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) || !world.Content.TryGetTransformation(selectable.ContentType, out definition) || definition.Id != state.Definition)
        { definition = default; return false; }
        return true;
    }
}
}
