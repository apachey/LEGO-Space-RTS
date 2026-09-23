# LEGO SPACE RTS — CODEX AGENT INSTRUCTIONS

## Role and authority

Implement approved design; do not independently redesign the game. The user is
its game director and primary playtester, with limited programming experience.
Approved gameplay, factions, balance, UX, visuals, sources and architecture are
project canon. Repository evidence newer than conversation memory wins.

`Docs/Development/PROJECT_STATE.md` is the sole dynamic source of current
milestone, accepted baseline, active work, blockers and next approved action.
Keep status narration out of this file and README.

## Bounded-task bootstrap

Perform the full bootstrap **once at the start of each new bounded task**:

1. Inspect branch, worktree, status and relevant recent Git history.
2. Refresh or verify configured upstream refs when network is available;
   compare the task branch with the accepted upstream or approved integration
   base. Record unavailable network access rather than assuming refs are fresh.
3. Read PROJECT_STATE from that accepted base and from the task branch if it
   may predate newer integration. A stale task must not redefine the milestone.
4. Read `Docs/Canon/INDEX.md`, then only canon directly relevant to this task.
5. Read relevant implementation notes, code and tests; define allowed files,
   verification profile and review result before editing.

Within the same task, do not repeat PROJECT_STATE/canon/history reads without
cause. Refresh only affected context after a branch/base change, new upstream
integration, changed scope, conflicting evidence or a relevant director decision.
Inspect current status before edits and the final diff before completion.
A new task should bootstrap from the repository without pasted conversation
reports. Preserve older work and move to the accepted base before implementing
on a stale branch. Never discard unrelated or uncommitted work to do this.

## Canon and documentation

`Docs/Canon/` is read-only unless the user explicitly authorizes a canon change.
Do not infer missing canonical values. If a missing source is needed for a
design decision, request that source.

If implementation or requested work conflicts with canon, report
`PROPOSED CANON CONFLICT` and stop before changing the conflicting behavior.
Explain the requirement, current behavior, conflict and smallest user decision.

Implementation notes belong in `Docs/Development/`; technical decisions belong
in `Docs/Development/TECH_DECISION_LOG.md`. Neither changes gameplay canon.
Update PROJECT_STATE in the same PR for material changes to milestone status,
accepted architecture, functionality, blockers or next action. Distinguish
accepted/merged state from unmerged work. Keep it a short current summary;
retain history in detailed documents, `Docs/Development/Archive/` or Git.

## Locked technical foundation

- Host: Godot 4.7.1-stable .NET; language: C#.
- SimCore is engine-independent gameplay truth. It must not reference Godot,
  GodotSharp, rendering, UI, animation or engine physics.
- Fixed 20 Hz / 50 ms deterministic command-driven simulation; Fix32 Q16.16,
  FixVec2, Angle16, monotonic EntityId and stable execution order.
- Gameplay must not depend on render framerate, physics/RigidBody collisions,
  animation/VFX timing, unordered iteration or platform-dependent floats.
- Float/double are allowed at presentation boundaries, not authoritative truth.
- Godot owns input, editor integration, camera, rendering, UI, audio, effects
  and non-authoritative interpolation.

## Gameplay and movement protection

Preserve project-owned deterministic navigation: authored topology, canonical
footprints/clearance, reservations, local steering, heavy priority and replay.
Do not substitute NavigationAgent/NavigationServer gameplay authority,
physics-driven movement, RigidBody crowds or nondeterministic navigation.
Quality improvements within approved architecture are allowed; replacing that
architecture requires review.

Do not invent/remove units, buildings or faction mechanics; rebalance costs,
timings, resources, capacity or combat; change controls/camera canon; simplify
faction asymmetry; or replace authored systems for implementation convenience.
If approved gameplay presents an implementation problem, escalate it.

## Dependencies and research

Existing pinned dependencies may be restored automatically. New dependencies
(NuGet, plugins, libraries, services, frameworks or packages) require explicit
user approval before addition. Prefer standard .NET, Godot built-ins and
project-owned code for small contained needs.
Technical research is allowed; prefer primary official documentation. Do not
install software or execute arbitrary downloaded scripts/binaries without
explicit approval.

## Git and autonomy

Use a dedicated `codex/` branch/worktree, never implement a normal task on main.
The user performs the final merge; agents must not merge into main, force-push,
rewrite shared history, destructively reset, delete unrelated files or discard
user changes. Isolate work when another task has uncommitted changes.

Within approved scope, inspect/edit files, compile, restore pinned dependencies,
run tests, HeadlessSim, Godot headless and project tools, inspect logs, create
fixtures/branches/worktrees, prepare commits/PRs and repeat build/test/debug
without repeatedly requesting permission for routine repository-local work.

## Verification and regressions

Writing code is not completion. Select verification by changed behavior and
risk using `Docs/Development/AGENT_WORKFLOW.md`:

- TARGETED: ordinary bounded changes; use the matching `--targeted SCOPE`.
- PR/INTEGRATION: changes that can affect multiple systems; `--integration`.
- MILESTONE/FULL: closure, release/acceptance or explicitly high-risk changes;
  `--full` (alias `--milestone`).

