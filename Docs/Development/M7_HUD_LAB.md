# M7 HUD Lab

Status: implementation and art-direction laboratory for T068/T069. It is not visual
canon.

## Purpose

The HUD Lab separates two decisions that must not be confused:

1. Phase 07 already fixes the information hierarchy and functional screen
   relationships.
2. The game director has not fixed the HUD's shapes, typography, color language
   or ornament.

The production HUD and the laboratory therefore share the same retained Godot
`Control` view and one-way `HudFrame` presentation contract. The laboratory
supplies synthetic legal-knowledge fixtures; production supplies state derived
from the current local simulation/client view. Neither path writes gameplay
components.

Open it from `F8` → **M7 HUD Lab**, or launch:

```text
--m7-hud-lab
```

## Fixed functional skeleton

- top: Ore, Energy, spendable Crystals, Operations Capacity, faction mechanic
  and match state;
- bottom-left: fog-correct north-up minimap and actionable alert access; a
  production alert click centers its associated Energy Domain or Worksite;
- bottom-center: a wide entity/type-group information bay followed by a
  dedicated tactical portrait bay;
- bottom-right: an icon-first canonical 3×4 command card and a compact local
  queue strip;
- temporary layers: compact objective tracker, event feed, Energy Domain
  popover and quick/expanded tooltip.

The minimap is now the retained T069 implementation. It uses the same bounded
view in the laboratory and production: baked terrain, viewer knowledge mask,
batched shape markers, remembered static contacts, alerts/network lines and a
camera viewport polygon. Left-click/drag moves the camera; right-click issues a
ground move or rally point; `Shift` + right-click queues a unit move. The lab
turns those gestures into visible synthetic feedback without writing gameplay.

## Scenario fixtures

Keys `1`–`8` switch deterministic UI states:

1. Rock Raider single heavy unit;
2. mixed 45-unit army grouped into five gameplay types;
3. three-facility aggregate production and queue;
4. Energy Domain brownout with alert and diagnostic popover;
5. Astronaut tactical transformation and objective tracker;
6. Alien Charge, commitment and Surge context;
7. Martian Aero Tube component status;
8. Critical command-structure alert plus expanded tooltip.

These are information-density tests, not proposed visual variants.

## Live controls

The laboratory exposes the current presentation tokens and four art-finish
modes on one invariant geometry:

- **Hybrid · Vector + Raster** is the schema-7 default. The retained Control
  layout, functional sections, hit targets and responsive junction positions
  stay code-native. The complete generated faction edge wall and its protected
  corners form the visible outer frame, because that authored frame is stronger
  than the earlier duplicate vector chassis. In Hybrid it is explicitly
  frame-only: its old dark faction fill and extra vector accent rails are not
  drawn, so the independent surface palette remains visible. The structural
  renderer contributes only a quiet interior field and adaptive functional
  junctions—no second silhouette, shoulders or outer rails. One continuous,
  aspect-cropped console-surface field supplies material detail inside the
  selection and command bays without a row of repeated bordered rectangles.
- **Structural Console** preserves its earlier adaptive vector chassis,
  sculpted shoulders, rails and restrained console-surface sampling exactly.
  It removes Hybrid's outer faction frame and continuous inner raster fields,
  isolating the older structural contribution without changing HUD geometry.
- **Legacy Frames** keeps all five previously generated faction frame families,
  their faction-dark fill and their accent rails intact as the unmodified
  comparison baseline. Hybrid reuses only their raster edge/corner construction;
  it does not inherit the Legacy fill treatment.
- **Clean** removes the structural and legacy decorative layers and exposes the
  functional layout by itself.

The redesign uses the useful composition grammar of a classic full-width RTS
command console—minimap, adaptive selection bay, dedicated portrait and
icon-first command card—without copying another game's art, icons or data. Art
finish changes never alter panel rectangles or hit targets, so the four modes
are a material/rendering comparison rather than different layouts.

Surface color is a separate axis from both faction identity and art finish.
Schema 7 retains the six deliberately broad full-surface families introduced
in schema 6:

- **Light Ceramic** (fresh-profile default): pale mineral/ceramic shell with
  warm orange and teal controls;
- **Warm Sandstone**: earthen brown construction with amber controls;
- **Oxide Workshop**: dark rust-red workshop metal with hot orange controls;
- **Field Olive**: muted green field equipment with yellow controls;
- **Alien Porcelain**: violet porcelain/crystalline surfaces with lime accents;
- **Neutral Graphite**: neutral grey machinery with copper accents.

