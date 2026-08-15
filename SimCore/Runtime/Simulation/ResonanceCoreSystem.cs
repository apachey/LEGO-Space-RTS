using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class ResonanceCoreSystem : ISimSystem
{
    public const byte BaselineSlots = 4;
    public const byte ExpandedSlots = 6;
    public const ushort CommitTicks = 8 * EnergyDomainSystem.TicksPerSecond;
    public const ushort WithdrawTicks = 15 * EnergyDomainSystem.TicksPerSecond;
    public const int BaseEnergyDemandPerSecond = 3;
    public const int EnergyDemandPerInstalledCrystal = 2;

    private static readonly ContentId ResonanceCoreType = StableId.FromKey("building.ali.resonance_core");
    private static readonly ContentId CommandCoreType = StableId.FromKey("building.ali.etx_command_core");

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResonanceCore.Has(id) || !IsValidCore(world, id)) continue;
            ref ResonanceCore core = ref world.Entities.ResonanceCore.Get(id);
            byte maximum = MaximumSlots(core);
            if (core.DesiredCommittedCrystals > maximum) core.DesiredCommittedCrystals = maximum;

            if (core.TransitionKind == ResonanceTransitionKind.None)
            {
                if (!TryStartNextTransition(world, id, ref core)) continue;
            }

            if (core.TransitionRemainingTicks > 0) core.TransitionRemainingTicks--;
            if (core.TransitionRemainingTicks > 0) continue;
            CompleteTransition(world, id, ref core);
        }
    }

    public static bool TryAttachCore(SimulationWorld world, EntityId coreEntity, EntityId commandCore, bool expandedLatticeUnlocked = false)
    {
        if (world.Entities.ResonanceCore.Has(coreEntity) || !HasCompletedBuildingType(world, coreEntity, ResonanceCoreType) ||
            !HasCompletedBuildingType(world, commandCore, CommandCoreType) || !SameOwnerAndDomain(world, coreEntity, commandCore)) return false;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.ResonanceCore.TryGet(alive[i], out ResonanceCore existing) && existing.CommandCore == commandCore) return false;
        world.Entities.ResonanceCore.Set(coreEntity, new ResonanceCore
        {
            CommandCore = commandCore,
            ExpandedLatticeUnlocked = expandedLatticeUnlocked
        });
        if (world.Entities.EnergyDomainMember.TryGet(coreEntity, out EnergyDomainMember member)) EnergyDomainSystem.Recalculate(world, member.DomainRoot);
        return true;
    }

    public static bool TrySetDesiredCommitment(SimulationWorld world, byte playerSlot, EntityId coreEntity, byte desiredCommittedCrystals)
    {
        if (!IsValidCore(world, coreEntity) || !world.Entities.Ownership.TryGet(coreEntity, out Ownership owner) || owner.PlayerSlot != playerSlot) return false;
        ref ResonanceCore core = ref world.Entities.ResonanceCore.Get(coreEntity);
        if (desiredCommittedCrystals > MaximumSlots(core)) return false;
        int projected = CountCommitted(core) + (core.TransitionKind == ResonanceTransitionKind.Commit ? 1 : 0);
        if (desiredCommittedCrystals > projected && CountSpendableCrystals(world, playerSlot) < desiredCommittedCrystals - projected) return false;
        core.DesiredCommittedCrystals = desiredCommittedCrystals;
        return true;
    }

    public static byte MaximumSlots(ResonanceCore core) => core.ExpandedLatticeUnlocked ? ExpandedSlots : BaselineSlots;

    public static byte CountCommitted(ResonanceCore core)
    {
        int bits = core.CommittedSlotMask & 0x3f;
        byte count = 0;
        while (bits != 0) { count += (byte)(bits & 1); bits >>= 1; }
        return count;
    }

    public static byte CountInstalled(ResonanceCore core)
        => (byte)(CountCommitted(core) + (core.TransitionKind == ResonanceTransitionKind.Withdraw ? 1 : 0));

    public static int ContinuousEnergyDemand(ResonanceCore core)
        => BaseEnergyDemandPerSecond + CountInstalled(core) * EnergyDemandPerInstalledCrystal;

    public static bool IsValidCore(SimulationWorld world, EntityId coreEntity)
    {
        if (!world.Entities.ResonanceCore.TryGet(coreEntity, out ResonanceCore core) || !HasCompletedBuildingType(world, coreEntity, ResonanceCoreType) ||
            !HasCompletedBuildingType(world, core.CommandCore, CommandCoreType) || !SameOwnerAndDomain(world, coreEntity, core.CommandCore)) return false;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId other = alive[i];
            if (other != coreEntity && world.Entities.ResonanceCore.TryGet(other, out ResonanceCore existing) && existing.CommandCore == core.CommandCore) return false;
        }
        return true;
    }

    private static bool TryStartNextTransition(SimulationWorld world, EntityId coreEntity, ref ResonanceCore core)
    {
        byte committed = CountCommitted(core);
        if (core.DesiredCommittedCrystals == committed) return false;
        if (!world.Entities.Ownership.TryGet(coreEntity, out Ownership owner)) return false;

        if (core.DesiredCommittedCrystals > committed)
        {
            if (!TryFindCrystalBank(world, owner.PlayerSlot, requireCrystal: true, out EntityId bank)) return false;
            byte slot = FindFirstFreeSlot(core.CommittedSlotMask, MaximumSlots(core));
            if (slot == byte.MaxValue) return false;
            world.Entities.ResourceBank.Get(bank).ProcessedAmount--;
            core.TransitionBank = bank;
            core.TransitionSlot = slot;
            core.TransitionKind = ResonanceTransitionKind.Commit;
            core.TransitionTotalTicks = CommitTicks;
            core.TransitionRemainingTicks = CommitTicks;
            return true;
        }

        if (!TryFindCrystalBank(world, owner.PlayerSlot, requireCrystal: false, out EntityId withdrawalBank)) return false;
        byte withdrawalSlot = FindLastCommittedSlot(core.CommittedSlotMask);
        if (withdrawalSlot == byte.MaxValue) return false;
        core.CommittedSlotMask = (byte)(core.CommittedSlotMask & ~(1 << withdrawalSlot));
        AlienChargeSystem.RecalculatePlayer(world, owner.PlayerSlot);
        core.TransitionBank = withdrawalBank;
        core.TransitionSlot = withdrawalSlot;
        core.TransitionKind = ResonanceTransitionKind.Withdraw;
        core.TransitionTotalTicks = WithdrawTicks;
        core.TransitionRemainingTicks = WithdrawTicks;
        return true;
    }

    private static void CompleteTransition(SimulationWorld world, EntityId coreEntity, ref ResonanceCore core)
    {
        ResonanceTransitionKind completed = core.TransitionKind;
        if (completed == ResonanceTransitionKind.Commit)
            core.CommittedSlotMask = (byte)(core.CommittedSlotMask | (1 << core.TransitionSlot));
        else if (completed == ResonanceTransitionKind.Withdraw)
        {
            if (!world.Entities.ResourceBank.TryGet(core.TransitionBank, out ResourceBank bank) || bank.Type != ResourceType.Crystal ||
                !world.Entities.Ownership.TryGet(core.TransitionBank, out Ownership bankOwner) || !world.Entities.Ownership.TryGet(coreEntity, out Ownership coreOwner) || bankOwner.PlayerSlot != coreOwner.PlayerSlot)
                throw new InvalidOperationException("Resonance withdrawal lost its authoritative Crystal bank.");
            world.Entities.ResourceBank.Get(core.TransitionBank).ProcessedAmount++;
        }
        core.TransitionBank = EntityId.None;
        core.TransitionSlot = 0;
        core.TransitionKind = ResonanceTransitionKind.None;
        core.TransitionTotalTicks = 0;
        core.TransitionRemainingTicks = 0;
        if (world.Entities.EnergyDomainMember.TryGet(coreEntity, out EnergyDomainMember member)) EnergyDomainSystem.Recalculate(world, member.DomainRoot);
    }

    private static bool HasCompletedBuildingType(SimulationWorld world, EntityId entity, ContentId type)
        => world.Entities.Building.TryGet(entity, out Building building) && building.Type == type && building.State == BuildingState.Completed &&
           world.Entities.Selectable.TryGet(entity, out Selectable selectable) && selectable.ContentType == type;

    private static bool SameOwnerAndDomain(SimulationWorld world, EntityId a, EntityId b)
        => world.Entities.Ownership.TryGet(a, out Ownership aOwner) && world.Entities.Ownership.TryGet(b, out Ownership bOwner) && aOwner.PlayerSlot == bOwner.PlayerSlot &&
           world.Entities.EnergyDomainMember.TryGet(a, out EnergyDomainMember aDomain) && world.Entities.EnergyDomainMember.TryGet(b, out EnergyDomainMember bDomain) && aDomain.DomainRoot == bDomain.DomainRoot;

    private static int CountSpendableCrystals(SimulationWorld world, byte playerSlot)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Ownership.TryGet(alive[i], out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(alive[i], out ResourceBank bank) && bank.Type == ResourceType.Crystal)
                total = checked(total + bank.ProcessedAmount);
        return total;
    }

    private static bool TryFindCrystalBank(SimulationWorld world, byte playerSlot, bool requireCrystal, out EntityId bankEntity)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == ResourceType.Crystal && (!requireCrystal || bank.ProcessedAmount > 0))
            { bankEntity = id; return true; }
        }
        bankEntity = EntityId.None;
        return false;
    }

    private static byte FindFirstFreeSlot(byte mask, byte maximum)
    {
        for (byte slot = 0; slot < maximum; slot++) if ((mask & (1 << slot)) == 0) return slot;
        return byte.MaxValue;
    }

    private static byte FindLastCommittedSlot(byte mask)
    {
        for (int slot = ExpandedSlots - 1; slot >= 0; slot--) if ((mask & (1 << slot)) != 0) return (byte)slot;
        return byte.MaxValue;
    }
}
}
