using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4RepairTests
{
    [Test]
    public void CrewFieldRepairRestoresHpAndConsumesCanonicalResources()
    {
        SimulationWorld world = PreparedWorld(out EntityId crew, out EntityId hover, out EntityId bank, out EntityId domain);
        Fix32 healthBefore = world.Entities.Health.Get(hover).Current;
        int oreBefore = world.Entities.ResourceBank.Get(bank).ProcessedAmount;
        Fix32 energyBefore = world.Entities.EnergyDomain.Get(domain).Reserve;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Repair,
            new[] { crew }, FixVec2.Zero, targetEntity: hover));

        SimulationRunner runner = new(world);
        ref EnergyDomain isolatedDomain = ref world.Entities.EnergyDomain.Get(domain);
        isolatedDomain.GenerationPerSecond = 0; isolatedDomain.ContinuousDemandPerSecond = 0; isolatedDomain.PoweredDemandPerSecond = 0;
        runner.StepTicks(40);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Health.Get(hover).Current, Is.GreaterThan(healthBefore));
            Assert.That(world.Entities.Health.Get(hover).Current, Is.LessThanOrEqualTo(world.Entities.Health.Get(hover).Maximum));
            Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.LessThan(oreBefore));
            Assert.That(world.Entities.EnergyDomain.Get(domain).Reserve, Is.LessThan(energyBefore));
            Assert.That(world.Entities.Weapon.Get(crew).FireSequence, Is.Zero, "Repairing Crew cannot attack.");
        });
    }

    [Test]
    public void RepairPausesWithoutOreAndNeverCreatesFreeHp()
    {
        SimulationWorld world = PreparedWorld(out EntityId crew, out EntityId hover, out EntityId bank, out _);
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 0;
        Fix32 before = world.Entities.Health.Get(hover).Current;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 2, SimCommandType.Repair,
            new[] { crew }, FixVec2.Zero, targetEntity: hover));

        new SimulationRunner(world).StepTicks(40);

        Assert.That(world.Entities.Health.Get(hover).Current, Is.EqualTo(before));
    }

    [Test]
    public void ActiveRepairRoundTripsWithDeterministicContinuation()
    {
        SimulationWorld world = PreparedWorld(out EntityId crew, out EntityId hover, out _, out _);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 3, SimCommandType.Repair,
            new[] { crew }, FixVec2.Zero, targetEntity: hover));
        SimulationRunner original = new(world); original.StepTicks(10);
        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world), world.Content);
        SimulationRunner continuation = new(restored);

        original.StepTicks(30); continuation.StepTicks(30);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Entities.Health.Get(hover).Current, Is.EqualTo(world.Entities.Health.Get(hover).Current));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    [Test]
    public void PreparedRepairArenaSuppliesDamagedHoverCrewAndResources()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 4, SimCommandType.DebugPrepareRepairTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));
        new SimulationRunner(world).StepOneTick();
        EntityId crew = Find(world, 0, DebugPlaytestScenario.CrewKey);
        EntityId hover = Find(world, 0, DebugPlaytestScenario.HoverScoutKey);
        EntityId bank = world.Entities.Alive.First(id => world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.Get(id).PlayerSlot == 0);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Builder.Has(crew), Is.True);
            Assert.That(world.Entities.Health.Get(hover).Current, Is.EqualTo(Fix32.FromInt(40)));
            Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.GreaterThanOrEqualTo(500));
        });
    }

    [Test]
    public void IncomingDamageReducesRepairAndDamageToCrewPausesItForOneSecond()
    {
        SimulationWorld world = PreparedWorld(out EntityId crew, out EntityId hover, out _, out _);
        ref Health target = ref world.Entities.Health.Get(hover); target.LastDamageTick = 0;
        ref Health repairer = ref world.Entities.Health.Get(crew); repairer.LastDamageTick = 0;
        Fix32 before = target.Current;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 5, SimCommandType.Repair,
            new[] { crew }, FixVec2.Zero, targetEntity: hover));
        SimulationRunner runner = new(world);

        runner.StepTicks(SimClock.TicksPerSecond);
        Assert.That(target.Current, Is.EqualTo(before), "Damage to the Crew pauses field repair for one second.");
        runner.StepTicks(SimClock.TicksPerSecond);
        Fix32 restored = target.Current - before;
        Assert.That(restored, Is.InRange(Fix32.FromInt(4), Fix32.FromRatio(43, 10)),
            "A target damaged in the previous two seconds receives the canonical 60% repair rate.");
    }

    private static SimulationWorld PreparedWorld(out EntityId crew, out EntityId hover, out EntityId bank, out EntityId domain)
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        crew = Find(world, 0, DebugPlaytestScenario.CrewKey);
        hover = Find(world, 0, DebugPlaytestScenario.HoverScoutKey);
        bank = world.Entities.Alive.First(id => world.Entities.ResourceBank.Has(id) && world.Entities.Ownership.Get(id).PlayerSlot == 0);
        Assert.That(EnergyDomainSystem.TryGetPlayerDomain(world, 0, out domain), Is.True);
        world.Entities.ResourceBank.Get(bank).ProcessedAmount = 500;
        world.Entities.EnergyDomain.Get(domain).Reserve = Fix32.FromInt(100);
        FixVec2 targetPosition = FixVec2.FromInts(40, 40);
        ResetPosition(world, hover, targetPosition);
        ResetPosition(world, crew, targetPosition + new FixVec2(Fix32.FromRatio(5, 4), Fix32.Zero));
        ref Health health = ref world.Entities.Health.Get(hover);
        health.Current = health.Maximum - Fix32.FromInt(40);
        health.LastDamageTick = -1;
        world.Spatial.Rebuild(world.Entities);
        return world;
    }

    private static void ResetPosition(SimulationWorld world, EntityId id, FixVec2 position)
    {
        ref SimTransform transform = ref world.Entities.Transform.Get(id); transform.Position = position;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(id); nav.Target = position; nav.HasTarget = false; nav.PathDirty = false;
        ref Movement movement = ref world.Entities.Movement.Get(id); movement.CurrentSpeed = Fix32.Zero; movement.CurrentVelocity = FixVec2.Zero;
        movement.DesiredMovement = FixVec2.Zero; movement.LastPosition = position; movement.State = MovementState.Idle;
    }

    private static EntityId Find(SimulationWorld world, byte owner, string key)
    {
        ContentId type = StableId.FromKey(key);
        return world.Entities.Alive.First(id => world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type &&
            world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == owner);
    }
}
