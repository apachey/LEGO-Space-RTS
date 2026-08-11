using System;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct PrototypeMovementProfile
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly Fix32 MaxSpeed;
    public readonly Fix32 Acceleration;
    public readonly Fix32 Deceleration;
    public readonly ushort TurnRatePerTick;
    public readonly ReversePolicy ReversePolicy;
    public readonly MovementLayer Layer;
    public PrototypeMovementProfile(string stableKey, Fix32 maxSpeed, Fix32 acceleration, Fix32 deceleration, ushort turnRatePerTick, ReversePolicy reversePolicy, MovementLayer layer)
    {
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey); MaxSpeed = maxSpeed; Acceleration=acceleration; Deceleration=deceleration;
        TurnRatePerTick=turnRatePerTick; ReversePolicy=reversePolicy; Layer = layer;
    }
    public PrototypeMovementProfile(string stableKey, Fix32 maxSpeed, MovementLayer layer)
        : this(stableKey,maxSpeed,Fix32.FromInt(2),Fix32.FromRatio(5,2),865,ReversePolicy.Reduced,layer) { }
}

public readonly struct PrototypeCombatProfile
{
    public readonly bool IsTargetable;
    public readonly CombatTargetClass TargetClass;
    public readonly CombatTargetLayer TargetLayer;
    public readonly CombatTargetFlags TargetFlags;
    public readonly ushort MaximumHitPoints;
    public readonly byte ArmorRating;
    public readonly TargetPriorityProfile PriorityProfile;
    public readonly TargetLayerMask LegalTargetLayers;
    public readonly TargetClassMask LegalTargetClasses;
    public readonly Fix32 AcquisitionRadius;
    public readonly ContentId WeaponProfile;
    public bool CanAcquireTargets => LegalTargetLayers != TargetLayerMask.None && LegalTargetClasses != TargetClassMask.None && AcquisitionRadius > Fix32.Zero;

    public PrototypeCombatProfile(CombatTargetClass targetClass, CombatTargetLayer targetLayer, CombatTargetFlags targetFlags,
        ushort maximumHitPoints, byte armorRating)
        : this(targetClass, targetLayer, targetFlags, maximumHitPoints, armorRating, TargetPriorityProfile.Support, TargetLayerMask.None, TargetClassMask.None, Fix32.Zero) { }

    public PrototypeCombatProfile(CombatTargetClass targetClass, CombatTargetLayer targetLayer, CombatTargetFlags targetFlags, ushort maximumHitPoints, byte armorRating,
        TargetPriorityProfile priorityProfile, TargetLayerMask legalTargetLayers, TargetClassMask legalTargetClasses, Fix32 acquisitionRadius,
        ContentId weaponProfile = default)
    {
        if (targetClass < CombatTargetClass.Personnel || targetClass > CombatTargetClass.FortifiedStructure) throw new ArgumentOutOfRangeException(nameof(targetClass));
        if (targetLayer < CombatTargetLayer.Ground || targetLayer > CombatTargetLayer.TrueAir) throw new ArgumentOutOfRangeException(nameof(targetLayer));
        if ((targetFlags & ~(CombatTargetFlags.CombatThreat | CombatTargetFlags.Worker | CombatTargetFlags.Transport | CombatTargetFlags.Support | CombatTargetFlags.DefensiveStructure | CombatTargetFlags.Production | CombatTargetFlags.EconomicInfrastructure | CombatTargetFlags.Command)) != 0) throw new ArgumentOutOfRangeException(nameof(targetFlags));
        if (maximumHitPoints == 0 || armorRating > 5) throw new ArgumentOutOfRangeException(nameof(maximumHitPoints));
        if ((legalTargetLayers & ~TargetLayerMask.All) != 0 || (legalTargetClasses & ~TargetClassMask.All) != 0) throw new ArgumentOutOfRangeException(nameof(legalTargetLayers));
        bool hasTargeting = legalTargetLayers != TargetLayerMask.None || legalTargetClasses != TargetClassMask.None || acquisitionRadius != Fix32.Zero;
        if (hasTargeting && (legalTargetLayers == TargetLayerMask.None || legalTargetClasses == TargetClassMask.None || acquisitionRadius <= Fix32.Zero)) throw new ArgumentException("Targeting metadata must provide legal layers, legal classes and a positive acquisition radius together.");
        IsTargetable = true; TargetClass = targetClass; TargetLayer = targetLayer; TargetFlags = targetFlags;
        MaximumHitPoints = maximumHitPoints; ArmorRating = armorRating;
        PriorityProfile = priorityProfile; LegalTargetLayers = legalTargetLayers; LegalTargetClasses = legalTargetClasses; AcquisitionRadius = acquisitionRadius;
        WeaponProfile = weaponProfile;
    }
}

public readonly struct WeaponDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly TargetLayerMask LegalTargetLayers;
    public readonly TargetClassMask LegalTargetClasses;
    public readonly TargetPriorityProfile PriorityProfile;
    public readonly ushort BaseDamage;
    public readonly DamageType DamageType;
    public readonly ushort CooldownTicks;
    public readonly Fix32 Range;
    public readonly Fix32 MinimumRange;
    public readonly WeaponDeliveryKind DeliveryKind;
    public readonly Fix32 ProjectileSpeed;
    public readonly bool RequiresLineOfSight;

    public WeaponDefinition(string stableKey, TargetLayerMask legalTargetLayers, TargetClassMask legalTargetClasses,
        TargetPriorityProfile priorityProfile, ushort baseDamage, DamageType damageType, ushort cooldownTicks,
        Fix32 range, Fix32 minimumRange, WeaponDeliveryKind deliveryKind, Fix32 projectileSpeed, bool requiresLineOfSight)
    {
        if (string.IsNullOrWhiteSpace(stableKey)) throw new ArgumentNullException(nameof(stableKey));
        if (legalTargetLayers == TargetLayerMask.None || (legalTargetLayers & ~TargetLayerMask.All) != 0) throw new ArgumentOutOfRangeException(nameof(legalTargetLayers));
        if (legalTargetClasses == TargetClassMask.None || (legalTargetClasses & ~TargetClassMask.All) != 0) throw new ArgumentOutOfRangeException(nameof(legalTargetClasses));
        if (priorityProfile < TargetPriorityProfile.AntiLight || priorityProfile > TargetPriorityProfile.Control) throw new ArgumentOutOfRangeException(nameof(priorityProfile));
        if (damageType < DamageType.Light || damageType > DamageType.Control) throw new ArgumentOutOfRangeException(nameof(damageType));
        if (deliveryKind < WeaponDeliveryKind.Projectile || deliveryKind > WeaponDeliveryKind.Contact) throw new ArgumentOutOfRangeException(nameof(deliveryKind));
        if (baseDamage == 0 || cooldownTicks == 0 || range <= Fix32.Zero || minimumRange < Fix32.Zero || minimumRange >= range) throw new ArgumentOutOfRangeException(nameof(range));
        if ((deliveryKind == WeaponDeliveryKind.Projectile && projectileSpeed <= Fix32.Zero) ||
            (deliveryKind != WeaponDeliveryKind.Projectile && projectileSpeed != Fix32.Zero)) throw new ArgumentOutOfRangeException(nameof(projectileSpeed));
        StableKey = stableKey; Id = StableId.FromKey(stableKey); LegalTargetLayers = legalTargetLayers; LegalTargetClasses = legalTargetClasses;
        PriorityProfile = priorityProfile; BaseDamage = baseDamage; DamageType = damageType; CooldownTicks = cooldownTicks;
        Range = range; MinimumRange = minimumRange; DeliveryKind = deliveryKind; ProjectileSpeed = projectileSpeed; RequiresLineOfSight = requiresLineOfSight;
    }
}

