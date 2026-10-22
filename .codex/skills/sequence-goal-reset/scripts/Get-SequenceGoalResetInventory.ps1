#Requires -Version 7.0
<#
.SYNOPSIS
    Inventories one or more sequence-goal branches before a destructive reset.

.DESCRIPTION
    Performs read-only Git and GitHub queries. It identifies changed goal documents,
    coordinator pull requests, resolved associated PR rows, delivery changes, and
    agent-loop evidence without closing PRs, deleting branches, or rewriting refs.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string[]]$GoalBranch,

    [ValidateNotNullOrEmpty()]
    [string]$Remote = 'origin',

    [ValidateNotNullOrEmpty()]
    [string]$DefaultBaseBranch = 'main'
)

$ErrorActionPreference = 'Stop'

function Invoke-TextCommand {
    param(
        [Parameter(Mandatory)][string]$Command,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $stderrPath = [IO.Path]::GetTempFileName()
    try {
        $output = & $Command @Arguments 2> $stderrPath
        $exitCode = $LASTEXITCODE
        $stderr = @(Get-Content -LiteralPath $stderrPath -ErrorAction SilentlyContinue)
        if ($exitCode -ne 0) {
            $argumentText = $Arguments -join ' '
            throw "$Command $argumentText failed with exit code ${exitCode}: $($stderr -join [Environment]::NewLine)"
        }
    } finally {
        Remove-Item -LiteralPath $stderrPath -Force -ErrorAction SilentlyContinue
    }

    return @($output | ForEach-Object { "$_" })
}

function Get-RepositoryName {
    param([Parameter(Mandatory)][string]$RemoteName)

    $url = @(Invoke-TextCommand -Command git -Arguments @('remote', 'get-url', '--', $RemoteName))[0]
    $match = [regex]::Match($url, '(?i)(?:github\.com[:/])(?<name>[^/]+/[^/]+?)(?:\.git)?$')
    if (-not $match.Success) {
        throw "Remote '$RemoteName' is not a recognizable GitHub repository URL: $url"
    }

    return $match.Groups['name'].Value
}

function ConvertFrom-JsonArray {
    param([string[]]$InputText)

    $raw = $InputText -join [Environment]::NewLine
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @()
    }

    return @($raw | ConvertFrom-Json)
}

