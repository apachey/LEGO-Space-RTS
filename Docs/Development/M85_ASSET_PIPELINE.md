# M8.5 T081 — PRODUCTION ASSET PIPELINE

**Status:** implementation standard for T082–T092. This document does not
change gameplay or visual canon. Phase 09C and the accepted M7 Final direction
remain authoritative.

## Purpose

Every production unit and infrastructure model must travel through the same
repeatable path:

1. evidence and design package;
2. editable Blender source;
3. deterministic GLB export;
4. validated Godot import;
5. gameplay-camera and state review;
6. explicit game-director acceptance.

The shipping build consumes exported assets only. It never depends on Blender
or live DCC integration.

## Scale and coordinates

- Blender uses metres and applies all object scale before export.
- `1 Blender metre = 1 Godot world unit`.
- `1 authoritative build cell = 2 Godot world units`.
- Blender is Z-up and the model faces positive Y.
- Godot is Y-up and the imported model faces negative Z. Blender's built-in
  glTF exporter maps positive Y to this direction.
- The root origin is the ground-contact centre for units and the authoritative
  footprint centre for infrastructure.
- Root translation and rotation are zero; root scale is `(1, 1, 1)`.
- Art may extend outside a footprint through drills, wings, cranes or effects,
  but selection, placement and movement footprints remain authoritative.
- Imported presentation is never rescaled to repair a wrong source asset.

## Source and export layout

Each player-facing asset owns:

```text
ArtSource/M85/<stable-id>/<stable-id>.blend
GodotClient/Assets/M85/<stable-id>/<stable-id>.glb
Content/Presentation/Assets/<stable-id>.asset.json
```

The sidecar record stores the stable content ID, source classification,
official-set/motif references, provenance, footprint, intended dimensions,
required states, material roles, sockets, LOD budgets and review state. Source
classifications are the canonical `OFFICIAL-DIRECT`, `OFFICIAL-ADAPTED`,
`COMPOSITE-ADAPTATION` and `NEW GAME CONTENT` values.

Generated or project-authored pipeline fixtures must say explicitly that they
are not roster art. They may prove the pipeline but may not satisfy T083/T085.

## Naming contract

- Model root: `Asset_<PascalName>`.
- Geometry: `<Role>_<Importance>_<SemanticName>_<Lod>`.
- Material roles use the accepted runtime prefixes: `Body`, `Accent`, `Tool`,
  `Rubber`, `Glass`, `Signal`, `Lamp`; unprefixed structural mechanics use
  `Neutral`.
- Importance is `Hero`, `Support` or `Micro`.
- LOD suffix is `Close`, `Combat` or `Strategic`.
- LOD roots are exactly `LOD_Close`, `LOD_Combat`, `LOD_Strategic`.
- Moving assemblies use `Pivot_<Function>`.
- Presentation attachment points use `Socket_<Function>`.
- Names are stable API. Renaming a pivot or socket requires updating its asset
  record and validation in the same change.

Required common sockets are:

- `Socket_Selection` at ground centre;
- `Socket_Health` above the readable silhouette;
- at least one interaction/weapon/tool origin appropriate to the asset;
- relevant VFX and audio emitter sockets;
- cargo, production exit, Tube or network sockets where gameplay requires them.

Sockets are presentation references only. They never decide targeting,
collision, movement, fog, damage or any other gameplay truth.

## Geometry and LOD contract

All production assets separate:

- **Hero:** source-defining silhouette, locomotion and functional mechanism;
- **Support:** readable LEGO-derived construction and attachment logic;
- **Micro:** close-view enrichment that may disappear early.

Three authored gameplay LODs are required:

- **Close** preserves production construction and operators;
- **Combat** is the primary production target and keeps all Hero plus necessary
  Support geometry;
- **Strategic** keeps silhouette, locomotion, primary tool/canopy, major colour
  blocks and transformation state.