public readonly struct PrototypeEntityDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly string FactionKey;
    public readonly string SourceClassification;
    public readonly string MovementProfileKey;
    public readonly FootprintClass Footprint;
    public readonly SelectableKind SelectableKind;
    public readonly byte VisionRadius;
    public readonly string ViewProfileKey;
    public readonly ushort OreTicksPerUnit;
    public readonly byte OreCarryCapacity;
    public readonly byte OperationsCapacity;
    public readonly PrototypeCombatProfile Combat;

    public PrototypeEntityDefinition(string stableKey, string factionKey, string sourceClassification, string movementProfileKey,
        FootprintClass footprint, SelectableKind selectableKind, byte visionRadius, string viewProfileKey, ushort oreTicksPerUnit = 0,
        byte oreCarryCapacity = 0, byte operationsCapacity = 0, PrototypeCombatProfile combat = default)
    {
        if ((oreTicksPerUnit == 0) != (oreCarryCapacity == 0)) throw new ArgumentException("Worker extraction cadence and carry capacity must both be present or absent.");
        if (oreCarryCapacity > 0 && selectableKind != SelectableKind.Worker) throw new ArgumentException("Only Worker definitions may carry worker harvesting metadata.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey)); Id = StableId.FromKey(stableKey);
        FactionKey = factionKey ?? throw new ArgumentNullException(nameof(factionKey));
        SourceClassification = sourceClassification ?? throw new ArgumentNullException(nameof(sourceClassification));
        MovementProfileKey = movementProfileKey ?? throw new ArgumentNullException(nameof(movementProfileKey));
        Footprint = footprint; SelectableKind = selectableKind; VisionRadius = visionRadius;
        ViewProfileKey = viewProfileKey ?? throw new ArgumentNullException(nameof(viewProfileKey));
        OreTicksPerUnit = oreTicksPerUnit; OreCarryCapacity = oreCarryCapacity; OperationsCapacity = operationsCapacity; Combat = combat;
    }
}

public readonly struct ResourceNodeDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly ResourceType Type;
    public readonly ResourceDepositSize DepositSize;
    public readonly int Capacity;
    public readonly HarvestInteraction HarvestInteraction;
    public readonly ResourceDepletionProfile DepletionProfile;
    public readonly ushort ReducedThresholdBasisPoints;
    public readonly ushort LowThresholdBasisPoints;
    public readonly ushort CriticalThresholdBasisPoints;
    public readonly string ViewProfileKey;

    public ResourceNodeDefinition(string stableKey, ResourceType type, ResourceDepositSize depositSize, int capacity,
        HarvestInteraction harvestInteraction, ResourceDepletionProfile depletionProfile,
        ushort reducedThresholdBasisPoints, ushort lowThresholdBasisPoints, ushort criticalThresholdBasisPoints,
        string viewProfileKey)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (reducedThresholdBasisPoints > 10_000 || lowThresholdBasisPoints > reducedThresholdBasisPoints || criticalThresholdBasisPoints > lowThresholdBasisPoints)
            throw new ArgumentOutOfRangeException(nameof(reducedThresholdBasisPoints), "Resource model-state thresholds must descend within 0..10000 basis points.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey);
        Type = type;
        DepositSize = depositSize;
        Capacity = capacity;
        HarvestInteraction = harvestInteraction;
        DepletionProfile = depletionProfile;
        ReducedThresholdBasisPoints = reducedThresholdBasisPoints;
        LowThresholdBasisPoints = lowThresholdBasisPoints;
        CriticalThresholdBasisPoints = criticalThresholdBasisPoints;
        ViewProfileKey = viewProfileKey ?? throw new ArgumentNullException(nameof(viewProfileKey));
    }
}

