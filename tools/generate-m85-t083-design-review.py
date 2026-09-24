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
    import re
    match = re.search(rf'<article id="{re.escape(key)}">.*?</article>', html_text, re.S)
    if not match:
        raise ValueError(f'Missing review card: {key}')
    return match.group(0)

def replace_article(html_text, key, replacement):
    import re
    pattern = rf'<article id="{re.escape(key)}">.*?</article>'
    updated, count = re.subn(pattern, lambda _: replacement, html_text, count=1, flags=re.S)
    if count != 1:
        raise ValueError(f'Expected one review card: {key}; found {count}')
    return updated

def unit_output_paths(key):
    review_name = 'hover_scout_target_review.html' if key == 'hover_scout' else 'review.html'
    return (BATCH / f'{key}.md', BATCH / review_name, OUT / 'registry.json')

def update_registry_entry(document, expected):
    matches = [a for a in document['assets'] if a['stableId'] == expected['stableId']]
    if len(matches) != 1:
        raise ValueError(f"Expected one registry identity: {expected['stableId']}")
    matches[0].clear()
    matches[0].update(expected)

def run(check=False):
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
    assert len(units) == 35 and len(proposals['records']) == 8
    assert {a['stableId'] for a in proposals['records']} == {a['stableId'] for a in units if a['faction'] == 'RockRaiders'}
    for item in sources + images['generated']:
        p = ROOT / item['file']
        assert p.is_file(), p
        assert hashlib.sha256(p.read_bytes()).hexdigest() == item['sha256'], p
    for a in proposals['records']:
        assert not a['directorAccepted'] and not a['productionAuthorized']
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
        target_artifact = (primary if primary.startswith('ArtSource/') else candidate_art[-1] if candidate_art else primary)
        target_mode = 'ADAPTED' if target_artifact.startswith('ArtSource/') else 'SOURCE_LOCKED'
        refs = [s for s in sources if s['setId'] == a['sourceSet']]
        ref_lines = ['- [' + Path(s['file']).name + '](' + rel(s['file']) + ') — [' + s['kind'] + '](' + s['sourceUrl'] + ')' for s in refs]
        source_packet = 'Docs/Development/M85SuperScout/Packets/' + sid.replace('.', '_') + '.md'
        source_text = (ROOT / source_packet).read_text()
        source_section = source_text.split('## B. Reference board\n',1)[1].split('\n## C.',1)[0]
        # Links inside the inherited evidence section are web links; no historical status is copied.
        txt = f'# {n:02d} · {a["title"]} — T083 design proposal\n\n'
        txt += f'**{sid} · {r["footprint"]} · {r["sourceClassification"]}**\n\n'
        txt += '**Статус:** завершений дизайн-кандидат для режисерського огляду; не прийнятий дизайн і не модель. T082 прийнято як основу.\n\n'
        target_caption = picture['caption'] if target_artifact == primary else next(v['caption'] for v in picture['additional'] if v['file'] == target_artifact)
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
            'hover_scout':'Keep Pivot_ScannerYaw and Pivot_ToolPitch at rest; no unsupported moving sensor tower. Socket_SurveyPulse/ScannerVfx sit at the nose pair. Selection stays at ground projection; Health above the operator.',
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
        txt += '## Рішення режисера\n\n'+a['reviewDecision']+'\n\n'
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
        cards.append(f'''<article id="{key}"><div class="assethead"><span class="num">{n:02}</span><div><h2>{esc(a['title'])}</h2><p>LEGO {a['sourceSet']} · {r['footprint']} · Кандидат, не прийнято</p></div></div><div class="columns"><div><h3>Production Design Target · {target_mode}</h3><a href="{esc(rel(target_artifact))}">{hero}</a><p class="caption">{esc(target_caption)}</p><h3>Source Evidence</h3>{source_link}{target_extra}<details><summary>T082 comparison</summary><img src="{esc(rel(selection[sid]['file']))}" alt="T082 comparison"><p>Comparative reference only; status preserved: {esc(selection[sid]['status'])}.</p></details></div><div><h3>Design Spec · appearance</h3><p>{esc(a['appearance'])}</p><h3 class="adapt">Allowed adaptation</h3><p>{esc(a['adaptation'])}</p><h3>Scale</h3><p>{esc(a['scale'])}</p><h3>Materials and team marker</h3><p>{esc(a['palette'])}</p><p>{esc(a['identificationTile'])}</p></div></div><details><summary>Construction, views and states</summary><p>{esc(a['construction'])}</p><ul>{''.join('<li>'+esc(v)+'</li>' for v in a['views'])}</ul><table>{state_html}</table></details><div class="unknown"><b>Unknowns / later checks</b><p>{esc(a['unknown'])}</p></div><div class="decision"><b>Director review</b><p>{esc(a['reviewDecision'])}</p></div><p><a href="{key}.md">Full Design Spec: sources, materials and mechanics →</a></p></article>''')
        registry.append({'stableId':sid,'file':'Batch01RockRaiders/'+key+'.md','status':'DESIGN_PROPOSAL_FOR_DIRECTOR_REVIEW','directorAccepted':False,'productionAuthorized':False,'sourceEvidence':{'manifest':'Batch01RockRaiders/References/source_manifest.json','sourceSet':a['sourceSet']},'designSpec':'Batch01RockRaiders/'+key+'.md','productionDesignTarget':{'mode':target_mode,'state':'TARGET_FOR_DIRECTOR_REVIEW','artifact':target_artifact,'rationale':'Official source visual retained closely.' if target_mode=='SOURCE_LOCKED' else 'Explicit game adaptation visual.'}})
    existing_registry = read('Docs/Development/M85UnitDesign/registry.json')
    rr_ids = {a['stableId'] for a in registry}
    preserved = [a for a in existing_registry['assets'] if a['stableId'] not in rr_ids and a.get('status') != 'QUEUED_NOT_AUTHORED']
    registry.extend(preserved)
    for u in units:
        if u['stableId'] not in {a['stableId'] for a in registry}:
            registry.append({'stableId':u['stableId'],'status':'QUEUED_NOT_AUTHORED','directorAccepted':False,'productionAuthorized':False})
    files[OUT/'registry.json']=json.dumps({'task':'T083','target':35,'authoredProposals':len(rr_ids)+len(preserved),'directorAccepted':sum(a.get('directorAccepted',False) for a in registry),'productionModels':sum(a.get('productionModel',False) for a in registry),'assets':registry},ensure_ascii=False,indent=2)+'\n'
    nav=''.join(f'<a href="#{a["stableId"].rsplit(".",1)[-1]}">{n:02} {esc(a["title"])}</a>' for n,a in enumerate(proposals['records'],1))
    files[BATCH/'review.html']='''<!doctype html><html lang="uk"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>T083 · Rock Raiders design review</title><style>
:root{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;color:#172c32;background:#eff1ed;font-size:17px;line-height:1.55}*{box-sizing:border-box}body{margin:0}header,main{max-width:1260px;margin:auto;padding:32px}header{padding-top:60px}h1{font-size:clamp(32px,5vw,62px);line-height:1.06;max-width:950px;margin:14px 0 24px}h2{font-size:29px;margin:0}h3{font-size:18px;margin:24px 0 6px}p{margin:8px 0 18px}a{color:#006d70;text-underline-offset:3px}nav{display:flex;flex-wrap:wrap;gap:8px;margin:24px 0}nav a{background:white;border:1px solid #cad5ce;padding:7px 12px;border-radius:6px;text-decoration:none;font-size:14px}.eyebrow,.num{font-family:monospace;letter-spacing:2px;color:#006f70}.stats{display:flex;gap:25px;flex-wrap:wrap}.stats b{font-size:30px;display:block}.stats span{font-size:14px}article{background:white;border:1px solid #d6ded8;border-radius:12px;margin:0 0 36px;padding:26px;scroll-margin-top:20px}.assethead{display:flex;gap:18px;align-items:start;border-bottom:1px solid #d8e2dd;margin-bottom:24px}.num{font-size:35px}.assethead p,.caption,figcaption{color:#526367;font-size:14px}.columns{display:grid;grid-template-columns:1fr 1fr;gap:28px}.hero{display:block;width:100%;height:380px;object-fit:contain;background:#f4f4f0;border-radius:8px}img{max-width:100%;height:auto}figure{margin:25px 0}figure img{width:100%;border-radius:8px}.adapt{color:#976319}details{border-top:1px solid #d6ded8;padding:14px 0;margin-top:16px}summary{cursor:pointer;font-weight:650}table{border-collapse:collapse;width:100%;font-size:15px}td,th{text-align:left;vertical-align:top;padding:12px;border-bottom:1px solid #d6ded8}th{width:24%}.unknown,.decision{padding:16px 20px;border-radius:6px;margin-top:18px}.unknown{background:#f3f2ee}.decision{background:#e4f1eb}.decision p,.unknown p{margin-bottom:0}.banner{border-left:4px solid #c29240;padding:12px 20px;background:#fbf6e9}.footer{padding:20px;color:#526367;font-size:14px}@media(max-width:800px){header,main{padding:20px}.columns{grid-template-columns:1fr}.hero{height:330px}article{padding:18px}h2{font-size:24px}}@media print{nav,details{display:none}article{break-before:page}.hero{height:260px}body{background:white}}
</style><header><div class="eyebrow">LEGO SPACE RTS / T083 / ПАРТІЯ 01</div><h1>Rock Raiders.<br>Конструкція має бути впізнаваною.</h1><p>8 дизайн-кандидатів: від робітника до важкого транспортера. Завершені джерельні набори, конкретні адаптації та окремо позначене невідоме.</p><div class="stats"><span><b>8 / 35</b>пакетів підготовлено</span><span><b>0 / 35</b>дизайнів прийнято</span><span><b>0</b>нових ігрових моделей</span></div><p class="banner">T082 прийнято як референсну основу. Ця сторінка пропонує рішення T083 для огляду, не повторює blind review й не засвідчує модельну геометрію. Камера/LOD — після появи моделей у T084. Сірі T082 зображення — історичний порівняльний контекст, кольорові джерела — основа деталей; нові адаптації позначені прямо.</p><nav>'''+nav+'''</nav></header><main>'''+''.join(cards)+'''<p class="footer">Джерела LEGO Group та архівні фото — дослідницькі референси, не ігрові текстури. Нові generated-зображення — лише пропозиції зовнішнього вигляду. Статус прийняття змінюється тільки після відповіді режисера; ця сторінка не записує прийняття кнопками чи локальним сховищем.</p></main></html>'''
    for p, data in files.items():
        if check:
            assert p.is_file() and p.read_text()==data, f'Stale generated artifact: {p}'
        else:
            p.parent.mkdir(parents=True,exist_ok=True);p.write_text(data)
    accepted=sum(a.get('directorAccepted',False) for a in registry)
    print(f'PASS: 35 exact roster identities, {len(rr_ids)+len(preserved)} authored proposals, {accepted} approvals; {len(sources)} source images and {len(images["generated"])} generated images hash-checked; {len(files)} documents '+('match' if check else 'written'))

if __name__ == '__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args();run(args.check)
