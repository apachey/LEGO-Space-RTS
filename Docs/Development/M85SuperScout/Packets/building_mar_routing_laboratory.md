# Routing Laboratory — T082 Super Scout packet

**Stable ID:** `building.mar.routing_laboratory`

**Packet state:** `T082 REFERENCE FOUNDATION ACCEPTED — T083/T085 DESIGN PENDING`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Infrastructure`
- Gameplay role: Technology / network support
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7317
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
| 7317 — Aero Tube Hangar | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7317)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160159.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7317-1) | PRIMARY_VERIFIED | Aero Tube, pump, hypersled, station and skiff mechanisms |

### Source audit [Martians:7317]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160159.pdf)
- Construction map:
  - Evidence pages 3-8: Compact two-leg utility walker with open rider, long probe and claw; supporting station equipment evidence.
  - Evidence pages 9-40: Large open Aero Tube Hangar with elevated frame, central ribbed Tube/ramp, ring coupler, handling crane, work platforms and rock load.
  - Evidence pages 41-48: Two related raised four-leg control/service stations with antenna or dish equipment.
  - Evidence pages 49-62: Separate red and blue Tube docking arches with visible supports, end stops and open approach paths.
  - Evidence pages 63-67: Three hypersleds, route signs, coupler/branch pieces and the stacked three-chamber pressure unit.
  - Evidence pages 68-70: Complete physical network assembly and play routing: long transparent/flexible Tubes connect hangar, endpoints, junction and pressure unit.
  - Evidence pages 71: Cross-set lineup; no additional 7317 construction evidence.
- View/mechanism coverage: front=VERIFIED p35-40 and p68-70; rear=PARTIAL p28-40 and p68-70; leftRight=VERIFIED p9-70; top=VERIFIED p9-70; threeQuarter=VERIFIED p1, p35-40 and p68-70; undersideInterior=VERIFIED p9-67 staged open structures and Tube modules; mechanism=VERIFIED p68-70 physical Tube/sled network; PARTIAL crane, routing and pressure action
- Verified findings:
  - The source is a decentralized transport system rather than one sealed headquarters: open hangar, endpoint stations, colored docking arches, sleds, couplers, pressure unit and long Tubes remain separate readable modules.
  - The main Hangar is an elevated irregular frame organized around a real central Tube/ramp and circular coupler, with platforms and a crane left visibly open.
  - The pressure source is a distinct stacked three-chamber black unit, and the final pages make the Tubes continuous physical travel paths between stations rather than decorative cables.
- Remaining evidence gaps:
  - The game's Hangar, Settlement Station, Pressure Generator, Routing Laboratory and Link must divide the shared source modules explicitly; the manual does not define canonical building boundaries, throughput rules or automatic route selection.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tall eccentric control stack wrapped around a multi-way physical Tube switching mechanism.

Non-removable identity anchors:

- multi-way Tube switch
- high open control platform
- rotating route indicator

- Rejected V1 blind-review code: `S38`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Multi-way physical Tube switch — makes routing technology a machine rather than abstract UI — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - High eccentric open control platform — creates a distinct technology skyline — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Rotating route drum and indicator arms — expose network calculation and active path direction — CANON_DERIVED_ADAPTATION.
- Structural load path: A broad planted switch base carries several Tube throats and a tall offset control stack; braces route the upper platform into the base without sealing the mechanism.
- Repeated modules / connection grammar: Multi-way switch, open laboratory platform, route drum, indicator arms and service couplers remain readable.
- Source-faithful versus adapted boundary: The composition uses verified 7317 junction/control pieces plus complex mechanism grammar; it cannot become a radar dish, generic science tower or second Station endpoint.

## E. Material and texture contract

- Geometry must carry:
  - multi-way Tube switch
  - high open control platform
  - rotating route drum and indicators
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary technology and network-support structure.
- Planted/contact rule: Switch base and tower supports remain planted; route throats preserve visible clear paths.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_RouteDrum` | Asset_RoutingLaboratory | indexed rotation among visible route states | research or network activity |
| `Pivot_IndicatorArm` | Asset_RoutingLaboratory | bounded pointer arc toward selected path | active route presentation |
| `Pivot_SwitchGate` | Asset_RoutingLaboratory | sealed inactive throat to aligned open path | network activity |

- Required beats:
  - Construction seats switch before raising the control platform.
  - Idle drum pauses on a readable route.
  - Research indexes mechanisms in a deliberate sequence.
  - Network reroute aligns gate and indicator; damage jams outer paths before tower collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Research`, `Socket_RouteSwitch`, `Socket_Network`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact 7317 junction/control donor split and added mechanism arrangement require director review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Record named pivots, contacts and sockets; real gameplay-camera validation belongs to T084/T086 after production models exist.
4. Author only the specified reusable textures after human material review.
5. Use the accepted T082 reference foundation to prepare the T083/T085 visual design package. Do not repeat the completed three-scale review without a specific identity defect.

**State:** `T082_REFERENCE_FOUNDATION_ACCEPTED_DESIGN_PENDING`

**Approving reviewer:** game director accepted the complete T082 reference corpus on 2026-09-23; asset-specific design and production remain unapproved.
