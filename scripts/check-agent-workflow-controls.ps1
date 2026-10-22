#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = [System.Collections.Generic.List[string]]::new()
$toolRegistry = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/agent-tool-environments.json') -Raw | ConvertFrom-Json
$codexCliVersion = [string]$toolRegistry.profiles.'codex-ci'.pinnedCommandVersions.codex
if ([string]::IsNullOrWhiteSpace($codexCliVersion)) { $failures.Add('The codex-ci tool profile does not pin a Codex CLI version.') }
$workflows = @(
    '.github/workflows/ai-delivery-sequence-goal-loop.yml',
    '.github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml',
    '.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml',
    '.github/workflows/ai-delivery-sequence-goal-associated-pr-feedback.yml',
    '.github/workflows/ai-harness-docs-semantic-gardening.yml',
    '.github/workflows/ai-harness-high-risk-inferential-review.yml'
)
$runnerByWorkflow = @{
    '.github/workflows/ai-delivery-sequence-goal-loop.yml' = 'scripts/run-sequence-goal-agent.ps1'
    '.github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml' = 'scripts/run-sequence-goal-pr-review-agent.ps1'
    '.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml' = 'scripts/run-sequence-goal-human-comment-agent.ps1'
}

foreach ($relativePath in $workflows) {
    $path = Join-Path $repoRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing enrolled agent workflow: $relativePath")
        continue
    }

    $text = Get-Content -LiteralPath $path -Raw
    $resourceText = $text
    if ($runnerByWorkflow.ContainsKey($relativePath)) {
        $runnerPath = Join-Path $repoRoot $runnerByWorkflow[$relativePath]
        if (-not (Test-Path -LiteralPath $runnerPath -PathType Leaf)) { $failures.Add("Missing enrolled agent runner: $($runnerByWorkflow[$relativePath])") } else { $resourceText += "`n" + (Get-Content -LiteralPath $runnerPath -Raw) }
    }
    $literalTimeoutPattern = '(?m)^\s*timeout-minutes:\s*[1-9][0-9]*\s*$'
    $variableTimeoutPattern = "(?m)^\s*timeout-minutes:\s*\$\{\{\s*fromJSON\(vars\.[A-Z][A-Z0-9_]*\s*\|\|\s*'[1-9][0-9]*'\)\s*\}\}\s*$"
    if ($text -notmatch $literalTimeoutPattern -and $text -notmatch $variableTimeoutPattern) {
        $failures.Add("$relativePath has no bounded timeout-minutes value.")
    }
    if ($text -notmatch '(?i)(MAX_ATTEMPTS|max attempts|maxAttempts)') {
        $failures.Add("$relativePath does not declare or enforce an attempt budget.")
    }
    if ($text -notmatch '(?i)(upload-artifact|run evidence|run-evidence|run\.json|evidence)') {
        $failures.Add("$relativePath does not preserve terminal run evidence.")
    }
    if ($text -notmatch '(?i)(OPENAI_API_KEY|CODEX_API_KEY)') {
        $failures.Add("$relativePath does not use the approved agent credential mapping.")
    }
    if ($resourceText -notmatch '(?i)--json') { $failures.Add("$relativePath does not capture Codex JSONL usage.") }
    if ($resourceText -notmatch '(?i)(--model|-Model)') { $failures.Add("$relativePath does not pin the model used for budget evidence.") }
    $escapedCodexVersion = [regex]::Escape($codexCliVersion)
    $installPattern = '@openai/codex@(\$\{CODEX_CLI_VERSION\}|\$env:CODEX_CLI_VERSION|' + $escapedCodexVersion + ')'
    if ($text -notmatch $installPattern) { $failures.Add("$relativePath does not install the registry-pinned Codex CLI version $codexCliVersion.") }
    if ($text -match '(\$\{CODEX_CLI_VERSION\}|\$env:CODEX_CLI_VERSION)' -and $text -notmatch [regex]::Escape("CODEX_CLI_VERSION: '$codexCliVersion'")) { $failures.Add("$relativePath does not bind CODEX_CLI_VERSION to $codexCliVersion.") }
    if ($resourceText -notmatch 'check-agent-resource-budget\.ps1') { $failures.Add("$relativePath does not enforce token and estimated-cost ceilings.") }
    if ($text -notmatch 'agent-resource-') { $failures.Add("$relativePath does not retain resource/cost evidence as an artifact.") }
}

& (Join-Path $PSScriptRoot 'check-agent-resource-budget.ps1') -ValidateConfiguration
if ($LASTEXITCODE -ne 0) { $failures.Add('Agent resource budget configuration is invalid or stale.') }

if ($failures.Count -gt 0) {
    Write-Host 'agent workflow control check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host "agent workflow control check passed ($($workflows.Count) workflows)" -ForegroundColor Green
exit 0
