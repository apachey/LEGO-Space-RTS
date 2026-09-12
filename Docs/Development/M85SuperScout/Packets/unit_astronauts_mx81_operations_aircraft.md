# MX-81 Hypersonic Operations Aircraft — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mx81_operations_aircraft`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Strategic scan / support
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7644
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
| 7644 — MX-81 Hypersonic Operations Aircraft | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7644)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534843.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4537579.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7644-1) | PRIMARY_VERIFIED | large modular operations aircraft |

### Source audit [Astronauts:7644]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534843.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4537579.pdf)
- Construction map:
  - Evidence pages book 1, 3-35: Alien scout craft and separate mission payload pods; supporting opposition evidence.
  - Evidence pages book 1, 36-75; book 2, 2-24: Tall modular service/launch tower and platform completed across both books.
  - Evidence pages book 2, 26-60: MX-81 central aircraft hull, broad wing plane, cockpit and twin orange engine masses.
  - Evidence pages book 2, 61-73: Long side booms, detachable operational craft/pods and final multi-module aircraft assembly.
- View/mechanism coverage: front=VERIFIED book 2 p52-76; rear=VERIFIED book 2 p60-76; leftRight=VERIFIED book 2 p26-76; top=VERIFIED book 2 p26-73; threeQuarter=VERIFIED covers and book 2 p73-76; undersideInterior=VERIFIED book 2 p26-60 staged airframe; mechanism=PARTIAL book 2 p61-73 detachable side modules and launch/service tower; flight operation not animated
- Verified findings:
  - MX-81 is a very wide flying operations platform with a dense central command hull and long thin span.
  - Twin orange engine/pod masses sit forward of a layered white-and-black wing plane.
  - Small independently readable support craft and equipment pods attach along the broad carrier frame rather than disappearing inside a solid fuselage.
- Remaining evidence gaps:
  - The manuals prove modular carried craft and support architecture but not the game's exact scan, transport or in-flight service functions; their attachment and launch states require an asset-specific contract.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A huge multi-module operations aircraft with a long command spine and visibly separable mission sections.

Non-removable identity anchors:

- very wide hypersonic wing plan
- long modular command spine
- multiple detachable mission pods

- Rejected V1 blind-review code: `S43`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.solar_explorer` — Both are large modular Astronaut support assets. Mitigations: Solar Explorer is a long three-module ground carrier on subordinate adapted running gear; MX-81 is a very wide true-air wing. / Solar Explorer is dominated by one curved honeycomb habitat canopy and a circular rear frame; MX-81 carries several detachable mission pods across its span. / Solar Explorer retains rugged Field construction; MX-81 uses clean high-performance Mission geometry.
- `unit.aliens.alien_mothership` — Both are huge extremely wide airborne command/support silhouettes. Mitigations: MX-81 is a long command spine crossed by distinct wing and pod modules; Mothership is one interrupted circular hull. / MX-81 preserves several gaps between detachable mission pods; Mothership preserves one continuous open machinery channel. / MX-81 has a clear nose-to-tail direction; Mothership reads radially until its tail and bay opening establish heading.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Very wide hypersonic wing plan — establishes flagship aerospace scale — SOURCE_VERIFIED.
  - Long central command spine — carries cockpit, sensors and operations spaces — SOURCE_VERIFIED.
  - Multiple docked mission modules — expose scan, service and limited transport functions while remaining one aircraft — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: A long reinforced center spine carries the wide wing and distributes docked module loads to visible attachment points.
- Repeated modules / connection grammar: Main aircraft, scan/sensor sections and mission pods remain visually separable but one selectable support unit.
- Source-faithful versus adapted boundary: Source modules support different operations, not independent buildable units or superweapon damage.

## E. Material and texture contract

- Geometry must carry:
  - very wide wing
  - long command spine
  - multiple docked mission pods
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Huge true-air platform with slow deliberate banking.
- Planted/contact rule: No battlefield ground contact; service presentation occurs at Flight Operations Pad.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_SensorArray` | Asset_MX81 | bounded scan yaw/pitch | authoritative scan support state |
| `Pivot_MissionPod` | Asset_MX81 | small dock/lock service path | service presentation only |

- Required beats:
  - Idle high-altitude loiter.
  - Travel uses broad slow banking.
  - Scan opens sensors and emits a bounded pulse.
  - Damage disables a secondary pod first; destruction breaks wing sections from spine.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ScanPulse`, `Socket_Service`, `Socket_EngineLeft`, `Socket_EngineRight`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact source module retained for limited transport must be selected before production modeling.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
