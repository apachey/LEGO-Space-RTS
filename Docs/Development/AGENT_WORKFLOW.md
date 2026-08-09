# Agent-first development workflow

The goal is simple: every change is prepared away from the accepted game, checked automatically, reviewed, and then handed to the user to accept. Agents implement approved work; they do not decide new game design.

## The product workflow

1. **Task and scope** — The user defines the game-development outcome. The agent reads `AGENTS.md`, the canon index, only the relevant canon/technical documents, and the current implementation.
2. **Isolated worktree** — Work starts on a `codex/` task branch in its own worktree. `main` remains the user-accepted game and is never automatically merged.
3. **Implementation** — The agent changes only the approved area. Gameplay, canon, architecture, dependencies, or data-format changes that were not approved are escalated instead of guessed.
4. **Automated verification** — `./tools/verify.sh` is the fast gate. Determinism, navigation, movement, save/replay, milestone, or large changes also require `./tools/verify.sh --full`. A failing `BLOCKING_NOW` gate blocks completion. `BLOCKING_LATER` and `DIAGNOSTIC` results remain visible without solely blocking the current milestone; skipped checks are reported as `SKIPPED` with a reason.
5. **Self-review** — The agent reviews the final Git diff for hidden gameplay changes, canon edits, engine leakage into SimCore, nondeterminism, generated files, dependencies, unsafe scripts, and machine-specific paths.
6. **Playable macOS build** — When export templates are installed, `./tools/build-mac.sh` creates `Builds/macOS/LEGO Space RTS.app`. Build output is local and not committed.
7. **Human playtest** — The user is asked only for judgement automation cannot provide: movement/camera/selection feel, clarity, visual quality, audio, and strategic readability. Compilation, hashes, file round trips, and headless loading remain automated.
8. **Pull Request** — After verification and review, the task branch can be pushed and opened as a Pull Request. CI repeats the portable SimCore checks; this Mac remains authoritative for Godot and macOS integration.
9. **USER merge** — The user reviews and merges. Agents do not merge into `main`.

## Everyday commands

From the repository root:

```bash
./tools/doctor.sh              # explain whether the Mac is ready
./tools/verify.sh              # fast, reliable development gate
./tools/verify.sh --full       # Phase 10 determinism/stress/export gate
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
