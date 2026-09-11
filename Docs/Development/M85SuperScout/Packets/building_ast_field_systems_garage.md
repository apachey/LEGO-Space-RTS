# Field Systems Garage — T082 Super Scout packet

**Stable ID:** `building.ast.field_systems_garage`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Field production
- Authoritative footprint: `Huge`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7301, 7312, 7315
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
| 7312 — T3-Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7312)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7312-1) | PRIMARY_VERIFIED | three-wheel field chassis, suspension and modularity |
| 7315 — Solar Explorer | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7315)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7315-1) | PRIMARY_VERIFIED | solar arrays, field modules and service construction |

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

### Source audit [Astronauts:7315]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf)
- Construction map:
  - Evidence pages 2-7: Forward cockpit and low exploration nose module.
  - Evidence pages 8-17: Long modular habitation/cargo body plus separate small support pod.
  - Evidence pages 18-23: Twin-panel solar/service tail built around a tall circular frame and attached to the long body.
  - Evidence pages 24-26: Cross-set alternate models and extended modular combinations; not direct production geometry.
- View/mechanism coverage: front=VERIFIED p1 and p22-23; rear=PARTIAL p18-23; leftRight=VERIFIED p2-23; top=VERIFIED p2-23; threeQuarter=VERIFIED p1 and p22-26; undersideInterior=VERIFIED p2-21 staged construction; mechanism=PARTIAL p18-23 separable solar/service module; deployment not demonstrated
- Verified findings:
  - Solar Explorer identity comes from a long low modular convoy body rather than a single compact rover.
  - The rear service section carries two broad solar wings around a tall circular machinery frame.
  - Cockpit, habitat/cargo body, support pod and solar tail remain independently readable modules.
- Remaining evidence gaps:
  - The manual supports separable modules but not the game's deployed Forward Service state; stabilizers, access route and deployment motion remain explicit adaptation work.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A rugged open garage sized around unconventional Life on Mars wheels and swappable field modules.

Non-removable identity anchors:

- wide low field-vehicle door
- external module racks
- exposed blue-gray service frame

- Rejected V1 blind-review code: `S23`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `building.ast.mission_vehicle_bay` — Both are huge Astronaut vehicle-production buildings. Mitigations: Field Garage is low, rugged and blue-gray; Mission Bay is clean, tall and white-orange. / Field Garage stores irregular external modules; Mission Bay uses standardized paired assembly rails. / Field Garage door clearance is shaped around unusual field wheels; Mission Bay has a straight huge mission-vehicle exit.
- `building.rock_raiders.vehicle_service_bay` — The draft sheet exposes two huge low open repair gantries around a vehicle lane. Mitigations: Raider Service Bay uses paired overhead repair arms; Field Garage uses unequal side racks and rugged modules. / Raider Service Bay is a straight drive-through frame; Field Garage shapes its exit around oversized field wheels. / Raider Service Bay concentrates its skyline in the gantry; Field Garage stays lower and spreads storage mass laterally.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Wide low field-vehicle door — clears Rover, Trike and Solar Explorer modules — CANON_DERIVED_ADAPTATION.
  - External irregular module racks — preserve rugged Life on Mars logistics — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Exposed blue-gray service frame — distinguishes Field production from clean Mission assembly — CANON_DERIVED_ADAPTATION.
- Structural load path: A low open frame spans one unobstructed vehicle lane while side racks and repair arms load into outer supports.
- Repeated modules / connection grammar: Drive-through lane, wheel/module racks, repair arms and crew access remain independently readable.
- Source-faithful versus adapted boundary: No direct garage set exists; every visible module derives from verified 7301/7312/7315 field equipment grammar.

## E. Material and texture contract

- Geometry must carry:
  - wide low vehicle opening
  - external module racks
  - exposed field frame
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_field_frame_surface` — Restrained molded and brushed variation for rugged blue-gray Field Systems frames without faking structural seams. Channels: Tangent-space normal and linear roughness; body color stays parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Field Systems surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: Outer supports stay planted outside the vehicle lane.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ServiceArmLeft` | Asset_FieldGarage | rack-to-vehicle service arc | production/repair progress |
| `Pivot_ServiceArmRight` | Asset_FieldGarage | mirrored service arc | production/repair progress |

- Required beats:
  - Idle tools remain raised from lane.
  - Production stages modules visibly.
  - Vehicle exits through the named clear lane.
  - Damage drops one arm outward; destruction never blocks exit before authoritative state.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ProductionExit`, `Socket_Service`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - This new composition requires director review of exact donor modules before modeling.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
