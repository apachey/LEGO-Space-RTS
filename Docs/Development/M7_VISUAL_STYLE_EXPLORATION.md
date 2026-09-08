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

First review outcome:

- none of the four treatments is an accepted direction;
- material realism and graphic toon remain weak candidates worth comparing;
- clean PBR and hand-painted/retro are retained as comparison anchors, not as
  forward art-direction candidates;
- no visual canon is inferred from this ranking.

## Round 2 research-driven range

The game director supplied a non-canonical research brief centered on full 3D
mechanical readability, visible construction, broad functional forms and clean
RTS presentation. The full source brief and reference classification are kept
in `Docs/Development/M7_ART_DIRECTION_RESEARCH_BRIEF.md`.

Round 2 no longer compares unrelated rendering categories. It tests four
interpretations inside the desired center:

1. **Industrial Mass** — grounded weight, restrained painted/metal response and
   low-noise terrain;
2. **Heroic RTS** — stronger silhouette separation, saturation and shaped
   warm/cool lighting;
3. **Constructive LEGO** — molded polymer plus exposed mechanisms, without a
   physical-toy or tabletop premise;
4. **Graphic Volume** — controlled light bands and broad values while retaining
   full 3D volume.

The Round 2 Raider carrier also consumes the accepted palette semantics: its
large chassis and rear housing form an earth-brown mass, while neon-orange
signals and the neon-lime work light are authored emission. Canopy glass remains
non-emissive.

First Round 2 review outcome:

- Heroic RTS is the strongest current direction, but is not accepted or locked;
- Industrial Mass contributes desirable color depth;
- Heroic RTS contributes the preferred color response and bloom;
- Constructive LEGO contributes desirable molded-material highlights;
- the overt warm filtering in Industrial Mass and Heroic RTS is rejected;
- outline must be judged independently rather than being bundled into Graphic
  Volume.

The refinement pass keeps the four treatments stable, moves the Industrial key
light close to neutral, makes the Heroic key only subtly warm and preserves its
cool fill, saturation and bloom. Constructive highlights are retained. Outline
is off by default and toggles with `O` on any treatment, allowing a same-style
A/B comparison without changing terrain, material model or lighting.

Industrial Annihilation is the strongest individual reference. Beyond All
Reason contributes scale and mechanical weight; StarCraft II and Warcraft III
contribute silhouette hierarchy and controlled exaggeration; Planetary
Annihilation contributes clean volumetric graphic treatment. These are analysis
axes, not instructions to copy another game's assets or formula.

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
- Life on Mars astronauts;
- Mars Mission astronauts;
- Mars Mission aliens;
- Martian 7311, 7313, 7314, 7316 and 7317-derived infrastructure.

The percentages are **candidate visible-area targets**, not inventory part-count
ratios and not claims that every source inventory contains that percentage of
plastic by volume. Part count alone is invalid because one large hull or panel
can dominate the image while many pins and connectors barely contribute visible
area. Inventory and instruction evidence identifies which colors coexist; the
assembled model's visible surfaces determine dominance.

| Review group | Candidate visible-area ratio |
|---|---|
| Rock Raiders | dark turquoise 28 / dark industrial gray 22 / earth brown 18 / black 12 / light gray and metal 9 / hazard yellow 5 / luminous neon orange 3 / luminous neon lime 3 |
| Life on Mars astronauts | blue and medium blue 29 / white 22 / gray 20 / black 13 / tan and earth orange 7 / luminous red signals 4 / clear warm lamps 2 / trans-smoke 3 |
| Mars Mission astronauts | white 43 / orange 20 / black 13 / light blue-gray 13 / dark blue-gray 6 / luminous blue signals 5 |
| Mars Mission aliens | black 43 / lime shell 21 / luminous neon lime 14 / dark red and blue 9 / pearl and dark gray 9 / signals 4 |
| Martian aggregate | black 18 / grays 18 / tan 14 / sand families 22 / set-specific primaries 17 / earth orange 5 / luminous red 1 / luminous neon orange 2 / luminous neon lime 2 / luminous blue 1 |

The Rock Raiders correction makes earth brown a major vehicle surface rather
than misclassifying it as background terrain. The prior version omitted this
faction-defining mass and was rejected.

The Martian aggregate is only an overview. The lab's second page preserves five
incompatible source families rather than flattening them into one faction blue:

