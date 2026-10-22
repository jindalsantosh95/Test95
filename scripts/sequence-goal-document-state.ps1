#Requires -Version 7.0
. (Join-Path $PSScriptRoot 'update-sequence-goal-document.ps1')

function Get-GoalTaskState {
    param([string]$Text, [string]$Task)
    $pattern = '(?ms)^### Delivery: ' + [regex]::Escape($Task) + '\r?\n```json\r?\n(?<json>.*?)\r?\n```\s*(?=^### |^## |\z)'
    $matches = [regex]::Matches($Text, $pattern)
    if ($matches.Count -gt 1) { throw "Duplicate delivery task: $Task" }
    if ($matches.Count -eq 1) { return $matches[0].Groups['json'].Value | ConvertFrom-Json }
    if ($Text -match ('(?m)^### Delivery: ' + [regex]::Escape($Task) + '\s*$')) { throw "Malformed delivery task: $Task" }
    return $null
}

function Set-GoalTaskState {
    param([string]$Text, [string]$Task, [object]$State)
    $existing = Get-GoalTaskState $Text $Task
    if ($null -ne $existing -and ($existing | ConvertTo-Json -Depth 15 -Compress) -ceq ($State | ConvertTo-Json -Depth 15 -Compress)) { return $Text }
    $section = '### Delivery: ' + $Task + "`n" + '```json' + "`n" + ($State | ConvertTo-Json -Depth 15) + "`n" + '```' + "`n`n"
    $pattern = '(?ms)^### Delivery: ' + [regex]::Escape($Task) + '\r?\n```json\r?\n.*?\r?\n```\s*(?=^### |^## |\z)'
    if ($Text -match $pattern) { return [regex]::Replace($Text, $pattern, { $section }) }
    $progress = '(?ms)^## Delivery progress\r?\n.*?(?=^## |\z)'
    if ($Text -match $progress) {
        return [regex]::Replace($Text, $progress, { param($match) $match.Value.TrimEnd() + "`n`n$section" })
    }
    return $Text.TrimEnd() + "`n`n## Delivery progress`n`n$section"
}

function Read-GoalTaskState {
    param([string]$RepositoryRoot, [string]$Branch, [string]$DiagramPath, [string]$Task)
    git -C $RepositoryRoot fetch --no-tags origin "refs/heads/$Branch" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Cannot refresh goal state.' }
    $text = (git -C $RepositoryRoot show "FETCH_HEAD:$DiagramPath") -join "`n"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read goal document.' }
    Get-GoalTaskState $text $Task
}

function Write-GoalTaskState {
    param([string]$RepositoryRoot, [string]$Branch, [string]$DiagramPath, [string]$Task, [object]$State, [string]$ExpectedGoalDigest)
    $stateLibrary = Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1'
    $readinessLibrary = Join-Path $PSScriptRoot 'sequence-goal-production-readiness.ps1'
    $edit = {
        param($path)
        . $stateLibrary
        $text = Get-Content -LiteralPath $path -Raw
        if ($ExpectedGoalDigest) {
            . $readinessLibrary
            if ($ExpectedGoalDigest -cnotmatch '^[a-f0-9]{64}$' -or (Get-ProductionGoalDigest $text) -cne $ExpectedGoalDigest) {
                throw 'Goal requirements changed after verification; reverify before recording completion.'
            }
        }
        $updated = Set-GoalTaskState $text $Task $State
        if ($updated -cne $text) { [IO.File]::WriteAllText((Join-Path $pwd $path), $updated) }
    }.GetNewClosure()
    Update-SequenceGoalDocument -RepositoryRoot $RepositoryRoot -Branch $Branch -DiagramPath $DiagramPath -Edit $edit -Message "chore(sequence-goal): $Task $($State.status)"
}

function Get-GoalCoordinatorState {
    param([string]$Path)
    Get-GoalTaskState (Get-Content -LiteralPath $Path -Raw) Coordinator
}

function Set-GoalCoordinatorState {
    param([string]$Path, [object]$State)
    $text = Get-Content -LiteralPath $Path -Raw
    [IO.File]::WriteAllText((Resolve-Path -LiteralPath $Path).Path, (Set-GoalTaskState $text Coordinator $State))
}
