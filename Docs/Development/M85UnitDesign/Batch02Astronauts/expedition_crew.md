# Expedition Crew — T083 design proposal

**Stable ID:** `unit.astronauts.expedition_crew`<br>
**Faction / class / scale:** Astronauts / worker-builder / Tiny<br>
**Status:** `DESIGN_ACCEPTED_T084_AUTHORIZED` — accepted by the game director on 2026-09-24. T084 is authorized by the design decision but has **not started**; this bounded task stops here.

**Director decision:** “підходить” (accepted, 2026-09-24). The accepted visual direction is the paired Field/Mission appearance family, shared broad insignia, and shared worker-tool read described below.

## Design decision for review

One minifigure-derived worker family has two equal, selectable appearance lineages. Field Systems retains the Life on Mars human crew's rugged blue-gray/white field equipment; Mission Systems retains Mars Mission's cleaner white/orange equipment. Shared Astronaut insignia and the same compact engineering tool identify their common unit role. Neither variant is the default, an upgrade, a gameplay specialization, or a separately selectable unit.

The illustration in [review.html](review.html) is an authored construction/readability schematic, not LEGO source photography or a claim about exact prints. Choose whether this paired-variant direction is acceptable, and whether its insignia/tool emphasis reads clearly.

## Authority and reference trail

