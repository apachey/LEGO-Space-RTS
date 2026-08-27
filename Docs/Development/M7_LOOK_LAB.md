# M7 — REALTIME LOOK LAB

## Status

**NON-CANONICAL ART-DIRECTION TOOL.**

The Look Lab does not define a visual style and contains no approved style
presets. It provides one continuous, editable renderer profile so the game
director can search the visual space in the actual Godot renderer and return an
exact machine-readable result.

The older Style Lab and Material Lab remain reproducible engineering fixtures.
They are no longer the primary art-direction workflow.

## Review scene

The lab evaluates the look at actual RTS scale rather than a close hero view:

- perspective camera with 36-degree vertical FOV;
- canonical 24–72 build-cell zoom range, defaulting to the supplied 39.5-cell
  review baseline;
- free orbit, pan and zoom plus exact camera sliders and JSON focus coordinates;
- four instances of the same 7,776-triangle Raider carrier facing different
  directions;
- one intact LEGO-construction-language building and one smaller burning
  building;
- one unit continuously firing at the intact building;
- pooled particle muzzle flash, pooled emissive tracer, pooled exterior-face
  impact, sparks, luminous fire and smoke evidence;
- textured rough ground, depressed tracks and a Crystal cluster;
- no HUD, selection marker or health-bar fixture: interface art direction is a
  separate future review.

This is a presentation-only scene. It does not run or modify authoritative
gameplay simulation.

## Live controls

The left panel contains twelve sections:

1. **Scene & Camera** — zoom, pitch, yaw, animation speed and scene evidence
   visibility;
2. **Shading** — diffuse wrap, shadow floor, smooth/banded lighting, specular
   scale, material rim and light tints;
3. **Materials** — color selection for seven role-authored families: painted
   hull, coated structure, accent coating, blackened mechanisms, tool steel,
   rubber and building shell. Their finish, texture, normal and roughness
   response are curated together instead of exposed as interdependent shader
   knobs; terrain remains in **Ground**;
4. **Glass & Emission** — non-emissive optical glass separated from signal,
   lamp and Crystal emission, same-hue edge darkening, halo and local-light
   response plus independent signal and Crystal pulse behavior. Work lamps are
   steady and switch smoothly through dusk, remain off by day and stay on in
   Underground;
5. **Lighting** — key, fill, ambient and rim color/direction/energy plus stable
   four-cascade, camera-fitted shadows and far-field background influence;
6. **World Light Cycle** — presentation-only Earth, Mars, Moon, authored
   Planet U and fixed Underground profiles with clock animation, local time,
   extended blue/golden-hour bands, light/dark period lengths, night
   readability and local-light boost;
7. **Post FX** — master enable, tonemapper, exposure, brightness, contrast,
   saturation, temperature/tint, bloom, vignette, grain, sharpen, posterize and
   static grain and static posterization dither;
8. **Outline** — independent master toggle, color, pixel width, opacity,
   depth/normal thresholds, silhouette/crease weights and distance fade;
9. **Animation** — sim-driven locomotion blend, planted wheel roll, independent
   chassis suspension/lean, functional drill, recoil, transformation pose and
   animation-significance tier preview;
10. **Destruction** — intact-source disappearance, scale-banded hero LEGO-module
   count and motion, secondary dust, fixed pool budgets, live
   telemetry, manual trigger and unit/structure/alternating loop targets;
11. **VFX** — prewarmed tracer/muzzle/impact budgets and live pool telemetry,
   load-preview emitter count, tracer, particle muzzle/impact, spark, luminous
   fire and smoke appearance;
12. **Ground** — non-repeating two-color macro/mineral variation, depth-tested
   bump/depression tread tracks and unit spacing.

The surface families intentionally begin with distinct physical responses:
painted shell, dusty brown structure, metallic mechanisms and tools, matte
rubber, coated building shell and rough rock are not one material recolored.

## Profile workflow

- **COPY ALL JSON** copies a complete `schemaVersion: 6` profile, including the
  current zoom. It never copies only a diff.
- **PASTE & APPLY** validates the schema, migrates schemas 1–5, ignores the
  removed HUD/scorch fields, clamps unsafe values and applies the complete
  profile live.
- **RESET SECTION** restores the active section only.
- **RESET ALL** restores the neutral lab baseline.
- **PAUSE / RESUME** freezes motion for comparisons.
- **HIDE · TAB** hides the editor panel and reframes the scene; `Tab` restores
  it.
- right-drag orbits; middle-drag, `WASD` or arrows pan; wheel zooms; `Q/E`
  rotate; `F` resets the complete view.
- `Escape` returns to the playable prototype.

The runtime entry is `F8` → **M7 Look Lab**. Automation may launch it with:

`--m7-look-lab --m7-look-smoke --m7-look-controls visible|hidden --m7-look-zoom 24..72`

Automated lighting evidence can additionally select
`--m7-look-world manual|earth|mars|moon|planet-u|underground` and
`--m7-look-time 0..24`.

Source-render capture is provided by `tools/capture-m7-look-lab.sh`.

## Emission and outline implementation

The discarded inverted-hull pass is not used. The Look Lab samples the Forward+
depth and normal/roughness buffers in a fullscreen pass. Depth discontinuities
control silhouettes; normal discontinuities control creases. High-roughness
terrain suppresses crease lines so the toggle does not turn the ground into
noise. Emissive surfaces write actual HDR emission, receive a darker same-hue
edge and drive a separate bright-pixel halo. Vehicle headlights are
forward/downward spot lights, work lamps use compact local pools, and
fire/impact/Crystal sources retain their own lighting roles. Functional vehicle
lights fade in through twilight and are off at noon. The outline toggle can be
evaluated without switching any other look parameter.

## Texture scope

