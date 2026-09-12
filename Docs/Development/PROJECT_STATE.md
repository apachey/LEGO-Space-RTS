# LEGO SPACE RTS — CURRENT PROJECT STATE

This file is the short repository handoff. Canon and current Git evidence remain
authoritative when anything here becomes stale.

The dense M7 cross-thread continuity index is
`Docs/Development/M7_THREAD_HANDOFF.md`.

## Current milestone

**M0–M6 are implemented, verified and game-director accepted. M7 visual canon
remains deliberately open. M8 T070–T074 are complete, including the complete
roster-reference gate for the approved command/runtime catalog: all 35
canonical unit chassis, 31 infrastructure definitions, 38
research definitions and 35 stable command families are present, and legal
construction, production, research and the affected faction actions now use
their authoritative prerequisites, resources, timings and deterministic state.
Content compilation now fails on unresolved or contradictory roster, map,
presentation, production-exit and Aero Tube eligibility references.
The formerly unresolved T073 values are approved and recorded in
`Docs/Development/M8_T073_BINDING_REVIEW.md`.
By explicit game-director decision, the roadmap now includes authoritative
**M8.5 — Full Content & Presentation Production** between M8 and M9. Phase 09C
requires complete production designs/models for all 35 units and 31
infrastructure entries plus a blocking Super Scout reference-intelligence
corpus, full-roster animation/VFX, icons/portraits, frontend/menu UI, audio and
integrated human acceptance. M9 may no longer claim Skirmish Alpha presentation
directly from the data-only M8 roster.
Super Scout and production acceptance explicitly reject generic AI-looking
forms: every major part must have understood identity/function, animation must
be mechanically specified, texture needs must be planned and authored, and
each final asset requires an iterative game-director review loop to explicit
acceptance.
Preset-style comparisons are retired as the primary
workflow; the gameplay-scale realtime Look Lab is now at schema 9 with free
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
language remains explicitly non-canonical outside the accepted combination. A
minimum four-faction M7 visual-direction review flow combines the strongest
existing Look Lab and HUD Lab directions at a wide 84-cell opening plus exact
24/44/72-cell checks. The game director accepted M7 Final with outline on as the
production visual direction. The T081 production asset pipeline is complete,
fully verified and game-director accepted. T082 Super Scout reference
intelligence is now in progress: its exact 66-asset identity/source baseline and
all four faction source-page audits are complete. Forty-three faction-scoped
source records are visually audited across 48 official books plus one archival
instruction scan; only Martian sources 1195 and 3750 remain evidence gaps. All
66 assets now also have faction-internal semantic-construction,
motion/socket and material/texture-needs drafts. The first complete primitive
24/44/72-cell concept-silhouette corpus was rejected after the game director
recognized 0/66 assets at 24 cells. The materially different source-derived
Pilot V2 then passed game-director blind review at 4/4, approving that method
for a new 66-asset pass. The complete-corpus 24/44/72 review gate remains open.
A post-T069
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

## M7 visual-direction acceptance

- A directly launchable four-faction review flow is
  available from `F8` → **Review M7 visual direction**. It presents bounded
  prototypes for 4970 Chrome Crusher, 7647 MX-41 Switch Fighter, 7646 ETX Alien
  Infiltrator and 7313 Red Planet Protector without treating them as final M8.5
  production models.
- The scene exposes three like-for-like world treatments over the same
  four-faction proof: the original acceptance-candidate presentation, the exact
  final M7 schema-9 Earth Hybrid stack reused directly from Look Lab code, and a
  hybrid that retains the current Filmic response over the M7 terrain/sky/post
  stack. The final M7 screen-space outline is an independent on/off comparison
  in all three modes. The scene also retains mechanical animation, pooled combat
  VFX, LEGO destruction, the Hybrid Vector + Raster HUD and legal minimap.
  On-screen controls expose faction focus, continuous 24–108-cell wheel zoom and
  exact 24/44/72-cell camera checks; the review checklist is documented in
  `Docs/Development/M7_VISUAL_ACCEPTANCE_CANDIDATE.md`.
- The revised three-look candidate passed the complete full suite with zero
  blocking failures at
  `Artifacts/Verification/20260908T213822Z-full-summary.txt`: 317 NUnit tests,
  retained M6/M7 gates, all three candidate camera bands, all three look modes
  with outline off/on, determinism, replay/snapshot checks, content regeneration
  and a fresh macOS export. The exported app also passed a direct M7 Final +
  outline-on candidate smoke. The preserved M9 60-mover diagnostic remains the
  only non-blocking diagnostic failure at 2/60 completion.
