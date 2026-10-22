# Search and Book E2E coverage matrix

**PayPal gap:** TC57 implements sandbox login, single approval, confirmation and Trips;
not live-qualified and excluded from release packs. The known confirmation blocker remains
a failure; login-only progress is not a full test pass.
See [PayPal coverage and privacy gates](PAYPAL-COVERAGE.md).

## Open coverage requirement — US-origin Economy Delight seats

**Awaiting exact PO/developer requirements; not implemented or qualified.** Add a
representative US-origin USD journey to verify the confirmed free-seat entitlement,
retained assignment and unchanged Review total. Eligible seat types/carriers and exclusions
must be confirmed before authoring. TC23's Economy Classic USD booking does not cover this.
See [pending seat requirements](SEATS-COVERAGE.md#pending-requirements--us-origin-economy-delight-free-seats).
This remains an explicit gap at coverage review; executable test counts, release packs
and component-coverage percentages are unchanged.

**Review MVP scope:** visual/accessibility checks are manual; PWP eligibility/selection and
unspecified banner variants are outside this ticket. New TC56 covers pre-HPP back/re-entry
and retained total/payment readiness, pending local qualification (`Suite=Readiness`).
Fare-conditions navigation and linked itinerary comparison need app-owned destination,
route-field and return-action IDs. See [functional blockers](REVIEW-TOTAL-COVERAGE.md).
This does not increase Regression/Sanity counts or claim complete Review coverage.

**20 October — pre-HPP Review total:** shared complete-booking checkout now checks price
arithmetic, Summary-total/currency/points retention, guest PWP exclusion and guidance/actions.
Implemented with 25 new offline cases, including independent seat-detail/cart/Review pricing;
local TC43 stopped on a seat-map error before Review, so live qualification remains pending. See
[Review total coverage](REVIEW-TOTAL-COVERAGE.md) for the explicit itinerary, tax/seat-source,
banner, PWP and visual gaps. This does not increase the scenario count or constitute a
new passing component-coverage claim.

For run-specific, source-aware reporting see [dynamic iOS source mapping](IOS-SOURCE-COVERAGE.md).
It inventories the requested commit, invalidates changed mappings, and joins current TRX results.
Older dated percentages below are historical and are not used in Slack.
The **Current component coverage** table is an executable reporting input:
`get-mobile-ios-coverage.ps1` parses it to calculate Slack's documented component-mapping
percentage. This is representative functional mapping, not measured Swift code coverage;
its source freshness is reported separately.

## Release-pack selection — 19 October 2026

Use the [release-pack inventory and sign-off policy](RELEASE-PACKS.md): 14 Sanity,
54 Regression (including both backend comparisons and TC55 card-presentation agreement). Regression requires backend validation
enabled and API connectivity from the runner; the workflow enables validation automatically.
TC44–50 are enrolled following targeted local passes; TC43 is included with the historical payment blocker requiring live requalification.
TC30 and Loyalty TC03 are deferred; TC36/37 remain readiness checks. These selections
supersede historical pack labels below, but do not establish fresh passing evidence or
measured Swift code coverage. Review documented exclusions before release acceptance.

## Seats qualification — 6 October 2026

The detailed [seats coverage matrix](SEATS-COVERAGE.md) maps the user-visible controls and
state transitions against pinned iOS develop, with a lean proposed pack and explicit app-ID gaps.
Planned scenarios are not included in executable or passing totals.

TC43 adds a separate one-adult guest Economy Classic seat journey, outside regression
(`Suite=Qualification`). Latest local attempt reached the seat map and tapped an offered
seat but could not locate `seatSelection.selectSeat` to confirm it. **Not passed**; no payment
submitted. 343 offline checks passed. Evidence:
`TestResults/seats-classic-local-20261006/local.trx`. See the test catalogue for scope/flags.

## Latest one-adult guest smoke — 6 October 2026

Passed locally on iPhone 17 Pro / iOS 26.5 in 3m33s: LHR-JFK search, Virgin selection,
Summary, passenger details (native gender menu), hosted test payment, confirmation and booking
presence in Trips. The run used a temporary one-adult TC01 variant; original five-passenger
source was restored. This does not requalify the five-passenger scenario or full regression.
Evidence: `TestResults/guest-payment-entry-smoke-20261006/local.trx`; exact-TRX analysis found
no failures. All 338 mobile offline/retry checks passed. Formatting passed; the wider harness
has an unrelated stale context-routing metadata check (33 days; maximum 30).

## TC41/TC42 UI/backend baggage parity — regression

| Criterion | Owner | Status |
| --- | --- | --- |
| Fare benefits retained in Summary | TC29 | Locally passed; no backend dependency |
| Backend-first matched flight/fare baggage count/weight/dimensions | TC41 | Two local passes; ordinary accessibility IDs only |
| Connecting partner: every ordered leg, transfer airport, fare and itinerary baggage | TC42 | Passed locally in 153s; two allowance rows and two dimension comparisons |
| External runner / BrowserStack API path | TC41 runner-side wrapper | Implemented; runner connectivity and remote qualification pending |

TC41/TC42 use the opt-in `Suite=BackendRegression&RegressionArea=SearchAndBook` pack,
excluded from default `Suite=Regression` runs. Enable backend validation on the runner. TC41 has two local passes;
TC42 has one local pass. BrowserStack qualification remains pending for both.
See [implementation and configuration](BACKEND-UI-VALIDATION.md).

## TC40 return Summary Change — locally qualified, 30 September 2026

`SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal`
covers one-adult return outbound change from Summary, mandatory inbound reselection, distinct
replacement schedules in both directions, scoped route/date/time/carrier/fare retention and
the displayed currency total. Reuses existing action IDs and page objects; independent class
with SearchAndBook regression traits. Stops at Summary without checkout.

Maps to iOS `NativeFlightSummaryViewModel.changeFlight`,
`NativeFlightBookingJourney.setOutboundSelection` (clears inbound and booking progress),
`setInboundSelection` and `rebuildSummary`. Price checking follows
`FlightSearchFareProduct.fareSelectionPriceText`: signed inbound adjustments are added to
the outbound base; an unsigned amount is the full total. Same-price replacements are valid.
Requires at least two selectable Virgin schedules in each direction. Snapshot evidence proves
UI state, not internal basket reset or full per-segment identity. Android/BrowserStack unqualified.

Passed on local iPhone 17 Pro/iOS 26.5 in **2m54s**:
`TestResults/tc40-local-20260930-inboundcontract/local.trx`.
All 283 offline/retry checks also passed (284 total, zero failures/skips); Release build,
exact-TRX analysis and project-scoped harness verification passed.
The first run exposed a test assumption: inbound has no flexible-date carousel. The corrected
test verifies the return date in Summary and still requires the inbound results route.
This is one local pass, not repeated-run or BrowserStack qualification.
Inventory: 41 scenarios; 38 with passing evidence, two on hold, one skipped.
The preceding 40-scenario totals below are historical.

**Loyalty TC04 qualified locally — 30 September 2026: passed in 2m06s.**
This supersedes the failed/pending TC04 evidence below. The passenger editor now opts into
container-scoped XCTest `mobile: swipe` upwards rather than `mobile: scroll`'s press-and-drag.
This avoids the hold that was activating the SwiftUI loyalty menu. Attempts remain bounded;
a hidden container fails explicitly rather than swiping behind an overlay. Other scroll callers,
the calendar and Android navigation are unchanged. No app code or selector contract was changed.

The local iPhone 17 Pro/iOS 26.5 scenario verified member-profile prepopulation, opt-out clearing,
replacement passenger/contact entry, save/reopen retention and persistent not-travelling state.
No passenger submission, HPP or payment was attempted. **271 offline/retry checks plus one live
scenario passed (272 total, zero failures/skips)**; Release build and project-scoped harness
verification passed. Exact-TRX analysis found no failures:
`TestResults/member-not-travelling-local-20260930-nativeswipe/local.trx`.
This is one local qualification, not repeated-run, Android or BrowserStack qualification.
Consolidated historical evidence: **40 scenarios, 37 with passing evidence, two unresolved/on hold,
one skipped**; no new full-pack run. Earlier totals and investigations below are historical.


**Latest Loyalty TC04 local evidence — 30 September 2026: still failing; supersedes the historical notes below.**
The read-only profile snapshot no longer scrolls through fields. It handles the observed composed
DOB button ID and WebDriverAgent's omission of empty native-control values; missing controls,
hidden editors and conflicting values still fail. Login, initial profile matching, member opt-out
and all seven cleared-field assertions passed. Replacement name, DOB, gender and email entry
completed, but contact entry failed trying to reach the telephone field after three bounded
scrolls, with the passenger editor hidden behind an overlay. The user identified the overlay as
the loyalty-airline menu, not the country picker. A diagnostic rerun
(`TestResults/member-not-travelling-local-20260930-interactiontrace/local.trx`: 269 offline
passes, one live failure in 2m11s) checked editor visibility after each input action and after
the country-code state check. Those checks passed; the next telephone-scroll operation failed.
The installed WebDriverAgent implements `mobile: scroll` using a 0.5-second press before
dragging and another hold afterwards. Long-press activation of the SwiftUI loyalty Menu is the
strongly supported mechanism; gesture-level interception was not directly instrumented.
Next step: qualify a container-scoped native swipe without that long press, retaining bounded
attempts and target/overlay checks. Temporary diagnostic probes were removed; no gesture fix
or iOS app change has been applied. This is not evidence of a backend failure or missing
telephone ID. Save/reopen retention is not yet qualified. No booking/payment was attempted.

Latest exact TRX: `TestResults/member-not-travelling-local-20260930-countrycheck/local.trx`:
**269 offline/retry checks passed; one live scenario failed**. Release build and project-scoped
harness verification passed. Reading existing field state does not require visibility of each
field, but the containing editor must be visible. Native empty values are accepted only from
identified Button/TextField controls, never from a missing node. Country-code state is checked
before scrolling/opening its selector; avoiding that redundant scroll alone did not resolve the
contact-entry overlay. Consolidated status remains **40 scenarios: 36 with passing evidence,
three unresolved/on hold, one skipped**. No full-pack rerun or Android/BrowserStack qualification.

**Loyalty TC04 first local run — 29 September 2026: failed (2m20s).** Login, search and
passenger-editor entry completed, but the initial profile read could not bring
`PassengerDetailsContactTelephoneText` into view after eight bounded downward scrolls.
The sanitised hierarchy contains that ID with `visible=false`; this is not a missing-ID finding.
Failure occurred before selecting not-travelling, so clearing/replacement retention remain unqualified.
No passenger submission/payment was attempted. Exact-TRX analysis confirms one failure:
`TestResults/member-not-travelling-local-20260929-first/local.trx`.
Investigate the automation reader's scrolling (including direction/overshoot) before attributing
this to app behaviour. Consolidated: 40 regression scenarios, 36 with passing evidence,
three unresolved/on hold and one skipped. This supersedes the pending-live status below.

## Member not travelling — implementation, pending live qualification

`LoyaltySearchBook_TC04_MemberNotTravelling_ClearsProfileAndRetainsReplacementTraveller`
is a separate iOS regression class in SearchAndBook, not My Account. It leases a SIT Red account
through the existing Core-backed provider (minimum points zero), performs a one-adult currency
search and verifies the lead passenger matches the member. The app-owned not-travelling action
must clear first/surname, DOB, gender, membership, profile email and telephone. The test enters
synthetic replacement/contact details, saves, reopens and checks retention and opt-out state.
It does not submit passenger details, reach HPP, create a booking or alter the member profile.
The lease is retained through logout cleanup; no member values are included in new assertion errors.
English empty-field placeholders are recognised explicitly; missing value evidence fails.
Country-code reset and loyalty-programme selector reset are not asserted in this first scenario.

Maps to iOS `FlightPassengerDetailsViewModel.selectMemberTravelling(false)`,
`clearProfilePopulatedContactDetails` and editor save/reopen. Requires the member menu IDs from
iOS PR #1244; Android should export equivalent action IDs, selected state and readable field values.
Android and BrowserStack execution are not qualified. This is implemented coverage, not a passing
live result: **40 regression scenarios, 36 with passing evidence, one newly unqualified, two
unresolved/on hold and one skipped**. Older totals below are historical.

**TC39 local qualification — 29 September 2026:** passed on iPhone 17 Pro / iOS 26.5
in 2m19s. The offered Summary upgrade retained route/carrier/schedule and matched the target fare
and recalculated currency total. No checkout/payment was attempted. Exact-TRX analysis found no
failures: `TestResults/tc39-local-20260929-first/local.trx`. This supersedes TC39's pending-live
status below. Consolidated regression: **39 scenarios, 36 with passing evidence, two unresolved/on
hold and one skipped**. This is a targeted local pass, not a full-pack or BrowserStack run; it does
not establish live coverage of both alternative price-display branches or return-trip upgrades.

## TC39 Summary Upgrade — implementation, pending live qualification

`SearchBook_TC39_OneWaySummaryUpgrade_UpdatesFareAndTotalPreservingFlight` is a new independent
iOS regression class. One adult searches LHR-JFK, selects Virgin Economy, then selects an upgrade
actually offered on Summary. App-owned indexed IDs distinguish the target title, additional charge
and replacement total. Assertions compare exact fare and currency amount while retaining the original
route, carrier and displayed schedule. The action is submitted once; an app-owned Summary error
fails without retry. The scenario stops before checkout and requires an offered positive-priced upgrade.
Zero/negative/reward pricing, return-trip upgrades and BrowserStack qualification are not covered.
Maps to iOS `NativeFlightSummaryViewModel.upgradeFlight`, authoritative itinerary pricing and
`NativeFlightSummaryUpgradeSection`. Existing TC22 only covers the results fare-selector upgrade.
The IDs are delivered in iOS PR #1244; Android equivalence remains a delivery requirement, not
implemented support. Regression now has **39 scenarios: 35 with passing evidence, TC39 unqualified,
two unresolved/on hold and one skipped**. Older counts/statuses below are historical.

**TC38 local qualification — 29 September 2026:** passed on iPhone 17 Pro / iOS 26.5
in 2m04s. Summary Change selected a different schedule and retained the replacement route,
carrier, exact fare title and currency amount. No checkout/payment was attempted. Exact-TRX
analysis found no failures: `TestResults/tc38-local-20260929-first/local.trx`.
This supersedes TC38's pending-live status below. Consolidated Search and Book regression:
38 scenarios, 35 with passing evidence, two unresolved/on hold and one skipped. This is a
single local pass, not a full-pack rerun or BrowserStack qualification.

## Summary Change addition — 29 September 2026

TC38 (`SearchBook_TC38_OneWaySummaryChange_RetainsReplacementFlightFareAndPrice`) now covers
one-adult, one-way Summary Change through `NativeFlightSummaryItinerary0ChangeButton` (iOS PR #1244).
It discovers a Virgin replacement with a different displayed schedule and verifies the replacement
route/carrier/schedule, exact selected fare title and positive single-adult currency price on Summary.
No payment is submitted. Live qualification is pending; this is implementation, not a new passing result.
It maps to `NativeFlightSummaryViewModel.changeFlight` and results fare reselection.
Summary Change is only partially closed: return outbound invalidation/inbound reselection remains
uncovered, as do Summary Upgrade and member-not-travelling. Schedule distinction is not complete
per-segment identity validation. Two selectable Virgin schedules are a test-data precondition.
Android should expose an equivalent action on the actual button plus observed fare/total values;
no Android scenario support is claimed. This supersedes older statements that no Change test exists.

This matrix compares Appium scenarios with externally observable Search and Book paths in the iOS
code. It intentionally excludes native unit-test, Swift line-coverage and backend-test results.
Coverage here means that an Appium journey exercises and asserts a user-visible app behaviour.

Additional readiness qualification, 29 September: the existing panel cancellation tests were
renamed TC36 (date edits/cancel, local pass 50s) and TC37 (passenger counts/confirm/cancel, local
pass 26s). They remain outside the 37-scenario regression pack pending cross-platform requalification.
See `TEST-CATALOG.md` for exact evidence and `SEARCH-BOOK-COVERAGE-GAPS.md` for the required
Summary Change/Upgrade, member-not-travelling and detailed connection app contracts.

## Latest iOS develop audit

**Current override — 29 September:** Loyalty TC01 subsequently passed through HPP, confirmation
and Trips (`TestResults/redemption-diagnostics-20260929/redemption.trx`). TC01 guest is on hold
for the app-side passenger fix; PWP investigation is on hold during implementation changes.
The earlier failures below are historical, not the latest redemption verdict.
TC31-TC35 now implement iOS discount-panel state, fare dismissal, required passenger-name validation
and the Terms gate. All five passed locally on 29 September in 5m49s
(`TestResults/gap-scenarios-local-20260929/local.trx`; analyser: no failures).
Regression has 37 scenarios: 34 passing evidence, two unresolved/on hold and one skipped.
The whole regression pack has not been rerun together, and BrowserStack qualification is outstanding.
See [the gap register](SEARCH-BOOK-COVERAGE-GAPS.md).

**Coverage qualification:** the historical 19/20 component tally below counts the seat-step
bypass, not actual seat selection, and counts representative paths rather than all branches.
Do not report it as 95% of iOS functionality or code tested. The focused 29 September source
review used local checkout `27482f94` plus working-tree changes; it does not replace the full
22 September develop inventory with a new percentage.

Latest loyalty execution: updated credentials cleared both login failures. TC02 passed; TC03
now fails verifying the selected Pay with Points value. Enabled Economy redemption TC01 passed
live affordability (30,000 of 150,000 points), Summary and passenger steps, then failed on the
app's explicit payment-failure state after HPP submission. No payment retry, confirmation,
Trips completion or backend points-ledger claim. See the catalogue for exact TRX evidence.

Execution update (28 September 2026): the full local regression reported 21 passed, 9 failed
and 2 skipped in 40 minutes. Targeted automation repairs subsequently passed TC04, TC05, TC17
and TC28. TC03 then passed its full three-passenger return booking through payment, confirmation
and Trips in 4m50s. TC18 also passed after correcting its indexed fare-promotion selector,
proving the matched Economy Classic fare changed from £545.46 to £540.46 with HEMAN.
The five-passenger TC01 hub/WDA hang and loyalty login rejection remain open. Component mapping percentages below describe
historical behavioural breadth, not a current all-green pack. See `TEST-CATALOG.md` for evidence.

### Historical 22 September ViewModel inventory

The table in this subsection is historical; the current component table below supersedes
its seat-gap and execution statements.

This mapping was rechecked on 22 September 2026 against local iOS `develop` commit `a5151181`
(`chore(search-and-book): update flight result designs (#1211)`). The audit covered the current
Search and Book screen ViewModels, router, flight-search UI models and user-visible SwiftUI paths.
It does not include native unit tests or claim Swift statement/branch coverage.

| Principal ViewModel / routing area | Appium mapping | Position |
| --- | --- | --- |
| `SearchAndBookViewModel` and `SearchAndBookRouter` | TC01-TC05, TC17-TC18, TC23-TC28 | Mapped through entry, results and downstream routing |
| `NativeFlightSearchEntryViewModel` including discount-code extension | TC01-TC05, TC18, TC23-TC28 | Mapped; TC18 owns the controlled promotion path |
| `NativeFlightAirportSelectionViewModel` | TC01-TC05, TC17-TC18, TC23-TC28 | Mapped; TC17 owns recent-airport persistence |
| `FlightDatePickerViewModel` | TC01-TC05, TC18, TC23-TC28 | Mapped; TC05 owns boundaries and TC27-TC28 prove results-date propagation |
| `NativeFlightPassengerSelectorViewModel` | TC01-TC05 | Mapped; TC05 owns limits and validation |
| `NativeFlightSearchResultsViewModel` and Brand/refinement/route extensions | TC01-TC04, TC06-TC29 | Mapped across results, dynamic refinement, cabins, fares, flexible dates and edit search |
| `NativeFlightSummaryViewModel` | TC01-TC04, TC24-TC26, TC29, Loyalty TC02 | Mapped; TC29 Summary → Flight details passed locally 28 September 2026 |
| `FlightPassengerDetailsViewModel` | TC01, TC03, Loyalty TC01-TC03 | Mapped for guest and prepopulated-member behaviour |
| `NativeFlightSeatSelectionViewModel` | No direct Appium scenario | Deliberate gap: current automation startup profile skips the unfinished seat-selection step |
| `NativeFlightPaymentHandoffViewModel` | TC01, TC03, TC55 and Loyalty TC01 | Mapped; TC55 card-presentation agreement passed locally; cancellation and eligibility boundaries remain open |
| `NativeFlightBookingConfirmationViewModel` | TC01, TC03 and Loyalty TC01 | Guest execution verified; redemption confirmation unverified due to explicit post-HPP failure |

This gives direct scenario mapping for 10 of 11 principal screen ViewModels (**91%**). That figure
measures whether a ViewModel has an owning Appium journey, not whether every property, error branch
or rendering permutation is exercised. The latest result-design changes are represented by TC20-TC22,
TC27-TC29 and the reward journeys; equal-height layout and other purely visual styling remain native
UI/snapshot concerns rather than semantic Appium assertions.

## Current component coverage

Latest focused source review: [16 October develop impact review](IOS-DEVELOP-REVIEW-20261016.md)
against `b4697b474473f690d618c765f86265e3f72ce84f`. Linked-infant passenger-card text and child
proximity guidance were added after the 13 October review. TC49 now asserts the linked-infant
text; child guidance remains blocked on its missing action identifier. This is not live
qualification or a refreshed whole-source baseline. Current
breakdown: **14 Mapped, 5 Partial, 1 Gap**. Slack derives that breakdown and the names of
partial/gap areas from this table. The complete 22 September source baseline remains
`NeedsReview`; the focused review must not be presented as a full baseline refresh.
Loyalty TC02 subsequently passed calendar selection on BrowserStack but failed at the
missing insufficient-points warning; tracked in
[mobile bug #188](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/188).

Slack derives its component mapping percentage directly from this table: rows marked Mapped
or Partial divided by all assessed rows. Partial rows are included in mapping breadth but
listed separately; this is not exhaustive functionality or Swift code coverage. The existing
20-component assessment gives 19 mapped/partial (95%), including five partial areas and one
gap. This is the documented baseline, not a fresh develop qualification. Source drift marks
it as needing review; new functionality must be assessed before refreshing the baseline.

| App component | iOS implementation reference | Current Appium coverage | Coverage intent | Decision | Mapping status |
| --- | --- | --- | --- | --- | --- |
| Entry and routing | `NativeFlightSearchEntryViewModel`, `SearchAndBookRouter` | TC01-TC05, TC17-TC18, TC23-TC28 | Enter native Search and Book and submit valid criteria | Keep one-way, return and focused entry coverage | Mapped |
| Airport selection and recents | `NativeFlightAirportSelectionViewModel`, `UserDefaultsRecentAirportStore` | TC01-TC05, TC17-TC18, TC23-TC28 | Find airports and retain the latest five per direction | TC17 owns the user-visible recents contract | Mapped |
| Calendar and journey dates | `FlightDatePickerViewModel`, search-entry view model | TC01-TC05, TC18, TC23-TC28 | Select one-way/return dates, valid boundaries and a controlled promotion month | TC05 owns boundaries; journeys prove integration | Mapped |
| Passenger selection | passenger-selector view model and passenger counts | TC01-TC05 | Defaults, mixed passengers and supported limits reach search | TC05 owns validation; TC01/TC03 provide representative E2E data | Mapped |
| Search results | search-results view model | TC01-TC04, TC06-TC16, TC18-TC28 | Live results display and can be selected | Avoid duplicating backend offer permutations | Mapped |
| Sorting | results refinement view model | TC06-TC10, TC15-TC16 | Each sort orders correctly and switching/reset works | TC07-TC09 own ordering; refine TC10 to switching state only | Mapped |
| Filters and cascading availability | results filter view and refinement adapter | TC06, TC11-TC16, TC19 | Dynamic options match results, combine, switch, clear and cascade | Keep: each test owns a distinct interaction contract | Mapped |
| Flexible dates | flexible-date carousel and view-model logic | TC27 | Changing an in-viewport available date refreshes results, retains the exact selected date and preserves route context | Passed locally on 22 September 2026 | Mapped |
| Edit search | results edit-search view model and router | TC28 | Editing from inbound results updates route and departure date in results | Passed locally on 22 September 2026 | Mapped |
| Promotion | discount-code view model and service integration | TC18 | An eligible live promotion changes the matching fare | Keep one controlled positive Appium journey | Mapped |
| Currency and market | result price formatting | TC23 | US-origin guest pricing is shown in USD | Keep one market-boundary check | Mapped |
| Cabin and fare selection | cabin grouping, Brand content and fare selector | TC20-TC22, TC29 | Virgin grouping, partner Brand names, upgrade navigation and selected-fare benefits are correct | Keep; TC29 discovers the displayed fare and compares Summary → Flight details baggage; passed locally | Mapped |
| Flight details and connections | flight-details view and connection mapping | TC11-TC12, TC26 | TC26 compares ordered leg airports/dates/times/operators and connection continuity between Results and Summary details | Extended TC26 passed locally 30 September against rebuilt app (2m17s); supersedes installed-build blocker. TC11 remains card-first; no all-carrier/Android/BrowserStack qualification | Mapped |
| Flight Summary | summary view model and views | TC01-TC04, TC24-TC26, TC29, TC38-TC42, Loyalty TC02 | Retention, change/upgrade and baggage comparisons | iOS #1277 removed the insufficient-points warning/guard still required by Loyalty TC02; agree replacement outcome, do not waive | Partial |
| Passenger details | passenger-details view model | TC01, TC03, Loyalty TC01-TC03 | Guest and prepopulated member details can continue | Keep representative guest and member Appium paths | Mapped |
| Seat step | seat-selection view model | TC44-TC47, TC49-TC54; TC43 Qualification, TC48 no-seat E2E | Assignment lifecycle, sheets, independent adults, prices, linked lap infant, carrier eligibility and child-guidance display boundaries | TC51–54 are unqualified and exact guidance copy lacks IDs; unavailable eligible-map traversal, retained refreshed offers and paid-seat fulfilment remain open | Partial |
| Payment handoff | payment-handoff view model and navigation | TC01, TC03, TC48; Loyalty TC01 | Valid booking reaches HPP and backend failures are reported | #1294 needs happy-path requalification; dismiss/reopen open; controlled failure remains backlog | Partial |
| Confirmation and Trips | native confirmation and Trips handoff | TC01, TC03, TC48; Loyalty TC01; TC43 Qualification | Exact submitted count/names, reference and Trips; total remains nonempty-only | #1293 assertions updated, live qualification pending; receipt and confirmation details/seat fulfilment remain gaps | Partial |
| Loyalty reward and points | account-backed booking paths | Loyalty TC01-TC03 | Login, insufficient points and Pay with Points work with leased accounts | Consolidated; keep the two distinct points behaviours | Partial |
| Error and retry states | search/results/payment error presentation | Failure diagnostics only | User-visible failure and retry contracts are deterministic | Gap: controlled search retry and payment failure scenarios | Gap |

## Existing test disposition

| Tests | Role | Disposition |
| --- | --- | --- |
| TC01 | Core one-way guest E2E | Keep through confirmation and Trips |
| TC02 | Partner one-way data variant | Stops at Flight Summary; partner checkout repeats TC01 |
| TC03 | Core return guest E2E | Keep through confirmation and Trips |
| TC04 | Mixed-carrier return data variant | Stops at combined Flight Summary; checkout repeats TC03 |
| TC05 | Entry rules | Keep serialized; uniquely owns calendar/passenger UI boundaries |
| TC06 | Short combined refinement smoke | Retained as smoke/pre-merge coverage and removed from full regression |
| TC07-TC09 | Individual sort contracts | Keep: short, parallel and diagnostically precise |
| TC10 | Sort switching | Refined to switching/selection/reset only; TC07-TC09 own ordering |
| TC11-TC16, TC19 | Filter interaction contracts | Keep; they cover distinct dynamic filter behaviours |
| TC17 | Recent airports | Keep; unique persisted UI behaviour |
| TC18 | Promotion | Keep while HEMAN remains controlled test data |
| TC20-TC22 | Cabin and upgrade contracts | Keep; distinct carrier and fare-selector behaviours |
| TC23 | US currency | Keep; unique locale/market boundary |
| TC24-TC26 | Flight Summary contracts | Keep; focused and reusable in longer journeys |
| Loyalty TC01 | High-balance Economy redemption E2E | Enabled; 100,000+ table points, live affordability and retained Summary points before HPP, confirmation and Trips |
| Loyalty TC02 | Insufficient reward points | Keep; unique guarded journey |
| Loyalty TC03 | Pay with Points | Keep; unique payment mode |

The proposed refinement leaves two guest flows exercising HPP, confirmation and Trips: one one-way
and one return. Carrier, connection, cabin and loyalty variants stop at the earliest screen that
proves their distinct contract. This reduces runtime and downstream exposure without losing a
user-visible behaviour.

## Next minimal additions

| Proposed ID | Scenario | End point | Why Appium owns it |
| --- | --- | --- | --- |
| TC27 | Select an in-viewport available flexible date and verify the exact selected date plus refreshed route/results state | Results | Ready; passed locally on 22 September 2026 |
| TC28 | Edit an inbound-stage search and verify changed route and departure date in results | Results | Ready; passed locally on 22 September 2026 |
| TC29 | Retained fare title; baggage inclusion and advertised count/weight match selected benefits; explicitly excluded categories may be absent; compare three-axis dimensions/units when advertised | Summary → Flight details | Passed locally 30 September, 58s: two inclusion checks, one weight check, zero quantity/dimension comparisons. Summary displayed dimensions but selected fare did not advertise them. Dimension accuracy needs a suitable fare or exact app-offer/backend correlation; linear limits, BrowserStack and all-carrier coverage remain outstanding |
| TC30 | Controlled search failure followed by successful retry | Results | Draft pending deterministic fail-once hook |
| TC31 | Discount draft validation and trimmed committed value | Search panel | Passed locally 29 September 2026 |
| TC32 | Discount apply/reopen, cancel, replacement and clear | Search panel | Passed locally 29 September 2026 |
| TC33 | Cancel a fare-sheet cabin change; retain result evidence and original cabin on reopening | Results | Passed locally 29 September 2026 |
| TC34 | Missing first name exposes validation and prevents traveller save | Passenger editor | Passed locally 29 September 2026 |
| TC35 | Valid single guest: Terms off/on/off gates Continue | Passenger hub | Passed locally 29 September 2026; no payment |
| Backlog | Controlled payment failure shows retry state and emits diagnostics | Payment error | Deliberately deferred for later delivery |

## Measuring coverage

Use two Appium measures rather than treating a test count as code coverage:

1. **Implemented path coverage**: a user-visible iOS code area has a mapped Appium scenario with the
   selectors and assertions needed to prove its contract.
2. **Executed path coverage**: that scenario has reached and asserted the mapped code area against
   the intended app build.

The code-to-scenario review identifies 20 externally observable component areas:

- 19/20 (**95%**) have a representative implemented mapping, including five partial areas.
- 14/20 (**70%**) are classified Mapped, 5/20 (**25%**) Partial and 1/20 (**5%**) Gap.
  Mapped does not mean every method, branch or visual state is tested.
- No current executed-coverage percentage is established against the latest develop build.
  Historical local passes do not qualify changed confirmation/payment contracts or the
  insufficient-points regression. Use each run's TRX separately from mapping breadth.

These are behavioural path percentages. They do not claim statement or branch coverage; obtaining
that figure would require a separately instrumented app build driven by Appium.

## Coverage confidence

Mapping breadth is high, but release confidence remains conditional on qualification of
the latest app and resolution of known failures. The historical 7/10 judgement is not a
current sign-off score. This assessment separates mapping from execution:

| Evidence dimension | Current confidence | Basis and limitation |
| --- | --- | --- |
| User-facing contract breadth | High | 19/20 component areas have an implemented Appium path; deterministic controlled error/retry remains absent |
| Executed cross-screen evidence | Requires qualification | Historical passes exist, but no current whole-pack executed percentage is established; Loyalty TC02 reproduced bug #188 |
| Code-to-test traceability | Moderate-high | Each of the 20 externally observable code areas is mapped to its owning Appium scenarios, but this remains a behavioural rather than line-level measurement |
| Rendered UI integration | Moderate-high | Appium validates real cross-screen behaviour, selectors and displayed outcomes; TC27-TC29 have local evidence but BrowserStack verification remains outstanding |
| Dynamic/live dependencies | Moderate | Airlines, fares, filters, promotion data, HPP and downstream services can change independently of the automation code |

TC29 has historical local passing evidence. Confidence should not be described as very high until a
deterministic search/payment failure-and-recovery path is implemented and the representative guest
and loyalty completion journeys are stable through HPP, confirmation and Trips.
# Seats update — 7 October 2026

Newest TC47 profiling run stopped at an app seat-map error before discovery (1m42s). Automation
timing/read consolidation is offline-validated, not live-qualified; the earlier pass below is historical.
Evidence: `TestResults/seats-tc47-profile-local-20261007/local.trx`. No performance gain claimed.

**Latest TC47 passes locally (6m02s)** after automation-only safe-area/candidate changes.
Selection GBP 40, replacement GBP 35 and removal GBP 0 all matched detail/cart/trip totals.
One bounded drag; no payment. Evidence: `TestResults/seats-tc47-safe-area-local-20261007/local.trx`.
Supersedes historical TC47 blockers below; no repeated-run/BrowserStack qualification claimed.

Current hardened shared seat helper requires the outstanding iOS `seatSelection.viewport` ID;
TC43–TC47 need requalification. Rebuilt TC47 verified its first GBP 40 charge/GBP 964.20 total,
then unexpectedly reached Payment during replacement. No replacement/removal pass claimed.

TC47 adds one-adult paid-seat charge/total recalculation on select, different-price replacement
and removal. PR1273 price IDs and two visible distinct paid GBP offers required. Local run failed
at the first price-ID lookup (2m44s); installed app predates commit `c12c922d`. Rebuild/install
before retrying; pricing assertions remain unqualified. Evidence:
`TestResults/seats-tc47-local-20261007/local.trx`. Covers draft price presentation, not backend
submission/reconciliation or payment.

TC46 extends qualification to two adults: automatic active-passenger advance, duplicate-seat
prevention through disabled state, switching retention and independent removal. First local run
reached seats but exposed the app seat-map error before cards appeared (2m09s); seat assertions
were unqualified. No automatic retry. Evidence: `TestResults/seats-tc46-local-20261007/local.trx`.
The user-authorised rerun passed all assertions in 2m37s without further code changes:
`TestResults/seats-tc46-local-rerun-20261007/local.trx`. BrowserStack remains pending; the first
seat-map error's cause is still unknown.
Maps to seat view-model `select`/`deselect`/`isSelectable` and passenger carousel state; no payment.

TC44/TC45 add independent seat assignment lifecycle and details/key cancellation scenarios,
stopping before seat submission/payment (`Suite=Qualification`, not regression). Both passed locally
on 7 October in 1m57s/2m15s; BrowserStack remains unqualified. TC43 reached Payment after
assignment/submission but failed preparation on 7 October.
See [seat coverage](SEATS-COVERAGE.md) for remaining price, availability, multi-traveller/flight and
fulfilment gaps. These are behaviour mappings, not measured Swift coverage.
