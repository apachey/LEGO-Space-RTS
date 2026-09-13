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
  - Two short lateral arms end in one small yellow-green emitter each; the compact rear block supports the open pilot without becoming a canopy.
- Remaining evidence gaps:
  - The source establishes two lateral emitters but does not independently establish their gameplay firing relationship; authoritative weapon behavior remains unchanged by this visual audit.

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

Alien identity: armored biomechanical craft with skeletal supports, living conduits and bio-organic systems (Phase 02A). Technology does not prohibit biology. Exterior set construction and official promo/interior evidence must be combined; literal labels, anatomical inference and new adaptations must be distinguished. No swarm gameplay, generic hive substitution or automatic asset acceptance.

- [brickmaster_2007_mothership_cutaway](https://archive.org/details/brickmaster-issue17/page/n5/mode/1up) — OFFICIAL_PROMO_VISUAL_INFERENCE: Pale bone-like supports and branching channels inside the armored Mothership; skeletal/vascular interpretation is visual inference, not a literal blood-vessel label.
- [lego_club_2008_infiltrator_cutaway](https://archive.org/details/LEGOClubMagazineUS-JulyAugust2008-Miniland/page/n19/mode/1up) — OFFICIAL_PROMO_LITERAL_LABEL: Bio-Organic antigravity propulsion drive explicitly establishes biological technology.

- Accepted exterior source assemblies and silhouettes remain intact; do not cover craft in invented organs.
- Every biological or manufactured assembly needs a purpose, parent, load path and source/adaptation classification.
- Living conduits may flex during physical hull articulation; any internal contraction is a reviewed adaptation, never gameplay authority.
- Construction remains arrive, anchor, unfold, connect, energize; no gestation or spawning mechanic.
- Armored exterior, skeletal support, living conduit and crystal interfaces stay distinct; no generic wet skin or realistic gore.
- Explain gameplay role and donor/adaptation method before showing any new concept; generated images are not source evidence.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny armored black-lime biomechanical hover worker with one integrated utility clamp, skeletal support and living conduits; not contemporary human robotics or a self-mining crystal.

Non-removable identity anchors:

- low armored biomechanical hover shell
- external cargo cradle around an internal rib/conduit assembly
- single integrated folding utility clamp

- Rejected V1 blind-review code: `S49`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Black and bright lime armored biomechanical craft with skeletal supports, living conduits and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not substitute generic insects, hives, unrelated tentacles, contemporary human robots or black-neon towers for source-grounded biomechanical craft (Phase 02A).
- Nearest-confusion baseline:

- `unit.astronauts.expedition_crew` — Both are tiny mobile economy/build units. Mitigations: Expedition Crew is upright and bipedal; ETX Servitor is a low hover shell. / Expedition Crew carries tools externally; Servitor wraps around a crystal cradle. / Expedition Crew uses human helmet/backpack masses; Servitor uses a folding single manipulator and no humanoid body.
- `unit.martians.worker_robot` — Both are small nonhuman workers, but Alien biomechanical technology must not read as ordinary Martian robotics. Mitigations: Servitor hovers inside one low armored biomechanical shell; Worker Robot walks on two long articulated legs. / Servitor encloses skeletal supports and living conduits in black-lime structure; Worker Robot leaves the seated Martian visible between two broad feet. / Servitor uses one hull-integrated utility clamp; Worker Robot's adapted tool must remain subordinate to its bipedal source silhouette.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Low armored hover shell — keeps the worker spacecraft-derived and non-humanoid — CANON_DERIVED_ADAPTATION.
  - Internal skeletal support and living conduit assembly — inherits official cutaway family grammar; exact worker anatomy is new adaptation — CANON_DERIVED_ADAPTATION.
  - External cargo cradle and one integrated folding utility clamp — receives material, harvests, builds and repairs through visible contact; worker body is not the resource — CANON_DERIVED_ADAPTATION.
- Structural load path: Armored crescent shell encloses a supporting rib/core assembly; a hull-integrated clamp transfers work load into that support, with connected living conduits and external cargo path. Exact geometry remains review-required.
- Repeated modules / connection grammar: 5617 shell and 7646 articulated/conduit grammar plus official Mothership/Infiltrator promo interiors inform one biomechanical utility body; a contemporary human robotic deck/arm is not the target.
- Source-faithful versus adapted boundary: No official dedicated Alien worker exists. Clamp, cargo path and exact internal anatomy are disclosed adaptations, not SOURCE_VERIFIED organs. Revised composition requires proposal approval and image review; no creature legs or floating self-mining crystal.

## E. Material and texture contract

- Geometry must carry:
  - low crescent hover shell
  - central crystal cradle
  - single folding manipulator
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded armored-shell roughness; distinguish exterior armor from source-grounded living interior systems, without inventing wet skin or panel structure (Phase 02A). Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
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
  - Historical mechanical composition was rejected. Revised biomechanical donor/clamp/cargo anatomy and motion need a disclosed proposal and director review; Phase 02A does not approve a new image.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
