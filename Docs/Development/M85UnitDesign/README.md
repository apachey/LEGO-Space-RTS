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
  explicit proposed-versus-accepted labels. Do not infer gameplay from source
  modules. Modeling/LOD/camera evidence belongs to the production gates.
- **Short handoff:** what is proposed, what preserved references it follows,
  exact checks/results, review link and smallest director decision. Record
  acceptance only after explicit review; identify deferred production checks.
