# M7 HUD Lab

Status: implementation and art-direction laboratory for T068/T069. It is not
visual canon.

## Purpose

Phase 07 fixes the HUD's information hierarchy and functional screen anchors,
but it does not select a final visual language. The production HUD and HUD Lab
therefore share the same retained Godot `Control` view and one-way `HudFrame`
presentation contract. Production supplies client-legal state; the laboratory
supplies deterministic information-density fixtures. Neither path writes
gameplay state.

Open the lab from `F8` -> **M7 HUD Lab**, or launch it with:

```text
--m7-hud-lab
```

## Fixed functional skeleton

- top: Ore, Energy, spendable Crystals, Operations Capacity, faction mechanic
  and match state;
- bottom-left: fog-correct north-up minimap and actionable alert access;
- bottom-center: a wide entity/type-group information bay followed by a
  dedicated tactical portrait bay;
- bottom-right: an icon-first canonical 3x4 command card and compact local
  queue strip;
- temporary layers: objective tracker, event feed, Energy Domain popover and
  quick/expanded tooltip.

The retained T069 minimap uses baked terrain, viewer knowledge, remembered
static contacts, batched shape markers, alerts/network lines and a camera
viewport polygon. Left-click/drag moves the camera; right-click issues a ground
move or rally point; `Shift` + right-click queues a unit move. Laboratory
gestures produce visible synthetic feedback without writing gameplay.

## Scenario fixtures

Keys `1`-`8` switch deterministic UI states:

1. Rock Raider single heavy unit;
2. mixed 45-unit Rock Raider army grouped into five gameplay types;
3. three-facility Rock Raider production and queue;
4. Rock Raider Energy Domain brownout;
5. Astronaut tactical transformation and objective tracker;
6. Alien Charge, commitment and Surge context;
7. Martian Aero Tube component status;
8. critical Rock Raider command-structure alert and expanded tooltip.

These are information-density and interaction tests, not separate visual
variants.

## Schema 8 visual contract

Schema 8 removes the six free-floating console palettes as the normal art
workflow. A playable faction now selects one complete recipe: raster frame,
shell surfaces, recessed field, accents, text and selection treatment change
together. This prevents a correct frame from being paired accidentally with an
unrelated violet, navy or ceramic interior.

There are exactly four normal recipes:

- **Rock Raiders** -- dark industrial grey, dark turquoise, important earth
  brown and restrained hazard yellow. Brown is structural palette mass, not a
  minor dirt tint.
- **Astronauts** -- one coherent faction kit combining the human source
  traditions: white/light-grey expedition structure, medium blue field
  equipment, restrained mission orange and small red/blue signals. Life on Mars
  humans and Mars Mission humans are not exposed as two playable HUD factions.
- **Aliens** -- black/charcoal structure with lime and neon-lime energy, limited
  neutral grey and tiny separated dark-red/blue signals. Purple is explicitly
  excluded from the Alien recipe.
- **Martians** -- tan, sand-red and blue pneumatic/mechanical construction with
  controlled lime energy. Sand purple may appear only as a localized Martian
  infrastructure reference; it is not an Alien identity color.

Shared good/warning/danger semantics remain stable so critical state never has
to be relearned per faction. Individual color editing is still available for
diagnosis and experimentation, but it marks the profile **Custom** and no longer
pretends to be a faction-authentic recipe.

The same geometry supports four finishes:

- **Hybrid · Vector + Raster** is the schema-8 default. Generated raster art is
  kept where it contributes authored material detail--protected corners,
  adjacent shoulders and one central clasp--while responsive rails, functional
  seams and hit geometry stay code-native. Each recipe derives its own alpha
  aperture from the transparent centre of that faction's frame. The plate,
  raster surface and HUD content are clipped by that aperture before the full
  perimeter is drawn on top. Long rails therefore remain continuous on all
  four sides without stretching or repeating raster detail. The bottom console
  is one bounded chassis with a continuous interior surface, and one renderer
  owns each divider.
- **Structural Console** isolates the earlier vector chassis and its responsive
  construction without the generated faction frame.
- **Legacy Frames** preserves the previous full raster-frame renderer and old
  assets for honest A/B comparison. Its clip aperture is derived from that
  active Legacy texture rather than the Hybrid v2 texture. It is a baseline,
  not the selected design and not permission to reintroduce five playable skin
  slots.
- **Clean** removes decorative chrome and exposes the functional layout.

Art-finish changes never move panels or hit targets. Faction changes preserve
the outer top-strip/deck geometry, functional bay order and minimum hit sizes;
their inner content rectangle may shift by a few pixels to follow the authored
aperture. This keeps the comparison about presentation without allowing the
surface to leak beyond its own frame.

The useful lesson from StarCraft II is functional hierarchy and the legibility
of a full-width command console, not its specific frame art, iconography or
Terran-like dark-blue color treatment. Detailed audit and regeneration rules
are recorded in `Docs/Development/M7_HUD_FRAME_AUDIT.md`.

## Live controls

