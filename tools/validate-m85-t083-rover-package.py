#!/usr/bin/env python3
"""Validate the hand-authored Astronaut Rover T083 review package."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DESIGN = ROOT / 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover.md'
REVIEW = ROOT / 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover_review.html'
ART = ROOT / 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover_schematic.svg'
SPEC = ROOT / 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover_design_spec.md'
SOURCE_MANIFEST = ROOT / 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover_source_evidence.json'
REGISTRY = ROOT / 'Docs/Development/M85UnitDesign/registry.json'
PACKET = ROOT / 'Docs/Development/M85SuperScout/Packets/unit_astronauts_rover.md'
CONTRACTS = ROOT / 'Content/Presentation/SuperScout/astronauts_production_contracts.json'
EVIDENCE = ROOT / 'Content/Presentation/SuperScout/astronauts_source_evidence.json'


def check(condition, message):
    if not condition:
        raise SystemExit(f'FAIL: {message}')


design = DESIGN.read_text()
review = REVIEW.read_text()
art = ART.read_text()
spec = SPEC.read_text()
source_manifest = json.loads(SOURCE_MANIFEST.read_text())
registry = json.loads(REGISTRY.read_text())
contracts = json.loads(CONTRACTS.read_text())
evidence = json.loads(EVIDENCE.read_text())
entry = next(a for a in registry['assets'] if a['stableId'] == 'unit.astronauts.rover')
contract = next(a for a in contracts['assets'] if a['stableId'] == 'unit.astronauts.rover')
source = next(s for s in evidence['sources'] if s['setId'] == '7301')

authored_count = sum(asset.get('status') != 'QUEUED_NOT_AUTHORED' for asset in registry['assets'])
accepted_count = sum(bool(asset.get('directorAccepted')) for asset in registry['assets'])
check(registry['target'] == 35 and registry['authoredProposals'] == authored_count and registry['directorAccepted'] == accepted_count, 'registry roster/proposal/acceptance counters match asset records')
check(entry.get('file') == 'Batch02Astronauts/rover.md', 'registry package path')
check(entry.get('review') == 'Batch02Astronauts/rover_review.html', 'registry review path')
check(entry['status'] == 'DESIGN_ACCEPTED_T084_AUTHORIZED' and entry['directorAccepted'] and entry['productionAuthorized'], 'Rover director acceptance and T084 authorization')
check(entry.get('acceptanceGate') == 'PRODUCTION_DESIGN_TARGET_GATE' and entry.get('acceptanceDate') == '2026-09-24', 'Rover acceptance record')
target = entry.get('productionDesignTarget', {})
check(target.get('state') == 'DIRECTOR_ACCEPTED', 'Rover target must be director-accepted')
check(target.get('mode') == 'SOURCE_LOCKED', 'Rover must use source-locked target mode')
check(target.get('reviewReady') is True and target.get('appearanceSummary'), 'Rover target lacks review-ready appearance summary')
check(target.get('artifact') == 'Docs/Development/M85UnitDesign/Batch02Astronauts/rover_review.html', 'Rover target artifact identity')
check(entry.get('sourceEvidence', {}).get('manifest') == 'Batch02Astronauts/rover_source_evidence.json', 'Rover source evidence manifest identity')
check(entry.get('designSpec') == 'Batch02Astronauts/rover_design_spec.md', 'Rover Design Spec identity')
check(entry.get('gameAdaptations') == ['Restrained temporary survey pulse at the source front boom tip; presentation effect only.'], 'Rover visible adaptation list')
check(contract['contractState'] == 'SOURCE_VERIFIED', 'T082 contract identity')
check(source['instructionPdfs'][0]['url'].endswith('/4156314.pdf'), 'official 7301 source URL')
check(source['instructionPdfs'][0]['sha256'] in design, 'official source hash recorded')
normalized_design = ' '.join(design.split()).lower()
for term in ('production design target', 'source evidence and authority', 'design spec and actual game adaptation', 'source limits and later gates', 'director decision', 't084: authorized, not started'):
    check(term in normalized_design, f'missing package section: {term}')
for token in ('white spherical / half-dome', 'open-top/open-seat', 'black front bar/boom', 'short black mast with dish', 'no large blue equipment box'):
    check(token.lower() in normalized_design, f'missing source/readability requirement: {token}')
check('NON-AUTHORITATIVE FOR APPEARANCE' in art and 'not source evidence' in art.lower(), 'schematic must be visibly non-authoritative for appearance')
check('NON-AUTHORITATIVE FOR APPEARANCE' in review and 'AUTHORITATIVE PRODUCTION DESIGN TARGET' in review and 'page 1' in review, 'review must distinguish source-locked target from schematic')
check('NON-AUTHORITATIVE FOR APPEARANCE' in review and 'Schematic' in review, 'review must label explanatory schematic')
for correction in ('white spherical / half-dome', 'short black mast with dish', 'no large blue equipment box'):
    check(correction in normalized_design, f'director source correction missing from package: {correction}')
check('no large blue equipment box' in review.lower(), 'review board must reflect the corrected source read')
normalized_spec = ' '.join(spec.split()).lower()
normalized_review = ' '.join(review.split()).lower()
for content, label in ((normalized_spec, 'Design Spec'), (normalized_review, 'review')):
    check('short vertical black antenna/mast' in content, f'{label} must use the source-supported antenna/mast description')
    check('does not interpret it as a dish' in content, f'{label} must not infer antenna/mast terminal geometry')
check('4156314.pdf' in source_manifest['sourceArtifact']['url'] and source_manifest['sourceArtifact']['sha256'] in design, 'official source provenance manifest')
check('page 1' in spec and 'boom tip' in spec, 'Design Spec must define the source target and bounded game adaptation')
check('NO visible geometry' in review or 'No visible body' in review, 'review must state actual visible geometry delta')
for href in re.findall(r'(?:href|src)="([^"]+)"', review):
    if href.startswith(('http://', 'https://', '#')):
        continue
    check((REVIEW.parent / href).resolve().is_file(), f'broken review link: {href}')
check(PACKET.is_file(), 'T082 packet exists')
check(SPEC.is_file() and SOURCE_MANIFEST.is_file(), 'Rover Design Spec and source manifest exist')
print('PASS: Rover package identity, T082 source provenance, director acceptance, source-faithful antenna wording and local review links')


def validate_acceptance_gate():
    # Preserve the already accepted Expedition Crew decision as a pre-gate
    # acceptance. All future accepted records must satisfy the visual gate.
    for asset in registry['assets']:
        if not asset.get('directorAccepted'):
            continue
        if asset['stableId'] == 'unit.astronauts.expedition_crew':
            check(asset.get('acceptanceGate') == 'LEGACY_ACCEPTED_PRE_GATE', 'Expedition Crew grandfathering marker')
            continue
        required = ('sourceEvidence', 'designSpec', 'productionDesignTarget')
        for field in required:
            check(asset.get(field), f"accepted unit lacks {field}: {asset['stableId']}")
        source_ref = asset['sourceEvidence']
        source_manifest = source_ref.get('manifest') if isinstance(source_ref, dict) else None
        manifest_path = (ROOT / source_manifest) if source_manifest else None
        if manifest_path is None or not manifest_path.is_file():
            manifest_path = (ROOT / 'Docs/Development/M85UnitDesign' / source_manifest) if source_manifest else None
        check(manifest_path is not None and manifest_path.is_file(), f"accepted unit source manifest missing: {asset['stableId']}")
        spec = asset['designSpec']
        check((ROOT / 'Docs/Development/M85UnitDesign' / spec).is_file(), f"accepted unit Design Spec missing: {asset['stableId']}")
        production_target = asset['productionDesignTarget']
        check(production_target.get('mode') in ('SOURCE_LOCKED', 'ADAPTED', 'ORIGINAL_EXTENDED', 'MULTI_VIEW_BLOCKOUT'), f"invalid target mode: {asset['stableId']}")
        check(production_target.get('reviewReady') is True and production_target.get('appearanceSummary'), f"accepted unit target lacks review-ready appearance information: {asset['stableId']}")
        artifact = production_target.get('artifact')
        check(artifact and (ROOT / artifact).is_file(), f"missing production target artifact: {asset['stableId']}")
        check('schematic' not in artifact.lower(), f"schematic cannot satisfy appearance acceptance: {asset['stableId']}")


validate_acceptance_gate()
