using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic visibility-safe target selection. Weapon execution begins in T041.</summary>
public sealed class TargetingSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!world.Entities.Targeting.TryGet(source, out Targeting targeting) ||
                !world.Entities.Transform.TryGet(source, out SimTransform sourceTransform) ||
                !world.Entities.Ownership.TryGet(source, out Ownership ownership)) continue;

            if (targeting.SelectionKind == TargetSelectionKind.DirectOrder)
            {
                if (IsLegalTarget(world, source, targeting.CurrentTarget, requireVisible: true)) continue;
                ClearTarget(world, source);
                if (world.TryGetQueue(source, out UnitCommandQueue queue) && queue.Count > 0) CommandExecutionSystem.TryStartNextOrder(world, source);
                continue;
            }

            if (targeting.PriorityProfile == TargetPriorityProfile.Support)
            {
                ClearTarget(world, source);
                continue;
            }

            int queryRadius = checked((targeting.AcquisitionRadius.Raw + Fix32.OneRaw - 1) / Fix32.OneRaw);
            world.Spatial.Query(sourceTransform.Position, queryRadius, world.ScratchEntities);
            EntityId best = EntityId.None;
            int bestRank = int.MaxValue;
            long bestDistanceSquared = long.MaxValue;
            long radiusRaw = targeting.AcquisitionRadius.Raw;
            long radiusSquared = radiusRaw * radiusRaw;
            for (int c = 0; c < world.ScratchEntities.Count; c++)
            {
                EntityId candidate = world.ScratchEntities[c];
                if (!IsLegalTarget(world, source, candidate, requireVisible: true) ||
                    !world.Entities.Targetable.TryGet(candidate, out Targetable targetable) ||
                    !world.Entities.Transform.TryGet(candidate, out SimTransform candidateTransform)) continue;
                if (targeting.PriorityProfile == TargetPriorityProfile.Scout && (targetable.Flags & CombatTargetFlags.CombatThreat) == 0) continue;
                long dx = (long)candidateTransform.Position.X.Raw - sourceTransform.Position.X.Raw;
                long dy = (long)candidateTransform.Position.Y.Raw - sourceTransform.Position.Y.Raw;
                long distanceSquared = dx * dx + dy * dy;
                if (distanceSquared > radiusSquared) continue;
                int rank = PriorityRank(targeting.PriorityProfile, targetable);
                if (best == EntityId.None || rank < bestRank ||
                    (rank == bestRank && (distanceSquared < bestDistanceSquared ||
                    (distanceSquared == bestDistanceSquared && candidate.Value < best.Value))))
                {
                    best = candidate; bestRank = rank; bestDistanceSquared = distanceSquared;
                }
            }

            ref Targeting stored = ref world.Entities.Targeting.Get(source);
            stored.CurrentTarget = best;
            stored.SelectionKind = best == EntityId.None ? TargetSelectionKind.None : TargetSelectionKind.Automatic;
        }
    }

    public static bool TryIssueDirectOrder(SimulationWorld world, byte playerSlot, EntityId source, EntityId target)
    {
        if (!world.Entities.Targeting.Has(source) || !world.Entities.Ownership.TryGet(source, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !IsLegalTarget(world, source, target, requireVisible: true)) return false;
        ref Targeting targeting = ref world.Entities.Targeting.Get(source);
        targeting.CurrentTarget = target;
        targeting.SelectionKind = TargetSelectionKind.DirectOrder;
        return true;
    }

    public static void ClearTarget(SimulationWorld world, EntityId source)
    {
        if (!world.Entities.Targeting.Has(source)) return;
        ref Targeting targeting = ref world.Entities.Targeting.Get(source);
        targeting.CurrentTarget = EntityId.None;
        targeting.SelectionKind = TargetSelectionKind.None;
    }

    public static bool IsLegalTarget(SimulationWorld world, EntityId source, EntityId target, bool requireVisible)
    {
        if (target == EntityId.None || source == target || !world.Entities.Exists(source) || !world.Entities.Exists(target) ||
            !world.Entities.Targeting.TryGet(source, out Targeting targeting) || !world.Entities.Targetable.TryGet(target, out Targetable targetable) ||
            !world.Entities.Ownership.TryGet(source, out Ownership sourceOwner) || !world.Entities.Ownership.TryGet(target, out Ownership targetOwner) ||
            sourceOwner.PlayerSlot == targetOwner.PlayerSlot || !world.Entities.Transform.TryGet(target, out SimTransform transform)) return false;
        TargetLayerMask layer = targetable.Layer == CombatTargetLayer.Ground ? TargetLayerMask.Ground : TargetLayerMask.TrueAir;
        TargetClassMask targetClass = (TargetClassMask)(1 << (int)targetable.Class);
        if ((targeting.LegalLayers & layer) == 0 || (targeting.LegalClasses & targetClass) == 0) return false;
        if (!requireVisible) return true;
        int x = transform.Position.X.FloorToInt(), y = transform.Position.Y.FloorToInt();
        return (uint)x < (uint)FogState.Width && (uint)y < (uint)FogState.Height && world.Fog.IsVisible(sourceOwner.PlayerSlot, x, y);
    }

    private static int PriorityRank(TargetPriorityProfile profile, Targetable target) => profile switch
    {
        TargetPriorityProfile.AntiLight => target.Class == CombatTargetClass.Personnel ? 0 : target.Class == CombatTargetClass.LightMachine ? 1 : Has(target, CombatTargetFlags.CombatThreat) ? 2 : 3,
        TargetPriorityProfile.AntiHeavy => target.Class == CombatTargetClass.HeavyMachine ? 0 : target.Class == CombatTargetClass.MassiveMachine ? 1 : target.Class == CombatTargetClass.MediumMachine ? 2 : 3,
        TargetPriorityProfile.AntiAir => Has(target, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support) ? 0 : Has(target, CombatTargetFlags.Transport) ? 1 : 2,
        TargetPriorityProfile.Siege => Has(target, CombatTargetFlags.DefensiveStructure) ? 0 : Has(target, CombatTargetFlags.Production) ? 1 : Has(target, CombatTargetFlags.EconomicInfrastructure) ? 2 : Has(target, CombatTargetFlags.Command) ? 3 : 4,
        TargetPriorityProfile.Harassment => Has(target, CombatTargetFlags.Worker) ? 0 : Has(target, CombatTargetFlags.EconomicInfrastructure) ? 1 : target.Class == CombatTargetClass.LightMachine && Has(target, CombatTargetFlags.Support) ? 2 : 3,
        TargetPriorityProfile.Generalist => Has(target, CombatTargetFlags.CombatThreat) ? 0 : 1,
        TargetPriorityProfile.Scout => Has(target, CombatTargetFlags.CombatThreat) ? 0 : 1,
        TargetPriorityProfile.Control => target.Class == CombatTargetClass.LightMachine ? 0 : target.Class == CombatTargetClass.MediumMachine ? 1 : 2,
        _ => 0
    };

    private static bool Has(Targetable target, CombatTargetFlags flags) => (target.Flags & flags) != 0;
}
}