- **Game-director acceptance is recorded:** M7 Final is the production visual
  direction, with the screen-space outline enabled by default. The review flow
  starts at 84 cells and retains continuous mouse-wheel zoom from 24 to 108
  cells. Current and Hybrid remain comparison modes only. This accepts the
  direction, not the bounded prototype geometry as final production art. The
  Phase 09C visual-direction dependency is satisfied. T081 is separately
  game-director accepted after its pipeline review and corrections.
- The accepted default, exact 24/44/72 checks, all three comparison modes and
  both outline states pass their Godot smoke fixtures, and a fresh exported app
  starts at `zoom=84 look=m7-final outline=on`. The latest general fast run is
  `Artifacts/Verification/20260908T221707Z-fast-summary.txt`: every stage except
  the pre-existing HUD Lab process teardown passed. That HUD fixture printed its
  complete PASS marker on both attempts, then Godot exited with its intermittent
  macOS mutex teardown error; this is not visual acceptance and was not hidden
  by weakening the gate.

## M8 T070–T074 canonical roster, research and command data

- The source content and built-in fallback catalog now contain exactly 35
  canonical buildable units: 8 Rock Raiders, 13 Astronauts, 6 Aliens and 8
  Martians. No unit was invented, removed, split or rebalanced.
- Every entry carries its canonical faction/source classification, base
  footprint, movement layer and profile, Operations Capacity, HP, armor, sight,
  target class and presentation key. The 38 movement profiles encode the
  accepted Phase 06 speed, acceleration and turn classes for all base chassis
  plus the retained MX-41 alternate mode and technical/static fixtures.
- The source map, generated map binary and repository-local fallback map share
  the final Phase 06 sight values for the four existing Raider opening units.
  Strict authored-spawn agreement remains enabled.
- Static validation and NUnit regression coverage assert the exact 8/13/6/8
  faction split, all 35 stable keys, positive OC/durability and representative
  binary round-trips.
- The same source and built-in fallback catalog now contain exactly 31
  canonical infrastructure definitions: 8 Rock Raiders, 8 Astronaut, 6 Alien
  and 9 Martian structures. Entries carry the Phase 03/04/06 faction/source,
  footprint, Ore/Crystal/Energy costs, build time, OC, energy and durability
  data without inventing missing values.
- Source schema 16 and compiled format 17 add explicit building Crystal costs
  plus low/high footprint-mask words for the canonical 10×8 Flight Operations
  Pad and 9×9 Aero Tube Hangar. Formats 2–16 remain readable; a compiler parity
  guard requires checked-in JSON and the built-in fallback to serialize
  identically.
- The catalog now contains exactly 38 canonical technologies: 9 Rock Raider,
  10 Astronaut, 10 Alien and 9 Martian definitions. Every entry carries its
  faction, physical research provider, canonical categories, Ore/Energy/Crystal
  cost, 20-Hz duration, stable presentation/localization references and at
  least one validated unlock or parameter effect.
- T072 prerequisite groups encode AND between groups and OR within a group.
  Completed-research, building and state-threshold requirements are distinct;
  Alien Advanced Resonance Architecture preserves four committed Crystals for
  the whole research job, Astronaut Integrated Expedition Command preserves its
  Field + Mission + one-of-five Mission specialization relationship, and
  Martian Grand Network Integration preserves Redundant Routing plus two
  connected Stations.
- The research validator rejects duplicate IDs, invalid factions/providers,
  unresolved or cross-faction content effects and every possible prerequisite
  cycle. Advanced Excavation Systems and Grand Network Integration expose
  separate Searcher gates so the later command definition can require both.
- Source schema 17 / compiled format 18 append the research DAG after the T071
  payload. Formats 2–17 remain readable; format 17 yields an empty research
  catalog. Checked-in JSON, the built-in fallback and tracked content binary
  compile byte-identically at content hash `93530AA6EC0E2742`.
- T073 source schema 18 / compiled format 19 add all 31 construction action
  bindings, all 35 production recipes and 35 stable command definitions. The
  action prerequisites resolve against same-faction building/research data and
  preserve AND-of-OR semantics. The tracked binary and built-in fallback remain
  byte-identical at content hash `5C197B27F8C40EC8`; formats 2–18 remain
  readable.
- All 31 construction definitions and all 35 production recipes are now
  runtime-eligible subject to their authored prerequisites. Ore, Energy and
  Crystal costs are reserved deterministically; Crystal-bearing construction
  cannot bypass its cost.
