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
| 7699 — MT-101 Armored Drilling Unit | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7699)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7699-1) | PRIMARY_VERIFIED | six-wheel heavy drilling chassis with permanent front cabin, directly docked rear spacecraft and its contained two-wheel mini-bike |

### Source audit [Astronauts:7699]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf)
- Construction map:
  - Evidence pages book 1, 3-14: Human two-wheel mini-bike (completed on p8) and separately built opposing Alien scout; distinguish contained human vehicle from excluded opponent.
  - Evidence pages book 1, 15-29: Detachable rear spacecraft with its own orange canopy, paired equipment cylinders and fins, completed on p29; this is not the permanent front cabin of the heavy chassis.
  - Evidence pages book 1, 30-47; book 2, 2-17: Long open suspended chassis, side rails and rear service bay.
  - Evidence pages book 2, 18-27: Six separately mounted huge orange wheels and completed heavy running gear.
  - Evidence pages book 2, 28-34: Forward shell and detachable support/tool components attach to the chassis.
  - Evidence pages book 2, 35-43: Permanent front cabin and independent upper launcher/drill; p43 explicitly separates the rear spacecraft from MT-101 and the mini-bike from the spacecraft.
- View/mechanism coverage: front=VERIFIED book 2 p24-43; rear=VERIFIED book 2 p27-43; leftRight=VERIFIED both books; top=VERIFIED book 2 p2-43; threeQuarter=VERIFIED covers and book 2 p35-43; undersideInterior=VERIFIED book 1 p30-47 and book 2 p2-27; mechanism=VERIFIED book 2 p35-43 rotating drill carriage and movable rear module
- Verified findings:
  - MT-101 is a long open heavy chassis suspended between six individually mounted broad hard-plastic orange barrel wheels.
  - The steep armored cockpit is a permanent part of the main vehicle at the front; detaching the rear spacecraft does not remove it.
  - Two large curved studded white side shells close above the central chassis and the docked rear module.
  - The rear spacecraft is an integrated detachable human module of MT-101, not an excluded unrelated support flyer.
  - The human two-wheel mini-bike enters the rear spacecraft from behind while the shells are open; book 2 p43 establishes hierarchy, not a literal upward extraction axis.
  - The drilling carriage stays exposed on a slender articulated boom above the forward cabin/frame; its black spiky tip, two star cutters and rear cup/collar remain distinct from the compact Bionicle Zamor sphere launcher.
  - The drill and launcher may not be merged into one conventional tank cannon.
- Source assembly scope: `OFFICIAL_MODEL_STRUCTURE_ONLY_NOT_RUNTIME_ROSTER`
  - MT101 — parent: none (root); ROOT_HEAVY_CHASSIS_WITH_PERMANENT_FRONT_CABIN; evidence: book 2, 35 and 43
  - RearSpacecraft — parent: MT101; DIRECTLY_DOCKED_DETACHABLE_MODULE_BELOW_CURVED_SHELLS; evidence: book 1, 15-29; book 2, 42-43
  - MiniBike — parent: RearSpacecraft; REAR_LOADED_CONTAINED_TWO_WHEEL_VEHICLE; evidence: book 1, 8; book 2, 42-43
- Excluded opponent: Separately built Alien scout, book 1, 9-14; never substitute it for a human module.
- Source/gameplay boundary: Independent spacecraft/bike commands, roles, costs and entity ownership are not specified by current MT-101 canon; do not infer them from toy separability.
- Remaining evidence gaps:
  - The spring-projectile play action is not the game's contact-drill behavior; final drill reach, impact pose and chassis suspension response require a production animation plan.
  - Source-backed mini-bike stowage and extraction clearances still need production construction validation; current images/native controls do not prove the complete nested assembly.
  - Independent rear-spacecraft and mini-bike gameplay remains a director canon decision, not an automatic extra roster assignment.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A long six-wheel armored drill carrier with a permanent steep forward cockpit, curved central shells, exposed articulated drill and compact Zamor launcher, plus a directly docked rear spacecraft containing a two-wheel mini-bike.

Non-removable identity anchors:

- six broad suspended barrel wheels
- permanent steep forward cockpit
- paired curved studded central shells
- exposed spiky drill with two star cutters and rear cup collar
- compact Bionicle Zamor sphere launcher separate from drill
- directly docked rear spacecraft with wedge cockpit and visible docking seam

