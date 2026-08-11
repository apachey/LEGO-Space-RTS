using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class ConstructionSystem : ISimSystem
{
    public static readonly Fix32 InteractionRange = Fix32.FromRatio(5, 4);
    private static readonly ContentId HqType = StableId.FromKey("building.rock_raiders.hq");

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId builderId = alive[i];
            if (!world.Entities.Builder.TryGet(builderId, out Builder builder) || builder.JobState == BuilderJobState.Idle) continue;
            if (builder.JobState == BuilderJobState.MovingToRepair || builder.JobState == BuilderJobState.Repairing) continue;
            if (!world.Entities.ConstructionSite.Has(builder.ConstructionTarget) ||
                !world.Entities.Building.TryGet(builder.ConstructionTarget, out Building building) ||
                !world.Entities.Transform.TryGet(builderId, out SimTransform builderTransform))
            {
                FinishBuilderAssignment(world, builderId);
                continue;
            }

            if (!IsWithinRange(building, builderTransform.Position))
            {
                if (builder.JobState != BuilderJobState.MovingToSite ||
                    !world.Entities.Navigation.TryGet(builderId, out NavigationAgent nav) || !nav.HasTarget)
                    StartBuilder(world, builderId, builder.ConstructionTarget);
                continue;
            }

            ref Builder activeBuilder = ref world.Entities.Builder.Get(builderId);
            if (activeBuilder.JobState != BuilderJobState.Constructing)
            {
                ref NavigationAgent nav = ref world.Entities.Navigation.Get(builderId);
                ref Movement move = ref world.Entities.Movement.Get(builderId);
                CommandExecutionSystem.StopMovement(world, builderId, ref nav, ref move);
                activeBuilder.JobState = BuilderJobState.Constructing;
            }
            AdvanceSite(world, builder.ConstructionTarget);
        }
    }

    public static bool AssignBuilder(SimulationWorld world, EntityId builderId, EntityId siteId, bool queue)
    {
        if (!IsEligibleForSite(world, builderId, siteId)) return false;
        if (queue && CommandExecutionSystem.IsBusy(world, builderId))
        {
            if (!world.GetQueue(builderId).Enqueue(new UnitOrder(UnitOrderType.Construct, FixVec2.Zero, targetEntity: siteId))) return false;
            SetPrimaryBuilder(world, siteId, builderId);
            return true;
        }

        if (!queue)
        {
            ReleaseBuilderAssignment(world, builderId);
            world.GetQueue(builderId).Clear();
        }
        CommandExecutionSystem.CancelHarvest(world, builderId);
        return StartBuilder(world, builderId, siteId);
    }

    public static void ReleaseBuilderAssignment(SimulationWorld world, EntityId builderId)
    {
        if (!world.Entities.Builder.TryGet(builderId, out Builder builder)) return;
        EntityId oldTarget = builder.ConstructionTarget;
        ref Builder stored = ref world.Entities.Builder.Get(builderId);
        stored.ConstructionTarget = EntityId.None;
        stored.RepairTarget = EntityId.None;
        stored.RepairOreRemainder = Fix32.Zero;
        stored.JobState = BuilderJobState.Idle;
        if (oldTarget != EntityId.None) RecomputePrimaryBuilder(world, oldTarget, builderId);

        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId siteId = alive[i];
            if (siteId == oldTarget || !world.Entities.ConstructionSite.TryGet(siteId, out ConstructionSite site) || site.AssignedBuilder != builderId) continue;
            RecomputePrimaryBuilder(world, siteId, builderId);
        }
    }

    public static void ReleaseSiteAssignments(SimulationWorld world, EntityId siteId)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId builderId = alive[i];
            if (!world.Entities.Builder.TryGet(builderId, out Builder builder) || builder.ConstructionTarget != siteId) continue;
            ref Builder stored = ref world.Entities.Builder.Get(builderId);
            stored.ConstructionTarget = EntityId.None;
            stored.JobState = BuilderJobState.Idle;
            StartFollowingWork(world, builderId);
        }
    }

    internal static bool StartBuilder(SimulationWorld world, EntityId builderId, EntityId siteId)
    {
        if (!IsEligibleForSite(world, builderId, siteId) ||
            !world.Entities.Building.TryGet(siteId, out Building building) ||
            !world.Entities.Transform.TryGet(builderId, out SimTransform builderTransform)) return false;
        ref Builder builder = ref world.Entities.Builder.Get(builderId);
        if (builder.ConstructionTarget != EntityId.None && builder.ConstructionTarget != siteId) ReleaseBuilderAssignment(world, builderId);
        builder = ref world.Entities.Builder.Get(builderId);
        builder.ConstructionTarget = siteId;
        SetPrimaryBuilder(world, siteId, builderId);

        if (IsWithinRange(building, builderTransform.Position))
        {
            ref NavigationAgent nearNav = ref world.Entities.Navigation.Get(builderId);
            ref Movement nearMove = ref world.Entities.Movement.Get(builderId);
            CommandExecutionSystem.StopMovement(world, builderId, ref nearNav, ref nearMove);
            builder.JobState = BuilderJobState.Constructing;
            return true;
        }

        builder.JobState = BuilderJobState.MovingToSite;
        NavigationAgent nav = world.Entities.Navigation.Get(builderId);
        FixVec2 approach = FormationPlanner.ResolvePassableSlot(world, ClosestApproach(building, builderTransform.Position), nav.Footprint);
        CommandExecutionSystem.SetMove(world, builderId, approach);
        return true;
    }

    private static void AdvanceSite(SimulationWorld world, EntityId siteId)
    {
        if (!world.Entities.ConstructionSite.Has(siteId)) return;
        ref ConstructionSite site = ref world.Entities.ConstructionSite.Get(siteId);
        if (site.RequiredTicks == 0 || site.ProgressTicks >= site.RequiredTicks) return;
        int totalOre = checked(site.ReservedOre + site.ConsumedOre);
        ushort nextProgress = checked((ushort)(site.ProgressTicks + 1));
        int initialCommit = checked((totalOre + 4) / 5);
        int progressiveOre = totalOre - initialCommit;
        int targetConsumed = checked(initialCommit + (int)((long)progressiveOre * nextProgress / site.RequiredTicks));
        int newlyConsumed = targetConsumed - site.ConsumedOre;
        if (newlyConsumed < 0 || newlyConsumed > site.ReservedOre) throw new System.InvalidOperationException($"Invalid construction commitment on site {siteId.Value}.");
        int totalEnergy = checked(site.ReservedEnergy + site.ConsumedEnergy);
        int initialEnergyCommit = checked((totalEnergy + 4) / 5);
        int progressiveEnergy = totalEnergy - initialEnergyCommit;
        int targetEnergyConsumed = checked(initialEnergyCommit + (int)((long)progressiveEnergy * nextProgress / site.RequiredTicks));
        int newlyConsumedEnergy = targetEnergyConsumed - site.ConsumedEnergy;
        if (newlyConsumedEnergy < 0 || newlyConsumedEnergy > site.ReservedEnergy) throw new System.InvalidOperationException($"Invalid construction Energy commitment on site {siteId.Value}.");
        site.ReservedOre -= newlyConsumed;
        site.ConsumedOre += newlyConsumed;
        site.ReservedEnergy -= newlyConsumedEnergy;
        site.ConsumedEnergy += newlyConsumedEnergy;
        site.ProgressTicks = nextProgress;
        if (site.ProgressTicks < site.RequiredTicks) return;

        ref Building building = ref world.Entities.Building.Get(siteId);
        building.State = BuildingState.Completed;
        world.Entities.ConstructionSite.Remove(siteId);
        if (world.Content.IsProducer(building.Type)) world.Entities.Production.Set(siteId, new Production());
        if (world.Entities.EnergyDomainMember.TryGet(siteId, out EnergyDomainMember member))
        {
            if (building.Type == HqType && member.DomainRoot != siteId)
            {
                world.Entities.EnergyDomainMember.Set(siteId, new EnergyDomainMember { DomainRoot = siteId });
                EnergyDomainSystem.Recalculate(world, member.DomainRoot);
                world.Entities.EnergyDomain.Set(siteId, new EnergyDomain { Reserve = Fix32.Zero });
                EnergyDomainSystem.Recalculate(world, siteId);
            }
            else EnergyDomainSystem.Recalculate(world, member.DomainRoot);
        }
        ReleaseSiteAssignments(world, siteId);
    }

    private static void FinishBuilderAssignment(SimulationWorld world, EntityId builderId)
    {
        ReleaseBuilderAssignment(world, builderId);
        StartFollowingWork(world, builderId);
    }

    private static void StartFollowingWork(SimulationWorld world, EntityId builderId)
    {
        if (CommandExecutionSystem.TryStartNextOrder(world, builderId)) return;
        if (world.Entities.ResourceCarrier.TryGet(builderId, out ResourceCarrier carrier) && carrier.Amount > 0 && CommandExecutionSystem.StartDelivery(world, builderId)) return;
        if (!world.Entities.Navigation.Has(builderId) || !world.Entities.Movement.Has(builderId)) return;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(builderId);
        ref Movement move = ref world.Entities.Movement.Get(builderId);
        CommandExecutionSystem.StopMovement(world, builderId, ref nav, ref move);
    }

    private static bool IsEligibleForSite(SimulationWorld world, EntityId builderId, EntityId siteId)
    {
        return world.Entities.Exists(builderId) && world.Entities.Builder.Has(builderId) && world.Entities.Navigation.Has(builderId) &&
            world.Entities.Movement.Has(builderId) && world.Entities.ConstructionSite.Has(siteId) &&
            world.Entities.Ownership.TryGet(builderId, out Ownership builderOwner) &&
            world.Entities.Ownership.TryGet(siteId, out Ownership siteOwner) && builderOwner.PlayerSlot == siteOwner.PlayerSlot;
    }

    private static void SetPrimaryBuilder(SimulationWorld world, EntityId siteId, EntityId builderId)
    {
        if (!world.Entities.ConstructionSite.Has(siteId)) return;
        ref ConstructionSite site = ref world.Entities.ConstructionSite.Get(siteId);
        if (site.AssignedBuilder == EntityId.None || builderId.Value < site.AssignedBuilder.Value) site.AssignedBuilder = builderId;
    }

    private static void RecomputePrimaryBuilder(SimulationWorld world, EntityId siteId, EntityId excluded)
    {
        if (!world.Entities.ConstructionSite.Has(siteId)) return;
        EntityId best = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId candidate = alive[i];
            if (candidate == excluded || !world.Entities.Builder.TryGet(candidate, out Builder builder) || builder.ConstructionTarget != siteId) continue;
            if (best == EntityId.None || candidate.Value < best.Value) best = candidate;
        }
        world.Entities.ConstructionSite.Get(siteId).AssignedBuilder = best;
    }

    private static bool IsWithinRange(Building building, FixVec2 position)
        => FixVec2.Distance(position, ClosestFootprintPoint(building, position)) <= InteractionRange;

    private static FixVec2 ClosestApproach(Building building, FixVec2 origin)
    {
        FixVec2 edge = ClosestFootprintPoint(building, origin);
        FixVec2 outward = (origin - edge).NormalizeSafe();
        if (outward.Equals(FixVec2.Zero)) outward = new FixVec2(-Fix32.One, Fix32.Zero);
        return edge + outward * Fix32.FromRatio(3, 4);
    }

    private static FixVec2 ClosestFootprintPoint(Building building, FixVec2 position)
    {
        Fix32 minX = Fix32.FromInt(building.AnchorX), maxX = Fix32.FromInt(building.AnchorX + building.FootprintWidth);
        Fix32 minY = Fix32.FromInt(building.AnchorY), maxY = Fix32.FromInt(building.AnchorY + building.FootprintHeight);
        Fix32 x = position.X < minX ? minX : position.X > maxX ? maxX : position.X;
        Fix32 y = position.Y < minY ? minY : position.Y > maxY ? maxY : position.Y;
        return new FixVec2(x, y);
    }
}
}
