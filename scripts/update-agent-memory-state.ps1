#Requires -Version 5.1
<#
.SYNOPSIS
    Performs atomic, revisioned lifecycle transitions for local agent memory.

.DESCRIPTION
    Owns the ignored .codex/state/session-state.json lifecycle. All writes are
    protected by a named mutex and use an atomic same-directory replacement.
    -StatePath exists so deterministic exercises can use isolated evidence.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Initialize', 'Resume', 'Checkpoint', 'RecordVerification', 'Block', 'Complete')]
    [string]$Action,
    [string]$StatePath,
    [string]$Objective,
    [ValidateSet('SolutionPath', 'ProjectPath', 'ScopePath')]
    [string]$ScopeKind,
    [string]$ScopePath,
    [string[]]$NextActions,
    [string[]]$Blockers,
    [string]$VerificationCommand,
    [ValidateSet('pass', 'fail', 'not_run')]
    [string]$VerificationResult,
    [string]$Actor
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($StatePath)) {
    $StatePath = Join-Path $repoRoot '.codex/state/session-state.json'
} elseif (-not [System.IO.Path]::IsPathRooted($StatePath)) {
    $StatePath = Join-Path $repoRoot $StatePath
}

if ([string]::IsNullOrWhiteSpace($Actor)) {
    $Actor = if (-not [string]::IsNullOrWhiteSpace($env:USERNAME)) { $env:USERNAME } elseif (-not [string]::IsNullOrWhiteSpace($env:USER)) { $env:USER } else { 'local-agent' }
}

function Get-UtcNow { return (Get-Date).ToUniversalTime().ToString('o') }

function Get-PathHash([string]$Value) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.ToLowerInvariant())
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '')
    } finally { $sha.Dispose() }
}

function New-State([string]$Now) {
    $resolvedObjective = if ([string]::IsNullOrWhiteSpace($Objective)) { 'Continue work in this repository worktree' } else { $Objective }
    $resolvedKind = if ([string]::IsNullOrWhiteSpace($ScopeKind)) { 'ScopePath' } else { $ScopeKind }
    $resolvedPath = if ([string]::IsNullOrWhiteSpace($ScopePath)) { '.' } else { $ScopePath }
    $resolvedNext = @(if ($null -eq $NextActions -or $NextActions.Count -eq 0) { 'Review the worktree and continue the active objective' } else { $NextActions })
    return [ordered]@{
        schemaVersion = 2
        sessionId = [guid]::NewGuid().ToString('D')
        revision = 1
        objective = $resolvedObjective
        status = 'active'
        scope = [ordered]@{ kind = $resolvedKind; path = $resolvedPath }
        nextActions = [object[]]$resolvedNext
        blockers = @()
        lastVerification = [ordered]@{ command = 'not run'; result = 'not_run'; ranAtUtc = $Now }
        createdAtUtc = $Now
        lastResumedAtUtc = $Now
        resumeCount = 0
        updatedAtUtc = $Now
        updatedBy = $Actor
        history = @([ordered]@{ revision = 1; event = 'initialized'; atUtc = $Now; actor = $Actor })
    }
}

function Assert-Strings([object[]]$Values, [string]$Name) {
    foreach ($value in @($Values)) {
        if ([string]::IsNullOrWhiteSpace([string]$value)) { throw "$Name entries must be non-empty strings." }
    }
}

function Add-History([object]$State, [string]$Event, [string]$Now) {
    $State.revision = [int]$State.revision + 1
    $entries = @($State.history) + [pscustomobject][ordered]@{ revision = $State.revision; event = $Event; atUtc = $Now; actor = $Actor }
    $State.history = @($entries | Select-Object -Last 20)
    $State.updatedAtUtc = $Now
    $State.updatedBy = $Actor
}

