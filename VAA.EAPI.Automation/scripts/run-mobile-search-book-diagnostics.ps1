[CmdletBinding()]
param(
    [string]$Filter = "Category=MobileApp-SearchBook-SmokeE2E",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "../VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj"
$localSettingsPath = Join-Path $PSScriptRoot "../VAA.EAPI.Automation.MobileApp.UI/local.settings.local.json"
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$resultsDirectory = Join-Path $PSScriptRoot "../TestResults/mobile-diagnostics-$runId"
$diagnosticTrx = "mobile-diagnostic.trx"

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

if (-not (Test-Path -LiteralPath $localSettingsPath)) {
    throw "Create VAA.EAPI.Automation.MobileApp.UI/local.settings.local.json before running BrowserStack diagnostics."
}

$localSettings = Get-Content -LiteralPath $localSettingsPath -Raw | ConvertFrom-Json
if ($localSettings.RunMode -ne "Remote" -or
    $localSettings.BrowserStack.NetworkLogs -ne $true -or
    $localSettings.BrowserStack.CaptureNetworkContent -ne $true) {
    throw "Set RunMode to Remote and enable BrowserStack.NetworkLogs and BrowserStack.CaptureNetworkContent in local.settings.local.json before running diagnostics."
}

Write-Host "Running once with BrowserStack network capture available for failure reporting."
dotnet test $project -c $Configuration --filter $Filter --results-directory $resultsDirectory --logger "trx;LogFileName=$diagnosticTrx"
$diagnosticExitCode = $LASTEXITCODE

Write-Host "Diagnostic TRX: $(Join-Path $resultsDirectory $diagnosticTrx)"
Write-Host "Network content is sanitised and written only when a test fails; payment is never retried."
exit $diagnosticExitCode
