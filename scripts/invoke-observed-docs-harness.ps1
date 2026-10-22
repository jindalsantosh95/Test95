#Requires -Version 5.1
[CmdletBinding()]
param([string]$RunId = ([guid]::NewGuid().ToString('N')))

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$safeRunId = $RunId -replace '[^A-Za-z0-9_.-]', '_'
$runRoot = Join-Path $repoRoot "TestResults/docs-harness-observability/$safeRunId"
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
$started = [datetime]::UtcNow
$traceId = [guid]::NewGuid().ToString('N')
$outputPath = Join-Path $runRoot 'verify-output.txt'

& (Join-Path $PSScriptRoot 'verify.ps1') -ScopePath docs -SkipBuild -SkipDeploymentCheck *>&1 |
    ForEach-Object { ([string]$_).Replace($repoRoot, '<repo>') } |
    Tee-Object -FilePath $outputPath | ForEach-Object { Write-Host $_ }
$exitCode = $LASTEXITCODE
$completed = [datetime]::UtcNow
$durationMs = [math]::Round(($completed - $started).TotalMilliseconds)
$outcome = if ($exitCode -eq 0) { 'passed' } else { 'failed' }

@(
    [ordered]@{ timestampUtc = $started.ToString('o'); level = 'Information'; event = 'docs_harness_started'; runId = $safeRunId; traceId = $traceId },
    [ordered]@{ timestampUtc = $completed.ToString('o'); level = $(if ($exitCode -eq 0) { 'Information' } else { 'Error' }); event = 'docs_harness_completed'; runId = $safeRunId; traceId = $traceId; outcome = $outcome; exitCode = $exitCode }
) | ForEach-Object { ($_ | ConvertTo-Json -Compress) } | Set-Content -LiteralPath (Join-Path $runRoot 'logs.jsonl') -Encoding utf8

[ordered]@{ schemaVersion = 1; runId = $safeRunId; measuredAtUtc = $completed.ToString('o'); metrics = @(@{ name = 'docs_harness_duration_ms'; value = $durationMs }, @{ name = 'docs_harness_success'; value = $(if ($exitCode -eq 0) { 1 } else { 0 }) }) } |
    ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'metrics.json') -Encoding utf8
[ordered]@{ schemaVersion = 1; traceId = $traceId; runId = $safeRunId; spans = @(@{ name = 'docs-harness-verify'; startedAtUtc = $started.ToString('o'); completedAtUtc = $completed.ToString('o'); durationMs = $durationMs; status = $outcome }) } |
    ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'traces.json') -Encoding utf8
[ordered]@{ schemaVersion = 1; runId = $safeRunId; traceId = $traceId; environment = 'local'; component = 'repository-docs-harness'; outcome = $outcome; exitCode = $exitCode; completedAtUtc = $completed.ToString('o'); evidencePath = "TestResults/docs-harness-observability/$safeRunId" } |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $runRoot 'summary.json') -Encoding utf8

Write-Host "observability evidence: TestResults/docs-harness-observability/$safeRunId" -ForegroundColor Cyan
exit $exitCode
