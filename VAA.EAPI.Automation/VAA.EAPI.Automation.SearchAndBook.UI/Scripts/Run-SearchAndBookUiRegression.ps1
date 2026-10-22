param(
    [Parameter(Mandatory = $false)]
    [string]$EnvironmentName = "stg",

    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://stg.virginatlantic.com",

    [Parameter(Mandatory = $false)]
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Local", "BrowserStack")]
    [string]$RunMode = "Local",

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackOperatingSystem = "Windows",

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackOperatingSystemVersion = "11",

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackBrowser = "chrome",

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackBrowserVersion = "latest",

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackDeviceName = "",

    [Parameter(Mandatory = $false)]
    [switch]$BrowserStackLocal,

    [Parameter(Mandatory = $false)]
    [string]$BrowserStackLocalIdentifier = "",

    [Parameter(Mandatory = $false)]
    [bool]$Headless = $true,

    [Parameter(Mandatory = $false)]
    [switch]$InstallBrowsers,

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$ResultsRoot = "TestResults",

    [Parameter(Mandatory = $false)]
    [string]$Filter = "Category=SearchAndBook-UI-Regression"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptRoot "..")
$automationRoot = Resolve-Path (Join-Path $projectRoot "..")
$repositoryRoot = Resolve-Path (Join-Path $automationRoot "..")
$projectPath = Join-Path $projectRoot "VAA.EAPI.Automation.SearchAndBook.UI.csproj"
$runName = "search-and-book-ui-$EnvironmentName-regression"
$resultsDirectory = Join-Path $automationRoot (Join-Path $ResultsRoot $runName)
$trxPath = Join-Path $resultsDirectory "$runName.trx"
$artifactDirectory = Join-Path $automationRoot (Join-Path $ResultsRoot "playwright-$EnvironmentName-regression")
$analyserPath = Join-Path $repositoryRoot "scripts\analyse-test-run-results.ps1"

Write-Host "Building Search & Book UI Playwright project..."
dotnet build $projectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($InstallBrowsers) {
    $playwrightScript = Join-Path $projectRoot "bin\$Configuration\net10.0\playwright.ps1"
    $powerShellCommand = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($null -eq $powerShellCommand) {
        $powerShellCommand = Get-Command powershell -ErrorAction Stop
    }
    Write-Host "Installing Playwright browser runtime: $Browser..."
    & $powerShellCommand.Source -ExecutionPolicy Bypass -File $playwrightScript install $Browser
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "Running Search & Book UI Playwright regression against $EnvironmentName ($BaseUrl)..."
$env:SAB_BASE_URL = $BaseUrl
$env:SAB_ARTIFACT_DIR = $artifactDirectory
$env:SAB_BROWSER = $Browser
$env:SAB_HEADLESS = $Headless.ToString().ToLowerInvariant()
$env:SAB_RUN_MODE = $RunMode

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

dotnet test $projectPath `
    -c $Configuration `
    --no-build `
    --filter $Filter `
    --results-directory $resultsDirectory `
    --logger "trx;LogFileName=$runName.trx"
$testExitCode = $LASTEXITCODE

Write-Host "Analysing TRX output..."
if (Test-Path -LiteralPath $trxPath) {
    & $analyserPath -TrxPath $trxPath -SuiteName "Search & Book UI Playwright regression" -EnvironmentName $EnvironmentName
    $analysisExitCode = $LASTEXITCODE
}
else {
    Write-Error "TRX file was not created: $trxPath"
    $analysisExitCode = 1
}

if ($testExitCode -ne 0) {
    exit $testExitCode
}

if ($analysisExitCode -ne 0) {
    exit $analysisExitCode
}
