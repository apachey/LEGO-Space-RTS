using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic Rock Raider Crew field repair for M4 T046.</summary>
public sealed class RepairSystem : ISimSystem
{
    public static readonly Fix32 InteractionRange = Fix32.FromRatio(3, 2);
    public static readonly Fix32 CrewFieldRatePerSecond = Fix32.FromInt(7);
    private static readonly Fix32 UnderFireMultiplier = Fix32.FromRatio(60, 100);
    private static readonly Fix32 FieldCostMultiplier = Fix32.FromRatio(135, 100);

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId repairer = alive[i];
            if (!world.Entities.Builder.TryGet(repairer, out Builder builder) ||
                (builder.JobState != BuilderJobState.MovingToRepair && builder.JobState != BuilderJobState.Repairing)) continue;
            if (!IsEligible(world, repairer, builder.RepairTarget)) { Finish(world, repairer); continue; }

            ref Health targetHealth = ref world.Entities.Health.Get(builder.RepairTarget);
            if (targetHealth.Current >= targetHealth.Maximum) { Finish(world, repairer); continue; }
            Fix32 gap = CombatGeometry.ContactGap(world, repairer, builder.RepairTarget);
            if (gap > InteractionRange)
            {
                if (builder.JobState != BuilderJobState.MovingToRepair ||
                    !world.Entities.Navigation.TryGet(repairer, out NavigationAgent moving) || !moving.HasTarget)
                    SetApproach(world, repairer, builder.RepairTarget);
                continue;
            }

            ref Builder active = ref world.Entities.Builder.Get(repairer);
            if (active.JobState != BuilderJobState.Repairing)
            {
                ref NavigationAgent nav = ref world.Entities.Navigation.Get(repairer);
                ref Movement movement = ref world.Entities.Movement.Get(repairer);
                CommandExecutionSystem.StopMovement(world, repairer, ref nav, ref movement);
                active.JobState = BuilderJobState.Repairing;
            }
            if (world.Entities.Health.TryGet(repairer, out Health repairerHealth) &&
                repairerHealth.LastDamageTick >= 0 && world.Tick.Value - repairerHealth.LastDamageTick <= SimClock.TicksPerSecond) continue;

