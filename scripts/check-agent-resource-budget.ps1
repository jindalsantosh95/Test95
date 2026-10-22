#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$WorkflowId,
    [string]$Model,
    [string]$UsagePath,
    [string]$EvidencePath,
    [switch]$ValidateConfiguration
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$configPath = Join-Path $repoRoot 'docs/agent-resource-budgets.json'
$failures = [System.Collections.Generic.List[string]]::new()
$culture = [System.Globalization.CultureInfo]::InvariantCulture
function Format-UsdForReport([decimal]$Amount) {
    return ([Math]::Ceiling($Amount * 100) / 100).ToString('0.00', $culture)
}

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) { throw 'Missing docs/agent-resource-budgets.json.' }
try {
    $configJson = Get-Content -LiteralPath $configPath -Raw
    $jsonParams = @{}
    if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
    $config = $configJson | ConvertFrom-Json @jsonParams
} catch { throw 'Agent resource budget configuration is invalid JSON.' }

if ([int]$config.schemaVersion -ne 1) { $failures.Add('schemaVersion must be 1.') }
if ([string]$config.currency -ne 'USD') { $failures.Add('currency must be USD.') }
$reviewed = [DateTimeOffset]::MinValue
if (-not [DateTimeOffset]::TryParse([string]$config.pricingReviewedAtUtc, [ref]$reviewed) -or $reviewed.Offset -ne [TimeSpan]::Zero) {
    $failures.Add('pricingReviewedAtUtc must be an explicit UTC timestamp.')
} elseif (((Get-Date).ToUniversalTime() - $reviewed.UtcDateTime).TotalDays -gt [int]$config.maxPricingAgeDays) {
    $failures.Add("Pricing review is older than $($config.maxPricingAgeDays) days.")
}
if (@($config.pricingSources).Count -eq 0 -or @($config.pricingSources | Where-Object { [string]$_ -notmatch '^https://developers\.openai\.com/' }).Count -gt 0) {
    $failures.Add('pricingSources must contain official developers.openai.com URLs only.')
}

foreach ($modelProperty in @($config.models.psobject.Properties)) {
    $rate = $modelProperty.Value
    foreach ($field in @('inputUsdPerMillion', 'cachedInputUsdPerMillion', 'outputUsdPerMillion')) {
        if ([decimal]$rate.$field -le 0) { $failures.Add("Model '$($modelProperty.Name)' has invalid $field.") }
    }
}
foreach ($workflowProperty in @($config.workflows.psobject.Properties)) {
    $budget = $workflowProperty.Value
    if (@($budget.allowedModels).Count -eq 0) { $failures.Add("Workflow '$($workflowProperty.Name)' has no allowedModels.") }
    foreach ($allowedModel in @($budget.allowedModels)) {
        if ($config.models.psobject.Properties.Name -notcontains [string]$allowedModel) { $failures.Add("Workflow '$($workflowProperty.Name)' uses unpriced model '$allowedModel'.") }
    }
    foreach ($field in @('maxInputTokensPerRun', 'maxOutputTokensPerRun', 'maxEstimatedUsdPerRun', 'maxEstimatedUsdPer7Days')) {
        if ([decimal]$budget.$field -le 0) { $failures.Add("Workflow '$($workflowProperty.Name)' has invalid $field.") }
    }
    if ([decimal]$budget.maxEstimatedUsdPer7Days -lt [decimal]$budget.maxEstimatedUsdPerRun) {
        $failures.Add("Workflow '$($workflowProperty.Name)' maxEstimatedUsdPer7Days must be greater than or equal to maxEstimatedUsdPerRun.")
    }
}

if ($ValidateConfiguration) {
    if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }; exit 1 }
    Write-Host "agent resource budget configuration passed ($(@($config.workflows.psobject.Properties).Count) workflows; $(@($config.models.psobject.Properties).Count) models)" -ForegroundColor Green
    exit 0
}

if ([string]::IsNullOrWhiteSpace($WorkflowId) -or [string]::IsNullOrWhiteSpace($Model) -or [string]::IsNullOrWhiteSpace($UsagePath)) {
    throw '-WorkflowId, -Model, and -UsagePath are required for usage enforcement.'
}
$workflowProperty = $config.workflows.psobject.Properties | Where-Object Name -eq $WorkflowId | Select-Object -First 1
if ($null -eq $workflowProperty) { $failures.Add("Workflow '$WorkflowId' is not enrolled.") }
$modelProperty = $config.models.psobject.Properties | Where-Object Name -eq $Model | Select-Object -First 1
if ($null -eq $modelProperty) { $failures.Add("Model '$Model' has no approved rate card.") }

