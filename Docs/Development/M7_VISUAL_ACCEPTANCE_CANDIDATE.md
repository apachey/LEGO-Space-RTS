# M7 visual-direction acceptance candidate

Status: **M7 visual direction accepted by the game director**.

The accepted production baseline is **M7 Final with screen-space outline on**.
The three-way composite proof remains available as a comparison and regression
fixture for Phase 09C. It deliberately reuses the final M7 Look Lab code, HUD Lab,
material, lighting, animation, VFX and LEGO-destruction foundations. The four
bounded code-native models are review prototypes, not substitutes for the
production assets required by T081–T091.

## Launch

1. Launch `Builds/macOS/LEGO Space RTS.app`.
2. Press `F8`.
3. Select **Review M7 visual direction**.

The candidate can also be launched directly from the project with
`--m7-acceptance-candidate`. `Escape` returns to the playable opening.

## What is represented

The same four-faction scene can be switched without relaunching:

- The scene starts in **M7 Final**, with outline on, at a wide **84-cell** view.
- The mouse wheel provides continuous zoom from 24 to 108 cells.
- `5` **Current** preserves the original acceptance-candidate presentation.
- `6` **M7 Final** uses the exact final M7 schema-9 Earth Hybrid Surface stack:
  the Look Lab terrain mesh, procedural sky, lighting, linear tonemapping and
  screen-space post implementation.
- `7` **Hybrid** keeps the final M7 terrain, sky and screen-space post while
  retaining the current candidate's Filmic response.
- `O` independently switches the final M7 screen-space outline on or off in
  every mode. Outline state is not bundled into any of the three directions.

- Rock Raiders: set 4970 **Chrome Crusher**, with teal/brown industrial mass,
  wheels, drill, work light and exposed mechanical construction.
- Astronauts: set 7647 **MX-41 Switch Fighter**, with white/blue/orange clean
  aerospace construction, blue canopy, wheels and transforming wings.
- Mars Mission Aliens: set 7646 **ETX Alien Infiltrator**, with a black/lime
  low alien silhouette, lime canopy, claws and resonance light.
- Life on Mars Martians: set 7313 **Red Planet Protector**, with a tall
  blue/sand-blue/red walking-machine silhouette, orange canopy and articulated
  arms.
- Neutral Hybrid Surface terrain, current role-authored M7 materials, the final
  M7 Earth world-light and screen-composite implementations, mechanical animation,
  combat shot/impact VFX, pooled LEGO debris/destruction and the current
  Hybrid Vector + Raster HUD with the legal minimap.

Source labels can be hidden with `L` so silhouette and palette recognition can
be judged without assistance. The proof preserves the three source themes; it
does not assert that any of these prototypes is final production geometry.

## Review controls

- Mouse wheel: continuous zoom between 24 and 108 cells.
- `Z`, `X`, `C`: canonical close/combat/strategic widths of 24/44/72 cells.
- `5`, `6`, `7`: Current, exact M7 Final, or Hybrid visual treatment.
- `O`: independent screen-space outline on/off.
- `0`: four-faction overview.
- `1`–`4`: focus Rock Raiders, Astronauts, Aliens or Martians and switch the
  representative faction HUD state.
- `V`: replay representative combat VFX.
- `D`: destroy the focused prototype; it restores automatically.
- `Space`: pause/resume animation for silhouette inspection.
- `H`, `L`, `Tab`: toggle HUD, source labels or review panel.

## Game-director checklist

The direction has been approved. These questions remain the production
regression checklist; automated checks only establish that the scene runs and
contains the intended systems.

1. **World/material/lighting:** Preserve M7 Final with outline on as the
   production baseline. Current and Hybrid are comparison modes only.
2. **Zoom/readability:** Do silhouette, faction color and important function
   survive the 84-cell starting view, continuous wheel zoom, and the exact
   24/44/72 camera checks?
3. **Four-faction differentiation:** Are the four sources recognizably distinct,
   LEGO-built and protected from a generic blended sci-fi treatment?
4. **HUD/minimap:** Is Hybrid Vector + Raster the preferred production HUD
   direction, and does its legal minimap coexist with the world view?
5. **Animation:** Does the planted mechanical motion and visible function feel
   right as the production animation language?
6. **VFX:** Is the restrained tracer/muzzle/impact language clear at gameplay
   scale without replacing unit readability?
7. **LEGO destruction:** Does the blast, dust and bounded colored LEGO breakup
   establish the intended destruction character?

The game director accepted M7 Final with outline on and requested the 84-cell
starting view plus continuous scroll zoom. This records the Phase 09C visual
direction dependency; it does not accept the prototype geometry as final art.
T081 has not started in this change.
