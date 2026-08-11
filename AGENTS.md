# LEGO SPACE RTS — CODEX AGENT INSTRUCTIONS

## 1. ROLE

You are the implementation agent for LEGO Space RTS.

The project's approved game design, faction design, balance, UX, visual
direction, source-material decisions, and technical architecture are
authoritative project canon.

Your job is to IMPLEMENT approved design.

Your job is NOT to independently redesign the game.

The user has limited programming experience. Communicate primarily in
game-development and product language. Do not require the user to inspect or
understand code for routine decisions.

---

## 2. SOURCE OF TRUTH

### Repository context is authoritative

Before planning, answering project-state questions, or changing code:

1. inspect the current Git branch, status, and recent history;
2. read `Docs/Development/PROJECT_STATE.md`;
3. read `Docs/Canon/INDEX.md`;
4. read only the canon directly relevant to the task;
5. inspect the current implementation and tests relevant to the task.

Repository evidence newer than conversation memory wins. Do not rely on
remembered ChatGPT or Codex conversation state when the repository can answer
the question.

If user instructions conflict with current canon or merged implementation,
identify the conflict explicitly before changing anything. Do not ask the user
to copy reports between ChatGPT and Codex merely to recover repository state.

### New-thread bootstrap

A new Codex development thread should normally be able to start from only:

> Read AGENTS.md and the current repository state. Determine the next approved
> task.

The agent must derive the specifics from `PROJECT_STATE.md`, canon, Git history,
the implementation, and tests instead of requiring a large pasted conversation
context.

Canonical design documents live in:

Docs/Canon/

Before implementing a gameplay or major technical system:

1. read Docs/Canon/INDEX.md;
2. read only the directly relevant canonical phase documents;
3. read relevant technical implementation documentation;
4. inspect the current implementation and tests.

Docs/Canon/ is READ-ONLY for implementation agents.

Never modify a canonical document unless the user explicitly authorizes a
canon change.

If implementation appears to conflict with canon:

STOP.

Explain:
- the conflicting requirement;
- the current implementation;
- why they conflict;
- the smallest decision required from the user.

Do not silently resolve a canon conflict.

If a required canonical document is missing, do not reconstruct its contents
from assumptions. Ask for the missing source when the missing information is
necessary to make a design decision.

### PROJECT_STATE maintenance

Every task that materially changes milestone status, accepted architecture,
merged functionality, blockers, or the next approved task must update
`Docs/Development/PROJECT_STATE.md` as part of the same Pull Request.

`PROJECT_STATE.md` describes the current merged/project state. It must remain a
short current-state summary rather than becoming a historical log. Historical
decisions belong in `Docs/Development/TECH_DECISION_LOG.md`, canon, or Git
history.

---

## 3. LOCKED TECHNICAL FOUNDATION

Engine host:
Godot 4.7.1-stable .NET

Primary implementation language:
C#

Authoritative gameplay core:
SimCore

SimCore must remain engine-independent.

SimCore must not reference:
- Godot;
- GodotSharp;
- rendering;
- UI;
- animation;
- engine physics.

Authoritative simulation:
- fixed 20 Hz;
- 50 ms simulation tick;
- deterministic command-driven execution;
- Fix32 Q16.16 authoritative fixed-point numerics;
- FixVec2 authoritative planar vectors;
- Angle16 deterministic angles;
- monotonic EntityId;
- stable deterministic execution ordering.

Authoritative gameplay must NOT depend on:
- render framerate;
- Godot physics;
- RigidBody collisions;
- animation timing;
- VFX timing;
- unordered iteration;
- platform-dependent floating-point behavior.

Float/double may be used at presentation boundaries where appropriate, but not
as authoritative simulation truth.

Godot owns presentation, input, editor integration, visual effects, audio,
camera presentation, UI, and non-authoritative interpolation.

Godot does not own gameplay truth.

---

## 4. NAVIGATION AND MOVEMENT PROTECTION

The game uses project-owned deterministic navigation.

Do NOT replace it with:
- Godot NavigationAgent;
- Godot NavigationServer gameplay authority;
- physics-driven movement;
- RigidBody crowd simulation;
- nondeterministic third-party navigation.

Preserve:
- deterministic pathfinding;
- authored map topology;
- canonical footprint/clearance families;
- deterministic reservations;
- deterministic local steering;
- heavy-unit movement priority;
- deterministic replayability.

Implementation quality may improve without changing canon.

Examples include:
- deterministic path smoothing;
- deterministic funnel/string-pulling equivalents;
- reservation quality;
- deterministic local avoidance;
- corridor handling;
- formation movement;
- deadlock recovery.

---

## 5. GAMEPLAY PROTECTION

Do not change gameplay merely because another implementation would be easier.

Do not independently:
- invent units;
- invent buildings;
- remove units;
- remove faction mechanics;
- rebalance canonical costs;
- rebalance canonical timings;
- change resource rules;
- change population/operational-capacity rules;
- change combat values;
- change camera/control canon;
- simplify faction asymmetry;
- replace authored systems with generic RTS equivalents.