- Production, research and Mission Refit cancellation use the approved 20%
  initial Ore/Energy commitment, linear remaining consumption, half-consumed
  refund and 50%-progress Crystal commitment. Waiting jobs refund fully;
  Mission Refit retains owned modules and its facility-destruction exception.
- The 13 producers use the approved front-centred exit dimensions. Excavation
  uses the six explicit per-machine/per-terrain durations and 25/50 Energy.
- Astronaut 18-cell Energy areas overlap into pooled domains without changing
  Forward Service topology. Alien structures deterministically choose the
  nearest operational Command Core within 18 cells while retaining the
  absolute one-Resonance-Core-per-Command-Core rule. Martian Tube components
  pool Energy, and Settlement Station retains +1 Energy/s with 150 reserve.
- ETX Defense Node placement records its mandatory initial mode. Later mode
  changes take 120 ticks, cost nothing and pause during the exact trailing
  80-tick hostile-damage pressure window.
- Aero Tube Link length-dependent cost/time and Resonance Core committed-Crystal
  demand remain delegated to their existing authoritative systems.
- Public command codes 19–35 now pass the unchanged command packet layout with
  guarded payload validation and the T073 action handlers. Research completion,
  production/research/excavation jobs and Defense Node state are authoritative
  snapshot state.
- T074 closes the full shipping roster and authored-map reference graph during
  content compilation. It checks stable-ID uniqueness, canonical counts,
  movement/weapon/presentation/localization references, production coverage and
  producer exits, the exact three Tube-eligible Martian units, map starts,
  spawns, resource receivers and excavatable presentation. Eight deliberately
  broken fixtures preserve specific diagnostics for every acceptance case.
- Stale pre-roster shorthand IDs in the retained M5 Astronaut, Martian and Alien
  runtime bindings now use the T070 canonical IDs. This lets newly produced
  canonical T-3 Trikes receive Mission Refit state and keeps Forward Service,
  Aero Tube eligibility and the M5 acceptance fixture bound to the actual roster.

## M8.5 T081 accepted production asset pipeline

- `Docs/Development/M85_ASSET_PIPELINE.md` defines the common Blender → GLB
  → Godot production path for T082–T092 without changing gameplay or visual
  canon. One Blender metre equals one Godot world unit; one authoritative build
  cell equals two world units. Roots remain at ground/footprint centre with
  identity transforms, and Blender positive Y imports as Godot negative Z.
- Stable semantic names cover geometry importance, material roles, mechanical
  pivots and presentation sockets. Every production asset requires authored
  Close, Combat and Strategic LODs whose complexity decreases while identity,
  locomotion and primary function remain readable.
- The representative reference vehicle proves the entire round trip with
  868/332/168 triangles, six pivots, six sockets and the accepted M7 material
  roles. It is explicitly a technical pipeline fixture, not roster art, and
  cannot count toward the 35 production units or satisfy later asset tasks.
- The sidecar records source classification, provenance, dimensions, LOD
  budgets, material roles, pivots, sockets and director-review state. Static
  validation checks the source and GLB structure; Godot validates the imported
  scene at exact 24/44/72-cell camera distances. Full verification regenerates
  the GLB from the editable Blender source and requires byte-identical output.
- A normal build exposes the review through `F8` → **Review M8.5 asset
  pipeline**. `Q`/`E`, the left/right arrows or on-screen buttons orbit the
  camera; `Z`, `X` and `C` select the exact 24, 44 and 72-cell checks. Technical
  attachment markers remain visible through the model for unambiguous review.
- The first game-director pass accepted the scale, orientation, LOD progression
  and other visible evidence, but found that the fixed camera hid the yellow
  mechanical-pivot markers. Camera orbit and through-model marker visibility
  correct that review blocker. The follow-up review confirmed the drill hinge
  but rejected shared midpoint markers between visually separate wheels; the
  four Close-LOD wheels now each carry a pivot at their own centre. The game
  director reviewed the correction and accepted the complete T081 pipeline on
  2026-09-09.
- The candidate has zero blocking verification failures and a launchable macOS
  build. Its game-director acceptance is `ACCEPTED`; T082 is now unblocked but
  has started on a separate task branch.

## M8.5 T082 Super Scout reference intelligence

- `Content/Presentation/SuperScout/roster_identity_baseline.json` binds the
  research corpus to the exact 35-unit / 31-infrastructure runtime roster. It
  records the canonical faction, role, footprint, source classification,
  source-set lineage, silhouette thesis and three non-removable identity anchors
  for every asset.
