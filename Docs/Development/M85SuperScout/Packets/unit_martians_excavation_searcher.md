# Excavation Searcher — T082 Super Scout packet

**Stable ID:** `unit.martians.excavation_searcher`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Siege / manipulation walker
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7316
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
| 7316 — Excavation Searcher | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7316)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130811.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7316-1) | PRIMARY_VERIFIED | multi-leg excavation, crane and material handling |

### Source audit [Martians:7316]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-16: Forward tan/orange Excavation Searcher module with long drill, paired claws and separately built planted tool/leg assemblies.
  - PDF pages 17-30: Large irregular rear body, multiple spaced legs and tall articulated crane assemble, then dock to the forward module.
  - PDF pages 31-33: Independent low material sled/container builds and docks beneath the complete Searcher.
  - PDF pages 34-57: Separate dark excavation support rig with arches, hoses and human operator; useful opposition/industrial evidence, not direct Martian Searcher geometry.
  - PDF pages 58-59: Cross-set alternate humanoid rebuild; not a demonstrated primary Searcher transformation.
- View/mechanism coverage: front=VERIFIED p1 and p27-33; rear=PARTIAL p27-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p27-33; undersideInterior=VERIFIED p2-33 staged modules and sled; mechanism=PARTIAL p28-33 crane/claws/module docking; no primary gait, siege cycle or full material route
- Verified findings:
  - The Martian Excavation Searcher is a huge low many-legged machine assembled from visibly separate forward tool, rear processing/crane and underslung sled modules.
  - A long drill, paired orange claws and tall crane create three different working directions around the irregular body instead of one humanoid front.
  - The low sled demonstrates material handling beneath the chassis, while the separate dark rig and final humanoid rebuild must not be mistaken for the primary Martian silhouette.
- Remaining evidence gaps:
  - The manual does not provide a walking gait, supported siege contact sequence, complete crane-to-processor route or game's manipulation attack; these require later semantic and motion contracts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A gigantic many-legged excavation machine carrying an unmistakable crane-claw and material-handling body.

Non-removable identity anchors:

- multi-legged planted base
- large articulated crane-claw
- high irregular processing superstructure

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.red_planet_protector` — Both are large articulated Martian control machines. Mitigations: Protector is a tall twin-foot biped with a detachable upper craft; Searcher is a huge low many-legged excavation chassis. / Protector carries paired long emitter arms; Searcher separates a forward drill/claw module from a tall rear crane. / Protector changes into a compact craft; Searcher exposes an underslung material sled and irregular processing route.

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
