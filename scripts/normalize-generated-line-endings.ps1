#Requires -Version 5.1
<#
.SYNOPSIS
    Normalizes generated text files to CRLF in the current working tree.

.DESCRIPTION
    Sequence-goal agents can create or edit files after checkout, bypassing
    Git's checkout-time eol normalization. This script repairs working-tree
    line endings before verification without changing Git's normalized index
    model. It preserves whether each file already used a UTF-8 BOM.
#>
[CmdletBinding()]
param(
    [string]$Base = 'origin/main'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

$extensions = @(
    '.cs',
    '.csproj',
    '.props',
    '.targets',
    '.sln',
    '.md',
    '.json',
    '.yml',
    '.yaml',
    '.ps1'
)

$baseChangedFiles = @()
if (-not [string]::IsNullOrWhiteSpace($Base)) {
    & git -C $repoRoot rev-parse --verify $Base *> $null
    if ($LASTEXITCODE -eq 0) {
        $range = "$Base...HEAD"
        $baseChangedFiles = @(& git -C $repoRoot diff --name-only --no-renames $range --)
    } else {
        Write-Warning "Base ref '$Base' was not found; normalizing working-tree changes only."
    }
}

$changedFiles = @(
    $baseChangedFiles
    & git -C $repoRoot diff --name-only --no-renames HEAD --
    & git -C $repoRoot diff --cached --name-only --no-renames --
    & git -C $repoRoot ls-files --others --exclude-standard
) |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { ($_ -replace '\\', '/') } |
    Sort-Object -Unique

$normalized = New-Object System.Collections.Generic.List[string]

foreach ($file in $changedFiles) {
    if ($file -notmatch '^(VAA\.EAPI\.|docs/)') {
        continue
    }

    $extension = [System.IO.Path]::GetExtension($file)
    if ($extensions -notcontains $extension) {
        continue
    }

    $path = Join-Path $repoRoot $file
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes -contains 0) {
        continue
    }

    $hasUtf8Bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $encoding = New-Object System.Text.UTF8Encoding($hasUtf8Bom)
    $text = [System.IO.File]::ReadAllText($path)
    $crlfText = $text -replace "\r\n|\n|\r", "`r`n"
    if ($crlfText -ne $text) {
        [System.IO.File]::WriteAllText($path, $crlfText, $encoding)
        $normalized.Add($file) | Out-Null
    }
}

if ($normalized.Count -gt 0) {
    Write-Host "normalized generated line endings for $($normalized.Count) file(s):"
    $normalized | ForEach-Object { Write-Host "- $_" }
} else {
    Write-Host 'generated line endings already normalized'
}

exit 0
