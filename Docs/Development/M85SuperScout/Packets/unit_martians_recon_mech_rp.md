# Recon-Mech RP — T082 Super Scout packet

**Stable ID:** `unit.martians.recon_mech_rp`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Detection / anti-air walker
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7314
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

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tall biped carrying a broad aircraft-like cockpit body, asymmetric drill-and-claw arms and a rear pressure tank.

Non-removable identity anchors:

- broad detachable upper craft
- asymmetric long drill and claw arms
- paired legs and tall rear pressure tank

- Blind-review code: `S36`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.red_planet_protector` — Both are modular Martian bipeds built around detachable upper craft. Mitigations: Recon-Mech carries a wide asymmetric drill-and-claw span; Protector carries two matched long emitter arms. / Recon-Mech exposes a tall rear pressure tank; Protector preserves a broad wedge nose and separate torso module. / Recon-Mech relocates its leg block behind the cockpit for flight; Protector distributes both leg and torso modules around its low craft state.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Broad aircraft-like open upper craft — carries operator and elevated detection mass — SOURCE_VERIFIED.
  - Asymmetric long drill/lance and claw arms — prevent a generic symmetric walker — SOURCE_VERIFIED.
  - Two broad feet and tall rear pressure tank — establish the planted profile and pneumatic supply — SOURCE_VERIFIED.
- Structural load path: The broad upper craft and rear tank transfer through a visible waist dock into exactly two leg chains and broad feet; each unequal arm remains hose-fed from the body.
- Repeated modules / connection grammar: Upper craft, drill/lance arm, claw arm, rear pressure tank and two-leg lower chassis retain their source seams.
- Source-faithful versus adapted boundary: The source flight rebuild is hand-separated and gameplay defines a walker, not a transforming unit. Production preserves modular seams but does not imply a flight state.

## E. Material and texture contract

- Geometry must carry:
  - broad upper craft
  - asymmetric drill and claw arms
  - paired legs and rear pressure tank
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `mar_open_frame_surface` — Restrained molded variation for blue, sand-red and neutral open frames without faking connections or turning Martians into polished Alien machines. Channels: Tangent-space normal and linear roughness; body colors remain parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected structural frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Life on Mars machinery; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_pneumatic_tube_surface` — Controlled transparent Tube, hose and pressure-window response with readable interiors and no baked cargo or glow. Channels: Linear roughness, transmission control and restrained color mask; geometry defines walls, couplers and contents. Resolution: 1024x1024; texel density: 512 px/m on localized Tube and hose UVs; tiling: Continuous trim along each physical route; phase remains continuous through generated spans.; LOD fallback: Simplified transparent route with opaque end couplers at Combat; one translucent route band at Strategic. Provenance/state: Project-authored procedural/trim source informed by verified 7317 Tubes; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_mechanism_service_surface` — Subtle functional wear and lubrication variation for pivots, claws, runners, pumps and open handling machinery. Channels: Tangent-space normal, linear roughness and restrained tool-contact mask; no baked structural shadows. Resolution: 1024x1024; texel density: 384 px/m at Close on mechanism islands; tiling: Shared trim regions aligned to mechanical travel; no per-part noise reset.; LOD fallback: Retain broad Tool/Neutral material split at Combat; merge to Tool master at Strategic. Provenance/state: Project-authored procedural/trim source informed by audited 7313/7314/7316/7317 mechanisms; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `mar_route_signal_atlas` — Route direction, Station identity, loading, pressure, detection and displacement-state indicators. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs; directional route arrows may repeat at fixed spacing.; LOD fallback: One route direction and one operating Signal per asset at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Tall two-leg walker with stable elevated sensor carriage.
- Planted/contact rule: Broad feet alternate contact; anti-air tracking keeps at least one stable support phase.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LegLeft` | Asset_ReconMechRP | planted-to-step hip/knee arc | distance traveled and gait phase |
| `Pivot_LegRight` | Asset_ReconMechRP | opposed gait arc | distance traveled and gait phase |
| `Pivot_AeroTracker` | Asset_ReconMechRP | elevated yaw/pitch tracking above the upper craft | presentation aim and detection |
| `Pivot_ClawArm` | Asset_ReconMechRP | bounded counterbalance and ground-defense arc | ground attack presentation |

- Required beats:
  - Idle sensor sweep preserves arm asymmetry.
  - Walk stabilizes the rear pressure tank over alternating feet.
  - Anti-air attack tracks high and fires from the elevated mount.
  - Damage vents the tank; destruction separates upper craft from planted base after failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootLeft`, `Socket_FootRight`, `Socket_MuzzleAir`, `Socket_MuzzleGround`, `Socket_Detector`, `Socket_AudioStep`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The elevated tracker must be distinguished from the source drill/lance without erasing the unequal arm silhouette.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
