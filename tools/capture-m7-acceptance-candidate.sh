#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment

OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/m7-visual-acceptance-44.png}"
ZOOM="${2:-84}"
LABELS="${3:-visible}"
HUD="${4:-visible}"
REVIEW="${5:-hidden}"
CAPTURE_FRAME="${6:-90}"
DESTRUCTION="${7:-off}"
LOOK="${8:-m7-final}"
OUTLINE="${9:-on}"
GODOT="$(discover_godot 2>/dev/null || true)"
CAPTURE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-m7-acceptance.XXXXXX")"
trap 'rm -f -- "${CAPTURE_LOG}"' EXIT

usage() {
  printf 'Usage: %s [output.png] [zoom:24..108] [labels:visible|hidden] [hud:visible|hidden] [review:visible|hidden] [capture-frame] [destruction:on|off] [current|m7-final|hybrid] [outline:on|off]\n' "$0" >&2
}

[[ "${ZOOM}" =~ ^[0-9]+$ ]] && (( ZOOM >= 24 && ZOOM <= 108 )) || { usage; exit 2; }
case "${LABELS}" in visible|hidden) ;; *) usage; exit 2 ;; esac
case "${HUD}" in visible|hidden) ;; *) usage; exit 2 ;; esac
case "${REVIEW}" in visible|hidden) ;; *) usage; exit 2 ;; esac
case "${DESTRUCTION}" in on|off) ;; *) usage; exit 2 ;; esac
case "${LOOK}" in current|m7-final|hybrid) ;; *) usage; exit 2 ;; esac
case "${OUTLINE}" in on|off) ;; *) usage; exit 2 ;; esac
[[ "${CAPTURE_FRAME}" =~ ^[0-9]+$ ]] && (( CAPTURE_FRAME >= 24 && CAPTURE_FRAME <= 600 )) || {
  printf 'FAIL: capture frame must be an integer between 24 and 600.\n' >&2
  exit 2
}
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi

mkdir -p "$(dirname "${OUTPUT}")"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
if [[ "${DESTRUCTION}" == "on" ]]; then
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --m7-acceptance-candidate --m7-acceptance-smoke --m7-acceptance-zoom "${ZOOM}" \
    --m7-acceptance-labels "${LABELS}" --m7-acceptance-hud "${HUD}" --m7-acceptance-review "${REVIEW}" \
    --m7-acceptance-capture-frame "${CAPTURE_FRAME}" --m7-acceptance-look "${LOOK}" \
    --m7-acceptance-outline "${OUTLINE}" --m7-acceptance-destruction --capture-path "${OUTPUT}"
else
  "${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
    --m7-acceptance-candidate --m7-acceptance-smoke --m7-acceptance-zoom "${ZOOM}" \
    --m7-acceptance-labels "${LABELS}" --m7-acceptance-hud "${HUD}" --m7-acceptance-review "${REVIEW}" \
    --m7-acceptance-capture-frame "${CAPTURE_FRAME}" --m7-acceptance-look "${LOOK}" \
    --m7-acceptance-outline "${OUTLINE}" --capture-path "${OUTPUT}"
fi

PASS_PREFIX="M7 ACCEPTANCE CANDIDATE: PASS factions=4 prototypes=4"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q "${PASS_PREFIX}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 acceptance-candidate capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if ! grep -q "${PASS_PREFIX}.*zoom=${ZOOM}.*looks=3 activeLook=${LOOK} outline=${OUTLINE}.*acceptance=m7-final-outline-on" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 acceptance candidate did not preserve zoom=%s look=%s outline=%s and accepted direction.\n' "${ZOOM}" "${LOOK}" "${OUTLINE}" >&2
  exit 1
fi
if grep -qE 'SHADER ERROR|SCRIPT ERROR|ERROR: Shader compilation failed' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 acceptance candidate reported a shader or script error.\n' >&2
  exit 1
fi
printf 'PASS: M7 acceptance-candidate capture saved to %s\n' "${OUTPUT}"
