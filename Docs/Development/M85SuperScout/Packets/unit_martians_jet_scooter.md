# Jet Scooter — T082 Super Scout packet

**Stable ID:** `unit.martians.jet_scooter`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Ground-hover harassment
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 7303
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
| 7303 — Jet Scooter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7303)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130291.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7303-1) | PRIMARY_VERIFIED | Martian light attack scooter |

### Source audit [Martians:7303]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130291.pdf)
- Construction map:
  - Evidence pages 1: Complete seven-step Jet Scooter build with long blue central deck, parallel side tubes, clustered orange nose nozzles and open rider.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 steps 3-7; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 1-7; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=MISSING static micro-build only
- Verified findings:
  - Jet Scooter is a long narrow open sled rather than a short body between two oversized engine pods.
  - Parallel exposed side tubes reinforce the central spine, while a cluster of small orange nozzles makes the nose strongly directional.
  - The rider and rounded blue rear equipment remain above the otherwise thin deck.
- Remaining evidence gaps:
  - The source does not establish hover height, propulsion cycle or a canonical attack emitter; those remain production motion and socket decisions.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long narrow open sled whose paired side tubes and clustered orange nose nozzles frame the rider spine.

Non-removable identity anchors:

- long thin central deck
- paired exposed side tubes
- clustered orange forward nozzles

- Rejected V1 blind-review code: `S55`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.double_hover` — Both are tiny open Martian ground-hover craft. Mitigations: Double Hover has two long forward runners and unlike rear modules; Jet Scooter has one long spine with paired side tubes. / Double Hover ends in blunt parallel forks; Jet Scooter points a cluster of orange nozzles forward. / Double Hover reads short and laterally offset; Jet Scooter reads narrow and aggressively directional.
- `unit.aliens.razor_skimmer` — Both are small fast hover harassment units. Mitigations: Razor Skimmer is a broad black blade plan; Jet Scooter is a long thin open sled around its rider. / Razor centers on a lime core between two large prongs; Jet Scooter points a cluster of small orange nozzles ahead of paired side tubes. / Razor's propulsion is visually integrated into the hull; Jet Scooter leaves its tubes, deck and rear equipment exposed.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Long thin central deck — makes the scooter a directional sled rather than a short pod — SOURCE_VERIFIED.
  - Paired exposed side tubes — frame the rider spine and propulsion path — SOURCE_VERIFIED.
  - Clustered orange forward nozzles — provide the unmistakable attack direction — SOURCE_VERIFIED.
- Structural load path: The central deck carries the exposed rider while two parallel side tubes connect the front nozzle cluster to rounded rear equipment.
- Repeated modules / connection grammar: Thin deck, rider, side-tube pair, nose nozzles and rear pressure mass remain distinct.
- Source-faithful versus adapted boundary: Attack pulse, hover response and Tube-transfer pose are adaptations; no enclosed canopy, broad wings or oversized side engines.

## E. Material and texture contract

- Geometry must carry:
  - long thin deck
  - paired exposed side tubes
  - clustered forward nozzles
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Very fast ground-layer hover with restrained sled yaw and no flight roll.
- Planted/contact rule: No physical ground contact; one low hover plane follows legal ground topology.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_NozzleCluster` | Asset_JetScooter | short aligned recoil along the central deck | authoritative Scooter Pulse attack |
| `Pivot_RearPressureVane` | Asset_JetScooter | small bounded steering yaw | turn intensity presentation |

- Required beats:
  - Idle pressure flicker stays below weapon intensity.
  - Travel stretches the long deck cleanly along motion.
  - Attack recoils the nose cluster and emits forward.
  - Tube loading aligns the deck; destruction separates one side tube after failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Muzzle`, `Socket_TubeTransfer`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The source does not prove the steering vane or recoil travel; both require restrained blockout review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
