using System;
using System.Collections.Generic;
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
    public readonly ushort FacingToleranceAngle16;
    public readonly ushort MaximumMovingFireSpeedBasisPoints;

    public WeaponDefinition(string stableKey, TargetLayerMask legalTargetLayers, TargetClassMask legalTargetClasses,
        TargetPriorityProfile priorityProfile, ushort baseDamage, DamageType damageType, ushort cooldownTicks,
        Fix32 range, Fix32 minimumRange, WeaponDeliveryKind deliveryKind, Fix32 projectileSpeed, bool requiresLineOfSight,
        ushort facingToleranceDegrees = 180, ushort maximumMovingFireSpeedBasisPoints = 10_000)
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
        if (facingToleranceDegrees == 0 || facingToleranceDegrees > 180) throw new ArgumentOutOfRangeException(nameof(facingToleranceDegrees));
        if (maximumMovingFireSpeedBasisPoints > 10_000) throw new ArgumentOutOfRangeException(nameof(maximumMovingFireSpeedBasisPoints));
        StableKey = stableKey; Id = StableId.FromKey(stableKey); LegalTargetLayers = legalTargetLayers; LegalTargetClasses = legalTargetClasses;
        PriorityProfile = priorityProfile; BaseDamage = baseDamage; DamageType = damageType; CooldownTicks = cooldownTicks;
        Range = range; MinimumRange = minimumRange; DeliveryKind = deliveryKind; ProjectileSpeed = projectileSpeed; RequiresLineOfSight = requiresLineOfSight;
        FacingToleranceAngle16 = checked((ushort)((facingToleranceDegrees * 65_536L) / 360));
        MaximumMovingFireSpeedBasisPoints = maximumMovingFireSpeedBasisPoints;
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

public readonly struct TransformationModeDefinition
{
    public readonly string StateKey;
    public readonly ContentId StateId;
    public readonly string DisplayName;
    public readonly string MovementProfileKey;
    public readonly FootprintClass Footprint;
    public readonly byte VisionRadius;
    public readonly string ViewProfileKey;
    public readonly PrototypeCombatProfile Combat;

    public TransformationModeDefinition(string stateKey, string displayName, string movementProfileKey, FootprintClass footprint,
        byte visionRadius, string viewProfileKey, PrototypeCombatProfile combat)
    {
        if (string.IsNullOrWhiteSpace(stateKey) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(movementProfileKey) ||
            string.IsNullOrWhiteSpace(viewProfileKey) || !combat.IsTargetable) throw new ArgumentException("Transformation modes require stable state, presentation, movement and combat data.");
        StateKey = stateKey; StateId = StableId.FromKey(stateKey); DisplayName = displayName; MovementProfileKey = movementProfileKey;
        Footprint = footprint; VisionRadius = visionRadius; ViewProfileKey = viewProfileKey; Combat = combat;
    }
}

/// <summary>Canonical two-state tactical transformation definition. Mode A is the entity's authored spawn state.</summary>
public readonly struct TransformationDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly string EntityStableKey;
    public readonly ContentId EntityType;
    public readonly TransformationModeDefinition ModeA;
    public readonly TransformationModeDefinition ModeB;
    public readonly ushort AToBDurationTicks;
    public readonly ushort BToADurationTicks;
    public readonly ushort CancellationThresholdBasisPoints;
    public readonly ushort RollbackTicks;
    public readonly ushort ReversalLockTicks;
    public readonly bool MoveDuringTransition;
    public readonly bool AttackDuringTransition;
    public readonly TargetLayerMask TransitionTargetLayers;

    public TransformationDefinition(string stableKey, string entityStableKey, TransformationModeDefinition modeA, TransformationModeDefinition modeB,
        ushort aToBDurationTicks, ushort bToADurationTicks, ushort cancellationThresholdBasisPoints, ushort rollbackTicks, ushort reversalLockTicks,
        bool moveDuringTransition, bool attackDuringTransition, TargetLayerMask transitionTargetLayers)
    {
        if (string.IsNullOrWhiteSpace(stableKey) || string.IsNullOrWhiteSpace(entityStableKey) || modeA.StateId == modeB.StateId ||
            aToBDurationTicks == 0 || bToADurationTicks == 0 || cancellationThresholdBasisPoints == 0 || cancellationThresholdBasisPoints > 10_000 ||
            rollbackTicks == 0 || reversalLockTicks == 0 || transitionTargetLayers == TargetLayerMask.None ||
            (transitionTargetLayers & ~TargetLayerMask.All) != 0) throw new ArgumentException("Invalid transformation definition.");
        if (modeA.Combat.MaximumHitPoints != modeB.Combat.MaximumHitPoints)
            throw new ArgumentException("Tactical transformation modes must preserve maximum HP.");
        StableKey = stableKey; Id = StableId.FromKey(stableKey); EntityStableKey = entityStableKey; EntityType = StableId.FromKey(entityStableKey);
        ModeA = modeA; ModeB = modeB; AToBDurationTicks = aToBDurationTicks; BToADurationTicks = bToADurationTicks;
        CancellationThresholdBasisPoints = cancellationThresholdBasisPoints; RollbackTicks = rollbackTicks; ReversalLockTicks = reversalLockTicks;
        MoveDuringTransition = moveDuringTransition; AttackDuringTransition = attackDuringTransition; TransitionTargetLayers = transitionTargetLayers;
    }

    public TransformationModeDefinition GetMode(ContentId state) => state == ModeA.StateId ? ModeA : state == ModeB.StateId ? ModeB : default;
    public TransformationModeDefinition GetDestination(ContentId source) => source == ModeA.StateId ? ModeB : source == ModeB.StateId ? ModeA : default;
    public ushort GetDuration(ContentId source) => source == ModeA.StateId ? AToBDurationTicks : source == ModeB.StateId ? BToADurationTicks : (ushort)0;
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
    public readonly ulong FootprintMaskHigh;
    public readonly bool Rotatable;
    public readonly ushort OreCost;
    public readonly ushort EnergyCost;
    public readonly byte CrystalCost;
    public readonly ushort BuildTicks;
    public readonly byte ProductionExitWidth;
    public readonly byte ProductionExitDepth;
    public readonly FootprintClass ProductionExitFootprint;
    public readonly byte OperationsCapacityProvided;
    public readonly ushort EnergyGenerationPerSecond;
    public readonly ushort EnergyReserveCapacity;
    public readonly ushort ContinuousEnergyDemandPerSecond;
    public readonly EnergyFunctionalClass EnergyFunctionalClass;
    public readonly byte WorksiteServiceRadius;
    public readonly ContentActionPrerequisiteGroup[] PrerequisiteGroups;

    public BuildingDefinition(string stableKey, byte footprintWidth, byte footprintHeight, ulong footprintMask, bool rotatable,
        ushort oreCost, ushort energyCost, ushort buildTicks, byte productionExitWidth = 0, byte productionExitDepth = 0,
        FootprintClass productionExitFootprint = FootprintClass.Tiny, byte operationsCapacityProvided = 0,
        ushort energyGenerationPerSecond = 0, ushort energyReserveCapacity = 0, ushort continuousEnergyDemandPerSecond = 0,
        EnergyFunctionalClass energyFunctionalClass = EnergyFunctionalClass.StaticDefenseAndNonessential, byte worksiteServiceRadius = 0,
        byte crystalCost = 0, ulong footprintMaskHigh = 0, ContentActionPrerequisiteGroup[]? prerequisiteGroups = null)
    {
        if (footprintWidth == 0 || footprintHeight == 0 || footprintWidth > 10 || footprintHeight > 10) throw new ArgumentOutOfRangeException(nameof(footprintWidth));
        int cells = footprintWidth * footprintHeight;
        ulong allowedMask = cells >= 64 ? ulong.MaxValue : (1UL << cells) - 1UL;
        int highCells = cells - 64;
        ulong allowedMaskHigh = highCells <= 0 ? 0UL : highCells == 64 ? ulong.MaxValue : (1UL << highCells) - 1UL;
        if ((footprintMask == 0 && footprintMaskHigh == 0) || (footprintMask & ~allowedMask) != 0 || (footprintMaskHigh & ~allowedMaskHigh) != 0)
            throw new ArgumentOutOfRangeException(nameof(footprintMask));
        if ((productionExitWidth == 0) != (productionExitDepth == 0)) throw new ArgumentException("Production exit width and depth must both be present or absent.");
        StableKey = stableKey ?? throw new ArgumentNullException(nameof(stableKey));
        Id = StableId.FromKey(stableKey);
        FootprintWidth = footprintWidth; FootprintHeight = footprintHeight; FootprintMask = footprintMask; FootprintMaskHigh = footprintMaskHigh; Rotatable = rotatable;
        OreCost = oreCost; EnergyCost = energyCost; CrystalCost = crystalCost; BuildTicks = buildTicks;
        ProductionExitWidth = productionExitWidth; ProductionExitDepth = productionExitDepth; ProductionExitFootprint = productionExitFootprint;
        OperationsCapacityProvided = operationsCapacityProvided;
        EnergyGenerationPerSecond = energyGenerationPerSecond; EnergyReserveCapacity = energyReserveCapacity;
        ContinuousEnergyDemandPerSecond = continuousEnergyDemandPerSecond;
        EnergyFunctionalClass = energyFunctionalClass;
        WorksiteServiceRadius = worksiteServiceRadius;
        PrerequisiteGroups = prerequisiteGroups ?? Array.Empty<ContentActionPrerequisiteGroup>();
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
        int bitIndex = sourceY * FootprintWidth + sourceX;
        return bitIndex < 64
            ? (FootprintMask & (1UL << bitIndex)) != 0
            : (FootprintMaskHigh & (1UL << (bitIndex - 64))) != 0;
    }
}

