# Red Planet Cruiser — T082 Super Scout packet

**Stable ID:** `unit.martians.red_planet_cruiser`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Frontline combat
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7311
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
| 7311 — Red Planet Cruiser | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7311)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130806.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7311-1) | PRIMARY_VERIFIED | Martian cruiser proportions and palette |

### Source audit [Martians:7311]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130806.pdf)
- Construction map:
  - Evidence pages 2-11: Complete low green/gray Red Planet Cruiser with broad offset platforms, open central rider, forward wheel/equipment and long side probes.
  - Evidence pages 12-15: Separate two-stage docking pedestal is built and the complete cruiser mounts above it.
- View/mechanism coverage: front=PARTIAL p8-15; rear=PARTIAL p8-15; leftRight=VERIFIED p2-15 construction sequence; top=VERIFIED p2-15; threeQuarter=VERIFIED p1 and p8-15; undersideInterior=VERIFIED p2-13 staged cruiser and pedestal; mechanism=VERIFIED p12-15 detachable docking pedestal; movement and weapon cycles remain missing
- Verified findings:
  - Red Planet Cruiser is a low broad and deliberately irregular craft whose flat outer platforms outweigh its small central body.
  - The Martian operator, green central equipment and long side-mounted probe/weapon rods remain exposed instead of enclosed by a conventional cockpit.
  - A separately built tall pedestal supports the entire cruiser from below, proving a docking/deployment relationship that must be handled deliberately in the game adaptation.
- Remaining evidence gaps:
  - The source does not define whether the pedestal travels, deploys or stays at a facility; the frontline production unit needs an explicit supported state choice without inventing gameplay.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low broad irregular cruiser with flat offset platforms, an exposed central operator and a separate tall docking pedestal.

Non-removable identity anchors:

- broad offset platform plan
- open central operator and equipment
- long side probes plus underslung pedestal

- Rejected V1 blind-review code: `S57`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Broad offset platform plan — provides the medium frontline mass — SOURCE_VERIFIED.
  - Open central operator and equipment — keeps the machine distinctly Martian — SOURCE_VERIFIED.
  - Long side probes plus integrated underslung pedestal — preserve the source's irregular vertical support relationship — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A low central spine carries offset outer platforms and long probes while the source docking pedestal becomes an integrated underslung hover/support assembly rather than a separate gameplay entity.
- Repeated modules / connection grammar: Central operator deck, asymmetric platform pair, side probes/projector and underslung support pedestal remain legible.
- Source-faithful versus adapted boundary: The source proves docking but not a deployable pedestal. Gameplay has one simple hover unit, so the pedestal remains attached and does not create an unapproved stance.

## E. Material and texture contract

- Geometry must carry:
  - broad offset platforms
  - open operator center
  - long probes and underslung pedestal
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Medium ground-layer hover with dependable, restrained turns.
- Planted/contact rule: No normal ground contact; the underslung pedestal reads as hover/support equipment and never detaches in play.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ProjectorLeft` | Asset_RedPlanetCruiser | bounded forward target-tracking yaw | presentation aim |
| `Pivot_ProjectorRight` | Asset_RedPlanetCruiser | mirrored tracking yaw | presentation aim |

- Required beats:
  - Idle platform equipment cycles asymmetrically.
  - Travel keeps the broad low plan stable.
  - Attack aligns side projectors around the central target line.
  - Damage disables an outer platform; destruction drops the pedestal with the hull.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Muzzle`, `Socket_ProjectorLeft`, `Socket_ProjectorRight`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The integrated pedestal blockout must retain source identity without implying a second selectable object or deploy command.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
