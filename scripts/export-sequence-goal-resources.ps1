# Disposable input files for the existing cost calculator; the document owns history.
param([string[]]$DiagramPaths)
. (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
$directory = Join-Path ([IO.Path]::GetTempPath()) "goal-budget-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory $directory | Out-Null
foreach ($diagram in $DiagramPaths) {
    $state = Get-GoalTaskState (Get-Content $diagram -Raw) Resources
    foreach ($entry in @($state.attempts)) {
        if (-not $entry) { continue }
        if ($entry.key -notmatch '^[A-Za-z0-9_.-]+$') { throw 'Invalid goal resource key.' }
        $path = Join-Path $directory $entry.key
        $entry.budget | ConvertTo-Json -Depth 12 | Set-Content $path -Encoding utf8
        $path
    }
}
