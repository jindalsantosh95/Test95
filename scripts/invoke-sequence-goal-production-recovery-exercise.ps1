#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-recovery.ps1')
$head = 'a' * 40
Assert-ProductionRemoteHead $head { $head }
function Assert-RecoveryRejected([scriptblock]$Action, [string]$Expected) {
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $_.Exception.Message -like $Expected }
    if (-not $rejected) { throw "Expected recovery rejection: $Expected" }
}
Assert-RecoveryRejected { Assert-ProductionRemoteHead $head { 'b' * 40 } } 'Production remote branch changed*'
$script:polls = 0; $script:delays = 0
$pr = Wait-ProductionHead $head { $head } {
    $script:polls++
    if ($script:polls -eq 1) { return $null }
    @{headRefOid=$(if ($script:polls -eq 2) { 'b' * 40 } else { $head })}
} -Attempts 3 -Delay { $script:delays++ }
if ($pr.headRefOid -cne $head -or $script:polls -ne 3 -or $script:delays -ne 2) { throw 'Stale PR metadata did not converge correctly.' }
Assert-RecoveryRejected { Wait-ProductionHead $head { $head } { @{headRefOid=('b'*40)} } -Attempts 2 -Delay {} } 'PR metadata did not converge*'
$script:reads = 0; $script:polls = 0
Assert-RecoveryRejected {
    Wait-ProductionHead $head { $script:reads++; if ($script:reads -eq 1) { $head } else { 'b'*40 } } { $script:polls++; @{headRefOid=('b'*40)} } -Delay {}
} 'Production remote branch changed*'
if ($script:polls -ne 1) { throw 'PR polling continued after remote movement.' }
Write-Host 'Production publication helper exercise passed: remote identity, missing/stale PR metadata, convergence, exhaustion, and movement during polling.'
# Real Git restart and concurrent-update coverage for goal-document recovery.
& (Join-Path $PSScriptRoot 'invoke-sequence-goal-document-exercise.ps1')
& (Join-Path $PSScriptRoot 'invoke-sequence-goal-git-auth-exercise.ps1')
