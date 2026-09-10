#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/default-camera-spawn.png}"
CAPTURE_MODE="${2:-}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-visual-smoke.log"
if [[ -n "${CAPTURE_MODE}" && "${CAPTURE_MODE}" != "--construction" && "${CAPTURE_MODE}" != "--repair" && "${CAPTURE_MODE}" != "--transport" && "${CAPTURE_MODE}" != "--transformation" && "${CAPTURE_MODE}" != "--transformation-rollback" && "${CAPTURE_MODE}" != "--excavation" ]]; then printf 'Usage: %s [output.png] [--construction|--repair|--transport|--transformation|--transformation-rollback|--excavation]\n' "$0" >&2; exit 2; fi

if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2; exit 1; fi
mkdir -p "$(dirname "${OUTPUT}")"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -p:NuGetAudit=false --ignore-failed-sources
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
if [[ "${CAPTURE_MODE}" == "--construction" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-construction
elif [[ "${CAPTURE_MODE}" == "--repair" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-repair
elif [[ "${CAPTURE_MODE}" == "--transport" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-transport
elif [[ "${CAPTURE_MODE}" == "--transformation" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-transformation
elif [[ "${CAPTURE_MODE}" == "--transformation-rollback" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-transformation-rollback
elif [[ "${CAPTURE_MODE}" == "--excavation" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}" --capture-excavation
else
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --automated-smoke-immediate-exit --capture-smoke --capture-path "${OUTPUT}"
fi
if [[ ! -s "${OUTPUT}" ]]; then printf 'FAIL: visual smoke capture was not produced.\n' >&2; exit 1; fi
printf 'PASS: visual smoke capture saved to %s\n' "${OUTPUT}"
