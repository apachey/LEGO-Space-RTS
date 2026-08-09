#!/usr/bin/env bash

set -u -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
# shellcheck source=tools/lib/common.sh
source "${SCRIPT_DIR}/lib/common.sh"
ROOT="$(repo_root)"
setup_dotnet_environment

PASS_COUNT=0
WARN_COUNT=0
FAIL_COUNT=0

pass() { PASS_COUNT=$((PASS_COUNT + 1)); printf 'PASS  %s\n' "$*"; }
warn() { WARN_COUNT=$((WARN_COUNT + 1)); printf 'WARN  %s\n' "$*"; }
fail() { FAIL_COUNT=$((FAIL_COUNT + 1)); printf 'FAIL  %s\n' "$*"; }

check_file() {
  local relative="$1"
  local label="$2"
  if [[ -f "${ROOT}/${relative}" ]]; then pass "${label}: ${relative}"; else fail "${label} missing: ${relative}"; fi
}

printf 'LEGO Space RTS development environment doctor\n'
printf 'Repository: %s\n\n' "${ROOT}"

if command -v sw_vers >/dev/null 2>&1; then pass "Host: macOS $(sw_vers -productVersion) ($(uname -m))"; else warn "macOS version could not be detected"; fi

if command -v git >/dev/null 2>&1; then
  pass "Git: $(git --version)"
  BRANCH="$(git -C "${ROOT}" branch --show-current 2>/dev/null || true)"
  if [[ -z "${BRANCH}" ]]; then warn "Git worktree is detached";
  elif [[ "${BRANCH}" == "main" ]]; then warn "Current branch is main; implementation work belongs on a task branch";
  else pass "Task branch: ${BRANCH}"; fi
  HOOKS_PATH="$(git -C "${ROOT}" config --get core.hooksPath 2>/dev/null || true)"
  if [[ "${HOOKS_PATH}" == ".githooks" ]]; then pass "Repository Git safety hooks enabled"; else warn "Repository Git safety hooks are not enabled; run ./tools/setup-git-hooks.sh"; fi
else
  fail "Git is not installed or not on PATH"
fi

if command -v gh >/dev/null 2>&1; then pass "GitHub CLI: $(gh --version | head -1)"; else warn "GitHub CLI is unavailable (only needed for PR convenience)"; fi

if command -v dotnet >/dev/null 2>&1; then
  DOTNET_ACTIVE="$(dotnet --version 2>&1 || true)"
  pass ".NET CLI: $(command -v dotnet) (active SDK ${DOTNET_ACTIVE})"
  DOTNET_SDKS="$(dotnet --list-sdks 2>&1 || true)"
  if printf '%s\n' "${DOTNET_SDKS}" | awk '{split($1,v,"."); if (v[1] >= 8) found=1} END {exit found ? 0 : 1}'; then
    pass "Compatible .NET SDK found (minimum 8): $(printf '%s' "${DOTNET_SDKS}" | tr '\n' ';' | sed 's/;$//')"
  else
    fail "No compatible .NET SDK (8 or newer) is installed"
  fi
  if dotnet --list-runtimes | grep -Eq '^Microsoft.NETCore.App 8\.'; then
    pass "Native .NET 8 runtime installed"
  else
    warn "Native .NET 8 runtime is absent; harness uses DOTNET_ROLL_FORWARD=Major for net8.0 tools"
  fi
else
  fail ".NET CLI is not installed or not on PATH"
fi

GODOT="$(discover_godot 2>/dev/null || true)"
if [[ -z "${GODOT}" ]]; then
  fail "Godot executable was not discovered"
elif godot_is_required_mono "${GODOT}"; then
  pass "Godot 4.7.1 .NET: ${GODOT} ($("${GODOT}" --version 2>&1))"
else
  fail "Discovered Godot is not 4.7.1-stable .NET/Mono: ${GODOT} ($("${GODOT}" --version 2>&1 || true))"
fi

