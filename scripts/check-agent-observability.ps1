#Requires -Version 5.1
[CmdletBinding()]
param([switch]$RequireEvidence)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$registryPath = Join-Path $repoRoot 'docs/agent-observability-sources.json'
$feedbackPath = Join-Path $repoRoot 'docs/harness-runtime-feedback.md'
$failures = [System.Collections.Generic.List[string]]::new()

try { $registry = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json }
catch { Write-Error "Invalid or missing observability source registry: $($_.Exception.Message)"; exit 1 }

if ($registry.schemaVersion -ne 1) { $failures.Add('Observability registry schemaVersion must be 1.') }
$activeSources = @($registry.sources | Where-Object status -eq 'active')
if ($activeSources.Count -eq 0) { $failures.Add('At least one active agent-readable observability source is required.') }

foreach ($source in $activeSources) {
    foreach ($field in @('id', 'environment', 'component', 'owner', 'collectorInterface', 'queryInterface', 'evidenceRoot', 'correlationField', 'redaction', 'access')) {
        if ([string]::IsNullOrWhiteSpace([string]$source.$field)) { $failures.Add("Source '$($source.id)' is missing $field.") }
    }
    foreach ($signal in @('logs', 'metrics', 'traces')) {
        if (@($source.signals) -notcontains $signal) { $failures.Add("Source '$($source.id)' does not expose $signal.") }
    }
    if ([int]$source.retentionDays -lt 1) { $failures.Add("Source '$($source.id)' must define positive retentionDays.") }
    if ([int]$source.maxQueryWindowMinutes -lt 1 -or [int]$source.maxQueryWindowMinutes -gt 1440) { $failures.Add("Source '$($source.id)' must define a bounded maxQueryWindowMinutes between 1 and 1440.") }
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ([string]$source.collectorInterface)) -PathType Leaf)) {
        $failures.Add("Source '$($source.id)' collector interface is missing: $($source.collectorInterface)")
    }
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ([string]$source.queryInterface)) -PathType Leaf)) {
        $failures.Add("Source '$($source.id)' query interface is missing: $($source.queryInterface)")
    }
    if ($RequireEvidence) {
        $root = Join-Path $repoRoot ([string]$source.evidenceRoot)
        $run = Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
        if ($null -eq $run) { $failures.Add("Source '$($source.id)' has no captured run evidence.") }
        else {
            foreach ($file in @('logs.jsonl', 'metrics.json', 'traces.json', 'summary.json')) {
                if (-not (Test-Path -LiteralPath (Join-Path $run.FullName $file) -PathType Leaf)) { $failures.Add("Latest source run is missing $file.") }
            }
            try {
                $summary = Get-Content -LiteralPath (Join-Path $run.FullName 'summary.json') -Raw | ConvertFrom-Json
                if ([string]::IsNullOrWhiteSpace([string]$summary.runId) -or [string]::IsNullOrWhiteSpace([string]$summary.traceId)) { $failures.Add('Latest source summary is missing runId or traceId.') }
                $metrics = Get-Content -LiteralPath (Join-Path $run.FullName 'metrics.json') -Raw | ConvertFrom-Json
                if (@($metrics.metrics).Count -lt 2) { $failures.Add('Latest source evidence does not contain required duration and success metrics.') }
                $traces = Get-Content -LiteralPath (Join-Path $run.FullName 'traces.json') -Raw | ConvertFrom-Json
                if (@($traces.spans).Count -lt 1) { $failures.Add('Latest source evidence contains no trace span.') }
            } catch { $failures.Add("Latest source evidence is invalid JSON: $($_.Exception.Message)") }
        }
    }
}

if (-not (Test-Path -LiteralPath $feedbackPath -PathType Leaf)) { $failures.Add('Missing docs/harness-runtime-feedback.md.') }
else {
    $feedback = Get-Content -LiteralPath $feedbackPath -Raw
    $closedRows = [regex]::Matches($feedback, '(?im)^\|\s*`HRF-[^`]+`\s*\|.*\|\s*Closed\s*\|\s*$')
    if ($closedRows.Count -lt 2) { $failures.Add('Runtime feedback log must contain at least two closed, verified learning entries.') }
}

if ($failures.Count -gt 0) {
    Write-Host 'agent observability check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "agent observability check passed ($($activeSources.Count) active source; runtime feedback entries validated)" -ForegroundColor Green
exit 0
