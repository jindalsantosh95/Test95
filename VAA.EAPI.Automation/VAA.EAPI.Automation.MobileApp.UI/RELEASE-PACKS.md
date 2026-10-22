# Search and Book release sign-off packs

**13 October compatibility warning:** latest reviewed iOS develop removed the insufficient-points
warning/Continue restriction expected by Loyalty TC02. It remains in Sanity/Regression intentionally;
resolve the requirement with the app/product owner before sign-off, rather than excluding the failure.
Confirmation count/name assertions have been updated and need live qualification on the new build.
See [the source impact review](IOS-DEVELOP-REVIEW-20261013.md). Pack counts below are unchanged.

## Selection policy — 16 October 2026

**19 October update:** TC23 is now a full US-origin/USD booking through confirmation and
Trips, not a search-only price check. TC43's existing full booking with a selected seat is
enrolled in both packs. Loyalty TC01 already completes logged-in reward redemption through
payment, confirmation and Trips. No live journeys were rerun for this membership change;
TC23's expanded checkout and TC43's historical payment issue still require qualification.
TC43 verifies assignment before payment and a completed booking, not authoritative
post-payment seat fulfilment for every traveller/sector.

Use the existing Suite and RegressionArea traits. Sanity is a subset of Regression, not a separate set of journeys. Each scenario keeps its own concrete class and isolated session. No assertion, selector, retry policy or app code was changed to enroll these tests.

| Pack | Scenarios | Purpose |
| --- | ---: | --- |
| Sanity | 14 | Representative checks, including four complete booking journeys: UK no-seat, US/USD, seats and logged-in redemption |
| Regression | 54 | All 52 enrolled UI scenarios plus two backend-to-UI comparisons, including every Sanity check |

**20 October update:** TC55 is enrolled after a local pass through LOS card-presentation
verification, consent, confirmation and Trips (approximately 4m48s including lifecycle).
TC52/TC53 route/fare corrections also passed locally. These are individual local results,
not a fresh full-pack or BrowserStack pass. TC55 creates a test booking/payment and depends
on short-notice LHR–LOS inventory and backend fraud eligibility. Cancellation is not covered.

These are enrolled tests, not 54 freshly passing tests. Local evidence exists for TC44–50. TC51–54 are enrolled so the known child-guidance accessibility defect remains visible in release results; TC43 is now included with its historical payment issue remaining visible. Repeated-run stability and BrowserStack qualification remain pending. TC48 passed on main locally on 12 October in 4m11s through payment, confirmation and Trips. Full regression must run against the actual release candidate before sign-off. TC01 failed local requalification on 12 October at Terms navigation after all five passengers were saved; a native-swipe comparison also stalled. It remains a visible failing regression scenario, not an exclusion or a sign-off pass.

Sanity deliberately samples search-entry validation (TC05), combined filter/sort (TC15), cascading filter availability (TC19), connecting/partner Summary (TC26), flexible dates (TC27), editing submitted search criteria/results (TC28), fare/baggage retention (TC29), passenger Terms (TC35), seat pricing/replacement/removal (TC47), complete guest no-seat booking (TC48), US-origin USD booking (TC23), booking with an assigned seat (TC43), complete redemption (Loyalty TC01), and insufficient-points blocking (Loyalty TC02). It is not a substitute for Regression.

## Run the packs

Run from the VAA.EAPI.Automation folder. First configure the gitignored MobileApp.UI/local.settings.local.json using the project example: correct RunMode, Platform=iOS, app build, device, backend environment and account-storage credentials. Never put secrets into commands or evidence. For Remote, confirm the current uploaded BrowserStack app ID and project Virgin Atlantic Mobile - Search and Book; the previously configured upload has not yet been verified as current. For TC41/42 also set BackendValidation.Enabled=true and verify API connectivity from the runner itself (BrowserStack connectivity alone is insufficient).

These packs create test bookings/payments and redemption consumes test points. Use only the approved test environment and adequate leased accounts. Promo TC18 additionally requires a currently valid HEMAN May return MAN–JFK offer; expired/missing promotion data is a blocker, not a reason to weaken assertions or silently skip it. Seat pricing needs differently priced offered seats; partner tests need eligible partner inventory.

Choose ONE filter:

~~~text
Sanity:          Suite=Sanity&RegressionArea=SearchAndBook&Platform=iOS
Regression:      Suite=Regression&RegressionArea=SearchAndBook&Platform=iOS
~~~

Example full release run (replace the run folder with a unique release/build identifier every time):

~~~sh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter 'Suite=Regression&RegressionArea=SearchAndBook&Platform=iOS' --results-directory TestResults/release-candidate-unique --logger 'trx;LogFileName=regression.trx'
pwsh -NoProfile -File ../scripts/analyse-test-run-results.ps1 -TrxPath TestResults/release-candidate-unique/regression.trx -SuiteName SearchAndBook-Regression -EnvironmentName SIT
~~~

Append --list-tests to the dotnet command to preview selection without opening an app session. Expected discovery: 14 Sanity, 54 Regression. Existing xUnit configuration permits four BrowserStack classes concurrently; the local-session gate serializes a single simulator. Account leasing remains in the existing harness. No shared-session ordering is introduced.

## Sign-off evidence and guardrails

1. Record app commit/build/upload, automation commit, environment, device/OS, filter, run time and BrowserStack build/session links where applicable.
2. Require the expected number of tests to execute, with zero failed, skipped, missing or aborted tests. A zero-exit command alone is not sufficient sign-off. Analyse only this run's TRX and retain failure diagnostics.
3. Record exclusions below and all open relevant app defects in the release decision. An accepted exclusion requires the release owner's explicit risk acceptance; a green pack does not approve excluded functionality.
4. For fixes, rerun affected tests and retain the original failure. Do not retry payments or hide intermittent failures behind repeated green attempts.
5. Requalify on the target BrowserStack build/device before claiming remote readiness. Local passes do not establish Android or remote coverage.

The contract JSON releasePacks section pins membership by stable scenario ID. Offline ReleasePackContractTests compare it against discovered xUnit traits, require independent non-skipped scenarios, enforce Sanity as a subset, preserve backend opt-in, and require every numbered SearchBook/LoyaltySearchBook scenario to be enrolled or explicitly excluded. When adding tests, update traits, the contract, this inventory and coverage mapping together. Existing My Account, OCI, visual and accessibility suites remain separate; these packs do not sign off the whole application or accessibility compliance.

## Visible exclusions and remaining risk

| Scenario / area | Why not a release-pack pass |
| --- | --- |
| TC06 combined legacy smoke | Overlaps targeted refinement checks; remains Suite=Smoke |
| TC30 controlled search recovery | Deferred; deterministic failure hook unavailable; explicitly skipped draft |
| TC36 / TC37 calendar and passenger draft cancellation | Suite=Readiness; shared app contract needs qualification |
| TC43 booking with seats | Included in Sanity/Regression. Historical payment issue #177 requires live requalification; pre-payment passes do not establish successful booking or seat fulfilment |
| Loyalty TC03 Pay with Points | Suite=Deferred; paused for app redesign, not represented by redemption coverage |
| Infant selected-seat display and restrictions | TC49 now requires the adult card's linked `+ infant` text and independent assignment ownership; live qualification on #1311 and restriction coverage remain open |
| Visual, accessibility, My Account and OCI | Separate scopes; run applicable packs/manual checks for affected releases |

The pack provides broad behavioural coverage of search entry, results, filtering/sorting, fare/carrier mapping, Summary changes/upgrades, passenger validation, loyalty, seats and checkout. It is not measured Swift line/branch coverage and cannot establish a percentage of iOS code executed. Use E2E-COVERAGE-MATRIX.md, SEATS-COVERAGE.md and TEST-CATALOG.md for assertion limits and dated evidence.

## Exact inventory

### Post-run Slack reporting

#### BrowserStack iOS build selection

For the manual workflow, leave `app_id` blank to select the newest shared
`VirginAtlantic-sit-debug-v<version>-<build>-<YYYYMMDD>-<HHMM>.ipa` upload by its
BrowserStack upload timestamp. Android and other environments/build types are excluded.
The resolver reads at most ten pages of 100 uploads; inaccessible, missing, malformed,
ambiguous or truncated matching inventory fails setup rather than choosing a fallback.
An explicit immutable `bs://<40-character hash>` overrides automatic selection for
reproductions or release-candidate qualification. Operators must verify that override
is the intended SIT iOS build; its upload metadata is not looked up.

