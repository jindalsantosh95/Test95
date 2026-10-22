#Requires -Version 5.1
<#
.SYNOPSIS
    Runs the configured coding agent command for sequence-goal human comment follow-up.

.DESCRIPTION
    Set SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON to a JSON payload that resolves to:
      - object: { "executable": "<exe>", "args": ["<arg1>", ...] }
      - array: ["<exe>", "<arg1>", ...]

    Supported placeholders in executable and args:
      {prNumber}        -> pull request number
      {branch}          -> current branch name
      {repoRoot}        -> absolute repository root
      {commentEvidence} -> repo-relative evidence JSON file containing non-AI comments
      {model}           -> model selected for human comment handling

    The agent command must update each comment item in the evidence JSON with:
      decision: agree | disagree | not-applicable
      responseComment: human-readable response to post back to the PR
      actionSummary: what was changed when agreeing, or why no file change was needed
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][int]$PrNumber,
    [Parameter(Mandatory)][string]$CommentEvidencePath,
    [string]$BranchName,
    [string]$UsagePath,
    [string]$BudgetEvidencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

if ([string]::IsNullOrWhiteSpace($BranchName)) {
    $BranchName = (& git -C $repoRoot rev-parse --abbrev-ref HEAD).Trim()
}

if ([string]::IsNullOrWhiteSpace($env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON)) {
    throw 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON is required. Configure it as a repository secret/variable for non-AI comment follow-up.'
}

$humanCommentAgentModel = if (-not [string]::IsNullOrWhiteSpace($env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL)) {
    $env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL
} elseif (-not [string]::IsNullOrWhiteSpace($env:SEQUENCE_GOAL_REVIEW_AGENT_MODEL)) {
    $env:SEQUENCE_GOAL_REVIEW_AGENT_MODEL
} else {
    'gpt-6-sol'
}

$evidenceAbsolute = if ([System.IO.Path]::IsPathRooted($CommentEvidencePath)) {
    (Resolve-Path -LiteralPath $CommentEvidencePath).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repoRoot $CommentEvidencePath)).Path
}

$root = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
$rootUri = New-Object System.Uri($root)
$evidenceUri = New-Object System.Uri($evidenceAbsolute)

if (-not $rootUri.IsBaseOf($evidenceUri)) {
    throw "CommentEvidencePath must resolve under the repository root. Resolved path: $evidenceAbsolute"
}

$evidenceRelative = [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($evidenceUri).ToString())

$commandTemplateJson = $env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON
$template = $commandTemplateJson | ConvertFrom-Json
# The secret is only needed to parse the launch template; clear it before process spawn.
Remove-Item Env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON -ErrorAction SilentlyContinue
[System.Environment]::SetEnvironmentVariable('SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON', $null, 'Process')
$executable = $null
$args = @()

if ($template -is [System.Array]) {
    if ($template.Count -lt 1) {
        throw 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON array must contain at least the executable.'
    }

    $executable = [string]$template[0]
    $args = @($template | Select-Object -Skip 1 | ForEach-Object { [string]$_ })
} elseif ($template.PSObject.Properties.Name -contains 'executable') {
    $executable = [string]$template.executable
    if ($template.PSObject.Properties.Name -contains 'args' -and $null -ne $template.args) {
        $args = @($template.args | ForEach-Object { [string]$_ })
    }
} else {
    throw 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON must be an array [exe,args...] or object { executable, args }.'
}

if ([string]::IsNullOrWhiteSpace($executable)) {
    throw 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON executable cannot be empty.'
}

$replace = @{
    '{prNumber}'        = [string]$PrNumber
    '{branch}'          = $BranchName
    '{repoRoot}'        = $repoRoot
    '{commentEvidence}' = $evidenceRelative
    '{model}'           = $humanCommentAgentModel
}

foreach ($key in $replace.Keys) {
    $executable = $executable.Replace($key, $replace[$key])
}

$resolvedArgs = @()
foreach ($arg in $args) {
    $resolved = $arg
    foreach ($key in $replace.Keys) {
        $resolved = $resolved.Replace($key, $replace[$key])
    }
    $resolvedArgs += $resolved
}

Write-Host "Running sequence-goal human comment agent command for PR #$PrNumber using model $humanCommentAgentModel" -ForegroundColor Cyan
Write-Verbose "Executable: $executable"
if ($resolvedArgs.Count -gt 0) {
    Write-Verbose "Args: $($resolvedArgs -join ' ')"
}

