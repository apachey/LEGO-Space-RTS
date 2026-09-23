# Vehicle Service Bay — T082 Super Scout packet

**Stable ID:** `building.rock_raiders.vehicle_service_bay`

**Packet state:** `T082 REFERENCE FOUNDATION ACCEPTED — T083/T085 DESIGN PENDING`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Infrastructure`
- Gameplay role: Light production / repair
- Authoritative footprint: `Huge`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 4990
- Current confidence: verified canonical identity and faction-internal construction/motion/material draft; source-bounded decisions remain explicit below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: The faction-internal construction, motion, socket and material draft is recorded below. Its unresolved asset-specific choices belong to T083/T085 design review; T082 corpus acceptance does not approve a final visual design or model.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 4990 — Rock Raiders HQ | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4990)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4990-1) | PRIMARY_VERIFIED | industrial architecture, crane, processing and service motifs |

### Source audit [RockRaiders:4990]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf)
- Construction map:
  - Evidence pages 2-7: Small work vehicles, crystal handling and compact workstation modules.
  - Evidence pages 8-10: Tall illuminated machinery/power tower with open service access.
  - Evidence pages 11-19: Long articulated crane/tool boom mounted to the tower and used for rock handling.
  - Evidence pages 20-29: Open vehicle-width gantry/workshop with sloped supports, lamps and overhead rails.
  - Evidence pages 30-34: Conveyor/processing module attaches to the gantry and completes a visible material route.
  - Evidence pages 35-40: Modules connect across an irregular rock worksite base rather than a sealed building shell.
  - Evidence pages 41-43: Final product photography supplies overall skyline, module spacing and worksite context.
- View/mechanism coverage: front=VERIFIED p41-43; rear=PARTIAL p35-43; leftRight=VERIFIED p35-43; top=VERIFIED p35-40; threeQuarter=VERIFIED p1 and p41-43; undersideInterior=VERIFIED p2-40 staged module and base construction; mechanism=VERIFIED p11-19 crane/tool boom; PARTIAL p20-34 gantry/conveyor service path
- Verified findings:
  - HQ identity comes from a loose network of independently readable work modules on an uneven base, not from a single enclosed headquarters block.
  - Tower, articulated crane, open vehicle gantry and conveyor/processing path establish the reusable infrastructure grammar.
  - Entrances and service lanes remain physically open and minifigure/vehicle scaled.
- Remaining evidence gaps:
  - The manual supports modular industrial functions but does not assign the game's exact Ore Plant, Power Station, Service Bay or Workshop boundaries; those remain explicit canonical adaptations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** An open drive-through gantry whose repair arms frame a clearly usable vehicle space.

Non-removable identity anchors:

- open vehicle-width service lane
- overhead repair gantry
- tool and spare-part racks

- Rejected V1 blind-review code: `S60`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `building.rock_raiders.engineering_workshop` — Both are huge open industrial production buildings. Mitigations: Service Bay has a low drive-through lane; Workshop has a tall heavy-machine exit. / Service Bay frames the lane with repair arms; Workshop is dominated by a drill/lift assembly rig. / Service Bay exposes spare racks at crew height; Workshop uses reinforced asymmetrical load-bearing frames.
- `building.ast.field_systems_garage` — The draft sheet exposes two huge low open repair gantries around a vehicle lane. Mitigations: Raider Service Bay uses paired overhead repair arms; Field Garage uses unequal side racks and rugged modules. / Raider Service Bay is a straight drive-through frame; Field Garage shapes its exit around oversized field wheels. / Raider Service Bay concentrates its skyline in the gantry; Field Garage stays lower and spreads storage mass laterally.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Open vehicle-width lane — accepts light/utility vehicles for production and repair — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Overhead repair gantry — carries tools above the vehicle while preserving an unobstructed exit — SOURCE_VERIFIED.
  - Tool and spare-part racks — communicate servicing rather than a generic factory shell — SOURCE_VERIFIED.
  - Worksite service interface — establishes the canonical local service zone — CANON_DERIVED_ADAPTATION.
- Structural load path: Two braced side frames carry an overhead gantry around a clear drive-through floor; racks and service machinery stay outside the exit envelope.
- Repeated modules / connection grammar: Drive-through lane, overhead tool bridge, paired service arms, parts racks and worksite service node.
- Source-faithful versus adapted boundary: The 4990 gantry language is split into a dedicated light-production/repair facility without closing the workshop into a hangar.

## E. Material and texture contract

- Geometry must carry:
  - clear service lane
  - gantry rails and carriage
  - repair arms
  - parts racks
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Static infrastructure.
- Planted/contact rule: Side frames anchor outside the vehicle lane; no support crosses the production exit.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_GantryTravel` | Asset_VehicleServiceBay | bounded fore/aft path along service lane | production or repair progress |
| `Pivot_RepairArmLeft` | Pivot_GantryTravel | hinged reach from stow to vehicle contact envelope | repair/assembly progress |
| `Pivot_RepairArmRight` | Pivot_GantryTravel | hinged reach from stow to vehicle contact envelope | repair/assembly progress |

- Required beats:
  - Idle tools stow clear of the lane.
  - Production assembles visible chassis/tool modules then releases the vehicle through the open exit.
  - Repair settles the target, positions gantry and cycles contact tools.
  - Brownout parks powered tools safely; destruction drops gantry sections away from the lane where possible.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Interaction`, `Socket_ServiceTarget`, `Socket_ProductionExit`, `Socket_Worksite`, `Socket_RepairVfxLeft`, `Socket_RepairVfxRight`, `Socket_AudioGantry`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final bay width and gantry travel derive from the canonical Small/Medium produced roster and require the later scale lineup.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Record named pivots, contacts and sockets; real gameplay-camera validation belongs to T084/T086 after production models exist.
4. Author only the specified reusable textures after human material review.
5. Use the accepted T082 reference foundation to prepare the T083/T085 visual design package. Do not repeat the completed three-scale review without a specific identity defect.

**State:** `T082_REFERENCE_FOUNDATION_ACCEPTED_DESIGN_PENDING`

**Approving reviewer:** game director accepted the complete T082 reference corpus on 2026-09-23; asset-specific design and production remain unapproved.
