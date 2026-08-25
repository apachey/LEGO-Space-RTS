# M7 LOOK LAB — SCHEMA 4 AUDIT

## Status

**NON-CANONICAL LOOK-DEVELOPMENT AUDIT.**

This audit responds to the game-director review and the supplied schema-1
profile. It records what was defective, what changed, which supplied settings
became the new review baseline and where human judgement is still required.

## Defect audit

| Review finding | Root cause | Schema-2 result |
| --- | --- | --- |
| Emission controls appeared inert | The old surfaces did not provide a complete HDR + halo + local-light response | Signal, lamp, Crystal, tracer and fire now have real HDR emission; object halo, darker same-hue luminous edge and local-light energy/range are independent controls |
| Lighting background appeared inert | It changed only the clear background, which the terrain covered | `Far-field background` now affects both the environment and distant terrain through an explicit influence control, live without rebuilding materials |
| Grain and dither crawled | Both sampled time-varying noise | Grain is static screen-space noise; dither is a static ordered pattern and is active only when posterization is active |
| Tracks overpainted units | The preview used a no-depth-test transparent material | Tracks are depth-tested ground strips with tread pattern, width and length controls |
| Tracer emission appeared inert | The tracer used a flat transparent material | It now uses the HDR emissive material and responds to tracer energy |
| Fire did not light the scene | The old fire card was visual only | Fire is GPU particles plus an orange local light; energy and flicker affect both |
| Impact was hidden inside the target | Target point was the building center | The hit point is calculated on the exterior box face; automation asserts that placement |
| Scorch had no useful visual proposition | It was an arbitrary overlay unrelated to accepted damage art direction | Scorch is removed from the lab and schema; schema-1 scorch fields are safely ignored |
| HUD/health systems competed | The scene mixed temporary HUD, markers and multiple health representations | All HUD, health bars, minimap, selection and target fixtures are removed; UI will receive its own design phase |
| Muzzle flash read as a card | It was mesh-based | Muzzle and impact are short GPU-particle bursts |
| Material families still read alike | Procedural shader variation had no authored surface signal | Four grayscale generated detail maps now modulate painted shell, brushed metal, rubber and quarry ground independently |
| Camera could not inspect the scene | Only preset sliders/wheel were practical | Right-drag orbit, middle-drag/WASD pan, wheel zoom, Q/E rotation, F reset, exact sliders and copied focus coordinates are available |
| Outline-off automation did not turn it off | The command-line override handled `on` but not `off` after the default changed | Both `on` and `off` are explicit and are captured as an A/B regression pair |

## Supplied profile treatment

The supplied camera, shading, material colors/physical values, glass, emission
colors/energies, light rig, post, outline, combat VFX and ground values seed the
schema-2 review baseline. New texture, halo, luminous-edge, local-light,
background-influence and track-shape values receive conservative defaults.

One supplied value is intentionally not adopted as the reset baseline:
`lighting.backgroundColor: #0785ff`. That value was selected while the old
control produced no visible scene response, so it cannot yet be treated as a
reviewed preference. Reset uses neutral `#151d25`; pasting the old profile still
migrates and displays `#0785ff` correctly for a fresh judgement.

Removed schema-1 HUD, scorch and ground-normal fields do not survive copy-back.
The old profile remains paste-compatible and is automatically exported as
the current schema after application.

## Texture generation record

The built-in image generator produced four 1024×1024 grayscale review maps:

- `painted_shell_detail.png` — seamless clean painted/molded shell micro-surface,
  broad subtle cloudy variation and sparse restrained wear, flat neutral data;
- `brushed_metal_detail.png` — seamless fine directional brushed-metal grain,
  low-contrast machining variation, no lighting or object silhouette;
- `rubber_detail.png` — seamless matte rubber micro-grain with fine pores and
  restrained variation, no lighting or molded object;
- `quarry_ground_detail.png` — seamless subdued quarry regolith/compacted rock,
  broad mineral variation and sparse fine grit, no shadows or perspective.

All prompts required seamless tiling, grayscale values, orthographic flat
surface data, no baked lighting, no text, no border, no isolated object and no
large recognizable feature. These are look-development textures only.

## Recommendations for the next review

1. Judge materials first with the supplied profile, then temporarily neutralize
   post temperature/tint and saturation. The combination `temperature 0.45`,
   `tint 0.52`, `saturation 1.40` is intentionally strong and can conceal whether
   a material color is working on its own.
2. Compare outline on/off before tuning its width. Width `3.1` and opacity `1.0`
   are strong at RTS scale; if silhouettes help but internal lines compete,
   reduce crease strength before silhouette strength.
3. Tune bright objects in this order: surface energy, halo, then local light.
   The supplied zero bloom threshold and VFX energies up to 12 can clip bright
   color; halo/local light can preserve impact without raising the core value.
4. Keep generated texture strength modest while judging material identity.
   Large strength values make every broad LEGO form noisy and work against RTS
   readability.
5. Review the new blue background only from a migrated paste, not as an assumed
   accepted value. Its far-field influence now has a visible consequence.
6. Do not evaluate HUD in this fixture. Its visual language, health display and
   selection language need one coherent interface scene later.

## Objective verification boundary

