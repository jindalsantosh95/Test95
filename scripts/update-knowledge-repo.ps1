#Requires -Version 5.1
<#
.SYNOPSIS
    Append or refresh commit metadata in docs/knowledge-repo.md.

.PARAMETER Commit
    Commit-ish to record. Default: HEAD.

.PARAMETER OutputFile
    Knowledge repo markdown path relative to the repository root.

.PARAMETER FromStaged
    Build an entry from currently staged files (pre-commit mode).

.PARAMETER TrackedPathSpec
    Git pathspecs to include. Defaults to the whole repository.

.PARAMETER ValidationCommand
    Optional validation command(s) to record with the entry.

.PARAMETER RiskNote
    Optional risk or implementation note to record with the entry.
#>
[CmdletBinding()]
param(
    [string]$Commit = 'HEAD',
    [string]$OutputFile = 'docs/knowledge-repo.md',
    [switch]$FromStaged,
    [string[]]$TrackedPathSpec = @('.'),
    [string[]]$ValidationCommand,
    [string]$RiskNote
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$outputPath = Join-Path $repoRoot $OutputFile

if (-not (Test-Path -LiteralPath $outputPath)) {
    throw "knowledge repo file not found: $outputPath"
}

function ConvertTo-NormalizedPath {
    param([Parameter(Mandatory)][string]$Path)

    return $Path.Replace('\', '/')
}

function Test-IgnoredPath {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$KnowledgeRepoPath
    )

    return $Path -eq $KnowledgeRepoPath -or
        $Path -like '*/bin/*' -or
        $Path -like '*/obj/*' -or
        $Path -like '*/.vs/*' -or
        $Path -like '.git/*'
}

function Get-ProjectNameForPath {
    param([Parameter(Mandatory)][string]$Path)

    $absolutePath = Join-Path $repoRoot $Path
    $dir = if (Test-Path -LiteralPath $absolutePath -PathType Leaf) {
        Split-Path -Parent $absolutePath
    } else {
        $absolutePath
    }

    while (-not [string]::IsNullOrWhiteSpace($dir) -and $dir.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projectFiles = @(Get-ChildItem -LiteralPath $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projectFiles.Count -eq 1) {
            return [System.IO.Path]::GetFileNameWithoutExtension($projectFiles[0].Name)
        }

        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) {
            break
        }

        $dir = $parent
    }

    return $null
}

function Test-StringSetEqual {
    param(
        [string[]]$Left,
        [string[]]$Right
    )

    $leftSorted = @($Left | Sort-Object)
    $rightSorted = @($Right | Sort-Object)

    if ($leftSorted.Count -ne $rightSorted.Count) {
        return $false
    }

    for ($i = 0; $i -lt $leftSorted.Count; $i++) {
        if ($leftSorted[$i] -ne $rightSorted[$i]) {
            return $false
        }
    }

    return $true
}

function Get-EntryFiles {
    param(
        [string[]]$EntryLines,
        [string]$IgnoredPath
    )

    $entryFiles = @()
    $inFiles = $false

    foreach ($line in $EntryLines) {
        if ($line -eq '- Files:') {
            $inFiles = $true
            continue
        }

        if (-not $inFiles) {
            continue
        }

        if ($line -match '^\s+-\s+(.+)$') {
            $entryFile = ConvertTo-NormalizedPath -Path $Matches[1].Trim()
            if ($entryFile -ne $IgnoredPath) {
                $entryFiles += $entryFile
            }

            continue
        }

        if ($line -match '^-\s+\S+:' -or $line -match '^###\s+') {
            break
        }
    }

    return $entryFiles
}

