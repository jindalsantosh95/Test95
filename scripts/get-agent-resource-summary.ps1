#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$WorkflowId,
    [Parameter(Mandatory)][string[]]$BudgetEvidencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$config = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/agent-resource-budgets.json') -Raw | ConvertFrom-Json
$evidencePaths = @($BudgetEvidencePath | ForEach-Object {
    if ([System.IO.Path]::IsPathRooted($_)) {
        (Resolve-Path -LiteralPath $_).Path
    } else {
        (Resolve-Path -LiteralPath (Join-Path $repoRoot $_)).Path
    }
})
$evidenceItems = @($evidencePaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw | ConvertFrom-Json })
$evidence = $evidenceItems[0]
$workflow = $config.workflows.psobject.Properties | Where-Object Name -eq $WorkflowId | Select-Object -ExpandProperty Value -First 1
if ($null -eq $workflow) { throw "Workflow '$WorkflowId' is not enrolled in docs/agent-resource-budgets.json." }
foreach ($item in $evidenceItems) {
    if ([string]$item.workflow -ne $WorkflowId) { throw "Budget evidence workflow '$($item.workflow)' does not match '$WorkflowId'." }
}
$requiredMaximumFields = @('maxInputTokensPerRun', 'maxOutputTokensPerRun', 'maxEstimatedUsdPerRun')
foreach ($field in $requiredMaximumFields) {
    if ($workflow.psobject.Properties.Name -notcontains $field -or [decimal]$workflow.$field -le 0) {
        throw "Workflow '$WorkflowId' has missing or invalid $field."
    }
}

$culture = [System.Globalization.CultureInfo]::InvariantCulture
function Format-UsdForReport([decimal]$Amount) {
    $roundedUp = [Math]::Ceiling($Amount * 100) / 100
    return $roundedUp.ToString('0.00', $culture)
}

$actualInput = [int64](($evidenceItems | ForEach-Object { [int64]$_.usage.inputTokens } | Measure-Object -Sum).Sum)
$actualCachedInput = [int64](($evidenceItems | ForEach-Object { [int64]$_.usage.cachedInputTokens } | Measure-Object -Sum).Sum)
$actualOutput = [int64](($evidenceItems | ForEach-Object { [int64]$_.usage.outputTokens } | Measure-Object -Sum).Sum)
$actualReasoningOutput = [int64](($evidenceItems | ForEach-Object { [int64]$_.usage.reasoningOutputTokens } | Measure-Object -Sum).Sum)
$actualEstimatedUsd = [decimal](($evidenceItems | ForEach-Object { [decimal]$_.estimatedStandardApiEquivalentUsd } | Measure-Object -Sum).Sum)
$budgetDecisionCount = $evidenceItems.Count
$summary = [ordered]@{
    workflow = $WorkflowId
    model = [string]$evidence.model
    maximum = [ordered]@{
        inputTokens = [int64]$workflow.maxInputTokensPerRun * $budgetDecisionCount
        outputTokens = [int64]$workflow.maxOutputTokensPerRun * $budgetDecisionCount
        estimatedUsd = [decimal]$workflow.maxEstimatedUsdPerRun * $budgetDecisionCount
    }
    actual = [ordered]@{
        totalTokens = $actualInput + $actualOutput
        inputTokens = $actualInput
        cachedInputTokens = $actualCachedInput
        outputTokens = $actualOutput
        reasoningOutputTokens = $actualReasoningOutput
        estimatedUsd = $actualEstimatedUsd
    }
}
$summary.maximumLabel = "{0} input tokens; {1} output tokens; `${2} USD" -f `
    $summary.maximum.inputTokens.ToString('N0', $culture), `
    $summary.maximum.outputTokens.ToString('N0', $culture), `
    (Format-UsdForReport $summary.maximum.estimatedUsd)
$summary.actualLabel = "{0} tokens ({1} input, {2} cached + {3} output, {4} reasoning); `${5} USD" -f `
    $summary.actual.totalTokens.ToString('N0', $culture), `
    $actualInput.ToString('N0', $culture), `
    $actualCachedInput.ToString('N0', $culture), `
    $actualOutput.ToString('N0', $culture), `
    $actualReasoningOutput.ToString('N0', $culture), `
    (Format-UsdForReport $summary.actual.estimatedUsd)

$summary | ConvertTo-Json -Depth 6 -Compress
