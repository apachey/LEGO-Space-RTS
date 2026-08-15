using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M3BrownoutTests
{
    private static readonly ContentId ProcessingPlant = StableId.FromKey("building.rock_raiders.ore_processing_plant");
    private static readonly ContentId PowerStation = StableId.FromKey("building.rock_raiders.power_station");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");
    private static readonly ContentId HoverScout = StableId.FromKey("unit.rock_raiders.hover_scout");

    [Test]
    public void BrownoutUsesFunctionalClassThenPriorityThenEntityIdWithoutPartialPower()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out EntityId root), Is.True);
        EntityId first = AddBuilding(world, ProcessingPlant, root, 20);
        EntityId second = AddBuilding(world, ProcessingPlant, root, 30);
        EntityId third = AddBuilding(world, ProcessingPlant, root, 40);
        EntityId bay = AddBuilding(world, ServiceBay, root, 50);
        EnergyDomainSystem.Recalculate(world, root);
        world.Entities.EnergyDomain.Get(root).Reserve = Fix32.Zero;
        BrownoutSystem.Recalculate(world, root);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.PowerState.Get(first).IsPowered, Is.True);
            Assert.That(world.Entities.PowerState.Get(second).IsPowered, Is.True);
            Assert.That(world.Entities.PowerState.Get(third).IsPowered, Is.False);
            Assert.That(world.Entities.PowerState.Get(bay).IsPowered, Is.False, "Production class follows resource processing.");
            Assert.That(world.Entities.EnergyDomain.Get(root).PoweredDemandPerSecond, Is.EqualTo(2));
        });

        Assert.That(BrownoutSystem.TrySetPriority(world, 0, new[] { third }, EnergyPriority.High), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.PowerState.Get(third).IsPowered, Is.True);
            Assert.That(world.Entities.PowerState.Get(first).IsPowered, Is.True);
            Assert.That(world.Entities.PowerState.Get(second).IsPowered, Is.False);
        });
    }

    [Test]
    public void ProductionPausesWithProgressRetainedAndResumesAfterRecovery()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out EntityId root), Is.True);
        EntityId bay = AddBuilding(world, ServiceBay, root, 30);
        world.Entities.Production.Set(bay, new Production());
        AddBuilding(world, ProcessingPlant, root, 40);
        AddBuilding(world, ProcessingPlant, root, 50);
        EnergyDomainSystem.Recalculate(world, root);
        Assert.That(ProductionSystem.TryQueue(world, 0, bay, HoverScout), Is.True);
        ushort before = world.Entities.Production.Get(bay).Get(0).RemainingTicks;
        world.Entities.EnergyDomain.Get(root).Reserve = Fix32.Zero;
        BrownoutSystem.Recalculate(world, root);

        new SimulationRunner(world).StepTicks(10);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.PowerState.Get(bay).IsPowered, Is.False);
            Assert.That(world.Entities.Production.Get(bay).Get(0).RemainingTicks, Is.EqualTo(before));
        });

        AddBuilding(world, PowerStation, root, 60);
        EnergyDomainSystem.Recalculate(world, root);
        new SimulationRunner(world).StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.EnergyDomain.Get(root).IsBrownout, Is.False);
            Assert.That(world.Entities.PowerState.Get(bay).IsPowered, Is.True);
            Assert.That(world.Entities.Production.Get(bay).Get(0).RemainingTicks, Is.EqualTo(before - 1));
        });
    }

    [Test]
    public void BrownoutRevisionChangesOncePerPowerSetTransitionNotEveryTick()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out EntityId root), Is.True);
        AddBuilding(world, ProcessingPlant, root, 20); AddBuilding(world, ProcessingPlant, root, 30); AddBuilding(world, ProcessingPlant, root, 40);
        EnergyDomainSystem.Recalculate(world, root);
        uint before = world.Entities.EnergyDomain.Get(root).BrownoutRevision;
        EnergyDomainSystem.DebugDrainPlayerDomains(world, 0);
        uint entered = world.Entities.EnergyDomain.Get(root).BrownoutRevision;
        new SimulationRunner(world).StepTicks(40);
        Assert.Multiple(() =>
        {
            Assert.That(entered, Is.EqualTo(before + 1));
            Assert.That(world.Entities.EnergyDomain.Get(root).BrownoutRevision, Is.EqualTo(entered));
            Assert.That(world.Entities.EnergyDomain.Get(root).LastBrownoutEvent, Is.EqualTo(BrownoutEventKind.Entered));
        });
    }

    [Test]
    public void UnusedGenerationRechargesReserveAndRecoversTheDomain()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out EntityId root), Is.True);
        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        domain.Reserve = Fix32.Zero; domain.GenerationPerSecond = 2; domain.ContinuousDemandPerSecond = 3;
        domain.PoweredDemandPerSecond = 0; domain.IsBrownout = true;

        new EnergyDomainSystem().Step(world);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.EnergyDomain.Get(root).Reserve, Is.GreaterThan(Fix32.Zero));
            Assert.That(world.Entities.EnergyDomain.Get(root).IsBrownout, Is.False);
            Assert.That(world.Entities.EnergyDomain.Get(root).LastBrownoutEvent, Is.EqualTo(BrownoutEventKind.Recovered));
        });
    }

    [Test]
    public void PriorityCommandAndSnapshotPreserveAuthoritativeBrownoutState()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out EntityId root), Is.True);
        EntityId first = AddBuilding(world, ProcessingPlant, root, 20);
        AddBuilding(world, ProcessingPlant, root, 30);
        EntityId third = AddBuilding(world, ProcessingPlant, root, 40);
        EnergyDomainSystem.Recalculate(world, root);
        EnergyDomainSystem.DebugDrainPlayerDomains(world, 0);
        world.Commands.Enqueue(new CommandEnvelope(world.Tick.Next(), 0, 1, SimCommandType.SetEnergyPriority, new[] { third }, FixVec2.Zero, energyPriority: EnergyPriority.High));
        new SimulationRunner(world).StepOneTick();
        Assert.That(world.Entities.PowerState.Get(third).Priority, Is.EqualTo(EnergyPriority.High));
        Assert.That(world.Entities.PowerState.Get(first).IsPowered, Is.True);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
            Assert.That(restored.Entities.PowerState.Get(third).Priority, Is.EqualTo(EnergyPriority.High));
            Assert.That(restored.Entities.EnergyDomain.Get(root).IsBrownout, Is.True);
        });
    }

    private static EntityId AddBuilding(SimulationWorld world, ContentId type, EntityId root, short anchorX)
    {
        BuildingDefinition definition = world.Content.Buildings.Single(x => x.Id == type);
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(id, new SimTransform { Position = FixVec2.FromInts(anchorX, 30), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, AnchorX = anchorX, AnchorY = 30, FootprintWidth = definition.FootprintWidth, FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed });
        world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = root });
        world.Entities.WorksiteMember.Set(id, new WorksiteMember { ComponentRoot = root });
        return id;
    }
}
}
