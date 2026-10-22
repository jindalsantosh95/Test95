#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$EvidenceDirectory,
    [Parameter(Mandatory)][string]$CoordinatorRepository,
    [Parameter(Mandatory)][string]$CoordinatorBranch,
    [string]$IosBaseBranch = 'develop',
    [string]$AndroidBaseBranch = 'develop',
    [string]$WebUiRepository,
    [string]$WebUiBaseBranch = 'main',
    [string]$GitHubAppSlug,
    [Parameter(Mandatory)]
    [ValidateScript({
        if ([System.IO.Path]::IsPathRooted($_) -or $_ -notmatch '^docs[\\/]sequence-diagrams[\\/](?!_)[A-Za-z0-9_.-]+\.md$') {
            throw "Diagram path '$_' must be a repository-relative Markdown leaf under docs/sequence-diagrams/."
        }
        $true
    })]
    [string[]]$DiagramPaths
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (& git rev-parse --show-toplevel).Trim()).Path
$diagramRoot = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'docs/sequence-diagrams')).Path
$validatedDiagramPaths = @($DiagramPaths | ForEach-Object {
    $candidate = Join-Path $repoRoot $_
    $resolvedCandidate = (Resolve-Path -LiteralPath $candidate).Path
    $candidateItem = Get-Item -LiteralPath $resolvedCandidate
    if ($null -ne $candidateItem.LinkType) {
        $resolvedCandidate = $candidateItem.ResolveLinkTarget($true).FullName
    }
    $resolvedParent = (Resolve-Path -LiteralPath (Split-Path -Parent $resolvedCandidate)).Path
    if (-not $resolvedParent.Equals($diagramRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Diagram path '$_' resolves outside docs/sequence-diagrams/." }
    [System.IO.Path]::GetRelativePath($repoRoot, $resolvedCandidate) -replace '\\', '/'
})
$evidence = @(Get-ChildItem -LiteralPath $EvidenceDirectory -Filter '*.json' -File -ErrorAction SilentlyContinue | Sort-Object Name | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
})
$trustedAuthors = @('github-actions', 'github-actions[bot]')
if (-not [string]::IsNullOrWhiteSpace($GitHubAppSlug)) { $trustedAuthors += @($GitHubAppSlug, "${GitHubAppSlug}[bot]", "app/$GitHubAppSlug") }
$productionTargets = @{
    Backend = @{ repository = $CoordinatorRepository; base = 'main' }
    MobileIos = @{ repository = 'VAAD1ST/vaa-mobile-ios'; base = $IosBaseBranch }
    MobileAndroid = @{ repository = 'VAAD1ST/vaa-mobile-android'; base = $AndroidBaseBranch }
    WebUi = @{ repository = $WebUiRepository; base = $WebUiBaseBranch }
}
function Test-AssociatedPrMetadata {
    param([Parameter(Mandatory)][string]$Body, [Parameter(Mandatory)][string]$Phase, [Parameter(Mandatory)][string]$Surface)
    $coordinatorIdentity = "Associated sequence-goal delivery for ``$CoordinatorRepository@$CoordinatorBranch``."
    $hasExactLine = {
        param([string]$Value)
        [regex]::IsMatch($Body, "(?m)^$([regex]::Escape($Value))\s*$")
    }
    $escapedSurface = [regex]::Escape($Surface)
    $escapedPhase = [regex]::Escape($Phase)
    $currentMetadata = [regex]::IsMatch($Body, ('(?m)^- Surface: `{{1,2}}{0}`{{1,2}}\s*$' -f $escapedSurface)) -and
        [regex]::IsMatch($Body, ('(?m)^- Phase: `{{1,2}}{0}`{{1,2}}\s*$' -f $escapedPhase))
    (& $hasExactLine $coordinatorIdentity) -and $currentMetadata
}
foreach ($item in $evidence) {
    $surface = [string]$item.surface
    $phase = [string]$item.phase
    $deliverySurface = $surface -replace '^QA-', ''
    $target = if ($phase -eq 'QA') { @{ repository = $CoordinatorRepository; base = 'main' } } else { $productionTargets[$deliverySurface] }
    if ($surface -notin @('Backend','MobileIos','MobileAndroid','WebUi','QA-Backend','QA-MobileIos','QA-MobileAndroid','QA-WebUi') -or
        $phase -notin @('Production','QA') -or ($surface.StartsWith('QA-') -ne ($phase -eq 'QA')) -or
        [string]$item.coordinatorRepository -cne $CoordinatorRepository -or [string]$item.coordinatorBranch -cne $CoordinatorBranch -or
        $null -eq $target -or [string]::IsNullOrWhiteSpace([string]$target.repository) -or
        [string]$item.repository -cne [string]$target.repository -or [string]$item.baseBranch -cne [string]$target.base -or
        [string]::IsNullOrWhiteSpace([string]$item.headBranch) -or [int]$item.number -le 0) {
        throw "Associated PR evidence for '$surface' has invalid workflow identity fields."
    }
    $prNumber = [int]$item.number
    $repository = [string]$item.repository
    $stderrPath = Join-Path ([System.IO.Path]::GetTempPath()) "sequence-goal-associated-pr-$prNumber-$([guid]::NewGuid().ToString('N')).stderr"
    try {
        $prJson = gh pr view $prNumber --repo $repository --json number,url,state,mergedAt,headRefName,baseRefName,body,author,headRepository 2> $stderrPath
        $ghExitCode = $LASTEXITCODE
        $ghError = if (Test-Path -LiteralPath $stderrPath) { ((Get-Content -LiteralPath $stderrPath -Raw) ?? '').Trim() } else { '' }
    } finally {
        Remove-Item -LiteralPath $stderrPath -Force -ErrorAction SilentlyContinue
    }
    if ($ghExitCode -ne 0) { throw "Failed to resolve associated PR $repository#$prNumber via gh (exit $ghExitCode): $ghError" }
    $pr = $prJson | ConvertFrom-Json
    if ($pr.number -ne [int]$item.number -or $pr.url -ne [string]$item.url -or
        $pr.headRefName -ne [string]$item.headBranch -or $pr.baseRefName -ne [string]$item.baseBranch -or
        $pr.headRepository.nameWithOwner -ne [string]$item.repository -or $pr.author.login -notin $trustedAuthors -or
        -not (Test-AssociatedPrMetadata -Body ([string]$pr.body) -Phase $phase -Surface $deliverySurface)) {
        throw "Associated PR evidence for '$surface' did not match the authenticated PR identity."
    }
    if ($pr.state -eq 'CLOSED' -and [string]::IsNullOrWhiteSpace([string]$pr.mergedAt)) { throw "Associated PR evidence for '$surface' is closed without merge and cannot be recorded as an active checkpoint." }
    $item.state = if ($pr.mergedAt) { 'Merged' } else { [string]$pr.state }
    $item | Add-Member -NotePropertyName mergedAt -NotePropertyValue ([string]$pr.mergedAt) -Force
}
foreach ($diagram in $validatedDiagramPaths) {
    $diagramPath = Join-Path $repoRoot $diagram
    $text = Get-Content -LiteralPath $diagramPath -Raw
    $sectionPattern = '(?ms)^## Associated delivery pull requests\s*$.*?(?=^## |\z)'
    $sectionMatch = [regex]::Match($text, $sectionPattern)
    $rows = [ordered]@{}
    $coordinatorRow = $null
    if ($sectionMatch.Success) {
        $legacyEntries = @([regex]::Matches($sectionMatch.Value, '(?im)^-\s+\[#\d+:[^\]]+\]\([^)]+\)\s*$'))
        if ($legacyEntries.Count -gt 0) {
            throw "Associated delivery PR ledger contains legacy bullet entries in $diagram; migrate them explicitly before table-based delivery recording."
        }
        foreach ($row in [regex]::Matches($sectionMatch.Value, '(?im)^\|\s*``?(?<surface>Backend|MobileIos|MobileAndroid|WebUi|QA-(?:Backend|MobileIos|MobileAndroid|WebUi))``?\s*\|.*$')) {
            $rowSurface = $row.Groups['surface'].Value
            if ($rows.Contains($rowSurface)) { throw "Associated delivery PR ledger contains duplicate active rows for $rowSurface in $diagram." }
            $rows[$rowSurface] = $row.Value.TrimEnd()
        }
        $coordinatorMatch = [regex]::Match($sectionMatch.Value, '(?im)^\|\s*`?Coordinator`?\s*\|.*$')
        if ($coordinatorMatch.Success) { $coordinatorRow = $coordinatorMatch.Value.TrimEnd() }
        $recognizedRows = @($rows.Values) + @($coordinatorRow)
        foreach ($tableLine in @($sectionMatch.Value -split "`r?`n" | Where-Object { $_ -match '^\s*\|' })) {
            $normalizedLine = $tableLine.TrimEnd()
            if ($normalizedLine -match '^\s*\|\s*Surface\s*\|' -or
                $normalizedLine -match '^\s*\|(?:\s*:?-+:?\s*\|)+\s*$' -or
                $normalizedLine -in $recognizedRows) { continue }
            throw "Associated delivery PR ledger contains an unrecognized or malformed table row in $diagram; refusing to discard it."
        }
    }
    if ($evidence.Count -eq 0 -and $rows.Count -eq 0) {
        throw "No associated delivery PR evidence or existing ledger rows were available to record for $diagram."
    }
    $historyPattern = '(?ms)^## Historical associated delivery pull requests\s*$.*?(?=^## |\z)'
    $historyMatch = [regex]::Match($text, $historyPattern)
    $history = @()
    if ($historyMatch.Success) {
        $history += @([regex]::Matches($historyMatch.Value, '(?m)^- .+$') | ForEach-Object { $_.Value })
    }
    foreach ($item in $evidence) {
        $surface = [string]$item.surface
        if ($rows.Contains($surface)) {
            $old = [regex]::Match($rows[$surface], '^\|\s*``?[^`]+``?\s*\|\s*``?(?<repository>[^`]+)``?\s*\|\s*``?(?<base>[^`]+)``?\s*\|\s*``?(?<head>[^`]+)``?\s*\|\s*\[#(?<number>\d+)\]\((?<url>[^)]+)\)\s*\|\s*``?(?<state>[^`]+)``?\s*\|\s*(?<merged>[^|]+)\|\s*$')
            if (-not $old.Success) { throw "Existing associated delivery row for $surface is malformed in $diagram; refusing to overwrite its PR identity." }
            if ($old.Groups['repository'].Value -ne [string]$item.repository -or [int]$old.Groups['number'].Value -ne [int]$item.number) {
                $oldRepository = $old.Groups['repository'].Value
                $oldNumber = [int]$old.Groups['number'].Value
                $oldPhase = if ($surface.StartsWith('QA-')) { 'QA' } else { 'Production' }
                $oldDeliverySurface = $surface -replace '^QA-', ''
                $oldTarget = if ($oldPhase -eq 'QA') { @{ repository = $CoordinatorRepository; base = 'main' } } else { $productionTargets[$oldDeliverySurface] }
                if ($null -eq $oldTarget -or [string]::IsNullOrWhiteSpace([string]$oldTarget.repository) -or
                    $oldRepository -cne [string]$oldTarget.repository -or $old.Groups['base'].Value -cne [string]$oldTarget.base) {
                    throw "Superseded associated PR target for $surface is outside the configured repository/base allowlist."
                }
                $oldPrJson = gh pr view $oldNumber --repo $oldRepository --json number,url,state,mergedAt,headRefName,baseRefName,body,author,headRepository
                if ($LASTEXITCODE -ne 0) { throw "Failed to verify superseded associated PR $($old.Groups['repository'].Value)#$($old.Groups['number'].Value)." }
                $oldPr = $oldPrJson | ConvertFrom-Json
                if ($null -eq $oldTarget -or [string]::IsNullOrWhiteSpace([string]$oldTarget.repository) -or
                    $oldRepository -cne [string]$oldTarget.repository -or $old.Groups['base'].Value -cne [string]$oldTarget.base -or
                    $oldPr.number -ne $oldNumber -or $oldPr.url -ne $old.Groups['url'].Value -or
                    $oldPr.headRefName -ne $old.Groups['head'].Value -or $oldPr.baseRefName -ne $old.Groups['base'].Value -or
                    $oldPr.headRepository.nameWithOwner -ne $oldRepository -or $oldPr.author.login -notin $trustedAuthors -or
                    -not (Test-AssociatedPrMetadata -Body ([string]$oldPr.body) -Phase $oldPhase -Surface $oldDeliverySurface)) {
                    throw "Superseded associated PR $oldRepository#$oldNumber did not match the authenticated ledger identity for $surface."
                }
                if ($oldPr.state -eq 'OPEN' -or (-not [string]::IsNullOrWhiteSpace([string]$oldPr.mergedAt) -and $oldPhase -ne 'QA')) {
                    throw "Cannot replace active or merged production PR $($old.Groups['repository'].Value)#$($old.Groups['number'].Value) for $surface."
                }
                $oldMergedAt = if ([string]::IsNullOrWhiteSpace([string]$oldPr.mergedAt)) { 'n/a' } else { [string]$oldPr.mergedAt }
                $oldState = if ($oldMergedAt -eq 'n/a') { [string]$oldPr.state } else { 'Merged' }
                $history += "- ``$surface``: [$($old.Groups['repository'].Value)#$($old.Groups['number'].Value)]($($old.Groups['url'].Value)) — ``$oldState``; merged at $oldMergedAt."
            }
        }
        $mergedAt = if ([string]::IsNullOrWhiteSpace([string]$item.mergedAt)) { 'n/a' } else { [string]$item.mergedAt }
        $rows[$surface] = "| ``$surface`` | ``$($item.repository)`` | ``$($item.baseBranch)`` | ``$($item.headBranch)`` | [#$($item.number)]($($item.url)) | ``$($item.state)`` | $mergedAt |"
    }
    $table = @(
        '## Associated delivery pull requests'
        ''
        '| Surface | Repository | Base branch | Head branch | PR | State | Merged at UTC |'
        '| --- | --- | --- | --- | --- | --- | --- |'
    ) + @($rows.GetEnumerator() | Sort-Object @{ Expression = {
        $surfaceOrder = @('Backend', 'MobileIos', 'MobileAndroid', 'WebUi', 'QA-Backend', 'QA-MobileIos', 'QA-MobileAndroid', 'QA-WebUi')
        $index = [array]::IndexOf($surfaceOrder, [string]$_.Key)
        if ($index -lt 0) { [int]::MaxValue } else { $index }
    } }, Key | ForEach-Object Value)
    if ($null -ne $coordinatorRow) { $table += $coordinatorRow }
    $section = ($table -join "`n") + "`n"
    if ($sectionMatch.Success) { $text = [regex]::Replace($text, $sectionPattern, { param($match) $section }) }
    else { $text = $text.TrimEnd() + "`n`n$section" }
    $history = @($history | Sort-Object -Unique)
    if ($history.Count -gt 0) {
        $historySection = "## Historical associated delivery pull requests`n`n" + ($history -join "`n") + "`n"
        if ($text -match $historyPattern) { $text = [regex]::Replace($text, $historyPattern, { param($match) $historySection }) }
        else { $text = $text.TrimEnd() + "`n`n$historySection" }
    }
    Set-Content -LiteralPath $diagramPath -Value $text -Encoding UTF8
}