- Rejected V1 blind-review code: `S10`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mobile_mining_platform` — Both are large white-orange ground mining machines. Mitigations: Mining Platform exposes its material path and detachable processing module; MT-101 closes paired curved shells over an open suspension chassis. / Mining Platform uses a broad harvesting head; MT-101 carries an exposed spiky two-disc drill on a slender forward boom. / Mining Platform reads as an equipment platform with cargo space; MT-101 reads as a long six-wheel breach chassis with a compact separate Zamor launcher.
- `unit.astronauts.mt201_ultra_drill_walker` — Both are advanced heavy Mission drilling machines. Mitigations: MT-101 remains a long six-wheel vehicle; MT-201 deploys four stabilizer legs. / MT-101's drill projects from an exposed slender boom above its forward cabin; MT-201's drill is a towering central installation. / MT-101 preserves a mobile horizontal profile; MT-201 changes to a huge vertical siege silhouette.
- `unit.rock_raiders.chrome_crusher` — Both are large wheeled heavy drill assault machines. Mitigations: Chrome Crusher has four giant wheels; MT-101 has six broad suspended barrel wheels. / Chrome Crusher exposes teal industrial work machinery; MT-101 closes paired white-orange curved shells over an open black chassis. / Chrome Crusher's drill shares the silhouette with a raised work light and cargo gear; MT-101 exposes a spiky two-disc drill on a separate slender boom beside a compact Zamor launcher.

## D. Construction contract

- Contract state: `SOURCE_VERIFIED`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Six independently mounted broad orange barrel wheels — carry the huge suspended chassis — SOURCE_VERIFIED.
  - Permanent steep armored forward cockpit — remains on the main chassis when the rear spacecraft detaches — SOURCE_VERIFIED.
  - Paired curved studded central shells — close above the open chassis and nested rear module — SOURCE_VERIFIED.
  - Exposed rotating drill carriage — slender articulated boom carries a spiky bit, two star cutters and rear cup/collar above the forward cabin/frame — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Compact Bionicle Zamor sphere launcher — stays visually and mechanically distinct from the drill — SOURCE_VERIFIED.
  - Rear spacecraft — directly docked detachable module belonging to MT-101, with wedge cockpit, paired equipment cylinders, low wings and two high fins — SOURCE_VERIFIED book 1 p29 / book 2 p42-43.
  - Two-wheel mini-bike — contained and rear-loaded into the rear spacecraft, not loose cargo on the main chassis or an omitted supporting build — SOURCE_VERIFIED book 1 p8 / book 2 p42-43.
- Structural load path: Six wheel mounts support a long open chassis; the permanent forward cockpit braces the nose while the raised drill carriage transfers thrust into the central rails. The directly docked rear spacecraft carries its own contained mini-bike and equipment cylinders.
- Repeated modules / connection grammar: Six wheel modules, permanent front cabin, long chassis, independent drill carriage and separate compact Bionicle Zamor sphere launcher; MT101 -> RearSpacecraft -> MiniBike. Rear spacecraft is directly docked; two-wheel mini-bike is contained inside it. Exclude only the opposing Alien scout, not either human module.
- Source-faithful versus adapted boundary: Preserve the complete source assembly and distinguish the permanent front cabin from the rear spacecraft cockpit. Toy detachability does not approve independent spacecraft/bike gameplay, commands, costs or extra roster entities; those remain a director canon decision. Source projectile play does not define combat; the contact drill cannot open authored terrain routes or fuse with the Zamor launcher.

## E. Material and texture contract

- Geometry must carry:
  - six broad barrel wheels
  - permanent steep front cabin
  - paired curved studded central shells
  - exposed spiky drill with two star cutters and rear cup collar
  - compact Bionicle Zamor sphere launcher separate from drill
  - directly docked rear spacecraft with wedge cockpit, side tubes, low wings and two high fins
  - two-wheel mini-bike stowed inside rear spacecraft with credible rear-loading clearance
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
  - Source-led rear spacecraft docking and contained mini-bike stowage/extraction clearances require validation before a complete-model claim; existing images and native construction are incomplete controls.
  - Independent rear-spacecraft and mini-bike operation, roles and entity/cost rules require director-approved gameplay canon; no deploy/extract command is implemented or authorized by this source correction.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
