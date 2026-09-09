# Service & Refit Hub — T082 Super Scout packet

**Stable ID:** `building.ast.service_refit_hub`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Technology / refit
- Authoritative footprint: `Huge`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7315, 7690
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
| 7315 — Solar Explorer | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7315)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7315-1) | PRIMARY_VERIFIED | solar arrays, field modules and service construction |
| 7690 — MB-01 Eagle Command Base | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7690)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7690-1) | PRIMARY_VERIFIED | human command base, transfer system and service architecture |

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

### Source audit [Astronauts:7690]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages book 1, 3-15: Alien scout craft; supporting opposition evidence.
  - PDF pages book 1, 16-49: Compact astronaut drilling/transfer station, pressure tank, hoses and material routes.
  - PDF pages book 1, 50-75; book 2, 2-31: Tall open A-frame service gantry, crane/transfer boom and elevated mission modules.
  - PDF pages book 2, 32-68: Long white/orange shuttle assembled and suspended within the gantry.
  - PDF pages book 2, 69-72: Final base-wide views and explicit crane/transfer interaction.
- View/mechanism coverage: front=VERIFIED book 2 p23-72; rear=VERIFIED book 2 p26-72; leftRight=VERIFIED both books; top=VERIFIED book 2 p17-72; threeQuarter=VERIFIED covers and book 2 p29-72; undersideInterior=VERIFIED staged open gantry and shuttle construction; mechanism=VERIFIED book 1 p47-49 and book 2 p23-31 transfer hoses/crane; PARTIAL shuttle service cycle
- Verified findings:
  - MB-01 is an open mission complex dominated by a tall white A-frame gantry rather than an enclosed headquarters block.
  - A separate drilling/transfer station, pressure tank and routed hoses make resources and service physically legible.
  - The long shuttle hangs inside the gantry with visible overhead and side access, establishing a reusable Command Base, refit and flight-service grammar.
- Remaining evidence gaps:
  - The source combines command, extraction and shuttle service in one playset; the game's Command Base, Refit Hub and Sentinel adaptations still need explicit boundaries between shared modules.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A modular service ring where Field and Mission machines visibly dock to standardized interfaces.

Non-removable identity anchors:

- central refit turntable
- contrasting Field and Mission docking arms
- visible replacement-module racks

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `building.ast.flight_operations_pad` — Both are broad open Astronaut service surfaces. Mitigations: Flight Pad is flat and strongly directional; Refit Hub is a circular docking arrangement. / Flight Pad uses retractable aircraft service arms; Refit Hub uses contrasting Field and Mission docking arms. / Flight Pad culminates in a beacon; Refit Hub culminates in a replacement-module turntable.

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
