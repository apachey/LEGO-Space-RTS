# ETX Alien Infiltrator — T082 Super Scout packet

**Stable ID:** `unit.aliens.etx_alien_infiltrator`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Craft / anti-heavy walker
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7646
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
| 7646 — ETX Alien Infiltrator | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7646)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7646-1) | PRIMARY_VERIFIED | alien craft-to-walker transformation |

### Source audit [Aliens:7646]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf)
- Construction map:
  - Evidence pages 3-27: Astronaut equipment and figures; supporting opposition evidence.
  - Evidence pages 29-50: Long central Infiltrator craft with split nose, exposed crew/tool bay, red weapon tips and lime conduits.
  - Evidence pages 51-67: Two independent curved side modules attach to the central craft and receive weapons, cables and flexible lime conduits.
  - Evidence pages 68: Explicit conversion: both curved side modules and the long forward hull rotate downward into a planted three-leg walker.
- View/mechanism coverage: front=VERIFIED p49-50 and p67-68; rear=VERIFIED p47-50 and p67-68; leftRight=VERIFIED p29-68; top=VERIFIED p29-67; threeQuarter=VERIFIED cover and p49-50/p67-68; undersideInterior=VERIFIED p29-67 staged modules; mechanism=VERIFIED p68 craft-to-three-leg walker conversion; gait and weapon cycle remain partial
- Verified findings:
  - Infiltrator flight state is a long central two-seat craft flanked by two separately built crescent modules.
  - The walker has three planted members, not a generic four- or six-legged spider: the two curved side modules and the long split nose rotate downward.
  - Flexible lime conduits visibly link the crew/energy core to the moving side modules, making the transformation mechanical rather than magical.
- Remaining evidence gaps:
  - The source proves the large rotations but not a continuous gait, stable planted attack pose, heavy-target weapon path or detector sweep; those remain explicit production motion contracts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long three-part alien craft that rotates its nose and curved side modules into a tall three-legged walker.

Non-removable identity anchors:

- long split central nose
- paired curved side modules
- three planted members in walker state

- Rejected V1 blind-review code: `S56`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.aliens.etx_alien_strike` — Both are medium ETX craft with large curved black-lime modules. Mitigations: Alien Strike remains a continuous flying crescent craft; Infiltrator rises on three planted members. / Alien Strike preserves two huge lateral crescent lobes and a long lime tail; Infiltrator preserves a long split nose and paired curved side modules. / Alien Strike keeps its cockpit and emitter on a horizontal airframe; Infiltrator's walker state places the crew core above an anti-heavy contact stance.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Long split central nose — forms the third planted member in walker state — SOURCE_VERIFIED.
  - Two curved side modules — rotate from flight flanks into the other two legs — SOURCE_VERIFIED.
  - Exposed crew/energy core with lime conduits — visibly feeds all moving modules — SOURCE_VERIFIED.
- Structural load path: A long central craft carries the crew/core; two side crescents attach through visible pivots and conduits to form one three-contact walker.
- Repeated modules / connection grammar: Central nose/body, mirrored crescent modules, conduit network and heavy weapon/detector assembly remain one unit.
- Source-faithful versus adapted boundary: Exactly three planted members are mandatory. Detector sweep, gait and heavy weapon timing are adapted without inventing spider legs.

## E. Material and texture contract

- Geometry must carry:
  - long split nose
  - paired curved side modules
  - visible lime conduit links
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Fast craft travel or articulated three-contact walker movement according to authoritative state.
- Planted/contact rule: No contacts in craft state; nose and both side modules establish three contacts before walker action.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_NoseLeg` | Asset_AlienInfiltrator | long nose rotates from flight axis to front planted member | authoritative transformation progress |
| `Pivot_SideLegLeft` | Asset_AlienInfiltrator | crescent flight flank to planted leg arc | transformation and gait |
| `Pivot_SideLegRight` | Asset_AlienInfiltrator | mirrored flank-to-leg arc | transformation and gait |

- Required beats:
  - Idle matches current state.
  - Craft travel stays long and low; walker gait cycles three contacts.
  - Transformation establishes all contacts before readiness.
  - Heavy attack concentrates at the core; destruction separates one crescent after state ends.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootNose`, `Socket_FootLeft`, `Socket_FootRight`, `Socket_Muzzle`, `Socket_Detector`, `Socket_AudioTransform`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Three-contact gait and weapon/detector separation require production animation review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
