# T082 — MT-101 controlled completed appearance

Date: 2026-09-14. Branch: `codex/m85-t082`, not merged. Canon impact: NONE.

## What changed / why

The director's `далі` continues the recorded controlled native MT-101 next step
into one completed raster appearance. This finishes the internal construction
instead of asking for approval of primitive geometry or starting a third
free-form reconstruction after the two historical attempts.

MT-101 is a heavy assault vehicle against armored targets and structures,
not a resource harvester or a new Rock Raiders terrain-routing capability.
The completed appearance has panel/stud detail, a permanent closed front cabin
with a seated human operator, a separate upper circular ball launcher and
articulated side auger, and a visibly docked rear spacecraft with its own
cockpit, parallel equipment tubes and fins.

![MT-101 completed candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/mt101_completed_rev1.png)

Status: `UNREVIEWED_CONTROLLED_COMPLETED_APPEARANCE`. No production acceptance.
The current comparison uses this exact image, not the rejected old FullV2
raster or the internal native blockout. Claw's separate appearance acceptance
is recorded; Crystal Reaper, Strike, Jet Scooter and Protector remain open.

## Source controls / generation

Built-in `image_gen`, `sketch-to-render`, one raster finishing pass.
Historical free-form attempts: two; controlled native approach: one.
Five input roles distinguish layout from source references and appearance:

- Native three-quarter layout and overhead six-contact construction control.
- Official 7699 instruction book 2 cover, exterior reference only.
- Official book 2 page 43, rear-module and upper-launcher ownership only.
- Accepted Claw-Tank image, monochrome finish only, not MT geometry.

The cached official pages were visually inspected; no new PDF retrieval or
source claim from generated pixels. Official source:
<https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf>.
Exact inputs, copies, hashes and authority are in
`ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/edit_manifest.json`.
Full generation prompt:
[edit_prompt.txt](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/edit_prompt.txt).
Output SHA256: `0c1f5ed2eaea7103bc5f2c7bca3fce190d0a90f0a1ef728ba9576868e939e291`.

## Manual judgement / risks

Review the completed appearance's resemblance to MT-101 and cabin/tool/rear-craft
composition, not previously approved dimensions or an unfinished blockout.
The new PNG was visually inspected: four main wheels are visible; two far
contacts from the native construction remain occluded. The raster therefore
does not prove all six wheels. Source/native geometry may drift; no pixel-exact
preservation, certified LEGO buildability or complete source fidelity is claimed.
Final production topology, animation and gameplay-camera acceptance remain open.
T082/T083 remain HOLD; no other image acceptance is inferred from continuation.

## Files / regression coverage / automated verification

New MT image, full prompt, two cached official page copies and provenance record;
separate Claw approval record; gallery selection/generator, review guards and
current development notes. Original images, boards and generation-time review
records remain unchanged. No gameplay, runtime, dependency or Canon changes.
Four Claw authority guards and four MT finishing guards preserve the prior 61
cases (69 total), rejecting inferred acceptance, hidden retries, native-art
promotion and six-wheel raster proof. Guards validate provenance/claims, not
visual topology.

`python3 tools/Validation/validate_m85_super_scout.py`: PASS, preserving T082 HOLD.
`python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS 69/69.
`python3 tools/generate-m85-current-comparison.py --check`: PASS, exact 66 IDs/67 views.
`git diff --check`: PASS.
`./tools/verify.sh`: PASS all 23 fast blocking stages, 317 NUnit tests and
69 review guards, zero blocking/diagnostic failures. Exact summary:
`Artifacts/Verification/20260913T222650Z-fast-summary.txt` (UTC run ID).

## Build / branch

No new playable macOS export or production-model integration; image review only.
Work remains on `codex/m85-t082`; no merge or push.
