# Tunnel Transport — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.tunnel_transport`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Heavy air transport
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4980
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
| 4980 — Tunnel Transport | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4980)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128427.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4980-1) | PRIMARY_VERIFIED | twin-propeller heavy lift construction |

### Set 4980 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-14: Forward cockpit/cargo vehicle module with low worksite chassis and open load bed.
  - PDF pages 15-20: Separate compact wheeled support module.
  - PDF pages 21-29: Wide rotor transport frame, twin propeller pods and attachment to the carried modules.
  - PDF pages 30-32: Cargo container, alternate carried load and final multi-angle product photography.
- View/mechanism coverage: front=VERIFIED p1 and p23-32; rear=VERIFIED p24-32; leftRight=VERIFIED p21-32; top=VERIFIED p21-29; threeQuarter=VERIFIED p1 and p27-32; undersideInterior=VERIFIED p21-29 open transport frame and load connections; mechanism=PARTIAL p21-29 rotors and cargo attachment; no rotor animation sequence
- Verified findings:
  - The aircraft is a skeletal load-bearing bridge with two giant rotor pods, not a conventional enclosed helicopter fuselage.
  - Carried modules remain visibly independent beneath/within the transport frame.
  - The long crossbeam, pod spacing and open central cradle are the primary structural identity.
- Remaining evidence gaps:
  - Rotor pitch and suspension response are not authored by the manual and remain presentation interpretations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A broad industrial airlifter suspended between two giant propeller pods and a visible cargo cradle.

Non-removable identity anchors:

- twin oversized propeller pods
- open underslung cargo frame
- wide load-bearing crossbeam

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.astronauts.mx71_recon_dropship` — Both are large true-air transports. Mitigations: Tunnel Transport is carried by two giant propeller pods; MX-71 uses broad fixed wings and compact engines. / Tunnel Transport has an industrial crossbeam and open heavy cradle; MX-71 has a streamlined cockpit and rover-sized underslung bay. / Tunnel Transport reads teal, skeletal and load-first; MX-71 reads white-orange, aerodynamic and expedition-first.

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