public readonly struct UnitProductionDefinition
{
    public readonly ContentId UnitType;
    public readonly string UnitStableKey;
    public readonly ContentId ProducerType;
    public readonly string ProducerStableKey;
    public readonly ContentId[] ProducerTypes;
    public readonly string[] ProducerStableKeys;
    public readonly ushort OreCost;
    public readonly ushort EnergyCost;
    public readonly byte CrystalCost;
    public readonly byte OperationsCapacity;
    public readonly ushort BuildTicks;
    public readonly ContentActionPrerequisiteGroup[] PrerequisiteGroups;

    public UnitProductionDefinition(string unitStableKey, string producerStableKey, ushort oreCost, ushort energyCost,
        byte crystalCost, byte operationsCapacity, ushort buildTicks, ContentActionPrerequisiteGroup[]? prerequisiteGroups = null)
        : this(unitStableKey, new[] { producerStableKey }, oreCost, energyCost, crystalCost, operationsCapacity, buildTicks, prerequisiteGroups)
    {
    }

    public UnitProductionDefinition(string unitStableKey, string[] producerStableKeys, ushort oreCost, ushort energyCost,
        byte crystalCost, byte operationsCapacity, ushort buildTicks, ContentActionPrerequisiteGroup[]? prerequisiteGroups = null)
    {
        if (string.IsNullOrWhiteSpace(unitStableKey)) throw new ArgumentNullException(nameof(unitStableKey));
        if (producerStableKeys == null || producerStableKeys.Length == 0) throw new ArgumentException("At least one producer is required.", nameof(producerStableKeys));
        if (buildTicks == 0 || operationsCapacity == 0) throw new ArgumentOutOfRangeException(nameof(buildTicks));
        ProducerStableKeys = new string[producerStableKeys.Length];
        ProducerTypes = new ContentId[producerStableKeys.Length];
        for (int i = 0; i < producerStableKeys.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(producerStableKeys[i])) throw new ArgumentException("Producer keys cannot be empty.", nameof(producerStableKeys));
            ProducerStableKeys[i] = producerStableKeys[i];
            ProducerTypes[i] = StableId.FromKey(producerStableKeys[i]);
        }
        UnitStableKey = unitStableKey; UnitType = StableId.FromKey(unitStableKey);
        ProducerStableKey = ProducerStableKeys[0]; ProducerType = ProducerTypes[0];
        OreCost = oreCost; EnergyCost = energyCost; CrystalCost = crystalCost; OperationsCapacity = operationsCapacity; BuildTicks = buildTicks;
        PrerequisiteGroups = prerequisiteGroups ?? Array.Empty<ContentActionPrerequisiteGroup>();
    }

    public bool CanProduceAt(ContentId buildingType)
    {
        for (int i = 0; i < ProducerTypes.Length; i++) if (ProducerTypes[i] == buildingType) return true;
        return false;
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
    public TransformationDefinition[] Transformations { get; }
    public ResearchDefinition[] Research { get; }
    public CommandDefinition[] Commands { get; }
    public ulong ContentHash { get; internal set; }
    public PrototypeContentCatalog(PrototypeMovementProfile[] movementProfiles, PrototypeEntityDefinition[] entities, ResourceNodeDefinition[]? resourceNodes = null, BuildingDefinition[]? buildings = null, UnitProductionDefinition[]? production = null, WeaponDefinition[]? weapons = null, TransformationDefinition[]? transformations = null, ResearchDefinition[]? research = null, CommandDefinition[]? commands = null)
    {
        MovementProfiles = movementProfiles ?? Array.Empty<PrototypeMovementProfile>();
        Entities = entities ?? Array.Empty<PrototypeEntityDefinition>();
        ResourceNodes = resourceNodes ?? Array.Empty<ResourceNodeDefinition>();
        Buildings = buildings ?? Array.Empty<BuildingDefinition>();
        Production = production ?? Array.Empty<UnitProductionDefinition>();
        Weapons = weapons ?? Array.Empty<WeaponDefinition>();
        Transformations = transformations ?? Array.Empty<TransformationDefinition>();
        Research = research ?? Array.Empty<ResearchDefinition>();
        Commands = commands ?? Array.Empty<CommandDefinition>();
        for (int i = 0; i < Entities.Length; i++)
        {
            ContentId weaponProfile = Entities[i].Combat.WeaponProfile;
            if (weaponProfile.Value == 0) continue;
            if (!TryGetWeapon(weaponProfile, out WeaponDefinition weapon)) throw new ArgumentException($"Entity {Entities[i].StableKey} references an unknown weapon profile.");
            if (!Entities[i].Combat.CanAcquireTargets || Entities[i].Combat.LegalTargetLayers != weapon.LegalTargetLayers ||
                Entities[i].Combat.LegalTargetClasses != weapon.LegalTargetClasses || Entities[i].Combat.PriorityProfile != weapon.PriorityProfile)
                throw new ArgumentException($"Entity {Entities[i].StableKey} targeting metadata disagrees with weapon {weapon.StableKey}.");
        }
        for (int i = 0; i < Transformations.Length; i++)
        {
            TransformationDefinition transformation = Transformations[i];
            if (!TryGetEntity(transformation.EntityType, out PrototypeEntityDefinition entity)) throw new ArgumentException($"Transformation {transformation.StableKey} references an unknown entity.");
            ValidateTransformationMode(transformation, entity, transformation.ModeA, authoredMode: true);
            ValidateTransformationMode(transformation, entity, transformation.ModeB, authoredMode: false);
        }
        ResearchDefinitionValidator.Validate(this);
        ValidateActionPrerequisites();
        CommandDefinitionValidator.Validate(this);
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
        for (int i = 0; i < Production.Length; i++) if (Production[i].CanProduceAt(buildingType)) return true;
        return false;
    }
    public bool TryGetWeapon(string stableKey, out WeaponDefinition definition) => TryGetWeapon(StableId.FromKey(stableKey), out definition);
    public bool TryGetWeapon(ContentId id, out WeaponDefinition definition)
    {
        for (int i = 0; i < Weapons.Length; i++) if (Weapons[i].Id == id) { definition = Weapons[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetTransformation(ContentId entityType, out TransformationDefinition definition)
    {
        for (int i = 0; i < Transformations.Length; i++) if (Transformations[i].EntityType == entityType) { definition = Transformations[i]; return true; }
        definition = default; return false;
    }
    public bool TryGetResearch(string stableKey, out ResearchDefinition definition) => TryGetResearch(StableId.FromKey(stableKey), out definition);
    public bool TryGetResearch(ContentId id, out ResearchDefinition definition)
    {
        for (int i = 0; i < Research.Length; i++) if (Research[i].Id == id) { definition = Research[i]; return true; }
        definition = default; return false;
    }

    public bool TryGetCommand(SimCommandType type, out CommandDefinition definition)
    {
        for (int i = 0; i < Commands.Length; i++) if (Commands[i].CommandType == type) { definition = Commands[i]; return true; }
        definition = default; return false;
    }

    private void ValidateActionPrerequisites()
    {
        HashSet<uint> buildingIds = new();
        Dictionary<string, BuildingDefinition> buildingsByKey = new(StringComparer.Ordinal);
        for (int i = 0; i < Buildings.Length; i++)
        {
            BuildingDefinition building = Buildings[i];
            if (!buildingIds.Add(building.Id.Value) || !buildingsByKey.TryAdd(building.StableKey, building))
                throw new ArgumentException($"Duplicate building action definition {building.StableKey}.");
            if (building.PrerequisiteGroups.Length == 0) continue;
            if (!TryGetEntity(building.Id, out PrototypeEntityDefinition owner) || owner.SelectableKind != SelectableKind.Building)
                throw new ArgumentException($"Construction action {building.StableKey} has no matching building entity.");
            ValidatePrerequisiteGroups(building.StableKey, owner.FactionKey, building.PrerequisiteGroups);
        }
        ValidateBuildingPrerequisiteDag(buildingsByKey);

        HashSet<uint> productionUnits = new();
        for (int i = 0; i < Production.Length; i++)
        {
            UnitProductionDefinition production = Production[i];
            if (!productionUnits.Add(production.UnitType.Value))
                throw new ArgumentException($"Duplicate production definition {production.UnitStableKey}.");
            if (!TryGetEntity(production.UnitType, out PrototypeEntityDefinition unit) || unit.SelectableKind == SelectableKind.Building ||
                !string.Equals(unit.StableKey, production.UnitStableKey, StringComparison.Ordinal))
                throw new ArgumentException($"Production {production.UnitStableKey} references a missing unit.");
            if (unit.OperationsCapacity != production.OperationsCapacity)
                throw new ArgumentException($"Production {production.UnitStableKey} Operations Capacity disagrees with the unit definition.");
            HashSet<uint> producerIds = new();
            for (int producer = 0; producer < production.ProducerTypes.Length; producer++)
            {
                ContentId producerType = production.ProducerTypes[producer];
                if (!producerIds.Add(producerType.Value))
                    throw new ArgumentException($"Production {production.UnitStableKey} repeats producer {production.ProducerStableKeys[producer]}.");
                if (!TryGetBuilding(producerType, out BuildingDefinition producerBuilding) ||
                    !TryGetEntity(producerType, out PrototypeEntityDefinition producerEntity) ||
                    producerEntity.SelectableKind != SelectableKind.Building ||
                    !string.Equals(producerBuilding.StableKey, production.ProducerStableKeys[producer], StringComparison.Ordinal) ||
                    !string.Equals(producerEntity.FactionKey, unit.FactionKey, StringComparison.Ordinal))
                    throw new ArgumentException($"Production {production.UnitStableKey} references missing producer {production.ProducerStableKeys[producer]}.");
            }
            ValidatePrerequisiteGroups(production.UnitStableKey, unit.FactionKey, production.PrerequisiteGroups);
        }
    }

    private void ValidatePrerequisiteGroups(string owner, string ownerFaction, ContentActionPrerequisiteGroup[] groups)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            ContentActionPrerequisite[] alternatives = groups[groupIndex].Alternatives;
            if (alternatives == null || alternatives.Length == 0) throw new ArgumentException($"Action {owner} has an empty prerequisite group.");
            for (int alternativeIndex = 0; alternativeIndex < alternatives.Length; alternativeIndex++)
            {
                ContentActionPrerequisite requirement = alternatives[alternativeIndex];
                string uniqueKey = ((byte)requirement.Kind).ToString() + ":" + requirement.TargetStableKey;
                if (!seen.Add(uniqueKey)) throw new ArgumentException($"Action {owner} repeats prerequisite {requirement.TargetStableKey}.");
                bool resolved;
                string targetFaction;
                switch (requirement.Kind)
                {
                    case ContentActionPrerequisiteKind.Building:
                        PrototypeEntityDefinition buildingEntity = default;
                        resolved = TryGetBuilding(requirement.TargetId, out BuildingDefinition building) &&
                            string.Equals(building.StableKey, requirement.TargetStableKey, StringComparison.Ordinal) &&
                            TryGetEntity(requirement.TargetId, out buildingEntity) &&
                            buildingEntity.SelectableKind == SelectableKind.Building;
                        targetFaction = resolved ? buildingEntity.FactionKey ?? string.Empty : string.Empty;
                        break;
                    case ContentActionPrerequisiteKind.Research:
                        resolved = TryGetResearch(requirement.TargetId, out ResearchDefinition research) &&
                            string.Equals(research.StableKey, requirement.TargetStableKey, StringComparison.Ordinal);
                        targetFaction = resolved ? research.FactionKey ?? string.Empty : string.Empty;
                        break;
                    default:
                        resolved = false;
                        targetFaction = string.Empty;
                        break;
                }
                if (!resolved) throw new ArgumentException($"Action {owner} references missing prerequisite {requirement.TargetStableKey}.");
                if (!string.Equals(targetFaction, ownerFaction, StringComparison.Ordinal))
                    throw new ArgumentException($"Action {owner} references cross-faction prerequisite {requirement.TargetStableKey}.");
            }
        }
    }

    private static void ValidateBuildingPrerequisiteDag(Dictionary<string, BuildingDefinition> byKey)
    {
        Dictionary<string, byte> state = new(StringComparer.Ordinal);
        foreach (string key in byKey.Keys) VisitBuildingPrerequisites(key, byKey, state);
    }

    private static void VisitBuildingPrerequisites(string key, Dictionary<string, BuildingDefinition> byKey, Dictionary<string, byte> state)
    {
        if (state.TryGetValue(key, out byte existing))
        {
            if (existing == 1) throw new ArgumentException($"Construction prerequisite cycle includes {key}.");
            if (existing == 2) return;
        }
        state[key] = 1;
        BuildingDefinition definition = byKey[key];
        for (int groupIndex = 0; groupIndex < definition.PrerequisiteGroups.Length; groupIndex++)
        {
            ContentActionPrerequisite[] alternatives = definition.PrerequisiteGroups[groupIndex].Alternatives;
            for (int i = 0; i < alternatives.Length; i++)
            {
                ContentActionPrerequisite prerequisite = alternatives[i];
                if (prerequisite.Kind == ContentActionPrerequisiteKind.Building && byKey.ContainsKey(prerequisite.TargetStableKey))
                    VisitBuildingPrerequisites(prerequisite.TargetStableKey, byKey, state);
            }
        }
        state[key] = 2;
    }

    private void ValidateTransformationMode(TransformationDefinition transformation, PrototypeEntityDefinition entity, TransformationModeDefinition mode, bool authoredMode)
    {
        if (!TryGetMovement(mode.MovementProfileKey, out _)) throw new ArgumentException($"Transformation {transformation.StableKey} references missing movement {mode.MovementProfileKey}.");
        if (!TryGetWeapon(mode.Combat.WeaponProfile, out WeaponDefinition weapon)) throw new ArgumentException($"Transformation {transformation.StableKey} references a missing weapon.");
        if (mode.Combat.LegalTargetLayers != weapon.LegalTargetLayers || mode.Combat.LegalTargetClasses != weapon.LegalTargetClasses || mode.Combat.PriorityProfile != weapon.PriorityProfile)
            throw new ArgumentException($"Transformation {transformation.StableKey} mode targeting disagrees with its weapon.");
        if (authoredMode && (entity.MovementProfileKey != mode.MovementProfileKey || entity.Footprint != mode.Footprint || entity.VisionRadius != mode.VisionRadius ||
            entity.Combat.TargetClass != mode.Combat.TargetClass || entity.Combat.TargetLayer != mode.Combat.TargetLayer || entity.Combat.ArmorRating != mode.Combat.ArmorRating ||
            entity.Combat.WeaponProfile != mode.Combat.WeaponProfile))
            throw new ArgumentException($"Transformation {transformation.StableKey} mode A must match the authored entity spawn state.");
    }
}

public static class PrototypeContentFactory
{
    /// <summary>Built-in mirror of the checked-in prototype source data; used by headless/debug startup before generated files are present.</summary>
    public static PrototypeContentCatalog CreateM2Catalog()
    {
        PrototypeMovementProfile[] profiles =
        {
            new PrototypeMovementProfile("movement.aliens.alien_jet", Fix32.FromRatio(310,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.aliens.alien_mothership", Fix32.FromRatio(165,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 410, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.aliens.etx_alien_infiltrator", Fix32.FromRatio(205,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.aliens.etx_alien_strike", Fix32.FromRatio(235,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.aliens.etx_servitor", Fix32.FromRatio(165,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.aliens.razor_skimmer", Fix32.FromRatio(230,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.astronauts.expedition_crew", Fix32.FromRatio(140,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.mission_fighter", Fix32.FromRatio(315,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.astronauts.mobile_mining_platform", Fix32.FromRatio(95,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.mono_jet", Fix32.FromRatio(285,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.astronauts.mt101_armored_drilling_unit", Fix32.FromRatio(108,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.mt201_ultra_drill_walker", Fix32.FromRatio(92,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 592, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.mt51_claw_tank", Fix32.FromRatio(130,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.mx71_recon_dropship", Fix32.FromRatio(215,100), Fix32.FromRatio(5,2), Fix32.FromRatio(25,8), 865, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.astronauts.mx81_operations_aircraft", Fix32.FromRatio(190,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 592, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.astronauts.rover", Fix32.FromRatio(215,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.solar_explorer", Fix32.FromRatio(110,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.astronauts.t3_trike", Fix32.FromRatio(170,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.martians.aero_skiff", Fix32.FromRatio(260,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.martians.double_hover", Fix32.FromRatio(225,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.martians.excavation_searcher", Fix32.FromRatio(98,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 592, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.martians.jet_scooter", Fix32.FromRatio(240,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.martians.recon_mech_rp", Fix32.FromRatio(132,100), Fix32.FromInt(4), Fix32.FromInt(5), 865, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.martians.red_planet_cruiser", Fix32.FromRatio(150,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.martians.red_planet_protector", Fix32.FromRatio(118,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.martians.worker_robot", Fix32.FromRatio(130,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.chrome_crusher", Fix32.FromRatio(92,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 592, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.crew", Fix32.FromRatio(135,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.hover_scout", Fix32.FromRatio(225,100), Fix32.FromInt(6), Fix32.FromRatio(15,2), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.prototype.loader_dozer", Fix32.FromRatio(130,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.mx41_flight", Fix32.FromRatio(245,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.None, MovementLayer.TrueAir),
            new PrototypeMovementProfile("movement.prototype.mx41_ground", Fix32.FromRatio(175,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.nav_huge", Fix32.FromRatio(90,100), Fix32.FromRatio(9,10), Fix32.FromRatio(9,8), 410, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.prototype.rapid_rider", Fix32.FromRatio(210,100), Fix32.FromInt(4), Fix32.FromInt(5), 1638, ReversePolicy.Full, MovementLayer.GroundHover),
            new PrototypeMovementProfile("movement.prototype.static", Fix32.Zero, Fix32.Zero, Fix32.Zero, 0, ReversePolicy.None, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.rock_raiders.drill_craft", Fix32.FromRatio(155,100), Fix32.FromInt(4), Fix32.FromInt(5), 1229, ReversePolicy.Reduced, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.rock_raiders.granite_grinder", Fix32.FromRatio(115,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 865, ReversePolicy.Full, MovementLayer.Ground),
            new PrototypeMovementProfile("movement.rock_raiders.tunnel_transport", Fix32.FromRatio(175,100), Fix32.FromRatio(3,2), Fix32.FromRatio(15,8), 592, ReversePolicy.None, MovementLayer.TrueAir)
        };
        WeaponDefinition[] weapons =
        {
            new WeaponDefinition("weapon.ast.mx41.flight_pulse", TargetLayerMask.All, TargetClassMask.All, TargetPriorityProfile.Generalist, 16, DamageType.General, 23, Fix32.FromRatio(9,2), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
            new WeaponDefinition("weapon.ast.mx41.pursuit_projector", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.AntiLight, 20, DamageType.Light, 21, Fix32.FromRatio(9,2), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
            new WeaponDefinition("weapon.rr.chrome_crusher.chrome_drill", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Siege, 55, DamageType.Siege, 32, Fix32.FromRatio(21,20), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 30, 3_500),
            new WeaponDefinition("weapon.rr.crew.portable_mining_tool", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Support, 6, DamageType.General, 24, Fix32.FromRatio(4,5), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 45, 3_500),
            new WeaponDefinition("weapon.rr.hover_scout.survey_pulse", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Scout, 6, DamageType.General, 30, Fix32.FromInt(3), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
            new WeaponDefinition("weapon.rr.loader_dozer.scoop_ram", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.AntiLight, 18, DamageType.General, 27, Fix32.FromRatio(9,10), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 45, 3_500)
        };
        PrototypeEntityDefinition[] entities =
        {
            new PrototypeEntityDefinition("building.ali.etx_command_core", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ali.etx_command_core", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command, 2400, 3)),
            new PrototypeEntityDefinition("building.ali.etx_defense_node", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.ali.etx_defense_node", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 700, 1)),
            new PrototypeEntityDefinition("building.ali.etx_fabricator", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.ali.etx_fabricator", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1350, 2)),
            new PrototypeEntityDefinition("building.ali.power_coupler", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.ali.power_coupler", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 850, 1)),
            new PrototypeEntityDefinition("building.ali.reconfiguration_dock", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ali.reconfiguration_dock", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production | CombatTargetFlags.Support, 1600, 2)),
            new PrototypeEntityDefinition("building.ali.resonance_core", "Aliens", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.ali.resonance_core", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure | CombatTargetFlags.Support, 1200, 2)),
            new PrototypeEntityDefinition("building.ast.field_systems_garage", "Astronauts", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ast.field_systems_garage", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1450, 2)),
            new PrototypeEntityDefinition("building.ast.flight_operations_pad", "Astronauts", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ast.flight_operations_pad", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1400, 2)),
            new PrototypeEntityDefinition("building.ast.frontier_extraction_station", "Astronauts", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.ast.frontier_extraction_station", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1250, 2)),
            new PrototypeEntityDefinition("building.ast.mb01_eagle_command_base", "Astronauts", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ast.mb01_eagle_command_base", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command, 2700, 4)),
            new PrototypeEntityDefinition("building.ast.mission_vehicle_bay", "Astronauts", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ast.mission_vehicle_bay", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1700, 3)),
            new PrototypeEntityDefinition("building.ast.modular_sentinel_defense", "Astronauts", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.ast.modular_sentinel_defense", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 800, 2)),
            new PrototypeEntityDefinition("building.ast.service_refit_hub", "Astronauts", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.ast.service_refit_hub", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production | CombatTargetFlags.Support, 1650, 3)),
            new PrototypeEntityDefinition("building.ast.solar_energy_array", "Astronauts", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.ast.solar_energy_array", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 900, 1)),
            new PrototypeEntityDefinition("building.mar.aero_guard_tower", "Martians", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.mar.aero_guard_tower", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 760, 2)),
            new PrototypeEntityDefinition("building.mar.aero_tube_hangar", "Martians", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.mar.aero_tube_hangar", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command | CombatTargetFlags.Production, 2800, 4)),
            new PrototypeEntityDefinition("building.mar.aero_tube_link", "Martians", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Tiny, SelectableKind.Building, 0, "view.placeholder.mar.aero_tube_link", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure | CombatTargetFlags.Support, 320, 1)),
            new PrototypeEntityDefinition("building.mar.deflector_arm", "Martians", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.mar.deflector_arm", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 850, 3)),
            new PrototypeEntityDefinition("building.mar.excavation_plant", "Martians", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.mar.excavation_plant", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1250, 2)),
            new PrototypeEntityDefinition("building.mar.mechanical_workshop", "Martians", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.mar.mechanical_workshop", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production | CombatTargetFlags.Support, 1450, 2)),
            new PrototypeEntityDefinition("building.mar.pressure_generator", "Martians", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.mar.pressure_generator", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 900, 1)),
            new PrototypeEntityDefinition("building.mar.routing_laboratory", "Martians", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.mar.routing_laboratory", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production | CombatTargetFlags.Support, 1500, 2)),
            new PrototypeEntityDefinition("building.mar.settlement_station", "Martians", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.mar.settlement_station", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command | CombatTargetFlags.EconomicInfrastructure, 1900, 3)),
            new PrototypeEntityDefinition("building.rock_raiders.crusher_barrier", "RockRaiders", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.rock_raiders.crusher_barrier", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 1100, 4)),
            new PrototypeEntityDefinition("building.rock_raiders.crystal_vault", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.rock_raiders.crystal_vault", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1600, 4)),
            new PrototypeEntityDefinition("building.rock_raiders.cutter_mast", "RockRaiders", "NEW_GAME_CONTENT", "movement.prototype.static", FootprintClass.Small, SelectableKind.Building, 0, "view.placeholder.rock_raiders.cutter_mast", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.DefensiveStructure, 800, 2)),
            new PrototypeEntityDefinition("building.rock_raiders.engineering_workshop", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.engineering_workshop", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1900, 3)),
            new PrototypeEntityDefinition("building.rock_raiders.hq", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.hq", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.FortifiedStructure, CombatTargetLayer.Ground, CombatTargetFlags.Command, 3000, 5)),
            new PrototypeEntityDefinition("building.rock_raiders.ore_processing_plant", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.rock_raiders.ore_processing_plant", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1350, 2)),
            new PrototypeEntityDefinition("building.rock_raiders.power_station", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Large, SelectableKind.Building, 0, "view.placeholder.rock_raiders.power_station", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.EconomicInfrastructure, 1000, 1)),
            new PrototypeEntityDefinition("building.rock_raiders.vehicle_service_bay", "RockRaiders", "COMPOSITE_ADAPTED", "movement.prototype.static", FootprintClass.Huge, SelectableKind.Building, 0, "view.placeholder.rock_raiders.vehicle_service_bay", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.Structure, CombatTargetLayer.Ground, CombatTargetFlags.Production, 1700, 3)),
            new PrototypeEntityDefinition("prototype.nav.huge", "Technical", "ENGINEERING_ONLY", "movement.prototype.nav_huge", FootprintClass.Huge, SelectableKind.CombatSupport, 8, "view.placeholder.navigation.huge", 0, 0, 0, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.None, 880, 5)),
            new PrototypeEntityDefinition("unit.aliens.alien_jet", "Aliens", "OFFICIAL_DIRECT", "movement.aliens.alien_jet", FootprintClass.Small, SelectableKind.CombatSupport, 12, "view.placeholder.aliens.alien_jet", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat, 180, 0)),
            new PrototypeEntityDefinition("unit.aliens.alien_mothership", "Aliens", "OFFICIAL_ADAPTED", "movement.aliens.alien_mothership", FootprintClass.Huge, SelectableKind.CombatSupport, 14, "view.placeholder.aliens.alien_mothership", 0, 0, 8, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat | CombatTargetFlags.Transport | CombatTargetFlags.Support, 1200, 4)),
            new PrototypeEntityDefinition("unit.aliens.etx_alien_infiltrator", "Aliens", "OFFICIAL_ADAPTED", "movement.aliens.etx_alien_infiltrator", FootprintClass.Medium, SelectableKind.CombatSupport, 13, "view.placeholder.aliens.etx_alien_infiltrator", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 360, 2)),
            new PrototypeEntityDefinition("unit.aliens.etx_alien_strike", "Aliens", "OFFICIAL_ADAPTED", "movement.aliens.etx_alien_strike", FootprintClass.Medium, SelectableKind.CombatSupport, 10, "view.placeholder.aliens.etx_alien_strike", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat, 330, 1)),
            new PrototypeEntityDefinition("unit.aliens.etx_servitor", "Aliens", "NEW_GAME_CONTENT", "movement.aliens.etx_servitor", FootprintClass.Tiny, SelectableKind.Worker, 8, "view.placeholder.aliens.etx_servitor", 30, 8, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Worker | CombatTargetFlags.Support, 110, 0)),
            new PrototypeEntityDefinition("unit.aliens.razor_skimmer", "Aliens", "COMPOSITE_ADAPTED", "movement.aliens.razor_skimmer", FootprintClass.Small, SelectableKind.CombatSupport, 10, "view.placeholder.aliens.razor_skimmer", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 210, 1)),
            new PrototypeEntityDefinition("unit.astronauts.expedition_crew", "Astronauts", "COMPOSITE_ADAPTED", "movement.astronauts.expedition_crew", FootprintClass.Tiny, SelectableKind.Worker, 8, "view.placeholder.astronauts.expedition_crew", 30, 8, 1, new PrototypeCombatProfile(CombatTargetClass.Personnel, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Worker | CombatTargetFlags.Support, 110, 0)),
            new PrototypeEntityDefinition("unit.astronauts.mission_fighter", "Astronauts", "COMPOSITE_ADAPTED", "movement.astronauts.mission_fighter", FootprintClass.Small, SelectableKind.CombatSupport, 11, "view.placeholder.astronauts.mission_fighter", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat, 240, 1)),
            new PrototypeEntityDefinition("unit.astronauts.mobile_mining_platform", "Astronauts", "COMPOSITE_ADAPTED", "movement.astronauts.mobile_mining_platform", FootprintClass.Large, SelectableKind.CombatSupport, 9, "view.placeholder.astronauts.mobile_mining_platform", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 400, 2)),
            new PrototypeEntityDefinition("unit.astronauts.mono_jet", "Astronauts", "OFFICIAL_DIRECT", "movement.astronauts.mono_jet", FootprintClass.Small, SelectableKind.CombatSupport, 13, "view.placeholder.astronauts.mono_jet", 0, 0, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat, 150, 0)),
            new PrototypeEntityDefinition("unit.astronauts.mt101_armored_drilling_unit", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.mt101_armored_drilling_unit", FootprintClass.Large, SelectableKind.CombatSupport, 9, "view.placeholder.astronauts.mt101_armored_drilling_unit", 0, 0, 5, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 700, 4)),
            new PrototypeEntityDefinition("unit.astronauts.mt201_ultra_drill_walker", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.mt201_ultra_drill_walker", FootprintClass.Huge, SelectableKind.CombatSupport, 9, "view.placeholder.astronauts.mt201_ultra_drill_walker", 0, 0, 6, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 760, 4)),
            new PrototypeEntityDefinition("unit.astronauts.mt51_claw_tank", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.mt51_claw_tank", FootprintClass.Large, SelectableKind.CombatSupport, 9, "view.placeholder.astronauts.mt51_claw_tank", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 460, 3)),
            new PrototypeEntityDefinition("unit.astronauts.mx41_switch_fighter", "Astronauts", "OFFICIAL_ADAPTED", "movement.prototype.mx41_ground", FootprintClass.Medium, SelectableKind.CombatSupport, 10, "view.placeholder.astronauts.mx41_switch_fighter", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 320, 2, TargetPriorityProfile.AntiLight, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(17,2), StableId.FromKey("weapon.ast.mx41.pursuit_projector"))),
            new PrototypeEntityDefinition("unit.astronauts.mx71_recon_dropship", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.mx71_recon_dropship", FootprintClass.Large, SelectableKind.CombatSupport, 12, "view.placeholder.astronauts.mx71_recon_dropship", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat | CombatTargetFlags.Transport | CombatTargetFlags.Support, 420, 2)),
            new PrototypeEntityDefinition("unit.astronauts.mx81_operations_aircraft", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.mx81_operations_aircraft", FootprintClass.Huge, SelectableKind.CombatSupport, 15, "view.placeholder.astronauts.mx81_operations_aircraft", 0, 0, 6, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 720, 3)),
            new PrototypeEntityDefinition("unit.astronauts.rover", "Astronauts", "OFFICIAL_DIRECT", "movement.astronauts.rover", FootprintClass.Small, SelectableKind.CombatSupport, 13, "view.placeholder.astronauts.rover", 0, 0, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 130, 0)),
            new PrototypeEntityDefinition("unit.astronauts.solar_explorer", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.solar_explorer", FootprintClass.Large, SelectableKind.CombatSupport, 11, "view.placeholder.astronauts.solar_explorer", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support | CombatTargetFlags.Transport, 520, 2)),
            new PrototypeEntityDefinition("unit.astronauts.t3_trike", "Astronauts", "OFFICIAL_ADAPTED", "movement.astronauts.t3_trike", FootprintClass.Medium, SelectableKind.CombatSupport, 9, "view.placeholder.astronauts.t3_trike", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 260, 1)),
            new PrototypeEntityDefinition("unit.martians.aero_skiff", "Martians", "COMPOSITE_ADAPTED", "movement.martians.aero_skiff", FootprintClass.Small, SelectableKind.CombatSupport, 12, "view.placeholder.martians.aero_skiff", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.Transport | CombatTargetFlags.Support, 190, 0)),
            new PrototypeEntityDefinition("unit.martians.double_hover", "Martians", "OFFICIAL_DIRECT", "movement.martians.double_hover", FootprintClass.Small, SelectableKind.CombatSupport, 12, "view.placeholder.martians.double_hover", 0, 0, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Support, 120, 0)),
            new PrototypeEntityDefinition("unit.martians.excavation_searcher", "Martians", "OFFICIAL_ADAPTED", "movement.martians.excavation_searcher", FootprintClass.Huge, SelectableKind.CombatSupport, 11, "view.placeholder.martians.excavation_searcher", 0, 0, 6, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 760, 4)),
            new PrototypeEntityDefinition("unit.martians.jet_scooter", "Martians", "OFFICIAL_DIRECT", "movement.martians.jet_scooter", FootprintClass.Small, SelectableKind.CombatSupport, 10, "view.placeholder.martians.jet_scooter", 0, 0, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 180, 0)),
            new PrototypeEntityDefinition("unit.martians.recon_mech_rp", "Martians", "OFFICIAL_ADAPTED", "movement.martians.recon_mech_rp", FootprintClass.Medium, SelectableKind.CombatSupport, 13, "view.placeholder.martians.recon_mech_rp", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 300, 2)),
            new PrototypeEntityDefinition("unit.martians.red_planet_cruiser", "Martians", "OFFICIAL_ADAPTED", "movement.martians.red_planet_cruiser", FootprintClass.Medium, SelectableKind.CombatSupport, 9, "view.placeholder.martians.red_planet_cruiser", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 400, 2)),
            new PrototypeEntityDefinition("unit.martians.red_planet_protector", "Martians", "OFFICIAL_ADAPTED", "movement.martians.red_planet_protector", FootprintClass.Large, SelectableKind.CombatSupport, 10, "view.placeholder.martians.red_planet_protector", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 560, 3)),
            new PrototypeEntityDefinition("unit.martians.worker_robot", "Martians", "OFFICIAL_ADAPTED", "movement.martians.worker_robot", FootprintClass.Tiny, SelectableKind.Worker, 8, "view.placeholder.martians.worker_robot", 30, 8, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Worker | CombatTargetFlags.Support, 115, 1)),
            new PrototypeEntityDefinition("unit.rock_raiders.chrome_crusher", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.chrome_crusher", FootprintClass.Large, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.chrome_crusher", 0, 0, 6, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 880, 5, TargetPriorityProfile.Siege, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(101,20), StableId.FromKey("weapon.rr.chrome_crusher.chrome_drill"))),
            new PrototypeEntityDefinition("unit.rock_raiders.crew", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.crew", FootprintClass.Tiny, SelectableKind.Worker, 8, "view.placeholder.rock_raiders.crew", 30, 8, 1, new PrototypeCombatProfile(CombatTargetClass.Personnel, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Worker | CombatTargetFlags.Support, 110, 0, TargetPriorityProfile.Support, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(24,5), StableId.FromKey("weapon.rr.crew.portable_mining_tool"))),
            new PrototypeEntityDefinition("unit.rock_raiders.drill_craft", "RockRaiders", "OFFICIAL_ADAPTED", "movement.rock_raiders.drill_craft", FootprintClass.Small, SelectableKind.CombatSupport, 8, "view.placeholder.rock_raiders.drill_craft", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 190, 1)),
            new PrototypeEntityDefinition("unit.rock_raiders.granite_grinder", "RockRaiders", "OFFICIAL_ADAPTED", "movement.rock_raiders.granite_grinder", FootprintClass.Medium, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.granite_grinder", 0, 0, 4, new PrototypeCombatProfile(CombatTargetClass.HeavyMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 480, 3)),
            new PrototypeEntityDefinition("unit.rock_raiders.hover_scout", "RockRaiders", "OFFICIAL_DIRECT", "movement.prototype.hover_scout", FootprintClass.Small, SelectableKind.CombatSupport, 12, "view.placeholder.rock_raiders.hover_scout", 0, 0, 1, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat | CombatTargetFlags.Support, 120, 0, TargetPriorityProfile.Scout, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromInt(7), StableId.FromKey("weapon.rr.hover_scout.survey_pulse"))),
            new PrototypeEntityDefinition("unit.rock_raiders.loader_dozer", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.loader_dozer", FootprintClass.Medium, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.loader_dozer", 0, 0, 3, new PrototypeCombatProfile(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 360, 2, TargetPriorityProfile.AntiLight, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(49,10), StableId.FromKey("weapon.rr.loader_dozer.scoop_ram"))),
            new PrototypeEntityDefinition("unit.rock_raiders.rapid_rider", "RockRaiders", "OFFICIAL_ADAPTED", "movement.prototype.rapid_rider", FootprintClass.Small, SelectableKind.CombatSupport, 9, "view.placeholder.rock_raiders.rapid_rider", 0, 0, 2, new PrototypeCombatProfile(CombatTargetClass.LightMachine, CombatTargetLayer.Ground, CombatTargetFlags.Transport, 170, 0)),
            new PrototypeEntityDefinition("unit.rock_raiders.tunnel_transport", "RockRaiders", "OFFICIAL_ADAPTED", "movement.rock_raiders.tunnel_transport", FootprintClass.Huge, SelectableKind.CombatSupport, 11, "view.placeholder.rock_raiders.tunnel_transport", 0, 0, 5, new PrototypeCombatProfile(CombatTargetClass.MassiveMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.Transport, 650, 3))
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
            new BuildingDefinition("building.ali.etx_command_core", 7, 7, 0x1FFFFFFFFFFFFUL, false, 230, 60, 800, operationsCapacityProvided: 16, energyGenerationPerSecond: 2, energyReserveCapacity: 150, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy),
            new BuildingDefinition("building.ali.etx_defense_node", 2, 2, 0xFUL, false, 120, 30, 600, continuousEnergyDemandPerSecond: 2),
            new BuildingDefinition("building.ali.etx_fabricator", 6, 6, 0xFFFFFFFFFUL, false, 140, 25, 640, operationsCapacityProvided: 4, continuousEnergyDemandPerSecond: 2, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch),
            new BuildingDefinition("building.ali.power_coupler", 4, 4, 0xFFFFUL, false, 130, 10, 560, energyGenerationPerSecond: 12, energyReserveCapacity: 100),
            new BuildingDefinition("building.ali.reconfiguration_dock", 8, 7, 0xFFFFFFFFFFFFFFUL, true, 210, 60, 1000, operationsCapacityProvided: 6, continuousEnergyDemandPerSecond: 4, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch, crystalCost: 1),
            new BuildingDefinition("building.ali.resonance_core", 4, 4, 0xFFFFUL, false, 160, 50, 800, continuousEnergyDemandPerSecond: 3, energyFunctionalClass: EnergyFunctionalClass.ServiceAndFactionSystems),
            new BuildingDefinition("building.ast.field_systems_garage", 7, 6, 0x3FFFFFFFFFFUL, true, 130, 10, 600, operationsCapacityProvided: 4, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch),
            new BuildingDefinition("building.ast.flight_operations_pad", 10, 8, ulong.MaxValue, true, 200, 45, 900, operationsCapacityProvided: 5, continuousEnergyDemandPerSecond: 3, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch, crystalCost: 1, footprintMaskHigh: 0xFFFFUL),
            new BuildingDefinition("building.ast.frontier_extraction_station", 6, 6, 0xFFFFFFFFFUL, false, 140, 15, 640, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ResourceProcessing),
            new BuildingDefinition("building.ast.mb01_eagle_command_base", 8, 8, ulong.MaxValue, false, 260, 40, 900, operationsCapacityProvided: 16, energyGenerationPerSecond: 2, energyReserveCapacity: 150, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy),
            new BuildingDefinition("building.ast.mission_vehicle_bay", 8, 7, 0xFFFFFFFFFFFFFFUL, true, 190, 35, 900, operationsCapacityProvided: 5, continuousEnergyDemandPerSecond: 2, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch, crystalCost: 1),
            new BuildingDefinition("building.ast.modular_sentinel_defense", 2, 2, 0xFUL, false, 110, 20, 560, continuousEnergyDemandPerSecond: 1),
            new BuildingDefinition("building.ast.service_refit_hub", 7, 7, 0x1FFFFFFFFFFFFUL, false, 170, 30, 760, operationsCapacityProvided: 4, continuousEnergyDemandPerSecond: 2, energyFunctionalClass: EnergyFunctionalClass.ServiceAndFactionSystems),
            new BuildingDefinition("building.ast.solar_energy_array", 6, 5, 0x3FFFFFFFUL, true, 120, 0, 560, energyGenerationPerSecond: 8, energyReserveCapacity: 100),
            new BuildingDefinition("building.mar.aero_guard_tower", 2, 2, 0xFUL, false, 120, 25, 560, continuousEnergyDemandPerSecond: 2),
            new BuildingDefinition("building.mar.aero_tube_hangar", 9, 9, ulong.MaxValue, false, 280, 40, 1000, operationsCapacityProvided: 16, energyGenerationPerSecond: 2, energyReserveCapacity: 150, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy, footprintMaskHigh: 0x1FFFFUL),
            new BuildingDefinition("building.mar.aero_tube_link", 1, 1, 0x1UL, false, 50, 10, 200, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ServiceAndFactionSystems),
            new BuildingDefinition("building.mar.deflector_arm", 3, 3, 0x1FFUL, false, 100, 15, 500, continuousEnergyDemandPerSecond: 1),
            new BuildingDefinition("building.mar.excavation_plant", 6, 6, 0xFFFFFFFFFUL, false, 130, 15, 600, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ResourceProcessing),
            new BuildingDefinition("building.mar.mechanical_workshop", 7, 6, 0x3FFFFFFFFFFUL, true, 150, 20, 700, operationsCapacityProvided: 5, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch),
            new BuildingDefinition("building.mar.pressure_generator", 4, 4, 0xFFFFUL, false, 130, 10, 600, energyGenerationPerSecond: 9, energyReserveCapacity: 100),
            new BuildingDefinition("building.mar.routing_laboratory", 6, 6, 0xFFFFFFFFFUL, false, 170, 35, 840, operationsCapacityProvided: 4, continuousEnergyDemandPerSecond: 2, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch, crystalCost: 1),
            new BuildingDefinition("building.mar.settlement_station", 7, 7, 0x1FFFFFFFFFFFFUL, false, 220, 30, 800, operationsCapacityProvided: 12, energyGenerationPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy),
            new BuildingDefinition("building.rock_raiders.crusher_barrier", 3, 1, 0x7UL, true, 90, 10, 480),
            new BuildingDefinition("building.rock_raiders.crystal_vault", 5, 5, 0x1FFFFFFUL, false, 180, 40, 900, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ResourceProcessing),
            new BuildingDefinition("building.rock_raiders.cutter_mast", 2, 2, 0xFUL, false, 120, 25, 600, continuousEnergyDemandPerSecond: 2),
            new BuildingDefinition("building.rock_raiders.engineering_workshop", 8, 8, ulong.MaxValue, false, 220, 50, 1100, operationsCapacityProvided: 6, continuousEnergyDemandPerSecond: 3, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch),
            new BuildingDefinition("building.rock_raiders.hq", 8, 8, ulong.MaxValue, false, 320, 40, 1200, productionExitWidth: 2, productionExitDepth: 2, productionExitFootprint: FootprintClass.Tiny, operationsCapacityProvided: 16, energyGenerationPerSecond: 2, energyReserveCapacity: 150, energyFunctionalClass: EnergyFunctionalClass.CommandAndBasicEconomy, worksiteServiceRadius: 18),
            new BuildingDefinition("building.rock_raiders.ore_processing_plant", 6, 6, 0xFFFFFFFFFUL, false, 140, 15, 600, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ResourceProcessing),
            new BuildingDefinition("building.rock_raiders.power_station", 5, 5, 0x1FFFFFFUL, false, 150, 20, 700, energyGenerationPerSecond: 10, energyReserveCapacity: 120),
            new BuildingDefinition("building.rock_raiders.vehicle_service_bay", 8, 6, 0xFFFFFFFFFFFFUL, true, 160, 20, 800, productionExitWidth: 3, productionExitDepth: 3, productionExitFootprint: FootprintClass.Medium, operationsCapacityProvided: 4, continuousEnergyDemandPerSecond: 1, energyFunctionalClass: EnergyFunctionalClass.ProductionAndResearch, worksiteServiceRadius: 12),
        };
        buildings = CanonicalActionDefinitions.BindBuildingPrerequisites(buildings);
        UnitProductionDefinition[] production = CanonicalActionDefinitions.CreateProduction();
        PrototypeCombatProfile mx41GroundCombat = new(CombatTargetClass.MediumMachine, CombatTargetLayer.Ground, CombatTargetFlags.CombatThreat, 320, 2,
            TargetPriorityProfile.AntiLight, TargetLayerMask.Ground, TargetClassMask.All, Fix32.FromRatio(17,2), StableId.FromKey("weapon.ast.mx41.pursuit_projector"));
        PrototypeCombatProfile mx41FlightCombat = new(CombatTargetClass.MediumMachine, CombatTargetLayer.TrueAir, CombatTargetFlags.CombatThreat, 320, 2,
            TargetPriorityProfile.Generalist, TargetLayerMask.All, TargetClassMask.All, Fix32.FromRatio(17,2), StableId.FromKey("weapon.ast.mx41.flight_pulse"));
        TransformationDefinition[] transformations =
        {
            new TransformationDefinition("transformation.astronauts.mx41_switch", "unit.astronauts.mx41_switch_fighter",
                new TransformationModeDefinition("state.astronauts.mx41.ground", "Ground", "movement.prototype.mx41_ground", FootprintClass.Medium, 10, "view.placeholder.astronauts.mx41_ground", mx41GroundCombat),
                new TransformationModeDefinition("state.astronauts.mx41.flight", "Flight", "movement.prototype.mx41_flight", FootprintClass.Medium, 10, "view.placeholder.astronauts.mx41_flight", mx41FlightCombat),
                45, 45, 4_000, 12, 160, false, false, TargetLayerMask.All)
        };
        ResearchDefinition[] research = CanonicalResearchDefinitions.Create();
        PrototypeContentCatalog catalog = new PrototypeContentCatalog(profiles, entities, resources, buildings, production, weapons, transformations, research, CanonicalCommandDefinitions.Create());
        PrototypeContentCodec.Write(catalog);
        return catalog;
    }
}


public static class PrototypeContentCodec
{
    private const int Magic = 0x4350534C; // LSPC little-endian bytes.
    public const int FormatVersion = 19;

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
            writer.Write(b.StableKey); writer.Write(b.Id.Value); writer.Write(b.FootprintWidth); writer.Write(b.FootprintHeight); writer.Write(b.FootprintMask); writer.Write(b.FootprintMaskHigh); writer.Write(b.Rotatable);
            writer.Write(b.OreCost); writer.Write(b.EnergyCost); writer.Write(b.CrystalCost); writer.Write(b.BuildTicks); writer.Write(b.ProductionExitWidth); writer.Write(b.ProductionExitDepth); writer.Write((byte)b.ProductionExitFootprint);
            writer.Write(b.OperationsCapacityProvided);
            writer.Write(b.EnergyGenerationPerSecond); writer.Write(b.EnergyReserveCapacity); writer.Write(b.ContinuousEnergyDemandPerSecond);
            writer.Write((byte)b.EnergyFunctionalClass);
            writer.Write(b.WorksiteServiceRadius);
            WriteActionPrerequisites(writer, b.PrerequisiteGroups);
        }
        writer.Write(catalog.Production.Length);
        for (int i = 0; i < catalog.Production.Length; i++)
        {
            UnitProductionDefinition p = catalog.Production[i];
            writer.Write(p.UnitStableKey); writer.Write(p.UnitType.Value); writer.Write(p.ProducerStableKeys.Length);
            for (int producer = 0; producer < p.ProducerStableKeys.Length; producer++)
            {
                writer.Write(p.ProducerStableKeys[producer]); writer.Write(p.ProducerTypes[producer].Value);
            }
            writer.Write(p.OreCost); writer.Write(p.EnergyCost); writer.Write(p.CrystalCost); writer.Write(p.OperationsCapacity); writer.Write(p.BuildTicks);
            WriteActionPrerequisites(writer, p.PrerequisiteGroups);
        }
        writer.Write(catalog.Weapons.Length);
        for (int i = 0; i < catalog.Weapons.Length; i++)
        {
            WeaponDefinition weapon = catalog.Weapons[i];
            writer.Write(weapon.StableKey); writer.Write(weapon.Id.Value); writer.Write((byte)weapon.LegalTargetLayers); writer.Write((byte)weapon.LegalTargetClasses);
            writer.Write((byte)weapon.PriorityProfile); writer.Write(weapon.BaseDamage); writer.Write((byte)weapon.DamageType); writer.Write(weapon.CooldownTicks);
            writer.Write(weapon.Range.Raw); writer.Write(weapon.MinimumRange.Raw); writer.Write((byte)weapon.DeliveryKind); writer.Write(weapon.ProjectileSpeed.Raw); writer.Write(weapon.RequiresLineOfSight);
            writer.Write(weapon.FacingToleranceAngle16); writer.Write(weapon.MaximumMovingFireSpeedBasisPoints);
        }
        writer.Write(catalog.Transformations.Length);
        for (int i = 0; i < catalog.Transformations.Length; i++) WriteTransformation(writer, catalog.Transformations[i]);
        writer.Write(catalog.Research.Length);
        for (int i = 0; i < catalog.Research.Length; i++) WriteResearch(writer, catalog.Research[i]);
        writer.Write(catalog.Commands.Length);
        for (int i = 0; i < catalog.Commands.Length; i++) WriteCommand(writer, catalog.Commands[i]);
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
                byte width = reader.ReadByte(), height = reader.ReadByte(); ulong mask = reader.ReadUInt64();
                ulong maskHigh = formatVersion >= 17 ? reader.ReadUInt64() : 0UL;
                bool rotatable = reader.ReadBoolean();
                ushort oreCost = reader.ReadUInt16(), energyCost = reader.ReadUInt16();
                byte crystalCost = formatVersion >= 17 ? reader.ReadByte() : (byte)0;
                ushort buildTicks = reader.ReadUInt16();
                byte exitWidth = reader.ReadByte(), exitDepth = reader.ReadByte(); FootprintClass exitFootprint = (FootprintClass)reader.ReadByte();
                byte capacityProvided = formatVersion >= 7 ? reader.ReadByte() : LegacyOperationsCapacityProvided(key);
                LegacyEnergyDefinition(key, out ushort generation, out ushort reserveCapacity, out ushort demand);
                if (formatVersion >= 8) { generation = reader.ReadUInt16(); reserveCapacity = reader.ReadUInt16(); demand = reader.ReadUInt16(); }
                EnergyFunctionalClass functionalClass = formatVersion >= 9 ? (EnergyFunctionalClass)reader.ReadByte() : LegacyEnergyFunctionalClass(key);
                byte worksiteServiceRadius = formatVersion >= 16 ? reader.ReadByte() : LegacyWorksiteServiceRadius(key);
                ContentActionPrerequisiteGroup[] prerequisites = formatVersion >= 19 ? ReadActionPrerequisites(reader) : Array.Empty<ContentActionPrerequisiteGroup>();
                buildings[i] = new BuildingDefinition(key, width, height, mask, rotatable, oreCost, energyCost, buildTicks, exitWidth, exitDepth, exitFootprint, capacityProvided, generation, reserveCapacity, demand, functionalClass, worksiteServiceRadius, crystalCost, maskHigh, prerequisites);
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
                string unitKey = reader.ReadString(); uint unitId = reader.ReadUInt32();
                if (formatVersion >= 19)
                {
                    int producerCount = reader.ReadInt32();
                    if (producerCount <= 0 || producerCount > 16) throw new InvalidDataException("Invalid production producer count.");
                    string[] producerKeys = new string[producerCount]; uint[] producerIds = new uint[producerCount];
                    for (int producer = 0; producer < producerCount; producer++) { producerKeys[producer] = reader.ReadString(); producerIds[producer] = reader.ReadUInt32(); }
                    ushort ore = reader.ReadUInt16(), energy = reader.ReadUInt16(); byte crystals = reader.ReadByte(), capacity = reader.ReadByte(); ushort ticks = reader.ReadUInt16();
                    ContentActionPrerequisiteGroup[] prerequisites = ReadActionPrerequisites(reader);
                    production[i] = new UnitProductionDefinition(unitKey, producerKeys, ore, energy, crystals, capacity, ticks, prerequisites);
                    for (int producer = 0; producer < producerCount; producer++)
                        if (production[i].ProducerTypes[producer].Value != producerIds[producer]) throw new InvalidDataException("Stable production producer ID mismatch.");
                }
                else
                {
                    string producerKey = reader.ReadString(); uint producerId = reader.ReadUInt32();
                    production[i] = new UnitProductionDefinition(unitKey, producerKey, reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadByte(), reader.ReadByte(), reader.ReadUInt16());
                    if (production[i].ProducerType.Value != producerId) throw new InvalidDataException("Stable production producer ID mismatch.");
                }
                if (production[i].UnitType.Value != unitId) throw new InvalidDataException("Stable production unit ID mismatch.");
            }
        }
        WeaponDefinition[] weapons = formatVersion >= 11 ? ReadWeapons(reader, formatVersion >= 12, formatVersion >= 14) : LegacyWeapons();
        TransformationDefinition[] transformations = formatVersion >= 15 ? ReadTransformations(reader) : Array.Empty<TransformationDefinition>();
        ResearchDefinition[] research = formatVersion >= 18 ? ReadResearch(reader) : Array.Empty<ResearchDefinition>();
        CommandDefinition[] commands = formatVersion >= 19 ? ReadCommands(reader) : Array.Empty<CommandDefinition>();
        if (stream.Position != stream.Length) throw new InvalidDataException("Trailing prototype content bytes.");
        PrototypeContentCatalog result = new PrototypeContentCatalog(profiles, entities, resourceNodes, buildings, production, weapons, transformations, research, commands) { ContentHash = DeterministicHash.Fnv1A64(bytes) };
        return result;
    }

    private static void WriteActionPrerequisites(BinaryWriter writer, ContentActionPrerequisiteGroup[] groups)
    {
        writer.Write(groups.Length);
        for (int group = 0; group < groups.Length; group++)
        {
            ContentActionPrerequisite[] alternatives = groups[group].Alternatives;
            writer.Write(alternatives.Length);
            for (int alternative = 0; alternative < alternatives.Length; alternative++)
            {
                ContentActionPrerequisite prerequisite = alternatives[alternative];
                writer.Write((byte)prerequisite.Kind); writer.Write(prerequisite.TargetStableKey); writer.Write(prerequisite.TargetId.Value);
            }
        }
    }

    private static ContentActionPrerequisiteGroup[] ReadActionPrerequisites(BinaryReader reader)
    {
        int groupCount = reader.ReadInt32();
        if (groupCount < 0 || groupCount > 32) throw new InvalidDataException("Invalid action prerequisite group count.");
        ContentActionPrerequisiteGroup[] groups = new ContentActionPrerequisiteGroup[groupCount];
        for (int group = 0; group < groupCount; group++)
        {
            int alternativeCount = reader.ReadInt32();
            if (alternativeCount <= 0 || alternativeCount > 32) throw new InvalidDataException("Invalid action prerequisite alternative count.");
            ContentActionPrerequisite[] alternatives = new ContentActionPrerequisite[alternativeCount];
            for (int alternative = 0; alternative < alternativeCount; alternative++)
            {
                ContentActionPrerequisiteKind kind = (ContentActionPrerequisiteKind)reader.ReadByte(); string key = reader.ReadString(); uint id = reader.ReadUInt32();
                alternatives[alternative] = new ContentActionPrerequisite(kind, key);
                if (alternatives[alternative].TargetId.Value != id) throw new InvalidDataException("Stable action prerequisite ID mismatch.");
            }
            groups[group] = new ContentActionPrerequisiteGroup(alternatives);
        }
        return groups;
    }

    private static void WriteCommand(BinaryWriter writer, CommandDefinition command)
    {
        writer.Write(command.StableKey); writer.Write(command.Id.Value); writer.Write((ushort)command.CommandType);
        writer.Write(command.EligibleEntityTags.Length);
        for (int i = 0; i < command.EligibleEntityTags.Length; i++) writer.Write(command.EligibleEntityTags[i]);
        writer.Write((byte)command.TargetType); writer.Write((byte)command.QueuePolicy);
        writer.Write(command.RequiredResearchStableKey); writer.Write(command.RequiredResearch.Value);
        writer.Write(command.ValidationHandlerId); writer.Write(command.ExecutionHandlerId); writer.Write(command.UiSlotProfile); writer.Write(command.TargetingPreviewProfile);
    }

    private static CommandDefinition[] ReadCommands(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > 256) throw new InvalidDataException("Invalid command definition count.");
        CommandDefinition[] commands = new CommandDefinition[count];
        for (int commandIndex = 0; commandIndex < count; commandIndex++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); SimCommandType type = (SimCommandType)reader.ReadUInt16();
            int tagCount = reader.ReadInt32();
            if (tagCount <= 0 || tagCount > 32) throw new InvalidDataException("Invalid command eligibility tag count.");
            string[] tags = new string[tagCount]; for (int tag = 0; tag < tagCount; tag++) tags[tag] = reader.ReadString();
            CommandTargetType target = (CommandTargetType)reader.ReadByte(); CommandQueuePolicy queue = (CommandQueuePolicy)reader.ReadByte();
            string requiredResearch = reader.ReadString(); uint requiredResearchId = reader.ReadUInt32();
            commands[commandIndex] = new CommandDefinition(key, type, tags, target, queue, requiredResearch,
                reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString());
            if (commands[commandIndex].Id.Value != id || commands[commandIndex].RequiredResearch.Value != requiredResearchId)
                throw new InvalidDataException("Stable command ID mismatch.");
        }
        return commands;
    }

    private static void WriteResearch(BinaryWriter writer, ResearchDefinition definition)
    {
        writer.Write(definition.StableKey); writer.Write(definition.Id.Value); writer.Write(definition.FactionKey); writer.Write((byte)definition.Categories);
        writer.Write(definition.SourceBuildingStableKey); writer.Write(definition.SourceBuildingType.Value);
        writer.Write(definition.OreCost); writer.Write(definition.EnergyCost); writer.Write(definition.CrystalCost); writer.Write(definition.ResearchTicks);
        writer.Write(definition.PrerequisiteGroups.Length);
        for (int groupIndex = 0; groupIndex < definition.PrerequisiteGroups.Length; groupIndex++)
        {
            ResearchPrerequisiteDefinition[] alternatives = definition.PrerequisiteGroups[groupIndex].Alternatives;
            writer.Write(alternatives.Length);
            for (int i = 0; i < alternatives.Length; i++)
            {
                ResearchPrerequisiteDefinition prerequisite = alternatives[i];
                writer.Write((byte)prerequisite.Kind); writer.Write(prerequisite.TargetStableKey); writer.Write(prerequisite.TargetId.Value);
                writer.Write(prerequisite.MinimumValue); writer.Write((byte)prerequisite.Persistence);
            }
        }
        writer.Write(definition.UnlockTags.Length);
        for (int i = 0; i < definition.UnlockTags.Length; i++)
        {
            writer.Write(definition.UnlockTags[i]); writer.Write(StableId.FromKey(definition.UnlockTags[i]).Value);
        }
        writer.Write(definition.ParameterModifiers.Length);
        for (int i = 0; i < definition.ParameterModifiers.Length; i++)
        {
            ResearchParameterModifier modifier = definition.ParameterModifiers[i];
            writer.Write(modifier.TargetStableKey); writer.Write(modifier.TargetId.Value); writer.Write((byte)modifier.Operation); writer.Write(modifier.Value);
        }
        writer.Write(definition.MutuallyExclusiveGroupKey); writer.Write(definition.PresentationProfileKey); writer.Write(definition.DisplayNameLocKey);
    }

    private static ResearchDefinition[] ReadResearch(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > 1024) throw new InvalidDataException("Invalid research definition count.");
        ResearchDefinition[] definitions = new ResearchDefinition[count];
        for (int definitionIndex = 0; definitionIndex < count; definitionIndex++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); string faction = reader.ReadString(); ResearchCategory categories = (ResearchCategory)reader.ReadByte();
            string sourceBuilding = reader.ReadString(); uint sourceBuildingId = reader.ReadUInt32();
            ushort ore = reader.ReadUInt16(), energy = reader.ReadUInt16(); byte crystals = reader.ReadByte(); ushort ticks = reader.ReadUInt16();
            int groupCount = reader.ReadInt32();
            if (groupCount < 0 || groupCount > 256) throw new InvalidDataException("Invalid research prerequisite group count.");
            ResearchPrerequisiteGroup[] groups = new ResearchPrerequisiteGroup[groupCount];
            for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
            {
                int alternativeCount = reader.ReadInt32();
                if (alternativeCount <= 0 || alternativeCount > 256) throw new InvalidDataException("Invalid research prerequisite alternative count.");
                ResearchPrerequisiteDefinition[] alternatives = new ResearchPrerequisiteDefinition[alternativeCount];
                for (int i = 0; i < alternativeCount; i++)
                {
                    ResearchPrerequisiteKind kind = (ResearchPrerequisiteKind)reader.ReadByte(); string target = reader.ReadString(); uint targetId = reader.ReadUInt32();
                    alternatives[i] = new ResearchPrerequisiteDefinition(kind, target, reader.ReadUInt16(), (ResearchPrerequisitePersistence)reader.ReadByte());
                    if (alternatives[i].TargetId.Value != targetId) throw new InvalidDataException("Stable research prerequisite ID mismatch.");
                }
                groups[groupIndex] = new ResearchPrerequisiteGroup(alternatives);
            }
            int unlockCount = reader.ReadInt32();
            if (unlockCount < 0 || unlockCount > 1024) throw new InvalidDataException("Invalid research unlock count.");
            string[] unlocks = new string[unlockCount];
            for (int i = 0; i < unlockCount; i++)
            {
                unlocks[i] = reader.ReadString(); uint unlockId = reader.ReadUInt32();
                if (StableId.FromKey(unlocks[i]).Value != unlockId) throw new InvalidDataException("Stable research unlock ID mismatch.");
            }
            int modifierCount = reader.ReadInt32();
            if (modifierCount < 0 || modifierCount > 1024) throw new InvalidDataException("Invalid research modifier count.");
            ResearchParameterModifier[] modifiers = new ResearchParameterModifier[modifierCount];
            for (int i = 0; i < modifierCount; i++)
            {
                string target = reader.ReadString(); uint targetId = reader.ReadUInt32();
                modifiers[i] = new ResearchParameterModifier(target, (ResearchModifierOperation)reader.ReadByte(), reader.ReadInt32());
                if (modifiers[i].TargetId.Value != targetId) throw new InvalidDataException("Stable research modifier ID mismatch.");
            }
            definitions[definitionIndex] = new ResearchDefinition(key, faction, categories, sourceBuilding, ore, energy, crystals, ticks, groups, unlocks, modifiers,
                reader.ReadString(), reader.ReadString(), reader.ReadString());
            if (definitions[definitionIndex].Id.Value != id || definitions[definitionIndex].SourceBuildingType.Value != sourceBuildingId)
                throw new InvalidDataException("Stable research or source-building ID mismatch.");
        }
        return definitions;
    }

    private static void WriteTransformation(BinaryWriter writer, TransformationDefinition definition)
    {
        writer.Write(definition.StableKey); writer.Write(definition.Id.Value); writer.Write(definition.EntityStableKey); writer.Write(definition.EntityType.Value);
        WriteTransformationMode(writer, definition.ModeA); WriteTransformationMode(writer, definition.ModeB);
        writer.Write(definition.AToBDurationTicks); writer.Write(definition.BToADurationTicks); writer.Write(definition.CancellationThresholdBasisPoints);
        writer.Write(definition.RollbackTicks); writer.Write(definition.ReversalLockTicks); writer.Write(definition.MoveDuringTransition);
        writer.Write(definition.AttackDuringTransition); writer.Write((byte)definition.TransitionTargetLayers);
    }

    private static void WriteTransformationMode(BinaryWriter writer, TransformationModeDefinition mode)
    {
        writer.Write(mode.StateKey); writer.Write(mode.StateId.Value); writer.Write(mode.DisplayName); writer.Write(mode.MovementProfileKey);
        writer.Write((byte)mode.Footprint); writer.Write(mode.VisionRadius); writer.Write(mode.ViewProfileKey);
        writer.Write((byte)mode.Combat.TargetClass); writer.Write((byte)mode.Combat.TargetLayer); writer.Write((ushort)mode.Combat.TargetFlags);
        writer.Write(mode.Combat.MaximumHitPoints); writer.Write(mode.Combat.ArmorRating); writer.Write((byte)mode.Combat.PriorityProfile);
        writer.Write((byte)mode.Combat.LegalTargetLayers); writer.Write((byte)mode.Combat.LegalTargetClasses); writer.Write(mode.Combat.AcquisitionRadius.Raw);
        writer.Write(mode.Combat.WeaponProfile.Value);
    }

    private static TransformationDefinition[] ReadTransformations(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > 1024) throw new InvalidDataException("Invalid transformation definition count.");
        TransformationDefinition[] definitions = new TransformationDefinition[count];
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString(); uint id = reader.ReadUInt32(); string entityKey = reader.ReadString(); uint entityId = reader.ReadUInt32();
            TransformationModeDefinition modeA = ReadTransformationMode(reader), modeB = ReadTransformationMode(reader);
            definitions[i] = new TransformationDefinition(key, entityKey, modeA, modeB, reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
                reader.ReadBoolean(), reader.ReadBoolean(), (TargetLayerMask)reader.ReadByte());
            if (definitions[i].Id.Value != id || definitions[i].EntityType.Value != entityId) throw new InvalidDataException("Stable transformation ID mismatch.");
        }
        return definitions;
    }

    private static TransformationModeDefinition ReadTransformationMode(BinaryReader reader)
    {
        string stateKey = reader.ReadString(); uint stateId = reader.ReadUInt32(); string display = reader.ReadString(); string movement = reader.ReadString();
        FootprintClass footprint = (FootprintClass)reader.ReadByte(); byte vision = reader.ReadByte(); string view = reader.ReadString();
        PrototypeCombatProfile combat = new((CombatTargetClass)reader.ReadByte(), (CombatTargetLayer)reader.ReadByte(), (CombatTargetFlags)reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadByte(), (TargetPriorityProfile)reader.ReadByte(), (TargetLayerMask)reader.ReadByte(), (TargetClassMask)reader.ReadByte(),
            Fix32.FromRaw(reader.ReadInt32()), new ContentId(reader.ReadUInt32()));
        TransformationModeDefinition mode = new(stateKey, display, movement, footprint, vision, view, combat);
        if (mode.StateId.Value != stateId) throw new InvalidDataException("Stable transformation state ID mismatch.");
        return mode;
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

    private static byte LegacyWorksiteServiceRadius(string stableKey) => stableKey switch
    {
        "building.rock_raiders.hq" => 18,
        "building.rock_raiders.vehicle_service_bay" => 12,
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

    private static WeaponDefinition[] ReadWeapons(BinaryReader reader, bool includeProjectileSpeed, bool includeContactRules)
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
            bool requiresLineOfSight = reader.ReadBoolean();
            ushort facingDegrees = includeContactRules ? checked((ushort)((reader.ReadUInt16() * 360L + 32_767) / 65_536)) : LegacyFacingToleranceDegrees(key, delivery);
            ushort movingBasisPoints = includeContactRules ? reader.ReadUInt16() : delivery == WeaponDeliveryKind.Contact ? (ushort)3_500 : (ushort)10_000;
            weapons[i] = new WeaponDefinition(key, layers, classes, priority, damage, damageType, cooldown, range, minimumRange, delivery, projectileSpeed, requiresLineOfSight, facingDegrees, movingBasisPoints);
            if (weapons[i].Id.Value != id) throw new InvalidDataException("Stable weapon ID mismatch.");
        }
        return weapons;
    }

    private static ushort LegacyFacingToleranceDegrees(string stableKey, WeaponDeliveryKind delivery)
        => delivery != WeaponDeliveryKind.Contact ? (ushort)180 : stableKey == "weapon.rr.chrome_crusher.chrome_drill" ? (ushort)30 : (ushort)45;

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
        new WeaponDefinition("weapon.rr.chrome_crusher.chrome_drill", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Siege, 55, DamageType.Siege, 32, Fix32.FromRatio(21,20), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 30, 3_500),
        new WeaponDefinition("weapon.rr.crew.portable_mining_tool", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Support, 6, DamageType.General, 24, Fix32.FromRatio(4,5), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 45, 3_500),
        new WeaponDefinition("weapon.rr.hover_scout.survey_pulse", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.Scout, 6, DamageType.General, 30, Fix32.FromInt(3), Fix32.Zero, WeaponDeliveryKind.Projectile, Fix32.FromInt(12), true),
        new WeaponDefinition("weapon.rr.loader_dozer.scoop_ram", TargetLayerMask.Ground, TargetClassMask.All, TargetPriorityProfile.AntiLight, 18, DamageType.General, 27, Fix32.FromRatio(9,10), Fix32.Zero, WeaponDeliveryKind.Contact, Fix32.Zero, true, 45, 3_500)
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
