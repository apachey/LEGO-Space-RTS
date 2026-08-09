# Phase 09 Engine Amendment — Godot 4.7.x .NET

**Status:** authoritative amendment for engine-specific Phase 09 decisions used by Phase 10 v0.2.

## Replaced canon

The former Unity 6.3 LTS host decision is replaced by:

- engine: **Godot 4.7.x .NET**;
- Phase 10 pin: **Godot 4.7.1-stable .NET**;
- primary language: C#;
- renderer: Godot **Forward+**;
- runtime UI: Godot `Control`/scene UI in C#;
- input: Godot `InputMap` + C# command adapter;
- presentation scene graph: Godot `Node`/`Node3D`;
- camera: `Camera3D`;
- debug/world overlays: `MultiMesh`, `ImmediateMesh`, normal 3D nodes;
- future transport carrier: project-owned command/snapshot protocol over a Godot packet transport; ENet is the default M6 candidate.

The Unity-specific decisions for URP, UI Toolkit, Unity Transport, asmdefs, Unity serialization/meta policy, Unity Test Framework, Unity scene bootstrap and Unity project settings are superseded.

## Preserved canon

The amendment does **not** alter:

- game design, roster, economy, world, combat, UX or visual canon unless explicitly engine-specific;
- Windows 11 x86-64 initial target;
- 60 FPS presentation target;
- 20 Hz deterministic authoritative simulation;
- Q16.16 `Fix32`;
- `Angle16`;
- engine-independent `SimCore`;
- custom deterministic entity/component architecture;
- stable content IDs and monotonic runtime `EntityId`;
- 160×160 build grid / 320×320 nav raster;
- custom deterministic hierarchical pathfinding;
- local steering/reservations;
- CPU-authoritative fog/LoS;
- binary snapshot/replay/hash architecture;
- human-editable JSON → validated immutable runtime data;
- dedicated-server command/snapshot architecture;
- performance gates and deterministic golden-run discipline.

## Engine-boundary rule

`SimCore` may not reference `Godot`, `GodotSharp`, Godot scene objects, engine time, physics callbacks, rendering state or Input state.

The Godot host converts player intent into `CommandEnvelope` values and consumes immutable/read-only presentation snapshots. It may never mutate authoritative components as a rendering convenience.

## Physics rule

Godot physics remains non-authoritative. No `CharacterBody3D`, `RigidBody3D`, `Area3D`, collision signal or PhysicsServer result decides gameplay movement, collision, targeting, fog, pathing or state transitions.

## Future networking rule

Godot's network/packet APIs may carry packets in M6, but they do not become game-state authority. The project still owns:

- command serialization;
- command ordering;
- snapshot serialization;
- protocol versions;
- stable IDs;
- reconciliation/validation policy;
- replay compatibility.

Changing the transport carrier in the future must not change deterministic offline command semantics.
