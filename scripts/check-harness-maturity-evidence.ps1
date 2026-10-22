#Requires -Version 5.1
[CmdletBinding()]
param([string]$LedgerPath, [string]$SummaryPath, [switch]$RequireMature)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$policyPath = Join-Path $repoRoot 'docs/harness-maturity-policy.json'
$failures = [System.Collections.Generic.List[string]]::new()
$jsonParams = @{}
if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
try { $policy = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json @jsonParams } catch { Write-Host 'harness maturity check failed: invalid policy' -ForegroundColor Red; exit 1 }
if ([int]$policy.schemaVersion -ne 1 -or [int]$policy.targetConsecutivePassingCycles -lt 2 -or [int]$policy.maxRetainedCycles -lt [int]$policy.targetConsecutivePassingCycles) { $failures.Add('Maturity policy shape is invalid.') }
if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ([string]$policy.evidenceWorkflow)) -PathType Leaf)) { $failures.Add('Maturity evidence workflow is missing.') }
$operational = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/harness-operational-capabilities.json') -Raw | ConvertFrom-Json @jsonParams
$operationalIds = @($operational.capabilities.id)
foreach ($id in @($policy.trackedCapabilities)) { if ($operationalIds -notcontains [string]$id) { $failures.Add("Tracked capability '$id' is absent from the operational registry.") } }

if (-not [string]::IsNullOrWhiteSpace($LedgerPath) -or -not [string]::IsNullOrWhiteSpace($SummaryPath)) {
    if (-not (Test-Path -LiteralPath $LedgerPath -PathType Leaf) -or -not (Test-Path -LiteralPath $SummaryPath -PathType Leaf)) { $failures.Add('Both retained ledger and summary paths are required.') }
    else {
        try { $ledger = Get-Content -LiteralPath $LedgerPath -Raw | ConvertFrom-Json @jsonParams; $summary = Get-Content -LiteralPath $SummaryPath -Raw | ConvertFrom-Json @jsonParams } catch { $failures.Add('Retained maturity evidence is invalid JSON.') }
        if ($null -ne $ledger -and $null -ne $summary) {
            $cycles = @($ledger.cycles); $runIds = @($cycles.runId)
            if ([int]$summary.cycleCount -ne $cycles.Count -or [string]$summary.evidenceWorkflow -ne [string]$policy.evidenceWorkflow) { $failures.Add('Maturity summary count or workflow provenance is inconsistent.') }
            if ($cycles.Count -gt 0 -and [string]$summary.latestRunId -ne [string]$cycles[0].runId) { $failures.Add('Maturity summary latestRunId is inconsistent.') }
            if (($runIds | Select-Object -Unique).Count -ne $runIds.Count) { $failures.Add('Maturity ledger contains duplicate run IDs.') }
            if ($cycles.Count -gt [int]$policy.maxRetainedCycles) { $failures.Add('Maturity ledger exceeds its bounded history.') }
            foreach ($cycle in $cycles) {
                if ([string]$cycle.workflow -ne [string]$policy.evidenceWorkflow -or @('success', 'failure', 'cancelled') -notcontains [string]$cycle.outcome -or [string]::IsNullOrWhiteSpace([string]$cycle.trigger)) { $failures.Add("Cycle '$($cycle.runId)' has invalid provenance, trigger, or outcome.") }
            }
            $orderedIds = @($cycles | Sort-Object { [DateTimeOffset]::Parse([string]$_.completedAtUtc) } -Descending | ForEach-Object runId)
            if (($orderedIds -join '|') -ne ($runIds -join '|')) { $failures.Add('Maturity ledger cycles are not ordered newest first.') }
            $expectedAllMature = $true
            foreach ($id in @($policy.trackedCapabilities)) {
                $record = @($summary.capabilities | Where-Object id -eq $id) | Select-Object -First 1
                if ($null -eq $record) { $failures.Add("Maturity summary is missing '$id'."); continue }
                $expected = 0
                foreach ($cycle in $cycles) { if ([string]$cycle.trigger -ne 'schedule') { continue }; if (@($cycle.capabilities) -notcontains $id) { continue }; if ([string]$cycle.outcome -ne 'success') { break }; $expected++ }
                if ([int]$record.consecutivePassingCycles -ne $expected -or [bool]$record.mature -ne ($expected -ge [int]$policy.targetConsecutivePassingCycles)) { $failures.Add("Maturity summary is inconsistent for '$id'.") }
                if ($expected -lt [int]$policy.targetConsecutivePassingCycles) { $expectedAllMature = $false }
            }
            if ([bool]$summary.allMature -ne $expectedAllMature) { $failures.Add('Maturity summary allMature value is inconsistent.') }
            if ($RequireMature -and $summary.allMature -ne $true) { $failures.Add('Tracked capabilities have not completed the required consecutive scheduled cycles.') }
        }
    }
} elseif ($RequireMature) { $failures.Add('-RequireMature requires retained -LedgerPath and -SummaryPath evidence.') }

if ($failures.Count -gt 0) { Write-Host "harness maturity check failed: $($failures -join ' ')" -ForegroundColor Red; exit 1 }
Write-Host 'harness maturity policy/evidence check passed' -ForegroundColor Green
exit 0
