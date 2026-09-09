# Alien Mothership — T082 Super Scout packet

**Stable ID:** `unit.aliens.alien_mothership`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Strategic carrier / support
- Authoritative footprint: `Huge`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7691
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
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |

### Source audit [Aliens:7691]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 3-27: Astronaut extraction station; supporting opposition evidence.
  - PDF pages 28-49: Large circular Mothership hull, open central machinery channel, lime conduits, weapon hardpoints and long multi-blade tail.
  - PDF pages 50-67: Three detachable small-craft modules: a central lime-conduit craft and two round-disc weapon/capture pods with independent riders.
- View/mechanism coverage: front=VERIFIED p43-49; rear=VERIFIED p44-49; leftRight=VERIFIED p28-49; top=VERIFIED p28-49; threeQuarter=VERIFIED cover and p43-50; undersideInterior=VERIFIED p28-48 staged circular frame; interior remains open rather than enclosed; mechanism=VERIFIED p50-67 detachable subcraft and capture/weapon pods; carrier launch cycle remains partial
- Verified findings:
  - The Mothership is a huge flattened circular black hull interrupted by a visible central machinery channel rather than a sealed saucer.
  - Several long black and translucent-lime tail blades extend from one side, preventing a rotationally symmetric disc silhouette.
  - Paired lime conduits route across the open center, while three independently readable small craft establish the carrier language outside the main hull.
- Remaining evidence gaps:
  - The manual proves detachable subcraft but not the game's payload capacity, launch/recovery timing, reinforcement function or Charge-support state; all require a carrier-specific contract.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A vast flattened circular black-lime carrier broken by an open machinery channel and a long multi-blade tail.

Non-removable identity anchors:

- huge interrupted circular hull
- open lime-conduit machinery channel
- directional tail and detachable subcraft

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `building.ali.etx_command_core` — The Command Core is intentionally Mothership-derived. Mitigations: Mothership keeps a huge interrupted circular airborne hull and long tail; Command Core is a compact grounded segment with support legs. / Mothership carries detachable attack modules; Command Core replaces them with entrances and fixed service interfaces. / Mothership's machinery channel stays low and horizontal; Command Core raises and protects its core as a command landmark.

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
