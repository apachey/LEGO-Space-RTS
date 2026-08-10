using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public enum PlacementFailure : byte
{
    None = 0,
    UnknownBuilding = 1,
    InvalidOrientation = 2,
    NoEligibleBuilder = 3,
    MissingPrerequisite = 4,
    OutsideMap = 5,
    FootprintOccupied = 6,
    ResourceAccessBlocked = 7,
    NonBuildableTerrain = 8,
    TerrainFeature = 9,
    NoLegalProductionExit = 10,
    InsufficientOre = 11,
    NoEnergyDomain = 12,
    InsufficientEnergy = 13
}

public readonly struct PlacementValidation
{
    public readonly PlacementFailure Failure;
    public readonly EntityId Builder;
    public readonly EntityId FundingBank;
    public readonly EntityId EnergyDomainRoot;
    public bool IsValid => Failure == PlacementFailure.None;
    public PlacementValidation(PlacementFailure failure, EntityId builder = default, EntityId fundingBank = default, EntityId energyDomainRoot = default)
    { Failure = failure; Builder = builder; FundingBank = fundingBank; EnergyDomainRoot = energyDomainRoot; }
}

public static class ConstructionPlacement
{
    private static readonly ContentId HqType = StableId.FromKey("building.rock_raiders.hq");

    public static PlacementValidation Validate(SimulationWorld world, byte playerSlot, IReadOnlyList<EntityId> builders,
        ContentId buildingType, short anchorX, short anchorY, byte orientation)
    {
        if (!world.Content.TryGetBuilding(buildingType, out BuildingDefinition definition)) return new PlacementValidation(PlacementFailure.UnknownBuilding);
        if (orientation > 3 || (!definition.Rotatable && orientation != 0)) return new PlacementValidation(PlacementFailure.InvalidOrientation);
        EntityId builder = FindEligibleBuilder(world, playerSlot, builders);
        if (builder == EntityId.None) return new PlacementValidation(PlacementFailure.NoEligibleBuilder);
        if (buildingType != HqType && !HasCompletedHq(world, playerSlot)) return new PlacementValidation(PlacementFailure.MissingPrerequisite);
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        if (anchorX < 0 || anchorY < 0 || anchorX + width > MapGrid.BuildWidth || anchorY + height > MapGrid.BuildHeight)
            return new PlacementValidation(PlacementFailure.OutsideMap);

        PlacementFailure occupancy = ValidateFootprintOccupancy(world, definition, anchorX, anchorY, orientation);
        if (occupancy != PlacementFailure.None) return new PlacementValidation(occupancy);
        PlacementFailure terrain = ValidateFootprintTerrain(world.Map, definition, anchorX, anchorY, orientation);
        if (terrain != PlacementFailure.None) return new PlacementValidation(terrain);
        if (definition.ProductionExitWidth > 0 && !ValidateProductionExit(world, definition, anchorX, anchorY, orientation))
            return new PlacementValidation(PlacementFailure.NoLegalProductionExit);

        EntityId bank = FindFundingBank(world, playerSlot, definition.OreCost, SiteCenter(anchorX, anchorY, width, height));
        if (bank == EntityId.None) return new PlacementValidation(PlacementFailure.InsufficientOre);
        if (!EnergyDomainSystem.TryResolveForEntity(world, bank, playerSlot, out EntityId energyDomain)) return new PlacementValidation(PlacementFailure.NoEnergyDomain);
        if (!EnergyDomainSystem.CanSpend(world, energyDomain, definition.EnergyCost)) return new PlacementValidation(PlacementFailure.InsufficientEnergy);
        return new PlacementValidation(PlacementFailure.None, builder, bank, energyDomain);
    }

