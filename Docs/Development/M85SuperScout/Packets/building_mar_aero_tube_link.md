# Aero Tube Link — T082 Super Scout packet

**Stable ID:** `building.mar.aero_tube_link`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Infrastructure`
- Gameplay role: Network connection
- Authoritative footprint: `Tiny`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7317, 3750
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
| 7317 — Aero Tube Hangar | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7317)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160159.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7317-1) | PRIMARY_VERIFIED | Aero Tube, pump, hypersled, station and skiff mechanisms |
| 3750 — Life on Mars Accessories | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/3750)<br>no direct official PDF located | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=3750-1) | CANON_VERIFIED_ARCHIVAL | hypersled and Aero Tube connection equipment |

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

### Source audit [Martians:3750]

- Evidence state: `ARCHIVAL_GAP`
- Evidence links: no viewable evidence link recorded
- Construction map:
  - No construction-page range is available for this evidence type.
- View/mechanism coverage: front=PARTIAL archival product imagery; rear=MISSING; leftRight=PARTIAL archival product imagery; top=PARTIAL archival product imagery; threeQuarter=PARTIAL archival product imagery; undersideInterior=MISSING; mechanism=MISSING
- Verified findings:
  - Canon and archival inventory establish Life on Mars accessories as a hypersled and Aero Tube connection source, but not a modelable coupler interior or verified sled route.
- Remaining evidence gaps:
  - Locate official or clearly labeled archival construction evidence before the 3750 coupler and hypersled details become production geometry.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low transparent Tube span with readable couplers and a moving hypersled path, not a solid wall.

Non-removable identity anchors:

- continuous transparent tube
- distinct end couplers
- visible internal sled corridor

- Rejected V1 blind-review code: `S66`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Continuous transparent Tube span — shows a traversable route rather than a wall or cable — SOURCE_VERIFIED.
  - Distinct mechanical end couplers — bind the route to two eligible Stations — SOURCE_VERIFIED.
  - Visible internal hypersled corridor and direction signals — communicate travel without simulating strategic cargo packets — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: Project-generated route supports carry a continuous Tube between two Station couplers; bends remain readable and never create arbitrary player-drawn geometry.
- Repeated modules / connection grammar: Start coupler, generated Tube spans/bends, route supports, end coupler and visible sled corridor form one destructible link.
- Source-faithful versus adapted boundary: 7317 proves the complete Tube/sled system. 3750 has no modelable interior evidence and therefore fixes no coupler, sled or route geometry until better evidence appears.

## E. Material and texture contract

- Geometry must carry:
  - continuous transparent route
  - mechanical end couplers
  - visible internal sled corridor
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary node-to-node network route with presentation-only sled travel.
- Planted/contact rule: End couplers lock to Stations and intermediate supports plant along the authored/generated legal route.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CouplerStart` | Asset_AeroTubeLink | unsealed construction pose to locked pressure seal | construction and active-link state |
| `Pivot_CouplerEnd` | Asset_AeroTubeLink | mirrored seal motion | construction and active-link state |
| `Pivot_SledGuide` | Asset_AeroTubeLink | continuous interior path from origin to destination | resource-flow or eligible-unit transfer presentation |

- Required beats:
  - Construction places supports and joins continuous Tube spans.
  - Activation seals both couplers before route flow begins.
  - Transfer shows a bounded sled moving from selected origin to destination.
  - Damage vents the hit span; destruction stops presentation immediately and leaves a readable severed link.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_LinkStart`, `Socket_LinkEnd`, `Socket_TubePath`, `Socket_DamageVfx`, `Socket_AudioPressure`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Locate modelable 3750 evidence before assigning it any exact coupler or hypersled detail; 7317 remains the production geometry authority meanwhile.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
