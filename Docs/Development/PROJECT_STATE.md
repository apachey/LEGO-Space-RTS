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
- Persistent per-unit command-cohort/formation intent is implemented and
  included in snapshot/state hashing (snapshot format v2). Cohorts perform at
  most one deterministic reflow per three seconds of non-progress, narrow their
  columns, and release an impractical exact slot when the mover is already
  legally inside the formation's settling envelope.
- Local locomotion resolves choices in Heavy-first footprint/Entity-ID order.
  Lower-priority movers can keep a legal sidestep/turn-around escape while the
  right-of-way mover briefly waits; a final deterministic safety pass prevents
  a compressed pair from moving closer or entering illegal overlap.
- Meaningful-progress recovery is measured toward the active route waypoint
  over accumulated movement rather than reset by arbitrary per-tick motion, so
  arrival micro-movement cannot indefinitely suppress reflow/repath.
- The current full verification acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 68 NUnit tests, the explicit representative
  24-mover gate, compiled content, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot-v2 continuation, regeneration and macOS export.
- A launchable debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.

## Current gates

- Representative 24-mover Movement Architecture v2 scenario:
  `BLOCKING_NOW` for M2; implemented and green in its focused local run. It
  covers all footprint families, two constrained-route cohorts, the authored
  medium and Heavy passages, controlled opposing-friendly Heavy/Small traffic,
  formation reflow, Excavatable topology refresh, legal starts, completion,
  deterministic recovery diagnostics and repeated final hashes.
- Legacy 60-mover stress: `DIAGNOSTIC` during M2–M5 and `BLOCKING_LATER`
  before M6. The latest full run remains diagnostic-failing at 51.67%
  completion (31/60), 8,483 oscillation incidents and elevated tail latency;
  it does not block the current M2 task.

## Known unresolved work

- human movement-feel acceptance, especially confirmation that completed units
  no longer visibly jitter/dance and that Heavy/Small yielding reads naturally;
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

1. Human movement-feel playtest of the exported macOS build.
2. Merge and close M2 only after human acceptance.
3. Only after M2 acceptance: M3.
