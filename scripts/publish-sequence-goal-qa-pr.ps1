#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CoordinatorBranch,
    [Parameter(Mandatory)][ValidateSet('Backend', 'MobileIos', 'MobileAndroid', 'WebUi')][string]$Surface,
    [Parameter(Mandatory)][string]$Agent,
    [Parameter(Mandatory)][string]$DiagramPath,
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][string]$ProductionRepository,
    [Parameter(Mandatory)][int]$ProductionPrNumber,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{40}$')][string]$ProductionHeadSha,
    [string]$BaseBranch = 'main',
    [switch]$PrepareOnly
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1')
if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) { throw 'GH_TOKEN is required for authenticated QA Git and GitHub operations.' }
$existingGitConfigCount = 0
if (-not [string]::IsNullOrWhiteSpace($env:GIT_CONFIG_COUNT) -and
    (-not [int]::TryParse($env:GIT_CONFIG_COUNT, [ref]$existingGitConfigCount) -or $existingGitConfigCount -lt 0)) {
    throw 'GIT_CONFIG_COUNT must be a non-negative integer when supplied.'
}
$gitConfigNames = @('GIT_CONFIG_COUNT')
for ($index = 0; $index -lt $existingGitConfigCount; $index++) {
    $gitConfigNames += @("GIT_CONFIG_KEY_$index", "GIT_CONFIG_VALUE_$index")
}
$appendedGitConfigNames = @(
    "GIT_CONFIG_KEY_$existingGitConfigCount",
    "GIT_CONFIG_VALUE_$existingGitConfigCount",
    "GIT_CONFIG_KEY_$($existingGitConfigCount + 1)",
    "GIT_CONFIG_VALUE_$($existingGitConfigCount + 1)"
)
$priorGitConfig = @{}
foreach ($name in $gitConfigNames) {
    $item = Get-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue
    if ($null -ne $item) { $priorGitConfig[$name] = $item.Value }
}
try {
    $gitBasic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("x-access-token:$($env:GH_TOKEN)"))
    $env:GIT_CONFIG_COUNT = [string]($existingGitConfigCount + 2)
    Set-Item -LiteralPath "Env:GIT_CONFIG_KEY_$existingGitConfigCount" -Value 'http.https://github.com/.extraheader'
    Set-Item -LiteralPath "Env:GIT_CONFIG_VALUE_$existingGitConfigCount" -Value "AUTHORIZATION: basic $gitBasic"
    Set-Item -LiteralPath "Env:GIT_CONFIG_KEY_$($existingGitConfigCount + 1)" -Value 'core.hooksPath'
    Set-Item -LiteralPath "Env:GIT_CONFIG_VALUE_$($existingGitConfigCount + 1)" -Value 'NUL'
if ($Repository -notmatch '^[A-Za-z0-9][A-Za-z0-9_.-]*/[A-Za-z0-9][A-Za-z0-9_.-]*$') {
    throw "Repository must be a non-option owner/name value."
}
if ($ProductionRepository -notmatch '^VAAD1ST/[A-Za-z0-9][A-Za-z0-9_.-]*$' -or $ProductionPrNumber -le 0) {
    throw 'Production checkpoint must identify a valid VAAD1ST repository and positive PR number.'
}
if ([string]::IsNullOrWhiteSpace($BaseBranch) -or $BaseBranch.StartsWith('-')) {
    throw "BaseBranch must be a non-option Git branch name."
}
git check-ref-format --branch $BaseBranch 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) { throw "BaseBranch '$BaseBranch' is not a valid Git branch name." }

