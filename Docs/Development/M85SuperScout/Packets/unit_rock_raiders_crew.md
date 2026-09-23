# Rock Raider Crew — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.crew`

**Packet state:** `T082 REFERENCE FOUNDATION ACCEPTED — T083/T085 DESIGN PENDING`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Worker / engineer
- Authoritative footprint: `Tiny`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4930
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
| 4930 — Rock Raiders Crew | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4930)<br>no direct official PDF located<br>[archival evidence 1](https://kb.rockraidersunited.com/4930_Rock_Raiders_Crew)<br>[archival evidence 2](https://kb.rockraidersunited.com/images/b/bd/4930_OutBox.jpg)<br>[archival evidence 3](https://www.bricklink.com/catalogItemInv.asp?S=4930-1) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4930-1) | CANON_VERIFIED_ARCHIVAL | five named crew minifigures, small equipment/control stand, loose handheld tools and crystal boulder; no major vehicle |

### Source audit [RockRaiders:4930]

- Evidence state: `ARCHIVAL_PRODUCT_VISUALLY_AUDITED`
- Evidence links: [archival product reference 1](https://kb.rockraidersunited.com/4930_Rock_Raiders_Crew), [archival product reference 2](https://kb.rockraidersunited.com/images/b/bd/4930_OutBox.jpg), [archival product reference 3](https://www.bricklink.com/catalogItemInv.asp?S=4930-1)
- Construction map:
  - No construction-page range is available for this evidence type.
- View/mechanism coverage: front=VERIFIED archival box and out-of-box photos; rear=MISSING; leftRight=PARTIAL archival out-of-box photo; top=NOT_APPLICABLE minifigure/equipment pack; threeQuarter=VERIFIED archival box and out-of-box photos; undersideInterior=NOT_APPLICABLE; mechanism=PARTIAL loose handheld tools and equipment stand; no authored action sequence
- Verified findings:
  - The set contains the five named crew minifigures Axle, Bandit, Docs, Jet and Sparks; the figures, not a vehicle, are its primary playable identity.
  - The remaining source material is a very small equipment/control stand, loose tools including a large handheld saw assembly, and a boulder containing an energy crystal.
  - 4930 provides crew appearance and portable-equipment variation. It does not justify a crew vehicle, a large building or one fixed backpack shared by all five characters.
- Remaining evidence gaps:
  - Rear printing and exact backpack/lamp combinations for all five characters remain incomplete in the located product photos; production must use other verified crew appearances or present the variant choice to the game director.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A minifigure-scale industrial crew member led by a carried tool, not a generic infantryman.

Non-removable identity anchors:

- helmet-and-visor crew profile
- oversized portable mining or repair tool
- compact backpack and work-light mass

- Rejected V1 blind-review code: `S31`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.astronauts.expedition_crew` — Both are tiny minifigure-scale workers. Mitigations: Raider Crew leads with an oversized industrial hand tool; Expedition Crew leads with a standardized modular attachment. / Raider Crew uses helmet/visor, compact work light and dark-teal industrial blocks; Expedition Crew uses sealed astronaut helmet and white lineage markings. / Raider Crew reads improvised and tool-specific; Expedition Crew reads standardized and mission-configurable.
- `unit.martians.worker_robot` — At far scale both reduce to a tiny upright worker with two planted contacts. Mitigations: Raider Crew keeps a full minifigure torso above short legs; Worker Robot suspends a shallow rider wedge between two long mechanical legs. / Raider Crew carries one large tool outside the body; Worker Robot's small adapted tool remains subordinate to the biped frame. / Raider Crew's negative space is between arms and tool; Worker Robot must retain a tall open gap between its legs.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Five named crew variants — Axle, Bandit, Docs, Jet and Sparks provide distinct headwear, torso color and face reads inside one shared worker class — SOURCE_VERIFIED.
  - Portable tool loadout — draws from the source's loose drill, shovel, scanner and large handheld saw equipment while communicating extraction, construction or repair — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Variant-specific helmet, visor, goggles or cap — preserves character identity instead of forcing one generic uniform silhouette — SOURCE_VERIFIED.
  - Compact lamp/back equipment — keeps work function readable beside machines without becoming a permanent weapon pack — CANON_DERIVED_ADAPTATION.
- Structural load path: A minifigure-derived torso and hips carry variant-specific headwear and a handheld tool; any lamp/back equipment attaches to the torso and remains visibly lighter than the carried work tool.
- Repeated modules / connection grammar: Shared movement/interaction rig plus five appearance variants, swappable mining/repair tools and restrained optional lamp/back equipment. The tiny 4930 equipment stand remains a world/support motif, not part of the Crew body.
- Source-faithful versus adapted boundary: 4930 proves five distinct people and loose work equipment, not one standardized soldier. Production may unify animation and gameplay sockets but may not erase the character variation or invent a 4930-derived vehicle.

## E. Material and texture contract

- Geometry must carry:
  - variant-specific headwear and visor profile
  - portable tool head
  - optional compact lamp/back-equipment mass
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Foot; quick, compact steps with the carried tool kept clear of the legs.
- Planted/contact rule: Both feet alternate contact during travel; work and repair settle into a stable two-foot stance.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_Tool` | Asset_RockRaiderCrew | shoulder/hand arc from carried rest to contact pose | work, repair, construction or contact-attack presentation progress |
| `Pivot_Lamp` | Asset_RockRaiderCrew | fixed mount; optional narrow aim follow only if the chosen source variant supports it | visibility and operating state |

- Required beats:
  - Idle weight shift and equipment check.
  - Foot locomotion with restrained tool counter-swing.
  - Extract/build/repair uses anticipation, visible tool contact, repeated work cycle and settle.
  - Damage interrupts the work cycle; destruction separates tool/backpack before the body collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ToolContact`, `Socket_WorkVfx`, `Socket_Lamp`, `Socket_AudioTool`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Choose whether normal production cycles the five named appearance variants evenly or weights particular specialists; rear printing and exact lamp/backpack combinations still require additional crew imagery or director approval.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Record named pivots, contacts and sockets; real gameplay-camera validation belongs to T084/T086 after production models exist.
4. Author only the specified reusable textures after human material review.
5. Use the accepted T082 reference foundation to prepare the T083/T085 visual design package. Do not repeat the completed three-scale review without a specific identity defect.

**State:** `T082_REFERENCE_FOUNDATION_ACCEPTED_DESIGN_PENDING`

**Approving reviewer:** game director accepted the complete T082 reference corpus on 2026-09-23; asset-specific design and production remain unapproved.
