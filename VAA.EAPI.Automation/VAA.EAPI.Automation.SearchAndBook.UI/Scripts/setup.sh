#!/usr/bin/env bash

set -euo pipefail

script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK 8 or later is required. Install it from https://dotnet.microsoft.com/download/dotnet/8.0 and run setup again." >&2
  exit 1
fi

if ! command -v pwsh >/dev/null 2>&1; then
  echo "PowerShell 7 is required to run the shared setup and TRX analyser." >&2
  if command -v brew >/dev/null 2>&1; then
    echo "Install it with: brew install --cask powershell" >&2
  else
    echo "Install it from: https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-macos" >&2
  fi
  exit 1
fi

exec pwsh -NoProfile -File "$script_directory/Setup-SearchAndBookUi.ps1" "$@"
