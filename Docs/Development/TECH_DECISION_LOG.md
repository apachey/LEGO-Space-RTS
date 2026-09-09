# LEGO SPACE RTS — TECHNICAL DECISION LOG

This file records implementation-level technical decisions.

It does NOT override or replace Docs/Canon/.

If an implementation decision conflicts with canonical design or architecture,
the conflict must be escalated before implementation.

---

## Current engine amendment

Godot 4.7.1-stable .NET is the current engine host.

C# remains the implementation language.

SimCore remains engine-independent and authoritative for gameplay simulation.

The previous Unity-specific host decision is superseded only at the
engine-integration layer.

---

## 2026-08-09 — Repository-local development harness

- Local automation discovers the exact Godot 4.7.1-stable .NET executable, with `GODOT_BIN` retained only as an explicit fallback.
- Project targets remain unchanged. `global.json` declares .NET 8 as the minimum SDK line and permits later installed SDK majors; local scripts opt `net8.0` tools into major runtime roll-forward when a native .NET 8 runtime is absent.
- Fast verification runs all independent stages and reports every executed `PASS`, `FAIL`, or `SKIPPED` result instead of stopping at the first defect. Full verification adds the documented Phase 10 repeated determinism, replay, snapshot, stress, content-regeneration, and macOS export checks.
- Godot/macOS integration remains authoritative on the local Mac. Pull Request CI covers only portable source/content validation and engine-independent .NET/SimCore execution.
- Repository Git hooks are opt-in through `tools/setup-git-hooks.sh`; normal implementation commits cannot include `Docs/Canon/`, and direct pushes to remote `main` require an explicit human override.
- The imported `Tools/` directory is normalized to lowercase `tools/` so the documented commands work identically on macOS and case-sensitive CI filesystems.

These are implementation workflow decisions only. They do not change game canon or authoritative simulation rules.

---

## 2026-08-09 — M2 bounded cohort reflow and Heavy-first local resolution

- Multi-unit Move commands carry a minimal per-unit `FormationIntent`: command
  cohort ID, shared anchor/heading, deterministic slot, footprint spacing,
  current column count and last reflow tick. No persistent traffic manager,
  passage coordinator or future movement scheduler is introduced.
- Authoritative snapshot format advances from v1 to v2 so active and queued
  formation intent survives save/restore and participates in state hashing.
- A cohort may reflow at most once per canonical three-second non-progress
  interval. Reflow narrows columns; an impractical exact slot may settle at the
  mover's current legal position only after it has reached the calculated
  formation envelope around the command anchor.
- Local candidate selection is deterministic Heavy-first by footprint, then
  Entity ID. A lower-priority mover evaluates sidestep/turn-around escape
  against a briefly waiting right-of-way mover; a bounded final safety pass
  cancels unsafe steps after all local choices are known.
- The M2 blocking fixture uses 24 mixed movers: two ten-unit constrained-route
  cohorts plus dedicated Small/Huge pairs for controlled opposing-friendly
  traffic and Excavatable route refresh. This preserves every Phase 09B gate
  requirement without turning the fixture into a general 12-vs-12 traffic
  scheduling benchmark.

These are implementation decisions within Movement Architecture v2 and do not
change gameplay canon.

---

## 2026-08-09 — M3 deterministic resource-node foundation

- M3 begins with dependency task T030 only. Worker job acquisition, mining
  cadence, payload carrying and delivery remain T031 rather than being folded
  into the resource-node component.
- Canonical Ore definitions use stable IDs and the Phase 04 capacities: Small
  600, Standard 900, Rich 1,350 and Deep contested seam 2,400. All use finite
  depletion; authored model thresholds expose readable full, reduced, low,
  critical and exhausted presentation states without hidden yield modifiers.
- The prototype map authors two Standard deposits per starting side for the
  canonical 1,800 safe starting Ore. Resource nodes are neutral heterogeneous
  ECS entities rather than stationary movement agents.
- Content format v3 and compiled-map format v2 add resource definitions and
  authored node spawns. Their readers retain compatibility with content v2 and
  map v1 respectively.
- Snapshot format v3 adds an ordered component mask per entity so authoritative
  snapshots can represent both movers and resource nodes. The reader retains
  M2 snapshot-v2 compatibility; resource capacity, remaining amount and model
  thresholds participate in deterministic state hashing.

These are implementation decisions within the approved M3 architecture and do
not change gameplay canon.

---

## 2026-08-09 — M3 deterministic worker harvesting and hauled-resource boundary

- T031 adds a queueable entity-targeted Harvest command. Only authoritative
  Worker entities can accept it; mixed selections leave non-workers unchanged.
- Basic Crew use the canonical 30-tick / 1.5-second Ore cadence and eight-Ore
  payload. Task phase, extraction progress, resource/receiver targets and cargo
  are ECS state rather than presentation timing.
- Outside the later Worksite-service implementation, Crew physically return
  full or final partial loads to the nearest owned HQ receiver. One starting HQ
  receiver is authored per player; no resource teleports and no player-issued
  hauler micro or persistent logistics manager is introduced.
- Receiver inventory is stored as hauled-but-unprocessed material. T031 does
  not create a spendable bank; T032 owns banking and whole-system conservation
  from finite deposits through carried, hauled and processed states.
- Snapshot format v4 / simulation protocol v2 add Worker, ResourceCarrier and
  ResourceReceiver state plus queued entity targets. Readers retain snapshot
  v2/v3 compatibility. Compiled-map format v3 adds authored starting receivers
  while retaining v1/v2 reads.
- Prototype content format v4 stores the canonical Crew extraction cadence and
  carry capacity in compiled gameplay metadata; the v2/v3 readers apply the
  same canonical baseline when loading older Worker definitions.

These are implementation decisions within the approved M3 economy and Rock
Raider resource-flow canon. They do not change gameplay canon.

---

## 2026-08-10 — M3 local resource banking and conservation boundary

- T032 models processed Ore as an authoritative local `ResourceBank` component
  on each receiver rather than a universal player wallet. This preserves the
  later Worksite connectivity and pool-split rules without implementing the
  T049 Worksite graph early.
- Banking executes in stable Entity ID order before harvesting. Material
  delivered during a tick therefore remains explicitly hauled at the receiver
  until the next tick, when the complete waiting amount moves into that local
  processed reserve.
- Canon specifies HQ emergency receiving but no separate numerical HQ
  processing cadence. T032 therefore does not invent an Ore-per-second rate;
  mature Ore Processing Plant throughput remains later approved work.
- Conservation diagnostics measure raw deposit remainder, carried payload,
  hauled receiver inventory and processed reserves as one closed quantity.
  Construction spending and reservation are excluded until T033 introduces
  their authoritative transaction boundary.
- Snapshot format v5 / simulation protocol v3 add resource-bank state.
  Snapshot v2-v4 reads remain supported; legacy receivers are upgraded with an
  empty local bank during load so they continue under current simulation rules.

These are implementation decisions within approved M3 resource-flow canon.
They do not change gameplay canon.

---

## 2026-08-10 — M3 authoritative construction placement boundary

- T033 compiles explicit footprint masks, rotation, Ore/Energy cost, build
  ticks and authored production-exit reservations for the four first-playable
  Rock Raider buildings. Rectangular prototype masks use the same data path as
  future non-rectangular structures.
- A Build command contains the final Stable Content ID, integer build-cell
  anchor, legal 90-degree orientation and candidate builders. SimCore
  revalidates builder ownership/eligibility, completed-HQ prerequisite,
  terrain/elevation, occupancy, resource access, production exit and local Ore
  affordability in deterministic Entity ID order.
- A valid order creates the Construction Site with its final Entity ID and
  immediately applies its final footprint to authoritative map topology. T034
  will progress that same entity to a completed building rather than spawning
  a replacement.
- Full Ore cost moves from one nearest eligible local reserve into the site's
  reservation. Conservation includes this reserved state. Cancelling an
  unstarted site returns the reservation in full and releases the footprint.
- Starting HQ reserves contain the canonical 500 processed Ore from Phase 04,
  so the first approved building can be placed immediately rather than making
  T033 depend on an unintended pre-build harvesting delay.
- Energy costs are compiled and retained on Construction Sites, but are not
  rejected against an Energy Domain before the scheduled T037 implementation.
  This staged enforcement keeps T033 playable without inventing a temporary
  energy wallet or implementing T037 early.
