# T082 — Alien size-only director acceptance

Date: 2026-09-13. Branch: `codex/m85-t082`; not merged.
Status: DIRECTOR_ACCEPTED_VISUAL_DIMENSIONS_ONLY.

## Review evidence and scope

The director rejected all three biomechanical revision-2 previews as too
detailed for official LEGO construction complexity and identified the Servitor
as oversized. The requested method compares existing accepted concepts and
official LEGO sources using a connector-grid ruler, not generated-image stud
counts. Merely shrinking a detailed image does not address construction density.

After the proposed controlled, shared-scale blockout method, the director
replied `+`. The delivered inline sheet showed all three simple volumes with
the same reference plate and schematic minifigure. The director then replied:

> розміри хороші

This accepts the displayed visual dimensions, not shape, donor composition,
anatomy, material, image, animation rig or production readiness. The simplified
sheet is not a physically buildable LEGO instruction or a final model.

## Accepted visual envelope

| Subject | Width × length, studs | Height, plates | Nominal physical comparison |
| --- | --- | --- | --- |
| ETX Servitor body | 6 × 8 | 8 | 48 × 64 × 25.6 mm |
| ETX Servitor with short clamp | 6 × 10 | Body height remains 8 | 48 × 80 mm plan envelope |
| Ground Pulse | 12 × 12 | 15 total | 96 × 96 × 48 mm |
| Air Lance | Same 12 × 12 base extent | 26 total | 96 × 96 × 83.2 mm |

Worker body height excludes ground-hover clearance. Defense height includes
its foundation and weapon head. Approval of equal defense base dimensions is
not independent approval of the four-pad foundation's exact shape/connections.
There is no approval of added cargo enlarging the worker's body or extra weapon
geometry beyond these bounds.

## Reference and provenance

The historical sheet is retained verbatim at
`M85SuperScout/Silhouettes/FullV2/alien-scale-blockouts.html`. Its labels still
say proposal: that is the exact state the director reviewed, not a stale new
approval request. This document records the subsequent size-only decision.
Sheet SHA-256: `9ad6322ba6bb8dea22a1ebee7fb55c75927ebdee90b6ed30d39bf5603e1fdda0`.

- Grid convention: nominal 8 mm stud-centre spacing, 3.2 mm plate thickness,
  following the [LDraw format specification](https://www.ldraw.org/article/218.html).
  These are comparison units, not factory-tolerance measurements.
- The 4 × 6 foundation plate visible at the beginning of the
  [official T3-Trike 7312 instruction](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130807.pdf)
  supplies a 32 × 48 mm reference. It is NOT the entire Trike's size.
- The minifigure is a schematic approximate 40 mm guide, not an exact sourced
  part drawing. T3-Trike full assembled bounds have not been reconstructed here.
- The small [official Alien Jet 5617](https://www.lego.com/en-us/service/buildinginstructions/5617)
  remains the worker's source-family simplicity comparison, not a claim that
  the worker uses identical dimensions or an official alternate build.

## Next bounded work and protection

Carry these accepted dimensions into controlled source-derived shape work.
Use large shell assemblies and restrained protected living connections; do not
restore the rejected dense ribs, armor layering and conduit decoration. Exact
source joints, shell profiles and functional tool/head connections remain open
for review. Do not treat approval of scale as approval of all schematic shapes.

No new free-form image generation occurred in this block. Existing composed
attempt-2 previews, generation hashes/statuses and two-attempt stops remain
intact. No third free-form generation is authorized by the size-only response.
Original accepted images, solar/Frontier review state and MT-101/MX-71 stops are
unchanged. All source/adaptation and complete 24/44/72-cell review gates remain.

Physical comparison dimensions are NOT gameplay build cells, collision,
selection, navigation clearance or automatic engine-world scaling. The accepted
T081 import convention and authoritative gameplay footprints are unchanged.
Canon impact: NONE. No gameplay/model integration or macOS export in this block.

## Documentation-block verification

- `python3 tools/Validation/validate_m85_super_scout.py` — PASS.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py` — 22/22 PASS;
  existing acceptance/attempt/image provenance guards reused without changes.
- `cmp` of the historical inline sheet and its repository copy — PASS.
- `git diff --check` — PASS.

This block records feedback and preserves the reviewed sheet only. No code,
runtime catalog, generated packet, acceptance manifest or production model
changed. `./tools/verify.sh` was not rerun for this documentation-only decision;
earlier harness results are historical, not claimed as a fresh run here.
No Godot launch or manual playable-build test was needed. Remaining human
review concerns source-derived exterior form and restrained detail, not size.