| Martian source | Candidate visible-area ratio |
|---|---|
| 7311 | sand green 34 / dark green 23 / tan 15 / black 12 / gray 9 / trans-neon orange 7 |
| 7313 | bright blue 30 / sand blue 25 / grays 17 / black 14 / trans-neon orange 8 / white and tan 6 |
| 7314 | bright red 33 / sand red 25 / black 15 / grays 14 / trans green 8 / tan 5 |
| 7316 | tan and beige 42 / black 17 / grays 15 / earth orange 10 / sand red and purple 8 / luminous neon lime 8 |
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

The 7316 correction is intentionally large: beige/tan is the assembled
Excavation Searcher's dominant visible hull area. The discarded ratio was too
close to an inventory-count reading and over-weighted numerous smaller black and
gray elements.

The lab adds two abstract-model pages. Every model uses the same relief
silhouette made from exactly 100 equal front-facing panels. One panel therefore
represents one percent of visible frontal area. Colors grow from core to edge in
the same segment order as the ratio pages, allowing massing to be judged on an
object instead of only as a bar chart.

Transparent samples are shown on light and dark backings. Their semantics are:

- tinted canopy/tube/window: transmission only, no emission;
- status lamp: optional local emission;
- energy channel/resource Crystal: authored emission;
- opaque fluorescent color: never automatically emissive.

Faction light assignments are a separate authored layer:

| Group | Luminous roles |
|---|---|
| Rock Raiders | neon orange / neon lime |
| Mars Mission astronauts | blue |
| Mars Mission aliens | neon lime |
| Life on Mars astronauts | red / warm light through a colorless transparent lens |
| Life on Mars Martians | red / neon orange / neon lime / blue |

These assignments do not make every object of the same hue luminous. The Mars
Mission orange glass remains non-emissive; a bright red Martian hull remains
non-emissive unless a separate signal/light role is assigned.

## Realtime Look Lab pivot

The game director rejected further preset-style comparison because differences
were bundled, difficult to isolate and not representative of the final
gameplay camera. The primary workflow is now the continuous **M7 Look Lab**
documented in `Docs/Development/M7_LOOK_LAB.md`.

It shows four identical units, two buildings, combat/fire evidence and textured
ground under the Phase 07 gameplay camera. HUD and fog presentation are tested
separately rather than contaminating material review. Every
meaningful art-direction variable is exposed independently and the complete
versioned profile can be copied as JSON. The four Round 2 styles remain
historical comparison fixtures, not presets inside the new lab and not current
forward candidates.

Outline has also changed implementation: the old inverted-hull pass is retired.
The Look Lab uses scene depth and normal/roughness buffers, with separate
silhouette and crease controls and rough-terrain suppression.

## Legacy evidence and review

Automated capture must render every style from the same fresh launch and record
model/node counts. Human review decides material appeal, readability, style and
animation feel. The comparison is invalid if geometry, camera or animation
changes between styles.

Implemented controls:

- `F8` → **M7 Style Lab**, then `1` Industrial Mass, `2` Heroic RTS,
  `3` Constructive LEGO, `4` Graphic Volume, `O` outline off/on and `Escape`;
- `F8` → **M7 Palette Lab**, then `1`–`6` and `Escape`;
- pages `3` and `4` are the faction and Martian 100-panel abstract models;
- page `5` tests non-emissive transparency and page `6` the faction light map;
- all four styles contain no HUD/GUI `CanvasLayer`;
- palette labels are `Label3D` content inside the isolated review scene;
- exported-build smoke captures verify both labs outside the editor.

Round 2 evidence from the exported macOS build:

- `Artifacts/Screenshots/m7-exported-style-industrial-mass.png`;
- `Artifacts/Screenshots/m7-exported-style-heroic-rts.png`;
- `Artifacts/Screenshots/m7-exported-style-constructive-lego.png`;
- `Artifacts/Screenshots/m7-exported-style-graphic-volume.png`.

All four paths preserve 48 imported meshes, 7,776 triangles, the shared camera
and zero HUD/GUI canvas layers. The refined Heroic outline A/B was also captured
from the exported application as
`Artifacts/Screenshots/m7-exported-heroic-rts-outline-off.png` and
`Artifacts/Screenshots/m7-exported-heroic-rts-outline-on.png`. Full automated
verification passed on 2026-08-22 at
`Artifacts/Verification/20260822T085432Z-full-summary.txt`. This qualifies the
fixture for subjective art-direction review; it does not select or canonize a
treatment.
