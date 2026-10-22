#Requires -Version 5.1
<#
.SYNOPSIS
    Validates docs/harness-remediation-log.md freshness and remediation closure discipline.

.DESCRIPTION
    Fails when the remediation log is missing, stale, or contains overdue open items.
#>
[CmdletBinding()]
param(
    [int]$MaxReviewAgeDays = 14
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$logPath = Join-Path $repoRoot 'docs/harness-remediation-log.md'
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)

    $failures.Add($Message) | Out-Null
}

function Get-TableRows {
    param(
        [Parameter(Mandatory)][object]$Lines,
        [Parameter(Mandatory)][string]$SectionHeading
    )

    $materialized = @($Lines)
    if ($materialized.Count -eq 0) {
        return @()
    }

    $start = -1
    for ($i = 0; $i -lt $materialized.Count; $i++) {
        if ([string]$materialized[$i] -eq $SectionHeading) {
            $start = $i
            break
        }
    }

    if ($start -lt 0) {
        return @()
    }

    $rows = @()
    for ($i = $start + 1; $i -lt $materialized.Count; $i++) {
        $line = [string]$materialized[$i]
        if ($line -match '^##\s+') {
            break
        }

        if (
            $line -match '^\|.*\|$' -and
            $line -notmatch '^\|\s*-+' -and
            $line -notmatch '^\|\s*Check Name\s*\|' -and
            $line -notmatch '^\|\s*Review Date UTC\s*\|' -and
            $line -notmatch '^\|\s*ID\s*\|'
        ) {
            $rows += $line
        }
    }

    return $rows
}

function Convert-RowToColumns {
    param([Parameter(Mandatory)][string]$Row)

    return @($Row.Trim('|').Split('|') | ForEach-Object { $_.Trim() })
}

function Try-ParseDateTimeOffset {
    param(
        [Parameter(Mandatory)][string]$RawValue,
        [ref]$Parsed
    )

    try {
        $Parsed.Value = [DateTimeOffset]::Parse($RawValue, [System.Globalization.CultureInfo]::InvariantCulture)
        return $true
    } catch {
        return $false
    }
}

function Test-IsExplicitUtcTimestamp {
    param(
        [Parameter(Mandatory)][string]$RawValue,
        [Parameter(Mandatory)][DateTimeOffset]$Parsed
    )

    return $Parsed.Offset -eq [TimeSpan]::Zero -and $RawValue.Trim().EndsWith('Z', [System.StringComparison]::OrdinalIgnoreCase)
}