- Starting HQs now own their canonical 8x8 gameplay footprints. Crew delivery
  targets the accessible building perimeter, preserving physical hauling after
  the HQ becomes navigation-blocking.
- Snapshot format v6 / simulation protocol v4 add Building and
  ConstructionSite state plus Build command fields. Replay format v2 adds the
  same command payload; current readers retain prior snapshot/replay support.

These are implementation decisions within approved M3 construction canon.
They do not change gameplay canon.

---

## 2026-08-10 — M3 deterministic construction jobs and commitment

- T034 adds the canon-listed Builder component rather than extending Worker
  harvesting state into a second responsibility. A Builder owns one active
  Construction Site target and may queue additional Construct orders in the
  existing per-unit deterministic command queue.
- Construction begins only when an assigned Crew reaches the accessible edge
  of the site's already-reserved footprint. One Crew contributes one work tick
  per simulation tick, matching the compiled canonical build time; additional
  eligible Crew contribute in stable Entity ID order.
- The first physical work tick commits exactly 20% of Ore (integer costs round
  upward to avoid under-commitment). The remaining 80% is consumed by an
  integer cumulative-progress formula, eliminating fractional accumulation and
  guaranteeing the full cost is consumed on the final tick.
- Started cancellation refunds all still-reserved Ore plus integer 50% of
  consumed Ore. Unstarted cancellation retains the T033 full-refund behavior.
- Completion removes ConstructionSite state and changes Building state to
  Completed on the same Entity ID. No replacement entity or presentation-only
  completion timer is introduced.
- Snapshot format v7 / simulation protocol v5 add Builder state and consumed
  construction Ore. Replay v3 recognizes contextual AssistConstruction while
  retaining v1-v2 reads; snapshot readers retain v2-v6 compatibility.

These are implementation decisions within approved M3 construction canon.
They do not change gameplay canon.

---

## 2026-08-10 — M3 deterministic production queues and spawn boundary

- T035 adds one fixed-capacity authoritative `Production` component per
  completed producer. Its eight queue slots retain Stable Unit ID, local
  funding reserve, Ore commitment, Energy/Crystal requirements, reserved OC
  metadata and tick progress without introducing an unordered collection.
- Queue acceptance withdraws the full Ore cost from the nearest eligible owned
  local reserve in deterministic distance/Entity-ID order. Operations Capacity
  and Energy requirements are retained but not enforced before their scheduled
  T036 and T037 systems.
- Only the active first item advances. Completion validates the producer's
  compiled exit footprint, checks deterministic candidate positions and
  creates a short-lived reservation within the production-system tick before
  spawning the entity. A blocked completion remains at zero ticks in the
  facility and retries without speculative entity creation.
- Produced entities use compiled movement, footprint, selection, vision and
  Worker/Builder metadata. A legal rally becomes a normal authoritative Move
  command; a produced Crew rallied directly to an Ore node enters the existing
  authoritative harvesting flow.
- Snapshot format v8 widens the component mask to 32 bits and adds Production
  state. Snapshot v2-v7 readers remain supported and attach empty queues to
  legacy completed producers. Simulation protocol v6, replay v4 and prototype
  content v6 carry the corresponding command and production-definition data.

These are implementation decisions within approved M3 production canon. They
do not change gameplay canon.

---

## 2026-08-10 — M3 derived Operations Capacity and canonical opening

- T036 stores canonical OC cost on compiled mobile-unit definitions and OC
  provision on compiled building definitions. Production metadata must match
  its produced unit's OC value, preventing queue and active-state disagreement.
- Per-player `active / reserved / maximum` is a derived authoritative cache.
  Stable Entity ID iteration sums active units, all fixed production-queue
  reservations and completed provider buildings, then clamps maximum to the
  canonical competitive ceiling of 100.
- Production re-derives OC before each order, so several commands accepted in
  one tick observe earlier reservations. A rejected over-cap order does not
  withdraw Ore. Completion naturally changes reserved OC into active OC because
  the queue item is removed only after the new entity is created.
- Derived OC is not duplicated in snapshot bytes. Snapshot v8 already preserves
  every authoritative input — entity content identity, ownership, buildings and
  production queues — and recalculates the cache after load. This keeps protocol
  v6 compatible while ensuring deterministic restore and continuation.
- The M2 DEV map retains its authored 18/8 mixed movement cohorts for headless
  engineering gates. The player-facing Godot composition now uses a separate
  scenario construction path over the same compiled map geometry/resources,
  replacing only the opening mobile roster with the approved canonical six Crew
  per player. Each starting HQ therefore presents the canonical `6 / 16 OC`.

These are implementation decisions within approved M3 Operations Capacity and
opening-roster canon. They do not change gameplay canon.

---

## 2026-08-10 — M3 deterministic Energy Domain foundation

- T037 adds authoritative Energy Domain and domain-membership components.
  Starting HQs root separate domains with the canonical 120 stored Energy,
  150 reserve capacity and +2 E/s auxiliary generation. Construction Sites
  inherit the funding HQ's domain; a completed expansion HQ becomes a new
  independent domain. T049 remains responsible for later Worksite-zone graph
  overlap, merge and split behavior.
- Compiled building metadata supplies generation, reserve capacity and
  continuous demand. Domain totals are recalculated only on topology/content
  events such as initialization and building completion; the normal 20 Hz
  system updates only cached domains. A deterministic raw fixed-point remainder
  makes every 20 ticks equal the exact integer E/s rate.
- Construction now reserves its full one-time Energy cost with Ore. Physical
  work commits the same canonical 20% initial share and progressive 80%; cancel
  refunds all unspent Energy plus half of consumed Energy. Production withdraws
  its one-time Energy cost before accepting a queue item and leaves Ore untouched
  when Energy is insufficient.
- A deficit drains reserve to zero, but T037 does not disable structures.
  Deterministic priority grouping and powered/unpowered effects remain the
  separately scheduled T038 Brownout task.
- Snapshot format v9 / simulation protocol v7 persist domain reserve,
  membership, cached flow and construction Energy commitment while retaining
  v2-v8 reads. Prototype content v8 adds canonical Rock Raider generation,
  reserve-capacity and demand values.

These are implementation decisions within approved M3 Energy canon. They do
not change gameplay canon.

---

## 2026-08-10 — M3 deterministic Brownout ordering and state

- T038 adds a per-consumer authoritative `PowerState` with persistent
  High / Normal / Low priority. Completed consumers are ordered first by their
  compiled canonical functional class, then player priority, then Entity ID.
  Each consumer is either fully powered or disabled; no fractional allocation
  or presentation-owned shutdown decision is permitted.
- Recalculation is event-driven: domain topology/metrics, reserve crossing zero,
  refunds and priority commands recalculate the powered set. The normal 20 Hz
  Energy tick consumes only cached powered demand during Brownout. A transition
  revision and event kind change once when the Brownout or powered set changes,
  preventing repeated per-tick alert events.
- An unpowered Vehicle Service Bay retains its production queue and exact tick
  progress; an unpowered Ore Processing Plant retains delivered pending Ore
  until processing resumes. Field units remain outside the building-power
  component and therefore remain controllable as required by canon.
- Snapshot format v10 / simulation protocol v8 add powered state and Brownout
  domain fields. Replay v5 and command serialization add authoritative priority
  changes plus a development-only reserve-drain command. Prototype content v9
  carries the canonical functional classes. Existing snapshot v2-v9 and replay
  v1-v4 reads remain supported.

These are implementation decisions within approved M3 Brownout canon. They do
not change gameplay canon.

---

## 2026-08-10 — M3 Basic HUD hierarchy and developer-tool separation

- T039 introduces the first player-facing Phase 07/08 HUD skeleton in the
  Godot presentation layer: top resources/OC and one bottom selection/state
  area. It does not move gameplay authority out of SimCore or add a second
  source of economic truth.
- The compact Energy display uses aggregate reserve, capacity, generation,
  demand and net rate. Its popover exposes the current prototype domain without
  turning permanent HUD space into a domain spreadsheet. Domain-count and
  segmentation presentation remains dependent on the later T049 topology.
- Energy priority is deliberately contextual. Its controls are instantiated in
  the selected-building status panel and hidden unless the selection contains
  an owned Energy consumer. Brownout alerts describe the domain-level cause;
  individual building state remains local. This preserves the canon requirement
  that normal priority adjustment is rare rather than routine macro labor.
