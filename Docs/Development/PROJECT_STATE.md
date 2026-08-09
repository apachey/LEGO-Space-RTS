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
  and reservation determinism uses a dedicated conflict fixture.
- PR #3 Phase 09B governance sync and canon-index entry are merged.
- The current fast verification gate is green, including builds, NUnit,
  compiled-content validation, HeadlessSim and Godot headless smoke.

## Current gates

- Representative 24-mover Movement Architecture v2 scenario:
  `BLOCKING_NOW` for M2; not yet implemented.
- Legacy 60-mover stress: `DIAGNOSTIC` during M2–M5 and `BLOCKING_LATER`
  before M6.

## Known unresolved work

- Movement Architecture v2 runtime implementation;
- representative 24-mover gate implementation;
- final M2 playable macOS build and human movement-feel acceptance.

## Explicitly rejected / do not resurrect

- universal 12-tick space-time movement scheduler as normal locomotion;
- passage coordinator;
- convoy scheduler;
- staging scheduler;
- general MAPF, CBS or cooperative A*;
- benchmark-specific movement hacks.

## Next approved development sequence

1. Persistent route-corridor foundation.
2. Deterministic local separation/yield integration.
3. Formation/cohort reflow integration.
4. Representative 24-mover M2 scenario.
5. Automated M2 verification.
6. macOS playable build.
7. Human playtest.
8. Only after M2 acceptance: M3.