if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
    Add-Failure "Missing remediation log: $logPath"
} else {
    $lines = Get-Content -LiteralPath $logPath

    $branchRows = Get-TableRows -Lines $lines -SectionHeading '## Branch Protection Confirmations'
    if ($branchRows.Count -eq 0) {
        Add-Failure 'Branch Protection Confirmations must contain required-check confirmation rows.'
    } else {
        $requiredChecks = @('parity-gate', 'harness-coverage', 'verify', 'closed-loop-gate')
        $seen = @{}
        foreach ($row in $branchRows) {
            $columns = Convert-RowToColumns -Row $row
            if ($columns.Count -lt 7) {
                Add-Failure "Invalid branch protection row shape: $row"
                continue
            }

            $checkName = $columns[0].Trim('`')
            $confirmed = $columns[2]
            $bypassPolicy = $columns[3]
            $confirmedAtRaw = $columns[5]
            $seen[$checkName] = $true

            if ($confirmed -ne 'Yes') {
                Add-Failure "Branch protection confirmation for '$checkName' must be 'Yes'."
            }

            if ([string]::IsNullOrWhiteSpace($bypassPolicy)) {
                Add-Failure "Branch protection confirmation for '$checkName' must include bypass policy text."
            }

            $confirmedAt = $null
            if (-not (Try-ParseDateTimeOffset -RawValue $confirmedAtRaw -Parsed ([ref]$confirmedAt))) {
                Add-Failure "Invalid Confirmed At UTC timestamp for '$checkName': $confirmedAtRaw"
            } elseif (-not (Test-IsExplicitUtcTimestamp -RawValue $confirmedAtRaw -Parsed $confirmedAt)) {
                Add-Failure "Confirmed At UTC for '$checkName' must use explicit UTC Z suffix."
            }
        }

        foreach ($requiredCheck in $requiredChecks) {
            if (-not $seen.ContainsKey($requiredCheck)) {
                Add-Failure "Missing required branch protection confirmation row for '$requiredCheck'."
            }
        }
    }

    $driftRows = Get-TableRows -Lines $lines -SectionHeading '## Drift Review Log'
    if ($driftRows.Count -eq 0) {
        Add-Failure 'Drift Review Log must contain at least one review row.'
    } else {
        $latest = [DateTimeOffset]::MinValue
        $passCount = 0

        foreach ($row in $driftRows) {
            $columns = Convert-RowToColumns -Row $row
            if ($columns.Count -lt 5) {
                Add-Failure "Invalid drift review row shape: $row"
                continue
            }

            $reviewDateRaw = $columns[0]
            $result = $columns[2]
            $owner = $columns[3]
            $parsedReviewDate = $null

            if (-not (Try-ParseDateTimeOffset -RawValue $reviewDateRaw -Parsed ([ref]$parsedReviewDate))) {
                Add-Failure "Invalid review date in Drift Review Log: $reviewDateRaw"
            } elseif (-not (Test-IsExplicitUtcTimestamp -RawValue $reviewDateRaw -Parsed $parsedReviewDate)) {
                Add-Failure "Review Date UTC must use explicit UTC Z suffix: $reviewDateRaw"
            } else {
                if ($parsedReviewDate -gt $latest) {
                    $latest = $parsedReviewDate
                }
            }

            if ([string]::IsNullOrWhiteSpace($owner)) {
                Add-Failure "Drift review row is missing owner: $row"
            }

            if ([string]::IsNullOrWhiteSpace($result)) {
                Add-Failure "Drift review row is missing result: $row"
            } elseif ($result -eq 'Pass') {
                $passCount += 1
            }
        }

        if ($driftRows.Count -lt 2) {
            Add-Failure 'Closed-loop autonomy evidence requires at least 2 drift review rows.'
        }

        if ($passCount -lt 2) {
            Add-Failure 'Closed-loop autonomy evidence requires at least 2 passing drift review rows.'
        }

        if ($latest -ne [DateTimeOffset]::MinValue) {
            $ageDays = ((Get-Date).ToUniversalTime() - $latest.UtcDateTime).TotalDays
            if ($ageDays -gt $MaxReviewAgeDays) {
                Add-Failure "Latest drift review is stale ($([math]::Floor($ageDays)) days old). Expected <= $MaxReviewAgeDays days."
            }
        }
    }

    $remediationRows = Get-TableRows -Lines $lines -SectionHeading '## Remediation Items'
    if ($remediationRows.Count -eq 0) {
        Write-Host 'no remediation items listed; treating as no open findings' -ForegroundColor DarkGray
    } else {
        foreach ($row in $remediationRows) {
            $columns = Convert-RowToColumns -Row $row
            if ($columns.Count -lt 10) {
                Add-Failure "Invalid remediation row shape: $row"
                continue
            }

            $id = $columns[0]
            $openedAtRaw = $columns[1]
            $owner = $columns[3]
            $dueAtRaw = $columns[4]
            $status = $columns[5]
            $repeatCountRaw = $columns[6]
            $escalationControl = $columns[7]
            $closedAtRaw = $columns[8]
            $evidence = $columns[9]

            if ([string]::IsNullOrWhiteSpace($id)) {
                Add-Failure "Remediation row is missing ID: $row"
            }

            if (@('Open', 'InProgress', 'Closed') -notcontains $status) {
                Add-Failure "Remediation row has invalid status '$status': $row"
            }

            $openedAt = $null
            if (-not (Try-ParseDateTimeOffset -RawValue $openedAtRaw -Parsed ([ref]$openedAt))) {
                Add-Failure "Remediation row has invalid Opened At UTC '$openedAtRaw': $row"
            } elseif (-not (Test-IsExplicitUtcTimestamp -RawValue $openedAtRaw -Parsed $openedAt)) {
                Add-Failure "Opened At UTC must use explicit UTC Z suffix: $openedAtRaw"
            }

            if ([string]::IsNullOrWhiteSpace($owner)) {
                Add-Failure "Remediation row is missing owner: $row"
            }

            $dueAt = $null
            if (-not (Try-ParseDateTimeOffset -RawValue $dueAtRaw -Parsed ([ref]$dueAt))) {
                Add-Failure "Remediation row has invalid due date '$dueAtRaw': $row"
                continue
            } elseif (-not (Test-IsExplicitUtcTimestamp -RawValue $dueAtRaw -Parsed $dueAt)) {
                Add-Failure "Due At UTC must use explicit UTC Z suffix: $dueAtRaw"
                continue
            }

            if ($status -in @('Open', 'InProgress')) {
                if ((Get-Date).ToUniversalTime() -gt $dueAt.UtcDateTime) {
                    Add-Failure "Remediation item '$id' is overdue with status '$status'."
                }
            }

            $repeatCount = 0
            if (-not [int]::TryParse($repeatCountRaw, [ref]$repeatCount) -or $repeatCount -lt 1) {
                Add-Failure "Remediation item '$id' has invalid repeat count '$repeatCountRaw'."
            } elseif ($repeatCount -ge 3 -and [string]::IsNullOrWhiteSpace($escalationControl)) {
                Add-Failure "Remediation item '$id' has repeat count $repeatCount and must include Escalation Control."
            }

            if ($status -eq 'Closed') {
                $closedAt = $null
                if (-not (Try-ParseDateTimeOffset -RawValue $closedAtRaw -Parsed ([ref]$closedAt))) {
                    Add-Failure "Closed remediation item '$id' must include a valid Closed At UTC value."
                } elseif (-not (Test-IsExplicitUtcTimestamp -RawValue $closedAtRaw -Parsed $closedAt)) {
                    Add-Failure "Closed At UTC for '$id' must use explicit UTC Z suffix."
                }
            }

            if ([string]::IsNullOrWhiteSpace($evidence)) {
                Add-Failure "Remediation item '$id' must include evidence reference."
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'harness remediation log check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }

    Write-Host 'Fix: update docs/harness-remediation-log.md with fresh drift run evidence and remediation closure details.' -ForegroundColor Yellow
    exit 1
}

Write-Host 'harness remediation log check passed' -ForegroundColor Green
exit 0
