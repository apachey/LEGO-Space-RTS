# Recon-Mech RP — T082 Super Scout packet

**Stable ID:** `unit.martians.recon_mech_rp`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

## A. Identity and authority

- Faction: `Martians`
- Kind: `Unit`
- Gameplay role: Detection / anti-air walker
- Authoritative footprint: `Medium`
- Source classification: `OFFICIAL-ADAPTED`
- Approved source sets/motifs: 7314
- Current confidence: verified canonical identity; construction confidence remains bounded by the source verification shown below.

Authoritative references:

- `Docs/Canon/00_CANON_SET_REGISTRY.md`
- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`
- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`
- `Content/PrototypeEntities.json`

Open question: Source-view coverage and construction-critical page ranges are recorded for the audited sources below. Asset-specific adaptation boundaries still must be resolved before this packet can leave HOLD.

## B. Reference board

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
| 7314 — Recon-Mech RP | [LEGO instructions](https://www.lego.com/en-us/service/building-instructions/7314)<br>[official PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130809.pdf) | [inventory](https://www.bricklink.com/catalogItemInv.asp?S=7314-1) | PRIMARY_VERIFIED | tall recon walker and sensor grammar |

### Source audit [Martians:7314]

- Evidence state: `OFFICIAL_PDF_VISUALLY_AUDITED`
- Evidence links: [official instruction PDF 1](https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4130809.pdf)
- Construction map:
  - Evidence pages 2-16: Broad red/gray upper craft with open central cockpit, long asymmetric drill/lance and claw arms, wrist hoses and rear equipment.
  - Evidence pages 17-25: Separate two-leg lower chassis with broad feet; upper craft docks above it to form the Recon-Mech.
  - Evidence pages 26-28: Tall rear pressure tank attaches behind the cockpit and between the upper modules.
  - Evidence pages 29-33: Final photography and explicit hand-separated flight conversion: lower body detaches, rotates and reconnects behind the upper craft.
- View/mechanism coverage: front=VERIFIED p25-33; rear=VERIFIED p25-33; leftRight=VERIFIED p2-33; top=VERIFIED p2-33; threeQuarter=VERIFIED p1 and p25-33; undersideInterior=VERIFIED p2-28 staged modules; mechanism=VERIFIED p30-33 mech-to-flight reconfiguration and articulated arms; gait, detection and anti-air cycle remain missing
- Verified findings:
  - Recon-Mech is a tall biped carrying a broad aircraft-like upper body, not a narrow sensor tower with weaponry as a minor detail.
  - Its two arms are strongly asymmetric: one ends in a long drill/lance and the other in a large black claw, with visible hoses feeding both sides.
  - The tall rear pressure tank and detachable lower body remain recognizable when the legs are reattached behind the cockpit for the flight configuration.
- Remaining evidence gaps:
  - The source proves modular flight conversion but not a continuous transform, walking gait, scanner grammar or anti-air tracking path; those canonical functions need a source-respecting production contract.

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** A source set is a container of models, figures, equipment and detachable modules, not an automatic one-to-one unit mapping. Any composed design must disclose its exact donor components, adaptation and rejected alternatives before director approval.

## C. Recognition contract

**Silhouette thesis:** A tall biped carrying a broad aircraft-like cockpit body, asymmetric drill-and-claw arms and a rear pressure tank.

Non-removable identity anchors:

- broad detachable upper craft
- asymmetric long drill and claw arms
- paired legs and tall rear pressure tank

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.
- Forbidden genericization: Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.
- Nearest-confusion baseline:

- `unit.martians.red_planet_protector` — Both are modular Martian bipeds built around detachable upper craft. Mitigations: Recon-Mech carries a wide asymmetric drill-and-claw span; Protector carries two matched long emitter arms. / Recon-Mech exposes a tall rear pressure tank; Protector preserves a broad wedge nose and separate torso module. / Recon-Mech relocates its leg block behind the cockpit for flight; Protector distributes both leg and torso modules around its low craft state.

## D. Construction contract

- Hero geometry must preserve every recognition anchor above.
- Support geometry must explain how hero masses connect, carry load and articulate.
- Micro geometry may enrich close view but may not become required for recognition.
- Exact chassis/load path, repeated modules, mounting logic, scale ratios and approved adaptations: `HOLD — SOURCE DECOMPOSITION REQUIRED`.

## E. Material and texture contract

- Silhouette, openings, major panel breaks, moving joints and LEGO connection logic remain geometry.
- Surface channels may carry controlled color masks, roughness, emission, decals and non-structural relief only.
- Required reusable and bespoke texture sets, resolution, tiling, texel density, LOD fallback and import settings: `HOLD — TEXTURE-NEEDS AUDIT REQUIRED`.
- Baked lighting, fake silhouette structure and illegible micro-noise are prohibited.

## F. State and animation contract

- Applicable idle, locomotion/operation, work, attack, production, repair, transform/deploy, disabled, damage and destruction beats: `HOLD — MECHANISM EVIDENCE REQUIRED`.
- Every moving assembly must receive a named pivot, parent, axis/path, rest/extreme poses and authoritative presentation driver.
- Animation may communicate gameplay state but never decide gameplay timing.

## G. Presentation hookups

- `Socket_Selection` and `Socket_Health` are mandatory.
- Tool, weapon, projectile, VFX, lamp and audio sockets follow only from verified function.
- Cargo, passenger, service, production-exit or network sockets apply where the canonical role requires them.
- Identification Tile, icon silhouette, portrait camera and reduced-presentation fallback: `HOLD — PRESENTATION AUDIT REQUIRED`.

## H. Insight and decision ledger

- Verified fact: stable identity, faction, role, footprint, source classification and mapped source family.
- Canon-derived interpretation: silhouette thesis and identity anchors above.
- Unknown: exact multi-angle construction, articulation, material ratios, texture inventory and confusion mitigation until the remaining audits are complete.
- Consequential contradictions: none recorded at identity-baseline stage.

## I. Build handoff

1. Verify and cite the complete multi-angle source board.
2. Decompose primary masses and negative spaces from orthogonal evidence.
3. Resolve LEGO load path, connection grammar and moving mechanism.
4. Complete material/texture and state/animation contracts.
5. Produce 24/44/72-cell black silhouettes and run the cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
