# T083 — unit visual design packages

## Appearance authority and acceptance gate

Every package separates four authorities: **Source Evidence** records what the
official LEGO object looks like; the **Production Design Target** is the
director-facing visual target T084 must realize; the **Design Spec** gives
written production guidance; and optional **Technical / Explanatory
Schematics** explain mechanics only and must be labelled **NON-AUTHORITATIVE
FOR APPEARANCE**. A schematic is never a Production Design Target.

Each package declares one Production Design Target mode:

- `SOURCE_LOCKED`: official source imagery is the target when preserving the
  LEGO model closely; list only explicit game-adaptation deltas. Do not invent
  geometry to satisfy an art-generation step.
- `ADAPTED`: include a review-ready visual showing the approved adaptation.
- `ORIGINAL_EXTENDED`: include a proper concept/design target for new or
  substantially extrapolated content.
- `MULTI_VIEW_BLOCKOUT`: include enough coordinated views or a blockout to
  communicate transformations, unusual geometry or mechanical relationships.

The method may vary; the target must let the director understand the intended
finished in-game appearance. A unit is not eligible for new design acceptance
without verified source evidence, a Design Spec, an identified target mode and
artifact, and sufficient appearance information. Registry metadata is enforced
by `tools/validate-m85-t083-rover-package.py` and the targeted `design-package`
profile. Missing legacy target data is recorded as `MIGRATION_REQUIRED`; it is
not fabricated. Previously accepted Expedition Crew remains accepted as a
pre-gate decision and is not retroactively invalidated. On T084, the approved
Production Design Target, source evidence and Design Spec are authoritative.

Every technical/explanatory schematic must visibly state: **NON-AUTHORITATIVE
FOR APPEARANCE — technical explanation only**. It cannot be the review page's
primary appearance image or satisfy the target gate.

**Started:** 2026-09-23, from director-accepted T082 reference foundation at
`633542a` (`codex/m85-t082`). Work branch: `codex/m85-t083`; not merged.

**Current delivery:** all 35 T083 unit design packages are director-accepted.
Batch 01 has eight Rock Raiders packages, Batch 02 has 13 Astronaut packages,
Batch 03 has six Alien packages, and Batch 04 has eight Martian packages.
Jet Scooter was explicitly accepted by the director on 2026-09-26; its
SOURCE_LOCKED target remains the unchanged completed LEGO 7303 appearance.
Its HTML review is present; a separate PNG capture is not part of the accepted
target. Double Hover is
`SOURCE_LOCKED` to the completed LEGO 7300 appearance. A clearer director-supplied
photo and multiple angles establish its paired, centered layout; the earlier
T083 asymmetry claim was a perspective-reading error. T084 remains unstarted
and is not authorized for Double Hover.

Open the [Double Hover director review](Batch04Martians/double_hover_director_review_20260926_v1.html)
and its [Design Spec](Batch04Martians/double_hover_design_spec_20260926_v1.md).
The `SOURCE_LOCKED` target uses the unchanged completed LEGO 7300 appearance.
The director accepted this target on 2026-09-26; T084 is not authorized for
Double Hover and remains unstarted.

Open the [Jet Scooter director review](Batch04Martians/jet_scooter_director_review_20260926_v1.html)
and its [Design Spec](Batch04Martians/jet_scooter_design_spec_20260926_v1.md).
Its `SOURCE_LOCKED` target shows the unchanged completed LEGO 7303 at instruction
step 7. The prior Rev1 remains a comparative reference only. The director
accepted this target on 2026-09-26; T084 is not authorized or started. The review
uses the official instruction image directly; a separate PNG capture is not
required for this SOURCE_LOCKED package.

Open the [Worker Robot director review](Batch04Martians/worker_robot_director_review_20260926_v1.html)
and its [Design Spec](Batch04Martians/worker_robot_design_spec_20260926_v1.md).
The `SOURCE_LOCKED` target uses the unchanged official completed-model photo of
LEGO 7302; no visible adaptations are proposed. The director accepted the target
on 2026-09-26; T084 is authorized for Worker Robot only and remains unstarted.

