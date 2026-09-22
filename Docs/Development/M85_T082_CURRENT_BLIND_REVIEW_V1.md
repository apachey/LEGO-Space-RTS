# T082 — renewed current blind review V1

Prepared: 2026-09-21. Handoff: 2026-09-22. Branch: `codex/m85-t082`, not merged.
Canon impact: NONE.

## Purpose

The historical blind boards remain immutable evidence of their failed or partial
reviews. `CurrentBlindReviewV1` is a separate complete review package built only
from the latest 66 selections in `FullV2/CurrentComparisonV1`. It does not
rewrite the previous V/S-code results and does not promote any image to a
production model.

The review generator verifies the exact current-selection manifest hash and the
hash of every selected image before producing a board. It then assigns a fresh
randomized `R01`–`R66` order, removes colour cues, applies the same
footprint-relative scale rule at 24, 44 and 72 camera cells, and emits two pages
of 33 cards at every width. The same R-code order is preserved at all three
widths so recognition changes can be compared without changing identity.

There are 66 reviewed roster identities and 67 stored current views. ETX Defense
Node's Ground Pulse view is the reviewed primary image; its Air Lance view is a
preserved alternate configuration, not a 67th roster asset, and is excluded
from this blind pass.

## Active review sequence

1. Review both 24-cell pages together: page 1 contains `R01`–`R33`; page 2
   contains `R34`–`R66`.
2. Record the director's raw identification for every code. `Не знаю`, an
   uncertain guess and a collision with another model are valid findings and
   must not be converted into a pass.
3. Any wrong, uncertain or indistinguishable result stops the sequence at 24
   cells. Revise those appearances and issue a new version with fresh blind
   codes before repeating the complete 24-cell pass.
4. Only a clean renewed 24-cell result unlocks the already prepared 44-cell
   pages; a clean 44-cell result then unlocks 72 cells.
5. The answer key, names, factions, roles, selected paths and current statuses
   remain withheld until the corresponding blind answers are recorded.

Current state: `HOLD_FOR_GAME_DIRECTOR_24_CELL_REVIEW` (`BLOCKING_NOW`). The two
24-cell pages are the only active review surfaces. The prepared 44/72 pages are
intentionally not part of this handoff yet.

## Reproducible artifacts

- Generator: `tools/generate-m85-current-blind-review.py`
- Review directory:
  `Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentBlindReviewV1/`
- Public review manifest: `review_manifest.json`
- Active pages: `blind_24_cells_page_1.png`, `blind_24_cells_page_2.png`
- Withheld mapping: `WITHHELD_ANSWER_KEY.md`

The manifest locks the source-selection hash, board seed `85084`, R-code range,
six board hashes, answer-key hash, exact counts and gate state. The validator
also rejects identity leakage through the public manifest, roster drift,
changed selected bytes, mismatched board dimensions, accidental reuse of the
historical order, and non-reproducible output.

## Scope boundary

This is a recognition gate for preproduction appearances. Passing it would not
by itself approve exact LEGO geometry, construction, animation, materials,
alternate configurations, gameplay-camera capture or T083/T085 production.
Mothership source corrections and the two disclosed Martian archival evidence
gaps remain separate obligations. Gameplay, simulation, dependencies, public
formats and canon are unchanged.

## Verification

- `python3 -B tools/generate-m85-current-blind-review.py --check`: PASS, six
  reproducible boards, 66 identities, 24/44/72 widths, HOLD state.
- `python3 -B tools/Validation/validate_m85_super_scout.py`: PASS.
- `python3 -B tools/Validation/test_m85_super_scout_review_guards.py`: PASS,
  136/136 scenarios.
- `./tools/verify.sh --full`: PASS with zero blocking failures at
  `Artifacts/Verification/20260920T213037Z-full-summary.txt`; 317/317 NUnit
  tests, T082 integrity/guards, deterministic/replay/snapshot checks,
  headless/network presentation gates, byte-identical content/GLB regeneration
  and macOS export passed. The retained Stress60 result is the expected
  `BLOCKING_LATER` M9 diagnostic failure and does not block this T082 review.
- `git diff --check`: PASS.

The final full run includes the tracked IBM Plex font used by the boards.
The preceding sandboxed run failed during .NET restore and was stopped with
exit 143; it is not counted as PASS. The completed full rerun above supersedes
that interrupted verification attempt.

## Completion report

- What changed: six complete current-selection boards are prepared; the two
  24-cell pages are ready for director review.
- Why: later accepted corrections must be reviewed together at comparable
  sizes without altering the previous review's source images or results.
- Files changed: new board generator and artifacts; review-status record;
  validator and four guard cases; development handoff and project-state notes.
- Automated verification: exact executed commands and final results are above.
- Regression coverage: 132 existing review guards plus four new cases covering
  automatic acceptance, production overclaim, stale selection and lost alternate
  accounting; all 136 pass.
- Build: export and smoke succeeded at `Builds/macOS/LEGO Space RTS.app`.
  The review images remain preproduction artifacts, outside the playable models.
- Manual review requested: identify each code on the two 24-cell pages; record
  uncertain guesses, unknowns and confusing silhouettes in the director's words.
- Risks / unresolved issues: these are footprint-relative reference boards, not
  actual game-camera captures. Complete T082 acceptance and the source/production
  obligations listed above remain open. Existing Stress60 is `BLOCKING_LATER` M9.
- Canon impact: NONE.
- Branch / worktree: `codex/m85-t082` in the shared project checkout; not merged.
