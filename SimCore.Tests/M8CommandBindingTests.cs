using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M8CommandBindingTests
{
    [Test]
    public void ApprovedProductionExitsAndSettlementReserveAreAuthoredExactly()
    {
        PrototypeContentCatalog catalog = PrototypeContentFactory.CreateM2Catalog();
        (string Key, byte Width, byte Depth, FootprintClass Footprint)[] exits =
        {
            ("building.rock_raiders.hq",2,2,FootprintClass.Tiny),("building.rock_raiders.vehicle_service_bay",3,3,FootprintClass.Medium),
            ("building.rock_raiders.engineering_workshop",5,5,FootprintClass.Huge),("building.ast.mb01_eagle_command_base",3,3,FootprintClass.Small),
            ("building.ast.field_systems_garage",4,4,FootprintClass.Large),("building.ast.mission_vehicle_bay",5,5,FootprintClass.Huge),
            ("building.ast.flight_operations_pad",5,5,FootprintClass.Huge),("building.ali.etx_command_core",2,2,FootprintClass.Tiny),
            ("building.ali.etx_fabricator",3,3,FootprintClass.Small),("building.ali.reconfiguration_dock",5,5,FootprintClass.Huge),
            ("building.mar.aero_tube_hangar",3,3,FootprintClass.Small),("building.mar.settlement_station",2,2,FootprintClass.Tiny),
            ("building.mar.mechanical_workshop",5,5,FootprintClass.Huge)
        };
        foreach (var expected in exits)
        {
            BuildingDefinition actual = catalog.Buildings.Single(x => x.StableKey == expected.Key);
            Assert.That((actual.ProductionExitWidth, actual.ProductionExitDepth, actual.ProductionExitFootprint), Is.EqualTo((expected.Width, expected.Depth, expected.Footprint)), expected.Key);
        }
        BuildingDefinition settlement = catalog.Buildings.Single(x => x.StableKey == "building.mar.settlement_station");
        Assert.That((settlement.EnergyReserveCapacity, settlement.EnergyGenerationPerSecond), Is.EqualTo(((ushort)150, (ushort)1)));
    }

    [Test]
    public void ApprovedCancellationArithmeticUsesTwentyPercentAndCrystalMidpoint()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CancellationAccounting.Refund(101, 0, 100), Is.EqualTo(101));
            Assert.That(CancellationAccounting.Consumed(101, 1, 100), Is.EqualTo(21));
            Assert.That(CancellationAccounting.Consumed(101, 50, 100), Is.EqualTo(61));
            Assert.That(CancellationAccounting.Refund(101, 50, 100), Is.EqualTo(70));
            Assert.That(CancellationAccounting.CrystalsCommitted(49, 100), Is.False);
            Assert.That(CancellationAccounting.CrystalsCommitted(50, 100), Is.True);
        });
    }

    [Test]
    public void WaitingProductionCancellationRefundsFullyAndActiveItemUsesProgressiveLoss()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(0);
        EntityId hq = world.Entities.Alive.First(id => world.Entities.Production.Has(id));
        ContentId crew = StableId.FromKey("unit.rock_raiders.crew");
        int before = world.GetProcessedResourceTotal(0, ResourceType.Ore);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, crew), Is.True);
        Assert.That(ProductionSystem.TryQueue(world, 0, hq, crew), Is.True);
        Assert.That(ProductionSystem.TryCancel(world, 0, hq, 1), Is.True);
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(before - 50));
        ref Production production = ref world.Entities.Production.Get(hq);
        ProductionQueueItem active = production.Get(0); active.RemainingTicks = 160; production.Set(0, active);
        Assert.That(ProductionSystem.TryCancel(world, 0, hq, 0), Is.True);
        Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(before - 15));
    }

    [Test]
    public void CrystalBuildingReservesAndCommitsCrystalAtMidpoint()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId worker = ScenarioFactory.OwnedIds(world, 0).Single(id => world.Entities.Worker.Has(id));
        EntityId oreBank = world.Entities.Alive.First(id => world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == ResourceType.Ore);
        world.Entities.ResourceBank.Get(oreBank).ProcessedAmount = 500;
        EntityId crystalBank = world.Entities.Create();
        world.Entities.Ownership.Set(crystalBank, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(crystalBank, world.Entities.Transform.Get(oreBank));
        world.Entities.ResourceBank.Set(crystalBank, new ResourceBank { Type = ResourceType.Crystal, ProcessedAmount = 2 });
        world.Entities.WorksiteMember.Set(crystalBank, world.Entities.WorksiteMember.Get(oreBank));
        ContentId laboratory = StableId.FromKey("building.mar.routing_laboratory");
        AddBuilding(world, 0, "building.mar.mechanical_workshop", 100, 30);
        AddBuilding(world, 0, "building.mar.pressure_generator", 115, 30);
        AddBuilding(world, 0, "building.mar.excavation_plant", 125, 30);

        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, laboratory, 30, 90, 0, out EntityId waiting, out PlacementFailure failure), Is.True, failure.ToString());
        Assert.That(world.Entities.ResourceBank.Get(crystalBank).ProcessedAmount, Is.EqualTo(1));
        Assert.That(ConstructionPlacement.TryCancel(world, 0, waiting), Is.True);
        Assert.That(world.Entities.ResourceBank.Get(crystalBank).ProcessedAmount, Is.EqualTo(2));

        Assert.That(ConstructionPlacement.TryPlace(world, 0, new[] { worker }, laboratory, 30, 90, 0, out EntityId active, out failure), Is.True, failure.ToString());
        ref ConstructionSite site = ref world.Entities.ConstructionSite.Get(active);
        site.ProgressTicks = checked((ushort)(site.RequiredTicks / 2));
        Assert.That(ConstructionPlacement.TryCancel(world, 0, active), Is.True);
        Assert.That(world.Entities.ResourceBank.Get(crystalBank).ProcessedAmount, Is.EqualTo(1));
    }

    [Test]
    public void ExcavationDurationsAreExplicitPerMachineAndDrillCraftIsFastest()
    {
        ContentId drill = StableId.FromKey("unit.rock_raiders.drill_craft"), crusher = StableId.FromKey("unit.rock_raiders.chrome_crusher"), grinder = StableId.FromKey("unit.rock_raiders.granite_grinder");
        Assert.Multiple(() =>
        {
            Assert.That(ExcavationSystem.TryDuration(drill, false, out ushort ds) && ds == 400); Assert.That(ExcavationSystem.TryDuration(drill, true, out ushort dr) && dr == 800);
            Assert.That(ExcavationSystem.TryDuration(crusher, false, out ushort cs) && cs == 500); Assert.That(ExcavationSystem.TryDuration(crusher, true, out ushort cr) && cr == 900);
            Assert.That(ExcavationSystem.TryDuration(grinder, false, out ushort gs) && gs == 600); Assert.That(ExcavationSystem.TryDuration(grinder, true, out ushort gr) && gr == 1100);
            Assert.That(ds, Is.LessThan(cs)); Assert.That(ds, Is.LessThan(gs)); Assert.That(dr, Is.LessThan(cr)); Assert.That(dr, Is.LessThan(gr));
            Assert.That((ExcavationSystem.StandardEnergy, ExcavationSystem.ReinforcedEnergy), Is.EqualTo(((ushort)25, (ushort)50)));
        });
    }

    [Test]
    public void AstronautAreasOverlapIntoOneDomainWithoutForwardServiceCoupling()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        EntityId command = AddBuilding(world, 0, "building.ast.mb01_eagle_command_base", 10, 20);
        EntityId solar = AddBuilding(world, 0, "building.ast.solar_energy_array", 60, 20);
        FactionEnergyDomainSystem.Rebuild(world);
        world.Entities.EnergyDomain.Get(command).Reserve = Fix32.FromInt(70);
        world.Entities.EnergyDomain.Get(solar).Reserve = Fix32.FromInt(80);
        world.Entities.Transform.Get(solar).Position = FixVec2.FromInts(40, 20);
        EntityId service = AddBuilding(world, 0, "building.ast.service_refit_hub", 70, 20);
        FactionEnergyDomainSystem.Rebuild(world);
        Assert.That(world.Entities.EnergyDomainMember.Get(command).DomainRoot, Is.EqualTo(command));
        Assert.That(world.Entities.EnergyDomainMember.Get(service).DomainRoot, Is.EqualTo(command));
        Assert.That(world.Entities.EnergyDomainMember.Get(solar).DomainRoot, Is.EqualTo(command));
        Assert.That(world.Entities.EnergyDomain.Get(command).Reserve, Is.EqualTo(Fix32.FromInt(150)));
        Assert.That(world.Entities.EnergyDomain.Has(solar), Is.False);
        Assert.That(world.Entities.ForwardServiceMember.Has(command), Is.False);
    }

    [Test]
    public void AlienNearestCoreTieAndAbsoluteResonanceOwnershipAreDeterministic()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        EntityId first = AddBuilding(world, 0, "building.ali.etx_command_core", 20, 20);
        EntityId second = AddBuilding(world, 0, "building.ali.etx_command_core", 40, 20);
        EntityId fabricator = AddBuilding(world, 0, "building.ali.etx_fabricator", 30, 20);
        EntityId resonanceA = AddBuilding(world, 0, "building.ali.resonance_core", 29, 20);
        EntityId resonanceB = AddBuilding(world, 0, "building.ali.resonance_core", 31, 20);
        FactionEnergyDomainSystem.Rebuild(world);
        Assert.That(world.Entities.EnergyDomainMember.Get(fabricator).DomainRoot, Is.EqualTo(first));
        Assert.That(world.Entities.ResonanceCore.Get(resonanceA).CommandCore, Is.EqualTo(first));
        Assert.That(world.Entities.ResonanceCore.Get(resonanceB).CommandCore, Is.EqualTo(second));
    }

    [Test]
    public void DefenseNodeReconfigurationPausesForExactlyEightyQuietTicks()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        EntityId node = world.Entities.Create(), enemy = world.Entities.Create();
        world.Entities.Ownership.Set(node, new Ownership { PlayerSlot = 0 }); world.Entities.Ownership.Set(enemy, new Ownership { PlayerSlot = 1 });
        world.Entities.Targetable.Set(node, new Targetable { Class = CombatTargetClass.Structure, Layer = CombatTargetLayer.Ground, Flags = CombatTargetFlags.DefensiveStructure });
        world.Entities.Health.Set(node, new Health { Maximum = Fix32.FromInt(100), Current = Fix32.FromInt(100) });
        DefenseNodeSystem.Register(world, node, DefenseNodeMode.GroundPulse);
        Assert.That(DefenseNodeSystem.TryStartReconfiguration(world, 0, node, DefenseNodeMode.AirLance), Is.True);
        new DefenseNodeSystem().Step(world); DefenseNodeSystem.TryGetState(world, node, out DefenseNodeState afterOne); ushort progressed = afterOne.RemainingTicks;
        DamageSystem.Resolve(world, new DamageRequest(enemy, node, StableId.FromKey("weapon.test"), 1, DamageType.General));
        SimulationRunner runner = new(world); runner.StepTicks(79);
        DefenseNodeSystem.TryGetState(world, node, out DefenseNodeState paused); Assert.That(paused.RemainingTicks, Is.EqualTo(progressed));
        runner.StepOneTick();
        DefenseNodeSystem.TryGetState(world, node, out DefenseNodeState resumed); Assert.That(resumed.RemainingTicks, Is.EqualTo(progressed - 1));
    }

    private static EntityId AddBuilding(SimulationWorld world, byte player, string key, int x, int y)
    {
        ContentId type = StableId.FromKey(key); BuildingDefinition definition = world.Content.Buildings.Single(b => b.Id == type);
        EntityId id = world.Entities.Create(); world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = FixVec2.FromInts(x, y) });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, AnchorX = (short)x, AnchorY = (short)y, FootprintWidth = definition.FootprintWidth, FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed });
        return id;
    }
}
}
