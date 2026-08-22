# M7 T064 — MATERIAL ART-DIRECTION HANDOFF

## Status

The first Godot material-master foundation and isolated Material Lab are
implemented, but the game director rejected a single-material-first workflow as
the art-direction method. The fixture is retained as engineering infrastructure;
it is not accepted T064 art direction.

The game director owns the final visual choices. Implementation agents own the
material code, asset import, rendering integration, automated smoke and build.

## Visual canon status

Phase 08 rendering, material, lighting, palette and general style conclusions
are suspended by explicit game-director decision. No replacement visual canon
exists while the Style Lab is under review. The Phase 07 functional UX contract
and non-visual gameplay canon remain unchanged.

## Prepared fixtures

From a normal launch:

1. press `F8`;
2. select **M7 Look Lab**;
3. adjust independent sections in the left panel at gameplay zoom;
4. use the mouse wheel or exact 24–72 build-cell zoom slider;
5. use **HIDE · TAB** for an unobstructed game view;
6. use **COPY ALL JSON** and paste the complete result back to the implementation
   agent;
7. press `Escape` to return to the prototype.

The full control and scene contract is in `M7_LOOK_LAB.md`. The Palette Lab
remains available separately. The Style Lab remains command-line reproducible
legacy evidence, but it is no longer offered as the main F8 art-direction path.

The Style Lab keeps one unit, camera, animation and composition invariant. Its
research-driven second round switches Industrial Mass, Heroic RTS, Constructive
LEGO and Graphic Volume treatments. The Palette Lab shows:

- candidate visible-area ratios for Rock Raiders, Life on Mars astronauts, Mars
  Mission astronauts, Mars Mission aliens and the aggregate Martian range;
- separate 7311, 7313, 7314, 7316 and 7317 Martian families;
- identical 100-panel abstract models where each panel equals one percent of
  visible frontal surface area;
- transparent materials on split light/dark backing;
- generic work-lamp and energy-resource emission classified by function;
- the explicit luminous-color language for every faction group.

Rock Raiders now includes earth brown as a major 18% vehicle surface. The 7316
Excavation Searcher now treats tan/beige as the dominant 42% visible surface;
these are corrections to the earlier invalid part-count-biased read.

The first style review selected no direction. Its four broad categories are
retired as forward candidates. Round 2 follows the game director's non-canonical
research brief in `M7_ART_DIRECTION_RESEARCH_BRIEF.md`; visual canon is still
open.

The first Round 2 review does not select a direction, but it narrows the useful
evidence: Heroic RTS is the current preference; Industrial Mass color depth,
Heroic color/bloom and Constructive highlights are positive traits. The overt
warm key in Industrial and Heroic was rejected and has been neutralized. Outline
is no longer built into Graphic Volume; it is an independent off-by-default A/B
toggle on every treatment.

The first equal-swatch fixture remains as **Old Material Lab** for engineering
comparison. It is not the current art-direction review path.

## Art-direction response requested

Search at normal gameplay scale instead of choosing a bundled treatment. Pause
when useful, compare outline off/on independently, hide the controls for the
clean composition, then copy the complete JSON profile. Palette review is
already accepted and remains independent from the open rendering decision.

## Texture policy

Clean molded polymer does not need a photographic albedo texture. Its baseline
comes from geometry, color, roughness and lighting. Texture work is reserved for:

- source-authentic prints and decals;
- controlled damage/wear masks near functional contact zones;
- terrain low-frequency variation;
- optional normal/ORM support after a material is visually accepted.

Image generation can produce useful look-development boards and albedo concepts.
It does not by itself prove seamless tiling or generate production-trustworthy
normal/ORM data. Every generated candidate must be checked at real gameplay scale.

## Image-generation decision

Image generation is no longer used to compare implementable rendering styles.
It produced excessive geometry, failed to preserve the invariant scene and made
materially different prompts converge on the same polished image. Future use is
limited to non-authoritative mood or texture thumbnails when specifically useful.

The representative art-direction evidence is now the real Godot Style Lab.

## Round 2 verification and build

The four treatments have been rendered both from the source project and from
the exported macOS application. Each exported path preserved the controlled
fixture at 48 meshes, 7,776 triangles and zero HUD/GUI canvas layers.

Evidence:

- `Artifacts/Screenshots/m7-exported-style-industrial-mass.png`;
- `Artifacts/Screenshots/m7-exported-style-heroic-rts.png`;
- `Artifacts/Screenshots/m7-exported-style-constructive-lego.png`;
- `Artifacts/Screenshots/m7-exported-style-graphic-volume.png`.

The refined Heroic A/B evidence is:

- `Artifacts/Screenshots/m7-exported-heroic-rts-outline-off.png`;
- `Artifacts/Screenshots/m7-exported-heroic-rts-outline-on.png`.

Full verification passed with no blocking failures:
`Artifacts/Verification/20260822T085432Z-full-summary.txt`.

The superseding realtime Look Lab passed the complete suite at
`Artifacts/Verification/20260822T100418Z-full-summary.txt`. Direct launches of
the exported macOS app produced
`Artifacts/Screenshots/m7-exported-look-lab-controls.png` and
`Artifacts/Screenshots/m7-exported-look-lab-game-view.png` with the required
four-unit gameplay composition at zoom 44.

Playable build: `Builds/macOS/LEGO Space RTS.app`. The requested human task is
only visual judgement: adjust the Look Lab, judge material credibility,
mechanical readability, ground restraint, light hierarchy, animation and VFX,
then copy the full JSON. No style is accepted by the existence of this build.
