# LEGO SPACE RTS — CURRENT PROJECT STATE

This file summarizes current repository state. Canon and current Git evidence
remain authoritative when anything here becomes stale.

## Current milestone

**M5 — Four-Faction System Proof / T056 Tube transit implemented and fully verified on the current stacked task branch.**

M3 is merged and human-accepted. At the game director's explicit request,
development moved directly to M5 T049 rather than beginning M4 T040. M4 combat
T040-T048 remains unimplemented and deferred; it must not be described as
complete or silently treated as an M5 dependency.

T049 is human-accepted and recorded in commit `3f677b2`. T050 is recorded in
commit `8731f61`; T051 is recorded in commit `ad93ec1`; T052 is recorded in
commit `f6f390a`; T053 is recorded in commit `5844ee1`; T054 is recorded in
commit `2525747`. T055 is recorded in commit `da075fc`. T056 is stacked on all
seven in `codex/m5-tube-transit`; none of these M5 task commits is merged to
main.

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
- The current M3 T039 full verification is green across every
  `BLOCKING_NOW` stage: builds, 125 NUnit tests, the explicit representative
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
  making the approved early buildings immediately placeable. T037 now supplies
  the separate canonical starting Energy through each HQ domain.
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
- Human T035 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that producer selection, queue progress, unit spawning and
  rally behavior work. No blocking production interaction or readability defect
  was reported.
- M3 T036 Operations Capacity is implemented on the current task branch.
  Authoritative OC is derived in stable Entity ID order from active mobile-unit
  metadata, every queued production reservation and completed
  capacity-providing buildings. The competitive maximum is clamped to 100.
- Production now validates `active + reserved + product <= maximum` before Ore
  is spent. Accepted items reserve their full OC immediately; completion moves
  the amount from reserved to active. Removing a queue item releases its derived
  reservation, and losing infrastructure may create an over-cap state without
  deleting or penalizing existing units.
- Prototype content v7 compiles the canonical Rock Raider unit costs — Crew 1,
  Hover Scout 1, Rapid Rider 2, Loader Dozer 3 and Chrome Crusher 6 — plus HQ
  +16 and Vehicle Service Bay +4 capacity sources. Production definitions are
  compiler-validated against their unit metadata.
- With game-director approval, the player-facing executable now uses the
  canonical opening roster of one HQ and six Crew per player, displaying
  `6 / 16 OC`. The original mixed 18-unit DEV scenario remains unchanged for
  M2 movement and deterministic engineering coverage.
- The prototype HUD shows used/maximum OC, queued reservations, the 85% warning
  and `OVER CAPACITY`; production buttons include OC cost and disable when the
  next unit would exceed capacity. OC is re-derived after snapshot restore from
  already-serialized units, buildings and production queues, so snapshot v8 and
  simulation protocol v6 remain valid without duplicated cached state.
- Human T036 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that the canonical six-Crew opening, OC display,
  reservation warning and queued-to-active capacity transition work. No
  blocking Operations Capacity defect was reported.
- M3 T037 Energy Domains are implemented on the current task branch. Each
  starting HQ roots an authoritative domain with the canonical 120 / 150
  Energy reserve and +2 E/s auxiliary generation. Cached domain generation,
  reserve capacity and continuous demand update on building completion rather
  than scanning the whole base every simulation tick.
- Prototype content v8 adds the canonical first-playable Rock Raider values:
  HQ +2 E/s and +150 reserve capacity, Power Station +10 E/s and +120 reserve
  capacity, and 1 E/s continuous demand for both the Ore Processing Plant and
  Vehicle Service Bay. New expansion HQs establish independent domains; the
  later T049 Worksite graph remains responsible for zone-overlap merge/split.
- Construction and production now validate and withdraw one-time Energy before
  accepting an order. Construction tracks reserved/consumed Energy alongside
  Ore and applies the same 20% commitment, progressive consumption and
  cancellation refund rules. Insufficient Energy rejects an order without
  spending Ore.