public readonly struct BuildingDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly byte FootprintWidth;
    public readonly byte FootprintHeight;
    public readonly ulong FootprintMask;
    public readonly bool Rotatable;
    public readonly ushort OreCost;
    public readonly ushort EnergyCost;
    public readonly ushort BuildTicks;
    public readonly byte ProductionExitWidth;
    public readonly byte ProductionExitDepth;
    public readonly FootprintClass ProductionExitFootprint;
    public readonly byte OperationsCapacityProvided;
    public readonly ushort EnergyGenerationPerSecond;
    public readonly ushort EnergyReserveCapacity;
    public readonly ushort ContinuousEnergyDemandPerSecond;
    public readonly EnergyFunctionalClass EnergyFunctionalClass;

    public BuildingDefinition(string stableKey, byte footprintWidth, byte footprintHeight, ulong footprintMask, bool rotatable,
        ushort oreCost, ushort energyCost, ushort buildTicks, byte productionExitWidth = 0, byte productionExitDepth = 0,
        FootprintClass productionExitFootprint = FootprintClass.Tiny, byte operationsCapacityProvided = 0,
        ushort energyGenerationPerSecond = 0, ushort energyReserveCapacity = 0, ushort continuousEnergyDemandPerSecond = 0,
        EnergyFunctionalClass energyFunctionalClass = EnergyFunctionalClass.StaticDefenseAndNonessential)
    {
        if (footprintWidth == 0 || footprintHeight == 0 || footprintWidth > 8 || footprintHeight > 8) throw new ArgumentOutOfRangeException(nameof(footprintWidth));
        int cells = footprintWidth * footprintHeight;
        ulong allowedMask = cells == 64 ? ulong.MaxValue : (1UL << cells) - 1UL;
        if (footprintMask == 0 || (footprintMask & ~allowedMask) != 0) throw new ArgumentOutOfRangeException(nameof(footprintMask));
        if ((productionExitWidth == 0) != (productionExitDepth == 0)) throw new ArgumentException("Production exit width and depth must both be present or absent.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey);
        FootprintWidth = footprintWidth; FootprintHeight = footprintHeight; FootprintMask = footprintMask; Rotatable = rotatable;
        OreCost = oreCost; EnergyCost = energyCost; BuildTicks = buildTicks;
        ProductionExitWidth = productionExitWidth; ProductionExitDepth = productionExitDepth; ProductionExitFootprint = productionExitFootprint;
        OperationsCapacityProvided = operationsCapacityProvided;
        EnergyGenerationPerSecond = energyGenerationPerSecond; EnergyReserveCapacity = energyReserveCapacity;
        ContinuousEnergyDemandPerSecond = continuousEnergyDemandPerSecond;
        EnergyFunctionalClass = energyFunctionalClass;
    }

    public byte RotatedWidth(byte orientation) => (orientation & 1) == 0 ? FootprintWidth : FootprintHeight;
    public byte RotatedHeight(byte orientation) => (orientation & 1) == 0 ? FootprintHeight : FootprintWidth;

    public bool Occupies(byte x, byte y, byte orientation)
    {
        int sourceX, sourceY;
        switch (orientation & 3)
        {
            case 0: sourceX = x; sourceY = y; break;
            case 1: sourceX = y; sourceY = FootprintHeight - 1 - x; break;
            case 2: sourceX = FootprintWidth - 1 - x; sourceY = FootprintHeight - 1 - y; break;
            default: sourceX = FootprintWidth - 1 - y; sourceY = x; break;
        }
        return (FootprintMask & (1UL << (sourceY * FootprintWidth + sourceX))) != 0;
    }
}

public readonly struct UnitProductionDefinition
{
    public readonly ContentId UnitType;
    public readonly string UnitStableKey;
    public readonly ContentId ProducerType;
    public readonly string ProducerStableKey;
    public readonly ushort OreCost;
    public readonly ushort EnergyCost;
    public readonly byte CrystalCost;
    public readonly byte OperationsCapacity;
    public readonly ushort BuildTicks;

    public UnitProductionDefinition(string unitStableKey, string producerStableKey, ushort oreCost, ushort energyCost,
        byte crystalCost, byte operationsCapacity, ushort buildTicks)
    {
        if (string.IsNullOrWhiteSpace(unitStableKey)) throw new ArgumentNullException(nameof(unitStableKey));
        if (string.IsNullOrWhiteSpace(producerStableKey)) throw new ArgumentNullException(nameof(producerStableKey));
        if (buildTicks == 0 || operationsCapacity == 0) throw new ArgumentOutOfRangeException(nameof(buildTicks));
        UnitStableKey = unitStableKey; UnitType = StableId.FromKey(unitStableKey);
        ProducerStableKey = producerStableKey; ProducerType = StableId.FromKey(producerStableKey);
        OreCost = oreCost; EnergyCost = energyCost; CrystalCost = crystalCost; OperationsCapacity = operationsCapacity; BuildTicks = buildTicks;
    }
}

/// <summary>Immutable prototype gameplay metadata compiled from Content/PrototypeEntities.json.</summary>
public sealed class PrototypeContentCatalog
{
    public PrototypeMovementProfile[] MovementProfiles { get; }
    public PrototypeEntityDefinition[] Entities { get; }
    public ResourceNodeDefinition[] ResourceNodes { get; }
    public BuildingDefinition[] Buildings { get; }
    public UnitProductionDefinition[] Production { get; }
    public WeaponDefinition[] Weapons { get; }
    public ulong ContentHash { get; internal set; }
    public PrototypeContentCatalog(PrototypeMovementProfile[] movementProfiles, PrototypeEntityDefinition[] entities, ResourceNodeDefinition[]? resourceNodes = null, BuildingDefinition[]? buildings = null, UnitProductionDefinition[]? production = null, WeaponDefinition[]? weapons = null)
    {
        MovementProfiles = movementProfiles ?? Array.Empty<PrototypeMovementProfile>();
        Entities = entities ?? Array.Empty<PrototypeEntityDefinition>();
        ResourceNodes = resourceNodes ?? Array.Empty<ResourceNodeDefinition>();
        Buildings = buildings ?? Array.Empty<BuildingDefinition>();
        Production = production ?? Array.Empty<UnitProductionDefinition>();
        Weapons = weapons ?? Array.Empty<WeaponDefinition>();
        for (int i = 0; i < Entities.Length; i++)
        {
            ContentId weaponProfile = Entities[i].Combat.WeaponProfile;
            if (weaponProfile.Value == 0) continue;
            if (!TryGetWeapon(weaponProfile, out WeaponDefinition weapon)) throw new ArgumentException($"Entity {Entities[i].StableKey} references an unknown weapon profile.");
            if (!Entities[i].Combat.CanAcquireTargets || Entities[i].Combat.LegalTargetLayers != weapon.LegalTargetLayers ||
                Entities[i].Combat.LegalTargetClasses != weapon.LegalTargetClasses || Entities[i].Combat.PriorityProfile != weapon.PriorityProfile)
                throw new ArgumentException($"Entity {Entities[i].StableKey} targeting metadata disagrees with weapon {weapon.StableKey}.");
        }
    }

