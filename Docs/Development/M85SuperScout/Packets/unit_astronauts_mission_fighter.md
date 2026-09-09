# Mission Fighter — T082 Super Scout packet

**Stable ID:** `unit.astronauts.mission_fighter`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Astronauts`
- Kind: `Unit`
- Gameplay role: Air superiority
- Authoritative footprint: `Small`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 5619, 7695
- Current confidence: verified canonical identity; construction confidence remains bounded by the source verification shown below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: Source-view coverage and construction-critical page ranges are recorded for the audited sources below. Asset-specific adaptation boundaries still must be resolved before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 5619 — Crystal Hawk | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/5619)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4533843.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=5619-1) | PRIMARY_VERIFIED | small astronaut interceptor |
| 7695 — MX-11 Astro Fighter | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7695)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517774.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7695-1) | PRIMARY_VERIFIED | astronaut fighter wing and defense hardpoint language |

### Source audit [Astronauts:5619]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 1: Complete five-step Crystal Hawk build and final operator-scale three-quarter view.
  - PDF pages 2: Promotional reverse page; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p1 final view; rear=PARTIAL p1 construction sequence; leftRight=PARTIAL p1 mirrored wing build; top=VERIFIED p1 staged wing placement; threeQuarter=VERIFIED p1 cover and final step; undersideInterior=PARTIAL p1 exposed plate sequence; mechanism=MISSING static micro-build only
- Verified findings:
  - The fighter is an extremely compact open-seat craft built around a narrow black central spine.
  - Two broad white swept wings and paired cyan nose emitters carry more silhouette weight than the tiny cockpit.
  - Orange grille accents repeat at the wing roots and above the rear seat.
- Remaining evidence gaps:
  - Acquire a clean orthogonal rear or underside image before fixing propulsion and landing details for the Mission Fighter family.

### Source audit [Astronauts:7695]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 2-13: Complete MX-11 Astro Fighter build: flat wing plate, orange canopy nose, tail/antenna and pilot scale.
  - PDF pages 14-24: Inventory and promotional pages; no additional construction evidence.
- View/mechanism coverage: front=PARTIAL p9-13; rear=PARTIAL p10-13; leftRight=VERIFIED p2-13; top=VERIFIED p2-13; threeQuarter=VERIFIED p1 and p11-13; undersideInterior=VERIFIED p2-9 staged plate build; mechanism=MISSING static micro-fighter
- Verified findings:
  - MX-11 is a thin white delta-wing craft with a sharp orange canopy/nose at its center.
  - The entire fighter stays close to one plate thickness, separating it from bulkier mission aircraft.
  - A small dark rear equipment block and antenna provide the only raised mass behind the pilot.
- Remaining evidence gaps:
  - Clean underside and propulsion views are still required before consolidating MX-11 with the Crystal Hawk into one Mission Fighter family.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A clean white-orange interceptor with a compact cockpit and unmistakable swept mission wings.

Non-removable identity anchors:

- sharp swept wing pair
- small central blue canopy
- white-orange nose and engine split

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.
- Forbidden genericization: Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.
- Nearest-confusion baseline:

- `unit.astronauts.mono_jet` — Both are small true-air Astronaut craft. Mitigations: Mono Jet is a narrow improvised field fuselage; Mission Fighter has a crisp swept mission-wing plan. / Mono Jet has an open cockpit; Mission Fighter uses a compact enclosed blue canopy. / Mono Jet remains Field white/gray/blue; Mission Fighter uses strong white/orange Mission blocks.
- `unit.astronauts.mx41_switch_fighter` — Both use the white-orange Mission aerospace language. Mitigations: Mission Fighter is always airborne with two swept wings; MX-41 has a six-wheel ground stance. / Mission Fighter keeps one compact flight silhouette; MX-41 exposes oversized folding side frames. / Mission Fighter has no visible transformation seam; MX-41's central rocket nose and wing-wheel hinge must read in both states.

## D. Construction contract

- Hero geometry must preserve every recognition anchor above.
- Support geometry must explain how hero masses connect, carry load and articulate.
- Micro geometry may enrich close view but may not become required for recognition.
- Exact chassis/load path, repeated modules, mounting logic, scale ratios and approved adaptations: `HOLD — SOURCE DECOMPOSITION REQUIRED`.

## E. Material and texture contract

- Silhouette, openings, major panel breaks, moving joints and LEGO connection logic remain geometry.
- Surface channels may carry controlled color masks, roughness, emission, decals and non-structural relief only.
- Required reusable and bespoke texture sets, resolution, tiling, texel density, LOD fallback and import settings: `HOLD — TEXTURE-NEEDS AUDIT REQUIRED`.
- Baked lighting, fake silhouette structure and illegible micro-noise are prohibited.

## F. State and animation contract

- Applicable idle, locomotion/operation, work, attack, production, repair, transform/deploy, disabled, damage and destruction beats: `HOLD — MECHANISM EVIDENCE REQUIRED`.
- Every moving assembly must receive a named pivot, parent, axis/path, rest/extreme poses and authoritative presentation driver.
- Animation may communicate gameplay state but never decide gameplay timing.

## G. Presentation hookups

- `Socket_Selection` and `Socket_Health` are mandatory.
- Tool, weapon, projectile, VFX, lamp and audio sockets follow only from verified function.
- Cargo, passenger, service, production-exit or network sockets apply where the canonical role requires them.
- Identification Tile, icon silhouette, portrait camera and reduced-presentation fallback: `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, faction, role, footprint, source classification and mapped source family.
- Canon-derived interpretation: silhouette thesis and identity anchors above.
- Unknown: exact multi-angle construction, articulation, material ratios, texture inventory and confusion mitigation until the remaining audits are complete.
- Consequential contradictions: none recorded at identity-baseline stage.

## I. Build handoff

1. Verify and cite the complete multi-angle source board.
2. Decompose primary masses and negative spaces from orthogonal evidence.
3. Resolve LEGO load path, connection grammar and moving mechanism.
4. Complete material/texture and state/animation contracts.
5. Produce 24/44/72-cell black silhouettes and run the cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