$repoRoot = (Resolve-Path (& git rev-parse --show-toplevel).Trim()).Path
$diagramRoot = Join-Path $repoRoot 'docs/sequence-diagrams'
foreach ($directory in @((Join-Path $repoRoot 'docs'), $diagramRoot)) {
    $item = Get-Item -LiteralPath $directory -Force -ErrorAction Stop
    if (-not $item.PSIsContainer -or $null -ne $item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "QA diagram directory '$directory' must be a regular directory."
    }
}
$fullSafeGoal = $CoordinatorBranch -replace '[^A-Za-z0-9_.-]', '-'
$safeGoal = $fullSafeGoal
$safeGoal = $safeGoal.Substring(0, [Math]::Min($safeGoal.Length, 150))
$surfaceSlug = $Surface.ToLowerInvariant()
$goalHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($CoordinatorBranch))).Substring(0, 10).ToLowerInvariant()
$branch = "sequence-goal/$($safeGoal.ToLowerInvariant())-qa-$surfaceSlug-$goalHash"
$derivedBranch = $branch
$retryBranchPattern = '^(?:' + [regex]::Escape($derivedBranch) + ')-retry-\d+-\d+$'
$diagramAbsolute = if ([System.IO.Path]::IsPathRooted($DiagramPath)) {
    (Resolve-Path -LiteralPath $DiagramPath).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repoRoot $DiagramPath)).Path
}
$rootWithSeparator = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
$rootUri = [System.Uri]::new($rootWithSeparator)
$diagramUri = [System.Uri]::new($diagramAbsolute)
$diagramItem = Get-Item -LiteralPath $diagramAbsolute -Force -ErrorAction Stop
if (-not $rootUri.IsBaseOf($diagramUri) -or [System.IO.Path]::GetExtension($diagramAbsolute) -ne '.md' -or
    (Split-Path -Parent $diagramAbsolute) -cne $diagramRoot -or $diagramItem.PSIsContainer -or
    $null -ne $diagramItem.LinkType -or ($diagramItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
    throw 'DiagramPath must identify a regular Markdown file directly under docs/sequence-diagrams.'
}
$diagramText = Get-Content -LiteralPath $diagramAbsolute -Raw -Encoding UTF8
$goalMatch = [regex]::Match($diagramText, '(?im)^\|\s*Goal\s*\|(?<value>[^|]+)\|\s*$')
$goalTitle = if ($goalMatch.Success) { $goalMatch.Groups['value'].Value.Trim().Trim('`') } else { [System.IO.Path]::GetFileNameWithoutExtension($DiagramPath) }
$coordinatorIdentity = "Associated sequence-goal delivery for ``$Repository@$CoordinatorBranch``."
$surfaceIdentity = "- Surface: ``$Surface``"
$phaseIdentity = '- Phase: `QA`'
$productionRepositoryIdentity = "- Production repository: ``$ProductionRepository``"
$productionPrIdentity = "- Production PR: ``$ProductionPrNumber``"
$productionShaIdentity = "- Production head SHA: ``$($ProductionHeadSha.ToLowerInvariant())``"
$hasExactLine = {
    param([string]$Body, [string]$Value)
    [regex]::IsMatch($Body, '(?m)^' + [regex]::Escape($Value) + '\s*$')
}
$hasCoordinatorIdentity = { param([string]$Body) [regex]::IsMatch($Body, ('(?m)^Associated sequence-goal delivery for `{{1,2}}{0}`{{1,2}}\.\s*$' -f [regex]::Escape("$Repository@$CoordinatorBranch"))) }
$hasCurrentMetadata = {
    param([string]$Body)
    (& $hasExactLine $Body $surfaceIdentity) -and (& $hasExactLine $Body $phaseIdentity) -and
    (& $hasExactLine $Body $productionRepositoryIdentity) -and (& $hasExactLine $Body $productionPrIdentity) -and
    (& $hasExactLine $Body $productionShaIdentity)
}
$trustedAuthors = @('github-actions', 'github-actions[bot]')
if (-not [string]::IsNullOrWhiteSpace($env:SEQUENCE_GOAL_GITHUB_APP_SLUG)) { $trustedAuthors += @($env:SEQUENCE_GOAL_GITHUB_APP_SLUG, "$($env:SEQUENCE_GOAL_GITHUB_APP_SLUG)[bot]", "app/$($env:SEQUENCE_GOAL_GITHUB_APP_SLUG)") }
function Assert-ProductionCurrent {
    . "$PSScriptRoot/sequence-goal-production-readiness.ps1"
    $current = gh pr view $ProductionPrNumber --repo $ProductionRepository --json state,headRefOid,body | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $current.state -notin @('OPEN', 'MERGED') -or
        -not (Test-ProductionReadiness -State (Read-DeliveryGoalState $Repository $CoordinatorBranch $Surface) -HeadSha $current.headRefOid) -or
        ([string]$current.headRefOid).ToLowerInvariant() -cne $ProductionHeadSha.ToLowerInvariant()) {
        throw 'Production checkpoint changed before QA publication; rerun QA against the current production PR.'
    }
}
$existingJson = gh pr list --repo $Repository --state open --limit 1000 --json number,url,state,isDraft,headRefName,baseRefName,body,author,headRepository
if ($LASTEXITCODE -ne 0) { throw 'Failed to discover existing QA delivery PRs.' }
$openPullRequests = @($existingJson | ConvertFrom-Json)
$candidates = @($openPullRequests | Where-Object {
    (& $hasCoordinatorIdentity ([string]$_.body)) -and
    ($_.headRefName -in @($derivedBranch) -or $_.headRefName -match $retryBranchPattern)
})
$matching = @($candidates | Where-Object {
    [void]($metadataMatches = (& $hasCurrentMetadata ([string]$_.body)))
    (& $hasCoordinatorIdentity ([string]$_.body)) -and $metadataMatches -and
    $_.author.login -in $trustedAuthors -and
    $_.headRepository.nameWithOwner -eq $Repository -and
    ($_.headRefName -in @($derivedBranch) -or $_.headRefName -match $retryBranchPattern) -and
    $_.baseRefName -eq $BaseBranch
})
$existing = @($matching | Where-Object state -eq 'OPEN')
if ($existing.Count -gt 1) { throw "Multiple open QA PRs found for $coordinatorIdentity and $Surface." }
if ($existing.Count -eq 1) { $branch = [string]$existing[0].headRefName }
if ($existing.Count -eq 0) {
    $ledgerSurface = "QA-$Surface"
    $surfaceRowExists = [regex]::IsMatch($diagramText, '(?im)^\|\s*``?' + [regex]::Escape($ledgerSurface) + '``?\s*\|')
    $ledgerPattern = '(?im)^\|\s*``?' + [regex]::Escape($ledgerSurface) + '``?\s*\|\s*``?(?<repository>[^`]+)``?\s*\|\s*``?(?<base>[^`]+)``?\s*\|\s*``?(?<head>[^`]+)``?\s*\|\s*\[#(?<number>\d+)\]\((?<url>[^)]+)\)'
    $ledgerMatch = [regex]::Match($diagramText, $ledgerPattern)
    if ($surfaceRowExists -and -not $ledgerMatch.Success) { throw "Recorded $ledgerSurface delivery row is malformed; refusing to create a replacement QA PR." }
    if ($ledgerMatch.Success -and ($ledgerMatch.Groups['repository'].Value -ne $Repository -or $ledgerMatch.Groups['base'].Value -ne $BaseBranch)) {
        throw "Recorded $ledgerSurface delivery row does not match the configured repository and base branch."
    }
    if ($ledgerMatch.Success) {
        $checkpointJson = gh pr view ([int]$ledgerMatch.Groups['number'].Value) --repo $Repository --json number,url,state,mergedAt,headRefName,baseRefName,body,author,headRepository
        if ($LASTEXITCODE -ne 0) { throw 'Failed to revalidate the recorded QA delivery checkpoint.' }
        $checkpoint = $checkpointJson | ConvertFrom-Json
        $checkpointMetadata = (& $hasCoordinatorIdentity ([string]$checkpoint.body)) -and
          (& $hasCurrentMetadata ([string]$checkpoint.body))
        if ($checkpoint.mergedAt -and $checkpoint.headRefName -eq $ledgerMatch.Groups['head'].Value -and $checkpoint.baseRefName -eq $BaseBranch -and
            $checkpoint.headRepository.nameWithOwner -eq $Repository -and [string]$checkpoint.url -ceq $ledgerMatch.Groups['url'].Value -and
            $checkpoint.author.login -in $trustedAuthors -and $checkpointMetadata) {
            $existing = @($checkpoint)
            $branch = [string]$checkpoint.headRefName
        }
    }
}

