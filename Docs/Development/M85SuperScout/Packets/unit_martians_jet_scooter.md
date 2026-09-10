# Jet Scooter — T082 Super Scout packet

**Stable ID:** `unit.martians.jet_scooter`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Ground-hover harassment
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 7303
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
| 7303 — Jet Scooter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7303)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130291.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7303-1) | PRIMARY_VERIFIED | Martian light attack scooter |

### Source audit [Martians:7303]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130291.pdf)
- Construction map:
  - Evidence pages 1: Complete seven-step Jet Scooter build with long blue central deck, parallel side tubes, clustered orange nose nozzles and open rider.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 steps 3-7; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 1-7; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=MISSING static micro-build only
- Verified findings:
  - Jet Scooter is a long narrow open sled rather than a short body between two oversized engine pods.
  - Parallel exposed side tubes reinforce the central spine, while a cluster of small orange nozzles makes the nose strongly directional.
  - The rider and rounded blue rear equipment remain above the otherwise thin deck.
- Remaining evidence gaps:
  - The source does not establish hover height, propulsion cycle or a canonical attack emitter; those remain production motion and socket decisions.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long narrow open sled whose paired side tubes and clustered orange nose nozzles frame the rider spine.

Non-removable identity anchors:

- long thin central deck
- paired exposed side tubes
- clustered orange forward nozzles

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.double_hover` — Both are tiny open Martian ground-hover craft. Mitigations: Double Hover has two long forward runners and unlike rear modules; Jet Scooter has one long spine with paired side tubes. / Double Hover ends in blunt parallel forks; Jet Scooter points a cluster of orange nozzles forward. / Double Hover reads short and laterally offset; Jet Scooter reads narrow and aggressively directional.
- `unit.aliens.razor_skimmer` — Both are small fast hover harassment units. Mitigations: Razor Skimmer is a broad black blade plan; Jet Scooter is a long thin open sled around its rider. / Razor centers on a lime core between two large prongs; Jet Scooter points a cluster of small orange nozzles ahead of paired side tubes. / Razor's propulsion is visually integrated into the hull; Jet Scooter leaves its tubes, deck and rear equipment exposed.

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
