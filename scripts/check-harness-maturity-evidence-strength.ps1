#Requires -Version 5.1
[CmdletBinding()]
param([Parameter(Mandatory)][string]$EvidencePath)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $EvidencePath -PathType Leaf)) { throw "Maturity exercise evidence not found: $EvidencePath" }
$evidence = Get-Content -LiteralPath $EvidencePath -Raw | ConvertFrom-Json; $checks = @($evidence.checks.psobject.Properties)
if ([string]$evidence.capability -ne 'continuous-maturity-evidence' -or $evidence.passed -ne $true -or $checks.Count -lt 4 -or @($checks | Where-Object { $_.Value -ne $true }).Count -gt 0) { Write-Host 'harness maturity evidence strength failed' -ForegroundColor Red; exit 1 }
Write-Host "harness maturity evidence strength passed: $($checks.Count)/$($checks.Count) checks" -ForegroundColor Green
exit 0