$untracked = @(git ls-files --others --exclude-standard | ForEach-Object { ($_ -replace '\\', '/') })
$allowed = @(
    @(git diff --name-only --no-renames HEAD)
    $untracked
) | ForEach-Object { ($_ -replace '\\', '/') } | Where-Object {
    $_ -match '^VAA\.EAPI\.Automation/' -and $_ -notmatch '^VAA\.EAPI\.Automation/TestResults/'
} | Sort-Object -Unique
if ($existing.Count -eq 1 -and $existing[0].mergedAt) {
    Write-Host 'A new QA attempt requires an open draft; the merged ledger checkpoint is retained only as history.'
    $existing = @()
    $branch = $derivedBranch
}

if (-not $PrepareOnly) {
$usagePath = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'TestResults/agent-resource') `
    -Filter "sequence-goal-loop-*-automationtests-$surfaceSlug-*.jsonl" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$attestedPaths = @()
if ($null -eq $usagePath) {
    throw "QA resource-usage evidence was not produced for $Surface."
}
$outcomePath = "$($usagePath.FullName).outcome.json"
if (-not (Test-Path -LiteralPath $outcomePath -PathType Leaf)) { throw "QA outcome evidence was not produced for $Surface." }
$outcome = Get-Content -LiteralPath $outcomePath -Raw | ConvertFrom-Json
if ($outcome.schemaVersion -ne 1 -or $outcome.phase -ne 'AutomationTests') { throw "QA outcome evidence is invalid for $Surface." }
$attestedPaths = @($outcome.existingCoveragePaths | ForEach-Object { [string]$_ -replace '\\', '/' } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
$resourceUsage = $null
$budgetPath = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'TestResults/agent-resource') `
    -Filter "sequence-goal-loop-*-automationtests-$surfaceSlug-*.budget.json" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if ($null -eq $budgetPath) { throw "QA resource evidence was not produced for $Surface." }
