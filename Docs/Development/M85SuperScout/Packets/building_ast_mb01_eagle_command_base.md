# MB-01 Eagle Command Base — T082 Super Scout packet

**Stable ID:** `building.ast.mb01_eagle_command_base`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Command / expansion
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7690
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
| 7690 — MB-01 Eagle Command Base | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7690)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7690-1) | PRIMARY_VERIFIED | human command base, transfer system and service architecture |

### Source audit [Astronauts:7690]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf)
- Construction map:
  - Evidence pages book 1, 3-15: Alien scout craft; supporting opposition evidence.
  - Evidence pages book 1, 16-49: Compact astronaut drilling/transfer station, pressure tank, hoses and material routes.
  - Evidence pages book 1, 50-75; book 2, 2-31: Tall open A-frame service gantry, crane/transfer boom and elevated mission modules.
  - Evidence pages book 2, 32-68: Long white/orange shuttle assembled and suspended within the gantry.
  - Evidence pages book 2, 69-72: Final base-wide views and explicit crane/transfer interaction.
- View/mechanism coverage: front=VERIFIED book 2 p23-72; rear=VERIFIED book 2 p26-72; leftRight=VERIFIED both books; top=VERIFIED book 2 p17-72; threeQuarter=VERIFIED covers and book 2 p29-72; undersideInterior=VERIFIED staged open gantry and shuttle construction; mechanism=VERIFIED book 1 p47-49 and book 2 p23-31 transfer hoses/crane; PARTIAL shuttle service cycle
- Verified findings:
  - MB-01 is an open mission complex dominated by a tall white A-frame gantry rather than an enclosed headquarters block.
  - A separate drilling/transfer station, pressure tank and routed hoses make resources and service physically legible.
  - The long shuttle hangs inside the gantry with visible overhead and side access, establishing a reusable Command Base, refit and flight-service grammar.
- Remaining evidence gaps:
  - The source combines command, extraction and shuttle service in one playset; the game's Command Base, Refit Hub and Sentinel adaptations still need explicit boundaries between shared modules.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A modular white-orange command complex with a tall central tower and readable shuttle/transfer machinery.

Non-removable identity anchors:

- tall mission-control tower
- pneumatic transfer spine
- open shuttle and service pads

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Tall open A-frame command gantry — establishes the base skyline and shuttle-scale opening — SOURCE_VERIFIED.
  - Separate drilling/transfer station with pressure tank and hoses — makes command logistics physical — SOURCE_VERIFIED.
  - Open shuttle and service pads — preserve circulation through the complex — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A broad modular foundation carries the A-frame, tower and transfer equipment while keeping vehicle/shuttle lanes unobstructed.
- Repeated modules / connection grammar: Command tower, A-frame gantry, transfer station, service pad and expansion interface remain separable functional zones.
- Source-faithful versus adapted boundary: 7690 combines several functions; the game HQ may coordinate them but cannot become one sealed generic command block.

## E. Material and texture contract

- Geometry must carry:
  - tall A-frame gantry
  - separate transfer station
  - open service lanes
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: Broad foundation and gantry feet remain planted; all entrances and exits stay clear.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_TransferBoom` | Asset_MB01 | station-to-shuttle service arc | resource/service presentation |
| `Pivot_CommandArray` | Asset_MB01 | bounded scan yaw | operational state |

- Required beats:
  - Idle command traffic and bounded signals.
  - Crew production opens a clear exit.
  - Resource transfer moves through hose/boom path.
  - Damage disables tower/boom modules before main collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ProductionExit`, `Socket_ResourceReceive`, `Socket_Command`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Module boundaries shared with Service & Refit Hub require the cross-building skyline review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
