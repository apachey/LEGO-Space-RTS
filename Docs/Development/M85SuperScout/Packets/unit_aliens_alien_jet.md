# Alien Jet — T082 Super Scout packet

**Stable ID:** `unit.aliens.alien_jet`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Air scout / interceptor
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-DIRECT`
- Approved source sets/motifs: 5617
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
| 5617 — Alien Jet | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/5617)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=5617-1) | PRIMARY_VERIFIED | small alien jet and compact ETX grammar |

### Source audit [Aliens:5617]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525566.pdf)
- Construction map:
  - Evidence pages 1: Complete five-step Alien Jet build with exposed pilot, swept black deck and paired flexible lime arches.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 cover and final step; rear=PARTIAL p1 staged build; leftRight=PARTIAL p1 construction sequence; top=VERIFIED p1 steps 3-5; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 bare plate sequence; mechanism=PARTIAL p1 flexible lime arches; no authored flight or weapon motion
- Verified findings:
  - Alien Jet is an extremely small open craft built around a broad swept black plate rather than an enclosed fuselage.
  - Two tall flexible lime arches rise over the exposed pilot and dominate the profile from the front and side.
  - A single forward yellow emitter and short rear equipment block keep the craft directional despite its minimal body.
- Remaining evidence gaps:
  - Clean rear and underside views are still required before fixing propulsion, landing and weapon sockets for the production Alien Jet.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny open black-lime interceptor with a swept plate body and two bright arches above its pilot.

Non-removable identity anchors:

- broad swept plate body
- exposed central alien pilot
- paired tall lime conduit arches

- Blind-review code: `S28`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.aliens.razor_skimmer` — Both are small swept black-lime attack craft. Mitigations: Alien Jet has a visible airborne swept-plate profile; Razor Skimmer stays almost flat against the ground. / Alien Jet raises two bright conduit arches over an open pilot; Razor Skimmer projects two long forward razor prongs. / Alien Jet centers on its pilot; Razor Skimmer centers on an exposed lime energy core.
- `unit.astronauts.mission_fighter` — The draft sheet reduces both small interceptors to a swept central fuselage and paired wings. Mitigations: Mission Fighter uses a closed compact canopy; Alien Jet leaves the pilot open beneath two raised arches. / Mission Fighter's wings form clean rearward wedges; Alien Jet uses one thin irregular swept plate. / Mission Fighter has a solid pointed nose; Alien Jet's arches create a persistent central negative space.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Broad swept black plate — creates the tiny interceptor plan — SOURCE_VERIFIED.
  - Exposed central alien pilot — preserves the open micro-craft scale — SOURCE_VERIFIED.
  - Two tall lime conduit arches — dominate front and side recognition — SOURCE_VERIFIED.
- Structural load path: One swept plate carries the open pilot frame, paired flexible arches, forward emitter and compact rear equipment block.
- Repeated modules / connection grammar: Swept deck, pilot/control center, paired arches and nose emitter remain independently readable.
- Source-faithful versus adapted boundary: True-air interpolation and energy pulse timing are adapted; no enclosed canopy, bulky engine pod or folding nose panels.

## E. Material and texture contract

- Geometry must carry:
  - broad swept plate
  - open pilot cavity
  - paired tall lime arches
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Fast true-air interception with sharp but readable banking.
- Planted/contact rule: No battlefield ground contact; altitude is shown by shadow and stable flight datum.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ConduitLeft` | Asset_AlienJet | very small load flex; source rest retained | turn intensity and damage presentation |
| `Pivot_ConduitRight` | Asset_AlienJet | mirrored small flex | turn intensity and damage presentation |

- Required beats:
  - Idle aggressive loiter.
  - Travel uses crisp banking around the flat plate.
  - Attack brightens the forward emitter for one bounded pulse.
  - Damage destabilizes one arch; destruction breaks plate and rear block.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Muzzle`, `Socket_Engine`, `Socket_Surge`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Rear propulsion and landing detail remain subordinate until better rear/underside evidence appears.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
