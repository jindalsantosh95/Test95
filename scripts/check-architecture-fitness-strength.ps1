#Requires -Version 5.1
[CmdletBinding()]
param([switch]$RequireEvidence)
$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
& (Join-Path $PSScriptRoot 'check-architecture-fitness.ps1')
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
if($RequireEvidence){
    $latest=Get-ChildItem -LiteralPath (Join-Path $repoRoot 'TestResults/architecture-fitness') -Filter mutation-evidence.json -File -Recurse -ErrorAction SilentlyContinue|Sort-Object LastWriteTimeUtc -Descending|Select-Object -First 1
    if($null -eq $latest){Write-Error 'No architecture mutation evidence exists.';exit 1}
    $e=Get-Content -LiteralPath $latest.FullName -Raw|ConvertFrom-Json
    if($e.total -lt 5 -or $e.killed -ne $e.total -or $e.score -ne 100){Write-Error 'Latest architecture mutation score is below 100%.';exit 1}
}
Write-Host 'architecture fitness strength check passed' -ForegroundColor Green
exit 0
