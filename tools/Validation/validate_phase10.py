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
    'GodotClient/Scripts/Presentation/UnitViewManager.cs','GodotClient/Scripts/Presentation/PresentationAnimationDriver.cs',
    'GodotClient/Scripts/Presentation/PresentationVfxPool.cs','GodotClient/Scripts/Presentation/M7LookProfile.cs',
    'GodotClient/Scripts/Presentation/M7LookMaterialFactory.cs',
    'GodotClient/Scripts/Presentation/M7WorldLightingEvaluator.cs',
    'GodotClient/Scripts/Presentation/PresentationDestruction.cs',
    'GodotClient/Scripts/Client/M7LookLab.cs',
    'GodotClient/Assets/M7/Textures/regolith_surface_v2.png',
    'GodotClient/Scripts/UI/BasicHud.cs','GodotClient/Scripts/UI/DebugHud.cs','GodotClient/Scripts/UI/M5PlaytestHud.cs',
    'SimCore/Runtime/Scenarios/M5AcceptanceScenarioFactory.cs','Docs/IMPLEMENTATION_REPORT.md',
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
check('RuntimeScenarioLoader.LoadActive' in composition, 'Godot composition root does not use the active runtime scenario loader')
check('FindChild' not in composition and 'GetNode<' not in composition, 'composition root uses runtime dependency discovery')
loader = (ROOT/'GodotClient/Scripts/Client/RuntimeScenarioLoader.cs').read_text()
check('PrototypeContentCodec.Read' in loader and 'CompiledMapCodec.ReadDefinition' in loader, 'Godot runtime does not consume compiled content/map when present')
check('requested ? LoadM5Acceptance() : LoadCanonicalOpening()' in loader, 'normal launch no longer defaults to the canonical opening')
check('M5AcceptanceScenarioFactory.Create' in loader and 'PrepareM5Acceptance' in composition, 'M5 acceptance handoff is not wired to the runtime')
basic_hud = (ROOT/'GodotClient/Scripts/UI/BasicHud.cs').read_text()
hud_view = (ROOT/'GodotClient/Scripts/UI/HudView.cs').read_text()
hud_profile = (ROOT/'GodotClient/Scripts/UI/M7HudProfile.cs').read_text()
check('CurrentSchemaVersion = 4' in hud_profile and 'Clamp(ArtSkin.Faction, 0, 4)' in hud_profile,
      'M7 HUD Profile schema 4 five-faction art domain missing')
hud_minimap = (ROOT/'GodotClient/Scripts/UI/HudMinimapView.cs').read_text()
minimap_source = (ROOT/'GodotClient/Scripts/Presentation/MinimapPresentationSource.cs').read_text()
input_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsInputController.cs').read_text()
camera_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsCameraController.cs').read_text()
for token in ['ResourceStrip','SelectionPanel','PortraitSlot','ContextualEnergyPriority','EnergyDomainPopover','MinimapSlot','CommandPanel','CommandGrid','SelectionTypeGroups','EventFeed']:
    check(token in hud_view, f'T068 retained HUD element missing: {token}')
for token in ['HudFrame','BuildGroupedSelection','BuildCommands','CanQueueProduction','QueueProduction']:
    check(token in basic_hud, f'T068 production HUD binding missing: {token}')
for token in ['SafeAreaPercent { get; set; } = 96f','UiScale','TextScale','CommandPanelWidth','SelectionMaxWidth']:
    check(token in hud_profile, f'T068 responsive HUD profile missing: {token}')
check('CommandCapacity = 12' in hud_view and 'GroupCapacity = 8' in hud_view,
      'T068 retained command/type-card pools are not bounded')
for token in ['HudMinimapView','SetCameraPolygon','PixelToBuildCell','IssueMinimapGroundCommand','GetGroundViewportPolygon']:
    check(token in hud_view + basic_hud + hud_minimap + input_controller + camera_controller,
          f'T069 minimap integration missing: {token}')
for token in ['MultiMeshInstance2D','MarkerCapacity','IsNorthUp => true','CameraRequested','GroundCommandRequested']:
    check(token in hud_minimap, f'T069 bounded north-up minimap view missing: {token}')