function Remove-LatestMatchingPendingEntry {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string[]]$CurrentFiles,
        [string]$IgnoredPath
    )

    $raw = Get-Content -Raw -LiteralPath $Path
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return $false
    }

    $lines = @($raw -split '\r?\n')
    $pendingEntryIndexes = @()

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^###\s+.+\s+UTC\s+-\s+pending$') {
            $pendingEntryIndexes += $i
        }
    }

    if ($pendingEntryIndexes.Count -eq 0) {
        return $false
    }

    $start = $pendingEntryIndexes[-1]
    $end = $lines.Count

    for ($i = $start + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^###\s+') {
            $end = $i
            break
        }
    }

    $entryLines = @($lines[$start..($end - 1)])
    $entryFiles = @(Get-EntryFiles -EntryLines $entryLines -IgnoredPath $IgnoredPath)

    if (-not (Test-StringSetEqual -Left $entryFiles -Right $CurrentFiles)) {
        return $false
    }

    $remainingLines = @()

    if ($start -gt 0) {
        $remainingLines += $lines[0..($start - 1)]
    }

    if ($end -lt $lines.Count) {
        $remainingLines += $lines[$end..($lines.Count - 1)]
    }

    Set-Content -LiteralPath $Path -Value $remainingLines

    return $true
}

function Test-DocsOnly {
    param([string[]]$ChangedFiles)

    $nonDocs = @($ChangedFiles | Where-Object {
        $_ -notlike 'docs/*' -and
        $_ -notlike 'doc/*' -and
        $_ -notmatch '(^|/)README\.md$' -and
        $_ -notmatch '(^|/)AGENTS\.md$' -and
        $_ -notmatch '\.md$'
    })

    return $nonDocs.Count -eq 0
}

function Get-StagedSummary {
    param([string[]]$ChangedFiles)

    if (Test-DocsOnly -ChangedFiles $ChangedFiles) {
        return 'pending docs update (captured at pre-commit)'
    }

    if ($ChangedFiles | Where-Object { $_ -match '\.Deployment/' -or $_ -match '\.Deployment\.deployproj$' }) {
        return 'pending deployment project update (captured at pre-commit)'
    }

    if ($ChangedFiles | Where-Object { $_ -match '/Program\.cs$' }) {
        return 'pending startup/DI update (captured at pre-commit)'
    }

    if ($ChangedFiles | Where-Object { $_ -match '/Triggers/' }) {
        return 'pending trigger update (captured at pre-commit)'
    }

    return 'pending repository update (captured at pre-commit)'
}

function Get-AutomaticRiskNotes {
    param([string[]]$ChangedFiles)

    $notes = @()

    if (Test-DocsOnly -ChangedFiles $ChangedFiles) {
        $notes += 'docs-only change; no runtime behavior expected'
    }

    if ($ChangedFiles | Where-Object { $_ -match '/Program\.cs$' -or $_ -match '(^|/)STARTUP_AND_DI\.md$' }) {
        $notes += 'startup/DI surface changed; verify registrations and configuration docs'
    }

    if ($ChangedFiles | Where-Object { $_ -match '\.Deployment/' -or $_ -match '\.Deployment\.deployproj$' -or $_ -match '(^|/)HARNESS_PIPELINE_CHECKLIST\.md$' }) {
        $notes += 'deployment surface changed; verify deployment project and pipeline alignment'
    }

    if ($ChangedFiles | Where-Object { $_ -match '/Triggers/' }) {
        $notes += 'trigger/subscriber surface changed; verify binding names, topics, schedules, and retries'
    }

    if ($ChangedFiles | Where-Object { $_ -match '/Application/' -or $_ -match '/Services/' -or $_ -match '/Infrastructure/' }) {
        $notes += 'behavior surface changed; verify handler/service flow and tests'
    }

    if ($ChangedFiles | Where-Object { $_ -match '\.csproj$' -or $_ -match '\.deployproj$' -or $_ -match '\.sln$' }) {
        $notes += 'project or solution file changed; verify package/deployment conventions'
    }

    if ($ChangedFiles | Where-Object { $_ -like 'scripts/*' -or $_ -like '.githooks/*' -or $_ -like '.codex/*' }) {
        $notes += 'harness or hook automation changed; verify local hook behavior'
    }

    if ($notes.Count -eq 0) {
        $notes += 'no automatic risk flags'
    }

    return @($notes | Sort-Object -Unique)
}

