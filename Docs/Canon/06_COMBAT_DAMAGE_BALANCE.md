# LEGO SPACE RTS — COMBAT, DAMAGE & BALANCE FRAMEWORK v1.0

**Phase:** 06 — Combat, Damage & Balance Framework  
**Status:** AUTHORITATIVE PROJECT CANON  
**Authority:** Project Instructions + Phase 00 Canon Set Registry + Phase 01 Game Bible Foundation + Phase 02 Faction Bible & Asymmetry + Phase 03 Unit & Building Roster + Phase 04 Economy, Technology & Progression + Phase 05 World & Map Bible

---

# CANON STATUS

Phase 06 converts the complete Phase 03 roster into a prototype-ready deterministic combat simulation.

The inherited playable roster remains exactly:

- Rock Raiders — **8 units / 8 buildings**
- Astronauts — **13 units / 8 buildings**
- Aliens — **6 units / 6 buildings**
- Martians — **8 units / 9 buildings/infrastructure**
- total — **35 buildable units**
- total — **31 building/infrastructure entries**.

Phase 04 explicitly reserved final HP, armor, damage, range, projectile behavior, movement speed, combat repair, counter ratios, and related numerical combat values for this phase.

Phase 05 establishes the map geometry this combat system must fit, including approximately 160 × 160-cell standard 1v1 maps, 105–120-cell start separation, 10–15-cell medium passages, 16–24-cell broad lanes, 25+ cell open fields, and 12-cell minimum major heavy routes.

No Phase 03 roster amendment is made.

No Phase 04 unit cost, OC, build-time, or building-cost amendment is required.

**CANON SET COVERAGE REMAINS 64 / 64.**

---

# VALUE-STATUS LANGUAGE

Two classifications are used throughout this document.

### LOCKED SYSTEM VALUE — LSV

A rule or relationship that should not normally change without redesigning the combat system.

Examples:

- deterministic attacks;
- armor architecture;
- target classes;
- damage-type families;
- no conventional stealth layer;
- Surge affecting cadence rather than damage;
- Martian displacement Stability;
- transport crash-survival rule.

### BASELINE TUNING VALUE — BTV

A concrete prototype number that is canonical for implementation now but expected to receive empirical tuning.

Examples:

- 360 HP;
- 4.8-cell range;
- 1.50 cells/second;
- 22 damage;
- 1.15-second cooldown.

No BTV is left blank.

---

# PART I — GLOBAL COMBAT MODEL

## SIMULATION PHILOSOPHY — LSV

Combat is:

# DETERMINISTIC, TARGET-RESOLVED, READABLE MACHINERY WARFARE

Normal competitive attacks have **no random miss chance**.

There is no universal accuracy statistic.

There is no passive evasion percentage.

There are no random critical hits.

A player who repeats the same combat interaction under the same conditions should receive substantially the same result.

This preserves the Phase 01 requirement that positioning, composition, focus, reinforcement, scouting, and movement—not invisible probability—define combat.

## ATTACK SEQUENCE

1. Unit acquires a legal target.
2. Range and line of sight are checked.
3. Weapon begins its firing event.
4. Damage is committed according to the weapon's behavior:
   - immediate for beam/contact resolution;
   - on projectile impact for projectile weapons.
5. Damage type modifies base damage against target class.
6. Armor Rating modifies the result.
7. HP is reduced.
8. Destruction begins at 0 HP.

## PROJECTILE COLLISION

Ordinary projectiles do **not** use expensive free-flight physics against every LEGO object.

They do not accidentally collide with:

- allied units;
- unrelated enemy units;
- decorative LEGO debris.

Only deliberately defined:

- splash;
- terrain obstruction;
- target-loss behavior;

affects their result.

## LINE OF FIRE

Ground ranged weapons require visible combat line of sight when firing.

Major:

- cliffs;
- solid map walls;
- designated occluding structures;

may block line of sight.

Ordinary units do not act as ballistic walls.

## UNIT DEATH

At 0 HP:

- unit commands cease;
- weapon events not already committed are cancelled;
- destruction animation begins;
- movement collision is removed in the same authoritative simulation tick;
- the remaining wreck/debris is nonblocking under Part XXXIX.

## STRUCTURE DESTRUCTION

At 0 HP:

- production/repair/research/defense immediately stops;
- Energy or network function is removed;
- collapse begins;
- movement collision is removed in the same authoritative simulation tick;
- rubble and remaining collapse visuals are nonblocking;
- structure is then removed from active gameplay.

There is no hidden random survival roll.

---

# PART II — UNIT DURABILITY ARCHITECTURE

The combat model uses:

# HP + TARGET CLASS + ARMOR RATING

There are no shields as a universal second health bar.

Alien energy technology does not receive a generic faction shield solely because it is energetic.

## TARGET CLASSES — LSV

| Class | Typical role | Typical HP band | Visual expectation |
|---|---|---:|---|
| **Personnel** | Minifigure-scale crew | 90–130 | Clearly vulnerable personnel |
| **Light Machine** | Scout, light hover, small aircraft | 110–280 | Small, responsive machinery |
| **Medium Machine** | Skirmisher, medium craft | 250–450 | Clearly combat-capable but mobile |
| **Heavy Machine** | Frontline, support platform, heavy drill | 400–800 | Substantial chassis / walker |
| **Massive Machine** | Flagship vehicle, major aircraft | 650–1,300 | Large strategic investment |
| **Structure** | Normal infrastructure | 800–2,000 | Functional installation |
| **Fortified Structure** | HQ, major station, static defense | 700–3,000 | Reinforced strategic position |

True air is a **movement and targeting layer**, not an armor class.

---

# PART III — DAMAGE / WEAPON ARCHITECTURE

Six functional damage types are used.

## DAMAGE TYPES — LSV

**LIGHT**  
Fast anti-personnel/anti-light attack.

**GENERAL**  
Broadly useful conventional damage.

**BREACH**  
Drills, heavy cutters and concentrated anti-machine tools.

**SIEGE**  
Specialized structure destruction.

**ANTI-AIR**  
Weapons whose normal competitive target set is true-air units.

**CONTROL**  
Low-damage mechanical displacement/deflection effects.

## DAMAGE MATRIX — BTV

| Damage type | Personnel | Light | Medium | Heavy | Massive | Structure | Fortified |
|---|---:|---:|---:|---:|---:|---:|---:|
| **Light** | 1.25× | 1.30× | 0.90× | 0.70× | 0.60× | 0.60× | 0.50× |
| **General** | 1.10× | 1.05× | 1.00× | 0.90× | 0.80× | 0.80× | 0.70× |
| **Breach** | 0.75× | 0.85× | 1.10× | 1.55× | 1.45× | 1.10× | 1.00× |
| **Siege** | 0.55× | 0.65× | 0.80× | 1.00× | 1.15× | 1.60× | 1.75× |
| **Anti-Air** | 0.90× | 1.40× | 1.25× | 1.10× | 0.95× | 0.30× | 0.25× |
| **Control** | 0.90× | 1.00× | 0.85× | 0.70× | 0.50× | 0.35× | 0.25× |

Anti-Air weapons normally cannot target structures despite the implementation matrix containing fallback coefficients.

## DAMAGE FORMULA — LSV

**Final Damage = Base Damage × Damage-Type Modifier × Armor Multiplier**

Armor multiplier is defined in Part VI.

Modifiers deliberately create approximately 1.4–1.8× specialist advantages rather than absolute locks.

---

# PART IV — TIME-TO-KILL PHILOSOPHY

## TARGET BANDS — BTV

| Interaction | Healthy approximate TTK |
|---|---:|
| Scout vs scout | 12–18s |
| Light vs light | 10–16s |
| Single harassment unit vs worker | 7–11s |
| Specialist vs intended target | 8–14s at comparable investment |
| Generalist vs equal-cost target | 14–21s |
| Heavy vs heavy | 18–28s |
| 4–6-unit focus vs medium | 4–8s |
| 4–6-unit focus vs heavy | 7–13s |
| Correct static defense vs lone raider | 8–14s |
| One protected dedicated siege unit vs normal structure | 25–55s |
| Two dedicated siege units vs normal structure | 14–30s |
| Ordinary early force vs command structure | 60–110s |

Combat therefore permits:

- reaction;
- retreat;
- reinforcement;
- target switching;
- support;

without creating giant damage sponges.

## RANGE / MOVEMENT / TTK RELATIONSHIP

Long-range units pay through at least one of:

- lower durability;
- lower efficiency against the wrong class;
- setup;
- minimum range;
- reduced mobility.

Fast units pay a mobility premium in:

- durability;
- raw DPS;
- specialization;
- resource efficiency.

Focus fire can shorten TTK drastically, but projectile commitment, screening and overkill create meaningful costs for excessive target concentration.

---

# PART V — HEALTH SCALE

Prototype anchors:

| Reference | HP |
|---|---:|
| Worker | ~110 |
| Light scout | 120–150 |
| Standard light combat machine | 180–320 |
| Medium frontline | 360–480 |
| Heavy combat vehicle | 520–760 |
| Advanced massive machine | 760–1,200 |
| Normal production building | 1,350–1,900 |
| Command structure | 2,400–3,000 |

Every final unit/building value appears later.

---

# PART VI — ARMOR

## ARMOR RATING — LSV

Armor does **not** subtract flat damage.

Every Armor Rating point reduces incoming post-class damage by:

# 4%

| Armor | Multiplier |
|---:|---:|
| 0 | 1.00 |
| 1 | 0.96 |
| 2 | 0.92 |
| 3 | 0.88 |
| 4 | 0.84 |
| 5 | 0.80 |

This permits rapid weapons to remain useful while preserving clear chassis differences.

Maximum normal competitive Armor Rating is **5**.

Damage cannot be reduced below 1 by armor alone.

---

# PART VII — ATTACK RANGE SCALE

Ranges are measured in Phase 05 build-grid cells.

| Category | Range |
|---|---:|
| **Contact** | 0.7–1.2 |
| **Short** | 2.5–3.5 |
| **Medium** | 4.0–5.5 |
| **Long** | 6.0–7.5 |
| **Siege** | 8.5–10.0 |
| **Extreme information** | 11–16 sight/detection |

This makes 10–15-cell medium passages large enough for genuine line formation while preserving the special value of long and siege weapons.

Static defense does not normally control an entire 34 × 34 expansion footprint.

---

# PART VIII — MOVEMENT SPEED SCALE

Speed is measured in build-grid cells per second.

| Band | Speed |
|---|---:|
| Very slow heavy | 0.90–1.05 |
| Heavy | 1.05–1.30 |
| Worker | 1.30–1.45 |
| Medium | 1.35–1.65 |
| Fast | 1.70–2.05 |
| Scout / fast hover | 2.10–2.40 |
| Light true air | 2.45–3.15 |
| Large true air | 1.65–2.15 |

A 110-cell route therefore takes:

- 122s at 0.90;
- 88s at 1.25;
- 73s at 1.50;
- 49s at 2.25.

Normal combined ground forces cluster around the Phase 05 80–100-second representative cross-map target.

---

# PART IX — ACCELERATION & TURNING

## ACCELERATION CLASSES — BTV

| Class | Acceleration |
|---|---:|
| Snap | 6.0 cells/s² |
| Quick | 4.0 |
| Standard | 2.5 |
| Heavy | 1.5 |
| Massive | 0.9 |

Deceleration is **1.25× acceleration**.

Pathing begins deceleration automatically before the destination so heavy units feel massive without routinely overshooting commands.

## TURN CLASSES — BTV

| Class | Turn rate |
|---|---:|
| Agile | 180°/s |
| Quick | 135°/s |
| Standard | 95°/s |
| Heavy | 65°/s |
| Massive | 45°/s |

## REVERSE SPEED

- foot / walker / hover: **80%**
- tracked/wheeled: **55%**
- massive wheeled: **45%**
- true-air craft: no reverse; they decelerate and turn.

Turreted systems may maintain target orientation independently where explicitly stated.

Vehicle handling is tactical character, not a driving simulator.

---

# PART X — UNIT FOOTPRINT & COLLISION

| Footprint | Collision radius |
|---|---:|
| Tiny | 0.35 cells |
| Small | 0.55 |
| Medium | 0.85 |
| Large | 1.15 |
| Huge | 1.60 |

## COLLISION RULES — LSV

- Friendly units never permanently occupy the same space.
- Friendly units may compress their separation by up to **15% for 1.5s** during path exchange.
- Large units receive path-reservation priority over small friendly units.
- Small friendly units automatically sidestep for a reserved heavy path.
- Enemy units never phase through one another.
- Enemy units cannot be physically pushed merely because another unit is larger unless an explicit displacement mechanic applies.
- Transport unloading reserves legal placement space.

These rules allow Huge units to operate on Phase 05's ≥12-cell heavy routes without pathing deadlock.

---

# PART XI — FORMATIONS

## DEFAULT FORMATION — LSV

One ordinary move order automatically creates a **role-aware loose battle formation**.

Order from front to rear:

1. short-range/heavy frontline;
2. medium ranged combat;
3. anti-air intermixed with the main body;
4. support;
5. undeployed siege.

Air assets form a separate overhead echelon.

## SPACING

Normal formation spacing uses approximately:

**collision diameter + 0.35 cells**

between centers.

Large splash-sensitive formations automatically avoid excessive stacking.

## GROUP SPEED

During ordinary formation movement the group travels at approximately **90% of the slowest selected combat unit's maximum speed** until formation cohesion is no longer practical.

Fast scouts ordered separately retain full speed.

## OPTIONAL FORMATION COMMAND

Only one extra universal formation control is required:

**SPREAD**

It increases formation separation by approximately 40%.

It is useful against splash and hazards but reduces corridor efficiency.

Formation switching is optional, not constant required micro.

---

# PART XII — ATTACK ORDERS & TARGET ACQUISITION

## ATTACK

Direct attack on a legal target.

Player-issued target priority overrides automatic priorities.

Normal direct-target chase leash:

**12 cells** from the point at which pursuit began.

Dedicated air interceptors:

**16 cells.**

## ATTACK-MOVE

Units advance toward the ordered location.

Acquisition radius is:

**minimum of sight radius and weapon range + 4 cells.**

A target may pull an attack-moving unit no more than **7 cells** from its intended route before the unit returns.

## FOCUS TARGET

Not a separate ability.

It is a direct Attack order issued by a selected group.

Multiple units may freely attack one target.

## STOP

Cancels:

- current attack;
- chase;
- queued movement.

## HOLD POSITION

Units may:

- rotate;
- fire;
- acquire targets.

They do not intentionally move more than **0.75 cells** from their held position.

## PATROL

Uses attack-move acquisition logic with an **8-cell patrol-segment leash**.

---

# PART XIII — TARGET PRIORITY

Default role priorities:

| Role | Priority |
|---|---|
| Anti-light | Personnel → Light → nearest threat |
| Anti-heavy | Heavy → Massive → Medium |
| Anti-air | True air threatening combat/support → transport |
| Siege | Defensive structure → production → economic infrastructure → command |
| Harassment | Workers → exposed economic infrastructure → light support |
| Generalist | Closest legal attacker → player-marked target |
| Scout | Self-defense only; does not abandon scouting route aggressively |
| Support | Avoids frontline; attacks only when explicitly armed and safe |
| Control | Eligible light/medium unit whose displacement changes local engagement |

Direct player orders always override default priorities if the target is legal.

---

# PART XIV — PROJECTILE MODEL

## STANDARD PROJECTILE SPEEDS — BTV

| Type | Speed |
|---|---:|
| Slow siege | 5–6 cells/s |
| Mechanical/normal | 8–10 |
| Fast | 12 |
| Guided AA | 10–12 |
| Beam/contact | immediate |

## TARGET LOSS

If the target is destroyed before impact:

**guided projectile:** dissipates; does not retarget.

**ordinary projectile:** continues to the last committed impact position.

**splash projectile:** still explodes at that position.

**beam/contact:** damage was already resolved.

No projectile automatically searches for a new target after launch.

## SLOW SIEGE

ETX siege bolts target an impact position rather than perfectly following fast moving units.

This makes their low projectile speed tactically relevant while remaining deterministic.

---

# PART XV — FOCUS FIRE & OVERKILL

Focus fire matters because removing one dangerous unit early changes an engagement.

