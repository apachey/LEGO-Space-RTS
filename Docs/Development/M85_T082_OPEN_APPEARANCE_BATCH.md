# T082 — open completed-appearance batch after Claw, MT-101, Crystal Reaper and Alien Strike acceptance

Opened 2026-09-13; updated 2026-09-20. Branch: `codex/m85-t082`, not merged.
Canon impact: NONE.

## What changed / why

The director's `+` to `038fc8a` accepts only the displayed Claw-Tank second
localized correction for comparative appearance review. Exact image bytes and
authority are recorded in `Content/Presentation/SuperScout/claw_tank_appearance_review.json`.
Its generation-time unreviewed manifest is historical, not rewritten.
No extra retry or approval of MT-61/other assets is inferred.

The current gallery marks the Claw, MT-101, source-rebuilt Crystal Reaper and
source-rebuilt ETX Alien Strike images accepted and removes them from
priority-pending. The priority filter now contains only Jet Scooter and Red
Planet Protector. This changes review priority, not the roster or gameplay. All
66 identities and 67 views remain.

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
deployment. Source-rebuild Rev6 is explicitly accepted for comparative
appearance review. Its two planar articulated wing chains contain four thick
quarter-annulus shells, with the compact central keel, exposed small Alien,
paired nose rails, forward emitter and layered tail blades still readable.
Phase 02A biomechanical identity remains applicable; this exterior raster does
not establish internal anatomy, exact hidden hinge fit or final production
fidelity. Intermediate instruction steps are retained only as historical
construction evidence, never as final-silhouette authority.

![Alien Strike](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/ETXAlienStrikeSourceRebuildV1/etx_alien_strike_source_rebuild_rev6.png)

SHA256: `0eaeb78c30af354da2ca24c21822138a76cf18022ba68e9118d432ab171bed4b`.

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

Crystal Reaper Rev2 and ETX Alien Strike Rev6 are
`DIRECTOR_ACCEPTED_APPEARANCE_FOR_COMPARATIVE_REVIEW`. Jet Scooter and Red
Planet Protector remain `UNREVIEWED_CORRECTION_CANDIDATE`. Neither approval
accepts production topology, gameplay, Mothership source corrections, archival
gaps or the full T082 gate.

Continuation 2026-09-19: MT-101 source-rebuild Rev5 is accepted for comparative
appearance review; its exact scope is recorded separately and does not approve
production or independent nested-module gameplay. Continuation 2026-09-20:
Crystal Reaper source-rebuild Rev2 is accepted under the same comparative-only
boundary. ETX Alien Strike source-rebuild Rev6 is now accepted under that same
boundary; no ground/siege state, internal anatomy, exact topology, flight-height
presentation or production integration is inferred. Jet Scooter is next,
followed by Red Planet Protector.

## Files / automated verification / regression / build

The ETX acceptance adds its immutable six-attempt source record, selected Rev6
review record, current-gallery replacement and exact source/authority guards.
The guards lock the accepted bytes, completed official cover and PDF lineage,
reject Revs1–5, prohibit a ground/siege state and prevent intermediate
instruction steps from becoming final-appearance authority.
Focused validator, 119/119 review guards, byte-exact gallery regeneration and
`git diff --check`: PASS. `./tools/verify.sh --full`: PASS every
`BLOCKING_NOW` stage, including 317 NUnit tests, the 119 review guards,
byte-identical Blender regeneration and macOS export; the preserved 60-mover
M9 stress case remains the expected `BLOCKING_LATER` diagnostic failure.
Summary: `Artifacts/Verification/20260920T060359Z-full-summary.txt`.
The debug build was refreshed at `Builds/macOS/LEGO Space RTS.app`; ETX itself
is not integrated into runtime presentation. No dependency or Canon change.
