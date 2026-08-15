using System;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5AlienChargeSurgeTests
{
    private static readonly ContentId CommandCoreType = StableId.FromKey("building.ali.etx_command_core");
    private static readonly ContentId ResonanceCoreType = StableId.FromKey("building.ali.resonance_core");

    [Test]
    public void CommittedCrystalsGenerateCanonicalMillichargeAndClampAtCapacity()
    {
        Fixture fixture = CreateFixture(4);
        AlienChargeState initial = fixture.World.GetAlienCharge(0);
        Assert.Multiple(() =>
        {
            Assert.That(initial.MaximumMillicharge, Is.EqualTo(100_000));
            Assert.That(initial.GenerationMillichargePerSecond, Is.EqualTo(1_600));
            Assert.That(initial.OperationalCoreCount, Is.EqualTo(1));
            Assert.That(initial.CommittedCrystalCount, Is.EqualTo(4));
        });

        fixture.Runner.StepTicks(1_250);
        Assert.That(fixture.World.GetAlienCharge(0).CurrentMillicharge, Is.EqualTo(100_000));
        fixture.Runner.StepTicks(40);
        Assert.That(fixture.World.GetAlienCharge(0).CurrentMillicharge, Is.EqualTo(100_000), "Charge neither overflows nor decays at maximum.");
    }

    [Test]
    public void BrownoutPausesGenerationButRetainsCapacityAndWithdrawalClampsExcess()
    {
        Fixture fixture = CreateFixture(4);
        fixture.Runner.StepTicks(1_250);
        ref EnergyDomain domain = ref fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot);
        domain.Reserve = Fix32.Zero;
        domain.GenerationPerSecond = 0;
        BrownoutSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        AlienChargeState paused = fixture.World.GetAlienCharge(0);
        Assert.Multiple(() =>
        {
            Assert.That(paused.CurrentMillicharge, Is.EqualTo(100_000));
            Assert.That(paused.MaximumMillicharge, Is.EqualTo(100_000));
            Assert.That(paused.GenerationMillichargePerSecond, Is.Zero);
            Assert.That(paused.BrownoutPausedCoreCount, Is.EqualTo(1));
        });
        fixture.Runner.StepTicks(100);
        Assert.That(fixture.World.GetAlienCharge(0).CurrentMillicharge, Is.EqualTo(100_000));

        SetOnline(fixture);
        Assert.That(ResonanceCoreSystem.TrySetDesiredCommitment(fixture.World, 0, fixture.Core, 3), Is.True);
        fixture.Runner.StepOneTick();
        AlienChargeState afterWithdrawalStart = fixture.World.GetAlienCharge(0);
        Assert.Multiple(() =>
        {
            Assert.That(afterWithdrawalStart.MaximumMillicharge, Is.EqualTo(80_000));
            Assert.That(afterWithdrawalStart.CurrentMillicharge, Is.EqualTo(80_000), "Excess Charge discharges immediately when withdrawal removes capacity.");
            Assert.That(afterWithdrawalStart.GenerationMillichargePerSecond, Is.EqualTo(1_200));
        });
    }

    [Test]
    public void SurgeSpendsFiftyChargeThenRunsCanonicalBuildupAndWindow()
    {
        Fixture fixture = CreateFixture(4);
        EntityId receiver = AddReceiver(fixture.World, 0, FixVec2.FromInts(6, 0));
        fixture.Runner.StepTicks(625);
        AlienChargeSystem.SetResonanceInitiationUnlocked(fixture.World, 0, true);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, fixture.Core), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(fixture.World.GetAlienCharge(0).CurrentMillicharge, Is.Zero);
            Assert.That(fixture.World.Entities.SurgeZone.Get(fixture.Core).BuildupRemainingTicks, Is.EqualTo(15));
            Assert.That(AlienChargeSystem.IsSurged(fixture.World, receiver), Is.False);
        });

        fixture.Runner.StepTicks(14);
        Assert.That(fixture.World.Entities.SurgeZone.Get(fixture.Core).BuildupRemainingTicks, Is.EqualTo(1));
        fixture.Runner.StepOneTick();
        SurgeZone active = fixture.World.Entities.SurgeZone.Get(fixture.Core);
        Assert.Multiple(() =>
        {
            Assert.That(active.ActiveRemainingTicks, Is.EqualTo(360));
            Assert.That(active.RadiusBuildCells, Is.EqualTo(12));
            Assert.That(AlienChargeSystem.IsSurged(fixture.World, receiver), Is.True);
            Assert.That(AlienChargeSystem.ApplySurgedCooldownTicks(100), Is.EqualTo(80));
            Assert.That(AlienChargeSystem.ApplySurgedReconfigurationTicks(100), Is.EqualTo(70));
        });

        fixture.Runner.StepTicks(359);
        Assert.That(fixture.World.Entities.SurgeZone.Get(fixture.Core).ActiveRemainingTicks, Is.EqualTo(1));
        fixture.Runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(fixture.World.Entities.SurgeZone.Has(fixture.Core), Is.False);
            Assert.That(AlienChargeSystem.IsSurged(fixture.World, receiver), Is.False);
        });
    }

    [Test]
    public void ValidationAndMembershipEnforceUnlockOwnershipPowerRadiusAndNoStacking()
    {
        Fixture fixture = CreateFixture(4);
        fixture.Runner.StepTicks(625);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, fixture.Core), Is.False, "Research proof-unlock is required.");
        AlienChargeSystem.SetResonanceInitiationUnlocked(fixture.World, 0, true);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 1, fixture.Core), Is.False);

        EntityId secondCommandCore = AddBuilding(fixture.World, CommandCoreType, fixture.EnergyRoot, 0, FixVec2.FromInts(20, 0));
        EntityId secondCore = AddBuilding(fixture.World, ResonanceCoreType, fixture.EnergyRoot, 0, FixVec2.FromInts(10, 0));
        Assert.That(ResonanceCoreSystem.TryAttachCore(fixture.World, secondCore, secondCommandCore), Is.True);
        ref ResonanceCore second = ref fixture.World.Entities.ResonanceCore.Get(secondCore);
        second.CommittedSlotMask = 0b0000_1111;
        second.DesiredCommittedCrystals = 4;
        EnergyDomainSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        AlienChargeSystem.RecalculateAll(fixture.World);
        fixture.Runner.StepTicks(313);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, fixture.Core), Is.True);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, secondCore), Is.True);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, fixture.Core), Is.False, "One anchor cannot host overlapping windows.");

        EntityId owned = AddReceiver(fixture.World, 0, FixVec2.FromInts(5, 0));
        EntityId enemy = AddReceiver(fixture.World, 1, FixVec2.FromInts(5, 0));
        fixture.Runner.StepTicks(15);
        Assert.Multiple(() =>
        {
            Assert.That(fixture.World.Entities.SurgeReceiver.Get(owned).ActiveZone, Is.EqualTo(fixture.Core), "Overlapping zones resolve to the stable lowest anchor without stacking.");
            Assert.That(AlienChargeSystem.IsSurged(fixture.World, enemy), Is.False);
        });
        fixture.World.Entities.Transform.Get(owned).Position = FixVec2.FromInts(30, 0);
        fixture.Runner.StepOneTick();
        Assert.That(AlienChargeSystem.IsSurged(fixture.World, owned), Is.False);

        ref EnergyDomain domain = ref fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot);
        domain.Reserve = Fix32.Zero;
        BrownoutSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        EntityId thirdCommandCore = AddBuilding(fixture.World, CommandCoreType, fixture.EnergyRoot, 0, FixVec2.FromInts(40, 0));
        EntityId thirdCore = AddBuilding(fixture.World, ResonanceCoreType, fixture.EnergyRoot, 0, FixVec2.FromInts(40, 0));
        Assert.That(ResonanceCoreSystem.TryAttachCore(fixture.World, thirdCore, thirdCommandCore), Is.True);
        Assert.That(AlienChargeSystem.TryStartSurge(fixture.World, 0, thirdCore), Is.False, "A browned-out Core is not a valid new Surge anchor.");
    }

    [Test]
    public void ChargeZoneReceiverAndPendingCommandPreserveSnapshotContinuation()
    {
        Fixture fixture = CreateFixture(4);
        EntityId receiver = AddReceiver(fixture.World, 0, FixVec2.FromInts(4, 0));
        fixture.Runner.StepTicks(625);
        AlienChargeSystem.SetResonanceInitiationUnlocked(fixture.World, 0, true);
        int startTick = fixture.World.Tick.Value;
        fixture.World.Commands.Enqueue(new CommandEnvelope(new SimTick(startTick + 1), 0, 1, SimCommandType.StartSurge, Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: fixture.Core));
        fixture.World.Commands.Enqueue(new CommandEnvelope(new SimTick(startTick + 500), 0, 2, SimCommandType.StartSurge, Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: fixture.Core));
        fixture.Runner.StepTicks(8);
        Assert.That(fixture.World.Commands.Count, Is.EqualTo(1));
        ulong before = StateHasher.Hash(fixture.World);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(fixture.World));
        Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(before));
        SimulationRunner restored = new(restoredWorld);
        fixture.Runner.StepTicks(120);
        restored.StepTicks(120);
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(fixture.World)));
            Assert.That(AlienChargeSystem.IsSurged(restoredWorld, receiver), Is.True);
            Assert.That(restoredWorld.GetAlienCharge(0).ResonanceInitiationUnlocked, Is.True);
        });
    }

    private static Fixture CreateFixture(byte committedCrystals)
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        ExcavationTopologySystem.InitializeFeatures(world);
        EntityId energyRoot = AddBuilding(world, StableId.FromKey("building.rock_raiders.hq"), EntityId.None, 0, FixVec2.FromInts(-12, 0));
        world.Entities.EnergyDomainMember.Set(energyRoot, new EnergyDomainMember { DomainRoot = energyRoot });
        world.Entities.EnergyDomain.Set(energyRoot, new EnergyDomain());
        AddBuilding(world, StableId.FromKey("building.rock_raiders.power_station"), energyRoot, 0, FixVec2.FromInts(-12, 4));
        AddBuilding(world, StableId.FromKey("building.rock_raiders.power_station"), energyRoot, 0, FixVec2.FromInts(-12, 8));
        AddBuilding(world, StableId.FromKey("building.rock_raiders.power_station"), energyRoot, 0, FixVec2.FromInts(-12, 12));
        EntityId commandCore = AddBuilding(world, CommandCoreType, energyRoot, 0, FixVec2.FromInts(-4, 0));
        EntityId core = AddBuilding(world, ResonanceCoreType, energyRoot, 0, FixVec2.Zero);
        EntityId crystalBank = world.Entities.Create();
        world.Entities.Ownership.Set(crystalBank, new Ownership { PlayerSlot = 0 });
        world.Entities.ResourceBank.Set(crystalBank, new ResourceBank { Type = ResourceType.Crystal });
        Assert.That(ResonanceCoreSystem.TryAttachCore(world, core, commandCore), Is.True);
        ref ResonanceCore resonance = ref world.Entities.ResonanceCore.Get(core);
        resonance.CommittedSlotMask = (byte)((1 << committedCrystals) - 1);
        resonance.DesiredCommittedCrystals = committedCrystals;
        SimulationRunner runner = new(world);
        SetOnline(new Fixture(world, runner, energyRoot, core));
        return new Fixture(world, runner, energyRoot, core);
    }

    private static void SetOnline(Fixture fixture)
    {
        fixture.World.Entities.EnergyDomain.Get(fixture.EnergyRoot).Reserve = Fix32.FromInt(100);
        BrownoutSystem.Recalculate(fixture.World, fixture.EnergyRoot);
        AlienChargeSystem.RecalculateAll(fixture.World);
    }

    private static EntityId AddReceiver(SimulationWorld world, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.SurgeReceiver.Set(id, new SurgeReceiver());
        return id;
    }

    private static EntityId AddBuilding(SimulationWorld world, ContentId type, EntityId domainRoot, byte owner, FixVec2 position)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building { Type = type, State = BuildingState.Completed, FootprintWidth = 4, FootprintHeight = 4 });
        if (domainRoot != EntityId.None) world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = domainRoot });
        return id;
    }

    private readonly record struct Fixture(SimulationWorld World, SimulationRunner Runner, EntityId EnergyRoot, EntityId Core);
}
}
