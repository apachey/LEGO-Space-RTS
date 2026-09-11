# Double Hover — T082 Super Scout packet

**Stable ID:** `unit.martians.double_hover`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Scout
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 7300
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
| 7300 — Double Hover | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7300)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156313.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7300-1) | PRIMARY_VERIFIED | paired-hover scout construction |

### Source audit [Martians:7300]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156313.pdf)
- Construction map:
  - Evidence pages 1: Complete seven-step Double Hover build with twin long forward runners, open rider deck and two unlike rear equipment masses.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 steps 5-7; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 1-7; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=MISSING static micro-build only
- Verified findings:
  - Double Hover is a narrow open sled on two long parallel forward runners, not a platform balanced above two circular hover discs.
  - The exposed rider sits between asymmetrical rear modules: one large round dish-like hover/engine mass and one compact block.
  - The blunt parallel fork silhouette is the clearest distinction from the longer, nozzle-led Jet Scooter.
- Remaining evidence gaps:
  - Clean rear and underside views are still required before fixing lift, propulsion and landing contacts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny open scout on two long parallel forward runners, balanced by unlike rear equipment modules around its rider.

Non-removable identity anchors:

- paired long forward runners
- central exposed rider
- one round rear hover mass beside one block

- Rejected V1 blind-review code: `S64`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.jet_scooter` — Both are tiny open Martian ground-hover craft. Mitigations: Double Hover has two long forward runners and unlike rear modules; Jet Scooter has one long spine with paired side tubes. / Double Hover ends in blunt parallel forks; Jet Scooter points a cluster of orange nozzles forward. / Double Hover reads short and laterally offset; Jet Scooter reads narrow and aggressively directional.
- `unit.martians.aero_skiff` — Both are small open Martian utility platforms. Mitigations: Double Hover keeps two long runners close to the ground; Aero Skiff must preserve an elevated airborne deck and visible lift mass. / Double Hover carries one operator between unlike rear modules; Aero Skiff preserves a separate passenger/cargo perch. / Double Hover points two straight forks forward; Aero Skiff retains the composite source family's intentionally asymmetric deck.
- `unit.rock_raiders.hover_scout` — The draft sheet exposes two tiny open survey sleds with similarly low parallel masses. Mitigations: Hover Scout must read as one continuous deck; Double Hover must preserve two long separated runners. / Hover Scout places one scanner bar at the nose; Double Hover places deliberately unlike equipment masses behind the rider. / Hover Scout uses a compact single-seat centre; Double Hover keeps a long fork gap visible from above.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Two long parallel forward runners — create the blunt forked scout silhouette — SOURCE_VERIFIED.
  - Central exposed rider deck — preserves the micro-machine scale — SOURCE_VERIFIED.
  - Unlike rear masses — one round hover/engine dish beside one compact block — SOURCE_VERIFIED.
- Structural load path: A narrow central deck joins the paired runners and supports the rider between two deliberately asymmetric rear equipment masses.
- Repeated modules / connection grammar: Runner pair, open rider deck, round rear hover mass and opposite equipment block remain readable.
- Source-faithful versus adapted boundary: Hover behavior and network-information signals are adapted; do not replace the runners with circular pads or mirror the rear modules.

## E. Material and texture contract

- Geometry must carry:
  - paired long forward runners
  - open central rider deck
  - unlike rear equipment masses
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Fast ground-layer hover with crisp direction changes and no true-air banking.
- Planted/contact rule: No physical wheel or foot contact; both runners share one low terrain-following hover datum.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_RunnerLeft` | Asset_DoubleHover | very small independent terrain-settle heave around source rest | presentation ground response |
| `Pivot_RunnerRight` | Asset_DoubleHover | mirrored bounded heave | presentation ground response |

- Required beats:
  - Idle rider scans while rear masses remain asymmetric.
  - Travel keeps the fork level and close to terrain.
  - Station-network information uses one bounded signal pulse.
  - Tube loading aligns runners to the route; destruction preserves the paired fork silhouette.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Sensor`, `Socket_TubeTransfer`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Rear propulsion and underside lift remain bounded until cleaner source views appear.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Replace the rejected 0/66 primitive silhouette with the source-derived method after Pilot V2 review, then run a new 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
