# MX-41 Switch Fighter — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mx41_switch_fighter`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Ground / air transformer
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7647
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
| 7647 — MX-41 Switch Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7647)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7647-1) | PRIMARY_VERIFIED | six-wheel ground-to-flight transformation |

### Source audit [Astronauts:7647]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4525547.pdf)
- Construction map:
  - Evidence pages 3-14: Small alien attack craft; supporting opposition evidence.
  - Evidence pages 15-42: Long orange-canopy Switch Fighter hull and layered side shell.
  - Evidence pages 43-58: Separate full-span folding wing/chassis slab with six wheel mounts.
  - Evidence pages 59-64: Explicit conversion: wing tips fold, slab docks under the hull, six wheels attach, then the wing unfolds for flight.
- View/mechanism coverage: front=VERIFIED p37-64; rear=VERIFIED p41-64; leftRight=VERIFIED p15-64; top=VERIFIED p15-64; threeQuarter=VERIFIED p1 and p60-70; undersideInterior=VERIFIED p43-60 separate chassis/wing slab; mechanism=VERIFIED p58-64 physical ground-to-flight conversion
- Verified findings:
  - The ground vehicle and fighter are the same long cockpit hull carried by a separate folding wing/chassis slab.
  - Six orange wheels remain fully visible beneath the slab in ground state.
  - Transformation is readable through large wing-tip rotations and docking/undocking of the hull, not a cosmetic effect.
- Remaining evidence gaps:
  - The manual demonstrates a hand-separated hull during conversion; the production animation must define a believable continuous connection without changing the canonical two-state read.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A six-wheel surface machine whose side frames visibly fold into a pointed flight configuration.

Non-removable identity anchors:

- six-wheel ground stance
- large folding wing-side assemblies
- central white-orange rocket nose

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mission_fighter` — Both use the white-orange Mission aerospace language. Mitigations: Mission Fighter is always airborne with two swept wings; MX-41 has a six-wheel ground stance. / Mission Fighter keeps one compact flight silhouette; MX-41 exposes oversized folding side frames. / Mission Fighter has no visible transformation seam; MX-41's central rocket nose and wing-wheel hinge must read in both states.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Six-wheel ground chassis — creates the pursuit stance — SOURCE_VERIFIED.
  - Two large folding wing-side assemblies — perform the ground-to-flight conversion — SOURCE_VERIFIED.
  - Central white-orange rocket nose — remains the common directional core in both states — SOURCE_VERIFIED.
- Structural load path: The central nose/chassis spine carries three wheel pairs and two hinged side frames whose loads stay visibly connected through transformation.
- Repeated modules / connection grammar: Central cockpit/nose, mirrored wing-wheel frames and rear propulsion remain one transforming machine.
- Source-faithful versus adapted boundary: The manual proves two configurations but briefly hand-separates the cockpit hull; game motion must supply a credible continuous connection rather than visible disassembly.

## E. Material and texture contract

- Geometry must carry:
  - six-wheel stance
  - large folding side frames
  - central rocket nose
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Six-wheel ground pursuit or true-air flight according to authoritative state.
- Planted/contact rule: Six wheels ground in Pursuit; no wheel contact in Flight.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WingFrameLeft` | Asset_MX41 | source-aligned ground-side to flight-wing fold | authoritative transformation progress |
| `Pivot_WingFrameRight` | Asset_MX41 | mirrored fold | authoritative transformation progress |
| `Pivot_NoseLink` | Asset_MX41 | continuous guided shift preserving hull connection | authoritative transformation progress |

- Required beats:
  - Idle appropriate to current state.
  - Ground travel shows six wheel rotations; flight banks.
  - Transformation visibly locks each frame before state completion.
  - Damage interrupts presentation but not authoritative timing; wreck preserves transformation seams.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Weapon`, `Socket_Muzzle`, `Socket_EngineLeft`, `Socket_EngineRight`, `Socket_AudioTransform`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Continuous nose linkage requires blockout approval because the source conversion uses hand separation.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
