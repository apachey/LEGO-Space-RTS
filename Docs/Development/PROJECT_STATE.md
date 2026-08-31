# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

## Current milestone

**M0–M6 are implemented, verified and game-director accepted. M7 visual canon
remains deliberately open. Preset-style comparisons are retired as the primary
workflow; the gameplay-scale realtime Look Lab is now at schema 8 with free
camera control, ten reproducible surface/glare assets, exact shader-bound role-authored material
families with color-only art controls, per-function emission, automatic
dusk/night spot headlights and work lamps, direct fire lighting, five presentation-only world-light
cycles with extended blue/golden-hour bands, planted-wheel animation, bounded
VFX/destruction pools, deterministic material-audit captures, a natural world-light gradient,
an A/B between map-scale authored terrain composition and a new visible
Raster Forward treatment, five world-specific exploratory surface identities,
depressed tracks and complete JSON copy/paste. The prior raster treatment is
retained only for copied-profile migration. T068 now
provides a separate responsive full-HUD framework and T069 replaces its minimap
placeholder with a client-legal north-up tactical map. The HUD Lab is now schema
8 with a full-width RTS command console and four same-geometry finishes: Hybrid
Vector + Raster, Structural Console, preserved Legacy Frames and Clean. Normal
use exposes exactly four faction-bound visual recipes: Rock Raiders, one unified
Astronaut kit, corrected black/lime Aliens without purple, and Martians. Frame,
shell surfaces and accents change together; the former six independent surface
families survive only as migration/debug history. Hybrid keeps authored raster
detail in protected corner modules, continues the actual faction frame along
every side with unstretched isotropic edge tiling, uses one continuous bounded
console interior and assigns each divider to one renderer.
The old dark-navy test baseline is neither default nor canon. HUD visual
language remains explicitly non-canonical. A post-T069
quality revision makes outline/halo independent from
the style post-pass, hardens profile paste, bounds VFX event memory and avoids
unrelated live material rebuilds. HUD and health visuals remain excluded from the world Look Lab. The
six-page Palette Ratio Lab is game-director accepted. The revised Look Lab
awaits game-director profile exploration and review.**

- The branch includes the verified post-M5 movement handoff from `44e2caf`
  plus T058–T063.
- The old `codex/60-mover-fix` work remains preserved failed research through
  commits `6105cf0` and `f896b01`; it is not merge-ready production code.
- By explicit game-director decision, Stress60 is now `BLOCKING_LATER — M9
  large-battle acceptance`. It remains visible during M7–M8 but does not block
  M6 acceptance or subsequent production work.

## Locked technical foundation

- Godot 4.7.1-stable .NET host with C#.
- Engine-independent deterministic SimCore at fixed 20 Hz.
- Fix32, FixVec2 and Angle16 authoritative numerics.
- Project-owned deterministic navigation and movement.
- Dedicated-server authoritative multiplayer. Godot ENet carries project-owned
  packets and never owns gameplay truth.

## Accepted gameplay baseline

- M2 selection, controls, camera, fog/vision and deterministic movement.
- M3 economy, construction, production, Operations Capacity, Energy Domains,
  brownouts and Basic HUD.
- M4 combat, armor, destruction, repair, Rapid Rider transport and MX-41
  tactical transformation.
- M5 Worksites, excavation topology, forward service, Mission Refit, Alien
  Charge/Surge, tubes and displacement/stability.

## M6 T058–T063 implementation

The headless dedicated-server entry remains:

`--dedicated-server --network-bind <address> --network-port <port>`

### T058–T059 transport and command authority

- ENet/UDP accepts at most two clients and exposes reliable-ordered,
  unreliable-sequenced and reliable-bulk logical channels.
- Server-created cryptographic tokens bind peers to player slots.
- Reliable intent commands use exact-next sequence processing, rate limits,
  canonical payload shapes and authoritative ownership/fog/resource/placement/
  technology/state checks.
- Accepted commands are rebuilt with the server player slot and next legal
  simulation tick. Debug commands are never accepted from network sessions.

### T060 snapshot replication

