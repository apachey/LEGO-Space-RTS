# LEGO SPACE RTS — CANON INDEX

**Status:** Authoritative implementation-facing canon index.

Normal implementation agents may READ these files but may not modify them without explicit user approval of a canon change.

## Canonical sources

- `00_CANON_SET_REGISTRY.md` — Phase 00, approved official-source registry baseline.
- `01_GAME_BIBLE_FOUNDATION.md` — Phase 01, Game Bible Foundation.
- `02_FACTION_BIBLE.md` — Phase 02, Faction Bible & Asymmetry.
- `02A_ALIEN_BIOMECHANICAL_SOURCE_RECONCILIATION.md` — authoritative coherent
  Alien biomechanical identity/source rewrite, director-approved 2026-09-13;
  not a gameplay or balance amendment.
- `03_UNIT_BUILDING_ROSTER.md` — Phase 03, Unit & Building Roster + Canon Set Mapping.
- `04_ECONOMY_TECHNOLOGY_PROGRESSION.md` — Phase 04, Economy, Technology & Progression.
- `05_WORLD_MAP_BIBLE.md` — Phase 05, World & Map Bible.
- `06_COMBAT_DAMAGE_BALANCE.md` — Phase 06, Combat, Damage & Balance Framework.
- `07_CONTROLS_CAMERA_UX_INTERFACE.md` — Phase 07, Controls, Camera, UX & Interface.
- `08_VISUAL_BIBLE_UI_ART_DIRECTION.md` — Phase 08, Visual Bible & UI Art
  Direction. **VISUAL AUTHORITY SUSPENDED:** by explicit game-director decision
  on 2026-08-20, its rendering, material, lighting, palette and stylistic
  conclusions are historical research only while M7 performs a new art-direction
  search. Phase 07 functional UX requirements and non-visual gameplay canon are
  unaffected. No replacement visual canon exists until the game director locks
  one after the code-native Style Lab.
- `09_TECHNICAL_ARCHITECTURE.md` — Phase 09, deterministic technical architecture. **Engine-specific Unity host decisions in this file are superseded by Phase 09A.**
- `09A_GODOT_ENGINE_AMENDMENT.md` — authoritative Godot 4.7.x .NET engine-host amendment.
- `09B_MOVEMENT_ARCHITECTURE_AND_PROTOTYPE_GATE_AMENDMENT.md` — authoritative Movement Architecture v2 and prototype-gate amendment.
- `09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md` — authoritative
  M8.5 full-roster reference intelligence, models, buildings, animation, VFX,
  iconography, frontend/UI and audio production milestone inserted between M8
  and M9.

## Precedence rule

When Phase 09 and Phase 09A conflict only on engine-host-specific implementation details, **Phase 09A prevails**. Within the movement-implementation and prototype-gate scope explicitly amended by Phase 09B, **Phase 09B prevails**. Within full-roster visual/audio content production and the M8-to-M9 milestone transition, **Phase 09C prevails**. All other engine-independent SimCore, determinism, data, pathfinding, networking-protocol, performance-gate, and gameplay architecture from Phase 09 remain authoritative unless an amendment explicitly says otherwise.

## Agent rule

For Alien biological/technological identity and its derived presentation,
Phase 02A prevails over the former absolute non-biological requirements in
Phases 01–08. It does not lift Phase 08's visual-authority suspension or approve
generated assets. Gameplay, balance and technical architecture are unchanged.

Do not infer missing canonical values from this index. Read the directly relevant canonical source before implementing a gameplay or major technical system. If two authoritative sources appear to conflict outside an explicit amendment, stop and escalate rather than silently choosing one.