    public bool ContainsEntityKey(string stableKey) => TryGetEntity(stableKey, out _);
    public bool TryGetEntity(string stableKey, out PrototypeEntityDefinition definition)
    {
        for (int i = 0; i < Entities.Length; i++) if (string.Equals(Entities[i].StableKey, stableKey, StringComparison.Ordinal)) { definition = Entities[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetEntity(ContentId id, out PrototypeEntityDefinition definition)
    {
        for (int i = 0; i < Entities.Length; i++) if (Entities[i].Id == id) { definition = Entities[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetMovement(string stableKey, out PrototypeMovementProfile profile)
    {
        for (int i = 0; i < MovementProfiles.Length; i++) if (string.Equals(MovementProfiles[i].StableKey, stableKey, StringComparison.Ordinal)) { profile = MovementProfiles[i]; return true; }
        profile = default; return false;
    }
    public bool ContainsResourceNodeKey(string stableKey) => TryGetResourceNode(stableKey, out _);
    public bool TryGetResourceNode(string stableKey, out ResourceNodeDefinition definition)
    {
        for (int i = 0; i < ResourceNodes.Length; i++) if (string.Equals(ResourceNodes[i].StableKey, stableKey, StringComparison.Ordinal)) { definition = ResourceNodes[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetBuilding(string stableKey, out BuildingDefinition definition) => TryGetBuilding(StableId.FromKey(stableKey), out definition);
    public bool TryGetBuilding(ContentId id, out BuildingDefinition definition)
    {
        for (int i = 0; i < Buildings.Length; i++) if (Buildings[i].Id == id) { definition = Buildings[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetProduction(ContentId unitType, out UnitProductionDefinition definition)
    {
        for (int i = 0; i < Production.Length; i++) if (Production[i].UnitType == unitType) { definition = Production[i]; return true; }
        definition = default; return false;
    }
    public bool IsProducer(ContentId buildingType)
    {
        for (int i = 0; i < Production.Length; i++) if (Production[i].ProducerType == buildingType) return true;
        return false;
    }
    public bool TryGetWeapon(string stableKey, out WeaponDefinition definition) => TryGetWeapon(StableId.FromKey(stableKey), out definition);
    public bool TryGetWeapon(ContentId id, out WeaponDefinition definition)
    {
        for (int i = 0; i < Weapons.Length; i++) if (Weapons[i].Id == id) { definition = Weapons[i]; return true; }
        definition = default; return false;
    }
}

public static class PrototypeContentFactory
{
    /// <summary>Built-in mirror of the checked-in prototype source data; used by headless/debug startup before generated files are present.</summary>
    public static PrototypeContentCatalog CreateM2Catalog()
    {
        PrototypeMovementProfile[] profiles =
        {
            new PrototypeMovementProfile("movement.prototype.chrome_crusher", Fix32.FromRatio(92,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.crew", Fix32.FromRatio(135,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.hover_scout", Fix32.FromRatio(225,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.prototype.loader_dozer", Fix32.FromRatio(130,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.nav_huge", Fix32.FromRatio(9,10), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 410, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.rapid_rider", Fix32.FromRatio(210,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.prototype.static", Fix32.Zero, Fix32.Zero, Fix32.Zero, 0, ReversePolicy.None, MovementLayer.Ground)
        };
        WeaponDefinition[] weapons =
        {
            new WeaponDefinition("weapon.rr.chrome_crusher.chrome_drill", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Siege, 55, DamageType.Siege, 32, Fix32.FromRatio(21,20), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true),
            new WeaponDefinition("weapon.rr.crew.portable_mining_tool", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Support, 6, DamageType.General, 24, Fix32.FromRatio(4,5), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true),
            new WeaponDefinition("weapon.rr.hover_scout.survey_pulse", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Scout, 6, DamageType.General, 30, Fix32.FromInt(3), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
            new WeaponDefinition("weapon.rr.loader_dozer.scoop_ram", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.AntiLight, 18, DamageType.General, 27, Fix32.FromRatio(9,10), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true)
        };
        PrototypeEntityDefinition[] entities =
        {
            new PrototypeEntityDefinition("building.rock_raiders.hq", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.hq", combat: new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command, 3000, 5)),
            new PrototypeEntityDefinition("building.rock_raiders.ore_processing_plant", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.rock_raiders.ore_processing_plant", combat: new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1350, 2)),
            new PrototypeEntityDefinition("building.rock_raiders.power_station", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.rock_raiders.power_station", combat: new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1000, 1)),
            new PrototypeEntityDefinition("building.rock_raiders.vehicle_service_bay", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.vehicle_service_bay", combat: new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1700, 3)),
            new PrototypeEntityDefinition("prototype.nav.huge", "Technical", "ENGINEERING_ONLY", "movement.prototype.nav_huge", FootprintClass.Huge, SelectableKind.CombatSupport, 8, "view.placeholder.navigation.huge", combat: new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.None, 880, 5)),
            new PrototypeEntityDefinition("unit.rock_raiders.chrome_crusher", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.chrome_crusher", FootprintClass.Large, SelectableKind.CombatSupport, 8, "view.placeholder.rock_raiders.chrome_crusher", operationsCapacity: 6, combat: new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 880, 5, TargetPriorityProfile.Siege, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(101,20), StableId.FromKey("weapon.rr.chrome_crusher.chrome_drill"))),
            new PrototypeEntityDefinition("unit.rock_raiders.crew", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.crew", FootprintClass.Tiny, SelectableKind.Worker, 7, "view.placeholder.rock_raiders.crew", 30, 8, 1, new PrototypeCombatProfile(CombatTargetClass.Personnel, CombatTargetLayer.Ground, CombatTargetFlags.Worker | CombatTargetFlags.Support | CombatTargetFlags.CombatThreat, 110, 0, TargetPriorityProfile.Support, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(24,5), StableId.FromKey("weapon.rr.crew.portable_mining_tool"))),
            new PrototypeEntityDefinition("unit.rock_raiders.hover_scout", "RockRaiders", "OFFICIAL_DIRECT", "movement.prototype.hover_scout", FootprintClass.Small, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.hover_scout", operationsCapacity: 1, combat: new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Support | CombatTargetFlags.CombatThreat, 120, 0, TargetPriorityProfile.Scout, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromInt(7), StableId.FromKey("weapon.rr.hover_scout.survey_pulse"))),
            new PrototypeEntityDefinition("unit.rock_raiders.loader_dozer", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.loader_dozer", FootprintClass.Medium, SelectableKind.CombatSupport, 7, "view.placeholder.rock_raiders.loader_dozer", operationsCapacity: 3, combat: new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 360, 2, TargetPriorityProfile.AntiLight, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(49,10), StableId.FromKey("weapon.rr.loader_dozer.scoop_ram"))),
            new PrototypeEntityDefinition("unit.rock_raiders.rapid_rider", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.rapid_rider", FootprintClass.Small, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.rapid_rider", operationsCapacity: 2, combat: new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Transport, 170, 0))
        };
        ResourceNodeDefinition[] resources =
        {
            new ResourceNodeDefinition("resource.ore.deep_contested_seam", ResourceType.Ore, ResourceDepositSize.DeepContestedSeam, 2400, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.deep_contested_seam"),
            new ResourceNodeDefinition("resource.ore.rich", ResourceType.Ore, ResourceDepositSize.Rich, 1350, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.rich"),
            new ResourceNodeDefinition("resource.ore.small", ResourceType.Ore, ResourceDepositSize.Small, 600, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.small"),
            new ResourceNodeDefinition("resource.ore.standard", ResourceType.Ore, ResourceDepositSize.Standard, 900, HarvestInteraction.Mine, ResourceDepletionProfile.Finite, 7500, 5000, 2500, "view.placeholder.resource.ore.standard")
        };
        BuildingDefinition[] buildings =
        {
            new BuildingDefinition("building.rock_raiders.hq", 8, 8, ulong.MaxValue, false, 320, 40, 1200, 2, 2, FootprintClass.Tiny, 16, 2, 150, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy),
            new BuildingDefinition("building.rock_raiders.ore_processing_plant", 6, 6, (1UL << 36) - 1UL, false, 140, 15, 600, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ResourceProcessing),
            new BuildingDefinition("building.rock_raiders.power_station", 5, 5, (1UL << 25) - 1UL, false, 150, 20, 700, energyGenerationPerSecond: 10, energyReserveCapacity: 120),
            new BuildingDefinition("building.rock_raiders.vehicle_service_bay", 8, 6, (1UL << 48) - 1UL, true, 160, 20, 800, 3, 3, FootprintClass.Medium, 4, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch)
        };
        UnitProductionDefinition[] production =
        {
            new UnitProductionDefinition("unit.rock_raiders.crew", "building.rock_raiders.hq", 50, 0, 0, 1, 320),
            new UnitProductionDefinition("unit.rock_raiders.hover_scout", "building.rock_raiders.vehicle_service_bay", 75, 10, 0, 1, 400),
            new UnitProductionDefinition("unit.rock_raiders.loader_dozer", "building.rock_raiders.vehicle_service_bay", 125, 15, 0, 3, 720),
            new UnitProductionDefinition("unit.rock_raiders.rapid_rider", "building.rock_raiders.vehicle_service_bay", 90, 10, 0, 2, 560)
        };
        PrototypeContentCatalog catalog = new PrototypeContentCatalog(profiles, entities, resources, buildings, production, weapons);
        PrototypeContentCodec.Write(catalog);
        return catalog;
    }
}


public static class PrototypeContentCodec
{
    private const int Magic = 0x4350534C; // LSPC little-endian bytes.
    public const int FormatVersion = 13;

    public static byte[] Write(PrototypeContentCatalog catalog)
    {
        using MemoryStream stream = new MemoryStream(4096);
        using BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(Magic); writer.Write(FormatVersion);
        writer.Write(catalog.MovementProfiles.Length);
        for (int i = 0; i < catalog.MovementProfiles.Length; i++)
        {
            PrototypeMovementProfile p = catalog.MovementProfiles[i];
            writer.Write(p.StableKey); writer.Write(p.Id.Value); writer.Write(p.MaxSpeed.Raw); writer.Write(p.Acceleration.Raw); writer.Write(p.Deceleration.Raw);
            writer.Write(p.TurnRatePerTick); writer.Write((byte)p.ReversePolicy); writer.Write((byte)p.Layer);
        }
        writer.Write(catalog.Entities.Length);
        for (int i = 0; i < catalog.Entities.Length; i++)
        {
            PrototypeEntityDefinition e = catalog.Entities[i];
            writer.Write(e.StableKey); writer.Write(e.Id.Value); writer.Write(e.FactionKey); writer.Write(e.SourceClassification);
            writer.Write(e.MovementProfileKey); writer.Write((byte)e.Footprint); writer.Write((byte)e.SelectableKind); writer.Write(e.VisionRadius); writer.Write(e.ViewProfileKey);
            writer.Write(e.OreTicksPerUnit); writer.Write(e.OreCarryCapacity); writer.Write(e.OperationsCapacity);
            writer.Write(e.Combat.IsTargetable);
            if (e.Combat.IsTargetable)
            {
                writer.Write((byte)e.Combat.TargetClass); writer.Write((byte)e.Combat.TargetLayer); writer.Write((ushort)e.Combat.TargetFlags);
                writer.Write(e.Combat.MaximumHitPoints); writer.Write(e.Combat.ArmorRating);
                writer.Write((byte)e.Combat.PriorityProfile); writer.Write((byte)e.Combat.LegalTargetLayers); writer.Write((byte)e.Combat.LegalTargetClasses); writer.Write(e.Combat.AcquisitionRadius.Raw);
                writer.Write(e.Combat.WeaponProfile.Value);
            }
        }
        writer.Write(catalog.ResourceNodes.Length);
        for (int i = 0; i < catalog.ResourceNodes.Length; i++)
        {
            ResourceNodeDefinition n = catalog.ResourceNodes[i];
            writer.Write(n.StableKey); writer.Write(n.Id.Value); writer.Write((byte)n.Type); writer.Write((byte)n.DepositSize); writer.Write(n.Capacity);
            writer.Write((byte)n.HarvestInteraction); writer.Write((byte)n.DepletionProfile);
            writer.Write(n.ReducedThresholdBasisPoints); writer.Write(n.LowThresholdBasisPoints); writer.Write(n.CriticalThresholdBasisPoints); writer.Write(n.ViewProfileKey);
        }
        writer.Write(catalog.Buildings.Length);
        for (int i = 0; i < catalog.Buildings.Length; i++)
        {
            BuildingDefinition b = catalog.Buildings[i];
            writer.Write(b.StableKey); writer.Write(b.Id.Value); writer.Write(b.FootprintWidth); writer.Write(b.FootprintHeight); writer.Write(b.FootprintMask); writer.Write(b.Rotatable);
            writer.Write(b.OreCost); writer.Write(b.EnergyCost); writer.Write(b.BuildTicks); writer.Write(b.ProductionExitWidth); writer.Write(b.ProductionExitDepth); writer.Write((byte)b.ProductionExitFootprint);
            writer.Write(b.OperationsCapacityProvided);
            writer.Write(b.EnergyGenerationPerSecond); writer.Write(b.EnergyReserveCapacity); writer.Write(b.ContinuousEnergyDemandPerSecond);
            writer.Write((byte)b.EnergyFunctionalClass);
        }
        writer.Write(catalog.Production.Length);
        for (int i = 0; i < catalog.Production.Length; i++)
        {
            UnitProductionDefinition p = catalog.Production[i];
            writer.Write(p.UnitStableKey); writer.Write(p.UnitType.Value); writer.Write(p.ProducerStableKey); writer.Write(p.ProducerType.Value);
            writer.Write(p.OreCost); writer.Write(p.EnergyCost); writer.Write(p.CrystalCost); writer.Write(p.OperationsCapacity); writer.Write(p.BuildTicks);
        }
        writer.Write(catalog.Weapons.Length);
        for (int i = 0; i < catalog.Weapons.Length; i++)
        {
            WeaponDefinition weapon = catalog.Weapons[i];
            writer.Write(weapon.StableKey); writer.Write(weapon.Id.Value); writer.Write((byte)weapon.LegalTargetLayers); writer.Write((byte)weapon.LegalTargetClasses);
            writer.Write((byte)weapon.PriorityProfile); writer.Write(weapon.BaseDamage); writer.Write((byte)weapon.DamageType); writer.Write(weapon.CooldownTicks);
            writer.Write(weapon.Range.Raw); writer.Write(weapon.MinimumRange.Raw); writer.Write((byte)weapon.DeliveryKind); writer.Write(weapon.ProjectileSpeed.Raw); writer.Write(weapon.RequiresLineOfSight);
        }
        writer.Flush();
        byte[] bytes = stream.ToArray(); catalog.ContentHash = DeterministicHash.Fnv1A64(bytes); return bytes;
    }

    public static PrototypeContentCatalog Read(byte[] bytes)
    {
        using MemoryStream stream = new MemoryStream(bytes, false);
        using BinaryReader reader = new BinaryReader(stream);
        if (reader.ReadInt32() != Magic) throw new InvalidDataException("Prototype content magic mismatch.");
        int formatVersion = reader.ReadInt32();
        if (formatVersion < 2 || formatVersion > FormatVersion) throw new InvalidDataException("Prototype content format version mismatch.");
        int profileCount = reader.ReadInt32(); if (profileCount < 0 || profileCount > 1024) throw new InvalidDataException("Invalid movement profile count.");
        PrototypeMovementProfile[] profiles = new PrototypeMovementProfile[profileCount];
        for (int i = 0; i < profileCount; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); Fix32 speed = Fix32.FromRaw(reader.ReadInt32());
            Fix32 acceleration=Fix32.FromRaw(reader.ReadInt32()); Fix32 deceleration=Fix32.FromRaw(reader.ReadInt32()); ushort turn=reader.ReadUInt16();
            ReversePolicy reverse=(ReversePolicy)reader.ReadByte(); MovementLayer layer = (MovementLayer)reader.ReadByte();
            profiles[i] = new PrototypeMovementProfile(key, speed, acceleration, deceleration, turn, reverse, layer); if (profiles[i].Id.Value != id) throw new InvalidDataException("Stable movement ID mismatch.");
        }
        int entityCount = reader.ReadInt32(); if (entityCount < 0 || entityCount > 4096) throw new InvalidDataException("Invalid prototype entity count.");
        PrototypeEntityDefinition[] entities = new PrototypeEntityDefinition[entityCount];
        for (int i = 0; i < entityCount; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); string faction = reader.ReadString(); string source = reader.ReadString(); string movement = reader.ReadString();
            FootprintClass fp = (FootprintClass)reader.ReadByte(); SelectableKind kind = (SelectableKind)reader.ReadByte(); byte vision = reader.ReadByte(); string view = reader.ReadString();
            ushort oreTicks = formatVersion >= 4 ? reader.ReadUInt16() : kind == SelectableKind.Worker ? (ushort)30 : (ushort)0;
            byte oreCapacity = formatVersion >= 4 ? reader.ReadByte() : kind == SelectableKind.Worker ? (byte)8 : (byte)0;
            byte operationsCapacity = formatVersion >= 7 ? reader.ReadByte() : LegacyOperationsCapacity(key);
            PrototypeCombatProfile combat = formatVersion >= 10 ? ReadCombatProfile(reader, formatVersion >= 11, formatVersion >= 13) : LegacyCombatProfile(key);
            if (formatVersion == 10) combat = AddLegacyWeaponProfile(key, combat);
            if (formatVersion < 13) combat = AddLegacyDurability(key, combat);
            entities[i] = new PrototypeEntityDefinition(key, faction, source, movement, fp, kind, vision, view, oreTicks, oreCapacity, operationsCapacity, combat); if (entities[i].Id.Value != id) throw new InvalidDataException("Stable entity ID mismatch.");
        }
        ResourceNodeDefinition[] resourceNodes = Array.Empty<ResourceNodeDefinition>();
        if (formatVersion >= 3)
        {
            int resourceCount = reader.ReadInt32(); if (resourceCount < 0 || resourceCount > 1024) throw new InvalidDataException("Invalid resource node definition count.");
            resourceNodes = new ResourceNodeDefinition[resourceCount];
            for (int i = 0; i < resourceCount; i++)
            {
                string key = reader.ReadString(); uint id = reader.ReadUInt32(); ResourceType type = (ResourceType)reader.ReadByte(); ResourceDepositSize size = (ResourceDepositSize)reader.ReadByte(); int capacity = reader.ReadInt32();
                HarvestInteraction interaction = (HarvestInteraction)reader.ReadByte(); ResourceDepletionProfile depletion = (ResourceDepletionProfile)reader.ReadByte();
                ushort reduced = reader.ReadUInt16(); ushort low = reader.ReadUInt16(); ushort critical = reader.ReadUInt16(); string view = reader.ReadString();
                resourceNodes[i] = new ResourceNodeDefinition(key, type, size, capacity, interaction, depletion, reduced, low, critical, view);
                if (resourceNodes[i].Id.Value != id) throw new InvalidDataException("Stable resource node ID mismatch.");
            }
        }
        BuildingDefinition[] buildings = Array.Empty<BuildingDefinition>();
        if (formatVersion >= 5)
        {
            int buildingCount = reader.ReadInt32(); if (buildingCount < 0 || buildingCount > 1024) throw new InvalidDataException("Invalid building definition count.");
            buildings = new BuildingDefinition[buildingCount];
            for (int i = 0; i < buildingCount; i++)
            {
                string key = reader.ReadString(); uint id = reader.ReadUInt32();
                byte width = reader.ReadByte(), height = reader.ReadByte(); ulong mask = reader.ReadUInt64(); bool rotatable = reader.ReadBoolean();
                ushort oreCost = reader.ReadUInt16(), energyCost = reader.ReadUInt16(), buildTicks = reader.ReadUInt16();
                byte exitWidth = reader.ReadByte(), exitDepth = reader.ReadByte(); FootprintClass exitFootprint = (FootprintClass)reader.ReadByte();
                byte capacityProvided = formatVersion >= 7 ? reader.ReadByte() : LegacyOperationsCapacityProvided(key);
                LegacyEnergyDefinition(key, out ushort generation, out ushort reserveCapacity, out ushort demand);
                if (formatVersion >= 8) { generation = reader.ReadUInt16(); reserveCapacity = reader.ReadUInt16(); demand = reader.ReadUInt16(); }
                EnergyFunctionalClass functionalClass = formatVersion >= 9 ? (EnergyFunctionalClass)reader.ReadByte() : LegacyEnergyFunctionalClass(key);
                buildings[i] = new BuildingDefinition(key, width, height, mask, rotatable, oreCost, energyCost, buildTicks, exitWidth, exitDepth, exitFootprint, capacityProvided, generation, reserveCapacity, demand, functionalClass);
                if (buildings[i].Id.Value != id) throw new InvalidDataException("Stable building ID mismatch.");
            }
        }
        UnitProductionDefinition[] production = Array.Empty<UnitProductionDefinition>();
        if (formatVersion >= 6)
        {
            int productionCount = reader.ReadInt32(); if (productionCount < 0 || productionCount > 4096) throw new InvalidDataException("Invalid production definition count.");
            production = new UnitProductionDefinition[productionCount];
            for (int i = 0; i < productionCount; i++)
            {
                string unitKey = reader.ReadString(); uint unitId = reader.ReadUInt32(); string producerKey = reader.ReadString(); uint producerId = reader.ReadUInt32();
                production[i] = new UnitProductionDefinition(unitKey, producerKey, reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadByte(), reader.ReadByte(), reader.ReadUInt16());
                if (production[i].UnitType.Value != unitId || production[i].ProducerType.Value != producerId) throw new InvalidDataException("Stable production ID mismatch.");
            }
        }
        WeaponDefinition[] weapons = formatVersion >= 11 ? ReadWeapons(reader, formatVersion >= 12) : LegacyWeapons();
        if (stream.Position != stream.Length) throw new InvalidDataException("Trailing prototype content bytes.");
        PrototypeContentCatalog result = new PrototypeContentCatalog(profiles, entities, resourceNodes, buildings, production, weapons) { ContentHash = DeterministicHash.Fnv1A64(bytes) };
        return result;
    }

    private static byte LegacyOperationsCapacity(string stableKey) => stableKey switch
    {
        "unit.rock_raiders.crew" => 1,
        "unit.rock_raiders.hover_scout" => 1,
        "unit.rock_raiders.rapid_rider" => 2,
        "unit.rock_raiders.loader_dozer" => 3,
        "unit.rock_raiders.chrome_crusher" => 6,
        _ => 0
    };

    private static byte LegacyOperationsCapacityProvided(string stableKey) => stableKey switch
    {
        "building.rock_raiders.hq" => 16,
        "building.rock_raiders.vehicle_service_bay" => 4,
        _ => 0
    };

    private static void LegacyEnergyDefinition(string stableKey, out ushort generation, out ushort reserveCapacity, out ushort demand)
    {
        generation = stableKey switch { "building.rock_raiders.hq" => 2, "building.rock_raiders.power_station" => 10, _ => 0 };
        reserveCapacity = stableKey switch { "building.rock_raiders.hq" => 150, "building.rock_raiders.power_station" => 120, _ => 0 };
        demand = stableKey switch { "building.rock_raiders.ore_processing_plant" => 1, "building.rock_raiders.vehicle_service_bay" => 1, _ => 0 };
    }

    private static EnergyFunctionalClass LegacyEnergyFunctionalClass(string stableKey) => stableKey switch
    {
        "building.rock_raiders.hq" => EnergyFunctionalClass.CommandAndBasicEconomy,
        "building.rock_raiders.ore_processing_plant" => EnergyFunctionalClass.ResourceProcessing,
        "building.rock_raiders.vehicle_service_bay" => EnergyFunctionalClass.ProductionAndResearch,
        _ => EnergyFunctionalClass.StaticDefenseAndNonessential
    };

    private static PrototypeCombatProfile ReadCombatProfile(BinaryReader reader, bool includeWeaponProfile, bool includeDurability)
    {
        if (!reader.ReadBoolean()) return default;
        CombatTargetClass targetClass = (CombatTargetClass)reader.ReadByte();
        CombatTargetLayer targetLayer = (CombatTargetLayer)reader.ReadByte();
        CombatTargetFlags targetFlags = (CombatTargetFlags)reader.ReadUInt16();
        ushort maximumHitPoints = includeDurability ? reader.ReadUInt16() : (ushort)1;
        byte armorRating = includeDurability ? reader.ReadByte() : (byte)0;
        return new PrototypeCombatProfile(targetClass, targetLayer, targetFlags, maximumHitPoints, armorRating,
            (TargetPriorityProfile)reader.ReadByte(), (TargetLayerMask)reader.ReadByte(), (TargetClassMask)reader.ReadByte(), Fix32.FromRaw(reader.ReadInt32()),
            includeWeaponProfile ? new ContentId(reader.ReadUInt32()) : default);
    }

    private static WeaponDefinition[] ReadWeapons(BinaryReader reader, bool includeProjectileSpeed)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > 4096) throw new InvalidDataException("Invalid weapon definition count.");
        WeaponDefinition[] weapons = new WeaponDefinition[count];
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32();
            TargetLayerMask layers = (TargetLayerMask)reader.ReadByte(); TargetClassMask classes = (TargetClassMask)reader.ReadByte();
            TargetPriorityProfile priority = (TargetPriorityProfile)reader.ReadByte(); ushort damage = reader.ReadUInt16(); DamageType damageType = (DamageType)reader.ReadByte(); ushort cooldown = reader.ReadUInt16();
            Fix32 range = Fix32.FromRaw(reader.ReadInt32()); Fix32 minimumRange = Fix32.FromRaw(reader.ReadInt32()); WeaponDeliveryKind delivery = (WeaponDeliveryKind)reader.ReadByte();
            Fix32 projectileSpeed = includeProjectileSpeed ? Fix32.FromRaw(reader.ReadInt32()) : delivery == WeaponDeliveryKind.Projectile ? Fix32.FromInt(12) : Fix32.Zero;
            weapons[i] = new WeaponDefinition(key, layers, classes, priority, damage, damageType, cooldown, range, minimumRange, delivery, projectileSpeed, reader.ReadBoolean());
            if (weapons[i].Id.Value != id) throw new InvalidDataException("Stable weapon ID mismatch.");
        }
        return weapons;
    }

    private static PrototypeCombatProfile AddLegacyWeaponProfile(string stableKey, PrototypeCombatProfile combat)
    {
        string? weaponKey = LegacyWeaponKey(stableKey);
        return weaponKey is null ? combat : new PrototypeCombatProfile(combat.TargetClass, combat.TargetLayer, combat.TargetFlags,
            combat.MaximumHitPoints, combat.ArmorRating,
            combat.PriorityProfile, combat.LegalTargetLayers, combat.LegalTargetClasses, combat.AcquisitionRadius, StableId.FromKey(weaponKey));
    }

    private static PrototypeCombatProfile AddLegacyDurability(string stableKey, PrototypeCombatProfile combat)
    {
        if (!combat.IsTargetable) return combat;
        LegacyDurability(stableKey, out ushort hitPoints, out byte armorRating);
        return new PrototypeCombatProfile(combat.TargetClass, combat.TargetLayer, combat.TargetFlags, hitPoints, armorRating,
            combat.PriorityProfile, combat.LegalTargetLayers, combat.LegalTargetClasses, combat.AcquisitionRadius, combat.WeaponProfile);
    }

    internal static void LegacyDurability(string stableKey, out ushort hitPoints, out byte armorRating)
    {
        (hitPoints, armorRating) = stableKey switch
        {
            "building.rock_raiders.hq" => ((ushort)3000, (byte)5),
            "building.rock_raiders.ore_processing_plant" => ((ushort)1350, (byte)2),
            "building.rock_raiders.power_station" => ((ushort)1000, (byte)1),
            "building.rock_raiders.vehicle_service_bay" => ((ushort)1700, (byte)3),
            "unit.rock_raiders.crew" => ((ushort)110, (byte)0),
            "unit.rock_raiders.hover_scout" => ((ushort)120, (byte)0),
            "unit.rock_raiders.loader_dozer" => ((ushort)360, (byte)2),
            "unit.rock_raiders.chrome_crusher" => ((ushort)880, (byte)5),
            "unit.rock_raiders.rapid_rider" => ((ushort)170, (byte)0),
            "prototype.nav.huge" => ((ushort)880, (byte)5),
            _ => ((ushort)1, (byte)0)
        };
    }

    private static string? LegacyWeaponKey(string stableKey) => stableKey switch
    {
        "unit.rock_raiders.crew" => "weapon.rr.crew.portable_mining_tool",
        "unit.rock_raiders.hover_scout" => "weapon.rr.hover_scout.survey_pulse",
        "unit.rock_raiders.loader_dozer" => "weapon.rr.loader_dozer.scoop_ram",
        "unit.rock_raiders.chrome_crusher" => "weapon.rr.chrome_crusher.chrome_drill",
        _ => null
    };

    private static WeaponDefinition[] LegacyWeapons() => new[]
    {
        new WeaponDefinition("weapon.rr.chrome_crusher.chrome_drill", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Siege, 55, DamageType.Siege, 32, Fix32.FromRatio(21,20), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true),
        new WeaponDefinition("weapon.rr.crew.portable_mining_tool", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Support, 6, DamageType.General, 24, Fix32.FromRatio(4,5), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true),
        new WeaponDefinition("weapon.rr.hover_scout.survey_pulse", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Scout, 6, DamageType.General, 30, Fix32.FromInt(3), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
        new WeaponDefinition("weapon.rr.loader_dozer.scoop_ram", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.AntiLight, 18, DamageType.General, 27, Fix32.FromRatio(9,10), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true)
    };

    private static PrototypeCombatProfile LegacyCombatProfile(string stableKey) => stableKey switch
    {
        "building.rock_raiders.hq" => new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command, 3000, 5),
        "building.rock_raiders.ore_processing_plant" => new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1350, 2),
        "building.rock_raiders.power_station" => new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1000, 1),
        "building.rock_raiders.vehicle_service_bay" => new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1700, 3),
        "unit.rock_raiders.crew" => new PrototypeCombatProfile(CombatTargetClass.Personnel, CombatTargetLayer.Ground, CombatTargetFlags.Worker | CombatTargetFlags.Support | CombatTargetFlags.CombatThreat, 110, 0, TargetPriorityProfile.Support, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(24,5), StableId.FromKey("weapon.rr.crew.portable_mining_tool")),
        "unit.rock_raiders.hover_scout" => new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Support | CombatTargetFlags.CombatThreat, 120, 0, TargetPriorityProfile.Scout, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromInt(7), StableId.FromKey("weapon.rr.hover_scout.survey_pulse")),
        "unit.rock_raiders.loader_dozer" => new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 360, 2, TargetPriorityProfile.AntiLight, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(49,10), StableId.FromKey("weapon.rr.loader_dozer.scoop_ram")),
        "unit.rock_raiders.chrome_crusher" => new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 880, 5, TargetPriorityProfile.Siege, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(101,20), StableId.FromKey("weapon.rr.chrome_crusher.chrome_drill")),
        "unit.rock_raiders.rapid_rider" => new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Transport, 170, 0),
        "prototype.nav.huge" => new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.None, 880, 5),
        _ => default
    };
}
}
