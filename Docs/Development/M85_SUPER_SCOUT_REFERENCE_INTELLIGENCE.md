# M8.5 T082 — SUPER SCOUT REFERENCE INTELLIGENCE

**Status:** Approved execution specification for the Phase 09C preproduction
research gate.

## Mission

Build a complete, traceable visual-construction knowledge base for all 35 units
and 31 infrastructure entries before roster-wide concept art or 3D production.

The deliverable must answer, for every asset:

> What makes this exact unit or structure unmistakable, how could it credibly
> be constructed as a LEGO-derived machine, and how must its mechanism move so
> its function is legible from the real RTS camera?

Super Scout is not complete when many references have been collected. It is
complete when the references have been converted into buildable decisions,
comparative identity safeguards and clearly labeled unknowns.

## Non-negotiable outcomes

- A modeler can begin without inventing the asset's identity or mechanism.
- A rigger/animator can name the required pivots, contact points and motion
  phases before topology is finalized.
- A VFX/audio implementer can locate functional emitters and understand their
  cause.
- A UI artist can derive an icon silhouette and model portrait from the same
  identity anchors used in the world asset.
- The game director can compare the complete roster and detect duplicates,
  genericization or faction drift before expensive production begins.
- A later contributor can trace every material claim to a source and distinguish
  verified fact, approved adaptation and unresolved interpretation.
- The output cannot pass as generic AI-polished concept art: every major form
  has a known identity, connection, purpose and relationship to LEGO-derived
  construction.
- Texture needs are known before modeling finishes, including what must remain
  geometry and what belongs in reusable or bespoke texture/material channels.

## Research method

### 1. Establish the authoritative identity

Start from Phase 00, the Phase 03 roster and directly relevant faction,
gameplay, UX and visual canon. Record the exact Stable ID, display identity,
source classification and gameplay footprint. Do not let a convenient web
image redefine the canonical unit.

### 2. Build the primary-source set

Seek orthogonal evidence rather than many near-duplicate beauty images:

- official building instructions and inventories;
- official catalog scans and product photography;
- front, rear, side, top and underside views where available;
- official transformation, articulation or play-feature evidence;
- official animation/game footage when it demonstrates motion or operation;
- multiple source variants when canon intentionally combines or adapts them.

Archival databases may verify inventory, mold, color and set-family facts.
Secondary reconstructions may fill viewing-angle gaps only when labeled as
interpretation. Generated imagery is never evidence of source construction.

Treat every set as a container, not as an automatic one-set/one-unit mapping.
Before assigning it to the roster, audit the complete inventory and every
separately built model: detachable craft, repeated modules, minifigures,
equipment, resources, alternate builds and opposing-faction contents may each
support different assets. The 7691 Mothership, for example, contains a carrier,
a long front craft, two seated side craft and two disc-like jetpack modules.
Those parts must be recorded separately, but the approved production mapping
keeps them inside one selectable Mothership whose integrated sections open and
unfold around its internal bays. Separately built source modules are evidence,
not an automatic instruction to split the game unit.

Source use follows these boundaries:

- released official sets and their subassemblies may be primary evidence;
- official promotional material and official combination builds may be used
  when the exact model or module is named;
- unreleased official material may inform ideation, but production mapping
  requires game-director approval and a canon check;
- official games such as LEGO Rock Raiders, CrystAlien Conflict and LEGO
  Battles may suggest operation, recombination and RTS readability, but are not
  designs to copy or automatic canon;
- alternative official versions require game-director review;
- fan MOCs are excluded from the current T082 source pool.

When no official set directly matches a canonical asset, compare several
methods: direct adaptation, promoting an official subassembly, combining named
official modules, using an official game's recombination only as a method
precedent, synthesizing recurring faction construction grammar, or starting
from the canonical gameplay function and cladding it with source-supported
modules. Any proposed composed design must name the donors, exact borrowed
parts, connection/load path, new work, rejected alternatives and visible game
consequence before asking for approval. CrystAlien Conflict's Training Camp —
a 2-by-2 repetition of Dropship-like nose modules — is a useful precedent for
the method, not a building to reproduce.

