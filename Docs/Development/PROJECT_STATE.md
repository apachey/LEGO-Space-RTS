# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M0–M6 are implemented, verified and game-director accepted. The first M7
Style Lab review selected no direction. A non-canonical research brief now
defines the center for a second Style Lab round, which is implemented, rendered,
fully verified and ready for game-director review. The six-page Palette Ratio
Lab is game-director accepted. Visual canon remains deliberately open.**

- The branch includes the verified post-M5 movement handoff from `44e2caf`
  plus T058–T063.
- The old `codex/60-mover-fix` work remains preserved failed research through
  commits `6105cf0` and `f896b01`; it is not merge-ready production code.
- By explicit game-director decision, Stress60 is now `BLOCKING_LATER — M9
  large-battle acceptance`. It remains visible during M7–M8 but does not block
  M6 acceptance or subsequent production work.

## Locked technical foundation

- Godot 4.7.1-stable .NET host with C#.
- Engine-independent deterministic SimCore at fixed 20 Hz.
- Fix32, FixVec2 and Angle16 authoritative numerics.
- Project-owned deterministic navigation and movement.
- Dedicated-server authoritative multiplayer. Godot ENet carries project-owned
  packets and never owns gameplay truth.

## Accepted gameplay baseline

- M2 selection, controls, camera, fog/vision and deterministic movement.
- M3 economy, construction, production, Operations Capacity, Energy Domains,
  brownouts and Basic HUD.
- M4 combat, armor, destruction, repair, Rapid Rider transport and MX-41
  tactical transformation.
- M5 Worksites, excavation topology, forward service, Mission Refit, Alien
  Charge/Surge, tubes and displacement/stability.

## M6 T058–T063 implementation

The headless dedicated-server entry remains:

`--dedicated-server --network-bind <address> --network-port <port>`

### T058–T059 transport and command authority

- ENet/UDP accepts at most two clients and exposes reliable-ordered,
  unreliable-sequenced and reliable-bulk logical channels.
- Server-created cryptographic tokens bind peers to player slots.
- Reliable intent commands use exact-next sequence processing, rate limits,
  canonical payload shapes and authoritative ownership/fog/resource/placement/
  technology/state checks.
- Accepted commands are rebuilt with the server player slot and next legal
  simulation tick. Debug commands are never accepted from network sessions.

### T060 snapshot replication

- The 20-Hz server publishes recipient snapshots every two ticks: canonical
  10-Hz state delivery on unreliable-sequenced transport.
- The server delta-encodes only against a retained baseline that the client
  explicitly acknowledged. Both sides keep bounded baseline history.
- Stable-ID upserts/removals cover presentation entities and projectiles. Each
  recipient also receives only their own economy/capacity/charge, production,
  queued orders, Energy Domain, Worksite and Tube summaries.
- Sparse fog bitsets use standard-library Deflate compression. The real
  two-client acceptance observed a largest initial packet of 665 bytes, below
  ENet's default MTU.
- Client helpers reconstruct full state and interpolate position/orientation
  without changing simulation authority.

### T061 fog filtering

- A recipient snapshot cannot recreate `SimulationWorld` and contains only
  owned, neutral or currently visible presentation state.
- Hidden opponents are absent. Loss of visibility is an entity removal; the
  prior presentation is exposed only as client-side last-known state.
- Hidden target IDs are removed from weapon and repair presentation references.
- Per-player fog and economy data are captured independently; a compromised
  client does not receive the other player's private state.

### T062 reconnect

- Disconnected sessions retain token, player slot and last command sequence for
  60 authoritative seconds.
- Reconnect requires exact simulation protocol, gameplay content and initial map
  hashes.
- Reliable-bulk restore sends a current full legal recipient snapshot plus the
  player's pending accepted commands, unit orders and production queues, then
  resumes regular snapshot streaming from a new baseline.
- The ENet acceptance disconnects player 0 while player 1 stays connected,
  restores sequence 1 on a replacement peer and executes sequence 2.

### T063 network replay

- The server records the initial authoritative snapshot, accepted command log,
  gameplay/map hashes, deterministic seed field, state hashes every 20 ticks
  and seek snapshots every 200 ticks.
- Replay format 16 stores final tick/hash and remains backward-readable for
  replay format 15.
- Playback seeks from the nearest snapshot and fails on hash mismatch.
- Full authoritative replay is unavailable during an active match so it cannot
  bypass fog filtering. After explicit match completion, authenticated clients
  receive it as hash-verified 48-KiB reliable-bulk chunks.
- The ENet acceptance delivered a 666,891-byte server log and reproduced the
  authoritative final hash.

## M7 T064 material-master implementation

The implementation below remains useful infrastructure, but its single
stylized-PBR premise is not an accepted visual target.

- The Godot client owns one centralized PBR material family for molded polymer,
  tool metal, rubber, transparent polymer, Crystal and terrain. It uses the
  engine's built-in `StandardMaterial3D`; no dependency or custom shader was
  added.
- Existing terrain and placeholder entity presentation now consume the same
  masters. Authoritative simulation, content, networking and gameplay formats
  are unchanged.
- The isolated **M7 Material Lab** presents all six families, five canonical
  source-palette lineages, separate team Identification Tiles and polymer
  roughness 0.32 / 0.40 / 0.50 under the same fixed neutral lighting.
- A normal exported build exposes the fixture through `F8` → **M7 Material
  Lab** and provides **RETURN TO PROTOTYPE**. The command-line smoke is
  `--m7-material-lab --m7-material-smoke`.
