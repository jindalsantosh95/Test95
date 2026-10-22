#Requires -Version 7.0
[CmdletBinding()]
param([string]$Branch, [string]$RunId, [string]$Attempt, [bool]$ExpectBudget = $false)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
$root = Split-Path $PSScriptRoot -Parent
$directory = Join-Path ([IO.Path]::GetTempPath()) "goal-cost-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory $directory | Out-Null
$artifacts = gh api "repos/$env:GITHUB_REPOSITORY/actions/runs/$RunId/artifacts?per_page=100" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $artifacts.total_count -gt 100) { throw 'Cannot enumerate this run resource artifacts.' }
foreach ($phase in @('production','automation')) {
    $pattern = "$phase-agent-resource-*-$RunId-$Attempt"
    if (@($artifacts.artifacts | Where-Object name -Like $pattern).Count -eq 0) { continue }
    gh run download $RunId --repo $env:GITHUB_REPOSITORY --pattern $pattern --dir (Join-Path $directory $phase) | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Cannot download this run resource evidence.' }
}
$files = @(Get-ChildItem $directory -Recurse -Filter '*.budget.json' -File)
$markers = @(Get-ChildItem $directory -Recurse -Filter '*.executed' -File)
foreach ($marker in $markers) {
    if (-not (Test-Path ($marker.FullName.Substring(0, $marker.FullName.Length - 9) + '.budget.json'))) { throw 'Executed agent has no budget evidence.' }
}
if ($files.Count -eq 0) {
    if ($ExpectBudget -or $markers.Count) { throw 'Executed agent has no budget evidence.' }
    $global:LASTEXITCODE = 0
    return
}
$entries = @()
foreach ($file in $files) {
    & "$PSScriptRoot/get-agent-resource-summary.ps1" -WorkflowId sequence-goal-loop -BudgetEvidencePath $file.FullName | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Invalid budget evidence.' }
    $phase = if ($file.FullName -match '[\\/]automation[\\/]') { 'automation' } else { 'production' }
    $entries += [ordered]@{ key="$RunId-$Attempt-$phase-$($file.Name)"; budget=(Get-Content $file.FullName -Raw | ConvertFrom-Json) }
}
$comparison = gh api "repos/$env:GITHUB_REPOSITORY/compare/main...$([uri]::EscapeDataString($Branch))" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Cannot locate goal document.' }
$paths = @($comparison.files | Where-Object filename -Match '^docs/sequence-diagrams/(?!_)[^/]+\.md$')
if ($paths.Count -ne 1) { throw 'Expected one goal document.' }
$library = Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1'
$edit = {
    param($path)
    . $library
    $text = Get-Content $path -Raw
    $current = Get-GoalTaskState $text 'Resources'
    $byKey = [ordered]@{}
    foreach ($entry in @($current.attempts) + $entries) { if ($entry) { $byKey[$entry.key] = $entry } }
    $updated = Set-GoalTaskState $text 'Resources' @{ attempts=@($byKey.Values) }
    [IO.File]::WriteAllText((Join-Path $pwd $path), $updated)
}.GetNewClosure()
Update-SequenceGoalDocument -RepositoryRoot $root -Branch $Branch -DiagramPath $paths[0].filename -Edit $edit -Message 'chore(sequence-goal): record attempt cost'
