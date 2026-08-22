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
- canonical 24–72 build-cell zoom range, defaulting to the reviewed 35;
- free orbit, pan and zoom plus exact camera sliders and JSON focus coordinates;
- four instances of the same 7,776-triangle Raider carrier facing different
  directions;
- one intact LEGO-construction-language building and one smaller burning
  building;
- one unit continuously firing at the intact building;
- particle muzzle flash, emissive tracer, exterior-face impact, sparks, luminous
  fire and smoke evidence;
- rough ground, tracks, a Crystal cluster and a soft fog-of-war preview;
- no HUD, selection marker or health-bar fixture: interface art direction is a
  separate future review.

This is a presentation-only scene. It does not run or modify authoritative
gameplay simulation.

## Live controls

The left panel contains nine sections:

1. **Scene & Camera** — zoom, pitch, yaw, animation speed and scene evidence
   visibility;
2. **Shading** — diffuse wrap, shadow floor, smooth/banded lighting, specular
   scale, material rim and light tints;
3. **Materials** — separate editable families for painted hull, structural
   earth, accent, dark mechanisms, tool steel, rubber, building shell and
   ground rock, each with generated detail-texture strength and scale;
4. **Glass & Emission** — non-emissive optical glass separated from signal,
   lamp and Crystal emission, same-hue edge darkening, halo and local-light
   response;
5. **Lighting** — key, fill, ambient and rim color/direction/energy plus shadow
   softness and opacity, plus far-field background influence;
6. **Post FX** — master enable, tonemapper, exposure, brightness, contrast,
   saturation, temperature/tint, bloom, vignette, grain, sharpen, posterize and
   static grain and static posterization dither;
7. **Outline** — independent master toggle, color, pixel width, opacity,
   depth/normal thresholds, silhouette/crease weights and distance fade;
8. **VFX** — tracer, particle muzzle/impact, spark, luminous fire and smoke
   appearance;
9. **Ground** — two-color macro/micro variation, depth-tested tread tracks and
   unit spacing.

The surface families intentionally begin with distinct physical responses:
painted shell, dusty brown structure, metallic mechanisms and tools, matte
rubber, coated building shell and rough rock are not one material recolored.

## Profile workflow

- **COPY ALL JSON** copies a complete `schemaVersion: 2` profile, including the
  current zoom. It never copies only a diff.
- **PASTE & APPLY** validates the schema, migrates schema 1, ignores the removed
  HUD/scorch fields, clamps unsafe values and applies the complete profile live.
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

Source-render capture is provided by `tools/capture-m7-look-lab.sh`.

## Emission and outline implementation

The discarded inverted-hull pass is not used. The Look Lab samples the Forward+
depth and normal/roughness buffers in a fullscreen pass. Depth discontinuities
control silhouettes; normal discontinuities control creases. High-roughness
terrain suppresses crease lines so the toggle does not turn the ground into
noise. Emissive surfaces write actual HDR emission, receive a darker same-hue
edge, drive a separate bright-pixel halo and have optional nearby lights. The
outline toggle can be evaluated without switching any other look parameter.

## Texture scope

Four generated, grayscale, tileable look-development textures distinguish
painted shell, brushed metal, rubber and quarry ground. They are deliberately
low-detail modulation maps rather than baked lighting or photographic albedo.
Texture strength and triplanar scale remain material-family controls. They are
review assets, not accepted production normal/ORM maps.

Two attempts to expose a live ground normal-strength control produced invalid
terrain rendering in the Look Lab. In accordance with the two-attempt stop
rule, that control is removed rather than left misleading. Normal/ORM authoring
remains a later material-pipeline task if the texture direction is accepted.

## Decision boundary

Automation validates scene structure, mesh/triangle counts, schema-2 round-trip,
schema-1 migration, zoom bounds, generated-texture presence, emissive material
bindings, particle fixtures, exterior impact placement, shader compilation and
captures. It cannot accept visual style, material appeal, readability, VFX feel
or camera feel. Those remain the game director's decision, and copied profile
JSON is the exact handoff for the next implementation iteration.

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
