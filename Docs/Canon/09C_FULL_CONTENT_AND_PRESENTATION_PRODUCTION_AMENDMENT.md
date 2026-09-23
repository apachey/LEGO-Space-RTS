# LEGO SPACE RTS — FULL CONTENT & PRESENTATION PRODUCTION AMENDMENT

**Status:** Authoritative milestone and implementation-facing canon amendment.

**Approved:** 2026-09-08 by explicit game-director direction.

## 1. Purpose and precedence

Phase 09 defines M7 as a limited visual vertical slice, M8 as the complete
roster data pass and M9 as Skirmish Alpha. That sequence did not assign the
production of the complete player-facing roster, interface art or audio to a
blocking milestone.

This amendment inserts:

# M8.5 — FULL CONTENT & PRESENTATION PRODUCTION

between M8 and M9.

Within full-roster models, buildings, presentation animation, VFX, iconography,
frontend/menu presentation and audio scope, this amendment supersedes the
direct M8 → M9 transition and Phase 09's instruction to defer all 35 final unit
art assets and all 31 final building art assets. Existing task identifiers
T001–T080 remain stable; M8.5 uses T081–T092.

This amendment does not select or change the visual style. M7 remains
responsible for producing and obtaining game-director acceptance of the visual
production standard that M8.5 applies across the complete roster.

## 2. Entry gate

M8.5 may enter production only when:

- M8 T070–T074 has produced and validated the complete gameplay roster;
- the game director has accepted the M7 visual direction and its gameplay-scale
  readability target;
- the M7 material, lighting, animation-driver, VFX, destruction, HUD and
  minimap foundations are suitable for production use;
- every official-derived asset can begin from the source/reference requirements
  already established by the visual canon;
- any new external tool, dependency, asset library or service has received the
  separate approval required by project policy.

M8 gameplay-data work may continue while visual-direction review is open.
Full-roster art production may not silently choose a visual direction that M7
has not accepted.

## 3. Player-facing production baseline

M8.5 converts the complete functional roster from data/proxy presentation into
a coherent player-facing production baseline.

At M8.5 exit:

- all 35 buildable units have distinct, recognizable, source-grounded or
  canon-grounded production designs and integrated 3D assets;
- all 31 building/infrastructure entries have distinct production designs and
  integrated 3D assets, including the Aero Tube Link;
- normal player-facing roster entries no longer use primitive cube, capsule or
  generic shared-body placeholders;
- models preserve authoritative footprint, pivot, selection anchor, movement
  profile and gameplay state rather than altering simulation to fit art;
- materials, team identification and faction identity remain readable at the
  canonical RTS camera distances;
- required locomotion, function, weapon, construction, production, repair,
  transformation, deployment, damage and destruction states have presentation
  coverage where applicable;
- all player-facing commands, production items, research, resources, roles,
  statuses and minimap categories have coherent icon treatment;
- all screens required to enter, configure, play, pause, reconnect to and leave
  the M9 skirmish loop have production presentation;
- the game has a complete baseline of UI, alert, vehicle/mechanical, combat,
  construction/repair, faction-system and environmental sound effects;
- no essential information is communicated only through color, animation, VFX
  or sound;
- source provenance, licensing status and production ownership are recorded for
  every external or official-derived asset.
- every unit and infrastructure design is backed by a reviewed Super Scout
  reference-intelligence packet, and the complete roster passes comparative
  silhouette/identity review before modeling is accepted.
- every player-facing model is an intentional stylized LEGO-derived
  interpretation with coherent construction and function; generic AI-looking
  forms, decorative noise and superficially LEGO-colored conventional vehicles
  do not satisfy the production baseline;
- every asset has an explicit geometry-versus-texture/material plan, and all
  approved required texture sets are generated, integrated and reviewed rather
  than left as future placeholders;
- every asset receives explicit game-director review and acceptance after
  presentation in the real gameplay camera and relevant animation states.

Production baseline means shippable-in-alpha and coherent, not immutable final
polish. M9 may tune readability, mix, performance and presentation in the
context of complete matches, but may not depend on replacing the entire roster
presentation later.

## 4. M8.5 implementation backlog

