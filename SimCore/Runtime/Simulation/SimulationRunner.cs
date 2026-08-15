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
    public readonly long ResonanceCoreTimestampTicks;
    public readonly long EnergyTimestampTicks;
    public readonly long AlienChargeTimestampTicks;
    public readonly long OperationsCapacityTimestampTicks;
    public readonly long SpatialTimestampTicks;
    public readonly long ForwardServiceTimestampTicks;
    public readonly long MissionRefitTimestampTicks;
    public readonly long VisionTimestampTicks;

    public TickProfile(long totalTimestampTicks,long commandTimestampTicks,long navigationTimestampTicks,
        long movementIntentTimestampTicks,long localSeparationTimestampTicks,long transformTimestampTicks,long bankingTimestampTicks,long harvestTimestampTicks,long constructionTimestampTicks,long productionTimestampTicks,long resonanceCoreTimestampTicks,long energyTimestampTicks,long alienChargeTimestampTicks,long operationsCapacityTimestampTicks,long spatialTimestampTicks,long forwardServiceTimestampTicks,long missionRefitTimestampTicks,long visionTimestampTicks)
    {
        TotalTimestampTicks=totalTimestampTicks;CommandTimestampTicks=commandTimestampTicks;NavigationTimestampTicks=navigationTimestampTicks;
        MovementIntentTimestampTicks=movementIntentTimestampTicks;LocalSeparationTimestampTicks=localSeparationTimestampTicks;
        TransformTimestampTicks=transformTimestampTicks;BankingTimestampTicks=bankingTimestampTicks;HarvestTimestampTicks=harvestTimestampTicks;ConstructionTimestampTicks=constructionTimestampTicks;ProductionTimestampTicks=productionTimestampTicks;ResonanceCoreTimestampTicks=resonanceCoreTimestampTicks;EnergyTimestampTicks=energyTimestampTicks;AlienChargeTimestampTicks=alienChargeTimestampTicks;OperationsCapacityTimestampTicks=operationsCapacityTimestampTicks;SpatialTimestampTicks=spatialTimestampTicks;ForwardServiceTimestampTicks=forwardServiceTimestampTicks;MissionRefitTimestampTicks=missionRefitTimestampTicks;VisionTimestampTicks=visionTimestampTicks;
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
        ForwardServiceSystem forwardService = new();
        _systems = new ISimSystem[]
        {
            new CommandExecutionSystem(),
            new NavigationRequestSystem(),
            new MovementIntentSystem(),
            new LocalSeparationSystem(),
            new TransformMovementSystem(),
            new TubeTransferSystem(),
            new ResourceBankingSystem(),
            new HarvestSystem(),
            new ConstructionSystem(),
            new ProductionSystem(),
            new ResonanceCoreSystem(),
            new EnergyDomainSystem(),
            new AlienChargeSystem(),
            new OperationsCapacitySystem(),
            new SpatialIndexSystem(),
            forwardService,
            new MissionRefitSystem(),
            new VisionSystem()
        };
        EnergyDomainSystem.RecalculateAll(World);
        AlienChargeSystem.RecalculateAll(World);
        OperationsCapacitySystem.Recalculate(World);
        World.Spatial.Rebuild(World.Entities);
        forwardService.Step(World);
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
        long c=0,n=0,m=0,l=0,t=0,b=0,h=0,j=0,p=0,q=0,e=0,a=0,o=0,s=0,f=0,r=0,v=0;
        World.Tick=World.Tick.Next();
        for(int i=0;i<_systems.Length;i++)
        {
            long start=Stopwatch.GetTimestamp();_systems[i].Step(World);long elapsed=Stopwatch.GetTimestamp()-start;
            switch(i){case 0:c=elapsed;break;case 1:n=elapsed;break;case 2:m=elapsed;break;case 3:l=elapsed;break;case 4:t=elapsed;break;case 5:t+=elapsed;break;case 6:b=elapsed;break;case 7:h=elapsed;break;case 8:j=elapsed;break;case 9:p=elapsed;break;case 10:q=elapsed;break;case 11:e=elapsed;break;case 12:a=elapsed;break;case 13:o=elapsed;break;case 14:s=elapsed;break;case 15:f=elapsed;break;case 16:r=elapsed;break;case 17:v=elapsed;break;}
        }
        return new TickProfile(Stopwatch.GetTimestamp()-totalStart,c,n,m,l,t,b,h,j,p,q,e,a,o,s,f,r,v);
    }

    public void StepTicks(int count) { for (int i = 0; i < count; i++) StepOneTick(); }
}
}
