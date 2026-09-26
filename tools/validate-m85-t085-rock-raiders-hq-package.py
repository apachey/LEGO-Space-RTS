#!/usr/bin/env python3
"""Validate the T085 Rock Raiders HQ design package and the T085 registry.

Checks roster identity/order and counters, the HQ proposal state (not
accepted, no T086 authorization), source-manifest provenance against the
accepted T082 evidence, composition render hashes, Design Spec sections,
review-page labels and local links, and that accepted T082/T083 records are
unchanged in state.  Standard library only; no network or browser.
"""
import importlib.util
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True

ROOT = Path(__file__).resolve().parents[1]
PKG_DIR = ROOT / 'Docs/Development/M85InfrastructureDesign'
REGISTRY = PKG_DIR / 'registry.json'
SPEC = PKG_DIR / 'Batch01RockRaiders/rock_raiders_hq_design_spec_20260926_v1.md'
REVIEW = PKG_DIR / 'Batch01RockRaiders/rock_raiders_hq_director_review_20260926_v1.html'
ART = ROOT / 'ArtSource/M85/T085/RockRaidersHQV1'
SOURCE_MANIFEST = ART / 'source_evidence_manifest.json'
RENDER_MANIFEST = ART / 'composition_render_manifest.json'
BASELINE = ROOT / 'Content/Presentation/SuperScout/roster_identity_baseline.json'
T082_EVIDENCE = ROOT / 'Content/Presentation/SuperScout/rock_raiders_source_evidence.json'
T082_PACKET = ROOT / 'Docs/Development/M85SuperScout/Packets/building_rock_raiders_hq.md'
T083_REGISTRY = ROOT / 'Docs/Development/M85UnitDesign/registry.json'
GENERATOR = ROOT / 'tools/generate-m85-t085-rock-raiders-hq-target.py'
HQ = 'building.rock_raiders.hq'


def check(condition, message):
    if not condition:
        raise SystemExit('FAIL: ' + message)


registry = json.loads(REGISTRY.read_text())
baseline = json.loads(BASELINE.read_text())
infra = [a['stableId'] for a in baseline['assets'] if a['kind'] != 'Unit']
assets = registry['assets']

# Roster identity, canonical order and counters.
check(registry['task'] == 'T085' and registry['target'] == 31 == len(infra), 'T085 target must be the 31 canonical infrastructure entries')
check([a['stableId'] for a in assets] == infra, 'registry order must match the canonical infrastructure roster')
check([a['rosterIndex'] for a in assets] == list(range(1, 32)), 'roster indices 1..31')
check(registry['authoredProposals'] == sum(a['status'] != 'QUEUED_NOT_AUTHORED' for a in assets), 'authored counter')
check(registry['directorAccepted'] == sum(bool(a['directorAccepted']) for a in assets), 'accepted counter')
check(registry['productionModels'] == 0, 'no T086 production model may exist')
for a in assets:
    check((ROOT / a['t082Packet']).is_file(), 'missing T082 packet for ' + a['stableId'])
    check(a['productionAuthorized'] is False, 'T086 must not be authorized: ' + a['stableId'])
    if a['stableId'] != HQ:
        check(a['status'] == 'QUEUED_NOT_AUTHORED' and a['directorAccepted'] is False, 'only the HQ is authored in this task: ' + a['stableId'])

# HQ proposal record.
hq = assets[0]
check(hq['stableId'] == HQ and hq['rosterIndex'] == 1, 'the HQ is the first canonical infrastructure entry')
check(hq['status'] == 'DESIGN_PROPOSED_AWAITING_DIRECTOR_REVIEW' and hq['directorAccepted'] is False, 'HQ must be proposed, not accepted')
target = hq['productionDesignTarget']
check(target['mode'] == 'ADAPTED' and target['state'] == 'PROPOSED' and target['reviewReady'] is True and target['appearanceSummary'], 'HQ target mode/state')
for key in ('artifact', 'designSpec', 'compositionRenders', 'heroImage'):
    check((ROOT / target[key]).is_file(), 'missing target file: ' + key)
check('schematic' not in target['artifact'].lower(), 'a schematic cannot be the target artifact')
check((PKG_DIR / hq['designSpec']).is_file() and (PKG_DIR / hq['review']).is_file(), 'HQ spec/review paths')
check(len(hq['gameAdaptations']) == 8 and len(hq['openDirectorDecisions']) == 3, 'HQ adaptations A1-A8 and three open decisions')
check(hq['sourceEvidence']['officialPixelCheck'] == 'NOT_PERFORMED_NETWORK_POLICY_BLOCKED', 'official pixel check limitation must stay explicit')

