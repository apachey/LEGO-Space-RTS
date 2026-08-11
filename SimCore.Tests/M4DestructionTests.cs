using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public sealed class M4DestructionTests
{
    [Test]
    public void ConstructionPlaytestPreparationStartsFromCanonicalOpeningWithoutResourceGrind()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 91, SimCommandType.DebugPrepareConstructionTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));

        new SimulationRunner(world).StepOneTick();

        EntityId site = world.Entities.Alive.Single(id => world.Entities.ConstructionSite.Has(id));
        ConstructionSite construction = world.Entities.ConstructionSite.Get(site);
        EntityId bank = world.Entities.Alive.Single(id => world.Entities.ResourceBank.Has(id) &&
            world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0);
        Assert.Multiple(() =>
        {
            Assert.That(construction.ProgressTicks, Is.GreaterThan(0));
            Assert.That(world.Entities.Building.Get(site).State, Is.EqualTo(BuildingState.ConstructionSite));
            Assert.That(world.Entities.ResourceBank.Get(bank).ProcessedAmount, Is.GreaterThan(4_000));
            Assert.That(world.Entities.Ownership.Get(site).PlayerSlot, Is.Zero);
        });
    }

    [Test]
    public void DestructionPlaytestPreparationProvidesExactTargetsFromCanonicalOpening()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        Assert.That(FindOptional(world, 0, DebugPlaytestScenario.ChromeCrusherKey), Is.EqualTo(EntityId.None),
            "The shipped opening deliberately contains no Chrome before debug preparation.");
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 92, SimCommandType.DebugPrepareDestructionTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));

        new SimulationRunner(world).StepOneTick();

        EntityId friendlyChrome = Find(world, 0, DebugPlaytestScenario.ChromeCrusherKey);
        EntityId enemyCrew = FindClosest(world, 1, DebugPlaytestScenario.CrewKey, DebugPlaytestScenario.DestructionArenaCenter);
        EntityId enemyChrome = Find(world, 1, DebugPlaytestScenario.ChromeCrusherKey);
        EntityId enemyBuilding = Find(world, 1, DebugPlaytestScenario.DestructionBuildingKey);
        Assert.Multiple(() =>
        {
            Assert.That(FixVec2.Distance(world.Entities.Transform.Get(friendlyChrome).Position, DebugPlaytestScenario.DestructionArenaCenter), Is.LessThan(Fix32.FromInt(20)));
            Assert.That(FixVec2.Distance(world.Entities.Transform.Get(enemyCrew).Position, DebugPlaytestScenario.DestructionArenaCenter), Is.LessThan(Fix32.FromInt(20)));
            Assert.That(FixVec2.Distance(world.Entities.Transform.Get(enemyChrome).Position, DebugPlaytestScenario.DestructionArenaCenter), Is.LessThan(Fix32.FromInt(20)));
            Assert.That(world.Entities.Building.Get(enemyBuilding).State, Is.EqualTo(BuildingState.Completed));
            Assert.That(world.Entities.Health.Get(enemyCrew).Current, Is.EqualTo(Fix32.FromInt(12)));
            Assert.That(world.Entities.Health.Get(enemyChrome).Current, Is.EqualTo(Fix32.FromInt(160)));
            Assert.That(world.Entities.Health.Get(enemyBuilding).Current, Is.EqualTo(Fix32.FromInt(180)));
            Assert.That(world.Fog.IsVisible(0, world.Entities.Transform.Get(enemyBuilding).Position.X.FloorToInt(),
                world.Entities.Transform.Get(enemyBuilding).Position.Y.FloorToInt()), Is.True);
            Assert.That(world.Fog.IsVisible(0, world.Entities.Transform.Get(enemyCrew).Position.X.FloorToInt(),
                world.Entities.Transform.Get(enemyCrew).Position.Y.FloorToInt()), Is.True);
            Assert.That(world.Fog.IsVisible(0, world.Entities.Transform.Get(enemyChrome).Position.X.FloorToInt(),
                world.Entities.Transform.Get(enemyChrome).Position.Y.FloorToInt()), Is.True);
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(world.Entities.Transform.Get(friendlyChrome).Position),
                world.Entities.Navigation.Get(friendlyChrome).Footprint), Is.True);
            Assert.That(CombatGeometry.ContactGap(world, friendlyChrome, enemyBuilding), Is.GreaterThan(Fix32.Zero));
            Assert.That(CombatGeometry.ContactGap(world, enemyCrew, enemyBuilding), Is.GreaterThan(Fix32.Zero));
            Assert.That(CombatGeometry.ContactGap(world, enemyChrome, enemyBuilding), Is.GreaterThan(Fix32.Zero));
        });
    }

    [Test]
    public void StandardUnitImmediatelyLosesGameplayAndCollisionButKeepsCosmeticDebris()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(18);
        EntityId crew = Find(world, 0, "unit.rock_raiders.crew");
        ref Health health = ref world.Entities.Health.Get(crew); health.Current = Fix32.Zero;
        SimulationRunner runner = new(world);

        runner.StepOneTick();

        DestructionState destruction = world.Entities.Destruction.Get(crew);
        Assert.Multiple(() =>
        {
            Assert.That(destruction.Kind, Is.EqualTo(DestructionKind.Unit));
            Assert.That(destruction.BlockingUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.StandardUnitBlockingTicks));
            Assert.That(destruction.VisualUntilTick - destruction.StartedTick, Is.EqualTo(DestructionSystem.StandardUnitVisualTicks));
            Assert.That(world.Entities.Selectable.Has(crew), Is.False);
            Assert.That(world.Entities.Ownership.Has(crew), Is.False);
            Assert.That(world.Entities.Targetable.Has(crew), Is.False);
            Assert.That(world.Entities.Weapon.Has(crew), Is.False);
            Assert.That(world.Entities.Navigation.Has(crew), Is.False, "A 0-HP unit must stop blocking in the same tick.");
            Assert.That(world.Entities.Movement.Has(crew), Is.False);
            Assert.That(world.Entities.Exists(crew), Is.True, "The nonblocking visual wreck remains authoritative until its visual expiry.");
        });

        runner.StepTicks(DestructionSystem.StandardUnitVisualTicks - 1);
        Assert.That(world.Entities.Exists(crew), Is.True);
        runner.StepOneTick();
        Assert.That(world.Entities.Exists(crew), Is.False);
    }

    [Test]
    public void MassiveMachineImmediatelyStopsBlockingButKeepsTwelveSecondDebris()
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
            Assert.That(world.Entities.Navigation.Has(crusher), Is.False);
            Assert.That(world.Entities.Movement.Has(crusher), Is.False);
        });
        runner.StepTicks(DestructionSystem.MassiveUnitVisualTicks - 1);
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
    public void MidNonblockingDebrisSnapshotRoundTripsAndContinuesAtTheSameVisualRemovalTick()
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
            Assert.That(restoredWorld.Entities.Navigation.Has(crew), Is.False);
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(world)));
        });

        original.StepTicks(DestructionSystem.StandardUnitVisualTicks - 11);
        restored.StepTicks(DestructionSystem.StandardUnitVisualTicks - 11);
        Assert.That(world.Entities.Exists(crew), Is.True);
        Assert.That(restoredWorld.Entities.Exists(crew), Is.True);
        original.StepOneTick(); restored.StepOneTick();
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
            Assert.That(entity.DestructionProgressBasisPoints, Is.EqualTo(10_000));
            Assert.That(entity.NonBlockingDebrisTicks, Is.Zero);
            Assert.That(world.Entities.Selectable.Has(crew), Is.False);
            Assert.That(world.Entities.Navigation.Has(crew), Is.False);
        });
    }

    [Test]
    public void PreparedChromeCanApproachAndAttackThePreparedBuilding()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 93, SimCommandType.DebugPrepareDestructionTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));
        SimulationRunner runner = new(world); runner.StepOneTick();
        EntityId chrome = Find(world, 0, DebugPlaytestScenario.ChromeCrusherKey);
        EntityId building = Find(world, 1, DebugPlaytestScenario.DestructionBuildingKey);
        FixVec2 start = world.Entities.Transform.Get(chrome).Position;
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2), 0, 94, SimCommandType.Attack,
            new[] { chrome }, FixVec2.Zero, targetEntity: building));

        runner.StepTicks(180);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Weapon.Get(chrome).FireSequence, Is.GreaterThan(0));
            Assert.That(FixVec2.Distance(start, world.Entities.Transform.Get(chrome).Position), Is.GreaterThan(Fix32.One));
        });
    }

    [Test]
    public void PreparedHoverAttacksBuildingFromOutsideItsFootprint()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1), 0, 95, SimCommandType.DebugPrepareDestructionTest,
            System.Array.Empty<EntityId>(), FixVec2.Zero));
        SimulationRunner runner = new(world); runner.StepOneTick();
        EntityId hover = Find(world, 0, DebugPlaytestScenario.HoverScoutKey);
        EntityId building = Find(world, 1, DebugPlaytestScenario.DestructionBuildingKey);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2), 0, 96, SimCommandType.Attack,
            new[] { hover }, FixVec2.Zero, targetEntity: building));

        runner.StepTicks(90);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.Weapon.Get(hover).FireSequence, Is.GreaterThan(0));
            Assert.That(CombatGeometry.ContactGap(world, hover, building), Is.GreaterThan(Fix32.Zero));
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

    private static EntityId FindOptional(SimulationWorld world, byte owner, string contentKey)
    {
        ContentId type = StableId.FromKey(contentKey);
        for (int i = 0; i < world.Entities.Alive.Count; i++)
        {
            EntityId id = world.Entities.Alive[i];
            if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == type &&
                world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == owner) return id;
        }
        return EntityId.None;
    }

    private static EntityId FindClosest(SimulationWorld world, byte owner, string contentKey, FixVec2 origin)
    {
        ContentId type = StableId.FromKey(contentKey);
        EntityId best = EntityId.None;
        Fix32 bestDistance = Fix32.MaxValue;
        for (int i = 0; i < world.Entities.Alive.Count; i++)
        {
            EntityId id = world.Entities.Alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != type ||
                !world.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != owner ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            Fix32 distance = FixVec2.Distance(origin, transform.Position);
            if (best == EntityId.None || distance < bestDistance) { best = id; bestDistance = distance; }
        }
        Assert.That(best, Is.Not.EqualTo(EntityId.None), $"Missing prepared {contentKey} for player {owner}.");
        return best;
    }
}