- The 20-Hz server publishes recipient snapshots every two ticks: canonical
  10-Hz state delivery on unreliable-sequenced transport.
- The server delta-encodes only against a retained baseline that the client
  explicitly acknowledged. Both sides keep bounded baseline history.
- Stable-ID upserts/removals cover presentation entities and projectiles. Each
  recipient also receives only their own economy/capacity/charge, production,
  queued orders, Energy Domain, Worksite and Tube summaries.
- Sparse fog bitsets use standard-library Deflate compression. The real
  two-client acceptance observed a largest initial packet of 665 bytes, below
  ENet's default MTU.
- Client helpers reconstruct full state and interpolate position/orientation
  without changing simulation authority.

### T061 fog filtering

- A recipient snapshot cannot recreate `SimulationWorld` and contains only
  owned, neutral or currently visible presentation state.
- Hidden opponents are absent. Loss of visibility is an entity removal; the
  prior presentation is exposed only as client-side last-known state.
- Hidden target IDs are removed from weapon and repair presentation references.
- Per-player fog and economy data are captured independently; a compromised
  client does not receive the other player's private state.

### T062 reconnect

- Disconnected sessions retain token, player slot and last command sequence for
  60 authoritative seconds.
- Reconnect requires exact simulation protocol, gameplay content and initial map
  hashes.
- Reliable-bulk restore sends a current full legal recipient snapshot plus the
  player's pending accepted commands, unit orders and production queues, then
  resumes regular snapshot streaming from a new baseline.
- The ENet acceptance disconnects player 0 while player 1 stays connected,
  restores sequence 1 on a replacement peer and executes sequence 2.

### T063 network replay

- The server records the initial authoritative snapshot, accepted command log,
  gameplay/map hashes, deterministic seed field, state hashes every 20 ticks
  and seek snapshots every 200 ticks.
- Replay format 16 stores final tick/hash and remains backward-readable for
  replay format 15.
- Playback seeks from the nearest snapshot and fails on hash mismatch.
- Full authoritative replay is unavailable during an active match so it cannot
  bypass fog filtering. After explicit match completion, authenticated clients
  receive it as hash-verified 48-KiB reliable-bulk chunks.
- The ENet acceptance delivered a 666,891-byte server log and reproduced the
  authoritative final hash.

## M7 T064 material-master implementation

The implementation below remains useful infrastructure, but its single
stylized-PBR premise is not an accepted visual target.

- The Godot client owns one centralized PBR material family for molded polymer,
  tool metal, rubber, transparent polymer, Crystal and terrain. It uses the
  engine's built-in `StandardMaterial3D`; no dependency or custom shader was
  added.
- Existing terrain and placeholder entity presentation now consume the same
  masters. Authoritative simulation, content, networking and gameplay formats
  are unchanged.
- The isolated **M7 Material Lab** presents all six families, five canonical
  source-palette lineages, separate team Identification Tiles and polymer
  roughness 0.32 / 0.40 / 0.50 under the same fixed neutral lighting.
- A normal exported build exposes the fixture through `F8` → **M7 Material
  Lab** and provides **RETURN TO PROTOTYPE**. The command-line smoke is
  `--m7-material-lab --m7-material-smoke`.
- Exact RGB matching and subjective material response are explicitly draft.
  Game-director review is required for highlight width, black lift, white
  retention, transparent tint and Crystal emission.

## M7 visual-style exploration decision

- By explicit game-director direction, Phase 08 rendering, material, lighting,
  palette and general style conclusions are suspended. No replacement visual
  canon exists during exploration.
- Image generation is rejected as representative evidence: it did not preserve
  scene invariants, produced infeasible detail, and collapsed distinct prompts
  into near-identical polished imagery.
- The code-native **M7 Style Lab** is implemented in Godot using one reproducible
  Blender-authored drill rig: 57 Blender objects, 48 imported mesh nodes and
  7,776 rendered triangles. The same camera, geometry, animation and scene expose
  four switchable Round 2 treatments: Industrial Mass, Heroic RTS, Constructive
  LEGO and Graphic Volume. Only materials, shaders, lighting, VFX and terrain
  change.
