# T082 — MT-101 nested source-assembly correction

Date: 2026-09-14. Branch: `codex/m85-t082`, not merged. Canon impact: NONE.

## What changed / why

The director identified that MT-101 had already been generated and that its
rear spacecraft is an integrated detachable module containing a mini-bike.
The subsequent `так` authorizes correcting source decomposition/model
requirements, not unreviewed gameplay roles or another generation retry.

Correct source structure:

```text
MT-101 heavy chassis (permanent front cabin)
└── directly docked detachable rear spacecraft (own cockpit)
    └── contained extractable two-wheel mini-bike
```

The opposing Alien scout in the mixed set is excluded. Neither human module is
an unrelated support build to omit. The bike is not loose cargo on MT-101;
its source parent is the rear spacecraft. Internal stowage need not be visibly
exposed in every docked exterior view, but fit/extraction must be established
before claiming a complete model. No speculative cavity is inferred from a
polished raster.

## Root cause / evidence

The prior source audit mislabeled book 1 p15-29, which builds the rear spacecraft,
as the forward mission cockpit. The production contract then explicitly
excluded the human support flyer and never mapped its mini-bike. Later native
and raster prompts restored a rear craft without correcting the source chain.
Previous automated passes proved consistency of those incomplete records,
not completeness against every source assembly.

The preceding read-only investigation visually inspected the cached official
instructions: book 1 p8 completes the two-wheel bike; p15/p29 builds and completes
the spacecraft. Book 2 p35 shows the permanent front cabin on the heavy chassis;
p43 explicitly shows both levels of separation. Sources:
[book 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf),
[book 2](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf).
Their existing hashes/page counts remain in the source evidence ledger.

## Files / regression coverage

- Source evidence now names the three source components, exact parents,
  connections, page evidence, excluded opponent and source/gameplay boundary.
- The MT recognition thesis retains the rear-module docking seam; contained
  bike is a construction obligation, not an always-visible silhouette anchor.
- MT construction/material requirements retain both human modules and credible
  internal stowage/extraction clearances. Generated MT packet, Mission Vehicle
  Bay donor packet and source/construction/material matrices are regenerated.
- Source-analysis policy gains a nested-vehicle safeguard and the 7699 case.
- Separate `mt101_nested_structure_review.json` records the latest director
  finding and current `REVISION_REQUIRED_INCOMPLETE_NESTED_ASSEMBLY` status.
  Current gallery uses that later review, not generation-time status. Images,
  prompts, native sources, historical boards and attempt counts are untouched.
- Nine new negative tests preserve the previous 69 (78 total): missing bike,
  wrong bike parent, missing page evidence, restored support-flyer exclusion,
  omitted semantic bike, invented independent gameplay, inferred image acceptance,
  hidden gameplay decision and lost rear-module recognition.

This is static source/model metadata and claim validation, not a runtime graph,
new engine subsystem or automatic visual-topology checker. Existing roster,
combat, cost, movement, transport and ownership behavior remain unchanged.

## Automated verification

- `python3 tools/generate-m85-super-scout-packets.py --check`: PASS (66 packets, 19 matrices).
- `python3 tools/generate-m85-current-comparison.py --check`: PASS (66 assets, 67 views).
- `python3 tools/Validation/validate_m85_super_scout.py`: PASS; T082 remains HOLD.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS, 78/78.
- `./tools/verify.sh`: PASS, 23 stages; 317/317 NUnit and 78/78 review guards.
  Summary: `Artifacts/Verification/20260913T224456Z-fast-summary.txt`.
- `git diff --check`: PASS.

`./tools/verify.sh --full`: actual summary FAIL, solely the accidentally
interrupted NUnit stage described below; all other 28 blocking stages PASS.
Summary: `Artifacts/Verification/20260913T224751Z-full-summary.txt`.
The replacement NUnit stage passes 317/317. Golden100, replay, snapshot,
byte-identical GLB regeneration and macOS export PASS. The preserved 60-mover
M9 diagnostic remains DIAGNOSTIC_FAIL (2/60 completion); it is BLOCKING_LATER,
not a new gate or an authorized navigation architecture task.

The first focused run failed because the old
policy validator still required exactly six analysis safeguards after the seventh
was added; the guard now requires seven distinct entries and the exact nested
entry. No previous test or acceptance gate was removed or weakened.

During full verification the agent accidentally interrupted the NUnit stage;
that run retains its actual FAIL rather than rewriting its log. A direct retry
without the project environment also aborted because the installed runtime is
.NET 10 while tests target net8.0. No dependency was installed or framework changed.
The actual replacement stage, using the existing harness environment, passed
317/317: `bash -c 'source tools/lib/common.sh; setup_dotnet_environment; dotnet test SimCore.Tests/SimCore.Tests.csproj -c Release --no-build --no-restore --disable-build-servers --verbosity minimal'`.

## Build / manual playtest / risks

No new generation, production integration or manual playtest requested.
Full verification exported and smoke-tested `Builds/macOS/LEGO Space RTS.app`;
this is the existing playable prototype, not an integrated MT-101 model.
Source records are corrected; existing raster/native models
still do not establish complete bike fit/extraction. T082 remains HOLD.

Independent rear-spacecraft/bike roles, commands, costs and entity ownership
are not specified by Phase 03's ground-only MT-101 entry. The correction does
not invent them or change Canon. A separately reviewed director gameplay decision
is required before implementing independent operation, not before correcting
the source model. The next MT modeling work must validate the complete source
assembly rather than repeat an entire free-form MT image. Other already approved
T082 review work may continue.

Work remains on `codex/m85-t082`; no merge or push.
