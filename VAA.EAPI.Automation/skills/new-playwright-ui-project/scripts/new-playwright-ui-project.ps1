#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Z][A-Za-z0-9]*$')]
    [string]$DomainName,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Description,

    [string]$AutomationRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
)

$ErrorActionPreference = 'Stop'
$solutionPath = Join-Path $AutomationRoot 'VAA.EAPI.Automation.sln'
if (-not (Test-Path $solutionPath -PathType Leaf)) {
    throw "VAA.EAPI.Automation.sln was not found beneath '$AutomationRoot'."
}

$projectName = "VAA.EAPI.Automation.$DomainName.UI"
$projectPath = Join-Path $AutomationRoot $projectName
if (Test-Path $projectPath) {
    throw "Refusing to overwrite existing path '$projectPath'."
}

$environmentPrefix = ([regex]::Replace($DomainName, '(?<!^)([A-Z])', '_$1')).ToUpperInvariant()
$baseUrlVariable = "${environmentPrefix}_UI_BASE_URL"

function Write-TemplateFile {
    param([string]$RelativePath, [string]$Content)

    $path = Join-Path $projectPath $RelativePath
    $directory = Split-Path $path -Parent
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $normalised = ($Content.TrimStart("`r", "`n") -replace "`r?`n", "`r`n") + "`r`n"
    $encoding = [System.Text.UTF8Encoding]::new($RelativePath.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase))
    [System.IO.File]::WriteAllText($path, $normalised, $encoding)
}

New-Item -ItemType Directory -Path $projectPath | Out-Null

Write-TemplateFile "$projectName.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="BrowserStack.Playwright" />
    <PackageReference Include="Xunit.SkippableFact" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\VAA.EAPI.Automation.Core\VAA.EAPI.Automation.Core.csproj" />
    <ProjectReference Include="..\VAA.EAPI.Automation.Core.Playwright\VAA.EAPI.Automation.Core.Playwright.csproj" />
  </ItemGroup>
</Project>
"@

Write-TemplateFile 'Pages/BasePage.cs' @"
using Microsoft.Playwright;

namespace $projectName.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }
}
"@

Write-TemplateFile 'Pages/LandingPage.cs' @"
using Microsoft.Playwright;

namespace $projectName.Pages;

public sealed class LandingPage : BasePage
{
    public LandingPage(IPage page) : base(page)
    {
    }

    public async Task OpenAsync(string baseUrl)
    {
        await Page.GotoAsync(baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    public async Task<string> ReadTitleAsync()
    {
        return await Page.TitleAsync();
    }
}
"@

Write-TemplateFile 'Flows/LandingSmokeFlow.cs' @"
using VAA.EAPI.Automation.Core.Playwright.Browser;
using $projectName.Pages;

namespace $projectName.Flows;

public sealed class LandingSmokeFlow
{
    public async Task<LandingSmokeResult> RunAsync(PlaywrightBrowserOptions options)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        LandingPage landingPage = new(session.Page);
        await landingPage.OpenAsync(options.BaseUrl.ToString());
        string title = await landingPage.ReadTitleAsync();
        await session.MarkPassedAsync("The landing page loaded with a title.");
        return new LandingSmokeResult(title);
    }
}

public sealed record LandingSmokeResult(string Title);
"@

Write-TemplateFile 'Assertions/LandingAssertions.cs' @"
using $projectName.Flows;

namespace $projectName.Assertions;

public static class LandingAssertions
{
    public static void HasLoaded(LandingSmokeResult result)
    {
        Assert.False(string.IsNullOrWhiteSpace(result.Title), "The landing page title should be present.");
    }
}
"@

Write-TemplateFile 'Tests/Smoke/LandingSmokeTests.cs' @"
using $projectName.Assertions;
using $projectName.Flows;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using Xunit;

namespace $projectName.Tests.Smoke;

public sealed class LandingSmokeTests
{
    [SkippableFact]
    [Trait("Category", "$DomainName-UI-Smoke")]
    public async Task LandingPage_Loads()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("$baseUrlVariable");
        Skip.If(string.IsNullOrWhiteSpace(baseUrl), "$baseUrlVariable is not configured.");

        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl!);
        LandingSmokeFlow flow = new();
        LandingSmokeResult result = await flow.RunAsync(options);

        LandingAssertions.HasLoaded(result);
    }
}
"@

Write-TemplateFile 'settings.example.json' @"
{
  "$baseUrlVariable": "https://replace-with-environment.example"
}
"@

Write-TemplateFile 'Scripts/Setup.ps1' @"
#requires -Version 7.0
`$ErrorActionPreference = 'Stop'
`$project = Join-Path `$PSScriptRoot '..\$projectName.csproj'
dotnet restore `$project
dotnet build `$project -c Release --no-restore
`$playwright = Get-ChildItem (Join-Path `$PSScriptRoot '..\bin\Release') -Filter 'playwright.ps1' -Recurse | Select-Object -First 1
if (`$null -eq `$playwright) { throw 'playwright.ps1 was not produced by the build.' }
pwsh `$playwright.FullName install chromium
"@

Write-TemplateFile 'Scripts/Run-Regression.ps1' @"
#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]`$BaseUrl)
`$env:$baseUrlVariable = `$BaseUrl
`$project = Join-Path `$PSScriptRoot '..\$projectName.csproj'
dotnet test `$project -c Release --filter 'Category=$DomainName-UI-Smoke' --logger 'trx;LogFileName=$DomainName-ui.trx'
"@

Write-TemplateFile 'README.md' @"
# $projectName

$Description.

## Architecture

Tests express scenarios, flows orchestrate journeys, pages own every Playwright call and selector,
and assertion helpers validate business outcomes. Do not import `Microsoft.Playwright` from `Tests`
or `Flows`.

## Setup

Use `settings.example.json` as a list of required environment variables; it is documentation, not
automatically loaded configuration. Set `$baseUrlVariable` directly before a manual run.

```pwsh
Scripts/Setup.ps1
Scripts/Run-Regression.ps1 -BaseUrl https://replace-with-environment.example
```

The setup script installs Chromium only. Add BrowserStack and cross-browser execution deliberately
after the starter smoke path is stable.
"@

dotnet sln $solutionPath add (Join-Path $projectPath "$projectName.csproj")
Write-Host "Created $projectName and added it to VAA.EAPI.Automation.sln."
Write-Host 'No packages, browsers, or live tests were run.'
