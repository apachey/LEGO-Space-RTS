# T085 — infrastructure visual design packages

**Started:** 2026-09-26 on `codex/m85-t085`, from accepted `codex/m85-t082`
at `4858c32` (merged T083 PR #16). Not merged. T084 and T086 are not started.

Phase 09C T085 requires 31 game-director-approved source, silhouette,
construction, state and material sheets, one for each canonical
infrastructure entry. `registry.json` lists all 31 entries in the canonical
roster order of `roster_identity_baseline.json` / Phase 03 and records each
entry's proposal and acceptance state. Current status belongs in
[PROJECT_STATE](../PROJECT_STATE.md).

## Authority and acceptance gate

T085 reuses the accepted T083 authority split (see the
[T083 README](../M85UnitDesign/README.md)):

- **Source Evidence** records what the official LEGO object looks like, with
  URL, authority level, retrieval state and hashes.
- The **Production Design Target** is the director-facing appearance T086 must
  realize. Modes: `SOURCE_LOCKED`, `ADAPTED`, `ORIGINAL_EXTENDED`,
  `MULTI_VIEW_BLOCKOUT`.
- The **Design Spec** gives written production guidance: composition on the
  authoritative footprint, visible adaptations, construction, operational,
  brownout, damage and destruction states, gameplay-camera readability,
  materials/textures, pivots/sockets, LODs and confusion boundaries.
- Technical schematics and project-authored renders are review aids. They are
  never official imagery or source evidence.

Infrastructure adds three obligations to the unit template:

1. Map the authoritative footprint, non-rotation or rotation rule and SimCore
   production exit onto the design; the whole footprint is movement-blocked,
   so receiving and exit points sit on its edge.
2. Cover construction progress, operational, brownout, the four canonical
   damage thresholds and destruction/rubble presentation.
3. Record which official motifs the building consumes and which neighbouring
   infrastructure must not repeat, so shared sources (for example LEGO 4990)
   do not produce confusable buildings.

An entry is eligible for acceptance only with verified source evidence, a
Design Spec, an identified target mode and artifact, and sufficient appearance
information. Acceptance is recorded only after the director's explicit
decision; T086 authorization is a separate, explicit instruction.

## Current delivery

| # | Entry | Mode | State |
|---:|---|---|---|
| 1 | [Rock Raiders HQ](Batch01RockRaiders/rock_raiders_hq_director_review_20260926_v1.html) · [Design Spec](Batch01RockRaiders/rock_raiders_hq_design_spec_20260926_v1.md) | `ADAPTED` | Proposed, awaiting director review; not accepted |
| 2–31 | Remaining roster | — | Queued, not authored |

The Rock Raiders HQ package was authored in a cloud session whose network
policy denied every LEGO reference host. Its official LEGO 4990 facts come
from text search summaries; no official page, PDF raster or completed-set
photograph was inspected. Module appearance therefore stays locked to the
official instructions, and the composition renders are placeholders for
module form, colour and height until that pixel check happens.

## Batch plan

| Batch | Scope | Count |
|---|---|---:|
| 01 | Rock Raiders infrastructure | 8 |
| 02 | Astronaut infrastructure | 8 |
| 03 | Alien infrastructure | 6 |
| 04 | Martian infrastructure, including the Aero Tube Link | 9 |

## Reproduction and verification

- `./tools/verify.sh --targeted design-package` — existing T082 source
  integrity, review-recording guards and the T083 acceptance gate.
- `python3 tools/validate-m85-t085-rock-raiders-hq-package.py` — T085 registry
  roster/order/counters, HQ proposal state, source manifest provenance,
  render hashes, Design Spec sections and review links.
- `python3 tools/generate-m85-t085-rock-raiders-hq-target.py --check` —
  composition scene determinism and committed render hashes (no browser).
- `python3 tools/generate-m85-t085-rock-raiders-hq-target.py` — re-renders
  the composition views with a local Chromium or Chrome (`CHROME_BIN`).

Design-only work launches no game export, M6 networking, M7 exploration stack,
production geometry or in-engine camera capture. Actual gameplay-camera and
LOD review belongs to T086.
