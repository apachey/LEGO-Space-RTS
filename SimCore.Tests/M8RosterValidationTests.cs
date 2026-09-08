using LegoSpaceRTS.SimCore;
using NUnit.Framework;
using System.IO;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M8RosterValidationTests
{
    [Test]
    public void CanonicalCatalogAndMapCloseEveryRosterReference()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();

        Assert.DoesNotThrow(() => CanonicalRosterValidator.Validate(catalog, DevMapFactory.CreateDefinition()));
        Assert.Multiple(() =>
        {
            Assert.That(CanonicalRosterReferences.TubeEligibleUnitKeys, Is.EqualTo(new[]
            {
                "unit.martians.worker_robot", "unit.martians.double_hover", "unit.martians.jet_scooter"
            }));
            Assert.That(CanonicalRosterReferences.TubeEligibleUnitKeys.All(key =>
                catalog.TryGetEntity(key, out PrototypeEntityDefinition entity) &&
                TubeTransferSystem.IsEligibleContentType(entity.Id)), Is.True);
            Assert.That(TubeTransferSystem.IsEligibleContentType(
                StableId.FromKey(CanonicalRosterReferences.MartianExcavationSearcher)), Is.False);
        });
    }

    [Test]
    public void BrokenDuplicateIdFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        PrototypeContentCatalog broken = CopyCatalog(source, entities: source.Entities.Append(source.Entities[0]).ToArray());

        AssertDiagnostic(() => CanonicalRosterValidator.Validate(broken, DevMapFactory.CreateDefinition()), "T074_DUPLICATE_ID");
    }

    [Test]
    public void BrokenMissingVisualProfileFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        PrototypeEntityDefinition original = source.Entities.Single(entity => entity.StableKey == CanonicalRosterReferences.MartianWorkerRobot);
        PrototypeEntityDefinition brokenEntity = CopyEntity(original, viewProfile: string.Empty);
        PrototypeContentCatalog broken = CopyCatalog(source,
            entities: source.Entities.Select(entity => entity.Id == original.Id ? brokenEntity : entity).ToArray());

        AssertDiagnostic(() => CanonicalRosterValidator.Validate(broken, DevMapFactory.CreateDefinition()), "T074_MISSING_VISUAL_PROFILE");
    }

    [Test]
    public void BrokenInvalidWeaponFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        PrototypeEntityDefinition original = source.Entities.Single(entity => entity.StableKey == "unit.rock_raiders.crew");
        PrototypeCombatProfile combat = original.Combat;
        PrototypeCombatProfile brokenCombat = new(combat.TargetClass, combat.TargetLayer, combat.TargetFlags,
            combat.MaximumHitPoints, combat.ArmorRating, combat.PriorityProfile, combat.LegalTargetLayers,
            combat.LegalTargetClasses, combat.AcquisitionRadius, StableId.FromKey("weapon.missing.fixture"));
        PrototypeEntityDefinition brokenEntity = CopyEntity(original, combat: brokenCombat);

        AssertDiagnostic(() => CopyCatalog(source,
            entities: source.Entities.Select(entity => entity.Id == original.Id ? brokenEntity : entity).ToArray()),
            "unknown weapon profile");
    }

    [Test]
    public void BrokenResearchCycleFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        ResearchDefinition first = source.Research.Single(research => research.StableKey == "research.rr.cutter_package");
        ResearchDefinition second = source.Research.Single(research => research.StableKey == "research.rr.service_gantries");
        ResearchDefinition firstBroken = CopyResearch(first, ResearchRequirement(second.StableKey));
        ResearchDefinition secondBroken = CopyResearch(second, ResearchRequirement(first.StableKey));

        AssertDiagnostic(() => CopyCatalog(source, research: source.Research.Select(research =>
            research.Id == first.Id ? firstBroken : research.Id == second.Id ? secondBroken : research).ToArray()), "cycle");
    }

    [Test]
    public void BrokenMissingProductionSourceFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        UnitProductionDefinition original = source.Production.Single(production => production.UnitStableKey == CanonicalRosterReferences.MartianWorkerRobot);
        UnitProductionDefinition brokenProduction = new(original.UnitStableKey, new[] { "building.missing.fixture" },
            original.OreCost, original.EnergyCost, original.CrystalCost, original.OperationsCapacity,
            original.BuildTicks, original.PrerequisiteGroups);

        AssertDiagnostic(() => CopyCatalog(source, production: source.Production.Select(production =>
            production.UnitType == original.UnitType ? brokenProduction : production).ToArray()), "references missing producer");
    }

    [Test]
    public void BrokenIllegalTubeUnitFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        string[] brokenEligibility = CanonicalRosterReferences.TubeEligibleUnitKeys
            .Append(CanonicalRosterReferences.MartianExcavationSearcher).ToArray();

        AssertDiagnostic(() => CanonicalRosterValidator.Validate(source, DevMapFactory.CreateDefinition(), brokenEligibility),
            "T074_ILLEGAL_TUBE_UNIT");
    }

    [Test]
    public void BrokenMapStartFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        MapDefinition map = DevMapFactory.CreateDefinition();
        MapDefinition broken = new(map.Grid,
            new[] { new MapStart(0, FixVec2.FromInts(-1, 10)), map.Starts[1] }, map.InitialEntities,
            map.VisionTestGeometry, map.InitialResourceNodes, map.InitialResourceReceivers);

        AssertDiagnostic(() => CanonicalRosterValidator.Validate(source, broken), "T074_BAD_MAP_START");
    }

    [Test]
    public void BrokenImpossibleBuildingExitFixtureFailsWithSpecificDiagnostic()
    {
        PrototypeContentCatalog source = PrototypeContentFactory.CreateM2Catalog();
        BuildingDefinition original = source.Buildings.Single(building => building.StableKey == "building.ali.reconfiguration_dock");
        BuildingDefinition brokenBuilding = CopyBuilding(original, 1, 1, FootprintClass.Tiny);
        PrototypeContentCatalog broken = CopyCatalog(source, buildings: source.Buildings.Select(building =>
            building.Id == original.Id ? brokenBuilding : building).ToArray());

        AssertDiagnostic(() => CanonicalRosterValidator.Validate(broken, DevMapFactory.CreateDefinition()),
            "T074_IMPOSSIBLE_BUILDING_EXIT");
    }

    private static PrototypeContentCatalog CopyCatalog(PrototypeContentCatalog source,
        PrototypeEntityDefinition[]? entities = null, BuildingDefinition[]? buildings = null,
        UnitProductionDefinition[]? production = null, ResearchDefinition[]? research = null)
        => new(source.MovementProfiles, entities ?? source.Entities, source.ResourceNodes, buildings ?? source.Buildings,
            production ?? source.Production, source.Weapons, source.Transformations, research ?? source.Research, source.Commands);

    private static PrototypeEntityDefinition CopyEntity(PrototypeEntityDefinition source, string? viewProfile = null,
        PrototypeCombatProfile? combat = null)
        => new(source.StableKey, source.FactionKey, source.SourceClassification, source.MovementProfileKey,
            source.Footprint, source.SelectableKind, source.VisionRadius, viewProfile ?? source.ViewProfileKey,
            source.OreTicksPerUnit, source.OreCarryCapacity, source.OperationsCapacity, combat ?? source.Combat);

    private static ResearchDefinition CopyResearch(ResearchDefinition source, ResearchPrerequisiteGroup[] prerequisites)
        => new(source.StableKey, source.FactionKey, source.Categories, source.SourceBuildingStableKey,
            source.OreCost, source.EnergyCost, source.CrystalCost, source.ResearchTicks, prerequisites,
            source.UnlockTags, source.ParameterModifiers, source.MutuallyExclusiveGroupKey,
            source.PresentationProfileKey, source.DisplayNameLocKey);

    private static ResearchPrerequisiteGroup[] ResearchRequirement(string stableKey)
        => new[] { new ResearchPrerequisiteGroup(new[]
        {
            new ResearchPrerequisiteDefinition(ResearchPrerequisiteKind.Research, stableKey)
        }) };

    private static BuildingDefinition CopyBuilding(BuildingDefinition source, byte exitWidth, byte exitDepth,
        FootprintClass exitFootprint)
        => new(source.StableKey, source.FootprintWidth, source.FootprintHeight, source.FootprintMask, source.Rotatable,
            source.OreCost, source.EnergyCost, source.BuildTicks, exitWidth, exitDepth, exitFootprint,
            source.OperationsCapacityProvided, source.EnergyGenerationPerSecond, source.EnergyReserveCapacity,
            source.ContinuousEnergyDemandPerSecond, source.EnergyFunctionalClass, source.WorksiteServiceRadius,
            source.CrystalCost, source.FootprintMaskHigh, source.PrerequisiteGroups);

    private static void AssertDiagnostic(TestDelegate action, string diagnostic)
    {
        Exception? error = Assert.Catch(action);
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain(diagnostic));
    }
}
}
