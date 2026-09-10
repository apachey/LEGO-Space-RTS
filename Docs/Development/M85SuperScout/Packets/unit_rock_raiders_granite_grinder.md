# Granite Grinder — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.granite_grinder`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Anti-heavy drill walker
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4940
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
| 4940 — Granite Grinder | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4940)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128317.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4940-1) | PRIMARY_VERIFIED | drill walker construction and articulation |

### Source audit [RockRaiders:4940]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128317.pdf)
- Construction map:
  - Evidence pages 2-12: Two mirrored ski-foot/leg modules and their shared upper bridge are assembled independently.
  - Evidence pages 13-22: Long central drill craft body, cockpit cage, rear wheel/tool mass and drill boom are built as a separate module.
  - Evidence pages 23-24: Upper drill module is mounted across the paired leg modules; final operator-scale three-quarter view.
- View/mechanism coverage: front=PARTIAL p1 and p24; rear=PARTIAL p18-22; leftRight=VERIFIED p13-24 construction rotation; top=VERIFIED p13-23; threeQuarter=VERIFIED p1 and p23-24; undersideInterior=VERIFIED p2-16 staged subassemblies; mechanism=PARTIAL p23 module connection; gait and drill motion not demonstrated
- Verified findings:
  - The recognizable walker is a bridge between two mirrored planted foot modules, not a wheeled chassis with decorative legs.
  - The drill/cockpit body is a removable longitudinal module carried above the feet.
  - The long drill is structurally balanced by rear wheel/equipment mass and a high open cage.
- Remaining evidence gaps:
  - The manual proves modular construction but not a walking gait; leg articulation and contact phases remain an explicit adaptation decision.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tall two-legged drilling machine balanced around a long central tool.

Non-removable identity anchors:

- bipedal planted legs
- long forward drill boom
- high open operator cage

- Blind-review code: `S22`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.drill_craft` — Both are short-range Raider excavation machines. Mitigations: Drill Craft stays low on an open hover sled; Granite Grinder is a tall planted biped. / Drill Craft frames its operator between paired raised side pods; Granite Grinder places the operator high above two large feet. / Drill Craft keeps two small mirrored saw discs; Granite Grinder carries one long drill boom balanced by rear machinery.
- `unit.rock_raiders.chrome_crusher` — Both are major teal drill machines. Mitigations: Granite Grinder has two legs; Chrome Crusher has four huge wheels. / Granite Grinder is tall and narrow; Chrome Crusher is long and low. / Granite Grinder balances one boom; Chrome Crusher combines drill, work light and cargo machinery along a heavy chassis.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Mirrored ski-foot leg modules — create the planted walker base and carry the upper bridge — SOURCE_VERIFIED.
  - Long drill/cockpit body — delivers the anti-heavy tool through a readable longitudinal load path — SOURCE_VERIFIED.
  - Rear wheel/equipment counter-mass — balances the long forward drill and prevents a generic humanoid read — SOURCE_VERIFIED.
  - Brace behavior — transfers sustained drill force into both planted feet — CANON_DERIVED_ADAPTATION.
- Structural load path: Two mirrored foot/leg modules carry a transverse bridge; the removable drill body spans that bridge so tool thrust is visibly shared by both feet.
- Repeated modules / connection grammar: Mirrored planted legs, shared bridge, longitudinal drill/cockpit module and rear counterweight.
- Source-faithful versus adapted boundary: The source proves construction but not gait; the production walk must preserve the broad two-foot contact and avoid agile humanoid motion.

## E. Material and texture contract

- Geometry must carry:
  - paired ski feet
  - bridge gap
  - long drill boom
  - rear counterweight
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Heavy biped walker with short deliberate alternating steps and minimal upper-body sway.
- Planted/contact rule: At least one ski-foot stays planted during travel; both feet lock before the 2.5-second sustained drill ramp.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_LegLeft` | Asset_GraniteGrinder | constrained fore/aft step arc with small vertical clearance | locomotion phase |
| `Pivot_LegRight` | Asset_GraniteGrinder | opposed constrained step arc with small vertical clearance | locomotion phase |
| `Pivot_DrillFeed` | Asset_GraniteGrinder | short forward pitch/feed toward target; travel breaks state | authoritative contact and ramp progress |
| `Pivot_DrillSpin` | Pivot_DrillFeed | roll around drill axis | authoritative attack/excavation progress |

- Required beats:
  - Idle alternates faint hydraulic pressure between planted feet.
  - Move uses deliberate foot lift, plant, weight transfer and settle.
  - Attack plants both feet, feeds drill, ramps vibration, then releases on movement or target loss.
  - Damage introduces bridge/leg lag; wreck collapses across the two foot modules with drill still recognizable.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootLeft`, `Socket_FootRight`, `Socket_DrillContact`, `Socket_DrillDust`, `Socket_DrillSparks`, `Socket_AudioDrill`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact gait amplitudes require greybox camera review because the manual contains no walking sequence; the contact rule is locked, the style is not.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
