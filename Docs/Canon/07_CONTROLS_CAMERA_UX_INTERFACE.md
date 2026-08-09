# LEGO SPACE RTS — CONTROLS, CAMERA, UX & INTERFACE v1.0

**Phase:** 07 — Controls, Camera, UX & Interface  
**Status:** AUTHORITATIVE PROJECT CANON  
**Authority:** Project Instructions + Phase 00 Canon Set Registry + Phase 01 Game Bible Foundation + Phase 02 Faction Bible & Asymmetry + Phase 03 Unit & Building Roster + Phase 04 Economy, Technology & Progression + Phase 05 World & Map Bible + Phase 06 Combat, Damage & Balance Framework

---

# CANON STATUS

Phase 07 defines the complete player-operation layer for the existing RTS.

It inherits without amendment:

- all **35 buildable units**;
- all **31 building / infrastructure entries**;
- Martian Aero Tube Links;
- Rock Monsters and authored neutral interactables;
- Ore / Energy / Crystal economy;
- Operations Capacity;
- Energy Domains and brownout;
- Rock Raider Worksites and automated industrial hauling;
- Astronaut Mission Refit and Forward Service;
- Alien Crystal Charge, Resonance Cores, Surge and Defense Resonance Shunt;
- Martian Aero Tube Networks and mechanical displacement;
- Phase 05 fog, terrain and airspace rules;
- Phase 06 universal commands, combat target layers, transformation/deployment states, repair rules and deterministic combat.

Phase 06 establishes the relevant canonical timing anchors:

- MX-41 transformation — **2.25s**;
- Solar Explorer deploy — **3.0s**;
- MT-201 deploy — **3.5s**;
- Alien Strike reconfiguration — **2.8s**;
- Alien Infiltrator reconfiguration — **2.0s**;
- Mission Refit Configuration Lock — **20s**;
- Alien Surge — **50 Charge / 18s**;
- Martian Stability — **8s**;
- Tube Arrival Recovery — **0.75s**.

Phase 07 does **not** introduce a new gameplay resource, combat stance family, unit class, faction-wide mechanic, production system, or map rule.

**CANON SET COVERAGE REMAINS 64 / 64.**

---

# PART I — UX NORTH STAR

# THE PLAYER COMMANDS A CIVILIZATION, NOT ITS BUSYWORK.

Every interaction must prioritize, in order:

1. strategic intent;
2. battlefield information;
3. meaningful macro decisions;
4. faction mechanics;
5. precise execution.

Routine labor already established as automatic remains automatic.

The player decides:

- where to expand;
- what to build;
- what to produce;
- what to research;
- where armies move;
- which targets matter;
- when faction systems are committed.

The player does not repeatedly manage actions whose only purpose is proving that they can click quickly.

The interface rewards speed and mastery without hiding information required for competent play.

---

# PART II — CORE INTERACTION PRINCIPLES

## 1. SHARED RTS LANGUAGE

Selection, movement, attack orders, control groups, production and camera behavior follow the same fundamentals for all four factions.

Faction asymmetry comes from systems, not arbitrary input differences.

## 2. DIRECT MANIPULATION FIRST

World objects should normally be selected and commanded in the world.

Global panels accelerate expert play but do not replace the physical battlefield.

## 3. RIGHT-CLICK MEANS THE MOST CONSERVATIVE LEGAL CONTEXT ACTION

Context logic never converts an innocent click into an unexpected expensive or destructive special ability.

Strategic commitments require explicit commands.

## 4. PLAYER INTENT WINS

A new non-queued order replaces previous ordinary orders immediately unless the entity is inside an explicitly irreversible transition.

## 5. INFORMATION PRECEDES DECORATION

The player must understand:

- legality;
- state;
- range;
- cost;
- timing;
- connectivity;
- danger;

before interface ornament is considered.

## 6. PROGRESSIVE DISCLOSURE

Normal play shows only essential information.

Selection, targeting, hovering and overlays reveal deeper information as needed.

## 7. LOW COMMAND REDUNDANCY

One command should solve one conceptual problem.

No filler stances, redundant fire modes or duplicate faction buttons are created.

## 8. VISIBLE CONSEQUENCES

Commands produce immediate cursor, marker, state or panel feedback even if the machine itself requires time to turn, deploy or transform.

## 9. CORRECTABLE MISTAKES

Unplaced build ghosts, cancellable transitions and unstarted construction are easy to cancel.

Reasonable misclick correction should not require fighting the interface.

## 10. MINIMAL MODAL INTERRUPTION

Ordinary combat, construction, refitting, research and production never use confirmation popups.

## 11. COLOR IS NEVER THE ONLY SIGNAL

Shape, iconography, line treatment, motion and text reinforce every strategically important color distinction.

## 12. EXPERT SPEED DOES NOT REQUIRE OBSCURE EXPLOITS

Hotkeys, control groups, minimap commands, production selection and queueing accelerate normal intended play.

---

# PART III — PRIMARY INPUT PLATFORM

The canonical primary platform is:

# PC — MOUSE + KEYBOARD

The interaction model assumes:

- accurate pointer input;
- two primary mouse buttons;
- mouse wheel;
- middle-mouse input;
- a full keyboard;
- simultaneous modifier keys.

All gameplay commands are remappable.

Remapping includes:

- keyboard actions;
- mouse buttons;
- camera inputs;
- control-group modifiers;
- overlay keys.

Conflicting bindings generate a warning but are permitted when contexts do not overlap.

## Controller

Controller support is **not a launch requirement for the canonical competitive interface**.

It is deferred as a secondary control project.

Mouse/keyboard interaction must never be weakened to force mechanical parity with controller input.

---

# PART IV — MOUSE INTERACTION

## LEFT CLICK

Normal left click:

- selects one owned selectable entity;
- replaces the current selection;
- selects visible enemy/neutral objects for inspection without granting control.

Clicking empty terrain clears selection unless a targeting mode is active.

## RIGHT CLICK

Normal right click issues the canonical contextual order defined in Part V.

Right-click while an explicit targeting mode is active cancels that targeting mode rather than issuing another command.

## LEFT DRAG

Creates a selection marquee.

Normal marquee selection applies the box-selection priority rules in Part XV.

## RIGHT DRAG

Right-drag has no gameplay order.

A right-click command is recognized only when pointer displacement remains below a small drag threshold.

This prevents camera or hand movement from accidentally issuing orders.

## DOUBLE CLICK

Double-clicking an owned mobile entity selects all currently visible owned entities of the same gameplay type.

## SHIFT + CLICK

Shift modifies selection additively.

Shift-click:

- adds an unselected entity;
- removes an already selected entity.

Shift + marquee adds marquee results to the current selection.

Shift + command queues the command where queueing is legal.

## CTRL + CLICK

Ctrl-clicking an owned unit or building selects all owned entities of the same gameplay type across the entire map.

Ctrl + marquee disables the normal worker-filter rule and selects all owned mobile entities inside the box.

## ALT + CLICK

Alt is the **Inspect / Select-Under modifier**.

Holding Alt shows the relevant range/coverage overlay for the current selection.

Alt-clicking through overlapping true-air or Huge units selects the lowest legal ground selectable under the pointer.

Alt + marquee subtracts matching entities from the current selection.

## MIDDLE MOUSE

Middle-mouse drag pans the camera directly.

No momentum remains after release.

## MOUSE WHEEL

Mouse wheel changes zoom.

Zoom is biased toward the world position beneath the pointer rather than always toward screen center.

Alt + wheel adjusts camera tilt within the safe range established in Part IX.

---

# PART V — CONTEXTUAL RIGHT-CLICK MODEL

Normal right-click priority is deterministic.

## EMPTY GROUND

**Move.**

## VISIBLE ENEMY

If at least one selected entity can attack the target:

- eligible units receive direct Attack;
- ineligible mobile combat/support units move in formation support of the eligible attackers;
- units in Hold Position remain in place.

If no selected entity can legally attack the target, the order is rejected.

The game does not send an entirely noncombat selection charging toward an enemy because the player happened to right-click it.

## FRIENDLY DAMAGED UNIT OR STRUCTURE

If selected units include eligible manual repairers:

- eligible repairers Repair;
- non-repairers remain on their current order.

If no repairer is selected, the click behaves as Move to the friendly object's current position.

## FRIENDLY TRANSPORT

Eligible selected passengers Load.

Ineligible entities remain where they are.

## RESOURCE DEPOSIT

Eligible workers/economic units Harvest.

Other selected entities do not receive a movement order from clicking the resource itself.

## CONSTRUCTION SITE

Eligible builders assist construction.

Other units are unaffected.

## EXCAVATABLE TERRAIN

When an eligible Rock Raider excavator is selected:

**Excavate.**

Otherwise the object behaves as terrain rather than pretending the unit can interact with it.

## FRIENDLY SERVICE FACILITY

A damaged eligible machine right-clicking an appropriate dedicated service facility enters service/repair.

Mission Refit does **not** trigger through this contextual action because it is a strategic configuration decision.

## MARTIAN AERO TUBE STATION

If Tube-eligible Martian units are selected and a valid origin/destination relationship exists:

**Tube Transfer.**

Otherwise the Station behaves as an ordinary friendly destination.

## NEUTRAL INTERACTABLE

The specific authored interaction is used only when the selected unit is eligible.

No generic adventure-game “Use Everything” command exists.

---

# PART VI — CURSOR STATE

Cursor state priority:

1. explicit targeting mode;
2. legal contextual action;
3. ordinary select/move;
4. invalid action.

Canonical functional cursor states are:

- Select;
- Move;
- Attack / direct target;
- Attack-Move destination;
- Repair;
- Harvest;
- Build;
- Invalid Build;
- Load;
- Unload;
- Excavate;
- Mission Refit;
- Transform / Deploy / Reconfigure;
- Tube Link;
- Tube Transfer;
- Surge Anchor;
- Martian Utility Target;
- Invalid Target.

A legal hostile target and legal friendly target must remain distinguishable without relying only on color.

An invalid cursor is accompanied by a short reason when the player attempts the action.

Range, radius or destination previews accompany only actions where that spatial information changes the decision.

---

# PART VII — CAMERA MODEL

The canonical camera is:

# CONSTRAINED PERSPECTIVE STRATEGIC CAMERA

It uses true perspective rather than orthographic projection.

Perspective better communicates:

- large LEGO machinery;
- aircraft height;
- articulated walkers;
- caverns;
- dramatic base structures;
- mechanical transformations;

while the narrow field of view and constrained pitch prevent excessive cinematic distortion.

## CAMERA BASELINE

**Projection:** perspective  
**Vertical field of view:** 36°  
**Default pitch:** 58° downward from horizontal  
**Default yaw:** 45° clockwise from map north  
**Normal rotation increments:** 90°

At 16:9:

**Closest operational zoom:** approximately 24 horizontal build-grid cells visible.

**Normal combat zoom:** approximately 44 horizontal cells.

**Maximum strategic zoom:** approximately 72 horizontal cells.

The camera never becomes a satellite/icon-only strategic layer.

## PAN SPEED

Keyboard pan interpolates with zoom:

- close — 18 cells/s;
- normal — 30 cells/s;
- far — 46 cells/s.

Edge scrolling uses approximately 80% of keyboard pan speed.

## CAMERA ACCELERATION

Keyboard/edge movement reaches target velocity in approximately **0.14s**.

Deceleration occurs over approximately **0.10s**.

## CAMERA SMOOTHING

Camera positional smoothing baseline:

**0.08s critically damped response.**

Direct middle-drag remains effectively immediate.

## ZOOM

One wheel notch changes camera distance by approximately **8%**.

Zoom smoothing:

**0.10s.**

---

# PART VIII — CAMERA ROTATION

Normal gameplay allows camera rotation.

Rotation is:

# DISCRETE — 90° INCREMENTS

Default keys:

- `,` — rotate counter-clockwise;
- `.` — rotate clockwise.

Free arbitrary yaw rotation is not part of competitive gameplay.

`Home` restores default yaw and pitch while preserving camera position.

## MINIMAP ORIENTATION

The minimap is permanently **north-up**.

It never rotates with the camera.

The camera viewport indicator rotates instead.

This preserves spatial orientation.

---

# PART IX — CAMERA TILT

Gameplay tilt is **slightly adjustable**.

Allowed pitch range:

# 52°–64° DOWNWARD FROM HORIZONTAL

Default:

# 58°

Alt + mouse wheel changes pitch in 2° steps.

The range is deliberately narrow.

The player can improve foreground visibility or appreciate machinery slightly more closely without converting the game into a low-angle action camera.

---

# PART X — CAMERA ZOOM PHILOSOPHY

## CLOSE BAND — APPROXIMATELY 24–32 CELLS ACROSS

Purpose:

- inspect LEGO machinery;
- read transformation states;
- watch construction;
- manage a small tactical engagement;
- interact accurately with dense bases.

It is still an RTS camera.

It is not a toy-inspection macro camera.

## NORMAL COMBAT BAND — APPROXIMATELY 36–52 CELLS

Primary gameplay band.

It supports:

- medium-scale formations;
- 7–10-cell weapon relationships;
- support positioning;
- siege;
- air/ground interaction;
- base-edge fighting.

## STRATEGIC BAND — APPROXIMATELY 52–72 CELLS

Purpose:

- navigate bases;
- manage several production zones;
- move armies between fronts;
- inspect expansion relationships.

Individual unit silhouettes remain visible.

The game never reduces armies to abstract NATO-style counters or satellite dots in the main world view.

---

# PART XI — CAMERA OCCLUSION

Competitive information may not be hidden by decorative geometry.

## FOREGROUND GEOLOGY

Foreground cliffs and major geology use camera-aware fade/dither when they obstruct:

- selected units;
- cursor targets;
- important current commands.

## CAVERNS

Ceilings automatically hide or cut away for the active playable chamber.

Low-tunnel geometry remains visible around its edges because it communicates air-blocking space.

## LARGE STRUCTURES

Only obstructing foreground portions fade.

The full structure does not vanish simply because the camera passes behind it.

## HUGE AIRCRAFT

Huge aircraft remain fully readable during normal play.

When pointer interaction occurs beneath them:

- their lower visual mass becomes temporarily translucent around the cursor;
- Alt-click selects ground objects below.

## FOG

Occlusion handling never reveals objects hidden by fog of war.

---

# PART XII — CAMERA BOUNDS

The camera focus point may move slightly beyond the playable ground so edge bases remain comfortable to inspect.

Maximum normal overscroll:

**approximately 8 build-grid cells.**

Bounds tighten at maximum strategic zoom.

The camera may never expose a large empty void outside the authored world.

At normal settings, no more than approximately 15% of the viewport should contain non-playable exterior space.

Spectator mode may use wider bounds.

---

# PART XIII — CAMERA LOCATIONS / BOOKMARKS

Four camera bookmarks are supported.

Default:

- F5;
- F6;
- F7;
- F8.

`Ctrl + F5–F8` stores the current camera location, zoom and orientation.

Pressing the key recalls it.

Control-group double-tap centers the camera on the group.

It does **not** enter a persistent follow-camera mode.

The strategic camera belongs to the player, not the selected army.

---

# PART XIV — SELECTION MODEL

Supported selection operations:

- single selection;
- marquee selection;
- additive selection;
- subtractive selection;
- visible same-type selection;
- global same-type selection;
- select all army;
- cycle idle worker;
- select all idle workers;
- production selection through the global production interface.

No dedicated global “select every damaged unit” shortcut exists.

Critical infrastructure is instead surfaced through alerts and base-management UI.

Default shortcuts:

- F1 — next idle worker;
- Shift + F1 — all idle workers;
- F2 — all combat/support army units.

---

# PART XV — BOX-SELECTION PRIORITY

Ordinary marquee selection follows this priority:

1. owned mobile combat/support units;
2. workers if no combat/support units are enclosed;
3. aircraft and ground combat units may coexist in one selection;
4. buildings are not added by an ordinary marquee.

Therefore dragging over an army standing beside workers does not accidentally select the economy.

`Ctrl + drag` selects all owned mobile entities inside the box, including workers.

Buildings are selected directly, through global same-type selection, or through production/base-management interfaces.

---

# PART XVI — LARGE / AIR UNIT SELECTION

True-air selection uses a projected selectable anchor rather than making the entire visual hull an opaque interaction wall.

Single-click priority:

- visible aircraft if the cursor directly touches its selectable hull;
- otherwise the ground entity under the cursor.

Alt-click forces ground-under selection.

Marquee selection checks each aircraft's strategic anchor point rather than requiring the complete model silhouette to lie inside the rectangle.

Huge units therefore remain easy to select without preventing ground micro underneath.

---

# PART XVII — SELECTION LIMIT

The player has no practical gameplay selection cap.

Implementation baseline:

# 128 SIMULTANEOUS SELECTED ENTITIES

This exceeds the 100 Operations Capacity competitive ceiling and therefore cannot normally force a player to divide one army for UI reasons.

---

# PART XVIII — MULTI-SELECTION INFORMATION

Mixed selections are grouped by gameplay type.

Example:

- Loader Dozer ×5;
- Granite Grinder ×3;
- Hover Scout ×2.

Each type card communicates:

- count;
- aggregate health distribution;
- relevant state count;
- configuration/deployment count where applicable.

Example:

**T3-Trike ×6 — Escort 4 / Survey 2 — 1 Damaged**

Clicking a type card narrows the selection to that type.

Individual cards become available after narrowing the group.

No normal mixed army requires 30 full-size portrait tiles.

---

# PART XIX — CONTROL GROUPS

Ten control groups:

# 0–9

Default behavior:

**Ctrl + number** — overwrite group.

**Shift + number** — add current selection.

**Alt + number** — remove current selection from that group.

**Number** — recall group.

**Double-tap number** — recall and center camera.

Control-group membership persists through:

- transformations;
- deployment;
- Mission Refit;
- transport loading;
- Tube transit.

Dead entities are removed automatically.

Buildings may belong to control groups.

Transported passengers retain group membership and center at the carrier's current location.

---

# PART XX — COMMAND QUEUE

Shift is the universal command-queue modifier.

## QUEUEABLE

- Move;
- direct Attack;
- Attack-Move;
- Patrol waypoints;
- construction placements;
- Harvest targets;
- Repair targets;
- Excavation targets.

## NON-QUEUEABLE

- Stop;
- Hold Position;
- Spread toggle;
- Mission Refit;
- Surge;
- Defense Resonance Shunt;
- Energy priority changes;
- cancellation actions.

These represent immediate state or strategic commitment decisions.

## CONDITIONAL

- Load;
- Unload;
- Tube Transfer;
- tactical Transform / Deploy / Reconfigure.

Conditional actions are validated again when execution is reached.

One tactical transformation may be queued after movement.

Repeated alternating transform commands cannot be stacked to exploit animations.

A queued Tube Transfer fails clearly if no valid route exists when it reaches execution.

---

# PART XXI — UNIVERSAL COMMAND PANEL

The command panel is a:

# 3 × 4 PRIMARY GRID — MAXIMUM 12 VISIBLE COMMANDS

It occupies the lower-right control region.

The grid has stable conceptual zones:

**upper-left:** core order/state.

**upper-middle:** service/configuration.

**upper-right:** special faction action.

**remaining cells:** movement, repair, transport, build/context commands.

Buttons disappear only when their command class is truly irrelevant.

Commands that exist but are currently unavailable remain visible but disabled when understanding the missing requirement is strategically important.

Submenus replace the grid in place.

`Esc` or right-click returns one level.

No submenu opens a full-screen panel during combat.

---

# PART XXII — COMMAND SLOT DISCIPLINE

Normal mixed-army combat exposes no more faction-specific active commands than the Phase 06 complexity budget requires.

Typical maximum simultaneous faction-specific commands:

- Rock Raiders — 1;
- Astronauts — 2;
- Aliens — 3;
- Martians — 2.

Unused command slots remain unused.

Empty space is preferable to invented micro.

---

# PART XXIII — STANDARD HOTKEY LANGUAGE

The game uses a:

# HYBRID HOTKEY MODEL

Universal orders use mnemonic keys.

Contextual build, production, research and faction-special panels use positional grid keys.

Core defaults:

| Action | Default |
|---|---|
| Move | M |
| Attack cursor | A |
| Attack-Move | A then ground |
| Direct Attack | A then enemy / right-click enemy |
| Stop | S |
| Hold Position | H |
| Patrol | P |
| Repair | R |
| Spread | V |
| Load | L |
| Unload | U |
| Build | B |
| Center selected | C |
| Ping | G |
| Cancel / Back | Esc |
| Last actionable alert | Space |

The Attack command is deliberately unified:

- target enemy → direct Attack;
- target ground → Attack-Move.

This removes a redundant button without changing the underlying two order types.

---

# PART XXIV — FACTION-SPECIFIC HOTKEY CONSISTENCY

Faction commands reuse three conceptual grid positions:

# Q — STATE CHANGE

Used for:

- Transform;
- Deploy;
- Reconfigure;
- Protector Stance;
- Searcher Brace.

# W — SERVICE / CONFIGURATION / NETWORK

Used for:

- Mission Refit;
- Crystal commitment panel;
- Tube Link construction;
- equivalent infrastructure configuration.

# E — SPECIAL TARGETED FACTION ACTION

Used for:

- Excavation;
- Defense Resonance Shunt where selected;
- Excavation Clamp;
- Tube Transfer where selected-unit context applies;
- Surge when an eligible anchor is selected.

Equivalent mechanics therefore reuse learned physical positions without pretending the mechanics themselves are identical.

---

# PART XXV — RALLY POINTS

Every production structure supports a rally point.

Right-clicking terrain sets a ground rally.

Rallying a worker-producing structure directly to a visible resource deposit causes produced workers to begin harvesting.

Other production structures rally to the ground adjacent to a resource rather than interpreting it as an economic command.

Rallying to a friendly unit stores that unit's **current location**.

It does not create automatic permanent following.

New units do not automatically:

- enter transports;
- enter Aero Tubes;
- begin Mission Refit;
- dock for service.

Rally behavior remains predictable.

---

# PART XXVI — BUILDING SELECTION

A selected structure always exposes:

- name;
- functional role;
- HP;
- current damage state;
- target/armor class when relevant;
- Energy state;
- queue or research;
- rally;
- local faction-system state.

Detailed armor numbers, exact weapon formulas and advanced economics live in expanded tooltips.

A selected production building shows its queue immediately.

A selected network/service building prioritizes that mechanic's local information over generic lore.

---

# PART XXVII — BUILD MODE

`B` opens Build Mode.

Buildings are organized into consistent categories:

1. Economy;
2. Production;
3. Technology / Service;
4. Defense / Infrastructure.

The exact assets differ by faction.

## PLACEMENT

A semi-transparent building ghost snaps to the Phase 05 build grid.

Grid resolution:

# 1 BUILD-GRID CELL

The player sees:

- footprint;
- production exit;
- relevant service radius;
- Tube socket geometry;
- air-clearance requirement;
- blocked cells.

A placement is confirmed with left click.

Right-click or Esc cancels the ghost.

## INVALID PLACEMENT

The reason is stated directly.

Possible messages include:

- Non-buildable terrain;
- Footprint occupied;
- Resource access blocked;
- No legal production exit;
- Heavy-unit clearance blocked;
- Outside required infrastructure;
- Tube route obstructed;
- Air clearance obstructed;
- Terrain feature prevents construction.

## RESOURCE RESERVATION

Placement uses the Phase 04 reservation model immediately.

The cost shown before placement is the exact amount that will be reserved.

---

# PART XXVIII — BUILDING ROTATION

Rotation exists only for structures whose orientation changes function or spatial readability.

During Build Mode:

# R — ROTATE 90°

Rotatable examples include structures with:

- production ramps;
- vehicle bays;
- elongated service geometry;
- aircraft exits;
- Tube endpoints.

Symmetrical generators, compact command modules and defenses without meaningful orientation auto-orient visually and do not require player rotation.

Default orientation attempts to face the largest legal open path.

---

# PART XXIX — PRODUCTION EXIT SAFETY

Every production building has an authored exit reservation.

Placement validation checks the largest unit the building can produce.

A structure cannot be placed if its primary exit has no legal route to navigable terrain.

This explicitly protects:

- Chrome Crusher;
- MT-101;
- MT-201;
- Excavation Searcher;
- large aircraft.

If a valid exit later becomes temporarily blocked by units:

- production completes;
- the unit waits inside the exit buffer;
- nearby friendly units receive temporary path-clear priority;
- the unit emerges when legal space exists.

A rally point may be blocked temporarily.

The building itself cannot be permanently designed into a trap.

---

# PART XXX — CONSTRUCTION QUEUE / MULTI-BUILD

Shift-placement retains the current building ghost after each placement.

Each confirmed site independently reserves its cost.

A worker may receive a queue of several construction sites.

The worker visits them in order.

Repeated infrastructure therefore does not require reopening Build Mode for every structure.

No whole-base blueprint automation exists.

`Ctrl+Z` within the brief pre-construction placement window cancels the most recent unstarted placement if no irreversible cost has yet been committed.

---

# PART XXXI — RESOURCE HUD

The global resource strip always displays:

## ORE

Current owned processed Ore.

Where Ore is split between disconnected local pools, a pool-segmentation indicator appears.

## ENERGY

Compact canonical format:

# RESERVE / CAPACITY | GENERATION ↑ | DEMAND ↓ | NET ±/s

Example:

**86 / 250 | 18↑ | 15↓ | +3/s**

## CRYSTALS

Spendable Crystals.

Committed Alien Crystals remain integrated into the Alien faction-mechanic module rather than masquerading as another strategic resource.

## OPERATIONS CAPACITY

# CURRENT / MAXIMUM

At **85%** capacity the meter enters advance-warning state.

At cap, production requiring additional OC is disabled.

---

# PART XXXII — ENERGY UX

The global Energy display answers immediately:

- how much is stored;
- how much is generated;
- how much is demanded;
- whether reserve is rising or falling.

Clicking the Energy display opens a compact **Energy Domain popover**.

Each domain shows:

- nearest command/settlement identifier;
- reserve;
- capacity;
- generation;
- demand;
- net rate;
- brownout state.

The popover is diagnostic, not a permanent spreadsheet.

Selecting a domain:

- centers its relevant infrastructure if requested;
- enables Energy Overlay for that domain.

---

# PART XXXIII — ENERGY PRIORITY CONTROLS

Power priority remains:

- High;
- Normal;
- Low.

It is changed from a selected building's expanded status row.

Normal is default.

High and Low receive compact persistent state icons.

Normal has no icon.

Multi-selecting compatible owned buildings permits setting all of them to the same priority.

Energy priority does not occupy a permanent command-panel slot during ordinary play.

---

# PART XXXIV — BROWNOUT UX

On brownout entry:

1. one High-priority alert identifies the Energy Domain;
2. the top Energy display shows deficit state;
3. disabled buildings receive a brownout indicator;
4. selecting a disabled structure says exactly why it stopped.

Example:

**Brownout — Low-priority structure disabled. Domain demand exceeds generation by 4 E/s.**

The same domain does not repeatedly issue an alarm for each disabled structure.

A new alert occurs only when:

- brownout begins;
- severity materially escalates;
- the domain recovers.

---

# PART XXXV — PRODUCTION UI

Each production button shows:

- unit icon;
- Ore cost;
- Energy cost;
- Crystal cost where applicable;
- OC requirement.

Expanded tooltip adds:

- build time;
- role;
- prerequisites;
- strategic purpose.

## QUEUE

Maximum queued items per production structure:

# 8

The active item occupies the first slot.

Waiting items may be reordered by drag.

The active item cannot be moved behind waiting items.

Reordering has no economic effect.

Cancellation displays the expected refund before confirmation by click.

No modal confirmation appears.

## REPEAT PRODUCTION

There is no automatic infinite repeat-production toggle in competitive play.

Queueing and multi-production selection provide sufficient low-busywork scaling without allowing forgotten factories to consume the economy indefinitely.

---

# PART XXXVI — MULTI-PRODUCTION SELECTION

Selecting several identical production structures creates one aggregate production panel.

Clicking a unit:

- adds it to the eligible building with the shortest projected queue;
- round-robins ties.

Shift-click adds five copies, distributed by shortest projected completion time.

The player may expand the aggregate panel to inspect individual facility queues.

This supports large production bases without converting one click into “build one copy at every structure.”

---

# PART XXXVII — GLOBAL PRODUCTION ACCESS

The game uses a hybrid model.

Physical structure selection remains fully supported.

Expert players may press:

# F3 — PRODUCTION OVERVIEW

The overview is a side drawer containing actual owned production structures grouped by type and base.

It allows:

- selecting facilities;
- queueing units;
- seeing idle facilities;
- viewing aggregate queues.

It does not create fictional off-map production.

Every queue still belongs to a physical structure.

Clicking a facility in the drawer can center the camera on it.

---

# PART XXXVIII — RESEARCH / TECHNOLOGY UI

# F4 — TECHNOLOGY OVERVIEW

Research is organized by the Phase 04 categories:

- Economic;
- Operational;
- Combat;
- Faction System.

Faction progression relationships are shown as compact prerequisite chains, not one enormous technology web.

Each research card communicates:

- name;
- category;
- cost;
- time;
- prerequisite;
- affected assets;
- visible/mechanical consequence;
- current state.

States:

- Locked;
- Available;
- Researching;
- Complete.

A locked technology names the exact missing requirement.

Research may also be initiated from its physical research structure.

---

# PART XXXIX — TECH DISCOVERY & ENEMY INFORMATION

Enemy information obeys fog and scouting.

Seeing an enemy structure reveals:

- its identity;
- obvious functional role;
- currently visible physical state.

It does not reveal:

- production queue;
- research progress;
- banked resources.

Seeing a transformed/deployed unit reveals that current state.

Visible equipment upgrades may be represented in the enemy information panel when their physical modification is genuinely observable.

A visible Resonance Core may communicate its physically visible occupied commitment arms.

It never reveals the enemy's exact global Charge amount.

---

# PART XL — UNIT INFORMATION PANEL

Single-unit hierarchy:

1. unit name;
2. role tags;
3. HP;
4. current state;
5. weapon target layers;
6. special mechanic;
7. transport/service information where relevant.

Secondary details:

- target class;
- Armor Rating;
- damage type;
- damage;
- cooldown;
- range;
- minimum range;
- movement speed;
- sight;
- detection.

Secondary values appear in Expanded Detail rather than filling the default panel.

Official/source-family naming may appear as a subtitle when useful for identity.

---

# PART XLI — TOOLTIP SYSTEM

## QUICK TOOLTIP

Appears after approximately:

# 0.30s

Shows:

- function;
- cost;
- hotkey;
- short strategic sentence.

## EXPANDED TOOLTIP

Appears after approximately:

# 0.90s

or immediately while Alt is held over UI.

Adds:

- counters;
- vulnerabilities;
- exact mechanics;
- timing;
- prerequisite explanation;
- affected target layers.

Settings may force:

- Quick only;
- Expanded always;
- custom delay.

Tooltips explain strategy rather than simply repeating names.

---

# PART XLII — ROLE & COUNTER LANGUAGE

Canonical role vocabulary:

- Worker;
- Scout;
- Frontline;
- Anti-Light;
- Anti-Heavy;
- Anti-Air;
- Siege;
- Support;
- Transport;
- Control.

A unit should normally display no more than three role tags.

True Air, Hover, Walker and Ground are movement information rather than combat-role tags.

---

# PART XLIII — HEALTH BARS

Default behavior is **Contextual**.

Friendly health bars appear when:

- selected;
- damaged;
- recently in combat.

Enemy bars appear when:

- targeted/selected;
- damaged during visible combat.

Neutral hostile bars use the same logic.

Structure bars appear while:

- selected;
- damaged;
- under attack.

Options include:

- Contextual;
- Always for owned;
- Always for visible combatants;
- Selected only.

---

# PART XLIV — DAMAGE STATES

Phase 06 damage thresholds remain authoritative.

## HEALTHY

# 70–100%

## DAMAGED

# 35–69%

## HEAVILY DAMAGED

# 20–34%

## CRITICAL

# BELOW 20%

These are readability states only.

They do not reduce:

- production speed;
- attack rate;
- movement;
- research.

Later visual presentation may use:

- displaced LEGO pieces;
- sparks;
- exposed machinery;
- alarms.

---

# PART XLV — RANGE DISPLAY

Range circles are contextual, not permanent.

They appear automatically when:

- placing static defense;
- hovering a range-relevant command;
- deploying siege;
- targeting with a ranged special action.

Holding Alt with a selection shows its relevant attack/service/sensor ranges.

If multiple unit types are selected, overlapping identical ranges are consolidated rather than drawing one circle per unit.

Minimum range is shown as an inner boundary.

The same overlay grammar is reused for:

- attack range;
- minimum range;
- Surge radius;
- service radius;
- detector/sight;
- Worksite service;
- Tube Link preview.

---

# PART XLVI — TARGET LEGALITY FEEDBACK

A mixed force never rejects an entire command because one member cannot execute it.

On hostile hover, the targeting cursor may show:

# ELIGIBLE / SELECTED

Example:

**6 / 10**

Reasons for ineligibility are available in tooltip:

- Ground weapon cannot target true air;
- Inside minimum range;
- No line of sight;
- No ground path;
- Reconfiguring;
- Transported.

Eligible units act.

Ineligible non-Hold combat/support units move into sensible supporting formation.

Hold-position units remain held.

---

# PART XLVII — FOG OF WAR UX

Fog has three canonical states.

## UNEXPLORED

Terrain and objects are unknown.

No topology, deposits or structures are shown.

## EXPLORED BUT NOT CURRENTLY VISIBLE

Terrain remains as remembered geometry.

Dynamic information is stale.

Previously seen enemy structures remain as last-known silhouettes.

Resources remain at their last observed depletion state.

## CURRENTLY VISIBLE

All normally observable current information is shown.

Phase 06 mobile last-known enemy markers persist for approximately **3 seconds**, then disappear.

Terrain changes outside vision do not update memory.

A Rock Raider excavation opened in fog remains unknown until observed again.

---

# PART XLVIII — SIGHT & DETECTION

Sight determines normal visibility.

Detection handles special concealed authored elements and information interactions.

Because standard competitive play contains no normal cloak metagame, the UI never presents Detection as though every army requires a dedicated stealth counter.

Holding Alt on a selected detector displays:

- sight radius;
- detection radius as a distinct line treatment.

---

# PART XLIX — MINIMAP

The minimap is a competitive information surface.

Subject to fog it shows:

- terrain;
- exploration state;
- friendly units;
- visible enemy units;
- remembered enemy structures;
- resources;
- expansions;
- objectives;
- attack alerts;
- visible true-air threats;
- known siege pressure;
- own Tube topology;
- known enemy Tube infrastructure;
- major visible Surge activation.

## SYMBOL DISCIPLINE

Ground mobile unit — simple point.

True air — triangular/winged point.

Structure — square/block.

Resource — dedicated resource symbol.

The minimap does not attempt to display every unit role.

Worker and army density are distinguished primarily by clustering and scale rather than dozens of icons.

---

# PART L — MINIMAP INTERACTION

Left-click:

**move camera.**

Left-drag:

**move camera viewport continuously.**

Right-click with units selected:

**Move to minimap position.**

Shift + right-click:

**queue Move.**

While Attack targeting is active, right-click minimap produces Attack-Move.

Direct focus-target attack on an individual minimap pip is not supported.

Direct targeting belongs in the world view where target identity is clear.

`G` then minimap click places a team ping.

---

# PART LI — MINIMAP NETWORK OVERLAYS

## MARTIANS

Owned Aero Tube connections appear as thin topology lines.

Selected Station:

- emphasizes its connected component;
- highlights reachable Stations.

Disconnected/damaged links use different line patterns, not only different colors.

Enemy Tube topology appears only where visibility/fog rules permit knowledge.

## ROCK RAIDERS

Worksites do **not** draw permanent full-map connection lines.

The minimap shows:

- Worksite nodes;
- disconnected strategic structures when relevant.

Detailed radius relationships belong to the Worksite overlay.

---

# PART LII — ALERT SYSTEM

Four alert classes:

# CRITICAL

Examples:

- command structure in severe danger;
- catastrophic objective failure imminent;
- major owned base collapse.

# HIGH

Examples:

- expansion under meaningful attack;
- Resonance Core attacked;
- major Tube segmentation;
- Worksite infrastructure severed;
- brownout begins;
- visible enemy siege begins firing on important infrastructure.

# NORMAL

Examples:

- research complete;
- major construction complete;
- important resource deposit depleted;
- expansion complete.

# INFORMATIONAL

Examples:

- refit complete;
- normal advanced unit complete;
- Tube transfer complete when specifically requested.

Critical/High alerts may create audio + minimap pulse.

Normal/Informational primarily use the event feed.

---

# PART LIII — ATTACK ALERT LOGIC

Damage alone is not sufficient to generate an alert.

A new attack sector alert is generated when one of the following occurs:

- a meaningful owned cluster loses approximately 8% aggregate HP within 2 seconds;
- workers are actively attacked;
- a strategic structure is attacked;
- siege begins operating against infrastructure.

Sector cooldown:

# 12s

for approximately a 14-cell local area.

Damage during cooldown does not replay the same alert.

Escalation overrides cooldown if:

- a command structure becomes threatened;
- the position falls to Critical;
- worker losses begin;
- an important faction-system structure becomes threatened.

---

# PART LIV — EVENT FEED

A lightweight event feed records strategic events.

Typical entries:

- research complete;
- expansion complete;
- major structure lost;
- Resonance Core destroyed;
- Tube network segmented;
- Deep Ore exhausted;
- Crystal field depleted;
- major refit complete.

It is not a combat log.

Routine damage numbers and every unit death do not create entries.

---

# PART LV — ROCK RAIDER WORKSITE UX

## NORMAL VIEW

Worksite routing is intentionally quiet.

Serviced structures need no permanent link line.

Disconnected infrastructure displays a compact warning state.

Automated hauling remains visible in-world but its logistics rigs are not selectable.

## WORKSITE STRUCTURE SELECTED

The panel shows:

- Worksite component;
- service status;
- Energy Domain;
- local/connected processed reserve;
- repair/service availability;
- disconnected dependencies.

The selected service radius appears temporarily.

## WORKSITE OVERLAY

F10 activates the faction infrastructure overlay.

For Rock Raiders it shows:

- HQ 18-cell service zones;
- Vehicle Service Bay 12-cell zones;
- connected component grouping;
- disconnected structures;
- raw extraction/receiving relationship;
- Energy-domain boundaries.

It uses broad industrial coverage regions rather than cable diagrams.

---

# PART LVI — ROCK RAIDER EXCAVATION UX

Eligible excavation units:

- Drill Craft;
- later eligible Granite Grinder / Chrome Crusher interactions defined by existing canon.

When one is selected, valid currently known authored Excavatable Terrain gains a subtle interaction highlight on hover.

Hover displays:

- feature type;
- expected route/resource result when legitimately known;
- Energy cost;
- estimated duration;
- eligible machine.

Standard feature:

approximately **25 Energy**.

Reinforced feature:

approximately **50 Energy**.

Issuing Excavate:

1. marks the feature;
2. displays progress on selected machine and feature;
3. briefly previews the resulting legal opening;
4. begins the physical drilling operation.

Cancellation follows normal command interruption if excavation has not reached an irreversible authored transition.

Unseen terrain beyond the feature is not revealed by the tooltip.

---

# PART LVII — ROCK RAIDER REPAIR UX

Three repair relationships are distinct.

## FIELD REPAIR

Manual Crew command.

The repair cursor displays:

- eligible target;
- approximate resource expenditure;
- under-fire reduction if active.

## WORKSITE PASSIVE MAINTENANCE

Automatic.

No command button exists.

A stationary eligible Raider machine that qualifies for passive maintenance displays a small service indicator only while selected or hovered.

## VEHICLE SERVICE BAY

Dedicated service.

Damaged machines may right-click the Bay.

The panel shows repair queue and estimated restoration.

Under-fire reduction is communicated as a degraded service-rate state rather than hidden.

---

# PART LVIII — ASTRONAUT MISSION REFIT UX

Mission Refit uses a dedicated **configuration interface**, visually and behaviorally distinct from tactical transformation.

Refit-eligible assets only:

- T3-Trike;
- Mobile Mining Platform;
- Modular Sentinel Defense.

An eligible asset shows:

- current configuration;
- owned alternate module;
- unowned alternate module;
- service eligibility.

Pressing W opens its two-configuration selector.

## T3-TRIKE

Escort ↔ Survey.

First alternate installation:

- 25 Ore;
- 10 Energy;
- 18s.

Later swap:

- 8 Ore;
- 5 Energy;
- 10s.

## MOBILE MINING PLATFORM

Ore Drill ↔ Crystal Reaper.

First alternate installation:

- 60 Ore;
- 35 Energy;
- 2 Crystals;
- 25s.

Later swap:

- 15 Ore;
- 10 Energy;
- 15s.

## MODULAR SENTINEL

Ground Interdiction ↔ Air Interception.

First alternate installation:

- 30 Ore;
- 20 Energy;
- 20s.

Later swap:

- 10 Ore;
- 10 Energy;
- 15s.

## CONFIGURATION LOCK

After completion:

# 20s

A compact lock indicator appears on the selected asset.

The world does not show a giant countdown over every refitted unit.

## SERVICE REQUIREMENT

If outside valid service:

the Refit button remains visible but disabled with:

**Requires Forward Service.**

Solar Explorer may field-refit eligible mobile assets.

It may not refit Sentinel structures.

---

# PART LIX — ASTRONAUT SERVICE COVERAGE UX

Service & Refit Hub radius:

# 18 CELLS

Deployed Solar Explorer radius:

# 10 CELLS

The service radius appears when:

- the structure/unit is selected;
- a refit-eligible unit is selected near the boundary;
- Deploy is being previewed;
- F10 faction infrastructure overlay is active.

It is never a permanent combat circle.

An eligible unit inside coverage displays **Service Available** in its panel.

Outside coverage:

**No Forward Service.**

---

# PART LX — ASTRONAUT TRANSFORMATION UX

Mission Refit and tactical state changes use different command categories.

## MISSION REFIT

- strategic;
- costs resources;
- requires service;
- takes substantial time;
- has Configuration Lock.

## TACTICAL TRANSFORMATION / DEPLOYMENT

Applies to:

- MX-41;
- Solar Explorer;
- MT-201.

Uses Q — State Change.

Current and target forms are always named.

Example:

**Ground → Flight — 2.25s**

Transition progress appears above the command grid and on the selected entity's health/state plate.

Reversal lock appears after completion.

Cancellation eligibility changes visibly at the Phase 06 cancel threshold.

---

# PART LXI — ALIEN CRYSTAL / CHARGE HUD

Alien Crystals remain one strategic resource with two ownership states.

The top HUD shows:

# CRYSTALS — SPENDABLE

Adjacent to it, one integrated Alien Resonance module shows:

# CHARGE / MAX | +CHARGE/s | COMMITTED CRYSTALS

Example:

**Charge 72 / 100 | +1.6/s | 4 committed**

This is one faction mechanic panel.

Committed Crystals are not presented as a fourth or fifth independent resource.

Hovering the module expands:

- operational Core count;
- brownout-paused Cores;
- total commitment capacity.

Charge does not decay and pauses generation at maximum as established in Phase 04.

---

# PART LXII — RESONANCE CORE UX

Selecting a Resonance Core shows:

- committed slots;
- maximum slots;
- Charge generation contributed;
- continuous Energy demand;
- operational/brownout state;
- commitment change.

The commitment interface sets a **desired committed count**.

The Core then performs the canonical 8-second commit or 15-second withdrawal operations sequentially.

This avoids repetitive one-Crystal micromanagement while preserving real transition time.

A withdrawing slot displays progress.

Brownout clearly shows:

**Charge generation paused — committed Crystals remain installed.**

Global Charge remains visible in the top HUD.

---

# PART LXIII — ALIEN SURGE UX

Surge may be initiated from:

- a selected valid Resonance Core;
- an upgraded valid Alien Mothership;
- the Alien Resonance HUD shortcut.

Surge command flow:

1. player activates Surge;
2. all valid owned anchors highlight;
3. hovering an anchor previews its zone;
4. HUD shows **50 Charge** cost;
5. player confirms the anchor;
6. 0.75s buildup begins;
7. 18s Surge Window begins.

Core radius:

# 12 CELLS

Mothership Relay radius:

# 10 CELLS

The active zone remains readable in the world and minimap.

A single global Surge timer appears in the Alien mechanic module for the most recently activated player-owned zone; additional concurrent zones, if economically possible, are represented as small secondary timers.

Eligible units automatically gain/lose Surge when crossing the boundary.

The player never individually activates Surged units.

Insufficient Charge produces:

**Requires 50 Charge — current: X.**

Enemy Surge zones are shown only while legitimately visible.

---

# PART LXIV — ALIEN RECONFIGURATION UX

ETX Alien Strike and ETX Alien Infiltrator use Q — State Change.

## ALIEN STRIKE

Flight ↔ Siege.

Panel communicates:

- current state;
- target state;
- 2.8s transition;
- gained siege role;
- lost mobility/air state;
- 8s reversal lock.

## INFILTRATOR

Craft ↔ Walker.

Panel communicates:

- current state;
- target state;
- 2.0s transition;
- movement/target-layer consequences;
- gained combat function;
- 6s reversal lock.

Under Surge, displayed transition time updates to the canonical ×0.70 value before activation.

---

# PART LXV — DEFENSE RESONANCE SHUNT UX

Defense Resonance Shunt remains a **local selected-defense emergency action**.

Select ETX Defense Node → E.

Button displays:

- 25 Charge;
- 12s duration.

On activation:

- local Node enters its high-output state;
- local timer appears;
- Alien Charge HUD decreases immediately.

The Node communicates its improved cadence/tracking state.

Shunt is not a global “defend all bases” button.

---

# PART LXVI — MARTIAN NETWORK UX

The central Martian network interface is one **Aero Network Overlay**.

F10 activates it.

It shows:

- Stations;
- active Links;
- connected components;
- damaged/severed spans;
- pooled resource components;
- Energy-domain components;
- transfer routes when relevant.

Normal combat view shows only physical Tubes and local state indicators.

Full topology does not remain overlaid permanently.

Selecting a Station temporarily emphasizes its own component.

---

# PART LXVII — MARTIAN LINK CONSTRUCTION

Select:

- Aero Tube Hangar;
- or Settlement Station.

Press W → Build Link.

Valid destination Stations highlight.

Hovering one displays:

- route;
- link length;
- Ore cost;
- one-time Energy activation cost;
- continuous 1 E/s demand;
- connection slots used.

Canonical cost:

# 50 ORE + 2 ORE PER LINK CELL

Activation:

# 10 ENERGY

Construction:

# 10s + 0.4s PER LINK CELL

The game determines the readable path automatically.

The player never draws Tube bends manually.

Invalid reasons include:

- connection limit reached;
- destination already connected;
- route blocked;
- Station unavailable;
- insufficient resources.

---

# PART LXVIII — MARTIAN TUBE TRANSFER

Tube-eligible units remain:

- Worker Robot;
- Double Hover;
- Jet Scooter.

Select eligible units → E — Tube Transfer.

A valid nearby origin Station highlights automatically.

Connected destinations highlight.

The player chooses one destination.

The units:

1. move to origin;
2. enter Station transfer queue;
3. load into the network;
4. travel off-map through Tube simulation;
5. arrive at destination;
6. complete 0.75s Arrival Recovery.

A valid right-click on a reachable destination Station invokes the same action directly.

Mixed selections transfer only eligible units.

Heavy Martian units remain where they are and receive a concise eligibility message.

---

# PART LXIX — MARTIAN STATION QUEUE

Baseline Station throughput:

# 2 SIMULTANEOUS UNIT TRANSFERS

After Hypersled Throughput:

# 3

Station selection displays:

- active transfer channels;
- queued unit count;
- grouped destinations;
- estimated travel time.

Queues are summarized by unit type/destination.

The player does not manage Tube cargo slots individually.

---

# PART LXX — MARTIAN NETWORK FAILURE UX

When a Link or Station loss segments the network:

1. one High alert fires;
2. affected component briefly highlights in the network overlay;
3. isolated Stations receive a disconnected state;
4. global Ore/Energy HUD indicates that multiple local pools now exist;
5. affected transfers display rerouting/return state.

With Redundant Routing, eligible routes automatically reroute according to existing canon.

Without a route, transit resolves safely rather than killing units.

The UI does not generate one alert per broken Tube span.

---

# PART LXXI — MARTIAN COMBAT-STATE UX

## PROTECTOR STANCE

Q toggles Mobile ↔ Protector Stance.

Deploy:

**2.4s**

Undeploy:

**2.0s**

Reversal lock:

**8s**.

The selected-unit panel communicates:

- immobility;
- improved defensive/frontal role;
- current lock.

## SEARCHER BRACE

Q toggles Mobile ↔ Excavation Brace.

Deploy:

**2.5s**

Undeploy:

**2.0s**

Lock:

**10s**.

Siege range appears during deployment preview.

## EXCAVATION CLAMP

E enters target mode.

Legal friendly/enemy displacement targets highlight.

Massive/structure targets clearly show immunity.

## STABILITY

After hostile displacement the affected unit receives **8s Stability**.

World representation:

- one small mechanical-stability indicator immediately after displacement;
- fades after approximately 1.5s.

Exact remaining Stability appears only in the selected unit panel.

The battlefield therefore does not fill with eight-second countdown icons.

---

# PART LXXII — TRANSPORT UX

Shared transport language applies to:

- Rapid Rider;
- Tunnel Transport;
- MX-71 Recon Dropship;
- Alien Mothership;
- Aero Skiff;
- any canonical Solar Explorer passenger capacity.

## LOAD

Select passengers → right-click transport.

Or select passengers + transport together and press L.

Eligible passengers distribute across selected transports by nearest available capacity.

## LOAD ALL ELIGIBLE SELECTED

When transport and potential passengers share a selection, L loads every selected eligible passenger until capacity is filled.

It never automatically grabs arbitrary nearby units outside the selection.

## UNLOAD

Select transport → U → target ground.

The cursor previews:

- legal unload area;
- required clearance;
- invalid terrain.

## CAPACITY

Transport panel always shows:

# OCCUPIED / TOTAL

Incompatible units remain visibly ineligible.

Transport destruction continues to emergency-deploy surviving passengers at 40% HP with Phase 06 recovery penalties.

---

# PART LXXIII — PASSENGER PANEL

Transport selection shows passengers grouped by type.

Each passenger entry includes:

- type;
- health;
- transported state.

Default U unloads all.

The player may click one passenger/type entry and issue **Unload Selected**.

Selective unloading is therefore possible without turning transport play into passenger-by-passenger mandatory micro.

Passengers cannot be individually targeted by enemies while loaded.

---

# PART LXXIV — REPAIR UX

Repair uses one shared interaction language.

## MANUAL REPAIR

Repairer selected → R or right-click damaged friendly.

Panel shows:

- repair eligibility;
- approximate rate;
- expected resource cost to full;
- under-fire efficiency state.

## DEDICATED SERVICE

Damaged machine right-clicks eligible service facility.

Dedicated repair queue appears on facility selection.

## PASSIVE MAINTENANCE

Only systems already established as automatic operate automatically.

Examples:

- Rock Raider Worksite passive maintenance;
- Martian out-of-combat Worker Robot maintenance after relevant technology.

## ALIEN DOCK REPAIR

Eligible Alien units right-click Fabricator/Reconfiguration Dock.

## SOLAR EXPLORER

Deployed Solar Explorer exposes repair availability through service coverage.

No faction receives a hidden generic regeneration aura.

---

# PART LXXV — COMBAT COMMAND UX

## MOVE

Right-click terrain or M + left-click.

World marker:

brief destination marker.

## ATTACK

Right-click visible enemy or A + target.

Target receives a short attack bracket.

## ATTACK-MOVE

A + ground.

Destination receives an attack-move marker distinct in shape from Move.

## STOP

S.

Immediate command acknowledgment.

No target cursor.

## HOLD

H.

Selected entities display Hold state in panel and brief world marker.

## PATROL

P then destination.

Patrol endpoints/path appear while the unit remains selected.

## SPREAD

V.

Toggles Normal ↔ Spread formation preference.

All feedback is functional and brief.

---

# PART LXXVI — FORMATION UX

Phase 06's automatic role-aware loose formation remains default.

Spread increases spacing by approximately:

# 40%

Spread is a persistent formation preference on the selected units until toggled back.

New units begin in Normal.

A mixed selection containing different formation preferences shows:

**Formation: Mixed**

No additional universal formation stances exist.

---

# PART LXXVII — SIEGE UX

Siege interfaces share state language but preserve mechanical differences.

## GRANITE GRINDER

No deployment.

Contact-range Attack clearly previews approach point.

Sustained-drill engagement status appears when active.

## CHROME CRUSHER

No siege stance.

Attack targeting emphasizes contact requirement and path accessibility.

## MT-201

Q deploys Travel → Drill.

Placement preview shows:

- final anchored footprint;
- targetable structure relationship;
- deployment progress.

## ALIEN STRIKE

Q reconfigures Flight → Siege.

Siege mode shows:

- 9-cell attack range;
- current minimum-range relationship where applicable;
- immobility/state sacrifice.

## EXCAVATION SEARCHER

Q enables Excavation Brace.

Brace state exposes its siege relationship plus E Excavation Clamp.

The UI does not falsely make all five machines use the same artillery metaphor.

---

# PART LXXVIII — AIR UX

True air uses a dedicated movement-layer indicator.

Ground-layer hover remains visually grounded and does not receive the air indicator.

For true air:

- selection anchor includes altitude/layer mark;
- minimap uses triangular air point;
- targeting cursor distinguishes legal AA interaction;
- ground-only weapons show invalid target state;
- Air-Blocking Volumes appear when an air path is being targeted.

There is no player-controlled altitude.

Air/ground mixed selections remain legal.

---

