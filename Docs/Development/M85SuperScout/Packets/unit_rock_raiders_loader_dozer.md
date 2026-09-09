# Loader Dozer — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.loader_dozer`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Heavy utility / frontline
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4950
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
| 4950 — Loader Dozer | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4950)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128419.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4950-1) | PRIMARY_VERIFIED | loader chassis, scoop and defensive derivation |

### Source audit [RockRaiders:4950]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-10: Wide low chassis, rear frame, control station and paired structural side rails.
  - PDF pages 11-20: Full-width bucket, front linkage, high cage, lighting and wheel mounts.
  - PDF pages 21-22: Explicit bucket lift/tilt play feature and rock-loading pose.
- View/mechanism coverage: front=VERIFIED p1 and p11-22; rear=PARTIAL p17-20; leftRight=VERIFIED p2-22 construction sequence; top=VERIFIED p2-20; threeQuarter=VERIFIED p1 and p21-24; undersideInterior=VERIFIED p2-8 chassis build; mechanism=VERIFIED p21-22 bucket lift/tilt linkage
- Verified findings:
  - The bucket is carried by visible side linkages and must remain the dominant forward mass.
  - Four equal large wheels sit outside a broad plate-built chassis.
  - The operator cage is rear-weighted, leaving the front linkage and bucket visually exposed.
- Remaining evidence gaps:
  - Opposite-side product photography is still desirable for exact hose and control placement.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A squat four-wheel loader dominated by a broad articulated scoop.

Non-removable identity anchors:

- full-width front scoop
- four large industrial wheels
- rear-weighted exposed frame

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `building.rock_raiders.crusher_barrier` — The defense is intentionally Loader-derived. Mitigations: Loader Dozer has four readable wheels; Crusher Barrier is planted on side braces. / Loader Dozer carries a movable scoop; Crusher Barrier presents a fixed crusher/ram face. / Loader Dozer has a rear operator frame; Crusher Barrier has no driver silhouette or travel direction.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Full-width bucket — loads rubble and performs the baseline scoop ram — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Side lift/tilt linkages — visibly transfer hydraulic force into the bucket — SOURCE_VERIFIED.
  - Four-wheel chassis and rear cage — support the forward tool while keeping the operator behind it — SOURCE_VERIFIED.
  - Cutter package — replaces the bucket contact face with a rotating anti-light industrial saw after research — CANON_DERIVED_ADAPTATION.
- Structural load path: The broad plate chassis carries four outer wheels; paired side linkages route bucket load to the rear-weighted frame and operator cage.
- Repeated modules / connection grammar: Four repeated wheel modules, mirrored lift arms, bucket/ram module and optional researched cutter module.
- Source-faithful versus adapted boundary: Combat is expressed through source-derived bucket/cutter contact only; no ranged gun or armored tank hull.

## E. Material and texture contract

- Geometry must carry:
  - bucket volume and cutting edge
  - side hydraulic linkage
  - four wheel silhouettes
  - optional cutter disc
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Heavy wheeled ground movement with visible suspension compression and wide steering response.
- Planted/contact rule: All four wheels remain grounded; ram/cut actions settle the chassis before contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WheelFrontLeft` | Asset_LoaderDozer | roll on axle; steering yaw where linkage permits | movement distance and steering |
| `Pivot_WheelFrontRight` | Asset_LoaderDozer | roll on axle; steering yaw where linkage permits | movement distance and steering |
| `Pivot_WheelRearLeft` | Asset_LoaderDozer | roll on axle | movement distance |
| `Pivot_WheelRearRight` | Asset_LoaderDozer | roll on axle | movement distance |
| `Pivot_BucketLift` | Asset_LoaderDozer | source-verified upward/downward linkage arc | clear, load, ram and idle presentation |
| `Pivot_BucketTilt` | Pivot_BucketLift | source-verified scoop/dump pitch arc | load, dump or ram presentation |
| `Pivot_CutterSpin` | Asset_LoaderDozer | roll around cutter axis; absent in baseline loadout | Cutter Package contact state |

- Required beats:
  - Idle hydraulics settle with bucket near ground.
  - Move shows wheel roll and heavy suspension response.
  - Scoop/ram lowers, settles, drives contact, then recoils and recovers.
  - Cutter loadout spins only during committed contact.
  - Damage sags one linkage; destruction drops the bucket before frame breakup.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_BucketContact`, `Socket_CutterContact`, `Socket_ImpactVfx`, `Socket_Lamp`, `Socket_AudioHydraulic`, `Socket_AudioTool`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact hose routing on the unseen side remains a non-silhouette production detail.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
