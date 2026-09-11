# ETX Servitor — T082 Super Scout packet

**Stable ID:** `unit.aliens.etx_servitor`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Worker / builder
- Authoritative footprint: `Tiny`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 5617, 7646, 7691
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
| 5617 — Alien Jet | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/5617)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=5617-1) | PRIMARY_VERIFIED | small alien jet and compact ETX grammar |
| 7646 — ETX Alien Infiltrator | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7646)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7646-1) | PRIMARY_VERIFIED | alien craft-to-walker transformation |
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |

### Source audit [Aliens:5617]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf)
- Construction map:
  - Evidence pages 1: Complete five-step Alien Jet build with exposed pilot, swept black deck and paired flexible lime arches.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 staged build; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-5; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=PARTIAL p1 flexible lime arches; no authored flight or weapon motion
- Verified findings:
  - Alien Jet is an extremely small open craft built around a broad swept black plate rather than an enclosed fuselage.
  - Two tall flexible lime arches rise over the exposed pilot and dominate the profile from the front and side.
  - A single forward yellow emitter and short rear equipment block keep the craft directional despite its minimal body.
- Remaining evidence gaps:
  - Clean rear and underside views are still required before fixing propulsion, landing and weapon sockets for the production Alien Jet.

### Source audit [Aliens:7646]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf)
- Construction map:
  - Evidence pages 3-27: Astronaut equipment and figures; supporting opposition evidence.
  - Evidence pages 29-50: Long central Infiltrator craft with split nose, exposed crew/tool bay, red weapon tips and lime conduits.
  - Evidence pages 51-67: Two independent curved side modules attach to the central craft and receive weapons, cables and flexible lime conduits.
  - Evidence pages 68: Explicit conversion: both curved side modules and the long forward hull rotate downward into a planted three-leg walker.
- View/mechanism coverage: front=VERIFIED p49-50 and p67-68; rear=VERIFIED p47-50 and p67-68; leftRight=VERIFIED p29-68; top=VERIFIED p29-67; threeQuarter=VERIFIED cover and p49-50/p67-68; undersideInterior=VERIFIED p29-67 staged modules; mechanism=VERIFIED p68 craft-to-three-leg walker conversion; gait and weapon cycle remain partial
- Verified findings:
  - Infiltrator flight state is a long central two-seat craft flanked by two separately built crescent modules.
  - The walker has three planted members, not a generic four- or six-legged spider: the two curved side modules and the long split nose rotate downward.
  - Flexible lime conduits visibly link the crew/energy core to the moving side modules, making the transformation mechanical rather than magical.
- Remaining evidence gaps:
  - The source proves the large rotations but not a continuous gait, stable planted attack pose, heavy-target weapon path or detector sweep; those remain explicit production motion contracts.

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

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny black-lime hover worker wrapped around one crystal-handling manipulator, not a humanoid robot.

Non-removable identity anchors:

- low curved hover shell
- single crystal cradle
- folding utility manipulator

- Rejected V1 blind-review code: `S49`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.astronauts.expedition_crew` — Both are tiny mobile economy/build units. Mitigations: Expedition Crew is upright and bipedal; ETX Servitor is a low hover shell. / Expedition Crew carries tools externally; Servitor wraps around a crystal cradle. / Expedition Crew uses human helmet/backpack masses; Servitor uses a folding single manipulator and no humanoid body.
- `unit.martians.worker_robot` — Both are small mechanical nonhuman workers. Mitigations: Servitor hovers inside one low curved shell; Worker Robot walks on two long articulated legs. / Servitor encloses its core in black-lime structure; Worker Robot leaves the seated Martian visible between two broad feet. / Servitor uses one dominant folding manipulator; Worker Robot's adapted tool must remain subordinate to its bipedal source silhouette.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Low curved hover shell — keeps the worker spacecraft-derived and non-humanoid — CANON_DERIVED_ADAPTATION.
  - Central crystal cradle — receives Ore/Crystals and exposes the economy function — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Single folding utility manipulator — harvests, builds and repairs through visible mechanical contact — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A shallow crescent frame carries the hover mechanism and central cradle; one articulated arm routes work load into the frame without creating legs.
- Repeated modules / connection grammar: Hover shell, crystal cradle, folding clamp/tool and rear energy feed derive from 5617, 7646 and 7691 mechanical grammar.
- Source-faithful versus adapted boundary: No official dedicated Alien worker exists. Exact donors must be disclosed before modeling; no humanoid robot, insect or floating crystal with unexplained parts.

## E. Material and texture contract

- Geometry must carry:
  - low crescent hover shell
  - central crystal cradle
  - single folding manipulator
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_crystal_containment_mask` — Facet-localized Charge intensity and containment contact masks without baked glow or fake crystal depth. Channels: Linear roughness, transmission control and separate emission mask. Resolution: 1024x1024; texel density: Object-local crystal atlas; not world-density bound.; tiling: Non-tiling per approved crystal form.; LOD fallback: One faceted Glass mass and bounded emission at Strategic. Provenance/state: Project-authored procedural crystal source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Ground-layer hover with fast lateral-stable movement and no true-air banking.
- Planted/contact rule: No wheel/foot contact; construction settles the shell low and plants the manipulator at the work point.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ManipulatorBase` | Asset_ETXServitor | stowed shell line to forward/downward work arc | harvest, construction or repair progress |
| `Pivot_CrystalClamp` | Pivot_ManipulatorBase | open to secure/release clamp | resource handoff progress |

- Required beats:
  - Idle cradle scan.
  - Travel stows the manipulator and uses restrained hover heave.
  - Work settles, unfolds, contacts and repeats.
  - Damage releases carried material; destruction separates arm from shell.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ToolContact`, `Socket_Cargo`, `Socket_WorkVfx`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - A composed-design proposal must name the exact 5617/7646/7691 donor modules and rejected worker layouts before director approval.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
