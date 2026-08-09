using System.Diagnostics;
using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class GodotSimBridge : Node
{
    public const int MaxCatchUpTicks = 4;
    private const double TickSeconds = 0.05;
    private SimulationRunner? _runner;
    private double _accumulator;

    public PresentationSnapshot? Previous { get; private set; }
    public PresentationSnapshot? Current { get; private set; }
    public float InterpolationAlpha => (float)Math.Clamp(_accumulator / TickSeconds, 0.0, 1.0);
    public SimulationWorld World => _runner?.World ?? throw new InvalidOperationException("Simulation bridge is not configured.");
    public double LastSimulationMs { get; private set; }
    public double LastPathfindingMs { get; private set; }
    public ulong GameplayContentHash { get; private set; }

    public void Configure(SimulationWorld world, ulong gameplayContentHash)
    {
        _runner = new SimulationRunner(world);
        GameplayContentHash = gameplayContentHash;
        Previous = Current = PresentationSnapshot.Capture(world, 0);
        ProcessPriority = -100;
    }

    public override void _Process(double delta)
    {
        if (_runner is null) return;
        _accumulator += delta;
        int steps = 0;
        while (_accumulator >= TickSeconds && steps < MaxCatchUpTicks)
        {
            Previous = Current;
            TickProfile profile = _runner.StepOneTickProfiled();
            LastSimulationMs = profile.TotalTimestampTicks * 1000.0 / Stopwatch.Frequency;
            LastPathfindingMs = profile.NavigationTimestampTicks * 1000.0 / Stopwatch.Frequency;
            Current = PresentationSnapshot.Capture(_runner.World, 0);
            _accumulator -= TickSeconds;
            steps++;
        }
        // Authoritative time is never discarded. Excess remainder stays queued for later frames.
    }

    public void Enqueue(CommandEnvelope command) => World.Commands.Enqueue(command);
    public string StateHashHex() => StateHasher.HashHex(World);
}
