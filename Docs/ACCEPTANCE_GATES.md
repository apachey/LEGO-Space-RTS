# M0–M2 Acceptance Gates — Godot amendment

Phase 10 v0.4 is complete only when all executable gates below pass on the pinned toolchain.

## M0 — engine and architecture spike

- Godot **4.7.1-stable .NET** opens `GodotClient/project.godot` without project-conversion warnings.
- `dotnet build LEGO.SpaceRTS.Phase10.sln` succeeds with warnings-as-errors.
- SimCore references neither Godot nor Unity assemblies.
- authoritative SimCore runtime contains no `float`/`double` gameplay state/calculation.
- native `Bootstrap` → `PrototypeRTS` scene startup succeeds.
- Godot headless `--smoke` succeeds.
- rendered placeholder entities come from read-only presentation snapshots.
- no Godot physics/navigation callback determines authoritative gameplay.

## M1 — deterministic headless sim

- all pure NUnit tests pass;
- snapshot round trip passes;
- restore → continue produces identical final hash;
- replay → expected final hash passes;
- 100 repeated golden runs produce identical checkpoint/final hashes;
- `HeadlessSim` works without Godot;
- benchmark reports ≥20× realtime for the designated M1 benchmark;
- simulation/pathfinding performance thresholds are met or any failure is treated as a Phase 10 defect.

## M2 — first controllable RTS map

The developer can:

1. launch the Godot scene;
2. see `DEV_FirstControllableRTS` at canonical scale;
3. pan, zoom and rotate the strategic camera;
4. see multiple placeholder simulation entities;
5. click and drag-select them with visible selection rings/marquee;
6. intentionally use the mixed-army worker filter and Ctrl+drag override;
7. issue Move;
8. issue Shift-queued Move and see numbered ordered destination markers;
9. Stop selected units;
10. Hold selected units and observe the Holding state distinction in diagnostics;
11. assign/recall control groups and see group membership on units;
12. navigate around authored obstacles using smoothed legal waypoint paths;
13. move mixed footprint sizes through test lanes;
14. observe friendly local avoidance without persistent unit interpenetration;
15. observe heavy reservation priority without lower-priority units freezing indefinitely;
16. press F9 to open the authored closed Excavatable connection and observe localized topology update;
17. see CPU-authored fog/explored state update;
18. run the same simulation through HeadlessSim;
19. save deterministic state;
20. reload deterministic state;
21. replay commands and obtain the same final hash.

## Navigation stress

For the 60-mover designated stress scenario:

- no reachable 5-second persistent deadlock;
- completion ≥98%;
- command-to-motion latency within the prototype gate implemented by HeadlessSim;
- oscillation diagnostics stay within the implemented threshold;
- five clearance families are exercised, with Huge marked engineering-only rather than a new gameplay unit.

## Data gate

- regenerate `GodotClient/Compiled` with `ContentCompiler`;
- generated data round-trips through SimCore codecs;
- Godot logs `compiled runtime data`, not fallback, for release validation;
- changing source JSON without regenerating binaries must be caught by the build/review process.

M3 must not begin with an uninvestigated M0/M1/M2 gate failure.
