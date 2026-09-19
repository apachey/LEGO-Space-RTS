# T082 — open completed-appearance batch after Claw, MT-101 and Crystal Reaper acceptance

Opened 2026-09-13; updated 2026-09-20. Branch: `codex/m85-t082`, not merged.
Canon impact: NONE.

## What changed / why

The director's `+` to `038fc8a` accepts only the displayed Claw-Tank second
localized correction for comparative appearance review. Exact image bytes and
authority are recorded in `Content/Presentation/SuperScout/claw_tank_appearance_review.json`.
Its generation-time unreviewed manifest is historical, not rewritten.
No extra retry or approval of MT-61/other assets is inferred.

The current gallery marks the Claw, MT-101 and source-rebuilt Crystal Reaper
images accepted and removes them from priority-pending. The priority filter now
contains the three remaining completed correction candidates below. This changes review
priority, not the roster or gameplay. All 66 identities and 67 views remain;
Crystal Reaper alone was rebuilt from its official 7645 source pages in this
continuation.

## Completed appearances for review

These are completed current-gallery appearances, not primitive blockouts or
claims of production approval. Review source resemblance and visible tool/body
composition; dimensions do not need reapproval.

### 1. Mobile Mining Platform — Crystal Reaper configuration

Specialized remote resource extraction. Source-rebuild Rev2 is explicitly
accepted for comparative appearance review. It has two harvesting cutters, two
separate manipulators and a directly docked upper craft, not a trailer. Canon
combines 7645/7648/the human 7693 miner in one configurable family; Ore Drill
appearance and Service-controlled Mission Refit remain open.

![Crystal Reaper](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/MobileMiningPlatformSourceRebuildV1/mobile_mining_platform_source_rebuild_rev2.png)

SHA256: `9011581e8d388e4f04b8a044d4a39f23f695ae2899252b66684e48b9e2311f1d`.

### 2. ETX Alien Strike

Structure assault from the air, with mobile attack support. One continuous
7693 flying crescent craft; no legs, planted braces, ground mode or siege
deployment. Phase 02A biomechanical identity remains applicable; this exterior
raster does not establish internal anatomy or final operator production fidelity.

![Alien Strike](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_aliens_etx_alien_strike_rev2.png)

SHA256: `60e3ccabf7f765062723746b287cd180a0e7a1fbac99b6de3071d0202535b686`.

### 3. Jet Scooter

Tiny fast Martian ground-hover attacker for anti-light harassment and rapid
reinforcement. 7303's open rider, narrow sled and side tubes, with the requested
lower/sleeker body. Aero Tube eligibility is already gameplay canon, not a new
capability inferred from this image.

![Jet Scooter](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_martians_jet_scooter_rev2.png)

SHA256: `96cf56cadc8083d6eb074ce530de994d4f4feebbd6a602250f24fd1820e9a788`.

### 4. Red Planet Protector

Martian anti-heavy positional defense, protecting Tube exits and objectives.
7313's wedge upper craft, two-foot lower body and unequal top-mounted emitter
booms. Protector Stance is an existing gameplay obligation; the image does not
prove its continuous transition or a source toy rebuild as an in-game mechanism.

![Red Planet Protector](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_martians_red_planet_protector_rev3.png)

SHA256: `4dd1481384b55dd77848fce313b0cdb9936c7e27ab34e951d662d57503553be9`.

## Scope / risks / next step

Crystal Reaper Rev2 is `DIRECTOR_ACCEPTED_APPEARANCE_FOR_COMPARATIVE_REVIEW`.
Alien Strike, Jet Scooter and Red Planet Protector remain
`UNREVIEWED_CORRECTION_CANDIDATE`. The Crystal Reaper `+` does not accept its
Ore Drill or Mission Refit configurations, production topology, gameplay,
Mothership source corrections, archival gaps or the full T082 gate.

Continuation 2026-09-19: MT-101 source-rebuild Rev5 is accepted for comparative
appearance review; its exact scope is recorded separately and does not approve
production or independent nested-module gameplay. Continuation 2026-09-20:
Crystal Reaper source-rebuild Rev2 is accepted under the same comparative-only
boundary. No approval of the three remaining images is inferred.

## Files / automated verification / regression / build

Separate Claw appearance-review record, gallery selection/generator and review
guards plus current notes. Four new negative guards reject missing latest
authority, production promotion, another-asset acceptance and hidden production
gates. The prior 61 cases remain unchanged.
Focused validator, 65/65 review guards and gallery regeneration: PASS.
`./tools/verify.sh`: PASS 23 fast blocking stages, 317 NUnit tests and 65 review
guards, zero blocking/diagnostic failures, at
`Artifacts/Verification/20260913T190246Z-fast-summary.txt`. This result predates
the MT-101 finishing changes; their verification is recorded in its separate note.
No playable export, runtime/model integration, dependency or Canon change.
