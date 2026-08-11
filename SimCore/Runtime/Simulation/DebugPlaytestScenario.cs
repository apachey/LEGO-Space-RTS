using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Command-driven fixtures that remove setup grind from human prototype playtests.</summary>
public static class DebugPlaytestScenario
{
    public const string ConstructionBuildingKey = "building.rock_raiders.ore_processing_plant";
    public const string DestructionBuildingKey = "building.rock_raiders.ore_processing_plant";
    public const string CrewKey = "unit.rock_raiders.crew";
    public const string HoverScoutKey = "unit.rock_raiders.hover_scout";
    public const string ChromeCrusherKey = "unit.rock_raiders.chrome_crusher";
    public static readonly FixVec2 DestructionArenaCenter = FixVec2.FromInts(103, 103);
    public static readonly FixVec2 RepairArenaCenter = FixVec2.FromInts(72, 96);

    private static readonly (short X, short Y)[] DestructionBuildingAnchors =
    {
        (100, 100), (118, 100), (100, 118), (118, 118)
    };

    public static EntityId PrepareConstruction(SimulationWorld world, byte playerSlot)
    {
        EntityId existing = FindNewestConstructionSite(world, playerSlot);
        if (existing != EntityId.None) return existing;

        List<EntityId> builders = new();
        EntityId fundingBank = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot) continue;
            if (world.Entities.Builder.Has(id)) builders.Add(id);
            if (fundingBank == EntityId.None && world.Entities.ResourceBank.Has(id)) fundingBank = id;
        }
        if (builders.Count == 0 || fundingBank == EntityId.None) return EntityId.None;

        ref ResourceBank bank = ref world.Entities.ResourceBank.Get(fundingBank);
        bank.ProcessedAmount = Math.Max(bank.ProcessedAmount, 5_000);
        if (EnergyDomainSystem.TryGetPlayerDomain(world, playerSlot, out EntityId root))
        {
            ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
            domain.Reserve = Fix32.Min(domain.ReserveCapacity, Fix32.Max(domain.Reserve, Fix32.FromInt(100)));
        }

        ContentId buildingType = StableId.FromKey(ConstructionBuildingKey);
        const int preferredAnchorX = 21;
        const int preferredAnchorY = 71;
        for (int radius = 0; radius <= 28; radius++)
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;
            short x = checked((short)(preferredAnchorX + dx));
            short y = checked((short)(preferredAnchorY + dy));
            if (!ConstructionPlacement.TryPlace(world, playerSlot, builders, buildingType, x, y, 0, out EntityId site, out _)) continue;
            ref ConstructionSite construction = ref world.Entities.ConstructionSite.Get(site);
            construction.ProgressTicks = checked((ushort)Math.Max(1, construction.RequiredTicks * 10 / 100));
            return site;
        }
        return EntityId.None;
    }

    public static EntityId PrepareDestruction(SimulationWorld world, byte playerSlot)
    {
        if (playerSlot >= world.PlayerCount) return EntityId.None;
        byte enemyPlayer = checked((byte)((playerSlot + 1) % world.PlayerCount));
        EntityId building = FindArenaBuilding(world, enemyPlayer);
        if (building == EntityId.None)
        {
            for (int offset = 0; offset < DestructionBuildingAnchors.Length; offset++)
            {
                (short x, short y) = DestructionBuildingAnchors[offset];
                if (!CanPlaceDebugBuilding(world, DestructionBuildingKey, x, y)) continue;
                building = SpawnDebugBuilding(world, enemyPlayer, DestructionBuildingKey, x, y);
                break;
            }
        }
        if (building == EntityId.None || !world.Entities.Transform.TryGet(building, out SimTransform buildingTransform)) return EntityId.None;

        FixVec2 center = buildingTransform.Position;
        EntityId friendlyChrome = EnsureUnit(world, playerSlot, ChromeCrusherKey, PreparedPosition(world, ChromeCrusherKey, center + FixVec2.FromInts(-10, 0)));
        EnsureUnit(world, playerSlot, HoverScoutKey, PreparedPosition(world, HoverScoutKey, center + FixVec2.FromInts(-8, -8)));
        EntityId enemyCrew = EnsureUnit(world, enemyPlayer, CrewKey, PreparedPosition(world, CrewKey, center + FixVec2.FromInts(8, -8)));
        EntityId enemyChrome = EnsureUnit(world, enemyPlayer, ChromeCrusherKey, PreparedPosition(world, ChromeCrusherKey, center + FixVec2.FromInts(10, 0)));
        SpawnInvisibleObserver(world, playerSlot, center);
        SpawnInvisibleObserver(world, playerSlot, world.Entities.Transform.Get(enemyCrew).Position);
        SpawnInvisibleObserver(world, playerSlot, world.Entities.Transform.Get(enemyChrome).Position);

        SetTestHealth(world, enemyCrew, 12);
        SetTestHealth(world, enemyChrome, 160);
        SetTestHealth(world, building, 48, useAsMaximum: true);
        OperationsCapacitySystem.Recalculate(world);
        world.Spatial.Rebuild(world.Entities);
        new VisionSystem().Step(world);
        return friendlyChrome;
    }

    public static EntityId PrepareRepair(SimulationWorld world, byte playerSlot)
    {
        EntityId crew = EnsureUnit(world, playerSlot, CrewKey, PreparedPosition(world, CrewKey, RepairArenaCenter + FixVec2.FromInts(-4, 0)));
        EntityId hover = EnsureUnit(world, playerSlot, HoverScoutKey, PreparedPosition(world, HoverScoutKey, RepairArenaCenter));
        if (world.Entities.Health.Has(hover))
        {
            ref Health health = ref world.Entities.Health.Get(hover);
            health.Current = Fix32.FromInt(40);
            health.LastDamageTick = -1;
        }
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot)
                world.Entities.ResourceBank.Get(id).ProcessedAmount = System.Math.Max(500, world.Entities.ResourceBank.Get(id).ProcessedAmount);
        }
        if (EnergyDomainSystem.TryGetPlayerDomain(world, playerSlot, out EntityId root))
            world.Entities.EnergyDomain.Get(root).Reserve = Fix32.Min(world.Entities.EnergyDomain.Get(root).ReserveCapacity, Fix32.FromInt(100));
        world.Spatial.Rebuild(world.Entities);
        new VisionSystem().Step(world);
        return crew;
    }

    private static void SpawnInvisibleObserver(SimulationWorld world, byte ownerSlot, FixVec2 position)
    {
        EntityId observer = world.Entities.Create();
        world.Entities.Ownership.Set(observer, new Ownership { PlayerSlot = ownerSlot });
        world.Entities.Transform.Set(observer, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Vision.Set(observer, new Vision { RadiusBuildCells = 12, LastFogX = -1, LastFogY = -1 });
    }

    private static EntityId FindNewestConstructionSite(SimulationWorld world, byte playerSlot)
    {
        EntityId result = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.ConstructionSite.Has(id) && world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot)
                result = id;
        }
        return result;
    }

    private static EntityId FindArenaBuilding(SimulationWorld world, byte ownerSlot)
    {
        ContentId type = StableId.FromKey(DestructionBuildingKey);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building building) && building.Type == type &&
                world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == ownerSlot &&
                building.AnchorX >= 96 && building.AnchorY >= 96) return id;
        }
        return EntityId.None;
    }

    private static EntityId EnsureUnit(SimulationWorld world, byte ownerSlot, string contentKey, FixVec2 position)
    {
        ContentId type = StableId.FromKey(contentKey);
        EntityId unit = EntityId.None;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId candidate = alive[i];
            if (world.Entities.Selectable.TryGet(candidate, out Selectable selectable) && selectable.ContentType == type &&
                world.Entities.Ownership.TryGet(candidate, out Ownership owner) && owner.PlayerSlot == ownerSlot)
            { unit = candidate; break; }
        }
        if (unit == EntityId.None) unit = ScenarioFactory.SpawnProducedUnit(world, ownerSlot, type, position);
        ResetUnit(world, unit, position);
        return unit;
    }

    private static FixVec2 PreparedPosition(SimulationWorld world, string contentKey, FixVec2 desired)
    {
        if (!world.Content.TryGetEntity(contentKey, out PrototypeEntityDefinition definition)) return desired;
        return FormationPlanner.ResolvePassableSlot(world, desired, definition.Footprint);
    }

    private static void ResetUnit(SimulationWorld world, EntityId unit, FixVec2 position)
    {
        world.RemoveRuntimeState(unit);
        ref SimTransform transform = ref world.Entities.Transform.Get(unit);
        transform.Position = position;
        transform.Orientation = Angle16.Zero;
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(unit);
        navigation.Target = position;
        navigation.HasTarget = false;
        navigation.PathDirty = false;
        navigation.Formation = default;
        navigation.PathTopologyVersion = world.Map.TopologyVersion;
        ref Movement movement = ref world.Entities.Movement.Get(unit);
        movement.CurrentSpeed = Fix32.Zero;
        movement.CurrentVelocity = FixVec2.Zero;
        movement.DesiredMovement = FixVec2.Zero;
        movement.State = MovementState.Idle;
        movement.PathIndex = 0;
        movement.LastPosition = position;
        movement.StuckTicks = 0;
        movement.CompressionTicks = 0;
        world.GetQueue(unit).Clear();
        if (world.Entities.Targeting.Has(unit))
        {
            ref Targeting targeting = ref world.Entities.Targeting.Get(unit);
            targeting.CurrentTarget = EntityId.None;
            targeting.SelectionKind = TargetSelectionKind.None;
            targeting.HasPursuitOrigin = false;
            targeting.HasApproachSlot = false;
            targeting.HasCombatMove = false;
        }
        if (world.Entities.Weapon.Has(unit))
        {
            ref WeaponState weapon = ref world.Entities.Weapon.Get(unit);
            weapon.CooldownRemainingTicks = 0;
        }
        if (world.Entities.Health.Has(unit))
        {
            ref Health health = ref world.Entities.Health.Get(unit);
            health.Current = health.Maximum;
            health.LastDamageTick = -1;
        }
        if (world.Entities.Vision.Has(unit))
        {
            ref Vision vision = ref world.Entities.Vision.Get(unit);
            vision.LastFogX = -1;
            vision.LastFogY = -1;
        }
    }

    private static void SetTestHealth(SimulationWorld world, EntityId id, int hitPoints, bool useAsMaximum = false)
    {
        if (!world.Entities.Health.Has(id)) return;
        ref Health health = ref world.Entities.Health.Get(id);
        if (useAsMaximum) health.Maximum = Fix32.FromInt(hitPoints);
        health.Current = Fix32.Min(health.Maximum, Fix32.FromInt(hitPoints));
        health.LastDamageTick = world.Tick.Value;
    }

    private static bool CanPlaceDebugBuilding(SimulationWorld world, string buildingKey, short anchorX, short anchorY)
    {
        if (!world.Content.TryGetBuilding(buildingKey, out BuildingDefinition definition)) return false;
        byte width = definition.FootprintWidth, height = definition.FootprintHeight;
        if (anchorX < 0 || anchorY < 0 || anchorX + width > MapGrid.BuildWidth || anchorY + height > MapGrid.BuildHeight) return false;
        for (int y = anchorY * MapGrid.NavPerBuild; y < (anchorY + height) * MapGrid.NavPerBuild; y++)
        for (int x = anchorX * MapGrid.NavPerBuild; x < (anchorX + width) * MapGrid.NavPerBuild; x++)
        {
            MapCellFlags flags = world.Map.GetFlags(x, y);
            if ((flags & MapCellFlags.Buildable) == 0 || (flags & MapCellFlags.Impassable) != 0) return false;
        }
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 nearestX = Fix32.Max(Fix32.FromInt(anchorX), Fix32.Min(transform.Position.X, Fix32.FromInt(anchorX + width)));
            Fix32 nearestY = Fix32.Max(Fix32.FromInt(anchorY), Fix32.Min(transform.Position.Y, Fix32.FromInt(anchorY + height)));
            Fix32 radius = world.Entities.Navigation.TryGet(id, out NavigationAgent navigation)
                ? FootprintRules.CollisionRadiusBuild(navigation.Footprint) : Fix32.Zero;
            if (FixVec2.Distance(transform.Position, new FixVec2(nearestX, nearestY)) < radius) return false;
        }
        return true;
    }

    private static EntityId SpawnDebugBuilding(SimulationWorld world, byte ownerSlot, string buildingKey, short anchorX, short anchorY)
    {
        if (!world.Content.TryGetBuilding(buildingKey, out BuildingDefinition definition) ||
            !world.Content.TryGetEntity(buildingKey, out PrototypeEntityDefinition entityDefinition)) return EntityId.None;
        ContentId type = StableId.FromKey(buildingKey);
        Building building = new()
        {
            Type = type, AnchorX = anchorX, AnchorY = anchorY, Orientation = 0,
            FootprintWidth = definition.FootprintWidth, FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed
        };
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = ownerSlot });
        world.Entities.Transform.Set(id, new SimTransform
        {
            Position = new FixVec2(Fix32.FromRatio(anchorX * 2 + definition.FootprintWidth, 2), Fix32.FromRatio(anchorY * 2 + definition.FootprintHeight, 2)),
            Orientation = Angle16.Zero
        });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, building);
        ScenarioFactory.AddCombatComponents(world, id, entityDefinition);
        world.SetConstructionOccupied(building, true);
        return id;
    }
}
}