- The separate **Palette Ratio Lab** is implemented with six switchable pages:
  faction visible-area candidates, five distinct Martian source families, two
  identical-silhouette 100-panel abstract-model comparisons, transparency
  semantics and the explicit faction light language. Every displayed ratio
  group totals 100%; no common player blue is injected.
- Rock Raiders now gives earth brown 18% of visible vehicle surface. The 7316
  Excavation Searcher now gives tan/beige the dominant 42%, correcting the prior
  part-count-biased read of both palettes.
- Transparent colors are classified semantically. Tinted transparent polymer
  and glass do not emit merely because they are saturated; only authored energy,
  lamps and resources receive emission.
- Authored faction emission is: Rock Raiders neon orange/lime; Mars Mission
  astronauts blue; Mars Mission aliens neon lime; Life on Mars astronauts red
  plus a colorless warm lamp; and Life on Mars Martians red/orange/lime/blue.
- Team/player color remains valuable but is deferred to model-level ownership
  tests and is excluded from faction palette analysis.

The first style review did not select a direction. The game director then
supplied `Docs/Development/M7_ART_DIRECTION_RESEARCH_BRIEF.md`, explicitly as
non-canonical development research. Its center is a volumetric mechanical 3D RTS
with broad readable forms, visible LEGO construction logic, restrained
environmental noise and no photoreal/tabletop premise. Round 2 replaces the old
forward candidates with Industrial Mass, Heroic RTS, Constructive LEGO and
Graphic Volume. Visual canon remains deliberately unlocked.

The first Round 2 review currently favors Heroic RTS, while retaining Industrial
color depth and Constructive highlights as useful ingredients. It rejects the
overt warm filtering in Industrial and Heroic. Their key lights are now close to
neutral/subtly warm respectively, without removing Heroic bloom or cool fill.
Outline is removed from Graphic Volume and exposed as an independent,
off-by-default `O` toggle on all four styles.

Preset comparison did not yield a selected direction, and the game director
rejected continuing with bundled variants. The primary path is now `F8` →
**M7 Look Lab**, documented in `Docs/Development/M7_LOOK_LAB.md`. It uses the
actual 36-degree gameplay camera and 24–72 build-cell zoom, four identical units,
two buildings, live combat/fire evidence and expanded textured ground. Twelve
independent control sections feed a complete schema-9 JSON profile; schemas 1–8
are migrated without redefining their reviewed ground treatments. Temporary
HUD, selection and health visuals are excluded.

The old inverted-hull outline is retired. The Look Lab uses a real Forward+
depth and normal/roughness pass, separates silhouette from crease strength and
suppresses rough-terrain crease noise. Emissive surfaces now provide real HDR
output, a darker same-hue edge, a separate halo and optional local lights.
Signals, steady lamps and Crystals have independent pulse behavior. Ten
authored review assets remain reproducible; eight are active across paint,
brushed metal, rubber, quarry ground, quiet regolith colour, machine panels and
emissive glare. The fine building-panel and older detailed-regolith maps are
retained as unbound audit history. The old Style and Material labs remain reproducible engineering
fixtures; the Palette Lab remains the accepted palette review fixture.

## M7 T065/T066 presentation implementation

- A shared Godot presentation driver consumes immutable snapshot state for
  locomotion, function, repair, fire recoil, normalized transformation, damage
  and destruction. It drives named mechanical rig pivots but cannot mutate
  authoritative simulation.
- Animation significance implements Tier A every frame, Tier B at approximately
  30 Hz and Tier C at approximately 15 Hz. The laboratory exposes the response
  values and tier override through a four-state identical-rig choreography.
- Generic fixed-capacity pools prewarm and reuse presentation nodes. The
  playable prototype uses 96 projectile views, 24 particle muzzle bursts and 32
  impact bursts; the Look Lab prewarms 64/32/48 tracer/muzzle/impact nodes and
  exposes independent active budgets plus live active/peak/reused/dropped
  telemetry.
- Per-source weapon-fire high-watermarks suppress duplicate cosmetic events.
  Pool exhaustion drops only VFX and never changes projectile, damage or other
  gameplay truth.
