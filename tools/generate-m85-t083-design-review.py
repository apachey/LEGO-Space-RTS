#!/usr/bin/env python3
"""Build the T083 review documents without changing T082 or runtime assets."""
import argparse
import hashlib
import html
import json
import os
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Development/M85UnitDesign'
BATCH = OUT / 'Batch01RockRaiders'

def read(p):
    return json.loads((ROOT / p).read_text())

def rel(p, start=BATCH):
    return os.path.relpath(ROOT / p, start)

def esc(s):
    return html.escape(str(s), quote=True)

def article(html_text, key):
    match = re.search(rf'<article id="{re.escape(key)}">.*?</article>', html_text, re.S)
    if not match:
        raise ValueError(f'Missing review card: {key}')
    return match.group(0)

def replace_article(html_text, key, replacement):
    pattern = rf'<article id="{re.escape(key)}">.*?</article>'
    updated, count = re.subn(pattern, lambda _: replacement, html_text, count=1, flags=re.S)
    if count != 1:
        raise ValueError(f'Expected one review card: {key}; found {count}')
    return updated

def unit_output_paths(key):
    review_name = 'hover_scout_target_review.html' if key == 'hover_scout' else 'review.html'
    return (BATCH / f'{key}.md', BATCH / review_name, OUT / 'registry.json')

def hover_scout_target_review(record, target_image, target_caption, source_image):
    states = ''.join(f'<tr><th>{esc(name)}</th><td>{esc(description)}</td></tr>' for name, description in record['states'])
    views = ''.join(f'<li>{esc(view)}</li>' for view in record['views'])
    return f'''<!doctype html>
<html lang="uk"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>T083 · Hover Scout — Production Design Target</title>
<style>
:root{{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;color:#172c32;background:#eff1ed;font-size:17px;line-height:1.55}}*{{box-sizing:border-box}}body{{margin:0}}header,main{{max-width:1160px;margin:auto;padding:28px}}header{{padding-top:48px}}h1{{font-size:clamp(34px,5vw,58px);line-height:1.05;margin:12px 0}}h2{{font-size:26px;margin:0 0 12px}}h3{{font-size:18px;margin:18px 0 5px}}p{{margin:8px 0 14px}}a{{color:#006d70;text-underline-offset:3px}}.eyebrow{{font:13px monospace;letter-spacing:2px;color:#006f70}}.status,.decision,.unknown{{padding:16px 20px;border-radius:7px;margin:18px 0}}.status{{background:#fbf6e9;border-left:4px solid #c29240}}.decision{{background:#e4f1eb}}.unknown{{background:#f3f2ee}}.grid{{display:grid;grid-template-columns:1.15fr 1fr;gap:22px;align-items:start}}section{{background:white;border:1px solid #d6ded8;border-radius:12px;padding:22px;margin:0 0 22px}}figure{{margin:0}}figure img{{display:block;width:100%;height:auto;max-height:620px;object-fit:contain;background:#f5f5f1;border-radius:6px}}figcaption{{font-size:14px;color:#526367;margin-top:9px}}.source img{{max-height:600px}}table{{border-collapse:collapse;width:100%;font-size:15px}}td,th{{text-align:left;vertical-align:top;padding:10px;border-bottom:1px solid #d6ded8}}th{{width:27%}}ul{{padding-left:22px}}@media(max-width:780px){{header,main{{padding:18px}}.grid{{grid-template-columns:1fr}}section{{padding:17px}}}}@media print{{body{{background:white}}section{{break-inside:avoid}}}}
</style></head><body>
<header><div class="eyebrow">LEGO SPACE RTS / M8.5 / T083</div><h1>Hover Scout · 4910</h1>
<p><b>Production Design Target: SOURCE_LOCKED</b> · {('DIRECTOR ACCEPTED · 2026-09-24' if record.get('directorAccepted') else 'подано на рішення директора')} · T084 не розпочато.</p>
<div class="status"><b>Що має зібрати T084:</b> відкритий низький пластинчастий Scout з офіційного кроку 7: синьо-бірюзовий оператор у відкритому місці, велика кутова сканерно-інструментальна збірка спереду та окремий задній тримач. Зберегти силует і кольорові маси інструкції. Не додавати закритий корпус, сенсорну башту чи велику масу під днищем.</div></header>
<main><div class="grid"><section><h2>Цільовий вигляд</h2><figure><a href="{esc(target_image)}"><img src="{esc(target_image)}" alt="Офіційний Hover Scout LEGO 4910, завершений крок 7"></a><figcaption>{esc(target_caption)}</figcaption></figure><p><a href="{esc(source_image)}">Відкрити повну офіційну сторінку інструкції 4910</a></p></section>
<section class="source"><h2>Повне джерело</h2><figure><a href="{esc(source_image)}"><img src="{esc(source_image)}" alt="Повна сторінка 1 офіційної інструкції LEGO 4910"></a><figcaption>Офіційна сторінка 1. Кадр цільового вигляду вище — точне кадрування кроку 7 з цього незміненого растра.</figcaption></figure></section></div>
<section><h2>Дельти для гри</h2><p>{esc(record['adaptation'])}</p><p><b>Масштаб:</b> {esc(record['scale'])}</p><p><b>Мітка команди:</b> {esc(record['identificationTile'])}</p><p><b>Матеріали:</b> {esc(record['palette'])}</p></section>
<section><h2>Конструкція й ракурси</h2><p>{esc(record['construction'])}</p><ul>{views}</ul></section>
<section><h2>Стани</h2><table>{states}</table></section>
<section class="unknown"><h2>Межі джерела</h2><p>{esc(record['unknown'])}</p><p>Інструкція не показує строгий низ і протилежний бік. Кадрування цілі не домальоване й не змінює офіційні пікселі.</p></section>
<section class="decision"><h2>Рішення директора</h2><p>{esc(record['reviewDecision'])}</p><p><a href="hover_scout.md">Відкрити повну специфікацію T083</a></p></section></main></body></html>'''

