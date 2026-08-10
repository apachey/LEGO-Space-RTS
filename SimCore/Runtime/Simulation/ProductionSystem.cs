using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class ProductionSystem : ISimSystem
{
    private readonly List<SpawnReservation> _spawnReservations = new(16);

    public void Step(SimulationWorld world)
    {
        _spawnReservations.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId facilityId = alive[i];
            if (!world.Entities.Production.TryGet(facilityId, out Production production) || production.Count == 0) continue;
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
            !world.Content.TryGetProduction(unitType, out UnitProductionDefinition definition) || definition.ProducerType != building.Type) return false;
        ref Production production = ref world.Entities.Production.Get(facilityId);
        if (production.Count >= Production.Capacity) return false;
        if (!OperationsCapacitySystem.CanReserve(world, playerSlot, definition.OperationsCapacity)) return false;
        EntityId bankId = FindFundingBank(world, playerSlot, definition.OreCost, world.Entities.Transform.Get(facilityId).Position);
        if (bankId == EntityId.None) return false;
        ref ResourceBank bank = ref world.Entities.ResourceBank.Get(bankId);
        bank.ProcessedAmount = checked(bank.ProcessedAmount - definition.OreCost);
        bool queued = production.TryEnqueue(new ProductionQueueItem
        {
            UnitType = definition.UnitType, FundingBank = bankId, ReservedOre = definition.OreCost,
            RequiredEnergy = definition.EnergyCost, RequiredCrystals = definition.CrystalCost,
            ReservedOperationsCapacity = definition.OperationsCapacity, TotalTicks = definition.BuildTicks, RemainingTicks = definition.BuildTicks
        });
        if (!queued) bank.ProcessedAmount = checked(bank.ProcessedAmount + definition.OreCost);
        else OperationsCapacitySystem.Recalculate(world);
        return queued;
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

    private static EntityId FindFundingBank(SimulationWorld world, byte playerSlot, int oreCost, FixVec2 origin)
    {
        EntityId best = EntityId.None; Fix32 bestDistance = Fix32.MaxValue;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != ResourceType.Ore || bank.ProcessedAmount < oreCost ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 distance = FixVec2.Distance(origin, transform.Position);
            if (best == EntityId.None || distance < bestDistance || (distance == bestDistance && id.Value < best.Value)) { best = id; bestDistance = distance; }
        }
        return best;
    }

    private readonly struct SpawnReservation
    {
        public readonly FixVec2 Position; public readonly Fix32 Radius;
        public SpawnReservation(FixVec2 position, Fix32 radius) { Position = position; Radius = radius; }
    }
}
}
