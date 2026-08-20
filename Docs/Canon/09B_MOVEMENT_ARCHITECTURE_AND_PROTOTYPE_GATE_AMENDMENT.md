# LEGO SPACE RTS — MOVEMENT ARCHITECTURE & PROTOTYPE-GATE AMENDMENT v1.0

**Phase:** 09B — Movement Architecture & Prototype-Gate Amendment  
**Status:** AUTHORITATIVE PROJECT CANON  
**Approval:** User-approved Movement Architecture v2, 2026-08-09  
**Authority:** Phase 05 — World & Map Bible; Phase 06 — Combat, Damage & Balance Framework; Phase 07 — Controls, Camera, UX & Interface; Phase 09 — Technical Architecture & Prototype Implementation Spec; Phase 09A — Godot Engine Amendment; Phase 10 executable prototype evidence.

---

# CANON STATUS

This amendment supersedes only the movement-implementation and prototype-gate decisions identified below.

It does **not** change:

- gameplay movement speeds;
- acceleration or deceleration classes;
- turn rates;
- five canonical collision-footprint families;
- Phase 06 friendly-compression limits;
- Heavy-over-light movement priority;
- role-aware formation behavior;
- map dimensions or canonical corridor widths;
- authored Excavatable topology;
- 20 Hz fixed-step authoritative simulation;
- Fix32 / deterministic authoritative numerics;
- engine-independent SimCore;
- deterministic replay and state hashing;
- custom authoritative ground navigation;
- Godot's non-authoritative presentation role;
- any faction mechanic, unit, building, economy, combat or UX canon.

The amendment exists because executable Phase 10 work demonstrated that the original Phase 09 local-movement architecture was disproportionately complex for the prototype and encouraged development of a general multi-agent traffic scheduler rather than the intended RTS movement layer.

The prototype evidence is sufficient to amend the technical architecture. The project will not continue escalating the rejected reservation/scheduler design merely to satisfy one stress fixture.

---

# 1. PREVIOUS TECHNICAL CANON SUPERSEDED

Phase 09 previously defined local movement around:

- deterministic candidate-velocity steering;
- a mandatory rolling 12-tick / 0.6-second per-agent reservation horizon;
- future-cell reservation arbitration;
- narrow-passage exchange through reservation authority;
- progressively more elaborate reservation-based deadlock recovery.

The following requirement is superseded:

> Normal unit movement must maintain a collision-free 12-tick future reservation schedule as its central local-movement authority.

The following implementations are **not** part of the canonical baseline:

- general space-time reservation scheduling for every moving unit;
- passage ownership as a universal movement mechanism;
- serialized convoy scheduling;
- staging-lane scheduling;
- full cooperative A*;
- Conflict-Based Search;
- general MAPF;
- benchmark-specific traffic coordinators.

These techniques may be reconsidered only after a separate architecture review demonstrates a concrete need that the simpler canonical movement stack cannot satisfy.

---

# 2. MOVEMENT ARCHITECTURE v2 — NORTH STAR

Ground movement is now:

# DETERMINISTIC GLOBAL ROUTING
# + PERSISTENT ROUTE CORRIDOR
# + GROUP / FORMATION INTENT
# + SIMPLE DETERMINISTIC LOCAL SEPARATION
# + BOUNDED RECOVERY

The system must optimize first for:

1. units reaching legal destinations;
2. no permanent reachable-goal stalls;
3. readable RTS group movement;
4. Heavy machinery receiving reliable right-of-way;
5. deterministic replayability;
6. acceptable CPU cost;
7. implementation simplicity sufficient for continued game development.

Perfect multi-agent future scheduling is not a goal.

---

# 3. GLOBAL ROUTING — PRESERVED

The authoritative ground-navigation foundation remains project-owned deterministic hierarchical grid navigation.

Preserve:

- 0.5-build-cell navigation resolution;
- deterministic HPA-style hierarchical routing;
- deterministic local A*;
- canonical clearance by footprint family;
- deterministic path tie-breaking;
- dynamic local topology updates when structures or Excavatable Features change;
- topology-version tracking;
- no Godot Navigation authority;
- no authoritative engine physics.

Global routing answers:

> Which legal route should this unit/group use to reach the destination?

It does not attempt to solve every future collision among moving agents.

---

# 4. PERSISTENT ROUTE CORRIDOR

A moving ground entity maintains a **persistent route corridor** derived from its deterministic global route.

The corridor is an authoritative SimCore concept.

It may be represented by:

- hierarchical portals;
- ordered navigation regions;
- bounded route segments;
- a corridor mask/window;
- or an equivalent deterministic grid-native representation.

