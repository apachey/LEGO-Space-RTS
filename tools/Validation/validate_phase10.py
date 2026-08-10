#!/usr/bin/env python3
from pathlib import Path
import json, re, sys

ROOT = Path(__file__).resolve().parents[2]
errors = []

def require(path):
    p = ROOT / path
    if not p.exists(): errors.append(f"missing: {path}")
    return p

def check(cond, message):
    if not cond: errors.append(message)

required = [
    'SimCore/Runtime/Core/Fix32.cs','SimCore/Runtime/Core/FixVec2.cs','SimCore/Runtime/Core/Angle16.cs',
    'SimCore/Runtime/Ecs/EntityStore.cs','SimCore/Runtime/Commands/Commands.cs',
    'SimCore/Runtime/Serialization/SnapshotSerializer.cs','SimCore/Runtime/Replay/ReplayLog.cs',
    'SimCore/Runtime/Navigation/HierarchicalPathfinder.cs','SimCore/Runtime/Simulation/SimulationRunner.cs',
    'HeadlessSim/Program.cs','Content/PrototypeEntities.json','Content/Maps/DEV_FirstControllableRTS.map.json',
    'GodotClient/project.godot','GodotClient/LEGO.SpaceRTS.Godot.csproj','GodotClient/LEGO.SpaceRTS.Godot.sln',
    'GodotClient/Scenes/Bootstrap.tscn','GodotClient/Scenes/PrototypeRTS.tscn',
    'GodotClient/Scripts/Client/RtsCompositionRoot.cs','GodotClient/Scripts/Client/RuntimeScenarioLoader.cs',
    'GodotClient/Scripts/Presentation/GodotSimBridge.cs','GodotClient/Scripts/Presentation/RtsCameraController.cs',
    'GodotClient/Scripts/Presentation/SelectionController.cs','GodotClient/Scripts/Presentation/RtsInputController.cs',
    'GodotClient/Scripts/Presentation/FogPresenter.cs','GodotClient/Scripts/Presentation/DebugRenderer.cs',
    'GodotClient/Scripts/UI/BasicHud.cs','GodotClient/Scripts/UI/DebugHud.cs','Docs/IMPLEMENTATION_REPORT.md',
    'tools/doctor.sh','tools/verify.sh','tools/run-game.sh','tools/build-mac.sh','tools/capture-visual-smoke.sh',
    'tools/setup-git-hooks.sh','.githooks/pre-commit','.githooks/pre-push',
    '.github/workflows/simcore-pr.yml','global.json','Docs/Development/AGENT_WORKFLOW.md'
]
for r in required: require(r)

runtime = list((ROOT/'SimCore/Runtime').rglob('*.cs'))
runtime_text = '\n'.join(p.read_text(encoding='utf-8') for p in runtime)
check('UnityEngine' not in runtime_text, 'authoritative SimCore runtime contains UnityEngine')
check('Godot' not in runtime_text, 'authoritative SimCore runtime contains Godot engine reference/token')
check(not re.search(r'\b(float|double)\b', runtime_text), 'authoritative SimCore runtime contains float/double')
check(not re.search(r'^\s*namespace\s+[\w.]+\s*;', runtime_text, re.M), 'C# 10 file-scoped namespace found in portable SimCore')
check('record struct' not in runtime_text, 'C# 10 record struct found in portable SimCore')
check('TODO' not in runtime_text and 'FIXME' not in runtime_text, 'placeholder marker found in SimCore runtime')

simproj = (ROOT/'SimCore/LegoSpaceRTS.SimCore.csproj').read_text()
check('<TargetFramework>netstandard2.1</TargetFramework>' in simproj, 'SimCore portable target is not netstandard2.1')
check('<LangVersion>9.0</LangVersion>' in simproj, 'SimCore portable language boundary is not C# 9.0')
check('Godot.NET.Sdk' not in simproj and 'Unity' not in simproj, 'SimCore project has an engine SDK/dependency')
check(not (ROOT/'SimCore/Runtime/LegoSpaceRTS.SimCore.asmdef').exists(), 'legacy Unity asmdef still exists in SimCore')
check(not (ROOT/'Game').exists(), 'legacy Unity Game directory still exists')