$inputTokens = [int64]0; $cachedInputTokens = [int64]0; $outputTokens = [int64]0; $reasoningTokens = [int64]0; $turns = 0; $estimatedCost = [decimal]0
if (-not (Test-Path -LiteralPath $UsagePath -PathType Leaf)) {
    $failures.Add("Usage JSONL not found: $UsagePath")
} elseif ($null -ne $workflowProperty -and $null -ne $modelProperty) {
    foreach ($line in @(Get-Content -LiteralPath $UsagePath)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        try { $event = $line | ConvertFrom-Json } catch { $failures.Add('Usage stream contains invalid JSONL.'); continue }
        if ([string]$event.type -ne 'turn.completed' -or $null -eq $event.usage) { continue }
        $turns++
        $turnInput = [int64]$event.usage.input_tokens
        $turnCached = [int64]$event.usage.cached_input_tokens
        $turnOutput = [int64]$event.usage.output_tokens
        $turnReasoning = [int64]$event.usage.reasoning_output_tokens
        if ($turnInput -lt 0 -or $turnCached -lt 0 -or $turnOutput -lt 0 -or $turnReasoning -lt 0 -or $turnCached -gt $turnInput) { $failures.Add('Usage tokens must be non-negative and cached input cannot exceed input.'); continue }
        $inputTokens += $turnInput; $cachedInputTokens += $turnCached; $outputTokens += $turnOutput; $reasoningTokens += $turnReasoning
        $rate = $modelProperty.Value
        $inputMultiplier = [decimal]1; $outputMultiplier = [decimal]1
        if ([int64]$rate.longContextThresholdTokens -gt 0 -and $turnInput -gt [int64]$rate.longContextThresholdTokens) {
            $inputMultiplier = [decimal]$rate.longContextInputMultiplier; $outputMultiplier = [decimal]$rate.longContextOutputMultiplier
        }
        $uncached = $turnInput - $turnCached
        # Codex reports reasoning_output_tokens as the reasoning subset of output_tokens.
        # Preserve it for diagnostics, but do not bill or gate the same tokens twice.
        $estimatedCost += (([decimal]$uncached * [decimal]$rate.inputUsdPerMillion * $inputMultiplier) + ([decimal]$turnCached * [decimal]$rate.cachedInputUsdPerMillion * $inputMultiplier) + ([decimal]$turnOutput * [decimal]$rate.outputUsdPerMillion * $outputMultiplier)) / 1000000
    }
    if ($turns -eq 0) { $failures.Add('Usage stream contains no turn.completed usage event.') }
    $budget = $workflowProperty.Value
    if (@($budget.allowedModels) -notcontains $Model) { $failures.Add("Model '$Model' is not allowed for workflow '$WorkflowId'.") }
    if ($inputTokens -gt [int64]$budget.maxInputTokensPerRun) { $failures.Add("Input-token ceiling exceeded: $inputTokens > $($budget.maxInputTokensPerRun).") }
    $billableOutputTokens = $outputTokens
    if ($billableOutputTokens -gt [int64]$budget.maxOutputTokensPerRun) { $failures.Add("Output-token ceiling exceeded: $billableOutputTokens > $($budget.maxOutputTokensPerRun).") }
    if ($estimatedCost -gt [decimal]$budget.maxEstimatedUsdPerRun) { $failures.Add("Estimated cost ceiling exceeded: $(Format-UsdForReport $estimatedCost) > $(Format-UsdForReport ([decimal]$budget.maxEstimatedUsdPerRun)) USD.") }
}

$passed = $failures.Count -eq 0
if ([string]::IsNullOrWhiteSpace($EvidencePath)) { $EvidencePath = [System.IO.Path]::ChangeExtension($UsagePath, '.budget.json') }
$evidenceDir = Split-Path -Parent $EvidencePath
if (-not [string]::IsNullOrWhiteSpace($evidenceDir)) { New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null }
$evidence = [ordered]@{
    schemaVersion = 1; workflow = $WorkflowId; model = $Model; passed = $passed
    measuredAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    usage = [ordered]@{ turns = $turns; inputTokens = $inputTokens; cachedInputTokens = $cachedInputTokens; outputTokens = $outputTokens; reasoningOutputTokens = $reasoningTokens }
    estimatedStandardApiEquivalentUsd = [Math]::Round($estimatedCost, 6)
    costBasis = [string]$config.costBasis
    failures = @($failures)
}
$evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $EvidencePath -Encoding utf8
if (-not $passed) { Write-Host "agent resource budget failed: $($failures -join ' ')" -ForegroundColor Red; exit 1 }
Write-Host "agent resource budget passed: input=$inputTokens output=$outputTokens estimatedUsd=$(Format-UsdForReport $estimatedCost)" -ForegroundColor Green
exit 0