    public static bool TryPlace(SimulationWorld world, byte playerSlot, IReadOnlyList<EntityId> builders, ContentId buildingType,
        short anchorX, short anchorY, byte orientation, out EntityId site, out PlacementFailure failure, bool queueBuilder = false)
    {
        PlacementValidation validation = Validate(world, playerSlot, builders, buildingType, anchorX, anchorY, orientation);
        failure = validation.Failure; site = EntityId.None;
        if (!validation.IsValid || !world.Content.TryGetBuilding(buildingType, out BuildingDefinition definition)) return false;
        if (!EnergyDomainSystem.TrySpend(world, validation.EnergyDomainRoot, definition.EnergyCost)) { failure = PlacementFailure.InsufficientEnergy; return false; }
        ref ResourceBank bank = ref world.Entities.ResourceBank.Get(validation.FundingBank);
        bank.ProcessedAmount = checked(bank.ProcessedAmount - definition.OreCost);
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        Building building = new()
        {
            Type = buildingType, AnchorX = anchorX, AnchorY = anchorY, Orientation = orientation,
            FootprintWidth = width, FootprintHeight = height, State = BuildingState.ConstructionSite
        };
        site = world.Entities.Create();
        world.Entities.Ownership.Set(site, new Ownership { PlayerSlot = playerSlot });
        world.Entities.Transform.Set(site, new SimTransform { Position = SiteCenter(anchorX, anchorY, width, height), Orientation = new Angle16((ushort)(orientation * 16384)) });
        world.Entities.Selectable.Set(site, new Selectable { IsSelectable = true, ContentType = buildingType, Kind = SelectableKind.Building });
        world.Entities.Building.Set(site, building);
        world.Entities.EnergyDomainMember.Set(site, new EnergyDomainMember { DomainRoot = validation.EnergyDomainRoot });
        world.Entities.ConstructionSite.Set(site, new ConstructionSite
        {
            AssignedBuilder = EntityId.None, FundingBank = validation.FundingBank, ReservedOre = definition.OreCost, ConsumedOre = 0,
            RequiredEnergy = definition.EnergyCost, ReservedEnergy = definition.EnergyCost, ConsumedEnergy = 0, EnergyDomainRoot = validation.EnergyDomainRoot,
            RequiredTicks = definition.BuildTicks, ProgressTicks = 0
        });
        world.SetConstructionOccupied(building, true);
        ConstructionSystem.AssignBuilder(world, validation.Builder, site, queueBuilder);
        return true;
    }

    public static bool TryCancelUnstarted(SimulationWorld world, byte playerSlot, EntityId site)
    {
        if (!world.Entities.ConstructionSite.TryGet(site, out ConstructionSite construction) || construction.ProgressTicks != 0) return false;
        return TryCancel(world, playerSlot, site);
    }