for token in ['PresentationSnapshot snapshot','_rememberedStatic','VisibilityState.Explored','CaptureLegalNetworkLines','KnownEnemyNetwork','confirmedVisibleSegments','SurgeZone','CaptureLegalPings']:
    check(token in minimap_source, f'T069 client-legal minimap source missing: {token}')
check('HudMinimapPlaceholder' not in hud_view, 'T069 placeholder minimap remains in the production HUD')
debug_hud = (ROOT/'GodotClient/Scripts/UI/DebugHud.cs').read_text()
check('Visible = false' in debug_hud and 'Drain Energy' in debug_hud, 'developer tools must remain available but hidden by default')
debug_renderer = (ROOT/'GodotClient/Scripts/Presentation/DebugRenderer.cs').read_text()
for token in ['DrawNavigation { get; set; } = true','DrawClusters { get; set; } = true','DrawPaths { get; set; } = true','DrawExcavatable { get; set; } = true']:
    check(token not in debug_renderer, f'developer visualization leaks into normal play: {token}')
check('OrderNumber' not in input_controller and 'DestinationRing' in input_controller, 'move feedback must use an unnumbered restrained destination marker')
check('IssueMinimapGroundCommand' in input_controller and 'CommandModifiers.Queue' in input_controller,
      'T069 minimap move/queued-move command binding is missing')
unit_view = (ROOT/'GodotClient/Scripts/Presentation/UnitViewManager.cs').read_text()
check('ConstructionProgressLabel' not in unit_view and 'ConstructionProgressBar' in unit_view, 'construction progress must use a restrained world bar rather than a fixed-size billboard label')
animation_driver = (ROOT/'GodotClient/Scripts/Presentation/PresentationAnimationDriver.cs').read_text()
for token in ['static PresentationAnimationInput FromSnapshots','NormalInterval = 1f / 30f','DistantInterval = 1f / 15f','PresentationAnimationRigBinding']:
    check(token in animation_driver, f'T065 animation presentation foundation missing: {token}')
vfx_pool = (ROOT/'GodotClient/Scripts/Presentation/PresentationVfxPool.cs').read_text()
for token in ['PresentationEventDeduplicator','PresentationVfxPoolStats','TryAcquire','_dropped++','IPooledPresentationVfx']:
    check(token in vfx_pool, f'T066 pooled VFX foundation missing: {token}')
check('QueueFree()' not in vfx_pool, 'T066 pooled VFX nodes must be reused rather than freed per effect')
look_profile = (ROOT/'GodotClient/Scripts/Presentation/M7LookProfile.cs').read_text()
check('CurrentSchemaVersion = 6' in look_profile and 'AnimationLook Animation' in look_profile and
      'DestructionLook Destruction' in look_profile and 'VfxPoolLook VfxPool' in look_profile and
      'WorldCycleLook WorldCycle' in look_profile and 'SignalPulseAmount' in look_profile and
      'LampPulseAmount' in look_profile and 'CrystalPulseAmount' in look_profile and
      'ReliefStrength' in look_profile and 'TextureBlendMode' in look_profile and
      'InspectionPass' in look_profile and 'ApplySchemaSixMigration' in look_profile,
      'M7 Look Profile schema 6 texture/emission/world-cycle controls missing')
look_materials = (ROOT/'GodotClient/Scripts/Presentation/M7LookMaterialFactory.cs').read_text()
for token in ['regolith_surface_v2.png','varying vec3 view_position','varying vec3 local_normal',
              'ground_relief_normal','inspection_pass','anti_tiled_albedo','anti_tiled_height',
              'AuthoredOpaque','OpaqueEmissiveShader','RegolithTexturePath = GroundAlbedoTexturePath']:
    check(token in look_materials, f'M7 corrected material inspection pipeline missing: {token}')
check('textureLod(detail_texture' not in look_materials and 'textureLod(albedo_texture' not in look_materials,
      'M7 material pipeline must use derivative-aware texture sampling at RTS zoom')
world_lighting = (ROOT/'GodotClient/Scripts/Presentation/M7WorldLightingEvaluator.cs').read_text()
for token in ['signed solar altitude','M7WorldEnvironment.Underground','NightReadability is a floor/boost',
              'ValidateDeterministic','Direct sun remained active below the horizon',
              'FunctionalLightFactor','BlueHourAltitude = -18f','NeutralDayAltitude = 24f',
              'resolvedShadowFactor']:
    check(token in world_lighting, f'M7 deterministic world-light evaluator missing: {token}')
