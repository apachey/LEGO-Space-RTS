#!/usr/bin/env bash

set -u -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment

MODE="fast"
if [[ "${1:-}" == "--full" ]]; then MODE="full"; shift
elif [[ "${1:-}" == "--m9-acceptance" ]]; then MODE="m9-acceptance"; shift
fi
if (( $# > 0 )); then printf 'Usage: %s [--full|--m9-acceptance]\n' "$0" >&2; exit 2; fi

ARTIFACT_DIR="${ROOT}/Artifacts/Verification"
mkdir -p "${ARTIFACT_DIR}"
RUN_ID="$(date -u '+%Y%m%dT%H%M%SZ')-${MODE}"
SUMMARY_FILE="${ARTIFACT_DIR}/${RUN_ID}-summary.txt"
STAGE_LABELS=()
STAGE_RESULTS=()
FAILURES=0
DIAGNOSTIC_FAILURES=0

record_stage() {
  STAGE_LABELS+=("$1")
  STAGE_RESULTS+=("$2")
}

run_stage() {
  local label="$1"
  local slug="$2"
  shift 2
  local log="${ARTIFACT_DIR}/${RUN_ID}-${slug}.log"
  local status

  printf '\n== %s ==\n' "${label}"
  "$@" 2>&1 | tee "${log}"
  status=${PIPESTATUS[0]}
  if (( status == 0 )); then
    printf 'PASS: %s\n' "${label}"
    record_stage "${label}" "PASS"
  elif (( status == 77 )); then
    printf 'SKIPPED: %s (see %s)\n' "${label}" "${log#"${ROOT}/"}"
    record_stage "${label}" "SKIPPED"
  else
    printf 'FAIL: %s (exit %d; see %s)\n' "${label}" "${status}" "${log#"${ROOT}/"}"
    record_stage "${label}" "FAIL"
    FAILURES=$((FAILURES + 1))
  fi
}

run_diagnostic_stage() {
  local label="$1"
  local slug="$2"
  shift 2
  local log="${ARTIFACT_DIR}/${RUN_ID}-${slug}.log"
  local status

  printf '\n== %s ==\n' "${label}"
  "$@" 2>&1 | tee "${log}"
  status=${PIPESTATUS[0]}
  if (( status == 0 )); then
    printf 'DIAGNOSTIC PASS: %s\n' "${label}"
    record_stage "${label}" "DIAGNOSTIC_PASS"
  elif (( status == 77 )); then
    printf 'DIAGNOSTIC SKIPPED: %s (see %s)\n' "${label}" "${log#"${ROOT}/"}"
    record_stage "${label}" "DIAGNOSTIC_SKIPPED"
  else
    printf 'DIAGNOSTIC FAIL: %s (exit %d; see %s)\n' "${label}" "${status}" "${log#"${ROOT}/"}"
    record_stage "${label}" "DIAGNOSTIC_FAIL"
    DIAGNOSTIC_FAILURES=$((DIAGNOSTIC_FAILURES + 1))
  fi
}

compile_and_compare_content() {
  local temp_dir
  temp_dir="$(mktemp -d "${TMPDIR:-/tmp}/lego-space-rts-content.XXXXXX")" || return 1
  dotnet run --project "${ROOT}/tools/ContentCompiler/ContentCompiler.csproj" -c Release --no-build --no-restore -- \
    "${ROOT}/Content/PrototypeEntities.json" \
    "${ROOT}/Content/Maps/DEV_FirstControllableRTS.map.json" \
    "${temp_dir}" || { local rc=$?; rm -rf "${temp_dir}"; return "${rc}"; }
  cmp "${temp_dir}/PrototypeEntities.contentbin" "${ROOT}/GodotClient/Compiled/PrototypeEntities.contentbin" || { rm -rf "${temp_dir}"; printf 'Tracked compiled entity content is stale.\n' >&2; return 1; }
  cmp "${temp_dir}/DEV_FirstControllableRTS.mapbin" "${ROOT}/GodotClient/Compiled/DEV_FirstControllableRTS.mapbin" || { rm -rf "${temp_dir}"; printf 'Tracked compiled map is stale.\n' >&2; return 1; }
  rm -rf "${temp_dir}"
  printf 'Compiled content exactly matches source regeneration.\n'
}

regenerate_tracked_content() {
  local temp_dir
  temp_dir="$(mktemp -d "${TMPDIR:-/tmp}/lego-space-rts-regenerate.XXXXXX")" || return 1
  cp "${ROOT}/GodotClient/Compiled/PrototypeEntities.contentbin" "${temp_dir}/PrototypeEntities.contentbin" || { rm -rf "${temp_dir}"; return 1; }
  cp "${ROOT}/GodotClient/Compiled/DEV_FirstControllableRTS.mapbin" "${temp_dir}/DEV_FirstControllableRTS.mapbin" || { rm -rf "${temp_dir}"; return 1; }
  dotnet run --project "${ROOT}/tools/ContentCompiler/ContentCompiler.csproj" -c Release --no-build --no-restore -- \
    "${ROOT}/Content/PrototypeEntities.json" \
    "${ROOT}/Content/Maps/DEV_FirstControllableRTS.map.json" \
    "${ROOT}/GodotClient/Compiled" || { local rc=$?; rm -rf "${temp_dir}"; return "${rc}"; }
  cmp "${temp_dir}/PrototypeEntities.contentbin" "${ROOT}/GodotClient/Compiled/PrototypeEntities.contentbin" || { rm -rf "${temp_dir}"; printf 'Compiled entity content regeneration is not idempotent.\n' >&2; return 1; }
  cmp "${temp_dir}/DEV_FirstControllableRTS.mapbin" "${ROOT}/GodotClient/Compiled/DEV_FirstControllableRTS.mapbin" || { rm -rf "${temp_dir}"; printf 'Compiled map regeneration is not idempotent.\n' >&2; return 1; }
  rm -rf "${temp_dir}"
  printf 'Tracked compiled content regenerates idempotently.\n'
}

headless_dll() {
  printf '%s\n' "${ROOT}/HeadlessSim/bin/Release/net8.0/HeadlessSim.dll"
}

verify_replay_hash() {
  local temp_dir record_output replay_output record_hash replay_hash
  temp_dir="$(mktemp -d "${TMPDIR:-/tmp}/lego-space-rts-replay.XXXXXX")" || return 1
  record_output="$(dotnet "$(headless_dll)" --scenario golden --ticks 3200 --record-replay "${temp_dir}/golden.replay")" || { local rc=$?; printf '%s\n' "${record_output}"; rm -rf "${temp_dir}"; return "${rc}"; }
  printf '%s\n' "${record_output}"
  replay_output="$(dotnet "$(headless_dll)" --replay "${temp_dir}/golden.replay" --ticks 3200)" || { local rc=$?; printf '%s\n' "${replay_output}"; rm -rf "${temp_dir}"; return "${rc}"; }
  printf '%s\n' "${replay_output}"
  record_hash="$(printf '%s\n' "${record_output}" | sed -n 's/.*finalHash=\([0-9A-F]*\).*/\1/p' | tail -1)"
  replay_hash="$(printf '%s\n' "${replay_output}" | sed -n 's/.*finalHash=\([0-9A-F]*\).*/\1/p' | tail -1)"
  rm -rf "${temp_dir}"
  [[ -n "${record_hash}" && "${record_hash}" == "${replay_hash}" ]] || { printf 'Replay hash mismatch: record=%s replay=%s\n' "${record_hash}" "${replay_hash}" >&2; return 1; }
  printf 'Replay final hash verified: %s\n' "${record_hash}"
}

verify_snapshot_continuation() {
  local temp_dir direct_output initial_output continued_output direct_hash continued_hash
  temp_dir="$(mktemp -d "${TMPDIR:-/tmp}/lego-space-rts-snapshot.XXXXXX")" || return 1
  direct_output="$(dotnet "$(headless_dll)" --scenario first --ticks 1600)" || { local rc=$?; printf '%s\n' "${direct_output}"; rm -rf "${temp_dir}"; return "${rc}"; }
  printf '%s\n' "${direct_output}"
  initial_output="$(dotnet "$(headless_dll)" --scenario first --ticks 800 --snapshot-out "${temp_dir}/first.snapshot")" || { local rc=$?; printf '%s\n' "${initial_output}"; rm -rf "${temp_dir}"; return "${rc}"; }
  printf '%s\n' "${initial_output}"
  continued_output="$(dotnet "$(headless_dll)" --snapshot-in "${temp_dir}/first.snapshot" --ticks 800)" || { local rc=$?; printf '%s\n' "${continued_output}"; rm -rf "${temp_dir}"; return "${rc}"; }
  printf '%s\n' "${continued_output}"
  direct_hash="$(printf '%s\n' "${direct_output}" | sed -n 's/.*finalHash=\([0-9A-F]*\).*/\1/p' | tail -1)"
  continued_hash="$(printf '%s\n' "${continued_output}" | sed -n 's/.*finalHash=\([0-9A-F]*\).*/\1/p' | tail -1)"
  rm -rf "${temp_dir}"
  [[ -n "${direct_hash}" && "${direct_hash}" == "${continued_hash}" ]] || { printf 'Snapshot continuation mismatch: direct=%s continued=%s\n' "${direct_hash}" "${continued_hash}" >&2; return 1; }
  printf 'Snapshot restore/continuation hash verified: %s\n' "${direct_hash}"
}

godot_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'PHASE10 GODOT HEADLESS SMOKE: PASS'; then
    printf 'Godot exited without the required smoke PASS marker.\n' >&2
    return 1
  fi
}

godot_m6_transport_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m6-transport-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M6 TRANSPORT SMOKE: PASS serverConnections=2'; then
    printf 'Godot exited without the required two-connection M6 transport PASS marker.\n' >&2
    return 1
  fi
}

