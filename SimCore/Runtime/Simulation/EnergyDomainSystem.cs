using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class EnergyDomainSystem : ISimSystem
{
    public const int TicksPerSecond = 20;
    public const int CanonicalStartingReserve = 120;
    private static readonly ContentId HqType = StableId.FromKey("building.rock_raiders.hq");

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId root = alive[i];
            if (!world.Entities.EnergyDomain.Has(root)) continue;
            ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
            bool wasBrownout = domain.IsBrownout;
            int demand = domain.IsBrownout ? domain.PoweredDemandPerSecond : domain.ContinuousDemandPerSecond;
            int netPerSecond = checked(domain.GenerationPerSecond - demand);
            long numerator = checked((long)netPerSecond * Fix32.OneRaw + domain.FlowRemainderRaw);
            int deltaRaw = checked((int)(numerator / TicksPerSecond));
            int remainder = checked((int)(numerator % TicksPerSecond));
            long nextRaw = checked((long)domain.Reserve.Raw + deltaRaw);
            if (nextRaw <= 0)
            {
                bool crossedZero = domain.Reserve > Fix32.Zero;
                domain.Reserve = Fix32.Zero;
                domain.FlowRemainderRaw = 0;
                if (crossedZero) BrownoutSystem.Recalculate(world, root);
            }
            else if (nextRaw >= domain.ReserveCapacity.Raw)
            {
                domain.Reserve = domain.ReserveCapacity;
                domain.FlowRemainderRaw = 0;
                if (wasBrownout && domain.Reserve > Fix32.Zero) BrownoutSystem.Recalculate(world, root);
            }
            else
            {
                domain.Reserve = Fix32.FromRaw(checked((int)nextRaw));
                domain.FlowRemainderRaw = remainder;
                if (wasBrownout) BrownoutSystem.Recalculate(world, root);
            }
        }
    }

    public static void InitializeOpeningDomains(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed || building.Type != HqType) continue;
            world.Entities.EnergyDomain.Set(id, new EnergyDomain { Reserve = Fix32.FromInt(CanonicalStartingReserve) });
            world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = id });
        }
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed || world.Entities.EnergyDomainMember.Has(id)) continue;
            if (world.Entities.Ownership.TryGet(id, out Ownership ownership) && TryFindOwnedDomain(world, ownership.PlayerSlot, out EntityId root))
                world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = root });
        }
        RecalculateAll(world);
    }

    public static bool TryResolveForEntity(SimulationWorld world, EntityId entity, byte playerSlot, out EntityId root)
    {
        if (world.Entities.EnergyDomainMember.TryGet(entity, out EnergyDomainMember member) &&
            world.Entities.EnergyDomain.Has(member.DomainRoot) && IsOwnedBy(world, member.DomainRoot, playerSlot))
        {
            root = member.DomainRoot;
            return true;
        }
        if (!TryFindOwnedDomain(world, playerSlot, out root)) return false;
        world.Entities.EnergyDomainMember.Set(entity, new EnergyDomainMember { DomainRoot = root });
        Recalculate(world, root);
        return true;
    }

    public static bool TryGetPlayerDomain(SimulationWorld world, byte playerSlot, out EntityId root)
        => TryFindOwnedDomain(world, playerSlot, out root);

    public static bool CanSpendForEntity(SimulationWorld world, EntityId entity, byte playerSlot, int amount)
        => world.Entities.EnergyDomainMember.TryGet(entity, out EnergyDomainMember member) && IsOwnedBy(world, member.DomainRoot, playerSlot) && CanSpend(world, member.DomainRoot, amount);

    public static bool CanSpend(SimulationWorld world, EntityId root, int amount)
        => amount >= 0 && world.Entities.EnergyDomain.TryGet(root, out EnergyDomain domain) && domain.Reserve >= Fix32.FromInt(amount);

    public static bool TrySpend(SimulationWorld world, EntityId root, int amount)
    {
        if (!CanSpend(world, root, amount)) return false;
        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        domain.Reserve -= Fix32.FromInt(amount);
        if (domain.Reserve == Fix32.Zero) BrownoutSystem.Recalculate(world, root);
        return true;
    }

    public static void Refund(SimulationWorld world, EntityId root, int amount)
    {
        if (amount < 0 || !world.Entities.EnergyDomain.Has(root)) throw new System.ArgumentOutOfRangeException(nameof(amount));
        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        domain.Reserve = Fix32.Min(domain.ReserveCapacity, domain.Reserve + Fix32.FromInt(amount));
        BrownoutSystem.Recalculate(world, root);
    }

    public static void RecalculateAll(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.EnergyDomain.Has(alive[i])) Recalculate(world, alive[i]);
    }

    public static void Recalculate(SimulationWorld world, EntityId root)
    {
        if (!world.Entities.EnergyDomain.Has(root)) return;
        int generation = 0, demand = 0, capacity = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member) || member.DomainRoot != root ||
                !world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed ||
                !world.Content.TryGetBuilding(building.Type, out BuildingDefinition definition)) continue;
            generation = checked(generation + definition.EnergyGenerationPerSecond);
            demand = checked(demand + definition.ContinuousEnergyDemandPerSecond);
            capacity = checked(capacity + definition.EnergyReserveCapacity);
        }
        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        domain.GenerationPerSecond = generation;
        domain.ContinuousDemandPerSecond = demand;
        domain.ReserveCapacity = Fix32.FromInt(capacity);
        if (domain.Reserve > domain.ReserveCapacity) domain.Reserve = domain.ReserveCapacity;
        if (domain.Reserve < Fix32.Zero) domain.Reserve = Fix32.Zero;
        BrownoutSystem.Recalculate(world, root);
    }

    public static void DebugDrainPlayerDomains(SimulationWorld world, byte playerSlot)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId root = alive[i];
            if (!world.Entities.EnergyDomain.Has(root) || !IsOwnedBy(world, root, playerSlot)) continue;
            ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
            domain.Reserve = Fix32.Zero; domain.FlowRemainderRaw = 0;
            BrownoutSystem.Recalculate(world, root);
        }
    }

    private static bool TryFindOwnedDomain(SimulationWorld world, byte playerSlot, out EntityId root)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.EnergyDomain.Has(id) && IsOwnedBy(world, id, playerSlot)) { root = id; return true; }
        }
        root = EntityId.None;
        return false;
    }

    private static bool IsOwnedBy(SimulationWorld world, EntityId id, byte playerSlot)
        => world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == playerSlot;
}
}
