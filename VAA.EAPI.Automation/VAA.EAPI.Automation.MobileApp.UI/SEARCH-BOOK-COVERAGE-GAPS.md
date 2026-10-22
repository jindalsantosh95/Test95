# Search and Book behavioural coverage gaps

## Latest source review — 13 October 2026

[Develop impact review and next actions](IOS-DEVELOP-REVIEW-20261013.md) supersedes the dated
positions below where they conflict. Confirmation traveller assertions are updated but not live
qualified. The removed insufficient-points guard requires a product decision. Receipt/details,
payment dismiss/reopen, PWP and retained-seat refresh remain open; seats are no longer out of scope.

## Current status — 30 September 2026

TC41 now has two local passes for the separate backend-first matched-flight/fare baggage comparison.
TC42 extends this to ordered connecting-partner legs and passed locally in 153s,
including two allowance rows and two dimension comparisons.
Both are now enrolled in Search and Book regression (43 scenarios). BrowserStack qualification
remains open. No app context hook or Basket API credential is required.
It does not change TC29's historical dimension-coverage limitation. See
[BACKEND-UI-VALIDATION.md](BACKEND-UI-VALIDATION.md).

Use this table for outstanding work. Sections under **Evidence history** retain earlier
investigations; their old blockers and pending labels are not the current status.

| Area | Current position | Remaining work |
| --- | --- | --- |
| Discount lifecycle, fare cancellation and passenger required-name/Terms gates | TC31–TC35 locally qualified | No new scenario needed for these outcomes |
| Calendar and passenger draft cancellation | TC36/TC37 locally qualified | Journey-type switching PANEL-TC02 remains separately pending iOS requalification |
| Summary Change and Upgrade | TC38/TC39/TC40 locally qualified | Not exhaustive upgrade/price permutations |
| Member not travelling | Loyalty TC04 locally qualified | Country-code reset and loyalty-programme reset not asserted |
| Connecting legs | Extended TC26 locally qualified | Not independent backend truth or every carrier/connection combination |
| Baggage dimensions | TC29 passed locally 30 September (58s): inclusion=2, quantity=0, weight=1, dimensions=0; one Summary row displayed dimensions | Dimension retention NOT EXERCISED. Need dimension-bearing fare or backend evidence correlated to the exact selected offer/basket, not another search |
| Linear baggage limits / full fare rules | Not covered | Agree source data and display contract before assertions; do not invent carrier limits |
| Five-passenger guest checkout | On hold for app investigation | Requalify original TC01 after relevant app fix |
| Pay with Points | On hold during app changes | Revisit selection, apply/remove, totals and completion against revised contract |
| Controlled search recovery / no results | TC30 skipped; no-results data not deterministic | App hook or controlled data needed |
| Controlled payment failure | Explicitly deferred | Resume only when requested |
| Seat selection / reward ledger | Outside this UI gap pass / backend-first respectively | Separate scope and reliable backend correlation needed |

## Evidence history

### TC29 qualification — 30 September 2026

Exact evidence: `TestResults/tc29-baggage-local-20260930/local.trx`; analyser found no failures.
299 offline/retry checks and scoped harness verification passed. Reporting now distinguishes
advertised values actually compared from Summary-only dimension display. Scenario count is unchanged.
Source review: `SearchAndBookFlightSearchServiceAdapter+Brand.makeFeatureGroups` maps Brand
amenities into fare benefits; `SearchAndBookFlightSearchServiceAdapter+Baggage.makeBaggageAllowance`
maps structured Summary baggage; `NativeFlightSummaryBaggageSection.dimensionsDescription`
renders three axes or a linear fallback. These are different data contracts. The live Economy
Light fare provided no expected dimensions, so its Summary dimensions cannot be independently
validated by copying those same values as the expectation. No missing-ID finding is established.

## TC26 per-leg extension — locally qualified, 30 September 2026

The rebuilt isolated iOS branch passed the extended TC26 in 2m17s (2m32s run duration).
All scoped field reads, search endpoints/date, multiple operators, connecting-airport continuity,
ordered Results-to-Summary retention and existing stop/+1 assertions passed. Exact-TRX analysis
found no failures: `TestResults/tc26-legs-local-20260930-rebuilt/local.trx`. No checkout/payment.
This supersedes the installed-build blocker below. It qualifies one local itinerary, not every
carrier/connection combination, Android, BrowserStack or independent backend correctness.

## TC26 per-leg extension — implemented, local qualification blocked, 30 September 2026

