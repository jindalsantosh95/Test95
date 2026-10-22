[CmdletBinding()]
param(
    [switch]$SkipConnectivityTest
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "mobile-local-settings-validation.ps1")
$solutionRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $solutionRoot
$projectPath = Join-Path $solutionRoot "VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj"
$projectDirectory = Split-Path -Parent $projectPath
$settingsPath = Join-Path $projectDirectory "local.settings.local.json"
$exampleSettingsPath = Join-Path $projectDirectory "local.settings.local.example.json"
$analyserPath = Join-Path $repositoryRoot "scripts/analyse-test-run-results.ps1"
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$resultsRoot = Join-Path $solutionRoot "TestResults/mobile-setup-$runId"

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory)]
        [string]$Description,
        [Parameter(Mandatory)]
        [scriptblock]$Command
    )

    Write-Host "`n==> $Description"
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

function Get-LocalSettings {
    if (-not (Test-Path $settingsPath)) {
        return $null
    }

    try {
        $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
        return $settings
    }
    catch {
        throw "Could not parse $settingsPath. Start again from $exampleSettingsPath. $($_.Exception.Message)"
    }
}

Push-Location $solutionRoot
try {
    Write-Host "Mobile App UI setup and connectivity check"
    Write-Host "This workflow never runs booking or payment journey tests."

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw ".NET SDK was not found. Install the .NET 10 SDK and rerun this command."
    }

    $installedSdks = & dotnet --list-sdks
    if (-not ($installedSdks | Where-Object { $_ -match '^10\.' })) {
        throw ".NET 10 SDK was not found. Installed SDKs: $($installedSdks -join ', ')"
    }
    Write-Host "[OK] .NET 10 SDK is installed."
    & (Join-Path $PSScriptRoot "test-mobile-local-settings-validation.ps1")

    if (-not (Test-Path $settingsPath)) {
        Copy-Item $exampleSettingsPath $settingsPath
        Write-Host "[ACTION REQUIRED] Created $settingsPath from the safe example."
        Write-Host "Choose RunMode. For Remote, set BrowserStack credentials/app; for Local, set Platform and its Appium block."
    }
    else {
        Write-Host "[OK] Local settings file exists and is gitignored."
    }

    Invoke-CheckedCommand "Restore Mobile App UI dependencies" {
        dotnet restore $projectPath
    }
    Invoke-CheckedCommand "Build Mobile App UI" {
        dotnet build $projectPath -c Release --no-restore
    }

    $offlineResults = Join-Path $resultsRoot "offline"
    Invoke-CheckedCommand "Run safe offline mobile harness tests" {
        dotnet test $projectPath -c Release --no-build `
            --filter "Category=MobileApp-Offline|FullyQualifiedName~BrowserStackSessionRetryPolicyTests" `
            --results-directory $offlineResults `
            --logger "trx;LogFileName=mobile-setup-offline.trx"
    }
    & $analyserPath `
        -TrxPath (Join-Path $offlineResults "mobile-setup-offline.trx") `
        -SuiteName "Mobile App UI setup (offline)" `
        -EnvironmentName "local"

    if ($SkipConnectivityTest) {
        Write-Host "`n[OK] Offline setup completed. Connectivity test was skipped by request."
        exit 0
    }

    $settings = Get-LocalSettings
    $missingSettings = [System.Collections.Generic.List[string]]::new()
    $runMode = $settings.RunMode
    if ($runMode -eq "Remote") {
        $executionEnvironment = "BrowserStack"
        if ([string]::IsNullOrWhiteSpace($settings.BrowserStack.Username) -or
            $settings.BrowserStack.Username -like "*replace-with*") {
            $missingSettings.Add("BrowserStack.Username in local.settings.local.json")
        }
        if ([string]::IsNullOrWhiteSpace($settings.BrowserStack.AccessKey) -or
            $settings.BrowserStack.AccessKey -like "*replace-with*") {
            $missingSettings.Add("BrowserStack.AccessKey in local.settings.local.json")
        }
        if ([string]::IsNullOrWhiteSpace($settings.BrowserStack.App) -or
            $settings.BrowserStack.App -like "*replace-with*") {
            $missingSettings.Add("BrowserStack.App in local.settings.local.json")
        }
    }
    elseif ($runMode -eq "Local") {
        $executionEnvironment = "local Appium"
        foreach ($missing in @(Get-MobileLocalMissingSettings -Settings $settings)) {
            $missingSettings.Add($missing)
        }
    }
    else {
        $executionEnvironment = "unknown"
        $missingSettings.Add("RunMode must be Remote or Local in local.settings.local.json")
    }

    if ($missingSettings.Count -gt 0) {
        Write-Host "`n[SETUP INCOMPLETE] Offline checks passed, but connectivity needs:"
        foreach ($setting in $missingSettings) {
            Write-Host " - $setting"
        }
        Write-Host "No Appium session was started and no journey tests were run."
        exit 2
    }

    $connectivityResults = Join-Path $resultsRoot "connectivity"
    Invoke-CheckedCommand "Launch the app and verify Appium connectivity" {
        dotnet test $projectPath -c Release --no-build `
            --filter "Category=MobileApp-Connectivity" `
            --results-directory $connectivityResults `
            --logger "trx;LogFileName=mobile-connectivity.trx"
    }
    & $analyserPath `
        -TrxPath (Join-Path $connectivityResults "mobile-connectivity.trx") `
        -SuiteName "Mobile App UI connectivity" `
        -EnvironmentName $executionEnvironment

    Write-Host "`n[SUCCESS] Mobile App UI is configured and $executionEnvironment connectivity passed."
    Write-Host "Results: $resultsRoot"
}
finally {
    Pop-Location
}
