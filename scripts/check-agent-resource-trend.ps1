#Requires -Version 5.1
[CmdletBinding()]
param([Parameter(Mandatory)][string]$EvidenceDirectory, [int]$WindowDays = 7, [string]$SummaryPath)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$jsonParams = @{}
if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
$config = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/agent-resource-budgets.json') -Raw | ConvertFrom-Json @jsonParams
$cutoff = (Get-Date).ToUniversalTime().AddDays(-$WindowDays)
$records = @()
$failures = @()
$culture = [System.Globalization.CultureInfo]::InvariantCulture
function Format-UsdForReport([decimal]$Amount) {
    return ([Math]::Ceiling($Amount * 100) / 100).ToString('0.00', $culture)
}
if (Test-Path -LiteralPath $EvidenceDirectory -PathType Container) {
    foreach ($file in Get-ChildItem -LiteralPath $EvidenceDirectory -Recurse -Filter '*.budget.json' -File) {
        try { $record = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json @jsonParams } catch { throw "Invalid budget evidence: $($file.FullName)" }
        if ([int]$record.schemaVersion -ne 1) { $failures += "Unsupported budget evidence schema: $($file.FullName)"; continue }
        if ($config.workflows.psobject.Properties.Name -notcontains [string]$record.workflow) { $failures += "Unenrolled workflow in budget evidence: $($file.FullName)"; continue }
        $measured = [DateTimeOffset]::MinValue
        if (-not [DateTimeOffset]::TryParse([string]$record.measuredAtUtc, [ref]$measured)) { $failures += "Invalid measuredAtUtc in budget evidence: $($file.FullName)"; continue }
        if ([decimal]$record.estimatedStandardApiEquivalentUsd -lt 0) { $failures += "Negative estimated cost in budget evidence: $($file.FullName)"; continue }
        if ($measured.UtcDateTime -ge $cutoff) {
            $records += $record
            if ($record.passed -ne $true) { $failures += "Failed or incomplete per-run budget decision: $($file.FullName)" }
        }
    }
}
$workflows = @()
foreach ($workflowProperty in @($config.workflows.psobject.Properties)) {
    $matching = @($records | Where-Object workflow -eq $workflowProperty.Name)
    $total = [decimal](($matching | Measure-Object -Property estimatedStandardApiEquivalentUsd -Sum).Sum)
    $limit = [decimal]$workflowProperty.Value.maxEstimatedUsdPer7Days
    $passed = $total -le $limit
    if (-not $passed) { $failures += "$($workflowProperty.Name) $WindowDays-day estimated cost $(Format-UsdForReport $total) exceeds $(Format-UsdForReport $limit) USD." }
    $workflows += [ordered]@{ workflow = $workflowProperty.Name; runs = $matching.Count; estimatedUsd = [Math]::Round($total, 6); ceilingUsd = $limit; passed = $passed }
}
$summary = [ordered]@{ schemaVersion = 1; windowDays = $WindowDays; generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); records = $records.Count; passed = $failures.Count -eq 0; workflows = $workflows; failures = $failures }
if (-not [string]::IsNullOrWhiteSpace($SummaryPath)) { $dir = Split-Path -Parent $SummaryPath; if ($dir) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }; $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $SummaryPath -Encoding utf8 }
if ($failures.Count -gt 0) { Write-Host "agent resource trend failed: $($failures -join ' ')" -ForegroundColor Red; exit 1 }
Write-Host "agent resource trend passed: $($records.Count) evidence record(s) across $($workflows.Count) workflows" -ForegroundColor Green
exit 0