- Existing engineering metrics, navigation overlays and destructive Brownout
  trigger remain available through a hidden F8 developer panel. All developer
  overlays default off. Normal Move feedback is a small unnumbered translucent
  ring with a bounded lifetime rather than a numbered marker or permanent path.
  They no longer compete visually with the economy display or represent
  intended player interaction frequency.
- The separate permanent Production panel was removed after human HUD review.
  A portrait/view slot is reserved inside the unified selection area, while
  production actions and the facility queue appear contextually only when the
  relevant building is selected. This retains the implemented M3 interaction
  without implying that production deserves permanent global screen space. The
  contextual column retains its width while hidden. Human review clarified
  that the residual HQ-selection defect is vertical growth of the bottom panel
  beyond its intended bounds rather than horizontal reflow; it remains accepted
  non-blocking UI polish and is not considered fixed by the width reservation.
- The fixed-size Label3D construction percentage is rejected: Godot scales that
  mode as if the camera were one world unit away, producing a giant blurred
  billboard in the strategic camera. World presentation uses a compact bar;
  exact progress remains in the selection panel. Dedicated construction visual
  smoke protects this boundary.
- Explored fog must eventually render per-player last-known resource/structure
  records, not current hidden SimCore state. The present stateless presentation
  snapshot continues to omit those entities until a separately reviewed,
  serialized knowledge model is implemented with multiplayer fog filtering.
- T039 does not pre-implement the later minimap, complete 3×4 command grid, F3
  production drawer, full queue editing or final faction art. Runtime smoke and
  static validation instead protect the Basic HUD anchors and hidden developer
  boundary appropriate to this milestone.

These are implementation decisions within approved M3 HUD and UX canon. They
do not change gameplay canon.

---

## 2026-08-10 — M4 deterministic targeting authority and priority order

- T040 adds authoritative `Targetable` and `Targeting` components. Target class,
  ground/true-air layer, semantic role flags, legal target masks, acquisition
  radius and current target live in SimCore; Godot only performs visible-object
  hit testing and presents the resulting target ring.
- Acquisition runs after visibility/LoS in the locked 20 Hz system order and
  queries the existing deterministic spatial buckets. Candidates must be alive,
  visible, hostile and legal for the attacker's layer/class masks. Canonical
  role priority is resolved before distance, with Entity ID as the final tie.
- Direct Attack is a queueable versioned command. It overrides automatic
  priority while legal and is cleared on target loss or loss of shared-player
  visibility, preventing exact through-fog tracking. T040 does not invent
  weapon firing, damage or a temporary chase authority; those remain scheduled
  M4 systems.
- Snapshot format v11 / simulation protocol v9 add the two combat components;
  replay v6 admits Attack commands and queued Attack orders; prototype content
  v10 carries canonical first-playable target metadata. Existing supported
  snapshot and replay readers remain intact.

These are implementation decisions within approved M4 targeting and combat UX
canon. They do not change gameplay canon.

---

## 2026-08-10 — M4 authoritative weapon readiness and firing events

- T041 adds compiled `WeaponDefinition` records and per-entity authoritative
  `WeaponState`. The state records the stable weapon profile, remaining cooldown,
  monotonic firing sequence and last fired target/tick. Godot consumes the
  sequence for a short muzzle flash but never authorizes firing.
- Weapons execute after T040 targeting in stable Entity ID order. Each 20 Hz
  step advances an active cooldown, then a ready weapon revalidates target
  legality, shared visibility, exact fixed-point range, minimum range and combat
  LoS before emitting a firing event and starting the full canonical cooldown.
  Readiness is retained while no eligible in-range target exists.
- Prototype content v11 defines the canonical first-playable weapon profiles:
  Crew 24 ticks, Hover Scout 30, Loader Dozer 27 and Chrome Crusher 32. Damage,
  type and delivery metadata are compiled now for later common resolvers, but
  T041 applies no HP change. The built-in catalog uses the compiler's canonical
  stable-key ordering so identical built-in and compiled definitions produce
  the same compatibility hash.
- Snapshot format v12 / simulation protocol v10 persist weapon readiness and
  firing history while retaining supported v2-v11 readers. Replay remains v6
  because no command representation changed.
- T041 does not create projectile records, resolve damage/armor, implement
  contact approach slots/facing/moving-contact rules or add combat chase. Those
  remain T042-T044 and cannot be inferred from the presentation flash.
- The game director explicitly deferred rather than accepted the human T040
  interaction gate so automated T041 work could proceed on a stacked branch.
  Both human gates remain pending before T042 begins.

These are implementation decisions within approved M4 weapon and combat-system
canon. They do not change gameplay canon.

---

## 2026-08-11 — M4 shared transformation foundation and roster rollout boundary

- Phase 03/06 canon distinguishes player-facing tactical transformations from
  deployment, Mission Refit, automatic machinery and visual-only folding. The
  canonical state-change roster currently includes MX-41 Switch Fighter, Solar
  Explorer, MT-201 Ultra-Drill Walker, ETX Alien Strike, ETX Alien Infiltrator,
  Red Planet Protector and Excavation Searcher. Physical transformability by
  itself does not create another command.
- The official LEGO instruction archive corroborates the source-set identities
  and physical models behind this roster: 7647 MX-41 Switch Fighter, 7646 ETX
  Alien Infiltrator, 7693 ETX Alien Strike, 7649 MT-201 Ultra-Drill Walker,
  7315 Solar Explorer and 7316 Excavation Searcher. Gameplay states, timings and
  roles remain governed by project canon rather than inferred from packaging.
- T048 therefore implements one reusable, data-driven authoritative state
  machine and proves it end-to-end with MX-41 instead of cloning seven unit-
  specific transform systems or prematurely adding the unimplemented roster.
  Each later unit supplies its own two mode bundles, duration, cancellation,
  target-layer and lock data while specialized consequences remain owned by
  their scheduled systems: Forward Service, siege/deployment, Surge,
  Stability/Sweep and Brace/Clamp.
- Cancellation retains the actual normalized progress already reached and
  reverses that presentation over the canonical rollback duration. It never
  changes the source mode early or visually snaps to its source height.

These decisions apply the approved Phase 03/06/07/09 transformation architecture
without changing gameplay canon.

---

## 2026-08-11 — M5 T049 deterministic Worksite graph and local-pool access

- The game director explicitly advanced the task sequence from merged M3 to
  M5 T049. This defers but does not complete or remove M4 T040-T048.
- Worksite service radius is compiled building metadata: Rock Raiders HQ is 18
  build cells and Vehicle Service Bay is 12. Completed nodes connect only when
  same-owner circular service zones overlap. The lowest Entity ID is the stable
  component root, so insertion order cannot change component identity.
- Graph recomputation is event-driven. It runs for scenario initialization and
  building lifecycle events, not every simulation tick. Cached WorksiteNode,
  WorksiteMember and WorksiteComponent state is authoritative, hashed and
  serialized.
- Processed Ore remains in its physical HQ ResourceBank. Accessibility is a
  component query rather than a global wallet. Orders may debit several local
  banks within one connected component, choosing the nearest funding endpoint
  and then stable Entity-ID order. Cancellation refunds return to that funding
  endpoint; disconnected components are never combined.
- Energy Domain identity follows Worksite component identity. Merge adds the
  prior component reserves. Split apportions raw fixed-point reserve by the
  surviving local reserve capacity in deterministic root order, assigning the
  integer remainder to the last stable component. This conserves represented
  Energy exactly while keeping local generators/capacity with their buildings.
- The exact capacity of Phase 04's promised short local buffer for a component
  with no surviving reserve-capacity structure is not canonically specified.
  T049 does not invent that balance value; it remains a required decision before
  later destruction gameplay can create that edge state.
- The Basic HUD chooses the selected structure's Worksite when possible and
  otherwise the first stable player component. It shows component-local Ore and
  Energy plus total component count, while SimCore remains authoritative.
- Snapshot v11 / simulation protocol v9 / replay v6 add Worksite topology.
  Prototype content schema and binary format v10 add service radius with legacy
  defaults for prior compiled content.

These decisions implement existing Phase 04/09 Worksite canon. They do not
change gameplay canon.

---

## 2026-08-11 — M5 T050 authored Excavatable topology boundary

- Each authored Excavatable Feature now has a stable key, canonical terrain
  class, required-Energy metadata, visual-profile ID, local navigation mask and
  explicit post-opening buildability. The prototype feature is a 25-Energy
  Fractured Rock Wall whose opened route remains non-buildable; this preserves
  Phase 05's rule that excavation and buildability are orthogonal layers.