Strategic geometry may not contain Micro parts. Triangle counts must decrease
strictly from Close to Combat to Strategic. Exact budgets are assigned per asset
in its sidecar after T082 evidence and T083/T085 design review; a universal
triangle count is intentionally not invented before asset complexity is known.
Large silhouette changes use bounded hysteresis and crossfade/dither in the
runtime presentation layer.

## Pivot and socket rules

- Pivots use zero scale and unskewed axes.
- Wheel/track/leg contact rests on the ground plane in the authored rest pose.
- Rotation axes must match the mechanical connection visible in the model.
- Weapon/tool sockets face negative Z after Godot import.
- Health and selection anchors remain usable across every LOD and state.
- Transformation/deployment assets retain the same authoritative root and map
  normalized presentation progress onto named pivots or bones.
- Animation timing consumes authoritative state; animation events never apply
  gameplay results.

## Materials and textures

- Geometry carries semantic material-role names, not final per-instance team
  colours.
- Godot binds the accepted M7 role-authored material family after import.
- Team Identification Tiles remain a separate readable ownership treatment.
- Texture sets must declare purpose, channels, scale, tiling, resolution,
  reduced-LOD behaviour and provenance in the asset record.
- Baked lighting and fake silhouette-defining structure in textures are
  prohibited.
- Transparent saturated polymer does not emit unless the asset record marks a
  real lamp, signal, energy or resource function.

## Godot import lock

Production GLB files use the built-in Godot scene importer:

- root scale `1.0` with root-scale application enabled;
- authored node names preserved;
- tangents ensured;
- authored LODs retained (`generate_lods=false`);
- shadow meshes enabled;
- animation imported at 30 fps with immutable tracks removed;
- embedded images permitted only for the pipeline fixture; production textures
  are separately tracked assets unless the asset record approves otherwise;
- no import script or new plugin dependency.

Godot physics nodes and collision callbacks are not added by the importer.
Presentation collision/debug anchors may exist, but SimCore remains the sole
gameplay authority.

## Representative round trip

T081 owns a deliberately non-roster reference vehicle:

- editable source:
  `ArtSource/M85/PipelineReference/pipeline_reference_vehicle.blend`;
- deterministic generator:
  `tools/blender/generate_m85_pipeline_reference.py`;
- exported runtime asset:
  `GodotClient/Assets/M85/PipelineReference/pipeline_reference_vehicle.glb`;
- sidecar contract:
  `Content/Presentation/Assets/pipeline.reference.vehicle.asset.json`;
- static GLB/sidecar validator:
  `tools/Validation/validate_m85_asset_pipeline.py`;
- Godot import and gameplay-camera fixture:
  `F8` → **Review M8.5 asset pipeline**.

The fixture proves unit scale, forward/up conversion, ground root, three
strictly reducing authored LODs, semantic geometry, named pivots/sockets,
accepted material binding and Blender-to-Godot import. It is not a canonical
unit design and must never be counted toward the 35 production units.
Full verification also regenerates the GLB in a temporary directory and
requires it to match the tracked runtime asset byte for byte.

## Per-asset acceptance checklist

An asset is not production-complete until all applicable items pass:

- Super Scout packet and source ledger are complete;
- design package is explicitly accepted;
- editable source and deterministic/exportable GLB are present;
- sidecar matches stable roster ID and authoritative footprint;
- root, axes, dimensions, hierarchy, pivots and sockets validate;
- Close/Combat/Strategic geometry validates and stays recognizable;
- material and texture plan is integrated without placeholder references;
- required animation, operation, damage, construction and wreck states work;
- near/standard/far gameplay captures and relevant state captures exist;
- provenance/licensing and production ownership are recorded;
- the game director explicitly accepts the presented candidate.

## Dependency boundary

T081 adds no package, plugin, asset library or service. Blender 4.4+ is the
existing DCC used by the project, GLB is the deterministic interchange format,
and Godot 4.7.1 .NET performs the runtime import.