- Exact RGB matching and subjective material response are explicitly draft.
  Game-director review is required for highlight width, black lift, white
  retention, transparent tint and Crystal emission.

## M7 visual-style exploration decision

- By explicit game-director direction, Phase 08 rendering, material, lighting,
  palette and general style conclusions are suspended. No replacement visual
  canon exists during exploration.
- Image generation is rejected as representative evidence: it did not preserve
  scene invariants, produced infeasible detail, and collapsed distinct prompts
  into near-identical polished imagery.
- The code-native **M7 Style Lab** is implemented in Godot using one reproducible
  Blender-authored drill rig: 57 Blender objects, 48 imported mesh nodes and
  7,776 rendered triangles. The same camera, geometry, animation and scene expose
  four switchable Round 2 treatments: Industrial Mass, Heroic RTS, Constructive
  LEGO and Graphic Volume. Only materials, shaders, lighting, VFX and terrain
  change.
- The separate **Palette Ratio Lab** is implemented with six switchable pages:
  faction visible-area candidates, five distinct Martian source families, two
  identical-silhouette 100-panel abstract-model comparisons, transparency
  semantics and the explicit faction light language. Every displayed ratio
  group totals 100%; no common player blue is injected.
- Rock Raiders now gives earth brown 18% of visible vehicle surface. The 7316
  Excavation Searcher now gives tan/beige the dominant 42%, correcting the prior
  part-count-biased read of both palettes.
- Transparent colors are classified semantically. Tinted transparent polymer
  and glass do not emit merely because they are saturated; only authored energy,
  lamps and resources receive emission.
- Authored faction emission is: Rock Raiders neon orange/lime; Mars Mission
  astronauts blue; Mars Mission aliens neon lime; Life on Mars astronauts red
  plus a colorless warm lamp; and Life on Mars Martians red/orange/lime/blue.
- Team/player color remains valuable but is deferred to model-level ownership
  tests and is excluded from faction palette analysis.

The first style review did not select a direction. The game director then
supplied `Docs/Development/M7_ART_DIRECTION_RESEARCH_BRIEF.md`, explicitly as
non-canonical development research. Its center is a volumetric mechanical 3D RTS
with broad readable forms, visible LEGO construction logic, restrained
environmental noise and no photoreal/tabletop premise. Round 2 replaces the old
forward candidates with Industrial Mass, Heroic RTS, Constructive LEGO and
Graphic Volume. Visual canon remains deliberately unlocked.

From a normal build, press `F8` and choose **M7 Style Lab** or **M7 Palette
Lab**. Style controls are `1`–`4`; palette-page controls are `1`–`6`; `Escape`
returns to the playable prototype. The rejected first fixture remains available
as **Old Material Lab** for engineering comparison only.

## Integration format boundary

- authoritative snapshot format **20**;
- simulation protocol **18**;
- replay format **16** (backward reader for 15);
- compiled content format **16** / source schema **15**;
- command packet format **1**;
- recipient snapshot packet format **3**;
- reconnect packet format **1**;
- network replay chunk format **1**.

## Verification state

The current M7 exploration branch passed `./tools/verify.sh --full` on
2026-08-22 with 278 NUnit tests, 24/24 representative mover acceptance, every
T058–T063 ENet smoke, the four-style Godot smoke, all six Palette Ratio Lab
pages, 100-repeat determinism, replay record/playback, snapshot continuation,
compiled-content regeneration and macOS export. Exact summary:
`Artifacts/Verification/20260822T083326Z-full-summary.txt`.

The freshly exported app was launched directly into the 500-panel faction
abstract-model page and the ten-role faction light-language page. Both captured
and emitted their PASS markers from the exported build. Evidence:
`Artifacts/Screenshots/m7-exported-palette-faction-models.png` and
`Artifacts/Screenshots/m7-exported-palette-light-language.png`.

The exported build was also launched directly into all four Round 2 treatments;
each rendered a capture and emitted its PASS marker. Evidence is
`Artifacts/Screenshots/m7-exported-style-industrial-mass.png`,
`m7-exported-style-heroic-rts.png`, `m7-exported-style-constructive-lego.png`
and `m7-exported-style-graphic-volume.png` in the same directory.

Stress60 remained the expected diagnostic failure with phase completion
**4/60, 5/60 and 2/60**. The exported macOS debug build is:
`Builds/macOS/LEGO Space RTS.app`.

## Deferred M9 large-battle gate

The legal stress60 fixture still exposes mid-route corridor traffic/yield
deadlock. The approved immutable endpoints and bounded arrival sequencer solve
the representative 24-mover arrival wall but not this distinct scale case.

Do not resurrect or stack the rejected portal-flow/local-pressure experiments.
Another traffic coordinator/solver attempt requires **ARCHITECTURE REVIEW
REQUIRED**. The benchmark remains a diagnostic through M7–M8 and becomes
blocking only when M9 must prove its stable-large-battle exit.

## Next approved action

1. Game director reviews Industrial Mass, Heroic RTS, Constructive LEGO and
   Graphic Volume; do not record T064 acceptance until one direction is
   explicitly locked.
2. Record the game director's keep/reject/hybrid feedback and implement only the
   resulting narrowed visual branch.
3. Keep Stress60 visible without starting an unreviewed third movement attempt;
   revisit it for M9 or earlier only if a catastrophic movement regression
   appears.