- The dense MapGrid remains the authoritative pathing/LoS raster, while a
  deterministic ECS Excavatable component provides the feature's authoritative
  gameplay identity and Blocked/ActiveExcavation/Open state. Scenario creation
  binds map features to entities in ascending authored feature-ID order.
- Opening is one-way and idempotent. A successful transition removes the
  Excavatable, Impassable and GroundOccluder flags, increments topology exactly
  once, rebuilds only the affected HPA clusters plus their existing one-cluster
  neighbor halo, and invalidates only spatially affected route corridors.
  Opened ground has no faction ownership restriction.
- T050 implements the scheduled topology/local-rebuild slice. It deliberately
  does not invent per-machine progress rates or an exact excavation duration
  within canon's 15–35 / 30–60 second ranges. The existing F9/debug command is
  retained as the deterministic completion trigger until the separately scoped
  excavation execution/order layer has approved exact timing and eligibility
  data.
- Snapshot v12 / simulation protocol v10 / replay v7 add authoritative feature
  components and full map-feature metadata. Compiled map/source schema v4 adds
  the same metadata while readers retain legacy compiled maps and snapshots.

These decisions implement existing Phase 05/09 excavation-topology canon. They
do not change gameplay canon.

---

## 2026-08-15 — M4/M5 integration format boundary

- The independently developed and accepted M4 and M5 branches both used
  snapshot format 19 / simulation protocol 17 for different binary layouts.
  The combined implementation therefore writes snapshot format 20 / protocol
  18 and replay format 15.
- Snapshot format 20 uses a 64-bit entity-component mask so the complete M4
  combat/transport/transformation state and M5 worksite/excavation/refit/
  resonance state have non-overlapping authoritative bits.
- The reader preserves the merged M4 snapshot 19 / protocol 17 layout as the
  public predecessor. The unmerged, branch-only M5 format-19 layout and replay
  versions 7–14 are intentionally not treated as compatible public formats.
- M4 command identifiers 11–15 remain stable. M5 commands receive identifiers
  16–18 in the combined protocol.
- Combined prototype content writes binary format 16 / source schema 15; the
  format-15 reader remains compatible and supplies canonical legacy Worksite
  service radii for M4-era compiled content.

This is an approved serialization/protocol integration decision. It does not
change gameplay canon.

---

## 2026-08-20 — Pre-M6 immutable endpoints and bounded formation arrival

- The game director approved the StarCraft-like command model in which one
  multi-unit Move creates a legal endpoint cloud and assigns every selected
  unit one immutable final endpoint immediately.
- Canonical role bands remain intact. Within a role band, deterministic
  minimum-total-distance matching chooses endpoints with Entity ID as the final
  tie-break.
- The approved formation-arrival sequencer is limited to the destination zone.
  Later rows may use deterministic temporary route goals until earlier rows
  complete, but `NavigationAgent.Target` remains the final endpoint and is never
  replaced by a staging or current-position value.
- Temporary arrival goals are derived from serialized FormationIntent every
  tick. They add no persistent manager state and require no snapshot, protocol,
  replay or content-format change.
- This decision does not approve a portal-flow controller, passage scheduler,
  direct authoritative position pushing, or an external movement dependency.
- The sequencer restores the honest representative 24-mover M2 scenario to
  PASS, but it does not satisfy the separate legal 60-mover mid-route traffic
  gate. That remaining architecture is not approved by this entry.

This is an approved bounded movement-architecture decision. It does not change
gameplay canon.

---

## 2026-08-20 — Pre-M6 bounded local-pressure experiment rejected

- After the legal 60-mover fixture isolated a mid-route traffic/yield failure,
  the game director approved one final clean-room experiment inside the existing
  local-separation candidate scorer.
- The experiment added no persistent state or subsystem: stuck movers weighted
  existing legal candidates by nearby friendly pressure and could use at most a
  two-times local corridor-deviation window. Normal movement integration,
  terrain legality and Heavy priority remained authoritative.
- Release build, focused movement regressions and the honest representative M2
  acceptance remained green, but first-phase stress completion stayed 4/60.
  Deadlocks increased from 43 to 47 and oscillations from 3,504 to 12,831.
- The experiment was rejected and removed. It is not an accepted architecture
  and must not be resurrected by tuning weights or widening the corridor again.
- Canon at the time made the preserved 60-mover stress blocking before **M6
  networked 1v1 acceptance**, not before M6 implementation started. The
  verified endpoint/arrival work could therefore be integrated and M6
  development begin. The later M6 closeout entry supersedes that milestone
  assignment.
- A further traffic-coordination architecture still requires separate approval.
  Accepting M6 while the stress remains red would instead require an explicit
  canon amendment to the gate classification.

This records a rejected technical experiment and the resulting stop boundary.
It does not change gameplay canon.

---

## 2026-08-20 — M6 T058 Godot ENet raw-packet carrier

- Phase 09A replaces T058's obsolete Unity Transport wording with a Godot
  packet carrier. Godot 4.7.1 `ENetMultiplayerPeer` is used directly for raw
  packets; the project does not use node RPCs as gameplay authority.
- The T058 server carrier accepts at most two clients. The two-connection limit
  is configured at ENet host creation and new connections are refused while
  both slots are occupied.
- Three logical channels preserve the canonical transport separation:
  reliable ordered for future commands/control, unreliable sequenced for future
  regular snapshots and reliable bulk for future manifests/reconnect payloads.
  T058 carries opaque byte arrays only and assigns no gameplay meaning to them.
- The normal verification harness starts the production dedicated host on a
  loopback UDP port, connects two ENet clients, validates distinct peer IDs and
  exchanges targeted packets through every logical channel.
- T059 owns command decoding/validation/authority. T060 owns snapshot cadence,
  deltas and acknowledgment baselines. Neither is implemented by this carrier.

This implements the approved Godot transport boundary without changing
gameplay canon or SimCore command semantics.

---

## 2026-08-20 — M6 T059 project-owned server command authority

- A versioned project-owned command protocol now rides T058's reliable-ordered
  ENet channel. It defines a server session welcome, bounded command request and
  deterministic accepted/rejected acknowledgment; ENet still has no gameplay
  authority and T060 snapshot packets remain separate.
- The server assigns the first two peers distinct player slots and
  cryptographically random nonzero session tokens. Client-supplied player slots
  and execution ticks are never authoritative.
- A fresh session begins at client sequence 1 and accepts exactly the next
  sequence. Authenticated rejected commands consume their sequence, duplicates
  cannot execute twice, and a new session token resets the sequence boundary.
- Security rate limiting is bounded at 64 authenticated command attempts per
  authoritative simulation tick per session. This is a transport-abuse ceiling,
  not a gameplay balance rule.
- Validation rejects malformed/version-incompatible/oversized packets, debug
  commands, unsorted or duplicate Entity IDs, missing or foreign entities,
  ineligible command sources, hidden or illegal targets and the implemented
  resource, Energy/Charge, technology, state, placement and Forward Service
  failures. Authoritative gameplay systems still revalidate mutable legality
  when the accepted command executes.
- A valid request is reconstructed with the bound server player slot and the
  next legal 20-Hz simulation tick before entering the existing deterministic
  `CommandBuffer`. Offline `CommandEnvelope`, snapshot format 20, simulation
  protocol 18, replay format 15 and content format 16 remain unchanged.
- The dedicated-server entry now loads compiled authoritative gameplay data and
  advances SimCore without rendering. The normal verification harness exercises
  the real carrier with two clients, three accepted commands and six rejected
  forgery/replay/malformed/debug cases.

This implements approved T059 authority and security requirements. It changes
no gameplay canon and deliberately leaves state replication to T060.

---

## 2026-08-20 — M6 T060–T061 acknowledged snapshots and recipient knowledge

- The authoritative 20-Hz host publishes state every two ticks on the
  unreliable-sequenced channel. Each client acknowledges the newest applied
  snapshot on reliable-ordered transport; the server only delta-encodes against
  a retained, explicitly acknowledged baseline.
- Structural deltas contain stable-ID entity/projectile upserts and removals.
  Client reconstruction retains a bounded baseline history, so a delta remains
  valid when its acknowledged baseline is older than the latest rendered frame.
