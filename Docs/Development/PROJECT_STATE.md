# LEGO SPACE RTS — CURRENT PROJECT STATE

This file summarizes current repository state. Canon and current Git evidence
remain authoritative when anything here becomes stale.

## Current milestone

**M4 — Combat / T045 playtest handoff corrected; human retest pending.**

The accepted M3 T030-T039 economy/base-building stack is merged through PR #10.
M4 T040 Targeting, T041 Weapons, T042 Projectiles and T044 Contact weapons are
fully automated-verified and human-accepted. T043 Damage / Armor and T045
Destruction are implemented on the current stacked branches. The first T045
playtest handoff was rejected because it exposed an overlay regression, lacked
the required Chrome/building fixtures and delegated resource setup to the game
director. Those handoff defects are corrected; human retest is still required
before T046 Repair begins.

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
- M4 T040 Targeting is implemented on the current task branch. Canonical target
  class, ground/true-air layer and role flags are compiled for the first-playable
  Rock Raider roster and buildings. Armed entities select only visible hostile
  legal targets through the existing deterministic spatial index, with
  role-profile ranking followed by distance and Entity ID ties.
- Direct Attack is an authoritative queueable command and overrides automatic
  priority while legal. Hidden, friendly and wrong-layer targets are rejected;
  loss of legal visibility clears the direct target without exact through-fog
  tracking. Stop/Move ordinary intent clears the current combat target.
- Player-facing right-click on a visible enemy issues Attack only to compatible
  selected units. An orange-red world ring identifies the current target of the
  selected group; a wholly unarmed selection is not converted into a charge.
- Snapshot v11 / simulation protocol v9, replay v6 and prototype content v10
  preserve targeting state, queued Attack intent and canonical target metadata
  while retaining existing legacy readers. T041 builds weapon cooldown/firing
  on that target authority; later combat tasks own projectiles, damage and
  destruction.
- The T040 full verification acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 131 NUnit tests, the representative 24-mover
  movement gate, compiled content, HeadlessSim, Godot smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
- A launchable T040 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- M4 T041 Weapons is implemented on the current stacked task branch. Four
  canonical Rock Raider weapon definitions cover Crew Portable Mining Tool,
  Hover Scout Survey Pulse, Loader Dozer Scoop Ram and Chrome Crusher Chrome
  Drill. Rapid Rider remains an unarmed transport.
- Weapon readiness executes immediately after targeting in the authoritative
  20 Hz pipeline. A ready weapon fires only at a visible legal target inside its
  exact range with clear combat LoS, records a monotonic firing revision and
  begins its exact canonical 24 / 30 / 27 / 32-tick cooldown. Cooldown advances
  without a target; a ready weapon waits without losing readiness when range or
  LoS is invalid.
- Godot receives only the authoritative firing revision/target and presents a
  short amber muzzle flash. Presentation timing cannot authorize a shot or
  alter cooldown. HP damage, armor resolution and contact approach/facing rules
  remain intentionally absent until T043-T044.
- Snapshot v12 / simulation protocol v10 and prototype content v11 preserve
  weapon profile, readiness, firing sequence and last firing event while
  retaining supported legacy readers. Replay remains v6 because T041 adds no
  command encoding.
- The built-in headless content mirror now uses the compiler's canonical stable
  key ordering, so built-in and tracked compiled startup report the same
  gameplay-content hash for identical definitions.
- The T041 full verification acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 138 NUnit tests, the representative 24-mover
  movement gate, compiled content, HeadlessSim, Godot smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
- A launchable T041 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- M4 T042 Projectiles is implemented on the current stacked task branch. The
  canonical Hover Scout Survey Pulse launches an authoritative compact
  projectile record at 12 cells/s; Contact weapons do not create projectile
  records and remain T044 scope.
- Projectile creation and movement execute after weapon firing in stable
  Projectile-ID order at authoritative 20 Hz. Ordinary shots commit the impact
  position at launch, ignore unrelated units and continue to that position if
  the original target is destroyed. They never retarget.
- Every launched shot remains independent. Simultaneous arrivals emit ordered
  impact records without an attacker cap, preserving genuine overkill for the
  T043 damage/armor resolver. T042 does not reduce HP or destroy entities.
- Snapshot v13 / simulation protocol v11 and prototype content v12 preserve
  projectile speed metadata, monotonic IDs, in-flight state and same-tick impact
  output while retaining supported legacy readers. Replay remains v6 because
  T042 adds no command encoding.
- Godot presents visible authoritative projectiles as small interpolated amber
  pulses. Visual objects are presentation-only and cannot collide, redirect or
  authorize impact.
- The T042 full verification acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 143 NUnit tests, the representative 24-mover
  movement gate, compiled content, HeadlessSim, Godot smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
- A launchable T042 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- Human T040-T042 playtest acceptance is complete as of 2026-08-11. The game
  director confirmed automatic target acquisition, direct right-click target
  override, readable target-ring feedback, visible firing feedback and visible
  Survey Pulse travel. The current muzzle flash and projectile are accepted as
  prototype placeholders rather than final combat VFX.
