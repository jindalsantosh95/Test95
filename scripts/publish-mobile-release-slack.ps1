[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$TrxPath,
    [Parameter(Mandatory)][ValidateSet('Sanity', 'Regression', 'Targeted')][string]$Pack,
    [string]$TestName = '',
    [Parameter(Mandatory)][ValidateSet('success', 'failure', 'cancelled')][string]$JobStatus,
    [Parameter(Mandatory)][ValidatePattern('^https://github\.com/VAAD1ST/eapi/actions/runs/[0-9]+$')][string]$RunUrl,
    [ValidateSet('dev', 'sit', 'uat', 'stg')][string]$TargetEnvironment = 'sit',
    [string]$WebhookUrl = $env:SLACK_WEBHOOK_URL,
    [string]$PayloadPath,
    [string]$CoveragePath,
    [string]$AppSelectionPath,
    [switch]$PreviewOnly
)

$ErrorActionPreference = 'Stop'
$contract = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json') | ConvertFrom-Json
# Resolve the expected count from the release contract rather than duplicating pack membership.
$expected = switch ($Pack) {
    'Sanity' { @($contract.releasePacks.sanity).Count }
    'Regression' { @($contract.releasePacks.regression).Count }
    'Targeted' {
        if ([string]::IsNullOrEmpty($TestName)) { throw 'Targeted reporting requires an exact test name.' }
        $selection = & (Join-Path $PSScriptRoot 'resolve-mobile-test-selection.ps1') -TestName $TestName
        $selection.Expected
    }
}
if ($expected -le 0) { throw 'Release pack membership could not be resolved.' }
$summary = [ordered]@{ total = 0; passed = 0; failed = 0; skipped = 0; failures = @(); bookings = @() }
if (Test-Path -LiteralPath $TrxPath -PathType Leaf) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create((Resolve-Path -LiteralPath $TrxPath).Path, $settings)
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
    } finally { $reader.Dispose() }
    $results = @($document.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']"))
    $summary.total = $results.Count
    foreach ($result in $results) {
        $scenarioMatch = [regex]::Match([string]$result.testName, '(?:^|\.)(?<name>(?:Loyalty)?SearchBook_TC[0-9]+_[A-Za-z0-9_]+)$')
        $scenarioName = if ($scenarioMatch.Success) { $scenarioMatch.Groups['name'].Value } else { 'Unrecognised scenario (see restricted TRX)' }
        if ($result.outcome -eq 'Passed') {
            $stdoutNode = $result.SelectSingleNode("*[local-name()='Output']/*[local-name()='StdOut']")
            $stdout = if ($null -ne $stdoutNode) { $stdoutNode.InnerText } else { '' }
            # Use the shared marker and bind it to this TRX scenario; never copy free-form output.
            $bookingPattern = '(?m)^\[confirmed-booking\] PNR=(?<pnr>[A-Z0-9]{6}); scenario=' + [regex]::Escape($scenarioName) + '\r?$'
            foreach ($bookingMatch in [regex]::Matches($stdout, $bookingPattern)) {
                $summary.bookings += @{ bookingReference = $bookingMatch.Groups['pnr'].Value; scenario = $scenarioName; surname = '' }
            }
        }
        switch ([string]$result.outcome) {
            'Passed' { $summary.passed++ }
            'NotExecuted' { $summary.skipped++ }
            default {
                $summary.failed++
                # Only numbered method identifiers: never theory arguments, error messages or stdout.
                $summary.failures += @{ name = $scenarioName; bucket = 'See workflow diagnostics; cause not inferred' }
            }
        }
    }
} else {
    Write-Warning 'No TRX produced; reporting incomplete run, not a pass.'
}
$temporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
# A passing unrelated/duplicate result must never make a targeted run green.
if ($Pack -eq 'Targeted' -and (Test-Path -LiteralPath $TrxPath -PathType Leaf)) {
    try { $null = & (Join-Path $PSScriptRoot 'resolve-mobile-test-selection.ps1') -TestName $TestName -TrxPath $TrxPath }
    catch { $JobStatus = 'failure' }
}
$reportLabel = if ($Pack -eq 'Targeted') { "Targeted - $TestName" } else { $Pack }
$coverageText = 'iOS mapped functional coverage: unavailable. No current source report was produced.'
$coverage = $null
if ($CoveragePath -and (Test-Path -LiteralPath $CoveragePath -PathType Leaf)) {
    $coverage = Get-Content -Raw -LiteralPath $CoveragePath | ConvertFrom-Json
    if ($coverage.schemaVersion -ne 1) { throw 'Unsupported iOS coverage report.' }
    if ($coverage.componentCoverage) {
        $component = $coverage.componentCoverage
        foreach ($field in @('total', 'mapped', 'partial', 'gaps')) {
            if ([string]$component.$field -notmatch '^[0-9]{1,6}$') { throw 'Invalid component coverage count.' }
        }
        if ($component.total -le 0 -or $component.mapped + $component.gaps -ne $component.total -or $component.partial -gt $component.mapped -or
            $component.freshness -notin @('Unknown', 'MatchesReviewedSource', 'NeedsReview') -or
            $component.reviewedCommit -notmatch '^[a-f0-9]{40}$' -or $component.reviewedDate -notmatch '^\d{4}-\d{2}-\d{2}$') { throw 'Invalid component matrix metadata.' }
        $percent = [math]::Round(100.0 * $component.mapped / $component.total, 1)
        $coverageText = "iOS user-facing component coverage (documented matrix)`n$($component.mapped)/$($component.total) components mapped ($percent%), including $($component.partial) partial; gaps: $($component.gaps).`nMatrix freshness: $($component.freshness). Reviewed $($component.reviewedDate), iOS $($component.reviewedCommit.Substring(0,12)).`nPercentage describes representative mapping, not exhaustive functionality, this run's pass rate or Swift code coverage.`nSource/build comparison: unavailable. See ios-coverage.md and the linked matrix."
        $coverageText += "`nBreakdown: $($component.mapped - $component.partial) mapped, $($component.partial) partial, $($component.gaps) gap."
        # Only matrix component headings, never free-form failure details or passenger data.
        if ($component.components) {
            $areas = @($component.components)
            if ($areas.Count -ne $component.total -or
                @($areas | Where-Object { $_.status -eq 'Partial' }).Count -ne $component.partial -or
                @($areas | Where-Object { $_.status -eq 'Gap' }).Count -ne $component.gaps) { throw 'Component detail counts do not match coverage totals.' }
            foreach ($area in $areas) {
                if ($area.status -notin @('Mapped', 'Partial', 'Gap') -or
                    $area.name -cnotmatch '\A[A-Za-z][A-Za-z0-9 /(),&-]{0,99}\z') { throw 'Invalid component summary heading.' }
            }
            foreach ($state in @('Partial', 'Gap')) {
                $names = @($areas | Where-Object status -eq $state | ForEach-Object { $_.name.Replace('&', '&amp;') })
                if ($names.Count) { $coverageText += "`n${state} areas: $($names -join '; ')." }
            }
        }
    }
    if ($coverage.status -eq 'Available') {
        if ($coverage.sourceCommit -notmatch '^[a-f0-9]{40}$' -or $coverage.buildAlignment -notin @('Unknown', 'DeclaredMatch', 'Mismatch')) { throw 'Invalid iOS coverage provenance.' }
        foreach ($field in @('inventoryFiles', 'reviewedMappings', 'changedMappings', 'unmappedFiles', 'passedMappings', 'failedMappings', 'notRunMappings')) {
            if ([string]$coverage.$field -notmatch '^[0-9]{1,6}$') { throw 'Invalid iOS coverage count.' }
        }
        $coverageText = $coverageText.Replace('Source/build comparison: unavailable.', "Scanned iOS: $($coverage.sourceCommit.Substring(0,12)); build alignment: $($coverage.buildAlignment) (operator-declared).")
    }
}
$appText = 'App build: unresolved (see workflow setup).'
if ($AppSelectionPath -and (Test-Path -LiteralPath $AppSelectionPath -PathType Leaf)) {
    $app = Get-Content -Raw -LiteralPath $AppSelectionPath | ConvertFrom-Json
    if ($app.schemaVersion -ne 1 -or $app.mode -notin @('Explicit', 'LatestSitIos') -or
        $app.appUrl -cnotmatch '\Abs://[a-f0-9]{40}\z') { throw 'Invalid app selection report.' }
    $appText = "App build ($($app.mode)): $($app.appUrl)"
    if ($app.mode -eq 'LatestSitIos') {
        if ($app.name -cnotmatch '\AVirginAtlantic-sit-debug-v[0-9]+(?:\.[0-9]+){1,3}-[0-9]+-[0-9]{8}-[0-9]{4}\.ipa\z' -or
            $app.version -cnotmatch '\A[0-9]+(?:\.[0-9]+){1,3}\z' -or
            $app.uploadedAt -cnotmatch '\A[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}Z\z') { throw 'Invalid app selection metadata.' }
        $appText += "`n$($app.name); version $($app.version); uploaded $($app.uploadedAt)"
    } else { $appText += "`nExplicit override; upload metadata and source commit not verified." }
}
$coverageText = "$appText`n`n$coverageText"
New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
try {
    $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $temporaryDirectory 'run-summary.json')
    & (Join-Path $PSScriptRoot 'publish-search-book-ui-slack.ps1') -ArtifactRoot $temporaryDirectory `
        -WebhookUrl $WebhookUrl -RunUrl $RunUrl -WorkflowName "Mobile Search & Book - $reportLabel (iOS / BrowserStack)" `
        -TargetEnvironment $TargetEnvironment -MobileReport -ExpectedTotal $expected -JobStatus $JobStatus `
        -PayloadPath $PayloadPath -PreviewOnly:$PreviewOnly -MobileCoverageText $coverageText
} finally {
    Remove-Item -LiteralPath (Join-Path $temporaryDirectory 'run-summary.json') -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $temporaryDirectory
}