- Replication is presentation-only and cannot recreate `SimulationWorld`.
  Recipient snapshots include legal visible presentation, recipient fog bitsets
  and only that player's economy/capacity/charge, production queues, unit orders,
  Energy Domains, Worksites and Tube-network summaries. Deflate compression from
  the standard .NET library keeps the initial two-player smoke payload below
  ENet's MTU without a new dependency.
- Hidden enemy entities are absent rather than marked hidden. Loss of visibility
  is a removal transition; last-known presentation remains client-side. Entity
  references embedded in visible weapon/repair presentation are cleared when
  their target is neither owned nor currently visible to the recipient.
- Fixed-point interpolation helpers expose position and shortest-arc orientation
  sampling without changing authoritative simulation state.

This implements the canonical 10-Hz delta and no-hidden-data boundary. It does
not change fog gameplay, simulation authority or gameplay canon.

---

## 2026-08-20 — M6 T062 retained-session reconnect

- A disconnected authenticated session is retained for 60 authoritative seconds
  with its cryptographic token, player slot and last processed command sequence.
  Reserved slots cannot be claimed by a fresh session during that window.
- Reconnect requires exact simulation protocol, gameplay content and initial map
  hashes. A valid new peer is rebound to the retained identity; invalid, expired
  or mismatched requests receive no authoritative state.
- The server responds on reliable-bulk transport with a current full legal
  recipient snapshot plus the player's pending accepted commands, unit order
  queues and production queues. Regular acknowledged snapshot streaming then
  resumes from a new full baseline.
- The loopback acceptance disconnects player 0 while player 1 remains connected,
  restores sequence 1 on a replacement ENet peer and proves sequence 2 executes
  exactly once after the rebind.

This adds reconnect state restoration without host migration, gameplay changes
or hidden-state disclosure.

---

## 2026-08-20 — M6 T063 post-match authoritative network replay

- The dedicated server records only commands accepted by T059 authority, along
  with the initial authoritative snapshot, gameplay/map hashes and deterministic
  seed field. Development state hashes are stored every 20 ticks and full seek
  snapshots every 200 ticks.
- Replay format 16 adds ordered hash/seek checkpoints and the authoritative final
  tick/hash while retaining a tested reader for replay format 15.
- Playback can start from the nearest seek checkpoint, verifies its snapshot
  hash, replays later commands and stops immediately on a scheduled hash mismatch.
- Authoritative replay bytes are unavailable during an active match so replay
  delivery cannot bypass T061 fog filtering. After explicit match completion,
  an authenticated client may request the finalized log on reliable-bulk
  transport.
- Replays are divided into independently bounded 48-KiB chunks with transfer
  metadata and a whole-file deterministic hash. The client assembler accepts
  out-of-order chunks, rejects inconsistent metadata and publishes bytes only
  after the complete hash verifies.

This implements server-log playback and seekability without changing gameplay
canon or the authoritative 20-Hz simulation.

---

## 2026-08-20 — M6 accepted; 60-mover scale gate deferred to M9

- The game director accepted the complete verified M6 T058–T063 network stack.
- The preserved Stress60 failure is isolated to large-scale mid-route friendly
  traffic/yield behavior. It does not invalidate the green representative
  24-mover movement gate or the real two-client M6 transport, command, snapshot,
  privacy, reconnect and replay acceptances.
- Stress60 remains unchanged and continuously visible as
  `BLOCKING_LATER — M9 large-battle acceptance` during M7–M8. The verification
  mode that promotes it to blocking is `./tools/verify.sh --m9-acceptance`.
- This matches M9's canonical stable-large-battle exit and Phase 09B's rule that
  one torture fixture must not silently force a broad architecture rewrite.
- Any earlier third movement-architecture attempt still requires explicit
  architecture review; a catastrophic regression in accepted normal movement
  remains blocking immediately.

This is an explicitly approved technical-canon gate reclassification. It does
not change gameplay, movement behavior, thresholds or performance targets.

---

## 2026-08-20 — M7 T064 Godot-native LEGO material masters

- The Unity/URP task label is implemented on the approved Godot host as a small
  centralized `StandardMaterial3D` family: molded polymer, tool metal, rubber,
  transparent polymer, Crystal and terrain.
- Built-in Godot PBR is the baseline because it already provides the required
  albedo, metallic, roughness, transparency and emission controls. A custom
  shader is deferred until a measured visual or performance need justifies it.
- Faction body colors are parameters. Team identity remains a separate small
  Identification Tile material rather than a body recolor.
- A fixed presentation-only Material Lab displays all six families,
  five source-palette swatches and the complete canonical polymer roughness
  range under one fixed neutral lighting rig. Its automated smoke validates
  configuration, not subjective art quality.
- The existing prototype terrain and placeholder entity materials now consume
  the same centralized family. SimCore, content formats, gameplay and network
  state remain unchanged.

The exact palette match, highlight width, dark-value lift, transparency and
emission remain game-director art-direction gates before T064 is accepted.

---

## 2026-08-20 — Reopen visual art direction and replace generated comparisons

- The game director explicitly suspended Phase 08 rendering, material,
  lighting, palette and general visual-style authority. The document remains a
  historical/source-research reference; no replacement visual canon exists
  during M7 exploration.
- The first T064 Material Lab remains useful engineering infrastructure but is
  not accepted as the art-direction method or final material system.
- Image-generated style comparisons are rejected as representative evidence
  because they did not preserve scene invariants, used infeasible detail and
  collapsed distinct prompts into near-identical results.
- M7 now uses one code-native Style Lab: the same optimized Blender test model,
  animation, camera and composition under genuinely different Godot materials,
  shaders, light rigs, effects and terrain treatments.
- Palette ratios and transparent-material semantics are reviewed separately.
  Saturated transparent elements are non-emissive unless their authored
  function is energy, lighting or a luminous resource.

This changes presentation direction only. SimCore, gameplay, networking,
formats, Phase 07 functional UX and deterministic architecture are unchanged.

---

## 2026-08-21 — Palette massing correction and explicit faction light language

- Inventory part counts are rejected as a color-ratio proxy. Palette candidates
  must be weighted by assembled visible surface area; small pins and connectors
  cannot outweigh large hull panels.
- Rock Raiders earth brown is restored as a major 18% vehicle surface. The 7316
  Excavation Searcher is corrected to a dominant 42% tan/beige surface.
- Two identical abstract 3D carriers use exactly 100 equal visible panels per
  model so one panel equals one percentage point and color massing can be
  reviewed on an object rather than only in bars.
- Emission is role-bound, not hue-bound. The implemented light map is Rock
  Raiders neon orange/lime; Mars Mission astronauts blue; Mars Mission aliens
  neon lime; Life on Mars astronauts red plus warm light through clear lenses;
  and Life on Mars Martians red/orange/lime/blue.
- The first four-style review selected no direction. Material realism and
  graphic toon remain weak candidates; clean PBR and hand-painted/retro are
  retained only as comparison anchors.

This is non-canonical art-direction exploration. No visual style or palette is
locked, and gameplay, SimCore and network architecture remain unchanged.

---

## 2026-08-22 — Research-driven M7 Style Lab round 2

- Round 1's clean PBR, material realism, graphic toon and hand-painted/retro
  categories are retired as forward candidates after selecting no direction.
- The game director's new visual research is stored as a non-canonical
  development brief. It centers the search on volumetric mechanical 3D,
  readable broad forms, visible LEGO construction logic and clean RTS-scale
  presentation without photoreal or tabletop premises.
- One invariant Raider carrier now compares Industrial Mass, Heroic RTS,
  Constructive LEGO and Graphic Volume. Geometry, camera, animation, semantic
  palette and timing remain fixed; only materials, shaders, lighting, terrain
  and VFX rendering vary.
- The carrier uses the accepted Rock Raiders massing and semantic light roles:
  earth brown is structural, orange/lime signals are emissive, and canopy glass
  is not emissive.
- All four treatments passed the real Godot renderer, full regression and direct
  exported-build capture. This makes them valid review evidence, not accepted
  art direction.

This is non-canonical presentation research. It changes no gameplay, SimCore,
networking, format or deterministic architecture, and it locks no visual style.

---

## 2026-08-22 — Separate outline from M7 rendering direction

- First Round 2 review currently favors Heroic RTS without accepting it. The
  useful ingredients are Industrial color depth, Heroic color/bloom and
  Constructive molded highlights.
- Overt warm key lighting is rejected. Industrial is moved close to neutral and
  Heroic retains only a restrained warm key against its cool fill.
