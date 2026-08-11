using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public static class WorksiteGraphSystem
{
    private static readonly ContentId HqType = StableId.FromKey("building.rock_raiders.hq");

    private readonly struct NodeInfo
    {
        public readonly EntityId Entity;
        public readonly byte Owner;
        public readonly FixVec2 Center;
        public readonly byte Radius;

        public NodeInfo(EntityId entity, byte owner, FixVec2 center, byte radius)
        { Entity = entity; Owner = owner; Center = center; Radius = radius; }
    }

    public static void InitializeOpening(SimulationWorld world)
    {
        Rebuild(world);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId root = alive[i];
            if (!world.Entities.WorksiteComponent.Has(root) || !world.Entities.EnergyDomain.Has(root) || !ComponentContainsHq(world, root)) continue;
            ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
            domain.Reserve = Fix32.Min(domain.ReserveCapacity, Fix32.FromInt(EnergyDomainSystem.CanonicalStartingReserve));
        }
        EnergyDomainSystem.RecalculateAll(world);
    }

    public static bool Rebuild(SimulationWorld world)
    {
        List<NodeInfo> nodes = CollectNodes(world);
        Dictionary<uint, uint> nodeRoots = BuildNodeComponents(nodes);
        Dictionary<uint, uint> memberships = BuildMemberships(world, nodes, nodeRoots);
        if (MatchesCurrentTopology(world, nodes, nodeRoots, memberships)) return false;

        Dictionary<uint, EnergyDomain> oldDomains = new();
        Dictionary<uint, uint> oldMemberships = new();
        uint nextRevision = 1;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.EnergyDomain.TryGet(id, out EnergyDomain domain)) oldDomains.Add(id.Value, domain);
            if (world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member)) oldMemberships.Add(id.Value, member.ComponentRoot.Value);
            if (world.Entities.WorksiteComponent.TryGet(id, out WorksiteComponent component) && component.TopologyRevision >= nextRevision)
                nextRevision = checked(component.TopologyRevision + 1);
        }

        Dictionary<uint, long> newReserveRaw = AllocateReserve(world, oldDomains, oldMemberships, memberships);
        Dictionary<uint, uint> brownoutRevision = AllocateBrownoutRevision(oldDomains, oldMemberships, memberships);
        Dictionary<uint, ushort> nodeCounts = new();
        Dictionary<uint, ushort> memberCounts = new();
        for (int i = 0; i < nodes.Count; i++) Increment(nodeCounts, nodeRoots[nodes[i].Entity.Value]);
        for (int i = 0; i < alive.Count; i++)
            if (memberships.TryGetValue(alive[i].Value, out uint memberRoot)) Increment(memberCounts, memberRoot);

        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            world.Entities.WorksiteNode.Remove(id);
            world.Entities.WorksiteMember.Remove(id);
            world.Entities.WorksiteComponent.Remove(id);
            world.Entities.EnergyDomainMember.Remove(id);
            world.Entities.EnergyDomain.Remove(id);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            NodeInfo node = nodes[i];
            world.Entities.WorksiteNode.Set(node.Entity, new WorksiteNode { ServiceRadius = node.Radius, ComponentRoot = new EntityId(nodeRoots[node.Entity.Value]) });
        }
        for (int i = 0; i < alive.Count; i++)
        {
            uint entityValue = alive[i].Value;
            if (!memberships.TryGetValue(entityValue, out uint rootValue)) continue;
            EntityId entity = new(entityValue), root = new(rootValue);
            world.Entities.WorksiteMember.Set(entity, new WorksiteMember { ComponentRoot = root });
            world.Entities.EnergyDomainMember.Set(entity, new EnergyDomainMember { DomainRoot = root });
        }
        List<uint> orderedRoots = new(nodeCounts.Keys); orderedRoots.Sort();
        for (int i = 0; i < orderedRoots.Count; i++)
        {
            uint rootValue = orderedRoots[i]; ushort nodeCount = nodeCounts[rootValue];
            EntityId root = new(rootValue);
            world.Entities.WorksiteComponent.Set(root, new WorksiteComponent
            {
                NodeCount = nodeCount,
                MemberCount = memberCounts.TryGetValue(rootValue, out ushort count) ? count : (ushort)0,
                TopologyRevision = nextRevision
            });
            long reserveRaw = newReserveRaw.TryGetValue(rootValue, out long value) ? value : 0;
            world.Entities.EnergyDomain.Set(root, new EnergyDomain
            {
                Reserve = Fix32.FromRaw(checked((int)Math.Clamp(reserveRaw, 0, int.MaxValue))),
                BrownoutRevision = brownoutRevision.TryGetValue(rootValue, out uint revision) ? revision : 0,
                LastBrownoutEvent = BrownoutEventKind.Changed
            });
        }
        EnergyDomainSystem.RecalculateAll(world);
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ConstructionSite.Has(id)) continue;
            ref ConstructionSite site = ref world.Entities.ConstructionSite.Get(id);
            if (world.Entities.WorksiteMember.TryGet(site.FundingBank, out WorksiteMember fundingMember) && world.Entities.EnergyDomain.Has(fundingMember.ComponentRoot))
                site.EnergyDomainRoot = fundingMember.ComponentRoot;
        }
        return true;
    }

    public static bool TryGetComponentForEntity(SimulationWorld world, EntityId entity, out EntityId root)
    {
        if (world.Entities.WorksiteMember.TryGet(entity, out WorksiteMember member) && world.Entities.WorksiteComponent.Has(member.ComponentRoot))
        { root = member.ComponentRoot; return true; }
        root = EntityId.None; return false;
    }

    public static bool TryGetComponentAt(SimulationWorld world, byte playerSlot, FixVec2 position, out EntityId root)
    {
        root = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.WorksiteNode.TryGet(id, out WorksiteNode node) ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform) || !Contains(transform.Position, node.ServiceRadius, position)) continue;
            if (root == EntityId.None || node.ComponentRoot.Value < root.Value) root = node.ComponentRoot;
        }
        return root != EntityId.None;
    }

    public static EntityId[] GetPlayerComponents(SimulationWorld world, byte playerSlot)
    {
        List<EntityId> roots = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.WorksiteComponent.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == playerSlot)
                roots.Add(id);
        }
        return roots.ToArray();
    }

    public static bool SharesComponent(SimulationWorld world, EntityId first, EntityId second)
        => world.Entities.WorksiteMember.TryGet(first, out WorksiteMember a) &&
           world.Entities.WorksiteMember.TryGet(second, out WorksiteMember b) && a.ComponentRoot == b.ComponentRoot;

    public static int GetProcessedResourceTotal(SimulationWorld world, EntityId componentRoot, ResourceType type)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) || member.ComponentRoot != componentRoot ||
                !world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type) continue;
            total = checked(total + bank.ProcessedAmount);
        }
        return total;
    }

    public static int GetPendingHauledResourceTotal(SimulationWorld world, EntityId componentRoot, ResourceType type)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) || member.ComponentRoot != componentRoot ||
                !world.Entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver) || receiver.AcceptedType != type) continue;
            total = checked(total + receiver.PendingHauledAmount);
        }
        return total;
    }

    public static bool TryFindFundingBank(SimulationWorld world, byte playerSlot, EntityId requiredComponent, ResourceType type,
        int amount, FixVec2 origin, out EntityId fundingBank)
    {
        fundingBank = EntityId.None;
        if (amount < 0) return false;
        Dictionary<uint, int> totals = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type || bank.ProcessedAmount < 0 ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) ||
                (requiredComponent != EntityId.None && member.ComponentRoot != requiredComponent)) continue;
            totals[member.ComponentRoot.Value] = checked(totals.TryGetValue(member.ComponentRoot.Value, out int total) ? total + bank.ProcessedAmount : bank.ProcessedAmount);
        }

        Fix32 bestDistance = Fix32.MaxValue;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) || !totals.TryGetValue(member.ComponentRoot.Value, out int total) || total < amount ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 distance = FixVec2.Distance(origin, transform.Position);
            if (fundingBank == EntityId.None || distance < bestDistance || (distance == bestDistance && id.Value < fundingBank.Value))
            { fundingBank = id; bestDistance = distance; }
        }
        return fundingBank != EntityId.None;
    }

    public static bool TrySpendProcessedResource(SimulationWorld world, byte playerSlot, EntityId fundingBank, ResourceType type, int amount)
    {
        if (amount < 0 || !world.Entities.WorksiteMember.TryGet(fundingBank, out WorksiteMember fundingMember) ||
            !world.Entities.Ownership.TryGet(fundingBank, out Ownership fundingOwner) || fundingOwner.PlayerSlot != playerSlot) return false;
        List<EntityId> banks = new(); int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != type || bank.ProcessedAmount < 0 ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) || member.ComponentRoot != fundingMember.ComponentRoot) continue;
            banks.Add(id); total = checked(total + bank.ProcessedAmount);
        }
        if (total < amount) return false;
        banks.Sort((a, b) => a == b ? 0 : a == fundingBank ? -1 : b == fundingBank ? 1 : a.Value.CompareTo(b.Value));
        int remaining = amount;
        for (int i = 0; i < banks.Count && remaining > 0; i++)
        {
            ref ResourceBank bank = ref world.Entities.ResourceBank.Get(banks[i]);
            int spent = Math.Min(bank.ProcessedAmount, remaining);
            bank.ProcessedAmount -= spent; remaining -= spent;
        }
        return remaining == 0;
    }

    private static List<NodeInfo> CollectNodes(SimulationWorld world)
    {
        List<NodeInfo> nodes = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed ||
                !world.Content.TryGetBuilding(building.Type, out BuildingDefinition definition) || definition.WorksiteServiceRadius == 0 ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            nodes.Add(new NodeInfo(id, ownership.PlayerSlot, transform.Position, definition.WorksiteServiceRadius));
        }
        return nodes;
    }

    private static Dictionary<uint, uint> BuildNodeComponents(List<NodeInfo> nodes)
    {
        int[] parent = new int[nodes.Count];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;
        for (int i = 0; i < nodes.Count; i++)
        for (int j = i + 1; j < nodes.Count; j++)
            if (nodes[i].Owner == nodes[j].Owner && ZonesOverlap(nodes[i], nodes[j])) Union(parent, i, j);

        Dictionary<int, uint> roots = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            int set = Find(parent, i);
            if (!roots.TryGetValue(set, out uint root) || nodes[i].Entity.Value < root) roots[set] = nodes[i].Entity.Value;
        }
        Dictionary<uint, uint> result = new();
        for (int i = 0; i < nodes.Count; i++) result.Add(nodes[i].Entity.Value, roots[Find(parent, i)]);
        return result;
    }

    private static Dictionary<uint, uint> BuildMemberships(SimulationWorld world, List<NodeInfo> nodes, Dictionary<uint, uint> nodeRoots)
    {
        Dictionary<uint, uint> memberships = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Building.Has(id) || !world.Entities.Ownership.TryGet(id, out Ownership ownership) ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            uint bestRoot = 0;
            for (int n = 0; n < nodes.Count; n++)
            {
                NodeInfo node = nodes[n];
                if (node.Owner != ownership.PlayerSlot || !Contains(node.Center, node.Radius, transform.Position)) continue;
                uint candidate = nodeRoots[node.Entity.Value];
                if (bestRoot == 0 || candidate < bestRoot) bestRoot = candidate;
            }
            if (bestRoot != 0) memberships.Add(id.Value, bestRoot);
        }
        return memberships;
    }

    private static bool MatchesCurrentTopology(SimulationWorld world, List<NodeInfo> nodes, Dictionary<uint, uint> nodeRoots, Dictionary<uint, uint> memberships)
    {
        int currentNodes = 0, currentMembers = 0, currentComponents = 0;
        Dictionary<uint, ushort> expectedNodeCounts = new(), expectedMemberCounts = new();
        for (int i = 0; i < nodes.Count; i++) Increment(expectedNodeCounts, nodeRoots[nodes[i].Entity.Value]);
        for (int i = 0; i < world.Entities.Alive.Count; i++)
            if (memberships.TryGetValue(world.Entities.Alive[i].Value, out uint root)) Increment(expectedMemberCounts, root);
        HashSet<uint> components = new(nodeRoots.Values);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.WorksiteNode.TryGet(id, out WorksiteNode node))
            {
                currentNodes++;
                NodeInfo expectedNode = nodes.Find(x => x.Entity == id);
                if (!nodeRoots.TryGetValue(id.Value, out uint expected) || node.ComponentRoot.Value != expected || node.ServiceRadius != expectedNode.Radius) return false;
            }
            if (world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member))
            {
                currentMembers++;
                if (!memberships.TryGetValue(id.Value, out uint expected) || member.ComponentRoot.Value != expected ||
                    !world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember energyMember) || energyMember.DomainRoot != member.ComponentRoot) return false;
            }
            if (world.Entities.WorksiteComponent.TryGet(id, out WorksiteComponent component))
            {
                currentComponents++;
                if (!components.Contains(id.Value) || !expectedNodeCounts.TryGetValue(id.Value, out ushort nodesExpected) || component.NodeCount != nodesExpected ||
                    !expectedMemberCounts.TryGetValue(id.Value, out ushort membersExpected) || component.MemberCount != membersExpected || !world.Entities.EnergyDomain.Has(id)) return false;
            }
        }
        return currentNodes == nodes.Count && currentMembers == memberships.Count && currentComponents == components.Count;
    }

    private static Dictionary<uint, long> AllocateReserve(SimulationWorld world, Dictionary<uint, EnergyDomain> oldDomains,
        Dictionary<uint, uint> oldMemberships, Dictionary<uint, uint> newMemberships)
    {
        Dictionary<uint, Dictionary<uint, int>> weights = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            uint entityValue = alive[i].Value;
            if (!oldMemberships.TryGetValue(entityValue, out uint oldRoot)) continue;
            if (!oldDomains.ContainsKey(oldRoot) || !newMemberships.TryGetValue(entityValue, out uint newRoot)) continue;
            EntityId entity = new(entityValue);
            if (!world.Entities.Building.TryGet(entity, out Building building) || building.State != BuildingState.Completed ||
                !world.Content.TryGetBuilding(building.Type, out BuildingDefinition definition) || definition.EnergyReserveCapacity == 0) continue;
            if (!weights.TryGetValue(oldRoot, out Dictionary<uint, int>? targets)) weights.Add(oldRoot, targets = new Dictionary<uint, int>());
            targets[newRoot] = checked(targets.TryGetValue(newRoot, out int current) ? current + definition.EnergyReserveCapacity : definition.EnergyReserveCapacity);
        }

        Dictionary<uint, long> result = new();
        List<uint> oldRoots = new(oldDomains.Keys); oldRoots.Sort();
        for (int oldIndex = 0; oldIndex < oldRoots.Count; oldIndex++)
        {
            uint oldRoot = oldRoots[oldIndex]; EnergyDomain domain = oldDomains[oldRoot];
            if (!weights.TryGetValue(oldRoot, out Dictionary<uint, int>? targets) || targets.Count == 0) continue;
            List<uint> ordered = new(targets.Keys); ordered.Sort();
            int totalWeight = 0; for (int i = 0; i < ordered.Count; i++) totalWeight = checked(totalWeight + targets[ordered[i]]);
            long remaining = domain.Reserve.Raw; int remainingWeight = totalWeight;
            for (int i = 0; i < ordered.Count; i++)
            {
                uint target = ordered[i]; int weight = targets[target];
                long share = i == ordered.Count - 1 ? remaining : remaining * weight / remainingWeight;
                result[target] = checked(result.TryGetValue(target, out long current) ? current + share : share);
                remaining -= share; remainingWeight -= weight;
            }
        }
        return result;
    }

    private static Dictionary<uint, uint> AllocateBrownoutRevision(Dictionary<uint, EnergyDomain> oldDomains,
        Dictionary<uint, uint> oldMemberships, Dictionary<uint, uint> newMemberships)
    {
        Dictionary<uint, uint> result = new();
        List<uint> entities = new(oldMemberships.Keys); entities.Sort();
        for (int i = 0; i < entities.Count; i++)
        {
            uint entity = entities[i], oldRoot = oldMemberships[entity];
            if (!newMemberships.TryGetValue(entity, out uint newRoot) || !oldDomains.TryGetValue(oldRoot, out EnergyDomain domain)) continue;
            if (!result.TryGetValue(newRoot, out uint current) || domain.BrownoutRevision > current) result[newRoot] = domain.BrownoutRevision;
        }
        return result;
    }

    private static bool ComponentContainsHq(SimulationWorld world, EntityId root)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.WorksiteMember.TryGet(id, out WorksiteMember member) && member.ComponentRoot == root &&
                world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed && building.Type == HqType) return true;
        }
        return false;
    }

    private static bool ZonesOverlap(NodeInfo a, NodeInfo b)
    {
        long dx = (long)a.Center.X.Raw - b.Center.X.Raw, dy = (long)a.Center.Y.Raw - b.Center.Y.Raw;
        long radius = (long)(a.Radius + b.Radius) * Fix32.OneRaw;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static bool Contains(FixVec2 center, byte radiusCells, FixVec2 point)
    {
        long dx = (long)center.X.Raw - point.X.Raw, dy = (long)center.Y.Raw - point.Y.Raw;
        long radius = (long)radiusCells * Fix32.OneRaw;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static int Find(int[] parent, int value)
    {
        while (parent[value] != value) value = parent[value];
        return value;
    }

    private static void Union(int[] parent, int a, int b)
    {
        int rootA = Find(parent, a), rootB = Find(parent, b);
        if (rootA == rootB) return;
        if (rootA < rootB) parent[rootB] = rootA; else parent[rootA] = rootB;
    }

    private static void Increment(Dictionary<uint, ushort> counts, uint key)
        => counts[key] = checked((ushort)(counts.TryGetValue(key, out ushort count) ? count + 1 : 1));
}
}
