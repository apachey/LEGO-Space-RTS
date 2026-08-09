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
    public void ThreeSecondCohortStallReflowsOnlyOncePerTick()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(4);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,9,SimCommandType.Move,ids,FixVec2.FromInts(60,74)));
        new SimulationRunner(world).StepOneTick();
        int initialColumns=world.Entities.Navigation.Get(ids[0]).Formation.Columns;
        for(int i=0;i<ids.Length;i++)
        {
            ref Movement move=ref world.Entities.Movement.Get(ids[i]);move.StuckTicks=59;move.State=MovementState.Holding;move.CurrentSpeed=Fix32.Zero;move.CurrentVelocity=FixVec2.Zero;
        }
        new MovementIntentSystem().Step(world);new TransformMovementSystem().Step(world);

        Assert.That(world.FormationReflowDiagnostics,Is.EqualTo(1));
        for(int i=0;i<ids.Length;i++)
        {
            FormationIntent intent=world.Entities.Navigation.Get(ids[i]).Formation;
            Assert.That(intent.Columns,Is.EqualTo(Math.Max(1,initialColumns-1)));
            Assert.That(intent.LastReflowTick,Is.EqualTo(world.Tick.Value));
        }
    }

    [Test]
    public void ReleasedImpracticalSlotSettlesInsideFormationEnvelope()
    {
        SimulationWorld world=ScenarioFactory.CreateFirstControllable(3);EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        world.Commands.Enqueue(new CommandEnvelope(new SimTick(1),0,12,SimCommandType.Move,ids,FixVec2.FromInts(30,74)));
        new SimulationRunner(world).StepOneTick();
        for(int i=0;i<ids.Length;i++)
        {
            ref NavigationAgent nav=ref world.Entities.Navigation.Get(ids[i]);ref Movement move=ref world.Entities.Movement.Get(ids[i]);
            ref SimTransform transform=ref world.Entities.Transform.Get(ids[i]);transform.Position=nav.Formation.Anchor+new FixVec2(Fix32.Zero,Fix32.FromInt(i));move.LastPosition=transform.Position;
            nav.Formation.Columns=1;nav.Formation.LastReflowTick=-1;move.StuckTicks=59;move.State=MovementState.Holding;
        }
        new MovementIntentSystem().Step(world);new TransformMovementSystem().Step(world);

        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);SimTransform transform=world.Entities.Transform.Get(ids[i]);
            Assert.That(nav.Formation.IsActive,Is.False);
            Assert.That(nav.Target,Is.EqualTo(transform.Position));
        }
    }
}
