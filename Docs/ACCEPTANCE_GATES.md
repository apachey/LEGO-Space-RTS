# M0–M2 Acceptance Gates — Godot amendment

Phase 10 v0.4 is complete only when all `BLOCKING_NOW` executable gates below
pass on the pinned toolchain. `DIAGNOSTIC` gates still run and report their full
measurements, but do not solely block the current milestone.

Gate classifications:

- `BLOCKING_NOW` — must pass for the current milestone;
- `BLOCKING_LATER` — preserved and must pass at its named later milestone;
- `DIAGNOSTIC` — runs and reports during the current milestone without solely
  failing that milestone.

## M0 — engine and architecture spike (`BLOCKING_NOW`)

- Godot **4.7.1-stable .NET** opens `GodotClient/project.godot` without project-conversion warnings.
- `dotnet build LEGO.SpaceRTS.Phase10.sln` succeeds with warnings-as-errors.
- SimCore references neither Godot nor Unity assemblies.
- authoritative SimCore runtime contains no `float`/`double` gameplay state/calculation.
- native `Bootstrap` → `PrototypeRTS` scene startup succeeds.
- Godot headless `--smoke` succeeds.
- rendered placeholder entities come from read-only presentation snapshots.
- no Godot physics/navigation callback determines authoritative gameplay.

## M1 — deterministic headless sim (`BLOCKING_NOW`)

- all pure NUnit tests pass;
- snapshot round trip passes;
- restore → continue produces identical final hash;
- replay → expected final hash passes;
- 100 repeated golden runs produce identical checkpoint/final hashes;
- `HeadlessSim` works without Godot;
- benchmark reports ≥20× realtime for the designated M1 benchmark;
- simulation/pathfinding performance thresholds are met or any failure is treated as a Phase 10 defect.

## M2 — first controllable RTS map (`BLOCKING_NOW`)

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
15. observe Heavy right-of-way without lower-priority units freezing indefinitely;
16. press F9 to open the authored closed Excavatable connection and observe localized topology update;
17. see CPU-authored fog/explored state update;
18. run the same simulation through HeadlessSim;
19. save deterministic state;
20. reload deterministic state;
21. replay commands and obtain the same final hash.

## Representative Movement Architecture v2 scenario (`BLOCKING_NOW` — M2)

The M2 movement gate is the Phase 09B representative medium-army scenario:

- 24 mixed ground movers with all five footprint families represented;
- at least two command cohorts;
- a 10–15-build-cell medium passage and a >=12-cell Heavy route;
- opposing-friendly traffic, Heavy-vs-Small/Tiny right-of-way, and normal
  formation reflow through constrained terrain;
- one authored Excavatable topology opening followed by deterministic legal
  route/corridor validation;
- legal non-overlapping starts and an explicitly feasible generous deadline;
- no benchmark-specific special-case logic.

The automated pass criteria are the complete criteria in Phase 09B: all 24
reachable movers complete, no reachable permanent hard-stall or persistent
illegal overlap remains, no enemy phasing occurs, lower-priority traffic is not
permanently starved, topology refresh is deterministic and legal, repeated
hashes match, and normal Phase 09 performance budgets are met.

The scenario/fixture is intentionally **not implemented by the Phase 09B
governance sync**. Implementing Movement Architecture v2 and this executable
fixture belongs to the next separately approved implementation task.

M2 also requires the Phase 09B human movement pass covering responsiveness,
group readability, formation reflow, Heavy movement character, chokepoints,
jitter/dancing, command feedback, and camera/readability.

## Legacy 60-mover stress (`BLOCKING_LATER` — PRE-M6; `DIAGNOSTIC` — M2–M5)

For the 60-mover designated stress scenario:

- no reachable 5-second persistent deadlock;
- completion ≥98%;
- command-to-motion latency within the prototype gate implemented by HeadlessSim;
- oscillation diagnostics stay within the implemented threshold;
- five clearance families are exercised, with Huge marked engineering-only rather than a new gameplay unit.

The benchmark is preserved with the same measurements and thresholds. During
M2–M5, `./tools/verify.sh --full` reports a failure of this stage as diagnostic
rather than failing M2 solely for that result. It becomes blocking again before
M6 movement/network-scale acceptance. A catastrophic regression still requires
investigation, but this single torture benchmark may not silently dictate a
major architecture rewrite.

## Data gate

- regenerate `GodotClient/Compiled` with `ContentCompiler`;
- generated data round-trips through SimCore codecs;
- Godot logs `compiled runtime data`, not fallback, for release validation;
- changing source JSON without regenerating binaries must be caught by the build/review process.

M3 must not begin until all revised Phase 09B M2 `BLOCKING_NOW` automated gates
and the required human M2 playtest pass. The legacy 60-mover diagnostic does not
replace or waive those gates.