godotproj = (ROOT/'GodotClient/LEGO.SpaceRTS.Godot.csproj').read_text()
check('Godot.NET.Sdk/4.7.1' in godotproj, 'Godot .NET SDK is not pinned to 4.7.1')
check('<TargetFramework>net8.0</TargetFramework>' in godotproj, 'Godot client does not target net8.0')
check('../SimCore/LegoSpaceRTS.SimCore.csproj' in godotproj, 'Godot client does not reference portable SimCore')
content_bin=require('GodotClient/Compiled/PrototypeEntities.contentbin')
map_bin=require('GodotClient/Compiled/DEV_FirstControllableRTS.mapbin')
if content_bin.exists():
    data=content_bin.read_bytes(); check(len(data)>=8 and data[:4]==bytes.fromhex('4c535043'), 'compiled content magic/size mismatch')
if map_bin.exists():
    data=map_bin.read_bytes(); check(len(data)>=6 and data[:4]==bytes.fromhex('5354524d'), 'compiled map magic/size mismatch')

project_godot = (ROOT/'GodotClient/project.godot').read_text()
check('config/features=PackedStringArray("4.7", "C#", "Forward Plus")' in project_godot, 'Godot project features are not pinned to 4.7/C#/Forward Plus')
check('run/main_scene="res://Scenes/Bootstrap.tscn"' in project_godot, 'Godot main scene is not Bootstrap')
check('physics_ticks_per_second=60' in project_godot, 'Godot presentation physics cadence is not explicitly non-authoritative 60 Hz')
check('textures/vram_compression/import_etc2_astc=true' in project_godot, 'Godot macOS universal texture import support is not enabled')

export_presets = (ROOT/'GodotClient/export_presets.cfg').read_text()
check('name="macOS"' in export_presets, 'macOS export preset missing')
check('export_path="../Builds/macOS/LEGO Space RTS.app"' in export_presets, 'macOS export path mismatch')
check(export_presets.count('include_filter="Compiled/*.contentbin,Compiled/*.mapbin"') == 2, 'compiled runtime data is not included in desktop exports')
global_json = json.loads((ROOT/'global.json').read_text())
check(global_json.get('sdk',{}).get('version') == '8.0.100', '.NET minimum SDK policy is not 8.0.100')
check(global_json.get('sdk',{}).get('rollForward') == 'latestMajor', '.NET SDK roll-forward policy mismatch')

ci_workflow = (ROOT/'.github/workflows/simcore-pr.yml').read_text()
check('Tools/' not in ci_workflow, 'CI workflow contains a case-sensitive legacy Tools/ path')
check('python3 tools/Validation/validate_phase10.py' in ci_workflow, 'CI workflow does not run lowercase static validation path')
check('tools/ContentCompiler/ContentCompiler.csproj' in ci_workflow, 'CI workflow does not use lowercase ContentCompiler path')

bridge = (ROOT/'GodotClient/Scripts/Presentation/GodotSimBridge.cs').read_text()
check('TickSeconds = 0.05' in bridge, 'Godot bridge does not feed 20 Hz/50ms simulation ticks')
check('MaxCatchUpTicks = 4' in bridge, 'Godot bridge catch-up limit is not four ticks')
check('Excess remainder stays queued' in bridge, 'Godot bridge does not document no authoritative tick skipping')
composition = (ROOT/'GodotClient/Scripts/Client/RtsCompositionRoot.cs').read_text()
check('RuntimeScenarioLoader.LoadCanonicalOpening' in composition, 'Godot composition root does not use canonical runtime opening loader')
check('FindChild' not in composition and 'GetNode<' not in composition, 'composition root uses runtime dependency discovery')
loader = (ROOT/'GodotClient/Scripts/Client/RuntimeScenarioLoader.cs').read_text()
check('PrototypeContentCodec.Read' in loader and 'CompiledMapCodec.ReadDefinition' in loader, 'Godot runtime does not consume compiled content/map when present')
basic_hud = (ROOT/'GodotClient/Scripts/UI/BasicHud.cs').read_text()
for token in ['ResourceStrip','SelectionPanel','PortraitSlot','ContextualSlot','ContextualActions','ContextualEnergyPriority','EnergyDomainPopover','OPERATIONS','CRYSTALS']:
    check(token in basic_hud, f'T039 Basic HUD element missing: {token}')
