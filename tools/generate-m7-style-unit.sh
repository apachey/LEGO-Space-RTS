#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
BLENDER="/Applications/Blender.app/Contents/MacOS/Blender"
OUTPUT="${ROOT}/GodotClient/Assets/M7/raider_drill_rig.glb"
SOURCE_BLEND="${ROOT}/ArtSource/M7/raider_drill_rig.blend"

if [[ ! -x "${BLENDER}" ]]; then
  printf 'FAIL: Blender 4.4 was not found at %s\n' "${BLENDER}" >&2
  exit 1
fi

"${BLENDER}" --background --python "${ROOT}/tools/blender/generate_m7_style_unit.py" -- "${OUTPUT}" "${SOURCE_BLEND}"
if [[ ! -s "${OUTPUT}" ]]; then
  printf 'FAIL: M7 Style Lab GLB was not produced.\n' >&2
  exit 1
fi
printf 'PASS: M7 Style Lab unit generated at %s\n' "${OUTPUT}"
printf 'PASS: Editable Blender source saved to %s\n' "${SOURCE_BLEND}"