It is restrained by:

- projectile travel time;
- large target screening;
- approach-slot limits on contact units;
- formation frontage;
- target accessibility;
- genuine overkill;
- no projectile retarget after target death.

There is no artificial cap on attackers.

The player is rewarded for focus, but repeatedly sending 12 projectiles into a target needing one final hit wastes real attack cycles.

---

# PART XVI — SPLASH & AREA DAMAGE

No normal friendly fire.

## SPLASH PROFILES

### SMALL — 0.75 cells

- center 0–0.30: 100%
- outer: 50%

### MEDIUM — 1.25 cells

- 0–0.40: 100%
- 0.40–0.80: 60%
- 0.80–1.25: 35%

### LARGE — 1.75 cells

- 0–0.50: 100%
- 0.50–1.10: 60%
- outer: 30%

Normal weapons rarely exceed Medium splash.

Ground splash does not hit true air.

Air splash does not hit ground unless explicitly defined.

This makes clumping punishable without making 10–15-cell passages unusable.

---

# PART XVII — CONTACT / DRILL COMBAT

## APPROACH SLOTS

Contact targets expose a ring of legal engagement positions.

Friendly contact attackers reserve different approach slots rather than all trying to occupy the same point.

## CONTACT CONDITIONS

Typical requirements:

- range ≤1.0 cell;
- forward heavy drill facing within 30°;
- other contact tools within 45°.

## MOVING CONTACT

Contact machines may maintain engagement while moving at up to **35% maximum speed**.

They cannot deal full drill damage while driving normally through a target.

## TARGET ESCAPE

Contact creates no artificial movement lock.

The target may:

- reverse;
- turn;
- retreat;
- be transported;
- be displaced.

## DRILL RAMP

Granite Grinder gains its sustained-drill bonus after maintaining the same target for **2.5 seconds**.

Moving outside contact, switching targets, or losing facing removes the ramp immediately.

---

# PART XVIII — SIEGE SYSTEM

The five important siege relationships remain deliberately different.

| Unit | Siege relationship |
|---|---|
| **Granite Grinder** | Mobile anti-heavy contact specialist with secondary structure breach; ramps when allowed to stay attached |
| **Chrome Crusher** | Very durable advanced direct breacher; highest Raider structure pressure; must physically reach target |
| **MT-101** | Mobile anti-heavy heavy assault; modest secondary structure function; no siege deployment |
| **MT-201** | Primary Astronaut siege; physically anchors at the structure and applies enormous contact drill pressure |
| **ETX Alien Strike** | Highly mobile air relocation → vulnerable long-range deployed siege |
| **Excavation Searcher** | Medium-range articulated positional siege combined with mechanical manipulation |

Siege units require protection.

None is a generic long-range cannon wearing different faction colors.

---

# PART XIX — STATIC DEFENSE

All costs remain Phase 04 values.

## STATIC DEFENSE TABLE

| Defense | Cost O/E/C | HP | Class / Armor | Target | Weapon | Dmg / Type | CD | DPS | Range | Behavior | Energy |
|---|---|---:|---|---|---|---|---:|---:|---:|---|---|
| **Crusher Barrier** | 90/10/0 | 1,100 | Fortified / A4 | Ground | Hydraulic Batter | 20 Light | 1.20 | 16.67 | 1.25 | Mechanical contact; also path obstacle | No continuous draw |
| **Cutter Mast** | 120/25/0 | 800 | Fortified / A2 | Air | Precision Cutter Beam | 24 AA | 1.00 | 24.00 | 7.50 | Beam; 120°/s tracking | 2 E/s |
| **Sentinel — Ground** | 110/20/0 | 800 | Fortified / A2 | Ground | Interdiction Burst | 28 General | 1.20 | 23.33 | 6.00 | Fast projectile | 1 E/s |
| **Sentinel — Air** | same chassis | 800 | Fortified / A2 | Air | Air Interceptor | 24 AA | 0.90 | 26.67 | 7.50 | Guided | 1 E/s |
| **ETX Node — Ground** | 120/30/0 | 700 | Fortified / A1 | Ground | Ground Pulse | 26 General | 1.10 | 23.64 | 5.50 | Energy projectile | 2 E/s |
| **ETX Node — Air** | same chassis | 700 | Fortified / A1 | Air | Air Lance | 24 AA | 0.90 | 26.67 | 7.00 | Beam | 2 E/s |
| **Deflector Arm** | 100/15/0 | 850 | Fortified / A3 | Ground | Deflection Pulse | 14 Control | 8.00 | 1.75 | 4.50 | Displacement, not normal damage defense | 1 E/s |
| **Aero Guard Tower** | 120/25/0 | 760 | Fortified / A2 | Air | Aero Tracker | 22 AA | 0.90 | 24.44 | 7.25 | Guided | 2 E/s |

Powered defenses cease firing during brownout under Phase 04 rules.

Crusher Barrier remains mechanically functional because it has no continuous Energy demand.

Static defenses are locally efficient but immobile and economically punishable through map-control loss.

---

# PART XX — STRUCTURE DURABILITY

## COMBAT FOOTPRINT CLASSES

These are combat/pathing footprint baselines for implementation.

| Structure footprint | Approximate grid relationship |
|---|---|
| Compact | 2×2–4×4 |
| Standard | ~5×5–6×6 |
| Large | ~7×6–8×8 |
| Major | ~8×8–9×9 |
| Extended Pad | ~10×8 |
| Link | 1-cell-wide route |

## COMPLETE STRUCTURE DURABILITY TABLE

| Faction | Structure | Footprint | HP | Class | Armor | Harassment priority | Destruction consequence | Siege vulnerability |
|---|---|---|---:|---|---:|---|---|---|
| RR | Rock Raiders HQ | Major 8×8 | 3,000 | Fortified | 5 | Medium | Loses command/Worksite node/OC | Normal Fortified |
| RR | Ore Processing Plant | Standard 6×6 | 1,350 | Structure | 2 | High | Mature Ore processing disrupted | High |
| RR | Power Station | Standard 5×5 | 1,000 | Structure | 1 | Very high | Removes 10 E/s and reserve support | High |
| RR | Vehicle Service Bay | Large 8×6 | 1,700 | Structure | 3 | High | Production + Worksite servicing lost | Medium |
| RR | Engineering Workshop | Major 8×8 | 1,900 | Structure | 3 | High | Heavy production/tech access lost | Medium |
| RR | Crystal Vault | Standard 5×5 | 1,600 | Fortified | 4 | High | Advanced crystal handling exposed | Medium |
| RR | Crusher Barrier | Compact 3×1 | 1,100 | Fortified | 4 | Tactical | Ground blocking removed | Medium |
| RR | Cutter Mast | Compact 2×2 | 800 | Fortified | 2 | Tactical | AA zone removed | High |
| AST | MB-01 Eagle Command Base | Major 8×8 | 2,700 | Fortified | 4 | Medium | Command/expansion/OC lost | Normal |
| AST | Field Systems Garage | Large 7×6 | 1,450 | Structure | 2 | High | Field production lost | High |
| AST | Mission Vehicle Bay | Large 8×7 | 1,700 | Structure | 3 | High | Mission ground production lost | Medium |
| AST | Flight Operations Pad | Extended 10×8 | 1,400 | Structure | 2 | Very high vs air build | Air production lost | High |
| AST | Service & Refit Hub | Large 7×7 | 1,650 | Structure | 3 | Very high | Repair/refit/network access lost | Medium |
| AST | Solar Energy Array | Standard 6×5 | 900 | Structure | 1 | Very high | 8 E/s generation lost | High |
| AST | Frontier Extraction Station | Standard 6×6 | 1,250 | Structure | 2 | High | Worker routes/processing disrupted | High |
| AST | Modular Sentinel Defense | Compact 2×2 | 800 | Fortified | 2 | Tactical | Configured defense removed | High |
| ALI | ETX Command Core | Large 7×7 | 2,400 | Fortified | 3 | Medium | Command/foothold/OC lost | Normal |
| ALI | Resonance Core | Compact 4×4 | 1,200 | Structure | 2 | **Very high** | Charge capacity/generation recalculated; salvage rules trigger | High |
| ALI | ETX Fabricator | Standard 6×6 | 1,350 | Structure | 2 | High | Light production lost | High |
| ALI | Reconfiguration Dock | Large 8×7 | 1,600 | Structure | 2 | High | Advanced ETX production/service lost | High |
| ALI | Power Coupler | Compact 4×4 | 850 | Structure | 1 | **Very high** | 12 E/s generation lost | Very high |
| ALI | ETX Defense Node | Compact 2×2 | 700 | Fortified | 1 | Tactical | Defense coverage removed | Very high |
| MAR | Aero Tube Hangar | Major 9×9 | 2,800 | Fortified | 4 | High | Major Station, air production, network node lost | Normal |
| MAR | Settlement Station | Large 7×7 | 1,900 | Fortified | 3 | **Very high** | Local economy remains lost; network segments around it sever | Medium |
| MAR | Mechanical Workshop | Large 7×6 | 1,450 | Structure | 2 | High | Ground production lost | High |
| MAR | Pressure Generator | Compact 4×4 | 900 | Structure | 1 | Very high | 9 E/s generation lost | High |
| MAR | Routing Laboratory | Standard 6×6 | 1,500 | Structure | 2 | High | Technology/network upgrades unavailable | High |
| MAR | Excavation Plant | Standard 6×6 | 1,250 | Structure | 2 | High | Local processing/storage lost | High |
| MAR | Deflector Arm | Compact 3×3 | 850 | Fortified | 3 | Tactical | Positional control removed | High |
| MAR | Aero Guard Tower | Compact 2×2 | 760 | Fortified | 2 | Tactical | AA coverage removed | High |
| MAR | Aero Tube Link | 1-cell route | 320 per functional 6-cell span | Structure | 1 | Very high | Link severed until repaired | Very high |

A Tube Link is one strategic connection but uses **320-HP functional spans** for damage/path targeting. Destroying any span severs the Link.

---

# PART XXI — WORKER & ECONOMIC HARASSMENT

## WORKER DURABILITY

| Worker | HP | Class | Armor | Speed |
|---|---:|---|---:|---:|
| Rock Raider Crew | 110 | Personnel | 0 | 1.35 |
| Expedition Crew | 110 | Personnel | 0 | 1.40 |
| ETX Servitor | 110 | Light Machine | 0 | 1.65 |
| Worker Robot | 115 | Light Machine | 1 | 1.30 |

Typical single-unit worker TTK:

- Razor Skimmer vs 110-HP Personnel: **~7.0s**
- Mono Jet vs Personnel: **~8.8s**
- Jet Scooter vs Personnel: **~6.9s**
- Alien Jet ground strafe vs Personnel: **~12s**

Two harassment units create an immediate serious threat.

Three can rapidly punish an undefended mineral line.

## FACTION ESCAPE DIFFERENCES

**Rock Raiders:** Crew can retreat into durable developed Worksit space and receive superior repair/service support.

**Astronauts:** conventional movement, nearby transport and clean evacuation routes.

**Aliens:** Servitor's 1.65 speed provides the strongest worker self-repositioning.

**Martians:** Worker Robots may use Aero Tubes when a route is available; the transfer is predictable and not instantaneous.

No faction worker is safe because of an automatic immunity.

---

# PART XXII — REPAIR & COMBAT SUSTAIN

Phase 04 establishes the economic repair relationship; this phase sets throughput.

## UNIVERSAL FIELD-REPAIR RULES — LSV

- Repairer cannot attack or move while repairing.
- Maximum **3 manual repairers** on one target.
- Stacking coefficients:
  - first: 100%;
  - second: 70%;
  - third: 40%.
- If repaired unit received hostile damage during the previous 2s, worker field-repair output operates at **60%** rate.
- If repairer itself is damaged, its repair pauses for **1s**.
- Field repairs use **1.35×** the dedicated-service resource cost.
- Dedicated full unit restoration costs **28% original Ore + 10% original Energy**, proportional to missing HP.
- Structure repair cost remains **30% Ore + 15% Energy**, proportional to missing HP.
- Crystals are not repaid for normal repair.

## ROCK RAIDER REPAIR

**Crew field repair:** 7 HP/s.

**Crew inside serviced Worksite:** 10 HP/s.

**Vehicle Service Bay:** 28 HP/s per repair job.

With Phase 04 **Service Gantries**, the Bay handles two simultaneous repair jobs.

A Bay target under active enemy damage is serviced at **50%**, or 14 HP/s.

### WORKSITE PASSIVE MAINTENANCE

A stationary Raider machine that has:

- not attacked;
- not received damage;

for 6 seconds inside a serviced Worksite automatically receives:

**4 HP/s**

using normal dedicated-service repair resources.

This is out-of-combat maintenance, not regeneration.

Rock Raiders receive **no damage aura** near their base.

## ASTRONAUT REPAIR

**Expedition Crew:** 6 HP/s.

**Service & Refit Hub:** 22 HP/s, 11 HP/s if the serviced unit is actively under fire.

**Deployed Solar Explorer:** 14 HP/s to one target within 3.5 cells, 7 HP/s under fire.

Integrated Expedition Command adds a second refit bay as established in Phase 04; it does not create a generic combat-healing aura.

## ALIEN REPAIR

Aliens intentionally lack Raider-like field sustain.

**ETX Servitor:** 5 HP/s, **structures only**.

**ETX Fabricator:** 16 HP/s to docked Jet/Razor/Servitor.

**Reconfiguration Dock:** 22 HP/s to docked Strike/Infiltrator.

**Alien Mothership:** carried eligible craft may be repaired at 10 HP/s only if the Mothership has neither dealt nor received combat damage for 4s.

No Alien mobile repair aura exists.

## MARTIAN REPAIR

**Worker Robot:** 5 HP/s.

**Mechanical Workshop:** 18 HP/s to one serviced machine.

**Aero Tube Hangar:** 12 HP/s to a Tube-eligible light unit.

**Excavation Searcher:** 8 HP/s to one friendly machine while stationary; its attack and Clamp are disabled during this repair.

Phase 04's Mechanical Worker Toolset gains one combat-side interpretation:

idle Worker Robots near a Station automatically maintain damaged Martian structures/Tube Links at **3 HP/s out of combat**.

Martian sustain remains weaker than Raider sustain and depends on extraction/repositioning.

---

# PART XXIII — TRANSFORMATION & DEPLOYMENT TIMINGS

| Asset | Transition | Time | Move during | Attack during | Vulnerability | Cancel | Reversal lock |
|---|---|---:|---|---|---|---|---:|
| MX-41 Ground → Flight | Transform | 2.25s | No | No | Air + ground targetable during transition | Before 40%; 0.6s return | 8s |
| MX-41 Flight → Ground | Transform | 2.25s | No | No | Air + ground | Same | 8s |
| Solar Explorer → Service | Deploy | 3.0s | No | No | Normal | Before 50% | 8s |
| Solar Explorer → Mobile | Undeploy | 2.5s | No | No | Normal | Before 50% | 8s |
| MT-201 → Drill | Deploy | 3.5s | No | No | Normal, immobile | Before 50%; 0.8s return | 10s |
| MT-201 → Travel | Undeploy | 3.0s | No | No | Normal | Before 50% | 10s |
| Alien Strike → Siege | Reconfigure | 2.8s | No | No | Air + ground | Before 40% | 8s |
| Alien Strike → Flight | Reconfigure | 2.8s | No | No | Air + ground | Before 40% | 8s |
| Infiltrator Craft ↔ Walker | Reconfigure | 2.0s | No | No | Ground-targetable throughout | Before 40% | 6s |
| Protector → Stance | Deploy | 2.4s | No | No | Normal | Before 50% | 8s |
| Protector → Mobile | Undeploy | 2.0s | No | No | Normal | Before 50% | 8s |
| Searcher → Excavation Brace | Deploy | 2.5s | No | No | Normal | Before 50% | 10s |
| Searcher → Mobile | Undeploy | 2.0s | No | No | Normal | Before 50% | 10s |

No transformation may be repeatedly animation-cancelled for free combat advantage.

---

# PART XXIV — ASTRONAUT MISSION REFIT COMBAT VALUES

Phase 04's refit costs, durations and 20-second Configuration Lock remain unchanged.

## T3-TRIKE

