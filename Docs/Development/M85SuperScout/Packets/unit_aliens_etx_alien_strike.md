# ETX Alien Strike — T082 Super Scout packet

**Stable ID:** `unit.aliens.etx_alien_strike`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Air / deployed siege
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7693
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
| 7693 — ETX Alien Strike | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7693)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7693-1) | PRIMARY_VERIFIED | alien strike transformation and human mining module |

### Source audit [Aliens:7693]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf)
- Construction map:
  - Evidence pages 3-16: Astronaut mining vehicle; supporting opposition evidence.
  - Evidence pages 17-35: ETX Alien Strike central keel, hinged side frames, paired crescent lobes, detachable corner modules and long translucent-lime tail blades.
  - Evidence pages 36-59: Cross-set alternate astronaut craft; not direct Alien Strike production geometry.
- View/mechanism coverage: front=VERIFIED p28-35; rear=PARTIAL p28-35; leftRight=VERIFIED p17-35; top=VERIFIED p17-35; threeQuarter=VERIFIED cover and p28-35; undersideInterior=VERIFIED p17-28 staged open frame; mechanism=PARTIAL p24-35 hinged crescent/side modules and detachable corner pads; no complete flight-to-siege sequence
- Verified findings:
  - Alien Strike is built around a long central black keel with a lime multi-blade tail and two huge crescent side lobes.
  - The lobes and smaller corner pads attach through visible pivots, creating a much wider final attack geometry around the narrow flight spine.
  - Gray central structure, red forward facets and translucent-lime tail layers remain deliberately exposed inside the black shell.
- Remaining evidence gaps:
  - The manual proves rearrangeable hinged modules but does not show the game's exact flight and planted siege endpoints, support contacts or high-output weapon path; those states require explicit adaptation.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A curved attack craft whose wing halves unfold into a planted siege frame around the central core.

Non-removable identity anchors:

- split curved wing shell
- central crystal-cockpit mass
- visible deployed ground braces

- Blind-review code: `S53`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.aliens.etx_alien_infiltrator` — Both are medium transforming ETX craft. Mitigations: Alien Strike unfolds wide into planted siege braces; Infiltrator rises on three planted members. / Alien Strike preserves two huge crescent lobes and a lime tail; Infiltrator preserves a long split nose and paired curved side modules. / Alien Strike's deployed state points fire outward; Infiltrator's walker state places the crew core above an anti-heavy contact stance.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Long central keel and lime tail blades — remain the directional spine in both states — SOURCE_VERIFIED.
  - Two huge crescent side lobes — widen from flight shell into siege frame — SOURCE_VERIFIED.
  - Planted corner contacts and concentrated emitter path — convert the hinged source modules into readable siege operation — CANON_DERIVED_ADAPTATION.
- Structural load path: The central keel carries the cockpit/core and tail while visible side hinges route each crescent lobe into adapted planted contacts.
- Repeated modules / connection grammar: Central flight spine, mirrored crescent lobes, corner pads, tail blades and siege emitter remain one transforming craft.
- Source-faithful versus adapted boundary: The source proves hinges but not final endpoints. Production must preserve those modules while presenting credible flight and planted states without hand separation.

## E. Material and texture contract

- Geometry must carry:
  - long keel and tail blades
  - paired huge crescents
  - visible planted siege braces
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: True air in Strike state; stationary ground-targetable siege platform when deployed.
- Planted/contact rule: No ground contact in flight; all adapted braces lock before siege readiness.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CrescentLeft` | Asset_AlienStrike | flight shell to wide planted siege arc | authoritative deployment progress |
| `Pivot_CrescentRight` | Asset_AlienStrike | mirrored deployment arc | authoritative deployment progress |
| `Pivot_Emitter` | Asset_AlienStrike | stowed channel to forward siege aim | siege attack progress |

- Required beats:
  - Idle flight loiter.
  - Flight travel keeps crescents swept.
  - Deploy lands, opens and locks contacts before weapon-ready signal.
  - Siege attack charges along the keel; damage does not change authoritative state timing.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Muzzle`, `Socket_BraceLeft`, `Socket_BraceRight`, `Socket_Surge`, `Socket_AudioTransform`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The final planted endpoints and continuous load path need director-approved transformation blockout.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