# PART LXXIX — ATTACK-RANGE / AIR-RANGE READABILITY

Normal 44-cell combat view shows enough world space for:

- unit;
- target;
- most of a 9-cell siege relationship;
- nearby flank;
- static 7–7.5-cell AA coverage.

The player should not need to zoom out simply to understand one normal attack range.

Range overlays are clipped intelligently at screen edges rather than forcing camera movement.

---

# PART LXXX — WORKER UX

Worker controls support:

- Harvest;
- Build;
- Repair;
- Return/delivery behavior;
- idle detection.

F1 cycles idle workers.

Shift+F1 selects all idle workers.

The worker information panel may summarize current assignment:

- Ore;
- Crystals;
- Construction;
- Repair;
- Idle.

The game does not show generic “X/3 saturated” worker counters because faction economies do not share a StarCraft-style saturation model.

Rock Raider automatic logistics rigs are not part of worker selection.

---

# PART LXXXI — RESOURCE-DEPOSIT UX

Current-vision deposits expose exact remaining strategic quantity.

Ore deposit selection/hover shows:

- deposit class;
- exact current Ore;
- active worker count.

Crystal fields show:

- exact remaining Crystals;
- active worker count.

In explored fog:

- last-seen value remains;
- it is explicitly marked as stale.

Exact numbers are chosen because resource depletion is a strategic planning system, not intended hidden-information gameplay.

---

# PART LXXXII — RESOURCE DEPLETION ALERTS

A resource site with active owned harvesting generates one advance warning when it reaches approximately:

# 15% REMAINING

The warning is Normal priority.

At exhaustion, one Informational/Normal event occurs depending on economic significance.

A field with no owned worker assignment generates no depletion alert.

Multiple adjacent fragments belonging to one authored deposit group are treated as one alert source.

---

# PART LXXXIII — MAP OBJECT INTERACTION

All map objects use the same base language:

- hover identifies;
- left-click inspects;
- right-click performs legal context action;
- explicit targeting mode performs unusual actions.

Classes include:

- Excavatable Terrain;
- destructible;
- hazard;
- neutral structure;
- Rock Monster;
- objective.

No separate adventure interaction cursor system is introduced.

---

# PART LXXXIV — HAZARD UX

Hazards communicate primarily through world behavior.

Phase 06 telegraph timings remain canonical, including:

- 2.5s volcanic buildup;
- 1.5s crystal/cryogenic buildup;
- 2s rockfall warning;
- 3s ice-fracture spread.

When the world animation alone might be ambiguous, a restrained ground-boundary overlay reinforces the affected zone.

Paths crossing currently dangerous terrain receive a hazard mark while the command is being previewed.

The game does not automatically cancel a legal risky move.

---

# PART LXXXV — ROCK MONSTER UX

Rock Monsters use a neutral-hostile information treatment.

Minimap:

neutral hostile creature marker.

Selection panel:

- Rock Monster;
- Heavy target class;
- HP;
- Armor;
- current aggression state.

The player is not shown an exact leash circle by default.

Holding Alt while the visible Rock Monster is selected/hovered may show approximate known aggression territory for learning/accessibility.

No production, technology or faction panel exists.

No standard kill bounty is displayed because none exists.

---

# PART LXXXVI — OBJECTIVE UX

The reusable objective framework supports:

- Destroy;
- Defend;
- Escort;
- Capture / Activate;
- Extract;
- Survive;
- Reconnect;
- Excavate.

Active objectives occupy a compact top-side tracker.

Each objective may have:

- world marker;
- minimap marker;
- progress;
- optional timer;
- status.

Standard competitive elimination play does not display unnecessary objective instructions.

---

# PART LXXXVII — SCORE / MATCH STATUS

Ranked competitive HUD displays:

- match timer;
- player/team identities;
- defeat/disconnection state.

It does **not** display a live composite score that indirectly reveals enemy economy or army value.

Detailed statistics are post-game or spectator information.

---

# PART LXXXVIII — PAUSE

Single-player:

`Esc` pauses simulation and opens menu.

Competitive multiplayer:

`Esc` opens menu without pausing simulation.

There is no tactical pause in standard ranked play.

Custom/tournament rules may enable administrative pause separately.

---

# PART LXXXIX — GAME SPEED

Standard multiplayer is locked to:

# 1.0×

Single-player/campaign may support:

- 0.5×;
- 1.0×;
- 1.5×;
- 2.0×.

Game-speed controls are not part of competitive hotkey execution.

---

# PART XC — PING SYSTEM

`G` enters Ping mode.

Left-click world or minimap places one universal attention ping.

The ping communicates:

- position;
- player identity;
- short lifetime.

It does not automatically issue commands to allies.

Repeated ping spam is rate-limited.

---

# PART XCI — TEAM INFORMATION

Team UI may show information the simulation legitimately shares with allies.

It never creates additional vision.

Allied units/structures use allied identification treatment.

The HUD does not automatically reveal an ally's:

- exact resource bank;
- private production queues;
- research progress;
- Charge.

Players coordinate those decisions voluntarily.

---

# PART XCII — CHAT & COMMUNICATION

`Enter` opens team chat by default in team modes.

`Shift + Enter` opens all-chat where the ruleset permits it.

Chat input temporarily captures gameplay keys.

Messages use player/team identity markers independent of faction color alone.

Mute and block controls are available from the player list.

---

# PART XCIII — ACCESSIBILITY

Launch accessibility requirements include:

- full key remapping;
- mouse-button remapping;
- adjustable camera speed;
- edge-scroll toggle;
- adjustable UI scale;
- text-size settings;
- cursor-size settings;
- color-vision presets;
- high-contrast selection mode;
- health-bar behavior options;
- tooltip delay options;
- reduced motion;
- screen-shake control;
- flash reduction;
- subtitle/text equivalents for important audio alerts;
- hold/toggle alternatives where an input would otherwise require sustained pressure;
- adjustable double-click interval.

Core competitive information remains identical.

Accessibility changes presentation/input, not simulation rules.

---

# PART XCIV — COLOR-INDEPENDENT INFORMATION

The following must never rely exclusively on color:

- selected vs unselected;
- friendly vs hostile;
- air vs ground;
- legal vs illegal target;
- health severity;
- resource type;
- brownout;
- Worksite disconnected state;
- Forward Service state;
- Tube segmentation;
- Surge;
- Stability;
- player/team identity.

Shape, icon, outline, pattern, text or motion must provide a secondary cue.

---

# PART XCV — AUDIO UX REQUIREMENTS

Audio must reinforce:

- command acknowledgment;
- illegal action;
- attack alert class;
- production/research completion;
- brownout;
- Refit completion;
- Surge buildup/activation;
- Tube failure;
- excavation completion.

Critical and High alert classes require perceptually distinct sounds.

No essential information may exist only in audio.

Exact sound design belongs to the later Audio implementation/art direction.

---

# PART XCVI — UNIT RESPONSE PHILOSOPHY

Units acknowledge meaningful player orders quickly.

Responses are:

- short;
- faction-appropriate;
- readable over battle audio.

Repeated identical commands within a short interval should not generate a complete voice line every time.

Movement spam should produce restrained acknowledgment.

Faction character may emerge through:

- mechanical sounds;
- crew responses;
- Martian machinery;
- Alien resonance feedback;

without obscuring command confirmation.

---

# PART XCVII — ERROR MESSAGE LANGUAGE

Errors state:

# WHAT FAILED + WHY

Examples:

**Cannot attack — ground weapon cannot target true air.**

**Cannot deploy — footprint obstructed.**

**Cannot refit — no Forward Service.**

**Cannot transfer — no connected Aero Tube route.**

**Cannot build — heavy production exit blocked.**

**Cannot Surge — requires 50 Charge.**

Messages remain short and contextual.

---

# PART XCVIII — CONFIRMATION DIALOG POLICY

Ordinary competitive actions never require modal confirmation.

Confirmation is reserved for actions outside normal execution, such as:

- surrender;
- leaving an active match.

Construction cancellation, production cancellation, research cancellation, Refit cancellation and ordinary command replacement do not use confirmation dialogs.

Refund consequences are shown before cancellation.

---

# PART XCIX — CANCEL / UNDO PHILOSOPHY

Esc/right-click cancels active targeting or Build Mode.

Cancellation buttons expose:

- expected Ore refund;
- Energy refund;
- irreversible Crystal loss where applicable.

No player is expected to memorize refund formulas.

Recent unstarted building placement may use the pre-construction Ctrl+Z cancellation shortcut defined earlier.

After an irreversible transition threshold, the interface changes Cancel to unavailable and explains why.

---

# PART C — NOTIFICATION TIMERS

Timers are shown at the lowest useful hierarchy.

## GLOBAL HUD

- Surge;
- truly global strategic alerts.

## SELECTED ENTITY

- transformation;
- deployment;
- Refit;
- Configuration Lock;
- Stability;
- Tube transit;
- repair.

## WORLD PROGRESS

- construction;
- excavation;
- selected research structure where visible.

No battlefield contains giant timers over every affected unit.

---

# PART CI — GLOBAL VS LOCAL STATES

## GLOBAL HUD

- Ore;
- Energy;
- Crystals;
- OC;
- Alien Charge;
- alerts;
- match state.

## LOCAL / SELECTED OBJECT

- HP;
- weapon;
- repair;
- transformation;
- Mission Refit;
- passenger list;
- Station queue;
- Crystal commitment;
- Energy priority.

## MAP OVERLAY

- Energy Domains;
- Worksite coverage;
- Forward Service;
- Aero Tube topology;
- attack range;
- sensor coverage;
- buildability.

This separation is canonical.

---

# PART CII — OVERLAY SYSTEM

Four manual strategic overlay classes:

# F9 — ENERGY DOMAINS

# F10 — FACTION INFRASTRUCTURE

Rock Raiders — Worksites.  
Astronauts — Forward Service.  
Aliens — Resonance/Core domain relationships.  
Martians — Aero Tube Network.

# F11 — RANGE / SENSOR COVERAGE

# F12 — BUILDABILITY / PATHING

Only one full strategic overlay is active by default at a time.

Contextual temporary previews may appear over it.

Build Mode automatically invokes local buildability information regardless of F12.

---

# PART CIII — BUILD GRID

The grid is visible:

- locally around a building ghost;
- during Tube Link construction where useful;
- during explicit footprint/range previews.

The normal world has no permanent chessboard/grid overlay.

At maximum, Build Mode displays approximately the local construction region around the cursor rather than the entire map.

---

# PART CIV — BASE MANAGEMENT

`Backspace` cycles owned command/expansion structures.

`Shift + Backspace` cycles backward.

F3 identifies idle production.

Clicking a brownout, depletion or critical-infrastructure event centers the associated location.

Global Production and Technology drawers accelerate management without replacing the world.

No empire spreadsheet exists.

---

# PART CV — SELECTING PRODUCTION BY TYPE

Generic selection rules solve production selection.

Methods:

- Ctrl-click a production structure → all owned structures of that type;
- select its type card in F3 Production Overview;
- double-click → visible same-type structures where direct clicking is appropriate.

No faction gets bespoke “select all factories” key combinations.

---

# PART CVI — MULTI-BASE ENERGY UX

Global HUD shows aggregate Energy only.

When Energy exists in several domains, an adjacent domain-count/status marker appears.

Example:

**Energy +5/s | 3 Domains | 1 Deficit**

Clicking opens domain detail.

Rock Raider disconnected Worksites, separate Alien Core domains and segmented Martian networks therefore remain diagnosable without filling the top bar with separate resource displays.

---

# PART CVII — MULTI-BASE RESOURCE UX

## ROCK RAIDERS

Top HUD displays total owned processed Ore.

If it is divided among disconnected Worksite pools:

**Pool Split** indicator appears.

When spending within a specific Worksite, the local accessible amount is emphasized.

Raw/unprocessed Ore is not counted as spendable.

## ASTRONAUTS

One standard global reserve.

## ALIENS

One standard Ore/uncommitted-Crystal reserve.

Charge is separate derived state, not resource ownership.

## MARTIANS

Connected settlement components pool spendable resources.

When segmented, HUD shows pool count.

Selecting a Station displays that component's accessible reserve.

This communicates local ownership differences without exposing internal packet simulation.

---

# PART CVIII — OPERATIONS CAPACITY UX

Canonical display:

# USED / MAX

At 85%:

advance-warning treatment appears once.

At maximum:

unit production requiring OC is disabled with:

**Operations Capacity full — increase operational infrastructure.**

If destruction reduces maximum below current use, display:

**OVER CAPACITY**

Existing units remain functional as established by Phase 04.

Hovering OC lists major structures contributing capacity.

The final icon must represent operations/logistics broadly, not simply a minifigure head.

---

# PART CIX — UNIT CREATION FEEDBACK

A completed unit:

1. physically emerges/assembles;
2. resolves production-exit clearance;
3. executes rally;
4. remains unselected unless the player explicitly selects it.

The camera never automatically jumps to a completed unit.

Routine workers/light units do not generate intrusive alerts.

Advanced/Massive first-of-type completions may create an Informational event.

Units are not automatically inserted into control groups.

---

# PART CX — COMMAND FEEDBACK IN WORLD

World command markers have limited lifetime.

Move:

~0.6s destination mark.

Attack:

~0.8s target bracket.

Attack-Move:

~0.8s distinct destination mark.

Rally:

persists while the producer is selected.

Patrol:

path persists while selected.

Excavation:

marker/progress persists during operation.

Tube Transfer:

route highlights while transfer is selected/active.

Surge:

zone persists for buildup + active duration.

---

# PART CXI — PATH PREVIEW

Normal Move does not draw a full path.

Path lines appear for:

- Shift-queued movement while Shift is held;
- Tube Link construction;
- Tube Transfer;
- large/Huge unit route failure;
- excavation-result preview;
- unusual air-blocking path.

This prevents constant navigation spaghetti.

---

# PART CXII — INVALID PATH FEEDBACK

A failed movement command gives immediate destination feedback plus reason.

Examples:

- No ground route;
- Airspace blocked;
- Unit too large for passage;
- Destination impassable;
- Heavy unit cannot use Aero Tubes;
- Tube network disconnected.

Units never silently ignore a fresh player command.

Repeated identical failures are rate-limited.

---

# PART CXIII — GROUP COMMAND RESOLUTION

One universal rule applies:

# ELIGIBLE MEMBERS EXECUTE; INELIGIBLE MEMBERS REMAIN SELECTED AND DO NOTHING DANGEROUS.

Exceptions for direct enemy Attack:

ineligible mobile support units may maintain the formation behind eligible attackers unless Hold is active.

Examples:

## ATTACK AIR TARGET

AA-capable units Attack.

Others support-move or Hold.

## LOAD INTO TRANSPORT

Eligible passengers Load.

Ineligible units stay.

## TUBE TRANSFER

Worker Robot / Double Hover / Jet Scooter transfer.

Heavy units remain.

One concise message reports the split.

---

# PART CXIV — COMMAND PRIORITY / INTERRUPTION

A non-Shift command replaces the unit's ordinary command queue.

Player orders may immediately interrupt:

- Move;
- Attack;
- Attack-Move;
- Patrol;
- ordinary Repair;
- Harvest.

Transition actions obey their existing cancellation windows.

Mission Refit obeys its explicit cancellation rules.

Tube transit cannot be interrupted after the unit has entered the network.

Movement/Attack-Move issued during transit becomes an arrival order and executes after Arrival Recovery.

---

# PART CXV — TRANSFORMATION CANCELLATION

During a transformation/deployment progress bar, the cancellable portion is visually separated from the committed portion.

Phase 06 thresholds remain authoritative:

- MX-41 / Alien Strike / Infiltrator — cancel before 40%;
- most deployment states — cancel before 50%.

Before threshold:

Stop or state-toggle may cancel.

After threshold:

the action finishes and a new command waits.

Rollback duration is displayed when Phase 06 defines one.

---

# PART CXVI — STATE ICON LANGUAGE

Canonical status families:

- Transforming / Deploying;
- Deployed;
- Repairing;
- Refitting;
- Configuration Locked;
- Surged;
- Stable;
- Transported / Transit;
- Disconnected;
- Brownout.

World-space priority:

1. dangerous system failure;
2. active transition;
3. major temporary combat state.

Lower-priority information moves to the selection panel.

---

# PART CXVII — STATUS EFFECT CLUTTER RULE

Baseline maximum:

# 2 WORLD-SPACE STATUS ICONS PER ENTITY

Health bar is separate.

When more states apply, only the two highest-priority states appear in world space.

The selection panel retains complete status information.

---

# PART CXVIII — UI SCREEN OCCUPANCY

At 1920×1080 baseline:

- top resource/status strip targets ≤5% screen height;
- bottom primary control region targets approximately 18–21% height;
- minimap targets approximately 19–20% of screen height;
- normal gameplay center remains unobstructed.

Temporary side drawers such as Production/Technology may occupy approximately 30–32% width but close immediately with Esc/F3/F4.

Normal combat never uses a permanent full-screen management layer.

---

# PART CXIX — HUD LAYOUT ARCHITECTURE

Canonical information anchors:

## TOP

Resources, OC, global faction mechanic, match state.

## BOTTOM LEFT

Minimap and alert access.

## BOTTOM CENTER

Selection/group/entity information.

## BOTTOM RIGHT

Command grid, queue, configuration interaction.

This arrangement is chosen for pointer travel and information hierarchy.

Phase 08 may change shapes, framing and ornament but not these functional relationships without a recorded UX revision.

---

# PART CXX — FACTION HUD VARIATION

The overall HUD skeleton is shared.

## ROCK RAIDERS

Contextual Worksite component/pool status.

## ASTRONAUTS

Service / Refit eligibility appears contextually when relevant rather than adding a permanent global meter.

## ALIENS

Permanent Charge/commitment module.

## MARTIANS

Compact network-component / segmentation status.

Faction identity is expressed through framing and mechanic content.

Basic control locations remain constant.

---

# PART CXXI — RESPONSIVE UI / RESOLUTION

Primary design resolution:

# 1920 × 1080 — 16:9

Required support:

- 1280×720;
- 16:10;
- 21:9;
- 1440p;
- 4K.

UI anchors remain at their functional screen edges.

Ultrawide layouts do not stretch information over the entire width; central interaction zones use a maximum logical width.

UI scaling is independent of render resolution.

---

# PART CXXII — SAFE AREA

Adjustable safe area:

# 90–100% OF AVAILABLE DISPLAY REGION

Default PC:

# 96%

Critical information never touches the absolute screen edge.

Ultrawide users retain reasonable cursor travel distances.

---

# PART CXXIII — SCREEN SHAKE & CAMERA FEEDBACK

Default shake intensity is restrained.

Large machinery impacts may create brief positional camera vibration.

Camera feedback:

- never rotates the player's strategic orientation;
- never changes zoom;
- never alters cursor world position;
- never changes issued commands.

Default shake strength target:

approximately **30% of the maximum authored effect**.

Accessibility may reduce it to zero.

---

# PART CXXIV — SELECTION FEEDBACK

Selected entities use:

- ground/anchor selection ring;
- readable outline;
- panel confirmation.

Current attack target uses a distinct bracket.

Hover and selection are visually different states.

True air uses elevated/air-layer treatment.

Selection effects remain visible over faction materials without recoloring the whole model.

---

# PART CXXV — TEAM / PLAYER IDENTIFICATION

Canonical faction palettes remain visible.

Player identity is added through:

- selection-ring accent;
- small faction-safe model markings;
- minimap/panel identity;
- health-bar/outline accents.

Mirror matches must remain readable.

Team-color accessibility mode may strengthen these markers.

It does not repaint the entire Rock Raider, Astronaut, Alien or Martian machine into arbitrary team colors.

---

# PART CXXVI — ENEMY UNIT INFORMATION

Visible enemy unit selection may show:

- name;
- role;
- HP;
- target class;
- Armor Rating;
- weapon target layer;
- range;
- currently visible transformation/deployment state.

It does not show:

- exact current weapon cooldown;
- control groups;
- hidden queued orders;
- production origin;
- private technology progress;
- resources;
- Alien Charge.

Enemy information disappears/turns to last-known state when fog rules require it.

---

# PART CXXVII — SPECTATOR / REPLAY UX BOUNDARY

Spectator and replay modes may later add:

- both players' resources;
- army value;
- production;
- research;
- global Charge;
- complete map vision;
- timeline.

Those features are not available to active players.

Player UX architecture must not be built around privileged spectator data.

---

# PART CXXVIII — TUTORIAL / ONBOARDING PRINCIPLES

The tutorial teaches transferable RTS fundamentals first:

- select;
- move;
- harvest;
- build;
- produce;
- attack;
- expand.

Faction mechanics then layer onto the same control language.

Teaching is:

- contextual;
- short;
- dismissible;
- demonstrative.

Campaign/tutorial missions may isolate one mechanic intentionally.

Normal matches are not halted by repeated modal tutorial popups.

---

# PART CXXIX — FACTION LEARNING CURVE

## ROCK RAIDERS

Teach:

- Worksite coverage;
- excavation;
- service.

Never teach manual hauling because it does not exist.

## ASTRONAUTS

Teach:

- Forward Service;
- Mission Refit;
- difference between Refit and tactical transformation.

## ALIENS

Teach:

- spendable vs committed Crystals;
- Charge;
- Surge;
- ETX reconfiguration.

## MARTIANS

Teach:

- Stations;
- Tube Links;
- transfer;
- segmentation;
- mechanical utility.

The underlying selection/camera/combat controls remain transferable.

---

# PART CXXX — NEW PLAYER VS EXPERT LAYER

## NEW PLAYER

Can play entirely through:

- visible command buttons;
- right-click context;
- alerts;
- straightforward Build/Production interfaces.

## EXPERT PLAYER

Can accelerate the same actions through:

- hotkeys;
- global same-type selection;
- control groups;
- queueing;
- Production/Technology drawers;
- minimap commands;
- camera bookmarks.

There is no separate “simplified competitive interface” with different rules.

---

# PART CXXXI — INPUT LATENCY / RESPONSIVENESS TARGET

## COMMAND ACKNOWLEDGEMENT

Local cursor/world feedback appears within:

# 1 RENDERED FRAME

after valid input wherever technically possible.

## PHYSICAL EXECUTION

The simulation obeys:

- turn rate;
- acceleration;
- deployment;
- transformation;
- loading;
- Tube travel.

A heavy Chrome Crusher can physically respond slowly while its destination marker appears immediately.

Networked simulation should consume a command on the next valid simulation step.

The player must never confuse intentional machinery inertia with input loss.

---

# PART CXXXII — COMPETITIVE FAIRNESS

The UI never grants information outside simulation visibility.

It may not reveal:

- hidden Tube segmentation;
- unseen excavation;
- unseen transformation;
- enemy banked resources;
- enemy Charge;
- unseen research;
- current enemy production queue.

Own-state information may be exact.

Enemy information requires observation.

Camera zoom/rotation limits are the same for all players.

---

# PART CXXXIII — SIX MATCHUP UX AUDIT

## RAIDERS VS ASTRONAUTS

Worksite coverage uses industrial-area overlay language.

Astronaut Forward Service uses service-node radius language.

Mission Refit uses module cards.

Excavation uses terrain interaction.

The systems do not visually collapse into one generic support radius.

## RAIDERS VS ALIENS

Alien Surge is visible through bounded resonance zones and the Raider player still sees Worksite disconnection as a separate infrastructure warning.

No giant Surge warning covers the battlefield.

## RAIDERS VS MARTIANS

Raider excavation changes shared map topology.

Martian Tubes connect Station nodes.

One manipulates terrain; the other manipulates network relationships.

Their overlays remain visually distinct.

## ASTRONAUTS VS ALIENS

Astronaut Service/Refit is local asset preparation.

Alien Charge/Surge is global-tempo investment.

Both remain legible simultaneously.

## ASTRONAUTS VS MARTIANS

True air, Forward Service and Tube topology use independent overlay categories and are not automatically shown together.

## ALIENS VS MARTIANS

Alien temporary power states use energetic state indicators.

Martian systems use mechanical/network indicators.

The player can distinguish tempo from connectivity without lore knowledge.

---

# PART CXXXIV — MIRROR UX AUDIT

## ROCK RAIDER MIRROR

Player identity must remain visible across similar Worksite machinery.

## ASTRONAUT MIRROR

Configuration state is distinct from player identification.

## ALIEN MIRROR

Overlapping Surge zones clearly belong to their owning player.

Enemy Charge totals remain private.

## MARTIAN MIRROR

Tube lines identify owner through pattern/accent and do not merge visually when crossing.

No allied/enemy network is mistaken for one shared component.

---

# PART CXXXV — CAMERA / UI READABILITY TEST CASES

## SMALL SKIRMISH — 5–8 UNITS

At normal combat zoom:

- individual units readable;
- health state clear;
- target feedback precise.

## NORMAL MID-GAME FIGHT — 15–25 UNITS

Group cards remain compact.

Role-aware formation and mixed targeting remain legible.

## LARGE LATE-GAME ENGAGEMENT — 30–45 VISIBLE UNITS

World-space status icon cap prevents icon clouds.

Type-group selection panel prevents portrait overload.

## BASE ASSAULT

Structures, siege ranges, defenses and repair states remain distinguishable.

## AIR / GROUND FIGHT

True-air anchors and AA legality remain readable without fake altitude controls.

## CAVERN

Occlusion system removes ceilings/foreground obstruction without revealing fog.

## MARTIAN NETWORK BATTLE

Selected-component Tube overlay identifies reinforcement geometry without showing every routing detail permanently.

## ALIEN SURGE ATTACK

Surge zone, timer and affected visible units remain obvious while normal attack markers and health bars remain readable.

These are mandatory prototype usability scenarios.

---

# PART CXXXVI — PERFORMANCE-AWARE UX

UX systems are event-driven wherever possible.

Examples:

- Tube overlay recalculates on topology change;
- Energy-domain list updates on domain state change;
- group cards aggregate unit state rather than drawing dozens of full widgets;
- hidden overlays are not continuously rendered.

Recommended prototype update cadence:

- selection/cursor feedback — frame-rate;
- minimap unit positions — approximately 10 Hz with visual interpolation;
- resource text — event-driven / low-frequency smoothing;
- topology overlays — change-driven.

No permanent whole-map high-cost diagnostic view exists.

---

# PART CXXXVII — IMPLEMENTATION DATA MODEL

## COMMAND DEFINITION

Each command requires data fields for:

- command ID;
- conceptual category;
- target filter;
- cursor;
- hotkey;
- queue policy;
- interruption policy;
- availability requirements;
- invalid-reason text;
- range preview;
- world marker;
- faction slot.

## SELECTABLE ENTITY UI DATA

Each selectable requires:

- display name;
- faction/player;
- role tags;
- HP;
- damage state;
- target/armor class;
- movement layer;
- weapons;
- attack layers/ranges;
- status states;
- production/research data;
- transport data;
- faction-mechanic data.

## ALERT DEFINITION

Each alert requires:

- priority;
- trigger;
- spatial sector;
- cooldown;
- minimap behavior;
- event-feed text;
- audio class.

## OVERLAY DEFINITION

Each overlay requires:

- ownership visibility;
- geometry source;
- auto-show triggers;
- manual toggle;
- stack policy;
- fog policy.

The UI should therefore be predominantly data-driven rather than coded uniquely for every asset.

---

# PART CXXXVIII — INPUT ACTION LIST

Canonical remappable action families:

**Selection:** select, marquee, additive, subtractive, same-type, select-under.

**Orders:** Move, Attack, Stop, Hold, Patrol, Spread, Repair, Load, Unload.

**Economy:** Build, Harvest/context.

**Faction:** State Change, Service/Configuration, Special Action.

**Camera:** pan, zoom, rotate, tilt, reset, center, bookmarks.

**Groups:** assign, add, remove, recall.

**Global management:** idle worker, all army, Production Overview, Technology Overview, overlays, cycle expansions.

**Communication:** ping, chat.

**System:** pause/menu, cancel.

---

# PART CXXXIX — COMMAND MATRIX

| Command | Target | Queue | Mixed-group behavior | Primary input |
|---|---|---|---|---|
| Move | Ground | Yes | All mobile legal units | Right-click / M |
| Attack | Enemy | Yes | Eligible attack; others support/Hold | Right-click enemy / A |
| Attack-Move | Ground | Yes | Combat-capable units; supports move | A + ground |
| Stop | None | No | Immediate | S |
| Hold | None | No | Immediate state | H |
| Patrol | Ground | Yes | Mobile eligible | P |
| Spread | None | No | Applies formation preference | V |
| Repair | Friendly damaged | Yes | Repairers only | R / context |
| Load | Transport | Conditional | Eligible passengers | L / context |
| Unload | Ground | Conditional | Selected transports | U |
| Build | Ground | Worker queue | Builders only | B |
| Excavate | Feature | Yes | Eligible Raider machinery | E/context |
| Mission Refit | Configuration | No | Eligible serviced assets | W |
| Transform/Deploy | State | Conditional | Relevant assets only | Q |
| Surge | Anchor | No | Global faction commitment | E/HUD |
| Shunt | Selected Node | No | Node only | E |
| Tube Link | Station | No | Station infrastructure | W |
| Tube Transfer | Station | Conditional | Eligible light Martians | E/context |
| Clamp | Unit | No | Searcher only | E |

---

# PART CXL — HUD INFORMATION MATRIX

| Information | Global HUD | Selection | World | Minimap | Overlay |
|---|---|---|---|---|---|
| Ore | Yes | Local pool where relevant | — | Resources | Component context |
| Energy | Yes | Local domain | Brownout icon | Alert | F9 |
| Crystals | Yes | Core/local use | Deposits | Deposits | — |
| OC | Yes | Producer cost | — | — | — |
| HP | — | Exact | Context bar | — | — |
| Attack range | — | Detail | On demand | — | F11 |
| Mission Refit | — | Primary | Transition | — | Service via F10 |
| Charge | Alien | Core detail | Surge | Surge | Resonance via F10 |
| Worksite | Compact warning | Detailed | Selected/disconnected | Nodes | F10 |
| Aero Tube | Compact warning | Station detail | Physical links | Network | F10 |
| Production | Summary alert | Queue | Building activity | — | F3 drawer |
| Research | Completion | Current | Building activity | — | F4 drawer |
| Fog | — | Last-known labels | Primary | Primary | Respects fog |

---

# PART CXLI — CAMERA BASELINE TABLE

| Parameter | Canonical prototype |
|---|---|
| Projection | Perspective |
| Vertical FOV | 36° |
| Default pitch | 58° downward |
| Tilt range | 52°–64° |
| Default yaw | 45° from north |
| Rotation | 90° increments |
| Close view width | ~24 cells |
| Normal combat width | ~44 cells |
| Far strategic width | ~72 cells |
| Keyboard pan close | 18 cells/s |
| Keyboard pan normal | 30 cells/s |
| Keyboard pan far | 46 cells/s |
| Edge-scroll speed | 80% keyboard pan |
| Pan acceleration | ~0.14s |
| Pan deceleration | ~0.10s |
| Camera smoothing | ~0.08s |
| Wheel zoom step | ~8% |
| Zoom smoothing | ~0.10s |
| Normal overscroll | ~8 cells |
| Minimap orientation | Permanently north-up |

Empirical usability tuning may adjust close/normal/far spans by approximately ±10% without reopening the constrained-perspective architecture.

---

# PART CXLII — DEFAULT HOTKEY TABLE

| Input | Action |
|---|---|
| Left Click | Select |
| Right Click | Context command |
| Left Drag | Marquee |
| Shift | Add selection / queue |
| Ctrl + Click | Global same type |
| Ctrl + Drag | Marquee including workers |
| Alt + Click | Select under |
| Alt + Drag | Subtract selection |
| Alt Hold | Inspect relevant ranges |
| Middle Drag | Camera pan |
| Wheel | Zoom |
| Alt + Wheel | Tilt |
| , / . | Rotate camera 90° |
| Home | Reset camera orientation |
| M | Move |
| A | Attack / Attack-Move |
| S | Stop |
| H | Hold |
| P | Patrol |
| R | Repair |
| V | Spread |
| L | Load |
| U | Unload |
| B | Build |
| Q | State Change |
| W | Service / Configuration / Network |
| E | Faction Special |
| C | Center current selection |
| G | Ping |
| Space | Next recent actionable alert |
| F1 | Idle worker |
| Shift+F1 | All idle workers |
| F2 | All army |
| F3 | Production Overview |
| F4 | Technology Overview |
| F5–F8 | Camera bookmarks |
| Ctrl+F5–F8 | Store bookmark |
| F9 | Energy Overlay |
| F10 | Faction Infrastructure Overlay |
| F11 | Range / Sensor Overlay |
| F12 | Buildability / Pathing Overlay |
| Backspace | Next command/expansion structure |
| Shift+Backspace | Previous command/expansion structure |
| 0–9 | Recall control group |
| Ctrl+0–9 | Assign group |
| Shift+0–9 | Add to group |
| Alt+0–9 | Remove from group |
| Esc | Cancel / Back / menu |

Every binding is fully remappable.

---

# PART CXLIII — FACTION UX REFERENCE CARDS

## ROCK RAIDERS

**PRIMARY HUD ADDITION**  
Worksite/pool segmentation warning.

**PRIMARY OVERLAY**  
Worksite service.

**PRIMARY UNIQUE INTERACTION**  
Excavation.

**SECONDARY UNIQUE INTERACTION**  
Industrial service/repair.

**COMMON COMBAT COMMAND BURDEN**  
Very low.

**MOST IMPORTANT ALERT**  
Worksite/service disconnection or important processing attack.

**MAIN CLUTTER RISK**  
Service circles and hauling visualization.

**UX NEVER DO THIS**  
Require selecting or manually routing automated hauling rigs.

---

## ASTRONAUTS

**PRIMARY HUD ADDITION**  
Contextual Forward Service/Refit eligibility.

**PRIMARY OVERLAY**  
Forward Service.

**PRIMARY UNIQUE INTERACTION**  
Mission Refit.

**SECONDARY UNIQUE INTERACTION**  
Tactical transformation/deployment.

**COMMON COMBAT COMMAND BURDEN**  
Low–moderate.

**MOST IMPORTANT ALERT**  
Forward Service loss during active configuration strategy.

**MAIN CLUTTER RISK**  
Confusing Refit with transformation.

**UX NEVER DO THIS**  
Use one identical “change mode” treatment for Refit, MX-41, Solar Explorer and MT-201.

---

## ALIENS

**PRIMARY HUD ADDITION**  
Charge / maximum / generation / committed Crystals.

**PRIMARY OVERLAY**  
Resonance/Surge coverage.

**PRIMARY UNIQUE INTERACTION**  
Crystal commitment + Surge.

**SECONDARY UNIQUE INTERACTION**  
ETX reconfiguration.

**COMMON COMBAT COMMAND BURDEN**  
Highest, but still bounded at 2–3 meaningful actions.

**MOST IMPORTANT ALERT**  
Resonance Core threatened/destroyed.

**MAIN CLUTTER RISK**  
Charge, commitment, Surge and transformation becoming separate meter spam.

**UX NEVER DO THIS**  
Present Charge as another harvested map resource.

---

## MARTIANS

**PRIMARY HUD ADDITION**  
Network component/segmentation status.

**PRIMARY OVERLAY**  
Aero Tube topology.

**PRIMARY UNIQUE INTERACTION**  
Tube Link / transfer.

**SECONDARY UNIQUE INTERACTION**  
Mechanical positional states/Clamp.

**COMMON COMBAT COMMAND BURDEN**  
Low–moderate.

**MOST IMPORTANT ALERT**  
Network segmentation.

**MAIN CLUTTER RISK**  
Permanent Tube-route and Stability icon spaghetti.

**UX NEVER DO THIS**  
Require passenger-by-passenger Tube routing.

---

# PART CXLIV — VISUAL-BIBLE HANDOFF

Phase 08 must visually differentiate the functional states established here.

Required visual families:

## INTERACTION

- selected;
- hovered;
- targeted;
- legal;
- invalid;
- queued.

## TARGET LAYER

- ground;
- hover;
- true air.

## HEALTH

- Healthy;
- Damaged;
- Heavily Damaged;
- Critical.

## COMMAND

- Move;
- Attack;
- Attack-Move;
- Repair;
- Harvest;
- Build;
- Load/Unload;
- Excavate.

## TEMPORARY STATE

- transformation;
- deployed;
- Mission Refit;
- Configuration Lock;
- Surge;
- Stability;
- Tube Transit;
- Arrival Recovery.

## INFRASTRUCTURE

- Worksite connected/disconnected;
- Forward Service;
- Energy Domain;
- brownout;
- Aero Tube connected/severed;
- Resonance operational/browned-out.

## INFORMATION HIERARCHY

Phase 08 must preserve distinctions between:

- global resource;
- faction derived resource;
- local state;
- warning;
- targeting preview;
- map overlay.

Exact:

- palette;
- cursor drawings;
- icon drawings;
- fonts;
- borders;
- materials;
- UI ornament;

remain Phase 08 responsibilities.

---

# PART CXLV — UX ANTI-PATTERNS

The project explicitly prohibits:

- different core RTS controls by faction;
- permanent giant range circles;
- permanent full Worksite or Tube overlays;
- ability bars filled because slots exist;
- manual Rock Raider hauling;
- Tube passenger-by-passenger micro;
- Mission Refit visually conflated with tactical transformation;
- Charge presented as a normal fourth resource;
- giant Stability countdowns over Martian armies;
- alerts for every minor damage event;
- modal confirmations during normal play;
- arbitrary free camera rotation;
- a toy-inspection close camera that is unusable for strategy;
- a satellite far camera that replaces units with icons;
- hidden dangerous right-click behavior;
- UI information leaking through fog;
- team identification that destroys faction palettes;
- generic military/sci-fi hologram UI divorced from LEGO Space identity;
- imitation of another RTS HUD merely for nostalgia;
- excessive studs, stickers or toy-box framing that reduces information density;
- permanent path lines for ordinary movement;
- automatic camera theft on unit/research completion;
- auto-activating strategic faction mechanics;
- auto-selecting every newly produced unit;
- exact enemy economic information without scouting;
- unreadable mixtures of several strategic overlays.

---

# PART CXLVI — IMPLEMENTATION FEASIBILITY AUDIT

The Phase 07 UX can be implemented through standard modern RTS architecture.

It relies on reusable systems:

- selection manager;
- context-action resolver;
- command definitions;
- target filters;
- command queues;
- control groups;
- production/research models;
- state-tag UI;
- map overlays;
- alert rules;
- fog-aware information providers.

Faction-specific UI is primarily data/configuration layered on common systems.

No faction requires:

- an entirely separate selection engine;
- bespoke mouse behavior;
- permanent full-map simulation queries;
- dozens of unique active ability widgets.

The most specialized implementations are:

- Worksite component visualization;
- Mission Refit configuration data;
- Alien Charge/Surge data;
- Aero Tube topology.

All four correspond to already-canonical gameplay systems and justify their implementation cost.

---

# PART CXLVII — COMPETITIVE UX AUDIT

The canonical UX confirms:

- every essential combat command has a hotkey;
- every faction mechanic has keyboard access;
- no important action depends solely on clicking a small UI icon;
- enemy information obeys fog;
- control groups are identical across factions;
- extreme camera settings cannot create unfair vision;
- minimap interaction supports expert play;
- alerts inform but do not automatically react;
- expert execution is faster through intended shortcuts;
- high APM improves responsiveness but routine mechanical labor is not artificially created.

No competitive UX conflict with Phase 01–06 remains.

---

# PART CXLVIII — SOURCE / LEGO UX AUDIT

LEGO identity should later appear through:

- mechanical selection feedback;
- physical construction;
- modular configuration imagery;
- vehicle-specific state diagrams;
- faction framing;
- charming but restrained acknowledgement.

The interface must not become:

- a physical LEGO instruction booklet during combat;
- a toy-store box design;
- plastic macro photography;
- childish sticker clutter.

The game remains a premium strategy interface built around LEGO machinery.

---

# PART CXLIX — FINAL UX FLOW EXAMPLES

## ROCK RAIDER EXCAVATION

1. Hover Scout discovers a known Excavatable Feature.
2. Player selects Drill Craft.
3. Hovering the feature shows Excavate cursor and 25/50 Energy class.
4. Player right-clicks or uses E.
5. Destination marker and resulting-opening preview appear.
6. Drill Craft moves into position.
7. Excavation progress appears locally.
8. Player may issue another order while cancellation remains legal.
9. Terrain opens.
10. Shared pathing updates and completion event appears.

