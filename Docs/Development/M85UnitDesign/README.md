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

---

# Unit design packages — bounded-task template

Use this template for one T083 unit package. Current status belongs in
[PROJECT_STATE](../PROJECT_STATE.md). This template creates no asset proposal
and grants no design/model acceptance. Preserve separately authored T083 work;
when integrating with its README, retain its content and append this template.

- **Unit ID/name:** exact stable T082 ID, display name and faction; one unit.
- **Authoritative references:** accepted T082 packet, completed source model,
  exact director-selected images/corrections, provenance and bounded unknowns.
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
