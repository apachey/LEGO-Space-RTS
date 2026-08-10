using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4TargetingTests
{
    [Test]
    public void AntiLightPriorityBeatsDistanceAndUsesEntityIdForExactTies()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnTargeter(world, 0, FixVec2.FromInts(20, 20), TargetPriorityProfile.AntiLight);
        SpawnTarget(world, 1, FixVec2.FromInts(21, 20), CombatTargetClass.LightMachine, CombatTargetFlags.CombatThreat);
        EntityId expected = SpawnTarget(world, 1, FixVec2.FromInts(23, 20), CombatTargetClass.Personnel, CombatTargetFlags.Worker);
        SpawnTarget(world, 1, FixVec2.FromInts(17, 20), CombatTargetClass.Personnel, CombatTargetFlags.Worker);

        new SimulationRunner(world).StepOneTick();

        Targeting targeting = world.Entities.Targeting.Get(source);
        Assert.That(targeting.CurrentTarget, Is.EqualTo(expected));
        Assert.That(targeting.SelectionKind, Is.EqualTo(TargetSelectionKind.Automatic));
    }

    [Test]
    public void SiegePriorityUsesDefenseProductionEconomyCommandOrder()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnTargeter(world, 0, FixVec2.FromInts(30, 30), TargetPriorityProfile.Siege);
        SpawnTarget(world, 1, FixVec2.FromInts(31, 30), CombatTargetClass.FortifiedStructure, CombatTargetFlags.Command);
        SpawnTarget(world, 1, FixVec2.FromInts(32, 30), CombatTargetClass.Structure, CombatTargetFlags.EconomicInfrastructure);
        SpawnTarget(world, 1, FixVec2.FromInts(33, 30), CombatTargetClass.Structure, CombatTargetFlags.Production);
        EntityId defense = SpawnTarget(world, 1, FixVec2.FromInts(35, 30), CombatTargetClass.FortifiedStructure, CombatTargetFlags.DefensiveStructure);

        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Entities.Targeting.Get(source).CurrentTarget, Is.EqualTo(defense));
    }

    [Test]
    public void DirectAttackOverridesAutomaticPriorityAndClearsWhenVisionIsLost()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnTargeter(world, 0, FixVec2.FromInts(40, 40), TargetPriorityProfile.AntiLight);
        SpawnTarget(world, 1, FixVec2.FromInts(41, 40), CombatTargetClass.Personnel, CombatTargetFlags.Worker);
        EntityId ordered = SpawnTarget(world, 1, FixVec2.FromInts(45, 40), CombatTargetClass.Structure, CombatTargetFlags.EconomicInfrastructure);
        new VisionSystem().Step(world);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Attack, new[] { source }, FixVec2.Zero, targetEntity: ordered));
        SimulationRunner runner = new(world);

        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Targeting.Get(source).CurrentTarget, Is.EqualTo(ordered));
            Assert.That(world.Entities.Targeting.Get(source).SelectionKind, Is.EqualTo(TargetSelectionKind.DirectOrder));
        });

        world.Entities.Transform.Get(ordered).Position = FixVec2.FromInts(70, 70);
        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Targeting.Get(source).CurrentTarget, Is.EqualTo(EntityId.None));
            Assert.That(world.Entities.Targeting.Get(source).SelectionKind, Is.EqualTo(TargetSelectionKind.None));
        });
    }

    [Test]
    public void HiddenFriendlyAndWrongLayerTargetsAreIllegal()
    {
        SimulationWorld world = NewWorld();
        EntityId source = SpawnTargeter(world, 0, FixVec2.FromInts(50, 50), TargetPriorityProfile.Generalist, TargetLayerMask.Ground);
        EntityId hidden = SpawnTarget(world, 1, FixVec2.FromInts(80, 80), CombatTargetClass.MediumMachine, CombatTargetFlags.CombatThreat);
        EntityId friendly = SpawnTarget(world, 0, FixVec2.FromInts(51, 50), CombatTargetClass.MediumMachine, CombatTargetFlags.CombatThreat);
        EntityId air = SpawnTarget(world, 1, FixVec2.FromInts(52, 50), CombatTargetClass.LightMachine, CombatTargetFlags.CombatThreat, CombatTargetLayer.TrueAir);
        new VisionSystem().Step(world);

        Assert.Multiple(() =>
        {
            Assert.That(TargetingSystem.IsLegalTarget(world, source, hidden, requireVisible: true), Is.False);
            Assert.That(TargetingSystem.IsLegalTarget(world, source, friendly, requireVisible: true), Is.False);
            Assert.That(TargetingSystem.IsLegalTarget(world, source, air, requireVisible: true), Is.False);
        });
    }

    [Test]
    public void TargetingSnapshotAndAttackReplayContinueDeterministically()
    {
        SimulationWorld initial = NewWorld();
        EntityId source = SpawnTargeter(initial, 0, FixVec2.FromInts(60, 60), TargetPriorityProfile.Generalist);
        EntityId target = SpawnTarget(initial, 1, FixVec2.FromInts(63, 60), CombatTargetClass.HeavyMachine, CombatTargetFlags.CombatThreat);
        new VisionSystem().Step(initial);
        ReplayLog authored = new(SnapshotSerializer.Serialize(initial));
        authored.Commands.Add(new CommandEnvelope(new SimTick(1), 0, 1, SimCommandType.Attack, new[] { source }, FixVec2.Zero, targetEntity: target));
        ReplayLog restoredLog = ReplayLog.Deserialize(authored.Serialize());
        SimulationRunner a = authored.CreateRunner(), b = restoredLog.CreateRunner();

        a.StepTicks(5); b.StepTicks(5);

        Assert.Multiple(() =>
        {
            Assert.That(a.World.Entities.Targeting.Get(source).CurrentTarget, Is.EqualTo(target));
            Assert.That(b.World.Entities.Targeting.Get(source).SelectionKind, Is.EqualTo(TargetSelectionKind.DirectOrder));
            Assert.That(StateHasher.Hash(b.World), Is.EqualTo(StateHasher.Hash(a.World)));
            SimulationWorld snapshot = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(a.World));
            Assert.That(StateHasher.Hash(snapshot), Is.EqualTo(StateHasher.Hash(a.World)));
        });
    }

    private static SimulationWorld NewWorld() => new(new MapGrid("map.test.m4_targeting"), 2);

    private static EntityId SpawnTargeter(SimulationWorld world, byte owner, FixVec2 position, TargetPriorityProfile priority,
        TargetLayerMask legalLayers = TargetLayerMask.Ground)
    {
        EntityId id = SpawnTarget(world, owner, position, CombatTargetClass.MediumMachine, CombatTargetFlags.CombatThreat);
        world.Entities.Targeting.Set(id, new Targeting
        {
            CurrentTarget = EntityId.None, AcquisitionRadius = Fix32.FromInt(10), LegalLayers = legalLayers,
            LegalClasses = TargetClassMask.All, PriorityProfile = priority, SelectionKind = TargetSelectionKind.None
        });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = 10, LastFogX = -1, LastFogY = -1 });
        return id;
    }

    private static EntityId SpawnTarget(SimulationWorld world, byte owner, FixVec2 position, CombatTargetClass targetClass,
        CombatTargetFlags flags, CombatTargetLayer layer = CombatTargetLayer.Ground)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Targetable.Set(id, new Targetable { Class = targetClass, Layer = layer, Flags = flags });
        return id;
    }
}
