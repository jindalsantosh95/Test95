. (Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1')

function Assert-ProductionRemoteHead {
    param([Parameter(Mandatory)][string]$ExpectedHead, [Parameter(Mandatory)][scriptblock]$ReadRemoteHead)
    if ((& $ReadRemoteHead) -cne $ExpectedHead) { throw 'Production remote branch changed; refusing stale publication.' }
}

function Wait-ProductionHead {
    param(
        [Parameter(Mandatory)][string]$ExpectedHead,
        [Parameter(Mandatory)][scriptblock]$ReadRemoteHead,
        [Parameter(Mandatory)][scriptblock]$ReadPullRequest,
        [ValidateRange(1, 10)][int]$Attempts = 6,
        [scriptblock]$Delay = { Start-Sleep -Seconds 2 }
    )
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        Assert-ProductionRemoteHead -ExpectedHead $ExpectedHead -ReadRemoteHead $ReadRemoteHead
        $observedPullRequest = & $ReadPullRequest
        if ($null -ne $observedPullRequest -and $observedPullRequest.headRefOid -ceq $ExpectedHead) { return $observedPullRequest }
        if ($attempt -lt $Attempts) { & $Delay }
    }
    throw 'PR metadata did not converge to the pushed production head. Checkpoint retained; restart publication safely.'
}

# The goal document is the recovery record. No encoded commit trailers.
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')

function Read-ProductionGoalState {
    param([string]$RepositoryRoot, [string]$Identity)
    $record = Read-GoalTaskState -RepositoryRoot $script:GoalRoot -Branch $script:GoalBranch -DiagramPath $script:GoalPath -Task "$script:GoalSurface Production"
    if ($null -eq $record) { return $null }
    if ($record.identity -cne $Identity) {
        $record.status = 'InProgress'
        $record | Add-Member -NotePropertyName verifiedCommit -NotePropertyValue '' -Force
    }
    $record.outcome = ConvertFrom-ProductionOutcome ($record.outcome | ConvertTo-Json -Depth 12)
    return $record
}

function Write-ProductionGoalState {
    param([string]$RepositoryRoot, [string]$Identity, $Outcome, $ResourceUsage, [switch]$Reserved)
    $validated = ConvertFrom-ProductionOutcome ($Outcome | ConvertTo-Json -Depth 12)
    if (-not $Reserved -and $validated.status -eq 'Complete' -and ($Identity -split "`n")[-1] -cnotmatch '^[a-f0-9]{64}$') {
        throw 'Complete production requires the verified goal digest.'
    }
    if (-not $Reserved) {
        $dirty = @(git -C $RepositoryRoot status --porcelain)
        if ($LASTEXITCODE -ne 0 -or $dirty.Count -gt 0) { throw 'Commit and publish implementation before recording its verification.' }
    }
    $head = (git -C $RepositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read published production commit.' }
    $record = [pscustomobject][ordered]@{
        status = $(if ($Reserved -or $Outcome.status -eq 'Incomplete') { 'InProgress' } else { $Outcome.status })
        branch = $script:GoalProductionBranch
        pullRequest = $script:GoalProductionPr
        identity = $Identity
        verifiedCommit = $(if (-not $Reserved -and $Outcome.status -eq 'Complete') { $head } else { '' })
        evidence = $(if ($env:GITHUB_RUN_ID) { "https://github.com/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID" } else { $script:GoalProductionPr })
        outcome = $validated
        resourceUsage = $ResourceUsage
    }
    Write-GoalTaskState -RepositoryRoot $script:GoalRoot -Branch $script:GoalBranch -DiagramPath $script:GoalPath -Task "$script:GoalSurface Production" -State $record -ExpectedGoalDigest $(if ($record.status -eq 'Complete') { ($Identity -split "`n")[-1] } else { '' })
    return $record
}

function Get-ProductionRecoveryMode {
    param($State, [string]$HeadSha, [bool]$NeedsReadyTransition = $false)
    if ($null -eq $State -or $State.status -ne 'Complete' -or $State.verifiedCommit -cne $HeadSha) { return 'Resume' }
    if ($NeedsReadyTransition -or [string]::IsNullOrWhiteSpace($State.pullRequest)) { return 'Publish' }
    return 'Reuse'
}