- Surplus and deficit flow use exact deterministic 20 Hz fixed-point
  accumulation. Deficit may drain reserve to zero, but structure shutdown is
  intentionally deferred to T038 Brownout. Snapshot v9 / simulation protocol
  v7 preserve domain reserve, membership and construction commitments while
  retaining v2-v8 read compatibility.
- The prototype HUD shows reserve/capacity, generation, demand and a visible
  `RESERVE DRAINING` state. Build previews include Ore plus Energy cost;
  production buttons expose Energy cost and disable when the selected domain
  cannot pay it.
- Human T037 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that the Energy HUD, reserve flow, generation/demand and
  construction/production Energy spending work. No blocking Energy Domain
  interaction or readability defect was reported.
- M3 T038 Brownout is implemented on the current task branch. When a domain's
  reserve reaches zero while demand exceeds generation, complete consumers are
  powered whole or disabled in deterministic functional-class, user-priority
  and Entity-ID order. High / Normal / Low is authoritative per-building state;
  reactivation uses the same ordering and transition revisions change only
  when the powered set changes.
- Disabled resource processing and production stop their authoritative work;
  queued production progress is retained and resumes after recovery. Mobile
  units remain controllable. Snapshot v10 / simulation protocol v8 and replay
  v5 preserve priority, powered state, Brownout transitions and pending priority
  commands while retaining prior readers.
- The prototype HUD reports domain Brownout and powered/total demand, selected
  structures report the exact disable reason and priority, production reports
  a retained-progress pause, and disabled buildings receive a distinct dark
  material plus `BROWNOUT` world label. A development-only drain command makes
  the human readability gate reproducible without waiting for reserve depletion.
- Human T038 playtest acceptance is complete as of 2026-08-10: the game
  director confirmed that Brownout entry, disabled-building feedback and manual
  priority reassignment work. No blocking functional defect was reported. The
  broader concern that several individually reasonable systems could create
  unfun aggregate complexity remains a design-review consideration for later
  milestone playtests, not a T038 implementation failure.
- M3 T039 Basic HUD is implemented and human-accepted on the current task
  branch. The permanent player-facing debug text
  wall has been replaced by the shared Phase 07/08 skeleton: a compact top
  economy strip and one bottom selection/state area. The normal battlefield
  center remains unobstructed.
- The top strip now presents processed Ore, the canonical compact Energy
  reserve/capacity/generation/demand/net expression, spendable Crystals and
  used/maximum Operations Capacity. Queued OC, the 85% warning, over-cap state,
  pending received Ore and Energy deficit/Brownout treatments remain visible
  without exposing simulation diagnostics.
- Clicking Energy opens a compact single-domain diagnostic popover. Brownout
  produces one domain-level banner and selected disabled buildings state the
  exact cause. High/Normal/Low controls exist only in the selected-building
  status area and disappear entirely during ordinary play when no compatible
  consumer is selected.
- Selection now gives readable entity names and contextual Worker,
  construction, power and production status, with layout space reserved for a
  future unit/structure portrait. There is no permanent separate Production
  panel: facility actions appear inside the selection area only while a valid
  producer is selected. Canonical costs, tooltips, click/Shift-click behavior,
  retained queue progress and Brownout pause state remain available.
- Navigation/performance metrics, visualization toggles and the development
  Energy-drain trigger moved to a separate hidden F8 developer panel. Navigation
  grids, HPA clusters, persistent path lines and excavatable outlines are all
  off during normal play. Move feedback is now a small unnumbered translucent
  ring with a short canonical lifetime. The unusably large blurred fixed-size
  construction percentage billboard is removed: world-space progress is a thin
  bar and the exact percentage remains in the selected-site panel. The
  contextual-action column now reserves stable width, but human review
  clarified that the remaining HQ-selection defect is vertical growth of the
  bottom panel beyond its intended bounds, not horizontal layout movement.
  Runtime Godot smoke verifies
  the resource, selection, portrait and contextual-action anchors; a dedicated
  construction capture seeds and verifies the progress bar. No authoritative
  SimCore or protocol state changed in this T039 refinement.
- Human T039 playtest acceptance is complete as of 2026-08-10: the game
  director accepted the current Basic HUD baseline and requested moving on.
  Selecting Rock Raiders HQ can still make the bottom HUD extend downward
  beyond its intended bounds; the stable-width change did not fix that height
  defect. It is explicitly accepted as non-blocking UI polish and must not be
  reported as fixed.
