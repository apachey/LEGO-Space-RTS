#!/usr/bin/env bash

set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." >/dev/null 2>&1 && pwd -P)"
git -C "${ROOT}" config core.hooksPath .githooks
CONFIGURED="$(git -C "${ROOT}" config --get core.hooksPath)"
if [[ "${CONFIGURED}" != ".githooks" ]]; then printf 'FAIL: core.hooksPath was not configured.\n' >&2; exit 1; fi
printf 'PASS: repository Git hooks enabled at .githooks\n'
