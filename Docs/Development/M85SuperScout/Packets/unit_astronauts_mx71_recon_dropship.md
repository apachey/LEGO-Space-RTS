# MX-71 Recon Dropship — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mx71_recon_dropship`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Air transport
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7692
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
| 7692 — MX-71 Recon Dropship | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7692)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7692-1) | PRIMARY_VERIFIED | dropship cargo cradle and small alien attack craft |

### Source audit [Astronauts:7692]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf)
- Construction map:
  - Evidence pages 3-12: Separate compact six-wheel reconnaissance payload rover.
  - Evidence pages 13-23: Alien attack craft; supporting opposition evidence.
  - Evidence pages 24-40: Dropship cockpit, central keel and open cargo interface.
  - Evidence pages 41-61: Long tail/wing frame and suspended-load structure.
  - Evidence pages 62-69: Landing pads, side equipment cylinders and final payload-clearance geometry.
  - Evidence pages 70-71: Explicit lift/release of the independent rover beneath the fuselage.
- View/mechanism coverage: front=VERIFIED p62-71; rear=VERIFIED p65-71; leftRight=VERIFIED p24-71; top=VERIFIED p24-69; threeQuarter=VERIFIED p1 and p69-71; undersideInterior=VERIFIED p24-70 open cargo cradle; mechanism=VERIFIED p70-71 payload lift/release and landing-pad motion
- Verified findings:
  - MX-71 is a long narrow lifting aircraft organized around an open underside cargo cradle.
  - The six-wheel rover remains visibly independent below the fuselage and defines the transport function at a glance.
  - Tall landing pads and long side/tail booms preserve payload clearance rather than reading as decorative aircraft fins.
- Remaining evidence gaps:
  - The production transport must support several canonical payload types while retaining the source's visible external-carry identity and clear loading contacts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A high-wing white-orange dropship with a clearly readable underslung rover cradle.

Non-removable identity anchors:

- broad high-mounted wings
- open underslung cargo cradle
- forward blue canopy and twin engine masses

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.rock_raiders.tunnel_transport` — Both are large true-air transports. Mitigations: Tunnel Transport is carried by two giant propeller pods; MX-71 uses broad fixed wings and compact engines. / Tunnel Transport has an industrial crossbeam and open heavy cradle; MX-71 has a streamlined cockpit and rover-sized underslung bay. / Tunnel Transport reads teal, skeletal and load-first; MX-71 reads white-orange, aerodynamic and expedition-first.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Long high-wing fuselage — provides payload clearance and flight direction — SOURCE_VERIFIED.
  - Open underslung cargo cradle — exposes the carried vehicle as the primary transport read — SOURCE_VERIFIED.
  - Tall landing pads and side equipment cylinders — keep the load clear during docking — SOURCE_VERIFIED.
- Structural load path: The long keel and high wing transfer payload weight around, not through, the open cradle; landing pads terminate below the cargo datum.
- Repeated modules / connection grammar: Fuselage/wing, landing-pad pair, lift cradle and independent payload use explicit docking contacts.
- Source-faithful versus adapted boundary: Several canonical payloads may replace the source rover, but none may be hidden inside an opaque fuselage.

## E. Material and texture contract

- Geometry must carry:
  - high wing and long fuselage
  - open cargo cradle
  - visible carried payload gap
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: True-air transport with heavy restrained banking.
- Planted/contact rule: Landing pads settle before load or unload; payload has its own ground contacts after release.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CargoCradle` | Asset_MX71 | raise/lock and lower/release along underside path | authoritative load/unload progress |
| `Pivot_LandingPads` | Asset_MX71 | flight rest to planted stance | landing presentation |

- Required beats:
  - Idle airborne loiter or landed service.
  - Travel shows payload-aware heavy banking.
  - Load settles, locks cradle and raises payload.
  - Unload reverses; damage never releases cargo before authoritative result.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Cargo`, `Socket_Load`, `Socket_Unload`, `Socket_EngineLeft`, `Socket_EngineRight`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Cradle clearance must be validated against every canonical eligible payload before modeling lock.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
