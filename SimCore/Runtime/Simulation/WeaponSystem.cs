using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Authoritative readiness, firing-event and cooldown execution. Resolution begins in T042-T044.</summary>
public sealed class WeaponSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!world.Entities.Weapon.Has(source)) continue;
            ref WeaponState state = ref world.Entities.Weapon.Get(source);
            if (state.CooldownRemainingTicks > 0) state.CooldownRemainingTicks--;
            if (state.CooldownRemainingTicks > 0 ||
                TransportSystem.IsAttackLocked(world, source) ||
                (world.Entities.Health.TryGet(source, out Health sourceHealth) && sourceHealth.IsDepleted) ||
                !world.Content.TryGetWeapon(state.WeaponProfile, out WeaponDefinition weapon) ||
                !world.Entities.Targeting.TryGet(source, out Targeting targeting) || targeting.CurrentTarget == EntityId.None ||
                !TargetingSystem.IsLegalTarget(world, source, targeting.CurrentTarget, requireVisible: true) ||
                !world.Entities.Transform.TryGet(source, out SimTransform sourceTransform) ||
                !world.Entities.Transform.TryGet(targeting.CurrentTarget, out SimTransform targetTransform) ||
                !world.Entities.Ownership.TryGet(source, out Ownership ownership) ||
                !BrownoutSystem.IsOperational(world, source)) continue;

            if (weapon.DeliveryKind == WeaponDeliveryKind.Contact)
            {
                Fix32 gap = CombatGeometry.ContactGap(world, source, targeting.CurrentTarget);
                if (gap > weapon.Range || gap < weapon.MinimumRange) continue;
                Angle16 targetFacing = Angle16.FromDirection(targetTransform.Position - sourceTransform.Position);
                if (System.Math.Abs(Angle16.ShortestDelta(sourceTransform.Orientation, targetFacing)) > weapon.FacingToleranceAngle16) continue;
                if (world.Entities.Movement.TryGet(source, out Movement movement) &&
                    movement.CurrentSpeed > movement.MaxSpeed * Fix32.FromRatio(weapon.MaximumMovingFireSpeedBasisPoints, 10_000)) continue;
            }
            else
            {
                Fix32 gap = CombatGeometry.RangedGap(world, source, targeting.CurrentTarget);
                if (gap > weapon.Range || gap < weapon.MinimumRange) continue;
            }

            if (weapon.RequiresLineOfSight)
            {
                FixVec2 aim = CombatGeometry.AimPoint(world, source, targeting.CurrentTarget);
                int sx = sourceTransform.Position.X.FloorToInt(), sy = sourceTransform.Position.Y.FloorToInt();
                int tx = aim.X.FloorToInt(), ty = aim.Y.FloorToInt();
                if (!VisionSystem.HasLineOfSight(world.Map, sx, sy, tx, ty)) continue;
            }

            state.FireSequence = checked(state.FireSequence + 1);
            state.LastFiredTarget = targeting.CurrentTarget;
            state.LastFiredTick = world.Tick.Value;
            state.CooldownRemainingTicks = weapon.CooldownTicks;
        }
    }
}
}
