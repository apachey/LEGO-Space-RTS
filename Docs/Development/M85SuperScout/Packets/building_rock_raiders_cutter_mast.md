# Cutter Mast — T082 Super Scout packet

**Stable ID:** `building.rock_raiders.cutter_mast`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Infrastructure`
- Gameplay role: Anti-air defense
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 4910, 4990
- Current confidence: verified canonical identity; construction confidence remains bounded by the source verification shown below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: Source-view coverage and construction-critical page ranges are recorded for the audited Rock Raiders sources below. Asset-specific adaptation boundaries still must be resolved before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 4910 — The Hover Scout | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4910)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128290.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4910-1) | PRIMARY_VERIFIED | hover scout construction, palette and equipment |
| 4990 — Rock Raiders HQ | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4990)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4990-1) | PRIMARY_VERIFIED | industrial architecture, crane, processing and service motifs |

### Set 4910 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1: Complete Hover Scout build from flat base plate through open operator deck, front scanner/tool mass and rear equipment.
  - PDF pages 2: Separate small worksite/scanner station; useful for the Cutter Mast base language, not a reverse view of the Scout.
- View/mechanism coverage: front=PARTIAL p1 cover/final build; rear=PARTIAL p1 final steps; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-7; threeQuarter=VERIFIED p1 cover and steps; undersideInterior=MISSING; mechanism=PARTIAL p1 scanner/tool mounting; no authored movement sequence
- Verified findings:
  - The Scout is an exposed plate-built sled rather than a closed hovercraft.
  - The operator, scanner/tool and rear rack are independent readable masses.
  - The second-page station provides a source-faithful tripod/pedestal vocabulary for later survey-derived infrastructure.
- Remaining evidence gaps:
  - Acquire explicit underside and opposite-side evidence before final modeling.
  - Any animated scanner sweep is an approved presentation interpretation, not proven by the manual.

### Set 4990 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-7: Small work vehicles, crystal handling and compact workstation modules.
  - PDF pages 8-10: Tall illuminated machinery/power tower with open service access.
  - PDF pages 11-19: Long articulated crane/tool boom mounted to the tower and used for rock handling.
  - PDF pages 20-29: Open vehicle-width gantry/workshop with sloped supports, lamps and overhead rails.
  - PDF pages 30-34: Conveyor/processing module attaches to the gantry and completes a visible material route.
  - PDF pages 35-40: Modules connect across an irregular rock worksite base rather than a sealed building shell.
  - PDF pages 41-43: Final product photography supplies overall skyline, module spacing and worksite context.
- View/mechanism coverage: front=VERIFIED p41-43; rear=PARTIAL p35-43; leftRight=VERIFIED p35-43; top=VERIFIED p35-40; threeQuarter=VERIFIED p1 and p41-43; undersideInterior=VERIFIED p2-40 staged module and base construction; mechanism=VERIFIED p11-19 crane/tool boom; PARTIAL p20-34 gantry/conveyor service path
- Verified findings:
  - HQ identity comes from a loose network of independently readable work modules on an uneven base, not from a single enclosed headquarters block.
  - Tower, articulated crane, open vehicle gantry and conveyor/processing path establish the reusable infrastructure grammar.
  - Entrances and service lanes remain physically open and minifigure/vehicle scaled.
- Remaining evidence gaps:
  - The manual supports modular industrial functions but does not assign the game's exact Ore Plant, Power Station, Service Bay or Workshop boundaries; those remain explicit canonical adaptations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A tall survey-and-cutting mast whose tracking head reads as repurposed worksite equipment.

Non-removable identity anchors:

- thin industrial mast
- scanner-cutter tracking head
- tripod service base with work light

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `building.mar.aero_guard_tower` — Both are small tall anti-air structures. Mitigations: Cutter Mast is a thin industrial tripod; Aero Guard Tower is an open stacked Martian platform. / Cutter Mast has one scanner-cutter head; Aero Guard Tower has paired articulated tracking arms. / Cutter Mast carries a warm work light; Aero Guard Tower exposes blue/sand-red mechanics and a Tube connection.

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
