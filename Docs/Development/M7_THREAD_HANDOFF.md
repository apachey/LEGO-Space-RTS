# M7 THREAD HANDOFF

**Prepared:** 2026-09-01

**Purpose:** dense context transfer into a clean Codex task

**Scope:** M7 visual vertical slice only; this is not visual canon

This file preserves the decisions and implementation state that matter after a
long art-direction thread. It is an index into repository evidence, not a
replacement for canon, `AGENTS.md`, or `PROJECT_STATE.md`.

## Start contract for the receiving task

1. This task is **M7 only**. Do not begin T070/M8, even though the global
   project state permits M8 work.
2. Read `AGENTS.md`, `Docs/Development/PROJECT_STATE.md`, this handoff, then
   only the M7 development/canon files relevant to the next user request.
3. Keep `Docs/Canon/` read-only. Visual exploration remains deliberately
   non-canonical until the game director explicitly locks a direction.
4. Repository state and tracked evidence outrank remembered conversation.
5. Do not change the game merely to continue visual work. SimCore remains
   authoritative and presentation remains non-authoritative.
6. Do not manufacture a next art-direction decision. The game director owns
   subjective selection; implementation, objective audit and regression
   prevention belong to Codex.
7. Avoid repeated foreground screenshot runs while the game director is using
   the laptop. Prefer one consolidated capture pass after objective checks,
   and inspect every capture rather than treating file creation as validation.
8. When the game director says "continue/go", complete a coherent M7 increment
   instead of stopping after each tiny adjustment.

## Exact repository state at transfer

- Source branch before this handoff commit: `codex/m7-quality-revision`.
- Last implementation commit: `c2c8826294247b830ba9bdc5824ab334c3e00d84`
  (`fix(m7): restore continuous masked HUD frames`).
- The branch is local-only, has no upstream and is 28 commits ahead of
  `origin/main` at this point. It has not been merged into `main`.
- The worktree was clean before this documentation transfer.
- An unused `codex/m8-t070-unit-definitions` branch points at the same
  `c2c8826` commit. It contains no M8 implementation and must be ignored here.
- M0-M6 remain accepted. M7 infrastructure is implemented and verified, but
  most visual choices remain open.
- Current profile formats are Look Lab schema **9** and HUD Lab schema **8**.
- `Artifacts/` and `Builds/` are gitignored. Their evidence remains in this
  source worktree but will not be cloned into a fresh worktree automatically.

## Game-director decisions that must survive the transfer

### Overall direction and process

- There is no current visual canon. Phase 08 visual conclusions were suspended
  so the style could be found through controlled experiments.
- Image generation was rejected as representative style evidence. It produced
  infeasible detail, near-duplicate treatments and attractive images that did
  not honestly show what the real-time implementation could deliver.
- The useful process is a reproducible in-engine scene at real RTS camera
  distance, with direct controls and complete JSON copy/paste.
- Desired centre of gravity: volumetric mechanical 3D, broad readable
  silhouettes, visible mass, understandable construction, large functional
  parts, clean color masses and enough close detail without sacrificing RTS
  readability. LEGO is a **construction language**, not tabletop photography.
- The shorthand research centre is Industrial Annihilation + StarCraft II +
  Beyond All Reason translated into LEGO construction language. It is not a
  copying formula. The full positive/negative reference audit lives in
  `Docs/Development/M7_ART_DIRECTION_RESEARCH_BRIEF.md`.
- Heroic RTS was the strongest Round 2 comparison, but was not selected as the
  direction. Retain the useful ingredients--Industrial color depth, Heroic
  color/bloom, Constructive LEGO highlights and readable glints--without the
  rejected blunt warm cast.
- The six-page Palette Ratio Lab is accepted. The world style and final HUD
  visual language are not.

### Palette and emission semantics

- Never inject a shared player-blue identity into all factions.
- Judge visible color area and visual importance, not raw LEGO part count.
- Rock Raiders require meaningful earth brown: the accepted lab ratio gives it
  18% of visible vehicle mass.
- The 7316 Excavation Searcher is beige/tan-led: the accepted study gives beige
  42%, rather than treating it as a minor accent.
- Transparent color is not automatically light. Glass/polymer may be saturated
  without emission; emission follows authored function.
- Authored luminous roles:
  - Rock Raiders: neon orange and neon lime;
  - Mars Mission Astronauts: blue;
  - Mars Mission Aliens: neon lime;
  - Life on Mars Astronauts: red signals plus warm light through clear lenses;
  - Life on Mars Martians: red, neon orange, neon lime and blue.
- Static lamps do not pulse. Signals may pulse only when their real function
  calls for it. Crystals can pulse independently.

### Look Lab and world presentation

- The representative fixture is gameplay scale: four identical units facing
  different directions, one unobstructed shooter, one intact building, one
  burning building, Crystal, ground staging, tracks, destruction and combat
  VFX. HUD is tested separately.