check('CommandPanel' not in basic_hud, 'production must be contextual to selected facilities rather than a permanent separate panel')
debug_hud = (ROOT/'GodotClient/Scripts/UI/DebugHud.cs').read_text()
check('Visible = false' in debug_hud and 'Drain Energy' in debug_hud, 'developer tools must remain available but hidden by default')
debug_renderer = (ROOT/'GodotClient/Scripts/Presentation/DebugRenderer.cs').read_text()
for token in ['DrawNavigation { get; set; } = true','DrawClusters { get; set; } = true','DrawPaths { get; set; } = true','DrawExcavatable { get; set; } = true']:
    check(token not in debug_renderer, f'developer visualization leaks into normal play: {token}')
input_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsInputController.cs').read_text()
check('OrderNumber' not in input_controller and 'DestinationRing' in input_controller, 'move feedback must use an unnumbered restrained destination marker')
unit_view = (ROOT/'GodotClient/Scripts/Presentation/UnitViewManager.cs').read_text()
check('ConstructionProgressLabel' not in unit_view and 'ConstructionProgressBar' in unit_view, 'construction progress must use a restrained world bar rather than a fixed-size billboard label')

clock = (ROOT/'SimCore/Runtime/Core/SimTime.cs').read_text()
check('TicksPerSecond = 20' in clock, 'SimClock is not 20 Hz')
mapgrid = (ROOT/'SimCore/Runtime/Map/MapGrid.cs').read_text()
check('BuildWidth = 160' in mapgrid and 'BuildHeight = 160' in mapgrid and 'NavPerBuild = 2' in mapgrid, 'map dimensions/nav scale mismatch')
nav = (ROOT/'SimCore/Runtime/Navigation/HierarchicalPathfinder.cs').read_text()
check('ClusterSize = 10' in nav, 'HPA cluster baseline is not 10 nav cells')
systems = (ROOT/'SimCore/Runtime/Simulation/SimulationSystems.cs').read_text()
check('ReservationPlanningSystem' not in systems, 'universal normal-locomotion reservation planner is still present')
check('LocalSeparationSystem' in systems and 'LookaheadTicks = 6' in systems, 'bounded local-separation foundation missing')
commands = (ROOT/'SimCore/Runtime/Commands/Commands.cs').read_text()
check('Capacity = 16' in commands, 'command queue capacity is not 16')
selection = (ROOT/'GodotClient/Scripts/Presentation/SelectionController.cs').read_text()
check('128' in selection, 'selection 128-entity foundation missing')
input_bindings = (ROOT/'GodotClient/Scripts/Client/InputBindings.cs').read_text()
check('InputMap' in input_bindings and 'debug_open_excavatable' in input_bindings, 'Godot InputMap action foundation missing')
check('debug_hud_toggle' in input_bindings, 'developer HUD does not have a dedicated hidden-panel toggle')
for token in ['SimCommandType.Move','SimCommandType.Stop','SimCommandType.HoldPosition','SimCommandType.DebugOpenExcavatable']:
    check(token in input_controller, f'Godot M2 command missing: {token}')

source = json.loads((ROOT/'Content/Maps/DEV_FirstControllableRTS.map.json').read_text())
check(source.get('schemaVersion') == 3, 'map source schema version mismatch')
check(source.get('stableId') == 'map.dev_first_controllable_rts', 'map stable ID mismatch')
check(source.get('buildSize') == [160,160] and source.get('navScale') == 2, 'map source dimensions mismatch')
check(len(source.get('starts',[])) >= 2, 'map starts missing')
check(len(source.get('flagRects',[])) >= 10, 'authored obstacle/pathing data missing')
check(len(source.get('elevationRects',[])) >= 2, 'elevation test data missing')
check(len(source.get('excavatableFeatures',[])) == 1, 'expected one M2 Excavatable feature')
check(len(source.get('initialEntities',[])) >= 26, 'initial prototype entity spawns missing')
check(len(source.get('resourceNodes',[])) == 4, 'M3 starting Ore node spawns missing')
check(len(source.get('resourceReceivers',[])) == 2, 'M3 starting HQ resource receivers missing')
check(len(source.get('visionTestGeometry',[])) >= 4, 'vision test geometry missing')

