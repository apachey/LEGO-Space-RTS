#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
STYLE="${1:-industrial-mass}"
OUTPUT="${2:-${ROOT}/Artifacts/Screenshots/m7-style-${STYLE}.png}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-m7-style-${STYLE}.log"

case "${STYLE}" in
  industrial-mass|heroic-rts|constructive-lego|graphic-volume) ;;
  *) printf 'Usage: %s [industrial-mass|heroic-rts|constructive-lego|graphic-volume] [output.png]\n' "$0" >&2; exit 2 ;;
esac

if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -p:NuGetAudit=false --ignore-failed-sources
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --headless --import --path "${ROOT}/GodotClient"
"${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --m7-style-lab --m7-style-smoke --m7-style "${STYLE}" --capture-path "${OUTPUT}"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q 'M7 STYLE LAB: PASS styles=4' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Style Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
printf 'PASS: M7 Style Lab capture saved to %s\n' "${OUTPUT}"
