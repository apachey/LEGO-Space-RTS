# LEGO SPACE RTS — TECHNICAL ARCHITECTURE & PROTOTYPE IMPLEMENTATION SPEC v1.0

**Phase:** 09 — Technical Architecture & Prototype Implementation Spec  
**Status:** AUTHORITATIVE PROJECT CANON  
**Authority:** Project Instructions + Phase 00 Canon Set Registry + Phase 01 Game Bible Foundation + Phase 02 Faction Bible & Asymmetry + Phase 03 Unit & Building Roster + Phase 04 Economy, Technology & Progression + Phase 05 World & Map Bible + Phase 06 Combat, Damage & Balance Framework + Phase 07 Controls, Camera, UX & Interface + Phase 08 Visual Bible & UI Art Direction

---

# CANON STATUS

Phase 09 converts the complete game specification into an implementation architecture.

It does **not** redesign the game to suit an engine.

It preserves:

- four playable factions;
- 35 buildable units;
- 31 building / infrastructure entries;
- Martian Aero Tube Links;
- 100 Operations Capacity per player;
- approximately 30–45-unit normal large battles;
- 160 × 160-cell standard 1v1 maps;
- approximately 2 meters per build-grid cell;
- deterministic combat;
- actual projectile travel where specified;
- real overkill;
- physical pathing and collision;
- authored Excavatable Terrain;
- Rock Raider Worksites;
- Astronaut Mission Refit and Forward Service;
- Alien Crystal Charge, Resonance Cores and Surge;
- Martian Aero Tube Networks, displacement and Stability;
- true air;
- transport passengers;
- fog of war;
- faction-specific Energy Domains;
- replay-compatible commands;
- the Phase 07 PC mouse-and-keyboard interface;
- the Phase 08 premium stylized LEGO presentation.

The final playable roster remains **8 Rock Raider units, 13 Astronaut units, 6 Alien units and 8 Martian units**, with 31 infrastructure entries overall.

Phase 06 already establishes deterministic combat, five collision-footprint families, role-aware formations, real projectile travel, bounded friendly compression, deterministic destruction and the rule that simulation values remain authoritative rather than animation or VFX.

Phase 07 establishes a 128-entity implementation selection ceiling, persistent Entity identity across transformations/refits/transports/Tube transit, command queues and explicit player-command semantics.

Phase 08 establishes the 44-cell combat view as the primary visual-readability standard and requires LEGO construction logic rather than realistic military-science-fiction presentation.

No Phase 00–08 canon amendment is required.

---

# PART I — TECHNICAL NORTH STAR

# DATA-DRIVEN, DETERMINISTIC RTS SIMULATION

# SEPARATED FROM PRESENTATION,

# WITH SERVER-AUTHORITATIVE GAMEPLAY STATE,

# REPRODUCIBLE COMMANDS,

# AND TOOL-FRIENDLY CONTENT.

The game is architected around eleven engineering principles.

1. **Simulation owns gameplay truth.** Rendering, animation, physics effects, audio and UI never determine authoritative outcomes.

2. **Determinism is designed in, not retrofitted.** Authoritative gameplay avoids nondeterministic engine physics, floating-point gameplay state, unstable iteration and hidden timing dependencies.

3. **Content is data-driven.** Units, structures, weapons, research, movement profiles, commands, maps, hazards and faction systems use validated schemas and stable identifiers.

4. **State is explicit.** Important gameplay states are serializable and inspectable rather than implied by animation clips, GameObject hierarchy or transient callbacks.

5. **Commands are reproducible.** Player input becomes a small deterministic command stream suitable for multiplayer, replays and debugging.

6. **Bounded complexity beats theoretical scale.** The architecture is optimized for the actual game—100 OC and medium armies—not thousands of simulation entities.

7. **Faction systems extend common infrastructure.** Worksites, Refit, Charge and Aero Tubes use composable systems rather than four separate game engines.

8. **Performance is budgeted from the start.** Simulation, pathfinding, animation, VFX, UI and networking have measurable budgets.

9. **Debugging is a product feature for development.** State inspection, hashes, overlays, deterministic replay and validation exist from the prototype phase.

10. **Dependencies are deliberately minimal.** Authoritative simulation, navigation, serialization and command processing do not depend on opaque third-party middleware.

11. **Engine upgrades never silently change gameplay.** A new engine/package version is adopted only after deterministic, replay, network and performance suites pass.

---

# PART II — ENGINE COMPARISON

The serious candidates are:

- Unity;
- Unreal Engine;
- Godot.

Current first-party information establishes the following baseline.

Unity's current release family has **Unity 6.3 LTS**, supported until **December 2027**. Unity describes Update releases as preferred for many new/mid-cycle productions but explicitly positions LTS for teams locking a production version. Unity 6.5 has subsequently shipped as a Supported release, so 6.3 is not selected because it is numerically newest; it is selected because this project benefits from a deliberately pinned LTS baseline. ([unity.com](https://unity.com/releases/unity-6/support))

