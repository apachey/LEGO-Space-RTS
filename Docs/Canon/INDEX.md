# LEGO SPACE RTS — CANON INDEX

This directory contains the authoritative game-design and technical canon used
for implementation.

Files in this directory are READ-ONLY for normal implementation agents.

## Expected canonical sources

### Phase 00 — Canon Set Registry
Verified official LEGO Rock Raiders, Mars Mission, and Life on Mars source
registry and mandatory source-material coverage.

Expected file:
`00_CANON_SET_REGISTRY.md`

### Phase 01 — Creative Foundation & Game Bible
Core game premise, strategic structure, shared design principles, setting
foundation, and global gameplay identity.

Expected file:
`01_GAME_BIBLE_FOUNDATION.md`

### Phase 02 — Faction Bible & Asymmetry
Authoritative faction identities and faction-specific systemic architectures.

Expected file:
`02_FACTION_BIBLE.md`

### Phase 03 — Unit & Building Roster
Complete playable unit/building roster and official-set coverage mapping.

Expected file:
`03_UNIT_BUILDING_ROSTER.md`

### Phase 04 — Economy, Technology & Progression
Resources, harvesting, processing, Energy, construction, production,
operational capacity, expansion economics, technologies, and prerequisites.

Expected file:
`04_ECONOMY_TECHNOLOGY_PROGRESSION.md`

### Phase 05 — World & Map Bible
The Meridian Reach, terrain, map topology, resource placement, strategic map
geometry, and authored environmental systems.

Expected file:
`05_WORLD_MAP_BIBLE.md`

### Phase 06 — Combat, Damage & Balance Framework
Authoritative prototype combat, movement, durability, weapons, ranges,
formations, collision, counter structure, and numerical balance.

Expected file:
`06_COMBAT_DAMAGE_BALANCE.md`

### Phase 07 — Controls, Camera, UX & Interface
Authoritative player interaction, selection, commands, control groups, camera,
information architecture, and RTS usability rules.

Expected file:
`07_CONTROLS_CAMERA_UX_INTERFACE.md`

### Phase 08 — Visual Bible & UI Art Direction
Authoritative rendering and visual-development rules, faction readability,
LEGO visual language, materials, presentation, and UI art direction.

Expected file:
`08_VISUAL_BIBLE_UI_ART_DIRECTION.md`

### Phase 09 — Technical Architecture & Prototype Implementation Spec
Engine-independent deterministic simulation architecture and implementation
requirements.

Expected file:
`09_TECHNICAL_ARCHITECTURE.md`

### Phase 09A — Godot Engine Amendment
Godot 4.7.x .NET replaces the former Unity-specific host decisions while
preserving engine-independent simulation and gameplay canon.

Expected file:
`09A_GODOT_ENGINE_AMENDMENT.md`

## Agent rule

Do not infer missing canonical values merely from this index.

If a task requires a missing canonical source, request that source when the
missing information is necessary for a design decision.

Current Phase 10 implementation documents under the repository's normal Docs/
tree are implementation records, not replacements for missing Phase 00–09
canonical source documents.
