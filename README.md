# LEGO Space RTS

A deterministic C# RTS with a Godot 4.7.1-stable .NET presentation host.
For the accepted baseline, current milestone and next approved task, read
[PROJECT_STATE](Docs/Development/PROJECT_STATE.md).

## Architecture

- `SimCore/`: engine-independent gameplay at fixed 20 Hz, fixed-point numerics,
  deterministic commands, project-owned navigation, snapshots and replay.
- `GodotClient/`: Forward+ rendering, input, camera, UI, audio and effects;
  dedicated-server networking uses project-owned packets over Godot ENet.
- `Content/` and `tools/ContentCompiler/`: source JSON and validated deterministic
  runtime binaries under `GodotClient/Compiled/`.
- `SimCore.Tests/` and `HeadlessSim/`: NUnit regression coverage and deterministic
  command-line scenarios/benchmarks.
- `Docs/Canon/`: approved design; `Docs/Development/`: workflow and implementation.

## Run, build and test

Requires the pinned Godot .NET host, .NET 8 or a compatible SDK, and Git.
Windows 11 x86-64 is the primary platform; macOS debug builds support playtests.
From the repository root on macOS:

```bash
./tools/doctor.sh                         # check installed prerequisites
./tools/run-game.sh                       # user-launched game
./tools/build-mac.sh                      # Builds/macOS/LEGO Space RTS.app
./tools/verify.sh                         # targeted core checks
./tools/verify.sh --targeted workflow      # documentation/harness maintenance
./tools/verify.sh --integration           # cross-system regression checks
./tools/verify.sh --full                  # milestone/acceptance checks + export
```

Portable .NET entrypoints:

```bash
dotnet restore LEGO.SpaceRTS.Phase10.sln
dotnet build LEGO.SpaceRTS.Phase10.sln -c Release
dotnet test SimCore.Tests/SimCore.Tests.csproj -c Release
dotnet run --project HeadlessSim -- --scenario first --ticks 1200 --hash-every 200
```

For editor use, open `GodotClient/project.godot` in the pinned .NET editor.
Automation must remain headless; renderer captures are explicitly announced.
Verification logs live in `Artifacts/Verification/`; captures in
`Artifacts/Screenshots/`; build outputs are local and ignored by Git.
See [AGENT_WORKFLOW](Docs/Development/AGENT_WORKFLOW.md) for profile selection,
system-specific checks, gate classifications and review/merge boundaries.
