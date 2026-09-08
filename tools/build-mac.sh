#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
VERIFY_ONLY=false
if [[ "${1:-}" == "--verify" ]]; then VERIFY_ONLY=true; shift; fi
if (( $# > 0 )); then printf 'Usage: %s [--verify]\n' "$0" >&2; exit 2; fi

GODOT="$(discover_godot 2>/dev/null || true)"
if [[ -z "${GODOT}" ]] || ! godot_is_required_mono "${GODOT}"; then
  printf 'FAIL: Godot 4.7.1-stable .NET/Mono was not discovered. Run ./tools/doctor.sh.\n' >&2
  exit 1
fi
if ! macos_export_template_available; then
  TEMPLATE_DIR="$(godot_template_dir 2>/dev/null || true)"
  printf 'SKIPPED: Godot 4.7.1-stable.mono macOS export template is missing.\n' >&2
  if [[ -n "${TEMPLATE_DIR}" ]]; then printf 'Expected file: %s/macos.zip\n' "${TEMPLATE_DIR}" >&2; else printf 'Install the official export templates from the Godot editor once, then rerun this command.\n' >&2; fi
  exit 77
fi

OUTPUT_DIR="${ROOT}/Builds/macOS"
OUTPUT_APP="${OUTPUT_DIR}/LEGO Space RTS.app"
EXPORT_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-export.XXXXXX")"
mkdir -p "${OUTPUT_DIR}"
dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
printf 'Exporting debug macOS app to %s\n' "${OUTPUT_APP}"
set +e
"${GODOT}" --headless --path "${ROOT}/GodotClient" --export-debug "macOS" "${OUTPUT_APP}" 2>&1 | tee "${EXPORT_LOG}"
EXPORT_STATUS=${PIPESTATUS[0]}
set -e
if (( EXPORT_STATUS != 0 )) || grep -q '^ERROR:' "${EXPORT_LOG}"; then
  rm -f "${EXPORT_LOG}"
  printf 'FAIL: Godot reported an export error; the app bundle is not certified playable.\n' >&2
  exit 1
fi
rm -f "${EXPORT_LOG}"
APP_EXECUTABLE="$(find "${OUTPUT_APP}/Contents/MacOS" -maxdepth 1 -type f -perm -111 -print -quit 2>/dev/null || true)"
if [[ ! -d "${OUTPUT_APP}" || -z "${APP_EXECUTABLE}" ]]; then
  printf 'FAIL: Godot returned without producing the expected playable app bundle.\n' >&2
  exit 1
fi
codesign --verify --deep --strict "${OUTPUT_APP}"
SMOKE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-app-smoke.XXXXXX")"
SMOKE_ENGINE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-app-smoke-engine.XXXXXX")"
set +e
"${APP_EXECUTABLE}" --headless --log-file "${SMOKE_ENGINE_LOG}" --quit-after 600 -- --smoke 2>&1 | tee "${SMOKE_LOG}"
SMOKE_STATUS=${PIPESTATUS[0]}
set -e
if (( SMOKE_STATUS != 0 )) || ! grep -q 'Prototype content source: compiled runtime data' "${SMOKE_LOG}" || ! grep -q 'PHASE10 GODOT HEADLESS SMOKE: PASS' "${SMOKE_LOG}"; then
  rm -f "${SMOKE_LOG}" "${SMOKE_ENGINE_LOG}"
  printf 'FAIL: exported app did not pass the compiled-content PrototypeRTS smoke.\n' >&2
  exit 1
fi
rm -f "${SMOKE_LOG}" "${SMOKE_ENGINE_LOG}"

CANDIDATE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-m7-acceptance-app-smoke.XXXXXX")"
CANDIDATE_ENGINE_LOG="$(mktemp "${TMPDIR:-/tmp}/lego-space-rts-m7-acceptance-app-engine.XXXXXX")"
set +e
"${APP_EXECUTABLE}" --headless --log-file "${CANDIDATE_ENGINE_LOG}" --quit-after 600 -- \
  --m7-acceptance-candidate --m7-acceptance-smoke --m7-acceptance-zoom 44 \
  --m7-acceptance-look m7-final --m7-acceptance-outline on \
  --m7-acceptance-labels hidden --m7-acceptance-review hidden 2>&1 | tee "${CANDIDATE_LOG}"
CANDIDATE_STATUS=${PIPESTATUS[0]}
set -e
if (( CANDIDATE_STATUS != 0 )) || ! grep -q 'M7 ACCEPTANCE CANDIDATE: PASS factions=4 prototypes=4.*zoom=44.*looks=3 activeLook=m7-final outline=on.*acceptance=m7-final-outline-on' "${CANDIDATE_LOG}"; then
  rm -f "${CANDIDATE_LOG}" "${CANDIDATE_ENGINE_LOG}"
  printf 'FAIL: exported app did not pass the accepted M7 visual-direction smoke.\n' >&2
  exit 1
fi
rm -f "${CANDIDATE_LOG}" "${CANDIDATE_ENGINE_LOG}"
if [[ "${VERIFY_ONLY}" == true ]]; then printf 'PASS: macOS export smoke produced a launchable app bundle.\n'; else printf 'PASS: playable debug build created at %s\n' "${OUTPUT_APP}"; fi
