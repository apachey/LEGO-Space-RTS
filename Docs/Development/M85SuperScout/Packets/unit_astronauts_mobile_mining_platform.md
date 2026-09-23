# Mobile Mining Platform — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mobile_mining_platform`

**Packet state:** `T082 REFERENCE FOUNDATION ACCEPTED — T083/T085 DESIGN PENDING`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Adaptive resource extraction
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7645, 7648, 7693
- Current confidence: verified canonical identity and faction-internal construction/motion/material draft; source-bounded decisions remain explicit below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: The faction-internal construction, motion, socket and material draft is recorded below. Its unresolved asset-specific choices belong to T083/T085 design review; T082 corpus acceptance does not approve a final visual design or model.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 7645 — MT-61 Crystal Reaper | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7645)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7645-1) | PRIMARY_VERIFIED | harvesting blades, mining modules and small alien craft |
| 7648 — MT-21 Mobile Mining Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7648)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525546.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7648-1) | PRIMARY_VERIFIED | mobile mining and detachable support module |
| 7693 — ETX Alien Strike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7693)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7693-1) | PRIMARY_VERIFIED | exclusively airborne alien strike craft articulation and human mining module |

### Source audit [Astronauts:7645]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf)
- Construction map:
  - Evidence pages book 1, 3-26: Alien attack craft; supporting opposition evidence.
  - Evidence pages book 1, 27-50: White/orange mining cab and initial wheeled working platform.
  - Evidence pages book 1, 51-75; book 2, 2-17: Separate tool, drill and support assemblies for the mining system.
  - Evidence pages book 2, 18-45: Large Crystal Reaper chassis, tracked conversion and twin front harvesting-wheel installation.
  - Evidence pages book 2, 48-63: Powered controls, cables and explicit harvesting play feature.
- View/mechanism coverage: front=VERIFIED book 2 p38-66; rear=VERIFIED book 2 p44-66; leftRight=VERIFIED both books; top=VERIFIED book 2 p18-63; threeQuarter=VERIFIED covers and book 2 p43-66; undersideInterior=VERIFIED book 1 p27-75 and book 2 p18-45; mechanism=VERIFIED book 2 p45-63 powered twin harvesting wheels and tracked conversion
- Verified findings:
  - Crystal Reaper configuration is defined by two enormous exposed harvesting wheels mounted ahead of a low tracked body.
  - Two distinct articulated manipulators remain visible around the front harvesting area instead of being collapsed into the saws.
  - The substantial upper cockpit/processing assembly docks directly onto the tracked chassis and retains a clear detachable-spacecraft seam; it is not a trailer.
  - Orange structural rails, cables and motor blocks remain visible around the white mission shell.
  - The source separates cockpit craft, tool modules and running gear, supporting one configurable Mobile Mining Platform family rather than a generic sealed harvester.
- Remaining evidence gaps:
  - The source proves the detachable upper spacecraft assembly, but its exact gameplay processing role and the boundary between reusable Mobile Mining Platform chassis and Crystal-only harvesting module must be fixed during the asset-specific refit plan.

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

### Source audit [Astronauts:7693]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf)
- Construction map:
  - Evidence pages 3-16: Compact human mining vehicle with orange canopy, low four-wheel chassis and tall side tool arm.
  - Evidence pages 17-51: Large exclusively airborne alien strike craft with articulated crescent panels; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p12-16 human vehicle; rear=PARTIAL p13-16 human vehicle; leftRight=VERIFIED p3-16; top=VERIFIED p3-16; threeQuarter=VERIFIED p1 and p15-16; undersideInterior=VERIFIED p3-12 bare human chassis; mechanism=PARTIAL p13-16 hinged side tool; extraction action not demonstrated
- Verified findings:
  - The human mining module is a short low vehicle with a prominent orange cockpit and exposed wheelbase.
  - One tall side-mounted tool arm creates deliberate asymmetry around the otherwise compact body.
  - The vehicle supports the small Ore-oriented configuration of the composite Mobile Mining Platform family.
- Remaining evidence gaps:
  - The manual does not show a complete extraction cycle, so the tool contact, material intake and deployment state must be derived with corroboration from 7645/7648.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A modular mining platform whose Crystal Reaper state is dominated by twin front harvesting wheels, two manipulators and a directly docked detachable spacecraft/processing module.

Non-removable identity anchors:

- twin front harvesting wheels or alternate drill head
- two articulated manipulator arms
- directly docked detachable spacecraft/processing module
- visible crystal or ore handling path

- Rejected V1 blind-review code: `S54`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mt101_armored_drilling_unit` — Both are large white-orange ground mining machines. Mitigations: Mining Platform exposes its material path and detachable processing module; MT-101 closes paired curved shells over an open suspension chassis. / Mining Platform uses a broad harvesting head; MT-101 carries an exposed spiky two-disc drill on a slender forward boom. / Mining Platform reads as an equipment platform with cargo space; MT-101 reads as a long six-wheel breach chassis with a prominent separate crossbow-shaped Zamor launcher above its cabin.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Broad replaceable harvesting head — distinguishes Ore Drill from Crystal Reaper function; the 7645 state preserves two enormous front harvesting wheels — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Two articulated front manipulators — remain separate from the harvesting wheels and physically handle material — SOURCE_VERIFIED.
  - Directly docked detachable spacecraft/processing module — receives and routes extracted material without reading as a trailer — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Visible material path and cargo space — keep the unit economic rather than military — CANON_DERIVED_ADAPTATION.
- Structural load path: A broad low ground chassis carries the operator and directly docked upper module while a standardized front mount transfers tool load to wheels/tracks; the two manipulators route through independent side joints.
- Repeated modules / connection grammar: Shared chassis, Ore Drill head, Crystal Reaper twin-wheel head, two manipulators, detachable upper spacecraft/processing module and visible container route.
- Source-faithful versus adapted boundary: 7645 proves a detachable upper spacecraft assembly, twin harvesting wheels and two manipulators. The game keeps them inside one selectable configurable Mobile Mining Platform; 7648 and 7693 provide additional labeled donor modules rather than averaged generic mining geometry.

## E. Material and texture contract

- Geometry must carry:
  - broad harvesting head
  - detachable processing bay
  - visible material route
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Heavy wheeled/tracked ground travel; tool remains clear during movement.
- Planted/contact rule: Running gear settles before the harvesting head contacts a resource.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_HarvesterLift` | Asset_MobileMiningPlatform | travel rest to resource contact arc | authoritative extraction progress |
| `Pivot_ProcessingModule` | Asset_MobileMiningPlatform | guided release/seat path at Service infrastructure | authoritative refit progress |

- Required beats:
  - Idle material-system check.
  - Travel keeps intake raised.
  - Extraction plants, contacts, transfers material and clears.
  - Refit swaps one complete head; destruction spills inert containers.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ToolContact`, `Socket_MaterialIntake`, `Socket_Cargo`, `Socket_Module`, `Socket_AudioTool`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Cross-source donor choice for the shared chassis must be presented before production modeling.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Record named pivots, contacts and sockets; real gameplay-camera validation belongs to T084/T086 after production models exist.
4. Author only the specified reusable textures after human material review.
5. Use the accepted T082 reference foundation to prepare the T083/T085 visual design package. Do not repeat the completed three-scale review without a specific identity defect.

**State:** `T082_REFERENCE_FOUNDATION_ACCEPTED_DESIGN_PENDING`

**Approving reviewer:** game director accepted the complete T082 reference corpus on 2026-09-23; asset-specific design and production remain unapproved.
