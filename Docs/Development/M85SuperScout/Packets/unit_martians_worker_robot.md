# Worker Robot — T082 Super Scout packet

**Stable ID:** `unit.martians.worker_robot`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Worker / builder
- Authoritative footprint: `Tiny`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7302
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
| 7302 — Worker Robot | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7302)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130290.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7302-1) | PRIMARY_VERIFIED | Martian worker walker construction |

### Source audit [Martians:7302]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130290.pdf)
- Construction map:
  - Evidence pages 1: The Martian figure and shallow central operator wedge are built separately; the wedge includes an exposed seat/control tile and rear wall.
  - Evidence pages 2: Two mirrored multi-joint leg assemblies, each ending in one broad rectangular foot, attach to the left and right sides of the central wedge; the final seated biped and cover walking pose are shown.
- View/mechanism coverage: front=VERIFIED p1 cover and p2 final; rear=PARTIAL p1-2 sequence; leftRight=VERIFIED p1 cover and p2 steps 5-7; top=VERIFIED p1-2; threeQuarter=VERIFIED p1 cover and p2 final; undersideInterior=PARTIAL p1 bare base and p2 separate feet; mechanism=PARTIAL p2 mirrored leg joints plus cover walking pose; no complete gait or tool action sequence
- Verified findings:
  - The source machine is a tiny open bipedal walker with two mirrored articulated legs; the earlier four-spoke interpretation was incorrect.
  - Each leg terminates in one broad foot, producing exactly two ground contacts around the shallow central operator wedge.
  - The source has no dedicated arm or work tool, so the canonical builder/repair manipulator must be a clearly labeled subordinate adaptation that does not erase the two-leg silhouette.
- Remaining evidence gaps:
  - The manual proves leg topology and a walking pose but not a full planted gait cycle.
  - The production contract must choose a compact worker tool mount without converting the small biped into a generic humanoid robot or a multi-legged platform.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny open Martian biped whose shallow operator wedge rides between two long articulated legs and broad feet.

Non-removable identity anchors:

- shallow central operator wedge
- two mirrored multi-joint legs
- two broad rectangular feet

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.aliens.etx_servitor` — Both are small mechanical nonhuman workers. Mitigations: Servitor hovers inside one low curved shell; Worker Robot walks on two long articulated legs. / Servitor encloses its core in black-lime structure; Worker Robot leaves the seated Martian visible between two broad feet. / Servitor uses one dominant folding manipulator; Worker Robot's adapted tool must remain subordinate to its bipedal source silhouette.

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