- Free camera review is required: orbit, pan, rotation and 24-72-cell zoom.
- Materials must read as different material families, not one shader recolored.
  The normal art-director UI should stay simple: choose semantic role and base
  color while code owns physically coherent texture/relief/reflection recipes.
  Do not return raw Blender-like sliders to the normal workflow.
- Fine raster noise was repeatedly visible only at extreme zoom or shimmered.
  The productive terrain direction is **Hybrid Surface**: authored broad forms
  plus restrained raster color/normal/roughness response and broad relief only
  outside traffic surfaces. The game director explicitly preferred the hybrid
  ground.
- The authored Earth shape is not random geology. It is a deterministic lab
  diagram: work pad around the four units, service aprons to both buildings, a
  route/mineral seam toward the Crystal and bedrock outside traffic. It is
  presentation staging, not map-generation canon.
- Preserve Authored Surface and Raster Forward as honest method comparisons;
  Legacy Raster is migration evidence, not a recommended direction.
- Earth, Mars, Moon, Planet U and Underground are exploratory identities. None
  is approved biome canon.
- Day/night must show extended, readable blue and golden hours through natural
  light/sky gradients rather than a blunt warm screen filter. Functional
  headlights and work lights fade on through darkness, are off at noon and
  actually illuminate the terrain. Underground keeps appropriate local light.
- Outline remains an independent on/off test and may work with the style post
  disabled. Film grain starts at zero; animated/crawling grain or dither is not
  part of the baseline, and dither is relevant only with posterization.
- Preserve real unit shadows without the former large stepped black terrain
  bands or pixel-hole artifacts.
- Wheels stay planted while the chassis receives restrained suspension.
  Generic hit wobble is removed. The corrected recoil and explosion/destruction
  direction were liked; destruction uses bounded LEGO modules rather than
  shrinking an object below the terrain. Smoke must not cover the luminous
  flame.
- The current texture stack is valid look-development infrastructure, not an
  accepted production normal/ORM pipeline or final surface style.

### HUD direction and current technical acceptance

- StarCraft II is a reference for functional hierarchy and a readable
  full-width RTS console, not for literal frame art or a universal Terran-like
  dark navy skin. The old dark-blue test theme is not canon.
- The preferred direction is **Hybrid · Vector + Raster**: code-native
  responsive layout and hit geometry enriched by authored raster detail that
  would be wasteful to recreate procedurally.
- Preserve Legacy Frames for honest A/B comparison. The game director strongly
  liked the generated/raster frame art; do not delete it or replace it with
  weaker procedural corners.
- There is exactly one complete recipe per playable faction. Frame, fill,
  background, accents and text treatment belong together:
  - Rock Raiders: dark industrial grey, dark turquoise, important earth brown,
    restrained hazard yellow; no white top/interior default;
  - Astronauts: one unified white/light-grey + medium-blue kit with controlled
    mission orange and small red/blue signals;
  - Aliens: black/charcoal + lime/neon-lime, limited neutral grey, no purple;
  - Martians: tan, sand-red and blue pneumatic/mechanical construction with
    controlled lime energy.
- Do not expose Life on Mars humans and Mars Mission humans as separate HUD
  factions. Do not detach frames from their faction recipe.
- The latest technical correction is accepted as correct by the game director:
  the fill/content aperture now follows each frame, square black/white corner
  backings are gone, the raster plate cannot escape the boundary, the frame is
  continuous on all four sides, and the Rock Raiders mask no longer eats the
  interface. This was the explicit final response: the defect was fixed.
- The technical acceptance above is not an automatic declaration of final HUD
  visual canon. The Hybrid direction and frames are strongly liked; future
  subjective lock still requires explicit game-director wording.
- Oxanium SemiBold owns headings/section labels. IBM Plex Sans owns body text,
  values and buttons. Keep semantic text roles, curated sizes and
  surface-aware contrast in code rather than returning basic typography
  choices to the art director.
- The production HUD and lab share one functional hierarchy. Art finishes may
  not move panels or hit targets. Hide unimplemented command slots instead of
  filling them with meaningless decoration.

## Implemented M7 systems

- T064 material/look-development infrastructure, real-time Look Lab and
  generated review texture routes.
- Code-native Style Lab and accepted six-page Palette Ratio Lab.
- T065 shared presentation animation driver and mechanical rig binding.
- T066 bounded reusable projectile/muzzle/impact VFX pools.
- T067 bounded LEGO destruction visuals.
- T068 responsive retained HUD framework and deterministic density fixtures.
- T069 fog-correct north-up minimap with client-legal state and input handling.
- Look Lab schema 9: Authored, Raster Forward, Hybrid and migration-only Legacy
  ground paths; five world-light cycles; free camera; complete JSON handoff.
- HUD Lab schema 8: eight fixtures, four faction-bound recipes, four finishes,
  exact faction apertures, responsive aspect fixtures and complete JSON
  handoff.
