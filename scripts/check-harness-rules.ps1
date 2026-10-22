#Requires -Version 5.1
<#
.SYNOPSIS
    Validates core harness engineering guardrails with actionable remediation.

.DESCRIPTION
    This script checks the repository harness wiring and docs map for critical
    controls that coding agents rely on. Failures include a remediation hint.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param(
        [Parameter(Mandatory)][string]$Check,
        [Parameter(Mandatory)][string]$Fix
    )

    $failures.Add("$Check`n  Fix: $Fix") | Out-Null
}

function Get-FileText {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $path = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure -Check "Missing required file: $RepoRelativePath" -Fix "Restore or create $RepoRelativePath with the expected harness guidance."
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
    $pattern = "(?ims)^##\s+$escapedHeading\s*$\r?\n(?<section>.*?)(?=^##\s+|\z)"
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) {
        return $null
    }

    return $match.Groups['section'].Value
}

$requiredWorkflowDisplayNames = [ordered]@{
    '.github/workflows/ai-harness-agent-resource-and-cost-trend-review.yml'       = 'ai-harness-agent-resource-and-cost-trend-review'
    '.github/workflows/ai-harness-vaa-eapi-automation.yml'                = 'ai-harness-vaa-eapi-automation'
    '.github/workflows/ai-harness-closed-loop-autonomy.yml'              = 'ai-harness-closed-loop-autonomy'
    '.github/workflows/ai-harness-docs-agent-loop-guard.yml'              = 'ai-harness-docs-agent-loop-guard'
    '.github/workflows/ai-harness-docs-semantic-gardening.yml'            = 'ai-harness-docs-semantic-gardening'
    '.github/workflows/ai-harness-drift-parity.yml'               = 'ai-harness-drift-parity'
    '.github/workflows/ai-harness-high-risk-autolabel.yml'                = 'ai-harness-high-risk-autolabel'
    '.github/workflows/ai-harness-high-risk-coverage.yml'         = 'ai-harness-high-risk-coverage'
    '.github/workflows/ai-harness-high-risk-inferential-review.yml'       = 'ai-harness-high-risk-inferential-review'
    '.github/workflows/ai-harness-mermaid-validation.yml'                 = 'ai-harness-mermaid-validation'
    '.github/workflows/ai-harness-repository-parity.yml'                = 'ai-harness-repository-parity'
    '.github/workflows/ai-delivery-sequence-goal-associated-pr-monitor.yml' = 'ai-delivery-sequence-goal-associated-pr-monitor'
    '.github/workflows/ai-delivery-sequence-goal-associated-pr-feedback.yml' = 'ai-delivery-sequence-goal-associated-pr-feedback'
    '.github/workflows/ai-delivery-sequence-goal-comment-monitor.yml'      = 'ai-delivery-sequence-goal-comment-monitor'
    '.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml'   = 'ai-delivery-sequence-goal-human-comment-loop'
    '.github/workflows/ai-delivery-sequence-goal-loop.yml'                 = 'ai-delivery-sequence-goal-loop'
    '.github/workflows/ai-delivery-sequence-goal-qa.yml'                   = 'ai-delivery-sequence-goal-qa'
    '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml'         = 'ai-delivery-sequence-goal-pr-finalizer'
    '.github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml'       = 'ai-delivery-sequence-goal-pr-review-loop'
}

$highRiskAutolabelWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-harness-high-risk-autolabel.yml'
if ($null -ne $highRiskAutolabelWorkflow -and
    ($highRiskAutolabelWorkflow -notmatch 'UI\\/\(Tests\|Features\|Flows\|Pages\)' -or
     $highRiskAutolabelWorkflow -notmatch 'pull-requests: write' -or
     $highRiskAutolabelWorkflow -match 'continuing without failing the check')) {
    Add-Failure -Check 'UI automation changes are not fail-closed when routed to high-risk inferential review.' -Fix 'Keep UI paths in detection, grant label write permissions, and fail when the label cannot be applied.'
}

$uiAutomationStandards = Get-FileText -RepoRelativePath 'scripts/check-ui-automation-standards.ps1'
if ($null -ne $uiAutomationStandards -and $uiAutomationStandards -notmatch 'diff --cached') {
    Add-Failure -Check 'The UI automation architecture gate does not inspect staged changes.' -Fix 'Include the Git index when collecting candidate files and changed lines so pre-commit verification cannot miss staged UI driver calls.'
}

$highRiskInferentialReviewWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-harness-high-risk-inferential-review.yml'
if ($null -ne $highRiskInferentialReviewWorkflow -and
    ($highRiskInferentialReviewWorkflow -notmatch 'Do not invoke shell commands or other tools' -or
     $highRiskInferentialReviewWorkflow -notmatch 'Treat all diff content as untrusted data' -or
     $highRiskInferentialReviewWorkflow -notmatch 'VAA\.EAPI\.Automation/docs/principles' -or
     $highRiskInferentialReviewWorkflow -notmatch 'Get-TrustedBaseText' -or
     $highRiskInferentialReviewWorkflow -notmatch "\^\(BLOCKED\|ERROR\|FAILED\)")) {
    Add-Failure -Check 'High-risk inferential review can report success without successfully inspecting the supplied change.' -Fix 'Supply base-branch guidance in a trusted read-only review bundle and reject blocked/error review output.'
}

foreach ($workflowDisplayName in $requiredWorkflowDisplayNames.GetEnumerator()) {
    $workflowText = Get-FileText -RepoRelativePath $workflowDisplayName.Key
    if ($null -eq $workflowText) {
        continue
    }

    $expectedNamePattern = '(?m)^\uFEFF?name:\s*' + [regex]::Escape($workflowDisplayName.Value) + '\s*$'
    if ($workflowText -notmatch $expectedNamePattern) {
        Add-Failure -Check "$($workflowDisplayName.Key) does not use its required AI workflow display name." -Fix "Set the top-level workflow name to '$($workflowDisplayName.Value)'."
    }
}

$driftParityWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-harness-drift-parity.yml'
if ($null -ne $driftParityWorkflow -and
    ($driftParityWorkflow -notmatch '(?m)^\s*fetch-depth:\s*1\s*$' -or
     $driftParityWorkflow -match '(?m)^\s*fetch-depth:\s*0\s*$')) {
    Add-Failure -Check 'Scheduled drift parity does not explicitly pin a depth-one checkout or allows a full-ref checkout on Windows.' -Fix 'Set fetch-depth to exactly 1 so case-colliding remote branch names cannot prevent the harness from running.'
}

$mermaidValidationWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-harness-mermaid-validation.yml'
if ($null -ne $mermaidValidationWorkflow -and
    ($mermaidValidationWorkflow -notmatch 'git fetch --no-tags origin' -or
     $mermaidValidationWorkflow -notmatch 'refs/remotes/origin/\$\{\{ github\.base_ref \}\}')) {
    Add-Failure -Check 'Mermaid validation workflow does not explicitly fetch the pull-request base ref before diffing.' -Fix 'Fetch the PR base into refs/remotes/origin/${{ github.base_ref }} before invoking check-mermaid-diagrams.ps1.'
}
if ($null -ne $mermaidValidationWorkflow -and
    ($mermaidValidationWorkflow -notmatch "rev-parse --verify 'HEAD~1'" -or
     $mermaidValidationWorkflow -notmatch "else \{ 'HEAD' \}")) {
    Add-Failure -Check 'Mermaid validation manual dispatch does not handle a missing HEAD~1 ref.' -Fix 'Use HEAD~1 when resolvable and fall back to HEAD for single-commit histories.'
}

$mermaidValidationScript = Get-FileText -RepoRelativePath 'scripts/check-mermaid-diagrams.ps1'
$mermaidVerifyScript = Get-FileText -RepoRelativePath 'scripts/verify.ps1'
if ($null -ne $mermaidVerifyScript -and
    ($mermaidVerifyScript -notmatch "check-mermaid-diagrams\.ps1'\) -BaseRef [^\r\n]+ -LintOnly" -or
     $mermaidVerifyScript -notmatch "invoke-mermaid-validation-exercise\.ps1'\) -LintOnly")) {
    Add-Failure -Check 'Local verification is missing Mermaid authoring lint or its regression exercise.' -Fix 'Run changed-Markdown Mermaid lint and invoke-mermaid-validation-exercise.ps1 -LintOnly in verify.ps1, including fast mode.'
}
if ($null -ne $mermaidValidationScript -and $mermaidValidationScript -notmatch '\$\{BaseRef\}\.\.\.HEAD') {
    Add-Failure -Check 'Mermaid validation changed-file discovery does not use a merge-base comparison.' -Fix 'Use ${BaseRef}...HEAD so base-branch-only Markdown is not attributed to the delivery branch.'
}

$agents = Get-FileText -RepoRelativePath 'AGENTS.md'
if ($null -ne $agents) {
    if ($agents -notmatch 'docs/readme\.md') {
        Add-Failure -Check 'AGENTS.md does not point to docs/readme.md as the maintained map.' -Fix 'Add docs/readme.md to the AGENTS.md entrypoint and reading order.'
    }
}

$docsReadme = Get-FileText -RepoRelativePath 'docs/readme.md'
if ($null -ne $docsReadme) {
    $requiredMapEntries = @(
        'exec-plans/readme.md',
        'decisions/readme.md',
        'tech-debt-tracker.md',
        'harness-engineering.md',
        'agent-memory.md',
        'knowledge-repo-guide.md'
    )

    foreach ($entry in $requiredMapEntries) {
        if ($docsReadme -notmatch [regex]::Escape($entry)) {
            Add-Failure -Check "docs/readme.md is missing map entry: $entry" -Fix "Add $entry to the documentation map so agents can discover it."
        }
    }
}

$verifyScript = Get-FileText -RepoRelativePath 'scripts/verify.ps1'
if ($null -ne $verifyScript) {
    if ($verifyScript -notmatch 'check-knowledge-health\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run knowledge health checks.' -Fix 'Wire scripts/check-knowledge-health.ps1 into verify.ps1 default flow.'
    }

    if ($verifyScript -notmatch 'check-harness-rules\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run harness rules lint.' -Fix 'Wire scripts/check-harness-rules.ps1 into verify.ps1 default flow.'
    }

    if ($verifyScript -notmatch 'invoke-sequence-goal-inferential-review-normalization-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run the sequence-goal inferential-review normalization regression exercise.' -Fix 'Wire scripts/invoke-sequence-goal-inferential-review-normalization-exercise.ps1 into the default verifier flow.'
    }

    if ($verifyScript -notmatch 'invoke-sequence-goal-associated-pr-recorder-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run the sequence-goal associated-PR recorder regression exercise.' -Fix 'Wire scripts/invoke-sequence-goal-associated-pr-recorder-exercise.ps1 into the default verifier flow.'
    }

    if ($verifyScript -notmatch 'check-preferred-extensions\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run preferred shared-extension checks.' -Fix 'Wire scripts/check-preferred-extensions.ps1 into verify.ps1 default flow.'
    }
    if ($verifyScript -notmatch 'check-test-standards\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not enforce unit-test coding standards.' -Fix 'Wire scripts/check-test-standards.ps1 into verify.ps1 default flow.'
    }

    foreach ($requiredTestEvidenceToken in @('--results-directory', 'trx;LogFileName=', '--collect:XPlat Code Coverage', 'analyse-test-run-results.ps1', '-TrxPath')) {
        if ($verifyScript -notmatch [regex]::Escape($requiredTestEvidenceToken)) {
            Add-Failure -Check "scripts/verify.ps1 does not enforce test evidence token: $requiredTestEvidenceToken" -Fix 'Emit run-scoped TRX and coverage, then invoke analyse-test-run-results.ps1 for each test project.'
        }
    }

    $hasAgentMemoryPathBinding = $verifyScript -match '(?ms)\$agentMemoryCheck\s*=\s*Join-Path\s+\$PSScriptRoot\s+''check-agent-memory-state\.ps1'''
    $hasAgentMemoryInvocation = $verifyScript -match '(?ms)&\s+\$agentMemoryCheck(\s|$)'
    if (-not ($hasAgentMemoryPathBinding -and $hasAgentMemoryInvocation)) {
        Add-Failure -Check 'scripts/verify.ps1 does not wire and invoke agent-memory state checks via script path binding.' -Fix "Bind `$agentMemoryCheck = Join-Path `$PSScriptRoot 'check-agent-memory-state.ps1' and invoke it with '& `$agentMemoryCheck' in verify.ps1 default flow."
    }

    if ($verifyScript -notmatch 'check-sequence-diagram-status\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run sequence-diagram status checks.' -Fix 'Wire scripts/check-sequence-diagram-status.ps1 into verify.ps1 default flow.'
    }

    if ($verifyScript -notmatch 'check-docs-harness-support\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run docs harness support checks.' -Fix 'Wire scripts/check-docs-harness-support.ps1 into verify.ps1 default flow.'
    }

    if ($verifyScript -notmatch 'check-operational-harness\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run operational harness registry checks.' -Fix 'Wire scripts/check-operational-harness.ps1 into verify.ps1 default flow.'
    }

    if ($verifyScript -notmatch 'check-agent-workflow-controls\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run agent workflow execution-control checks.' -Fix 'Wire scripts/check-agent-workflow-controls.ps1 into verify.ps1 default flow.'
    }
    if ($verifyScript -notmatch 'check-agent-observability\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not run agent-readable observability checks.' -Fix 'Wire scripts/check-agent-observability.ps1 into verify.ps1 default flow.'
    }
    if ($verifyScript -notmatch 'invoke-agent-recovery-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise agent recovery and diagnostic replay.' -Fix 'Wire scripts/invoke-agent-recovery-exercise.ps1 into verify.ps1.'
    }
    if ($verifyScript -notmatch 'invoke-self-correcting-loop-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise the general self-correcting agent loop.' -Fix 'Wire scripts/invoke-self-correcting-loop-exercise.ps1 into verify.ps1.'
    }
    if ($verifyScript -notmatch 'invoke-behavior-harness-mutation-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise behavior-harness mutation strength.' -Fix 'Wire scripts/invoke-behavior-harness-mutation-exercise.ps1 into verify.ps1.'
    }
    if ($verifyScript -notmatch 'invoke-architecture-fitness-mutation-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise repository architecture fitness.' -Fix 'Wire scripts/invoke-architecture-fitness-mutation-exercise.ps1 into verify.ps1.'
    }
    if ($verifyScript -notmatch 'invoke-agent-memory-lifecycle-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise automatic memory lifecycle and cross-process resume.' -Fix 'Wire scripts/invoke-agent-memory-lifecycle-exercise.ps1 into verify.ps1.'
    }
    if ($verifyScript -notmatch 'invoke-agent-resource-budget-exercise\.ps1') {
        Add-Failure -Check 'scripts/verify.ps1 does not exercise resource and cost ceilings.' -Fix 'Wire scripts/invoke-agent-resource-budget-exercise.ps1 into verify.ps1.'
    }
    foreach ($requiredExercise in @('invoke-agent-tool-environment-exercise.ps1', 'invoke-agent-observability-exercise.ps1', 'invoke-harness-maturity-evidence-exercise.ps1')) {
        if ($verifyScript -notmatch [regex]::Escape($requiredExercise)) { Add-Failure -Check "scripts/verify.ps1 does not invoke $requiredExercise." -Fix 'Wire all operational /docs exercises into verify.ps1.' }
    }
}

$sequenceDiagramStatusScript = Get-FileText -RepoRelativePath 'scripts/check-sequence-diagram-status.ps1'
if ($null -ne $sequenceDiagramStatusScript -and
    ($sequenceDiagramStatusScript -notmatch 'UX design inspection matrix remains planned' -or
     $sequenceDiagramStatusScript -notmatch '\$fileStatus -eq ''Complete''' -or
     $sequenceDiagramStatusScript -notmatch '\$designInspectionMatrix\.Success')) {
    Add-Failure -Check 'Complete sequence diagrams can retain a planned UX design inspection matrix.' -Fix 'Reject a planned Design inspection matrix when a supplied-UX diagram is Complete.'
}

$preCommit = Get-FileText -RepoRelativePath '.githooks/pre-commit'
if ($null -ne $preCommit) {
    if ($preCommit -notmatch 'verify-staged\.ps1') {
        Add-Failure -Check '.githooks/pre-commit does not run scripts/verify-staged.ps1.' -Fix 'Use scripts/verify-staged.ps1 -SkipBuild in pre-commit before knowledge update.'
    }

    if ($preCommit -notmatch 'update-knowledge-repo\.ps1') {
        Add-Failure -Check '.githooks/pre-commit does not update docs/knowledge-repo.md.' -Fix 'Call scripts/update-knowledge-repo.ps1 -FromStaged and stage docs/knowledge-repo.md.'
    }
}

$harnessEngineering = Get-FileText -RepoRelativePath 'docs/harness-engineering.md'
if ($null -ne $harnessEngineering) {
    if ($harnessEngineering -notmatch 'verify-staged\.ps1 -SkipBuild') {
        Add-Failure -Check 'docs/harness-engineering.md does not document staged-file verification.' -Fix 'Document scripts/verify-staged.ps1 -SkipBuild in harness usage.'
    }

    if ($harnessEngineering -notmatch 'check-knowledge-health\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document knowledge health checks.' -Fix 'Document scripts/check-knowledge-health.ps1 in harness usage.'
    }

    if ($harnessEngineering -notmatch 'check-sequence-diagram-status\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document sequence-diagram status checks.' -Fix 'Document scripts/check-sequence-diagram-status.ps1 in harness usage.'
    }

    if ($harnessEngineering -notmatch 'check-mermaid-diagrams\.ps1' -or $harnessEngineering -notmatch 'invoke-mermaid-validation-exercise\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document Mermaid render validation and its regression exercise.' -Fix 'Document scripts/check-mermaid-diagrams.ps1 and scripts/invoke-mermaid-validation-exercise.ps1 in harness usage.'
    }

    if ($harnessEngineering -notmatch 'check-docs-harness-support\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document docs harness support checks.' -Fix 'Document scripts/check-docs-harness-support.ps1 in harness usage.'
    }

    if ($harnessEngineering -notmatch 'check-preferred-extensions\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document preferred shared-extension checks.' -Fix 'Document scripts/check-preferred-extensions.ps1 and the required .Is(...) convention.'
    }

    if ($harnessEngineering -notmatch 'ai-delivery-sequence-goal-pr-review-loop\.yml' -or $harnessEngineering -notmatch 'run-sequence-goal-pr-review-agent\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document sequence-goal PR review loop controls.' -Fix 'Document .github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml and scripts/run-sequence-goal-pr-review-agent.ps1 in harness engineering.'
    }

    if ($harnessEngineering -notmatch 'ai-delivery-sequence-goal-human-comment-loop\.yml' -or $harnessEngineering -notmatch 'run-sequence-goal-human-comment-agent\.ps1') {
        Add-Failure -Check 'docs/harness-engineering.md does not document sequence-goal human comment loop controls.' -Fix 'Document .github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml and scripts/run-sequence-goal-human-comment-agent.ps1 in harness engineering.'
    }

    if ($harnessEngineering -notmatch 'ai-delivery-sequence-goal-pr-finalizer\.yml') {
        Add-Failure -Check 'docs/harness-engineering.md does not document sequence-goal PR finalizer controls.' -Fix 'Document .github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml in harness engineering.'
    }

    if ($harnessEngineering -notmatch 'codex","args":\["--dangerously-bypass-approvals-and-sandbox","exec"' -or $harnessEngineering -match 'codex","args":\["exec","--sandbox","workspace-write"|codex","args":\["run"') {
        Add-Failure -Check 'docs/harness-engineering.md does not document non-interactive Codex CLI execution.' -Fix 'Use codex --dangerously-bypass-approvals-and-sandbox exec inside the isolated GitHub Actions runner, not codex run or codex exec --sandbox workspace-write, in sequence-goal agent command examples.'
    }

    if ($harnessEngineering -notmatch 'Agent delivery constraints') {
        Add-Failure -Check 'docs/harness-engineering.md does not tell implementation agents to use diagram delivery constraints.' -Fix 'Update the sequence-goal command example so coding agents read and follow the diagram Agent delivery constraints section.'
    }
}

$preferredExtensionsCheck = Get-FileText -RepoRelativePath 'scripts/check-preferred-extensions.ps1'
if ($null -ne $preferredExtensionsCheck -and $preferredExtensionsCheck -notmatch 'StringComparison\\\.OrdinalIgnoreCase') {
    Add-Failure -Check 'scripts/check-preferred-extensions.ps1 does not detect OrdinalIgnoreCase static comparisons.' -Fix 'Restore the required detector for string.Equals(..., StringComparison.OrdinalIgnoreCase).'
}

$reviewTeamLabelsWorkflow = Get-FileText -RepoRelativePath '.github/workflows/review-team-labels.yml'
if ($null -ne $reviewTeamLabelsWorkflow) {
    if ($reviewTeamLabelsWorkflow -notmatch '(?m)^\s+pull-requests:\s+write\s*$') {
        Add-Failure -Check 'Review Team Labels cannot update pull request labels.' -Fix 'Grant pull-requests: write in .github/workflows/review-team-labels.yml.'
    }

    if ($reviewTeamLabelsWorkflow -notmatch [regex]::Escape('context.payload.pull_request?.base?.ref')) {
        Add-Failure -Check 'Review Team Labels does not load configuration from the current base branch.' -Fix 'Resolve review-team-labels.json from pull_request.base.ref so long-lived pull requests do not use a base SHA that predates the configuration file.'
    }

    if ($reviewTeamLabelsWorkflow -match [regex]::Escape('context.payload.pull_request?.base?.sha')) {
        Add-Failure -Check 'Review Team Labels still resolves configuration from a pull request base SHA.' -Fix 'Use pull_request.base.ref so configuration remains available for long-lived pull requests.'
    }
}

$agentMemoryDoc = Get-FileText -RepoRelativePath 'docs/agent-memory.md'
if ($null -ne $agentMemoryDoc) {
    if ($agentMemoryDoc -notmatch 'session-state\.example\.json' -or $agentMemoryDoc -notmatch 'session-state\.json') {
        Add-Failure -Check 'docs/agent-memory.md does not define the tracked contract and local session-state artifacts.' -Fix 'Document both .codex/state/session-state.example.json and .codex/state/session-state.json.'
    }
}

$agentMemoryCheck = Get-FileText -RepoRelativePath 'scripts/check-agent-memory-state.ps1'
if ($null -eq $agentMemoryCheck) {
    Add-Failure -Check 'Missing scripts/check-agent-memory-state.ps1.' -Fix 'Add scripts/check-agent-memory-state.ps1 and wire it into scripts/verify.ps1.'
}

foreach ($memoryLifecycleFile in @(
    'scripts/update-agent-memory-state.ps1',
    'scripts/invoke-agent-memory-lifecycle-exercise.ps1',
    'scripts/check-agent-memory-lifecycle.ps1'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $memoryLifecycleFile) -PathType Leaf)) {
        Add-Failure -Check "Missing $memoryLifecycleFile." -Fix 'Restore the automatic, revisioned memory lifecycle and its deterministic exercise.'
    }
}

foreach ($resourceBudgetFile in @(
    'docs/agent-resource-and-cost-controls.md',
    'docs/agent-resource-budgets.json',
    'scripts/check-agent-resource-budget.ps1',
    'scripts/get-agent-resource-summary.ps1',
    'scripts/check-agent-resource-trend.ps1',
    'scripts/invoke-agent-resource-budget-exercise.ps1',
    'scripts/check-agent-resource-budget-strength.ps1',
    '.github/workflows/ai-harness-agent-resource-and-cost-trend-review.yml'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $resourceBudgetFile) -PathType Leaf)) {
        Add-Failure -Check "Missing $resourceBudgetFile." -Fix 'Restore per-run token/cost enforcement and monthly trend review.'
    }
}

foreach ($operationalControlFile in @(
    'docs/agent-tool-and-executable-environments.md',
    'docs/agent-tool-environments.json',
    'scripts/check-agent-tool-environment.ps1',
    'scripts/invoke-agent-tool-environment-exercise.ps1',
    'scripts/check-agent-tool-environment-strength.ps1',
    'scripts/query-agent-observability.ps1',
    'scripts/invoke-agent-observability-exercise.ps1',
    'scripts/check-agent-observability-strength.ps1',
    'docs/harness-continuous-maturity.md',
    'docs/harness-maturity-policy.json',
    'scripts/update-harness-maturity-evidence.ps1',
    'scripts/check-harness-maturity-evidence.ps1',
    'scripts/invoke-harness-maturity-evidence-exercise.ps1',
    'scripts/check-harness-maturity-evidence-strength.ps1'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $operationalControlFile) -PathType Leaf)) { Add-Failure -Check "Missing $operationalControlFile." -Fix 'Restore tool-environment, observability-query, and continuous-maturity controls.' }
}

$sessionStartHook = Get-FileText -RepoRelativePath '.codex/hooks/session_start_context.ps1'
if ($null -ne $sessionStartHook -and $sessionStartHook -notmatch '(?s)update-agent-memory-state\.ps1.+-Action Resume') {
    Add-Failure -Check 'SessionStart does not automatically initialize/resume durable memory.' -Fix 'Call update-agent-memory-state.ps1 -Action Resume from the session-start hook.'
}
$postToolHook = Get-FileText -RepoRelativePath '.codex/hooks/post_tool_use_feedback.ps1'
if ($null -ne $postToolHook -and $postToolHook -notmatch 'RecordVerification') {
    Add-Failure -Check 'PostToolUse does not persist verification outcomes.' -Fix 'Record verification commands and results through the memory lifecycle command.'
}
$stopHook = Get-FileText -RepoRelativePath '.codex/hooks/stop_verify_reminder.ps1'
if ($null -ne $stopHook -and $stopHook -notmatch '(?s)update-agent-memory-state\.ps1.+-Action Checkpoint') {
    Add-Failure -Check 'Stop does not checkpoint durable memory.' -Fix 'Call update-agent-memory-state.ps1 -Action Checkpoint from the stop hook.'
}

if (-not (Test-Path -LiteralPath (Join-Path $repoRoot '.codex/state/session-state.example.json') -PathType Leaf)) {
    Add-Failure -Check 'Missing .codex/state/session-state.example.json contract artifact.' -Fix 'Add the tracked example using the required contract in docs/agent-memory.md.'
}

$gitIgnore = Get-FileText -RepoRelativePath '.gitignore'
if ($null -ne $gitIgnore -and $gitIgnore -notmatch '(?m)^/\.codex/state/session-state\.json\r?$') {
    Add-Failure -Check '.codex/state/session-state.json is not ignored.' -Fix 'Ignore the local session state so concurrent engineers do not create merge conflicts.'
}

$trackedLocalState = @(& git -C $repoRoot ls-files -- '.codex/state/session-state.json')
if ($trackedLocalState -contains '.codex/state/session-state.json') {
    Add-Failure -Check '.codex/state/session-state.json is tracked by Git.' -Fix 'Remove the local session state from the Git index; only session-state.example.json may be committed.'
}

$behaviorHarness = Get-FileText -RepoRelativePath 'docs/behavior-harness.md'
if ($null -ne $behaviorHarness) {
    $sensorCatalog = Get-MarkdownSectionText -Text $behaviorHarness -Heading 'Sensor Catalog'
    if ([string]::IsNullOrWhiteSpace($sensorCatalog)) {
        Add-Failure -Check 'docs/behavior-harness.md is missing the Sensor Catalog section.' -Fix 'Add a Sensor Catalog section with concrete behavior sensor rows.'
    } else {
        $hasPlaceholder = $sensorCatalog -match '(?im)^\|\s*`?<flow or endpoint>`?\s*\|'
        $hasConcreteEntry = $sensorCatalog -match '(?im)^\|\s*(?!`?<flow or endpoint>`?)(?!Behavior\s*\|)(?!---\|)([^|]+)\|([^|]+)\|([^|]+)\|'
        if ($hasPlaceholder -and -not $hasConcreteEntry) {
            Add-Failure -Check 'docs/behavior-harness.md Sensor Catalog still has only template placeholders and no concrete behavior sensors.' -Fix 'Replace Sensor Catalog placeholder rows with at least one implemented flow mapped to a real test/script sensor.'
        }
    }
}

