using System.Diagnostics;

namespace LegoSpaceRTS.SimCore
{
public readonly struct TickProfile
{
    public readonly long TotalTimestampTicks;
    public readonly long CommandTimestampTicks;
    public readonly long NavigationTimestampTicks;
    public readonly long MovementIntentTimestampTicks;
    public readonly long LocalSeparationTimestampTicks;
    public readonly long TransformTimestampTicks;
    public readonly long BankingTimestampTicks;
    public readonly long HarvestTimestampTicks;
    public readonly long ConstructionTimestampTicks;
    public readonly long ProductionTimestampTicks;
    public readonly long SpatialTimestampTicks;
    public readonly long VisionTimestampTicks;

    public TickProfile(long totalTimestampTicks,long commandTimestampTicks,long navigationTimestampTicks,
        long movementIntentTimestampTicks,long localSeparationTimestampTicks,long transformTimestampTicks,long bankingTimestampTicks,long harvestTimestampTicks,long constructionTimestampTicks,long productionTimestampTicks,long spatialTimestampTicks,long visionTimestampTicks)
    {
        TotalTimestampTicks=totalTimestampTicks;CommandTimestampTicks=commandTimestampTicks;NavigationTimestampTicks=navigationTimestampTicks;
        MovementIntentTimestampTicks=movementIntentTimestampTicks;LocalSeparationTimestampTicks=localSeparationTimestampTicks;
        TransformTimestampTicks=transformTimestampTicks;BankingTimestampTicks=bankingTimestampTicks;HarvestTimestampTicks=harvestTimestampTicks;ConstructionTimestampTicks=constructionTimestampTicks;ProductionTimestampTicks=productionTimestampTicks;SpatialTimestampTicks=spatialTimestampTicks;VisionTimestampTicks=visionTimestampTicks;
    }

    public long PathfindingTimestampTicks => NavigationTimestampTicks;
}

public sealed class SimulationRunner
{
    private readonly ISimSystem[] _systems;
    public SimulationWorld World { get; }

    public SimulationRunner(SimulationWorld world)
    {
        World = world;
        _systems = new ISimSystem[]
        {
            new CommandExecutionSystem(),
            new NavigationRequestSystem(),
            new MovementIntentSystem(),
            new LocalSeparationSystem(),
            new TransformMovementSystem(),
            new ResourceBankingSystem(),
            new HarvestSystem(),
            new ConstructionSystem(),
            new ProductionSystem(),
            new SpatialIndexSystem(),
            new VisionSystem()
        };
        World.Spatial.Rebuild(World.Entities);
    }

    public void StepOneTick()
    {
        World.Tick = World.Tick.Next();
        for (int i = 0; i < _systems.Length; i++) _systems[i].Step(World);
    }

    /// <summary>Development-only timing path. Stopwatch values never influence authoritative state.</summary>
    public TickProfile StepOneTickProfiled()
    {
        long totalStart=Stopwatch.GetTimestamp();
        long c=0,n=0,m=0,l=0,t=0,b=0,h=0,j=0,p=0,s=0,v=0;
        World.Tick=World.Tick.Next();
        for(int i=0;i<_systems.Length;i++)
        {
            long start=Stopwatch.GetTimestamp();_systems[i].Step(World);long elapsed=Stopwatch.GetTimestamp()-start;
            switch(i){case 0:c=elapsed;break;case 1:n=elapsed;break;case 2:m=elapsed;break;case 3:l=elapsed;break;case 4:t=elapsed;break;case 5:b=elapsed;break;case 6:h=elapsed;break;case 7:j=elapsed;break;case 8:p=elapsed;break;case 9:s=elapsed;break;case 10:v=elapsed;break;}
        }
        return new TickProfile(Stopwatch.GetTimestamp()-totalStart,c,n,m,l,t,b,h,j,p,s,v);
    }

    public void StepTicks(int count) { for (int i = 0; i < count; i++) StepOneTick(); }
}
}
