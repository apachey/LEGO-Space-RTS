# Aero Guard Tower — T082 Super Scout packet

**Stable ID:** `building.mar.aero_guard_tower`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Infrastructure`
- Gameplay role: Anti-air defense
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 7314, 7317
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
| 7314 — Recon-Mech RP | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7314)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130809.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7314-1) | PRIMARY_VERIFIED | tall recon walker and sensor grammar |
| 7317 — Aero Tube Hangar | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7317)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160159.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7317-1) | PRIMARY_VERIFIED | Aero Tube, pump, hypersled, station and skiff mechanisms |

### Source audit [Martians:7314]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130809.pdf)
- Construction map:
  - Evidence pages 2-16: Broad red/gray upper craft with open central cockpit, long asymmetric drill/lance and claw arms, wrist hoses and rear equipment.
  - Evidence pages 17-25: Separate two-leg lower chassis with broad feet; upper craft docks above it to form the Recon-Mech.
  - Evidence pages 26-28: Tall rear pressure tank attaches behind the cockpit and between the upper modules.
  - Evidence pages 29-33: Final photography and explicit hand-separated flight conversion: lower body detaches, rotates and reconnects behind the upper craft.
- View/mechanism coverage: front=VERIFIED p25-33; rear=VERIFIED p25-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p25-33; undersideInterior=VERIFIED p2-28 staged modules; mechanism=VERIFIED p30-33 mech-to-flight reconfiguration and articulated arms; gait, detection and anti-air cycle remain missing
- Verified findings:
  - Recon-Mech is a tall biped carrying a broad aircraft-like upper body, not a narrow sensor tower with weaponry as a minor detail.
  - Its two arms are strongly asymmetric: one ends in a long drill/lance and the other in a large black claw, with visible hoses feeding both sides.
  - The tall rear pressure tank and detachable lower body remain recognizable when the legs are reattached behind the cockpit for the flight configuration.
- Remaining evidence gaps:
  - The source proves modular flight conversion but not a continuous transform, walking gait, scanner grammar or anti-air tracking path; those canonical functions need a source-respecting production contract.

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

**Silhouette thesis:** A tall Recon-Mech-derived sensor tower with mechanically tracking arms above a Tube-connected base.

Non-removable identity anchors:

- very tall open sensor mast
- paired tracking arms
- visible Tube-linked lower platform

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `building.mar.deflector_arm` — Both are small new Martian defensive structures with articulated upper mechanisms. Mitigations: Deflector Arm stays low with one long pushing arm; Aero Guard Tower is tall with paired tracking arms. / Deflector Arm ends in a broad paddle; Aero Guard Tower ends in sensor/anti-air heads. / Deflector Arm uses wide planted feet; Aero Guard Tower visibly joins the Tube network at its lower platform.
- `building.rock_raiders.cutter_mast` — Both are small tall anti-air structures. Mitigations: Cutter Mast is a thin industrial tripod; Aero Guard Tower is an open stacked Martian platform. / Cutter Mast has one scanner-cutter head; Aero Guard Tower has paired articulated tracking arms. / Cutter Mast carries a warm work light; Aero Guard Tower exposes blue/sand-red mechanics and a Tube connection.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Very tall open sensor mast — creates the faction's clearest anti-air landmark — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Paired mechanically tracking arms — expose guided air interception without a conventional turret — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Visible Tube-linked lower platform — ties defense to settlement infrastructure — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A compact planted Tube-side base braces an open mast; the paired tracking arms transfer motion and weapon load through an elevated crosshead.
- Repeated modules / connection grammar: Lower Tube platform, mast, detector head, two tracking arms and guided emitters remain readable.
- Source-faithful versus adapted boundary: New structure combines 7314 elevated tracking language with 7317 tower/platform grammar. It may not become a closed radar tower or symmetric gun turret.

## E. Material and texture contract

- Geometry must carry:
  - very tall open mast
  - paired tracking arms
  - Tube-linked lower platform
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary anti-air defense.
- Planted/contact rule: Lower platform and mast braces remain planted while only the elevated tracker follows air targets.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_AeroTrackerYaw` | Asset_AeroGuardTower | bounded high yaw around the mast | presentation aim |
| `Pivot_AeroTrackerPitch` | Pivot_AeroTrackerYaw | paired-arm elevation toward legal air targets | presentation aim |
| `Pivot_SensorDish` | Asset_AeroGuardTower | offset scan sweep distinct from weapon aim | detection and idle presentation |

- Required beats:
  - Construction braces mast before raising arms.
  - Idle sensor scans independently.
  - Attack aligns both arms and launches from the elevated crosshead.
  - Brownout lowers trackers; damage bends an arm before mast collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_MuzzleLeft`, `Socket_MuzzleRight`, `Socket_Detector`, `Socket_Network`, `Socket_AudioWeapon`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The 7314 tracker and 7317 tower donor allocation must be shown against Recon-Mech at all three camera widths.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
