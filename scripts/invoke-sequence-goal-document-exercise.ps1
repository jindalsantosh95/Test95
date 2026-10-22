#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-recovery.ps1')
function Assert-GoalCase([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "Goal recovery exercise failed: $Name" }
    Write-Host "PASS: $Name"
}
$root = Join-Path ([IO.Path]::GetTempPath()) "goal-document-test-$([guid]::NewGuid().ToString('N'))"
$remote = Join-Path $root 'remote.git'
$writer = Join-Path $root 'writer'
$other = Join-Path $root 'other'
New-Item -ItemType Directory -Path $root | Out-Null
git init --bare $remote | Out-Host
git clone $remote $writer | Out-Host
git -C $writer config user.name test
git -C $writer config user.email test@example.invalid
$path = 'docs/sequence-diagrams/test.md'
New-Item -ItemType Directory -Path (Join-Path $writer 'docs/sequence-diagrams') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $writer $path) -Value "# Goal`n`n| Harness Sensor | pending |`n`nAcceptance: fraud lock persists."
git -C $writer add .
git -C $writer commit -m initial | Out-Host
git -C $writer push origin HEAD:goal | Out-Host
git clone --branch goal $remote $other | Out-Host
git -C $other config user.name test
git -C $other config user.email test@example.invalid
$script:GoalRoot = $writer; $script:GoalBranch = 'goal'; $script:GoalPath = $path; $script:GoalSurface = 'Backend'
$script:GoalProductionBranch = 'production'; $script:GoalProductionPr = 'https://example.invalid/pr/1'
$incomplete = [pscustomobject]@{ status='Incomplete'; summary='Working'; remainingWork=@('Add concurrency tests'); acceptanceEvidence=@(); verification=@() }
$null = Write-ProductionGoalState $writer 'goal-identity' $incomplete @{} -Reserved
$record = Read-ProductionGoalState $writer 'goal-identity'
Assert-GoalCase ($record.status -eq 'InProgress' -and (Get-ProductionRecoveryMode $record ('a'*40)) -eq 'Resume') 'interrupted work resumes without a reservation override'
$dirtyPath = Join-Path $writer 'uncommitted.txt'
Set-Content -LiteralPath $dirtyPath -Value 'unverified changes'
$dirtyRejected = $false
try { $null = Write-ProductionGoalState $writer 'goal-identity' $incomplete @{} } catch { $dirtyRejected = $_.Exception.Message -like 'Commit and publish*' }
Remove-Item -LiteralPath $dirtyPath
Assert-GoalCase $dirtyRejected 'uncommitted work cannot become published verification evidence'
$text = (git -C $writer show "FETCH_HEAD:$path") -join "`n"
$digest = Get-ProductionGoalDigest $text
Assert-GoalCase ($digest -eq (Get-ProductionGoalDigest (Set-GoalTaskState $text 'Backend QA' @{status='InProgress'}))) 'status changes do not invalidate requirements'
Assert-GoalCase ($digest -ne (Get-ProductionGoalDigest ($text.Replace('fraud lock persists','fraud lock persists atomically')))) 'acceptance changes invalidate verification'
# Force a real non-fast-forward push between the updater's fetch and push.
$race = [pscustomobject]@{ first = $true; other = $other; path = $path }
$stateLibrary = Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1'
$edit = {
    param($document)
    . $stateLibrary
    if ($race.first) {
        $race.first = $false
        git -C $race.other pull --ff-only origin goal | Out-Host
        $otherText = Get-Content -LiteralPath (Join-Path $race.other $race.path) -Raw
        Set-Content -LiteralPath (Join-Path $race.other $race.path) -Value (Set-GoalTaskState $otherText 'Backend QA' @{status='InProgress'})
        git -C $race.other add .
        git -C $race.other commit -m 'QA starts' | Out-Host
        git -C $race.other push origin HEAD:goal | Out-Host
    }
    $current = Get-Content -LiteralPath $document -Raw
    Set-Content -LiteralPath $document -Value (Set-GoalTaskState $current 'Backend Production' @{status='InProgress';remainingWork=@('Finish tests')})
}.GetNewClosure()
Update-SequenceGoalDocument -RepositoryRoot $writer -Branch goal -DiagramPath $path -Edit $edit -Message 'Production progresses'
$production = Read-GoalTaskState $writer goal $path 'Backend Production'
$qa = Read-GoalTaskState $writer goal $path 'Backend QA'
Assert-GoalCase ($production.remainingWork[0] -eq 'Finish tests' -and $qa.status -eq 'InProgress') 'concurrent production and QA updates both survive'
$text = (git -C $writer show "FETCH_HEAD:$path") -join "`n"
Assert-GoalCase ($text.Contains('Acceptance: fraud lock persists.')) 'requirements survive concurrent status commits'
$head = (git -C $writer rev-parse FETCH_HEAD).Trim()
Write-GoalTaskState $writer goal $path 'Backend QA' $qa
$null = Read-GoalTaskState $writer goal $path 'Backend QA'
Assert-GoalCase ($head -eq (git -C $writer rev-parse FETCH_HEAD).Trim()) 'identical status is idempotent'
$complete = @{status='Complete'; verifiedCommit=('a'*40)}
Assert-GoalCase ((Get-ProductionRecoveryMode $complete ('b'*40)) -eq 'Resume') 'changed production code cannot reuse old verification'
Assert-GoalCase ((Get-ProductionRecoveryMode $complete ('a'*40)) -eq 'Publish') 'interruption after verification recovers publication'
$complete.pullRequest = 'https://example.invalid/pr/1'
Assert-GoalCase ((Get-ProductionRecoveryMode $complete ('a'*40)) -eq 'Reuse') 'verified published production is reused'
$malformedRejected = $false
try { Get-GoalTaskState "### Delivery: Backend QA`ninvalid" 'Backend QA' } catch { $malformedRejected = $true }
Assert-GoalCase $malformedRejected 'malformed state cannot silently become a fresh task'
# Change requirements after the first optimistic edit but before its push. The
# retry must compare the digest again and refuse stale completion atomically.
$global:goalRaceGit = @(Get-Command git -CommandType Application)[0].Source
$global:goalRaceOther = $other; $global:goalRacePath = $path; $global:goalRacePending = $true
function global:git {
    if ($global:goalRacePending -and 'push' -in $args) {
        $global:goalRacePending = $false
        & $global:goalRaceGit -C $global:goalRaceOther pull --ff-only origin goal | Out-Host
        $file = Join-Path $global:goalRaceOther $global:goalRacePath
        [IO.File]::WriteAllText($file, ([IO.File]::ReadAllText($file)).Replace('fraud lock persists.', 'fraud lock persists atomically.'))
        & $global:goalRaceGit -C $global:goalRaceOther add .
        & $global:goalRaceGit -C $global:goalRaceOther commit -m 'Change acceptance during completion' | Out-Host
        & $global:goalRaceGit -C $global:goalRaceOther push origin HEAD:goal | Out-Host
    }
    & $global:goalRaceGit @args
    $global:LASTEXITCODE = $LASTEXITCODE
}
try {
    $staleRejected = $false
    $outcome = @{status='Complete';summary='Verified';remainingWork=@();acceptanceEvidence=@('Tests passed');verification=@(@{command='verify';exitCode=0})}
    try { $null = Write-ProductionGoalState $writer $digest $outcome @{} } catch { $staleRejected = $_.Exception.Message -like 'Goal requirements changed*'; if (-not $staleRejected) { throw } }
    Assert-GoalCase ($staleRejected -and -not $global:goalRacePending) 'completion rechecks requirements after a concurrent push'
} finally { Remove-Item Function:global:git }
$afterRace = Read-GoalTaskState $writer goal $path 'Backend Production'
Assert-GoalCase ($afterRace.status -eq 'InProgress') 'stale completion never reaches the goal branch'
# Exercise the real authenticated recorder and direct publisher against local Git.
$fixtureScripts = Join-Path $writer 'scripts'
New-Item -ItemType Directory -Path $fixtureScripts | Out-Null
foreach ($name in @('save-sequence-goal-delivery.ps1', 'record-sequence-goal-associated-prs.ps1', 'sequence-goal-document-state.ps1', 'sequence-goal-production-readiness.ps1', 'update-sequence-goal-document.ps1')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $fixtureScripts
}
$global:qaDocumentHead = 'b' * 40
function global:gh {
    if ($args[0] -ne 'pr' -or $args[1] -ne 'view') { throw 'Unexpected GitHub operation in document test.' }
    $global:LASTEXITCODE = 0
    @{
        number=42; url='https://github.com/VAAD1ST/eapi/pull/42'; state='OPEN'; mergedAt=$null
        headRefOid=$global:qaDocumentHead
        headRefName='qa-branch'; baseRefName='main'; headRepository=@{nameWithOwner='VAAD1ST/eapi'}
        author=@{login='github-actions[bot]'}
        body='Associated sequence-goal delivery for `VAAD1ST/eapi@goal`.' + "`n" + '- Phase: `QA`' + "`n" + '- Surface: `Backend`'
    } | ConvertTo-Json -Depth 4
}
try {
    $evidence = [pscustomobject]@{
        surface='QA-Backend'; phase='QA'; coordinatorRepository='VAAD1ST/eapi'; coordinatorBranch='goal'
        agent='test'; repository='VAAD1ST/eapi'; baseBranch='main'; headBranch='qa-branch'; number=42
        url='https://github.com/VAAD1ST/eapi/pull/42'; state='OPEN'; productionHeadSha=('a'*40)
    }
    $verifiedGoalText = (git -C $writer show "FETCH_HEAD:$path") -join "`n"
    $verification = @{VerifiedQaCommit=('b'*40);VerifiedGoalDigest=(Get-ProductionGoalDigest $verifiedGoalText)}
    & (Join-Path $fixtureScripts 'save-sequence-goal-delivery.ps1') -Evidence $evidence -DiagramPath $path @verification
    $saved = Read-GoalTaskState $writer goal $path 'Backend QA'
    Assert-GoalCase ($saved.pullRequest -eq $evidence.url -and $saved.verifiedCommit -eq ('a'*40)) 'direct QA publication records the PR and tested commit'
    $production = Read-GoalTaskState $writer goal $path 'Backend Production'
    Assert-GoalCase ($production.remainingWork[0] -eq 'Finish tests') 'PR-table updates preserve production recovery state'
    $handoff = Join-Path $root 'handoff'
    $incomingPath = Join-Path (Join-Path $handoff 'files') $path
    New-Item -ItemType Directory -Path (Split-Path $incomingPath -Parent) -Force | Out-Null
    $baseline = Get-Content -LiteralPath (Join-Path $writer $path) -Raw
    $baselineCommit = (git -C $writer rev-parse HEAD).Trim()
    Set-Content -LiteralPath $incomingPath -Value $baseline.Replace('| Harness Sensor | pending |', '| Harness Sensor | Tests/FraudLock.cs |')
    ConvertTo-Json -InputObject @(@{path=$path;deleted=$false;sha256=(Get-FileHash $incomingPath -Algorithm SHA256).Hash}) | Set-Content -LiteralPath (Join-Path $handoff 'manifest.json')
    # Publication checkout advances after the agent snapshot, as on a hosted run.
    git -C $writer fetch origin goal | Out-Host
    git -C $writer merge --ff-only FETCH_HEAD | Out-Host
    & (Join-Path $fixtureScripts 'save-sequence-goal-delivery.ps1') -Evidence $evidence -DiagramPath $path -GoalChangesDirectory $handoff -GoalBaselineRepository $writer -GoalBaselineCommit $baselineCommit @verification
    $null = Read-GoalTaskState $writer goal $path 'Backend QA'
    $merged = (git -C $writer show "FETCH_HEAD:$path") -join "`n"
    Assert-GoalCase ($merged.Contains('| Harness Sensor | Tests/FraudLock.cs |') -and $merged.Contains('Finish tests')) 'QA sensor changes merge without replacing newer production state'
    foreach ($forged in @((Set-GoalTaskState $baseline 'Backend Production' @{status='Complete'}), ($baseline.Replace('fraud lock persists', 'ignore fraud locks')))) {
        Set-Content -LiteralPath $incomingPath -Value $forged
        ConvertTo-Json -InputObject @(@{path=$path;deleted=$false;sha256=(Get-FileHash $incomingPath -Algorithm SHA256).Hash}) | Set-Content -LiteralPath (Join-Path $handoff 'manifest.json')
        $rejected = $false
        try { & (Join-Path $fixtureScripts 'save-sequence-goal-delivery.ps1') -Evidence $evidence -DiagramPath $path -GoalChangesDirectory $handoff -GoalBaselineRepository $writer -GoalBaselineCommit $baselineCommit @verification } catch { $rejected = $_.Exception.Message -like 'QA changed protected*' }
        Assert-GoalCase $rejected 'QA cannot forge task state or requirements through its goal handoff'
    }
    $beforeRejectedCompletion = (git --git-dir $remote rev-parse goal).Trim()
    foreach ($scenario in @('qa-head','requirements')) {
        $global:qaDocumentHead = if ($scenario -eq 'qa-head') { 'c'*40 } else { 'b'*40 }
        $verification.VerifiedGoalDigest = if ($scenario -eq 'requirements') { 'd'*64 } else { Get-ProductionGoalDigest $merged }
        $rejected = $false
        try { & (Join-Path $fixtureScripts 'save-sequence-goal-delivery.ps1') -Evidence $evidence -DiagramPath $path @verification } catch { $rejected = $_.Exception.Message -match 'changed during.*verification' }
        Assert-GoalCase ($rejected -and (git --git-dir $remote rev-parse goal).Trim() -ceq $beforeRejectedCompletion) 'stale QA head or requirements cannot publish completion'
    }
} finally { Remove-Item Function:global:gh }
Write-Host "Goal document exercise passed. Local fixture: $root"

