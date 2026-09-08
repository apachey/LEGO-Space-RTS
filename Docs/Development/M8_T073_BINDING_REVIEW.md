# M8 T073 — APPROVED COMMAND BINDINGS

This development record captures the game-director decisions approved for
T073. It is implementation guidance, not a modification to gameplay canon.
The earlier recommendations are resolved and implemented as follows.

## 1. Cancellation accounting — approved

- Waiting production, research and Mission Refit jobs refund 100% of Ore,
  Energy and Crystals.
- An active job commits 20% of Ore/Energy when it starts, then consumes the
  remaining 80% linearly with progress.
- Cancellation returns all unspent Ore/Energy plus 50% of consumed Ore/Energy
  with deterministic integer rounding.
- Crystals commit at 50% progress. They refund fully before commitment and do
  not refund afterward.
- Mission Refit retains already owned modules.
- The canonical Service/Refit facility-destruction handling is unchanged.

The same Crystal reservation/50%-commit boundary is used by the newly enabled
Crystal-bearing construction actions so their authored cost cannot be bypassed.

## 2. Authored production exits — approved

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

All exits are front-centred, authored data and reserve clearance for the listed
largest footprint.

## 3. Excavation duration — approved changed values

There is no shared duration and no hidden machine multiplier.

| Machine | Standard | Reinforced |
|---|---:|---:|
| Drill Craft | 20s / 400 ticks | 40s / 800 ticks |
| Chrome Crusher | 25s / 500 ticks | 45s / 900 ticks |
| Granite Grinder | 30s / 600 ticks | 55s / 1100 ticks |

Standard excavation costs 25 Energy and Reinforced excavation costs 50 Energy.
Drill Craft remains the fastest dedicated route-opening asset in both classes.

## 4. Energy-domain membership — approved clarification

Astronauts:

- MB-01 Eagle Command Base, Service & Refit Hub and Solar Energy Array each
  establish an 18-cell Energy-domain area.
- Overlapping areas form one Energy Domain and pool generation/reserve.
- Energy-domain topology remains separate from Forward Service topology.

Aliens:

- A structure associates with the nearest operational ETX Command Core within
  18 cells; equal distances use the lowest `EntityId`.
- Membership recalculates deterministically when relevant Core operational or
  topology state changes.
- Each Command Core owns at most one Resonance Core.
- A Resonance Core cannot reassociate to an already claimed Command Core. It
  remains without valid Command-Core membership until a valid Core is free.

## 5. Settlement Station local reserve — approved

- Reserve capacity is 150 Energy.
- Existing Self +1 Energy/s auxiliary generation remains unchanged.
- Connected Martian components pool generation/reserve through the existing
  Aero Tube topology rules.

## 6. ETX Defense Node reconfiguration — approved exact pressure rule

- Placement requires an initial Ground Pulse or Air Lance mode.
- Later reconfiguration takes 6s / 120 ticks and costs no resources.
- Direct combat pressure means that the Node dealt or received hostile combat
  damage during the previous 4.0s / 80 ticks.
- Reconfiguration cannot start under pressure. Pressure during an active change
  pauses progress without resetting it; progress resumes after 80 ticks without
  a new hostile combat interaction.

## Implemented T073 boundary

The runtime now accepts the reserved command identities, validates their legal
payloads, binds construction/production/research and the affected faction
actions, and serializes the new authoritative jobs and state. Snapshot format
21 and simulation protocol 19 preserve deterministic continuation; snapshot
format 20 remains readable. The command packet layout and replay container
format remain unchanged.
