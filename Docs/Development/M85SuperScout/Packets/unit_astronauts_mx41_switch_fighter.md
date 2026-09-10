# MX-41 Switch Fighter — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mx41_switch_fighter`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Ground / air transformer
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7647
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
| 7647 — MX-41 Switch Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7647)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7647-1) | PRIMARY_VERIFIED | six-wheel ground-to-flight transformation |

### Source audit [Astronauts:7647]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf)
- Construction map:
  - Evidence pages 3-14: Small alien attack craft; supporting opposition evidence.
  - Evidence pages 15-42: Long orange-canopy Switch Fighter hull and layered side shell.
  - Evidence pages 43-58: Separate full-span folding wing/chassis slab with six wheel mounts.
  - Evidence pages 59-64: Explicit conversion: wing tips fold, slab docks under the hull, six wheels attach, then the wing unfolds for flight.
- View/mechanism coverage: front=VERIFIED p37-64; rear=VERIFIED p41-64; leftRight=VERIFIED p15-64; top=VERIFIED p15-64; threeQuarter=VERIFIED p1 and p60-70; undersideInterior=VERIFIED p43-60 separate chassis/wing slab; mechanism=VERIFIED p58-64 physical ground-to-flight conversion
- Verified findings:
  - The ground vehicle and fighter are the same long cockpit hull carried by a separate folding wing/chassis slab.
  - Six orange wheels remain fully visible beneath the slab in ground state.
  - Transformation is readable through large wing-tip rotations and docking/undocking of the hull, not a cosmetic effect.
- Remaining evidence gaps:
  - The manual demonstrates a hand-separated hull during conversion; the production animation must define a believable continuous connection without changing the canonical two-state read.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A six-wheel surface machine whose side frames visibly fold into a pointed flight configuration.

Non-removable identity anchors:

- six-wheel ground stance
- large folding wing-side assemblies
- central white-orange rocket nose

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mission_fighter` — Both use the white-orange Mission aerospace language. Mitigations: Mission Fighter is always airborne with two swept wings; MX-41 has a six-wheel ground stance. / Mission Fighter keeps one compact flight silhouette; MX-41 exposes oversized folding side frames. / Mission Fighter has no visible transformation seam; MX-41's central rocket nose and wing-wheel hinge must read in both states.

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