### 3. Decompose visual identity

Describe the asset as a hierarchy of recognizable masses:

1. hero silhouette masses;
2. functional/support geometry;
3. faction construction grammar;
4. material and color-area distribution;
5. readable details that survive standard or far camera distances.

Identify the minimum silhouette that remains unmistakable in solid black. Note
which detail may disappear at lower LOD without changing identity.

Name what each major visible part actually is. A shape without a known
structural, operational or identity purpose is treated as unresolved, not as
free decorative detail.

Before stating a topology, appendage count or locomotion type, trace each major
part through the construction sequence, name its parent connection and decide
whether it is a foot, wheel, hover element, tool, support or decoration. Confirm
the result against a completed-model or product view. This two-view guard is
mandatory because a single final angle can make two articulated legs look like
four radial protrusions.

### 4. Reconstruct credible construction

Explain how visible masses connect and carry load. Identify repeated modules,
hinges, wheel/track/leg mounts, cockpit access, tool support, tube routing,
service openings and production exits. Mark areas where literal source geometry
must be enlarged, simplified or separated for gameplay readability.

Do not add exposed studs or greebles merely to signal LEGO. Construction logic
must be structural and functional.

### 5. Derive motion from mechanism

For each moving assembly, specify:

- node/pivot name and parent;
- axis or constrained path;
- rest and extreme poses;
- planted/contact requirement;
- authoritative state or event that drives it;
- anticipation, action, settle and recovery phases;
- secondary motion and significance-tier fallback;
- VFX/audio sockets and timing windows;
- failure, damage or interrupted-state behavior.

Animation should expose how the machine works. It may never decide gameplay
timing or hide an authoritative state change.

### 6. Plan materials and textures

For every visible feature, decide whether it belongs in geometry, a material
parameter, a reusable texture family or a bespoke texture. Preserve silhouette,
moving connections, LEGO construction and large panel breaks in geometry.
Reserve textures for information that is genuinely surface-bound, such as
controlled panel relief, wear policy, labels, masks, roughness variation,
emission patterns and terrain-scale detail.

Specify every required texture's purpose, source or generation method, channel
packing, color-space treatment, resolution, texel density, tiling behavior,
LOD/far-view fallback and import settings. Generated textures require visual
review and provenance. Baked lighting, fake geometry that breaks under motion,
illegible micro-noise and unreviewed prompt artifacts are rejected.

### 7. Compare against the entire roster

Every asset is reviewed next to its nearest visual neighbors, not in isolation.
Compare assets that share faction, role, footprint, locomotion or source era.
Record at least three deliberate identity differences for every plausible
confusion pair.

## Per-asset packet template

Each packet uses the following fixed structure.

### A. Identity and authority

- Stable ID and display name;
- faction and gameplay role;
- source classification and approved set/variant mapping;
- authoritative canon references;
- source-confidence summary and open questions.

### B. Reference board

- front, rear, left/right, top and three-quarter views;
- underside/interior/exploded evidence when mechanically relevant;
- instruction/inventory extracts for construction-critical areas;
- motion sequence or play-feature frames;
- citation, retrieval date, provenance/rights note and confidence per source.

### C. Recognition contract

- one-sentence silhouette thesis;
- three-to-seven non-removable identity anchors;
- black-silhouette thumbnails at near, standard and far RTS scale;
- dominant proportion ratios and negative spaces;
- palette/material area hierarchy;
- semantic callouts naming every identity-bearing assembly and its function;
- forbidden genericizations and nearest confusion risks.

### D. Construction contract

- hero, support and detail geometry bands;
- chassis/structural load path;
- repeated modules and connection grammar;
- wheel, leg, hover, tube, cockpit, tool and weapon mounting logic;
- source-faithful elements versus approved gameplay adaptations;
- scale relationship to footprint, minifigure and related roster assets.

### E. Material and texture contract

