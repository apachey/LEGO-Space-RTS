using System;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5ResonanceCoreTests
{
    private static readonly ContentId CommandCoreType = StableId.FromKey("building.ali.etx_command_core");
    private static readonly ContentId ResonanceCoreType = StableId.FromKey("building.ali.resonance_core");

    [Test]
    public void DesiredCommitmentFillsBaselineSlotsSequentiallyAtCanonicalTiming()
    {
        Fixture fixture = CreateFixture(crystals: 5);
        long conservation = ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total;
        Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 2), Is.True);

        fixture.Runner.StepOneTick();
        ResonanceCore transition = fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(transition.TransitionKind, Is.EqualTo(ResonanceTransitionKind.Commit));
            Assert.That(transition.TransitionTotalTicks, Is.EqualTo(160));
            Assert.That(transition.TransitionRemainingTicks, Is.EqualTo(159));
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(4));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total, Is.EqualTo(conservation));
        });

        fixture.Runner.StepTicks(159);
        ResonanceCore oneCommitted = fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(ResonanceCoreSystem.CountCommitted(oneCommitted), Is.EqualTo(1));
            Assert.That(oneCommitted.TransitionKind, Is.EqualTo(ResonanceTransitionKind.None));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(5));
        });

        fixture.Runner.StepTicks(160);
        ResonanceCore completed = fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(ResonanceCoreSystem.CountCommitted(completed), Is.EqualTo(2));
            Assert.That(completed.CommittedSlotMask, Is.EqualTo(0b0000_0011));
            Assert.That(completed.TransitionKind, Is.EqualTo(ResonanceTransitionKind.None));
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(3));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(7));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total, Is.EqualTo(conservation));
        });
    }

    [Test]
    public void WithdrawalRemovesChargeContributionImmediatelyButReturnsCrystalAfterFifteenSeconds()
    {
        Fixture fixture = CreateFixture(crystals: 1);
        ref ResonanceCore core = ref fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        core.CommittedSlotMask = 0b0000_1111;
        core.DesiredCommittedCrystals = 4;
        EnergyDomainSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        long conservation = ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total;
        Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 2), Is.True);

        fixture.Runner.StepOneTick();
        ResonanceCore withdrawing = fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(withdrawing.TransitionKind, Is.EqualTo(ResonanceTransitionKind.Withdraw));
            Assert.That(withdrawing.TransitionTotalTicks, Is.EqualTo(300));
            Assert.That(withdrawing.TransitionRemainingTicks, Is.EqualTo(299));
            Assert.That(ResonanceCoreSystem.CountCommitted(withdrawing), Is.EqualTo(3), "The withdrawing slot stops contributing immediately.");
            Assert.That(ResonanceCoreSystem.CountInstalled(withdrawing), Is.EqualTo(4), "The Crystal remains physically installed until withdrawal completes.");
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(1));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(11));
        });

        fixture.Runner.StepTicks(299);
        Assert.Multiple(() =>
        {
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(2));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(9));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total, Is.EqualTo(conservation));
        });
        fixture.Runner.StepTicks(300);
        ResonanceCore completed = fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(ResonanceCoreSystem.CountCommitted(completed), Is.EqualTo(2));
            Assert.That(fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(3));
            Assert.That(fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).ContinuousDemandPerSecond, Is.EqualTo(7));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total, Is.EqualTo(conservation));
        });
    }

    [Test]
    public void ValidationEnforcesOwnershipCapacityFundsDomainAndOneCorePerCommandCore()
    {
        Fixture fixture = CreateFixture(crystals: 2);
        Assert.Multiple(() =>
        {
            Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 1, fixture.Core, 1), Is.False);
            Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 5), Is.False);
            Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 3), Is.False);
        });

        ref ResonanceCore core = ref fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        core.ExpandedLatticeUnlocked = true;
        fixture.World.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount = 6;
        Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 6), Is.True);

        EntityId duplicate = AddBuilding(fixture.World, ResonanceCoreType, fixture.EnergyRoot, 0);
        Assert.That(ResonanceCoreSystem.TryAttachCore(fixture.World, duplicate, fixture.CommandCore), Is.False);
        EntityId wrongOwnerCommandCore = AddBuilding(fixture.World, CommandCoreType, fixture.EnergyRoot, 1);
        EntityId otherCore = AddBuilding(fixture.World, ResonanceCoreType, fixture.EnergyRoot, 0);
        Assert.That(ResonanceCoreSystem.TryAttachCore(fixture.World, otherCore, wrongOwnerCommandCore), Is.False);
    }

    [Test]
    public void BrownoutPausesOperationWithoutReleasingCommittedCrystals()
    {
        Fixture fixture = CreateFixture(crystals: 0);
        ref ResonanceCore core = ref fixture.World.Entities.ResonanceCore.Get(fixture.Core);
        core.CommittedSlotMask = 0b0000_0011;
        core.DesiredCommittedCrystals = 2;
        EnergyDomainSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        ref EnergyDomain domain = ref fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot);
        domain.Reserve = Fix32.Zero;
        BrownoutSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        EnergyDomain result = fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot);

        Assert.Multiple(() =>
        {
            Assert.That(result.ContinuousDemandPerSecond, Is.EqualTo(7));
            Assert.That(result.IsBrownout, Is.True);
            Assert.That(BrownoutSystem.IsOperational(fixture.World, fixture.Core), Is.False);
            Assert.That(ResonanceCoreSystem.CountCommitted(fixture.World.Entities.ResonanceCore.Get(fixture.Core)), Is.EqualTo(2));
            Assert.That(ResourceConservation.Measure(fixture.World, ResourceType.Crystal).Total, Is.EqualTo(2));
        });
    }

    [Test]
    public void ActiveCommitmentPreservesDeterministicSnapshotContinuation()
    {
        Fixture fixture = CreateFixture(crystals: 4);
        fixture.World.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.SetResonanceCommitment, Array.Empty<EntityId>(), FixVec2.Zero,
            targetEntity: fixture.Core, desiredResonanceCommitment: 1));
        fixture.World.Commands.Enqueue(new CommandEnvelope(new SimTick(100), 0, 2, SimCommandType.SetResonanceCommitment, Array.Empty<EntityId>(), FixVec2.Zero,
            targetEntity: fixture.Core, desiredResonanceCommitment: 2));
        fixture.Runner.StepTicks(80);
        Assert.That(fixture.World.Entities.ResonanceCore.Get(fixture.Core).TransitionRemainingTicks, Is.EqualTo(80));
        Assert.That(fixture.World.Commands.Count, Is.EqualTo(1));
        ulong before = StateHasher.Hash(fixture.World);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(fixture.World));
        Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(before));
        SimulationRunner restored = new(restoredWorld);
        fixture.Runner.StepTicks(240);
        restored.StepTicks(240);
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(fixture.World)));
            Assert.That(ResonanceCoreSystem.CountCommitted(restoredWorld.Entities.ResonanceCore.Get(fixture.Core)), Is.EqualTo(2));
            Assert.That(restoredWorld.Entities.ResourceBank.Get(fixture.Bank).ProcessedAmount, Is.EqualTo(2));
        });
    }

    private static Fixture CreateFixture(int crystals)
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        ExcavationTopologySystem.InitializeFeatures(world);
        EntityId energyRoot = AddBuilding(world, StableId.FromKey("building.rock_raiders.hq"), EntityId.None, 0);
        world.Entities.EnergyDomainMember.Set(energyRoot, new EnergyDomainMember { DomainRoot = energyRoot });
        world.Entities.EnergyDomain.Set(energyRoot, new EnergyDomain());
        EntityId commandCore = AddBuilding(world, CommandCoreType, energyRoot, 0);
        EntityId core = AddBuilding(world, ResonanceCoreType, energyRoot, 0);
        EntityId bank = world.Entities.Create();
        world.Entities.Ownership.Set(bank, new Ownership { PlayerSlot = 0 });
        world.Entities.ResourceBank.Set(bank, new ResourceBank { Type = ResourceType.Crystal, ProcessedAmount = crystals });
        Assert.That(ResonanceCoreSystem.TryAttachCore(world, core, commandCore), Is.True);
        SimulationRunner runner = new(world);
        world.Entities.EnergyDomain.Get(energyRoot).Reserve = Fix32.FromInt(100);
        return new Fixture(world, runner, energyRoot, commandCore, core, bank);
    }

    private static EntityId AddBuilding(SimulationWorld world, ContentId type, EntityId domainRoot, byte owner)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, State = BuildingState.Completed, FootprintWidth = 4, FootprintHeight = 4 });
        if (domainRoot != EntityId.None) world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = domainRoot });
        return id;
    }

    private readonly record struct Fixture(SimulationWorld World, SimulationRunner Runner, EntityId EnergyRoot, EntityId CommandCore, EntityId Core, EntityId Bank);
}
}
