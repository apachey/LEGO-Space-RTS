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
    'GodotClient/project.godot','GodotClient/LEGO.SpaceRTS.Godot.csproj',
    'GodotClient/Scenes/Bootstrap.tscn','GodotClient/Scenes/PrototypeRTS.tscn',
    'GodotClient/Scripts/Client/RtsCompositionRoot.cs','GodotClient/Scripts/Client/RuntimeScenarioLoader.cs',
    'GodotClient/Scripts/Presentation/GodotSimBridge.cs','GodotClient/Scripts/Presentation/RtsCameraController.cs',
    'GodotClient/Scripts/Presentation/SelectionController.cs','GodotClient/Scripts/Presentation/RtsInputController.cs',
    'GodotClient/Scripts/Presentation/FogPresenter.cs','GodotClient/Scripts/Presentation/DebugRenderer.cs',
    'GodotClient/Scripts/UI/DebugHud.cs','Docs/IMPLEMENTATION_REPORT.md'
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

bridge = (ROOT/'GodotClient/Scripts/Presentation/GodotSimBridge.cs').read_text()
check('TickSeconds = 0.05' in bridge, 'Godot bridge does not feed 20 Hz/50ms simulation ticks')
check('MaxCatchUpTicks = 4' in bridge, 'Godot bridge catch-up limit is not four ticks')
check('Excess remainder stays queued' in bridge, 'Godot bridge does not document no authoritative tick skipping')
composition = (ROOT/'GodotClient/Scripts/Client/RtsCompositionRoot.cs').read_text()
check('RuntimeScenarioLoader.LoadFirstControllable' in composition, 'Godot composition root does not use runtime content loader')
check('FindChild' not in composition and 'GetNode<' not in composition, 'composition root uses runtime dependency discovery')
loader = (ROOT/'GodotClient/Scripts/Client/RuntimeScenarioLoader.cs').read_text()
check('PrototypeContentCodec.Read' in loader and 'CompiledMapCodec.ReadDefinition' in loader, 'Godot runtime does not consume compiled content/map when present')

clock = (ROOT/'SimCore/Runtime/Core/SimTime.cs').read_text()
check('TicksPerSecond = 20' in clock, 'SimClock is not 20 Hz')
mapgrid = (ROOT/'SimCore/Runtime/Map/MapGrid.cs').read_text()
check('BuildWidth = 160' in mapgrid and 'BuildHeight = 160' in mapgrid and 'NavPerBuild = 2' in mapgrid, 'map dimensions/nav scale mismatch')
nav = (ROOT/'SimCore/Runtime/Navigation/HierarchicalPathfinder.cs').read_text()
check('ClusterSize = 10' in nav, 'HPA cluster baseline is not 10 nav cells')
systems = (ROOT/'SimCore/Runtime/Simulation/SimulationSystems.cs').read_text()
check('HorizonTicks = 12' in systems, 'reservation horizon is not 12 ticks')
commands = (ROOT/'SimCore/Runtime/Commands/Commands.cs').read_text()
check('Capacity = 16' in commands, 'command queue capacity is not 16')
selection = (ROOT/'GodotClient/Scripts/Presentation/SelectionController.cs').read_text()
check('128' in selection, 'selection 128-entity foundation missing')
input_bindings = (ROOT/'GodotClient/Scripts/Client/InputBindings.cs').read_text()
check('InputMap' in input_bindings and 'debug_open_excavatable' in input_bindings, 'Godot InputMap action foundation missing')
input_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsInputController.cs').read_text()
for token in ['SimCommandType.Move','SimCommandType.Stop','SimCommandType.HoldPosition','SimCommandType.DebugOpenExcavatable']:
    check(token in input_controller, f'Godot M2 command missing: {token}')

source = json.loads((ROOT/'Content/Maps/DEV_FirstControllableRTS.map.json').read_text())
check(source.get('schemaVersion') == 1, 'map source schema version mismatch')
check(source.get('stableId') == 'map.dev_first_controllable_rts', 'map stable ID mismatch')
check(source.get('buildSize') == [160,160] and source.get('navScale') == 2, 'map source dimensions mismatch')
check(len(source.get('starts',[])) >= 2, 'map starts missing')
check(len(source.get('flagRects',[])) >= 10, 'authored obstacle/pathing data missing')
check(len(source.get('elevationRects',[])) >= 2, 'elevation test data missing')
check(len(source.get('excavatableFeatures',[])) == 1, 'expected one M2 Excavatable feature')
check(len(source.get('initialEntities',[])) >= 26, 'initial prototype entity spawns missing')
check(len(source.get('visionTestGeometry',[])) >= 4, 'vision test geometry missing')

content_source = json.loads((ROOT/'Content/PrototypeEntities.json').read_text())
check(content_source.get('schemaVersion') == 2 and content_source.get('contentKind') == 'prototype_entities', 'prototype content schema mismatch')
entity_keys = [e.get('stableId') for e in content_source.get('entities',[])]
check(len(entity_keys) >= 5 and len(entity_keys) == len(set(entity_keys)), 'prototype content entries missing/duplicated')
for required_key in ['unit.rock_raiders.crew','unit.rock_raiders.hover_scout','unit.rock_raiders.loader_dozer','unit.rock_raiders.chrome_crusher','prototype.nav.huge']:
    check(required_key in entity_keys, f'prototype content key missing: {required_key}')
by_key={e.get('stableId'):e for e in content_source.get('entities',[])}
check(by_key.get('unit.rock_raiders.loader_dozer',{}).get('footprint')=='Medium','Loader Dozer M2 footprint must match Phase 06 Medium')
check(by_key.get('unit.rock_raiders.chrome_crusher',{}).get('footprint')=='Large','Chrome Crusher M2 footprint must match Phase 06 Large')
check(by_key.get('prototype.nav.huge',{}).get('sourceClassification')=='ENGINEERING_ONLY','Huge stress profile must remain engineering-only')
profile_by_key={p.get('stableId'):p for p in content_source.get('movementProfiles',[])}
check(profile_by_key.get('movement.prototype.crew',{}).get('speedRatio')==[135,100],'Crew M2 speed must match Phase 06 1.35')
check(profile_by_key.get('movement.prototype.hover_scout',{}).get('speedRatio')==[225,100],'Hover Scout M2 speed must match Phase 06 2.25')
check(profile_by_key.get('movement.prototype.loader_dozer',{}).get('speedRatio')==[130,100],'Loader Dozer M2 speed must match Phase 06 1.30')
check(profile_by_key.get('movement.prototype.chrome_crusher',{}).get('speedRatio')==[92,100],'Chrome Crusher M2 speed must match Phase 06 0.92')
map_keys = [e.get('contentKey') for e in source.get('initialEntities',[])]
check(all(k in set(entity_keys) for k in map_keys), 'map source contains unknown prototype entity reference')

headless = (ROOT/'HeadlessSim/Program.cs').read_text()
for token in ['--snapshot-in','--snapshot-out','--replay','--record-replay','--benchmark','--path-benchmark','--hash-every','--repeat','--golden-manifest-out','--golden-manifest-in','--dump-state','--compiled-dir']:
    check(token in headless, f'HeadlessSim switch missing: {token}')

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
