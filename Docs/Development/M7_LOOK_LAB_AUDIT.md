# M7 LOOK LAB — SCHEMA 2 AUDIT

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
schema 2 after application.

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
