# LEGO Space RTS — FIRST PLAYABLE PROTOTYPE IMPLEMENTATION v0.4

**Phase:** 10 — M0–M2 Implementation  
**Engine amendment:** Godot **4.7.1-stable .NET**, C#  
**Primary platform:** Windows 11 x86-64  
**Status:** M0–M2 implementation package + macOS runtime-feedback patches; executable acceptance gates still require the pinned local toolchain

This repository replaces only the Unity-specific host from the original Phase 10 package. The authoritative gameplay simulation remains the engine-independent `SimCore` library. Godot owns presentation, input, camera, debug visualization and UI; it does not own gameplay truth.


## v0.4 hands-on M2 patch

The first macOS playtest exposed presentation and movement issues that static validation could not reveal. v0.4 adds visible selection/marquee feedback, control-group labels, numbered Shift-queue markers, legal formation-slot resolution, cost-preserving path smoothing, improved reservation yielding and immediate collision avoidance. See `Docs/M2_USABILITY_MOVEMENT_PATCH_v0.4.md`.

## Locked technical spine preserved

- authoritative simulation: pure C# `SimCore`, no Godot/Unity dependency;
- fixed simulation: **20 Hz / 50 ms**;
- numerics: `Fix32` Q16.16, `FixVec2`, `Angle16`;
- entity model: deterministic custom component stores with monotonic `EntityId`;
- commands: deterministic tick/player/sequence ordering, 16-order per-entity queue;
- state hashing: stable FNV-1a-based authoritative hashing;
- snapshots/replay: explicit versioned binary formats;
- map: 160×160 build cells, 320×320 navigation raster;
- navigation: custom deterministic HPA-style clusters (**10 nav cells**) + local A*;
- five footprint/clearance families; deterministic local avoidance and **12-tick** reservations with heavy priority;
- CPU-authoritative fog/LoS;
- authored dynamic Excavatable topology;
- 60-mover stress scenario and deterministic golden-run foundation;
- future multiplayer remains custom command/snapshot protocol; Godot transport is only a carrier when M6 begins.

## Godot-specific replacement decisions

- renderer: **Godot Forward+**;
- normal runtime UI: Godot `Control` nodes in C#;
- input abstraction: Godot `InputMap`;
- world overlays/debug: `MeshInstance3D`, `MultiMeshInstance3D`, `ImmediateMesh`;
- camera: `Camera3D`, preserving Phase 07 perspective/FOV/pitch/yaw/zoom constraints;
- Godot physics: presentation-only/non-authoritative;
- future network carrier: Godot ENet/packet APIs are the default candidate, but M6 protocol semantics remain project-owned and are not implemented here.

## Repository layout

- `SimCore/` — authoritative engine-independent simulation.
- `SimCore.Tests/` — pure NUnit deterministic tests.
- `HeadlessSim/` — pure .NET deterministic CLI/benchmark runner.
- `GodotClient/` — native Godot 4.7.1 .NET project and M2 presentation/input host.
- `Content/` — human-editable source content/map JSON.
- `GodotClient/Compiled/` — deterministic compiled runtime content consumed by the Godot host.
- `tools/ContentCompiler/` — source JSON → validated binary compiler.
- `tools/Validation/` — source-level invariant validator.
- `Tests/Golden/` — trusted golden-manifest location.
- `Docs/` — architecture, bootstrap, tests, gates and engine-amendment record.

## Prerequisites

1. Godot **4.7.1-stable .NET** x86-64.
2. .NET **8 SDK or later compatible SDK**; CI is pinned to .NET 8.x for this package.
3. Windows 11 x86-64 for the primary playable prototype target.
4. Git; Git LFS only when future large binary source art/audio enters the repository.

## First local build

From repository root:

```powershell
dotnet restore LEGO.SpaceRTS.Phase10.sln
dotnet build LEGO.SpaceRTS.Phase10.sln -c Debug
dotnet test SimCore.Tests/SimCore.Tests.csproj -c Debug
dotnet run --project tools/ContentCompiler/ContentCompiler.csproj -- Content/PrototypeEntities.json Content/Maps/DEV_FirstControllableRTS.map.json GodotClient/Compiled
```

Then open `GodotClient/project.godot` in the **4.7.1-stable .NET** editor and run the project. `Bootstrap.tscn` transitions into `PrototypeRTS.tscn`.

The repository contains pre-generated M2 binaries for loader/bootstrap convenience. The artifact environment could not execute the C# compiler, so these binaries are not treated as certified output: regenerate them with `ContentCompiler` before acceptance. Source JSON remains authoritative.

## Godot headless smoke

With the pinned Godot .NET executable:

```powershell
Godot_v4.7.1-stable_mono_win64.exe --headless --path GodotClient -- --smoke
```

Expected process result after the technical scene advances:

```text
PHASE10 GODOT HEADLESS SMOKE: PASS ...
```

This smoke confirms the Godot host can instantiate and advance the simulation; it does not replace pure deterministic regression/benchmark tests.

## Headless deterministic runner

```powershell
dotnet run --project HeadlessSim -- --scenario first --ticks 1200 --hash-every 200
dotnet run --project HeadlessSim -- --scenario golden --ticks 3200 --repeat 100 --hash-every 400
dotnet run --project HeadlessSim -- --scenario stress60 --ticks 3000 --benchmark --path-benchmark --enforce-performance-gates
```

Snapshot/replay capabilities include `--snapshot-in`, `--snapshot-out`, `--replay`, `--record-replay`, `--golden-manifest-in`, `--golden-manifest-out`, and `--dump-state`.

## M2 controls

- LMB: select; drag: marquee.
- Shift: additive selection / queued Move.
- Ctrl-click: visible same-type selection foundation.
- Alt: subtract selection / camera tilt modifier.
- RMB or `M`: Move.
- `S`: Stop.
- `H`: Hold Position.
- `Ctrl+0–9`: assign group; `Shift+0–9`: add; `Alt+0–9`: remove; `0–9`: recall; double-tap centers camera.
- Arrow keys / middle-drag: camera pan.
- `,` / `.`: 90° camera rotation.
- Wheel: zoom; Alt+wheel: tilt.
- `Home`: camera reset.
- `F9`: engineering-only OPEN EXCAVATABLE FEATURE command.

## Verification status

This package was statically audited in the artifact environment. That environment does **not** contain a functioning Godot 4.7.1 .NET editor or `dotnet` SDK/runtime, so this package does not falsely claim that compilation, NUnit, Godot headless smoke, 100-run golden validation, or benchmark gates passed here.

Run:

```bash
python3 tools/Validation/validate_phase10.py
```

then complete the executable gates in `Docs/ACCEPTANCE_GATES.md` before beginning M3.