def update_registry_entry(document, expected):
    matches = [a for a in document['assets'] if a['stableId'] == expected['stableId']]
    if len(matches) != 1:
        raise ValueError(f"Expected one registry identity: {expected['stableId']}")
    matches[0].clear()
    matches[0].update(expected)

def accepted_unit_ids(registry_document):
    """Registry acceptance owns a package; proposal inputs cannot revoke it."""
    return {
        asset['stableId']
        for asset in registry_document['assets']
        if asset.get('directorAccepted') is True
    }

def preserve_accepted_review_cards(generated_review, current_review, records, accepted_ids):
    """Keep accepted cards byte-for-byte while regenerating proposal cards."""
    for record in records:
        sid = record['stableId']
        if sid in accepted_ids:
            key = sid.rsplit('.', 1)[-1]
            generated_review = replace_article(
                generated_review, key, article(current_review, key)
            )
    return generated_review

def proposal_outputs_for_check(files, records, accepted_ids):
    """Remove frozen accepted package artifacts from proposal-output checks."""
    owned = dict(files)
    for record in records:
        sid = record['stableId']
        if sid in accepted_ids:
            key = sid.rsplit('.', 1)[-1]
            markdown, _, _ = unit_output_paths(key)
            owned.pop(markdown, None)
            if sid == 'unit.rock_raiders.hover_scout':
                owned.pop(BATCH / 'hover_scout_target_review.html', None)
    return owned

