using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct ResourceConservationTotals
{
    public readonly long Raw;
    public readonly long Carried;
    public readonly long Hauled;
    public readonly long Processed;
    public readonly long Reserved;
    public readonly long Consumed;

    public ResourceConservationTotals(long raw, long carried, long hauled, long processed, long reserved = 0, long consumed = 0)
    {
        Raw = raw;
        Carried = carried;
        Hauled = hauled;
        Processed = processed;
        Reserved = reserved;
        Consumed = consumed;
    }

    public long Total => checked(checked(checked(checked(Raw + Carried) + checked(Hauled + Processed)) + Reserved) + Consumed);
}

public static class ResourceConservation
{
    public static ResourceConservationTotals Measure(SimulationWorld world, ResourceType type)
    {
        long raw = 0, carried = 0, hauled = 0, processed = 0, reserved = 0, consumed = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.ResourceNode.TryGet(id, out ResourceNode node) && node.Type == type)
                raw = checked(raw + node.Remaining);
            if (world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) && carrier.Type == type)
                carried = checked(carried + carrier.Amount);
            if (world.Entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver) && receiver.AcceptedType == type)
                hauled = checked(hauled + receiver.PendingHauledAmount);
            if (world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == type)
                processed = checked(processed + bank.ProcessedAmount);
            if (type == ResourceType.Ore && world.Entities.ConstructionSite.TryGet(id, out ConstructionSite site))
            {
                reserved = checked(reserved + site.ReservedOre);
                consumed = checked(consumed + site.ConsumedOre);
            }
        }
        return new ResourceConservationTotals(raw, carried, hauled, processed, reserved, consumed);
    }
}
}
