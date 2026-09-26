# T3-Trike — T083 design spec (director review)

**Stable ID:** `unit.astronauts.t3_trike`
**Faction / role:** Astronauts / Escort and Survey
**Target mode:** `SOURCE_LOCKED` — the target panels are the two completed official source vehicles, without invented geometry.
**State:** `DESIGN_ACCEPTED_T084_AUTHORIZED` on 2026-09-24; T084 has not started.

## Source Evidence

- **LEGO 7312 T3-Trike**, official instruction PDF 4130807, completed vehicle and construction views. The packet's source audit identifies its transparent cockpit, large rear wheel, twin forward outrigger/contact assemblies, hoses and detachable tools. [Official PDF](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf)
- **LEGO 7694 MT-31 Trike**, official instruction PDF 4517775, completed vehicle and construction views. The packet's source audit identifies its open operator position, long orange equipment cylinder, three oversized orange wheels and exposed angled supports. [Official PDF](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517775.pdf)
- Both official sources and their view gaps are already visually audited in the accepted T082 packet. This package does not claim new source inspection or add a new source.

## Production Design Target

The target is the exact official completed model for each source lineage, shown in the [director review](t3_trike_production_design_review_20260924_v1.html):

1. **7312 / Life on Mars:** preserve the source outline, proportions, transparent cockpit, rear wheel, paired outriggers, exposed hoses and source colors.
2. **7694 / Mars Mission:** preserve the source outline, proportions, open operator position, orange equipment cylinder, three orange wheels, exposed supports and source colors.

These are separate source-locked appearances in the same gameplay family. Do not average their colors, wheel sizes or suspension layouts into a third design. Do not add armor, enclosure, antennae, insignia, or a new center module. Director decision (2026-09-24): these are unequal configuration looks for one gameplay unit, not peer variations. The 7312 Life on Mars appearance is the base Escort. After Field Survey Package research, the player may refit to Survey and show the 7694 Mars Mission appearance.

## Design Spec

- Keep three terrain contacts, exposed articulated support structure, a visible operator and a recognizable modular center/equipment mass. Preserve each donor model's distinct wheel and cockpit proportions.
- Retain the Field Systems blue-gray/white material language on the 7312 appearance and the white/orange/black Mission Systems language on the 7694 appearance. The shared faction identity comes from interfaces and insignia; do not blend the source palettes.
- Keep the 7312 and 7694 appearances independently identifiable at RTS scale. Follow accepted M7 Final materials and lighting with outline on; no new camera or gameplay-camera claim is made here.
- Gameplay configurations remain Escort (initial) and Survey. Escort uses the official 7312 Life on Mars appearance. Survey becomes available after Field Survey Package research and uses the official 7694 Mars Mission appearance. This is a presentation-state mapping for the same unit; it does not change research, refit, simulation or unit rules.
- Motion remains presentation-only: restrained wheel rotation and bounded suspension travel; no source evidence proves a particular driving gait. This package does not alter gameplay or refit rules.

## Technical / explanatory schematic

None supplied. The official source images are the appearance targets, not schematics.

## Decision and limits

Please approve or reject the two-source appearance target as one T3-Trike family. If approved, also state which appearance is used for initial Escort and which for Survey, or confirm that both configurations keep the same chosen source appearance. No mapping is inferred here. If the gameplay-family contract instead requires one fixed chassis with a swapped center module, the target must be revised to show that adaptation before T084; these official full builds do not prove that composite.

No source geometry is authorized for production until this review decision is recorded. Human camera-readability, model dimensions, pivots and LOD review remain T084/T086 work.
