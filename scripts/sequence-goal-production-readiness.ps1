function Get-ProductionGoalDigest {
    param([Parameter(Mandatory)][string]$Text)
    # Lifecycle/cost retention must not invalidate the semantic implementation checkpoint.
    $Text = [regex]::Replace($Text, '(?ms)^### Delivery: .*?\r?\n```json\r?\n.*?\r?\n```\s*', '')
    # QA publishes evidence in these rows without changing the requirements.
    $Text = [regex]::Replace($Text, '(?m)^\|\s*(?:Harness Sensor|Acceptance evidence)\s*\|[^\r\n]*\r?\n?', '')
    $semantic = [regex]::Replace($Text, '(?ms)^## (?:Associated delivery pull requests|Historical associated delivery pull requests|Delivery progress|Agent Loop Closure Evidence)\b.*?(?=^## |\z)', '')
    $semantic = [regex]::Replace($semantic, '(?ms)^## Agent Loop Goal\s*\n(?<table>.*?)(?=^## |\z)', {
        param($match)
        ($match.Groups['table'].Value -split '\r?\n' | Where-Object { $_ -match '^\|\s*(Goal|Delivery surface|Done when|Promotion verification)\s*\|' }) -join "`n"
    })
    $bytes = [Text.Encoding]::UTF8.GetBytes(($semantic -replace '\r\n', "`n").Trim())
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant() } finally { $sha.Dispose() }
}

# Shared production outcome and immutable-head readiness contract.
function ConvertFrom-ProductionOutcome {
    param([Parameter(Mandatory)][string]$Message, [switch]$RequireStopReason)
    try { $result = $Message | ConvertFrom-Json -ErrorAction Stop } catch { throw 'Production agent must return a JSON outcome, not a prose completion message.' }
    if ($result.status -cnotin @('Complete', 'Incomplete', 'Blocked') -or
        [string]::IsNullOrWhiteSpace([string]$result.summary)) { throw 'Invalid production outcome status or summary.' }
    if ($result.remainingWork -isnot [array] -or $result.acceptanceEvidence -isnot [array] -or
        $result.verification -isnot [array]) { throw 'Production outcome requires remainingWork, acceptanceEvidence and verification arrays.' }
    foreach ($entry in @($result.acceptanceEvidence) + @($result.remainingWork)) {
        if ($entry -isnot [string] -or [string]::IsNullOrWhiteSpace($entry)) { throw 'Production evidence and remaining work must contain nonempty strings.' }
    }
    if ($result.status -eq 'Complete' -and (@($result.verification).Count -eq 0 -or
        @($result.verification | Where-Object { [string]::IsNullOrWhiteSpace([string]$_.command) -or $_.exitCode -isnot [long] -and $_.exitCode -isnot [int] -or $_.exitCode -ne 0 }).Count -gt 0)) {
        throw 'Complete production requires successful mandatory verification commands with integer exit codes.'
    }
    if ($result.status -eq 'Complete' -and (@($result.remainingWork).Count -ne 0 -or @($result.acceptanceEvidence).Count -eq 0)) {
        throw 'Complete production requires acceptance evidence and no remaining work.'
    }
    if ($result.status -ne 'Complete' -and @($result.remainingWork).Count -eq 0) { throw 'Incomplete or Blocked production must identify remaining work.' }
    if ($RequireStopReason -and $result.status -ne 'Complete' -and [string]::IsNullOrWhiteSpace([string]$result.stopReason)) { throw 'Incomplete or Blocked production must explain stopReason.' }
    return $result
}

function Test-ProductionReadiness {
    param($State, [string]$HeadSha, [string]$GoalDigest = $State.currentGoalDigest)
    if ($HeadSha -notmatch '^[a-f0-9]{40}$' -or $null -eq $State) { return $false }
    return $State.status -ceq 'Complete' -and $State.verifiedCommit -ceq $HeadSha -and
        $GoalDigest -cmatch '^[a-f0-9]{64}$' -and ([string]$State.identity -split "`n")[-1] -ceq $GoalDigest
}

# Read the authoritative document from the coordinator, even in a target-repository job.
function Test-QaReadiness {
    param($State, [string]$ProductionHead, [string]$QaHead, [string]$GoalDigest = $State.currentGoalDigest)
    return $null -ne $State -and $State.status -ceq 'Complete' -and
        $ProductionHead -cmatch '^[a-f0-9]{40}$' -and $QaHead -cmatch '^[a-f0-9]{40}$' -and
        $State.productionCommit -ceq $ProductionHead -and $State.verifiedCommit -ceq $ProductionHead -and
        $State.verifiedQaCommit -ceq $QaHead -and $GoalDigest -cmatch '^[a-f0-9]{64}$' -and $State.goalDigest -ceq $GoalDigest
}

function Read-DeliveryGoalState {
    param([string]$Repository, [string]$Branch, [string]$Surface, [string]$Phase = 'Production')
    . (Join-Path $PSScriptRoot 'sequence-goal-document-state.ps1')
    $priorToken = $env:GH_TOKEN
    try {
        if ($env:COORDINATOR_GH_TOKEN) { $env:GH_TOKEN = $env:COORDINATOR_GH_TOKEN }
        $comparison = gh api "repos/$Repository/compare/main...$([uri]::EscapeDataString($Branch))" | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0) { throw 'Cannot locate delivery goal.' }
        $paths = @($comparison.files | Where-Object { $_.filename -match '^docs/sequence-diagrams/(?!_)[^/]+\.md$' })
        if ($paths.Count -ne 1) { throw 'Expected exactly one delivery goal document.' }
        $file = gh api "repos/$Repository/contents/$($paths[0].filename)?ref=$([uri]::EscapeDataString($Branch))" | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0 -or $file.encoding -ne 'base64') { throw 'Cannot read delivery goal.' }
        $text = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($file.content))
        $state = Get-GoalTaskState $text "$Surface $Phase"
        if ($null -ne $state) {
            $state | Add-Member -NotePropertyName currentGoalDigest -NotePropertyValue (Get-ProductionGoalDigest $text) -Force
        }
        return $state
    } finally { $env:GH_TOKEN = $priorToken }
}