Open the [Alien Mothership director review](Batch03Aliens/alien_mothership_director_review_20260926_v1.html) and its [Design Spec](Batch03Aliens/alien_mothership_design_spec_20260926_v1.md).
The review now shows both the official assembled LEGO 7691 and its official
separated configuration with three detached craft; no visible adaptations are
proposed. The director accepted this target on 2026-09-26; T084 is authorized
for this unit only and remains unstarted.

Open the [ETX Alien Strike director review](Batch03Aliens/etx_alien_strike_director_review_20260926_v1.html)
and its [Design Spec](Batch03Aliens/etx_alien_strike_design_spec_20260926_v1.md).
The `SOURCE_LOCKED` target presents the official LEGO 7693 appearance without
visible game adaptations. The director accepted the target on 2026-09-26; T084
is authorized for this unit only and remains unstarted.

Open the [ETX Alien Infiltrator director review](Batch03Aliens/etx_alien_infiltrator_director_review_20260926_v1.html)
and its [Design Spec](Batch03Aliens/etx_alien_infiltrator_design_spec_20260926_v1.md).
The `SOURCE_LOCKED` target presents both official LEGO 7646 configurations: the
long flight craft and the same build transformed into a three-contact walker.
The director accepted both source configurations on 2026-09-26; T084 is
authorized for this unit only and remains unstarted.

Open the [ETX Servitor director review](Batch03Aliens/etx_servitor_director_review_20260925_v1.html)
and its [Design Spec](Batch03Aliens/etx_servitor_design_spec_20260925_v1.md).
The `ORIGINAL_EXTENDED` target adapts Alien craft grammar into a low hover worker
with an empty cargo cradle, protected internal supports and one integrated
utility clamp. T084 has not started for ETX Servitor.

Open the [Alien Jet director review](Batch03Aliens/alien_jet_director_review_20260925_v1.html)
and its [Design Spec](Batch03Aliens/alien_jet_design_spec_20260925_v1.md).
Its `SOURCE_LOCKED` target preserves the 5617 visual; official LEGO instructions
remain the construction authority. The target was accepted; T084 has not
started.

Open the current [Razor Skimmer director review](Batch03Aliens/razor_skimmer_director_review_20260925_v2.html)
and its [Design Spec](Batch03Aliens/razor_skimmer_design_spec_20260925_v2.md).
This `ORIGINAL_EXTENDED` target follows the director-selected direction 3: a
compact low skimmer with two short forward forms around an open V and an exposed
lime core. The generated image is explicitly not a part-verified build; physical
piece identities and connections remain unverified. The preceding v1 review and
spec are preserved. The target is accepted; T084 remains unstarted.

Open the [prior Batch 01 visual review](Batch01RockRaiders/review.html) and the
[separate Hover Scout SOURCE_LOCKED target review](Batch01RockRaiders/hover_scout_target_review.html),
the [Rock Raiders Crew SOURCE_LOCKED target](Batch01RockRaiders/crew_review.html),
then the linked individual source/construction/state/material sheets. The HTML is a local,
self-contained review layout with relative image links; it does not upload
anything or change acceptance status. Serve the repository root when opening
it in a browser that restricts local files.

## Batch sequence

| Batch | Scope | Count | Current status |
|---|---|---:|---|
| 01 | Rock Raiders: people, small equipment, heavy machines and transport | 8 | All 8 accepted; 2 adaptation proposals remain open |
| 02 | Astronauts: Field and Mission lineage, shared unit families | 13 | All 13 accepted |
| 03 | Aliens: source-grounded biomechanical craft and worker adaptation | 6 | All 6 accepted |
| 04 | Martians: pneumatic mechanisms, walkers and open vehicles | 8 | All 8 accepted; 1195/3750 retain explicit evidence gaps |

