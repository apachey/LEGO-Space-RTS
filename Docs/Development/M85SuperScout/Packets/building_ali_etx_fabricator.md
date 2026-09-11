# ETX Fabricator — T082 Super Scout packet

**Stable ID:** `building.ali.etx_fabricator`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Infrastructure`
- Gameplay role: Light production
- Authoritative footprint: `Large`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 7691, 7690
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
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |
| 7690 — MB-01 Eagle Command Base | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7690)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7690-1) | PRIMARY_VERIFIED | human command base, transfer system and service architecture |

### Source audit [Aliens:7691]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf)
- Construction map:
  - Evidence pages 3-27: Astronaut extraction station; supporting opposition evidence.
  - Evidence pages 28-49: Large circular Mothership carrier hull, open central machinery channel, lime conduits, docking hardpoints and long multi-blade tail.
  - Evidence pages 50-56: Two mirrored narrow seated weapon craft are built as independent modules for the carrier.
  - Evidence pages 57-63: A long lime-conduit front/central craft is assembled independently and docked into the carrier's open machinery channel.
  - Evidence pages 64-67: Two disc-like alien jetpack modules are built separately, accept individual aliens and dock at the carrier's outer wing/arm ends.
- View/mechanism coverage: front=VERIFIED p43-49; rear=VERIFIED p44-49; leftRight=VERIFIED p28-49; top=VERIFIED p28-49; threeQuarter=VERIFIED cover and p43-50; undersideInterior=VERIFIED p28-48 staged circular frame; interior remains open rather than enclosed; mechanism=VERIFIED p50-67 detachable subcraft and capture/weapon pods; carrier launch cycle remains partial
- Verified findings:
  - The Mothership is one huge flattened circular black carrier interrupted by a visible central machinery channel rather than a sealed saucer.
  - Several long black and translucent-lime tail blades extend from one side, preventing a rotationally symmetric disc silhouette.
  - Its separately assembled front craft, two seated side craft and two jetpack modules are contained/docked parts of the flagship presentation: they establish how the single Mothership can open, unfold and expose internal craft bays rather than defining separate Mothership entities.
- Remaining evidence gaps:
  - The manual proves docked subcraft but not the game's launch/recovery timing, unfolding sequence, reinforcement function or Charge-support state; all require a single-unit carrier contract.
  - Training eligible Alien units inside the Mothership is a game-director proposal. The exact roster, cost, build time, capacity interaction and whether production requires an unfolded state remain unresolved gameplay decisions and are not locked by T082.

### Source audit [Aliens:7690]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf)
- Construction map:
  - Evidence pages book 1, 3-15: Complete compact Alien scout craft with paired curved outer lobes, central open pilot/tool frame, lime arches and long rear fin.
  - Evidence pages book 1, 16-75; book 2, 2-72: Astronaut command base, gantry and shuttle; supporting containment/docking opposition evidence, not small-craft geometry.
- View/mechanism coverage: front=VERIFIED book 1 p12-15; rear=PARTIAL book 1 p10-15; leftRight=VERIFIED book 1 p3-15; top=VERIFIED book 1 p3-15; threeQuarter=VERIFIED cover and book 1 p12-15; undersideInterior=VERIFIED book 1 p3-12 staged open frame; mechanism=PARTIAL book 1 p12-15 flexible conduits and weapon mounts; no locomotion sequence
- Verified findings:
  - The scout uses two broad curved black lobes around an exposed central operator/tool frame, producing a broken crescent silhouette.
  - Paired lime conduits arch forward over the center while several small yellow emitters remain distributed across the lobes.
  - One long lime rear fin gives the otherwise round craft a strong directional tail.
- Remaining evidence gaps:
  - The source does not establish whether the consolidated Razor/Servitor family retains the tail fin, distributed emitters or central exposed operator, so those choices must be resolved in the cross-source construction map.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low docked alien hull that assembles small craft along an open forward launch channel.

Non-removable identity anchors:

- curved docked-hull shell
- open forward fabrication channel
- overhead crystal feed arms

- Rejected V1 blind-review code: `S15`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `building.ali.reconfiguration_dock` — Both are craft-derived Alien production structures. Mitigations: Fabricator is low and forward-open; Reconfiguration Dock is huge and vertically split. / Fabricator moves small craft along one channel; Reconfiguration Dock suspends a transforming craft in a central cradle. / Fabricator uses compact feed arms; Reconfiguration Dock exposes multiple large hinge arcs.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Low curved docked-hull shell — keeps light production spacecraft-derived — CANON_DERIVED_ADAPTATION.
  - Open forward fabrication/launch channel — shows small craft assembly and exit — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Overhead crystal-feed arms — route components and energy into the craft — CANON_DERIVED_ADAPTATION.
- Structural load path: Two low hull halves carry feed arms around one unobstructed forward channel and transfer loads into outer landing supports.
- Repeated modules / connection grammar: Docked shell halves, production cradle, feed arms, launch channel and resource interface.
- Source-faithful versus adapted boundary: New structure uses 7691 bay and 7690 small-craft grammar; no terrestrial conveyor factory or sealed black box.

## E. Material and texture contract

- Geometry must carry:
  - low curved shell
  - open forward channel
  - paired overhead feed arms
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_crystal_containment_mask` — Facet-localized Charge intensity and containment contact masks without baked glow or fake crystal depth. Channels: Linear roughness, transmission control and separate emission mask. Resolution: 1024x1024; texel density: Object-local crystal atlas; not world-density bound.; tiling: Non-tiling per approved crystal form.; LOD fallback: One faceted Glass mass and bounded emission at Strategic. Provenance/state: Project-authored procedural crystal source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary unfolded structure.
- Planted/contact rule: Outer hull supports remain planted; launch channel stays clear.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_FeedArmLeft` | Asset_ETXFabricator | resource rest to assembly contact arc | production progress |
| `Pivot_FeedArmRight` | Asset_ETXFabricator | mirrored assembly arc | production progress |

- Required beats:
  - Construction opens the low shell.
  - Idle arms park above channel edges.
  - Production assembles visible modules and releases forward.
  - Damage disables an arm; collapse avoids hiding the authoritative exit.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ProductionExit`, `Socket_Assembly`, `Socket_ResourceReceive`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact Mothership-bay and 7690 donor modules require a composed-design proposal before approval.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
