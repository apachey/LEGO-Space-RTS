# Alien Jet — T082 Super Scout packet

**Stable ID:** `unit.aliens.alien_jet`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Air scout / interceptor
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 5617
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
| 5617 — Alien Jet | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/5617)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=5617-1) | PRIMARY_VERIFIED | small alien jet and compact ETX grammar |

### Source audit [Aliens:5617]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf)
- Construction map:
  - Evidence pages 1: Complete five-step Alien Jet build with exposed pilot, swept black deck and paired flexible lime arches.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 staged build; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-5; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=PARTIAL p1 flexible lime arches; no authored flight or weapon motion
- Verified findings:
  - Alien Jet is an extremely small open craft built around a broad swept black plate rather than an enclosed fuselage.
  - Two tall flexible lime arches rise over the exposed pilot and dominate the profile from the front and side.
  - A single forward yellow emitter and short rear equipment block keep the craft directional despite its minimal body.
- Remaining evidence gaps:
  - Clean rear and underside views are still required before fixing propulsion, landing and weapon sockets for the production Alien Jet.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny open black-lime interceptor with a swept plate body and two bright arches above its pilot.

Non-removable identity anchors:

- broad swept plate body
- exposed central alien pilot
- paired tall lime conduit arches

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.aliens.razor_skimmer` — Both are small swept black-lime attack craft. Mitigations: Alien Jet has a visible airborne swept-plate profile; Razor Skimmer stays almost flat against the ground. / Alien Jet raises two bright conduit arches over an open pilot; Razor Skimmer projects two long forward razor prongs. / Alien Jet centers on its pilot; Razor Skimmer centers on an exposed lime energy core.

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
