# Field Systems Garage — T082 Super Scout packet

**Stable ID:** `building.ast.field_systems_garage`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Field production
- Authoritative footprint: `Huge`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7301, 7312, 7315
- Current confidence: verified canonical identity; construction confidence remains bounded by the source verification shown below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: Source-view coverage and construction-critical page ranges are recorded for the audited sources below. Asset-specific adaptation boundaries still must be resolved before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 7301 — Rover | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7301)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7301-1) | PRIMARY_VERIFIED | two-wheel human field rover |
| 7312 — T3-Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7312)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7312-1) | PRIMARY_VERIFIED | three-wheel field chassis, suspension and modularity |
| 7315 — Solar Explorer | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7315)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7315-1) | PRIMARY_VERIFIED | solar arrays, field modules and service construction |

### Source audit [Astronauts:7301]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1: Complete seven-step Rover build, equipment mast and final operator view.
  - PDF pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 final view; rear=PARTIAL p1 steps 1-6; leftRight=VERIFIED p1 final and staged construction; top=VERIFIED p1 steps 3-6; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare chassis; mechanism=MISSING static open rover
- Verified findings:
  - The Rover is a tiny open four-wheel platform rather than an enclosed car.
  - Its long forward scanner/tool boom projects beyond the wheels and dominates the side profile.
  - The seated operator, blue equipment box and tall antenna remain exposed above the flat white chassis.
- Remaining evidence gaps:
  - A rear view is still required before final antenna, storage and propulsion placement.

### Source audit [Astronauts:7312]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-9: Central T3-Trike cockpit/chassis, twin front outriggers and large rear wheel assembly.
  - PDF pages 10-13: Separate service robot and scanner station modules.
  - PDF pages 14-17: Outrigger equipment, hoses, final three-wheel machine and operator scale.
  - PDF pages 18: Cross-set alternate walker; not direct T3-Trike geometry.
- View/mechanism coverage: front=VERIFIED p1 and p14-17; rear=PARTIAL p8-17; leftRight=VERIFIED p2-17 construction sequence; top=VERIFIED p2-16; threeQuarter=VERIFIED p1 and p16-17; undersideInterior=VERIFIED p2-8 exposed chassis; mechanism=PARTIAL p14-16 rotating outrigger/tool mounts; no driving sequence
- Verified findings:
  - The signature layout is one huge rear wheel plus two long forward outriggers ending in smaller contact points.
  - The spherical transparent cockpit is the visual hub between wheel and outriggers.
  - Tools mount at the outrigger tips and central side sockets, supporting a visible refit language without changing the three-contact silhouette.
- Remaining evidence gaps:
  - The source proves modular attachment points but not the game's Escort/Survey conversion sequence; that adaptation requires its own mechanical plan.

### Source audit [Astronauts:7315]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-7: Forward cockpit and low exploration nose module.
  - PDF pages 8-17: Long modular habitation/cargo body plus separate small support pod.
  - PDF pages 18-23: Twin-panel solar/service tail built around a tall circular frame and attached to the long body.
  - PDF pages 24-26: Cross-set alternate models and extended modular combinations; not direct production geometry.
- View/mechanism coverage: front=VERIFIED p1 and p22-23; rear=PARTIAL p18-23; leftRight=VERIFIED p2-23; top=VERIFIED p2-23; threeQuarter=VERIFIED p1 and p22-26; undersideInterior=VERIFIED p2-21 staged construction; mechanism=PARTIAL p18-23 separable solar/service module; deployment not demonstrated
- Verified findings:
  - Solar Explorer identity comes from a long low modular convoy body rather than a single compact rover.
  - The rear service section carries two broad solar wings around a tall circular machinery frame.
  - Cockpit, habitat/cargo body, support pod and solar tail remain independently readable modules.
- Remaining evidence gaps:
  - The manual supports separable modules but not the game's deployed Forward Service state; stabilizers, access route and deployment motion remain explicit adaptation work.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A rugged open garage sized around unconventional Life on Mars wheels and swappable field modules.

Non-removable identity anchors:

- wide low field-vehicle door
- external module racks
- exposed blue-gray service frame

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `building.ast.mission_vehicle_bay` — Both are huge Astronaut vehicle-production buildings. Mitigations: Field Garage is low, rugged and blue-gray; Mission Bay is clean, tall and white-orange. / Field Garage stores irregular external modules; Mission Bay uses standardized paired assembly rails. / Field Garage door clearance is shaped around unusual field wheels; Mission Bay has a straight huge mission-vehicle exit.

## D. Construction contract

- Hero geometry must preserve every recognition anchor above.
- Support geometry must explain how hero masses connect, carry load and articulate.
- Micro geometry may enrich close view but may not become required for recognition.
- Exact chassis/load path, repeated modules, mounting logic, scale ratios and approved adaptations: `HOLD — SOURCE DECOMPOSITION REQUIRED`.

## E. Material and texture contract

- Silhouette, openings, major panel breaks, moving joints and LEGO connection logic remain geometry.
- Surface channels may carry controlled color masks, roughness, emission, decals and non-structural relief only.
- Required reusable and bespoke texture sets, resolution, tiling, texel density, LOD fallback and import settings: `HOLD — TEXTURE-NEEDS AUDIT REQUIRED`.
- Baked lighting, fake silhouette structure and illegible micro-noise are prohibited.

## F. State and animation contract

- Applicable idle, locomotion/operation, work, attack, production, repair, transform/deploy, disabled, damage and destruction beats: `HOLD — MECHANISM EVIDENCE REQUIRED`.
- Every moving assembly must receive a named pivot, parent, axis/path, rest/extreme poses and authoritative presentation driver.
- Animation may communicate gameplay state but never decide gameplay timing.

## G. Presentation hookups

- `Socket_Selection` and `Socket_Health` are mandatory.
- Tool, weapon, projectile, VFX, lamp and audio sockets follow only from verified function.
- Cargo, passenger, service, production-exit or network sockets apply where the canonical role requires them.
- Identification Tile, icon silhouette, portrait camera and reduced-presentation fallback: `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, faction, role, footprint, source classification and mapped source family.
- Canon-derived interpretation: silhouette thesis and identity anchors above.
- Unknown: exact multi-angle construction, articulation, material ratios, texture inventory and confusion mitigation until the remaining audits are complete.
- Consequential contradictions: none recorded at identity-baseline stage.

## I. Build handoff

1. Verify and cite the complete multi-angle source board.
2. Decompose primary masses and negative spaces from orthogonal evidence.
3. Resolve LEGO load path, connection grammar and moving mechanism.
4. Complete material/texture and state/animation contracts.
5. Produce 24/44/72-cell black silhouettes and run the cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
