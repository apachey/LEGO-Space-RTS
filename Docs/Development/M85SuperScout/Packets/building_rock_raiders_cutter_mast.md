# Cutter Mast — T082 Super Scout packet

**Stable ID:** `building.rock_raiders.cutter_mast`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Infrastructure`
- Gameplay role: Anti-air defense
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 4910, 4990
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
| 4910 — The Hover Scout | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4910)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128290.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4910-1) | PRIMARY_VERIFIED | hover scout construction, palette and equipment |
| 4990 — Rock Raiders HQ | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4990)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4990-1) | PRIMARY_VERIFIED | industrial architecture, crane, processing and service motifs |

### Source audit [RockRaiders:4910]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1: Complete Hover Scout build from flat base plate through open operator deck, front scanner/tool mass and rear equipment.
  - PDF pages 2: Separate small worksite/scanner station; useful for the Cutter Mast base language, not a reverse view of the Scout.
- View/mechanism coverage: front=PARTIAL p1 cover/final build; rear=PARTIAL p1 final steps; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-7; threeQuarter=VERIFIED p1 cover and steps; undersideInterior=MISSING; mechanism=PARTIAL p1 scanner/tool mounting; no authored movement sequence
- Verified findings:
  - The Scout is an exposed plate-built sled rather than a closed hovercraft.
  - The operator, scanner/tool and rear rack are independent readable masses.
  - The second-page station provides a source-faithful tripod/pedestal vocabulary for later survey-derived infrastructure.
- Remaining evidence gaps:
  - Acquire explicit underside and opposite-side evidence before final modeling.
  - Any animated scanner sweep is an approved presentation interpretation, not proven by the manual.

### Source audit [RockRaiders:4990]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-7: Small work vehicles, crystal handling and compact workstation modules.
  - PDF pages 8-10: Tall illuminated machinery/power tower with open service access.
  - PDF pages 11-19: Long articulated crane/tool boom mounted to the tower and used for rock handling.
  - PDF pages 20-29: Open vehicle-width gantry/workshop with sloped supports, lamps and overhead rails.
  - PDF pages 30-34: Conveyor/processing module attaches to the gantry and completes a visible material route.
  - PDF pages 35-40: Modules connect across an irregular rock worksite base rather than a sealed building shell.
  - PDF pages 41-43: Final product photography supplies overall skyline, module spacing and worksite context.
- View/mechanism coverage: front=VERIFIED p41-43; rear=PARTIAL p35-43; leftRight=VERIFIED p35-43; top=VERIFIED p35-40; threeQuarter=VERIFIED p1 and p41-43; undersideInterior=VERIFIED p2-40 staged module and base construction; mechanism=VERIFIED p11-19 crane/tool boom; PARTIAL p20-34 gantry/conveyor service path
- Verified findings:
  - HQ identity comes from a loose network of independently readable work modules on an uneven base, not from a single enclosed headquarters block.
  - Tower, articulated crane, open vehicle gantry and conveyor/processing path establish the reusable infrastructure grammar.
  - Entrances and service lanes remain physically open and minifigure/vehicle scaled.
- Remaining evidence gaps:
  - The manual supports modular industrial functions but does not assign the game's exact Ore Plant, Power Station, Service Bay or Workshop boundaries; those remain explicit canonical adaptations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A tall survey-and-cutting mast whose tracking head reads as repurposed worksite equipment.

Non-removable identity anchors:

- thin industrial mast
- scanner-cutter tracking head
- tripod service base with work light

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `building.mar.aero_guard_tower` — Both are small tall anti-air structures. Mitigations: Cutter Mast is a thin industrial tripod; Aero Guard Tower is an open stacked Martian platform. / Cutter Mast has one scanner-cutter head; Aero Guard Tower has paired articulated tracking arms. / Cutter Mast carries a warm work light; Aero Guard Tower exposes blue/sand-red mechanics and a Tube connection.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Thin industrial mast — elevates the anti-air mechanism without reading as a missile tower — CANON_DERIVED_ADAPTATION.
  - Scanner-cutter head — tracks true-air targets and emits the precision cutting beam — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Tripod service base — adapts the 4910 station vocabulary into a planted worksite defense — SOURCE_VERIFIED.
  - Warm work lamp/signal — communicates powered tracking state separately from the cutter beam — CANON_DERIVED_ADAPTATION.
- Structural load path: A wide tripod/service base carries a narrow segmented mast; the tracking head sits on explicit yaw and pitch bearings with power routed down the mast.
- Repeated modules / connection grammar: Tripod base, mast segments, yaw carriage, pitch cutter/scanner head, work lamp and service box.
- Source-faithful versus adapted boundary: Approved new content uses survey/cutter grammar; it may not gain missile pods, a conventional gun barrel or ground-target behavior.

## E. Material and texture contract

- Geometry must carry:
  - tripod footprint
  - thin segmented mast
  - scanner-cutter head and bearings
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Static infrastructure.
- Planted/contact rule: Three broad service feet remain fixed; upper tracking recoil cannot move the base.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CutterYaw` | Asset_CutterMast | continuous bounded/unbounded yaw as allowed by cable design; forward service rest | authoritative air-target bearing |
| `Pivot_CutterPitch` | Pivot_CutterYaw | upward pitch from service rest to tracked elevation | authoritative air-target elevation |

- Required beats:
  - Idle performs a slow source-bounded survey check, then returns to rest.
  - Acquire yaws first, pitches second and changes the tracking signal.
  - Attack holds aim while the beam window plays, then cools and settles; gameplay damage is authoritative.
  - Brownout parks the head and extinguishes active signal; destruction folds mast sections in sequence.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_CutterBeam`, `Socket_TrackingSignal`, `Socket_Lamp`, `Socket_Service`, `Socket_AudioTracking`, `Socket_AudioCutter`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Cable-safe yaw range and exact head proportions require greybox review; the visible consequence is whether the mast can track behind itself without impossible hose twisting.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
