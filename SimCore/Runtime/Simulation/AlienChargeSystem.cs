using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public struct AlienChargeState
{
    public int CurrentMillicharge;
    public int MaximumMillicharge;
    public int GenerationMillichargePerSecond;
    public ushort OperationalCoreCount;
    public ushort BrownoutPausedCoreCount;
    public ushort CommittedCrystalCount;
    public bool ResonanceInitiationUnlocked;
}

public sealed class AlienChargeSystem : ISimSystem
{
    public const int MillichargePerCharge = 1000;
    public const int BaseCoreCapacityMillicharge = 20 * MillichargePerCharge;
    public const int CapacityPerCrystalMillicharge = 20 * MillichargePerCharge;
    public const int GenerationPerCrystalMillichargePerSecond = 400;
    public const int SurgeCostMillicharge = 50 * MillichargePerCharge;
    public const ushort SurgeBuildupTicks = 15;
    public const ushort SurgeActiveTicks = 18 * EnergyDomainSystem.TicksPerSecond;
    public const byte ResonanceCoreSurgeRadius = 12;
    public const byte MothershipRelaySurgeRadius = 10;

    private readonly List<EntityId> _activeZones = new();

    public void Step(SimulationWorld world)
    {
        UpdateSurgeZones(world);
        UpdateSurgeReceivers(world);
        for (byte player = 0; player < world.PlayerCount; player++)
        {
            ref AlienChargeState state = ref world.GetAlienChargeRef(player);
            if (state.CurrentMillicharge >= state.MaximumMillicharge || state.GenerationMillichargePerSecond <= 0) continue;
            int perTick = state.GenerationMillichargePerSecond / EnergyDomainSystem.TicksPerSecond;
            state.CurrentMillicharge = Math.Min(state.MaximumMillicharge, checked(state.CurrentMillicharge + perTick));
        }
    }

    public static void RecalculateAll(SimulationWorld world)
    {
        for (byte player = 0; player < world.PlayerCount; player++) RecalculatePlayer(world, player);
    }

