#Requires -Version 5.1
[CmdletBinding()]
param([Parameter(Mandatory)][string]$EvidencePath)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $EvidencePath -PathType Leaf)) { throw "Memory lifecycle evidence not found: $EvidencePath" }
$evidence = Get-Content -LiteralPath $EvidencePath -Raw | ConvertFrom-Json
$checks = @($evidence.checks.psobject.Properties)
if ([string]$evidence.capability -ne 'memory-resume' -or $evidence.passed -ne $true -or $checks.Count -lt 8 -or @($checks | Where-Object { $_.Value -ne $true }).Count -gt 0) {
    Write-Host 'agent memory lifecycle evidence check failed' -ForegroundColor Red
    exit 1
}
Write-Host "agent memory lifecycle evidence passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
exit 0
