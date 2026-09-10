# Excavation Searcher — T082 Super Scout packet

**Stable ID:** `unit.martians.excavation_searcher`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Siege / manipulation walker
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7316
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
| 7316 — Excavation Searcher | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7316)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130811.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7316-1) | PRIMARY_VERIFIED | multi-leg excavation, crane and material handling |

### Source audit [Martians:7316]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130811.pdf)
- Construction map:
  - Evidence pages 2-16: Forward tan/orange Excavation Searcher module with long drill, paired claws and separately built planted tool/leg assemblies.
  - Evidence pages 17-30: Large irregular rear body, multiple spaced legs and tall articulated crane assemble, then dock to the forward module.
  - Evidence pages 31-33: Independent low material sled/container builds and docks beneath the complete Searcher.
  - Evidence pages 34-57: Separate dark excavation support rig with arches, hoses and human operator; useful opposition/industrial evidence, not direct Martian Searcher geometry.
  - Evidence pages 58-59: Cross-set alternate humanoid rebuild; not a demonstrated primary Searcher transformation.
- View/mechanism coverage: front=VERIFIED p1 and p27-33; rear=PARTIAL p27-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p27-33; undersideInterior=VERIFIED p2-33 staged modules and sled; mechanism=PARTIAL p28-33 crane/claws/module docking; no primary gait, siege cycle or full material route
- Verified findings:
  - The Martian Excavation Searcher is a huge low many-legged machine assembled from visibly separate forward tool, rear processing/crane and underslung sled modules.
  - A long drill, paired orange claws and tall crane create three different working directions around the irregular body instead of one humanoid front.
  - The low sled demonstrates material handling beneath the chassis, while the separate dark rig and final humanoid rebuild must not be mistaken for the primary Martian silhouette.
- Remaining evidence gaps:
  - The manual does not provide a walking gait, supported siege contact sequence, complete crane-to-processor route or game's manipulation attack; these require later semantic and motion contracts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A gigantic many-legged excavation machine carrying an unmistakable crane-claw and material-handling body.

Non-removable identity anchors:

- multi-legged planted base
- large articulated crane-claw
- high irregular processing superstructure

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.red_planet_protector` — Both are large articulated Martian control machines. Mitigations: Protector is a tall twin-foot biped with a detachable upper craft; Searcher is a huge low many-legged excavation chassis. / Protector carries paired long emitter arms; Searcher separates a forward drill/claw module from a tall rear crane. / Protector changes into a compact craft; Searcher exposes an underslung material sled and irregular processing route.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Wide multi-legged planted base — establishes Huge scale and mechanical stability — SOURCE_VERIFIED.
  - Large articulated crane-claw and forward excavation tools — show manipulation and siege as physical work — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - High irregular processing body with underslung sled path — exposes material handling instead of a turret — SOURCE_VERIFIED.
- Structural load path: Spaced legs support separate forward tool and rear processing/crane modules; brace loads travel through the central chassis while the underslung sled corridor stays clear.
- Repeated modules / connection grammar: Forward drill/claw package, many-leg base, rear processor, tall crane and underslung material sled form one asymmetric machine.
- Source-faithful versus adapted boundary: The separate human rig and cross-set humanoid rebuild are excluded. Gait, Brace and Excavation Clamp extend the verified primary Searcher rather than inventing artillery.

## E. Material and texture contract

- Geometry must carry:
  - multi-legged base
  - large crane-claw and excavation tools
  - high processor and underslung sled path
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Slow Huge multi-leg walker; stationary braced siege/control state.
- Planted/contact rule: A stable multi-foot support polygon is maintained in travel; Brace plants all primary contacts before the long clamp extends.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LegBankLeft` | Asset_ExcavationSearcher | sequenced planted-to-recovery arcs across left legs | distance traveled and gait phase |
| `Pivot_LegBankRight` | Asset_ExcavationSearcher | opposed right-leg sequence | distance traveled and gait phase |
| `Pivot_CraneBase` | Asset_ExcavationSearcher | stowed rear position to handling yaw arc | repair, tow or material presentation |
| `Pivot_ExcavationClamp` | Pivot_CraneBase | retracted to legal target contact extension | authoritative Clamp or braced siege progress |

- Required beats:
  - Idle crane checks the open sled path.
  - Walk sequences many contacts without humanoid rhythm.
  - Brace plants the chassis before siege readiness.
  - Clamp reaches, secures and retracts; destruction collapses processing and crane modules after failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Brace`, `Socket_ClampContact`, `Socket_DrillContact`, `Socket_Cargo`, `Socket_Repair`, `Socket_AudioMechanism`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The complete crane-to-processor route and many-leg gait require a gameplay-camera blockout before modeling.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
