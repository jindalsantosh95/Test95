#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CoordinatorRepository,
    [Parameter(Mandatory)][string]$CoordinatorBranch,
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][string]$BaseBranch,
    [Parameter(Mandatory)][string]$Branch,
    [Parameter(Mandatory)][ValidateSet('Production', 'QA')][string]$Phase,
    [Parameter(Mandatory)][ValidateSet('Backend', 'MobileIos', 'MobileAndroid', 'WebUi')][string]$Surface,
    [Parameter(Mandatory)][string]$Agent,
    [Parameter(Mandatory)][string]$DiagramPath,
    [string]$GoalIdentity,
    [string]$ProductionRepository,
    [int]$ProductionPrNumber,
    [string]$ProductionHeadSha,
    [object]$ExistingPr,
    [switch]$RecoverPushedBranch
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
$sourceRoot = Split-Path $PSScriptRoot -Parent
$task = "$Surface $Phase"
$state = [ordered]@{
    status = 'InProgress'; repository = $Repository; baseBranch = $BaseBranch
    branch = $Branch; pullRequest = $(if ($ExistingPr) { $ExistingPr.url } else { '' })
    productionRepository = $ProductionRepository; productionNumber = $ProductionPrNumber
    productionCommit = $ProductionHeadSha
    summary = 'Work registered; restart resumes this branch.'
}
if ($Phase -eq 'Production') {
    $state.identity = $GoalIdentity
    $state.outcome = @{ status='Incomplete'; summary=$state.summary; remainingWork=@('Complete goal acceptance criteria.'); acceptanceEvidence=@(); verification=@() }
}
# Establish the ref atomically before claiming ownership. A fresh registration
# must not adopt or overwrite a concurrently created branch.
if ($ExistingPr) {
    $current = gh pr view $ExistingPr.number --repo $Repository --json number,url,state,headRefName,headRefOid,baseRefName,body,isDraft,author,headRepository | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $current.state -ne 'OPEN' -or $current.headRefName -cne $Branch -or $current.baseRefName -cne $BaseBranch) { throw 'Registered PR identity changed.' }
    if (-not $current.isDraft) {
        gh pr ready $current.number --repo $Repository --undo | Out-Host
        if ($LASTEXITCODE -ne 0) { throw 'Cannot return registered PR to draft.' }
    }
    Write-GoalTaskState -RepositoryRoot $sourceRoot -Branch $CoordinatorBranch -DiagramPath $DiagramPath -Task $task -State $state
    $current
} else {
    $expected = ''
    if ($RecoverPushedBranch) {
        $expected = (git rev-parse HEAD).Trim()
        if ($LASTEXITCODE -ne 0) { throw 'Cannot read recovered branch head.' }
    }
    git push "--force-with-lease=refs/heads/${Branch}:$expected" origin "HEAD:refs/heads/$Branch" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Cannot publish registered branch.' }
    Write-GoalTaskState -RepositoryRoot $sourceRoot -Branch $CoordinatorBranch -DiagramPath $DiagramPath -Task $task -State $state
    [pscustomobject]@{ number = 0; headRefName = $Branch; url = ''; state = 'OPEN' }
}