look_lab = (ROOT/'GodotClient/Scripts/Client/M7LookLab.cs').read_text()
for token in ['BuildAnimationSettings','PresentationAnimationDriver','BuildDestructionSettings',
              'PresentationDestructionDriver','PresentationVfxPool<PooledLegoDebrisBurst>',
              'PresentationVfxPool<PooledTracerEffect>','UpdatePoolStatsLabel','ValidatePoolReuse',
              'BuildWorldCycleSettings','M7WorldLightingEvaluator.Evaluate','GlareTexturePath',
              'PresentationVfxPool<PooledExplosionBurst>','--m7-look-profile',
              '--m7-look-material-view','--m7-look-material-audit','FormatCaptureFloat',
              'DirectionalLight3D.ShadowMode.Parallel4Splits','_worldFunctionalLightFactor',
              'float broadRelief']:
    check(token in look_lab, f'M7 Look Lab T065/T066/T067 integration missing: {token}')
check('Start from surface character' not in look_lab and 'Physical surface depth' not in look_lab,
      'M7 normal Materials UI must remain role-authored rather than exposing low-level shader controls')
destruction_presentation = (ROOT/'GodotClient/Scripts/Presentation/PresentationDestruction.cs').read_text()
for token in ['PresentationDestructionScaleBand','PresentationDestructionDriver','MaxFragments = 18',
              'MultiMeshInstance3D','LegoDebrisBurstRequest','PooledExplosionBurst',
              'FireLobeCount','HotFragmentCount','SmokePuffCount','ValidateSmoke']:
    check(token in destruction_presentation, f'T067 bounded LEGO destruction foundation missing: {token}')
check('RigidBody3D' not in destruction_presentation and 'CollisionShape3D' not in destruction_presentation,
      'T067 cosmetic debris must not introduce physical bodies or colliders')

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
check(source.get('schemaVersion') == 4, 'map source schema version mismatch')
check(source.get('stableId') == 'map.dev_first_controllable_rts', 'map stable ID mismatch')
check(source.get('buildSize') == [160,160] and source.get('navScale') == 2, 'map source dimensions mismatch')
check(len(source.get('starts',[])) >= 2, 'map starts missing')
check(len(source.get('flagRects',[])) >= 10, 'authored obstacle/pathing data missing')
check(len(source.get('elevationRects',[])) >= 2, 'elevation test data missing')
check(len(source.get('excavatableFeatures',[])) == 1, 'expected one M2 Excavatable feature')
excavatable=source.get('excavatableFeatures',[{}])[0] if source.get('excavatableFeatures') else {}
check(excavatable.get('stableId') == 'feature.dev.fractured_shortcut', 'T050 Excavatable stable ID mismatch')
check(excavatable.get('class') == 'FracturedRockWall' and excavatable.get('requiredEnergy') == 25, 'T050 Excavatable class/Energy metadata mismatch')
check(excavatable.get('initialState') == 'Blocked' and excavatable.get('openBuildable') is False, 'T050 Excavatable topology metadata mismatch')
check(len(source.get('initialEntities',[])) >= 26, 'initial prototype entity spawns missing')
check(len(source.get('resourceNodes',[])) == 4, 'M3 starting Ore node spawns missing')
check(len(source.get('resourceReceivers',[])) == 2, 'M3 starting HQ resource receivers missing')
check(len(source.get('visionTestGeometry',[])) >= 4, 'vision test geometry missing')

