# M7 — VISUAL STYLE EXPLORATION

## Goal

Select a technically reproducible visual direction through the actual Godot
renderer. No tested style is canon until the game director explicitly locks it.

## Controlled Style Lab

Every style uses the same:

- optimized Raider drill-rig proxy model;
- camera, framing and output resolution;
- animation timing and transform pivots;
- scene scale and object placement;
- semantic color assignments;
- effect events.

Only these may change:

- material response and shaders;
- light/color-grading treatment;
- terrain surface treatment;
- VFX rendering language.

Initial implementable range:

1. clean stylized PBR control;
2. real-material translation;
3. graphic toon shading;
4. hand-painted/retro RTS treatment.

The physical-diorama ImageGen result is not included: it is both outside the
desired direction and a poor proxy for achievable realtime rendering.

## Test-unit budget

The model is a style carrier, not final unit art. It must provide enough form to
test colored body masses, tool metal, rubber, transparent elements, functional
emission and animation without image-generation-level noise.

Target:

- one glTF binary produced reproducibly by a tracked Blender script;
- approximately 7.5k–20k rendered triangles for the close test model;
- deliberately named drill, wheel and suspension pivots;
- no skeleton unless deformation proves necessary;
- no texture dependency for the first style comparison.

Implemented outcome:

- Blender 4.4 source: `ArtSource/M7/raider_drill_rig.blend`;
- reproducible generator: `tools/generate-m7-style-unit.sh`;
- Godot runtime GLB: `GodotClient/Assets/M7/raider_drill_rig.glb`;
- 57 Blender objects, 48 imported mesh nodes and 7,776 rendered triangles;
- named drill, suspension and six wheel pivots animated at the Godot presentation
  boundary;
- semantic body, accent, neutral, tool, rubber, glass, signal and lamp slots.

This proves the local pipeline can produce controlled, optimized hard-surface
RTS models at this detail tier. It does not claim that this first proxy is final
army art or that image-generation-level hero detail is an appropriate target.
Uncontrolled AI mesh generation is deferred: it would add topology cleanup and
LOD uncertainty before the project has identified a concrete modeling gap.

## Palette Ratio Lab

The palette review is separate from the style review. It shows visible-area
ratios rather than equal swatches and keeps player/team color disabled.

Required groups:

- Rock Raiders;
- Astronaut Field;
- Astronaut Mission;
- Aliens;
- Martian 7311, 7313, 7314, 7316 and 7317-derived infrastructure.

The percentages are **candidate visible-area targets**, not claims that every
source inventory contains that percentage of plastic by volume. Inventory and
instruction evidence determines dominant families, unusual accents and which
colors coexist. The targets then deliberately discount hidden pins, axles and
other small connector parts that would otherwise distort an RTS silhouette.

| Review group | Candidate visible-area ratio |
|---|---|
| Rock Raiders | dark turquoise 34 / dark industrial gray 27 / black 19 / light gray and metal 11 / hazard yellow 6 / trans-neon green 3 |
| Astronaut Field | blue and medium blue 30 / white 22 / gray 21 / black 13 / tan and earth orange 7 / red and yellow 4 / trans-smoke and red 3 |
| Astronaut Mission | white 43 / orange 20 / black 13 / light blue-gray 13 / dark blue-gray 6 / transparent signals 5 |
| Aliens | black 43 / lime shell 21 / trans-neon green 14 / dark red and blue 9 / pearl and dark gray 9 / signals 4 |
| Martian aggregate | black 18 / grays 18 / tan 14 / sand families 22 / set-specific primaries 17 / earth orange 5 / transparent accents 6 |

The Martian aggregate is only an overview. The lab's second page preserves five
incompatible source families rather than flattening them into one faction blue:

| Martian source | Candidate visible-area ratio |
|---|---|
| 7311 | sand green 34 / dark green 23 / tan 15 / black 12 / gray 9 / trans-neon orange 7 |
| 7313 | bright blue 30 / sand blue 25 / grays 17 / black 14 / trans-neon orange 8 / white and tan 6 |
| 7314 | bright red 33 / sand red 25 / black 15 / grays 14 / trans green 8 / tan 5 |
| 7316 | black 27 / tan 20 / grays 20 / earth orange 13 / sand red and purple 12 / trans-neon green 8 |
| 7317 | tan 22 / light gray 20 / black 16 / sand purple 13 / sand red 10 / trans-smoke brown 10 / trans green and red 9 |

Research anchors are the BrickLink inventories/instructions for
[Chrome Crusher 4970](https://www.bricklink.com/v2/catalog/catalogitem.page?S=4970-1),
[Solar Explorer 7315](https://www.bricklink.com/catalogItemInv.asp?S=7315-1),
[Mars Mission 7690](https://www.bricklink.com/v2/catalog/catalogitem.page?S=7690-1),
[ETX Alien Mothership Assault 7691](https://www.bricklink.com/catalogItemInv.asp?S=7691-1),
and the Life on Mars source sets
[7311](https://brickset.com/inventories/7311-1),
[7313](https://brickset.com/inventories/7313-1),
[7314](https://www.bricklink.com/v2/catalog/catalogitem.page?S=7314-1),
[7316](https://www.bricklink.com/v2/catalog/catalogitem.page?S=7316-1) and
[7317](https://www.bricklink.com/catalogItemInv.asp?S=7317-1).

Transparent samples are shown on light and dark backings. Their semantics are:

- tinted canopy/tube/window: transmission only, no emission;
- status lamp: optional local emission;
- energy channel/resource Crystal: authored emission;
- opaque fluorescent color: never automatically emissive.

## Evidence and review

Automated capture must render every style from the same fresh launch and record
model/node counts. Human review decides material appeal, readability, style and
animation feel. The comparison is invalid if geometry, camera or animation
changes between styles.

Implemented controls:

- `F8` → **M7 Style Lab**, then `1`–`4` and `Escape`;
- `F8` → **M7 Palette Lab**, then `1`–`3` and `Escape`;
- all four styles contain no HUD/GUI `CanvasLayer`;
- palette labels are `Label3D` content inside the isolated review scene;
- exported-build smoke captures verify both labs outside the editor.
