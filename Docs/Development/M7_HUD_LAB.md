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
- bottom-left: fog-correct north-up minimap and actionable alert access;
- bottom-center: entity or type-group selection information;
- bottom-right: canonical 3×4 command grid and local queue;
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

The laboratory exposes every current presentation token rather than bundled
presets:

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
- five generated faction frame families, an art-frame enable, opacity and
  construction-weight control. Rock Raiders uses turquoise/brown industrial
  framing; Mars Mission Astronauts use ivory/orange aerospace framing; Mars
  Mission Aliens use black/lime/violet crystalline framing; Life on Mars
  Astronauts use sand/red/blue retro-pneumatic framing; and Life on Mars
  Martians use red/tan/blue/lime organic-pneumatic framing. These
  skins preserve the same functional anchors and do not copy another game's
  layouts or assets.

`Tab` hides the laboratory controls for a clean evaluation.

## JSON handoff

**COPY JSON** places the complete schema-4 `M7HudProfile` in the clipboard.
**PASTE JSON** accepts the same profile, normalizes all values and applies it
live. Schema 1–3 profiles migrate to schema-4 defaults for minimap and faction
art. Unknown fields are ignored; unsupported schema versions fail visibly.

The profile is intentionally independent from `M7LookProfile`: world rendering
and interface art direction can be reviewed and versioned separately.

## Automation

Headless fixture validation:

```text
Godot --headless --path GodotClient -- \
  --m7-hud-lab --m7-hud-smoke \
  --m7-hud-scenario mixed-army --m7-hud-aspect 21-9
```

Capture helper:

```text
./tools/capture-m7-hud-lab.sh \
  Artifacts/Screenshots/m7-hud-lab-mixed-army.png \
  mixed-army 16-9
```

The main verification suite exercises mixed selection, production, brownout and
critical-tooltip fixtures across four aspect ratios. The smoke also verifies all
eight fixture contracts, client-legal minimap knowledge, remembered-static
rules, pixel/cell command conversion, the 12-slot command bound, required
retained nodes, all five imported faction frames, prior-schema migration and
schema-4 JSON round-trip.

## Deliberately deferred

- final HUD visual canon and acceptance of any generated faction framing;
- icons, portraits, authored typography and localization assets;
- T073 complete command catalog and every target-mode button path;
- attack-move-on-minimap and networked `G` team pings, because their target-mode
  and command transport belong to T073 rather than being invented in T069;
- final alert animation/audio, health-bar art and accessibility settings menu.

Disabled command slots in the current production prototype identify T073
catalog work; existing contextual world commands and already implemented Stop,
State Change, production and Energy-priority actions remain functional.
