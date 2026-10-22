param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [switch]$SkipBrowserInstall
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptRoot "..")
$automationRoot = Resolve-Path (Join-Path $projectRoot "..")
$repositoryRoot = Resolve-Path (Join-Path $automationRoot "..")
$projectPath = Join-Path $projectRoot "VAA.EAPI.Automation.SearchAndBook.UI.csproj"
$runName = "search-and-book-ui-setup"
$resultsDirectory = Join-Path $projectRoot (Join-Path "TestResults" $runName)
$trxPath = Join-Path $resultsDirectory "$runName.trx"
$artifactDirectory = Join-Path $projectRoot (Join-Path "TestResults" "playwright-setup")
$analyserPath = Join-Path $repositoryRoot (Join-Path "scripts" "analyse-test-run-results.ps1")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK 10 is required. Install it from https://dotnet.microsoft.com/download/dotnet/10.0 and run setup again."
}

$sdkVersion = & dotnet --version
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$sdkMajorVersion = 0
if (-not [int]::TryParse(($sdkVersion -split '\.')[0], [ref]$sdkMajorVersion) -or $sdkMajorVersion -lt 10) {
    throw ".NET SDK 10 is required; found $sdkVersion."
}

Write-Host "Using .NET SDK $sdkVersion."
Write-Host "Restoring Search & Book UI dependencies..."
& dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Building Search & Book UI Playwright project..."
& dotnet build $projectPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (-not $SkipBrowserInstall) {
    $playwrightScript = Join-Path $projectRoot (Join-Path "bin" (Join-Path $Configuration (Join-Path "net10.0" "playwright.ps1")))
    if (-not (Test-Path -LiteralPath $playwrightScript)) {
        throw "Playwright installer was not generated at $playwrightScript."
    }

    Write-Host "Installing Playwright browser runtime: $Browser..."
    & $playwrightScript install $Browser
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "Running offline Playwright setup verification..."
$env:SAB_ARTIFACT_DIR = $artifactDirectory
$env:SAB_BROWSER = $Browser
$env:SAB_HEADLESS = "true"
$env:SAB_TRACE = "true"

& dotnet test $projectPath `
    -c $Configuration `
    --no-build `
    --filter "Category=SearchAndBook-UI-Setup" `
    --results-directory $resultsDirectory `
    --logger "trx;LogFileName=$runName.trx"
$testExitCode = $LASTEXITCODE

Write-Host "Analysing setup verification TRX output..."
if (Test-Path -LiteralPath $trxPath) {
    & $analyserPath -TrxPath $trxPath -SuiteName "Search & Book UI setup verification" -EnvironmentName "local"
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

Write-Host "Search & Book UI setup verified successfully."
Write-Host "Screenshot: $(Join-Path $artifactDirectory 'setup-verification.png')"
Write-Host "Trace: $(Join-Path $artifactDirectory 'setup-trace.zip')"
Write-Host "TRX: $trxPath"
