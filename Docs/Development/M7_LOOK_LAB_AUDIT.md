# M7 LOOK LAB — VISUAL QUALITY AUDIT

## Status

**NON-CANONICAL LOOK-DEVELOPMENT AUDIT.**

This audit responds to the game-director reviews and supplied schema-1/schema-4
profile. It records what was defective, what changed, which supplied settings
became the new review baseline and where human judgement is still required.

## Defect audit

| Review finding | Root cause | Current result |
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

The first texture pass produced four 1024×1024 grayscale review maps:

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

## Schema-5 material, lighting and presentation revision

The supplied schema-4 profile is now the reset baseline and migrates without
losing any reviewed value. The revision separates previously ambiguous controls
and fixes defects revealed only after authored texture maps became visible:

- material controls use surface-character presets and perceptual labels instead
  of raw Blender terminology; the UI identifies the map used by every family;
- generated regolith, bolted-machine and building-panel maps drive tinted albedo,
  height-derived normals and gloss breakup, while a transparent glare sprite
  drives muzzle/impact particles; stable mip selection prevents RTS-scale
  sparkle;
- terrain no longer emits into bloom, and its old sinusoidal/tiled color noise is
  replaced by anti-tiled authored detail plus non-repeating domain-warped broad
  patches;
- signal and Crystal pulse are independent; static lamps do not pulse by
  default; fire has direct illumination/reach controls and smoke renders behind
  the luminous flame;
- the chassis receives restrained suspension while wheel centers remain planted;
  generic unit-hit wobble is removed, recoil uses the named muzzle after layout,
  and only the correctly oriented unobstructed shooter fires;
- destruction hides the intact source and emits bounded LEGO modules rather
  than shrinking it under the terrain;
- the misleading fog proxy is removed from this material/lighting fixture;
  production fog remains a separate gameplay/readability test;
- a presentation-only World Light Cycle section covers Earth, Mars, Moon,
  authored Planet U and fixed Underground, including natural color transitions,
  authored light/dark duration ratios and explicit readability/local-light
  safeguards.

These are laboratory capabilities, not selected art direction. Planet U's
lighting is explicitly an authored experiment rather than astronomical canon.

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

## Schema-6 art-director simplification

The follow-up review identified two artifacts that looked related but had
different causes. Large stepped black bands were low-sun lighting across the
2.5-metre triangles of an overly wavy 600-metre terrain mesh; small grain around
real unit shadows came from variable-penumbra sampling over a fixed 180-metre
shadow distance. The terrain now uses broad low-amplitude relief and a neutral
low-frequency height source. Directional shadows use four blended,
camera-fitted cascades, stable filter blur and a technical near-horizon fade.

The normal Materials workflow is now deliberately small: select a role and its
base color. Painted coating, coated brown structure, accent coating, blackened
mechanisms, tool steel, rubber and building panels own curated physical and
texture recipes. The brown chassis no longer uses quarry stone detail, and the
misleading generic surface presets/raw shader sliders are absent from the
art-director UI. Automated captures retain internal inspection passes.

Earth blue hour now spans the wider -24° to -5° solar-altitude band and golden
hour runs from 2° to 32°; the same evaluator preserves each world's authored
palette and light/dark ratio. Functional lamps transition on through civil
twilight, are off at noon and remain enabled Underground. Signals and Crystals
remain semantically emissive, while daytime local-light pools are suppressed.

The complete full suite passed with zero blocking failures at
`Artifacts/Verification/20260825T194320Z-full-summary.txt`. The expected
Stress60 result remains the existing `BLOCKING_LATER — M9` diagnostic. Current
visual evidence is `m7-authored-materials-earth-noon.png`,
`m7-blue-hour-auto-lights.png`, `m7-golden-hour-extended.png` and
`m7-shadow-fix-earth-0620.png` under `Artifacts/Screenshots/`.

## Schema-6 raster, local-light and sky follow-up

