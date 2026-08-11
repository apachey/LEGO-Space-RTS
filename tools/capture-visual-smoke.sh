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
if [[ -n "${CAPTURE_MODE}" && "${CAPTURE_MODE}" != "--construction" && "${CAPTURE_MODE}" != "--excavation" ]]; then printf 'Usage: %s [output.png] [--construction|--excavation]\n' "$0" >&2; exit 2; fi

if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2; exit 1; fi
mkdir -p "$(dirname "${OUTPUT}")"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -p:NuGetAudit=false --ignore-failed-sources
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
if [[ "${CAPTURE_MODE}" == "--construction" ]]; then
  "${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --capture-smoke --capture-path "${OUTPUT}" --capture-construction
elif [[ "${CAPTURE_MODE}" == "--excavation" ]]; then
  "${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --capture-smoke --capture-path "${OUTPUT}" --capture-excavation
else
  "${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- --capture-smoke --capture-path "${OUTPUT}"
fi
if [[ ! -s "${OUTPUT}" ]]; then printf 'FAIL: visual smoke capture was not produced.\n' >&2; exit 1; fi
printf 'PASS: visual smoke capture saved to %s\n' "${OUTPUT}"