### ESCORT

- HP: 260
- Light Machine / Armor 1
- speed: 1.70
- sight: 9
- detection: —
- weapon: 18 Light / 1.10s
- base DPS: 16.36
- range: 4.5
- role: anti-light screening.

### SURVEY

- HP: 260
- Light Machine / Armor 0
- speed: 1.85
- sight: 13
- detection: 8
- weapon: 8 General / 1.40s
- DPS: 5.71
- range: 3.5.

Survey retains sufficient survivability for reconnaissance but gives up roughly two-thirds of Escort combat output.

## MOBILE MINING PLATFORM

Ore Drill and Crystal Reaper have identical combat stats:

- 400 HP;
- Heavy / Armor 2;
- 0.95 speed;
- 8 General / 1.50s;
- 5.33 DPS;
- range 3.5.

There is no combat-optimized mining configuration.

## MODULAR SENTINEL

Ground Interdiction and Air Interception values are defined in Part XIX.

Wrong-module play is strategically disadvantageous but the 800-HP chassis does not disappear merely because the opponent attacks through the uncovered target layer.

---

# PART XXV — ALIEN CRYSTAL CHARGE COMBAT

Phase 04's:

- **50 Charge cost**
- **18-second Surge Window**

remain locked.

## SURGE — LSV

Surge modifies exactly two combat dimensions:

### 1. ATTACK CADENCE

Eligible weapon cooldown:

# ×0.80

Equivalent sustained DPS increase:

# +25%

### 2. ETX RECONFIGURATION SPEED

Alien source-derived transformation/deployment durations:

# ×0.70

Surge does **not** increase:

- HP;
- Armor;
- damage per hit;
- range;
- movement speed;
- sight.

This produces temporary offensive intensity without simultaneous across-the-board stat inflation.

## ELIGIBLE UNITS

- Alien Jet
- Razor Skimmer
- ETX Alien Strike
- ETX Alien Infiltrator
- Alien Mothership

ETX Servitor is not eligible.

## ZONE RULES

**Resonance Core Surge radius:** 12 cells.

**Mothership Relay radius:** 10 cells.

A unit:

- gains cadence immediately upon entering;
- loses cadence immediately upon leaving.

Reload progress is proportionally remapped, preventing free instant shots.

A transformation receives the ×0.70 duration only if Surge is active when the transformation begins.

Multiple Surge zones do not stack.

## TELEGRAPH

Activation has a 0.75-second visible resonance buildup before the 18-second combat window begins.

The Charge is already spent during the buildup.

---

# PART XXVI — ALIEN TRANSFORMATION BALANCE

## ETX ALIEN STRIKE

### FLIGHT

- true air;
- speed 2.35;
- 14 General / 1.25s;
- DPS 11.20;
- range 4.5;
- can attack ground and air;
- moderate combat only.

### DEPLOYED

- stationary ground-targetable Medium Machine;
- 40 Siege / 1.70s;
- DPS 23.53;
- range 9.0;
- minimum range 3.0;
- 0.75 splash;
- slow 6-cell/s positional projectile.

Flight gains mobility.

Deployment gains structure pressure.

Neither is universally superior.

## ETX ALIEN INFILTRATOR

### CRAFT

- ground-layer hover;
- speed 2.05;
- sight 13;
- detection 9;
- 10 General / 1.20s;
- 8.33 DPS;
- range 4.0.

### WALKER

- Heavy class / Armor 3;
- speed 1.25;
- sight 11;
- detection 9;
- 34 Breach / 1.25s;
- 27.20 DPS;
- range 3.5.

Craft provides information and mobility.

Walker provides anti-heavy pressure.

---

# PART XXVII — MARTIAN MECHANICAL REPOSITIONING

Phase 02 and 03 establish Martian control as tow, redirect and modest displacement rather than stun-lock.

## GLOBAL SIZE RESPONSE — LSV

| Target class | Full hostile displacement relationship |
|---|---|
| Personnel | Full |
| Light | Full |
| Medium | ~60–70% |
| Heavy | ~25–35% |
| Massive | Immune |
| Structure | Immune |

## STABILITY

After any hostile displacement, a unit receives:

# STABILITY — 8 seconds

During Stability:

- additional hostile displacement distance ×0.25;
- displacement cannot cancel a transform/deployment;
- no additional movement interruption occurs.

Stability is the central anti-chain-control safeguard.

## DEFLECTOR ARM

Every 8 seconds it may automatically Deflect the nearest meaningful eligible ground target in 4.5 cells.

Displacement:

- Personnel/Light: 2.5 cells
- Medium: 1.5
- Heavy: 0.75
- Massive: 0

Damage:

14 Control.

## RED PLANET PROTECTOR — GUARD SWEEP

Requires canonical **Utility Mechanisms**.

Automatic frontal response no more than once every 8 seconds:

- Light: 2.0 cells
- Medium: 1.0
- Heavy: 0.5
- Massive: 0.

The Sweep does not stun.

## EXCAVATION SEARCHER — EXCAVATION CLAMP

One of the Martian army's few normal active combat commands.

- range: 4.5
- cooldown: 14s
- hostile damage: 8 Control
- friendly damage: none.

Enemy pull:

- Personnel/Light: 3.0 cells
- Medium: 2.0
- Heavy: 1.0
- Massive: immune.

Friendly tow:

- Personnel/Light/Medium: up to 3.0
- Heavy: 1.5
- Massive: 0.

Structures cannot be moved.

No displacement may force a unit into:

- lava;
- void;
- impassable geometry.

The result clamps to the nearest legal position.

---

# PART XXVIII — MARTIAN COMBAT NETWORK INTERACTION

Phase 04 limits Tube transfer to Worker Robot, Double Hover and Jet Scooter and gives normal transfers an 8–12-second travel expectation.

## EXIT RECOVERY

After Tube unloading:

- unit is targetable immediately;
- for 0.75s it cannot attack;
- movement operates at 60%;
- after 0.75s normal combat returns.

There is no exit invulnerability.

## EXIT SPACE

A Station maintains a 3 × 3-cell internal exit reservation.

Enemies cannot occupy the innermost 1-cell spawn core.

They may occupy and fight around its perimeter.

If the exit is obstructed:

1. unit waits in transfer queue up to 4s;
2. game searches for legal space within 3 cells;
3. unit exits at the nearest valid point.

Exit camping is therefore strategically possible without allowing permanent geometric imprisonment.

---

# PART XXIX — ROCK RAIDER INDUSTRIAL ADVANCE

The Worksite Network already provides repair and service infrastructure; Phase 04 deliberately does not make it a combat aura.

Exact combat-side Worksite benefits are:

- Crew repair 7 → **10 HP/s**
- passive out-of-combat maintenance **4 HP/s**
- Vehicle Service Bay **28 HP/s**
- short reinforcement distance
- nearby static defenses
- resource-efficient repair.

Rock Raiders receive **no**:

- damage bonus;
- range bonus;
- armor aura;
- movement aura.

Outside a Worksite, every Raider weapon and chassis operates at full base combat statistics.

What disappears is sustain, not basic functionality.

---

# PART XXX — ROCK RAIDER DRILL ESCALATION

## DRILL CRAFT

- 190 HP
- Light / A1
- speed 1.55
- 12 Siege / 1.25
- 9.60 base DPS
- 0.85 contact
- +100% damage against authored Excavatable/Destructible geology objects only.

It remains an engineering vehicle.

## GRANITE GRINDER

- 480 HP
- Heavy / A3
- speed 1.15
- 34 Breach / 1.50
- 22.67 DPS
- +25% damage after 2.5s continuous same-target drilling.

The Grinder is the economically efficient Raider answer to heavy machinery.

## CHROME CRUSHER

- 880 HP
- Massive / A5
- speed 0.92
- 55 Siege / 1.60
- 34.38 DPS
- +25% pre-class damage against Heavy/Massive machines.

Chrome Crusher is much better at:

- structures;
- Massive targets;
- surviving direct breach.

Granite Grinder remains:

- cheaper;
- faster;
- lower OC;
- available earlier;
- more cost-efficient for ordinary Heavy targets.

Chrome therefore does not obsolete Granite.

---

# PART XXXI — ASTRONAUT COMBINED ARMS

## MISSION FIGHTER VS SWITCH FIGHTER

**Mission Fighter**

- 3.15 air speed;
- 24.44 raw AA DPS;
- 5.5 range;
- dedicated air-only weapon.

**Switch Fighter**

- 2.45 flight speed;
- 13.91 flight General DPS;
- ground/air flexibility;
- 19.05 anti-light ground DPS in ground mode.

Mission Fighter clearly wins dedicated air superiority.

Switch Fighter clearly provides wider tactical application.

## CLAW-TANK VS MT-101

**Claw-Tank**

- 460 HP;
- 1.30 speed;
- anti-light splash;
- 21.67 Light DPS;
- can fire while retreating because its upper assembly tracks independently.

**MT-101**

- 700 HP;
- 1.08 speed;
- contact Breach;
- strong anti-heavy;
- poor against distributed light skirmishers.

Neither replaces the other.

## MT-101 VS MT-201

**MT-101:** mobile anti-heavy frontline.

**MT-201:** immobile deployed structure killer.

MT-101 has no deployment burden and can participate in ordinary maneuver.

MT-201 wins structure races but is strategically obvious.

## SOLAR EXPLORER

Improves army sustain and refit access.

It contributes only 6.67 raw DPS.

Destroying or forcing it away reduces sustain, not the army's entire damage engine.

## MX-81

Provides:

- 15 sight;
- 10 detection;
- long-range information;
- moderate 13.33 General DPS.

It is an expensive information/support aircraft, not a mandatory damage multiplier.

---

# PART XXXII — AIR COMBAT

## TRUE-AIR MODEL — LSV

All true-air units occupy one abstract combat altitude.

There is:

- no altitude toggle;
- no dive height;
- no manual vertical micro.

True air ignores normal surface terrain according to Phase 05 but respects explicit Air-Blocking Volumes.

## AIR COLLISION

Air units have soft horizontal separation.

They do not collide with ground units.

## AIR ROLE LIMITS

True air cannot normally:

- capture/hold surface territory by itself;
- harvest;
- construct bases;
- physically block ground chokepoints.

## IDENTITY

**Mission Fighter:** premier dependable interceptor.

**Alien Jet:** fastest aggressive interception/scouting pressure; weaker in straight duel.

**Mono Jet:** light surface harassment/scouting.

**Aero Skiff:** scout/transport, unarmed.

**Tunnel Transport:** heavy strategic lift, unarmed.

**MX-71:** transport/recon with light defensive fire.

**MX-81:** advanced support.

**Alien Mothership:** carrier/Surge support with modest weapons.

---

# PART XXXIII — ANTI-AIR

| AA asset | Raw AA DPS | Range | Behavior | Strategic relationship |
|---|---:|---:|---|---|
| Mission Fighter | 24.44 | 5.5 | Guided, speed 12 | Best mobile conventional interceptor |
| Alien Jet | 18.95 | 5.0 | Guided, speed 12 | Faster pressure; +25% cadence under Surge |
| Recon-Mech RP | 20.00 | 6.5 | Guided, speed 10 | Ground walker AA + detection |
| Cutter Mast | 24.00 | 7.5 | Beam | Strong Raider fixed air denial |
| Sentinel Air | 26.67 | 7.5 | Guided | Highest normal static single-target AA |
| ETX Air Lance | 26.67 | 7.0 | Beam | Strong but fragile/power-dependent |
| Aero Guard Tower | 24.44 | 7.25 | Guided | Network defense AA |

Air units can still concentrate against isolated AA, but unsupported aircraft cannot safely cross a properly prepared defended base.

---

# PART XXXIV — TRANSPORT COMBAT RULES

## UNIVERSAL TRANSPORT LOAD VALUES

Passenger size points:

- Personnel/Tiny: 1
- Small: 2
- Medium: 3
- Large: 4
- Huge: 6.

## TRANSPORT CAPACITY

| Transport | Capacity / restrictions |
|---|---|
| Rapid Rider | 4; Personnel only |
| Tunnel Transport | 8; Raider machines including one Chrome Crusher |
| Solar Explorer | 4; Crew/Rover-class support passengers |
| MX-71 | 8; Crew, Rover, T3, Claw-Tank; no MT-101/MT-201/MX-81 |
| Alien Mothership | 10; Servitors/Jets/Razors and max two Strike/Infiltrator-class craft |
| Aero Skiff | 2; two Worker Robots or one Double Hover/Jet Scooter |

## LOADING

Universal baseline:

- 1.5s docking/settling;
- 1.0s per normal passenger.

Rapid Rider:

- 0.75s per passenger.

Transport and passenger must remain within loading range.

Taking direct damage pauses loading progress for 0.75s.

Loading under fire is possible but not instantaneous.

## UNLOADING

- 1.0s settle;
- 0.75s per normal passenger.

Rapid Rider:

- 0.50s per Crew.

## PASSENGER TARGETING

Passengers are not individually targetable while loaded.

## TRANSPORT DESTRUCTION — LSV

No random full-cargo deletion.

When a transport is destroyed:

- surviving passengers emergency-deploy onto nearest legal ground;
- each returns at **40% maximum HP**;
- cannot attack for **1.5s**;
- movement is −30% for **2.5s**.

Passengers therefore usually survive but emerge badly compromised.

Transport play remains risky without arbitrary catastrophic dice rolls.

---

# PART XXXV — DETECTION & INFORMATION

There is **no conventional competitive invisibility/stealth subsystem**.

No normal roster requires permanent cloaking.

Detection remains useful for:

- special concealed map objects;
- campaign concealment;
- sensor interference;
- crystal-discharge interference;
- special map devices;
- future mission scripting.

## LAST-KNOWN INFORMATION

Mobile enemy:

- last-known marker persists **3s** after vision loss;
- then disappears.

Known enemy structure:

- fog-memory silhouette remains until area is revisited.

There is no exact through-fog tracking.

---

# PART XXXVI — ENVIRONMENTAL COMBAT DAMAGE

Phase 05 requires predictable hazards and explicitly avoids faction immunity.

| Hazard | Telegraph | Combat effect |
|---|---|---|
| Lava | Permanent boundary | Ground impassable; air crosses; displacement cannot push into it |
| Volcanic vent | 2.5s buildup | 10 HP/s for 4s; then 8s inactive |
| Crystal discharge | 1.5s glow | 18 initial + 6 HP/s for 3s; sight/detection −25% for 4s |
| Rockfall | 2s dust/cracks | 35 damage + Rough Ground for 8s |
| Cryogenic vent | 1.5s frost buildup | 8 HP/s for 3s; movement −25% for 4s |
| Pressure/steam | Visible pressure cycle | 6 HP/s for 3s; sight −30% for 3s |
| Ice fracture | 3s crack spread | Creates Rough Ground / scripted route closure; normally no direct damage |
| Ion storm | Strong warning | True-air sight −25%; no direct damage |
| Abandoned machinery | Fixed visible cycle | 50 mechanical damage on contact |

Aliens receive no crystal-discharge immunity.

Rock Raiders receive no lava/rockfall immunity.

---

# PART XXXVII — ROCK MONSTER COMBAT

Rock Monsters remain neutral non-respawning environmental/resource guardians under Phase 05.

## ROCK MONSTER — BTV

- HP: **700**
- class: Heavy Machine
- Armor: **3**
- footprint: Large
- speed: **1.05**
- acceleration: Heavy
- turn: Standard
- sight: **8**
- aggression trigger: **8 cells**
- leash: **14 cells**
- respawn: **No**

### ROCK SLAM

- damage: 32 General
- cooldown: 1.60s
- raw DPS: 20
- range: 1.0
- ground only
- splash: 0.75, 50% outer.

## TARGET PRIORITY

1. immediate threat;
2. workers harvesting nearby guarded resource;
3. structures inside habitat.

No normal kill bounty exists.

The reward is access to the position/resource it guarded.

A small prepared combat group can clear one.

A lone worker cannot safely ignore it.

Rock Raiders are not immune.

---

# PART XXXVIII — BUILDING DESTRUCTION & BASE ASSAULT

Buildings retain full gameplay functionality until 0 HP.

There is no production-speed degradation at arbitrary damage percentages.

Damage thresholds are presentation/readability states only:

- below 70% — damaged;
- below 35% — heavily damaged;
- below 20% — critical.

These states may affect:

- visible missing pieces;
- sparks;
- alarm feedback;

but not hidden statistical efficiency.

## ENERGY

Destroying a generator immediately changes Phase 04 Energy generation.

Brownout consequences remain Phase 04 rules.

## PRODUCTION

A production building's current item is lost if the structure is destroyed.

It is not lost merely because the building is damaged.

## NETWORK

Destroying a Martian Station or Link immediately updates network connectivity.

## RESONANCE

Destroying Resonance Core immediately updates Charge capacity and invokes Phase 04 salvage rules.

---

# PART XXXIX — DEATH / DESTRUCTION GAMEPLAY

## UNIT WRECKS

Standard unit:

- blocking collision: **none after 0 HP; removed in the same authoritative simulation tick**
- nonblocking debris: until **8s**
- remaining purely visual pieces may continue beyond that.

Huge/Massive unit:

- blocking collision: **none after 0 HP; removed in the same authoritative simulation tick**
- nonblocking debris: until **12s**.

## STRUCTURE RUBBLE

Destroyed structures have **no blocking collision after 0 HP**. Their authored
footprint becomes pathable in the same authoritative simulation tick. Collapse
and rubble may remain visually, but are cosmetic and nonblocking.

There is no permanent wreck-field pathing problem.

## SALVAGE

No universal combat salvage system exists.

Exceptions:

- Phase 04 Resonance Salvage Cache;
- authored campaign/map mechanics.

---

# PART XL — COMBAT UPGRADES

Phase 04 deliberately avoids a universal Damage I / II / III ladder.

## ROCK RAIDERS

### CUTTER PACKAGE

Loader Dozer replaces Scoop Ram:

**baseline:**  
18 General / 1.35s / 13.33 DPS.

**Cutter:**  
22 Light / 1.15s / 19.13 DPS.

Visual change: prominent Chain Dozer-derived rotating cutter.

### REINFORCED DRILLING ASSEMBLIES

Already unlocks Granite Grinder and reinforced excavation.

Granite's sustained 2.5s drill-ramp behavior is the combat expression of the reinforced assembly.

### SERVICE GANTRIES

No direct combat stat bonus.

Enables two simultaneous Service Bay repair jobs as Phase 04 specifies.

## ASTRONAUTS

### FIELD SURVEY PACKAGE

T3 Survey becomes available.

Rover gains:

- +1 sight;
- +2 detection.

These values are included in final post-research information ranges where appropriate.

### FIELD SUSTAINMENT PACKAGE

Unlocks Solar Explorer deployed service.

No combat aura.

### SWITCHFRAME ACTUATION

Unlocks MX-41 and its 2.25s tactical transformation relationship.

### AEROSPACE COORDINATION

Unlocks Mission Fighter/MX-71.

No hidden global aircraft damage bonus.

### HEAVY MISSION CHASSIS / DEEP MISSION DRILLING

Unlock MT-101 / MT-201 respectively.

No generic faction damage percentage.

### INTEGRATED EXPEDITION COMMAND

Unlocks MX-81 and second Service Hub refit bay.

No death-ball combat aura.

## ALIENS

### RESONANCE INITIATION

Unlocks Surge.

### SIEGE PHASE COUPLING

Unlocks Strike siege transformation.

### INFILTRATION MATRIX

Unlocks Infiltrator.

### DEFENSE RESONANCE SHUNT

25 Charge / 12s:

- ETX Defense Node cooldown ×0.75;
- tracking +30%;
- no range/HP/damage increase.

### MOTHERSHIP RESONANCE RELAY

Allows Mothership to act as 10-cell mobile Surge anchor.

## MARTIANS

### MECHANICAL WORKER TOOLSET

Adds 3 HP/s out-of-combat automatic Station-area maintenance.

### WALKER ARTICULATION

Unlocks Recon-Mech / Protector family.

### UTILITY MECHANISMS

Enables:

- Deflector Arm;
- Protector Guard Sweep;
- full Protector positional resistance.

### ADVANCED EXCAVATION SYSTEMS

Unlocks Excavation Searcher and Excavation Clamp functionality.

No generic Martian damage ladder is introduced.

---

# PART XLI — ECONOMIC VALUE & COMBAT VALUE

For internal diagnostics only, Phase 06 uses:

# COMBAT RESOURCE VALUE — CRV

**CRV = Ore + 0.5 × Energy + 50 × Crystals**

CRV is not visible in the game.

It helps detect accidental extremes without forcing equal ratios.

Examples:

| Unit | Approx. CRV | Design reading |
|---|---:|---|
| Loader Dozer | 132.5 | High local durability per resource; pays with contact range |
| Razor Skimmer | 117.5 | Lower durability; pays premium for extreme mobility |
| T3-Trike | 117.5 | Efficient early combined-arms chassis |
| Granite Grinder | 257.5 | Specialized heavy killer |
| MT-101 | 415 | Expensive durable mobile Breach |
| Chrome Crusher | 575 | Very high direct breaching investment |
| MX-81 | 645 | Low damage efficiency because information/air mobility are the purchased value |
| Alien Mothership | 800 | Strategic carrier/Surge platform, deliberately poor raw DPS-per-resource |

Metrics used during tuning:

- neutral effective HP / CRV;
- intended-target DPS / CRV;
- DPS / OC;
- build-time pressure;
- Crystal intensity;
- mobility premium.

Factions are **not** normalized to identical ratios.

---

# PART XLII — PHASE 04 COST REVISION RULE

Combat analysis does not justify any Phase 04 cost change.

# PHASE 04 → PHASE 06 NUMERICAL AMENDMENTS

| Asset | Parameter | Phase 04 | Phase 06 | Reason | Downstream effect |
|---|---|---|---|---|---|
| **None** | — | — | — | Combat roles can be balanced through HP, armor, range, targeting, movement, weapon behavior and state relationships | Phase 04 economy remains intact |

All Phase 04:

- Ore;
- Energy;
- Crystal;
- OC;
- production times;
- building costs;

remain canonical prototype baselines.

---

# PART XLIII — COUNTER STRENGTH STANDARD

## SOFT COUNTER

Expected equal-resource direct result:

**~55–60% winner advantage**

Typical efficiency improvement:

**~1.15–1.30×**

Positioning can readily overturn it.

## STRONG COUNTER

Expected equal-resource direct result:

**~65–72%**

Typical efficiency:

**~1.35–1.60×**

Opponent should usually need:

- support;
- terrain;
- retreat;
- focus advantage;

to reverse it.

## HARD-ISH SPECIALIST COUNTER

Expected equal-resource direct result:

**~75–80%**

Typical efficiency:

**~1.60–1.85×**

Still not absolute.

The specialist can lose through:

- poor approach;
- support failure;
- flanking;
- map geometry;
- economic disparity.

No routine 5× hard-lock modifier exists.

---

# PART XLIV — UNIT COUNTER MATRIX

## ROCK RAIDERS

| Unit | Good against | Poor against | Reliable pressure against it | Supports |
|---|---|---|---|---|
| Crew | — | all combat | any raider | repair/infrastructure |
| Hover Scout | information | combat | fast skirmishers | whole army |
| Drill Craft | terrain/undefended structures | mobile army | almost any combat unit | route creation |
| Rapid Rider | positioning | direct combat | interceptors | Crew raids |
| Loader Dozer | Personnel/Light | Heavy, air, kiting | Breach/ranged | frontline |
| Granite Grinder | Heavy/Massive | swarms, air, kiting | Light/range | breaching |
| Chrome Crusher | structures/Massive | air, long-range flanks | anti-heavy/kite | siege |
| Tunnel Transport | strategic relocation | AA | dedicated AA | heavy machinery |

## ASTRONAUTS

| Unit | Good against | Poor against | Reliable pressure | Supports |
|---|---|---|---|---|
| Expedition Crew | — | combat | harassment | economy/repair |
| Rover | information | combat | fast light | composition |
| T3 Escort | Light | Heavy | Breach | screening |
| T3 Survey | information | fighting | nearly all combat | reconnaissance |
| Mono Jet | workers/scouts | AA/heavy | dedicated AA | scouting |
| Solar Explorer | sustain | focus/siege | anti-heavy/focus | mixed army |
| Mission Fighter | true air | ground | static AA | air control |
| Switch Fighter | changing attack axes | specialists | dedicated counters | tactical flexibility |
| Mining Platform | economy | combat | harassment | resource strategy |
| MX-71 | transport | interceptors | AA | mobility |
| Claw-Tank | Light/Medium | Heavy | Breach | frontline |
| MT-101 | Heavy | light kiting/air | anti-heavy range | frontline breach |
| MT-201 | structures | flank/air/mobile pressure | mobile attackers | siege |
| MX-81 | information/support | heavy AA | interceptor focus | combined arms |

## ALIENS

| Unit | Good against | Poor against | Reliable pressure | Supports |
|---|---|---|---|---|
| Servitor | — | combat | harassment | economy |
| Alien Jet | air/scouting | Mission Fighter/static AA | dedicated AA | pressure |
| Razor | Personnel/Light | durable frontline | anti-light/generalists | harassment |
| Strike | structures | flanks/minimum range | mobile army | siege |
| Infiltrator | Heavy/Massive | light swarms/focus | anti-light | information/antiheavy |
| Mothership | strategic support | concentrated AA | dedicated AA + infrastructure attack | whole Alien force |

## MARTIANS

| Unit | Good against | Poor against | Reliable pressure | Supports |
|---|---|---|---|---|
| Worker Robot | — | combat | harassment | economy/repair |
| Double Hover | scouting | combat | fast attackers | network info |
| Jet Scooter | workers/light | Heavy | frontline | raids |
| Aero Skiff | transport/scout | AA | AA | light reinforcement |
| Cruiser | general surface combat | anti-medium/heavy | Breach | frontline |
| Recon-Mech | air | ground heavy | Breach/general | detection |
| Protector | Heavy | range/flanks/siege | anti-heavy/mobile range | positional line |
| Searcher | structures/control | ranged focus/air | focus/siege | entire control army |

---

# PART XLV — PER-UNIT FINAL COMBAT SPEC

All costs below are inherited from Phase 04.

## ROCK RAIDERS — CHASSIS / MOVEMENT

| Unit | Stage | Cost O/E/C | OC | HP | Class / Armor | Footprint | Movement | Speed | Accel / Turn | Sight / Detect |
|---|---|---|---:|---:|---|---|---|---:|---|---|
| Rock Raider Crew | Early | 50/0/0 | 1 | 110 | Personnel / 0 | Tiny | Foot | 1.35 | Quick / Agile | 8 / — |
| Hover Scout | Early | 75/10/0 | 1 | 120 | Light / 0 | Small | Hover | 2.25 | Snap / Agile | 12 / 8 |
| Drill Craft | Early | 95/15/0 | 2 | 190 | Light / 1 | Small | Wheeled | 1.55 | Quick / Quick | 8 / — |
| Rapid Rider | Early | 90/10/0 | 2 | 170 | Light / 0 | Small | Amphibious skimmer | 2.10 | Quick / Agile | 9 / — |
| Loader Dozer | Early–Mid | 125/15/0 | 3 | 360 | Medium / 2 | Medium | Wheeled | 1.30 | Heavy / Standard | 9 / — |
| Granite Grinder | Mid | 190/35/1 | 4 | 480 | Heavy / 3 | Medium | Walker | 1.15 | Heavy / Standard | 9 / — |
| Chrome Crusher | Advanced | 330/90/4 | 6 | 880 | Massive / 5 | Large | Wheeled | 0.92 | Massive / Heavy | 9 / — |
| Tunnel Transport | Advanced | 300/110/3 | 5 | 650 | Massive / 3 | Huge | True air | 1.75 | Heavy / Heavy | 11 / — |

## ROCK RAIDERS — COMBAT / SYSTEMS

| Unit | G/A | Weapon | Dmg / type / CD / DPS | Range | Projectile / splash | Priority | State / cooldown | Repair | Transport | Target / weakness / behavior |
|---|---|---|---|---:|---|---|---|---|---|---|
| Crew | G | Portable Mining Tool | 6 General /1.2 /5.0 | 0.8 | Contact | Self-defense | — | 7; 10 in Worksite | Rapid/Tunnel | Economy unit; flee normal combat |
| Hover Scout | G | Survey Pulse | 6 General /1.5 /4.0 | 3.0 | Pulse 12 | Threat only | Passive survey | Service Bay | Tunnel | Information; fragile |
| Drill Craft | G | Mining Drill | 12 Siege /1.25 /9.6 | 0.85 | Contact | Terrain/structure | Excavation orders | Service Bay | Tunnel | Engineering first; combat weak |
| Rapid Rider | — | **NO NORMAL COMBAT WEAPON** | — | — | — | — | Load/unload | Service Bay | — | Personnel transport |
| Loader Dozer | G | Scoop Ram / Cutter | 18 Gen/1.35/13.3 → 22 Light/1.15/19.1 | 0.9 | Contact | Personnel/Light | Cutter Package | Service Bay | Tunnel | Durable screen; kitable |
| Granite Grinder | G | Granite Drill | 34 Breach/1.5/22.7; 28.3 ramped | 1.0 | Contact | Heavy | 2.5s drill ramp | Service Bay | Tunnel | Heavy specialist |
| Chrome Crusher | G | Chrome Drill | 55 Siege/1.6/34.4 | 1.05 | Contact | Structures/Massive | +25% vs Heavy/Massive | Service Bay | Tunnel | Direct breacher; slow |
| Tunnel Transport | — | **NO NORMAL COMBAT WEAPON** | — | — | — | — | Transport | Service Bay | — | Strategic lift; AA target |

## ASTRONAUTS — CHASSIS / MOVEMENT

| Unit | Stage | Cost O/E/C | OC | HP | Class / Armor | Footprint | Movement | Speed | Accel / Turn | Sight / Detect |
|---|---|---|---:|---:|---|---|---|---:|---|---|
| Expedition Crew | Early | 50/0/0 | 1 | 110 | Personnel /0 | Tiny | Foot | 1.40 | Quick/Agile | 8/— |
| Rover | Early | 70/5/0 | 1 | 130 | Light /0 | Small | Wheeled | 2.15 | Snap/Agile | 13/6 |
| T3-Trike | Early | 110/15/0 | 2 | 260 | Light /1 Escort; /0 Survey | Medium | Rough wheeled | 1.70 /1.85 | Quick/Quick | 9/—; 13/8 |
| Mono Jet | Early | 100/35/0 | 1 | 150 | Light /0 | Small | True air | 2.85 | Snap/Agile | 13/— |
| Solar Explorer | Mid | 180/40/1 | 4 | 520 | Heavy /2 | Large | Wheeled | 1.10 | Heavy/Heavy | 11/4 |
| Mission Fighter | Mid | 140/55/1 | 2 | 240 | Light /1 | Small | True air | 3.15 | Snap/Agile | 11/— |
| MX-41 Switch Fighter | Mid | 170/60/1 | 3 | 320 | Medium /2 | Medium | Wheeled / Air | 1.75 /2.45 | Quick/Quick | 10/— |
| Mobile Mining Platform | Mid | 170/25/0 | 3 | 400 | Heavy /2 | Large | Tracked | 0.95 | Heavy/Heavy | 9/— |
| MX-71 Recon Dropship | Mid | 200/70/1 | 4 | 420 | Heavy /2 | Large | True air | 2.15 | Standard/Standard | 12/6 |
| MT-51 Claw-Tank | Mid | 180/35/0 | 3 | 460 | Heavy /3 | Large | Tracked | 1.30 | Heavy/Standard | 9/— |
| MT-101 ADU | Advanced | 280/70/2 | 5 | 700 | Heavy /4 | Large | Wheeled | 1.08 | Heavy/Heavy | 9/— |
| MT-201 Ultra-Drill | Advanced | 320/90/3 | 6 | 760 | Heavy /4 | Huge | Walker / deployed | 0.92 /0 | Massive/Heavy | 9/— |
| MX-81 Operations Aircraft | Advanced | 380/130/4 | 6 | 720 | Massive /3 | Huge | True air | 1.90 | Heavy/Heavy | 15/10 |

## ASTRONAUTS — COMBAT / SYSTEMS

