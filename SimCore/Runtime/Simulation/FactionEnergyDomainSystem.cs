using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Faction-specific deterministic Energy-domain membership outside the Raider Worksite graph.</summary>
public sealed class FactionEnergyDomainSystem : ISimSystem
{
    public const byte DomainRadius = 18;
    private static readonly ContentId AstCommand = StableId.FromKey("building.ast.mb01_eagle_command_base");
    private static readonly ContentId AstService = StableId.FromKey("building.ast.service_refit_hub");
    private static readonly ContentId AstSolar = StableId.FromKey("building.ast.solar_energy_array");
    private static readonly ContentId AlienCommand = StableId.FromKey("building.ali.etx_command_core");

    public void Step(SimulationWorld world)
    {
        ulong signature = Signature(world);
        if (signature == world.FactionEnergyTopologySignature) return;
        world.FactionEnergyTopologySignature = signature;
        Rebuild(world);
    }

    public static bool Rebuild(SimulationWorld world)
    {
        List<EntityId> astNodes = new(), alienNodes = new(), martianStations = new(), factionBuildings = new();
        Dictionary<uint, uint> oldMemberships = new(); Dictionary<uint, EnergyDomain> oldDomains = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed ||
                !world.Content.TryGetEntity(building.Type, out PrototypeEntityDefinition entity) || !world.Entities.Transform.Has(id)) continue;
            if (entity.FactionKey == "RockRaiders") continue;
            factionBuildings.Add(id);
            if (world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember oldMember)) oldMemberships[id.Value] = oldMember.DomainRoot.Value;
            if (world.Entities.EnergyDomain.TryGet(id, out EnergyDomain oldDomain)) oldDomains[id.Value] = oldDomain;
            if (entity.FactionKey == "Astronauts" && (building.Type == AstCommand || building.Type == AstService || building.Type == AstSolar)) astNodes.Add(id);
            else if (entity.FactionKey == "Aliens" && building.Type == AlienCommand && BrownoutSystem.IsOperational(world, id)) alienNodes.Add(id);
            else if (entity.FactionKey == "Martians" && world.Entities.TubeStation.Has(id)) martianStations.Add(id);
        }
        Dictionary<uint, EntityId> desired = new();
        AssignAreaComponents(world, astNodes, factionBuildings, "Astronauts", desired);
        AssignNearest(world, alienNodes, factionBuildings, "Aliens", desired);
        AssignMartian(world, martianStations, factionBuildings, desired);
        Dictionary<uint, long> allocatedReserve = AllocateReserve(world, oldDomains, oldMemberships, desired);
        bool changed = false;
        for (int i = 0; i < factionBuildings.Count; i++)
        {
            EntityId id = factionBuildings[i]; EntityId next = desired.TryGetValue(id.Value, out EntityId root) ? root : EntityId.None;
            if (world.Entities.Building.TryGet(id, out Building building) && world.Content.TryGetEntity(building.Type, out PrototypeEntityDefinition entity) &&
                ((entity.FactionKey == "Astronauts" && astNodes.Count == 0) || (entity.FactionKey == "Aliens" && alienNodes.Count == 0) ||
                 (entity.FactionKey == "Martians" && martianStations.Count == 0))) continue;
            EntityId previous = world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member) ? member.DomainRoot : EntityId.None;
            if (previous == next) continue;
            if (next == EntityId.None) world.Entities.EnergyDomainMember.Remove(id);
            else world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = next });
            changed = true;
        }
        HashSet<uint> roots = new(); foreach (EntityId root in desired.Values) roots.Add(root.Value);
        foreach (uint oldRoot in oldDomains.Keys)
            if (!roots.Contains(oldRoot) && !world.Entities.WorksiteComponent.Has(new EntityId(oldRoot))) world.Entities.EnergyDomain.Remove(new EntityId(oldRoot));
        foreach (uint value in roots) if (!world.Entities.EnergyDomain.Has(new EntityId(value))) world.Entities.EnergyDomain.Set(new EntityId(value), new EnergyDomain());
        ResonanceCoreSystem.ReassociateAll(world);
        if (changed)
        {
            EnergyDomainSystem.RecalculateAll(world);
            foreach (uint root in roots)
                if (allocatedReserve.TryGetValue(root, out long raw) && world.Entities.EnergyDomain.Has(new EntityId(root)))
                {
                    ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(new EntityId(root));
                    domain.Reserve = Fix32.Min(domain.ReserveCapacity, Fix32.FromRaw(checked((int)System.Math.Clamp(raw, 0, int.MaxValue))));
                    BrownoutSystem.Recalculate(world, new EntityId(root));
                }
        }
        return changed;
    }

    private static Dictionary<uint, long> AllocateReserve(SimulationWorld world, Dictionary<uint, EnergyDomain> oldDomains,
        Dictionary<uint, uint> oldMemberships, Dictionary<uint, EntityId> desired)
    {
        Dictionary<uint, Dictionary<uint, int>> weights = new();
        foreach (KeyValuePair<uint, uint> oldMembership in oldMemberships)
        {
            if (!desired.TryGetValue(oldMembership.Key, out EntityId nextRoot) || !oldDomains.ContainsKey(oldMembership.Value)) continue;
            EntityId entity = new(oldMembership.Key);
            if (!world.Entities.Building.TryGet(entity, out Building building) || !world.Content.TryGetBuilding(building.Type, out BuildingDefinition definition) || definition.EnergyReserveCapacity == 0) continue;
            if (!weights.TryGetValue(oldMembership.Value, out Dictionary<uint, int>? targets)) weights[oldMembership.Value] = targets = new();
            targets[nextRoot.Value] = checked(targets.TryGetValue(nextRoot.Value, out int current) ? current + definition.EnergyReserveCapacity : definition.EnergyReserveCapacity);
        }
        Dictionary<uint, long> result = new(); List<uint> oldRoots = new(oldDomains.Keys); oldRoots.Sort();
        for (int i = 0; i < oldRoots.Count; i++)
        {
            uint oldRoot = oldRoots[i]; if (!weights.TryGetValue(oldRoot, out Dictionary<uint, int>? targets) || targets.Count == 0) continue;
            List<uint> ordered = new(targets.Keys); ordered.Sort(); int remainingWeight = 0;
            for (int t = 0; t < ordered.Count; t++) remainingWeight = checked(remainingWeight + targets[ordered[t]]);
            long remaining = oldDomains[oldRoot].Reserve.Raw;
            for (int t = 0; t < ordered.Count; t++)
            {
                uint root = ordered[t]; int weight = targets[root]; long share = t == ordered.Count - 1 ? remaining : remaining * weight / remainingWeight;
                result[root] = checked(result.TryGetValue(root, out long current) ? current + share : share);
                remaining -= share; remainingWeight -= weight;
            }
        }
        return result;
    }

    private static void AssignAreaComponents(SimulationWorld world, List<EntityId> nodes, List<EntityId> buildings, string faction, Dictionary<uint, EntityId> desired)
    {
        Dictionary<uint, EntityId> roots = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            EntityId root = nodes[i];
            for (int j = 0; j < nodes.Count; j++) if (SameOwner(world, nodes[i], nodes[j]) && InRange(world, nodes[i], nodes[j]) && nodes[j].Value < root.Value) root = nodes[j];
            roots[nodes[i].Value] = root;
        }
        // Repeated relaxation joins transitive overlaps into one deterministic component.
        for (int pass = 0; pass < nodes.Count; pass++)
            for (int i = 0; i < nodes.Count; i++) for (int j = 0; j < nodes.Count; j++)
                if (SameOwner(world, nodes[i], nodes[j]) && InRange(world, nodes[i], nodes[j]))
                { EntityId root = roots[nodes[i].Value].Value < roots[nodes[j].Value].Value ? roots[nodes[i].Value] : roots[nodes[j].Value]; roots[nodes[i].Value] = root; roots[nodes[j].Value] = root; }
        for (int i = 0; i < buildings.Count; i++)
        {
            EntityId building = buildings[i]; if (!IsFaction(world, building, faction)) continue;
            EntityId best = EntityId.None;
            for (int n = 0; n < nodes.Count; n++) if (SameOwner(world, building, nodes[n]) && WithinDomainArea(world, building, nodes[n]))
            { EntityId root = roots[nodes[n].Value]; if (best == EntityId.None || root.Value < best.Value) best = root; }
            if (best != EntityId.None) desired[building.Value] = best;
        }
    }

    private static void AssignNearest(SimulationWorld world, List<EntityId> nodes, List<EntityId> buildings, string faction, Dictionary<uint, EntityId> desired)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            EntityId building = buildings[i]; if (!IsFaction(world, building, faction)) continue;
            EntityId best = EntityId.None; Fix32 bestDistance = Fix32.MaxValue;
            for (int n = 0; n < nodes.Count; n++)
            {
                EntityId node = nodes[n]; if (!SameOwner(world, building, node)) continue;
                Fix32 distance = FixVec2.Distance(world.Entities.Transform.Get(building).Position, world.Entities.Transform.Get(node).Position);
                if (distance > Fix32.FromInt(DomainRadius)) continue;
                if (best == EntityId.None || distance < bestDistance || (distance == bestDistance && node.Value < best.Value)) { best = node; bestDistance = distance; }
            }
            if (best != EntityId.None) desired[building.Value] = best;
        }
    }

    private static void AssignMartian(SimulationWorld world, List<EntityId> stations, List<EntityId> buildings, Dictionary<uint, EntityId> desired)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            EntityId building = buildings[i]; if (!IsFaction(world, building, "Martians")) continue;
            EntityId best = EntityId.None; Fix32 bestDistance = Fix32.MaxValue;
            for (int n = 0; n < stations.Count; n++)
            {
                EntityId station = stations[n]; if (!SameOwner(world, building, station)) continue;
                EntityId root = TubeGraphSystem.TryGetComponent(world, station, out EntityId component) ? component : station;
                Fix32 distance = FixVec2.Distance(world.Entities.Transform.Get(building).Position, world.Entities.Transform.Get(station).Position);
                if (distance > Fix32.FromInt(DomainRadius)) continue;
                if (best == EntityId.None || distance < bestDistance || (distance == bestDistance && root.Value < best.Value)) { best = root; bestDistance = distance; }
            }
            if (best != EntityId.None) desired[building.Value] = best;
        }
    }

    private static bool InRange(SimulationWorld world, EntityId a, EntityId b) => FixVec2.Distance(world.Entities.Transform.Get(a).Position, world.Entities.Transform.Get(b).Position) <= Fix32.FromInt(DomainRadius * 2);
    private static bool WithinDomainArea(SimulationWorld world, EntityId a, EntityId b) => FixVec2.Distance(world.Entities.Transform.Get(a).Position, world.Entities.Transform.Get(b).Position) <= Fix32.FromInt(DomainRadius);
    private static bool SameOwner(SimulationWorld world, EntityId a, EntityId b) => world.Entities.Ownership.Get(a).PlayerSlot == world.Entities.Ownership.Get(b).PlayerSlot;
    private static bool IsFaction(SimulationWorld world, EntityId id, string faction) => world.Entities.Building.TryGet(id, out Building b) && world.Content.TryGetEntity(b.Type, out PrototypeEntityDefinition e) && e.FactionKey == faction;

    private static ulong Signature(SimulationWorld world)
    {
        ulong hash = 1469598103934665603UL;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i]; if (!world.Entities.Building.TryGet(id, out Building building)) continue;
            hash = (hash ^ id.Value) * 1099511628211UL; hash = (hash ^ building.Type.Value) * 1099511628211UL; hash = (hash ^ (byte)building.State) * 1099511628211UL;
            if (world.Entities.Transform.TryGet(id, out SimTransform transform)) { hash = (hash ^ (uint)transform.Position.X.Raw) * 1099511628211UL; hash = (hash ^ (uint)transform.Position.Y.Raw) * 1099511628211UL; }
            if (world.Entities.PowerState.TryGet(id, out PowerState power)) hash = (hash ^ (power.IsPowered ? 1UL : 0UL)) * 1099511628211UL;
        }
        return (hash ^ world.TubeTopologyRevision) * 1099511628211UL;
    }
}
}
