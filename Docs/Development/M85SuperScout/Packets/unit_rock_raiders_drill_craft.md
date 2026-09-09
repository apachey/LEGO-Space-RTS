# Drill Craft — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.drill_craft`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Engineering utility
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 1277
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
| 1277 — Drill Craft | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/1277)<br>no direct official PDF located | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=1277-1) | CANON_VERIFIED_ARCHIVAL | compact drill silhouette and construction |

### Source audit [RockRaiders:1277]

- Evidence state: `ARCHIVAL_GAP`
- Construction map:
  - No official construction-page range is available.
- View/mechanism coverage: front=MISSING; rear=MISSING; leftRight=MISSING; top=MISSING; threeQuarter=MISSING; undersideInterior=MISSING; mechanism=MISSING
- Verified findings:
  - Canon and archival inventory establish the compact Drill Craft identity and parts family, but they do not establish a modelable load path or verified articulation.
- Remaining evidence gaps:
  - Locate an official instruction scan, official catalog construction view or clearly labeled archival manual before resolving Drill Craft construction.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A tiny work craft whose forward drill occupies more visual weight than its body.

Non-removable identity anchors:

- single dominant forward drill
- compact operator cage
- short industrial wheelbase

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.granite_grinder` — Both use a large forward drill. Mitigations: Drill Craft stays short and wheeled; Granite Grinder is a tall planted biped. / Drill Craft has a compact low cage; Granite Grinder places the operator high above the ground. / Drill Craft is drill-first with minimal body; Granite Grinder has a long boom balanced by rear machinery.

## D. Construction contract

- Contract state: `SOURCE_BOUNDED_PROVISIONAL`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Oversized forward drill — excavates authored terrain and performs short-range structure contact — CANON_DERIVED_ADAPTATION.
  - Compact operator cage — keeps the machine visibly a small work craft — CANON_DERIVED_ADAPTATION.
  - Short wheel chassis — brings the drill into contact without turning the asset into a miniature tank — SOURCE_BOUNDED_PROVISIONAL.
- Structural load path: A short central frame must carry drill thrust back into the wheel chassis and brace the open operator cage behind the tool.
- Repeated modules / connection grammar: Drill/tool boom, compact cage and wheel/contact chassis; exact repeated wheel modules are not yet source-proven.
- Source-faithful versus adapted boundary: The missing 1277 construction source blocks final wheel count, axle positions, drill support and rear counterweight geometry.

## E. Material and texture contract

- Geometry must carry:
  - drill helix and tip
  - operator cage opening
  - wheel/contact silhouette
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Wheeled ground movement with quick steering; final wheel rotation pivots wait for source confirmation.
- Planted/contact rule: All authored wheels remain grounded; excavation settles the chassis before drill contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_DrillFeed` | Asset_DrillCraft | short source-bounded forward feed along drill axis | authoritative excavation/contact state; exact travel pending source |
| `Pivot_DrillSpin` | Pivot_DrillFeed | continuous roll around tool axis; stopped rest | excavation or contact-attack presentation progress |

- Required beats:
  - Idle tool check without continuous drill spin.
  - Move keeps drill still and visibly clear of the ground.
  - Excavate settles, feeds and spins the drill, then retracts after the authoritative result.
  - Damage stops tool motion; wreck preserves the drill/cage relationship.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_DrillContact`, `Socket_DrillDust`, `Socket_DrillSparks`, `Socket_AudioDrill`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Do not lock the production chassis or wheel pivots until official/archival 1277 construction evidence is found or the game director approves a clearly labeled reconstruction.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
