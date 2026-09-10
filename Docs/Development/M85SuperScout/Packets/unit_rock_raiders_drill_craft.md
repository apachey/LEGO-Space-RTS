# Drill Craft — T082 Super Scout packet

**Stable ID:** `unit.rock_raiders.drill_craft`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `RockRaiders`
- Kind: `Unit`
- Gameplay role: Engineering utility
- Authoritative footprint: `Small`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 1277
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
| 1277 — Drill Craft / Hovercraft with Ice Saws | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/1277)<br>no direct official PDF located<br>[archival evidence 1](https://kb.rockraidersunited.com/images/2/2f/1277_Hovercraft_with_Ice_Saws.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=1277-1) | CANON_VERIFIED_ARCHIVAL | low open hovercraft construction, two small mirrored ice-saw tools, side lift/engine pods and exposed operator relationship |

### Source audit [RockRaiders:1277]

- Evidence state: `ARCHIVAL_PDF_VISUALLY_AUDITED`
- Evidence links: [Archival scan hosted by Rock Raiders United; the pages themselves carry LEGO branding, set number 1277, product code 4132646 and a 1999 LEGO Group copyright notice, but the PDF is not served from LEGO's current archive. 1](https://kb.rockraidersunited.com/images/2/2f/1277_Hovercraft_with_Ice_Saws.pdf)
- Construction map:
  - Evidence pages 1: Complete flat base, paired yellow side wedges, teal side/rear fittings, central hazard panel and exposed Sparks operator are assembled.
  - Evidence pages 2: Two raised round side hover/engine pods and two forward ice-saw arms attach to the low open craft; the final model is shown from the front-left three-quarter view.
- View/mechanism coverage: front=VERIFIED cover and p2 step 6; rear=PARTIAL p1-2 construction sequence; leftRight=PARTIAL cover and p1-2; top=VERIFIED p1-2 steps 1-6; threeQuarter=VERIFIED cover and p2 final; undersideInterior=PARTIAL p1 bare plate foundation; mechanism=PARTIAL p2 saw and side-pod attachment; no spin, hover or steering sequence
- Verified findings:
  - The complete source is a very low open hovercraft/sled with an exposed operator, not a wheeled miniature drill vehicle.
  - Two forward ice saws are separate mirrored tool arms; the source does not contain one central helical drill.
  - Two raised round side pods and the central hazard panel carry more source identity than any rear bodywork.
- Remaining evidence gaps:
  - The opposite side and strict underside remain unverified.
  - The exact in-game contact pose for both small saws must preserve their mirrored source relationship while keeping excavation feedback readable at Strategic zoom.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tiny low open hover sled identified by two small mirrored forward ice saws and paired raised side lift pods around the exposed operator.

Non-removable identity anchors:

- two small mirrored forward ice saws
- flat open operator sled
- paired raised round side lift pods

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.
- Forbidden genericization: Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.
- Nearest-confusion baseline:

- `unit.rock_raiders.granite_grinder` — Both are short-range Raider excavation machines. Mitigations: Drill Craft stays low on an open hover sled; Granite Grinder is a tall planted biped. / Drill Craft frames its operator between paired raised side pods; Granite Grinder places the operator high above two large feet. / Drill Craft keeps two small mirrored saw discs; Granite Grinder carries one long drill boom balanced by rear machinery.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Flat open hover sled — carries the operator and tool on one shallow plate-built spine — SOURCE_VERIFIED.
  - Paired raised round side pods — provide the source's dominant lift/engine masses and frame the operator — SOURCE_VERIFIED.
  - Central hazard panel and exposed Sparks position — separate the operator deck from the forward tool package — SOURCE_VERIFIED.
  - Two small mirrored forward ice saws — preserve the source's defining tool silhouette while jointly performing canonical excavation and short-range structure contact — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
- Structural load path: One shallow plate-built spine carries the open operator deck, hazard panel and two vertical side-pod mounts; two short mirrored front arms transfer the paired saw contact load into that spine.
- Repeated modules / connection grammar: Flat sled, mirrored side lift/engine pods, central operator/hazard station and two small independently rotating saw arms.
- Source-faithful versus adapted boundary: The archival manual's hovercraft, two ice saws and lack of wheels are preserved. Canon adapts their cutting action to authored terrain excavation; a single replacement drill, wheel chassis, tank hull or invented rear counterweight is forbidden.

## E. Material and texture contract

- Geometry must carry:
  - two small mirrored saw discs and separate support arms
  - flat open operator sled and hazard panel
  - paired raised round side pods
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Lamp`, `Neutral`.
- Reusable texture requirements:
  - `rr_tool_wear` — Directional scuff and cutting wear on drill, scoop, cutter and clamp contact surfaces only. Channels: Linear wear mask, tangent-space normal and roughness variation; no baked highlights. Resolution: 1024x1024; texel density: 512 px/m on localized tool UVs; tiling: Non-tiling trim/atlas regions aligned to the mechanical wear direction.; LOD fallback: Normal and fine mask removed at Strategic; Tool material and silhouette remain. Provenance/state: Project-authored procedural source informed by the official tool surfaces; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `rr_hazard_and_service_decals` — Hazard stripes, service arrows, bay limits, lift points and restrained equipment labels. Channels: sRGB RGBA decal atlas; alpha is coverage, never shadowing. Resolution: 1024x1024; texel density: Minimum 256 px/m on readable Close/Combat labels; tiling: Atlas placement only; stripes may repeat along authored straight runs without stretching.; LOD fallback: Keep only broad hazard bands at Combat; remove text and micro-labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Low ground-layer hover movement derived from the source craft; quick steering with restrained sled heave and no true-air banking.
- Planted/contact rule: The sled settles to one low hover datum before excavation; both saws establish mirrored hard work contacts while the paired side pods stabilize the chassis visually.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_SawFeedLeft` | Asset_DrillCraft | short forward/downward contact arc; raised travel rest | authoritative excavation/contact state |
| `Pivot_SawFeedRight` | Asset_DrillCraft | mirrored short forward/downward contact arc; raised travel rest | authoritative excavation/contact state |
| `Pivot_SawSpinLeft` | Pivot_SawFeedLeft | continuous roll around saw axle; stopped rest | excavation or contact-attack presentation progress |
| `Pivot_SawSpinRight` | Pivot_SawFeedRight | opposed continuous roll around saw axle; stopped rest | excavation or contact-attack presentation progress |

- Required beats:
  - Idle hover settles around the shallow source sled with both saws stopped.
  - Move uses restrained side-pod vibration and small terrain-following heave, not wheel roll or aircraft banking.
  - Excavate settles, lowers both arms, spins the two saws in opposed directions, and raises them after the authoritative result.
  - Damage destabilizes one side pod and stops tool motion; the wreck preserves the open sled, paired pods and twin-saw relationship.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_SawContactLeft`, `Socket_SawContactRight`, `Socket_CutDust`, `Socket_CutSparks`, `Socket_HoverLeft`, `Socket_HoverRight`, `Socket_AudioSaws`, `Socket_AudioHover`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - The source manual does not prove whether the two raised round side pods rotate, glow or remain visually static. Keep them mechanically quiet until later motion evidence or director review selects a restrained presentation behavior.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
