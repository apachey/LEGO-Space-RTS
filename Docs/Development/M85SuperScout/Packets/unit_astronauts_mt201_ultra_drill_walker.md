# MT-201 Ultra-Drill Walker — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mt201_ultra_drill_walker`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Deployed siege
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7649
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
| 7649 — MT-201 Ultra-Drill Walker | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7649)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538485.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538486.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7649-1) | PRIMARY_VERIFIED | drill installation-to-walker transformation |

### Source audit [Astronauts:7649]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538485.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4538486.pdf)
- Construction map:
  - Evidence pages book 1, 3-20: Alien strike craft; supporting opposition evidence.
  - Evidence pages book 1, 21-46; book 2, 2-18: Low travel chassis, operator cab and central rotating machinery core.
  - Evidence pages book 2, 19-27: Massive longitudinal drill and drive assembly.
  - Evidence pages book 2, 28-34: Forward cockpit/tool pod and central module completion.
  - Evidence pages book 2, 35-43: Four independent ball-jointed leg pods attach and rotate into the deployed walker stance.
- View/mechanism coverage: front=VERIFIED book 2 p34-43; rear=VERIFIED book 2 p35-43; leftRight=VERIFIED both books; top=VERIFIED book 2 p2-43; threeQuarter=VERIFIED covers and book 2 p42-47; undersideInterior=VERIFIED book 2 p2-42; mechanism=VERIFIED book 2 p35-43 four-leg deployment and body rotation
- Verified findings:
  - MT-201 is a central drill/cockpit machine surrounded by four independently built articulated leg pods.
  - Each broad white leg has a dark planted foot and visible ball-joint connection.
  - Deployment rotates the leg set around the central machinery so travel and anchored drilling states remain visibly related.
- Remaining evidence gaps:
  - The source shows manual leg repositioning but not a timed gait or stable drilling contact sequence; both require an explicit animation contract.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A colossal drill installation that opens into a planted four-legged siege walker.

Non-removable identity anchors:

- enormous vertical drill tower
- four deployable stabilizer legs
- detachable observation craft

- Rejected V1 blind-review code: `S18`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mt101_armored_drilling_unit` — Both are advanced heavy Mission drilling machines. Mitigations: MT-101 remains a long six-wheel vehicle; MT-201 deploys four stabilizer legs. / MT-101's drill is a forward nose; MT-201's drill is a towering central installation. / MT-101 preserves a mobile horizontal profile; MT-201 changes to a huge vertical siege silhouette.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Massive central drill/cockpit installation — dominates both configurations — SOURCE_VERIFIED.
  - Four independent articulated leg pods — unfold into the planted siege stance — SOURCE_VERIFIED.
  - Rotating machinery core — connects travel body and drill state without hiding the transformation — SOURCE_VERIFIED.
- Structural load path: Four ball-jointed leg pods attach around the central machinery core and brace the longitudinal drill through four broad feet.
- Repeated modules / connection grammar: Central drill body, rotating core, four repeated leg pods and forward observation/tool section.
- Source-faithful versus adapted boundary: The manual hand-positions legs; production must create a continuous deployment sequence but may not invent a different leg count or separate observation unit.

## E. Material and texture contract

- Geometry must carry:
  - enormous central drill
  - four articulated leg pods
  - rotating central machinery
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Walker travel; immobile when deployed for siege.
- Planted/contact rule: Travel alternates four feet; siege locks all four before drill contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_CoreRotation` | Asset_MT201 | travel orientation to siege orientation | authoritative deployment progress |
| `Pivot_LegFrontLeft` | Pivot_CoreRotation | folded to planted ball-joint arc | deployment and gait |
| `Pivot_LegFrontRight` | Pivot_CoreRotation | mirrored fold/plant arc | deployment and gait |
| `Pivot_DrillSpin` | Asset_MT201 | roll around tool axis | authoritative siege attack progress |

- Required beats:
  - Idle travel stance.
  - Walker movement uses stable four-foot sequence.
  - Deploy rotates core and locks four feet before readiness.
  - Siege spins only after lock; destruction collapses legs around the drill.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_FootFrontLeft`, `Socket_FootFrontRight`, `Socket_FootRearLeft`, `Socket_FootRearRight`, `Socket_DrillContact`, `Socket_AudioDeploy`, `Socket_AudioDrill`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - A continuous four-leg deployment blockout must prove no visible disassembly.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
