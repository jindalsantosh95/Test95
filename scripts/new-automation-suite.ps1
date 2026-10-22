#requires -Version 6.1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Name,

    [Parameter(Mandatory = $true)]
    [ValidatePattern("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    [string]$Slug,

    [Parameter(Mandatory = $true)]
    [string]$Owner,

    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [Parameter(Mandatory = $true)]
    [string[]]$Categories,

    [string[]]$Cron,

    [ValidateRange(1, 360)]
    [int]$TimeoutMinutes = 60,

    [string[]]$RequiredVariables = @(),

    [string[]]$RequiredSecrets = @(
        "KEY_VAULT_URL",
        "KEY_VAULT_TENANT_ID",
        "KEY_VAULT_CLIENT_ID",
        "KEY_VAULT_CLIENT_SECRET"
    ),

    [ValidateNotNullOrEmpty()]
    [string]$SlackSecretName = "SLACK_WEBHOOK_URL"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if ([System.IO.Path]::IsPathRooted($ProjectPath) -or $ProjectPath -match "(^|[\\/])\.\.([\\/]|$)") {
    throw "ProjectPath must be a repository-relative path without '..' segments: $ProjectPath"
}

$resolvedProjectPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $ProjectPath))
$rootPrefix = $repositoryRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if (-not $resolvedProjectPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "ProjectPath must stay inside the repository: $ProjectPath"
}

if (-not (Test-Path -LiteralPath $resolvedProjectPath -PathType Leaf)) {
    throw "Project was not found: $ProjectPath"
}

if ($Categories.Count -eq 0) {
    throw "At least one test category is required."
}

$uniqueCategories = @($Categories | Select-Object -Unique)
$uniqueRequiredVariables = @($RequiredVariables | Select-Object -Unique)
$uniqueRequiredSecrets = @($RequiredSecrets + $SlackSecretName | Select-Object -Unique)

foreach ($category in $uniqueCategories) {
    if ($category -notmatch "^[A-Za-z0-9_.-]+$") {
        throw "Invalid test category '$category'."
    }
}

if ([string]::IsNullOrWhiteSpace($SlackSecretName)) {
    throw "SlackSecretName cannot be empty or whitespace."
}

foreach ($settingName in @($uniqueRequiredVariables) + @($uniqueRequiredSecrets)) {
    if ([string]::IsNullOrWhiteSpace($settingName) -or $settingName -notmatch "^[A-Z][A-Z0-9_]*$") {
        throw "Setting names must use uppercase snake case: $settingName"
    }
}

$manifestRelativePath = "config/automation-suites/$Slug.json"
$workflowRelativePath = ".github/workflows/qa-$Slug.yml"
$manifestPath = Join-Path $repositoryRoot $manifestRelativePath
$workflowPath = Join-Path $repositoryRoot $workflowRelativePath

foreach ($targetPath in @($manifestPath, $workflowPath)) {
    if (Test-Path -LiteralPath $targetPath) {
        throw "Refusing to overwrite existing file: $targetPath"
    }
}

$manifest = [ordered]@{
    '$schema' = "./schema.json"
    schemaVersion = 1
    name = $Name
    slug = $Slug
    owner = $Owner
    projectPath = $ProjectPath.Replace("\", "/")
    categories = $uniqueCategories
    summaryConfigPath = "config/regression-summary/default/default-summary.json"
    timeoutMinutes = $TimeoutMinutes
    runnerLabels = @("self-hosted", "Windows", "eapi-automation")
    requiredVariables = $uniqueRequiredVariables
    requiredSecrets = $uniqueRequiredSecrets
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $manifestPath -Encoding utf8

$scheduleLines = if ($Cron.Count -gt 0) {
    "  schedule:`n" + (($Cron | ForEach-Object { "    - cron: `"$_`"" }) -join "`n") + "`n"
} else {
    ""
}

$workflowTemplate = @'
name: '__WORKFLOW_NAME__'

on:
  workflow_dispatch:
    inputs:
      target_environment:
        description: Target environment
        required: true
        default: sit
        type: choice
        options: [dev, sit, uat]
__SCHEDULE__
jobs:
  run-suite:
    uses: ./.github/workflows/qa-run-automation-suite.yml
    with:
      manifest_path: __MANIFEST__
      target_environment: ${{ github.event.inputs.target_environment || 'sit' }}
      enable_slack_for_branch_runs: ${{ github.event_name == 'workflow_dispatch' }}
    secrets:
      key_vault_url: ${{ secrets.KEY_VAULT_URL }}
      key_vault_tenant_id: ${{ secrets.KEY_VAULT_TENANT_ID }}
      key_vault_client_id: ${{ secrets.KEY_VAULT_CLIENT_ID }}
      key_vault_client_secret: ${{ secrets.KEY_VAULT_CLIENT_SECRET }}
      slack_webhook_url: ${{ secrets.__SLACK_SECRET__ }}
'@

$workflowName = ("Automation - $Name").Replace("'", "''")
$workflow = $workflowTemplate.
    Replace("__WORKFLOW_NAME__", $workflowName).
    Replace("__SCHEDULE__", $scheduleLines).
    Replace("__MANIFEST__", $manifestRelativePath).
    Replace("__SLACK_SECRET__", $SlackSecretName)

$workflow | Set-Content -LiteralPath $workflowPath -Encoding utf8

& (Join-Path $PSScriptRoot "resolve-automation-suite.ps1") -ManifestPath $manifestRelativePath | Out-Null

Write-Host "Created $manifestRelativePath"
Write-Host "Created $workflowRelativePath"
Write-Host "Provision these repository settings before the first live run:"
foreach ($variableName in $uniqueRequiredVariables) {
    Write-Host "  variable: $variableName"
}
foreach ($secretName in $manifest.requiredSecrets) {
    Write-Host "  secret: $secretName"
}
