# Mission Vehicle Bay — T082 Super Scout packet

**Stable ID:** `building.ast.mission_vehicle_bay`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Mission ground production
- Authoritative footprint: `Huge`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7645, 7647, 7697, 7699
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
| 7645 — MT-61 Crystal Reaper | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7645)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7645-1) | PRIMARY_VERIFIED | harvesting blades, mining modules and small alien craft |
| 7647 — MX-41 Switch Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7647)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7647-1) | PRIMARY_VERIFIED | six-wheel ground-to-flight transformation |
| 7697 — MT-51 Claw-Tank Ambush | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7697)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4515381.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7697-1) | PRIMARY_VERIFIED | tracked claw tank and small alien craft |
| 7699 — MT-101 Armored Drilling Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7699)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7699-1) | PRIMARY_VERIFIED | six-wheel heavy drilling chassis |

### Source audit [Astronauts:7645]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf)
- Construction map:
  - Evidence pages book 1, 3-26: Alien attack craft; supporting opposition evidence.
  - Evidence pages book 1, 27-50: White/orange mining cab and initial wheeled working platform.
  - Evidence pages book 1, 51-75; book 2, 2-17: Separate tool, drill and support assemblies for the mining system.
  - Evidence pages book 2, 18-45: Large Crystal Reaper chassis, tracked conversion and twin front harvesting-wheel installation.
  - Evidence pages book 2, 48-63: Powered controls, cables and explicit harvesting play feature.
- View/mechanism coverage: front=VERIFIED book 2 p38-66; rear=VERIFIED book 2 p44-66; leftRight=VERIFIED both books; top=VERIFIED book 2 p18-63; threeQuarter=VERIFIED covers and book 2 p43-66; undersideInterior=VERIFIED book 1 p27-75 and book 2 p18-45; mechanism=VERIFIED book 2 p45-63 powered twin harvesting wheels and tracked conversion
- Verified findings:
  - Crystal Reaper configuration is defined by two enormous exposed harvesting wheels mounted ahead of a low tracked body.
  - Two distinct articulated manipulators remain visible around the front harvesting area instead of being collapsed into the saws.
  - The substantial upper cockpit/processing assembly docks directly onto the tracked chassis and retains a clear detachable-spacecraft seam; it is not a trailer.
  - Orange structural rails, cables and motor blocks remain visible around the white mission shell.
  - The source separates cockpit craft, tool modules and running gear, supporting one configurable Mobile Mining Platform family rather than a generic sealed harvester.
- Remaining evidence gaps:
  - The source proves the detachable upper spacecraft assembly, but its exact gameplay processing role and the boundary between reusable Mobile Mining Platform chassis and Crystal-only harvesting module must be fixed during the asset-specific refit plan.

### Source audit [Astronauts:7647]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf)
- Construction map:
  - Evidence pages 3-14: Small alien attack craft; supporting opposition evidence.
  - Evidence pages 15-42: Long orange-canopy Switch Fighter hull and layered side shell.
  - Evidence pages 43-58: Separate full-span folding wing/chassis slab with six wheel mounts.
  - Evidence pages 59-64: Explicit conversion: wing tips fold, slab docks under the hull, six wheels attach, then the wing unfolds for flight.
- View/mechanism coverage: front=VERIFIED p37-64; rear=VERIFIED p41-64; leftRight=VERIFIED p15-64; top=VERIFIED p15-64; threeQuarter=VERIFIED p1 and p60-70; undersideInterior=VERIFIED p43-60 separate chassis/wing slab; mechanism=VERIFIED p58-64 physical ground-to-flight conversion
- Verified findings:
  - The ground vehicle and fighter are the same long cockpit hull carried by a separate folding wing/chassis slab.
  - Six orange wheels remain fully visible beneath the slab in ground state.
  - Transformation is readable through large wing-tip rotations and docking/undocking of the hull, not a cosmetic effect.
- Remaining evidence gaps:
  - The manual demonstrates a hand-separated hull during conversion; the production animation must define a believable continuous connection without changing the canonical two-state read.

### Source audit [Astronauts:7697]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4515381.pdf)
- Construction map:
  - Evidence pages 3-15: Alien ambush craft; supporting opposition evidence.
  - Evidence pages 16-36: Low rectangular Claw-Tank running-gear frame, twin tracked sides and central rotation mount.
  - Evidence pages 37-47: Circular upper body and orange transparent operator canopy.
  - Evidence pages 48-63: Long articulated tool/claw arms, side equipment and wheel/track details.
  - Evidence pages 64-68: Rotation and articulated-tool play evidence with final multi-angle views.
