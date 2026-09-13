# T082 — controlled Alien shape drafts

Date: 2026-09-13. Branch: `codex/m85-t082`; not merged.
Status: PREPRODUCTION_SHAPE_DRAFT_DIMENSIONS_ONLY_ACCEPTED.

## Delivered review block

The director accepted the shared-scale dimensions and then explicitly requested
continuing useful work rather than ending turns with approval acknowledgements.
The approved next step was controlled source-informed shape work, not a third
free-form image attempt or final roster model production. Communication cadence
and unnecessary engine-launch reassurance are now recorded in AGENTS.md.

Three real, editable, faceted Blender drafts now retain the accepted bounds.
Two common-scale orthographic boards show all three alongside the previously
disclosed approximate 40 mm schematic minifigure and actual-sized 4×6 plate.
The same models produce both views; these are native geometry renders, not
independent AI interpretations of shapes or dimensions.

![Common three-quarter shape board](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/AlienShapeReview/alien_shape_drafts_three_quarter.png)

![Common overhead shape board](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/AlienShapeReview/alien_shape_drafts_top.png)

## Functional and donor map

| Draft | Role | Source-informed major forms | Proposed connection/adaptation |
| --- | --- | --- | --- |
| Servitor | Ore/Crystal collection; building and repair | 5617 compact deck; 7646 paired shell articulation | Two broad shell valves attach to one load-bearing deck/spine; two short protected links feed the short paired utility jaws. Seat, pilot, protective rider arches, guns and walking limbs are not transplanted. |
| Ground Pulse | Existing fixed anti-ground configuration | 7697 curved shell/emitter grammar | Three large crescents connect through radial roots to the central spine; emitters point horizontally. The crescent crown is a disclosed recombination, not an official alternate build. |
| Air Lance | Existing fixed anti-air configuration | 7692 paired blades and rear triple-emitter assembly | Two broad upright blade proxies root into the same foundation; the reoriented three-emitter cluster connects to a raised central support. Relocating/reorienting the rear assembly is an adaptation, not a front-gun source claim. |

Both defense drafts call exactly the same four-fixed-contact foundation builder.
Broad rigid roots connect pads to a central armored capsule and head interface;
they are stationary structural contacts, not walking legs. Two recessed links
provide the biomechanical-family cue. Phase 02A's 7691/promo lineage supports
the living-technology identity, not literal verification of these new interiors.
There is no transplanted fighter atop an unrelated pedestal.

Sources are the retained audited official instruction images: 5617 p1, 7646
p68 (plus its existing shell/motion audit), 7697 p15 and 7692 p23. Exact official
URLs and promo evidence/confidence remain in
`M85_T082_SOLAR_AND_BIOMECHANICAL_REVISION_BLOCK.md` and Phase 02A.
The three crescent sections, blades, clamp and base are authored shape proxies,
NOT exact molded LEGO part meshes or proved physically buildable assemblies.

## Complexity and scale

| Draft | Accepted comparison envelope | Mesh modules | Triangles |
| --- | --- | --- | --- |
| Servitor | Body 6×8 studs, 8 plates high; 6×10 with clamp | 15 | 468 |
| Ground Pulse | 12×12 studs, 15 plates total | 32 | 1,184 |
| Air Lance | Same 12×12 base, 26 plates total | 25 | 732 |

Mesh modules are named authored geometry groups, NOT LEGO piece counts. Counts
exclude the reference figure, plate, floor, labels and lights. Body height
excludes the worker's visible hover clearance. The 8 mm comparison stud and
3.2 mm plate are physical comparison units only; game footprints and the T081
world-unit convention are unchanged.

No layered armor, dense rib grid, extra printed-line geometry, textures, final
palette, glow or simulated organs were added. Curved shells stay large faceted
pieces, with only a few connection studs. Protected living links are restrained
proposals, not an accepted organic-material treatment. Some studs slightly
intersect their receiving surfaces as connected parts; all joints/rigging and
surface treatment still need production design after shape acceptance.

## Files and reproduction

- `tools/blender/generate_m85_alien_shape_drafts.py`: bounded existing-Blender
  generator and negative geometry guards; refuses existing output targets.