- Outline is no longer bundled with Graphic Volume. It is an independent,
  off-by-default presentation toggle available on every treatment through `O`
  or `--m7-outline off|on`.
- Automated smoke covers all four styles with outline off and Heroic RTS with
  outline on. Exported-build captures provide the same-style A/B evidence.

This is non-canonical presentation refinement. It locks no rendering style and
changes no gameplay, SimCore, networking, formats or deterministic behavior.

---

## 2026-08-22 — Replace preset-style selection with a realtime Look Profile

- Bundled style variants are retired as the primary art-direction workflow.
  They did not let the game director isolate material, light, post, outline and
  VFX decisions or judge them in a representative gameplay composition.
- The new M7 Look Lab is presentation-only and uses the Phase 07 camera contract:
  36-degree perspective, 24–72 build-cell zoom and the normal 44-cell default.
- One continuous `schemaVersion: 1` JSON profile owns independent camera, scene,
  shading, material-family, glass, emission, lighting, post, outline, VFX,
  ground and HUD parameters. Copy exports the full state, never a diff.
- Material identity is semantic rather than a recolor: painted shell,
  structural earth, accent, dark mechanisms, tool steel, rubber, building shell
  and ground rock have separate physical responses.
- Outline uses Forward+ depth and normal/roughness buffers. The old inverted-hull
  pass is retired; silhouettes and creases are controlled independently, and
  rough terrain is excluded from crease noise.
- The legacy Style, Palette and Material labs remain preserved. The Palette Lab
  stays accepted; no rendering profile becomes canon without explicit
  game-director approval.

This is non-canonical presentation tooling. It adds no dependency and changes no
gameplay, SimCore authority, networking, format or deterministic behavior.

---

## 2026-08-22 — Repair the Look Lab around observable controls

- The game director's schema-1 profile becomes the schema-2 review baseline,
  except for a bright-blue background selected while that control was not
  observable. Schema 1 remains paste-compatible and migrates forward.
- HUD, health, selection and target fixtures are removed because interface art
  direction is a separate phase. The arbitrary scorch overlay is removed rather
  than polished without an accepted damage language.
- Every emissive role now writes HDR emission and may drive same-hue edge/halo
  and local-light response. Tracer and fire use the same observable energy
  language; muzzle/impact/fire move to GPU particles.
- Film grain and posterization dither are static. Ground tracks use depth-tested
  tread geometry. Impact position is derived from the target's exterior face.
- Four generated grayscale detail maps are accepted only as non-canonical
  look-development inputs for paint, brushed metal, rubber and quarry ground.
  They do not establish a production texture or normal/ORM pipeline.
- A live ground normal-strength control is not shipped after two materially
  different shader implementations produced invalid terrain rendering. A later
  normal/ORM pipeline requires a separately reviewed implementation task.

This changes presentation tooling only. It changes no gameplay, SimCore,
networking, serialization format, dependency or visual canon.

---

## 2026-08-22 — T065/T066 shared animation drivers and bounded VFX pools

- Godot presentation now owns a reusable animation-state driver and mechanical
  rig binding. Inputs are immutable presentation state; output controls wheel,
  suspension, functional tool, recoil, transformation and damage presentation
  only and never writes authoritative state.
- Animation parameter significance follows the preserved architecture: Tier A
  every render frame, Tier B approximately 30 Hz and Tier C approximately 15
  Hz. Transform interpolation and entity placement remain independent.
- Weapon-fire effects use a per-source monotonic high-watermark so repeated
  snapshots, reconnect restore or presentation rebuild do not replay already
  observed cosmetic events.
- The Godot host prewarms fixed-capacity generic pools. Projectile views,
  particle muzzle bursts and contact impacts are reused; budget overflow drops
  only cosmetic presentation and records telemetry.
- The M7 Look Lab consumes the same implementations, advances to schema 3 and
  exposes independent animation response, tier override, VFX budget, preview
  load and live pool telemetry controls. Schema 1 and 2 remain paste-compatible.

This implements T065/T066 presentation infrastructure without a new dependency,
gameplay change, SimCore mutation, network-format change or visual-canon choice.

## 2026-08-22 — T068 retained HUD frame and separate art-direction profile

- Phase 09A supersedes the old Unity `UI Toolkit` name with Godot
  `Control`/container UI in C#. T068 implements the intended architecture, not
  the superseded host technology.
- Production HUD state is projected into a one-way `HudFrame`. A retained
  `HudView` owns layout and widgets and only updates when the frame signature
  changes; it does not rebuild the visual tree per render frame.
- Mixed selections are bounded to eight pooled type cards and commands to the
  canonical 3×4 grid. The implementation never creates one elaborate portrait
  per selected entity.
- The view owns the Phase 07 anchors, 96% default safe area, independent UI/text
  scaling and central-width bounds. Godot's project base remains 1920×1080 with
  `canvas_items` stretch; HUD dimensions remain presentation-only.
- HUD art direction uses its own schema-1 `M7HudProfile` and laboratory rather
  than re-entering the world-rendering Look Lab. This keeps currently open
  visual choices editable without weakening the canonical functional layout.
- T069 owns the real minimap. T068 only reserves and labels its bottom-left
  view contract. T073 still owns the complete command catalog; unavailable
  target-mode slots remain explicit rather than silently inventing commands.

This changes Godot presentation architecture only. SimCore, commands, gameplay,
networking and serialization formats are unchanged.

## 2026-08-22 — T069 client-legal retained minimap layers

- The production HUD and HUD Lab share one retained `HudMinimapView`; T069 does
  not create a separate debug renderer that could drift from player behavior.
- Static terrain is cached by topology revision. Viewer fog and bounded marker,
  line and ping frames update at the HUD's 10-Hz cadence; marker positions are
  visually interpolated while the camera viewport polygon updates every render
  frame.
- Current contacts come from the existing viewer-filtered
  `PresentationSnapshot`. Presentation memory retains only last-observed enemy
  structures and resources under explored fog. Hidden enemy mobiles and true
  air are never reconstructed from `SimulationWorld`.
- Enemy Tube memory is segment-based: adjacent route cells are learned only
  while visible, remain as known infrastructure under explored fog and are
  removed when renewed visibility disproves them. Visible enemy Surge anchors
  likewise originate only from the filtered presentation snapshot.
- Four bounded `MultiMeshInstance2D` channels encode mobile, true-air,
  structure and resource symbol shapes. This avoids a `Control`/node per marker
  while leaving all readability colors and opacities in schema-2
  `M7HudProfile`.
- Minimap left-click/drag uses the presentation camera. Right-click reuses the
  existing Move/rally commands and queued modifier after converting the pixel
  to the exact canonical build-cell center.
- Attack-move targeting and networked team pings remain T073 work because no
  current target-mode/ping command path exists. T069 does not change the command
  packet, recipient snapshot or replay format to approximate them.

This changes Godot presentation/input integration only. SimCore, gameplay
rules, network formats, replay formats and visual canon are unchanged.

## 2026-09-01 — T071 versioned wide infrastructure footprints and Crystal costs

- The canonical infrastructure roster includes a 10×8 Flight Operations Pad
  and a 9×9 Aero Tube Hangar, so the former single-`ulong` 8×8 building mask
  cannot represent all 31 entries. `BuildingDefinition` now stores low and high
  64-bit mask words and bounds the current source to 10×10 cells.
- Four canonical structures cost one Crystal. Building costs therefore now
  carry Ore, Energy and Crystals consistently with unit-production costs.
- Prototype source schema 16 requires the explicit Crystal field. The compiler
  remains able to read schema 15, defaulting missing Crystal costs to zero.
- Compiled content format 17 writes the second mask word and Crystal cost. Its
  reader remains backward-compatible with formats 2–16, treating their masks
  as the low word and their building Crystal cost as zero.
- Aero Tube Link stores its canonical base cost/time and 1×1 span archetype in
  the generic definition. Its existing authoritative Tube system continues to
  apply the canonical per-length cost/time rules. The generic entity definition
  preserves the canonical 320-HP functional-span durability reference; a
  damageable placed-span realization remains later command/reference/system
  binding rather than a T071 claim. Resonance demand per committed Crystal
  likewise remains authoritative in the existing Resonance system rather than
  being flattened into static data.
- Canon does not provide final nonrectangular masks, structure sight values,
  production-exit geometry or default configurable-defense modes. T071 does
  not invent them; later M8 command/reference work may bind only values already
  established by canon.
