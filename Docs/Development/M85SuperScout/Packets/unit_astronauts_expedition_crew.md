# Expedition Crew — T082 Super Scout packet

**Stable ID:** `unit.astronauts.expedition_crew`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Worker / builder
- Authoritative footprint: `Tiny`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7301, 7690
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
| 7301 — Rover | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7301)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7301-1) | PRIMARY_VERIFIED | four-wheel human field rover with long scanner/tool boom |
| 7690 — MB-01 Eagle Command Base | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7690)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7690-1) | PRIMARY_VERIFIED | human command base, transfer system and service architecture |

### Source audit [Astronauts:7301]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf)
- Construction map:
  - Evidence pages 1: Complete seven-step Rover build, equipment mast and final operator view.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 final view; rear=PARTIAL p1 steps 1-6; leftRight=VERIFIED p1 final and staged construction; top=VERIFIED p1 steps 3-6; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare chassis; mechanism=MISSING static open rover
- Verified findings:
  - The Rover is a tiny open four-wheel platform rather than an enclosed car.
  - Its long forward scanner/tool boom projects beyond the wheels and dominates the side profile.
  - The seated operator, blue equipment box and tall antenna remain exposed above the flat white chassis.
- Remaining evidence gaps:
  - A rear view is still required before final antenna, storage and propulsion placement.

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

**Silhouette thesis:** A mission crew member whose field pack and modular tool identify an expedition worker rather than infantry.

Non-removable identity anchors:

- sealed astronaut helmet
- standardized modular field pack
- interchangeable engineering tool

- Rejected V1 blind-review code: `S52`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.rock_raiders.crew` — Both are tiny minifigure-scale workers. Mitigations: Raider Crew leads with an oversized industrial hand tool; Expedition Crew leads with a standardized modular attachment. / Raider Crew uses helmet/visor, compact work light and dark-teal industrial blocks; Expedition Crew uses sealed astronaut helmet and white lineage markings. / Raider Crew reads improvised and tool-specific; Expedition Crew reads standardized and mission-configurable.
- `unit.aliens.etx_servitor` — Both are tiny mobile economy/build units. Mitigations: Expedition Crew is upright and bipedal; ETX Servitor is a low hover shell. / Expedition Crew carries tools externally; Servitor wraps around a crystal cradle. / Expedition Crew uses human helmet/backpack masses; Servitor uses a folding single manipulator and no humanoid body.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Sealed helmet and shared insignia — establish one expedition organization across both source lineages — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Lineage-specific field pack — carries repair, construction and refit equipment without homogenizing Field and Mission crews — CANON_DERIVED_ADAPTATION.
  - Interchangeable engineering tool — communicates extraction, building, repair and emergency defense — CANON_DERIVED_ADAPTATION.
- Structural load path: Minifigure-derived torso and hips carry the helmet, backpack and two-handed tool; no heavy weapon mass shifts the worker silhouette.
- Repeated modules / connection grammar: Shared crew rig with Field and Mission helmet/pack variants plus swappable work tools.
- Source-faithful versus adapted boundary: Source characters may vary, but production unifies gameplay sockets and insignia without inventing infantry armor.

## E. Material and texture contract

- Geometry must carry:
  - sealed helmet profile
  - lineage-specific pack mass
  - portable engineering tool
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Foot; compact expedition stride with tool and pack counter-motion.
- Planted/contact rule: Both feet settle before work or refit installation.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_Tool` | Asset_ExpeditionCrew | carried rest to target contact arc | construction, extraction, repair or refit progress |
| `Pivot_PackTool` | Asset_ExpeditionCrew | small deploy/check arc; stowed rest | service or idle presentation |

- Required beats:
  - Idle equipment check.
  - Foot travel with restrained pack lag.
  - Work/refit anticipation, repeated contact and settle.
  - Damage interrupts work; destruction separates tool before body collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ToolContact`, `Socket_WorkVfx`, `Socket_Refit`, `Socket_AudioTool`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Choose the production weighting of Field and Mission appearance variants during the complete-roster review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
