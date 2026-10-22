#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Profile = 'docs-local',
    [string]$RegistryPath,
    [switch]$Probe,
    [string]$EvidencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
if ([string]::IsNullOrWhiteSpace($RegistryPath)) { $RegistryPath = Join-Path $repoRoot 'docs/agent-tool-environments.json' }
$failures = [System.Collections.Generic.List[string]]::new()

try {
    $jsonParams = @{}
    if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $jsonParams.DateKind = 'String' }
    $registry = Get-Content -LiteralPath $RegistryPath -Raw | ConvertFrom-Json @jsonParams
}
catch { Write-Host "agent tool environment check failed: invalid or missing registry: $RegistryPath" -ForegroundColor Red; exit 1 }

if ([int]$registry.schemaVersion -ne 1) { $failures.Add('schemaVersion must be 1.') }
$reviewed = [DateTimeOffset]::MinValue
if (-not [DateTimeOffset]::TryParse([string]$registry.reviewedAtUtc, [ref]$reviewed) -or $reviewed.Offset -ne [TimeSpan]::Zero) {
    $failures.Add('reviewedAtUtc must be an explicit UTC timestamp.')
} elseif (((Get-Date).ToUniversalTime() - $reviewed.UtcDateTime).TotalDays -gt [int]$registry.maxReviewAgeDays) {
    $failures.Add("Tool environment registry is older than $($registry.maxReviewAgeDays) days.")
}

$profileProperty = $registry.profiles.psobject.Properties | Where-Object Name -eq $Profile | Select-Object -First 1
if ($null -eq $profileProperty) { $failures.Add("Profile '$Profile' is not enrolled.") }
else {
    $definition = $profileProperty.Value
    foreach ($field in @('description', 'networkPolicy', 'secretPolicy', 'evidenceRoot', 'cleanup')) {
        if ([string]::IsNullOrWhiteSpace([string]$definition.$field)) { $failures.Add("Profile '$Profile' is missing $field.") }
    }
    if (@($definition.requiredCommands).Count -eq 0) { $failures.Add("Profile '$Profile' has no requiredCommands.") }
    if (@($definition.requiredCommands | Select-Object -Unique).Count -ne @($definition.requiredCommands).Count) { $failures.Add("Profile '$Profile' contains duplicate requiredCommands.") }
    $versionProperties = @()
    if ($null -ne $definition.minimumMajorVersions) { $versionProperties += @($definition.minimumMajorVersions.psobject.Properties) }
    if ($null -ne $definition.pinnedCommandVersions) { $versionProperties += @($definition.pinnedCommandVersions.psobject.Properties) }
    foreach ($versionProperty in $versionProperties) {
        if (@($definition.requiredCommands) -notcontains $versionProperty.Name) { $failures.Add("Profile '$Profile' versions undeclared command '$($versionProperty.Name)'.") }
    }
    foreach ($relativePath in @($definition.writableRoots) + @([string]$definition.evidenceRoot)) {
        $normalized = ([string]$relativePath).Replace('\', '/').Trim('/')
        if ([string]::IsNullOrWhiteSpace($normalized) -or [System.IO.Path]::IsPathRooted([string]$relativePath) -or $normalized.Split('/') -contains '..' -or -not $normalized.StartsWith('TestResults')) {
            $failures.Add("Profile '$Profile' has unsafe writable path '$relativePath'.")
        }
    }

    if ($Probe) {
        foreach ($command in @($definition.requiredCommands)) {
            if ($null -eq (Get-Command -Name ([string]$command) -ErrorAction SilentlyContinue)) { $failures.Add("Required command '$command' is unavailable for profile '$Profile'.") }
        }
        $minimumVersionProperties = if ($null -ne $definition.minimumMajorVersions) { @($definition.minimumMajorVersions.psobject.Properties) } else { @() }
        foreach ($versionProperty in $minimumVersionProperties) {
            if ($failures.Count -gt 0) { break }
            $versionOutput = (& ([string]$versionProperty.Name) '--version' 2>&1 | Select-Object -First 1) -join ''
            $match = [regex]::Match([string]$versionOutput, '(?<major>\d+)\.')
            if (-not $match.Success -or [int]$match.Groups['major'].Value -lt [int]$versionProperty.Value) { $failures.Add("Command '$($versionProperty.Name)' does not meet minimum major version $($versionProperty.Value).") }
        }
        if ($definition.worktreeRequired -eq $true) {
            $inside = (& git -C $repoRoot rev-parse --is-inside-work-tree 2>$null).Trim()
            if ($LASTEXITCODE -ne 0 -or $inside -ne 'true') { $failures.Add("Profile '$Profile' requires a Git worktree.") }
        }
        if ($failures.Count -eq 0) {
            $probeRoot = Join-Path $repoRoot ([string]$definition.evidenceRoot)
            $probeDirectory = Join-Path $probeRoot ('probe-' + [guid]::NewGuid().ToString('N'))
            try {
                New-Item -ItemType Directory -Path $probeDirectory -Force | Out-Null
                $probeFile = Join-Path $probeDirectory 'probe.txt'
                [guid]::NewGuid().ToString('N') | Set-Content -LiteralPath $probeFile -Encoding utf8
                if (-not (Test-Path -LiteralPath $probeFile -PathType Leaf)) { $failures.Add('Isolated evidence write/read probe failed.') }
            } finally {
                if (Test-Path -LiteralPath $probeDirectory) { Remove-Item -LiteralPath $probeDirectory -Recurse -Force }
            }
        }
    }
}

$passed = $failures.Count -eq 0
if (-not [string]::IsNullOrWhiteSpace($EvidencePath)) {
    $parent = Split-Path -Parent $EvidencePath
    if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    [ordered]@{ schemaVersion = 1; profile = $Profile; passed = $passed; probe = [bool]$Probe; checkedAtUtc = (Get-Date).ToUniversalTime().ToString('o'); failures = @($failures) } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $EvidencePath -Encoding utf8
}
if (-not $passed) { Write-Host "agent tool environment check failed: $($failures -join ' ')" -ForegroundColor Red; exit 1 }
Write-Host "agent tool environment check passed: $Profile (probe=$([bool]$Probe))" -ForegroundColor Green
exit 0
