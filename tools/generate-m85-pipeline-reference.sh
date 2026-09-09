#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
BLENDER="/Applications/Blender.app/Contents/MacOS/Blender"
OUTPUT="${ROOT}/GodotClient/Assets/M85/PipelineReference/pipeline_reference_vehicle.glb"
SOURCE_BLEND="${ROOT}/ArtSource/M85/PipelineReference/pipeline_reference_vehicle.blend"

if [[ ! -x "${BLENDER}" ]]; then
  printf 'FAIL: Blender 4.4+ was not found at %s\n' "${BLENDER}" >&2
  exit 1
fi

"${BLENDER}" --background --python "${ROOT}/tools/blender/generate_m85_pipeline_reference.py" -- "${OUTPUT}" "${SOURCE_BLEND}"
if [[ ! -s "${OUTPUT}" || ! -s "${SOURCE_BLEND}" ]]; then
  printf 'FAIL: M8.5 pipeline source or GLB was not produced.\n' >&2
  exit 1
fi
printf 'PASS: M8.5 pipeline GLB generated at %s\n' "${OUTPUT}"
printf 'PASS: Editable Blender source saved to %s\n' "${SOURCE_BLEND}"
