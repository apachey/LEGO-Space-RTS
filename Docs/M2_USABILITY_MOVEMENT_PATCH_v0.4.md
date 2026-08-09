# M2 Usability & Movement Patch — v0.4

This patch responds to the first hands-on M2 control test on macOS/Godot 4.7.1 .NET.

## Observed runtime feedback

1. Selection logically worked but selection feedback was effectively invisible, and marquee selection appeared to miss some units.
2. RMB Move worked.
3. Units could interpenetrate / jam against one another.
4. Shift-queued Move had unclear feedback and units appeared to begin later orders at inconsistent times.
5. Stop worked.
6. Hold looked identical to Stop in M2.
7. Control groups worked but membership was not shown on units.
8. Paths visibly looked like raw grid/A* output.

## v0.4 changes

### Selection

- Added an actual screen-space marquee rectangle while dragging.
- Fixed the engineering selection ring: the v0.3 TorusMesh was rotated edge-on even though Godot TorusMesh already lies in the ground plane.
- Selection rings now use an unshaded/no-depth-test material for reliable prototype visibility.
- Increased click/marquee pick radius by footprint class so selection is less center-point fragile.
- Preserved the canonical Phase 07 worker-priority rule: ordinary mixed army marquee filters workers when combat/support units are also enclosed. The debug HUD now explicitly says how many workers were filtered and reminds the tester that Ctrl+drag includes workers.

### Control groups

- Units now show persistent presentation-only Label3D membership text such as `1` or `1,3` when assigned to control groups.
- This does not change authoritative simulation state.

### Queued movement feedback

- Move destinations now stay visible as numbered world markers.
- A normal RMB Move creates marker `1`; Shift+RMB appends `2`, `3`, etc.
- Stop/Hold clear the preview.
- The preview clears automatically after the associated group has no active/queued movement.
- The simulation remains per-unit queued-command execution; v0.4 does not invent a synchronized group wait barrier.

### Movement smoothness

- Intermediate path nodes no longer act as braking destinations.
- If another Move is queued, the current queued waypoint is also treated as a through-point rather than a full stop.
- Only the final unqueued destination uses full braking logic.

### Formation destination safety

- Formation slots are now resolved to a nearby legal passable nav position if the ideal slot falls inside/too close to an obstacle.
- This removes a source of apparently random units that waited forever because their individual formation slot was unreachable even when the group-center RMB target was legal.

### Reservations and local avoidance

The v0.3 movement stack had two important weaknesses:

1. reservation denial meant a full freeze;
2. local avoidance scored primarily a 12-tick future position, allowing immediate overlap before the future penalty became relevant.

v0.4 changes this to:

- reserve present occupied footprints before future-space planning;
- reserve a short sampled corridor toward smoothed waypoints instead of treating sparse waypoint indices as contiguous path cells;
- a denied reservation yields at 35% speed instead of freezing completely;
- immediate one-tick predicted overlap is rejected when units are currently separated;
- already-overlapping units strongly prefer candidates that increase separation;
- the existing 15% / 1.5s friendly-compression rule remains intact;
- collision radii are centralized in `FootprintRules` so movement and tests use the same authoritative values.

### Path quality

- Raw local A* still creates a deterministic legal grid route internally.
- The route is now passed through deterministic cost-preserving line-of-sight smoothing.
- Smoothing never accepts an impassable shortcut and does not accept a shortcut more expensive than the original segment, including Ground Rough-terrain cost.
- Lookahead is bounded to 48 nav cells (24 build cells) to control path-request cost.
- Debug path rendering therefore shows strategic waypoints/straight legal segments instead of every 0.5-cell A* step.

This improves M2 path readability without replacing the Phase 09 HPA + deterministic local A* architecture.

### Hold vs Stop

No gameplay change is made here.

In M2 there is no combat, so both visibly halt movement:

- **Stop:** clears orders and returns to Idle.
- **Hold:** clears movement orders and enters Holding; once combat exists, the unit may rotate/fire but does not intentionally chase away from the held position.

The debug HUD now explains this explicitly.

## Added/updated tests

- deterministic smoothed-path repeatability;
- queued Move remains queued until current Move completes;
- two crossing units may not interpenetrate below the canonical 85% temporary-friendly-compression boundary;
- existing navigation/determinism/reservation tests remain present.

## Verification performed in artifact environment

`python3 tools/Validation/validate_phase10.py`

Result:

`PHASE10 GODOT STATIC VALIDATION: PASS (27 authoritative C# files checked)`

The artifact environment still lacks the pinned Godot/.NET executable toolchain, so v0.4 C# compilation, NUnit, runtime movement behavior, golden hashes and performance gates must be rerun on the local Mac before M2 can pass.