- M5 T049 Worksite graph is implemented on the current task branch. Completed
  Rock Raiders HQs and Vehicle Service Bays create the canonical 18-cell and
  12-cell service zones from compiled content metadata. Same-owner overlapping
  zones form cached deterministic components rooted by the lowest Entity ID;
  building operating centers determine service membership.
- Worksite topology updates are event-driven on scenario initialization,
  building placement/cancellation and construction completion. Merge and split
  preserve exact whole/raw Energy reserve across surviving storage capacity,
  rederive generation/demand/Brownout membership, and preserve local HQ Ore
  banks rather than physically merging them.
- Connected Worksites expose all local processed Ore banks to construction and
  production. A single order may draw from multiple same-component banks in
  deterministic nearest-bank/Entity-ID order; disconnected components never
  pool funds. An already supplied production queue remains present after a
  split, while new orders immediately lose remote-bank access.
- The HUD now reports Ore and Energy for the selected/active Worksite, shows
  component count, labels selected structures as serviced or disconnected, and
  identifies the specific Worksite in Brownout alerts. Authoritative Worksite
  node/member/component state is covered by snapshot v11, simulation protocol
  v9 and replay v6; content schema/binary format v10 carries service radii and
  retains older binary readers.
- T049 regression coverage verifies service geometry and ownership, bridge
  merge, removal split, local Ore retention, Energy conservation, multi-bank
  spending, immediate access loss, idempotent graph rebuild and deterministic
  snapshot continuation.
- T049 full verification is green across every `BLOCKING_NOW` stage: warnings-
  as-errors builds, 130 NUnit tests, representative 24-mover acceptance,
  compiled-content identity, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  The preserved 60-mover stage remains diagnostic-failing at its unchanged
  31/60 completion and 8,483 oscillation incidents.
- A launchable T049 debug build exists at `Builds/macOS/LEGO Space RTS.app` and
  passed export smoke. `Artifacts/Screenshots/m5-worksite-hud.png` confirms the
  M5 resource strip and selected-structure Worksite status render cleanly.
- Human T049 readability acceptance is complete as of 2026-08-11: the game
  director confirmed that the selected-structure Worksite status and active
  Worksite resource display are understandable in the playable build.
- M5 T050 Excavation topology is implemented on the current stacked task
  branch. The authored prototype shortcut is now a stable Fractured Rock Wall
  feature with canonical 25-Energy metadata, a visual-profile ID and explicit
  non-buildable opened terrain. Compiled map/source schema v4 carries the full
  metadata and retains readers for compiled map versions 1–3.
- Every authored feature binds to an authoritative ECS Excavatable entity in
  stable feature-ID order. Blocked/ActiveExcavation/Open state participates in
  snapshot v12, simulation protocol v10, replay v7, deterministic hashes and
  ordered state dumps. Legacy snapshots receive deterministic feature entities
  from their preserved map state.
- Opening is one-way and idempotent: Excavatable, Impassable and GroundOccluder
  flags clear together, topology advances exactly once, only affected HPA
  clusters plus the existing neighbor halo rebuild, and only spatially affected
  corridors become dirty. The opened route is ordinary owner-neutral ground.
- Six focused T050 regression tests are authored for metadata/entity identity,
  local/idempotent rebuild, universal routing, compiled-map roundtrip,
  snapshot/hash continuity and invalid authored overlap/identity rejection.
- T050 full verification is green across every `BLOCKING_NOW` stage: warnings-
  as-errors builds, 136 NUnit tests, representative 24-mover acceptance,
  compiled-content identity, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  The preserved 60-mover stage remains diagnostic-failing at its unchanged
  31/60 completion and 8,483 oscillation incidents. The authoritative summary
  is `Artifacts/Verification/20260811T175008Z-full-summary.txt`.
- The T050 Godot visual smoke opens the feature through the deterministic
  command path, verifies matching MapGrid/ECS Open state and captures the
  resulting route at `Artifacts/Screenshots/m5-excavation-open.png`.
