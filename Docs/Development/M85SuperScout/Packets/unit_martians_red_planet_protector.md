# Red Planet Protector — T082 Super Scout packet

**Stable ID:** `unit.martians.red_planet_protector`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Positional anti-heavy control
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7313
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
| 7313 — Red Planet Protector | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7313)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160158.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7313-1) | PRIMARY_VERIFIED | Martian protector articulation and control mechanisms |

### Source audit [Martians:7313]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-14: Low blue/gray wedge craft with open operator area, hoses and two long removable emitter/tool arms.
  - PDF pages 15-23: Separate broad twin-foot biped lower body; the complete upper craft docks onto it to form the tall Protector.
  - PDF pages 24-28: Two independent low ground support/emitter devices are built and shown beside the complete Protector; they are not part of its body.
  - PDF pages 29-33: Final source photography and explicit hand-separated reconfiguration from biped into a low craft with the leg and central body modules relocated.
- View/mechanism coverage: front=VERIFIED p28-33; rear=PARTIAL p28-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p28-33; undersideInterior=VERIFIED p2-28 staged modules; mechanism=VERIFIED p30-33 biped-to-craft reconfiguration; continuous motion and planted control action remain missing
- Verified findings:
  - The source Protector is a modular tall biped assembled from a low wedge craft, a broad two-foot lower body and two long detachable emitter arms.
  - Its upper craft keeps a broad triangular nose and visible hoses; the separate leg blocks and side arms remain readable even after final assembly.
  - The alternate low craft is made by hand-separating and relocating major modules, so the source proves both silhouettes but not a continuous in-game planted transformation.
- Remaining evidence gaps:
  - The canonical Martian palette, anti-heavy control action and credible continuous mobile-to-planted transition require an explicit adaptation contract; the source's blue/gray paint and hand-separated rebuild do not decide them.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A modular tall biped assembled from a broad wedge craft, twin-foot lower body and two long detachable emitter arms.

Non-removable identity anchors:

- broad wedge upper craft
- separate twin-foot biped base
- paired long detachable emitter arms

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.recon_mech_rp` — Both are modular Martian bipeds built around detachable upper craft. Mitigations: Recon-Mech carries a wide asymmetric drill-and-claw span; Protector carries two matched long emitter arms. / Recon-Mech exposes a tall rear pressure tank; Protector preserves a broad wedge nose and separate torso module. / Recon-Mech relocates its leg block behind the cockpit for flight; Protector distributes both leg and torso modules around its low craft state.
- `unit.martians.excavation_searcher` — Both are large articulated Martian control machines. Mitigations: Protector is a tall twin-foot biped with a detachable upper craft; Searcher is a huge low many-legged excavation chassis. / Protector carries paired long emitter arms; Searcher separates a forward drill/claw module from a tall rear crane. / Protector changes into a compact craft; Searcher exposes an underslung material sled and irregular processing route.

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
