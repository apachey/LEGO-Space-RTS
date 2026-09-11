# Excavation Plant — T082 Super Scout packet

**Stable ID:** `building.mar.excavation_plant`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Infrastructure`
- Gameplay role: Resource processing
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7316
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
| 7316 — Excavation Searcher | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7316)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130811.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7316-1) | PRIMARY_VERIFIED | multi-leg excavation, crane and material handling |

### Source audit [Martians:7316]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130811.pdf)
- Construction map:
  - Evidence pages 2-16: Forward tan/orange Excavation Searcher module with long drill, paired claws and separately built planted tool/leg assemblies.
  - Evidence pages 17-30: Large irregular rear body, multiple spaced legs and tall articulated crane assemble, then dock to the forward module.
  - Evidence pages 31-33: Independent low material sled/container builds and docks beneath the complete Searcher.
  - Evidence pages 34-57: Separate dark excavation support rig with arches, hoses and human operator; useful opposition/industrial evidence, not direct Martian Searcher geometry.
  - Evidence pages 58-59: Cross-set alternate humanoid rebuild; not a demonstrated primary Searcher transformation.
- View/mechanism coverage: front=VERIFIED p1 and p27-33; rear=PARTIAL p27-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p27-33; undersideInterior=VERIFIED p2-33 staged modules and sled; mechanism=PARTIAL p28-33 crane/claws/module docking; no primary gait, siege cycle or full material route
- Verified findings:
  - The Martian Excavation Searcher is a huge low many-legged machine assembled from visibly separate forward tool, rear processing/crane and underslung sled modules.
  - A long drill, paired orange claws and tall crane create three different working directions around the irregular body instead of one humanoid front.
  - The low sled demonstrates material handling beneath the chassis, while the separate dark rig and final humanoid rebuild must not be mistaken for the primary Martian silhouette.
- Remaining evidence gaps:
  - The manual does not provide a walking gait, supported siege contact sequence, complete crane-to-processor route or game's manipulation attack; these require later semantic and motion contracts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A planted Searcher-derived processing machine with a crane intake and visible rock-transfer path.

Non-removable identity anchors:

- large crane-claw intake
- raised processing hopper
- outbound sled or Tube transfer point

- Rejected V1 blind-review code: `S63`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `building.ast.frontier_extraction_station` — Both are large planted extraction structures organized around one material channel. Mitigations: Frontier Station leads with a compact drill receiver; Excavation Plant leads with a tall Searcher-derived crane intake. / Frontier Station routes output into sealed containers; Excavation Plant exposes an open rock-transfer path. / Frontier Station stays low and rectilinear; Excavation Plant stands on irregular braces around an elevated handling body.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Large Searcher-derived crane-claw intake — receives rock through physical contact — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Raised open processing hopper — keeps the material route visible — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Outbound hypersled or Tube transfer point — connects local processing to settlement logistics — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A planted asymmetric frame carries the raised hopper and crane while a low intake-to-output corridor routes material into an unobstructed sled/Tube handoff.
- Repeated modules / connection grammar: Crane intake, hopper, processor, local storage rack, Worker receive point and outbound transfer remain distinct.
- Source-faithful versus adapted boundary: 7316 supplies handling machinery and sleds, not a complete building. The Plant must not copy the Searcher body wholesale or become a sealed refinery.

## E. Material and texture contract

- Geometry must carry:
  - large crane-claw intake
  - raised processing hopper
  - outbound sled/Tube point
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary resource-processing structure.
- Planted/contact rule: Crane base, hopper frame and output supports remain planted around a clear Worker approach.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_IntakeCrane` | Asset_ExcavationPlant | Worker receive point to raised hopper arc | resource handoff progress |
| `Pivot_HopperGate` | Asset_ExcavationPlant | closed processing state to outbound release angle | resource processing progress |
| `Pivot_OutputSled` | Asset_ExcavationPlant | processor berth to Tube/local handoff path | resource-output presentation |

- Required beats:
  - Idle keeps intake and output lanes clear.
  - Receive grips and lifts one visible load.
  - Process passes material through hopper to output.
  - Damage disables crane or gate before plant collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ResourceReceive`, `Socket_ResourceOutput`, `Socket_TubeTransfer`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The fixed Plant must share 7316 vocabulary without being mistaken for a parked Excavation Searcher.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
