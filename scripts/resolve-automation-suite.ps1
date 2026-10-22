#requires -Version 6.1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ManifestPath,

    [string]$GithubOutputPath
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Resolve-RepositoryFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if ([System.IO.Path]::IsPathRooted($RelativePath) -or $RelativePath -match "(^|[\\/])\.\.([\\/]|$)") {
        throw "$Description must be repository-relative without '..' segments: $RelativePath"
    }

    $rootPrefix = $repositoryRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $resolvedPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $RelativePath))

    if (-not $resolvedPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Description must stay inside the repository: $RelativePath"
    }

    return $resolvedPath
}

$resolvedManifestPath = Resolve-RepositoryFilePath -RelativePath $ManifestPath -Description "ManifestPath"

if (-not (Test-Path -LiteralPath $resolvedManifestPath -PathType Leaf)) {
    throw "Automation suite manifest was not found: $ManifestPath"
}

$manifestJson = Get-Content -LiteralPath $resolvedManifestPath -Raw
$schemaPath = Join-Path $repositoryRoot "config/automation-suites/schema.json"

try {
    $schemaIsValid = $manifestJson | Test-Json -SchemaFile $schemaPath -ErrorAction Stop
} catch {
    throw "Automation suite manifest does not satisfy schema.json: $($_.Exception.Message)"
}

if (-not $schemaIsValid) {
    throw "Automation suite manifest does not satisfy config/automation-suites/schema.json."
}

$manifest = $manifestJson | ConvertFrom-Json

if ($manifest.schemaVersion -ne 1) {
    throw "Unsupported automation suite schemaVersion '$($manifest.schemaVersion)'. Expected 1."
}

foreach ($requiredProperty in @("name", "slug", "owner", "projectPath", "categories")) {
    if ($null -eq $manifest.$requiredProperty -or [string]::IsNullOrWhiteSpace([string]$manifest.$requiredProperty)) {
        throw "Manifest property '$requiredProperty' is required."
    }
}

if ([string]$manifest.slug -notmatch "^[a-z0-9]+(?:-[a-z0-9]+)*$") {
    throw "Manifest slug must be lowercase kebab-case: $($manifest.slug)"
}

$categories = @($manifest.categories)
if ($categories.Count -eq 0) {
    throw "Manifest categories must contain at least one test category."
}

foreach ($category in $categories) {
    if ([string]$category -notmatch "^[A-Za-z0-9_.-]+$") {
        throw "Invalid test category '$category'."
    }
}

$projectPath = [string]$manifest.projectPath
$resolvedProjectPath = Resolve-RepositoryFilePath -RelativePath $projectPath -Description "projectPath"
if (-not (Test-Path -LiteralPath $resolvedProjectPath -PathType Leaf)) {
    throw "Manifest projectPath was not found: $projectPath"
}

$summaryConfigPath = if ([string]::IsNullOrWhiteSpace([string]$manifest.summaryConfigPath)) {
    "config/regression-summary/default/default-summary.json"
} else {
    [string]$manifest.summaryConfigPath
}

foreach ($configPath in @($summaryConfigPath, [string]$manifest.slackConfigPath)) {
    if (-not [string]::IsNullOrWhiteSpace($configPath)) {
        $resolvedConfigPath = Resolve-RepositoryFilePath -RelativePath $configPath -Description "configuration path"
        if (-not (Test-Path -LiteralPath $resolvedConfigPath -PathType Leaf)) {
            throw "Manifest configuration file was not found: $configPath"
        }
    }
}

$timeoutMinutes = if ($null -eq $manifest.timeoutMinutes) { 60 } else { [int]$manifest.timeoutMinutes }
if ($timeoutMinutes -lt 1 -or $timeoutMinutes -gt 360) {
    throw "Manifest timeoutMinutes must be between 1 and 360."
}

$runnerLabels = if ($null -eq $manifest.runnerLabels) {
    @("self-hosted", "Windows", "eapi-automation")
} else {
    @($manifest.runnerLabels)
}

if ($runnerLabels.Count -eq 0) {
    throw "Manifest runnerLabels must contain at least one label."
}

$testFilter = (($categories | ForEach-Object { "Category=$_" }) -join "`n")
if ($manifest.PSObject.Properties.Name -contains 'testFilter') {
    if ([string]$manifest.testFilter -notmatch '\A[A-Za-z0-9_.=&|()~-]+\z') {
        throw 'Invalid manifest testFilter.'
    }
    $testFilter = [string]$manifest.testFilter
}
$requiredVariables = if ($null -eq $manifest.requiredVariables) { @() } else { @($manifest.requiredVariables) }
$allowedVariables = if ($null -eq $manifest.allowedVariables) { @() } else { @($manifest.allowedVariables) }
$requiredSecrets = if ($null -eq $manifest.requiredSecrets) { @() } else { @($manifest.requiredSecrets) }
$slackSecretName = [string]$manifest.slackSecretName
$outputs = [ordered]@{
    suite_name = [string]$manifest.name
    workflow_slug = [string]$manifest.slug
    test_project_path = $projectPath
    test_filter = $testFilter
    summary_config_path = $summaryConfigPath
    slack_config_path = [string]$manifest.slackConfigPath
    slack_secret_name = $slackSecretName
    timeout_minutes = [string]$timeoutMinutes
    runner_labels_json = ConvertTo-Json -InputObject @($runnerLabels) -Compress
    required_variables_json = ConvertTo-Json -InputObject @($requiredVariables) -Compress
    allowed_variables_json = ConvertTo-Json -InputObject @($allowedVariables) -Compress
    required_secrets_json = ConvertTo-Json -InputObject @($requiredSecrets) -Compress
}

if ([string]::IsNullOrWhiteSpace($GithubOutputPath)) {
    $outputs | ConvertTo-Json -Depth 5
    return
}

foreach ($entry in $outputs.GetEnumerator()) {
    $delimiter = "eapi_$([Guid]::NewGuid().ToString('N'))"
    @(
        "$($entry.Key)<<$delimiter"
        [string]$entry.Value
        $delimiter
    ) | Out-File -LiteralPath $GithubOutputPath -Encoding utf8 -Append
}
