# T082 — controlled MT-101 / MX-71 source-locked corrections

Date: 2026-09-13. Branch: `codex/m85-t082`, not merged.

## What changed

Two completed simplified monochrome native review candidates replace neither
accepted images nor production models. Their editable source, two principal
renders and supplemental finished overhead/front views are retained under
`ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/`.
Status: `CONTROLLED_SOURCE_LOCKED_APPEARANCE_REQUIRES_DIRECTOR_REVIEW`.

The director's latest `+` separately accepts the earlier six-panel Solar
appearance and overall connected docked Mothership composition from `ea19e42`.
This limited approval is recorded in
`Content/Presentation/SuperScout/completed_composition_review.json`.
Wrong Mothership operator anatomy and raised printed detail remain pending;
there is no approval of training rules, unfolding topology, models or a roster.

## Why / source evidence

Two earlier free-form MT-101 image approaches respectively lost the permanent
cabin and countable six-wheel layout; MX-71's second aircraft image still had
the wrong weapon grouping. The recorded next step explicitly requests a
topology-controlled source-trace construction, not a third free-form retry.
This block uses one controlled native approach and actual editable geometry.
Initial internal lighting and tread-origin errors were corrected within that
approach. The overexposed internal renders are not review selections and were
moved recoverably to `/private/tmp/lego-m85-source-locked-internal-20260913`.

Official instructions were inspected as rendered pages, not only text:

- MT-101 / 7699: [book 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf),
  [book 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf).
  Book 2 cover, assembly page 35, tool assembly page 41 and removal page 43
  distinguish the permanent steep front cabin, six-wheel suspension,
  independently mounted upper ball launcher and articulated drilling boom,
  plus removable rear spacecraft. A source toy removal mechanism is not new
  gameplay permission.
- MX-71 / 7692: [official instructions](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf),
  finished assembly page 69 and payload mechanism pages 70–71. The source has
  a narrow long spine, closed forward cabin, sloping front wing plates, twin
  rear engine booms and close-clamped six-wheel rover. Four short airframe
  emitters are grouped into mirrored inner and lower outer forward pairs.

These are source-informed reconstructions with simplified shape language,
estimated comparison-stud dimensions and matte neutral materials. They are
not exact LEGO piece meshes, a certified physically buildable set, source
colours, approved gameplay-scale mapping or production geometry.

## 1. MT-101 Armored Drilling Unit

Role remains the approved heavy ground drilling/combat unit. Six main contact
wheels share a ground plane. Its closed front cockpit remains under the
permanent chassis independently of the rear spacecraft. The upper gun and
side articulated drill have separate roots. Docked rear wings, equipment tubes
and its own small cockpit are present; no towing trailer is introduced.

![MT-101 completed monochrome candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/mt101_appearance.png)

The main three-quarter view occludes two far-side wheels. The completed
overhead view supplies their visual evidence; this is not a primitive blockout
or another required dimension-approval gate.

![MT-101 finished overhead contact view](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/mt101_finished_overhead.png)

## 2. MX-71 Recon Dropship

Role remains approved air transport. The four short source-derived emitters
are two exact mirrored pairs, not an asymmetric cannon group. The source rover
is a close-docked visual payload, not an added roster entry or new transport
rule; its horizontal auger is not a fifth aircraft gun.

![MX-71 completed monochrome candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/mx71_appearance.png)

The far outer gun is occluded in the principal view. Its completed frontal
view shows both pairs without relying on an inferred camera-facing layout.

![MX-71 finished front weapon grouping](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/mx71_finished_front.png)

## Files changed

- Native preproduction generator, editable `.blend`, four review-view PNGs and
  bounded control/provenance audit; no GLBs or Godot import files.
- Separate limited composition approval record, T082 validation and review
  recording guards. Previous 29 tests remain; 12 added guards bring total to 41.
- Current project state, progress and review notes. Canon/runtime unchanged.

## Automated verification / regression coverage

Executed:

- Background Blender generation with `--python-exit-code 1`: PASS baseline
  and nine negative control cases. Initial reading of Blender's array property
  failed and was fixed by serializing it as a list; the failure was not hidden.
- Background Blender reopen audit using this generator with
  `-- --audit ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/source_locked_corrections.blend`:
  PASS actual tyre mesh centres/ground contacts, real barrel transforms and
  four forward axes, closed six-face glazing and independent tool/cradle roots.
  An initial audit invocation omitted Blender's argument separator and exited
  1; the corrected invocation actually exited 0.
- Same audit with `--details`: PASS supplemental finished view renders and
  saved-scene audit; both additional view hashes are retained and guarded.
- `python3 tools/Validation/validate_m85_super_scout.py`: PASS corpus integrity;
  production HOLD retained.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS 41/41.
- `./tools/verify.sh --full`: PASS all 29 blocking stages, 317/317 NUnit tests,
  41/41 recording guards, 100-repeat deterministic/replay/snapshot checks and
  deterministic content/GLB regeneration. Zero blocking failures. The unchanged
  60-mover M9 `BLOCKING_LATER` diagnostic still fails at 2/60, not a new
  regression or permission to rewrite movement. Evidence:
  `Artifacts/Verification/20260913T173046Z-full-summary.txt`.
- The focused validator and 41 recording guards were rerun after supplemental
  view/identity/scope locks were completed: PASS. `git diff --check`: PASS.

Native anchor/control mutations guard lost contacts/cabin, cabin detached with
the rear craft, fused tool mounts, shifted/missing/reversed aircraft guns and
lost payload/cradle. They protect native control records, not pixel fidelity.
The saved-scene audit additionally checks actual meshes/transforms. Principal
and supplemental renders require director visual judgement.

## Build / manual review / risks / canon / branch

No new model integration is made. Full verification actually produced and
smoke-tested `Builds/macOS/LEGO Space RTS.app`, containing the existing game,
not these review candidates. Manual review is appearance
and source resemblance only: the images above require no farming, setup or
editor launch. Source-exact profiles, detailed hinges, cockpit interiors,
animations, final materials and production budgets remain unverified. Neither
candidate is automatically accepted. Historical two-attempt stops remain as
evidence, with this controlled next-step candidate recorded separately.
Complete 24/44/72-cell T082 acceptance and production HOLD remain.
Canon impact: NONE. Branch: `codex/m85-t082`, not merged.
