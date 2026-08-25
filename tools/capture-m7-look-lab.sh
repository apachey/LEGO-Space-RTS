#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"
OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/m7-look-lab.png}"
CONTROLS="${2:-visible}"
ZOOM="${3:-}"
POST="${4:-}"
OUTLINE="${5:-}"
WORLD="${6:-}"
LOCAL_TIME="${7:-}"
PROFILE="${8:-}"
CAPTURE_FRAME="${9:-24}"
MATERIAL_VIEW="${10:-}"
MATERIAL_AUDIT="${11:-off}"
CAPTURE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-m7-look-lab.XXXXXX")"
trap 'rm -f -- "${CAPTURE_LOG}"' EXIT
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi
if [[ -n "${PROFILE}" ]]; then
  ZOOM="${ZOOM:-profile}"
  POST="${POST:-profile}"
  OUTLINE="${OUTLINE:-profile}"
  WORLD="${WORLD:-profile}"
  LOCAL_TIME="${LOCAL_TIME:-profile}"
  MATERIAL_VIEW="${MATERIAL_VIEW:-profile}"
else
  ZOOM="${ZOOM:-35}"
  POST="${POST:-on}"
  OUTLINE="${OUTLINE:-off}"
  WORLD="${WORLD:-manual}"
  LOCAL_TIME="${LOCAL_TIME:-12}"
  MATERIAL_VIEW="${MATERIAL_VIEW:-combined}"
fi

usage() {
  printf 'Usage: %s [output.png] [visible|hidden] [zoom-cells|profile] [post:on|off|profile] [outline:on|off|profile] [manual|earth|mars|moon|planet-u|underground|profile] [local-time|profile] [profile.json] [capture-frame] [combined|base|color|relief|reflection|profile] [material-audit:on|off]\n' "$0" >&2
}

case "${CONTROLS}" in visible|hidden) ;; *) usage; exit 2 ;; esac
case "${ZOOM}" in
  profile) [[ -n "${PROFILE}" ]] || { usage; exit 2; } ;;
  *)
    [[ "${ZOOM}" =~ ^([0-9]+([.][0-9]*)?|[.][0-9]+)$ ]] || { usage; exit 2; }
    awk -v value="${ZOOM}" 'BEGIN { exit !(value >= 24 && value <= 72) }' || {
      printf 'FAIL: zoom must be between 24 and 72 build cells.\n' >&2; exit 2;
    }
    ;;
esac
case "${POST}" in on|off) ;; profile) [[ -n "${PROFILE}" ]] || { usage; exit 2; } ;; *) usage; exit 2 ;; esac
case "${OUTLINE}" in on|off) ;; profile) [[ -n "${PROFILE}" ]] || { usage; exit 2; } ;; *) usage; exit 2 ;; esac
case "${WORLD}" in manual|earth|mars|moon|planet-u|underground) ;;
  profile) [[ -n "${PROFILE}" ]] || { usage; exit 2; } ;;
  *) usage; exit 2 ;;
esac
if [[ "${LOCAL_TIME}" == profile && -z "${PROFILE}" ]]; then usage; exit 2; fi
if [[ "${LOCAL_TIME}" != profile ]]; then
  [[ "${LOCAL_TIME}" =~ ^([0-9]+([.][0-9]*)?|[.][0-9]+)$ ]] || { usage; exit 2; }
  awk -v value="${LOCAL_TIME}" 'BEGIN { exit !(value >= 0 && value <= 24) }' || {
    printf 'FAIL: local time must be between 0 and 24 hours.\n' >&2; exit 2;
  }
fi
[[ "${CAPTURE_FRAME}" =~ ^[0-9]+$ ]] && (( CAPTURE_FRAME >= 1 && CAPTURE_FRAME <= 600 )) || {
  printf 'FAIL: capture frame must be an integer between 1 and 600.\n' >&2; exit 2;
}
case "${MATERIAL_VIEW}" in combined|base|color|relief|reflection) ;;
  profile) [[ -n "${PROFILE}" ]] || { usage; exit 2; } ;;
  *) printf 'FAIL: unknown material inspection view: %s\n' "${MATERIAL_VIEW}" >&2; exit 2 ;;
esac
case "${MATERIAL_AUDIT}" in on|off) ;;
  *) printf 'FAIL: material audit must be on or off.\n' >&2; exit 2 ;;
