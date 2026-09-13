# T082 — restore the successful MX-71 appearance; localized weapons edit

Date: 2026-09-13. Branch: `codex/m85-t082`, not merged.

Later director review: the `+` to commit `6a211dc` accepts the displayed edited
MX-71 appearance for comparative review only. The separate current approval is
`Content/Presentation/SuperScout/mx71_localized_appearance_review.json`.
The generation-time candidate status below remains historical; inputs, prompt,
attempt count and image bytes are not rewritten. Far-mount occlusion remains
disclosed. MT-101/native geometry/production/T082 are not accepted by this review.

## What changed / why

The director corrected the previous native-render handoff: the earlier generated
MX-71 airframe was already good; only its four-emitter grouping needed work.
The implementation incorrectly treated a technical source-trace construction
as a requested new final appearance. The earlier FullV2 next-step record itself
said to retain the existing accepted airframe improvements. A layout check was
therefore not permission to replace that appearance with simplified geometry.

The subsequent `+` approves returning to the exact earlier image and editing
only the gun assemblies. One built-in `image_gen` precise-object edit now uses
that image as EDIT TARGET and official 7692 instruction page 69 as weapon-layout
reference only. The successful aircraft silhouette, closed canopy, long narrow
spine, hoses, side tubes, tail booms/fins and close underslung rover remain
visually retained. Exact unchanged pixels are not promised by generated imagery.

The MX-71 native source/renders are explicitly internal layout reference only,
not final appearance candidates. Their bytes and verified four-mount geometry
are preserved. MT-101 is not accepted or reopened by this MX-only approval.

## Completed edited appearance

![MX-71 localized weapon edit](/Users/pavlosidash/Developer/Lego-Space-RTS/ArtSource/M85/Preproduction/MX71LocalizedWeaponEditV1/mx71_weapons_only_rev1.png)

Status: `UNREVIEWED_LOCALIZED_WEAPON_EDIT_CANDIDATE`.
This new edit still requires director appearance acceptance; approval of the
base aircraft is not automatic approval of the changed weapons or a model.

Visible inspection: near inner and lower outer short emitters plus far inner
emitter are visible. The far outer mount remains occluded in the original
three-quarter camera. The image alone does not prove four correctly attached
guns. The source/native evidence still requires four mirrored forward mounts
in any future production model; engines and the payload auger are not guns.
No camera/airframe redesign was performed merely to expose that far mount.

## Provenance / director authority

- Exact edit target: retained
  `Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_astronauts_mx71_recon_dropship_rev1.png`;
  SHA-256 `513bdca746a01949ed56829bc6295a03b625e16c0617d71b5b1c4f9995e52b2b`.
- Official reference: [7692 instructions, page 69](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf).
  Cached page image is copied alongside the edit as `reference_7692_page69.png`;
  SHA-256 `e0ee470e7e7aa827dcd6bda3a6ea32624b24b4f65e3efd3485b7d466ee17040d`.
  Source colours and aircraft mass are not transferred to the edit target.
- Exact full prompt: `ArtSource/M85/Preproduction/MX71LocalizedWeaponEditV1/edit_prompt.txt`.
  Built-in tool only; no CLI/API fallback. The generated output was copied
  non-destructively into the project; the built-in original remains saved.
- Selected edit SHA-256:
  `1d89bed19d106cd3697f47796312e7e58f911ae85d94fa17ca0fc0ead6718c8f`.
- Separate `edit_manifest.json` records the latest director `+`, reviewed base
  commit `f3a9deb`, narrow edit authority, one localized call, input roles,
  exact hashes, unchanged production HOLD and disclosed occlusion.

The historical two free-form attempts and original FullV2 board/manifests remain
untouched. This is a separately authorized localized correction, not a hidden
third free-form aircraft reconstruction or permission for further variants.

## Files changed

- One edited PNG, its official reference page, exact prompt and scoped manifest.
- Native preproduction review-role metadata and generator now exclude MX-71's
  internal layout from final-appearance use. No native geometry bytes changed.
- T082 validation/review tests and current project/review notes. No runtime,
  gameplay, roster, source contracts, canonical data or import files changed.

## Automated verification / regression coverage

- `python3 tools/Validation/validate_m85_super_scout.py`: PASS; roster HOLD and
  original image/history hashes remain protected.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS 47/47.
  All previous 41 cases retained; six added guards reject lost/narrowed-away
  director authority, automatic candidate acceptance, substitution of the good
  base airframe, hidden extra attempts, false four-visible-mount proof and
  promotion of internal native layout into final appearance.
- Background generation/edit: one built-in image call completed and its output
  was visually inspected. It is a candidate image, not a correctness test.
- `./tools/verify.sh`: PASS, all 23 fast blocking stages, 317 NUnit tests and
  47 recording guards; zero blocking or diagnostic failures. Exact summary:
  `Artifacts/Verification/20260913T174729Z-fast-summary.txt`.
- `git diff --check`: PASS.

## Build / manual review / risks / canon / branch

No new playable export or roster-model integration is requested or produced by
this image-only correction. The existing exported game does not contain it.
Manual review concerns only the completed edited appearance above, especially
the short emitter grouping while retaining the good airframe. No repeat size
approval, unfinished-blockout review or game setup is requested.
Far outer gun occlusion, exact attachment topology and production fidelity
remain unverified by this raster. Complete T082 24/44/72-cell acceptance and
production HOLD remain. Canon impact: NONE. Branch: `codex/m85-t082`, not merged.
