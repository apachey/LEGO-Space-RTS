# LEGO SPACE RTS — CURRENT PROJECT STATE

This file summarizes current repository state. Canon and current Git evidence
remain authoritative when anything here becomes stale.

## Current milestone

**M3 — Economy & Base Building / deterministic Ore loop in progress.**

The revised M2 automated gates pass and PR #8 is merged. Human movement-feel
acceptance is also complete: the remaining settling jitter is minimal and some
movement actions can still read oddly, but the game director accepted both as
non-blocking polish rather than further M2 work.

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

## Completed / current branch work

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
- The current M3 T035 full verification is green across every
  `BLOCKING_NOW` stage: builds, 105 NUnit tests, the explicit representative
  24-mover gate, compiled content, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
- A launchable debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- M3 T030 resource nodes are implemented on the current task branch: canonical
  finite Ore definitions cover Small (600), Standard (900), Rich (1,350) and
  Deep contested (2,400) deposits; the prototype map provides two visible
  Standard deposits / 1,800 safe Ore per starting side.
- Resource depletion is authoritative, clamps at zero, drives four readable
  depletion stages plus exhaustion, participates in state hashing and survives
  snapshot/replay continuation. Snapshot v3 supports heterogeneous ECS entities
  while retaining read compatibility with M2 snapshot v2.
- M3 T031 worker harvesting is implemented on the current task branch. A
  player-facing right-click Harvest command affects eligible Crew only; each
  Crew extracts exactly 1 Ore per 30 authoritative ticks and carries at most 8.
- Full and partial loads are physically returned to the nearest owned starting
  HQ receiver. Delivered material remains explicitly `PendingHauledAmount`, not
  a spendable/global bank, preserving the canonical raw → hauled → processed
  boundary for T032.
- The prototype map now includes one visible starting HQ receiver per player.
  Snapshot v4 / simulation protocol v2 preserve worker task progress, carrier
  payloads, receiver targets, queued Harvest targets and hauled receiver state;
  legacy snapshot v2/v3 readers remain supported.
- Human T031 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed the playable harvesting, carrying and HQ-delivery loop
  works. No blocking interaction or readability defect was reported.
- M3 T032 resource banking is implemented on the current task branch. Each HQ
  receiver owns a local authoritative processed-Ore reserve; hauled Ore remains
  staged for one tick before deterministic banking, and the debug HUD reports
  the player's processed and waiting-at-HQ totals.
- Conservation measurement covers the complete finite-resource path — raw
  deposits, Crew cargo, hauled receiver inventory and processed local reserves.
  Tick-by-tick regression coverage verifies that the playable extraction and
  delivery loop neither creates nor loses Ore. Snapshot v5 / simulation
  protocol v3 preserve local bank state; readers retain v2-v4 compatibility.
- Human T032 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that Ore is successfully harvested and reaches the
  authoritative processed reserve. No blocking economy-loop defect was
  reported.
- M3 T033 construction placement is implemented on the current task branch.
  Build Mode previews grid-snapped ghosts for the four first-playable Rock
  Raider structures and reports server-equivalent invalid-placement reasons.
- Each standard player start now receives the canonical 500 processed Ore,
  making the approved early buildings immediately placeable. The scheduled
  T037 Energy implementation will add the separate canonical starting Energy.
- Authoritative placement validates an owned Crew builder, prerequisite HQ,
  compiled rotated footprint mask, terrain/elevation, entity and resource-node
  occupancy, authored production exit and a single local Ore reserve. A valid
  order reserves its full Ore cost and creates the Construction Site with the
  final entity identity and final blocking footprint.
- Unstarted cancellation returns the full Ore reservation and releases the
  topology footprint. Snapshot v6 / simulation protocol v4 and replay v2
  preserve placed sites and pending Build commands. Prototype content v5 adds
  the canonical 8x8 HQ, 6x6 Processing Plant, 5x5 Power Station and rotatable
  8x6 Vehicle Service Bay definitions.
