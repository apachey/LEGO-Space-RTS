# T082 — completed Alien appearance concepts

Date: 2026-09-13. Branch: `codex/m85-t082`; not merged.
Status: DIRECTOR_ACCEPTED_APPEARANCE_FOR_COMPARATIVE_REVIEW.

Subsequent director response: `+` to the completed three-image handoff from
commit `b05ede8`, accepting these selected appearances for comparative review
and directing continuation. This does not accept shipping/native model fidelity,
animation or the complete T082 corpus. Exact evidence and hashes are retained
in the sibling manifest and protected by the T082 validator.

## What changed / why

Three finished exterior concepts replace primitive construction boards as the
director-facing handoff. The director already accepted dimensions and explicitly
requested completed appearance, not another unfinished-geometry approval gate.
AGENTS.md now records that distinction. The initial handoff requested appearance
acceptance; the subsequent response above resolves that image-only review.

Native Blender construction preserves the accepted comparison envelopes and
identical defense foundations. A single completed raster appearance per asset
then refines that construction into broad molded toy-scale pieces, restrained
lime inserts and small transparent energy interfaces. The native previews remain
internal references, not the selected appearance images or shipping models.
This is controlled sketch-to-render finishing, not a third free-form composition
revision. The old composed-revision-2 files, attempts and stop gates are untouched.

## Completed director-facing images and roles

Servitor: economic hover worker for Ore/Crystal collection, construction and
repair, without pilot, walking limbs or weapons. 5617's compact deck and 7646's
paired shell articulation inform the two enclosing valves and short utility
clamp; protected links are a proposed biomechanical adaptation, not literal
official internals.

![Servitor completed concept](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/AlienCompletedAppearanceV1/servitor_appearance.png)

Ground Pulse: fixed anti-ground defense. 7697's curved shell/emitter grammar
informs three connected broken-ring sections, three radial supports and three
horizontal emitters. The recombined crown is not an official alternate build.
Four broad fixed foundation contacts are not walking limbs.

![Ground Pulse completed concept](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/AlienCompletedAppearanceV1/ground_pulse_appearance.png)

Air Lance: fixed anti-air defense. 7692's paired blades and rear triple-emitter
assembly inform two upright panels around three upward emitters. Relocating and
reorienting the assembly is the disclosed adaptation; it is not a claim about
the official vehicle's front weapon. It uses the same native rooted foundation.

![Air Lance completed concept](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/AlienCompletedAppearanceV1/air_lance_appearance.png)

Phase 02A's approved Alien living-technology identity remains authoritative.
These proposed support slots and living links are not newly proven anatomy.
No harvesting, construction, firing, movement or balance rules change.

## Construction evidence and limitations

Native bounds remain: Servitor body 6×8 studs, 8 plates high, 6×10 including
clamp; defenses 12×12 studs, Ground Pulse 15 plates and Air Lance 26 plates high.
The 8 mm stud / 3.2 mm plate comparison does not redefine engine scale or
gameplay footprints. Authored base geometry counts are 22/48/42 mesh modules
and 792/2,140/1,344 triangles respectively, NOT physical LEGO piece counts or
evaluated modifier triangle counts. See `appearance_audit.json` for bounds,
material checks and native artifact hashes.

Eight native audit checks pass, including evaluated finished bounds, the three
Ground Pulse lobes, firing-axis distinction, identical lower geometry and no
permanent emission. Foundation geometry hash:
`f0c5d77d40b3115ba3d753879c12df1b5ad3ae9a16cf5a2ba5b8b7c334290694`.
Raster seams, studs and curves are approximate surface interpretations, not
exact editable part meshes or proved physical builds. Raster foundations are
not claimed pixel-identical, nor are raster dimensions objectively verified.
Generated stud lettering appears despite the no-logo prompt; it is not official
product provenance and is not an approved production texture.

## Files / provenance

- `tools/blender/finish_m85_alien_concepts.py`: bounded native finishing,
  negative guards and read-only saved-source audit; protects existing targets.
- `ArtSource/M85/Preproduction/AlienCompletedAppearanceV1/`: editable native
  `.blend`, six construction reference PNGs, native audit, three selected
  `*_appearance.png` images and `completed_appearance_manifest.json`.
- The manifest retains the exact imagegen prompts, official instruction URLs,
  page references, original generated output paths and selected-image hashes.
  Servitor/Air Lance outputs were recovered after interruption, not regenerated.
  Ground Pulse had no recoverable original output; the same prompt and inputs
  were resubmitted once to complete it. No visual redesign or extra selected
  appearance variant was made.
- `AGENTS.md` and `PROJECT_STATE.md`: clarified handoff boundary/current state.

## Automated verification / regression coverage

Executed for this work block before interruption:

```sh
./tools/verify.sh
```

PASS: all 23 blocking stages, 317/317 NUnit tests, zero blocking failures.
Evidence: `Artifacts/Verification/20260913T103130Z-fast-summary.txt`.
No runtime code changed afterward. This is not a new full-suite run.

Native and focused checks were rerun when recovering the images:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/finish_m85_alien_concepts.py -- --self-test
/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/finish_m85_alien_concepts.py -- --audit-existing ArtSource/M85/Preproduction/AlienCompletedAppearanceV1/alien_finished_concepts_v1.blend
python3 tools/Validation/validate_m85_super_scout.py
python3 tools/Validation/test_m85_super_scout_review_guards.py
git diff --check
```

Native self-test: PASS 6/6 (baseline, four inherited negative geometry guards,
new permanently emissive lime rejection). Saved native source: PASS all eight
checks. Corpus validation: PASS with the existing 66-asset HOLD retained.
Director-review recording regressions: PASS 22/22. Diff whitespace: PASS.
The initial restricted-environment native rerun terminated before audit; the
same native commands were repeated with approved background execution outside
that restriction. The failed launch is not counted as a passing audit.

Selected raster hashes were checked against the retained manifest after copy.
No tests were weakened and no accepted image/review record was replaced.

## Build / manual review / remaining gates

No new playable export, GLB, texture set, LOD, rig, animation or gameplay import
was produced for these concepts. The previous playable build is not a delivery
of this new appearance. Human review is completed exterior appearance only;
no renewed size confirmation, blockout judgement or game setup is requested.
Source recognition, construction density and biomechanical readability still
require the director's judgement. Full 24/44/72-cell acceptance and existing
MT-101/MX-71 stops remain open; T082 still blocks T083/T085.

Canon impact: NONE. Work remains on `codex/m85-t082`, not merged into main.
