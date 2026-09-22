# T082 — current comparison and next priority candidates

Date: 2026-09-21. Branch: `codex/m85-t082`, not merged. Canon impact: NONE.

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

## Renewed blind-review package

`FullV2/CurrentBlindReviewV1/` now turns the exact current selection into a new
complete blind-review package without modifying any historical board. It uses
fresh randomized `R01`–`R66` codes, grayscale presentation and one stable order
across the two 24-, 44- and 72-cell pages. All six boards are prepared and
hash-locked, but only both 24-cell pages are active. Their 66 raw
identifications must be recorded before any 44/72 page is shown. Any wrong,
uncertain or indistinguishable result requires revision and a fresh blind
version rather than continuing at another size.

The package reviews one primary view per roster identity. ETX Defense Node's
alternate Air Lance configuration remains preserved in the current gallery but
is excluded from this pass; it is not a separate roster item. The answer key
and all identity-bearing metadata remain withheld during review. Protocol and
scope are recorded in
`Docs/Development/M85_T082_CURRENT_BLIND_REVIEW_V1.md`.

## Accepted correction records retained in the current gallery

### MT-61-derived Mobile Mining Platform — resource extraction

Canon Phase 03 §8 combines 7645, 7648 and the human 7693 vehicle into one
configurable economic family. Source-rebuild Rev2 is accepted for comparative
appearance review as the Crystal Reaper state: two harvesting cutters, two
manipulators and a directly docked upper spacecraft/processing module, not an
unsupported towing trailer. It does not add a new roster unit or approve a toy
removal mechanism as gameplay. Ore Drill state, service-controlled Mission
Refit and final production topology remain obligations.

![MT-61 Crystal Reaper accepted appearance](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/MobileMiningPlatformSourceRebuildV1/mobile_mining_platform_source_rebuild_rev2.png)

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

Claw, MT-101, Crystal Reaper, ETX Alien Strike, the explicitly selected Jet
Scooter Rev1 and Red Planet Protector source-rebuild Rev1 appearances are now
accepted for comparative review; do not ask for their approval again. The
completed priority batch is recorded in
`Docs/Development/M85_T082_OPEN_APPEARANCE_BATCH.md`. The priority filter is
empty; the next review is the renewed complete comparison/blind-review gate,
not another size or primitive-blockout gate. The renewed package is now ready
at 24 cells; 44/72 remain locked behind the complete 24-cell result.
These approvals cannot accept the 66-entry gallery, alternate configurations or production.

Continuation 2026-09-14: the director identifies incomplete MT-101 source
decomposition. Current selection retains the controlled raster bytes only as
`REVISION_REQUIRED_INCOMPLETE_NESTED_ASSEMBLY`, not a complete appearance proposal.
The directly docked rear spacecraft contains a two-wheel mini-bike; existing
native/raster controls do not establish its stowage/extraction. Source records
and model requirements are corrected without another generation or gameplay change.
See `Docs/Development/M85_T082_MT101_NESTED_SOURCE_CORRECTION.md`.
The original prompt and wheel occlusion remain documented in
`Docs/Development/M85_T082_MT101_CONTROLLED_APPEARANCE.md`. Jet Scooter's later
source-rebuild Rev1 and the completed source-corrected Red Planet Protector Rev1
are now explicitly accepted.
No historical user finding is silently reclassified. Mothership's
operators/raised printing and the two
Martian archival gaps also remain disclosed. T082/T083 remain HOLD.

## Files / verification / regression / build

Renewed blind-review package: six reproducible grayscale boards, a withheld
answer key, public manifest, generator, validator support and four new negative
guards. Focused regeneration/validation and all 136 review guards pass.
`./tools/verify.sh --full` passed every blocking stage, 317/317 NUnit tests and
a fresh macOS export at
`Artifacts/Verification/20260920T213037Z-full-summary.txt`. The expected
Stress60 `BLOCKING_LATER` M9 diagnostic remains non-blocking for T082.

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
