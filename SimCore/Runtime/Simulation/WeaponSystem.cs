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
                !world.Content.TryGetWeapon(state.WeaponProfile, out WeaponDefinition weapon) ||
                !world.Entities.Targeting.TryGet(source, out Targeting targeting) || targeting.CurrentTarget == EntityId.None ||
                !TargetingSystem.IsLegalTarget(world, source, targeting.CurrentTarget, requireVisible: true) ||
                !world.Entities.Transform.TryGet(source, out SimTransform sourceTransform) ||
                !world.Entities.Transform.TryGet(targeting.CurrentTarget, out SimTransform targetTransform) ||
                !world.Entities.Ownership.TryGet(source, out Ownership ownership) ||
                !BrownoutSystem.IsOperational(world, source)) continue;

            long dx = (long)targetTransform.Position.X.Raw - sourceTransform.Position.X.Raw;
            long dy = (long)targetTransform.Position.Y.Raw - sourceTransform.Position.Y.Raw;
            long distanceSquared = dx * dx + dy * dy;
            long maximum = weapon.Range.Raw;
            long minimum = weapon.MinimumRange.Raw;
            if (distanceSquared > maximum * maximum || distanceSquared < minimum * minimum) continue;

            if (weapon.RequiresLineOfSight)
            {
                int sx = sourceTransform.Position.X.FloorToInt(), sy = sourceTransform.Position.Y.FloorToInt();
                int tx = targetTransform.Position.X.FloorToInt(), ty = targetTransform.Position.Y.FloorToInt();
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