- M5 T051 Forward Service membership is implemented on the current stacked
  task branch. Completed, operational `building.ast.service_refit_hub` sources
  provide the canonical 18-cell radius; only `DeploymentState.Deployed`
  `unit.ast.solar_explorer` sources provide the canonical 10-cell radius.
  Construction sites, mobile/transitioning Solar Explorers and Brownout-
  disabled Hubs do not provide service.
- Same-owner T3-Trikes cache a stable primary provider in deterministic Entity
  ID order. Providers occupy dedicated four-cell spatial buckets; a member is
  re-queried only after crossing a build-grid query cell or after the active
  provider set changes through deployment, destruction, ownership, position or
  power state. The runtime never performs an every-member/every-provider nested
  scan each tick, and Forward Service grants no combat aura.
- Deployment, provider and member state participate in snapshot v13,
  simulation protocol v11, replay v8, deterministic hashes and ordered state
  dumps. Six focused regressions cover canonical activation/radii, exact
  geometry and ownership, query-cell movement, deployment/destruction/
  Brownout invalidation, incomplete Hub rejection and deterministic snapshot
  continuation. The selected-unit HUD exposes `Service Available` / `No
  Forward Service`, and the F8 developer tools can draw provider radii.
- T051 full verification is green across every `BLOCKING_NOW` stage: warnings-
  as-errors builds, 142 NUnit tests, representative 24-mover acceptance,
  compiled-content identity, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  The golden/replay hash is `234EBEC1C990C6AB`; the snapshot continuation hash
  is `BF3928D89A687E6E`. The preserved 60-mover stage remains diagnostic-failing
  at its unchanged 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260811T181947Z-full-summary.txt`.
- A launchable T051 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.
- M5 T052 Mission Refit is implemented for the narrow T3-Trike proof. T3s
  initialize in Escort with the Escort module owned. An authoritative explicit
  command validates ownership, Field Survey Package proof-unlock, active same-
  owner Forward Service, configuration legality, funds and the 20-second
  Configuration Lock before creating a service job on the same entity.
- First Survey installation commits the canonical 25 Ore / 10 Energy for 18
  seconds. Once Survey is owned, Escort↔Survey swaps commit 8 Ore / 5 Energy
  for 10 seconds. Completion preserves Entity ID, records owned modules, applies
  the 20-second lock and refreshes the currently implemented canonical T3
  movement/sight values (Escort 1.70/9; Survey 1.85/13). Active jobs reject
  movement commands and retain normalized authoritative progress.
- Mission configuration, owned-module mask, proof research unlock, lock timer
  and committed service-job state participate in snapshot v14, simulation
  protocol v12, replay v9, hashes, state dumps and Ore conservation accounting.
  Four focused regressions cover first install/cost/identity, later swap and
  lock, command rejection, movement downtime and mid-job deterministic
  snapshot continuation. The selected-unit HUD reports configuration, module,
  lock and refit progress.
- T052 full verification is green across every `BLOCKING_NOW` stage: warnings-
  as-errors builds, 146 NUnit tests, representative 24-mover acceptance,
  compiled-content identity, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  The golden/replay hash is `89EBF2D2B4CA5A67`; the snapshot continuation hash
  is `A8259F4F2EE7B862`. The preserved 60-mover stage remains diagnostic-failing
  at its unchanged 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260811T183406Z-full-summary.txt`.
- A launchable T052 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.
- M5 T053 Resonance Core commitment is implemented as a narrow authoritative
  Alien-system proof. A completed `building.ali.resonance_core` links to one
  same-owner `building.ali.etx_command_core` in the same Energy Domain, and a
  Command Core may own at most one Resonance Core.
- Each Core owns an explicit deterministic slot mask with four baseline slots
  or six after the proof Expanded Resonance Lattice unlock. The explicit
  desired-count command validates ownership, capacity and spendable faction
  Crystals, then performs one operation at a time in stable slot order: 8
  seconds to commit and 15 seconds to withdraw.
