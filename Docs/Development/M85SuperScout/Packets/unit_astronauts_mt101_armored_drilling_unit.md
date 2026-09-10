# MT-101 Armored Drilling Unit — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mt101_armored_drilling_unit`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Heavy anti-heavy assault
- Authoritative footprint: `Large`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7699
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
| 7699 — MT-101 Armored Drilling Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7699)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7699-1) | PRIMARY_VERIFIED | six-wheel heavy drilling chassis |

### Source audit [Astronauts:7699]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf)
- Construction map:
  - Evidence pages book 1, 3-14: Small human wheeled support craft and alien scout; supporting module evidence.
  - Evidence pages book 1, 15-29: White/orange forward mission cockpit and equipment cylinders.
  - Evidence pages book 1, 30-47; book 2, 2-17: Long open suspended chassis, side rails and rear service bay.
  - Evidence pages book 2, 18-27: Six separately mounted huge orange wheels and completed heavy running gear.
  - Evidence pages book 2, 28-34: Forward shell and detachable support/tool components attach to the chassis.
  - Evidence pages book 2, 35-43: Elevated rotating drill carriage, long drill tool and explicit rotation/tool play evidence.
- View/mechanism coverage: front=VERIFIED book 2 p24-43; rear=VERIFIED book 2 p27-43; leftRight=VERIFIED both books; top=VERIFIED book 2 p2-43; threeQuarter=VERIFIED covers and book 2 p35-43; undersideInterior=VERIFIED book 1 p30-47 and book 2 p2-27; mechanism=VERIFIED book 2 p35-43 rotating drill carriage and movable rear module
- Verified findings:
  - MT-101 is a long open heavy chassis suspended between six individually mounted oversized orange wheels.
  - The armored white cockpit sits low at the front while the raised drilling carriage remains exposed above the central/rear frame.
  - The long drill rotates on its own elevated mount, preserving a machine-tool identity instead of becoming a conventional tank cannon.
- Remaining evidence gaps:
  - The spring-projectile play action is not the game's contact-drill behavior; final drill reach, impact pose and chassis suspension response require a production animation plan.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long six-wheel armored drill carrier whose suspended chassis frames a massive central nose tool.

Non-removable identity anchors:

- six large suspended wheels
- central armored drill nose
- long white-orange equipment deck

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mobile_mining_platform` — Both are large white-orange ground mining machines. Mitigations: Mining Platform exposes its material path and detachable processing module; MT-101 encloses the tool in an armored assault nose. / Mining Platform uses a broad harvesting head; MT-101 uses one central heavy drill. / Mining Platform reads as an equipment platform with cargo space; MT-101 reads as a long six-wheel breach chassis.
- `unit.astronauts.mt201_ultra_drill_walker` — Both are advanced heavy Mission drilling machines. Mitigations: MT-101 remains a long six-wheel vehicle; MT-201 deploys four stabilizer legs. / MT-101's drill is a forward nose; MT-201's drill is a towering central installation. / MT-101 preserves a mobile horizontal profile; MT-201 changes to a huge vertical siege silhouette.
- `unit.rock_raiders.chrome_crusher` — Both are large wheeled heavy drill assault machines. Mitigations: Chrome Crusher has four giant wheels; MT-101 has six suspended wheels. / Chrome Crusher exposes teal industrial work machinery; MT-101 uses a clean white-orange armored mission deck. / Chrome Crusher's drill shares the silhouette with a raised work light and cargo gear; MT-101's drill forms a central armored nose.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Six independently mounted orange wheels — carry the huge suspended chassis — SOURCE_VERIFIED.
  - Low armored forward cockpit — protects the operator behind the nose — SOURCE_VERIFIED.
  - Elevated rotating drill carriage — delivers the anti-heavy contact tool through an exposed machine path — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: Six wheel mounts support a long open chassis; the raised drill carriage transfers thrust into the central rails while rear equipment balances it.
- Repeated modules / connection grammar: Six wheel modules, forward cockpit, long chassis, drill carriage and rear service equipment.
- Source-faithful versus adapted boundary: Source projectile play does not define combat; the long drill establishes physical contact and cannot open authored terrain routes.

## E. Material and texture contract

- Geometry must carry:
  - six huge wheels
  - low armored cockpit
  - elevated long drill carriage
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Rubber`, `Glass`, `Signal`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Heavy six-wheel ground movement with visible suspension.
- Planted/contact rule: All wheels remain grounded; drill attack settles the chassis before carriage alignment.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_DrillYaw` | Asset_MT101 | source-aligned carriage rotation toward target | presentation aim |
| `Pivot_DrillSpin` | Pivot_DrillYaw | roll around tool axis | authoritative contact attack progress |
| `Pivot_SuspensionFront` | Asset_MT101 | bounded vertical response | terrain response |

- Required beats:
  - Idle carriage check.
  - Travel emphasizes six-wheel suspension.
  - Attack aligns, settles and spins at contact.
  - Damage disrupts rear equipment; wreck preserves wheels and drill carriage.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_DrillContact`, `Socket_DrillSparks`, `Socket_DrillDust`, `Socket_AudioDrive`, `Socket_AudioDrill`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final contact reach and suspension compression require production blockout.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
