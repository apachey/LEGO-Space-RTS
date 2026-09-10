# MT-51 Claw-Tank — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mt51_claw_tank`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Frontline anti-light / medium
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7697
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
| 7697 — MT-51 Claw-Tank Ambush | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7697)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4515381.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7697-1) | PRIMARY_VERIFIED | tracked claw tank and small alien craft |

### Source audit [Astronauts:7697]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4515381.pdf)
- Construction map:
  - Evidence pages 3-15: Alien ambush craft; supporting opposition evidence.
  - Evidence pages 16-36: Low rectangular Claw-Tank running-gear frame, twin tracked sides and central rotation mount.
  - Evidence pages 37-47: Circular upper body and orange transparent operator canopy.
  - Evidence pages 48-63: Long articulated tool/claw arms, side equipment and wheel/track details.
  - Evidence pages 64-68: Rotation and articulated-tool play evidence with final multi-angle views.
- View/mechanism coverage: front=VERIFIED p58-68; rear=VERIFIED p59-68; leftRight=VERIFIED p16-68; top=VERIFIED p16-68; threeQuarter=VERIFIED p1 and p63-68; undersideInterior=VERIFIED p16-47 staged base/turntable; mechanism=VERIFIED p58-68 rotating upper body and articulated tool arms
- Verified findings:
  - MT-51 is organized around a circular orange-canopy upper body rotating above a low, wide tracked frame.
  - Long independently articulated tool/claw arms radiate from the turret instead of forming a conventional forward gun.
  - The broad running gear stays visually separate from the rotating body, supporting movement and weapon orientation in different directions.
- Remaining evidence gaps:
  - The source demonstrates manual rotation and arm movement but not the game's exact multi-target attack cycle or fighting-retreat locomotion; animation timing remains open.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tracked mission vehicle with a rotating upper body and a huge articulated claw opposite its weapon arm.

Non-removable identity anchors:

- paired track blocks
- rotating central cockpit
- asymmetric claw and tool arms

- Blind-review code: `S13`. Draft boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). This is a concept silhouette, not an approved model.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `PENDING` — no nearest-neighbor pair has been assigned yet.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Paired track blocks — form the low stable movement base — SOURCE_VERIFIED.
  - Circular rotating cockpit body — separates aim direction from travel — SOURCE_VERIFIED.
  - Asymmetric articulated claw and tool arms — engage nearby light targets without resembling a tank cannon — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: Two track blocks support a central turntable; the circular upper body routes each long arm load through the rotating ring.
- Repeated modules / connection grammar: Mirrored tracks, rotating cockpit, claw arm and opposing tool/weapon arm.
- Source-faithful versus adapted boundary: Combat timing is adapted, but rotation and articulation follow the source mechanisms; no conventional turret barrel.

## E. Material and texture contract

- Geometry must carry:
  - paired tracks
  - round rotating cockpit
  - asymmetric long arms
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Tracked ground travel while the upper body may retain target orientation.
- Planted/contact rule: Both tracks remain grounded; arm strikes settle the turntable before contact.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_UpperYaw` | Asset_MT51 | continuous bounded target-facing yaw | presentation aim |
| `Pivot_ClawArm` | Pivot_UpperYaw | multi-joint reach and clamp arc | authoritative attack progress |
| `Pivot_ToolArm` | Pivot_UpperYaw | independent target reach arc | authoritative attack progress |

- Required beats:
  - Idle arms remain asymmetric.
  - Travel rolls tracks while upper body tracks target.
  - Attack alternates tool aim and claw contact.
  - Damage sags one arm; destruction drops upper body from tracks.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_ClawContact`, `Socket_Muzzle`, `Socket_TrackLeft`, `Socket_TrackRight`, `Socket_AudioDrive`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Exact multi-target arm cadence waits for animation and readability review.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Run the game-director blind review on the 24/44/72-cell silhouette draft and revise any failed distinction.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
