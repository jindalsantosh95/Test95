#Requires -Version 5.1
[CmdletBinding()]
param([switch]$RequireEvidence)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = [System.Collections.Generic.List[string]]::new()
foreach ($required in @('scripts/invoke-self-correcting-loop.ps1','scripts/invoke-self-correcting-loop-exercise.ps1')) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $required) -PathType Leaf)) { $failures.Add("Missing self-correcting loop artifact: $required") }
}
$runner = Get-Content -LiteralPath (Join-Path $repoRoot 'scripts/invoke-self-correcting-loop.ps1') -Raw
foreach ($marker in @('MaxValidationAttempts','TimeoutMinutes','validation attempt budget exhausted','correction command failed','phases','failureReason')) {
    if ($runner -notmatch [regex]::Escape($marker)) { $failures.Add("Self-correcting runner is missing $marker.") }
}
if ($RequireEvidence) {
    $latest = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'TestResults/self-correcting-loop') -Filter success-evidence.json -File -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $latest) { $failures.Add('No self-correcting loop exercise evidence exists.') }
    else {
        $evidence = Get-Content -LiteralPath $latest.FullName -Raw | ConvertFrom-Json
        if ($evidence.state -ne 'Succeeded' -or $evidence.correctionCount -lt 1) { $failures.Add('Latest self-correcting loop evidence did not demonstrate correction.') }
    }
}
if ($failures.Count -gt 0) {
    Write-Host 'self-correcting loop check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'self-correcting loop check passed' -ForegroundColor Green
exit 0