content_source = json.loads((ROOT/'Content/PrototypeEntities.json').read_text())
check(content_source.get('schemaVersion') == 15 and content_source.get('contentKind') == 'prototype_entities', 'prototype content schema mismatch')
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
expected_durability={
    'unit.rock_raiders.crew':(110,0),
    'unit.rock_raiders.hover_scout':(120,0),
    'unit.rock_raiders.rapid_rider':(170,0),
    'unit.rock_raiders.loader_dozer':(360,2),
    'unit.rock_raiders.chrome_crusher':(880,5),
    'building.rock_raiders.hq':(3000,5),
    'building.rock_raiders.ore_processing_plant':(1350,2),
    'building.rock_raiders.power_station':(1000,1),
    'building.rock_raiders.vehicle_service_bay':(1700,3),
}
check({key:(by_key.get(key,{}).get('combatTarget',{}).get('hitPoints'),by_key.get(key,{}).get('combatTarget',{}).get('armorRating')) for key in expected_durability} == expected_durability, 'canonical T043 hit points or Armor Ratings missing')
expected_weapon_profiles={
    'unit.rock_raiders.crew':'weapon.rr.crew.portable_mining_tool',
    'unit.rock_raiders.hover_scout':'weapon.rr.hover_scout.survey_pulse',
    'unit.rock_raiders.loader_dozer':'weapon.rr.loader_dozer.scoop_ram',
    'unit.rock_raiders.chrome_crusher':'weapon.rr.chrome_crusher.chrome_drill',
}
check({key:by_key.get(key,{}).get('weaponProfile') for key in expected_weapon_profiles} == expected_weapon_profiles, 'canonical M4 Rock Raider weapon-profile references missing')
check('weaponProfile' not in by_key.get('unit.rock_raiders.rapid_rider',{}), 'Rapid Rider is an unarmed transport and must not acquire attack targets')
weapon_by_key={w.get('stableId'):w for w in content_source.get('weaponDefinitions',[])}
expected_weapons={
    'weapon.rr.crew.portable_mining_tool':(6,'General',24,[4,5],'Contact',[0,1],'Support',45,3500),
    'weapon.rr.hover_scout.survey_pulse':(6,'General',30,[3,1],'Projectile',[12,1],'Scout',180,10000),
    'weapon.rr.loader_dozer.scoop_ram':(18,'General',27,[9,10],'Contact',[0,1],'AntiLight',45,3500),
    'weapon.rr.chrome_crusher.chrome_drill':(55,'Siege',32,[21,20],'Contact',[0,1],'Siege',30,3500),
}
check({key:(weapon_by_key.get(key,{}).get('damage'),weapon_by_key.get(key,{}).get('damageType'),weapon_by_key.get(key,{}).get('cooldownTicks'),weapon_by_key.get(key,{}).get('rangeRatio'),weapon_by_key.get(key,{}).get('delivery'),weapon_by_key.get(key,{}).get('projectileSpeedRatio'),weapon_by_key.get(key,{}).get('priorityProfile'),weapon_by_key.get(key,{}).get('facingToleranceDegrees'),weapon_by_key.get(key,{}).get('maximumMovingFireSpeedBasisPoints')) for key in expected_weapons} == expected_weapons, 'canonical T042/T044 Rock Raider weapon/contact definitions missing or incorrect')
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
expected_worksite_radii = {
    'building.rock_raiders.hq': 18,
    'building.rock_raiders.vehicle_service_bay': 12,
}
check({key:building_by_key.get(key,{}).get('worksiteServiceRadius') for key in expected_worksite_radii} == expected_worksite_radii, 'canonical M5 Worksite service radii missing or incorrect')

forward_service = (ROOT/'SimCore/Runtime/Simulation/ForwardServiceSystem.cs').read_text()
for token in ['building.ast.service_refit_hub','unit.ast.solar_explorer','unit.ast.t3_trike','ServiceHubRadius = 18','DeployedSolarExplorerRadius = 10','ProviderBucketBuildCells = 4']:
    check(token in forward_service, f'canonical T051 Forward Service contract missing: {token}')
check('DeploymentState.Deployed' in forward_service and 'BrownoutSystem.IsOperational' in forward_service, 'T051 provider activation rules missing')
check('world.Entities.Ownership' in forward_service and 'Contains(' in forward_service, 'T051 owner/radius membership validation missing')

mission_refit = (ROOT/'SimCore/Runtime/Simulation/MissionRefitSystem.cs').read_text()
for token in ['FirstSurveyInstallOre = 25','FirstSurveyInstallEnergy = 10','18 * EnergyDomainSystem.TicksPerSecond','LaterSwapOre = 8','LaterSwapEnergy = 5','10 * EnergyDomainSystem.TicksPerSecond','20 * EnergyDomainSystem.TicksPerSecond','unit.ast.t3_trike']:
    check(token in mission_refit, f'canonical T052 Mission Refit contract missing: {token}')