- A committing Crystal leaves the spendable bank when its operation starts and
  occupies its slot on completion. Withdrawal removes its active commitment at
  operation start, retains the physically installed Crystal and its Energy
  pressure during progress, then returns that Crystal to the authoritative
  bank on completion. Crystal conservation covers occupied and transitioning
  slots.
- Core demand is integrated into the existing Energy Domain/Brownout ordering:
  3 E/s base plus 2 E/s for each physically installed Crystal. Brownout pauses
  Charge-producing operation without releasing commitments. The selected-Core
  HUD exposes occupied/maximum/desired slots, demand, operational state and
  normalized transition progress; the resource strip now reports spendable,
  committed and transitioning Crystals.
- Resonance state and pending desired-count commands participate in snapshot
  v15, simulation protocol v13, replay v10, deterministic hashes and ordered
  state dumps. Five focused regressions cover sequential canonical timing,
  withdrawal timing and conservation, validation/one-Core limits, Brownout
  retention and mid-transition snapshot continuation with a pending command.
- T053 full verification is green across every `BLOCKING_NOW` stage: warnings-
  as-errors builds, 151 NUnit tests, representative 24-mover acceptance,
  compiled-content identity, HeadlessSim, Godot headless smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  The golden/replay hash is `F39B55E2DBB4D8EF`; the snapshot continuation hash
  is `4874BF0ED8A2272A`. The preserved 60-mover stage remains diagnostic-failing
  at its unchanged 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260811T190115Z-full-summary.txt`.
- A launchable T053 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.
- M5 T054 implements authoritative Alien Charge in integer millicharge. Each
  valid Resonance Core contributes 20 maximum Charge plus 20 per committed
  Crystal; each committed Crystal generates 0.4 Charge/s while its Core is
  powered. Generation stops at maximum, never decays, pauses under Brownout,
  and excess Charge discharges immediately when commitment capacity is removed.
- Resonance Initiation proof state unlocks a command-driven Surge anchored on a
  valid, owned, operational Core. Activation spends 50 Charge immediately,
  runs a 0.75-second buildup, then provides an 18-second radius-12 zone.
  Eligible same-owner receivers gain and lose membership immediately at the
  boundary; concurrent overlaps resolve to one stable lowest-Entity-ID anchor
  and never stack. Canonical helpers apply ×0.80 cooldown and ×0.70 ETX
  reconfiguration timing without changing damage, range or movement.
- Charge, proof unlock, Surge zones, receiver membership and pending Surge
  commands participate in snapshot v16, simulation protocol v14, replay v11,
  hashes and ordered state dumps. The HUD reports Charge/current maximum/rate,
  committed Crystals, Core contribution/Brownout state and active zone timers.
  Five focused regressions cover generation and capacity, Brownout/withdrawal,
  exact Surge timing and cost, ownership/power/radius/non-stacking validation,
  and deterministic snapshot continuation.
- T054 full verification is green across every `BLOCKING_NOW` stage:
  warnings-as-errors builds, 156 NUnit tests, representative 24-mover
  acceptance, compiled-content identity, HeadlessSim, Godot headless smoke,
  100-repeat determinism, replay, snapshot continuation, regeneration and
  macOS export. The golden/replay hash is `2279975248A1B755`; the snapshot
  continuation hash is `3694DFD1315BCF18`. The preserved 60-mover stage remains
  diagnostic-failing at 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260815T110324Z-full-summary.txt`.
- A launchable T054 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.
- M5 T055 implements the authoritative Martian Aero Tube graph proof. Completed
  Aero Tube Hangars and Settlement Stations register as explicit graph nodes;
  completed Links store normalized endpoint IDs, an ordered build-cell route,
  canonical length, 1 E/s demand and operational state. Automatic routing uses
  deterministic Station sockets and two stable orthogonal candidates, rejecting
  blocked routes without asking the player to draw bends.
- Link/component topology is event-driven rather than scanned every tick.
  Deterministic BFS assigns each connected component the lowest Station Entity
  ID, caches Station/operational-Link counts and increments one component-level
  segmentation revision when a surviving component splits. Disabled Links keep
  their physical connection slot but do not connect components; invalid incident
  Links and orphan routes are removed deterministically after Station loss.