Automated smoke checks schema migration/round-trip, material-role coverage,
texture imports, free-camera bounds, particle fixture existence, emissive
bindings and exterior impact placement. Captures cover post/outline A/B. Human
review remains necessary for texture credibility, bloom hierarchy, luminous
edge appeal, VFX timing, track readability and camera feel.

The full project suite passed with zero blocking failures at
`Artifacts/Verification/20260822T123634Z-full-summary.txt`. The preserved
Stress60 result remains the expected `BLOCKING_LATER — M9` diagnostic failure;
it is unrelated to this presentation-only change.

The final follow-up fast suite, including schema migration and all three Look
Lab fixtures, passed at
`Artifacts/Verification/20260822T124347Z-fast-summary.txt` after confirming
that zero smoke/spark counts emit zero particles.

## T065/T066 technical enrichment

The schema-3 pass adds production animation drivers and VFX pools without
selecting an art direction:

- all four lab units bind six wheel pivots, the suspension pivot and drill pivot
  through the same reusable rig binding used by playable unit views;
- the driver consumes presentation state for movement, function, repair,
  weapon fire, transformation, damage and destruction, with A/every-frame,
  B/30-Hz and C/15-Hz significance updates;
- the lab exposes every new motion-response value independently and provides a
  four-state choreography that does not move authoritative world positions;
- tracer, muzzle and impact evidence uses fixed-capacity prewarmed pools with
  independent live budgets and active/peak/reused/dropped telemetry;
- a smoke probe verifies one-node prewarm, expiry, reuse and cosmetic overflow
  dropping; separate checks verify state transitions, tier cadence, rig socket
  counts and schema-1/schema-2 migration;
- the playable prototype uses the same pool for authoritative projectile views
  and event-deduplicated muzzle/contact-impact bursts.

These checks prove that controls are wired and the systems remain
presentation-only. Human review is still required for animation character,
recoil feel, suspension appeal, effect timing and readable budget limits.
The playable prototype still uses generic primitive placeholder bodies without
named mechanical pivots: it exercises state evaluation, significance and pool
integration, while the imported six-wheel Look Lab rig provides the visible
mechanical proof. Roster-specific rig binding remains part of later content
import, not this infrastructure task.

Full verification passed at
`Artifacts/Verification/20260822T133654Z-full-summary.txt` with zero blocking
failures. The exported schema-3 application was launched directly for both the
controls-visible and clean-view fixtures; both reported four driver bindings,
three pools and 144 prewarmed nodes. Captures are
`Artifacts/Screenshots/m7-exported-look-lab-v3-controls.png` and
`Artifacts/Screenshots/m7-exported-look-lab-v3-clean.png`.

## T067 bounded destruction enrichment

The schema-4 pass adds a fifth independent review domain without selecting its
art direction. Destruction is split into a readable source-body collapse,
scale-banded hero LEGO modules and secondary dust. Hero modules use two
preallocated `MultiMesh` channels and local cosmetic ballistic motion; they are
not individual rigid bodies, do not collide and never affect pathing or damage.
The production view prewarms 16 hero and 16 dust bursts. The lab prewarms 12 of
each, exposes lower active budgets and reports active bursts, visible modules,
peak, reuse and drops.

The unit/structure/alternating preview, manual trigger and copied parameters
make the lab a useful art-direction fixture, not an accepted effect. Human
review is still required for breakup rhythm, fragment scale/count, wreck
silhouette, dust character and whether each faction eventually needs different
destruction profiles.

The complete full suite passed at
`Artifacts/Verification/20260822T142339Z-full-summary.txt`. The freshly
exported macOS app then ran the maximum 18-module Structure preview without
runtime or shader errors and produced
`Artifacts/Screenshots/m7-t067-exported-destruction-structure.png`.

## M7 quality revision

The post-T069 review hardened the laboratories without changing any reviewed
look values or selecting visual canon:

- post styling can now be disabled while the independent depth/normal outline
  and emissive halo remain active; the smoke matrix includes an explicit
  `post=off, outline=on` regression fixture;
- partial or malformed Look/HUD JSON inherits family-specific defaults and
  invalid colors are normalized to safe canonical values instead of leaking
  nulls or accidental fallback colors into shaders;
- pause and animation speed now govern every GPU-particle path, and the pause
  button is synchronized after paste/reset as well as direct toggling;
- live camera, lighting, emission, track and VFX edits no longer rebuild
  unrelated unit or effect materials; effect materials are shared and rebuilt
  only when their actual inputs change;
- per-entity VFX event memory is released when a presentation entity leaves the
  view, keeping long matches bounded;
- HUD actionable-alert state now participates in retained-view invalidation and
  disabled alerts cannot emit an action request.

The complete full suite passed with zero blocking failures at
`Artifacts/Verification/20260822T194330Z-full-summary.txt`: 278 NUnit tests,
all M6/M7 Godot smokes, four Look Lab post/outline/zoom fixtures, 100-repeat
determinism, replay/snapshot continuation and a launchable macOS export. The
preserved Stress60 failure remains the expected `BLOCKING_LATER — M9`
diagnostic. Exported-build visual evidence for the independent composite path
is `Artifacts/Screenshots/m7-quality-revision-outline-without-post.png`.
