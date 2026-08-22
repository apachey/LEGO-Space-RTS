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
ZOOM="${3:-35}"
POST="${4:-on}"
OUTLINE="${5:-off}"
CAPTURE_LOG="${TMPDIR:-/tmp}/lego-space-rts-m7-look-lab.log"
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi

case "${CONTROLS}" in visible|hidden) ;; *) printf 'Usage: %s [output.png] [visible|hidden] [zoom-cells] [on|off]\n' "$0" >&2; exit 2 ;; esac
case "${POST}" in on|off) ;; *) printf 'Usage: %s [output.png] [visible|hidden] [zoom-cells] [on|off]\n' "$0" >&2; exit 2 ;; esac
case "${OUTLINE}" in on|off) ;; *) printf 'Usage: %s [output.png] [visible|hidden] [zoom-cells] [on|off] [on|off]\n' "$0" >&2; exit 2 ;; esac
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi

mkdir -p "$(dirname "${OUTPUT}")"
: > "${CAPTURE_LOG}"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --headless --import --path "${ROOT}/GodotClient"
"${GODOT}" --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --m7-look-lab --m7-look-smoke --m7-look-controls "${CONTROLS}" --m7-look-zoom "${ZOOM}" --m7-look-post "${POST}" --m7-look-outline "${OUTLINE}" --capture-path "${OUTPUT}"
if [[ ! -s "${OUTPUT}" ]] || ! grep -q "M7 LOOK LAB: PASS schema=2 units=4 meshes=192 triangles=31104 buildings=2 firing=1 burning=1 controls=${CONTROLS} zoom=${ZOOM} post=${POST} outline=${OUTLINE}" "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if grep -qE 'SHADER ERROR|SCRIPT ERROR|ERROR:' "${CAPTURE_LOG}"; then
  printf 'FAIL: M7 Look Lab reported a shader, script, or runtime error.\n' >&2
  exit 1
fi
printf 'PASS: M7 Look Lab capture saved to %s\n' "${OUTPUT}"