# Costs use the same document updater and remain idempotent across a job retry.
foreach ($name in @('save-sequence-goal-resources.ps1','export-sequence-goal-resources.ps1','get-agent-resource-summary.ps1')) {
    Copy-Item (Join-Path $PSScriptRoot $name) $fixtureScripts
}
Copy-Item (Join-Path (Split-Path $PSScriptRoot -Parent) 'docs/agent-resource-budgets.json') (Join-Path $writer 'docs/agent-resource-budgets.json')
function global:gh {
    $global:LASTEXITCODE = 0
    if ($args[0] -eq 'api') {
        if ($args[1] -match '/artifacts') { '{"total_count":2,"artifacts":[{"name":"production-agent-resource-Backend-100-1"},{"name":"automation-agent-resource-Backend-100-1"}]}' }
        else { '{"files":[{"filename":"docs/sequence-diagrams/test.md"}]}' }
        return
    }
    if ($args[0] -ne 'run' -or $args[1] -ne 'download') { throw 'Unexpected resource GitHub operation.' }
    $directory = $args[([array]::IndexOf($args,'--dir')+1)]
    New-Item -ItemType Directory $directory -Force | Out-Null
    $budget = @{workflow='sequence-goal-loop';model='fixture';usage=@{inputTokens=10;cachedInputTokens=0;outputTokens=5;reasoningOutputTokens=0};estimatedStandardApiEquivalentUsd=0.01}
    $budget | ConvertTo-Json | Set-Content (Join-Path $directory 'test.budget.json')
    Set-Content (Join-Path $directory 'test.executed') 'true'
}
try {
    & "$fixtureScripts/save-sequence-goal-resources.ps1" -Branch goal -RunId 100 -Attempt 1 -ExpectBudget $true
    $cost = Read-GoalTaskState $writer goal $path Resources
    Assert-GoalCase ($cost.attempts.Count -eq 2) 'production and QA costs recorded in document'
    $before = (git -C $writer rev-parse FETCH_HEAD).Trim()
    & "$fixtureScripts/save-sequence-goal-resources.ps1" -Branch goal -RunId 100 -Attempt 1 -ExpectBudget $true
    $cost = Read-GoalTaskState $writer goal $path Resources
    Assert-GoalCase ($cost.attempts.Count -eq 2 -and $before -ceq (git -C $writer rev-parse FETCH_HEAD).Trim()) 'cost retry neither doubles totals nor adds a commit'
    $exportDoc = Join-Path $root 'cost-goal.md'
    git -C $writer show "FETCH_HEAD:$path" | Set-Content $exportDoc
    $paths = @(& "$fixtureScripts/export-sequence-goal-resources.ps1" -DiagramPaths @($exportDoc))
    $total = & "$fixtureScripts/get-agent-resource-summary.ps1" -WorkflowId sequence-goal-loop -BudgetEvidencePath $paths | ConvertFrom-Json
    Assert-GoalCase ($total.actual.inputTokens -eq 20 -and $total.actual.outputTokens -eq 10) 'document costs preserve aggregate totals'
} finally { Remove-Item Function:global:gh }