## ASTRONAUT REFIT

1. Player selects T3-Trike.
2. Panel shows Escort and Forward Service Available.
3. W opens Mission Refit.
4. Survey configuration shows whether its module is owned.
5. Exact cost/time is previewed.
6. Player confirms.
7. Trike becomes unavailable during Refit.
8. Progress completes.
9. Survey state becomes active.
10. 20-second Configuration Lock appears in selected panel.

## ALIEN SURGE

1. Charge reaches at least 50.
2. Player activates Surge from HUD or eligible anchor.
3. Valid Cores/Mothership highlight.
4. Hover shows 12/10-cell zone.
5. Player confirms anchor.
6. 50 Charge is consumed.
7. 0.75s buildup plays.
8. 18-second timer starts.
9. Eligible units entering/leaving update automatically.
10. Zone ends without further input.

## MARTIAN TRANSFER

1. Player selects several Jet Scooters and Worker Robots near a Station.
2. E enters Tube Transfer.
3. Connected destinations highlight.
4. Player chooses destination.
5. Eligible units enter origin queue automatically.
6. Station shows two/three active channels.
7. Units travel.
8. They emerge at the destination.
9. Arrival Recovery shows locally for 0.75s.
10. Units execute any valid queued arrival order.

## STANDARD BASE ASSAULT

1. Player selects mixed army.
2. A + ground issues Attack-Move.
3. Formation moves in role-aware order.
4. Enemy Heavy appears.
5. Player right-clicks it for direct focus.
6. Eligible attackers engage; others support.
7. Siege unit reaches position.
8. Player deploys siege state where required.
9. Range preview confirms structure access.
10. Player later issues Move to retreat; cancellable states respond immediately and committed transitions finish first.

## NEW EXPANSION

1. Player selects worker.
2. B opens Economy category.
3. Command structure ghost appears.
4. Buildability, exit/footprint and local faction infrastructure previews appear.
5. Player confirms.
6. Resources reserve.
7. Worker constructs.
8. Completion event appears without moving camera.
9. Rally is assigned.
10. The faction's normal economy/network logic begins.

---

# PART CL — FINAL SELF-AUDIT

The Phase 07 specification has been checked against the requested interaction requirements.

Confirmed:

- every unit can be selected and commanded;
- every structure has an information/interaction treatment;
- universal commands have deterministic input;
- control groups are complete;
- queue behavior is classified;
- contextual right-click behavior is deterministic;
- camera values are prototype-ready;
- normal combat zoom accommodates Phase 06 ranges;
- Huge aircraft do not block ground selection;
- multi-production is supported;
- global production remains spatially grounded;
- research is readable;
- Energy reserve/generation/demand are diagnosable;
- brownout has direct explanation;
- OC is visible and warnable;
- Worksite operation requires no hauling micro;
- excavation is targetable/readable;
- Mission Refit is distinct from tactical transformation;
- Forward Service is readable;
- Crystal commitment and Charge remain one coherent mechanic;
- Surge execution requires one anchor decision rather than per-unit activation;
- Aero Tube topology is readable;
- Tube transfer supports groups;
- network segmentation is diagnosable quickly;
- Martian Stability avoids world-icon spam;
- transport language is shared;
- repair language is shared;
- air/ground legality is explicit;
- minimap supports all major faction interactions;
- fog never leaks private information;
- alerts are rate-limited;
- faction HUD variation preserves shared muscle memory;
- accessibility does not rely on color;
- no new unnecessary combat stance or faction system has been created;
- implementation can be data-driven.

---

# PART CLI — CANON ESTABLISHED BY PHASE 07

The following decisions are now authoritative.

1. **Mouse + keyboard PC is the canonical primary input platform.**

2. Controller support is deferred and may not weaken mouse/keyboard RTS controls.

3. Selection follows classic left-click / marquee fundamentals with:
   - Shift additive selection;
   - Ctrl global same-type selection;
   - Alt select-under/subtractive interaction.

4. Standard marquee selection prioritizes combat/support units over workers.

5. Buildings are excluded from normal army marquees.

6. Selection capacity baseline is **128 entities**.

7. There are **10 universal control groups**.

8. Control groups survive transformations, Refit, transport and Tube transit.

9. Normal right-click uses conservative deterministic context resolution.

10. Strategic commitments such as Refit, Surge and Crystal commitment never trigger accidentally through ordinary right-click.

11. Shift is the universal command queue modifier.

12. Move, Attack, Attack-Move, Patrol, Build, Harvest, Repair and Excavation are normally queueable.

13. Strategic state commitments are not queued.

14. The universal command panel contains a maximum **3×4 / 12 primary slots**.

15. The default hotkey system is **hybrid mnemonic + contextual positional grid**.

16. Q is the learned State Change position.

17. W is the learned Service/Configuration/Network position.

18. E is the learned faction Special Action position.

19. The canonical gameplay camera is constrained **perspective**.

20. Camera FOV baseline is **36°**.

21. Default pitch is **58° downward**, adjustable **52°–64°**.

22. Camera rotation is limited to **90° increments**.

23. Minimap remains permanently north-up.

24. Gameplay zoom bands are approximately:
   - 24-cell close;
   - 44-cell normal combat;
   - 72-cell strategic.

25. Far zoom never becomes a satellite/icon layer.

26. Foreground occlusion uses camera-aware fade/cutaway without violating fog.

27. Four camera bookmarks use F5–F8.

28. Health-bar behavior is contextual by default.

29. Phase 06 damage-state thresholds are preserved:
   - 70% Healthy boundary;
   - below 70% Damaged;
   - below 35% Heavily Damaged;
   - below 20% Critical.

30. Range overlays are contextual/on-demand rather than permanent.

31. Target legality communicates partial mixed-group eligibility rather than rejecting a legal group command.

32. Enemy/mobile last-known information obeys Phase 06/05 fog behavior.

33. The minimap differentiates ground mobile, true air, structures and strategic resources without icon soup.

34. Right-click minimap issues Move; Attack-Move requires Attack targeting mode.

35. Alerts have Critical / High / Normal / Informational priority.

36. Attack alerts are sector-based and rate-limited rather than triggered by every hit.

37. A lightweight strategic event feed exists.

38. Worksite UX uses service areas/components rather than logistics lines.

39. Rock Raider hauling remains automatic and non-selectable.

40. Excavatable Terrain exposes cost, approximate duration and known result without revealing fogged information.

41. Passive Raider Worksite maintenance requires no player command.

42. Mission Refit uses a dedicated configuration interface.

43. Only T3-Trike, Mobile Mining Platform and Modular Sentinel receive Mission Refit UI.

44. Mission Refit preserves all Phase 04 costs, timing and 20-second Configuration Lock.

45. Forward Service appears contextually and through the faction infrastructure overlay.

46. Tactical transformations/deployments use Q and remain visually/behaviorally distinct from Mission Refit.

47. Alien HUD permanently exposes:
   - Charge;
   - maximum Charge;
   - Charge generation;
   - committed Crystal count.

48. Alien Charge remains a derived mechanic, not a map resource.

49. Resonance Core commitment uses a desired-count interface to avoid repetitive per-Crystal clicking while preserving canonical transition times.

50. Surge can be initiated from the Alien HUD or a valid anchor.

51. Surge previews anchor radius before spending 50 Charge.

52. Surge automatically applies to eligible units entering/leaving its zone.

53. Defense Resonance Shunt remains a selected-Defense-Node emergency command.

54. Martian Aero Tube topology uses one dedicated network overlay.

55. Tube Link construction is Station-to-Station; bends are automatic.

56. Tube construction preview exposes:
   - route;
   - length;
   - Ore cost;
   - Energy;
   - connection slots.

57. Tube transfer supports group selection.

58. Only Worker Robot, Double Hover and Jet Scooter can Tube-transfer.

59. Station transfer throughput is displayed as two channels baseline / three with Hypersled Throughput.

60. Network segmentation generates one component-level alert rather than link-by-link spam.

61. Protector Stance, Searcher Brace and Excavation Clamp use the common state/special command grammar.

62. Martian Stability is primarily selected-panel information with only brief world feedback.

63. All faction transports use one shared Load/Unload interaction language.

64. Passenger lists are grouped and support selective unloading.

65. Repair uses one shared interaction language while respecting faction-specific service rules.

66. Spread is the game's only optional universal formation-state control.

67. True air and ground-layer hover receive distinct UI treatment.

68. There is no altitude control.

69. Worker UX does not use universal saturation counts.

70. Resource deposits expose exact current amount while visible and last-seen amount in fog.

71. Active economic deposits warn once at approximately 15% remaining.

72. Rock Monsters use neutral-hostile UX and do not receive faction UI.

73. Standard ranked play has no tactical pause and fixed 1.0× speed.

74. Ping uses a dedicated G targeting mode.

75. Essential information never depends only on color or audio.

76. Normal competitive actions do not use modal confirmation dialogs.

77. Global and local information are explicitly separated.

78. Strategic overlays are:
   - F9 Energy;
   - F10 Faction Infrastructure;
   - F11 Range/Sensor;
   - F12 Buildability/Pathing.

79. The grid appears locally during relevant construction/interaction rather than permanently.

80. F3 provides spatially grounded global Production Overview.

81. F4 provides Technology Overview.

82. Backspace cycles command/expansion structures.

83. Energy Domain problems are summarized globally and diagnosed locally.

84. Rock Raider/Martian split-resource states receive pool indicators.

85. Operations Capacity retains the 85% warning threshold and 100-cap architecture.

86. Unit completion never steals the camera.

87. Ordinary paths are not permanently drawn.

88. Invalid path/action reasons are explicit.

89. Mixed-group commands follow one universal eligible-subset rule.

90. Transition cancellation thresholds are shown rather than hidden.

91. World-space status indicators are capped at **two per entity**.

92. The same functional HUD anchors apply to every faction.

93. Faction identity changes HUD content and later art, not fundamental muscle memory.

94. Input acknowledgment is immediate even when physical machinery execution is deliberately slow.

95. Active-player UI never exposes spectator-only information.

96. Tutorial flow teaches shared RTS language before faction mechanics.

97. Prototype UX is implementable through reusable data-driven command/state/overlay systems.

# ALL 35 BUILDABLE UNITS HAVE COMPLETE PLAYER-INTERACTION COVERAGE.

# ALL 31 BUILDING / INFRASTRUCTURE ENTRIES HAVE COMPLETE PLAYER-INTERACTION COVERAGE.

# ALL PHASE 06 ACTIVE COMMANDS AND STATES ARE REPRESENTABLE THROUGH THE INTERFACE.

# CANON SET COVERAGE REMAINS 64 / 64.

---

# EMPIRICAL USABILITY TESTING

No normal interaction-design decision remains unresolved.

Prototype testing should measure:

- whether 44-cell normal combat view should shift within approximately ±10%;
- whether 24-cell closest zoom preserves enough tactical context;
- whether 72-cell far zoom remains sufficiently readable;
- whether 58° default pitch produces optimal silhouette separation;
- whether 90° rotation remains useful without disorientation;
- whether 0.30s / 0.90s tooltip timings are comfortable;
- whether the 12-second attack-alert sector cooldown needs modest adjustment;
- whether two simultaneous world status icons are sufficient in the largest Martian/Alien engagements;
- whether the F9–F12 overlay arrangement produces fast enough expert access;
- whether 3×4 command-panel density remains readable at 1280×720;
- whether large/Huge air Select-Under behavior is reliable;
- whether multi-production shortest-queue distribution produces expected facility use;
- whether Mission Refit multi-selection communicates partial eligibility clearly;
- whether Tube destination selection remains fast in five-plus-Station networks;
- whether Surge zone boundaries remain readable during 30–45-unit battles.

These tests may tune prototype values.

They do not reopen the interaction architecture by default.

---

# PREVIOUS CANON CHANGED BY PHASE 07

# NONE.

Phase 07 does not change:

- the 35-unit roster;
- the 31-entry infrastructure roster;
- Phase 04 economy;
- Operations Capacity;
- Energy Domains;
- Worksite architecture;
- Mission Refit economics;
- Alien Charge economics;
- Aero Tube economics;
- Phase 05 topology/fog;
- Phase 06 combat statistics;
- transformation timing;
- repair throughput;
- Surge effect;
- Martian Stability;
- Tube Arrival Recovery.

---

# PART CLII — PHASE 08 REQUIREMENTS

The next phase is:

# VISUAL BIBLE & UI ART DIRECTION

Phase 08 inherits the complete functional UX established here.

It must visually design the game knowing:

- perspective camera;
- 58° default pitch;
- 24 / 44 / 72-cell zoom bands;
- discrete camera rotation;
- selection/target feedback requirements;
- four health-state thresholds;
- true-air/ground distinctions;
- legal/invalid cursor states;
- range overlays;
- Worksite overlay;
- Forward Service overlay;
- Resonance/Surge state;
- Aero Tube topology;
- minimap symbol categories;
- four alert priorities;
- top resource hierarchy;
- Alien Charge hierarchy;
- 3×4 command grid;
- grouped-selection cards;
- global/local state division;
- world-state icon cap;
- color-independent information requirements;
- faction HUD variation constraints;
- screen-space limits;
- accessibility requirements.

Phase 08 may define:

- final UI palette;
- faction framing;
- typography;
- cursor artwork;
- icon artwork;
- HUD materials;
- visual effects;
- selection-ring styling;
- minimap visual styling;
- damage presentation;
- range-overlay styling;
- network-overlay styling;
- alert animation;
- LEGO material language.

Phase 08 may **not** change the functional interaction model merely for visual novelty.

---

# PHASE 07 — CONTROLS, CAMERA, UX & INTERFACE IS COMPLETE AND LOCKED.