$sequenceDiagrams = Get-FileText -RepoRelativePath 'docs/sequence-diagrams.md'
if ($null -ne $sequenceDiagrams) {
    if ($sequenceDiagrams -notmatch 'sequence-diagrams/_template\.sequence-diagram\.md') {
        Add-Failure -Check 'docs/sequence-diagrams.md does not reference the sequence diagram template.' -Fix 'Add a Template section linking docs/sequence-diagrams/_template.sequence-diagram.md.'
    }

    if ($sequenceDiagrams -notmatch 'sequence-goal PR review loop') {
        Add-Failure -Check 'docs/sequence-diagrams.md does not explain the post-PR sequence-goal review loop.' -Fix 'Document that generated PRs continue through AI review resolution and approval before the loop stops.'
    }

    if ($sequenceDiagrams -notmatch 'human-comment loop') {
        Add-Failure -Check 'docs/sequence-diagrams.md does not explain the post-PR non-AI comment loop.' -Fix 'Document that generated PRs respond to non-AI comments without resolving human review threads.'
    }

    if ($sequenceDiagrams -notmatch "default unstyled presentation" -or
        $sequenceDiagrams -notmatch 'do not add background or fill colours, borders, boundary boxes or containers' -or
        $sequenceDiagrams -notmatch 'actual project name where one exists') {
        Add-Failure -Check 'Sequence-diagram guidance is missing the generated visual-style or project-label rules.' -Fix 'Require unstyled diagrams without visual boundaries and label repository resources with applicable project names.'
    }

    if ($sequenceDiagrams -notmatch 'PR finalizer') {
        Add-Failure -Check 'docs/sequence-diagrams.md does not explain the closed-PR finalizer.' -Fix 'Document that completion evidence travels in the coordinator PR and closed-unmerged PRs archive abandonment evidence on their source branch.'
    }

    if ($sequenceDiagrams -notmatch 'Agent delivery constraints') {
        Add-Failure -Check 'docs/sequence-diagrams.md does not require delivery constraints for code-generation goals.' -Fix 'Document the Agent delivery constraints section so Ready goals can guide autonomous implementation without broad discovery.'
    }
    if ($sequenceDiagrams -notmatch 'UX contract' -or
        $sequenceDiagrams -notmatch 'direct implementation instruction' -or
        $sequenceDiagrams -notmatch 'sequence diagram remains authoritative for flow and behaviour') {
        Add-Failure -Check 'Sequence-diagram guidance does not make supplied UX authoritative.' -Fix 'Require a UX contract that treats supplied artefacts as direct UI instructions and maps them to sequence-owned flow and behaviour.'
    }
}

$sequenceGoalCoordinatorWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-loop.yml'
$sequenceGoalQaWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-qa.yml'
$sequenceGoalDeliveryResolver = Get-FileText -RepoRelativePath 'scripts/resolve-sequence-goal-delivery-agents.ps1'
$goalDocumentPublisher = Get-FileText -RepoRelativePath 'scripts/save-sequence-goal-delivery.ps1'
$goalDocumentUpdater = Get-FileText -RepoRelativePath 'scripts/update-sequence-goal-document.ps1'
$sequenceGoalWorkflow = @($sequenceGoalCoordinatorWorkflow, $sequenceGoalQaWorkflow, $sequenceGoalDeliveryResolver, $goalDocumentPublisher) -join "`n"
$associatedPrMonitorWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-associated-pr-monitor.yml'
$associatedPrFeedbackWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-associated-pr-feedback.yml'
$inferentialReviewNormalizer = Get-FileText -RepoRelativePath 'scripts/normalize-sequence-goal-inferential-review.ps1'
$draftPrExclusionPattern = '(?:!\s*[A-Za-z_$][\w$]*(?:\.|\?\.)draft\b|[A-Za-z_$][\w$]*(?:\.|\?\.)draft\s*={2,3}\s*false\b|false\s*={2,3}\s*[A-Za-z_$][\w$]*(?:\.|\?\.)draft\b)'
if ($null -ne $sequenceGoalWorkflow) {
    $productionSnapshotIndex = $sequenceGoalWorkflow.IndexOf('$productionPrJson = gh pr view')
    $qaTokenRemovalIndex = if ($productionSnapshotIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('Remove-Item Env:GH_TOKEN', $productionSnapshotIndex) } else { -1 }
    $productionContextArgumentIndex = if ($qaTokenRemovalIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('-ProductionContextPath $productionContextPath', $qaTokenRemovalIndex) } else { -1 }
    $productionContextCleanupIndex = if ($productionContextArgumentIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('Cannot safely remove an unconsumed production snapshot', $productionContextArgumentIndex) } else { -1 }
    $qaHandoffIndex = if ($productionContextArgumentIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('sequence-goal-qa-handoff-', $productionContextArgumentIndex) } else { -1 }
    $trustedCheckoutIndex = if ($qaHandoffIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('name: Checkout trusted QA publication workspace', $qaHandoffIndex) } else { -1 }
    $trustedPublishIndex = if ($trustedCheckoutIndex -ge 0) { $sequenceGoalWorkflow.IndexOf('name: Publish QA delivery from trusted checkout', $trustedCheckoutIndex) } else { -1 }
    $qaAutomationAgentStepMatch = [regex]::Match($sequenceGoalWorkflow, '(?ms)^\s{6}- name: Generate automation tests for implementation\s*$.*?(?=^\s{6}- name:|\z)')
    $qaAutomationAgentStep = if ($qaAutomationAgentStepMatch.Success) { $qaAutomationAgentStepMatch.Value } else { '' }
    $qaAutomationAgentRunMatch = [regex]::Match($qaAutomationAgentStep, '(?ms)^\s{8}run:\s*\|\s*$\r?\n(?<body>.*)')
    if (-not $qaAutomationAgentStepMatch.Success -or
        -not $qaAutomationAgentRunMatch.Success -or
        $qaAutomationAgentStep -notmatch 'DELIVERY_SURFACE: \$\{\{ matrix\.surface \}\}' -or
        $qaAutomationAgentStep -notmatch 'DELIVERY_AGENT: \$\{\{ matrix\.agent \}\}' -or
        $qaAutomationAgentStep -notmatch 'DELIVERY_REPOSITORY: \$\{\{ matrix\.repository \}\}' -or
        $qaAutomationAgentStep -notmatch 'DELIVERY_BASE_BRANCH: \$\{\{ matrix\.base_branch \}\}' -or
        $qaAutomationAgentRunMatch.Groups['body'].Value -match '\$\{\{' -or
        $qaAutomationAgentRunMatch.Groups['body'].Value -notmatch '\$surface = \$env:DELIVERY_SURFACE' -or
        $qaAutomationAgentRunMatch.Groups['body'].Value -notmatch '\$agent = \$env:DELIVERY_AGENT' -or
        $qaAutomationAgentRunMatch.Groups['body'].Value -notmatch '\$productionRepository -cne \$env:DELIVERY_REPOSITORY' -or
        $qaAutomationAgentRunMatch.Groups['body'].Value -notmatch '\$productionBaseBranch -cne \$env:DELIVERY_BASE_BRANCH') {
        Add-Failure -Check 'Sequence-goal QA automation embeds GitHub expressions in its oversized run block or omits a required environment mapping.' -Fix 'Keep all four matrix values in the QA step environment and consume only their runtime environment variables inside the PowerShell run block.'
    }
    if ($productionSnapshotIndex -lt 0 -or $qaTokenRemovalIndex -le $productionSnapshotIndex -or $productionContextArgumentIndex -le $qaTokenRemovalIndex -or $productionContextCleanupIndex -le $productionContextArgumentIndex -or
        $qaHandoffIndex -le $productionContextCleanupIndex -or $trustedCheckoutIndex -le $qaHandoffIndex -or $trustedPublishIndex -le $trustedCheckoutIndex -or
        $sequenceGoalWorkflow -notmatch 'productionPr\.author\.login -notin \$trustedAuthors' -or
        $sequenceGoalWorkflow -notmatch 'productionPrRecheck\.headRefOid -ne \$productionPr\.headRefOid' -or
        $sequenceGoalWorkflow -notmatch 'Production context ancestor cannot be a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Remove-Item Env:OPENAI_API_KEY' -or
        $sequenceGoalWorkflow -notmatch 'Remove-Item Env:CODEX_API_KEY' -or
        $sequenceGoalWorkflow -notmatch 'Remove-Item Env:SEQUENCE_GOAL_AGENT_COMMAND_JSON' -or
        $sequenceGoalWorkflow -notmatch "@\('GITHUB_ENV', 'GITHUB_PATH', 'GITHUB_OUTPUT', 'GITHUB_STEP_SUMMARY'\)" -or
        $sequenceGoalWorkflow -notmatch 'QA agent modified GitHub Actions command file' -or
        $sequenceGoalWorkflow -notmatch 'Protected Git metadata directory cannot be a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Worktree \.git entry cannot be a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Protected Git metadata file cannot be a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'infoExcludeHash' -or
        $sequenceGoalWorkflow -notmatch '\$infoExcludePath\s*\r?\n\s*\(Join-Path \$worktreeRoot ''scripts/publish-sequence-goal-qa-pr\.ps1''\)' -or
        $sequenceGoalWorkflow -notmatch 'Cannot safely remove an unconsumed production snapshot through linked context ancestor' -or
        $sequenceGoalWorkflow -notmatch 'Production context target already exists before snapshot creation' -or
        $sequenceGoalWorkflow -notmatch 'Refusing to remove a production context target not created by this invocation' -or
        $sequenceGoalWorkflow -notmatch 'GitHub Actions command path cannot contain a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Assert-NoLinkedCommandPath' -or
        $sequenceGoalWorkflow -notmatch '\$commandPathRoot = \[System\.IO\.Path\]::GetPathRoot' -or
        $sequenceGoalWorkflow -notmatch 'Automation-test generation cannot publish a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'QA agent changed a path through a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Automation-test generation cannot stage a path through a symbolic link or reparse point' -or
        $sequenceGoalWorkflow -notmatch 'Failed to inspect tracked QA agent changes' -or
        $sequenceGoalWorkflow -notmatch 'Failed to inspect untracked QA agent changes' -or
        $sequenceGoalWorkflow -notmatch 'QA agent changed paths outside its publication allowlist' -or
        $sequenceGoalWorkflow -notmatch '\$_ -cnotin \$coordinatorGoalPaths' -or
        $sequenceGoalWorkflow -notmatch 'Post-build validation detected modified protected Git state component' -or
        $sequenceGoalWorkflow -notmatch 'QA_INPROGRESS_LIST' -or
        $sequenceGoalWorkflow -notmatch 'sequence-goal-qa-handoff-\$\(\$env:GITHUB_RUN_ID\)-\$\(\$env:GITHUB_RUN_ATTEMPT\)' -or
        $sequenceGoalWorkflow -notmatch 'QA publication handoff resolves outside RUNNER_TEMP' -or
        $sequenceGoalWorkflow -notmatch 'QA handoff requires a regular file' -or
        $sequenceGoalWorkflow -notmatch 'QA handoff hash mismatch' -or
        $sequenceGoalWorkflow -notmatch 'persist-credentials: false\s*\r?\n\s*path: trusted-qa-publication' -or
        $sequenceGoalWorkflow -notmatch 'working-directory: trusted-qa-publication' -or
        $sequenceGoalWorkflow -notmatch 'does not publish failure evidence from a checkout that evaluated agent-controlled projects' -or
        $sequenceGoalWorkflow -notmatch 'QA handoff hash mismatch' -or
        $sequenceGoalWorkflow -notmatch 'Production PR metadata exceeds the 100,000-character agent-context limit' -or
        $sequenceGoalWorkflow -notmatch '\[production PR diff truncated to fit the QA agent context limit\]' -or
        $sequenceGoalWorkflow -notmatch '\$contentLimit = \$MaximumCharacters - \$truncationMarker\.Length' -or
        $sequenceGoalWorkflow -notmatch '\$maximumProductionDiffCharacters = 500000 - \$productionContextPrefix\.Length - \$productionContextSuffix\.Length - 1' -or
        $sequenceGoalWorkflow -notmatch '\$productionContextContent\.Length -gt 500000' -or
        $sequenceGoalWorkflow -notmatch 'Set-Content -LiteralPath \$productionContextPath -Encoding UTF8 -NoNewline' -or
        $sequenceGoalWorkflow -notmatch 'Get-BoundedProductionDiff' -or
        $sequenceGoalWorkflow -notmatch '\$process\.StandardOutput\.Read' -or
        $sequenceGoalWorkflow -notmatch '\$process\.Kill\(\$true\)' -or
        ([regex]::Matches($sequenceGoalWorkflow, 'catch \[System\.InvalidOperationException\]')).Count -lt 2 -or
        $sequenceGoalWorkflow -notmatch "(?s)- name: Commit generated automation tests.*?if: env\.QA_HAS_INPROGRESS == 'true' && steps\.automation_agent\.outcome == 'success'" -or
        $sequenceGoalWorkflow -notmatch "steps\.automation_agent\.outcome == 'failure'" -or
        $sequenceGoalWorkflow -notmatch 'Test-Path Env:GH_TOKEN' -or
        $sequenceGoalWorkflow -notmatch 'Unconsumed production PR snapshot still exists after cleanup' -or
        $sequenceGoalWorkflow -notmatch 'Get-ChangedGitTrustComponents' -or
        $sequenceGoalWorkflow -notmatch 'ConvertTo-Json -InputObject \$beforeState\.\$_' -or
        $sequenceGoalWorkflow -notmatch 'ConvertTo-Json -InputObject \$afterState\.\$_' -or
        $sequenceGoalWorkflow -notmatch 'QA agent modified protected Git state component') {
        Add-Failure -Check 'Sequence-goal QA agents do not hand off bounded production context and generated files to a fresh trusted publication checkout.' -Fix 'Run the agent credential-free, create a validated data-only handoff, and publish only from a fresh post-agent checkout.'
    }

    if ($sequenceGoalWorkflow -notmatch '(?s)# Agent Loop Smoke Test.*Maximum agent resources.*Actual agent resources.*## Agent delivery constraints.*## Expected harness') {
        Add-Failure -Check 'Sequence-goal smoke fixture is missing Agent delivery constraints.' -Fix 'Keep the generated smoke diagram aligned with the required sequence-diagram sections.'
    }
    if ($sequenceGoalWorkflow -notmatch '(?s)\$allBudgetEvidencePaths\s*=.*Where-Object\s*\{\s*-not \[string\]::IsNullOrWhiteSpace') {
        Add-Failure -Check 'Sequence-goal resource aggregation does not discard empty evidence paths.' -Fix 'Filter null and empty values before deciding whether resource evidence exists.'
    }
    if ($sequenceGoalWorkflow -notmatch [regex]::Escape('$isSmokeTest = ''${{ inputs.smoke_test }}'' -eq ''true''') -or
        $sequenceGoalWorkflow -notmatch 'if \(\$isSmokeTest\) \{ \$declaredSurfaces = @\(\) \}') {
        Add-Failure -Check 'Sequence-goal smoke completion incorrectly requires an external delivery surface.' -Fix 'Keep smoke mode exempt from associated delivery PR surface matching.'
    }
    if ($sequenceGoalDeliveryResolver -notmatch '(?s)\$smokeDiagram\s*=.*?if \(-not \$SmokeTest -and \(Test-Path -LiteralPath \$smokeDiagram.*?\$unexpectedSmokeDiagrams.*?foreach \(\$diagram in \$diagrams\).*?\$actionableDiagrams \+= \$diagram.*?if \(\$SmokeTest\) \{ continue \}.*?\$surfaceMatch') {
        Add-Failure -Check 'Sequence-goal smoke routing can contact delivery repositories or become stuck after an interrupted run.' -Fix 'Fail closed on leftover fixtures, reject non-smoke actionable goals, and bypass surface and ledger processing during smoke routing.'
    }

    $automationRestoreStepIndex = $sequenceGoalWorkflow.IndexOf('Restore automation dependencies before generation')
    $automationGenerationStepIndex = $sequenceGoalWorkflow.IndexOf('Generate automation tests for implementation')
    if ($sequenceGoalWorkflow -notmatch "name: Install and verify SwiftLint for iOS delivery" -or
        $sequenceGoalWorkflow -notmatch "if: matrix\.surface == 'MobileIos'" -or
        $sequenceGoalWorkflow -notmatch 'https://github\.com/realm/SwiftLint/releases/download/\$SWIFTLINT_VERSION/portable_swiftlint\.zip' -or
        $sequenceGoalWorkflow -notmatch 'd6cb0aa7a2f5f1ef306fc9e37bcb54dc9a26facc8f7784ac0c3dd3eccf5c6ba6' -or
        $sequenceGoalWorkflow -notmatch 'shasum -a 256 --check' -or
        $sequenceGoalWorkflow -notmatch '\$GITHUB_PATH' -or
        $sequenceGoalWorkflow -notmatch 'SWIFTLINT_BIN=' -or
        $sequenceGoalWorkflow -notmatch '0\.65\.0') {
        Add-Failure -Check 'Sequence-goal iOS production delivery can start without the repository-pinned SwiftLint release.' -Fix 'Download, checksum, expose, and verify the SwiftLint 0.65.0 portable release before invoking the iOS production agent.'
    }
    foreach ($iosNamingToken in @(
        'feat/sequence-goal-$($safeRef.ToLowerInvariant())-$refHash',
        'git config core.hooksPath .githooks',
        '& ./.githooks/pre-push',
        'feat(sequence-goal): deliver mobile iOS changes'
    )) {
        if ($sequenceGoalWorkflow -notmatch [regex]::Escape($iosNamingToken)) {
            Add-Failure -Check "Sequence-goal iOS delivery is missing README naming control: $iosNamingToken" -Fix 'Use the Mobile iOS README branch, commit, PR-title, and pre-push-hook conventions.'
        }
    }
    if ($sequenceGoalWorkflow -notmatch [regex]::Escape('sequence-goal/$safeRef-$($env:DELIVERY_SURFACE)-$refHash')) {
        Add-Failure -Check 'Sequence-goal Android/WebUI delivery does not preserve coordinator-branch casing.' -Fix 'Only lowercase the Mobile iOS branch; retain the sanitised coordinator ref for Android and WebUI PR reuse.'
    }
    if ($sequenceGoalWorkflow -notmatch 'supplies UX but has no completed screenshot/render comparison evidence' -or
        $sequenceGoalWorkflow -notmatch 'has no concrete UI/UX acceptance test under VAA\.EAPI\.Automation/' -or
        $sequenceGoalWorkflow -notmatch 'must retain at least an actual render and comparison/diff image' -or
        $sequenceGoalWorkflow -notmatch 'Test-EvidencePathUnderRoot' -or
        $sequenceGoalWorkflow -notmatch 'IsPathRooted\(\$RelativePath\)' -or
        $sequenceGoalWorkflow -notmatch 'StringComparison\]::OrdinalIgnoreCase' -or
        $sequenceGoalWorkflow -notmatch 'rendered UI/UX evidence is missing or outside docs/ and TestResults/' -or
        $sequenceGoalWorkflow -notmatch 'visual comparison result is not Passed' -or
        $sequenceGoalWorkflow -notmatch 'authoritative UX evidence checks' -or
        $sequenceGoalWorkflow -notmatch 'Supplied UX' -or
        $sequenceGoalWorkflow -notmatch 'mp4\|mov\|webm' -or
        $sequenceGoalWorkflow -notmatch 'no completed screen/state mapping evidence' -or
        $sequenceGoalWorkflow -notmatch 'no completed pre-implementation design inspection matrix' -or
        $sequenceGoalWorkflow -notmatch 'none\|not applicable\|n/a' -or
        $sequenceGoalWorkflow -notmatch 'IsNullOrWhiteSpace\(\$visualVerification' -or
        $sequenceGoalWorkflow -notmatch [regex]::Escape('^\s*`?(?:planned|tbd|todo|none|not applicable|n/a)\b') -or
        $sequenceGoalWorkflow -notmatch '<\[\^>\]\+>') {
        Add-Failure -Check 'Sequence-goal completion can accept supplied UX without executable UI tests and retained visual proof.' -Fix 'Require an existing UI/UX acceptance test, allowlisted actual and comparison image evidence, and a Passed comparison result before semantic acceptance succeeds.'
    }
    if (([regex]::Matches($sequenceGoalWorkflow, [regex]::Escape('<[^>]+>'))).Count -lt 4) {
        Add-Failure -Check 'Sequence-goal semantic acceptance rejects arbitrary less-than characters.' -Fix 'Reject placeholder-shaped angle-bracket tokens without rejecting legitimate XML, redirects, or comparison evidence.'
    }
    if ($sequenceGoalWorkflow -notmatch 'invalid Supplied UX evidence' -or
        $sequenceGoalWorkflow -notmatch [regex]::Escape('^(?:none|n/a)$')) {
        Add-Failure -Check 'Sequence-goal semantic acceptance treats Not applicable as absent UX.' -Fix 'Reject Not applicable explicitly and reserve None/n/a for the no-UX detector.'
    }
    if (([regex]::Matches($sequenceGoalWorkflow, 'check-mermaid-diagrams\.ps1 -BaseRef origin/main')).Count -lt 2) {
        Add-Failure -Check 'Sequence-goal workflow does not render changed Mermaid diagrams during initial and remediation verification.' -Fix 'Run scripts/check-mermaid-diagrams.ps1 -BaseRef origin/main in both verification paths.'
    }
    foreach ($associatedPrCostToken in @('## Agent resources and cost', 'Actual production-agent usage', 'productionBySurface')) {
        if ($sequenceGoalWorkflow -notmatch [regex]::Escape($associatedPrCostToken)) {
            Add-Failure -Check "Sequence-goal workflow does not expose associated-PR cost token: $associatedPrCostToken" -Fix 'Record each remote production agent cost in its associated PR and retain a per-surface breakdown for the coordinator PR.'
        }
    }
    if ($sequenceGoalWorkflow -notmatch 'sequence-goal-open-points human-comment' -or
        $sequenceGoalWorkflow -notmatch 'awaiting answers to open questions raised on the delivery PR' -or
        $sequenceGoalWorkflow -match 'must close Open points to finalize before completion') {
        Add-Failure -Check 'Sequence-goal delivery does not preserve verified goals with open questions for human input.' -Fix 'Keep verified goals InProgress, open the delivery PR, and mark its open-point question for the human-comment loop.'
    }
    if ($sequenceGoalWorkflow -match 'if \(\$pendingQuestions\.Count -eq 0\) \{\s*\$locks') {
        Add-Failure -Check 'Sequence-goal delivery retains run locks while awaiting open-point answers.' -Fix 'Release verified run locks before waiting for human input so expected InProgress goals do not become stale-lock failures.'
    }
    if (([regex]::Matches($sequenceGoalWorkflow, 'format-generated-csharp\.ps1')).Count -lt 2) {
        Add-Failure -Check 'Sequence-goal workflow does not deterministically format generated C# before initial and remediation verification.' -Fix 'Run scripts/format-generated-csharp.ps1 after generation and during remediation.'
    }
    if (([regex]::Matches($sequenceGoalWorkflow, "mode\s*=\s*'CoveredByFullScope'")).Count -lt 1 -or
        ([regex]::Matches($sequenceGoalWorkflow, 'requires at least one resolved solution or project scope')).Count -lt 1) {
        Add-Failure -Check 'Sequence-goal Full verification does not safely handle mixed build and repository fallback scopes.' -Fix 'Require a project or solution scope for Full verification, then record repository fallback scopes as covered.'
    }
    if ($sequenceGoalWorkflow -notmatch 'OPENAI_API_KEY') {
        Add-Failure -Check 'Sequence-goal workflow does not pass the OpenAI API key to the code-generation agent.' -Fix 'Set OPENAI_API_KEY from secrets.OPENAI_API_KEY in .github/workflows/ai-delivery-sequence-goal-loop.yml.'
    }

    if ($sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_AGENT_MODEL') {
        Add-Failure -Check 'Sequence-goal workflow does not define the code-generation agent model.' -Fix 'Set SEQUENCE_GOAL_AGENT_MODEL with a current full-model default and pass it to scripts/run-sequence-goal-agent.ps1.'
    }
    if (@([regex]::Matches($sequenceGoalWorkflow, "SEQUENCE_GOAL_AGENT_MODEL:\s*\$\{\{ vars\.SEQUENCE_GOAL_AGENT_MODEL \|\| 'gpt-6-sol' \}\}")).Count -lt 1) {
        Add-Failure -Check 'Sequence-goal generation does not use the configured GitHub model variable.' -Fix 'Use vars.SEQUENCE_GOAL_AGENT_MODEL with the enrolled gpt-6-sol fallback in the production agent phase.'
    }

    if ($sequenceGoalWorkflow -notmatch 'actions/setup-node@v4' -or $sequenceGoalWorkflow -notmatch 'npm install -g @openai/codex' -or $sequenceGoalWorkflow -notmatch 'codex --version') {
        Add-Failure -Check 'Sequence-goal workflow does not install the Codex CLI before agent execution.' -Fix 'Use actions/setup-node, npm install -g @openai/codex, and codex --version before running scripts/run-sequence-goal-agent.ps1.'
    }

    if ($sequenceGoalWorkflow -notmatch '(?m)^  resolve-delivery-agents:' -or
        $sequenceGoalWorkflow -notmatch "'Backend'\s+\{\s*@\('dev-jim-api-ai', 'qa-rob-api-ai'\)" -or
        $sequenceGoalWorkflow -notmatch "'MobileIos'\s+\{\s*@\('dev-jim-ios-ai', 'qa-rob-ios-ai'\)" -or
        $sequenceGoalWorkflow -notmatch "'MobileAndroid'\s+\{\s*@\('dev-mike-android-ai', 'qa-rob-android-ai'\)" -or
        $sequenceGoalWorkflow -notmatch "'WebUi'\s+\{\s*@\('dev-mike-web-ai', 'qa-rob-web-ai'\)") {
        Add-Failure -Check 'Sequence-goal workflow AI stage display names have drifted.' -Fix 'Restore the enrolled dev-jim API/iOS, dev-mike Android/WebUI, and qa-rob-*-ai agent names.'
    }
    if ($sequenceGoalWorkflow -notmatch '(?ms)^  complete-goal:.*?^    name:\s*sl-seb-ai\s*$') {
        Add-Failure -Check 'Sequence-goal coordinator agent display name has drifted.' -Fix 'Set the complete-goal job display name to sl-seb-ai.'
    }
    $completionJob = [regex]::Match($sequenceGoalWorkflow, '(?ms)^  complete-goal:\s*$\n(?<body>.*?)(?=^  [A-Za-z0-9_-]+:\s*$|\z)')
    if (-not $completionJob.Success -or
        $completionJob.Groups['body'].Value -notmatch "needs\.run-sequence-loop\.result == 'success' && needs\.run-sequence-loop\.outputs\.has_inprogress == 'true'") {
        Add-Failure -Check 'Sequence-goal completion runs for non-goal workflow maintenance pushes.' -Fix 'Require an InProgress goal before restoring durable evidence and completing the coordinator job.'
    }
    $productionJob = [regex]::Match($sequenceGoalWorkflow, '(?ms)^  deliver-cross-repository-production:\s*$\n(?<body>.*?)(?=^  [A-Za-z0-9_-]+:\s*$|\z)')
    $automationJob = [regex]::Match($sequenceGoalWorkflow, '(?ms)^  deliver-automation-tests:\s*$\n(?<body>.*?)(?=^  [A-Za-z0-9_-]+:\s*$|\z)')
    $deliveryActorGuard = "(?m)^    if:.*github\.event_name == 'workflow_dispatch'.*github\.actor != 'github-actions\[bot\]'.*github\.actor != vars\.SEQUENCE_GOAL_GITHUB_APP_SLUG.*github\.actor != format\('\{0\}\[bot\]', vars\.SEQUENCE_GOAL_GITHUB_APP_SLUG\)"
    if (-not $productionJob.Success -or $productionJob.Groups['body'].Value -notmatch $deliveryActorGuard -or
        -not $automationJob.Success -or
        $automationJob.Groups['body'].Value -notmatch 'needs: \[resolve-delivery-agents, validate-continuation\]' -or
        $sequenceGoalDeliveryResolver -notmatch 'has no authenticated production PR checkpoint' -or
        $sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_GITHUB_APP_SLUG is required when sequence-goal GitHub App credentials are configured') {
        Add-Failure -Check 'Sequence-goal production or QA delivery can restart from an automation-authored coordinator push.' -Fix 'Guard both delivery jobs against GitHub Actions and configured GitHub App actors while retaining explicit workflow_dispatch recovery.'
    }
    if ($sequenceGoalWorkflow -notmatch 'VAAD1ST/vaa-mobile-ios' -or
        $sequenceGoalWorkflow -notmatch 'VAAD1ST/vaa-mobile-android' -or
        $sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_WEBUI_REPOSITORY' -or
        $sequenceGoalWorkflow -notmatch '(?m)^  deliver-cross-repository-production:' -or
        $sequenceGoalWorkflow -notmatch 'TargetRepositoryRoot' -or
        $sequenceGoalWorkflow -notmatch 'Associated delivery pull requests' -or
        $sequenceGoalWorkflow -notmatch 'Register coordinator PR in goal documents' -or
        (Get-FileText -RepoRelativePath 'scripts/open-sequence-goal-coordinator-pr.ps1') -notmatch 'Failed to inspect existing coordinator PRs' -or
        $sequenceGoalWorkflow -notmatch 'coordinatorRow' -or
        $sequenceGoalWorkflow -notmatch 'awaiting all associated delivery PRs to merge') {
        Add-Failure -Check 'Sequence-goal workflow cross-repository production delivery or all-PR completion gating has drifted.' -Fix 'Restore the iOS/Android/WebUI repository matrix, remote agent execution, associated PR ledger, and merged-PR completion gate.'
    }

    if ($sequenceGoalWorkflow -notmatch 'Test-ProductionReadiness' -or
        $sequenceGoalWorkflow -notmatch '\$resumeProduction' -or
        $sequenceGoalQaWorkflow -notmatch 'Test-ProductionReadiness') {
        Add-Failure -Check 'Sequence-goal production readiness or resume gate is missing.' -Fix 'Resume incomplete production and require current-head readiness before QA.'
    }

    if ($sequenceGoalQaWorkflow -notmatch 'needs:\s*\[resolve-delivery-agents, validate-continuation\]' -or
        $sequenceGoalWorkflow -notmatch 'has_inprogress:\s*\$\{\{ steps\.route\.outputs\.has_inprogress \}\}' -or
        $sequenceGoalWorkflow -notmatch 'inprogress_list:\s*\$\{\{ steps\.route\.outputs\.inprogress_list \}\}' -or
        $sequenceGoalWorkflow -notmatch "needs\.resolve-delivery-agents\.outputs\.qa_count != '0'" -or
        $sequenceGoalWorkflow -notmatch '\$inProgressDiagrams = @\(\)' -or
        $sequenceGoalWorkflow -notmatch '-eq ''InProgress''\) \{ \$inProgressDiagrams \+= \$diagram \}' -or
        $sequenceGoalWorkflow -notmatch 'Reusing verified production head' -or
        $sequenceGoalWorkflow -notmatch 'Generate automation tests for implementation' -or
        $sequenceGoalWorkflow -notmatch 'publish-sequence-goal-qa-pr\.ps1' -or
        $sequenceGoalWorkflow -notmatch 'ProductionHeadSha' -or
        $sequenceGoalWorkflow -notmatch 'DeliveryPhase AutomationTests' -or
        $sequenceGoalWorkflow -notmatch 'git diff --name-only --no-renames origin/main\.\.\.HEAD' -or
        $sequenceGoalWorkflow -notmatch 'Existing automation coverage is already committed on this branch' -or
        $sequenceGoalWorkflow -notmatch 'actions/setup-dotnet@v4' -or
        $sequenceGoalWorkflow -notmatch 'Cache automation NuGet packages' -or
        ([regex]::Matches($sequenceGoalWorkflow, 'dotnet restore VAA\.EAPI\.Automation/VAA\.EAPI\.Automation\.sln')).Count -lt 2 -or
        ([regex]::Matches($sequenceGoalWorkflow, "steps\.automation_restore\.outcome == 'success'")).Count -lt 3 -or
        $automationRestoreStepIndex -lt 0 -or
        $automationGenerationStepIndex -lt 0 -or
        $automationRestoreStepIndex -gt $automationGenerationStepIndex -or
        $sequenceGoalWorkflow -notmatch 'Automation generation or verification failed' -or
        $sequenceGoalWorkflow -notmatch 'dotnet build VAA\.EAPI\.Automation/VAA\.EAPI\.Automation\.sln -c Release --no-restore') {
        Add-Failure -Check 'Sequence-goal workflow does not deliver or validate automation coverage in a dependent job after product implementation.' -Fix 'Run a deliver-automation-tests job after the production job, accept substantive output, validated existing-test attestation, or a no-change result, and build the automation solution before promotion verification.'
    }

    if ($sequenceGoalWorkflow -notmatch 'ref:\s*\$\{\{ github\.ref_name \}\}' -or
        $sequenceGoalWorkflow -notmatch 'git pull --rebase origin \$env:GITHUB_REF_NAME' -or
        $sequenceGoalWorkflow -notmatch 'manifest\.json' -or
        $sequenceGoalWorkflow -notmatch 'Get-FileHash -LiteralPath \$sourcePath') {
        Add-Failure -Check 'Sequence-goal automation handoff is not resilient to stale branch state or handoff tampering.' -Fix 'Check out the current branch tip, validate the handoff manifest and hashes, and rebase before coordinator publication.'
    }

    if (([regex]::Matches($sequenceGoalWorkflow, 'fetch-depth:\s*1')).Count -lt 2 -or
        ([regex]::Matches($sequenceGoalWorkflow, 'git fetch --no-tags --unshallow origin \+refs/heads/main:refs/remotes/origin/main \$branchRef')).Count -lt 2) {
        Add-Failure -Check 'Sequence-goal Windows jobs fetch all remote refs and remain vulnerable to case-colliding branch names.' -Fix 'Use shallow checkout followed by a selective unshallow fetch of main and the active delivery branch.'
    }

    if ($sequenceGoalWorkflow -notmatch 'actions/create-github-app-token@v2' -or $sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_GITHUB_APP_ID' -or $sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_GITHUB_APP_PRIVATE_KEY' -or $sequenceGoalWorkflow -notmatch 'steps\.app-token\.outputs\.token\s*\|\|\s*github\.token') {
        Add-Failure -Check 'Sequence-goal workflow does not create and update delivery PRs with optional GitHub App authentication.' -Fix 'Use the sequence-goal GitHub App token when configured and retain an explicit GITHUB_TOKEN fallback so workflow checks do not fail before App rollout.'
    }

    if ($sequenceGoalWorkflow -notmatch "id:\s*app-credentials" -or $sequenceGoalWorkflow -notmatch "if:\s*steps\.app-credentials\.outputs\.configured == 'true'") {
        Add-Failure -Check 'Sequence-goal workflow attempts GitHub App authentication when its secrets are absent.' -Fix 'Conditionally create the App token only when both App credential values are configured.'
    }

    if ($sequenceGoalWorkflow -notmatch 'SEQUENCE_GOAL_GITHUB_APP_SLUG') {
        Add-Failure -Check 'Sequence-goal workflow does not suppress recursive runs from the GitHub App.' -Fix 'Exclude the configured SEQUENCE_GOAL_GITHUB_APP_SLUG actor from the push-triggered delivery job.'
    }

    if ($sequenceGoalWorkflow -notmatch 'MaximumAgentResources' -or
        $sequenceGoalWorkflow -notmatch 'ActualAgentResources' -or
        $sequenceGoalWorkflow -notmatch 'ProductionAgentResources' -or
        $sequenceGoalWorkflow -notmatch 'AutomationAgentResources' -or
        $sequenceGoalWorkflow -notmatch 'get-agent-resource-summary\.ps1') {
        Add-Failure -Check 'Sequence-goal completion does not record total and per-generation-stage token/USD resources.' -Fix 'Read both retained generation-phase budget evidence files and pass their breakdown plus combined labels to promote-sequence-goal.ps1.'
    }
    if ($sequenceGoalWorkflow -notmatch '(?s)name:\s*Upload remote production-agent resource evidence\s+if:\s*always\(\)' -or
        $sequenceGoalWorkflow -notmatch '(?s)name:\s*Upload automation-agent resource evidence\s+if:\s*always\(\)' -or
        $sequenceGoalWorkflow -notmatch 'retain-agent-resource-evidence:' -or
        $sequenceGoalWorkflow -notmatch 'export-sequence-goal-resources' -or
        $sequenceGoalWorkflow -notmatch '\$\{\{ github\.run_id \}\}-\$\{\{ github\.run_attempt \}\}') {
        Add-Failure -Check 'Sequence-goal failed delivery attempts can lose or overwrite measured agent cost evidence.' -Fix 'Upload production and automation resource evidence with always() semantics and append sanitized run/attempt-specific budget decisions to the coordinator resource ledger.'
    }
    if ($sequenceGoalWorkflow -notmatch '## Agent resources and cost' -or
        $sequenceGoalWorkflow -notmatch 'resourceUsage\?\.maximumLabel' -or
        $sequenceGoalWorkflow -notmatch 'resourceUsage\?\.actualLabel' -or
        $sequenceGoalWorkflow -notmatch 'resourceUsage\?\.production\?\.actualLabel' -or
        $sequenceGoalWorkflow -notmatch 'resourceUsage\?\.automation\?\.actualLabel') {
        Add-Failure -Check 'Sequence-goal delivery PR descriptions omit total or per-generation-stage agent resources.' -Fix 'Render production, automation, and combined resourceUsage labels in created and refreshed delivery PR bodies.'
    }
}
if ($null -ne $associatedPrMonitorWorkflow) {
    if ($associatedPrMonitorWorkflow -match $draftPrExclusionPattern) {
        Add-Failure -Check 'Associated sequence-goal PR monitor excludes draft PRs from feedback handling.' -Fix 'Process every trusted open associated PR regardless of draft state.'
    }
    if ($associatedPrMonitorWorkflow -notmatch "cron: '\*/15 \* \* \* \*'" -or
        $associatedPrMonitorWorkflow -notmatch 'merged_at' -or
        $associatedPrMonitorWorkflow -notmatch 'createWorkflowDispatch' -or
        $associatedPrMonitorWorkflow -notmatch 'ai-delivery-sequence-goal-loop\.yml' -or
        $associatedPrMonitorWorkflow -notmatch 'Skipping unavailable diagram' -or
        $associatedPrMonitorWorkflow -notmatch 'catch \(error\)') {
        Add-Failure -Check 'Associated sequence-goal PR monitor no longer polls and resumes fully merged cross-repository goals.' -Fix 'Poll associated PR merge state and dispatch ai-delivery-sequence-goal-loop.yml only after every recorded PR is merged.'
    }
    if ($associatedPrMonitorWorkflow -notmatch 'Associated feedback dispatch failed for \$\{item\.repository\}#\$\{item\.number\}; continuing with the remaining goals' -or
        $associatedPrMonitorWorkflow -notmatch 'Coordinator dispatch failed for \$\{candidate\.branch\}; continuing with the remaining goals') {
        Add-Failure -Check 'Associated-PR monitor dispatch failures can abort the remaining goal scan.' -Fix 'Catch feedback and coordinator workflow-dispatch failures per goal, warn, and continue scanning other coordinator branches.'
    }
    if ($associatedPrMonitorWorkflow -notmatch 'ai-delivery-sequence-goal-associated-pr-feedback\.yml' -or
        $associatedPrMonitorWorkflow -notmatch 'github\.paginate\.iterator\(github\.rest\.repos\.listBranches' -or
        $associatedPrMonitorWorkflow -notmatch 'registered.length !== 1' -or
        $associatedPrMonitorWorkflow -notmatch 'listComments' -or
        $associatedPrMonitorWorkflow -notmatch 'listReviewComments' -or
        $associatedPrMonitorWorkflow -notmatch 'listReviews' -or
        $associatedPrMonitorWorkflow -notmatch 'checks\.listForRef' -or
        $associatedPrMonitorWorkflow -notmatch 'getCombinedStatusForRef' -or
        $associatedPrMonitorWorkflow -notmatch 'latestStatusesByContext' -or
        $associatedPrMonitorWorkflow -notmatch 'latestStatuses\.some' -or
        $associatedPrMonitorWorkflow -notmatch 'ref: pr\.head\.sha' -or
        $associatedPrMonitorWorkflow -notmatch 'filter: "latest"' -or
        $associatedPrMonitorWorkflow -notmatch 'failingConclusions' -or
        $associatedPrMonitorWorkflow -notmatch 'check-run' -or
        $associatedPrMonitorWorkflow -notmatch 'commit-status' -or
        $associatedPrMonitorWorkflow -notmatch 'mergeable_state === "dirty"' -or
        $associatedPrMonitorWorkflow -notmatch 'merge-conflict' -or
        $associatedPrMonitorWorkflow -notmatch '\[\.\.\.issueComments, \.\.\.reviewComments\]' -or
        $associatedPrMonitorWorkflow -notmatch 'filter\(comment => appAuthors\.has' -or
        $associatedPrMonitorWorkflow -notmatch 'hasCompleteInferentialContract' -or
        $associatedPrMonitorWorkflow -notmatch 'coordinator_diagram: item\.diagram' -or
        $associatedPrMonitorWorkflow -notmatch 'Associated sequence-goal delivery for `\{1,2\}VAAD1ST\\/eapi@' -or
        $associatedPrMonitorWorkflow -notmatch 'Ignoring untrusted associated PR' -or
        $associatedPrMonitorWorkflow -notmatch 'sequence-goal-associated-feedback:') {
        Add-Failure -Check 'Associated sequence-goal PR monitor no longer discovers or dispatches unhandled surface feedback and failing checks.' -Fix 'Poll conversation comments, review comments, review summaries, check runs, and commit statuses; deduplicate by durable response marker; dispatch the associated feedback worker.'
    }
    if ($associatedPrMonitorWorkflow -notmatch 'Backend: \{ repository: process\.env\.SEQUENCE_GOAL_COORDINATOR_REPOSITORY, base: "main" \}' -or
        $associatedPrMonitorWorkflow -notmatch 'MobileIos: \{ repository: "VAAD1ST/vaa-mobile-ios"' -or
        $associatedPrMonitorWorkflow -notmatch 'MobileAndroid: \{ repository: "VAAD1ST/vaa-mobile-android"' -or
        $associatedPrMonitorWorkflow -notmatch 'expectedPhase === "QA" \? "main"' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-actions:\s*write' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-checks:\s*read' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-contents:\s*read' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-issues:\s*write' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-pull-requests:\s*write' -or
        $associatedPrMonitorWorkflow -notmatch 'permission-statuses:\s*read') {
        Add-Failure -Check 'Associated-PR monitor target defaults or App permissions do not match its API operations.' -Fix 'Default Backend production to main and request the permissions required by monitoring and dispatch.'
    }
}
if ($null -ne $associatedPrFeedbackWorkflow) {
    if ($associatedPrFeedbackWorkflow -match $draftPrExclusionPattern) {
        Add-Failure -Check 'Associated sequence-goal PR feedback worker excludes draft PRs.' -Fix 'Accept every trusted open associated PR regardless of draft state.'
    }
    if ($associatedPrFeedbackWorkflow -notmatch '(?s)surface:\s+description: Delivery surface\.\s+required: true\s+type: string' -or
        $associatedPrFeedbackWorkflow -notmatch "inputs\.surface == 'MobileIos' && 'macos-26'" -or
        $associatedPrFeedbackWorkflow -notmatch 'coordinatorIdentity = new RegExp' -or
        $associatedPrFeedbackWorkflow -notmatch 'COORDINATOR_DIAGRAM' -or
        $associatedPrFeedbackWorkflow -notmatch 'ledgerBound' -or
        $associatedPrFeedbackWorkflow -notmatch 'expectedRepository' -or
        $associatedPrFeedbackWorkflow -notmatch 'expectedBase' -or
        $associatedPrFeedbackWorkflow -notmatch 'sequence-goal-associated-feedback:' -or
        $associatedPrFeedbackWorkflow -notmatch 'decision \(valid, invalid, or not-applicable\)' -or
        $associatedPrFeedbackWorkflow -notmatch 'issues\.createComment' -or
        $associatedPrFeedbackWorkflow -notmatch 'pulls\.createReplyForReviewComment' -or
        $associatedPrFeedbackWorkflow -notmatch 'buildReviewThreadLookup' -or
        ($associatedPrFeedbackWorkflow -notmatch 'getRootCommentId' -and $associatedPrFeedbackWorkflow -notmatch 'in_reply_to_id \|\| comment\.id') -or
        $associatedPrFeedbackWorkflow -notmatch 'comments\(first: 1\)' -or
        $associatedPrFeedbackWorkflow -notmatch 'threads\.nodes \|\| \[\]' -or
        $associatedPrFeedbackWorkflow -notmatch 'thread\.comments\?\.nodes \|\| \[\]' -or
        $associatedPrFeedbackWorkflow -notmatch 'reviewThreadLookup\.get' -or
        $associatedPrFeedbackWorkflow -notmatch 'review thread could not be found for resolution' -or
        $associatedPrFeedbackWorkflow -notmatch 'resolveReviewThread' -or
        $associatedPrFeedbackWorkflow -notmatch 'id:\s*commit' -or
        $associatedPrFeedbackWorkflow -notmatch '\$diffExitCode = \$LASTEXITCODE' -or
        $associatedPrFeedbackWorkflow -notmatch '\$diffExitCode -eq 1' -or
        $associatedPrFeedbackWorkflow -notmatch 'changes_pushed=true' -or
        $associatedPrFeedbackWorkflow -notmatch 'changes_pushed=false' -or
        $associatedPrFeedbackWorkflow -notmatch "steps\.validate\.outputs\.requires_target_verification == 'true'" -or
        $associatedPrFeedbackWorkflow -notmatch 'Publish inferential review and start target PR verification' -or
        $associatedPrFeedbackWorkflow -notmatch 'Published the complete inferential-review contract and started target CI' -or
        $associatedPrFeedbackWorkflow -notmatch "target_verification\.outcome == 'success'.*target_verification\.outcome == 'skipped'" -or
        $associatedPrFeedbackWorkflow -notmatch 'filter\(comment => appAuthors\.has' -or
        $associatedPrFeedbackWorkflow -notmatch 'checks\.listForRef' -or
        $associatedPrFeedbackWorkflow -notmatch 'getCombinedStatusForRef' -or
        $associatedPrFeedbackWorkflow -notmatch 'latestStatusesByContext' -or
        $associatedPrFeedbackWorkflow -notmatch 'for \(const status of latestStatuses\)' -or
        $associatedPrFeedbackWorkflow -notmatch 'checks\.listAnnotations' -or
        $associatedPrFeedbackWorkflow -notmatch 'check_run_id: run\.id, per_page: 50' -or
        $associatedPrFeedbackWorkflow -notmatch 'Additional check annotations may have been omitted' -or
        $associatedPrFeedbackWorkflow -notmatch 'actions/jobs/\{job_id\}/logs' -or
        $associatedPrFeedbackWorkflow -notmatch 'run\.details_url' -or
        $associatedPrFeedbackWorkflow -notmatch '/\\/job\\/\(\\d\+\)' -or
        $associatedPrFeedbackWorkflow -notmatch 'execFileSync\("unzip"' -or
        $associatedPrFeedbackWorkflow -notmatch 'stdio: "ignore"' -or
        $associatedPrFeedbackWorkflow -notmatch 'fs\.readSync' -or
        $associatedPrFeedbackWorkflow -notmatch 'Failed job log tail' -or
        $associatedPrFeedbackWorkflow -notmatch 'steps\.pr\.outputs\.head_sha' -or
        $associatedPrFeedbackWorkflow -notmatch '\.slice\(-16000\)' -or
        $associatedPrFeedbackWorkflow -notmatch 'authorType: "ci"' -or
        $associatedPrFeedbackWorkflow -notmatch 'authorType: "repository-state"' -or
        $associatedPrFeedbackWorkflow -notmatch 'merge_conflict' -or
        $associatedPrFeedbackWorkflow -notmatch 'without rebasing or force-pushing' -or
        $associatedPrFeedbackWorkflow -notmatch 'failing CI checks' -or
        $associatedPrFeedbackWorkflow -notmatch 'Install and verify SwiftLint for iOS remediation' -or
        $associatedPrFeedbackWorkflow -notmatch "inputs\.surface == 'MobileIos'" -or
        $associatedPrFeedbackWorkflow -notmatch "SWIFTLINT_VERSION: '0\.65\.0'" -or
        $associatedPrFeedbackWorkflow -notmatch 'd6cb0aa7a2f5f1ef306fc9e37bcb54dc9a26facc8f7784ac0c3dd3eccf5c6ba6' -or
        $associatedPrFeedbackWorkflow -notmatch 'portable_swiftlint\.zip' -or
        $associatedPrFeedbackWorkflow -notmatch 'shasum -a 256 --check' -or
        $associatedPrFeedbackWorkflow -notmatch 'GITHUB_PATH' -or
        $associatedPrFeedbackWorkflow -notmatch 'inferentialReview: null' -or
        $associatedPrFeedbackWorkflow -notmatch 'kind: "inferential-review"' -or
        $associatedPrFeedbackWorkflow -notmatch 'Agent omitted the required Mobile iOS inferential-review metadata update' -or
        $associatedPrFeedbackWorkflow -notmatch 'surface\.replace\(/\^QA-/' -or
        $associatedPrFeedbackWorkflow -notmatch 'credentialFailureIds: immutableCredentialFailures' -or
        $associatedPrFeedbackWorkflow -notmatch 'immutableCredentialFailures = "\$\{\{ inputs\.surface \}\}" === "MobileIos" \? items' -or
        $associatedPrFeedbackWorkflow -notmatch '\["check-run", "commit-status"\]\.includes\(item\.kind\)' -or
        $associatedPrFeedbackWorkflow -notmatch '\$_.credentialFailureId -notin \$credentialFailureIds' -or
        $associatedPrFeedbackWorkflow -notmatch 'EXPECTED_HEAD_SHA:.*steps\.commit\.outputs\.expected_head_sha' -or
        $associatedPrFeedbackWorkflow -notmatch 'normalize-sequence-goal-inferential-review\.ps1 -EvidencePath \$env:EVIDENCE_PATH' -or
        $associatedPrFeedbackWorkflow -notmatch '\$passesValue -isnot \[System\.Array\]' -or
        $associatedPrFeedbackWorkflow -notmatch '\$actualPasses\.Count -ne \$requiredPasses\.Count' -or
        $associatedPrFeedbackWorkflow -notmatch '\$uniquePasses\.Count -ne \$requiredPasses\.Count' -or
        $associatedPrFeedbackWorkflow -notmatch '\$actualPasses -join "`0"' -or
        $associatedPrFeedbackWorkflow -notmatch 'Agent recorded an invalid inferential-review metadata update' -or
        $associatedPrFeedbackWorkflow -notmatch 'github\.rest\.pulls\.update' -or
        $associatedPrFeedbackWorkflow -notmatch 'Published the complete inferential-review contract' -or
        $associatedPrFeedbackWorkflow -notmatch 'git diff --check' -or
        $associatedPrFeedbackWorkflow -notmatch 'check-agent-resource-budget\.ps1') {
        Add-Failure -Check 'Associated surface feedback worker no longer enforces trusted routing, idempotent replies, remediation decisions, verification, or resource controls.' -Fix 'Restore the ledger-routed macOS/Linux feedback worker contract and its fail-closed controls.'
    }
    if ($null -eq $inferentialReviewNormalizer -or
        $inferentialReviewNormalizer -notmatch 'passDetails' -or
        $inferentialReviewNormalizer -notmatch "'findings-addressed'" -or
        $inferentialReviewNormalizer -notmatch 'ConvertTo-Json -Depth 20') {
        Add-Failure -Check 'Associated-PR feedback no longer normalizes detailed inferential-review evidence.' -Fix 'Restore deterministic normalization of detailed pass objects before the strict workflow validator runs.'
    }
    if ($associatedPrFeedbackWorkflow -notmatch 'userAuthoredThreadRoots' -or
        $associatedPrFeedbackWorkflow -notmatch 'containsUserComment' -or
        $associatedPrFeedbackWorkflow -notmatch '(?s)if \(reviewThread\.containsUserComment\).*?continue;.*?resolveReviewThread') {
        Add-Failure -Check 'Associated-PR feedback can resolve a review thread containing user feedback.' -Fix 'Identify user-authored review-thread roots and leave every matching thread unresolved after posting the response.'
    }
    if ($associatedPrFeedbackWorkflow -notmatch '(?s)const isUserAuthored = login => \{\s+const normalizedLogin = String\(login \|\| ""\)\.toLowerCase\(\);\s+return !aiAuthors\.has\(normalizedLogin\) && !normalizedLogin\.endsWith\("\[bot\]"\);') {
        Add-Failure -Check 'Associated-PR feedback classifies review authors case-sensitively.' -Fix 'Normalize author logins before checking AI and bot identities so user-involved threads cannot be resolved incorrectly.'
    }
    if ($associatedPrFeedbackWorkflow -match 'job_id:\s*run\.id') {
        Add-Failure -Check 'Associated-PR feedback incorrectly uses a check-run ID as an Actions job ID.' -Fix 'Derive the Actions job ID from the check run details_url before downloading and extracting its log archive.'
    }
    if ($associatedPrFeedbackWorkflow -match '(?ms)^      - name: Handle associated PR feedback(?:(?!^      - name:).)*GH_TOKEN:') {
        Add-Failure -Check 'Associated-PR feedback exposes its write-capable App token to the remediation agent.' -Fix 'Keep the App token confined to controlled checkout, GitHub API, push, and reply steps; pass only captured evidence to the agent process.'
    }
    if ($associatedPrFeedbackWorkflow -notmatch '\$unexpectedAgentLoopChanges' -or
        $associatedPrFeedbackWorkflow -notmatch '\$_ -cne \$allowedEvidencePath' -or
        $associatedPrFeedbackWorkflow -notmatch '\$checkExitCode -in @\(1, 2, 8\)' -or
        $associatedPrFeedbackWorkflow -match 'const verificationNote = targetVerification') {
        Add-Failure -Check 'Associated-PR feedback does not isolate lifecycle evidence or normalize deferred check states safely.' -Fix 'Permit only the current evidence file under .codex/agent-loop, handle pending and empty check exit codes, and report verification failure only from its reachable enforcement step.'
    }
    if ($associatedPrFeedbackWorkflow -notmatch 'deferred-target-pr' -or
        $associatedPrFeedbackWorkflow -notmatch 'verification entry''s `reason` field' -or
        $associatedPrFeedbackWorkflow -notmatch 'deferred-target-pr with a non-empty reason' -or
        $associatedPrFeedbackWorkflow -notmatch 'Wait for target PR verification' -or
        $associatedPrFeedbackWorkflow -notmatch 'gh pr checks' -or
        $associatedPrFeedbackWorkflow -notmatch '\$checkExitCode = \$LASTEXITCODE' -or
        $associatedPrFeedbackWorkflow -notmatch '\$checksNotRegistered' -or
        $associatedPrFeedbackWorkflow -notmatch 'no checks reported' -or
        $associatedPrFeedbackWorkflow -notmatch '\$checkJson = ''\[\]''' -or
        $associatedPrFeedbackWorkflow -notmatch 'Unable to query target PR checks' -or
        $associatedPrFeedbackWorkflow -notmatch 'registrationDeadline' -or
        $associatedPrFeedbackWorkflow -notmatch '\[DateTime\]::UtcNow -ge \$registrationDeadline' -or
        $associatedPrFeedbackWorkflow -notmatch 'target verification has no runnable check' -or
        $associatedPrFeedbackWorkflow -notmatch 'continue-on-error: true' -or
        $associatedPrFeedbackWorkflow -notmatch "steps\.target_verification\.outcome == 'failure'" -or
        $associatedPrFeedbackWorkflow -notmatch 'Feedback replies were withheld' -or
        $associatedPrFeedbackWorkflow -notmatch 'verification-blocked:' -or
        $associatedPrFeedbackWorkflow -notmatch 'durable verification-blocked evidence was posted' -or
        $associatedPrFeedbackWorkflow -notmatch 'headRefOid' -or
        $associatedPrFeedbackWorkflow -notmatch '\$evidence\.headSha = \$verifiedHeadSha' -or
        $associatedPrFeedbackWorkflow -notmatch 'targetPrChecks' -or
        $associatedPrFeedbackWorkflow -notmatch 'Agent resources:' -or
        $associatedPrFeedbackWorkflow -notmatch '\.toFixed\(2\)' -or
        $associatedPrFeedbackWorkflow -match '\.toFixed\([3-9]\)' -or
        $associatedPrFeedbackWorkflow -match 'Number\.EPSILON' -or
        $associatedPrFeedbackWorkflow -notmatch "ToString\('0\.00', \[System\.Globalization\.CultureInfo\]::InvariantCulture\)" -or
        $associatedPrFeedbackWorkflow -notmatch 'currencySymbol' -or
        $associatedPrFeedbackWorkflow -notmatch 'GITHUB_STEP_SUMMARY') {
        Add-Failure -Check 'Associated surface feedback cannot delegate credential-bound verification safely or expose run cost.' -Fix 'Allow only credential-bound checks to defer to the target PR, wait for those checks to pass before replying, retain their evidence, and publish token/cost usage.'
    }
}

