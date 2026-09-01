using System;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5MissionRefitTests
{
    private static readonly ContentId HubType = StableId.FromKey("building.ast.service_refit_hub");
    private static readonly ContentId T3Type = StableId.FromKey("unit.ast.t3_trike");

    [Test]
    public void FirstSurveyInstallCommitsCanonicalCostAndCompletesOnSameEntity()
    {
        Fixture fixture = CreateFixture();
        EntityId original = fixture.Trike;
        int oreBefore = fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount;
        fixture.World.Commands.Enqueue(RefitCommand(1, original, MissionConfiguration.T3Survey));

        fixture.Runner.StepOneTick();
        MissionRefitJob job = fixture.World.Entities.MissionRefitJob.Get(original);
        Assert.Multiple(() =>
        {
            Assert.That(job.TotalTicks, Is.EqualTo(360));
            Assert.That(job.RemainingTicks, Is.EqualTo(359));
            Assert.That(job.CommittedOre, Is.EqualTo(25));
            Assert.That(job.CommittedEnergy, Is.EqualTo(10));
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(oreBefore - 25));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).Reserve, Is.EqualTo(Fix32.FromInt(90)));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).GenerationPerSecond, Is.EqualTo(2));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(2));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Ore).Total, Is.EqualTo(fixture.OreConservationBefore));
        });

        fixture.Runner.StepTicks(359);
        MissionRefitState state = fixture.World.Entities.MissionRefitState.Get(original);
        Assert.Multiple(() =>
        {
            Assert.That(fixture.World.Entities.Exists(original), Is.True);
            Assert.That(fixture.World.Entities.MissionRefitJob.Has(original), Is.False);
            Assert.That(state.CurrentConfiguration, Is.EqualTo(MissionConfiguration.T3Survey));
            Assert.That(state.OwnedConfigurationMask, Is.EqualTo(3));
            Assert.That(state.ConfigurationLockTicks, Is.EqualTo(400));
            Assert.That(fixture.World.Entities.Movement.Get(original).MaxSpeed, Is.EqualTo(Fix32.FromRatio(185, 100)));
            Assert.That(fixture.World.Entities.Vision.Get(original).RadiusBuildCells, Is.EqualTo(13));
        });
    }

    [Test]
    public void OwnedModuleSwapUsesReducedCostTimeAndConfigurationLock()
    {
        Fixture fixture = CreateFixture();
        ref MissionRefitState state = ref fixture.World.Entities.MissionRefitState.Get(fixture.Trike);
        state.CurrentConfiguration = MissionConfiguration.T3Survey;
        state.OwnedConfigurationMask = 3;
        state.ConfigurationLockTicks = 1;

        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Escort), Is.False);
        fixture.Runner.StepOneTick();
        Assert.That(state.ConfigurationLockTicks, Is.Zero);
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Escort), Is.True);
        MissionRefitJob job = fixture.World.Entities.MissionRefitJob.Get(fixture.Trike);
        Assert.Multiple(() =>
        {
            Assert.That(job.TotalTicks, Is.EqualTo(200));
            Assert.That(job.CommittedOre, Is.EqualTo(8));
            Assert.That(job.CommittedEnergy, Is.EqualTo(5));
        });
        fixture.Runner.StepTicks(200);
        Assert.That(fixture.World.Entities.MissionRefitState.Get(fixture.Trike).CurrentConfiguration, Is.EqualTo(MissionConfiguration.T3Escort));
        Assert.That(fixture.World.Entities.Movement.Get(fixture.Trike).MaxSpeed, Is.EqualTo(Fix32.FromRatio(170, 100)));
        Assert.That(fixture.World.Entities.Vision.Get(fixture.Trike).RadiusBuildCells, Is.EqualTo(9));
    }

    [Test]
    public void ValidationRejectsMissingUnlockServiceFundsAndIllegalTargets()
    {
        Fixture fixture = CreateFixture();
        ref MissionRefitState state = ref fixture.World.Entities.MissionRefitState.Get(fixture.Trike);
        state.SurveyUnlocked = false;
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Survey), Is.False);
        state.SurveyUnlocked = true;
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 1, fixture.Trike, MissionConfiguration.T3Survey), Is.False);
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Escort), Is.False);

        fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount = 24;
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Survey), Is.False);
        fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount = 200;
        fixture.World.Entities.Transform.Get(fixture.Trike).Position = FixVec2.FromInts(80, 80);
        new ForwardServiceSystem().Step(fixture.World);
        Assert.That(MissionRefitSystem.TryStartT3Refit(fixture.World, 0, fixture.Trike, MissionConfiguration.T3Survey), Is.False);
    }

    [Test]
    public void ActiveRefitBlocksMovementCommandsAndPreservesSnapshotContinuation()
    {
        Fixture fixture = CreateFixture();
        fixture.World.Commands.Enqueue(RefitCommand(1, fixture.Trike, MissionConfiguration.T3Survey));
        fixture.World.Commands.Enqueue(new CommandEnvelope(new SimTick(2), 0, 2, SimCommandType.Move, new[] { fixture.Trike }, FixVec2.FromInts(100, 100)));
        fixture.Runner.StepTicks(80);
        Assert.That(fixture.World.Entities.Navigation.Get(fixture.Trike).HasTarget, Is.False);
        ulong before = StateHasher.Hash(fixture.World);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(fixture.World));
        Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(before));
        SimulationRunner restored = new(restoredWorld);
        fixture.Runner.StepTicks(280);
        restored.StepTicks(280);
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(fixture.World)));
            Assert.That(restoredWorld.Entities.MissionRefitState.Get(fixture.Trike).CurrentConfiguration, Is.EqualTo(MissionConfiguration.T3Survey));
            Assert.That(restoredWorld.Entities.Exists(fixture.Trike), Is.True);
        });
    }

    private static CommandEnvelope RefitCommand(int tick, EntityId trike, MissionConfiguration target)
        => new(new SimTick(tick), 0, 1, SimCommandType.MissionRefit, Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: trike, missionConfiguration: target);

    private static Fixture CreateFixture()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        ExcavationTopologySystem.InitializeFeatures(world);
        EntityId energyRoot = world.Entities.Create();
        world.Entities.Ownership.Set(energyRoot, new Ownership { PlayerSlot = 0 });
        ContentId hqType = StableId.FromKey("building.rock_raiders.hq");
        world.Entities.Transform.Set(energyRoot, new SimTransform { Position = FixVec2.FromInts(12, 12), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(energyRoot, new Selectable { IsSelectable = true, ContentType = hqType, Kind = SelectableKind.Building });
        world.Entities.Building.Set(energyRoot, new Building { Type = hqType, AnchorX = 8, AnchorY = 8, FootprintWidth = 8, FootprintHeight = 8, State = BuildingState.Completed });
        world.Entities.EnergyDomainMember.Set(energyRoot, new EnergyDomainMember { DomainRoot = energyRoot });
        world.Entities.EnergyDomain.Set(energyRoot, new EnergyDomain { Reserve = Fix32.FromInt(100), ReserveCapacity = Fix32.FromInt(150) });
        EntityId bank = world.Entities.Create();
        world.Entities.Ownership.Set(bank, new Ownership { PlayerSlot = 0 });
        world.Entities.ResourceBank.Set(bank, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = 200 });
        EntityId hub = world.Entities.Create();
        world.Entities.Ownership.Set(hub, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(hub, new SimTransform { Position = FixVec2.FromInts(40, 40), Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(hub, new Selectable { IsSelectable = true, ContentType = HubType, Kind = SelectableKind.Building });
        world.Entities.Building.Set(hub, new Building { Type = HubType, AnchorX = 37, AnchorY = 37, FootprintWidth = 7, FootprintHeight = 7, State = BuildingState.Completed });
        world.Entities.EnergyDomainMember.Set(hub, new EnergyDomainMember { DomainRoot = energyRoot });
        EntityId trike = world.Entities.Create();
        FixVec2 trikePosition = FixVec2.FromInts(45, 40);
        world.Entities.Ownership.Set(trike, new Ownership { PlayerSlot = 0 });
        world.Entities.Transform.Set(trike, new SimTransform { Position = trikePosition, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(trike, new Selectable { IsSelectable = true, ContentType = T3Type, Kind = SelectableKind.CombatSupport });
        world.Entities.Movement.Set(trike, new Movement { MaxSpeed = Fix32.FromRatio(170, 100), LastPosition = trikePosition });
        world.Entities.Navigation.Set(trike, new NavigationAgent { Footprint = FootprintClass.Medium, Layer = MovementLayer.Ground, Target = trikePosition });
        world.Entities.Vision.Set(trike, new Vision { RadiusBuildCells = 9, LastFogX = -1, LastFogY = -1 });
        MissionRefitSystem.EnsureState(world, trike);
        world.Entities.MissionRefitState.Get(trike).SurveyUnlocked = true;
        SimulationRunner runner = new(world);
        world.Entities.EnergyDomain.Get(energyRoot).Reserve = Fix32.FromInt(100);
        return new Fixture(world, runner, energyRoot, bank, trike, ResourceConservation.Measure(world, ResourceType.Ore).Total);
    }

    private readonly record struct Fixture(SimulationWorld World, SimulationRunner Runner, EntityId EnergyRoot, EntityId Bank, EntityId Trike, long OreConservationBefore);
}
}