    public static void RecalculatePlayer(SimulationWorld world, byte playerSlot)
    {
        if (playerSlot >= world.PlayerCount) return;
        int maximum = 0, generation = 0, committedTotal = 0;
        ushort operational = 0, paused = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot ||
                !world.Entities.ResonanceCore.TryGet(id, out ResonanceCore core) || !ResonanceCoreSystem.IsValidCore(world, id)) continue;
            int committed = ResonanceCoreSystem.CountCommitted(core);
            committedTotal = checked(committedTotal + committed);
            maximum = checked(maximum + BaseCoreCapacityMillicharge + committed * CapacityPerCrystalMillicharge);
            if (BrownoutSystem.IsOperational(world, id))
            {
                operational++;
                generation = checked(generation + committed * GenerationPerCrystalMillichargePerSecond);
            }
            else paused++;
        }

        ref AlienChargeState state = ref world.GetAlienChargeRef(playerSlot);
        state.MaximumMillicharge = maximum;
        state.GenerationMillichargePerSecond = generation;
        state.OperationalCoreCount = operational;
        state.BrownoutPausedCoreCount = paused;
        state.CommittedCrystalCount = checked((ushort)committedTotal);
        if (state.CurrentMillicharge > maximum) state.CurrentMillicharge = maximum;
        if (state.CurrentMillicharge < 0) state.CurrentMillicharge = 0;
    }

    public static void SetResonanceInitiationUnlocked(SimulationWorld world, byte playerSlot, bool unlocked)
    {
        ref AlienChargeState state = ref world.GetAlienChargeRef(playerSlot);
        state.ResonanceInitiationUnlocked = unlocked;
    }

    public static bool TryStartSurge(SimulationWorld world, byte playerSlot, EntityId anchor)
    {
        if (playerSlot >= world.PlayerCount || world.Entities.SurgeZone.Has(anchor) || !world.Entities.Transform.Has(anchor) ||
            !world.Entities.Ownership.TryGet(anchor, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !world.Entities.ResonanceCore.Has(anchor) || !ResonanceCoreSystem.IsValidCore(world, anchor) || !BrownoutSystem.IsOperational(world, anchor)) return false;
        RecalculatePlayer(world, playerSlot);
        ref AlienChargeState state = ref world.GetAlienChargeRef(playerSlot);
        if (!state.ResonanceInitiationUnlocked || state.CurrentMillicharge < SurgeCostMillicharge) return false;
        state.CurrentMillicharge -= SurgeCostMillicharge;
        world.Entities.SurgeZone.Set(anchor, new SurgeZone
        {
            RadiusBuildCells = ResonanceCoreSurgeRadius,
            BuildupRemainingTicks = SurgeBuildupTicks
        });
        return true;
    }

    public static bool IsSurged(SimulationWorld world, EntityId entity)
        => world.Entities.SurgeReceiver.TryGet(entity, out SurgeReceiver receiver) && receiver.ActiveZone != EntityId.None &&
           world.Entities.SurgeZone.TryGet(receiver.ActiveZone, out SurgeZone zone) && zone.BuildupRemainingTicks == 0 && zone.ActiveRemainingTicks > 0;

    public static int ApplySurgedCooldownTicks(int baseTicks)
        => baseTicks <= 0 ? throw new ArgumentOutOfRangeException(nameof(baseTicks)) : checked((baseTicks * 4 + 4) / 5);

    public static int ApplySurgedReconfigurationTicks(int baseTicks)
        => baseTicks <= 0 ? throw new ArgumentOutOfRangeException(nameof(baseTicks)) : checked((baseTicks * 7 + 9) / 10);

    private void UpdateSurgeZones(SimulationWorld world)
    {
        _activeZones.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.SurgeZone.Has(id)) continue;
            ref SurgeZone zone = ref world.Entities.SurgeZone.Get(id);
            if (zone.BuildupRemainingTicks > 0)
            {
                zone.BuildupRemainingTicks--;
                if (zone.BuildupRemainingTicks == 0) zone.ActiveRemainingTicks = SurgeActiveTicks;
            }
            else if (zone.ActiveRemainingTicks > 0)
            {
                zone.ActiveRemainingTicks--;
                if (zone.ActiveRemainingTicks == 0) { world.Entities.SurgeZone.Remove(id); continue; }
            }
            if (zone.BuildupRemainingTicks == 0 && zone.ActiveRemainingTicks > 0) _activeZones.Add(id);
        }
    }

    private void UpdateSurgeReceivers(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId entity = alive[i];
            if (!world.Entities.SurgeReceiver.Has(entity)) continue;
            ref SurgeReceiver receiver = ref world.Entities.SurgeReceiver.Get(entity);
            receiver.ActiveZone = EntityId.None;
            if (!world.Entities.Ownership.TryGet(entity, out Ownership owner) || !world.Entities.Transform.TryGet(entity, out SimTransform transform)) continue;
            for (int z = 0; z < _activeZones.Count; z++)
            {
                EntityId anchor = _activeZones[z];
                if (!world.Entities.Ownership.TryGet(anchor, out Ownership anchorOwner) || anchorOwner.PlayerSlot != owner.PlayerSlot ||
                    !world.Entities.Transform.TryGet(anchor, out SimTransform anchorTransform) || !world.Entities.SurgeZone.TryGet(anchor, out SurgeZone zone)) continue;
                Fix32 radius = Fix32.FromInt(zone.RadiusBuildCells);
                if (FixVec2.DistanceSquared(transform.Position, anchorTransform.Position) > radius * radius) continue;
                if (receiver.ActiveZone == EntityId.None || anchor.Value < receiver.ActiveZone.Value) receiver.ActiveZone = anchor;
            }
        }
    }
}
}