foreach ($costReportingScript in @('scripts/check-agent-resource-budget.ps1', 'scripts/check-agent-resource-trend.ps1', 'scripts/get-agent-resource-summary.ps1')) {
    $costReportingText = Get-FileText -RepoRelativePath $costReportingScript
    if ($null -ne $costReportingText -and
        ($costReportingText -notmatch 'Format-UsdForReport' -or
         $costReportingText -notmatch 'ToString\(''0\.00'', \$culture\)')) {
        Add-Failure -Check "$costReportingScript does not format user-facing USD costs to exactly two decimal places." -Fix 'Use the invariant two-decimal Format-UsdForReport helper for every human-readable cost while retaining precise values in JSON evidence.'
    }
}
foreach ($deliveryMetadataToken in @('## Delivered outcome', '## Delivered files', 'Delivery ownership', 'goalTitle', 'titlePrefix')) {
    if ($sequenceGoalWorkflow -notmatch [regex]::Escape($deliveryMetadataToken)) {
        Add-Failure -Check "Sequence-goal production PR metadata is missing goal-specific token: $deliveryMetadataToken" -Fix 'Derive each production PR title from the goal and surface, and describe its owner and concrete delivered files.'
    }
}
if ($sequenceGoalWorkflow -notmatch '## Overall deliverable' -or
    $sequenceGoalWorkflow -notmatch [regex]::Escape('const title = `feat(sequence-goal): ${titleSuffix}`')) {
    Add-Failure -Check 'Sequence-goal coordinator PR metadata does not lead with the main goal and overall deliverable.' -Fix 'Use the primary goal as the coordinator title and lead its body with the overall cross-surface deliverable.'
}

$testStandardsCheck = Get-FileText -RepoRelativePath 'scripts/check-test-standards.ps1'
if ($null -ne $testStandardsCheck -and
    ($testStandardsCheck -notmatch 'introduces Moq in new or amended unit-test code' -or
     $testStandardsCheck -notmatch 'introduces an ad hoc IHttpConnector test implementation' -or
     $testStandardsCheck -notmatch 'Test-ProjectHasMockResource' -or
     $testStandardsCheck -notmatch 'ls-files --others --exclude-standard' -or
     $testStandardsCheck -notmatch 'Test-Path -LiteralPath \$resolvedScope -PathType Leaf' -or
     $testStandardsCheck -notmatch [regex]::Escape("if (`$HeadRef -eq 'HEAD')"))) {
    Add-Failure -Check 'Unit-test standards check does not enforce Moq and shared HTTP mock-pipeline rules across supported changed-code scopes.' -Fix 'Detect added Moq/ad hoc connector APIs, require shared DI plus .mock resources, include committed/working-tree/untracked test code, and normalize project-file scopes to their directory.'
}
$testStandardsModule = Get-FileText -RepoRelativePath 'scripts/TestStandards.psm1'
$testStandardsExercise = Get-FileText -RepoRelativePath 'scripts/invoke-test-standards-exercise.ps1'
if ($null -ne $testStandardsModule -and
    ($testStandardsModule -notmatch 'Test-ContainsMoqUsage' -or
     $testStandardsModule -notmatch 'Test-ContainsAdHocHttpConnectorUsage' -or
     $testStandardsModule -notmatch 'Test-ContainsSharedHttpTestPipeline' -or
     $testStandardsModule -notmatch 'global::' -or
     $testStandardsModule -notmatch 'A-Za-z_' -or
     $testStandardsModule -notmatch '\\\(')) {
    Add-Failure -Check 'Unit-test standards Moq detector does not cover aliases, qualified names, and call syntax.' -Fix 'Detect alias/global imports, qualified Moq names, and generic, member, or call-shaped Moq APIs.'
}
if ($null -ne $testStandardsExercise -and
    ($testStandardsExercise -notmatch 'using M = Moq' -or
     $testStandardsExercise -notmatch 'Mock\(objectType\)' -or
     $testStandardsExercise -notmatch 'FakeConnector : IHttpConnector' -or
     $testStandardsExercise -notmatch 'AddSingleton<IHttpClientFactory' -or
     $verifyScript -notmatch 'invoke-test-standards-exercise\.ps1' -or
     $verifyScript -notmatch "failed \+= 'test-standards-strength'")) {
    Add-Failure -Check 'Unit-test standards detector bypass cases are not exercised by verification.' -Fix 'Run executable alias-import and non-generic Mock call regression cases from verify.ps1.'
}

$generatedCsharpFormatter = Get-FileText -RepoRelativePath 'scripts/format-generated-csharp.ps1'
if ($null -ne $generatedCsharpFormatter -and $generatedCsharpFormatter -notmatch 'ls-files --others --exclude-standard') {
    Add-Failure -Check 'Generated C# formatter does not include untracked files created during remediation.' -Fix 'Include untracked .cs files before resolving owning projects and invoking dotnet format.'
}

$sequenceGoalCommentMonitorWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-comment-monitor.yml'
if ($null -ne $sequenceGoalCommentMonitorWorkflow) {
    if ($sequenceGoalCommentMonitorWorkflow -match $draftPrExclusionPattern) {
        Add-Failure -Check 'Sequence-goal comment monitor excludes draft PRs.' -Fix 'Dispatch comment workers for every trusted open sequence-goal PR regardless of draft state.'
    }
    if ($sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*pull_request_review:\s*$' -or $sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*pull_request_review_comment:\s*$' -or $sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*issue_comment:\s*$') {
        Add-Failure -Check 'Sequence-goal comment monitor is not event-driven for every supported PR comment type.' -Fix 'Dispatch from submitted reviews, inline review comments, and top-level PR issue comments; retain schedule only as recovery.'
    }

    if ($sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*workflow_run:\s*$' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*workflows:\s*\[\s*["'']?Copilot Label Automation["'']?\s*\]\s*$' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch 'workflowRun\?\.pull_requests\?\.\[0\]\?\.number' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch 'workflowRun\?\.conclusion\s*!==\s*"success"') {
        Add-Failure -Check 'Sequence-goal comment monitor has no trusted Copilot completion relay.' -Fix 'Use workflow_run for the completed Copilot Label Automation workflow and resolve its pull request number before dispatching comment agents.'
    }

    if ($sequenceGoalCommentMonitorWorkflow -notmatch '(?m)^\s*-\s*cron:\s*[''"]?\*/15 \* \* \* \*[''"]?\s*$' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch 'scheduledRecovery\s*=\s*context\.eventName\s*===\s*"schedule"' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch 'const requestedPr\s*=\s*scheduledRecovery\s*\?\s*null' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch '"ai-delivery-sequence-goal-human-comment-loop\.yml"' -or
        $sequenceGoalCommentMonitorWorkflow -notmatch '"ai-delivery-sequence-goal-pr-review-loop\.yml"') {
        Add-Failure -Check 'Sequence-goal comment monitor cannot recover AI and human feedback events blocked as action_required.' -Fix 'Retain the fifteen-minute full-PR recovery scan and dispatch both the human-comment and AI-review workers for every trusted open sequence-goal PR.'
    }

    if ($sequenceGoalCommentMonitorWorkflow -notmatch 'actions/create-github-app-token@v2' -or $sequenceGoalCommentMonitorWorkflow -notmatch 'SEQUENCE_GOAL_GITHUB_APP_SLUG') {
        Add-Failure -Check 'Sequence-goal comment monitor does not authenticate and filter with the dedicated GitHub App.' -Fix 'Use the sequence-goal GitHub App token for trusted event-driven worker dispatch.'
    }

    if ($sequenceGoalCommentMonitorWorkflow -notmatch 'steps\.app-token\.outputs\.token\s*\|\|\s*github\.token' -or $sequenceGoalCommentMonitorWorkflow -notmatch "if:\s*steps\.app-credentials\.outputs\.configured == 'true'") {
        Add-Failure -Check 'Sequence-goal comment monitor does not support safe operation before GitHub App configuration.' -Fix 'Skip App-token creation when credentials are absent and use the workflow token as an explicit rollout fallback.'
    }

    if ($sequenceGoalCommentMonitorWorkflow -notmatch 'context\.eventName === "issue_comment" && !context\.payload\.issue\?\.pull_request') {
        Add-Failure -Check 'Sequence-goal comment monitor can fan out a regular issue comment to all delivery PRs.' -Fix 'Return without dispatching when an issue_comment payload is not associated with a pull request.'
    }
}

$codingStandards = Get-FileText -RepoRelativePath 'docs/standards/coding-standards.md'
if ($null -ne $codingStandards -and
    $codingStandards -notmatch 'each top-level class, record, struct, enum, and interface in its own `\.cs` file named after that type') {
    Add-Failure -Check 'Coding standards do not require separate files for top-level C# types.' -Fix 'Document the one-top-level-type-per-file convention in docs/standards/coding-standards.md.'
}

$agentLoopWorkflow = Get-FileText -RepoRelativePath 'docs/agent-loop-workflow.md'
$sequenceGoalScript = Get-FileText -RepoRelativePath 'scripts/run-sequence-goal-agent.ps1'
$sequenceGoalQaPublisher = Get-FileText -RepoRelativePath 'scripts/publish-sequence-goal-qa-pr.ps1'
if ($null -ne $sequenceGoalScript -and
    ($sequenceGoalScript -notmatch 'Do not introduce Moq or copy legacy Moq patterns' -or
     $sequenceGoalScript -notmatch 'HttpClientFactoryFixture')) {
    Add-Failure -Check 'Sequence-goal agent guardrails do not explicitly require the repository TDD test approach.' -Fix 'Inject the no-Moq, real-DI, and resource-boundary fixture requirements for generation and remediation.'
}
if ($null -ne $sequenceGoalScript) {
    if ($sequenceGoalScript -notmatch 'Join-Path \$repoRoot "TestResults/agent-resource/sequence-goal-loop-\$\(\$env:GITHUB_RUN_ID\)-\$deliveryPhaseSlug-\$deliverySurfaceSlug-\$diagramSlug\$evidenceLabelSuffix\.jsonl"' -or
        $sequenceGoalScript -notmatch 'IsPathRooted\(\$UsagePath\)' -or
        $sequenceGoalScript -notmatch 'IsPathRooted\(\$BudgetEvidencePath\)' -or
        $sequenceGoalScript -notmatch 'ended without a terminal turn\.completed event' -or
        $sequenceGoalScript -notmatch "\$lastParsedEventType -ne 'turn\.completed'" -or
        $sequenceGoalScript -notmatch 'Test-Path -LiteralPath \$UsagePath -PathType Leaf' -or
        $sequenceGoalScript -notmatch '\$budgetSucceeded = \$\?') {
        Add-Failure -Check 'Sequence-goal phases can share stale usage evidence or accept an incomplete Codex turn.' -Fix 'Use phase-specific JSONL files and require both a terminal turn.completed event and successful resource evidence validation.'
    }
    if ($sequenceGoalScript -notmatch 'Command-output safety:' -or
        $sequenceGoalScript -notmatch 'TestResults/agent-command-logs' -or
        $sequenceGoalScript -notmatch '\$maximumInfrastructureRetries = 1' -or
        $sequenceGoalScript -notmatch 'event stream lagged\|dropped \\d\+ events' -or
        $sequenceGoalScript -notmatch '\$null -ne \$liveEvent -and' -or
        $sequenceGoalScript -notmatch 'Retrying once') {
        Add-Failure -Check 'Sequence-goal agent execution does not mitigate oversized command streams or transient dropped-event failures.' -Fix 'Keep noisy command output in retained files and retry one time only when dropped events prevent a terminal turn event.'
    }
    if ($null -ne $agentLoopWorkflow -and
        ($agentLoopWorkflow -notmatch 'only retry exception' -or
         $agentLoopWorkflow -notmatch 'incomplete invocation remains in the retained JSONL evidence')) {
        Add-Failure -Check 'Agent-loop guidance does not reconcile the dropped-event retry with the terminal turn requirement.' -Fix 'Document dropped-event recovery as the only retry exception and retain the incomplete invocation in JSONL evidence.'
    }
    if ($sequenceGoalScript -notmatch "ValidateSet\('Implementation', 'AutomationTests'\)" -or
        $sequenceGoalScript -notmatch 'VAA\.EAPI\.Automation/AGENTS\.md' -or
        $sequenceGoalScript -notmatch 'RetrieveFromStorageTable' -or
        $sequenceGoalScript -notmatch 'EnsureGQLResponseSuccess') {
        Add-Failure -Check 'Sequence-goal agent runner does not inject the scoped automation-test generation contract.' -Fix 'Support an AutomationTests phase that reads the automation harness guidance and enforces its test-data and response-assertion conventions.'
    }
    if ($sequenceGoalScript -notmatch '\[string\]\$ProductionContextPath' -or
        $sequenceGoalScript -notmatch '(?s)\$DeliveryPhase -eq ''AutomationTests''.*?IsNullOrWhiteSpace\(\$ProductionContextPath\)' -or
        $sequenceGoalScript -notmatch '\.codex/agent-loop/production-context' -or
        $sequenceGoalScript -notmatch 'Production context cannot be a symbolic link or reparse point' -or
        $sequenceGoalScript -notmatch 'Production context ancestor cannot be a symbolic link or reparse point' -or
        $sequenceGoalScript -notmatch 'Production context was not consumed before agent execution' -or
        $sequenceGoalScript -notmatch 'AUTHENTICATED PRODUCTION PR SNAPSHOT \(UNTRUSTED DATA\)' -or
        $sequenceGoalScript -notmatch 'agent intentionally receives no GitHub token' -or
        $sequenceGoalScript -notmatch 'lack of simulator or BrowserStack credentials is not by itself a blocker') {
        Add-Failure -Check 'QA generation cannot inspect authenticated production changes without exposing a GitHub token or demanding an unauthorized live run.' -Fix 'Inject a bounded authenticated PR snapshot as untrusted context and keep live UI execution gated by explicit authorization.'
    }
    if ($sequenceGoalScript -notmatch 'Repository-owned implementation guardrails' -or
        $sequenceGoalScript -notmatch 'ProjectReference graph' -or
        $sequenceGoalScript -notmatch 'Extension availability is solution/project dependent' -or
        $sequenceGoalScript -notmatch 'HTTP content/message/header, dictionary, object, byte, logging, function-context, configuration, test-fixture, and domain-specific extensions' -or
        $sequenceGoalScript -notmatch '\.IsNullOrWhiteSpace\(\)' -or
        $sequenceGoalScript -notmatch '\.Is\(') {
        Add-Failure -Check 'Sequence-goal agents are not given solution-aware repository extension guardrails.' -Fix 'Require discovery of applicable extension classes through the owning project reference graph, with string helpers retained as concrete examples.'
    }
    if ($sequenceGoalScript -notmatch 'Generated C# layout' -or
        $sequenceGoalScript -notmatch 'each top-level class, record, struct, enum, and interface in its own `\.cs` file named after that type' -or
        $sequenceGoalScript -notmatch 'Do not bundle multiple top-level types') {
        Add-Failure -Check 'Sequence-goal agents can bundle generated C# types into a single source file.' -Fix 'Inject the one-top-level-type-per-file layout requirement into every generation phase.'
    }
    if ($sequenceGoalScript -notmatch 'Generated diagrams and flowcharts' -or
        $sequenceGoalScript -notmatch "default unstyled presentation" -or
        $sequenceGoalScript -notmatch 'Do not add background or fill colours, borders, boundary boxes or containers' -or
        $sequenceGoalScript -notmatch 'actual project name where one exists') {
        Add-Failure -Check 'Sequence-goal agents are missing generated diagram presentation rules.' -Fix 'Inject the unstyled, boundary-free, project-labelled diagram contract into every generation phase.'
    }
    if ($sequenceGoalScript -notmatch 'open and visually inspect it before planning' -or
        $sequenceGoalScript -notmatch 'direct implementation instruction' -or
        $sequenceGoalScript -notmatch 'Data mapping, static inline content, or accessibility identifiers alone are not sufficient' -or
        $sequenceGoalScript -notmatch 'compare it with the associated production PR' -or
        $sequenceGoalScript -notmatch 'Supplied UX requires a complete UX contract') {
        Add-Failure -Check 'Sequence-goal UI agents can ignore or downgrade supplied UX.' -Fix 'Fail early without an authoritative UX contract, require visual inspection for implementation, and make UI automation reject incomplete mapped UI.'
    }
    if ($sequenceGoalScript -notmatch 'Supplied UX requires a completed Design inspection matrix before the delivery agent can start' -or
        $sequenceGoalScript -notmatch '\$designInspectionMatrix\.Groups\[''value''\]\.Value -match') {
        Add-Failure -Check 'Sequence-goal delivery can start before supplied UX has a completed design inspection matrix.' -Fix 'Fail in the launcher before invoking the coding agent when the matrix is absent or placeholder text.'
    }
    if ($sequenceGoalScript -notmatch 'Supplied UX cannot be Not applicable' -or
        $sequenceGoalScript -notmatch [regex]::Escape('^(?:none|n/a)$')) {
        Add-Failure -Check 'Sequence-goal agent treats Not applicable as absent UX.' -Fix 'Reject Not applicable explicitly and reserve None/n/a for the no-UX detector.'
    }
    if ($sequenceGoalScript -notmatch '\[\^\\s\|\(\)\]\+' -or $sequenceGoalScript -notmatch 'mp4\|mov\|webm') {
        Add-Failure -Check 'Sequence-goal agent UX detection can miss bare file paths or video artefacts.' -Fix 'Recognise bare relative paths, URLs, image files, documents, and video formats as supplied UX.'
    }
    if ($sequenceGoalScript -notmatch 'Treat every mandatory target-repository verification command as a delivery gate' -or
        $sequenceGoalScript -notmatch 'downstream pull-request CI is additional evidence, not a substitute') {
        Add-Failure -Check 'Cross-repository agents can skip mandatory target lint or verification.' -Fix 'Fail closed when a required target command fails or cannot run, without deferring the gate to pull-request CI.'
    }
    if ($sequenceGoalScript -notmatch 'SEQUENCE_GOAL_AGENT_MODEL') {
        Add-Failure -Check 'Sequence-goal agent script does not read the code-generation model variable.' -Fix 'Read SEQUENCE_GOAL_AGENT_MODEL and expose it through the {model} placeholder.'
    }
    if ($sequenceGoalScript -notmatch 'The workflow has preloaded the mandatory repository context below' -or
        $sequenceGoalScript -notmatch 'Never concatenate multiple large Get-Content calls' -or
        $sequenceGoalScript -notmatch 'VAA\.EAPI\.Automation/docs/principles/test-design-principles\.md' -or
        $sequenceGoalScript -notmatch 'AUTOMATION_COVERAGE_ALREADY_EXISTS:' -or
        $sequenceGoalScript -notmatch 'Do not reread them') {
        Add-Failure -Check 'Sequence-goal agent bootstrap can reread large mandatory guidance files in one blocking subprocess.' -Fix 'Preload mandatory context into the prompt and require additional large files to be read individually in bounded commands.'
    }
    if ($sequenceGoalScript -notmatch 'Resource budget configuration is missing the workflows\.sequence-goal-loop entry' -or
        $sequenceGoalScript -notmatch "is not enrolled for sequence-goal-loop resource enforcement" -or
        $sequenceGoalScript -notmatch 'has no approved rate card') {
        Add-Failure -Check 'Sequence-goal agent launcher does not clearly reject a missing workflow budget, a model outside the allowlist, or a missing rate card.' -Fix 'Validate the sequence-goal-loop budget and SEQUENCE_GOAL_AGENT_MODEL against docs/agent-resource-budgets.json before invoking Codex.'
    }

    if ($sequenceGoalScript -notmatch '\{model\}') {
        Add-Failure -Check 'Sequence-goal agent script does not expose the model placeholder.' -Fix 'Support the {model} placeholder so the configured agent command can receive the selected model.'
    }

    if ($sequenceGoalScript -notmatch 'CODEX_API_KEY') {
        Add-Failure -Check 'Sequence-goal agent script does not map API-key auth for Codex CLI.' -Fix 'Map OPENAI_API_KEY to CODEX_API_KEY before invoking codex exec so API-key automation authenticates correctly.'
    }

    if ($sequenceGoalScript -notmatch 'sandbox_mode = "danger-full-access"' -or $sequenceGoalScript -notmatch 'approval_policy = "never"' -or $sequenceGoalScript -notmatch '--dangerously-bypass-approvals-and-sandbox' -or $sequenceGoalScript -notmatch 'Normalized Codex CI launch args') {
        Add-Failure -Check 'Sequence-goal agent script does not force write-capable Codex CI permissions.' -Fix 'Write an ephemeral Codex config and normalize Codex launch args to --dangerously-bypass-approvals-and-sandbox before invoking codex exec in the isolated GitHub Actions runner.'
    }
}
if ($null -eq $sequenceGoalQaPublisher -or
    $sequenceGoalQaPublisher -notmatch '(?m)^#Requires -Version 7\.0\s*$' -or
    $sequenceGoalQaPublisher -notmatch "Repository -notmatch '\^\[A-Za-z0-9\]" -or
    $sequenceGoalQaPublisher -notmatch "BaseBranch\.StartsWith\('-'\)" -or
    $sequenceGoalQaPublisher -notmatch 'git check-ref-format --branch \$BaseBranch' -or
    $sequenceGoalQaPublisher -notmatch 'sequence-goal/.+qa-' -or
    $sequenceGoalQaPublisher -notmatch 'git -c core\.longpaths=true worktree add --detach' -or
    $sequenceGoalQaPublisher -notmatch 'git -c core\.longpaths=true worktree remove --force' -or
    $sequenceGoalQaPublisher -notmatch '\$rootUri\.IsBaseOf\(\$diagramUri\)' -or
    $sequenceGoalQaPublisher -notmatch 'GetExtension\(\$diagramAbsolute\) -ne ''\.md''' -or
    $sequenceGoalQaPublisher -notmatch 'DiagramPath must identify a regular Markdown file directly under docs/sequence-diagrams' -or
    $sequenceGoalQaPublisher -notmatch 'foreach \(\$file in @\(\$allowed \| Where-Object \{ \$_ -in \$untracked \}\)\) \{ git add -N -- \$file \}' -or
    $sequenceGoalQaPublisher -notmatch 'register-sequence-goal-branch.ps1' -or
    $sequenceGoalQaPublisher -notmatch 'gh pr edit' -or
    $sequenceGoalQaPublisher -notmatch '\$goalTitle' -or
    $sequenceGoalQaPublisher -notmatch '\$qaTitle = "test\(\$qaScope\)' -or
    $sequenceGoalQaPublisher -notmatch '## Delivered outcome' -or
    $sequenceGoalQaPublisher -notmatch '## Delivered automation' -or
    $sequenceGoalQaPublisher -notmatch '## Delivery ownership' -or
    $sequenceGoalQaPublisher -notmatch 'Production head SHA' -or
    $sequenceGoalQaPublisher -notmatch 'foreach \(\$file in \$allowed\)' -or
    $sequenceGoalQaPublisher -notmatch 'QA-\$Surface\.json') {
    Add-Failure -Check 'Sequence-goal QA agents do not publish isolated, production-bound EAPI pull requests.' -Fix 'Publish each QA surface from an independent main-based worktree, bind it to the production checkpoint, and clean source changes after publication.'
}
$reviewScopeOpening = 'Review only regressions introduced by this PR relative to its base branch that affect the stated delivery acceptance criteria.'
$draftRegistration = Get-FileText -RepoRelativePath 'scripts/register-sequence-goal-branch.ps1'
if ($null -eq $draftRegistration -or
    $draftRegistration -match 'gh pr create' -or
    $draftRegistration -notmatch 'Write-GoalTaskState' -or
    $draftRegistration -match '\.sequence-goal-delivery/' -or
    $sequenceGoalWorkflow -notmatch '(?s)register-sequence-goal-branch\.ps1.*?run-sequence-goal-agent\.ps1' -or
    $sequenceGoalQaWorkflow -notmatch '(?s)publish-sequence-goal-qa-pr\.ps1 -PrepareOnly.*?run-sequence-goal-agent\.ps1' -or
    $sequenceGoalQaPublisher -notmatch 'QA publication requires its registered goal-document branch') {
    Add-Failure -Check 'Delivery agents can start without goal-document branch registration.' -Fix 'Record the branch in the goal document before invoking either agent; create PRs after substantive work.'
}
$reviewScopeDeferral = 'Do not request unrelated hardening, architectural improvements, pre-existing fixes, stylistic changes, or speculative edge cases here; record those as follow-up work.'
$reviewScopeRereview = 'Do not restart an unrestricted whole-system redesign review.'
if ($null -ne $sequenceGoalWorkflow -and
    (([regex]::Matches($sequenceGoalWorkflow, [regex]::Escape('## Review scope'))).Count -lt 3 -or
     ([regex]::Matches($sequenceGoalWorkflow, [regex]::Escape($reviewScopeOpening))).Count -lt 3 -or
     ([regex]::Matches($sequenceGoalWorkflow, [regex]::Escape($reviewScopeDeferral))).Count -lt 3 -or
     ([regex]::Matches($sequenceGoalWorkflow, [regex]::Escape($reviewScopeRereview))).Count -lt 3)) {
    Add-Failure -Check 'Sequence-goal production or coordinator PR descriptions do not constrain automated review to delivery regressions.' -Fix 'Keep the review-scope contract in production PR creation and refresh bodies and in the coordinator PR body.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch [regex]::Escape('## Review scope') -or
     $sequenceGoalQaPublisher -notmatch [regex]::Escape($reviewScopeOpening) -or
     $sequenceGoalQaPublisher -notmatch [regex]::Escape($reviewScopeDeferral) -or
     $sequenceGoalQaPublisher -notmatch [regex]::Escape($reviewScopeRereview))) {
    Add-Failure -Check 'Sequence-goal QA PR descriptions do not constrain automated review to delivery regressions.' -Fix 'Keep the review-scope contract in every generated QA PR body.'
}
$sequenceGoalAssociatedPrRecorderContract = Get-FileText -RepoRelativePath 'scripts/record-sequence-goal-associated-prs.ps1'
if ($null -ne $sequenceGoalAssociatedPrRecorderContract -and
    ($sequenceGoalAssociatedPrRecorderContract -notmatch '\$oldPhase -ne ''QA''' -or
     $sequenceGoalAssociatedPrRecorderContract -notmatch '\$oldMergedAt' -or
     $sequenceGoalAssociatedPrRecorderContract -notmatch "\{ 'Merged' \}")) {
    Add-Failure -Check 'A new QA delivery cannot safely supersede and archive its authenticated merged checkpoint.' -Fix 'Permit only QA merged-checkpoint replacement and retain the exact prior merged identity in historical evidence.'
}
$emptyStderrNormalizationPattern = '\(\(Get-Content -LiteralPath \$stderrPath -Raw\) \?\? ''''\)\.Trim\(\)'
if ($null -ne $sequenceGoalAssociatedPrRecorderContract -and
    ($sequenceGoalAssociatedPrRecorderContract -notmatch $emptyStderrNormalizationPattern -or
     $associatedPrFeedbackWorkflow -notmatch $emptyStderrNormalizationPattern)) {
    Add-Failure -Check 'Sequence-goal GitHub CLI stderr handling can dereference null for a successful command.' -Fix 'Normalize an empty redirected stderr file to an empty string before trimming it in recorder and feedback verification paths.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch '\$surfaces = @\(\)' -or
     $sequenceGoalDeliveryResolver -notmatch 'SEQUENCE_GOAL_COORDINATOR_REPOSITORY' -or
     $sequenceGoalWorkflow -notmatch '(?m)^  deliver-cross-repository-production:' -or
     $sequenceGoalWorkflow -notmatch 'publish-sequence-goal-qa-pr\.ps1' -or
     $sequenceGoalWorkflow -notmatch 'ProductionHeadSha' -or
     $sequenceGoalWorkflow -notmatch 'Backend production agent changed QA-owned automation paths' -or
     $sequenceGoalWorkflow -notmatch '(?ms)^  open-coordinator-pr:.*?needs: \[resolve-delivery-agents, run-sequence-loop\]' -or
     $sequenceGoalWorkflow -notmatch 'open-sequence-goal-coordinator-pr\.ps1' -or
     $sequenceGoalQaWorkflow -notmatch '(?m)^  deliver-automation-tests:' -or
     $sequenceGoalWorkflow -notmatch 'save-sequence-goal-delivery\.ps1' -or
     $sequenceGoalQaWorkflow -notmatch '\$coordinatorGoalPaths = @\(\$diagrams' -or
     $sequenceGoalQaWorkflow -notmatch '\$_ -cnotin \$coordinatorGoalPaths' -or
     $sequenceGoalQaWorkflow -notmatch '\$_\.path -cin \$coordinatorGoalPaths' -or
     $sequenceGoalQaWorkflow -notmatch 'coordinator_handoff_path=\$coordinatorHandoffRoot" \| Out-File -FilePath \$githubCommandFileState\[''GITHUB_OUTPUT''\]\.path -Append' -or
     $sequenceGoalQaWorkflow -notmatch 'name: Upload validated coordinator goal changes' -or
     $sequenceGoalQaWorkflow -notmatch 'save-sequence-goal-delivery\.ps1 -Evidence \$checkpoint -GoalChangesDirectory' -or
     $null -eq $sequenceGoalQaWorkflow -or
     $sequenceGoalQaWorkflow -match 'uses: \./\.github/workflows/ai-delivery-sequence-goal-loop\.yml' -or
     $sequenceGoalWorkflow -notmatch 'convertPullRequestToDraft' -or
     $sequenceGoalWorkflow -notmatch 'markPullRequestReadyForReview' -or
     (Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml') -notmatch 'declaresNoOpenPoints' -or
     (Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml') -notmatch 'expected exactly one canonical Open points to finalize section' -or
     (Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml') -notmatch 'unresolved open points remain')) {
    Add-Failure -Check 'Sequence-goal production, QA, or coordinator delivery can bypass the individual-PR lifecycle or mishandle unresolved open points.' -Fix 'Route every delivery surface through associated PRs, publish verified question-blocked goals as draft coordinator PRs, and refuse completion until the questions are resolved.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'Failed to commit Ready-to-InProgress status' -or
     $sequenceGoalWorkflow -notmatch 'Failed to push Ready-to-InProgress status' -or
     $sequenceGoalWorkflow -match 'id: commit_impl\s*\r?\n\s*if: false')) {
    Add-Failure -Check 'Sequence-goal coordinator checkpoints or ledger persistence can silently skip or fail.' -Fix 'Fail closed on goal status and associated-PR document update failures without disabled lifecycle gates.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch "statusMatch\.Groups\['status'\]\.Value\.Trim\(\) -eq 'Complete'" -or
     $sequenceGoalWorkflow -notmatch "(?s)summary\.status -eq 'InProgress'.*openQuestions")) {
    Add-Failure -Check 'Sequence-goal workflow can treat unresolved open points as Complete or fail to publish them for human input.' -Fix 'Set has_complete only from an actual Complete status and separately make verified question-blocked goals publishable as draft PRs.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'qa_matrix:' -or
     $sequenceGoalWorkflow -notmatch '(?ms)^  deliver-automation-tests:.*?strategy:\s*\n\s+fail-fast: false.*?matrix: \$\{\{ fromJSON\(needs\.resolve-delivery-agents\.outputs\.qa_matrix\) \}\}' -or
     $sequenceGoalWorkflow -notmatch 'name: \$\{\{ matrix\.agent \}\}' -or
     $sequenceGoalWorkflow -match '\$surfaces = ''\$\{\{ needs\.run-sequence-loop\.outputs\.delivery_surfaces \}\}''\.Split')) {
    Add-Failure -Check 'Sequence-goal QA surfaces are not isolated as per-agent matrix jobs.' -Fix 'Build a QA matrix from the selected surfaces, run one named QA agent per matrix job with fail-fast disabled, and do not loop all QA surfaces through one mutable runner.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'actions\.listWorkflowRuns' -or
     $associatedPrMonitorWorkflow -notmatch 'pulls.listFiles' -or
     $associatedPrMonitorWorkflow -notmatch 'trustedOpenCoordinator' -or
     $associatedPrMonitorWorkflow -notmatch 'registeredCoordinatorNumbers\.includes' -or
     $associatedPrMonitorWorkflow -notmatch 'candidate\.hasUnresolvedOpenPoints' -or
     $associatedPrMonitorWorkflow -notmatch 'questionPayloadAcknowledged' -or
     $associatedPrMonitorWorkflow -notmatch 'expectedQuestionLines' -or
     $associatedPrMonitorWorkflow -notmatch 'Marked recovered Complete coordinator PR' -or
     $associatedPrMonitorWorkflow -notmatch 'waiting for its question or review workflow instead of redispatching' -or
     $associatedPrMonitorWorkflow -notmatch 'QA-\(\?:Backend\|MobileIos\|MobileAndroid\|WebUi\)')) {
    Add-Failure -Check 'Associated-PR monitoring still depends on an early coordinator PR or excludes QA PR rows.' -Fix 'Discover InProgress coordinator branches directly and monitor Backend, remote production, and per-surface QA rows.'
}

if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'Write-GoalTaskState' -or
     $sequenceGoalWorkflow -notmatch '-Task Coordinator -State' -or
     $sequenceGoalWorkflow -notmatch 'Initialize coordinator state in goal document')) {
    Add-Failure -Check 'Sequence-goal workflow does not persist coordinator state in the goal document.' -Fix 'Record branch and run identity in the Coordinator task through the shared document updater.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -match 'gh pr list --repo \$Repository --state all --head \$branch' -or
     $sequenceGoalQaPublisher -notmatch 'git ls-remote --heads origin "refs/heads/\$branch"' -or
     $sequenceGoalQaPublisher -notmatch 'Collision-safe QA branch')) {
    Add-Failure -Check 'Sequence-goal QA publisher treats closed PRs or stale deterministic branches as current delivery.' -Fix 'Reuse only authenticated open PRs and allocate a new run-qualified branch when the deterministic ref already exists; never delete a potentially claimed ref.'
}

$sequenceGoalLoopWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-loop.yml'
if ($null -ne $sequenceGoalLoopWorkflow) {
    if ($sequenceGoalLoopWorkflow -notmatch 'gh pr list --repo \$env:DELIVERY_REPOSITORY --state open --limit 1000' -or
        $sequenceGoalLoopWorkflow -notmatch 'Where-Object state -eq ''OPEN''' -or
        $sequenceGoalLoopWorkflow -notmatch 'git ls-remote --heads origin "refs/heads/\$branch"' -or
        $sequenceGoalLoopWorkflow -notmatch 'Collision-safe delivery branch' -or
        $sequenceGoalQaPublisher -notmatch 'gh pr list --repo \$Repository --state open --limit 1000' -or
        $sequenceGoalQaPublisher -notmatch '\$derivedBranch' -or
        $sequenceGoalQaPublisher -notmatch '\$retryBranchPattern') {
        Add-Failure -Check 'Sequence-goal delivery discovery treats closed PRs or stale deterministic branches as current delivery.' -Fix 'Discover only open production and QA PRs and allocate a new run-qualified branch when a deterministic ref already exists.'
    }
    if ($sequenceGoalLoopWorkflow -match 'Substring\(3\)\.Trim\(\)') {
        Add-Failure -Check 'Sequence-goal workflow parses git status porcelain for generated paths.' -Fix 'Collect tracked changes with git diff --name-only and untracked changes with git ls-files so paths containing spaces are preserved.'
    }

    if ($sequenceGoalLoopWorkflow -notmatch 'git diff --name-only --no-renames HEAD' -or $sequenceGoalLoopWorkflow -notmatch 'git ls-files --others --exclude-standard') {
        Add-Failure -Check 'Sequence-goal workflow does not safely collect tracked and untracked generated paths.' -Fix 'Use git diff --name-only --no-renames HEAD together with git ls-files --others --exclude-standard.'
    }
}

