# M7 HUD Frame Audit

Status: non-canonical development research for the schema-8 HUD Lab.

## Decision being tested

The useful direction is not pure vector UI or an unmodified generated frame.
It is a controlled hybrid:

- raster art contributes bevels, wear, fasteners, hoses, molded seams and other
  material detail that would be wasteful to reproduce procedurally;
- code-native geometry owns responsive rails, section boundaries, clipping,
  hit targets and text/icon layout;
- one complete frame/surface recipe belongs to each of the four playable
  factions;
- Legacy remains visible as an A/B baseline, not as a fifth skin system.

This is still an experiment. It does not approve the frames or establish final
HUD canon.

## Why the schema-7 hybrid looked dirty

The previous renderer treated a complete square frame as though it were a tile
atlas. Repeating the full image along an edge and cropping the last repeat
created beads, clipped ornaments and fragments outside the intended silhouette.
Center ornaments were then cropped again as junction modules, so the same visual
idea appeared twice.

At the same time, panel borders, raster walls, structural spines and separate
interior overlays all tried to describe the same divisions. Bringing decorative
chrome to the front while leaving it unclipped let those systems cover content
and escape panel bounds. The result was not a texture-quality problem alone; it
was an ownership problem.

Schema 8 therefore applies these rules:

1. one renderer owns each divider;
2. raster corners are protected rather than repeatedly re-cropped;
3. vector rails bridge responsive spans between protected raster modules;
4. the lower HUD reads as one continuous chassis, not several framed cards;
5. decoration is clipped and stays behind interaction content;
6. unused ornament is removed instead of being filled with decorative noise;
7. the complete frame and its interior palette change as one faction recipe.

## Reference method

Reference work uses product boxes, catalogs, promotional art and contemporary
games to recover color massing and construction language. It does not copy
logos, screenshots, another game's layout or LEGO packaging graphics into the
HUD.

Useful source groups include:

- LEGO's [7690 MB-01 Eagle Command Base instructions](https://www.lego.com/en-gb/service/building-instructions/7690)
  for Mars Mission human construction and color relationships;
- the original developer's [CrystAlien Conflict portfolio](https://4t2.ca/portfolio/legmam/index.html)
  for official promotional RTS interface context;
- the [Life on Mars 2001 catalog index](https://www.bricklink.com/catalogList.asp?catID=172&catType=C&itemYear=2001)
  and individual set/box photography for visible-area color massing;
- the scanned [2000 UK LEGO catalog](https://images.brickset.com/library/Catalogues/c00uk.pdf)
  for Rock Raiders packaging and product presentation;
- original Rock Raiders PC interface captures for restrained industrial metal,
  pipework and green CRT hierarchy.

StarCraft II remains useful only for the functional hierarchy of a readable
full-width RTS console. A Terran-like blue-black skin is neither a faction
reference nor a default.

Color study must judge visible area and visual importance, not raw part count.
That is why Rock Raiders brown and the dominant beige mass of 7316 cannot be
discarded merely because smaller pieces are more numerous.

## Faction frame briefs

### Rock Raiders

- Core mass: dark industrial grey, dark turquoise and earth brown.
- Accents: restrained hazard yellow; neon orange/lime only where the function
  actually calls for energy or a signal.
- Construction language: mine machinery, braces, pipework, battered service
  plates and cave-worksite equipment.
- Avoid: a generic cyan sci-fi cockpit, clean white aerospace panels or brown
  used only as dirt sprayed over a grey frame.

### Astronauts

- One playable faction, one coherent frame.
- Core mass: white/light-grey structure with medium-blue expedition equipment.
- Accents: controlled Mars Mission orange plus small red/blue signals.
- Construction language: rugged field-science hardware joined with purposeful
  aerospace/mission modules.
- Avoid: splitting Life on Mars humans and Mars Mission humans into two HUD
  identities, or making the frame read as two unrelated halves.

### Aliens

- Core mass: black/charcoal and lime shell/energy, with limited neutral grey.
- Tiny dark-red and blue signals may appear as separate functional details.
- Construction language: resonance nodes, faceted containment and black/lime
  alien machinery.
- Avoid: purple/violet. It is not part of this faction's normal recipe and must
  not return through the frame, shell fill, glow or background.

### Martians

- Core mass: tan, sand-red and blue with pneumatic/mechanical structure.
- Accents: controlled lime energy and source-specific red/orange/blue signals.
- Sand purple may appear locally where Martian infrastructure references
  support it; it must not dominate the complete console.
- Construction language: Aero Tubes, pressure vessels, rounded interfaces and
  heterogeneous but deliberately balanced Martian machinery.
- Avoid: recoloring the Alien frame, or reducing the broad Life on Mars source
  family to a single neon-green identity.

## Raster generation and integration rules

Each generated source must:

- use a real transparent background--never a baked checkerboard;
- contain no text, logos, HUD data, icon labels or environmental scene;
- keep authored detail in explicitly protected corner/module zones;
- provide readable rail attachment points without assuming one fixed aspect;
- emphasize large LEGO-like construction and material cues over micro-detail;
- survive normal RTS scale before being judged at close zoom;
- avoid global glow, vignette or lighting baked into the texture;
- avoid fine high-contrast noise that shimmers after scaling;
- retain enough unornamented span for code-native seams and content density.

The renderer must:

- preserve raster aspect ratio and never stretch a complete frame across one
  axis;
- never repeat a complete square frame as an edge tile;
- use mipmaps/filtering appropriate for the review scale;
- clip every decorative layer to the owning panel;
- keep raster decoration behind labels, icons and buttons;
- render one continuous interior surface rather than several competing cards;
- hide empty command slots instead of filling them with meaningless ornament;
- keep shared warning/danger semantics independent from faction accent color.

## Acceptance checklist for every iteration

Capture all four faction recipes at 16:9, then stress at least one at 21:9 and
4:3. Review at actual gameplay scale before zooming in.

For each capture, verify:

- no ornament crosses the safe area or covers interactive content;
- no repeated beads, half-corners, stray fragments or double dividers;
- frame, shell and accents unmistakably belong to the same faction;
- Alien contains no purple and Rock Raiders retains meaningful brown mass;
- the unified Astronaut frame reads as one designed object;
- Martian source variety remains controlled rather than random;
- minimap, selection, portrait and commands remain immediately understandable;
- raster detail enriches the result without replacing functional hierarchy;
- the same fixture keeps identical geometry in Hybrid and Legacy comparison.

Only after those checks should a generated frame be promoted into the normal
faction-bound recipe. Passing this audit still means "ready for art-direction
review," not "visual canon accepted."