- T065/T066 add no dependency and do not change SimCore, gameplay or any network
  or serialization format. Final animation character and VFX look remain
  game-director review gates.
- Current playable prototype bodies are primitive placeholders without named
  mechanical pivots. Production state evaluation and significance are wired;
  the imported six-wheel Look Lab rig is the visible binding proof until
  roster-specific model import supplies production rig sockets.

## M7 T067 bounded LEGO destruction

- Authoritative zero-HP behavior is unchanged: SimCore removes gameplay
  function, selection/navigation participation and collision before any visual
  collapse is presented.
- Production and lab views share a local destruction driver, scale-banded hero
  fragment counts and fixed-capacity pools for LEGO modules and dust.
- Hero modules use two preallocated `MultiMesh` channels with manually evaluated
  cosmetic motion, bounce, settle and fade. No per-brick rigid bodies, colliders
  or gameplay-relevant physics were introduced.
- The Look Lab can destroy the fourth unit, the burning structure or alternate
  them. The intact source disappears into a module burst rather than sinking or
shrinking. Fragment, flash, smoke, ring, dust and pool-budget controls remain
live; the current schema-9 copy/paste retains the complete experiment.
- The implementation does not select destruction art direction. Breakup rhythm,
  wreck silhouette and eventual faction-specific profiles remain game-director
  review gates.

## M7 T068 responsive HUD framework

- The superseded Unity `UI Toolkit` label is implemented through Phase 09A's
  Godot `Control`/container host. A one-way `HudFrame` separates production
  state projection from retained view/layout code.
- The canonical Phase 07 anchors now exist: top global status, bottom-left
  minimap/alert access, bottom-center selection, bottom-right 3×4 command/queue
  interaction. T069 now owns the implemented minimap inside this retained slot.
- Mixed selections aggregate up to 128 selected entities by gameplay type and
  bind to eight reusable cards rather than generating portrait walls.
- The centered bounded console supports 90–100% safe area, 98% default,
  independent UI/text scale and height-relative responsive scaling. Functional bay order is
  minimap, adaptive selection, dedicated tactical portrait and an icon-first
  3×4 command card; the local production queue is a compact strip above it.
- `F8` → **M7 HUD Lab** provides eight information-density fixtures, four
  aspect previews, live layout/type/surface/content/color tokens and complete
  schema-8 JSON copy/paste, including schemas 1–7 migration. Hybrid,
  Structural, Legacy and Clean finishes share invariant outer chassis geometry
  and hit targets. Hybrid derives the exact closed inner alpha aperture from
  each faction's actual frame and draws the complete perimeter last. It adds no
  generic corner radius or dilation that can cut into controls or cross the
  authored wall. The lower plate is full-bleed beneath the frame;
  one nine-slice shader masks its raster field, minimap, fog/markers and outer
  command surface without relying on Godot's Z-sensitive backbuffer clipping.
  The surface fill and those registered edge surfaces share the same authored
  aperture, leaving its exterior corner pixels transparent rather than restoring
  a square backing. Hybrid repeats the source frame's middle edge strips at
  their native aspect ratio between protected corners; no flat black or light
  vector slab sits behind the frame's transparent cut-outs. The generic panel
  style also preserves an explicitly transparent source alpha. A separate
  per-faction interactive rectangle keeps minimap,
  selection, portrait and command controls clear of all four authored rails
  while the visual plate stays full-bleed. Rock Raiders top and lower surfaces remain dark rather
  than inheriting a technical white mask. Authored raster corners and edge
  strips remain proportionally correct and form an uninterrupted four-sided
  frame without full-span raster stretching.
  Structural isolates the earlier vector chassis, Legacy preserves the old full-frame
  renderer for direct comparison and Clean exposes the functional skeleton.

  Schema 8 replaces independent named surface palettes in normal use with one
  complete recipe per playable faction. Rock Raiders bind industrial grey,
  dark turquoise and meaningful earth brown; Astronauts use one coherent
  white/light-grey, medium-blue and restrained-orange kit spanning both human
  source traditions; Aliens bind black/lime without the erroneous purple; and
  Martians bind tan, sand-red and blue pneumatic construction with controlled
  lime energy. Frame, shell and accents cannot drift apart unless the lab is
  explicitly switched to **Custom** audit colors. Shared warning/danger semantics
  remain stable. The retired fifth Life on Mars human art slot migrates to the
  unified Astronaut recipe. A code-native faction-coloured blueprint portrait
  still distinguishes units, groups, structures and transformations without
  changing the locked HUD anchors. The laboratory can now override the faction
  kit while leaving a scenario's content untouched; production auto-binding is
  unchanged and Reset restores it.
  HUD typography now uses vendored OFL-licensed Oxanium SemiBold for titles and
  IBM Plex Sans Regular/Medium for body text and numeric values. Six semantic
  roles, a 900p readability floor and surface-specific contrast resolution
  replace blanket accent/muted colouring and recursive one-size-fits-all gaps.
  The lab exposes overall text scale but no longer delegates baseline type size
  or text-colour correction to the art director.
  Details are in
  `Docs/Development/M7_HUD_LAB.md`; source and regeneration rules are in
  `Docs/Development/M7_HUD_FRAME_AUDIT.md`.