def run(check=False, unit=None):
    proposals = json.loads((BATCH / 'design_proposals.json').read_text())
    roster = read('Content/Presentation/SuperScout/roster_identity_baseline.json')['assets']
    units = [a for a in roster if a['kind'] == 'Unit']
    by_id = {a['stableId']: a for a in units}
    contracts = read('Content/Presentation/SuperScout/rock_raiders_production_contracts.json')
    by_contract = {a['stableId']: a for a in contracts['assets']}
    sources = json.loads((BATCH / 'References/source_manifest.json').read_text())['references']
    reference_selections = read('Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentComparisonV1/selection_manifest.json')['selections']
    selection = {a['stableId']: a for a in reference_selections}
    images = json.loads((BATCH / 'appearance_manifest.json').read_text())
    current_registry = read('Docs/Development/M85UnitDesign/registry.json')
    accepted_ids = accepted_unit_ids(current_registry)
    assert len(units) == 35 and len(proposals['records']) == 8
    assert {a['stableId'] for a in proposals['records']} == {a['stableId'] for a in units if a['faction'] == 'RockRaiders'}
    for item in sources + images['generated']:
        p = ROOT / item['file']
        assert p.is_file(), p
        assert hashlib.sha256(p.read_bytes()).hexdigest() == item['sha256'], p
    for picture in images['assets'].values():
        if 'targetArtifact' in picture:
            p = ROOT / picture['targetArtifact']
            assert p.is_file(), p
            assert hashlib.sha256(p.read_bytes()).hexdigest() == picture['targetSha256'], p
    for a in proposals['records']:
        if a['directorAccepted']:
            assert a['productionAuthorized'] and a.get('acceptanceDate') and a.get('acceptanceNote'), a['stableId']
        else:
            assert not a['productionAuthorized'], a['stableId']
        assert a['sourceSet'] in by_id[a['stableId']]['sourceSets']
        for field in ('appearance','construction','adaptation','scale','identificationTile','palette','unknown','reviewDecision'):
            assert a[field].strip(), (a['stableId'], field)
        assert len(a['states']) >= 4 and len(a['views']) == 4
    files = {}
    cards = []
    registry = []
    for n, a in enumerate(proposals['records'], 1):
        sid = a['stableId']; key = sid.rsplit('.', 1)[-1]; r = by_id[sid]; c = by_contract[sid]
        picture = images['assets'][sid]
        primary = picture['primary']
        candidate_art = [v['file'] for v in picture.get('additional', []) if v['file'].startswith('ArtSource/')]
        target_artifact = picture.get('targetArtifact', primary if primary.startswith('ArtSource/') else candidate_art[-1] if candidate_art else primary)
        target_mode = picture.get('targetMode', 'ADAPTED' if target_artifact.startswith('ArtSource/') else 'SOURCE_LOCKED')
        refs = [s for s in sources if s['setId'] == a['sourceSet']]
        ref_lines = ['- [' + Path(s['file']).name + '](' + rel(s['file']) + ') — [' + s['kind'] + '](' + s['sourceUrl'] + ')' for s in refs]
        source_packet = 'Docs/Development/M85SuperScout/Packets/' + sid.replace('.', '_') + '.md'
        source_text = (ROOT / source_packet).read_text()
        source_section = source_text.split('## B. Reference board\n',1)[1].split('\n## C.',1)[0]
        if sid == 'unit.rock_raiders.crew':
            # T083 uses the director-corrected official character spelling;
            # preserve the accepted T082 source packet unchanged.
            source_section = source_section.replace('Axle', 'Axel')
        # Links inside the inherited evidence section are web links; no historical status is copied.
        txt = f'# {n:02d} · {a["title"]} — T083 design proposal\n\n'
        txt += f'**{sid} · {r["footprint"]} · {r["sourceClassification"]}**\n\n'
        if a['directorAccepted']:
            txt += f'**Статус:** дизайн прийнято директором {a["acceptanceDate"]}; T084 авторизовано для цього юніта, але не розпочато. Це не модель. T082 прийнято як основу.\n\n'
        else:
            txt += '**Статус:** завершений дизайн-кандидат для режисерського огляду; не прийнятий дизайн і не модель. T082 прийнято як основу.\n\n'
        target_caption = picture['targetCaption'] if 'targetCaption' in picture else picture['caption'] if target_artifact == primary else next(v['caption'] for v in picture['additional'] if v['file'] == target_artifact)
        txt += f'## Production Design Target · {target_mode}\n\n{a["appearance"]}\n\n![{a["title"]}]({rel(target_artifact)})\n\n{target_caption}\n\n'
        txt += '## Source Evidence\n\nOfficial LEGO/source imagery below defines the original object; generated proposals are not source evidence.\n\n'
        source_artifact = primary if not primary.startswith('ArtSource/') else next((v['file'] for v in picture.get('additional', []) if not v['file'].startswith('ArtSource/')), None)
        if source_artifact:
            txt += f'![Official source evidence]({rel(source_artifact)})\n\n'
        for extra in picture.get('additional', []):
            txt += f'![{extra["caption"]}]({rel(extra["file"])})\n\n{extra["caption"]}\n\n'
        for title,field in [('Пропорції та масштаб','scale'),('Конструкція','construction'),('Запропонована адаптація','adaptation'),('Командна позначка','identificationTile'),('Кольори й матеріали','palette')]:
            txt += f'## {title}\n\n{a[field]}\n\n'
        txt += '## Ракурси\n\n' + '\n'.join('- '+v for v in a['views']) + '\n\n'
        txt += 'Це словесні вимоги до ракурсів завершеного дизайну, не заявка на наявність нових ортографічних зображень. Підтверджене покриття й прогалини джерел перелічені нижче.\n\n'
        txt += '## Стани\n\n| Стан | Видимий результат |\n|---|---|\n' + '\n'.join('| '+s+' | '+v+' |' for s,v in a['states']) + '\n\n'
        txt += 'Усі рухи лише відображають стан симуляції. Немає нових команд, потужності, місткості чи таймінгів. Виробництво, brownout та окремий construction-progress стан не додаються юніту за аналогією з будівлею; поява/ремонт використовують чинні події.\n\n'
        txt += '## Геометрія, текстури й віддалення\n\n'
        geometry = c['materials']['geometryMustCarry']
        if key == 'drill_craft':
            geometry = ['two elongated saw bars with a fixed spine and separate moving tooth/chain treatment', 'flat operator sled and hazard panel', 'paired raised round side pods; not cutting discs']
        if key == 'crew':
            geometry = ['five source-specific headwear profiles', 'minifigure hands, feet and body proportions', 'portable tool outline; no mandatory invented backpack']
        txt += '\n'.join('- Геометрія: '+v+'.' for v in geometry) + '\n\n'
        txt += 'Close: зберігає джерельні головні деталі, кріплення й оператора. Combat: ті самі великі маси й контакти, менше дрібних швів/зубців. Strategic: зберігає форму основи, інструмента та стану; без мікродеталей. Числовий бюджет призначається після прийняття дизайну; перевірка реальною камерою й LOD — T084.\n\n'
        for fam in contracts['sharedMaterialPlan']['reusableTextureFamilies']:
            if fam['id'] in c['materials']['textureFamilies']:
                txt += f'- `{fam["id"]}`: {fam["resolution"]}; {fam["channels"]} {fam["tiling"]} {fam["lodFallback"]}\n'
        if key == 'crew':
            txt += '- `rr_crew_identity_masks` (T083 proposal): 1024×1024 RGBA atlas, sRGB flat color and coverage, non-tiling; five manually authored source-grounded face/torso regions; no invented rear print. Close retains faces/torso shapes, Combat simplifies, Strategic uses outfit blocks. No photographed lighting or generated lettering.\n'
        txt += '\nУсі текстури тут SPECIFIED_NOT_AUTHORED. Схвалені M7 матеріали залишаються основою; фотографії джерел і generated-концепти не є ігровими текстурами. Нові маски/декалі — майбутня робота проєкту з окремим оглядом. Не переносити текстурою отвори, опорні вузли або тіні.\n\n'
        txt += '## Механіка, точки прив’язки, LOD\n\n'
        txt += f'[Іменовані опори та точки T082]({rel(source_packet)}#f-state-and-animation-contract) — початкова схема, з такими уточненнями T083:\n\n'
        refinements = {
            'crew':'Pivot_Tool follows hand contact. Pivot_Lamp is optional only where the source outfit actually carries one. Health stays above headwear; Selection stays at the feet.',
            'hover_scout':'Keep Pivot_ScannerYaw and Pivot_ToolPitch at rest; no unsupported moving sensor tower. Socket_SurveyPulse/ScannerVfx sit at the existing forward scanner/tool assembly. Selection stays at ground projection; Health above the operator.',
            'drill_craft':'Pivot_SawFeedLeft/Right lower the rigid bars. Pivot_SawSpinLeft/Right names may remain as reserved driver bindings but drive a tooth-chain treatment, not rigid-bar rotation. Socket_SawContactLeft/Right at the exposed forward cutting zones. Side pods stay fixed.',
            'rapid_rider':'External propulsion cylinder housings stay fixed; existing Pivot_PropulsionLeft/Right must not rotate those entire shells. Passenger entry/exit at rear rim; four local occupancy anchors in the tub, no additional authoritative seats.',
            'loader_dozer':'Pivot_BucketLift carries the original arm pair in both loadouts. Pivot_BucketTilt carries bucket/yoke pitch. Pivot_CutterSpin at the transverse disc axle. BucketContact/CutterContact follow the respective visible cutting edge, never wheel centre.',
            'granite_grinder':'LegLeft/Right preserve rigid ski-foot contacts; DrillSpin sits on the drill axis, DrillFeed supplies only short source-bounded feed. FootLeft/Right below ski soles. Health above cage, never attached to the moving drill tip.',
            'chrome_crusher':'Four wheel pivots at their own axles. DrillSpin follows the source off-centre drill, not the vehicle midline. ToolBeam keeps its visible mount. Cargo socket stays inside the open service deck; no new ranged-weapon muzzle.',
            'tunnel_transport':'RotorLeft/Right keep source axes and fixed pitch; CargoHoist carries the suspended yoke and CargoSway has a short damped arc. CargoAttach under centre beam; LoadApproach below the open frame. Health above rotor skyline, Selection at ground projection.'}
        txt += refinements[key] + '\n\n'
        txt += 'Existing socket inventory: ' + ', '.join('`'+s+'`' for s in c['sockets']) + '. No runtime binding is changed by this document. Exact clearance and animation arcs remain T084/T087 verification, not a request for a pre-model camera gate.\n\n'
        txt += '## Підтверджене, адаптоване, невідоме\n\n' + source_section + '\n\n'
        txt += '\n'.join(ref_lines) + '\n\n'
        if a['sourceRefinement']: txt += '**Уточнення джерела в T083:** '+a['sourceRefinement']+'\n\n'
        txt += '**Невідоме / межа доказу:** '+a['unknown']+'\n\n'
        txt += 'The T082 source packet remains immutable research history; current proposal decisions above supersede only its explicitly identified design/motion suggestions. Source facts not reviewed here retain their stated confidence. No generated image proves hidden attachment geometry.\n\n'
        decision = a['acceptanceNote'] if a['directorAccepted'] else a['reviewDecision']
        txt += '## Рішення режисера\n\n'+decision+'\n\n'
        txt += '**BLOCKING_NOW:** прийняття цього дизайн-кандидата перед його T084. **BLOCKING_LATER:** реальна геометрія, зазори, матеріали, камера/LOD і прив’язка анімації. **DIAGNOSTIC:** порівняння з сусідніми юнітами; не повторювати прийнятий T082 blind review без конкретного дефекту.\n'
        files[BATCH/(key+'.md')] = txt
        # Display-only crop of full original page; full page link remains available.
        crop = picture.get('crop')
        if crop and target_artifact == primary:
            x,y,w,h,iw,ih=crop
            hero=f'<svg class="hero" role="img" aria-label="{esc(a["title"])} completed source" viewBox="{x} {y} {w} {h}"><image href="{esc(rel(primary))}" width="{iw}" height="{ih}"/></svg>'
        else:
            hero=f'<img class="hero" src="{esc(rel(target_artifact))}" alt="{esc(a["title"])} Production Design Target" loading="lazy">'
        extras=''.join(f'<figure><img src="{esc(rel(v["file"]))}" alt="{esc(v["caption"])}" loading="lazy"><figcaption>{esc(v["caption"])}</figcaption></figure>' for v in picture.get('additional',[]))
        state_html=''.join(f'<tr><th>{esc(s)}</th><td>{esc(v)}</td></tr>' for s,v in a['states'])
        target_extra = ''.join(f'<figure><img src="{esc(rel(v["file"]))}" alt="{esc(v["caption"])}" loading="lazy"><figcaption>{esc(v["caption"])}</figcaption></figure>' for v in picture.get('additional', []) if v['file'] != target_artifact)
        source_link = f'<p><a href="{esc(rel(source_artifact))}">Open Source Evidence</a></p>' if source_artifact else ''
        label = f'DIRECTOR ACCEPTED · {a["acceptanceDate"]}' if a['directorAccepted'] else 'Кандидат, не прийнято'
        cards.append(f'''<article id="{key}"><div class="assethead"><span class="num">{n:02}</span><div><h2>{esc(a['title'])}</h2><p>LEGO {a['sourceSet']} · {r['footprint']} · {label}</p></div></div><div class="columns"><div><h3>Production Design Target · {target_mode}</h3><a href="{esc(rel(target_artifact))}">{hero}</a><p class="caption">{esc(target_caption)}</p><h3>Source Evidence</h3>{source_link}{target_extra}<details><summary>T082 comparison</summary><img src="{esc(rel(selection[sid]['file']))}" alt="T082 comparison"><p>Comparative reference only; status preserved: {esc(selection[sid]['status'])}.</p></details></div><div><h3>Design Spec · appearance</h3><p>{esc(a['appearance'])}</p><h3 class="adapt">Allowed adaptation</h3><p>{esc(a['adaptation'])}</p><h3>Scale</h3><p>{esc(a['scale'])}</p><h3>Materials and team marker</h3><p>{esc(a['palette'])}</p><p>{esc(a['identificationTile'])}</p></div></div><details><summary>Construction, views and states</summary><p>{esc(a['construction'])}</p><ul>{''.join('<li>'+esc(v)+'</li>' for v in a['views'])}</ul><table>{state_html}</table></details><div class="unknown"><b>Unknowns / later checks</b><p>{esc(a['unknown'])}</p></div><div class="decision"><b>Director review</b><p>{esc(a['reviewDecision'])}</p></div><p><a href="{key}.md">Full Design Spec: sources, materials and mechanics →</a></p></article>''')
        entry = {'stableId':sid,'file':'Batch01RockRaiders/'+key+'.md','status':a.get('status','DESIGN_PROPOSAL_FOR_DIRECTOR_REVIEW'),'directorAccepted':a['directorAccepted'],'productionAuthorized':a['productionAuthorized'],'sourceEvidence':{'manifest':'Batch01RockRaiders/References/source_manifest.json','sourceSet':a['sourceSet']},'designSpec':'Batch01RockRaiders/'+key+'.md','productionDesignTarget':{'mode':target_mode,'state':'DIRECTOR_ACCEPTED' if a['directorAccepted'] else 'TARGET_FOR_DIRECTOR_REVIEW','artifact':target_artifact,'rationale':'Official source visual retained closely.' if target_mode=='SOURCE_LOCKED' else 'Explicit game adaptation visual.'}}
        if a['directorAccepted']:
            entry.update({'acceptanceGate':'PRODUCTION_DESIGN_TARGET_GATE','acceptanceDate':a['acceptanceDate'],'acceptanceNote':a['acceptanceNote']})
        registry.append(entry)
    existing_registry = read('Docs/Development/M85UnitDesign/registry.json')
    rr_ids = {a['stableId'] for a in registry}
    preserved = [a for a in existing_registry['assets'] if a['stableId'] not in rr_ids and a.get('status') != 'QUEUED_NOT_AUTHORED']
    registry.extend(preserved)
    existing_by_id = {a['stableId']: a for a in existing_registry['assets']}
    registry = [existing_by_id[a['stableId']] if existing_by_id.get(a['stableId'], {}).get('directorAccepted') else a for a in registry]
    for u in units:
        if u['stableId'] not in {a['stableId'] for a in registry}:
            registry.append({'stableId':u['stableId'],'status':'QUEUED_NOT_AUTHORED','directorAccepted':False,'productionAuthorized':False})
    files[OUT/'registry.json']=json.dumps({'task':'T083','target':35,'authoredProposals':len(rr_ids)+len(preserved),'directorAccepted':sum(a.get('directorAccepted',False) for a in registry),'productionModels':sum(a.get('productionModel',False) for a in registry),'assets':registry},ensure_ascii=False,indent=2)+'\n'
    nav=''.join(f'<a href="#{a["stableId"].rsplit(".",1)[-1]}">{n:02} {esc(a["title"])}</a>' for n,a in enumerate(proposals['records'],1))
    files[BATCH/'review.html']='''<!doctype html><html lang="uk"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>T083 · Rock Raiders design review</title><style>
:root{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;color:#172c32;background:#eff1ed;font-size:17px;line-height:1.55}*{box-sizing:border-box}body{margin:0}header,main{max-width:1260px;margin:auto;padding:32px}header{padding-top:60px}h1{font-size:clamp(32px,5vw,62px);line-height:1.06;max-width:950px;margin:14px 0 24px}h2{font-size:29px;margin:0}h3{font-size:18px;margin:24px 0 6px}p{margin:8px 0 18px}a{color:#006d70;text-underline-offset:3px}nav{display:flex;flex-wrap:wrap;gap:8px;margin:24px 0}nav a{background:white;border:1px solid #cad5ce;padding:7px 12px;border-radius:6px;text-decoration:none;font-size:14px}.eyebrow,.num{font-family:monospace;letter-spacing:2px;color:#006f70}.stats{display:flex;gap:25px;flex-wrap:wrap}.stats b{font-size:30px;display:block}.stats span{font-size:14px}article{background:white;border:1px solid #d6ded8;border-radius:12px;margin:0 0 36px;padding:26px;scroll-margin-top:20px}.assethead{display:flex;gap:18px;align-items:start;border-bottom:1px solid #d8e2dd;margin-bottom:24px}.num{font-size:35px}.assethead p,.caption,figcaption{color:#526367;font-size:14px}.columns{display:grid;grid-template-columns:1fr 1fr;gap:28px}.hero{display:block;width:100%;height:380px;object-fit:contain;background:#f4f4f0;border-radius:8px}img{max-width:100%;height:auto}figure{margin:25px 0}figure img{width:100%;border-radius:8px}.adapt{color:#976319}details{border-top:1px solid #d6ded8;padding:14px 0;margin-top:16px}summary{cursor:pointer;font-weight:650}table{border-collapse:collapse;width:100%;font-size:15px}td,th{text-align:left;vertical-align:top;padding:12px;border-bottom:1px solid #d6ded8}th{width:24%}.unknown,.decision{padding:16px 20px;border-radius:6px;margin-top:18px}.unknown{background:#f3f2ee}.decision{background:#e4f1eb}.decision p,.unknown p{margin-bottom:0}.banner{border-left:4px solid #c29240;padding:12px 20px;background:#fbf6e9}.footer{padding:20px;color:#526367;font-size:14px}@media(max-width:800px){header,main{padding:20px}.columns{grid-template-columns:1fr}.hero{height:330px}article{padding:18px}h2{font-size:24px}}@media print{nav,details{display:none}article{break-before:page}.hero{height:260px}body{background:white}}
</style><header><div class="eyebrow">LEGO SPACE RTS / T083 / ПАРТІЯ 01</div><h1>Rock Raiders.<br>Конструкція має бути впізнаваною.</h1><p>8 дизайн-кандидатів: від робітника до важкого транспортера. Завершені джерельні набори, конкретні адаптації та окремо позначене невідоме.</p><div class="stats"><span><b>8 / 35</b>пакетів підготовлено</span><span><b>0 / 35</b>дизайнів прийнято</span><span><b>0</b>нових ігрових моделей</span></div><p class="banner">T082 прийнято як референсну основу. Ця сторінка пропонує рішення T083 для огляду, не повторює blind review й не засвідчує модельну геометрію. Камера/LOD — після появи моделей у T084. Сірі T082 зображення — історичний порівняльний контекст, кольорові джерела — основа деталей; нові адаптації позначені прямо.</p><nav>'''+nav+'''</nav></header><main>'''+''.join(cards)+'''<p class="footer">Джерела LEGO Group та архівні фото — дослідницькі референси, не ігрові текстури. Нові generated-зображення — лише пропозиції зовнішнього вигляду. Статус прийняття змінюється тільки після відповіді режисера; ця сторінка не записує прийняття кнопками чи локальним сховищем.</p></main></html>'''
    hover_record = next(a for a in proposals['records'] if a['stableId'] == 'unit.rock_raiders.hover_scout')
    hover_picture = images['assets']['unit.rock_raiders.hover_scout']
    hover_review_path = BATCH / 'hover_scout_target_review.html'
    files[hover_review_path] = hover_scout_target_review(
        hover_record,
        rel(hover_picture['targetArtifact']),
        hover_picture['targetCaption'],
        rel(hover_picture['primary']),
    )
    if unit is not None:
        selected = next((a for a in proposals['records'] if a['stableId'] == unit), None)
        if selected is None:
            raise ValueError(f'Unknown or unauthored T083 unit: {unit}')
        key = unit.rsplit('.', 1)[-1]
        md_path, batch_review_path, registry_path = unit_output_paths(key)
        review_path = hover_review_path if unit == 'unit.rock_raiders.hover_scout' else batch_review_path
        generated_card = None if unit == 'unit.rock_raiders.hover_scout' else article(files[batch_review_path], key)

        current_registry = read('Docs/Development/M85UnitDesign/registry.json')
        expected_entry = next(a for a in registry if a['stableId'] == unit)
        if unit == 'unit.rock_raiders.crew':
            expected_entry['review'] = 'Batch01RockRaiders/crew_review.html'
            expected_entry['designSpec'] = 'Batch01RockRaiders/crew_design_spec.md'
            expected_entry['sourceEvidence'] = {
                'manifest': 'Batch01RockRaiders/References/source_manifest.json',
                'reference': '4930-outbox.jpg',
                'provenance': 'Archive-hosted photograph of the completed official set; photographer/license unverified; research/reference only.'
            }
            expected_entry['productionDesignTarget'].update({
                'state': 'DIRECTOR_REVIEW_READY',
                'artifact': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/crew_review.html',
                'designSpec': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/crew_design_spec.md',
                'sourceImage': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/References/4930-outbox.jpg',
                'reviewReady': True,
                'appearanceSummary': 'One Tiny Crew unit identity with five source-specific 4930 figure looks and their individual visible headwear, clothing colors and tools; no visible game-geometry adaptation.'
            })
        if unit == 'unit.rock_raiders.hover_scout':
            expected_entry['review'] = 'Batch01RockRaiders/hover_scout_target_review.html'
            expected_entry['designSpec'] = 'Batch01RockRaiders/hover_scout.md'
            expected_entry['sourceEvidence'] = {
                'manifest': 'Batch01RockRaiders/References/hover_scout_source_manifest.json',
                'reference': '4910-page01.png',
                'provenance': 'Official LEGO 4910 instruction PDF page 1; unmodified full-page raster retained in the unit-specific source manifest.'
            }
            expected_entry['productionDesignTarget'].update({
                'mode': 'SOURCE_LOCKED',
                'state': 'DIRECTOR_ACCEPTED' if expected_entry['directorAccepted'] else 'DIRECTOR_REVIEW_READY',
                'artifact': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/hover_scout_target_review.html',
                'rationale': 'Official LEGO 4910 completed Scout at instruction step 7, framed directly from the official page; no new exterior geometry.',
                'designSpec': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/hover_scout.md',
                'sourceImage': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/References/4910-hover-scout-target-step7.png',
                'reviewReady': True,
                'appearanceSummary': 'Build the open low plate sled shown at official 4910 instruction step 7: seated blue/turquoise operator, large forward angular scanner/tool assembly, exposed deck and separate rear tool rack; no closed hull, invented sensor tower or added underside hero mass.'
            })
        if unit == 'unit.rock_raiders.rapid_rider':
            expected_entry['review'] = 'Batch01RockRaiders/rapid_rider_target_review.html'
            expected_entry['sourceEvidence'] = {
                'manifest': 'Batch01RockRaiders/References/source_manifest.json',
                'reference': '4920-page01.png',
                'provenance': 'Official LEGO 4920 instruction PDF page 1; unmodified full-page raster; research/reference only.'
            }
            expected_entry['designSpec'] = 'Batch01RockRaiders/rapid_rider.md'
            expected_entry['productionDesignTarget'].update({
                'mode': 'SOURCE_LOCKED',
                'state': 'DIRECTOR_ACCEPTED' if expected_entry['directorAccepted'] else 'TARGET_FOR_DIRECTOR_REVIEW',
                'artifact': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/rapid_rider_target_review.html',
                'rationale': 'The official completed LEGO 4920 already represents the intended exterior; preserve its one-driver appearance and source proportions.',
                'designSpec': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/rapid_rider.md',
                'sourceImage': 'Docs/Development/M85UnitDesign/Batch01RockRaiders/References/4920-page01.png',
                'reviewReady': True,
                'appearanceSummary': 'Official completed LEGO 4920 Rapid Rider, preserving the twin-hull exterior, source proportions, open cargo tub and one source-scale driver; no added passenger seating or visible exterior adaptation.'
            })
        current_entry = next((a for a in current_registry['assets'] if a['stableId'] == unit), None)
        if current_entry is None:
            raise ValueError(f'Missing registry identity: {unit}')

        if unit in accepted_unit_ids(current_registry):
            print(f'PASS: {unit} is director-accepted; frozen package and registry outputs preserved')
            return

        if check:
            assert md_path.is_file() and md_path.read_text() == files[md_path], f'Stale generated artifact: {md_path}'
            if unit == 'unit.rock_raiders.hover_scout':
                assert review_path.is_file() and review_path.read_text() == files[review_path], f'Stale generated review artifact: {review_path}'
            else:
                assert batch_review_path.is_file() and article(batch_review_path.read_text(), key) == generated_card, f'Stale generated review card: {batch_review_path}#{key}'
            assert current_entry == expected_entry, f'Stale registry record: {registry_path}#{unit}'
        else:
            md_path.write_text(files[md_path])
            if unit == 'unit.rock_raiders.hover_scout':
                review_path.write_text(files[review_path])
            else:
                batch_review_path.write_text(replace_article(batch_review_path.read_text(), key, generated_card))
            update_registry_entry(current_registry, expected_entry)
            current_registry['authoredProposals'] = sum(a.get('status') != 'QUEUED_NOT_AUTHORED' for a in current_registry['assets'])
            current_registry['directorAccepted'] = sum(bool(a.get('directorAccepted')) for a in current_registry['assets'])
            current_registry['productionModels'] = sum(bool(a.get('productionModel')) for a in current_registry['assets'])
            registry_path.write_text(json.dumps(current_registry, ensure_ascii=False, indent=2) + '\n')
        print(f'PASS: {unit} regenerated/checked independently; unrelated unit package files were not written')
        return

    # Accepted registry records own their approved package files. Keep their
    # Markdown/review cards untouched; only proposal-owned outputs participate
    # in reproducibility checks or regeneration.
    current_review = (BATCH / 'review.html').read_text()
    files[BATCH / 'review.html'] = preserve_accepted_review_cards(
        files[BATCH / 'review.html'], current_review,
        proposals['records'], accepted_ids,
    )
    proposal_files = proposal_outputs_for_check(files, proposals['records'], accepted_ids)
    for p, data in proposal_files.items():
        if check:
            assert p.is_file() and p.read_text()==data, f'Stale generated artifact: {p}'
        else:
            p.parent.mkdir(parents=True,exist_ok=True);p.write_text(data)
    accepted=sum(a.get('directorAccepted',False) for a in registry)
    print(f'PASS: 35 exact roster identities, {len(rr_ids)+len(preserved)} authored proposals, {accepted} approvals; {len(sources)} source images and {len(images["generated"])} generated images hash-checked; {len(proposal_files)} proposal-owned documents '+('match' if check else 'written'))

if __name__ == '__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');parser.add_argument('--unit',help='Regenerate or check one authored T083 unit by stable ID');args=parser.parse_args();run(args.check,args.unit)
