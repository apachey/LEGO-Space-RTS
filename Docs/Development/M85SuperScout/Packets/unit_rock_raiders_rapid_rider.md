# Rapid Rider — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.rapid_rider`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Crew transport
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4920
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
| 4920 — Rapid Rider | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4920)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128168.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4920-1) | PRIMARY_VERIFIED | twin-hull transport and propulsion |

### Set 4920 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1-2: Complete twin-hull Rapid Rider build, central deck, raised canopy/bridge, rear drive equipment and carried rock load.
- View/mechanism coverage: front=PARTIAL p1 cover and p2 final; rear=PARTIAL p2 steps 10-14; leftRight=PARTIAL p1-2 construction sequence; top=VERIFIED p1-2; threeQuarter=VERIFIED p1 cover and p2 final; undersideInterior=PARTIAL p1 steps 1-4 expose hull foundations; mechanism=PARTIAL p2 rear propulsion and cargo placement; no movement sequence
- Verified findings:
  - Two long parallel hulls remain separate around a narrow central deck.
  - The open load/passenger zone is structural negative space and must not be roofed over.
  - Rear propulsion and carried cargo are visually subordinate to the twin-hull plan.
- Remaining evidence gaps:
  - Acquire a clean orthogonal rear view before final propulsion placement.
  - Amphibious hover behavior is canonical adaptation and is not demonstrated by the static manual.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A twin-hull underground water skimmer with a clearly open cargo/passenger gap.

Non-removable identity anchors:

- parallel catamaran hulls
- rear propulsion pair
- open central cargo and rider space

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.hover_scout` — Both are small low Rock Raider utility craft. Mitigations: Hover Scout has one flat survey deck; Rapid Rider has two parallel hulls. / Hover Scout carries a forward scanner; Rapid Rider carries paired rear propulsion. / Hover Scout reads as single-seat information equipment; Rapid Rider preserves an open passenger/cargo gap.

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
