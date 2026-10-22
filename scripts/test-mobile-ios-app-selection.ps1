$ErrorActionPreference = 'Stop'
$directory = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$metadata = Join-Path $directory 'app.json'
$output = Join-Path $directory 'output.txt'
$originalUsername = $env:BROWSERSTACK_USERNAME
$originalKey = $env:BROWSERSTACK_ACCESS_KEY
$env:BROWSERSTACK_USERNAME = 'synthetic'
$env:BROWSERSTACK_ACCESS_KEY = 'SECRET_SENTINEL'
function New-App($character, $name, $uploaded) {
    [pscustomobject]@{ app_name = $name; app_version = '1.13.0'; app_id = ($character * 40); app_url = ('bs://' + ($character * 40)); uploaded_at = $uploaded }
}
$old = New-App 'a' 'VirginAtlantic-sit-debug-v1.13.0-5-20261013-1032.ipa' '2026-10-13 10:45:28 UTC'
$latest = New-App 'b' 'VirginAtlantic-sit-debug-v1.13.0-6-20261013-1334.ipa' '2026-10-13 13:48:19 UTC'
$android = New-App 'c' 'VirginAtlantic-prod-release-v1.13.0-934-20261013-1358.aab' '2026-10-13 14:10:21 UTC'
$fixture = @{ requests = 0; case = ''; apps = @() }
$mock = {
    param($Method, $Headers, $TimeoutSec, $MaximumRedirection, $Uri)
    $fixture.requests++
    if ($Uri -notmatch '\Ahttps://api-cloud.browserstack.com/app-automate/recent_group_apps\?limit=100&offset=[0-9]+\z' -or
        $TimeoutSec -ne 20 -or $MaximumRedirection -ne 0 -or $Method -ne 'Get') { throw 'Unsafe inventory request' }
    if ($fixture.case -eq 'transport') { throw 'SECRET_SENTINEL' }
    if ($fixture.case -eq 'limit') { return @(1..100 | ForEach-Object { $android }) }
    if ($fixture.case -eq 'paged') {
        if ($fixture.requests -eq 1) { return @(1..100 | ForEach-Object { $android }) }
        return @($latest)
    }
    # REST JSON arrays are returned as one pipeline object, unlike ordinary function output.
    return ,$fixture.apps
}.GetNewClosure()
Set-Item Function:Invoke-RestMethod $mock
try {
    foreach ($script:case in @('latest', 'paged', 'explicit', 'empty', 'wrong-platform', 'malformed', 'tie', 'transport', 'limit', 'bad-override')) {
        $script:requests = 0
        $script:apps = @($old, $android, $latest) # deliberately unsorted
        $override = ''
        switch ($script:case) {
            'explicit' { $override = 'bs://' + ('d' * 40) }
            'bad-override' { $override = "bs://bad`ninjected=true" }
            'empty' { $script:apps = @() }
            'wrong-platform' { $script:apps = @($android) }
            'malformed' { $script:apps = @(New-App 'b' $latest.app_name 'invalid') }
            'tie' { $script:apps = @($latest, (New-App 'e' $latest.app_name $latest.uploaded_at)) }
        }
        'preserve' | Set-Content -LiteralPath $metadata
        $fixture.case = $script:case; $fixture.apps = $script:apps; $fixture.requests = 0
        'preserve' | Set-Content -LiteralPath $output
        $errorText = ''
        try { & (Join-Path $PSScriptRoot 'resolve-mobile-ios-app.ps1') -AppId $override -MetadataPath $metadata -GithubOutputPath $output }
        catch { $errorText = $_.ToString() }
        $shouldPass = $script:case -in @('latest', 'paged', 'explicit')
        if ($shouldPass) {
            if ($errorText) { throw "Unexpected failure: $script:case $errorText" }
            $record = Get-Content -Raw -LiteralPath $metadata | ConvertFrom-Json
            $expected = if ($script:case -eq 'explicit') { $override } else { $latest.app_url }
            if ($record.appUrl -ne $expected -or -not (Get-Content -Raw $output).Contains("app_id=$expected")) { throw 'Wrong pinned build' }
            if ($script:case -eq 'explicit' -and $fixture.requests -ne 0) { throw 'Explicit override queried inventory' }
            if ($script:case -eq 'paged' -and $fixture.requests -ne 2) { throw 'Pagination was not exercised' }
        } else {
            if (-not $errorText -or (Get-Content -Raw $metadata).Trim() -ne 'preserve' -or (Get-Content -Raw $output).Trim() -ne 'preserve') { throw "Failure was not closed: $script:case" }
            if ($errorText.Contains('SECRET_SENTINEL')) { throw 'Transport secret leaked' }
            if ($script:case -eq 'limit' -and $fixture.requests -ne 10) { throw 'Inventory scan was not bounded' }
        }
        Write-Host "PASS: app selection $script:case"
    }
    $report = Join-Path $directory 'payload.json'
    & (Join-Path $PSScriptRoot 'resolve-mobile-ios-app.ps1') -AppId ('bs://' + ('d' * 40)) -MetadataPath $metadata -GithubOutputPath $output
    & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath (Join-Path $directory 'missing.trx') -Pack Sanity -JobStatus failure `
        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -AppSelectionPath $metadata -PreviewOnly -PayloadPath $report
    if (-not (Get-Content -Raw $report).Contains('Explicit override')) { throw 'Explicit app report missing' }
    $script:case = 'latest'; $script:apps = @($latest)
    $fixture.case = 'latest'; $fixture.apps = @($latest)
    & (Join-Path $PSScriptRoot 'resolve-mobile-ios-app.ps1') -AppId '' -MetadataPath $metadata -GithubOutputPath $output
    & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath (Join-Path $directory 'missing.trx') -Pack Sanity -JobStatus failure `
        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -AppSelectionPath $metadata -PreviewOnly -PayloadPath $report
    $raw = Get-Content -Raw $report
    if (-not $raw.Contains($latest.app_name) -or -not $raw.Contains($latest.app_url) -or -not $raw.Contains('2026-10-13 13:48:19Z') -or $raw.Contains('SECRET_SENTINEL')) { throw 'Safe app provenance missing from Slack' }
    Write-Host 'PASS: selected app provenance in Slack preview'
} finally {
    $env:BROWSERSTACK_USERNAME = $originalUsername
    $env:BROWSERSTACK_ACCESS_KEY = $originalKey
    # This fixture owns the GUID directory and synthetic contents only.
    Remove-Item -LiteralPath $directory -Recurse -Force
}
