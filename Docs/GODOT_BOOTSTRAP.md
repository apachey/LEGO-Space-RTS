# Godot 4.7.1 .NET bootstrap

## Project pin

Open `GodotClient/project.godot` only with **Godot 4.7.1-stable .NET** for Phase 10 certification. Do not use the Standard (non-.NET) build.

The C# host project uses `Godot.NET.Sdk/4.7.1` and `net8.0`; the portable SimCore remains `netstandard2.1` / C# 9.0 to keep the engine boundary conservative.

## Rendering

`project.godot` declares `Forward Plus`. M2 uses simple generated meshes/materials only. No final art is included.

## Scenes

- `Bootstrap.tscn` — minimal startup scene.
- `PrototypeRTS.tscn` — explicit composition-root scene.

Core runtime dependencies are instantiated/wired by `RtsCompositionRoot`; no `FindObjectOfType` equivalent or global scene search is used.

## Runtime content

Regenerate deterministic runtime data after editing source JSON:

```powershell
dotnet run --project Tools/ContentCompiler -- Content/PrototypeEntities.json Content/Maps/DEV_FirstControllableRTS.map.json GodotClient/Compiled
```

The client validates/loads the same `PrototypeContentCodec` and `CompiledMapCodec` binary formats as the pure C# tooling.

## Godot import/cache policy

Do not commit `.godot/`, `.mono/`, import cache or editor-local state. Commit `.godot` source text only when it is an authored project file such as `project.godot`; generated `.godot/` directory is ignored.

## Simulation clock

The host calls `GodotSimBridge._Process(delta)` only to feed an accumulator. The bridge steps whole **50 ms** authoritative ticks and processes at most four per rendered frame. Excess backlog is retained; no authoritative tick is silently skipped.

Godot's configured physics tick rate is intentionally independent and has no gameplay authority.

## Presentation

`PresentationSnapshot` is the only normal source for rendered entity state. Render transforms interpolate between previous/current snapshots. Selection identity, command legality and fog authority are never interpolated.

## Headless smoke

```powershell
Godot_v4.7.1-stable_mono_win64.exe --headless --path GodotClient -- --smoke
```

The smoke runner exits non-zero if the scene cannot advance a valid M2 simulation.
