# Rock Raider Crew — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.crew`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

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

Open question: The faction-internal construction, motion, socket and material draft is recorded below. Its unresolved decisions and the complete-roster silhouette/director gates must be cleared before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 4930 — Rock Raiders Crew | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4930)<br>no direct official PDF located | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4930-1) | CANON_VERIFIED_ARCHIVAL | crew, tools and portable equipment |

### Source audit [RockRaiders:4930]

- Evidence state: `ARCHIVAL_GAP`
- Construction map:
  - No official construction-page range is available.
- View/mechanism coverage: front=PARTIAL archival character imagery; rear=MISSING; leftRight=PARTIAL archival character imagery; top=NOT_APPLICABLE; threeQuarter=PARTIAL archival character imagery; undersideInterior=NOT_APPLICABLE; mechanism=MISSING
- Verified findings:
  - Canon and inventory confirm a crew/equipment source rather than a single vehicle assembly.
- Remaining evidence gaps:
  - Locate official or clearly labeled archival front/rear character and equipment sheets before fixing the Crew backpack, lamp and tool variants.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A minifigure-scale industrial crew member led by a carried tool, not a generic infantryman.

Non-removable identity anchors:

- helmet-and-visor crew profile
- oversized portable mining or repair tool
- compact backpack and work-light mass

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.astronauts.expedition_crew` — Both are tiny minifigure-scale workers. Mitigations: Raider Crew leads with an oversized industrial hand tool; Expedition Crew leads with a standardized modular attachment. / Raider Crew uses helmet/visor, compact work light and dark-teal industrial blocks; Expedition Crew uses sealed astronaut helmet and white lineage markings. / Raider Crew reads improvised and tool-specific; Expedition Crew reads standardized and mission-configurable.

## D. Construction contract

- Contract state: `SOURCE_BOUNDED_PROVISIONAL`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Helmet/visor silhouette — identifies protected industrial personnel rather than infantry — CANON_DERIVED_ADAPTATION.
  - Portable tool — performs extraction, construction, repair and weak contact defense — CANON_DERIVED_ADAPTATION.
  - Backpack/work lamp — carries compact service equipment and keeps the worker readable beside machinery — SOURCE_BOUNDED_PROVISIONAL.
- Structural load path: Minifigure-derived torso and hips carry the helmet, backpack and two-handed tool; the tool must remain visibly portable rather than body-mounted.
- Repeated modules / connection grammar: Shared crew body plus swappable mining/repair tool and restrained backpack/lamp variants.
- Source-faithful versus adapted boundary: Canon fixes the worker/tool read, but the missing 4930 rear evidence prevents locking one exact backpack and lamp arrangement.

## E. Material and texture contract

- Geometry must carry:
  - helmet and visor profile
  - portable tool head
  - backpack/lamp mass
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
  - Choose the production backpack/lamp variant after a verified 4930 rear/equipment source is found or the game director explicitly approves a bounded adaptation.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