if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'group: sequence-goal-loop-\$\{\{ github\.ref \}\}' -or
     $sequenceGoalWorkflow -notmatch "needs\.run-sequence-loop\.result == 'success'.*needs\.resolve-delivery-agents\.outputs\.remote_count != '0'" -or
     $sequenceGoalWorkflow -notmatch '(?s)retain-agent-resource-evidence:.*?if: always\(\)' -or
     $sequenceGoalWorkflow -notmatch '\$completedProductionSurfaces' -or
     $sequenceGoalWorkflow -notmatch '\$completedQaSurfaces' -or
     $sequenceGoalWorkflow -notmatch 'does not publish failure evidence from a checkout that evaluated agent-controlled projects' -or
     $sequenceGoalWorkflow -notmatch "git stash push --include-untracked -m 'discarded sequence-goal verification failure workspace'")) {
    Add-Failure -Check 'Sequence-goal workflow lacks full-run serialization, bot-run suppression, merged-surface filtering, or clean failure-evidence recovery.' -Fix 'Serialize the full branch workflow, suppress bot push resolution, skip merged ledger surfaces, and clean generated automation before rebasing failure evidence.'
}
if ($null -ne $sequenceGoalQaPublisher -and $sequenceGoalQaPublisher -notmatch '\$fullSafeGoal') {
    Add-Failure -Check 'QA branch naming omits the full coordinator identity.' -Fix 'Sanitize the coordinator identity before deriving its bounded branch name.'
}
if ($null -ne $sequenceGoalQaPublisher -and $sequenceGoalQaPublisher -notmatch 'gh pr list --repo \$Repository --state open --base \$BaseBranch') {
    Add-Failure -Check 'QA PR discovery does not narrow open candidates by base on the server.' -Fix 'Pass the configured base branch to gh pr list before applying trusted identity filters client-side.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch 'Failed to revalidate the recorded QA delivery checkpoint' -or
     $sequenceGoalQaPublisher -notmatch '\$checkpoint\.mergedAt' -or
     $sequenceGoalQaPublisher -notmatch 'Recorded \$ledgerSurface delivery row is malformed')) {
    Add-Failure -Check 'QA publication can replace a concurrently merged or malformed ledger checkpoint.' -Fix 'Revalidate the recorded QA PR before creating work and reuse an authenticated merged checkpoint.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch '\$priorGitConfig' -or
     $sequenceGoalQaPublisher -notmatch '(?s)try \{.*?\} finally \{' -or
     $sequenceGoalQaPublisher -notmatch 'Remove-Item -LiteralPath "Env:\$name"')) {
    Add-Failure -Check 'QA publication leaks temporary Git authentication into the caller process.' -Fix 'Save the existing process-level Git configuration and restore or remove every temporary authentication variable in a finally block.'
}

