# M2 Godot/macOS Presentation Patch — v0.3

This patch addresses issues discovered during the first real macOS Godot 4.7.1 .NET run.

## Fixed

- Initial RTS camera now centers on player 0's spawned simulation entities instead of the center of the 160x160 map.
- Vertical keyboard/edge/middle-drag camera motion now follows screen-up/screen-down rather than the inverse horizontal camera-offset vector.
- Engineering placeholder unit meshes are significantly larger and are sized in presentation world units close to their gameplay footprint, improving 44-cell-view readability.
- Debug HUD uses larger text/buttons and a larger panel.
- Prototype window override is raised from 1280x720 to 1600x900 while the canonical 1920x1080 logical viewport is preserved; the debug HUD also uses explicit larger font sizes for Retina/HiDPI readability.
- Map debug presentation now visually separates impassable terrain from Rough Ground.
- Fog/background tint is more neutral to avoid the misleading purple-floor appearance.

## Not changed

- SimCore deterministic gameplay.
- 20 Hz simulation.
- fixed-point math.
- map/nav dimensions.
- movement/pathfinding/reservations.
- fog authority.
- gameplay canon.

## Certification

This environment does not contain Godot or the .NET SDK, so the patch is statically reviewed but must be compiled and smoke-tested in the user's Godot 4.7.1-stable .NET environment.
