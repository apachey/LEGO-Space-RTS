# Crusher Barrier — T082 Super Scout packet

**Stable ID:** `building.rock_raiders.crusher_barrier`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Infrastructure`
- Gameplay role: Anti-ground defense
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
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
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128419.pdf)
- Construction map:
  - Evidence pages 2-10: Wide low chassis, rear frame, control station and paired structural side rails.
  - Evidence pages 11-20: Full-width bucket, front linkage, high cage, lighting and wheel mounts.
  - Evidence pages 21-22: Explicit bucket lift/tilt play feature and rock-loading pose.
- View/mechanism coverage: front=VERIFIED p1 and p11-22; rear=PARTIAL p17-20; leftRight=VERIFIED p2-22 construction sequence; top=VERIFIED p2-20; threeQuarter=VERIFIED p1 and p21-24; undersideInterior=VERIFIED p2-8 chassis build; mechanism=VERIFIED p21-22 bucket lift/tilt linkage
- Verified findings:
  - The bucket is carried by visible side linkages and must remain the dominant forward mass.
  - Four equal large wheels sit outside a broad plate-built chassis.
  - The operator cage is rear-weighted, leaving the front linkage and bucket visually exposed.
- Remaining evidence gaps:
  - Opposite-side product photography is still desirable for exact hose and control placement.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low defensive barricade made from loader machinery rather than a conventional gun turret.

Non-removable identity anchors:

- ground-hugging reinforced barrier
- loader-derived ram or crusher face
- side hydraulic braces

- Rejected V1 blind-review code: `S07`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.loader_dozer` — The defense is intentionally Loader-derived. Mitigations: Loader Dozer has four readable wheels; Crusher Barrier is planted on side braces. / Loader Dozer carries a movable scoop; Crusher Barrier presents a fixed crusher/ram face. / Loader Dozer has a rear operator frame; Crusher Barrier has no driver silhouette or travel direction.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Low reinforced barrier body — physically blocks and controls a route — CANON_DERIVED_ADAPTATION.
  - Loader-derived crusher/ram face — batters nearby light ground targets through contact — CANON_DERIVED_ADAPTATION.
  - Side hydraulic braces — transmit impact into the ground and distinguish it from a wall prop — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A ground-hugging crossbeam carries the ram face between two wide side braces; contact force routes backward into visible foundation pads.
- Repeated modules / connection grammar: Barrier crossbeam, crusher/ram face, mirrored hydraulic braces and protected service box.
- Source-faithful versus adapted boundary: This is approved new content derived from Loader mechanics; no turret, projectile or bunker enclosure may be added.

## E. Material and texture contract

- Geometry must carry:
  - low barrier thickness
  - ram/scoop face
  - hydraulic braces and foundation pads
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Static infrastructure.
- Planted/contact rule: Both side braces and central base remain planted throughout the contact cycle.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CrusherFace` | Asset_CrusherBarrier | short horizontal shove or hinged scoop arc from guarded rest to contact extreme | authoritative anti-ground attack progress |
| `Pivot_BraceLeft` | Asset_CrusherBarrier | small compression at visible hydraulic joint | contact/recoil presentation |
| `Pivot_BraceRight` | Asset_CrusherBarrier | small compression at visible hydraulic joint | contact/recoil presentation |

- Required beats:
  - Idle keeps the ram face guarded and lane-blocking.
  - Attack anticipates with brace compression, strikes once, recoils and settles.
  - Brownout stops active striking while the physical barrier remains.
  - Damage bends one brace; destruction breaks the ram face from the planted base.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_RamContact`, `Socket_ImpactVfx`, `Socket_Service`, `Socket_AudioHydraulic`, `Socket_AudioImpact`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Greybox review must choose horizontal shove versus short scoop arc based on silhouette clarity; both preserve the same canon contact behavior.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