check('ForwardServiceSystem.TryGetProviderForMember' in mission_refit and 'world.Entities.MissionRefitJob.Set' in mission_refit, 'T052 service validation or authoritative job missing')
check('state.CurrentConfiguration = job.NewConfiguration' in mission_refit and 'MissionRefitJob.Remove' in mission_refit, 'T052 identity-preserving completion missing')

resonance = (ROOT/'SimCore/Runtime/Simulation/ResonanceCoreSystem.cs').read_text()
for token in ['BaselineSlots = 4','ExpandedSlots = 6','8 * EnergyDomainSystem.TicksPerSecond','15 * EnergyDomainSystem.TicksPerSecond','BaseEnergyDemandPerSecond = 3','EnergyDemandPerInstalledCrystal = 2','building.ali.resonance_core','building.ali.etx_command_core']:
    check(token in resonance, f'canonical T053 Resonance Core contract missing: {token}')
check('TrySetDesiredCommitment' in resonance and 'DesiredCommittedCrystals' in resonance and 'TryStartNextTransition' in resonance, 'T053 desired-count sequential commitment missing')
check('ResourceType.Crystal' in resonance and 'CommittedSlotMask' in resonance and 'EnergyDomainSystem.Recalculate' in resonance, 'T053 Crystal ownership/slot/Energy integration missing')

alien_charge = (ROOT/'SimCore/Runtime/Simulation/AlienChargeSystem.cs').read_text()
for token in ['MillichargePerCharge = 1000','BaseCoreCapacityMillicharge = 20 * MillichargePerCharge','CapacityPerCrystalMillicharge = 20 * MillichargePerCharge','GenerationPerCrystalMillichargePerSecond = 400','SurgeCostMillicharge = 50 * MillichargePerCharge','SurgeBuildupTicks = 15','SurgeActiveTicks = 18 * EnergyDomainSystem.TicksPerSecond','ResonanceCoreSurgeRadius = 12','MothershipRelaySurgeRadius = 10']:
    check(token in alien_charge, f'canonical T054 Charge/Surge contract missing: {token}')
check('BrownoutSystem.IsOperational' in alien_charge and 'CurrentMillicharge' in alien_charge and 'MaximumMillicharge' in alien_charge, 'T054 powered Charge generation or authoritative millicharge state missing')
check('ApplySurgedCooldownTicks' in alien_charge and '(baseTicks * 4 + 4) / 5' in alien_charge and 'ApplySurgedReconfigurationTicks' in alien_charge and '(baseTicks * 7 + 9) / 10' in alien_charge, 'T054 canonical Surge timing multipliers missing')
check('ResonanceInitiationUnlocked' in alien_charge and 'world.Entities.SurgeZone.Set' in alien_charge and 'receiver.ActiveZone' in alien_charge, 'T054 proof unlock, zone, or deterministic membership missing')

tube_graph = (ROOT/'SimCore/Runtime/Simulation/TubeGraphSystem.cs').read_text()
for token in ['SettlementStationConnectionLimit = 3','AeroTubeHangarConnectionLimit = 5','RedundantRoutingBonusConnections = 1','ActiveLinkEnergyDemandPerSecond = 1','LinkBaseOreCost = 50','LinkOreCostPerBuildCell = 2','LinkActivationEnergyCost = 10','10 * EnergyDomainSystem.TicksPerSecond','LinkConstructionTicksPerBuildCell = 8','building.mar.aero_tube_hangar','building.mar.settlement_station']:
    check(token in tube_graph, f'canonical T055 Tube graph contract missing: {token}')
check('Queue<uint>' in tube_graph and 'ComponentRoot' in tube_graph and 'TubeSegmentationRevision' in tube_graph, 'T055 deterministic BFS/component segmentation missing')
check('TryBuildAutomaticRoute' in tube_graph and 'RouteIsStructurallyValid' in tube_graph and 'TryAddCompletedLink' in tube_graph, 'T055 automatic ordered Tube route creation missing')
check('TrySpendConnectedProcessedResource' in tube_graph and 'GetConnectedProcessedResourceTotal' in tube_graph, 'T055 connected resource-pool access missing')

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