- `Content/Presentation/SuperScout/source_ledger.json` and
  `source_instruction_index.json` record 39 relied-upon source families,
  reference-only rights handling, official instruction pages, exact direct LEGO
  PDF links and BrickLink inventory corroboration. Thirty-five sources now have
  direct official PDFs; four older/promotional sources remain explicitly
  archival-backed.
- The Rock Raiders source pass visually audited all seven available official
  manuals plus the archival 1277 instruction scan and 4930 product evidence.
  The 4990 HQ manual establishes the tower/crane/gantry/conveyor worksite
  grammar used by adapted buildings; 1277 establishes a low twin-saw hover
  sled; and 4930 establishes five crew figures plus small equipment and tools.
- The Astronaut source pass visually audited all 18 mapped sources across 23
  official PDF books with exact page counts, hashes, construction ranges and
  view/mechanism coverage. It confirms the faction's Field/Mission split,
  modular carriers and work platforms, open gantry/service language and the
  source geometry behind the MX-41, MX-71, MT-201, MT-51 and MT-101 mechanisms.
- The Alien pass visually audited all eight faction-mapped sources across ten
  official books. Faction-and-set evidence keys isolate opposing builds inside
  mixed boxes. The pass distinguishes the small-craft families, verifies the
  Infiltrator's three-leg conversion and Mothership's open circular carrier
  construction. The earlier Strike siege-endpoint inference is explicitly
  superseded: 7693 is exclusively airborne, and its hinges only articulate the
  continuous flight craft. Composite Razor construction remains an explicit
  adaptation rather than a guess.
- The Martian pass visually audited all eight available mapped sources across
  eight official books and retained 1195/3750 as explicit archival gaps. It
  corrects the small-craft and Worker Robot profiles, verifies distinct modular
  transformations for the 7313/7314 mech families, isolates the primary 7316
  Martian Searcher from supporting builds, and establishes 7317 as a physical
  multi-station Tube/sled network rather than decorative piping.
- The Rock Raiders contract pass converts source and canon into buildable
  semantic/load-path drafts, named motion pivots and contacts, presentation
  sockets and four fully specified reusable texture families for all 16 faction
  assets. Archival evidence now grounds Crew as five visible character/equipment
  variants. By explicit game-director canon decision, Drill Craft preserves
  1277's low hover sled, paired side pods and two small forward ice saws instead
  of replacing them with one oversized drill. All 16 packets remain HOLD for
  the complete-roster silhouette and game-director gates.
- The T082 source-analysis policy now requires a whole-set audit and separately
  records figures, equipment and detachable modules before roster mapping. It
  also requires traced connections plus a completed-model view before topology
  claims, correcting the earlier false four-spoke reading of the bipedal 7302
  Worker Robot. The 7691 Mothership is explicitly one selectable unfolding
  carrier; its separately built source modules are internal/bay evidence, not
  automatic separate roster assignments. Any composed design must disclose
  exact donors, new work and rejected alternatives before director review.
- The Astronaut contract pass covers all 13 units and eight infrastructure
  entries. It keeps rugged Field Systems and clean modular Mission Systems
  visibly distinct while sharing insignia and service interfaces. All source
  transformations, carried payloads, refit modules, production exits, motion
  pivots, state beats and presentation sockets are explicit, with four
  specified-but-not-authored reusable texture families.
- The Alien contract pass covers all six units and six infrastructure entries.
  Black/lime craft-derived mechanics, readable crescents, bay openings,
  source-specific articulated frames and bounded crystal hardware replace generic
  biological or interchangeable black-neon forms. The Mothership is one
  selectable unfolding unit; training units inside it remains an explicitly
  unresolved gameplay proposal rather than an assumed production rule. Four
  reusable Alien texture families are specified but not authored.
- The Martian contract pass covers all eight units and nine infrastructure
  entries. It preserves open blue/sand-red machinery, the corrected two-legged
  Worker, distinct runner-led hover craft, unequal walker tools, physical
  pressure/Tubes and readable cranes, clamps, switches and platforms. Protector
  and Searcher state changes remain continuous mechanical actions rather than
  hand-separated source rebuilds.
- The 1195 and 3750 archival gaps remain explicit but do not force guessed
  geometry: verified 7317 platform/docking grammar carries the Aero Skiff draft,
  and its complete Tube system carries the Link draft. Four reusable Martian
  texture families are specified but not authored.
- Generated semantic, motion and material matrices now receive their faction
  label explicitly; this corrects stale Rock Raiders headings previously shown
  on the otherwise-correct Astronaut and Alien matrices.