$sequenceGoalAssociatedPrRecorder = Get-FileText -RepoRelativePath 'scripts/record-sequence-goal-associated-prs.ps1'
if ($null -ne $sequenceGoalAssociatedPrRecorder -and
    ($sequenceGoalAssociatedPrRecorder -notmatch 'ValidateScript' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'repository-relative Markdown leaf under docs/sequence-diagrams' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'Resolve-Path -LiteralPath \$candidate' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'ResolveLinkTarget\(\$true\)' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'Split-Path -Parent \$resolvedCandidate' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\[Parameter\(Mandatory\)\]\[string\]\$CoordinatorRepository' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'gh pr view \$prNumber --repo \$repository' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\$ghExitCode = \$LASTEXITCODE' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\$diagramPath = Join-Path \$repoRoot \$diagram' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'duplicate active rows for \$rowSurface' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'legacy bullet entries' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'Existing associated delivery row for \$surface is malformed' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'did not match the authenticated PR identity' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\$validatedDiagramPaths')) {
    Add-Failure -Check 'Associated-PR recorder accepts unsafe diagram write targets.' -Fix 'Require repository-relative Markdown leaves directly under docs/sequence-diagrams and resolve both the leaf and its parent against the repository root.'
}
if ($null -ne $sequenceGoalAssociatedPrRecorder -and
    ($sequenceGoalAssociatedPrRecorder -notmatch 'function Test-AssociatedPrMetadata' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\[regex\]::IsMatch\(\$Body, "\(\?m\)\^' -or
     $sequenceGoalAssociatedPrRecorder -match '\.Contains\(\$surfaceIdentity')) {
    Add-Failure -Check 'Associated-PR recorder accepts quoted or prose-embedded identity metadata.' -Fix 'Require exact canonical coordinator, surface, and phase lines; reject unsupported metadata.'
}
if ($null -ne $sequenceGoalAssociatedPrRecorder -and
    ($sequenceGoalAssociatedPrRecorder -notmatch 'unrecognized or malformed table row' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '--json number,url,state,mergedAt,headRefName,baseRefName,body,author,headRepository' -or
     $sequenceGoalAssociatedPrRecorder -notmatch 'Superseded associated PR .* did not match the authenticated ledger identity')) {
    Add-Failure -Check 'Associated-PR recorder can discard malformed rows or archive an unauthenticated superseded PR.' -Fix 'Fail on every unrecognized ledger row and revalidate the complete repository, branch, author, coordinator, phase, and surface identity before archiving a superseded PR.'
}
if ($null -ne $sequenceGoalAssociatedPrRecorder -and
    ($sequenceGoalAssociatedPrRecorder -notmatch "NewGuid\(\)\.ToString\('N'\)" -or
     $sequenceGoalAssociatedPrRecorder -notmatch '(?s)try \{.*?gh pr view.*?\} finally \{.*?Remove-Item -LiteralPath \$stderrPath')) {
    Add-Failure -Check 'Associated-PR recorder reuses or leaks its native-command stderr file.' -Fix 'Use an invocation-unique temporary path and remove it from a finally block.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'branch = \$env:GITHUB_REF_NAME' -or
     $sequenceGoalWorkflow -match "GetBytes\('\$\{\{ github\.ref_name \}\}'\)" -or
     $sequenceGoalWorkflow -match "branch\s*=\s*'\$\{\{ github\.ref_name \}\}'")) {
    Add-Failure -Check 'Sequence-goal coordinator state interpolates the branch name into PowerShell source.' -Fix 'Read GITHUB_REF_NAME from the environment when recording coordinator branch identity.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalQaPublisher -notmatch '\$derivedBranch' -or
     $sequenceGoalQaPublisher -notmatch '\$_\.headRefName -in @\(\$derivedBranch\)' -or
     $sequenceGoalWorkflow -notmatch '(?s)Clean smoke-test artifacts.*?Remove-Item -LiteralPath \$diagram')) {
    Add-Failure -Check 'Sequence-goal QA preflight or smoke cleanup does not preserve branch identity and goal lifecycle.' -Fix 'Filter reusable QA PRs by the canonical branch identity and remove the completed smoke goal document during cleanup.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'gh pr list --repo .* --state open --base .* --head \$branch' -or
     $sequenceGoalWorkflow -notmatch 'failed final authenticated identity validation' -or
     $sequenceGoalWorkflow -notmatch '(?s)format-generated-csharp\.ps1 -Base origin/main.*?publish-sequence-goal-qa-pr\.ps1')) {
    Add-Failure -Check 'Sequence-goal delivery publication or completion omits final formatting and authenticated PR identity checks.' -Fix 'Format QA changes before publication, scope created-PR lookup by base, and revalidate the full associated-PR identity immediately before completion.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'id: coordinator-token' -or
     $sequenceGoalWorkflow -notmatch '\$_\.headRefName -in @\(\$derivedBranch\)' -or
     $sequenceGoalWorkflow -notmatch 'Inferential review' -or
     $sequenceGoalQaWorkflow -notmatch 'Trusted QA publication left uncommitted coordinator-checkout changes' -or
     $sequenceGoalWorkflow -notmatch '\$current\.headRefName -ne \$row\.Groups\[''head''\]' -or
     $sequenceGoalWorkflow -notmatch 'Read coordinator state from goal document' -or
     $sequenceGoalWorkflow -notmatch 'valid existing resource record' -or
     $sequenceGoalWorkflow -notmatch '\$fullSafeRef' -or
     $sequenceGoalWorkflow -notmatch 'prepare smoke-test goal' -or
     $sequenceGoalWorkflow -notmatch 'livePr\.headRepository\.nameWithOwner' -or
     $sequenceGoalWorkflow -notmatch 'trustedCoordinatorAuthors' -or
     $sequenceGoalWorkflow -notmatch 'http\.https://github\.com/\.extraheader \$gitAuthHeader' -or
     $sequenceGoalWorkflow -notmatch 'Coordinator PR identity verification failed after create/update' -or
     $sequenceGoalWorkflow -notmatch 'Set-GoalCoordinatorState' -or
     $sequenceGoalWorkflow -notmatch 'save-sequence-goal-resources.ps1' -or
     $sequenceGoalQaWorkflow -notmatch 'save-sequence-goal-resources.ps1')) {
    Add-Failure -Check 'Sequence-goal delivery does not protect cross-repository credentials, deterministic PR reuse, metadata, or QA resume paths.' -Fix 'Use separate coordinator credentials, bind reusable PRs to the derived branch, preserve inferential review metadata, allow initial QA-only publication, and fail closed on retained-evidence writes.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -match 'feedbackWorkerBusy' -or
     $associatedPrMonitorWorkflow -notmatch 'ref: coordinatorBranch\.headSha' -or
     $associatedPrMonitorWorkflow -notmatch 'hasClosedUnmerged' -or
     $associatedPrMonitorWorkflow -notmatch 'closed without merge' -or
     $associatedPrMonitorWorkflow -notmatch 'attemptCount >= 3' -or
     $associatedPrMonitorWorkflow -notmatch 'const delayMinutes = hasPendingAuthoredFeedback && attemptCount === 0 \? 0 : 5;' -or
     $associatedPrMonitorWorkflow -notmatch 'hasPendingInferentialReview \? \[pr\.updated_at\]' -or
     $associatedPrMonitorWorkflow -notmatch '\(!deliveryPending && hasPendingInferentialReview\)' -or
     $associatedPrMonitorWorkflow -notmatch 'worker concurrency will serialize dispatch, but exhaustion is deferred' -or
     $associatedPrMonitorWorkflow -notmatch '!feedbackActivityKnown' -or
     $associatedPrMonitorWorkflow -notmatch 'let hasDeliveryPending = false;' -or
     $associatedPrMonitorWorkflow -notmatch 'hasDeliveryPending = true;' -or
     $associatedPrMonitorWorkflow -notmatch 'shared writer locking keeps feedback handling serialized with delivery recovery' -or
     $associatedPrMonitorWorkflow -notmatch 'const needsRecovery = missingChildren \|\| hasDeliveryPending' -or
     $associatedPrMonitorWorkflow -notmatch 'Automatic continuation limit reached' -or
     $associatedPrMonitorWorkflow -notmatch '!allActiveMerged && !\(hasClosedUnmerged && !hasActiveOrUntrusted\)' -or
     $associatedPrMonitorWorkflow -notmatch 'associated\.length > 0 && !allActiveMerged' -or
     $associatedPrMonitorWorkflow -notmatch 'pulls.listFiles' -or
     $associatedPrMonitorWorkflow -notmatch 'registered.length !== 1' -or
     $associatedPrMonitorWorkflow -notmatch 'diagramPaths.length !== 1' -or
     $associatedPrMonitorWorkflow -notmatch 'trustedCoordinatorAuthors.has' -or
     $associatedPrMonitorWorkflow -notmatch 'Date\.now\(\) < eligibleAt' -or
     $associatedPrMonitorWorkflow -notmatch 'pr\.head\.ref === item\.head' -or
     $associatedPrMonitorWorkflow -notmatch 'sequence-goal-associated-feedback:exhausted:' -or
     $associatedPrMonitorWorkflow -notmatch 'Automatic remediation stopped after three unsuccessful attempts' -or
     $associatedPrMonitorWorkflow -notmatch 'latestFailedRunUrl' -or
     $associatedPrMonitorWorkflow -notmatch 'Open the latest failed run or check' -or
     $associatedPrFeedbackWorkflow -notmatch 'group: sequence-goal-associated-feedback-\$\{\{ inputs\.repository \}\}-\$\{\{ inputs\.pr_number \}\}' -or
     $associatedPrFeedbackWorkflow -notmatch 'group: sequence-goal-production-writer-\$\{\{ inputs\.repository \}\}-\$\{\{ inputs\.coordinator_branch \}\}-\$\{\{ inputs\.surface \}\}' -or
     $associatedPrFeedbackWorkflow -notmatch 'deliveryState.status' -or
     $associatedPrFeedbackWorkflow -notmatch 'core\.setOutput\("delivery_pending"' -or
     $associatedPrFeedbackWorkflow -notmatch 'feedback fixes are verified without publishing readiness or removing draft state')) {
    Add-Failure -Check 'Associated-PR monitoring can lose originating goal identity, globally block remediation, delay authored feedback, retry checks too quickly, or stall on closed PRs.' -Fix 'Discover the exact goal document through its authenticated open coordinator PR, rely on per-PR and shared writer concurrency, dispatch human and AI comments without debounce, retain the fixed five-minute delay for check-driven attempts, and ignore closed checkpoints.'
}
$sequenceGoalPrFinalizerContract = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml'
$sequenceGoalHumanCommentWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml'
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'FEEDBACK_MAX_ATTEMPTS: 3' -or
     $associatedPrFeedbackWorkflow -notmatch 'run-name: associated-feedback-' -or
     $associatedPrFeedbackWorkflow -notmatch 'sequence-goal-associated-feedback:attempt:' -or
     $associatedPrFeedbackWorkflow -notmatch 'Associated PR head changed while feedback was running' -or
     $associatedPrFeedbackWorkflow -notmatch 'Every independently collected Mobile iOS credential failure must be covered' -or
     $associatedPrFeedbackWorkflow -notmatch 'Review thread \$\{threadId\} could not be refreshed' -or
     $associatedPrFeedbackWorkflow -notmatch 'Associated PR became terminal or changed head' -or
     $associatedPrFeedbackWorkflow -notmatch 'refusing to publish verification from the stale head')) {
    Add-Failure -Check 'Associated-PR feedback cannot be safely retried or fail closed on a stale head.' -Fix 'Allow bounded workflow reruns and reject stale-head verification so the monitor redispatches.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and $null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'feedback_key:' -or
     $associatedPrFeedbackWorkflow -notmatch 'attempt:\$\{headSha\}:\$\{feedbackKey\}:' -or
     $associatedPrMonitorWorkflow -notmatch 'feedback-\$\{require\("crypto"\).*?pendingFeedbackIds.*?slice\(0, 16\)' -or
     $associatedPrMonitorWorkflow -notmatch 'hasPendingAuthoredFeedback && attemptCount === 0 \? 0 : 5' -or
     $associatedPrMonitorWorkflow -notmatch 'feedback_key: feedbackKey' -or
     $associatedPrFeedbackWorkflow -notmatch 'core\.setOutput\("feedback_key", feedbackKey\)' -or
     $associatedPrFeedbackWorkflow -notmatch 'feedbackKey !== process\.env\.EXPECTED_FEEDBACK_KEY' -or
     $associatedPrFeedbackWorkflow -notmatch '\(!deliveryPending \|\| phase === "Production"\)' -or
     $associatedPrMonitorWorkflow -notmatch 'hasPendingFeedback && \(!deliveryPending \|\| expectedPhase === "Production"\)' -or
     $associatedPrMonitorWorkflow -notmatch 'QA recovery retains exclusive write ownership until publication completes' -or
     $associatedPrMonitorWorkflow -notmatch 'exhausted:\$\{pr\.head\.sha\}:\$\{feedbackKey\}' -or
     $associatedPrMonitorWorkflow -notmatch 'A different pending-feedback set or a new head SHA starts a fresh bounded remediation series' -or
     $associatedPrFeedbackWorkflow -match 'Sequence-Production-Checkpoint' -or
     $associatedPrFeedbackWorkflow -notmatch 'if \(evidence\.surface === "MobileIos" && !deliveryPending\)')) {
    Add-Failure -Check 'Associated feedback can inherit an unrelated retry budget, erase pending production recovery state, or suppress valid pending-iOS replies.' -Fix 'Key the complete pending-feedback set independently, refresh the authenticated production checkpoint after pending-delivery changes, and bypass readiness-only reply validation while delivery remains pending.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'checkedPasses\.length === requiredPasses\.length' -or
     $associatedPrFeedbackWorkflow -notmatch 'Live Mobile iOS PR is draft or no longer contains the canonical six-pass' -or
     ([regex]::Matches($associatedPrFeedbackWorkflow, 'gh api --paginate --slurp')).Count -lt 2)) {
    Add-Failure -Check 'Associated feedback accepts malformed inferential evidence or truncates exact-head checks.' -Fix 'Require the exact ordered six-pass contract immediately before readiness and paginate every exact-head check/status query.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'checkedInferentialPasses\.length === requiredInferentialPasses\.length' -or
     $associatedPrMonitorWorkflow -notmatch 'inferentialOutcomes\.length === 1')) {
    Add-Failure -Check 'Associated monitoring accepts a malformed Mobile iOS inferential-review contract.' -Fix 'Require exactly the six canonical checked pass lines in order and one outcome line.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch '\$actualPasses\.Count -ne \$requiredPasses\.Count' -or
     $sequenceGoalWorkflow -notmatch '\$outcomes\.Count -ne 1')) {
    Add-Failure -Check 'Sequence-goal completion accepts a malformed Mobile iOS inferential-review contract.' -Fix 'Compare the exact ordered checked pass list and require one outcome before completion.'
}
if ($null -ne $sequenceGoalPrFinalizerContract -and
    ($sequenceGoalPrFinalizerContract -notmatch 'actualPasses\.length !== requiredPasses\.length' -or
     $sequenceGoalPrFinalizerContract -notmatch 'deliverySummary' -or
     $sequenceGoalPrFinalizerContract -notmatch 'Delivery: Coordinator')) {
    Add-Failure -Check 'Finalization accepts malformed inferential evidence or omits coordinator state from the goal document.' -Fix 'Require the canonical pass sequence and read delivery state from the Coordinator entry in the goal document.'
}
if ($null -ne $sequenceGoalHumanCommentWorkflow -and
    ($sequenceGoalHumanCommentWorkflow -notmatch 'const \{ data: livePr \} = await github\.rest\.pulls\.get' -or
     $sequenceGoalHumanCommentWorkflow -notmatch 'headSha: livePr\.head\.sha')) {
    Add-Failure -Check 'Human-comment evidence can bind live feedback to a stale PR head.' -Fix 'Fetch the live PR immediately before writing the immutable evidence snapshot.'
}
if ($null -ne $sequenceGoalQaPublisher -and $sequenceGoalQaPublisher -notmatch '\$checkpoint\.url -ceq \$ledgerMatch\.Groups\[''url''\]\.Value') {
    Add-Failure -Check 'QA merged-checkpoint reuse does not authenticate the ledger URL.' -Fix 'Require the live PR URL to exactly equal the recorded ledger URL.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'headSha: branch\.commit\.sha' -or
     $associatedPrMonitorWorkflow -notmatch 'trustedOpenCoordinator\.head\.sha !== candidate\.goalHeadSha' -or
     $associatedPrMonitorWorkflow -match 'pr\.head\?\.sha === branch\.commit\.sha')) {
    Add-Failure -Check 'Complete recovery is not bound to the inspected coordinator revision.' -Fix 'Read the goal at the discovered commit and require the live coordinator to retain that head before marking ready.'
}
if ($null -ne $sequenceGoalAssociatedPrRecorder -and
    ($sequenceGoalAssociatedPrRecorder -notmatch '\(\?m\)\^\- Surface: `\{\{1,2\}\}' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '\(\?m\)\^\- Phase: `\{\{1,2\}\}')) {
    Add-Failure -Check 'Associated-PR recorder rejects a supported canonical metadata quoting form.' -Fix 'Accept exactly one or two backticks on anchored current surface and phase lines.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'productionRepositories\[bodySurface\]' -or
     $associatedPrFeedbackWorkflow -notmatch 'productionBases\[bodySurface\]' -or
     $associatedPrFeedbackWorkflow -match 'productionRepositories\[surface\]')) {
    Add-Failure -Check 'Associated feedback indexes production routing with a phase-qualified QA surface.' -Fix 'Derive the delivery surface once and use it for production repository and base lookups.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch '\$existingGitConfigCount \+ 1' -or
     $sequenceGoalQaPublisher -notmatch 'GIT_CONFIG_KEY_\$existingGitConfigCount' -or
     $sequenceGoalQaPublisher -match "GIT_CONFIG_COUNT = '1'")) {
    Add-Failure -Check 'QA publication overwrites inherited Git command configuration.' -Fix 'Append the authentication header after all inherited GIT_CONFIG entries and restore the complete environment family.'
}
if ($null -ne $sequenceGoalPrReviewWorkflow -and
    ($sequenceGoalPrReviewWorkflow -notmatch 'Review evidence batch was already finalized' -or
     $sequenceGoalPrReviewWorkflow -notmatch 'finalized_head=\$expectedHead' -or
     $sequenceGoalPrReviewWorkflow -match "diffExitCode -eq 0\) \{ throw 'Review evidence produced no durable")) {
    Add-Failure -Check 'Idempotent review-evidence finalization fails instead of resuming.' -Fix 'Treat an authenticated already-applied evidence batch as a successful no-op and emit its unchanged head.'
}
if ($null -ne $sequenceGoalScript -and
    ($sequenceGoalScript -notmatch 'function Remove-CSharpCommentsPreservingStrings' -or
     $sequenceGoalScript -match '\[regex\]::Replace\(\[regex\]::Replace\(\$Content, ''\(\?s\)/\\\*\.\*\?\\\*/''')) {
    Add-Failure -Check 'Automation fingerprint comment removal can erase C# string-literal content.' -Fix 'Use the stateful C# comment stripper that preserves regular, verbatim, interpolated, and character literals.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'hasDedicatedLine' -or
     $associatedPrMonitorWorkflow -notmatch '\(!hasCompleteInferentialContract \|\| pr\.draft\)' -or
     $associatedPrMonitorWorkflow -match '\(value\.body \|\| ""\)\.includes\("\[from-codex\]"\)')) {
    Add-Failure -Check 'Associated-PR monitor trusts user-controlled response text or quoted lifecycle markers.' -Fix 'Treat feedback as handled only when a trusted automation comment contains the exact lifecycle marker on its own line.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'hasDedicatedLine' -or
     $associatedPrFeedbackWorkflow -match 'comment\.body\.includes\("\[from-codex\]"\)' -or
     $associatedPrFeedbackWorkflow -notmatch 'blockingCheckRuns' -or
     $associatedPrFeedbackWorkflow -notmatch 'filter=latest' -or
     $associatedPrFeedbackWorkflow -notmatch 'if \(!hasCompleteContract' -or
     $associatedPrFeedbackWorkflow -notmatch 'hasCanonicalReview' -or
     $associatedPrFeedbackWorkflow -notmatch 'markPullRequestReadyForReview')) {
    Add-Failure -Check 'Associated feedback trusts user-controlled response text, blocks delegated target checks, or exposes iOS PRs before inferential review.' -Fix 'Use exact trusted-author marker lines, exclude only exact delegated check identities from generic failures, and keep new iOS PRs draft until normalized inferential evidence is recorded.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch '(?s)Collect unhandled feedback.*?Reserve durable actionable feedback attempt' -or
     $associatedPrFeedbackWorkflow -notmatch 'fixed five-minute associated-feedback retry interval' -or
     $associatedPrFeedbackWorkflow -notmatch 'immutableCredentialFailures = "\$\{\{ inputs\.surface \}\}" === "MobileIos" \? items')) {
    Add-Failure -Check 'Associated-feedback attempts are not reserved from actionable feedback or do not classify complete credential diagnostics.' -Fix 'Collect first, reserve and rate-limit only actionable attempts, and classify annotations and log tails through the immutable collected check body.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'checkRunsByIdentity' -or
     $associatedPrFeedbackWorkflow -notmatch 'const maximumBatchSize = 20' -or
     $associatedPrFeedbackWorkflow -notmatch 'const pendingFeedbackIds = collectedItems' -or
     $associatedPrFeedbackWorkflow -notmatch 'Processing \$\{items\.length\} of \$\{collectedItems\.length\} unhandled items' -or
     $associatedPrFeedbackWorkflow -notmatch 'Associated-feedback agent failed twice' -or
     $associatedPrFeedbackWorkflow -match 'failingConclusions = new Set\(\[[^\]]*(?:action_required|cancelled)')) {
    Add-Failure -Check 'Associated-feedback collection can overwhelm remediation with duplicate or non-actionable CI history.' -Fix 'Deduplicate latest checks, exclude cancellation/approval states, prioritize authored feedback, and retain the bounded 20-item recovery batches.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'checkRunsByIdentity' -or
     $associatedPrMonitorWorkflow -match 'failingConclusions = new Set\(\[[^\]]*(?:action_required|cancelled)')) {
    Add-Failure -Check 'Associated-feedback monitor and worker disagree about actionable check failures.' -Fix 'Deduplicate by provider/name and exclude cancellation/approval states in both collection paths.'
}
if ($null -ne $sequenceGoalHumanCommentWorkflow -and $sequenceGoalHumanCommentWorkflow -notmatch 'human-comment root identity field') {
    Add-Failure -Check 'Human-comment evidence root identity is mutable.' -Fix 'Compare PR, branch, base, head SHA, and run ID with the immutable collector snapshot.'
}
if ($null -ne $sequenceGoalPrReviewWorkflow -and $sequenceGoalPrReviewWorkflow -notmatch 'AI review root identity field') {
    Add-Failure -Check 'PR-review evidence root identity is mutable.' -Fix 'Compare PR, branch, base, head SHA, and run ID with the immutable collector snapshot.'
}
$normalizedImmutableSha256Contract = '(?m)^\s*\$actualImmutableSha256 = \(Get-FileHash -LiteralPath \$env:IMMUTABLE_PATH -Algorithm SHA256\)\.Hash\.ToLowerInvariant\(\)\r?\n\s*if \(\$actualImmutableSha256 -cne \$env:IMMUTABLE_SHA256\.ToLowerInvariant\(\)\)'
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch 'immutable_sha256' -or
     $associatedPrFeedbackWorkflow -notmatch $normalizedImmutableSha256Contract -or
     $associatedPrFeedbackWorkflow -notmatch 'Get-IdentityView' -or
     $associatedPrFeedbackWorkflow -notmatch '\^VAAD1ST\\/\[A-Za-z0-9_\.\-\]\+\$' -or
     $associatedPrFeedbackWorkflow -notmatch 'missingRequiredChecks' -or
     $associatedPrFeedbackWorkflow -notmatch "app\.slug.*'github-actions'.*required\.author" -or
     $associatedPrFeedbackWorkflow -notmatch "creator\.login.*'external-ci'.*required\.author")) {
    Add-Failure -Check 'Associated-PR feedback trusts agent-editable identities or unrelated target checks.' -Fix 'Bind delegation to an immutable collected failure ID, then require the same check name/context and provider to succeed on the expected head.'
}
if ($null -ne $associatedPrMonitorWorkflow -and $associatedPrMonitorWorkflow -match 'retentionCutoff') {
    Add-Failure -Check 'Associated-PR monitoring expires active coordinator discovery after a fixed retention period.' -Fix 'Discover goals for as long as their authenticated coordinator PR remains open.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'Final state.*Abandoned' -or
      $sequenceGoalQaWorkflow -match '(?m)^  run-sequence-loop:' -or
      $sequenceGoalQaWorkflow -match '(?m)^  deliver-cross-repository-production:' -or
      $sequenceGoalDeliveryResolver -notmatch 'has no authenticated production PR checkpoint' -or
     $sequenceGoalWorkflow -notmatch '(?m)^  dispatch-missing-qa:' -or
     $sequenceGoalWorkflow -notmatch 'QA_HAS_INPROGRESS:.*needs\.resolve-delivery-agents\.outputs\.has_inprogress' -or
     $sequenceGoalWorkflow -notmatch 'INPROGRESS_LIST:.*needs\.resolve-delivery-agents\.outputs\.inprogress_list')) {
    Add-Failure -Check 'Abandoned goals can be automatically resumed or App-authored ledger pushes cannot complete QA.' -Fix 'Honor durable abandonment evidence and use resolver context for both QA and completion when the coordinator job is intentionally skipped.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'Marked recovered Complete coordinator PR' -or
     $associatedPrMonitorWorkflow -notmatch 'Coordinator closed during monitoring; no restart dispatched' -or
     $associatedPrMonitorWorkflow -notmatch 'trustedOpenCoordinator' -or
     $associatedPrMonitorWorkflow -notmatch 'Deferring Complete recovery' -or
     $associatedPrMonitorWorkflow -notmatch 'has a closed-unmerged child PR; refusing to create an invalid coordinator PR')) {
    Add-Failure -Check 'Associated-PR monitor cannot resume publication of an open Complete goal safely.' -Fix 'Mark the authenticated open coordinator ready only after validating child bindings and the unchanged coordinator head; never recreate a closed coordinator.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -notmatch '\`\{1,2\}VAAD1ST/eapi@' -or
     $associatedPrFeedbackWorkflow -match 'if \(\$headBefore -ne \$headAfter\)')) {
    Add-Failure -Check 'Associated-PR routing rejects valid double-backtick metadata or retains unreachable head-change retry logic.' -Fix 'Accept canonical single/double-backtick identities and fail closed once the expected target head changes.'
}
if ($null -ne $sequenceGoalScript -and $null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalScript -notmatch '\$UsagePath\.outcome\.json' -or
     $sequenceGoalScript -notmatch '\$lastParsedEventType -eq ''item\.completed''' -or
     $sequenceGoalScript -notmatch '--diff-filter=AMR' -or
     $sequenceGoalScript -notmatch '\^VAA\\\.EAPI\\\.Automation/\.\+\\\.\(cs\|feature\)\$' -or
     $sequenceGoalScript -notmatch 'AutomationTests completed without automation changes or a valid existing-coverage attestation' -or
     $sequenceGoalQaPublisher -notmatch 'QA resource-usage evidence was not produced' -or
     $sequenceGoalQaPublisher -notmatch 'QA outcome evidence was not produced')) {
    Add-Failure -Check 'Sequence-goal QA publishing still depends on an unchecked prose-only agent result.' -Fix 'Validate the AutomationTests result immediately and publish from versioned outcome evidence.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'qa_count:' -or
     $sequenceGoalWorkflow -notmatch "deliver-cross-repository-production:.*(?s).*needs: \[resolve-delivery-agents, run-sequence-loop, open-coordinator-pr\]" -or
     $sequenceGoalWorkflow -notmatch "needs\.open-coordinator-pr\.result == 'success'" -or
     $sequenceGoalQaWorkflow -notmatch 'resolve-sequence-goal-delivery-agents\.ps1 -RequestedSurface' -or
     $sequenceGoalDeliveryResolver -notmatch '\$RequestedSurface\.Trim\(\)' -or
     $sequenceGoalCoordinatorWorkflow -notmatch "group: sequence-goal-loop-\$\{\{ github\.ref \}\}" -or
     $sequenceGoalWorkflow -match 'max-parallel:\s*1' -or
     $sequenceGoalWorkflow -notmatch "deliver-automation-tests:.*(?s).*needs\.resolve-delivery-agents\.outputs\.qa_count != '0'" -or
     $sequenceGoalCoordinatorWorkflow -notmatch "needs\.resolve-delivery-agents\.outputs\.qa_count == '0'" -or
     $sequenceGoalWorkflow -notmatch 'if: always\(\).*needs\.resolve-delivery-agents\.result' -or
     $sequenceGoalWorkflow -notmatch "deliver-cross-repository-production\.result == 'skipped'.*remote_count == '0'" -or
     $sequenceGoalWorkflow -notmatch "deliver-cross-repository-production\.result == 'success'.*deliver-cross-repository-production\.result == 'skipped'" -or
     $sequenceGoalWorkflow -match 'record-production-only-delivery-prs:' -or
     $sequenceGoalCoordinatorWorkflow -notmatch 'retain-agent-resource-evidence:.*(?s).*needs: \[resolve-delivery-agents, open-coordinator-pr, deliver-cross-repository-production\]' -or
     $sequenceGoalWorkflow -notmatch '\$phaseIdentity\s*=.*Phase:.*Production' -or
     $sequenceGoalQaPublisher -notmatch '\$phaseIdentity\s*=.*Phase:.*QA' -or
     $sequenceGoalWorkflow -notmatch '\$_.author\.login -in \$trustedAuthors' -or
     $sequenceGoalWorkflow -notmatch '\$_.headRepository\.nameWithOwner' -or
     $sequenceGoalWorkflow -notmatch 'Collision-safe delivery branch' -or
     $sequenceGoalWorkflow -notmatch 'Failed to discover existing associated delivery PRs' -or
     $sequenceGoalQaPublisher -notmatch 'Failed to discover existing QA delivery PRs' -or
     $sequenceGoalQaWorkflow -notmatch 'Download associated cross-repository PR evidence.*(?s).*continue-on-error: true' -or
     $sequenceGoalWorkflow -notmatch '\$allBudgetEvidencePaths\.Count -gt 0' -or
     $sequenceGoalWorkflow -notmatch '\$refHash = \[Convert\]::ToHexString' -or
     $sequenceGoalAssociatedPrRecorderContract -notmatch 'Historical associated delivery pull requests')) {
    Add-Failure -Check 'Sequence-goal phase ordering, empty-QA completion, collision-resistant naming, or historical PR evidence has drifted.' -Fix 'Gate production on coordinator success, model an explicit QA count, hash new deterministic branches while reusing existing PR identities, and move superseded ledger rows into history.'
}
if ($null -ne $sequenceGoalWorkflow -and $null -ne $sequenceGoalQaPublisher -and $null -ne $sequenceGoalAssociatedPrRecorder -and
    (([regex]::Matches($sequenceGoalWorkflow, '"app/\$appSlug"')).Count -lt 4 -or
     $sequenceGoalQaPublisher -notmatch '"app/\$\(\$env:SEQUENCE_GOAL_GITHUB_APP_SLUG\)"' -or
     $sequenceGoalAssociatedPrRecorder -notmatch '"app/\$GitHubAppSlug"')) {
    Add-Failure -Check 'Sequence-goal delivery rejects GitHub CLI App author identities.' -Fix 'Trust the authenticated GitHub App in slug, slug[bot], and GraphQL app/slug forms throughout delivery discovery and ledger recording.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -match "has_deferred == 'true' && steps\.commit\.outputs\.changes_pushed" -or
     $associatedPrFeedbackWorkflow -notmatch "target_verification\.outcome == 'success'.*target_verification\.outcome == 'skipped'" -or
     $associatedPrFeedbackWorkflow -notmatch '\$lastCheckSignature' -or
     $associatedPrFeedbackWorkflow -notmatch 'five-minute stabilisation window')) {
    Add-Failure -Check 'Deferred associated-PR verification can be acknowledged without passing a stable target check set.' -Fix 'Verify deferred results even without a source push, withhold durable markers on failure, and restart a five-minute window whenever the target check set changes.'
}
if ($null -ne $associatedPrFeedbackWorkflow) {
    $commitFeedbackIndex = $associatedPrFeedbackWorkflow.IndexOf('Commit and push valid feedback changes')
    $publishInferentialIndex = $associatedPrFeedbackWorkflow.IndexOf('Publish inferential review and start target PR verification')
    $waitTargetIndex = $associatedPrFeedbackWorkflow.IndexOf('Wait for target PR verification')
    if ($commitFeedbackIndex -lt 0 -or $publishInferentialIndex -le $commitFeedbackIndex -or $waitTargetIndex -le $publishInferentialIndex -or
        $associatedPrFeedbackWorkflow -notmatch '\$requiresTargetVerification = \$evidence\.surface -eq ''MobileIos''' -or
        $associatedPrFeedbackWorkflow -notmatch 'id: publish_inferential' -or
        $associatedPrFeedbackWorkflow -notmatch 'beforeUpdate\.body \|\| ""\) !== currentBody' -or
        $associatedPrFeedbackWorkflow -notmatch 'refreshed\.body \|\| ""\) !== nextBody' -or
        ([regex]::Matches($associatedPrFeedbackWorkflow, 'github\.rest\.pulls\.update')).Count -ne 1 -or
        $associatedPrFeedbackWorkflow -notmatch 'readyResult\.markPullRequestReadyForReview\.pullRequest\.isDraft' -or
        $associatedPrFeedbackWorkflow -notmatch 'Sort-Object kind,id,name,author -Unique' -or
        $associatedPrFeedbackWorkflow -notmatch "name = 'Documentation Validation'" -or
        $associatedPrFeedbackWorkflow -notmatch "name = 'Build and Test'" -or
        $associatedPrFeedbackWorkflow -notmatch "steps\.publish_inferential\.outcome == 'success'" -or
        $associatedPrFeedbackWorkflow -notmatch 'Report failed target PR verification' -or
        $associatedPrFeedbackWorkflow -notmatch 'Target PR verification did not pass' -or
        $associatedPrFeedbackWorkflow -match 'credential-bound target verification did not produce' -or
        $associatedPrFeedbackWorkflow -notmatch 'markPullRequestReadyForReview' -or
        $associatedPrFeedbackWorkflow -notmatch 'refreshed\.head\.sha !== expectedHeadSha') {
        Add-Failure -Check 'Mobile iOS inferential review no longer starts credentialed target CI before verification.' -Fix 'Publish the canonical review against the pushed SHA, mark the PR ready, and then wait for the target repository checks.'
    }
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch '\$LASTEXITCODE -notin @\(0, 5\)' -or
     $sequenceGoalWorkflow -notmatch '\$global:LASTEXITCODE = 0' -or
     $sequenceGoalWorkflow -notmatch '\$cleanupErrors \+= "target repository \(exit \$LASTEXITCODE\)"' -or
     $sequenceGoalWorkflow -notmatch '\$cleanupErrors \+= "coordinator repository \(exit \$LASTEXITCODE\)"' -or
     $sequenceGoalWorkflow -match '(?m)^\s*run: git config --local --unset-all http\.https://github\.com/\.extraheader')) {
    Add-Failure -Check 'Sequence-goal Git authentication cleanup can falsely fail a successful job.' -Fix 'Accept git-config exit code 5 when the key is already absent, reject unexpected codes, and reset the native exit code after cleanup.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch '\$goalHash = \[Convert\]::ToHexString' -or
     $sequenceGoalQaPublisher -notmatch '\$coordinatorIdentity' -or
     $sequenceGoalQaPublisher -notmatch '\$surfaceIdentity' -or
     $sequenceGoalQaPublisher -notmatch '\$phaseIdentity' -or
     $sequenceGoalQaPublisher -notmatch '\$_.author\.login -in \$trustedAuthors' -or
     $sequenceGoalQaPublisher -notmatch '\$_.headRepository\.nameWithOwner -eq \$Repository' -or
     $sequenceGoalQaPublisher -notmatch 'Failed to discover existing QA delivery PRs' -or
     $sequenceGoalQaPublisher -notmatch 'Failed to refresh QA base branch' -or
     $sequenceGoalQaPublisher -notmatch 'Collision-safe QA branch' -or
     $sequenceGoalQaPublisher -notmatch 'headRefName')) {
    Add-Failure -Check 'QA delivery discovery lacks canonical branch identity or collision protection.' -Fix 'Use canonical coordinator-derived QA branches and authenticate reusable PRs by coordinator, surface, phase, repository, and author.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch '\$hasExactLine' -or
     $sequenceGoalWorkflow -notmatch '\$invalidAssociatedRows' -or
     $sequenceGoalWorkflow -match '\.Contains\(\$coordinatorIdentity')) {
    Add-Failure -Check 'Sequence-goal completion or PR reuse accepts embedded identity text or silently ignores malformed ledger rows.' -Fix 'Require anchored metadata lines for every child PR identity and reject every unrecognized associated-delivery table row.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch '\$hasExactLine' -or
     $sequenceGoalQaPublisher -match '\.Contains\(\$coordinatorIdentity')) {
    Add-Failure -Check 'Sequence-goal QA publication accepts coordinator identity text embedded in unrelated PR prose.' -Fix 'Authenticate supported QA metadata using anchored lines, including when reusing a merged PR.'
}
$sequenceGoalPrFinalizerContract = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml'
if ($null -ne $sequenceGoalPrFinalizerContract -and
    ($sequenceGoalPrFinalizerContract -notmatch 'evidenceCommitShas.*filter\(sha => sha !== pr\.merge_commit_sha\)' -or
     $sequenceGoalPrFinalizerContract -notmatch 'invalidTableRows' -or
     $sequenceGoalPrFinalizerContract -notmatch 'pr\.merged \? "Merged" : "Closed"' -or
     $sequenceGoalPrFinalizerContract -notmatch 'text\.replace\(coordinatorPattern, coordinatorRow\)' -or
     $sequenceGoalPrFinalizerContract -notmatch '\(pr\.merged && !.*Status' -or
     $sequenceGoalPrFinalizerContract -notmatch 'Delivery: Coordinator' -or
     $sequenceGoalPrFinalizerContract -notmatch 'coordinatorRow' -or
     $sequenceGoalPrFinalizerContract -notmatch 'Transient evidence was not archived')) {
    Add-Failure -Check 'Sequence-goal finalization trusts merge-authored evidence, malformed ledger rows, or loses the terminal coordinator checkpoint.' -Fix 'Trust evidence only from authenticated source commits, validate all table rows, and retain merged or closed coordinator identity with the correct terminal state.'
}
if ($null -ne $sequenceGoalScript -and
    ($sequenceGoalScript -notmatch 'feature runner currently materializes only' -or
     $sequenceGoalScript -notmatch '\(\?:\\\[\|,\)\\s\*\(\?:InlineData' -or
     $sequenceGoalScript -notmatch '\$global:LASTEXITCODE = 0' -or
     $sequenceGoalScript -match "\[regex\]::Replace\(.*'\\s\+', ''")) {
    Add-Failure -Check 'Sequence-goal automation fingerprints lose executable literals, bindings, provider changes, or unsupported outlines.' -Fix 'Preserve literal whitespace, include test support files and step bindings, and reject scenario outlines until the repository runner supports them.'
}
if ($null -ne $sequenceGoalQaPublisher -and
    ($sequenceGoalQaPublisher -notmatch '\(\?<url>\[\^\)\]\+\)' -or
     $sequenceGoalQaPublisher -notmatch 'if \(\$existing.Count -eq 1 -and \$existing\[0\].mergedAt\)' -or
     $sequenceGoalQaPublisher -notmatch 'A new QA attempt requires an open draft')) {
    Add-Failure -Check 'QA checkpoint recovery can lose URL identity or discard newly generated automation changes.' -Fix 'Capture the ledger URL and allocate an open draft when preparing a new attempt after a merged checkpoint.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    ($associatedPrFeedbackWorkflow -match 'legacyPhase|legacySurface' -or
     $associatedPrFeedbackWorkflow -notmatch 'currentMetadata' -or
     $associatedPrFeedbackWorkflow -notmatch 'currentCoordinatorIdentity')) {
    Add-Failure -Check 'Associated feedback accepts phase or surface fragments outside a complete metadata record.' -Fix 'Match only anchored current metadata records before running the write-capable worker.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'currentMetadata' -or
     $associatedPrMonitorWorkflow -notmatch 'trustedOpenCoordinator\.head\.sha !== candidate\.goalHeadSha')) {
    Add-Failure -Check 'Associated monitoring accepts incomplete identity metadata or races trusted coordinator finalization.' -Fix 'Require complete current metadata, an open coordinator, and the same coordinator head before readiness publication.'
}
$sequenceGoalEvidenceFinalizerContract = Get-FileText -RepoRelativePath 'scripts/finalize-sequence-goal-pr-evidence.ps1'
if ($null -ne $sequenceGoalEvidenceFinalizerContract -and
    ($sequenceGoalEvidenceFinalizerContract -notmatch '\$canonicalBatchPattern' -or
     $sequenceGoalEvidenceFinalizerContract -notmatch 'HEAD:\$diagramRelative' -or
     $sequenceGoalEvidenceFinalizerContract -notmatch 'github-actions\\\[bot\\\].*users\\\.noreply\\\.github\\\.com' -or
     $sequenceGoalEvidenceFinalizerContract -notmatch 'exists outside a canonical closure batch')) {
    Add-Failure -Check 'PR evidence idempotence trusts an unstructured agent-editable marker.' -Fix 'Accept an existing batch only when the marker is inside the complete canonical closure-batch structure.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'state: "open", base: "main"' -or
     $associatedPrMonitorWorkflow -notmatch 'registered.length !== 1' -or
     $associatedPrMonitorWorkflow -notmatch 'Coordinator closed during monitoring; no restart dispatched')) {
    Add-Failure -Check 'Associated monitoring can restart a closed coordinator.' -Fix 'Discover goals through exactly one authenticated open coordinator PR and recheck that it remains open before dispatch.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and $associatedPrFeedbackWorkflow -notmatch 'git diff --cached --name-only --no-renames') {
    Add-Failure -Check 'Associated-PR feedback change policy ignores agent-staged paths.' -Fix 'Include the cached index in protected-path and changed-file budget validation before committing.'
}
foreach ($commentWorkflow in @($sequenceGoalPrReviewWorkflow, $sequenceGoalHumanCommentWorkflow)) {
    if ($null -ne $commentWorkflow -and
        (([regex]::Matches($commentWorkflow, 'git diff --cached --name-only --no-renames')).Count -lt 4 -or
         $commentWorkflow -notmatch 'RUN_ATTEMPT: \$\{\{ github\.run_attempt \}\}' -or
         $commentWorkflow -match 'context\.runAttempt')) {
        Add-Failure -Check 'Coordinator review/comment workflow ignores staged paths or records an undefined run attempt.' -Fix 'Inspect the cached index in both policy scans and pass github.run_attempt explicitly into lifecycle evidence.'
    }
}

$verifyScopeResolver = Get-FileText -RepoRelativePath 'scripts/resolve-verify-scopes.ps1'
if ($null -ne $verifyScopeResolver -and ($verifyScopeResolver -notmatch 'IsPathRooted' -or $verifyScopeResolver -notmatch '\$_ -eq ''\.\.''')) {
    Add-Failure -Check 'Verify-scope resolver accepts rooted or traversal-based explicit changed-file paths.' -Fix 'Reject rooted paths and parent traversal segments before resolving repository workspaces.'
}

$sequenceGoalPrReviewWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml'
if ($null -ne $sequenceGoalPrReviewWorkflow) {
    if ($sequenceGoalPrReviewWorkflow -match $draftPrExclusionPattern) {
        Add-Failure -Check 'Sequence-goal PR review workflow excludes draft PRs.' -Fix 'Process every trusted open sequence-goal PR regardless of draft state.'
    }
    if ($sequenceGoalPrReviewWorkflow -notmatch 'check-mermaid-diagrams\.ps1 -Path \$reviewMarkdownFiles') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not render reviewer-generated Mermaid diagrams.' -Fix 'Run scripts/check-mermaid-diagrams.ps1 against the staged reviewer file list before scoped verification.'
    }
    if ($sequenceGoalPrReviewWorkflow -notmatch 'reviewThreads\(first: 100, after: \$threadCursor\)' -or $sequenceGoalPrReviewWorkflow -notmatch 'pageInfo \{ hasNextPage endCursor \}' -or $sequenceGoalPrReviewWorkflow -notmatch 'while \(threadCursor\)') {
        Add-Failure -Check 'Sequence-goal AI review-thread collection is not fully paginated.' -Fix 'Paginate review threads so AI feedback beyond the first GraphQL page is not missed.'
    }
    if ($sequenceGoalPrReviewWorkflow -notmatch 'sequence-goal-open-points human-comment' -or $sequenceGoalPrReviewWorkflow -notmatch 'isAiComment') {
        Add-Failure -Check 'Sequence-goal AI review loop does not exclude generated human-input questions.' -Fix 'Exclude comments carrying the sequence-goal-open-points human-comment marker from AI review classification.'
    }
    if ($sequenceGoalPrReviewWorkflow -match 'startsWith\("codex/"\)' -or $sequenceGoalPrReviewWorkflow -notmatch 'headBranch\s*!==\s*""' -or $sequenceGoalPrReviewWorkflow -notmatch 'headBranch\s*!==\s*"main"') {
        Add-Failure -Check 'Sequence-goal PR review workflow restricts delivery branches by prefix or permits main as the head branch.' -Fix 'Accept any non-empty same-repository head branch except main; do not require a codex/ prefix.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'run-sequence-goal-pr-review-agent\.ps1') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not run the PR review agent script.' -Fix 'Call scripts/run-sequence-goal-pr-review-agent.ps1 from .github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'git diff --cached --name-only --diff-filter=ACMRD' -or $sequenceGoalPrReviewWorkflow -notmatch 'resolve-verify-scopes\.ps1 -ChangedFiles \$reviewChangedFiles') {
        Add-Failure -Check 'Sequence-goal PR review workflow verifies the full delivery PR instead of reviewer-generated changes.' -Fix 'Resolve verification scopes from the staged reviewer file list so delivery-owned failures do not block review handling.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'OPENAI_API_KEY') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not pass the OpenAI API key to the review agent.' -Fix 'Set OPENAI_API_KEY from secrets.OPENAI_API_KEY in .github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'SEQUENCE_GOAL_REVIEW_AGENT_MODEL') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not define the review-agent model.' -Fix 'Set SEQUENCE_GOAL_REVIEW_AGENT_MODEL with the enrolled gpt-6-sol default.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'Discarded protected-path change from reviewer agent' -or $sequenceGoalPrReviewWorkflow -notmatch 'human maintainer must assess and apply any change') {
        Add-Failure -Check 'Sequence-goal PR review workflow lets protected-path feedback abort safe review handling.' -Fix 'Discard reviewer-agent changes to protected paths, record a human-maintainer handoff, and continue processing safe feedback.'
    }

    if ($sequenceGoalPrReviewWorkflow -match "'\^\\\.codex/agent-loop/'" -or
        $sequenceGoalPrReviewWorkflow -notmatch '\[regex\]::Escape\(\$currentEvidence\)') {
        Add-Failure -Check 'Sequence-goal PR review workflow permits reviewer changes to durable agent-loop state.' -Fix 'Allow only the immutable evidence path for the current review run under .codex/agent-loop.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'actions/setup-node@v4' -or $sequenceGoalPrReviewWorkflow -notmatch 'npm install -g @openai/codex' -or $sequenceGoalPrReviewWorkflow -notmatch 'codex --version') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not install the Codex CLI before review-agent execution.' -Fix 'Use actions/setup-node, npm install -g @openai/codex, and codex --version before running scripts/run-sequence-goal-pr-review-agent.ps1.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'resolveReviewThread') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not resolve handled AI review threads.' -Fix 'Use the GitHub GraphQL resolveReviewThread mutation after posting a [from-codex] resolution comment.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'github\.paginate\(github\.rest\.pulls\.listReviewComments' -or
        $sequenceGoalPrReviewWorkflow -notmatch 'userAuthoredThreadRoots' -or
        $sequenceGoalPrReviewWorkflow -notmatch 'getRootCommentId\(reviewThreadCommentId\)' -or
        $sequenceGoalPrReviewWorkflow -notmatch '!candidate\.containsUserComment' -or
        $sequenceGoalPrReviewWorkflow -notmatch '(?s)if \(thread\.containsUserComment\).*?continue;.*?resolveReviewThread' -or
        $sequenceGoalPrReviewWorkflow -notmatch 'assertThreadHasNoUserComment\(thread\.threadId\)') {
        Add-Failure -Check 'Sequence-goal PR review workflow can resolve a review thread containing user feedback.' -Fix 'Fully collect review comments, normalize each GraphQL-derived review-comment ID to its true root, exclude user-authored threads from AI review handling, and guard the resolution mutation against a user-authored comment.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch '\[from-codex\]') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not mark automated resolution comments.' -Fix 'Post review-thread replies containing [from-codex] before resolving AI comments.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'waiting for approval to main') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not gate completion on approval to main.' -Fix 'Fail closed while the generated sequence-goal PR has no approval for main.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'Agent fix/review resources' -or $sequenceGoalPrReviewWorkflow -notmatch 'maximumLabel' -or $sequenceGoalPrReviewWorkflow -notmatch 'actualLabel') {
        Add-Failure -Check 'Sequence-goal PR review responses omit maximum/actual fix costs and tokens.' -Fix 'Include the current review run resourceUsage labels in every AI review response.'
    }

    if ($sequenceGoalPrReviewWorkflow -match 'reviewDecision\s*===\s*"APPROVED"') {
        Add-Failure -Check 'Sequence-goal PR review workflow trusts reviewDecision for approval.' -Fix 'Gate approval only on filtered non-AI approvals so bot/AI approval cannot complete the loop.'
    }

    if ($sequenceGoalPrReviewWorkflow -match 'git status --porcelain') {
        Add-Failure -Check 'Sequence-goal PR review workflow parses git status porcelain for generated paths.' -Fix 'Use git diff --name-only --no-renames HEAD plus git ls-files --others --exclude-standard so renames/deletes are handled safely.'
    }

    if ($sequenceGoalPrReviewWorkflow -match 'git add -- \$file') {
        Add-Failure -Check 'Sequence-goal PR review workflow stages generated changes without deletions.' -Fix 'Use git add -A -- $file so deletes and rename-expanded paths are staged consistently.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch 'sequence-goal-pr-mutation-' -or $sequenceGoalPrReviewWorkflow -notmatch 'refusing to rebase stale review decisions') {
        Add-Failure -Check 'Sequence-goal PR review workflow does not serialize PR mutations or reject stale decisions.' -Fix 'Use the shared PR-number concurrency group and fail closed when the reviewed head moves.'
    }

    if ($sequenceGoalPrReviewWorkflow -notmatch '\^\\\.github/workflows/ai-delivery-sequence-goal-\[\^/\]\+\\\.yml\$' -or $sequenceGoalPrReviewWorkflow -notmatch '\^scripts/check-harness-rules\\\.ps1\$' -or $sequenceGoalPrReviewWorkflow -notmatch 'Reset-BlockedChanges -Patterns \$blockedPatterns -AllowedPatterns \$reviewableHarnessPatterns') {
        Add-Failure -Check 'Sequence-goal PR review workflow cannot safely apply valid harness feedback.' -Fix 'Allow only sequence-goal workflow files and check-harness-rules.ps1 through both protected-path decision and generated-change validation gates.'
    }
}

$sequenceGoalPrReviewScript = Get-FileText -RepoRelativePath 'scripts/run-sequence-goal-pr-review-agent.ps1'
if ($null -ne $sequenceGoalPrReviewScript) {
    if ($sequenceGoalPrReviewScript -notmatch 'foreach \(\$attempt in 1\.\.2\)' -or
        $sequenceGoalPrReviewScript -notmatch 'Combined JSONL and resource evidence were retained for diagnosis' -or
        $sequenceGoalPrReviewScript -notmatch 'Join-Path \$repoRoot \$UsagePath' -or
        $sequenceGoalPrReviewScript -notmatch 'Remove-Item -LiteralPath \$UsagePath -Force -ErrorAction SilentlyContinue') {
        Add-Failure -Check 'Sequence-goal PR-review execution does not tolerate one opaque agent-process failure.' -Fix 'Retry one non-zero Codex exit in the retained worktree and preserve combined usage evidence.'
    }

    if ($sequenceGoalPrReviewScript -notmatch 'SEQUENCE_GOAL_REVIEW_AGENT_COMMAND_JSON') {
        Add-Failure -Check 'Sequence-goal PR review agent script does not use the review-agent command secret.' -Fix 'Read SEQUENCE_GOAL_REVIEW_AGENT_COMMAND_JSON and replace PR review placeholders before launching the agent command.'
    }

    if ($sequenceGoalPrReviewScript -notmatch '\{reviewEvidence\}') {
        Add-Failure -Check 'Sequence-goal PR review agent script does not expose the review evidence placeholder.' -Fix 'Support the {reviewEvidence} placeholder so agents can read and update AI review decisions.'
    }

    if ($sequenceGoalPrReviewScript -notmatch 'SEQUENCE_GOAL_REVIEW_AGENT_MODEL' -or $sequenceGoalPrReviewScript -notmatch '\{model\}') {
        Add-Failure -Check 'Sequence-goal PR review agent script does not expose model selection.' -Fix 'Read SEQUENCE_GOAL_REVIEW_AGENT_MODEL and support the {model} placeholder.'
    }

    if ($sequenceGoalPrReviewScript -notmatch 'CODEX_API_KEY') {
        Add-Failure -Check 'Sequence-goal PR review agent script does not map API-key auth for Codex CLI.' -Fix 'Map OPENAI_API_KEY to CODEX_API_KEY before invoking codex exec so API-key automation authenticates correctly.'
    }

    if ($sequenceGoalPrReviewScript -notmatch 'sandbox_mode = "danger-full-access"' -or $sequenceGoalPrReviewScript -notmatch 'approval_policy = "never"' -or $sequenceGoalPrReviewScript -notmatch '--dangerously-bypass-approvals-and-sandbox' -or $sequenceGoalPrReviewScript -notmatch 'Normalized Codex CI launch args') {
        Add-Failure -Check 'Sequence-goal PR review agent script does not force write-capable Codex CI permissions.' -Fix 'Write an ephemeral Codex config and normalize Codex launch args to --dangerously-bypass-approvals-and-sandbox before invoking codex exec in the isolated GitHub Actions runner.'
    }

    if ($sequenceGoalPrReviewScript -notmatch 'sequence-diagram status' -or $sequenceGoalPrReviewScript -notmatch 'check-sequence-diagram-status\.ps1' -or $sequenceGoalPrReviewScript -notmatch 'check-behavior-harness\.ps1') {
        Add-Failure -Check 'Sequence-goal PR review agent lacks the atomic diagram-status contract.' -Fix 'Append instructions requiring diagram metadata, index, and behavior-harness updates plus both consistency checks.'
    }

    if ($sequenceGoalPrReviewScript -notmatch '\$suffix\.Count\s+-eq\s+0' -or
        $sequenceGoalPrReviewScript -notmatch 'IsNullOrWhiteSpace\(\$suffix\[-1\]\)' -or
        $sequenceGoalPrReviewScript -notmatch '\$suffix\[-1\]\.StartsWith\(') {
        Add-Failure -Check 'Sequence-goal PR review agent can mistake injected option values for a prompt.' -Fix 'Validate that the normalized post-exec suffix contains a non-empty positional prompt before injecting model options.'
    }
}

$sequenceGoalHumanCommentWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml'
$sequenceGoalEvidenceFinalizer = Get-FileText -RepoRelativePath 'scripts/finalize-sequence-goal-pr-evidence.ps1'
if ($null -ne $sequenceGoalPrReviewWorkflow -and
    $null -ne $sequenceGoalHumanCommentWorkflow -and
    ($sequenceGoalPrReviewWorkflow -notmatch 'finalize-sequence-goal-pr-evidence\.ps1' -or
     $sequenceGoalHumanCommentWorkflow -notmatch 'finalize-sequence-goal-pr-evidence\.ps1' -or
     $sequenceGoalPrReviewWorkflow -notmatch '(?s)Persist handled review evidence.*?-RetainEvidence.*?Resolve handled AI review threads' -or
     $sequenceGoalHumanCommentWorkflow -notmatch '(?s)Persist handled human-comment evidence.*?-RetainEvidence.*?Reply to human comments without resolving' -or
     $sequenceGoalPrReviewWorkflow -notmatch "git add -A -- docs/sequence-diagrams '\$\{\{ steps\.state\.outputs\.evidence_path \}\}'" -or
     $sequenceGoalHumanCommentWorkflow -notmatch "git add -A -- docs/sequence-diagrams '\$\{\{ steps\.comments\.outputs\.evidence_path \}\}'" -or
     $sequenceGoalPrReviewWorkflow -notmatch 'immutable_sha256' -or
     $sequenceGoalPrReviewWorkflow -notmatch $normalizedImmutableSha256Contract -or
     $sequenceGoalPrReviewWorkflow -notmatch 'Agent changed workflow-owned AI review thread identity fields' -or
     $sequenceGoalHumanCommentWorkflow -notmatch 'immutable_sha256' -or
     $sequenceGoalHumanCommentWorkflow -notmatch $normalizedImmutableSha256Contract -or
     $sequenceGoalHumanCommentWorkflow -notmatch 'Agent changed workflow-owned human-comment identity fields')) {
    Add-Failure -Check 'Coordinator feedback workflows do not persist lifecycle evidence before acknowledging feedback.' -Fix 'Aggregate and commit review/comment evidence into the coordinator goal before responding or resolving, while retaining runner-local evidence for response composition.'
}
if ($null -ne $sequenceGoalHumanCommentWorkflow -and
    ($sequenceGoalHumanCommentWorkflow -notmatch '\[regex\]::Escape\(\$currentEvidence\)' -or
     $sequenceGoalHumanCommentWorkflow -notmatch '\$_ -match ''\^\\\.codex/agent-loop/'' -and \$_ -cne \$currentEvidence')) {
    Add-Failure -Check 'Sequence-goal human-comment workflow permits agent changes to durable lifecycle state.' -Fix 'Restore every agent-loop path except the exact current evidence file and allowlist only that escaped path.'
}
if ($null -ne $sequenceGoalEvidenceFinalizer -and
    ($sequenceGoalEvidenceFinalizer -notmatch '\[Parameter\(Mandatory\)\]\[string\]\$EvidencePath' -or
     $sequenceGoalEvidenceFinalizer -notmatch '\$currentLifecycle' -or
     $sequenceGoalEvidenceFinalizer -notmatch '\$canonicalPlaceholders' -or
     $sequenceGoalEvidenceFinalizer -notmatch '-cnotin \$canonicalPlaceholders' -or
     $sequenceGoalEvidenceFinalizer -notmatch 'Only canonical pre-completion placeholders may be initialized' -or
     $sequenceGoalEvidenceFinalizer -match 'Get-ChildItem .*pr-reviews.*human-comments')) {
    Add-Failure -Check 'Sequence-goal evidence finalization can reprocess old batches or overwrite cumulative lifecycle usage.' -Fix 'Consume only the current immutable evidence path and add its usage to the existing lifecycle total.'
}

if ($null -ne $sequenceGoalHumanCommentWorkflow) {
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'sequence-goal-response:\$\{kind\}:\$\{id\}' -or
        $sequenceGoalHumanCommentWorkflow -notmatch 'isCodexResponseFor\(candidate, "issue-comment"' -or
        $sequenceGoalHumanCommentWorkflow -notmatch 'isCodexResponseFor\(candidate, "review-summary"' -or
        $sequenceGoalHumanCommentWorkflow -notmatch 'otherEvidence') {
        Add-Failure -Check 'Sequence-goal human-comment workflow can confuse unrelated bot replies or mutate historical evidence.' -Fix 'Correlate every response to its exact source item and restore every historical evidence file except the current workflow-owned file.'
    }
    if ($sequenceGoalHumanCommentWorkflow -match $draftPrExclusionPattern) {
        Add-Failure -Check 'Sequence-goal human-comment workflow excludes draft PRs.' -Fix 'Process every trusted open sequence-goal PR regardless of draft state.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'trustedResponseAuthors\.has\(login\)') {
        Add-Failure -Check 'Sequence-goal human-comment collection trusts an unverified response marker.' -Fix 'Accept correlated Codex responses only from the configured AI or automation identities.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'check-mermaid-diagrams\.ps1 -Path \$humanCommentMarkdownFiles') {
        Add-Failure -Check 'Sequence-goal human-comment workflow does not render agent-generated Mermaid diagrams.' -Fix 'Run scripts/check-mermaid-diagrams.ps1 against the staged human-comment file list before scoped verification.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'sequence-goal-open-points human-comment' -or $sequenceGoalHumanCommentWorkflow -notmatch 'body\.includes\(generatedHumanCommentMarker\)') {
        Add-Failure -Check 'Sequence-goal human comment loop does not include generated open-point questions.' -Fix 'Treat the sequence-goal-open-points human-comment marker as human input even when the GitHub App authored the question.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'github\.paginate\(github\.rest\.issues\.listComments' -or $sequenceGoalHumanCommentWorkflow -notmatch 'github\.paginate\(github\.rest\.pulls\.listReviews' -or $sequenceGoalHumanCommentWorkflow -notmatch 'pageInfo \{ hasNextPage endCursor \}' -or $sequenceGoalHumanCommentWorkflow -notmatch 'while \(threadCursor\)') {
        Add-Failure -Check 'Sequence-goal human comment collection is not fully paginated.' -Fix 'Paginate PR issue comments, reviews, and review threads so feedback beyond the first API page is not missed.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch '!body\.includes\("\[from-codex\]"\)\s*&&\s*\(body\.includes\(generatedHumanCommentMarker\)') {
        Add-Failure -Check 'Generated human-comment marker can override the [from-codex] response exclusion.' -Fix 'Apply the [from-codex] exclusion before marker-based human-comment classification to prevent self-reprocessing.'
    }
    if ($sequenceGoalHumanCommentWorkflow -match 'startsWith\("codex/"\)' -or $sequenceGoalHumanCommentWorkflow -notmatch 'headBranch\s*!==\s*""' -or $sequenceGoalHumanCommentWorkflow -notmatch 'headBranch\s*!==\s*"main"') {
        Add-Failure -Check 'Sequence-goal human comment workflow restricts delivery branches by prefix or permits main as the head branch.' -Fix 'Accept any non-empty same-repository head branch except main; do not require a codex/ prefix.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'run-sequence-goal-human-comment-agent\.ps1') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not run the human comment agent script.' -Fix 'Call scripts/run-sequence-goal-human-comment-agent.ps1 from .github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'git diff --cached --name-only --diff-filter=ACMRD' -or $sequenceGoalHumanCommentWorkflow -notmatch 'resolve-verify-scopes\.ps1 -ChangedFiles \$humanCommentChangedFiles') {
        Add-Failure -Check 'Sequence-goal human comment workflow verifies the full delivery PR instead of human-comment agent changes.' -Fix 'Resolve verification scopes from the staged human-comment file list so delivery-owned failures do not block comment handling.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'OPENAI_API_KEY') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not pass the OpenAI API key to the comment agent.' -Fix 'Set OPENAI_API_KEY from secrets.OPENAI_API_KEY in .github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not define the human-comment agent model.' -Fix 'Set SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL with the enrolled gpt-6-sol default.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'Discarded protected-path change from human-comment agent' -or $sequenceGoalHumanCommentWorkflow -notmatch 'human maintainer must assess and apply any change') {
        Add-Failure -Check 'Sequence-goal human-comment workflow lets protected-path feedback abort safe comment handling.' -Fix 'Discard comment-agent changes to protected paths, record a human-maintainer handoff, and continue processing safe feedback.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'actions/setup-node@v4' -or $sequenceGoalHumanCommentWorkflow -notmatch 'npm install -g @openai/codex' -or $sequenceGoalHumanCommentWorkflow -notmatch 'codex --version') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not install the Codex CLI before human-comment agent execution.' -Fix 'Use actions/setup-node, npm install -g @openai/codex, and codex --version before running scripts/run-sequence-goal-human-comment-agent.ps1.'
    }

    if ($sequenceGoalHumanCommentWorkflow -match 'resolveReviewThread') {
        Add-Failure -Check 'Sequence-goal human comment workflow resolves human review threads.' -Fix 'Remove resolveReviewThread from the human comment workflow; human/non-AI threads must remain unresolved for human closure.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch '\[from-codex\]') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not mark automated response comments.' -Fix 'Post human-comment replies containing [from-codex].'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'intentionally not resolved') {
        Add-Failure -Check 'Sequence-goal human comment workflow does not state that human threads stay unresolved.' -Fix 'Include an explicit response note that human/non-AI review threads are intentionally not resolved by the agent loop.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'Agent fix/comment resources' -or $sequenceGoalHumanCommentWorkflow -notmatch 'maximumLabel' -or $sequenceGoalHumanCommentWorkflow -notmatch 'actualLabel') {
        Add-Failure -Check 'Sequence-goal human-comment responses omit maximum/actual fix costs and tokens.' -Fix 'Include the current comment-handling run resourceUsage labels in every response.'
    }

    if ($sequenceGoalHumanCommentWorkflow -match 'git status --porcelain') {
        Add-Failure -Check 'Sequence-goal human comment workflow parses git status porcelain for generated paths.' -Fix 'Use git diff --name-only --no-renames HEAD plus git ls-files --others --exclude-standard so renames/deletes are handled safely.'
    }

    if ($sequenceGoalHumanCommentWorkflow -match 'git add -- \$file') {
        Add-Failure -Check 'Sequence-goal human comment workflow stages generated changes without deletions.' -Fix 'Use git add -A -- $file so deletes and rename-expanded paths are staged consistently.'
    }

    if ($sequenceGoalHumanCommentWorkflow -notmatch 'sequence-goal-pr-mutation-' -or $sequenceGoalHumanCommentWorkflow -notmatch 'refusing to rebase stale comment decisions') {
        Add-Failure -Check 'Sequence-goal human-comment workflow does not serialize PR mutations or reject stale decisions.' -Fix 'Use the shared PR-number concurrency group and fail closed when the reviewed head moves.'
    }
}

$sequenceGoalHumanCommentScript = Get-FileText -RepoRelativePath 'scripts/run-sequence-goal-human-comment-agent.ps1'
if ($null -ne $sequenceGoalHumanCommentScript) {
    if ($sequenceGoalHumanCommentScript -notmatch 'foreach \(\$attempt in 1\.\.2\)' -or
        $sequenceGoalHumanCommentScript -notmatch 'Combined JSONL and resource evidence were retained for diagnosis' -or
        $sequenceGoalHumanCommentScript -notmatch 'Join-Path \$repoRoot \$UsagePath' -or
        $sequenceGoalHumanCommentScript -notmatch 'Remove-Item -LiteralPath \$UsagePath -Force -ErrorAction SilentlyContinue') {
        Add-Failure -Check 'Sequence-goal human-comment execution does not tolerate one opaque agent-process failure.' -Fix 'Retry one non-zero Codex exit in the retained worktree and preserve combined usage evidence.'
    }
    if ($sequenceGoalHumanCommentScript -notmatch 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON') {
        Add-Failure -Check 'Sequence-goal human comment agent script does not use the human-comment command secret.' -Fix 'Read SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON and replace comment placeholders before launching the agent command.'
    }

    if ($sequenceGoalHumanCommentScript -notmatch '\{commentEvidence\}') {
        Add-Failure -Check 'Sequence-goal human comment agent script does not expose the comment evidence placeholder.' -Fix 'Support the {commentEvidence} placeholder so agents can read and update non-AI comment decisions.'
    }

    if ($sequenceGoalHumanCommentScript -notmatch 'SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL' -or $sequenceGoalHumanCommentScript -notmatch '\{model\}') {
        Add-Failure -Check 'Sequence-goal human comment agent script does not expose model selection.' -Fix 'Read SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL and support the {model} placeholder.'
    }

    if ($sequenceGoalHumanCommentScript -notmatch 'CODEX_API_KEY') {
        Add-Failure -Check 'Sequence-goal human comment agent script does not map API-key auth for Codex CLI.' -Fix 'Map OPENAI_API_KEY to CODEX_API_KEY before invoking codex exec so API-key automation authenticates correctly.'
    }

    if ($sequenceGoalHumanCommentScript -notmatch 'sandbox_mode = "danger-full-access"' -or $sequenceGoalHumanCommentScript -notmatch 'approval_policy = "never"' -or $sequenceGoalHumanCommentScript -notmatch '--dangerously-bypass-approvals-and-sandbox' -or $sequenceGoalHumanCommentScript -notmatch 'Normalized Codex CI launch args') {
        Add-Failure -Check 'Sequence-goal human comment agent script does not force write-capable Codex CI permissions.' -Fix 'Write an ephemeral Codex config and normalize Codex launch args to --dangerously-bypass-approvals-and-sandbox before invoking codex exec in the isolated GitHub Actions runner.'
    }
}

$sequenceGoalPrFinalizerWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml'
if ($null -ne $sequenceGoalPrFinalizerWorkflow) {
    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'github\.rest\.pulls\.listFiles' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'github\.rest\.pulls\.listCommits' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'closingCommitShas\.has' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'hasQaRows' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'PR lacks the sequence-goal coordinator marker' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'identity or schema does not match the closing coordinator PR' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'replayWithoutTransientEvidence' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'git add -A -- docs/sequence-diagrams\.md docs/sequence-diagrams') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not bind writes and evidence to durable PR identity or replay safely.' -Fix 'Resolve diagrams from PR changed files, validate evidence against the closing PR/head ancestry, preserve archived totals on replay, and stage the lifecycle index update.'
    }
    if ($sequenceGoalHumanCommentWorkflow -notmatch 'PR became terminal or changed head before human-comment evidence finalization') {
        Add-Failure -Check 'Sequence-goal human-comment workflow can publish lifecycle evidence after the PR becomes terminal.' -Fix 'Revalidate the live open PR and exact head immediately before finalizing evidence.'
    }
    if ($sequenceGoalPrFinalizerWorkflow -match 'startsWith\("codex/"\)' -or $sequenceGoalPrFinalizerWorkflow -notmatch 'headBranch\s*===\s*""' -or $sequenceGoalPrFinalizerWorkflow -notmatch 'headBranch\s*===\s*"main"') {
        Add-Failure -Check 'Sequence-goal PR finalizer restricts delivery branches by prefix or permits main as the head branch.' -Fix 'Accept any non-empty same-repository head branch except main; do not require a codex/ prefix.'
    }

    if ($sequenceGoalPrFinalizerWorkflow -notmatch '(?s)pull_request:\s*\r?\n\s+types:\s*\[closed\]') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not run on closed pull requests.' -Fix 'Trigger .github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml on pull_request types [closed].'
    }

    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'Agent Loop Closure Evidence') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not archive closure evidence into goal documents.' -Fix 'Write an Agent Loop Closure Evidence section into delivered docs/sequence-diagrams/*.md files.'
    }

    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'coordinatorRow' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch '"Status", "`InProgress`"' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'delivery remains incomplete') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not record coordinator abandonment.' -Fix 'Update the Coordinator row and keep the goal InProgress when its coordinator PR closes without merge.'
    }

    foreach ($lifecycleCostToken in @('Lifecycle agent resources', 'Delivery agent resources', 'AI review agent resources', 'Human comment agent resources', 'Total lifecycle agent resources')) {
        if ($sequenceGoalPrFinalizerWorkflow -notmatch [regex]::Escape($lifecycleCostToken)) {
            Add-Failure -Check "Sequence-goal PR finalizer does not record lifecycle cost token: $lifecycleCostToken" -Fix 'Aggregate delivery, AI-review, and human-comment resource evidence into the goal diagram at PR closure.'
        }
    }
    if ($sequenceGoalPrFinalizerWorkflow -match 'Number\.EPSILON' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch '\.toFixed\(2\)') {
        Add-Failure -Check 'Sequence-goal lifecycle cost reporting can inflate exact-cent values or omit two-decimal formatting.' -Fix 'Ceil only sub-cent remainders and render every lifecycle USD total with exactly two decimal places.'
    }

    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'const finalState = pr\.merged \? "Merged" : "Abandoned"' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'let targetBranch = pr\.merged \? "main" : pr\.head\.ref' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'steps\.commit-finalizer\.outcome == ''success''') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not fail closed for merged and abandoned coordinator lifecycle evidence.' -Fix 'Record merged evidence on main, abandoned evidence on the source branch, and comment only after the lifecycle commit succeeds.'
    }

    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'pr-reviews' -or $sequenceGoalPrFinalizerWorkflow -notmatch 'human-comments') {
        Add-Failure -Check 'Sequence-goal PR finalizer does not consume transient review/comment evidence.' -Fix 'Summarize .codex/agent-loop/pr-reviews and human-comments evidence into the goal document before removing transient files.'
    }
    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'Delivery: Coordinator' -or
        $sequenceGoalWorkflow -notmatch 'Write-GoalTaskState') {
        Add-Failure -Check 'Coordinator state or finalizer evidence is not bound to the goal document.' -Fix 'Write coordinator progress and read finalizer evidence from the goal document.'
    }
    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'throw new Error\(`Malformed lifecycle evidence') {
        Add-Failure -Check 'Sequence-goal PR finalizer silently discards malformed lifecycle evidence.' -Fix 'Fail closed when a review or human-comment evidence file cannot be parsed.'
    }
    if ($sequenceGoalPrFinalizerWorkflow -notmatch 'group:.*sequence-goal-pr-mutation-\{0\}' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'core\.setOutput\("ref", pr\.head\.sha\)' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'Coordinator branch moved after closure' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'removedEvidencePaths' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'evidenceModifyingCommits' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'unappliedAiEvidence' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'sequence-goal-evidence-batch:' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'expectedSurfaces' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'await github\.rest\.pulls\.get' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'trusted .* identity contract' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'Main changed during finalization; rerun to validate' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'trustedAuthors\.has\(commits\[0\]\.committer\?\.login' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'childPr\.html_url !== row\.url' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'diagrams.length === 1' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'fs\.existsSync\(diagrams' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'hasExactLine' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'no available delivered goal diagram' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch '\+refs/heads/\$\{pr\.head\.ref\}:refs/remotes/origin/\$\{pr\.head\.ref\}' -or
        $sequenceGoalPrFinalizerWorkflow -match 'retainedLifecycle \|\|') {
        Add-Failure -Check 'Sequence-goal PR finalizer can consume mutable closure state, trust ledger state, or double-count lifecycle resources.' -Fix 'Pin abandoned finalization to the close-event SHA, reject untrusted post-close movement, resolve every expected child PR before completion, and calculate lifecycle totals from the delivery baseline plus retained evidence exactly once.'
    }

    if ($sequenceGoalPrFinalizerWorkflow -match 'deleteRef|git push origin --delete|gh api.*git/refs') {
        Add-Failure -Check 'Sequence-goal PR finalizer attempts to delete delivery branches.' -Fix 'Remove branch deletion from the finalizer; GitHub repository settings handle branch deletion after merge.'
    }
    if ($sequenceGoalPrFinalizerWorkflow -notmatch '\.codex/agent-loop/abandonments/pr-' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'id: checkout-ref' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'ref: \$\{\{ steps\.checkout-ref\.outputs\.ref \}\}' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'SOURCE_DELETED_AT_CHECKOUT:.*steps\.checkout-ref\.outputs\.source_deleted' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'checkout.*origin/main' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'core\.setOutput\("standalone_archive", archivePath\)' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'core\.setOutput\("should_commit", String\(Boolean\(changed\)\)\)' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'sequence-goal-finalizer:' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'issues\.updateComment' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'Preserving existing standalone abandonment archive' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'Write-StandaloneAbandonmentArchive' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'Failed to stage standalone abandonment evidence' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'three lease-protected main refreshes' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'git ls-remote --exit-code --heads origin' -or
        $sequenceGoalPrFinalizerWorkflow -notmatch 'deletedBeforeFinalization: true') {
        Add-Failure -Check 'Sequence-goal PR finalizer loses abandonment evidence when the coordinator branch was deleted before finalization.' -Fix 'Switch to the current main ref and retain a sanitized standalone abandonment record without recreating or importing the deleted branch.'
    }
}

