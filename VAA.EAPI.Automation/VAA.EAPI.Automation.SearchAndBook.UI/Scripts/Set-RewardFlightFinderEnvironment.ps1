param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("test1", "test2", "test3", "test4", "test5", "stg")]
    [string]$EnvironmentName
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$profilesPath = Join-Path $scriptRoot "..\Resources\RewardFlightFinder\environment-profiles.json"
$profiles = (Get-Content -LiteralPath $profilesPath -Raw | ConvertFrom-Json).profiles
$environmentProfile = @($profiles | Where-Object { $_.name -eq $EnvironmentName })

if ($environmentProfile.Count -ne 1) {
    throw "Reward Flight Finder environment profile '$EnvironmentName' was not found in $profilesPath."
}

$env:SAB_BASE_URL = $environmentProfile[0].baseUrl
$env:RFF_EXPECTED_HOST = $environmentProfile[0].expectedHost

Write-Host "Configured Reward Flight Finder for $($environmentProfile[0].name) ($($environmentProfile[0].backend)): $($environmentProfile[0].baseUrl)"
Write-Host "RFF_ORIGIN, RFF_DESTINATION and no-availability route data remain configured through local settings or CI variables."
