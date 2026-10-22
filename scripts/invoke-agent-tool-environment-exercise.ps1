#Requires -Version 5.1
[CmdletBinding()]
param([string]$EvidenceDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) { $EvidenceDirectory = Join-Path $repoRoot ('TestResults/agent-tool-environment/' + [guid]::NewGuid().ToString('N')) }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$checker = Join-Path $PSScriptRoot 'check-agent-tool-environment.ps1'
$registryPath = Join-Path $repoRoot 'docs/agent-tool-environments.json'

$realEvidence = Join-Path $EvidenceDirectory 'real-profile.json'
& $checker -Profile docs-local -Probe -EvidencePath $realEvidence
$realPassed = $LASTEXITCODE -eq 0

$missingToolRegistry = Join-Path $EvidenceDirectory 'missing-tool-registry.json'
$missingTool = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json
$missingTool.profiles.'docs-local'.requiredCommands += 'missing-harness-tool-exercise'
$missingTool | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $missingToolRegistry -Encoding utf8
& $checker -Profile docs-local -RegistryPath $missingToolRegistry -Probe
$missingToolRejected = $LASTEXITCODE -ne 0

$unsafeRegistry = Join-Path $EvidenceDirectory 'unsafe-registry.json'
$unsafe = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json
$unsafe.profiles.'docs-local'.writableRoots = @('../outside')
$unsafe | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $unsafeRegistry -Encoding utf8
& $checker -Profile docs-local -RegistryPath $unsafeRegistry
$unsafeRootRejected = $LASTEXITCODE -ne 0

$checks = [ordered]@{
    realProfilePassed = $realPassed -and (Get-Content -LiteralPath $realEvidence -Raw | ConvertFrom-Json).passed -eq $true
    missingExecutableRejected = $missingToolRejected
    unsafeWritableRootRejected = $unsafeRootRejected
    isolatedEvidenceRootUsed = (Resolve-Path -LiteralPath $EvidenceDirectory).Path.StartsWith((Join-Path $repoRoot 'TestResults'))
}
$passed = @($checks.GetEnumerator() | Where-Object { $_.Value -ne $true }).Count -eq 0
$summaryPath = Join-Path $EvidenceDirectory 'summary.json'
[ordered]@{ schemaVersion = 1; capability = 'tool-executable-environment'; passed = $passed; exercisedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); checks = $checks } |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding utf8
if (-not $passed) { Write-Host 'agent tool environment exercise failed' -ForegroundColor Red; exit 1 }
Write-Host "agent tool environment exercise passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
Write-Host "evidence: $summaryPath" -ForegroundColor DarkGray
exit 0
