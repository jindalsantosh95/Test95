#Requires -Version 5.1
[CmdletBinding()]
param([string]$EvidenceDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) { $EvidenceDirectory = Join-Path $repoRoot ('TestResults/agent-resource-budget/' + [guid]::NewGuid().ToString('N')) }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$checker = Join-Path $PSScriptRoot 'check-agent-resource-budget.ps1'

$passUsage = Join-Path $EvidenceDirectory 'pass.jsonl'
@(
    '{"type":"thread.started","thread_id":"exercise"}',
    '{"type":"turn.completed","usage":{"input_tokens":100000,"cached_input_tokens":20000,"output_tokens":10000,"reasoning_output_tokens":1000}}'
) | Set-Content -LiteralPath $passUsage -Encoding utf8
$passEvidence = Join-Path $EvidenceDirectory 'pass.budget.json'
& $checker -WorkflowId 'docs-semantic-gardening' -Model 'gpt-5.4-mini' -UsagePath $passUsage -EvidencePath $passEvidence
if ($LASTEXITCODE -ne 0) { throw 'Expected compliant usage to pass.' }

$breachUsage = Join-Path $EvidenceDirectory 'breach.jsonl'
'{"type":"turn.completed","usage":{"input_tokens":200000,"cached_input_tokens":0,"output_tokens":20000,"reasoning_output_tokens":0}}' | Set-Content -LiteralPath $breachUsage -Encoding utf8
$breachEvidence = Join-Path $EvidenceDirectory 'breach.budget.json'
& $checker -WorkflowId 'docs-semantic-gardening' -Model 'gpt-5.4-mini' -UsagePath $breachUsage -EvidencePath $breachEvidence
$breachRejected = $LASTEXITCODE -ne 0

$missingEvidence = Join-Path $EvidenceDirectory 'missing.budget.json'
& $checker -WorkflowId 'docs-semantic-gardening' -Model 'gpt-5.4-mini' -UsagePath (Join-Path $EvidenceDirectory 'missing.jsonl') -EvidencePath $missingEvidence
$missingRejected = $LASTEXITCODE -ne 0

$longContextUsage = Join-Path $EvidenceDirectory 'long-context.jsonl'
'{"type":"turn.completed","usage":{"input_tokens":300000,"cached_input_tokens":0,"output_tokens":10000,"reasoning_output_tokens":0}}' | Set-Content -LiteralPath $longContextUsage -Encoding utf8
$longContextEvidence = Join-Path $EvidenceDirectory 'long-context.budget.json'
& $checker -WorkflowId 'sequence-goal-loop' -Model 'gpt-6-sol' -UsagePath $longContextUsage -EvidencePath $longContextEvidence
$longContextRecord = Get-Content -LiteralPath $longContextEvidence -Raw | ConvertFrom-Json
$longContextMultiplierApplied = [decimal]$longContextRecord.estimatedStandardApiEquivalentUsd -eq [decimal]1.35
$solPricingPassed = $true
foreach ($workflow in @('sequence-goal-loop', 'sequence-goal-pr-review-loop', 'sequence-goal-human-comment-loop', 'sequence-goal-associated-pr-feedback')) {
    $solEvidence = Join-Path $EvidenceDirectory "$workflow-sol.budget.json"
    & $checker -WorkflowId $workflow -Model 'gpt-6-sol' -UsagePath $passUsage -EvidencePath $solEvidence
    $solRecord = Get-Content -LiteralPath $solEvidence -Raw | ConvertFrom-Json
    $solPricingPassed = $solPricingPassed -and $LASTEXITCODE -eq 0 -and $solRecord.passed -and [decimal]$solRecord.estimatedStandardApiEquivalentUsd -eq [decimal]0.264
}
# Cached multi-turn delivery can exceed the legacy token caps while staying below $30.
$deliveryUsage = Join-Path $EvidenceDirectory 'delivery-cached.jsonl'
'{"type":"turn.completed","usage":{"input_tokens":30000000,"cached_input_tokens":30000000,"output_tokens":50000}}' | Set-Content -LiteralPath $deliveryUsage -Encoding utf8
$deliveryCostBreachUsage = Join-Path $EvidenceDirectory 'delivery-cost-breach.jsonl'
'{"type":"turn.completed","usage":{"input_tokens":8000000,"cached_input_tokens":0,"output_tokens":10000}}' | Set-Content -LiteralPath $deliveryCostBreachUsage -Encoding utf8
$deliveryHeadroomPassed = $true
$deliveryCostBreachRejected = $true
$configuration = Get-Content (Join-Path $repoRoot 'docs/agent-resource-budgets.json') -Raw | ConvertFrom-Json
foreach ($workflow in @('sequence-goal-loop', 'sequence-goal-pr-review-loop', 'sequence-goal-human-comment-loop', 'sequence-goal-associated-pr-feedback')) {
    foreach ($model in $configuration.workflows.$workflow.allowedModels) {
        $evidence = Join-Path $EvidenceDirectory "$workflow-$model-headroom.budget.json"
        & $checker -WorkflowId $workflow -Model $model -UsagePath $deliveryUsage -EvidencePath $evidence
        $record = Get-Content $evidence -Raw | ConvertFrom-Json
        $deliveryHeadroomPassed = $deliveryHeadroomPassed -and $LASTEXITCODE -eq 0 -and $record.passed
        $evidence = Join-Path $EvidenceDirectory "$workflow-$model-cost-breach.budget.json"
        & $checker -WorkflowId $workflow -Model $model -UsagePath $deliveryCostBreachUsage -EvidencePath $evidence
        $record = Get-Content $evidence -Raw | ConvertFrom-Json
        $deliveryCostBreachRejected = $deliveryCostBreachRejected -and $LASTEXITCODE -ne 0 -and -not $record.passed -and
            @($record.failures).Count -eq 1 -and $record.failures[0] -like 'Estimated cost ceiling exceeded:*'
    }
}
$resourceSummary = & (Join-Path $PSScriptRoot 'get-agent-resource-summary.ps1') -WorkflowId 'docs-semantic-gardening' -BudgetEvidencePath $passEvidence | ConvertFrom-Json
$expectedReportedCost = ([Math]::Ceiling([decimal]$resourceSummary.actual.estimatedUsd * 100) / 100).ToString('0.00', [System.Globalization.CultureInfo]::InvariantCulture)
$reportedCostRoundedUp = ([string]$resourceSummary.actualLabel).EndsWith("`$$expectedReportedCost USD", [System.StringComparison]::Ordinal)

