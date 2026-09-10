# Red Planet Protector — T082 Super Scout packet

**Stable ID:** `unit.martians.red_planet_protector`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Positional anti-heavy control
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7313
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
| 7313 — Red Planet Protector | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7313)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160158.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7313-1) | PRIMARY_VERIFIED | Martian protector articulation and control mechanisms |

### Source audit [Martians:7313]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4160158.pdf)
- Construction map:
  - Evidence pages 2-14: Low blue/gray wedge craft with open operator area, hoses and two long removable emitter/tool arms.
  - Evidence pages 15-23: Separate broad twin-foot biped lower body; the complete upper craft docks onto it to form the tall Protector.
  - Evidence pages 24-28: Two independent low ground support/emitter devices are built and shown beside the complete Protector; they are not part of its body.
  - Evidence pages 29-33: Final source photography and explicit hand-separated reconfiguration from biped into a low craft with the leg and central body modules relocated.
- View/mechanism coverage: front=VERIFIED p28-33; rear=PARTIAL p28-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p28-33; undersideInterior=VERIFIED p2-28 staged modules; mechanism=VERIFIED p30-33 biped-to-craft reconfiguration; continuous motion and planted control action remain missing
- Verified findings:
  - The source Protector is a modular tall biped assembled from a low wedge craft, a broad two-foot lower body and two long detachable emitter arms.
  - Its upper craft keeps a broad triangular nose and visible hoses; the separate leg blocks and side arms remain readable even after final assembly.
  - The alternate low craft is made by hand-separating and relocating major modules, so the source proves both silhouettes but not a continuous in-game planted transformation.
- Remaining evidence gaps:
  - The canonical Martian palette, anti-heavy control action and credible continuous mobile-to-planted transition require an explicit adaptation contract; the source's blue/gray paint and hand-separated rebuild do not decide them.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A modular tall biped assembled from a broad wedge craft, twin-foot lower body and two long detachable emitter arms.

Non-removable identity anchors:

- broad wedge upper craft
- separate twin-foot biped base
- paired long detachable emitter arms

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.recon_mech_rp` — Both are modular Martian bipeds built around detachable upper craft. Mitigations: Recon-Mech carries a wide asymmetric drill-and-claw span; Protector carries two matched long emitter arms. / Recon-Mech exposes a tall rear pressure tank; Protector preserves a broad wedge nose and separate torso module. / Recon-Mech relocates its leg block behind the cockpit for flight; Protector distributes both leg and torso modules around its low craft state.
- `unit.martians.excavation_searcher` — Both are large articulated Martian control machines. Mitigations: Protector is a tall twin-foot biped with a detachable upper craft; Searcher is a huge low many-legged excavation chassis. / Protector carries paired long emitter arms; Searcher separates a forward drill/claw module from a tall rear crane. / Protector changes into a compact craft; Searcher exposes an underslung material sled and irregular processing route.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Broad wedge upper craft — remains the dominant mobile torso — SOURCE_VERIFIED.
  - Separate twin-foot biped base — visibly carries and braces the upper craft — SOURCE_VERIFIED.
  - Paired long detachable-source emitter arms — become continuous articulated Guard Sweep tools — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A reinforced visible waist connection carries the wedge craft into the twin-foot base while both long arms route sweep loads through shoulder braces into the planted stance.
- Repeated modules / connection grammar: Wedge craft, two-foot lower chassis, paired emitter arms, hoses and stance braces remain readable without hand separation.
- Source-faithful versus adapted boundary: The source proves two rebuilt silhouettes but not a continuous transform. The game transition must keep all parts connected and visibly trade movement for frontal bracing.

## E. Material and texture contract

- Geometry must carry:
  - broad wedge upper craft
  - separate twin-foot base
  - paired long emitter arms
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Large walker in mobile state; stationary frontal control anchor in Protector Stance.
- Planted/contact rule: Mobile gait alternates two feet; deployment widens and locks both feet before stance armor and Guard Sweep presentation become ready.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LegLeft` | Asset_RedPlanetProtector | walking step to widened planted stance | movement or authoritative stance progress |
| `Pivot_LegRight` | Asset_RedPlanetProtector | mirrored step-to-brace arc | movement or authoritative stance progress |
| `Pivot_EmitterLeft` | Asset_RedPlanetProtector | mobile carry to frontal sweep coverage | stance progress and Guard Sweep |
| `Pivot_EmitterRight` | Asset_RedPlanetProtector | mirrored coverage arc | stance progress and attack |

- Required beats:
  - Idle mobile stance keeps wedge and limbs distinct.
  - Walk carries the long arms clear of feet.
  - Deploy widens feet, lowers wedge and locks arms before readiness.
  - Guard Sweep uses physical arm motion; destruction breaks modules only after authoritative failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootLeft`, `Socket_FootRight`, `Socket_MuzzleLeft`, `Socket_MuzzleRight`, `Socket_GuardSweep`, `Socket_AudioDeploy`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - A continuous connected blockout must prove both canonical states without copying the source's hand-rebuilt conversion.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
