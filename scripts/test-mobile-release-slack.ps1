$ErrorActionPreference = 'Stop'
# Windows PowerShell 5.1 reads BOM-less scripts using the system ANSI code page.
# Keep these shared publishers ASCII-safe so UTF-8 punctuation cannot become quotes.
foreach ($name in @('publish-mobile-release-slack.ps1', 'publish-search-book-ui-slack.ps1')) {
    $bytes = [IO.File]::ReadAllBytes((Join-Path $PSScriptRoot $name))
    if (@($bytes | Where-Object { $_ -gt 127 }).Count -gt 0) {
        throw "Keep $name ASCII-safe for Windows PowerShell 5.1; construct display symbols from code points."
    }
}
Write-Host 'PASS: shared Slack publishers are Windows PowerShell encoding-safe'
$directory = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$trx = Join-Path $directory 'fixture.trx'
$payload = Join-Path $directory 'payload.json'
$originalStepSummary = $env:GITHUB_STEP_SUMMARY
$stepSummary = Join-Path $directory 'step-summary.md'
$env:GITHUB_STEP_SUMMARY = $stepSummary
$contract = Get-Content -Raw (Join-Path $PSScriptRoot '../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json') | ConvertFrom-Json
$count = @($contract.releasePacks.sanity).Count
try {
    foreach ($size in @(53, 54)) {
        $rows = ($contract.releasePacks.regression | Select-Object -First $size | ForEach-Object {
            $id = $_
            $method = ($contract.scenarios | Where-Object id -eq $id).testMethod
            "<UnitTestResult testName='Example.$method' outcome='Passed'/>"
        }) -join ''
        "<TestRun><Results>$rows</Results></TestRun>" | Set-Content $trx
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $trx -Pack Regression -JobStatus success `
            -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload
        $report = Get-Content -Raw $payload | ConvertFrom-Json
        if ($report.text.EndsWith('(Passed)') -ne ($size -eq 54)) { throw 'Regression report must require all 54 results' }
        Write-Host "PASS: Regression reporting requires 54 results (fixture $size)"
    }
    # All fixture output is isolated from the actual CI job summary, including
    # non-preview transport-policy checks below.
    foreach ($case in @('passed', 'failed', 'skipped', 'cancelled', 'missing', 'empty', 'short')) {
        $size = if ($case -eq 'short') { 1 } elseif ($case -eq 'empty') { 0 } else { $count }
        $rows = for ($index = 0; $index -lt $size; $index++) {
            $outcome = if ($index -eq 0 -and $case -eq 'failed') { 'Failed' } elseif ($index -eq 0 -and $case -eq 'skipped') { 'NotExecuted' } else { 'Passed' }
            "<UnitTestResult testName='Example.SearchBook_TC05_Synthetic' outcome='$outcome'><Output><StdOut>[confirmed-booking] PNR=ABC123; scenario=SearchBook_TC05_Synthetic`nSECRET_SENTINEL</StdOut><ErrorInfo><Message>SECRET_SENTINEL</Message></ErrorInfo></Output></UnitTestResult>"
        }
        "<TestRun xmlns='http://microsoft.com/schemas/VisualStudio/TeamTest/2010'><Results>$($rows -join '')</Results></TestRun>" | Set-Content $trx
        $inputPath = if ($case -eq 'missing') { Join-Path $directory 'missing.trx' } else { $trx }
        $status = if ($case -eq 'cancelled') { 'cancelled' } elseif ($case -eq 'failed') { 'failure' } else { 'success' }
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $inputPath -Pack Sanity -JobStatus $status `
            -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload
        $raw = Get-Content -Raw $payload
        if (Test-Path -LiteralPath $stepSummary) { throw "Preview wrote a job summary in $case" }
        $parsed = $raw | ConvertFrom-Json
        if ($raw.Contains('SECRET_SENTINEL')) { throw "Sensitive output leaked in $case" }
        $expectedStatus = if ($case -eq 'passed') { '(Passed)' } elseif ($case -eq 'failed') { '(Failed)' } else { '(Incomplete / requires review)' }
        if (-not $parsed.text.EndsWith($expectedStatus)) { throw "Incorrect status for $case" }
        if ($case -eq 'passed' -and -not $raw.Contains('ABC123')) { throw 'Confirmed booking reference missing' }
        Write-Host "PASS: $case"
    }
    'Existing summary must be preserved' | Set-Content -LiteralPath $stepSummary
    $summaryHash = (Get-FileHash -LiteralPath $stepSummary).Hash
    & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $trx -Pack Sanity -JobStatus success `
        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload
    if ((Get-FileHash -LiteralPath $stepSummary).Hash -ne $summaryHash) { throw 'Preview changed an existing job summary' }
    Write-Host 'PASS: preview neither creates nor changes a job summary'
    & {
        function Invoke-RestMethod { }
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $trx -Pack Sanity -JobStatus success `
            -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -WebhookUrl 'https://example.invalid/synthetic' -PayloadPath $payload
    }
    $summaryText = Get-Content -Raw -LiteralPath $stepSummary
    if (-not $summaryText.Contains('Existing summary must be preserved') -or -not $summaryText.Contains('### Mobile Search')) { throw 'Non-preview job summary was not appended' }
    Write-Host 'PASS: real reporting still appends the job summary (transport mocked)'
    '<invalid' | Set-Content $trx
    $rejected = $false
    try {
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $trx -Pack Sanity -JobStatus success `
            -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload
    } catch { $rejected = $true }
    if (-not $rejected) { throw 'Malformed TRX was accepted' }
    Write-Host 'PASS: malformed TRX rejected'
    $coveragePath = Join-Path $directory 'coverage.json'
    @{
        schemaVersion = 1; status = 'Available'; sourceCommit = ('a' * 40); buildAlignment = 'Mismatch'
        inventoryFiles = 20; reviewedMappings = 3; changedMappings = 2; unmappedFiles = 15
        passedMappings = 1; failedMappings = 1; notRunMappings = 1; removedMappings = @()
        componentCoverage = @{ total = 20; mapped = 19; partial = 3; gaps = 1; freshness = 'NeedsReview'; reviewedCommit = ('b' * 40); reviewedDate = '2026-09-22' }
    } | ConvertTo-Json -Depth 5 | Set-Content $coveragePath
    & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath (Join-Path $directory 'missing.trx') -Pack Sanity -JobStatus failure `
        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload -CoveragePath $coveragePath
    $raw = Get-Content -Raw $payload
    if (-not $raw.Contains('Mismatch') -or -not $raw.Contains('19/20 components mapped (95%)') -or -not $raw.Contains('NeedsReview') -or $raw.Contains('20 Swift files')) { throw 'Dynamic component coverage summary missing or misleading.' }
    Write-Host 'PASS: dynamic coverage payload and mismatch warning'
    # Use the maintained matrix as well as synthetic counts: document changes must reach Slack.
    & (Join-Path $PSScriptRoot 'get-mobile-ios-coverage.ps1') -TrxPath (Join-Path $directory 'missing.trx') -OutputDirectory $directory
    $matrixReport = Get-Content -Raw (Join-Path $directory 'ios-coverage.json') | ConvertFrom-Json
    & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath (Join-Path $directory 'missing.trx') -Pack Sanity -JobStatus failure `
        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload -CoveragePath (Join-Path $directory 'ios-coverage.json')
    $raw = Get-Content -Raw $payload
    $component = $matrixReport.componentCoverage
    if (-not $raw.Contains("Breakdown: $($component.mapped - $component.partial) mapped, $($component.partial) partial, $($component.gaps) gap.")) { throw 'Matrix breakdown missing' }
    foreach ($area in @($component.components | Where-Object status -ne 'Mapped')) {
        if (-not $raw.Contains($area.name.Replace('&', '&amp;'))) { throw 'Partial/gap area missing from Slack' }
    }
    Write-Host 'PASS: maintained matrix breakdown and partial/gap areas reach Slack'
    $component.components[0].name = '<!channel>'
    $matrixReport | ConvertTo-Json -Depth 8 | Set-Content $coveragePath
    $rejected = $false
    try {
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath (Join-Path $directory 'missing.trx') -Pack Sanity -JobStatus failure `
            -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload -CoveragePath $coveragePath
    } catch { $rejected = $true }
    if (-not $rejected) { throw 'Unsafe component heading was accepted' }
    Write-Host 'PASS: unsafe component heading rejected'
    if ($raw.Contains('AI-proposed') -or $raw.Contains('Latest develop assessment')) { throw 'Deferred AI assessment must not appear in Slack' }
    Write-Host 'PASS: matrix-only reporting has no AI assessment dependency'
    # Shadow the transport in this scope: no real webhook or network call is made.
    & {
        function Invoke-RestMethod { throw 'SECRET_TRANSPORT_SENTINEL' }
        foreach ($webhook in @('', 'https://example.invalid/synthetic')) {
            foreach ($mobile in @($false, $true)) {
                $rejected = $false
                $warnings = @()
                try {
                    & (Join-Path $PSScriptRoot 'publish-search-book-ui-slack.ps1') -ArtifactRoot $directory `
                        -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -WebhookUrl $webhook `
                        -MobileReport:$mobile -WarningVariable warnings
                } catch {
                    $rejected = $true
                    if ($_.ToString().Contains('SECRET_TRANSPORT_SENTINEL')) { throw 'Transport details leaked' }
                }
                if ($rejected -ne $mobile) { throw 'Slack strictness changed for an existing caller' }
                if (-not $mobile -and @($warnings).Count -eq 0) { throw 'Optional notification failure must warn' }
                if (($warnings -join '').Contains('SECRET_TRANSPORT_SENTINEL')) { throw 'Transport warning leaked details' }
                Write-Host "PASS: notification failure policy mobile=$mobile configured=$([bool]$webhook)"
            }
        }
    }
} finally {
    $env:GITHUB_STEP_SUMMARY = $originalStepSummary
    foreach ($path in @($trx, $payload, $stepSummary, (Join-Path $directory 'coverage.json'), (Join-Path $directory 'ios-coverage.json'), (Join-Path $directory 'ios-coverage.md'))) { Remove-Item -LiteralPath $path -ErrorAction SilentlyContinue }
    Remove-Item -LiteralPath $directory
}
