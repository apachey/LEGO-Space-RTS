# Manual M2 smoke test — Godot 4.7.1 .NET

1. Regenerate `GodotClient/Compiled` with `ContentCompiler`.
2. Open `GodotClient/project.godot` in Godot 4.7.1-stable .NET.
3. Run the project and confirm the log reports `compiled runtime data`.
4. Confirm the technical map, obstruction blocks, Rough Ground, placeholder units and fog presentation are visible.
5. Pan with arrows/middle drag; zoom with wheel; rotate with comma/period; reset with Home.
6. Single-select and marquee-select units. Confirm workers do not pollute an ordinary combat/support marquee when priority applies.
7. Right-click a legal destination and confirm a command marker plus authoritative movement.
8. Shift-right-click at least two destinations and confirm queued movement executes in order.
9. Issue Stop and Hold Position.
10. Assign at least two control groups, add/remove members and recall them; double-tap a group to center the camera.
11. Route mixed Tiny/Small/Medium/Large units through the medium, heavy and wide lanes.
12. Enable reservation/path debug and confirm moving agents receive deterministic reservation behavior rather than overlapping permanently.
13. Observe CPU fog: explored space remains explored; hidden enemy truth is not rendered from mutable engine entities.
14. Send units toward the closed Excavatable route; confirm it blocks pathing.
15. Press F9; confirm only the affected topology changes and the route becomes available without rebuilding the whole map.
16. Run the Godot headless `--smoke` command and require exit code 0.
17. Run pure NUnit, 100-run golden and stress benchmark gates.

Failure conditions include hash divergence, reachable persistent deadlock, hidden truth leak, Godot physics/navigation becoming gameplay authority, or any SimCore engine reference.
