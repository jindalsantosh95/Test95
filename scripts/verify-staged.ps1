#Requires -Version 5.1
<#
.SYNOPSIS
    Runs the repository harness against workspaces inferred from staged files.

.DESCRIPTION
    Resolves staged files to the nearest owning project or solution and runs
    scripts/verify.ps1 for each distinct scope. Docs, scripts, hooks, and other
    root-level changes fall back to a repository-scoped lightweight harness run.
#>
[CmdletBinding()]
param(
    [string]$Base = 'main',
    [switch]$SkipBuild,
    [switch]$SkipDeploymentCheck,
    [switch]$ListCommands
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$verifyScript = Join-Path $PSScriptRoot 'verify.ps1'

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

function New-VerifyCommand {
    param(
        [Parameter(Mandatory)][ValidateSet('SolutionPath', 'ProjectPath', 'ScopePath')][string]$Kind,
        [Parameter(Mandatory)][string]$Path
    )

    return [PSCustomObject]@{
        Kind = $Kind
        Path = ConvertTo-NormalizedPath -Path $Path
    }
}

function Resolve-WorkspaceForRepoFile {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $absolutePath = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        return $null
    }

    if ($RepoRelativePath -like '*.sln') {
        return New-VerifyCommand -Kind 'SolutionPath' -Path $RepoRelativePath
    }

    if ($RepoRelativePath -like '*.csproj') {
        return New-VerifyCommand -Kind 'ProjectPath' -Path $RepoRelativePath
    }

    $dir = if (Test-Path -LiteralPath $absolutePath -PathType Leaf) {
        Split-Path -Parent $absolutePath
    } else {
        $absolutePath
    }

    while (-not [string]::IsNullOrWhiteSpace($dir) -and $dir.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projects = @(Get-ChildItem -LiteralPath $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projects.Count -eq 1) {
            return New-VerifyCommand -Kind 'ProjectPath' -Path (ConvertTo-RepoRelativePath -Path $projects[0].FullName)
        }

        $solutions = @(Get-ChildItem -LiteralPath $dir -Filter '*.sln' -File -ErrorAction SilentlyContinue)
        if ($solutions.Count -eq 1) {
            return New-VerifyCommand -Kind 'SolutionPath' -Path (ConvertTo-RepoRelativePath -Path $solutions[0].FullName)
        }

        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) {
            break
        }

        $dir = $parent
    }

    return $null
}

function Format-CommandArgument {
    param([Parameter(Mandatory)][string]$Value)

    if ($Value -match '\s') {
        return '"' + ($Value -replace '"', '\"') + '"'
    }

    return $Value
}

function Format-VerifyCommand {
    param([Parameter(Mandatory)]$Command)

    $parts = @('scripts/verify.ps1')

    if ($Command.Kind -eq 'ScopePath') {
        $parts += '-ScopePath'
        $parts += (Format-CommandArgument -Value $Command.Path)
        $parts += '-SkipBuild'
    } else {
        $parts += "-$($Command.Kind)"
        $parts += (Format-CommandArgument -Value $Command.Path)

        if ($SkipBuild) {
            $parts += '-SkipBuild'
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Base) -and $Base -ne 'main') {
        $parts += '-Base'
        $parts += (Format-CommandArgument -Value $Base)
    }

    if ($SkipDeploymentCheck) {
        $parts += '-SkipDeploymentCheck'
    }

    return ($parts -join ' ')
}

function Get-VerifyParameters {
    param([Parameter(Mandatory)]$Command)

    $parameters = @{
        Base = $Base
    }

    if ($Command.Kind -eq 'ScopePath') {
        $parameters.ScopePath = $Command.Path
        $parameters.SkipBuild = $true
    } else {
        $parameters[$Command.Kind] = $Command.Path

        if ($SkipBuild) {
            $parameters.SkipBuild = $true
        }
    }

    if ($SkipDeploymentCheck) {
        $parameters.SkipDeploymentCheck = $true
    }

    return ,$parameters
}

$stagedFiles = @(
    & git -C $repoRoot diff --cached --name-only --diff-filter=ACMR |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { ConvertTo-NormalizedPath -Path $_ } |
    Where-Object { $_ -ne 'docs/knowledge-repo.md' }
)

$commands = @()
$needsRepoScope = $false

foreach ($file in $stagedFiles) {
    $workspaceCommand = Resolve-WorkspaceForRepoFile -RepoRelativePath $file
    if ($null -eq $workspaceCommand) {
        $needsRepoScope = $true
        continue
    }

    $commands += $workspaceCommand
}

if ($needsRepoScope -or $commands.Count -eq 0) {
    $commands += New-VerifyCommand -Kind 'ScopePath' -Path '.'
}

$commands = @(
    $commands |
    Sort-Object Kind, Path -Unique
)

if ($ListCommands) {
    foreach ($command in $commands) {
        Write-Output (Format-VerifyCommand -Command $command)
    }

    exit 0
}

$failed = @()

foreach ($command in $commands) {
    $display = Format-VerifyCommand -Command $command
    Write-Host ''
    Write-Host "==> Staged verification: $display" -ForegroundColor Cyan

    $verifyParameters = Get-VerifyParameters -Command $command
    & $verifyScript @verifyParameters
    if ($LASTEXITCODE -ne 0) {
        $failed += $display
    }
}

if ($failed.Count -gt 0) {
    Write-Host ''
    Write-Host 'staged verification failed:' -ForegroundColor Red
    foreach ($failure in $failed) {
        Write-Host " - $failure" -ForegroundColor Red
    }

    exit 1
}

Write-Host ''
Write-Host 'staged verification passed' -ForegroundColor Green
exit 0
