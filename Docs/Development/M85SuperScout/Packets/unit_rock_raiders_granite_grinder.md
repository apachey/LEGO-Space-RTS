# Granite Grinder — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.granite_grinder`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Anti-heavy drill walker
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4940
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
| 4940 — Granite Grinder | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4940)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128317.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4940-1) | PRIMARY_VERIFIED | drill walker construction and articulation |

### Source audit [RockRaiders:4940]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-12: Two mirrored ski-foot/leg modules and their shared upper bridge are assembled independently.
  - PDF pages 13-22: Long central drill craft body, cockpit cage, rear wheel/tool mass and drill boom are built as a separate module.
  - PDF pages 23-24: Upper drill module is mounted across the paired leg modules; final operator-scale three-quarter view.
- View/mechanism coverage: front=PARTIAL p1 and p24; rear=PARTIAL p18-22; leftRight=VERIFIED p13-24 construction rotation; top=VERIFIED p13-23; threeQuarter=VERIFIED p1 and p23-24; undersideInterior=VERIFIED p2-16 staged subassemblies; mechanism=PARTIAL p23 module connection; gait and drill motion not demonstrated
- Verified findings:
  - The recognizable walker is a bridge between two mirrored planted foot modules, not a wheeled chassis with decorative legs.
  - The drill/cockpit body is a removable longitudinal module carried above the feet.
  - The long drill is structurally balanced by rear wheel/equipment mass and a high open cage.
- Remaining evidence gaps:
  - The manual proves modular construction but not a walking gait; leg articulation and contact phases remain an explicit adaptation decision.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A tall two-legged drilling machine balanced around a long central tool.

Non-removable identity anchors:

- bipedal planted legs
- long forward drill boom
- high open operator cage

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.drill_craft` — Both use a large forward drill. Mitigations: Drill Craft stays short and wheeled; Granite Grinder is a tall planted biped. / Drill Craft has a compact low cage; Granite Grinder places the operator high above the ground. / Drill Craft is drill-first with minimal body; Granite Grinder has a long boom balanced by rear machinery.
- `unit.rock_raiders.chrome_crusher` — Both are major teal drill machines. Mitigations: Granite Grinder has two legs; Chrome Crusher has four huge wheels. / Granite Grinder is tall and narrow; Chrome Crusher is long and low. / Granite Grinder balances one boom; Chrome Crusher combines drill, work light and cargo machinery along a heavy chassis.

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
