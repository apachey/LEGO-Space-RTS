# Mono Jet — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mono_jet`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Light air scout / harassment
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 7310
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
| 7310 — Mono Jet | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7310)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130805.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7310-1) | PRIMARY_VERIFIED | human field aircraft silhouette |

### Source audit [Astronauts:7310]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130805.pdf)
- Construction map:
  - Evidence pages 1-2: Complete eleven-step Mono Jet build with side, top and final operator views.
- View/mechanism coverage: front=PARTIAL p2 final view; rear=VERIFIED p2 steps 9-11; leftRight=VERIFIED p1-2 construction rotation; top=VERIFIED p1-2; threeQuarter=VERIFIED p1 cover and p2 final; undersideInterior=VERIFIED p1 bare plate and engine pod sequence; mechanism=MISSING static micro-flyer
- Verified findings:
  - The Mono Jet is an extremely compact asymmetric one-person craft with no enclosed fuselage.
  - One dominant treaded cylindrical drum sits directly between a shrouded conical nozzle and the tiny operator platform.
  - The exposed operator, short wedge deck and small axle-mounted signal dish prevent it from reading as a long or symmetric fighter.
- Remaining evidence gaps:
  - The manual does not establish landing gear or control-surface motion; both remain presentation adaptations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** An extremely compact one-person field craft built around one dominant transverse drum between a shrouded nozzle and tiny exposed control deck.

Non-removable identity anchors:

- single dominant cylindrical drum
- side shrouded conical nozzle
- tiny open operator wedge

- Rejected V1 blind-review code: `S16`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mission_fighter` — Both are small true-air Astronaut craft. Mitigations: Mono Jet is an asymmetric drum-and-platform microcraft; Mission Fighter has a crisp swept mission-wing plan. / Mono Jet exposes its operator beside the dominant drum; Mission Fighter uses a compact enclosed blue canopy. / Mono Jet remains Field white/gray/blue; Mission Fighter uses strong white/orange Mission blocks.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Single dominant transverse drum — defines the tiny asymmetric field craft — SOURCE_VERIFIED.
  - Side shrouded conical nozzle — keeps propulsion concentrated on one side — SOURCE_VERIFIED.
  - Tiny open operator wedge and axle-mounted dish — preserve the exposed microcraft scale — SOURCE_VERIFIED.
- Structural load path: One short transverse axle joins the dominant drum, shrouded nozzle and tiny operator platform without a long fuselage.
- Repeated modules / connection grammar: Drum/nozzle assembly, exposed control wedge and small axle-mounted dish remain independently readable.
- Source-faithful versus adapted boundary: True-air behavior and light precision attack are canonical adaptations; silhouette remains the direct compact 7310 craft.

## E. Material and texture contract

- Geometry must carry:
  - single dominant cylindrical drum
  - side shrouded conical nozzle
  - tiny open operator wedge
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_field_frame_surface` — Restrained molded and brushed variation for rugged blue-gray Field Systems frames without faking structural seams. Channels: Tangent-space normal and linear roughness; body color stays parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Field Systems surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: True air; agile banking around the compact transverse drum assembly.
- Planted/contact rule: No ground contact in normal operation; hover datum and shadow communicate altitude.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_Drum` | Asset_MonoJet | restrained axle rotation if needed for propulsion presentation | flight thrust presentation |
| `Pivot_SignalDish` | Asset_MonoJet | small bounded signal or aim arc while remaining attached beside the controls | survey or attack presentation |

- Required beats:
  - Idle air loiter.
  - Travel banks the compact asymmetric body without helicopter bob.
  - Attack aligns the operator wedge before fire.
  - Damage destabilizes the drum/nozzle assembly; destruction separates it from the operator platform.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Weapon`, `Socket_Muzzle`, `Socket_Engine`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The source does not prove that the drum rotates in operation; any rotation remains restrained presentation-only adaptation pending blockout review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