- Importing definitions does not make the new infrastructure legally
  constructible. Until T073 supplies the complete faction/research command
  catalog, authoritative placement and network validation retain the four
  already playable M3 Rock Raiders structures. This also keeps Crystal-bearing
  infrastructure data-only until its legal command path can reserve and refund
  every canonical resource cost.

This is a backward-readable content-format evolution required to represent
existing canon. It changes no gameplay canon, snapshot/network protocol,
dependency or visual direction.

## 2026-09-01 — T072 data-only research DAG and effect-reference boundary

- The canonical Phase 04 research roster is 38 definitions: 9 Rock Raider, 10
  Astronaut, 10 Alien and 9 Martian technologies. Each definition stores its
  faction, physical source building, exact Ore/Energy/Crystal cost, 20-Hz
  research duration, categories, prerequisite groups, unlock tags, parameter
  modifiers and stable presentation/localization references.
- Prerequisite groups are ANDed; alternatives inside a group are ORed. The
  schema distinguishes completed research, owned buildings and authoritative
  state thresholds. It also preserves whether a threshold is checked at start
  or maintained while researching, which is required for the Alien four-
  committed-Crystal gate.
- Source-building bindings come from the Phase 04 Rock Raider structure column
  and technology trees together with Phase 03's faction technology roles:
  Astronaut research uses the Service & Refit Hub, Alien research uses the
  Resonance Core or Reconfiguration Dock branch, and Martian research uses the
  Routing Laboratory. The five alternatives for Integrated Expedition Command
  are exactly the five technologies listed under Phase 04 `MISSION SYSTEMS`.
- Content unlocks resolve against the imported unit/building catalog. Capability
  tags and parameter targets use explicit stable namespaces. The catalog rejects
  duplicate technologies, invalid factions/providers, unresolved or
  cross-faction content effects, malformed prerequisites and every possible
  research cycle, including edges inside an any-of group.
- Advanced Excavation Systems and Grand Network Integration retain separate
  Searcher capability tags. T073 must require both when it defines the Searcher
  production command; T072 does not incorrectly make either technology
  sufficient on its own.
- Prototype source schema 17 adds the research source. Compiled content format
  18 appends the research payload after format-17 data; formats 2–17 remain
  readable with an empty research catalog.
- T072 is intentionally data-only. It does not add research jobs, queues,
  completion state, commands, UI, network payloads or effect application, and
  it does not rewire the retained M5 proof flags. Those runtime bindings belong
  to T073 and later complete-reference closure.

This changes compiled gameplay content and its compatibility hash without
changing gameplay canon, snapshot/network/replay formats, dependencies or
visual direction.

## 2026-09-07 — T073 guarded command/action catalog checkpoint

- Prototype source schema 18 and compiled content format 19 add the complete
  31-building construction prerequisite table, all 35 canonical unit-production
  recipes and 35 stable command-family definitions. Formats 2–18 remain
  readable.
- Construction and production prerequisites use the same deterministic
  AND-of-OR representation. Runtime validation rejects missing, duplicate,
  cross-faction and cyclic building references, invalid producer/unit bindings,
  OC disagreement, malformed command codes and incomplete command catalogs.
- Public command enum values 1–18 retain their accepted byte representation.
  Values 19–35 reserve stable catalog/network identities, but `CommandEnvelope`
  intentionally rejects them until their payloads and authoritative handlers
  land. Snapshot 20, simulation protocol 18, replay 16 and command packet 1 are
  unchanged.
- The complete recipe table does not silently activate unfinished production.
  Scenario creation, construction completion, legacy snapshot hydration, local
  input and server validation retain the four accepted M3 Raider products until
  Crystal/research accounting, faction Energy domains and authored exits are
  bound.
- Missing numeric and accounting rules are isolated in
  `Docs/Development/M8_T073_BINDING_REVIEW.md` as explicit proposals, not canon.
  T073 remains open until the game director approves or amends them and the
  affected runtime commands are implemented.

This is a guarded data/compatibility checkpoint. It changes no gameplay canon,
dependency, visual direction or existing network/snapshot/replay layout.

## 2026-09-08 — T073 approved guarded runtime bindings

- The game director approved the six implementation decisions recorded in
  `M8_T073_BINDING_REVIEW.md`: cancellation accounting, 13 production exits,
  explicit excavation durations, Astronaut/Alien Energy membership, Settlement
  Station reserve and the exact Defense Node combat-pressure rule.
- Production, research and Mission Refit use one deterministic cancellation
  calculation: no-progress jobs refund fully; active jobs commit 20% of
  Ore/Energy and consume the remaining 80% linearly; cancellation returns
  unspent plus half consumed; Crystals commit at 50% progress. Existing
  Service/Refit facility-destruction handling remains separate and unchanged.
- Crystal-bearing construction now reserves its authored Crystal rather than
  receiving a free runtime path when the 31-building catalog is enabled.
- Astronaut 18-cell areas merge transitively while remaining separate from
  Forward Service. Alien membership uses nearest operational Command Core,
  distance then `EntityId`, and deterministically preserves one Resonance Core
  per Command Core. Faction topology merges and splits preserve pooled reserve.
- Settlement Station stores 150 Energy and retains Self +1 Energy/s. Defense
  Node mode changes take 120 ticks and pause until 80 ticks have elapsed since
  the most recent hostile damage dealt or received.
- Snapshot format 21 and simulation protocol 19 serialize the new job and
  faction state. Snapshot 20 remains readable; replay 16 and command packet 1
  remain unchanged.
- Focused T073 tests pass 30/30. The relevant deterministic, snapshot/replay,
  content, M5-faction and M6-network compatibility selection passes 107/107.
- The requested single full verification run passed 308/308 NUnit tests and
  every runtime/build/content/M6/M7 blocking stage. Its summary retained one
  blocking static-validation failure because the validator still expected the
  former Settlement Station reserve of 0. After aligning that guard with the
  approved 150 reserve, standalone static/source validation passes. The prior
  HUD-lab mutex teardown did not recur. Stress60 remains the known 2/60
  `BLOCKING_LATER` M9 diagnostic.

This implements approved gameplay canon without a dependency, visual-direction
change or T074 work.

## 2026-09-08 — T074 complete roster-reference gate

- The shipping content compiler now runs one aggregate closure gate after both
  the canonical catalog and authored map are compiled. Generic and legacy
  catalogs remain readable; strict 35-unit/31-building/38-research/35-command
  counts apply only to the shipping M8 build path.
- The gate validates cross-section stable-ID uniqueness, faction and roster
  completeness, movement/weapon/presentation/localization references,
  production coverage, the approved producer exits, exact Aero Tube eligibility
  and authored map starts/spawns/resources/features.
- Runtime systems and the retained M5 acceptance fixture now share canonical
  T070 roster IDs instead of older faction abbreviations. The engineering-only
  synthetic M5 target IDs remain local fixture identities and do not represent
  shipping roster entries.
- Eight intentionally broken fixtures cover duplicate IDs, missing visuals,
  invalid weapons, research cycles, missing producers, illegal Tube units, bad
  starts and impossible producer exits with explicit diagnostics.
- Targeted roster/M5 coverage passes 57/57. Full verification passes 317/317
  tests and every T074-relevant runtime, content, network and export stage, with
  one known Godot HUD-process mutex failure after that fixture printed PASS;
  the affected and remaining HUD fixtures pass when rerun separately. Stress60
  remains the preserved 2/60 `BLOCKING_LATER` M9 diagnostic.

This changes no gameplay values, dependency, content/snapshot/network/replay
format or visual direction.

## 2026-09-08 — Insert M8.5 full content and presentation production

- The game director explicitly approved a new blocking production milestone
  between M8 and M9. The previous sequence established a limited M7 vertical
  slice and complete M8 gameplay data but left the production of the full
  player-facing roster, interface art and audio without a milestone owner.
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
  initially defined M8.5 as tasks T081–T091. The 2026-09-09 Super Scout
  decision below expands the current range to T081–T092. Existing T001–T080 IDs
  remain stable; task number order does not override milestone dependency order.
- M8.5 covers production design and integrated models for all 35 units and 31
  infrastructure entries, full-roster animation/VFX/destruction, complete
  icons/cards/portraits, the M9-required frontend/menu/UI presentation, audio
  SFX/ambience and final integrated acceptance.
