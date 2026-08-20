using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct NetworkOwnedEnergyDomain
{
    public readonly EntityId Root;
    public readonly int ReserveRaw;
    public readonly int ReserveCapacityRaw;
    public readonly int GenerationPerSecond;
    public readonly int ContinuousDemandPerSecond;
    public readonly int PoweredDemandPerSecond;
    public readonly uint BrownoutRevision;
    public readonly bool IsBrownout;

    public NetworkOwnedEnergyDomain(EntityId root, EnergyDomain domain)
    {
        Root = root; ReserveRaw = domain.Reserve.Raw; ReserveCapacityRaw = domain.ReserveCapacity.Raw;
        GenerationPerSecond = domain.GenerationPerSecond; ContinuousDemandPerSecond = domain.ContinuousDemandPerSecond;
        PoweredDemandPerSecond = domain.PoweredDemandPerSecond; BrownoutRevision = domain.BrownoutRevision; IsBrownout = domain.IsBrownout;
    }

    internal NetworkOwnedEnergyDomain(EntityId root, int reserveRaw, int capacityRaw, int generation, int demand, int poweredDemand,
        uint revision, bool isBrownout)
    {
        Root = root; ReserveRaw = reserveRaw; ReserveCapacityRaw = capacityRaw; GenerationPerSecond = generation;
        ContinuousDemandPerSecond = demand; PoweredDemandPerSecond = poweredDemand; BrownoutRevision = revision; IsBrownout = isBrownout;
    }
}

public readonly struct NetworkOwnedWorksite
{
    public readonly EntityId Root;
    public readonly ushort NodeCount;
    public readonly ushort MemberCount;
    public readonly uint TopologyRevision;
    public NetworkOwnedWorksite(EntityId root, WorksiteComponent component)
    { Root = root; NodeCount = component.NodeCount; MemberCount = component.MemberCount; TopologyRevision = component.TopologyRevision; }
}

public readonly struct NetworkOwnedTubeNetwork
{
    public readonly EntityId Root;
    public readonly ushort StationCount;
    public readonly ushort OperationalLinkCount;
    public readonly uint TopologyRevision;
    public NetworkOwnedTubeNetwork(EntityId root, TubeComponent component)
    { Root = root; StationCount = component.StationCount; OperationalLinkCount = component.OperationalLinkCount; TopologyRevision = component.TopologyRevision; }
}

/// <summary>Private production, queued orders and faction-network summaries for one snapshot recipient.</summary>
public sealed class NetworkOwnSystemsState
{
    public NetworkOwnedOrderQueue[] OrderQueues { get; set; } = Array.Empty<NetworkOwnedOrderQueue>();
    public NetworkOwnedProductionQueue[] ProductionQueues { get; set; } = Array.Empty<NetworkOwnedProductionQueue>();
    public NetworkOwnedEnergyDomain[] EnergyDomains { get; set; } = Array.Empty<NetworkOwnedEnergyDomain>();
    public NetworkOwnedWorksite[] Worksites { get; set; } = Array.Empty<NetworkOwnedWorksite>();
    public NetworkOwnedTubeNetwork[] TubeNetworks { get; set; } = Array.Empty<NetworkOwnedTubeNetwork>();

