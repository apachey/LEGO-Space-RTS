#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment

OUTPUT="${1:-${ROOT}/Artifacts/Screenshots/m85-t081-asset-pipeline.png}"
ZOOM="${2:-44}"
CAPTURE_FRAME="${3:-60}"
YAW="${4:-36}"
GODOT="$(discover_godot 2>/dev/null || true)"
CAPTURE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-m85-pipeline-capture.XXXXXX")"
trap 'rm -f -- "${CAPTURE_LOG}"' EXIT

[[ "${ZOOM}" =~ ^(24|44|72)$ ]] || { printf 'Usage: %s [output.png] [zoom:24|44|72] [capture-frame]\n' "$0" >&2; exit 2; }
[[ "${CAPTURE_FRAME}" =~ ^[0-9]+$ ]] && (( CAPTURE_FRAME >= 20 && CAPTURE_FRAME <= 300 )) || {
  printf 'FAIL: capture frame must be an integer between 20 and 300.\n' >&2
  exit 2
}
[[ "${YAW}" =~ ^-?[0-9]+([.][0-9]+)?$ ]] || {
  printf 'FAIL: camera yaw must be a number in degrees.\n' >&2
  exit 2
}
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1 .NET was not found.\n' >&2
  exit 1
fi
if [[ "${OUTPUT}" != /* ]]; then OUTPUT="${ROOT}/${OUTPUT}"; fi

mkdir -p "$(dirname "${OUTPUT}")"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
"${GODOT}" --disable-crash-handler --log-file "${CAPTURE_LOG}" --quit-after 600 --path "${ROOT}/GodotClient" -- \
  --automated-smoke-immediate-exit --m85-asset-pipeline --m85-asset-pipeline-smoke --m85-asset-pipeline-zoom "${ZOOM}" \
  --m85-asset-pipeline-yaw "${YAW}" --m85-asset-pipeline-capture-frame "${CAPTURE_FRAME}" \
  --capture-path "${OUTPUT}"

if [[ ! -s "${OUTPUT}" ]] || ! grep -q 'M8.5 ASSET PIPELINE: PASS source=blend export=glb import=PackedScene' "${CAPTURE_LOG}"; then
  printf 'FAIL: T081 asset-pipeline capture or PASS marker was not produced.\n' >&2
  exit 1
fi
if grep -qE 'SHADER ERROR|SCRIPT ERROR|ERROR:' "${CAPTURE_LOG}"; then
  printf 'FAIL: T081 asset-pipeline capture reported a shader or script error.\n' >&2
  exit 1
fi
printf 'PASS: T081 asset-pipeline capture saved to %s\n' "${OUTPUT}"
