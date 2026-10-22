#Requires -Version 5.1
[CmdletBinding()]
param([string]$EvidenceDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) { $EvidenceDirectory = Join-Path $repoRoot ('TestResults/harness-maturity/' + [guid]::NewGuid().ToString('N')) }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$updater = Join-Path $PSScriptRoot 'update-harness-maturity-evidence.ps1'; $checker = Join-Path $PSScriptRoot 'check-harness-maturity-evidence.ps1'
$empty = Join-Path $EvidenceDirectory 'empty'; New-Item -ItemType Directory -Path $empty -Force | Out-Null
$cycle1 = Join-Path $EvidenceDirectory 'cycle1'; & $updater -HistoryDirectory $empty -OutputDirectory $cycle1 -RunId exercise-1 -Outcome success -Trigger schedule -CompletedAtUtc ([datetime]'2026-08-07T01:00:00Z')
$cycle2 = Join-Path $EvidenceDirectory 'cycle2'; & $updater -HistoryDirectory $cycle1 -OutputDirectory $cycle2 -RunId exercise-2 -Outcome success -Trigger schedule -CompletedAtUtc ([datetime]'2026-08-07T02:00:00Z')
& $checker -LedgerPath (Join-Path $cycle2 'maturity-ledger.json') -SummaryPath (Join-Path $cycle2 'maturity-summary.json') -RequireMature
$twoPassesMature = $LASTEXITCODE -eq 0
$cycle3 = Join-Path $EvidenceDirectory 'cycle3'; & $updater -HistoryDirectory $cycle2 -OutputDirectory $cycle3 -RunId exercise-3 -Outcome failure -Trigger schedule -CompletedAtUtc ([datetime]'2026-08-07T03:00:00Z')
& $checker -LedgerPath (Join-Path $cycle3 'maturity-ledger.json') -SummaryPath (Join-Path $cycle3 'maturity-summary.json') -RequireMature
$failureResetRejected = $LASTEXITCODE -ne 0
$duplicateRejected = $false
try { & $updater -HistoryDirectory $cycle3 -OutputDirectory (Join-Path $EvidenceDirectory 'duplicate') -RunId exercise-3 -Outcome success; $duplicateRejected = $LASTEXITCODE -ne 0 } catch { $duplicateRejected = $true }
$cycle3Summary = Get-Content -LiteralPath (Join-Path $cycle3 'maturity-summary.json') -Raw | ConvertFrom-Json
$checks = [ordered]@{ firstCycleNotMature = (Get-Content -LiteralPath (Join-Path $cycle1 'maturity-summary.json') -Raw | ConvertFrom-Json).allMature -eq $false; twoConsecutivePassesMature = $twoPassesMature; failedCycleResetsPromotion = $failureResetRejected -and $cycle3Summary.allMature -eq $false; duplicateRunRejected = $duplicateRejected }
$passed = @($checks.GetEnumerator() | Where-Object { $_.Value -ne $true }).Count -eq 0
$summaryPath = Join-Path $EvidenceDirectory 'summary.json'
[ordered]@{ schemaVersion = 1; capability = 'continuous-maturity-evidence'; passed = $passed; exercisedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); checks = $checks } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding utf8
if (-not $passed) { Write-Host 'harness maturity evidence exercise failed' -ForegroundColor Red; exit 1 }
Write-Host "harness maturity evidence exercise passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
Write-Host "evidence: $summaryPath" -ForegroundColor DarkGray
exit 0
