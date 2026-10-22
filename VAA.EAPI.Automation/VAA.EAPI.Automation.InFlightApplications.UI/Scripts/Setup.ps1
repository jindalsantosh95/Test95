#requires -Version 7.0
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\VAA.EAPI.Automation.InFlightApplications.UI.csproj'
dotnet restore $project
dotnet build $project -c Release --no-restore
$playwright = Get-ChildItem (Join-Path $PSScriptRoot '..\bin\Release') -Filter 'playwright.ps1' -Recurse | Select-Object -First 1
if ($null -eq $playwright) { throw 'playwright.ps1 was not produced by the build.' }
pwsh $playwright.FullName install chromium