| Unit | G/A | Weapon | Dmg / type / CD / DPS | Range | Behavior | State | Repair / transport | Target / weakness |
|---|---|---|---|---:|---|---|---|---|
| Expedition Crew | G | Field Tool | 6 General/1.2/5.0 | 0.8 | Contact | — | Crew repair6; Solar/MX71 | Worker |
| Rover | G | Survey Pulse | 7 General/1.4/5.0 | 3.3 | Pulse12 | Survey passive | Service; Solar/MX71 | Recon; fragile |
| T3 Escort | G | Escort Projector | 18 Light/1.10/16.36 | 4.5 | Fast12 | Mission Refit | Service; MX71 | Light; weak vs Heavy |
| T3 Survey | G | Survey Projector | 8 General/1.4/5.71 | 3.5 | Pulse12 | Mission Refit | Service; MX71 | Information |
| Mono Jet | G | Surface Pulse | 12 Light/1.2/10.0 | 4.0 | Pulse12 | — | Flight service | Workers; AA |
| Solar Explorer | G | Defense Pulse | 10 General/1.5/6.67 | 4.0 | Pulse10 | Deploy service 3.0s | Repair14; carries4 | Support target |
| Mission Fighter | A | Interceptor Lance | 22 AA/0.9/24.44 | 5.5 | Guided12 | — | Flight Pad | Air specialist |
| Switch — Ground | G | Pursuit Projector | 20 Light/1.05/19.05 | 4.5 | Fast12 | 2.25s transform | Service | Light |
| Switch — Flight | G+A | Flight Pulse | 16 General/1.15/13.91 | 4.5 | Pulse12 | 2.25s transform | Flight service | Flexible, not specialist |
| Mining Platform | G | Utility Pulse | 8 General/1.5/5.33 | 3.5 | Pulse10 | Ore/Crystal refit | Service | Economic unit |
| MX-71 | G+A | Defensive Pulse | 12 General/1.25/9.60 | 4.5 | Pulse12 | Transport | Flight service; cap8 | Transport; AA |
| Claw-Tank | G | Multi-Claw Projector | 26 Light/1.2/21.67 | 3.2 | 0.75 splash; fires while moving | — | Service; MX71 | Light/Medium |
| MT-101 | G | Armored Drill | 38 Breach/1.35/28.15 | 1.0 | Contact | — | Service | Heavy; range/kiting |
| MT-201 Travel | G | Defense Pulse | 14 General/1.4/10 | 3.5 | Pulse10 | Deploy3.5s | Service | Limited travel fighting |
| MT-201 Drill | G | Ultra-Drill | 62 Siege/1.5/41.33 | 1.2 | Contact | Stationary | Service | Structures; flank/air |
| MX-81 | G+A | Operations Pulse | 16 General/1.2/13.33 | 5.0 | Pulse12 | Support | Flight service | Information; heavy AA |

## ALIENS — CHASSIS / MOVEMENT

| Unit | Stage | Cost O/E/C | OC | HP | Class / Armor | Footprint | Movement | Speed | Accel / Turn | Sight / Detect |
|---|---|---|---:|---:|---|---|---|---:|---|---|
| ETX Servitor | Early | 50/5/0 | 1 | 110 | Light /0 | Tiny | Hover | 1.65 | Quick/Agile | 8/— |
| Alien Jet | Early | 95/35/0 | 2 | 180 | Light /0 | Small | True air | 3.10 | Snap/Agile | 12/— |
| Razor Skimmer | Early | 105/25/0 | 2 | 210 | Light /1 | Small | Hover | 2.30 | Snap/Agile | 10/— |
| ETX Alien Strike | Mid | 190/60/1 | 4 | 330 | Medium /1 | Medium | True air / deployed | 2.35 /0 | Quick/Quick | 10/— |
| ETX Alien Infiltrator | Mid | 210/65/2 | 4 | 360 | Medium /2 craft; Heavy /3 walker | Medium | Hover / Walker | 2.05 /1.25 | Quick→Heavy / Quick→Std | 13→11 /9 |
| Alien Mothership | Advanced | 420/160/6 | 8 | 1,200 | Massive /4 | Huge | True air | 1.65 | Heavy/Massive | 14/6 |

## ALIENS — COMBAT / SYSTEMS

| Unit | G/A | Weapon | Dmg/type/CD/DPS | Range | Behavior | State | Repair/transport | Target/weakness |
|---|---|---|---|---:|---|---|---|---|
| Servitor | — | **NO NORMAL COMBAT WEAPON** | — | — | — | Worker | Fabricator; Mothership | Economy |
| Alien Jet | A | Interceptor Pulse | 18 AA/0.95/18.95 | 5.0 | Guided12 | Surge eligible | Mothership | Air; Mission Fighter |
| Alien Jet | G | Ground Strafe | 8 Light/1.10/7.27 | 3.8 | Pulse12 | Surge eligible | Mothership | Light harassment |
| Razor | G | Razor Pulse | 15 Light/1.2/12.50 | 4.0 | Projectile10 | Surge eligible | Mothership | Workers/Light; durable lines |
| Strike Flight | G+A | Strike Pulse | 14 General/1.25/11.20 | 4.5 | Pulse12 | Transform2.8s | Dock/Mothership | Mobility |
| Strike Siege | G | Siege Resonator | 40 Siege/1.7/23.53 | 9.0; min3 | Slow6, splash0.75 | Stationary | Dock/Mothership | Structures; flank |
| Infiltrator Craft | G | Infiltration Pulse | 10 Gen/1.2/8.33 | 4.0 | Pulse12 | Transform2s | Dock/Mothership | Info |
| Infiltrator Walker | G | Heavy Disruptor | 34 Breach/1.25/27.20 | 3.5 | Projectile8 | Heavy state | Dock/Mothership | Heavy; light focus |
| Mothership | G+A | Mothership Pulse | 18 General/1.0/18 | 5.5 | Pulse12 | Surge anchor upgrade | internal10 HP/s; cap10 | Support; heavy AA |

## MARTIANS — CHASSIS / MOVEMENT

| Unit | Stage | Cost O/E/C | OC | HP | Class / Armor | Footprint | Movement | Speed | Accel / Turn | Sight / Detect |
|---|---|---|---:|---:|---|---|---|---:|---|---|
| Worker Robot | Early | 50/0/0 | 1 | 115 | Light /1 | Tiny | Walker | 1.30 | Quick/Quick | 8/— |
| Double Hover | Early | 65/10/0 | 1 | 120 | Light /0 | Small | Hover | 2.25 | Snap/Agile | 12/6 |
| Jet Scooter | Early | 90/15/0 | 1 | 180 | Light /0 | Small | Hover | 2.40 | Snap/Agile | 10/— |
| Aero Skiff | Early–Mid | 120/40/0 | 2 | 190 | Light /0 | Small | True air | 2.60 | Snap/Agile | 12/— |
| Red Planet Cruiser | Early–Mid | 150/25/0 | 3 | 400 | Medium /2 | Medium | Hover | 1.50 | Quick/Quick | 9/— |
| Recon-Mech RP | Mid | 170/35/1 | 3 | 300 | Medium /2 | Medium | Walker | 1.32 | Quick/Standard | 13/9 |
| Red Planet Protector | Mid | 220/50/2 | 4 | 560 | Heavy /3; stance A4 | Large | Walker / stance | 1.18 /0 | Heavy/Standard | 10/— |
| Excavation Searcher | Advanced | 330/80/3 | 6 | 760 | Massive /4 | Huge | Walker / braced | 0.98 /0 | Massive/Heavy | 11/4 |

## MARTIANS — COMBAT / SYSTEMS

| Unit | G/A | Weapon | Dmg/type/CD/DPS | Range | Behavior | State | Repair/transport | Target/weakness |
|---|---|---|---|---:|---|---|---|---|
| Worker Robot | G | Utility Tool | 5 Gen/1.25/4 | 0.8 | Contact | Tube eligible | repair5; Tube/Skiff | Worker |
| Double Hover | — | **NO NORMAL COMBAT WEAPON** | — | — | — | Tube eligible | Tube/Skiff | Scout |
| Jet Scooter | G | Scooter Pulse | 14 Light/1.10/12.73 | 3.8 | Projectile10 | Tube eligible | Tube/Skiff | Worker/Light |
| Aero Skiff | — | **NO NORMAL COMBAT WEAPON** | — | — | — | Light transport | — | Scout/transport; AA |
| Cruiser | G | Cruiser Projector | 22 Gen/1.15/19.13 | 4.8 | Projectile9 | — | Workshop | General frontline |
| Recon-Mech | A | Aero Tracker | 20 AA/1.0/20 | 6.5 | Guided10 | — | Workshop | Air |
| Recon-Mech | G | Sensor Ping | 10 Gen/1.4/7.14 | 4.5 | Pulse | — | Workshop | weak ground defense |
| Protector Mobile | G | Protector Tool | 32 Breach/1.4/22.86 | 2.6 | Mechanical | Stance2.4s | Workshop | Heavy |
| Protector Stance | G | Protector Tool | 32 Breach/1.25/25.60 | 3.2 | frontal120° | A4/control resistance | Workshop | anchor; flank |
| Searcher Mobile | G | Handling Claw | 20 Gen/1.5/13.33 | 2.5 | mechanical | Clamp14s | repair8 | utility |
| Searcher Braced | G | Excavation Clamp | 46 Siege/1.7/27.06 | 4.5; min1.5 | mechanical extension | Brace2.5s | repair disabled while attacking | siege/control |

---

# PART XLVI — PER-BUILDING FINAL COMBAT SPEC

The complete 31-entry HP/Class/Armor table is Part XX.

Additional universal building rules:

- all structures repairable;
- defensive structures obey Part XIX;
- non-defensive buildings have **NO NORMAL COMBAT WEAPON**;
- powered infrastructure loses relevant active functions in brownout;
- HQ auxiliary generation follows Phase 04;
- command structures remain functional until destruction;
- Siege damage interacts entirely through the Damage Matrix;
- no secret building-specific resistance table exists.

All 31 entries therefore possess complete durability/combat-state data.

---

# PART XLVII — COMPOSITION ARCHETYPE NUMERICAL CHECK

CRV is diagnostic only.

## ROCK RAIDERS

| Composition | Cost O/E/C | OC | HP | Raw relevant DPS | Mobility | Reading |
|---|---|---:|---:|---:|---|---|
| 4 Loader + 2 Grinder + Scout | 955/140/2 | 21 | 2,520 | ~103 | Medium-low | Durable general pressure |
| 4 Grinder + 3 Loader + 2 Crew | 1,235/185/4 | 27 | 3,220 | ~141 | Low | Heavy-kill/repair push |
| 2 Chrome + 3 Grinder + 2 Loader | 1,480/315/11 | 30 | 3,920 | ~163 | Very low | Siege/Heavy peak |
| Tunnel + Chrome + 2 Grinder + Loader | 1,135/285/9 | 22 | 2,850 | ~93 | Strategic lift | Sacrifices raw army density for relocation |

No composition dominates.

Heavy breach has enormous Crystal and mobility costs.

## ASTRONAUTS

| Composition | O/E/C | OC | HP | Raw relevant DPS | Key dependency |
|---|---|---:|---:|---:|---|
| 4 T3 + 2 Mono + Solar | 820/170/1 | 14 | 1,860 | ~92 | Weak to Heavy |
| 4 Claw + 2 Mission Fighter + MX71 | 1,200/320/3 | 20 | 2,740 | ~145 split by layer | Needs anti-heavy |
| 2 MT101 + MT201 + 2 Claw + Solar | 1,420/340/8 | 26 | 3,600 | ~148 | Slow / expensive |
| 3 Switch + 3 Mission Fighter + MX71 | 1,130/415/7 | 19 | 2,100 | ~140 split | High Energy/AA exposure |
| MX81 + 3 Claw + 2 T3 + Solar | 1,320/305/5 | 23 | 3,140 | ~118 | Broad support, not maximum damage |

Astronaut breadth therefore creates options rather than one universal death-ball.

## ALIENS

| Composition | O/E/C | OC | HP | Raw relevant DPS | Reading |
|---|---|---:|---:|---:|---|
| 6 Razor + 3 Jet | 915/255/0 | 18 | 1,800 | ~132 split | High mobility; low Heavy answer |
| 4 Razor + 2 Strike + Jet | 895/255/2 | 18 | 1,680 | ~116+ siege state | Surge/siege timing |
| 3 Infiltrator + 3 Razor + 2 Jet | 1,135/340/6 | 22 | 2,070 | ~157 split | Heavy-kill pressure |
| Mothership + 3 Razor + 2 Jet + Strike | 1,115/365/7 | 22 | 2,520 | ~117 + support | Large centralized investment |

Surge improves each force temporarily but does not solve an absent counter class.

## MARTIANS

| Composition | O/E/C | OC | HP | Relevant DPS | Reading |
|---|---|---:|---:|---:|---|
| 5 Scooter + 3 Double + Skiff | 765/145/0 | 10 | 1,450 | ~64 | Cheap raid; no Heavy answer |
| 4 Cruiser + 2 Recon + 2 Protector | 1,380/270/6 | 26 | 3,320 | ~168 split | Elastic Tube Defense Web |
| 2 Recon + 3 Protector + Searcher | 1,330/300/11 | 24 | 3,040 | ~144 | Expensive Walker Control |
| 4 Scooter + 3 Cruiser + Protector + 2 Double | 1,160/205/2 | 19 | 2,720 | ~134 | Network Pincer |

No Martian army gains maximum:

- mobility;
- anti-air;
- anti-heavy;
- siege;

simultaneously.

---

# PART XLVIII — EARLY-GAME BALANCE

## ROCK RAIDERS

Loader Dozer is difficult for light attackers to efficiently kill, but:

- its 0.9 range prevents pursuit;
- it cannot threaten air;
- it cannot catch Razor/Scooter in open space.

It protects economic territory rather than automatically hunting mobile raiders.

## ASTRONAUTS

T3 Escort provides the most conventional early screen.

Mono Jet can threaten workers but dies quickly to real AA.

Survey Refit substantially weakens T3 combat output.

## ALIENS

Razor harassment kills a worker in roughly 7s if uncontested.

It cannot efficiently fight:

- Loader;
- Claw;
- other durable lines.

Alien Jet gives air pressure but loses direct air superiority to Mission Fighter once that specialist appears.

## MARTIANS

Jet Scooter is a potent low-cost worker threat.

Its 180 HP and Light class mean conventional anti-light units kill it efficiently.

Early Red Planet Cruiser is a stronger fight commitment but at:

- 150 Ore;
- 3 OC;
- lower speed;

and cannot participate in Tube transfer.

## EARLY DEFENSE RESULT

No opening combat unit is close to unanswerable.

Workers remain vulnerable without becoming one-volley casualties.

---

# PART XLIX — MID-GAME BALANCE

The mid-game roster obtains clear reasons to exist.

**Granite Grinder:** efficient Heavy contact counter.

**Claw-Tank:** anti-light mobile line.

**Switch Fighter:** tactical state flexibility.

**Mission Fighter:** dedicated air superiority.

**Alien Strike:** mobile siege.

**Alien Infiltrator:** anti-heavy state/information craft.

**Red Planet Cruiser:** reliable Martian generalist.

**Recon-Mech:** mobile detection/AA.

**Red Planet Protector:** anti-heavy positional anchor.

The largest danger is Astronaut breadth; this is contained through:

- production specialization;
- Energy/Crystal costs;
- weak wrong-role efficiency;
- dedicated tech prerequisites;
- no universal support aura.

---

# PART L — ADVANCED-GAME BALANCE

## CHROME CRUSHER

Powerful only if delivered into contact.

Air/range/mobility punish it.

## TUNNEL TRANSPORT

Provides strategic mobility at the cost of:

- 300 Ore;
- 110 Energy;
- 3 Crystals;
- 5 OC;
- zero normal DPS.

## MT-101

Strong mobile anti-heavy but remains contact-bound.

## MT-201

Devastating protected structure drill, immobile in siege state.

## MX-81

Information/support rather than superweapon.

## ALIEN MOTHERSHIP

1,200 HP is large, but:

- it costs 420/160/6;
- consumes 8 OC;
- has only 18 raw General DPS;
- concentrates Surge/transport value into one AA-vulnerable target.

