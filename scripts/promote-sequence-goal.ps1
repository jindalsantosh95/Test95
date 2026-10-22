#Requires -Version 5.1
<#
.SYNOPSIS
    Promotes a sequence diagram Agent Loop Goal status and keeps index/status metadata in sync.

.DESCRIPTION
    Updates one diagram under docs/sequence-diagrams and the matching row in
    docs/sequence-diagrams.md. Applies transition guards by default:
      Draft -> Ready
      Ready -> InProgress
      InProgress -> Complete
      Complete -> Draft
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$DiagramPath,
    [Parameter(Mandatory)][ValidateSet('Draft', 'Ready', 'InProgress', 'Complete')][string]$ToStatus,
    [string]$TaskRef,
    [string]$PrUrl,
    [string]$CompletedBy,
    [string]$TransitionNote,
    [string]$MaximumAgentResources,
    [string]$ActualAgentResources,
    [string]$ProductionAgentResources,
    [string]$AutomationAgentResources,
    [switch]$UpdateFieldsOnly,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

function Resolve-RepoInputPath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return (Resolve-Path -LiteralPath (Join-Path $repoRoot $Path)).Path
}

function Normalize-RepoRelativePath {
    param([Parameter(Mandatory)][string]$AbsolutePath)

    $root = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = New-Object System.Uri($root)
    $pathUri = New-Object System.Uri($AbsolutePath)
    return [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString())
}

function Set-GoalField {
    param(
        [Parameter(Mandatory)][string]$Section,
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Value,
        [string]$Newline = "`n"
    )

    if ($Value.IndexOf('|') -ge 0 -or $Value.IndexOf("`r") -ge 0 -or $Value.IndexOf("`n") -ge 0) {
        throw "Goal field '$Field' value cannot contain '|', CR, or LF because it must fit in one markdown table cell."
    }

    $escapedField = [regex]::Escape($Field)
    $linePattern = "(?im)^\|\s*$escapedField\s*\|\s*.*?\|\s*$"
    $line = "| $Field | $Value |"

    if ([regex]::IsMatch($Section, $linePattern)) {
        return [regex]::Replace(
            $Section,
            $linePattern,
            [System.Text.RegularExpressions.MatchEvaluator]{
                param($match)
                return $line
            }
        )
    }

    $doneWhenMatch = [regex]::Match($Section, '(?im)^\|\s*Done when\s*\|.*$')
    if ($doneWhenMatch.Success) {
        return $Section.Substring(0, $doneWhenMatch.Index) + "$line$Newline" + $Section.Substring($doneWhenMatch.Index)
    }

    return ($Section.TrimEnd() + "$Newline$line$Newline")
}

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

$diagramAbsolutePath = Resolve-RepoInputPath -Path $DiagramPath
$diagramRelativePath = Normalize-RepoRelativePath -AbsolutePath $diagramAbsolutePath
if ($diagramRelativePath -notlike 'docs/sequence-diagrams/*.md') {
    throw "DiagramPath must point to docs/sequence-diagrams/*.md. Resolved: $diagramRelativePath"
}

$diagramText = Get-Content -LiteralPath $diagramAbsolutePath -Raw
$goalMatch = [regex]::Match(
    $diagramText,
    '(?ims)^##\s+Agent Loop Goal\s*$\r?\n(?<section>.*?)(?=^##\s+|\z)'
)
if (-not $goalMatch.Success) {
    throw "Agent Loop Goal section not found in $diagramRelativePath"
}

$section = $goalMatch.Groups['section'].Value
$newline = if ($diagramText.Contains("`r`n")) { "`r`n" } else { "`n" }
$statusMatch = [regex]::Match($section, '(?im)^\|\s*Status\s*\|\s*`(?<status>[^`]+)`\s*\|\s*$')
if (-not $statusMatch.Success) {
    throw "Status row not found in Agent Loop Goal section for $diagramRelativePath"
}

$fromStatus = $statusMatch.Groups['status'].Value.Trim()
$allowedTransitions = @{
    Draft      = @('Ready')
    Ready      = @('InProgress')
    InProgress = @('Complete')
    Complete   = @('Draft')
}

if ($fromStatus -eq $ToStatus -and -not $UpdateFieldsOnly) {
    Write-Host "No-op: $diagramRelativePath is already $ToStatus" -ForegroundColor DarkGray
    exit 0
}