content_source = json.loads((ROOT/'Content/PrototypeEntities.json').read_text())
check(content_source.get('schemaVersion') == 10 and content_source.get('contentKind') == 'prototype_entities', 'prototype content schema mismatch')
entity_keys = [e.get('stableId') for e in content_source.get('entities',[])]
check(len(entity_keys) >= 5 and len(entity_keys) == len(set(entity_keys)), 'prototype content entries missing/duplicated')
for required_key in ['building.rock_raiders.hq','building.rock_raiders.ore_processing_plant','building.rock_raiders.power_station','building.rock_raiders.vehicle_service_bay','unit.rock_raiders.crew','unit.rock_raiders.hover_scout','unit.rock_raiders.rapid_rider','unit.rock_raiders.loader_dozer','unit.rock_raiders.chrome_crusher','prototype.nav.huge']:
    check(required_key in entity_keys, f'prototype content key missing: {required_key}')
by_key={e.get('stableId'):e for e in content_source.get('entities',[])}
check(by_key.get('unit.rock_raiders.loader_dozer',{}).get('footprint')=='Medium','Loader Dozer M2 footprint must match Phase 06 Medium')
check(by_key.get('unit.rock_raiders.chrome_crusher',{}).get('footprint')=='Large','Chrome Crusher M2 footprint must match Phase 06 Large')
check(by_key.get('prototype.nav.huge',{}).get('sourceClassification')=='ENGINEERING_ONLY','Huge stress profile must remain engineering-only')
check(by_key.get('unit.rock_raiders.crew',{}).get('workerHarvest') == {'oreTicksPerUnit':30,'oreCarryCapacity':8}, 'Crew canonical Ore harvesting metadata missing')
expected_unit_oc={'unit.rock_raiders.crew':1,'unit.rock_raiders.hover_scout':1,'unit.rock_raiders.rapid_rider':2,'unit.rock_raiders.loader_dozer':3,'unit.rock_raiders.chrome_crusher':6}
check({key:by_key.get(key,{}).get('operationsCapacity') for key in expected_unit_oc} == expected_unit_oc, 'canonical Rock Raider Operations Capacity metadata missing or incorrect')
expected_combat_targets={
    'unit.rock_raiders.crew':('Personnel','Ground',['CombatThreat','Worker','Support']),
    'unit.rock_raiders.hover_scout':('LightMachine','Ground',['CombatThreat','Support']),
    'unit.rock_raiders.loader_dozer':('MediumMachine','Ground',['CombatThreat']),
    'unit.rock_raiders.rapid_rider':('LightMachine','Ground',['Transport']),
    'unit.rock_raiders.chrome_crusher':('MassiveMachine','Ground',['CombatThreat']),
    'building.rock_raiders.hq':('FortifiedStructure','Ground',['Command']),
    'building.rock_raiders.ore_processing_plant':('Structure','Ground',['EconomicInfrastructure']),
    'building.rock_raiders.power_station':('Structure','Ground',['EconomicInfrastructure']),
    'building.rock_raiders.vehicle_service_bay':('Structure','Ground',['Production']),
}
check({key:(by_key.get(key,{}).get('combatTarget',{}).get('class'),by_key.get(key,{}).get('combatTarget',{}).get('layer'),by_key.get(key,{}).get('combatTarget',{}).get('flags')) for key in expected_combat_targets} == expected_combat_targets, 'canonical M4 target classes, layers or role flags missing')
expected_targeting={
    'unit.rock_raiders.crew':([4,5],'Support'),
    'unit.rock_raiders.hover_scout':([3,1],'Scout'),
    'unit.rock_raiders.loader_dozer':([9,10],'AntiLight'),
    'unit.rock_raiders.chrome_crusher':([21,20],'Siege'),
}
check({key:(by_key.get(key,{}).get('targeting',{}).get('weaponRangeRatio'),by_key.get(key,{}).get('targeting',{}).get('priorityProfile')) for key in expected_targeting} == expected_targeting, 'canonical M4 Rock Raider targeting profiles or weapon-range inputs missing')
check('targeting' not in by_key.get('unit.rock_raiders.rapid_rider',{}), 'Rapid Rider is an unarmed transport and must not acquire attack targets')
profile_by_key={p.get('stableId'):p for p in content_source.get('movementProfiles',[])}
check(profile_by_key.get('movement.prototype.crew',{}).get('speedRatio')==[135,100],'Crew M2 speed must match Phase 06 1.35')
check(profile_by_key.get('movement.prototype.hover_scout',{}).get('speedRatio')==[225,100],'Hover Scout M2 speed must match Phase 06 2.25')
check(profile_by_key.get('movement.prototype.loader_dozer',{}).get('speedRatio')==[130,100],'Loader Dozer M2 speed must match Phase 06 1.30')
check(profile_by_key.get('movement.prototype.rapid_rider',{}).get('speedRatio')==[210,100],'Rapid Rider speed must match Phase 06 2.10')
check(profile_by_key.get('movement.prototype.chrome_crusher',{}).get('speedRatio')==[92,100],'Chrome Crusher M2 speed must match Phase 06 0.92')
map_keys = [e.get('contentKey') for e in source.get('initialEntities',[])]
check(all(k in set(entity_keys) for k in map_keys), 'map source contains unknown prototype entity reference')
resource_by_key={r.get('stableId'):r for r in content_source.get('resourceNodeDefinitions',[])}
expected_ore={'resource.ore.small':600,'resource.ore.standard':900,'resource.ore.rich':1350,'resource.ore.deep_contested_seam':2400}
check({key:resource_by_key.get(key,{}).get('capacity') for key in expected_ore} == expected_ore, 'canonical M3 Ore capacities missing or incorrect')
check(all(resource_by_key.get(key,{}).get('type') == 'Ore' and resource_by_key.get(key,{}).get('depletionProfile') == 'Finite' for key in expected_ore), 'Ore resource nodes must use finite depletion')
resource_map_keys=[e.get('contentKey') for e in source.get('resourceNodes',[])]
check(all(k in resource_by_key for k in resource_map_keys), 'map source contains unknown resource node reference')
check(resource_map_keys.count('resource.ore.standard') == 4, 'prototype map must provide two Standard Ore deposits per start')
receiver_map_keys=[e.get('contentKey') for e in source.get('resourceReceivers',[])]
check(receiver_map_keys == ['building.rock_raiders.hq','building.rock_raiders.hq'], 'prototype map must provide one HQ receiver per start')
production_by_unit={p.get('unit'):p for p in content_source.get('productionDefinitions',[])}
expected_production={
    'unit.rock_raiders.crew':('building.rock_raiders.hq',50,0,1,320),
    'unit.rock_raiders.hover_scout':('building.rock_raiders.vehicle_service_bay',75,10,1,400),
    'unit.rock_raiders.rapid_rider':('building.rock_raiders.vehicle_service_bay',90,10,2,560),
    'unit.rock_raiders.loader_dozer':('building.rock_raiders.vehicle_service_bay',125,15,3,720),
}
check(len(production_by_unit)==4,'first-playable production definitions missing or duplicated')
for unit,(producer,ore,energy,oc,ticks) in expected_production.items():
    production=production_by_unit.get(unit,{})
    check(production.get('producer')==producer and production.get('cost')=={'ore':ore,'energy':energy,'crystals':0} and production.get('operationsCapacity')==oc and production.get('buildTicks')==ticks, f'canonical production definition mismatch: {unit}')