The approved iOS branch `feature/search-book-leg-accessibility` now supplies directional
airport/date/time fields. Connection IATA is the value on `NativeFlightDetailsConnection{n}`,
not a separate AirportCode child. Dates expose yyyy-MM-dd; clocks retain their displayed values.
TC26 now reads Results Flight details before fare selection, checks search endpoints/date,
multiple operating carriers and adjacent/connection airport continuity, then compares the ordered
legs including optional descriptors in Summary Flight details. Existing stop/+1 checks remain.
297 offline/retry checks passed. Local execution reached Flight details but failed on missing
`NativeFlightDetailsSegment1OriginAirportCode` after bounded scrolling. The installed binary
also lacked the new destination-code and metadata-type markers; rebuild/install the isolated
iOS branch before retrying. Exact evidence: `TestResults/tc26-legs-local-20260930/local.trx`.
Scenario count remains 41; the old TC26 pass does not qualify the new assertions.
No Android, BrowserStack or backend-truth coverage is claimed.

### Historical blocker (superseded by the implementation above)

Inspected the local iOS action-ID branch and local develop (27482f94; not a fresh remote
develop comparison). The action-ID branch exposes segment containers, Descriptor and
OperatingCarrier IDs. DetailedFlightRowInfoView has no dedicated airport/date/time value IDs;
NativeFlightDetailsView uses showsAirportCodes=false. Do not infer directional airport identity
from child order, airport-name labels or the connection +1 marker.

Proposed iOS/Android-equivalent contract, not implemented: each
`NativeFlightDetailsSegment{n}` (one-based) should expose `OriginAirportCode`,
`DestinationAirportCode`, `DepartureDate`, `ArrivalDate`, `DepartureTime`, `ArrivalTime`
as uniquely identified accessible values scoped to that segment. Airport codes may be accessible
values without changing visual design. Preserve existing Descriptor/OperatingCarrier IDs and
connection containers; expose `NativeFlightDetailsConnection{n}AirportCode` for an unambiguous
connection check. Date values need an agreed format including year; times are local to each
airport, so do not compare them chronologically without timezone/offset evidence.

Once delivered: read all result-details legs before fare selection, verify adjacent airport
continuity and connection airport, then compare every ordered leg with Summary Flight details.
The same observed data proves retention, not independent backend correctness. TC26 remains
unchanged and its previous pass does not cover these new per-leg assertions. No live run for
this extension; app-owned contract work needs approval before implementation.

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

Validation: Release build, 234 offline/retry checks and scoped harness verification passed.
Exact-TRX analyser: no failures (`TestResults/summary-change-offline-20260929-final/offline.trx`).

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

## Companion iOS / Android delivery — 29 September 2026

The isolated iOS branch `feature/search-book-cross-platform-action-ids`, based on refreshed
develop `22694194`, now implements Summary Change/Upgrade action IDs, upgrade title and separate
difference/total-price IDs, member self/not-travelling option IDs with selected traits, and scoped
per-leg descriptor/operating-carrier text IDs. Source implementation is not runtime qualification:
the app must be rebuilt before the pending Appium scenarios can use these contracts.

