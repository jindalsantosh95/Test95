# Mobile Automation Test Catalogue

## PayPal — TC57 readiness, not live-qualified

`SearchBook_TC57_GuestPayPalSandbox_CompletesBookingAndAppearsInTrips`:
guest LHR–JFK Economy Classic, Review checks, Core Vault credentials, PayPal sandbox login,
approve once, then require confirmation/travellers/booking reference and Trips using shared checks.
Explicit local SIT/privacy/payment opt-ins; BrowserStack blocked. Confirmation is a known
user-reported blocker: the test must fail there, not pass just because login succeeded.
See [PayPal scope and safe execution](PAYPAL-COVERAGE.md). Release-pack counts unchanged.

## Planned coverage — awaiting requirements, not an executable test

US-origin Economy Delight free-seat entitlement (representative JFK–LHR/USD journey):
**not implemented or qualified**. Exact seat eligibility and acceptance criteria have been
requested from the PO/developer. TC23 covers Economy Classic/USD, not this entitlement.
See [requirements and proposed assertions](SEATS-COVERAGE.md#pending-requirements--us-origin-economy-delight-free-seats).
No test ID assigned; exclude from Sanity/Regression and passing/executable totals.

## Review navigation — prepared, awaiting qualification

`SearchBook_TC56_GuestReviewBackNavigation_ReentryRetainsTotalAndPaymentReadiness`
uses one guest adult, LHR–JFK Economy Classic and the existing skip-seat flag. Checks
Review, returns to passenger details, continues without refilling, and checks the retained
total and payment readiness again. Stops before HPP. `Suite=Readiness`, not Regression or
Sanity until qualified; existing release-pack counts remain unchanged.

## Pre-HPP Review total — new shared assertions

Complete guest/member booking callers now validate Review total before HPP. See
[the detailed coverage map](REVIEW-TOTAL-COVERAGE.md) for arithmetic, retained currency,
reward points, optional seat charges and explicit exclusions. This adds assertions to
existing journeys, not new scenarios. Live qualification pending; previous passes do
not cover these new assertions. Regression remains 54; Sanity 14.

## Confirmation warning preparation — 16 October 2026

Six Draft fixture cases are prepared: W001, W002, W003, W001+W003, W002+W003 and no warnings.
Each fixture includes the exact backend message. Combined cases expect both messages and the
control expects none; confirmation must remain visible. Message order is not prescribed.
These are not runnable Appium tests and do not increase the live scenario/coverage totals.
The eleven existing controlled-failure scenarios and release pack membership are unchanged.
See [warning outcomes and activation checklist](../docs/MOBILE_FAULT_INJECTION.md#prepared-confirmation-warnings--16-october-2026).
Release build and 539 offline/retry checks passed (11 new fixture/contract checks), with no
live session or payment. Backend-message fixture revalidation also passed all 539 checks:
`TestResults/confirmation-warning-messages-20261016-final/offline.trx`.

## Regression investigation — 15 October 2026

The [14 October FullRegression run](https://github.com/VAAD1ST/eapi/actions/runs/38603002304)
executed 48 tests: 40 passed, eight failed, none skipped. This is historical run evidence,
not qualification of the following automation changes:

- TC05 now checks the inclusive 330-day boundary and disabled day 331, matching the iOS
  `NativeFlightSearchEntryViewModel.maximumTravelDate` contract. The previous one-year
  expectation was incorrect. Missing day controls still fail explicitly.
- TC32 failed during guest setup before its promo assertions. Guest and login paths now
  share state-verified Account navigation, with at most two recorded recovery taps when
  the destination fails to appear. No whole-test, credential, payment or transport retry.
- TC12 lost its WebDriverAgent connection (`socket hang up`); underlying cause unconfirmed.
- TC44/TC45 encountered the app-owned seat error before seat interaction. Cause unconfirmed;
  HTTP 200 alone does not establish GraphQL success.
- TC42 remains under backend investigation. TC50 is a user-confirmed known product bug;
  Loyalty TC02's required behaviour has not yet been released.

Validation: Release build and **525 offline/retry checks passed** on 15 October, including eight
Account-navigation guardrail cases. TC05's local attempt stopped before calendar checks because
Account displayed `Something went a little off course`. Evidence:
`TestResults/tc05-boundary-local-20261015/local.trx` beneath the Automation solution.
The follow-up guard now fails immediately on that visible error rather than re-tapping Account;
it is offline-tested, not live-qualified. TC32 was held because it shares this blocked startup path.
TC05 and TC32 local requalification remains pending. Test names and release-pack membership are unchanged.

PR review follow-up: readiness-wait expiry now has a dedicated exception; command timeouts
inside polling (including privacy dismissal) propagate unchanged and cannot trigger a re-tap.
**528 offline/retry checks passed**, including three additional poll/expiry/success cases,
with exact-TRX analysis reporting no failures:
`TestResults/account-readiness-review-20261015/offline.trx`. No additional live run.

## Opt-in fault injection — 14 October 2026

Expanded pack (all `Readiness=Draft`, sequential, no release-pack membership):

| Test | Source / assertion | Qualification |
| --- | --- | --- |
| `SearchBookFault_TC01_ControlledSearchError_ShowsErrorAndAllowsSearch` | 3001; exact search error and enabled Search | BrowserStack passed 14 October |
| `SearchBookFault_TC02_SearchUnavailable_ShowsErrorAndAllowsSearch` | 3002; generic app error and enabled Search | BrowserStack passed 14 October |
| `SearchBookFault_TC03_NoAvailabilityError_StopsSearchAndAllowsCorrection` | 101682; current generic app error, not dedicated empty state | BrowserStack passed 14 October |
| `SearchBookFault_TC04_SupplierSessionMissing_ShowsErrorAndAllowsSearch` | 100600; supplier session, not B2C login expiry | BrowserStack passed 14 October |
| `SearchBookFault_TC05_BasketUnavailable_StaysOnSummaryWithoutOpeningPassengers` | 2004; exact summary error, enabled Continue, no passenger navigation | BrowserStack passed 14 October |
| `SearchBookFault_TC06_SearchRequestTimeout_StopsLoadingAndAllowsSearch` | Withhold search response for 150 seconds | Local pass 14 October on provisioned simulator (3m14s); BrowserStack failed, cause unconfirmed, Appium transport errors also observed |
| `SearchBookFault_TC07_PostHppPaymentStatusFailure_ShowsErrorWithoutConfirmation` | Status-update GraphQL error; dedicated non-retryable page | 19 Oct local PASS (2m49s), iOS PR1331 `0dd5d294`: injection, exact title/message, enabled owned Return action, no Retry/confirmation. Return not tapped. Draft; separate payment opt-in; BrowserStack pending |
| `SearchBookFault_TC08_PostHppBookingFailure_ShowsErrorWithoutConfirmation` | Order GraphQL error; no confirmation or retry | Implemented; not live qualified; separate payment opt-in |
| `SearchBookFault_TC09_PostHppBookingTimeout_StopsLoadingWithoutConfirmation` | Order-response hold; bounded error state | Implemented; not live qualified; separate payment opt-in |
| `SearchBookFault_TC10_OutboundSeatMapFailure_ShowsReassuranceAndOpensInboundMap` | Outbound seat-map GraphQL 500; all three approved messages in owned error group, no Retry; Continue opens inbound map | Passed locally 16 October with confirmed injection; BrowserStack unqualified; no seat selection or payment |
| `SearchBookFault_TC11_SeatSaveFailure_ShowsErrorWithoutOpeningPayment` | Seat-save GraphQL 500; exact error, enabled Continue, retained draft assignment, no payment | Implemented; not live qualified; no retry or payment |
| `SearchBookFault_TC12_InboundSeatMapFailure_ShowsReassuranceAndContinuesToPayment` | Unsupported outbound plus inbound seat-map GraphQL 500; all three approved messages in owned error group, no Retry; Continue reaches payment | Passed locally 16 October with confirmed injection; BrowserStack unqualified; no payment action |

See the [error coverage matrix and outstanding gaps](../docs/MOBILE_FAULT_INJECTION.md#expanded-error-coverage).

The table above is the authoritative qualification status. TC01 also passed the original local
POC on 14 October 2026 using exact-process capture on a disposable iPhone 17 Pro / iOS 26.5
(1m17s including lifecycle), followed by approved BrowserStack execution. The earlier
tunnel-only/awaiting-approval status is superseded. Repeated-run stability remains unqualified;
all cases retain Draft readiness while app error handling is being agreed and remain outside
Sanity/Regression. See [setup and boundaries](../docs/MOBILE_FAULT_INJECTION.md).
TC30 successful recovery remains deferred.

## Latest source compatibility — 13 October 2026

See [the develop impact review](IOS-DEVELOP-REVIEW-20261013.md) for source SHA and open work.
TC01/TC03/TC43/TC48 and Loyalty TC01 now share exact confirmation count/name assertions;
live requalification is pending. Loyalty TC02 retains its expected warning/disabled Continue,
which iOS #1277 removed: known contract mismatch, not a waived release check. No test was added,
removed or retagged. Receipt/details, payment dismissal, PWP and refreshed-seat gaps remain open.

Release selection: [14-test Sanity / 54-test Regression](RELEASE-PACKS.md).

**TC53 — 20 October:** MAN–MIA one-way, adult and child, Air France-led Economy Standard.
Verify every leg is unsupported; expect direct payment navigation without guidance or
seat actions. Local retest passed in 3m25s (3m35s total), with Air France Economy Standard.
No payment attempted. TRX: `TestResults/tc53-man-mia-live-20261020/tc53.trx`;
analyser found no failures; 548 offline checks and scoped harness passed.

**TC52 latest fare rule:** Virgin Economy Classic inbound, never Light; connections remain
dynamic. Every outbound leg is verified unsupported. Both cabin/fare names are logged.
No unqualified Delta fare fallback. Local retest **passed** (2m45s test / 3m01s total):
KLM Economy Light outbound (verified unsupported legs), Virgin Economy Classic inbound,
and child guidance displayed. No seats submitted or payment attempted.
TRX: `TestResults/tc52-classic-live-20261020/tc52.trx`; analyser found no failures.
548 offline checks and scoped harness passed. Supersedes the fare-free rule below.

**TC52 current selection:** unsupported outbound (every leg verified), then any inbound
containing a verified Virgin or Delta leg. No fixed airport connection, stop count,
carrier combination or fare title. This supersedes the restrictive itinerary below.
Local retest selected both flights and completed passenger details, but failed because
payment opened directly instead of seat selection/guidance. No payment attempted;
eligibility/configuration/app-handling cause remains unconfirmed.
TRX: `TestResults/tc52-dynamic-live-20261020/tc52.trx` (2m53s total);
548 offline checks and the scoped harness passed.

**TC52 — 20 October:** changed to YYZ–LHR return, requiring an Air France-led outbound
with two explicitly unsupported AF/KLM legs and Virgin LHR–JFK / Delta JFK–YYZ inbound.
Flight-details scrolling now recovers an overshot candidate. Retest selected the outbound,
but failed return selection (no Virgin cabin attempted) after 6m49s total. Inventory versus
selection cause remains unconfirmed; guidance was not reached.
Current TRX: `TestResults/tc52-scroll-live-20261020/tc52.trx`; 551 offline checks and scoped harness passed.
The previous local retest failed after 2m07s locating
`NativeFlightResult4FlightDetails`, before fare selection and proximity checks. Cause
unconfirmed; not a proximity-regression result. TRX: `TestResults/tc52-yyz-live-20261020/tc52.trx`.
547 offline checks passed, including eight new itinerary-matching cases; scoped harness passed.
That guide is the current pack inventory, with exact names and exclusions; historical suite
labels in dated notes below are not the current selection policy.

## Current status — 12 October 2026

This table is the current handover. Dated notes below are retained as evidence/history,
not competing current statuses. A targeted pass does not establish a clean full-suite run.
No fresh full regression against the latest iOS build or BrowserStack is claimed.

| Area / scenarios | Latest established position | Next action |
| --- | --- | --- |
| Framework / offline checks | 13 October: 470 offline/retry checks passed, including no-hold calendar gesture and bounded overshoot correction; project-scoped harness passed | BrowserStack qualification of the new gesture pending |
| Search, filters, sorting, dates, Summary, fares/baggage | Targeted local passing evidence; 13 October TC36 cross-month edit/cancel passed twice consecutively (`calendar-local-edit-20261013-b/local.trx` and `-c/local.trx`); 470 offline checks passed | Full regression on a pinned build; no full-pack claim |
| TC41/42 backend-to-UI baggage | Locally passed; included in Regression, also tagged `Suite=BackendRegression` | Enable backend settings and runner connectivity when selecting this pack |
| Loyalty TC02 insufficient points | 13 October local run passed calendar/date assertions and outbound/inbound fare selection, then failed waiting for `NativeFlightSummaryInsufficientPoints`; supersedes the historical local pass | App/product clarification for removed warning/Continue restriction; do not waive the assertion |
| Loyalty TC01 redemption | Passed through payment, confirmation and Trips on 29 September | Requalify on current build with full regression |
| Guest TC01 five mixed passengers | 12 October: all five saved with green ticks; Terms navigation timed out re-finding PassengerDetailsHub. Native swipe experiment reproduced the snapshot stall and was reverted | Investigate overview accessibility snapshot/gesture interaction; no payment reached, no confirmed backend cause |
| Loyalty TC03 Pay with Points | Paused pending app redesign; last recorded selector validation did not pass | Requalify after app delivery |
| TC44/45/46 seat assignment, cancellation and two adults | Local passes recorded; intermittent app seat-map errors also observed | Repeated-run / remote qualification |
| TC47 seat pricing | Local pass; latest measured total about 3m32s | Retain exact price assertions; remote qualification pending |
| TC49 lap infant | Historical local pass: adult is the only independently assignable passenger; source now also requires linked `+ infant` text | New text assertion needs an iOS #1311 build; infant restrictions remain separate |
| TC43 booking with seats | 9 October: failed after seat flow at payment preparation Retry (2m52s), consistent with #177; no payment submission | App/downstream investigation; no retry bypass |
| TC48 booking without seats | Passed on main on 12 October: payment, confirmation and exact booking reference in Trips (4m11s run) | Enrolled in Sanity/Regression; BrowserStack qualification pending |
| TC50 unsupported carriers | Passed locally on 9 October (4m51s): unsupported itinerary bypassed seats; same-session Virgin control opened seats | Enrolled in Regression; one pass does not close intermittent issue #179 or establish remote stability |
| Child proximity TC51–54 | Implemented display/absence checks for Virgin, unsupported-outbound/Virgin-inbound, all-unsupported and direct Delta eligibility | Enrolled in Regression; duplicated sheet ID is an expected visible failure until fixed, and exact copy/action IDs remain open |

TC44–54 are enrolled in `Suite=Regression`; TC51–54 intentionally expose the current
accessibility failure in release results. As of 19 October, TC43 is also enrolled in
Sanity/Regression; its historical payment failure still needs live requalification.
TC30 and Loyalty TC03 are explicitly `Suite=Deferred`, not green release coverage.
Passing an isolated seat interaction is not proof of completed seat fulfilment.
These are iOS results, not Android qualification. Coverage here is
behavioural mapping, not measured Swift line/branch coverage.

TC51–54 are independent iOS qualification scenarios. TC51 proves the owned child-guidance
sheet displays for an eligible Virgin child booking. TC52 proves an explicitly unsupported
outbound does not suppress guidance for an eligible Virgin inbound. TC53 proves an all-unsupported
child itinerary bypasses both guidance and seats. TC54 reflects the Develop rule that direct
DL-operated Economy is eligible and therefore displays guidance. All stop before payment and none
claim live passing evidence. An eligible outbound whose retrieved map resolves unavailable before
an eligible inbound remains blocked on deterministic data. Exact copy checks require app-owned IDs
on the title and two body messages; the current tests do not locate English strings directly.

| ID | Test method | Qualification assertion |
| --- | --- | --- |
| TC51 | `SearchBook_TC51_AdultAndChildVirginOneWay_DisplaysSeatProximityGuidance` | Eligible Virgin child journey displays the owned guidance sheet |
| TC52 | `SearchBook_TC52_UnsupportedOutboundEligibleInbound_DisplaysGuidanceForFirstEligibleSector` | Every outbound leg is unsupported; guidance displays for an inbound with a verified Virgin or Delta leg |
| TC53 | `SearchBook_TC53_AdultAndChildUnsupportedOneWay_BypassesGuidanceAndSeats` | All-unsupported child journey reaches payment without guidance or seats |
| TC54 | `SearchBook_TC54_AdultAndChildDirectDeltaOneWay_DisplaysSeatProximityGuidance` | Direct DL-operated journey uses an offered Delta Main fare and displays guidance |

### Latest run evidence

- TC01 five passengers, 12 October: `TestResults/tc01-five-passengers-20261012/local.trx`
  failed in `AcceptBookingTerms` while re-resolving `PassengerDetailsHub` after scrolling.
  Screenshot showed all five synthetic travellers saved. Subsequent iOS accessibility
  snapshots stalled, including diagnostic capture; no backend failure was established.
- Native swipe comparison: `TestResults/tc01-hub-swipe-20261012/live/local.trx`.
  The element-scoped XCTest swipe returned, but the following hub snapshot stalled again.
  Experimental overview gesture changes were reverted; no iOS code was changed.
  Five-traveller orchestration checks were retained (429 offline checks passed).

- Release pack contract validation, 12 October: `TestResults/release-packs-20261012/offline.trx`
  (427 passed, zero failures/skips). No new live app sessions for trait changes.
- TC48 on main, 12 October: `TestResults/tc48-main-e2e-20261012/local.trx`
  (passed, 4m11s). Complete one-adult guest no-seat payment, confirmation and Trips.

- Review fixes, 12 October: `TestResults/hpp-review-20261012/offline.trx`
  (423 passed, no failures/skips). Six additional cases protect missing-value Text fallback
  without hiding an empty, placeholder or incorrect native value. No live booking rerun.

- Latest-main PR validation, 12 October: `TestResults/mobile-hpp-pr-20261012/offline.trx`
  (417 passed, no failures/skips). Exact-run analysis and scoped repository verification passed.
  Live scenario dates below remain 9 October; offline validation does not refresh live evidence.

- Latest TC48: `TestResults/tc48-hpp-hardened-20261009/local.trx` (passed, 3m27s run).
  Keyboard dismissal and full-screen target checks prevented typing into the previous field;
  retained billing values were checked before one Pay action. Payment, confirmation and Trips
  passed, live-validating the VoiceOver-spaced reference matcher. Exact-run analysis found no failures.
  No iOS/HPP code changes. The final equivalent shared string-helper substitution was validated
  offline rather than creating another booking solely for a convention change.

- Later standalone TC48 retry: `TestResults/tc48-rerun-20261009/local.trx` (failed).
  Expected Town / City value `Crawley`; reader returned placeholder `Town / City`.
  Failure at 16:06:17 UTC. No payment submission or confirmation; exact-run analysis
  identifies HPP/UI as the investigation area, not a proven backend failure.
  The previous fare-selection error did not reproduce on this retry.

- Trips matcher/offline: `TestResults/mobile-trips-fix-20261009/offline.trx` (406 passed).
- Requested local TC43/48/50 recheck: `TestResults/mobile-booking-recheck-final-20261009/local.trx`
  (TC50 passed; TC43 and TC48 failed, zero skipped; runner duration 10m17s including startup).
  Exact-run analyser completed: both failures classified as unclassified/manual review.
  Visible app error states are captured; a correlated backend cause has not been established.
  No test payment was submitted in this run.
- An initial run was stopped during startup because the updated assembly was still building;
  it is not qualification evidence. The final run uses the completed build.

Failure screenshots and sanitised logs are linked in the TRX output. TC48 failed at
15:36:44 UTC, TC43 at 15:39:46 UTC on 9 October. The former screenshot shows the
fare-selector error; the latter shows payment Retry. The exception's legacy wording
"Partner cabin" does not establish a partner carrier: TC48 selected a Virgin itinerary.
TC48's matcher was not reached in that earlier three-test run; the later standalone pass above
now qualifies it locally. No assertions were bypassed to pass the booking.

## Detailed inventory and dated execution history

The following notes preserve previous investigations, dependencies and historical counts.
Use the current-status table above where an older note conflicts with a later result.

Family/carrier seat expansion is tracked in
[SEATS-FAMILY-ELIGIBILITY-PLAN.md](SEATS-FAMILY-ELIGIBILITY-PLAN.md).
No new executable scenarios or regression counts are added by that plan.

## TC48 — one-adult booking without seats (7 October 2026)

`SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips`
is a separate iOS Qualification class. Virgin LHR-JFK, three months ahead, Economy Classic,
one guest adult; skip-seat launch flag enabled. Reuses checkout, confirmation and Trips.
Pending live result; does not replace or requalify mixed-passenger TC01, whose latest
run was cancelled after stalling. TC43 is the matching criteria comparison with seats.

Latest readiness check (7 October): TC45 passed after shared active-passenger readiness was added.
TC44 stopped on the app's explicit seat-map error; no longer a premature missing-element failure.
TC46 passed in the preceding remaining-pack run. TC43 remains blocked by payment preparation Retry.
Evidence and limitations: [seat coverage](SEATS-COVERAGE.md).

Latest TC47 local performance check (7 October): PASS, 3m32s including lifecycle, versus 5m59s.
Same paid selection/replacement/removal assertions; no submission/payment. See
[seat evidence and boundaries](SEATS-COVERAGE.md). Other seat scenarios were not rerun in this check.

## Seat journey qualification — 6 October 2026

`SearchBook_TC43_OneAdultGuestSeatSelection_CompletesBookingAndAppearsInTrips`
is a separate iOS class, initially `Suite=Qualification` (not regression). It reuses guest
checkout, requires the seat map, selects an enabled offered seat, confirms its detail sheet
and verifies the exact seat against passenger zero before payment, confirmation and Trips.
Local attempt 6 October 2026: **not passed**. With skip disabled and Economy Classic selected,
the app reached the seat map and the test tapped a visible enabled seat, then timed out locating
`seatSelection.selectSeat` in the seat-detail sheet. No seat confirmation/payment/booking
completion was performed. Evidence: `TestResults/seats-classic-local-20261006/local.trx`
(343 offline checks passed, one live scenario failed; 3m16s total).
Earlier Light-fare attempts bypassed seats; develop explicitly excludes Economy Light.
Follow-up diagnosis confirmed both visible sheet buttons expose `seatSelection.detailSheet`,
masking the confirm/close IDs. The diagnostic-only run had 344 offline passes and one live
contract failure: `TestResults/seats-contract-diagnostic-20261006/local.trx`.
See [the code-to-test seats matrix](SEATS-COVERAGE.md) for the accessibility repair and planned pack.
No post-payment seat-fulfilment, multi-sector, Android or
BrowserStack qualification is claimed.

## Latest one-adult guest smoke — 6 October 2026

Passed locally on iPhone 17 Pro / iOS 26.5 in 3m33s: LHR-JFK search, Virgin selection,
Summary, passenger details (native gender menu), hosted test payment, confirmation and booking
presence in Trips. The run used a temporary one-adult TC01 variant; original five-passenger
source was restored. This does not requalify the five-passenger scenario or full regression.
Evidence: `TestResults/guest-payment-entry-smoke-20261006/local.trx`; exact-TRX analysis found
no failures. All 338 mobile offline/retry checks passed. Formatting passed; the wider harness
has an unrelated stale context-routing metadata check (33 days; maximum 30).

## Backend-first baggage regression — opt-in pack, 8 October 2026

`SearchBook_TC42_ConnectingPartnerSummaryBaggage_MatchesBackendSelectedLegsAndFare`
is a separate iOS regression class in the same BackendValidation category. It matches both
ordered legs and the connection airport, the exact fare and itinerary-level baggage for a
one-stop itinerary with at least one partner operating carrier. Passed locally on 30 September
2026 in 153.037s: two allowance rows and two dimension comparisons passed.
Evidence: `TestResults/tc42-local-20260930/local.trx`. BrowserStack qualification remains pending.
TC41/TC42 are included in Regression; `Suite=BackendRegression&RegressionArea=SearchAndBook`
remains available as a diagnostic subset filter.
The default regression pack excludes these two opt-in cases. Historical run totals below
are unchanged, not a new full-pack result.

### TC41 — two local passes, 30 September 2026

`SearchBook_TC41_OneWaySummaryBaggage_MatchesBackendSelectedFlightAndFare`
is an independent iOS class in `Category=MobileApp-SearchBook-BackendValidation`,
`Suite=BackendRegression`, `RegressionArea=SearchAndBook`. It compares one-way Summary rows against the
independently prepared backend flight/fare summary, requiring a dimension comparison. No checkout.
Both scenarios retain their BackendValidation category for targeted execution and require
`BackendValidation:Enabled=true` plus backend network access on the .NET runner.
See [runner configuration and safeguards](BACKEND-UI-VALIDATION.md).

## TC40 return Summary Change — locally qualified, 30 September 2026

`SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal`
is a separate iOS SearchAndBook regression class. It changes outbound and then explicitly selects
a different inbound schedule, verifies both scoped Summary itineraries and exact fare titles,
and checks the total using the observed base price and signed inbound adjustment/full total.
Requires two selectable Virgin schedules in each direction; does not book or pay. Inventory is
now 41 scenarios: 38 with passing evidence, two on hold and one skipped.

Passed locally on iPhone 17 Pro/iOS 26.5 in **2m54s**:
`TestResults/tc40-local-20260930-inboundcontract/local.trx`.
The same run passed **283 offline/retry checks** (284 total, zero failures/skips).
Release build, exact-TRX analysis and project-scoped harness verification passed.
The initial run incorrectly expected an inbound flexible-date carousel; iOS only exposes that
carousel outbound. The corrected test verifies the inbound date in its scoped Summary instead.
Android/BrowserStack and repeated-run reliability remain unqualified. No checkout/payment.

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

Validation: Release build and **261 offline/retry checks passed**, with no failures reported by
the exact-TRX analyser (`TestResults/member-not-travelling-offline-20260929/offline.trx`).
Project-scoped harness verification also passed. No account login or Appium session was run
for this implementation; the functional scenario still requires local qualification.

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

Validation: Release build and 250 offline/retry checks passed; exact-TRX analysis found no failures
(`TestResults/summary-upgrade-offline-20260929/offline.trx`). Project-scoped harness verification
also passed. These checks do not qualify Appium interaction with the rebuilt app.

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

TC38 implementation validation: Release build and **234 offline/retry checks passed**;
exact-TRX analysis found no failures in
`TestResults/summary-change-offline-20260929-final/offline.trx`.
Project-scoped `scripts/verify.ps1 -ProjectPath ... -SkipBuild` passed. No TC38 Appium
session was run for this implementation; live qualification remains pending.

Latest addition: `SearchBook_TC38_OneWaySummaryChange_RetainsReplacementFlightFareAndPrice`
is implemented, pending live qualification. It selects one adult LHR-JFK, uses the iOS PR #1244
Summary Change action and chooses another offered Virgin flight with a different displayed
departure/arrival schedule. It checks replacement route, carrier, schedule, exact fare title,
currency amount and enabled Continue, then stops before checkout. Two distinct offered schedules
are required; insufficient inventory fails explicitly rather than silently reselecting the original.
This adds one independent regression class: **38 Search and Book regression scenarios**, with
34 previously passing, one newly unqualified, two unresolved/on hold and one skipped.
Return outbound invalidation/inbound reselection, Summary Upgrade and member-not-travelling remain
gaps. Android action IDs remain proposed, not qualified. TC24-26 passed locally on 29 September
in 5m44s (`TestResults/summary-pr1244-local-20260929-1543/local.trx`). Older totals below are historical.

Latest shared-panel qualification (29 September): the existing PANEL-TC03/04 scenarios are now
TC36/37, not duplicates. TC36 date edit/cancellation passed locally (50s), and TC37 passenger
confirmation/cancellation passed locally (26s). TC36 uses a scoped journey-picker compatibility
adapter; TC37 matches the iOS summary's `people/person` wording and leaves minimum-count rules
to the dedicated validation scenario. The decrement-enabled-state accessibility defect remains open.
Evidence: `TestResults/cancellation-local-20260929/local.trx` (TC36 pass; earlier TC37 failure),
`TestResults/passenger-cancel-qualified-20260929/local.trx` (TC37 pass).
Both remain outside regression in `Suite=Readiness`, `Area=SearchAndBook` pending cross-platform
requalification; the 34/37 consolidated regression figure below is unchanged. Offline: 219 passed.

## Current consolidated position — 29 September 2026

This section supersedes older execution summaries below. The previous 32-scenario Search and
Book regression has 29 passing results, guest TC01 awaiting the app-side passenger-hub fix,
Loyalty TC03 investigation on hold during PWP changes, and TC30 explicitly skipped.
Neither on-hold investigation is automatically skipped in code. Redemption Loyalty TC01 passed
through HPP, confirmation and Trips on 29 September in 3m12s
(`TestResults/redemption-diagnostics-20260929/redemption.trx`). PWP still failed selected-value
verification (`TestResults/pwp-local-20260929/pwp.trx`); no payment was submitted.

New TC31-TC35 bring Search and Book regression to **37 scenarios**: 34 with passing evidence,
two unresolved/on hold and one skipped. They are
included in regression and have independent classes. TC31/32 are panel-only; TC33 searches;
TC34/35 select a fare and open passenger details but stop before passenger submission or payment.
See [the gap register](SEARCH-BOOK-COVERAGE-GAPS.md) for exact source scope and remaining work.

| ID | Test method | Coverage | Status |
| --- | --- | --- | --- |
| TC31 | `SearchBook_TC31_DiscountCode_RejectsInvalidDraftsAndTrimsAppliedCode` | Empty/short/invalid-character validation, three-character boundary and trimmed applied value | Passed locally 29 September 2026 |
| TC32 | `SearchBook_TC32_DiscountCode_CancelPreservesAppliedCodeAndClearRemovesIt` | Apply/reopen, cancelled edit, replacement and clear/reset | Passed locally 29 September 2026 |
| TC33 | `SearchBook_TC33_FareSheetCancellation_PreservesResultAndOriginalCabinOnReopen` | Change displayed cabin without selecting a fare; cancel retains first-result evidence and original cabin on reopening | Passed locally 29 September 2026; needs two displayed cabins |
| TC34 | `SearchBook_TC34_MissingPassengerFirstName_ShowsErrorAndPreventsSave` | Required first-name error and blocked save on redesigned guest editor | Passed locally 29 September 2026 |
| TC35 | `SearchBook_TC35_ValidGuestPassenger_TermsAcceptanceControlsContinue` | Saved valid single guest: Terms off/on/off disables/enables/disables Continue without submitting | Passed locally 29 September 2026 |

TC31/32 map to `NativeFlightSearchEntryViewModel+DiscountCode` and `NativeFlightDiscountCodeView`.
TC33 maps to results `dismissFareSelector` and its fare-sheet cabin state. TC34/35 map to
`FlightPassengerDetailsViewModel` validation and the hub's `canContinue` expression.
Synthetic codes exercise local panel state only; TC18 still owns real discount entitlement.
Release build, 210 offline/retry checks and scoped repository verification passed on 29 September.
The exact-TRX analyser found no failures (`TestResults/coverage-gaps-phase2-20260929/offline.trx`).
TC31-TC35 subsequently all passed locally on iPhone 17 Pro/iOS 26.5 in one 5m49s run.
TC31: 22s; TC32: 24s; TC33: 57s; TC34: 1m02s; TC35: 1m33s.
Evidence: `TestResults/gap-scenarios-local-20260929/local.trx`; exact-TRX analysis found no failures.
No payment was attempted. This is one local qualification, not BrowserStack or long-term flake evidence.

## Latest regression and targeted repair evidence — 28 September 2026

Diagnostics infrastructure update: local iOS failures now collect bounded, sanitised app-log
metadata before teardown. The offline/retry pack passed 196 tests; a non-booking local syslog
probe confirmed app-process entries are available. No functional scenario was added or rerun
for this change, and no backend payment cause has yet been captured. See README for limits,
configuration and output paths.

Latest loyalty requalification: after updating the gitignored password, both active loyalty
scenarios passed login. TC02 passed its insufficient-points journey in 1m57s. TC03 reached
payment handoff and clicked a points option, but timed out verifying the selected value; logout
cleanup also could not find Account. Evidence: `TestResults/loyalty-password-recheck-20260928/loyalty.trx`.

TC01 is now enabled and renamed to explicitly cover high-balance Economy redemption. The
local run verified login, a Virgin Economy reward fare costing **30,000 points** against a
**150,000-point live balance**, retained Summary points, enabled Continue and member passenger
completion. HPP payment was submitted once, then the app displayed **Try secure payment again**.
The test failed in 3m15s; confirmation and Trips were not reached, payment was not retried and
no backend cause or points deduction is inferred. Logout cleanup could not reach Account.
Evidence: `TestResults/redemption-live-20260928/redemption.trx`; analyser reports HPP/UI failure.
All **185 offline/retry checks** and scoped harness verification passed. The current aggregate
evidence is 28 passing scenarios, 3 outstanding (guest TC01, Loyalty TC01/TC03), and only TC30
deferred. This combines targeted runs, not a new full-pack execution.

The full local iOS Search and Book regression selected 32 tests: **21 passed, 9 failed,
2 skipped** in 40.04 minutes (`TestResults/search-book-regression-local-20260928/regression.trx`).
TC01 required a manual WebDriverAgent restart after the driver hung during passenger Terms
scrolling; its resulting connection-refused error is not a backend diagnosis. TC03/04/28
failed cross-month date setup, TC05 could not expose its maximum-date boundary, TC17 used an
invalid native selector, TC18 lacked applied-promotion text, and Loyalty TC02/03 were rejected
at login. The two skips were the existing TC30 and Loyalty TC01 drafts. This run does not prove
HPP, confirmation or Trips completion; historical readiness labels are not a current full-pack pass.

Calendar repair: direction now follows visible ISO day IDs when SwiftUI recycles the target,
and safe bounds come from the calendar scroll viewport and footer, not a fixed screen ratio.
Six offline cases cover forward/backward navigation, hidden/occluded dates and missing evidence;
the offline/retry pack passed **164 cases**. TC05 and TC28 both passed locally with unchanged
business assertions (`TestResults/calendar-fix-live-20260928/calendar.trx`). That diagnostic filter
also unintentionally selected Android TC05, which failed its platform guard; it is not an iOS
calendar failure. Subsequent filters use the exact `.SearchBook_TCxx_` method prefix.

Recent airports repair: the iOS adapter now reads static-text descendants with a native
class-chain selector scoped to the identified field, avoiding Selenium's CSS conversion.
TC17 passed locally in 59 seconds (`TestResults/calendar-recents-fixed-20260928/targeted.trx`).
The same exact-filter run passed TC04 through mixed-carrier return Flight Summary in 2m02s.
TC03 shares the repaired calendar helper but has not yet been rerun through checkout; keep its
full E2E outcome unverified. Passenger Terms/WDA hang, promotion evidence and loyalty login
rejection remain outstanding. No scenario contract, selector ID or expected business result changed.

Subsequent passenger investigation: Terms/Continue now use bounded page-owned container scrolling.
The offline/retry checks passed 166 cases and scoped harness verification passed (including the
previous passenger-test line-ending issue). TC01 with five travellers still hung while WDA awaited
an accessibility snapshot at the hub; app sampling showed SwiftUI layout activity. A manual WDA
termination ended that diagnostic attempt, so its final timeout is not a product/API verdict.
TC03 with three travellers subsequently **passed its full return-booking E2E in 4m50s**, including
calendar selection, passenger completion, Terms, HPP payment, confirmation and Trips
(`TestResults/passenger-hub-return-20260928/return.trx`). This supersedes its previous calendar-only
failure. TC01 is still unresolved; no five-passenger pass is claimed.

Promo repair: TC18's fare reader used the obsolete selector-wide promotion ID. The app exposes
an indexed badge for each fare. Automation now discovers the exact Classic fare title, reads
its own price/badge, condition-waits required promo text and rejects ambiguous evidence. Seven
offline cases cover exact titles, non-contiguous indexes, duplicate IDs, absent/other-fare badges
and conflicts. All **173 offline/retry checks** and scoped harness verification passed.
TC18 then **passed locally in 1m49s**, proving HEMAN on the result and Classic fare, matching
itinerary evidence and a reduction from **£545.46 to £540.46**
(`TestResults/promo-hardening-live-20260928/promo.trx`). This was an automation selector mismatch,
not evidence of a failed promotion. No iOS change or weakened discount assertion was needed.
A second fresh-session run also passed in **1m50s** with the same £5 reduction
(`TestResults/promo-hardening-repeat-20260928/promo.trx`). Both current TRX analyses reported
no failures. This is two-run local qualification, not BrowserStack or long-term flake qualification.
The baseline full-pack result above remains historical; the whole pack has not been rerun.

The component-to-test ownership, overlap review and lean E2E recommendations are maintained in
[`E2E-COVERAGE-MATRIX.md`](E2E-COVERAGE-MATRIX.md).

The current code-to-scenario mapping was last rechecked on 22 September 2026 against local iOS
`develop` commit `a5151181` (`update flight result designs`).

This is the living inventory for mobile automation in `EAPI.Automation.sln`. Update it in the
same pull request whenever a test is added, renamed, removed, skipped, made ready, or materially
changes its assertion scope.

The machine-readable source for Search and Book and Account scenario mapping is
[`Contracts/search-book-automation-contract.json`](Contracts/search-book-automation-contract.json).
The contract tests enforce its one-to-one mapping to categorised xUnit methods. This catalogue
adds the human-readable readiness, execution and coverage context needed by QAs and coding agents.

## Status definitions

| Status | Meaning |
| --- | --- |
| Ready | Runnable against its configured environment and included when its category is selected. |
| On hold | Implemented, but deliberately not treated as runnable until its recorded app contract is delivered. |
| Draft | Implemented but explicitly skipped until its recorded dependency is ready. |
| Not ready | A specification or trial implementation blocked by an app or environment contract. |
| Offline | Runs without Appium, a simulator, a device or BrowserStack. |

## Current summary

### Shared Search panel additions from PR 2194

These six additional scenarios remain outside regression; TC17 below is reused, not duplicated.
Platform traits describe intended adapters, not blanket Android E2E support. See
[`MOBILE_ANDROID_SEARCH_PANEL.md`](../docs/MOBILE_ANDROID_SEARCH_PANEL.md) for execution evidence,
the platform boundary and the pending iOS qualification gate.

| Method | Platforms | Selection |
| --- | --- | --- |
| `SearchPanel_TC01_SearchPanel_ValidCriteriaEnableSearch` | iOS / Android | SearchPanel-Readiness |
| `SearchPanel_TC02_JourneyType_SwitchingToReturnRequiresReturnDate` | iOS / Android | SearchPanel-Pending; explicitly skipped pending iOS revalidation |
| `SearchBook_TC36_TravelDates_EditUpdatesSummaryAndCancelPreservesCommittedDates` | iOS / Android | SearchPanel-Readiness |
| `SearchBook_TC37_PassengerCounts_ConfirmPersistsAndCancelDiscardsDraft` | iOS / Android | SearchPanel-Readiness |
| `AndroidSearchBook_TC05_DiscountCode_ValidatesAppliesAndClearsPanelEntry` | Android | SearchBook-AndroidReadiness |
| `AndroidSearchBook_TC06_SelectionSheets_DismissalPreservesRoute` | Android | SearchBook-AndroidReadiness |

Category names above are prefixed with `MobileApp-`. The visual pack from main is documented
separately in [mobile visual validation](../docs/MOBILE_VISUAL_VALIDATION.md).

### Original functional coverage inventory

The counts below describe PR 2177's original functional scope, not the merged project's total:
main also contributes the readiness and visual packs and additional offline cases.

| Project and area | Methods | Current position |
| --- | ---: | --- |
| `MobileApp.UI` Search and Book | 30 | 28 Ready, 1 On hold and 1 Draft; 4 Smoke E2E, 1 Smoke and 25 Functional Validation |
| `MobileApp.UI` Loyalty and Account | 4 | 3 Ready and 1 Draft |
| `MobileApp.UI` connectivity | 1 | Ready |
| `MobileApp.UI` harness and diagnostics | 21 | Offline; 42 parameter-expanded cases |
| `MobileApp.OCI.UI` Online Check-In | 1 | Not ready |
| Legacy `MobileApp` API/integration project | 7 | Non-Appium mobile backend/integration tests |

Before main integration, `MobileApp.UI` contained 56 test methods and 77 discovered cases. Of its 35 app-facing
methods, 32 were runnable against the then-current app build, TC29 was On hold pending a stable live-path
Flight Summary price-sheet and baggage accessibility contract, and 2 explicitly documented Draft
methods are skipped. The separate OCI method is also skipped.

## Full regression pack

Review-summary follow-up: three additional offline methods (18 cases) reject reversed results
routes, require scoped carrier identifiers rather than Virgin marketing text, and verify filter
selection from either the native `selected` or `value` attributes. No live scenarios were added.

Passenger compatibility review: `PassengerFlowCompatibilityTests` adds two offline methods
(eight cases) protecting contact-entry order on legacy/redesigned forms and layout-based
traveller-card selection. These checks do not replace a live multi-passenger checkout rerun.

Review hardening (28 September 2026): `FlightSummaryEvidenceTests` contains fourteen offline methods
(49 theory cases plus one fact, 50 cases total) covering captured SwiftUI carrier-ID propagation,
legacy/redesigned summary routes,
filter reset selection, ordered routes, missing/wrong
fare evidence, carrier-row extraction and the TC29 skip/readiness gate. These do not add live
regression journeys. The later TC29 update replaces the skip gate with a runnable
`Readiness=Ready` gate and adds 19 offline baggage-assertion/reader cases (see evidence below).
Summary route/name readers retain documented English structural dependencies; a live iOS rerun
is still required to qualify the tightened assertions against the installed app.

Every business-facing `SmokeE2E` and `FunctionalValidation` test carries
`Suite=Regression`. Connectivity and offline framework checks are deliberately excluded. The
pack currently contains 34 scenarios: 30 runnable, 1 implemented pending its target app build,
2 Draft and 1 Not ready. On hold, Draft and Not ready members remain explicitly skipped, so they are visible as
regression intent without being reported as executed coverage.

Run the full regression selection from `VAA.EAPI.Automation` with:

~~~pwsh
dotnet test VAA.EAPI.Automation.sln -c Release --filter "Suite=Regression"
~~~

Run only the primary Search and Book and Account regression project with:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Suite=Regression"
~~~

Every regression scenario also declares its owning `RegressionArea`:

| Regression area | Included scenarios | Filter |
| --- | --- | --- |
| Search and Book | Search TC01-TC05 and TC07-TC35 plus logged-in Loyalty TC01-TC03 | `Suite=Regression&RegressionArea=SearchAndBook` |
| My Account | My Account Personal Details TC01 | `Suite=Regression&RegressionArea=MyAccount` |
| Online Check-In | OCI TC01 | `Suite=Regression&RegressionArea=OnlineCheckIn` |

This separates business ownership without removing any scenario from the full regression pack.

## Search and Book

All scenarios target iOS. Each independently runnable scenario has a unique ID and normally owns
its own Appium session, allowing BrowserStack execution across separate xUnit classes.

TC24-TC26 deliberately stop at Flight Summary today. Their page-object assertions are reusable
journey stages: as downstream app areas become complete and stable, full E2E scenarios should call
the same summary validation before continuing through passengers, payment, confirmation and Trips.

| ID | Test method | Type | Coverage | Status |
| --- | --- | --- | --- | --- |
| TC01 | `SearchBook_TC01_MixedPassengerOneWay_SelectsVirginFlight` | Smoke E2E | Guest, five mixed passengers, Virgin one-way, payment, confirmation and Trips | Ready |
| TC02 | `SearchBook_TC02_MixedPassengerOneWay_SelectsNonVirginFlight` | Smoke E2E | Guest, five mixed passengers and partner one-way retained through Flight Summary | Ready |
| TC03 | `SearchBook_TC03_TwoAdultsAndChildReturn_SelectsVirginOutboundAndInbound` | Smoke E2E | Guest return, Virgin both ways, payment, confirmation and Trips | Ready |
| TC04 | `SearchBook_TC04_TwoAdultsAndChildReturn_SelectsVirginOutboundAndPartnerInbound` | Smoke E2E | Guest return, Virgin outbound and partner inbound retained through combined Flight Summary | Ready |
| TC05 | `SearchBook_TC05_SearchEntry_EnforcesCalendarAndPassengerRules` | Functional Validation | Required search data, calendar boundaries, return range and passenger limits | Ready; non-parallel collection |
| TC06 | `SearchBook_TC06_ResultsSortAndFilter_PersistUntilFiltersAreCleared` | Smoke | Short pre-merge refinement check; excluded from full regression | Ready |
| TC07 | `SearchBook_TC07_PriceSort_OrdersDisplayedResultsAscending` | Functional Validation | Price ordering across displayed results | Ready |
| TC08 | `SearchBook_TC08_DepartureTimeSort_OrdersResultsAndRecommendedRestoresBaseline` | Functional Validation | Departure ordering and Recommended reset | Ready |
| TC09 | `SearchBook_TC09_DurationSort_OrdersResultsAndRecommendedRestoresBaseline` | Functional Validation | Duration ordering and Recommended reset | Ready |
| TC10 | `SearchBook_TC10_SortSwitching_PreservesSelectionAndRecommendedRestoresBaseline` | Functional Validation | Switching preserves each selected sort and Recommended restores baseline; TC07-TC09 own ordering | Ready |
| TC11 | `SearchBook_TC11_AvailableAirlineFilters_EveryResultContainsSelectedOperatedLeg` | Functional Validation | Dynamically discovered airline filters and operated-leg evidence | Ready |
| TC12 | `SearchBook_TC12_ConnectionsFilters_EveryResultMatchesSelectedCount` | Functional Validation | Dynamically available connection filters using visible card summaries | Ready |
| TC13 | `SearchBook_TC13_DepartureTimeFilters_EveryResultFallsInsideSelectedWindow` | Functional Validation | Dynamically available departure windows | Ready |
| TC14 | `SearchBook_TC14_FilterSwitching_ReplacesDiscoveredAirlineAndValidatesResults` | Functional Validation | Replacing one discovered airline filter with another | Ready |
| TC15 | `SearchBook_TC15_PriceSortWithDiscoveredAirlineFilter_PreservesOrderingAndMatchesCarrier` | Functional Validation | Combined price sort and airline filter | Ready |
| TC16 | `SearchBook_TC16_ClearFilters_RemovesDiscoveredAirlineAndPreservesPriceSort` | Functional Validation | Clear filter while preserving selected sort | Ready |
| TC17 | `SearchBook_TC17_RecentAirportSearches_RetainFiveMostRecentPerDirection` | Functional Validation | Five recent origins/destinations, newest first, within one session | Ready |
| TC18 | `SearchBook_TC18_HemanPromoCode_DiscountsManchesterNewYorkReturnEconomyClassicByFivePounds` | Functional Validation | HEMAN, May MAN-JFK return, matched itinerary and £5 discount; Economy Light excluded | Ready; controlled promotion dependency |
| TC19 | `SearchBook_TC19_FilterSelection_RecalculatesAvailabilityAcrossOtherGroups` | Functional Validation | Cascading option availability and matching results | Ready |
| TC20 | `SearchBook_TC20_VirginOperatingCarrierFlightCards_UseBroadCabinGroupNames` | Functional Validation | Virgin operating-carrier broad cabin naming | Ready |
| TC21 | `SearchBook_TC21_OtherOperatingCarrierFlightCards_UseBackendBrandCabinNames` | Functional Validation | Other operating carriers retain backend Brand/fare naming | Ready |
| TC22 | `SearchBook_TC22_VirginFlightCabin_UpgradeViewFaresOpensNextAvailableCabin` | Functional Validation | Virgin upgrade card and next available cabin | Ready |
| TC23 | `SearchBook_TC23_OneAdultGuestFromJfkToLhr_CompletesUsdBookingAndAppearsInTrips` | Smoke E2E | Guest JFK–LHR: USD results/Summary/confirmation, payment and exact reference in Trips | Expanded checkout awaiting live qualification |
| TC24 | `SearchBook_TC24_VirginOperatingCarrier_FlightSummaryMatchesSelectedFlight` | Functional Validation | Summary retains Virgin route, operating carrier, fare, date/times, total price and Continue state | Ready |
| TC25 | `SearchBook_TC25_OtherOperatingCarrier_FlightSummaryMatchesSelectedFlight` | Functional Validation | Summary retains partner route, operating carrier, backend fare, date/times, total price and Continue state | Ready |
| TC26 | `SearchBook_TC26_MultiCarrierConnectingFlight_FlightSummaryRetainsStopsAndAdditionalCarrier` | Functional Validation | Non-direct `+1` result retains stops and all ordered leg airports/dates/times/operators in Summary details; verifies connection continuity | Extended scenario passed locally 30 September against rebuilt app in 2m17s; supersedes missing-ID failure. Android/BrowserStack not qualified |
| TC27 | `SearchBook_TC27_FlexibleDateSelection_RefreshesResultsAndPreservesRoute` | Functional Validation | Selects a genuinely in-viewport alternative date, verifies that exact date remains selected after refresh, and preserves route context | Ready; passed locally on 22 September 2026 |
| TC28 | `SearchBook_TC28_EditSubmittedSearch_UpdatesCriteriaAndResultsRoute` | Functional Validation | Edit from inbound results changes the return criteria and verifies the amended route and departure date in results | Ready; passed locally on 22 September 2026 |
| TC29 | `SearchBook_TC29_SelectedFareBenefitsAndBaggage_PropagateToFlightSummary` | Functional Validation | Selected fare title retained; baggage comparison counts distinguish exercised inclusion/count/weight/dimensions | Passed locally 30 September (58s): inclusion=2, weight=1, quantity=0, dimensions=0. Summary dimensions visible, but expected fare dimensions absent; dimension accuracy remains unqualified |
| TC30 | `SearchBook_TC30_TransientSearchFailure_RetryRecoversToResults` | Functional Validation | Controlled fail-once search recovers through retry | Draft; skipped pending deterministic iOS test hook |

## Loyalty booking and My Account

These scenarios select and lease suitable accounts from the existing EAPI Azure Storage
capabilities before using the app. A skipped scenario remains documented coverage intent and must
not be reported as active regression coverage.

| ID | Test method | Type | Coverage | Status |
| --- | --- | --- | --- | --- |
| Loyalty TC01 | `LoyaltySearchBook_TC01_HighBalanceEconomyRedemption_CompletesThroughHppConfirmationAndTrips` | Smoke E2E | High-balance SIT member, affordable Virgin Economy reward fare, retained Summary points, HPP cash component, confirmation and Trips | Enabled; local run reached HPP submission then explicit payment failure; confirmation/Trips unverified |
| Loyalty TC02 | `LoyaltySearchBook_TC02_LowBalanceReturnUpperClass_BlocksContinuationForInsufficientPoints` | Functional Validation | Low-balance Red account, return Upper reward and insufficient-points stop | Ready |
| Loyalty TC03 | `LoyaltySearchBook_TC03_CurrencyBooking_SelectsDisplayedPayWithPointsOption` | Functional Validation | Currency search, prepopulated member passenger and points-option selection | Ready; stops before applying points |
| My Account TC01 | `LoyaltyAccount_TC01_StoredMemberFirstName_MatchesPersonalDetailsAndExposesAccountActions` | Functional Validation | My Account Personal Details: stored first name, Edit and Delete actions | Ready; surname blocked by app accessibility contract |

## Connectivity and Online Check-In

| ID | Project | Test method | Coverage | Status |
| --- | --- | --- | --- | --- |
| Connectivity | `MobileApp.UI` | `MobileApp_Connectivity_AppiumSessionStartsSuccessfully` | Driver creation and expected app landing state | Ready; deliberately serialized |
| OCI TC01 | `MobileApp.OCI.UI` | `OnlineCheckIn_TC01_SingleAdultPriorityRoute_GeneratesBoardingPass` | Storage-backed candidate preparation, trip addition, check-in and boarding pass | Not ready; skipped pending stable Trips/OCI IDs |

## Offline harness and diagnostic tests

| Test group | Methods / cases | Purpose |
| --- | ---: | --- |
| `MobileAutomationContractTests` | 10 methods / 22 cases | Selector ownership, platform declaration, parsing, unique IDs, category mapping, self-healing configuration and parallel class isolation |
| `BrowserStackNetworkLogSanitiserTests` | 3 / 3 | Safe failure-only GraphQL/network summaries |
| `BrowserStackSessionRetryPolicyTests` | 2 / 9 | Retry transient BrowserStack failures but reject configuration and product failures |
| `IosProcessArgumentsTests` | 4 / 4 | Feature-flag normalization, deduplication and unsafe-value rejection |
| `MobileHealingPolicyTests` | 3 / 5 | Bounded recovery telemetry, failure preservation and retry guardrails |
| `FlightSummaryEvidenceTests` | 14 / 50 | Legacy/redesigned routes, observed fare/carrier evidence, filter state and TC29 gate |
| `FareBaggageAssertionsTests` | 6 / 19 | Retained fare, positive/excluded baggage, advertised count/weight, omitted exclusions and exact indexed hierarchy evidence |
| `NativeUiFailureSnapshotTests` | 2 / 2 | Bounded pre-teardown evidence, repeated IDs and exclusion of private text |
| `PassengerFlowCompatibilityTests` | 6 / 17 | Legacy/redesigned contact-entry order, indexed IDs, exact selection evidence, scoped row reading and picker tap-bound guard |
| `AndroidSearchContractTests` | 2 / 2 | Android selector and readiness contracts |
| `SharedSearchPanelTests` | 10 / 25 | Shared panel configuration and adapter behaviour |

The `Category=MobileApp-Offline` selection contains 130 cases in this revision. The nine
`BrowserStackSessionRetryPolicyTests` cases are offline but do not yet carry that category and
should be aligned in a follow-up change.

## Latest focused verification

28 September 2026 rebuilt iOS qualification: temporary one-adult TC01 passed passenger
details (including exact gender retention, DOB, contact, save and Terms) and HPP interaction.
It failed on the app's `Try secure payment again` state; confirmation/Trips remain unverified.
139 offline/retry checks passed. Mixed-passenger TC01 was restored and was not qualified by
this run. Evidence: `TestResults/rebuilt-ios-20260928/one-adult/qualification.trx`.

On 22 September 2026, TC27 and TC28 passed locally against the configured iPhone 17 Pro simulator.
TC27 now rejects the iOS carousel's misleading `visible=true` value for horizontally off-screen
dates, chooses a date whose bounds are within the viewport, and verifies the exact selected date
using the native `value=1` accessibility state. Its one optional native-tap recovery is bounded and
recorded by the healing harness. TC28 verifies both the amended MAN-JFK route and amended departure
date after resubmission.

On 28 September 2026, the old TC29 reached `NativeFlightSummaryPriceSheet` and failed waiting for
`NativeFlightSummaryFareConditionsLink` (1 failed; `TestResults/tc29-inspection-20260928/tc29.trx`).
This supersedes the earlier missing-price-sheet diagnosis. TC29 now uses Summary → Flight details,
checks the retained fare title and compares baggage inclusion plus advertised count/weight. It is
runnable and `Ready`: the rebuilt-app rerun passed in 56 seconds
(`TestResults/tc29-fixed-live-20260928/tc29.trx`). The investigation corrected an automation expectation:
Economy Light excludes checked baggage, so Summary legitimately returns only carry-on baggage.
An explicitly excluded category may be absent or explicitly excluded, but must never be included.
Included benefits still require matching count/weight evidence. Nineteen offline assertion/reader
cases bring the selected mobile/retry-policy checks to 158 passing cases. Dimensions, full fare rules and all-carrier
coverage remain outside this focused scenario.

On 17 September 2026, TC24-TC26 were compiled and launched locally against the configured iPhone
17 Pro simulator. The authentication-expiry startup alert discovered by TC24 is now handled by the
shared entry flow. The app then returned its explicit `We could not start this search just now`
state before results, matching the known downstream search issue; therefore no Flight Summary
assertion executed and no pass/fail conclusion is recorded for those checks. The updated offline
contract and inventory suite passed all 27 cases.

## Legacy mobile backend/integration tests

The `VAA.EAPI.Automation.MobileApp` project is not the Appium UI suite. It currently contains:

- airport-data search and CJS comparison tests;
- mobile GraphQL schema validation;
- Service Bus app-notification integration;
- penetration-feedback injection and operation-overload checks.

Keep these separate from device/UI coverage when reporting mobile regression results.

## Execution model

- BrowserStack: separate concrete xUnit classes may run in parallel, capped at four sessions.
- Local iOS: session creation is serialized because one simulator/WebDriverAgent is shared.
- Connectivity is in the non-parallel `Mobile` collection by design.
- TC05 is also currently in that collection and should be reviewed because it reduces remote
  concurrency despite owning no obvious cross-device state.
- Tests use stable accessibility IDs first. Temporary fallbacks require a documented removal
  condition tied to an app-readiness change.

## Maintenance contract

When adding or changing a test:

1. Allocate the next unique sequential ID within the owning area.
2. Use `<Area>_TC##_Condition_ExpectedBehaviour` naming.
3. Add or update its entry in the machine-readable contract when it is a Search and Book or
   Account scenario.
4. Add or update the row in this catalogue, including truthful readiness and partial coverage.
5. Give the test Category, TestType, Feature/Journey where relevant, Platform and Readiness traits.
   Add `Suite=Regression` and the owning `RegressionArea` to every business-facing Smoke E2E or
   Functional Validation test.
6. Put an independently runnable remote scenario in its own concrete class.
7. Record controlled data, environment, app-selector and downstream dependencies.
8. Run the offline contract suite before raising the pull request.

Do not count skipped, draft, not-ready or deliberately partial scenarios as completed end-to-end
coverage. Update this catalogue when a blocker is removed, not only when the test code changes.
## Seat-only qualification additions — 7 October 2026

Newest TC47 profiling attempt failed at app seat-map error before discovery (1m42s):
`TestResults/seats-tc47-profile-local-20261007/local.trx`. New automation timing/read consolidation
passed 364 offline checks; no speed comparison or live qualification yet. Earlier pass below retained.

**Latest TC47: local PASS, 6m02s**, superseding the historical blockers below.
One safe-area drag; 46D GBP 40 -> 50A GBP 35 -> removal GBP 0; trip totals
GBP 964.20 -> GBP 959.20 -> baseline GBP 924.20. Evidence:
`TestResults/seats-tc47-safe-area-local-20261007/local.trx`. No payment; Qualification only,
BrowserStack and repeated-run stability pending. No new iOS change was needed for this fix.

Current shared-helper version is pending qualification: unobscured/hittable seat guards and bounded
viewport scrolling require the outstanding iOS `seatSelection.viewport` contract. Historical
TC43–TC47 evidence below predates that hardening. Rebuilt TC47 passed its first GBP 40 price check
but then reached Payment unexpectedly; see `SEATS-COVERAGE.md` for evidence and remaining work.

TC47: `SearchBook_TC47_PaidSeatReplacementAndRemoval_RecalculateChargeAndTripTotal`.
Implemented; local attempt blocked at the first price ID (2m44s, 7 October); independent iOS qualification class. Uses one guest adult,
LHR-JFK Economy Classic and two visible offered paid seats with distinct GBP prices. Compares map
offer/detail price, cart charge and total after selection/replacement/removal. Requires PR1273's
three price IDs. No payment or authoritative backend seat fulfilment assertion.
Installed app predates PR1273 commit `c12c922d`; rebuild/install before retrying. No pricing
assertions completed. Evidence: `TestResults/seats-tc47-local-20261007/local.trx`.

| Test | Scope | Status |
| --- | --- | --- |
| `SearchBook_TC44_OneAdultSeatSelection_ReplacementAndRemovalUpdateAssignment` | Select, replace, remove; exact assignment to the sole traveller | Local pass, 7 October, 1m57s |
| `SearchBook_TC45_SeatDetailsAndKey_DismissalPreservesPassengerAssignment` | Details/key cancellation before and after assignment | Local pass, 7 October, 2m15s |
| `SearchBook_TC46_TwoAdultSeatSelection_RetainsIndependentAssignments` | Auto-advance, duplicate-seat prevention, passenger switching and independent removal | Local rerun passed, 7 October, 2m37s |

TC46 uses two synthetic guest adults in its own qualification class. It asserts the active
passenger's native selected state before manually switching, and verifies the other traveller's
seat is disabled rather than attempting an invalid selection. Assignment reads use exact owned
IDs even when auto-advance moves a card partly offscreen; clicks still require visible enabled controls.
No payment, prices, return-sector or post-booking coverage is claimed.
First local run: `TestResults/seats-tc46-local-20261007/local.trx`, failed in 2m09s.
Search and both passenger forms completed; the seat screen exposed `seatSelection.error` and
no passenger cards. No automatic retry was made and no safe app-log metadata explained the cause.
The initial wait now detects that app error explicitly rather than timing out on a missing card.
The user-authorised rerun passed all assertions in 2m37s without further code changes:
`TestResults/seats-tc46-local-rerun-20261007/local.trx`. Exact-TRX analysis found no failures.
The app error did not recur, so its fail-fast branch was not exercised on this successful run.
BrowserStack and repeated-run stability remain unqualified.

All three have separate classes, `Suite=Qualification`, `Journey=Seats`, iOS only. None submits
seats nor enters payment. Require two visible enabled offers, Economy Classic and seat-skip off.
These add three scenarios, not regression passes. TC43 remains the full booking qualification test;
its 7 October reruns passed seat selection/submission but failed payment preparation.

Final evidence: `TestResults/seat-focused-final-20261007/final.trx`: both live tests and 349 offline
checks passed. Ten architecture checks also passed. Seat discovery/detail opening now takes about
4.6s rather than 43-44s. BrowserStack is not qualified; this is not a full seat regression pack.
## Lap-infant qualification — 8 October 2026

`SearchBook_TC49_AdultWithLapInfant_OnlyAdultHasIndependentSeatAssignment`
(`LapInfantSeatAssignmentTests`, iOS, Seats, FunctionalValidation, Qualification):
one adult plus one lap infant; assert complete assignable count equals one, select,
require the adult passenger card to identify the linked `+ infant`, replace and remove
an offered adult seat, and retain active passenger zero. The text assertion requires
iOS #1311 and has not been live-qualified. Stops
before seat submission/payment. Requires PR1273 count accessibility value;
Passed locally on iPhone 17 Pro / iOS 26.5 on 8 October 2026: 4m08s test,
4m27s including session lifecycle. Exact-run analysis found no failures in
`TestResults/lap-infant-20261008/local.trx`; 375 offline checks passed.
The historical pass predates the linked-infant assertion. BrowserStack and infant-restriction
coverage remain open.
## Unsupported operating-carrier qualification

`SearchBook_TC50_UnsupportedOperatingCarriers_BypassSeatsWhileVirginRemainsEligible`
(`UnsupportedCarrierSeatBypassTests`, iOS, Seats, FunctionalValidation, Qualification)
requires every observed leg to identify AF/KL and selects Economy Standard, then
asserts direct payment entry without any seat Continue/Skip. Partner fare selection
uses Economy Standard (not Virgin's Classic name). It returns to Search
and requires a Virgin Classic journey to open seats in the same session, excluding
a global skip override as the explanation. Neither journey submits seats/payment.
Unknown operators, missing required fare inventory and missing positive control fail;
no substitution or silent skip. Local qualification blocked on 8 October: after
switching to Economy Standard for KLM LHR-AMS-JFK, fare submission displayed
"We could not start this search just now. Please try again." Screenshot confirms
Standard on both legs. Neither bypass nor the Virgin control was reached.
Evidence: `TestResults/seat-bypass-20261008/local-standard.trx` (2m25s test).
The initial Classic-name assumption was corrected; 391 offline checks pass.
Backend cause is unconfirmed; captured app-log error signals have no correlated response.
BrowserStack remains unqualified. Wider harness: context-routing/operational-harness
failures remain; formatting passed. No payment, no iOS changes.

The repeat run reproduced the same pre-Summary failure:
`TestResults/seat-bypass-20261008-rerun/local.trx` (2m34s test).
Tracked as [iOS KLM fare-selection bug #179](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/179).
TC50 remains qualification-only; neither seat bypass nor its positive control has passed.

## PR preparation verification — 8 October 2026

Review update after main sync: 394 offline/retry checks and scoped repository verification
passed. TC41/TC42 are included in Regression with an additional BackendRegression subset trait; TC43 shares the
seat-enabled base. No live rerun. Evidence: `TestResults/mobile-review-20261008/offline.trx`.
The earlier pre-merge harness metadata failures below are historical.

Release build and 391 mobile offline checks passed; 10 architecture checks passed.
Both exact-run TRX files were analysed with no failed tests:
`TestResults/mobile-pr-prep-20261008/mobile-offline.trx` and
`TestResults/mobile-pr-prep-20261008/architecture.trx`.
This is offline validation, not a fresh simulator or BrowserStack regression run.
Legacy NU1701 dependency warnings remain. Repository-wide context-routing and
operational-registry checks flag review metadata older than 30 days; these are not
waived or made green by the mobile test passes.
# Card-presentation qualification — 20 October 2026

`SearchBook_TC55_ShortNoticeLosCardPresentation_AgreementCompletesBookingAndTrips`
is an iOS `Suite=Regression`, `RegressionArea=SearchAndBook` test, not part of Sanity.
One guest adult, LHR–LOS one-way in three days, Virgin Economy Classic.
Uses real test HPP, requires airport-check instructions before consent, then confirmation
and Trips. No fault injection, payment retry or cancellation assertion.
Local 20 October: first attempt stopped before payment on unsupported LOS Summary parsing;
LOS mapping now offline-tested. Second attempt reached the real post-HPP verification page
in 2m24s, then failed reading only the first of four same-ID instruction paragraphs.
Reader corrected to aggregate visible matches; heading/introduction and both action IDs
were present. Agreement was not submitted; confirmation/Trips remain unqualified.
Evidence: `TestResults/tc55-los-local-20261020-b/tc55.trx`.
Subsequent corrected run **passed** through agreement, confirmation and Trips:
`TestResults/tc55-los-local-20261020-c/tc55.trx` (4m08.940s body, approximately 4m48s
including lifecycle). The analyser found no failures; 553 offline checks passed.
This supersedes the unqualified agreement status above. Cancellation, eligibility boundary
cases and BrowserStack qualification remain outside this passing evidence.
