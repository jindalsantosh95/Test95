#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$BehaviorHarnessPath = 'docs/behavior-harness.md',
    [string]$SequenceDiagramPath = 'docs/sequence-diagrams'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
function Resolve-RepoPath([string]$Value) {
    $candidate = if ([System.IO.Path]::IsPathRooted($Value)) { $Value } else { Join-Path $repoRoot $Value }
    $resolved = (Resolve-Path -LiteralPath $candidate).Path
    if (-not $resolved.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'Behavior harness inputs must stay inside the repository.' }
    return $resolved
}
$harnessPath = Resolve-RepoPath $BehaviorHarnessPath
$diagramRoot = Resolve-RepoPath $SequenceDiagramPath
$failures = [System.Collections.Generic.List[string]]::new()
$text = Get-Content -LiteralPath $harnessPath -Raw
$sectionMatch = [regex]::Match($text, '(?ims)^##\s+Sensor Catalog\s*$\r?\n(?<section>.*?)(?=^##\s+|\z)')
if (-not $sectionMatch.Success) { Write-Error 'Behavior harness has no Sensor Catalog section.'; exit 1 }
$rows = @([regex]::Matches($sectionMatch.Groups['section'].Value, '(?im)^\|\s*`(?<behavior>vaa-eapi-[^`]+)`\s*\|\s*(?<sensor>.*?)\s*\|\s*(?<proof>.*?)\s*\|\s*$'))
$catalog = @{}
foreach ($row in $rows) {
    $behavior = $row.Groups['behavior'].Value.Trim()
    if ($catalog.ContainsKey($behavior)) { $failures.Add("Duplicate behavior catalog row: $behavior"); continue }
    $catalog[$behavior] = $true
    $sensorCell = $row.Groups['sensor'].Value
    if ($sensorCell -match '(?i)\b(planned|tbd|placeholder|none)\b') { $failures.Add("Implemented behavior '$behavior' uses a placeholder sensor.") }
    $paths = @([regex]::Matches($sensorCell, '`(?<path>[A-Za-z0-9_.\-/]+\.(cs|ps1|yml|yaml|json))`') | ForEach-Object { $_.Groups['path'].Value })
    if ($paths.Count -eq 0) { $failures.Add("Implemented behavior '$behavior' has no concrete sensor path.") }
    foreach ($path in $paths) {
        if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $path) -PathType Leaf)) { $failures.Add("Behavior '$behavior' references missing sensor: $path") }
    }
}

$complete = @()
foreach ($diagram in Get-ChildItem -LiteralPath $diagramRoot -Filter '*.md' -File) {
    if ($diagram.Name -match '^(?i)_template') { continue }
    $diagramText = Get-Content -LiteralPath $diagram.FullName -Raw
    if ($diagramText -notmatch '(?im)^\|\s*Status\s*\|\s*`Complete`\s*\|\s*$') { continue }
    $behavior = [System.IO.Path]::GetFileNameWithoutExtension($diagram.Name)
    $complete += $behavior
    if (-not $catalog.ContainsKey($behavior)) { $failures.Add("Complete behavior '$behavior' is missing from the Sensor Catalog.") }
    $sensorMatch = [regex]::Match($diagramText, '(?im)^\|\s*Harness Sensor\s*\|\s*(?<sensor>.*?)\s*\|\s*$')
    if (-not $sensorMatch.Success -or $sensorMatch.Groups['sensor'].Value -match '(?i)\b(planned|tbd|placeholder|none)\b') {
        $failures.Add("Complete behavior '$behavior' lacks a concrete diagram sensor.")
    }
}
if ($complete.Count -eq 0) { $failures.Add('No Complete sequence behaviors were discovered.') }

if ($failures.Count -gt 0) {
    Write-Host 'behavior harness check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "behavior harness check passed ($($catalog.Count) catalog behaviors; $($complete.Count) Complete flows enrolled)" -ForegroundColor Green
exit 0
