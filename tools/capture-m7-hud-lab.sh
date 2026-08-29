#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/m7-hud-lab-mixed-army.png}"
SCENARIO="${2:-mixed-army}"
ASPECT="${3:-16-9}"
CONTROLS="${4:-hidden}"
FINISH="${5:-hybrid}"
PALETTE="${6:-light}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-m7-hud-lab.log"
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi

case "${SCENARIO}" in rock-unit|mixed-army|production|brownout|astronaut-transform|alien-resonance|martian-network|critical-tooltip) ;;
  *) printf 'Usage: %s [output.png] [rock-unit|mixed-army|production|brownout|astronaut-transform|alien-resonance|martian-network|critical-tooltip] [16-9|16-10|21-9|4-3]\n' "$0" >&2; exit 2 ;;
esac
case "${ASPECT}" in 16-9|16-10|21-9|4-3) ;;
  *) printf 'Usage: %s [output.png] [scenario] [16-9|16-10|21-9|4-3]\n' "$0" >&2; exit 2 ;;
esac
case "${CONTROLS}" in visible|hidden) ;;
  *) printf 'Usage: %s [output.png] [scenario] [aspect] [visible|hidden] [hybrid|structural|legacy|clean] [light|sandstone|oxide|olive|alien|graphite|custom]\n' "$0" >&2; exit 2 ;;
esac
case "${FINISH}" in
  hybrid) EXPECTED_FINISH="HybridConsole" ;;
  structural) EXPECTED_FINISH="StructuralConsole" ;;
  legacy) EXPECTED_FINISH="LegacyFrames" ;;
  clean) EXPECTED_FINISH="Clean" ;;
  *) printf 'Usage: %s [output.png] [scenario] [aspect] [visible|hidden] [hybrid|structural|legacy|clean] [light|sandstone|oxide|olive|alien|graphite|custom]\n' "$0" >&2; exit 2 ;;
esac
case "${PALETTE}" in
  light) EXPECTED_PALETTE="LightCeramic" ;;
  sandstone) EXPECTED_PALETTE="WarmSandstone" ;;
  oxide) EXPECTED_PALETTE="OxideWorkshop" ;;
  olive) EXPECTED_PALETTE="FieldOlive" ;;
  alien) EXPECTED_PALETTE="AlienPorcelain" ;;
  graphite) EXPECTED_PALETTE="NeutralGraphite" ;;
  custom) EXPECTED_PALETTE="Custom" ;;
  *) printf 'Usage: %s [output.png] [scenario] [aspect] [visible|hidden] [hybrid|structural|legacy|clean] [light|sandstone|oxide|olive|alien|graphite|custom]\n' "$0" >&2; exit 2 ;;
esac
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --headless --import --path "${ROOT}/GodotClient"
"${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${SCENARIO}" --m7-hud-aspect "${ASPECT}" --m7-hud-finish "${FINISH}" --m7-hud-palette "${PALETTE}" --m7-hud-controls "${CONTROLS}" --capture-path "${OUTPUT}"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q "M7 HUD LAB: PASS scenarios=8 commands=12 minimap=legal.*factionSkins=5 finishes=4 palettes=6.*schema=7 active=${SCENARIO} finish=${EXPECTED_FINISH} palette=${EXPECTED_PALETTE}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 HUD Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if grep -qE 'SCRIPT ERROR|ERROR:' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 HUD Lab reported a script or runtime error.\n' >&2
  exit 1
fi
printf 'PASS: M7 HUD Lab capture saved to %s\n' "${OUTPUT}"
