#Requires -Version 5.1
[CmdletBinding()]
param([switch]$RequireEvidence)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
& (Join-Path $PSScriptRoot 'check-behavior-harness.ps1')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($RequireEvidence) {
    $latest = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'TestResults/behavior-harness') -Filter mutation-evidence.json -File -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $latest) { Write-Error 'No behavior mutation evidence exists.'; exit 1 }
    $evidence = Get-Content -LiteralPath $latest.FullName -Raw | ConvertFrom-Json
    if ($evidence.total -lt 3 -or $evidence.killed -ne $evidence.total -or $evidence.score -ne 100) { Write-Error 'Latest behavior mutation evidence is incomplete or below 100%.'; exit 1 }
}
Write-Host 'behavior harness strength check passed' -ForegroundColor Green
exit 0