- All 66 A–I packet files, the identity/source matrix and a 44-pair confusion
  register regenerate deterministically. Every confusion pair has three visible
  differentiation requirements, and every packet is visibly `HOLD`, so this
  baseline cannot be confused with completed evidence or game-director
  acceptance.
- The first complete black primitive-silhouette corpus covered all 66 assets at
  the 24/44/72 camera ratios, but the game director recognized 0/66 at the first
  24-cell gate. Its generic primitive composition is rejected and retained only
  as a failed baseline. A four-asset Pilot V2 instead reconstructs hashed source
  instruction views into isolated black concept renders. On 2026-09-11 the game
  director identified Alien Mothership, Worker Robot, Chrome Crusher and Drill
  Craft correctly at 4/4, approving the method for full-roster and camera-scale
  expansion without accepting the future complete corpus.
- The Full V2 expansion now has a complete review-ready 66/66 corpus: all 16 Rock Raiders,
  all twenty-one Astronaut concepts, all twelve Alien concepts and all seventeen
  Martian concepts use the accepted source-derived matte-black render method,
  and every output is recorded by stable ID and SHA-256 hash.
  A closer official 7310 reference resolved Mono Jet's single asymmetric engine;
  Mobile Mining Platform discloses its 7645/7648/7693 modular construction, and
  MX-81 keeps its mission sections docked to one aircraft. All eight Astronaut
  infrastructure silhouettes now preserve their named 7315/7690 descendants or
  explicitly disclose their cross-set donors; the new Sentinel is specifically
  a 7690 service pedestal plus interchangeable 7695-derived defense heads.
  MT-101 and MT-201 required a materially different topology-controlled reference
  method after two earlier candidates each failed to expose all six wheels or all
  four planted legs. Their final review candidates now make all six wheels and
  all four legs independently countable. Alien Mothership is explicitly one
  selectable carrier whose front, side-bay and jetpack-drone modules remain
  attached while unfolding; it is not split into several units. Servitor,
  Razor Skimmer and all six Alien infrastructure silhouettes disclose their
  exact official donor sets. The Martian set preserves the corrected two-leg
  Worker Robot, both modular two-leg combat machines, the four-leg Excavation
  Searcher and the 7317 pneumatic Tube grammar. Aero Skiff claims no exact 1195
  geometry, and Aero Tube Link claims no exact 3750 geometry; both remain
  explicitly bounded by those archival gaps and use verified 7317 construction.
  Six self-contained blind-review PNG boards cover the complete corpus at the
  24/44/72 camera widths with stable V-codes. This does not change T082's
  `BLOCKING_NOW` state or constitute corpus approval before game-director review.
