using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct OperationsCapacityState
{
    public readonly int Active;
    public readonly int Reserved;
    public readonly int Maximum;

    public OperationsCapacityState(int active, int reserved, int maximum)
    {
        Active = active; Reserved = reserved; Maximum = maximum;
    }

    public int Used => Active + Reserved;
    public bool IsOverCapacity => Active > Maximum || Used > Maximum;
    public bool IsAdvanceWarning => Maximum > 0 && Used * 100 >= Maximum * 85;
}

public sealed class OperationsCapacitySystem : ISimSystem
{
    public const int CompetitiveMaximum = 100;

    public void Step(SimulationWorld world) => Recalculate(world);

    public static void Recalculate(SimulationWorld world)
    {
        world.ClearOperationsCapacity();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot >= world.PlayerCount) continue;
            OperationsCapacityState state = world.GetOperationsCapacity(ownership.PlayerSlot);
            int active = state.Active, reserved = state.Reserved, maximum = state.Maximum;
            if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.Kind != SelectableKind.Building &&
                selectable.Kind != SelectableKind.ResourceNode && world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition unit))
                active = checked(active + unit.OperationsCapacity);
            if (world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed &&
                world.Content.TryGetBuilding(building.Type, out BuildingDefinition buildingDefinition))
                maximum = Math.Min(CompetitiveMaximum, checked(maximum + buildingDefinition.OperationsCapacityProvided));
            if (world.Entities.Production.TryGet(id, out Production production))
                for (int q = 0; q < production.Count; q++)
                    reserved = checked(reserved + production.Get(q).ReservedOperationsCapacity);
            world.SetOperationsCapacity(ownership.PlayerSlot, new OperationsCapacityState(active, reserved, maximum));
        }
    }

    public static bool CanReserve(SimulationWorld world, byte playerSlot, byte productOperationsCapacity)
    {
        if (productOperationsCapacity == 0 || playerSlot >= world.PlayerCount) return false;
        Recalculate(world);
        OperationsCapacityState state = world.GetOperationsCapacity(playerSlot);
        return state.Used + productOperationsCapacity <= state.Maximum;
    }
}
}
