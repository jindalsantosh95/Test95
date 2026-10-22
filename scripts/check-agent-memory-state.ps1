#Requires -Version 5.1
<#
.SYNOPSIS
    Validates the repository agent-memory contract and optional local state.

.DESCRIPTION
    Always validates the tracked session-state example. When an untracked local
    session-state file exists, validates its shape and freshness as well.

.PARAMETER Initialize
    Creates revisioned local session state through the lifecycle command when
    one does not already exist.
#>
[CmdletBinding()]
param(
    [int]$MaxAgeDays = 30,
    [switch]$Initialize
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$examplePath = Join-Path $repoRoot '.codex/state/session-state.example.json'
$localStatePath = Join-Path $repoRoot '.codex/state/session-state.json'
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)
    $failures.Add($Message) | Out-Null
}

function Require-String {
    param([object]$Value, [string]$Name, [string]$Artifact)
    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        Add-Failure "$Artifact`: $Name must be a non-empty string."
    }
}

function Require-Array {
    param([object]$Value, [string]$Name, [string]$Artifact)
    if ($null -eq $Value -or -not ($Value -is [System.Array])) {
        Add-Failure "$Artifact`: $Name must be an array."
        return
    }

    foreach ($item in @($Value)) {
        if ($null -eq $item -or [string]::IsNullOrWhiteSpace([string]$item)) {
            Add-Failure "$Artifact`: $Name entries must be non-empty strings."
            break
        }
    }
}

function Try-ParseDateTimeOffset {
    param([string]$Value, [ref]$Parsed)
    try {
        $Parsed.Value = [DateTimeOffset]::Parse($Value, [System.Globalization.CultureInfo]::InvariantCulture)
        return $true
    } catch {
        return $false
    }
}

