#Requires -Version 5.1
<#
.SYNOPSIS
    Resolves changed files to verify scopes for scripts/verify.ps1.

.DESCRIPTION
    Computes changed files between Base and Head, maps each changed file to the nearest
    owning solution/project, and emits a JSON array suitable for GitHub Actions matrix use.
    Falls back to a repository-scope lightweight run when no workspace can be resolved.

.PARAMETER Base
    Git base ref/commit. Default: origin/main.

.PARAMETER Head
    Git head ref/commit. Default: HEAD.

.PARAMETER ChangedFiles
    Optional repository-relative file paths to resolve instead of computing a Git diff.
    Use this for generated or staged changes that are not represented by Head.
#>
[CmdletBinding()]
param(
    [string]$Base = 'origin/main',
    [string]$Head = 'HEAD',
    [string[]]$ChangedFiles
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

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

function New-Scope {
    param(
        [Parameter(Mandatory)][ValidateSet('SolutionPath', 'ProjectPath', 'ScopePath')][string]$Kind,
        [Parameter(Mandatory)][string]$Path
    )

    return [PSCustomObject]@{
        kind = $Kind
        path = ConvertTo-NormalizedPath -Path $Path
    }
}

function Resolve-WorkspaceForRepoFile {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $absolutePath = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        return $null
    }

    if ($RepoRelativePath -like '*.sln') {
        return New-Scope -Kind 'SolutionPath' -Path $RepoRelativePath
    }

    if ($RepoRelativePath -like '*.csproj') {
        return New-Scope -Kind 'ProjectPath' -Path $RepoRelativePath
    }

    $dir = if (Test-Path -LiteralPath $absolutePath -PathType Leaf) {
        Split-Path -Parent $absolutePath
    } else {
        $absolutePath
    }

    while (-not [string]::IsNullOrWhiteSpace($dir) -and $dir.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projects = @(Get-ChildItem -LiteralPath $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projects.Count -eq 1) {
            return New-Scope -Kind 'ProjectPath' -Path (ConvertTo-RepoRelativePath -Path $projects[0].FullName)
        }

        $solutions = @(Get-ChildItem -LiteralPath $dir -Filter '*.sln' -File -ErrorAction SilentlyContinue)
        if ($solutions.Count -eq 1) {
            return New-Scope -Kind 'SolutionPath' -Path (ConvertTo-RepoRelativePath -Path $solutions[0].FullName)
        }

        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) {
            break
        }

        $dir = $parent
    }

    return $null
}

$changedFiles = if ($PSBoundParameters.ContainsKey('ChangedFiles')) {
    @($ChangedFiles)
} else {
    @(& git -C $repoRoot diff --name-only --diff-filter=ACMR "$Base" "$Head")
}

$changedFiles = @(
    $changedFiles |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object {
        $normalizedPath = ConvertTo-NormalizedPath -Path $_
        if ([System.IO.Path]::IsPathRooted($normalizedPath) -or @($normalizedPath.Split('/') | Where-Object { $_ -eq '..' }).Count -gt 0) {
            throw "Changed file path must be repository-relative and traversal-free: '$normalizedPath'."
        }

        $normalizedPath
    } |
    Where-Object { $_ -ne 'docs/knowledge-repo.md' }
)

$scopes = @()
$hadUnmappedFile = $false

foreach ($file in $changedFiles) {
    $scope = Resolve-WorkspaceForRepoFile -RepoRelativePath $file
    if ($null -eq $scope) {
        $hadUnmappedFile = $true
        continue
    }

    $scopes += $scope
}

if ($scopes.Count -eq 0 -or $hadUnmappedFile) {
    $scopes += New-Scope -Kind 'ScopePath' -Path '.'
}

$scopes = @(
    $scopes |
    Sort-Object kind, path -Unique
)

$json = $scopes | ConvertTo-Json -Compress

# ConvertTo-Json emits a bare object for single-item arrays in Windows PowerShell 5.1.
# The workflow matrix "include" input must always receive a JSON array.
if ($scopes.Count -eq 1 -and $json.TrimStart().StartsWith('{')) {
    "[$json]"
} else {
    $json
}