esac
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --headless --import --path "${ROOT}/GodotClient"
EXTRA_ARGS=()
if [[ -n "${PROFILE}" ]]; then
  if [[ "${PROFILE}" != /* ]]; then PROFILE="${ROOT}/${PROFILE}"; fi
  EXTRA_ARGS+=(--m7-look-profile "${PROFILE}")
fi
if [[ "${ZOOM}" != profile ]]; then EXTRA_ARGS+=(--m7-look-zoom "${ZOOM}"); fi
if [[ "${POST}" != profile ]]; then EXTRA_ARGS+=(--m7-look-post "${POST}"); fi
if [[ "${OUTLINE}" != profile ]]; then EXTRA_ARGS+=(--m7-look-outline "${OUTLINE}"); fi
if [[ "${WORLD}" != profile ]]; then EXTRA_ARGS+=(--m7-look-world "${WORLD}"); fi
if [[ "${LOCAL_TIME}" != profile ]]; then EXTRA_ARGS+=(--m7-look-time "${LOCAL_TIME}"); fi
if [[ "${MATERIAL_VIEW}" != profile ]]; then EXTRA_ARGS+=(--m7-look-material-view "${MATERIAL_VIEW}"); fi
if [[ "${MATERIAL_AUDIT}" == "on" ]]; then EXTRA_ARGS+=(--m7-look-material-audit); fi
"${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --m7-look-lab --m7-look-smoke --m7-look-controls "${CONTROLS}" --m7-look-capture-frame "${CAPTURE_FRAME}" \
  "${EXTRA_ARGS[@]}" --capture-path "${OUTPUT}"
PASS_PREFIX="M7 LOOK LAB: PASS schema=6 units=4 meshes=192 triangles=31104 buildings=2"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q "${PASS_PREFIX}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if ! grep -q "${PASS_PREFIX}.*controls=${CONTROLS}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply controls=%s.\n' "${CONTROLS}" >&2; exit 1
fi
if [[ "${POST}" != profile ]] && ! grep -q "${PASS_PREFIX}.*post=${POST}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply post=%s.\n' "${POST}" >&2; exit 1
fi
if [[ "${OUTLINE}" != profile ]] && ! grep -q "${PASS_PREFIX}.*outline=${OUTLINE}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply outline=%s.\n' "${OUTLINE}" >&2; exit 1
fi
if [[ "${ZOOM}" != profile ]]; then
  EXPECTED_ZOOM="$(awk -v value="${ZOOM}" 'BEGIN { text=sprintf("%.2f", value); sub(/0+$/, "", text); sub(/[.]$/, "", text); print text }')"
  if ! grep -q "${PASS_PREFIX}.*zoom=${EXPECTED_ZOOM}" "${CAPTURE_LOG}"; then
    printf 'FAIL: M7 Look Lab did not apply zoom=%s.\n' "${EXPECTED_ZOOM}" >&2; exit 1
  fi
fi
if [[ "${WORLD}" != profile ]] && ! grep -q "${PASS_PREFIX}.*world=${WORLD}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply world=%s.\n' "${WORLD}" >&2; exit 1
fi
if [[ "${LOCAL_TIME}" != profile && "${WORLD}" != profile && "${WORLD}" != manual ]]; then
  EXPECTED_TIME="$(awk -v value="${LOCAL_TIME}" 'BEGIN { text=sprintf("%.2f", value); sub(/0+$/, "", text); sub(/[.]$/, "", text); print text }')"
  if ! grep -q "${PASS_PREFIX}.*time=${EXPECTED_TIME}" "${CAPTURE_LOG}"; then
    printf 'FAIL: M7 Look Lab did not apply local time=%s.\n' "${EXPECTED_TIME}" >&2; exit 1
  fi
fi
if [[ "${MATERIAL_VIEW}" != profile ]] && ! grep -q "${PASS_PREFIX}.*materialView=${MATERIAL_VIEW}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply material view=%s.\n' "${MATERIAL_VIEW}" >&2; exit 1
fi
if ! grep -q "${PASS_PREFIX}.*audit=${MATERIAL_AUDIT}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab did not apply material audit=%s.\n' "${MATERIAL_AUDIT}" >&2; exit 1
fi
if grep -qE 'SHADER ERROR|SCRIPT ERROR|ERROR:' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab reported a shader, script, or runtime error.\n' >&2
  exit 1
fi
printf 'PASS: M7 Look Lab capture saved to %s\n' "${OUTPUT}"
