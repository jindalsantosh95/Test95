#requires -Version 7.0
[CmdletBinding()]
param([string]$BaseUrl)
if ($BaseUrl) { $env:IN_FLIGHT_APPLICATIONS_UI_BASE_URL = $BaseUrl }
$env:SAB_TRACE = 'false'
$env:IN_FLIGHT_APPLICATIONS_RUN_OTP_TESTS = 'true'
$project = Join-Path $PSScriptRoot '..\VAA.EAPI.Automation.InFlightApplications.UI.csproj'
$runId = Get-Date -Format 'yyyyMMdd-HHmmssfff'
$resultsDirectory = Join-Path $PSScriptRoot "..\TestResults\otp-$runId"
dotnet test $project -c Release --filter 'Category=InFlightApplications-UI-OtpJourney' --results-directory $resultsDirectory --logger 'trx;LogFileName=InFlightApplications-ui-otp.trx'
