# Unity → Godot Porting / Reuse Audit

This audit exists to enforce the amendment instruction: reuse portable SimCore code where valid rather than blindly rewriting it.

## Authoritative SimCore reuse

A direct tree comparison against the original Phase 10 v0.1 package shows that the authoritative SimCore was retained almost entirely.

Of the **27 authoritative `SimCore/Runtime` C# files**, only one source file was functionally amended:

- `Scenarios/ScenarioFactory.cs` — added an overload accepting a compiled `MapDefinition` and `PrototypeContentCatalog` so the Godot host can instantiate exactly the same deterministic scenario from compiled runtime data.

Removed because they were Unity packaging metadata, not simulation logic:

- `SimCore/Runtime/LegoSpaceRTS.SimCore.asmdef`;
- `SimCore/package.json`.

All deterministic math, ID, entity, command, hash, snapshot, replay, map, HPA/A*, reservation, movement, spatial and fog/LoS source files were otherwise reused rather than rewritten.

## Portable tooling reuse

`ContentCompiler` remains the same portable C# compiler pipeline.

`HeadlessSim` remains the same pure .NET runner with only engine-amendment support added:

- package/build label updated to v0.2 Godot;
- `--compiled-dir` added so the pure runner can exercise the exact binary map/content consumed by the Godot host.

## Test reuse

The existing pure NUnit suite remains.

Amendments:

- `AssemblyBoundaryTests` now rejects Godot references as well as Unity references;
- `CompiledScenarioTests` verifies factory data and codec-round-tripped data create the same initial authoritative state hash.

## Code deliberately rebuilt

The former Unity `Game/` host was not translated line-by-line. It was removed.

Godot-native replacements live under `GodotClient/` and use appropriate engine primitives:

- `Node`/`Node3D` composition;
- `Camera3D`;
- `InputMap`;
- `MeshInstance3D`/`MultiMeshInstance3D`/`ImmediateMesh`;
- `CanvasLayer`/`Control` containers;
- native `.tscn` scenes;
- `--headless` smoke execution.

These classes are presentation/input adapters around SimCore, not a second gameplay implementation.

## Porting conclusion

The amendment changes the engine host while preserving the deterministic simulation investment. The Godot branch is therefore an engine port of the presentation/integration layer, not a gameplay rewrite.
