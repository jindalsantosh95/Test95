#Requires -Version 5.1
[CmdletBinding()]
param([switch]$RequireEvidence)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = [System.Collections.Generic.List[string]]::new()
foreach ($required in @('scripts/agent-execution-state.ps1','scripts/invoke-agent-recovery-exercise.ps1','docs/agent-execution-and-recovery.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $required) -PathType Leaf)) { $failures.Add("Missing recovery artifact: $required") }
}
$toolText = Get-Content -LiteralPath (Join-Path $repoRoot 'scripts/agent-execution-state.ps1') -Raw
foreach ($marker in @('Acquire','Checkpoint','RecordMutation','MarkAmbiguous','Reconcile','ReplayDiagnostic','Complete','TriageApproved','IdempotencyKey','maxAttempts','timeoutMinutes')) {
    if ($toolText -notmatch [regex]::Escape($marker)) { $failures.Add("Recovery state manager is missing $marker support.") }
}
if ($RequireEvidence) {
    $evidenceRoot = Join-Path $repoRoot 'TestResults/agent-recovery'
    $latest = Get-ChildItem -LiteralPath $evidenceRoot -Filter execution-state.json -File -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $latest) { $failures.Add('No recovery exercise evidence exists.') }
    else {
        $state = Get-Content -LiteralPath $latest.FullName -Raw | ConvertFrom-Json
        if ($state.state -ne 'Succeeded') { $failures.Add('Latest recovery exercise did not succeed.') }
        if (@($state.mutations).Count -ne 1 -or @($state.reconciliations).Count -lt 1 -or @($state.replays).Count -lt 1) { $failures.Add('Latest recovery evidence is incomplete.') }
    }
}
if ($failures.Count -gt 0) {
    Write-Host 'agent recovery check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'agent recovery check passed' -ForegroundColor Green
exit 0
