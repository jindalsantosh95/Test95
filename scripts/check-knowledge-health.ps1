#Requires -Version 5.1
<#
.SYNOPSIS
    Verifies repository-local knowledge artifacts are present and internally linked.

.DESCRIPTION
    Checks the durable docs map, first-class planning/decision/debt artifacts,
    docs-local markdown links, and recent generated knowledge log entry shape.
#>
[CmdletBinding()]
param(
    [string]$DocsRoot = 'docs',
    [int]$KnowledgeLogEntriesToCheck = 5,
    [switch]$CheckRepositoryLinks,
    [switch]$SkipKnowledgeLogShape
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$docsRootPath = Join-Path $repoRoot $DocsRoot
$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([Parameter(Mandatory)][string]$Message)

    $script:failures.Add($Message) | Out-Null
}

function ConvertTo-NormalizedPath {
    param([Parameter(Mandatory)][string]$Path)

    return $Path.Replace('\', '/')
}

function ConvertTo-RepoRelativePath {
    param([Parameter(Mandatory)][string]$Path)

    $root = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = New-Object System.Uri($root)
    $pathUri = New-Object System.Uri((Resolve-Path -LiteralPath $Path).Path)

    return ConvertTo-NormalizedPath -Path ([System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()))
}

function Test-RequiredPath {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $path = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "missing required knowledge artifact: $RepoRelativePath"
    }
}

function Test-LocalMarkdownLinks {
    if (-not (Test-Path -LiteralPath $docsRootPath -PathType Container)) {
        Add-Failure "docs root not found: $DocsRoot"
        return
    }

    $markdownFiles = @(
        Get-ChildItem -LiteralPath $docsRootPath -Recurse -Filter '*.md' -File -ErrorAction SilentlyContinue |
        Sort-Object FullName
    )

    $linkPattern = '!\[[^\]]*\]\(([^)]+)\)|(?<!!)\[[^\]]+\]\(([^)]+)\)'

    foreach ($file in $markdownFiles) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        $matches = [regex]::Matches($content, $linkPattern)
        $repoRelativeFile = ConvertTo-RepoRelativePath -Path $file.FullName

        foreach ($match in $matches) {
            $target = @($match.Groups | Where-Object { $_.Success -and $_.Value -and $_.Name -match '^\d+$' })[-1].Value.Trim()

            if ([string]::IsNullOrWhiteSpace($target)) {
                continue
            }

            if ($target.StartsWith('<') -and $target.EndsWith('>')) {
                $target = $target.Substring(1, $target.Length - 2).Trim()
            }

            if ($target -match '^(https?:|mailto:)' -or $target.StartsWith('#')) {
                continue
            }

            $targetPath = ($target -split '#', 2)[0]
            $targetPath = ($targetPath -split '\?', 2)[0]

            if ([string]::IsNullOrWhiteSpace($targetPath)) {
                continue
            }

            if ([System.IO.Path]::IsPathRooted($targetPath)) {
                continue
            }

            if ($targetPath -match '^(?<path>.+):\d+$') {
                $targetPath = $Matches.path
            }

            $decodedTargetPath = [System.Uri]::UnescapeDataString($targetPath)
            $candidate = Join-Path $file.DirectoryName $decodedTargetPath
            $candidateFullPath = [System.IO.Path]::GetFullPath($candidate)

            if (-not $CheckRepositoryLinks) {
                $docsRootFullPath = [System.IO.Path]::GetFullPath($docsRootPath)
                if (-not $candidateFullPath.StartsWith($docsRootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
                    continue
                }
            }

            if (-not (Test-Path -LiteralPath $candidateFullPath)) {
                Add-Failure "$repoRelativeFile links to missing local target: $target"
            }
        }
    }
}

function Test-KnowledgeLogShape {
    $knowledgeLogPath = Join-Path $repoRoot 'docs/knowledge-repo.md'
    if (-not (Test-Path -LiteralPath $knowledgeLogPath -PathType Leaf)) {
        Add-Failure 'missing generated knowledge log: docs/knowledge-repo.md'
        return
    }

    $lines = @(Get-Content -LiteralPath $knowledgeLogPath)
    $entryStarts = @()

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^###\s+.+\s+UTC\s+-\s+\S+') {
            $entryStarts += $i
        }
    }

    if ($entryStarts.Count -eq 0) {
        Add-Failure 'docs/knowledge-repo.md has no generated entries'
        return
    }

    $requiredFields = @(
        '- Summary:',
        '- Projects:',
        '- Areas:',
        '- Validation:',
        '- Risks/Notes:',
        '- Files:'
    )

    $firstEntryIndexToCheck = 0
    if ($KnowledgeLogEntriesToCheck -gt 0 -and $entryStarts.Count -gt $KnowledgeLogEntriesToCheck) {
        $firstEntryIndexToCheck = $entryStarts.Count - $KnowledgeLogEntriesToCheck
    }

    for ($entryIndex = $firstEntryIndexToCheck; $entryIndex -lt $entryStarts.Count; $entryIndex++) {
        $start = $entryStarts[$entryIndex]
        $end = if ($entryIndex + 1 -lt $entryStarts.Count) { $entryStarts[$entryIndex + 1] - 1 } else { $lines.Count - 1 }
        $entryLines = @($lines[$start..$end])
        $entryTitle = $lines[$start]

        foreach ($field in $requiredFields) {
            $hasField = $entryLines | Where-Object { $_.StartsWith($field, [System.StringComparison]::Ordinal) }
            if (-not $hasField) {
                Add-Failure "knowledge log entry '$entryTitle' is missing field: $field"
            }
        }
    }
}

$requiredPaths = @(
    'AGENTS.md',
    'docs/readme.md',
    'docs/harness-engineering.md',
    'docs/harness-contract.md',
    'docs/harness-coverage.md',
    'docs/harness-pipeline-checklist.md',
    'docs/knowledge-repo-guide.md',
    'docs/knowledge-repo.md',
    'docs/exec-plans/readme.md',
    'docs/exec-plans/template.md',
    'docs/exec-plans/active/readme.md',
    'docs/exec-plans/completed/readme.md',
    'docs/decisions/readme.md',
    'docs/decisions/template.md',
    'docs/tech-debt-tracker.md'
)

foreach ($path in $requiredPaths) {
    Test-RequiredPath -RepoRelativePath $path
}

Test-LocalMarkdownLinks

if (-not $SkipKnowledgeLogShape) {
    Test-KnowledgeLogShape
}

if ($failures.Count -gt 0) {
    Write-Host 'knowledge health check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }

    exit 1
}

Write-Host 'knowledge health check passed' -ForegroundColor Green
exit 0
