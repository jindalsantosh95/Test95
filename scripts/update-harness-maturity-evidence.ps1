#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$HistoryDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [Parameter(Mandatory)][string]$RunId,
    [Parameter(Mandatory)][ValidateSet('success', 'failure', 'cancelled')][string]$Outcome,
    [string]$Trigger = 'exercise',
    [datetime]$CompletedAtUtc = [datetime]::UtcNow
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$jsonParams = @{}
if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
$policy = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/harness-maturity-policy.json') -Raw | ConvertFrom-Json @jsonParams
$cycles = @()
$candidate = Get-ChildItem -LiteralPath $HistoryDirectory -Recurse -Filter 'maturity-ledger.json' -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if ($null -ne $candidate) {
    $previous = Get-Content -LiteralPath $candidate.FullName -Raw | ConvertFrom-Json @jsonParams
    if ([int]$previous.schemaVersion -ne 1) { throw 'Previous maturity ledger has unsupported schemaVersion.' }
    $cycles = @($previous.cycles)
}
if (@($cycles | Where-Object runId -eq $RunId).Count -gt 0) { throw "Maturity run '$RunId' already exists; duplicate cycles are rejected." }

$cycles += [ordered]@{ runId = $RunId; workflow = [string]$policy.evidenceWorkflow; trigger = $Trigger; completedAtUtc = $CompletedAtUtc.ToUniversalTime().ToString('o'); outcome = $Outcome; capabilities = @($policy.trackedCapabilities) }
$cycles = @($cycles | Sort-Object { [DateTimeOffset]::Parse([string]$_.completedAtUtc) } -Descending | Select-Object -First ([int]$policy.maxRetainedCycles))
$capabilities = @()
foreach ($capabilityId in @($policy.trackedCapabilities)) {
    $consecutive = 0
    foreach ($cycle in $cycles) {
        if ([string]$cycle.trigger -ne 'schedule') { continue }
        if (@($cycle.capabilities) -notcontains $capabilityId) { continue }
        if ([string]$cycle.outcome -ne 'success') { break }
        $consecutive++
    }
    $capabilities += [ordered]@{ id = $capabilityId; consecutivePassingCycles = $consecutive; targetCycles = [int]$policy.targetConsecutivePassingCycles; mature = $consecutive -ge [int]$policy.targetConsecutivePassingCycles }
}
$allMature = @($capabilities | Where-Object mature -ne $true).Count -eq 0
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
[ordered]@{ schemaVersion = 1; generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); cycles = $cycles } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'maturity-ledger.json') -Encoding utf8
[ordered]@{ schemaVersion = 1; generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); evidenceWorkflow = [string]$policy.evidenceWorkflow; allMature = $allMature; cycleCount = $cycles.Count; latestRunId = $RunId; capabilities = $capabilities } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'maturity-summary.json') -Encoding utf8
Write-Host "harness maturity evidence updated: cycles=$($cycles.Count) allMature=$allMature" -ForegroundColor Green
exit 0