## EXCAVATION SEARCHER

Advanced Martian positional utility/siege.

Massive but slow.

## MATURE MARTIAN NETWORK

Moves only light eligible units through Tubes.

Heavy army still follows conventional routes.

Advanced technology therefore escalates strategic possibilities rather than replacing earlier units.

---

# PART LI — SIX MATCHUP BALANCE AUDIT

## 1. ROCK RAIDERS VS ASTRONAUTS

**Healthy tension:** Raider serviced durability vs Astronaut composition/range.

**Dangerous interaction:** Astronaut air can bypass slow Raider heavy formations.

**Required counters:** Cutter Mast must make unescorted air economically dangerous; Mission Fighters remain superior to Raider transport aircraft.

**Map assumption:** major Raider routes ≥12 cells; multiple pressure routes.

**Safeguard:** Worksite repair under fire is reduced; Raiders cannot simply absorb unlimited ranged pressure.

## 2. ROCK RAIDERS VS ALIENS

**Healthy tension:** sustain vs tempo.

**Danger:** 18-second Surge may delete isolated service/economy assets.

**Required counters:** Loader/Barrier punish Razors that commit; Cutter punishes Jets/Strike flight; Granite/Chrome punish walker/heavy ETX if contact is made.

**Safeguard:** Surge changes cadence, not movement/HP/range; failed Surge consumes 50 Charge.

## 3. ROCK RAIDERS VS MARTIANS

**Healthy tension:** developed territory vs network geometry.

**Danger:** displacement could theoretically prevent contact machines from functioning.

**Safeguard:** Heavy displacement is only 0.5–1 cell and Stability prevents chains; Massive Chrome is immune.

Raider siege strongly threatens Stations/Links but must physically traverse heavy routes.

## 4. ASTRONAUTS VS ALIENS

**Healthy tension:** prepared response vs attack timing.

Mission Fighter has a decisive direct AA efficiency advantage over Alien Jet.

Alien Jet instead brings:

- earlier/aggressive scouting;
- speed;
- Surge cadence.

Infiltrator pressures MT-101/MT-201, forcing Claw/T3/light support.

Astronaut Refit downtime remains exploitable during Alien timing windows.

## 5. ASTRONAUTS VS MARTIANS

**Healthy tension:** broad mobility vs prepared network mobility.

Dropship can create pressure away from Tube routes.

Tube networks reinforce predictable exits.

Claw-Tanks screen lighter displacement threats.

Protector/Searcher can disrupt heavy Astronaut advances but cannot move MT-201/MT-101 large distances.

Martian AA prevents casual air dominance without erasing air play.

## 6. ALIENS VS MARTIANS

**Healthy tension:** offensive redirection vs prepared positional redirection.

Razor pressure can attack Tube infrastructure.

Martians can reinforce through surviving Stations.

Surge can overwhelm one location but cannot accelerate movement outside the zone.

Alien Strike's deployed state is vulnerable to Searcher/Protector manipulation only within their limited ground interaction ranges.

Mothership cannot be displaced because it is true air.

---

# PART LII — MIRROR MATCHES

## ROCK RAIDER MIRROR

Anti-trench safeguards:

- starting Ore depletes;
- repair consumes resources;
- Worksite repair is reduced under fire;
- Chrome/Granite siege breaks static positions;
- static defense has limited range.

## ASTRONAUT MIRROR

Anti-refit-loop safeguards:

- refit cost;
- refit downtime;
- 20s Configuration Lock;
- dedicated specialist differences remain large enough that constant swapping loses tempo.

## ALIEN MIRROR

Anti-first-Surge-decides-game safeguards:

- no HP/range/speed Surge bonus;
- baseline combat remains viable;
- 50 Charge has meaningful recovery time;
- Surge zones do not stack.

## MARTIAN MIRROR

Anti-network-stalemate safeguards:

- Heavy units cannot Tube;
- Links have 320-HP damageable spans;
- Stations are siegeable;
- exit recovery prevents instant ambush burst;
- Stability prevents mutual control-lock chains.

---

# PART LIII — FOCUS-FIRE TESTS

Representative four Red Planet Cruisers provide approximately 76.5 raw General DPS.

Against actual armor/class values:

| Target | Effective group DPS | Approx. TTK |
|---|---:|---:|
| Worker — 110 HP | 84.2 | 1.3s |
| Razor — 210 Light/A1 | 77.1 | 2.7s |
| Cruiser — 400 Medium/A2 | 70.4 | 5.7s |
| MT-101 — 700 Heavy/A4 | 57.9 | 12.1s |
| Sentinel — 800 Fortified/A2 | 49.3 | 16.2s |
| Mission Vehicle Bay — 1,700 Structure/A3 | 53.9 | 31.6s |
| MB-01 — 2,700 Fortified/A4 | 45.0 | 60.0s |

This demonstrates:

- groups can rapidly remove exposed light targets;
- Heavy machines survive meaningful focus;
- early/generalist groups do not instantly erase command structures.

## SIEGE COMPARISON

Approximate TTK against 1,700-HP Structure/A3 production:

- **1 MT-201 deployed:** ~29s
- **2 MT-201:** ~14.6s
- **1 Chrome Crusher:** ~35s
- **2 Chrome Crushers:** ~17.6s
- **1 Excavation Searcher braced:** ~44.6s
- **2 Searchers:** ~22.3s
- **1 Alien Strike deployed:** ~51s
- **2 Alien Strikes:** ~25.7s.

The differences correspond to their battlefield risks:

MT-201/Chrome must fight close.

Searcher operates somewhat behind the line and controls position.

Alien Strike has 9-cell range and superior strategic relocation.

---

# PART LIV — MOBILITY / RANGE TESTS

## GRANITE GRINDER VS RANGED OPPONENTS

Granite:

- speed 1.15;
- contact range 1.0.

Cruiser:

- speed 1.50;
- 4.8 range.

A Cruiser can kite a lone Grinder on open ground.

This is intentional.

Granite's intended prey—Heavy machinery—is slower and/or more committed.

Raiders answer mobile ranged kiting with:

- Loader screening;
- terrain;
- Tunnel Transport;
- static coverage;
- positioning.

## LOADER DOZER VS LIGHT SKIRMISHER

Loader cannot catch Razor/Scooter in open space.

It instead makes the economic area difficult to enter.

A light skirmisher must stop or path through constrained space to produce damage.

## MT-101 VS RANGED SPECIALIST

MT-101 cannot catch a properly withdrawing Infiltrator Craft or fast light skirmisher.

Its intended target is Heavy frontline and structure approach.

## RAZOR HARASSMENT

2.30 speed allows clean disengagement from most frontline vehicles.

Static defense and faster interceptors remain required.

This mobility is already paid for through low durability/Heavy inefficiency.

## MISSION FIGHTER VS FLEEING AIRCRAFT

Mission Fighter speed:

**3.15**

Alien Jet:

**3.10**

The dedicated interceptor therefore slowly closes on a fleeing Jet after gaining a legal pursuit line.

It cannot instantly run down every air unit.

## MARTIAN DISPLACEMENT

Stability guarantees repeated displacement cannot indefinitely reset closing distance.

---

# PART LV — STATIC DEFENSE TESTS

Using Ground Sentinel as reference:

## VS ONE RAZOR

Sentinel effective DPS against Razor:

~23.5.

Razor TTK:

~8.9s.

The lone harassment unit should withdraw.

## THREE RAZORS VS SENTINEL

Their Light damage is highly inefficient against Fortified structure.

A frontal unsupported attack requires roughly 46s before accounting for the Sentinel killing attackers.

This is intentionally bad composition.

## MOBILE GENERALIST ASSAULT

Two Cruisers can pressure the Sentinel but cannot effortlessly ignore it.

Additional army positioning or focus is required.

## CORRECT SIEGE

One deployed Alien Strike attacks from:

- 9 range;
- outside Sentinel Ground's 6 range.

It destroys the Sentinel in approximately 21s if protected.

## MASS STATIC DEFENSE

Defense occupies:

- Ore;
- Energy demand;
- build space;

without creating:

- workers;
- mobile army;
- expansion;
- map control.

Mass defense therefore yields the rest of the map economically.

---

# PART LVI — ALIEN SURGE BENCHMARKS

Representative force:

- 4 Razor Skimmers;
- 2 deployed Alien Strikes.

Raw firing output:

**without Surge:** approximately 97.1 DPS.

**with Surge:** approximately 121.4 DPS.

Theoretical additional pre-modifier damage during an uninterrupted 18s window:

**~437 damage.**

This is approximately:

- one substantial light/medium machine;
- or part of a heavy unit;

rather than an entire army.

## CHARGE RECOVERY

With 4 committed Crystals:

- Charge generation = 1.6/s;
- replacing 50 spent Charge takes **31.25s**.

A failed Surge therefore gives the opponent a meaningful counter-pressure period.

Target playtest expectation:

- stable equal-economic Alien force outside Surge: approximately even;
- well-positioned equal force during Surge: clear 60–65% local advantage;
- failure to kill meaningful value during the window: materially inefficient.

---

# PART LVII — MARTIAN DISPLACEMENT BENCHMARKS

| Target | Deflector | Searcher Clamp | Chain-control result |
|---|---:|---:|---|
| Worker | 2.5 cells | 3.0 | Next displacement ×0.25 for 8s |
| Light hover | 2.5 | 3.0 | Same |
| Medium vehicle | 1.5 | 2.0 | Same |
| Heavy machine | 0.75 | 1.0 | Same |
| Deployed eligible Heavy | 0.375 | 0.5 | Same |
| Massive ground machine | 0 | 0 | Immune |
| True-air Massive | Not legal | Not legal | Immune |

Displacement changes:

- frontage;
- target accessibility;
- retreat geometry.

It does not remove player control.

---

# PART LVIII — REPAIR BENCHMARKS

## ONE WORKSITE CREW VS RAZOR DAMAGE ON LOADER

Razor effective DPS vs Medium/A2 Loader:

~10.35.

Crew Worksite repair while target is under fire:

**6 HP/s** after the 60% combat factor.

Net damage remains:

~4.35 HP/s.

Repair materially extends survival but does not cancel the attacker.

## THREE WORKSITE CREW

Stacked nominal:

10 + 7 + 4 = 21 HP/s.

Under fire:

**12.6 HP/s.**

Three economic workers can approximately neutralize one light attacker, which is acceptable because:

- three workers stop mining;
- they cluster in danger;
- two or more attackers overcome them.

## SPECIALIST DAMAGE

An Infiltrator Walker or similar Breach specialist produces well over 25 effective DPS against appropriate machines.

Worker repair cannot keep pace.

## DEDICATED SERVICE

Vehicle Bay:

28 HP/s normally.

14 under direct attack.

This rewards retreating to the Worksite rather than holding indefinitely in direct combat.

## SIEGE

Normal siege DPS substantially exceeds practical repair rates.

Dedicated siege therefore forces bases backward.

---

# PART LIX — AIR / GROUND VALUE CHECK

Air pays a strategic mobility premium.

Examples using diagnostic CRV:

**T3-Trike**

- ~117.5 CRV;
- 16.36 relevant raw DPS;
- can hold ground.

**Mono Jet**

- ~117.5 CRV;
- 10 ground DPS;
- cannot hold ground;
- gains true-air mobility.

**Mission Fighter**

- ~217.5 CRV;
- 24.44 AA DPS;
- essentially no ground value.

**MX-71**

- ~285 CRV;
- only 9.6 raw DPS;
- purchased primarily for transport/recon.

**Mothership**

- ~800 CRV;
- 18 raw DPS;
- purchased primarily for carrier/Surge support.

True air is therefore not costed as if unrestricted movement were free.

---

# PART LX — ECONOMIC HARASSMENT CHECK

Phase 04 workers generally cost 50 Ore and 16s.

## ONE WORKER KILL

Value:

- 50 Ore replacement;
- 16s replacement opportunity;
- lost mining during replacement;
- temporary worker evacuation.

A single kill is tactically meaningful but does not necessarily justify losing a 90–105-Ore harassment unit.

## TWO WORKER KILLS

Usually a clearly acceptable trade if the raider then dies.

The direct 100 Ore replacement cost plus mining downtime approaches/exceeds early harassment investment.

## THREE OR MORE

Exceptional harassment result.

## ZERO KILLS

A raid can still be useful if it:

- forces 4–6 workers to retreat for 8–12s;
- interrupts a Crystal operation;
- forces static-defense expenditure;
- reveals tech;
- delays expansion.

Harassment is therefore not reduced to kill-cost arithmetic.

---

# PART LXI — SNOWBALL & COMEBACK CHECK

There is no hidden comeback modifier.

## ARMY WIPE

A wiped player retains recovery opportunity if:

- production survives;
- workers survive;
- expansions remain.

Replacement normally requires real build time and resources.

## WORKER LOSS

2–4 losses are recoverable.

Large worker-line collapse produces major macro disadvantage but does not directly delete structures.

## GENERATOR LOSS

Energy reserve delays immediate shutdown.

Brownout still gives the attacker real value.

## RESONANCE CORE LOSS

Alien Charge capacity/generation and committed Crystal risk create a serious but not automatically terminal setback.

## TUBE SEGMENTATION

Disconnected Martian settlements remain locally functional under Phase 04.

## WORKSITE DISRUPTION

Raider structures remain locally functional but lose shared sustain/logistics.

## SERVICE HUB LOSS

Astronaut units retain full base combat stats; they lose repair/refit convenience.

Recovery therefore emerges from surviving infrastructure rather than artificial rubber-banding.

---

# PART LXII — APM / MICRO BUDGET

Normal mixed-army combat should expose approximately:

## ROCK RAIDERS

**0–1 faction-specific combat commands**

Typical:

- transport load/unload.

Excavation is strategic, not constant battle micro.

## ASTRONAUTS

**1–2**

Typical:

- Switch transformation;
- MT-201 deployment.

Solar deployment/refit usually occurs between fights.

## ALIENS

**2–3**

Typical:

- Surge;
- Strike transformation;
- Infiltrator transformation.

No individual Razor/Jet active ability is required.

## MARTIANS

**1–2**

Typical:

- Protector Stance;
- Searcher Clamp.

Tube transfer is strategic logistics rather than constant unit-by-unit combat input.

Shared RTS:

- move;
- attack;
- focus;
- hold;
- retreat;

remains the bulk of battle control.

---

# PART LXIII — READABILITY AUDIT

At normal RTS distance:

**Fast vs slow:** communicated through locomotion speed and animation cadence.

**Light vs heavy:** silhouette, footprint and acceleration.

**Contact vs ranged:** large visible tools versus projectile mounts.

**Anti-air:** upward tracking, distinct firing direction.

**Siege:** visible anchoring/reconfiguration.

**Surge:** whole eligible Alien machine clearly enters high-output resonance state.

**Martian control:** articulated physical push/pull.

**Damage:** missing pieces/sparks at universal health thresholds.

**Repair:** visible reconstruction/tool activity.

No fundamental class relationship relies only on a tooltip.

---

# PART LXIV — SOURCE-FIDELITY COMBAT AUDIT

## ROCK RAIDERS

Combat remains based on:

- drills;
- loaders;
- cutting;
- transport;
- repair;
- industrial defense.

They have not become a conventional armored army.

## ASTRONAUTS

Field Systems retain:

- Rover;
- T3;
- Solar Explorer;
- field reconnaissance/support.

Mission Systems provide:

- fighter;
- dropship;
- heavy drilling;
- specialized combat.

Both lineages remain tactically relevant.

## ALIENS

They remain:

- black/lime technological machines;
- crystal-powered;
- transforming;
- mobile.

No biological swarm mechanic has been added.

## MARTIANS

Their combat remains:

- articulated;
- mechanical;
- hover/walker/network-based;
- physically manipulative.

They do not become an energy-burst reskin of Aliens.

The source-derived roles established in Phase 03 remain intact.

---

# PART LXV — NO-OBSOLESCENCE AUDIT

**Hover Scout:** cheapest Raider geological information.

**Loader Dozer:** cheaper durable anti-light screen than later drills.

**Rover:** cheapest broad Astronaut ground reconnaissance.

**T3-Trike:** low-cost Escort or Survey module.

**Mono Jet:** cheapest true-air reconnaissance/harassment.

