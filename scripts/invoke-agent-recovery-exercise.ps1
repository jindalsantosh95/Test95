#Requires -Version 5.1
[CmdletBinding()]
param([string]$RunId = "recovery-$([guid]::NewGuid().ToString('N'))")

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$root = Join-Path $repoRoot "TestResults/agent-recovery/$RunId"
$statePath = Join-Path $root 'execution-state.json'
$tool = Join-Path $PSScriptRoot 'agent-execution-state.ps1'
New-Item -ItemType Directory -Path $root -Force | Out-Null

& $tool Initialize $statePath $RunId -Workflow 'recovery-exercise' -Scope 'docs' -Objective 'Prove bounded recovery and diagnostic replay' -BaseCommit ((& git -C $repoRoot rev-parse HEAD).Trim()) -MaxAttempts 2 -TimeoutMinutes 5
& $tool Acquire $statePath $RunId
& $tool Checkpoint $statePath $RunId -Phase 'before-external-write' -NextAction 'record idempotent issue mutation'
& $tool RecordMutation $statePath $RunId -IdempotencyKey 'issue:docs-recovery-exercise' -MutationKind 'simulated-issue' -ExternalReference 'simulated/1'
& $tool MarkAmbiguous $statePath $RunId -IdempotencyKey 'issue:docs-recovery-exercise'
& $tool Reconcile $statePath $RunId -IdempotencyKey 'issue:docs-recovery-exercise' -ReconciliationResult Confirmed -ExternalReference 'simulated/1'
& $tool RecordMutation $statePath $RunId -IdempotencyKey 'issue:docs-recovery-exercise' -MutationKind 'simulated-issue' -ExternalReference 'simulated/1'
& $tool ReplayDiagnostic $statePath $RunId
& $tool Complete $statePath $RunId -TerminalState Succeeded

# Prove stale locks fail closed and can only be superseded with explicit triage.
$staleAPath = Join-Path $root 'stale-a.json'
$staleBPath = Join-Path $root 'stale-b.json'
& $tool Initialize $staleAPath "$RunId-stale-a" -Workflow 'recovery-exercise' -Scope 'docs/stale-lock' -Objective 'Create stale lock' -MaxAttempts 1
& $tool Acquire $staleAPath "$RunId-stale-a"
$lockPath = Get-ChildItem -LiteralPath $root -Filter 'scope-*.lock.json' -File | Select-Object -First 1 -ExpandProperty FullName
$lock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json
$lock.acquiredAtUtc = [datetime]::UtcNow.AddHours(-2).ToString('o')
$lock | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $lockPath -Encoding utf8
& $tool Initialize $staleBPath "$RunId-stale-b" -Workflow 'recovery-exercise' -Scope 'docs/stale-lock' -Objective 'Recover stale lock' -MaxAttempts 1
$blockedWithoutTriage = $false
try { & $tool Acquire $staleBPath "$RunId-stale-b" -StaleAfterMinutes 30 }
catch { $blockedWithoutTriage = $true }
if (-not $blockedWithoutTriage) { throw 'Stale lock acquisition did not fail closed before triage.' }
& $tool Acquire $staleBPath "$RunId-stale-b" -StaleAfterMinutes 30 -TriageApproved
& $tool Complete $staleBPath "$RunId-stale-b" -TerminalState Succeeded

$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$failures = @()
if ($state.state -ne 'Succeeded') { $failures += 'terminal state is not Succeeded' }
if (@($state.mutations).Count -ne 1) { $failures += 'duplicate mutation was not suppressed' }
if (@($state.reconciliations | Where-Object result -eq 'Confirmed').Count -ne 1) { $failures += 'ambiguous mutation was not reconciled' }
if (@($state.replays | Where-Object readOnly -eq $true).Count -ne 1) { $failures += 'read-only diagnostic replay was not recorded' }
if ([string]::IsNullOrWhiteSpace([string]$state.lastDuplicateSuppressed.idempotencyKey)) { $failures += 'duplicate suppression evidence is missing' }
if (@($state.replays)[0].mutationCountBefore -ne @($state.mutations).Count) { $failures += 'diagnostic replay changed mutation state' }
$staleB = Get-Content -LiteralPath $staleBPath -Raw | ConvertFrom-Json
if (@($staleB.reconciliations | Where-Object result -eq 'SupersededAfterTriage').Count -ne 1) { $failures += 'stale lock triage evidence is missing' }

if ($failures.Count -gt 0) { throw "Recovery exercise failed: $($failures -join '; ')" }
Write-Host "agent recovery exercise passed: TestResults/agent-recovery/$RunId/execution-state.json" -ForegroundColor Green
exit 0
