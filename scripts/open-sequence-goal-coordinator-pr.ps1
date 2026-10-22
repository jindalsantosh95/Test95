[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string[]]$DiagramPaths,

    [Parameter(Mandatory)]
    [string]$Repository,

    [Parameter(Mandatory)]
    [string]$CoordinatorBranch,

    [string]$BaseBranch = 'main',

    [string]$GitHubAppSlug
)

$ErrorActionPreference = 'Stop'

if ($Repository -notmatch '^[^/]+/[^/]+$') {
    throw "Repository must use owner/name form; received '$Repository'."
}
if ([string]::IsNullOrWhiteSpace($CoordinatorBranch)) {
    throw 'CoordinatorBranch is required.'
}
if ($CoordinatorBranch -eq $BaseBranch) {
    throw 'The coordinator branch must differ from its base branch.'
}

$diagramDirectory = 'docs/sequence-diagrams'
foreach ($ancestor in @('docs', $diagramDirectory)) {
    $item = Get-Item -LiteralPath $ancestor -Force -ErrorAction Stop
    if ($null -ne $item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or -not $item.PSIsContainer) {
        throw "Sequence-goal diagram directory '$ancestor' must be a regular directory."
    }
}

$resolvedDiagrams = @($DiagramPaths | ForEach-Object {
    $item = Get-Item -LiteralPath $_ -Force -ErrorAction SilentlyContinue
    if ($null -eq $item -or -not (Test-Path -LiteralPath $_ -PathType Leaf)) {
        throw "Sequence-goal diagram '$_' does not exist."
    }
    if ($null -ne $item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or ((Split-Path -Parent $_) -replace '\\','/') -ne $diagramDirectory) { throw "Sequence-goal diagram '$_' must be a regular file directly under docs/sequence-diagrams." }
    $_
})
if ($resolvedDiagrams.Count -ne 1) {
    throw "Expected exactly one coordinator diagram; received $($resolvedDiagrams.Count)."
}

$diagramPath = $resolvedDiagrams[0]
$diagramText = Get-Content -LiteralPath $diagramPath -Raw
$status = [regex]::Match($diagramText, '(?im)^\|\s*Status\s*\|\s*`(?<value>[^`]+)`\s*\|\s*$')
if (-not $status.Success -or $status.Groups['value'].Value.Trim() -ne 'InProgress') {
    throw "Coordinator PR publication requires an InProgress goal in '$diagramPath'."
}
$goal = [regex]::Match($diagramText, '(?im)^\|\s*Goal\s*\|(?<value>[^|]+)\|\s*$')
$goalTitle = if ($goal.Success) { $goal.Groups['value'].Value.Trim().Trim('`') } else { [IO.Path]::GetFileNameWithoutExtension($diagramPath) }
$title = "feat(sequence-goal): $goalTitle"
if ($title.Length -gt 120) { $title = "$($title.Substring(0, 117).TrimEnd())..." }