# Source provenance against accepted T082 evidence.
manifest = json.loads(SOURCE_MANIFEST.read_text())
t082 = next(s for s in json.loads(T082_EVIDENCE.read_text())['sources'] if s['setId'] == '4990')
pdf = next(s for s in manifest['sources'] if s['id'] == 'lego-instructions-pdf')
check(pdf['url'] == t082['instructionPdfs'][0]['url'] and pdf['sha256'] == t082['instructionPdfs'][0]['sha256'], 'official 4990 PDF URL/hash must match T082')
check(pdf['authority'] == 'PRIMARY_OFFICIAL' and 'NOT_RETRIEVED' in pdf['thisSession'], 'official PDF must be marked not retrieved in this session')
check(manifest['directorAccepted'] is False and manifest['productionAuthorized'] is False, 'source manifest acceptance state')
check('www.lego.com' in manifest['retrievalEnvironment']['deniedHosts'], 'blocked official host must be recorded')
for s in manifest['sources']:
    check(s['authority'] and (s.get('url') or s.get('urls')), 'source record lacks authority/url: ' + s['id'])
check(any('30346c01' in c for s in manifest['sources'] for c in s.get('claims', [])), 'light-assembly part fact recorded')
check(any('30271px3' in c for s in manifest['sources'] for c in s.get('claims', [])), 'raised baseplate part fact recorded')
check(manifest['findings']['correctionsToEarlierT082Draft'], 'T082 wording differences recorded')

# Composition renders: scene determinism and hashes (no browser).
spec_obj = importlib.util.spec_from_file_location('t085_hq_generator', GENERATOR)
generator = importlib.util.module_from_spec(spec_obj)
spec_obj.loader.exec_module(generator)
generator.check(generator.scene_document())
renders = json.loads(RENDER_MANIFEST.read_text())
check(renders['kind'] == 'PROJECT_AUTHORED_COMPOSITION_RENDERS' and 'Not official LEGO imagery' in renders['authority'], 'renders must be labelled project-authored')

# Design Spec sections and key statements.
spec = ' '.join(SPEC.read_text().split())
for token in ('## Production Design Target', 'SOURCE_LOCKED', '`ADAPTED`', '## Що підтверджує джерело', '## Уточнення до старого пакета T082',
              '## Масштаб і компонування', '## Видимі ігрові адаптації', '## Стани', '## Читабельність з ігрової камери',
              '## Матеріали й текстури', '## Півоти й сокети', '## LOD', '## Межі для наступних пакетів Rock Raiders',
              '## Невідоме й що потрібно для фіналізації', '## Рішення директора', '## Докази',
              '30346c01', '30271px3', 'не стріляє', 'Healthy 70–100 %', 'Damaged 35–69 %', 'Heavily Damaged 20–34 %', 'Critical < 20 %',
              'Не прийнято', 'T086:** НЕ РОЗПОЧАТО', 'eccfc7b81cc9799637011e45b143d87cac6a23ebe49d2fec4f0a42b322f66dae'):
    check(token in spec, 'Design Spec missing: ' + token)

# Review page labels and local links.
review = REVIEW.read_text()
for token in ('ЗАПРОПОНОВАНО · НЕ ПРИЙНЯТО', 'ADAPTED', 'T086 НЕ РОЗПОЧАТО', 'Проєктна ілюстрація композиції, не фото LEGO',
              'Що потрібно вирішити', 'Обмеження', 'Нічого не прийнято', '30346c01', '30271px3'):
    check(token in review, 'review page missing: ' + token)
check('T085 ПРИЙНЯТО' not in review, 'review page must not claim acceptance')
local = [h for h in re.findall(r'(?:href|src)="([^"]+)"', review) if not h.startswith(('http://', 'https://', '#'))]
check(local, 'review page must reference local renders')
for href in local:
    check((REVIEW.parent / href).resolve().is_file(), 'broken review link: ' + href)
check(target['heroImage'].split('/')[-1] in review, 'review hero must be the target hero render')

# Preserved accepted records.
packet = T082_PACKET.read_text()
check('T082_REFERENCE_FOUNDATION_ACCEPTED_DESIGN_PENDING' in packet, 'T082 HQ packet state must be unchanged')
t083 = json.loads(T083_REGISTRY.read_text())
check(t083['target'] == 35 and t083['directorAccepted'] == 35 == sum(bool(a['directorAccepted']) for a in t083['assets']), 'T083 acceptances must be preserved')

print('PASS: T085 registry (31 canonical entries, 1 proposed, 0 accepted), Rock Raiders HQ ADAPTED proposal, '
      'T082 source provenance, %d composition render hashes, Design Spec sections, review labels/links and preserved T082/T083 state'
      % len(renders['renders']))