for directory in SimCore SimCore.Tests HeadlessSim tools/ContentCompiler tools/Validation GodotClient Content Tests/Golden Docs/Canon Docs/Development; do
  if [[ -d "${ROOT}/${directory}" ]]; then pass "Repository directory: ${directory}/"; else fail "Repository directory missing: ${directory}/"; fi
done

check_file "LEGO.SpaceRTS.Phase10.sln" "Solution"
check_file "SimCore/LegoSpaceRTS.SimCore.csproj" "SimCore project"
check_file "SimCore.Tests/SimCore.Tests.csproj" "NUnit test project"
check_file "HeadlessSim/HeadlessSim.csproj" "HeadlessSim project"
check_file "tools/ContentCompiler/ContentCompiler.csproj" "ContentCompiler project"
check_file "tools/ContentCompiler/Program.cs" "ContentCompiler entry point"
check_file "tools/Validation/validate_phase10.py" "Static validator"
check_file "Content/PrototypeEntities.json" "Source entity content"
check_file "Content/Maps/DEV_FirstControllableRTS.map.json" "Source M2 map"
check_file "GodotClient/Compiled/PrototypeEntities.contentbin" "Compiled entity content"
check_file "GodotClient/Compiled/DEV_FirstControllableRTS.mapbin" "Compiled M2 map"
check_file "GodotClient/project.godot" "Godot project"
check_file "GodotClient/LEGO.SpaceRTS.Godot.sln" "Godot export solution"
check_file "GodotClient/Scenes/Bootstrap.tscn" "Godot bootstrap scene"
check_file "GodotClient/Scenes/PrototypeRTS.tscn" "PrototypeRTS scene"
check_file "GodotClient/export_presets.cfg" "Godot export presets"
check_file "Tests/Golden/README.md" "Golden/replay policy"
check_file "Docs/ACCEPTANCE_GATES.md" "Phase 10 acceptance gates"
check_file "global.json" ".NET SDK policy"
check_file "tools/verify.sh" "Verification harness"
check_file "tools/run-game.sh" "Game launcher"
check_file "tools/build-mac.sh" "macOS build harness"

if grep -q 'PackageReference Include="NUnit"' "${ROOT}/SimCore.Tests/SimCore.Tests.csproj"; then pass "NUnit test infrastructure detected"; else fail "NUnit package reference missing"; fi
if grep -q '<TargetFramework>netstandard2.1</TargetFramework>' "${ROOT}/SimCore/LegoSpaceRTS.SimCore.csproj"; then pass "SimCore target: netstandard2.1"; else fail "SimCore target framework mismatch"; fi
if grep -q '<TargetFramework>net8.0</TargetFramework>' "${ROOT}/HeadlessSim/HeadlessSim.csproj" && grep -q '<TargetFramework>net8.0</TargetFramework>' "${ROOT}/GodotClient/LEGO.SpaceRTS.Godot.csproj"; then pass "Host/tool target: net8.0"; else fail "Host/tool target framework mismatch"; fi
if grep -q 'run/main_scene="res://Scenes/Bootstrap.tscn"' "${ROOT}/GodotClient/project.godot"; then pass "Godot entry point: Bootstrap.tscn"; else fail "Godot main scene is not Bootstrap.tscn"; fi
if grep -q 'name="macOS"' "${ROOT}/GodotClient/export_presets.cfg"; then pass "Repository macOS export preset configured"; else fail "Repository macOS export preset missing"; fi

TEMPLATE_DIR="$(godot_template_dir 2>/dev/null || true)"
if macos_export_template_available; then
  pass "Godot macOS export template: ${TEMPLATE_DIR}/macos.zip"
elif [[ -n "${TEMPLATE_DIR}" ]]; then
  warn "Godot template directory exists but macos.zip is missing: ${TEMPLATE_DIR}"
else
  warn "Godot 4.7.1-stable.mono export templates are not installed"
fi

printf '\nDoctor summary: PASS=%d WARN=%d FAIL=%d\n' "${PASS_COUNT}" "${WARN_COUNT}" "${FAIL_COUNT}"
if (( FAIL_COUNT > 0 )); then exit 1; fi
