# T3-Trike — T082 Super Scout packet

**Stable ID:** `unit.astronauts.t3_trike`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Escort / survey
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7312, 7694
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
| 7312 — T3-Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7312)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7312-1) | PRIMARY_VERIFIED | three-wheel field chassis, suspension and modularity |
| 7694 — MT-31 Trike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7694)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517775.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7694-1) | PRIMARY_VERIFIED | Mission Systems trike equipment variant |

### Set 7312 source audit

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

### Set 7694 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-13: Narrow open trike body built around a long orange equipment cylinder and exposed operator position.
  - PDF pages 14-21: Three huge orange wheels attach through long angled arm/axle assemblies; final operator-scale views.
- View/mechanism coverage: front=VERIFIED p15-21; rear=PARTIAL p16-21; leftRight=VERIFIED p2-21; top=VERIFIED p2-21; threeQuarter=VERIFIED p1 and p15-21; undersideInterior=VERIFIED p2-18 exposed chassis; mechanism=PARTIAL p14-20 articulated wheel arms; suspension motion not demonstrated
- Verified findings:
  - MT-31 retains an unmistakable three-wheel layout with oversized orange tires on long exposed supports.
  - The narrow central body is mostly orange equipment cylinder and open seat rather than protective hull.
  - Its Mission Systems equipment mass is visually heavier than 7312 while remaining recognizably part of the same trike family.
- Remaining evidence gaps:
  - The source does not define the game's Escort/Survey payload swap or suspension travel; both require a shared T3-Trike family plan.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A rough-terrain trike defined by one leading wheel, two broad rear wheels and a modular centre bay.

Non-removable identity anchors:

- one-front two-rear wheel triangle
- high articulated suspension
- replaceable centre mission module

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.rover` — Both are rugged Life on Mars wheeled field scouts. Mitigations: Rover has two wheels; T3-Trike has one front and two broad rear wheels. / Rover is tiny and open with no centre module; T3-Trike has a large swappable mission bay. / Rover carries a narrow sensor bar; T3-Trike is defined by high articulated suspension.

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