The exact representation is an implementation decision.

The corridor must support:

- local locomotion deviating from an exact waypoint line without invalidating the whole route;
- local path straightening/smoothing;
- recovery from small steering deviations;
- topology invalidation when authored terrain changes;
- bounded local repair before an expensive global repath;
- deterministic state hashing / replay.

The corridor constrains **where progress is legal**.

It is not a reservation schedule for future occupancy.

---

# 5. GROUP / COMMAND-COHORT INTENT

A normal multi-unit Move command creates a temporary deterministic **command cohort**.

The cohort exists to share:

- destination intent;
- broad route direction;
- formation orientation;
- formation reflow context.

It is not a rigid squad entity and does not change player selection/control-group semantics.

Units retain individual Entity IDs, paths, combat states and destinations/slots.

The cohort may dissolve or split deterministically when:

- routes diverge materially;
- units become separated;
- units receive new commands;
- formation cohesion is no longer practical.

The player is not required to micro-manage cohort state.

---

# 6. FORMATION BEHAVIOR — PRESERVED AND SIMPLIFIED

Phase 06 role-aware loose formation remains canonical.

Normal role order remains:

1. short-range / Heavy frontline;
2. medium ranged;
3. anti-air distributed through body;
4. support;
5. undeployed siege;
6. separate air echelon.

Normal spacing remains approximately:

**collision diameter + 0.35 build cells**

with the existing Spread command increasing separation.

During movement, formation layout adapts to available route-corridor width.

The implementation may:

- reduce columns;
- elongate ranks;
- temporarily release a slot;
- reassign a nearby legal slot;
- reform after a choke.

A formation is not allowed to block its own progress merely to preserve perfect visual geometry.

Formation cohesion is a preference beneath path legality and forward progress.

---

# 7. LOCAL LOCOMOTION

Local locomotion converts corridor intent into immediate deterministic movement.

It uses a small bounded set of deterministic concerns:

- desired progress direction;
- canonical acceleration / deceleration;
- canonical turning;
- static collision;
- nearby friendly collision;
- nearby enemy collision;
- formation-slot preference;
- Heavy-over-light priority;
- short-range separation / yielding;
- bounded local lookahead where useful.

Neighbor processing must be deterministic, with Entity ID as final tie-break.

The algorithm must remain simple enough to reason about, profile and test.

No local-movement step may require solving a general multi-agent future schedule.

---

# 8. FRIENDLY SEPARATION / YIELD / SOFT PUSH

Phase 06 collision behavior remains authoritative:

- friendly units never permanently occupy the same space;
- friendly separation may compress by up to 15% for no more than 1.5 seconds during exchange;
- Large/Heavy movement receives priority over smaller friendly movement;
- Small friendly units automatically sidestep/yield for Heavy movement;
- enemies do not phase through each other;
- ordinary size difference does not create hostile displacement.

Movement Architecture v2 implements these rules using deterministic local resolution.

For friendly conflicts:

- higher-priority movers retain more of their desired progress;
- lower-priority movers receive stronger yield/separation correction;
- Entity ID resolves final equal-priority ties;
- legal lateral displacement is preferred to unnecessary stopping;
- bounded friendly soft-push/separation may resolve crowd pressure;
- no entity may be pushed through impassable terrain;
- no persistent illegal overlap is permitted.

"Soft push" is a friendly locomotion-resolution rule.

It is **not** a combat displacement mechanic and does not apply to enemies.

---

# 9. RESERVATIONS — NARROWED ROLE

Reservations remain canonical only where a discrete future placement must be protected.

Required reservation use cases include:

- production/structure exits;
- transport unloading;
- Aero Tube exits / arrival placement;
- other explicit spawn/placement operations that cannot legally overlap.

Normal continuous ground locomotion does **not** require the former universal 12-tick future reservation schedule.

A small deterministic local occupancy/lookahead mechanism may be used if implementation evidence justifies it, but:

- it must remain subordinate to the corridor/local-movement architecture;
- it must not evolve into a new general scheduler without architecture approval;
- it must not require passage/convoy scheduling merely to make ordinary RTS movement work.

---

# 10. STUCK RECOVERY

The existing Phase 09 progress-time anchors are retained but their actions are amended for Movement Architecture v2.

A unit that fails meaningful progress:

### After approximately 1 second
Increase local separation/yield urgency and refresh immediate locomotion intent.

### After approximately 2 seconds
Refresh/repair the local route corridor and next steering target.

### After approximately 3 seconds
Release/reassign impractical formation-slot intent and allow deterministic local reflow.

### After approximately 5 seconds
Record a path-deadlock diagnostic and request a full deterministic hierarchical repath.

