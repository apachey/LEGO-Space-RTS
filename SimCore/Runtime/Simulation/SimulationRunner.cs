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
    public readonly long EnergyTimestampTicks;
    public readonly long OperationsCapacityTimestampTicks;
    public readonly long SpatialTimestampTicks;
    public readonly long VisionTimestampTicks;
    public readonly long TargetingTimestampTicks;
    public readonly long WeaponTimestampTicks;

    public TickProfile(long totalTimestampTicks,long commandTimestampTicks,long navigationTimestampTicks,
        long movementIntentTimestampTicks,long localSeparationTimestampTicks,long transformTimestampTicks,long bankingTimestampTicks,long harvestTimestampTicks,long constructionTimestampTicks,long productionTimestampTicks,long energyTimestampTicks,long operationsCapacityTimestampTicks,long spatialTimestampTicks,long visionTimestampTicks,long targetingTimestampTicks,long weaponTimestampTicks)
    {
        TotalTimestampTicks=totalTimestampTicks;CommandTimestampTicks=commandTimestampTicks;NavigationTimestampTicks=navigationTimestampTicks;
        MovementIntentTimestampTicks=movementIntentTimestampTicks;LocalSeparationTimestampTicks=localSeparationTimestampTicks;
        TransformTimestampTicks=transformTimestampTicks;BankingTimestampTicks=bankingTimestampTicks;HarvestTimestampTicks=harvestTimestampTicks;ConstructionTimestampTicks=constructionTimestampTicks;ProductionTimestampTicks=productionTimestampTicks;EnergyTimestampTicks=energyTimestampTicks;OperationsCapacityTimestampTicks=operationsCapacityTimestampTicks;SpatialTimestampTicks=spatialTimestampTicks;VisionTimestampTicks=visionTimestampTicks;TargetingTimestampTicks=targetingTimestampTicks;WeaponTimestampTicks=weaponTimestampTicks;
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
            new EnergyDomainSystem(),
            new OperationsCapacitySystem(),
            new SpatialIndexSystem(),
            new VisionSystem(),
            new TargetingSystem(),
            new WeaponSystem()
        };
        EnergyDomainSystem.RecalculateAll(World);
        OperationsCapacitySystem.Recalculate(World);
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
        long c=0,n=0,m=0,l=0,t=0,b=0,h=0,j=0,p=0,e=0,o=0,s=0,v=0,g=0,w=0;
        World.Tick=World.Tick.Next();
        for(int i=0;i<_systems.Length;i++)
        {
            long start=Stopwatch.GetTimestamp();_systems[i].Step(World);long elapsed=Stopwatch.GetTimestamp()-start;
            switch(i){case 0:c=elapsed;break;case 1:n=elapsed;break;case 2:m=elapsed;break;case 3:l=elapsed;break;case 4:t=elapsed;break;case 5:b=elapsed;break;case 6:h=elapsed;break;case 7:j=elapsed;break;case 8:p=elapsed;break;case 9:e=elapsed;break;case 10:o=elapsed;break;case 11:s=elapsed;break;case 12:v=elapsed;break;case 13:g=elapsed;break;case 14:w=elapsed;break;}
        }
        return new TickProfile(Stopwatch.GetTimestamp()-totalStart,c,n,m,l,t,b,h,j,p,e,o,s,v,g,w);
    }

    public void StepTicks(int count) { for (int i = 0; i < count; i++) StepOneTick(); }
}
}
