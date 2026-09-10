# MT-201 Ultra-Drill Walker — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mt201_ultra_drill_walker`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Deployed siege
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7649
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
| 7649 — MT-201 Ultra-Drill Walker | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7649)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538485.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538486.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7649-1) | PRIMARY_VERIFIED | drill installation-to-walker transformation |

### Source audit [Astronauts:7649]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538485.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538486.pdf)
- Construction map:
  - Evidence pages book 1, 3-20: Alien strike craft; supporting opposition evidence.
  - Evidence pages book 1, 21-46; book 2, 2-18: Low travel chassis, operator cab and central rotating machinery core.
  - Evidence pages book 2, 19-27: Massive longitudinal drill and drive assembly.
  - Evidence pages book 2, 28-34: Forward cockpit/tool pod and central module completion.
  - Evidence pages book 2, 35-43: Four independent ball-jointed leg pods attach and rotate into the deployed walker stance.
- View/mechanism coverage: front=VERIFIED book 2 p34-43; rear=VERIFIED book 2 p35-43; leftRight=VERIFIED both books; top=VERIFIED book 2 p2-43; threeQuarter=VERIFIED covers and book 2 p42-47; undersideInterior=VERIFIED book 2 p2-42; mechanism=VERIFIED book 2 p35-43 four-leg deployment and body rotation
- Verified findings:
  - MT-201 is a central drill/cockpit machine surrounded by four independently built articulated leg pods.
  - Each broad white leg has a dark planted foot and visible ball-joint connection.
  - Deployment rotates the leg set around the central machinery so travel and anchored drilling states remain visibly related.
- Remaining evidence gaps:
  - The source shows manual leg repositioning but not a timed gait or stable drilling contact sequence; both require an explicit animation contract.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A colossal drill installation that opens into a planted four-legged siege walker.

Non-removable identity anchors:

- enormous vertical drill tower
- four deployable stabilizer legs
- detachable observation craft

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mt101_armored_drilling_unit` — Both are advanced heavy Mission drilling machines. Mitigations: MT-101 remains a long six-wheel vehicle; MT-201 deploys four stabilizer legs. / MT-101's drill is a forward nose; MT-201's drill is a towering central installation. / MT-101 preserves a mobile horizontal profile; MT-201 changes to a huge vertical siege silhouette.

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