godot_m6_command_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m6-command-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M6 COMMAND AUTHORITY SMOKE: PASS sessions=2 accepted=3 rejected=6'; then
    printf 'Godot exited without the required T059 authority PASS marker.\n' >&2
    return 1
  fi
}

godot_m6_snapshot_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m6-snapshot-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M6 SNAPSHOT FOG SMOKE: PASS clients=2 cadenceHz=10 acknowledgedDeltas=2 hiddenEntities=0'; then
    printf 'Godot exited without the required T060/T061 snapshot/fog PASS marker.\n' >&2
    return 1
  fi
}

godot_m6_reconnect_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m6-reconnect-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M6 RECONNECT SMOKE: PASS player=0 lastSequence=1'; then
    printf 'Godot exited without the required T062 reconnect PASS marker.\n' >&2
    return 1
  fi
}

godot_m6_replay_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m6-replay-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M6 NETWORK REPLAY SMOKE: PASS commands=1'; then
    printf 'Godot exited without the required T063 server-log playback PASS marker.\n' >&2
    return 1
  fi
}

godot_m7_material_smoke() {
  local godot output status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-material-lab --m7-material-smoke 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M7 T064 MATERIAL LAB: PASS families=6 factionSwatches=5 identificationTiles=5'; then
    printf 'Godot exited without the required T064 material-master PASS marker.\n' >&2
    return 1
  fi
}