    public static bool TryCancel(SimulationWorld world, byte playerSlot, EntityId site)
    {
        if (!world.Entities.ConstructionSite.TryGet(site, out ConstructionSite construction) ||
            !world.Entities.Building.TryGet(site, out Building building) ||
            !world.Entities.Ownership.TryGet(site, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
            !world.Entities.ResourceBank.Has(construction.FundingBank)) return false;
        ref ResourceBank bank = ref world.Entities.ResourceBank.Get(construction.FundingBank);
        int consumedRefund = construction.ProgressTicks == 0 ? construction.ConsumedOre : construction.ConsumedOre / 2;
        bank.ProcessedAmount = checked(bank.ProcessedAmount + checked(construction.ReservedOre + consumedRefund));
        int consumedEnergyRefund = construction.ProgressTicks == 0 ? construction.ConsumedEnergy : construction.ConsumedEnergy / 2;
        EnergyDomainSystem.Refund(world, construction.EnergyDomainRoot, checked(construction.ReservedEnergy + consumedEnergyRefund));
        world.Entities.ConstructionSite.Remove(site);
        ConstructionSystem.ReleaseSiteAssignments(world, site);
        world.SetConstructionOccupied(building, false);
        return world.Entities.Destroy(site);
    }

    public static IntRect GetProductionExit(BuildingDefinition definition, short anchorX, short anchorY, byte orientation)
    {
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        short x, y, exitWidth, exitHeight;
        if ((orientation & 1) == 0) { exitWidth = definition.ProductionExitWidth; exitHeight = definition.ProductionExitDepth; }
        else { exitWidth = definition.ProductionExitDepth; exitHeight = definition.ProductionExitWidth; }
        switch (orientation & 3)
        {
            case 0: x = (short)(anchorX + (width - exitWidth) / 2); y = (short)(anchorY + height); break;
            case 1: x = (short)(anchorX + width); y = (short)(anchorY + (height - exitHeight) / 2); break;
            case 2: x = (short)(anchorX + (width - exitWidth) / 2); y = (short)(anchorY - exitHeight); break;
            default: x = (short)(anchorX - exitWidth); y = (short)(anchorY + (height - exitHeight) / 2); break;
        }
        return new IntRect(x, y, exitWidth, exitHeight);
    }

    private static EntityId FindEligibleBuilder(SimulationWorld world, byte playerSlot, IReadOnlyList<EntityId> builders)
    {
        EntityId best = EntityId.None;
        for (int i = 0; i < builders.Count; i++)
        {
            EntityId id = builders[i];
            if (!world.Entities.Exists(id) || !world.Entities.Builder.Has(id) ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot) continue;
            if (best == EntityId.None || id.Value < best.Value) best = id;
        }
        return best;
    }

    private static bool HasCompletedHq(SimulationWorld world, byte playerSlot)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building building) && building.Type == HqType && building.State == BuildingState.Completed &&
                world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == playerSlot) return true;
        }
        return false;
    }

    private static PlacementFailure ValidateFootprintOccupancy(SimulationWorld world, BuildingDefinition candidate, short anchorX, short anchorY, byte orientation)
    {
        IntRect candidateBounds = new(anchorX, anchorY, candidate.RotatedWidth(orientation), candidate.RotatedHeight(orientation));
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building existing))
            {
                if (FootprintsOverlap(world, candidate, anchorX, anchorY, orientation, existing)) return PlacementFailure.FootprintOccupied;
                if (world.Content.TryGetBuilding(existing.Type, out BuildingDefinition existingDefinition) && existingDefinition.ProductionExitWidth > 0 &&
                    RectanglesOverlap(candidateBounds, GetProductionExit(existingDefinition, existing.AnchorX, existing.AnchorY, existing.Orientation)))
                    return PlacementFailure.NoLegalProductionExit;
            }
            if (!world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            if (world.Entities.ResourceNode.Has(id) && Contains(candidate, anchorX, anchorY, orientation, transform.Position))
                return PlacementFailure.ResourceAccessBlocked;
            if (world.Entities.Navigation.TryGet(id, out NavigationAgent navigation) && CircleOverlaps(candidate, anchorX, anchorY, orientation, transform.Position, FootprintRules.CollisionRadiusBuild(navigation.Footprint)))
                return PlacementFailure.FootprintOccupied;
        }
        return PlacementFailure.None;
    }

    private static PlacementFailure ValidateFootprintTerrain(MapGrid map, BuildingDefinition definition, short anchorX, short anchorY, byte orientation)
    {
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        int? elevation = null;
        for (byte y = 0; y < height; y++)
        for (byte x = 0; x < width; x++)
        {
            if (!definition.Occupies(x, y, orientation)) continue;
            for (int ny = (anchorY + y) * MapGrid.NavPerBuild; ny < (anchorY + y + 1) * MapGrid.NavPerBuild; ny++)
            for (int nx = (anchorX + x) * MapGrid.NavPerBuild; nx < (anchorX + x + 1) * MapGrid.NavPerBuild; nx++)
            {
                MapCellFlags flags = map.GetFlags(nx, ny);
                if ((flags & MapCellFlags.Excavatable) != 0) return PlacementFailure.TerrainFeature;
                if ((flags & MapCellFlags.Buildable) == 0 || (flags & MapCellFlags.Impassable) != 0) return PlacementFailure.NonBuildableTerrain;
                int currentElevation = map.GetElevation(nx, ny);
                if (!elevation.HasValue) elevation = currentElevation;
                else if (elevation.Value != currentElevation) return PlacementFailure.NonBuildableTerrain;
            }
        }
        return PlacementFailure.None;
    }

    private static bool ValidateProductionExit(SimulationWorld world, BuildingDefinition definition, short anchorX, short anchorY, byte orientation)
    {
        IntRect exit = GetProductionExit(definition, anchorX, anchorY, orientation);
        if (exit.X < 0 || exit.Y < 0 || exit.X + exit.Width > MapGrid.BuildWidth || exit.Y + exit.Height > MapGrid.BuildHeight) return false;
        for (int y = exit.Y; y < exit.Y + exit.Height; y++)
        for (int x = exit.X; x < exit.X + exit.Width; x++)
        {
            for (int ny = y * MapGrid.NavPerBuild; ny < (y + 1) * MapGrid.NavPerBuild; ny++)
            for (int nx = x * MapGrid.NavPerBuild; nx < (x + 1) * MapGrid.NavPerBuild; nx++)
                if ((world.Map.GetFlags(nx, ny) & MapCellFlags.Impassable) != 0) return false;
            if (!world.Pathfinder.IsPassable(MapGrid.BuildToNav(new FixVec2(Fix32.FromRatio(x * 2 + 1, 2), Fix32.FromRatio(y * 2 + 1, 2))), definition.ProductionExitFootprint)) return false;
        }
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building building) && RectanglesOverlap(exit, new IntRect(building.AnchorX, building.AnchorY, building.FootprintWidth, building.FootprintHeight))) return false;
            if (world.Entities.Transform.TryGet(id, out SimTransform transform) && exit.Contains(transform.Position.X.FloorToInt(), transform.Position.Y.FloorToInt())) return false;
        }
        return true;
    }

    private static EntityId FindFundingBank(SimulationWorld world, byte playerSlot, int oreCost, FixVec2 siteCenter)
    {
        EntityId best = EntityId.None; Fix32 bestDistance = Fix32.MaxValue;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) || bank.Type != ResourceType.Ore || bank.ProcessedAmount < oreCost ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 distance = FixVec2.Distance(siteCenter, transform.Position);
            if (best == EntityId.None || distance < bestDistance || (distance == bestDistance && id.Value < best.Value)) { best = id; bestDistance = distance; }
        }
        return best;
    }

    private static bool FootprintsOverlap(SimulationWorld world, BuildingDefinition candidate, short anchorX, short anchorY, byte orientation, Building existing)
    {
        if (!world.Content.TryGetBuilding(existing.Type, out BuildingDefinition existingDefinition)) return true;
        byte width = candidate.RotatedWidth(orientation), height = candidate.RotatedHeight(orientation);
        if (!RectanglesOverlap(new IntRect(anchorX, anchorY, width, height), new IntRect(existing.AnchorX, existing.AnchorY, existing.FootprintWidth, existing.FootprintHeight))) return false;
        for (byte y = 0; y < height; y++) for (byte x = 0; x < width; x++)
        {
            if (!candidate.Occupies(x, y, orientation)) continue;
            int ex = anchorX + x - existing.AnchorX, ey = anchorY + y - existing.AnchorY;
            if (ex >= 0 && ey >= 0 && ex < existing.FootprintWidth && ey < existing.FootprintHeight && existingDefinition.Occupies((byte)ex, (byte)ey, existing.Orientation)) return true;
        }
        return false;
    }

    private static bool Contains(BuildingDefinition definition, short anchorX, short anchorY, byte orientation, FixVec2 position)
    {
        int x = position.X.FloorToInt() - anchorX, y = position.Y.FloorToInt() - anchorY;
        return x >= 0 && y >= 0 && x < definition.RotatedWidth(orientation) && y < definition.RotatedHeight(orientation) && definition.Occupies((byte)x, (byte)y, orientation);
    }

    private static bool CircleOverlaps(BuildingDefinition definition, short anchorX, short anchorY, byte orientation, FixVec2 center, Fix32 radius)
    {
        byte width = definition.RotatedWidth(orientation), height = definition.RotatedHeight(orientation);
        Fix32 minX = Fix32.FromInt(anchorX), maxX = Fix32.FromInt(anchorX + width), minY = Fix32.FromInt(anchorY), maxY = Fix32.FromInt(anchorY + height);
        Fix32 nearestX = center.X < minX ? minX : center.X > maxX ? maxX : center.X;
        Fix32 nearestY = center.Y < minY ? minY : center.Y > maxY ? maxY : center.Y;
        Fix32 dx = center.X - nearestX, dy = center.Y - nearestY;
        return dx * dx + dy * dy < radius * radius;
    }

    private static bool RectanglesOverlap(IntRect a, IntRect b)
        => a.X < b.X + b.Width && a.X + a.Width > b.X && a.Y < b.Y + b.Height && a.Y + a.Height > b.Y;

    private static FixVec2 SiteCenter(short anchorX, short anchorY, byte width, byte height)
        => new(Fix32.FromRatio(anchorX * 2 + width, 2), Fix32.FromRatio(anchorY * 2 + height, 2));
}
}