| ID | Task | Depends on | Acceptance |
|---|---|---|---|
| T081 | Production standard and asset pipeline lock | M7 acceptance, T074 | reviewed scale/pivot/socket/LOD/source/import rules and representative round trip |
| T082 | Super Scout full-roster reference intelligence | T081 | 66 evidence-backed asset packets plus approved roster silhouette, semantic-construction, animation and texture-needs matrices |
| T083 | Complete 35-unit visual design packages | T082 | 35 game-director-approved source/silhouette/construction/state/material sheets |
| T084 | Complete 35-unit production models | T083 | 35 recognizable integrated LEGO-derived models with approved textures/materials, team read, required LODs and actual gameplay-camera readability review |
| T085 | Complete 31-infrastructure visual design packages | T082 | 31 game-director-approved source/silhouette/construction/state/material sheets |
| T086 | Complete 31-infrastructure production models | T085 | 31 recognizable integrated LEGO-derived models with approved textures/materials, construction, operational and damage presentation plus actual gameplay-camera readability review |
| T087 | Full-roster rigging and animation pass | T084, T086, T065 | every applicable authored state binds to simulation-driven presentation |
| T088 | World, resource, VFX and destruction content pass | T084, T086, T066, T067 | complete required resources, combat/faction effects, damage and destruction profiles with reduced-VFX fallbacks |
| T089 | Complete icon, card and portrait system | T073, T084, T086 | no unresolved player-facing icon/portrait reference across units, buildings, commands, research, resources, roles, statuses or minimap |
| T090 | Frontend, menu and full UI presentation pass | T068, T069, T089 | complete M9-skirmish entry/settings/lobby/loading/pause/reconnect/result flow at supported layouts |
| T091 | Audio direction and implementation pass | T073, T084, T086, T088 | complete referenced SFX/ambience event set, faction readability, mix budgets and accessibility equivalents |
| T092 | Full-content integration and acceptance | T087–T091 | automated reference/import/performance gates, full-roster gameplay captures, playable build and explicit game-director visual/audio acceptance |

T081–T092 execute after T074 even though their identifiers numerically follow
the preserved M9 identifiers T075–T080.

## 5. Super Scout reference-intelligence gate

T082 is a research and preproduction gate, not a search for attractive images
and not a license to redesign gameplay. Its job is to remove ambiguity before
concept production and modeling begin.

Generated imagery may be used later as controlled ideation material, but it is
not evidence and may not become a production design merely because it looks
polished. Super Scout must prevent generic AI-slop outcomes by grounding every
major mass, connection, material break and moving mechanism in source evidence,
canon, gameplay function or an explicit game-director-approved adaptation.

T082 covers all 35 buildable units and all 31 infrastructure entries. Each
asset receives an evidence-backed packet that is sufficient for a separate
artist or modeling agent to build the correct asset without guessing its
identity, construction, scale or motion.

Research follows this source order:

1. approved official-source registry and existing project canon;
2. official instructions, inventories, catalogs and multi-angle product media;
3. official animation, game or promotional motion evidence where applicable;
4. reputable archival databases used to confirm part, color or variant facts;
5. secondary interpretation only when primary evidence is unavailable, clearly
   labeled by confidence and never promoted into canon by repetition.

Every asset packet must document:

- exact identity, faction, source lineage, set/variant mapping and confidence;
- front, side, rear, top and three-quarter evidence sufficient to understand
  volume rather than copying one flattering image;
- the three-to-seven silhouette anchors that must survive gameplay LODs;
- distinctive color/material distribution, transparent parts, lights, tools,
  wheels, legs, tubes, cockpits and other identity-bearing masses;
- plausible LEGO construction logic, repeated modules, connection grammar and
  where game-resolution adaptation is required;
- a semantic part map explaining what every identity-bearing mass, module,
  opening, tool, light and connection is and what gameplay or presentation
  function it communicates;
- relative scale against minifigure, neighboring roster entries, footprint and
  canonical RTS camera;
- locomotion mechanism and planted/contact behavior;
- named articulation, rig pivots and moving assemblies;
- idle, move, work, attack, production, repair, transform/deploy, damage and
  destruction beats that apply to the asset;
- what mechanical cause each animation must communicate and which motions are
  presentation-only;
- weapon/tool origin, recoil, projectile, exhaust, lamp, VFX and audio sockets;
- a geometry-versus-texture/material decision map identifying which forms must
  exist in silhouette geometry and which approved information belongs in
  albedo/color masks, normal/height detail, roughness, emission, decals or
  tiling surface textures;
- the exact texture sets that must be authored or generated later, including
  purpose, channels, scale, tiling, resolution/LOD behavior and prohibited
  baked-in lighting or false structural detail;
- operational, disabled, brownout, construction, damage and wreck states where
  applicable;
- special factual or visual insights that materially strengthen identity,
  including source-supported asymmetry, unusual mechanisms, variants or
  historically misrepresented colors;
- forbidden genericizations: the changes that would make the asset read as a
  generic tank, buggy, spaceship, factory or tower instead of itself;
- nearest visual-confusion risks inside the roster and the deliberate
  differences that prevent them;
- unresolved evidence, contradictions and required game-director decisions.

The cross-roster output includes:

- black-silhouette contact sheets at near, standard and far gameplay scales;
- front/side/top proportion boards grouped by faction, role and footprint;
- palette/material and transparent-function comparisons;
- texture/material-needs matrices with reusable families and genuinely bespoke
  requirements separated;
- locomotion and animation-mechanism matrices;
- building skyline, entrance, production-exit and network-connection matrices;
- a confusion audit proving that units with similar roles or chassis remain
  identifiable without labels, icons, selection rings or faction names;