- T068 does not approve HUD art direction. Authored icons, portraits, final
  typography acceptance, faction framing, alert motion/audio and health-bar visuals remain
  game-director review work. T073 still owns the complete command catalog.

## M7 T069 fog-correct minimap

- The production HUD and HUD Lab share a retained, permanently north-up
  minimap. Terrain is cached by topology revision; viewer fog and bounded
  strategic markers refresh at 10 Hz while marker motion is interpolated.
- Current contacts come from the viewer-filtered presentation snapshot. Hidden
  mobile/air enemies are absent; only last-observed enemy structures and
  resources may remain under explored fog and are removed when disproved by
  renewed visibility.
- Mobile, true-air, structure and resource shapes render through four bounded
  `MultiMeshInstance2D` channels. Owned Tube links, remembered enemy Tube
  segments, visible Surge pulses, alerts and the render-frame camera polygon
  are independent layers.
- Left-click/drag controls the camera. Right-click reuses the existing exact
  build-cell Move/rally path and `Shift` queues Move. Attack-move targeting and
  networked team pings remain T073 because their command/target-mode path does
  not yet exist.
- HUD schema 2 exposes minimap fog, marker, interpolation, viewport, network,
  alert and color tokens. Eight legal-knowledge fixtures and four responsive
  aspect smokes exercise the shared implementation. Details are in
  `Docs/Development/M7_MINIMAP.md`.
- No gameplay, SimCore, deterministic rule, network packet, replay format or
  visual canon changed.

## M7 quality revision

- The Look Lab composite remains available for outline and emissive halo when
  style post-processing is disabled. Automation now proves the independent
  `post off + outline on` path in addition to the existing A/B and zoom cases.
- Look/HUD copy-paste normalizes invalid colors and gives partial material
  families their correct family defaults. Existing schema migration remains
  compatible.
- GPU-particle pause is complete, live VFX materials are shared/cached by their
  actual inputs, and event-deduplication state is removed with retired entity
  views so long sessions remain bounded.
- Retained HUD invalidation includes actionable-alert state; non-actionable
  alerts cannot dispatch an action request. No HUD or world art direction was
  selected by this revision.
