# Crystal Vault — T082 Super Scout packet

**Stable ID:** `building.rock_raiders.crystal_vault`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Infrastructure`
- Gameplay role: Crystal storage / advanced technology
- Authoritative footprint: `Large`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 4970, 4990
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
| 4990 — Rock Raiders HQ | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4990)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4990-1) | PRIMARY_VERIFIED | industrial architecture, crane, processing and service motifs |

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

### Source audit [RockRaiders:4990]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4129017.pdf)
- Construction map:
  - Evidence pages 2-7: Small work vehicles, crystal handling and compact workstation modules.
  - Evidence pages 8-10: Tall illuminated machinery/power tower with open service access.
  - Evidence pages 11-19: Long articulated crane/tool boom mounted to the tower and used for rock handling.
  - Evidence pages 20-29: Open vehicle-width gantry/workshop with sloped supports, lamps and overhead rails.
  - Evidence pages 30-34: Conveyor/processing module attaches to the gantry and completes a visible material route.
  - Evidence pages 35-40: Modules connect across an irregular rock worksite base rather than a sealed building shell.
  - Evidence pages 41-43: Final product photography supplies overall skyline, module spacing and worksite context.
- View/mechanism coverage: front=VERIFIED p41-43; rear=PARTIAL p35-43; leftRight=VERIFIED p35-43; top=VERIFIED p35-40; threeQuarter=VERIFIED p1 and p41-43; undersideInterior=VERIFIED p2-40 staged module and base construction; mechanism=VERIFIED p11-19 crane/tool boom; PARTIAL p20-34 gantry/conveyor service path
- Verified findings:
  - HQ identity comes from a loose network of independently readable work modules on an uneven base, not from a single enclosed headquarters block.
  - Tower, articulated crane, open vehicle gantry and conveyor/processing path establish the reusable infrastructure grammar.
  - Entrances and service lanes remain physically open and minifigure/vehicle scaled.
- Remaining evidence gaps:
  - The manual supports modular industrial functions but does not assign the game's exact Ore Plant, Power Station, Service Bay or Workshop boundaries; those remain explicit canonical adaptations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A visibly reinforced industrial store wrapped around a luminous but non-emissive crystal chamber.

Non-removable identity anchors:

- central protected crystal cage
- thick segmented blast frame
- small guarded transfer hatch

- Rejected V1 blind-review code: `S01`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Central crystal cage — stores/processes the advanced resource while leaving it readable — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Thick segmented blast frame — communicates durable secure storage — CANON_DERIVED_ADAPTATION.
  - Small guarded transfer hatch — shows controlled input/output and distinguishes the Vault from a power generator — CANON_DERIVED_ADAPTATION.
  - Mechanical clamps — hold crystals as material, not as a magical floating core — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A heavy outer frame anchors radial/paired clamps around a central chamber; the transfer hatch remains crew-scale and deliberately smaller than the main cage.
- Repeated modules / connection grammar: Protected chamber, segmented frame, clamp set, guarded transfer hatch and service/control platform.
- Source-faithful versus adapted boundary: Source crystal/cargo handling informs the facility, but exact enclosure is new; stored crystals do not become an always-emissive reactor.

## E. Material and texture contract

- Geometry must carry:
  - outer blast frame
  - central cage/chamber
  - mechanical clamps
  - small transfer hatch
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Static infrastructure.
- Planted/contact rule: Broad blast-frame feet surround the chamber and visually resist outward load.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_TransferHatch` | Asset_CrystalVault | short guarded hinge/slide from sealed to transfer opening | authoritative crystal delivery/use state |
| `Pivot_ClampSet` | Asset_CrystalVault | synchronized small radial open/close path | crystal transfer and operational state |

- Required beats:
  - Idle keeps clamps closed and crystal lighting steady, not pulsing by default.
  - Transfer opens the small hatch, releases clamps only as needed, then reseals.
  - Brownout removes active signals but preserves physical storage.
  - Damage cracks outer frame modules first; destruction releases controlled non-authoritative crystal debris after gameplay resolution.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_CrystalReceive`, `Socket_CrystalChamber`, `Socket_TransferVfx`, `Socket_Lamp`, `Socket_AudioClamp`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The chamber's exact transparent-versus-open ratio needs material/silhouette review; the crystal remains visible and non-self-emissive unless a real signal/lamp function is assigned.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
