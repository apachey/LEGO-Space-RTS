# Hover Scout — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.hover_scout`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Scout / detector
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 4910
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

### Source audit [RockRaiders:4910]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128290.pdf)
- Construction map:
  - Evidence pages 1: Complete Hover Scout build from flat base plate through open operator deck, front scanner/tool mass and rear equipment.
  - Evidence pages 2: Separate small worksite/scanner station; useful for the Cutter Mast base language, not a reverse view of the Scout.
- View/mechanism coverage: front=PARTIAL p1 cover/final build; rear=PARTIAL p1 final steps; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-7; threeQuarter=VERIFIED p1 cover and steps; undersideInterior=MISSING; mechanism=PARTIAL p1 scanner/tool mounting; no authored movement sequence
- Verified findings:
  - The Scout is an exposed plate-built sled rather than a closed hovercraft.
  - The operator, scanner/tool and rear rack are independent readable masses.
  - The second-page station provides a source-faithful tripod/pedestal vocabulary for later survey-derived infrastructure.
- Remaining evidence gaps:
  - Acquire explicit underside and opposite-side evidence before final modeling.
  - Any animated scanner sweep is an approved presentation interpretation, not proven by the manual.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low one-person survey sled with its scanner and hover deck visible as separate masses.

Non-removable identity anchors:

- flat open hover deck
- forward survey scanner
- exposed seated operator and tool rack

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.rapid_rider` — Both are small low Rock Raider utility craft. Mitigations: Hover Scout has one flat survey deck; Rapid Rider has two parallel hulls. / Hover Scout carries a forward scanner; Rapid Rider carries paired rear propulsion. / Hover Scout reads as single-seat information equipment; Rapid Rider preserves an open passenger/cargo gap.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Flat hover deck — carries the operator close to the ground and communicates fragile speed — SOURCE_VERIFIED.
  - Forward scanner/tool mass — performs geological and concealed-object survey plus the minor pulse origin — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Rear equipment rack — balances the scanner and keeps the source's open utility construction visible — SOURCE_VERIFIED.
- Structural load path: One shallow plate-built spine carries the central seat, forward scanner and rear rack; no enclosed hull may hide their separate mounts.
- Repeated modules / connection grammar: Forward scanner bracket, open operator deck and rear utility rack remain three independently readable modules.
- Source-faithful versus adapted boundary: Ground-layer hover height and scanner sweep are presentation adaptations; underside lift hardware remains unresolved.

## E. Material and texture contract

- Geometry must carry:
  - thin deck edge
  - scanner head
  - open operator gap and rear rack
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Ground hover; fast lateral-stable travel with small terrain-following heave, never true-air banking.
- Planted/contact rule: No wheel contact; a bounded hover datum stays close to the ground and respects ground topology.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ScannerYaw` | Asset_HoverScout | short left/right survey arc around vertical axis; forward rest | passive detector presentation and survey pulse |
| `Pivot_ToolPitch` | Pivot_ScannerYaw | small downward/upward pitch around visible hinge | survey pulse or idle equipment check |

- Required beats:
  - Idle hover settles around one low datum.
  - Move uses restrained heave and rack vibration, not aircraft roll.
  - Survey pulse begins with scanner aim, then signal/VFX pulse and return.
  - Damage destabilizes hover briefly; wreck rests as a low broken sled.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_SurveyPulse`, `Socket_ScannerVfx`, `Socket_Lamp`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Underside lift geometry remains provisional until opposite-side/underside evidence is available; it may not grow into a second hero mass.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