- M7 still owns selection and game-director acceptance of the visual production
  standard. M8 still owns functional roster data. Neither a style laboratory
  nor a data definition may be presented as a substitute for a completed
  player-facing asset.
- M9 retains T075–T080 and its AI, save/load, performance, network soak and
  Skirmish Alpha responsibilities, but cannot close with primitive full-roster
  placeholders or unresolved production presentation.
- Music, voice acting and localization recording are not made blocking by this
  decision. They require separate approved scope if added later. No dependency,
  external asset pack or service is authorized by the roadmap amendment.

## 2026-09-09 — Add M8.5 Super Scout reference-intelligence gate

- The game director explicitly added a deep, detailed preproduction research
  task for every unit and building so production does not begin from shallow
  image boards or implicit assumptions.
- Phase 09C now assigns T082 to Super Scout reference intelligence. Because no
  M8.5 task has begun and Phase 09C is still isolated on its roadmap branch,
  the former T082–T091 labels move to T083–T092 without colliding with an
  implementation or merged public format. T001–T080 remain unchanged.
- T082 produces 66 evidence-backed asset packets, multi-angle source ledgers,
  silhouette/construction/scale/animation contracts and full-roster comparison
  matrices. It explicitly identifies special source-supported mechanisms,
  variants, common misrepresentations, genericization risks and unresolved
  canon questions.
- Acceptance is outcome-based: the complete representative roster must remain
  identifiable in blind gameplay-scale silhouette review, animation intent must
  be directly buildable, and consequential unknowns must be resolved or
  escalated. Reference volume alone is not a pass.
- `Docs/Development/M85_SUPER_SCOUT_REFERENCE_INTELLIGENCE.md` is the execution
  specification and fixed packet template. Generated imagery may support later
  ideation but is not accepted as source evidence.
- The game director additionally requires an explicit anti-slop quality gate:
  every major assembly must have understood identity, function and credible
  LEGO-derived construction rather than unexplained generated detail.
- Every packet includes a buildable animation/rig contract and a
  geometry-versus-texture/material plan. Required texture sets must be generated,
  integrated and reviewed with documented channels, scale and provenance.
- Passing internal audits only makes an asset ready for director review. The
  candidate is shown in reproducible gameplay and state/animation views,
  corrected from game-director feedback and reaches PASS only after explicit
  acceptance; unresolved consequential choices are asked rather than guessed.

## 2026-09-09 — T081 production asset interchange and validation contract

- M8.5 production uses the existing Blender 4.4+ DCC, deterministic GLB export
  and Godot 4.7.1 built-in scene import. The shipping build consumes GLB and
  does not depend on Blender. No package, plugin, service or external asset
  library is added.
- The locked scale is one Blender metre to one Godot world unit and two world
  units per authoritative build cell. Blender positive Y is the authored
  forward axis and imports as Godot negative Z; roots retain identity transforms
  at ground contact or authoritative footprint centre.
- Production assets carry three explicitly authored LODs (Close, Combat and
  Strategic), semantic geometry/material names, mechanical pivots,
  presentation-only sockets and a provenance/review sidecar. LOD complexity
  must strictly decrease without losing gameplay-scale identity or function.
- The first round trip exposed and corrected an initially inverted authored
  forward convention before roster production. The non-roster technical
  vehicle now proves 868/332/168 triangles, six pivots, six sockets, semantic
  material binding, exact 24/44/72-cell Godot views and byte-identical Blender
  regeneration.
- This fixture validates the production path only. It cannot count toward the
  35-unit roster, replace T082 evidence, or satisfy T083/T085 model production.
  After accepting its scale, orientation and LOD progression, the game director
  requested an orbitable review and then corrected the wheel pivots from two
  shared midpoints to four individual wheel centres. The corrected fixture was
  explicitly accepted on 2026-09-09, completing T081 and unblocking T082.

This changes no gameplay value, authoritative simulation, public data format,
dependency or accepted M7 visual direction.

## 2026-09-09 — Start T082 with a roster-locked identity/source baseline

- T082 begins from the authoritative runtime roster rather than a hand-copied
  art list. The first corpus pass contains exactly 35 unit and 31 infrastructure
  stable IDs and validates faction, footprint and canonical source
  classification against `Content/PrototypeEntities.json`.
- Every asset now has a generated A–I packet with its source family, silhouette
  thesis and three non-removable identity anchors. Packets remain explicitly
  `HOLD` until multi-angle evidence, construction, animation, texture and
  cross-roster confusion work is complete.
- A shared 39-source ledger records official instruction and archival inventory
  references with retrieval confidence and rights notes. Reference images are
  not redistributed in the repository.
- The generated form is intentional: one structured baseline produces all 66
  review packets, the identity/source matrix and the first 31-pair confusion
  register, while validation prevents roster drift or a partially generated
  corpus from looking complete. Each confusion pair records exactly three
  required visible differences.

This is a research/preproduction implementation decision only. It changes no
gameplay, simulation, public data format, dependency, canon or accepted visual
direction, and it does not approve any T083/T085 production design.

## 2026-09-09 — Make official instruction PDFs the traceable T082 evidence unit

- Source confidence now depends on locating a direct official LEGO PDF rather
  than merely loading the set landing page. The shared index records every
  exact PDF URL while keeping missing older/promotional sources explicit.
- This recovered direct primary manuals for seven sources that had remained
  archival in the initial pass, including the 4990 Rock Raiders HQ. The corpus
  now has 35 direct-PDF sources and four archival gaps.
- Faction-deep visual audits record the PDF page count and hash,
  construction-critical page ranges, orthogonal-view/mechanism coverage,
  source-supported findings and missing evidence. Temporary PDFs and rendered
  contact sheets are not redistributed.
- Rock Raiders is the first completed source-family audit: seven manuals are
  visually audited and 1277/4930 remain explicit gaps. Manual evidence proves
  source construction and mechanism only; it does not silently approve the
  adaptation boundary of any game asset.

This changes no gameplay, simulation, public data format, dependency, canon or
accepted visual direction. All affected asset packets remain `HOLD`.

## 2026-09-09 — Record Astronaut source-derived construction and motion boundaries

- All 18 mapped Astronaut sources are now visually audited across 23 official
  instruction books. The evidence records exact hashes, page ranges, view
  coverage, construction findings and remaining adaptation gaps without
  redistributing the manuals.
- Evidence is keyed by both faction and source set. Mixed playsets often contain
  opposing human and Alien models under one set number, so a shared set ID alone
  must never inject one side's construction findings into the other side's
  asset packet.
- The audit preserves the source-line split instead of averaging every asset
  into generic white science fiction: Field Systems remain exposed, rugged and
  modular, while Mission Systems retain cleaner white/orange carrier, gantry,
  tool and articulated-machine construction.
- A manual's rebuild sequence proves geometry and attachment intent, but does
  not automatically define an in-game animation. In particular, the MX-41
  cockpit hull is manually separated during the source transformation; its
  production transform must maintain a credible continuous mechanical
  connection while preserving the verified ground and flight silhouettes.
- Similar boundaries remain explicit for the MX-71 payload release, MT-201
  deployment/gait, MT-51 attack cycle and MT-101 contact drilling. Their motion
  contracts will be completed later in T082 and may communicate, but never
  determine, authoritative gameplay timing.

This is a source-traceability and production-handoff decision only. It changes
no gameplay, simulation, public data format, dependency, canon or accepted
visual direction. All affected packets remain `HOLD`.

## 2026-09-09 — Non-intrusive macOS Godot automation

- Routine Godot verification is permanently headless and disables Godot's
  in-process crash handler. It must not open the editor or game window.
- Godot 4.7.1 intermittently aborted during native macOS teardown after a smoke
  fixture had already printed PASS, producing distracting system crash dialogs
  and false host-process failures. Disposable smoke runs now preserve their
  real result code and exit directly only after the fixture has completed and
  reported PASS/FAIL, bypassing that teardown path.
- The direct exit is limited to macOS headless runs, explicit automated capture
  runs, and smoke runners. Normal playable/editor sessions still perform normal
  Godot shutdown, so real player-session failures are not concealed.
- A real renderer is required to create viewport screenshots; Godot's headless
  dummy renderer cannot produce them. Visible capture scripts are therefore
  excluded from routine automated verification and may run only when new visual
  evidence is materially required and announced to the user.

This changes no gameplay, deterministic simulation, production rendering,
public data format or dependency.
