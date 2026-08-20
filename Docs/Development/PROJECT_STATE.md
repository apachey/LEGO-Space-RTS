# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M0–M6 are implemented, verified and game-director accepted. The complete M6
integration state contains T058–T063 and the accepted post-M5 movement handoff.
M7 T064 is the next approved implementation task.**

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

The complete T060–T063 code passed `./tools/verify.sh --full` on 2026-08-20
with 278 NUnit tests, 24/24 representative movement acceptance, every T058–T063
ENet smoke, 100-repeat determinism, replay record/playback, snapshot
continuation, compiled-content regeneration and macOS export. Exact summary:
`Artifacts/Verification/20260820T174836Z-full-summary.txt`.

The preceding `./tools/verify.sh` fast run also passed all 278 tests and every
T058–T063 network gate. Exact summary:
`Artifacts/Verification/20260820T171352Z-fast-summary.txt`.

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

1. Begin M7 T064 from the accepted post-M6 baseline.
2. Keep Stress60 visible without starting an unreviewed third movement attempt;
   revisit it for M9 or earlier only if a catastrophic movement regression
   appears.