Selection happens once before test execution. Every parallel session uses the same
resolved app ID even if another app is uploaded during the run. `app-selection.json`
is retained with the results, and GitHub/Slack reporting includes the ID, filename,
version and upload time for automatic selection. These are upload metadata, not proof
of an iOS source commit; `ios_ref` remains independently declared coverage-map input.
Local runs still use the configured app in the gitignored settings file.

The reusable native-mobile reporting hook is `scripts/publish-mobile-release-slack.ps1`
at the repository root. It uses the existing Search and Book Slack publisher and
`SLACK_WEBHOOK_URL` connection. No new webhook/channel is required. It is wired to
`.github/workflows/qa-mobile-search-book-sanity.yml` for both manual SIT release packs.

The Windows self-hosted job uses Windows PowerShell (`powershell`) and installs .NET SDK
`10.0.x` into the runner's temporary directory, without writing to Program Files or
requiring PowerShell 7.
The optional `test_name` dispatch input selects one exact method from the Sanity or
Regression pack, including TC41/TC42 backend comparisons. For those two targeted cases,
the workflow enables SIT backend validation on the self-hosted runner; UI-only targets
keep it disabled. For a one-adult guest booking without seats, enter:

```text
SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips
```

Leave `test_name` blank and select `pack`: `Sanity` (default, 14) or `Regression`
(54 including TC41/42 and TC55). Regression enables backend
validation against SIT from the existing self-hosted runner; the app sessions still run
on BrowserStack, with up to four concurrent sessions and the existing 90-minute job limit.
Select the PR branch in the manual dispatch to qualify unmerged automation changes.
Exact result membership is checked against the contract, not merely the exit code.
When using `test_name`, leave `pack=Sanity`; ambiguous pack/test combinations fail setup.
Partial names, arbitrary filters,
deferred scenarios and backend-comparison scenarios are rejected. Targeted runs require
exactly the selected test to pass and report `Targeted` (expected count 1), not Sanity
sign-off. BrowserStack project, app selection, failure evidence, booking references and
Slack reporting remain shared. These tests may create SIT bookings or consume test points.
The workflow is named **Automation - Mobile Search and Book**. Its existing filename and
concurrency group are retained to preserve dispatch links and prevent overlapping runs.
The separate FullRegression option is removed; use Regression instead.

Call it from an `always()` post-test step after uploading the current run's TRX and
diagnostics. Pass the test/build outcome, not the notification step's status, and retain
the original test exit code. Supply the existing webhook secret via the step environment.

```powershell
./scripts/publish-mobile-release-slack.ps1 -TrxPath '<current-run>/regression.trx' `
  -Pack Regression -JobStatus failure -TargetEnvironment sit `
  -RunUrl "https://github.com/VAAD1ST/eapi/actions/runs/$env:GITHUB_RUN_ID"
```

Replace the example `failure` with the actual job outcome (`success`, `failure` or
`cancelled`); choose `Sanity` or `Regression`. The expected test count
comes from the release-pack contract. Missing/empty/short results, skipped tests and
cancelled/failed jobs cannot produce a green mobile report. Invalid TRX fails reporting.
Reports include totals, pass rate, failed method names (first ten), confirmed test booking
references (first eight) and a workflow link for uploaded evidence. Booking references are
extracted only from `[confirmed-booking] PNR=ABC123; scenario=SearchBook_TC48_...`
records in passed-test output, matched to the TRX scenario. All current complete booking
tests (guest one-way/return, with/without seats and redemption) call the shared
`SearchBookTestBase.ReportSuccessfulBooking` after final assertions. New booking tests
must use this method instead of writing ad-hoc reference text or only setting session status.
The marker is written to xUnit/TRX independently of BrowserStack status-update availability;
partial journeys and failed tests are not reported as successful bookings.
No passenger surnames, credentials, raw errors or stdout are copied to the mobile report.
The workflow link is the evidence entry point; direct BrowserStack session links are not
yet extracted by this hook. It is a summary, not independent release sign-off.

Use `-PreviewOnly -PayloadPath <file.json>` to inspect a report without sending anything.
Run `pwsh -NoProfile -File scripts/test-mobile-release-slack.ps1` from the repository root
for offline payload checks. No live Slack message was sent during implementation.