$trendDirectory = Join-Path $EvidenceDirectory 'trend'
New-Item -ItemType Directory -Path $trendDirectory -Force | Out-Null
Copy-Item -LiteralPath $passEvidence -Destination (Join-Path $trendDirectory 'run-1.budget.json')
$trendSummary = Join-Path $EvidenceDirectory 'trend-summary.json'
& (Join-Path $PSScriptRoot 'check-agent-resource-trend.ps1') -EvidenceDirectory $trendDirectory -SummaryPath $trendSummary
$trendPassed = $LASTEXITCODE -eq 0

$trendBreachDirectory = Join-Path $EvidenceDirectory 'trend-breach'
New-Item -ItemType Directory -Path $trendBreachDirectory -Force | Out-Null
foreach ($index in 1..10) { Copy-Item -LiteralPath $passEvidence -Destination (Join-Path $trendBreachDirectory "run-$index.budget.json") }
$trendBreachSummary = Join-Path $EvidenceDirectory 'trend-breach-summary.json'
& (Join-Path $PSScriptRoot 'check-agent-resource-trend.ps1') -EvidenceDirectory $trendBreachDirectory -SummaryPath $trendBreachSummary
$trendBreachRejected = $LASTEXITCODE -ne 0

$checks = [ordered]@{
    compliantUsagePassed = (Get-Content -LiteralPath $passEvidence -Raw | ConvertFrom-Json).passed -eq $true
    tokenAndCostBreachRejected = $breachRejected -and (Get-Content -LiteralPath $breachEvidence -Raw | ConvertFrom-Json).passed -eq $false
    missingUsageRejected = $missingRejected -and (Get-Content -LiteralPath $missingEvidence -Raw | ConvertFrom-Json).passed -eq $false
    longContextPricingApplied = $longContextMultiplierApplied
    solDeliveryAndReviewPricingApplied = $solPricingPassed
    affordableDeliveryBeyondLegacyTokenCapsPassed = $deliveryHeadroomPassed
    deliveryDollarCeilingIndependentlyEnforced = $deliveryCostBreachRejected
    reportedCostRoundedUpToTwoDecimals = $reportedCostRoundedUp
    trendWindowPassed = $trendPassed -and (Get-Content -LiteralPath $trendSummary -Raw | ConvertFrom-Json).passed -eq $true -and [int](Get-Content -LiteralPath $trendSummary -Raw | ConvertFrom-Json).records -eq 1
    trendCeilingBreachRejected = $trendBreachRejected -and (Get-Content -LiteralPath $trendBreachSummary -Raw | ConvertFrom-Json).passed -eq $false
}
$passed = @($checks.GetEnumerator() | Where-Object { -not $_.Value }).Count -eq 0
$summary = [ordered]@{ schemaVersion = 1; capability = 'resource-cost-control'; passed = $passed; exercisedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); checks = $checks }
$summaryPath = Join-Path $EvidenceDirectory 'summary.json'
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding utf8
if (-not $passed) { Write-Host 'agent resource budget exercise failed' -ForegroundColor Red; exit 1 }
Write-Host "agent resource budget exercise passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
Write-Host "evidence: $summaryPath" -ForegroundColor DarkGray
exit 0
