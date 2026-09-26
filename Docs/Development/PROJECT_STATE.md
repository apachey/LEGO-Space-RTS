# LEGO SPACE RTS — CURRENT PROJECT STATE

The sole dynamic project-status handoff. Canon and newer Git evidence prevail;
this summary distinguishes director acceptance from Git integration.
Historical snapshots linked below are not current instructions.

## Accepted baseline

- Current accepted workflow base: `4858c32` on `origin/codex/m85-t082`,
  merging the completed T083 unit design packages (PR #16) over cleanup PR #15
  and director-accepted T082 `633542a`. Older `origin/main` is not this base.
- T085 work is on `codex/m85-t085`, branched from `4858c32`; not merged.
- Core remains Godot 4.7.1-stable .NET/C#, engine-independent deterministic
  SimCore, fixed 20 Hz and project-owned navigation. No architecture change.

## Current milestone

**M8.5 — Full Content & Presentation Production**, between M8 and M9.
Phase 09C requires production designs/models for 35 units and 31 infrastructure
entries, animation/VFX, icons/portraits, frontend/UI, audio and human acceptance.
The complete M8 data roster alone is not M9 Skirmish Alpha presentation.

## Completed and accepted

- M0–M6: simulation foundation, movement/selection/camera/fog, economy,
  construction/production/capacity/energy, combat/repair/transport/transform,
  faction systems and dedicated-server command/snapshot/reconnect/replay stack.
- M7 production visual direction: **M7 Final, outline on**; accepted default
  opens at 84 cells, with continuous 24–108 zoom and exact 24/44/72 review bands.
  Current/Hybrid are comparison modes. Prototype geometry is not final art.
- M7 material, lighting, animation, VFX/destruction, HUD and legal-minimap
  foundations exist. Style/Palette/Look labs remain investigative tools;
  historical exploration does not reopen the accepted production direction.
- M8 T070–T074: 35 unit chassis, 31 infrastructure definitions, 38 research
  definitions and 35 stable command families with canonical bindings and
  content-reference validation. Approved T073 values are recorded separately.
- M8.5 T083: all 35 unit visual design packages are complete and explicitly
  director-accepted. The acceptance-aware generator preserves accepted package
  artifacts and checks proposal-owned outputs. T083 is merged into
  `codex/m85-t082` via PR #16 (`4858c32`). T084 has not started.
- T081: Blender → GLB → Godot production pipeline is implemented, verified
  and director-accepted; scale, orientation, LODs, pivots/sockets and materials
  have an accepted reference fixture.
- T082: the complete 66-asset source/reference corpus is director-accepted.
  Four faction audits cover 43 source records across 48 official books plus
  one archival scan, with semantic construction/motion/material planning.
- Renewed 24/44/72 diagnostic boards received positive general review.
  There is no per-code 66/66 score; do not manufacture one or repeat that gate.
- T082 accepts research and recognizable reference identity, not 66 final
  production designs/models. T083 now provides the accepted design target for
  each of the 35 units; production models remain a later phase.

## Work in progress

- T085 started on 2026-09-26 on `codex/m85-t085` (not merged). Entry 1 of 31,
  Rock Raiders HQ, has an `ADAPTED` Production Design Target proposal awaiting
  director review: the complete LEGO 4990 base re-seated on one 8×8 rock plinth,
  with a non-firing rotating light beam, Crew pad, receiving hopper and energy
  crystal. It is not accepted; T086 is not started or authorized; no other
  infrastructure entry has started. Official 4990 imagery could not be opened
  in the cloud session (network policy denied LEGO hosts), so module appearance
  stays locked to the official instructions pending a pixel check. Review:
  [HQ director review](M85InfrastructureDesign/Batch01RockRaiders/rock_raiders_hq_director_review_20260926_v1.html),
  registry `M85InfrastructureDesign/registry.json`.
- T083 is complete and merged (PR #16); all 35 unit design packages are
  director-accepted. The accepted targets and unit-specific T084 authorization
  states are recorded in `M85UnitDesign/registry.json`. No production model has
  started. Crew's prior acceptance was reconfirmed on
  2026-09-24. T3-Trike was director-accepted on 2026-09-24: 7312 Life on Mars
  is base Escort; after Field Survey Package research it may refit to the 7694
  Mars Mission Survey appearance. T084 is authorized for this unit only and
  remains unstarted. Rapid Rider uses the official LEGO 4920 appearance with one
  driver; the four-passenger adaptation was rejected, and any extra visible
  passenger/cargo state is deferred.
- Granite Grinder is director-accepted via the official LEGO 4940 SOURCE_LOCKED
  target with a restrained short-step movement adaptation; T084 is authorized
  for this unit only and remains unstarted. Its accepted review records the
  official source target and keeps the source gait gap explicit.
- Loader Dozer is director-accepted via the official LEGO 4950 SOURCE_LOCKED
  target with bucket; T084 is authorized but remains unstarted. The researched
  Cutter Package remains separate, and its fitted appearance is not approved by
  this target.
- Chrome Crusher is director-accepted via the official LEGO 4970 SOURCE_LOCKED
  target with one small flat team marker on the outer central frame. T084 is
  authorized for this unit only and remains unstarted.
- Tunnel Transport is director-accepted via the ADAPTED loaded 4980/Chrome
  Crusher target; T084 is authorized for this unit only and remains unstarted.
- Source/construction/state/material sheets and the local visual review are at
  [Batch 01 review](M85UnitDesign/Batch01RockRaiders/review.html) and
  [Expedition Crew review](M85UnitDesign/Batch02Astronauts/review.html).
- Hover Scout's `SOURCE_LOCKED` Production Design Target is the completed LEGO
  4910 shown at step 7 on official instruction page 1. The target adds no
  exterior geometry; its approved game deltas are low hover height, one brief
  signal from the existing forward scanner/tool assembly, and a small flat
  team marker. The old Batch 01 review card remains preserved; the accepted
  decision is recorded in its separately named target review.
- Rover's `SOURCE_LOCKED` Production Design Target is the completed LEGO 7301
  shown at step 7 on page 1 of official instruction PDF 4156314. Its corrected
  review/spec separate that source target, official evidence, a restrained
  accepted survey pulse and a clearly non-authoritative schematic; rear/hidden
  evidence gaps remain disclosed. Conflicting T082 prose and contracts remain
  unchanged.
- Two appearance proposals remain open: Loader Dozer Cutter and Rapid Rider
  with four Crew. The Tunnel Transport loaded target is accepted. Its Design Spec
  resolves the upper-hanger ambiguity; the raster itself does not prove hidden
  load geometry.
- On 2026-09-24 the director corrected Mono Jet's classification: official
  LEGO 7310 is a ground monowheel trike, not an aircraft. Its canonical movement
  and target layers now use Ground. The director accepted its SOURCE_LOCKED T083
  package on 2026-09-24; T084 remains unstarted.
- Solar Explorer is director-accepted on 2026-09-24 via the accepted LEGO 7315
  spacecraft target. Its central solar/lab module deploys as Forward Service;
  cockpit/nose and rear/engine reconnect axially. T084 is authorized for this
  unit only and remains unstarted.
- Mission Fighter is director-accepted on 2026-09-24 via the official LEGO
  7695 MX-11 Astro Fighter SOURCE_LOCKED target. LEGO 5619 Crystal Hawk is a
  separate model and is not part of this unit's target. T084 is authorized for
  Mission Fighter only and remains unstarted.
- MX-41 Switch Fighter T083 is director-accepted on 2026-09-25 via the official
  LEGO 7647 SOURCE_LOCKED target, covering its flight and completed ground
  configurations. T084 is authorized for MX-41 only and remains unstarted.
- Mobile Mining Platform is director-accepted on 2026-09-25 via its ADAPTED
  Production Design Target: shared tracked chassis, Crystal Reaper twin-wheel
  and Ore Drill single-spiral configurations, two independent manipulators and
  directly docked upper module. T084 is authorized for this unit only and has
  not started.
- MX-71 Recon Dropship was director-accepted on 2026-09-25 via its ADAPTED
  Production Design Target: retain the completed LEGO 7692 carrier, including
  its amber cockpit and compact landing supports, with one permitted payload
  below. T084 is authorized for MX-71 only and remains unstarted; final cargo
  clearance remains a T084 production check.
- MT-51 Claw-Tank was director-accepted on 2026-09-25 via its ADAPTED T083
  Production Design Target, using the director-supplied final visual. T084 is
  authorized for MT-51 only and remains unstarted. MT-101 Armored Drilling
  Unit was director-accepted on 2026-09-25 via its SOURCE_LOCKED T083 target;
  the full source-led appearance includes the attached rear aircraft module.
  The official LEGO 7699 book 2 cover anchors the complete configuration;
  instructions support construction and the nested mini-bike relationship.
  No visible game adaptation is proposed. T084 is authorized for MT-101 only
  and remains unstarted.
  MT-201 Ultra-Drill Walker was director-accepted on 2026-09-25 via its
  SOURCE_LOCKED LEGO 7649 target, retaining the detachable observation shuttle
  as a visual module of this unit. T084 is authorized for MT-201 only and has
  not started. ETX Servitor was director-accepted on 2026-09-25 via its
  `ORIGINAL_EXTENDED` T083 target: low black Alien hover worker with paired
  shell valves, protected support/conduit slot, empty cargo cradle and one
  integrated utility clamp. T084 is authorized for ETX Servitor only and has
  not started. Razor Skimmer was director-accepted on 2026-09-26 via its
  `ORIGINAL_EXTENDED` target: a compact low hover skimmer with two short inward-
  swept prongs around an open V and an exposed lime core. T084 is authorized for
  Razor Skimmer only and has not started; exact LEGO elements and connections
  remain unverified. ETX Alien Strike was director-accepted on 2026-09-26 via its `SOURCE_LOCKED`
  LEGO 7693 target; T084 is authorized for this unit only and remains unstarted.
  ETX Alien Infiltrator is director-accepted via the SOURCE_LOCKED LEGO 7646
  target; T084 is authorized for this unit only and remains unstarted. Alien
  Mothership T083 is director-accepted on 2026-09-26 via the
  SOURCE_LOCKED LEGO 7691 target in both source-shown configurations. Worker
  Robot was accepted as SOURCE_LOCKED to the unchanged LEGO 7302 target. Double
  Hover was director-accepted on 2026-09-26 as the unchanged
  SOURCE_LOCKED LEGO 7300 target with symmetric paired runners and grilles,
  centered rider and rear dish, and controls in front. No visible adaptations.
  T084 is not authorized for Double Hover and remains unstarted. Jet Scooter was director-accepted on 2026-09-26 via the unchanged SOURCE_LOCKED
  LEGO 7303 target. T084 is not authorized for it and remains unstarted.
- Aero Skiff T083 was director-accepted on 2026-09-26 as the symmetric
  `SOURCE_LOCKED` LEGO 1195 appearance, using the high-quality visualization
  based on the director-provided completed-set photo and component-layout
  clarification. Earlier offset/asymmetry and second-seat proposals were
  withdrawn. T084 is not authorized or started; no production model is started.
  No production model is started.
- Red Planet Cruiser T083 was director-accepted on 2026-09-26 via its
  `SOURCE_LOCKED` LEGO 7311 Production Design Target: open cruiser of medium
  visual height, two articulated arms with open interpretation of attachments,
  and director-specified hopping on one leg followed by gliding as visual
  movement direction. Gameplay movement rules are unchanged. Official
  instructions and completed-set views correct the older T082 wide-offset,
  two-stage-pedestal description; the T082 record is preserved. T084 is
  authorized for this unit only and remains unstarted. The review PNG capture is
  still missing; the HTML decision screen and source images remain available.
- Recon-Mech RP T083 was director-accepted on 2026-09-26 via its unchanged
  `SOURCE_LOCKED` completed LEGO 7314 target. No visible adaptations. T084
  remains not started and not authorized. The T082 `drill/lance` wording is
  corrected in this package against the official completed model and secondary
  views; the older T082 packet remains unchanged.
- Workflow cleanup is merged and synchronized locally. This synchronization
  preserves all prior T083 content; it does not continue design or infer approval.
- Alien Jet T083 is director-accepted via its unchanged `SOURCE_LOCKED` LEGO
  5617 Production Design Target. T084 is authorized for Alien Jet only and has
  not started.
- Red Planet Protector T083 is director-accepted via the `SOURCE_LOCKED` LEGO
  7313 appearance and official instruction-cover pose; no visible adaptations.
  T084 remains not started and not authorized.

## Blockers and deferred gates

- T083 director-acceptance gate is complete: all 35 unit packages have explicit
  acceptance records. T084 remains authorized only for units marked in the
  registry; Double Hover, Jet Scooter and Recon-Mech RP are not authorized.
  No production model has started.
- `BLOCKING_LATER`: actual gameplay-camera/model-LOD review belongs to
  T084/T086 after production geometry exists; final animation belongs to T087.
- Martian sources 1195/3750 retain bounded archival evidence gaps. Do not invent
  unseen construction. Preserve their documented later-phase ownership.
- `BLOCKING_LATER — M9 large-battle acceptance`: Stress60 still exposes
  mid-route corridor traffic/yield deadlock. It remains a visible diagnostic
  in full verification before M9, not a blocker for bounded production work.
- `BLOCKING_NOW` for locking the T085 HQ module appearance: an official LEGO
  4990 pixel check (network access to `www.lego.com` or director-supplied
  images). The composition decision itself is not blocked.
- Cloud sessions lack Pillow, so the T082 design-package validator stages fail
  there before checking content; the same failure reproduces on `4858c32`.
- Representative 24-mover behavior remains covered by the NUnit suite.
- Failed portal-flow/local-pressure research (`6105cf0`, `f896b01`) is preserved,
  not production-ready. Another traffic coordinator/solver attempt requires
  `ARCHITECTURE REVIEW REQUIRED`; no third movement attempt is authorized.
- M9 acceptance must wait for the required M8.5 T083–T092 production work.

## Next approved action

The director reviews the T085 Rock Raiders HQ proposal: composition, the
non-firing light beam, and whether to accept the composition before or after
the official 4990 pixel check. Do not start the next infrastructure entry,
T084 or T086 until the director instructs. No production model has started.
Design-only package work uses the targeted `design-package` profile plus the
package validator; no game export, M6 networking or M7 exploration stack.

## Detailed records and historical evidence

- [Workflow and verification profiles](AGENT_WORKFLOW.md).
- [Technical decisions](TECH_DECISION_LOG.md).
- [M7 continuity](M7_THREAD_HANDOFF.md) and
  [accepted visual direction](M7_VISUAL_ACCEPTANCE_CANDIDATE.md).
- [M8 command bindings](M8_T073_BINDING_REVIEW.md).
- [T081 asset pipeline](M85_ASSET_PIPELINE.md).
- [T082 reference foundation](M85_SUPER_SCOUT_REFERENCE_INTELLIGENCE.md),
  [progress/closure detail](M85_SUPER_SCOUT_PROGRESS.md),
  [packet index](M85SuperScout/PACKET_INDEX.md) and
  [renewed blind review](M85_T082_CURRENT_BLIND_REVIEW_V1.md).
- [Complete pre-cleanup PROJECT_STATE](Archive/PROJECT_STATE_2026-09-23_pre_workflow_cleanup.md)
  preserves the exact committed 1,409-line narrative, old results and evidence
  paths from `633542a`. Its older open/hold statements are historical only.
