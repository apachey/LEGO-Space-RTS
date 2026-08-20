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
    public readonly long TargetingTimestampTicks;
    public readonly long WeaponTimestampTicks;
    public readonly long ProjectileTimestampTicks;
    public readonly long DamageTimestampTicks;

    public TickProfile(long totalTimestampTicks,long commandTimestampTicks,long navigationTimestampTicks,
        long movementIntentTimestampTicks,long localSeparationTimestampTicks,long transformTimestampTicks,long bankingTimestampTicks,long harvestTimestampTicks,long constructionTimestampTicks,long productionTimestampTicks,long resonanceCoreTimestampTicks,long energyTimestampTicks,long alienChargeTimestampTicks,long operationsCapacityTimestampTicks,long spatialTimestampTicks,long forwardServiceTimestampTicks,long missionRefitTimestampTicks,long visionTimestampTicks,long targetingTimestampTicks,long weaponTimestampTicks,long projectileTimestampTicks,long damageTimestampTicks)
    {
        TotalTimestampTicks=totalTimestampTicks;CommandTimestampTicks=commandTimestampTicks;NavigationTimestampTicks=navigationTimestampTicks;
        MovementIntentTimestampTicks=movementIntentTimestampTicks;LocalSeparationTimestampTicks=localSeparationTimestampTicks;
        TransformTimestampTicks=transformTimestampTicks;BankingTimestampTicks=bankingTimestampTicks;HarvestTimestampTicks=harvestTimestampTicks;ConstructionTimestampTicks=constructionTimestampTicks;ProductionTimestampTicks=productionTimestampTicks;ResonanceCoreTimestampTicks=resonanceCoreTimestampTicks;EnergyTimestampTicks=energyTimestampTicks;AlienChargeTimestampTicks=alienChargeTimestampTicks;OperationsCapacityTimestampTicks=operationsCapacityTimestampTicks;SpatialTimestampTicks=spatialTimestampTicks;ForwardServiceTimestampTicks=forwardServiceTimestampTicks;MissionRefitTimestampTicks=missionRefitTimestampTicks;VisionTimestampTicks=visionTimestampTicks;TargetingTimestampTicks=targetingTimestampTicks;WeaponTimestampTicks=weaponTimestampTicks;ProjectileTimestampTicks=projectileTimestampTicks;DamageTimestampTicks=damageTimestampTicks;
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
            new TransformationSystem(),
            new TransportSystem(),
            new RepairSystem(),
            new ContactApproachSystem(),
            new FormationArrivalSystem(),
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
            new VisionSystem(),
            new TargetingSystem(),
            new ContactFacingSystem(),
            new WeaponSystem(),
            new ContactDamageSystem(),
            new ProjectileSystem(),
            new DamageSystem(),
            new DestructionSystem()
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
        long c=0,n=0,m=0,l=0,t=0,b=0,h=0,j=0,p=0,q=0,e=0,a=0,o=0,s=0,f=0,r=0,v=0,g=0,w=0,x=0,d=0;
        World.Tick=World.Tick.Next();
        for(int i=0;i<_systems.Length;i++)
        {
            long start=Stopwatch.GetTimestamp();_systems[i].Step(World);long elapsed=Stopwatch.GetTimestamp()-start;
            ISimSystem system=_systems[i];
            if(system is CommandExecutionSystem)c+=elapsed;
            else if(system is ContactApproachSystem||system is FormationArrivalSystem||system is NavigationRequestSystem)n+=elapsed;
            else if(system is MovementIntentSystem)m+=elapsed;
            else if(system is LocalSeparationSystem)l+=elapsed;
            else if(system is TransformationSystem||system is TransportSystem||system is RepairSystem||system is TransformMovementSystem||system is TubeTransferSystem)t+=elapsed;
            else if(system is ResourceBankingSystem)b+=elapsed;
            else if(system is HarvestSystem)h+=elapsed;
            else if(system is ConstructionSystem)j+=elapsed;
            else if(system is ProductionSystem)p+=elapsed;
            else if(system is ResonanceCoreSystem)q+=elapsed;
            else if(system is EnergyDomainSystem)e+=elapsed;
            else if(system is AlienChargeSystem)a+=elapsed;
            else if(system is OperationsCapacitySystem)o+=elapsed;
            else if(system is SpatialIndexSystem)s+=elapsed;
            else if(system is ForwardServiceSystem)f+=elapsed;
            else if(system is MissionRefitSystem)r+=elapsed;
            else if(system is VisionSystem)v+=elapsed;
            else if(system is TargetingSystem||system is ContactFacingSystem)g+=elapsed;
            else if(system is WeaponSystem)w+=elapsed;
            else if(system is ProjectileSystem)x+=elapsed;
            else if(system is ContactDamageSystem||system is DamageSystem||system is DestructionSystem)d+=elapsed;
        }
        return new TickProfile(Stopwatch.GetTimestamp()-totalStart,c,n,m,l,t,b,h,j,p,q,e,a,o,s,f,r,v,g,w,x,d);
    }

    public void StepTicks(int count) { for (int i = 0; i < count; i++) StepOneTick(); }
}
}
