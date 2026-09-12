# Solar Explorer — T082 Super Scout packet

**Stable ID:** `unit.astronauts.solar_explorer`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Forward service / refit
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7315
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
| 7315 — Solar Explorer | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7315)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7315-1) | PRIMARY_VERIFIED | solar arrays, field modules and service construction |

### Source audit [Astronauts:7315]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130810.pdf)
- Construction map:
  - Evidence pages 2-7: Detachable forward cockpit craft with low exploration nose and broad side plates.
  - Evidence pages 8-17: Long modular habitation/cargo body under one curved honeycomb canopy plus separate small support pod.
  - Evidence pages 18-23: Compact rear service section with circular frame, dome and paired small fins attached to the long body.
  - Evidence pages 24-26: Cross-set alternate models and extended modular combinations; not direct production geometry.
- View/mechanism coverage: front=VERIFIED p1 and p22-23; rear=PARTIAL p18-23; leftRight=VERIFIED p2-23; top=VERIFIED p2-23; threeQuarter=VERIFIED p1 and p22-26; undersideInterior=VERIFIED p2-21 staged construction; mechanism=PARTIAL p18-23 separable solar/service module; deployment not demonstrated
- Verified findings:
  - Solar Explorer identity comes from one long low non-wheeled modular carrier rather than from the separate small four-wheel rover.
  - The central habitat/cargo box is covered by one large curved honeycomb canopy; the compact rear section is organized around a circular frame, dome and paired small fins.
  - Detachable cockpit craft, habitat/cargo body, support pod and rear service section remain independently readable modules.
- Remaining evidence gaps:
  - The manual supports separable modules but not the game's deployed Forward Service state; stabilizers, access route and deployment motion remain explicit adaptation work.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long modular ground-support carrier preserving the detachable cockpit craft, honeycomb-canopied habitat and circular-frame rear service section of 7315 on a disclosed adapted running gear.

Non-removable identity anchors:

- large curved honeycomb canopy
- long three-module carrier
- circular rear frame with paired small fins
- clearly subordinate adapted ground running gear

- Rejected V1 blind-review code: `S46`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mx81_operations_aircraft` — Both are large modular Astronaut support assets. Mitigations: Solar Explorer is a long three-module ground carrier on subordinate adapted running gear; MX-81 is a very wide true-air wing. / Solar Explorer is dominated by one curved honeycomb habitat canopy and a circular rear frame; MX-81 carries several detachable mission pods across its span. / Solar Explorer retains rugged Field construction; MX-81 uses clean high-performance Mission geometry.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Large curved honeycomb canopy — dominates the central habitat/cargo module — SOURCE_VERIFIED.
  - Long three-module source body — joins cockpit craft, habitat and rear service section without pretending the official set supplied road wheels — SOURCE_VERIFIED.
  - Subordinate ground running gear — fulfills the canonical wheeled support role while preserving the 7315 modules above it — CANON_DERIVED_ADAPTATION.
- Structural load path: A disclosed adapted ground carrier supports the three source-faithful modules through visible standardized couplers without swallowing their original forms.
- Repeated modules / connection grammar: Detachable cockpit craft, honeycomb-canopied habitat/cargo body, separate support pod, circular-frame rear service section and adapted running gear remain distinct.
- Source-faithful versus adapted boundary: The official source proves separable modules and a small independent four-wheel rover but no wheels for the complete assembly; the game's large wheeled ground-platform requirement is a canonical adaptation and must remain visually subordinate.

## E. Material and texture contract

- Geometry must carry:
  - large curved honeycomb canopy
  - long three-module carrier
  - circular rear frame with paired small fins
  - subordinate adapted ground running gear
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_field_frame_surface` — Restrained molded and brushed variation for rugged blue-gray Field Systems frames without faking structural seams. Channels: Tangent-space normal and linear roughness; body color stays parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 3 m repeat across connected frames.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Field Systems surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Large canonical wheeled ground-support platform with restrained module lag.
- Planted/contact rule: Every adapted road wheel stays grounded in travel; the service module plants before repair/refit begins.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_RunningGearFront` | Asset_SolarExplorer | wheel roll and restrained suspension travel | distance traveled and terrain response |
| `Pivot_RunningGearRear` | Asset_SolarExplorer | wheel roll and delayed module-follow suspension travel | distance traveled and terrain response |
| `Pivot_ServiceModule` | Asset_SolarExplorer | connected rear position to released planted service placement | authoritative deployment progress |

- Required beats:
  - Idle connected systems check.
  - Travel keeps the three source modules dominant over the adapted running gear.
  - Deploy stops the carrier, releases the service section and plants it visibly.
  - Undeploy reverses the coupler sequence; destruction leaves the three major modules readable.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Service`, `Socket_Refit`, `Socket_PassengerEntry`, `Socket_PassengerExit`, `Socket_AudioDrive`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Because 7315 supplies no complete-vehicle wheels, the exact adapted wheel count, axle placement and continuous service-module deployment connection require a later director-reviewed production blockout.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
