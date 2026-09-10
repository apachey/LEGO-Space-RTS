# Frontier Extraction Station — T082 Super Scout packet

**Stable ID:** `building.ast.frontier_extraction_station`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Resource processing
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7691, 7648
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
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |
| 7648 — MT-21 Mobile Mining Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7648)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7648-1) | PRIMARY_VERIFIED | mobile mining and detachable support module |

### Source audit [Astronauts:7691]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf)
- Construction map:
  - Evidence pages 3-27: Human extraction station with long rail/sled, upright operator rig, flexible transfer hose and canister.
  - Evidence pages 28-49: Large circular alien mothership body; supporting opposition evidence.
  - Evidence pages 50-68: Alien subcraft and attachment mechanisms; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p17-27 human station; rear=PARTIAL p20-27 human station; leftRight=VERIFIED p3-27; top=VERIFIED p3-27; threeQuarter=VERIFIED p1 and p21-27; undersideInterior=VERIFIED p3-23 staged human station; mechanism=VERIFIED p20-27 hose/canister extraction play; station processing cycle remains partial
- Verified findings:
  - The human station is a narrow linear extraction rig rather than a broad factory.
  - A low rail/sled leads to a small upright operator tower with a visible hose and external material canister.
  - The compact open structure supports a frontier receiver identity but does not justify a sealed processing building.
- Remaining evidence gaps:
  - The exact resource intake, storage and outgoing process for the composite Frontier Extraction Station must be added from other verified Mars Mission mining sources.

### Source audit [Astronauts:7648]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf)
- Construction map:
  - Evidence pages 3-16: Compact orange/white mining rover with exposed low chassis and large wheels.
  - Evidence pages 17-25: Separate tall articulated extraction/tool mast on a small wheeled base.
  - Evidence pages 26-28: Both modules shown together at operator scale.
- View/mechanism coverage: front=PARTIAL p15-28; rear=PARTIAL p15-28; leftRight=VERIFIED p3-28; top=VERIFIED p3-25; threeQuarter=VERIFIED p1 and p25-28; undersideInterior=VERIFIED p3-20 staged chassis; mechanism=PARTIAL p17-25 hinged tool mast; extraction cycle not demonstrated
- Verified findings:
  - The source is a paired mining system: a compact rover and a visibly independent upright tool platform.
  - Both machines use low exposed white frames with orange wheel or equipment masses rather than armored hulls.
  - The tall mast gives the support module a distinct vertical read beside the horizontal rover.
- Remaining evidence gaps:
  - The game combines this source with other mining vehicles, so the retained mini-robot/support-module relationship must be defined without creating an extra buildable unit.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A compact expedition station that visibly receives drilled material and transfers it into sealed mission containers.

Non-removable identity anchors:

- drill-facing receiving hopper
- sealed crystal or ore container rack
- small pneumatic transfer mast

- Blind-review code: `S04`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `building.rock_raiders.ore_processing_plant` — Both are low large resource buildings with a visible receiving and transfer line. Mitigations: Ore Plant feeds an open crusher and sorting stack; Frontier Station feeds sealed mission containers. / Ore Plant's highest mass is an irregular industrial crusher; Frontier Station's highest mass is a compact expedition control module. / Ore Plant leaves processed rock visibly exposed; Frontier Station closes output into standardized container silhouettes.
- `building.mar.excavation_plant` — Both are large planted extraction structures organized around one material channel. Mitigations: Frontier Station leads with a compact drill receiver; Excavation Plant leads with a tall Searcher-derived crane intake. / Frontier Station routes output into sealed containers; Excavation Plant exposes an open rock-transfer path. / Frontier Station stays low and rectilinear; Excavation Plant stands on irregular braces around an elevated handling body.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Drill-facing receiving rail and hopper — expose incoming material flow — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Sealed ore/crystal container rack — makes stored output legible — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Small pneumatic transfer mast with hose — connects intake to containers — SOURCE_VERIFIED.
- Structural load path: A narrow linear foundation carries the intake rail, upright operator mast and external containers along one readable process direction.
- Repeated modules / connection grammar: Receiving sled, hopper, operator mast, transfer hose and container rack remain distinct.
- Source-faithful versus adapted boundary: 7691 proves the narrow station and hose; 7648 supplies material handling. The composition may not become a broad generic refinery.

## E. Material and texture contract

- Geometry must carry:
  - linear receiving rail
  - external container rack
  - upright transfer mast
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: Linear base remains planted with a clear resource-facing intake.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_TransferArm` | Asset_FrontierExtraction | intake-to-container arc | resource processing progress |
| `Pivot_HopperGate` | Asset_FrontierExtraction | closed to receive/release angle | resource handoff progress |

- Required beats:
  - Idle intake remains clear.
  - Receive draws material along the rail.
  - Process transfers it visibly into a container.
  - Damage breaks hose/rack before base collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ResourceReceive`, `Socket_ResourceOutput`, `Socket_TransferVfx`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact 7648 donor module must be selected and disclosed before final modeling.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
