# Mobile Mining Platform — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mobile_mining_platform`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Adaptive resource extraction
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7645, 7648, 7693
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
| 7645 — MT-61 Crystal Reaper | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7645)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7645-1) | PRIMARY_VERIFIED | harvesting blades, mining modules and small alien craft |
| 7648 — MT-21 Mobile Mining Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7648)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7648-1) | PRIMARY_VERIFIED | mobile mining and detachable support module |
| 7693 — ETX Alien Strike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7693)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7693-1) | PRIMARY_VERIFIED | alien strike transformation and human mining module |

### Set 7645 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages book 1, 3-26: Alien attack craft; supporting opposition evidence.
  - PDF pages book 1, 27-50: White/orange mining cab and initial wheeled working platform.
  - PDF pages book 1, 51-75; book 2, 2-17: Separate tool, drill and support assemblies for the mining system.
  - PDF pages book 2, 18-45: Large Crystal Reaper chassis, tracked conversion and twin front harvesting-wheel installation.
  - PDF pages book 2, 48-63: Powered controls, cables and explicit harvesting play feature.
- View/mechanism coverage: front=VERIFIED book 2 p38-66; rear=VERIFIED book 2 p44-66; leftRight=VERIFIED both books; top=VERIFIED book 2 p18-63; threeQuarter=VERIFIED covers and book 2 p43-66; undersideInterior=VERIFIED book 1 p27-75 and book 2 p18-45; mechanism=VERIFIED book 2 p45-63 powered twin harvesting wheels and tracked conversion
- Verified findings:
  - Crystal Reaper configuration is defined by two enormous exposed harvesting wheels mounted ahead of a low tracked body.
  - Orange structural rails, cables and motor blocks remain visible around the white mission shell.
  - The source separates cab, tool modules and running gear, supporting one configurable Mobile Mining Platform family rather than a generic sealed harvester.
- Remaining evidence gaps:
  - The exact boundary between reusable Mobile Mining Platform chassis and Crystal-only harvesting module must be fixed during the asset-specific refit plan.

### Set 7648 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 3-16: Compact orange/white mining rover with exposed low chassis and large wheels.
  - PDF pages 17-25: Separate tall articulated extraction/tool mast on a small wheeled base.
  - PDF pages 26-28: Both modules shown together at operator scale.
- View/mechanism coverage: front=PARTIAL p15-28; rear=PARTIAL p15-28; leftRight=VERIFIED p3-28; top=VERIFIED p3-25; threeQuarter=VERIFIED p1 and p25-28; undersideInterior=VERIFIED p3-20 staged chassis; mechanism=PARTIAL p17-25 hinged tool mast; extraction cycle not demonstrated
- Verified findings:
  - The source is a paired mining system: a compact rover and a visibly independent upright tool platform.
  - Both machines use low exposed white frames with orange wheel or equipment masses rather than armored hulls.
  - The tall mast gives the support module a distinct vertical read beside the horizontal rover.
- Remaining evidence gaps:
  - The game combines this source with other mining vehicles, so the retained mini-robot/support-module relationship must be defined without creating an extra buildable unit.

### Set 7693 source audit

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 3-16: Compact human mining vehicle with orange canopy, low four-wheel chassis and tall side tool arm.
  - PDF pages 17-51: Large transforming alien strike craft; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p12-16 human vehicle; rear=PARTIAL p13-16 human vehicle; leftRight=VERIFIED p3-16; top=VERIFIED p3-16; threeQuarter=VERIFIED p1 and p15-16; undersideInterior=VERIFIED p3-12 bare human chassis; mechanism=PARTIAL p13-16 hinged side tool; extraction action not demonstrated
- Verified findings:
  - The human mining module is a short low vehicle with a prominent orange cockpit and exposed wheelbase.
  - One tall side-mounted tool arm creates deliberate asymmetry around the otherwise compact body.
  - The vehicle supports the small Ore-oriented configuration of the composite Mobile Mining Platform family.
- Remaining evidence gaps:
  - The manual does not show a complete extraction cycle, so the tool contact, material intake and deployment state must be derived with corroboration from 7645/7648.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A modular mining platform whose harvesting head and detachable processing bay dominate a noncombat chassis.

Non-removable identity anchors:

- wide harvesting or drill head
- detachable processing module
- visible crystal or ore handling path

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mt101_armored_drilling_unit` — Both are large white-orange ground mining machines. Mitigations: Mining Platform exposes its material path and detachable processing module; MT-101 encloses the tool in an armored assault nose. / Mining Platform uses a broad harvesting head; MT-101 uses one central heavy drill. / Mining Platform reads as an equipment platform with cargo space; MT-101 reads as a long six-wheel breach chassis.

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
