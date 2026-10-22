#Requires -Version 5.1
<#
.SYNOPSIS
    Validates context-routing operating guidance and high-risk PR context wiring.
#>
[CmdletBinding()]
param(
    [int]$MaxAgeDays = 30
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$guidancePath = Join-Path $repoRoot 'docs/context-routing-operating-guidance.md'
$templatePath = Join-Path $repoRoot '.github/PULL_REQUEST_TEMPLATE.md'
$workflowPath = Join-Path $repoRoot '.github/workflows/ai-harness-high-risk-coverage.yml'
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)
    $failures.Add($Message) | Out-Null
}

function Get-RequiredFileText {
    param([string]$Path, [string]$Name)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Missing required file: $Name"
        return $null
    }

    return Get-Content -LiteralPath $Path -Raw
}

function Try-ParseDateTimeOffset {
    param([string]$RawValue, [ref]$Parsed)
    try {
        $Parsed.Value = [DateTimeOffset]::Parse($RawValue, [System.Globalization.CultureInfo]::InvariantCulture)
        return $true
    } catch {
        return $false
    }
}

function Get-RoutingScopes {
    param([Parameter(Mandatory)][string]$GuidanceText)

    $lines = $GuidanceText -split "\r?\n"
    $scopes = @()
    $inRoutingMap = $false

    foreach ($line in $lines) {
        if ($line -match '^##\s+Routing Map\s*$') {
            $inRoutingMap = $true
            continue
        }

        if ($inRoutingMap -and $line -match '^##\s+') {
            break
        }

        if (-not $inRoutingMap) {
            continue
        }

        if ($line -notmatch '^\|.*\|$' -or $line -match '^\|+\s*-+') {
            continue
        }

        $columns = @($line.Trim('|').Split('|') | ForEach-Object { $_.Trim().Trim('`') })
        if ($columns.Count -gt 0 -and $columns[0] -ne 'Scope') {
            $scopes += $columns[0]
        }
    }

    return @($scopes | Sort-Object -Unique)
}

$guidance = Get-RequiredFileText -Path $guidancePath -Name 'docs/context-routing-operating-guidance.md'
$template = Get-RequiredFileText -Path $templatePath -Name '.github/PULL_REQUEST_TEMPLATE.md'
$workflow = Get-RequiredFileText -Path $workflowPath -Name '.github/workflows/ai-harness-high-risk-coverage.yml'

if ($null -ne $guidance) {
    $requiredHeadings = @(
        '## Routing Map',
        '## Required Context Fields For High-Risk PRs',
        '## Operational Freshness Rules'
    )

    foreach ($heading in $requiredHeadings) {
        if ($guidance -notmatch [regex]::Escape($heading)) {
            Add-Failure "Guidance file is missing section: $heading"
        }
    }

    $routingScopes = Get-RoutingScopes -GuidanceText $guidance
    if ($routingScopes -notcontains '/docs' -or $routingScopes -notcontains '/VAA.EAPI.Automation/docs') {
        Add-Failure 'Guidance routing map must cover both /docs and /VAA.EAPI.Automation/docs scopes.'
    }

    $reviewMatch = [regex]::Match($guidance, '(?im)^LastReviewedUtc:\s*(?<ts>\S+)\s*$')
    if (-not $reviewMatch.Success) {
        Add-Failure 'Guidance file must include LastReviewedUtc: <ISO-8601 UTC timestamp>.'
    } else {
        $raw = $reviewMatch.Groups['ts'].Value.Trim()
        $parsed = $null
        if (-not (Try-ParseDateTimeOffset -RawValue $raw -Parsed ([ref]$parsed))) {
            Add-Failure "Invalid LastReviewedUtc value '$raw'."
        } elseif ($parsed.Offset -ne [TimeSpan]::Zero -or -not $raw.EndsWith('Z', [System.StringComparison]::OrdinalIgnoreCase)) {
            Add-Failure 'LastReviewedUtc must be explicit UTC with Z suffix.'
        } else {
            $nowUtc = (Get-Date).ToUniversalTime()
            if ($parsed.UtcDateTime -gt $nowUtc.AddMinutes(5)) {
                Add-Failure 'LastReviewedUtc cannot be in the future.'
            }

            $ageDays = ($nowUtc - $parsed.UtcDateTime).TotalDays
            if ($ageDays -gt $MaxAgeDays) {
                Add-Failure "LastReviewedUtc is stale ($([math]::Floor($ageDays)) days old). Expected <= $MaxAgeDays days."
            }
        }
    }
}

$contextFields = @(
    'Scope:',
    'Dependencies:',
    'Assumptions:',
    'Impacted sensors:',
    'Rollback/owner:'
)

if ($null -ne $template) {
    if ($template -notmatch '## Context Routing \(Required For `high-risk` PRs\)') {
        Add-Failure 'PR template is missing the high-risk Context Routing section.'
    }

    foreach ($field in $contextFields) {
        if ($template -notmatch [regex]::Escape($field)) {
            Add-Failure "PR template is missing required context field: $field"
        }
    }
}

if ($null -ne $workflow) {
    foreach ($field in $contextFields) {
        if ($workflow -notmatch [regex]::Escape($field)) {
            Add-Failure "High-risk workflow is missing context enforcement field token: $field"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'context routing check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }

    Write-Host 'Fix: update context-routing guidance, PR template, and high-risk workflow wiring.' -ForegroundColor Yellow
    exit 1
}

Write-Host 'context routing check passed' -ForegroundColor Green
exit 0
