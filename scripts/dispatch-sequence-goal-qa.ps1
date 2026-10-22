[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][string]$CoordinatorBranch,
    [Parameter(Mandatory)][string]$Surface,
    [Parameter(Mandatory)][string]$ProductionRepository,
    [Parameter(Mandatory)][int]$ProductionNumber,
    [switch]$PublishVerifiedOnly
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1')
$production = gh pr view $ProductionNumber --repo $ProductionRepository --json state,body,headRefOid | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $production.state -notin @('OPEN','MERGED') -or -not (Test-ProductionReadiness (Read-DeliveryGoalState $Repository $CoordinatorBranch $Surface) $production.headRefOid)) { throw 'QA dispatch requires current production readiness.' }
$deliveryToken = $env:GH_TOKEN
try {
if ($env:COORDINATOR_GH_TOKEN) { $env:GH_TOKEN = $env:COORDINATOR_GH_TOKEN }
if (-not $PublishVerifiedOnly) {
$runs = @(gh run list --repo $Repository --workflow ai-delivery-sequence-goal-qa.yml --branch $CoordinatorBranch --limit 1000 --json status,displayTitle | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0 -or $runs.Count -ge 1000) { throw 'Cannot exhaustively reconcile QA runs; dispatch denied.' }
$title = "sequence-goal-qa $CoordinatorBranch $Surface"
$headTitle = "$title $($production.headRefOid)"
if (@($runs | Where-Object { $_.status -in @('queued','in_progress','waiting','requested','pending') -and ([string]$_.displayTitle).Trim() -ceq $headTitle }).Count -gt 0) {
    Write-Host 'Matching QA is active; reusing its run.'
    return
}
}
$prs = @(gh pr list --repo $Repository --state all --search ('"' + $CoordinatorBranch + '" in:body') --limit 1000 --json state,isDraft,mergedAt,body,author,headRepository,baseRefName,headRefOid,url | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0 -or $prs.Count -ge 1000) { throw 'Cannot exhaustively reconcile QA PRs; dispatch denied.' }
$authors = @('github-actions','github-actions[bot]')
if ($env:SEQUENCE_GOAL_GITHUB_APP_SLUG) { $authors += @($env:SEQUENCE_GOAL_GITHUB_APP_SLUG, "$($env:SEQUENCE_GOAL_GITHUB_APP_SLUG)[bot]", "app/$($env:SEQUENCE_GOAL_GITHUB_APP_SLUG)") }
$requiredLines = @(
    "Associated sequence-goal delivery for ``$Repository@$CoordinatorBranch``."
    '- Phase: `QA`'
    "- Surface: ``$Surface``"
    "- Production repository: ``$ProductionRepository``"
    "- Production PR: ``$ProductionNumber``"
    "- Production head SHA: ``$($production.headRefOid)``"
)
foreach ($pr in $prs) {
    if (($pr.state -ne 'OPEN' -and -not $pr.mergedAt) -or $pr.author.login -notin $authors -or $pr.headRepository.nameWithOwner -cne $Repository -or $pr.baseRefName -ne 'main') { continue }
    $missing = @($requiredLines | Where-Object { -not [regex]::IsMatch([string]$pr.body, '(?m)^' + [regex]::Escape($_) + '\s*$') })
    $qaState = Read-DeliveryGoalState $Repository $CoordinatorBranch $Surface QA
    if ($missing.Count -eq 0 -and $qaState.pullRequest -ceq $pr.url -and (Test-QaReadiness $qaState $production.headRefOid $pr.headRefOid)) {
        if ($pr.isDraft) {
            gh pr ready $pr.url --repo $Repository
            if ($LASTEXITCODE -ne 0) { throw 'Failed to publish verified QA readiness; the goal document retains completion for recovery.' }
        }
        Write-Host 'Matching published QA PR already exists; reusing its evidence.'
        return
    }
}
if ($PublishVerifiedOnly) { throw 'QA readiness publication requires a current verified completion entry.' }
gh workflow run ai-delivery-sequence-goal-qa.yml --repo $Repository --ref $CoordinatorBranch -f "coordinator_branch=$CoordinatorBranch" -f "surface=$Surface" -f "production_head_sha=$($production.headRefOid)"
if ($LASTEXITCODE -ne 0) { throw 'Failed to dispatch QA; production checkpoint remains recoverable.' }
} finally { $env:GH_TOKEN = $deliveryToken }
