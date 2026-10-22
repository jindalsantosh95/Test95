#Requires -Version 5.1
<#
.SYNOPSIS
    Validates that /docs harness-engineering artifacts have operational support.

.DESCRIPTION
    Checks the docs-level harness controls that move guidance from prose into
    deterministic verification: behavior sensors resolve to repository files,
    Complete sequence goals reference implemented sensors, reusable templates are
    discoverable, and drift/runtime feedback evidence is present.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([Parameter(Mandatory)][string]$Message)

    $failures.Add($Message) | Out-Null
}

function Join-RepoPath {
    param([Parameter(Mandatory)][string[]]$Segments)

    $path = $repoRoot
    foreach ($segment in $Segments) {
        $path = Join-Path $path $segment
    }

    return $path
}

function Get-RequiredText {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $path = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing required docs harness artifact: $RepoRelativePath"
        return $null
    }

    return Get-Content -LiteralPath $path -Raw
}

function Get-MarkdownSectionText {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Heading
    )

    $escapedHeading = [regex]::Escape($Heading)
    $match = [regex]::Match($Text, "(?ims)^##\s+$escapedHeading\s*$\r?\n(?<section>.*?)(?=^##\s+|\z)")
    if (-not $match.Success) {
        return $null
    }

    return $match.Groups['section'].Value
}

