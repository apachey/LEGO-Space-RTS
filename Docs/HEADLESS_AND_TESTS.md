# Headless simulation and tests

## Toolchain

`HeadlessSim`, `SimCore.Tests` and `ContentCompiler` run on normal .NET 8 tooling and do not require Godot.

This is intentional: deterministic regression must not require a renderer/editor.

## Pure NUnit

```powershell
dotnet test SimCore.Tests/SimCore.Tests.csproj -c Release
```

The suite covers deterministic math, IDs, entity storage, commands, navigation, formations, reservations, movement/fog, snapshot/replay round trips, content format and stress behavior.

`AssemblyBoundaryTests` rejects both Unity and Godot references from SimCore.

## Validate compiled runtime data through the pure runner

```powershell
dotnet run --project HeadlessSim -- --scenario first --compiled-dir GodotClient/Compiled --ticks 1200 --hash-every 200
```

This exercises the same compiled map/content binaries consumed by the Godot host.

## Deterministic repeated run

```powershell
dotnet run --project HeadlessSim -- --scenario golden --ticks 3200 --repeat 100 --hash-every 400
```

All runs must end with the same hash. A trusted checkpoint manifest may be generated only from a known-good executable run and then committed under `Tests/Golden/`.

## Snapshot continuation

```powershell
dotnet run --project HeadlessSim -- --scenario first --ticks 800 --snapshot-out first.snapshot
dotnet run --project HeadlessSim -- --snapshot-in first.snapshot --ticks 800 --snapshot-out continued.snapshot
```

The corresponding automated test validates restore → continue equivalence.

## Replay

```powershell
dotnet run --project HeadlessSim -- --scenario golden --ticks 3200 --record-replay golden.replay
dotnet run --project HeadlessSim -- --replay golden.replay --ticks 3200
```

## Representative 24-mover M2 gate (`BLOCKING_NOW`)

Phase 09B replaces the former blocking M2 torture gate with a representative
24-mover Movement Architecture v2 scenario. The scenario and Movement
Architecture v2 implementation are now present and accepted. The focused gate
is `M2MovementAcceptanceTests`; it remains in the suite as regression coverage.

## M3 resource-node foundation

`M3ResourceNodeTests` validates the canonical finite Ore capacities, 1,800 safe
starting Ore per side, clamped depletion/model states, and snapshot/state-hash
continuation for heterogeneous mover/resource-node entities.

## Performance / 60 movers (`BLOCKING_LATER` — M6 acceptance)

```powershell
dotnet run --project HeadlessSim -- --scenario stress60 --ticks 26000 --benchmark --path-benchmark --enforce-performance-gates
```

The runner reports wall time, ticks/second, realtime multiplier, tick/path p95/p99/max, per-system means, motion-delay diagnostics, mover completion, stuck recovery, deadlock and oscillation counters.

The measurements and thresholds are unchanged. During M6 development,
`./tools/verify.sh --full` records a red result from this stage as
`BLOCKING_LATER`; `./tools/verify.sh --m6-acceptance` promotes it to
`BLOCKING_NOW`. M6 implementation may continue, but M6 networked 1v1 may not be
accepted while this gate is red.

Phase 09 targets retained by the engine amendment:

- sustained headless throughput ≥20× realtime for the M1 benchmark;
- simulation ≤2.5 ms p95 normal / ≤4.0 ms p99 stress;
- pathfinding ≤1.5 ms p95 normal / ≤3.0 ms p99 stress.

## Godot headless host smoke

After pure tests and content compilation:

```powershell
Godot_v4.7.1-stable_mono_win64.exe --headless --path GodotClient -- --smoke
```

This checks that the native Godot C# host loads, constructs the composition root, advances the shared simulation and exposes a valid presentation snapshot/hash. It is a host-integration smoke, not the deterministic benchmark runner.

## M6 T058 ENet transport smoke

```powershell
Godot_v4.7.1-stable_mono_win64.exe --headless --path GodotClient -- --m6-transport-smoke
```

The smoke starts the production T058 dedicated transport host on an ephemeral
loopback UDP port, connects two independent ENet clients, verifies distinct
peer IDs and sends targeted raw packets through all three logical channels. A
pass prints `M6 TRANSPORT SMOKE: PASS serverConnections=2`. The gate is
`BLOCKING_NOW` in the normal verification harness.

The same bootstrap can run as a transport-only dedicated host:

```powershell
Godot_v4.7.1-stable_mono_win64.exe --headless --path GodotClient -- --dedicated-server --network-bind 127.0.0.1 --network-port 24567
```

T058 intentionally does not implement command validation or snapshot
replication.
