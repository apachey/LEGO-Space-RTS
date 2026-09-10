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
PALETTE="${6:-faction}"
KIT="${7:-scenario}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-m7-hud-lab.log"
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi

usage() {
  printf 'Usage: %s [output.png] [rock-unit|mixed-army|production|brownout|astronaut-transform|alien-resonance|martian-network|critical-tooltip] [16-9|16-10|21-9|4-3] [visible|hidden] [hybrid|structural|legacy|clean] [faction|light|sandstone|oxide|olive|alien|graphite|custom] [scenario|rock-raiders|astronauts|aliens|martians]\n' "$0" >&2
}

case "${SCENARIO}" in rock-unit|mixed-army|production|brownout|astronaut-transform|alien-resonance|martian-network|critical-tooltip) ;;
  *) usage; exit 2 ;;
esac
KIT_ARGS=()
KIT_ENABLED=0
case "${KIT}" in
  scenario)
    case "${SCENARIO}" in
      astronaut-transform) EXPECTED_KIT="Astronauts" ;;
      alien-resonance) EXPECTED_KIT="Aliens" ;;
      martian-network) EXPECTED_KIT="Martians" ;;
      *) EXPECTED_KIT="Rock Raiders" ;;
    esac
    ;;
  rock|rock-raiders|rockraiders)
    EXPECTED_KIT="Rock Raiders"; KIT_ARGS=(--m7-hud-kit rock-raiders); KIT_ENABLED=1 ;;
  astronaut|astronauts)
    EXPECTED_KIT="Astronauts"; KIT_ARGS=(--m7-hud-kit astronauts); KIT_ENABLED=1 ;;
  alien|aliens)
    EXPECTED_KIT="Aliens"; KIT_ARGS=(--m7-hud-kit aliens); KIT_ENABLED=1 ;;
  martian|martians)
    EXPECTED_KIT="Martians"; KIT_ARGS=(--m7-hud-kit martians); KIT_ENABLED=1 ;;
  *) usage; exit 2 ;;
esac
case "${ASPECT}" in 16-9|16-10|21-9|4-3) ;;
  *) usage; exit 2 ;;
esac
case "${CONTROLS}" in visible|hidden) ;;
  *) usage; exit 2 ;;
esac
case "${FINISH}" in
  hybrid) EXPECTED_FINISH="HybridConsole" ;;
  structural) EXPECTED_FINISH="StructuralConsole" ;;
  legacy) EXPECTED_FINISH="LegacyFrames" ;;
  clean) EXPECTED_FINISH="Clean" ;;
  *) usage; exit 2 ;;
esac
PALETTE_ARGS=()
PALETTE_ENABLED=0
case "${PALETTE}" in
  faction|faction-bound|auto|canonical)
    # Schema 8 binds the complete surface recipe to the scenario faction.
    # Omitting the legacy palette switch is intentional: passing it would
    # convert the profile into a manual Custom audit override.
    EXPECTED_PALETTE="FactionBound"
    ;;
  light|sandstone|oxide|olive|alien|graphite|custom)
    # Retained for comparing old captures. In schema 8 every independent
    # palette selection is explicitly reported as a Custom override.
    EXPECTED_PALETTE="Custom"
    PALETTE_ARGS=(--m7-hud-palette "${PALETTE}")
    PALETTE_ENABLED=1
    ;;
  *) usage; exit 2 ;;
esac
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --headless --disable-crash-handler --import --path "${ROOT}/GodotClient"
if [[ "${PALETTE_ENABLED}" == "1" && "${KIT_ENABLED}" == "1" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --automated-smoke-immediate-exit --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${SCENARIO}" --m7-hud-aspect "${ASPECT}" --m7-hud-finish "${FINISH}" "${PALETTE_ARGS[@]}" "${KIT_ARGS[@]}" --m7-hud-controls "${CONTROLS}" --capture-path "${OUTPUT}"
elif [[ "${PALETTE_ENABLED}" == "1" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --automated-smoke-immediate-exit --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${SCENARIO}" --m7-hud-aspect "${ASPECT}" --m7-hud-finish "${FINISH}" "${PALETTE_ARGS[@]}" --m7-hud-controls "${CONTROLS}" --capture-path "${OUTPUT}"
elif [[ "${KIT_ENABLED}" == "1" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --automated-smoke-immediate-exit --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${SCENARIO}" --m7-hud-aspect "${ASPECT}" --m7-hud-finish "${FINISH}" "${KIT_ARGS[@]}" --m7-hud-controls "${CONTROLS}" --capture-path "${OUTPUT}"
else
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --automated-smoke-immediate-exit --m7-hud-lab --m7-hud-smoke --m7-hud-scenario "${SCENARIO}" --m7-hud-aspect "${ASPECT}" --m7-hud-finish "${FINISH}" --m7-hud-controls "${CONTROLS}" --capture-path "${OUTPUT}"
fi
if [[ ! -s "${OUTPUT}" ]] || ! grep -q "M7 HUD LAB: PASS scenarios=8 commands=12 minimap=legal.*factionSkins=4.*schema=8 active=${SCENARIO} finish=${EXPECTED_FINISH} palette=${EXPECTED_PALETTE}.*apertureMasks=4 kitSwitch=interactive kit=${EXPECTED_KIT}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 HUD Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if grep -qE 'SCRIPT ERROR|ERROR:' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 HUD Lab reported a script or runtime error.\n' >&2
  exit 1
fi
printf 'PASS: M7 HUD Lab capture saved to %s\n' "${OUTPUT}"
