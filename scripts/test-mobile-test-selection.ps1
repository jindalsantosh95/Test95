$ErrorActionPreference = 'Stop'
$resolver = Join-Path $PSScriptRoot 'resolve-mobile-test-selection.ps1'
$name = 'SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips'
$selection = & $resolver -TestName $name
if ($selection.Expected -ne 1 -or $selection.Pack -ne 'Targeted' -or
    $selection.Filter -cne "FullyQualifiedName=VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook.GuestWithoutSeatsBookingTests.$name&RegressionArea=SearchAndBook&Platform=iOS") { throw 'Incorrect targeted filter' }
$default = & $resolver
if ($default.Pack -ne 'Sanity' -or $default.Expected -ne 14 -or
    $default.Methods -cnotcontains 'SearchBook_TC19_FilterSelection_RecalculatesAvailabilityAcrossOtherGroups' -or
    $default.Methods -cnotcontains 'SearchBook_TC28_EditSubmittedSearch_UpdatesCriteriaAndResultsRoute') { throw 'Sanity must include all 14 scenarios, including cascading filters and search editing' }
$regression = & $resolver -Pack Regression
if ($regression.Expected -ne 54 -or $regression.Pack -ne 'Regression' -or
    $regression.Filter -cne 'Suite=Regression&RegressionArea=SearchAndBook&Platform=iOS') { throw 'Incorrect regression selection' }
$rejected = $false
try { $null = & $resolver -Pack Regression -TestName $name } catch { $rejected = $true }
if (-not $rejected) { throw 'Ambiguous pack and test accepted' }
$rejected = $false
try { $null = & $resolver -Pack FullRegression } catch { $rejected = $true }
if (-not $rejected) { throw 'Removed FullRegression option accepted' }
$workflow = Get-Content -Raw (Join-Path $PSScriptRoot '../.github/workflows/qa-mobile-search-book-sanity.yml')
if ($workflow -notmatch '(?m)^name: Automation - Mobile Search and Book\r?$' -or
    $workflow -match 'FullRegression') { throw 'Workflow name or pack choices are stale' }
