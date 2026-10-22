#Requires -Version 5.1
[CmdletBinding()]
param([string]$BaseRef = 'main', [string]$ScopePath = 'VAA.EAPI.Automation')

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
& git -C $repoRoot rev-parse --verify --quiet $BaseRef *> $null
if ($LASTEXITCODE -ne 0) {
    $BaseRef = "origin/$BaseRef"
    & git -C $repoRoot rev-parse --verify --quiet $BaseRef *> $null
    if ($LASTEXITCODE -ne 0) { throw "Base ref '$BaseRef' was not found." }
}

$mergeBaseOutput = @(& git -C $repoRoot merge-base $BaseRef HEAD | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$mergeBase = if ($mergeBaseOutput.Count -gt 0) { [string]$mergeBaseOutput[0] } else { '' }
$comparison = if ([string]::IsNullOrWhiteSpace($mergeBase)) { "$BaseRef..HEAD" } else { "$mergeBase...HEAD" }
$candidatePattern = '^VAA\.EAPI\.Automation/[^/]+\.UI/(Tests|Features|Flows)/.+\.cs$'
$excludedSetupPattern = '/Tests/Setup/'
$statusLines = @(& git -C $repoRoot diff --name-status --diff-filter=AMR $comparison -- $ScopePath)
$changedFiles = @{}
$addedFiles = @{}
foreach ($line in $statusLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $columns = @($line -split "`t")
    $status = [string]$columns[0]
    $relativePath = [string]$columns[-1]
    if ($relativePath -match $candidatePattern -and $relativePath -notmatch $excludedSetupPattern) {
        $changedFiles[$relativePath] = $true
        if ($status -like 'A*') { $addedFiles[$relativePath] = $true }
    }
}

$stagedStatusLines = @(& git -C $repoRoot diff --cached --name-status --diff-filter=AMR -- $ScopePath)
foreach ($line in $stagedStatusLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $columns = @($line -split "`t")
    $status = [string]$columns[0]
    $relativePath = [string]$columns[-1]
    if ($relativePath -match $candidatePattern -and $relativePath -notmatch $excludedSetupPattern) {
        $changedFiles[$relativePath] = $true
        if ($status -like 'A*') { $addedFiles[$relativePath] = $true }
    }
}

$untrackedFiles = @(& git -C $repoRoot ls-files --others --exclude-standard -- $ScopePath |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and $_ -match $candidatePattern -and $_ -notmatch $excludedSetupPattern })
foreach ($relativePath in $untrackedFiles) {
    $changedFiles[$relativePath] = $true
    $addedFiles[$relativePath] = $true
}

$architectureFitnessRelativePath = 'VAA.EAPI.Automation/VAA.EAPI.Automation.ArchitectureTests/ArchitectureFitnessTests.cs'
$architectureFitnessText = [string]((& git -C $repoRoot show "${BaseRef}:$architectureFitnessRelativePath") -join "`n")
if ($LASTEXITCODE -ne 0) { throw "Unable to load the established UI driver-token policy from '$BaseRef'." }
if ([string]::IsNullOrWhiteSpace($architectureFitnessText)) { throw 'The established UI driver-token policy source is empty.' }
$tokenBlock = [regex]::Match(
    $architectureFitnessText,
    'ForbiddenUiDriverTokens\s*=\s*new\[\]\s*\{(?<tokens>.*?)\};',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $tokenBlock.Success) { throw 'Unable to load the established UI driver-token policy.' }
$forbiddenTokens = @([regex]::Matches($tokenBlock.Groups['tokens'].Value, '"(?<token>(?:[^"\\]|\\.)*)"') |
    ForEach-Object { [regex]::Unescape($_.Groups['token'].Value) } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($forbiddenTokens.Count -eq 0) { throw 'The established UI driver-token policy contains no tokens.' }

function Get-ChangedText {
    param([Parameter(Mandatory)][string]$RelativePath)

    $absolutePath = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "Changed UI source '$RelativePath' was not found."
    }

    if ($addedFiles.ContainsKey($RelativePath)) {
        return [string](Get-Content -LiteralPath $absolutePath -Raw)
    }

    $diffLines = @(
        & git -C $repoRoot diff --unified=0 $comparison -- $RelativePath
        & git -C $repoRoot diff --cached --unified=0 -- $RelativePath
    )
    return (@($diffLines | Where-Object { $_ -and $_.StartsWith('+') -and -not $_.StartsWith('+++') }) -join "`n")
}

$failures = @()
foreach ($entry in $changedFiles.GetEnumerator()) {
    $relativePath = $entry.Key
    $text = [string](Get-ChangedText -RelativePath $relativePath)
    foreach ($token in $forbiddenTokens) {
        if ($text.IndexOf($token, [System.StringComparison]::Ordinal) -ge 0) {
            $failures += "$relativePath introduces '$token'. Keep driver primitives behind page objects and browser infrastructure."
        }
    }

    if ($relativePath -match '/(Tests|Features)/') {
        foreach ($boundaryToken in @('PlaywrightBrowserSession', 'session.Page')) {
            if ($text.IndexOf($boundaryToken, [System.StringComparison]::Ordinal) -ge 0) {
                $failures += "$relativePath introduces '$boundaryToken'. Invoke a journey flow or established fixture from scenario code."
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'UI automation architecture violations:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "UI automation standards passed for $($changedFiles.Count) changed UI test/feature/flow file(s)." -ForegroundColor Green
exit 0
