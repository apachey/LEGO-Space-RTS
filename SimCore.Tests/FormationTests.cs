using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class FormationTests
{
    [Test] public void SlotsRotateWithHeadingAndSpreadIncreasesSeparation()
    {
        FixVec2 center=FixVec2.FromInts(40,40); FixVec2 heading=FixVec2.FromInts(1,0);
        FixVec2 normal=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,heading,3,false);
        FixVec2 spread=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,heading,3,true);
        Assert.That(FixVec2.Distance(center,spread),Is.GreaterThan(FixVec2.Distance(center,normal)));
        FixVec2 north=FormationPlanner.GetSlot(center,0,9,FootprintClass.Medium,FixVec2.FromInts(0,1),3,false);
        Assert.That(north,Is.Not.EqualTo(normal));
    }

    [TestCase(FootprintClass.Tiny)]
    [TestCase(FootprintClass.Small)]
    [TestCase(FootprintClass.Medium)]
    [TestCase(FootprintClass.Large)]
    [TestCase(FootprintClass.Huge)]
    public void CompletedFormationSlotsLeaveCanonicalSettlingMargin(FootprintClass footprint)
    {
        FixVec2 center=FixVec2.FromInts(40,40);
        FixVec2 first=FormationPlanner.GetSlot(center,0,2,footprint,FixVec2.FromInts(1,0),2,false);
        FixVec2 second=FormationPlanner.GetSlot(center,1,2,footprint,FixVec2.FromInts(1,0),2,false);
        Fix32 canonicalMinimum=FootprintRules.CollisionRadiusBuild(footprint)*Fix32.FromInt(2)+Fix32.FromRatio(35,100);

        Assert.That(FixVec2.Distance(first,second).Raw,Is.GreaterThanOrEqualTo(canonicalMinimum.Raw),
            $"{footprint} arrival slots would keep local separation active after the Move completed.");
    }

    [Test]
    public void MultiUnitMoveCreatesPersistentDeterministicCohortIntent()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        FixVec2 anchor=FixVec2.FromInts(60,74);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,77,SimCommandType.Move,ids,anchor));
        new SimulationRunner(world).StepOneTick();

        FormationIntent first=world.Entities.Navigation.Get(ids[0]).Formation;
        Assert.That(first.IsActive,Is.True);Assert.That(first.CohortId,Is.EqualTo(77));Assert.That(first.Anchor,Is.EqualTo(anchor));
        for(int i=0;i<ids.Length;i++)
        {
            FormationIntent intent=world.Entities.Navigation.Get(ids[i]).Formation;
            Assert.That(intent.CohortId,Is.EqualTo(first.CohortId));Assert.That(intent.MemberCount,Is.EqualTo(ids.Length));
        }

        SimulationWorld restored=SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        for(int i=0;i<ids.Length;i++)Assert.That(restored.Entities.Navigation.Get(ids[i]).Formation.CohortId,Is.EqualTo(first.CohortId));
    }

    [Test]
    public void MoveAssignsSameRoleUnitsToNearestFinalSlotsInsteadOfEntityOrder()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        EntityId small=ids[1],medium=ids[2];
        ref SimTransform smallTransform=ref world.Entities.Transform.Get(small);smallTransform.Position=FixVec2.FromInts(24,82);
        ref SimTransform mediumTransform=ref world.Entities.Transform.Get(medium);mediumTransform.Position=FixVec2.FromInts(24,66);
        world.Spatial.Rebuild(world.Entities);

        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,78,SimCommandType.Move,new[]{small,medium},FixVec2.FromInts(60,74)));
        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Entities.Navigation.Get(small).Formation.SlotIndex,Is.EqualTo(1),"The upper unit should receive the upper slot instead of crossing the other unit's route.");
        Assert.That(world.Entities.Navigation.Get(medium).Formation.SlotIndex,Is.EqualTo(0),"The lower unit should receive the lower slot instead of crossing the other unit's route.");
    }

    [Test]
    public void MinimumCostAssignmentFindsGlobalOptimumAndUsesStableTieBreaks()
    {
        long[,] costs=
        {
            { 1, 2, 2 },
            { 1, 100, 100 },
            { 100, 1, 1 }
        };

        Assert.That(FormationSlotAssignment.Solve(costs),Is.EqualTo(new[]{1,0,2}));
        Assert.That(FormationSlotAssignment.Solve(new long[,]{{5,5},{5,5}}),Is.EqualTo(new[]{0,1}));
    }

    [Test]
    public void SpatialAssignmentDoesNotSwapCanonicalRoleBands()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        EntityId worker=ids[0],heavy=ids[3];
        ref SimTransform workerTransform=ref world.Entities.Transform.Get(worker);workerTransform.Position=FixVec2.FromInts(50,74);
        ref SimTransform heavyTransform=ref world.Entities.Transform.Get(heavy);heavyTransform.Position=FixVec2.FromInts(20,74);
        world.Spatial.Rebuild(world.Entities);

        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,79,SimCommandType.Move,new[]{worker,heavy},FixVec2.FromInts(60,74)));
        new SimulationRunner(world).StepOneTick();

        Assert.That(world.Entities.Navigation.Get(heavy).Formation.SlotIndex,Is.EqualTo(0));
        Assert.That(world.Entities.Navigation.Get(worker).Formation.SlotIndex,Is.EqualTo(1));
    }

    [Test]
    public void CollectiveSlotCloudRemainsPassableAndNonOverlappingNearTerrain()
    {
        SimulationWorld world=ScenarioFactory.CreateRepresentative24();EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        EntityId[] cohort=new EntityId[10];System.Array.Copy(ids,10,cohort,0,10);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,80,SimCommandType.Move,cohort,FixVec2.FromInts(42,107)));
        new SimulationRunner(world).StepOneTick();

        for(int i=0;i<cohort.Length;i++)
        {
            NavigationAgent first=world.Entities.Navigation.Get(cohort[i]);
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(first.Target),first.Footprint),Is.True);
            for(int j=i+1;j<cohort.Length;j++)
            {
                NavigationAgent second=world.Entities.Navigation.Get(cohort[j]);
                Fix32 minimum=FootprintRules.CollisionRadiusBuild(first.Footprint)+FootprintRules.CollisionRadiusBuild(second.Footprint)+Fix32.FromRatio(35,100);
                Assert.That(FixVec2.Distance(first.Target,second.Target).Raw,Is.GreaterThanOrEqualTo(minimum.Raw),$"Slots {i}/{j} collapsed near terrain.");
            }
        }
    }

    [Test]
    public void ArrivalSequencerKeepsFinalEndpointsImmutableAndRoutesLaterRowsToStaging()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(10);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,81,SimCommandType.Move,ids,FixVec2.FromInts(60,74)));
        new SimulationRunner(world).StepOneTick();

        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);
            if(nav.Formation.SlotIndex/nav.Formation.Columns>0)world.Entities.Transform.Get(ids[i]).Position=nav.Target-nav.Formation.Heading.NormalizeSafe()*Fix32.FromInt(3);
        }
        new FormationArrivalSystem().Step(world);new NavigationRequestSystem().Step(world);

        FixVec2[] stagingTargets=new FixVec2[ids.Length];int stagingCount=0;
        for(int i=0;i<ids.Length;i++)
        {
            EntityId id=ids[i];NavigationAgent nav=world.Entities.Navigation.Get(id);RouteCorridor corridor=world.GetCorridor(id)!;
            int row=nav.Formation.SlotIndex/nav.Formation.Columns;
            FixVec2 routeGoal=MapGrid.NavCellCenterToBuild(corridor.Cells[corridor.Cells.Count-1]);
            if(row==0)Assert.That(MapGrid.BuildToNav(routeGoal),Is.EqualTo(MapGrid.BuildToNav(nav.Target)));
            else
            {
                FixVec2 staging=FormationPlanner.GetArrivalStagingTarget(world,nav);stagingTargets[stagingCount++]=staging;
                Assert.That(nav.Target,Is.Not.EqualTo(staging),"Staging must not overwrite the assigned final endpoint.");
                Assert.That(MapGrid.BuildToNav(routeGoal),Is.EqualTo(MapGrid.BuildToNav(staging)));
                Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(staging),nav.Footprint),Is.True);
            }
        }
        for(int i=0;i<stagingCount;i++)for(int j=i+1;j<stagingCount;j++)
            Assert.That(FixVec2.Distance(stagingTargets[i],stagingTargets[j]).Raw,Is.GreaterThanOrEqualTo(Fix32.FromRatio(7,10).Raw));
    }

    [Test]
    public void ArrivalSequencerReleasesNextRowAfterFrontRowCompletes()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(10);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,82,SimCommandType.Move,ids,FixVec2.FromInts(60,74)));
        SimulationRunner runner=new(world);runner.StepOneTick();

        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);
            if(nav.Formation.SlotIndex/nav.Formation.Columns>0)world.Entities.Transform.Get(ids[i]).Position=nav.Target-nav.Formation.Heading.NormalizeSafe()*Fix32.FromInt(3);
        }
        new FormationArrivalSystem().Step(world);new NavigationRequestSystem().Step(world);

        for(int i=0;i<ids.Length;i++)
        {
            EntityId id=ids[i];NavigationAgent nav=world.Entities.Navigation.Get(id);
            if(nav.Formation.SlotIndex/nav.Formation.Columns!=0)continue;
            ref SimTransform transform=ref world.Entities.Transform.Get(id);transform.Position=nav.Target;
            ref Movement movement=ref world.Entities.Movement.Get(id);movement.LastPosition=transform.Position;
        }
        runner.StepTicks(2);

        for(int i=0;i<ids.Length;i++)
        {
            EntityId id=ids[i];NavigationAgent nav=world.Entities.Navigation.Get(id);
            if(!nav.HasTarget)continue;
            RouteCorridor corridor=world.GetCorridor(id)!;
            Assert.That(corridor.Cells[corridor.Cells.Count-1],Is.EqualTo(MapGrid.BuildToNav(nav.Target)));
        }
    }

    [Test]
    public void StopAndReplacementMoveDissolveOldCohortMembership()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(3);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        SimulationRunner runner=new(world);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,1,SimCommandType.Move,ids,FixVec2.FromInts(60,74)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2),0,2,SimCommandType.Move,new[]{ids[0]},FixVec2.FromInts(50,70)));
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(2),0,3,SimCommandType.Stop,new[]{ids[1]},FixVec2.Zero));
        runner.StepTicks(2);

        Assert.That(world.Entities.Navigation.Get(ids[0]).Formation.IsActive,Is.False);
        Assert.That(world.Entities.Navigation.Get(ids[1]).Formation.IsActive,Is.False);
        Assert.That(world.Entities.Navigation.Get(ids[2]).Formation.IsActive,Is.True);
    }

    [Test]
    public void ThreeSecondCohortStallRefreshesRoutesWithoutRewritingAssignedEndpoints()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,9,SimCommandType.Move,ids,FixVec2.FromInts(60,74)));
        new SimulationRunner(world).StepOneTick();
        FixVec2[] assignedTargets=new FixVec2[ids.Length];
        for(int i=0;i<ids.Length;i++)
        {
            assignedTargets[i]=world.Entities.Navigation.Get(ids[i]).Target;
            ref Movement move=ref world.Entities.Movement.Get(ids[i]);move.StuckTicks=59;move.State=MovementState.Holding;move.CurrentSpeed=Fix32.Zero;move.CurrentVelocity=FixVec2.Zero;
        }
        Assert.That(FormationPlanner.ReflowCohort(world,ids[0]),Is.True);

        Assert.That(world.FormationReflowDiagnostics,Is.EqualTo(1));
        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);
            Assert.That(nav.Formation.IsActive,Is.True);
            Assert.That(nav.Formation.LastReflowTick,Is.EqualTo(world.Tick.Value));
            Assert.That(nav.HasTarget,Is.True);
            Assert.That(nav.Target,Is.EqualTo(assignedTargets[i]));
            Assert.That(nav.PathDirty,Is.True);
        }
    }

    [Test]
    public void ImpracticalSlotRecoveryDoesNotConvertCurrentPositionIntoCompletedMove()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(3);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,12,SimCommandType.Move,ids,FixVec2.FromInts(30,74)));
        new SimulationRunner(world).StepOneTick();
        FixVec2[] assignedTargets=new FixVec2[ids.Length];
        for(int i=0;i<ids.Length;i++)
        {
            ref NavigationAgent nav=ref world.Entities.Navigation.Get(ids[i]);ref Movement move=ref world.Entities.Movement.Get(ids[i]);
            assignedTargets[i]=nav.Target;
            ref SimTransform transform=ref world.Entities.Transform.Get(ids[i]);transform.Position=nav.Formation.Anchor+new FixVec2(Fix32.Zero,Fix32.FromInt(i+5));move.LastPosition=transform.Position;
            nav.Formation.Columns=1;nav.Formation.LastReflowTick=-1;move.StuckTicks=59;move.State=MovementState.Holding;
        }
        Assert.That(FormationPlanner.ReflowCohort(world,ids[0]),Is.True);

        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);SimTransform transform=world.Entities.Transform.Get(ids[i]);
            Assert.That(nav.Formation.IsActive,Is.True);
            Assert.That(nav.HasTarget,Is.True);
            Assert.That(nav.Target,Is.EqualTo(assignedTargets[i]));
            Assert.That(nav.Target,Is.Not.EqualTo(transform.Position));
        }
    }
}