function Test-GitBranchName {
    param([Parameter(Mandatory)][string]$BranchName)

    & git check-ref-format --branch $BranchName 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

function Test-GitRemoteName {
    param([Parameter(Mandatory)][string]$RemoteName)

    if ($RemoteName.StartsWith('-')) {
        return $false
    }

    & git check-ref-format "refs/remotes/$RemoteName/validation-probe" 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

function Test-GitObjectExists {
    param([Parameter(Mandatory)][string]$ObjectName)

    & git cat-file -e $ObjectName 2>$null
    return $LASTEXITCODE -eq 0
}

foreach ($command in @('git', 'gh')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Required command '$command' is not available on PATH."
    }
}

Invoke-TextCommand -Command git -Arguments @('rev-parse', '--show-toplevel') | Out-Null
if (-not (Test-GitRemoteName -RemoteName $Remote)) {
    throw "Remote is not a valid Git remote name: $Remote"
}
$repository = Get-RepositoryName -RemoteName $Remote
$repositoryOwner = $repository.Split('/')[0]
$results = New-Object System.Collections.Generic.List[object]

if (-not (Test-GitBranchName -BranchName $DefaultBaseBranch)) {
    throw "Default base branch is not a valid Git branch name: $DefaultBaseBranch"
}

foreach ($branch in $GoalBranch) {
    if ([string]::IsNullOrWhiteSpace($branch)) {
        continue
    }

    $branch = $branch.Trim()
    try {
        if (-not (Test-GitBranchName -BranchName $branch)) {
            throw "Goal branch is not a valid Git branch name: $branch"
        }

    Invoke-TextCommand -Command git -Arguments @(
        'fetch', '--no-tags', '--', $Remote,
        "+refs/heads/${DefaultBaseBranch}:refs/remotes/$Remote/$DefaultBaseBranch",
        "+refs/heads/${branch}:refs/remotes/$Remote/$branch") | Out-Null

    $remoteBranch = "$Remote/$branch"
    $coordinatorPr = ConvertFrom-JsonArray -InputText (Invoke-TextCommand -Command gh -Arguments @(
        'pr', 'list', '--repo', $repository, '--state', 'all', '--head', $branch,
        '--json', 'number,state,isDraft,mergedAt,closedAt,headRefName,baseRefName,url,title'))
    $baseCandidates = @($coordinatorPr |
        ForEach-Object { $_.baseRefName } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique)
    $baseResolution = if ($baseCandidates.Count -eq 1) {
        'CoordinatorPullRequest'
    } elseif ($baseCandidates.Count -eq 0) {
        'DefaultFallback'
    } else {
        'AmbiguousCoordinatorPullRequests'
    }
    $baseBranch = if ($baseCandidates.Count -eq 1) { $baseCandidates[0] } else { $DefaultBaseBranch }
    if (-not (Test-GitBranchName -BranchName $baseBranch)) {
        throw "Resolved base is not a valid Git branch name: $baseBranch"
    }

    if ($baseBranch -ne $DefaultBaseBranch) {
        Invoke-TextCommand -Command git -Arguments @(
            'fetch', '--no-tags', '--', $Remote,
            "+refs/heads/${baseBranch}:refs/remotes/$Remote/$baseBranch") | Out-Null
    }

    $baseRef = "$Remote/$baseBranch"
    $changedFiles = Invoke-TextCommand -Command git -Arguments @('diff', '--name-only', "$baseRef...$remoteBranch")
    $goalDocuments = New-Object System.Collections.Generic.List[string]
    $deletedGoalDocuments = New-Object System.Collections.Generic.List[string]
    $associatedRows = New-Object System.Collections.Generic.List[object]

    foreach ($path in @($changedFiles | Where-Object { $_ -match '^docs/sequence-diagrams/(?!_template)[^/]+\.md$' })) {
        if (-not (Test-GitObjectExists -ObjectName "${remoteBranch}:$path")) {
            $deletedGoalDocuments.Add($path)
            continue
        }

        $text = (Invoke-TextCommand -Command git -Arguments @('show', "${remoteBranch}:$path")) -join [Environment]::NewLine
        if ($text -notmatch '(?im)^##\s+Agent Loop Goal\s*$') {
            continue
        }

        $goalDocuments.Add($path)
        $associatedSection = [regex]::Match($text, '(?is)^##\s+Associated delivery pull requests\s*$\s*(?<body>.*?)(?=^##\s+|\z)')
        if ($associatedSection.Success) {
            foreach ($row in [regex]::Matches(
                $associatedSection.Groups['body'].Value,
                '(?im)^\|\s*`?(?<surface>[^|`]+)`?\s*\|\s*(?<repository>[^|]+?)\s*\|\s*`?(?<base>[^|`]+)`?\s*\|\s*`?(?<head>[^|`]+)`?\s*\|\s*(?<pr>[^|]+?)\s*\|\s*`?(?<state>[^|`]+)`?\s*\|')) {
                if ($row.Groups['surface'].Value.Trim() -in @('Surface', '---')) {
                    continue
                }

                $associatedRows.Add([pscustomobject]@{
                    goalDocument = $path
                    surface = $row.Groups['surface'].Value.Trim()
                    repository = $row.Groups['repository'].Value.Trim().Trim('`')
                    baseBranch = $row.Groups['base'].Value.Trim()
                    headBranch = $row.Groups['head'].Value.Trim()
                    pullRequest = $row.Groups['pr'].Value.Trim()
                    state = $row.Groups['state'].Value.Trim()
                })
            }
        }
    }

    $associatedPullRequests = New-Object System.Collections.Generic.List[object]
    foreach ($associatedRow in $associatedRows) {
        $prUrlMatch = [regex]::Match(
            $associatedRow.pullRequest,
            '(?i)github\.com/(?<repository>[^/]+/[^/]+)/pull/(?<number>\d+)')
        $prNumberMatch = [regex]::Match($associatedRow.pullRequest, '^#?(?<number>[1-9]\d*)$')
        $prRepository = if ($prUrlMatch.Success) {
            $prUrlMatch.Groups['repository'].Value
        } else {
            $associatedRow.repository
        }
        $prNumber = if ($prUrlMatch.Success) {
            $prUrlMatch.Groups['number'].Value
        } elseif ($prNumberMatch.Success) {
            $prNumberMatch.Groups['number'].Value
        } else {
            $null
        }

        $resolvedPr = $null
        $resolutionError = $null
        if ([string]::IsNullOrWhiteSpace($prNumber) -or $prRepository -notmatch '^[^/\s]+/[^/\s]+$') {
            $resolutionError = 'The ledger row has no resolvable repository and PR number.'
        } else {
            try {
                $resolvedPr = ((Invoke-TextCommand -Command gh -Arguments @(
                    'pr', 'view', $prNumber, '--repo', $prRepository,
                    '--json', 'number,state,isDraft,mergedAt,closedAt,headRefName,baseRefName,url,title')) -join [Environment]::NewLine) |
                    ConvertFrom-Json
            } catch {
                $resolutionError = $_.Exception.Message
            }
        }

        $associatedPullRequests.Add([pscustomobject]@{
            goalDocument = $associatedRow.goalDocument
            surface = $associatedRow.surface
            recordedRepository = $associatedRow.repository
            recordedBaseBranch = $associatedRow.baseBranch
            recordedHeadBranch = $associatedRow.headBranch
            recordedState = $associatedRow.state
            pullRequest = $resolvedPr
            resolutionError = $resolutionError
        })
    }

    $coordinatorMarker = "$repository@$branch"
    $metadataPullRequests = @(@(
        foreach ($state in @('open', 'closed')) {
            ConvertFrom-JsonArray -InputText (Invoke-TextCommand -Command gh -Arguments @(
                'search', 'prs', $coordinatorMarker, '--owner', $repositoryOwner,
                '--state', $state, '--match', 'body', '--limit', '100',
            '--json', 'number,url,state,isDraft,title,body,repository,closedAt'))
        }
    ) |
        Where-Object { $_.body -match [regex]::Escape($coordinatorMarker) } |
        Select-Object number, url, state, isDraft, title, repository, closedAt)
    $ledgerPullRequestKeys = @($associatedPullRequests | ForEach-Object {
        if ($null -ne $_.pullRequest) {
            "$($_.pullRequest.url)".TrimEnd('/')
        }
    })
    $unledgeredMetadataPullRequests = @($metadataPullRequests | Where-Object {
        "$($_.url)".TrimEnd('/') -notin $ledgerPullRequestKeys
    })
    $ledgerError = if ($unledgeredMetadataPullRequests.Count -gt 0) {
        "Associated delivery PR metadata was found outside the goal ledger: $($unledgeredMetadataPullRequests.url -join ', ')"
    } else {
        $null
    }

    $targetingGoalBranchPrs = ConvertFrom-JsonArray -InputText (Invoke-TextCommand -Command gh -Arguments @(
        'pr', 'list', '--repo', $repository, '--state', 'all', '--base', $branch,
        '--json', 'number,state,isDraft,mergedAt,closedAt,headRefName,baseRefName,url,title'))
    $divergence = (@(Invoke-TextCommand -Command git -Arguments @('rev-list', '--left-right', '--count', "$baseRef...$remoteBranch"))[0] -split '\s+')
    $evidenceFiles = @($changedFiles | Where-Object {
        $_ -match '^\.codex/agent-loop/' -or
        $_ -match '(^|/)TestResults/' -or
        $_ -match '(?i)(^|/)(evidence|run-results?)(/|$)'
    })
    $deliveryFiles = @($changedFiles | Where-Object {
        $_ -notmatch '^docs/' -and $_ -notin $evidenceFiles
    })
    $remoteHead = @(Invoke-TextCommand -Command git -Arguments @('rev-parse', $remoteBranch))[0]

        $results.Add([pscustomobject]@{
            goalBranch = $branch
            inventoryError = $ledgerError
            baseBranch = $baseBranch
            baseResolution = $baseResolution
            baseCandidates = $baseCandidates
            behindBase = [int]$divergence[0]
            aheadOfBase = [int]$divergence[1]
            remoteHead = $remoteHead
            goalDocuments = $goalDocuments.ToArray()
            deletedGoalDocuments = $deletedGoalDocuments.ToArray()
            coordinatorPullRequests = @($coordinatorPr)
            pullRequestsTargetingGoalBranch = @($targetingGoalBranchPrs)
            associatedPullRequestRows = $associatedRows.ToArray()
            associatedPullRequests = $associatedPullRequests.ToArray()
            metadataPullRequests = $metadataPullRequests
            unledgeredMetadataPullRequests = $unledgeredMetadataPullRequests
            evidenceFiles = $evidenceFiles
            deliveryFiles = $deliveryFiles
            changedFiles = @($changedFiles)
        })
    } catch {
        $results.Add([pscustomobject]@{
            goalBranch = $branch
            inventoryError = $_.Exception.Message
        })
    }
}

$results | ConvertTo-Json -Depth 8
