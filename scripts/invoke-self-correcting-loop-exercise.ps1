#Requires -Version 5.1
[CmdletBinding()]
param([string]$RunId = "self-correction-$([guid]::NewGuid().ToString('N'))")

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$rootRelative = "TestResults/self-correcting-loop/$RunId"
$root = Join-Path $repoRoot $rootRelative
$marker = Join-Path $root 'corrected.marker'
$evidenceRelative = "$rootRelative/success-evidence.json"
$evidence = Join-Path $repoRoot $evidenceRelative
New-Item -ItemType Directory -Path $root -Force | Out-Null

$escapedMarker = $marker.Replace("'", "''")
$validation = @{ executable = 'pwsh'; args = @('-NoProfile','-Command',"if (Test-Path -LiteralPath '$escapedMarker') { exit 0 } else { Write-Error 'marker missing'; exit 1 }") } | ConvertTo-Json -Compress
$correction = @{ executable = 'pwsh'; args = @('-NoProfile','-Command',"New-Item -ItemType File -Path '$escapedMarker' -Force | Out-Null") } | ConvertTo-Json -Compress

& (Join-Path $PSScriptRoot 'invoke-self-correcting-loop.ps1') -Objective 'Prove validation, correction, and revalidation' -ScopePath docs -ValidationCommandJson $validation -CorrectionCommandJson $correction -EvidencePath $evidenceRelative -MaxValidationAttempts 2 -TimeoutMinutes 5
if ($LASTEXITCODE -ne 0) { throw 'Expected successful self-correction exercise to pass.' }
$result = Get-Content -LiteralPath $evidence -Raw | ConvertFrom-Json
if ($result.state -ne 'Succeeded' -or $result.correctionCount -ne 1) { throw 'Successful exercise evidence has the wrong terminal state or correction count.' }
$phases = @($result.phases | ForEach-Object phase)
if (($phases -join ',') -ne 'validation,correction,validation') { throw "Unexpected correction phase sequence: $($phases -join ',')" }

# Prove an ineffective correction exhausts its bounded validation budget and fails closed.
$failureEvidenceRelative = "$rootRelative/failure-evidence.json"
$neverValid = @{ executable = 'pwsh'; args = @('-NoProfile','-Command',"Write-Error 'still invalid'; exit 1") } | ConvertTo-Json -Compress
$noOpCorrection = @{ executable = 'pwsh'; args = @('-NoProfile','-Command','exit 0') } | ConvertTo-Json -Compress
$failedClosed = $false
& (Join-Path $PSScriptRoot 'invoke-self-correcting-loop.ps1') -Objective 'Prove bounded failure' -ScopePath docs -ValidationCommandJson $neverValid -CorrectionCommandJson $noOpCorrection -EvidencePath $failureEvidenceRelative -MaxValidationAttempts 2 -TimeoutMinutes 5
if ($LASTEXITCODE -ne 0) { $failedClosed = $true }
if (-not $failedClosed) { throw 'Ineffective correction did not fail closed.' }
$failureResult = Get-Content -LiteralPath (Join-Path $repoRoot $failureEvidenceRelative) -Raw | ConvertFrom-Json
if ($failureResult.state -ne 'Failed' -or $failureResult.failureReason -ne 'validation attempt budget exhausted') { throw 'Failure exercise did not preserve the expected terminal evidence.' }

Write-Host "self-correcting loop exercise passed: $rootRelative" -ForegroundColor Green
exit 0