function Write-AtomicJson([object]$State) {
    $directory = Split-Path -Parent $StatePath
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    $tempPath = Join-Path $directory ('.session-state.' + [guid]::NewGuid().ToString('N') + '.tmp')
    $backupPath = Join-Path $directory ('.session-state.' + [guid]::NewGuid().ToString('N') + '.bak')
    try {
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($tempPath, (($State | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)
        if (Test-Path -LiteralPath $StatePath -PathType Leaf) {
            [System.IO.File]::Replace($tempPath, $StatePath, $backupPath)
        } else {
            Move-Item -LiteralPath $tempPath -Destination $StatePath
        }
    } finally {
        if (Test-Path -LiteralPath $tempPath -PathType Leaf) { Remove-Item -LiteralPath $tempPath -Force }
        if (Test-Path -LiteralPath $backupPath -PathType Leaf) { Remove-Item -LiteralPath $backupPath -Force }
    }
}

$mutexName = 'VaaEapiAgentMemory_' + (Get-PathHash -Value ([System.IO.Path]::GetFullPath($StatePath)))
$mutex = New-Object System.Threading.Mutex($false, $mutexName)
$acquired = $false
try {
    $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds(10))
    if (-not $acquired) { throw 'Timed out waiting for the agent-memory state lock.' }

    $now = Get-UtcNow
    $exists = Test-Path -LiteralPath $StatePath -PathType Leaf
    if ($Action -eq 'Initialize') {
        if ($exists) {
            Write-Host "agent memory already initialized: $StatePath" -ForegroundColor DarkGray
            exit 0
        }
        $state = New-State -Now $now
        Write-AtomicJson -State $state
        Write-Host "agent memory initialized (revision 1): $StatePath" -ForegroundColor Green
        exit 0
    }

    if (-not $exists) {
        $state = New-State -Now $now
        Write-AtomicJson -State $state
    }

    try { $state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json } catch { throw "Agent memory is invalid JSON and was left unchanged: $StatePath" }
    if ([int]$state.schemaVersion -ne 2 -or [int]$state.revision -lt 1) { throw 'Agent memory must use schemaVersion 2 with a positive revision; run the migration documented in docs/agent-memory.md.' }

    switch ($Action) {
        'Resume' {
            if ([string]$state.status -eq 'completed') {
                $state = New-State -Now $now
                if ([string]::IsNullOrWhiteSpace($Objective)) { $state.objective = 'Start the next work item in this repository worktree' }
                $state.history[0].event = 'started-after-completion'
            } else {
                if (-not [string]::IsNullOrWhiteSpace($Objective)) { $state.objective = $Objective }
                if (-not [string]::IsNullOrWhiteSpace($ScopeKind)) { $state.scope.kind = $ScopeKind }
                if (-not [string]::IsNullOrWhiteSpace($ScopePath)) { $state.scope.path = $ScopePath }
                $state.status = 'active'
                $state.lastResumedAtUtc = $now
                $state.resumeCount = [int]$state.resumeCount + 1
                Add-History -State $state -Event 'resumed' -Now $now
            }
        }
        'Checkpoint' {
            if (-not [string]::IsNullOrWhiteSpace($Objective)) { $state.objective = $Objective }
            if (-not [string]::IsNullOrWhiteSpace($ScopeKind)) { $state.scope.kind = $ScopeKind }
            if (-not [string]::IsNullOrWhiteSpace($ScopePath)) { $state.scope.path = $ScopePath }
            if ($null -ne $NextActions -and $NextActions.Count -gt 0) { Assert-Strings $NextActions 'nextActions'; $state.nextActions = [object[]]@($NextActions) }
            if ($null -ne $Blockers) { Assert-Strings $Blockers 'blockers'; $state.blockers = @($Blockers) }
            Add-History -State $state -Event 'checkpointed' -Now $now
        }
        'RecordVerification' {
            if ([string]::IsNullOrWhiteSpace($VerificationCommand) -or [string]::IsNullOrWhiteSpace($VerificationResult)) { throw 'RecordVerification requires -VerificationCommand and -VerificationResult.' }
            $state.lastVerification = [ordered]@{ command = $VerificationCommand; result = $VerificationResult; ranAtUtc = $now }
            Add-History -State $state -Event "verification-$VerificationResult" -Now $now
        }
        'Block' {
            if ($null -eq $Blockers -or $Blockers.Count -eq 0) { throw 'Block requires at least one -Blockers entry.' }
            Assert-Strings $Blockers 'blockers'
            $state.status = 'blocked'; $state.blockers = @($Blockers)
            if ($null -ne $NextActions -and $NextActions.Count -gt 0) { Assert-Strings $NextActions 'nextActions'; $state.nextActions = [object[]]@($NextActions) }
            Add-History -State $state -Event 'blocked' -Now $now
        }
        'Complete' {
            $state.status = 'completed'; $state.blockers = @(); $state.nextActions = [object[]]@('Start a new session for the next work item')
            Add-History -State $state -Event 'completed' -Now $now
        }
    }

    Write-AtomicJson -State $state
    Write-Host "agent memory $($Action.ToLowerInvariant()) (revision $($state.revision)): $StatePath" -ForegroundColor Green
} finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