**Mission Fighter:** dedicated AA remains superior to Switch Fighter.

**Claw-Tank:** anti-light frontline remains more efficient than MT-101 against light armies.

**Razor Skimmer:** cheapest Alien surface harassment even after ETX tech.

**Alien Jet:** dedicated interceptor stays relevant beside Strike/Mothership.

**Double Hover:** low-cost scout and Tube-eligible information unit.

**Jet Scooter:** cheapest Martian combat unit and Tube raider.

**Red Planet Cruiser:** affordable conventional frontline between light Tube units and expensive walkers.

No advanced technology strictly replaces these assets.

---

# PART LXVI — ECONOMIC BASELINE AMENDMENTS

# NONE

Phase 04 economic values remain unchanged.

This is an explicit Phase 06 conclusion, not an omission.

---

# PART LXVII — FINAL UNIT BALANCE TABLES

## ROCK RAIDERS

| Unit | Cost | OC | HP | Armor | Speed | G DPS | A DPS | Range | Role | Primary target | Main weakness | Special |
|---|---|---:|---:|---|---:|---:|---:|---:|---|---|---|---|
| Crew | 50/0/0 |1|110|Pers A0|1.35|5|—|0.8|Worker/repair|—|combat|Worksite repair |
| Hover Scout |75/10/0|1|120|Light A0|2.25|4|—|3.0|Scout|information|combat|Detector8 |
| Drill Craft |95/15/0|2|190|Light A1|1.55|9.6|—|0.85|Excavation|terrain|units|+geology dmg |
| Rapid Rider |90/10/0|2|170|Light A0|2.10|—|—|—|Transport|—|combat|Crew cap4 |
| Loader |125/15/0|3|360|Med A2|1.30|13.3/19.1|—|0.9|Frontline|Light|kite/Heavy|Cutter |
| Grinder |190/35/1|4|480|Heavy A3|1.15|22.7/28.3|—|1.0|Anti-heavy|Heavy|kite/air|drill ramp |
| Chrome |330/90/4|6|880|Massive A5|0.92|34.4|—|1.05|Siege|Structures|air/range|+25% Heavy/Massive |
| Tunnel |300/110/3|5|650|Massive A3|1.75|—|—|—|Transport|—|AA|cap8 |

## ASTRONAUTS

| Unit | Cost | OC | HP | Armor | Speed | G DPS | A DPS | Range | Role | Primary target | Weakness | Special |
|---|---|---:|---:|---|---:|---:|---:|---|---|---|---|---|
| Crew |50/0/0|1|110|Pers A0|1.40|5|—|0.8|Worker|—|combat|repair6 |
| Rover |70/5/0|1|130|Light A0|2.15|5|—|3.3|Scout|info|combat|D6 |
| T3 Escort |110/15/0|2|260|Light A1|1.70|16.36|—|4.5|Light screen|Light|Heavy|Refit |
| T3 Survey |same|2|260|Light A0|1.85|5.71|—|3.5|Survey|info|combat|Sight13/D8 |
| Mono Jet |100/35/0|1|150|Light A0|2.85|10|—|4.0|Air harass|workers|AA|— |
| Solar |180/40/1|4|520|Heavy A2|1.10|6.67|—|4.0|Support|—|focus|Service deploy |
| Mission Fighter |140/55/1|2|240|Light A1|3.15|—|24.44|5.5|Interceptor|Air|ground/AA|— |
| Switch Fighter |170/60/1|3|320|Med A2|1.75/2.45|19.05/13.91|13.91|4.5|Flexible|Light/mobility|specialists|Transform |
| Mining Platform |170/25/0|3|400|Heavy A2|0.95|5.33|—|3.5|Economy|—|harass|Refit |
| MX-71 |200/70/1|4|420|Heavy A2|2.15|9.6|9.6|4.5|Transport|—|AA|cap8 |
| Claw-Tank |180/35/0|3|460|Heavy A3|1.30|21.67|—|3.2|Frontline|Light/Med|Heavy|small splash |
| MT-101 |280/70/2|5|700|Heavy A4|1.08|28.15|—|1.0|Anti-heavy|Heavy|range/air|contact |
| MT-201 |320/90/3|6|760|Heavy A4|0.92|10 /41.33 siege|—|3.5/1.2|Siege|Structures|flank/air|Deploy |
| MX-81 |380/130/4|6|720|Massive A3|1.90|13.33|13.33|5.0|Support|info|AA|Sight15/D10 |

## ALIENS

| Unit | Cost | OC | HP | Armor | Speed | G DPS | A DPS | Range | Role | Target | Weakness | Special |
|---|---|---:|---:|---|---:|---:|---:|---|---|---|---|---|
| Servitor |50/5/0|1|110|Light A0|1.65|—|—|—|Worker|—|harass|No weapon |
| Alien Jet |95/35/0|2|180|Light A0|3.10|7.27|18.95|3.8/5.0|Interceptor|Air|AA/Mission Fighter|Surge |
| Razor |105/25/0|2|210|Light A1|2.30|12.5|—|4.0|Harass|Worker/Light|frontline|Surge |
| Strike |190/60/1|4|330|Med A1|2.35/0|11.2/23.53 siege|11.2|4.5/9|Siege|Structures|flank|Transform/Surge |
| Infiltrator |210/65/2|4|360|MedA2/HeavyA3|2.05/1.25|8.33/27.2|—|4/3.5|Info/antiheavy|Heavy|Light focus|Transform/Surge |
| Mothership |420/160/6|8|1200|Massive A4|1.65|18|18|5.5|Carrier/support|—|heavy AA|Surge anchor |

## MARTIANS

| Unit | Cost | OC | HP | Armor | Speed | G DPS | A DPS | Range | Role | Target | Weakness | Special |
|---|---|---:|---:|---|---:|---:|---:|---|---|---|---|---|
| Worker Robot |50/0/0|1|115|Light A1|1.30|4|—|0.8|Worker|—|harass|Tube |
| Double Hover |65/10/0|1|120|Light A0|2.25|—|—|—|Scout|info|combat|Tube |
| Jet Scooter |90/15/0|1|180|Light A0|2.40|12.73|—|3.8|Harass|Worker/Light|Heavy|Tube |
| Aero Skiff |120/40/0|2|190|Light A0|2.60|—|—|—|Air transport|—|AA|cap2 |
| Cruiser |150/25/0|3|400|Med A2|1.50|19.13|—|4.8|Generalist|Medium|Breach|— |
| Recon-Mech |170/35/1|3|300|Med A2|1.32|7.14|20|4.5/6.5|AA/recon|Air|Heavy|D9 |
| Protector |220/50/2|4|560|Heavy A3/A4|1.18|22.86/25.6|—|2.6/3.2|Anti-heavy|Heavy|range/flank|Stance/Sweep |
| Searcher |330/80/3|6|760|Massive A4|0.98|13.33/27.06 siege|—|2.5/4.5|Siege/control|Structures|focus/air|Clamp |

---

# PART LXVIII — FINAL DEFENSE / STRUCTURE TABLES

The authoritative **Structure Durability Table** is Part XX.

The authoritative **Static Defense Table** is Part XIX.

These tables are the final prototype implementation baseline.

---

# PART LXIX — DAMAGE MATRIX

Authoritative implementation matrix is Part III and is repeated here for implementation convenience.

| Type | Pers | Light | Medium | Heavy | Massive | Structure | Fortified |
|---|---:|---:|---:|---:|---:|---:|---:|
| Light |1.25|1.30|0.90|0.70|0.60|0.60|0.50|
| General |1.10|1.05|1.00|0.90|0.80|0.80|0.70|
| Breach |0.75|0.85|1.10|1.55|1.45|1.10|1.00|
| Siege |0.55|0.65|0.80|1.00|1.15|1.60|1.75|
| Anti-Air |0.90|1.40|1.25|1.10|0.95|0.30|0.25|
| Control |0.90|1.00|0.85|0.70|0.50|0.35|0.25|

Armor:

**Final × (1 − Armor × 0.04).**

---

# PART LXX — MOVEMENT TABLE

| Unit | Movement | Speed | Accel | Turn | Footprint |
|---|---|---:|---|---|---|
| Rock Raider Crew | Foot |1.35|Quick|Agile|Tiny |
| Hover Scout | Hover |2.25|Snap|Agile|Small |
| Drill Craft | Wheeled |1.55|Quick|Quick|Small |
| Rapid Rider | Amphib skimmer |2.10|Quick|Agile|Small |
| Loader Dozer | Wheeled |1.30|Heavy|Standard|Medium |
| Granite Grinder | Walker |1.15|Heavy|Standard|Medium |
| Chrome Crusher | Wheeled |0.92|Massive|Heavy|Large |
| Tunnel Transport | True air |1.75|Heavy|Heavy|Huge |
| Expedition Crew | Foot |1.40|Quick|Agile|Tiny |
| Rover | Wheeled |2.15|Snap|Agile|Small |
| T3 Escort | Rough wheeled |1.70|Quick|Quick|Medium |
| T3 Survey | Rough wheeled |1.85|Quick|Quick|Medium |
| Mono Jet | True air |2.85|Snap|Agile|Small |
| Solar Explorer | Wheeled |1.10|Heavy|Heavy|Large |
| Mission Fighter | True air |3.15|Snap|Agile|Small |
| Switch Ground | Wheeled |1.75|Quick|Quick|Medium |
| Switch Flight | True air |2.45|Quick|Quick|Medium |
| Mining Platform | Tracked |0.95|Heavy|Heavy|Large |
| MX-71 | True air |2.15|Standard|Standard|Large |
| Claw-Tank | Tracked |1.30|Heavy|Standard|Large |
| MT-101 | Wheeled |1.08|Heavy|Heavy|Large |
| MT-201 | Walker |0.92|Massive|Heavy|Huge |
| MX-81 | True air |1.90|Heavy|Heavy|Huge |
| ETX Servitor | Hover |1.65|Quick|Agile|Tiny |
| Alien Jet | True air |3.10|Snap|Agile|Small |
| Razor Skimmer | Hover |2.30|Snap|Agile|Small |
| Alien Strike | True air |2.35|Quick|Quick|Medium |
| Infiltrator Craft | Hover |2.05|Quick|Quick|Medium |
| Infiltrator Walker | Walker |1.25|Heavy|Standard|Medium |
| Mothership | True air |1.65|Heavy|Massive|Huge |
| Worker Robot | Walker |1.30|Quick|Quick|Tiny |
| Double Hover | Hover |2.25|Snap|Agile|Small |
| Jet Scooter | Hover |2.40|Snap|Agile|Small |
| Aero Skiff | True air |2.60|Snap|Agile|Small |
| Red Planet Cruiser | Hover |1.50|Quick|Quick|Medium |
| Recon-Mech | Walker |1.32|Quick|Standard|Medium |
| Protector | Walker |1.18|Heavy|Standard|Large |
| Excavation Searcher | Walker |0.98|Massive|Heavy|Huge |

---

# PART LXXI — WEAPON TABLE

| ID | Owner | Targets | Dmg | Type | CD | DPS | Range | Min | Behavior / speed | Splash / special |
|---|---|---|---:|---|---:|---:|---:|---:|---|---|
| RR_TOOL | Crew | G |6|General|1.20|5.00|0.8|0|Contact|—|
| RR_SCOUT | Hover Scout |G|6|General|1.50|4.00|3.0|0|Pulse12|—|
| RR_DRILL_LIGHT | Drill Craft |G|12|Siege|1.25|9.60|0.85|0|Contact|+100% authored geology |
| RR_SCOOP | Loader |G|18|General|1.35|13.33|0.9|0|Contact|baseline |
| RR_CUTTER | Loader upgrade |G|22|Light|1.15|19.13|0.9|0|Contact|replaces Scoop |
| RR_GRANITE | Grinder |G|34|Breach|1.50|22.67|1.0|0|Contact|+25% after2.5s |
| RR_CHROME | Chrome |G|55|Siege|1.60|34.38|1.05|0|Contact|+25% Heavy/Massive |
| AST_TOOL | Expedition Crew |G|6|General|1.20|5.00|0.8|0|Contact|—|
| AST_ROVER | Rover |G|7|General|1.40|5.00|3.3|0|Pulse12|—|
| AST_T3_ESC | T3 Escort |G|18|Light|1.10|16.36|4.5|0|Fast12|—|
| AST_T3_SURV | T3 Survey |G|8|General|1.40|5.71|3.5|0|Pulse12|—|
| AST_MONO | Mono Jet |G|12|Light|1.20|10.00|4.0|0|Pulse12|—|
| AST_SOLAR | Solar Explorer |G|10|General|1.50|6.67|4.0|0|Pulse10|—|
| AST_FIGHTER_AA | Mission Fighter |A|22|AA|0.90|24.44|5.5|0|Guided12|—|
| AST_SWITCH_G | Switch Ground |G|20|Light|1.05|19.05|4.5|0|Fast12|—|
| AST_SWITCH_F | Switch Flight |G+A|16|General|1.15|13.91|4.5|0|Pulse12|—|
| AST_MMP | Mining Platform |G|8|General|1.50|5.33|3.5|0|Pulse10|—|
| AST_MX71 | MX-71 |G+A|12|General|1.25|9.60|4.5|0|Pulse12|—|
| AST_CLAW | Claw-Tank |G|26|Light|1.20|21.67|3.2|0|Mechanical/pulse|Splash0.75 |
| AST_MT101 | MT-101 |G|38|Breach|1.35|28.15|1.0|0|Contact|—|
| AST_MT201_T | MT-201 travel |G|14|General|1.40|10|3.5|0|Pulse10|—|
| AST_MT201_D | MT-201 drill |G|62|Siege|1.50|41.33|1.2|0|Contact|—|
| AST_MX81 | MX-81 |G+A|16|General|1.20|13.33|5.0|0|Pulse12|—|
| ALI_JET_AA | Alien Jet |A|18|AA|0.95|18.95|5.0|0|Guided12|Surge |
| ALI_JET_G | Alien Jet |G|8|Light|1.10|7.27|3.8|0|Pulse12|Surge |
| ALI_RAZOR | Razor |G|15|Light|1.20|12.50|4.0|0|Projectile10|Surge |
| ALI_STRIKE_F | Strike flight |G+A|14|General|1.25|11.20|4.5|0|Pulse12|Surge |
| ALI_STRIKE_S | Strike siege |G|40|Siege|1.70|23.53|9.0|3.0|Slow positional6|Splash0.75; Surge |
| ALI_INFIL_C | Infil craft |G|10|General|1.20|8.33|4.0|0|Pulse12|Surge |
| ALI_INFIL_W | Infil walker |G|34|Breach|1.25|27.20|3.5|0|Projectile8|Surge |
| ALI_MOTHER | Mothership |G+A|18|General|1.00|18|5.5|0|Pulse12|Surge |
| MAR_WORK | Worker Robot |G|5|General|1.25|4|0.8|0|Contact|—|
| MAR_SCOOT | Jet Scooter |G|14|Light|1.10|12.73|3.8|0|Projectile10|—|
| MAR_CRUISER | Cruiser |G|22|General|1.15|19.13|4.8|0|Projectile9|—|
| MAR_RECON_AA | Recon-Mech |A|20|AA|1.00|20|6.5|0|Guided10|—|
| MAR_RECON_G | Recon-Mech |G|10|General|1.40|7.14|4.5|0|Pulse|—|
| MAR_PROT_M | Protector mobile |G|32|Breach|1.40|22.86|2.6|0|Mechanical|—|
| MAR_PROT_S | Protector stance |G|32|Breach|1.25|25.60|3.2|0|Mechanical|frontal120° |
| MAR_SEARCH_M | Searcher mobile |G|20|General|1.50|13.33|2.5|0|Mechanical|—|
| MAR_SEARCH_S | Searcher braced |G|46|Siege|1.70|27.06|4.5|1.5|Mechanical extension|—|
| DEF_RR_G | Crusher Barrier |G|20|Light|1.20|16.67|1.25|0|Mechanical|—|
| DEF_RR_A | Cutter Mast |A|24|AA|1.00|24|7.5|0|Beam|—|
| DEF_AST_G | Sentinel Ground |G|28|General|1.20|23.33|6.0|0|Projectile10|—|
| DEF_AST_A | Sentinel Air |A|24|AA|0.90|26.67|7.5|0|Guided12|—|
| DEF_ALI_G | Node Ground |G|26|General|1.10|23.64|5.5|0|Projectile9|Shunt eligible |
| DEF_ALI_A | Node Air |A|24|AA|0.90|26.67|7.0|0|Beam|Shunt eligible |
| DEF_MAR_CTRL | Deflector Arm |G|14|Control|8.0|1.75|4.5|0|Mechanical pulse|Displacement |
| DEF_MAR_AA | Aero Guard |A|22|AA|0.90|24.44|7.25|0|Guided10|—|
| NEUT_RMON | Rock Monster |G|32|General|1.60|20|1.0|0|Mechanical|Splash0.75 |

