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
    'SimCore/Runtime/Content/CanonicalActionDefinitions.cs','SimCore/Runtime/Content/CommandDefinition.cs',
    'SimCore/Runtime/Content/CanonicalRosterValidator.cs',
    'SimCore/Runtime/Serialization/SnapshotSerializer.cs','SimCore/Runtime/Replay/ReplayLog.cs',
    'SimCore/Runtime/Navigation/HierarchicalPathfinder.cs','SimCore/Runtime/Simulation/SimulationRunner.cs',
    'HeadlessSim/Program.cs','Content/PrototypeEntities.json','Content/Maps/DEV_FirstControllableRTS.map.json',
    'GodotClient/project.godot','GodotClient/LEGO.SpaceRTS.Godot.csproj','GodotClient/LEGO.SpaceRTS.Godot.sln',
    'GodotClient/Scenes/Bootstrap.tscn','GodotClient/Scenes/PrototypeRTS.tscn',
    'GodotClient/Scripts/Client/RtsCompositionRoot.cs','GodotClient/Scripts/Client/RuntimeScenarioLoader.cs',
    'GodotClient/Scripts/Client/AutomatedSmokeExit.cs',
    'GodotClient/Scripts/Presentation/GodotSimBridge.cs','GodotClient/Scripts/Presentation/RtsCameraController.cs',
    'GodotClient/Scripts/Presentation/SelectionController.cs','GodotClient/Scripts/Presentation/RtsInputController.cs',
    'GodotClient/Scripts/Presentation/FogPresenter.cs','GodotClient/Scripts/Presentation/DebugRenderer.cs',
    'GodotClient/Scripts/Presentation/UnitViewManager.cs','GodotClient/Scripts/Presentation/PresentationAnimationDriver.cs',
    'GodotClient/Scripts/Presentation/PresentationVfxPool.cs','GodotClient/Scripts/Presentation/M7LookProfile.cs',
    'GodotClient/Scripts/Presentation/M7LookMaterialFactory.cs',
    'GodotClient/Scripts/Presentation/M7WorldLightingEvaluator.cs',
    'GodotClient/Scripts/Presentation/PresentationDestruction.cs',
    'GodotClient/Scripts/Presentation/M85AssetPipelineContract.cs',
    'GodotClient/Scripts/Client/M7LookLab.cs',
    'GodotClient/Scripts/Client/M85AssetPipelineLab.cs',
    'GodotClient/Assets/M85/PipelineReference/pipeline_reference_vehicle.glb',
    'GodotClient/Assets/M85/PipelineReference/pipeline_reference_vehicle.glb.import',
    'ArtSource/M85/PipelineReference/pipeline_reference_vehicle.blend',
    'Content/Presentation/Assets/pipeline.reference.vehicle.asset.json',
    'Docs/Development/M85_ASSET_PIPELINE.md',
    'tools/Validation/validate_m85_asset_pipeline.py',
    'Content/Presentation/SuperScout/roster_identity_baseline.json',
    'Content/Presentation/SuperScout/source_ledger.json',
    'Content/Presentation/SuperScout/source_instruction_index.json',
    'Content/Presentation/SuperScout/source_analysis_policy.json',
    'Content/Presentation/SuperScout/rock_raiders_source_evidence.json',
    'Content/Presentation/SuperScout/astronauts_source_evidence.json',
    'Content/Presentation/SuperScout/aliens_source_evidence.json',
    'Content/Presentation/SuperScout/martians_source_evidence.json',
    'Content/Presentation/SuperScout/rock_raiders_production_contracts.json',
    'Content/Presentation/SuperScout/astronauts_production_contracts.json',
    'Content/Presentation/SuperScout/aliens_production_contracts.json',
    'Content/Presentation/SuperScout/martians_production_contracts.json',
    'Content/Presentation/SuperScout/confusion_register.json',
    'Docs/Development/M85SuperScout/PACKET_INDEX.md',
    'Docs/Development/M85SuperScout/Matrices/identity_source_matrix.csv',
    'Docs/Development/M85SuperScout/Matrices/confusion_register.md',
    'Docs/Development/M85SuperScout/Matrices/source_instruction_index.csv',
    'Docs/Development/M85SuperScout/Matrices/rock_raiders_source_audit.md',
    'Docs/Development/M85SuperScout/Matrices/astronauts_source_audit.md',
    'Docs/Development/M85SuperScout/Matrices/aliens_source_audit.md',
    'Docs/Development/M85SuperScout/Matrices/martians_source_audit.md',
    'Docs/Development/M85SuperScout/Matrices/rock_raiders_semantic_construction.md',
    'Docs/Development/M85SuperScout/Matrices/rock_raiders_motion_socket.md',
    'Docs/Development/M85SuperScout/Matrices/rock_raiders_material_texture.md',
    'tools/Validation/validate_m85_super_scout.py',
    'tools/generate-m85-super-scout-packets.py',
    'tools/blender/generate_m85_pipeline_reference.py','tools/generate-m85-pipeline-reference.sh',
    'GodotClient/Assets/M7/Textures/regolith_surface_v2.png',
    'GodotClient/Assets/M7/Textures/painted_shell_macro_v2.png',
    'GodotClient/Assets/M7/Textures/painted_shell_macro_v2.png.import',
    'GodotClient/Scripts/UI/BasicHud.cs','GodotClient/Scripts/UI/DebugHud.cs','GodotClient/Scripts/UI/M5PlaytestHud.cs',
    'GodotClient/Scripts/UI/HudPortraitView.cs','GodotClient/Scripts/UI/M7HudProfile.cs',
    'GodotClient/Scripts/UI/HudFactionSkinLibrary.cs','GodotClient/Scripts/UI/HudFactionChrome.cs',
    'GodotClient/Scripts/UI/HudFactionSurfaceMask.cs',
    'GodotClient/Scripts/UI/HudTypographyLibrary.cs','GodotClient/Scripts/UI/HudTypographyLibrary.cs.uid',
    'GodotClient/Scripts/UI/HudTextPalette.cs','GodotClient/Scripts/UI/HudTextPalette.cs.uid',
    'GodotClient/Assets/M7/Fonts/Oxanium-SemiBold.ttf',
    'GodotClient/Assets/M7/Fonts/Oxanium-SemiBold.ttf.import',
    'GodotClient/Assets/M7/Fonts/IBMPlexSans-Regular.ttf',
    'GodotClient/Assets/M7/Fonts/IBMPlexSans-Regular.ttf.import',
    'GodotClient/Assets/M7/Fonts/IBMPlexSans-Medium.ttf',
    'GodotClient/Assets/M7/Fonts/IBMPlexSans-Medium.ttf.import',
    'GodotClient/Assets/M7/Fonts/Oxanium-OFL.txt','GodotClient/Assets/M7/Fonts/IBM-Plex-OFL.txt',
    'GodotClient/Assets/M7/Fonts/README.md',
    'GodotClient/Shaders/hud_faction_aperture.gdshader',
    'GodotClient/Assets/M7/Hud/rock_raiders_frame.png','GodotClient/Assets/M7/Hud/astronauts_frame.png',
    'GodotClient/Assets/M7/Hud/aliens_frame.png','GodotClient/Assets/M7/Hud/martians_frame.png',
    'GodotClient/Assets/M7/Hud/astronauts_unified_frame_v2.png','GodotClient/Assets/M7/Hud/aliens_frame_v2.png',
    'GodotClient/Assets/M7/Hud/astronauts_unified_frame_v2.png.import',
    'GodotClient/Assets/M7/Hud/aliens_frame_v2.png.import',
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
automated_exit = (ROOT/'GodotClient/Scripts/Client/AutomatedSmokeExit.cs').read_text()
check('NativeImmediateExit(exitCode)' in automated_exit and
      'engineArguments.Contains("--headless")' in automated_exit and
      'context.GetTree().Quit(exitCode)' in automated_exit,
      'macOS smoke exit no longer separates disposable automation from normal game shutdown')
verify_script = (ROOT/'tools/verify.sh').read_text()
build_mac_script = (ROOT/'tools/build-mac.sh').read_text()
check('--headless --disable-crash-handler' in verify_script and
      '--headless --quit-after' not in verify_script and
      '--headless --log-file' not in verify_script and
      verify_script.count('--automated-smoke-immediate-exit') >= 16,
      'verification contains an intrusive or crash-handler-enabled Godot smoke launch')
check('--headless --disable-crash-handler' in build_mac_script and
      '--headless --log-file' not in build_mac_script and
      build_mac_script.count('--automated-smoke-immediate-exit') >= 3,
      'macOS build verification contains a crash-handler-enabled app smoke launch')
