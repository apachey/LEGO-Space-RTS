# Agent-first development workflow

The goal is simple: every change is prepared away from the accepted game, checked automatically, reviewed, and then handed to the user to accept. Agents implement approved work; they do not decide new game design.

## The product workflow

1. **Task and scope** — Perform the AGENTS.md bootstrap once per bounded task: verify the accepted Git base, read PROJECT_STATE and the canon index, then only task-relevant canon, implementation and tests. Refresh affected context only for a base/scope change, new integration, conflicting evidence or a relevant director decision.
2. **Isolated worktree** — Work starts on a `codex/` task branch in its own worktree. `main` remains the user-accepted game and is never automatically merged.
3. **Implementation** — The agent changes only the approved area. Gameplay, canon, architecture, dependencies, or data-format changes that were not approved are escalated instead of guessed.
4. **Automated verification** — Select the risk-based profile below. A failing `BLOCKING_NOW` gate blocks completion. `BLOCKING_LATER` and `DIAGNOSTIC` results remain visible without solely blocking the current milestone; report skips and checks not run honestly. Profile selection changes scheduling, not acceptance requirements.
5. **Self-review** — The agent reviews the final Git diff for hidden gameplay changes, canon edits, engine leakage into SimCore, nondeterminism, generated files, dependencies, unsafe scripts, and machine-specific paths.
6. **Playable macOS build** — For a player-facing handoff when task/environment allow, `./tools/build-mac.sh` creates `Builds/macOS/LEGO Space RTS.app`. Build output is local and not committed.
7. **Human playtest** — The user is asked only for judgement automation cannot provide: movement/camera/selection feel, clarity, visual quality, audio, and strategic readability. Compilation, hashes, file round trips, and headless loading remain automated.
8. **Pull Request** — After verification and review, the task branch can be pushed and opened as a Pull Request. CI repeats the portable SimCore checks; this Mac remains authoritative for Godot and macOS integration.
9. **USER merge** — The user reviews and merges. Agents do not merge into `main`.

## Verification levels

Choose checks for the changed behavior, not the historical milestone named in a
file. Multiple matching targeted scopes may be run for a bounded task. A PR by
itself does not force full verification; cross-system impact does require the
integration profile. User-specified checks take precedence.

| Level | Use | Entry point |
|---|---|---|
| TARGETED | Ordinary bounded change, docs, one unit design package or isolated system | `--targeted SCOPE`; no arguments means `core` |
| PR/INTEGRATION | Changes that can affect several systems or their contracts | `--integration` |
| MILESTONE/FULL | Milestone closure, release/acceptance, explicitly high-risk changes | `--full` or `--milestone` |

Simulation, movement, navigation, serialization/replay, faction, economy and
combat changes need integration when they affect multiple systems, and full
when high-risk (for example broad determinism or public-format changes).
A narrow fix still needs its relevant regression test; do not use a documentation
profile for runtime changes. Explain profile choice in the handoff.

All profiles run existing static/source validation. `workflow` adds shell syntax
and profile-routing checks only. `design-package` and `references` add existing
T082 source integrity and director-review recording guards, without .NET/Godot.
For T083, also run the selected package's own regeneration, provenance/hash,
link, roster-identity and acceptance-state checks and inspect its review artifact.
The reference profile does not certify new proposals. See the
[one-unit template](M85UnitDesign/README.md). Design-only work does **not** launch
export, M6 networking or the M7 exploration stack.

Runtime scopes restore/build the solution and Godot host before their fixtures.
The full NUnit suite includes `M2MovementAcceptanceTests` through normal SDK
source inclusion, without Explicit/Ignore attributes or a test filter; the old
second filtered invocation is removed, not its regression coverage.

## Preserved gates and routing

Every listed check retains its assertions and failure handling. Classification
is `BLOCKING_NOW` within its applicable scope unless explicitly stated below.
Not selected is not the same as PASS; `--list-stages` only prints the plan.

