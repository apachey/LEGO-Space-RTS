# M8 T073 — COMMAND BINDING REVIEW

This is a development review package, not gameplay canon. It records the
smallest decisions still required to bind every T073 command without inventing
rules that are absent from `Docs/Canon/`.

## Already determined by canon and implemented in the data checkpoint

- The catalog contains one stable definition for each of 35 command families.
- Construction and production prerequisites preserve AND between groups and OR
  within a group.
- All 35 canonical units have exact Phase 04 production costs, times, OC and
  producer bindings.
- Attack-Move applies to mobile selections so noncombat support units move with
  the group; Ping is player-scoped and does not require selection.
- Production reordering is represented, with the active item immovable behind
  waiting items.
- Protector Stance itself is not gated by Utility Mechanisms. That research
  gates Guard Sweep and the upgraded resistance effects.
- Transform, Deploy and Reconfigure use the shared state-change family. Guard
  Sweep and the Martian Deflector remain automatic rather than becoming extra
  buttons.

The existing command packet remains byte-for-byte unchanged. New command codes
19–35 are reserved in content but rejected by the runtime envelope until their
payloads and handlers are implemented.

## Director decisions required

### 1. Cancellation accounting

Phase 07 requires cancellation and a refund/loss preview for production,
research and Mission Refit. Phase 04 defines the exact progressive accounting
formula only for building construction.

Recommended rule:

- a waiting job returns 100% of Ore, Energy and Crystals;
- an active job commits 20% of Ore/Energy on start and consumes the remaining
  80% linearly over progress;
- cancellation returns all unspent Ore/Energy plus half of consumed
  Ore/Energy, using the existing deterministic integer rounding;
- Crystals commit at 50%; before that point they return in full, and afterward
  they do not return;
- Mission Refit retains the old/owned module and uses the same accounting,
  while facility destruction keeps its already canonical special handling.

### 2. Authored production exits

Phase 07 requires an authored, reserved exit area with clearance for the
largest product. Canon does not provide dimensions for the newly imported
producers. Recommended front-centered table, retaining the existing Raider
entries:

| Producer | Exit | Largest footprint |
|---|---:|---|
| Rock Raiders HQ | 2×2 | Tiny |
| Vehicle Service Bay | 3×3 | Medium |
| Engineering Workshop | 5×5 | Huge |
| MB-01 Eagle Command Base | 3×3 | Small |
| Field Systems Garage | 4×4 | Large |
| Mission Vehicle Bay | 5×5 | Huge |
| Flight Operations Pad | 5×5 | Huge |
| ETX Command Core | 2×2 | Tiny |
| ETX Fabricator | 3×3 | Small |
| Reconfiguration Dock | 5×5 | Huge |
| Aero Tube Hangar | 3×3 | Small |
| Settlement Station | 2×2 | Tiny |
| Mechanical Workshop | 5×5 | Huge |

### 3. Excavation duration

Canon gives 25 Energy and roughly 15–35 seconds for Standard excavation, and
50 Energy and roughly 30–60 seconds for Reinforced excavation, but no exact
feature duration or machine multiplier.

Recommended exact values: Standard **25 seconds / 500 ticks** and Reinforced
**45 seconds / 900 ticks**, with no hidden machine multiplier.

### 4. AST and Alien Energy-domain membership

Canon requires generous Astronaut operating areas and Alien Command-Core
membership, but supplies no association radius. The existing 18/10-cell
Forward Service radii are a different system and do not answer this.

Recommended rule: Astronaut static Energy emitters use an 18-cell radius and
join when their areas overlap; Alien structures join the nearest operational
Command Core within 18 cells, breaking equal-distance ties by the lowest
`EntityId`.

### 5. Settlement Station local reserve

Canon requires a local reserve for a disconnected Station but gives no
capacity. Recommended value: **150 Energy**, matching a primary command reserve
without turning the Station into a generator.

### 6. ETX Defense Node reconfiguration

Canon requires choosing Ground Pulse or Air Lance when built, and says later
reconfiguration is slow and unavailable under direct combat pressure. It does
not define duration or cost.

Recommended rule: the placement command requires the initial mode; later
reconfiguration takes **6 seconds / 120 ticks**, costs no additional resources,
and cannot start or progress while the Node is under direct combat pressure.

## Approval boundary

Approving this package authorizes implementation of these exact values and
rules in T073. Any changed value should be stated explicitly. Until approval,
the catalog remains usable for validation while affected runtime actions stay
unavailable.
