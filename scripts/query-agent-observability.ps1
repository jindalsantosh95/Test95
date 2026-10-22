#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$SourceId = 'local-docs-harness',
    [string]$RunId,
    [datetime]$StartUtc = [datetime]::MinValue,
    [datetime]$EndUtc = [datetime]::MinValue,
    [ValidateSet('all', 'logs', 'metrics', 'traces', 'summary')][string]$Signal = 'all',
    [switch]$RequireMatch,
    [string]$EvidenceRootOverride,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$jsonParams = @{}
if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
$registry = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/agent-observability-sources.json') -Raw | ConvertFrom-Json @jsonParams
$source = @($registry.sources | Where-Object id -eq $SourceId) | Select-Object -First 1
$failures = [System.Collections.Generic.List[string]]::new()
if ($null -eq $source -or [string]$source.status -ne 'active') { $failures.Add("Observability source '$SourceId' is not active.") }
if ([string]::IsNullOrWhiteSpace($RunId)) { $failures.Add('A non-sensitive RunId correlation value is required.') }
if ($StartUtc -eq [datetime]::MinValue -or $EndUtc -eq [datetime]::MinValue) { $failures.Add('StartUtc and EndUtc are required.') }
elseif ($StartUtc.Kind -ne [DateTimeKind]::Utc -or $EndUtc.Kind -ne [DateTimeKind]::Utc) { $failures.Add('StartUtc and EndUtc must be UTC values.') }
elseif ($EndUtc -le $StartUtc) { $failures.Add('EndUtc must be after StartUtc.') }
elseif (($EndUtc - $StartUtc).TotalMinutes -gt [int]$source.maxQueryWindowMinutes) { $failures.Add("Query window exceeds $($source.maxQueryWindowMinutes) minutes.") }

$relativeRoot = if ([string]::IsNullOrWhiteSpace($EvidenceRootOverride)) { [string]$source.evidenceRoot } else { $EvidenceRootOverride }
$normalizedRoot = $relativeRoot.Replace('\', '/').Trim('/')
if ([System.IO.Path]::IsPathRooted($relativeRoot) -or $normalizedRoot.Split('/') -contains '..' -or -not $normalizedRoot.StartsWith('TestResults/')) {
    $failures.Add('Evidence root must be repository-relative beneath TestResults/.')
}
$resolvedOutput = $null
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $resolvedOutput = if ([System.IO.Path]::IsPathRooted($OutputPath)) { [System.IO.Path]::GetFullPath($OutputPath) } else { [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputPath)) }
    $allowedOutputRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'TestResults')) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedOutput.StartsWith($allowedOutputRoot, [System.StringComparison]::OrdinalIgnoreCase)) { $failures.Add('OutputPath must be beneath the repository TestResults directory.') }
}

$items = @()
if ($failures.Count -eq 0) {
    $root = Join-Path $repoRoot $normalizedRoot
    foreach ($runDirectory in @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue)) {
        $summaryPath = Join-Path $runDirectory.FullName 'summary.json'
        if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) { continue }
        try { $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json @jsonParams } catch { continue }
        $completed = [DateTimeOffset]::MinValue
        if ([string]$summary.runId -ne $RunId -or -not [DateTimeOffset]::TryParse([string]$summary.completedAtUtc, [ref]$completed)) { continue }
        if ($completed.UtcDateTime -lt $StartUtc -or $completed.UtcDateTime -gt $EndUtc) { continue }
        $requestedSignals = if ($Signal -eq 'all') { @('logs', 'metrics', 'traces', 'summary') } else { @($Signal) }
        foreach ($requested in $requestedSignals) {
            $fileName = switch ($requested) { 'logs' { 'logs.jsonl' } 'metrics' { 'metrics.json' } 'traces' { 'traces.json' } 'summary' { 'summary.json' } }
            $path = Join-Path $runDirectory.FullName $fileName
            if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $failures.Add("Run '$RunId' is missing $fileName."); continue }
            $content = if ($requested -eq 'logs') { @(Get-Content -LiteralPath $path | Where-Object { $_ } | ForEach-Object { $_ | ConvertFrom-Json @jsonParams }) } else { Get-Content -LiteralPath $path -Raw | ConvertFrom-Json @jsonParams }
            $repositoryPath = ([System.IO.Path]::GetFullPath($path).Substring($repoRoot.Length + 1)).Replace('\', '/')
            $items += [ordered]@{ signal = $requested; runId = $RunId; repositoryPath = $repositoryPath; content = $content }
        }
    }
}
if ($RequireMatch -and $items.Count -eq 0 -and $failures.Count -eq 0) { $failures.Add('No evidence matched the bounded correlation query.') }
$result = [ordered]@{ schemaVersion = 1; sourceId = $SourceId; runId = $RunId; startUtc = $(if ($StartUtc -ne [datetime]::MinValue) { $StartUtc.ToString('o') } else { $null }); endUtc = $(if ($EndUtc -ne [datetime]::MinValue) { $EndUtc.ToString('o') } else { $null }); signal = $Signal; matched = $items.Count; passed = $failures.Count -eq 0; items = $items; failures = @($failures) }
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $resolvedOutput
    if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    $result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $resolvedOutput -Encoding utf8
} else { $result | ConvertTo-Json -Depth 12 }
if ($failures.Count -gt 0) { Write-Host "agent observability query failed: $($failures -join ' ')" -ForegroundColor Red; exit 1 }
exit 0
