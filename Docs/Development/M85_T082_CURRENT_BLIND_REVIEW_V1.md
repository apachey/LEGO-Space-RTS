# T082 — renewed current blind review V1

Prepared: 2026-09-21. 44-cell continuation: 2026-09-22. Branch: `codex/m85-t082`, not merged.
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

1. The director saw both 24-cell pages (`R01`–`R33`, then `R34`–`R66`) and said
   verbatim: “йдемо далі, наче все зрозуміло”. This is a qualitative go-ahead,
   **not** 66 recorded identifications or a scored recognition pass.
2. Show both 44-cell pages together, retaining the same R-code order. Record
   any uncertain, unknown, wrong-looking or confusable silhouettes in the
   director's words; do not infer unspoken answers.
3. A clearly problematic asset returns to revision with new blind codes before
   formal acceptance. A further qualitative go-ahead may open the diagnostic
   72-cell view, but does not retroactively score 24 or 44 cells.
4. A full-roster production/readability claim still requires explicit
   game-director evidence and acceptance. The answer key, names, factions,
   roles, selected paths and current statuses remain withheld during the blind
   review.

Current state: `HOLD_FOR_GAME_DIRECTOR_44_CELL_REVIEW` (`BLOCKING_NOW`). Both
44-cell pages are active; 72 remains prepared but withheld. The recorded 24-cell
feedback permits this diagnostic progression only. No production image/model
was accepted by that general comment.

## Reproducible artifacts

- Generator: `tools/generate-m85-current-blind-review.py`
- Review directory:
  `Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentBlindReviewV1/`
- Public review manifest: `review_manifest.json`
- Active pages: `blind_44_cells_page_1.png`, `blind_44_cells_page_2.png`
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

44-cell continuation verification on 2026-09-23:

- `python3 -B tools/generate-m85-current-blind-review.py --check`: PASS; all
  six board bytes and the withheld key remain reproducible and unchanged.
- `python3 -B tools/Validation/validate_m85_super_scout.py`: PASS.
- `python3 -B tools/Validation/test_m85_super_scout_review_guards.py`: PASS,
  137/137 cases, including refusal to treat qualitative feedback as scored.
- `./tools/verify.sh`: PASS with zero blocking or diagnostic failures at
  `Artifacts/Verification/20260922T210607Z-fast-summary.txt`; 317/317 NUnit
  tests and the T082 review guards passed. An earlier sandboxed attempt stalled
  at .NET restore and was terminated; it is not counted as PASS.
- `git diff --check`: PASS.

## Completion report

- What changed: six complete current-selection boards are prepared; the two
  44-cell pages are now active after the director's general 24-cell go-ahead.
- Why: later accepted corrections must be reviewed together at comparable
  sizes without altering the previous review's source images or results.
- Files changed: new board generator and artifacts; review-status record;
  validator and four guard cases; development handoff and project-state notes.
- Automated verification: exact executed commands and final results are above.
- Regression coverage: the original 136 guards covered automatic acceptance,
  production overclaim, stale selection and lost alternate accounting. A further
  guard now protects the unscored nature of the 24-cell feedback.
- Build: export and smoke succeeded at `Builds/macOS/LEGO Space RTS.app`.
  The review images remain preproduction artifacts, outside the playable models.
- Manual review requested: inspect both 44-cell pages as one set and flag
  uncertain, unknown or confusing silhouettes by R-code.
- Risks / unresolved issues: these are footprint-relative reference boards, not
  actual game-camera captures. Complete T082 acceptance and the source/production
  obligations listed above remain open. Existing Stress60 is `BLOCKING_LATER` M9.
- Canon impact: NONE.
- Branch / worktree: `codex/m85-t082` in the shared project checkout; not merged.