if (-not $Force -and -not ($UpdateFieldsOnly -and $fromStatus -eq $ToStatus)) {
    if (-not $allowedTransitions.ContainsKey($fromStatus) -or ($allowedTransitions[$fromStatus] -notcontains $ToStatus)) {
        throw "Invalid transition $fromStatus -> $ToStatus. Use -Force to override."
    }
}

$today = (Get-Date).ToString('yyyy-MM-dd')
$section = Set-GoalField -Section $section -Field 'Status' -Value "``$ToStatus``" -Newline $newline
$section = Set-GoalField -Section $section -Field 'Last reviewed' -Value $today -Newline $newline
$section = Set-GoalField -Section $section -Field 'Status Updated By' -Value '`scripts/promote-sequence-goal.ps1`' -Newline $newline

if (-not [string]::IsNullOrWhiteSpace($TaskRef)) {
    $section = Set-GoalField -Section $section -Field 'Task/Issue' -Value $TaskRef -Newline $newline
}

if (-not [string]::IsNullOrWhiteSpace($PrUrl)) {
    $section = Set-GoalField -Section $section -Field 'PR' -Value $PrUrl -Newline $newline
}

if (-not [string]::IsNullOrWhiteSpace($TransitionNote)) {
    $section = Set-GoalField -Section $section -Field 'Transition note' -Value $TransitionNote -Newline $newline
}

if ($ToStatus -eq 'Complete' -and -not [string]::IsNullOrWhiteSpace($CompletedBy)) {
    $section = Set-GoalField -Section $section -Field 'Completed by' -Value $CompletedBy -Newline $newline
}
if ($ToStatus -eq 'Complete' -and -not [string]::IsNullOrWhiteSpace($MaximumAgentResources)) {
    $section = Set-GoalField -Section $section -Field 'Maximum agent resources' -Value $MaximumAgentResources -Newline $newline
}
if ($ToStatus -eq 'Complete' -and -not [string]::IsNullOrWhiteSpace($ActualAgentResources)) {
    $section = Set-GoalField -Section $section -Field 'Actual agent resources' -Value $ActualAgentResources -Newline $newline
}
if ($ToStatus -eq 'Complete' -and -not [string]::IsNullOrWhiteSpace($ProductionAgentResources)) {
    $section = Set-GoalField -Section $section -Field 'Production code agent resources' -Value $ProductionAgentResources -Newline $newline
}
if ($ToStatus -eq 'Complete' -and -not [string]::IsNullOrWhiteSpace($AutomationAgentResources)) {
    $section = Set-GoalField -Section $section -Field 'Automation code agent resources' -Value $AutomationAgentResources -Newline $newline
}

$diagramUpdated = $diagramText.Substring(0, $goalMatch.Groups['section'].Index) + $section + $diagramText.Substring($goalMatch.Groups['section'].Index + $goalMatch.Groups['section'].Length)
Write-Utf8NoBom -Path $diagramAbsolutePath -Content $diagramUpdated

$indexPath = Join-Path (Join-Path $repoRoot 'docs') 'sequence-diagrams.md'
$indexText = Get-Content -LiteralPath $indexPath -Raw
$rowPath = ('sequence-diagrams/{0}' -f [System.IO.Path]::GetFileName($diagramAbsolutePath))
$rowPattern = '(?im)^(\|\s*\[[^\]]+\]\(' + [regex]::Escape($rowPath) + '\)\s*\|\s*[^|]*\|\s*)`[^`]+`(\s*\|\s*)$'
if (-not [regex]::IsMatch($indexText, $rowPattern)) {
    throw "Could not find matching index row for $rowPath in docs/sequence-diagrams.md"
}

$rowRegex = New-Object System.Text.RegularExpressions.Regex($rowPattern)
$indexUpdated = $rowRegex.Replace(
    $indexText,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($match)
        return "$($match.Groups[1].Value)``$ToStatus``$($match.Groups[2].Value)"
    },
    1
)
Write-Utf8NoBom -Path $indexPath -Content $indexUpdated

Write-Host "Promoted ${diagramRelativePath}: $fromStatus -> $ToStatus" -ForegroundColor Green
exit 0