$resourceUsage = & (Join-Path $repoRoot 'scripts/get-agent-resource-summary.ps1') `
    -WorkflowId 'sequence-goal-loop' -BudgetEvidencePath $budgetPath.FullName | ConvertFrom-Json
if ($allowed.Count -eq 0 -and $attestedPaths.Count -eq 0) {
    throw "$Agent produced neither QA changes nor a valid existing-coverage attestation for $Surface."
}
}

Assert-ProductionCurrent
$registered = Read-DeliveryGoalState $Repository $CoordinatorBranch $Surface QA
if ($existing.Count -eq 0 -and $registered -and $registered.branch) {
    $history = @(gh pr list --repo $Repository --state all --head $registered.branch --limit 1000 --json number,state,mergedAt | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot validate registered QA branch PR history.' }
    if ($history.Count -gt 0) {
        # Missing-PR recovery is only for a branch that never had a PR. Closed
        # or merged attempts remain historical and must not seed new QA work.
        $registered = $null
        if (-not $PrepareOnly) { throw 'Registered QA branch has PR history; prepare a fresh QA attempt.' }
    }
}
if ($existing.Count -eq 0 -and $registered -and $registered.productionCommit -ceq $ProductionHeadSha -and $registered.repository -ceq $Repository -and $registered.baseBranch -ceq $BaseBranch) {
    $branch = $registered.branch
    if ($branch -notin @($derivedBranch) -and $branch -notmatch $retryBranchPattern) { throw 'Invalid registered QA branch.' }
}
if (-not $PrepareOnly -and $existing.Count -eq 0 -and (-not $registered -or $registered.branch -cne $branch -or $registered.productionCommit -cne $ProductionHeadSha)) { throw 'QA publication requires its registered goal-document branch.' }
if ($PrepareOnly -and $existing.Count -eq 1) {
    & "$PSScriptRoot/register-sequence-goal-branch.ps1" -CoordinatorRepository $Repository -CoordinatorBranch $CoordinatorBranch -Repository $Repository -BaseBranch $BaseBranch -Branch $branch -Phase QA -Surface $Surface -Agent $Agent -DiagramPath $DiagramPath -ProductionRepository $ProductionRepository -ProductionPrNumber $ProductionPrNumber -ProductionHeadSha $ProductionHeadSha -ExistingPr $existing[0]
    return
}

if ($PrepareOnly -or $existing.Count -eq 0 -or ($existing.Count -eq 1 -and -not $existing[0].mergedAt)) {
  if ($PrepareOnly) {
    $staleRemoteBranch = @(git ls-remote --heads origin "refs/heads/$branch")
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the remote QA branch '$branch'." }
    if ($staleRemoteBranch.Count -gt 1) { throw "Multiple remote refs unexpectedly matched QA branch '$branch'." }
    if ($staleRemoteBranch.Count -eq 1 -and (-not $registered -or $registered.branch -cne $branch -or $registered.productionCommit -cne $ProductionHeadSha)) {
        if ([string]::IsNullOrWhiteSpace($env:GITHUB_RUN_ID) -or [string]::IsNullOrWhiteSpace($env:GITHUB_RUN_ATTEMPT)) { throw 'GitHub run identity is required to allocate a collision-safe QA branch.' }
        $branch = "${derivedBranch}-retry-$($env:GITHUB_RUN_ID)-$($env:GITHUB_RUN_ATTEMPT)"
        git check-ref-format --branch $branch 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Collision-safe QA branch '$branch' is invalid." }
        $retryRemoteBranch = @(git ls-remote --heads origin "refs/heads/$branch")
        if ($LASTEXITCODE -ne 0) { throw "Failed to inspect collision-safe QA branch '$branch'." }
        if ($retryRemoteBranch.Count -ne 0) { throw "Collision-safe QA branch '$branch' already exists; rerun with a new attempt." }
    }
  }
    git fetch --no-tags origin "+refs/heads/${BaseBranch}:refs/remotes/origin/${BaseBranch}"
    if ($LASTEXITCODE -ne 0) { throw "Failed to refresh QA base branch '$BaseBranch'." }
    $worktreeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "sequence-goal-qa-$([guid]::NewGuid().ToString('N'))"
    try {
      if ($PrepareOnly) {
        $start = "origin/$BaseBranch"
        if ($registered -and $registered.branch -ceq $branch -and $staleRemoteBranch.Count -eq 1) {
            git fetch origin "refs/heads/$branch" | Out-Host
            if ($LASTEXITCODE -ne 0) { throw 'Cannot fetch registered QA branch.' }
            $start = 'FETCH_HEAD'
        }
        git -c core.longpaths=true worktree add --detach $worktreeRoot $start | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Failed to create isolated QA worktree for '$branch'." }
        Push-Location $worktreeRoot
        try {
            & "$PSScriptRoot/register-sequence-goal-branch.ps1" -CoordinatorRepository $Repository -CoordinatorBranch $CoordinatorBranch -Repository $Repository -BaseBranch $BaseBranch -Branch $branch -Phase QA -Surface $Surface -Agent $Agent -DiagramPath $DiagramPath -ProductionRepository $ProductionRepository -ProductionPrNumber $ProductionPrNumber -ProductionHeadSha $ProductionHeadSha -RecoverPushedBranch:($registered -and $registered.branch -ceq $branch -and $staleRemoteBranch.Count -eq 1)
        } finally { Pop-Location }
        return
      }
        git fetch --no-tags origin "+refs/heads/${branch}:refs/remotes/origin/${branch}"
        if ($LASTEXITCODE -ne 0) { throw 'Failed to fetch registered QA branch.' }
        git -c core.longpaths=true worktree add --detach $worktreeRoot "origin/$branch"
        if ($LASTEXITCODE -ne 0) { throw 'Failed to check out registered QA head.' }
        if ($allowed.Count -gt 0) {
            foreach ($file in @($allowed | Where-Object { $_ -in $untracked })) { git add -N -- $file }
            if ($LASTEXITCODE -ne 0) { throw 'Failed to stage untracked QA changes for patch generation.' }
            $patchPath = Join-Path ([IO.Path]::GetTempPath()) "qa-patch-$([guid]::NewGuid().ToString('N')).diff"
            try {
                git diff --binary --output=$patchPath HEAD -- $allowed
                if ($LASTEXITCODE -ne 0) { throw 'Cannot prepare QA changes.' }
                git -C $worktreeRoot apply --reverse --check $patchPath 2>$null
                if ($LASTEXITCODE -ne 0) {
                    git -C $worktreeRoot apply --3way --index --whitespace=nowarn $patchPath
                    if ($LASTEXITCODE -ne 0) { throw 'QA changes conflict with published work; resolve the registered branch before restarting.' }
                }
            } finally { Remove-Item $patchPath -ErrorAction SilentlyContinue }
        } else {
            $evidenceRelative = "VAA.EAPI.Automation/docs/sequence-goal-deliveries/$($safeGoal.ToLowerInvariant())-$surfaceSlug-$goalHash.md"
            $evidenceAbsolute = Join-Path $worktreeRoot $evidenceRelative
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $evidenceAbsolute) | Out-Null
            @(
                '# Existing automation coverage attestation'
                ''
                "- Coordinator: ``$CoordinatorBranch``"
                "- Surface: ``$Surface``"
                "- QA agent: ``$Agent``"
                "- Goal: ``$DiagramPath``"
                "- Existing coverage: $($attestedPaths -join ', ')"
            ) | Set-Content -LiteralPath $evidenceAbsolute -Encoding UTF8
            git -C $worktreeRoot add -- $evidenceRelative
            if ($LASTEXITCODE -ne 0) { throw 'Failed to stage QA attestation evidence.' }
        }
        git -C $worktreeRoot config user.name 'github-actions[bot]'
        git -C $worktreeRoot config user.email 'github-actions[bot]@users.noreply.github.com'
        git -C $worktreeRoot diff --cached --quiet
        if ($LASTEXITCODE -eq 1) {
            git -C $worktreeRoot commit -m "test(sequence-goal): deliver $Surface QA coverage"
            if ($LASTEXITCODE -ne 0) { throw 'Failed to commit QA delivery changes.' }
        } elseif ($LASTEXITCODE -ne 0) { throw 'Cannot inspect QA changes.' }
        if ($existing.Count -eq 1 -and -not $existing[0].isDraft) {
            gh pr ready $existing[0].number --repo $Repository --undo
            if ($LASTEXITCODE -ne 0) { throw 'Cannot return QA PR to draft; replacement head was not pushed.' }
        }
        git -C $worktreeRoot push origin "HEAD:refs/heads/$branch"
        if ($LASTEXITCODE -ne 0) { throw 'Failed to push QA delivery branch.' }
    } finally {
        git -c core.longpaths=true worktree remove --force $worktreeRoot 2>$null
    }

    $body = @(
        "Associated sequence-goal delivery for ``$Repository@$CoordinatorBranch``."
        ''
        '## Delivered outcome'
        ''
        "Automation coverage for: $goalTitle"
        ''
        '## Delivery ownership'
        ''
        '- Phase: `QA`'
        "- Surface: ``$Surface``"
        "- Agent: ``$Agent``"
        $productionRepositoryIdentity
        $productionPrIdentity
        $productionShaIdentity
        "- Goal: ``$DiagramPath``"
        "- Base branch: ``$BaseBranch``"
        ''
        '## Delivered automation'
        ''
        $(if ($allowed.Count -gt 0) { @($allowed | ForEach-Object { "- ``$_``" }) } else { @($attestedPaths | ForEach-Object { "- Existing coverage: ``$_``" }) })
        ''
        'This PR is independently monitored for checks, reviews, comments, conflicts, and lifecycle cost evidence.'
        ''
        '## Review scope'
        ''
        'Review only regressions introduced by this PR relative to its base branch that affect the stated delivery acceptance criteria.'
        ''
        'Treat as blocking only correctness, security, trust-boundary, data-loss, or lifecycle-integrity defects introduced or materially changed by this PR. Do not request unrelated hardening, architectural improvements, pre-existing fixes, stylistic changes, or speculative edge cases here; record those as follow-up work.'
        ''
        'When re-reviewing, verify the fixes made for existing findings. Do not restart an unrestricted whole-system redesign review.'
        ''
        '## Agent resources and cost'
        ''
        "- Maximum budget: $($resourceUsage.maximumLabel)"
        "- Actual QA-agent usage: $($resourceUsage.actualLabel)"
    ) -join "`n"
    $qaScope = switch ($Surface) {
        'MobileIos' { 'ios' }
        'MobileAndroid' { 'android' }
        'WebUi' { 'web' }
        default { 'api' }
    }
    $qaTitle = "test($qaScope)`: $goalTitle"
    if ($qaTitle.Length -gt 120) { $qaTitle = "$($qaTitle.Substring(0, 117).TrimEnd())..." }
    Assert-ProductionCurrent
    $bodyPath = Join-Path ([IO.Path]::GetTempPath()) "sequence-goal-qa-body-$([guid]::NewGuid().ToString('N')).md"
    try {
        Set-Content -LiteralPath $bodyPath -Value $body -Encoding UTF8
        if ($existing.Count -eq 0) {
            gh pr create --repo $Repository --base $BaseBranch --head $branch --draft --title $qaTitle --body-file $bodyPath | Out-Host
            if ($LASTEXITCODE -ne 0) { throw 'Cannot create QA PR; restart from the registered branch.' }
            $existing = @(gh pr view $branch --repo $Repository --json number,url | ConvertFrom-Json)
            if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve published QA PR.' }
        }
        gh pr edit $existing[0].number --repo $Repository --title $qaTitle --body-file $bodyPath
        if ($LASTEXITCODE -ne 0) { throw 'Failed to update registered QA delivery PR.' }
        # Verification runs on the published head next. Keep this PR draft until
        # the verified completion entry has been committed to the goal document.
    } finally { Remove-Item -LiteralPath $bodyPath -ErrorAction SilentlyContinue }
    $createdJson = gh pr list --repo $Repository --state open --base $BaseBranch --head $branch --json number,url,state,mergedAt,headRefName,baseRefName,body,author,headRepository
    if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the created QA delivery PR.' }
    $existing = @($createdJson | ConvertFrom-Json)
    if ($existing.Count -ne 1 -or $existing[0].headRefName -ne $branch -or $existing[0].baseRefName -ne $BaseBranch -or
        $existing[0].headRepository.nameWithOwner -ne $Repository -or $existing[0].author.login -notin $trustedAuthors -or
        -not (& $hasCoordinatorIdentity ([string]$existing[0].body)) -or
        -not (& $hasExactLine ([string]$existing[0].body) $surfaceIdentity) -or
        -not (& $hasExactLine ([string]$existing[0].body) $phaseIdentity) -or
        -not (& $hasExactLine ([string]$existing[0].body) $productionRepositoryIdentity) -or
        -not (& $hasExactLine ([string]$existing[0].body) $productionPrIdentity) -or
        -not (& $hasExactLine ([string]$existing[0].body) $productionShaIdentity)) {
        throw 'Created QA delivery PR did not match its authenticated repository, author, branch, base, coordinator, and surface identity.'
    }
}