A reachable unit must not remain permanently stalled because its formation slot, local steering choice or stale corridor state never changes.

A unit intentionally blocked by an actually impassable/unreachable topology is not a movement deadlock.

---

# 11. DYNAMIC TOPOLOGY

Authored dynamic topology remains a mandatory capability.

When a structure changes relevant pathing or an Excavatable Feature opens:

- affected cells update;
- affected hierarchical portals update;
- topology version changes;
- impacted route corridors validate/repair;
- only units whose corridor/path is affected need route refresh;
- the whole map is not globally rebaked.

Movement Architecture v2 must continue to support Rock Raider excavation without rebuilding navigation from scratch.

---

# 12. DETERMINISM

Movement remains fully authoritative and deterministic.

Preserve:

- Fix32 authoritative position/speed/acceleration;
- Angle16 authoritative orientation;
- stable neighbor iteration;
- stable path tie-breaking;
- stable formation/cohort ordering;
- deterministic separation/yield resolution;
- deterministic stuck recovery;
- replay/state-hash coverage.

Godot remains presentation-only for movement.

No Rigidbody, CharacterBody, NavigationAgent, engine collision callback or render-frame delta may decide gameplay movement truth.

---

# 13. EXTERNAL REFERENCE POLICY

The project may study mature navigation architectures and algorithms.

Useful architectural references include concepts such as:

- route/path corridors;
- hierarchical pathfinding;
- local steering/crowd avoidance;
- formation reflow;
- flow fields for shared group intent;
- deterministic separation.

Reference study does not authorize copying license-incompatible source code.

A third-party runtime navigation dependency requires a separate explicit dependency/architecture approval.

Movement Architecture v2 does **not** add Recast/Detour, OpenRA, Recoil, RVO/ORCA or any other external runtime dependency.

---

# 14. M2 BLOCKING MOVEMENT GATE — REPLACED

The former 60+ mover torture scenario is no longer a blocking M2 completion gate.

M2 now uses a **representative medium-army movement acceptance scenario**.

## M2 REPRESENTATIVE MOVEMENT SCENARIO

The canonical M2 blocking scenario contains:

- **24 mixed ground movers**;
- all five footprint families represented;
- at least two command cohorts;
- one 10–15-build-cell medium passage;
- one >=12-cell Heavy route;
- at least one opposing-friendly traffic exchange;
- at least one Heavy-vs-Small/Tiny right-of-way interaction;
- at least one normal formation reflow through constrained terrain;
- one authored Excavatable topology opening followed by route/corridor validation;
- legal non-overlapping starts;
- physically feasible scripted movement timing;
- no benchmark-specific special-case logic.

## M2 AUTOMATED PASS

The blocking scenario passes only if:

- all 24 reachable commanded movers complete their scripted reachable movement by the authored generous deadline;
- no mover is permanently hard-stuck on a reachable route;
- no persistent illegal friendly overlap remains;
- no enemy phasing occurs;
- Heavy right-of-way does not permanently starve lower-priority traffic;
- topology opening causes deterministic legal route refresh;
- repeated runs produce identical authoritative hashes;
- normal movement performance remains inside the existing Phase 09 normal simulation/pathfinding budgets.

The fixture must include explicit travel-time feasibility validation so it cannot demand physically impossible movement.

## M2 HUMAN PASS

Automated correctness does not replace game-feel acceptance.

Before M2 closes, a human playtest must judge:

- Move responsiveness;
- group readability;
- formation reflow;
- Heavy movement character;
- chokepoint behavior;
- absence of obvious jitter/dancing;
- selection/queue feedback;
- camera/readability.

M2 closes only when both automated and human passes are acceptable.

---

# 15. 60-MOVER STRESS GATE — RECLASSIFIED

The existing 60+ mover stress scenario is preserved.

It is not deleted or weakened.

Its classification changes from:

**BLOCKING_NOW — M2**

to:

# BLOCKING_LATER — M9 LARGE-BATTLE SCALE GATE

During M2–M8 it is:

**DIAGNOSTIC / REGRESSION-STRESS**

It must remain runnable and should trend better as movement matures.

It becomes blocking before M9 Skirmish Alpha acceptance, when the project must
prove the canonical stable-large-battle exit and substantially meet the retained
performance targets.

The stress fixture may evolve to remain physically feasible and representative, but may not be hard-coded around one implementation.

The project must not redesign an entire movement architecture solely to satisfy this single stress fixture without a separate architecture review.

---

# 16. PERFORMANCE GATE CLASSIFICATION

Existing Phase 09 simulation budgets remain targets:

