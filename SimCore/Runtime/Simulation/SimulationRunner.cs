using System.Diagnostics;

namespace LegoSpaceRTS.SimCore
{
public readonly struct TickProfile
{
    public readonly long TotalTimestampTicks;
    public readonly long CommandTimestampTicks;
    public readonly long NavigationTimestampTicks;
    public readonly long ReservationTimestampTicks;
    public readonly long MovementIntentTimestampTicks;
    public readonly long AvoidanceTimestampTicks;
    public readonly long TransformTimestampTicks;
    public readonly long SpatialTimestampTicks;
    public readonly long VisionTimestampTicks;

    public TickProfile(long totalTimestampTicks,long commandTimestampTicks,long navigationTimestampTicks,long reservationTimestampTicks,
        long movementIntentTimestampTicks,long avoidanceTimestampTicks,long transformTimestampTicks,long spatialTimestampTicks,long visionTimestampTicks)
    {
        TotalTimestampTicks=totalTimestampTicks;CommandTimestampTicks=commandTimestampTicks;NavigationTimestampTicks=navigationTimestampTicks;
        ReservationTimestampTicks=reservationTimestampTicks;MovementIntentTimestampTicks=movementIntentTimestampTicks;AvoidanceTimestampTicks=avoidanceTimestampTicks;
        TransformTimestampTicks=transformTimestampTicks;SpatialTimestampTicks=spatialTimestampTicks;VisionTimestampTicks=visionTimestampTicks;
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
            new ReservationPlanningSystem(),
            new MovementIntentSystem(),
            new LocalAvoidanceSystem(),
            new TransformMovementSystem(),
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
        long c=0,n=0,r=0,m=0,a=0,t=0,s=0,v=0;
        World.Tick=World.Tick.Next();
        for(int i=0;i<_systems.Length;i++)
        {
            long start=Stopwatch.GetTimestamp();_systems[i].Step(World);long elapsed=Stopwatch.GetTimestamp()-start;
            switch(i){case 0:c=elapsed;break;case 1:n=elapsed;break;case 2:r=elapsed;break;case 3:m=elapsed;break;case 4:a=elapsed;break;case 5:t=elapsed;break;case 6:s=elapsed;break;case 7:v=elapsed;break;}
        }
        return new TickProfile(Stopwatch.GetTimestamp()-totalStart,c,n,r,m,a,t,s,v);
    }

    public void StepTicks(int count) { for (int i = 0; i < count; i++) StepOneTick(); }
}
}
