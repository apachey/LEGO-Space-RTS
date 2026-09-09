#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/t064-material-lab.png}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-t064-material-lab.log"

if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -p:NuGetAudit=false --ignore-failed-sources
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --automated-smoke-immediate-exit --m7-material-lab --m7-material-smoke --capture-path "${OUTPUT}"
if [[ ! -s "${OUTPUT}" ]]; then
  printf 'FAIL: T064 Material Lab capture was not produced.\n' >&2
  exit 1
fi
printf 'PASS: T064 Material Lab capture saved to %s\n' "${OUTPUT}"