smoke_exit_files = [
    'GodotClient/Scripts/Client/GodotSmokeRunner.cs',
    'GodotClient/Scripts/Client/M7MaterialLab.cs',
    'GodotClient/Scripts/Client/M7VisualStyleLab.cs',
    'GodotClient/Scripts/Client/M7PaletteRatioLab.cs',
    'GodotClient/Scripts/Client/M7LookLab.cs',
    'GodotClient/Scripts/Client/M7HudLab.cs',
    'GodotClient/Scripts/Client/M7VisualAcceptanceCandidate.cs',
    'GodotClient/Scripts/Client/M85AssetPipelineLab.cs',
    'GodotClient/Scripts/Networking/M6TransportSmokeRunner.cs',
    'GodotClient/Scripts/Networking/M6CommandAuthoritySmokeRunner.cs',
    'GodotClient/Scripts/Networking/M6SnapshotSmokeRunner.cs',
    'GodotClient/Scripts/Networking/M6ReconnectSmokeRunner.cs',
    'GodotClient/Scripts/Networking/M6ReplaySmokeRunner.cs',
]
for smoke_exit_file in smoke_exit_files:
    smoke_exit_text = (ROOT/smoke_exit_file).read_text()
    check('AutomatedSmokeExit.Finish' in smoke_exit_text and 'GetTree().Quit' not in smoke_exit_text,
          f'automated Godot fixture bypasses the crash-safe exit: {smoke_exit_file}')
content_bin=require('GodotClient/Compiled/PrototypeEntities.contentbin')
map_bin=require('GodotClient/Compiled/DEV_FirstControllableRTS.mapbin')
if content_bin.exists():
    data=content_bin.read_bytes()
    check(len(data)>=8 and data[:4]==bytes.fromhex('4c535043'), 'compiled content magic/size mismatch')
    check(len(data)>=8 and int.from_bytes(data[4:8], 'little', signed=True)==19, 'compiled content format version is not 19')
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
check('--m85-asset-pipeline' in composition and 'M85AssetPipelineLab' in composition and
      'OpenM85AssetPipelineLab' in composition and 'ReturnFromM85AssetPipelineLab' in composition,
      'T081 asset-pipeline review fixture is not wired to the runtime host')
loader = (ROOT/'GodotClient/Scripts/Client/RuntimeScenarioLoader.cs').read_text()
check('PrototypeContentCodec.Read' in loader and 'CompiledMapCodec.ReadDefinition' in loader, 'Godot runtime does not consume compiled content/map when present')
check('requested ? LoadM5Acceptance() : LoadCanonicalOpening()' in loader, 'normal launch no longer defaults to the canonical opening')
check('M5AcceptanceScenarioFactory.Create' in loader and 'PrepareM5Acceptance' in composition, 'M5 acceptance handoff is not wired to the runtime')
basic_hud = (ROOT/'GodotClient/Scripts/UI/BasicHud.cs').read_text()
hud_view = (ROOT/'GodotClient/Scripts/UI/HudView.cs').read_text()
hud_lab = (ROOT/'GodotClient/Scripts/Client/M7HudLab.cs').read_text()
hud_portrait = (ROOT/'GodotClient/Scripts/UI/HudPortraitView.cs').read_text()
hud_chrome = (ROOT/'GodotClient/Scripts/UI/HudFactionChrome.cs').read_text()
hud_surface_mask = (ROOT/'GodotClient/Scripts/UI/HudFactionSurfaceMask.cs').read_text()
hud_aperture_shader = (ROOT/'GodotClient/Shaders/hud_faction_aperture.gdshader').read_text()
hud_structural = (ROOT/'GodotClient/Scripts/UI/HudStructuralChrome.cs').read_text()
hud_raster_surface = (ROOT/'GodotClient/Scripts/UI/HudRasterSurfaceOverlay.cs').read_text()
hud_profile = (ROOT/'GodotClient/Scripts/UI/M7HudProfile.cs').read_text()
hud_faction_skins = (ROOT/'GodotClient/Scripts/UI/HudFactionSkinLibrary.cs').read_text()
hud_typography = (ROOT/'GodotClient/Scripts/UI/HudTypographyLibrary.cs').read_text()
hud_text_palette = (ROOT/'GodotClient/Scripts/UI/HudTextPalette.cs').read_text()
check('CurrentSchemaVersion = 8' in hud_profile and
      'Clamp(ArtSkin.Faction, 0, HudFactionSkinLibrary.Count - 1)' in hud_profile and
      'HudArtFinish Finish' in hud_profile and 'HybridConsole' in hud_profile and
      'HudSurfacePalette SurfacePalette' in hud_profile and 'FactionBound' in hud_profile and
      'HudSurfacePaletteLibrary' in hud_profile and 'HudFactionSkinLibrary.Apply' in hud_profile,
      'M7 HUD Profile schema 8 faction-bound skin domain missing')
for token in ['sourceSchemaVersion <= 7','parsed.ArtSkin.Faction switch','3 => 1','4 => 3',
              'parsed.ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound',
              'parsed.ArtSkin.Finish = HudArtFinish.LegacyFrames']:
    check(token in hud_profile, f'M7 HUD schema-8/Legacy migration coverage missing: {token}')
for faction in ['RockRaiders','Astronauts','Aliens','Martians']:
    check(hud_faction_skins.count(f'HudFaction.{faction}') == 1,
          f'M7 HUD must define exactly one complete skin recipe for {faction}')
for token in ['Count = 4','HudFactionSkinRecipe','HybridFramePath','LegacyFramePath',
              'ApertureInsetRatios','DestinationScale',
              'HudFactionRailStyle.Industrial','HudFactionRailStyle.Expedition',
              'HudFactionRailStyle.Resonance','HudFactionRailStyle.Pneumatic',
              'SharedGood','SharedWarning','SharedDanger','Validate(out string error)',
              'profile.ArtSkin.Faction = (int)recipe.Faction',
              'profile.ArtSkin.SurfacePalette = HudSurfacePalette.FactionBound',
              'profile.Colors.Background = recipe.Background','profile.Colors.Accent = recipe.Accent',
              'profile.Colors.Selection = recipe.Selection',
              'astronauts_unified_frame_v2.png','aliens_frame_v2.png']:
    check(token in hud_faction_skins, f'M7 four-faction HUD recipe library missing: {token}')
check('life_on_mars_astronauts_frame.png' not in hud_faction_skins,
      'retired Life on Mars astronaut sub-skin remains a fifth faction recipe')
alien_recipe = hud_faction_skins.split('HudFaction.Aliens', 1)[-1].split('HudFaction.Martians', 1)[0]
check('7754a6' not in alien_recipe.lower() and 'purple' not in alien_recipe.lower() and 'violet' not in alien_recipe.lower(),
      'Alien HUD recipe reintroduced the rejected purple/violet palette')
for frame_import in ['rock_raiders_frame.png.import','astronauts_frame.png.import',
                     'aliens_frame.png.import','martians_frame.png.import',
                     'astronauts_unified_frame_v2.png.import','aliens_frame_v2.png.import']:
    import_path = ROOT/'GodotClient/Assets/M7/Hud'/frame_import
    if import_path.exists():
        check('mipmaps/generate=true' in import_path.read_text(),
              f'M7 HUD frame must import with RTS-zoom mipmaps: {frame_import}')
hud_minimap = (ROOT/'GodotClient/Scripts/UI/HudMinimapView.cs').read_text()
minimap_source = (ROOT/'GodotClient/Scripts/Presentation/MinimapPresentationSource.cs').read_text()
input_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsInputController.cs').read_text()
camera_controller = (ROOT/'GodotClient/Scripts/Presentation/RtsCameraController.cs').read_text()
for token in ['ResourceStrip','BottomDeck','SelectionPanel','PortraitSlot','TacticalPortrait',
              'ContextualEnergyPriority','EnergyDomainPopover','MinimapSlot','CommandPanel',
              'CommandGrid','SelectionTypeGroups','EventFeed']:
    check(token in hud_view, f'T068 retained HUD element missing: {token}')
for token in ['HudPortraitView','DrawVehicle','DrawGroup','DrawStructure','DrawTransformation','ConciseCaption']:
    check(token in hud_portrait, f'M7 code-native tactical portrait missing: {token}')
for token in ['HudFactionChromeRole.BottomDeck','UsesSparseJunctionModules','UsesTiledEdgeWalls',
              'UsesFactionSurfaceFill','DrawTiledEdgeWalls','DrawHorizontalTiles','DrawVerticalTiles',
              'DrawLegacySurfaceFill','DrawLegacyRails','DrawFunctionalModules','SetJunctions']:
    check(token in hud_view + hud_chrome, f'M7 continuous faction control-deck chrome missing: {token}')
for token in ['UsesContinuousHybridRails => false',
              'UsesCompleteHybridPerimeter','UsesIsotropicRasterModules','AvoidsFullSpanRasterStretch',
              'DestinationCornerSize','DrawTiledEdgeWalls(outer, corner, modulate)',
              'UsesTiledEdgeWalls => true','UsesSparseJunctionModules => !_frameOnly',
              'UsesFactionSurfaceFill => !_frameOnly','ClipContents = true']:
    check(token in hud_chrome, f'M7 clean hybrid/retained Legacy composition missing: {token}')
