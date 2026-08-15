# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M4 and M5 are implemented and game-director accepted. Their combined
integration is being finalized on `codex/m4-m5-integration`.**

- M3 is merged and human-accepted.
- M4 T040–T048 combat, repair, transport and tactical transformation are merged
  to `origin/main` and human-accepted.
- M5 T049–T057 four-faction system proof and its six-part executable acceptance
  handoff are implemented and human-accepted.
- The integration branch combines both accepted milestones and has passed full
  repository verification. It is ready for the user's merge to `main`.
- The 60-mover diagnostic is permitted to remain diagnostic for M5, but becomes
  `BLOCKING_NOW` before M6 begins.

## Locked technical foundation

- Godot 4.7.1-stable .NET host with C#.
- Engine-independent deterministic SimCore at fixed 20 Hz.
- Fix32, FixVec2 and Angle16 authoritative numerics.
- Deterministic command execution, snapshots, replay and state hashing.
- Project-owned HPA/local-A* navigation with persistent route corridors,
  deterministic local separation, formation intent and bounded recovery.
- Godot owns presentation, input and UI; it does not own gameplay truth.

## Accepted playable systems

- M2 selection, controls, camera, fog/vision and deterministic movement.
- M3 finite resources, harvesting, local banks, construction, production,
  Operations Capacity, Energy Domains, brownouts and Basic HUD.
- M4 deterministic targeting, ranged/contact weapons, projectiles, damage,
  armor, destruction, Crew repair, Rapid Rider transport and MX-41 tactical
  transformation including rollback.
- M5 Worksite graphs, authored excavation topology, forward service and Mission
  Refit, Alien tube graphs/transfers, Resonance Core commitments, Alien Charge
  Surge, displacement/stability and Excavation Searcher clamp passage.
- Developer-prepared acceptance controls remain available for repeatable M4 and
  M5 visual checks.

## Integration format boundary

M4 and M5 were developed in parallel and both independently used snapshot
format 19 / simulation protocol 17 for incompatible layouts. The game director
approved the combined boundary:

- snapshot format **20**;
- simulation protocol **18**;
- replay format **15**;
- compiled prototype content format **16** / source schema **15**;
- read compatibility with the merged M4 snapshot **19 / 17**;
- no compatibility guarantee for the unmerged branch-only M5 snapshot-19 or
  replay-7–14 layouts.

M4 command IDs 11–15 remain stable. M5 commands use IDs 16–18. The combined
snapshot uses a 64-bit component mask so both milestones have non-overlapping
authoritative state.

## Verification state

Before integration, M4 and M5 each passed their own automated and human gates.
The latest accepted M5 full run contained 177 passing tests and produced the
macOS debug build at `Builds/macOS/LEGO Space RTS.app`.

The combined branch passed `./tools/verify.sh --full` on 2026-08-15 with all
blocking stages green, including 247 NUnit tests, 100-repeat determinism,
replay/final-hash verification, snapshot continuation, Godot smoke and macOS
export. The exact summary is
`Artifacts/Verification/20260815T154942Z-full-summary.txt`.

The permitted 60-mover diagnostic remained red: 31/60 movers completed, with
51.67% completion and 4.17× realtime throughput. This remains the explicit
blocking-later gate before M6. A snapshot produced directly by merged M4 format
19 / protocol 17 was also loaded and advanced by the combined format-20 reader.

## Next approved action

1. Complete and verify the M4+M5 integration branch.
2. Hand the branch to the game director for the final merge to `main`.
3. Before any M6 implementation, promote and satisfy the 60-mover gate.

Do not begin M6 merely because the integration compiles.
