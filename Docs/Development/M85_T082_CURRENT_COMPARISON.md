# T082 — current comparison and next priority candidates

Date: 2026-09-13. Branch: `codex/m85-t082`, not merged. Canon impact: NONE.

## What changed / why

The director's `+` to `6a211dc` accepts only the displayed completed localized
MX-71 edit for comparative review. Its selected bytes, latest authority and
remaining far-mount occlusion are recorded separately from its immutable
generation-time manifest. No MT-101 or native-layout approval is inferred.

The next approved T082 step still requires a renewed complete comparison, but
the historical FullV2 boards intentionally contain old rejected images.
A separate named current gallery now assembles all 66 exact roster IDs and
67 existing views. Accepted later appearances override earlier generation
states only in this new selection layer. Both Ground Pulse and Air Lance are
views of one Defense Node, not a 67th roster asset. Official donor studies,
colored rejected Alien Jet and internal MX native render are not selected.
The initial assembly generated no new art. The later director correction of
Claw-Tank now selects its second localized arm-composition candidate below.

Gallery: `Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentComparisonV1/index.html`.
Its selection manifest records every image's exact hash, origin and current
review availability. It is a named, unscaled DIAGNOSTIC assembly, not a blind
test, real Godot camera capture, accepted complete corpus or production gate.
Original first-review and FullV2 boards/manifests/image bytes remain unchanged.

## Next completed priority candidates

### MT-61-derived Mobile Mining Platform — resource extraction

Canon Phase 03 §8 combines 7645, 7648 and the human 7693 vehicle into one
configurable economic family. This candidate studies the Crystal Reaper state:
two harvesting cutters, two manipulators and directly docked upper spacecraft/
processing module, not an unsupported towing trailer. It does not add a new
roster unit or approve a toy removal mechanism as gameplay. Ore Drill state,
service-controlled Mission Refit and final production topology remain obligations.
The image is still `UNREVIEWED_CORRECTION_CANDIDATE`.

![MT-61 candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_astronauts_mobile_mining_platform_rev1.png)

### MT-51 Claw-Tank — frontline cover

Canon Phase 03 §10: tracked frontline combat vehicle against light/medium
targets, protecting specialist vehicles. Its upper body can keep aiming while
the tracks travel another way. The director rejected rev1's arm composition:
the previous prompt had converted the source set's separate Alien spacecraft
into a right-side claw instead of deleting it. It also left a gun on the left.
The latest explicit clarification requires viewer-left = one claw,
viewer-right = gun arm, without a right claw or third lateral appendage.
One built-in localized edit replaces the left barrel tips with a simple
two-jaw gripper and deletes the extra right pincer and its branch, retaining
the right twin-barrel gun, cockpit, tracks and T-pose. This is correction
attempt 2; no further retry is authorized. The later director `+` to `038fc8a`
accepts this completed appearance for comparative review, recorded separately
in `Content/Presentation/SuperScout/claw_tank_appearance_review.json`.
Its generation-time unreviewed status remains historical; production is HOLD.

![Claw-Tank corrected candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/ClawTankArmCorrectionV2/claw_tank_arms_rev2.png)

Exact prompt, source-page copy, hashes, director wording and visual inspection:
`ArtSource/M85/Preproduction/ClawTankArmCorrectionV2/`.
The official page 66 distinguishes one left gripper, one right gun arm and a
separate opposing craft. Future mixed-set edits must specify ownership and
object counts and distinguish deleting an excluded model from replacing a tool.
Record guards protect these requirements and selected bytes; they do not
automatically prove image topology or pixel-exact preservation.

## Remaining scope / manual judgement

Claw's appearance is now accepted; do not ask for its approval again.
The next completed four-image batch is recorded in
`Docs/Development/M85_T082_OPEN_APPEARANCE_BATCH.md`: Crystal Reaper,
Alien Strike, Jet Scooter and Red Planet Protector. Source resemblance and
tool/body composition need human judgement, not another size/blockout gate.
This batch cannot approve the 66-entry gallery, MT-101, configurations or production.

Continuation 2026-09-14: MT-101 now selects its first controlled completed
native-to-raster appearance, still unreviewed. Native construction remains
internal control; the old unresolved FullV2 raster is no longer selected.
See `Docs/Development/M85_T082_MT101_CONTROLLED_APPEARANCE.md` for the exact
prompt, source references and visible-wheel limitation. Earlier unreviewed Jet Scooter,
Strike and Protector records remain unreviewed; no historical user finding is
silently reclassified. Mothership's operators/raised printing and the two
Martian archival gaps also remain disclosed. T082/T083 remain HOLD.

## Files / verification / regression / build

Latest Claw approval/MT finishing continuation: new separate approval record,
MT completed raster/prompt/reference copies/provenance and eight added guards
(all previous 61 retained). `./tools/verify.sh`: PASS 23 fast blocking stages,
317 NUnit tests and 69 review guards, zero blocking/diagnostic failures,
at `Artifacts/Verification/20260913T222650Z-fast-summary.txt` (UTC).
Validator, byte-exact gallery regeneration and `git diff --check`: PASS.

Files: Claw image/prompt/reference/edit record, separate MX image-review record,
gallery generator + generated HTML/
selection manifest, validator/review guards and current development notes.
No gameplay/runtime/Canon/import/binary formats changed; no dependency added.
The initial nine new negative guards cover latest approval evidence, production/other-asset
overclaims, occlusion, exact gallery roster, selected MX, alternate defense head
and unresolved MT context. All previous 47 cases remain. Five later Claw guards
also reject reversed arm roles, converting excluded craft into tools, inferred
acceptance, hidden extra retries and selecting the old extra-claw image.

Claw correction focused checks: `python3 tools/Validation/test_m85_super_scout_review_guards.py`
PASS 61/61; `python3 tools/Validation/validate_m85_super_scout.py` and
`python3 tools/generate-m85-current-comparison.py --check`: PASS.
`git diff --check`: PASS. `./tools/verify.sh`: PASS all 23 fast blocking stages,
317 NUnit tests and 61 review guards, zero blocking or diagnostic failures.
Summary: `Artifacts/Verification/20260913T185521Z-fast-summary.txt`.

Earlier gallery assembly recording suite: `python3 tools/Validation/test_m85_super_scout_review_guards.py`
PASS 56/56. `python3 tools/Validation/validate_m85_super_scout.py` and
`python3 tools/generate-m85-current-comparison.py --check`: PASS.
Static HTML parsing resolved all 67 image links to existing files (missing=0).
`./tools/verify.sh`: PASS all 23 fast blocking stages, 317 NUnit tests and
56 review guards; zero blocking or diagnostic failures. Exact summary:
`Artifacts/Verification/20260913T183933Z-fast-summary.txt`.
`git diff --check`: PASS. The first sandbox `./tools/verify.sh` stalled before restore output and
was terminated (exit 143, not PASS); the identical harness was rerun with
access to the already installed .NET environment, with restore then passing.
The gallery generator supports byte-exact regeneration and checks every source
PNG hash. Browser inspection of the local
HTML was blocked by browser URL policy; no bypass was attempted. Browser layout
and filter interaction are therefore not claimed verified. This does not block
the original PNG candidates displayed above or their static provenance checks.

No new playable build/export or production integration. Canon impact: NONE.
Work remains on `codex/m85-t082`, not merged.