godot_m7_style_smoke() {
  local godot import_output output status style
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  import_output="$("${godot}" --headless --import --path "${ROOT}/GodotClient" 2>&1)"
  status=$?
  printf '%s\n' "${import_output}"
  if (( status != 0 )); then return "${status}"; fi
  for style in industrial-mass heroic-rts constructive-lego graphic-volume; do
    output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-style-lab --m7-style-smoke --m7-style "${style}" --m7-outline off 2>&1)"
    status=$?
    printf '%s\n' "${output}"
    if (( status != 0 )); then return "${status}"; fi
    if ! printf '%s\n' "${output}" | grep -q "M7 STYLE LAB: PASS styles=4.*active=${style} outline=off"; then
      printf 'Godot exited without the required controlled M7 Style Lab PASS marker for %s.\n' "${style}" >&2
      return 1
    fi
  done
  output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-style-lab --m7-style-smoke --m7-style heroic-rts --m7-outline on 2>&1)"
  status=$?
  printf '%s\n' "${output}"
  if (( status != 0 )); then return "${status}"; fi
  if ! printf '%s\n' "${output}" | grep -q 'M7 STYLE LAB: PASS styles=4.*active=heroic-rts outline=on'; then
    printf 'Godot exited without the required independent outline-toggle PASS marker.\n' >&2
    return 1
  fi
}

godot_m7_palette_smoke() {
  local godot output page status
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  for page in factions martian-sources faction-models martian-models transparency light-language; do
    output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-palette-lab --m7-palette-smoke --m7-palette-page "${page}" 2>&1)"
    status=$?
    printf '%s\n' "${output}"
    if (( status != 0 )); then return "${status}"; fi
    if ! printf '%s\n' "${output}" | grep -q "M7 PALETTE LAB: PASS factionGroups=5 martianGroups=5 nonEmissiveTransparent=5 luminousFunctions=10.*active=${page}"; then
      printf 'Godot exited without the required M7 Palette Ratio Lab PASS marker for %s.\n' "${page}" >&2
      return 1
    fi
  done
}

