# Worker Robot — T082 Super Scout packet

**Stable ID:** `unit.martians.worker_robot`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Worker / builder
- Authoritative footprint: `Tiny`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7302
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
| 7302 — Worker Robot | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7302)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130290.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7302-1) | PRIMARY_VERIFIED | Martian worker walker construction |

### Source audit [Martians:7302]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130290.pdf)
- Construction map:
  - Evidence pages 1: The Martian figure and shallow central operator wedge are built separately; the wedge includes an exposed seat/control tile and rear wall.
  - Evidence pages 2: Two mirrored multi-joint leg assemblies, each ending in one broad rectangular foot, attach to the left and right sides of the central wedge; the final seated biped and cover walking pose are shown.
- View/mechanism coverage: front=VERIFIED p1 cover and p2 final; rear=PARTIAL p1-2 sequence; leftRight=VERIFIED p1 cover and p2 steps 5-7; top=VERIFIED p1-2; threeQuarter=VERIFIED p1 cover and p2 final; undersideInterior=PARTIAL p1 bare base and p2 separate feet; mechanism=PARTIAL p2 mirrored leg joints plus cover walking pose; no complete gait or tool action sequence
- Verified findings:
  - The source machine is a tiny open bipedal walker with two mirrored articulated legs; the earlier four-spoke interpretation was incorrect.
  - Each leg terminates in one broad foot, producing exactly two ground contacts around the shallow central operator wedge.
  - The source has no dedicated arm or work tool, so the canonical builder/repair manipulator must be a clearly labeled subordinate adaptation that does not erase the two-leg silhouette.
- Remaining evidence gaps:
  - The manual proves leg topology and a walking pose but not a full planted gait cycle.
  - The production contract must choose a compact worker tool mount without converting the small biped into a generic humanoid robot or a multi-legged platform.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny open Martian biped whose shallow operator wedge rides between two long articulated legs and broad feet.

Non-removable identity anchors:

- shallow central operator wedge
- two mirrored multi-joint legs
- two broad rectangular feet

- Rejected V1 blind-review code: `S41`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.aliens.etx_servitor` — Both are small nonhuman workers, but Alien biomechanical technology must not read as ordinary Martian robotics. Mitigations: Servitor hovers inside one low armored biomechanical shell; Worker Robot walks on two long articulated legs. / Servitor encloses skeletal supports and living conduits in black-lime structure; Worker Robot leaves the seated Martian visible between two broad feet. / Servitor uses one hull-integrated utility clamp; Worker Robot's adapted tool must remain subordinate to its bipedal source silhouette.
- `unit.rock_raiders.crew` — At far scale both reduce to a tiny upright worker with two planted contacts. Mitigations: Raider Crew keeps a full minifigure torso above short legs; Worker Robot suspends a shallow rider wedge between two long mechanical legs. / Raider Crew carries one large tool outside the body; Worker Robot's small adapted tool remains subordinate to the biped frame. / Raider Crew's negative space is between arms and tool; Worker Robot must retain a tall open gap between its legs.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Shallow open operator wedge — keeps the machine tiny and visibly piloted — SOURCE_VERIFIED.
  - Two mirrored multi-joint legs — establish the corrected biped topology — SOURCE_VERIFIED.
  - Two broad rectangular feet plus compact side work tool — plant construction and repair without creating extra legs — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: The shallow rider wedge transfers weight into exactly two articulated side legs and two broad feet; one subordinate tool mount loads into the central frame.
- Repeated modules / connection grammar: Operator wedge, mirrored leg chains, broad foot pair and compact manipulator/tool remain independently readable.
- Source-faithful versus adapted boundary: 7302 proves the biped but not a work arm. The tool must remain subordinate and may not convert the unit into a humanoid robot, four-spoke platform or extra-legged walker.

## E. Material and texture contract

- Geometry must carry:
  - shallow operator wedge
  - two articulated legs
  - two broad feet and compact tool
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Tiny two-leg walker with quick short steps.
- Planted/contact rule: Exactly two broad feet alternate support; both settle before harvesting, construction or repair contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LegLeft` | Asset_WorkerRobot | mirrored hip/knee stepping arc from planted to recovery pose | distance traveled and gait phase |
| `Pivot_LegRight` | Asset_WorkerRobot | opposed stepping arc | distance traveled and gait phase |
| `Pivot_WorkTool` | Asset_WorkerRobot | stowed side position to short target-contact arc | harvest, construction, repair or defensive-tool progress |

- Required beats:
  - Idle rider and tool check.
  - Walk alternates exactly two broad planted feet.
  - Work settles both feet before tool contact.
  - Tube loading compacts the tool; destruction separates one leg after authoritative failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootLeft`, `Socket_FootRight`, `Socket_ToolContact`, `Socket_TubeTransfer`, `Socket_AudioStep`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The compact work-tool donor and mount require a silhouette comparison that preserves the verified two-leg profile.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