The laboratory keeps art-director controls that materially test layout and
readability:

- responsive preview: 16:9, 16:10, 21:9 and 4:3;
- safe area, independent UI/text scale and top/bottom region size;
- minimap, command and selection width targets;
- panel gap, padding, separation, opacity, border and corner radius;
- solid/outlined command buttons and health-state color mapping;
- heading, body and micro text sizes;
- content-density toggles for labels, hotkeys, portrait, event feed,
  objectives, minimap legend and command costs;
- minimap marker scale, motion smoothing, fog/contact opacity, grid, viewport,
  alert/network toggles and tactical-map colors;
- independently previewable faction recipe, Hybrid/Structural/Legacy/Clean
  finish, chrome intensity and scale; changing a preview kit preserves the
  active density fixture until Reset;
- complete color tokens for explicit **Custom** audits.

The code-native tactical portrait remains functional scaffolding. It shows a
faction-colored blueprint with distinct single-unit, grouped-force, structure
and transforming-unit silhouettes; authored roster portraits remain future
content.

`Tab` hides the laboratory controls for a clean evaluation.

## JSON handoff and migration

**COPY JSON** places the complete schema-8 `M7HudProfile` in the clipboard.
**PASTE JSON** accepts the same profile, normalizes all values and applies it
live.

- Schema 1-4 profiles migrate to **Legacy Frames** + **Custom**, preserving the
  old generated-frame/navy result. Early `frameOpacity`/`frameThickness`
  experiments migrate once to `chromeIntensity`/`chromeScale`.
- Schema 5 preserves an explicitly stored Structural/Legacy/Clean finish. A
  schema-5 profile without a finish uses **Structural Console** and remains
  **Custom**.
- Schema 6-7 explicit **Custom** colors remain Custom. A named experimental
  palette in a non-Legacy finish migrates to the profile's complete faction
  recipe rather than carrying an unrelated surface family forward.
- The retired Life on Mars human sub-skin maps to the shared **Astronauts**
  recipe. The old fifth Martian slot maps to the canonical fourth faction.
- A fresh schema-8 profile starts in **Hybrid · Vector + Raster** with the
  **Rock Raiders** faction-bound recipe.

Unknown fields are ignored; unsupported schema versions fail visibly. The HUD
profile remains independent from `M7LookProfile`, so world rendering and
interface review can be versioned separately.

## Automation

Headless fixture validation can select the fixture, aspect and finish. The
fixture supplies its canonical faction recipe:

```text
Godot --headless --path GodotClient -- \
  --m7-hud-lab --m7-hud-smoke \
  --m7-hud-scenario alien-resonance --m7-hud-aspect 21-9 \
  --m7-hud-finish hybrid
```

The capture helper retains the same scenario/aspect/control/finish workflow and
accepts an optional final faction-kit override:

```text
./tools/capture-m7-hud-lab.sh \
  Artifacts/Screenshots/m7-hud-alien-hybrid-schema8.png \
  mixed-army 16-9 hidden hybrid faction aliens
```

The sixth argument `faction` (also the helper default) leaves the palette
`FactionBound`. The optional seventh argument chooses `rock-raiders`,
`astronauts`, `aliens` or `martians` without changing the fixture. Omitting it
binds the kit to the scenario as production does. The old named palette
arguments remain available only for migration/debug comparisons and produce an
explicit **Custom** profile.

The schema-8 smoke contract must cover:

- all eight fixtures and all four playable faction recipes;
- identical outer chassis across all finishes and factions, finish-invariant
  hit geometry, and bounded faction-specific inner offsets;
- four independently switchable kits on one unchanged fixture;
- Hybrid's faction-specific aperture masks, bounded plate/content, complete
  four-sided perimeter, isotropic raster modules and single divider system;
- Legacy availability without treating its retired fifth art slot as a
  playable faction;
- all visible command buttons inside the safe area, including the 12-command
  mixed-army fixture;
- legal minimap knowledge, remembered-static rules and input conversion;
- schemas 1-7 migration and schema-8 JSON round-trip;
- required raster imports and one-to-one faction recipe mapping.

## Review evidence

New schema-8 captures should be named by faction and finish rather than by an
independent surface palette. A useful review set is one clean Hybrid capture
for each of Rock Raiders, Astronauts, Aliens and Martians, plus a matched
Hybrid/Legacy A/B using the same fixture, aspect and profile controls.

Earlier schema-7 screenshots remain historical evidence for why the independent
palette/tiled-frame approach was retired. They must not be presented as current
faction-color approval.

## Deliberately deferred

- final HUD visual canon or acceptance of any generated frame;
- final authored command icons, roster portraits, typography and localization;
- T073 complete command catalog and every target-mode path;
- attack-move-on-minimap and networked team pings;
- final alert animation/audio, health-bar art and accessibility settings menu.

Unimplemented command slots stay hidden rather than appearing as decorative
empty wells. T073 still owns the complete contextual command catalog; existing
world commands and implemented Stop, State Change, production and
Energy-priority actions remain functional.