- The same playtest confirmed that Direct Attack currently locks an out-of-range
  target without approaching it. This is an acknowledged missing combat-chase
  behavior, not a T040-T042 regression; the canonical 12-cell pursuit leash must
  be implemented in later M4 combat work before the loop is complete.
- M4 T043 Damage / Armor is implemented on the current stacked task branch.
  Every first-playable Rock Raider unit and building now spawns with its exact
  canonical HP, target class and Armor Rating. Authoritative health uses Fix32,
  preserving fractional matrix/armor results between hits rather than silently
  rounding balance values per shot.
- A delivery-independent SimCore resolver applies the complete six-by-seven
  Phase 06 damage-type matrix, then the canonical A0-A5 multiplier, clamps HP at
  zero and enforces the one-damage armor floor. Projectile impacts feed this
  resolver in stable Projectile-ID order; targets at zero HP are no longer legal
  targets and cannot fire. T045 still owns removal, wrecks and collision timers.
- Snapshot v14 / simulation protocol v12 and prototype content v13 preserve HP,
  Armor Rating, fractional current health and last-damage tick while retaining
  supported legacy readers. Replay remains v6 because T043 adds no command
  encoding.
- Godot displays exact HP / maximum HP, Armor Rating, target class and canonical
  healthy/damaged/heavily-damaged state in the selection panel. Contextual
  world-space health bars appear for selected, targeted or damaged visible
  entities; their state is presentation-only.
- The T043 full verification acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 157 NUnit tests, the representative 24-mover
  movement gate, compiled content, HeadlessSim, Godot smoke, 100-repeat
  determinism, replay, snapshot continuation, regeneration and macOS export.
  Verification summary: `Artifacts/Verification/20260810T232508Z-full-summary.txt`.
- A launchable T043 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- M4 T044 Contact weapons are implemented on the current stacked task branch.
  Contact distance is footprint-aware for units and rectangular structures.
  Attackers reserve distinct legal positions from a deterministic sixteen-point
  engagement ring, retain those reservations while the target moves and choose
  passable alternatives in stable Entity-ID order.
- Contact readiness now enforces canonical facing: ±30° for the Chrome Crusher
  drill and ±45° for the Crew tool and Loader scoop. An attacker may maintain
  engagement at at most 35% of maximum speed; full-speed drive-through contact
  cannot deal damage. Neither participant is locked, so retreat, reversal and
  displacement remain ordinary deterministic movement.
- Contact firing applies immediate delivery through the same canonical
  class/Armor resolver as projectile impacts. Direct attacks with projectile
  weapons now move into range rather than only selecting the target. All direct
  pursuit is bounded by the canonical twelve-cell leash from its recorded
  start, after which an unreachable target is released.
- Snapshot v15 / simulation protocol v13 and prototype content v14 preserve
  pursuit origin, contact-slot reservation, combat-move state, facing tolerance
  and moving-fire limit while retaining supported legacy readers. Replay remains
  v6 because T044 adds no command encoding.
- The T044 full verification candidate is green across every `BLOCKING_NOW`
  stage: builds, 162 NUnit tests, the representative 24-mover movement gate,
  compiled content, HeadlessSim, Godot smoke, 100-repeat determinism, replay,
  snapshot continuation, regeneration and macOS export. Verification summary:
  `Artifacts/Verification/20260811T070232Z-full-summary.txt`.
- A launchable T044 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch.
- The first T044 human playtest found three follow-up defects: world health bars
  inherited unit rotation/scale and could render with confusing overlap;
  Contact weapons reused the projectile-style spherical muzzle flash; and a
  Hover Scout could acquire a follow-up target outside firing range without
  beginning pursuit. These root causes are fixed on the current branch.
- Health bars are now independent camera-facing unshaded quads with explicit
  background/fill render order. Their fill changes mesh geometry around a fixed
  billboard origin, so damage cannot move it away from its background. Health
  materials are isolated from the non-billboard construction-progress
  materials after the shared-material follow-up caused a playtest regression.
  Godot smoke cycles health fill through 75%, 55% and 25%, checks containment
  at each step and verifies a seeded construction bar independently. The
  construction bar now remains at a fixed 1.75-world-unit height while the site
  grows; smoke verifies the invariant at early, middle and complete heights.
- Projectile weapons retain the compact muzzle flash, while Contact weapons
  show a short forward impact plate and continue to create no projectile.
  Invalid direct targets now fall back to automatic acquisition in the same
  targeting pass, and an idle ranged unit pursues its automatic follow-up
  target under the existing deterministic chase rules.
- The hidden F8 developer panel now includes `Move visible enemies`, which
  issues an ordinary deterministic player-1 Move command for reproducible
  moving-target feel review without changing normal match behavior. The
  latest follow-up fast and full verification are green with 165 NUnit tests,
  real Godot visual/runtime smoke, deterministic replay/snapshot continuation
  and macOS export. Full summary:
  `Artifacts/Verification/20260811T085345Z-full-summary.txt`; the final fixed-
  height bar fast verification is
  `Artifacts/Verification/20260811T090944Z-fast-summary.txt`. Human T044
  acceptance is complete as of 2026-08-11.