building_by_key={b.get('stableId'):b for b in content_source.get('buildingDefinitions',[])}
expected_buildings={
    'building.rock_raiders.hq':([8,8],320,40,1200),
    'building.rock_raiders.ore_processing_plant':([6,6],140,15,600),
    'building.rock_raiders.power_station':([5,5],150,20,700),
    'building.rock_raiders.vehicle_service_bay':([8,6],160,20,800),
}
for key,(size,ore,energy,ticks) in expected_buildings.items():
    definition=building_by_key.get(key,{})
    mask=definition.get('footprintMask',[])
    actual_size=[len(mask[0]) if mask else 0,len(mask)]
    check(actual_size==size and definition.get('cost')=={'ore':ore,'energy':energy} and definition.get('buildTicks')==ticks, f'canonical M3 building definition mismatch: {key}')
expected_capacity_providers={'building.rock_raiders.hq':16,'building.rock_raiders.ore_processing_plant':0,'building.rock_raiders.power_station':0,'building.rock_raiders.vehicle_service_bay':4}
check({key:building_by_key.get(key,{}).get('operationsCapacityProvided') for key in expected_capacity_providers} == expected_capacity_providers, 'canonical Rock Raider Operations Capacity providers missing or incorrect')
expected_energy={
    'building.rock_raiders.hq':(2,150,0),
    'building.rock_raiders.ore_processing_plant':(0,0,1),
    'building.rock_raiders.power_station':(10,120,0),
    'building.rock_raiders.vehicle_service_bay':(0,0,1),
}
check({key:(building_by_key.get(key,{}).get('energyGenerationPerSecond'),building_by_key.get(key,{}).get('energyReserveCapacity'),building_by_key.get(key,{}).get('continuousEnergyDemandPerSecond')) for key in expected_energy} == expected_energy, 'canonical Rock Raider Energy generation, reserve or demand metadata missing or incorrect')
expected_energy_classes = {
    'building.rock_raiders.hq': 'CommandAndBasicEconomy',
    'building.rock_raiders.ore_processing_plant': 'ResourceProcessing',
    'building.rock_raiders.power_station': 'StaticDefenseAndNonessential',
    'building.rock_raiders.vehicle_service_bay': 'ProductionAndResearch',
}
check({key:building_by_key.get(key,{}).get('energyFunctionalClass') for key in expected_energy_classes} == expected_energy_classes, 'canonical Brownout functional classes missing or incorrect')