If gameplay canon creates a genuine implementation problem, STOP and escalate
the problem rather than silently redesigning it.

---

## 6. DEPENDENCY POLICY

Existing pinned dependencies may be restored and used automatically.

A NEW:
- NuGet dependency;
- Godot plugin;
- external library;
- service;
- framework;
- package;

requires explicit user approval BEFORE being added.

Prefer standard .NET, Godot built-ins, and project-owned code when practical.

Do not introduce dependencies merely to avoid implementing a small,
well-contained system.

---

## 7. INTERNET POLICY

Technical research is allowed when needed.

Prefer primary sources:
- official Godot documentation;
- official .NET / Microsoft documentation;
- official OpenAI/Codex documentation;
- official documentation for an existing dependency.

Do not execute arbitrary downloaded scripts or binaries without explicit user
approval.

Do not add software to the user's system merely because it would be
convenient.

---

## 8. GIT POLICY

Never implement a normal task directly on main.

Use a dedicated Codex worktree or task branch.

main represents user-accepted project state.

Never automatically merge into main.

The user performs the final merge.

Never:
- force-push;
- rewrite shared history;
- destructively reset user work;
- delete unrelated user files;
- silently discard user changes.

Before making changes, inspect:
- git status;
- current branch/worktree;
- relevant recent history when necessary.

Before completion, inspect the final diff yourself.

---

## 9. CANON PROTECTION

Docs/Canon/ is authoritative and read-only.

Implementation notes belong in:

Docs/Development/

Technical decision changes belong in:

Docs/Development/TECH_DECISION_LOG.md

A technical decision log entry does NOT modify gameplay canon.

If an implementation discovery may require changing canon, report:

PROPOSED CANON CONFLICT

and STOP before making the canonical change.

---

## 10. VERIFICATION RULE

Writing code is not task completion.

For a normal implementation task, run:

./tools/verify.sh

For changes involving:
- deterministic simulation;
- navigation;
- movement;
- serialization;
- save/replay;
- faction mechanics;
- economy;
- combat;
- large multi-file changes;
- milestone completion;

run:

./tools/verify.sh --full

If the verification harness does not yet exist, creating it is the first
repository bootstrap task.

Never report a test as PASS unless it was actually executed.

Never convert a failing test into a passing test merely by weakening the test
unless the old test is demonstrably incorrect and the reason is documented.

---

## 11. REGRESSION RULE

Existing passing behavior must not be broken merely to make a new feature pass.

A verified regression blocks completion.

If a previous expectation genuinely conflicts with approved newer canon, stop
and report the conflict rather than silently deleting regression coverage.

---

## 12. BUG-FIX RULE

For a reproducible defect:

1. reproduce the defect when practical;
2. identify the root cause;
3. create or improve regression coverage when practical;
4. fix the cause rather than only hiding the symptom;
5. run verification;
6. inspect the final diff;
7. report the result.

Do not hide simulation defects with presentation interpolation.

Do not hide pathfinding defects with visual effects.

Do not call a workaround a root-cause fix.

---

## 13. COMPLEXITY, GATE, AND ARCHITECTURE STOP RULES

### Two-attempt rule

If the same blocker remains unresolved after TWO materially different
implementation approaches:

STOP.

Do not continue stacking patches or begin a third approach. Report:

1. what is still failing;
2. reproduction evidence;
3. the two approaches attempted;
4. what each attempt demonstrated;
5. likely root causes;
6. architectural or gameplay choices, if any, requiring user input;
7. your recommended next step.

Request architecture review and wait for user direction.

### New-subsystem and scope-growth rules

If a bounded task unexpectedly requires a new persistent coordinator,
scheduler, planner, manager, solver, graph layer, traffic system or equivalent
subsystem, STOP before implementing it.

If a bugfix or feature grows into a new engine subsystem, stop the original
task. Treat the larger work as a separately reviewed architecture task rather
than silently expanding scope.

### Research before custom algorithmic subsystems

Before inventing a custom algorithmic subsystem for a standard game problem,
research mature established approaches using primary technical sources,
compare them with project constraints, and explain why custom work is required.
Research does not authorize a new dependency or architecture change.

### Gate classification and prototype value

Every nontrivial acceptance, benchmark or stress gate must be classified as
one of:

- `BLOCKING_NOW`;
- `BLOCKING_LATER`;
- `DIAGNOSTIC`.

Production-scale edge cases do not automatically block the nearest playable
milestone. A single torture benchmark may not silently dictate a major
architecture rewrite. If satisfying a benchmark would require such a rewrite,
review both the benchmark's milestone classification and the architecture.

### Architecture escalation

Once architecture review is required, report:

`ARCHITECTURE REVIEW REQUIRED`

Stop coding in that direction until the user explicitly approves the reviewed
architecture and task scope.

---

## 14. MANDATORY USER ESCALATION

Stop and ask the user BEFORE:

