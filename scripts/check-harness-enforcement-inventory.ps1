#Requires -Version 5.1
<#
.SYNOPSIS
    Checks docs/harness-pipeline-checklist.md enforcement inventory for unresolved gaps.

.PARAMETER FailOnGap
    When supplied, unresolved gaps return exit code 1. Without this switch the
    script reports findings and exits 0, which is useful during rollout.
#>
[CmdletBinding()]
param(
    [switch]$FailOnGap
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$checklistPath = Join-Path $repoRoot 'docs/harness-pipeline-checklist.md'

if (-not (Test-Path -LiteralPath $checklistPath)) {
    throw "Checklist not found: $checklistPath"
}

$lines = Get-Content -LiteralPath $checklistPath
$tableLines = @(
    $lines |
    Where-Object { $_ -match '^\|.*\|$' } |
    Where-Object { $_ -notmatch '^\|+\s*-+' } |
    Where-Object { $_ -notmatch '^\|+\s*Scope\s*\|' }
)

$issues = @()
foreach ($line in $tableLines) {
    if (
        $line -match '\|\s*Gap\s*\|' -or
        $line -match '\bTBD\b' -or
        $line -match '<tbd>' -or
        $line -match '<[^>]+>' -or
        $line -match '\bPending\b' -or
        $line -match '\bInterim\b'
    ) {
        $issues += $line
    }
}

if ($issues.Count -gt 0) {
    Write-Host "harness enforcement inventory has unresolved entries:" -ForegroundColor Yellow
    foreach ($issue in $issues) {
        Write-Host " - $issue" -ForegroundColor Yellow
    }

    if ($FailOnGap) {
        exit 1
    }

    Write-Host 'inventory check running in report-only mode (no failure)' -ForegroundColor Yellow
    exit 0
}

Write-Host 'harness enforcement inventory check passed' -ForegroundColor Green
exit 0