for rejected in ['DrawContinuousHybridRails','DrawAuthoredEdgeModules']:
    check(rejected not in hud_chrome,
          f'M7 hybrid still contains the interrupted flat-rail composition: {rejected}')
check('256f * recipe.ProtectedFraction' not in hud_chrome,
      'M7 frame source guides still assume every faction texture is 256px')
for token in ['HudFactionSurfaceMask','TransparentThreshold','GetOrCreateMask',
              'ClipChildrenMode.Disabled','RegisterMaskedSurface','UsesFactionApertureMask',
              'UsesShaderApertureMask','ClipsRasterAndContent','UsesShapedApertureCorners',
              'HasTransparentOuterCorners','MaskCornersAreTransparent',
              'HasTransparentOuterBorder','MaskOuterBorderIsTransparent','ApertureCoverage',
              'UsesFullBleedBottomDeck','RegisteredMaskedSurfaceCount',
              'UsesSharedNineSliceGeometry','ContentRectFor','InteractiveRectFor',
              'DrawNineSlice','authored-aperture-v2',
              'source[center * 4 + 3]','touchesExterior','ApertureInsetRatios',
              'HudArtFinish.LegacyFrames','_recipe.LegacyFramePath','_recipe.HybridFramePath']:
    check(token in hud_surface_mask + hud_view,
          f'M7 faction-specific aperture mask missing: {token}')
check('_gutterMaskTexture' not in hud_surface_mask,
      'M7 rounded aperture still restores an opaque square corner gutter')
for rejected in ['InsideRoundedRect','cornerRadiusFraction','GenerateMipmaps','int overlap =']:
    check(rejected not in hud_surface_mask,
          f'M7 authored aperture reintroduced synthetic clipping/dilation: {rejected}')
check('ApertureCornerRadiusFraction' not in hud_faction_skins + hud_surface_mask,
      'M7 faction recipe still exposes the retired synthetic aperture radius')
for token in ['shader_type canvas_item','aperture_mask','mask_screen_rect','mask_source_corner',
              'mask_destination_corner','SCREEN_UV','source_axis','discard']:
    check(token in hud_aperture_shader, f'M7 faction aperture shader missing: {token}')
for token in ['Name = $"{name}FactionSurfaceMask"','DirectSurfaceMask',
              'Style(Colors.Transparent, Colors.Transparent, 0, true)',
              'ZIndex = 20']:
    check(token in hud_view, f'M7 masked chassis layer ordering missing: {token}')
for token in ['ConsoleSurfaceTexturePath','UsesTiledConsoleSurface','UsesSculptedShoulders',
              'UsesInteriorOnlyHybrid','DrawHybridInterior','ProtectedSourceSizeForFaction',
              'CreateChassisShape','DrawRecessedBays','DrawLayeredRails','SetJunctions']:
    check(token in hud_structural + hud_chrome, f'M7 structural RTS console chrome missing: {token}')
for token in ['IsFrameOnly','UsesVectorAccentRails','rasterFrameFinish','hybridFinish']:
    check(token in hud_chrome + hud_view, f'M7 hybrid legacy-frame/vector-interior composition missing: {token}')
check((ROOT/'GodotClient/Assets/M7/Hud/console_surface_v1.png').exists(),
      'M7 structural console raster surface missing')
for token in ['HudRasterSurfaceOverlay','UsesBoundedRasterPlates','UsesUnstretchedSourceRegions',
              'UsesSingleContinuousSurfaceField','UsesSingleDeckWideSurface','CropToAspect',
              'HudRasterSurfaceRole.BottomDeck',
              'Name = $"{name}RasterSurface"']:
    check(token in hud_raster_surface + hud_view, f'M7 hybrid raster fill detail missing: {token}')
check('HudRasterSurfaceRole.Selection' not in hud_raster_surface + hud_view and
      'HudRasterSurfaceRole.Command' not in hud_raster_surface + hud_view,
      'M7 Hybrid still layers competing selection/command raster surface systems')
check('HudFactionChromeRole.SquarePanel' not in hud_view + hud_chrome and
      'HudFactionChromeRole.MainPanel' not in hud_view + hud_chrome,
      'M7 HUD returned to separately framed/stretched inner panels')
for token in ['HudFrame','BuildGroupedSelection','BuildCommands','CanQueueProduction','QueueProduction']:
    check(token in basic_hud, f'T068 production HUD binding missing: {token}')
for token in ['AlertRequested += FocusCurrentAlert','FocusCurrentAlert','_alertFocusEntity']:
    check(token in basic_hud, f'T068 actionable production alert focus missing: {token}')
for token in ['button.AddThemeStyleboxOverride("disabled", transparent)',
              'float scale = ResponsiveScale()',
              'ResponsiveTextScale()',
              'SetBoxSpacing("ResourceRow"',
              'paddingOverride ?? _profile.Surface.InnerPadding',
              '_profile.Surface.SolidCommandButtons',
              'Layout.UiScale * Mathf.Clamp(height / 1080f']:
    check(token in hud_view, f'M7 direct art-director HUD control binding missing: {token}')
for token in ['Oxanium-SemiBold.ttf','IBMPlexSans-Regular.ttf','IBMPlexSans-Medium.ttf',
              'public static Font? Heading','public static Font? Body','public static Font? BodyMedium',
              'Validate(out string error)']:
    check(token in hud_typography, f'M7 curated HUD typography asset binding missing: {token}')
for token in ['HudTextColorSet','TopPrimary','DeckPrimary','SelectionPrimary','RaisedPrimary',
              'CommandPrimary','CommandSurface','CommandHoverSurface','CommandPressedSurface',
              'SelectionSurface','SelectionPlateSurface','CompositeSectionSurface','ContrastRatio',
              'ValidateFactionRecipes','4.5f','3f']:
    check(token in hud_text_palette + hud_view + hud_lab,
          f'M7 semantic readable text palette missing: {token}')
check('AddSlider(box, "Heading"' not in hud_lab and
      'AddSlider(box, "Body"' not in hud_lab and
      'AddSlider(box, "Micro"' not in hud_lab and
      'AddColor(box, "Text"' not in hud_lab and
      'AddColor(box, "Muted"' not in hud_lab,
      'M7 lab still delegates curated baseline typography/text contrast to manual sliders')
for token in ['ResponsiveWidthRatio','EffectiveLayoutScale','EffectiveInnerPadding','ResponsiveFontSize']:
    check(token not in hud_view, f'M7 HUD control is silently capped by viewport width: {token}')
check('ValidateInteractiveBindings' in hud_lab and 'outlineAlpha' in hud_lab and 'alertRequests' in hud_lab,
      'M7 HUD interaction regression gate missing')
for token in ['ValidateFactionSkinModes','HudSurfacePalette.FactionBound',
              'HudFactionSkinLibrary.Apply','HudFactionSkinLibrary.Validate',
              'HudFactionChrome.ValidateRecipes','factionSkins=4',
              'SelectFactionKit','EffectivePreviewFrame','FactionKit{faction}',
              'apertureMasks=4','kitSwitch=interactive','--m7-hud-kit']:
    check(token in hud_lab + hud_profile + hud_faction_skins + hud_chrome,
          f'M7 HUD faction-bound skin validation missing: {token}')
check('factionSkins=5' not in hud_lab, 'M7 HUD lab still reports a retired fifth faction skin')
check('SURFACE FAMILY' not in hud_lab,
      'M7 HUD lab still presents frame and surface family as independent normal-use systems')
for token in ['SafeAreaPercent { get; set; } = 98f','UiScale','TextScale','CommandPanelWidth','SelectionMaxWidth']:
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
check('Review M8.5 asset pipeline' in debug_hud and 'OpenM85AssetPipelineLab' in debug_hud,
      'T081 asset-pipeline review action is missing from F8 developer tools')
asset_pipeline_contract = (ROOT/'GodotClient/Scripts/Presentation/M85AssetPipelineContract.cs').read_text()
asset_pipeline_lab = (ROOT/'GodotClient/Scripts/Client/M85AssetPipelineLab.cs').read_text()
for token in ['WorldUnitsPerBuildCell = 2f','LOD_Close','LOD_Combat','LOD_Strategic',
              'Socket_Selection','Socket_Health','Socket_Weapon_Primary','Pivot_ToolPrimary',
              'closeTriangles != 868','combatTriangles != 332','strategicTriangles != 168',
              'ShowOnlyLod']:
    check(token in asset_pipeline_contract, f'T081 imported-scene contract missing: {token}')
