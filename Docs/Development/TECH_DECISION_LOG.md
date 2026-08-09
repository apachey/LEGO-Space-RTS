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