- The latest visual-quality pass binds raster maps by material role and validates
  the actual shader resources, gives vehicle lamps real terrain illumination,
  separates directional sky ownership from fill/rim lighting and preserves the
  extended blue/golden phases. Painted surfaces now separate a broad generated
  macro-albedo map from restrained relief, use a validated stronger broad
  albedo signal plus derivative-aware low-pass filtering at RTS zoom, and share
  model-wide texture coordinates instead of restarting on every
  LEGO sub-mesh. The Look Lab now exposes three explicit ground comparisons:
  Authored Surface, Raster Forward and Hybrid Surface. The authored composition
  is deterministic lab staging rather than geology: a spacing-aware four-unit
  work pad, service aprons to both buildings, a route/seam to the Crystal, and
  bedrock outside traffic. Raster Forward promotes large image-authored
  albedo/height forms while filtering pebble-scale noise and suppressing visible
  tiling. Hybrid retains the authored composition, adds stronger raster colour,
  normal and roughness response, and confines restrained vertex relief to loose
  soil/bedrock so fixtures and tracks remain at the shared ground boundary.
  Earth is explicitly the
  reviewed desert/sand-covered-slab identity; Mars, Moon, Planet U and
  Underground route distinct exploratory ground palettes and track dust.
  Schema 1–8 profiles retain their prior treatment meanings through migration.
  The HUD Lab schema-8 revision replaces the dirty stretched-frame composition
  with protected, proportion-preserving raster corners, isotropically tiled
  authored edges, an exact faction-frame aperture, shader-masked full-bleed
  edge surfaces, transparent exterior corners, one shared fill/content
  silhouette, a per-faction frame-safe interactive rectangle and one divider owner. The frame and
  complete shell palette now bind
  to exactly four playable-faction recipes; the Astronaut sources share one kit
  and Alien purple is removed. Legacy retains the old frame renderer for honest
  A/B comparison, while the former named non-faction palettes are migration or
  explicit Custom-audit history. Minimap, selection and commands remain
  code-native contiguous sections rather than separately framed texture cards.
  The layout is centered and bounded on wide displays, resource labels are
  visible by default, selection cards fill their assigned bay, and semantic
  spacing replaces recursive uniform gaps. Automation asserts that major panels
  and every visible command remain inside the safe area, including all 12 slots
  in the mixed-army fixture, across the four supported default-profile aspect
  fixtures. Laboratory scale, opacity, padding, spacing and command-fill
  values now apply directly instead of being silently capped by viewport width;
  baseline type roles, sizes and surface-aware text contrast are curated in code;
  geometric deck widths are labelled as safe-area-constrained targets. Production Energy and alert buttons
  retain reliable hit targets; actionable brownout/capacity alerts center their
  associated Worksite. Structural Console keeps its complete vector chassis
  while Legacy Frames and Clean remain direct same-layout comparisons. These
  remain review tools, not approved visual canon.

## Integration format boundary

- authoritative snapshot format **20**;
- simulation protocol **18**;
- replay format **16** (backward reader for 15);
- compiled content format **16** / source schema **15**;
- command packet format **1**;
- recipient snapshot packet format **3**;
- reconnect packet format **1**;
- network replay chunk format **1**.

## Verification state

The exact-aperture and uninterrupted-frame HUD correction passed the complete
fast suite on 2026-08-31 UTC with **zero blocking or diagnostic failures**: 278
NUnit tests, all retained M6/M7 smokes, all four faction kits, the complete
Hybrid/Structural/Legacy/Clean comparison, 16:9/16:10/21:9/4:3 containment,
transparent full-border mask validation, non-destructive aperture coverage,
zero-alpha Hybrid/Legacy parent backings and the same-scenario Alien-kit
override. The capture harness now waits for the requested faction/finish to be
redrawn after its internal switching probes. Exact summary:
`Artifacts/Verification/20260831T172412Z-fast-summary.txt`.

The schema-8 rounded-aperture HUD revision passed `./tools/verify.sh --full` on
2026-08-30 UTC with **zero blocking failures**: 278 NUnit tests, all retained
M6/M7 smokes, four faction-derived shader masks, masked minimap/fog/markers,
dark Rock Raiders surfaces, the complete Hybrid/Structural/Legacy/Clean
matrix, 16:9/16:10/21:9/4:3 containment, a same-fixture Alien-kit override
regression, 100-repeat determinism, replay/snapshot continuation, content
regeneration and a launchable macOS export. Exact summary:
`Artifacts/Verification/20260830T172045Z-full-summary.txt`. The preserved
60-mover M9 diagnostic again reported 2/60 completion and remains
`BLOCKING_LATER`; it does not block M7 and no movement architecture was changed.