- The canonical M7 exit is not formally closed. The world fixture still uses
  one reusable Raider carrier rather than representative final four-faction
  assets, and the current code-native portraits/icons remain scaffolding.

Primary implementation entry points:

- `GodotClient/Scripts/Client/M7LookLab.cs`
- `GodotClient/Scripts/Presentation/M7LookProfile.cs`
- `GodotClient/Scripts/Presentation/M7LookMaterialFactory.cs`
- `GodotClient/Scripts/Client/M7HudLab.cs`
- `GodotClient/Scripts/UI/M7HudProfile.cs`
- `GodotClient/Scripts/UI/HudView.cs`
- `GodotClient/Scripts/UI/HudFactionChrome.cs`
- `GodotClient/Scripts/UI/HudFactionSkinLibrary.cs`
- `GodotClient/Scripts/UI/HudFactionSurfaceMask.cs`
- `GodotClient/Shaders/hud_faction_aperture.gdshader`

Detailed design/implementation evidence:

- `Docs/Development/M7_LOOK_LAB.md`
- `Docs/Development/M7_LOOK_LAB_AUDIT.md`
- `Docs/Development/M7_HUD_LAB.md`
- `Docs/Development/M7_HUD_FRAME_AUDIT.md`
- `Docs/Development/M7_VISUAL_STYLE_EXPLORATION.md`
- `Docs/Development/M7_MINIMAP.md`

## Verification and review evidence

- Latest exact-aperture/continuous-frame fast suite:
  `Artifacts/Verification/20260831T172412Z-fast-summary.txt`.
  Result: 18/18 stages PASS, 278 NUnit tests, zero blocking and zero diagnostic
  failures.
- Latest complete schema-8 aperture full suite:
  `Artifacts/Verification/20260830T172045Z-full-summary.txt`.
  Result: zero blocking failures; the sole diagnostic is the intentionally
  deferred M9 60-mover case.
- A launchable macOS debug build exists at
  `Builds/macOS/LEGO Space RTS.app`, but it predates the last five HUD commits
  (`12030b3` through `c2c8826`). It is not evidence for the final aperture fix;
  export again before asking for a build-based review of current HEAD.
- Current HEAD HUD evidence:
  - `Artifacts/Screenshots/m7-hud-corner-final-rock.png`;
  - `Artifacts/Screenshots/m7-hud-corner-final-astronaut.png`;
  - `Artifacts/Screenshots/m7-hud-mask-rock-raiders-final.png`;
  - `Artifacts/Screenshots/m7-hud-mask-aliens.png`;
  - `Artifacts/Screenshots/m7-hud-mask-astronauts.png`;
  - `Artifacts/Screenshots/m7-hud-mask-martians.png`;
  - `Artifacts/Screenshots/m7-hud-rock-raiders-legacy-schema8.png`.
- Current world/material evidence:
  - `Artifacts/Screenshots/m7-ground-schema9-earth-hybrid.png`;
  - `Artifacts/Screenshots/m7-ground-schema9-earth-authored.png`;
  - `Artifacts/Screenshots/m7-ground-schema9-earth-raster.png`;
  - `Artifacts/Screenshots/m7-materials-stronger-zoom24-v3.png`;
  - `Artifacts/Screenshots/m7-blue-hour-headlights.png`;
  - `Artifacts/Screenshots/m7-golden-hour-gradient.png`.

Do not rerun the full suite merely to bootstrap the receiving task. Run
verification proportionally after an actual change.

## Open M7 decision surface

- The current transfer itself authorizes no new visual implementation. The
  receiving task should report ready and wait for the game director's next M7
  instruction.
- Still subjective/unlocked: final world rendering profile, final production
  material/texture language, final biome identities, final HUD visual canon,
  final command icons/roster portraits, animation feel, VFX feel and camera
  feel.
- Before closing M7, determine the minimum representative four-faction asset
  proof required by the milestone rather than silently treating the one Raider
  carrier as complete roster presentation.
- If the game director supplies Look/HUD JSON, treat that complete profile as
  the next exact review baseline; audit it before changing code.
- Do not mark T064 or the HUD art direction accepted from positive comments
  alone. Record acceptance only after an explicit lock.

## High-risk visual regressions to prevent

- square black or white backing visible beyond a faction-frame aperture;
- a mask cutting away valid HUD content or a frame breaking at corners;
- stretched full raster frames, repeated whole-frame edge tiles, clipped beads
  or competing divider systems;
- white Rock Raiders console mass, purple Alien identity, split Astronaut HUD
  identities or a universal dark-blue console;
- fine textures visible only at extreme zoom, obvious raster tiling, model
  texture phase restarting per LEGO sub-mesh or animated shimmer;
- terrain bloom, always-on headlights, nonfunctional local lights, blunt warm
  post filters or missing blue/golden-hour separation;
- units firing through one another, rear-facing weapon fire, disappearing
  recoil, whole-vehicle shake, shrinking destruction or smoke covering fire;
- HUD content added back into the world Look Lab.
