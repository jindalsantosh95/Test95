#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$automation = Join-Path $root 'VAA.EAPI.Automation'
$searchHelper = Join-Path $automation 'VAA.EAPI.Automation.Search/Helpers/OfferSearchHelper.cs'
$source = [IO.File]::ReadAllText($searchHelper)
if ($source -match '(?<!\r)\n') { throw 'C# checkout must preserve the CRLF formatting contract.' }

# Match the resource spelling used by the migrated regression test exactly.
$resource = 'Requests/Body/GraphQl/SalesBannerGqlQuery.txt'
$searchProject = Join-Path $automation 'VAA.EAPI.Automation.Search'
if (-not $source.Contains($resource) -or -not (Test-Path -LiteralPath (Join-Path $searchProject $resource))) {
    throw 'Search regression resource is missing or its source path is not portable.'
}

$ociProject = Join-Path $automation 'VAA.EAPI.Automation.MobileApp.OCI.UI'
[xml]$ociXml = Get-Content -LiteralPath (Join-Path $ociProject 'VAA.EAPI.Automation.MobileApp.OCI.UI.csproj')
foreach ($item in $ociXml.SelectNodes('//None[@Include and @CopyToOutputDirectory]')) {
    $itemPath = Join-Path $ociProject $item.Include.Replace('\', '/')
    if (-not (Test-Path -LiteralPath $itemPath)) { throw "Linked OCI resource not found: $($item.Include)" }
}

$myAccount = Join-Path $automation 'VAA.EAPI.Automation.MyAccount'
foreach ($factory in Get-ChildItem -LiteralPath (Join-Path $myAccount 'Requests/Factory') -Filter '*.cs') {
    foreach ($literal in [regex]::Matches([IO.File]::ReadAllText($factory.FullName), 'File\.ReadAllText\((?:@|\$)?"([^"{}]+)"\)')) {
        $resourcePath = $literal.Groups[1].Value
        if ($resourcePath.Contains('\') -or -not (Test-Path -LiteralPath (Join-Path $myAccount $resourcePath))) {
            throw "Nonportable MyAccount resource in $($factory.Name): $resourcePath"
        }
    }
}

# The unit-test discovery exclusions must behave identically on both platforms.
$workflow = [IO.File]::ReadAllText((Join-Path $root '.github/workflows/eapi-unit-tests.yml'))
if (-not $workflow.Contains('$fullName = $_.FullName.Replace(''\'', ''/'')')) {
    throw 'Unit-test discovery must normalize directory separators before filtering.'
}
foreach ($path in @('C:\repo\VAA.EAPI.Automation\Tests.csproj', '/repo/VAA.EAPI.Automation/Tests.csproj')) {
    if ($path.Replace('\', '/') -notmatch '/VAA\.EAPI\.Automation/') { throw "Exclusion failed: $path" }
}
Write-Host 'Portable Search/OCI resources, CRLF checkout and unit-test discovery checks passed.'