- geometry-versus-surface-detail decision map;
- reusable master-material assignments;
- required albedo/color-mask, normal/height, roughness, emission, decal or
  special-purpose textures;
- generation/source method, channel packing, resolution and texel density;
- tiling, LOD and far-camera fallback;
- prohibited baked lighting, false structure and micro-noise;
- provenance and human-review state.

### F. State and animation contract

- idle and locomotion mechanism;
- work/harvest/build/repair or production motion;
- weapon/tool action and recoil;
- transform, deploy, Refit, Surge or network behavior where applicable;
- construction, operational, disabled/brownout and damage states;
- destruction/wreck silhouette;
- named rig pivots, contact points and state-driver mapping.

### G. Presentation hookups

- selection and health anchors;
- projectile, VFX, light and audio sockets;
- passenger, service, production exit or network connection points;
- icon silhouette and rendered portrait camera;
- team Identification Tile placement;
- LOD survival rules and reduced-presentation fallback.

### H. Insight and decision ledger

- unusual source-supported mechanisms or asymmetry;
- variant history that affects the canonical read;
- commonly misrepresented colors, proportions or functions;
- valuable small details that strengthen identity without becoming clutter;
- contradictions, missing evidence and exact decisions required.

### I. Build handoff

- concise modeling order from primary masses to final readable detail;
- topology/rig risk list;
- first greybox acceptance shots;
- questions that must be answered before production modeling;
- explicit PASS/HOLD state and approving reviewer.

## Cross-roster deliverables

The 66 packets are accompanied by:

1. full-roster silhouette sheets at 24-, 44- and 72-cell camera framing;
2. faction construction-language boards;
3. footprint and relative-scale lineup;
4. palette/material area-ratio matrix;
5. locomotion and planted-contact matrix;
6. transformation/deployment/refit mechanism matrix;
7. weapon/tool/VFX/audio socket matrix;
8. building skyline, entrance, exit and network-connection matrix;
9. pairwise confusion register with mitigation decisions;
10. complete source and rights/provenance ledger;
11. texture/material requirement matrix separating reusable families from
    genuinely bespoke maps;
12. unresolved-decision queue ordered by downstream production cost.

## Game-director question and revision loop

Super Scout and later production do not silently resolve consequential
ambiguity. Each research batch ends with a short decision queue containing:

- the exact unresolved question;
- the best available evidence;
- two or more materially distinct interpretations when they exist;
- the visible and animation consequences of each interpretation;
- a recommendation and confidence level.

After internal source, silhouette, semantic, animation and texture audits pass,
the candidate is presented to the game director in reproducible views. The game
director either accepts it or describes corrections. Corrections are applied
and the same views are regenerated. No self-assessed “ideal” model bypasses
explicit game-director acceptance.

## Acceptance protocol

T082 is `BLOCKING_NOW` for T083 and T085.

It passes only when:

- all 66 packets contain every applicable required section;
- every factual claim and relied-upon image is traceable to its source;
- verified fact, approved adaptation, inference and unknown are visibly distinct;
- no packet depends on a single three-quarter reference;
- every identity-bearing assembly is named and its purpose understood;
- all canonical transforms and mechanically important states have buildable
  motion descriptions;
- every required texture has a justified role and production specification,
  while structural silhouette is not delegated to surface noise;
- a blind silhouette review identifies the representative complete-roster set
  without names, labels, role icons or selection rings;
- nearest-neighbor confusion audits have explicit differentiation decisions;
- gameplay-camera reviews prove that identity survives intended LODs;
- all high-cost contradictions are resolved by canon or escalated to the game
  director before modeling;
- no generic AI-looking form or unexplained decorative assembly survives the
  review corpus;
- the game director explicitly accepts the research corpus as the foundation
  for T083/T085 design production.

Useful discoveries are not permission to invent new gameplay, units, buildings,
faction mechanics or balance. Any discovery that conflicts with canon is
reported as `PROPOSED CANON CONFLICT` and stops the affected asset packet.