godot_m7_look_smoke() {
  local godot output status controls zoom outline post ground world surface fixture
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  for fixture in \
    "visible 35 on on authored earth earth-desert" \
    "hidden 35 off on authored mars mars-oxide" \
    "hidden 72 on on authored moon moon-regolith" \
    "hidden 35 on off authored planet-u planet-u-mineral" \
    "hidden 35 on on authored underground underground-cavern" \
    "hidden 35 on on raster earth earth-desert"; do
    read -r controls zoom outline post ground world surface <<< "${fixture}"
    output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-look-lab --m7-look-smoke --m7-look-controls "${controls}" --m7-look-zoom "${zoom}" --m7-look-post "${post}" --m7-look-outline "${outline}" --m7-look-ground "${ground}" --m7-look-world "${world}" 2>&1)"
    status=$?
    printf '%s\n' "${output}"
    if (( status != 0 )); then return "${status}"; fi
    if printf '%s\n' "${output}" | grep -qE 'SHADER ERROR|SCRIPT ERROR|ERROR: Shader compilation failed'; then
      printf 'M7 Look Lab emitted a shader or script error.\n' >&2
      return 1
    fi
    if ! printf '%s\n' "${output}" | grep -q "M7 LOOK LAB: PASS schema=8 units=4 meshes=192 triangles=31104 buildings=2 firing=on burning=on animationDrivers=4 destructionDriver=1 vfxPools=6 prewarmed=180 controls=${controls} zoom=${zoom} post=${post} outline=${outline}.*world=${world}.*ground=${ground} surface=${surface} materialView=combined audit=off"; then
      printf 'Godot exited without the required M7 Look Lab PASS marker for controls=%s zoom=%s post=%s outline=%s ground=%s world=%s surface=%s.\n' "${controls}" "${zoom}" "${post}" "${outline}" "${ground}" "${world}" "${surface}" >&2
      return 1
    fi
  done
}

godot_m7_hud_smoke() {
  local godot output status fixture scenario aspect finish palette
  godot="$(discover_godot 2>/dev/null || true)"
  if [[ -z "${godot}" ]]; then printf 'Godot executable not found.\n' >&2; return 1; fi
  if ! godot_is_required_mono "${godot}"; then printf 'Godot is not the required 4.7.1 .NET build: %s\n' "${godot}" >&2; return 1; fi
  for fixture in "mixed-army 16-9 hybrid light" "mixed-army 16-9 hybrid sandstone" "mixed-army 16-9 hybrid oxide" "mixed-army 16-9 hybrid alien" "mixed-army 16-9 structural graphite" "mixed-army 16-9 legacy light" "mixed-army 16-9 clean light" "production 16-10 hybrid sandstone" "brownout 21-9 hybrid oxide" "critical-tooltip 4-3 hybrid light"; do
    read -r scenario aspect finish palette <<< "${fixture}"
    output="$("${godot}" --headless --quit-after 600 --path "${ROOT}/GodotClient" -- --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${scenario}" --m7-hud-aspect "${aspect}" --m7-hud-finish "${finish}" --m7-hud-palette "${palette}" 2>&1)"
    status=$?
    printf '%s\n' "${output}"
    if (( status != 0 )); then return "${status}"; fi
    if printf '%s\n' "${output}" | grep -qE 'SCRIPT ERROR|ERROR:'; then
      printf 'M7 HUD Lab emitted a script or runtime error.\n' >&2
      return 1
    fi
    if ! printf '%s\n' "${output}" | grep -q "M7 HUD LAB: PASS scenarios=8 commands=12 minimap=legal.*factionSkins=5 finishes=4 palettes=6.*schema=6 active=${scenario}"; then
      printf 'Godot exited without the required T068/T069 HUD Lab PASS marker for scenario=%s aspect=%s finish=%s palette=%s.\n' "${scenario}" "${aspect}" "${finish}" "${palette}" >&2
      return 1
    fi
  done
}