for token in ['M85AssetPipelineContract.RuntimePath','BuildSharedMaterials','SetTextureAnchor',
              '--m85-asset-pipeline-smoke','--m85-asset-pipeline-zoom','--capture-path',
              '--m85-asset-pipeline-yaw','RotateCamera','Q / E or ← / →',
              'Z / X / C: 24 / 44 / 72','NoDepthTest = true','PIPELINE: PASS']:
    check(token in asset_pipeline_lab, f'T081 gameplay-camera round-trip fixture missing: {token}')
check('RigidBody3D' not in asset_pipeline_contract + asset_pipeline_lab and
      'CollisionShape3D' not in asset_pipeline_contract + asset_pipeline_lab,
      'T081 pipeline fixture introduced authoritative-looking physics nodes')
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
check('CurrentSchemaVersion = 9' in look_profile and 'AnimationLook Animation' in look_profile and
      'DestructionLook Destruction' in look_profile and 'VfxPoolLook VfxPool' in look_profile and
      'WorldCycleLook WorldCycle' in look_profile and 'SignalPulseAmount' in look_profile and
      'LampPulseAmount' in look_profile and 'CrystalPulseAmount' in look_profile and
      'ReliefStrength' in look_profile and 'TextureBlendMode' in look_profile and
      'InspectionPass' in look_profile and 'ApplySchemaSixMigration' in look_profile and
      'ApplySchemaSevenMigration' in look_profile and 'ApplySchemaEightMigration' in look_profile and
      'ApplySchemaNineMigration' in look_profile and
      'RasterForward' in look_profile and 'HybridSurface' in look_profile and
      'M7GroundSurfaceTreatment' in look_profile,
      'M7 Look Profile schema 9 texture/emission/world-cycle/ground-treatment controls missing')
look_materials = (ROOT/'GodotClient/Scripts/Presentation/M7LookMaterialFactory.cs').read_text()
paint_macro_import = (ROOT/'GodotClient/Assets/M7/Textures/painted_shell_macro_v2.png.import').read_text()
check('mipmaps/generate=true' in paint_macro_import,
      'M7 painted macro-albedo must import with mipmaps for RTS zoom stability')
for token in ['regolith_surface_v2.png','regolith_height.png','painted_shell_macro_v2.png',
              'varying vec3 view_position','varying vec3 local_normal',
              'ground_relief_normal','inspection_pass','anti_tiled_albedo','anti_tiled_height',
              'micro_scale / 4.0','AuthoredOpaque','OpaqueEmissiveShader',
              'RegolithTexturePath = GroundAlbedoTexturePath',
              'textureGrad(detail_texture','textureGrad(height_texture',
              'detail_filter_width','height_filter_width','groove_darkening',
              'surface_treatment','authored_zone_masks','authored_surface_color',
              'anti_tiled_raster_forward','environment_surface_palette','world_environment',
              'ValidateGroundTreatmentModes',
              'staging_traffic_mask','staging_segment_distance','hybrid_vertex_relief',
              'hybrid_raster_strength','staging_unit_spacing','displacement_gate',
              'textureLod(raster_forward_texture',
              'instance uniform vec3 texture_offset',
              'instance uniform vec3 texture_axis_x',
              'instance uniform vec3 texture_axis_y',
              'instance uniform vec3 texture_axis_z']:
    check(token in look_materials, f'M7 corrected material inspection pipeline missing: {token}')
check('textureLod(detail_texture' not in look_materials and 'textureLod(albedo_texture' not in look_materials,
      'M7 material pipeline must use derivative-aware texture sampling at RTS zoom')
check('detail_filter_width").As<double>() < 12.0' in look_materials,
      'M7 macro-albedo perceptual/filtering regression floor missing')
world_lighting = (ROOT/'GodotClient/Scripts/Presentation/M7WorldLightingEvaluator.cs').read_text()
for token in ['signed solar altitude','M7WorldEnvironment.Underground','NightReadability is a floor/boost',
              'ValidateDeterministic','Direct sun remained active below the horizon',
              'FunctionalLightFactor','BlueHourAltitude = -24f','SunriseAltitude = -5f',
              'GoldenHourStartAltitude = 2f','NeutralDayAltitude = 32f',
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
              'float broadRelief','SetTextureAnchor','ResolveTextureRoot',
              'CaptureTextureBindTransforms','ValidateTextureBindTransforms',
              '--m7-look-ground','AddGroundTreatmentChoices','CaptureGroundMarker',
              'CaptureSurfaceMarker','GroundSurfaceDisplayName','Raster Forward','Hybrid Surface',
              'groundSpacingBinding']:
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
check(excavatable.get('visualProfile') == 'view.placeholder.excavatable.fractured_rock_wall',
      'T074 map feature visual profile is unresolved')
check(len(source.get('initialEntities',[])) >= 26, 'initial prototype entity spawns missing')
check(len(source.get('resourceNodes',[])) == 4, 'M3 starting Ore node spawns missing')
check(len(source.get('resourceReceivers',[])) == 2, 'M3 starting HQ resource receivers missing')
check(len(source.get('visionTestGeometry',[])) >= 4, 'vision test geometry missing')

content_source = json.loads((ROOT/'Content/PrototypeEntities.json').read_text())
check(content_source.get('schemaVersion') == 18 and content_source.get('contentKind') == 'prototype_entities', 'prototype content schema mismatch')
content_catalog_text = (ROOT/'SimCore/Runtime/Content/PrototypeContentCatalog.cs').read_text()
check('public const int FormatVersion = 19;' in content_catalog_text, 'PrototypeContentCodec format version is not 19')
roster_validator_text = (ROOT/'SimCore/Runtime/Content/CanonicalRosterValidator.cs').read_text()
for diagnostic in ['T074_DUPLICATE_ID','T074_MISSING_VISUAL_PROFILE','T074_INVALID_WEAPON','T074_MISSING_PRODUCTION_SOURCE',
                   'T074_ILLEGAL_TUBE_UNIT','T074_BAD_MAP_START','T074_IMPOSSIBLE_BUILDING_EXIT']:
    check(diagnostic in roster_validator_text, f'T074 roster diagnostic missing: {diagnostic}')
content_compiler_text = (ROOT/'tools/ContentCompiler/Program.cs').read_text()
check('CanonicalRosterValidator.Validate(catalog, definition)' in content_compiler_text,
      'content compiler does not run T074 complete roster-reference validation')
entity_keys = [e.get('stableId') for e in content_source.get('entities',[])]
check(len(entity_keys) >= 5 and len(entity_keys) == len(set(entity_keys)), 'prototype content entries missing/duplicated')
unit_entries = [e for e in content_source.get('entities',[]) if e.get('stableId','').startswith('unit.')]
expected_m8_unit_counts = {'RockRaiders':8, 'Astronauts':13, 'Aliens':6, 'Martians':8}
actual_m8_unit_counts = {faction:sum(1 for unit in unit_entries if unit.get('faction') == faction) for faction in expected_m8_unit_counts}
check(len(unit_entries) == 35, 'M8 T070 must contain exactly 35 canonical unit definitions')
check(actual_m8_unit_counts == expected_m8_unit_counts, 'M8 T070 faction unit counts must be RockRaiders=8, Astronauts=13, Aliens=6, Martians=8')
check(all(unit.get('operationsCapacity',0) > 0 and unit.get('combatTarget',{}).get('hitPoints',0) > 0 for unit in unit_entries), 'M8 T070 unit definitions require canonical OC and durability metadata')
for required_key in ['building.rock_raiders.hq','building.rock_raiders.ore_processing_plant','building.rock_raiders.power_station','building.rock_raiders.vehicle_service_bay','unit.rock_raiders.crew','unit.rock_raiders.hover_scout','unit.rock_raiders.rapid_rider','unit.rock_raiders.loader_dozer','unit.rock_raiders.chrome_crusher','prototype.nav.huge']:
    check(required_key in entity_keys, f'prototype content key missing: {required_key}')
by_key={e.get('stableId'):e for e in content_source.get('entities',[])}
for key,entity in by_key.items():
    if isinstance(key,str) and (key.startswith('unit.') or key.startswith('building.')):
        expected_view='view.placeholder.' + key.split('.',1)[1]
        check(entity.get('viewProfile')==expected_view, f'unresolved canonical entity visual profile: {key}')
check(by_key.get('prototype.nav.huge',{}).get('viewProfile')=='view.placeholder.navigation.huge',
      'engineering Huge profile visual reference is unresolved')
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
for key,resource in resource_by_key.items():
    check(resource.get('viewProfile')==f'view.placeholder.{key}', f'unresolved canonical resource visual profile: {key}')
