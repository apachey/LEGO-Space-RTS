# Rover — T082 Super Scout packet

**Stable ID:** `unit.astronauts.rover`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Scout
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 7301
- Current confidence: verified canonical identity and faction-internal construction/motion/material draft; source-bounded decisions remain explicit below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: The faction-internal construction, motion, socket and material draft is recorded below. Its unresolved decisions and the complete-roster silhouette/director gates must be cleared before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 7301 — Rover | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7301)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7301-1) | PRIMARY_VERIFIED | two-wheel human field rover |

### Source audit [Astronauts:7301]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf)
- Construction map:
  - Evidence pages 1: Complete seven-step Rover build, equipment mast and final operator view.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 final view; rear=PARTIAL p1 steps 1-6; leftRight=VERIFIED p1 final and staged construction; top=VERIFIED p1 steps 3-6; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare chassis; mechanism=MISSING static open rover
- Verified findings:
  - The Rover is a tiny open four-wheel platform rather than an enclosed car.
  - Its long forward scanner/tool boom projects beyond the wheels and dominates the side profile.
  - The seated operator, blue equipment box and tall antenna remain exposed above the flat white chassis.
- Remaining evidence gaps:
  - A rear view is still required before final antenna, storage and propulsion placement.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny exposed field rover with two dominant wheels and a narrow forward sensor bar.

Non-removable identity anchors:

- two-wheel bike-like profile
- open rider position
- front sensor and sample rack

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.t3_trike` — Both are rugged Life on Mars wheeled field scouts. Mitigations: Rover has two wheels; T3-Trike has one front and two broad rear wheels. / Rover is tiny and open with no centre module; T3-Trike has a large swappable mission bay. / Rover carries a narrow sensor bar; T3-Trike is defined by high articulated suspension.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Two dominant side wheels — define the bike-like scout stance — SOURCE_VERIFIED.
  - Open central rider frame — keeps the vehicle light and exploratory — SOURCE_VERIFIED.
  - Narrow forward sensor and sample rack — explain passive survey and field collection — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A narrow exposed spine joins the two wheel axles and carries the rider, front sensor and rear sample rack.
- Repeated modules / connection grammar: Mirrored wheel pair, central rider frame, forward sensor and rear samples remain readable.
- Source-faithful versus adapted boundary: Passive survey is adapted from exploration equipment; no enclosed hull or combat turret.

## E. Material and texture contract

- Geometry must carry:
  - two oversized wheels
  - open rider gap
  - forward sensor bar
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_field_frame_surface` — Restrained molded and brushed variation for rugged blue-gray Field Systems frames without faking structural seams. Channels: Tangent-space normal and linear roughness; body color stays parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Field Systems surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Fast two-wheel ground travel with bounded body lean.
- Planted/contact rule: Both wheels track the terrain; idle uses a subtle support settle without inventing a third hero wheel.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WheelLeft` | Asset_Rover | roll on axle | distance traveled |
| `Pivot_WheelRight` | Asset_Rover | roll on axle | distance traveled |
| `Pivot_Sensor` | Asset_Rover | short scan pitch arc | survey presentation |

- Required beats:
  - Idle sensor sweep.
  - Travel with wheel roll and restrained lean.
  - Survey pulse aims, emits and returns.
  - Damage wobbles the exposed frame; wreck preserves both wheels.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_SurveyPulse`, `Socket_Sample`, `Socket_AudioDrive`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The Outrider visual variant remains a later family-selection decision.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
