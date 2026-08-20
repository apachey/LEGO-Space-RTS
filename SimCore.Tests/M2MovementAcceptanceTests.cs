using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M2MovementAcceptanceTests
{
    private const int PhaseDeadlineTicks=5000;
    private static readonly Fix32 CollisionTolerance=Fix32.FromRatio(1,100);

    [Test]
    public void Representative24MoversCompleteDeterministicallyAndLegally()
    {
        SimulationRunner first=CreateRepresentative();SimulationRunner repeat=CreateRepresentative();
        EntityId[] ids=ScenarioFactory.OwnedIds(first.World,0);EntityId[] repeatIds=ScenarioFactory.OwnedIds(repeat.World,0);
        Assert.That(ids.Length,Is.EqualTo(24));AssertAllFootprintsRepresented(first.World,ids);AssertLegalStarts(first.World,ids);AssertTravelDeadlineIsFeasible(first.World,ids);

        first.StepOneTick();repeat.StepOneTick();
        AssertCorridorCrossesGap(first.World,ids[0],124,149,"medium 13-build-cell passage");
        AssertCorridorCrossesGap(first.World,ids[10],196,231,"Heavy 18-build-cell route");
        RunUntilComplete(first,ids,PhaseDeadlineTicks,true);RunUntilComplete(repeat,repeatIds,PhaseDeadlineTicks,false);
        Assert.That(first.World.FormationReflowDiagnostics,Is.GreaterThan(0),"The constrained-passage phase did not exercise formation reflow.");

        EntityId[] westExchange={ids[20],ids[21]};EntityId[] eastExchange={ids[22],ids[23]};
        Assert.That(first.World.Entities.Navigation.Get(ids[20]).Footprint,Is.EqualTo(FootprintClass.Small));
        Assert.That(first.World.Entities.Navigation.Get(ids[21]).Footprint,Is.EqualTo(FootprintClass.Huge));
        int exchangeTick=first.World.Tick.Next().Value;
        EnqueueMove(first.World,exchangeTick,3,eastExchange,FixVec2.FromInts(65,100));
        EnqueueMove(first.World,exchangeTick,4,westExchange,FixVec2.FromInts(95,100));
        EnqueueMove(repeat.World,exchangeTick,3,new[]{repeatIds[22],repeatIds[23]},FixVec2.FromInts(65,100));
        EnqueueMove(repeat.World,exchangeTick,4,new[]{repeatIds[20],repeatIds[21]},FixVec2.FromInts(95,100));
        RunUntilComplete(first,ids,PhaseDeadlineTicks,true);RunUntilComplete(repeat,repeatIds,PhaseDeadlineTicks,false);

        EntityId[] excavationMovers=eastExchange;EntityId[] repeatExcavationMovers={repeatIds[22],repeatIds[23]};
        int stagingTick=first.World.Tick.Next().Value;
        EnqueueMove(first.World,stagingTick,5,excavationMovers,FixVec2.FromInts(65,135));
        EnqueueMove(repeat.World,stagingTick,5,repeatExcavationMovers,FixVec2.FromInts(65,135));
        RunUntilComplete(first,ids,PhaseDeadlineTicks,true);RunUntilComplete(repeat,repeatIds,PhaseDeadlineTicks,false);

        int topologyBefore=first.World.Map.TopologyVersion;int openTick=first.World.Tick.Next().Value;
        EnqueueExcavationOpen(first.World,openTick,6);EnqueueExcavationOpen(repeat.World,openTick,6);
        EnqueueMove(first.World,openTick+1,7,excavationMovers,FixVec2.FromInts(95,135));
        EnqueueMove(repeat.World,openTick+1,7,repeatExcavationMovers,FixVec2.FromInts(95,135));
        first.StepTicks(2);repeat.StepTicks(2);
        Assert.That(first.World.Map.TopologyVersion,Is.EqualTo(topologyBefore+1));
        for(int i=0;i<excavationMovers.Length;i++)Assert.That(first.World.GetCorridor(excavationMovers[i])?.TopologyVersion,Is.EqualTo(first.World.Map.TopologyVersion));
        AssertCorridorCrossesGap(first.World,excavationMovers[0],264,277,"opened Excavatable route");
        RunUntilComplete(first,ids,PhaseDeadlineTicks,true);RunUntilComplete(repeat,repeatIds,PhaseDeadlineTicks,false);
        Assert.That(StateHasher.Hash(repeat.World),Is.EqualTo(StateHasher.Hash(first.World)));
        Assert.That(repeat.World.DeadlockDiagnostics,Is.EqualTo(first.World.DeadlockDiagnostics),"Recovery diagnostics diverged between deterministic runs.");
    }

    private static SimulationRunner CreateRepresentative()
    {
        SimulationWorld world=ScenarioFactory.CreateRepresentative24();EntityId[] ids=ScenarioFactory.OwnedIds(world,0);
        EnqueueMove(world,1,1,Slice(ids,0,10),FixVec2.FromInts(118,68));
        EnqueueMove(world,1,2,Slice(ids,10,10),FixVec2.FromInts(42,107));
        return new SimulationRunner(world);
    }

    private static void EnqueueMove(SimulationWorld world,int tick,uint sequence,EntityId[] ids,FixVec2 destination)
        =>world.Commands.Enqueue(new CommandEnvelope(new SimTick(tick),0,sequence,SimCommandType.Move,ids,destination));

    private static void EnqueueExcavationOpen(SimulationWorld world,int tick,uint sequence)
        =>world.Commands.Enqueue(new CommandEnvelope(new SimTick(tick),0,sequence,SimCommandType.DebugOpenExcavatable,System.Array.Empty<EntityId>(),FixVec2.Zero,debugFeatureId:DevMapFactory.ExcavatableFeatureId));

    private static void RunUntilComplete(SimulationRunner runner,EntityId[] ids,int deadlineTicks,bool assertLegality)
    {
        int start=runner.World.Tick.Value;
        do
        {
            runner.StepOneTick();if(assertLegality)AssertNoIllegalFriendlyOverlap(runner.World,ids);
        }
        while(runner.World.Tick.Value-start<deadlineTicks&&!AllComplete(runner.World,ids));
        Assert.That(AllComplete(runner.World,ids),Is.True,
            $"Representative movers missed the generous deadline at tick {runner.World.Tick.Value}. {DescribeIncomplete(runner.World,ids)}");
    }

    private static bool AllComplete(SimulationWorld world,EntityId[] ids)
    {
        for(int i=0;i<ids.Length;i++)if(world.Entities.Navigation.Get(ids[i]).HasTarget)return false;
        return true;
    }

    private static string DescribeIncomplete(SimulationWorld world,EntityId[] ids)
    {
        System.Text.StringBuilder result=new();
        for(int i=0;i<ids.Length;i++)
        {
            EntityId id=ids[i];NavigationAgent nav=world.Entities.Navigation.Get(id);
            if(!nav.HasTarget)continue;
            SimTransform transform=world.Entities.Transform.Get(id);Movement movement=world.Entities.Movement.Get(id);
            RouteCorridor? corridor=world.GetCorridor(id);
            EntityId nearest=EntityId.None;Fix32 nearestDistance=Fix32.MaxValue;
            for(int otherIndex=0;otherIndex<ids.Length;otherIndex++)
            {
                EntityId other=ids[otherIndex];if(other==id)continue;
                Fix32 distance=FixVec2.Distance(transform.Position,world.Entities.Transform.Get(other).Position);
                if(distance<nearestDistance){nearest=other;nearestDistance=distance;}
            }
            bool nearestMoving=nearest!=EntityId.None&&world.Entities.Navigation.Get(nearest).HasTarget;
            result.Append($"{id}:foot={nav.Footprint},pos={transform.Position},target={nav.Target},stuck={movement.StuckTicks},compression={movement.CompressionTicks},cohort={nav.Formation.CohortId},slot={nav.Formation.SlotIndex}/{nav.Formation.MemberCount},cols={nav.Formation.Columns},path={movement.PathIndex}/{corridor?.Cells.Count ?? 0},valid={corridor?.IsValid},nearest={nearest}@{nearestDistance}/moving={nearestMoving}; ");
        }
        return result.ToString();
    }

    private static void AssertNoIllegalFriendlyOverlap(SimulationWorld world,EntityId[] ids)
    {
        for(int i=0;i<ids.Length;i++)for(int j=i+1;j<ids.Length;j++)
        {
            Fix32 minimum=CollisionRadiusSum(world,ids[i],ids[j])*Fix32.FromRatio(85,100);
            Fix32 distance=FixVec2.Distance(world.Entities.Transform.Get(ids[i]).Position,world.Entities.Transform.Get(ids[j]).Position);
            Assert.That(distance.Raw,Is.GreaterThanOrEqualTo(minimum.Raw-CollisionTolerance.Raw),$"Illegal friendly overlap at tick {world.Tick.Value}: {ids[i]}/{ids[j]}.");
        }
    }

    private static void AssertLegalStarts(SimulationWorld world,EntityId[] ids)
    {
        for(int i=0;i<ids.Length;i++)
        {
            NavigationAgent nav=world.Entities.Navigation.Get(ids[i]);SimTransform transform=world.Entities.Transform.Get(ids[i]);
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(transform.Position),nav.Footprint),Is.True,$"Fixture starts {ids[i]} on impassable terrain.");
        }
        for(int i=0;i<ids.Length;i++)for(int j=i+1;j<ids.Length;j++)
        {
            Fix32 minimum=CollisionRadiusSum(world,ids[i],ids[j]);Fix32 distance=FixVec2.Distance(world.Entities.Transform.Get(ids[i]).Position,world.Entities.Transform.Get(ids[j]).Position);
            Assert.That(distance.Raw,Is.GreaterThanOrEqualTo(minimum.Raw),$"Fixture starts overlap: {ids[i]}/{ids[j]}.");
        }
    }

    private static void AssertAllFootprintsRepresented(SimulationWorld world,EntityId[] ids)
    {
        bool[] seen=new bool[5];for(int i=0;i<ids.Length;i++)seen[(int)world.Entities.Navigation.Get(ids[i]).Footprint]=true;
        Assert.That(seen,Is.All.True);
    }

    private static void AssertCorridorCrossesGap(SimulationWorld world,EntityId id,int minimumNavY,int maximumNavY,string label)
    {
        RouteCorridor? corridor=world.GetCorridor(id);Assert.That(corridor?.IsValid,Is.True,$"No valid route through the {label}.");
        bool crossed=false;
        for(int i=0;i<corridor!.Cells.Count;i++)
        {
            NavCell cell=corridor.Cells[i];
            if(cell.X>=152&&cell.X<=167&&cell.Y>=minimumNavY&&cell.Y<=maximumNavY){crossed=true;break;}
            if(i==0)continue;
            NavCell previous=corridor.Cells[i-1];int minX=System.Math.Min(previous.X,cell.X),maxX=System.Math.Max(previous.X,cell.X);
            if(minX>160||maxX<160)continue;
            int crossingY;
            if(previous.X==cell.X)crossingY=(previous.Y+cell.Y)/2;
            else crossingY=previous.Y+(cell.Y-previous.Y)*(160-previous.X)/(cell.X-previous.X);
            if(crossingY>=minimumNavY&&crossingY<=maximumNavY){crossed=true;break;}
        }
        Assert.That(crossed,Is.True,$"Route did not cross the authored {label}.");
    }

    private static void AssertTravelDeadlineIsFeasible(SimulationWorld world,EntityId[] ids)
    {
        Fix32 longest=Fix32.Zero;Fix32 slowest=Fix32.MaxValue;
        for(int i=0;i<ids.Length;i++)
        {
            if(i>=20)continue;
            FixVec2 destination=i<10?FixVec2.FromInts(118,68):FixVec2.FromInts(42,107);
            longest=Fix32.Max(longest,FixVec2.Distance(world.Entities.Transform.Get(ids[i]).Position,destination));
            slowest=Fix32.Min(slowest,world.Entities.Movement.Get(ids[i]).MaxSpeed);
        }
        int physicalMinimumTicks=(longest/slowest*Fix32.FromInt(SimClock.TicksPerSecond)).CeilToInt();
        Assert.That(PhaseDeadlineTicks,Is.GreaterThanOrEqualTo(physicalMinimumTicks*2),"Acceptance deadline is not generously above physical travel time.");
    }

    private static EntityId[] Slice(EntityId[] source,int start,int count)
    {
        EntityId[] result=new EntityId[count];System.Array.Copy(source,start,result,0,count);return result;
    }

    private static Fix32 CollisionRadiusSum(SimulationWorld world,EntityId a,EntityId b)
        =>FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(a).Footprint)+FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(b).Footprint);
}
