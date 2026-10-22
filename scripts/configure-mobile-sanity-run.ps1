[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$BuildName,
    [ValidateSet('Sanity', 'Regression', 'Targeted')][string]$Pack = 'Sanity',
    [string]$TestName = ''
)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'This settings writer is for GitHub Actions only; local settings are left unchanged.' }
$root = Join-Path $PSScriptRoot '../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI'
$manifest = Get-Content -Raw (Join-Path $PSScriptRoot '../config/automation-suites/mobile-search-book-sanity.json') | ConvertFrom-Json
foreach ($name in @($manifest.requiredSecrets | Where-Object { $_ -ne 'SLACK_WEBHOOK_URL' })) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        throw "Required action secret is missing: $name"
    }
}
if ($env:MOBILE_APP_ID -notmatch '\Abs://[a-f0-9]{40}\z') { throw 'A valid BrowserStack app upload ID is required.' }
$settings = Get-Content -Raw (Join-Path $root 'local.settings.local.example.json') | ConvertFrom-Json
$settings.RunMode = 'Remote'
$settings.Platform = 'iOS'
$settings.BrowserStack.Username = $env:BROWSERSTACK_USERNAME
$settings.BrowserStack.AccessKey = $env:BROWSERSTACK_ACCESS_KEY
$settings.BrowserStack.App = $env:MOBILE_APP_ID
$settings.BrowserStack.Build = $BuildName
$settings.BrowserStack.Project = 'Virgin Atlantic Mobile - Search and Book'
# Release qualification needs video and visual evidence across native/HPP transitions.
$settings.BrowserStack.Profile = 'Diagnostic'
# Ordinary qualification must not retain payment/loyalty response bodies remotely.
# Content capture requires a separately authorised targeted diagnostic run.
$settings.BrowserStack.CaptureNetworkContent = $false
$settings.Loyalty.Password = $env:LOYALTY_ACCOUNT_PASSWORD
$settings.Core.KeyVaultUrl = $env:KEY_VAULT_URL
$settings.Core.KeyVaultTenantId = $env:KEY_VAULT_TENANT_ID
$settings.Core.KeyVaultClientId = $env:KEY_VAULT_CLIENT_ID
$settings.Core.KeyVaultClientSecret = $env:KEY_VAULT_CLIENT_SECRET
# Regression includes backend comparisons executed from the self-hosted runner.
$targetedBackend = $false
if ($Pack -eq 'Targeted' -and -not [string]::IsNullOrEmpty($TestName)) {
    $contract = Get-Content -Raw (Join-Path $root 'Contracts/search-book-automation-contract.json') | ConvertFrom-Json
    $matches = @($contract.scenarios | Where-Object {
        $_.testMethod -ceq $TestName -and $_.id -in @($contract.releasePacks.regression) -and
        $_.id -in @($contract.releasePacks.backendRegression)
    })
    $targetedBackend = $matches.Count -eq 1
}
$settings.BackendValidation.Enabled = $Pack -eq 'Regression' -or $targetedBackend
$settings | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $root 'local.settings.local.json') -Encoding utf8
Write-Host "Configured remote iOS $Pack for SIT; secret values withheld."