The T082 packet is [unit_astronauts_expedition_crew.md](../../M85SuperScout/Packets/unit_astronauts_expedition_crew.md). Its source ledger audits official instructions for [7301 Rover](https://www.lego.com/en-us/service/building-instructions/7301) (PDF [4156314](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4156314.pdf)) and [7690 MB-01 Eagle Command Base](https://www.lego.com/en-us/service/building-instructions/7690) (PDFs [4523177](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523177.pdf), [4523179](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523179.pdf)). Their instruction pages are used for the associated crew/equipment and construction context, not as proof of a single standard figure or a shared pack. The source records' view coverage and gaps remain authoritative; this package does not claim new source views.

Required project references, all read-only: [Phase 03 Expedition Crew and Astronaut character sections](../../../Canon/03_UNIT_BUILDING_ROSTER.md#1-expedition-crew); [Phase 02 Field/Mission relationship](../../../Canon/02_FACTION_BIBLE.md); [Phase 09C design/model gates](../../../Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md#4-m85-implementation-backlog); [T082 Astronaut production contract](../../../../Content/Presentation/SuperScout/astronauts_production_contracts.json); [T082 Astronaut source evidence](../../../../Content/Presentation/SuperScout/astronauts_source_evidence.json); [accepted M7 visual direction](../../M7_VISUAL_ACCEPTANCE_CANDIDATE.md). The T082 packet also carries the authoritative source ledger, open questions and neighbor-confusion records.

Canon maps Life on Mars human characters to Field Systems and Mars Mission astronauts to Mission Systems. It requires a shared Astronaut identity without homogenizing their equipment. The T082 contract's standardized pack, sealed helmet and modular tool are pre-production adaptations; this proposal preserves them only where they fit the selected source figure and does not add heavy armor, a gun, or identical backpacks to every figure.

Evidence levels: source lineage and minifigure basis are source/canon grounded; common insignia, role tool and paired presentation are adaptations proposed for director review. Missing rear-print and per-figure pack/lamp evidence remains unresolved; do not invent printing or equipment combinations.

## Appearance and silhouette

- **Primary read:** upright, single minifigure-sized human with a large distinct head/helmet mass, compact torso, two short legs, and one tool held close to the body.
- **Field Systems:** retain source character and headwear variation; use the accepted rugged white/light-gray/medium-blue material language. A compact field pack may be used only for a source-supported figure/equipment combination. Keep the equipment irregular and practical, not armored.
- **Mission Systems:** retain Mars Mission crew head/helmet variation and clean white/orange/black equipment language. The pack may read as a cleaner modular service unit; keep it compact and subordinate to the figure.
- **Shared identity:** one small flat Astronaut insignia on a visible torso/shoulder face; the same broad tool-contact silhouette across both appearances. No player-color body repaint.
- **Gameplay-camera anchor order:** head/helmet, torso color block and tool. Pack silhouette is secondary. At distant scale, the figure remains upright and human; no micro-labels are required.
- **Forbidden drift:** infantry armor, rifle, oversized backpack, sealed generic suit that erases source character variation, third blended color scheme, or exaggerated proportions that no longer read as LEGO minifigure-derived.

The family is Tiny; preserve the source minifigure proportion relationship. Exact world dimensions and camera proof belong to production. This sheet is a design comparison, not a 24/44/72 acceptance test or a gameplay-camera pass.

## Construction and semantic part map

| Part | Proposed construction | Read / function | Evidence status |
|---|---|---|---|
| Head and headwear | Source-specific minifigure head/headwear; do not standardize every face or helmet | Human field worker; largest identity mass | Source-grounded lineage; exact variant selection remains open |
| Torso/hips/legs | Minifigure-derived simple articulated forms | Upright worker, LEGO-derived scale | Canon-derived adaptation |
| Field/Mission pack | Small removable back module, variant-specific and only when supported by selected figure | Equipment lineage at oblique/top view | Proposed adaptation; exact donor combination unresolved |
| Insignia | Flat broad mark, no raised badge geometry | Common Astronaut organization | Proposed adaptation |
| Engineering tool | Compact two-handed tool with one clear working end, sourced from applicable crew equipment where verified | Extraction/build/repair contact; never a firearm | Canon function; exact shared tool form proposed |
| Hands/feet | Preserve minifigure proportions and visible planted stance | Reach and work contact | Canon-derived adaptation |

Load path is body → hands/tool contact; pack mounts to torso/back. No external stand, vehicle, crystal, or unrelated source-set module is attached to the unit. Use simple LEGO-like part boundaries and joints; no hidden mechanical substructure is part of the visual pitch.

## State and movement proposal

Existing T082 presentation contract only; this adds no gameplay states or timing.

| State | Readable presentation |
|---|---|
| Idle | Relaxed upright stance; one brief equipment/tool check, then tool rests close to torso. |
| Move | Compact alternating foot steps; slight pack lag only where a pack exists; no combat march. |
| Work / extraction / repair | Feet settle; tool reaches the contact point, performs a short repeated work beat, then returns to carry pose. |
| Refit installation | Same planted worker and tool-contact language; service action reads from the existing refit presentation event. |
| Emergency defense | Existing carried tool only; no firearm or new attack pose family. |
| Damage / destruction | Work pose interrupts; tool may separate before restrained LEGO-style breakup. No injury detail. |

Pivots and presentation sockets remain those in the T082 packet: `Pivot_Tool`, `Pivot_PackTool`, `Socket_Selection`, `Socket_Health`, `Socket_ToolContact`, `Socket_WorkVfx`, `Socket_Refit`, `Socket_AudioTool`. The pack pivot applies only when that variant includes a pack. Animation reflects authoritative state and never drives gameplay.

## Materials and texture plan

Use accepted M7 Final with outline on as the production presentation baseline. Keep large color/material regions in geometry/material assignment: Field white/light-gray/medium-blue; Mission white/orange/black; shared tool neutral/tool material; helmet glass only when a real transparent source part is present. Avoid universal glow, photographic wear, baked highlights, fake seams and tiny unreadable print.

Use only the existing T082 shared requirements if approved for production: `ast_service_insignia_decals` for the broad insignia/service mark, and `ast_console_signal_atlas` only if an actual carried display is selected. Both remain `SPECIFIED_NOT_AUTHORED`; this proposal does not require a display. No bespoke texture is required. At strategic distance remove labels and retain only the large head, torso, tool and major lineage color blocks.

## Confusion checks and open decisions

- Against Rock Raiders Crew: Expedition Crew keeps the white/blue or white/orange lineage blocks and common Astronaut mark; Raider Crew keeps its source-specific clothing/headwear and prominent industrial tool.
- Against ETX Servitor: upright human body and carried hand tool remain distinct from the low hover shell and wrapped crystal cradle.
- **Director decision:** accept paired equal Field/Mission appearances for this one unit, or choose one appearance direction for the shared roster family.
- **Director decision:** approve a shared broad engineering-tool silhouette and small common insignia, or identify a visible adjustment.
- Exact crew figure roster, per-figure pack/headwear combinations, rear print and tool donor remain bounded by available source evidence; production must not fill those gaps by invention.

## T084 handoff boundary

On acceptance, T084 may block out this approved appearance family and validate real-gameplay-camera readability, scale, geometry, pivots and required LODs. This document does not prove fit, joint clearance, silhouette at runtime, material authoring or animation quality. `T084: NOT STARTED`.