- Human T033 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that construction placement, Ore reservation and the
  unstarted cancellation/refund loop all work. No blocking placement or
  readability defect was reported.
- M3 T034 construction jobs are implemented on the current task branch. The
  assigned Crew physically travels to the reserved footprint edge before work
  begins; Shift-placement queues multiple sites and right-clicking a site lets
  other selected Crew assist.
- The first work tick commits 20% of reserved Ore and the remaining 80% is
  consumed progressively. Started-site cancellation returns all unspent Ore
  plus 50% of consumed Ore. Each contributing Crew supplies one deterministic
  work tick, so one Crew matches the canonical build time while assistance
  accelerates completion.
- Completion transitions the existing Construction Site entity to a completed
  Building without changing its identity or reserved footprint. The prototype
  presentation grows the amber site with authoritative progress and displays a
  percentage before switching to the completed-building presentation.
- Snapshot v7 / simulation protocol v5 preserve Builder jobs, construction
  targets, progressive commitment and queued site visits. Replay v3 supports
  the new assist command while retaining current v1-v2 read compatibility.
- Human T034 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that Crew travel, visible construction progress and
  completed-building transition all work. No blocking construction-job or
  readability defect was reported.
- M3 T035 production queues are implemented on the current task branch. Each
  completed HQ or Vehicle Service Bay owns an authoritative ordered queue of
  at most eight units. Queueing reserves the full Ore cost from the nearest
  owned local reserve; Energy, Crystal and Operations Capacity requirements
  are retained on the job for their scheduled systems rather than silently
  enforced by a temporary global wallet.
- Production advances at 20 Hz using the canonical Crew, Hover Scout, Rapid
  Rider and Loader Dozer costs and build times. A completed unit validates the
  building's authored exit, receives a deterministic short-term spawn
  reservation and waits inside the facility when every legal exit position is
  blocked. No speculative entity is repeatedly created and deleted.
- Completed production buildings can be selected directly. The prototype HUD
  exposes the four relevant production actions, Shift queues five copies using
  shortest projected completion and deterministic round-robin ties, and queue
  progress reports a blocked exit. Right-click assigns a rally point; a Crew
  rallied onto a visible Ore deposit immediately enters the harvesting loop.
- Snapshot v8 / simulation protocol v6 preserve queues, reservations, blocked
  completion and rally state. Replay v4 accepts the production and rally
  commands while retaining current v1-v3 read compatibility. Prototype content
  v6 compiles four production definitions and the canonical Rapid Rider mover.

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
  it does not block the current M3 task.

## Known unresolved work

- residual movement polish: completed units can still show minimal settling
  jitter and some movement actions can read oddly. This is accepted as
  non-blocking for M2; address only from a concrete reproduction or a later
  milestone requirement rather than reopening broad movement architecture;
- pre-existing HPA cluster-size discrepancy: Phase 09 specifies 10 build cells
  / 20 navigation nodes, while the imported runtime/static validator currently
  use 10 navigation nodes / 5 build cells; resolve in a separate canon-alignment
  task before changing cluster geometry;
- T035 retains but does not yet enforce Operations Capacity and Energy costs:
  T036 owns OC reservation/cap transitions and T037 owns Energy Domains as
  scheduled. The full production overview, waiting-item drag reordering and
  cancellation/refund presentation remain later interface/economy work beyond
  the T035 unit-spawn acceptance gate.

## Explicitly rejected / do not resurrect

- universal 12-tick space-time movement scheduler as normal locomotion;
- passage coordinator;
- convoy scheduler;
- staging scheduler;
- general MAPF, CBS or cooperative A*;
- benchmark-specific movement hacks.

## Next approved development sequence

1. Complete T035 human unit-production playtest acceptance.
2. Merge the accepted stacked T030-T035 M3 economy/base-building branches.
3. Begin T036 Operations Capacity after T035 acceptance.
