# Razor Skimmer — T082 Super Scout packet

**Stable ID:** `unit.aliens.razor_skimmer`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Unit`
- Gameplay role: Ground-hover harassment
- Authoritative footprint: `Small`
- Source classification: `COMPOSITE-ADAPTATION`
- Approved source sets/motifs: 7645, 7692, 7697
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
| 7645 — MT-61 Crystal Reaper | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7645)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534846.pdf)<br>[official PDF 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4549395.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7645-1) | PRIMARY_VERIFIED | harvesting blades, mining modules and small alien craft |
| 7692 — MX-71 Recon Dropship | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7692)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7692-1) | PRIMARY_VERIFIED | dropship cargo cradle and small alien attack craft |
| 7697 — MT-51 Claw-Tank Ambush | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7697)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4515381.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7697-1) | PRIMARY_VERIFIED | tracked claw tank and small alien craft |

### Source audit [Aliens:7645]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages book 1, 3-26: Complete low Alien attack craft: long twin-pronged nose, four swept blade modules, lime cockpit/energy masses, rear engines and exposed crew deck.
  - PDF pages book 1, 27-75; book 2, 2-63: Astronaut mining assemblies and powered Crystal Reaper; supporting opposition evidence, not Razor Skimmer geometry.
- View/mechanism coverage: front=VERIFIED book 1 p18-26; rear=VERIFIED book 1 p20-26; leftRight=VERIFIED book 1 p3-26; top=VERIFIED book 1 p3-26; threeQuarter=VERIFIED cover and book 1 p20-26; undersideInterior=VERIFIED book 1 p3-19 staged frame; mechanism=PARTIAL book 1 p20-26 removable crew and projectile mounts; no hover cycle
- Verified findings:
  - The source craft is a low, long black skimmer with a split spear-like nose rather than a compact disc.
  - Four independently mounted swept black blades flare around a narrow central body and preserve large negative spaces between them.
  - Lime cockpit/energy cylinders, gray rear thrusters and an exposed red rear crew deck remain separate readable masses.
- Remaining evidence gaps:
  - Razor Skimmer is a composite family, so the retained 7645 nose, blade count and crew-deck features must be chosen against the other six mapped small craft rather than copied wholesale.

### Source audit [Aliens:7692]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 3-12: Astronaut payload rover; supporting opposition evidence.
  - PDF pages 13-23: Complete narrow Alien attack craft with tall paired curved blades, open center, lime conduits and triple rear emitter cluster.
  - PDF pages 24-71: Astronaut dropship and visible cargo release; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p20-23; rear=VERIFIED p20-23; leftRight=VERIFIED p13-23; top=VERIFIED p13-23; threeQuarter=VERIFIED cover and p20-23; undersideInterior=VERIFIED p13-20 staged open frame; mechanism=PARTIAL p20-23 hose and projectile mounts; no hover or deployment sequence
- Verified findings:
  - This small craft is unusually narrow and tall, with two near-vertical curved black blades enclosing a large central slot.
  - Three lime rear emitters form a compact triangular cluster while a separate side weapon and flexible conduit remain exposed.
  - Its vertical blade read is distinct from both the flat 5617 Jet and the broad crescent craft of 7690/7697.
- Remaining evidence gaps:
  - The composite Razor Skimmer and Defense Node must decide whether this vertical twin-blade source becomes a variant, a deployed hardpoint motif or is excluded from the shared hero silhouette.

### Source audit [Aliens:7697]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Construction map:
  - PDF pages 3-15: Complete low Alien ambush craft with three curved black lobes, exposed pilot, arched lime conduits and distributed emitters.
  - PDF pages 16-68: Astronaut MT-51 Claw-Tank; supporting opposition evidence.
- View/mechanism coverage: front=PARTIAL p12-15; rear=PARTIAL p12-15; leftRight=VERIFIED p3-15; top=VERIFIED p3-15; threeQuarter=VERIFIED cover and p12-15; undersideInterior=VERIFIED p3-12 staged frame; mechanism=PARTIAL p12-15 flexible conduits and projectile mounts; no flight or ambush sequence
- Verified findings:
  - The ambush craft forms a low broken ring from three rounded black lobes around an open operator cavity.
  - Two lime conduits arch over the exposed pilot, and the widest outer lobe carries separated yellow emitters rather than one central nose gun.
  - Its flat three-lobe footprint distinguishes it from 7692's tall twin-blade craft and 7645's long spear form.
- Remaining evidence gaps:
  - The composite Razor/Defense family must determine which lobe, emitter and open-pilot features remain shared identity and which stay source-specific variants.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

## C. Recognition contract

**Silhouette thesis:** A ground-hugging black-lime blade craft with a forward crystal core and almost no vertical body.

Non-removable identity anchors:

- very low swept planform
- twin forward razor prongs
- central exposed lime energy core

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `unit.aliens.alien_jet` — Both are small swept black-lime attack craft. Mitigations: Alien Jet has a visible airborne swept-plate profile; Razor Skimmer stays almost flat against the ground. / Alien Jet raises two bright conduit arches over an open pilot; Razor Skimmer projects two long forward razor prongs. / Alien Jet centers on its pilot; Razor Skimmer centers on an exposed lime energy core.
- `unit.martians.jet_scooter` — Both are small fast hover harassment units. Mitigations: Razor Skimmer is a broad black blade plan; Jet Scooter is a long thin open sled around its rider. / Razor centers on a lime core between two large prongs; Jet Scooter points a cluster of small orange nozzles ahead of paired side tubes. / Razor's propulsion is visually integrated into the hull; Jet Scooter leaves its tubes, deck and rear equipment exposed.

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