function ConvertTo-RepoPathCandidate {
    param([Parameter(Mandatory)][string]$Value)

    $candidate = $Value.Trim()
    $candidate = $candidate.Trim('`').Trim()
    $candidate = $candidate -replace '\s+\(active\)$', ''
    $candidate = $candidate.Trim()

    if ($candidate -match '^(?i)(planned|n/a|none|tbd)[:\s]') {
        return $null
    }

    if ($candidate -match '(?<path>[A-Za-z0-9_.\-/]+?\.(cs|ps1|yml|yaml|md))') {
        $path = ($Matches.path -replace '\\', '/').TrimStart('/')
        $segments = @($path -split '/' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        if ($path -match '^[A-Za-z]:' -or $segments -contains '..') {
            return $null
        }

        return ($segments -join '/')
    }

    return $null
}

function Split-SensorCell {
    param([Parameter(Mandatory)][string]$Value)

    return @(
        $Value -split '(?i)<br\s*/?>|;' |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Test-RepoFile {
    param(
        [Parameter(Mandatory)][string]$RepoRelativePath,
        [Parameter(Mandatory)][string]$Context
    )

    $normalizedPath = ($RepoRelativePath -replace '\\', '/').TrimStart('/')
    $segments = @($normalizedPath -split '/' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ([System.IO.Path]::IsPathRooted($RepoRelativePath) -or $normalizedPath -match '^[A-Za-z]:' -or $segments -contains '..') {
        Add-Failure "$Context references non-repository-relative file path: $RepoRelativePath"
        return
    }

    $nativePath = Join-RepoPath -Segments $segments
    if (-not (Test-Path -LiteralPath $nativePath -PathType Leaf)) {
        Add-Failure "$Context references missing repository file: $RepoRelativePath"
    }
}

$behaviorHarness = Get-RequiredText -RepoRelativePath 'docs/behavior-harness.md'
if ($null -ne $behaviorHarness) {
    $sensorCatalog = Get-MarkdownSectionText -Text $behaviorHarness -Heading 'Sensor Catalog'
    if ([string]::IsNullOrWhiteSpace($sensorCatalog)) {
        Add-Failure 'docs/behavior-harness.md must contain a Sensor Catalog section.'
        $sensorCatalog = ''
    }

    $rows = @(
        [regex]::Matches($sensorCatalog, '(?im)^\|\s*`(?<behavior>[^`]+)`\s*\|\s*(?<sensor>.*?)\s*\|\s*(?<proof>.*?)\s*\|\s*$') |
        Where-Object { $_.Groups['behavior'].Value -ne 'Behavior' }
    )

    if ($rows.Count -eq 0) {
        Add-Failure 'docs/behavior-harness.md must contain at least one concrete Sensor Catalog row.'
    }

    foreach ($row in $rows) {
        $behavior = $row.Groups['behavior'].Value.Trim()
        $sensorItems = Split-SensorCell -Value $row.Groups['sensor'].Value
        $resolvedSensors = @(
            $sensorItems |
            ForEach-Object { ConvertTo-RepoPathCandidate -Value $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )

        if ($resolvedSensors.Count -eq 0) {
            Add-Failure "Behavior sensor '$behavior' must reference at least one concrete repository file."
            continue
        }

        foreach ($sensor in $resolvedSensors) {
            Test-RepoFile -RepoRelativePath $sensor -Context "Behavior sensor '$behavior'"
        }
    }
}

$sequenceIndex = Get-RequiredText -RepoRelativePath 'docs/sequence-diagrams.md'
if ($null -ne $sequenceIndex) {
    foreach ($required in @(
        'sequence-diagrams/_template.sequence-diagram.md',
        'scripts/check-sequence-diagram-status.ps1',
        'scripts/check-mermaid-diagrams.ps1',
        'scripts/promote-sequence-goal.ps1',
        'scripts/run-sequence-goal-agent.ps1'
    )) {
        if ($sequenceIndex -notmatch [regex]::Escape($required)) {
            Add-Failure "docs/sequence-diagrams.md must reference $required for template and agent-loop discoverability."
        }
    }
}

$sequenceFiles = @(
    Get-ChildItem -LiteralPath (Join-RepoPath -Segments @('docs', 'sequence-diagrams')) -Filter '*.md' -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notmatch '^(?i)(_template|template)' }
)

foreach ($diagram in $sequenceFiles) {
    $relative = "docs/sequence-diagrams/$($diagram.Name)"
    $text = Get-Content -LiteralPath $diagram.FullName -Raw
    $statusMatch = [regex]::Match($text, '(?im)^\|\s*Status\s*\|\s*`(?<status>[^`]+)`\s*\|\s*$')
    if (-not $statusMatch.Success -or $statusMatch.Groups['status'].Value.Trim() -ne 'Complete') {
        continue
    }

    $sensorMatch = [regex]::Match($text, '(?im)^\|\s*Harness Sensor\s*\|\s*(?<sensor>.*?)\s*\|\s*$')
    if (-not $sensorMatch.Success) {
        Add-Failure "$relative is Complete but has no Harness Sensor row."
        continue
    }

    $sensorItems = Split-SensorCell -Value $sensorMatch.Groups['sensor'].Value
    $resolvedSensors = @(
        $sensorItems |
        ForEach-Object { ConvertTo-RepoPathCandidate -Value $_ } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($resolvedSensors.Count -eq 0) {
        Add-Failure "$relative is Complete but does not reference a concrete harness sensor path."
        continue
    }

    foreach ($sensor in $resolvedSensors) {
        Test-RepoFile -RepoRelativePath $sensor -Context "$relative Complete goal"
    }
}

$requiredTemplateArtifacts = @(
    'docs/sequence-diagrams/_template.sequence-diagram.md',
    'docs/exec-plans/template.md',
    'docs/decisions/template.md'
)
foreach ($template in $requiredTemplateArtifacts) {
    Test-RepoFile -RepoRelativePath $template -Context 'Docs harness template support'
}

$requiredHarnessContracts = @(
    'docs/agent-observability.md',
    'docs/doc-gardening.md',
    'docs/agent-execution-and-recovery.md',
    'docs/agent-resource-and-cost-controls.md',
    'docs/agent-tool-and-executable-environments.md',
    'docs/harness-continuous-maturity.md'
)
foreach ($contract in $requiredHarnessContracts) {
    Test-RepoFile -RepoRelativePath $contract -Context 'Docs harness contract support'
}

Test-RepoFile -RepoRelativePath 'docs/harness-operational-capabilities.json' -Context 'Operational harness registry'
Test-RepoFile -RepoRelativePath 'scripts/check-operational-harness.ps1' -Context 'Operational harness sensor'
Test-RepoFile -RepoRelativePath '.github/workflows/ai-harness-docs-semantic-gardening.yml' -Context 'Semantic documentation gardening workflow'
Test-RepoFile -RepoRelativePath '.github/workflows/ai-harness-high-risk-inferential-review.yml' -Context 'High-risk inferential review workflow'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-workflow-controls.ps1' -Context 'Agent workflow execution-control sensor'
Test-RepoFile -RepoRelativePath 'docs/agent-observability-sources.json' -Context 'Agent observability source registry'
Test-RepoFile -RepoRelativePath 'docs/harness-runtime-feedback.md' -Context 'Runtime-to-harness feedback evidence'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-observability.ps1' -Context 'Agent observability sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-observed-docs-harness.ps1' -Context 'Observed docs harness entry point'
Test-RepoFile -RepoRelativePath 'scripts/query-agent-observability.ps1' -Context 'Bounded agent observability query'
Test-RepoFile -RepoRelativePath 'scripts/invoke-agent-observability-exercise.ps1' -Context 'Observability failure-path exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-observability-strength.ps1' -Context 'Observability strength sensor'
Test-RepoFile -RepoRelativePath 'docs/agent-tool-environments.json' -Context 'Agent tool environment registry'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-tool-environment.ps1' -Context 'Agent tool environment sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-agent-tool-environment-exercise.ps1' -Context 'Agent tool environment exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-tool-environment-strength.ps1' -Context 'Agent tool environment strength sensor'
Test-RepoFile -RepoRelativePath 'docs/harness-maturity-policy.json' -Context 'Continuous maturity policy'
Test-RepoFile -RepoRelativePath 'scripts/update-harness-maturity-evidence.ps1' -Context 'Continuous maturity evidence updater'
Test-RepoFile -RepoRelativePath 'scripts/check-harness-maturity-evidence.ps1' -Context 'Continuous maturity evidence sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-harness-maturity-evidence-exercise.ps1' -Context 'Continuous maturity evidence exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-harness-maturity-evidence-strength.ps1' -Context 'Continuous maturity strength sensor'
Test-RepoFile -RepoRelativePath 'scripts/agent-execution-state.ps1' -Context 'Durable agent execution-state manager'
Test-RepoFile -RepoRelativePath 'scripts/invoke-agent-recovery-exercise.ps1' -Context 'Agent recovery exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-recovery.ps1' -Context 'Agent recovery evidence sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-self-correcting-loop.ps1' -Context 'General self-correcting loop runner'
Test-RepoFile -RepoRelativePath 'scripts/invoke-self-correcting-loop-exercise.ps1' -Context 'Self-correcting loop exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-self-correcting-loop.ps1' -Context 'Self-correcting loop evidence sensor'
Test-RepoFile -RepoRelativePath 'scripts/check-behavior-harness.ps1' -Context 'Complete behavior enrollment sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-behavior-harness-mutation-exercise.ps1' -Context 'Behavior harness mutation exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-behavior-harness-strength.ps1' -Context 'Behavior harness strength sensor'
Test-RepoFile -RepoRelativePath 'docs/architecture-fitness-scopes.json' -Context 'Architecture fitness scope inventory'
Test-RepoFile -RepoRelativePath 'scripts/check-architecture-fitness.ps1' -Context 'Repository project-graph fitness sensor'
Test-RepoFile -RepoRelativePath 'scripts/invoke-architecture-fitness-mutation-exercise.ps1' -Context 'Architecture fitness mutation exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-architecture-fitness-strength.ps1' -Context 'Architecture fitness strength sensor'
Test-RepoFile -RepoRelativePath 'scripts/update-agent-memory-state.ps1' -Context 'Atomic revisioned agent-memory lifecycle'
Test-RepoFile -RepoRelativePath 'scripts/invoke-agent-memory-lifecycle-exercise.ps1' -Context 'Cross-process memory resume exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-memory-lifecycle.ps1' -Context 'Memory lifecycle evidence sensor'
Test-RepoFile -RepoRelativePath 'docs/agent-resource-budgets.json' -Context 'Machine-readable token and cost ceilings'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-resource-budget.ps1' -Context 'Per-run token and cost enforcement'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-resource-trend.ps1' -Context 'Thirty-day resource trend enforcement'
Test-RepoFile -RepoRelativePath 'scripts/invoke-agent-resource-budget-exercise.ps1' -Context 'Resource-budget mutation exercise'
Test-RepoFile -RepoRelativePath 'scripts/check-agent-resource-budget-strength.ps1' -Context 'Resource-budget evidence sensor'
Test-RepoFile -RepoRelativePath '.github/workflows/ai-harness-agent-resource-and-cost-trend-review.yml' -Context 'Monthly resource and cost trend review'

$dod = Get-RequiredText -RepoRelativePath 'docs/harness-definition-of-done.md'
if ($null -ne $dod) {
    foreach ($theme in @('Doc gardening cadence', 'Runtime feedback loop', 'Agent execution and recovery', 'Resource and cost controls', 'Tool integration and executable environment', 'Continuous maturity evidence')) {
        $pattern = "(?im)^\|\s*$([regex]::Escape($theme))\s*\|.*\|\s*(Amber|Green)\s*\|\s*$"
        if ($dod -notmatch $pattern) {
            Add-Failure "docs/harness-definition-of-done.md must track '$theme' as Amber until its operational exit criteria are sustained, or Green afterwards."
        }
    }

    foreach ($theme in @(
        'Architecture fitness gates',
        'Adoption KPI visibility',
        'Governance durability'
    )) {
        $pattern = "(?im)^\|\s*$([regex]::Escape($theme))\s*\|.*\|\s*Green\s*\|\s*$"
        if ($dod -notmatch $pattern) {
            Add-Failure "docs/harness-definition-of-done.md must mark '$theme' as Green once /docs harness support is fully operational."
        }
    }
}

$remediation = Get-RequiredText -RepoRelativePath 'docs/harness-remediation-log.md'
if ($null -ne $remediation) {
    foreach ($requiredEvidence in @(
        '.github/workflows/ai-harness-drift-parity.yml',
        '.github/workflows/ai-harness-closed-loop-autonomy.yml',
        'runtime-feedback',
        'weekly-kpi'
    )) {
        if ($remediation -notmatch [regex]::Escape($requiredEvidence)) {
            Add-Failure "docs/harness-remediation-log.md must include operational evidence for $requiredEvidence."
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'docs harness support check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }

    exit 1
}

Write-Host 'docs harness support check passed' -ForegroundColor Green
exit 0