- M4 T045 Destruction is implemented on the current stacked task branch. A
  zero-HP unit or structure loses commands, selection, targeting, weapons,
  vision and economic/energy function in the same authoritative tick. Builder
  assignments and entity-local command/navigation state are released rather
  than surviving into the wreck state; Operations Capacity and energy domains
  recalculate immediately.
- Standard unit wrecks remain blocking for the canonical 1.25 seconds and
  retain nonblocking cosmetic debris until 8 seconds total. Huge/Massive units
  block for 2.5 seconds and retain debris until 12 seconds total. Structure
  rubble blocks its authored footprint for 4 seconds, then clears pathfinding
  while leaving persistent cosmetic rubble. Cosmetic debris is presentation-
  only and never restores gameplay identity or collision.
- Snapshot v16 / simulation protocol v14 preserve mid-collapse owner/content,
  footprint, building anchor and authoritative timer state while retaining the
  supported v2-v15 readers. Replay remains v6 because the deterministic F8
  destruction helper reuses the existing command envelope encoding.
- The first T045 human handoff exposed that the playable canonical opening has
  only Crew while the engineering scenario used by earlier tests already had a
  Chrome Crusher. It also required the game director to gather resources and
  find/reveal a building, and a selected construction site incorrectly showed a
  health bar above its progress bar. That handoff is rejected and superseded.
- The F8 panel now provides two complete deterministic fixtures. `Prepare
  construction test` supplies 5,000 Ore, starts and selects a progressing site
  and centers the camera. `Prepare T045 arena` creates/selects the player's
  Chrome Crusher and frames an explicitly damaged enemy Crew, enemy Chrome and
  destructible building. Exact `Kill test Crew`, `Kill test Chrome` and `Kill
  test building` actions replace the ambiguous first-visible-target controls.
- Construction sites now suppress their world health bar entirely while the
  independent fixed-height progress bar is active. Godot smoke asserts both
  states simultaneously. Placeholder wrecks no longer progressively squash or
  “melt”; they darken at 0 HP and transition directly to flattened cosmetic
  debris only when their authoritative blocking collision clears.
- `AGENTS.md` now contains a mandatory manual-playtest handoff gate: future
  player-facing work cannot be called ready until the exact exported opening is
  provided with all entities/resources/visibility, camera and selection setup,
  and the prepared path has been exercised from a fresh launch.
- The T045 full automated acceptance candidate is green across every
  `BLOCKING_NOW` stage: builds, 173 NUnit tests, the representative 24-mover
  movement gate, compiled content, Godot destruction smoke, 100-repeat
  determinism, replay, mid-wreck snapshot continuation, regeneration and macOS
  export. Verification summary:
  `Artifacts/Verification/20260811T102530Z-full-summary.txt`. The legacy 60-mover
  stress remains the same M2-M5 diagnostic failure and does not block T045.
- A launchable T045 debug build was produced at
  `Builds/macOS/LEGO Space RTS.app` and passed the export smoke launch. Both
  prepared paths were then exercised from fresh launches of that exported app;
  captures are `Artifacts/Screenshots/t045-exported-construction-test.png` and
  `Artifacts/Screenshots/t045-exported-destruction-arena.png`. T045 is awaiting
  only human timing/readability acceptance.

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
  it does not block the current M4 task.

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
- T049 later adds full Rock Raider Worksite-zone connectivity, overlap, merge
  and split. The minimap, full 3×4 command grid, F3 production overview,
  waiting-item drag reordering and cancellation/refund presentation
  remain later interface/economy work beyond the current prototype gates.
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
- T044 now supplies footprint-aware contact delivery, deterministic approach
  slots/facing and the canonical 12-cell direct-pursuit leash. Attack-move and
  Patrol are not yet player-facing implemented commands, so their canonical
  route leashes remain pending with those later combat-command tasks; no
  presentation-owned chase or damage behavior is introduced.
- Muzzle flash and Survey Pulse presentation currently use simple short-lived
  prototype geometry. Survey Pulse remains a visible amber sphere; Contact
  tools now use a distinct source-attached impact plate instead of implying a
  missing projectile. Final differentiated combat VFX remains later work.
- Final LEGO breakup animation, bounded hero fragments and faction-specific
  destruction VFX remain M7 T067. T045 deliberately uses readable darkened
  collapse/rubble placeholders while preserving the canonical gameplay timers.

## Explicitly rejected / do not resurrect

- universal 12-tick space-time movement scheduler as normal locomotion;
- passage coordinator;
- convoy scheduler;
- staging scheduler;
- general MAPF, CBS or cooperative A*;
- benchmark-specific movement hacks.

## Next approved development sequence

1. Human playtest acceptance for M4 T045 collapse, wreck/rubble readability and
   the 1.25 / 2.5 / 4-second collision transitions.
2. M4 T046 Repair after T045 acceptance.
