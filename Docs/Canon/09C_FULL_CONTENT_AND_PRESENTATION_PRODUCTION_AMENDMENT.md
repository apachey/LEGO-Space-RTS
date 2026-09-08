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
T001–T080 remain stable; M8.5 uses T081–T091.

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

Production baseline means shippable-in-alpha and coherent, not immutable final
polish. M9 may tune readability, mix, performance and presentation in the
context of complete matches, but may not depend on replacing the entire roster
presentation later.

## 4. M8.5 implementation backlog

| ID | Task | Depends on | Acceptance |
|---|---|---|---|
| T081 | Production standard and asset pipeline lock | M7 acceptance, T074 | reviewed scale/pivot/socket/LOD/source/import rules and representative round trip |
| T082 | Complete 35-unit visual design packages | T081 | 35 approved source/silhouette/construction/state sheets |
| T083 | Complete 35-unit production models | T082 | 35 recognizable integrated models with materials, team read and required LODs |
| T084 | Complete 31-infrastructure visual design packages | T081 | 31 approved source/silhouette/construction/state sheets |
| T085 | Complete 31-infrastructure production models | T084 | 31 recognizable integrated models with construction, operational and damage presentation |
| T086 | Full-roster rigging and animation pass | T083, T085, T065 | every applicable authored state binds to simulation-driven presentation |
| T087 | World, resource, VFX and destruction content pass | T083, T085, T066, T067 | complete required resources, combat/faction effects, damage and destruction profiles with reduced-VFX fallbacks |
| T088 | Complete icon, card and portrait system | T073, T083, T085 | no unresolved player-facing icon/portrait reference across units, buildings, commands, research, resources, roles, statuses or minimap |
| T089 | Frontend, menu and full UI presentation pass | T068, T069, T088 | complete M9-skirmish entry/settings/lobby/loading/pause/reconnect/result flow at supported layouts |
| T090 | Audio direction and implementation pass | T073, T083, T085, T087 | complete referenced SFX/ambience event set, faction readability, mix budgets and accessibility equivalents |
| T091 | Full-content integration and acceptance | T086–T090 | automated reference/import/performance gates, representative full-roster captures, playable build and human visual/audio acceptance |

T081–T091 execute after T074 even though their identifiers numerically follow
the preserved M9 identifiers T075–T080.

## 5. Unit and building production requirements

Every unit and infrastructure production package includes, as applicable:

- identity, source classification and multi-view reference evidence;
- gameplay-camera silhouette and scale review;
- credible LEGO-derived construction logic without requiring literal
  per-brick simulation;
- production mesh hierarchy and named mechanical pivots;
- material roles, faction palette and team Identification Tiles;
- selection, health, projectile, VFX, audio and interaction anchors;
- LODs and reduced-presentation behavior appropriate to asset complexity;
- animation/state coverage and transformation/deployment correspondence;
- damage, wreck and bounded LEGO-like destruction presentation;
- icon and rendered-card/portrait outputs derived from the canonical model;
- import validation and performance telemetry.

Art never owns gameplay truth. Godot presentation consumes authoritative state;
it does not use animation, physics, VFX or audio timing to decide gameplay.

## 6. Iconography and interface content

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

T089 applies the accepted visual language to the complete frontend and in-match
UI required for Skirmish Alpha. It includes main menu, skirmish setup, faction
selection, multiplayer lobby/connection states, loading, settings, pause,
reconnect and match-result presentation. It does not add campaign, store,
account or matchmaking scope.

## 7. Audio production boundary

T090 establishes and implements the first complete gameplay-audio baseline:

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

## 8. M9 consequence

M9 T075–T080 remain the Skirmish Alpha tasks and retain their stable IDs.

M9 implementation may be prepared where technically independent, but M9 cannot
close or claim a representative Skirmish Alpha presentation until T091 passes.
The intended production sequence is:

**M7 visual standard → M8 functional roster → M8.5 full content and
presentation → M9 complete skirmish integration and polish.**

Stress60 remains governed by Phase 09B and the later explicit game-director
decision that makes it blocking for M9 large-battle acceptance, not M8.5.
