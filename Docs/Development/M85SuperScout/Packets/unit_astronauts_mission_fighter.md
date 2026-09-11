# Mission Fighter — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mission_fighter`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Air superiority
- Authoritative footprint: `Small`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 5619, 7695
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
| 5619 — Crystal Hawk | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/5619)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4533843.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=5619-1) | PRIMARY_VERIFIED | small astronaut interceptor |
| 7695 — MX-11 Astro Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7695)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517774.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7695-1) | PRIMARY_VERIFIED | astronaut fighter wing and defense hardpoint language |

### Source audit [Astronauts:5619]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4533843.pdf)
- Construction map:
  - Evidence pages 1: Complete five-step Crystal Hawk build and final operator-scale three-quarter view.
  - Evidence pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 final view; rear=PARTIAL p1 construction sequence; leftRight=PARTIAL p1 mirrored wing build; top=VERIFIED p1 staged wing placement; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 exposed plate sequence; mechanism=MISSING static micro-build only
- Verified findings:
  - The fighter is an extremely compact open-seat craft built around a narrow black central spine.
  - Two broad white swept wings and paired cyan nose emitters carry more silhouette weight than the tiny cockpit.
  - Orange grille accents repeat at the wing roots and above the rear seat.
- Remaining evidence gaps:
  - Acquire a clean orthogonal rear or underside image before fixing propulsion and landing details for the Mission Fighter family.

### Source audit [Astronauts:7695]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517774.pdf)
- Construction map:
  - Evidence pages 2-13: Complete MX-11 Astro Fighter build: flat wing plate, orange canopy nose, tail/antenna and pilot scale.
  - Evidence pages 14-24: Inventory and promotional pages; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p9-13; rear=PARTIAL p10-13; leftRight=VERIFIED p2-13; top=VERIFIED p2-13; threeQuarter=VERIFIED p1 and p11-13; undersideInterior=VERIFIED p2-9 staged plate build; mechanism=MISSING static micro-fighter
- Verified findings:
  - MX-11 is a thin white delta-wing craft with a sharp orange canopy/nose at its center.
  - The entire fighter stays close to one plate thickness, separating it from bulkier mission aircraft.
  - A small dark rear equipment block and antenna provide the only raised mass behind the pilot.
- Remaining evidence gaps:
  - Clean underside and propulsion views are still required before consolidating MX-11 with the Crystal Hawk into one Mission Fighter family.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A clean white-orange interceptor with a compact cockpit and unmistakable swept mission wings.

Non-removable identity anchors:

- sharp swept wing pair
- small central blue canopy
- white-orange nose and engine split

- Rejected V1 blind-review code: `S40`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mono_jet` — Both are small true-air Astronaut craft. Mitigations: Mono Jet is a narrow improvised field fuselage; Mission Fighter has a crisp swept mission-wing plan. / Mono Jet has an open cockpit; Mission Fighter uses a compact enclosed blue canopy. / Mono Jet remains Field white/gray/blue; Mission Fighter uses strong white/orange Mission blocks.
- `unit.astronauts.mx41_switch_fighter` — Both use the white-orange Mission aerospace language. Mitigations: Mission Fighter is always airborne with two swept wings; MX-41 has a six-wheel ground stance. / Mission Fighter keeps one compact flight silhouette; MX-41 exposes oversized folding side frames. / Mission Fighter has no visible transformation seam; MX-41's central rocket nose and wing-wheel hinge must read in both states.
- `unit.aliens.alien_jet` — The draft sheet reduces both small interceptors to a swept central fuselage and paired wings. Mitigations: Mission Fighter uses a closed compact canopy; Alien Jet leaves the pilot open beneath two raised arches. / Mission Fighter's wings form clean rearward wedges; Alien Jet uses one thin irregular swept plate. / Mission Fighter has a solid pointed nose; Alien Jet's arches create a persistent central negative space.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Sharp swept wing pair — defines conventional Mission interception — SOURCE_VERIFIED.
  - Small central blue canopy — keeps pilot scale readable — SOURCE_VERIFIED.
  - White-orange nose and rear engine split — unifies Crystal Hawk and MX-11 without averaging their plans — CANON_DERIVED_ADAPTATION.
- Structural load path: A shallow central keel carries the canopy, mirrored wings and compact rear engine block.
- Repeated modules / connection grammar: One shared combat rig supports two approved source-faithful body variants with identical sockets.
- Source-faithful versus adapted boundary: 5619 and 7695 remain selectable visual family members; neither is blended into a third invented fighter.

## E. Material and texture contract

- Geometry must carry:
  - swept wing pair
  - small central canopy
  - white-orange nose/engine split
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ast_mission_shell_surface` — Very subtle clean-shell roughness variation for white-orange Mission Systems hulls without weathering them into Raider machinery. Channels: Tangent-space normal and linear roughness; no photographic albedo or baked highlights. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat with continuous phase across large shells.; LOD fallback: Normal removed at Strategic; clean master-material blocks remain. Provenance/state: Project-authored procedural source informed by verified Mission Systems panels; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_service_insignia_decals` — Expedition insignia, docking marks, module IDs, service arrows and broad safety bands. Channels: sRGB RGBA decal atlas; alpha is coverage only. Resolution: 1024x1024; texel density: Minimum 256 px/m for Close/Combat readable marks; tiling: Atlas placement; only broad authored bands may repeat.; LOD fallback: Keep insignia and large docking bands at Combat; remove labels at Strategic. Provenance/state: Project-authored vector master exported to raster; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ast_console_signal_atlas` — Navigation, scan, refit, extraction and flight-operation displays plus bounded status lights. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 1024x1024; texel density: Screen-space atlas; not world-density bound.; tiling: Non-tiling stable panel IDs shared by Field and Mission variants.; LOD fallback: Replace panels with one bounded Signal or Lamp block at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Fast true-air interception with crisp banking.
- Planted/contact rule: No normal ground contact; landing remains a production-pad presentation only.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_WingLeft` | Asset_MissionFighter | small aerodynamic load flex | turn intensity |
| `Pivot_WingRight` | Asset_MissionFighter | mirrored load flex | turn intensity |

- Required beats:
  - Idle patrol loiter.
  - Travel uses clean fast banking.
  - Attack aligns nose and fires a bounded burst.
  - Damage trails from one engine; destruction breaks wing then keel.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_MuzzleLeft`, `Socket_MuzzleRight`, `Socket_Engine`, `Socket_AudioFlight`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Director review must confirm whether the two source variants alternate freely or are player-selected cosmetics.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Replace the rejected 0/66 primitive silhouette with the source-derived method after Pilot V2 review, then run a new 24/44/72-cell blind review.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