- a source ledger with URLs/citations, retrieval date, rights/provenance notes
  and confidence for every relied-upon reference.

T082 passes only when the game director can identify the complete representative
silhouette set without text prompts, the animation intent is buildable from the
packets, and all consequential unknowns are either resolved or explicitly
escalated. Research volume alone is not acceptance.

The T082 near/standard/far silhouette boards are footprint-relative
preproduction evidence, not captures of production models. Actual in-engine
gameplay-camera and model-LOD readability review belongs to T084 for units and
T086 for infrastructure, after the relevant models exist; it is not a
prerequisite for T082, T083 or T085. This sequencing does not waive the final
production-model review or the M8.5 acceptance requirement.

Unresolved choices are presented to the game director as concise visual or
mechanical questions with evidence and consequences. They are never filled by
an unmarked assumption merely to complete a packet.

The execution specification and packet template live in
`Docs/Development/M85_SUPER_SCOUT_REFERENCE_INTELLIGENCE.md`.

## 6. Per-asset director review loop

No T083–T092 asset may be called final because an implementation agent considers
it polished. Each unit and infrastructure entry follows this loop:

1. Super Scout evidence and open questions are reviewed;
2. the design package resolves silhouette, construction, semantics, animation
   and material/texture intent;
3. a production candidate is built and audited against the packet, canon,
   gameplay footprint and nearest confusion risks;
4. the candidate is shown in reproducible near/standard/far gameplay views,
   key animation/state views and a neutral inspection view;
5. the implementation agent reports remaining doubts rather than hiding them;
6. the game director accepts the candidate or describes corrections;
7. corrections are implemented and the same evidence set is shown again;
8. the asset reaches PASS only after explicit game-director acceptance.

The loop is bounded by decisions, not by an arbitrary revision count. Repeated
failure of two materially different implementations still invokes the project's
architecture/approach escalation rule rather than endless patch stacking.

## 7. Unit and building production requirements

Every unit and infrastructure production package includes, as applicable:

- identity, source classification and multi-view reference evidence;
- gameplay-camera silhouette and scale review;
- credible LEGO-derived construction logic without requiring literal
  per-brick simulation;
- production mesh hierarchy and named mechanical pivots;
- material roles, faction palette and team Identification Tiles;
- approved texture/material inventory with generated source files, channel
  documentation, import settings and provenance;
- selection, health, projectile, VFX, audio and interaction anchors;
- LODs and reduced-presentation behavior appropriate to asset complexity;
- animation/state coverage and transformation/deployment correspondence;
- damage, wreck and bounded LEGO-like destruction presentation;
- icon and rendered-card/portrait outputs derived from the canonical model;
- import validation and performance telemetry.

Art never owns gameplay truth. Godot presentation consumes authoritative state;
it does not use animation, physics, VFX or audio timing to decide gameplay.

## 8. Iconography and interface content

M8.5 completes the artwork required by the Phase 07 interaction architecture
without changing that architecture merely for decoration.

The production icon set covers:

- every buildable unit and infrastructure entry;
- every available command, target mode and faction action;
- every research entry and its locked/available/active/completed states;
- resources, Operations Capacity, Energy, Charge and network states;
- combat roles, production, repair, transport, transformation, Refit, Surge,
  Stability and other player-relevant statuses;
- minimap categories, alerts, cursors and command feedback.

Build and production portraits use the integrated game model so UI and world
identity cannot drift into separate designs.

T090 applies the accepted visual language to the complete frontend and in-match
UI required for Skirmish Alpha. It includes main menu, skirmish setup, faction
selection, multiplayer lobby/connection states, loading, settings, pause,
reconnect and match-result presentation. It does not add campaign, store,
account or matchmaking scope.

## 9. Audio production boundary

T091 establishes and implements the first complete gameplay-audio baseline:

- UI interaction and confirmation;
- alerts with perceptually distinct priority classes;
- locomotion and faction-specific machinery;
- weapons, impacts, damage and destruction;
- construction, production, harvesting, repair and transport;
- transformation, Refit, Charge/Surge, Worksite, Tube and displacement events;
- resources, world ambience and strategically relevant local emitters;
- variation, concurrency, distance attenuation, priority and mix budgets;
- subtitle/text/visual equivalents for essential audio information.

Procedurally authored, recorded or licensed audio may be used when provenance
and rights are explicit. Music, voice acting and localization recording are not
silently implied by this SFX/ambience gate; they require an approved content
scope if made blocking later.

## 10. M9 consequence

M9 T075–T080 remain the Skirmish Alpha tasks and retain their stable IDs.

M9 implementation may be prepared where technically independent, but M9 cannot
close or claim a representative Skirmish Alpha presentation until T092 passes.
The intended production sequence is:

**M7 visual standard → M8 functional roster → M8.5 full content and
presentation → M9 complete skirmish integration and polish.**

Stress60 remains governed by Phase 09B and the later explicit game-director
decision that makes it blocking for M9 large-battle acceptance, not M8.5.
