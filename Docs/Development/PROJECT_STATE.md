# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M4 and M5 are implemented, game-director accepted and merged to
`origin/main` through PR #12 (`85b02f2`). M6 T058 — network transport host —
and T059 — server command validation — are implemented on stacked task
branches through `codex/m6-t059-command-validation`.**

- The task branch includes the verified post-M5 movement handoff from
  `44e2caf`.
- The preserved 60-mover stress is `BLOCKING_LATER — M6 acceptance`; it does
  not block bounded M6 implementation.
- The old `codex/60-mover-fix` work is preserved as failed research through
  commits `6105cf0` and `f896b01`; it is not merge-ready production code.

## Locked technical foundation

- Godot 4.7.1-stable .NET host with C#.
- Engine-independent deterministic SimCore at fixed 20 Hz.
- Fix32, FixVec2 and Angle16 authoritative numerics.
- Deterministic command execution, snapshots, replay and state hashing.
- Project-owned deterministic navigation and movement.
- Dedicated-server authoritative multiplayer; Godot networking is packet
  transport only and does not own gameplay truth.

## Accepted gameplay baseline

- M2 selection, controls, camera, fog/vision and deterministic movement.
- M3 resources, construction, production, Operations Capacity, Energy Domains,
  brownouts and Basic HUD.
- M4 combat, armor, destruction, repair, Rapid Rider transport and MX-41
  tactical transformation.
- M5 Worksites, excavation topology, forward service, Mission Refit, Alien
  Charge/Surge and tubes, displacement/stability and clamp passage.
- Repeatable developer-prepared M4/M5 acceptance controls remain available.

## M6 T058–T059 server authority foundation

The Godot host now supports a headless dedicated-server entry:

`--dedicated-server --network-bind <address> --network-port <port>`

T058 provides:

- `ENetMultiplayerPeer` over UDP as a raw packet carrier;
- a hard maximum of two connected clients;
- explicit peer connection/disconnection tracking and targeted sends;
- reliable-ordered, unreliable-sequenced and reliable-bulk logical channels;
- a 64 KiB carrier packet ceiling;
- a normal blocking loopback smoke that connects two clients and exchanges
  targeted packets through all three channels.

T059 now layers project-owned command authority over that carrier:

- the dedicated entry loads the authoritative scenario and advances SimCore at
  a fixed 20 Hz;
- each connected peer receives a server-created player slot and cryptographically
  random session token;
- reliable command packets carry intent only and use explicit packet format and
  simulation-protocol versions with a 4096-byte request bound;
- the server enforces token/player binding, exactly-next sequence processing,
  a bounded per-tick rate limit, sorted unique entity IDs, ownership, entity and
  command eligibility, fog/legal target knowledge and the currently implemented
  resource, Energy/Charge, technology, placement and service checks;
- accepted intent is rebuilt with the server player slot and next legal
  simulation tick, then enters the existing deterministic `CommandBuffer`;
- every decodable request receives an accepted/rejected acknowledgment with the
  client sequence, execution tick and deterministic rejection code;
- debug/developer command families are never accepted from a network session.

T059 does **not** publish snapshots, delta baselines or client state. Fog-filtered
replication, reconnect and network replay remain T060–T063.

## Integration format boundary

The accepted post-M5 baseline remains:

- snapshot format **20**;
- simulation protocol **18**;
- replay format **15**;
- compiled content format **16** / source schema **15**.

T059 adds network command packet format **1** without changing offline
`CommandEnvelope`, snapshot, replay or content formats.

## Verification state

The accepted combined M4/M5 baseline passed `./tools/verify.sh --full` with 247
tests, determinism/replay/snapshot gates, Godot smoke and macOS export. The
pre-M6 movement handoff then passed the corrected full harness with 256 tests;
stress60 remained the classified diagnostic failure at 4/60, 5/60 and 2/60
phase completion.

T058 passed `./tools/verify.sh --full` on 2026-08-20 with every current
blocking stage green: 256 NUnit tests, honest 24/24 movement acceptance, the
new two-client ENet transport smoke, 100-repeat determinism, replay, snapshot
continuation, compiled-content regeneration and macOS export. The exact summary
is `Artifacts/Verification/20260820T153920Z-full-summary.txt`.

T059 passed `./tools/verify.sh --full` on 2026-08-20 with every current blocking
stage green: 263 NUnit tests, honest 24/24 movement acceptance, both two-client
ENet smokes, 100-repeat determinism, replay, snapshot continuation, compiled
content regeneration and macOS export. The authority smoke proves three legal
commands execute and six hostile/invalid inputs are rejected across real ENet
connections. The exact summary is
`Artifacts/Verification/20260820T162113Z-full-summary.txt`.

Stress60 remained the expected `BLOCKING_LATER` diagnostic failure at 4/60,
5/60 and 2/60 phase completion. The exported macOS debug build is
`Builds/macOS/LEGO Space RTS.app`.

## Movement acceptance blocker

The legal stress60 fixture still exposes mid-route corridor traffic/yield
deadlock. The approved immutable endpoints and bounded arrival sequencer fix the
representative 24-mover arrival wall, but do not solve this distinct scale case.

Do not resurrect or stack the failed portal-flow/local-pressure experiments.
Another movement coordinator/solver attempt requires `ARCHITECTURE REVIEW
REQUIRED`. Stress60 must pass before M6 networked 1v1 acceptance unless canon
explicitly changes the gate.

## Next approved action

1. Hand the verified stacked T059 branch to the game director for review and
   merge with its T058 dependency.
2. After T059 is accepted on the project baseline, begin **T060 — snapshot
   replication** at the canonical 10 Hz. Preserve the T059 authority boundary;
   do not pull fog filtering (T061) or reconnect (T062) into T060.
3. Keep stress60 visible as `BLOCKING_LATER` throughout M6 development and
   promote it to `BLOCKING_NOW` for M6 acceptance.
