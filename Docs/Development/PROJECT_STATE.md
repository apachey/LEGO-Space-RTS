# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M4 and M5 are implemented, game-director accepted, integrated, and merged to
`origin/main` through PR #12 (`85b02f2`). M6 implementation may begin; the
preserved 60-mover scale gate must pass before M6 networked 1v1 acceptance.**

- M3 is merged and human-accepted.
- M4 T040–T048 combat, repair, transport and tactical transformation are merged
  to `origin/main` and human-accepted.
- M5 T049–T057 four-faction system proof and its six-part executable acceptance
  handoff are implemented and human-accepted.
- The integrated post-M5 baseline has passed full repository verification.
- The preserved 60-mover stress is `BLOCKING_LATER — M6 acceptance`; it does
  not block starting M6 implementation.
- `codex/60-mover-fix` is preserved pre-M5 research and is not a merge-ready
  fix. Production work continues from the integrated post-M5 baseline.

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
51.67% completion and 4.17× realtime throughput. It is the explicit
`BLOCKING_LATER` gate for M6 networked 1v1 acceptance. It does not block M6
implementation from starting. A snapshot produced directly by merged M4 format
19 / protocol 17 was also loaded and advanced by the combined format-20 reader.

Pre-M6 investigation found that the old completion signal was not trustworthy:
formation reflow could replace a unit's assigned destination with its current
position and `HasTarget == false` was then counted as arrival. Removing that
false completion exposes the same defect in the representative M2 movement
scenario: 7 of 24 movers remain short of reachable, valid endpoints, usually
behind already-arrived friendly units. This does not reopen M3–M5 gameplay, but
the movement correction must preserve the accepted post-M5 baseline.

The game director approved a bounded deterministic formation-arrival sequencer.
Its prototype preserves immutable endpoints, routes later rows through temporary
arrival points only near the destination, and restores the honest representative
M2 acceptance to PASS (24/24).

The 60-mover fixture itself was also invalid: mixed Large/Huge units were spawned
two build cells apart despite collision diameters up to 3.2. The corrected fixture
uses legal four-cell spacing and now has explicit passability/non-overlap coverage.
With legal starts, the first honest stress phase reaches only 4/60 endpoints.
The remaining units form mid-route clusters around constrained central passages,
so the unresolved defect is corridor traffic/local yield rather than arrival or
endpoint legality.

The game director then approved one final bounded clean-room recovery experiment
inside the existing local-separation scorer. Stuck movers received deterministic
neighbor-pressure scoring and an explicitly wider but still bounded corridor
window; motion still used ordinary kinematics and preserved Heavy priority. It
did not improve physical completion (4/60 remained 4/60) and worsened the first-
phase diagnostics from 43 to 47 deadlocks and from 3,504 to 12,831 oscillations.
The failed experiment was removed rather than tuned or stacked with more patches.
After cleanup, `./tools/verify.sh` passed all normal blocking stages with 256
tests, the honest 24/24 M2 movement acceptance, compiled-content validation,
HeadlessSim smoke and Godot headless smoke. The exact summary is
`Artifacts/Verification/20260820T142908Z-fast-summary.txt`.

The corrected `./tools/verify.sh --full` classification also passed on
2026-08-20: every current blocking stage, 100-repeat determinism, replay,
snapshot continuation, compiled-content regeneration and macOS export were
green. Stress60 remained an explicit `BLOCKING_LATER` diagnostic failure with
phase completion 4/60, 5/60 and 2/60. The exact summary is
`Artifacts/Verification/20260820T144335Z-full-summary.txt`.

## Next approved action

The approved arrival sequencer fixes the representative arrival wall but does
not clear the distinct 60-mover mid-route traffic blocker. The preserved pre-M5
portal-flow/traffic-controller experiment already failed to satisfy the gate;
do not resurrect or stack it as another patch.

The exact Phase 09B wording makes this gate blocking **before M6 networked 1v1
acceptance**, not before M6 implementation starts. Therefore:

1. integrate the verified immutable-endpoint, arrival-sequencing and honest
   stress-fixture work;
2. begin **M6 T058 — network transport host foundation** from the accepted
   post-M5 baseline. Phase 09A supersedes T058's old Unity wording: inspect the
   current Godot host and official Godot packet/ENet APIs, then implement the
   smallest project-owned dedicated-server/two-connection carrier slice. Keep
   command semantics in SimCore and do not pull T059 command validation or T060
   snapshot replication into T058;
3. keep stress60 `BLOCKING_LATER` throughout M6 development and require it to
   pass before M6 acceptance.

`ARCHITECTURE REVIEW REQUIRED` only before another movement attempt that adds a
traffic coordinator/solver or otherwise expands movement architecture. The
failed local-pressure experiment is not a reason to hold T058 or other bounded
M6 implementation work.

## New-thread M6 bootstrap

A fresh Codex task should start with:

> Read AGENTS.md and the current repository state. Start M6 T058 from the
> accepted post-M5 movement foundation. Treat stress60 as BLOCKING_LATER for M6
> acceptance, not as a blocker for starting T058.

The task must verify that commit `44e2caf` (or its eventual merged descendant)
is present before planning T058. If it is not on the accepted base, stop and
integrate that movement handoff first rather than reconstructing it from chat.
