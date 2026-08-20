# M0–M2 Architecture

## Authority boundary

The repository is split into three runtime layers.

### 1. SimCore — authoritative

Pure C# with no game-engine reference. Owns:

- fixed-point math and angles;
- time/ticks;
- Stable IDs / `EntityId`;
- entity/component storage;
- map/topology;
- command ordering and queues;
- deterministic hierarchical navigation;
- formation intent;
- short-horizon reservations;
- movement/local avoidance;
- spatial index;
- fog/LoS;
- snapshot/replay/hash state.

### 2. GodotClient — non-authoritative host

Owns:

- real-time accumulator feeding complete SimCore ticks;
- read-only presentation snapshots;
- `Camera3D`;
- input → command-envelope translation;
- screen-space selection;
- control groups;
- primitive map/unit presentation;
- fog rendering from CPU-authoritative fog state;
- engineering overlays/HUD.

Godot scene transforms are never copied back into SimCore as gameplay truth.

### 3. Tools / HeadlessSim

Pure .NET tools own:

- content/map compilation;
- deterministic CLI regression;
- snapshot/replay IO;
- golden checkpoint validation;
- performance benchmarking;
- static invariant validation.

## Deterministic system order

The M2 simulation explicitly schedules the relevant subset of Phase 09 systems. The implemented runner executes commands, navigation/group intent, reservation planning, movement intent/local avoidance, transform/orientation updates, spatial rebuild, vision/LoS and state diagnostics in stable code-defined order. C# event subscription ordering does not determine authoritative outcomes.

## Math

`Fix32` is signed Q16.16. Authoritative multiply/divide use 64-bit intermediates. Overflow uses checked/fail-fast behavior where an overflow would otherwise corrupt state.

`FixVec2` implements deterministic 2D navigation math. `Angle16` uses a UInt16 turn representation and deterministic shortest-turn behavior. No authoritative `float`/`double` appears under `SimCore/Runtime`.

## Time

`SimClock.TicksPerSecond = 20`. Gameplay duration conversion is centralized. SimCore only advances through explicit `StepOneTick`/profiled variants.

## Identity and storage

Content definitions use stable ASCII source keys compiled to deterministic integer IDs. Runtime entities use monotonic IDs and are not recycled accidentally within a match.

Entity storage is compositional rather than one class per final unit. M2 components cover ownership, transform, movement, navigation, footprint/selection metadata, vision and order queues.

## Commands

`CommandEnvelope` carries execution tick, player slot, player sequence, type, entity IDs, target data/modifiers and debug topology feature where relevant.

Buffer ordering is deterministic by execution tick → player → sequence. M2 normal orders are Move, Stop and Hold Position; Shift Move queues. Queue capacity is 16 and overflow behavior is defined by SimCore.

The engineering-only topology command opens the authored Excavatable feature and uses the same map-topology pathway later gameplay excavation will drive.

## Snapshots, replay, hash

The save/snapshot format is explicit binary data with format/protocol versions. It does not rely on Godot scene serialization.

State hashing traverses authoritative state in stable order and excludes camera, VFX, audio, Godot transforms and local selection.

Replay records initial deterministic state plus ordered command stream and can validate a final hash.

## Map/data

Human-editable JSON is validated/compiled into immutable binary runtime data.

Canonical M2 map dimensions:

- build grid: 160×160;
- nav raster: 320×320;
- nav resolution: 0.5 build cell;
- HPA cluster: 10 nav cells.

`DEV_FirstControllableRTS` contains broad open areas, medium/heavy/wide routing tests, impassable blocks, Rough Ground, elevation/LoS geometry and one closed authored Excavatable connection.

## Navigation

Navigation is project-owned and deterministic; no Godot NavigationServer path is authoritative.

The stack is:

1. clearance-aware deterministic cluster/portal topology;
2. strategic HPA-style route;
3. deterministic local A* segments;
4. group-slot intent;
5. 12-tick short-horizon reservation planning;
6. deterministic movement/local avoidance;
7. stuck recovery and diagnostic counters.

Tie-breaking uses stable coordinate/entity ordering rather than hash/dictionary iteration accidents.

## Fog and LoS

Fog/explored/visible state is CPU-authoritative in SimCore. The Godot `FogPresenter` only visualizes player-0 state and never discovers hidden truth through render visibility.

Ground occlusion/elevation support is authored in map data and consumed by SimCore LoS.

## Godot presentation bridge

`GodotSimBridge`:

- accumulates render delta;
- steps whole 50 ms ticks;
- executes at most four catch-up ticks per rendered frame;
- keeps excess authoritative backlog;
- publishes previous/current snapshots;
- exposes interpolation alpha and profiling diagnostics.

This intentionally does not use `_PhysicsProcess` as the game simulation clock.

## Camera and selection

The Godot camera preserves Phase 07 concepts: perspective strategic camera, 36° FOV, 58° default pitch, 45° default yaw, 90° rotation increments and constrained zoom/tilt.

Selection is presentation-side and stable-ID based. M2 supports click/marquee, additive/subtractive semantics, worker/combat-support marquee priority, same-type foundation and 128-entity implementation ceiling.

Control-group membership is stored as `EntityId`, so future transform/refit/transport states can retain identity without the host tracking GameObjects/nodes as gameplay entities.

## Godot physics and navigation services

Godot PhysicsServer, physics bodies, collisions and NavigationServer are not authoritative. They may later be used to support non-gameplay presentation, queries or cosmetic effects only if their outputs cannot alter deterministic state.

## M6 compatibility

The server protocol remains a custom deterministic command/snapshot protocol.
T058 implements Godot ENet as a raw packet carrier with a headless dedicated
host, a maximum of two client connections and three logical channels:
reliable ordered, unreliable sequenced and reliable bulk. No Godot RPC node
tree becomes authoritative simulation state.

The carrier does not deserialize commands, validate player authority or publish
snapshots. Those responsibilities remain project-owned and begin in T059 and
T060 respectively.
