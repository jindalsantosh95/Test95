#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Initialize','Acquire','Checkpoint','RecordMutation','MarkAmbiguous','Reconcile','ReplayDiagnostic','Complete')][string]$Action,
    [Parameter(Mandatory)][string]$StatePath,
    [Parameter(Mandatory)][string]$RunId,
    [string]$Workflow = 'unspecified',
    [string]$Scope = 'unspecified',
    [string]$Objective = 'unspecified',
    [string]$BaseCommit = 'unknown',
    [string]$Phase,
    [string]$NextAction,
    [string]$IdempotencyKey,
    [string]$MutationKind,
    [string]$ExternalReference,
    [ValidateSet('Confirmed','Absent','Unknown')][string]$ReconciliationResult = 'Unknown',
    [ValidateSet('Succeeded','Failed','Cancelled','Abandoned')][string]$TerminalState = 'Succeeded',
    [int]$Attempt = 1,
    [int]$MaxAttempts = 1,
    [int]$TimeoutMinutes = 15,
    [int]$StaleAfterMinutes = 60,
    [switch]$TriageApproved
)

$ErrorActionPreference = 'Stop'

function Save-State([object]$State) {
    $directory = Split-Path -Parent $StatePath
    if (-not [string]::IsNullOrWhiteSpace($directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    $State.updatedAtUtc = [datetime]::UtcNow.ToString('o')
    $State | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $StatePath -Encoding utf8
}

function Read-State {
    if (-not (Test-Path -LiteralPath $StatePath -PathType Leaf)) { throw "Execution state does not exist: $StatePath" }
    return Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
}

function Add-Transition([object]$State, [string]$To, [string]$Reason) {
    $transitions = @($State.transitions)
    $transitions += [ordered]@{ from = [string]$State.state; to = $To; reason = $Reason; atUtc = [datetime]::UtcNow.ToString('o') }
    $State.transitions = $transitions
    $State.state = $To
}

function Get-LockPath([string]$ScopeValue) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($ScopeValue)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { $hash = $sha.ComputeHash($bytes) } finally { $sha.Dispose() }
    $name = ([System.BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant().Substring(0, 16)
    return Join-Path (Split-Path -Parent $StatePath) "scope-$name.lock.json"
}

if ($Action -eq 'Initialize') {
    if (Test-Path -LiteralPath $StatePath) { throw "Execution state already exists: $StatePath" }
    if ($Attempt -lt 1 -or $Attempt -gt $MaxAttempts) { throw 'Attempt must be within the configured maximum.' }
    $now = [datetime]::UtcNow.ToString('o')
    $state = [ordered]@{
        schemaVersion = 1; runId = $RunId; workflow = $Workflow; scope = $Scope; objective = $Objective
        baseCommit = $BaseCommit; attempt = $Attempt; maxAttempts = $MaxAttempts; timeoutMinutes = $TimeoutMinutes
        state = 'Queued'; checkpoint = $null; mutations = @(); reconciliations = @(); replays = @(); lastDuplicateSuppressed = $null
        transitions = @([ordered]@{ from = $null; to = 'Queued'; reason = 'initialized'; atUtc = $now })
        createdAtUtc = $now; updatedAtUtc = $now
    }
    Save-State $state
    exit 0
}

$state = Read-State
if ([string]$state.runId -ne $RunId) { throw "Run ID does not match state record '$($state.runId)'." }

switch ($Action) {
    'Acquire' {
        $lockPath = Get-LockPath ([string]$state.scope)
        if (Test-Path -LiteralPath $lockPath -PathType Leaf) {
            $lock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json
            if ([string]$lock.runId -ne $RunId) {
                $age = ([datetime]::UtcNow - ([datetime]$lock.acquiredAtUtc).ToUniversalTime()).TotalMinutes
                if ($age -le $StaleAfterMinutes) { throw "Scope is locked by run '$($lock.runId)'." }
                if (-not $TriageApproved) { throw "Stale scope lock from run '$($lock.runId)' requires explicit triage approval." }
                $reconciliations = @($state.reconciliations)
                $reconciliations += [ordered]@{ kind = 'stale-lock'; previousRunId = [string]$lock.runId; result = 'SupersededAfterTriage'; atUtc = [datetime]::UtcNow.ToString('o') }
                $state.reconciliations = $reconciliations
            }
        }
        [ordered]@{ runId = $RunId; scope = [string]$state.scope; acquiredAtUtc = [datetime]::UtcNow.ToString('o'); statePath = $StatePath } |
            ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $lockPath -Encoding utf8
        Add-Transition $state 'Running' 'scope lock acquired'
    }
    'Checkpoint' {
        if ([string]::IsNullOrWhiteSpace($Phase) -or [string]::IsNullOrWhiteSpace($NextAction)) { throw 'Phase and NextAction are required.' }
        $state.checkpoint = [ordered]@{ phase = $Phase; nextAction = $NextAction; atUtc = [datetime]::UtcNow.ToString('o') }
    }
    'RecordMutation' {
        if ([string]::IsNullOrWhiteSpace($IdempotencyKey) -or [string]::IsNullOrWhiteSpace($MutationKind)) { throw 'IdempotencyKey and MutationKind are required.' }
        $existing = @($state.mutations | Where-Object idempotencyKey -eq $IdempotencyKey)
        if ($existing.Count -eq 0) {
            $mutations = @($state.mutations)
            $mutations += [ordered]@{ idempotencyKey = $IdempotencyKey; kind = $MutationKind; externalReference = $ExternalReference; disposition = 'Recorded'; atUtc = [datetime]::UtcNow.ToString('o') }
            $state.mutations = $mutations
        } else {
            $state.lastDuplicateSuppressed = [ordered]@{ idempotencyKey = $IdempotencyKey; atUtc = [datetime]::UtcNow.ToString('o') }
        }
    }
    'MarkAmbiguous' {
        if ([string]::IsNullOrWhiteSpace($IdempotencyKey)) { throw 'IdempotencyKey is required.' }
        $mutation = @($state.mutations | Where-Object idempotencyKey -eq $IdempotencyKey | Select-Object -First 1)
        if ($mutation.Count -eq 0) { throw "Mutation '$IdempotencyKey' is not recorded." }
        $mutation[0].disposition = 'Ambiguous'
        Add-Transition $state 'WaitingForReview' 'ambiguous external mutation requires reconciliation'
    }
    'Reconcile' {
        if ([string]::IsNullOrWhiteSpace($IdempotencyKey) -or $ReconciliationResult -eq 'Unknown') { throw 'IdempotencyKey and a conclusive ReconciliationResult are required.' }
        $mutation = @($state.mutations | Where-Object idempotencyKey -eq $IdempotencyKey | Select-Object -First 1)
        if ($mutation.Count -eq 0 -or [string]$mutation[0].disposition -ne 'Ambiguous') { throw 'Only an ambiguous recorded mutation can be reconciled.' }
        $mutation[0].disposition = if ($ReconciliationResult -eq 'Confirmed') { 'Confirmed' } else { 'Absent' }
        $reconciliations = @($state.reconciliations)
        $reconciliations += [ordered]@{ idempotencyKey = $IdempotencyKey; result = $ReconciliationResult; externalReference = $ExternalReference; atUtc = [datetime]::UtcNow.ToString('o') }
        $state.reconciliations = $reconciliations
        Add-Transition $state 'Running' 'ambiguous mutation reconciled'
    }
    'ReplayDiagnostic' {
        $canonical = [ordered]@{ runId = [string]$state.runId; objective = [string]$state.objective; scope = [string]$state.scope; baseCommit = [string]$state.baseCommit; workflow = [string]$state.workflow }
        $json = $canonical | ConvertTo-Json -Compress
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try { $hash = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($json)) } finally { $sha.Dispose() }
        $digest = ([System.BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
        $replays = @($state.replays)
        $replays += [ordered]@{ sourceRunId = $RunId; readOnly = $true; inputDigest = $digest; mutationCountBefore = @($state.mutations).Count; atUtc = [datetime]::UtcNow.ToString('o') }
        $state.replays = $replays
    }
    'Complete' {
        if (@($state.mutations | Where-Object disposition -eq 'Ambiguous').Count -gt 0) { throw 'Cannot complete while an ambiguous mutation is unresolved.' }
        Add-Transition $state $TerminalState 'terminal outcome recorded'
        $lockPath = Get-LockPath ([string]$state.scope)
        if (Test-Path -LiteralPath $lockPath -PathType Leaf) {
            $lock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json
            if ([string]$lock.runId -eq $RunId) { Remove-Item -LiteralPath $lockPath }
        }
    }
}

Save-State $state
exit 0
