# LEGO SPACE RTS — CURRENT PROJECT STATE

The sole dynamic project-status handoff. Canon and newer Git evidence prevail;
this summary distinguishes director acceptance from Git integration.
Historical snapshots linked below are not current instructions.

## Accepted baseline

- Director-accepted development base: `633542a` on `codex/m85-t082`, closing
  the T082 reference foundation on 2026-09-23.
- Last verified upstream `origin/main`: `a6187e8`; the accepted T082 base is
  75 commits ahead and is not yet merged there. Do not start from older main
  and infer that accepted T081/T082 work is missing or must be repeated.
- Core remains Godot 4.7.1-stable .NET/C#, engine-independent deterministic
  SimCore, fixed 20 Hz and project-owned navigation. No architecture change.

## Current milestone

**M8.5 — Full Content & Presentation Production**, between M8 and M9.
Phase 09C requires production designs/models for 35 units and 31 infrastructure
entries, animation/VFX, icons/portraits, frontend/UI, audio and human acceptance.
The complete M8 data roster alone is not M9 Skirmish Alpha presentation.

## Completed and accepted

- M0–M6: simulation foundation, movement/selection/camera/fog, economy,
  construction/production/capacity/energy, combat/repair/transport/transform,
  faction systems and dedicated-server command/snapshot/reconnect/replay stack.
- M7 production visual direction: **M7 Final, outline on**; accepted default
  opens at 84 cells, with continuous 24–108 zoom and exact 24/44/72 review bands.
  Current/Hybrid are comparison modes. Prototype geometry is not final art.
- M7 material, lighting, animation, VFX/destruction, HUD and legal-minimap
  foundations exist. Style/Palette/Look labs remain investigative tools;
  historical exploration does not reopen the accepted production direction.
- M8 T070–T074: 35 unit chassis, 31 infrastructure definitions, 38 research
  definitions and 35 stable command families with canonical bindings and
  content-reference validation. Approved T073 values are recorded separately.
- T081: Blender → GLB → Godot production pipeline is implemented, verified
  and director-accepted; scale, orientation, LODs, pivots/sockets and materials
  have an accepted reference fixture.
- T082: the complete 66-asset source/reference corpus is director-accepted.
  Four faction audits cover 43 source records across 48 official books plus
  one archival scan, with semantic construction/motion/material planning.
- Renewed 24/44/72 diagnostic boards received positive general review.
  There is no per-code 66/66 score; do not manufacture one or repeat that gate.
- T082 accepts research and recognizable reference identity, not 66 final
  production designs/models. Asset-specific design acceptance remains required.

## Work in progress

- T083 design work exists separately on `codex/m85-t083` as uncommitted
  proposals. It is not part of the accepted baseline or this cleanup.
- That local work includes eight Rock Raiders proposals; none has T083 design
  acceptance. The remaining 27 packages are queued. Do not overwrite, import
  or promote those proposals during maintenance.
- Workflow cleanup is isolated on `codex/workflow-cleanup` from `633542a`:
  documentation compression and verification routing only. No gameplay, assets,
  visual direction or canonical decisions change.
- T084 production modeling has not started from these proposals.

## Blockers and deferred gates

- `BLOCKING_NOW`: each T083 asset needs explicit director design acceptance
  before its T084 model proceeds; research acceptance is not design acceptance.
- `BLOCKING_LATER`: actual gameplay-camera/model-LOD review belongs to
  T084/T086 after production geometry exists; final animation belongs to T087.
- Martian sources 1195/3750 retain bounded archival evidence gaps. Do not invent
  unseen construction. Preserve their documented later-phase ownership.
- `BLOCKING_LATER — M9 large-battle acceptance`: Stress60 still exposes
  mid-route corridor traffic/yield deadlock. It remains a visible diagnostic
  in full verification before M9, not a blocker for bounded production work.
- Representative 24-mover behavior remains covered by the NUnit suite.
- Failed portal-flow/local-pressure research (`6105cf0`, `f896b01`) is preserved,
  not production-ready. Another traffic coordinator/solver attempt requires
  `ARCHITECTURE REVIEW REQUIRED`; no third movement attempt is authorized.
- M9 acceptance must wait for the required M8.5 T083–T092 production work.

## Next approved action

After cleanup review/merge, open one bounded T083 unit design-package task from
the accepted integrated base, using the template in
[M85UnitDesign/README.md](M85UnitDesign/README.md). Reconcile the separate local
T083 work without discarding it; this cleanup does not continue its proposals.
Use accepted T082 packets and exact director-selected references, including
Alien Jet/Mothership, MT-101 nested craft/bike and MX-71 four emitters.
Do not infer gameplay from LEGO source modules or reopen accepted references.
Use package-specific checks and the targeted design-package profile; no game
export, M6 networking or M7 exploration stack is required for design-only work.

## Detailed records and historical evidence

- [Workflow and verification profiles](AGENT_WORKFLOW.md).
- [Technical decisions](TECH_DECISION_LOG.md).
- [M7 continuity](M7_THREAD_HANDOFF.md) and
  [accepted visual direction](M7_VISUAL_ACCEPTANCE_CANDIDATE.md).
- [M8 command bindings](M8_T073_BINDING_REVIEW.md).
- [T081 asset pipeline](M85_ASSET_PIPELINE.md).
- [T082 reference foundation](M85_SUPER_SCOUT_REFERENCE_INTELLIGENCE.md),
  [progress/closure detail](M85_SUPER_SCOUT_PROGRESS.md),
  [packet index](M85SuperScout/PACKET_INDEX.md) and
  [renewed blind review](M85_T082_CURRENT_BLIND_REVIEW_V1.md).
- [Complete pre-cleanup PROJECT_STATE](Archive/PROJECT_STATE_2026-09-23_pre_workflow_cleanup.md)
  preserves the exact committed 1,409-line narrative, old results and evidence
  paths from `633542a`. Its older open/hold statements are historical only.
