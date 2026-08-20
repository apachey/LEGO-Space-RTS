using System.Diagnostics;
using LegoSpaceRTS.SimCore;

static string? Arg(string[] args, string name)
{
    for (int i = 0; i < args.Length; i++) if (args[i] == name && i + 1 < args.Length) return args[i + 1];
    return null;
}
static bool Has(string[] args, string name) => Array.IndexOf(args, name) >= 0;
static double TimestampTicksToMs(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;
static long Percentile(List<long> values, double percentile)
{
    if (values.Count == 0) return 0;
    values.Sort();
    int index = (int)Math.Ceiling(percentile * values.Count) - 1;
    return values[Math.Clamp(index, 0, values.Count - 1)];
}
static double MeanMs(List<long> values)
{
    if(values.Count==0)return 0;long sum=0;for(int i=0;i<values.Count;i++)sum+=values[i];return TimestampTicksToMs(sum/values.Count);
}
static long MaxTicks(List<long> values)
{
    long max=0;for(int i=0;i<values.Count;i++)if(values[i]>max)max=values[i];return max;
}
static void PrintSystemMean(string name,List<long> samples) => Console.WriteLine($"system={name} meanMs={MeanMs(samples):F4} maxMs={TimestampTicksToMs(MaxTicks(samples)):F4}");

string scenario = Arg(args, "--scenario") ?? "golden";
int ticks = int.TryParse(Arg(args, "--ticks"), out int parsedTicks) ? parsedTicks : 3000;
int hashEvery = int.TryParse(Arg(args, "--hash-every"), out int parsedHash) ? parsedHash : 0;
int repeat = int.TryParse(Arg(args, "--repeat"), out int parsedRepeat) ? parsedRepeat : 1;
string? snapshotIn = Arg(args, "--snapshot-in");
string? snapshotOut = Arg(args, "--snapshot-out");
string? replayIn = Arg(args, "--replay");
string? replayOut = Arg(args, "--record-replay");
string? stateDumpOut = Arg(args, "--dump-state");
string? goldenManifestOut = Arg(args, "--golden-manifest-out");
string? goldenManifestIn = Arg(args, "--golden-manifest-in");
string? compiledDir = Arg(args, "--compiled-dir");
bool benchmark = Has(args, "--benchmark");
bool pathBenchmark = Has(args, "--path-benchmark");
bool enforceGates = Has(args, "--enforce-performance-gates");
if (pathBenchmark) { scenario = "stress60"; benchmark = true; if (Arg(args, "--ticks") is null) ticks = ScenarioFactory.Stress60FinalEvaluationTick; }

PrototypeContentCatalog builtInContent=PrototypeContentFactory.CreateM2Catalog();
PrototypeContentCatalog runtimeContent=builtInContent;
MapDefinition? compiledMap=null;
if(compiledDir is not null)
{
    string contentPath=Path.Combine(compiledDir,"PrototypeEntities.contentbin");
    string mapPath=Path.Combine(compiledDir,"DEV_FirstControllableRTS.mapbin");
    runtimeContent=PrototypeContentCodec.Read(File.ReadAllBytes(contentPath));
    compiledMap=CompiledMapCodec.ReadDefinition(File.ReadAllBytes(mapPath));
}
ulong gameplayContentHash=runtimeContent.ContentHash;
Console.WriteLine($"build=phase10-v0.2-godot gameplayContentHash={gameplayContentHash:X16} simProtocol={SnapshotSerializer.SimulationProtocolVersion} snapshotFormat={SnapshotSerializer.FormatVersion}");

Dictionary<int, ulong>? expectedGolden = null;
if (goldenManifestIn is not null)
{
    expectedGolden = new Dictionary<int, ulong>();
    foreach (string raw in File.ReadAllLines(goldenManifestIn))
    {
        string line=raw.Trim(); if(line.Length==0||line.StartsWith("#",StringComparison.Ordinal))continue;
        string[] parts=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length!=2||!int.TryParse(parts[0],out int tick)||!ulong.TryParse(parts[1],System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out ulong hash))
            throw new InvalidDataException($"Malformed golden manifest line: {raw}");
        expectedGolden.Add(tick,hash);
    }
}
int[] defaultGoldenCheckpoints={250,500,1000,1500,2000,3000};
ulong? reference = null;
string? referenceDump = null;
Dictionary<int, ulong>? referenceCheckpoints = null;
for (int run = 0; run < repeat; run++)
{
    SimulationRunner runner;
    ReplayLog? sourceReplay = null;
    if (replayIn is not null)
    {
        sourceReplay = ReplayLog.Deserialize(File.ReadAllBytes(replayIn));
        runner = sourceReplay.CreateRunner();
    }
    else if (snapshotIn is not null) runner = new SimulationRunner(SnapshotSerializer.Deserialize(File.ReadAllBytes(snapshotIn)));
    else if (scenario == "stress60") runner = new SimulationRunner(ScenarioFactory.CreateStress60());
    else if (scenario == "first") runner = new SimulationRunner(compiledMap is null ? ScenarioFactory.CreateFirstControllable() : ScenarioFactory.CreateFirstControllable(compiledMap, runtimeContent));
    else { sourceReplay = ScenarioFactory.CreateGoldenReplay(); runner = sourceReplay.CreateRunner(); }

    Dictionary<EntityId, FixVec2>? initialPositions = null;
    Dictionary<EntityId, int>? motionDelayTicks = null;
    Dictionary<EntityId, FixVec2>? stressPhaseTargets = null;
    List<(int Tick, int Completed, int Movers)>? stressPhaseResults = null;
    if (pathBenchmark)
    {
        initialPositions = new Dictionary<EntityId, FixVec2>();
        motionDelayTicks = new Dictionary<EntityId, int>();
        stressPhaseTargets = new Dictionary<EntityId, FixVec2>();
        stressPhaseResults = new List<(int Tick, int Completed, int Movers)>(3);
        EntityId[] ids = ScenarioFactory.OwnedIds(runner.World, 0);
        for (int i = 0; i < ids.Length; i++) initialPositions[ids[i]] = runner.World.Entities.Transform.Get(ids[i]).Position;
    }

    List<long>? tickSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? commandSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? navSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? movementSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? separationSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? transformSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? spatialSamples = benchmark ? new List<long>(ticks) : null;
    List<long>? visionSamples = benchmark ? new List<long>(ticks) : null;

    Dictionary<int,ulong>? observedGolden = goldenManifestOut is not null || expectedGolden is not null || repeat > 1 ? new Dictionary<int,ulong>() : null;
    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < ticks; i++)
    {
        if (benchmark)
        {
            TickProfile profile = runner.StepOneTickProfiled();
            tickSamples!.Add(profile.TotalTimestampTicks);commandSamples!.Add(profile.CommandTimestampTicks);navSamples!.Add(profile.NavigationTimestampTicks);
            movementSamples!.Add(profile.MovementIntentTimestampTicks); separationSamples!.Add(profile.LocalSeparationTimestampTicks);
            transformSamples!.Add(profile.TransformTimestampTicks);spatialSamples!.Add(profile.SpatialTimestampTicks);visionSamples!.Add(profile.VisionTimestampTicks);
        }
        else runner.StepOneTick();

        if (stressPhaseTargets is not null &&
            (runner.World.Tick.Value == ScenarioFactory.Stress60FirstMoveTick ||
             runner.World.Tick.Value == ScenarioFactory.Stress60SecondMoveTick ||
             runner.World.Tick.Value == ScenarioFactory.Stress60ThirdMoveTick))
        {
            stressPhaseTargets.Clear();
            EntityId[] commandedIds = ScenarioFactory.OwnedIds(runner.World, 0);
            for (int targetIndex = 0; targetIndex < commandedIds.Length; targetIndex++)
            {
                EntityId commandedId = commandedIds[targetIndex];
                NavigationAgent commandedNavigation = runner.World.Entities.Navigation.Get(commandedId);
                if (commandedNavigation.HasTarget) stressPhaseTargets[commandedId] = commandedNavigation.Target;
            }
        }

        if (motionDelayTicks is not null && initialPositions is not null && runner.World.Tick.Value <= 30)
        {
            foreach (KeyValuePair<EntityId, FixVec2> pair in initialPositions)
            {
                if (motionDelayTicks.ContainsKey(pair.Key)) continue;
                FixVec2 now = runner.World.Entities.Transform.Get(pair.Key).Position;
                if (FixVec2.DistanceSquared(now, pair.Value) > Fix32.FromRatio(1, 100) * Fix32.FromRatio(1, 100)) motionDelayTicks[pair.Key] = runner.World.Tick.Value;
            }
        }
        if (stressPhaseResults is not null &&
            (runner.World.Tick.Value == ScenarioFactory.Stress60SecondMoveTick - 1 ||
             runner.World.Tick.Value == ScenarioFactory.Stress60TopologyOpenTick - 1 ||
             runner.World.Tick.Value == ScenarioFactory.Stress60FinalEvaluationTick))
        {
            int phaseCompleted = 0, phaseMovers = 0;
            EntityId[] phaseIds = ScenarioFactory.OwnedIds(runner.World, 0);
            for (int phaseIndex = 0; phaseIndex < phaseIds.Length; phaseIndex++)
            {
                phaseMovers++;
                EntityId phaseId = phaseIds[phaseIndex];
                SimTransform phaseTransform = runner.World.Entities.Transform.Get(phaseId);
                if (stressPhaseTargets!.TryGetValue(phaseId, out FixVec2 assignedTarget) &&
                    FixVec2.Distance(phaseTransform.Position, assignedTarget) <= Fix32.One) phaseCompleted++;
            }
            stressPhaseResults.Add((runner.World.Tick.Value, phaseCompleted, phaseMovers));
        }
        bool checkpoint=false;
        for(int g=0;g<defaultGoldenCheckpoints.Length;g++)if(runner.World.Tick.Value==defaultGoldenCheckpoints[g]){checkpoint=true;break;}
        if(checkpoint && observedGolden is not null)
        {
            ulong checkpointHash=StateHasher.Hash(runner.World);observedGolden[runner.World.Tick.Value]=checkpointHash;
            if(expectedGolden is not null && expectedGolden.TryGetValue(runner.World.Tick.Value,out ulong expectedCheckpoint) && expectedCheckpoint!=checkpointHash)
            {
                Console.Error.WriteLine($"GOLDEN CHECKPOINT FAILURE tick={runner.World.Tick.Value} expected={expectedCheckpoint:X16} actual={checkpointHash:X16}");
                Environment.ExitCode=7;
            }
        }
        if (hashEvery > 0 && runner.World.Tick.Value % hashEvery == 0)
            Console.WriteLine($"tick={runner.World.Tick.Value} hash={StateHasher.HashHex(runner.World)}");
    }
    sw.Stop();

    ulong finalHash = StateHasher.Hash(runner.World);
    string currentDump=AuthoritativeStateDumper.Dump(runner.World);
    double tps = ticks / Math.Max(sw.Elapsed.TotalSeconds, 0.000001);
    double realtime = tps / SimClock.TicksPerSecond;
    Console.WriteLine($"run={run + 1} map={runner.World.Map.StableKey} entityCount={runner.World.Entities.Alive.Count} ticks={ticks} finalHash={finalHash:X16} gameplayContentHash={gameplayContentHash:X16}");
    if (benchmark)
    {
        double tickMean=MeanMs(tickSamples!);double tickP95 = TimestampTicksToMs(Percentile(tickSamples!, 0.95));double tickP99 = TimestampTicksToMs(Percentile(tickSamples!, 0.99));double tickMax=TimestampTicksToMs(MaxTicks(tickSamples!));
        double pathMean=MeanMs(navSamples!);double pathP95 = TimestampTicksToMs(Percentile(navSamples!, 0.95));double pathP99 = TimestampTicksToMs(Percentile(navSamples!, 0.99));double pathMax=TimestampTicksToMs(MaxTicks(navSamples!));
        Console.WriteLine($"elapsedMs={sw.Elapsed.TotalMilliseconds:F3} ticksPerSecond={tps:F2} realtimeMultiplier={realtime:F2}x");
        Console.WriteLine($"tickMeanMs={tickMean:F4} tickP95Ms={tickP95:F4} tickP99Ms={tickP99:F4} tickMaxMs={tickMax:F4}");
        Console.WriteLine($"pathMeanMs={pathMean:F4} pathP95Ms={pathP95:F4} pathP99Ms={pathP99:F4} pathMaxMs={pathMax:F4} pathRequests={runner.World.PathRequestsProcessed}");
        PrintSystemMean("command",commandSamples!);PrintSystemMean("movementIntent",movementSamples!);PrintSystemMean("localSeparation",separationSamples!);PrintSystemMean("transform",transformSamples!);PrintSystemMean("spatial",spatialSamples!);PrintSystemMean("fogLoS",visionSamples!);
        if (pathBenchmark)
        {
            for (int phaseIndex = 0; phaseIndex < stressPhaseResults!.Count; phaseIndex++)
            {
                (int phaseTick, int phaseCompleted, int phaseMovers) = stressPhaseResults[phaseIndex];
                double phaseCompletionRate = phaseMovers == 0 ? 1.0 : (double)phaseCompleted / phaseMovers;
                Console.WriteLine($"stressPhaseTick={phaseTick} completion={phaseCompleted}/{phaseMovers} completionRate={phaseCompletionRate:P2}");
                if (enforceGates && phaseCompletionRate < 0.98) Environment.ExitCode = 6;
            }
            if (enforceGates && stressPhaseResults.Count != 3) Environment.ExitCode = 6;
            int completed = 0, movers = 0;
            EntityId[] ids = ScenarioFactory.OwnedIds(runner.World, 0);
            for (int i = 0; i < ids.Length; i++)
            {
                movers++;
                EntityId id = ids[i];
                SimTransform transform = runner.World.Entities.Transform.Get(id);
                if (stressPhaseTargets!.TryGetValue(id, out FixVec2 assignedTarget) &&
                    FixVec2.Distance(transform.Position, assignedTarget) <= Fix32.One) completed++;
            }
            int delayMax = motionDelayTicks!.Count == 0 ? ticks : motionDelayTicks.Values.Max();
            double delayAverage = motionDelayTicks.Count == 0 ? ticks : motionDelayTicks.Values.Average();
            double completionRate = movers == 0 ? 1.0 : (double)completed / movers;
            Console.WriteLine($"movers={movers} completion={completed}/{movers} completionRate={completionRate:P2} commandToMotionAvgTicks={delayAverage:F2} commandToMotionMaxTicks={delayMax}");
            Console.WriteLine($"stuckRecoveryEvents={runner.World.StuckRecoveryDiagnostics} deadlockDiagnostics={runner.World.DeadlockDiagnostics} oscillationIncidents={runner.World.OscillationDiagnostics}");
            if (enforceGates && (completionRate < 0.98 || runner.World.DeadlockDiagnostics != 0 || runner.World.OscillationDiagnostics > 4 || delayMax > 6)) Environment.ExitCode = 6;
        }
        if (enforceGates && (realtime < 20.0 || tickP99 > 4.0 || pathP99 > 3.0)) Environment.ExitCode = 5;
    }

    if (reference is null) { reference = finalHash; referenceDump=currentDump; }
    else if (reference.Value != finalHash)
    {
        Console.Error.WriteLine($"DETERMINISM FAILURE: expected={reference.Value:X16} actual={finalHash:X16}");
        Console.Error.WriteLine(AuthoritativeStateDumper.FirstDifference(referenceDump ?? string.Empty,currentDump));
        Environment.ExitCode = 3; break;
    }

    if (repeat > 1 && observedGolden is not null)
    {
        if (referenceCheckpoints is null) referenceCheckpoints = new Dictionary<int, ulong>(observedGolden);
        else
        {
            bool checkpointMismatch = false;
            foreach (KeyValuePair<int, ulong> pair in referenceCheckpoints)
            {
                if (!observedGolden.TryGetValue(pair.Key, out ulong actual) || actual != pair.Value)
                {
                    Console.Error.WriteLine($"DETERMINISM CHECKPOINT FAILURE: tick={pair.Key} expected={pair.Value:X16} actual={(observedGolden.TryGetValue(pair.Key, out actual) ? actual.ToString("X16") : "MISSING")}");
                    Environment.ExitCode = 3;
                    checkpointMismatch = true;
                    break;
                }
            }
            if (checkpointMismatch) break;
        }
    }

    if (goldenManifestOut is not null && run == repeat - 1 && observedGolden is not null)
    {
        using StreamWriter writer=new StreamWriter(goldenManifestOut,false);
        writer.WriteLine("# LEGO Space RTS Phase 10 deterministic golden checkpoints");
        writer.WriteLine($"# build phase10-v0.2-godot content {gameplayContentHash:X16}");
        foreach(KeyValuePair<int,ulong> pair in observedGolden.OrderBy(x=>x.Key))writer.WriteLine($"{pair.Key} {pair.Value:X16}");
    }
    if (expectedGolden is not null && run == repeat - 1)
    {
        foreach(KeyValuePair<int,ulong> pair in expectedGolden)
            if(pair.Key<=ticks && (observedGolden is null || !observedGolden.ContainsKey(pair.Key))) throw new InvalidDataException($"Golden checkpoint {pair.Key} was not observed.");
    }
    if (snapshotOut is not null && run == repeat - 1) File.WriteAllBytes(snapshotOut, SnapshotSerializer.Serialize(runner.World));
    if (stateDumpOut is not null && run == repeat - 1) File.WriteAllText(stateDumpOut,currentDump);
    if (replayOut is not null && sourceReplay is not null && run == repeat - 1)
    {
        sourceReplay.ExpectedFinalHash = finalHash;
        sourceReplay.FinalTick = runner.World.Tick.Value;
        File.WriteAllBytes(replayOut, sourceReplay.Serialize());
    }
    if (sourceReplay is not null && sourceReplay.ExpectedFinalHash != 0 && sourceReplay.ExpectedFinalHash != finalHash)
    {
        Console.Error.WriteLine($"REPLAY HASH FAILURE: expected={sourceReplay.ExpectedFinalHash:X16} actual={finalHash:X16}"); Environment.ExitCode = 4;
    }
}