foreach ($invalid in @('TC48', "$name|Suite=Regression", "$name`nfilter=anything", 'SearchBook_TC999_Unknown', 'SearchBook_TC43_Unknown')) {
    $rejected = $false
    try { $null = & $resolver -TestName $invalid } catch { $rejected = $true }
    if (-not $rejected) { throw 'Invalid selection accepted' }
}
$contract = Get-Content -Raw (Join-Path $PSScriptRoot '../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json') | ConvertFrom-Json
# Pin every supported declaring class independently of the resolver. Pack additions
# must add an expected identity, not silently pass an acceptance-only check.
$expectedClasses = @{
    TC01 = 'VirginOneWayFlightSearchTests'
    TC02 = 'PartnerOneWayFlightSearchTests'
    TC03 = 'VirginReturnMultiPassengerFlightSearchTests'
    TC04 = 'MixedCarrierReturnMultiPassengerFlightSearchTests'
    TC05 = 'SearchEntryRuleTests'
    TC07 = 'PriceSortRefinementTests'
    TC08 = 'DepartureTimeSortRefinementTests'
    TC09 = 'DurationSortRefinementTests'
    TC10 = 'SortSwitchingRefinementTests'
    TC11 = 'AirlineFilterRefinementTests'
    TC12 = 'ConnectionsFilterRefinementTests'
    TC13 = 'DepartureTimeFilterRefinementTests'
    TC14 = 'FilterSwitchingRefinementTests'
    TC15 = 'SortAndFilterPersistenceRefinementTests'
    TC16 = 'ClearFiltersRefinementTests'
    TC17 = 'RecentAirportSearchTests'
    TC18 = 'PromoCodeSearchTests'
    TC19 = 'CascadingFilterRefinementTests'
    TC20 = 'VirginCabinNamingTests'
    TC21 = 'PartnerCabinNamingTests'
    TC22 = 'VirginCabinUpgradeTests'
    TC23 = 'USOriginOneWayFlightSearchTests'
    TC24 = 'VirginFlightSummaryTests'
    TC25 = 'OtherOperatingCarrierFlightSummaryTests'
    TC26 = 'MultiCarrierConnectingFlightSummaryTests'
    TC27 = 'FlexibleDateSearchTests'
    TC28 = 'EditSubmittedSearchTests'
    TC29 = 'FareBenefitsAndBaggageTests'
    TC31 = 'DiscountCodeValidationTests'
    TC32 = 'DiscountCodeLifecycleTests'
    TC33 = 'FareDismissalTests'
    TC34 = 'PassengerRequiredFieldTests'
    TC35 = 'PassengerTermsGateTests'
    TC38 = 'SummaryChangeTests'
    TC39 = 'SummaryUpgradeTests'
    TC40 = 'ReturnSummaryChangeTests'
    TC41 = 'BackendBaggageTests'
    TC42 = 'PartnerBackendBaggageTests'
    TC43 = 'GuestSeatBookingTests'
    TC44 = 'SeatAssignmentLifecycleTests'
    TC45 = 'SeatSheetCancellationTests'
    TC46 = 'TwoPassengerSeatAssignmentTests'
    TC47 = 'SeatPriceRecalculationTests'
    TC48 = 'GuestWithoutSeatsBookingTests'
    TC49 = 'LapInfantSeatAssignmentTests'
    TC50 = 'UnsupportedCarrierSeatBypassTests'
    TC51 = 'VirginChildSeatGuidanceTests'
    TC52 = 'InboundEligibleChildSeatGuidanceTests'
    TC53 = 'UnsupportedChildSeatGuidanceBypassTests'
    TC54 = 'DeltaChildSeatGuidanceTests'
    TC55 = 'CardPresentationBookingTests'
    'LOYALTY-TC01' = 'RepresentativeMemberLoyaltyBookingTests'
    'LOYALTY-TC02' = 'InsufficientPointsRewardBookingTests'
    'LOYALTY-TC04' = 'MemberNotTravellingTests'
}
$supported = @(@($contract.releasePacks.sanity) + @($contract.releasePacks.regression) | Select-Object -Unique)
if ($supported.Count -ne $expectedClasses.Count -or @($supported | Where-Object { -not $expectedClasses.ContainsKey($_) }).Count -gt 0) {
    throw 'Update the exact-filter fixture identities to match release-pack membership.'
}
foreach ($id in $supported) {
    $method = ($contract.scenarios | Where-Object id -eq $id).testMethod
    $resolved = & $resolver -TestName $method
    $area = if ($id -like 'LOYALTY-*') { 'Account' } else { 'SearchAndBook' }
    $expected = "FullyQualifiedName=VAA.EAPI.Automation.MobileApp.UI.Tests.$area.$($expectedClasses[$id]).$method&RegressionArea=SearchAndBook&Platform=iOS"
    if ($resolved.Filter -cne $expected) { throw "Wrong declaring-class filter for $id" }
    Write-Host "PASS: exact release-pack identity $id"
}
foreach ($id in @($contract.releasePacks.excluded.PSObject.Properties.Name)) {
    $method = ($contract.scenarios | Where-Object id -eq $id).testMethod
    if (-not $method) { continue }
    $rejected = $false
    try { $null = & $resolver -TestName $method } catch { $rejected = $true }
    if (-not $rejected) { throw 'Unsupported scenario accepted' }
}
$directory = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
try {
    $trx = Join-Path $directory 'selection.trx'
    foreach ($packSelection in @($regression)) {
        foreach ($omitLast in @($false, $true)) {
            $methods = @($packSelection.Methods)
            if ($omitLast) { $methods = $methods[0..($methods.Count - 2)] }
            $rows = ($methods | ForEach-Object { "<UnitTestResult testName='Example.$_' outcome='Passed'/>" }) -join ''
            "<TestRun><Results>$rows</Results></TestRun>" | Set-Content -LiteralPath $trx
            $rejected = $false
            try { $null = & $resolver -Pack $packSelection.Pack -TrxPath $trx } catch { $rejected = $true }
            if ($rejected -ne $omitLast) { throw 'Regression completeness guard failed' }
        }
        Write-Host "PASS: $($packSelection.Pack) exact membership and missing-result guard"
    }
    foreach ($case in @('pass', 'zero', 'wrong', 'duplicate', 'failed', 'skipped', 'malformed', 'sanity-pass', 'sanity-short')) {
        $rows = "<UnitTestResult testName='Example.$name' outcome='Passed'/>"
        switch ($case) {
            'zero' { $rows = '' }
            'wrong' { $rows = "<UnitTestResult testName='Example.SearchBook_TC48_Wrong' outcome='Passed'/>" }
            'duplicate' { $rows = $rows + $rows }
            'failed' { $rows = $rows.Replace('Passed', 'Failed') }
            'skipped' { $rows = $rows.Replace('Passed', 'NotExecuted') }
            'sanity-pass' { $rows = ($default.Methods | ForEach-Object { "<UnitTestResult testName='Example.$_' outcome='Passed'/>" }) -join '' }
        }
        "<TestRun><Results>$rows</Results></TestRun>" | Set-Content -LiteralPath $trx
        if ($case -eq 'malformed') { '<broken' | Set-Content -LiteralPath $trx }
        $testName = if ($case -like 'sanity-*') { '' } else { $name }
        $rejected = $false
        try { $null = & $resolver -TestName $testName -TrxPath $trx } catch { $rejected = $true }
        if ($rejected -eq ($case -in @('pass', 'sanity-pass'))) { throw "Incorrect validation: $case" }
        Write-Host "PASS: selection evidence $case"
    }
    foreach ($case in @('pass', 'wrong')) {
        $method = if ($case -eq 'pass') { $name } else { 'SearchBook_TC48_Wrong' }
        "<TestRun><Results><UnitTestResult testName='Example.$method' outcome='Passed'/></Results></TestRun>" | Set-Content -LiteralPath $trx
        $payload = Join-Path $directory 'slack.json'
        & (Join-Path $PSScriptRoot 'publish-mobile-release-slack.ps1') -TrxPath $trx -Pack Targeted -TestName $name -JobStatus success -RunUrl 'https://github.com/VAAD1ST/eapi/actions/runs/1' -PreviewOnly -PayloadPath $payload
        $report = Get-Content -Raw -LiteralPath $payload | ConvertFrom-Json
        if (-not $report.text.Contains('Targeted') -or ($report.text.EndsWith('(Passed)') -ne ($case -eq 'pass'))) { throw 'Targeted Slack status/label incorrect' }
        Write-Host "PASS: targeted Slack $case"
    }
} finally {
    # Only synthetic files in this test-created directory.
    Remove-Item -LiteralPath $directory -Recurse -Force
}
Write-Host 'PASS: release-pack selection and input guards'