The preceding schema-7 logical-hybrid-ground/polished-hybrid-HUD revision passed
`./tools/verify.sh --full` on 2026-08-29 with **zero blocking failures**: 278
NUnit tests, 24/24 representative mover acceptance, every T058–T063 ENet
smoke, all visual/palette pages, schema-9 Look Lab cases covering Authored
Surface, Raster Forward, Hybrid Surface at zoom 35/72 and all five environments,
schema-7
Hybrid/Structural/Legacy/Clean HUD cases with six broad surface families at
16:9/16:10/21:9/4:3, 100-repeat determinism, replay record/playback, snapshot
continuation, compiled-content regeneration and a launchable macOS export.
Exact summary: `Artifacts/Verification/20260829T131054Z-full-summary.txt`.

The freshly exported app was then launched directly into the schema-7 Hybrid
HUD and schema-9 Earth Hybrid Surface fixtures with the Metal Forward+ renderer;
both emitted their complete PASS markers and saved
`Artifacts/Screenshots/m7-exported-hud-hybrid-schema7.png` and
`Artifacts/Screenshots/m7-exported-look-earth-hybrid-schema9.png`.

The preserved 60-mover M9 diagnostic remains `BLOCKING_LATER` and reported
2/60 completion in this run; it does not block M7 acceptance and no movement
architecture change was attempted here.

The final terrain-density control correction then passed the complete fast
suite at `Artifacts/Verification/20260826T195728Z-fast-summary.txt` with zero
blocking or diagnostic failures.

The freshly exported app was launched directly into the 500-panel faction
abstract-model page and the ten-role faction light-language page. Both captured
and emitted their PASS markers from the exported build. Evidence:
`Artifacts/Screenshots/m7-exported-palette-faction-models.png` and
`Artifacts/Screenshots/m7-exported-palette-light-language.png`.

The exported build was also launched directly into all four Round 2 treatments;
each rendered a capture and emitted its PASS marker. Evidence is
`Artifacts/Screenshots/m7-exported-style-industrial-mass.png`,
`m7-exported-style-heroic-rts.png`, `m7-exported-style-constructive-lego.png`
and `m7-exported-style-graphic-volume.png` in the same directory.

The refined light and outline pass then passed the complete suite again at
`Artifacts/Verification/20260822T085432Z-full-summary.txt`. The exported Heroic
fixture emitted PASS markers with outline both off and on; evidence is
`Artifacts/Screenshots/m7-exported-heroic-rts-outline-off.png` and
`m7-exported-heroic-rts-outline-on.png` in the same directory.

The newly exported application was launched directly into the schema-2 Look Lab
at zoom 35 with controls/outline visible and with both hidden. Both emitted the
required PASS marker with four units, 192 unit meshes, 31,104 unit triangles and
two buildings. Evidence:
`Artifacts/Screenshots/m7-exported-look-lab-v2-controls.png` and
`Artifacts/Screenshots/m7-exported-look-lab-v2-outline-off.png`.

The schema-3 T065/T066 integration passed the complete full suite with zero
blocking failures at
`Artifacts/Verification/20260822T133654Z-full-summary.txt`. The freshly
exported application then launched directly into the controls-visible and
clean-view Look Lab fixtures; both emitted PASS with four driver bindings,
three VFX pools and 144 prewarmed VFX nodes. Evidence:
`Artifacts/Screenshots/m7-exported-look-lab-v3-controls.png` and
`Artifacts/Screenshots/m7-exported-look-lab-v3-clean.png`.

The schema-4 T067 bounded-destruction integration passed the complete full
suite with zero blocking failures at
`Artifacts/Verification/20260822T142339Z-full-summary.txt`. The freshly
exported application emitted the schema-4 PASS marker with five pools and 168
prewarmed nodes while exercising the maximum 18-module Structure breakup.
Evidence: `Artifacts/Screenshots/m7-t067-exported-destruction-structure.png`.

The T068 responsive HUD framework passed the complete full suite with zero
blocking failures at
`Artifacts/Verification/20260822T153853Z-full-summary.txt`. The suite exercised
mixed selection at 16:9, production at 16:10, brownout at 21:9 and the critical
tooltip at 4:3. The freshly exported application then emitted its PASS marker
for the critical-tooltip fixture at 21:9 after the final one-line alert fix.
Evidence:
`Artifacts/Screenshots/m7-t068-exported-hud-critical-21-9.png`.