# Execute the actual coordinator initialization twice against local Git. Old lock,
# origin and summary files deliberately exist but cannot control restart.
$workflow = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) '.github/workflows/ai-delivery-sequence-goal-loop.yml') -Raw
$initialize = [regex]::Match($workflow, '(?ms)      - name: Initialize coordinator state in goal document.*?        run: \|\r?\n(?<code>.*?)(?=^      - name:)').Groups['code'].Value -replace '(?m)^          ', ''
if (-not $initialize) { throw 'Coordinator initialization test could not find its workflow step.' }
$continuation = [regex]::Match($initialize, '(?ms)^\$previous = Get-GoalTaskState.*?^if \(\$automaticContinuations -gt 3\).*?\r?$').Value
if (-not $continuation) { throw 'Missing document continuation gate.' }
$priorAutomaticResume = $env:AUTOMATIC_RESUME
$priorDeliveryStage = $env:DELIVERY_STAGE
try {
    $env:DELIVERY_STAGE = 'Delivery'
    foreach ($automatic in @('false','true')) {
        $env:AUTOMATIC_RESUME = $automatic
        foreach ($count in @(0,1,2,3)) {
            $text = Set-GoalTaskState '# Goal' Coordinator @{automaticContinuations=$count}
            $rejected = $false
            try { & ([scriptblock]::Create($continuation + '; $script:observedContinuations=$automaticContinuations')) } catch { $rejected = $_.Exception.Message -like 'Automatic delivery continuation limit reached*' }
            if ($automatic -eq 'true' -and $count -eq 3) {
                Assert-GoalCase $rejected 'automatic recovery is bounded by the goal document'
            } else {
                Assert-GoalCase (-not $rejected -and $script:observedContinuations -eq $(if ($automatic -eq 'true') { $count+1 } else { 0 })) 'explicit restart resets recovery; automatic restart retains its count'
            }
        }
    }
    $env:AUTOMATIC_RESUME = 'true'; $env:DELIVERY_STAGE = 'Completion'
    $text = Set-GoalTaskState '# Goal' Coordinator @{automaticContinuations=3;continuationStage='Delivery'}
    & ([scriptblock]::Create($continuation + '; $script:observedContinuations=$automaticContinuations'))
    Assert-GoalCase ($script:observedContinuations -eq 1) 'completion gets its own bounded continuation allowance'
} finally { $env:AUTOMATIC_RESUME = $priorAutomaticResume; $env:DELIVERY_STAGE = $priorDeliveryStage }
$priorEnvironment = @{}
foreach ($name in @('INPROGRESS_LIST','GITHUB_REF_NAME','GITHUB_REPOSITORY','GITHUB_OUTPUT')) { $priorEnvironment[$name] = [Environment]::GetEnvironmentVariable($name) }
Push-Location $writer
try {
    $env:INPROGRESS_LIST = $path; $env:GITHUB_REF_NAME = 'goal'; $env:GITHUB_REPOSITORY = 'VAAD1ST/eapi'; $env:GITHUB_OUTPUT = Join-Path $root 'outputs.txt'
    foreach ($directory in @('locks','origins','runs')) {
        New-Item -ItemType Directory ".codex/agent-loop/$directory" -Force | Out-Null
        Set-Content ".codex/agent-loop/$directory/stale.json" '{"attempt":999,"invalid":"obsolete state"}'
    }
    foreach ($run in @(101,102)) {
        $code = $initialize -replace '\$\{\{ github.run_id \}\}', [string]$run -replace '\$\{\{ github.run_attempt \}\}', '1'
        & ([scriptblock]::Create($code))
        $state = Read-GoalTaskState $writer goal $path Coordinator
        Assert-GoalCase ($state.runId -eq [string]$run -and $state.branch -eq 'goal') 'coordinator restart reads no origin, lock, or summary sidecar'
    }
    $production = Read-GoalTaskState $writer goal $path 'Backend Production'
    $qa = Read-GoalTaskState $writer goal $path 'Backend QA'
    Assert-GoalCase ($production.remainingWork[0] -eq 'Finish tests' -and $qa.status -eq 'Complete') 'coordinator restart preserves production and QA progress'
    $document = (git -C $writer show "FETCH_HEAD:$path") -join "`n"
    $localDocument = Join-Path $root 'coordinator.md'
    Set-Content $localDocument $document
    $state = Get-GoalCoordinatorState $localDocument
    $state.status = 'Complete'
    Set-GoalCoordinatorState $localDocument $state
    Assert-GoalCase ((Get-GoalCoordinatorState $localDocument).status -eq 'Complete' -and (Get-Content $localDocument -Raw).Contains('Finish tests')) 'verification state updates preserve other document entries'
} finally {
    Pop-Location
    foreach ($name in $priorEnvironment.Keys) { [Environment]::SetEnvironmentVariable($name,$priorEnvironment[$name]) }
}

& node (Join-Path $PSScriptRoot 'invoke-sequence-goal-monitor-exercise.cjs')
if ($LASTEXITCODE -ne 0) { throw 'Goal-document monitor exercise failed.' }
