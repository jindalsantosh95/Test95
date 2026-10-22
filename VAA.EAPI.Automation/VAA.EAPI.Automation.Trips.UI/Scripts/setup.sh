#!/usr/bin/env bash
set -euo pipefail

script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v pwsh >/dev/null 2>&1; then
  echo "PowerShell 7 is required. Install it before running setup." >&2
  exit 1
fi

exec pwsh -NoProfile -File "$script_directory/Setup-TripsUi.ps1" "$@"
