using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public static class DisplacementSystem
{
    public const int StabilityTicks = 8 * SimClock.TicksPerSecond;
    public static readonly Fix32 StableDistanceMultiplier = Fix32.FromRatio(1, 4);
    private static readonly Fix32 SampleDistance = Fix32.FromRatio(1, 4);

    public static bool IsStable(SimulationWorld world, EntityId target)
        => world.Entities.Stability.TryGet(target, out Stability stability) && world.Tick.Value < stability.UntilTick;

    public static int RemainingStabilityTicks(SimulationWorld world, EntityId target)
        => world.Entities.Stability.TryGet(target, out Stability stability) ? Math.Max(0, stability.UntilTick - world.Tick.Value) : 0;

    public static Fix32 GetCanonicalDistance(DisplacementEffect effect, DisplacementRelation relation, FootprintClass footprint, bool deployed)
    {
        Fix32 distance;
        if (relation == DisplacementRelation.FriendlyTow)
        {
            if (effect != DisplacementEffect.ExcavationClamp) return Fix32.Zero;
            distance = footprint switch
            {
                FootprintClass.Tiny or FootprintClass.Small or FootprintClass.Medium => Fix32.FromInt(3),
                FootprintClass.Large => Fix32.FromRatio(3, 2),
                _ => Fix32.Zero
            };
        }
        else
        {
            distance = effect switch
            {
                DisplacementEffect.DeflectorArm => footprint switch
                {
                    FootprintClass.Tiny or FootprintClass.Small => Fix32.FromRatio(5, 2),
                    FootprintClass.Medium => Fix32.FromRatio(3, 2),
                    FootprintClass.Large => Fix32.FromRatio(3, 4),
                    _ => Fix32.Zero
                },
                DisplacementEffect.GuardSweep => footprint switch
                {
                    FootprintClass.Tiny or FootprintClass.Small => Fix32.FromInt(2),
                    FootprintClass.Medium => Fix32.One,
                    FootprintClass.Large => Fix32.Half,
                    _ => Fix32.Zero
                },
                DisplacementEffect.ExcavationClamp => footprint switch
                {
                    FootprintClass.Tiny or FootprintClass.Small => Fix32.FromInt(3),
                    FootprintClass.Medium => Fix32.FromInt(2),
                    FootprintClass.Large => Fix32.One,
                    _ => Fix32.Zero
                },
                _ => Fix32.Zero
            };
        }
        return deployed && footprint == FootprintClass.Large ? distance * Fix32.Half : distance;
    }

    public static bool TryApply(SimulationWorld world, EntityId source, EntityId target, DisplacementEffect effect,
        DisplacementRelation relation, FixVec2 direction, out FixVec2 resolvedPosition)
    {
        resolvedPosition = default;
        if (source == target || direction.Equals(FixVec2.Zero) || !world.Entities.Exists(source) || !world.Entities.Exists(target) ||
            world.Entities.Building.Has(target) || !world.Entities.Transform.TryGet(target, out SimTransform transform) ||
            !world.Entities.Navigation.TryGet(target, out NavigationAgent navigation) || navigation.Layer == MovementLayer.TrueAir ||
            !world.Entities.Ownership.TryGet(source, out Ownership sourceOwner) || !world.Entities.Ownership.TryGet(target, out Ownership targetOwner)) return false;
        bool hostile = sourceOwner.PlayerSlot != targetOwner.PlayerSlot;
        if ((relation == DisplacementRelation.Hostile) != hostile) return false;
        bool deployed = world.Entities.Deployment.TryGet(target, out Deployment deployment) && deployment.State == DeploymentState.Deployed;
        Fix32 distance = GetCanonicalDistance(effect, relation, navigation.Footprint, deployed);
        if (distance.Raw == 0) return false;
        if (hostile && IsStable(world, target)) distance *= StableDistanceMultiplier;
        FixVec2 unitDirection = direction.NormalizeSafe();
        int steps = (distance * Fix32.FromInt(4)).CeilToInt(); FixVec2 lastLegal = transform.Position;
        for (int i = 1; i <= steps; i++)
        {
            Fix32 sampledDistance = Fix32.Min(distance, SampleDistance * Fix32.FromInt(i));
            FixVec2 candidate = transform.Position + unitDirection * sampledDistance;
            if (!IsLegal(world, target, candidate, navigation)) break;
            lastLegal = candidate;
        }
        if (lastLegal.Equals(transform.Position)) return false;
        world.Entities.Transform.Get(target).Position = lastLegal; resolvedPosition = lastLegal;
        if (world.Entities.Movement.Has(target))
        {
            ref Movement movement = ref world.Entities.Movement.Get(target);
            movement.LastPosition = lastLegal; movement.PathIndex = 0;
        }
        if (navigation.HasTarget)
        {
            ref NavigationAgent targetNavigation = ref world.Entities.Navigation.Get(target);
            targetNavigation.PathDirty = true; targetNavigation.RequestAge = 0;
            world.Corridors.Remove(target.Value);
        }
        if (hostile) world.Entities.Stability.Set(target, new Stability { UntilTick = checked(world.Tick.Value + StabilityTicks) });
        world.Spatial.Rebuild(world.Entities);
        return true;
    }

    private static bool IsLegal(SimulationWorld world, EntityId target, FixVec2 candidate, NavigationAgent targetNavigation)
    {
        if (!world.Pathfinder.IsPassable(MapGrid.BuildToNav(candidate), targetNavigation.Footprint)) return false;
        Fix32 targetRadius = FootprintRules.CollisionRadiusBuild(targetNavigation.Footprint);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId other = alive[i];
            if (other == target || !world.Entities.Transform.TryGet(other, out SimTransform otherTransform) ||
                !world.Entities.Navigation.TryGet(other, out NavigationAgent otherNavigation) || otherNavigation.Layer == MovementLayer.TrueAir) continue;
            Fix32 required = targetRadius + FootprintRules.CollisionRadiusBuild(otherNavigation.Footprint);
            if (FixVec2.Distance(candidate, otherTransform.Position) < required) return false;
        }
        return true;
    }
}
}