The T069 minimap integration passed the complete full suite with zero blocking
failures at
`Artifacts/Verification/20260822T180629Z-full-summary.txt`: 278 NUnit tests,
all retained Godot/M6/M7 smokes, 100-repeat determinism, replay/snapshot checks,
content regeneration and macOS export. HUD Lab evidence is
`Artifacts/Screenshots/m7-t069-hud-minimap-mixed-army.png`; the production HUD
capture is `Artifacts/Screenshots/m7-t069-production-minimap.png`. The freshly
exported app also emitted the schema-2 minimap PASS marker for the Martian Tube
fixture; evidence is
`Artifacts/Screenshots/m7-t069-exported-minimap-martian.png`.

The M7 quality revision passed the complete full suite with zero blocking
failures at `Artifacts/Verification/20260822T194330Z-full-summary.txt`: 278
NUnit tests, all retained M6/M7 Godot smokes, the new post-off/outline-on
regression, 100-repeat determinism, replay/snapshot checks, content regeneration
and a fresh launchable macOS export. Exported-build evidence is
`Artifacts/Screenshots/m7-quality-revision-outline-without-post.png`.

The schema-6 Look Lab visual-quality and art-director simplification revision passed the complete full suite
with zero blocking failures at
`Artifacts/Verification/20260825T194320Z-full-summary.txt`: 278 NUnit tests,
all retained M6/M7 Godot smokes, four Look Lab fixtures, 100-repeat
determinism, replay/snapshot checks, content regeneration and a fresh
launchable macOS export. Review captures include
`Artifacts/Screenshots/m7-authored-materials-earth-noon.png`,
`m7-blue-hour-auto-lights.png`, `m7-golden-hour-extended.png` and
`m7-shadow-fix-earth-0620.png`.

The earlier schema-5 Look Lab/schema-4 faction-HUD revision passed the complete full
suite with zero blocking failures at
`Artifacts/Verification/20260825T013546Z-full-summary.txt`: 278 NUnit tests,
all retained M6/M7 Godot smokes, four Look Lab visibility/zoom/post/outline
fixtures, four HUD aspect/scenario fixtures, 100-repeat determinism,
replay/snapshot checks, content regeneration and a fresh launchable macOS
export. Visual review evidence is
`Artifacts/Screenshots/m7-look-lab-schema5-final.png`,
`m7-look-lab-earth-night.png`, `m7-look-lab-underground.png` and
`m7-hud-life-on-mars-astronauts-final.png` in the same directory.

The latest Stress60 run remained the expected diagnostic failure with phase
completion **4/60, 5/60 and 2/60**. The exported macOS debug build is:
`Builds/macOS/LEGO Space RTS.app`.

## Deferred M9 large-battle gate

The legal stress60 fixture still exposes mid-route corridor traffic/yield
deadlock. The approved immutable endpoints and bounded arrival sequencer solve
the representative 24-mover arrival wall but not this distinct scale case.

Do not resurrect or stack the rejected portal-flow/local-pressure experiments.
Another traffic coordinator/solver attempt requires **ARCHITECTURE REVIEW
REQUIRED**. The benchmark remains a diagnostic through M7–M8 and becomes
blocking only when M9 must prove its stable-large-battle exit.

## Next approved action

1. Begin M8 T070: import all 35 canonical unit definitions without inventing or
   rebalancing roster content.
2. The game director may independently explore `F8` → **M7 HUD Lab** and return
   its Hybrid/Structural/Legacy/Clean comparison across the four faction-bound
   recipes, then return a **COPY JSON** profile later; no HUD visual canon is
   required to begin T070.
3. The gameplay-scale **M7 Look Lab** remains available for world-style review;
   compare the named Authored surface with Raster Forward rather than the
   migration-only Legacy result. Do not record T064 acceptance until the game
   director explicitly locks a direction.
4. Keep Stress60 visible without starting an unreviewed third movement attempt;
   revisit it for M9 or earlier only if a catastrophic movement regression
   appears.
