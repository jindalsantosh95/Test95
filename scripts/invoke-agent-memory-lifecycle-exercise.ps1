#Requires -Version 5.1
[CmdletBinding()]
param([string]$EvidenceDirectory)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) {
    $EvidenceDirectory = Join-Path $repoRoot ('TestResults/agent-memory/' + [guid]::NewGuid().ToString('N'))
}
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$statePath = Join-Path $EvidenceDirectory 'session-state.json'
$summaryPath = Join-Path $EvidenceDirectory 'summary.json'
$lifecycle = Join-Path $PSScriptRoot 'update-agent-memory-state.ps1'

& $lifecycle -Action Initialize -StatePath $statePath -Objective 'Exercise durable memory' -ScopeKind ScopePath -ScopePath docs -NextActions 'Create checkpoint' -Actor 'memory-exercise'
if ($LASTEXITCODE -ne 0) { throw 'Memory initialization failed.' }
& $lifecycle -Action Checkpoint -StatePath $statePath -NextActions 'Run verification', 'Resume in a new process' -Actor 'memory-exercise'
& $lifecycle -Action RecordVerification -StatePath $statePath -VerificationCommand 'scripts/verify.ps1 -ScopePath docs -SkipBuild -SkipDeploymentCheck' -VerificationResult pass -Actor 'memory-exercise'

$beforeResume = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$processArguments = "-NoProfile -ExecutionPolicy Bypass -File `"$lifecycle`" -Action Resume -StatePath `"$statePath`" -Actor memory-exercise-resumer"
$processParameters = @{
    FilePath = (Get-Process -Id $PID).Path
    ArgumentList = $processArguments
    Wait = $true
    PassThru = $true
}
if ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT) {
    $processParameters.WindowStyle = 'Hidden'
}
$newProcess = Start-Process @processParameters
if ($newProcess.ExitCode -ne 0) { throw 'Cross-process memory resume failed.' }
$afterResume = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json

$digestBeforeRejectedTransition = (Get-FileHash -LiteralPath $statePath -Algorithm SHA256).Hash
$rejected = $false
try { & $lifecycle -Action RecordVerification -StatePath $statePath -Actor 'memory-exercise' } catch { $rejected = $true }
$digestAfterRejectedTransition = (Get-FileHash -LiteralPath $statePath -Algorithm SHA256).Hash

& $lifecycle -Action Block -StatePath $statePath -Blockers 'Synthetic approval required' -NextActions 'Resume after approval' -Actor 'memory-exercise'
$blocked = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
& $lifecycle -Action Resume -StatePath $statePath -Actor 'memory-exercise-resumer'
$resumedAfterBlock = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
& $lifecycle -Action Complete -StatePath $statePath -Actor 'memory-exercise'
$completed = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
& $lifecycle -Action Resume -StatePath $statePath -Actor 'memory-exercise-next-session'
$rotated = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json

$checks = [ordered]@{
    schemaVersion2 = [int]$afterResume.schemaVersion -eq 2
    revisionAdvancedAcrossProcess = [int]$afterResume.revision -eq ([int]$beforeResume.revision + 1)
    resumeCountAdvanced = [int]$afterResume.resumeCount -eq 1
    sessionIdentityPreserved = [string]$afterResume.sessionId -eq [string]$beforeResume.sessionId
    checkpointPreserved = @($afterResume.nextActions) -contains 'Run verification'
    verificationPreserved = [string]$afterResume.lastVerification.result -eq 'pass'
    invalidTransitionRejected = $rejected
    rejectedTransitionPreservedState = $digestBeforeRejectedTransition -eq $digestAfterRejectedTransition
    blockedStatePersisted = [string]$blocked.status -eq 'blocked' -and @($blocked.blockers) -contains 'Synthetic approval required'
    blockedSessionResumed = [string]$resumedAfterBlock.status -eq 'active' -and [string]$resumedAfterBlock.sessionId -eq [string]$afterResume.sessionId
    completedStatePersisted = [string]$completed.status -eq 'completed' -and [string]$completed.sessionId -eq [string]$afterResume.sessionId
    completedSessionRotated = [string]$rotated.status -eq 'active' -and [string]$rotated.sessionId -ne [string]$completed.sessionId -and [string]$rotated.history[0].event -eq 'started-after-completion'
}
$passed = @($checks.GetEnumerator() | Where-Object { -not $_.Value }).Count -eq 0
$summary = [ordered]@{
    schemaVersion = 1
    capability = 'memory-resume'
    passed = $passed
    exercisedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    statePath = $statePath
    initialRevision = [int]$beforeResume.revision
    resumedRevision = [int]$afterResume.revision
    checks = $checks
}
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
if (-not $passed) { Write-Host 'agent memory lifecycle exercise failed' -ForegroundColor Red; exit 1 }
Write-Host "agent memory lifecycle exercise passed: $($checks.Count)/$($checks.Count) checks; cross-process revision $($beforeResume.revision) -> $($afterResume.revision)" -ForegroundColor Green
Write-Host "evidence: $summaryPath" -ForegroundColor DarkGray
exit 0