expected_ore={'resource.ore.small':600,'resource.ore.standard':900,'resource.ore.rich':1350,'resource.ore.deep_contested_seam':2400}
check({key:resource_by_key.get(key,{}).get('capacity') for key in expected_ore} == expected_ore, 'canonical M3 Ore capacities missing or incorrect')
check(all(resource_by_key.get(key,{}).get('type') == 'Ore' and resource_by_key.get(key,{}).get('depletionProfile') == 'Finite' for key in expected_ore), 'Ore resource nodes must use finite depletion')
resource_map_keys=[e.get('contentKey') for e in source.get('resourceNodes',[])]
check(all(k in resource_by_key for k in resource_map_keys), 'map source contains unknown resource node reference')
check(resource_map_keys.count('resource.ore.standard') == 4, 'prototype map must provide two Standard Ore deposits per start')
receiver_map_keys=[e.get('contentKey') for e in source.get('resourceReceivers',[])]
check(receiver_map_keys == ['building.rock_raiders.hq','building.rock_raiders.hq'], 'prototype map must provide one HQ receiver per start')
production_entries=content_source.get('productionDefinitions',[])
production_by_unit={p.get('unit'):p for p in production_entries}
check(len(production_entries)==35 and len(production_by_unit)==35 and set(production_by_unit)=={unit.get('stableId') for unit in unit_entries}, 'M8 T073 must contain exactly one production recipe for each of the 35 canonical units')
expected_production={
    'unit.rock_raiders.crew':('building.rock_raiders.hq',50,0,1,320),
    'unit.rock_raiders.hover_scout':('building.rock_raiders.vehicle_service_bay',75,10,1,400),
    'unit.rock_raiders.rapid_rider':('building.rock_raiders.vehicle_service_bay',90,10,2,560),
    'unit.rock_raiders.loader_dozer':('building.rock_raiders.vehicle_service_bay',125,15,3,720),
}
for unit,(producer,ore,energy,oc,ticks) in expected_production.items():
    production=production_by_unit.get(unit,{})
    check(production.get('producers')==[producer] and production.get('cost')=={'ore':ore,'energy':energy,'crystals':0} and production.get('operationsCapacity')==oc and production.get('buildTicks')==ticks, f'canonical production definition mismatch: {unit}')
