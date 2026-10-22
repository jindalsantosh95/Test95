#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][object]$Evidence,
    [string]$DiagramPath,
    [string]$GoalChangesDirectory,
    [string]$GoalBaselineRepository,
    [ValidatePattern('^[a-f0-9]{40}$')][string]$GoalBaselineCommit,
    [ValidatePattern('^[a-f0-9]{40}$')][string]$VerifiedQaCommit,
    [ValidatePattern('^[a-f0-9]{64}$')][string]$VerifiedGoalDigest,
    [switch]$RegistrationOnly
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
$sourceRoot = Split-Path $PSScriptRoot -Parent
$recorder = Join-Path $PSScriptRoot 'record-sequence-goal-associated-prs.ps1'
$stateLibrary = Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1'
$readinessLibrary = Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1'
if ($Evidence.phase -eq 'QA' -and -not $RegistrationOnly -and (-not $VerifiedQaCommit -or -not $VerifiedGoalDigest)) { throw 'QA completion requires the verified QA head and goal digest.' }
if (-not $DiagramPath) {
    $diagrams = @(git -C $sourceRoot diff --name-only origin/main...HEAD | Where-Object { $_ -match '^docs/sequence-diagrams/(?!_)[^/]+\.md$' })
    if ($LASTEXITCODE -ne 0 -or $diagrams.Count -ne 1) { throw 'Expected one goal document.' }
    $DiagramPath = $diagrams[0]
}
$directory = Join-Path ([IO.Path]::GetTempPath()) "goal-evidence-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $directory | Out-Null
$evidencePath = Join-Path $directory 'delivery.json'
try {
    # QA may update its sensor/acceptance evidence. Merge against the original
    # snapshot, never replace today's document with yesterday's whole file.
    $goalChanges = @()
    if ($GoalChangesDirectory) {
        if (-not $GoalBaselineRepository -or -not $GoalBaselineCommit) { throw 'QA goal edits require the trusted pre-agent baseline.' }
        foreach ($entry in @(Get-Content -LiteralPath (Join-Path $GoalChangesDirectory 'manifest.json') -Raw | ConvertFrom-Json)) {
            if ($entry.path -cnotin @($DiagramPath, 'docs/sequence-diagrams.md') -or $entry.deleted) { throw 'QA goal change is outside its document/index.' }
            $incomingPath = Join-Path (Join-Path $GoalChangesDirectory 'files') $entry.path
            $item = Get-Item -LiteralPath $incomingPath -Force
            if ($item.PSIsContainer -or $item.LinkType -or (Get-FileHash -LiteralPath $incomingPath -Algorithm SHA256).Hash -cne $entry.sha256) { throw 'Invalid QA goal evidence.' }
            $baseText = (git -C $GoalBaselineRepository show "$($GoalBaselineCommit):$($entry.path)") -join "`n"
            if ($LASTEXITCODE -ne 0) { throw 'Cannot read QA goal baseline.' }
            $incomingText = Get-Content -LiteralPath $incomingPath -Raw
            # QA owns only these evidence rows. All task state, resources,
            # requirements, and index lifecycle fields remain workflow-owned.
            $allowedRows = '(?m)^\|\s*(?:Harness Sensor|Acceptance evidence)\s*\|[^\r\n]*'
            $baseProtected = ($baseText -replace "`r`n", "`n").TrimEnd()
            $incomingProtected = ($incomingText -replace "`r`n", "`n").TrimEnd()
            if ($entry.path -ceq $DiagramPath) {
                $baseProtected = [regex]::Replace($baseProtected, $allowedRows, '')
                $incomingProtected = [regex]::Replace($incomingProtected, $allowedRows, '')
                foreach ($label in @('Harness Sensor', 'Acceptance evidence')) {
                    $pattern = '(?m)^\|\s*' + $label + '\s*\|'
                    if ([regex]::Matches($baseText, $pattern).Count -ne [regex]::Matches($incomingText, $pattern).Count -or
                        [regex]::Matches($incomingText, $pattern).Count -gt 1) { throw 'QA goal evidence rows must remain unique.' }
                }
            }
            if ($baseProtected -cne $incomingProtected) { throw 'QA changed protected goal or index content; only sensor and acceptance evidence rows may change.' }
            $goalChanges += @{ path = $entry.path; baseline = $baseText; incoming = $incomingText }
        }
    }
    $Evidence | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $evidencePath -Encoding utf8
    $edit = {
        param($path)
        . $stateLibrary
        . $readinessLibrary
        if ($Evidence.phase -eq 'QA' -and -not $RegistrationOnly) {
            $text = Get-Content -LiteralPath $path -Raw
            if ((Get-ProductionGoalDigest $text) -cne $VerifiedGoalDigest) { throw 'Goal requirements changed during QA verification; rerun QA.' }
            $liveQa = gh pr view $Evidence.number --repo $Evidence.repository --json headRefOid,headRefName,url,state | ConvertFrom-Json
            if ($LASTEXITCODE -ne 0 -or $liveQa.headRefOid -cne $VerifiedQaCommit -or $liveQa.headRefName -cne $Evidence.headBranch -or $liveQa.url -cne $Evidence.url -or $liveQa.state -notin @('OPEN','MERGED')) { throw 'QA head or PR identity changed during verification; rerun QA.' }
        }
        foreach ($change in $goalChanges) {
            $baseFile = Join-Path $directory 'base.md'
            $incomingFile = Join-Path $directory 'incoming.md'
            [IO.File]::WriteAllText($baseFile, ($change.baseline -replace "`r`n", "`n").TrimEnd() + "`n")
            [IO.File]::WriteAllText($incomingFile, ($change.incoming -replace "`r`n", "`n").TrimEnd() + "`n")
            $currentPath = Join-Path $pwd $change.path
            [IO.File]::WriteAllText($currentPath, ([IO.File]::ReadAllText($currentPath) -replace "`r`n", "`n").TrimEnd() + "`n")
            git merge-file -- $change.path $baseFile $incomingFile
            if ($LASTEXITCODE -ne 0) { throw 'QA goal edits conflict with a newer edit; published QA work is retained for reconciliation.' }
        }
        & $recorder -EvidenceDirectory $directory -CoordinatorRepository $Evidence.coordinatorRepository -CoordinatorBranch $Evidence.coordinatorBranch -DiagramPaths @($path) -GitHubAppSlug $env:SEQUENCE_GOAL_GITHUB_APP_SLUG -IosBaseBranch $(if ($env:SEQUENCE_GOAL_IOS_BASE_BRANCH) { $env:SEQUENCE_GOAL_IOS_BASE_BRANCH } else { 'develop' }) -AndroidBaseBranch $(if ($env:SEQUENCE_GOAL_ANDROID_BASE_BRANCH) { $env:SEQUENCE_GOAL_ANDROID_BASE_BRANCH } else { 'develop' }) -WebUiRepository $env:SEQUENCE_GOAL_WEBUI_REPOSITORY -WebUiBaseBranch $(if ($env:SEQUENCE_GOAL_WEBUI_BASE_BRANCH) { $env:SEQUENCE_GOAL_WEBUI_BASE_BRANCH } else { 'main' })
        if ($Evidence.phase -eq 'QA') {
            $state = [ordered]@{
                status = $(if ($RegistrationOnly) { 'InProgress' } else { 'Complete' })
                repository = $Evidence.repository
                baseBranch = $Evidence.baseBranch
                productionCommit = $Evidence.productionHeadSha
                branch = $Evidence.headBranch
                pullRequest = $Evidence.url
                verifiedCommit = $(if ($RegistrationOnly) { '' } else { $Evidence.productionHeadSha })
                verifiedQaCommit = $(if ($RegistrationOnly) { '' } else { $VerifiedQaCommit })
                goalDigest = $(if ($RegistrationOnly) { '' } else { $VerifiedGoalDigest })
                evidence = $(if ($env:GITHUB_RUN_ID) { "https://github.com/$($Evidence.coordinatorRepository)/actions/runs/$env:GITHUB_RUN_ID" } else { $Evidence.url })
                summary = $(if ($RegistrationOnly) { 'QA in progress; restart reuses the draft branch.' } else { 'QA changes and verification published in the linked PR.' })
            }
            $text = Get-Content -LiteralPath $path -Raw
            Set-Content -LiteralPath $path -Value (Set-GoalTaskState $text "$($Evidence.surface -replace '^QA-') QA" $state) -Encoding utf8
        }
    }.GetNewClosure()
    $hasIndexChange = @($goalChanges | Where-Object { $_.path -ceq 'docs/sequence-diagrams.md' }).Count -gt 0
    Update-SequenceGoalDocument -RepositoryRoot $sourceRoot -Branch $Evidence.coordinatorBranch -DiagramPath $DiagramPath -Edit $edit -IncludeIndex:$hasIndexChange -Message "chore(sequence-goal): record $($Evidence.surface) $($Evidence.phase)"
} finally {
    foreach ($name in @('delivery.json', 'base.md', 'incoming.md')) { Remove-Item -LiteralPath (Join-Path $directory $name) -ErrorAction SilentlyContinue }
    Remove-Item -LiteralPath $directory -ErrorAction SilentlyContinue
}
