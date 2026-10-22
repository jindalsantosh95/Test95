param(
    [Parameter(Mandatory = $true)]
    [long]$RunId,

    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "sit", "uat")]
    [string]$Environment = "",

    [Parameter(Mandatory = $false)]
    [string]$Repository = "VAAD1ST/eapi",

    [Parameter(Mandatory = $false)]
    [string]$AzureTraceScriptPath = "",

    [Parameter(Mandatory = $false)]
    [string]$PythonCommand = "python3",

    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory = "",

    [Parameter(Mandatory = $false)]
    [ValidatePattern("^[1-9][0-9]*d$")]
    [string]$Lookback = "",

    [Parameter(Mandatory = $false)]
    [switch]$TraceAllIdentifiers
)

$ErrorActionPreference = "Stop"

function Assert-CommandAvailable {
    param([string]$Name)

    if ($null -eq (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found."
    }
}

function Get-ManifestLookback {
    param(
        [object]$Manifest,
        [string]$Override
    )

    if (-not [string]::IsNullOrWhiteSpace($Override)) {
        return $Override.ToLowerInvariant()
    }

    $timestamps = @($Manifest.failures |
        ForEach-Object { @($_.startTimeUtc, $_.endTimeUtc) } |
        Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
        ForEach-Object {
            $parsed = [DateTimeOffset]::MinValue
            if ([DateTimeOffset]::TryParse([string]$_, [ref]$parsed)) {
                $parsed.ToUniversalTime()
            }
        })

    if ($timestamps.Count -eq 0) {
        return "30d"
    }

    $earliest = @($timestamps | Sort-Object | Select-Object -First 1)[0]
    $ageDays = [Math]::Ceiling(([DateTimeOffset]::UtcNow - $earliest).TotalDays)
    $lookbackDays = [Math]::Max(2, $ageDays + 1)
    return "$lookbackDays`d"
}

Assert-CommandAvailable -Name "gh"
Assert-CommandAvailable -Name "az"
Assert-CommandAvailable -Name $PythonCommand

& gh auth status | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run 'gh auth login' and retry."
}

& az account show --output none
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not authenticated. Run 'az login' and retry."
}

if ([string]::IsNullOrWhiteSpace($AzureTraceScriptPath)) {
    $AzureTraceScriptPath = [string]$env:AZURE_CORRELATION_TRACE_SCRIPT
}
if ([string]::IsNullOrWhiteSpace($AzureTraceScriptPath)) {
    $userProfile = [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
    $AzureTraceScriptPath = Join-Path $userProfile ".codex/skills/azure-correlation-trace/scripts/trace_app_insights.py"
}
if (-not (Test-Path -LiteralPath $AzureTraceScriptPath -PathType Leaf)) {
    throw "Azure correlation trace helper was not found at '$AzureTraceScriptPath'. Set AZURE_CORRELATION_TRACE_SCRIPT or pass -AzureTraceScriptPath."
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "eapi-trace-run-$RunId-$([guid]::NewGuid().ToString('N'))"
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$artifactDirectory = Join-Path $OutputDirectory "artifacts"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null

Write-Host "Downloading regression artifacts for $Repository run $RunId..."
& gh run download $RunId --repo $Repository --pattern "*graphql*regression-results-*" --dir $artifactDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Unable to download regression artifacts for run $RunId."
}

$manifestFiles = @(Get-ChildItem -LiteralPath $artifactDirectory -Filter "trace-manifest-*.json" -File -Recurse)
if ($manifestFiles.Count -eq 0) {
    throw "No trace manifest was found. The run may predate failure-manifest support or contain no failed tests."
}

$manifests = @($manifestFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json })
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $manifests = @($manifests | Where-Object { $_.environment -eq $Environment })
}
if ($manifests.Count -eq 0) {
    throw "No trace manifest matched environment '$Environment'."
}

$summaryLines = @(
    "# Local Azure failure trace",
    "",
    "- Repository: ``$Repository``",
    "- Workflow run: ``$RunId``",
    "- Generated: ``$([DateTime]::UtcNow.ToString('o'))``",
    ""
)

foreach ($manifest in $manifests) {
    $traceEnvironment = ([string]$manifest.environment).ToLowerInvariant()
    if (@("dev", "sit", "uat") -notcontains $traceEnvironment) {
        throw "Manifest requested forbidden environment '$traceEnvironment'. Only dev, sit, and uat are supported."
    }

    $summaryLines += "## $($manifest.suite) ($traceEnvironment)"
    $summaryLines += ""
    $summaryLines += "Failures: **$($manifest.failureCount)**"
    $manifestLookback = Get-ManifestLookback -Manifest $manifest -Override $Lookback
    $summaryLines += "Azure lookback: **$manifestLookback**"
    $summaryLines += ""

    $identifiers = if ($TraceAllIdentifiers) {
        @($manifest.failures |
            ForEach-Object { @($_.identifiers) } |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
            Select-Object -Unique)
    }
    else {
        @($manifest.incidents |
            ForEach-Object { @($_.identifiers) | Select-Object -First 1 } |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
            Select-Object -Unique)
    }

    if ($identifiers.Count -eq 0) {
        $summaryLines += "No correlation or error identifiers were present in the failed-test output."
        $summaryLines += ""
        continue
    }

    $traceDirectory = Join-Path $OutputDirectory $traceEnvironment
    New-Item -ItemType Directory -Path $traceDirectory -Force | Out-Null
    foreach ($identifier in $identifiers) {
        $tracePath = Join-Path $traceDirectory "$identifier.json"
        Write-Host "Tracing $identifier in $traceEnvironment..."
        $traceOutput = & $PythonCommand $AzureTraceScriptPath --id $identifier --environment $traceEnvironment --lookback $manifestLookback
        $traceExitCode = $LASTEXITCODE
        if ($traceExitCode -ne 0 -and $traceExitCode -ne 3) {
            Write-Warning "Trace failed for $identifier."
            $summaryLines += "- ``$identifier``: trace failed"
            continue
        }

        $traceJson = $traceOutput -join "`n"
        $traceJson | Set-Content -LiteralPath $tracePath -Encoding utf8
        $trace = $traceJson | ConvertFrom-Json
        $operationIds = @($trace.operationIds)
        $eventCount = @($trace.matches | ForEach-Object { @($_.events) }).Count
        $matchText = if ($traceExitCode -eq 3) { "no matching telemetry" } else { "$eventCount matching event(s), $($operationIds.Count) distributed operation(s)" }
        $summaryLines += "- ``$identifier``: $matchText; raw trace ``$tracePath``"
    }
    $summaryLines += ""
}

$summaryPath = Join-Path $OutputDirectory "azure-trace-summary.md"
$summaryLines -join "`n" | Set-Content -LiteralPath $summaryPath -Encoding utf8
Write-Host "Trace summary: $summaryPath"
Write-Output ($summaryLines -join "`n")