$isCodex = [System.IO.Path]::GetFileNameWithoutExtension($executable) -eq 'codex'
if ($isCodex) {
    $codexConfig = @"
approval_policy = "never"
sandbox_mode = "danger-full-access"
"@
    $codexHome = if ([string]::IsNullOrWhiteSpace($env:CODEX_HOME)) {
        Join-Path ([System.IO.Path]::GetTempPath()) 'sequence-goal-codex-home'
    } else {
        $env:CODEX_HOME
    }

    New-Item -ItemType Directory -Force -Path $codexHome | Out-Null
    $env:CODEX_HOME = $codexHome
    Set-Content -LiteralPath (Join-Path $codexHome 'config.toml') -Value $codexConfig -Encoding UTF8

    if ($env:GITHUB_ACTIONS -eq 'true' -and -not [string]::IsNullOrWhiteSpace($env:USERPROFILE)) {
        $userCodexHome = Join-Path $env:USERPROFILE '.codex'
        New-Item -ItemType Directory -Force -Path $userCodexHome | Out-Null
        Set-Content -LiteralPath (Join-Path $userCodexHome 'config.toml') -Value $codexConfig -Encoding UTF8
    }

    Write-Host "Configured Codex CI permissions: sandbox_mode=danger-full-access, approval_policy=never" -ForegroundColor Cyan

    $normalizedArgs = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $resolvedArgs.Count; $i++) {
        $arg = $resolvedArgs[$i]
        if ($arg -in @('--dangerously-bypass-approvals-and-sandbox', '--yolo')) {
            continue
        }

        if ($arg -in @('--sandbox', '-s', '--ask-for-approval', '-a')) {
            $i++
            continue
        }

        if ($arg -like '--sandbox=*' -or $arg -like '--ask-for-approval=*') {
            continue
        }

        if ($arg -in @('--model', '-m')) {
            $i++
            continue
        }

        if ($arg -like '--model=*') {
            continue
        }

        $normalizedArgs.Add($arg) | Out-Null
    }

    $resolvedArgs = @('--dangerously-bypass-approvals-and-sandbox') + @($normalizedArgs)
    $execIndex = [Array]::IndexOf([object[]]$resolvedArgs, 'exec')
    if ($execIndex -lt 0) { throw 'Budgeted Codex commands must use codex exec.' }
    $suffix = if ($execIndex + 1 -lt $resolvedArgs.Count) { @($resolvedArgs[($execIndex + 1)..($resolvedArgs.Count - 1)]) } else { @() }
    $suffix = @($suffix | Where-Object { $_ -ne '--json' })
    $resolvedArgs = @($resolvedArgs[0..$execIndex]) + @('--json', '--model', $humanCommentAgentModel) + $suffix
    Write-Host "Normalized Codex CI launch args: --dangerously-bypass-approvals-and-sandbox" -ForegroundColor Cyan
} else {
    throw 'Enrolled human-comment execution must use Codex JSONL so token and cost ceilings can be enforced.'
}

if ([string]::IsNullOrWhiteSpace($UsagePath)) { $UsagePath = "TestResults/agent-resource/sequence-goal-human-comment-loop-$($env:GITHUB_RUN_ID).jsonl" }
$UsagePath = if ([System.IO.Path]::IsPathRooted($UsagePath)) { [System.IO.Path]::GetFullPath($UsagePath) } else { [System.IO.Path]::GetFullPath((Join-Path $repoRoot $UsagePath)) }
if ([string]::IsNullOrWhiteSpace($BudgetEvidencePath)) { $BudgetEvidencePath = [System.IO.Path]::ChangeExtension($UsagePath, '.budget.json') }
$BudgetEvidencePath = if ([System.IO.Path]::IsPathRooted($BudgetEvidencePath)) { [System.IO.Path]::GetFullPath($BudgetEvidencePath) } else { [System.IO.Path]::GetFullPath((Join-Path $repoRoot $BudgetEvidencePath)) }
$usageDirectory = Split-Path -Parent $UsagePath
if ($usageDirectory) { New-Item -ItemType Directory -Path $usageDirectory -Force | Out-Null }
Remove-Item -LiteralPath $UsagePath -Force -ErrorAction SilentlyContinue

$hadCodexApiKey = -not [string]::IsNullOrWhiteSpace($env:CODEX_API_KEY)
if (-not $hadCodexApiKey -and -not [string]::IsNullOrWhiteSpace($env:OPENAI_API_KEY)) {
    $env:CODEX_API_KEY = $env:OPENAI_API_KEY
}

Push-Location $repoRoot
try {
    $agentExitCode = 1
    foreach ($attempt in 1..2) {
        if ($attempt -gt 1) {
            Write-Warning "Human-comment agent attempt 1 ended with exit code $agentExitCode. Retrying once with the retained evidence and working tree."
        }
        & $executable @resolvedArgs | Tee-Object -FilePath $UsagePath -Append
        $agentExitCode = $LASTEXITCODE
        if ($agentExitCode -eq 0) { break }
    }
    & (Join-Path $PSScriptRoot 'check-agent-resource-budget.ps1') -WorkflowId 'sequence-goal-human-comment-loop' -Model $humanCommentAgentModel -UsagePath $UsagePath -EvidencePath $BudgetEvidencePath
    $budgetExitCode = $LASTEXITCODE
    if ($agentExitCode -ne 0) { throw "Human-comment agent command failed twice; final exit code $agentExitCode. Combined JSONL and resource evidence were retained for diagnosis." }
    if ($budgetExitCode -ne 0) { throw 'Human-comment agent exhausted its token or cost budget.' }
    $resourceUsage = & (Join-Path $PSScriptRoot 'get-agent-resource-summary.ps1') -WorkflowId 'sequence-goal-human-comment-loop' -BudgetEvidencePath $BudgetEvidencePath | ConvertFrom-Json
    $commentEvidence = Get-Content -LiteralPath $evidenceAbsolute -Raw | ConvertFrom-Json
    $commentEvidence | Add-Member -NotePropertyName resourceUsage -NotePropertyValue $resourceUsage -Force
    $commentEvidence | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $evidenceAbsolute -Encoding UTF8
} finally {
    if (-not $hadCodexApiKey) {
        Remove-Item Env:CODEX_API_KEY -ErrorAction SilentlyContinue
        [System.Environment]::SetEnvironmentVariable('CODEX_API_KEY', $null, 'Process')
    }
    Pop-Location
}

Write-Host "Sequence-goal human comment agent command completed for PR #$PrNumber" -ForegroundColor Green
exit 0

