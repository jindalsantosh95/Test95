param(
    [string]$EnvironmentName = "sit",
    [string]$BaseUrl = "https://vaa-test1.virginatlantic.com/my-trips-dev",
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",
    [ValidateSet("Local", "BrowserStack")]
    [string]$RunMode = "Local",
    [string]$BrowserStackOperatingSystem = "Windows",
    [string]$BrowserStackOperatingSystemVersion = "11",
    [string]$BrowserStackBrowser = "chrome",
    [string]$BrowserStackBrowserVersion = "latest",
    [string]$BrowserStackDeviceName = "",
    [switch]$BrowserStackLocal,
    [string]$BrowserStackLocalIdentifier = "",
    [bool]$Headless = $true,
    [switch]$InstallBrowsers,
    [string]$Configuration = "Release",
    [string]$ResultsRoot = "TestResults",
    [string]$Filter = "Category=Trips-UI-Regression"
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptRoot "..")
$automationRoot = Resolve-Path (Join-Path $projectRoot "..")
$repositoryRoot = Resolve-Path (Join-Path $automationRoot "..")
$projectPath = Join-Path $projectRoot "VAA.EAPI.Automation.Trips.UI.csproj"
$runName = "trips-ui-$EnvironmentName-regression"
$resultsDirectory = Join-Path $automationRoot (Join-Path $ResultsRoot $runName)
$trxPath = Join-Path $resultsDirectory "$runName.trx"
$artifactDirectory = Join-Path $automationRoot (Join-Path $ResultsRoot "playwright-$EnvironmentName-trips-ui")
$analyserPath = Join-Path $repositoryRoot "scripts\analyse-test-run-results.ps1"

dotnet build $projectPath -c $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($InstallBrowsers) {
    $playwrightScript = Join-Path $projectRoot "bin\$Configuration\net10.0\playwright.ps1"
    & $playwrightScript install $Browser
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$env:TRIPS_UI_BASE_URL = $BaseUrl
$env:TRIPS_UI_RUN_MODE = $RunMode
$env:TRIPS_UI_BROWSER = $Browser
$env:TRIPS_UI_HEADLESS = $Headless.ToString().ToLowerInvariant()
$env:TRIPS_UI_ARTIFACT_DIR = $artifactDirectory
$env:SAB_BASE_URL = $BaseUrl
$env:SAB_RUN_MODE = $RunMode
$env:SAB_BROWSER = $Browser
$env:SAB_HEADLESS = $Headless.ToString().ToLowerInvariant()
$env:SAB_ARTIFACT_DIR = $artifactDirectory
$env:BROWSERSTACK_PROJECT = "Virgin Atlantic Web - Trips UI"
$env:BROWSERSTACK_BUILD = "Trips UI - $EnvironmentName"
$env:SAB_BS_SESSION_NAME = "Trips UI - Retrieve Trip"

if ($RunMode -eq "BrowserStack") {
    if ([string]::IsNullOrWhiteSpace($env:BROWSERSTACK_USERNAME) -or [string]::IsNullOrWhiteSpace($env:BROWSERSTACK_ACCESS_KEY)) {
        throw "BROWSERSTACK_USERNAME and BROWSERSTACK_ACCESS_KEY must be set for BrowserStack execution."
    }

    $env:SAB_BS_OS = $BrowserStackOperatingSystem
    $env:SAB_BS_OS_VERSION = $BrowserStackOperatingSystemVersion
    $env:SAB_BS_BROWSER = $BrowserStackBrowser
    $env:SAB_BS_BROWSER_VERSION = $BrowserStackBrowserVersion
    $env:SAB_BS_DEVICE_NAME = $BrowserStackDeviceName
    $env:BROWSERSTACK_LOCAL = $BrowserStackLocal.IsPresent.ToString().ToLowerInvariant()
    $env:BROWSERSTACK_LOCAL_IDENTIFIER = $BrowserStackLocalIdentifier
}

dotnet test $projectPath -c $Configuration --no-build --filter $Filter `
    --results-directory $resultsDirectory --logger "trx;LogFileName=$runName.trx"
$testExitCode = $LASTEXITCODE

if (Test-Path -LiteralPath $trxPath) {
    & $analyserPath -TrxPath $trxPath -SuiteName "Trips UI Playwright regression" -EnvironmentName $EnvironmentName
    $analysisExitCode = $LASTEXITCODE
} else {
    Write-Error "TRX file was not created: $trxPath"
    $analysisExitCode = 1
}

if ($testExitCode -ne 0) { exit $testExitCode }
if ($analysisExitCode -ne 0) { exit $analysisExitCode }