---

# PART LXXII — COMBAT STATE TABLE

| State | Activation | Transition/duration | Cooldown/lock | Move | Attack | Stat change | Interruption |
|---|---|---|---|---|---|---|---|
| T3 Mission Refit | Service action | 10–18s depending module ownership | 20s Configuration Lock | No | No | Escort↔Survey complete stat package | Facility loss follows Phase04 cancellation |
| Mining Platform Refit | Service action | 15–25s |20s|No|No|Economic config only|Same |
| Sentinel Refit | Service action |15–20s|20s|—|No|Ground↔Air weapon|Cannot begin under attack |
| MX-41 Transform | Unit command |2.25s|8s|No|No|Ground↔true air|Cancel before40% |
| Solar Service Deploy | Unit command |3.0s/2.5s|8s|No|No|repair/refit access|Cancel before50% |
| MT-201 Drill Deploy | Unit command |3.5/3.0s|10s|No|No|Travel↔41.33 Siege DPS|Cancel before50% |
| Alien Strike Reconfigure | Unit command |2.8s; 1.96s Surged|8s|No|No|Air↔9-range siege|Air+ground targetable |
| Infiltrator Reconfigure | Unit command |2.0s;1.4 Surged|6s|No|No|Hover info↔Heavy walker|Cancel before40% |
| Alien Surge | Faction command |0.75 telegraph +18s|Charge-limited|Yes|Yes|Cooldown×0.80; ETX transform×0.70|Leaving zone removes cadence |
| Defense Resonance Shunt | Faction command |12s|Charge-limited|—|Yes|Node CD×0.75, track+30%|Ends with timer |
| Protector Stance | Unit command |2.4/2.0s|8s|No in stance|Yes|Armor+1; range/cadence; control resistance|Cancel before50% |
| Searcher Brace | Unit command |2.5/2.0s|10s|No|Yes|Siege weapon enabled|Cancel before50% |
| Searcher Clamp | Target command |Immediate actuation|14s|Brief stop|Attack cycle paused1s|Displacement|Stability prevents chain |
| Tube Transit | Station command |8–12s typical|Station throughput|No|No|Off-map transit|Break reroutes/returns |
| Tube Exit Recovery | Automatic |0.75s|—|60%|No|None afterward|Fully targetable |

---

# PART LXXIII — COUNTER REFERENCE

## ROCK RAIDERS — “WHAT I BUILD AGAINST…”

**Light/worker raiders:** Loader Dozer / Crusher Barrier.

**Heavy machines:** Granite Grinder.

**Massive/structures:** Chrome Crusher.

**Air:** Cutter Mast.

**Long-range mobile kiting:** use terrain, Loader screens, Tunnel Transport flanks rather than mass more drills.

**Enemy siege:** mobile Loader/Grinder flank supported by repair.

## ASTRONAUTS

**Light ground:** T3 Escort / Claw-Tank.

**Heavy:** MT-101.

**Static defense/structures:** MT-201.

**Air:** Mission Fighter / Air Sentinel.

**Fast mobile surface pressure:** Switch Fighter / T3.

**Need information:** Rover / Survey T3 / MX-81.

**Need sustain:** Solar Explorer.

## ALIENS

**Workers/Light:** Razor.

**Air:** Alien Jet.

**Heavy:** Infiltrator Walker.

**Structures:** Alien Strike.

**Need offensive timing:** Surge.

**Need strategic reach/support:** Mothership.

Aliens should not try to solve a missing counter merely by triggering Surge.

## MARTIANS

**Workers/light:** Jet Scooter.

**Normal ground line:** Cruiser.

**Air:** Recon-Mech / Aero Guard.

**Heavy:** Protector.

**Structures:** Searcher.

**Need rapid light reinforcement:** Aero Tubes.

**Need positional disruption:** Protector / Deflector / Searcher Clamp.

---

# PART LXXIV — PROTOTYPE BALANCE STATUS

## LOCKED SYSTEM VALUES

The following are structural canon:

- deterministic attack resolution;
- no universal accuracy/evasion RNG;
- HP + class + multiplicative Armor architecture;
- seven target classes;
- six damage families;
- air as movement/target layer rather than armor;
- no normal conventional stealth layer;
- no normal friendly fire;
- no projectile auto-retarget;
- attack-move chase limitation;
- role-aware formation;
- contact approach-slot system;
- transport passenger survival rule;
- Surge affects cadence and ETX transformation, not movement/HP/range;
- no Surge stacking;
- Massive immunity to Martian displacement;
- 8-second Stability after displacement;
- Worksite combat advantage comes through repair/service rather than damage aura;
- buildings retain function until destruction;
- no persistent blocking wreck fields.

Inherited LSVs remain:

- Surge cost **50 Charge**;
- Surge duration **18s**;
- Mission Refit 20-second Configuration Lock;
- Tube eligibility;
- Tube transfer architecture.

## BASELINE TUNING VALUES

Expected to be empirically tuned:

- unit HP;
- individual Armor Ratings;
- weapon damage;
- cooldowns;
- damage-matrix coefficients;
- ranges;
- speed;
- acceleration;
- turn rate;
- static-defense HP/DPS;
- repair throughput;
- displacement distances;
- transformation durations;
- sight/detection radii;
- hazard damage.

The prototype values in this document are nevertheless authoritative until superseded by later playtest revision.

---

# PART LXXV — CANON ESTABLISHED BY PHASE 06

Phase 06 establishes the following authoritative combat canon.

1. Standard combat is deterministic.

2. Normal competitive play contains no random miss/evasion system.

3. Durability uses HP + Target Class + multiplicative Armor Rating.

4. Target classes are:
   - Personnel;
   - Light Machine;
   - Medium Machine;
   - Heavy Machine;
   - Massive Machine;
   - Structure;
   - Fortified Structure.

5. Damage types are:
   - Light;
   - General;
   - Breach;
   - Siege;
   - Anti-Air;
   - Control.

6. Armor Rating reduces damage by 4% per level.

7. Specialist relationships are strong but not absolute.

8. Soft, Strong and Hard-ish counter standards are formally defined.

9. Combat TTK supports reaction and retreat while allowing real focus-fire lethality.

10. Attack ranges use a 0.7–10-cell combat scale compatible with Phase 05 corridors.

11. Normal combined ground movement is calibrated around Phase 05's 80–100-second representative map travel.

12. Acceleration and turn-rate classes make heavy machinery feel heavy without simulator-style controls.

13. Unit collision uses five standard footprint classes with short friendly compression and heavy-path priority.

14. Formation management is automatic and role-aware.

15. Attack-move uses limited predictable target acquisition/chasing.

16. Player direct-target orders always override automatic priority when legal.

17. Projectile travel and overkill are real.

18. Launched projectiles do not automatically retarget.

19. Splash is restrained and has no normal friendly fire.

20. Contact combat uses approach slots and no artificial engagement lock.

21. Granite Grinder is the Raider mid-game anti-heavy specialist.

22. Chrome Crusher is the Raider advanced direct breacher.

23. MT-101 is a mobile heavy combat drill.

24. MT-201 is the Astronaut primary close physical siege platform.

25. Alien Strike is mobile long-range reconfigurable siege.

26. Excavation Searcher is positional medium-range mechanical siege/control.

27. Static defenses deter small unsupported attacks but cannot replace mobile armies.

28. Command structures survive meaningful early pressure.

29. Economic/generator/network infrastructure remains deliberately more vulnerable.

30. Workers have approximately 110 HP and single harassment TTK around 7–11 seconds.

31. Rock Raiders possess the strongest developed-territory repair.

32. Repair rates are reduced during active incoming damage and consume economic resources.

33. Aliens do not receive Raider-like mobile sustain.

34. Martian survivability emphasizes repositioning/network recovery.

35. MX-41 transformation baseline is 2.25s.

36. MT-201 deploy baseline is 3.5s.

37. Solar Explorer deploy baseline is 3.0s.

38. Alien Strike reconfiguration is 2.8s baseline.

39. Infiltrator reconfiguration is 2.0s baseline.

40. Protector Stance and Searcher Brace are explicit mechanical states.

41. Alien Surge provides:
   - weapon cooldown ×0.80;
   - ETX transform/deploy time ×0.70;
   - no movement, HP, armor, range or per-hit damage increase.

42. Surge does not stack.

43. Martian displacement is class-scaled.

44. Massive units and structures are immune to ordinary hostile displacement.

45. Hostile displacement creates 8-second Stability.

46. Aero Tube arrival has a 0.75-second attack recovery and no invulnerability.

47. Rock Raider Industrial Advance is entirely emergent from:
   - durability;
   - service;
   - repair;
   - defenses;
   - reinforcement geometry.

48. True air occupies one abstract altitude layer.

49. Dedicated anti-air is deterministic and reliable.

50. Air pays a substantial mobility premium.

51. Transports cannot provide risk-free extraction.

52. Destroyed transports emergency-deploy passengers at 40% HP rather than randomly deleting them.

53. Standard competitive play has no full conventional invisibility system.

54. Environmental hazards are deterministic/predictable and no faction receives arbitrary immunity.

55. Rock Monster baseline:
   - 700 HP;
   - Heavy / Armor 3;
   - 32 General attack;
   - 1.6s cadence;
   - 14-cell leash;
   - no respawn;
   - no standard bounty.

56. Buildings remain operational until destruction.

57. Damage thresholds are visual/readability states, not hidden production debuffs.

58. Standard wreck collision clears rapidly.

59. No universal combat salvage system is introduced.

60. Phase 04's source-driven research tree is preserved.

61. No generic Damage I / II / III ladder is added.

62. Combat analysis requires **no Phase 04 cost amendment**.

63. The final authoritative unit baseline is contained in Parts XLV, LXVII and LXX.

64. The final authoritative weapon baseline is contained in Part LXXI.

# ALL 35 BUILDABLE UNITS HAVE COMPLETE COMBAT / MOVEMENT DATA.

# ALL 31 BUILDING / INFRASTRUCTURE ENTRIES HAVE DURABILITY / COMBAT DATA WHERE APPLICABLE.

# CANON SET COVERAGE REMAINS 64 / 64.

---

# EMPIRICAL PLAYTEST QUESTIONS

No ordinary design decision remains unresolved.

Later playtesting must measure rather than invent:

- whether baseline army TTK is approximately 10–15% too fast or slow;
- whether 4% Armor steps create sufficient perceptual difference;
- whether 1.15-speed Grinder reaches intended Heavy targets often enough;
- whether Razor worker pressure produces too much economic snowball;
- whether Raider active-under-fire repair should move within approximately ±20%;
- whether 25% Surge cadence creates the intended 60–65% temporary local engagement advantage;
- whether 8-second Martian Stability is sufficient to eliminate practical control chaining;
- whether static-defense HP is too favorable on particular map geometries;
- whether air/AA range relationships require approximately ±0.5-cell tuning.

These tests may alter BTVs.

They do not reopen the combat architecture by default.

---

# PREVIOUS CANON CHANGED BY PHASE 06

**None.**

Phase 06 fills values intentionally deferred by Phase 01–05.

It does not contradict or replace:

- Worksite architecture;
- Mission Refit;
- Crystal Charge;
- Aero Tube networking;
- Phase 03 roster;
- Phase 04 costs/progression;
- Phase 05 map topology.

---

# PART LXXVI — PHASE 07 REQUIREMENTS

The next phase is:

# CONTROLS, CAMERA, UX & INTERFACE

Phase 07 inherits the complete Phase 06 combat interaction model.

It must treat the following as known canonical player-facing systems.

## SELECTABLE OBJECTS

- all 35 buildable units;
- all 31 buildings/infrastructure;
- Rock Monsters and neutral combat targets where present.

## UNIVERSAL UNIT COMMANDS

- Move;
- Attack;
- Attack-Move;
- Focus Target through direct Attack;
- Stop;
- Hold Position;
- Patrol;
- optional Spread formation;
- Repair where eligible;
- Load;
- Unload.

## TARGETING INFORMATION

Phase 07 must communicate:

- legal ground/air targets;
- attack range;
- minimum range;
- target priority;
- current target;
- inability to attack a target layer.

## COMBAT STATES

It must clearly represent:

- damage;
- repair;
- transformation;
- deployment;
- Configuration Lock;
- Surge;
- Stability;
- Tube transit;
- Tube Arrival Recovery;
- refitting;
- brownout;
- disconnected Worksite/network state.

## ASTRONAUT COMMANDS

Must expose without clutter:

- T3 Mission Refit;
- Mobile Mining Platform Refit;
- Sentinel Refit;
- MX-41 Transform;
- Solar Explorer Deploy;
- MT-201 Drill Deploy.

## ALIEN COMMANDS

Must expose:

- Alien Strike reconfiguration;
- Infiltrator reconfiguration;
- 50-Charge Surge;
- valid Core/Mothership Surge anchors;
- Defense Resonance Shunt;
- Charge amount and maximum.

## MARTIAN COMMANDS

Must expose:

- Tube origin/destination transfer;
- Station queue;
- Protector Stance;
- Searcher Brace;
- Excavation Clamp;
- Stability feedback;
- network segmentation.

## ROCK RAIDER COMMANDS

Must expose:

- authored excavation;
- Worksite service state;
- repair/service access;
- transport handling.

It must not turn Worksite operation into per-hauler micro.

## CAMERA / SELECTION CONSEQUENCES

Camera and selection must support:

- medium-scale armies;
- Huge air units;
- contact machinery;
- 9-cell siege;
- 7.5-cell static AA;
- 25+ cell open battlefields;
- clear state telegraphs.

## HUD INFORMATION

Phase 07 already knows it must represent:

- Ore;
- Energy reserve/generation/demand;
- Crystals;
- Operations Capacity;
- HP;
- armor class/rating where useful;
- Charge;
- production;
- research;
- repair state;
- refit state;
- transport capacity;
- detector/sight relationships;
- fog of war;
- map objectives;
- Aero Tube network;
- Worksite servicing;
- Forward Service coverage;
- current weapon target eligibility.

## MINIMAP

Must convey at minimum:

- friendly/enemy unit presence;
- true-air threats where visible;
- known structures;
- attack alerts;
- expansion/resource information;
- active Tube topology;
- significant Surge activation;
- major known siege pressure;
- fog-of-war state.

## UI COMPLEXITY RULE

The interface must reflect the Phase 06 APM budget.

It must not invent new per-unit combat stances or buttons merely because interface space exists.

Phase 07 designs **how the player interacts with this combat model**.

It does not redesign the model itself.

---

# PHASE 06 CLOSURE

## DECISIONS ESTABLISHED

- complete deterministic combat simulation;
- complete HP/armor architecture;
- complete damage matrix;
- complete movement/range architecture;
- complete projectile/splash behavior;
- complete repair model;
- complete transport-risk model;
- complete transformation timings;
- complete Alien Surge combat implementation;
- complete Martian displacement implementation;
- complete static-defense stats;
- complete structure durability;
- complete Rock Monster combat baseline;
- complete 35-unit combat/movement specification;
- complete weapon baseline.

## UNRESOLVED QUESTIONS

None at the design-rule level.

Only empirical BTV tuning remains.

## DEPENDENCIES FOR LATER PHASES

Phase 07 receives complete player-command/state requirements.

Future Visual Bible work must visually communicate these mechanics without changing their strategic relationships.

Future technical implementation may optimize simulation technique while preserving deterministic outcomes.

## CANON CHANGES TO EARLIER PHASES

**None.**

# PHASE 06 — COMBAT, DAMAGE & BALANCE FRAMEWORK IS COMPLETE.
