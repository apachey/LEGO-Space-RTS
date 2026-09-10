# ETX Fabricator — T082 Super Scout packet

**Stable ID:** `building.ali.etx_fabricator`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Infrastructure`
- Gameplay role: Light production
- Authoritative footprint: `Large`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 7691, 7690
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
| 7690 — MB-01 Eagle Command Base | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7690)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7690-1) | PRIMARY_VERIFIED | human command base, transfer system and service architecture |

### Source audit [Aliens:7691]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf)
- Construction map:
  - Evidence pages 3-27: Astronaut extraction station; supporting opposition evidence.
  - Evidence pages 28-49: Large circular Mothership carrier hull, open central machinery channel, lime conduits, docking hardpoints and long multi-blade tail.
  - Evidence pages 50-56: Two mirrored narrow seated weapon craft are built as independent modules for the carrier.
  - Evidence pages 57-63: A long lime-conduit front/central craft is assembled independently and docked into the carrier's open machinery channel.
  - Evidence pages 64-67: Two disc-like alien jetpack modules are built separately, accept individual aliens and dock at the carrier's outer wing/arm ends.
- View/mechanism coverage: front=VERIFIED p43-49; rear=VERIFIED p44-49; leftRight=VERIFIED p28-49; top=VERIFIED p28-49; threeQuarter=VERIFIED cover and p43-50; undersideInterior=VERIFIED p28-48 staged circular frame; interior remains open rather than enclosed; mechanism=VERIFIED p50-67 detachable subcraft and capture/weapon pods; carrier launch cycle remains partial
- Verified findings:
  - The Mothership is one huge flattened circular black carrier interrupted by a visible central machinery channel rather than a sealed saucer.
  - Several long black and translucent-lime tail blades extend from one side, preventing a rotationally symmetric disc silhouette.
  - Its separately assembled front craft, two seated side craft and two jetpack modules are contained/docked parts of the flagship presentation: they establish how the single Mothership can open, unfold and expose internal craft bays rather than defining separate Mothership entities.
- Remaining evidence gaps:
  - The manual proves docked subcraft but not the game's launch/recovery timing, unfolding sequence, reinforcement function or Charge-support state; all require a single-unit carrier contract.
  - Training eligible Alien units inside the Mothership is a game-director proposal. The exact roster, cost, build time, capacity interaction and whether production requires an unfolded state remain unresolved gameplay decisions and are not locked by T082.

### Source audit [Aliens:7690]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf), [official instruction PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf)
- Construction map:
  - Evidence pages book 1, 3-15: Complete compact Alien scout craft with paired curved outer lobes, central open pilot/tool frame, lime arches and long rear fin.
  - Evidence pages book 1, 16-75; book 2, 2-72: Astronaut command base, gantry and shuttle; supporting containment/docking opposition evidence, not small-craft geometry.
- View/mechanism coverage: front=VERIFIED book 1 p12-15; rear=PARTIAL book 1 p10-15; leftRight=VERIFIED book 1 p3-15; top=VERIFIED book 1 p3-15; threeQuarter=VERIFIED cover and book 1 p12-15; undersideInterior=VERIFIED book 1 p3-12 staged open frame; mechanism=PARTIAL book 1 p12-15 flexible conduits and weapon mounts; no locomotion sequence
- Verified findings:
  - The scout uses two broad curved black lobes around an exposed central operator/tool frame, producing a broken crescent silhouette.
  - Paired lime conduits arch forward over the center while several small yellow emitters remain distributed across the lobes.
  - One long lime rear fin gives the otherwise round craft a strong directional tail.
- Remaining evidence gaps:
  - The source does not establish whether the consolidated Razor/Servitor family retains the tail fin, distributed emitters or central exposed operator, so those choices must be resolved in the cross-source construction map.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A low docked alien hull that assembles small craft along an open forward launch channel.

Non-removable identity anchors:

- curved docked-hull shell
- open forward fabrication channel
- overhead crystal feed arms

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `building.ali.reconfiguration_dock` — Both are craft-derived Alien production structures. Mitigations: Fabricator is low and forward-open; Reconfiguration Dock is huge and vertically split. / Fabricator moves small craft along one channel; Reconfiguration Dock suspends a transforming craft in a central cradle. / Fabricator uses compact feed arms; Reconfiguration Dock exposes multiple large hinge arcs.

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
