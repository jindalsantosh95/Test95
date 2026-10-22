#Requires -Version 5.1
[CmdletBinding()]
param([string]$EvidenceDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) { $EvidenceDirectory = Join-Path $repoRoot ('TestResults/agent-observability/' + [guid]::NewGuid().ToString('N')) }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$sourceRootRelative = ('TestResults/agent-observability/' + (Split-Path -Leaf $EvidenceDirectory) + '/source').Replace('\', '/')
$sourceRoot = Join-Path $repoRoot $sourceRootRelative
$runId = 'observability-exercise'
$runRoot = Join-Path $sourceRoot $runId
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
$now = (Get-Date).ToUniversalTime(); $started = $now.AddSeconds(-2); $traceId = [guid]::NewGuid().ToString('N')
@([ordered]@{ timestampUtc = $started.ToString('o'); level = 'Information'; event = 'exercise_started'; runId = $runId; traceId = $traceId }, [ordered]@{ timestampUtc = $now.ToString('o'); level = 'Information'; event = 'exercise_completed'; runId = $runId; traceId = $traceId; outcome = 'passed' }) | ForEach-Object { $_ | ConvertTo-Json -Compress } | Set-Content -LiteralPath (Join-Path $runRoot 'logs.jsonl') -Encoding utf8
[ordered]@{ schemaVersion = 1; runId = $runId; measuredAtUtc = $now.ToString('o'); metrics = @(@{ name = 'docs_harness_duration_ms'; value = 2000 }, @{ name = 'docs_harness_success'; value = 1 }) } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'metrics.json') -Encoding utf8
[ordered]@{ schemaVersion = 1; traceId = $traceId; runId = $runId; spans = @(@{ name = 'exercise'; startedAtUtc = $started.ToString('o'); completedAtUtc = $now.ToString('o'); durationMs = 2000; status = 'passed' }) } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'traces.json') -Encoding utf8
[ordered]@{ schemaVersion = 1; runId = $runId; traceId = $traceId; environment = 'local'; component = 'repository-docs-harness'; outcome = 'passed'; exitCode = 0; completedAtUtc = $now.ToString('o'); evidencePath = $sourceRootRelative } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'summary.json') -Encoding utf8

$query = Join-Path $PSScriptRoot 'query-agent-observability.ps1'
$queryOutput = Join-Path $EvidenceDirectory 'bounded-query.json'
& $query -RunId $runId -StartUtc $now.AddMinutes(-5) -EndUtc $now.AddMinutes(1) -Signal all -RequireMatch -EvidenceRootOverride $sourceRootRelative -OutputPath $queryOutput
$boundedPassed = $LASTEXITCODE -eq 0
& $query -RunId $runId -StartUtc $now.AddHours(-2) -EndUtc $now -EvidenceRootOverride $sourceRootRelative
$wideRejected = $LASTEXITCODE -ne 0
& $query -StartUtc $now.AddMinutes(-5) -EndUtc $now -EvidenceRootOverride $sourceRootRelative
$missingCorrelationRejected = $LASTEXITCODE -ne 0
$queryRecord = Get-Content -LiteralPath $queryOutput -Raw | ConvertFrom-Json
$checks = [ordered]@{ boundedCorrelationQueryPassed = $boundedPassed -and $queryRecord.passed -eq $true; allSignalsReturned = @($queryRecord.items).Count -eq 4; oversizedWindowRejected = $wideRejected; missingCorrelationRejected = $missingCorrelationRejected }
$passed = @($checks.GetEnumerator() | Where-Object { $_.Value -ne $true }).Count -eq 0
$summaryPath = Join-Path $EvidenceDirectory 'summary.json'
[ordered]@{ schemaVersion = 1; capability = 'runtime-observability-learning'; passed = $passed; exercisedAtUtc = $now.ToString('o'); checks = $checks } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding utf8
if (-not $passed) { Write-Host 'agent observability exercise failed' -ForegroundColor Red; exit 1 }
Write-Host "agent observability exercise passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
Write-Host "evidence: $summaryPath" -ForegroundColor DarkGray
exit 0
