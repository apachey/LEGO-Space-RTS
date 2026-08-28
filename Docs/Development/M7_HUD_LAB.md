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

The laboratory exposes the current presentation tokens and three art-finish
modes on one invariant geometry:

- **Structural Console** is the new default: a continuous sculpted chassis,
  recessed functional bays and an original dark industrial raster surface;
- **Legacy Frames** keeps all five previously generated faction frame families
  intact and applies them to the redesigned layout;
- **Clean** removes both decorative layers and exposes the functional layout by
  itself.

The redesign uses the useful composition grammar of a classic full-width RTS
command console—minimap, adaptive selection bay, dedicated portrait and
icon-first command card—without copying another game's art, icons or data. Art
finish changes never alter panel rectangles or hit targets, so the three modes
are a real A/B/C comparison rather than different layouts.

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
  controls. The square sources are not stretched over arbitrary panels:
  protected source corners retain their square proportions, while the authored
  horizontal and vertical wall spans are repeated at one uniform scale and only
  the final partial tile is cropped. One outer faction chassis therefore has
  complete textured walls around the bottom control deck instead of isolated
  corner decals; only two additional source modules mark the real
  minimap/selection and selection/command junctions. The three contiguous inner
  surfaces receive restrained faction-specific chassis fills but remain
  code-native functional regions, and the top resource strip owns the only
  other complete faction frame. Generated art is neither stretched nor cloned
  into nested panel borders. Rock
  Raiders uses turquoise/brown industrial framing; Mars Mission Astronauts use
  ivory/orange aerospace framing; Mars Mission Aliens use black/lime/violet
  crystalline framing; Life on Mars Astronauts use sand/red/blue
  retro-pneumatic framing; and Life on Mars Martians use red/tan/blue/lime
  organic-pneumatic framing. These skins preserve the same functional anchors
  and do not copy another game's layouts or assets.
- an original `console_surface_v1.png` industrial surface. Structural Console
  samples bounded source patches inside the code-native chassis rather than
  stretching one image across the screen. Large panels, shallow seams and
  restrained copper details provide the missing wall/fill material while
  functional separators, faction accents and outer silhouettes remain code.
- a code-native tactical portrait inside the selection section. It draws a
  faction-coloured blueprint display with distinct single-unit, grouped-force,
  structure and transforming-unit silhouettes, plus a concise caption. This
  replaces the text-only art placeholder and gives the retained layout a useful
  visual hierarchy while authored roster portraits remain future content.

`Tab` hides the laboratory controls for a clean evaluation.

## JSON handoff

**COPY JSON** places the complete schema-5 `M7HudProfile` in the clipboard.
**PASTE JSON** accepts the same profile, normalizes all values and applies it
live. Schema 1–4 profiles migrate to **Legacy Frames**, preserving the visual
result that was authored before the finish switch existed. Early schema-4
`frameOpacity`/`frameThickness` experiments still migrate once to
`chromeIntensity`/`chromeScale` and are omitted on the next copy. Unknown fields
are ignored; unsupported schema versions fail visibly.

The profile is intentionally independent from `M7LookProfile`: world rendering
and interface art direction can be reviewed and versioned separately.

## Automation

Headless fixture validation:

```text
Godot --headless --path GodotClient -- \
  --m7-hud-lab --m7-hud-smoke \
  --m7-hud-scenario mixed-army --m7-hud-aspect 21-9 \
  --m7-hud-finish structural
```

Capture helper:

```text
./tools/capture-m7-hud-lab.sh \
  Artifacts/Screenshots/m7-hud-lab-mixed-army.png \
  mixed-army 16-9 hidden structural
```

The main verification suite exercises Structural mixed selection, production,
brownout and critical-tooltip fixtures across four aspect ratios, plus identical
Structural/Legacy/Clean mixed-selection geometry. The smoke also verifies all
eight fixture contracts, client-legal minimap knowledge, remembered-static
rules, pixel/cell command conversion, the 12-slot command bound, required
retained nodes, the procedural tactical portrait, all five imported faction
frames, protected non-stretched corners, uniformly scaled tiled wall regions,
restrained faction chassis fills, one continuous bottom-deck
chassis, the Structural raster binding and sculpted shoulders, prior-schema
migration, schema-5 JSON round-trip and safe-area
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

Current post-redesign evidence under `Artifacts/Screenshots/` is:

- `m7-hud-final-structural-mixed-16-9-v2.png`,
  `m7-hud-final-legacy-mixed-16-9-v2.png` and
  `m7-hud-final-clean-mixed-16-9-v2.png` for the same-geometry A/B/C comparison;
- `m7-hud-final-structural-production-16-10-v3.png` and
  `m7-hud-final-legacy-production-16-10-v2.png` for queue/command density and the
  direct Structural-versus-old-frame comparison;
- `m7-hud-final-structural-brownout-21-9.png` for Energy Domain, alert and event
  overlays on ultrawide;
- `m7-hud-final-structural-critical-4-3.png` for the narrow
  tooltip/event/alert/deck collision gate.

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
