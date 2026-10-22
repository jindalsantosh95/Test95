#Requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$RequireMature,
    [string]$MaturityLedgerPath,
    [string]$MaturitySummaryPath,
    [int]$MaxReviewAgeDays = 30,
    [datetime]$AsOfUtc = [datetime]::UtcNow
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$registryPath = Join-Path $repoRoot 'docs/harness-operational-capabilities.json'
$failures = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -LiteralPath $registryPath -PathType Leaf)) {
    Write-Error 'Missing docs/harness-operational-capabilities.json.'
    exit 1
}

try {
    $registryJson = Get-Content -LiteralPath $registryPath -Raw
    $registry = $registryJson | ConvertFrom-Json
}
catch {
    Write-Error "Operational harness registry is invalid JSON: $($_.Exception.Message)"
    exit 1
}

if ($registry.schemaVersion -ne 1) { $failures.Add('schemaVersion must be 1.') }
if ([string]::IsNullOrWhiteSpace([string]$registry.owner)) { $failures.Add('Registry owner is required.') }

$reviewedAt = [datetime]::MinValue
try {
    $reviewedAt = ([datetime]$registry.reviewedAtUtc).ToUniversalTime()
}
catch {
    $failures.Add('reviewedAtUtc must be a valid UTC timestamp.')
}
if ($registryJson -notmatch '"reviewedAtUtc"\s*:\s*"[^"]+Z"') {
    $failures.Add('reviewedAtUtc must use UTC (Z).')
}
elseif (($AsOfUtc.ToUniversalTime() - $reviewedAt.ToUniversalTime()).TotalDays -gt $MaxReviewAgeDays) {
    $failures.Add("Registry review is older than $MaxReviewAgeDays days.")
}

$requiredIds = @(
    'memory-resume', 'architecture-fitness', 'behavior-strength',
    'high-risk-inferential-review', 'agent-observability',
    'semantic-doc-gardening', 'execution-recovery',
    'resource-cost-control', 'runtime-learning', 'self-correcting-loop',
    'tool-executable-environment', 'continuous-maturity-evidence'
)
$capabilities = @($registry.capabilities)
$ids = @($capabilities | ForEach-Object { [string]$_.id })
foreach ($requiredId in $requiredIds) {
    if ($ids -notcontains $requiredId) { $failures.Add("Missing capability '$requiredId'.") }
}
if (($ids | Select-Object -Unique).Count -ne $ids.Count) { $failures.Add('Capability IDs must be unique.') }

foreach ($capability in $capabilities) {
    $label = [string]$capability.id
    foreach ($field in @('id', 'theme', 'status', 'policy', 'entryPoint', 'gap', 'owner')) {
        if ([string]::IsNullOrWhiteSpace([string]$capability.$field)) {
            $failures.Add("Capability '$label' is missing $field.")
        }
    }
    if (@('planned', 'active', 'mature') -notcontains [string]$capability.status) {
        $failures.Add("Capability '$label' has invalid status '$($capability.status)'.")
    }
    if (@('required', 'advisory') -notcontains [string]$capability.policy) {
        $failures.Add("Capability '$label' has invalid policy '$($capability.policy)'.")
    }
    $passingCycles = [int]$capability.passingCycles
    $targetCycles = [int]$capability.targetCycles
    if ($passingCycles -lt 0 -or $targetCycles -lt 2 -or $passingCycles -gt $targetCycles) {
        $failures.Add("Capability '$label' has invalid cycle counts.")
    }
    if ($capability.status -eq 'mature' -and $passingCycles -lt $targetCycles) {
        $failures.Add("Capability '$label' cannot be mature before targetCycles is met.")
    }
    foreach ($relativePath in @($capability.evidence) + @([string]$capability.entryPoint)) {
        if ([string]::IsNullOrWhiteSpace($relativePath)) { continue }
        $normalized = $relativePath.Replace('\', '/').TrimStart('/')
        if ([System.IO.Path]::IsPathRooted($relativePath) -or $normalized.Split('/') -contains '..') {
            $failures.Add("Capability '$label' references unsafe path '$relativePath'.")
            continue
        }
        if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $normalized))) {
            $failures.Add("Capability '$label' references missing path '$relativePath'.")
        }
    }
}

if ($RequireMature) {
    if ([string]::IsNullOrWhiteSpace($MaturityLedgerPath) -or [string]::IsNullOrWhiteSpace($MaturitySummaryPath)) {
        $failures.Add('-RequireMature requires retained -MaturityLedgerPath and -MaturitySummaryPath evidence.')
    } else {
        & (Join-Path $PSScriptRoot 'check-harness-maturity-evidence.ps1') -LedgerPath $MaturityLedgerPath -SummaryPath $MaturitySummaryPath -RequireMature
        if ($LASTEXITCODE -ne 0) { $failures.Add('Retained continuous maturity evidence has not met promotion policy.') }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'operational harness check failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

$mature = @($capabilities | Where-Object status -eq 'mature').Count
$active = @($capabilities | Where-Object status -eq 'active').Count
$planned = @($capabilities | Where-Object status -eq 'planned').Count
Write-Host "operational harness check passed (mature=$mature active=$active planned=$planned)" -ForegroundColor Green
exit 0