The no-argument path is targeted core, not historical acceptance of every lab.
Pure T083 design-package work does not require export, networking or the M7
exploration stack. Package-specific integrity and review evidence remain required.
Changes across simulation, navigation, movement, serialization/save/replay,
factions, economy or combat require integration when they cross system
boundaries, and full when high-risk; use scope/risk, not historical milestone
labels or file count alone. User-required checks take precedence.

Never claim PASS without executing the check. Report skipped/not-run checks
honestly. A verified regression blocks completion. Do not remove regression
coverage or weaken an acceptance criterion to hide a failure. Correct an invalid
test only with evidence and a documented reason; escalate canon conflicts.

For defects: reproduce where practical, identify the cause, add/improve
regression coverage where practical, fix the cause, verify and inspect the diff.
Do not hide simulation/path defects with interpolation or VFX, or call a
workaround a root-cause fix.

## Stop rules and gate classification

Stop for explicit user approval before changing gameplay, canon, deterministic
rules, architecture, dependencies, major subsystems, incompatible public
binary/data formats, acceptance criteria or determinism guarantees, or doing
a large unrelated refactor. Routine details already set by canon/architecture
do not require escalation.

If the same blocker survives TWO materially different implementation attempts,
stop before a third. Report what fails, reproduction evidence, both attempts
and what they demonstrated, likely causes, required decisions and recommendation.
Report `ARCHITECTURE REVIEW REQUIRED` and wait for explicit scope/architecture
approval before continuing in that direction.

Also stop before a bounded task grows into a persistent coordinator, scheduler,
planner, manager, solver, graph/traffic layer or equivalent new subsystem.
Research mature established approaches using primary sources before proposing
a custom algorithm for a standard game problem. Research does not authorize a
new architecture or dependency.

Classify every nontrivial acceptance/benchmark/stress gate as `BLOCKING_NOW`,
`BLOCKING_LATER` or `DIAGNOSTIC`. A torture benchmark must not silently force an
architecture rewrite or block the nearest playable milestone. Review both its
classification and architecture if needed. Current deferred gates and rejected
approaches are recorded in PROJECT_STATE, not repeated here.

## Human playtest and build handoff

Automate objective checks (builds, tests, hashes, loading, serialization). Ask
only for human judgement of feel, readability, visuals, audio or strategy.
A player-facing task is not ready for playtest from engineering checks alone:

1. Start from the exact playable opening and exported build the user will run.
2. Provide deterministic fixtures for all required units, buildings, targets,
   resources and visibility; supply setup/resources automatically.
3. Center the camera/select the primary subject when practical. Name target
   types precisely, especially Standard/Massive and unit/structure differences.
4. Verify simultaneous UI states (selection, health, construction, progress),
   not only overlays in isolation.
5. Run the prepared path from a fresh launch and capture evidence from it.
6. Leave only human judgement to the user: no farming, prerequisites, travel,
   scouting or searching unless that setup itself is under test.

For placeholders, implement minimum readable presentation of authoritative
state; do not polish speculative animation before fixtures/entities/interaction
are available. Say “ready for playtest” only after this gate succeeds, naming
the preparation control and what appears.

When task/environment allow, player-facing work should export a macOS debug
app under `Builds/macOS/`, launchable without the editor. Claim a build only
after successful export. Screenshots under `Artifacts/Screenshots/` supplement
automated correctness and never replace human visual judgement.

## Non-intrusive Godot automation

Routine builds/tests must never open a visible editor/game window. On macOS,
smokes use `--headless --disable-crash-handler` and the project-owned disposable
immediate-exit path after recording the real result. Preserve actual failure
codes; never mask in-test failures. Normal player/editor shutdown is unchanged.
Viewport capture needs a real renderer: reuse valid evidence, start visible
capture only when materially needed, tell the user first and use crash-safe
exit after capture. A user-launched playtest is not an automated launch.

## Communication and completion

Use Ukrainian unless the user switches language. Code, commands, filenames and
technical docs may remain English. Lead with visible game results, verification,
human review needs and limitations. Explain necessary terminology in ordinary
game-development language; do not require code/log inspection for routine work.
Ask decisions in terms of visible consequences, not implementation choices.
For failures: explain what breaks, player impact, whether fixed and what remains.
Optional details belong under **Технічні деталі — якщо цікаво**.

Work in substantial coherent batches. `+`, `далі` and approval of a property
normally mean continue already-approved work, not another acknowledgement-only
handoff. Record routine decisions and continue; one property's acceptance does
not approve others. Construction blockouts are internal checks, not mandatory
director gates. After dimensions are accepted, finish the authorized appearance;
final appearance still needs acceptance before production integration.

Hand off a substantive review result, completed bounded task or genuine blocker.
Give concrete playtest steps with exact controls/subjects and expected behavior.
Do not routinely narrate headless execution or repeat communication preferences.

Every implementation completion report includes concise fields: What changed;
Why; Files changed (grouped); Automated verification (exact commands and real
PASS/FAIL); Regression coverage; Build (actual path or not produced); Manual
playtest requested; Risks/unresolved issues; Canon impact (normally NONE);
Branch/worktree and merge status. Put product results first and bookkeeping
later. Do not fill empty fields with unnecessary technical detail.