if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -match 'runs/resumed-\$\{\{ github\.run_id \}\}' -or
     $sequenceGoalWorkflow -notmatch 'Read coordinator state from goal document' -or
     $sequenceGoalWorkflow -notmatch 'Get-GoalCoordinatorState' -or
     $sequenceGoalWorkflow -notmatch '\$state\.branch -cne \$env:GITHUB_REF_NAME' -or
     $sequenceGoalWorkflow -notmatch '\$state\.repository -cne \$env:GITHUB_REPOSITORY' -or
     $sequenceGoalWorkflow -notmatch 'Goal document does not belong to this coordinator' -or
     $sequenceGoalWorkflow -notmatch 'steps\.evidence\.outputs\.goal_document')) {
    Add-Failure -Check 'Sequence-goal restart does not read the current goal document.' -Fix 'Read Coordinator state from the goal document and validate its repository and branch.'
}
if ($null -ne $sequenceGoalScript -and
    ($sequenceGoalScript -notmatch 'function Test-RunnableScenarioBlock' -or
     $sequenceGoalScript -notmatch 'Examples' -or
     $sequenceGoalScript -notmatch 'function Test-RunnableTestMethodMatch' -or
     $sequenceGoalScript -notmatch 'InlineData\|MemberData\|ClassData')) {
    Add-Failure -Check 'Sequence-goal automation evidence accepts data-less theories or Scenario Outlines.' -Fix 'Require a supported theory data attribute and at least one concrete Examples row before treating parameterized automation as executable.'
}
if ($null -ne $sequenceGoalScript -and
    ($sequenceGoalScript -notmatch '\(\?:Given\|When\|Then\|And\|But\)' -or
     $sequenceGoalScript -match '\(\?:Given\|When\|Then\|And\|But\|\\\*\)')) {
    Add-Failure -Check 'Sequence-goal automation evidence accepts step-free or unsupported-star Gherkin scenarios.' -Fix 'Match the checked-in feature runner by requiring an executable Given/When/Then/And/But step.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'Initialize coordinator state in goal document' -or
     $sequenceGoalWorkflow -match '\.Substring\(0, \[Math\]::Min\(120, \$branch(?:Name)?\.Length\)\)')) {
    Add-Failure -Check 'Sequence-goal branch-key truncation uses the unsanitized branch length.' -Fix 'Sanitize first and bound Substring with the sanitized prefix length at every branch-key producer.'
}
if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch '\[string\[\]\]\$draftArguments = @\(''--draft''\)' -or
     $sequenceGoalWorkflow -notmatch 'gh pr create .* @draftArguments \| Out-Null' -or
     $sequenceGoalWorkflow -notmatch '\$createdPrLookupAttempts = 6' -or
     $sequenceGoalWorkflow -notmatch 'Start-Sleep -Seconds 2')) {
    Add-Failure -Check 'Associated production PR creation can race GitHub discovery or Mobile iOS delivery can merge before inferential review.' -Fix 'Retry the authenticated post-create PR lookup and create new Mobile iOS delivery PRs as drafts.'
}
$coordinatorOwnedPathPatternToken = '$coordinatorOwnedPathPattern = ''^(?:\.codex/agent-loop/|docs/sequence-diagrams(?:\.md$|/)|docs/behavior-harness\.md$)'''
if ($null -ne $sequenceGoalWorkflow -and $null -ne $sequenceGoalScript -and
    ($sequenceGoalWorkflow -notmatch 'coordinator-owned goal or evidence paths' -or
     -not $sequenceGoalWorkflow.Contains($coordinatorOwnedPathPatternToken) -or
     $sequenceGoalWorkflow -notmatch '\$coordinatorChanges = @\(\$changedPaths \| Where-Object \{ \$_ -match \$coordinatorOwnedPathPattern \}\)' -or
     $sequenceGoalWorkflow -notmatch '\$headBeforeProductionAgent = \(git rev-parse HEAD\)\.Trim\(\)' -or
     $sequenceGoalWorkflow -notmatch 'git diff --name-only --no-renames \$headBeforeProductionAgent HEAD' -or
     $sequenceGoalWorkflow -notmatch 'git diff --name-only --no-renames "origin/\$\(\$env:DELIVERY_BASE_BRANCH\)\.\.\.HEAD"' -or
     $sequenceGoalWorkflow -notmatch '\$changedPaths = @\(\$baseBranchPaths; \$agentCommitPaths; \$workingTreePaths; \$untrackedPaths\)' -or
     $sequenceGoalWorkflow -notmatch 'Failed to inspect commits created during production agent execution' -or
     $sequenceGoalWorkflow -notmatch 'Production agents must leave changes uncommitted' -or
     $sequenceGoalWorkflow -notmatch 'Production agent changes cancel to an empty final tree' -or
     $sequenceGoalWorkflow -notmatch 'Production agent changed blocked path' -or
     $sequenceGoalScript -notmatch 'read-only coordinator inputs' -or
     $sequenceGoalScript -notmatch 'Never update their status, evidence, lifecycle, or tracking fields from a production branch' -or
     ([regex]::Matches($sequenceGoalScript, 'Leave changes unstaged\. Do not run `git add`, modify the index')).Count -ne 2 -or
     $sequenceGoalScript -notmatch 'Leave all target-repository changes uncommitted')) {
    Add-Failure -Check 'Backend production or remediation can write coordinator-owned goal and lifecycle evidence.' -Fix 'Keep coordinator goal documents read-only in production guidance and reject their paths before committing a Backend child PR.'
}
if ($null -ne $associatedPrFeedbackWorkflow -and
    (-not $associatedPrFeedbackWorkflow.Contains($coordinatorOwnedPathPatternToken) -or
     $associatedPrFeedbackWorkflow -notmatch '\$evidence\.surface -eq ''Backend''' -or
     $associatedPrFeedbackWorkflow -notmatch '\$changes = @\(\$baseBranchPaths; \$stagedPaths; \$workingTreePaths; \$untrackedPaths\)' -or
     $associatedPrFeedbackWorkflow -notmatch '\$coordinatorChanges = @\(\$substantive .*?\$coordinatorOwnedPathPattern' -or
     $associatedPrFeedbackWorkflow -notmatch 'Feedback agent committed workflow-owned evidence' -or
     $associatedPrFeedbackWorkflow -notmatch 'git cat-file -e "HEAD:\$allowedEvidencePath"' -or
     $associatedPrFeedbackWorkflow -notmatch 'Feedback agent committed workflow-owned evidence in HEAD' -or
     $associatedPrFeedbackWorkflow -notmatch 'Backend associated feedback changed coordinator-owned goal or evidence paths')) {
    Add-Failure -Check 'Backend associated-PR feedback can mutate coordinator-owned goal or lifecycle evidence.' -Fix 'Apply the coordinator-owned path filter to the complete substantive Backend feedback diff before commit.'
}