The iOS-owned `docs/SearchAndBook/native-search-and-book-automation-contract.md` now contains a
**Cross-platform delivery: Summary and member actions** section with Android-equivalent exported
Compose tags/resource-ID guidance, index bases, observable states and acceptance checks. See the
[canonical iOS contract](https://github.com/VAAD1ST/vaa-mobile-ios/blob/develop/docs/SearchAndBook/native-search-and-book-automation-contract.md)
after the companion branch is merged; until then the new section exists only on that local branch.
Its `summary-member-accessibility-plan.md` records scope and verification. Android implementation
has not been changed or qualified. Keep one canonical mapping rather than copying divergent tables.

Detailed route/time fields and independently expected baggage limits remain gaps. The new leg-text
IDs enable scoped observations but do not turn `+1` or label equality into full itinerary validation.
Change-flight tests must follow app invalidation rules: changing outbound may require selecting
inbound again. Do not incorrectly require retention of a deliberately invalidated return selection.

## Refinement and next app contracts — 29 September 2026

Local qualification completed: TC36 **passed, 50s** (`TestResults/cancellation-local-20260929/local.trx`);
TC37 **passed, 26s**, 35.5s including startup (`TestResults/passenger-cancel-qualified-20260929/local.trx`).
TC37 initially failed because the iOS adapter expected `5 travellers` whereas Appium exposed
`Who, 5 people`; the matcher now follows the source-confirmed singular/plural copy and waits for
the summary update. All 219 offline checks passed (`TestResults/cancellation-offline-qualified-20260929/offline.trx`).
No payment was attempted. Appium was stopped after qualification. Android and BrowserStack were not run.

The existing cancellation tests have been renamed, not duplicated:

- PANEL-TC03 → `SearchBook_TC36_TravelDates_EditUpdatesSummaryAndCancelPreservesCommittedDates`.
- PANEL-TC04 → `SearchBook_TC37_PassengerCounts_ConfirmPersistsAndCancelDiscardsDraft`.

Both retain independent classes and shared iOS/Android adapters, and remain `Suite=Readiness`,
`Area=SearchAndBook` until cross-platform qualification. The regression pack remains 37 scenarios.
TC37 now focuses on persistence/cancellation; minimum-count validation belongs to the dedicated
passenger rules test. The accessible decrement-enabled-state defect below is still outstanding,
not fixed or suppressed by deriving enabled state from counts.

The iOS journey picker uses one reviewed compatibility exception: prefer app IDs on native
buttons, otherwise find the exact English segment label **inside** `NativeFlightSearchJourneyTypePicker`.
Verify the resulting selected state. Remove this fallback once the app exposes IDs on actionable
segments, not only their Text children. No unbounded retries or global label lookup are added.

TC29 additionally compares advertised three-axis dimensions (including units) when available in
the selected fare benefit. Offline cases reject changed, missing or wrong-unit dimensions.
This is conditional coverage: linear dimensions, limits absent from fare benefits, and full fare
rules still need an independent expected-data contract. No live dimensions assertion is claimed
until a fare advertising those dimensions has been exercised.

Original required contracts (superseded in part by the companion source implementation above;
runtime qualification and the explicitly remaining gaps are still outstanding):

| Area | Required contract | Intended assertion |
| --- | --- | --- |
| Summary Change | `NativeFlightSummaryItinerary{index}ChangeButton`, stable selected-flight identity | Change one leg; verify new identity/fare/total and preserve the other leg |
| Summary Upgrade | Indexed upgrade option/action IDs, target fare and price adjustment values | Select an offered upgrade; verify target cabin/fare and recomputed summary, not navigation alone |
| Member not travelling | `PassengerDetailsMemberSelfOption`, `PassengerDetailsMemberNotTravellingOption`, selected-state value | Lease a member through Core; select not travelling; verify member data is not reused as traveller data; enter synthetic replacement details |
| Detailed connections | Per-segment operating carrier, flight number, origin/destination, departure/arrival values, available from results and Summary details | Compare every selected leg across screens; retain TC26 stop-count/+1 assertions |
| Full baggage | Category, quantity, weight/unit and dimensions/unit plus independent source expectations | Extend TC29; do not hard-code one airline's allowance across dynamic inventory |

The local iOS source exposes indexed segment containers but not a complete structured per-leg
identity contract. Generic descendants/whole-screen label scraping are not a substitute. App-side
changes require a separate isolated branch; the existing iOS working tree has unrelated user edits.

## Earlier shared panel qualification — 29 September 2026

PANEL-TC03 and PANEL-TC04 both failed locally before reaching their draft-cancellation assertions
(`TestResults/panel-draft-local-20260929/local.trx`, 1m11s total). These are outside the 37-scenario
regression pack; its previously reported consolidated results are unchanged.

- PANEL-TC03: `IosSearchPanelPage.SetRoundTrip` could not resolve
  `NativeFlightSearchRoundTripOption`. The SwiftUI picker declares an ID on its Text option,
  but this did not resolve as an actionable runtime control. The older iOS page uses a label
  predicate for Round trip; this is not proof that the shared adapter's ID contract works.
  Expose distinct IDs and selected states on the actionable native segments before promoting
  the shared scenario, or explicitly review a scoped compatibility exception.
- PANEL-TC04: adult count was one, but its decrement control reported enabled. The source model
  enforces minimum one. `NativeFlightPassengerSelectorView` replaces the Stepper accessibility
  representation with +/- Buttons that do not carry the original disabled states. The captured
  tree reports decrement enabled, including the zero-count child categories. Preserve this as an
  outstanding accessibility defect; mirror actual counter action availability onto the controls. Do not
  infer enabled state from passenger counts inside the automation adapter.

The TRX analyser classified the first timeout generically as service availability; the direct
selector failure is not evidence of a backend outage. No test assertions or iOS code were changed,
no payment was attempted, and the Appium server started for this run was stopped afterward.

## Latest live qualification — 29 September 2026

TC31-TC35 all passed on the configured iPhone 17 Pro/iOS 26.5 simulator, serially, in 5m49s
including session overhead. TC31: 22s; TC32: 24s; TC33: 57s; TC34: 1m02s; TC35: 1m33s.
Evidence: `TestResults/gap-scenarios-local-20260929/local.trx`; exact-TRX analyser: no failures.
No payment was attempted and no test-code repair was needed for TC31-TC35. This supersedes pre-run qualification
notes below. The 37-scenario regression now has 34 consolidated passing results, two unresolved/on
hold and one skipped. BrowserStack and full-pack requalification remain outstanding.

## Scope and evidence — 29 September 2026

This is a prioritised gap register, not a Swift line/branch coverage report. The focused review
used the local iOS checkout at `27482f94` plus its working tree. That checkout contains user-owned
PWP changes; none were edited. This is **not** a fresh audit of remote develop or every Swift branch.
The broader ViewModel inventory in `E2E-COVERAGE-MATRIX.md` remains the 22 September audit.

Source references below are relative to
`Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/` in the iOS repository.

## Acceptance-criterion disposition

### Second implementation pass — 29 September

TC33 implements fare-sheet cabin-change cancellation and verifies unchanged first-result evidence,
then reopens the original cabin. TC34 verifies a nonempty app-owned first-name error and that an
invalid guest cannot save. TC35 saves a valid single guest using the existing shared flow, then
proves Terms off/on/off gates Continue. It never clicks that Continue button. All three have one
concrete class per scenario, normal regression traits and failure evidence; live qualification is pending.

Second-pass verification: Release build and **210 offline/retry checks passed** at
`TestResults/coverage-gaps-phase2-20260929/offline.trx`. The exact-TRX analyser found no failures;
project-scoped repository verification with `-SkipBuild` passed after the build/test run.
This is not simulator or BrowserStack qualification.

The inspected iOS source reveals concrete blockers for the remaining requested interactions:

| Gap | Source evidence / required app contract |
| --- | --- |
| Change flight from Summary | `NativeFlightSummaryItinerarySection.cabinHeader` creates an `OutlineButton` without an action ID. Add a deterministic per-itinerary Change button ID, retaining separate itinerary/carrier/fare/date values. Proposed, not implemented: `NativeFlightSummaryItinerary{index}ChangeButton`. |
| Upgrade from Summary | `NativeFlightSummaryUpgradeSection` has no indexed card/action IDs. Add indexed upgrade card/action IDs plus observable target fare and price adjustment. Proposed, not implemented: `NativeFlightSummaryUpgrade{index}` and `NativeFlightSummaryUpgrade{index}Button`. Results TC22 is not equivalent. |
| Member not travelling | `FlightPassengerDetailsEditor.memberSelector` has a selector ID, but its two menu actions have no IDs. Add separate profile/not-travelling action IDs and retain selected value; avoid giving the same interaction ID to parent and child. Proposed, not implemented: `PassengerDetailsMemberSelfOption` and `PassengerDetailsMemberNotTravellingOption`. |

No iOS changes or placeholder live tests were added for these blockers. Do not use English-label
menus or positional taps to conceal missing contracts. Existing PANEL-TC02/03/04 are unchanged;
TC02 stays explicitly skipped until its qualification gate is met. Connection carrier identities
and baggage dimensions remain partial contracts requiring a separate source/evidence review.

| Behaviour / source | Existing coverage | Decision and next evidence required |
| --- | --- | --- |
| Discount draft syntax and trim: `UI/NativeSearchEntry/NativeFlightSearchEntryViewModel+DiscountCode.swift`, `isDiscountCodeDraftValid`, `applyDiscountCode` | TC18 only checks a valid promotion's actual fare discount | **New TC31**: empty and two-character codes, punctuation/internal spaces, minimum three characters, surrounding-space trim. App-owned Clear/Cancel IDs already exist in `NativeFlightDiscountCodeView.swift`. Implemented; live qualification pending. |
| Discount commit/cancel/replace/clear: same extension, `showDiscountCodeSheet`, `clearDiscountCode`; entry `dismissPresentedSheet` | Android has its own panel-state test; no equivalent iOS regression | **New TC32**: committed value survives a cancelled draft; replacement persists; clear restores empty state. Implemented; live qualification pending. No search is sent with synthetic QA codes. |
| Calendar/passenger draft cancellation and journey switching: `UI/NativeSearchEntry/NativeFlightSearchEntryViewModel.swift` | PANEL-TC02/03/04 already implement these; TC02 is explicitly skipped pending iOS qualification | **Requalify existing tests**, do not add duplicate iOS scenarios. Promote only after their current app contracts pass; currently outside regression. |
| Fare-sheet dismissal without committing: `UI/NativeSearchResults/NativeFlightSearchResultsViewModel.swift`, `dismissFareSelector` | TC22 checks upgrade navigation | **New TC33 implemented**: switch displayed cabin without selecting a fare, dismiss, compare original result text and reopen original cabin. Live qualification pending. |
| Change flight / upgrade from Summary: `UI/NativeFlightSummary/NativeFlightSummaryViewModel.swift`, `changeFlight`, `upgradeFlight` | TC22 upgrades in the results fare selector, not from Summary; TC24-26 check initial Summary retention | **Next focused addition**: choose a distinct offered flight/fare and assert updated Summary plus retained other leg on return journeys. No extra checkout required. Must assert observed identity/price, not that navigation merely succeeds. |
| Member not travelling: `UI/PassengerDetails/FlightPassengerDetailsViewModel.swift`, `selectMemberTravelling(false)` | Loyalty paths deliberately assume the sole member is travelling | **Missing path**: leased member, explicitly select not travelling, verify editable traveller state and synthetic replacement data. Reuse Core data providers and stop before payment. Inspect current selector/value contract first. |
| Passenger invalid fields / unaccepted Terms: passenger view-model validation and hub `canContinue` | Guest/member journeys cover successful completion | **New TC34/TC35 implemented**: first-name error/save blocking and Terms gating with valid guest data. Source-confirmed IDs; live qualification pending. Other field validation permutations remain backend-first. |
| Connecting-flight per-leg carrier identity | TC26 checks stop count, primary carrier and retained `+1`, not all additional carrier names | **Partial**: add scoped Flight details evidence when a full deterministic per-leg ID/value contract is available. Do not equate the `+1` marker with identity validation. |
| Baggage dimensions / full fare-rule details | TC29 covers inclusion/exclusion and advertised count/weight | **Partial**: dimensions and fare-rule content remain outside its assertions. Require app-owned structured values and agreed display rules; do not scrape generic labels or duplicate all backend Brand permutations. |
| No-results, controlled search failure/retry | TC30 is skipped pending a fail-once hook | **Blocked on deterministic data/hook**: avoid selecting an arbitrary route and assuming zero inventory or inducing uncontrolled network failures. |
| Controlled payment failure | Incidental failure detection exists, but no deterministic negative journey | **Deferred by user**. Failure logging is not a passing controlled-failure test. |
| Five-passenger guest checkout | TC01 implemented; fails at passenger hub | **On hold for app fix**, not a new-test gap. Requalify original scenario after rebuild; do not mask it with retries or smaller passenger counts. |
| PWP apply/remove, totals and completion | Loyalty TC03 currently selects an option only and fails verification | **On hold during implementation changes**. Reassess the new app contract before expanding it. |
| Actual seat selection | Startup profile skips seats; optional bypass exists | **Out of current delivered scope**. Do not count bypass as selected-seat coverage. |
| Reward points ledger reconciliation | Native redemption now completes through confirmation/Trips | **Backend-first**: reuse shared account/basket/ledger capabilities when a trustworthy native-to-backend correlation contract is available; visible confirmation alone is not proof of deduction. |

## Why these first two additions

TC31/TC32 cover separate local UI contracts, have source-confirmed accessibility IDs, need no
account leasing, live inventory, HPP or booking creation, and should be short to execute. Actual
runtime is unmeasured until local qualification. They keep the same page object, failure evidence,
session lifecycle, unique IDs and one-class-per-scenario conventions as existing regression.

The API suite already has `OfferPriceGraphQl_TC14_ReturnTrip_PromoCodeAppliedInSummary` and shared
promo-aware search/price helpers. The new UI cases do not duplicate promo entitlement matrices.
TC18 retains the one representative £5 end-user price assertion.

## Reporting rules and next sequence

Verification on 29 September: Release build and all **198 offline/retry checks passed**
(`TestResults/coverage-gaps-20260929-final/offline.trx`); the exact-TRX analyser found no failures.
Project-scoped repository verification with `-SkipBuild` passed after the separate build/test run,
including new-file formatting. No live Appium scenario ran during this change, and no app source,
populated settings, dependencies or remote PRs were modified.

1. Build and run offline contract/retry checks, then the project harness.
2. With explicit live-run authorisation, qualify TC31 and TC32 locally; do not count them as passing beforehand.
3. Requalify existing PANEL-TC02/03/04 rather than copy them into new tests.
4. Inspect Summary change-flight/upgrade and fare-dismissal contracts, then add focused state-retention checks.
5. Follow with member-not-travelling and representative passenger validation once their app contracts are agreed.

The 37-scenario functional regression inventory now comprises 34 with passing evidence,
two unresolved/on hold and one explicitly skipped. The complete suite
has not been rerun after these additions. Visual/accessibility checks are separate evidence layers.
No new code-coverage percentage is justified by adding two cases inside an already mapped component.
