[CmdletBinding()]
param(
    [string]$IosRepository,
    [ValidatePattern('^[A-Za-z0-9_./-]+$')][string]$IosRef = 'HEAD',
    [ValidatePattern('^([a-fA-F0-9]{40})?$')][string]$AppCommit = '',
    [Parameter(Mandatory)][string]$TrxPath,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$MappingPath = "$PSScriptRoot/../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/Contracts/ios-source-coverage.json",
    [string]$MatrixPath = "$PSScriptRoot/../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/E2E-COVERAGE-MATRIX.md",
    [string]$ContractPath = "$PSScriptRoot/../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json"
)
$ErrorActionPreference = 'Stop'
# Never leave a previous successful report behind if this scan fails validation.
foreach ($name in @('ios-coverage.json', 'ios-coverage.md')) {
    $oldReport = Join-Path $OutputDirectory $name
    if (Test-Path -LiteralPath $oldReport -PathType Leaf) { Remove-Item -LiteralPath $oldReport }
}
$mapping = Get-Content -Raw -LiteralPath $MappingPath | ConvertFrom-Json
$matrix = Get-Content -Raw -LiteralPath $MatrixPath
$section = [regex]::Match($matrix, '(?ms)^## Current component coverage\r?\n(.*?)(?=^## |\z)')
if (-not $section.Success) { throw 'Component coverage section is missing.' }
$components = @()
foreach ($line in ($section.Groups[1].Value -split '\r?\n')) {
    if (-not $line.StartsWith('|') -or $line.StartsWith('| ---') -or $line.StartsWith('| App component')) { continue }
    $cells = @($line.Trim('|').Split('|') | ForEach-Object { $_.Trim() })
    if ($cells.Count -ne 6 -or $cells[5] -notin @('Mapped', 'Partial', 'Gap')) { throw 'Each component row requires an explicit mapping status.' }
    $components += @{ name = $cells[0]; status = $cells[5]; tests = $cells[2]; detail = $cells[4] }
}
if ($components.Count -eq 0) { throw 'Empty component matrix cannot produce coverage.' }
$mappedCount = @($components | Where-Object status -ne 'Gap').Count
$contract = Get-Content -Raw -LiteralPath $ContractPath | ConvertFrom-Json
$methods = @{}
foreach ($scenario in $contract.scenarios) { $methods[$scenario.id] = $scenario.testMethod }
$outcomes = @{}
if (Test-Path -LiteralPath $TrxPath -PathType Leaf) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create((Resolve-Path $TrxPath).Path, $settings)
    try {
        $xml = [System.Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally { $reader.Dispose() }
    foreach ($result in $xml.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']")) {
        $name = ([string]$result.testName -split '\.')[-1]
        # Duplicate/retry evidence is ambiguous, never choose the passing attempt.
        $outcomes[$name] = if ($outcomes.ContainsKey($name)) { 'Ambiguous' } else { [string]$result.outcome }
    }
}
$report = [ordered]@{
    schemaVersion = 1; status = 'Unavailable'; sourceCommit = ''; appCommit = $AppCommit
    buildAlignment = 'Unknown'; inventoryFiles = 0; reviewedMappings = 0; changedMappings = 0
    unmappedFiles = 0; passedMappings = 0; failedMappings = 0; notRunMappings = 0
    limitation = 'Representative source-file mappings, not Swift line/branch coverage or proof of every user-facing behaviour. Passing scenarios are whole-test evidence only.'
    files = @(); removedMappings = @()
    componentCoverage = [ordered]@{
        total = $components.Count; mapped = $mappedCount
        partial = @($components | Where-Object status -eq 'Partial').Count
        gaps = @($components | Where-Object status -eq 'Gap').Count
        percent = [math]::Round(100.0 * $mappedCount / $components.Count, 1)
        freshness = 'Unknown'; reviewedCommit = $mapping.matrixReviewedCommit
        reviewedDate = $mapping.matrixReviewedDate; components = $components
    }
}
if ($IosRepository -and (Test-Path -LiteralPath $IosRepository -PathType Container)) {
    $sha = & git -C $IosRepository rev-parse --verify --end-of-options "${IosRef}^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0 -or "$sha" -notmatch '^[a-f0-9]{40}$') { throw 'Cannot resolve the requested iOS source commit.' }
    # Read the committed tree only: uncommitted local app edits cannot silently enter the report.
    $tree = @(& git -C $IosRepository ls-tree -r $sha -- $mapping.sourceRoot)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inventory the requested iOS source tree.' }
    $files = @{}
    foreach ($line in $tree) {
        if ($line -match '^100[0-9]{3} blob ([a-f0-9]{40})\t(.+\.swift)$') { $files[$Matches[2]] = $Matches[1] }
    }
    if ($files.Count -eq 0) { throw 'No Search and Book Swift source files found; coverage scope may have moved.' }
    $entries = @{}
    foreach ($entry in $mapping.mappings) {
        if ($entries.ContainsKey($entry.path)) { throw 'Duplicate source mapping.' }
        if (@($entry.tests).Count -eq 0 -or $entry.reviewedBlob -notmatch '^[a-f0-9]{40}$') { throw 'Invalid source mapping review metadata.' }
        foreach ($id in $entry.tests) { if (-not $methods.ContainsKey($id)) { throw "Unknown scenario in source mapping: $id" } }
        $entries[$entry.path] = $entry
    }
    $report.status = 'Available'
    $sourceTree = & git -C $IosRepository rev-parse "${sha}:$($mapping.sourceRoot)"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve Search and Book source tree.' }
    # The complete subtree catches additions and changes even outside the four legacy file mappings.
    $report.componentCoverage.freshness = if ($mapping.matrixReviewedSourceTree -and $sourceTree -eq $mapping.matrixReviewedSourceTree) { 'MatchesReviewedSource' } else { 'NeedsReview' }
    $report.sourceCommit = "$sha"
    $report.buildAlignment = if (-not $AppCommit) { 'Unknown' } elseif ($AppCommit -eq $sha) { 'DeclaredMatch' } else { 'Mismatch' }
    foreach ($path in ($files.Keys | Sort-Object)) {
        $entry = $entries[$path]
        $state = if (-not $entry) { 'Unmapped' } elseif ($entry.reviewedBlob -ne $files[$path]) { 'ChangedNeedsReview' } else { 'ReviewedMapping' }
        $runState = 'NotRun'
        if ($state -eq 'ReviewedMapping') {
            $results = @($entry.tests | ForEach-Object { if ($outcomes.ContainsKey($methods[$_])) { $outcomes[$methods[$_]] } else { 'NotRun' } })
            $runState = if (@($results | Where-Object { $_ -notin @('Passed', 'NotRun', 'NotExecuted') }).Count) { 'FailedOrAmbiguous' } elseif (@($results | Where-Object { $_ -ne 'Passed' }).Count) { 'NotRunOrSkipped' } else { 'Passed' }
        }
        $report.files += [ordered]@{ path = $path; blob = $files[$path]; mapping = $state; tests = @($entry.tests | Where-Object { $_ }); run = $runState }
    }
    $report.removedMappings = @($entries.Keys | Where-Object { -not $files.ContainsKey($_) } | Sort-Object)
    $report.inventoryFiles = $files.Count
    $report.reviewedMappings = @($report.files | Where-Object mapping -eq 'ReviewedMapping').Count
    $report.changedMappings = @($report.files | Where-Object mapping -eq 'ChangedNeedsReview').Count
    $report.unmappedFiles = @($report.files | Where-Object mapping -eq 'Unmapped').Count
    $report.passedMappings = @($report.files | Where-Object run -eq 'Passed').Count
    $report.failedMappings = @($report.files | Where-Object run -eq 'FailedOrAmbiguous').Count
    $report.notRunMappings = $report.reviewedMappings - $report.passedMappings - $report.failedMappings
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ios-coverage.json') -Encoding utf8
$lines = @('# iOS Search and Book mapped functional coverage', '', $report.limitation, '',
    "Documented component mapping: $($report.componentCoverage.mapped)/$($report.componentCoverage.total) ($($report.componentCoverage.percent)%); partial: $($report.componentCoverage.partial); gaps: $($report.componentCoverage.gaps).",
    "Matrix source freshness: $($report.componentCoverage.freshness). Reviewed $($report.componentCoverage.reviewedDate), commit $($report.componentCoverage.reviewedCommit).",
    'The percentage includes partial mappings and is documented mapping breadth, not this run passing or measured Swift coverage. Source changes require a human/agent impact review; they are not automatically classified as new functionality.', '',
    "Source commit: $($report.sourceCommit). Status: $($report.status). Build alignment: $($report.buildAlignment).",
    'App commit is an operator declaration, not build provenance verification. Missing source is unavailable, never 0% or 100%.', '',
    "Inventory: $($report.inventoryFiles) Swift files; reviewed mappings: $($report.reviewedMappings); changed: $($report.changedMappings); unmapped: $($report.unmappedFiles).", '',
    '| Source file | Mapping | Scenario IDs | Current-run evidence |', '| --- | --- | --- | --- |')
foreach ($file in $report.files) {
    $safePath = $file.path.Replace('|', '\|').Replace('<', '&lt;').Replace('>', '&gt;')
    $lines += "| $safePath | $($file.mapping) | $($file.tests -join ', ') | $($file.run) |"
}
$lines += @('', 'Removed mappings require review:', ($report.removedMappings -join ', '))
$lines += @('', '## Existing user-facing component matrix', '', '| Component | Mapping | Scenarios | Notes |', '| --- | --- | --- | --- |')
foreach ($component in $components) {
    $lines += "| $($component.name) | $($component.status) | $($component.tests) | $($component.detail) |"
}
$lines | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ios-coverage.md') -Encoding utf8
Write-Host "iOS source coverage: $($report.status); $($report.inventoryFiles) files; $($report.changedMappings) changed mappings."