- The first Full V2 24-cell game-director review is complete for 60/66 codes and
  requires revision; V51-V56 were not included. The strong candidates remain
  preserved, while incorrect source geometry, generic cross-faction buildings
  and ambiguous movement/operation silhouettes must be repaired before a new
  24-cell review. Initial correction candidates exist for Jet Scooter, ETX
  Alien Strike and Red Planet Protector. The conflict is now resolved by
  explicit game-director canon correction: ETX Alien Strike is exclusively
  airborne with no siege/deployment state. Second-wave candidates make the
  Strike a continuous crescent aircraft, restore Protector's asymmetric source
  cannons and make Jet Scooter lower and sleeker. Director review then rejected
  the Protector rev2 because the narrow weapon edit preserved the wrong bulky
  humanoid body. Rev3 rebuilds the whole 7313 topology as a broad wedge craft
  on a tiny waist and two thin legs, with no arms and two unequal top-mounted
  cannon booms. The candidates remain separate from the
  immutable first-review boards until the wider correction pass is ready.
  Director-supplied multi-angle references then corrected three more source
  reads: Mono Jet is a compact asymmetric drum/nozzle/operator assembly rather
  than a long sled; Alien Jet has an open pilot under two arches with two small
  lateral emitters; and the source-faithful complete Solar Explorer is a
  non-wheeled three-module assembly whose separate small rover is the wheeled
  component. Current gameplay canon still requires Solar Explorer to become a
  large wheeled ground-support unit, so its future production running gear
  remains an explicit adaptation rather than a claimed part of set 7315.
  On 2026-09-12 the game director accepted the new Mono Jet, the first
  monochrome Alien Jet attempt and the new Solar Explorer as individual
  correction candidates for the next composite review. The colored second
  Alien Jet attempt is explicitly excluded. The next correction wave restores
  MT-61 Crystal Reaper's directly docked detachable upper spacecraft/processing
  module, two manipulators and twin harvesting wheels, and removes the separate
  Alien ambush craft read from MT-51 Claw-Tank while preserving its tracked
  rotating-body layout. Both remain unreviewed candidates. MT-101 remains open:
  one new image-generation approach preserved all six wheels but still obscured
  its permanent cockpit, while a direct-cover approach restored the cockpit and
  separate upper mechanisms but hid part of the six-wheel topology. The
  two-attempt rule stops further free-form generation; its next pass requires a
  controlled source-trace blockout. T082 remains on hold until the remaining
  priority corrections and renewed 24-cell blind review are complete.
  The next source-closer correction rebuilds 4980 Tunnel Transport as a compact
  asymmetric two-rotor skeletal airlifter; the game director accepted it for
  the next composite review. Rapid Rider now restores the complete 4920
  catamaran, controls, work lights, cargo hopper and paired rear propulsion and
  is also director-accepted for that review. The first 7690 MB-01 correction
  wrongly incorporated the separate spacecraft into the base. Rev2 removed it
  but invented a vertical capsule. New director-supplied source photos establish
  the horizontal open three-arch command deck, octagonal portal, A-frame,
  front pneumatic manifold, branching hoses, separate three-lobed pump and
  four-barrel defense tower. The director explicitly authorized one additional
  MB-01 generation instead of manual blockout: rev3 uses the new photos directly
  and is director-accepted for the next composite review; barrel count remains
  partly occluded in this view and must be retained from source during modeling.
  This exception does not authorize extra MX-71 or MT-101 attempts. MX-71's first
  correction remained too broad; its second recovered the long narrow spine,
  separate tail booms and close underslung rover but still misplaced and
  simplified the four blue-tipped emitters. Its two allowed image attempts are
  exhausted, so the next step is a controlled source-trace blockout rather than
  a third free-form generation.
  The director accepted the source-only 7302 Worker Robot and 7647 MX-41
  flight-state corrections for the next composite review: compact open biped
  versus long cockpit hull and
  narrow folding wing with six wheels. Generated Martian facial relief is not
  source evidence; canonical worker tools and MX-41 ground/transform coverage
  remain production obligations. Accepted candidates are unchanged.
  The next source-only correction pair adds unreviewed Mission Fighter (7695)
  and Rover (7301) first attempts: closed canopy and crooked plate wings versus
  compact open four-sphere-wheel rover with horizontal side scanner. Exact
  prompts and image hashes are retained. This does not replace Mission Fighter's
  canonical 5619/7695 family or approve either image for production.
- The official 7301 instruction page also exposed a stale audit-reconciliation
  error: Rover is a four-wheel open platform, not the two-wheel bike described
  by the earlier identity and production drafts. The source ledger, identity,
  contract, confusion entry and generated packets now agree on four wheels;
  validation locks that evidence-backed correction while leaving rejected V1
  historical artifacts untouched.
- `tools/Validation/validate_m85_super_scout.py` compares the corpus with the
  authoritative runtime roster, checks counts, IDs, classifications, source
  coverage and the three-to-seven silhouette-anchor contract, and rejects stale
  generated packets.
- T082 remains `BLOCKING_NOW` for T083/T085. All faction source-page audits,
  all 66 production-contract drafts are complete; the first silhouette approach
  failed 0/66, Pilot V2 passed 4/4 and the new full corpus currently contains a
  hashed 66/66 review-ready corpus with six complete blind boards. The 24/44/72
  game-director review, any resulting revisions and explicit corpus acceptance
  remain required. Current detail is tracked in
  `Docs/Development/M85_SUPER_SCOUT_PROGRESS.md`.

Routine Godot verification is now non-intrusive on macOS: every gameplay smoke
runs headlessly with an explicit automated-smoke flag, records its real
PASS/FAIL exit code, then bypasses the Godot 4.7.1 native teardown that had
intermittently produced a system crash dialog after successful tests. The
verification and export scripts are regression-checked for this explicit flag.
Normal playable/editor sessions retain normal shutdown. Viewport captures still
require a real renderer, so they are not launched as a routine automated-test
step and must be announced when new visual evidence is actually required.

## Integration format boundary

- authoritative snapshot format **21** (backward reader for 20);
- simulation protocol **19**;
- replay format **16** (backward reader for 15);
- compiled content format **19** / source schema **18**;
- command packet format **1**;
- recipient snapshot packet format **3**;
- reconnect packet format **1**;
- network replay chunk format **1**.

## Verification state

