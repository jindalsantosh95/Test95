#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$root = Join-Path ([IO.Path]::GetTempPath()) "goal-registration-$([guid]::NewGuid().ToString('N'))"
$remote = Join-Path $root 'remote.git'
$writer = Join-Path $root 'writer'
New-Item -ItemType Directory $root | Out-Null
git init --bare $remote | Out-Host
git clone $remote $writer | Out-Host
git -C $writer config user.name test
git -C $writer config user.email test@example.invalid
New-Item -ItemType Directory (Join-Path $writer 'docs/sequence-diagrams') -Force | Out-Null
$goal = 'docs/sequence-diagrams/test.md'
Set-Content (Join-Path $writer $goal) '# Goal'
git -C $writer add .
git -C $writer commit -m initial | Out-Host
git -C $writer push origin HEAD:goal | Out-Host
$base = (git -C $writer rev-parse HEAD).Trim()
$fixtureScripts = Join-Path $writer scripts
New-Item -ItemType Directory $fixtureScripts | Out-Null
foreach ($name in @('register-sequence-goal-branch.ps1','sequence-goal-document-state.ps1','update-sequence-goal-document.ps1')) {
    Copy-Item (Join-Path $PSScriptRoot $name) $fixtureScripts
}
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
function global:gh { throw 'Branch registration must not create a PR before there is work to review.' }
Push-Location $writer
try {
    $arguments = @{ CoordinatorRepository='VAAD1ST/eapi';CoordinatorBranch='goal';Repository='VAAD1ST/eapi';BaseBranch='main';Branch='production';Phase='Production';Surface='Backend';Agent='test';DiagramPath=$goal;GoalIdentity='requirements' }
    $result = & "$fixtureScripts/register-sequence-goal-branch.ps1" @arguments
    if ($result.number -ne 0 -or $result.headRefName -ne 'production') { throw 'Registration did not return its branch.' }
    $remoteHead = ((git ls-remote --heads origin refs/heads/production) -split '\s+')[0]
    if ($remoteHead -cne $base) { throw 'Registration introduced a placeholder commit.' }
    $state = Read-GoalTaskState $writer goal $goal 'Backend Production'
    if ($state.branch -ne 'production' -or $state.identity -ne 'requirements' -or $state.status -ne 'InProgress') { throw 'Branch ownership was not saved.' }
    Set-Content implementation.txt 'real work'
    git add implementation.txt
    git commit -m implementation | Out-Host
    git push origin HEAD:production | Out-Host
    $published = (git rev-parse HEAD).Trim()
    $null = & "$fixtureScripts/register-sequence-goal-branch.ps1" @arguments -RecoverPushedBranch
    if (((git ls-remote --heads origin refs/heads/production) -split '\s+')[0] -cne $published) { throw 'Restart lost pushed implementation.' }
    # A colliding fresh registration cannot overwrite or claim someone else's ref.
    git push origin "${base}:refs/heads/collision" | Out-Host
    $arguments.Branch = 'collision'
    $collisionRejected = $false
    try { & "$fixtureScripts/register-sequence-goal-branch.ps1" @arguments | Out-Null } catch { $collisionRejected = $_.Exception.Message -eq 'Cannot publish registered branch.' }
    $registeredState = Read-GoalTaskState $writer goal $goal 'Backend Production'
    if (-not $collisionRejected -or $registeredState.branch -ne 'production' -or (git --git-dir $remote rev-parse collision).Trim() -cne $base) { throw 'A failed registration claimed or overwrote a colliding ref.' }
    $arguments.Phase='QA'; $arguments.Branch='qa'; $arguments.ProductionHeadSha=('a'*40)
    $null = & "$fixtureScripts/register-sequence-goal-branch.ps1" @arguments
    $state = Read-GoalTaskState $writer goal $goal 'Backend QA'
    if ($state.productionCommit -cne ('a'*40) -or $state.branch -ne 'qa') { throw 'QA lost its production binding.' }
    # Exercise the real QA publisher, including failure after push but before PR creation.
    foreach ($name in @('publish-sequence-goal-qa-pr.ps1','sequence-goal-production-readiness.ps1')) {
        Copy-Item (Join-Path $PSScriptRoot $name) $fixtureScripts
    }
    'param($WorkflowId,$BudgetEvidencePath); ''{"maximumLabel":"test","actualLabel":"test"}''' | Set-Content "$fixtureScripts/get-agent-resource-summary.ps1"
    New-Item -ItemType Directory 'VAA.EAPI.Automation/Tests' -Force | Out-Null
    Set-Content VAA.EAPI.Automation/Tests/fixture.txt 'before'
    git add VAA.EAPI.Automation
    git commit -m 'QA baseline' | Out-Host
    git push origin HEAD:main | Out-Host
    $global:qaExerciseRemote = $remote
    $global:qaExercisePr = $null
    $global:qaExerciseFailCreate = $true
    function global:gh {
        $global:LASTEXITCODE = 0
        if ($args[0] -eq 'api') {
            if ($args[1] -match '/compare/') { '{"files":[{"filename":"docs/sequence-diagrams/test.md"}]}' }
            else {
                $text = (git --git-dir $global:qaExerciseRemote show 'goal:docs/sequence-diagrams/test.md') -join "`n"
                @{encoding='base64';content=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($text))} | ConvertTo-Json
            }
            return
        }
        switch ("$($args[0]) $($args[1])") {
            'pr view' {
                if ($args[2] -eq '7') { @{state='OPEN';headRefOid=('a'*40)} | ConvertTo-Json }
                else { $global:qaExercisePr | ConvertTo-Json -Depth 6 }
            }
            'pr list' {
                $requestedState = $args[([array]::IndexOf($args,'--state')+1)]
                $requestedHead = if ('--head' -in $args) { $args[([array]::IndexOf($args,'--head')+1)] } else { '' }
                ConvertTo-Json -InputObject @($global:qaExercisePr | Where-Object { $_ -and ($requestedState -eq 'all' -or $_.state -eq 'OPEN') -and (-not $requestedHead -or $_.headRefName -eq $requestedHead) }) -Depth 6
            }
            'pr create' {
                if ($global:qaExerciseFailCreate) { $global:LASTEXITCODE=1; return }
                $body = Get-Content $args[([array]::IndexOf($args,'--body-file')+1)] -Raw
                $branch = $args[([array]::IndexOf($args,'--head')+1)]
                $global:qaExercisePr = [pscustomobject]@{number=42;url='https://github.com/VAAD1ST/eapi/pull/42';state='OPEN';body=$body;headRefName=$branch;baseRefName='main';mergedAt=$null;isDraft=$true;author=@{login='github-actions[bot]'};headRepository=@{nameWithOwner='VAAD1ST/eapi'}}
            }
            'pr edit' { $global:qaExercisePr.body = Get-Content $args[([array]::IndexOf($args,'--body-file')+1)] -Raw }
            'pr ready' {
                if ((git --git-dir $global:qaExerciseRemote rev-parse $global:qaExercisePr.headRefName).Trim() -cne $global:qaBeforeRetryHead) { throw 'QA replacement was pushed before returning the PR to draft.' }
                if ($global:qaFailDraft) { $global:LASTEXITCODE = 1; return }
                $global:qaExercisePr.isDraft = '--undo' -in $args
            }
            default { throw "Unexpected QA command: $args" }
        }
    }
    . (Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1')
    Write-GoalTaskState $writer goal $goal 'Backend Production' @{status='Complete';verifiedCommit=('a'*40);identity=(Get-ProductionGoalDigest '# Goal')}
    Write-GoalTaskState $writer goal $goal 'Backend QA' @{status='Pending'}
    $qaArguments = @{CoordinatorBranch='goal';Surface='Backend';Agent='test';DiagramPath=$goal;Repository='VAAD1ST/eapi';ProductionRepository='VAAD1ST/eapi';ProductionPrNumber=7;ProductionHeadSha=('a'*40)}
    $priorToken = $env:GH_TOKEN
    $env:GH_TOKEN = 'fixture-not-a-credential'
    try {
        $registered = & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments -PrepareOnly
        if ($registered.number -ne 0) { throw 'QA preparation created a placeholder PR.' }
        Set-Content VAA.EAPI.Automation/Tests/fixture.txt 'after'
        New-Item -ItemType Directory TestResults/agent-resource -Force | Out-Null
        $usage = 'TestResults/agent-resource/sequence-goal-loop-fixture-automationtests-backend-1.jsonl'
        Set-Content $usage '{}'
        Set-Content "$usage.outcome.json" '{"schemaVersion":1,"phase":"AutomationTests","existingCoveragePaths":[]}'
        Set-Content ($usage -replace '\.jsonl$','.budget.json') '{}'
        $failed = $false
        try { & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments } catch { $failed = $_.Exception.Message -like 'Cannot create QA PR*' }
        if (-not $failed) { throw 'Did not reach the intended publication interruption.' }
        $pushedHead = (git --git-dir $remote rev-parse $registered.headRefName).Trim()
        $global:qaExerciseFailCreate = $false
        & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments
        if ((git --git-dir $remote rev-parse $registered.headRefName).Trim() -cne $pushedHead) { throw 'Restart duplicated the already-published QA commit.' }
        if ((git --git-dir $remote show "$($registered.headRefName):VAA.EAPI.Automation/Tests/fixture.txt") -ne 'after' -or -not $global:qaExercisePr.isDraft) { throw 'QA publication must preserve code and remain draft until verification.' }
        $global:qaExercisePr.isDraft = $false
        $global:qaBeforeRetryHead = $pushedHead
        Set-Content VAA.EAPI.Automation/Tests/retry.txt 'retry coverage'
        $global:qaFailDraft = $true
        $draftRejected = $false
        try { & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments } catch { $draftRejected = $_.Exception.Message -like 'Cannot return QA PR to draft*' }
        if (-not $draftRejected -or (git --git-dir $remote rev-parse $registered.headRefName).Trim() -cne $pushedHead) { throw 'Failed draft transition did not prevent the replacement push.' }
        $global:qaFailDraft = $false
        & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments
        if (-not $global:qaExercisePr.isDraft -or (git --git-dir $remote show "$($registered.headRefName):VAA.EAPI.Automation/Tests/retry.txt") -ne 'retry coverage') { throw 'Reused ready QA PR must become draft before publishing replacement coverage.' }
        $global:qaExercisePr.state = 'CLOSED'
        $priorRun = $env:GITHUB_RUN_ID; $priorAttempt = $env:GITHUB_RUN_ATTEMPT
        $env:GITHUB_RUN_ID = '200'; $env:GITHUB_RUN_ATTEMPT = '2'
        try {
            $fresh = & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments -PrepareOnly
            if ($fresh.headRefName -eq $registered.headRefName -or $fresh.headRefName -notlike '*-retry-200-2' -or
                (git --git-dir $remote show "$($fresh.headRefName):VAA.EAPI.Automation/Tests/fixture.txt") -ne 'before') { throw 'Closed QA history was reused as a new attempt.' }
        } finally { $env:GITHUB_RUN_ID = $priorRun; $env:GITHUB_RUN_ATTEMPT = $priorAttempt }
        # An attestation-only retry must not fall back to a historical merged PR.
        $global:qaExercisePr.state = 'MERGED'; $global:qaExercisePr.mergedAt = '2026-10-06T12:00:00Z'
        Add-Content $goal ('| `QA-Backend` | `VAAD1ST/eapi` | `main` | `' + $registered.headRefName + '` | [#42](https://github.com/VAAD1ST/eapi/pull/42) |')
        Set-Content "$usage.outcome.json" '{"schemaVersion":1,"phase":"AutomationTests","existingCoveragePaths":["VAA.EAPI.Automation/Tests/fixture.txt"]}'
        & "$fixtureScripts/publish-sequence-goal-qa-pr.ps1" @qaArguments
        if ($global:qaExercisePr.headRefName -cne $fresh.headRefName -or
            (git --git-dir $remote show "$($fresh.headRefName):VAA.EAPI.Automation/Tests/fixture.txt") -ne 'before') { throw 'Attestation-only publication reused historical merged QA.' }
    } finally { $env:GH_TOKEN = $priorToken }
    Write-Host 'Registration exercise passed: document ownership, no placeholder commit/PR, restart preserves published work, QA commit binding.'
} finally { Pop-Location; Remove-Item Function:global:gh }
