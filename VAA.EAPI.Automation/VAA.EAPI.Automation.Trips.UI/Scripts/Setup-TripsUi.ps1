param(
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",
    [string]$Configuration = "Release",
    [switch]$SkipBrowserInstall
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptRoot "..")
$repositoryRoot = Resolve-Path (Join-Path (Resolve-Path (Join-Path $projectRoot "..")) "..")
$projectPath = Join-Path $projectRoot "VAA.EAPI.Automation.Trips.UI.csproj"

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK 10 is required."
}

Write-Host "Restoring Trips UI dependencies..."
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Building Trips UI Playwright project..."
dotnet build $projectPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipBrowserInstall) {
    $playwrightScript = Join-Path $projectRoot "bin\$Configuration\net10.0\playwright.ps1"
    if (-not (Test-Path -LiteralPath $playwrightScript)) {
        throw "Playwright installer was not generated at $playwrightScript."
    }

    & $playwrightScript install $Browser
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Trips UI setup completed successfully."
