[CmdletBinding()]
param(
    [string]$AppId = $env:MOBILE_APP_ID,
    [Parameter(Mandatory)][string]$MetadataPath,
    [string]$GithubOutputPath = $env:GITHUB_OUTPUT
)
$ErrorActionPreference = 'Stop'
$selected = $null
if (-not [string]::IsNullOrWhiteSpace($AppId)) {
    if ($AppId -cnotmatch '\Abs://[a-f0-9]{40}\z') { throw 'Invalid explicit BrowserStack app ID.' }
    $selected = [ordered]@{ schemaVersion = 1; mode = 'Explicit'; appUrl = $AppId; name = ''; version = ''; uploadedAt = '' }
} else {
    if ([string]::IsNullOrWhiteSpace($env:BROWSERSTACK_USERNAME) -or [string]::IsNullOrWhiteSpace($env:BROWSERSTACK_ACCESS_KEY)) {
        throw 'BrowserStack credentials are required to resolve the latest SIT iOS upload.'
    }
    $encoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("$($env:BROWSERSTACK_USERNAME):$($env:BROWSERSTACK_ACCESS_KEY)"))
    $headers = @{ Authorization = "Basic $encoded" }
    $candidates = @()
    $complete = $false
    # Inspect bounded pages, not just the latest upload (which may be Android/production).
    for ($page = 0; $page -lt 10; $page++) {
        try {
            $response = Invoke-RestMethod -Method Get -Headers $headers -TimeoutSec 20 -MaximumRedirection 0 `
                -Uri "https://api-cloud.browserstack.com/app-automate/recent_group_apps?limit=100&offset=$($page * 100)"
            $apps = @($response)
        } catch { throw 'BrowserStack app inventory could not be read; no app selected. Check access/service availability.' }
        foreach ($app in $apps) {
            $match = [regex]::Match([string]$app.app_name, '\AVirginAtlantic-sit-debug-v(?<version>[0-9]+(?:\.[0-9]+){1,3})-[0-9]+-[0-9]{8}-[0-9]{4}\.ipa\z')
            if (-not $match.Success) { continue }
            $uploaded = [DateTimeOffset]::MinValue
            $styles = [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal
            if ([string]$app.app_url -cnotmatch '\Abs://[a-f0-9]{40}\z' -or
                [string]$app.app_url -cne "bs://$($app.app_id)" -or
                [string]$app.app_version -cne $match.Groups['version'].Value -or
                -not [DateTimeOffset]::TryParseExact([string]$app.uploaded_at, "yyyy-MM-dd HH:mm:ss 'UTC'", [Globalization.CultureInfo]::InvariantCulture, $styles, [ref]$uploaded)) {
                throw 'Matching SIT iOS upload has invalid metadata; refusing ambiguous app selection.'
            }
            $candidates += [pscustomobject]@{ appUrl = [string]$app.app_url; name = [string]$app.app_name; version = [string]$app.app_version; uploaded = $uploaded }
        }
        if ($apps.Count -lt 100) { $complete = $true; break }
    }
    if (-not $complete) { throw 'BrowserStack inventory exceeded the bounded scan; specify an explicit app ID.' }
    if ($candidates.Count -eq 0) { throw 'No VirginAtlantic SIT debug iOS IPA upload was found; no fallback is permitted.' }
    $latest = $candidates | Sort-Object uploaded -Descending | Select-Object -First 1
    $ties = @($candidates | Where-Object { $_.uploaded -eq $latest.uploaded } | Select-Object -ExpandProperty appUrl -Unique)
    if ($ties.Count -ne 1) { throw 'Multiple SIT iOS builds have the latest upload timestamp; specify an explicit app ID.' }
    $selected = [ordered]@{ schemaVersion = 1; mode = 'LatestSitIos'; appUrl = $latest.appUrl; name = $latest.name; version = $latest.version; uploadedAt = $latest.uploaded.ToString('u') }
}
$parent = Split-Path -Parent $MetadataPath
if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
$selected | ConvertTo-Json | Set-Content -LiteralPath $MetadataPath -Encoding utf8
if ($GithubOutputPath) { "app_id=$($selected.appUrl)" | Out-File -LiteralPath $GithubOutputPath -Encoding utf8 -Append }
Write-Host "Resolved iOS app ($($selected.mode)): $($selected.appUrl) $($selected.name) $($selected.uploadedAt)"