    public static NetworkOwnSystemsState Capture(SimulationWorld world, byte playerSlot)
    {
        List<NetworkOwnedOrderQueue> orders = new(); List<NetworkOwnedProductionQueue> production = new();
        List<NetworkOwnedEnergyDomain> energy = new(); List<NetworkOwnedWorksite> worksites = new(); List<NetworkOwnedTubeNetwork> tubes = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot) continue;
            if (world.TryGetQueue(id, out UnitCommandQueue queue) && queue.Count > 0)
            {
                using MemoryStream stream = new(); using BinaryWriter writer = new(stream); queue.Serialize(writer); writer.Flush();
                orders.Add(new NetworkOwnedOrderQueue(id, stream.ToArray()));
            }
            if (world.Entities.Production.TryGet(id, out Production queueState)) production.Add(new NetworkOwnedProductionQueue(id, queueState));
            if (world.Entities.EnergyDomain.TryGet(id, out EnergyDomain domain)) energy.Add(new NetworkOwnedEnergyDomain(id, domain));
            if (world.Entities.WorksiteComponent.TryGet(id, out WorksiteComponent worksite)) worksites.Add(new NetworkOwnedWorksite(id, worksite));
            if (world.Entities.TubeComponent.TryGet(id, out TubeComponent tube)) tubes.Add(new NetworkOwnedTubeNetwork(id, tube));
        }
        return new NetworkOwnSystemsState
        {
            OrderQueues = orders.ToArray(), ProductionQueues = production.ToArray(), EnergyDomains = energy.ToArray(),
            Worksites = worksites.ToArray(), TubeNetworks = tubes.ToArray()
        };
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new(); using BinaryWriter w = new(stream);
        w.Write(OrderQueues.Length);
        for (int i = 0; i < OrderQueues.Length; i++) { w.Write(OrderQueues[i].EntityId.Value); WriteBytes(w, OrderQueues[i].SerializedOrders); }
        w.Write(ProductionQueues.Length);
        for (int i = 0; i < ProductionQueues.Length; i++) WriteProduction(w, ProductionQueues[i]);
        w.Write(EnergyDomains.Length);
        for (int i = 0; i < EnergyDomains.Length; i++)
        {
            NetworkOwnedEnergyDomain value = EnergyDomains[i]; w.Write(value.Root.Value); w.Write(value.ReserveRaw); w.Write(value.ReserveCapacityRaw);
            w.Write(value.GenerationPerSecond); w.Write(value.ContinuousDemandPerSecond); w.Write(value.PoweredDemandPerSecond); w.Write(value.BrownoutRevision); w.Write(value.IsBrownout);
        }
        w.Write(Worksites.Length);
        for (int i = 0; i < Worksites.Length; i++) { w.Write(Worksites[i].Root.Value); w.Write(Worksites[i].NodeCount); w.Write(Worksites[i].MemberCount); w.Write(Worksites[i].TopologyRevision); }
        w.Write(TubeNetworks.Length);
        for (int i = 0; i < TubeNetworks.Length; i++) { w.Write(TubeNetworks[i].Root.Value); w.Write(TubeNetworks[i].StationCount); w.Write(TubeNetworks[i].OperationalLinkCount); w.Write(TubeNetworks[i].TopologyRevision); }
        w.Flush(); return stream.ToArray();
    }

    public static NetworkOwnSystemsState Deserialize(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        using MemoryStream stream = new(bytes, false); using BinaryReader r = new(stream);
        NetworkOwnSystemsState state = new()
        {
            OrderQueues = ReadOrderQueues(r), ProductionQueues = ReadProductionQueues(r), EnergyDomains = ReadEnergy(r),
            Worksites = ReadWorksites(r), TubeNetworks = ReadTubes(r)
        };
        if (stream.Position != stream.Length) throw new InvalidDataException("Own-systems snapshot has trailing data.");
        return state;
    }

    private static NetworkOwnedOrderQueue[] ReadOrderQueues(BinaryReader r)
    {
        int count = ReadCount(r); NetworkOwnedOrderQueue[] values = new NetworkOwnedOrderQueue[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = ReadOrderedId(r, ref previous); values[i] = new NetworkOwnedOrderQueue(new EntityId(id), ReadBytes(r)); }
        return values;
    }

    private static NetworkOwnedProductionQueue[] ReadProductionQueues(BinaryReader r)
    {
        int count = ReadCount(r); NetworkOwnedProductionQueue[] values = new NetworkOwnedProductionQueue[count]; uint previous = 0;
        for (int i = 0; i < count; i++)
        {
            uint id = ReadOrderedId(r, ref previous); bool blocked = r.ReadBoolean(), hasRally = r.ReadBoolean();
            FixVec2 rally = new(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())); EntityId target = new(r.ReadUInt32());
            int items = ReadCount(r, Production.Capacity); NetworkProductionItem[] queue = new NetworkProductionItem[items];
            for (int q = 0; q < items; q++) queue[q] = new NetworkProductionItem(new ContentId(r.ReadUInt32()), r.ReadUInt16(), r.ReadUInt16(),
                r.ReadUInt16(), r.ReadUInt16(), r.ReadByte(), r.ReadByte());
            values[i] = new NetworkOwnedProductionQueue(new EntityId(id), blocked, hasRally, rally, target, queue);
        }
        return values;
    }

    private static NetworkOwnedEnergyDomain[] ReadEnergy(BinaryReader r)
    {
        int count = ReadCount(r); NetworkOwnedEnergyDomain[] values = new NetworkOwnedEnergyDomain[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = ReadOrderedId(r, ref previous); values[i] = new NetworkOwnedEnergyDomain(new EntityId(id), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadUInt32(), r.ReadBoolean()); }
        return values;
    }

    private static NetworkOwnedWorksite[] ReadWorksites(BinaryReader r)
    {
        int count = ReadCount(r); NetworkOwnedWorksite[] values = new NetworkOwnedWorksite[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = ReadOrderedId(r, ref previous); values[i] = new NetworkOwnedWorksite(new EntityId(id), new WorksiteComponent { NodeCount = r.ReadUInt16(), MemberCount = r.ReadUInt16(), TopologyRevision = r.ReadUInt32() }); }
        return values;
    }

    private static NetworkOwnedTubeNetwork[] ReadTubes(BinaryReader r)
    {
        int count = ReadCount(r); NetworkOwnedTubeNetwork[] values = new NetworkOwnedTubeNetwork[count]; uint previous = 0;
        for (int i = 0; i < count; i++) { uint id = ReadOrderedId(r, ref previous); values[i] = new NetworkOwnedTubeNetwork(new EntityId(id), new TubeComponent { StationCount = r.ReadUInt16(), OperationalLinkCount = r.ReadUInt16(), TopologyRevision = r.ReadUInt32() }); }
        return values;
    }

    private static void WriteProduction(BinaryWriter w, NetworkOwnedProductionQueue queue)
    {
        w.Write(queue.Producer.Value); w.Write(queue.SpawnBlocked); w.Write(queue.HasRallyPoint); w.Write(queue.RallyPoint.X.Raw); w.Write(queue.RallyPoint.Y.Raw);
        w.Write(queue.RallyTargetEntity.Value); w.Write(queue.Items.Length);
        for (int i = 0; i < queue.Items.Length; i++)
        {
            NetworkProductionItem item = queue.Items[i]; w.Write(item.UnitType.Value); w.Write(item.TotalTicks); w.Write(item.RemainingTicks);
            w.Write(item.ReservedOre); w.Write(item.RequiredEnergy); w.Write(item.RequiredCrystals); w.Write(item.ReservedOperationsCapacity);
        }
    }

    private static void WriteBytes(BinaryWriter w, byte[] bytes) { w.Write(bytes.Length); w.Write(bytes); }
    private static byte[] ReadBytes(BinaryReader r)
    {
        int length = r.ReadInt32(); if (length < 0 || length > 16 * 1024) throw new InvalidDataException("Invalid own order queue size.");
        byte[] bytes = r.ReadBytes(length); if (bytes.Length != length) throw new EndOfStreamException(); return bytes;
    }
    private static int ReadCount(BinaryReader r, int maximum = 16_384)
    { int count = r.ReadInt32(); if (count < 0 || count > maximum) throw new InvalidDataException("Invalid own-systems count."); return count; }
    private static uint ReadOrderedId(BinaryReader r, ref uint previous)
    { uint id = r.ReadUInt32(); if (id == 0 || id <= previous) throw new InvalidDataException("Own-system IDs are not in stable order."); previous = id; return id; }
}
}