function Test-IsUtcTimestamp {
    param([string]$RawValue, [DateTimeOffset]$Parsed)
    return $Parsed.Offset -eq [TimeSpan]::Zero -and $RawValue.Trim().EndsWith('Z', [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertFrom-JsonPreserveDateStrings {
    param([string]$Json)
    $convertParams = @{}
    $cmd = Get-Command ConvertFrom-Json
    if ($cmd.Parameters.ContainsKey('DateKind')) {
        $convertParams.DateKind = 'String'
    }
    return $Json | ConvertFrom-Json @convertParams
}

function Test-StateArtifact {
    param(
        [string]$Path,
        [string]$Artifact,
        [switch]$CheckFreshness
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Missing required state artifact $Artifact."
        return
    }

    try {
        $state = ConvertFrom-JsonPreserveDateStrings -Json (Get-Content -LiteralPath $Path -Raw)
    } catch {
        Add-Failure "$Artifact is not valid JSON."
        return
    }

    if ([int]$state.schemaVersion -ne 2) {
        Add-Failure "$Artifact`: schemaVersion must be 2."
    }
    $sessionGuid = [guid]::Empty
    if (-not [guid]::TryParse([string]$state.sessionId, [ref]$sessionGuid)) {
        Add-Failure "$Artifact`: sessionId must be a GUID."
    }
    if ($null -eq $state.revision -or [int]$state.revision -lt 1) {
        Add-Failure "$Artifact`: revision must be a positive integer."
    }
    if ($null -eq $state.resumeCount -or [int]$state.resumeCount -lt 0) {
        Add-Failure "$Artifact`: resumeCount must be a non-negative integer."
    }

    Require-String -Value $state.objective -Name 'objective' -Artifact $Artifact
    Require-String -Value $state.status -Name 'status' -Artifact $Artifact
    Require-String -Value $state.updatedBy -Name 'updatedBy' -Artifact $Artifact

    if (@('active', 'blocked', 'completed') -notcontains [string]$state.status) {
        Add-Failure "$Artifact`: status must be one of: active, blocked, completed."
    }

    if ($null -eq $state.scope) {
        Add-Failure "$Artifact`: scope object is required."
    } else {
        Require-String -Value $state.scope.kind -Name 'scope.kind' -Artifact $Artifact
        Require-String -Value $state.scope.path -Name 'scope.path' -Artifact $Artifact
        if (@('SolutionPath', 'ProjectPath', 'ScopePath') -notcontains [string]$state.scope.kind) {
            Add-Failure "$Artifact`: scope.kind must be one of: SolutionPath, ProjectPath, ScopePath."
        }
    }

    Require-Array -Value $state.nextActions -Name 'nextActions' -Artifact $Artifact
    Require-Array -Value $state.blockers -Name 'blockers' -Artifact $Artifact

    if ($null -eq $state.history -or -not ($state.history -is [System.Array]) -or @($state.history).Count -eq 0) {
        Add-Failure "$Artifact`: history must be a non-empty array."
    } else {
        $allowedEvents = @('initialized', 'started-after-completion', 'resumed', 'checkpointed', 'verification-pass', 'verification-fail', 'verification-not_run', 'blocked', 'completed')
        foreach ($entry in @($state.history)) {
            if ($null -eq $entry.revision -or [int]$entry.revision -lt 1) { Add-Failure "$Artifact`: history revision must be positive." }
            if ($allowedEvents -notcontains [string]$entry.event) { Add-Failure "$Artifact`: unsupported history event '$($entry.event)'." }
            Require-String -Value $entry.actor -Name 'history.actor' -Artifact $Artifact
        }
        if ([int](@($state.history)[-1]).revision -ne [int]$state.revision) {
            Add-Failure "$Artifact`: latest history revision must equal revision."
        }
    }

    if ($null -eq $state.lastVerification) {
        Add-Failure "$Artifact`: lastVerification object is required."
    } else {
        Require-String -Value $state.lastVerification.command -Name 'lastVerification.command' -Artifact $Artifact
        Require-String -Value $state.lastVerification.result -Name 'lastVerification.result' -Artifact $Artifact
        Require-String -Value $state.lastVerification.ranAtUtc -Name 'lastVerification.ranAtUtc' -Artifact $Artifact
        if (@('pass', 'fail', 'not_run') -notcontains [string]$state.lastVerification.result) {
            Add-Failure "$Artifact`: lastVerification.result must be one of: pass, fail, not_run."
        }
    }

    foreach ($timestamp in @(
        @{ Name = 'updatedAtUtc'; Value = [string]$state.updatedAtUtc; Fresh = $CheckFreshness },
        @{ Name = 'createdAtUtc'; Value = [string]$state.createdAtUtc; Fresh = $false },
        @{ Name = 'lastResumedAtUtc'; Value = [string]$state.lastResumedAtUtc; Fresh = $false },
        @{ Name = 'lastVerification.ranAtUtc'; Value = [string]$state.lastVerification.ranAtUtc; Fresh = $false }
    )) {
        $parsed = $null
        if ([string]::IsNullOrWhiteSpace($timestamp.Value) -or
            -not (Try-ParseDateTimeOffset -Value $timestamp.Value -Parsed ([ref]$parsed))) {
            Add-Failure "$Artifact`: $($timestamp.Name) must be a valid ISO-8601 timestamp."
        } elseif (-not (Test-IsUtcTimestamp -RawValue $timestamp.Value -Parsed $parsed)) {
            Add-Failure "$Artifact`: $($timestamp.Name) must use an explicit UTC Z suffix."
        } elseif ($parsed.UtcDateTime -gt (Get-Date).ToUniversalTime().AddMinutes(5)) {
            Add-Failure "$Artifact`: $($timestamp.Name) cannot be in the future."
        } elseif ($timestamp.Fresh -and ((Get-Date).ToUniversalTime() - $parsed.UtcDateTime).TotalDays -gt $MaxAgeDays) {
            Add-Failure "$Artifact`: updatedAtUtc is older than $MaxAgeDays days."
        }
    }

    foreach ($entry in @($state.history)) {
        $parsedHistory = $null
        if ([string]::IsNullOrWhiteSpace([string]$entry.atUtc) -or -not (Try-ParseDateTimeOffset -Value ([string]$entry.atUtc) -Parsed ([ref]$parsedHistory)) -or -not (Test-IsUtcTimestamp -RawValue ([string]$entry.atUtc) -Parsed $parsedHistory)) {
            Add-Failure "$Artifact`: history.atUtc must be an ISO-8601 UTC timestamp with a Z suffix."
        }
    }
}

if ($Initialize -and -not (Test-Path -LiteralPath $localStatePath -PathType Leaf)) {
    & (Join-Path $PSScriptRoot 'update-agent-memory-state.ps1') -Action Initialize
    if ($LASTEXITCODE -ne 0) { Add-Failure 'Failed to initialize local session state through the lifecycle command.' }
}

Test-StateArtifact -Path $examplePath -Artifact '.codex/state/session-state.example.json'

if (Test-Path -LiteralPath $localStatePath -PathType Leaf) {
    Test-StateArtifact -Path $localStatePath -Artifact '.codex/state/session-state.json' -CheckFreshness
} else {
    Write-Host 'local session state not present; run scripts/check-agent-memory-state.ps1 -Initialize to create it' -ForegroundColor DarkGray
}

if ($failures.Count -gt 0) {
    Write-Host 'agent memory state check failed:' -ForegroundColor Red
    foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
    Write-Host 'Fix the contract or local state per docs/agent-memory.md.' -ForegroundColor Yellow
    exit 1
}

Write-Host 'agent memory state check passed' -ForegroundColor Green
exit 0
