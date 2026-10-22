$ErrorActionPreference = 'Stop'
$root = Join-Path ([IO.Path]::GetTempPath()) ('ios-coverage-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$root/Source" -Force | Out-Null
function Invoke-Git([string[]]$Arguments) {
    $result = & git -C $root @Arguments
    if ($LASTEXITCODE -ne 0) { throw 'Fixture git command failed.' }
    return $result
}
function Assert-Equal($Actual, $Expected, [string]$Message) {
    if ($Actual -ne $Expected) { throw "$Message : expected $Expected, got $Actual" }
}
try {
    Invoke-Git @('init', '-q')
    'struct Model {}' | Set-Content "$root/Source/Model.swift"
    'struct Other {}' | Set-Content "$root/Source/Other.swift"
    Invoke-Git @('add', 'Source')
    Invoke-Git @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-qm', 'fixture')
    $sha = Invoke-Git @('rev-parse', 'HEAD')
    $blob = Invoke-Git @('rev-parse', 'HEAD:Source/Model.swift')
    $tree = Invoke-Git @('rev-parse', 'HEAD:Source')
    @{ schemaVersion = 1; sourceRoot = 'Source'; matrixReviewedCommit = $sha; matrixReviewedDate = '2026-09-22'; matrixReviewedSourceTree = $tree; mappings = @(@{ path = 'Source/Model.swift'; reviewedBlob = $blob; tests = @('TC05') }) } | ConvertTo-Json -Depth 6 | Set-Content "$root/map.json"
    @('## Current component coverage', '| App component | iOS | Tests | Intent | Decision | Mapping status |', '| --- | --- | --- | --- | --- | --- |', '| Search | Model | TC05 | Search | Keep | Mapped |', '| Error | Other | None | Error | Backlog | Gap |') | Set-Content "$root/matrix.md"
    @{ scenarios = @(@{ id = 'TC05'; testMethod = 'SearchBook_TC05_Synthetic' }) } | ConvertTo-Json -Depth 6 | Set-Content "$root/contract.json"
    $args = @{ IosRepository = $root; TrxPath = "$root/run.trx"; OutputDirectory = "$root/report"; MappingPath = "$root/map.json"; ContractPath = "$root/contract.json"; MatrixPath = "$root/matrix.md" }
    foreach ($outcome in @('Passed', 'Failed', 'NotExecuted')) {
        "<TestRun><Results><UnitTestResult testName='Example.SearchBook_TC05_Synthetic' outcome='$outcome'/></Results></TestRun>" | Set-Content "$root/run.trx"
        & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args -AppCommit $sha
        $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
        Assert-Equal $report.buildAlignment 'DeclaredMatch' 'provenance'
        Assert-Equal $report.componentCoverage.percent 50 'matrix coverage, not pass rate'
        Assert-Equal $report.componentCoverage.freshness 'MatchesReviewedSource' 'reviewed tree'
        Assert-Equal $report.inventoryFiles 2 'inventory'
        Assert-Equal $report.unmappedFiles 1 'new source'
        Assert-Equal $report.passedMappings ([int]($outcome -eq 'Passed')) 'pass evidence'
        Assert-Equal $report.failedMappings ([int]($outcome -eq 'Failed')) 'failure evidence'
        Write-Host "PASS: $outcome"
    }
    'struct NewFunctionality {}' | Set-Content "$root/Source/New.swift"
    Invoke-Git @('add', 'Source')
    Invoke-Git @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-qm', 'new functionality')
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal $report.changedMappings 0 'existing file mappings remain unchanged'
    Assert-Equal $report.componentCoverage.freshness 'NeedsReview' 'new unmapped source invalidates matrix freshness'
    Write-Host 'PASS: new functionality outside seed mappings flags matrix review'
    '<TestRun><Results><UnitTestResult testName="SearchBook_TC05_Synthetic" outcome="Passed"/><UnitTestResult testName="SearchBook_TC05_Synthetic" outcome="Passed"/></Results></TestRun>' | Set-Content "$root/run.trx"
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args -AppCommit ('f' * 40)
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal $report.failedMappings 1 'duplicate results'
    Assert-Equal $report.buildAlignment 'Mismatch' 'mismatch'
    Write-Host 'PASS: duplicate results and mismatched app commit'
    'struct Model { let changed = true }' | Set-Content "$root/Source/Model.swift"
    Invoke-Git @('add', 'Source')
    Invoke-Git @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-qm', 'change')
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal $report.changedMappings 1 'changed mapping'
    Assert-Equal $report.componentCoverage.freshness 'NeedsReview' 'source change must flag entire matrix'
    Assert-Equal $report.passedMappings 0 'changed source cannot be covered'
    Write-Host 'PASS: source change invalidates mapping'
    Invoke-Git @('rm', '-q', 'Source/Model.swift')
    Invoke-Git @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-qm', 'remove')
    Remove-Item "$root/run.trx"
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal @($report.removedMappings).Count 1 'removed source'
    Assert-Equal $report.buildAlignment 'Unknown' 'unknown build'
    Write-Host 'PASS: removed source and missing TRX'
    '| New screen | New | None | New | Backlog | Gap |' | Add-Content "$root/matrix.md"
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal $report.componentCoverage.total 3 'new component denominator'
    Assert-Equal $report.componentCoverage.percent 33.3 'new uncovered functionality lowers matrix percentage'
    Write-Host 'PASS: adding an uncovered matrix component updates percentage'
    $args.IosRepository = ''
    & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args
    $report = Get-Content -Raw "$root/report/ios-coverage.json" | ConvertFrom-Json
    Assert-Equal $report.status 'Unavailable' 'missing source'
    Write-Host 'PASS: missing source is unavailable'
    $args.IosRepository = $root
    '<!DOCTYPE TestRun [<!ENTITY x SYSTEM "file:///etc/passwd">]><TestRun>&x;</TestRun>' | Set-Content "$root/run.trx"
    $rejected = $false
    try { & "$PSScriptRoot/get-mobile-ios-coverage.ps1" @args } catch { $rejected = $true }
    Assert-Equal $rejected $true 'DTD rejection'
    Write-Host 'PASS: unsafe XML rejected'
} finally {
    # Only this invocation's generated fixture directory, never a user checkout.
    Remove-Item -LiteralPath $root -Recurse -Force
}
