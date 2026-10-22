#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$BaseRef = 'main',
    [string]$HeadRef = 'HEAD',
    [Parameter(Mandatory)][string]$ScopePath
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestStandards.psm1') -Force
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$resolvedScope = (Resolve-Path -LiteralPath $ScopePath).Path
if (Test-Path -LiteralPath $resolvedScope -PathType Leaf) {
    $resolvedScope = Split-Path -Parent $resolvedScope
}
$repoRootPrefix = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if ([string]::Equals($repoRoot, $resolvedScope, [System.StringComparison]::OrdinalIgnoreCase)) {
    $scopePrefix = ''
}
elseif ($resolvedScope.StartsWith($repoRootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    $scopePrefix = $resolvedScope.Substring($repoRootPrefix.Length).Replace('\', '/') + '/'
}
else {
    throw "ScopePath must resolve within the repository: $repoRoot"
}

function Test-IsUnitTestFile([string]$Path) {
    return $Path -match '(?i)(?:^|/)[^/]*Tests?(?:/|$)' -or $Path -match '(?i)Tests?\.cs$'
}

function Test-ProjectHasMockResource([string]$File) {
    $directory = Split-Path -Parent (Join-Path $repoRoot $File)
    while ($directory.StartsWith($repoRootPrefix, [System.StringComparison]::OrdinalIgnoreCase) -or [string]::Equals($directory, $repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projects = @(Get-ChildItem -LiteralPath $directory -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projects.Count -gt 0) {
            $sourceMockFiles = @(Get-ChildItem -LiteralPath $directory -Filter '*.mock' -File -Recurse -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
            if ($sourceMockFiles.Count -gt 0) { return $true }
            return $null -ne ($projects | Where-Object { (Get-Content -LiteralPath $_.FullName -Raw) -match '(?i)\.mocks?[\\/].*\.mock' } | Select-Object -First 1)
        }
        $parent = Split-Path -Parent $directory
        if ($parent -eq $directory) { break }
        $directory = $parent
    }
    return $false
}

$csharpPathspec = ":(glob)$scopePrefix**/*.cs"
$failures = New-Object System.Collections.Generic.List[string]
$diffLines = if ($HeadRef -eq 'HEAD') {
    @(& git -C $repoRoot diff --unified=0 $BaseRef -- $csharpPathspec)
} else {
    @(& git -C $repoRoot diff --unified=0 $BaseRef $HeadRef -- $csharpPathspec)
}
$currentFile = $null
$addedContentByFile = @{}
foreach ($line in $diffLines) {
    if ($line -match '^\+\+\+ b/(.+)$') {
        $currentFile = $Matches[1].Replace('\', '/')
        continue
    }

    if ($line.StartsWith('+') -and -not $line.StartsWith('+++') -and $null -ne $currentFile -and (Test-IsUnitTestFile $currentFile)) {
        if (-not $addedContentByFile.ContainsKey($currentFile)) {
            $addedContentByFile[$currentFile] = New-Object System.Collections.Generic.List[string]
        }
        $addedContentByFile[$currentFile].Add($line.Substring(1)) | Out-Null
    }
}

foreach ($file in $addedContentByFile.Keys) {
    $addedContent = $addedContentByFile[$file] -join [Environment]::NewLine
    if (Test-ContainsMoqUsage -Content $addedContent) {
        $failures.Add("$file introduces Moq in new or amended unit-test code; use real DI registrations, the shared HTTP/resource-boundary fixture with checked-in .mock files, or a small hand-written fake when resource mocking is not applicable.") | Out-Null
    }
    if (Test-ContainsAdHocHttpConnectorUsage -Content $addedContent) {
        $failures.Add("$file introduces an ad hoc IHttpConnector test implementation; register the real HttpConnector and shared xUnit IHttpClientFactory through ServiceCollection, backed by checked-in .mock files.") | Out-Null
    }
    $containsSharedPipeline = Test-ContainsSharedHttpTestPipeline -Content $addedContent
    if ($containsSharedPipeline -and -not (Test-ProjectHasMockResource -File $file)) {
        $failures.Add("$file registers the shared HTTP test pipeline but its test project has no checked-in or linked .mock resource.") | Out-Null
    }
}

if ($HeadRef -eq 'HEAD') {
    $untrackedFiles = @(
        & git -C $repoRoot ls-files --others --exclude-standard -- $csharpPathspec |
        ForEach-Object { $_.Replace('\', '/') } |
        Where-Object { Test-IsUnitTestFile $_ }
    )
    foreach ($file in $untrackedFiles) {
        $content = Get-Content -LiteralPath (Join-Path $repoRoot $file) -Raw
        if (Test-ContainsMoqUsage -Content $content) {
            $failures.Add("$file introduces Moq in new unit-test code; use real DI registrations, the shared HTTP/resource-boundary fixture with checked-in .mock files, or a small hand-written fake when resource mocking is not applicable.") | Out-Null
        }
        if (Test-ContainsAdHocHttpConnectorUsage -Content $content) {
            $failures.Add("$file introduces an ad hoc IHttpConnector test implementation; register the real HttpConnector and shared xUnit IHttpClientFactory through ServiceCollection, backed by checked-in .mock files.") | Out-Null
        }
        $containsSharedPipeline = Test-ContainsSharedHttpTestPipeline -Content $content
        if ($containsSharedPipeline -and -not (Test-ProjectHasMockResource -File $file)) {
            $failures.Add("$file registers the shared HTTP test pipeline but its test project has no checked-in or linked .mock resource.") | Out-Null
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'test standards check failed:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host 'test standards check passed' -ForegroundColor Green
exit 0