check(production_by_unit.get('unit.aliens.etx_servitor',{}).get('producers')==['building.ali.etx_command_core','building.ali.etx_fabricator'], 'ETX Servitor must retain both canonical producer references')
check(production_by_unit.get('unit.martians.worker_robot',{}).get('producers')==['building.mar.aero_tube_hangar','building.mar.settlement_station'], 'Worker Robot must retain both canonical producer references')
building_by_key={b.get('stableId'):b for b in content_source.get('buildingDefinitions',[])}
expected_buildings={
    # size, cost O/E/C, ticks, OC, energy generation/reserve/demand, brownout class, HP, target class, armor, source
    'building.ali.etx_command_core':([7,7],[230,60,0],800,16,[2,150,0],'CommandAndBasicEconomy',2400,'FortifiedStructure',3,'NEW_GAME_CONTENT'),
    'building.ali.etx_defense_node':([2,2],[120,30,0],600,0,[0,0,2],'StaticDefenseAndNonessential',700,'FortifiedStructure',1,'NEW_GAME_CONTENT'),
    'building.ali.etx_fabricator':([6,6],[140,25,0],640,4,[0,0,2],'ProductionAndResearch',1350,'Structure',2,'NEW_GAME_CONTENT'),
    'building.ali.power_coupler':([4,4],[130,10,0],560,0,[12,100,0],'StaticDefenseAndNonessential',850,'Structure',1,'NEW_GAME_CONTENT'),
    'building.ali.reconfiguration_dock':([8,7],[210,60,1],1000,6,[0,0,4],'ProductionAndResearch',1600,'Structure',2,'NEW_GAME_CONTENT'),
    'building.ali.resonance_core':([4,4],[160,50,0],800,0,[0,0,3],'ServiceAndFactionSystems',1200,'Structure',2,'NEW_GAME_CONTENT'),
    'building.ast.field_systems_garage':([7,6],[130,10,0],600,4,[0,0,1],'ProductionAndResearch',1450,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.ast.flight_operations_pad':([10,8],[200,45,1],900,5,[0,0,3],'ProductionAndResearch',1400,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.ast.frontier_extraction_station':([6,6],[140,15,0],640,0,[0,0,1],'ResourceProcessing',1250,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.ast.mb01_eagle_command_base':([8,8],[260,40,0],900,16,[2,150,0],'CommandAndBasicEconomy',2700,'FortifiedStructure',4,'OFFICIAL_ADAPTED'),
    'building.ast.mission_vehicle_bay':([8,7],[190,35,1],900,5,[0,0,2],'ProductionAndResearch',1700,'Structure',3,'COMPOSITE_ADAPTED'),
    'building.ast.modular_sentinel_defense':([2,2],[110,20,0],560,0,[0,0,1],'StaticDefenseAndNonessential',800,'FortifiedStructure',2,'NEW_GAME_CONTENT'),
    'building.ast.service_refit_hub':([7,7],[170,30,0],760,4,[0,0,2],'ServiceAndFactionSystems',1650,'Structure',3,'COMPOSITE_ADAPTED'),
    'building.ast.solar_energy_array':([6,5],[120,0,0],560,0,[8,100,0],'StaticDefenseAndNonessential',900,'Structure',1,'OFFICIAL_ADAPTED'),
    'building.mar.aero_guard_tower':([2,2],[120,25,0],560,0,[0,0,2],'StaticDefenseAndNonessential',760,'FortifiedStructure',2,'NEW_GAME_CONTENT'),
    'building.mar.aero_tube_hangar':([9,9],[280,40,0],1000,16,[2,150,0],'CommandAndBasicEconomy',2800,'FortifiedStructure',4,'OFFICIAL_ADAPTED'),
    'building.mar.aero_tube_link':([1,1],[50,10,0],200,0,[0,0,1],'ServiceAndFactionSystems',320,'Structure',1,'OFFICIAL_ADAPTED'),
    'building.mar.deflector_arm':([3,3],[100,15,0],500,0,[0,0,1],'StaticDefenseAndNonessential',850,'FortifiedStructure',3,'NEW_GAME_CONTENT'),
    'building.mar.excavation_plant':([6,6],[130,15,0],600,0,[0,0,1],'ResourceProcessing',1250,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.mar.mechanical_workshop':([7,6],[150,20,0],700,5,[0,0,1],'ProductionAndResearch',1450,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.mar.pressure_generator':([4,4],[130,10,0],600,0,[9,100,0],'StaticDefenseAndNonessential',900,'Structure',1,'OFFICIAL_ADAPTED'),
    'building.mar.routing_laboratory':([6,6],[170,35,1],840,4,[0,0,2],'ProductionAndResearch',1500,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.mar.settlement_station':([7,7],[220,30,0],800,12,[1,150,0],'CommandAndBasicEconomy',1900,'FortifiedStructure',3,'COMPOSITE_ADAPTED'),
    'building.rock_raiders.crusher_barrier':([3,1],[90,10,0],480,0,[0,0,0],'StaticDefenseAndNonessential',1100,'FortifiedStructure',4,'NEW_GAME_CONTENT'),
    'building.rock_raiders.crystal_vault':([5,5],[180,40,0],900,0,[0,0,1],'ResourceProcessing',1600,'FortifiedStructure',4,'COMPOSITE_ADAPTED'),
    'building.rock_raiders.cutter_mast':([2,2],[120,25,0],600,0,[0,0,2],'StaticDefenseAndNonessential',800,'FortifiedStructure',2,'NEW_GAME_CONTENT'),
    'building.rock_raiders.engineering_workshop':([8,8],[220,50,0],1100,6,[0,0,3],'ProductionAndResearch',1900,'Structure',3,'COMPOSITE_ADAPTED'),
    'building.rock_raiders.hq':([8,8],[320,40,0],1200,16,[2,150,0],'CommandAndBasicEconomy',3000,'FortifiedStructure',5,'OFFICIAL_ADAPTED'),
    'building.rock_raiders.ore_processing_plant':([6,6],[140,15,0],600,0,[0,0,1],'ResourceProcessing',1350,'Structure',2,'COMPOSITE_ADAPTED'),
    'building.rock_raiders.power_station':([5,5],[150,20,0],700,0,[10,120,0],'StaticDefenseAndNonessential',1000,'Structure',1,'COMPOSITE_ADAPTED'),
    'building.rock_raiders.vehicle_service_bay':([8,6],[160,20,0],800,4,[0,0,1],'ProductionAndResearch',1700,'Structure',3,'COMPOSITE_ADAPTED'),
}
building_entries=[e for e in content_source.get('entities',[]) if e.get('selectableKind') == 'Building']
expected_m8_building_counts={'RockRaiders':8,'Astronauts':8,'Aliens':6,'Martians':9}
actual_m8_building_counts={faction:sum(1 for building in building_entries if building.get('faction') == faction) for faction in expected_m8_building_counts}
check(len(building_by_key)==31 and set(building_by_key)==set(expected_buildings), 'M8 T071 must contain exactly the 31 canonical infrastructure definitions')
check(len(building_entries)==31 and {e.get('stableId') for e in building_entries}==set(expected_buildings), 'M8 T071 infrastructure entities must exactly match building definitions')
check(actual_m8_building_counts==expected_m8_building_counts, 'M8 T071 faction infrastructure counts must be RockRaiders=8, Astronauts=8, Aliens=6, Martians=9')
for key,(size,cost,ticks,oc,energy_values,functional_class,hp,target_class,armor,source_classification) in expected_buildings.items():
    definition=building_by_key.get(key,{})
    mask=definition.get('footprintMask',[])
    actual_size=[len(mask[0]) if mask else 0,len(mask)]
    expected_cost={'ore':cost[0],'energy':cost[1],'crystals':cost[2]}
    actual_energy=[definition.get('energyGenerationPerSecond'),definition.get('energyReserveCapacity'),definition.get('continuousEnergyDemandPerSecond')]
    entity=by_key.get(key,{})
    combat=entity.get('combatTarget',{})
    check(actual_size==size and all(len(row)==size[0] and set(row)=={'1'} for row in mask), f'canonical initial rectangular footprint mismatch: {key}')
    check(definition.get('cost')==expected_cost and definition.get('buildTicks')==ticks, f'canonical infrastructure cost/time mismatch: {key}')
    check(definition.get('operationsCapacityProvided')==oc and actual_energy==energy_values and definition.get('energyFunctionalClass')==functional_class, f'canonical infrastructure OC/Energy mismatch: {key}')
    check((combat.get('hitPoints'),combat.get('class'),combat.get('armorRating'),entity.get('sourceClassification'))==(hp,target_class,armor,source_classification), f'canonical infrastructure durability/source mismatch: {key}')
check(building_by_key['building.ast.flight_operations_pad']['cost']['crystals']==1 and building_by_key['building.mar.aero_tube_hangar']['footprintMask'][0]=='1'*9, 'T071 wide-footprint/Crystal-cost schema coverage missing')
expected_worksite_radii = {
    'building.rock_raiders.hq': 18,
    'building.rock_raiders.vehicle_service_bay': 12,
}
check({key:building_by_key.get(key,{}).get('worksiteServiceRadius') for key in expected_worksite_radii} == expected_worksite_radii, 'canonical M5 Worksite service radii missing or incorrect')

research_by_key={r.get('stableId'):r for r in content_source.get('researchDefinitions',[])}
for key,research in research_by_key.items():
    check(research.get('presentationProfile')==f'presentation.{key}', f'unresolved research presentation profile: {key}')
    check(research.get('displayNameLocKey')==f'loc.{key}.name', f'unresolved research localization key: {key}')
expected_research={
    # faction, source building, cost O/E/C, ticks
    'research.ali.advanced_resonance_architecture':('Aliens','building.ali.reconfiguration_dock',[240,110,5],1600),
    'research.ali.defense_resonance_shunt':('Aliens','building.ali.resonance_core',[120,50,1],900),
    'research.ali.etx_reconfiguration_matrix':('Aliens','building.ali.resonance_core',[160,65,2],1100),
    'research.ali.expanded_resonance_lattice':('Aliens','building.ali.resonance_core',[170,70,3],1200),
    'research.ali.infiltration_matrix':('Aliens','building.ali.reconfiguration_dock',[150,55,2],1000),
    'research.ali.mothership_resonance_relay':('Aliens','building.ali.reconfiguration_dock',[190,80,3],1200),
    'research.ali.rapid_fabrication_conduits':('Aliens','building.ali.resonance_core',[120,50,1],900),
    'research.ali.resonance_initiation':('Aliens','building.ali.resonance_core',[90,40,1],800),
    'research.ali.resonant_recovery_latches':('Aliens','building.ali.resonance_core',[120,45,1],900),
    'research.ali.siege_phase_coupling':('Aliens','building.ali.reconfiguration_dock',[150,55,2],1000),
    'research.ast.aerospace_coordination':('Astronauts','building.ast.service_refit_hub',[150,60,2],1100),
    'research.ast.deep_mission_drilling':('Astronauts','building.ast.service_refit_hub',[220,80,3],1400),
    'research.ast.field_survey_package':('Astronauts','building.ast.service_refit_hub',[80,15,0],700),
    'research.ast.field_sustainment_package':('Astronauts','building.ast.service_refit_hub',[120,30,0],900),
    'research.ast.heavy_mission_chassis':('Astronauts','building.ast.service_refit_hub',[180,60,2],1200),
    'research.ast.integrated_expedition_command':('Astronauts','building.ast.service_refit_hub',[240,90,3],1500),
    'research.ast.mission_operations_integration':('Astronauts','building.ast.service_refit_hub',[150,50,1],1100),
    'research.ast.mission_refit_protocols':('Astronauts','building.ast.service_refit_hub',[120,35,1],900),
    'research.ast.specialized_extraction_modules':('Astronauts','building.ast.service_refit_hub',[130,40,1],1000),
    'research.ast.switchframe_actuation':('Astronauts','building.ast.service_refit_hub',[140,55,1],1000),
    'research.mar.advanced_excavation_systems':('Martians','building.mar.routing_laboratory',[190,65,2],1200),
    'research.mar.aero_handling_decks':('Martians','building.mar.routing_laboratory',[100,30,0],800),
    'research.mar.grand_network_integration':('Martians','building.mar.routing_laboratory',[230,80,3],1500),
    'research.mar.hypersled_throughput':('Martians','building.mar.routing_laboratory',[120,30,0],900),
    'research.mar.mechanical_worker_toolset':('Martians','building.mar.routing_laboratory',[100,20,0],800),
    'research.mar.pressure_equalization_valves':('Martians','building.mar.routing_laboratory',[130,40,1],1000),
    'research.mar.redundant_routing':('Martians','building.mar.routing_laboratory',[150,45,1],1000),
    'research.mar.utility_mechanisms':('Martians','building.mar.routing_laboratory',[170,50,2],1100),
    'research.mar.walker_articulation':('Martians','building.mar.routing_laboratory',[160,45,1],1100),
    'research.rr.advanced_power_distribution':('RockRaiders','building.rock_raiders.engineering_workshop',[160,50,1],1100),
    'research.rr.cutter_package':('RockRaiders','building.rock_raiders.vehicle_service_bay',[90,20,0],700),
    'research.rr.deep_core_engineering':('RockRaiders','building.rock_raiders.crystal_vault',[220,90,4],1500),
    'research.rr.geological_survey_calibration':('RockRaiders','building.rock_raiders.hq',[80,20,0],700),
    'research.rr.high_capacity_processing':('RockRaiders','building.rock_raiders.engineering_workshop',[140,35,0],1000),
    'research.rr.industrial_expansion_program':('RockRaiders','building.rock_raiders.hq',[150,50,0],1100),
    'research.rr.reinforced_drilling_assemblies':('RockRaiders','building.rock_raiders.engineering_workshop',[160,45,1],1100),
    'research.rr.service_gantries':('RockRaiders','building.rock_raiders.vehicle_service_bay',[140,40,0],1000),
    'research.rr.worksite_automation':('RockRaiders','building.rock_raiders.ore_processing_plant',[120,30,0],900),
}
expected_research_counts={'RockRaiders':9,'Astronauts':10,'Aliens':10,'Martians':9}
actual_research_counts={faction:sum(1 for research in research_by_key.values() if research.get('faction')==faction) for faction in expected_research_counts}
check(len(research_by_key)==38 and set(research_by_key)==set(expected_research), 'M8 T072 must contain exactly the 38 canonical research definitions')
check(actual_research_counts==expected_research_counts, 'M8 T072 faction research counts must be RockRaiders=9, Astronauts=10, Aliens=10, Martians=9')
for key,(faction,source_building,cost,ticks) in expected_research.items():
    definition=research_by_key.get(key,{})
    check(definition.get('faction')==faction and definition.get('sourceBuilding')==source_building, f'canonical research faction/source mismatch: {key}')
    check(definition.get('cost')=={'ore':cost[0],'energy':cost[1],'crystals':cost[2]} and definition.get('researchTicks')==ticks, f'canonical research cost/time mismatch: {key}')
    check(source_building in building_by_key and by_key.get(source_building,{}).get('faction')==faction, f'research source building unresolved or cross-faction: {key}')
    check(bool(definition.get('categories')) and bool(definition.get('presentationProfile')) and bool(definition.get('displayNameLocKey')), f'research presentation/category metadata missing: {key}')
    check(bool(definition.get('unlockTags')) or bool(definition.get('parameterModifiers')), f'research effect metadata missing: {key}')
    for unlock in definition.get('unlockTags',[]):
        check(unlock.startswith('capability.') or unlock in by_key, f'unresolved research unlock reference: {key} -> {unlock}')
        if unlock in by_key: check(by_key[unlock].get('faction')==faction, f'cross-faction research unlock reference: {key} -> {unlock}')
    for modifier in definition.get('parameterModifiers',[]):
        check(modifier.get('target','').startswith('parameter.') and modifier.get('operation') in {'Add','Set','MultiplyBasisPoints'}, f'invalid research parameter modifier: {key}')
research_edges={key:[] for key in research_by_key}
for key,definition in research_by_key.items():
    for group in definition.get('prerequisiteGroups',[]):
        alternatives=group.get('anyOf',[])
        check(bool(alternatives), f'empty research prerequisite group: {key}')
        for prerequisite in alternatives:
            kind,target=prerequisite.get('kind'),prerequisite.get('target')
            check(prerequisite.get('minimum',0)>0 and prerequisite.get('persistence') in {'AtStart','WhileResearching'}, f'invalid research prerequisite metadata: {key}')
            if kind=='Research':
                check(target in research_by_key and research_by_key.get(target,{}).get('faction')==definition.get('faction'), f'unresolved/cross-faction research prerequisite: {key} -> {target}')
                research_edges[key].append(target)
            elif kind=='Building': check(target in building_by_key and by_key.get(target,{}).get('faction')==definition.get('faction'), f'unresolved/cross-faction building prerequisite: {key} -> {target}')
            elif kind=='StateThreshold': check(str(target).startswith('state.'), f'invalid research state prerequisite: {key} -> {target}')
            else: check(False, f'invalid research prerequisite kind: {key}')
research_visit={}
def visit_research(key):
    if research_visit.get(key)==1: return False
    if research_visit.get(key)==2: return True
    research_visit[key]=1
    if not all(visit_research(target) for target in research_edges[key]): return False
    research_visit[key]=2
    return True
check(all(visit_research(key) for key in research_edges), 'M8 T072 research prerequisite graph contains a cycle')
integrated=research_by_key['research.ast.integrated_expedition_command']['prerequisiteGroups']
check(len(integrated)==3 and len(integrated[2]['anyOf'])==5, 'Integrated Expedition Command must retain Field + Mission + one-of-five Mission specialization requirements')
alien_advanced=research_by_key['research.ali.advanced_resonance_architecture']['prerequisiteGroups'][0]['anyOf'][0]
check(alien_advanced=={'kind':'StateThreshold','target':'state.ali.committed_crystals','minimum':4,'persistence':'WhileResearching'}, 'Advanced Resonance Architecture must maintain four committed Crystals while researching')
grand_targets={entry['target'] for group in research_by_key['research.mar.grand_network_integration']['prerequisiteGroups'] for entry in group['anyOf']}
check(grand_targets=={'research.mar.redundant_routing','state.mar.connected_station_nodes'}, 'Grand Network Integration must require Redundant Routing and two connected Stations')

# T073 action data uses an AND of prerequisite groups, each containing one or
# more OR alternatives. Validate the source graph directly so broken references
# or cross-faction gates fail before the content compiler is invoked.
def validate_action_prerequisites(owner, owner_faction, groups, action_kind):
    if not isinstance(groups, list):
        check(False, f'{action_kind} prerequisiteGroups must be an array: {owner}')
        return ()
    signature=[]
    for group_index,group in enumerate(groups):
        alternatives=group.get('anyOf') if isinstance(group,dict) else None
        if not isinstance(alternatives,list) or not alternatives:
            check(False, f'{action_kind} prerequisite group must contain OR alternatives: {owner} group {group_index}')
            signature.append(())
            continue
        group_signature=[]
        for prerequisite in alternatives:
            kind=prerequisite.get('kind') if isinstance(prerequisite,dict) else None
            target=prerequisite.get('target') if isinstance(prerequisite,dict) else None
            group_signature.append((kind,target))
            check(kind in {'Building','Research'} and isinstance(target,str) and bool(target), f'invalid {action_kind} prerequisite: {owner} group {group_index}')
            if kind=='Building':
                resolved=target in building_by_key
                target_faction=by_key.get(target,{}).get('faction')
            elif kind=='Research':
                resolved=target in research_by_key
                target_faction=research_by_key.get(target,{}).get('faction')
            else:
                resolved=False
                target_faction=None
            check(resolved, f'unresolved {action_kind} prerequisite: {owner} -> {target}')
            if resolved:
                check(target_faction==owner_faction, f'cross-faction {action_kind} prerequisite: {owner} -> {target}')
        check(len(group_signature)==len(set(group_signature)), f'duplicate {action_kind} prerequisite alternative: {owner} group {group_index}')
        signature.append(tuple(group_signature))
    return tuple(signature)

construction_actions=content_source.get('constructionActionDefinitions',[])
construction_action_by_building={action.get('building'):action for action in construction_actions}
check(len(construction_actions)==31 and len(construction_action_by_building)==31 and set(construction_action_by_building)==set(building_by_key), 'M8 T073 must contain exactly one construction action definition for each of the 31 canonical buildings')
construction_shapes={}
for building,action in construction_action_by_building.items():
    owner_faction=by_key.get(building,{}).get('faction')
    check(building in building_by_key and owner_faction in expected_m8_building_counts, f'construction action references unknown building: {building}')
    construction_shapes[building]=validate_action_prerequisites(building,owner_faction,action.get('prerequisiteGroups'),'construction action')
check(construction_shapes.get('building.ast.service_refit_hub')==(
    (('Building','building.ast.field_systems_garage'),),
    (('Building','building.ast.solar_energy_array'),('Building','building.ast.frontier_extraction_station'))),
    'Service & Refit Hub must retain Field Garage AND (Solar Array OR Frontier Extraction Station)')
check(construction_shapes.get('building.mar.mechanical_workshop')==(
    (('Building','building.mar.aero_tube_hangar'),('Building','building.mar.settlement_station')),),
    'Mechanical Workshop must retain Hangar OR Settlement construction access')

production_shapes={}
for unit,production in production_by_unit.items():
    owner_faction=by_key.get(unit,{}).get('faction')
    producers=production.get('producers')
    check(unit in by_key and unit.startswith('unit.') and owner_faction in expected_m8_unit_counts, f'production recipe references unknown unit: {unit}')
    check(isinstance(producers,list) and bool(producers) and len(producers)==len(set(producers)), f'production recipe requires unique producer references: {unit}')
    for producer in producers if isinstance(producers,list) else []:
        check(producer in building_by_key, f'production recipe references unknown producer: {unit} -> {producer}')
        if producer in building_by_key:
            check(by_key.get(producer,{}).get('faction')==owner_faction, f'production recipe uses a cross-faction producer: {unit} -> {producer}')
    cost=production.get('cost',{})
    check(isinstance(cost,dict) and set(cost)=={'ore','energy','crystals'} and cost.get('ore',0)>0 and cost.get('energy',-1)>=0 and cost.get('crystals',-1)>=0, f'production recipe has invalid canonical cost fields: {unit}')
    check(production.get('operationsCapacity')==by_key.get(unit,{}).get('operationsCapacity') and production.get('buildTicks',0)>0, f'production recipe OC/time disagrees with canonical unit data: {unit}')
    production_shapes[unit]=validate_action_prerequisites(unit,owner_faction,production.get('prerequisiteGroups'),'production action')
check(production_shapes.get('unit.martians.excavation_searcher')==(
    (('Research','research.mar.grand_network_integration'),),
    (('Research','research.mar.advanced_excavation_systems'),)),
    'Excavation Searcher must retain Grand Network Integration AND Advanced Excavation Systems')

command_entries=content_source.get('commandDefinitions',[])
command_keys=[command.get('stableId') for command in command_entries]
command_types=[command.get('commandType') for command in command_entries]
check(len(command_entries)==35, 'M8 T073 must contain exactly 35 public command definitions')
check(len(command_keys)==len(set(command_keys))==len({key.lower() for key in command_keys if isinstance(key,str)}), 'M8 T073 command stable keys must be unique, including case-insensitively')
check(len(command_types)==len(set(command_types)), 'M8 T073 command types must be unique')
check(all(isinstance(key,str) and re.fullmatch(r'command\.[a-z0-9_]+',key) for key in command_keys), 'M8 T073 command stable keys must use canonical command.* ASCII form')
check(command_keys==sorted(command_keys), 'M8 T073 JSON command definitions must be sorted by stable key')

enum_match=re.search(r'public enum SimCommandType\s*:\s*ushort\s*\{(.*?)\n\}',commands,re.S)
enum_entries=re.findall(r'^\s*([A-Za-z_]\w*)\s*=\s*(\d+)\s*,?',enum_match.group(1),re.M) if enum_match else []
public_enum_entries=[(name,int(code)) for name,code in enum_entries if int(code)<1000]
public_enum_names=[name for name,_ in public_enum_entries]
public_enum_codes=[code for _,code in public_enum_entries]
expected_public_command_types=[
    'Move','Stop','HoldPosition','Harvest','Build','CancelConstruction','AssistConstruction','QueueProduction',
    'SetRallyPoint','SetEnergyPriority','Attack','Repair','Load','Unload','StateChange','MissionRefit',
    'SetResonanceCommitment','StartSurge','AttackMove','Patrol','SetSpread','Excavate','StartResearch',
    'CancelResearch','CancelProduction','TubeTransfer','TubeBuild','DefenseResonanceShunt','ProtectorStance',
    'SearcherBrace','ExcavationClamp','Ping','RapidFabrication','ReorderProduction','CancelMissionRefit'
]
expected_public_command_codes={name:code for code,name in enumerate(expected_public_command_types,1)}
check(len(public_enum_entries)==35 and len(set(public_enum_names))==35, 'SimCommandType must define exactly 35 unique public command names')
check(len(set(public_enum_codes))==35 and set(public_enum_codes)==set(range(1,36)), 'SimCommandType public network codes must be unique and contiguous from 1 through 35')
check(dict(public_enum_entries)==expected_public_command_codes, 'SimCommandType public command names or stable network-code assignments changed')
check(set(command_types)==set(public_enum_names), 'JSON command definitions do not cover the complete public SimCommandType enum')

def stable_content_id(key):
    value=2166136261
    for byte in key.lower().encode('ascii'):
        value=((value ^ byte) * 16777619) & 0xffffffff
    return value or 1

other_stable_keys=[]
for section in ['movementProfiles','entities','resourceNodeDefinitions','weaponDefinitions','researchDefinitions']:
    other_stable_keys.extend(item.get('stableId') for item in content_source.get(section,[]))
for transformation in content_source.get('transformationDefinitions',[]):
    other_stable_keys.append(transformation.get('stableId'))
    other_stable_keys.extend(mode.get('stableId') for mode in transformation.get('modes',[]))
stable_id_owners={}
for key in other_stable_keys + command_keys:
    if not isinstance(key,str) or not key.isascii() or not key:
        continue
    stable_id=stable_content_id(key)
    prior=stable_id_owners.get(stable_id)
    check(prior is None, f'duplicate or colliding stable content ID: {prior} / {key}')
    stable_id_owners[stable_id]=key

command_by_key={command.get('stableId'):command for command in command_entries}
allowed_eligibility_prefixes=('component.','capability.','tag.','unit.','building.')
for key,command in command_by_key.items():
    tags=command.get('eligibleEntityTags')
    check(isinstance(tags,list) and bool(tags) and len(tags)==len(set(tags)), f'command requires unique eligibility tags: {key}')
    referenced_factions=set()
    for tag in tags if isinstance(tags,list) else []:
        check(isinstance(tag,str) and tag.startswith(allowed_eligibility_prefixes), f'invalid command eligibility tag: {key} -> {tag}')
        if isinstance(tag,str) and tag.startswith('unit.'):
            check(tag in by_key and tag.startswith('unit.'), f'command references unknown eligible unit: {key} -> {tag}')
            if tag in by_key: referenced_factions.add(by_key[tag].get('faction'))
        elif isinstance(tag,str) and tag.startswith('building.'):
            check(tag in building_by_key, f'command references unknown eligible building: {key} -> {tag}')
            if tag in building_by_key: referenced_factions.add(by_key.get(tag,{}).get('faction'))
    required_research=command.get('requiredResearch','')
    check(not required_research or required_research in research_by_key, f'command references unknown required research: {key} -> {required_research}')
    if required_research in research_by_key and referenced_factions:
        check(referenced_factions=={research_by_key[required_research].get('faction')}, f'command research gate conflicts with eligible faction: {key}')
    for field,prefix in [('validationHandler','validate.'),('executionHandler','execute.'),('uiSlotProfile','slot.'),('targetingPreviewProfile','preview.')]:
        check(isinstance(command.get(field),str) and command[field].startswith(prefix), f'command {field} is missing or malformed: {key}')

# Compare every authored command field with the repository-local C# fallback.
# The fallback uses named eligibility arrays, which are simple enough to resolve
# lexically without duplicating the command catalog in this validator.
command_definition_text=(ROOT/'SimCore/Runtime/Content/CommandDefinition.cs').read_text()
csharp_tag_sets={name:tuple(re.findall(r'"([^"]+)"',values)) for name,values in re.findall(
    r'private static readonly string\[\]\s+(\w+)\s*=\s*\{([^}]*)\};',command_definition_text,re.S)}
csharp_command_pattern=re.compile(
    r'Def\(\s*"([^"]+)"\s*,\s*SimCommandType\.(\w+)\s*,\s*(\w+)\s*,\s*'
    r'CommandTargetType\.(\w+)\s*,\s*CommandQueuePolicy\.(\w+)\s*,\s*"([^"]*)"\s*,\s*'
    r'"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)',re.S)
csharp_command_rows=[]
for match in csharp_command_pattern.finditer(command_definition_text):
    stable_key,command_type,tag_set,target_type,queue_policy,required_research,validator,executor,slot,preview=match.groups()
    check(tag_set in csharp_tag_sets, f'C# command uses an unresolved eligibility tag set: {stable_key} -> {tag_set}')
    csharp_command_rows.append((stable_key,command_type,csharp_tag_sets.get(tag_set,()),target_type,queue_policy,required_research,validator,executor,slot,preview))
json_command_rows=[]
for command in command_entries:
    tags=command.get('eligibleEntityTags')
    json_command_rows.append((command.get('stableId'),command.get('commandType'),tuple(tags) if isinstance(tags,list) else (),
        command.get('targetType'),command.get('queuePolicy'),command.get('requiredResearch',''),command.get('validationHandler'),
        command.get('executionHandler'),command.get('uiSlotProfile'),command.get('targetingPreviewProfile')))
check(len(csharp_command_rows)==35 and len({row[0] for row in csharp_command_rows})==35 and len({row[1] for row in csharp_command_rows})==35, 'C# canonical command catalog must contain exactly 35 unique definitions')
check(sorted(json_command_rows)==sorted(csharp_command_rows), 'JSON and C# canonical command catalogs differ')

forward_service = (ROOT/'SimCore/Runtime/Simulation/ForwardServiceSystem.cs').read_text()
for token in ['building.ast.service_refit_hub','CanonicalRosterReferences.SolarExplorer','CanonicalRosterReferences.T3Trike','ServiceHubRadius = 18','DeployedSolarExplorerRadius = 10','ProviderBucketBuildCells = 4']:
    check(token in forward_service, f'canonical T051 Forward Service contract missing: {token}')
check('DeploymentState.Deployed' in forward_service and 'BrownoutSystem.IsOperational' in forward_service, 'T051 provider activation rules missing')
check('world.Entities.Ownership' in forward_service and 'Contains(' in forward_service, 'T051 owner/radius membership validation missing')

mission_refit = (ROOT/'SimCore/Runtime/Simulation/MissionRefitSystem.cs').read_text()
for token in ['FirstSurveyInstallOre = 25','FirstSurveyInstallEnergy = 10','18 * EnergyDomainSystem.TicksPerSecond','LaterSwapOre = 8','LaterSwapEnergy = 5','10 * EnergyDomainSystem.TicksPerSecond','20 * EnergyDomainSystem.TicksPerSecond','CanonicalRosterReferences.T3Trike']:
    check(token in mission_refit, f'canonical T052 Mission Refit contract missing: {token}')
check('ForwardServiceSystem.TryGetProviderForMember' in mission_refit and 'world.Entities.MissionRefitJob.Set' in mission_refit, 'T052 service validation or authoritative job missing')
check('state.CurrentConfiguration = job.NewConfiguration' in mission_refit and 'MissionRefitJob.Remove' in mission_refit, 'T052 identity-preserving completion missing')

tube_transfer = (ROOT/'SimCore/Runtime/Simulation/TubeTransferSystem.cs').read_text()
check('CanonicalRosterReferences.IsTubeEligible' in tube_transfer,
      'T074 Aero Tube eligibility does not use the canonical three-unit roster closure')
canonical_runtime_bindings='\n'.join((ROOT/path).read_text() for path in [
    'SimCore/Runtime/Simulation/ForwardServiceSystem.cs',
    'SimCore/Runtime/Simulation/MissionRefitSystem.cs',
    'SimCore/Runtime/Simulation/TubeTransferSystem.cs',
    'SimCore/Runtime/Scenarios/ScenarioFactory.cs',
    'SimCore/Runtime/Scenarios/M5AcceptanceScenarioFactory.cs',
    'SimCore/Runtime/Networking/ServerCommandAuthority.cs',
    'GodotClient/Scripts/UI/BasicHud.cs',
    'GodotClient/Scripts/UI/M5PlaytestHud.cs',
])
for stale_key in ['unit.ast.t3_trike','unit.ast.solar_explorer','unit.mar.worker_robot',
                  'unit.mar.double_hover','unit.mar.jet_scooter','unit.mar.excavation_searcher',
                  'unit.ali.razor_skimmer']:
    check(f'"{stale_key}"' not in canonical_runtime_bindings, f'T074 stale runtime roster reference: {stale_key}')

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
