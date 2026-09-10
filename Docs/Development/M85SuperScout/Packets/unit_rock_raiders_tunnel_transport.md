# Tunnel Transport — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.tunnel_transport`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Heavy air transport
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 4980
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
| 4980 — Tunnel Transport | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/4980)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128427.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=4980-1) | PRIMARY_VERIFIED | twin-propeller heavy lift construction |

### Source audit [RockRaiders:4980]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4128427.pdf)
- Construction map:
  - Evidence pages 2-14: Forward cockpit/cargo vehicle module with low worksite chassis and open load bed.
  - Evidence pages 15-20: Separate compact wheeled support module.
  - Evidence pages 21-29: Wide rotor transport frame, twin propeller pods and attachment to the carried modules.
  - Evidence pages 30-32: Cargo container, alternate carried load and final multi-angle product photography.
- View/mechanism coverage: front=VERIFIED p1 and p23-32; rear=VERIFIED p24-32; leftRight=VERIFIED p21-32; top=VERIFIED p21-29; threeQuarter=VERIFIED p1 and p27-32; undersideInterior=VERIFIED p21-29 open transport frame and load connections; mechanism=PARTIAL p21-29 rotors and cargo attachment; no rotor animation sequence
- Verified findings:
  - The aircraft is a skeletal load-bearing bridge with two giant rotor pods, not a conventional enclosed helicopter fuselage.
  - Carried modules remain visibly independent beneath/within the transport frame.
  - The long crossbeam, pod spacing and open central cradle are the primary structural identity.
- Remaining evidence gaps:
  - Rotor pitch and suspension response are not authored by the manual and remain presentation interpretations.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A broad industrial airlifter suspended between two giant propeller pods and a visible cargo cradle.

Non-removable identity anchors:

- twin oversized propeller pods
- open underslung cargo frame
- wide load-bearing crossbeam

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.astronauts.mx71_recon_dropship` — Both are large true-air transports. Mitigations: Tunnel Transport is carried by two giant propeller pods; MX-71 uses broad fixed wings and compact engines. / Tunnel Transport has an industrial crossbeam and open heavy cradle; MX-71 has a streamlined cockpit and rover-sized underslung bay. / Tunnel Transport reads teal, skeletal and load-first; MX-71 reads white-orange, aerodynamic and expedition-first.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Wide load-bearing crossbeam — carries the entire aircraft between separated rotor pods — SOURCE_VERIFIED.
  - Twin oversized rotor pods — provide true-air lift and the primary silhouette — SOURCE_VERIFIED.
  - Open central cargo cradle — visibly accepts Raider machines up to one Chrome Crusher — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Cockpit/control module — remains subordinate to the lifting frame and payload — SOURCE_VERIFIED.
- Structural load path: Two rotor pods feed lift into a wide skeletal crossbeam; the cargo cradle hangs from the beam center so the carried machine remains visibly independent.
- Repeated modules / connection grammar: Mirrored rotor pods, structural bridge, control module, cargo suspension/cradle and optional carried load.
- Source-faithful versus adapted boundary: The game may enlarge clearances for canonical capacity but may not add an enclosed fuselage or normal weapon system.

## E. Material and texture contract

- Geometry must carry:
  - wide crossbeam
  - two rotor discs/pods
  - open cargo cradle
  - visible carried load separation
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_heavy_frame_surface` — Subtle large-scale molded/painted industrial surface variation on broad frames without drawing false seams. Channels: Tangent-space normal plus linear roughness; body color remains a material parameter. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space field, 4 m repeat; no per-part phase reset.; LOD fallback: Half strength at Combat; disabled at Strategic in favor of master-material roughness. Provenance/state: Project-authored procedural source; human review required before production use. `SPECIFIED_NOT_AUTHORED`.
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_console_and_signal_atlas` — Geological readouts, service-state lamps and operational signal faces. Channels: sRGB color/alpha plus separate linear emission mask; transparent polymer is not automatically emissive. Resolution: 512x512; texel density: Screen-space authored atlas; one texel density is not applicable.; tiling: Non-tiling atlas with stable panel IDs.; LOD fallback: Replace screens with one bounded Signal or Lamp color block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: True air; heavy acceleration/turning with broad bank limits and visible payload inertia.
- Planted/contact rule: Normally airborne; loading requires a stationary low hover or source-respecting landing pose with the cradle aligned to the target.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_RotorLeft` | Asset_TunnelTransport | continuous roll around source rotor axis; pitch remains fixed unless later evidence supports it | airborne/movement state |
| `Pivot_RotorRight` | Asset_TunnelTransport | continuous roll around source rotor axis | airborne/movement state |
| `Pivot_CargoHoist` | Asset_TunnelTransport | bounded vertical cable/cradle path from travel lock to loading height | authoritative load/unload progress |
| `Pivot_CargoSway` | Pivot_CargoHoist | small damped fore/aft and lateral swing; zero at load completion | presentation-only response to acceleration |

- Required beats:
  - Idle hover maintains slow heavy rotor response.
  - Move banks the bridge slightly while payload sway lags and settles.
  - Load/unload enters stationary hover, lowers/aligns cradle, locks cargo, then raises it.
  - Damage introduces asymmetric pod vibration; destruction separates a rotor pod and releases cargo according to authoritative transport outcome.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_CargoAttach`, `Socket_LoadApproach`, `Socket_RotorVfxLeft`, `Socket_RotorVfxRight`, `Socket_Lamp`, `Socket_AudioRotorLeft`, `Socket_AudioRotorRight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Rotor pitch behavior is not source-proven; keep it fixed unless greybox readability demonstrates a need for a reviewed presentation adaptation.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
