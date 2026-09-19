# Chrome Crusher — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.chrome_crusher`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Heavy siege
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4970
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
| 4970 — The Chrome Crusher | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4970)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129264.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4970-1) | PRIMARY_VERIFIED | heavy wheeled drill, light and cargo machinery |

### Source audit [RockRaiders:4970]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129264.pdf)
- Construction map:
  - Evidence pages 2-11: Long forked chassis and independently built front control side modules.
  - Evidence pages 12-26: Motor block, raised frame, cockpit cage, cargo deck and flexible power/tool routing.
  - Evidence pages 27-33: Overhead tool/light beam, drill subassembly, wheel/light modules and final machine assembly.
  - Evidence pages 34-36: Drill motor operation/safety evidence and final mechanism instructions.
- View/mechanism coverage: front=VERIFIED p1 and p29-34; rear=PARTIAL p21-26; leftRight=VERIFIED p2-34 construction sequence; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p29-33; undersideInterior=VERIFIED p2-20 chassis and motor build; mechanism=VERIFIED p27-36 drill drive, wheel modules and movable tool/light assembly
- Verified findings:
  - The vehicle is a long open industrial chassis wrapped around motor, cargo and tool systems rather than a solid armored hull.
  - The powered drill projects far beyond the front cage and is mechanically connected to the internal motor.
  - Four huge wheels are separate side modules; the raised work-light/tool beam forms a second skyline above the drill.
- Remaining evidence gaps:
  - A strict orthogonal rear photograph remains desirable for final cargo and cable clearance.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long heavy mining vehicle with four huge wheels and a chrome drill projecting beyond the chassis.

Non-removable identity anchors:

- four individually readable huge wheels
- long front chrome drill
- raised work-light and cargo machinery

- Rejected V1 blind-review code: `S42`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.granite_grinder` — Both are major teal drill machines. Mitigations: Granite Grinder has two legs; Chrome Crusher has four huge wheels. / Granite Grinder is tall and narrow; Chrome Crusher is long and low. / Granite Grinder balances one boom; Chrome Crusher combines drill, work light and cargo machinery along a heavy chassis.
- `unit.astronauts.mt101_armored_drilling_unit` — Both are large wheeled heavy drill assault machines. Mitigations: Chrome Crusher has four giant wheels; MT-101 has six broad suspended barrel wheels. / Chrome Crusher exposes teal industrial work machinery; MT-101 closes paired white-orange curved shells over an open black chassis. / Chrome Crusher's drill shares the silhouette with a raised work light and cargo gear; MT-101 separates its spiky two-disc drill from a prominent horizontal Zamor launcher above the cabin.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Long forked chassis — carries motor, cargo and operator systems as an open industrial frame — SOURCE_VERIFIED.
  - Four huge wheel modules — provide the dominant mass and massive slow-ground read — SOURCE_VERIFIED.
  - Powered chrome drill — performs direct structure/heavy breaching and projects beyond the cage — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Raised tool/light beam and cargo deck — form the secondary skyline and service identity — SOURCE_VERIFIED.
- Structural load path: The long forked frame ties four separate wheel modules to a central motor block; drill power and thrust remain visibly routed through the nose support into that frame.
- Repeated modules / connection grammar: Four repeated wheels, central motor/cage, front drill drive, overhead work-light/tool beam and open cargo deck.
- Source-faithful versus adapted boundary: Armor may reinforce the source frame but may not seal it into a tank hull or hide the motor/cargo/tool relationships.

## E. Material and texture contract

- Geometry must carry:
  - open forked chassis
  - four huge wheels
  - drill motor path and helix
  - raised beam and cargo deck
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Massive wheeled movement with slow steering, visible suspension lag and no skid-like agility.
- Planted/contact rule: All four wheels stay grounded; breaching settles the chassis and compresses front suspension before tool contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WheelFrontLeft` | Asset_ChromeCrusher | roll on axle with bounded steering yaw | movement distance and steering |
| `Pivot_WheelFrontRight` | Asset_ChromeCrusher | roll on axle with bounded steering yaw | movement distance and steering |
| `Pivot_WheelRearLeft` | Asset_ChromeCrusher | roll on axle | movement distance |
| `Pivot_WheelRearRight` | Asset_ChromeCrusher | roll on axle | movement distance |
| `Pivot_DrillSpin` | Asset_ChromeCrusher | source-aligned roll around drill axis | authoritative attack/excavation progress |
| `Pivot_ToolBeam` | Asset_ChromeCrusher | bounded pitch at visible hinge from travel rest to work aim | idle work-light or demolition presentation |

- Required beats:
  - Idle engine and hoses show low-frequency mechanical vibration.
  - Move emphasizes wheel rotation, mass lag and frame flex.
  - Breach settles, spins the drill, compresses the nose and recovers after the authoritative hit.
  - Damage disables a secondary light/tool module first; destruction releases drill/beam/wheel modules before frame collapse.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_DrillContact`, `Socket_DrillDust`, `Socket_DrillSparks`, `Socket_LampWork`, `Socket_Cargo`, `Socket_AudioEngine`, `Socket_AudioDrill`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final rear cable and cargo clearance needs an orthogonal rear reference but does not block the verified chassis/load-path contract.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
