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
- canonical 24–72 build-cell zoom range, defaulting to 44;
- mouse-wheel zoom plus an exact zoom slider;
- four instances of the same 7,776-triangle Raider carrier facing different
  directions;
- one intact LEGO-construction-language building and one smaller burning
  building;
- one unit continuously firing at the intact building;
- muzzle, tracer, impact, sparks, fire, smoke and scorch evidence;
- rough ground, tracks, a Crystal cluster and a soft fog-of-war preview;
- selection and target markers, health readability, resources, minimap and a
  fake selection panel.

This is a presentation-only scene. It does not run or modify authoritative
gameplay simulation.

## Live controls

The left panel contains ten sections:

1. **Scene & Camera** — zoom, pitch, yaw, animation speed and scene evidence
   visibility;
2. **Shading** — diffuse wrap, shadow floor, smooth/banded lighting, specular
   scale, material rim and light tints;
3. **Materials** — separate editable families for painted hull, structural
   earth, accent, dark mechanisms, tool steel, rubber, building shell and
   ground rock;
4. **Glass & Emission** — non-emissive optical glass separated from signal,
   lamp and Crystal emission;
5. **Lighting** — key, fill, ambient and rim color/direction/energy plus shadow
   softness and opacity;
6. **Post FX** — master enable, tonemapper, exposure, brightness, contrast,
   saturation, temperature/tint, bloom, vignette, grain, sharpen, posterize and
   dither;
7. **Outline** — independent master toggle, color, pixel width, opacity,
   depth/normal thresholds, silhouette/crease weights and distance fade;
8. **VFX** — weapon, impact, spark, fire, smoke and scorch appearance;
9. **Ground** — two-color macro/micro variation, normal strength, tracks,
   scorch and unit spacing;
10. **HUD** — panel treatment, minimap contrast, health indicators, selection
    and target readability.

The surface families intentionally begin with distinct physical responses:
painted shell, dusty brown structure, metallic mechanisms and tools, matte
rubber, coated building shell and rough rock are not one material recolored.

## Profile workflow

- **COPY ALL JSON** copies a complete `schemaVersion: 1` profile, including the
  current zoom. It never copies only a diff.
- **PASTE & APPLY** validates the schema, clamps unsafe values and applies the
  complete profile live.
- **RESET SECTION** restores the active section only.
- **RESET ALL** restores the neutral lab baseline.
- **PAUSE / RESUME** freezes motion for comparisons.
- **HIDE · TAB** hides the editor panel and reframes the scene; `Tab` restores
  it.
- `Escape` returns to the playable prototype.

The runtime entry is `F8` → **M7 Look Lab**. Automation may launch it with:

`--m7-look-lab --m7-look-smoke --m7-look-controls visible|hidden --m7-look-zoom 24..72`

Source-render capture is provided by `tools/capture-m7-look-lab.sh`.

## Outline implementation

The discarded inverted-hull pass is not used. The Look Lab samples the Forward+
depth and normal/roughness buffers in a fullscreen pass. Depth discontinuities
control silhouettes; normal discontinuities control creases. High-roughness
terrain suppresses crease lines so the toggle does not turn the ground into
noise. Outline is off by default and can be evaluated without switching any
other look parameter.

## Decision boundary

Automation validates scene structure, mesh/triangle counts, profile round-trip,
zoom bounds, shader compilation and captures. It cannot accept visual style,
material appeal, readability, VFX feel or HUD balance. Those remain the game
director's decision, and copied profile JSON is the exact handoff for the next
implementation iteration.

Full verification passed on 2026-08-22 at
`Artifacts/Verification/20260822T100418Z-full-summary.txt`. The exported macOS
application was then launched directly into the lab and produced:

- `Artifacts/Screenshots/m7-exported-look-lab-controls.png`;
- `Artifacts/Screenshots/m7-exported-look-lab-game-view.png`.

Both paths reported schema 1, four units, 192 imported unit meshes, 31,104 unit
triangles, two buildings and active firing/burning evidence at zoom 44.