The latest priority correction wave, including the director-accepted Tunnel
Transport and Rapid Rider, the second-attempt MB-01 Eagle Command Base and the
explicit MX-71/MT-101 two-attempt blocks, passed `./tools/verify.sh --full` with
zero blocking failures at `Artifacts/Verification/20260912T135031Z-full-summary.txt`.
All 317 tests, T082 integrity, deterministic repeat/replay/snapshot checks,
content and asset regeneration, retained headless presentation/network gates
and the macOS export passed. The preserved 60-mover M9 stress scenario remains
the only `BLOCKING_LATER` diagnostic failure.

The 7693 exclusively-airborne canon correction, synchronized runtime metadata
and second-wave Jet Scooter / ETX Alien Strike / Red Planet Protector render
candidates passed `./tools/verify.sh --full` with zero blocking failures at
`Artifacts/Verification/20260912T012019Z-full-summary.txt`. All 317 tests,
T082 integrity, content regeneration, deterministic replay/snapshot checks,
headless Godot/network presentation gates, deterministic Blender regeneration
and the macOS export passed. The preserved 60-mover M9 stress scenario remains
the only `BLOCKING_LATER` diagnostic failure.

The review-ready 66/66 Full V2 corpus passed `./tools/verify.sh --full` with
zero blocking failures at
`Artifacts/Verification/20260911T232841Z-full-summary.txt`: all 317 tests, the
T082 integrity validator, deterministic repeat/replay/snapshot checks, retained
presentation and network gates, deterministic asset regeneration, and a fresh
macOS export passed. The only diagnostic failure is the preserved 60-mover
`BLOCKING_LATER` M9 stress scenario; it does not block T082 review.

The previous 24/66 Full V2 checkpoint, recorded 0/66 T082 V1 rejection and
game-director 4/4 source-derived Pilot V2 acceptance passed
`./tools/verify.sh --full` with zero blocking failures at
`Artifacts/Verification/20260911T124657Z-full-summary.txt`: all 317 tests,
the complete 66-asset roster and faction-bound evidence validator,
deterministic packet generation, the 24-mover gate, compiled content, retained
M6 networking, T081 round trip, retained M7 presentation gates and a fresh
macOS export passed. The T082 stage reports 66 HOLD packets, 39 source records,
35 direct official-PDF sources, four archival sources, seven official plus two
archival Rock Raiders audits with zero remaining Rock Raiders evidence gaps,
all 18 Astronaut sources audited across 23 official PDF books, all eight Alien
sources audited across ten official PDF books, all eight available Martian
sources audited across eight official PDF books with two Martian archival gaps,
all 66 assets across four factions with structured semantic/motion/material
contracts and no provisional contract, 19 generated comparison matrices, the
18 rejected deterministic V1 artifacts, the explicit 0/66 result, four hashed
Pilot V2 images and exact 4/4 responses, 24 hashed Full V2 concepts, the
source-corrected four-wheel Rover contract, and 44 confusion hypotheses with no roster drift, blind-sheet
label leak or cross-faction evidence leakage. Technical validation does not
reverse the V1 visual failure or claim complete-roster T082 acceptance.
Stress60 remained the expected 2/60 `BLOCKING_LATER` M9 diagnostic.

Every routine Godot smoke and the exported app emitted the explicit immediate
exit marker after its real PASS result. No Godot-named diagnostic report is
currently present after this clean run. The rejected V1 silhouette result is a
visual-design failure, not an automated-test or engine crash.

The recorded T081 game-director acceptance passed `./tools/verify.sh --full`
with zero blocking failures at
`Artifacts/Verification/20260909T094920Z-full-summary.txt`: all 317 tests, the
accepted sidecar gate, Blender/GLB validation and byte-identical regeneration,
all six imported pivots at 24/44/72 cells, retained Godot
presentation/network checks and a fresh macOS export pass. Stress60 remained
the expected 2/60 `BLOCKING_LATER` M9 diagnostic. The Godot diagnostic-report
count remained 25, so this full run produced no new macOS crash report.

The non-intrusive Godot automation revision passed `./tools/verify.sh --full`
with zero blocking failures at
`Artifacts/Verification/20260909T084741Z-full-summary.txt`. It exercised every
retained Godot smoke plus the exported macOS app without opening a game window.
The macOS Godot diagnostic-report count remained **25 before and 25 after** the
full run, confirming that none of the disposable test processes entered the
former crash-reporting teardown failure. Stress60 remained the expected 2/60
`BLOCKING_LATER` M9 diagnostic.

