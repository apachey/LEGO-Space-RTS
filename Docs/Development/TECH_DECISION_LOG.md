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
