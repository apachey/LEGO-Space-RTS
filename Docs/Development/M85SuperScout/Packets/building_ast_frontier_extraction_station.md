# Frontier Extraction Station — T082 Super Scout packet

**Stable ID:** `building.ast.frontier_extraction_station`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Infrastructure`
- Gameplay role: Resource processing
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7691, 7648
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
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |
| 7648 — MT-21 Mobile Mining Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7648)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7648-1) | PRIMARY_VERIFIED | mobile mining and detachable support module |

### Source audit [Astronauts:7691]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf)
- Construction map:
  - Evidence pages 3-27: Human extraction station with long rail/sled, upright operator rig, flexible transfer hose and canister.
  - Evidence pages 28-49: Large circular alien mothership body; supporting opposition evidence.
  - Evidence pages 50-68: Alien subcraft and attachment mechanisms; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p17-27 human station; rear=PARTIAL p20-27 human station; leftRight=VERIFIED p3-27; top=VERIFIED p3-27; threeQuarter=VERIFIED p1 and p21-27; undersideInterior=VERIFIED p3-23 staged human station; mechanism=VERIFIED p20-27 hose/canister extraction play; station processing cycle remains partial
- Verified findings:
  - The human station is a narrow linear extraction rig rather than a broad factory.
  - A low rail/sled leads to a small upright operator tower with a visible hose and external material canister.
  - The compact open structure supports a frontier receiver identity but does not justify a sealed processing building.
- Remaining evidence gaps:
  - The exact resource intake, storage and outgoing process for the composite Frontier Extraction Station must be added from other verified Mars Mission mining sources.

### Source audit [Astronauts:7648]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf)
- Construction map:
  - Evidence pages 3-16: Compact orange/white mining rover with exposed low chassis and large wheels.
  - Evidence pages 17-25: Separate tall articulated extraction/tool mast on a small wheeled base.
  - Evidence pages 26-28: Both modules shown together at operator scale.
- View/mechanism coverage: front=PARTIAL p15-28; rear=PARTIAL p15-28; leftRight=VERIFIED p3-28; top=VERIFIED p3-25; threeQuarter=VERIFIED p1 and p25-28; undersideInterior=VERIFIED p3-20 staged chassis; mechanism=PARTIAL p17-25 hinged tool mast; extraction cycle not demonstrated
- Verified findings:
  - The source is a paired mining system: a compact rover and a visibly independent upright tool platform.
  - Both machines use low exposed white frames with orange wheel or equipment masses rather than armored hulls.
  - The tall mast gives the support module a distinct vertical read beside the horizontal rover.
- Remaining evidence gaps:
  - The game combines this source with other mining vehicles, so the retained mini-robot/support-module relationship must be defined without creating an extra buildable unit.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A compact expedition station that visibly receives drilled material and transfers it into sealed mission containers.

Non-removable identity anchors:

- drill-facing receiving hopper
- sealed crystal or ore container rack
- small pneumatic transfer mast

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

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