$marker = 'Automated PR raised by the sequence goal loop for active goal delivery.'
$monitorMarker = 'Automated PR raised by the sequence goal loop after implementation verification.'
$completionMarker = 'Automated PR raised by the sequence goal loop after goal completion.'
$trustedAuthors = @('github-actions', 'github-actions[bot]')
if (-not [string]::IsNullOrWhiteSpace($GitHubAppSlug)) {
    $trustedAuthors += @($GitHubAppSlug, "$GitHubAppSlug[bot]", "app/$GitHubAppSlug")
}
$existing = @(gh pr list --repo $Repository --state open --base $BaseBranch --head $CoordinatorBranch --limit 10 --json number,url,state,isDraft,headRefName,baseRefName,body,author,headRepository | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect existing coordinator PRs.' }
$reusable = @($existing | Where-Object {
    $_.headRefName -eq $CoordinatorBranch -and
    $_.baseRefName -eq $BaseBranch -and
    $_.headRepository.nameWithOwner -eq $Repository -and
    $_.author.login -in $trustedAuthors -and
    (([string]$_.body).Contains($marker, [StringComparison]::Ordinal) -or
     ([string]$_.body).Contains($monitorMarker, [StringComparison]::Ordinal))
})
if ($existing.Count -ne $reusable.Count -or $reusable.Count -gt 1) {
    throw 'Open coordinator PR failed trusted author, repository, branch, base, or marker validation.'
}

$body = @(
    "<!-- $marker -->"
    "<!-- $monitorMarker -->"
    "<!-- $completionMarker -->"
    'Automated draft coordinator PR for an active sequence goal.'
    ''
    'This PR is the live coordination record for the delivery. It remains draft while production and QA child PRs are delivered, reviewed, remediated, and merged.'
    ''
    '## Overall deliverable'
    ''
    $goalTitle
    ''
    '## Delivery state'
    ''
    "- Goal document: ``$diagramPath``"
    "- Coordinator branch: ``$CoordinatorBranch``"
    '- Status: `InProgress`'
    '- Child PRs, verification, and agent resource costs are tracked in the goal document.'
) -join "`n"

if ($reusable.Count -eq 0) {
    $url = gh pr create --repo $Repository --base $BaseBranch --head $CoordinatorBranch --title $title --body $body --draft
    if ($LASTEXITCODE -ne 0) { throw 'Failed to create the draft coordinator PR.' }
    $pr = gh pr view $url --repo $Repository --json number,url,state,isDraft,headRefName,baseRefName,author,headRepository | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the created coordinator PR.' }
} else {
    $pr = $reusable[0]
    gh pr edit $pr.number --repo $Repository --title $title | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to refresh coordinator PR #$($pr.number)." }
    if (-not ([string]$pr.body).Contains($completionMarker, [StringComparison]::Ordinal)) {
        $compatibleBody = "$($pr.body)`n<!-- $completionMarker -->"
        gh pr edit $pr.number --repo $Repository --body $compatibleBody | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Failed to restore coordinator lifecycle marker on PR #$($pr.number)." }
    }
    if (-not $pr.isDraft) {
        gh pr ready $pr.number --repo $Repository --undo | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Failed to return active coordinator PR #$($pr.number) to draft." }
        $pr.isDraft = $true
    }
}

if ($pr.headRefName -ne $CoordinatorBranch -or $pr.baseRefName -ne $BaseBranch -or $pr.headRepository.nameWithOwner -ne $Repository -or $pr.author.login -notin $trustedAuthors) {
    throw 'Coordinator PR identity verification failed after create/update.'
}

$row = "| ``Coordinator`` | ``$Repository`` | ``$BaseBranch`` | ``$CoordinatorBranch`` | [#$($pr.number)]($($pr.url)) | ``OPEN`` | n/a |"
$pattern = '(?im)^\|\s*`Coordinator`\s*\|.*$'
if ([regex]::IsMatch($diagramText, $pattern)) {
    $diagramText = [regex]::Replace(
        $diagramText,
        $pattern,
        [Text.RegularExpressions.MatchEvaluator]{ param($match) $row }
    )
} else {
    $heading = '## Associated delivery pull requests'
    $headingIndex = $diagramText.IndexOf($heading, [StringComparison]::Ordinal)
    if ($headingIndex -lt 0) { throw "'$diagramPath' has no associated delivery pull requests section." }
    $nextHeading = $diagramText.IndexOf("`n## ", $headingIndex + $heading.Length, [StringComparison]::Ordinal)
    $insertAt = if ($nextHeading -lt 0) { $diagramText.Length } else { $nextHeading }
    $diagramText = "$($diagramText.Substring(0, $insertAt).TrimEnd())`n$row`n`n$($diagramText.Substring($insertAt).TrimStart())"
}
[IO.File]::WriteAllText((Resolve-Path -LiteralPath $diagramPath), $diagramText, [Text.UTF8Encoding]::new($false))

[ordered]@{
    number = [int]$pr.number
    url = [string]$pr.url
    draft = $true
    diagram = $diagramPath
} | ConvertTo-Json -Compress
