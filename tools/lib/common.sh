#!/usr/bin/env bash

# Shared, repository-local environment discovery for the development harness.

repo_root() {
  cd "$(dirname "${BASH_SOURCE[0]}")/../.." >/dev/null 2>&1 && pwd -P
}

setup_dotnet_environment() {
  export DOTNET_CLI_TELEMETRY_OPTOUT=1
  export DOTNET_NOLOGO=1
  # net8.0 tools can run on this Mac's newer installed runtime without changing
  # their target framework. A native .NET 8 runtime remains preferred when present.
  export DOTNET_ROLL_FORWARD="${DOTNET_ROLL_FORWARD:-Major}"
}

discover_godot() {
  local candidate

  if [[ -n "${GODOT_BIN:-}" && -x "${GODOT_BIN}" ]]; then
    printf '%s\n' "${GODOT_BIN}"
    return 0
  fi

  for candidate in \
    "$(command -v godot4 2>/dev/null || true)" \
    "$(command -v godot 2>/dev/null || true)" \
    "/Applications/Godot_mono.app/Contents/MacOS/Godot" \
    "/Applications/Godot.app/Contents/MacOS/Godot" \
    "${HOME:-}/Applications/Godot_mono.app/Contents/MacOS/Godot" \
    "${HOME:-}/Applications/Godot.app/Contents/MacOS/Godot"
  do
    if [[ -n "${candidate}" && -x "${candidate}" ]]; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done

  return 1
}

godot_is_required_mono() {
  local executable="$1"
  local version
  version="$("${executable}" --version 2>&1 || true)"
  [[ "${version}" == 4.7.1.stable.mono.* ]]
}

godot_template_dir() {
  local version_dir="4.7.1.stable.mono"
  local candidate
  for candidate in \
    "${HOME:-}/Library/Application Support/Godot/export_templates/${version_dir}" \
    "${HOME:-}/.local/share/godot/export_templates/${version_dir}"
  do
    if [[ -d "${candidate}" ]]; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done
  return 1
}

macos_export_template_available() {
  local template_dir
  template_dir="$(godot_template_dir 2>/dev/null || true)"
  [[ -n "${template_dir}" && -s "${template_dir}/macos.zip" ]]
}
