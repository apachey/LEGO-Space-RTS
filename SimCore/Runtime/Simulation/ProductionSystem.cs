using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class ProductionSystem : ISimSystem
{
    private readonly List<SpawnReservation> _spawnReservations = new(16);

    public static bool IsRuntimeEnabledUnit(ContentId unitType) => unitType.Value != 0;

    public static bool IsRuntimeEnabledProducer(PrototypeContentCatalog content, ContentId buildingType)
    {
        for (int i = 0; i < content.Production.Length; i++)
            if (content.Production[i].CanProduceAt(buildingType)) return true;
        return false;
    }

    public void Step(SimulationWorld world)
    {
        _spawnReservations.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId facilityId = alive[i];
            if (!world.Entities.Production.TryGet(facilityId, out Production production) || production.Count == 0) continue;
            if (!BrownoutSystem.IsOperational(world, facilityId)) continue;
            ref Production stored = ref world.Entities.Production.Get(facilityId);
            ProductionQueueItem item = stored.Get(0);
            if (item.RemainingTicks > 0)
            {
                item.RemainingTicks--;
                stored.Set(0, item);
            }
            if (item.RemainingTicks > 0) continue;
            if (!TrySpawn(world, facilityId, item, out EntityId spawned))
            {
                stored.SpawnBlocked = true;
                continue;
            }
            stored.RemoveFirst();
            ApplyRally(world, facilityId, spawned);
        }
    }

    public static bool TryQueue(SimulationWorld world, byte playerSlot, EntityId facilityId, ContentId unitType)
    {
        if (!world.Entities.Production.Has(facilityId) || !world.Entities.Building.TryGet(facilityId, out Building building) || building.State != BuildingState.Completed ||
            !world.Entities.Ownership.TryGet(facilityId, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
            !world.Content.TryGetProduction(unitType, out UnitProductionDefinition definition) ||
            !definition.CanProduceAt(building.Type) || !ActionPrerequisites.AreMet(world, playerSlot, definition.PrerequisiteGroups)) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId);
        if (production.Count >= Production.Capacity) return false;
        if (!OperationsCapacitySystem.CanReserve(world, playerSlot, definition.OperationsCapacity)) return false;
        if (!EnergyDomainSystem.TryResolveForEntity(world, facilityId, playerSlot, out EntityId energyDomain) ||
            !EnergyDomainSystem.CanSpend(world, energyDomain, definition.EnergyCost)) return false;
        EntityId componentRoot = WorksiteGraphSystem.TryGetComponentForEntity(world, facilityId, out EntityId component) ? component : EntityId.None;
        if (!TryFindFundingBank(world, playerSlot, componentRoot, ResourceType.Ore, definition.OreCost, facilityId, out EntityId bankId) ||
            !TryFindFundingBank(world, playerSlot, componentRoot, ResourceType.Crystal, definition.CrystalCost, facilityId, out EntityId crystalBank)) return false;
        if (!EnergyDomainSystem.TrySpend(world, energyDomain, definition.EnergyCost)) return false;
        if (!TrySpend(world, playerSlot, bankId, ResourceType.Ore, definition.OreCost))
        {
            EnergyDomainSystem.Refund(world, energyDomain, definition.EnergyCost);
            return false;
        }
        if (!TrySpend(world, playerSlot, crystalBank, ResourceType.Crystal, definition.CrystalCost))
        {
            world.Entities.ResourceBank.Get(bankId).ProcessedAmount = checked(world.Entities.ResourceBank.Get(bankId).ProcessedAmount + definition.OreCost);
            EnergyDomainSystem.Refund(world, energyDomain, definition.EnergyCost);
            return false;
        }
        bool queued = production.TryEnqueue(new ProductionQueueItem
        {
            UnitType = definition.UnitType, FundingBank = bankId, CrystalFundingBank = crystalBank, EnergyDomainRoot = energyDomain, ReservedOre = definition.OreCost,
            RequiredEnergy = definition.EnergyCost, RequiredCrystals = definition.CrystalCost,
            ReservedOperationsCapacity = definition.OperationsCapacity, TotalTicks = definition.BuildTicks, RemainingTicks = definition.BuildTicks
        });
        if (!queued)
        {
            world.Entities.ResourceBank.Get(bankId).ProcessedAmount = checked(world.Entities.ResourceBank.Get(bankId).ProcessedAmount + definition.OreCost);
            RefundResource(world, playerSlot, ResourceType.Crystal, definition.CrystalCost, crystalBank);
            EnergyDomainSystem.Refund(world, energyDomain, definition.EnergyCost);
        }
        else OperationsCapacitySystem.Recalculate(world);
        return queued;
    }

    public static bool TryCancel(SimulationWorld world, byte playerSlot, EntityId facilityId, byte queueIndex)
    {
        if (!world.Entities.Production.Has(facilityId) || !world.Entities.Ownership.TryGet(facilityId, out Ownership owner) ||
            owner.PlayerSlot != playerSlot) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId);
        if (queueIndex >= production.Count) return false;
        ProductionQueueItem item = production.Get(queueIndex);
        int progress = item.TotalTicks - item.RemainingTicks;
        int oreRefund = CancellationAccounting.Refund(item.ReservedOre, progress, item.TotalTicks);
        int energyRefund = CancellationAccounting.Refund(item.RequiredEnergy, progress, item.TotalTicks);
        if (world.Entities.ResourceBank.Has(item.FundingBank))
            world.Entities.ResourceBank.Get(item.FundingBank).ProcessedAmount = checked(world.Entities.ResourceBank.Get(item.FundingBank).ProcessedAmount + oreRefund);
        if (world.Entities.EnergyDomain.Has(item.EnergyDomainRoot)) EnergyDomainSystem.Refund(world, item.EnergyDomainRoot, energyRefund);
        if (progress * 2 < item.TotalTicks)
            RefundResource(world, playerSlot, ResourceType.Crystal, item.RequiredCrystals, item.CrystalFundingBank);
        production.RemoveAt(queueIndex);
        OperationsCapacitySystem.Recalculate(world);
        return true;
    }

    public static bool TryReorder(SimulationWorld world, byte playerSlot, EntityId facilityId, byte fromIndex, byte toIndex)
    {
        if (!world.Entities.Production.Has(facilityId) || !world.Entities.Ownership.TryGet(facilityId, out Ownership owner) ||
            owner.PlayerSlot != playerSlot) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId);
        return production.TryReorder(fromIndex, toIndex);
    }

    public static bool TryRapidFabrication(SimulationWorld world, byte playerSlot, EntityId facilityId)
    {
        if (!world.Entities.Production.Has(facilityId) || !world.Entities.Ownership.TryGet(facilityId, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !world.Entities.Building.TryGet(facilityId, out Building building) || !ResearchSystem.HasCompleted(world, playerSlot, StableId.FromKey("research.ali.rapid_fabrication_conduits"))) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId); if (production.Count == 0) return false;
        ProductionQueueItem item = production.Get(0); if (item.RapidFabricationUsed || item.RemainingTicks <= 200) return false;
        ContentId fabricator = StableId.FromKey("building.ali.etx_fabricator"), dock = StableId.FromKey("building.ali.reconfiguration_dock");
        int cost = building.Type == fabricator ? 25 : building.Type == dock ? 35 : 0;
        int removed = building.Type == fabricator ? 300 : building.Type == dock ? 400 : 0;
        if (cost == 0) return false;
        ref AlienChargeState charge = ref world.GetAlienChargeRef(playerSlot);
        int milliCost = cost * AlienChargeSystem.MillichargePerCharge; if (charge.CurrentMillicharge < milliCost) return false;
        charge.CurrentMillicharge -= milliCost; item.RemainingTicks = checked((ushort)System.Math.Max(200, item.RemainingTicks - removed)); item.RapidFabricationUsed = true;
        production.Set(0, item); return true;
    }

    internal static bool TryFindFundingBank(SimulationWorld world, byte playerSlot, EntityId componentRoot, ResourceType type, int amount,
        EntityId origin, out EntityId bank)
    {
        if (amount == 0) { bank = EntityId.None; return true; }
        if (componentRoot != EntityId.None)
            return WorksiteGraphSystem.TryFindFundingBank(world, playerSlot, componentRoot, type, amount,
                world.Entities.Transform.Get(origin).Position, out bank);
        bank = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(id, out ResourceBank candidate) && candidate.Type == type && candidate.ProcessedAmount >= amount)
            { bank = id; return true; }
        }
        return false;
    }

    internal static bool TrySpend(SimulationWorld world, byte playerSlot, EntityId bank, ResourceType type, int amount)
    {
        if (amount == 0) return true;
        if (world.Entities.WorksiteMember.Has(bank) && WorksiteGraphSystem.TrySpendProcessedResource(world, playerSlot, bank, type, amount)) return true;
        if (!world.Entities.ResourceBank.TryGet(bank, out ResourceBank value) || value.Type != type || value.ProcessedAmount < amount) return false;
        world.Entities.ResourceBank.Get(bank).ProcessedAmount -= amount;
        return true;
    }

    internal static void RefundResource(SimulationWorld world, byte playerSlot, ResourceType type, int amount, EntityId preferred)
    {
        if (amount == 0) return;
        EntityId bank = preferred;
        if (!world.Entities.ResourceBank.TryGet(bank, out ResourceBank current) || current.Type != type)
        {
            bank = EntityId.None;
            IReadOnlyList<EntityId> alive = world.Entities.Alive;
            for (int i = 0; i < alive.Count; i++)
                if (world.Entities.Ownership.TryGet(alive[i], out Ownership owner) && owner.PlayerSlot == playerSlot &&
                    world.Entities.ResourceBank.TryGet(alive[i], out ResourceBank candidate) && candidate.Type == type) { bank = alive[i]; break; }
        }
        if (bank != EntityId.None) world.Entities.ResourceBank.Get(bank).ProcessedAmount = checked(world.Entities.ResourceBank.Get(bank).ProcessedAmount + amount);
    }

    public static bool TrySetRally(SimulationWorld world, byte playerSlot, EntityId facilityId, FixVec2 point, EntityId resourceTarget)
    {
        if (!world.Entities.Production.Has(facilityId) || !world.Entities.Ownership.TryGet(facilityId, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
            point.X < Fix32.Zero || point.Y < Fix32.Zero || point.X >= Fix32.FromInt(MapGrid.BuildWidth) || point.Y >= Fix32.FromInt(MapGrid.BuildHeight)) return false;
        if (resourceTarget != EntityId.None && !world.Entities.ResourceNode.Has(resourceTarget)) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId);
        production.HasRallyPoint = true; production.RallyPoint = point; production.RallyTargetEntity = resourceTarget;
        return true;
    }

    private bool TrySpawn(SimulationWorld world, EntityId facilityId, ProductionQueueItem item, out EntityId spawned)
    {
        spawned = EntityId.None;
        if (!world.Entities.Building.TryGet(facilityId, out Building building) ||
            !world.Content.TryGetBuilding(building.Type, out BuildingDefinition buildingDefinition) ||
            !world.Content.TryGetEntity(item.UnitType, out PrototypeEntityDefinition unitDefinition) ||
            !world.Entities.Ownership.TryGet(facilityId, out Ownership ownership)) return false;
        IntRect exit = ConstructionPlacement.GetProductionExit(buildingDefinition, building.AnchorX, building.AnchorY, building.Orientation);
        FixVec2 center = new(Fix32.FromRatio(exit.X * 2 + exit.Width, 2), Fix32.FromRatio(exit.Y * 2 + exit.Height, 2));
        if (IsSpawnLegal(world, center, unitDefinition.Footprint))
        {
            spawned = SpawnAt(world, ownership.PlayerSlot, item.UnitType, center, unitDefinition.Footprint);
            return true;
        }
        for (int y = exit.Y; y < exit.Y + exit.Height; y++)
        for (int x = exit.X; x < exit.X + exit.Width; x++)
        {
            FixVec2 candidate = new(Fix32.FromRatio(x * 2 + 1, 2), Fix32.FromRatio(y * 2 + 1, 2));
            if (candidate.Equals(center) || !IsSpawnLegal(world, candidate, unitDefinition.Footprint)) continue;
            spawned = SpawnAt(world, ownership.PlayerSlot, item.UnitType, candidate, unitDefinition.Footprint);
            return true;
        }
        return false;
    }

    private EntityId SpawnAt(SimulationWorld world, byte playerSlot, ContentId unitType, FixVec2 position, FootprintClass footprint)
    {
        EntityId spawned = ScenarioFactory.SpawnProducedUnit(world, playerSlot, unitType, position);
        _spawnReservations.Add(new SpawnReservation(position, FootprintRules.CollisionRadiusBuild(footprint)));
        return spawned;
    }

    private bool IsSpawnLegal(SimulationWorld world, FixVec2 position, FootprintClass footprint)
    {
        if (!world.Pathfinder.IsPassable(MapGrid.BuildToNav(position), footprint)) return false;
        Fix32 radius = FootprintRules.CollisionRadiusBuild(footprint);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Navigation.TryGet(id, out NavigationAgent navigation) || !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 required = radius + FootprintRules.CollisionRadiusBuild(navigation.Footprint);
            if (FixVec2.Distance(position, transform.Position) < required) return false;
        }
        for (int i = 0; i < _spawnReservations.Count; i++)
            if (FixVec2.Distance(position, _spawnReservations[i].Position) < radius + _spawnReservations[i].Radius) return false;
        return true;
    }

    private static void ApplyRally(SimulationWorld world, EntityId facilityId, EntityId spawned)
    {
        if (!world.Entities.Production.TryGet(facilityId, out Production production) || !production.HasRallyPoint) return;
        if (production.RallyTargetEntity != EntityId.None && world.Entities.Worker.Has(spawned) && CommandExecutionSystem.StartHarvest(world, spawned, production.RallyTargetEntity)) return;
        if (!world.Entities.Navigation.TryGet(spawned, out NavigationAgent navigation) ||
            !world.Pathfinder.IsPassable(MapGrid.BuildToNav(production.RallyPoint), navigation.Footprint)) return;
        CommandExecutionSystem.SetMove(world, spawned, production.RallyPoint);
    }

    private readonly struct SpawnReservation
    {
        public readonly FixVec2 Position; public readonly Fix32 Radius;
        public SpawnReservation(FixVec2 position, Fix32 radius) { Position = position; Radius = radius; }
    }
}
}
