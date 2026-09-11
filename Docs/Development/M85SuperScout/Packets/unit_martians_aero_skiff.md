# Aero Skiff — T082 Super Scout packet

**Stable ID:** `unit.martians.aero_skiff`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Light air transport
- Authoritative footprint: `Small`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 1195, 7317
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
| 1195 — Alien Encounter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/1195)<br>no direct official PDF located | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=1195-1) | CANON_VERIFIED_ARCHIVAL | small Martian Aero Skiff source and human field variant |
| 7317 — Aero Tube Hangar | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7317)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160159.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7317-1) | PRIMARY_VERIFIED | Aero Tube, pump, hypersled, station and skiff mechanisms |

### Source audit [Martians:1195]

- Evidence state: `ARCHIVAL_GAP`
- Evidence links: no viewable evidence link recorded
- Construction map:
  - No construction-page range is available for this evidence type.
- View/mechanism coverage: front=PARTIAL archival product imagery; rear=MISSING; leftRight=PARTIAL archival product imagery; top=MISSING; threeQuarter=PARTIAL archival product imagery; undersideInterior=MISSING; mechanism=MISSING
- Verified findings:
  - Canon and archival inventory establish Alien Encounter as one Aero Skiff source family, but they do not establish a verified construction load path or lift mechanism.
- Remaining evidence gaps:
  - Locate an official instruction scan or clearly labeled archival construction views before fixing the 1195 contribution to the composite Aero Skiff.

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

**Silhouette thesis:** A light open flying platform carrying one passenger above a visible pneumatic or hover mechanism.

Non-removable identity anchors:

- open asymmetric skiff deck
- underslung lift mechanism
- single passenger or cargo perch

- Rejected V1 blind-review code: `S29`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.double_hover` — Both are small open Martian utility platforms. Mitigations: Double Hover keeps two long runners close to the ground; Aero Skiff must preserve an elevated airborne deck and visible lift mass. / Double Hover carries one operator between unlike rear modules; Aero Skiff preserves a separate passenger/cargo perch. / Double Hover points two straight forks forward; Aero Skiff retains the composite source family's intentionally asymmetric deck.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Open asymmetric skiff deck — prevents a generic enclosed aircraft — CANON_DERIVED_ADAPTATION.
  - Visible underslung lift and pressure mechanism — explains true-air movement mechanically — CANON_DERIVED_ADAPTATION.
  - Single passenger or cargo perch — exposes the two-point transport capacity — CANON_DERIVED_ADAPTATION.
- Structural load path: A narrow open deck carries the rider and one visible perch above a compact articulated lift frame tied to verified 7317 docking and pneumatic grammar.
- Repeated modules / connection grammar: Asymmetric deck, control perch, passenger/cargo restraint, underslung lift mechanism and docking interface remain separate.
- Source-faithful versus adapted boundary: 1195 lacks modelable construction evidence, so it fixes no connector, load path or lift geometry. Production uses verified 7317 platform/docking language and must disclose any later 1195 contribution.

## E. Material and texture contract

- Geometry must carry:
  - open asymmetric deck
  - underslung lift mechanism
  - single passenger/cargo perch
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Small true-air transport with quick but readable banking.
- Planted/contact rule: No battlefield ground contact; loading uses a stable airborne or Station-docked datum.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LiftMechanism` | Asset_AeroSkiff | compact travel angle to stabilized loading spread | load/unload presentation |
| `Pivot_PassengerRestraint` | Asset_AeroSkiff | open access to secured transport position | passenger occupancy |

- Required beats:
  - Idle loiter exposes the open deck.
  - Travel banks without hiding the passenger perch.
  - Load stabilizes lift and closes the restraint.
  - Damage destabilizes the underslung mechanism; destruction releases passengers only by authoritative transport rules.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Load`, `Socket_Unload`, `Socket_Cargo`, `Socket_Engine`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Locate modelable 1195 evidence before assigning it any exact production geometry; until then the verified 7317-derived construction carries the contract.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
