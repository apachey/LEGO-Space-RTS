# Pre-M6 movement reuse audit

Status: **research complete; bounded prototypes evaluated; M6 may start;
architecture review is required only before further movement-architecture
expansion**. No external dependency or third-party source code has been added
or approved.

## Decision

Do not replace the project-owned deterministic movement stack and do not add a
runtime navigation dependency for the pre-M6 gate.

The useful reusable result is a set of established design patterns, not a
drop-in library:

1. assign a unique legal destination to every selected mover when the command
   is accepted;
2. choose those assignments spatially, while retaining canonical role-aware
   formation preference;
3. keep long-range planning, local steering, collision resolution and arrival
   as separate concerns;
4. use bounded deterministic recovery instead of a global future reservation
   schedule;
5. validate stress timing against the slowest commanded mover before treating
   completion as an implementation gate.

This is compatible with Movement Architecture v2 and does not require a canon
rewrite.

## What StarCraft II establishes publicly

The primary public source is the Blizzard portion of the 2011 GDC session
[AI Navigation: It's Not a Solved Problem - Yet](https://www.gdcvault.com/play/1014515/AI-Navigation-It-s-Not).
The public material supports a layered design: planning, steering and bounded
collision handling are separate problems. A detailed community transcript
summary of the talk records constrained-Delaunay navmesh planning with A* and
funnel-style path construction, short path lookahead, Boids-derived steering,
arrival steering, and capped pushing. Most importantly for this task, it
records that a moving group gives every unit its own destination and never
allows two units to path to the same location:
[Game Development Stack Exchange summary](https://gamedev.stackexchange.com/questions/191954/how-can-i-create-the-arriving-engaging-in-combat-movement-like-in-starcraft-2).

The exact StarCraft II source and its destination-assignment algorithm are not
public. We therefore should not claim that it uses our proposed matching
algorithm. The defensible conclusion is narrower: the game treats unique
per-unit endpoints and arrival behavior as first-class parts of group movement,
rather than sending the whole selection to one shared point.

The current SimCore already creates a group of final points. Its missing piece
is assignment quality: endpoints are allocated by hard role/entity order, then
each point is repaired independently near terrain. That can create crossing
routes, duplicate repaired targets, and an arrival wall even though the initial
idea is correct.

## Open-source candidates inspected

The repositories were inspected at the listed commits. No code was copied.

| Candidate | Relevant behavior | License / compatibility | Decision |
| --- | --- | --- | --- |
| [0 A.D. `61a3b95`](https://github.com/0ad/0ad/tree/61a3b9507d974084e6badb88a0826bd89a6d5b8b) | Formation controller builds footprint-spaced offsets, groups units by class, assigns the nearest compatible slot, switches long moves to a narrow column, and uses fixed-point weight/pressure-based pushing. | GPL-2.0-or-later. Its persistent formation controller is also more rigid than the canonical command cohort. | Study only. Clean-room adapt nearest compatible assignment, column/reflow concepts, and bounded weight-aware pressure where useful. |
| [OpenRA `a520984`](https://github.com/OpenRA/OpenRA/tree/a520984d91eda9de48a62b1d15c1e3bad0d4fb1a) | Deterministic cell/subcell occupancy; progressively relaxes blocker classes; waits, notifies a blocker, repaths, or backs out of a blocking cell. Its own source notes local avoidance still needs improvement. | GPL-3.0-or-later. Cell occupancy and engine traits are not a drop-in fit. | Study only. Reuse the recovery sequence as a bounded behavioral reference, not source. |
| [RVO2-CS `a455da2`](https://github.com/snape/RVO2-CS/tree/a455da254cffd9ebb8d85f8eedb9d9332e69012a) | Mature ORCA local collision avoidance in C#. | Apache-2.0, but authoritative math is `float`, updates use parallel workers, responsibility is symmetric, and it provides neither pathfinding nor group endpoint assignment. A Fix32/heavy-priority port would be a new solver, not reuse. | Do not add. Reconsider only through separate dependency/architecture approval. |
| [Recast/Detour `9f4ce64`](https://github.com/recastnavigation/recastnavigation/tree/9f4ce64458dfae86e1239c525ddc219c4e9e06f1) | DetourCrowd combines navmesh corridors, local steering and obstacle avoidance. Its own documentation says the crowd owns agent positions, uses local targets, and is relatively expensive at roughly 20–30 managed agents. | zlib, but C++/float and would replace project-owned authoritative navigation. | Do not add. Architecture mismatch is decisive despite the permissive license. |
| [Recoil/Spring `9cda653`](https://github.com/beyond-all-reason/RecoilEngine/tree/9cda6533686f784fcba0ccf9ed42a746a7ee7181) | Large engine-specific stack with shared/partial shared paths, dynamic repath, footprint layers, collision rules, waypoint skipping and traffic-jam fallbacks. | GPL-2.0-or-later, C++/float, deeply coupled to the engine. | Study only. Useful evidence that shared paths and arrival exceptions are optimizations, not a standalone mover. |

### Concrete source observations

- 0 A.D.'s
  [`Formation.js`](https://github.com/0ad/0ad/blob/61a3b9507d974084e6badb88a0826bd89a6d5b8b/binaries/data/mods/public/simulation/components/Formation.js)
  separates class ordering from nearest physical slot selection. Its
  [`CCmpUnitMotion_System.cpp`](https://github.com/0ad/0ad/blob/61a3b9507d974084e6badb88a0826bd89a6d5b8b/source/simulation2/components/CCmpUnitMotion_System.cpp)
  uses fixed-point weight ratios and pushing pressure, but explicitly describes
  knowing when to path around an unpushable obstacle as the hard problem.
- OpenRA's
  [`Move.cs`](https://github.com/OpenRA/OpenRA/blob/a520984d91eda9de48a62b1d15c1e3bad0d4fb1a/OpenRA.Mods.Common/Activities/Move/Move.cs)
  searches with `All`, `Stationary`, `Immovable`, then `None` blocker policies,
  and applies wait/repath/unblock fallbacks. This is robust recovery, not smooth
  StarCraft-style crowd flow.
- RVO2-CS's
  [`Agent.cs`](https://github.com/snape/RVO2-CS/blob/a455da254cffd9ebb8d85f8eedb9d9332e69012a/RVOCS/Agent.cs)
  assigns each pair half of the avoidance correction and solves float linear
  programs; its
  [`Simulator.cs`](https://github.com/snape/RVO2-CS/blob/a455da254cffd9ebb8d85f8eedb9d9332e69012a/RVOCS/Simulator.cs)
  performs parallel float updates.
- Detour's
  [`DetourCrowd.h`](https://github.com/recastnavigation/recastnavigation/blob/9f4ce64458dfae86e1239c525ddc219c4e9e06f1/DetourCrowd/Include/DetourCrowd.h)
  documents the ownership, local-target and crowd-size limitations directly.
- Recoil's
  [`GroundMoveType.cpp`](https://github.com/beyond-all-reason/RecoilEngine/blob/9cda6533686f784fcba0ccf9ed42a746a7ee7181/rts/Sim/MoveTypes/GroundMoveType.cpp)
  contains explicit traffic-jam and blocked-waypoint exceptions, while
  [`PathManager.cpp`](https://github.com/beyond-all-reason/RecoilEngine/blob/9cda6533686f784fcba0ccf9ed42a746a7ee7181/rts/Sim/Path/QTPFS/PathManager.cpp)
  shows how much infrastructure is required for shared and dynamically refreshed
  paths.

## Baseline and fixture findings

The clean post-M5 baseline was run with:

```text
dotnet --roll-forward Major HeadlessSim/bin/Release/net8.0/HeadlessSim.dll \
  --scenario stress60 --ticks 3000 --benchmark --path-benchmark \
  --enforce-performance-gates
```

Observed result:

- completion: 31/60 (51.67%);
- deadlock diagnostics: 82;
- oscillation incidents: 8,483;
- command-to-motion maximum: 4 ticks;
- realtime multiplier: 4.00x on this local run.

The command-to-motion gate is already healthy. Completion, deadlock,
oscillation and performance are not.

The existing stress schedule is also not physically feasible:

- movers start between x=20..38 and y=58..68;
- the first target is (138,80) at tick 1;
- the next Move replaces it at tick 700;
- even the closest spawn is more than 100 build cells away;
- the fastest member can cover at most 2.25 * (699 / 20) = 78.64 build cells,
  before acceleration and path detours are considered.

The fixture therefore interrupts a reachable order before any member can
physically finish it, while its tests describe that period as completing the
first command. Mixed Huge movers are slower still. This violates the canon's
explicit travel-time-feasibility requirement and makes the current completion
signal ambiguous.

Fixing the fixture is not weakening the gate. The 60-mover scenario remains
`BLOCKING_LATER — M6 networked 1v1 acceptance`; each phase must receive a
justified generous deadline, and the fixture must assert that its timing is
feasible for the slowest member.

## Preserved pre-M5 work: salvage decision

The old `codex/60-mover-fix` worktree remains preserved and unmodified. It is
not mergeable onto post-M5 because it predates the accepted M5 integration and
overlaps core simulation files.

Safe ideas to reimplement selectively on the post-M5 base:

- deterministic collective legal slot-cloud generation;
- deterministic minimum-cost one-to-one slot assignment;
- focused tests for non-overlapping legal final endpoints and stable tie-breaks;
- bounded diagnostics that distinguish an intentional wait from a true stall.

Do not transplant as-is:

- the persistent portal-flow controller;
- row staging and repeated arrival reassignment;
- cohort-wide slowest-speed throttling;
- direct soft-push displacement of authoritative positions;
- hard-coded 7,000/14,000/22,000 timing without a feasibility calculation;
- any change that treats a current position as successful arrival merely to
  increase completion.

Those experiments expanded into interacting movement subsystems and still did
not satisfy the gate. They are evidence, not production code.

## Post-prototype evidence

The first clean-room production experiment implemented the game director's
proposed model on the integrated post-M5 base:

- one deterministic legal endpoint cloud is created for the whole selection;
- every selected mover receives exactly one unique endpoint immediately;
- minimum-total-distance matching is applied within canonical role bands;
- endpoint legality, non-overlap, role preservation and deterministic tie
  behavior have focused regression coverage.

Under the old `HasTarget == false` completion check, the 26,000-tick stress run
appeared to improve from 34/60 to 60/60 in the final phase, while deadlock
diagnostics fell from 83 to 54 and oscillation incidents from 58,013 to 10,933.
That apparent completion result is invalid: inspection showed units near their
starting area being counted as arrived after formation reflow replaced their
endpoint with their current position.

The benchmark now captures each endpoint when its Move command executes and
measures physical distance to that immutable point. The production prototype
also removes the current-position completion shortcut. That honest behavior
exposes a pre-existing false positive in the representative 24-mover M2 test:

- 7/24 movers remain incomplete at the generous 5,000-tick deadline;
- every reported route is reachable and valid;
- six of the seven movers have an already-completed friendly as their nearest
  blocker;
- the failed Large/Huge movers remain roughly 5–12 build cells from their
  assigned front-line endpoints;
- their stuck counters continue into the thousands rather than recovering.

This identifies arrival ordering, not endpoint legality or long-range routing,
as the remaining root problem. Canonical Heavy/front-line units sometimes
reach the formation after faster rear-role units have already parked across
their approach.

## Two-attempt result

1. **Collective legal endpoint cloud plus spatial assignment.** This removes
   duplicate/crossed destination defects and materially reduces diagnostic
   churn, but does not by itself prevent earlier arrivals from forming a wall.
2. **A wider bounded maneuver window on only the final corridor segment.** The
   representative failure was byte-for-byte equivalent in the reported unit
   positions, blockers and stuck counts. The experiment was removed.

`ARCHITECTURE REVIEW REQUIRED`

Per the repository two-attempt rule, implementation stops here rather than
stacking the preserved portal controller, staging, speed throttle and direct
soft-push experiments.

The recommended next architecture is a narrowly scoped deterministic
formation-arrival sequencer:

- all final endpoints remain assigned and immutable from command acceptance;
- front/role rows are admitted into the destination zone before rear rows;
- waiting rows use deterministic temporary arrival waypoints outside the final
  lanes, then continue to their original endpoints;
- it operates only near the commanded destination and owns no long-range route
  or global traffic scheduling;
- it never teleports or directly displaces authoritative positions;
- Hold Position, Heavy movement priority, topology changes and deterministic
  save/replay remain protected by explicit regression coverage.

This is smaller and safer than general friendly soft-push, which would need
new persistent yield/return state and could blur Hold Position and Heavy
right-of-way semantics. It is an architecture change and therefore requires
the game director's explicit approval before implementation.

## Approved arrival prototype and legal stress result

The game director approved the bounded formation-arrival sequencer. The clean
prototype keeps `NavigationAgent.Target` as the immutable final endpoint and
derives temporary staging route goals from existing serialized formation intent;
no snapshot-format change or external dependency is required. Later rows stage
only after entering the destination zone, and the next row is released when the
earlier row completes.

Results:

- focused formation/arrival tests: PASS;
- honest representative M2 movement: PASS, restoring 24/24 completion;
- final endpoints remain unchanged while temporary route goals are active.

The subsequent 60-mover run exposed a separate fixture defect. `SpawnGrid`
placed all mixed footprints two build cells apart, while Huge collision diameter
is 3.2 build cells. The scenario therefore violated the canonical legal,
non-overlapping-start requirement before tick one. The fixture now uses four-cell
spacing and has an explicit regression test for passability and pairwise legal
separation. Its generous phase timing remains feasible after the correction.

First honest phase result on the corrected fixture:

- physical completion: 4/60;
- realtime multiplier: 9.11×;
- tick p99: 27.8624 ms;
- path p99: 26.3133 ms;
- path requests: 4,151;
- stuck-recovery events: 274;
- deadlock diagnostics: 43;
- oscillation incidents: 3,504.

The state capture shows four front-row members at their endpoints and the
remaining movers split into persistent clusters before/around the central
constrained passages. Many units retain valid corridors but accumulate roughly
5,000–6,600 stuck ticks. This is a mid-route local traffic/yield failure, not an
arrival assignment failure.

`ARCHITECTURE REVIEW REQUIRED`

The preserved pre-M5 branch already explored a persistent portal-flow controller
and direct authoritative soft-push as part of an unsuccessful expanded stack.
The next review should not authorize those systems again. The smallest research-
supported option is a clean-room deterministic crowd-pressure/yield improvement
inside the existing local-separation candidate scoring, drawing only on the
studied fixed-point 0 A.D. concepts. It must remain bounded, position-integrated
through normal movement, subordinate to corridor legality and Heavy priority,
and must not become a passage scheduler.

## Final bounded local-separation experiment

The game director approved that smallest option as the final local attempt. The
experiment activated only after the existing stuck threshold, calculated a
stable weight-aware separation pressure from nearby friendly movers, scored the
existing legal steering candidates toward that pressure, and allowed recovery
to use a corridor window no wider than twice the normal local deviation. It did
not add state, a subsystem, a dependency, a passage reservation, or direct
position displacement.

Verification before the stress run remained green:

- Release build: PASS with zero warnings/errors;
- local separation, Heavy priority, corridor and formation tests: 31/31 PASS;
- honest representative M2 movement: PASS (24/24).

The first legal stress phase did not improve:

| Metric | Before | With bounded pressure |
| --- | ---: | ---: |
| Physical completion | 4/60 | 4/60 |
| Path requests | 4,151 | 4,557 |
| Stuck-recovery events | 274 | 370 |
| Deadlock diagnostics | 43 | 47 |
| Oscillation incidents | 3,504 | 12,831 |
| Realtime multiplier | 9.11× | 8.94× |

The candidate therefore amplified local direction switching without resolving
the multi-unit traffic dependency. It was removed. No production code from this
failed pressure/window experiment remains.

After removing it, the repository's normal `./tools/verify.sh` run passed all
stages: 256 NUnit tests, the honest 24/24 M2 movement acceptance, both Release
and Godot Debug builds, compiled-content checks, HeadlessSim smoke and Godot
headless smoke. The unresolved failure is therefore isolated to the canonical
pre-M6 60-mover scale gate rather than a general accepted-game regression.

The subsequent 26,000-tick `./tools/verify.sh --full` run passed every current
blocking stage, including 100-repeat determinism, replay, snapshot continuation,
content regeneration and a launchable macOS export. The honestly classified
`BLOCKING_LATER — M6 acceptance` stress produced:

- phase completion: 4/60, 5/60, then 2/60;
- realtime multiplier: 16.21×;
- tick/path p99: 21.9289 / 19.2034 ms;
- path requests: 14,370;
- deadlock diagnostics: 130;
- oscillation incidents: 34,750.

The full development verification therefore passes with one visible diagnostic
failure; `--m6-acceptance` promotes the same stress to a real blocking stage.

There is no evidence-based third local-scoring patch to apply.

The exact Phase 09B language says the stress becomes blocking **before M6
networked 1v1 acceptance**. It does not say that M6 implementation cannot start.
No canon rewrite is required to integrate the verified immutable-endpoint and
arrival work, carry the documented 60-mover limitation into M6, and begin the
first bounded M6 task.

`ARCHITECTURE REVIEW REQUIRED` before any further movement attempt that adds a
traffic coordinator/solver or otherwise expands the movement architecture. If
the gate still fails when M6 acceptance is being prepared, it must either pass
through an approved movement architecture or receive an explicit canon
reclassification; merely weakening the harness remains prohibited.

## Original implementation sequence (now completed through review gate)

1. Make stress60 an honestly blocking fixture: explicit phase constants,
   slowest-member/path-length feasibility coverage, per-phase completion
   evidence, and a blocking full-verification stage.
2. Reimplement only collective legal endpoints plus deterministic spatial
   matching on the post-M5 base. Preserve role-aware shape as a preference and
   stable tie-break, but prioritize reachable progress over exact slot purity.
3. Measure the first phase before adding another movement mechanism. Report
   which units fail, where, and whether the failure is path planning, local
   steering, collision resolution or arrival.
4. If endpoint assignment alone does not clear the gate, review one bounded
   arrival/traffic mechanism using that evidence. Do not stack the old portal,
   staging, speed-throttle and soft-push experiments together.

This sequence directly tests the game director's proposed model: one click
creates a group of final points immediately, and every selected unit receives a
specific point before movement begins.