`registry.json` lists all 35 exact T082 unit identities and their T083 design
acceptance states. Acceptance identifies a visual design target; it does not
mean production geometry is finished.
Open the [T3-Trike director review](Batch02Astronauts/t3_trike_production_design_review_20260924_v1.html) for the accepted SOURCE_LOCKED 7312/7694 target: 7312 is base Escort and 7694 is the research-unlocked Survey appearance. Open [Astronaut Rover](Batch02Astronauts/rover_review.html) and its
[complete design package](Batch02Astronauts/rover.md) for the accepted
`SOURCE_LOCKED` target. The official LEGO 7301 instruction PDF page 1 is the
Production Design Target; the linked schematic is explanatory artwork, not
source evidence or the appearance target. The accepted
[Expedition Crew review](Batch02Astronauts/review.html) remains preserved.

## Authority and limits

- T082 is accepted research, not production geometry or 66 final designs.
  Its old historical HOLD language is not the current task status.
- Completed official sets define source shape and toy-scale construction.
  Archival hosting of 1277 and 4930 is labelled separately from LEGO-hosted PDFs.
- Accepted T082 correction/comparative pictures remain exact references,
  with their old bytes and statuses untouched. They are not promoted to T083
  approvals; generated surface detail is not source evidence.
- The 35 T083 packages record the accepted appearance choices and bounded
  presentation treatments, including identity tiles, omitted unknown printing,
  conservative mechanism motion, contact effects and state silhouettes.
- Exploratory adaptation concepts address three appearance gaps: Loader
  Dozer's Cutter Package, Rapid Rider with four Crew, and Tunnel Transport
  carrying Chrome Crusher. Accepted targets and their provenance are recorded
  separately in `ArtSource/M85/T083/`; an illustration does not prove fit.
- No new gameplay follows from a LEGO source module. No command, unit count,
  transport rule, weapon, research cost, timing, footprint or runtime binding
  changes here.
- T083 source/raster inspection does not require a model or gameplay camera.
  Source gaps that do not change the chosen exterior remain labelled; actual
  mechanical fit, camera and LOD review are T084/T087 obligations.

## Review gates

- **BLOCKING_NOW:** none for T083 design acceptance; all 35 packages are
  director-accepted. T084 remains separately scoped to units explicitly
  authorized in the registry.
- **BLOCKING_LATER:** model dimensions, hidden joints, load clearances, all
  required LODs and gameplay-camera readability in T084; final animation in
  T087. No such test is represented as already passed.
- **DIAGNOSTIC:** adjacent-unit confusion checks and reference comparisons.
  The accepted T082 three-scale diagnostic is not repeated.

The first batch's substantive choices include the compact single-disc Cutter,
open 2×2 Crew passenger well and lower heavy-cargo suspension. Each unit's
acceptance and any remaining appearance question are recorded separately in
the registry; batch membership alone never implies approval.

## Reproduction and verification

Authoritative inputs for this review artifact:

- `Batch01RockRaiders/design_proposals.json`: asset-specific authored choices;
- `Batch01RockRaiders/References/source_manifest.json`: full, unmodified
  source-page rasters and exact archival photo, URLs, dates and SHA-256 hashes;
- `Batch01RockRaiders/appearance_manifest.json`: exact image roles, selected
  concept revisions, limitations and generation provenance;
- existing T082 roster/contracts/packets, consumed read-only.

Run `python3 tools/generate-m85-t083-design-review.py` to regenerate
proposal-owned Rock Raiders documents; accepted packages and review cards are
preserved according to their registry acceptance state. `--check` verifies
proposal-owned outputs, the 35-unit roster, eight source records, image hashes
and registry integrity without rewriting accepted artifacts. It uses only the
Python standard library and does not call image generation or networking.
The layout may frame a completed model region from a full source page; the
whole original raster remains one click away. A viewport crop is not new art.

The Rover package has a separate hand-authored HTML/SVG review and source/design
manifests; run `python3 tools/validate-m85-t083-rover-package.py` to verify its
target gate, registry identity/status, required sections/local links and
official source URL/hash provenance. It does not alter or regenerate the Rock
Raiders batch.