The latest review addressed the remaining cases where controls existed but the
rendered evidence did not prove their effect:

- each vehicle now owns two forward/downward `SpotLight3D` headlights plus one
  compact work light; their beam direction, ground intersection, range,
  attenuation and dusk/noon gate are structural smoke assertions;
- headlight and work-light lenses fade with environmental darkness. Status
  signals and inherently luminous Crystals remain separate semantic roles;
- local lights use real cone/range attenuation in the authored surface shader.
  The directional readability shadow floor no longer leaves residual local
  light across an entire clustered-light volume;
- only the key directional light contributes a procedural sun. Fill and
  camera-relative rim lights are scene-only, so camera orbit cannot rotate sky
  reflections;
- at that revision, raster checks began inspecting the exact loaded shader
  parameters rather than only asserting that PNG files exist. Paint, brushed
  metal, rubber, machine-panel bump, then-active building-panel bump, ground
  albedo, ground bump and roughness routes were covered. The building route is
  superseded by the raster-frequency follow-up below;
- the quiet regolith map supplies ground colour while independently transformed
  quarry samples supply shallow bump and reflection breakup. The older detailed
  `regolith_height.png` remains an audit asset but is intentionally not bound in
  the normal view because its recognizable ridges tile at RTS scale;
- blue hour remains classified at 4.5/5.0/5.5 and golden hour at
  6.5/7.2/7.8 in the deterministic Earth review clock. Procedural sky and
  twilight luminance make those extended phases visibly distinct without a
  separate warm post filter.

Current review evidence is `m7-blue-hour-headlights.png`,
`m7-golden-hour-gradient.png`, `m7-raster-materials-close.png` and
`m7-raster-materials-integrated.png` under `Artifacts/Screenshots/`. Visual
acceptance remains the game director's decision.

## Raster frequency and model-space follow-up

The next close/gameplay comparison showed that simply proving a PNG binding was
not enough. Large one-piece building meshes displayed the raster strongly while
the drill rigs looked almost flat until extreme zoom, and the visible building
response read as fine noise rather than material identity. Two independent
causes were present:

- paint colour and relief reused the same mostly high-frequency source, so bump
  and roughness carried more visible information than albedo;
- every imported LEGO sub-mesh sampled around its own local origin, repeatedly
  showing nearly the same small part of the map instead of composing one texture
  field across the complete vehicle.

Painted hull, coated structure, accent and building shell now use
`painted_shell_macro_v2.png` as a broad low-frequency albedo source. The earlier
`painted_shell_detail.png` remains a separate, strongly attenuated relief source;
machine panels, steel and rubber retain their role-specific maps. The fine
building-panel height map is deliberately unbound: at gameplay scale its panel
frequency read as a screen-space grid, so building shells reuse the broad macro
map at very low relief strength without groove darkening, with lower colour
contrast than the smaller vehicle parts. Each channel has its
own derivative multiplier for stable mip selection,
and the higher-frequency reflection octave was replaced with a broader sample.
Model children receive one cached bind-pose origin and full basis so the macro
pattern spans the assembled rig instead of restarting or changing axis on every
piece. Because the transform is captured before animation, changing a material
cannot shift the raster phase to the current wheel, suspension or drill pose.
This avoids the previous embossed/static-like building surface while keeping
the generated paint variation visible in the albedo channel.

The generated macro map is look-development data, not approved visual canon.
Current objective evidence is `m7-materials-final-color.png`,
`m7-materials-final-zoom24.png`, `m7-materials-final-zoom39-5.png`,
`m7-materials-final-zoom72.png`, `m7-materials-final-relief.png` and
`m7-materials-final-integrated.png` under `Artifacts/Screenshots/`. The normal
combined view remains deliberately cleaner than the isolated colour/relief
diagnostics. At the 72-cell strategic limit the macro layer is expected to
filter almost completely rather than alias; human review should judge whether
the 24–39.5-cell broad paint variation is strong enough without reintroducing
gameplay-scale shimmer.