- normal authoritative simulation: <=2.5 ms p95;
- stress authoritative simulation: <=4.0 ms p99;
- normal pathfinding: <=1.5 ms p95;
- stress pathfinding: <=3.0 ms p99.

For M2:

- the representative 24-mover blocking scenario must meet normal budgets;
- the 60-mover stress metrics are diagnostic unless a catastrophic regression occurs.

Before M9:

- the applicable stress budgets become blocking again.

---

# 17. DEVELOPMENT-COMPLEXITY POLICY

Technical architecture must serve game production rather than consume it.

The project adopts the following mandatory engineering policy.

## TWO-ATTEMPT RULE

After two materially different unsuccessful implementation approaches to the same blocker:

# STOP IMPLEMENTATION.

Perform architecture/root-cause review before a third approach.

A small follow-up fix to a newly identified concrete defect does not count as a materially different architecture attempt.

## NEW-SUBSYSTEM RULE

If a bugfix unexpectedly requires a new persistent:

- coordinator;
- scheduler;
- planner;
- graph layer;
- manager;
- solver;
- traffic-control system;

the implementation task must STOP before building that subsystem.

The subsystem requires deliberate architecture approval.

## RESEARCH-BEFORE-CUSTOM RULE

Before inventing a new algorithmic subsystem for a standard game-development problem:

1. review mature established approaches;
2. identify relevant engine/library/reference implementations;
3. compare them to project constraints;
4. state why a custom solution is still necessary, if it is.

## SCOPE-GROWTH RULE

If a task changes category — for example:

"fix local collision"

becoming:

"build persistent convoy scheduling"

— the original task stops.

The larger work becomes a separately approved architecture task.

## GATE-CLASSIFICATION RULE

Every nontrivial acceptance/stress test must be classified as one of:

- `BLOCKING_NOW`;
- `BLOCKING_LATER`;
- `DIAGNOSTIC`.

A new benchmark does not automatically become a current milestone blocker.

## PROTOTYPE-VALUE RULE

Prototype work prioritizes proving the nearest playable milestone.

Production-grade edge cases may remain diagnostic when they do not invalidate the milestone's actual player experience or architectural feasibility.

## NO BENCHMARK-DRIVEN ARCHITECTURE

Benchmarks test architecture.

A single torture fixture does not define architecture.

If a benchmark implies a broad architectural rewrite, review both the benchmark's milestone role and the architecture before coding.

---

# 18. PHASE 10 / M2 CONSEQUENCES

Phase 10 M2 completion now requires:

- current fast verification green;
- canonical deterministic core green;
- representative 24-mover Movement Architecture v2 blocking scenario green;
- Godot headless smoke green;
- macOS playable build produced;
- human movement/selection/camera playtest accepted.

M2 does **not** require the legacy 60-mover torture scenario to be green.

M3 Economy + Base Building may begin after the revised M2 blocking gate and human M2 playtest pass.

The 60-mover stress gate remains in the repository for later maturation.

---

# 19. PREVIOUS CANON CHANGED BY PHASE 09B

Phase 09 Parts XVIII–XIX and related M2 acceptance assumptions are amended as follows:

1. Mandatory per-agent 12-tick future reservation is removed as the central normal-locomotion architecture.
2. Persistent deterministic route corridors are added between hierarchical routing and local locomotion.
3. Normal crowd movement uses deterministic local separation/yield/soft-push rather than general space-time scheduling.
4. Reservations are narrowed primarily to discrete spawn/exit/unload placement and optional bounded local safety support.
5. Existing stuck timing anchors remain, but recovery actions are rewritten around corridor repair, formation reflow and deterministic repath.
6. The 60+ mover test is reclassified from M2 blocker to M9 large-battle blocking stress gate / M2–M8 diagnostic.
7. M2 receives a new representative 24-mover blocking scenario plus mandatory human movement-feel acceptance.
8. Development-complexity guardrails become authoritative technical policy.

No Phase 00–08 gameplay canon is changed.

Phase 09A Godot engine-host amendment remains intact.

---

# 20. CANON ESTABLISHED

The canonical movement implementation direction is now:

# HPA / LOCAL A*
# -> PERSISTENT ROUTE CORRIDOR
# -> COMMAND-COHORT / FORMATION INTENT
# -> DETERMINISTIC LOCAL SEPARATION / YIELD / FRIENDLY SOFT PUSH
# -> BOUNDED RECOVERY / REPATH

The architecture remains deterministic, fixed-point, engine-independent and compatible with future command/replay networking.

The project explicitly rejects continued prototype escalation into a general multi-agent traffic-planning engine.

**Movement Architecture v2 is locked until new executable evidence demonstrates a genuine need for another architecture review.**