All rows belong to Regression. TC41/42 also retain BackendRegression as a diagnostic subset trait, not a separate workflow pack. “Yes” identifies the additional Suite=Sanity tag. Names are stable xUnit method names; IDs are not renumbered.

| ID | Sanity | Scenario method |
| --- | --- | --- |
| TC01 | — | `SearchBook_TC01_MixedPassengerOneWay_SelectsVirginFlight` |
| TC02 | — | `SearchBook_TC02_MixedPassengerOneWay_SelectsNonVirginFlight` |
| TC03 | — | `SearchBook_TC03_TwoAdultsAndChildReturn_SelectsVirginOutboundAndInbound` |
| TC04 | — | `SearchBook_TC04_TwoAdultsAndChildReturn_SelectsVirginOutboundAndPartnerInbound` |
| TC05 | Yes | `SearchBook_TC05_SearchEntry_EnforcesCalendarAndPassengerRules` |
| TC07 | — | `SearchBook_TC07_PriceSort_OrdersDisplayedResultsAscending` |
| TC08 | — | `SearchBook_TC08_DepartureTimeSort_OrdersResultsAndRecommendedRestoresBaseline` |
| TC09 | — | `SearchBook_TC09_DurationSort_OrdersResultsAndRecommendedRestoresBaseline` |
| TC10 | — | `SearchBook_TC10_SortSwitching_PreservesSelectionAndRecommendedRestoresBaseline` |
| TC11 | — | `SearchBook_TC11_AvailableAirlineFilters_EveryResultContainsSelectedOperatedLeg` |
| TC12 | — | `SearchBook_TC12_ConnectionsFilters_EveryResultMatchesSelectedCount` |
| TC13 | — | `SearchBook_TC13_DepartureTimeFilters_EveryResultFallsInsideSelectedWindow` |
| TC14 | — | `SearchBook_TC14_FilterSwitching_ReplacesDiscoveredAirlineAndValidatesResults` |
| TC15 | Yes | `SearchBook_TC15_PriceSortWithDiscoveredAirlineFilter_PreservesOrderingAndMatchesCarrier` |
| TC16 | — | `SearchBook_TC16_ClearFilters_RemovesDiscoveredAirlineAndPreservesPriceSort` |
| TC17 | — | `SearchBook_TC17_RecentAirportSearches_RetainFiveMostRecentPerDirection` |
| TC18 | — | `SearchBook_TC18_HemanPromoCode_DiscountsManchesterNewYorkReturnEconomyClassicByFivePounds` |
| TC19 | Yes | `SearchBook_TC19_FilterSelection_RecalculatesAvailabilityAcrossOtherGroups` |
| TC20 | — | `SearchBook_TC20_VirginOperatingCarrierFlightCards_UseBroadCabinGroupNames` |
| TC21 | — | `SearchBook_TC21_OtherOperatingCarrierFlightCards_UseBackendBrandCabinNames` |
| TC22 | — | `SearchBook_TC22_VirginFlightCabin_UpgradeViewFaresOpensNextAvailableCabin` |
| TC23 | Yes | `SearchBook_TC23_OneAdultGuestFromJfkToLhr_CompletesUsdBookingAndAppearsInTrips` |
| TC24 | — | `SearchBook_TC24_VirginOperatingCarrier_FlightSummaryMatchesSelectedFlight` |
| TC25 | — | `SearchBook_TC25_OtherOperatingCarrier_FlightSummaryMatchesSelectedFlight` |
| TC26 | Yes | `SearchBook_TC26_MultiCarrierConnectingFlight_FlightSummaryRetainsStopsAndAdditionalCarrier` |
| TC27 | Yes | `SearchBook_TC27_FlexibleDateSelection_RefreshesResultsAndPreservesRoute` |
| TC28 | Yes | `SearchBook_TC28_EditSubmittedSearch_UpdatesCriteriaAndResultsRoute` |
| TC29 | Yes | `SearchBook_TC29_SelectedFareBenefitsAndBaggage_PropagateToFlightSummary` |
| TC31 | — | `SearchBook_TC31_DiscountCode_RejectsInvalidDraftsAndTrimsAppliedCode` |
| TC32 | — | `SearchBook_TC32_DiscountCode_CancelPreservesAppliedCodeAndClearRemovesIt` |
| TC33 | — | `SearchBook_TC33_FareSheetCancellation_PreservesResultAndOriginalCabinOnReopen` |
| TC34 | — | `SearchBook_TC34_MissingPassengerFirstName_ShowsErrorAndPreventsSave` |
| TC35 | Yes | `SearchBook_TC35_ValidGuestPassenger_TermsAcceptanceControlsContinue` |
| TC38 | — | `SearchBook_TC38_OneWaySummaryChange_RetainsReplacementFlightFareAndPrice` |
| TC39 | — | `SearchBook_TC39_OneWaySummaryUpgrade_UpdatesFareAndTotalPreservingFlight` |
| TC40 | — | `SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal` |
| TC41 | — | `SearchBook_TC41_OneWaySummaryBaggage_MatchesBackendSelectedFlightAndFare` |
| TC42 | — | `SearchBook_TC42_ConnectingPartnerSummaryBaggage_MatchesBackendSelectedLegsAndFare` |
| TC43 | Yes | `SearchBook_TC43_OneAdultGuestSeatSelection_CompletesBookingAndAppearsInTrips` |
| TC44 | — | `SearchBook_TC44_OneAdultSeatSelection_ReplacementAndRemovalUpdateAssignment` |
| TC45 | — | `SearchBook_TC45_SeatDetailsAndKey_DismissalPreservesPassengerAssignment` |
| TC46 | — | `SearchBook_TC46_TwoAdultSeatSelection_RetainsIndependentAssignments` |
| TC47 | Yes | `SearchBook_TC47_PaidSeatReplacementAndRemoval_RecalculateChargeAndTripTotal` |
| TC48 | Yes | `SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips` |
| TC49 | — | `SearchBook_TC49_AdultWithLapInfant_OnlyAdultHasIndependentSeatAssignment` |
| TC50 | — | `SearchBook_TC50_UnsupportedOperatingCarriers_BypassSeatsWhileVirginRemainsEligible` |
| TC51 | — | `SearchBook_TC51_AdultAndChildVirginOneWay_DisplaysSeatProximityGuidance` |
| TC52 | — | `SearchBook_TC52_UnsupportedOutboundEligibleInbound_DisplaysGuidanceForFirstEligibleSector` |
| TC53 | — | `SearchBook_TC53_AdultAndChildUnsupportedOneWay_BypassesGuidanceAndSeats` |
| TC54 | — | `SearchBook_TC54_AdultAndChildDirectDeltaOneWay_DisplaysSeatProximityGuidance` |
| TC55 | — | `SearchBook_TC55_ShortNoticeLosCardPresentation_AgreementCompletesBookingAndTrips` |
| LOYALTY-TC01 | Yes | `LoyaltySearchBook_TC01_HighBalanceEconomyRedemption_CompletesThroughHppConfirmationAndTrips` |
| LOYALTY-TC02 | Yes | `LoyaltySearchBook_TC02_LowBalanceReturnUpperClass_BlocksContinuationForInsufficientPoints` |
| LOYALTY-TC04 | — | `LoyaltySearchBook_TC04_MemberNotTravelling_ClearsProfileAndRetainsReplacementTraveller` |

## Source-aware Slack coverage

Workflow Sanity and targeted runs use the `Diagnostic` BrowserStack profile so video,
visual debugging and network logs are available from session creation. Recordings are
retained in BrowserStack and can contain the synthetic passenger/payment screens; use
approved test data and existing restricted project access. Local example settings remain
`Fast`; the workflow explicitly overrides the profile for release qualification.
Ordinary Sanity runs keep BrowserStack response-body capture disabled. Use the existing
targeted diagnostic path only with explicit authorisation; failure-only local reporting
does not prevent BrowserStack retaining bodies captured during the session. Mobile Slack
delivery remains required, while existing web callers retain best-effort notification.

The Sanity workflow also produces an iOS source inventory and current-run mapping report.
See [IOS-SOURCE-COVERAGE.md](IOS-SOURCE-COVERAGE.md) for the bounded scope, source-access
requirements, `ios_ref` / `app_commit` inputs, historical seed mappings and confidence limits.
Unavailable or stale mappings must not be interpreted as release sign-off.