They replace background, raised, recessed, accent, text and status tokens as a
coherent family while preserving identical geometry. They are exploration
presets, not faction assignments and not visual canon. In particular, the old
dark-navy test baseline is neither the schema-7 default nor an approved project
theme; it survives only when an old profile must be migrated without changing
its appearance, or when the director deliberately enters it as a **Custom**
palette. Editing any individual color marks the profile as **Custom**.

Live controls include:

- responsive preview: 16:9, 16:10, 21:9 and 4:3;
- safe area: 90–100%;
- independent UI and text scale;
- top/bottom region sizes;
- minimap, command and selection widths;
- panel gap, padding, separation, opacity, border and corner radius;
- solid/outlined command buttons and health-state color mapping;
- heading, body and micro text sizes;
- resource labels, hotkeys, portrait, event feed, objective, minimap legend and
  command-cost density;
- background, raised, recessed, accent, primary/muted text, good, warning,
  danger and selection colors.
- minimap marker scale and 10-Hz motion smoothing;
- separate explored/unseen fog opacity and remembered-contact opacity;
- grid, viewport line and alert-pulse strength;
- independent viewport, alert and network visibility toggles;
- terrain-family, owned/allied/enemy/neutral/resource, viewport and alert
  colors.
- five generated faction chrome families with enable, intensity and scale
  controls. **Hybrid** now uses each atlas's complete tiled edge wall,
  protected square corners and authored junction modules as one transparent
  outer frame. It suppresses the atlas renderer's faction-dark interior fill
  and supplemental vector rails, leaving the selected surface family in
  control of the HUD interior. **Legacy Frames** remains available with its
  original fill and rails for direct comparison. Rock Raiders uses
  turquoise/brown industrial framing; Mars
  Mission Astronauts use ivory/orange aerospace framing; Mars Mission Aliens
  use black/lime/violet crystalline framing; Life on Mars Astronauts use
  sand/red/blue retro-pneumatic framing; and Life on Mars Martians use
  red/tan/blue/lime organic-pneumatic framing. These experimental skins preserve
  the same functional anchors and do not copy another game's layouts or assets.
- an original `console_surface_v1.png` industrial surface. Structural Console
  retains its fixed tiled sampling. Hybrid crops one broad authored source
  field to each selection/command bay's aspect ratio and then scales it
  uniformly; it neither stretches the pixels nor repeats a line of identical
  framed plates. Raster seams, vents and fasteners therefore supplement the
  code-owned functional separators and hit targets instead of becoming a
  stretched HUD screenshot.
- a code-native tactical portrait inside the selection section. It draws a
  faction-coloured blueprint display with distinct single-unit, grouped-force,
  structure and transforming-unit silhouettes, plus a concise caption. This
  replaces the text-only art placeholder and gives the retained layout a useful
  visual hierarchy while authored roster portraits remain future content.

`Tab` hides the laboratory controls for a clean evaluation.

## JSON handoff

**COPY JSON** places the complete schema-7 `M7HudProfile` in the clipboard.
**PASTE JSON** accepts the same profile, normalizes all values and applies it
live.

- Schema 1–4 profiles migrate to **Legacy Frames** + **Custom**, preserving the
  generated-frame and old dark-surface result authored before the finish switch
  existed. Early schema-4 `frameOpacity`/`frameThickness` experiments still
  migrate once to `chromeIntensity`/`chromeScale` and are omitted on the next
  copy.
- Schema 5 preserves an explicitly stored Structural/Legacy/Clean finish. A
  schema-5 profile without a finish uses **Structural Console**. It migrates to
  **Custom**, and omitted color tokens receive the former schema-5 defaults so
  loading an old profile does not silently recolor it.
- Schema 6 already stored Hybrid, palette and color tokens. Loading it preserves
  those values exactly and upgrades only the renderer contract to schema 7:
  Hybrid now combines the full raster outer frame with the cleaner code-native
  interior. A schema-6 → schema-7 → JSON round-trip is covered by the lab smoke.
- A fresh schema-7 profile starts in **Hybrid · Vector + Raster** + **Light
  Ceramic**. Named palette buttons replace the full surface token family;
  individual color editing switches the palette marker to **Custom**.

Unknown fields are ignored; unsupported schema versions fail visibly.

