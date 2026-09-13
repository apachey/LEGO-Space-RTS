# T082 — remaining composed-candidate appearance review

Date: 2026-09-13. Branch: `codex/m85-t082`, not merged.

Latest director response `+` to the two-image handoff from commit `ea19e42`:
Solar's completed appearance and Mothership's overall connected docked
composition are accepted for comparative review. The operator/relief defects
explicitly disclosed below remain unaccepted and pending. Exact image hashes,
approval evidence and limited scope are now recorded separately in
`Content/Presentation/SuperScout/completed_composition_review.json`.
The original generation-time statuses below are historical, not current approval
states; FullV2 history and all production/gameplay gates remain untouched.

## What changed / why

The director's `+` accepts the three completed Alien appearance images shown
from commit `b05ede8`. Their manifest now records that limited approval, stable
asset/configuration mapping and exact selected hashes. Seven new negative
guards reject missing evidence, expanded scope, production acceptance, loss of
a defense head, substituted primitive drafts, changed hashes and extra variants.
All 22 previous review guards remain. Historical Full V2 attempts and boards
are untouched.

Continuation now collects two existing completed candidates into one handoff;
neither was regenerated or accepted by the Alien three-image approval. These
are appearance concepts, not primitive layout sketches or production models.

## 1. Solar Energy Array

Role: Astronaut energy infrastructure. Canonical output, reserve, construction
costs and energy rules are unchanged. The director requested six conventional
flat human panels in two rows of three. Revision 2 retains a low planted shared
support frame and compact control block. 7315 supplies solar/support lineage;
flattening and repeating six panels is the disclosed new adaptation, not a
literal official alternate build. Underside brackets are partly occluded.

![Solar Energy Array completed candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/building_astronauts_solar_energy_array_composed_rev2.png)

Status: UNREVIEWED_COMPOSED_DESIGN_CANDIDATE, composed attempt 2.
Selected SHA-256: `647d7cdac0e6edf5ba512a982e734d977d9e0cf273d663ffade368a6c73986d6`.

## 2. Alien Mothership — complete docked composition

Role: one selectable Alien combat carrier, with attached modules and unfolding
presentation. This image does not authorize training or launching internal
units. 7691 informs the crescent front craft, circular open carrier, two seated
side modules, two outer jetpack modules and four tail blades. These are connected
parts of one unit, not separately rostered craft.

![Alien Mothership completed composition candidate](/Users/pavlosidash/Developer/Lego-Space-RTS/Docs/Development/M85SuperScout/Silhouettes/FullV2/Renders/unit_aliens_alien_mothership_rev1.png)

Status: UNREVIEWED_CORRECTION_CANDIDATE, first complete docked correction.
Selected SHA-256: `95804c387d2ed74b13017466feec159552e92d7b952cb3ec4889a7dd94d6a11b`.

Known visible mismatch: generated operator heads/limbs are not the actual Mars
Mission minifigures. Some printed detail became relief. Neither is claimed
correct, accepted or source-verified. Any accepted overall craft composition
must retain this separate source-led operator/flat-detail correction obligation.
Exact joints, rear/underside topology and unfolding still need production work.

## Files / verification / regression coverage

Files changed: selected-appearance manifest; T082 validation and review tests;
current-state/progress/appearance notes and this grouped review document. No
image bytes, Full V2 manifests, runtime code or canon files changed.

Executed commands and results:

- `python3 tools/Validation/validate_m85_super_scout.py`: PASS, existing
  66-asset corpus HOLD retained and three selected appearances verified.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS 29/29,
  comprising seven new negative guards and all 22 existing guards.
- `git diff --check`: PASS.
- `./tools/verify.sh`: PASS, all 23 blocking stages, 317/317 NUnit tests,
  zero blocking or diagnostic failures. Evidence:
  `Artifacts/Verification/20260913T104926Z-fast-summary.txt`.

The two pending image SHA-256 values above were independently checked with
`shasum -a 256` against the retained historical manifest. No images were edited.

## Build / manual review / risks / canon / branch

No playable export or model integration is produced by this image-approval and
review-preparation block. Human review concerns only the two displayed completed
candidate appearances; no repeat judgement of the three accepted Alien images
or their dimensions is requested. Mothership operator/relief mismatch remains
explicit. MT-101/MX-71 two-attempt stops, complete 24/44/72-cell review and the
T082 production HOLD remain. Canon impact: NONE. Branch: `codex/m85-t082`,
not merged into main.
