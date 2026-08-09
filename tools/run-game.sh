#!/usr/bin/env bash

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment
GODOT="$(discover_godot 2>/dev/null || true)"

if [[ -z "${GODOT}" ]]; then printf 'FAIL: Godot 4.7.1 .NET was not found. Run ./tools/doctor.sh for details.\n' >&2; exit 1; fi
if ! godot_is_required_mono "${GODOT}"; then printf 'FAIL: %s is not Godot 4.7.1-stable .NET/Mono.\n' "${GODOT}" >&2; exit 1; fi

dotnet restore "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj"
dotnet build "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj" -c Debug --no-restore --disable-build-servers -m:1
printf 'Launching LEGO Space RTS with %s\n' "${GODOT}"
exec "${GODOT}" --path "${ROOT}/GodotClient" "$@"
