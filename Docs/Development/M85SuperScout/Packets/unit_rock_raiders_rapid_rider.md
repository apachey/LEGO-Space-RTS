# Rapid Rider — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.rapid_rider`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Crew transport
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4920
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
| 4920 — Rapid Rider | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4920)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128168.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4920-1) | PRIMARY_VERIFIED | twin-hull transport and propulsion |

### Source audit [RockRaiders:4920]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1-2: Complete twin-hull Rapid Rider build, central deck, raised canopy/bridge, rear drive equipment and carried rock load.
- View/mechanism coverage: front=PARTIAL p1 cover and p2 final; rear=PARTIAL p2 steps 10-14; leftRight=PARTIAL p1-2 construction sequence; top=VERIFIED p1-2; threeQuarter=VERIFIED p1 cover and p2 final; undersideInterior=PARTIAL p1 steps 1-4 expose hull foundations; mechanism=PARTIAL p2 rear propulsion and cargo placement; no movement sequence
- Verified findings:
  - Two long parallel hulls remain separate around a narrow central deck.
  - The open load/passenger zone is structural negative space and must not be roofed over.
  - Rear propulsion and carried cargo are visually subordinate to the twin-hull plan.
- Remaining evidence gaps:
  - Acquire a clean orthogonal rear view before final propulsion placement.
  - Amphibious hover behavior is canonical adaptation and is not demonstrated by the static manual.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A twin-hull underground water skimmer with a clearly open cargo/passenger gap.

Non-removable identity anchors:

- parallel catamaran hulls
- rear propulsion pair
- open central cargo and rider space

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.hover_scout` — Both are small low Rock Raider utility craft. Mitigations: Hover Scout has one flat survey deck; Rapid Rider has two parallel hulls. / Hover Scout carries a forward scanner; Rapid Rider carries paired rear propulsion. / Hover Scout reads as single-seat information equipment; Rapid Rider preserves an open passenger/cargo gap.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Twin hulls — provide the amphibious/skimmer base and unmistakable catamaran silhouette — SOURCE_VERIFIED.
  - Open central deck — holds up to four Crew and keeps loading visually exposed — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Rear propulsion pair — explains water/rough-surface motion without becoming weaponry — SOURCE_VERIFIED.
- Structural load path: Two long hulls connect through narrow transverse beams under the central deck; passenger load stays between them and below the raised bridge/canopy.
- Repeated modules / connection grammar: Mirrored hull pair, central operator/passenger deck, rear propulsion pair and restrained cargo mounts.
- Source-faithful versus adapted boundary: Limited land skimming is canonical; no wheels, enclosed cabin or combat hardpoint may be invented.

## E. Material and texture contract

- Geometry must carry:
  - separate long hulls
  - open passenger gap
  - rear propulsion pair
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Amphibious surface skimmer; fast ground-layer travel with small twin-hull heave and no true-air climb.
- Planted/contact rule: Both hull undersides share one low surface datum; loading/unloading requires a stationary settled pose.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_PropulsionLeft` | Asset_RapidRider | roll around visible drive axis; stopped rest | movement speed |
| `Pivot_PropulsionRight` | Asset_RapidRider | roll around visible drive axis; stopped rest | movement speed |

- Required beats:
  - Idle hulls settle independently by a very small amount.
  - Move adds restrained propulsion spin and water/dust response.
  - Load/unload settles first, then Crew enter or leave through the open center one at a time.
  - Damage unbalances one hull; wreck keeps the twin-hull negative space readable.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_PassengerEntry`, `Socket_PassengerExit`, `Socket_Cargo`, `Socket_PropulsionVfxLeft`, `Socket_PropulsionVfxRight`, `Socket_AudioDrive`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final rear propulsor clearance waits for a clean orthogonal rear reference; this does not alter the twin-hull contract.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