            Fix32 heal = CrewFieldRatePerSecond * SimClock.TickSeconds;
            int rank = RepairRank(world, repairer, builder.RepairTarget);
            if (rank >= 3) continue;
            if (rank == 1) heal *= Fix32.FromRatio(70, 100);
            else if (rank == 2) heal *= Fix32.FromRatio(40, 100);
            if (targetHealth.LastDamageTick >= 0 && world.Tick.Value - targetHealth.LastDamageTick <= 2 * SimClock.TicksPerSecond)
                heal *= UnderFireMultiplier;
            heal = Fix32.Min(heal, targetHealth.Maximum - targetHealth.Current);
            if (heal <= Fix32.Zero || !TryPay(world, repairer, builder.RepairTarget, heal, targetHealth.Maximum, ref active)) continue;
            targetHealth.Current += heal;
            if (targetHealth.Current >= targetHealth.Maximum) Finish(world, repairer);
        }
    }

    public static bool TryStart(SimulationWorld world, EntityId repairer, EntityId target, bool clearQueue)
    {
        if (!IsEligible(world, repairer, target)) return false;
        if (clearQueue) world.GetQueue(repairer).Clear();
        ConstructionSystem.ReleaseBuilderAssignment(world, repairer);
        CommandExecutionSystem.CancelHarvest(world, repairer);
        TargetingSystem.ClearTarget(world, repairer);
        ref Builder builder = ref world.Entities.Builder.Get(repairer);
        builder.RepairTarget = target;
        builder.RepairOreRemainder = Fix32.Zero;
        SetApproach(world, repairer, target);
        return true;
    }

    private static bool IsEligible(SimulationWorld world, EntityId repairer, EntityId target)
        => target != EntityId.None && repairer != target && world.Entities.Builder.Has(repairer) &&
           world.Entities.Navigation.Has(repairer) && world.Entities.Movement.Has(repairer) &&
           world.Entities.Health.TryGet(target, out Health health) && health.Current > Fix32.Zero && health.Current < health.Maximum &&
           world.Entities.Transform.Has(target) && world.Entities.Selectable.Has(target) &&
           world.Entities.Ownership.TryGet(repairer, out Ownership a) && world.Entities.Ownership.TryGet(target, out Ownership b) &&
           a.PlayerSlot == b.PlayerSlot;

    private static void SetApproach(SimulationWorld world, EntityId repairer, EntityId target)
    {
        FixVec2 source = world.Entities.Transform.Get(repairer).Position;
        FixVec2 destination = world.Entities.Transform.Get(target).Position;
        FixVec2 direction = (source - destination).NormalizeSafe();
        if (direction.Equals(FixVec2.Zero)) direction = new FixVec2(-Fix32.One, Fix32.Zero);
        FixVec2 desired = CombatGeometry.RangedApproachPoint(world, repairer, target, direction, InteractionRange * Fix32.FromRatio(3, 4));
        NavigationAgent nav = world.Entities.Navigation.Get(repairer);
        desired = FormationPlanner.ResolvePassableSlot(world, desired, nav.Footprint);
        CommandExecutionSystem.SetMove(world, repairer, desired);
        ref Builder builder = ref world.Entities.Builder.Get(repairer);
        builder.JobState = BuilderJobState.MovingToRepair;
    }

    private static bool TryPay(SimulationWorld world, EntityId repairer, EntityId target, Fix32 heal, Fix32 maximum, ref Builder job)
    {
        Selectable selectable = world.Entities.Selectable.Get(target);
        int oreBase, energyBase;
        Fix32 oreRatio, energyRatio;
        if (world.Content.TryGetBuilding(selectable.ContentType, out BuildingDefinition building))
        { oreBase = building.OreCost; energyBase = building.EnergyCost; oreRatio = Fix32.FromRatio(30, 100); energyRatio = Fix32.FromRatio(15, 100); }
        else if (world.Content.TryGetProduction(selectable.ContentType, out UnitProductionDefinition unit))
        { oreBase = unit.OreCost; energyBase = unit.EnergyCost; oreRatio = Fix32.FromRatio(28, 100); energyRatio = Fix32.FromRatio(10, 100); }
        else if (selectable.ContentType == StableId.FromKey(DebugPlaytestScenario.ChromeCrusherKey))
        { oreBase = 330; energyBase = 90; oreRatio = Fix32.FromRatio(28, 100); energyRatio = Fix32.FromRatio(10, 100); }
        else return false;

        Fix32 oreCost = heal * Fix32.FromInt(oreBase) * oreRatio * FieldCostMultiplier / maximum;
        Fix32 energyCost = heal * Fix32.FromInt(energyBase) * energyRatio * FieldCostMultiplier / maximum;
        Fix32 accrued = job.RepairOreRemainder + oreCost;
        int wholeOre = accrued.FloorToInt();
        EntityId bankId = FindBank(world, world.Entities.Ownership.Get(repairer).PlayerSlot);
        if (bankId == EntityId.None || (oreCost > Fix32.Zero && world.Entities.ResourceBank.Get(bankId).ProcessedAmount <= 0) ||
            world.Entities.ResourceBank.Get(bankId).ProcessedAmount < wholeOre) return false;
        EntityId root = EntityId.None;
        if (energyCost > Fix32.Zero && (!EnergyDomainSystem.TryGetPlayerDomain(world, world.Entities.Ownership.Get(repairer).PlayerSlot, out root) ||
            world.Entities.EnergyDomain.Get(root).Reserve < energyCost)) return false;
        if (wholeOre > 0) world.Entities.ResourceBank.Get(bankId).ProcessedAmount -= wholeOre;
        job.RepairOreRemainder = accrued - Fix32.FromInt(wholeOre);
        if (energyCost > Fix32.Zero) world.Entities.EnergyDomain.Get(root).Reserve -= energyCost;
        return true;
    }

    private static EntityId FindBank(SimulationWorld world, byte owner)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == ResourceType.Ore &&
                world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == owner) return id;
        }
        return EntityId.None;
    }

    private static int RepairRank(SimulationWorld world, EntityId repairer, EntityId target)
    {
        int rank = 0; IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId candidate = alive[i];
            if (candidate == repairer) break;
            if (!world.Entities.Builder.TryGet(candidate, out Builder builder) || builder.RepairTarget != target ||
                builder.JobState != BuilderJobState.Repairing || CombatGeometry.ContactGap(world, candidate, target) > InteractionRange) continue;
            rank++;
        }
        return rank;
    }

    private static void Finish(SimulationWorld world, EntityId repairer)
    {
        ConstructionSystem.ReleaseBuilderAssignment(world, repairer);
        if (!CommandExecutionSystem.TryStartNextOrder(world, repairer) && world.Entities.Navigation.Has(repairer) && world.Entities.Movement.Has(repairer))
        {
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(repairer);
            ref Movement movement = ref world.Entities.Movement.Get(repairer);
            CommandExecutionSystem.StopMovement(world, repairer, ref nav, ref movement);
        }
    }
}
}
