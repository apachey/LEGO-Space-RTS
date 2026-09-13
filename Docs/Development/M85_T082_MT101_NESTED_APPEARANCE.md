# T082 — MT-101 nested construction and finished appearance

2026-09-14. Branch `codex/m85-t082`, not merged. Canon impact: NONE.

## What changed / why

The director's `+` after `b5c3dd4` continues the corrected source requirements
into a local native rear-bay correction and finished monochrome appearance.
MT-101 retains its permanent front cabin, six-wheel heavy chassis, upper launcher
and separate side auger. Its directly docked rear spacecraft contains a small
two-wheel human mini-bike, not unrelated cargo on the main machine.

The old solid rear deck left nowhere to stow/extract the bike. It is replaced
with two longitudinal bay rails, support floor and retained nose floor. The bike
belongs to the rear spacecraft, which belongs to MT-101. All 596 other retained
meshes' vertices, transforms and parents were checked unchanged, including MX-71.
Official cached instruction book 1 p8/p29 and book 2 p43 were visually inspected.
The upward presentation path follows p43's separation diagram but is not a
certified literal toy mechanism or approved gameplay animation.

## Finished appearance / attempt boundary

Built-in imagegen preserves the old finished MT image, uses the new bay/separation
renders as internal construction controls, and official p8/p29 as geometry-only
references. Exact input roles, prompts, files and hashes are retained.

1. First targeted nested sheet: rejected internally; bike became tracked.
2. Second edit targets only the bike in both views. Two separate chunky wheels
   are visible; this finished proposal is selected for review, not accepted.

No third targeted edit. Historical two free-form attempts, one native approach
and previous one controlled raster finish remain recorded. This is a local
correction within that native approach, not a third free-form reconstruction.
Earlier artwork, native files, prompts and director finding remain unchanged.

Selected image: `ArtSource/M85/Preproduction/MT101NestedAppearanceV1/mt101_nested_appearance_rev2.png`.
Left: assembled MT-101. Right: rear spacecraft with bike above its open bay.
Far-side chassis wheels remain occluded: raster does not prove all six contacts,
stowage/extraction or literal source fidelity. Latest separate review supersedes
the incomplete image in the 66-asset/67-view gallery without recording approval.

## Files changed / regression coverage

- Versioned native scene, three internal renders, audit and local Blender script.
- Two raster attempts, two official-reference crops, prompts and edit manifest.
- Separate appearance review, regenerated gallery, validation, eight new review
  guards (all previous 78 retained; 86 total), and project-state notes.

Actual native bike fits the estimated 3.2-comparison-stud inner bay. 33 sampled
bike positions and 25 sampled carrier positions have zero triangle-surface
intersections against retained obstacles; intentional support floor exempt.
This is estimated envelope/sampled unmodified-mesh evidence, not continuous
collision proof, exact official measurements, evaluated bevel physics or
certified real-LEGO buildability. Native guards reject wrong parent, oversized
bike and lost upward axis. New review guards reject inferred acceptance, hidden
retries, selected tracked attempt, raster-as-fit proof, native-as-final-art,
hidden gameplay gate, flattened parents and claimed exact dimensions.

## Automated verification

- Background Blender generator: PASS, 596 retained meshes and three negatives.
  Initial sandbox launch failed with exit 139; approved background execution of
  the same generator passed. The failed attempt is not reported as PASS.
- Saved native reopened with final script: PASS, 33/25 samples, zero intersections;
  existing chassis contacts, gun axes, cabins and unrelated MX control pass.
  Commands actually executed:
  `/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/correct_m85_mt101_nested_assembly.py -- ArtSource/M85/Preproduction/MT101NestedAssemblyV1`
  and `/Applications/Blender.app/Contents/MacOS/Blender --background --python-exit-code 1 --python tools/blender/correct_m85_mt101_nested_assembly.py -- --audit ArtSource/M85/Preproduction/MT101NestedAssemblyV1/mt101_nested_assembly.blend`.
- `python3 tools/Validation/test_m85_super_scout_review_guards.py`: PASS, 86/86.
- `python3 tools/Validation/validate_m85_super_scout.py`: PASS; T082 HOLD.
- `python3 tools/generate-m85-current-comparison.py --check`: PASS, 66/67.
- `python3 tools/generate-m85-super-scout-packets.py --check`: PASS, 66 packets/19 matrices.
- `git diff --check`: PASS.
- `./tools/verify.sh --full`: PASS, all 29 blocking stages, 317/317 NUnit,
  86/86 review guards, Golden100, replay/snapshot and byte-identical GLB checks.
  Summary: `Artifacts/Verification/20260913T230854Z-full-summary.txt`.
  The retained Stress60 diagnostic remains FAIL (2/60 completion),
  `BLOCKING_LATER — M9`, not a new blocker or authorized movement rewrite.

## Build / manual playtest requested

No model integration. Full harness exported and smoke-tested the existing
`Builds/macOS/LEGO Space RTS.app`, not this MT-101 concept.
Only finished appearance judgement is requested; no blockout
approval, resource gathering or engine setup is delegated to the director.

## Risks / unresolved issues / canon impact

Appearance remains unreviewed; final 24/44/72-cell blind review, gameplay-camera
readability and production acceptance remain HOLD. Native and raster are distinct
evidence. Independent rear-spacecraft/bike commands, costs, roles and ownership
remain a separate director decision: current canon specifies ground MT-101 only.
No runtime, roster, balance, architecture or Canon changes. NONE.

## Branch / worktree

`codex/m85-t082` in shared project checkout; no merge or push.