- `ArtSource/M85/Preproduction/AlienShapeReview/alien_shape_drafts_v1.blend`:
  editable combined preproduction source with named functional modules.
- The two sibling PNG boards and `geometry_audit.json`: actual dimension,
  triangle/module and identical-foundation evidence plus artifact hashes.
- Size approval and historical inline sheet are retained separately in
  `M85_T082_ALIEN_SCALE_REVIEW.md`; all generated-image manifests remain intact.

Run with the existing Blender executable:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/generate_m85_alien_shape_drafts.py -- <new-task-owned-output-directory>
/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/generate_m85_alien_shape_drafts.py -- --self-test
/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/generate_m85_alien_shape_drafts.py -- --audit-existing ArtSource/M85/Preproduction/AlienShapeReview/alien_shape_drafts_v1.blend
```

`--python-exit-code 1` is required so a Python exception cannot be mistaken for
success merely because Blender's default process exit code is zero.
The self-test first validates the baseline, then rejects enlarged worker bounds,
different lower defense geometry, loss of the third crescent and horizontal
anti-air emitter axes. Each disposable mutation is restored and revalidated.

## Review boundary and known limitations

Review the three draft exteriors and their construction density. Previously
accepted dimensions do not need to be approved again. Donor recognition and
whether these restrained forms read as Alien biomechanical technology remain
human judgements; geometry checks do not establish visual acceptance.

This is not T083 production completion: no GLB, texture set, final pivots,
animations or gameplay import. The verification export contains the existing
game, not these unintegrated drafts. This work does not supersede any
accepted source-derived image, alter solar/Frontier review, clear MT-101/MX-71
stops or authorize a third free-form generation. Complete T082 24/44/72-cell
acceptance remains open. Canon impact: NONE; no gameplay/dependency change.

Initial renderer setup inside the filesystem sandbox failed during Blender's
Metal capability detection before the script ran. The same existing tool ran
outside that restriction. Initial review lighting overexposed the millimetre-
scale scene, and multiline labels crossed the floor; corrected light power and
label placement do not alter asset geometry. Non-selected renderer trials are
retained as local task history, not offered as review evidence.
The first negative test selected a non-envelope shell through positional child
ordering; the test now names the actual load-bearing boundary deck explicitly.
The 10% enlargement is unchanged; validation was not weakened.

## Executed verification

- Existing Blender command above with output
  `ArtSource/M85/Preproduction/AlienShapeReview` — PASS; both boards and editable
  source actually produced, five baseline geometry checks pass.
- Existing Blender `--self-test` command above — PASS 5/5, including four
  negative regressions. Final explicit-name targets remove child-order guessing.
- Existing Blender `--audit-existing` command above — PASS for the saved .blend;
  accepted bounds, worker body/tool separation, matching bases, three crescents
  and ground-horizontal/air-upward axes are retained after serialization.
- `PYTHONPYCACHEPREFIX=/private/tmp/lego-m85-native-render-history.60cfDw/pycache python3 -m py_compile tools/blender/generate_m85_alien_shape_drafts.py`
  — PASS (default system cache is outside the sandbox's writable roots).
- `./tools/verify.sh` — PASS, 317/317 NUnit, all 23 BLOCKING_NOW stages;
  `Artifacts/Verification/20260913T100848Z-fast-summary.txt`. Existing 22 director
  review guards pass unchanged; no acceptance manifest or historical image changed.
- `./tools/verify.sh --full` — PASS, 317/317 NUnit, all 29 BLOCKING_NOW stages;
  `Artifacts/Verification/20260913T101442Z-full-summary.txt`. All 100 deterministic
  repeats, replay/snapshot continuation, content/Blender regeneration and macOS
  export smoke pass. The preserved M9 60-mover BLOCKING_LATER diagnostic still
  fails at 2/60 completion; it does not block this preproduction review.
- `git diff --check` — PASS; final diff reviewed.

The existing game was freshly exported and verified at
`Builds/macOS/LEGO Space RTS.app`; these draft exteriors are not integrated.
No manual game playtest requested for these unintegrated drafts. All changes
are task-local preproduction on
`codex/m85-t082`, not merged into main.
