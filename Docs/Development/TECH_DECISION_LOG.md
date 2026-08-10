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
