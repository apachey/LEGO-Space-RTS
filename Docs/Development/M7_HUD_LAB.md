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

- **Hybrid · Vector + Raster** is the schema-6 default. Adaptive code-native
  silhouettes, bays, shoulders, rails and dividers own the layout. Raster art
  is used only where it adds material detail: fixed regions of the original
  console surface become bounded plates inside the selection and command bays,
  while small protected regions of the selected faction atlas become corner,
  side and functional-junction modules. The modules remain square and the
  plates keep their source aspect; neither is stretched across an arbitrary
  panel.
- **Structural Console** keeps the same adaptive chassis and its restrained
  console-surface sampling, but removes Hybrid's faction-atlas modules and
  bounded inner raster plates. It isolates the structural contribution without
  changing the HUD layout.
- **Legacy Frames** keeps all five previously generated faction frame families
  intact as a comparison baseline. It is not the schema-6 direction or the
  default.
- **Clean** removes the structural and legacy decorative layers and exposes the
  functional layout by itself.

The redesign uses the useful composition grammar of a classic full-width RTS
command console—minimap, adaptive selection bay, dedicated portrait and
icon-first command card—without copying another game's art, icons or data. Art
finish changes never alter panel rectangles or hit targets, so the four modes
are a material/rendering comparison rather than different layouts.

Surface color is a separate axis from both faction identity and art finish.
Schema 6 provides six deliberately broad full-surface families:

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
dark-navy test baseline is neither the schema-6 default nor an approved project
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
  controls. **Hybrid** extracts only protected square corners, side mechanisms
  and functional-junction modules from those atlases and places them over the
  adaptive vector chassis without arbitrary-axis stretching. **Legacy Frames**
  remains available when the earlier complete textured-wall treatment needs to
  be compared. Rock Raiders uses turquoise/brown industrial framing; Mars
  Mission Astronauts use ivory/orange aerospace framing; Mars Mission Aliens
  use black/lime/violet crystalline framing; Life on Mars Astronauts use
  sand/red/blue retro-pneumatic framing; and Life on Mars Martians use
  red/tan/blue/lime organic-pneumatic framing. These experimental skins preserve
  the same functional anchors and do not copy another game's layouts or assets.
- an original `console_surface_v1.png` industrial surface. Fixed source patches
  are uniformly scaled and partially cropped where necessary inside the
  code-native chassis. Hybrid also places a small number of bounded,
  aspect-preserving plates behind selection and command content. Raster seams,
  vents and fasteners therefore supplement the code-owned silhouette,
  functional separators and hit targets instead of becoming a stretched HUD
  screenshot.
- a code-native tactical portrait inside the selection section. It draws a
  faction-coloured blueprint display with distinct single-unit, grouped-force,
  structure and transforming-unit silhouettes, plus a concise caption. This
  replaces the text-only art placeholder and gives the retained layout a useful
  visual hierarchy while authored roster portraits remain future content.

`Tab` hides the laboratory controls for a clean evaluation.

## JSON handoff

**COPY JSON** places the complete schema-6 `M7HudProfile` in the clipboard.
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
- A fresh schema-6 profile starts in **Hybrid · Vector + Raster** + **Light
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
imported faction frames, protected square faction modules, bounded
aspect-preserving raster
plates, one continuous bottom-deck chassis, the console-surface binding and
sculpted shoulders, schema 1–5 migration, schema-6 JSON round-trip and safe-area
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

Current schema-6 evidence under `Artifacts/Screenshots/` is:

- `m7-hud-hybrid-light-v2.png` and `m7-hud-hybrid-oxide-v2.png` show the same
  mixed-army geometry with visibly different light and rust-red surface
  families, bounded inner raster plates and sparse faction modules;
- `m7-hud-hybrid-astronaut-light-v3.png`,
  `m7-hud-hybrid-alien-porcelain-v3.png`,
  `m7-hud-hybrid-martian-sandstone-v3.png` and
  `m7-hud-hybrid-rock-olive-v3.png` prove that both the faction raster modules
  and the independent console surface family can change without altering the
  retained HUD composition;
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