| Existing checks | Targeted scope | Integration | Full / milestone |
|---|---|---|---|
| NUnit (including representative 24-mover), content comparison, HeadlessSim, PrototypeRTS smoke | `core` | Yes | Yes |
| Five M6 ENet transport/authority/snapshot/reconnect/replay smokes | `network` | Yes | Yes |
| T081 pipeline contract + imported asset round trip | `assets` | Yes | Yes |
| T082 identity/source integrity + review recording guards | `references`, `design-package` | Yes | Yes |
| T064 material-master smoke (existing helper now selectable) | `material` | No | Yes |
| M7 Style exploration, all four styles + independent outline | `style` | No | Yes |
| M7 Palette exploration, six pages | `palette` | No | Yes |
| M7 Look exploration, eight fixtures | `look` | No | Yes |
| M7 HUD/minimap matrix | `hud` | Yes | Yes |
| M7 visual acceptance comparison matrix, seven fixtures | `visual` | Accepted default only | Entire matrix |
| 100-repeat golden run, replay hash, snapshot continuation | — | No | Yes |
| Content regeneration, deterministic Blender GLB regeneration, macOS export smoke | — | No | Yes |
| Stress60, 26,000 ticks, existing performance enforcement | — | No | Diagnostic until M9 |

The integration visual check is the existing accepted `default 84 on` fixture;
it uses the same marker, error checks and exit handling as the full matrix.
Style/Palette/Look exploration is run only for its matching targeted scope or
full/milestone, never by routine core or integration verification.
`--m9-acceptance` retains full coverage and promotes Stress60 from
`BLOCKING_LATER — M9 large-battle acceptance` to blocking. No stress assertion,
performance threshold, fixture or regression test is removed or relaxed.

## Everyday commands

From the repository root:

```bash
./tools/doctor.sh              # explain whether the Mac is ready
./tools/verify.sh              # TARGETED core (default)
./tools/verify.sh --targeted workflow  # docs/harness maintenance
./tools/verify.sh --targeted design-package  # reference integrity, no game
./tools/verify.sh --integration # PR/INTEGRATION
./tools/verify.sh --full        # MILESTONE/FULL (also --milestone)
./tools/verify.sh --m9-acceptance # full, with Stress60 blocking
./tools/verify.sh --integration --list-stages # inspect routing, execute nothing
./tools/run-game.sh            # launch the real PrototypeRTS project
./tools/build-mac.sh           # create the playable debug app
./tools/capture-visual-smoke.sh
./tools/setup-git-hooks.sh     # one-time local Git safety setup
```

Verification logs and summaries go to `Artifacts/Verification/`; screenshots go to `Artifacts/Screenshots/`. Both are ignored by Git. `./tools/doctor.sh` discovers the normal Godot application path automatically; `GODOT_BIN` remains an explicit fallback for a nonstandard installation.

The Git hooks block ordinary staged changes to `Docs/Canon/` and direct pushes to remote `main`. Their printed environment-variable overrides are intentionally explicit and reserved for deliberate human-authorized recovery or canon work.

## Persistent approval guidance

Codex command rules match exact argument prefixes. An approval accepted as a
persistent allow rule can skip repeat prompts for that prefix in future tasks;
the prefix must be reviewed narrowly. Project-local rules load only for a
trusted project, while approval-dialog allow-list entries are stored in the
user rules layer. See the official OpenAI documentation for
[Codex command rules](https://learn.chatgpt.com/docs/agent-configuration/rules)
and [agent approvals and security](https://learn.chatgpt.com/docs/agent-approvals-security).

The following repeat repository commands are suitable for narrow persistent
approval when Codex proposes their exact prefix:

- `dotnet build LEGO.SpaceRTS.Phase10.sln` and the explicit Godot project build;
- `dotnet test SimCore.Tests/SimCore.Tests.csproj`;
- `./tools/verify.sh` (the same prefix covers `./tools/verify.sh --full`);
- the exact Godot 4.7.1 .NET executable path followed by `--headless`, after
  `./tools/doctor.sh` has validated that executable;
- `./tools/build-mac.sh`;
- `./tools/doctor.sh` and `./tools/capture-visual-smoke.sh`;
- `python3 tools/Validation/validate_phase10.py`;
- other repository-owned validation scripts only after their source and exact
  command prefix have been reviewed.

Prefer the repository target in a `dotnet build`/`dotnet test` prefix instead
of persistently allowing every possible `dotnet` invocation. Do not create a
blanket `./tools/*`, shell, Python, or Godot prefix. Approval remains required
for `sudo`, destructive removal, `git reset --hard`, force-push, arbitrary
downloaded scripts, package/dependency installation, system-wide writes, and
other destructive or scope-expanding operations.

This repository guidance does not itself modify user-level or system-wide
Codex settings. A human may accept a safe prefix in the approval dialog or
deliberately configure a reviewed rule; agents must not broaden those settings
without explicit authorization.
