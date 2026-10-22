#requires -Version 7.0
[CmdletBinding()]
param([string]$BaseUrl)
if ($BaseUrl) { $env:IN_FLIGHT_APPLICATIONS_UI_BASE_URL = $BaseUrl }
$project = Join-Path $PSScriptRoot '..\VAA.EAPI.Automation.InFlightApplications.UI.csproj'
$runId = Get-Date -Format 'yyyyMMdd-HHmmssfff'
$resultsDirectory = Join-Path $PSScriptRoot "..\TestResults\regression-$runId"
dotnet test $project -c Release --filter 'Category=InFlightApplications-UI-Smoke|Category=InFlightApplications-UI-Validation|Category=InFlightApplications-UI-Journey|Category=InFlightApplications-UI-Accessibility' --results-directory $resultsDirectory --logger 'trx;LogFileName=InFlightApplications-ui.trx'