The profile is intentionally independent from `M7LookProfile`: world rendering
and interface art direction can be reviewed and versioned separately.

## Automation

Headless fixture validation:

```text
Godot --headless --path GodotClient -- \
  --m7-hud-lab --m7-hud-smoke \
  --m7-hud-scenario mixed-army --m7-hud-aspect 21-9 \
  --m7-hud-finish hybrid --m7-hud-palette sandstone
```

Capture helper:

```text
./tools/capture-m7-hud-lab.sh \
  Artifacts/Screenshots/m7-hud-lab-mixed-army.png \
  mixed-army 16-9 hidden hybrid light
```

The capture helper arguments are:

```text
capture-m7-hud-lab.sh OUTPUT SCENARIO ASPECT CONTROLS FINISH PALETTE
```

`FINISH` accepts `hybrid`, `structural`, `legacy` or `clean`. `PALETTE`
accepts `light`, `sandstone`, `oxide`, `olive`, `alien`, `graphite` or
`custom`. Defaults are `hybrid` and `light`.

The main verification suite exercises Hybrid mixed selection with four very
different surface families, Structural/Legacy/Clean comparison cases, and
Hybrid production, brownout and critical-tooltip fixtures across four aspect
ratios. It verifies identical Hybrid/Structural/Legacy/Clean primary geometry
and identical primary geometry across all six named palettes. The smoke also
verifies all eight fixture contracts, client-legal minimap knowledge,
remembered-static rules, pixel/cell command conversion, the 12-slot command
bound, required retained nodes, the procedural tactical portrait, all five
imported faction frames, protected square corners/modules, tiled raster walls,
bounded aspect-preserving raster surface fields, one continuous bottom-deck
chassis, the console-surface binding,
Hybrid frame-only composition, unchanged Structural shoulders, schema 1–6
migration, schema-7 JSON round-trip and safe-area
containment of every major panel plus every visible command button (including
the twelfth slot in the 12-command mixed-army fixture). It also proves that the
Energy readout's children cannot intercept its button input. Default-profile
fixtures cover 16:9, 16:10, 21:9 and 4:3; arbitrary combinations of maximum
layout controls are intentionally not presented as a supported boundary case.
Typography, panel opacity, spacing, padding and command fill are applied
directly, with smoke coverage for the solid/outline command-button switch.
Minimap and command widths are explicitly labelled as targets: the continuous
deck constrains them only when required to stay inside the selected safe area.
The laboratory reports this distinction instead of silently scaling all other
art-director values down with the viewport.

Current schema-7 evidence under `Artifacts/Screenshots/` is:

- `m7-hud-hybrid-frame-light-schema7.png` shows the Mars Mission Astronaut
  raster frame around Light Ceramic while the wide inner material field remains
  continuous and palette-owned;
- `m7-hud-hybrid-frame-alien-graphite-schema7.png` shows the same composition
  with the much darker Alien frame and Neutral Graphite interior, proving that
  Hybrid's frame does not force a common navy fill;
- `m7-hud-paired-hybrid-mixed-light-schema7.png` and
  `m7-hud-paired-legacy-mixed-light-schema7.png` are the honest A/B pair: same
  mixed-army fixture, Rock Raiders atlas, Light Ceramic palette, 16:9 geometry
  and camera. Their panel rectangles are identical; only Hybrid's continuous
  material field and code-native interior junction treatment differ;
- `m7-hud-structural-graphite-v1.png` isolates the Structural finish in neutral
  graphite;
- `m7-hud-final-legacy-mixed-16-9-v2.png` and
  `m7-hud-final-clean-mixed-16-9-v2.png` remain retained comparison evidence for
  the old complete-frame treatment and undecorated functional skeleton. They
  are comparison baselines, not claims that either is the selected direction.

## Deliberately deferred

- final HUD visual canon and acceptance of any generated faction framing;
- final authored command icons, roster portrait art, typography and
  localization assets; the current procedural glyphs/blueprints are functional
  visual scaffolding rather than approved final art;
- T073 complete command catalog and every target-mode button path;
- attack-move-on-minimap and networked `G` team pings, because their target-mode
  and command transport belong to T073 rather than being invented in T069;
- final alert animation/audio, health-bar art and accessibility settings menu.

Disabled command slots in the current production prototype identify T073
catalog work; existing contextual world commands and already implemented Stop,
State Change, production and Energy-priority actions remain functional.
