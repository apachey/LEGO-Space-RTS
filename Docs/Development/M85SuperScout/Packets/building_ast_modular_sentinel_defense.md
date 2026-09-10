# Modular Sentinel Defense — T082 Super Scout packet

**Stable ID:** `building.ast.modular_sentinel_defense`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Configurable ground / air defense
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 7690, 7695
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
| 7695 — MX-11 Astro Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7695)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517774.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7695-1) | PRIMARY_VERIFIED | astronaut fighter wing and defense hardpoint language |

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

### Source audit [Astronauts:7695]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517774.pdf)
- Construction map:
  - Evidence pages 2-13: Complete MX-11 Astro Fighter build: flat wing plate, orange canopy nose, tail/antenna and pilot scale.
  - Evidence pages 14-24: Inventory and promotional pages; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p9-13; rear=PARTIAL p10-13; leftRight=VERIFIED p2-13; top=VERIFIED p2-13; threeQuarter=VERIFIED p1 and p11-13; undersideInterior=VERIFIED p2-9 staged plate build; mechanism=MISSING static micro-fighter
- Verified findings:
  - MX-11 is a thin white delta-wing craft with a sharp orange canopy/nose at its center.
  - The entire fighter stays close to one plate thickness, separating it from bulkier mission aircraft.
  - A small dark rear equipment block and antenna provide the only raised mass behind the pilot.
- Remaining evidence gaps:
  - Clean underside and propulsion views are still required before consolidating MX-11 with the Crystal Hawk into one Mission Fighter family.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A clean modular defense pedestal whose replaceable head clearly changes ground-versus-air purpose.

Non-removable identity anchors:

- standard mission attachment pedestal
- swappable sensor-weapon head
- deployed stabilizer feet

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Standard mission attachment pedestal — supports both defensive configurations — CANON_DERIVED_ADAPTATION.
  - Swappable ground-interdiction or air-interception head — communicates target role before firing — CANON_DERIVED_ADAPTATION.
  - Deployed stabilizer feet — distinguish the structure from a parked vehicle — CANON_DERIVED_ADAPTATION.
- Structural load path: A compact planted base routes weapon load through a standardized vertical module socket into three or four visible stabilizers.
- Repeated modules / connection grammar: Common pedestal, ground head, air head, sensor block and stabilizer set.
- Source-faithful versus adapted boundary: This is approved new gameplay content clad with 7690 command and 7695 fighter-module grammar; exact donor pieces and rejected options require review.

## E. Material and texture contract

- Geometry must carry:
  - standard pedestal
  - clearly different module heads
  - deployed stabilizer feet
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: All stabilizers remain planted; refit requires visible downtime.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ModuleYaw` | Asset_ModularSentinel | bounded target-tracking yaw | presentation aim |
| `Pivot_ModulePitch` | Pivot_ModuleYaw | ground-low or air-high target pitch | presentation aim |
| `Pivot_Stabilizers` | Asset_ModularSentinel | folded construction to planted stance | construction state |

- Required beats:
  - Construction plants stabilizers and seats one head.
  - Idle scans within configuration limits.
  - Attack aims and fires from the fitted module.
  - Refit powers down, exchanges head and recalibrates; destruction ejects the module.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Module`, `Socket_Muzzle`, `Socket_Sensor`, `Socket_AudioWeapon`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - A composed-design proposal must compare 7690 and 7695 donor arrangements before director approval.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
