param(
    [Parameter(Mandatory = $false)]
    [bool]$IncludeJourneys = $true,

    [Parameter(Mandatory = $false)]
    [bool]$IncludeLoggedIn = $true
)

$profiles = @(
    [ordered]@{ profile = "windows-chrome"; os = "Windows"; osVersion = "11"; browser = "chrome"; browserVersion = "latest"; device = "" },
    [ordered]@{ profile = "windows-edge"; os = "Windows"; osVersion = "11"; browser = "edge"; browserVersion = "latest"; device = "" },
    [ordered]@{ profile = "mac-safari"; os = "OS X"; osVersion = "Tahoe"; browser = "playwright-webkit"; browserVersion = ""; device = "" }
)

$testNamespace = "VAA.EAPI.Automation.SearchAndBook.UI.Tests"
$definitions = @(
    @{ scenario = "One-way Economy Light and Upper Class"; method = "$testNamespace.Journeys.GuestJourneyTests.OneWay_VirginFare_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "One-way Premium paid seat"; method = "$testNamespace.Journeys.GuestJourneyTests.OneWay_Premium_PaidSeat_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "One-way Upper Class seat-selection flow"; method = "$testNamespace.Journeys.GuestJourneyTests.OneWay_UpperClass_AvailableSeat_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "Return Economy Light and Upper Class"; method = "$testNamespace.Journeys.GuestJourneyTests.Return_VirginFare_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "Return Premium paid seats"; method = "$testNamespace.Journeys.GuestJourneyTests.Return_Premium_PaidSeat_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "Return Upper Class per-leg seat selection"; method = "$testNamespace.Journeys.GuestJourneyTests.Return_UpperClass_PerLegSeatSelection_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "One-way and return multi-passenger"; method = "$testNamespace.Journeys.GuestJourneyTests.EconomyLight_TwoAdultsChildInfant_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "Point-of-sale currencies and insurance"; method = "$testNamespace.Journeys.GuestJourneyTests.PointOfSaleCurrency_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "One-way and return Delta-operated"; method = "$testNamespace.Journeys.GuestJourneyTests.DeltaOperated_GuestCardJourney_CompletesBooking"; completeBooking = $true },
    @{ scenario = "US cardholder offer"; method = "$testNamespace.Journeys.GuestJourneyTests.OneWay_UsCardholderOffer_OpensSynchronyApplication" },
    @{ scenario = "Summary upgrade"; method = "$testNamespace.Journeys.SummaryJourneyTests.EconomyLight_SummaryUpgrade_ReachesHppWithUpgradedPrice" },
    @{ scenario = "Adult DOB boundary"; method = "$testNamespace.Validation.PassengerValidationTests.AdultDob_EnforcesAgeBoundary" },
    @{ scenario = "Teen DOB range"; method = "$testNamespace.Validation.PassengerValidationTests.TeenDob_EnforcesAgeRange" },
    @{ scenario = "Child DOB range"; method = "$testNamespace.Validation.PassengerValidationTests.ChildDob_EnforcesAgeRange" },
    @{ scenario = "Infant DOB range"; method = "$testNamespace.Validation.PassengerValidationTests.InfantDob_EnforcesAgeRange" },
    @{ scenario = "Valid passenger submit"; method = "$testNamespace.Validation.PassengerValidationTests.ValidPassengerForm_SubmitIsEnabled" },
    @{ scenario = "Passenger loyalty number"; method = "$testNamespace.Validation.PassengerValidationTests.Passenger_CanAddVirginLoyaltyAccount" },
    @{ scenario = "Adult month range"; method = "$testNamespace.Validation.PassengerValidationTests.AdultYear2000_AllowsJanuaryAndDecember" },
    @{ scenario = "Leap-year day range"; method = "$testNamespace.Validation.PassengerValidationTests.LeapYearFebruary_AllowsDay29Not30" },
    @{ scenario = "Non-leap-year day range"; method = "$testNamespace.Validation.PassengerValidationTests.NonLeapYearFebruary_AllowsDay28Not29" },
    @{ scenario = "Thirty-day month correction"; method = "$testNamespace.Validation.PassengerValidationTests.April_RemovesPreviouslySelectedDay31" },
    @{ scenario = "Logged-in multi-passenger"; method = "$testNamespace.Journeys.GuestJourneyTests.LoggedIn_EconomyLight_TwoAdultsChildInfant_CompletesBooking"; requiresLoyalty = $true },
    @{ scenario = "Return reward multi-passenger"; method = "$testNamespace.Journeys.GuestJourneyTests.ReturnReward_EconomyClassic_TwoAdultsChildInfant_CompletesBooking"; requiresLoyalty = $true },
    @{ scenario = "Partial Pay with Points"; method = "$testNamespace.Journeys.GuestJourneyTests.Return_Premium_TwoAdults_PartialPayWithPoints_CompletesBooking"; requiresLoyalty = $true },
    @{ scenario = "Insufficient reward points"; method = "$testNamespace.Journeys.SummaryJourneyTests.RewardSearch_WithInsufficientPoints_BlocksPassengerContinue"; requiresLoyalty = $true }
    # Voucher journeys intentionally remain out of this STG/UAT matrix until the frontend changes are deployed there.
)

$include = @()
if ($IncludeJourneys) {
    $selected = @($definitions | Where-Object { $IncludeLoggedIn -or -not $_.requiresLoyalty })
    for ($index = 0; $index -lt $selected.Count; $index++) {
        $definition = $selected[$index]
        $profile = $profiles[$index % $profiles.Count]
        $completeBooking = [bool]$definition.completeBooking -and -not [bool]$definition.requiresLoyalty
        $include += [ordered]@{
            index = $index + 1
            scenario = $definition.scenario
            filter = "FullyQualifiedName=$($definition.method)"
            requiresLoyalty = [bool]$definition.requiresLoyalty
            completeBooking = $completeBooking.ToString().ToLowerInvariant()
            stopAtHpp = (-not $completeBooking).ToString().ToLowerInvariant()
            boundary = if ($completeBooking) { "confirmation" } else { "purpose-specific/HPP" }
            profile = $profile.profile
            os = $profile.os
            osVersion = $profile.osVersion
            browser = $profile.browser
            browserVersion = $profile.browserVersion
            device = $profile.device
        }
    }
}

$matrix = @{ include = $include }
$matrixJson = $matrix | ConvertTo-Json -Depth 5 -Compress

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_OUTPUT)) {
    "matrix=$matrixJson" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append
    "count=$($include.Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append
}

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    $lines = @(
        "### Search & Book journey shards",
        "",
        "Shards: $($include.Count)",
        "Maximum workflow parallelism: 10",
        "",
        "| # | Profile | Loyalty | Boundary | Scenario |",
        "| -: | --- | --- | --- | --- |"
    )
    $lines += $include | ForEach-Object {
        "| $($_.index) | $($_.profile) | $($_.requiresLoyalty) | $($_.boundary) | $($_.scenario) |"
    }
    $lines -join "`n" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}

$matrixJson
