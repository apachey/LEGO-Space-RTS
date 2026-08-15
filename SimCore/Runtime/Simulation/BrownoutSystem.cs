using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public static class BrownoutSystem
{
    private readonly struct Consumer
    {
        public readonly EntityId Entity;
        public readonly EnergyFunctionalClass FunctionalClass;
        public readonly int Demand;

        public Consumer(EntityId entity, EnergyFunctionalClass functionalClass, int demand)
        { Entity = entity; FunctionalClass = functionalClass; Demand = demand; }
    }

    public static bool IsOperational(SimulationWorld world, EntityId entity)
        => !world.Entities.PowerState.TryGet(entity, out PowerState state) || state.IsPowered;

    public static void Recalculate(SimulationWorld world, EntityId root)
    {
        if (!world.Entities.EnergyDomain.Has(root)) return;
        List<Consumer> consumers = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member) || member.DomainRoot != root ||
                !world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed) continue;
            int demand = 0;
            EnergyFunctionalClass functionalClass = EnergyFunctionalClass.StaticDefenseAndNonessential;
            if (world.Content.TryGetBuilding(building.Type, out BuildingDefinition definition))
            {
                demand = definition.ContinuousEnergyDemandPerSecond;
                functionalClass = definition.EnergyFunctionalClass;
            }
            if (world.Entities.ResonanceCore.TryGet(id, out ResonanceCore core) && ResonanceCoreSystem.IsValidCore(world, id))
            {
                demand = ResonanceCoreSystem.ContinuousEnergyDemand(core);
                functionalClass = EnergyFunctionalClass.ServiceAndFactionSystems;
            }
            if (demand == 0) continue;
            if (!world.Entities.PowerState.Has(id)) world.Entities.PowerState.Set(id, new PowerState { Priority = EnergyPriority.Normal, IsPowered = true });
            consumers.Add(new Consumer(id, functionalClass, demand));
        }

        consumers.Sort((a, b) =>
        {
            int comparison = a.FunctionalClass.CompareTo(b.FunctionalClass);
            if (comparison != 0) return comparison;
            comparison = world.Entities.PowerState.Get(a.Entity).Priority.CompareTo(world.Entities.PowerState.Get(b.Entity).Priority);
            return comparison != 0 ? comparison : a.Entity.Value.CompareTo(b.Entity.Value);
        });

        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        bool previousBrownout = domain.IsBrownout;
        bool brownout = domain.Reserve == Fix32.Zero && domain.ContinuousDemandPerSecond > domain.GenerationPerSecond;
        int available = brownout ? domain.GenerationPerSecond : int.MaxValue;
        int poweredDemand = 0;
        bool powerSetChanged = false;
        for (int i = 0; i < consumers.Count; i++)
        {
            Consumer consumer = consumers[i];
            bool shouldPower = !brownout || poweredDemand + consumer.Demand <= available;
            ref PowerState state = ref world.Entities.PowerState.Get(consumer.Entity);
            if (state.IsPowered != shouldPower) { state.IsPowered = shouldPower; powerSetChanged = true; }
            if (shouldPower) poweredDemand = checked(poweredDemand + consumer.Demand);
        }

        domain.PoweredDemandPerSecond = poweredDemand;
        domain.IsBrownout = brownout;
        if (previousBrownout != brownout || powerSetChanged)
        {
            domain.BrownoutRevision++;
            domain.LastBrownoutEvent = !previousBrownout && brownout ? BrownoutEventKind.Entered
                : previousBrownout && !brownout ? BrownoutEventKind.Recovered : BrownoutEventKind.Changed;
        }
        if (world.Entities.Ownership.TryGet(root, out Ownership owner)) AlienChargeSystem.RecalculatePlayer(world, owner.PlayerSlot);
    }

    public static bool TrySetPriority(SimulationWorld world, byte playerSlot, EntityId[] entities, EnergyPriority priority)
    {
        if (priority < EnergyPriority.High || priority > EnergyPriority.Low) return false;
        List<EntityId> roots = new();
        bool changed = false;
        for (int i = 0; i < entities.Length; i++)
        {
            EntityId id = entities[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.PowerState.Has(id) || !world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member)) continue;
            ref PowerState state = ref world.Entities.PowerState.Get(id);
            if (state.Priority == priority) continue;
            state.Priority = priority; changed = true;
            if (!roots.Contains(member.DomainRoot)) roots.Add(member.DomainRoot);
        }
        roots.Sort(EntityIdComparer.Instance);
        for (int i = 0; i < roots.Count; i++) Recalculate(world, roots[i]);
        return changed;
    }
}
}
