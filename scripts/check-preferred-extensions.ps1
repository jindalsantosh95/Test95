#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$BaseRef = 'main',
    [Parameter(Mandatory)][string]$ScopePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$resolvedScope = (Resolve-Path -LiteralPath $ScopePath).Path
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

$csharpPathspec = ":(glob)$scopePrefix**/*.cs"
$failures = New-Object System.Collections.Generic.List[string]
$forbiddenPattern = '(?s)(?:System\.)?String\.(?:Equals\s*\(.*?StringComparison\.OrdinalIgnoreCase|IsNullOrWhiteSpace\s*\()'
$extensionImplementation = 'VAA.EAPI.Microservices/VAA.EAPI.Common/Extensions/StringExtensions.cs'

$diffLines = @(& git -C $repoRoot diff --unified=0 $BaseRef -- $csharpPathspec)
$currentFile = $null
$addedContentByFile = @{}
foreach ($line in $diffLines) {
    if ($line -match '^\+\+\+ b/(.+)$') {
        $currentFile = $Matches[1].Replace('\', '/')
        continue
    }

    if ($line.StartsWith('+') -and -not $line.StartsWith('+++') -and $currentFile -ne $extensionImplementation) {
        if (-not $addedContentByFile.ContainsKey($currentFile)) {
            $addedContentByFile[$currentFile] = New-Object System.Collections.Generic.List[string]
        }

        $addedContentByFile[$currentFile].Add($line.Substring(1)) | Out-Null
    }
}

foreach ($file in $addedContentByFile.Keys) {
    if (($addedContentByFile[$file] -join [Environment]::NewLine) -match $forbiddenPattern) {
        $failures.Add("$file uses a built-in string helper covered by VAA.EAPI.Common.Extensions; use .Is(...) or .IsNullOrWhiteSpace().") | Out-Null
    }
}

$untrackedFiles = @(
    & git -C $repoRoot ls-files --others --exclude-standard -- $csharpPathspec |
    ForEach-Object { $_.Replace('\', '/') }
)

foreach ($file in $untrackedFiles) {
    if ($file -eq $extensionImplementation) {
        continue
    }

    $content = Get-Content -LiteralPath (Join-Path $repoRoot $file) -Raw
    if ($content -match $forbiddenPattern) {
        $failures.Add("$file uses a built-in string helper covered by VAA.EAPI.Common.Extensions; use .Is(...) or .IsNullOrWhiteSpace().") | Out-Null
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'preferred extension check failed:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host 'preferred extension check passed' -ForegroundColor Green
exit 0
