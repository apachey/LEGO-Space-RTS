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
No new image was generated, edited or substituted during this continuation.

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
the tracks travel another way. This candidate separates the source set's Alien
ambush spacecraft from the human tank and retains the rotating cockpit and
articulated tool arms. Generated barrel proportions and claw posing remain
interpretation. It is still `UNREVIEWED_CORRECTION_CANDIDATE`.

![Claw-Tank candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_astronauts_mt51_claw_tank_rev1.png)

## Remaining scope / manual judgement

Review only the two completed appearances above: source resemblance and whether
their intended roles are readable. No dimension reapproval, setup, playtest or
primitive-blockout approval is requested. Approval of this pair would not
approve the 66-entry gallery, MT-101, configuration coverage or production.

MT-101 still needs a completed source-controlled appearance after its two
free-form attempts. The existing native source construction remains available
internally; it is not handed off as finished art. Its old FullV2 raster in the
gallery is visibly marked unresolved context. Earlier unreviewed Jet Scooter,
Strike and Protector records remain unreviewed; no historical user finding is
silently reclassified. Mothership's operators/raised printing and the two
Martian archival gaps also remain disclosed. T082/T083 remain HOLD.

## Files / verification / regression / build

Files: separate MX image-review record, gallery generator + generated HTML/
selection manifest, validator/review guards and current development notes.
No gameplay/runtime/Canon/import/binary formats changed; no dependency added.
Nine new negative guards cover latest approval evidence, production/other-asset
overclaims, occlusion, exact gallery roster, selected MX, alternate defense head
and unresolved MT context. All previous 47 cases remain.

Focused recording suite: `python3 tools/Validation/test_m85_super_scout_review_guards.py`
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
