[CmdletBinding()]
param(
    [string]$RequestedSurface = '',
    [switch]$RequireProductionCheckpoint,
    [switch]$SmokeTest
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1')
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
. (Join-Path $PSScriptRoot 'sequence-goal-documentation-maintenance.ps1')
if ([string]::IsNullOrWhiteSpace($env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY)) {
    throw 'SEQUENCE_GOAL_COORDINATOR_REPOSITORY is required.'
}
$ErrorActionPreference = 'Stop'
git fetch --no-tags --prune origin +refs/heads/main:refs/remotes/origin/main
$fetchExitCode = $LASTEXITCODE
if ($fetchExitCode -ne 0) { throw 'Failed to fetch origin/main before resolving delivery goals.' }
$changedPaths = @(git diff --name-only origin/main...HEAD)
if ($LASTEXITCODE -ne 0) { throw 'Failed to determine changed paths for delivery routing.' }
if (-not $SmokeTest -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_REF_NAME)) {
    $pullRequests = @(gh pr list --repo $env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY --head $env:GITHUB_REF_NAME --base main --state open --json headRefName,baseRefName,isCrossRepository,labels | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0) { throw 'Failed to read PR documentation-maintenance intent.' }
    if (Test-SequenceGoalDocumentationMaintenance -PullRequests $pullRequests -Branch $env:GITHUB_REF_NAME -ChangedPaths $changedPaths) {
        if ($RequireProductionCheckpoint -or -not [string]::IsNullOrWhiteSpace($RequestedSurface)) {
            throw 'Documentation-only PRs cannot request production or QA delivery continuation.'
        }
        @('documentation_only=true', 'delivery_surfaces=', 'development_agent=', 'qa_agent=',
          'qa_matrix={"include":[]}', 'qa_count=0', 'remote_matrix={"include":[]}', 'remote_count=0',
          'has_diagrams=false', 'has_index=false', 'has_inprogress=false', 'inprogress_list=') |
            Out-File -FilePath $env:GITHUB_OUTPUT -Append
        Write-Host 'Documentation-maintenance PR: implementation dispatch is not applicable; normal PR checks remain required.'
        return
    }
}
'documentation_only=false' | Out-File -FilePath $env:GITHUB_OUTPUT -Append
$diagramDirectory = 'docs/sequence-diagrams'
foreach ($directory in @('docs', $diagramDirectory)) {
  $item = Get-Item -LiteralPath $directory -Force -ErrorAction Stop
  if (-not $item.PSIsContainer -or $null -ne $item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Sequence-goal directory '$directory' must be a regular directory." }
}
$diagrams = @(
    git diff --name-only origin/main...HEAD |
    Where-Object {
      $_ -like 'docs/sequence-diagrams/*.md' -and
      ((Split-Path -Leaf $_) -notmatch '(?i)^(_template|template)') -and
      (Test-Path -LiteralPath $_ -PathType Leaf) -and
      ($null -eq (Get-Item -LiteralPath $_ -Force).LinkType) -and
      -not ((Get-Item -LiteralPath $_ -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)
    }
)
$smokeDiagram = 'docs/sequence-diagrams/agent-loop-smoke-test.md'
if (-not $SmokeTest -and (Test-Path -LiteralPath $smokeDiagram -PathType Leaf)) {
    throw "A smoke-test fixture remains on the branch outside a smoke dispatch. Rerun the smoke workflow to complete cleanup."
}
if ($SmokeTest) {
    $unexpectedSmokeDiagrams = @($diagrams | Where-Object {
        if ($_ -eq $smokeDiagram) { return $false }
        $candidateText = Get-Content -LiteralPath $_ -Raw
        if ($candidateText -match '(?im)^\|\s*Final state\s*\|\s*`Abandoned`\s*\|\s*$') { return $false }
        $candidateStatus = [regex]::Match($candidateText, '(?im)^\|\s*Status\s*\|\s*`(?<value>[^`]+)`\s*\|\s*$')
        $candidateStatus.Success -and $candidateStatus.Groups['value'].Value.Trim() -in @('Ready', 'InProgress')
    })
    if ($unexpectedSmokeDiagrams.Count -gt 0) {
        throw "Smoke mode cannot run while non-smoke sequence goals are actionable: $($unexpectedSmokeDiagrams -join ';')"
    }
}
$surfaces = @()
$actionableSurfaces = @()
$actionableDiagrams = @()
$inProgressDiagrams = @()
$completedProductionSurfaces = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$completedQaSurfaces = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$recordedProductionSurfaces = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$productionCheckpoints = @{}
foreach ($diagram in $diagrams) {
  $text = Get-Content -LiteralPath $diagram -Raw
  if ($text -match '(?im)^\|\s*Final state\s*\|\s*`Abandoned`\s*\|\s*$') { continue }
  $status = [regex]::Match($text, '(?im)^\|\s*Status\s*\|\s*`(?<value>[^`]+)`\s*\|\s*$')
  if (-not $status.Success -or $status.Groups['value'].Value.Trim() -notin @('Ready', 'InProgress')) { continue }
  $actionableDiagrams += $diagram
    if ($status.Groups['value'].Value.Trim() -eq 'InProgress') { $inProgressDiagrams += $diagram }
    if ($SmokeTest) { continue }
  $surfaceMatch = [regex]::Match($text, '(?im)^\|\s*Delivery surface\s*\|(?<value>[^|]*)\|\s*$')
  $declared = if ($surfaceMatch.Success) { $surfaceMatch.Groups['value'].Value.Trim().Trim('`').Trim() } else { 'Backend' }
  if ([string]::IsNullOrWhiteSpace($declared)) {
    throw "Sequence goal '$diagram' has an empty Delivery surface. Use Backend, MobileIos, MobileAndroid, or WebUi."
  }
  $actionableSurfaces += @($declared -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
  $ledgerRows = [regex]::Matches($text, '(?im)^\|\s*``?(?<surface>Backend|MobileIos|MobileAndroid|WebUi|QA-(?:Backend|MobileIos|MobileAndroid|WebUi))``?\s*\|\s*``?(?<repository>[^`]+)``?\s*\|\s*``?(?<base>[^`]+)``?\s*\|\s*``?(?<head>[^`]+)``?\s*\|\s*\[#(?<number>\d+)\]\([^)]+\)\s*\|\s*``?(?<state>[^`]+)``?\s*\|\s*(?<merged>[^|]+)\|\s*$')
  foreach ($row in $ledgerRows) {
    $repository = $row.Groups['repository'].Value.Trim()
    $number = [int]$row.Groups['number'].Value
    $ledgerSurface = $row.Groups['surface'].Value
    $expectedPhase = if ($ledgerSurface.StartsWith('QA-')) { 'QA' } else { 'Production' }
    $expectedSurface = $ledgerSurface -replace '^QA-', ''
    $expectedBase = $row.Groups['base'].Value.Trim()
    $configuredRepository = if ($expectedPhase -eq 'QA') { $env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY } else { switch ($expectedSurface) {
      'Backend' { $env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY }
      'MobileIos' { 'VAAD1ST/vaa-mobile-ios' }
      'MobileAndroid' { 'VAAD1ST/vaa-mobile-android' }
      'WebUi' { $env:SEQUENCE_GOAL_WEBUI_REPOSITORY }
    } }
    $configuredBase = if ($expectedPhase -eq 'QA') { 'main' } else { switch ($expectedSurface) {
      'Backend' { 'main' }
      'MobileIos' { $env:SEQUENCE_GOAL_IOS_BASE_BRANCH }
      'MobileAndroid' { $env:SEQUENCE_GOAL_ANDROID_BASE_BRANCH }
      'WebUi' { $env:SEQUENCE_GOAL_WEBUI_BASE_BRANCH }
    } }
    if ([string]::IsNullOrWhiteSpace($configuredBase)) { $configuredBase = if ($expectedSurface -eq 'WebUi') { 'main' } else { 'develop' } }
    if ($repository -ne $configuredRepository -or $expectedBase -ne $configuredBase) {
      throw "Associated PR $repository#$number does not match the configured target for $expectedSurface."
    }
    $current = gh pr view $number --repo $repository --json state,mergedAt,baseRefName,headRefName,headRefOid,body,author,headRepository | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw "Cannot resolve current state for associated PR $repository#$number." }
    $appSlug = $env:SEQUENCE_GOAL_GITHUB_APP_SLUG; $trustedAuthors = @('github-actions', 'github-actions[bot]'); if ($appSlug) { $trustedAuthors += @($appSlug, "${appSlug}[bot]", "app/$appSlug") }
    $coordinatorIdentity = "Associated sequence-goal delivery for ``$($env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY)@$($env:GITHUB_REF_NAME)``."
    $hasExactLine = { param([string]$Body, [string]$Value) [regex]::IsMatch($Body, '(?m)^' + [regex]::Escape($Value) + '\s*$') }
    $currentMetadata = [regex]::IsMatch([string]$current.body, ('(?m)^- Phase: `{{1,2}}{0}`{{1,2}}\s*$' -f [regex]::Escape($expectedPhase))) -and
      [regex]::IsMatch([string]$current.body, ('(?m)^- Surface: `{{1,2}}{0}`{{1,2}}\s*$' -f [regex]::Escape($expectedSurface)))
    $authenticatedMetadata = (& $hasExactLine ([string]$current.body) $coordinatorIdentity) -and $currentMetadata
    if (-not $authenticatedMetadata -or
        $current.author.login -notin $trustedAuthors -or
        $current.headRepository.nameWithOwner -ne $repository -or
        $current.headRefName -ne $row.Groups['head'].Value.Trim() -or
        $current.baseRefName -ne $expectedBase) {
      throw "Merged associated PR $repository#$number does not match its authenticated coordinator, phase, surface, configured repository, and base identity."
    }
    if ($expectedPhase -eq 'Production' -and $current.state -eq 'CLOSED' -and [string]::IsNullOrWhiteSpace([string]$current.mergedAt)) { continue }
    if ($expectedPhase -eq 'Production') {
      [void]$recordedProductionSurfaces.Add($expectedSurface)
      $productionCheckpoints[$expectedSurface] = [ordered]@{ repository = $repository; number = $number; headSha = ([string]$current.headRefOid).ToLowerInvariant() }
    } else {
      $productionCheckpoint = $productionCheckpoints[$expectedSurface]
      $qaMatchesProduction = $null -ne $productionCheckpoint -and
        (& $hasExactLine ([string]$current.body) "- Production repository: ``$($productionCheckpoint.repository)``") -and
        (& $hasExactLine ([string]$current.body) "- Production PR: ``$($productionCheckpoint.number)``") -and
        (& $hasExactLine ([string]$current.body) "- Production head SHA: ``$($productionCheckpoint.headSha)``")
      if (-not $qaMatchesProduction) { continue }
    }
    if ([string]::IsNullOrWhiteSpace([string]$current.mergedAt)) { continue }
    $goalDigest = Get-ProductionGoalDigest $text
    if ($ledgerSurface.StartsWith('QA-')) {
      if (-not (Test-QaReadiness (Get-GoalTaskState $text "$expectedSurface QA") $productionCheckpoint.headSha $current.headRefOid $goalDigest)) { continue }
      [void]$completedQaSurfaces.Add($ledgerSurface.Substring(3))
    } else {
      if (-not (Test-ProductionReadiness (Get-GoalTaskState $text "$expectedSurface Production") $current.headRefOid $goalDigest)) { continue }
      [void]$completedProductionSurfaces.Add($ledgerSurface)
    }
  }
}
if (-not $SmokeTest -and $actionableDiagrams.Count -gt 1) {
  throw "Sequence-goal branches must own exactly one actionable Ready/InProgress goal diagram. Found $($actionableDiagrams.Count): $($actionableDiagrams -join ';')"
}
if ($actionableSurfaces.Count -gt 0) {
  $surfaces = @()
  $seenSurfaces = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
  foreach ($surface in $actionableSurfaces) {
    $canonicalSurface = switch ($surface) {
      'Backend' { 'Backend' }
      'MobileIos' { 'MobileIos' }
      'MobileAndroid' { 'MobileAndroid' }
      'WebUi' { 'WebUi' }
      default { throw "Unsupported Delivery surface '$surface'. Use Backend, MobileIos, MobileAndroid, or WebUi." }
    }
    if ($seenSurfaces.Add($canonicalSurface)) { $surfaces += $canonicalSurface }
  }
}
$requestedQaSurface = $RequestedSurface.Trim()
if (-not [string]::IsNullOrWhiteSpace($requestedQaSurface)) {
  if ($requestedQaSurface -notin @('Backend', 'MobileIos', 'MobileAndroid', 'WebUi')) {
    throw "Unsupported qa_surface '$requestedQaSurface'."
  }
  if ($requestedQaSurface -notin $surfaces) {
    throw "QA continuation surface '$requestedQaSurface' is not declared by the active goal."
  }
  if ($RequireProductionCheckpoint -and -not $recordedProductionSurfaces.Contains($requestedQaSurface)) {
    throw "QA continuation surface '$requestedQaSurface' has no authenticated production PR checkpoint."
  }
  $surfaces = @($requestedQaSurface)
}
$agentMap = @{}
foreach ($surface in $surfaces) {
  $agentMap[$surface] = switch ($surface) {
  'Backend'       { @('dev-jim-api-ai', 'qa-rob-api-ai') }
  'MobileIos'     { @('dev-jim-ios-ai', 'qa-rob-ios-ai') }
  'MobileAndroid' { @('dev-mike-android-ai', 'qa-rob-android-ai') }
  'WebUi'         { @('dev-mike-web-ai', 'qa-rob-web-ai') }
  default { throw "Unsupported Delivery surface '$surface'. Use Backend, MobileIos, MobileAndroid, or WebUi." }
  }
}
$developmentAgents = @($surfaces | ForEach-Object { $agentMap[$_][0] })
$qaAgents = @($surfaces | ForEach-Object { $agentMap[$_][1] })
$remoteTargets = @()
$qaTargets = @()
foreach ($selectedSurface in $surfaces) {
  $repository = switch ($selectedSurface) {
    'Backend' { $env:SEQUENCE_GOAL_COORDINATOR_REPOSITORY }
    'MobileIos' { 'VAAD1ST/vaa-mobile-ios' }
    'MobileAndroid' { 'VAAD1ST/vaa-mobile-android' }
    'WebUi' { $env:SEQUENCE_GOAL_WEBUI_REPOSITORY }
  }
  if ([string]::IsNullOrWhiteSpace($repository)) {
    throw "Delivery surface $selectedSurface requires repository variable SEQUENCE_GOAL_WEBUI_REPOSITORY."
  }
  $parts = $repository.Split('/', 2)
  if ($parts.Count -ne 2 -or @($parts | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
    throw "Invalid owner/repository mapping for ${selectedSurface}: '$repository'."
  }
  if ($parts[0] -ne 'VAAD1ST') {
    throw "Delivery surface $selectedSurface repository '$repository' must belong to VAAD1ST so the workflow GitHub App token can access it."
  }
  $baseBranch = switch ($selectedSurface) {
    'Backend' { 'main' }
    'MobileIos' { $env:SEQUENCE_GOAL_IOS_BASE_BRANCH }
    'MobileAndroid' { $env:SEQUENCE_GOAL_ANDROID_BASE_BRANCH }
    'WebUi' { $env:SEQUENCE_GOAL_WEBUI_BASE_BRANCH }
  }
  if ([string]::IsNullOrWhiteSpace($baseBranch)) {
    $baseBranch = if ($selectedSurface -eq 'WebUi') { 'main' } else { 'develop' }
  }
  if (-not $completedProductionSurfaces.Contains($selectedSurface)) {
    $remoteTargets += [ordered]@{
      surface = $selectedSurface
      repository = $repository
      owner = $parts[0]
      repo_name = $parts[1]
      base_branch = $baseBranch
      agent = $agentMap[$selectedSurface][0]
    }
  }
  if (-not $completedQaSurfaces.Contains($selectedSurface)) {
    $qaTargets += [ordered]@{
      surface = $selectedSurface
      repository = $repository
      base_branch = $baseBranch
      agent = $agentMap[$selectedSurface][1]
    }
  }
}
"delivery_surfaces=$($surfaces -join ';')" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"development_agent=$($developmentAgents -join ' + ')" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"qa_agent=$($qaAgents -join ' + ')" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"qa_matrix=$(@{ include = @($qaTargets) } | ConvertTo-Json -Depth 5 -Compress)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"qa_count=$($qaTargets.Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"remote_matrix=$(@{ include = @($remoteTargets) } | ConvertTo-Json -Depth 5 -Compress)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"remote_count=$($remoteTargets.Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"has_diagrams=$(if ($actionableDiagrams.Count -gt 0) { 'true' } else { 'false' })" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"has_index=$(if (@(git diff --name-only origin/main...HEAD) -contains 'docs/sequence-diagrams.md') { 'true' } else { 'false' })" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"has_inprogress=$(if ($inProgressDiagrams.Count -gt 0) { 'true' } else { 'false' })" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
"inprogress_list=$($inProgressDiagrams -join ';')" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
Write-Host "Delivery surfaces $($surfaces -join ', ') route to $($developmentAgents -join ', '), then $($qaAgents -join ', ')."
