#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Objective,
    [Parameter(Mandatory)][string]$ScopePath,
    [Parameter(Mandatory)][string]$ValidationCommandJson,
    [Parameter(Mandatory)][string]$CorrectionCommandJson,
    [Parameter(Mandatory)][string]$EvidencePath,
    [ValidateRange(1, 5)][int]$MaxValidationAttempts = 2,
    [ValidateRange(1, 60)][int]$TimeoutMinutes = 15
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$resolvedScope = (Resolve-Path -LiteralPath (Join-Path $repoRoot $ScopePath)).Path
if (-not $resolvedScope.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'ScopePath must stay inside the repository.' }
$resolvedEvidence = if ([System.IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repoRoot $EvidencePath }
if (-not $resolvedEvidence.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'EvidencePath must stay inside the repository.' }

function Resolve-Command([string]$Json, [string]$Name) {
    try { $value = $Json | ConvertFrom-Json }
    catch { throw "$Name command JSON is invalid: $($_.Exception.Message)" }
    if ($value -is [array]) {
        if ($value.Count -lt 1) { throw "$Name command array is empty." }
        return [pscustomobject]@{ executable = [string]$value[0]; arguments = @($value | Select-Object -Skip 1 | ForEach-Object { [string]$_ }) }
    }
    if ([string]::IsNullOrWhiteSpace([string]$value.executable)) { throw "$Name command requires executable." }
    return [pscustomobject]@{ executable = [string]$value.executable; arguments = @($value.args | ForEach-Object { [string]$_ }) }
}

function Invoke-BoundedCommand([object]$Command, [string]$Name, [int]$Attempt) {
    $started = [datetime]::UtcNow
    $output = @(& $Command.executable @($Command.arguments) 2>&1 | ForEach-Object { "$_" })
    $exitCode = $LASTEXITCODE
    if ($null -eq $exitCode) { $exitCode = 0 }
    return [ordered]@{
        phase = $Name; attempt = $Attempt; startedAtUtc = $started.ToString('o'); completedAtUtc = [datetime]::UtcNow.ToString('o')
        exitCode = $exitCode; output = @($output | Select-Object -Last 100)
    }
}

$validationCommand = Resolve-Command $ValidationCommandJson 'Validation'
$correctionCommand = Resolve-Command $CorrectionCommandJson 'Correction'
$runId = [guid]::NewGuid().ToString('N')
$startedAt = [datetime]::UtcNow
$phases = @()
$terminalState = 'Failed'
$failureReason = $null
$corrections = 0

for ($attempt = 1; $attempt -le $MaxValidationAttempts; $attempt++) {
    if (([datetime]::UtcNow - $startedAt).TotalMinutes -ge $TimeoutMinutes) { $failureReason = 'timeout budget exhausted'; break }
    $validation = Invoke-BoundedCommand $validationCommand 'validation' $attempt
    $phases += $validation
    if ($validation.exitCode -eq 0) { $terminalState = 'Succeeded'; break }
    if ($attempt -eq $MaxValidationAttempts) { $failureReason = 'validation attempt budget exhausted'; break }

    $correction = Invoke-BoundedCommand $correctionCommand 'correction' $attempt
    $phases += $correction
    $corrections++
    if ($correction.exitCode -ne 0) { $failureReason = 'correction command failed'; break }
}

$evidence = [ordered]@{
    schemaVersion = 1; runId = $runId; objective = $Objective; scope = $ScopePath; state = $terminalState
    maxValidationAttempts = $MaxValidationAttempts; timeoutMinutes = $TimeoutMinutes; correctionCount = $corrections
    failureReason = $failureReason; phases = $phases; startedAtUtc = $startedAt.ToString('o'); completedAtUtc = [datetime]::UtcNow.ToString('o')
}
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedEvidence) -Force | Out-Null
$evidence | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $resolvedEvidence -Encoding utf8

if ($terminalState -ne 'Succeeded') {
    Write-Host "self-correcting loop failed closed: $failureReason; evidence: $EvidencePath" -ForegroundColor Red
    exit 1
}
Write-Host "self-correcting loop passed after $corrections correction(s); evidence: $EvidencePath" -ForegroundColor Green
exit 0
