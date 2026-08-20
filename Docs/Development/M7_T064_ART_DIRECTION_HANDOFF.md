# M7 T064 — MATERIAL ART-DIRECTION HANDOFF

## Status

The Godot material-master foundation and isolated Material Lab are implemented.
The values shown in the lab are engineering look-development defaults inside the
canonical Phase 08 ranges. They are not final art-direction acceptance.

The game director owns the final visual choices. Implementation agents own the
material code, asset import, rendering integration, automated smoke and build.

## Canon already fixed

- materials are stylized PBR rather than photographed toys;
- molded polymer roughness remains approximately 0.32–0.50;
- tool metal roughness remains approximately 0.22–0.40;
- rubber is visibly softer and rougher than black polymer;
- transparent polymer is strongly tinted without expensive realistic refraction;
- Crystal remains faceted and readable without bloom, with a restrained core;
- terrain stays quieter than playable factions;
- faction/source palette dominates the body;
- team identity stays on small Identification Tiles, rings and UI rather than
  recoloring the body.

## Prepared fixture

From a normal launch:

1. press `F8`;
2. select **M7 Material Lab**;
3. the prototype reloads directly into the isolated look-development stage;
4. select **RETURN TO PROTOTYPE** to return.

The lab shows:

- five source-palette swatches: Rock Raiders, Astronaut Field, Astronaut Mission,
  Aliens and Martians;
- the six T064 material families;
- team Identification Tiles separated from body color;
- molded-polymer roughness at 0.32, 0.40 and 0.50 under the same fixed light.

Exact swatch RGB values are deliberately labeled draft. Source-set color matching
and subjective material response require game-director review.

## Art-direction response requested

The shortest useful response is:

1. polymer: `0.32`, `0.40`, `0.50`, or “between X and Y”;
2. Alien black: too crushed / correct / too lifted;
3. Astronaut white: too gray / correct / too bright;
4. transparent brown: too opaque / correct / too clear;
5. Crystal: too flat / correct / too emissive;
6. named faction swatches requiring adjustment.

## Texture policy

Clean molded polymer does not need a photographic albedo texture. Its baseline
comes from geometry, color, roughness and lighting. Texture work is reserved for:

- source-authentic prints and decals;
- controlled damage/wear masks near functional contact zones;
- terrain low-frequency variation;
- optional normal/ORM support after a material is visually accepted.

Image generation can produce useful look-development boards and albedo concepts.
It does not by itself prove seamless tiling or generate production-trustworthy
normal/ORM data. Every generated candidate must be checked at real gameplay scale.

## ImageGen prompt — Basalt Highlands look-development tile

```text
Use case: stylized-concept
Asset type: tileable game texture look-development preview
Primary request: seamless Basalt Highlands ground macro-texture for a premium stylized LEGO science-fiction RTS
Scene/backdrop: orthographic top-down surface only, edge-to-edge texture
Subject: broad dark basalt planes, clean angular fracture transitions, very restrained warm mineral variation
Style/medium: stylized PBR-inspired game texture concept, authored and graphic rather than photoreal scanned rock
Composition/framing: square seamless tile, low-frequency forms suitable beneath RTS combat units
Lighting/mood: neutral flat material-reference lighting with no directional cast shadows and no baked highlights
Color palette: charcoal, neutral dark gray, muted brown undertones; low saturation
Materials/textures: matte basalt, subtle planar value changes, minimal micro-noise
Constraints: seamless edges; no focal object; no visible grid; no LEGO studs; no bricks; no units; no UI; no text; no logos; no watermark; environment must stay quieter than playable faction colors
Avoid: photogrammetry, realistic gravel, dense cracks, lava glow, moss, rust, high-frequency noise, dramatic lighting
```

Any generated result remains preview-only and must not be connected to the
project until the game director accepts its direction.
