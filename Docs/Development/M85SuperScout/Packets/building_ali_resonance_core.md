# Resonance Core — T082 Super Scout packet

**Stable ID:** `building.ali.resonance_core`

**Packet state:** `FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Aliens`
- Kind: `Infrastructure`
- Gameplay role: Crystal Charge
- Authoritative footprint: `Small`
- Source classification: `NEW GAME CONTENT`
- Approved source sets/motifs: 7691, 7646
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
| 7691 — ETX Alien Mothership Assault | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7691)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4516029.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7691-1) | PRIMARY_VERIFIED | alien mothership, detachable craft and human extraction station |
| 7646 — ETX Alien Infiltrator | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7646)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7646-1) | PRIMARY_VERIFIED | alien craft-to-walker transformation |

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

### Source audit [Aliens:7646]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4534848.pdf)
- Construction map:
  - Evidence pages 3-27: Astronaut equipment and figures; supporting opposition evidence.
  - Evidence pages 29-50: Long central Infiltrator craft with split nose, exposed crew/tool bay, red weapon tips and lime conduits.
  - Evidence pages 51-67: Two independent curved side modules attach to the central craft and receive weapons, cables and flexible lime conduits.
  - Evidence pages 68: Explicit conversion: both curved side modules and the long forward hull rotate downward into a planted three-leg walker.
- View/mechanism coverage: front=VERIFIED p49-50 and p67-68; rear=VERIFIED p47-50 and p67-68; leftRight=VERIFIED p29-68; top=VERIFIED p29-67; threeQuarter=VERIFIED cover and p49-50/p67-68; undersideInterior=VERIFIED p29-67 staged modules; mechanism=VERIFIED p68 craft-to-three-leg walker conversion; gait and weapon cycle remain partial
- Verified findings:
  - Infiltrator flight state is a long central two-seat craft flanked by two separately built crescent modules.
  - The walker has three planted members, not a generic four- or six-legged spider: the two curved side modules and the long split nose rotate downward.
  - Flexible lime conduits visibly link the crew/energy core to the moving side modules, making the transformation mechanical rather than magical.
- Remaining evidence gaps:
  - The source proves the large rotations but not a continuous gait, stable planted attack pose, heavy-target weapon path or detector sweep; those remain explicit production motion contracts.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A compact faceted crystal mechanism visibly clamped by three or four black alien arms.

Non-removable identity anchors:

- dominant exposed lime crystal
- radial black restraint claws
- concentric charge-ring mechanism

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.
- Forbidden genericization: Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.
- Nearest-confusion baseline:

- `building.ali.power_coupler` — Both are small black-lime energy structures. Mitigations: Resonance Core is dominated by a large faceted crystal; Power Coupler has a thin conductor. / Resonance Core uses radial restraint claws; Power Coupler uses two opposing arcs. / Resonance Core reads as storage/charge intensity; Power Coupler reads as directional energy transfer.

## D. Construction contract

- Contract state: `CANON_DERIVED_ADAPTATION`. This is an internally checked draft, not game-director approval.
- Semantic part map:
  - Dominant exposed lime crystal — makes committed Crystal state readable — CANON_DERIVED_ADAPTATION.
  - Three or four radial black restraint claws — physically contain the crystal — SOURCE_VERIFIED/CANON_DERIVED_ADAPTATION.
  - Concentric Charge-ring mechanism — communicates capacity and generation without text — CANON_DERIVED_ADAPTATION.
- Structural load path: A low planted ring supports radial claws that close around one faceted crystal while conduits route Charge outward.
- Repeated modules / connection grammar: Base ring, crystal, restraint claws, Charge rings and connection ports remain independently readable.
- Source-faithful versus adapted boundary: This is new content from containment and Mothership machinery; no floating unexplained monolith or biological growth.

## E. Material and texture contract

- Geometry must carry:
  - large faceted crystal
  - radial restraint claws
  - concentric Charge rings
- Accepted master-material roles: `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral`.
- Reusable texture requirements:
  - `ali_black_hull_surface` — Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. Channels: Tangent-space normal and linear roughness; black body color remains parametric. Resolution: 2048x2048; texel density: 256 px/m at Close; tiling: Shared model-space 4 m repeat across connected hull modules.; LOD fallback: Half strength at Combat; master roughness only at Strategic. Provenance/state: Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_lime_conduit_surface` — Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. Channels: Linear roughness and restrained emissive mask; geometry defines every conduit. Resolution: 1024x1024; texel density: 512 px/m on localized conduit UVs; tiling: Short trim regions aligned to conduit direction; no phase reset at joints.; LOD fallback: Collapse to one bounded lime Signal strip at Strategic. Provenance/state: Project-authored procedural/trim source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_crystal_containment_mask` — Facet-localized Charge intensity and containment contact masks without baked glow or fake crystal depth. Channels: Linear roughness, transmission control and separate emission mask. Resolution: 1024x1024; texel density: Object-local crystal atlas; not world-density bound.; tiling: Non-tiling per approved crystal form.; LOD fallback: One faceted Glass mass and bounded emission at Strategic. Provenance/state: Project-authored procedural crystal source; human review required. `SPECIFIED_NOT_AUTHORED`.
  - `ali_bay_state_signal_atlas` — Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. Channels: sRGB color/alpha with separate linear emission mask. Resolution: 512x512; texel density: Screen-space and trim atlas; not world-density bound.; tiling: Non-tiling stable interface IDs.; LOD fallback: One directional Signal block per active interface at Strategic. Provenance/state: Project-authored vector/procedural source; human review required. `SPECIFIED_NOT_AUTHORED`.
- Bespoke texture requirements: none required in this faction draft.
- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited.

## F. State and animation contract

- Locomotion / operation: Stationary structure.
- Planted/contact rule: Low radial base remains planted; crystal stays mechanically restrained.

| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |
|---|---|---|---|
| `Pivot_ContainmentClaws` | Asset_ResonanceCore | open construction rest to secured crystal stance | construction and committed-Crystal state |
| `Pivot_ChargeRing` | Asset_ResonanceCore | slow indexed rotation around crystal | Charge generation presentation |

- Required beats:
  - Construction seats crystal and closes claws.
  - Idle rings show bounded active state.
  - Commit/withdraw visibly changes contained intensity over time.
  - Damage releases ring segments before core failure.
- Animation consumes authoritative state and never decides gameplay timing or results.

## G. Presentation hookups

- Required presentation sockets: `Socket_Selection`, `Socket_Health`, `Socket_Crystal`, `Socket_Charge`, `Socket_Energy`, `Socket_AudioOperations`.
- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.
- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.
- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.
- Remaining source/design decisions:
  - Final claw count must be resolved by the silhouette board rather than decorative preference.
- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling.

## I. Build handoff

1. Retain the audited evidence and every explicit adaptation boundary.
2. Greybox hero masses, openings and structural load path from the semantic map.
3. Validate named pivots, contacts and sockets in the real gameplay camera.
4. Author only the specified reusable textures after human material review.
5. Produce 24/44/72-cell black silhouettes and run the full cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