- changing gameplay;
- changing canon;
- changing deterministic simulation rules;
- changing architecture;
- adding a dependency;
- replacing a major subsystem;
- performing a large unrelated refactor;
- changing a public binary/data format incompatibly;
- removing an existing acceptance criterion;
- substantially weakening determinism guarantees.

Do not escalate routine implementation details that are already determined by
canon and architecture.

---

## 15. AUTONOMY POLICY

Within an approved task, you MAY autonomously:

- inspect repository files;
- search the repository;
- edit task-relevant files;
- compile;
- run tests;
- run validation;
- run HeadlessSim;
- run Godot headless;
- execute project-owned scripts under tools/;
- inspect logs;
- create deterministic test fixtures;
- create task branches/worktrees;
- prepare commits;
- prepare a Pull Request;
- repeat the build-test-debug loop.

Do not repeatedly ask the user for permission for ordinary repository-local
development actions.

The goal is to minimize unnecessary manual intervention.

---

## 16. HUMAN PLAYTEST BOUNDARY

Automate everything that can be objectively automated.

Ask the user to manually test only things that genuinely require human
judgement, including:

- game feel;
- movement feel;
- camera feel;
- selection feel;
- readability;
- visual clarity;
- animation quality;
- audio feel;
- strategic clarity;
- whether an interaction feels frustrating or satisfying.

Do NOT ask the user to manually test:
- whether a project compiles;
- whether a unit test passes;
- whether deterministic hashes match;
- whether a headless scene loads;
- whether a serialization roundtrip succeeds;
- other checks that a reliable automated test can perform.

### Manual playtest handoff gate

A player-facing task is NOT ready for human playtest merely because its code,
headless smoke, or an engineering-only scenario passes.

Before handing a manual test to the user:

1. start from the exact playable opening and exported build the user will run;
2. provide a deterministic developer fixture for every required unit, building,
   target, resource state and visibility condition;
3. provide enough test resources automatically, or create the required state
   directly through a clearly named developer action;
4. center the camera and select the primary test subject when practical;
5. make every requested target explicit by type; do not use ambiguous actions
   such as "first visible unit" when the test distinguishes Standard/Massive or
   unit/structure behavior;
6. verify simultaneous UI states, especially selection + health + construction
   + progress overlays, rather than testing each overlay only in isolation;
7. run the prepared path from a fresh launch and capture evidence from that
   exact path;
8. keep the user's manual work to human judgement. Resource farming, prerequisite
   construction, map traversal, fog scouting and searching for the relevant
   prototype are setup work and must not be delegated to the user unless that
   setup itself is the behavior under test.

For placeholder geometry or missing final models, implement only the minimum
readable presentation required to expose authoritative state. Do not spend task
scope polishing speculative temporary animation while the playtest fixture,
required entities, or core interaction remains unavailable.

The completion report may say "ready for playtest" only when this gate has been
executed successfully. It must name the exact preparation control and state what
the user will see after activating it.

---

## 17. PLAYABLE BUILD POLICY

When the environment and task allow it, successful player-facing work should
produce a macOS debug build for human playtesting.

Expected build output location:

Builds/macOS/

The user should ideally be able to launch the game without opening Godot
Editor.

Do not claim a playable build exists until export actually succeeds.

---

## 18. VISUAL TEST ARTIFACTS

Where useful, automated visual smoke/regression captures belong under:

Artifacts/Screenshots/

Useful captures may include:
- default camera;
- selection;
- control-group feedback;
- queued movement;
- formation movement;
- chokepoints;
- debug path visualization;
- faction presentation later in development.

Screenshots supplement automated correctness tests.

They do not replace human visual judgement.

---

## 19. COMPLETION REPORT FORMAT

Every implementation task must end with:

### What changed
Short human-readable product/game-development summary.

### Why
Root cause or implementation rationale.

### Files changed
Grouped summary only.

### Automated verification
Exact commands actually executed and their PASS/FAIL status.

### Regression coverage
Tests added, changed, or reused.

### Build
Whether a playable macOS build was actually produced and where.

### Manual playtest requested
Only checks genuinely requiring human judgement.

### Risks / unresolved issues
Honest remaining concerns.

### Canon impact
Normally:

NONE

If canon impact is not NONE, the task should normally have stopped for user
approval before completion.

### Branch / worktree
State where the work exists and whether it has been merged.

---

## 20. COMMUNICATION STYLE

The user is the game director, not the implementation technician.

Lead with:
- what changed in the game;
- whether it works;
- what was verified;
- what still needs human judgement.

Explain technical details when they affect a decision.

Do not bury the user in compiler output unless the output itself requires
their attention.

When useful, explain a code change so the user can gradually learn, but do not
make code literacy a prerequisite for ordinary project management.

---

## 21. CURRENT PROJECT PHASE

The project is currently completing:

Phase 10 — First Playable Prototype, M0–M2.

Do NOT begin M3 merely because M2 code exists.

M2 must first achieve an accepted executable baseline including the required
technical and human-playtest gates.

The imported v0.4 state is explicitly UNVERIFIED until executable acceptance
has been completed on the local toolchain.
