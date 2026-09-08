using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public enum DefenseNodeMode : byte { GroundPulse = 0, AirLance = 1 }

public struct DefenseNodeState
{
    public DefenseNodeMode CurrentMode;
    public DefenseNodeMode TargetMode;
    public ushort RemainingTicks;
    public int LastHostileCombatTick;
    public bool IsReconfiguring;
    public ushort ShuntRemainingTicks;
}

public sealed class DefenseNodeSystem : ISimSystem
{
    public const ushort ReconfigurationTicks = 120;
    public const int CombatPressureTicks = 80;
    public static readonly ContentId DefenseNodeType = StableId.FromKey("building.ali.etx_defense_node");

    public void Step(SimulationWorld world)
    {
        if (world.DefenseNodeStates.Count == 0) return;
        List<uint> nodes = new(world.DefenseNodeStates.Keys); nodes.Sort();
        for (int i = 0; i < nodes.Count; i++)
        {
            uint key = nodes[i]; DefenseNodeState state = world.DefenseNodeStates[key];
            if (!world.Entities.Exists(new EntityId(key))) { world.DefenseNodeStates.Remove(key); continue; }
            if (state.ShuntRemainingTicks > 0) state.ShuntRemainingTicks--;
            if (!state.IsReconfiguring || IsUnderPressure(world.Tick.Value, state.LastHostileCombatTick)) { world.DefenseNodeStates[key] = state; continue; }
            if (state.RemainingTicks > 0) state.RemainingTicks--;
            if (state.RemainingTicks == 0) { state.CurrentMode = state.TargetMode; state.IsReconfiguring = false; }
            world.DefenseNodeStates[key] = state;
        }
    }

    public static void Register(SimulationWorld world, EntityId node, DefenseNodeMode initialMode)
    {
        world.DefenseNodeStates[node.Value] = new DefenseNodeState
        {
            CurrentMode = initialMode, TargetMode = initialMode, LastHostileCombatTick = int.MinValue
        };
    }

    public static bool TryGetState(SimulationWorld world, EntityId node, out DefenseNodeState state)
        => world.DefenseNodeStates.TryGetValue(node.Value, out state);

    public static bool TryStartReconfiguration(SimulationWorld world, byte playerSlot, EntityId node, DefenseNodeMode targetMode)
    {
        if (!world.DefenseNodeStates.TryGetValue(node.Value, out DefenseNodeState state) || state.IsReconfiguring || state.CurrentMode == targetMode ||
            !world.Entities.Ownership.TryGet(node, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            IsUnderPressure(world.Tick.Value, state.LastHostileCombatTick)) return false;
        state.TargetMode = targetMode; state.RemainingTicks = ReconfigurationTicks; state.IsReconfiguring = true;
        world.DefenseNodeStates[node.Value] = state;
        return true;
    }

    public static void RecordHostileDamage(SimulationWorld world, EntityId source, EntityId target)
    {
        if (!world.Entities.Ownership.TryGet(source, out Ownership sourceOwner) || !world.Entities.Ownership.TryGet(target, out Ownership targetOwner) ||
            sourceOwner.PlayerSlot == targetOwner.PlayerSlot) return;
        Record(world, source); Record(world, target);
    }

    public static bool TryStartShunt(SimulationWorld world, byte playerSlot, EntityId node)
    {
        if (!world.DefenseNodeStates.TryGetValue(node.Value, out DefenseNodeState state) || state.ShuntRemainingTicks > 0 ||
            !world.Entities.Ownership.TryGet(node, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !ResearchSystem.HasCompleted(world, playerSlot, StableId.FromKey("research.ali.defense_resonance_shunt"))) return false;
        ref AlienChargeState charge = ref world.GetAlienChargeRef(playerSlot);
        const int cost = 25 * AlienChargeSystem.MillichargePerCharge;
        if (charge.CurrentMillicharge < cost) return false;
        charge.CurrentMillicharge -= cost; state.ShuntRemainingTicks = 12 * EnergyDomainSystem.TicksPerSecond;
        world.DefenseNodeStates[node.Value] = state; return true;
    }

    public static bool IsUnderPressure(int currentTick, int lastHostileTick)
        => lastHostileTick != int.MinValue && currentTick - lastHostileTick < CombatPressureTicks;

    private static void Record(SimulationWorld world, EntityId entity)
    {
        if (!world.DefenseNodeStates.TryGetValue(entity.Value, out DefenseNodeState state)) return;
        state.LastHostileCombatTick = world.Tick.Value; world.DefenseNodeStates[entity.Value] = state;
    }
}
}