Ten generated review assets remain reproducible. Eight are active: broad painted
macro-albedo, also reused at very low strength for broad building relief, and
separate restrained vehicle-paint relief; brushed metal and rubber detail;
machine-panel height-derived bump; quiet regolith colour; independently
transformed quarry bump/roughness; and an alpha glare sprite. The older detailed
`regolith_height.png` and fine `building_panel_height.png` remain for audit
history but are deliberately unbound
because its large ridges repeat at RTS scale. Albedo, height/bump and roughness
sampling use separate coordinates and calibrated contrast instead of stamping
one grayscale value into every channel. Imported LEGO sub-meshes use their
cached bind-pose origin and basis to share one model-space texture field;
derivative-aware mips avoid sparkle without allowing live animation or a colour
edit to shift the raster phase.
These are review assets, not accepted production normal/ORM maps.

## T065 animation-driver integration

The four imported rigs use the same `PresentationAnimationDriver` and
`PresentationAnimationRigBinding` classes as the playable `UnitViewManager`.
The driver reads presentation state, never mutates authoritative simulation and
supports the canonical significance cadence: Tier A every render frame, Tier B
at approximately 30 Hz and Tier C at approximately 15 Hz. Stable state inputs
cover locomotion, functional operation, repair, weapon recoil, normalized
transformation progress, damage and destruction.

The laboratory's four-state choreography is synthetic presentation evidence
only. It holds world positions fixed while showing locomotion mechanics,
functional drill motion, combat recoil and transformation response on
separate identical rigs. Disabling choreography returns all rigs to normal idle
except authored firing evidence.

## T066 VFX-pool integration

Tracer, muzzle and impact effects are prewarmed and reused rather than allocated
per shot. The laboratory prewarms 64 tracer, 32 muzzle and 48 impact nodes and
exposes smaller active budgets independently. When a budget is exhausted, only
the cosmetic spawn is dropped; simulation, projectile travel and damage are not
affected. The VFX panel reports active, peak, reused and dropped counts live.

The playable prototype consumes the same pool implementation for 96 projectile
views, 24 particle muzzle bursts and 32 particle impact bursts. Per-source
weapon-fire high-watermarks prevent duplicate cosmetic events when the same
snapshot or restored state is observed again.

## T067 LEGO-destruction integration

Gameplay destruction still becomes authoritative immediately at zero HP:
selection, collision, navigation and gameplay function are removed by SimCore.
The new visual phase begins only after that transition and cannot feed state
back into simulation.

The playable prototype and Look Lab share a bounded destruction driver and two
prewarmed cosmetic pools. The intact source is hidden rather than scaled into
or below the ground. A `MultiMesh` burst throws only scale-banded hero modules (3/5/7/10/14/
18 for Tiny through Structure), split between plate/beam shapes and round
mechanical parts. Their ballistic motion, bounce, settle and fade are local
presentation calculations with no physics bodies or colliders. A second pooled
GPU-particle burst supplies optional dust. Pool exhaustion drops only the
cosmetic burst.

The lab can target the fourth unit, the burning structure or alternate between
them. Fire/smoke/light evidence follows the structure. Fragment motion, count,
lifetime, fade, dust value and active pool budget are editable and copied in
schema 6. Existing schema 1–5
profiles inherit the neutral destruction defaults.

## Decision boundary

Automation validates scene structure, mesh/triangle counts, schema-6 round-trip,
schema-1 through schema-5 migration, animation and destruction-driver state,
six-wheel/drill/suspension rig binding, all six pool prewarm/spawn/reuse/drop
behavior, zoom bounds, exact authored-texture shader routing, directional
headlight geometry and day/night gating, procedural-sky light ownership,
emissive material bindings, exterior impact placement, shader compilation and
captures. It cannot accept visual style,
material appeal, readability, animation feel, VFX feel or camera feel. Those
remain the game director's decision, and copied profile JSON is the exact
handoff for the next implementation iteration.

The detailed defect audit and recommendations are in
`M7_LOOK_LAB_AUDIT.md`.

Full verification passed on 2026-08-22 at
`Artifacts/Verification/20260822T123634Z-full-summary.txt`. The freshly
exported macOS app then launched directly into schema 2 and captured:

- `Artifacts/Screenshots/m7-exported-look-lab-v2-controls.png` — controls
  visible, zoom 35, post on, outline on;
- `Artifacts/Screenshots/m7-exported-look-lab-v2-outline-off.png` — clean game
  view, zoom 35, post on, outline explicitly off.

Both exported paths reported four units, 192 unit meshes, 31,104 unit
triangles, two buildings and active firing/burning evidence.

After the final zero-count smoke/spark control correction, the complete fast
suite passed at `Artifacts/Verification/20260822T124347Z-fast-summary.txt` and
the macOS app was exported and directly captured again.

The schema-3 T065/T066 integration then passed the complete full suite at
`Artifacts/Verification/20260822T133654Z-full-summary.txt`. The freshly
exported application itself emitted the schema-3 PASS marker and captured:

- `Artifacts/Screenshots/m7-exported-look-lab-v3-controls.png` — controls
  visible, zoom 35, post on and outline on;
- `Artifacts/Screenshots/m7-exported-look-lab-v3-clean.png` — clean RTS view,
  zoom 35, post on and outline explicitly off.

Both paths reported four animation-driver bindings, three VFX pools and 144
prewarmed VFX nodes in addition to the preserved scene counts.

The schema-4 T067 destruction pass then passed the complete full suite at
`Artifacts/Verification/20260822T142339Z-full-summary.txt`. The freshly
exported application itself emitted the schema-4 PASS marker with five VFX
pools, 168 prewarmed nodes and the 18-module Structure path active. Evidence:
`Artifacts/Screenshots/m7-t067-exported-destruction-structure.png`.
