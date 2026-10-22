#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Base = 'origin/main'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

& git -C $repoRoot rev-parse --verify $Base *> $null
if ($LASTEXITCODE -ne 0) {
    throw "Base ref '$Base' was not found."
}

$newFiles = @(
    @(& git -C $repoRoot diff $Base --name-only --diff-filter=A -- '*.cs')
    @(& git -C $repoRoot ls-files --others --exclude-standard -- '*.cs')
) |
        ForEach-Object { ($_ -replace '\\', '/') } |
        Where-Object { Test-Path -LiteralPath (Join-Path $repoRoot $_) -PathType Leaf } |
        Sort-Object -Unique

if ($newFiles.Count -eq 0) {
    Write-Host 'no generated C# files require formatting'
    exit 0
}

$filesByProject = @{}
foreach ($file in $newFiles) {
    $absoluteFile = (Resolve-Path -LiteralPath (Join-Path $repoRoot $file)).Path
    $directory = Split-Path -Parent $absoluteFile
    $project = $null

    while ($directory.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projects = @(Get-ChildItem -LiteralPath $directory -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projects.Count -eq 1) {
            $project = $projects[0].FullName
            break
        }
        if ($projects.Count -gt 1) {
            throw "Multiple projects own generated file '$file': $($projects.FullName -join ', ')"
        }

        if ([string]::Equals($directory, $repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) { break }
        $directory = Split-Path -Parent $directory
    }

    if ([string]::IsNullOrWhiteSpace($project)) {
        throw "Could not resolve an owning project for generated C# file '$file'."
    }

    if (-not $filesByProject.ContainsKey($project)) {
        $filesByProject[$project] = [System.Collections.Generic.List[string]]::new()
    }
    $filesByProject[$project].Add($absoluteFile)
}

foreach ($project in $filesByProject.Keys | Sort-Object) {
    $projectDirectory = Split-Path -Parent $project
    Push-Location $projectDirectory
    try {
        $relativeFiles = @(
            $filesByProject[$project] |
                ForEach-Object { ((Resolve-Path -LiteralPath $_ -Relative) -replace '^\.[\\/]', '') }
        )
        Write-Host "formatting $($relativeFiles.Count) generated C# file(s) with $project"
        & dotnet format $project --include $relativeFiles
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet format failed for '$project' with exit code $LASTEXITCODE."
        }
    } finally {
        Pop-Location
    }
}

Write-Host "generated C# formatting passed for $($newFiles.Count) file(s)"
exit 0