Unreal Engine **5.8** shipped on **June 23, 2026**. Epic describes it as the final planned major UE5 release while UE6 work ramps up, with UE5 continuing to receive bug/regression support. UE offers mature multiplayer replication, including the default replication system, Replication Graph and Iris. ([unrealengine.com](https://www.unrealengine.com/news/unreal-engine-5-8-is-now-available))

Godot **4.7.1-stable** shipped on **July 14, 2026** and remains the strongest open-source candidate. Godot is MIT-licensed and commercially usable without royalties. ([godotengine.org](https://godotengine.org/article/maintenance-release-godot-4-7-1/))

## COMPARATIVE EVALUATION

| Requirement | Unity 6.x | Unreal 5.8 | Godot 4.7.1 |
|---|---|---|---|
| Stylized 3D renderer | Excellent | Excellent / highest ceiling | Good–very good |
| LEGO material work | Excellent URP/Shader Graph fit | Excellent material system | Good shader system |
| Medium animated-object counts | Excellent | Excellent | Good |
| Custom CPU simulation | **Excellent in C#** | Powerful but higher C++/engine integration cost | Good C#/C++/GDScript options |
| Pure engine-independent sim core | **Very natural** | Possible, but Unreal types/macros encourage engine coupling | Natural |
| Custom deterministic navigation | **Natural** | Possible, but replacing standard navigation adds substantial engine-side complexity | Natural |
| Fixed-point gameplay | **Straightforward in pure C#** | Straightforward in custom C++, but higher implementation burden | Possible |
| RTS UI | **Strong UI Toolkit fit** | Strong UMG/CommonUI | Capable |
| VFX | Strong | Exceptional | Capable |
| Animation | Strong | Exceptional | Good |
| Built-in networking | Strong transport + services ecosystem | **Most mature high-level replication ecosystem** | Capable lower-level stack |
| Custom command/snapshot netcode | **Good low-level fit** | Possible, but bypasses much of UE's strongest networking advantage | Possible |
| Dedicated server | Supported | Excellent | Supported |
| Data-oriented custom architecture | **Excellent without forcing DOTS** | Good, but UObject/Actor ecosystem encourages another model | Good |
| Pure ECS requirement | Optional | Optional | Optional |
| Asset pipeline | **Excellent for small team** | Excellent but heavier | Good |
| Editor extension | **Excellent C# editor tooling** | Excellent, usually more complex | Good |
| Automated tests | Unity Test Framework + pure NUnit | Automation framework | Built-in test ecosystem/community tooling |
| Profiling | Excellent | Excellent | Good |
| Source control friendliness | Good with text assets/meta discipline | Good with Perforce/Git-LFS discipline; binary assets heavier | Very good |
| AI-assisted code iteration | **Very strong C# ergonomics** | Good, but C++ compile/reflection complexity is higher | Strong |
| Small-team development speed | **Best overall fit** | Lower for this architecture | Very good |
| Large AAA rendering ceiling | High | **Highest** | Lower |
| Licensing predictability | Subscription threshold model; no runtime fee | Royalty model | **MIT** |
| Risk of engine overengineering | Moderate | **High for this RTS scale** | Low–moderate |
| Long-term commercial ecosystem | Excellent | Excellent | Growing |
| Overall project fit | **BEST** | Strongest rejected candidate | Viable but less production-complete |

Unity's current UI Toolkit is intended for runtime UI and separates UXML structure, USS styling and C# behavior; Unity Transport is explicitly its low-level networking transport; Unity's Test Framework supports Edit and Play Mode testing. These characteristics match the planned separation between engine presentation and a custom simulation core. ([docs.unity3d.com](https://docs.unity3d.com/kr/current/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/introduction-to-ui-toolkit.html))

Current licensing also does not introduce the abandoned Runtime Fee: Unity states that it was cancelled. In 2026 Unity Personal remains available below the stated $200,000 revenue/funding threshold and Pro is required above it under the current plan structure. ([unity.com](https://unity.com/products/pricing-updates))

Unreal remains technically formidable. Its current networking stack is significantly more capable than this project needs if the game uses standard Actor replication, while this project's deterministic command-driven simulation would deliberately bypass much of that advantage. Epic's current documentation also presents multiple replication systems; its Iris documentation and UE 5.8 release notes currently differ in wording around production status, reinforcing the decision not to base this RTS architecture around Iris-specific behavior. ([dev.epicgames.com](https://dev.epicgames.com/documentation/en-us/unreal-engine/components-of-iris-in-unreal-engine))

Godot's MIT licensing and source accessibility are exceptional strengths, but this project would spend more small-team engineering effort reaching the same level of 3D content tooling, large production pipeline ergonomics and mature commercial asset workflow already available in Unity. That additional work does not improve the actual RTS simulation. ([godotengine.org](https://godotengine.org/license/))

---

# PART III — ENGINE DECISION

# PRIMARY ENGINE

# UNITY 6.3 LTS

The exact patch version is pinned in source control.

No developer independently upgrades the project.

A Unity upgrade occurs only on a dedicated integration branch after:

- deterministic replay tests;
- headless simulation tests;
- client/server compatibility tests;
- benchmark scenes;
- visual regression smoke tests;
- content validation

all pass.

## PRIMARY PROGRAMMING LANGUAGE

# C#

Authoritative simulation code lives in ordinary C# assemblies without `UnityEngine` references.

## VISUAL SCRIPTING POLICY

Unity Visual Scripting is **not permitted for authoritative gameplay**.

It may be used only for:

- throwaway visual experiments;
- editor-only content workflows;
- non-authoritative presentation prototypes.

Shipping simulation rules, faction mechanics, damage, economy, pathfinding and networking are written in C#.

## SHADER TECHNOLOGY

# URP + SHADER GRAPH + LIMITED HAND-WRITTEN HLSL

Shader Graph handles the normal stylized LEGO material family.

Hand-written HLSL/custom functions are used when:

- Shader Graph produces avoidable cost;
- fog composition requires a custom pass;
- selection/outline logic requires specialized rendering;
- GPU instancing requires custom data access.

## UI TECHNOLOGY

# UI TOOLKIT — UXML + USS + C#

World-space selection rings and frequently repeated combat bars use a separate lightweight world-overlay renderer rather than thousands of independent retained UI documents.

## VFX TECHNOLOGY

# POOLED PARTICLE SYSTEM + INSTANCED MESH/DECAL EFFECTS + SHADER GRAPH

VFX Graph is **not a baseline dependency**.

It may later be approved for isolated hero-quality cosmetic effects if profiling demonstrates a clear production advantage.

## ANIMATION TECHNOLOGY

# UNITY ANIMATOR + ANIMATION RIGGING + C# PRESENTATION DRIVERS

Animation state follows simulation state.

Animation does not own gameplay state.

## NAVIGATION / PATHFINDING

# CUSTOM DETERMINISTIC HIERARCHICAL GRID NAVIGATION

Unity NavMesh is not authoritative.

## NETWORKING FOUNDATION

# UNITY TRANSPORT + CUSTOM DEDICATED-SERVER COMMAND/SNAPSHOT PROTOCOL

Unity Transport provides connection and packet transport; game-state semantics remain project-owned. Unity describes Transport as its low-level connection/send layer, which is exactly the desired abstraction level. ([docs.unity3d.com](https://docs.unity3d.com/kr/current/Manual/com.unity.transport.html))

## DATA / SERIALIZATION

# JSON SOURCE DATA + COMPILED IMMUTABLE BINARY RUNTIME DATA

Unity asset references use presentation-side ScriptableObjects keyed by the same Stable IDs.

## TESTING FOUNDATION

# PURE C# NUNIT + UNITY TEST FRAMEWORK + HEADLESS GOLDEN REPLAYS

Unity's current Test Framework supports both Edit and Play Mode tests. ([docs.unity3d.com](https://docs.unity3d.com/kr/current/Manual/com.unity.test-framework.html))

## WHY UNITY

Unity provides the strongest intersection of:

- C# iteration speed;
- engine-independent deterministic simulation;
- high-quality stylized rendering;
- editor extensibility;
- tooling;
- UI;
- asset import;
- profiling;
- Windows deployment;
- small-team development;
- AI-assisted coding.

## STRONGEST REJECTED ALTERNATIVE — UNREAL ENGINE 5.8

Unreal is not rejected for technical weakness.

It is rejected because this game's hardest systems are:

- deterministic simulation;
- custom pathfinding;
- replayable commands;
- faction topology;
- data tooling;

rather than extreme rendering scale.

Using Unreal would either:

1. couple the game more deeply to Actor/UObject/network replication systems that do not naturally provide the required deterministic command simulation; or
2. require building a largely independent simulation anyway while accepting heavier C++/editor/build complexity.

Unity gives the same architectural independence with materially less small-team overhead.

Current Unreal licensing also moves commercial game revenue into a royalty model above the first $1 million of attributable lifetime gross revenue, while Godot remains MIT and Unity uses its current subscription threshold model. Licensing is not the primary technical decision but reinforces the need to keep the architecture engine-portable at the simulation boundary. ([unrealengine.com](https://www.unrealengine.com/license))

---

# PART IV — TARGET PLATFORM

## INITIAL TARGET

# WINDOWS 11 64-BIT PC

## CPU ARCHITECTURE

# X86-64

## GRAPHICS API

Primary:

# DIRECTX 12

Fallback during development:

# DIRECTX 11

The project does not depend on a DX12-exclusive gameplay feature.

## MINIMUM SUPPORTED GAMEPLAY RESOLUTION

# 1280 × 720

The complete Phase 07 interface must remain functional at this resolution.

## PRIMARY TEST RESOLUTION

# 1920 × 1080

## SECONDARY VISUAL TEST RESOLUTION

# 2560 × 1440

## REFRESH ASSUMPTION

Primary performance certification:

# 60 HZ / 60 FPS

Higher-refresh displays are supported.

Simulation frequency does not increase with refresh rate.

## INPUT

Primary:

- mouse;
- keyboard.

Controller and console are not first-implementation requirements.

Input code is action-abstracted, but UI layout and control design remain uncompromised PC RTS controls.

---

# PART V — PERFORMANCE TARGETS

All numbers below are prototype acceptance targets rather than promises for every future machine.

## TARGET FRAME RATE

# 60 FPS

at 1920 × 1080 on the reference performance class.

## SIMULATION TICK RATE

# 20 HZ

One simulation tick:

# 50 MS OF SIMULATION TIME

## INPUT-TO-SIMULATION

Offline/local:

- average input wait to next tick: approximately **25 ms**;
- maximum normal wait: **50 ms**.

Immediate cursor/order feedback remains within one rendered frame.

## RENDER FRAME BUDGET

At 60 FPS:

# 16.67 MS TOTAL

Primary targets:

- CPU game/render preparation p95: ≤10 ms;
- GPU p95: ≤14 ms.

## AUTHORITATIVE SIMULATION

Normal:

# ≤2.5 MS P95 PER TICK

Stress:

# ≤4.0 MS P99

## PATHFINDING

Normal:

# ≤1.5 MS P95 OF A SIMULATION TICK

Stress:

# ≤3.0 MS P99

## ANIMATION

# ≤2.0 MS CPU P95 PER RENDER FRAME

## VFX

CPU:

# ≤1.0 MS P95

GPU:

# ≤2.5 MS P95

during the standard 45-unit battle benchmark.

## UI

# ≤1.5 MS CPU P95

during:

- 128-entity selection;
- production activity;
- minimap activity;
- alerts;
- faction HUD.

## NETWORK TARGET

Per active player client:

- upstream average: ≤16 KB/s;
- downstream average: ≤64 KB/s;
- downstream stress peak: ≤128 KB/s.

## MEMORY TARGET

Client:

- normal working set ≤4 GB;
- prototype hard warning at 6 GB.

VRAM at 1080p High:

- target ≤4 GB.

Dedicated server per ordinary match instance:

- target ≤1 GB.

## LOADING

Cold skirmish load:

- NVMe target ≤10 s;
- SATA-class SSD target ≤20 s.

Return to frontend:

- ≤5 s.

## REFERENCE HARDWARE CLASS

Performance certification assumes approximately:

- six modern desktop CPU cores;
- 16 GB system RAM;
- dedicated DX12-class GPU with at least 6 GB VRAM;
- SSD storage.

Specific consumer SKUs are not part of canon.

---

# PART VI — FIXED SIMULATION MODEL

Gameplay uses:

# FIXED 20 HZ AUTHORITATIVE SIMULATION

with:

# VARIABLE-RATE INTERPOLATED PRESENTATION.

Simulation never consumes render `deltaTime`.

## ACCUMULATOR

Client/offline host:

1. accumulate real time;
2. execute complete 50 ms ticks;
3. retain remainder;
4. interpolate rendering between published simulation states.

## MAXIMUM CATCH-UP

Maximum simulation work during one rendered frame:

# FOUR TICKS

Authoritative time is never discarded.

If the client falls further behind:

- presentation frames may be skipped;
- interpolation quality may reduce;
- simulation ticks are not skipped.

Persistent inability to maintain tick rate is a performance failure.

## PAUSE

Singleplayer pause stops accumulation of simulation ticks.

Presentation UI may continue updating.

## OFFLINE SPEED

Allowed implementation baseline:

- 0.5×;
- 1.0×;
- 1.5×;
- 2.0×.

These multiply real-time feed into the same 20-Hz simulation timeline.

They do not change the number of ticks per simulation second.

## RANKED MULTIPLAYER

# FIXED 1.0×

No player-controlled speed change.

## BENCHMARK ESCAPE HATCH

20 Hz is the canonical baseline.

A prototype benchmark may move the entire game to **30 Hz** only if 20 Hz fails both:

- perceptual command/movement quality;
- timing-quantization quality.

The change must occur before network/replay format lock.

It is not permitted to use different rates for different game modes.

---

# PART VII — SIMULATION VS PRESENTATION

## AUTHORITATIVE SIMULATION OWNS

- position;
- orientation;
- movement intention;
- path;
- reservation;
- HP;
- armor;
- target;
- weapon state;
- projectile;
- resources;
- Energy generation/reserve/demand;
- Crystals;
- committed Crystals;
- Charge;
- Operations Capacity;
- production;
- research;
- construction;
- repair;
- Worksite graph;
- Forward Service;
- Tube topology;
- Tube transit;
- visibility;
- hazard state;
- transformation/refit/deployment;
- passenger state;
- destruction timing;
- game rules.

## PRESENTATION OWNS

- rendered transform interpolation;
- mesh;
- LOD;
- pose;
- wheel rotation;
- track animation;
- drill animation;
- particles;
- sparks;
- cosmetic debris;
- camera shake;
- screen effects;
- UI transition animation;
- sound;
- non-gameplay light flicker.

## HARD RULES

A muzzle flash never causes damage.

An animation event never grants permission to fire.

A Rigidbody collision never decides gameplay collision.

A particle entering a target never proves a projectile hit.

A visual Tube capsule never determines passenger arrival.

A building animation finishing never determines construction completion.

---

# PART VIII — DETERMINISM STRATEGY

The simulation aims for:

# BIT-REPRODUCIBLE AUTHORITATIVE GAMEPLAY

for the supported x86-64 build and headless simulation.

## AUTHORITATIVE NUMERICS

Positions, speeds, acceleration, distances and relevant scalar fractions use a project-owned:

# Q16.16 FIXED-POINT TYPE

internally named:

`Fix32`.

64-bit intermediates are used for multiply/divide.

## INTEGER STATE

Use integers for:

- HP;
- base damage;
- resources;
- Crystal count;
- Operations Capacity;
- command sequence;
- ticks;
- production progress where integral tick count suffices.

## ANGLES

Orientation uses:

# UINT16 TURN UNITS

where:

- 0 = 0°;
- 16384 = 90°;
- 32768 = 180°;
- 65535 wraps to 0°.

Trigonometric lookup tables are generated once and checked into deterministic test data.

## FLOATING POINT

`float` and `double` are forbidden in authoritative simulation logic.

They are permitted when converting published simulation state into Unity presentation transforms.

## ENGINE PHYSICS

Unity PhysX is not authoritative.

Gameplay uses:

- custom circles;
- footprint masks;
- grid occupancy;
- fixed-point distance tests.

PhysX may be used for:

- cosmetic debris;
- camera ray interaction with non-gameplay presentation proxies.

## ITERATION

Gameplay-relevant collection iteration is explicitly ordered.

Do not rely on:

- `Dictionary` enumeration;
- `HashSet` enumeration;
- GameObject hierarchy order;
- callback registration order.

Dictionaries may be used for direct key lookup when their enumeration cannot affect state.

## PATH TIE BREAKING

Path nodes sort by:

1. total cost;
2. heuristic cost;
3. grid coordinate / portal ID;
4. deterministic insertion ordinal.

## COMMAND ORDERING

At one execution tick:

1. server-assigned execution tick;
2. player slot;
3. player command sequence.

Entity lists within commands are ascending Entity ID.

## STATE HASH

A stable 64-bit gameplay state hash is generated every:

# 20 TICKS — ONCE PER SIMULATION SECOND

Development determinism tests may hash every tick.

The hash traverses authoritative state in stable ID order.

---

# PART IX — RANDOM NUMBER POLICY

Normal competitive combat uses:

# NO GAMEPLAY RNG FOR HIT, MISS, CRIT OR EVASION.

## GAMEPLAY RNG

Any future gameplay-relevant random sequence must use explicit deterministic streams.

Baseline generator:

# PCG32

with:

- match seed;
- stream category ID;
- deterministic counter/state.

## STREAM OWNERSHIP

Separate deterministic streams exist for:

- authored procedural map choices, if used;
- neutral/campaign behaviors that genuinely require randomness;
- scripted scenario selection.

Normal weapons do not consume these streams.

## COSMETIC RNG

Presentation has separate client-local cosmetic randomness for:

- particle direction;
- tiny debris variation;
- sound variant choice;
- idle animation timing.

Cosmetic RNG:

- is not serialized;
- is not hashed;
- never affects simulation.

## REPLAY

Gameplay RNG initial states are recorded in the initial replay state.

Cosmetic reproduction is not required.

---

# PART X — WORLD COORDINATE SYSTEM

Canonical gameplay measurements remain in:

# BUILD-GRID CELLS.

## WORLD CONVERSION

# 1 BUILD CELL = 2.0 UNITY METERS.

Simulation coordinate `(1.0, 1.0)` therefore renders at approximately `(2 m, 2 m)` in Unity X/Z.

## AUTHORITATIVE POSITION

Stored in cell-space `Fix32`.

## BUILD GRID

Standard 1v1:

# 160 × 160 CELLS.

## NAVIGATION GRID

Navigation resolution:

# 0.5 BUILD CELL

or approximately:

# 1 METER.

Standard map:

# 320 × 320 NAVIGATION NODES.

## ELEVATION

Gameplay stores:

- discrete elevation band;
- deterministic ramp/interpolated gameplay height where required.

Visual terrain may be smoother.

## TRUE AIR

True air has:

- ground-projected X/Z navigation position;
- one abstract air movement layer;
- presentation altitude from Visual Profile.

Manual altitude does not exist.

## LOCAL/GLOBAL TRANSFORMS

Simulation position is always map-global.

Unity transforms may use normal scene hierarchy for visuals, but gameplay never derives coordinates from parent transforms.

---

# PART XI — MAP REPRESENTATION

A map is not one terrain mesh.

It is a package containing authoritative gameplay layers plus visual content.

The standard map represents:

- visual terrain;
- build grid;
- navigation grid;
- elevation;
- rough ground;
- shallow liquid;
- deep liquid;
- cliffs/impassable terrain;
- ramps;
- bridges;
- non-buildable regions;
- air blockers;
- hazards;
- Excavatable Features;
- resources;
- expansions;
- spawns;
- neutral entities;
- scripted campaign metadata.

Gameplay rules are never recoverable only by inspecting visual geometry.

---

# PART XII — MAP DATA LAYERS

| Layer | Authority | Source |
|---|---|---|
| Visual Terrain | Presentation | Authored |
| Navigation Base Flags | Simulation | Authored + compiled |
| Buildability | Simulation | Authored + derived |
| Elevation Bands | Simulation | Authored |
| Ramp Data | Simulation | Authored |
| LoS Occlusion | Simulation | Authored + derived |
| Air Blocking | Simulation | Authored |
| Rough Ground | Simulation | Authored |
| Shallow Liquid | Simulation | Authored |
| Deep Liquid | Simulation | Authored |
| Impassable Terrain | Simulation | Authored |
| Excavatable Features | Simulation | Authored entities |
| Hazard Zones | Simulation | Authored entities |
| Resource Nodes | Simulation | Authored entities |
| Expansion Zones | Simulation/validation | Authored |
| Player Starts | Simulation | Authored |
| Neutral Objects | Simulation | Authored |
| Competitive Validation | Tooling | Derived |
| Campaign Hooks | Scenario | Authored |

Derived data is rebuilt by a deterministic map compiler rather than manually maintained.

---

# PART XIII — MAP FILE FORMAT

Map source is modular.

## SOURCE PACKAGE

`MapManifest`

Contains:

- Map ID;
- Display Name localization key;
- Biome ID;
- map-format version;
- bounds;
- player count;
- player starts;
- navigation compiler version;
- source-content hash.

`GameplayLayers`

Contains authored cell layers.

`GameplayFeatures`

Contains:

- resources;
- Excavatable Features;
- hazards;
- neutral objects;
- expansion zones;
- air blockers;
- scripted markers.

`VisualScene`

Contains Unity terrain, scenery, lighting anchors and presentation content.

`CampaignHooks`

Optional.

## COMPILED PACKAGE

Build compilation outputs:

- `manifest.json`;
- `grid.bin`;
- `features.bin`;
- `validation.json`;
- visual Unity scene/content bundle.

`grid.bin` is a versioned, little-endian, explicitly packed format.

No localized text is included in gameplay identity.

---

# PART XIV — PATHFINDING NORTH STAR

# ONE STRATEGIC ROUTE PER MOVEMENT INTENT,

# CHEAP LOCAL CORRECTION PER UNIT,

# EXPLICIT CLEARANCE,

# AND EVENT-DRIVEN TOPOLOGY UPDATES.

The project does **not** run independent full-map A* for every unit every frame.

Pathfinding must remain understandable enough to debug visually.

---

# PART XV — NAVIGATION REPRESENTATION

The canonical system is:

# HIERARCHICAL GRID A* + LOCAL GRID CORRIDORS + DETERMINISTIC STEERING/RESERVATION.

## WHY NOT NAVMESH

A navmesh is excellent for many character games but is a poorer authoritative representation for this project because:

- buildability already uses grid cells;
- Excavatable Terrain changes explicit topology;
- structures alter occupancy;
- clearance classes matter;
- replay tie-breaking must be controlled;
- Martian/hover movement uses distinct rules.

## WHY NOT PURE FLOW FIELDS

Flow fields are excellent for many agents with one destination but unnecessarily constrain:

- mixed clearances;
- small medium-scale groups;
- exact formation destination behavior;
- frequent independent commands.

Flow fields remain a possible optimization for mass evacuation or AI analysis, not baseline movement.

## LEVELS

1. strategic cluster route;
2. local corridor path;
3. formation guidance;
4. individual deterministic local avoidance.

---

# PART XVI — HIERARCHICAL PATHFINDING

Default cluster size:

# 10 × 10 BUILD CELLS

equivalent to:

# 20 × 20 NAV NODES.

A 160 × 160 build map therefore contains:

# 16 × 16 = 256 CLUSTERS.

## PORTALS

Cluster edges compile contiguous traversable runs into portals by:

- movement profile;
- clearance class.

## ROUTE CACHE

Cache key contains:

- origin cluster;
- destination cluster;
- movement profile;
- clearance class;
- topology version.

## DYNAMIC UPDATES

When a structure is added/removed or an Excavatable Feature opens:

- affected navigation cells update;
- affected cluster portals rebuild;
- immediately neighboring portal relationships rebuild;
- topology version increments.

The whole map does not rebake.

## BENCHMARK

Prototype compares:

- 8-cell clusters;
- 10-cell baseline;
- 16-cell clusters.

10 cells remains canonical unless another value reduces stress-path p95 materially without increased stuck/route-quality failures.

---

# PART XVII — UNIT FOOTPRINTS

Phase 06 collision radii remain authoritative.

| Class | Collision Radius | Baseline Nav Clearance |
|---|---:|---:|
| Tiny | 0.35 cell | 1 nav node |
| Small | 0.55 | 2 |
| Medium | 0.85 | 2 |
| Large | 1.15 | 3 |
| Huge | 1.60 | 4 |

Navigation clearance is conservative grid clearance.

Actual local collision uses the exact fixed-point radius.

Structure exits include an explicit clearance class for their largest producible unit.

---

# PART XVIII — LOCAL AVOIDANCE

The canonical solution is:

# DETERMINISTIC CANDIDATE-VELOCITY STEERING + SHORT-TERM RESERVATION.

Full reciprocal-velocity-obstacle simulation is rejected as unnecessary and harder to make deterministic.

## CANDIDATE VELOCITIES

Units evaluate a fixed ordered set around desired heading:

- desired;
- ±15°;
- ±30°;
- ±45°;
- ±60°;
- ±90°;
- slow;
- stop.

Scores consider:

- route progress;
- collision prediction;
- reserved cells;
- formation slot;
- heading change;
- priority.

## NEIGHBORS

Neighbors are obtained from deterministic spatial buckets and processed by Entity ID.

## FRIENDLY COMPRESSION

Phase 06's:

# UP TO 15% FOR 1.5 SECONDS

is preserved.

Compression may solve a passing interaction.

It never permits permanent friendly stacking.

## ENEMIES

Enemy collision is hard.

Normal units do not phase through one another.

---

# PART XIX — UNIT RESERVATION

Movement uses a rolling reservation horizon of:

# 12 TICKS — 0.6 SECONDS.

Longer special reservations exist for exits/unloading.

Priority:

1. explicit structure/unload spawn reservation;
2. Huge;
3. Large;
4. Medium;
5. Small;
6. Tiny;
7. Entity ID tie break.

## USE CASES

Reservations protect:

- Chrome Crusher routes;
- major structure exits;
- transport unload;
- Tube exits;
- narrow-passage exchange;
- formation leaders.

## DEADLOCK RESPONSE

A unit that fails meaningful progress:

- after 1 second: increases local avoidance penalty;
- after 2 seconds: requests local path refresh;
- after 3 seconds: asks formation manager for slot reassignment;
- after 5 seconds: records a path-deadlock diagnostic and requests strategic repath.

Persistent reachable-path stalls beyond 5 seconds fail navigation acceptance tests.

---

# PART XX — FORMATIONS

Phase 06's role-aware loose formation remains the canonical battlefield formation.

## ROLE ORDER

1. short-range/heavy frontline;
2. medium ranged;
3. anti-air distributed through body;
4. support;
5. undeployed siege;
6. separate air echelon.

## SLOT GENERATION

Formation manager:

1. determines destination heading;
2. determines usable corridor width;
3. generates staggered slots;
4. sorts units by role and footprint;
5. assigns slots deterministically.

## DYNAMIC RESIZING

Formation slot maps change only when:

- group membership changes materially;
- route width enters another bucket;
- formation mode changes;
- group receives a new destination;
- obstruction makes current arrangement invalid.

Units do not constantly re-solve the whole formation.

## CORRIDOR COMPRESSION

Wide formation compresses into columns.

Once clear space is restored, rear units spread gradually rather than halting the group to reconstruct a perfect parade shape.

## SPREAD

Spread increases nominal separation by the canonical:

# APPROXIMATELY 40%.

## BREAK CONDITIONS

Formation cohesion may temporarily break for:

- attack pursuit;
- explicit focus orders;
- collision avoidance;
- unloading;
- Tube arrival;
- narrow passages.

Regrouping occurs opportunistically.

---

# PART XXI — TRUE AIR

True air uses a dedicated navigation layer.

## NAVIGATION

Aircraft navigate on a coarse 1-build-cell air grid.

They ignore:

- ground obstacles;
- cliffs;
- water;
- rubble.

They respect:

- map flight bounds;
- authored air-blocking volumes.

## COLLISION

Aircraft have horizontal spacing circles.

Vertical separation is not player-controlled.

Presentation may vertically offset crossing craft slightly for clarity without changing simulation position.

## GROUND ANCHOR

Every true-air entity owns an authoritative projected ground anchor.

It drives:

- selection;
- minimap location;
- navigation destination;
- ground-relative targeting.

## SHADOW

Presentation shadow follows the projected anchor.

## LANDING/DEPLOYMENT

A true-air asset changing to a ground state validates:

- ground footprint;
- occupancy;
- air-to-ground deployment clearance.

---

# PART XXII — GROUND HOVER

Hover uses the ground navigation architecture.

Movement profiles determine which cell flags it ignores.

A typical hover profile may ignore:

- Rough Ground penalty;
- shallow liquid;
- minor rubble.

It remains blocked by:

- cliff edges;
- solid walls;
- Deep Liquid where canon does not explicitly permit crossing;
- normal impassable terrain.

Hover never enters the true-air navigation system merely because the model visually floats.

---

# PART XXIII — DYNAMIC TERRAIN / EXCAVATION

Every Excavatable Feature is an authoritative entity.

## STATE

- Blocked;
- ActiveExcavation;
- Open.

## DATA

Feature contains:

- Stable ID;
- occupied navigation cells;
- visual profile;
- required Energy;
- progress ticks;
- eligible excavation categories;
- newly opened navigation mask;
- fog/LoS consequences.

## COMMAND

`Excavate(featureEntityId)`

The simulation validates:

- ownership;
- eligible unit;
- path/reach;
- Energy;
- feature state.

## COMPLETION

On final tick:

1. feature becomes Open;
2. blocking cells change;
3. relevant LoS cells change if applicable;
4. affected path clusters rebuild;
5. nearby route caches invalidate;
6. presentation receives a deterministic completion event.

Multiplayer sends no special nondeterministic terrain state; the authoritative feature state is part of snapshots and replay state.

---

# PART XXIV — FOG OF WAR

The canonical model is:

# CPU AUTHORITATIVE VISIBILITY GRID + GPU PRESENTATION TEXTURE.

## RESOLUTION

# 1 BUILD CELL PER FOG CELL.

Standard map:

# 160 × 160.

Each player owns:

- explored bitset;
- current ground visibility count;
- current air visibility count;
- detection state.

## CURRENT VISIBLE

A cell is visible while visibility count >0.

## EXPLORED

Once visible, terrain exploration remains remembered.

## UNSEEN

No terrain-detail knowledge beyond permitted map-baseline presentation.

## ENEMIES

The authoritative server sends enemy live state only while legally visible.

## LAST KNOWN

Client stores last legally received:

- position;
- type;
- visible configuration;
- last seen tick.

This is presentation knowledge, not hidden server truth.

## RESOURCES

When visibility is lost, client retains last-seen amount.

## GPU

CPU masks update a small texture which the renderer:

- upsamples;
- softens;
- combines with terrain.

The visual blur never changes simulation visibility.

---

# PART XXV — LINE OF SIGHT

Gameplay LoS uses deterministic grid occlusion.

No per-unit-to-target physics-raycast matrix exists.

## SOURCE

Each vision source knows:

- grid cell;
- elevation band;
- sight radius;
- air/ground status;
- detector radius.

## UPDATE

A vision source recomputes its contribution when:

- its fog cell changes;
- sight radius changes;
- elevation changes;
- relevant local occlusion topology changes.

## ALGORITHM

Precomputed radial offset tables plus deterministic shadow/occlusion scanning are used.

## OCCLUDERS

- major cliffs;
- authored wall cells;
- designated structures;
- authored large geometry blockers.

Ordinary units are not LoS blockers.

## TRUE AIR

Uses its appropriate air-vision profile.

Flying does not automatically reveal everything behind major designated blockers.

---

# PART XXVI — ENTITY ARCHITECTURE

The canonical gameplay architecture is:

# CUSTOM HYBRID DATA-ORIENTED COMPONENT MODEL.

It is neither:

- one MonoBehaviour per gameplay rule;
- a giant inheritance hierarchy;
- nor an engine-wide pure ECS requirement.

## WHY

The expected simulation entity scale is hundreds, not tens of thousands.

The architecture therefore values:

- deterministic ordering;
- easy debugging;
- serializability;
- composition;
- efficient arrays;

without forcing every feature into highly abstract ECS infrastructure.

## ENTITY ID

Match Entity ID:

# UINT32

allocated monotonically.

Entity IDs are never reused during a match.

Component-storage slots may be reused internally.

## IDENTITY

An entity retains the same Entity ID through:

- Mission Refit;
- transformation;
- deployment;
- loading;
- unloading;
- Tube transit.

This directly preserves Phase 07 control-group semantics.

---

# PART XXVII — GAMEPLAY COMPONENT MODEL

Core simulation components include:

| Component | Responsibility |
|---|---|
| SimTransform | Position/orientation |
| Ownership | Player/team |
| Health | HP/destruction |
| Armor | Rating/class |
| Movement | Speed/acceleration/turn profile |
| Navigation | Path/corridor/reservation |
| WeaponSet | Weapon profiles/cooldowns |
| Targeting | Current target/priorities |
| Vision | Sight/detection |
| Worker | Labor capability |
| ResourceCarrier | Payload |
| Builder | Construction |
| Production | Build queue |
| Research | Research queue/provider |
| EnergyProducer | E/s/reserve |
| EnergyConsumer | Demand/priority |
| CapacityProvider | OC provision |
| Transport | Capacity/loading |
| Passenger | Loaded state |
| Repair | Repair capability/job |
| Service | Service-region function |
| Transformation | State machine |
| Deployment | Deploy state |
| Refit | Configuration state |
| WorksiteNode | Raider connectivity |
| ResonanceCore | Crystal commitment/Charge |
| ChargeSource | Charge contribution |
| TubeStation | Martian endpoint |
| TubeLink | Martian edge |
| Displacement | Mechanical movement capability |
| Stability | Anti-chain-displacement timer |
| Excavatable | Terrain feature |
| Hazard | Deterministic environmental behavior |
| ResourceNode | Ore/Crystal state |
| NeutralAI | Rock Monster etc. |

Presentation counterparts are stored outside `Game.Sim`.

---

# PART XXVIII — DATA-DRIVEN UNIT DEFINITIONS

Canonical conceptual schema:

`UnitDefinition`

- `schemaVersion`
- `stableId`
- `numericContentId`
- `factionId`
- `displayNameLocKey`
- `sourceClassification`
- `sourceRegistryRefs[]`
- `gameplayTags[]`
- `techStage`
- `productionStructureId`
- `prerequisites[]`
- `cost`
  - Ore
  - Energy
  - Crystals
- `buildTicks`
- `operationsCapacity`
- `maxHP`
- `targetClass`
- `armorRating`
- `footprintClass`
- `movementProfileId`
- `speed`
- `accelerationClass`
- `turnClass`
- `reverseRatio`
- `sightCells`
- `detectionCells`
- `weaponProfileIds[]`
- `commandIds[]`
- `transportRules`
- `repairRules`
- `transformationId?`
- `refitProfileId?`
- `factionSystemHooks[]`
- `visualProfileId`
- `animationProfileId`
- `vfxProfileId`
- `uiProfileId`.

Per-unit C# subclasses are prohibited unless behavior truly cannot be represented by composable systems.

---

# PART XXIX — DATA-DRIVEN BUILDING DEFINITIONS

`BuildingDefinition`

- `schemaVersion`
- `stableId`
- `numericContentId`
- `factionId`
- `displayNameLocKey`
- `sourceClassification`
- `sourceRegistryRefs[]`
- `footprintMask`
- `rotationsAllowed`
- `exitDefinitions[]`
- `buildabilityRules[]`
- `cost`
- `buildTicks`
- `maxHP`
- `targetClass`
- `armorRating`
- `energyProduction`
- `energyDemand`
- `reserveCapacity`
- `energyPriorityClass`
- `operationsCapacityProvided`
- `productionEntries[]`
- `researchEntries[]`
- `serviceProfile?`
- `networkProfile?`
- `defenseWeaponProfiles[]`
- `constructionProfileId`
- `destructionProfileId`
- `visualProfileId`
- `uiProfileId`.

---

# PART XXX — DATA VALIDATION

Content compilation fails development builds for errors including:

- duplicate Stable ID;
- duplicate numeric content ID;
- missing production source;
- missing cost;
- missing build time;
- missing OC;
- invalid weapon;
- unresolved visual profile;
- unresolved UI command;
- unresolved technology prerequisite;
- technology cycle;
- invalid transformed-state relationship;
- missing target-layer definition;
- invalid Tube eligibility;
- illegal footprint;
- no valid production exit;
- invalid faction reference;
- missing localization key;
- missing animation state referenced by presentation;
- source-classification omission for roster content;
- incorrect expected roster counts.

The compiler explicitly asserts:

# 35 BUILDABLE UNIT DEFINITIONS

and:

# 31 INFRASTRUCTURE DEFINITIONS

excluding map-instanced Tube segments where the roster treats Aero Tube Link as one infrastructure entry.

Development does not silently substitute defaults for broken canonical content.

---

# PART XXXI — STABLE IDENTIFIERS

Stable IDs use lowercase dotted namespaces.

Examples:

- `unit.rr.chrome_crusher`
- `unit.ast.t3_trike`
- `unit.ali.etx_infiltrator`
- `unit.mar.excavation_searcher`
- `building.mar.aero_tube_hangar`
- `research.ali.resonance_initiation`
- `command.global.attack`
- `weapon.ast.mission_fighter.interceptor_lance`
- `map.meridian.basalt_highlands.01`.

## NUMERIC IDS

A checked-in Stable ID Registry assigns explicit numeric IDs.

IDs:

- are never generated from localized names;
- are never Unity Instance IDs;
- are never reused for another content item;
- are not recomputed from hashes.

Display names may change without invalidating:

- saves;
- replays;
- map data.

---

# PART XXXII — GAME COMMAND ARCHITECTURE

Player interaction becomes `CommandEnvelope`.

Conceptual fields:

- protocol version;
- player slot;
- client command sequence;
- command type;
- sorted Entity IDs;
- command payload.

Accepted authoritative log adds:

- execution tick;
- server command ordinal.

Canonical command families include:

- Move;
- Attack;
- AttackMove;
- Stop;
- Hold;
- Patrol;
- Harvest;
- Build;
- Repair;
- Load;
- Unload;
- Excavate;
- Transform;
- Deploy;
- Refit;
- CommitCrystal;
- WithdrawCrystal;
- Surge;
- Shunt;
- TubeTransfer;
- TubeBuild;
- ProtectorStance;
- SearcherBrace;
- ExcavationClamp.

## PAYLOAD EXAMPLES

`Move`

- selected entity IDs;
- fixed-point destination;
- Spread state if relevant.

`Attack`

- selected entity IDs;
- target Entity ID.

`Build`

- worker IDs;
- Building Stable ID;
- cell anchor;
- rotation.

`Refit`

- entity ID;
- target configuration;
- service-facility Entity ID.

`TubeTransfer`

- passenger entity IDs;
- destination Station ID.

Every payload contains intent—not presentation details.

---

# PART XXXIII — COMMAND BUFFERING

## INPUT

Client collects input every rendered frame.

## IMMEDIATE FEEDBACK

Before simulation acceptance the client may show:

- cursor state;
- move marker;
- target marker;
- queue visualization.

This remains cosmetic anticipation.

## TICK ASSIGNMENT

Server executes a valid command on the next legal simulation tick after receipt and validation.

There is no artificial fixed multiplayer input delay in the baseline architecture.

## ACKNOWLEDGMENT

Server returns:

- client sequence;
- accepted/rejected;
- execution tick;
- optional deterministic rejection code.

## SHIFT QUEUE

Queued commands exist as authoritative order entries.

## INVALID COMMAND

Rejected commands do not enter:

- simulation command log;
- replay.

## REPLAY

Only accepted authoritative commands are recorded.

---

# PART XXXIV — SELECTION ARCHITECTURE

Selection is client-side UX.

Authoritative simulation does not contain `Selected=true`.

## SELECTION SET

Stores Entity IDs.

Maximum implementation selection:

# 128

as established in Phase 07.

## CLICK TESTING

Presentation objects expose lightweight selectable proxies mapping to Entity ID.

## MARQUEE

Screen-space projected strategic anchors are tested.

## TRUE AIR

Uses projected selection anchor plus direct aircraft hull proxy.

## SAME TYPE

Comparison uses Stable Unit/Building ID, never model name.

## DEAD ENTITY

Authoritative destruction event removes it from client selection.

---

# PART XXXV — CONTROL GROUP PERSISTENCE

Control groups store logical Entity IDs.

Therefore:

- MX-41 transformation;
- Protector stance;
- Searcher brace;
- Mission Refit;
- loading;
- Tube transit

do not change group membership.

A destroyed entity is removed.

A transported unit's group camera center resolves to the carrier while loaded.

---

# PART XXXVI — ECONOMY CORE

The economy does not use one generic `Dictionary<Resource,int>` as its entire model.

Canonical categories:

## GLOBAL / SHARED

Used where faction rules permit direct faction-wide spending.

## LOCAL

Physically associated with:

- Worksite;
- Martian settlement;
- carrier/resource node.

## COMMITTED

Alien committed Crystals.

## DERIVED

- Charge;
- Operations Capacity;
- Energy balance;
- network throughput.

## RESOURCE TYPES

### ORE

Integral strategic resource.

### ENERGY

Contains:

- generation per simulation time;
- reserve;
- reserve capacity;
- continuous demand.

### CRYSTALS

Integral count.

### OPERATIONS CAPACITY

Derived current / reserved / maximum.

Phase 04's faction-specific ownership and macro relationships remain unchanged.

---

# PART XXXVII — ENERGY DOMAIN SYSTEM

`EnergyDomainSystem` maintains graph components rather than recalculating the whole base every tick.

Each domain caches:

- member Entity IDs;
- generators;
- consumers;
- reserve;
- reserve capacity;
- generation;
- demand;
- powered/unpowered state.

## ROCK RAIDERS

Domain follows connected Worksite components.

## ASTRONAUTS

Domain follows expedition power/service coverage relationships.

## ALIENS

Structures belong to operational Command Core domains.

## MARTIANS

Connected Aero Tube settlement component forms the domain.

## RECALCULATION TRIGGERS

- generator completion/destruction;
- consumer completion/destruction;
- priority change;
- network connection/disconnection;
- domain ownership change;
- transformation that changes demand.

Normal ticks only integrate cached supply/demand.

---

# PART XXXVIII — BROWNOUT SYSTEM

Brownout recalculation is event-driven.

Phase 04's functional ordering and High/Normal/Low per-building priority are preserved.

Deterministic order:

1. canonical functional class;
2. player priority override;
3. Entity ID.

When available generation/reserve can no longer support all demand:

- lower-priority complete consumers shut down;
- partial nondeterministic power is not distributed.

When supply returns:

- structures reactivate in the same deterministic order.

A state-change event is emitted once.

Presentation may fade lights or stop machinery but never owns the shutdown decision.

---

# PART XXXIX — OPERATIONS CAPACITY

Capacity state contains:

- current active;
- reserved in production;
- maximum.

A production order validates:

`current + reserved + productOC <= maximum`.

On production start:

- OC becomes reserved.

On cancellation:

- reservation releases.

On completion:

- reserved becomes active.

When a capacity-providing building is destroyed and active OC exceeds capacity:

- existing units remain legal;
- new production cannot begin until capacity is restored.

No unit is deleted.

---

# PART XL — ROCK RAIDER WORKSITE NETWORK

Worksite nodes:

- Rock Raiders HQ;
- Vehicle Service Bay.

Phase 04 radii remain:

- HQ: 18 cells;
- Service Bay: 12 cells.

Two nodes connect if service geometry overlaps according to the canonical rules.

The graph caches connected components.

Each component owns:

- processed-resource accessibility;
- Energy Domain identity;
- service membership;
- automated hauling endpoints;
- repair/service eligibility.

A disconnected building retains whatever local state Phase 04 permits rather than becoming instantly invalid.

---

# PART XLI — AUTOMATED ROCK RAIDER HAULING

Automated hauling is an authoritative logistics system but does not introduce player-selectable Phase 03 units.

## HAUL JOB

Contains:

- source;
- destination;
- resource type;
- amount;
- route;
- progress/state.

## PRESENTATION

Visible hauling rigs are presentation proxies bound to jobs.

They are not individually:

- selected;
- targeted as normal combat units;
- included in OC.

Where harassment must affect logistics, the authoritative abstraction exposes the canonical vulnerable logistics relationship without requiring hundreds of simulation carts.

## ROUTING

Jobs select deterministic eligible receiving/processing endpoints by:

1. valid Worksite component;
2. route cost;
3. endpoint load;
4. Entity ID.

---

# PART XLII — ASTRONAUT FORWARD SERVICE

Forward Service uses spatial membership.

Service sources include canonical:

- Service & Refit Hub;
- deployed Solar Explorer where applicable.

The system caches service-provider spatial buckets.

A unit updates service state when:

- it moves between provider query cells;
- provider deploys/undeploys;
- provider is destroyed;
- ownership changes.

No every-unit/every-provider full scan runs each tick.

---

# PART XLIII — MISSION REFIT

Mission Refit is an explicit service job.

## COMMAND VALIDATION

Checks:

- unit is Refit-eligible;
- legal source/target configuration;
- service provider is active;
- player can pay cost;
- Configuration Lock permits change;
- unit is not in illegal transport/deployment state.

## JOB

Stores:

- entity ID;
- old configuration;
- new configuration;
- remaining ticks;
- committed resources.

## COMPLETION

The same Entity ID changes configuration.

Components whose data changes are refreshed from compiled configuration data.

No replacement entity is spawned.

## PRESENTATION

Module removal/installation is driven by normalized authoritative job progress.

---

# PART XLIV — ALIEN CRYSTAL COMMITMENT

A Resonance Core owns explicit commitment slots.

A committed Crystal is:

- still an ordinary Crystal resource identity;
- removed from spendable reserve;
- associated with one Core.

Commit/withdraw are explicit commands.

Core destruction invokes Phase 04 salvage behavior.

Charge is a derived deterministic integer/fixed-point state.

No floating gauge independently advances in UI.

---

# PART XLV — ALIEN CHARGE GENERATION

Charge generation is integrated from cached Core state.

Recommended internal unit:

# MILLICHARGE

where:

# 1000 MILLICHARGE = 1 DISPLAYED CHARGE.

This avoids fractional accumulation issues.

Core generation changes only when:

- commitment changes;
- Core state changes;
- Energy state changes;
- research changes.

UI rounds/displays canonical Charge values.

---

# PART XLVI — ALIEN SURGE

Phase 06 Surge remains:

- 50 Charge;
- 0.75-second visible buildup;
- 18-second active window;
- ×0.80 weapon cooldown;
- ×0.70 ETX reconfiguration duration;
- no HP, armor, range or movement bonus.

Simulation represents cadence changes as rational integer ratios rather than floating multipliers.

Multiple Surge coverage never stacks.

Eligible unit queries use spatial buckets.

Entering/leaving coverage updates effective cooldown rate deterministically.

---

# PART XLVII — MARTIAN AERO TUBE NETWORK

The Tube Network is an explicit graph.

Nodes:

- Aero Tube Hangar;
- Settlement Station;
- other canonical Station endpoints.

Edges:

- Aero Tube Links.

Each Link stores:

- endpoint IDs;
- ordered route cells;
- length;
- Energy demand;
- operational state.

## CONNECTED COMPONENTS

Because Links can be destroyed, a simple insertion-only union-find is insufficient.

The network uses:

- cached connected-component IDs;
- targeted breadth-first recomputation when an edge/node changes.

At this project scale, graph size is tiny and correctness is more important than exotic dynamic-connectivity algorithms.

---

# PART XLVIII — TUBE NETWORK SEGMENTATION

When a Link or Station disappears:

1. affected old component is marked dirty;
2. BFS discovers surviving connected components;
3. resource/Energy pooling membership updates;
4. in-flight transfers validate their routes;
5. UI receives one segmentation event.

Existing local settlement storage remains owned locally according to Phase 04.

The entire Martian network is never recomputed every simulation tick.

---

# PART XLIX — TUBE TRANSFER

Tube-eligible competitive assets remain:

- Worker Robot;
- Double Hover;
- Jet Scooter.

A transfer job contains:

- passenger Entity ID;
- origin;
- destination;
- selected route;
- departure tick;
- travel ticks;
- current edge.

Passenger:

- retains Entity ID;
- is removed from ground spatial queries;
- cannot be targeted normally in transit.

Presentation spawns the hypersled/capsule visualization.

On arrival, Phase 06's exit-space and Arrival Recovery rules apply.

---

# PART L — MARTIAN DISPLACEMENT

Mechanical displacement never uses rigidbody force.

A displacement event defines:

- target;
- requested vector/distance;
- source;
- size response;
- Stability response.

The resolver:

1. computes allowed canonical distance;
2. clips against impassable geometry;
3. forbids lava/void/illegal destination;
4. checks occupancy;
5. finds nearest legal fixed-point position;
6. updates navigation intention;
7. applies Stability if hostile.

This guarantees replayable displacement.

---

# PART LI — STABILITY

Phase 06's:

# 8-SECOND STABILITY

is represented as `stabilityUntilTick`.

While active:

- additional hostile displacement ×0.25;
- transformation/deployment cannot be cancelled by displacement;
- no additional movement interruption is generated.

No update loop decrements a float timer.

Tick comparison determines state.

---

# PART LII — CONSTRUCTION

Building placement uses:

- integer build-cell anchor;
- legal 90° orientation where applicable;
- compiled footprint mask.

## FLOW

1. client previews ghost;
2. Build command is sent;
3. server revalidates terrain, occupancy, resources and builder;
4. Phase 04 resource reservation occurs;
5. Construction Site entity is created;
6. site progresses through ticks;
7. same entity transitions to Completed Building.

The completed building does not receive a new identity.

## FOOTPRINT

A site reserves its final gameplay footprint from construction start unless Phase 04 explicitly permits otherwise.

---

# PART LIII — PRODUCTION

Every production facility owns an ordered queue.

Queue item contains:

- Stable Unit ID;
- remaining ticks;
- resource commitment state;
- reserved OC;
- optional configured state.

Production is tick-driven.

On completion:

1. exit footprint is validated;
2. short-term spawn reservation is created;
3. entity spawns;
4. OC reservation becomes active;
5. rally command is generated if legal.

If blocked:

- completed unit waits in the facility;
- production state does not repeatedly spawn/delete entities.

---

# PART LIV — RESEARCH

Research definitions compile into a validated DAG.

Research state is owned by player/faction.

A Research job contains:

- Stable Research ID;
- source building;
- remaining ticks;
- cost state.

Completion emits an explicit technology-state event.

Systems subscribe to relevant changes or query compiled technology flags.

No research effect exists only as a UI state.

---

# PART LV — COMBAT SYSTEM

The combat system consumes Phase 06 data rather than re-encoding unit rules in classes.

System order:

1. legal target retention;
2. acquisition;
3. facing/range check;
4. weapon readiness;
5. firing event;
6. projectile creation or immediate resolution;
7. cooldown start.

Damage uses:

- damage type;
- target class;
- armor multiplier;
- explicit deterministic modifier.

The Phase 06 damage matrix and armor rules remain authoritative.

---

# PART LVI — TARGET ACQUISITION

Entity positions are indexed in a deterministic uniform spatial hash/grid.

Recommended bucket:

# 4 × 4 BUILD CELLS.

Separate indices exist for:

- ground;
- true air;
- structures/resources where useful.

A unit queries only buckets intersecting its acquisition radius.

Candidates are scored by canonical target priority.

Tie break:

- distance;
- Entity ID.

No every-attacker/every-enemy full scan occurs.

---

# PART LVII — PROJECTILES

Authoritative projectiles are compact simulation records.

They are **not GameObjects**.

Projectile data includes:

- Entity/Projectile ID;
- owner;
- source;
- target or impact position;
- fixed-point position;
- velocity;
- damage profile;
- lifetime;
- guidance behavior;
- splash profile.

## GUIDED

Tracks original target.

Does not retarget.

## ORDINARY

If target dies:

- continues toward committed impact position.

## SPLASH

Resolves deterministic distance rings.

## BEAM / CONTACT

May resolve immediately according to Phase 06.

Presentation pools visual projectile objects and interpolates from simulation state.

---

# PART LVIII — CONTACT WEAPONS

Drills and contact tools use explicit approach slots.

The target exposes an engagement ring derived from footprint.

Attackers reserve legal approach positions.

Full damage requires:

- correct range;
- canonical facing;
- weapon-specific state.

Granite Grinder ramp timing remains tick-based.

No physics collision callback activates drill damage.

---

# PART LIX — TRANSPORTS

Transport component owns:

- capacity;
- legal passenger profiles;
- loaded passenger IDs;
- loading/unloading state.

Passenger entities remain alive.

While loaded:

- removed from normal spatial index;
- not individually targetable;
- selection/control-group identity retained.

Transport destruction applies Phase 06's deterministic emergency deployment:

- passenger returns at 40% maximum HP;
- 1.5 s attack lock;
- temporary movement penalty.

No random cargo deletion occurs.

---

# PART LX — TRANSFORMATION / DEPLOYMENT

Transformations are data-driven state machines.

`TransformationDefinition` contains:

- legal source states;
- destination state;
- duration ticks;
- movement permission;
- attack permission;
- cancellation rule;
- footprint/movement profile changes;
- presentation profile.

State transition progress is authoritative.

Animation reads normalized progress.

At transition completion:

- movement/target/weapon components swap to destination configuration;
- Entity ID remains unchanged.

---

# PART LXI — REPAIR

Repair is an explicit job or service process.

Simulation owns:

- eligible repairer/provider;
- target;
- HP restored per tick;
- Ore/Energy consumed;
- incoming-damage modifiers;
- service-region rules.

Presentation tools, sparks and LEGO rebuilding motions follow repair progress.

No visual brick snapping directly modifies HP.

---

# PART LXII — DESTRUCTION

At 0 HP the simulation immediately removes gameplay function.

Phase 06 timing remains:

- normal unit collision clears in the same authoritative tick as 0 HP;
- Massive/Huge unit collision clears in the same authoritative tick as 0 HP;
- structure collision clears in the same authoritative tick as 0 HP.

After destruction begins:

- navigation occupancy clears;
- remaining debris is cosmetic.

Presentation may persist debris longer.

A battle can never permanently jam because decorative bricks were left on the pathing grid.

---

# PART LXIII — ROCK MONSTERS & NEUTRAL AI

Rock Monsters are normal authoritative neutral entities.

They use:

- deterministic target priorities;
- canonical aggression radius;
- canonical leash;
- no respawn;
- no random attack result.

Phase 06's neutral-combat framework remains unchanged.

Other neutral interactables use the same entity/component model where appropriate.

---

# PART LXIV — ENVIRONMENTAL HAZARDS

Hazards are authored deterministic state machines.

Example fields:

- active region;
- warmup ticks;
- active ticks;
- cooldown ticks;
- damage per tick;
- movement modifier;
- visibility modifier;
- authored phase/seed.

No physics particle determines damage.

Phase 06's lava, vent, crystal discharge, rockfall, cryogenic, pressure and other hazard rules remain data definitions.

---

# PART LXV — GAME LOOP ORDER

Authoritative simulation tick order is locked as:

1. finalize network/command intake;
2. validate/apply commands;
3. resolve topology mutations due at tick start;
4. economy / resources / Energy / OC;
5. construction / production / research / repair / service jobs;
6. path request scheduling and route results;
7. reservations / movement / transformation / transports / Tube transit;
8. visibility and LoS changes;
9. targeting;
10. weapon firing;
11. projectile movement/impact;
12. damage / displacement / destruction;
13. hazards and end-of-tick timed effects;
14. faction derived-state updates requiring post-combat state;
15. presentation-event emission;
16. state hash;
17. snapshot publication.

Individual systems may internally subdivide but cannot casually reorder gameplay consequences.

---

# PART LXVI — SYSTEM UPDATE FREQUENCY

## EVERY 20-HZ TICK

- command application;
- movement;
- reservations;
- transformation;
- projectile simulation;
- weapon cooldowns;
- transport;
- Tube transit;
- damage;
- destruction.

## EVENT-DRIVEN WITH TICK INTEGRATION

- Energy topology;
- Worksite topology;
- Tube components;
- production;
- research;
- construction;
- Charge generation.

## VISIBILITY

Source contribution updates on movement/state events; dirty work is spread predictably when required.

## AI

Strategic planner:

# 2 HZ

Tactical group planning:

# 4 HZ

## UI SAMPLING

Typical HUD view-model refresh:

# 10–20 HZ

with immediate event updates for important player feedback.

Rendering remains uncoupled.

---

# PART LXVII — SPATIAL QUERY SYSTEM

The simulation maintains fixed-grid spatial buckets.

Queries include:

- entities in radius;
- nearest legal target;
- service providers;
- Surge coverage;
- collision neighbors;
- displacement candidates;
- repair targets;
- resource interaction.

Buckets contain Entity IDs in deterministic order.

Entity movement updates membership only when bucket coordinate changes.

Spatial queries never depend on Unity physics layers.

---

# PART LXVIII — MEMORY / ALLOCATION POLICY

Authoritative simulation must produce:

# ZERO ROUTINE MANAGED ALLOCATIONS PER TICK

after warmup under the benchmark match.

Forbidden in hot loops:

- LINQ;
- temporary lists without pooling;
- boxing;
- closure allocation;
- string formatting.

Baseline preallocation targets:

- 1,024 gameplay entities;
- 8,192 projectile records;
- 2,048 movement/path jobs;
- 1,024 logistics/service jobs.

Containers may grow in development builds, with telemetry indicating capacity overflow.

Entity IDs themselves remain monotonic even if storage slots are recycled.

---

# PART LXIX — MULTITHREADING POLICY

Correctness precedes parallelism.

Baseline simulation systems execute in deterministic serial system order.

Parallelization is permitted for work whose outputs can be deterministically merged, such as:

- independent path searches;
- visibility-source rasterization;
- read-only AI scoring;
- presentation preparation.

Each parallel job writes to isolated buffers.

Merge ordering is deterministic.

The project does not make core gameplay depend on Unity Job System scheduling order.

Early prototype remains mostly single-threaded so bottlenecks are measured before complexity is introduced.

---

# PART LXX — MULTIPLAYER MODEL

Canonical multiplayer is:

# DEDICATED SERVER AUTHORITATIVE.

Singleplayer runs the same authoritative server/simulation interface in-process.

Clients own:

- input;
- camera;
- selection;
- UI;
- presentation;
- permitted cached knowledge.

Server owns all gameplay truth.

The architecture is **not peer-to-peer lockstep**.

Deterministic simulation is still required because it enables:

- replay;
- authoritative verification;
- debugging;
- optional shadow simulation;
- stable testing.

---

# PART LXXI — NETWORK TRANSPORT

Unity Transport is used as the low-level packet/connection foundation rather than as the gameplay architecture. Unity documents it as the low-level networking layer for connections and data transfer. ([docs.unity3d.com](https://docs.unity3d.com/kr/current/Manual/com.unity.transport.html))

Logical channels:

## RELIABLE ORDERED

- player commands;
- acknowledgments;
- lobby/match control;
- important state transitions;
- initial/reconnect transfer.

## UNRELIABLE SEQUENCED

- regular snapshots;
- noncritical presentation telemetry.

## RELIABLE BULK

- initial map/game manifest;
- reconnect snapshot;
- post-match replay delivery where appropriate.

---

# PART LXXII — SERVER AUTHORITY

Client may request.

Server decides.

Every command validates:

- player ownership;
- entity existence;
- command eligibility;
- fog/legal knowledge where applicable;
- target legality;
- resource cost;
- Energy/Charge;
- technology;
- cooldown/state;
- build placement;
- Tube route;
- service membership.

Clients never submit:

- damage results;
- HP;
- final movement positions;
- resource totals;
- research completion;
- projectile hits.

---

# PART LXXIII — NETWORK SNAPSHOTS

Server publishes gameplay snapshots at baseline:

# 10 HZ.

Snapshots are delta-compressed against acknowledged client baseline.

They contain only information the recipient may legally know.

Common replicated state:

- visible entity transforms;
- HP/state;
- visible orders where appropriate;
- own faction economy;
- own production;
- own networks;
- permitted public map state;
- relevant projectile state;
- fog exploration updates.

The client interpolates presentation between snapshots.

Owned command feedback may be locally anticipated visually, but authoritative correction always wins.

---

# PART LXXIV — MULTIPLAYER FOG PRIVACY

Hidden enemy simulation state is never sent merely because the client UI hides it.

Server interest filtering is based on authoritative per-player visibility.

When an enemy becomes hidden:

- client receives loss-of-visibility transition;
- live updates stop.

Last-known information remains client-side.

A compromised ordinary client therefore does not possess current hidden army positions in its normal replicated state.

---

# PART LXXV — COMMAND LATENCY

No fixed artificial lockstep input delay is added.

At approximately 50 ms network round-trip time, target normal command path is:

- input feedback: same render frame;
- packet to server;
- execution at next available 20-Hz tick;
- acknowledgment/snapshot return.

Target normal visible authoritative response:

# ≤125 MS

under 50 ms RTT.

Prototype hard warning:

# >175 MS

excluding packet-loss recovery.

---

# PART LXXVI — RECONNECTION

Server retains authoritative match state.

Reconnect protocol:

1. authenticate session;
2. send protocol/content/map hashes;
3. send current legal full client snapshot at tick T;
4. send own pending production/orders as appropriate;
5. client constructs presentation;
6. resume snapshot stream.

The baseline competitive prototype does not require seamless host migration because the server is dedicated.

---

# PART LXXVII — SPECTATOR FOUNDATION

The protocol supports a spectator permission profile.

Competitive baseline:

# 120-SECOND DELAY FOR FULL-INFORMATION PUBLIC SPECTATING

when implemented.

Spectator functionality is not required for the first network milestone.

Replay observation is higher priority.

Team-limited observer modes require explicit game-mode rules later.

---

# PART LXXVIII — DESYNC / STATE VERIFICATION

Because the server is authoritative, normal retail clients do not need identical complete hidden-state simulation.

Development builds may run a deterministic shadow simulation.

Verification:

- full state hash every 20 ticks;
- command log hash;
- content hash;
- map hash.

Any divergence records:

- first divergent tick;
- subsystem hashes;
- recent commands;
- RNG state;
- relevant entity dumps.

“Something went wrong” is not an acceptable desync report.

---

# PART LXXIX — REPLAY SYSTEM

Replay is:

# INITIAL STATE + AUTHORITATIVE COMMAND LOG + DETERMINISTIC SEEDS + HASH CHECKPOINTS.

Replay header includes:

- replay format version;
- simulation version;
- content hash;
- map hash;
- match settings;
- player/faction slots;
- initial deterministic seed.

## SEEKING

Store a full deterministic seek snapshot every:

# 200 TICKS — 10 SIMULATION SECONDS.

To seek:

1. load nearest prior snapshot;
2. replay accepted commands to desired tick.

## VALIDATION

Playback checks stored state hashes.

Ranked verification requires exact compatible content hash.

---

# PART LXXX — SAVE / LOAD

Initial save support is singleplayer.

A save contains:

- SaveVersion;
- content/map hashes;
- current tick;
- entity IDs/components;
- faction resources;
- Energy Domains;
- Worksite topology;
- Tube graph;
- Charge;
- production/research;
- transport state;
- fog explored/current state;
- RNG streams;
- active timed effects;
- AI state sufficient for deterministic continuation.

Presentation state such as active particle systems is not serialized.

After load, presentation rebuilds from authoritative state.

---

# PART LXXXI — SAVE / REPLAY COMPATIBILITY

Compatibility is explicit, not guessed.

Version families:

- `SimulationVersion`;
- `ContentSchemaVersion`;
- `MapFormatVersion`;
- `SaveVersion`;
- `ReplayVersion`;
- `NetworkProtocolVersion`.

Minor save migration may exist.

Old ranked replays are never silently converted in a way that could change simulation outcome.

If a replay cannot reproduce its original simulation version/content, the game reports it as incompatible.

---

# PART LXXXII — AI FOUNDATION

AI issues the same gameplay commands available to players.

It receives a:

# PLAYER KNOWLEDGE VIEW

that respects:

- fog;
- last-known enemies;
- own economy;
- known resource information.

AI does not directly mutate simulation state.

## LAYERS

### STRATEGIC

- expansion;
- economy;
- technology;
- army plan.

### TACTICAL GROUP

- positioning;
- focus;
- retreat;
- siege;
- transport.

### SYSTEM-SPECIFIC

Faction planners.

Individual units still use the common movement/combat systems rather than one behavior tree per unit.

---

# PART LXXXIII — FACTION AI

## ROCK RAIDERS

Planner reasons about:

- Worksite expansion;
- processing routes;
- excavation opportunity;
- Industrial Advance;
- repair proximity.

## ASTRONAUTS

Planner reasons about:

- opponent composition;
- Mission Refit;
- Field/Mission production;
- Forward Service;
- combined arms.

## ALIENS

Planner reasons about:

- Crystal commitment;
- Charge forecast;
- Surge timing;
- pressure route;
- reconfiguration.

## MARTIANS

Planner reasons about:

- Station geometry;
- Tube redundancy;
- transfers;
- segmentation;
- displacement opportunities.

No faction AI receives hidden mechanical shortcuts merely because its systems are complicated.

---

# PART LXXXIV — CAMERA IMPLEMENTATION

Camera is a presentation-only Unity system implementing Phase 07.

Canonical baseline remains:

- perspective;
- 36° vertical FOV;
- 58° default downward pitch;
- 52–64° legal pitch;
- 45° default yaw;
- 90° rotation increments;
- 24-cell close;
- 44-cell normal combat;
- 72-cell strategic.

Camera position uses normal floating-point Unity transforms because it is non-authoritative.

The camera never writes gameplay transforms.

---

# PART LXXXV — INPUT IMPLEMENTATION

Input system maps hardware to high-level input actions.

Flow:

`Hardware → InputAction → InteractionContext → PlayerIntent → CommandProposal`

Selection/camera actions stop before `CommandProposal` because they are local.

Gameplay actions produce command payloads.

Input bindings are remappable.

The implementation preserves Phase 07's contextual right-click and queue semantics rather than embedding behavior in individual unit scripts.

---

# PART LXXXVI — UI ARCHITECTURE

UI uses:

# MODEL–VIEW–VIEWMODEL-LIKE READ-ONLY GAMEPLAY BINDING.

## MODEL

Published simulation/client knowledge state.

## VIEWMODEL

Client transforms state into:

- resource readouts;
- selection cards;
- production lists;
- network status;
- warnings.

## VIEW

UI Toolkit.

UI may issue commands through a `CommandGateway`.

UI code never directly calls:

`unit.hp -= ...`

or changes authoritative component state.

---

# PART LXXXVII — HUD UPDATE MODEL

The UI avoids rebuilding whole visual trees every frame.

Use:

- retained controls;
- dirty-field binding;
- pooled selection/type cards;
- event-driven alerts.

Frequent values such as HP bars may sample published state each render frame where cheap.

Complex lists refresh only when underlying data changes.

128 selected entities remain grouped according to Phase 07 rather than generating 128 elaborate portraits.

---

# PART LXXXVIII — MINIMAP

Minimap has:

- baked terrain/base texture;
- fog mask;
- GPU/instanced entity markers;
- camera viewport polygon;
- alert/ping overlay.

Entity markers originate from client-legal knowledge.

The minimap remains north-up.

Mouse minimap commands convert pixel position into canonical cell-space coordinates before creating commands.

---

# PART LXXXIX — OVERLAYS

Overlay manager renders on demand:

- build grid;
- building footprint;
- production exit;
- Worksite service;
- Forward Service;
- Resonance/Surge;
- Aero Tube network;
- Energy Domain;
- sight/detection;
- attack range where UX requires;
- pathing debug in developer builds.

Normal gameplay does not keep all topology visible simultaneously.

This preserves Phase 07's clutter limits.

---

# PART XC — ALERTS

Alerts are semantic events, not arbitrary UI messages.

Priority classes:

- critical strategic loss;
- direct attack;
- network/system disruption;
- production/research completion;
- low-priority informational.

Rate limiting prevents multiple similar damage events from flooding the player.

Faction-critical examples:

- Rock Raider Worksite disconnected;
- Forward Service lost;
- Resonance Core destroyed;
- Martian network segmented.

---

# PART XCI — RENDERING PIPELINE

Canonical rendering:

# UNIVERSAL RENDER PIPELINE — FORWARD+.

Goals:

- strong stylized lighting;
- predictable PC performance;
- Shader Graph compatibility;
- custom renderer features;
- good small-team iteration.

Rendering makes extensive use of:

- SRP Batcher;
- GPU instancing;
- static batching where useful;
- LODGroup;
- MaterialPropertyBlock/instance data;
- pooled render objects.

HDRP is rejected as unnecessary overhead for the chosen art direction and battlefield scale.

---

# PART XCII — LEGO MATERIAL SYSTEM

A small master-material family serves the project.

## MOLDED POLYMER

Phase 08 stylized roughness family.

## TOOL METAL

Drills, cutters, axles.

## RUBBER

Wheels/flexible elements.

## TRANSPARENT POLYMER

Canopies and Tubes.

## CRYSTAL

Faceted transparent/emissive material.

## TERRAIN

Biome-controlled material family.

Faction body colors are parameters/presets, not entirely separate shader programs.

---

# PART XCIII — TEAM IDENTIFICATION

Phase 08's team-identification rule remains.

Team state modifies:

- Identification Tiles;
- signal band;
- selection rings;
- minimap;
- health-bar pips.

It does not recolor entire canonical LEGO palettes.

Implementation passes team index through:

- MaterialPropertyBlock;
- instanced structured data;
- world-overlay renderer.

---

# PART XCIV — LOD

Three primary gameplay LOD families remain:

- Close;
- Combat;
- Strategic.

The **Combat LOD** is the primary production target.

LOD transitions use:

- hysteresis;
- dither/crossfade where large silhouette change would otherwise pop.

Hero silhouette components survive Strategic LOD.

LOD does not remove gameplay-significant transformation pieces if their state must remain readable at 72 cells.

---

# PART XCV — ANIMATION ARCHITECTURE

Animation is layered.

## LOCOMOTION

Presentation derives:

- wheel rotation;
- track motion;
- leg gait speed;
- hover response

from simulation velocity.

## FUNCTION

Drills, cranes, loaders, pumps and tools follow explicit simulation operating states.

## TRANSFORMATION

Animation maps to authoritative normalized transition progress.

## DAMAGE

Visual damage state maps from canonical HP thresholds.

## CHARACTERS

Minifigure geometry uses rigid LEGO-faithful articulation established in Phase 08.

No realistic elbow/knee skin deformation is introduced.

---

# PART XCVI — ANIMATION UPDATE BUDGET

Presentation may use animation significance tiers.

## TIER A — IMPORTANT

Selected / near camera / transforming / firing.

Update:

# EVERY RENDER FRAME.

## TIER B — NORMAL BATTLE

Update animator at approximately:

# 30 HZ

with transform interpolation still each frame.

## TIER C — DISTANT

Animation parameter update:

# 15 HZ

or cheaper procedural approximation.

The system never changes authoritative motion.

---

# PART XCVII — VFX

VFX are event-driven.

Simulation emits a stable `PresentationEventID` composed from:

- tick;
- event ordinal;
- source entity.

Presentation uses this to avoid duplicate effects after:

- network correction;
- replay seek;
- client state rebuild.

Effect families include:

- weapon fire;
- impact;
- drill dust;
- crystal resonance;
- Tube pressure;
- construction;
- repair;
- destruction;
- environmental hazard.

Effect intensity is visually bounded by Phase 08 readability requirements.

---

# PART XCVIII — LEGO DESTRUCTION PRESENTATION

Gameplay destruction uses fixed collision timing.

Visual destruction uses:

- pre-authored break groups;
- pooled bricks/fragments;
- small number of cosmetic rigidbodies;
- nonphysics ballistic fragments;
- dust/spark effects.

Baseline camera-local budget:

- approximately 64 active cosmetic rigidbodies;
- approximately 128 cheap nonphysics fragments.

When the budget is exceeded:

- distant destruction uses cheaper breakup;
- gameplay result does not change.

The game never simulates every LEGO brick as authoritative physics.

---

# PART XCIX — TRANSPARENCY

Transparent assets receive explicit cost discipline.

Priority cases:

- canopies;
- crystals;
- Aero Tubes;
- selected VFX.

Avoid deeply layered transparent geometry.

At distance:

- simplify internal canopy rendering;
- simplify Tube interiors;
- preserve bright transport marker rather than full optical depth.

Transparency must not make unit silhouettes disappear against bright biomes.

---

# PART C — LIGHTING

Standard battlefield lighting uses:

- one strong directional/key light;
- ambient/environment contribution;
- probes where useful;
- limited local machine lights;
- restrained emissive bloom.

Dynamic combat light count is budgeted.

A baseline camera should not require dozens of shadow-casting point lights.

Recommended normal maximum:

# APPROXIMATELY 8 HIGH-IMPACT DYNAMIC LOCAL LIGHTS

in the active view, with lower-cost emissive substitutes for additional effects.

---

# PART CI — BIOME RENDERING

Each biome owns a `BiomeVisualProfile`.

Fields include:

- terrain material set;
- atmosphere;
- fog presentation;
- lighting preset;
- sky;
- decals;
- environmental VFX;
- post-processing;
- audio profile reference.

Biome visual settings never redefine authoritative:

- Rough Ground;
- hazard;
- buildability;
- movement.

Those come from map gameplay layers.

---

# PART CII — ASSET PIPELINE

Source classification remains attached to assets:

- OFFICIAL-DIRECT;
- OFFICIAL-ADAPTED;
- COMPOSITE-ADAPTATION;
- NEW GAME CONTENT.

Every production asset record contains source-set references.

## DCC

Canonical DCC source may be authored in Blender or equivalent professional tools.

The Unity project imports:

- deterministic exported FBX/glTF-compatible production files;
- textures;
- animations.

The game build does not depend on live DCC integration.

## MODEL STRUCTURE

Export separates:

- Hero geometry;
- support geometry;
- micro detail;
- gameplay pivot/anchors;
- animation bones;
- VFX sockets.

---

# PART CIII — SOURCE-FIDELITY ASSET METADATA

Every major unit/building presentation profile records:

- Source Classification;
- exact registry source sets/motifs;
- must-preserve features;
- visual family;
- forbidden redesign notes;
- current production reference links/internal images.

This prevents technical asset iteration from slowly drifting into generic science fiction.

---

# PART CIV — SOURCE CONTROL

Primary version control:

# GIT + GIT LFS.

Unity settings:

- Visible Meta Files;
- Force Text serialization where supported.

LFS tracks:

- source meshes;
- textures;
- audio;
- large binary reference files.

Gameplay JSON, code, map metadata and validation manifests remain normal text files.

Avoid many developers editing one giant Unity scene.

Maps use additive/modular authoring where practical.

---

# PART CV — DEPENDENCY POLICY

External dependency adoption requires:

- explicit owner;
- license review;
- current maintenance status;
- performance impact;
- determinism assessment;
- build-platform assessment;
- removal/exit strategy.

Authoritative gameplay does not depend on a third-party package for:

- fixed point;
- pathfinding;
- simulation ECS;
- command serialization;
- replay.

Official Unity packages are preferred where a package genuinely saves non-differentiating work.

Minimal dependencies are a deliberate maintainability strategy.

---

# PART CVI — GAMEPLAY DATA PIPELINE

Canonical gameplay source data is:

# UTF-8 JSON

stored outside Unity scene/prefab serialization.

Reasons:

- Git diffability;
- AI-assisted editing;
- external validation;
- headless accessibility;
- modding path;
- engine independence.

Pipeline:

`JSON Source → Schema Validator → Stable-ID Resolver → Content Compiler → Immutable Runtime Tables`

Unity presentation ScriptableObjects are joined by Stable ID.

Headless server loads the same compiled gameplay binary without Unity asset databases.

---

# PART CVII — VISUAL DATA PIPELINE

`VisualProfile` is a Unity-side asset keyed by Stable ID.

Contains:

- prefab;
- LOD references;
- material profile;
- animation profile;
- VFX profile;
- selection anchor;
- projectile sockets;
- transport/load anchors;
- destruction profile.

A gameplay UnitDefinition does not directly contain a Unity GUID.

This keeps simulation independent from asset layout.

---

# PART CVIII — CONTENT BUILD

Content build produces a `ContentManifest`.

It contains:

- content schema version;
- Stable ID registry hash;
- gameplay data hash;
- visual binding completeness;
- localization-key completeness;
- unit count;
- building count;
- research count;
- map catalog.

Client and server compare gameplay content hashes before multiplayer match start.

Ranked match cannot start with mismatched gameplay data.

---

# PART CIX — MAP AUTHORING TOOLS

Create a dedicated Unity editor:

# MAP AUTHOR WINDOW.

It supports painting/editing:

- buildability;
- nav flags;
- elevation bands;
- ramps;
- LoS blockers;
- air blockers;
- Rough Ground;
- liquids;
- Excavatable Features;
- hazard regions;
- resources;
- expansions;
- starts.

Debug overlays display compiled—not merely source—data.

A designer can therefore see exactly what the simulation will use.

---

# PART CX — MAP VALIDATION

Competitive map validation checks:

- player count;
- spawn completeness;
- safe starting Ore;
- no starting normal Crystal field where prohibited;
- valid ground connection between essential strategic areas;
- 12-cell heavy-route requirement;
- legal build pads;
- resource accessibility;
- airspace;
- symmetry/equivalence metrics where relevant;
- no faction-exclusive mandatory expansion;
- Excavatable route rule;
- structure exit clearance.

Automated flood fills validate every movement profile.

A map that fails topology validation cannot enter the competitive catalog.

---

# PART CXI — EDITOR DEBUG TOOLS

Required editor/runtime developer overlays:

- Entity ID labels;
- target state;
- navigation cells;
- HPA clusters/portals;
- current path;
- reservations;
- collision radius;
- formation slots;
- vision cells;
- fog counts;
- Energy Domains;
- Worksite components;
- Forward Service;
- Resonance coverage;
- Tube graph/components;
- resource ownership;
- production queues;
- state hashes.

These are first-class development tools, not post-launch polish.

---

# PART CXII — SIMULATION INSPECTOR

A developer may select any entity and inspect:

- Stable definition ID;
- Entity ID;
- all authoritative components;
- current command;
- queued commands;
- target;
- path;
- movement profile;
- component history over recent ticks;
- Energy/network memberships;
- active timed effects.

Simulation Inspector never edits ranked-authoritative state unless development cheats are explicitly enabled.

---

# PART CXIII — TELEMETRY

Developer builds record:

- total sim tick time;
- per-system tick time;
- path requests;
- nodes expanded;
- cache hit rate;
- movement stalls;
- visibility updates;
- entity count;
- projectile count;
- draw calls/batches;
- animation time;
- VFX cost;
- UI time;
- bytes up/down;
- command latency;
- snapshot size;
- state-hash result.

Player analytics, if added, remain a separate opt-in/product-policy layer.

---

# PART CXIV — PROFILING

Performance work uses repeatable captures, not anecdotal editor observation.

Tools include:

- Unity Profiler;
- Frame Debugger;
- GPU profiler;
- Render Graph diagnostics where applicable;
- Memory Profiler;
- custom simulation markers;
- custom pathfinding telemetry.

Every benchmark scene has:

- fixed seed;
- scripted commands;
- fixed camera path where rendering is tested.

---

# PART CXV — TESTING PYRAMID

## LEVEL 1 — PURE C# UNIT TESTS

- fixed point;
- damage;
- costs;
- graph logic;
- commands;
- serialization.

## LEVEL 2 — PROPERTY TESTS

- path legality;
- resource conservation;
- no illegal overlap;
- network segmentation.

## LEVEL 3 — HEADLESS SIM TESTS

- full matches;
- faction systems;
- determinism.

## LEVEL 4 — UNITY EDIT TESTS

- data/import/editor tools.

## LEVEL 5 — UNITY PLAY TESTS

- presentation bindings;
- camera;
- UI;
- selection.

## LEVEL 6 — NETWORK TESTS

- two+ processes;
- latency/loss;
- reconnect.

## LEVEL 7 — PERFORMANCE / SOAK

- benchmark scenes;
- hours-long stability.

---

# PART CXVI — DETERMINISM TESTING

The canonical determinism test:

1. load same compiled content;
2. load same map/seed;
3. replay identical command stream;
4. run multiple independent processes;
5. compare hash every tick.

Acceptance:

# ZERO HASH DIVERGENCES.

CI runs golden replay collections.

A code change that intentionally changes gameplay requires:

- explicit golden update;
- changelog note;
- affected balance/system review.

---

# PART CXVII — PATHFINDING TESTING

Tests include:

- every footprint through minimum legal corridors;
- crossing formations;
- opposite-direction traffic;
- worker/heavy interaction;
- structure exits;
- blocked exits;
- unloading;
- dynamic building destruction;
- Excavatable Feature opening;
- narrow chokepoint;
- 60+ simultaneous movers.

Acceptance:

- no permanent reachable-goal deadlock;
- p95/p99 budget pass;
- deterministic route output;
- no path through illegal terrain.

---

# PART CXVIII — FOG / LOS TESTING

Golden tests render visibility bitmaps for:

- flat ground;
- cliffs;
- ramps;
- walls;
- ground/air;
- detectors;
- multiple vision sources;
- destruction of occluder;
- excavation opening.

Expected CPU masks are checked byte-for-byte.

GPU softness is separately tested visually and does not affect pass/fail simulation data.

---

# PART CXIX — ECONOMY TESTING

Tests assert:

- resource conservation;
- construction reservation/refund;
- OC reservation;
- Energy reserve drain;
- brownout ordering;
- Worksite connectivity;
- Astronaut refit costs;
- Crystal commitment/withdrawal;
- Resonance destruction salvage;
- Tube local/shared storage;
- production cancellation.

No economy test relies on rendered UI numbers.

---

# PART CXX — COMBAT TESTING

Golden combat tests cover:

- each damage class;
- armor;
- every projectile behavior;
- overkill;
- splash;
- drills/contact;
- transformation;
- Surge cadence;
- Martian displacement/Stability;
- transport destruction;
- repair;
- structure collapse timing;
- hazards.

Phase 06 BTVs remain the starting expected numbers.

---

# PART CXXI — NETWORK TESTING

Network harness simulates:

- latency;
- jitter;
- packet loss;
- packet duplication;
- reconnect;
- burst commands.

Primary stress scenario:

- 2v2-equivalent simulation load;
- 100 OC each;
- concurrent combat;
- transports;
- Tube transfers;
- Surge;
- excavation;
- base production.

Pass:

- server p99 simulation ≤4 ms;
- no authoritative corruption;
- reliable commands eventually execute exactly once;
- bandwidth targets met;
- hidden state never leaks through snapshot contents.

---

# PART CXXII — SAVE / REPLAY TESTING

Every release candidate runs:

- save → load → continue → compare hash;
- replay complete match → compare final hash;
- random seek to each 10-second snapshot → replay forward → compare hash;
- corrupt/truncated file rejection;
- incompatible-version rejection.

No “best effort” malformed save loading exists.

---

# PART CXXIII — CONTENT VALIDATION TESTS

CI deliberately includes broken fixtures:

- duplicate ID;
- missing visual profile;
- invalid weapon;
- research cycle;
- missing production source;
- illegal Tube unit;
- bad map start;
- impossible building exit.

Each must fail with a specific diagnostic.

A validator that always passes is itself considered broken.

---

# PART CXXIV — BUILD CONFIGURATIONS

Required build configurations:

1. Editor Development;
2. Client Debug;
3. Client Development;
4. Client Shipping;
5. Dedicated Server Development;
6. Dedicated Server Shipping;
7. Headless Simulation Release;
8. Determinism Test Runner.

Shipping removes:

- unrestricted cheats;
- developer state editing;
- debug graph overlays;
- verbose per-tick logs.

Debug symbols are archived separately.

---

# PART CXXV — CI PIPELINE

Every main-branch change runs:

1. format/static checks;
2. pure C# tests;
3. gameplay JSON validation;
4. content compilation;
5. deterministic golden replay;
6. Unity Edit Mode tests;
7. client build;
8. server build;
9. Play Mode smoke tests;
10. benchmark smoke budget;
11. artifact hash generation.

Nightly:

- long benchmark suite;
- network soak;
- repeated deterministic simulation;
- save/replay matrix.

---

# PART CXXVI — ERROR HANDLING

Development principle:

# FAIL LOUDLY WHEN DATA OR AUTHORITATIVE STATE IS INVALID.

Examples that throw/fail startup in development:

- duplicate Stable ID;
- missing required definition;
- impossible enum/state;
- broken map compiler;
- version mismatch.

Shipping builds:

- reject invalid content/match;
- show clear user-safe error;
- generate diagnostic log.

They do not continue with guessed gameplay defaults.

---

# PART CXXVII — LOGGING

Structured log categories:

- Sim;
- Commands;
- Navigation;
- Network;
- Content;
- Economy;
- Combat;
- Faction.RR;
- Faction.AST;
- Faction.ALI;
- Faction.MAR;
- SaveReplay;
- Presentation;
- UI.

Logs include:

- tick;
- Entity ID;
- player;
- Stable ID where relevant.

High-frequency logs compile out or remain disabled by default.

---

# PART CXXVIII — VERSIONING

Project uses explicit version numbers rather than Unity Editor version as game-protocol truth.

Examples:

- `SimulationVersion = 1`
- `ContentSchemaVersion = 1`
- `MapFormatVersion = 1`
- `SaveVersion = 1`
- `ReplayVersion = 1`
- `NetworkProtocolVersion = 1`.

Changes that alter deterministic interpretation require appropriate version increments.

---

# PART CXXIX — MODIFIABILITY

The architecture is mod-friendly by construction.

Future offline/skirmish mods may supply:

- gameplay definitions;
- visual bindings;
- maps;
- localization.

However:

# RANKED MULTIPLAYER REQUIRES AN APPROVED CONTENT HASH.

Baseline multiplayer does not load arbitrary C# mod assemblies.

A future mod SDK is not part of the first implementation phase.

---

# PART CXXX — LOCALIZATION

All player-facing text uses localization keys.

Stable gameplay identity never uses:

- English display names;
- object labels;
- translated text.

UI Toolkit layouts must support expansion.

Source-set names retained for internal documentation are separate from runtime localized names.

---

# PART CXXXI — ACCESSIBILITY FOUNDATION

Architecture supports:

- complete key remapping;
- UI scale;
- text scale;
- cursor scale;
- reduced camera shake;
- reduced flashing;
- bloom intensity control;
- colorblind-safe ownership/target markers;
- subtitle/caption infrastructure;
- edge-scroll toggle;
- hold/toggle variants where suitable.

No strategic information relies on hue alone, preserving Phase 07/08 canon.

---

# PART CXXXII — SETTINGS

Versioned settings categories:

- Gameplay;
- Interface;
- Video;
- Audio;
- Input;
- Accessibility.

Stored per user outside authoritative saves.

Competitive server-lockable values include anything that could change gameplay interpretation.

Pure presentation settings remain local.

---

# PART CXXXIII — SECURITY MODEL

Client is untrusted.

Server validates every gameplay command.

Basic protections:

- ownership validation;
- command rate limits;
- sequence validation;
- state legality;
- build/resource checks;
- visibility restrictions;
- packet bounds checks;
- protocol version;
- session token.

Never deserialize arbitrary executable types from network or mod data.

No client can request “deal X damage” as a legal command.

---

# PART CXXXIV — DEDICATED SERVER

Dedicated server build contains:

- simulation;
- content;
- map gameplay data;
- networking;
- AI where required;
- logging/telemetry.

It omits:

- rendered meshes;
- normal VFX;
- camera;
- player HUD.

Initial production target remains Windows x64 server for architecture simplicity.

Linux dedicated hosting may be added after cross-platform deterministic server tests pass.

The protocol is designed not to prevent that later move.

---

# PART CXXXV — HEADLESS SIMULATION RUNNER

A separate pure .NET executable loads the same compiled `Game.Sim`.

Capabilities:

- run match without Unity;
- inject scripted commands;
- dump hashes;
- measure tick time;
- test AI;
- batch balance scenarios;
- fuzz commands/maps.

Target throughput for the 1v1 benchmark:

# AT LEAST 20× REAL TIME

on a normal developer CPU.

Desired:

# 60× REAL TIME.

This tool is central to future balance development.

---

# PART CXXXVI — REPOSITORY / MODULE MAP

Canonical module structure:

`/SimCore`
- Math
- Entities
- Components
- Systems
- Commands
- Navigation
- Economy
- Combat
- Factions
- Fog
- Serialization
- Replay

`/GameData`
- StableIdRegistry
- Units
- Buildings
- Weapons
- Research
- Commands
- Movement
- Maps

`/ContentCompiler`
- Schema
- Validation
- BinaryCompiler

`/Headless`
- SimRunner
- Benchmarks
- Fuzz

`/UnityClient`
- Presentation
- Input
- Camera
- UI
- Rendering
- Animation
- VFX
- Audio
- EditorTools

`/UnityServer`
- TransportHost
- SnapshotReplication
- MatchHost

`/Tests`
- Unit
- Golden
- Network
- Performance
- Content.

No `SimCore` assembly references Unity.

---

# PART CXXXVII — DEPENDENCY DIRECTION

Allowed:

`UnityClient → Sim interfaces/data`

`UnityServer → SimCore`

`Headless → SimCore`

`ContentCompiler → shared schemas`

`SimCore → shared deterministic primitives`

Forbidden:

`SimCore → UnityEngine`

`SimCore → presentation prefab`

`SimCore → UI`

`SimCore → PhysX`

`SimCore → animation clip`.

This rule is enforced with assembly boundaries.

---

# PART CXXXVIII — MINIMUM PLAYABLE PROTOTYPE

The first playable RTS prototype uses **Rock Raiders** because their core loop exposes economy, construction, movement and industrial identity without immediately requiring Refit/Charge/Tube complexity.

Exact prototype gameplay asset subset:

## UNITS

- Rock Raider Crew;
- Loader Dozer.

## BUILDINGS

- Rock Raiders HQ;
- Ore Processing Plant;
- Power Station;
- Vehicle Service Bay.

## MAP CONTENT

- Ore deposit;
- simple buildable terrain;
- one rough/impassable obstacle;
- basic fog.

## SYSTEMS

- camera;
- selection;
- move;
- attack;
- fixed simulation;
- pathfinding;
- harvesting;
- construction;
- production;
- HP;
- damage;
- destruction;
- Energy;
- Operations Capacity;
- basic HUD.

The prototype uses final Stable IDs, schema and simulation architecture.

It is not throwaway gameplay code.

---

# PART CXXXIX — FOUR-FACTION MECHANIC PROOF

After the minimum prototype, each faction receives a narrow system proof.

## ROCK RAIDERS

- HQ + Service Bay;
- Worksite overlap;
- Drill Craft;
- one Excavatable Feature.

## ASTRONAUTS

- T3-Trike;
- Service & Refit Hub;
- Escort ↔ Survey Refit;
- deployed Solar Explorer Forward Service.

## ALIENS

- Resonance Core;
- Crystal commitment;
- Razor Skimmer;
- 50-Charge Surge.

## MARTIANS

- Hangar;
- Settlement Station;
- Tube Link;
- Jet Scooter transfer;
- link destruction/segmentation.

No faction is considered architecturally proven until its defining system works under the same simulation/network model.

---

# PART CXL — VISUAL VERTICAL SLICE

The visual vertical slice is intentionally smaller than the roster.

Recommended hero set:

## ROCK RAIDERS

- Chrome Crusher;
- Rock Raiders HQ.

## ASTRONAUTS

- T3-Trike;
- MX-41 Switch Fighter;
- Service & Refit Hub.

## ALIENS

- ETX Alien Infiltrator;
- Resonance Core.

## MARTIANS

- Jet Scooter;
- Red Planet Protector;
- Settlement Station;
- Aero Tube Link.

## GLOBAL

- Rock Raider Crew/minifigure quality standard;
- Ore;
- Crystals;
- one Rock Monster;
- one Basalt Highlands combat region;
- complete fog/selection/HUD presentation.

This set exercises:

- wheels;
- hover;
- walkers;
- transformation;
- crystal effects;
- Tube transparency;
- LEGO destruction;
- four faction palettes.

---

# PART CXLI — ASSET EFFORT BUDGET

Use internal **Asset Complexity Points**, not calendar promises.

Baseline:

- simple worker/small machine: 1 AP;
- medium vehicle: 2 AP;
- heavy/transforming vehicle: 3 AP;
- Huge/flagship: 5 AP;
- normal building: 2 AP;
- major/network building: 3 AP;
- transformation complexity: +1 AP;
- bespoke destruction/rig: +1 AP.

Vertical Slice target:

# ≤30 AP

before environment/UI shared assets.

This prevents the visual milestone from accidentally becoming full content production.

---

# PART CXLII — PLACEHOLDER POLICY

Greybox assets must already use:

- final Entity ID semantics;
- final footprint;
- final pivot conventions;
- final movement profile;
- final gameplay data;
- final selection anchors.

Placeholder art may be primitive.

Placeholder gameplay architecture may not be a second temporary system.

A capsule standing in for Chrome Crusher still uses:

`unit.rr.chrome_crusher`

rather than `PrototypeTank01`.

---

# PART CXLIII — ARCHITECTURE SPIKES

Five mandatory empirical spikes remain.

## SPIKE A — 20 HZ SIMULATION

Baseline:

20 Hz.

Pass:

- command feel acceptable;
- interpolated movement smooth;
- deterministic combat timing acceptable;
- normal sim ≤2.5 ms p95.

Fail alternative:

30 Hz.

## SPIKE B — NAV CLUSTER SIZE

Baseline:

10 cells.

Compare:

8 / 10 / 16.

Choose lowest total stress cost meeting route-quality targets.

## SPIKE C — CPU FOG

Baseline:

160 × 160 CPU visibility.

Pass:

≤0.75 ms p95 dirty-update contribution in large battle.

Fail:

more aggressive source dirtying/partitioning; gameplay remains CPU-authoritative.

## SPIKE D — GAMEOBJECT PRESENTATION

Baseline:

pooled GameObject views.

Pass:

45-unit battle stays within animation/render CPU budgets.

Fail:

move projectile/effect/repeated-simple presentation first to more GPU-instanced representation.

Do **not** rewrite simulation as DOTS solely because presentation fails.

## SPIKE E — SNAPSHOT RATE

Baseline:

10 Hz.

If correction quality is inadequate while bandwidth has headroom:

15 Hz.

If bandwidth fails first:

improve delta/quantization before lowering gameplay tick rate.

---

# PART CXLIV — PATHFINDING STRESS BENCHMARK

Scene contains:

- 60+ moving ground units;
- all footprint classes;
- multiple simultaneous formation orders;
- 10–15-cell medium passage;
- ≥12-cell heavy route;
- buildings;
- workers crossing heavies;
- opposite-direction groups;
- active structure exit;
- Excavatable Feature opening during movement.

Pass:

- pathfinding ≤3 ms p99;
- no reachable unit hard-stuck >5 s;
- <1% agents oscillate without route progress for >2 s;
- legal route updates after excavation without full-map rebuild;
- deterministic final routes under identical command stream.

---

# PART CXLV — NETWORK STRESS BENCHMARK

Simulated four-player worst-case load:

- 100 OC per player;
- several bases;
- large battle;
- aircraft;
- projectiles;
- Tube transfers;
- Surge;
- construction;
- destruction;
- excavation.

Network conditions include:

- 50–100 ms latency;
- 1% packet loss;
- ±30 ms jitter.

Pass:

- server sim ≤4 ms p99;
- reliable commands exactly once;
- no illegal hidden-state data;
- average downstream ≤64 KB/s/player;
- peak ≤128 KB/s;
- upstream ≤16 KB/s average;
- no authoritative-state corruption.

---

# PART CXLVI — PERFORMANCE BENCHMARK SUITE

Canonical benchmark scenes:

1. `Economy_TwoBases`
2. `Battle_20`
3. `Battle_45`
4. `AirGround_Mixed`
5. `RR_IndustrialFull`
6. `AST_ServiceRefit`
7. `ALI_SurgeWindow`
8. `MAR_NetworkStress`
9. `FullBaseAssault`
10. `Pathing_ChokepointStress`.

Each has:

- fixed seed;
- fixed content hash;
- scripted commands;
- expected entity count;
- expected result hash.

---

# PART CXLVII — ENGINE UPGRADE POLICY

The Unity project is pinned to an exact 6.3 LTS patch.

Unity currently supports 6.3 LTS through December 2027, despite newer Supported Unity 6 releases existing. ([unity.com](https://unity.com/releases/unity-6/support))

Upgrade procedure:

1. create upgrade branch;
2. update Editor and official packages;
3. regenerate Library/build cache;
4. run content compile;
5. run all deterministic golden tests;
6. run save/replay compatibility;
7. run benchmark suite;
8. compare rendering;
9. merge only when no unacceptable regression exists.

“New feature available” is not enough reason to upgrade.

---

# PART CXLVIII — PROJECT MILESTONES

No calendar dates are assigned.

## M0 — ENGINE & ARCHITECTURE SPIKE

Exit:

- Unity project bootstrapped;
- SimCore independent;
- fixed point works;
- headless runner works;
- client can render one sim entity.

## M1 — DETERMINISTIC HEADLESS SIM

Exit:

- fixed 20-Hz runner;
- components;
- commands;
- state hash;
- save snapshot;
- deterministic repeatability.

## M2 — FIRST CONTROLLABLE RTS MAP

Exit:

- camera;
- selection;
- Move;
- pathfinding;
- collision;
- fog;
- 60 movers.

## M3 — ECONOMY & BASE BUILDING

Exit:

- worker;
- Ore;
- Energy;
- OC;
- construction;
- production;
- first expansion loop.

## M4 — COMBAT

Exit:

- targets;
- weapons;
- projectiles;
- damage;
- armor;
- destruction;
- repair;
- transport baseline.

## M5 — FOUR-FACTION SYSTEM PROOF

Exit:

- Worksite;
- Refit;
- Charge/Surge;
- Aero Tube/displacement foundations.

## M6 — NETWORKED 1v1

Exit:

- dedicated server;
- two clients;
- fog privacy;
- commands;
- snapshots;
- reconnect;
- replay.

## M7 — VISUAL VERTICAL SLICE

Exit:

- representative four-faction assets;
- final-quality material prototype;
- combat VFX;
- LEGO destruction;
- final-direction UI.

## M8 — COMPLETE ROSTER DATA PASS

Exit:

- all 35 units represented;
- all 31 infrastructure definitions represented;
- all weapons/research/commands validate.

## M9 — SKIRMISH ALPHA

Exit:

- full core RTS loop;
- four playable factions;
- AI opponent;
- competitive map;
- save/replay;
- stable large battle;
- performance targets substantially met.

---

# PART CXLIX — MILESTONE GATING RULE

A milestone does not close because its feature can be shown once.

It closes when:

- implementation exists;
- validation exists;
- tests exist;
- performance telemetry exists where relevant;
- known architecture debt is documented.

No later milestone may silently depend on “we will rewrite the whole system later.”

---

# PART CL — COMPLETE UNIT IMPLEMENTATION MATRIX

The following matrix establishes an implementation path for all 35 buildable units. Roster identity remains Phase 03 canon.

| Unit | Move / Footprint | Core combat | Special systems | Presentation requirement | Critical implementation test |
|---|---|---|---|---|---|
| Rock Raider Crew | Foot / Tiny | Weak tool | Harvest/build/repair | Minifigure rigid animation | worker economy |
| Hover Scout | Hover / Small | Minimal | Detection/geology | hover + scanner | excavation reveal |
| Drill Craft | Ground / Small | Drill utility | Excavate | drill/contact | route opening |
| Rapid Rider | Skimmer / Small | Essentially none | 4 Personnel transport | twin hull | load/unload |
| Loader Dozer | Wheeled / Medium | Scoop/Cutter | rubble utility | scoop/saw | contact/cutter upgrade |
| Granite Grinder | Walker / Medium | Breach drill | sustained drill ramp | leg/drill brace | approach/ramp |
| Chrome Crusher | Wheeled / Large | Heavy breach | siege contact | huge chrome drill | heavy reservation |
| Tunnel Transport | Air / Huge | none | heavy transport | twin prop/cargo | Chrome carry |
| Expedition Crew | Foot / Tiny | weak tool | build/repair/refit labor | two human lineages | service jobs |
| Rover | Wheeled / Small | light | scout | rugged Field | survey |
| T3-Trike | Rough wheel / Medium | Escort/Survey | Mission Refit | three-wheel silhouette | config swap |
| Mono Jet | Air / Small | light harassment | scout | small flyer | air layer |
| Solar Explorer | Ground / Large | defensive pulse | deploy service/transport | solar modules | Forward Service |
| Mission Fighter | Air / Small | AA specialist | — | Mission fighter | air targeting |
| MX-41 Switch Fighter | Ground/Air / Medium | mode weapons | transform | physical transformation | identity retention |
| Mobile Mining Platform | Tracked / Large | utility | Ore/Crystal Refit | mining modules | resource config |
| MX-71 Recon Dropship | Air / Large | defensive | transport | dropship unload | exit reservation |
| MT-51 Claw-Tank | Tracked / Large | anti-light/medium | — | rotating claw machinery | moving fire |
| MT-101 Armored Drill | Wheeled / Large | anti-heavy drill | — | massive ground drill | contact slots |
| MT-201 Ultra-Drill | Walker / Huge | siege | deploy | walker/anchor | deploy footprint |
| MX-81 Operations Aircraft | Air / Huge | support pulse | scan/support | modular huge air | large-air performance |
| ETX Servitor | Hover / Tiny | none | worker | folding clamps | Alien construction |
| Alien Jet | Air / Small | AA/strafe | Surge | folding craft | Surge cadence |
| Razor Skimmer | Hover / Small | anti-light | Surge | low curved hull | harassment |
| ETX Alien Strike | Air/deployed / Medium | siege | reconfigure/Surge | flight→cradle | minimum range |
| ETX Alien Infiltrator | Hover/Walker / Medium | anti-heavy | transform/detect/Surge | craft→walker | state components |
| Alien Mothership | Air / Huge | modest support | carrier/relay/Surge | multi-part flagship | carrier + relay |
| Worker Robot | Walker / Tiny | weak tool | worker/Tube | open walker | Tube eligibility |
| Double Hover | Hover / Small | none/light | scout/Tube | twin hover pads | Tube transfer |
| Jet Scooter | Hover / Small | anti-light | Tube | tiny fast craft | raid transit |
| Aero Skiff | Air / Small | none | light transport | eccentric flyer | air unload |
| Red Planet Cruiser | Hover / Medium | general | — | source Martian hull | hover frontline |
| Recon-Mech RP | Walker / Medium | AA | detect | tall open walker | detector/AA |
| Red Planet Protector | Walker / Large | anti-heavy/control | stance/Sweep | physical stance change | Stability interaction |
| Excavation Searcher | Walker / Huge | siege/control | Brace/Clamp | multi-leg excavator | deterministic Clamp |

---

# PART CLI — COMPLETE BUILDING IMPLEMENTATION MATRIX

Phase 06's structure footprints provide the initial collision/build masks.

| Infrastructure | Footprint | Core systems | Network/economy | Production/tech | Defense | Critical test |
|---|---:|---|---|---|---|---|
| Rock Raiders HQ | 8×8 | Command/building | Worksite/Energy/OC | Crew | — | Worksite start |
| Ore Processing Plant | 6×6 | Building | Ore processing | — | — | hauling endpoint |
| Power Station | 5×5 | Building | +Energy | — | — | brownout |
| Vehicle Service Bay | 8×6 | Service | Worksite/OC | light production/repair | — | service radius |
| Engineering Workshop | 8×8 | Production | Energy/OC | heavy production/research | — | huge exits |
| Crystal Vault | 5×5 | Storage | crystal | advanced tech | — | crystal access |
| Crusher Barrier | 3×1 | Blocker | — | — | ground | path occupancy |
| Cutter Mast | 2×2 | Defense | Energy | — | AA | air targeting |
| Eagle Command Base | 8×8 | Command | Energy/OC | Crew | — | expansion |
| Field Systems Garage | 7×6 | Production | OC | Field | — | branch production |
| Mission Vehicle Bay | 8×7 | Production | OC/Energy | Mission | — | heavy exits |
| Flight Operations Pad | 10×8 | Production | OC/Energy | Air | — | air spawn |
| Service & Refit Hub | 7×7 | Service | Forward Service | Refit/research | — | Refit |
| Solar Energy Array | 6×5 | Generator | Energy | — | — | domain |
| Frontier Extraction Station | 6×6 | Economy | resources | — | — | resource intake |
| Modular Sentinel | 2×2 | Defense | Energy | refit mode | ground/air | module swap |
| ETX Command Core | 7×7 | Command | domain/OC | Servitor | — | foothold |
| Resonance Core | 4×4 | Macro | Crystals/Charge | research hooks | — | commitment/salvage |
| ETX Fabricator | 6×6 | Production | Energy/OC | light units | — | rapid fabrication |
| Reconfiguration Dock | 8×7 | Production/service | Energy/OC | advanced ETX | — | transforms |
| Power Coupler | 4×4 | Generator | Energy | — | — | Alien domain |
| ETX Defense Node | 2×2 | Defense | Energy | configuration | ground/air | mode |
| Aero Tube Hangar | 9×9 | Command/Station | network/Energy/OC | worker/air | — | graph root |
| Settlement Station | 7×7 | Station | local store/OC | worker | — | segmentation |
| Mechanical Workshop | 7×6 | Production | Energy/OC | ground units | — | network base |
| Pressure Generator | 4×4 | Generator | Energy | — | — | local/domain |
| Routing Laboratory | 6×6 | Tech | network/OC | research | — | graph upgrades |
| Excavation Plant | 6×6 | Economy | processing | — | — | local reserve |
| Deflector Arm | 3×3 | Defense | Energy | — | ground control | displacement |
| Aero Guard Tower | 2×2 | Defense | Energy | — | AA | air |
| Aero Tube Link | 1-cell route | Graph edge | Tube/Energy | — | destructible | segmentation |

---

# PART CLII — FACTION SYSTEM IMPLEMENTATION MATRIX

| System | Authoritative representation | Trigger/update | Network concern | Primary failure test |
|---|---|---|---|---|
| Worksite | overlap graph | node/build event | own state | split/reconnect |
| Excavation | feature state + nav mutation | command/completion | public visible feature | route rebuild |
| Automated Hauling | logistics jobs | extraction demand | own economy | endpoint loss |
| Forward Service | spatial provider set | movement/deploy | own state | provider destroyed |
| Mission Refit | service job | explicit command | own + visible config | identity |
| Crystal Commitment | Core slots | command | own/public visual state | destruction |
| Charge | derived integer | tick integration | own value | conservation |
| Surge | timed area state | command/tick | visible effect | cadence |
| Tube Network | graph/components | edge/node event | owner network | segmentation |
| Tube Transfer | transit job | command/tick | visible legal effects | destroyed route |
| Displacement | deterministic resolver | combat action | normal combat | illegal destination |
| Stability | timed component | displacement | visible status | chain prevention |
| Brownout | Energy-domain state | energy event | owner + visible building state | tie order |

---

# PART CLIII — CORE RUNTIME SYSTEM MATRIX

| System | Authority | Cadence | Depends on |
|---|---|---:|---|
| CommandSystem | Server | 20 Hz | network/input |
| EntityLifecycle | Server | 20 Hz | commands/combat |
| Construction | Server | 20 Hz | economy |
| Economy | Server | 20 Hz integration | resources |
| EnergyDomain | Server | event + tick | topology |
| OC | Server | event | production/buildings |
| Production | Server | 20 Hz | economy/OC |
| Research | Server | 20 Hz | economy |
| PathScheduler | Server | 20 Hz | navigation |
| Movement | Server | 20 Hz | paths/reservations |
| Formation | Server | command/event | movement |
| Fog/LoS | Server | dirty/event | movement/map |
| Targeting | Server | 20 Hz | visibility/spatial |
| Weapons | Server | 20 Hz | targeting |
| Projectiles | Server | 20 Hz | weapons |
| Damage | Server | 20 Hz | impacts |
| Repair | Server | 20 Hz | economy |
| Transport | Server | 20 Hz | movement |
| Transform | Server | 20 Hz | commands |
| Worksite | Server | event | structures |
| Refit | Server | 20 Hz | service |
| Resonance | Server | 20 Hz | crystals/energy |
| TubeGraph | Server | event | structures/links |
| TubeTransit | Server | 20 Hz | graph |
| Displacement | Server | event | combat/navigation |
| Hazards | Server | 20 Hz | map |
| AI | Server | 2–4 Hz | knowledge view |
| Snapshot | Server | 10 Hz | all replicated state |

---

# PART CLIV — UNIT DATA SCHEMA

Canonical minimum pseudo-schema:

`UnitDefinition`
- Identity
- Source metadata
- Faction
- Tech/production
- Cost/time/OC
- Health/armor/class
- Footprint
- Movement
- Vision
- Weapons
- Commands
- Economy capabilities
- Repair/service
- Transport
- Transformation
- Refit
- Faction hooks
- Presentation IDs.

A unit may have configuration overlays.

Configuration overlays replace only explicitly listed fields.

They do not clone the whole UnitDefinition.

---

# PART CLV — BUILDING DATA SCHEMA

`BuildingDefinition`
- Identity/source
- Faction
- Footprint/rotation
- build rules
- cost/time
- HP/armor
- Energy
- OC
- production
- research
- service
- faction-network role
- weapons
- construction profile
- destruction profile
- presentation profile
- UI.

Footprints are explicit masks so future nonrectangular buildings remain possible without changing construction architecture.

---

# PART CLVI — WEAPON DATA SCHEMA

`WeaponDefinition`
- Stable ID
- valid target layers/classes
- damage
- damage type
- cooldown ticks
- range/minimum range
- projectile profile?
- beam/contact profile?
- splash profile?
- facing tolerance
- movement-fire rules
- target priority profile
- VFX event profile
- audio event profile.

No weapon script receives permission to write arbitrary HP directly.

Damage always enters the common resolver.

---

# PART CLVII — RESEARCH DATA SCHEMA

`ResearchDefinition`
- Stable ID
- faction
- prerequisites
- source building
- cost
- research ticks
- mutually exclusive rules if ever used
- unlock tags
- parameter modifiers
- presentation profile
- localization.

Compiler validates DAG and effect references.

---

# PART CLVIII — COMMAND DATA SCHEMA

`CommandDefinition`
- Stable ID
- command enum/network code
- eligible entity tags
- target type
- queueability
- required technology
- validation handler ID
- execution handler ID
- UI slot/profile
- targeting preview profile.

Network command code is stable and versioned independently of localization.

---

# PART CLIX — MOVEMENT PROFILE SCHEMA

`MovementProfile`
- Stable ID
- movement layer
- traversable terrain flags
- movement-cost table
- clearance class
- speed
- acceleration
- turn
- reverse ratio
- rough-ground behavior
- shallow/deep liquid behavior
- air-blocker behavior
- local-avoidance profile.

Hover and true air therefore cannot be accidentally conflated.

---

# PART CLX — TRANSFORMATION DATA SCHEMA

`TransformationDefinition`
- Stable ID
- source state
- destination state
- duration ticks
- cancellation threshold
- movement rule during transition
- attack rule
- resulting footprint
- resulting movement profile
- resulting weapons
- visual animation state
- VFX events.

Surge may apply a rational duration modifier to eligible ETX transformations.

---

# PART CLXI — RESOURCE NODE / HAZARD SCHEMAS

`ResourceNodeDefinition`
- type;
- capacity;
- harvest interaction;
- model state thresholds;
- depletion profile.

`HazardDefinition`
- trigger/phase;
- region;
- telegraph ticks;
- active ticks;
- cooldown;
- damage;
- movement/vision effects;
- target layers;
- visual profile.

Neither uses Unity collider callbacks as gameplay authority.

---

# PART CLXII — VISUAL PROFILE SCHEMA

`VisualProfile`
- Stable ID;
- prefab;
- LOD group;
- faction material preset;
- selection anchor;
- collision-debug anchor;
- weapon sockets;
- cargo anchors;
- animation profile;
- VFX profile;
- damage visual profile;
- destruction profile;
- shadow/air altitude profile;
- source-fidelity metadata.

A missing Visual Profile is a development validation failure for a shipping buildable entity.

---

# PART CLXIII — UI PROFILE SCHEMA

`UIProfile`
- Stable ID;
- icon;
- portrait/render preview;
- localized description key;
- role tags;
- command layout;
- tooltip data template;
- warning rules;
- selection-card rules;
- faction flavor profile.

The UI receives gameplay values from simulation/view model rather than duplicating them inside UI assets.

---

# PART CLXIV — IMPLEMENTATION BACKLOG

The first architecture backlog is ordered by dependencies rather than calendar estimate.

| ID | Milestone | Task | Depends on | Acceptance |
|---|---|---|---|---|
| T001 | M0 | Create SimCore assembly | — | no UnityEngine ref |
| T002 | M0 | Implement Fix32 | T001 | arithmetic tests |
| T003 | M0 | Implement deterministic Angle16 | T002 | turn tests |
| T004 | M0 | Stable ID registry | T001 | duplicate rejection |
| T005 | M0 | Gameplay JSON schema skeleton | T004 | sample compiles |
| T006 | M0 | Content compiler CLI | T005 | deterministic output |
| T007 | M0 | Headless runner | T001 | 20 Hz loop |
| T008 | M0 | Unity Sim bridge | T001 | renders entity |
| T009 | M1 | Entity store | T002 | create/destroy |
| T010 | M1 | Component stores | T009 | deterministic iteration |
| T011 | M1 | Tick scheduler | T009 | fixed order |
| T012 | M1 | Command envelope | T011 | deterministic execution |
| T013 | M1 | Snapshot serialization | T009 | roundtrip |
| T014 | M1 | State hasher | T009 | repeatable |
| T015 | M1 | Replay command log | T012 | deterministic replay |
| T016 | M1 | Golden replay CI | T014 | identical hashes |
| T017 | M2 | Map grid compiler | T006 | 160×160 map |
| T018 | M2 | Nav grid | T017 | terrain flags |
| T019 | M2 | HPA cluster compiler | T018 | portal graph |
| T020 | M2 | Local A* | T018 | legal paths |
| T021 | M2 | Movement system | T020 | speed/turn |
| T022 | M2 | Spatial buckets | T021 | deterministic queries |
| T023 | M2 | Local avoidance | T022 | crossing test |
| T024 | M2 | Reservation system | T023 | heavy priority |
| T025 | M2 | Formation manager | T024 | role formation |
| T026 | M2 | Camera | T008 | Phase07 values |
| T027 | M2 | Selection | T026 | 128 IDs |
| T028 | M2 | Fog grid | T017 | visible/explored |
| T029 | M2 | LoS | T028 | golden masks |
| T030 | M3 | Resource nodes | T009 | depletion |
| T031 | M3 | Worker harvesting | T030 | Ore loop |
| T032 | M3 | Resource banking | T031 | conservation |
| T033 | M3 | Construction placement | T017 | server validation |
| T034 | M3 | Construction jobs | T033 | build completion |
| T035 | M3 | Production queues | T034 | unit spawn |
| T036 | M3 | Operations Capacity | T035 | reserve/cap |
| T037 | M3 | Energy Domains | T034 | generation/demand |
| T038 | M3 | Brownout | T037 | priority test |
| T039 | M3 | Basic HUD | T032 | economy display |
| T040 | M4 | Targeting | T022 | priority |
| T041 | M4 | Weapons | T040 | cooldown |
| T042 | M4 | Projectiles | T041 | travel/overkill |
| T043 | M4 | Damage/armor | T042 | Phase06 formula |
| T044 | M4 | Contact weapons | T024 | drill slots |
| T045 | M4 | Destruction | T043 | collision timers |
| T046 | M4 | Repair | T043 | resource cost |
| T047 | M4 | Transport | T024 | load/unload/death |
| T048 | M4 | Transformation | T021 | ID persistence |
| T049 | M5 | Worksite graph | T037 | split/connect |
| T050 | M5 | Excavation topology | T019 | local rebuild |
| T051 | M5 | Forward Service | T022 | membership |
| T052 | M5 | Mission Refit | T051 | T3 swap |
| T053 | M5 | Resonance Core | T037 | commitment |
| T054 | M5 | Charge/Surge | T053 | canonical timing |
| T055 | M5 | Tube graph | T037 | connectivity |
| T056 | M5 | Tube transit | T055 | Jet Scooter route |
| T057 | M5 | Displacement/Stability | T024 | clamp legality |
| T058 | M6 | Unity Transport host | T012 | two connections |
| T059 | M6 | Server command validation | T058 | authority |
| T060 | M6 | Snapshot replication | T058 | 10 Hz deltas |
| T061 | M6 | Fog filtering | T060 | no hidden data |
| T062 | M6 | Reconnect | T060 | state restore |
| T063 | M6 | Network replay | T015 | server log playback |
| T064 | M7 | URP material masters | — | Phase08 target |
| T065 | M7 | Animation presentation drivers | T048 | sim-driven |
| T066 | M7 | VFX pooling | T042 | no gameplay effects |
| T067 | M7 | LEGO destruction visuals | T045 | budgeted debris |
| T068 | M7 | UI Toolkit full HUD framework | T039 | Phase07 layout |
| T069 | M7 | Minimap | T028 | fog-correct |
| T070 | M8 | Import all 35 unit definitions | T006 | count=35 |
| T071 | M8 | Import all 31 infrastructure definitions | T006 | count=31 |
| T072 | M8 | Research DAG | T006 | all tech valid |
| T073 | M8 | Complete command catalog | T012 | all legal actions |
| T074 | M8 | Complete roster validation suite | T070 | no unresolved refs |
| T075 | M9 | Basic AI | core systems | complete match |
| T076 | M9 | Faction AI modules | T075 | four factions |
| T077 | M9 | Save/load | T013 | hash continuation |
| T078 | M9 | Performance suite | all | budget report |
| T079 | M9 | Network soak | M6 | stable long match |
| T080 | M9 | Skirmish alpha gate | all | exit criteria pass |

---

# PART CLXV — FIRST CODE TO WRITE

The first ten implementation artifacts, in order, are:

1. `Fix32` and deterministic arithmetic tests.
2. `Angle16` and lookup-table tests.
3. Stable ID registry and content-ID resolver.
4. `EntityStore` + component-store skeleton.
5. fixed `SimulationRunner` and ordered system scheduler.
6. `CommandEnvelope` + command buffer.
7. packed `MapGrid`.
8. deterministic A*/HPA navigation prototype.
9. movement/reservation system.
10. `StateHasher` + snapshot serializer + HeadlessSim CLI.

The first rendered unit should appear only after the simulation can already:

- tick;
- create an entity;
- move it deterministically;
- serialize it;
- hash it.

Rendering is not the foundation.

---

# PART CLXVI — WHAT NOT TO BUILD YET

Do **not** front-load:

- full campaign scripting suite;
- cinematic tools;
- matchmaking/account backend;
- store/monetization systems;
- console/controller UI;
- all 35 final art assets;
- all 31 final building art assets;
- procedural world generation;
- arbitrary destructible terrain;
- per-brick physical destruction;
- advanced spectator frontend;
- replay video editor;
- machine-learning AI;
- mod SDK;
- workshop distribution;
- cross-platform multiplayer;
- massive DOTS/ECS migration;
- custom rendering pipeline replacing URP;
- elaborate audio middleware architecture before core gameplay;
- “future-proof” support for thousands of combat units.

These do not reduce the major Phase 09 risks.

---

# PART CLXVII — TECHNICAL RISK REGISTER

| Risk | Impact | Mitigation | Fallback |
|---|---|---|---|
| Determinism drift | Critical | fixed point, ordered systems, golden hashes | isolate offending system before feature growth |
| Path deadlocks | Critical | reservation + HPA stress suite | simplify local steering, increase reservation authority |
| Network hidden-info leak | Critical | server visibility filtering | reduce replicated state |
| Engine upgrade regression | High | pinned LTS + upgrade gate | remain on known patch |
| Tube topology complexity | High | explicit graph + small-scale BFS | rebuild whole Martian graph on topology event; still cheap |
| Worksite/energy topology bugs | High | shared graph utilities/tests | targeted component recompute |
| Presentation CPU load | High | pooling/LOD/significance | GPU-instance repeat views |
| VFX/transparent overdraw | Medium–High | budgets, effect tiers | simplified far effects |
| Content-reference drift | High | Stable IDs/compiler | fail build |
| Asset scope explosion | High | AP budgeting/placeholders | delay non-slice final art |
| AI faction complexity | Medium–High | common planner/command API | scripted strategy profiles |
| Replay compatibility | High | explicit versions/hashes | version-lock old replay |
| Save corruption | Medium | atomic writes + validation | backup previous save |
| Third-party dependency breakage | Medium | minimal dependency policy | remove/replace package |
| Unity presentation coupling into sim | Critical | assembly boundary/CI | build fails on forbidden refs |

---

# PART CLXVIII — ARCHITECTURE EXIT CRITERIA

Phase 09 architecture is considered successfully proven when:

## DETERMINISM

- 100 repeated golden simulations produce identical hashes.

## HEADLESS

- benchmark runs at ≥20× realtime.

## PATHING

- mixed 60-unit stress passes deadlock and timing gates.

## GAMEPLAY

- minimum Rock Raider RTS loop is complete.

## FACTION PROOF

- all four defining macro/mechanical systems function.

## NETWORK

- dedicated-server 1v1 completes without authoritative corruption.

## FOG SECURITY

- hidden enemy state absent from client snapshot inspection.

## REPLAY

- network match replays to identical final hash.

## PERFORMANCE

- 45-unit battle meets 60-FPS target on reference hardware class;
- server simulation remains within 4 ms p99 stress.

## CONTENT

- compiler successfully validates final schema shape;
- all prototype data uses stable IDs.

Architecture work may continue after these gates, but fundamental simulation model is no longer speculative.

---

# PART CLXIX — CANON ESTABLISHED BY PHASE 09

Phase 09 establishes the following authoritative technical canon.

1. Primary engine is **Unity 6.3 LTS**, pinned to an exact patch.
2. Primary language is **C#**.
3. Authoritative gameplay lives in an engine-independent `SimCore`.
4. Unity owns presentation, input, editor tooling and asset integration.
5. Authoritative gameplay runs at **20 Hz fixed timestep**.
6. Rendering is interpolated and variable-rate.
7. Gameplay numerical spatial state uses deterministic fixed-point arithmetic.
8. Unity physics is not authoritative.
9. Entity IDs are stable, monotonic and survive transformations/refits/transit.
10. Gameplay is data-driven through validated Stable-ID schemas.
11. Authoritative navigation is custom hierarchical grid navigation.
12. Standard navigation resolution is **0.5 build cell / 1 meter**.
13. Local movement uses deterministic steering plus reservations.
14. Role-aware formation logic is implemented above individual pathing.
15. True air has a distinct abstract navigation layer.
16. Ground hover remains ground-layer navigation.
17. Excavatable Terrain performs local navigation-topology updates.
18. Fog is CPU-authoritative and GPU-presented.
19. LoS uses deterministic grid occlusion, not mass physics raycasts.
20. Gameplay entity architecture is custom compositional/data-oriented, not a pure engine ECS.
21. Authoritative projectiles are simulation records, not GameObjects.
22. Animation and VFX never decide gameplay results.
23. Worksite, Refit, Charge and Tube systems share reusable infrastructure where sensible but retain faction-specific behavior.
24. Energy Domain topology is event-driven.
25. Brownout ordering is deterministic.
26. Networking is dedicated-server authoritative.
27. Unity Transport supplies low-level packet transport.
28. Multiplayer uses commands plus filtered snapshots rather than peer lockstep.
29. Server never sends current hidden enemy state to ordinary clients.
30. Snapshot baseline is 10 Hz.
31. Replay uses deterministic command logs plus seek snapshots and hash checkpoints.
32. Singleplayer uses the same authoritative simulation interface.
33. A pure .NET headless runner is mandatory.
34. Gameplay source definitions use JSON and compile to immutable runtime data.
35. Unity presentation assets bind through Stable IDs.
36. Git + LFS is the canonical source-control approach.
37. URP Forward+ is the canonical renderer.
38. UI Toolkit is the canonical screen-UI framework.
39. LEGO destruction uses bounded cosmetic debris rather than per-brick authoritative physics.
40. No full DOTS/ECS rewrite is planned for the core simulation.
41. Engine upgrades require deterministic/replay/performance certification.
42. 60 FPS at 1080p is the primary client target.
43. Authoritative sim budget is ≤2.5 ms p95 normal / ≤4 ms p99 stress.
44. Pathfinding stress budget is ≤3 ms p99.
45. A minimum Rock Raider playable prototype precedes broad content production.
46. All four faction mechanics must receive explicit architecture proofs before full roster implementation.
47. The first visual vertical slice deliberately uses a limited cross-faction asset set.
48. All normal implementation work proceeds against stable IDs and final-format architecture rather than disposable prototype classes.
49. Every future core system must remain replayable, serializable, inspectable and testable.
50. Fundamental architecture is optimized for the actual canonical 100-OC medium-scale RTS rather than hypothetical massive battles.

---

# PART CLXX — FINAL IMPLEMENTATION READINESS

Phase 09 establishes a concrete implementation path for:

# ALL 35 BUILDABLE UNITS

and:

# ALL 31 BUILDING / INFRASTRUCTURE ENTRIES.

It also explicitly supports:

- Martian Aero Tube Links;
- Rock Monsters;
- neutral map entities;
- resources;
- hazards;
- Excavatable Features;
- aircraft;
- transports;
- projectiles;
- construction sites;
- production;
- research;
- Worksite logistics;
- Mission Refit;
- Forward Service;
- Crystal commitment;
- Charge;
- Surge;
- Tube transit;
- displacement;
- Stability;
- Energy Domains;
- brownout;
- Operations Capacity;
- fog;
- replay;
- saves;
- AI;
- dedicated multiplayer.

The technical architecture does **not** require reduction of:

- army size;
- map size;
- faction asymmetry;
- projectile behavior;
- pathing;
- transport behavior;
- faction mechanics;
- visual ambition.

The only explicitly benchmark-dependent implementation parameters are:

- 20 Hz versus 30 Hz simulation if 20 Hz fails the defined responsiveness test;
- HPA cluster size;
- fog optimization strategy if the CPU baseline misses budget;
- snapshot frequency if 10 Hz produces unacceptable correction/bandwidth behavior;
- presentation representation if pooled GameObjects miss the rendering CPU target.

Every one has:

- a concrete baseline;
- a benchmark;
- a pass/fail criterion;
- a bounded fallback.

No gameplay-design question is deferred to technical convenience.

No previously established canon is replaced.

# CANON SET COVERAGE REMAINS 64 / 64 RELEASED ENTRIES COVERED.

# PHASE 09 — TECHNICAL ARCHITECTURE & PROTOTYPE IMPLEMENTATION SPEC — COMPLETE.