- Baseline limits are three Links per Settlement Station and five per Hangar;
  Redundant Routing adds one. Canonical completed-Link economics are exposed as
  50 + 2 Ore per build-grid cell, 10 Energy activation and 10 seconds + 0.4
  seconds per cell. Connected Station banks are jointly queryable/spendable in
  stable order while segmentation preserves each Station's physically local
  resource ownership.
- Tube Stations, Links, ordered routes, cached components and topology/
  segmentation revisions participate in snapshot v17, simulation protocol v15,
  replay v12, hashes and ordered state dumps. The selected-object HUD reports
  connection usage, component size, active Links, endpoints, length and demand.
  Five focused regressions cover limits/upgrades, automatic routing/economics/
  validation, resource pooling and split/reconnect, Station-loss cleanup and
  deterministic snapshot continuation.
- T055 full verification is green across every `BLOCKING_NOW` stage:
  warnings-as-errors builds, 161 NUnit tests, representative 24-mover
  acceptance, compiled-content identity, HeadlessSim, Godot headless smoke,
  100-repeat determinism, replay, snapshot continuation, regeneration and
  macOS export. The golden/replay hash is `3113B1A3BB4C900D`; the snapshot
  continuation hash is `FE000EA90D0C20A0`. The preserved 60-mover stage remains
  diagnostic-failing at 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260815T114624Z-full-summary.txt`.
- A launchable T055 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.
- M5 T056 implements deterministic Aero Tube passenger transit for Worker
  Robots, Double Hovers and Jet Scooters while rejecting heavy/ineligible
  units. Transfers retain their passenger Entity ID through approach, stable
  Station queueing, loading, off-map travel, unloading, blocked-exit waiting
  and Arrival Recovery; off-map passengers have no Transform and therefore do
  not participate in ground spatial queries or normal targeting.
- Canonical timing is exact at 20 Hz: 3 seconds loading, ceiling-rounded 0.12
  seconds per ordered Tube build-grid cell, 2 seconds unloading and 0.75
  seconds Arrival Recovery. Each Station provides two simultaneous channels,
  raised to three by Hypersled Throughput; overflow is ordered by request tick
  and passenger Entity ID. Move orders issued during transfer are retained as
  post-recovery arrival orders.
- Transfers select a stable lowest-Link-ID BFS route. A broken selected route
  safely returns the passenger to its origin after 6 seconds; Redundant Routing
  instead selects a surviving alternate route when one exists. Destination
  exits reserve the local 3x3 area, wait up to 4 seconds when obstructed, then
  use a deterministic nearest legal fallback within three build cells. No
  arbitrary passenger loss or presentation-owned gameplay state is introduced.
- Tube transfer jobs and selected Link routes participate in snapshot v18,
  simulation protocol v16, replay v13, hashes and ordered state dumps. The
  Station HUD reports its two/three transfer-channel capacity. Six focused
  regressions cover eligibility/timing, queue throughput, off-map identity and
  deferred orders, safe return, redundant rerouting and mid-transit snapshot
  continuation.
- T056 full verification is green across every `BLOCKING_NOW` stage:
  warnings-as-errors builds, 167 NUnit tests, representative 24-mover
  acceptance, compiled-content identity, HeadlessSim, Godot headless smoke,
  100-repeat determinism, replay, snapshot continuation, regeneration and
  macOS export. The golden/replay hash is `16DEA4B5FC2DD331`; the snapshot
  continuation hash is `6A4508CE6B19BCAC`. The preserved 60-mover stage remains
  diagnostic-failing at 31/60 completion and 8,483 oscillation incidents. The
  authoritative summary is
  `Artifacts/Verification/20260815T121716Z-full-summary.txt`.
- A launchable T056 debug build exists at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke.

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
  it does not block the current M5 task.

## Known unresolved work

- residual movement polish: completed units can still show minimal settling
  jitter and some movement actions can read oddly. This is accepted as
  non-blocking for M2; address only from a concrete reproduction or a later
  milestone requirement rather than reopening broad movement architecture;
- selecting Rock Raiders HQ can increase the bottom HUD's vertical extent
  beyond its intended bounds. T039 human acceptance classifies this as
  non-blocking UI polish; the existing stable-width contextual slot does not
  resolve it;
- pre-existing HPA cluster-size discrepancy: Phase 09 specifies 10 build cells
  / 20 navigation nodes, while the imported runtime/static validator currently
  use 10 navigation nodes / 5 build cells; resolve in a separate canon-alignment
  task before changing cluster geometry;
- The minimap, full 3×4 command grid, F3 production overview, waiting-item drag
  reordering and cancellation/refund presentation remain later
  interface/economy work beyond the current prototype gates.
- The canonical implementation schedule assigns the functional fog-correct
  minimap to M7 T069, so it is intentionally not pulled into T039. Building
  prototype records currently specify zero vision radius; adding local building
  vision requires an explicit gameplay/balance value rather than a presentation
  guess.
- Fog currently persists explored terrain cells but the presentation snapshot
  intentionally omits Ore and enemy objects once they leave current vision. The
  canonical result is a per-player last-known record: Ore must retain its last
  observed depletion state and enemy structures their last observed state,
  without publishing live hidden changes. Do not "fix" this by rendering the
  current SimCore entity in explored fog; that would become an information leak
  in multiplayer. Add serialized/per-viewer knowledge through a separately
  reviewed fog-information task no later than M6 T061 fog filtering.
- Phase 04 canon promises a short local Energy buffer for disconnected Raider
  infrastructure but does not assign its capacity. T049 does not invent a
  balance value for a Service-Bay-only component after HQ/storage loss. Resolve
  that exact buffer rule before M4 destruction can create this state in normal
  play; ordinary HQ-to-HQ Worksite split/merge is implemented and covered.
- T050 intentionally does not guess per-machine excavation duration within the
  canonical 15–35 / 30–60 second ranges. The deterministic F9 completion
  command continues to exercise topology; player-facing Excavate validation,
  progress and Energy spending require approved exact timing/eligibility data.
- T051 consumes authoritative deployment state but intentionally does not add
  the player command, 3.0/2.5-second Solar Explorer transition job or its
  presentation. Those belong to deferred T048 Transformation or a separately
  approved integration task; focused fixtures exercise the canonical deployed
  state without inventing an alternate transformation rule. Full Astronaut
  roster/infrastructure import remains scheduled for T070/T071.
- T052's proof unlock is authoritative but unit-local until the full Research
  DAG/player technology state arrives in T072. Normal provider destruction and
  Solar Explorer undeploy commands are unavailable because T045/T048 remain
  deferred; their mid-refit cancellation integration is therefore not exposed
  in the current executable. Before implementing that later path, canon must
  resolve how the 50% refund of the odd 25-Ore first-install cost rounds in the
  integer resource model.
- T054 provides the authoritative Charge/Surge slice, but actual weapon reload
  remapping and ETX transformation-job integration depend on deferred M4
  T041/T048. Mothership relay anchoring and full research-DAG wiring remain in
  T072; full Alien content/spawning remains in T071, so the current executable
  still does not expose the proof Core or Surge activation in normal play.
  Resonance destruction salvage remains dependent on deferred T045 destruction
  and must integrate the canonical cache rules when that path exists.
- T055 deliberately does not expose a free Link-creation player command. The
  graph accepts only a completed Link and exposes the canonical cost/timing;
  player-facing construction funding/progress, physical Link presentation and
  Martian local Energy-domain pooling require the full Martian content/economy
  integration scheduled for T071. T056 exposes the authoritative transfer API
  and state machine but does not invent missing Martian production content,
  player-facing Station selection/command wiring or Tube presentation; those
  remain part of the scheduled full Martian integration.

## Explicitly rejected / do not resurrect

- universal 12-tick space-time movement scheduler as normal locomotion;
- passage coordinator;
- convoy scheduler;
- staging scheduler;
- general MAPF, CBS or cooperative A*;
- benchmark-specific movement hacks.

## Next approved development sequence

1. Finish the T056 stacked branch handoff; merge only through game-director
   review.
2. Continue M5 with T057 Displacement/Stability after T056 is accepted/merged, unless
   the game director explicitly returns to deferred M4 T040 Targeting.
