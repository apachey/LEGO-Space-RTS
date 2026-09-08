#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
PAGE="${1:-factions}"
OUTPUT="${2:-${ROOT}/Artifacts/Screenshots/m7-palette-${PAGE}.png}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-m7-palette-${PAGE}.log"

case "${PAGE}" in
  factions|martian-sources|faction-models|martian-models|transparency|light-language) ;;
  *) printf 'Usage: %s [factions|martian-sources|faction-models|martian-models|transparency|light-language] [output.png]\n' "$0" >&2; exit 2 ;;
esac

if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -p:NuGetAudit=false --ignore-failed-sources
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --m7-palette-lab --m7-palette-smoke --m7-palette-page "${PAGE}" --capture-path "${OUTPUT}"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q 'M7 PALETTE LAB: PASS factionGroups=5 martianGroups=5' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Palette Ratio Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
printf 'PASS: M7 Palette Ratio Lab capture saved to %s\n' "${OUTPUT}"