$sequenceGoalCommentMonitorWorkflow = Get-FileText -RepoRelativePath '.github/workflows/ai-delivery-sequence-goal-comment-monitor.yml'
if ($null -ne $sequenceGoalCommentMonitorWorkflow) {
    if ($sequenceGoalCommentMonitorWorkflow -match 'startsWith\("codex/"\)' -or $sequenceGoalCommentMonitorWorkflow -notmatch '\(pullRequest\.head\?\.ref\s*\|\|\s*""\)\s*!==\s*""' -or $sequenceGoalCommentMonitorWorkflow -notmatch 'pullRequest\.head\.ref\s*!==\s*"main"') {
        Add-Failure -Check 'Sequence-goal comment monitor restricts delivery branches by prefix or permits main as the head branch.' -Fix 'Monitor any non-empty same-repository head branch except main; do not require a codex/ prefix.'
    }
}

$sequenceTemplate = Get-FileText -RepoRelativePath 'docs/sequence-diagrams/_template.sequence-diagram.md'
if ($null -ne $sequenceTemplate) {
    if ($sequenceTemplate -notmatch '## Agent Loop Goal') {
        Add-Failure -Check 'Sequence diagram template is missing Agent Loop Goal section.' -Fix 'Ensure docs/sequence-diagrams/_template.sequence-diagram.md includes the Agent Loop Goal table.'
    }

    if ($sequenceTemplate -notmatch '## Sequence \(detailed\)') {
        Add-Failure -Check 'Sequence diagram template is missing the detailed sequence section.' -Fix 'Add a Mermaid sequence block under a \"## Sequence (detailed)\" heading.'
    }

    if ($sequenceTemplate -notmatch '## Agent delivery constraints') {
        Add-Failure -Check 'Sequence diagram template is missing Agent delivery constraints.' -Fix 'Add a section for target paths, contract names, configuration, and harness commands.'
    }

    if ($sequenceTemplate -notmatch '## UX contract' -or
        $sequenceTemplate -notmatch 'Implementation authority' -or
        $sequenceTemplate -notmatch 'Not applicable` when Supplied UX is `None`; otherwise `Direct instruction' -or
        $sequenceTemplate -notmatch 'Direct instruction' -or
        $sequenceTemplate -notmatch '(?m)^\| Flow and behaviour authority \| `Sequence \(detailed\)` \|\r?$' -or
        $sequenceTemplate -notmatch 'Screen/state mapping' -or
        $sequenceTemplate -notmatch 'Design inspection matrix' -or
        $sequenceTemplate -notmatch 'Visual verification' -or
        $sequenceTemplate -notmatch 'UI/UX acceptance test' -or
        $sequenceTemplate -notmatch 'Rendered evidence' -or
        $sequenceTemplate -notmatch 'Comparison result') {
        Add-Failure -Check 'Sequence diagram template is missing the authoritative UI/UX acceptance contract.' -Fix 'Capture supplied UX, direct authority, sequence flow ownership, screen/state mapping, a design inspection matrix, visual verification, executable UI/UX acceptance test, rendered evidence, and a comparison result.'
    }

    if ($sequenceTemplate -notmatch "default unstyled presentation" -or
        $sequenceTemplate -notmatch 'without background or fill colours, borders, boundary boxes or containers' -or
        $sequenceTemplate -notmatch 'actual project names where applicable') {
        Add-Failure -Check 'Sequence diagram template is missing presentation and resource-labelling guidance.' -Fix 'Add the unstyled, boundary-free, project-labelled diagram contract to the template.'
    }

    if ($sequenceTemplate -match '(?m)^\|\s*(?:Maximum agent resources|Production code agent resources|Automation code agent resources|Actual agent resources|Lifecycle agent resources)\s*\|') {
        Add-Failure -Check 'Sequence diagram template includes agent resource placeholders that are generated at completion.' -Fix 'Remove resource placeholder rows from the Draft template; completion and PR-closure workflows populate them from retained evidence.'
    }
}

if ($null -ne $sequenceGoalQaWorkflow -and
    ($sequenceGoalQaWorkflow -notmatch 'REQUESTED_SURFACE: \$\{\{ inputs\.surface \}\}' -or
     $sequenceGoalQaWorkflow -notmatch 'RequestedSurface \$env:REQUESTED_SURFACE' -or
     $sequenceGoalQaWorkflow -notmatch 'QA coordinator branch cannot be main' -or
     $sequenceGoalQaWorkflow -notmatch "matrix\.surface != 'Backend' && steps\.app-credentials\.outputs\.configured == 'true'" -or
     ([regex]::Matches($sequenceGoalQaWorkflow, "id: app-token\s*\r?\n\s*if: steps\.app-credentials\.outputs\.configured == 'true'")).Count -ne 2)) {
    Add-Failure -Check 'Dedicated QA workflow does not bind dispatch inputs safely or conditionally create cross-repository credentials.' -Fix 'Pass the requested surface through an environment variable, reject main as a QA coordinator branch, and create each App token only when credentials are configured.'
}

$costControlledReviewWorkflows = @(
    Get-FileText -RepoRelativePath '.github/workflows/ai-harness-high-risk-autolabel.yml'
    Get-FileText -RepoRelativePath '.github/workflows/ai-harness-high-risk-coverage.yml'
    Get-FileText -RepoRelativePath '.github/workflows/ai-harness-mermaid-validation.yml'
)
if (($sequenceGoalWorkflow -match '(?m)^\s+- (?:docs/agent-loop-workflow|scripts/|\.github/workflows/)') -or
    ([regex]::Matches($sequenceGoalWorkflow, 'queue: max')).Count -lt 3 -or
    (Get-FileText -RepoRelativePath 'scripts/save-sequence-goal-resources.ps1') -notmatch 'Update-SequenceGoalDocument' -or
    $sequenceGoalQaWorkflow -notmatch '(?s)group: sequence-goal-qa-.*?cancel-in-progress: false' -or
    $sequenceGoalWorkflow -notmatch '(?s)- name: Dispatch missing QA surface.*?dispatch-sequence-goal-qa\.ps1' -or
    (Get-FileText -RepoRelativePath 'scripts/dispatch-sequence-goal-qa.ps1') -notmatch 'Matching QA is active; reusing its run' -or
    @($costControlledReviewWorkflows | Where-Object { $_ -notmatch '(?s)concurrency:.*?cancel-in-progress: true' }).Count -gt 0) {
    Add-Failure -Check 'GitHub Actions cost controls no longer suppress redundant runs while preserving durable writes.' -Fix 'Trigger delivery only for diagram changes, queue every ledger writer, skip active QA recovery without cancelling generation, and cancel superseded PR validation runs.'
}

if ($null -ne $sequenceGoalWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'retain-agent-resource-evidence:.*(?s).*needs: \[resolve-delivery-agents, open-coordinator-pr, deliver-cross-repository-production\]' -or
     $sequenceGoalWorkflow -notmatch 'complete-goal:.*(?s).*needs: \[resolve-delivery-agents, run-sequence-loop, open-coordinator-pr, deliver-cross-repository-production, retain-agent-resource-evidence\]' -or
     $sequenceGoalWorkflow -notmatch "github\.event_name == 'workflow_dispatch' && inputs\.smoke_test == true \|\| needs\.open-coordinator-pr\.result == 'success'" -or
     ([regex]::Matches($sequenceGoalWorkflow, "github\.event_name != 'workflow_dispatch' \|\| inputs\.smoke_test != true.*steps\.complete\.outputs\.has_publishable")).Count -ne 2)) {
    Add-Failure -Check 'Sequence-goal coordinator ordering or smoke finalization can race the draft PR publisher.' -Fix 'Wait for the draft coordinator PR before branch writes or completion, and skip coordinator-PR finalization during smoke runs.'
}

$resourceWriter = Get-FileText -RepoRelativePath 'scripts/save-sequence-goal-resources.ps1'
if ($resourceWriter -notmatch 'Executed agent has no budget evidence' -or $resourceWriter -notmatch 'Update-SequenceGoalDocument') {
    Add-Failure -Check 'Goal resource recording lost required evidence or retry protection.' -Fix 'Validate executed-agent costs and use the shared document updater.'
}

if ($null -ne $sequenceGoalWorkflow -and $null -ne $sequenceGoalQaWorkflow -and
    ($sequenceGoalWorkflow -notmatch 'Production agent modified the GitHub Actions output command file' -or
     $sequenceGoalWorkflow -notmatch '\$attempts\.Count -ne \$expectedCount' -or
     $sequenceGoalWorkflow -notmatch '\$attemptSurfaces\.Count -ne \$expectedCount' -or
     $sequenceGoalWorkflow -notmatch 'sequence-goal-production-started-\$\(\$env:DELIVERY_SURFACE\)' -or
     $sequenceGoalWorkflow -notmatch 'sequence-goal-production-started-\$\{\{ matrix\.surface \}\}' -or
     $sequenceGoalQaWorkflow -notmatch '\$diagramHash = \[Convert\]::ToHexString' -or
     $sequenceGoalQaWorkflow -notmatch '-UsagePath "TestResults/agent-resource/\$evidenceBaseName\.jsonl"' -or
     $sequenceGoalQaWorkflow -notmatch 'foreach \(\$startedMarker in \$startedMarkers\)')) {
    Add-Failure -Check 'Sequence-goal execution evidence can be forged or paired with the wrong resource budget.' -Fix 'Protect the production output command file, require every production matrix surface, and give QA an explicit marker-matched budget path.'
}

if ((Get-FileText -RepoRelativePath 'scripts/open-sequence-goal-coordinator-pr.ps1') -notmatch 'after implementation verification') {
    Add-Failure -Check 'The early coordinator PR is not recognizable by the associated-PR monitor.' -Fix 'Include the monitor-compatible coordinator marker in the draft PR body.'
}

if ($null -ne $sequenceGoalQaWorkflow -and
    ($sequenceGoalQaWorkflow -match 'recordExistingOnly|record-existing-only' -or
     $sequenceGoalQaWorkflow -notmatch 'QA agent changed paths outside its publication allowlist' -or
     $sequenceGoalQaWorkflow -notmatch 'ConvertTo-Json -InputObject @\(\$handoffEntries\)')) {
    Add-Failure -Check 'QA delivery bypasses publication checks or mixes workflow-owned state with its isolated handoff.' -Fix 'Reject record-only shortcuts, enforce the QA publication allowlist, and serialize an empty handoff as a JSON array.'
}

$coordinatorPublisher = Get-FileText -RepoRelativePath 'scripts/open-sequence-goal-coordinator-pr.ps1'
if ($coordinatorPublisher -notmatch 'after goal completion\.' -or
    $coordinatorPublisher -notmatch '"<!-- \$completionMarker -->"' -or
    $coordinatorPublisher -notmatch 'Failed to restore coordinator lifecycle marker') {
    Add-Failure -Check 'Draft coordinator publication breaks lifecycle consumer identification.' -Fix 'Retain the completion compatibility marker in new bodies and repair missing markers when reusing an authenticated coordinator.'
}
if ($null -ne $coordinatorPublisher -and
    $coordinatorPublisher -notmatch '\(\(Split-Path -Parent \$_\) -replace') {
    Add-Failure -Check 'Sequence-goal coordinator diagram path normalization can pass -replace to Split-Path.' -Fix 'Parenthesize the Split-Path result before applying the replacement operator.'
}
if ($coordinatorPublisher -notmatch 'Text\.RegularExpressions\.MatchEvaluator') {
    Add-Failure -Check 'Sequence-goal coordinator ledger replacement can interpret branch text as a regex replacement template.' -Fix 'Insert the coordinator row through a MatchEvaluator.'
}
if ($null -ne $coordinatorPublisher -and
    ($coordinatorPublisher -notmatch '"app/\$GitHubAppSlug"' -or
     $coordinatorPublisher -notmatch 'Contains\(\$monitorMarker')) {
    Add-Failure -Check 'Coordinator PR recovery does not accept every trusted App identity and marker form.' -Fix 'Accept the app/<slug> author form and the monitor-created coordinator marker when reusing a coordinator PR.'
}
if ($null -ne $associatedPrMonitorWorkflow -and ([regex]::Matches($associatedPrMonitorWorkflow, '`app/\$\{appSlug\}`')).Count -lt 3) {
    Add-Failure -Check 'Coordinator monitoring does not recognize every App author form accepted by the publisher.' -Fix 'Keep App author identity sets consistent across creation, monitoring, recovery, and completion.'
}
if ($null -ne $associatedPrMonitorWorkflow -and
    ($associatedPrMonitorWorkflow -notmatch 'SEQUENCE_GOAL_COORDINATOR_REPOSITORY: \$\{\{ github\.repository \}\}' -or
     $associatedPrMonitorWorkflow -match 'const appSlug = "\$\{\{' -or
     $associatedPrMonitorWorkflow -match 'repository: "\$\{\{ github\.repository \}\}')) {
    Add-Failure -Check 'Associated-PR monitor embeds workflow expressions in its oversized inline JavaScript.' -Fix 'Pass workflow values through step environment variables and read them through process.env.'
}
if ($null -ne $coordinatorPublisher -and $coordinatorPublisher -match 'gh pr edit \$pr\.number --repo \$Repository --title \$title --body') {
    Add-Failure -Check 'Coordinator restart overwrites its live delivery body.' -Fix 'Refresh only the title of an existing coordinator PR and preserve accumulated lifecycle context.'
}

if ($null -ne $sequenceGoalQaWorkflow -and
    ($sequenceGoalQaWorkflow -notmatch 'QA_INPROGRESS_LIST\.Split' -or
     $sequenceGoalQaWorkflow -notmatch 'save-sequence-goal-delivery\.ps1 -Evidence \$checkpoint' -or
     $sequenceGoalQaWorkflow -match 'allowedCoordinatorDocs' -or
     $sequenceGoalQaWorkflow -match '\$handoffPaths = @\(\$trackedAgentChanges; \$untrackedAgentChanges; \$workflowSummaryPath\)')) {
    Add-Failure -Check 'QA continuation mixes coordinator-only files with the isolated QA delivery handoff.' -Fix 'Pass diagram lists through environment variables, use the canonical surface for ledger dispatch discovery, and hand off only QA-owned changes.'
}

if ($goalDocumentPublisher -notmatch 'record-sequence-goal-associated-prs.ps1' -or
    $goalDocumentUpdater -notmatch 'for \(\$attempt = 1;' -or
    $goalDocumentUpdater -notmatch 'git @authArguments push origin' -or
    $goalDocumentUpdater -match 'push.*--force') {
    Add-Failure -Check 'Goal document publication lacks validated identities or optimistic retry.' -Fix 'Authenticate delivery PRs, reapply edits to the latest goal on contention, and never force-push goal state.'
}

if ($null -ne $sequenceGoalCoordinatorWorkflow -and
    ($sequenceGoalCoordinatorWorkflow -notmatch '(?ms)open-coordinator-pr:.*?concurrency:\s+group: sequence-goal-ledger-\$\{\{ github\.ref_name \}\}' -or
     $sequenceGoalCoordinatorWorkflow -notmatch 'INPROGRESS_LIST: \$\{\{ needs\.run-sequence-loop\.outputs\.inprogress_list \}\}' -or
     $sequenceGoalCoordinatorWorkflow -notmatch '\$env:INPROGRESS_LIST\.Split')) {
    Add-Failure -Check 'Coordinator publication is not serialized with ledger writes or safely bound to its diagram list.' -Fix 'Use the coordinator ledger concurrency group and pass the run output through an environment variable.'
}

if ($null -ne $sequenceGoalCoordinatorWorkflow -and
    ($sequenceGoalCoordinatorWorkflow -match 'steps\.agent|Record agent failure|Record implementation commit or policy failure|record-production-only-delivery-prs:' -or
     $sequenceGoalCoordinatorWorkflow -notmatch '(?ms)run-sequence-loop:.*?concurrency:\s+group: sequence-goal-ledger-\$\{\{ github\.ref_name \}\}' -or
     $sequenceGoalCoordinatorWorkflow -notmatch '(?ms)complete-goal:.*?concurrency:\s+group: sequence-goal-ledger-\$\{\{ github\.ref_name \}\}')) {
    Add-Failure -Check 'Sequence-goal coordinator runs generation inline or omits serialization for its document writes.' -Fix 'Keep generation in isolated surface jobs and serialize coordinator preparation and completion writes per branch.'
}

if ($null -ne $sequenceGoalQaWorkflow -and $null -ne $sequenceGoalQaPublisher -and $null -ne $sequenceGoalDeliveryResolver -and
    ($sequenceGoalQaWorkflow -notmatch 'production-checkpoint\.json' -or
     $sequenceGoalQaWorkflow -notmatch 'ProductionPrNumber = \[int\]\$productionCheckpoint\.number' -or
     $sequenceGoalQaPublisher -notmatch 'Assert-ProductionCurrent' -or
     $sequenceGoalQaPublisher -notmatch 'Production head SHA' -or
     $sequenceGoalDeliveryResolver -notmatch '\$productionCheckpoints\[\$expectedSurface\]' -or
     $sequenceGoalDeliveryResolver -notmatch '\$qaMatchesProduction')) {
    Add-Failure -Check 'QA reuse or completion is not bound to the current immutable production checkpoint.' -Fix 'Carry production repository, PR number, and head SHA through the QA handoff and PR metadata; revalidate before publication and completion.'
}

# The document is the only coordinator recovery record. Reintroducing a side ledger
# must fail this check even if older structural assertions happen to match.
foreach ($workflowText in @($sequenceGoalWorkflow, $sequenceGoalQaWorkflow, $associatedPrMonitorWorkflow, $sequenceGoalPrFinalizerWorkflow)) {
    if ($workflowText -match '\.codex/agent-loop/(?:origins|locks|runs)' -or $workflowText -match 'legacyBranch|legacyCoordinatorIdentity|resolveLegacyOrigin') {
        throw 'Legacy coordinator recovery or branch-compatibility path was reintroduced.'
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'harness rule check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host "- $failure" -ForegroundColor Red
    }

    exit 1
}

Write-Host 'harness rule check passed' -ForegroundColor Green
exit 0
