using System.Linq;
using System.Collections.Generic;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4DestructionTests
{
    [Test]
    public void StandardUnitImmediatelyLosesGameplayAndClearsAfterTwentyFiveTicks()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId crew = Find(world, 0, "unit.rock_raiders.crew");
        FixVec2 position = world.Entities.Transform.Get(crew).Position;
        ref Health health = ref world.Entities.Health.Get(crew); health.Current = Fix32.Zero;
        SimulationRunner runner = new(world);

        runner.StepOneTick();

        DestructionState destruction = world.Entities.Destruction.Get(crew);
        List<EntityId> nearby = new(); world.Spatial.Query(position, 2, nearby);
        Assert.Multiple(() =>
        {
            Assert.That(destruction.Kind, Is.EqualTo(DestructionKind.Unit));
            Assert.That(destruction.BlockingUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.StandardUnitBlockingTicks));
            Assert.That(destruction.VisualUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.StandardUnitVisualTicks));
            Assert.That(world.Entities.Selectable.Has(crew), Is.False);
            Assert.That(world.Entities.Ownership.Has(crew), Is.False);
            Assert.That(world.Entities.Targetable.Has(crew), Is.False);
            Assert.That(world.Entities.Weapon.Has(crew), Is.False);
            Assert.That(world.Entities.Navigation.Has(crew), Is.True, "The blocking wreck retains its collision footprint.");
            Assert.That(nearby, Does.Contain(crew));
        });

        runner.StepTicks(DestructionSystem.StandardUnitBlockingTicks - 1);
        Assert.That(world.Entities.Exists(crew), Is.True);
        runner.StepOneTick();
        Assert.That(world.Entities.Exists(crew), Is.False);
    }

    [Test]
    public void MassiveMachineUsesCanonicalTwoAndHalfSecondBlockingWreck()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId crusher = Find(world, 0, "unit.rock_raiders.chrome_crusher");
        ref Health health = ref world.Entities.Health.Get(crusher); health.Current = Fix32.Zero;
        SimulationRunner runner = new(world);

        runner.StepOneTick();

        DestructionState destruction = world.Entities.Destruction.Get(crusher);
        Assert.Multiple(() =>
        {
            Assert.That(destruction.BlockingUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.MassiveUnitBlockingTicks));
            Assert.That(destruction.VisualUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.MassiveUnitVisualTicks));
        });
        runner.StepTicks(DestructionSystem.MassiveUnitBlockingTicks - 1);
        Assert.That(world.Entities.Exists(crusher), Is.True);
        runner.StepOneTick();
        Assert.That(world.Entities.Exists(crusher), Is.False);
    }

    [Test]
    public void StructureStopsFunctionImmediatelyAndRubbleClearsAfterFourSeconds()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId hq = Find(world, 0, "building.rock_raiders.hq");
        Building building = world.Entities.Building.Get(hq);
        NavCell footprintCell = new((short)(building.AnchorX * MapGrid.NavPerBuild), (short)(building.AnchorY * MapGrid.NavPerBuild));
        Assert.That(world.Pathfinder.IsPassable(footprintCell, FootprintClass.Tiny), Is.False);
        ref Health health = ref world.Entities.Health.Get(hq); health.Current = Fix32.Zero;
        SimulationRunner runner = new(world);

        runner.StepOneTick();

        DestructionState destruction = world.Entities.Destruction.Get(hq);
        Assert.Multiple(() =>
        {
            Assert.That(destruction.Kind, Is.EqualTo(DestructionKind.Structure));
            Assert.That(destruction.BlockingUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.StructureBlockingTicks));
            Assert.That(world.Entities.Building.Has(hq), Is.False);
            Assert.That(world.Entities.Production.Has(hq), Is.False);
            Assert.That(world.Entities.ResourceBank.Has(hq), Is.False);
            Assert.That(world.Entities.EnergyDomain.Has(hq), Is.False);
            Assert.That(world.Pathfinder.IsPassable(footprintCell, FootprintClass.Tiny), Is.False, "Collapse rubble remains blocking.");
            PresentationEntity presentation = PresentationSnapshot.Capture(world, 0).Entities.Single(candidate => candidate.EntityId == hq);
            Assert.That(presentation.PersistentDebris, Is.True, "Structures leave pathable cosmetic debris after collapse.");
        });

        runner.StepTicks(DestructionSystem.StructureBlockingTicks - 1);
        Assert.That(world.Entities.Exists(hq), Is.True);
        runner.StepOneTick();
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(hq), Is.False);
            Assert.That(world.Pathfinder.IsPassable(footprintCell, FootprintClass.Tiny), Is.True);
        });
    }

    [Test]
    public void MidWreckSnapshotRoundTripsAndContinuesAtTheSameRemovalTick()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId crew = Find(world, 0, "unit.rock_raiders.crew");
        ref Health health = ref world.Entities.Health.Get(crew); health.Current = Fix32.Zero;
        SimulationRunner original = new(world);
        original.StepTicks(11);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        SimulationRunner restored = new(restoredWorld);
        Assert.Multiple(() =>
        {
            Assert.That(restoredWorld.Entities.Destruction.Get(crew).BlockingUntilTick, Is.EqualTo(world.Entities.Destruction.Get(crew).BlockingUntilTick));
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(world)));
        });

        original.StepTicks(15); restored.StepTicks(15);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Exists(crew), Is.False);
            Assert.That(restoredWorld.Entities.Exists(crew), Is.False);
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(world)));
        });
    }

    [Test]
    public void PresentationPublishesCollapseButNeverKeepsItSelectable()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId crew = Find(world, 0, "unit.rock_raiders.crew");
        ref Health health = ref world.Entities.Health.Get(crew); health.Current = Fix32.Zero;
        new SimulationRunner(world).StepOneTick();

        PresentationEntity entity = PresentationSnapshot.Capture(world, 0).Entities.Single(candidate => candidate.EntityId == crew);
        Assert.Multiple(() =>
        {
            Assert.That(entity.IsDestroyed, Is.True);
            Assert.That(entity.DestructionKind, Is.EqualTo(DestructionKind.Unit));
            Assert.That(entity.NonBlockingDebrisTicks, Is.EqualTo(DestructionSystem.StandardUnitVisualTicks - DestructionSystem.StandardUnitBlockingTicks));
            Assert.That(world.Entities.Selectable.Has(crew), Is.False);
        });
    }

    [Test]
    public void DeterministicDebugCommandPreparesAVisibleEnemyDestructionTest()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId friendly = Find(world, 0, "unit.rock_raiders.crew");
        EntityId enemy = Find(world, 1, "unit.rock_raiders.crew");
        world.Entities.Transform.Get(enemy).Position = world.Entities.Transform.Get(friendly).Position + FixVec2.FromInts(1, 0);
        new VisionSystem().Step(world);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 77, SimCommandType.DebugDestroyVisibleEnemy,
            System.Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: enemy));

        new SimulationRunner(world).StepOneTick();

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Health.Get(enemy).Current, Is.EqualTo(Fix32.Zero));
            Assert.That(world.Entities.Destruction.Has(enemy), Is.True);
            Assert.That(world.Entities.Selectable.Has(enemy), Is.False);
        });
    }

    private static EntityId Find(SimulationWorld world, byte owner, string contentKey)
    {
        ContentId type = StableId.FromKey(contentKey);
        for (int i = 0; i < world.Entities.Alive.Count; i++)
        {
            EntityId id = world.Entities.Alive[i];
            if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type &&
                world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == owner) return id;
        }
        Assert.Fail($"Missing {contentKey} for player {owner}.");
        return EntityId.None;
    }
}