foreach ($file in $allowed) {
    git reset --quiet HEAD -- $file 2>$null
    if ($file -in $untracked) {
        Remove-Item -LiteralPath $file -Force
    } else {
        # `git restore` was introduced after Git 2.21, which is still used by
        # supported local automation workstations. The index was reset above,
        # so checkout restores only this temporary-worktree file to HEAD.
        git checkout -- $file
    }
}

Assert-ProductionCurrent
$pr = $existing[0]
$evidenceDirectory = Join-Path $repoRoot 'TestResults/associated-prs'
New-Item -ItemType Directory -Force -Path $evidenceDirectory | Out-Null
[ordered]@{
    surface = "QA-$Surface"
    phase = 'QA'
    productionHeadSha = $ProductionHeadSha
    productionRepository = $ProductionRepository
    productionNumber = $ProductionPrNumber
    coordinatorRepository = $Repository
    coordinatorBranch = $CoordinatorBranch
    agent = $Agent
    repository = $Repository
    baseBranch = $BaseBranch
    headBranch = $branch
    number = [int]$pr.number
    url = [string]$pr.url
    state = $(if ($pr.mergedAt) { 'Merged' } else { [string]$pr.state })
    mergedAt = [string]$pr.mergedAt
    resourceUsage = $resourceUsage
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $evidenceDirectory "QA-$Surface.json") -Encoding UTF8

Write-Host "$Agent QA delivery is tracked by $($pr.url)." -ForegroundColor Green
} finally {
    foreach ($name in $appendedGitConfigNames) {
        Remove-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue
    }
    foreach ($name in $gitConfigNames) {
        if ($priorGitConfig.ContainsKey($name)) { Set-Item -LiteralPath "Env:$name" -Value $priorGitConfig[$name] }
        else { Remove-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue }
    }
}
