# T3-Trike — T082 Super Scout packet

**Stable ID:** `unit.astronauts.t3_trike`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Escort / survey
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7312, 7694
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
| 7312 — T3-Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7312)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7312-1) | PRIMARY_VERIFIED | three-wheel field chassis, suspension and modularity |
| 7694 — MT-31 Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7694)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517775.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7694-1) | PRIMARY_VERIFIED | Mission Systems trike equipment variant |

### Source audit [Astronauts:7312]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf)
- Construction map:
  - Evidence pages 2-9: Central T3-Trike cockpit/chassis, twin front outriggers and large rear wheel assembly.
  - Evidence pages 10-13: Separate service robot and scanner station modules.
  - Evidence pages 14-17: Outrigger equipment, hoses, final three-wheel machine and operator scale.
  - Evidence pages 18: Cross-set alternate walker; not direct T3-Trike geometry.
- View/mechanism coverage: front=VERIFIED p1 and p14-17; rear=PARTIAL p8-17; leftRight=VERIFIED p2-17 construction sequence; top=VERIFIED p2-16; threeQuarter=VERIFIED p1 and p16-17; undersideInterior=VERIFIED p2-8 exposed chassis; mechanism=PARTIAL p14-16 rotating outrigger/tool mounts; no driving sequence
- Verified findings:
  - The signature layout is one huge rear wheel plus two long forward outriggers ending in smaller contact points.
  - The spherical transparent cockpit is the visual hub between wheel and outriggers.
  - Tools mount at the outrigger tips and central side sockets, supporting a visible refit language without changing the three-contact silhouette.
- Remaining evidence gaps:
  - The source proves modular attachment points but not the game's Escort/Survey conversion sequence; that adaptation requires its own mechanical plan.

### Source audit [Astronauts:7694]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517775.pdf)
- Construction map:
  - Evidence pages 2-13: Narrow open trike body built around a long orange equipment cylinder and exposed operator position.
  - Evidence pages 14-21: Three huge orange wheels attach through long angled arm/axle assemblies; final operator-scale views.
- View/mechanism coverage: front=VERIFIED p15-21; rear=PARTIAL p16-21; leftRight=VERIFIED p2-21; top=VERIFIED p2-21; threeQuarter=VERIFIED p1 and p15-21; undersideInterior=VERIFIED p2-18 exposed chassis; mechanism=PARTIAL p14-20 articulated wheel arms; suspension motion not demonstrated
- Verified findings:
  - MT-31 retains an unmistakable three-wheel layout with oversized orange tires on long exposed supports.
  - The narrow central body is mostly orange equipment cylinder and open seat rather than protective hull.
  - Its Mission Systems equipment mass is visually heavier than 7312 while remaining recognizably part of the same trike family.
- Remaining evidence gaps:
  - The source does not define the game's Escort/Survey payload swap or suspension travel; both require a shared T3-Trike family plan.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A rough-terrain trike defined by one leading wheel, two broad rear wheels and a modular centre bay.

Non-removable identity anchors:

- one-front two-rear wheel triangle
- high articulated suspension
- replaceable centre mission module

- Blind-review code: `S58`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.rover` — Both are rugged Life on Mars wheeled field scouts. Mitigations: Rover has two wheels; T3-Trike has one front and two broad rear wheels. / Rover is tiny and open with no centre module; T3-Trike has a large swappable mission bay. / Rover carries a narrow sensor bar; T3-Trike is defined by high articulated suspension.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - One-front/two-rear wheel triangle — preserves the unmistakable trike plan — SOURCE_VERIFIED.
  - Long articulated wheel arms — explain rough-terrain suspension — SOURCE_VERIFIED.
  - Replaceable center mission bay — carries Escort or Survey equipment — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: The narrow central spine transfers the mission-bay load into the front fork and two long rear wheel arms.
- Repeated modules / connection grammar: Three wheel modules, exposed suspension arms and one standardized center payload interface.
- Source-faithful versus adapted boundary: 7312 and 7694 remain appearance/equipment members of one unit family; refit swaps the center module rather than the chassis.

## E. Material and texture contract

- Geometry must carry:
  - three-wheel triangle
  - long suspension arms
  - center mission module
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_field_frame_surface` — Restrained molded and brushed variation for rugged blue-gray Field Systems frames without faking structural seams. Channels: Tangent-space normal and linear roughness; body color stays parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Field Systems surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Rough-terrain wheeled travel with large visible suspension arcs.
- Planted/contact rule: All three wheels seek terrain contact; center bay remains above the suspension plane.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WheelFront` | Asset_T3Trike | roll plus steering yaw | movement and steering |
| `Pivot_SuspensionRearLeft` | Asset_T3Trike | bounded vertical arm travel | terrain response |
| `Pivot_SuspensionRearRight` | Asset_T3Trike | mirrored bounded vertical arm travel | terrain response |

- Required beats:
  - Idle suspension settle.
  - Travel emphasizes three-wheel rhythm.
  - Escort or Survey module operates from the center bay.
  - Refit removes and seats the replacement module; damage never changes gameplay state by animation.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Module`, `Socket_Weapon`, `Socket_SurveyPulse`, `Socket_AudioDrive`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final 7312-versus-7694 body weighting waits for the family silhouette review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
