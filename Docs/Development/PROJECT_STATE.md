# LEGO SPACE RTS — CURRENT PROJECT STATE

This file summarizes current repository state. Canon and current Git evidence
remain authoritative when anything here becomes stale.

## Current milestone

**M2 — First Controllable RTS Map / Movement Architecture v2 implementation.**

M3 has not begun and cannot begin until the revised M2 automated and human
acceptance gates pass.

## Engine / architecture

- Godot 4.7.1-stable .NET host;
- C#;
- engine-independent deterministic SimCore;
- fixed 20 Hz authoritative simulation;
- Fix32 and Angle16 authoritative numerics;
- deterministic replay and state hashing;
- project-owned hierarchical ground navigation;
- Godot owns presentation, input and UI, not gameplay truth.

## Current approved movement architecture

Authority:
`Docs/Canon/09B_MOVEMENT_ARCHITECTURE_AND_PROTOTYPE_GATE_AMENDMENT.md`

**HPA / local A* → persistent route corridor → command cohort / formation
intent → deterministic local separation / yield / friendly soft push → bounded
recovery / repath.**

The former universal space-time reservation and passage-coordinator design is
rejected as the normal locomotion baseline. Reservations remain only for the
narrow placement cases allowed by Phase 09B.

## Completed / merged work

- PR #2 correctness salvage is merged: commands execute on their declared tick,
  Stop clears movement and queued orders correctly, Stable IDs normalize case,
  and deterministic movement conflicts have regression coverage.
- PR #3 Phase 09B governance sync and canon-index entry are merged.
- Persistent route-corridor foundation is implemented: corridors persist in
  authoritative SimCore state, constrain bounded local deviation, participate
  in snapshot/state hashing, and selectively invalidate on topology changes.
- Deterministic local separation/yield is implemented for normal locomotion:
  bounded neighbor evaluation uses footprint priority and Entity ID tie-breaks,
  Heavy movers hold priority, friendly compression is limited to 15% for 30
  ticks, and enemy units never use friendly compression.
- The obsolete universal 12-tick normal-locomotion reservation planner and its
  movement-speed gate are removed from the authoritative pipeline.
- Human playtest arrival jitter caused by underspaced Medium/Large/Huge final
  formation slots is corrected: settled slots now retain at least the canonical
  collision-diameter + 0.35-build-cell margin, so local separation does not
  keep fighting an already completed Move.
- The current full verification blocking gate is green, including builds,
  NUnit, compiled-content validation, HeadlessSim, replay/snapshot determinism,
  Godot headless smoke and macOS export smoke.

## Current gates

- Representative 24-mover Movement Architecture v2 scenario:
  `BLOCKING_NOW` for M2; not yet implemented.
- Legacy 60-mover stress: `DIAGNOSTIC` during M2–M5 and `BLOCKING_LATER`
  before M6. The latest full run remains diagnostic-failing at 20% completion
  with elevated oscillation; it does not block the current M2 task.

## Known unresolved work

- formation/cohort reflow integration;
- representative 24-mover gate implementation;
- final M2 playable macOS build and human movement-feel acceptance.
- pre-existing HPA cluster-size discrepancy: Phase 09 specifies 10 build cells
  / 20 navigation nodes, while the imported runtime/static validator currently
  use 10 navigation nodes / 5 build cells; resolve in a separate canon-alignment
  task before changing cluster geometry.

## Explicitly rejected / do not resurrect

- universal 12-tick space-time movement scheduler as normal locomotion;
- passage coordinator;
- convoy scheduler;
- staging scheduler;
- general MAPF, CBS or cooperative A*;
- benchmark-specific movement hacks.

## Next approved development sequence

1. Formation/cohort reflow integration.
2. Representative 24-mover M2 scenario.
3. Automated M2 verification.
4. macOS playable build.
5. Human playtest.
6. Only after M2 acceptance: M3.