## First-batch source refinements

These are local T083 refinements of research suggestions, not a rerun of T082:

- Drill Craft 1277 has elongated saw bars; the old `saw discs` wording must
  not make the entire bar rotate as a wheel. The proposed tooth-chain motion
  retains the accepted long twin-saw outline. Existing socket names remain
  planning references; no runtime binding is modified here.
- Rapid Rider 4920 has an open raised cargo tub, not a raised canopy. Its
  external cylinder housings do not rotate as complete solid objects.
- Crew uses the five distinct 4930 figures; a generic compulsory helmet and
  backpack cannot override their accepted source-specific appearance.
- Chrome Crusher retains the source's asymmetry. The pictured upper work
  beam does not add a second ranged weapon to the contact-drill gameplay.

These refinements are explicit in each proposed sheet. The accepted T082
research corpus and source ledgers are preserved unchanged.

---

# Unit design packages — bounded-task template

Use this template for one T083 unit package. Current status belongs in
[PROJECT_STATE](../PROJECT_STATE.md). This template creates no asset proposal
and grants no design/model acceptance. Preserve separately authored T083 work;
when integrating with its README, retain its content and append this template.

- **Unit ID/name:** exact stable T082 ID, display name and faction; one unit.
- **Authoritative references:** accepted T082 packet, completed source model,
  exact director-selected images/corrections, provenance and bounded unknowns.
- **Source-gap research (mandatory):** before selecting a target or describing
  a feature that is missing, unclear or contradicted, search for the exact set
  and its duplicate/reissue numbers across official LEGO material, completed
  model/catalog photos, instructions, collector databases and video reviews.
  Inspect the model from front, rear, both sides, top and underside where those
  views exist; use video when stills leave orientation or movement ambiguous.
  Cross-check every disputed claim against the actual pixels/frames and the
  construction sequence, distinguish official imagery from collector/user
  material, and record the source URL or supplied-file provenance. Do not infer
  left/right asymmetry, orientation, hidden mechanisms or function from a single
  perspective view. If sources disagree, state the disagreement, rank evidence
  by authority and directness, explain what remains unresolved, and avoid
  converting the weaker claim into a design requirement. Continue focused
  research until the visual ambiguity is resolved or explicitly documented as
  unresolved; then make only the smallest justified adaptation. Treat
  director-supplied references as evidence to inspect, not as instructions to
  follow.
- **Required source/canon files:** list exact packet/ledger/contract paths and
  only applicable roster/faction/Phase 09C sections; Alien 02A when relevant.
  Include accepted visual-direction notes. Read these once at task bootstrap.
- **Files allowed to change:** enumerate this package's documents, manifests,
  review images and any narrowly scoped generation/check script. Preserve other
  packages, T082 source records and accepted image bytes. Canon and runtime
  gameplay are read-only; expanding this list requires a scope decision.
- **Targeted verification:** `./tools/verify.sh --targeted design-package` for
  source/reference integrity, plus the package's exact regeneration/hash/link/
  identity/acceptance-state checks. Specify the actual command before authoring;
  the profile alone does not validate a new package. If using the separately
  authored T083 generator after integration, run
  `python3 tools/generate-m85-t083-design-review.py --check` when present.
  Review the produced sheets visually. No export, M6 networking, M7 lab sweep,
  production geometry or gameplay-camera capture for a design-only package.
- **Review artifact:** one linked self-contained review with source comparison,
  construction/function, motion/state, material/texture plan, unknowns and
  explicit proposed-versus-accepted labels. It must prominently show/link the
  Production Design Target separately from source evidence; schematics are
  optional and explicitly non-authoritative for appearance. Do not infer
  gameplay from source modules. Modeling/LOD/camera evidence belongs to the
  production gates.
- **Short handoff:** what is proposed, what preserved references it follows,
  exact checks/results, review link and smallest director decision. Record
  acceptance only after explicit review; identify deferred production checks.