$knowledgeRepoPath = (ConvertTo-NormalizedPath -Path $OutputFile).TrimStart('/')
$utc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')

if ($FromStaged) {
    $sha = 'pending'
    $files = @(
        & git -C $repoRoot diff --cached --name-only --diff-filter=ACMR -- $TrackedPathSpec |
        Where-Object { $_ } |
        ForEach-Object { ConvertTo-NormalizedPath -Path $_ } |
        Where-Object { -not (Test-IgnoredPath -Path $_ -KnowledgeRepoPath $knowledgeRepoPath) }
    )
} else {
    $sha = (& git -C $repoRoot rev-parse --short $Commit).Trim()
    $subject = (& git -C $repoRoot show -s --format=%s $Commit).Trim()
    $files = @(
        & git -C $repoRoot show --name-only --pretty=format: $Commit -- $TrackedPathSpec |
        Where-Object { $_ } |
        ForEach-Object { ConvertTo-NormalizedPath -Path $_ } |
        Where-Object { -not (Test-IgnoredPath -Path $_ -KnowledgeRepoPath $knowledgeRepoPath) }
    )
}

if ($files.Count -eq 0) {
    Write-Host 'no tracked files detected for knowledge update; skipping' -ForegroundColor DarkGray
    exit 0
}

if ($FromStaged) {
    $subject = Get-StagedSummary -ChangedFiles $files
    $removedPendingEntry = Remove-LatestMatchingPendingEntry -Path $outputPath -CurrentFiles $files -IgnoredPath $knowledgeRepoPath
    if ($removedPendingEntry) {
        Write-Host 'knowledge repo refreshed: replaced latest matching pending entry' -ForegroundColor DarkGray
    }
}

$projects = @(
    $files |
    ForEach-Object { Get-ProjectNameForPath -Path $_ } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Sort-Object -Unique
)

$areas = @(
    $files |
    ForEach-Object {
        $parts = $_ -split '/'
        if ($parts.Count -eq 1) {
            '(root)'
        } else {
            $parts[0]
        }
    } |
    Where-Object { $_ } |
    Sort-Object -Unique
)

$validationCommands = @(
    $ValidationCommand |
    ForEach-Object { $_ -split '\s*,\s*' } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { $_.Trim() }
)
if ($validationCommands.Count -eq 0) {
    if ($FromStaged) {
        $validationCommands = @('not recorded (captured at pre-commit)')
    } else {
        $validationCommands = @('not supplied')
    }
}

$riskNotes = @(Get-AutomaticRiskNotes -ChangedFiles $files)
if (-not [string]::IsNullOrWhiteSpace($RiskNote)) {
    $riskNotes += $RiskNote.Trim()
}
$riskNotes = @($riskNotes | Sort-Object -Unique)

$entry = @()
$entry += "### $utc UTC - $sha"
$entry += ""
$entry += "- Summary: $subject"
$entry += "- Projects:"
if ($projects.Count -eq 0) {
    $entry += "  - (none)"
} else {
    $projects | ForEach-Object { $entry += "  - $_" }
}
$entry += "- Areas:"
if ($areas.Count -eq 0) {
    $entry += "  - (none)"
} else {
    $areas | ForEach-Object { $entry += "  - $_" }
}
$entry += "- Validation:"
$validationCommands | ForEach-Object { $entry += "  - $_" }
$entry += "- Risks/Notes:"
$riskNotes | ForEach-Object { $entry += "  - $_" }
$entry += "- Files:"
$files | ForEach-Object { $entry += "  - $_" }
$entry += ""

Add-Content -LiteralPath $outputPath -Value ($entry -join [Environment]::NewLine)
Write-Host "knowledge repo updated: $OutputFile" -ForegroundColor Green