M8.5 T081 passed `./tools/verify.sh --full` on 2026-09-09 UTC with **zero
blocking failures**: all 317 NUnit tests, the 24-mover gate, content and
network checks, all retained M7 presentation gates, Blender/GLB contract and
byte-identical regeneration, Godot import at 24/44/72 cells, 100-repeat
determinism, replay/snapshot continuation and a fresh launchable macOS export.
The preserved Stress60 diagnostic reported 2/60 completion and remains
`BLOCKING_LATER` for M9 rather than a T081 regression. Exact summary:
`Artifacts/Verification/20260909T082127Z-full-summary.txt`.

M8 T074 targeted roster/M5 coverage passes 57/57. Static validation and the
Godot C# host build pass after the final HUD-reference correction. The full
`./tools/verify.sh --full` run on 2026-09-08 UTC passes all 317 NUnit tests,
the 24-mover gate, content compilation/regeneration, deterministic and replay
checks, all M6 network smokes, the Style/Palette/Look Labs and a launchable
macOS export. Its summary reports one blocking HUD-Lab failure only because
Godot raised its known mutex teardown error after the ninth fixture had already
printed its PASS marker; the two remaining fixtures plus the affected brownout
fixture then pass in isolated reruns. Stress60 remains the unchanged 2/60
`BLOCKING_LATER` M9 diagnostic. Full-run summary:
`Artifacts/Verification/20260908T073441Z-full-summary.txt`.

M8 T073 targeted coverage passes 30/30 for command bindings/catalog,
construction and Mission Refit, and 107/107 for the relevant deterministic,
snapshot/replay, content, faction-system and M6 network compatibility selection.
The single requested `./tools/verify.sh --full` run on 2026-09-08 UTC passed
308/308 NUnit tests, the 24-mover gate, content and deterministic/replay checks,
all M6/M7 headless labs and a fresh macOS export. Its summary recorded one
blocking failure because static validation still expected the pre-approval
Settlement Station reserve of 0; after updating that guard to the approved 150,
the standalone static/source validation passes. The prior M7 HUD-lab mutex
teardown exit did not recur. Stress60 again completed 2/60 and remains the known
`BLOCKING_LATER` M9 diagnostic rather than a T073 regression. Exact full-run
summary: `Artifacts/Verification/20260908T005749Z-full-summary.txt`.

M8 T072 passed `./tools/verify.sh --full` on 2026-09-01 UTC with **zero
blocking failures**: 289 NUnit tests, exact 38-technology roster and 9/10/10/9
faction split, canonical costs/ticks/providers, AND/OR and maintained-threshold
DAG coverage, cycle/unresolved-effect rejection, format-17 backward reading,
source/fallback/binary parity, 24/24 representative mover acceptance, all
retained M6/M7 smokes, 100-repeat determinism, replay/snapshot continuation and
a fresh launchable macOS export. The preserved Stress60 M9 diagnostic again
reported 2/60 completion and remains `BLOCKING_LATER`. Exact summary:
`Artifacts/Verification/20260901T171512Z-full-summary.txt`.

M8 T071 passed `./tools/verify.sh --full` on 2026-09-01 UTC with **zero
blocking failures**: 285 NUnit tests, exact 35-unit and 31-infrastructure
rosters, wide-footprint/Crystal round-trips, format-16 backward reading,
pre-T073 local/network command gating, 24/24 representative mover acceptance,
content regeneration, all retained M6/M7 smokes, 100-repeat determinism,
replay/snapshot continuation and a fresh launchable macOS export. The preserved
Stress60 M9 diagnostic again reported 2/60 completion and remains
`BLOCKING_LATER`. Exact summary:
`Artifacts/Verification/20260901T112740Z-full-summary.txt`.

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

1. Continue T082 by expanding the game-director-approved source-derived Pilot V2
   method into a new complete 66-asset corpus. Then run both 24-cell pages and
   the 44- and 72-cell pages as a blind review. Revise every wrong, uncertain or
   indistinguishable S-code and expand the 44-pair confusion register wherever
   the review exposes a new neighbor. Retain the two explicit Martian archival
   gaps unless usable evidence appears, and escalate any gap that blocks a safe
   production decision. Do not treat the concept silhouettes, generated `HOLD`
   packets, T070 data definitions or the non-roster T081 pipeline fixture as
   production models.
2. Execute the remaining Phase 09C M8.5 T082–T092 work before final M9 Skirmish
   Alpha acceptance.
3. Use the separate M7 Look/HUD/Palette labs only to investigate a rejected aspect;
   they remain exploratory tools and do not independently record visual canon.
4. Keep Stress60 visible without starting an unreviewed third movement attempt;
   revisit it for M9 or earlier only if a catastrophic movement regression
   appears.