headless = (ROOT/'HeadlessSim/Program.cs').read_text()
for token in ['--snapshot-in','--snapshot-out','--replay','--record-replay','--benchmark','--path-benchmark','--hash-every','--repeat','--golden-manifest-out','--golden-manifest-in','--dump-state','--compiled-dir']:
    check(token in headless, f'HeadlessSim switch missing: {token}')
check('DETERMINISM CHECKPOINT FAILURE' in headless, 'HeadlessSim repeated-run checkpoint comparison missing')

# Very lightweight lexical delimiter audit after stripping strings/comments approximately.
def strip_strings_comments(s):
    # Strip string/char literals before comments so Godot paths such as res:// do not
    # masquerade as line comments during this deliberately lightweight audit.
    s = re.sub(r'@?"(?:""|\\.|[^"\\])*"', '""', s)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    s = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
    s = re.sub(r'//.*', '', s)
    return s
for p in list(ROOT.rglob('*.cs')):
    t = strip_strings_comments(p.read_text(encoding='utf-8'))
    for op,cl in [('(',')'),('[',']'),('{','}')]:
        check(t.count(op)==t.count(cl), f'delimiter imbalance {op}{cl}: {p.relative_to(ROOT)}')

# No legacy Unity host/source metadata may survive the amendment except historical wording in the amendment doc.
for p in ROOT.rglob('*'):
    if p.is_file() and p.suffix.lower() in {'.cs','.csproj','.sln','.json','.tscn','.godot','.cfg'}:
        txt = p.read_text(encoding='utf-8', errors='ignore')
        check('UnityEngine' not in txt or p.name == 'AssemblyBoundaryTests.cs', f'legacy UnityEngine reference outside boundary test: {p.relative_to(ROOT)}')

if errors:
    print('PHASE10 GODOT STATIC VALIDATION: FAIL')
    for e in errors: print(' -', e)
    sys.exit(1)
print(f'PHASE10 GODOT STATIC VALIDATION: PASS ({len(runtime)} authoritative C# files checked)')