run_stage "[BLOCKING_NOW] Static/source validation" "static" python3 "${ROOT}/tools/Validation/validate_phase10.py"
run_stage "[BLOCKING_NOW] .NET restore" "restore" dotnet restore "${ROOT}/LEGO.SpaceRTS.Phase10.sln" --disable-build-servers
run_stage "[BLOCKING_NOW] .NET solution build (warnings as errors)" "build" dotnet build "${ROOT}/LEGO.SpaceRTS.Phase10.sln" -c Release --no-restore --disable-build-servers -m:1
run_stage "[BLOCKING_NOW] Godot C# Debug host build" "godot-build" dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
run_stage "[BLOCKING_NOW] NUnit deterministic/snapshot/replay/stress suite" "tests" dotnet test "${ROOT}/SimCore.Tests/SimCore.Tests.csproj" -c Release --no-build --no-restore --disable-build-servers --verbosity minimal
run_stage "[BLOCKING_NOW] Representative 24-mover Movement Architecture v2 acceptance" "m2-movement" dotnet test "${ROOT}/SimCore.Tests/SimCore.Tests.csproj" -c Release --no-build --no-restore --disable-build-servers --verbosity minimal --filter "FullyQualifiedName~M2MovementAcceptanceTests"
run_stage "[BLOCKING_NOW] Content compilation and tracked-binary validation" "content" compile_and_compare_content
run_stage "[BLOCKING_NOW] HeadlessSim compiled-content smoke" "headless" dotnet "$(headless_dll)" --scenario first --compiled-dir "${ROOT}/GodotClient/Compiled" --ticks 1200 --hash-every 200
run_stage "[BLOCKING_NOW] Godot C# PrototypeRTS headless smoke" "godot" godot_smoke
run_stage "[BLOCKING_NOW T058] Godot ENet dedicated host with two clients" "m6-transport" godot_m6_transport_smoke
run_stage "[BLOCKING_NOW T059] Godot server command authority over ENet" "m6-command" godot_m6_command_smoke
run_stage "[BLOCKING_NOW T060/T061] Godot 10 Hz delta snapshots without hidden data" "m6-snapshot" godot_m6_snapshot_smoke
run_stage "[BLOCKING_NOW T062] Godot reconnect restore over ENet" "m6-reconnect" godot_m6_reconnect_smoke
run_stage "[BLOCKING_NOW T063] Godot authoritative server-log replay" "m6-replay" godot_m6_replay_smoke
run_stage "[BLOCKING_NOW M7 VISUAL EXPLORATION] Controlled Godot Style Lab" "m7-style" godot_m7_style_smoke
run_stage "[BLOCKING_NOW M7 VISUAL EXPLORATION] Six-page Palette Ratio Lab" "m7-palette" godot_m7_palette_smoke
run_stage "[BLOCKING_NOW M7 VISUAL EXPLORATION] Realtime gameplay-scale Look Lab" "m7-look" godot_m7_look_smoke
run_stage "[BLOCKING_NOW T068/T069] Responsive HUD and fog-correct minimap lab" "m7-hud" godot_m7_hud_smoke

if [[ "${MODE}" != "fast" ]]; then
  run_stage "[BLOCKING_NOW] 100-repeat deterministic golden run" "golden100" dotnet "$(headless_dll)" --scenario golden --ticks 3200 --repeat 100
  run_stage "[BLOCKING_NOW] Replay record/final-hash verification" "replay" verify_replay_hash
  run_stage "[BLOCKING_NOW] Snapshot restore/continuation verification" "snapshot" verify_snapshot_continuation
  if [[ "${MODE}" == "m9-acceptance" ]]; then
    run_stage "[BLOCKING_NOW M9 LARGE-BATTLE ACCEPTANCE] 60-mover navigation/performance stress" "stress60" dotnet "$(headless_dll)" --scenario stress60 --ticks 26000 --benchmark --path-benchmark --enforce-performance-gates
  else
    run_diagnostic_stage "[BLOCKING_LATER M9 LARGE-BATTLE ACCEPTANCE] 60-mover navigation/performance stress" "stress60" dotnet "$(headless_dll)" --scenario stress60 --ticks 26000 --benchmark --path-benchmark --enforce-performance-gates
  fi
  run_stage "[BLOCKING_NOW] Compiled-content regeneration" "content-regenerate" regenerate_tracked_content
  run_stage "[BLOCKING_NOW] macOS debug export smoke" "macos-export" "${ROOT}/tools/build-mac.sh" --verify
fi

{
  printf 'LEGO Space RTS verification summary (%s)\n' "${MODE}"
  printf 'Run: %s\n' "${RUN_ID}"
  for ((i=0; i<${#STAGE_LABELS[@]}; i++)); do printf '%-18s %s\n' "${STAGE_RESULTS[$i]}" "${STAGE_LABELS[$i]}"; done
  printf 'Blocking failures: %d\n' "${FAILURES}"
  printf 'Diagnostic failures: %d\n' "${DIAGNOSTIC_FAILURES}"
} | tee "${SUMMARY_FILE}"

if (( FAILURES > 0 )); then
  printf 'VERIFICATION: FAIL (%d failed stage(s); summary: %s)\n' "${FAILURES}" "${SUMMARY_FILE#"${ROOT}/"}"
  exit 1
fi
printf 'VERIFICATION: PASS (summary: %s)\n' "${SUMMARY_FILE#"${ROOT}/"}"