- View/mechanism coverage: front=VERIFIED p58-68; rear=VERIFIED p59-68; leftRight=VERIFIED p16-68; top=VERIFIED p16-68; threeQuarter=VERIFIED p1 and p63-68; undersideInterior=VERIFIED p16-47 staged base/turntable; mechanism=VERIFIED p58-68 rotating upper body and articulated tool arms
- Verified findings:
  - MT-51 is organized around a circular orange-canopy upper body rotating above a low, wide tracked frame.
  - Long independently articulated tool/claw arms radiate from the turret instead of forming a conventional forward gun.
  - The broad running gear stays visually separate from the rotating body, supporting movement and weapon orientation in different directions.
  - The separately constructed Alien ambush craft belongs to the opposing side of the mixed set and must never be fused into the Claw-Tank silhouette.
- Remaining evidence gaps:
  - The source demonstrates manual rotation and arm movement but not the game's exact multi-target attack cycle or fighting-retreat locomotion; animation timing remains open.

### Source audit [Astronauts:7699]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf)
- Construction map:
  - Evidence pages book 1, 3-14: Small human wheeled support craft and alien scout; supporting module evidence.
  - Evidence pages book 1, 15-29: White/orange forward mission cockpit and equipment cylinders.
  - Evidence pages book 1, 30-47; book 2, 2-17: Long open suspended chassis, side rails and rear service bay.
  - Evidence pages book 2, 18-27: Six separately mounted huge orange wheels and completed heavy running gear.
  - Evidence pages book 2, 28-34: Forward shell and detachable support/tool components attach to the chassis.
  - Evidence pages book 2, 35-43: Elevated rotating drill carriage, long drill tool and explicit rotation/tool play evidence.
- View/mechanism coverage: front=VERIFIED book 2 p24-43; rear=VERIFIED book 2 p27-43; leftRight=VERIFIED both books; top=VERIFIED book 2 p2-43; threeQuarter=VERIFIED covers and book 2 p35-43; undersideInterior=VERIFIED book 1 p30-47 and book 2 p2-27; mechanism=VERIFIED book 2 p35-43 rotating drill carriage and movable rear module
- Verified findings:
  - MT-101 is a long open heavy chassis suspended between six individually mounted oversized orange wheels.
  - The steep armored cockpit is a permanent part of the main vehicle at the front; the separate small support flyer does not replace or remove it.
  - The raised drilling carriage remains exposed above the central/rear frame, while a separate smaller gun/tool assembly retains its own mount.
  - The long drill rotates independently and may not be merged with the secondary gun/tool into one conventional tank cannon.
- Remaining evidence gaps:
  - The spring-projectile play action is not the game's contact-drill behavior; final drill reach, impact pose and chassis suspension response require a production animation plan.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A clean white-orange assembly bay with a huge central exit and standardized mission-module rails.

Non-removable identity anchors:

- huge unobstructed vehicle exit
- paired modular assembly rails
- white-orange overhead gantry

- Rejected V1 blind-review code: `S26`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `building.ast.field_systems_garage` — Both are huge Astronaut vehicle-production buildings. Mitigations: Field Garage is low, rugged and blue-gray; Mission Bay is clean, tall and white-orange. / Field Garage stores irregular external modules; Mission Bay uses standardized paired assembly rails. / Field Garage door clearance is shaped around unusual field wheels; Mission Bay has a straight huge mission-vehicle exit.
- `building.rock_raiders.engineering_workshop` — Both are huge production halls with one central heavy-unit opening. Mitigations: Engineering Workshop rises into one asymmetric drill/lift tower; Mission Vehicle Bay uses a centered clean overhead gantry. / Engineering Workshop exposes irregular tool and chassis racks; Mission Vehicle Bay repeats two standardized assembly rails. / Engineering Workshop's supports deliberately differ left-to-right; Mission Vehicle Bay preserves a controlled bilateral frame.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Huge straight vehicle exit — clears every Mission ground chassis — CANON_DERIVED_ADAPTATION.
  - Paired standardized assembly rails — visibly build wheels, tracks, tools and transformation modules — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - White-orange overhead gantry — identifies clean mission production — CANON_DERIVED_ADAPTATION.
- Structural load path: Two reinforced side frames carry the overhead gantry and assembly rails without entering the central exit volume.
- Repeated modules / connection grammar: Central exit, paired rails, module cells, overhead lift and crew/service path.
- Source-faithful versus adapted boundary: Composite donor mechanisms from 7645/7647/7697/7699 must remain named; no sealed sci-fi factory box.

## E. Material and texture contract

- Geometry must carry:
  - huge clear exit
  - paired assembly rails
  - overhead white-orange gantry
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: Side frames and rail foundations remain outside the production corridor.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_GantryLift` | Asset_MissionBay | vertical tool/module placement path | production progress |
| `Pivot_AssemblyRail` | Asset_MissionBay | bounded inward/outward service travel | production progress |

- Required beats:
  - Idle gantry parked clear.
  - Production stages major chassis modules.
  - Completed vehicle exits straight ahead.
  - Damage disables a rail or gantry before structural collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ProductionExit`, `Socket_ModuleStage`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact cross-set donor allocation requires a composed-design review before production.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
